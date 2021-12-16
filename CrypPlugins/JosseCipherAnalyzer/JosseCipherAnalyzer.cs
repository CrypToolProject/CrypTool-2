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
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using CrypTool.PluginBase.Utils;
using System;
using System.ComponentModel;
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
            ProgressChanged(0, 1);
            // Swap key length setting
            if (_settings.KeyLengthTo < _settings.KeyLengthFrom)
            {
                int temp = _settings.KeyLengthTo;
                _settings.KeyLengthTo = _settings.KeyLengthFrom;
                _settings.KeyLengthFrom = temp;
            }
            UpdateDisplayStart();
            for (int currentLength = _settings.KeyLengthFrom; currentLength <= _settings.KeyLengthTo; currentLength++)
            {
                _analyzer = GetAnalyzer(currentLength);
                IEvaluator evaluator = GetEvaluator();

                _analyzer.ProcessChanged += (sender, d) =>
                {
                    ProgressChanged(d, 1);
                };
                _analyzer.Evaluator = evaluator;
                ResultEntry result = _analyzer.Start(CipherTextInput);
                KeyOutput = result?.Key;
                PlainTextOutput = result?.Text;
                UpdateDisplayEnd(currentLength);
            }

            ProgressChanged(1, 1);
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

        private AttackType GetAnalyzer(int currentLength)
        {
            switch (_settings.AnalyzerMode)
            {
                case AnalyzerMode.Hillclimbing:
                    return new Hillclimbing(AlphabetInput, currentLength, _settings.Restarts, _presentation);
                case AnalyzerMode.SimulatedAnnealing:
                    return new SimulatedAnnealing(AlphabetInput, currentLength, _settings.Restarts, _presentation);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private IEvaluator GetEvaluator()
        {
            switch (_settings.CostFunctionTypes)
            {
                case CostFunctionTypes.Unigrams:
                    return new NGram(LanguageStatistics.GramsType.Unigrams, _settings.Language, _settings.UseSpaces);
                case CostFunctionTypes.Bigrams:
                    return new NGram(LanguageStatistics.GramsType.Bigrams, _settings.Language, _settings.UseSpaces);
                case CostFunctionTypes.Trigrams:
                    return new NGram(LanguageStatistics.GramsType.Trigrams, _settings.Language, _settings.UseSpaces);
                case CostFunctionTypes.Tetragrams:
                    return new NGram(LanguageStatistics.GramsType.Tetragrams, _settings.Language, _settings.UseSpaces);
                case CostFunctionTypes.Pentragrams:
                    return new NGram(LanguageStatistics.GramsType.Pentragrams, _settings.Language, _settings.UseSpaces);
                case CostFunctionTypes.Hexagrams:
                    return new NGram(LanguageStatistics.GramsType.Hexagrams, _settings.Language, _settings.UseSpaces);
                case CostFunctionTypes.IoC:
                    return new IndexOfCoincidence(AlphabetInput);
                case CostFunctionTypes.Entropy:
                    return new Entropy();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
            _analyzer.Evaluator = null;
            _startTime = new DateTime();
            _endTime = new DateTime();
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
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