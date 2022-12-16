/* 
   Copyright 2020 George Lasry, Nils Kopal, CrypTool project

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
using EnigmaAnalyzerLib;
using EnigmaAnalyzerLib.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using static CrypTool.EnigmaAnalyzer.EnigmaAnalyzer;
using static EnigmaAnalyzerLib.Key;

namespace CrypTool.EnigmaAnalyzer
{
    [Author("George Lasry, Nils Kopal", "george.lasry@CrypTool.org", "CrypTool project", "http://www.CrypTool.org")]
    [PluginInfo("CrypTool.EnigmaAnalyzer.Properties.Resources", "PluginCaption", "PluginTooltip", "EnigmaAnalyzer/DetailedDescription/doc.xml",
      "EnigmaAnalyzer/Images/Enigma.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class EnigmaAnalyzer : ICrypComponent
    {
        public delegate void OnNewBestKey(NewBestKeyEventArgs args);
        public delegate void OnNewBestPlaintext(NewBestPlaintextEventArgs args);
        public delegate void OnNewBestlistEntry(NewBestListEntryArgs args);
        public delegate void OnNewCryptanalysisStep(NewCryptanalysisStepArgs args);

        private const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const int MAX_BESTLIST_ENTRIES = 100;

        private readonly EnigmaAnalyzerSettings _settings = new EnigmaAnalyzerSettings();
        private readonly AssignmentPresentation _presentation = new AssignmentPresentation();

        private readonly IList<UnknownToken> _unknownList = new List<UnknownToken>();
        private readonly IList<UnknownToken> _lowerList = new List<UnknownToken>();

        private string _ciphertext = string.Empty;
        private string _crib = string.Empty;
        private string _plaintext = string.Empty;
        private string _key = string.Empty;
        private string _plugsInput = string.Empty;

        private UiResultReporter _resultReporter = null;

        public ISettings Settings => _settings;

        public UserControl Presentation => _presentation;

        [PropertyInfo(Direction.InputData, "CiphertextCaption", "CiphertextTooltip", true)]
        public string Ciphertext
        {
            get => _ciphertext;
            set => _ciphertext = value;
        }

        [PropertyInfo(Direction.InputData, "CribCaption", "CribTooltip", false)]
        public string Crib
        {
            get => _crib;
            set => _crib = value;
        }

        [PropertyInfo(Direction.InputData, "PlugsInputCaption", "PlugsInputTooltip", false)]
        public string PlugsInput
        {
            get => _plugsInput;
            set => _plugsInput = value;
        }

        [PropertyInfo(Direction.OutputData, "PlaintextCaption", "PlaintextTooltip", true)]
        public string Plaintext
        {
            get => _plaintext;
            set
            {
                _plaintext = value;
                OnPropertyChanged("Plaintext");
            }
        }

        [PropertyInfo(Direction.OutputData, "KeyCaption", "KeyTooltip", true)]
        public string Key
        {
            get => _key;
            set
            {
                _key = value;
                OnPropertyChanged("Key");
            }
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Format the string to contain only alphabet characters in upper case
        /// </summary>
        /// <param name="text">The string to be prepared</param>
        /// <returns>The properly formated string to be processed direct by the encryption function</returns>
        public string PreFormatInput(string text)
        {
            StringBuilder result = new StringBuilder();
            bool newToken = true;
            _unknownList.Clear();
            _lowerList.Clear();

            for (int i = 0; i < text.Length; i++)
            {
                if (ALPHABET.Contains(char.ToUpper(text[i])))
                {
                    newToken = true;
                    if (text[i] == char.ToLower(text[i])) //Solution for preserve FIXME underconstruction
                    {
                        if (_settings.UnknownSymbolHandling == 1)
                        {
                            _lowerList.Add(new UnknownToken(text[i], result.Length));
                        }
                        else
                        {
                            _lowerList.Add(new UnknownToken(text[i], i));
                        }

                    }                                      //underconstruction end
                    result.Append(char.ToUpper(text[i])); // FIXME: shall save positions of lowercase letters
                }
                else if (_settings.UnknownSymbolHandling != 1) // 1 := remove
                {
                    // 0 := preserve, 2 := replace by X
                    char symbol = _settings.UnknownSymbolHandling == 0 ? text[i] : 'X';

                    if (newToken)
                    {
                        _unknownList.Add(new UnknownToken(symbol, i));
                        newToken = false;
                    }
                    else
                    {
                        _unknownList.Last().Text += symbol;
                    }
                }
            }

            return result.ToString().ToUpper();

        }

        /// <summary>
        /// Formats the string processed by the encryption for presentation according
        /// to the settings given
        /// </summary>
        /// <param name="text">The encrypted text</param>
        /// <returns>The formatted text for output</returns>
        public string PostFormatOutput(string text)
        {
            StringBuilder workstring = new StringBuilder(text);
            foreach (UnknownToken token in _unknownList)
            {
                workstring.Insert(token.Position, token.Text);
            }

            foreach (UnknownToken token in _lowerList)   //Solution for preserve FIXME underconstruction
            {
                char help = workstring[token.Position];
                workstring.Remove(token.Position, 1);
                workstring.Insert(token.Position, char.ToLower(help));
            }                                           //underconstruction end

            switch (_settings.CaseHandling)
            {
                default:
                case 0: // preserve
                    // FIXME: shall restore lowercase letters
                    return workstring.ToString();
                case 1: // upper
                    return workstring.ToString().ToUpper();
                case 2: // lower
                    return workstring.ToString().ToLower();
            }
        }

        public void PreExecution()
        {

        }

        public void PostExecution()
        {

        }

        public void Execute()
        {
            // reset presentation
            _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _presentation.BestList.Clear();
            }, null);
            UpdateStartTime(DateTime.Now);

            _resultReporter = new UiResultReporter(this);
            _resultReporter.OnGuiLogNotificationOccured += OnGuiLogNotificationOccured;
            _resultReporter.OnPluginProgressChanged += OnPluginProgressChanged;
            _resultReporter.OnNewBestlistEntry += _resultReporter_OnNewBestlistEntry;
            _resultReporter.OnNewCryptanalysisStep += _resultReporter_OnNewCryptanalysisStep;

            switch (_settings.AnalysisMode)
            {
                case AnalysisMode.BOMBE:
                    try
                    {
                        NewCryptanalysisStepArgs step = new NewCryptanalysisStepArgs("Turing Bombe");
                        _resultReporter_OnNewCryptanalysisStep(step);
                        OnNewAnalysisMode(step);
                        PerformTuringBombeAnalysis();
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage(string.Format("Exception occured during Turing bombe analysis: {0}", ex.Message), NotificationLevel.Error);
                    }
                    finally
                    {
                        //inform cryptanalysis threads to stop
                        _resultReporter.ShouldTerminate = true;
                    }
                    break;
                case AnalysisMode.IC_SEARCH:
                    {
                        NewCryptanalysisStepArgs step = new NewCryptanalysisStepArgs("IoC Search");
                        _resultReporter_OnNewCryptanalysisStep(step);
                        OnNewAnalysisMode(step);
                        PerformICTrigramSearch(true);
                    }
                    break;
                case AnalysisMode.TRIGRAM_SEARCH:
                    {
                        NewCryptanalysisStepArgs step = new NewCryptanalysisStepArgs("Trigram Search");
                        _resultReporter_OnNewCryptanalysisStep(step);
                        OnNewAnalysisMode(step);
                        PerformICTrigramSearch(false);
                    }
                    break;
                case AnalysisMode.HILLCLIMBING:
                    {
                        NewCryptanalysisStepArgs step = new NewCryptanalysisStepArgs("Hillclimbing");
                        _resultReporter_OnNewCryptanalysisStep(step);
                        OnNewAnalysisMode(step);
                        PerformHillclimbingSimulatedAnnealing(HcSaRunnable.Mode.HC);
                    }
                    break;
                case AnalysisMode.SIMULATED_ANNEALING:
                    {
                        NewCryptanalysisStepArgs step = new NewCryptanalysisStepArgs("Simulated Annealing");
                        _resultReporter_OnNewCryptanalysisStep(step);
                        OnNewAnalysisMode(step);
                        PerformHillclimbingSimulatedAnnealing(HcSaRunnable.Mode.SA);
                    }
                    break;
                case AnalysisMode.GILLOGLY:
                    {
                        NewCryptanalysisStepArgs step = new NewCryptanalysisStepArgs("Gillogly");
                        _resultReporter_OnNewCryptanalysisStep(step);
                        OnNewAnalysisMode(step);
                        PerformGilloglyAttack();
                    }
                    break;
                default:
                    throw new NotImplementedException(string.Format("Cryptanalysis mode {0} not implemented", _settings.AnalysisMode));
            }
        }

        private void OnNewSearchSpace(Key from, Key to)
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    _presentation.SearchFrom.Value = string.Format("{0}", from.getKeystringShort());
                    _presentation.SearchTo.Value = string.Format("{0}", to.getKeystringShort());
                }
                catch (Exception)
                {
                    //do nothing
                }
            }, null);
        }


        private void OnNewAnalysisMode(NewCryptanalysisStepArgs args)
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    _presentation.AnalysisMode.Value = args.Step;
                }
                catch (Exception)
                {
                    //do nothing
                }
            }, null);
        }

        private void _resultReporter_OnNewCryptanalysisStep(NewCryptanalysisStepArgs args)
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    _presentation.AnalysisStep.Value = args.Step;
                }
                catch (Exception)
                {
                    //do nothing
                }
            }, null);
        }

        private void _resultReporter_OnNewBestlistEntry(NewBestListEntryArgs args)
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    if (_presentation.BestList.Count > 0 && args.ResultEntry.Value <= _presentation.BestList.Last().Value)
                    {
                        return;
                    }
                    //Insert new entry at correct place to sustain order of list:
                    int insertIndex = _presentation.BestList.TakeWhile(e => e.Value > args.ResultEntry.Value).Count();
                    _presentation.BestList.Insert(insertIndex, args.ResultEntry);

                    if (_presentation.BestList.Count > MAX_BESTLIST_ENTRIES)
                    {
                        _presentation.BestList.RemoveAt(MAX_BESTLIST_ENTRIES);
                    }
                    int ranking = 1;
                    foreach (BestListEntry e in _presentation.BestList)
                    {
                        e.Ranking = ranking;
                        ranking++;
                    }

                    //if we have a new number 1, we output it
                    if (args.ResultEntry.Ranking == 1)
                    {
                        Plaintext = args.ResultEntry.Text;
                        Key = args.ResultEntry.Key;
                    }

                    _presentation.CrypAnalysisResultListView.ScrollIntoView(_presentation.CrypAnalysisResultListView.Items[0]);
                }
                catch (Exception)
                {
                    //do nothing
                }
            }, null);
        }

        /// <summary>
        /// Performs a cryptanalysis using the Turing bombe algorithm
        /// </summary>
        private void PerformTuringBombeAnalysis()
        {
            //parameters for Turing bombe analysis
            string crib = Crib;
            short[] ciphertext = new short[MAXLEN];
            string strciphertext = Regex.Replace(Ciphertext != null ? Ciphertext.ToUpper() : string.Empty, "[^A-Z]", "");
            int clen;
            bool range = true;
            Key lowKey = new Key();
            Key highKey = new Key();
            Key key = new Key();
            string indicatorS = "";
            string indicatorMessageKeyS = "";
            int hc_sa_cycles = 2;
            int right_ring_sampling = 1;
            MRingScope middle_ring_scope = MRingScope.ALL;

            string crib_position = _settings.CribPositionFrom + "-" + _settings.CribPositionTo;
            int threads = _settings.CoresUsed + 1;

            //convert ciphertext to numerical representation
            clen = EnigmaUtils.getText(strciphertext, ciphertext);

            //validate/format parameters
            if (_settings.CribPositionTo < _settings.CribPositionFrom)
            {
                GuiLogMessage("Crib position invalid. The position 'from' has to be smaller or equal to the position 'to'", NotificationLevel.Error);
                return;
            }
            if (string.IsNullOrEmpty(crib))
            {
                GuiLogMessage("No crib given. Turing bombe can not work without any crib", NotificationLevel.Error);
                return;
            }
            if (string.IsNullOrEmpty(strciphertext))
            {
                GuiLogMessage("Empty ciphertext given. Turing bombe can not work without any ciphertext to analyze", NotificationLevel.Error);
                return;
            }
            if (!CheckRanges())
            {
                return;
            }
            SetPlugs(lowKey, highKey, key);

            //convert and check key range
            GenerateRangeStrings(out string rangeLowS, out string rangeHighS);
            int result = setRange(lowKey, highKey, rangeLowS, rangeHighS, _settings.Model);
            if (result != 1)
            {
                GuiLogMessage(string.Format("Invalid key range: {0}-{1} - Invalid key format, or first key has a higher value than the last key", rangeLowS, rangeHighS), NotificationLevel.Error);
                return;
            }

            OnNewSearchSpace(lowKey, highKey);

            //analysis objects
            BombeSearch bombeSearch = new BombeSearch();
            EnigmaStats enigmaStats = new EnigmaStats();

            //load correct language
            LoadAnalysisLanguage(enigmaStats);

            //perform Turing bombe analysis
            try
            {
                bombeSearch.bombeSearch(crib, ciphertext, clen, range, lowKey, highKey, key, indicatorS, indicatorMessageKeyS, hc_sa_cycles,
                    right_ring_sampling, middle_ring_scope, false, crib_position, threads, enigmaStats, _resultReporter);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception occured during execution of turing bombe analysis: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Performs a cryptanalysis using ic and trigram search
        /// </summary>
        /// <param name="findSettingsIc"></param>
        private void PerformICTrigramSearch(bool findSettingsIc)
        {
            //parameters
            short[] ciphertext = new short[MAXLEN];
            string strciphertext = Regex.Replace(Ciphertext != null ? Ciphertext.ToUpper() : string.Empty, "[^A-Z]", "");
            int clen;
            Key lowKey = new Key();
            Key highKey = new Key();

            string indicatorS = "";
            string indicatorMessageKeyS = "";

            int hc_sa_cycles = 2;
            int right_ring_sampling = 1;
            MRingScope middle_ring_scope = MRingScope.ALL;

            int threads = _settings.CoresUsed + 1;

            //check, if we have plugs
            if (!findSettingsIc)
            {
                if (string.IsNullOrEmpty(PlugsInput))
                {
                    GuiLogMessage("No plugs given. Trigram search can not work without any plugs", NotificationLevel.Error);
                    return;
                }
            }

            //convert ciphertext to numerical representation
            clen = EnigmaUtils.getText(strciphertext, ciphertext);

            //validate/format parameters                   
            if (string.IsNullOrEmpty(strciphertext))
            {
                GuiLogMessage("Empty ciphertext given. Cryptanalysis can not work without any ciphertext to analyze", NotificationLevel.Error);
                return;
            }
            if (!CheckRanges())
            {
                return;
            }
            SetPlugs(lowKey, highKey, null);

            //convert and check key range
            GenerateRangeStrings(out string rangeLowS, out string rangeHighS);
            int result = setRange(lowKey, highKey, rangeLowS, rangeHighS, _settings.Model);
            if (result != 1)
            {
                GuiLogMessage(string.Format("Invalid key range: {0}-{1} - Invalid key format, or first key has a higher value than the last key", rangeLowS, rangeHighS), NotificationLevel.Error);
                return;
            }

            OnNewSearchSpace(lowKey, highKey);

            //analysis objects
            TrigramICSearch trigramICSearch = new TrigramICSearch();
            EnigmaStats enigmaStats = new EnigmaStats();

            //load correct language
            LoadAnalysisLanguage(enigmaStats);

            //perform hill climbing
            try
            {
                trigramICSearch.searchTrigramIC(lowKey, highKey, findSettingsIc, middle_ring_scope, right_ring_sampling, false, hc_sa_cycles, 0,
                    threads, ciphertext, clen, indicatorS, indicatorMessageKeyS, enigmaStats, _resultReporter);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception occured during execution of cryptanalysis: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Sets plugs of lowKey, highKey, and key
        /// </summary>
        /// <param name="lowKey"></param>
        /// <param name="highKey"></param>
        /// <param name="key"></param>
        private void SetPlugs(Key lowKey, Key highKey, Key key)
        {
            if (!string.IsNullOrEmpty(PlugsInput))
            {
                lowKey.setStecker(PlugsInput);
                highKey.setStecker(PlugsInput);
                if (key != null)
                {
                    key.setStecker(PlugsInput);
                }
            }
        }

        /// <summary>
        /// Performs a cryptanalysis using (an improved version of) Gillogly's original attack
        /// </summary>
        /// <param name="findSettingsIc"></param>
        private void PerformGilloglyAttack()
        {
            //parameters
            short[] ciphertext = new short[MAXLEN];
            string strciphertext = Regex.Replace(Ciphertext != null ? Ciphertext.ToUpper() : string.Empty, "[^A-Z]", "");
            int clen;
            Key lowKey = new Key();
            Key highKey = new Key();

            int hc_sa_cycles = 2;
            int right_ring_sampling = 1;

            int threads = _settings.CoresUsed + 1;

            //convert ciphertext to numerical representation
            clen = EnigmaUtils.getText(strciphertext, ciphertext);

            //validate/format parameters                   
            if (string.IsNullOrEmpty(strciphertext))
            {
                GuiLogMessage("Empty ciphertext given. Cryptanalysis can not work without any ciphertext to analyze", NotificationLevel.Error);
                return;
            }
            if (!CheckRanges())
            {
                return;
            }

            SetPlugs(lowKey, highKey, null);

            //convert and check key range
            GenerateRangeStrings(out string rangeLowS, out string rangeHighS);
            int result = setRange(lowKey, highKey, rangeLowS, rangeHighS, _settings.Model);
            if (result != 1)
            {
                GuiLogMessage(string.Format("Invalid key range: {0}-{1} - Invalid key format, or first key has a higher value than the last key", rangeLowS, rangeHighS), NotificationLevel.Error);
                return;
            }

            OnNewSearchSpace(lowKey, highKey);

            //analysis objects
            GilloglyAttack gilloglyAttack = new GilloglyAttack();
            EnigmaStats enigmaStats = new EnigmaStats();

            //load correct language
            LoadAnalysisLanguage(enigmaStats);

            //perform hill climbing
            try
            {
                gilloglyAttack.PerformAttack(lowKey, highKey, right_ring_sampling, hc_sa_cycles,
                    threads, ciphertext, clen, enigmaStats, _resultReporter);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception occured during execution of cryptanalysis: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Performs a cryptanalysis using hill climbing or simulated annealing
        /// </summary>
        private void PerformHillclimbingSimulatedAnnealing(HcSaRunnable.Mode mode)
        {
            //parameters
            short[] ciphertext = new short[MAXLEN];
            string strciphertext = Regex.Replace(Ciphertext != null ? Ciphertext.ToUpper() : string.Empty, "[^A-Z]", "");
            int clen;
            bool range = true;
            int strength = 1;
            Key lowKey = new Key();
            Key highKey = new Key();
            Key key = new Key();

            int hc_sa_cycles = 2;
            int right_ring_sampling = 1;
            MRingScope middle_ring_scope = MRingScope.ALL;

            int threads = _settings.CoresUsed + 1;

            //convert ciphertext to numerical representation
            clen = EnigmaUtils.getText(strciphertext, ciphertext);

            //validate/format parameters                   
            if (string.IsNullOrEmpty(strciphertext))
            {
                GuiLogMessage("Empty ciphertext given. Cryptanalysis can not work without any ciphertext to analyze", NotificationLevel.Error);
                return;
            }
            if (!CheckRanges())
            {
                return;
            }
            SetPlugs(lowKey, highKey, key);

            //convert and check key range
            GenerateRangeStrings(out string rangeLowS, out string rangeHighS);
            int result = setRange(lowKey, highKey, rangeLowS, rangeHighS, _settings.Model);
            if (result != 1)
            {
                GuiLogMessage(string.Format("Invalid key range: {0}-{1} - Invalid key format, or first key has a higher value than the last key", rangeLowS, rangeHighS), NotificationLevel.Error);
                return;
            }

            OnNewSearchSpace(lowKey, highKey);

            //analysis objects
            HillClimb hillClimb = new HillClimb();
            EnigmaStats enigmaStats = new EnigmaStats();

            //load correct language
            LoadAnalysisLanguage(enigmaStats);

            //perform hill climbing
            try
            {
                hillClimb.hillClimbRange(range ? lowKey : key, range ? highKey : key, hc_sa_cycles, threads, 0,
                    middle_ring_scope, right_ring_sampling, ciphertext, clen, mode, strength, enigmaStats, _resultReporter);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception occured during execution of cryptanalysis: {0}", ex.Message), NotificationLevel.Error);
            }
        }

        /// <summary>
        /// Lets enigmaStats load the defined analysis language
        /// </summary>
        /// <param name="enigmaStats"></param>
        private void LoadAnalysisLanguage(EnigmaStats enigmaStats)
        {
            switch (_settings.AnalysisLanguage)
            {
                case Language.ENGLISH:
                    enigmaStats.loadBidictFromResources(EnigmaAnalyzerLib.Properties.Resources.english_logbigrams);
                    enigmaStats.loadTridictFromResource(EnigmaAnalyzerLib.Properties.Resources.english_logtrigrams);
                    break;
                case Language.GERMAN:
                default:
                    enigmaStats.loadBidictFromResources(EnigmaAnalyzerLib.Properties.Resources.german_logbigrams);
                    enigmaStats.loadTridictFromResource(EnigmaAnalyzerLib.Properties.Resources.german_logtrigrams);
                    break;
                    //todo: add french and italian resources
            }
        }

        /// <summary>
        /// Checks, if search ranges are valid, e.g. if left rotor from <= left rotor to
        /// </summary>
        /// <returns></returns>
        private bool CheckRanges()
        {
            //rotor settings
            if (_settings.Model == Model.M4 && _settings.GreekRotorFrom > _settings.GreekRotorTo)
            {
                GuiLogMessage(string.Format("Invalid key range: Greek rotor 'from' has to be smaller or equal to Greek rotor 'to'"), NotificationLevel.Error);
                return false;
            }
            if (_settings.LeftRotorFrom > _settings.LeftRotorTo)
            {
                GuiLogMessage(string.Format("Invalid key range: left rotor 'from' has to be smaller or equal to left rotor 'to'"), NotificationLevel.Error);
                return false;
            }
            if (_settings.MiddleRotorFrom > _settings.MiddleRotorTo)
            {
                GuiLogMessage(string.Format("Invalid key range: middle rotor 'from' has to be smaller or equal to middle rotor 'to'"), NotificationLevel.Error);
                return false;
            }
            if (_settings.RightRotorFrom > _settings.RightRotorTo)
            {
                GuiLogMessage(string.Format("Invalid key range: middle rotor 'from' has to be smaller or equal to middle rotor 'to'"), NotificationLevel.Error);
                return false;
            }

            //ring settings
            if (_settings.Model == Model.M4 && _settings.GreekRingFrom > _settings.GreekRingTo)
            {
                GuiLogMessage(string.Format("Invalid key range: Greek ring 'from' has to be smaller or equal to Greek ring 'to'"), NotificationLevel.Error);
                return false;
            }
            if (_settings.LeftRingFrom > _settings.LeftRingTo)
            {
                GuiLogMessage(string.Format("Invalid key range: left ring 'from' has to be smaller or equal to left ring 'to'"), NotificationLevel.Error);
                return false;
            }
            if (_settings.MiddleRingFrom > _settings.MiddleRingTo)
            {
                GuiLogMessage(string.Format("Invalid key range: middle ring 'from' has to be smaller or equal to middle ring 'to'"), NotificationLevel.Error);
                return false;
            }
            if (_settings.RightRingFrom > _settings.RightRingTo)
            {
                GuiLogMessage(string.Format("Invalid key range: middle ring 'from' has to be smaller or equal to middle ring 'to'"), NotificationLevel.Error);
                return false;
            }

            //rotor positions
            if (_settings.Model == Model.M4 && _settings.GreekRotorPositionFrom > _settings.GreekRotorPositionTo)
            {
                GuiLogMessage(string.Format("Invalid key range: Greek rotor position 'from' has to be smaller or equal to Greek rotor position 'to'"), NotificationLevel.Error);
                return false;
            }
            if (_settings.LeftRotorPositionFrom > _settings.LeftRotorPositionTo)
            {
                GuiLogMessage(string.Format("Invalid key range: left rotor position 'from' has to be smaller or equal to left rotor position 'to'"), NotificationLevel.Error);
                return false;
            }
            if (_settings.MiddleRotorPositionFrom > _settings.MiddleRotorPositionTo)
            {
                GuiLogMessage(string.Format("Invalid key range: middle rotor position 'from' has to be smaller or equal to middle rotor position 'to'"), NotificationLevel.Error);
                return false;
            }
            if (_settings.RightRotorPositionFrom > _settings.RightRotorPositionTo)
            {
                GuiLogMessage(string.Format("Invalid key range: middle rotor position 'from' has to be smaller or equal to middle rotor position 'to'"), NotificationLevel.Error);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Generates range strings for the enigma anylsis based on the settings
        /// </summary>
        /// <param name="rangeLowS"></param>
        /// <param name="rangeHighS"></param>
        private void GenerateRangeStrings(out string rangeLowS, out string rangeHighS)
        {
            /*
                  case Model.M3:
                        fminS = "B:111:AAA:AAA";
                        tmaxS = "C:888:ZZZ:ZZZ";
                        break;
                    case Model.M4:
                        fminS = "B:B111:AAAA:AAAA";
                        tmaxS = "C:G888:ZZZZ:ZZZZ";
                        break;
                    case Model.H:
                    default:
                        fminS = "A:111:AAA:AAA";
                        tmaxS = "C:555:ZZZ:ZZZ";
                        break;
            */

            switch (_settings.Model)
            {
                case Model.M4:
                    //M4 has 4 rotors
                    rangeLowS = string.Format("{0}:{1}{2}{3}{4}:{5}{6}{7}{8}:{9}{10}{11}{12}",
                       EnigmaAnalyzerSettings.GetReflectString(_settings.ReflectorFrom),
                       EnigmaAnalyzerSettings.GetRotorString(_settings.GreekRotorFrom),
                       EnigmaAnalyzerSettings.GetRotorString(_settings.LeftRotorFrom),
                       EnigmaAnalyzerSettings.GetRotorString(_settings.MiddleRotorFrom),
                       EnigmaAnalyzerSettings.GetRotorString(_settings.RightRotorFrom),
                       EnigmaAnalyzerSettings.GetRotorRingPositionString(_settings.GreekRingFrom),
                       EnigmaAnalyzerSettings.GetRotorRingPositionString(_settings.LeftRingFrom),
                       EnigmaAnalyzerSettings.GetRotorRingPositionString(_settings.MiddleRingFrom),
                       EnigmaAnalyzerSettings.GetRotorRingPositionString(_settings.RightRingFrom),
                       EnigmaAnalyzerSettings.GetRotorRingPositionString(_settings.GreekRotorPositionFrom),
                       EnigmaAnalyzerSettings.GetRotorRingPositionString(_settings.LeftRotorPositionFrom),
                       EnigmaAnalyzerSettings.GetRotorRingPositionString(_settings.MiddleRotorPositionFrom),
                       EnigmaAnalyzerSettings.GetRotorRingPositionString(_settings.RightRotorPositionFrom));
                    rangeHighS = string.Format("{0}:{1}{2}{3}{4}:{5}{6}{7}{8}:{9}{10}{11}{12}",
                        EnigmaAnalyzerSettings.GetReflectString(_settings.ReflectorTo),
                        EnigmaAnalyzerSettings.GetRotorString(_settings.GreekRotorTo),
                        EnigmaAnalyzerSettings.GetRotorString(_settings.LeftRotorTo),
                        EnigmaAnalyzerSettings.GetRotorString(_settings.MiddleRotorTo),
                        EnigmaAnalyzerSettings.GetRotorString(_settings.RightRotorTo),
                        EnigmaAnalyzerSettings.GetRotorRingPositionString(_settings.GreekRingTo),
                        EnigmaAnalyzerSettings.GetRotorRingPositionString(_settings.LeftRingTo),
                        EnigmaAnalyzerSettings.GetRotorRingPositionString(_settings.MiddleRingTo),
                        EnigmaAnalyzerSettings.GetRotorRingPositionString(_settings.RightRingTo),
                        EnigmaAnalyzerSettings.GetRotorRingPositionString(_settings.GreekRotorPositionTo),
                        EnigmaAnalyzerSettings.GetRotorRingPositionString(_settings.LeftRotorPositionTo),
                        EnigmaAnalyzerSettings.GetRotorRingPositionString(_settings.MiddleRotorPositionTo),
                        EnigmaAnalyzerSettings.GetRotorRingPositionString(_settings.RightRotorPositionTo));
                    break;

                default:
                    //all other have 3 rotors
                    rangeLowS = string.Format("{0}:{1}{2}{3}:{4}{5}{6}:{7}{8}{9}",
                        EnigmaAnalyzerSettings.GetReflectString(_settings.ReflectorFrom),
                        EnigmaAnalyzerSettings.GetRotorString(_settings.LeftRotorFrom),
                        EnigmaAnalyzerSettings.GetRotorString(_settings.MiddleRotorFrom),
                        EnigmaAnalyzerSettings.GetRotorString(_settings.RightRotorFrom),
                        EnigmaAnalyzerSettings.GetRotorRingPositionString(_settings.LeftRingFrom),
                        EnigmaAnalyzerSettings.GetRotorRingPositionString(_settings.MiddleRingFrom),
                        EnigmaAnalyzerSettings.GetRotorRingPositionString(_settings.RightRingFrom),
                        EnigmaAnalyzerSettings.GetRotorRingPositionString(_settings.LeftRotorPositionFrom),
                        EnigmaAnalyzerSettings.GetRotorRingPositionString(_settings.MiddleRotorPositionFrom),
                        EnigmaAnalyzerSettings.GetRotorRingPositionString(_settings.RightRotorPositionFrom));
                    rangeHighS = string.Format("{0}:{1}{2}{3}:{4}{5}{6}:{7}{8}{9}",
                        EnigmaAnalyzerSettings.GetReflectString(_settings.ReflectorTo),
                        EnigmaAnalyzerSettings.GetRotorString(_settings.LeftRotorTo),
                        EnigmaAnalyzerSettings.GetRotorString(_settings.MiddleRotorTo),
                        EnigmaAnalyzerSettings.GetRotorString(_settings.RightRotorTo),
                        EnigmaAnalyzerSettings.GetRotorRingPositionString(_settings.LeftRingTo),
                        EnigmaAnalyzerSettings.GetRotorRingPositionString(_settings.MiddleRingTo),
                        EnigmaAnalyzerSettings.GetRotorRingPositionString(_settings.RightRingTo),
                        EnigmaAnalyzerSettings.GetRotorRingPositionString(_settings.LeftRotorPositionTo),
                        EnigmaAnalyzerSettings.GetRotorRingPositionString(_settings.MiddleRotorPositionTo),
                        EnigmaAnalyzerSettings.GetRotorRingPositionString(_settings.RightRotorPositionTo));
                    break;

            }
        }


        public void Stop()
        {
            if (_resultReporter != null)
            {
                _resultReporter.ShouldTerminate = true;
            }
        }

        public void Initialize()
        {

        }

        public void Dispose()
        {

        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public void UpdateStartTime(DateTime startTime)
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    _presentation.StartTime.Value = startTime.ToString();
                }
                catch (Exception)
                {
                    //do nothing
                }
            }, null);
        }

        public void UpdateTimeLeftAndEndTimeLabels(TimeSpan timeLeft)
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    _presentation.TimeLeft.Value = timeLeft.ToString();
                    _presentation.EndTime.Value = (DateTime.Now + timeLeft).ToString();
                }
                catch (Exception)
                {
                    //do nothing
                }
            }, null);
        }
    }

    public class UnknownToken
    {
        public string Text { get; set; }
        public int Position { get; set; }

        public UnknownToken(char c, int position)
        {
            Text = char.ToString(c);
            Position = position;
        }

        public override string ToString()
        {
            return "[" + Text + "," + Position + "]";
        }
    }

    /// <summary>
    /// Single entry of the Bestlist
    /// </summary>
    public class BestListEntry : ICrypAnalysisResultListEntry, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private int _ranking;

        public int Ranking
        {
            get => _ranking;
            set
            {
                _ranking = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Ranking)));
            }
        }

        public double Value
        {
            get;
            set;
        }

        public string Key
        {
            get;
            set;
        }
        public string Text
        {
            get;
            set;
        }

        public double ExactValue => Math.Abs(Value);

        public string ClipboardValue => ExactValue.ToString();

        public string ClipboardKey => Key;

        public string ClipboardText => Text;

        public string ClipboardEntry =>
            Properties.Resources.RankingHeader + " " + Ranking + Environment.NewLine +
            Properties.Resources.ValueHeader + " " + ExactValue + Environment.NewLine +
            Properties.Resources.KeyHeader + " " + Key + Environment.NewLine +
            Properties.Resources.TextHeader + " " + Text;
    }

    /// <summary>
    /// The result reporter ist responsible for receiving new data during the cryptanalysis and
    /// then he raises events which are used by the component to output the data to the ui and/or
    /// component's outputs
    /// </summary>
    public class UiResultReporter : ResultReporter
    {
        private readonly EnigmaAnalyzer _EnigmaAnalyzer;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event OnNewBestKey OnNewBestKey;
        public event OnNewBestPlaintext OnNewBestPlaintext;
        public event OnNewBestlistEntry OnNewBestlistEntry;
        public event OnNewCryptanalysisStep OnNewCryptanalysisStep;

        private DateTime lastProgressUpdate = DateTime.Now;
        private int lastScore = int.MinValue;
        private long lastCount = 0;
        private const int UPDATE_INTERVAL_SECONDS = 1;

        private readonly object _lockObject = new object();

        public UiResultReporter(EnigmaAnalyzer EnigmaAnalyzer)
        {
            _EnigmaAnalyzer = EnigmaAnalyzer;
        }

        public override void displayBestKey(string key)
        {
            OnNewBestKey?.Invoke(new NewBestKeyEventArgs(key));
        }

        public override void displayBestPlaintext(string plaintext)
        {
            OnNewBestPlaintext?.Invoke(new NewBestPlaintextEventArgs(plaintext));
        }

        public override void displayProgress(long count, long max)
        {
            lock (_lockObject)
            {
                if (DateTime.Now > lastProgressUpdate.AddSeconds(UPDATE_INTERVAL_SECONDS))
                {
                    float speed = (count - lastCount) / (float)UPDATE_INTERVAL_SECONDS;
                    float totalSeconds = (max - count) / speed;

                    TimeSpan timeLeft = new TimeSpan(0, 0, 0, (int)totalSeconds);
                    _EnigmaAnalyzer.UpdateTimeLeftAndEndTimeLabels(timeLeft);
                    PluginProgressChanged(count, max);
                    lastProgressUpdate = DateTime.Now;
                    lastCount = count;
                }
            }
        }

        public override void reportResult(Key key, int currScore, string plaintext, string desc, int cribPosition = -1)
        {
            lock (_lockObject)
            {
                if (currScore > lastScore)
                {
                    BestListEntry resultEntry = new BestListEntry();
                    if (cribPosition == -1)
                    {
                        resultEntry.Key = key.getKeystringShort();
                    }
                    else
                    {
                        resultEntry.Key = string.Format("{0}, position={1}", key.getKeystringShort(), cribPosition);
                    }
                    resultEntry.Text = plaintext;
                    resultEntry.Value = currScore;
                    OnNewBestlistEntry?.Invoke(new NewBestListEntryArgs(resultEntry));
                    displayBestKey(resultEntry.Key);
                    displayBestPlaintext(resultEntry.Text);
                    lastScore = currScore;
                }
            }
        }

        public override bool shouldPushResult(int score)
        {
            return true;
        }

        public override void WriteException(string message, Exception ex)
        {
            GuiLogMessage(message, NotificationLevel.Error);
            //in case of an exception during the analysis, we stop the analysis process
            ShouldTerminate = true;
        }

        public override void WriteMessage(string message)
        {
            GuiLogMessage(message, NotificationLevel.Info);
        }

        public override void WriteWarning(string message)
        {
            GuiLogMessage(message, NotificationLevel.Warning);
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, _EnigmaAnalyzer, new GuiLogEventArgs(message, _EnigmaAnalyzer, logLevel));
        }

        private void PluginProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, _EnigmaAnalyzer, new PluginProgressEventArgs(value, max));
        }

        public override void UpdateCryptanalysisStep(string step)
        {
            OnNewCryptanalysisStep?.Invoke(new NewCryptanalysisStepArgs(step));
        }
    }

    public class NewBestKeyEventArgs : EventArgs
    {
        public string Key
        {
            get;
            private set;
        }
        public NewBestKeyEventArgs(string key) : base()
        {
            Key = key;
        }

    }

    public class NewBestPlaintextEventArgs : EventArgs
    {
        public string Plaintext
        {
            get;
            private set;
        }
        public NewBestPlaintextEventArgs(string plaintext) : base()
        {
            Plaintext = plaintext;
        }
    }

    public class NewBestListEntryArgs : EventArgs
    {
        public BestListEntry ResultEntry
        {
            get;
            private set;
        }
        public NewBestListEntryArgs(BestListEntry resultEntry) : base()
        {
            ResultEntry = resultEntry;
        }
    }

    public class NewCryptanalysisStepArgs : EventArgs
    {
        public string Step
        {
            get;
            private set;
        }
        public NewCryptanalysisStepArgs(string step) : base()
        {
            Step = step;
        }
    }

}
