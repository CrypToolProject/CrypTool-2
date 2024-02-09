/*
   Copyright CrypTool 2 Team josef.matwich@gmail.com

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
using CrypTool.CrypAnalysisViewControl;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using M209AnalyzerLib;
using M209AnalyzerLib.Common;
using M209AnalyzerLib.M209;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.Plugins.M209Analyzer
{
    public enum KeyFormat
    {
        Digits,
        LatinLetters
    }

    public delegate void UpdateOutput(string keyString, string plaintextString);

    [Author("Josef Matwich", "josef.matwich@gmail.com", "CrypTool 2 Team", "https://www.cryptool.org")]
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("CrypTool.Plugins.M209Analyzer.Properties.Resources", "M209AnalyzerCaption", "M209AnalyzerTooltip", "M209Analyzer/userdoc.xml", "M209Analyzer/Images/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific), ComponentCategory(ComponentCategory.CiphersClassic)]
    public class M209Analyzer : ICrypComponent
    {
        #region Private Variables

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        private readonly M209AnalyzerSettings _settings;
        private readonly M209AnalyzerPresentation _presentation = new M209AnalyzerPresentation();
        private M209AttackManager _m209AttackManager;
        private const int MAXBESTLISTENTRIES = 100;
        private const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWKXY";
        private DateTime _startTime;
        private DateTime _endTime;

        private DateTime _lastUpdateTime = DateTime.Now;
        private long _lastEvaluationCount = 0;

        private bool _running = false;

        private int _approximatedKeys = int.MaxValue;

        public M209Analyzer()
        {
            _settings = new M209AnalyzerSettings();
            _presentation.UpdateOutputFromUserChoice += UpdateOutputFromUserChoice;
        }

        #endregion

        #region Data Properties

        /// <summary>
        /// HOWTO: Input interface to read the input data. 
        /// You can add more input properties of other type if needed.
        /// </summary>
        [PropertyInfo(Direction.InputData, "CiphertextCaption", "CiphertextTooltip")]
        public string Ciphertext
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "KnownPlaintextCaption", "KnownPlaintextTooltip")]
        public string KnownPlaintext
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "PlaintextCaption", "PlaintextTooltip", false)]
        public string Plaintext
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "KeyCaption", "KeyTooltip", false)]
        public string Key
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings
        {
            get { return _settings; }
        }

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation
        {
            get { return _presentation; }
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
            this.UpdateDisplayStart();

            // Clear presentation
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ((M209AnalyzerPresentation)Presentation).BestList.Clear();
            }, null);

            if (string.IsNullOrWhiteSpace(Ciphertext) || string.IsNullOrWhiteSpace(Ciphertext))
            {
                throw new ArgumentException(Properties.Resources.NoCiphertextGiven);
            }

            _running = true;

            _m209AttackManager = new M209AttackManager();
            _approximatedKeys = ApproximateKeyCountTarget(Ciphertext.Length);

            _m209AttackManager.UseOwnBestList = false;
            _m209AttackManager.OnNewBestListEntry += HandleNewBestListEntry;
            _m209AttackManager.OnProgressStatusChanged += _m209AttackManager_OnProgressStatusChanged;
            _m209AttackManager.ShouldStop = !_running;

            _m209AttackManager.Threads = _settings.CoresUsed;

            _m209AttackManager.SAParameters.MinRatio = Math.Log(_settings.MinRatio);
            _m209AttackManager.SAParameters.StartTemperature = _settings.StartTemperature;
            _m209AttackManager.SAParameters.EndTemperature = _settings.EndTemperature;
            _m209AttackManager.SAParameters.Decrement = _settings.Decrement;

            ProgressChanged(0, 1);
            try
            {
                switch (_settings.AttackMode)
                {
                    case AttackMode.CiphertextOnly:
                        if (Ciphertext != null)
                        {
                            _m209AttackManager.CipherTextOnlyAttack(Ciphertext);
                        }
                        break;
                    case AttackMode.KnownPlaintext:
                        if (Ciphertext != null && KnownPlaintext != null)
                        {
                            _m209AttackManager.KnownPlainTextAttack(Ciphertext, KnownPlaintext);
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {

                GuiLogMessage(string.Format(Properties.Resources.ExceptionMessage, ex.Message), NotificationLevel.Error);
            }

            ProgressChanged(1, 1);
        }
        private void _m209AttackManager_OnProgressStatusChanged(object sender, M209AttackManager.OnProgressStatusChangedEventArgs e)
        {
            if (DateTime.Now >= _lastUpdateTime.AddSeconds(1))
            {
                long diffEvaluationCount = _m209AttackManager.EvaluationCount - _lastEvaluationCount;
                TimeSpan diffUpdateTime = DateTime.Now - _lastUpdateTime;
                Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    _endTime = DateTime.Now;
                    TimeSpan elapsedtime = _m209AttackManager.ElapsedTime;
                    TimeSpan elapsedspan = new TimeSpan(elapsedtime.Days, elapsedtime.Hours, elapsedtime.Minutes, elapsedtime.Seconds, 0);
                    _presentation.EndTime.Value = _endTime.ToString();
                    _presentation.ElapsedTime.Value = elapsedspan.ToString();
                    _presentation.CurrentlyAnalyzedKey.Value = e.EvaluationCount.ToString();
                    _presentation.CurrentlyAnalyzedKey.Value = $"2^{(long)(Math.Log(_m209AttackManager.EvaluationCount) / Math.Log(2))}";
                    _presentation.KeyPerMilliSecond.Value = $"{(_m209AttackManager.ElapsedTime.TotalMilliseconds == 0 ? 0 : (diffEvaluationCount / diffUpdateTime.TotalMilliseconds) * 1000).ToString("0#,0")} {Properties.Resources.KeysPerSec}";

                    if (_m209AttackManager.Threads == 1)
                    {
                        _presentation.AnalysisStep.Value = e.Phase;
                    }
                    else
                    {
                        _presentation.AnalysisStep.Value = Properties.Resources.AnalysisStepText;
                    }
                }
                , null);
                _lastUpdateTime = DateTime.Now;
                _lastEvaluationCount = _m209AttackManager.EvaluationCount;
            }
        }

        /// <summary>
        /// Called once after workflow execution has stopped.
        /// </summary>
        public void PostExecution()
        {
        }

        /// <summary>
        /// Triggered time when user clicks stop button.
        /// Shall abort long-running execution.
        /// </summary>
        public void Stop()
        {
            _running = false;
            _m209AttackManager.ShouldStop = true;
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

        private int ApproximateKeyCountTarget(int cipherTextLength)
        {
            if (_settings.AttackMode == AttackMode.CiphertextOnly)
            {

                if (cipherTextLength < 1000)
                {
                    return 1_000_000_000;
                }
                else if (cipherTextLength >= 1000 && cipherTextLength < 1249)
                {
                    return 210_000_000;
                }
                else if (cipherTextLength >= 1250 && cipherTextLength < 1499)
                {
                    return 133_000_000;
                }
                if (cipherTextLength >= 1500 && cipherTextLength > 1499)
                {
                    return 100_000_000;
                }
            }
            else if (_settings.AttackMode == AttackMode.KnownPlaintext)
            {
                if (cipherTextLength < 50)
                {
                    return 800_000_000;
                }
                else if (cipherTextLength >= 50 && cipherTextLength < 74)
                {
                    return 100_000_000;
                }
                else if (cipherTextLength >= 74 && cipherTextLength < 100)
                {
                    return 25_000_000;
                }
                else if (cipherTextLength >= 100)
                {
                    return 17_000_000;
                }
            }
            return 0;
        }

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

        /// <summary>
        /// User wants to output a selected key and text
        /// </summary>
        /// <param name="keyString"></param>
        /// <param name="plaintextString"></param>
        private void UpdateOutputFromUserChoice(string keyString, string plaintextString)
        {
            Key = keyString;
            Plaintext = plaintextString;
            OnPropertyChanged("Key");
            OnPropertyChanged("Plaintext");
        }

        private void HandleNewBestListEntry(object sender, M209AttackManager.OnNewBestListEntryEventArgs args)
        {
            AddNewBestListEntry(args.Key.ToString(), args.Score, args.Decryption, args.Key);
        }

        /// <summary>
        /// Adds an entry to the BestList
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="text"></param>
        private void AddNewBestListEntry(string key, double value, int[] text, Key currentKey)
        {
            //if we have a worse value than the last one, skip
            if (_presentation.BestList.Count > 10 && value <= _presentation.BestList.Last().Value)
            {
                return;
            }

            if (text == null)
            {
                Console.Write("Test");
            }

            ResultEntry entry = new ResultEntry
            {
                Key = key,
                Text = Utils.GetString(text),
                Value = value
            };

            //if we have a better value than the first one, also output it
            if (_presentation.BestList.Count == 0 || value > _presentation.BestList.First().Value)
            {
                Plaintext = entry.Text;
                Key = entry.Key;
                OnPropertyChanged("Plaintext");
                OnPropertyChanged("Key");
            }

            int insertIndex = 0;
            for (int i = 0; i < _presentation.BestList.Count; i++)
            {
                if (_presentation.BestList[i].Value > entry.Value)
                {
                    insertIndex = i + 1;
                }
                if (_presentation.BestList[i].Value == entry.Value)
                {
                    return;
                }
            }

            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    //insert new entry at correct place to sustain order of list:                    
                    _presentation.BestList.Insert(insertIndex, entry);
                    if (_presentation.BestList.Count > MAXBESTLISTENTRIES)
                    {
                        _presentation.BestList.RemoveAt(MAXBESTLISTENTRIES);
                    }
                    int ranking = 1;
                    foreach (ResultEntry e in _presentation.BestList)
                    {
                        e.Ranking = ranking;
                        ranking++;
                    }
                    var testValue = _presentation.BestList.First().Value;

                    if (_presentation.BestList.Count == 0 || entry.Value >= _presentation.BestList.First().Value)
                    {

                        // Show never 100% if correct not found
                        if (_m209AttackManager.EvaluationCount >= _approximatedKeys && _settings.AttackMode == AttackMode.CiphertextOnly)
                        {
                            ProgressChanged(0.99, 1.0);
                        }
                        else if (_settings.AttackMode == AttackMode.CiphertextOnly)
                        {
                            ProgressChanged(_presentation.BestList.First().Value, 50_000);
                        }
                        else if (_settings.AttackMode == AttackMode.KnownPlaintext)
                        {
                            if (_presentation.BestList.Count > 0)
                            {
                                ProgressChanged(_presentation.BestList[0].Value, 130_000);
                            }
                            else
                            {
                                ProgressChanged(0, 130_000);
                            }
                        }
                    }

                }
                catch (Exception)
                {
                    //do nothing
                }
                _endTime = DateTime.Now;
                TimeSpan elapsedtime = _m209AttackManager.ElapsedTime;
                TimeSpan elapsedspan = new TimeSpan(elapsedtime.Days, elapsedtime.Hours, elapsedtime.Minutes, elapsedtime.Seconds, 0);
                _presentation.EndTime.Value = _endTime.ToString();
                _presentation.ElapsedTime.Value = elapsedspan.ToString();

            }, null);
        }

        /// <summary>
        /// Resets presentation at startup
        /// </summary>
        private void UpdateDisplayStart()
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _startTime = DateTime.Now;
                _presentation.StartTime.Value = _startTime.ToString();
                _presentation.EndTime.Value = string.Empty;
                _presentation.ElapsedTime.Value = string.Empty;
                _presentation.CurrentlyAnalyzedKey.Value = string.Empty;
                _presentation.AnalysisMode.Value = _settings.AttackMode.ToString();
            }, null);
        }
    }

    /// <summary>
    /// A single entry of the best list presentation
    /// </summary>
    public class ResultEntry : ICrypAnalysisResultListEntry, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int ranking;
        public int Ranking
        {
            get => ranking;
            set
            {
                ranking = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Ranking)));
            }
        }

        public double Value { get; set; }

        public string DisplayValue
        {
            get
            {
                return $"{Value:N0}";
            }
        }

        public string Key { get; set; }
        public string Text { get; set; }

        public string ClipboardValue => Value.ToString();
        public string ClipboardKey => Key;
        public string ClipboardText => Text;
        public string ClipboardEntry =>
            "Rank: " + Ranking + Environment.NewLine +
            "Value: " + Value + Environment.NewLine +
            "Key: " + Key + Environment.NewLine +
            "Text: " + Text;
    }
}
