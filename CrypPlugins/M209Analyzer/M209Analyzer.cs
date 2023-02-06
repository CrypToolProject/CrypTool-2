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
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;
using System;
using System.Windows.Controls;
using M209Analyzer;
using CrypTool.PluginBase.Utils;
using CrypTool.CrypAnalysisViewControl;
using System.Threading;
using System.Windows.Threading;
using Cryptool.Plugins.M209Analyzer;
using System.CodeDom;
using CrypTool.PluginBase.Utils.Logging;
using System.Diagnostics;
using static Cryptool.Plugins.M209Analyzer.CiphertextOnly;
using System.Linq;

namespace CrypTool.Plugins.M209Analyzer
{
    public enum UnknownSymbolHandlingMode
    {
        Ignore = 0,
        Remove = 1,
        Replace = 2
    }

    public enum KeyFormat
    {
        Digits,
        LatinLetters
    }

    public delegate void UpdateOutput(string keyString, string plaintextString);

    [Author("Josef Matwich", "josef.matwich@student.uni-siegen.de", "CrypTool 2 Team", "https://www.cryptool.org")]
    // You can (and should) provide a user documentation as XML file and an own icon.
    [PluginInfo("CrypTool.M209Analyzer.Properties.Resources", "M209AnalyzerCaption", "M209AnalyzerTooltip", "M209Analyzer/userdoc.xml", new[] { "CrypWin/images/default.png" })]
    // HOWTO: Change category to one that fits to your plugin. Multiple categories are allowed.
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    public class M209Analyzer : ICrypComponent
    {
        #region Private Variables

        // HOWTO: You need to adapt the settings class as well, see the corresponding file.
        private readonly M209AnalyzerSettings _settings;
        private readonly M209AnalyzerPresentation _presentation = new M209AnalyzerPresentation();
        private const int MaxBestListEntries = 100;
        private const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWKXY";
        private DateTime _startTime;
        private DateTime _endTime;

        private bool _running = false;

        //the settings gramsType is between 0 and 4. Thus, we have to add 1 to cast it to a "GramsType", which starts at 1
        private Grams grams;


        private CiphertextOnly ciphertextOnly;

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
        [PropertyInfo(Direction.InputData, "Ciphertext", "Ciphertext only")]
        public string Ciphertext
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "Knowntext", "")]
        public string Knowntext
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "P", "Putative decryption")]
        public string P
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
                throw new ArgumentException("Properties.Resources.NoCiphertextGiven");
            }

            //the settings gramsType is between 0 and 4. Thus, we have to add 1 to cast it to a "GramsType", which starts at 1
            this.grams = LanguageStatistics.CreateGrams(_settings.Language, (LanguageStatistics.GramsType)(_settings.GramsType + 1), false);
            this.grams.Normalize(10_000_000);

            _running = true;

            this.ciphertextOnly = new CiphertextOnly(grams);
            ciphertextOnly.OnLogEvent += GetMessageFromChild;
            ciphertextOnly.OnAddBestListEntry += HandleNewBestListEntry;

            this.ciphertextOnly.Running = _running;

            //this.ciphertextOnly.Encrypt("WINDOW AT THE SAME INSTANT I SAW HIM RAISE HIS HAND AND AT THE SIGNAL I TOSSED MY ROCKET INTO THE ROOM WITH A CRY OF FIRE THE WORD WAS NO SOONER OUT OF MY MOUTH THAN THE WHOLE CROWD OF SPECTATORS WELL DRESSED AND ILL GENTLEMEN OSTLERS AND SERVANT MAIDS JOINED IN A GENERAL SHRIEK OF FIRE THICK CLOUDS OF SMOKE CURLED THROUGH THE ROOM AND OUT AT THE OPEN WINDOW I CAUGHT A GLIMPSE OF RUSHING FIGURES AND A MOMENT LATER THE VOICE OF HOLMES FROM WITHIN ASSURING THEM THAT IT WAS A FALSE ALARM SLIPPING THROUGH THE SHOUTING CROWD I MADE MY WAY TO THE CORNER OF THE STREET AND IN TEN MINUTES WAS REJOICED TO FIND MY FRIEND S ARM IN MINE AND TO GET AWAY FROM THE SCENE OF UPROAR HE WALKED SWIFTLY AND IN SILENCE FOR SOME FEW MINUTES UNTIL WE HAD TURNED DOWN ONE OF THE QUIET STREETS WHICH LEAD TOWARDS THE EDGEWARE ROAD YOU DID IT VERY NICELY DOCTOR HE REMARKED NOTHING COULD HAVE BEEN BETTER IT IS ALL RIGHT YOU HAVE THE PHOTOGRAPH I KNOW WHERE IT IS AND HOW DID YOU FIND OUT SHE SHOWED ME AS I TOLD YOU SHE WOULD I AM STILL IN THE DARK I DO NOT WISH TO MAKE A MYSTERY SAID HE LAUGHING THE MATTER WAS PERFECTLY SIMPLE YOU OF COURSE SAW THAT EVERYONE IN THE STREET WAS AN ACCOMPLICE THEY WERE ALL ENGAGED FOR THE EVENING I GUESSED AS MUCH THEN WHEN THE ROW BROKE OUT I HAD A LITTLE MOIST RED PAINT IN THE PALM OF MY HAND I RUSHED FORWARD FELL DOWN CLAPPED MY HAND TO MY FACE AND BECAME A PITEOUS SPECTACLE IT IS AN OLD TRICK THAT ALSO I COULD FATHOM THEN THEY CARRIED ME IN SHE WAS BOUND TO HAVE ME IN WHAT");


            // HOWTO: Use this to show the progress of a plugin algorithm execution in the editor.
            ProgressChanged(0, 1);
            try
            {
                GuiLogMessage($"Execute: AttackMode is {_settings.AttackMode}", NotificationLevel.Info);
                // HC - Hill climb
                // SA - Simulated Anealing
                switch (_settings.AttackMode)
                {
                    case AttackMode.CiphertextOnly:
                        int[] roundLayers = new int[4];
                        double bestScore = 0.0;

                        for (int i = 0; i < 100 && this._running; i++)
                        {
                            //double currentScore = this.ciphertextOnly.HCOuter(Ciphertext, "V");
                            this.ciphertextOnly.Solve(Ciphertext, 0,"V");
                            GuiLogMessage($"BestScore: {bestScore}", NotificationLevel.Info);

                            //if(bestScore < currentScore)
                            //{
                            //    bestScore = currentScore;
                            //}
                        }

                        this.ciphertextOnly.Solve(Ciphertext, 0, "V");
                        break;
                    case AttackMode.KnownPlaintext:
                        break;
                    default:
                        break;
                }
            }
            catch(Exception ex)
            {
                GuiLogMessage(string.Format("Exception occured: {0}", ex.Message), NotificationLevel.Error);
            }

            ProgressChanged(1, 1);
        }        

        private double LogMonograms(string P)
        {
            double result = 0.0;
            for (int i = 0; i < _settings.c; i++)
            {
                result += FrequencyOfCharInP(ALPHABET[i], P) * Math.Log(GetPropabilityOfCharInLanguage(ALPHABET[i]));
            }
            return result;
        }

        private double GetPropabilityOfCharInLanguage(char character)
        {
            return 0.0;
        }

        private int FrequencyOfCharInP(char character, string P)
        {
            return 0;
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
            this.ciphertextOnly.Running = false;
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

        private void GetMessageFromChild(object sender, OnLogEventArgs e)
        {
            GuiLogMessage(e.Message, e.LogLevel);
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

        private void HandleNewBestListEntry(object sender, OnAddBestListEntryEventArgs e)
        {
            this.AddNewBestListEntry(e.Key, e.Costvalue, e.Plaintext);
        }

            /// <summary>
            /// Adds an entry to the BestList
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            /// <param name="text"></param>
            private void AddNewBestListEntry(string key, double value, string text)
        {

            //if we have a worse value than the last one, skip
            if (_presentation.BestList.Count > 0 && value <= _presentation.BestList.Last().Value)
            {
                return;
            }

            ResultEntry entry = new ResultEntry
            {
                Key = key,
                Text = text,
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

            int insertIndex = _presentation.BestList.TakeWhile(e => e.Value > entry.Value).Count();
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    //insert new entry at correct place to sustain order of list:                    
                    _presentation.BestList.Insert(insertIndex, entry);
                    if (_presentation.BestList.Count > MaxBestListEntries)
                    {
                        _presentation.BestList.RemoveAt(MaxBestListEntries);
                    }
                    int ranking = 1;
                    foreach (ResultEntry e in _presentation.BestList)
                    {
                        e.Ranking = ranking;
                        ranking++;
                    }
                }
                catch (Exception)
                {
                    //do nothing
                }
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
            }, null);
        }

        /// <summary>
        /// Updates presentation during cryptanalysis
        /// </summary>
        /// <param name="key"></param>
        private void UpdateDisplay(int[] key)
        {
            string strkey;
            switch (_settings.KeyFormat)
            {
                default:
                case KeyFormat.Digits:
                    strkey = string.Format("{0},{1},{2},{3}", key[0].ToString("D2"), key[1].ToString("D2"), key[2].ToString("D2"), key[3].ToString("D2"));
                    break;
                case KeyFormat.LatinLetters:
                    strkey = "TODO: GenerateTextKey(key);";
                    break;
            }

            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _endTime = DateTime.Now;
                TimeSpan elapsedtime = _endTime.Subtract(_startTime);
                TimeSpan elapsedspan = new TimeSpan(elapsedtime.Days, elapsedtime.Hours, elapsedtime.Minutes, elapsedtime.Seconds, 0);
                _presentation.EndTime.Value = _endTime.ToString();
                _presentation.ElapsedTime.Value = elapsedspan.ToString();
                _presentation.CurrentlyAnalyzedKey.Value = strkey;
            }
            , null);
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
