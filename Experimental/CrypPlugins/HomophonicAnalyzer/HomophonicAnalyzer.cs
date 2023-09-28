/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrypTool.PluginBase;
using System.ComponentModel;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Threading;
using HomophonicAnalyzer.Properties;
using System.Threading;
using System.Text.RegularExpressions;

namespace CrypTool.Plugins.HomophonicAnalyzer
{
    public delegate void PluginProgress(double current, double maximum);
    public delegate void UpdateOutput(String key_string, String plaintext_string);
    delegate double CalculateFitness(Text plaintext);
    delegate void UpdateKeyDisplay(KeyCandidate keyCan);

    [Author("Armin Krauß", "krauss@cryptool.de", "CrypTool", "http://www.cryptool.org")]
    [PluginInfo("HomophonicAnalyzer.Properties.Resources", "PluginCaption", "PluginTooltip", "HomophonicAnalyzer/Documentation/doc.xml", "HomophonicAnalyzer/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]

    public class HomophonicAnalyzer : ICrypComponent
    {
        #region Private Variables

        private readonly HomophonicAnalyzerSettings settings = new HomophonicAnalyzerSettings();

        // StopFlag
        StopFlag stopFlag = new StopFlag();

        // Working data
        private Alphabet ptAlphabet = null;
        private Alphabet ctAlphabet = null;
        private Frequencies langFreq = null;
        private Dictionary langDic = null;
        private Text cText = null;
        private List<KeyCandidate> keyCandidates;

        // Statistics
        private TimeSpan total_time = new TimeSpan();       /*NEVER USED. WHAT TO DO ? */
        private TimeSpan currun_time;                      /*NEVER USED. WHAT TO DO ? */

        // Input property variables
        private String ciphertext;
        private String ciphertextalphabet;
        private String ciphertextalpha;

        // Output property variables
        private String plaintext;
        private String plaintext_alphabet_output;

        // Presentation
        private AssignmentPresentation masPresentation = new AssignmentPresentation();
        private DateTime startTime;
        private DateTime endTime;

        // Alphabet constants
        private const String English = "abcdefghijklmnopqrstuvwxyz";
        private const String German = "abcdefghijklmnopqrstuvwxyzäüöß";
        private const String Spanish = "abcdefghijklmnopqrstuvwxyzñ";
        private const String Latin = "abcdefghijklmnopqrstuvwxyz";
        private const String French = "abcdefghijklmnopqrstuvwxyz";
        private const String Hungarian = "abcdefghijklmnopqrstuvwxyz";
        private const String Swedish = "abcdefghijklmnopqrstuvwxyzåäö";
        private const String Italian = "abcdefghijklmnopqrstuvwxyz";
        private const String Dutch = "abcdefghijklmnopqrstuvwxyz";
        private const String Portuguese = "abcdefghijklmnopqrstuvwxyz";
        private const String Czech = "abcdefghijklmnopqrstuvwxyz";
        private const String Greek = "ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ";

        // Attackers
        private HillclimbingAttacker hillAttacker;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "PropCiphertextCaption", "PropCiphertextTooltip", true)]
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

        [PropertyInfo(Direction.OutputData, "PropPlaintextCaption", "PropPlaintextTooltip", true)]
        public String Plaintext
        {
            get { return this.plaintext; }
            set { }
        }

        [PropertyInfo(Direction.OutputData, "PropPlaintextalphabetoutputCaption", "PropPlaintextalphabetoutputTooltip", true)]
        public String Plaintext_Alphabet_Output
        {
            get { return this.plaintext_alphabet_output; }
            set { }
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings
        {
            get { return settings; }
        }

        /// <summary>
        /// HOWTO: You can provide a custom (tabbed) presentation to visualize your algorithm.
        /// Return null if you don't provide one.
        /// </summary>
        public UserControl Presentation
        {
            get { return this.masPresentation; }
        }

        /// <summary>
        /// HOWTO: You can provide custom (quickwatch) presentation to visualize your algorithm.
        /// Return null if you don't provide one.
        /// </summary>
        public UserControl QuickWatchPresentation
        {
            get { return null; }
        }

        public void PreExecution()
        {

        }

        public void Execute()
        {
            this.hillAttacker = new HillclimbingAttacker();

            Boolean inputOK = true;

            // Clear presentation
            ((AssignmentPresentation)Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ((AssignmentPresentation)Presentation).entries.Clear();
            }, null);

            // Prepare the cryptanalysis of the ciphertext

            // Set alphabet
            String plaintextalpha = detAlphabet(settings.Alphabet);
            ciphertext = Regex.Replace(ciphertext, @"\s", "");
            String distinctCiphertextLetters = new string(ciphertext.Distinct().OrderBy(c => c).ToArray());
            ciphertextalpha = String.IsNullOrEmpty(CiphertextAlphabet) ? distinctCiphertextLetters : CiphertextAlphabet;
            this.ptAlphabet = new Alphabet(plaintextalpha, 1, settings.Alphabet);
            this.ctAlphabet = new Alphabet(ciphertextalpha, 1, settings.Alphabet);

            // N-gram probabilities    
            String helper = IdentifyNGramFile(settings.Alphabet);
            if (helper != null)
            {
                this.langFreq = new Frequencies(this.ptAlphabet);
                this.langFreq.ReadProbabilitiesFromNGramFile(helper);
            }
            else
            {
                GuiLogMessage(Resources.no_ngram_file, NotificationLevel.Error);
            }

            // Dictionary
            if (settings.Alphabet == 0)
            {
                try
                {
                    this.langDic = new Dictionary("en-small.dic", ptAlphabet.Length);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(Resources.error_dictionary + " :" + ex.Message, NotificationLevel.Error);
                }
            }
            else if (settings.Alphabet == 1)
            {
                try
                {
                    this.langDic = new Dictionary("de-small.dic", ptAlphabet.Length);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(Resources.error_dictionary + " :" + ex.Message, NotificationLevel.Error);
                }
            }
            else if (settings.Alphabet == 2)
            {
                try
                {
                    this.langDic = new Dictionary("es-small.dic", ptAlphabet.Length);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(Resources.error_dictionary + " :" + ex.Message, NotificationLevel.Error);
                }
            }
            else if (settings.Alphabet == 3)
            {
                try
                {
                    this.langDic = new Dictionary("la-small.dic", ptAlphabet.Length);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(Resources.error_dictionary + " :" + ex.Message, NotificationLevel.Error);
                }
            }
            else if (settings.Alphabet == 4)
            {
                try
                {
                    this.langDic = new Dictionary("fr-small.dic", ptAlphabet.Length);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(Resources.error_dictionary + " :" + ex.Message, NotificationLevel.Error);
                }
            }
            else if (settings.Alphabet == 5)
            {
                try
                {
                    this.langDic = new Dictionary("hu-small.dic", ptAlphabet.Length);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(Resources.error_dictionary + " :" + ex.Message, NotificationLevel.Error);
                }
            }
            else if (settings.Alphabet == 6)
            {
                try
                {
                    this.langDic = new Dictionary("sv-small.dic", ptAlphabet.Length);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(Resources.error_dictionary + " :" + ex.Message, NotificationLevel.Error);
                }
            }
            else if (settings.Alphabet == 7)
            {
                try
                {
                    this.langDic = new Dictionary("it-small.dic", ptAlphabet.Length);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(Resources.error_dictionary + " :" + ex.Message, NotificationLevel.Error);
                }
            }
            else if (settings.Alphabet == 8)
            {
                try
                {
                    this.langDic = new Dictionary("nl-small.dic", ptAlphabet.Length);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(Resources.error_dictionary + " :" + ex.Message, NotificationLevel.Error);
                }
            }
            else if (settings.Alphabet == 9)
            {
                try
                {
                    this.langDic = new Dictionary("pt-small.dic", ptAlphabet.Length);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(Resources.error_dictionary + " :" + ex.Message, NotificationLevel.Error);
                }
            }
            else if (settings.Alphabet == 10)
            {
                try
                {
                    this.langDic = new Dictionary("cs-small.dic", ptAlphabet.Length);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(Resources.error_dictionary + " :" + ex.Message, NotificationLevel.Error);
                }
            }
            else if (settings.Alphabet == 11)
            {
                try
                {
                    this.langDic = new Dictionary("el-small.dic", ptAlphabet.Length);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(Resources.error_dictionary + " :" + ex.Message, NotificationLevel.Error);
                }
            }
            // Add new case for another language
            // elseif (settings.Alphabet == 1)
            // {
            // ......
            // }

            // Set ciphertext
            String helper1 = null;
            try
            {
                //helper1 = returnStreamContent(this.ciphertext);
                if (this.ciphertext.Length != 0)
                {
                    helper1 = this.ciphertext;
                }
            }
            catch
            {
                GuiLogMessage(Resources.error_ciphertext, NotificationLevel.Error);
            }

            if (helper1 != null)
            {
                this.cText = new Text(helper1, this.ctAlphabet, settings.TreatmentInvalidChars);
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
            // Dictionary correct?
            if (this.langDic == null)
            {
                GuiLogMessage(Resources.no_dictionary, NotificationLevel.Warning);
            }
            // Language frequencies
            if (this.langFreq == null)
            {
                GuiLogMessage(Resources.no_lang_freq, NotificationLevel.Error);
                inputOK = false;
            }
            // Check length of ciphertext and plaintext alphabet
            //if (this.ctAlphabet.Length != this.ptAlphabet.Length)
            //{
            //    GuiLogMessage(Resources.error_alphabet_length, NotificationLevel.Error);
            //    inputOK = false;
            //}

            // If input incorrect return otherwise execute analysis
            lock (this.stopFlag)
            {
                if (this.stopFlag.Stop) return;
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

            AnalyzeHillclimbing();

            this.UpdateDisplayEnd();

            //set final plugin progress to 100%:
            OnPluginProgressChanged(this, new PluginProgressEventArgs(1, 1));
        }

        public void PostExecution()
        {
            lock (this.stopFlag)
            {
                this.stopFlag.Stop = false;
            }
        }

        public void Pause()
        {
        }

        public void Stop()
        {
            this.hillAttacker.StopFlag = true;
            this.langDic.StopFlag = true;
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
        
        public void AnalyzeHillclimbing()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            
            // Initialize analyzer
            this.hillAttacker.Ciphertext = this.cText.ToString(this.ctAlphabet);
            this.hillAttacker.Restarts = settings.Restarts;
            this.hillAttacker.CiphertextAlphabet = ciphertextalpha;

            switch (settings.Alphabet)
            {
                case 0:
                    this.hillAttacker.PlaintextAlphabet = English;
                    break;
                case 1:
                    this.hillAttacker.PlaintextAlphabet = German;
                    break;
                case 2:
                    this.hillAttacker.PlaintextAlphabet = Spanish;
                    break;
                case 3:
                    this.hillAttacker.PlaintextAlphabet = Latin;
                    break;
                case 4:
                    this.hillAttacker.PlaintextAlphabet = French;
                    break;
                case 5:
                    this.hillAttacker.PlaintextAlphabet = Hungarian;
                    break;
                case 6:
                    this.hillAttacker.PlaintextAlphabet = Swedish;
                    break;
                case 7:
                    this.hillAttacker.PlaintextAlphabet = Italian;
                    break;
                case 8:
                    this.hillAttacker.PlaintextAlphabet = Dutch;
                    break;
                case 9:
                    this.hillAttacker.PlaintextAlphabet = Portuguese;
                    break;
                case 10:
                    this.hillAttacker.PlaintextAlphabet = Czech;
                    break;
                case 11:
                    this.hillAttacker.PlaintextAlphabet = Greek;
                    break;
                default:
                    this.hillAttacker.PlaintextAlphabet = English;
                    break;
            }

            this.hillAttacker.PluginProgressCallback = this.ProgressChanged;
            this.hillAttacker.UpdateKeyDisplay = this.UpdateKeyDisplay;

            // Start attack
            lock (this.stopFlag)
            {
                if (this.stopFlag.Stop) return;
            }

            this.hillAttacker.Execute(settings.ChooseAlgorithm);

            watch.Stop();

            //Output Performance
            string curTime = String.Format("{0:00}:{1:00}:{2:00}", watch.Elapsed.Hours, watch.Elapsed.Minutes, watch.Elapsed.Seconds);
            GuiLogMessage(Resources.hill_attack_finished + curTime, NotificationLevel.Info);
            GuiLogMessage(Resources.hill_attack_testedkeys + hillAttacker.TotalKeys.ToString("#,##0"), NotificationLevel.Info);

            //Calculate total seconds needed for hillclimbing
            int ms = watch.Elapsed.Milliseconds + 1;
            long keysPerSecond = (hillAttacker.TotalKeys * 1000) / ms;
            GuiLogMessage(Resources.hill_attack_keyspersecond + keysPerSecond.ToString("#,##0"), NotificationLevel.Info);
        }
        
        private void UpdateKeyDisplay(KeyCandidate keyCan)
        {
            try
            {
                bool update = false;

                // Add key if key does not already exists
                if (!this.keyCandidates.Contains(keyCan))
                {
                    this.keyCandidates.Add(keyCan);
                    this.keyCandidates.Sort(new KeyCandidateComparer());

                    if (this.keyCandidates.Count > 20)
                    {
                        this.keyCandidates.RemoveAt(this.keyCandidates.Count - 1);
                    }
                    update = true;
                }
                else
                {
                    int index = this.keyCandidates.IndexOf(keyCan);
                    KeyCandidate keyCanAlreadyInList = this.keyCandidates[index];
                    if (keyCan.HillAttack == true)
                    {
                        if (keyCanAlreadyInList.HillAttack == false)
                        {
                            keyCanAlreadyInList.HillAttack = true;
                            update = true;
                        }
                    }
                }

                // Display output
                if (update)
                {
                    this.plaintext = this.keyCandidates[0].Plaintext;
                    OnPropertyChanged("Plaintext");

                    this.plaintext_alphabet_output = ciphertextalpha + "\r\n" + this.keyCandidates[0].Key_string;
                    OnPropertyChanged("Plaintext_Alphabet_Output");

                    ((AssignmentPresentation)Presentation).Dispatcher.Invoke(DispatcherPriority.Normal,
                        (SendOrPostCallback)delegate
                        {
                            try
                            {
                                ((AssignmentPresentation)Presentation).entries.Clear();

                                for (int i = 0; i < this.keyCandidates.Count; i++)
                                {
                                    KeyCandidate keyCandidate = this.keyCandidates[i];

                                    ResultEntry entry = new ResultEntry();
                                    entry.Ranking = i.ToString();
                                    entry.Text = keyCandidate.Plaintext;
                                    entry.Key = keyCandidate.Key_string;
                                    
                                    if (keyCandidate.HillAttack == true)
                                    {
                                        entry.Attack = Resources.HillAttackDisplay;
                                    }

                                    //double f = Math.Log10(Math.Abs(keyCandidate.Fitness));
                                    double f = keyCandidate.Fitness;
                                    entry.Value = string.Format("{0:0.00000} ", f);
                                    ((AssignmentPresentation)Presentation).entries.Add(entry);

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
                GuiLogMessage("Exception during UpdateKeyDisplay: " + ex.Message, NotificationLevel.Error);
            }
        }

        private void UpdateDisplayStart()
        {
            ((AssignmentPresentation)Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate {
                this.startTime = DateTime.Now;
                ((AssignmentPresentation)Presentation).startTime.Content = "" + startTime;
                ((AssignmentPresentation)Presentation).endTime.Content = "";
                ((AssignmentPresentation)Presentation).elapsedTime.Content = "";
                ((AssignmentPresentation)Presentation).keysPerSecond.Content = "";
            }, null);
        }

        private void UpdateDisplayEnd()
        {
            ((AssignmentPresentation)Presentation).Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                this.endTime = DateTime.Now;
                TimeSpan elapsedtime = this.endTime.Subtract(this.startTime);
                TimeSpan elapsedspan = new TimeSpan(elapsedtime.Days, elapsedtime.Hours, elapsedtime.Minutes, elapsedtime.Seconds, 0);
                ((AssignmentPresentation)Presentation).endTime.Content = "" + this.endTime;
                ((AssignmentPresentation)Presentation).elapsedTime.Content = "" + elapsedspan;
                ((AssignmentPresentation)Presentation).keysPerSecond.Content = "";

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

        private string returnFileContent(String filename)
        {
            string res = "";

            using (TextReader reader = new StreamReader(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, filename)))
            {
                res = reader.ReadToEnd();
            }
            return res;
        }

        private string returnStreamContent(ICrypToolStream stream)
        {
            string res = "";

            if (stream == null)
            {
                return null;
            }

            using (CStreamReader reader = stream.CreateReader())
            {
                res = Encoding.Default.GetString(reader.ReadFully());


                if (res.Length == 0)
                {
                    return null;
                }
            }

            return res;
        }

        private string detAlphabet(int lang)
        {
            String alpha = "";
            // English
            if (lang == 0)
            {
                alpha = HomophonicAnalyzer.English;
            }
            else if (lang == 1)
            {
                alpha = HomophonicAnalyzer.German;
            }
            else if (lang == 2)
            {
                alpha = HomophonicAnalyzer.Spanish;
            }
            else if (lang == 3)
            {
                alpha = HomophonicAnalyzer.Latin;
            }
            else if (lang == 4)
            {
                alpha = HomophonicAnalyzer.French;
            }
            else if (lang == 5)
            {
                alpha = HomophonicAnalyzer.Hungarian;
            }
            else if (lang == 6)
            {
                alpha = HomophonicAnalyzer.Swedish;
            }
            else if (lang == 7)
            {
                alpha = HomophonicAnalyzer.Italian;
            }
            else if (lang == 8)
            {
                alpha = HomophonicAnalyzer.Dutch;
            }
            else if (lang == 9)
            {
                alpha = HomophonicAnalyzer.Portuguese;
            }
            else if (lang == 10)
            {
                alpha = HomophonicAnalyzer.Czech;
            }
            else if (lang == 11)
            {
                alpha = HomophonicAnalyzer.Greek;
            }
            // Add another case for a new language
            //else if ( lang == 1)
            //{
            //
            //}

            return alpha;
        }


        private string IdentifyNGramFile(int alpha_nr)
        {
            bool cs = false;
            string name = "";
            string lang = "";
            string casesen = "";

            if (alpha_nr == 0)
            {
                lang = "en";
            }
            else if (alpha_nr == 1)
            {
                lang = "de";
            }
            else if (alpha_nr == 2)
            {
                lang = "es";
            }
            else if (alpha_nr == 3)
            {
                lang = "la";
            }
            else if (alpha_nr == 4)
            {
                lang = "fr";
            }
            else if (alpha_nr == 5)
            {
                lang = "hu";
            }
            else if (alpha_nr == 6)
            {
                lang = "sv";
            }
            else if (alpha_nr == 7)
            {
                lang = "it";
            }
            else if (alpha_nr == 8)
            {
                lang = "nl";
            }
            else if (alpha_nr == 9)
            {
                lang = "pt";
            }
            else if (alpha_nr == 10)
            {
                lang = "cs";
            }
            else if (alpha_nr == 11)
            {
                lang = "el";
            }
            // Add another case for a new language
            //else if (alpha_nr == 1)
            //{
            //    lang = "xx";
            //}

            if (cs == false)
            {
                casesen = "nocs";
            }
            else
            {
                casesen = "cs";
            }

            // It is always looked for a 4-gram file at first. If the 4-gram file is not found the 3-gram file is choosen
            for (int i = 4; i > 2; i--)
            {
                name = lang + "-" + i.ToString() + "gram-" + casesen + ".bin";
                if (File.Exists(Path.Combine(DirectoryHelper.DirectoryLanguageStatistics, name)))
                {
                    return name;
                }
            }
            return null;
        }

        private void UpdateOutput(String key_string, String plaintext_string)
        {
            // Plaintext
            this.plaintext = plaintext_string;
            OnPropertyChanged("Plaintext");

            // Alphabet
            this.plaintext_alphabet_output = key_string;
            OnPropertyChanged("Plaintext_Alphabet_Output");
        }

        private String CreateAlphabetOutput(int[] key, Alphabet alphabet)
        {
            return String.Join("", key.Select(i => alphabet.GetLetterFromPosition(i)));
        }

        #endregion
    }

    public class ResultEntry
    {
        public string Ranking { get; set; }
        public string Value { get; set; }
        public string Key { get; set; }
        public string Text { get; set; }
        public string Attack { get; set; }
    }

    public class StopFlag
    {
        public Boolean Stop { get; set; }
    }
}
