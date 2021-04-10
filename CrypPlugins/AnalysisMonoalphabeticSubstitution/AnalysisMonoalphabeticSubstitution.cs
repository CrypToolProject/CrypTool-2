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
using System;
using System.Collections.Generic;
using System.Linq;
using CrypTool.PluginBase;
using System.ComponentModel;
using CrypTool.PluginBase.Miscellaneous;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading;
using CrypTool.AnalysisMonoalphabeticSubstitution.Properties;
using CrypTool.PluginBase.Utils;
using CrypTool.CrypAnalysisViewControl;

namespace CrypTool.AnalysisMonoalphabeticSubstitution
{
    public delegate void PluginProgress(double current, double maximum);
    public delegate void UpdateOutput(String key_string, String plaintext_string);
    delegate void UpdateKeyDisplay(KeyCandidate keyCan);

    [Author("Andreas Grüner", "Andreas.Gruener@web.de", "Humboldt University Berlin", "http://www.hu-berlin.de")]
    [PluginInfo("CrypTool.AnalysisMonoalphabeticSubstitution.Properties.Resources", "PluginCaption", "PluginTooltip", "AnalysisMonoalphabeticSubstitution/Documentation/doc.xml", "AnalysisMonoalphabeticSubstitution/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]

    public class AnalysisMonoalphabeticSubstitution : ICrypComponent
    {
        #region Private Variables

        private readonly AnalysisMonoalphabeticSubstitutionSettings settings = new AnalysisMonoalphabeticSubstitutionSettings();

        // StopFlag
        StopFlag stopFlag = new StopFlag();

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
        private String ciphertext;
      
        // Output property variables
        private String plaintext;
        private String plaintextalphabetoutput;

        // Presentation
        private AssignmentPresentation masPresentation = new AssignmentPresentation();
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
        public String Ciphertext
        {
            get { return this.ciphertext; }
            set { this.ciphertext = value; }
        }

        [PropertyInfo(Direction.InputData, "CiphertextAlphabetCaption", "CiphertextAlphabetTooltip", false)]
        public String CiphertextAlphabet
        {
            get { return this.ciphertextalphabet; }
            set { this.ciphertextalphabet = value; }
        }

        [PropertyInfo(Direction.OutputData, "PlaintextCaption", "PlaintextTooltip", true)]
        public String Plaintext
        {
            get { return this.plaintext; }
        }

        [PropertyInfo(Direction.OutputData, "PlaintextAlphabetOutputCaption", "PlaintextAlphabetOutputTooltip", true)]
        public String PlaintextAlphabetOutput
        {
            get { return this.plaintextalphabetoutput; }
        }

        [PropertyInfo(Direction.OutputData, "KeyOutputCaption", "KeyOutputTooltip", true)]
        public String KeyOutput
        {
            get { return this.keyoutput; }
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings
        {
            get { return settings; }
        }
        
        public UserControl Presentation
        {
            get { return this.masPresentation; }
        }
        
        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
            Ciphertext = null;
            CiphertextAlphabet = null;
        }

        public void Execute()
        {
            this.genAttacker = new GeneticAttacker();
            this.dicAttacker = new DictionaryAttacker();
            this.hillAttacker = new HillclimbingAttacker();
            
            Boolean inputOK = true;

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
            grams = LanguageStatistics.CreateGrams(settings.Language, (LanguageStatistics.GramsType)(settings.GramsType + 1), settings.UseSpaces);

            plaintextalphabet = grams.Alphabet;
            ciphertextalphabet = String.IsNullOrEmpty(CiphertextAlphabet)
                ? new string(Ciphertext.ToLower().Distinct().OrderBy(c => c).ToArray()).Replace("\r", "").Replace("\n", "")
                : new string(CiphertextAlphabet.ToLower().Distinct().OrderBy(c => c).ToArray()).Replace("\r", "").Replace("\n", "");             

            if(ciphertextalphabet[0] == ' ')
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
                    langDic = new Dictionary(LanguageStatistics.LanguageCode(settings.Language) + "-small.dic", plaintextalphabet.Length);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(Resources.error_dictionary + ": " + ex.Message, NotificationLevel.Error);
                }
                // Dictionary correct?
                if (this.langDic == null)
                {
                    GuiLogMessage(Resources.no_dictionary, NotificationLevel.Warning);
                }
            }

            // Plaintext Alphabet
            this.plaintextalphabetoutput = plaintextalphabet;
            OnPropertyChanged("PlaintextAlphabetOutput");

            if (ciphertext != null)
            {
                this.cText = new Text(ciphertext.ToLower(), this.ctAlphabet, settings.TreatmentInvalidChars);
            }
            else
            {
                this.cText = null;
            }

            // PTAlphabet correct?
            if (this.ptAlphabet == null)
            {
                GuiLogMessage(Resources.no_plaintext_alphabet, NotificationLevel.Error);
                inputOK = false;
            }

            // CTAlphabet correct?
            if (this.ctAlphabet == null)
            {
                GuiLogMessage(Resources.no_ciphertext_alphabet, NotificationLevel.Error);
                inputOK = false;
            }

            // Ciphertext correct?
            if (this.cText == null)
            {
                GuiLogMessage(Resources.no_ciphertext, NotificationLevel.Error);
                inputOK = false;
            }

            // Check length of ciphertext and plaintext alphabet
            if (this.ctAlphabet.Length > this.ptAlphabet.Length)
            {
                //if ciphertext alphabet is too long, we fallback to the plaintext alphabet
                GuiLogMessage(String.Format(Resources.error_alphabet_length, ciphertextalphabet, ciphertextalphabet.Length, plaintextalphabet, plaintextalphabet.Length), NotificationLevel.Warning);
                ctAlphabet = ptAlphabet;
            }
            
            // If input incorrect return otherwise execute analysis
            lock (this.stopFlag)
            {
                if (this.stopFlag.Stop)
                    return;
            }
            
            if (!inputOK)
            {
                inputOK = true;
                return;
            }

            this.UpdateDisplayStart();

            //this.masPresentation.DisableGUI();
            this.masPresentation.UpdateOutputFromUserChoice = this.UpdateOutput;
            this.keyCandidates = new List<KeyCandidate>();

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
                if (this.langDic != null)
                    AnalyzeDictionary();
                AnalyzeGenetic();
            }

            this.UpdateDisplayEnd();
            
            //set final plugin progress to 100%:
            OnPluginProgressChanged(this, new PluginProgressEventArgs(1.0, 1.0));
        }

