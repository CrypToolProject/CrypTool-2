/*
   Copyright 2011 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using CrypTool.AnalysisMonoalphabeticSubstitution.Properties;
using CrypTool.CrypAnalysisViewControl;
using CrypTool.PluginBase;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using LanguageStatisticsLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.AnalysisMonoalphabeticSubstitution
{
    public delegate void PluginProgress(double current, double maximum);
    public delegate void UpdateOutput(string key_string, string plaintext_string);

    internal delegate void UpdateKeyDisplay(KeyCandidate keyCan);

    [Author("Andreas Grüner", "Andreas.Gruener@web.de", "Humboldt University Berlin", "http://www.hu-berlin.de")]
    [PluginInfo("CrypTool.AnalysisMonoalphabeticSubstitution.Properties.Resources", "PluginCaption", "PluginTooltip", "AnalysisMonoalphabeticSubstitution/Documentation/doc.xml", "AnalysisMonoalphabeticSubstitution/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]

    public class AnalysisMonoalphabeticSubstitution : ICrypComponent
    {
        #region Private Variables

        private readonly AnalysisMonoalphabeticSubstitutionSettings settings = new AnalysisMonoalphabeticSubstitutionSettings();

        // StopFlag
        private readonly StopFlag stopFlag = new StopFlag();

        // Working data
        private Alphabet ptAlphabet = null;
        private Alphabet ctAlphabet = null;
        private Dictionary langDic = null;
        private Text cText = null;
        private List<KeyCandidate> keyCandidates;
        private string ciphertextalphabet;
        private string plaintextalphabet;
        private string keyoutput;
        private Grams grams;

        // Input property variables
        private string ciphertext;

        // Output property variables
        private string plaintext;
        private string plaintextalphabetoutput;

        // Presentation
        private readonly AssignmentPresentation masPresentation = new AssignmentPresentation();
        private DateTime startTime;
        private DateTime endTime;
        private long totalKeys;
        private double keysPerSecond;

        // Attackers
        private DictionaryAttacker dicAttacker;
        private GeneticAttacker genAttacker;
        private HillclimbingAttacker hillAttacker;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "CiphertextCaption", "CiphertextTooltip", true)]
        public string Ciphertext
        {
            get => ciphertext;
            set => ciphertext = value;
        }

        [PropertyInfo(Direction.InputData, "CiphertextAlphabetCaption", "CiphertextAlphabetTooltip", false)]
        public string CiphertextAlphabet
        {
            get => ciphertextalphabet;
            set => ciphertextalphabet = value;
        }

        [PropertyInfo(Direction.OutputData, "PlaintextCaption", "PlaintextTooltip", true)]
        public string Plaintext => plaintext;

        [PropertyInfo(Direction.OutputData, "PlaintextAlphabetOutputCaption", "PlaintextAlphabetOutputTooltip", true)]
        public string PlaintextAlphabetOutput => plaintextalphabetoutput;

        [PropertyInfo(Direction.OutputData, "KeyOutputCaption", "KeyOutputTooltip", true)]
        public string KeyOutput => keyoutput;

        #endregion

        #region IPlugin Members

        public ISettings Settings => settings;

        public UserControl Presentation => masPresentation;

        public UserControl QuickWatchPresentation => null;

        public void PreExecution()
        {
            Ciphertext = null;
            CiphertextAlphabet = null;
        }

        public void Execute()
        {
            genAttacker = new GeneticAttacker();
            dicAttacker = new DictionaryAttacker();
            hillAttacker = new HillclimbingAttacker();

            bool inputOK = true;

            // Clear presentation
            ((AssignmentPresentation)Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    ((AssignmentPresentation)Presentation).Entries.Clear();
                }
                catch (Exception ex)
                {
                    GuiLogMessage("Exception while clearing entries list:" + ex.Message, NotificationLevel.Error);
                }
            }, null);

            // Prepare the cryptanalysis of the ciphertext            
            ciphertext = ciphertext.ToLower();

            // Set alphabets
            string lang = LanguageStatistics.LanguageCode(settings.Language);

            // create language statics
            grams = LanguageStatistics.CreateGrams(settings.Language, DirectoryHelper.DirectoryLanguageStatistics, (GramsType)(settings.GramsType + 1), settings.UseSpaces);
            grams.Normalize(10_000_000);

            plaintextalphabet = grams.Alphabet;
            ciphertextalphabet = string.IsNullOrEmpty(CiphertextAlphabet)
                ? new string(Ciphertext.ToLower().Distinct().OrderBy(c => c).ToArray()).Replace("\r", "").Replace("\n", "")
                : new string(CiphertextAlphabet.ToLower().Distinct().OrderBy(c => c).ToArray()).Replace("\r", "").Replace("\n", "");

            if (ciphertextalphabet[0] == ' ')
            {
                ciphertextalphabet = ciphertextalphabet.Trim() + " ";
            }

            ptAlphabet = new Alphabet(plaintextalphabet);
            ctAlphabet = new Alphabet(ciphertextalphabet);

            if (settings.ChooseAlgorithm == 1)
            {
                ciphertext = ciphertext.ToLower();
                plaintextalphabet = plaintextalphabet.ToLower();
                ciphertextalphabet = plaintextalphabet;

                ptAlphabet = new Alphabet(plaintextalphabet);
                ctAlphabet = new Alphabet(ciphertextalphabet);

                // Dictionary
                try
                {
                    langDic = new Dictionary(settings.Language);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(Resources.error_dictionary + ": " + ex.Message, NotificationLevel.Error);
                }
                // Dictionary correct?
                if (langDic == null)
                {
                    GuiLogMessage(Resources.no_dictionary, NotificationLevel.Warning);
                }
            }

            // Plaintext Alphabet
            plaintextalphabetoutput = plaintextalphabet;
            OnPropertyChanged("PlaintextAlphabetOutput");

            if (ciphertext != null)
            {
                cText = new Text(ciphertext.ToLower(), ctAlphabet, settings.TreatmentInvalidChars);
            }
            else
            {
                cText = null;
            }

            // PTAlphabet correct?
            if (ptAlphabet == null)
            {
                GuiLogMessage(Resources.no_plaintext_alphabet, NotificationLevel.Error);
                inputOK = false;
            }

            // CTAlphabet correct?
            if (ctAlphabet == null)
            {
                GuiLogMessage(Resources.no_ciphertext_alphabet, NotificationLevel.Error);
                inputOK = false;
            }

            // Ciphertext correct?
            if (cText == null)
            {
                GuiLogMessage(Resources.no_ciphertext, NotificationLevel.Error);
                inputOK = false;
            }

            // Check length of ciphertext and plaintext alphabet
            if (ctAlphabet.Length > ptAlphabet.Length)
            {
                //if ciphertext alphabet is too long, we fallback to the plaintext alphabet
                GuiLogMessage(string.Format(Resources.error_alphabet_length, ciphertextalphabet, ciphertextalphabet.Length, plaintextalphabet, plaintextalphabet.Length), NotificationLevel.Warning);
                ctAlphabet = ptAlphabet;
            }

            // If input incorrect return otherwise execute analysis
            lock (stopFlag)
            {
                if (stopFlag.Stop)
                {
                    return;
                }
            }

            if (!inputOK)
            {
                inputOK = true;
                return;
            }

            UpdateDisplayStart();

            //this.masPresentation.DisableGUI();
            masPresentation.UpdateOutputFromUserChoice = UpdateOutput;
            keyCandidates = new List<KeyCandidate>();

            /* Algorithm:
             * 0 = Hillclimbing CPU
             * 1 = Genetic & Dictionary */
            if (settings.ChooseAlgorithm == 0)
            {
                AnalyzeHillclimbing(false);
                totalKeys = hillAttacker.TotalKeys;
            }
            else if (settings.ChooseAlgorithm == 1)
            {
                if (langDic != null)
                {
                    AnalyzeDictionary();
                }

                AnalyzeGenetic();
            }

            UpdateDisplayEnd();

            //set final plugin progress to 100%:
            OnPluginProgressChanged(this, new PluginProgressEventArgs(1.0, 1.0));
        }

        public void PostExecution()
        {
            lock (stopFlag)
            {
                stopFlag.Stop = false;
            }
            ciphertextalphabet = null;
        }

        public void Pause()
        {
        }

        public void Stop()
        {
            if (dicAttacker != null)
            {
                dicAttacker.StopFlag = true;
            }

            if (genAttacker != null)
            {
                genAttacker.StopFlag = true;
            }

            if (hillAttacker != null)
            {
                hillAttacker.StopFlag = true;
            }

            lock (stopFlag)
            {
                stopFlag.Stop = true;
            }
        }

        public void Initialize()
        {
            settings.Initialize();
        }

        public void Dispose()
        {
        }

        public void AnalyzeHillclimbing(bool GPU = false)
        {
            // Initialize analyzer
            hillAttacker.Ciphertext = ciphertext;
            hillAttacker.Restarts = settings.Restarts;
            hillAttacker.PlaintextAlphabet = plaintextalphabet;
            hillAttacker.CiphertextAlphabet = ciphertextalphabet;
            hillAttacker.grams = grams;
            hillAttacker.PluginProgressCallback = ProgressChanged;
            hillAttacker.UpdateKeyDisplay = UpdateKeyDisplay;

            // Start attack
            hillAttacker.ExecuteOnCPU();
        }

        private void AnalyzeDictionary()
        {
            ////////////////////// Create keys with dictionary attacker
            // Initialize dictionary attacker

            //this.dicAttacker = new DictionaryAttacker();
            dicAttacker.ciphertext = cText;
            dicAttacker.languageDictionary = langDic;
            dicAttacker.ciphertext_alphabet = ctAlphabet;
            dicAttacker.plaintext_alphabet = ptAlphabet;
            dicAttacker.Grams = grams;
            dicAttacker.PluginProgressCallback = ProgressChanged;
            dicAttacker.UpdateKeyDisplay = UpdateKeyDisplay;

            // Prepare text

            dicAttacker.PrepareAttack();

            // Deterministic search
            // Try to find full solution with all words enabled

            dicAttacker.SolveDeterministicFull();

            // Try to find solution with disabled words
            if (!dicAttacker.CompleteKey)
            {
                dicAttacker.SolveDeterministicWithDisabledWords();

                // Randomized search;
                if (!dicAttacker.PartialKey)
                {
                    dicAttacker.SolveRandomized();
                }
            }
        }

        private void AnalyzeGenetic()
        {
            ////////////////// Create keys with genetic attacker

            // Initialize analyzer
            genAttacker.Ciphertext = cText;
            genAttacker.Ciphertext_Alphabet = ctAlphabet;
            genAttacker.Plaintext_Alphabet = ptAlphabet;
            genAttacker.Grams = grams;
            genAttacker.PluginProgressCallback = ProgressChanged;
            genAttacker.UpdateKeyDisplay = UpdateKeyDisplay;

            // Start attack

            genAttacker.Analyze();
        }

        private void UpdateKeyDisplay(KeyCandidate keyCan)
        {
            try
            {
                bool update = false;

                // Add key if key does not already exist
                if (!keyCandidates.Contains(keyCan))
                {
                    keyCandidates.Add(keyCan);
                    keyCandidates.Sort(new KeyCandidateComparer());

                    if (keyCandidates.Count > 20)
                    {
                        keyCandidates.RemoveAt(keyCandidates.Count - 1);
                    }

                    update = true;
                }
                else
                {
                    int index = keyCandidates.IndexOf(keyCan);
                    KeyCandidate keyCanAlreadyInList = keyCandidates[index];

                    if (keyCan.DicAttack)
                    {
                        if (!keyCanAlreadyInList.DicAttack)
                        {
                            keyCanAlreadyInList.DicAttack = true;
                            update = true;
                        }
                    }
                    if (keyCan.GenAttack)
                    {
                        if (!keyCanAlreadyInList.GenAttack)
                        {
                            keyCanAlreadyInList.GenAttack = true;
                            update = true;
                        }
                    }
                    if (keyCan.HillAttack)
                    {
                        if (!keyCanAlreadyInList.HillAttack)
                        {
                            keyCanAlreadyInList.HillAttack = true;
                            update = true;
                        }
                    }
                }

                // Display output
                if (update)
                {
                    //this.plaintext = this.keyCandidates[0].Plaintext;
                    //OnPropertyChanged("Plaintext");

                    //this.plaintextalphabetoutput = CreateKeyOutput(this.keyCandidates[0].Key, this.ptAlphabet, this.ctAlphabet);
                    //OnPropertyChanged("PlaintextAlphabetOutput");
                    UpdateOutput(keyCandidates[0].Key_string, keyCandidates[0].Plaintext);

                    ((AssignmentPresentation)Presentation).Dispatcher.Invoke(DispatcherPriority.Normal,
                        (SendOrPostCallback)delegate
                       {
                           try
                           {
                               ((AssignmentPresentation)Presentation).Entries.Clear();

                               for (int i = 0; i < keyCandidates.Count; i++)
                               {
                                   KeyCandidate keyCandidate = keyCandidates[i];

                                   ResultEntry entry = new ResultEntry
                                   {
                                       Ranking = i + 1,
                                       Text = keyCandidate.Plaintext,
                                       Key = keyCandidate.Key_string
                                   };

                                   if (keyCandidate.GenAttack && !keyCandidate.DicAttack)
                                   {
                                       entry.Attack = Resources.GenAttackDisplay;
                                   }
                                   else if (keyCandidate.DicAttack && !keyCandidate.GenAttack)
                                   {
                                       entry.Attack = Resources.DicAttackDisplay;
                                   }
                                   else if (keyCandidate.GenAttack && keyCandidate.DicAttack)
                                   {
                                       entry.Attack = Resources.GenAttackDisplay + ", " + Resources.DicAttackDisplay;
                                   }
                                   else if (keyCandidate.HillAttack)
                                   {
                                       entry.Attack = Resources.HillAttackDisplay;
                                   }
                                   double fitness = keyCandidate.Fitness;
                                   entry.Value = fitness;
                                   ((AssignmentPresentation)Presentation).Entries.Add(entry);
                               }
                               ((AssignmentPresentation)Presentation).CrypAnalysisResultListView.ScrollIntoView(((AssignmentPresentation)Presentation).CrypAnalysisResultListView.Items[0]);
                           }
                           catch (Exception ex)
                           {
                               GuiLogMessage("Exception during UpdateKeyDisplay Presentation.Dispatcher: " + ex.Message, NotificationLevel.Error);
                           }
                       }, null);
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage("Exception during UpdateKeyDisplay: " + ex.Message, NotificationLevel.Error);
            }
        }

        private void UpdateDisplayStart()
        {
            ((AssignmentPresentation)Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    startTime = DateTime.Now;
                    ((AssignmentPresentation)Presentation).StartTime.Value = "" + startTime;
                    ((AssignmentPresentation)Presentation).EndTime.Value = "";
                    ((AssignmentPresentation)Presentation).ElapsedTime.Value = "";
                    ((AssignmentPresentation)Presentation).TotalKeys.Value = "";
                    ((AssignmentPresentation)Presentation).KeysPerSecond.Value = "";
                }
                catch (Exception ex)
                {
                    GuiLogMessage("Exception during UpdateDisplayStart: " + ex.Message, NotificationLevel.Error);
                }
            }, null);
        }

        private void UpdateDisplayEnd()
        {
            ((AssignmentPresentation)Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    System.Globalization.CultureInfo culture = System.Threading.Thread.CurrentThread.CurrentUICulture;

                    endTime = DateTime.Now;
                    TimeSpan elapsedtime = endTime.Subtract(startTime);
                    TimeSpan elapsedspan = new TimeSpan(elapsedtime.Days, elapsedtime.Hours, elapsedtime.Minutes, elapsedtime.Seconds, 0);

                    double totalSeconds = elapsedtime.TotalSeconds;
                    if (totalSeconds == 0)
                    {
                        totalSeconds = 0.001;
                    }

                    keysPerSecond = totalKeys / totalSeconds;

                    ((AssignmentPresentation)Presentation).EndTime.Value = "" + endTime;
                    ((AssignmentPresentation)Presentation).ElapsedTime.Value = "" + elapsedspan;
                    ((AssignmentPresentation)Presentation).TotalKeys.Value = string.Format(culture, "{0:##,#}", totalKeys);
                    ((AssignmentPresentation)Presentation).KeysPerSecond.Value = string.Format(culture, "{0:##,#}", (ulong)keysPerSecond);
                }
                catch (Exception ex)
                {
                    GuiLogMessage("Exception during UpdateDisplayEnd:" + ex.Message, NotificationLevel.Error);
                }
            }, null);
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

        #region Helper Functions       

        private void UpdateOutput(string key_string, string plaintext_string)
        {
            plaintext = plaintext_string;
            OnPropertyChanged("Plaintext");

            keyoutput = key_string;
            OnPropertyChanged("KeyOutput");
        }

        #endregion
    }

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
        public string Attack { get; set; }

        public string ClipboardValue => Value.ToString();
        public string ClipboardKey => Key;
        public string ClipboardText => Text;
        public string ClipboardEntry =>
            "Rank: " + Ranking + Environment.NewLine +
            "Value: " + Value + Environment.NewLine +
            "Attack: " + Attack + Environment.NewLine +
            "Key: " + Key + Environment.NewLine +
            "Text: " + Text;
    }

    public class StopFlag
    {
        public bool Stop { get; set; }
    }
}