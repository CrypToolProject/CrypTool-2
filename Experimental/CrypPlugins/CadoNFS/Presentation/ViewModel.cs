using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Numerics;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using CadoNFS.Processing;

namespace CadoNFS.Presentation
{
    public class ViewModel : INotifyPropertyChanged
    {
        private string estimatedEndTime;
        private double currentProcessingStepProgress;
        private ProcessingSteps currentProcessingStep;
        private DateTime startTime;
        private int? foundRelations;
        private int? neededRelations;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Log> Logs { get; } = new ObservableCollection<Log>();

        public ObservableCollection<BigInteger> Factors { get; } = new ObservableCollection<BigInteger>();

        public enum ProcessingSteps
        {
            Initializing,
            PolynomialSelection,
            LatticeSieving,
            Filtering,
            LinearAlgebra,
            QuadraticCharacters,
            SquareRoot,
            Completed
        }

        public ProcessingSteps CurrentProcessingStep
        {
            get => currentProcessingStep;
            set
            {
                currentProcessingStep = value;
                OnPropertyChanged(nameof(CurrentProcessingStep));
            }
        }

        public double CurrentProcessingStepProgress
        {
            get => currentProcessingStepProgress;
            set
            {
                currentProcessingStepProgress = value;
                OnPropertyChanged(nameof(CurrentProcessingStepProgress));
            }
        }

        public DateTime StartTime
        {
            get => startTime;
            set
            {
                startTime = value;
                OnPropertyChanged(nameof(StartTime));
            }
        }

        public string EstimatedEndTime
        {
            get => estimatedEndTime;
            set
            {
                estimatedEndTime = value;
                OnPropertyChanged(nameof(EstimatedEndTime));
            }
        }

        public int? FoundRelations
        {
            get => foundRelations;
            set
            {
                foundRelations = value;
                OnPropertyChanged(nameof(FoundRelations));
            }
        }

        public int? NeededRelations
        {
            get => neededRelations;
            set
            {
                neededRelations = value;
                OnPropertyChanged(nameof(NeededRelations));
            }
        }

        public void AddLoggingPresentation(string logName, LoggingInterception logging)
        {
            var log = new Log(logName);
            logging.LogEvent += (pid, time, level, name, message)
                => Dispatch(() => log.LogEntries.Add(new LogEntry { Time = time, Level = level, Name = name, Message = message }));

            Dispatch(() => Logs.Add(log));
        }

        public void ClearLoggingPresentations()
        {
            Dispatch(() => Logs.Clear());
        }

        internal void SetFactors(IEnumerable<BigInteger> factors)
        {
            Dispatch(() =>
            {
                Factors.Clear();
                foreach (var factor in factors)
                {
                    Factors.Add(factor);
                }
            });
        }

        private void Dispatch(Action action)
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate { action(); }, null);
        }

        private void OnPropertyChanged(string propertyName)
        {
            Dispatch(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
        }
    }

    public struct LogEntry
    {
        public DateTime Time { get; set; }
        public string Level { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
    }

    public class Log
    {
        public string LogName { get; }
        public ObservableCollection<LogEntry> LogEntries { get; } = new ObservableCollection<LogEntry>();

        public Log(string logName)
        {
            LogName = logName;
        }
    }
}