        public void PostExecution()
        {
            lock(this.stopFlag)
            {
                this.stopFlag.Stop = false;
            }
            this.ciphertextalphabet = null;
        }

        public void Pause()
        {
        }

        public void Stop()
        {
            if (this.dicAttacker != null) this.dicAttacker.StopFlag = true;
            if (this.genAttacker != null) this.genAttacker.StopFlag = true;
            if (this.hillAttacker != null) this.hillAttacker.StopFlag = true;
            if (this.langDic != null) this.langDic.StopFlag = true;
            lock (this.stopFlag)
            {
                this.stopFlag.Stop = true;
            }
        }

        public void Initialize()
        {
            this.settings.Initialize();
        }

        public void Dispose()
        {
        }

        public void AnalyzeHillclimbing(bool GPU = false)
        {
            // Initialize analyzer
            this.hillAttacker.Ciphertext = ciphertext;
            this.hillAttacker.Restarts = settings.Restarts;
            this.hillAttacker.PlaintextAlphabet = plaintextalphabet;
            this.hillAttacker.CiphertextAlphabet = ciphertextalphabet;
            this.hillAttacker.grams = grams;
            this.hillAttacker.PluginProgressCallback = this.ProgressChanged;
            this.hillAttacker.UpdateKeyDisplay = this.UpdateKeyDisplay;

            // Start attack
            hillAttacker.ExecuteOnCPU();
        }

        private void AnalyzeDictionary()
        {
            ////////////////////// Create keys with dictionary attacker
            // Initialize dictionary attacker
            
            //this.dicAttacker = new DictionaryAttacker();
            this.dicAttacker.ciphertext = this.cText;
            this.dicAttacker.languageDictionary = this.langDic;
            this.dicAttacker.ciphertext_alphabet = this.ctAlphabet;
            this.dicAttacker.plaintext_alphabet = this.ptAlphabet;
            this.dicAttacker.Grams = this.grams;
            this.dicAttacker.PluginProgressCallback = this.ProgressChanged;
            this.dicAttacker.UpdateKeyDisplay = this.UpdateKeyDisplay;
            
            // Prepare text

            this.dicAttacker.PrepareAttack();

            // Deterministic search
            // Try to find full solution with all words enabled

            this.dicAttacker.SolveDeterministicFull();

            // Try to find solution with disabled words
            if (!this.dicAttacker.CompleteKey)
            {
                this.dicAttacker.SolveDeterministicWithDisabledWords();

                // Randomized search;
                if (!this.dicAttacker.PartialKey)
                    this.dicAttacker.SolveRandomized();
            }
        }

