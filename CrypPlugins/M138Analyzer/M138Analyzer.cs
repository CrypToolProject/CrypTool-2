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
using CrypTool.CrypAnalysisViewControl;
using CrypTool.PluginBase;
using CrypTool.PluginBase.IO;
using CrypTool.PluginBase.Miscellaneous;
using CrypTool.PluginBase.Utils;
using LanguageStatisticsLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.M138Analyzer
{
    [Author("Nils Rehwald", "nilsrehwald@gmail.com", "Uni Kassel", "https://www.ais.uni-kassel.de")]
    [PluginInfo("CrypTool.M138Analyzer.Properties.Resources", "PluginCaption", "PluginTooltip", "M138Analyzer/userdoc.xml", "M138Analyzer/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class M138Analyzer : ICrypComponent
    {
        #region Private Variables

        private List<int[]> StripList = new List<int[]>(); //Store used strips
        private List<int[]> invStripList;
        private readonly int BestListLength = 20;

        private bool _isStopped = true;
        private DateTime _startTime;
        private DateTime _lastbeeptime;
        private int MinOffsetUserSelect;
        private int MaxOffsetUserSelect;
        private double KeysPerSecondCurrent = 0;
        private double KeysPerSecondAverage = 0;
        private bool fastConverge = false;

        private readonly M138AnalyzerSettings settings = new M138AnalyzerSettings();
        private int Attack = 0; //What attack whould be used

        private Grams grams;

        private string StripAlphabet;   // alphabet used by the strips
        private readonly string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";        // alphabet used by the chosen language, StripAlphabet may not use characters that are not contained in Alphabet

        private Dictionary<char, int> Char2Num;
        private int[] CiphertextNumbers;
        private int[] PlaintextNumbers;
        private int KeyLength = 25; //Default key for M138
        //private int MaxRetriesNecessary;
        private double BestCostValueOfAllKeys = double.MinValue;
        private int progressCounter, progressMax;


        private readonly M138AnalyzerPresentation _presentation = new M138AnalyzerPresentation();
        #endregion
        #region Constructor
        public M138Analyzer()
        {
            settings = new M138AnalyzerSettings();
            settings.UpdateTaskPaneVisibility();
            settings.PropertyChanged += new PropertyChangedEventHandler(settings_PropertyChanged);
            _presentation.SelectedResultEntry += SelectedResultEntry;
        }
        #endregion

        #region Data Properties

        //Inputs
        [PropertyInfo(Direction.InputData, "PlaintextCaption", "PlaintextTooltip")]
        public string Plaintext
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "CiphertextCaption", "CiphertextTooltip", true)]
        public string Ciphertext
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "StripsCaption", "StripsTooltip")]
        public string Strips
        {
            get;
            set;
        }

        //Outputs
        [PropertyInfo(Direction.OutputData, "ResultTextCaption", "ResultTextTooltip")]
        public string ResultText
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "CalculatedKeyCaption", "CalculatedKeyTooltip")]
        public string CalculatedKey
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        /// <summary>
        /// Provide plugin-related parameters (per instance) or return null.
        /// </summary>
        public ISettings Settings => settings;

        /// <summary>
        /// Provide custom presentation to visualize the execution or return null.
        /// </summary>
        public UserControl Presentation => _presentation;

        private void SelectedResultEntry(ResultEntry resultEntry)
        {
            try
            {
                CalculatedKey = resultEntry.Key;
                ResultText = resultEntry.Text;
                OnPropertyChanged("CalculatedKey");
                OnPropertyChanged("ResultText");
            }
            catch (Exception)
            {
            }
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
            int _restartNtimes;
            ResultEntry re;
            int[] _tmpKey;

            setLanguage();

            List<string> strips = SetStrips(string.IsNullOrEmpty(Strips) ? LoadStrips() : Strips);
            if (!CheckStrips(strips))
            {
                return;
            }

            Char2Num = Enumerable.Range(0, Alphabet.Length).ToDictionary(i => Alphabet[i]);
            StripList = strips.Select(s => MapTextIntoNumberSpace(s)).ToList();


            invStripList = StripList.Select(s =>
            {
                int[] inv = new int[Alphabet.Length];
                for (int i = 0; i < s.Length; i++)
                {
                    inv[i] = -1;
                }

                for (int i = 0; i < s.Length; i++)
                {
                    inv[s[i]] = i;
                }

                return inv;
            }).ToList();

            DateTime _estimatedEndTime = DateTime.Now;

            _isStopped = false;

            // Clear presentation
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                ((M138AnalyzerPresentation)Presentation).BestList.Clear();
            }, null);

            // Get Settings
            getUserSelections();

            switch (Attack)
            {
                #region Known Plaintext
                case 0: // Known Plaintext
                    if (string.IsNullOrEmpty(Ciphertext) || string.IsNullOrEmpty(Plaintext))
                    {
                        GuiLogMessage("Please provide ciphertext and plaintext to perform a known-plaintext attack", NotificationLevel.Error);
                        return;
                    }

                    Plaintext = RemoveInvalidChars(Plaintext.ToUpper(), StripAlphabet);
                    Ciphertext = RemoveInvalidChars(Ciphertext.ToUpper(), StripAlphabet);

                    //assert that ciphertext and plaintext have the same length
                    if (Plaintext.Length > Ciphertext.Length)
                    {
                        Plaintext.Remove(Ciphertext.Length);
                    }
                    else if (Plaintext.Length < Ciphertext.Length)
                    {
                        Ciphertext.Remove(Plaintext.Length);
                    }

                    CiphertextNumbers = MapTextIntoNumberSpace(Ciphertext);
                    PlaintextNumbers = MapTextIntoNumberSpace(Plaintext);

                    double _costValue1 = grams.CalculateCost(PlaintextNumbers);

                    //Call known-plaintext attack
                    //TextLength should be at least 25
                    List<string> AllPossibleKeysAsString = new List<string>();

                    UpdateDisplayStart();
                    DateTime _globalStartTime = DateTime.Now;

                    for (int offset = MinOffsetUserSelect; offset <= MaxOffsetUserSelect; offset++) //Go Over Keylength (Try all possible offsets)
                    {
                        DateTime _startTime = DateTime.Now;

                        List<List<int>> _keysForOffset = KnownPlaintextAttack(offset, Plaintext.Length, StripList.Count, StripList[0].Length);
                        if (_keysForOffset != null) //Found a Key for this offset
                        {
                            string s = string.Join(", ", _keysForOffset.Select(k => (k.Count > 1) ? "[" + string.Join("|", k) + "]" : k[0].ToString()));
                            //AddNewBestListEntry()
                            AddNewBestListEntryKnownPlaintext(s, _costValue1, offset);
                            AllPossibleKeysAsString.Add(s + " / " + offset);
                        }

                        TimeSpan _elapsedTime = DateTime.Now - _startTime;
                        _estimatedEndTime = _globalStartTime.AddSeconds(_elapsedTime.TotalSeconds * (MaxOffsetUserSelect + 1 - offset));

                        UpdateDisplayEnd(offset, _estimatedEndTime);
                        ProgressChanged(offset - MinOffsetUserSelect + 1, MaxOffsetUserSelect - MinOffsetUserSelect + 1);
                    }

                    CalculatedKey = string.Join(Environment.NewLine, AllPossibleKeysAsString);
                    ResultText = Plaintext;
                    OnPropertyChanged("CalculatedKey");
                    OnPropertyChanged("ResultText");
                    break;
                #endregion
                #region Partially Known Plaintext
                case 1: // Partially Known Plaintext
                    if (string.IsNullOrEmpty(Plaintext))
                    {
                        GuiLogMessage("Please provide a plaintext for a partially-known plaintext attack", NotificationLevel.Error);
                        return;
                    }

                    if (string.IsNullOrEmpty(Ciphertext))
                    {
                        GuiLogMessage("Please provide a ciphertext for a partially-known plaintext attack", NotificationLevel.Error);
                        return;
                    }

                    if (Ciphertext.Length < Plaintext.Length)
                    {
                        GuiLogMessage("For a partially-known plaintext attack, the length of the known ciphertext needs to be larger than the length of the known plaintext. Otherwise, please perform a known-plaintext attack", NotificationLevel.Error);
                        return;
                    }

                    Plaintext = RemoveInvalidChars(Plaintext.ToUpper(), Alphabet);
                    Ciphertext = RemoveInvalidChars(Ciphertext.ToUpper(), Alphabet);
                    PlaintextNumbers = MapTextIntoNumberSpace(Plaintext);
                    CiphertextNumbers = MapTextIntoNumberSpace(Ciphertext);

                    _restartNtimes = settings.HillClimbRestarts;

                    progressCounter = 0;
                    progressMax = MaxOffsetUserSelect - MinOffsetUserSelect + 1;

                    UpdateDisplayStart();

                    for (int offset = MinOffsetUserSelect; offset <= MaxOffsetUserSelect; offset++) //Do a known-plaintext on the known plaintext and afterwards do a Hill Climbing on the complete ciphertext
                    {
                        DateTime _startTime = DateTime.Now;
                        List<List<int>> _keysForOffset = KnownPlaintextAttack(offset, PlaintextNumbers.Length, StripList.Count, StripList[0].Length);
                        if (_keysForOffset != null) //Found a Key for this offset, do Hill Climbing on complete ciphertext
                        {
                            int countOnlyOne = 0;
                            int _numPosKeys = 1;
                            foreach (List<int> l in _keysForOffset)
                            {
                                _numPosKeys *= l.Count;
                                if (l.Count == 1)
                                {
                                    countOnlyOne++;
                                }
                            }

                            if (_numPosKeys > 100000)
                            {
                                //Go through Keylist. Eliminate largest List, Remove from List of fixed locations. Check whether still more than 1000 possibilities.
                                //if yes, delete next
                                //if no, repeat
                                //too much to do hill climbing on every possible key
                                if (countOnlyOne <= 10)
                                {
                                    GuiLogMessage("Too many possible keys found. Handling not yet implemented, please try a ciphertext-only attack.", NotificationLevel.Error);
                                    //Not Yet Implemented
                                    return;
                                }

                                int[] _tempKey = new int[KeyLength];
                                int[] _fixedPositions = new int[KeyLength];

                                int _i = 0;
                                foreach (List<int> l in _keysForOffset)
                                {
                                    if (l.Count == 1)
                                    {
                                        _tempKey[_i] = l[0];
                                        _fixedPositions[_i] = 1;
                                    }
                                    else
                                    {
                                        _tempKey[_i] = 0;
                                        _fixedPositions[_i] = 0;
                                    }
                                    _i++;
                                }

                                HillClimb(CiphertextNumbers, KeyLength, offset, StripList, _restartNtimes, fastConverge, _tempKey, _fixedPositions);
                            }
                            else
                            {
                                //Do Hill Climbing with given Startkey
                                int listLength = _keysForOffset.Count;
                                int[] counters = new int[listLength];
                                int[] _tmplistLen = new int[listLength];

                                for (int _tmpCount = 0; _tmpCount < listLength; _tmpCount++)
                                {
                                    counters[_tmpCount] = 0;
                                    _tmplistLen[_tmpCount] = _keysForOffset[_tmpCount].Count - 1;
                                }

                                bool increaseNext = false;
                                for (int _tmpCount = 0; _tmpCount < _numPosKeys; _tmpCount++)
                                {
                                    if (_isStopped)
                                    {
                                        return;
                                    }

                                    //Generate all possible keys
                                    int[] _tempKey = new int[KeyLength];

                                    for (int _tmpCount2 = 0; _tmpCount2 < listLength; _tmpCount2++)
                                    {
                                        if (increaseNext)
                                        {
                                            if (counters[_tmpCount2] < _tmplistLen[_tmpCount2])
                                            {
                                                counters[_tmpCount2]++;
                                                increaseNext = false;
                                            }
                                            else
                                            {
                                                counters[_tmpCount2] = 0;
                                                increaseNext = true;
                                            }
                                        }
                                        else
                                        {

                                        }
                                        _tempKey[_tmpCount2] = _keysForOffset[_tmpCount2][counters[_tmpCount2]];
                                    }

                                    //Create Bestlist
                                    int[] _plainText = Decrypt(CiphertextNumbers, _tempKey, offset, StripList);
                                    double _costValue = grams.CalculateCost(_plainText);
                                    AddNewBestListEntry(_tempKey, _costValue, CiphertextNumbers, offset);
                                    increaseNext = true;

                                    TimeSpan _elapsedTime = DateTime.Now - _startTime;
                                    _estimatedEndTime = DateTime.Now.AddSeconds(_elapsedTime.TotalSeconds * (MaxOffsetUserSelect + 1 - offset));
                                    UpdateDisplayEnd(offset, _estimatedEndTime);
                                    ProgressChanged(_tmpCount, _numPosKeys);
                                }

                            }
                        }

                        UpdateDisplayEnd(MaxOffsetUserSelect, DateTime.Now);
                        ProgressChanged(++progressCounter, progressMax);
                    }

                    re = _presentation.BestList.First();
                    UpdateDisplayEnd(re.Offset, DateTime.Now);
                    string _tmpKeyStrips2 = re.Key.Split('/')[0];
                    int[] _tmpKey2 = _tmpKeyStrips2.Split(',').Select(n => Convert.ToInt32(n)).ToArray();

                    CalculatedKey = re.Key;
                    ResultText = MapNumbersIntoTextSpace(Decrypt(CiphertextNumbers, _tmpKey2, re.Offset, StripList));
                    OnPropertyChanged("CalculatedKey");
                    OnPropertyChanged("ResultText");
                    break;
                #endregion
                #region Hill Climbing
                case 2: // Hill Climbing
                    if (string.IsNullOrEmpty(Ciphertext))
                    {
                        GuiLogMessage("Please provide a ciphertext to perform Hill Climbing", NotificationLevel.Error);
                        return;
                    }

                    Ciphertext = RemoveInvalidChars(Ciphertext.ToUpper(), StripAlphabet);
                    CiphertextNumbers = MapTextIntoNumberSpace(Ciphertext);
                    _restartNtimes = settings.HillClimbRestarts;

                    UpdateDisplayStart();

                    progressCounter = 0;
                    progressMax = MaxOffsetUserSelect - MinOffsetUserSelect + 1;

                    _estimatedEndTime = DateTime.Now;

                    for (int offset = MinOffsetUserSelect; offset <= MaxOffsetUserSelect; offset++)
                    {
                        if (_isStopped)
                        {
                            return;
                        }

                        DateTime _startTime = DateTime.Now;
                        UpdateDisplayEnd(offset, _estimatedEndTime);
                        HillClimb(CiphertextNumbers, KeyLength, offset, StripList, _restartNtimes, fastConverge);
                        TimeSpan _elapsedTime = DateTime.Now - _startTime;
                        _estimatedEndTime = DateTime.Now.AddSeconds(_elapsedTime.TotalSeconds * (MaxOffsetUserSelect - offset));

                        UpdateDisplayEnd(offset, _estimatedEndTime);
                        ProgressChanged(++progressCounter, progressMax);
                    }

                    re = _presentation.BestList.First();
                    UpdateDisplayEnd(re.Offset, DateTime.Now);
                    _tmpKey = re.Key.Split('/')[0].Split(',').Select(n => Convert.ToInt32(n)).ToArray();

                    CalculatedKey = re.Key;
                    ResultText = MapNumbersIntoTextSpace(Decrypt(CiphertextNumbers, _tmpKey, re.Offset, StripList));
                    OnPropertyChanged("CalculatedKey");
                    OnPropertyChanged("ResultText");
                    break;
                #endregion
                #region Simulated Annealing
                case 3: // Simulated Annealing
                    if (string.IsNullOrEmpty(Ciphertext))
                    {
                        GuiLogMessage("Please provide a ciphertext to perform Simulated Annealing", NotificationLevel.Error);
                        return;
                    }

                    Ciphertext = RemoveInvalidChars(Ciphertext.ToUpper(), StripAlphabet);
                    CiphertextNumbers = MapTextIntoNumberSpace(Ciphertext);

                    UpdateDisplayStart();

                    progressCounter = 0;
                    progressMax = MaxOffsetUserSelect - MinOffsetUserSelect + 1;
                    globalmaxscore = 0;

                    _estimatedEndTime = DateTime.Now;

                    for (int offset = MinOffsetUserSelect; offset <= MaxOffsetUserSelect; offset++)
                    {
                        if (_isStopped)
                        {
                            return;
                        }

                        DateTime _startTime = DateTime.Now;
                        UpdateDisplayEnd(offset, _estimatedEndTime);

                        SimulatedAnnealing(CiphertextNumbers, KeyLength, offset, settings.HillClimbRestarts, StripList);

                        TimeSpan _elapsedTime = DateTime.Now - _startTime;
                        _estimatedEndTime = DateTime.Now.AddSeconds(_elapsedTime.TotalSeconds * (MaxOffsetUserSelect - offset));

                        UpdateDisplayEnd(offset, _estimatedEndTime);
                        ProgressChanged(++progressCounter, progressMax);
                    }

                    re = _presentation.BestList.First();
                    UpdateDisplayEnd(re.Offset, DateTime.Now);
                    _tmpKey = re.Key.Split('/')[0].Split(',').Select(n => Convert.ToInt32(n)).ToArray();

                    CalculatedKey = re.Key;
                    ResultText = MapNumbersIntoTextSpace(Decrypt(CiphertextNumbers, _tmpKey, re.Offset, StripList));
                    OnPropertyChanged("CalculatedKey");
                    OnPropertyChanged("ResultText");
                    break;
                    #endregion
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
            _isStopped = true;
            KeysPerSecondCurrent = 0;
            KeysPerSecondAverage = 0;
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
            Strips = null;
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

        #region Helpers

        private void setLanguage()
        {
            string lang = LanguageStatistics.LanguageCode(settings.Language);
            try
            {
                grams = new Pentagrams(lang, DirectoryHelper.DirectoryLanguageStatistics);
            }
            catch (Exception)
            {
                grams = new Tetragrams(lang, DirectoryHelper.DirectoryLanguageStatistics);
            }
        }

        private void HillClimb(int[] _cipherText, int _keyLength, int _keyOffset, List<int[]> _stripes, int _restarts = 10, bool _fastConverge = false, int[] _startKey = null, int[] _fixedPos = null)
        {
            int _numberOfStrips = _stripes.Count; //Anzahl verfügbarer Streifen
            double _globalBestKeyCost = double.MinValue; //Kostenwert des globalen Maximums fuer diesen Offset
            int[] _globalBestKey;

            int[] _plainText = new int[_cipherText.Length];
            int[] _localBestKey = new int[_numberOfStrips];
            int[] _copykey = new int[_numberOfStrips]; //Copy of Runkey

            int _keyCount = 0;
            DateTime _startTime = DateTime.Now;
            Random _rand = new Random(Guid.NewGuid().GetHashCode());

            for (int RetryCounter = 0; RetryCounter < _restarts; RetryCounter++)
            {
                int[] _runkey = new int[_numberOfStrips]; //Runkey that contains every possible Strip. Only the first _keyLength strips will be used though
                List<int> _elements = Enumerable.Range(0, _numberOfStrips).ToList();

                //Startkey is given, just fill the empty parts of the key with random strips
                if (_startKey != null)
                {
                    for (int _i = 0; _i < _startKey.Length; _i++)
                    {
                        if (_elements.Contains(_startKey[_i]))
                        { //Given Startkey value at this position is a valid strip
                            _runkey[_i] = _startKey[_i]; //Use strip in Runkey
                            _elements.Remove(_startKey[_i]); //Remove Strip from list so it's not used twice
                        }
                        else
                        { //Location in startkey is empty / invalid
                            int _number = _rand.Next(0, _elements.Count);
                            _runkey[_i] = _elements[_number]; //Use a random strip
                            _elements.Remove(_elements[_number]);
                        }
                    }
                    for (int _i = _startKey.Length; _i < _numberOfStrips; _i++)
                    {
                        //Fill rest of the runkey
                        int _number = _rand.Next(0, _elements.Count);
                        _runkey[_i] = _elements[_number];
                        _elements.Remove(_elements[_number]);
                    }
                }
                //No Startkey is given, generate a random startkey
                else
                {
                    for (int _i = 0; _i < _numberOfStrips; _i++)
                    {
                        int _number = _rand.Next(0, _elements.Count);
                        _runkey[_i] = _elements[_number];
                        _elements.Remove(_elements[_number]);
                    }
                }

                //Do the actual Hill Climbing, Do Permutations of key,...

                bool _foundBetterKey;
                double _localBestKeyCost = double.MinValue;

                do
                {
                    if (_isStopped)
                    {
                        return;
                    }

                    _foundBetterKey = false;

                    //Iterate over the first 25 elements of the key (the actual key)
                    for (int i = 0; i < _keyLength; i++)
                    {
                        //Iterate over the complete key (100 elements) and swap one of the first 25 elements with one element of the key
                        //TODO: Might it be better to swap >1 elements at one time?
                        for (int j = i + 1; j < _numberOfStrips; j++)
                        {
                            Array.Copy(_runkey, _copykey, _numberOfStrips);

                            //Swap 2 Elements in copykey
                            if (_fixedPos == null)
                            {
                                int _tmpElement = _copykey[i];
                                _copykey[i] = _copykey[j];
                                _copykey[j] = _tmpElement;
                            }
                            else
                            {
                                //if (_fixedPos[i] == 1 & _fixedPos[j] == 1)
                                //{
                                //    continue;
                                //}
                                if (_fixedPos[i] == 1)
                                {
                                    continue;
                                }

                                if (j < 25 && _fixedPos[j] == 1)
                                {
                                    continue;
                                }

                                int _tmpElement = _copykey[i];
                                _copykey[i] = _copykey[j];
                                _copykey[j] = _tmpElement;

                                //TEST
                                _tmpElement = _copykey[(i + 7) % _keyLength];
                                _copykey[(i + 7) % _keyLength] = _copykey[(j + 3) % _numberOfStrips];
                                _copykey[(j + 3) % _numberOfStrips] = _tmpElement;
                                //TODO: Swap more elements at one time?
                            }

                            DecryptInPlace(_cipherText, _plainText, _copykey, _keyLength, _keyOffset, _stripes);
                            double _costValue = grams.CalculateCost(_plainText);

                            if (_localBestKeyCost < _costValue) //Tested key is better than the best local key so far
                            {
                                _localBestKeyCost = _costValue;
                                Array.Copy(_copykey, _localBestKey, _numberOfStrips);

                                //Fill Bestlist if necessary (could be that all keys found in a different run already have been better and there's no need to save this crap
                                _foundBetterKey = true;
                                if (_fastConverge)
                                {
                                    Array.Copy(_localBestKey, _runkey, _numberOfStrips);
                                }
                            }

                            _keyCount++;
                        }
                    }
                    Array.Copy(_localBestKey, _runkey, _numberOfStrips);
                } while (_foundBetterKey);

                TimeSpan _timeForOneRestart = DateTime.Now - _startTime;
                KeysPerSecondCurrent = _keyCount / _timeForOneRestart.TotalSeconds;
                KeysPerSecondAverage = (RetryCounter * KeysPerSecondAverage + KeysPerSecondCurrent) / (RetryCounter + 1);
                UpdateKeysPerSecond((int)KeysPerSecondCurrent, (int)KeysPerSecondAverage);

                //UpdateDisplayEnd(_keyOffset, _calcualtedEndTime);

                AddNewBestListEntry(_localBestKey.Take(KeyLength).ToArray(), _localBestKeyCost, CiphertextNumbers, _keyOffset, settings.HighscoreBeep);

                if (_localBestKeyCost > _globalBestKeyCost) //Found a better key than the best key found so far
                {
                    //New best global Key found
                    _globalBestKeyCost = _localBestKeyCost;
                    _globalBestKey = _localBestKey.Take(KeyLength).ToArray();

                    if (BestCostValueOfAllKeys < _globalBestKeyCost)
                    {
                        BestCostValueOfAllKeys = _globalBestKeyCost;

                        //New Best key over all offsets found, update output
                        ResultText = MapNumbersIntoTextSpace(Decrypt(CiphertextNumbers, _globalBestKey, _keyOffset, StripList));
                        OnPropertyChanged("ResultText");
                    }
                }
            }
        }

        private void save()
        {
            Array.Copy(key, oldkey, key.Length);
        }

        private void restore()
        {
            Array.Copy(oldkey, key, key.Length);
        }

        private void swap(int[] key, int i, int j)
        {
            int tmp = key[i];
            key[i] = key[j];
            key[j] = tmp;
        }

        private int[] randomKey()
        {
            int[] newkey = new int[key.Length];

            for (int i = 0; i < key.Length; i++)
            {
                newkey[i] = key[i];
            }

            for (int i = 0; i < key.Length; i++)
            {
                swap(newkey, i, rnd.Next(key.Length));
            }

            return newkey;
        }

        private double calculateScore(int[] _cipherText, int[] key, int _keyOffset, List<int[]> _stripes)
        {
            DecryptInPlace(_cipherText, plaintext, key, keyLength, _keyOffset, _stripes);
            //double score = Pentagrams.CalculateCost(plaintext);
            double score = grams.CalculateCost(plaintext);
            return score;
        }

        private void randomize()
        {
            int r = rnd.Next(100);

            if (r < 90)
            {
                swap(key, rnd.Next(keyLength), rnd.Next(key.Length));
            }
            else
            {
                for (int j = 0; j <= rnd.Next(5); j++)
                {
                    swap(key, rnd.Next(keyLength), rnd.Next(key.Length));
                }
            }
        }

        private void printbest(double score, double temp)
        {
            CalculatedKey = string.Format(" // {0:f14} {1:f14} temp={2:f14}\r", score, maxScore, temp);
            OnPropertyChanged("CalculatedKey");
        }

        private int[] key, oldkey;
        private int[] plaintext;
        private int keyLength;
        private int printcounter;
        private uint killcounter;
        private readonly Random rnd = new Random();
        private double maxScore, globalmaxscore;

        private void SimulatedAnnealing(int[] _cipherText, int _keyLength, int _keyOffset, int iterations, List<int[]> _stripes)
        {
            keyLength = _keyLength;
            key = Enumerable.Range(0, _stripes.Count).ToArray();
            oldkey = new int[key.Length];
            plaintext = new int[_cipherText.Length];
            double score = calculateScore(_cipherText, key, _keyOffset, _stripes);
            score = Math.Exp(grams.CalculateCost(_cipherText)); ;

            double inittemp = 0.1;
            double epsilon = 0.01;
            double factor = 0.999;

            DateTime starttime = DateTime.Now;
            DateTime lastupdate = DateTime.Now;
            DateTime nextupdate = lastupdate.AddSeconds(1);

            uint keycount = 0;
            uint totalkeycount = 0;

            for (int iteration = 0; iteration < iterations; iteration++)
            {
                key = randomKey();

                double workingScore;
                printcounter = 0;
                killcounter = 0;

                double currentScore = calculateScore(_cipherText, key, _keyOffset, _stripes);
                maxScore = currentScore;

                for (double temp = inittemp; temp > epsilon; temp *= factor)
                {
                    if (_isStopped)
                    {
                        return;
                    }

                    for (int t = 0; t < 1500; t++)
                    {
                        save();
                        randomize();

                        workingScore = calculateScore(_cipherText, key, _keyOffset, _stripes);

                        if (maxScore < workingScore)
                        {
                            maxScore = workingScore;
                            AddNewBestListEntry(key.Take(keyLength).ToArray(), maxScore, CiphertextNumbers, _keyOffset, settings.HighscoreBeep);
                            if (globalmaxscore < maxScore)
                            {
                                globalmaxscore = maxScore;
                                killcounter = 0;
                            }
                        }

                        double diff = workingScore - currentScore;
                        double p = Math.Exp(diff / temp);
                        bool accept = (diff >= 0) || (rnd.NextDouble() < p);

                        //if (++printcounter == 5000)
                        //{
                        //    printbest(currentScore, temp);
                        //    printcounter = 0;
                        //}

                        if (accept)
                        {
                            save();
                            currentScore = workingScore;
                        }

                        restore();

                        keycount++;
                        totalkeycount++;
                        killcounter++;
                        if (killcounter > settings.KillCounter)
                        {
                            break;
                        }
                    }


                    if (DateTime.Now >= nextupdate)
                    {
                        KeysPerSecondCurrent = keycount / (DateTime.Now - lastupdate).TotalSeconds;
                        KeysPerSecondAverage = totalkeycount / (DateTime.Now - starttime).TotalSeconds;
                        UpdateKeysPerSecond((int)KeysPerSecondCurrent, (int)KeysPerSecondAverage);

                        lastupdate = DateTime.Now;
                        nextupdate = lastupdate.AddSeconds(1);
                        keycount = 0;
                    }

                    if (killcounter > settings.KillCounter)
                    {
                        break;
                    }
                }
            }
        }

        private List<List<int>> KnownPlaintextAttack(int _offset, int _textLength, int _availableStrips, int _stripLength)
        {
            List<List<int>> _workingStrips = new List<List<int>>();
            List<List<int>> _possibleStrips = new List<List<int>>();

            for (int location = 0; location < _textLength; location++) //Go over the whole text
            {
                List<int> _possibleStripsForThisLocation = new List<int>();
                for (int i = 0; i < _availableStrips; i++)
                {
                    //Test each strip for this offset
                    if (StripList[i][(invStripList[i][PlaintextNumbers[location]] + _offset) % _stripLength] == CiphertextNumbers[location])
                    {
                        _possibleStripsForThisLocation.Add(i);
                    }
                }

                if (_possibleStripsForThisLocation.Count == 0)
                {
                    return null; //No strips work for this location, so the whole offset will not have a valid key. Break here
                }

                _possibleStrips.Add(_possibleStripsForThisLocation);
            }

            //Now there should be a non-empty list of Strips with working offsets for each position
            for (int location = 0; location < KeyLength; location++)
            {
                if (location >= _textLength)
                {
                    break;
                }

                //Make advantage of the period and check which strips still work
                List<int> _possibleStripsForThisLocation = _possibleStrips[location];
                for (int tmp = location + KeyLength; tmp < _textLength; tmp += KeyLength) //Go through the whole text again
                {
                    _possibleStripsForThisLocation = _possibleStripsForThisLocation.Intersect(_possibleStrips[tmp]).ToList<int>();
                    if (_possibleStripsForThisLocation.Count == 0)
                    {
                        return null;
                    }
                }
                _workingStrips.Add(_possibleStripsForThisLocation); //In Working Strips we should now have KeyLength Elements of Lists that each hold possible strips for their location
                //Now make this to a list of Lists that holds all possible keys
            }

            //Prevent strips from appearing twice in a key
            int _tmpCount = _workingStrips.Count; //Size of the list
            bool _stripWasKicked = true;

            while (_stripWasKicked)
            {
                _stripWasKicked = false;
                for (int i = 0; i < _tmpCount; i++) //Iterate over the List
                {
                    if (_workingStrips[i].Count == 1)
                    {
                        int _analyzedStrip = _workingStrips[i][0];
                        foreach (List<int> l in _workingStrips)
                        {
                            if (l != _workingStrips[i]) //Don't compare Location with itself
                            {
                                if (l.Contains(_analyzedStrip))
                                {
                                    l.Remove(_analyzedStrip);
                                    if (l.Count == 0)
                                    {
                                        return null;
                                    }

                                    _stripWasKicked = true;
                                }
                            }
                        }
                    }
                }
            }

            return _workingStrips;
        }

        private IEnumerable<IEnumerable<int>> PermuteAllKeys(IEnumerable<IEnumerable<int>> sequences)
        {
            IEnumerable<IEnumerable<int>> result = new[] { Enumerable.Empty<int>() };

            return sequences.Aggregate(result, (accumulator, sequence) => from accseq in accumulator from item in sequence select accseq.Concat(new[] { item }));
        }

        private void DecryptInPlace(int[] cipherText, int[] plainText, int[] key, int keylength, int keyoffset, List<int[]> stripes)
        {
            int stripeslength = stripes[0].Length;
            int ofs = (((-keyoffset) % stripeslength) + stripeslength) % stripeslength;

            for (int i = 0; i < cipherText.Length; i++)
            {
                int j = key[i % keylength];
                int k = invStripList[j][cipherText[i]];
                plainText[i] = stripes[j][(k + ofs) % stripeslength];
            }
        }

        private int[] Decrypt(int[] cipherText, int[] key, int keyoffset, List<int[]> stripes)
        {
            int[] plainText = new int[cipherText.Length];
            DecryptInPlace(cipherText, plainText, key, key.Length, keyoffset, stripes);
            return plainText;
        }

        private int[] MapTextIntoNumberSpace(string text)
        {
            return text.Select(c => Char2Num[c]).ToArray();
        }

        private string MapNumbersIntoTextSpace(int[] numbers)
        {
            return new string(numbers.Select(n => Alphabet[n]).ToArray());
        }

        private List<string> SetStrips(string text)
        {
            return text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        private string LoadStrips()
        {
            return File.ReadAllText(Path.Combine(DirectoryHelper.DirectoryCrypPlugins, "stripes.txt"));
        }

        private bool CheckStrips(List<string> strips)
        {
            if (strips == null || strips.Count == 0)
            {
                GuiLogMessage("The strips are undefined.", NotificationLevel.Error);
                return false;
            }

            StripAlphabet = string.Concat(strips[0].OrderBy(c => c).Distinct());

            foreach (string strip in strips)
            {
                string uniq = string.Concat(strip.OrderBy(c => c).Distinct());

                if (uniq.Length != strip.Length)
                {
                    GuiLogMessage("Error in strip '" + strip + "'. It contains duplicates.", NotificationLevel.Error);
                    return false;
                }

                if (uniq != StripAlphabet)
                {
                    GuiLogMessage("Error in strip '" + strip + "'. It uses a character set that differs from the first strip.", NotificationLevel.Error);
                    return false;
                }
            }

            if (RemoveInvalidChars(StripAlphabet, Alphabet).Length < StripAlphabet.Length)
            {
                GuiLogMessage("The strips use characters that are not allowed in the selected language.", NotificationLevel.Error);
                return false;
            }

            return true;
        }

        private void UpdateDisplayStart()
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                _startTime = DateTime.Now;
                ((M138AnalyzerPresentation)Presentation).StartTime.Value = "" + _startTime;
                ((M138AnalyzerPresentation)Presentation).EndTime.Value = "";
                ((M138AnalyzerPresentation)Presentation).ElapsedTime.Value = "";
            }, null);
        }

        private void UpdateDisplayEnd(int _offset, DateTime _estimatedEnd)
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                TimeSpan elapsedtime = DateTime.Now - _startTime;
                TimeSpan elapsedspan = new TimeSpan(elapsedtime.Days, elapsedtime.Hours, elapsedtime.Minutes, elapsedtime.Seconds, 0);
                ((M138AnalyzerPresentation)Presentation).EndTime.Value = "" + _estimatedEnd;
                ((M138AnalyzerPresentation)Presentation).ElapsedTime.Value = "" + elapsedspan;
                ((M138AnalyzerPresentation)Presentation).CurrentAnalysedKeylength.Value = "" + _offset;
            }, null);
        }

        private void UpdateKeysPerSecond(int _current, int _average)
        {
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                System.Globalization.CultureInfo culture = System.Threading.Thread.CurrentThread.CurrentUICulture;

                ((M138AnalyzerPresentation)Presentation).KeysPerSecondAverageLabel.Value = string.Format(culture, "{0:##,#}", _average);
                ((M138AnalyzerPresentation)Presentation).KeysPerSecondCurrentLabel.Value = string.Format(culture, "{0:##,#}", _current);
            }, null);
        }

        private void AddNewBestListEntry(int[] key, double value, int[] ciphertext, int offset, bool beep = false)
        {
            ResultEntry entry = new ResultEntry
            {
                Key = string.Join(", ", key) + " / " + offset,
                Text = MapNumbersIntoTextSpace(Decrypt(ciphertext, key, offset, StripList)),
                Value = value,
                Offset = offset
            };

            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    if (_presentation.BestList.Count > 0)
                    {
                        if (value <= _presentation.BestList.Last().Value)
                        {
                            return; //All Entries in Bestlist are better than this one
                        }
                        
                        //no dublicate entries
                        foreach(ResultEntry listEntry in _presentation.BestList)
                        {
                            if (entry.Key.Equals(listEntry.Key))
                            {
                                return;
                            }
                        }

                        if (value > _presentation.BestList.First().Value && beep)
                        {
                            if ((DateTime.Now - _startTime).Seconds > 10)   // skip meaningless highscores in the first 10 seconds
                            {
                                if ((DateTime.Now - _lastbeeptime).Seconds > 1) // only beep once per second for highscore bursts
                                {
                                    SystemSounds.Beep.Play();
                                    _lastbeeptime = DateTime.Now;
                                }
                            }
                        }
                    }
                    //Insert new entry at correct place to sustain order of list:
                    int insertIndex = _presentation.BestList.TakeWhile(e => e.Value > entry.Value).Count();
                    _presentation.BestList.Insert(insertIndex, entry);

                    if (_presentation.BestList.Count > BestListLength)
                    {
                        _presentation.BestList.RemoveAt(BestListLength);
                    }

                    int z = 1;
                    foreach (ResultEntry r in _presentation.BestList)
                    {
                        r.Ranking = z++;
                    }
                    _presentation.CrypAnalysisResultListView.ScrollIntoView(_presentation.CrypAnalysisResultListView.Items[0]);
                }
                catch (Exception)
                {
                    //Console.WriteLine(e.StackTrace);
                }

            }, null);

        }

        private void AddNewBestListEntryKnownPlaintext(string key, double value, int offset)
        {
            ResultEntry entry = new ResultEntry
            {
                Key = key,
                Text = Plaintext,
                Value = value,
                Offset = offset,
            };

            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    _presentation.BestList.Add(entry);
                    int z = 1;
                    foreach (ResultEntry r in _presentation.BestList)
                    {
                        r.Ranking = z++;
                    }
                }
                catch (Exception)
                {
                    //Console.WriteLine(e.StackTrace);
                }

            }, null);

        }

        private void getUserSelections()
        {
            KeyLength = settings.KeyLengthUserSelection;
            MaxOffsetUserSelect = settings.MaxOffsetUserSelection;
            MinOffsetUserSelect = settings.MinOffsetUserSelection;
            Attack = settings.Method;
            fastConverge = settings.FastConverge;
        }

        private string RemoveInvalidChars(string text, string alphabet)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char c in text)
            {
                if (alphabet.Contains(c.ToString()))
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }

        private void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                switch (e.PropertyName)
                {
                    case "Method":
                        settings.UpdateTaskPaneVisibility();
                        break;
                }
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format("Exception during settings_PropertyChanged: {0}", ex), NotificationLevel.Error);
            }
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
        public string Key { get; set; }
        public string Text { get; set; }
        public int Offset { get; set; }

        public double ExactValue => Value;

        public string ClipboardValue => ExactValue.ToString();
        public string ClipboardKey => Key;
        public string ClipboardText => Text;
        public string ClipboardEntry =>
            "Rank: " + Ranking + Environment.NewLine +
            "Value: " + ExactValue + Environment.NewLine +
            "Key: " + Key + Environment.NewLine +
            "Text: " + Text;
    }
}
