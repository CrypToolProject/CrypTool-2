using CadoNFS.Presentation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using static CadoNFS.Presentation.ViewModel;

namespace CadoNFS.Processing
{
    public class ProcessStatusHandler
    {
        private readonly ViewModel viewModel;
        private bool httpServerRunning;

        public event Action HttpServerRunningEvent;

        public ProcessStatusHandler(LoggingInterception logging, ViewModel viewModel)
        {
            logging.LogEvent += HandleLogEvent;
            this.viewModel = viewModel;
            Initialize();
        }

        private void Initialize()
        {
            viewModel.StartTime = DateTime.Now;
            viewModel.EstimatedEndTime = null;
            viewModel.CurrentProcessingStep = ProcessingSteps.Initializing;
            viewModel.CurrentProcessingStepProgress = 0;
            viewModel.FoundRelations = null;
            viewModel.NeededRelations = null;
        }

        private void HandleLogEvent(int pid, DateTime time, string level, string name, string message)
        {
            HandleLogNameStatus(name);
            HandleWorkUnitStatus(message);
            HandleNumberRelationsStatus(name, message);
        }

        private void HandleNumberRelationsStatus(string name, string message)
        {
            if (name == "Lattice Sieving")
            {
                const string filter = "total is now (.*)/(.*)";
                var match = Regex.Match(message, filter);
                if (match.Success)
                {
                    viewModel.FoundRelations = int.Parse(match.Groups[1].Value);
                    viewModel.NeededRelations = int.Parse(match.Groups[2].Value);
                }
            }
        }

        private void HandleWorkUnitStatus(string message)
        {
            const string filter = "Marking workunit .* as ok \\((.*)% => ETA (.*)\\)";
            var match = Regex.Match(message, filter);
            if (match.Success)
            {
                var progressPercentage = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                var eta = match.Groups[2].Value;

                viewModel.CurrentProcessingStepProgress = progressPercentage;
                //TODO: This should be a DateTime:
                viewModel.EstimatedEndTime = eta;
            }
        }

        private void HandleLogNameStatus(string name)
        {
            var logNameHandlers = new List<(string regex, Action action)>
            {
                ("HTTP server", HandleHttpServerLogEvent),
                ("Polynomial Selection .*", () => SetCurrentProcessingStep(ProcessingSteps.PolynomialSelection)),
                ("Lattice Sieving", () => SetCurrentProcessingStep(ProcessingSteps.LatticeSieving)),
                ("Filtering.*", () => SetCurrentProcessingStep(ProcessingSteps.Filtering)),
                ("Linear Algebra", () => SetCurrentProcessingStep(ProcessingSteps.LinearAlgebra)),
                ("Quadratic Characters", () => SetCurrentProcessingStep(ProcessingSteps.QuadraticCharacters)),
                ("Square Root", () => SetCurrentProcessingStep(ProcessingSteps.SquareRoot))
            };

            foreach (var (_, action) in logNameHandlers.Where(handler => Regex.IsMatch(name, handler.regex)))
            {
                action();
            }
        }

        private void HandleHttpServerLogEvent()
        {
            if (!httpServerRunning)
            {
                httpServerRunning = true;
                HttpServerRunningEvent?.Invoke();
            }
        }

        private void SetCurrentProcessingStep(ProcessingSteps step)
        {
            if (step > viewModel.CurrentProcessingStep)
            {
                viewModel.CurrentProcessingStepProgress = 0;
                viewModel.CurrentProcessingStep = step;
            }
        }
    }
}