        private void AnalyzeGenetic()
        {
            ////////////////// Create keys with genetic attacker

            // Initialize analyzer
            this.genAttacker.Ciphertext = this.cText;
            this.genAttacker.Ciphertext_Alphabet = this.ctAlphabet;
            this.genAttacker.Plaintext_Alphabet = this.ptAlphabet;
            this.genAttacker.Grams = this.grams;
            this.genAttacker.PluginProgressCallback = this.ProgressChanged;
            this.genAttacker.UpdateKeyDisplay = this.UpdateKeyDisplay;
            
            // Start attack
            
            this.genAttacker.Analyze();
        }

        private void UpdateKeyDisplay(KeyCandidate keyCan)
        {
            try
            {
                bool update = false;

                // Add key if key does not already exist
                if (!this.keyCandidates.Contains(keyCan))
                {
                    this.keyCandidates.Add(keyCan);
                    this.keyCandidates.Sort(new KeyCandidateComparer());

                    if (this.keyCandidates.Count > 20)
                        this.keyCandidates.RemoveAt(this.keyCandidates.Count - 1);

                    update = true;
                }
                else
                {
                    int index = this.keyCandidates.IndexOf(keyCan);
                    KeyCandidate keyCanAlreadyInList = this.keyCandidates[index];

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
                    UpdateOutput(this.keyCandidates[0].Key_string, this.keyCandidates[0].Plaintext);

                    ((AssignmentPresentation) Presentation).Dispatcher.Invoke(DispatcherPriority.Normal,
                        (SendOrPostCallback) delegate
                        {
                            try
                            {
                                ((AssignmentPresentation) Presentation).Entries.Clear();

                                for (int i = 0; i < this.keyCandidates.Count; i++)
                                {
                                    KeyCandidate keyCandidate = this.keyCandidates[i];

                                    ResultEntry entry = new ResultEntry();
                                    entry.Ranking = i+1;
                                    entry.Text = keyCandidate.Plaintext;
                                    entry.Key = keyCandidate.Key_string;

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
                                    double f = keyCandidate.Fitness;
                                    entry.Value = string.Format("{0:0.00000} ", f);
                                    ((AssignmentPresentation) Presentation).Entries.Add(entry);
                                }
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
                GuiLogMessage("Exception during UpdateKeyDisplay: " +ex.Message,NotificationLevel.Error);
            }
        }  
        
        private void UpdateDisplayStart()
        {
             ((AssignmentPresentation)Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate{
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
                    var culture = System.Threading.Thread.CurrentThread.CurrentUICulture;

                    endTime = DateTime.Now;
                    TimeSpan elapsedtime = endTime.Subtract(startTime);
                    TimeSpan elapsedspan = new TimeSpan(elapsedtime.Days, elapsedtime.Hours, elapsedtime.Minutes, elapsedtime.Seconds, 0);

                    double totalSeconds = elapsedtime.TotalSeconds;
                    if (totalSeconds == 0) totalSeconds = 0.001;
                    keysPerSecond = totalKeys / totalSeconds;

                    ((AssignmentPresentation)Presentation).EndTime.Value = "" + endTime;
                    ((AssignmentPresentation)Presentation).ElapsedTime.Value = "" + elapsedspan;
                    ((AssignmentPresentation)Presentation).TotalKeys.Value = String.Format(culture, "{0:##,#}", totalKeys);
                    ((AssignmentPresentation)Presentation).KeysPerSecond.Value = String.Format(culture, "{0:##,#}", (ulong)keysPerSecond);
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

        private void UpdateOutput(string key_string, String plaintext_string)
        {
            this.plaintext = plaintext_string;
            OnPropertyChanged("Plaintext");
            
            this.keyoutput = key_string;
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

        public string Value { get; set; }
        public string Key { get; set; }
        public string Text { get; set; }
        public string Attack { get; set; }

        public string ClipboardValue => Value;
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
        public Boolean Stop { get; set; }
    }
}