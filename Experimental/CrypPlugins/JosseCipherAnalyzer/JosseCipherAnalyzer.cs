/*
   Copyright CrypTool 2 Team <ct2contact@cryptool.org>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using CrypTool.JosseCipherAnalyzer.AttackTypes;
using CrypTool.JosseCipherAnalyzer.Enum;
using CrypTool.JosseCipherAnalyzer.Evaluator;
using CrypTool.JosseCipherAnalyzer.Properties;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using LanguageStatisticsLib;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.JosseCipherAnalyzer
{
    [Author("Niklas Weimann", "niklas.weimann@student.uni-siegen.de", "CrypTool 2 Team", "https://www.cryptool.org")]
    [PluginInfo("CrypTool.JosseCipherAnalyzer.Properties.Resources", "PluginName", "PluginTooltip",
        "JosseCipherAnalyzer/userdoc.xml", new[] { "JosseCipherAnalyzer/Images/icon.png" })]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class JosseCipherAnalyzer : ICrypComponent
    {
        #region Private Variables

        private readonly JosseCipherAnalyzerSettings _settings = new JosseCipherAnalyzerSettings();
        private readonly JosseCipherAnalyzerPresentation _presentation = new JosseCipherAnalyzerPresentation();
        public delegate void UpdateOutput(string keyString, string plaintextString);
        private string _plainTextOutput;
        private string _keyOutput;
        private DateTime _startTime;
        private DateTime _endTime;
        private AttackType _analyzer;
        private volatile bool _stopped;
        private const int MaxBestListEntries = 100;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "CipherTextInput", "CipherTextInputTooltip", true)]
        public string CipherTextInput { get; set; }

        [PropertyInfo(Direction.InputData, "AlphabetInput", "AlphabetInputTooltip")]
        public string AlphabetInput
        {
            get => _settings.Alphabet;
            set
            {
                if (value == null || value == _settings.Alphabet)
                {
                    return;
                }

                _settings.Alphabet = value;
                OnPropertyChanged(nameof(AlphabetInput));
            }
        }

        [PropertyInfo(Direction.OutputData, "PlainTextOutput", "PlainTextOutputTooltip")]
        public string PlainTextOutput
        {
            get => _plainTextOutput;
            set
            {
                _plainTextOutput = value;
                OnPropertyChanged(nameof(PlainTextOutput));
            }
        }

        [PropertyInfo(Direction.OutputData, "KeyOutput", "KeyOutputTooltip")]
        public string KeyOutput
        {
            get => _keyOutput;
            set
            {
                _keyOutput = value;
                OnPropertyChanged(nameof(KeyOutput));
            }
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings => _settings;


        public UserControl Presentation => _presentation;

        public JosseCipherAnalyzer()
        {
            _presentation.UpdateOutputFromUserChoice += UpdateOutputFromUserChoice;
        }

        private void UpdateOutputFromUserChoice(string keystring, string plaintextstring)
        {
            KeyOutput = keystring;
            PlainTextOutput = plaintextstring;
        }


        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            _stopped = false;
            ProgressChanged(0, 1);
            // Swap key length setting
            if (_settings.KeyLengthTo < _settings.KeyLengthFrom)
            {
                int temp = _settings.KeyLengthTo;
                _settings.KeyLengthTo = _settings.KeyLengthFrom;
                _settings.KeyLengthFrom = temp;
            }
            UpdateDisplayStart();

            string alphabet = AlphabetInput.ToUpper();
            if (string.IsNullOrEmpty(CipherTextInput) || !CipherTextInput.ToUpper().Any(c => alphabet.IndexOf(c) >= 0))
            {
                GuiLogMessage(Resources.NoCiphertextLetters, NotificationLevel.Error);
                return;
            }
            IEvaluator evaluator = GetEvaluator(alphabet);
            int keyLengths = _settings.KeyLengthTo - _settings.KeyLengthFrom + 1;
            ResultEntry best = null;

            for (int currentLength = _settings.KeyLengthFrom; currentLength <= _settings.KeyLengthTo && !_stopped; currentLength++)
            {
                int lengthIndex = currentLength - _settings.KeyLengthFrom;
                _analyzer = GetAnalyzer(alphabet, currentLength);
                _analyzer.Evaluator = evaluator;
                _analyzer.ProgressChanged += (sender, progress) => ProgressChanged(lengthIndex + progress, keyLengths);
                _analyzer.ResultFound += (sender, entry) => AddNewBestListEntry(entry);
                _analyzer.KeysPerSecondChanged += (sender, keysPerSecond) => UpdateKeysPerSecond(keysPerSecond);

                ResultEntry result = _analyzer.Start(CipherTextInput);
                UpdateDisplayEnd(currentLength);
                if (result == null)
                {
                    // stopped
                    break;
                }
                if (best == null || result.Value > best.Value)
                {
                    best = result;
                }
            }

            if (best != null)
            {
                KeyOutput = best.Key;
                PlainTextOutput = best.Text;
            }
            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Inserts a result into the best list, which stays sorted, free of duplicates and at most
        /// <see cref="MaxBestListEntries"/> long.
        /// </summary>
        private void AddNewBestListEntry(ResultEntry entry)
        {
            _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ObservableCollection<ResultEntry> bestList = _presentation.BestList;
                if (bestList.Count >= MaxBestListEntries && entry.Value <= bestList[bestList.Count - 1].Value)
                {
                    return;
                }
                if (bestList.Any(e => e.Key == entry.Key))
                {
                    return;
                }

                int insertIndex = bestList.TakeWhile(e => e.Value > entry.Value).Count();
                bestList.Insert(insertIndex, entry);
                if (bestList.Count > MaxBestListEntries)
                {
                    bestList.RemoveAt(MaxBestListEntries);
                }
                for (int i = insertIndex; i < bestList.Count; i++)
                {
                    bestList[i].Ranking = i + 1;
                }
            }, null);
        }

        private void UpdateKeysPerSecond(long keysPerSecond)
        {
            _presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _presentation.Keys.Value = $"{keysPerSecond:N0}";
            }, null);
        }

        /// <summary>
        /// Set start time in UI
        /// </summary>
        private void UpdateDisplayStart()
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _startTime = DateTime.Now;
                _presentation.StartTime.Value = "" + _startTime;
                _presentation.EndTime.Value = "";
                _presentation.ElapsedTime.Value = "";
                _presentation.BestList.Clear();
            }, null);
        }

        /// <summary>
        /// Set end time in UI
        /// </summary>
        private void UpdateDisplayEnd(int keylength)
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _endTime = DateTime.Now;
                TimeSpan elapsedtime = _endTime.Subtract(_startTime);
                TimeSpan elapsedspan = new TimeSpan(elapsedtime.Days, elapsedtime.Hours, elapsedtime.Minutes, elapsedtime.Seconds, 0);
                _presentation.EndTime.Value = "" + _endTime;
                _presentation.ElapsedTime.Value = "" + elapsedspan;
                _presentation.CurrentAnalysedKeylength.Value = "" + keylength;

            }, null);
        }

        private AttackType GetAnalyzer(string alphabet, int keyLength)
        {
            switch (_settings.AnalyzerMode)
            {
                case AnalyzerMode.Hillclimbing:
                    return new Hillclimbing(alphabet, keyLength, _settings.Restarts);
                case AnalyzerMode.SimulatedAnnealing:
                    return new SimulatedAnnealing(alphabet, keyLength, _settings.Restarts);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IEvaluator GetEvaluator(string alphabet)
        {
            switch (_settings.CostFunctionTypes)
            {
                case CostFunctionTypes.Unigrams:
                    return new NGram(GramsType.Unigrams, _settings.Language, _settings.UseSpaces, alphabet);
                case CostFunctionTypes.Bigrams:
                    return new NGram(GramsType.Bigrams, _settings.Language, _settings.UseSpaces, alphabet);
                case CostFunctionTypes.Trigrams:
                    return new NGram(GramsType.Trigrams, _settings.Language, _settings.UseSpaces, alphabet);
                case CostFunctionTypes.Tetragrams:
                    return new NGram(GramsType.Tetragrams, _settings.Language, _settings.UseSpaces, alphabet);
                case CostFunctionTypes.Pentragrams:
                    return new NGram(GramsType.Pentragrams, _settings.Language, _settings.UseSpaces, alphabet);
                case CostFunctionTypes.Hexagrams:
                    return new NGram(GramsType.Hexagrams, _settings.Language, _settings.UseSpaces, alphabet);
                case CostFunctionTypes.IoC:
                    return new IndexOfCoincidence(alphabet);
                case CostFunctionTypes.Entropy:
                    return new Entropy(alphabet);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            if (_analyzer != null)
            {
                _analyzer.Evaluator = null;
            }
            _startTime = new DateTime();
            _endTime = new DateTime();
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            _stopped = true;
            if (_analyzer != null)
            {
                _analyzer.ShouldStop = true;
            }
        }

        /// <summary>
        /// Called once when plugin is loaded into editor workspace.
        /// </summary>
        public void Initialize()
        {
        }

        /// <summary>
        /// Called once when plugin is removed from editor workspace.
        /// </summary>
        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}