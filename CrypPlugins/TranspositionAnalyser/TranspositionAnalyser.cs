/*                              
   Copyright 2022 CrypToolTeam

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
using CrypTool.PluginBase.Control;
using CrypTool.PluginBase.Miscellaneous;
using CrypTool.Plugins.CostFunction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace TranspositionAnalyser
{

    [Author("Daniel Kohnen, Julian Weyers, Simon Malischewski, Armin Wiefels, Nils Kopal", "kohnen@CrypTool.org, weyers@CrypTool.org, malischewski@CrypTool.org, wiefels@CrypTool.org", "Universität Duisburg-Essen", "http://www.uni-due.de")]
    [PluginInfo("TranspositionAnalyser.Properties.Resources", "PluginCaption", "PluginTooltip", "TranspositionAnalyser/DetailedDescription/doc.xml", "TranspositionAnalyser/Images/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class TranspositionAnalyser : ICrypComponent
    {
        private enum ReadInMode
        {
            byRow = 0,
            byColumn = 1
        };
        private enum PermutationMode
        {
            byRow = 0,
            byColumn = 1
        };
        private enum ReadOutMode
        {
            byRow = 0,
            byColumn = 1
        };

        private string _crib;
        private string _input;
        private HighscoreList _highscoreList;
        private ValueKeyComparer _comparer;
        private readonly Random _random = new Random(DateTime.Now.Millisecond);
        private readonly AutoResetEvent _autoResetEvent;
        private readonly TranspositionAnalyserSettings _settings;
        private readonly TranspositionAnalyserQuickWatchPresentation _presentation;
        private string[] _dictionary;

        #region Properties

        [PropertyInfo(Direction.InputData, "InputCaption", "InputTooltip", true)]
        public string Input
        {
            get => _input;

            set
            {
                _input = value;
                OnPropertyChange("Input");
            }
        }

        [PropertyInfo(Direction.InputData, "CribCaption", "CribTooltip", false)]
        public string Crib
        {
            get => _crib;

            set
            {
                _crib = value;
                OnPropertyChange("Crib");
            }
        }

        [PropertyInfo(Direction.InputData, "DictionaryCaption", "DictionaryTooltip", false)]
        public string[] Dictionary
        {
            get => _dictionary;
            set
            {
                _dictionary = value;
                OnPropertyChange("Dictionary");
            }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public TranspositionAnalyser()
        {
            _settings = new TranspositionAnalyserSettings();
            _presentation = new TranspositionAnalyserQuickWatchPresentation();
            Presentation = _presentation;
            _presentation.SelectedResultEntry += SelectedResultEntry;
            _autoResetEvent = new AutoResetEvent(false);
        }

        private void SelectedResultEntry(ResultEntry resultEntry)
        {
            try
            {
                Output = resultEntry.Text;
            }
            catch (Exception)
            {
            }
        }

        private IControlTranspoEncryption controlMaster;
        [PropertyInfo(Direction.ControlMaster, "ControlMasterCaption", "ControlMasterTooltip", false)]
        public IControlTranspoEncryption ControlMaster
        {

            get => controlMaster;
            set
            {
                controlMaster = value;
                OnPropertyChanged("ControlMaster");
            }
        }

        private IControlCost costMaster;
        [PropertyInfo(Direction.ControlMaster, "CostMasterCaption", "CostMasterTooltip", false)]
        public IControlCost CostMaster
        {
            get => costMaster;
            set => costMaster = value;
        }

        private string output;
        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip")]
        public string Output
        {
            get => output;
            set
            {
                output = value;
                OnPropertyChanged("Output");
            }
        }

        public void GuiLogMessage(string message, NotificationLevel loglevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(message, this, loglevel));
            }
        }

        #region IPlugin Member

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public ISettings Settings => _settings;

        public UserControl Presentation
        {
            get;
            private set;
        }

        public void PreExecution()
        {
            _dictionary = null;
            _crib = null;
            _input = null;
        }

        public void Execute()
        {
            if (_input == null)
            {
                GuiLogMessage("No input!", NotificationLevel.Error);
                return;
            }

            if (ControlMaster == null)
            {
                GuiLogMessage("You have to connect the Transposition component to the Transpostion Analyzer control!", NotificationLevel.Error);
                return;
            }

            if (costMaster == null)
            {
                GuiLogMessage("You have to connect the Cost Function component to the Transpostion Analyzer control!", NotificationLevel.Error);
                return;
            }

            if(_settings.MinLength <= 1)
            {
                GuiLogMessage("Minimum keylength has to be 2!", NotificationLevel.Error);
                return;
            }

            if (_settings.MaxLength < _settings.MinLength)
            {
                GuiLogMessage("Maximum keylength has to be greater or equal minimum keylength!", NotificationLevel.Error);
                return;
            }

            _comparer = new ValueKeyComparer(costMaster.GetRelationOperator() != RelationOperator.LargerThen);
            _highscoreList = new HighscoreList(_comparer, 10);

            _presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate { _presentation.Entries.Clear(); }, null);

            switch (_settings.Analysis_method)
            {
                case 0:
                    GuiLogMessage("Starting Brute-Force Analysis", NotificationLevel.Debug);
                    BruteforceAnalysis();
                    break;
                case 1:
                    GuiLogMessage("Starting Crib Analysis", NotificationLevel.Debug);
                    CribAnalysis(_crib, _input);
                    break;
                case 2:
                    GuiLogMessage("Starting Genetic Analysis", NotificationLevel.Debug);
                    GeneticAnalysis();
                    break;
                case 3:
                    GuiLogMessage("Starting Hill Climbing Analysis", NotificationLevel.Debug);
                    HillClimbingAnalysis();
                    break;
                case 4:
                    GuiLogMessage("Starting Dictionary Analysis", NotificationLevel.Debug);
                    DictionaryAnalysis();
                    break;
            }

            ProgressChanged(1, 1);
        }

        public void PostExecution()
        {
        }

        private bool stop;
        public void Stop()
        {
            _autoResetEvent.Set();
            stop = true;
        }

        public void Initialize()
        {
            _settings.UpdateTaskPaneVisibility();
        }

        public void Dispose()
        {
        }

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        private void OnPropertyChange(string propertyname)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(propertyname));
        }

        public void ProgressChanged(double value, double max)
        {
            if (OnPluginProgressChanged != null)
            {
                OnPluginProgressChanged(this, new PluginProgressEventArgs(value, max));
            }
        }

        private void ShowProgress(DateTime startTime, ulong totalKeys, ulong doneKeys, int currentlyAnalyzedKeylength)
        {
            if (!Presentation.IsVisible || stop)
            {
                return;
            }

            long ticksPerSecond = 10000000;

            TimeSpan elapsedtime = DateTime.Now.Subtract(startTime);
            double totalSeconds = elapsedtime.TotalSeconds;
            if (totalSeconds == 0)
            {
                totalSeconds = 0.001;
            }

            elapsedtime = new TimeSpan(elapsedtime.Ticks - (elapsedtime.Ticks % ticksPerSecond));   // truncate to seconds

            TimeSpan timeleft = new TimeSpan();
            DateTime endTime = new DateTime();
            double secstodo;

            bool endsInInfinity = true;
            double keysPerSec = doneKeys / totalSeconds;
            if (keysPerSec > 0)
            {
                endsInInfinity = false;
                if (totalKeys < doneKeys)
                {
                    totalKeys = doneKeys;
                }

                secstodo = (totalKeys - doneKeys) / keysPerSec;
                timeleft = new TimeSpan((long)secstodo * ticksPerSecond);
                try
                {
                    endTime = DateTime.Now.AddSeconds(secstodo);
                    endTime = new DateTime(endTime.Ticks - (endTime.Ticks % ticksPerSecond));   // truncate to seconds
                }
                catch (Exception)
                {
                    endsInInfinity = true;
                }
            }

            ((TranspositionAnalyserQuickWatchPresentation)Presentation).Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                System.Globalization.CultureInfo culture = Thread.CurrentThread.CurrentUICulture;

                _presentation.StartTime.Value = "" + startTime;
                _presentation.ElapsedTime.Value = "" + elapsedtime;
                _presentation.KeysPerSecond.Value = string.Format(culture, "{0:##,#}", (ulong)keysPerSec);
                _presentation.CurrentlyAnalyzedKeylength.Value = currentlyAnalyzedKeylength.ToString();

                if (!endsInInfinity)
                {
                    _presentation.TimeLeft.Value = "" + timeleft;
                    _presentation.EndTime.Value = "" + endTime;
                }
                else
                {
                    _presentation.TimeLeft.Value = "incalculable";
                    _presentation.EndTime.Value = "in a galaxy far, far away...";
                }

                _presentation.Entries.Clear();

                for (int i = 0; i < _highscoreList.Count; i++)
                {
                    ValueKey valueKey = _highscoreList[i];
                    ResultEntry entry = new ResultEntry
                    {
                        Ranking = i + 1,
                        Value = valueKey.score,
                        KeyArray = valueKey.key.ToCharArray()
                    };

                    StringBuilder builder = new StringBuilder();
                    foreach (char c in valueKey.key)
                    {
                        builder.Append(((int)c) + ", ");
                    }

                    if (valueKey.word == null)
                    {
                        entry.Key = "[" + builder.ToString().Substring(0, builder.Length - 2) + "]";
                    }
                    else
                    {
                        entry.Key = valueKey.word + " [" + builder.ToString().Substring(0, builder.Length - 2) + "]";
                    }
                    entry.Mode = valueKey.mode;
                    entry.Text = valueKey.plaintext;

                    _presentation.Entries.Add(entry);
                }
                _presentation.CrypAnalysisResultListView.ScrollIntoView(_presentation.CrypAnalysisResultListView.Items[0]);
            }
            , null);
        }

        private void UpdatePresentationList(ulong totalKeys, ulong doneKeys, DateTime starttime, int currentlyAnalyzedKeylength)
        {
            ShowProgress(starttime, totalKeys, doneKeys, currentlyAnalyzedKeylength);
            ProgressChanged(doneKeys, totalKeys);
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region bruteforce

        private int[] GetBruteforceSettings()
        {
            List<int> set = new List<int>();
            if (_settings.ColumnColumnColumn)
            {
                set.Add(0);
            }
            if (_settings.ColumnColumnRow)
            {
                set.Add(1);
            }
            if (_settings.RowColumnColumn)
            {
                set.Add(2);
            }
            if (_settings.RowColumnRow)
            {
                set.Add(3);
            }
            return (set.Count > 0) ? set.ToArray() : null;
        }

        private void BruteforceAnalysis()
        {
            int[] set = GetBruteforceSettings();

            if (set == null)
            {
                GuiLogMessage("Specify the type of transposition to examine.", NotificationLevel.Error);
                return;
            }

            if (_settings.MaxLength < 2 || _settings.MaxLength > 20)
            {
                GuiLogMessage("Check transposition bruteforce length. Minimum length is 2, maximum length is 20!", NotificationLevel.Error);
                return;
            }

            //backup original settings of transposition component
            int[] oldTranspositionSettings = new int[3];
            oldTranspositionSettings[0] = controlMaster.getSettings("ReadIn");
            oldTranspositionSettings[1] = controlMaster.getSettings("Permute");
            oldTranspositionSettings[2] = controlMaster.getSettings("ReadOut");

            try
            {
                ValueKey valueKey = new ValueKey();

                //Just for fractional-calculation:
                PermutationGenerator permutationGenerator = new PermutationGenerator(2);

                DateTime startTime = DateTime.Now;
                DateTime nextUpdate = DateTime.Now.AddMilliseconds(100);

                ulong totalKeys = 0;
                for (int i = _settings.MinLength; i <= _settings.MaxLength; i++)
                {
                    totalKeys += (ulong)permutationGenerator.getFactorial(i);
                }
                totalKeys *= (ulong)set.Length;

                ulong doneKeys = 0;

                stop = false;

                for (int keylength = _settings.MinLength; keylength <= _settings.MaxLength; keylength++)
                {
                    if (stop)
                    {
                        break;
                    }

                    // for every selected bruteforce mode:
                    for (int s = 0; s < set.Length; s++)
                    {
                        if (stop)
                        {
                            break;
                        }

                        switch (set[s])
                        {
                            case 0:
                                controlMaster.changeSettings("ReadIn", ReadInMode.byColumn);
                                controlMaster.changeSettings("Permute", PermutationMode.byColumn);
                                controlMaster.changeSettings("ReadOut", ReadOutMode.byColumn);
                                break;
                            case 1:
                                controlMaster.changeSettings("ReadIn", ReadInMode.byColumn);
                                controlMaster.changeSettings("Permute", PermutationMode.byColumn);
                                controlMaster.changeSettings("ReadOut", ReadOutMode.byRow);
                                break;
                            case 2:
                                controlMaster.changeSettings("ReadIn", ReadInMode.byRow);
                                controlMaster.changeSettings("Permute", PermutationMode.byColumn);
                                controlMaster.changeSettings("ReadOut", ReadOutMode.byColumn);
                                break;
                            case 3:
                                controlMaster.changeSettings("ReadIn", ReadInMode.byRow);
                                controlMaster.changeSettings("Permute", PermutationMode.byColumn);
                                controlMaster.changeSettings("ReadOut", ReadOutMode.byRow);
                                break;
                        }

                        permutationGenerator = new PermutationGenerator(keylength);

                        while (permutationGenerator.hasMore() && !stop)
                        {
                            int[] keyInt = permutationGenerator.getNext();

                            //Convert numerical key to string
                            StringBuilder keybuilder = new StringBuilder();
                            for (int i = 0; i < keylength; i++)
                            {
                                keybuilder.Append((char)(keyInt[i]));
                            }

                            Decrypt(valueKey, keybuilder.ToString());

                            if (_highscoreList.isBetter(valueKey))
                            {
                                Output = valueKey.plaintext;
                            }

                            _highscoreList.Add(valueKey);
                            doneKeys++;

                            if (DateTime.Now >= nextUpdate)
                            {
                                UpdatePresentationList(totalKeys, doneKeys, startTime, keylength);
                                nextUpdate = DateTime.Now.AddMilliseconds(1000);
                            }
                        }
                    }
                }

                UpdatePresentationList(totalKeys, doneKeys, startTime, _settings.MaxLength);
            }
            finally
            {
                //restore original settings of transposition component
                controlMaster.changeSettings("ReadIn", oldTranspositionSettings[0]);
                controlMaster.changeSettings("Permute", oldTranspositionSettings[1]);
                controlMaster.changeSettings("ReadOut", oldTranspositionSettings[2]);
            }
        }

        #endregion

        #region cribAnalysis

        private ArrayList bestlist;
        private int searchPosition;

        private void CribAnalysis(string crib, string ciphertext)
        {
            bestlist = new ArrayList();

            DateTime starttime = DateTime.Now;
            DateTime nextUpdate = starttime.AddMilliseconds(100);

            int maxKeylength = _settings.CribSearchKeylength;

            if (string.IsNullOrEmpty(crib))
            {
                GuiLogMessage("crib is empty", NotificationLevel.Error);
                return;
            }

            if (string.IsNullOrEmpty(ciphertext))
            {
                GuiLogMessage("ciphertext is empty", NotificationLevel.Error);
                return;
            }

            if (crib.Length < 2)
            {
                GuiLogMessage("Crib is too short.", NotificationLevel.Error);
                return;
            }

            if (maxKeylength <= 1)
            {
                GuiLogMessage("Keylength must be greater than 1", NotificationLevel.Error);
                return;
            }

            if (maxKeylength > crib.Length)
            {
                GuiLogMessage("Crib must be longer than maximum keylength", NotificationLevel.Error);
                return;
            }

            ulong totalKeys = 0;
            for (int i = 2; i <= _settings.CribSearchKeylength; i++)
            {
                totalKeys += (ulong)BinomialIter(i, ciphertext.Length % i);
            }

            ulong doneKeys = 0;

            stop = false;

            for (int keylength = 2; keylength <= maxKeylength; keylength++)
            {
                if (stop)
                {
                    break;
                }

                GuiLogMessage("Keylength: " + keylength, NotificationLevel.Debug);

                int[] binaryKey = GetDefaultBinaryKey(ciphertext, keylength);
                int[] firstKey = (int[])binaryKey.Clone();

                do
                {
                    char[,] cipherMatrix = CipherToMatrix(ciphertext, binaryKey);
                    char[,] cribMatrix = CribToMatrix(crib, keylength);

                    if (PossibleCribForCipher(cipherMatrix, cribMatrix, keylength))
                    {
                        ArrayList possibleList = Analysis(ciphertext, cipherMatrix, cribMatrix, keylength);
                        foreach (int[] k in possibleList)
                        {
                            if (!ContainsList(bestlist, k))
                            {
                                AddToBestList(k);
                            }
                        }
                    }

                    binaryKey = NextPossible(binaryKey, binaryKey.Sum());

                    doneKeys++;

                    if (DateTime.Now >= nextUpdate)
                    {
                        UpdatePresentationList(totalKeys, doneKeys, starttime, keylength);
                        nextUpdate = DateTime.Now.AddMilliseconds(1000);
                    }

                } while (!Equals(firstKey, binaryKey) && !stop);
            }

            UpdatePresentationList(totalKeys, doneKeys, starttime, maxKeylength);
        }

        private bool ContainsList(ArrayList list, int[] search)
        {
            foreach (int[] k in list)
            {
                if (Equals(k, search))
                {
                    return true;
                }
            }
            return false;
        }

        private void AddToBestList(int[] k)
        {
            ValueKey valueKey = new ValueKey();

            int[] first = (int[])k.Clone();

            do
            {
                bestlist.Add((int[])k.Clone());

                int[] keyPlusOne = new int[k.Length];
                for (int i = 0; i < k.Length; i++)
                {
                    keyPlusOne[i] = k[i] + 1;
                }

                StringBuilder keybuilder = new StringBuilder();
                for (int i = 0; i < keyPlusOne.Length; i++)
                {
                    keybuilder.Append((char)(keyPlusOne[i]));
                }

                Decrypt(valueKey, keybuilder.ToString());

                if (_highscoreList.isBetter(valueKey))
                {
                    Output = valueKey.plaintext;
                }

                _highscoreList.Add(valueKey);

                k = ShiftKey(k);

            } while (!Equals(k, first));
        }

        private int[] ShiftKey(int[] key)
        {
            int[] ret = new int[key.Length];
            ret[0] = key[key.Length - 1];
            for (int i = 1; i < ret.Length; i++)
            {
                ret[i] = key[i - 1];
            }

            return ret;
        }

        private ArrayList Analysis(string ciphertext, char[,] cipherMatrix, char[,] cribMatrix, int keylength)
        {
            ArrayList possibleList = new ArrayList();
            int[] key = new int[keylength];
            for (int i = 0; i < key.Length; i++)
            {
                key[i] = -1;
            }

            int keyPosition = 0;
            bool end = false;

            while (!end && !stop)
            {
                bool check = true;
                if (keyPosition == -1)
                {
                    end = true;
                    break;
                }

                if (key[keyPosition] == -1)
                {
                    for (int i = 0; i < key.Length; i++)
                    {
                        bool inUse = false;
                        for (int j = 0; j < keyPosition; j++)
                        {
                            if (i == key[j])
                            {
                                inUse = true;
                            }
                        }

                        if (!inUse)
                        {
                            key[keyPosition] = i;
                            break;
                        }
                    }
                }
                else
                {
                    bool incrementPosition = true;

                    if (keyPosition == 0 && searchPosition != -1)
                    {
                        char[] cipherCol = GetColumn(cipherMatrix, key[keyPosition], key.Length);
                        char[] cribCol = GetColumn(cribMatrix, keyPosition, key.Length);
                        int tmpSearchPosition = searchPosition;
                        searchPosition = -1;

                        if (ContainsAndCheckCribPosition(cipherCol, cribCol, tmpSearchPosition + 1))
                        {
                            keyPosition++;
                            check = false;
                            incrementPosition = false;
                        }
                    }

                    if (incrementPosition)
                    {
                        bool inUse = true;

                        while (inUse)
                        {
                            key[keyPosition] = key[keyPosition] + 1;
                            inUse = false;

                            for (int j = 0; j < keyPosition; j++)
                            {
                                if (key[keyPosition] == key[j])
                                {
                                    inUse = true;
                                }
                            }
                        }

                        if (key[keyPosition] >= key.Length)
                        {
                            key[keyPosition] = -1;
                            keyPosition--;
                            check = false;
                        }
                    }
                }

                if (keyPosition == 0 && key[0] == -1)
                {
                    break;
                }

                if (check)
                {
                    if (keyPosition >= 0 && keyPosition <= key.Length)
                    {
                        char[] cipherCol = GetColumn(cipherMatrix, key[keyPosition], key.Length);
                        char[] cribCol = GetColumn(cribMatrix, keyPosition, key.Length);

                        int startSearchAt = 0;
                        if (searchPosition != -1)
                        {
                            startSearchAt = searchPosition;
                        }

                        if (ContainsAndCheckCribPosition(cipherCol, cribCol, startSearchAt))
                        {
                            keyPosition++;
                        }

                        if (keyPosition == key.Length)
                        {
                            possibleList.Add(key.Clone());

                            keyPosition--;
                            key[keyPosition] = -1;
                            keyPosition--;
                        }

                        if (keyPosition == 0)
                        {
                            searchPosition = -1;
                        }
                    }
                }
            }
            return possibleList;
        }

        private char[] GetColumn(char[,] input, int column, int keylength)
        {
            char[] output = new char[input.Length / keylength];

            for (int i = 0; i < output.Length; i++)
            {
                output[i] = input[column, i];
            }

            return output;
        }

        private bool ContainsAndCheckCribPosition(char[] one, char[] two, int startSearchAt)
        {
            int max = one.Length - 1;

            if (searchPosition != -1)
            {
                max = startSearchAt + 2;
                if (max >= one.Length)
                {
                    max = startSearchAt;
                }
            }

            for (int i = startSearchAt; i <= max; i++)
            {
                if (one[i] == two[0])
                {
                    for (int j = 1; j < two.Length; j++)
                    {
                        if (i + j >= one.Length)
                        {
                            break;
                        }

                        if (two[j].Equals(new byte()))
                        {
                            if (searchPosition == -1)
                            {
                                searchPosition = i;
                            }
                        }
                        else
                        {
                            if (one[i + j] != two[j])
                            {
                                break;
                            }

                            if (j == two.Length - 1 && searchPosition == -1)
                            {
                                searchPosition = i;
                            }
                        }
                        return true;
                    }
                }
            }

            return false;
        }

        private bool Contains(char[] one, char[] two)
        {
            for (int i = 0; i < one.Length; i++)
            {
                if (one[i] == two[0])
                {
                    for (int j = 1; j < two.Length; j++)
                    {
                        if (i + j >= one.Length)
                        {
                            break;
                        }

                        if (two[j].Equals(new byte()))
                        {
                            if (searchPosition == -1)
                            {
                                searchPosition = i;
                            }
                        }
                        else
                        {
                            if (one[i + j] != two[j])
                            {
                                break;
                            }
                        }
                        return true;
                    }
                }
            }

            return false;
        }

        private bool Equals(int[] a, int[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }

        private int[] NextPossible(int[] input, int numberOfOnes)
        {
            do
            {
                input = AddBinOne(input);
            }
            while (Count(input, 1) != numberOfOnes);
            return input;
        }

        private int Count(int[] array, int countThis)
        {
            int c = 0;
            foreach (int i in array)
            {
                if (i == countThis)
                {
                    c++;
                }
            }
            return c;
        }

        private int[] AddBinOne(int[] input)
        {
            int i = input.Length - 1;
            while (i >= 0 && input[i] == 1)
            {
                input[i] = 0;
                i--;
            }
            if (i >= 0)
            {
                input[i] = 1;
            }

            return input;
        }

        private long BinomialIter(int n, int k)
        {
            long produkt = 1;

            if (k > n / 2)
            {
                k = n - k;
            }

            for (int i = 1; i <= k; ++i)
            {
                produkt = produkt * n-- / i;
            }

            return produkt;
        }

        private int[] GetDefaultBinaryKey(string ciphertext, int keylength)
        {
            int[] binaryKey = new int[keylength];
            int offset = (keylength - (ciphertext.Length % keylength)) % keylength;

            for (int i = 0; i < keylength; i++)
            {
                binaryKey[i] = (i < offset) ? 0 : 1;
            }
            return binaryKey;
        }

        private char[,] CipherToMatrix(string ciphertext, int[] key)
        {
            int height = (ciphertext.Length + key.Length - 1) / key.Length;
            char[,] cipherMatrix = new char[key.Length, height];
            int pos = 0;

            for (int a = 0; a < key.Length; a++)
            {
                for (int b = 0; b < height; b++)
                {
                    if ((b == height - 1) && (key[a] != 1))
                    {
                        break;
                    }
                    else
                    {
                        cipherMatrix[a, b] = ciphertext[pos++];
                    }
                }
            }

            return cipherMatrix;
        }

        private char[,] CribToMatrix(string crib, int keylength)
        {
            int height = (crib.Length + keylength - 1) / keylength;
            char[,] cribMatrix = new char[keylength, height];
            int pos = 0;

            for (int b = 0; b < height; b++)
            {
                for (int a = 0; a < keylength; a++)
                {
                    if (pos < crib.Length)
                    {
                        cribMatrix[a, b] = crib[pos++];
                    }
                }
            }

            return cribMatrix;
        }

        private bool PossibleCribForCipher(char[,] ciphertext, char[,] crib, int keylength)
        {
            bool found;
            for (int i = 0; i < keylength; i++)
            {
                char[] cribCol = GetColumn(crib, i, keylength);
                found = false;

                for (int j = 0; j < keylength; j++)
                {
                    char[] cipherCol = GetColumn(ciphertext, j, keylength);

                    if (Contains(cipherCol, cribCol))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    return false;
                }
            }

            return true;
        }     

        #endregion

        #region genetic analysis

        private void GeneticAnalysis()
        {          
            ValueKey valueKey = new ValueKey();

            DateTime startTime = DateTime.Now;
            DateTime nextUpdate = DateTime.Now.AddMilliseconds(100);

            HighscoreList highscoreList = new HighscoreList(_comparer, 12);

            ulong totalKeys = (ulong)_settings.Repeatings * (ulong)_settings.Iterations * 6 * (ulong)(_settings.MaxLength - _settings.MinLength + 1);
            ulong doneKeys = 0;

            stop = false;

            for (int keylength = _settings.MinLength; keylength <= _settings.MaxLength; keylength++) 
            {
                if (stop)
                {
                    break;
                }                

                for (int repeating = 0; repeating < _settings.Repeatings; repeating++)
                {
                    highscoreList.Clear();

                    for (int i = 0; i < highscoreList.Capacity; i++)
                    {
                        highscoreList.Add(CreateKey(RandomArray(keylength)));
                    }

                    for (int iteration = 0; iteration < _settings.Iterations; iteration++)
                    {
                        if (stop)
                        {
                            break;
                        }

                        // create children of best keys
                        int rndInt = 0;
                        int childrenCount = highscoreList.Count < 6 ? highscoreList.Count : 6;

                        for (int a = 0; a < childrenCount - 1; a++)
                        {
                            if (a % 2 == 0)
                            {
                                rndInt = _random.Next(keylength - 1) + 1;
                            }

                            // combine DNA of two parents
                            ValueKey parent1 = highscoreList[a];
                            ValueKey parent2 = highscoreList[(a % 2 == 0) ? a + 1 : a - 1];

                            char[] child = new char[parent1.key.Length];
                            Array.Copy(parent1.key.ToCharArray(), child, rndInt);

                            int pos = rndInt;
                            for (int b = 0; b < parent2.key.Length; b++)
                            {
                                for (int c = rndInt; c < parent1.key.Length; c++)
                                {
                                    if (parent1.key[c] == parent2.key[b])
                                    {
                                        child[pos] = parent1.key[c];
                                        pos++;
                                        break;
                                    }
                                }
                            }

                            // add a single mutation
                            int apos = _random.Next(keylength);
                            int bpos = (apos + _random.Next(1, keylength)) % keylength;
                            Swap(child, apos, bpos);

                            Decrypt(valueKey, new string(child));

                            highscoreList.Add(valueKey);

                            if (_highscoreList.isBetter(valueKey))
                            {
                                _highscoreList.Add(valueKey);
                                Output = valueKey.plaintext;
                            }

                            doneKeys++;
                        }

                        if (DateTime.Now >= nextUpdate)
                        {
                            _highscoreList.Merge(highscoreList);
                            UpdatePresentationList(totalKeys, doneKeys, startTime, keylength);
                            nextUpdate = DateTime.Now.AddMilliseconds(1000);
                        }
                    }
                }

                _highscoreList.Merge(highscoreList);
                UpdatePresentationList(totalKeys, doneKeys, startTime, keylength);
            }
        }

        #endregion

        #region Hill Climbing

        private void HillClimbingAnalysis()
        {           
            DateTime startTime = DateTime.Now;
            DateTime nextUpdate = DateTime.Now.AddMilliseconds(100);

            HighscoreList ROUNDLIST = new HighscoreList(_comparer, 10);

            ValueKey vk = new ValueKey();

            ulong totalKeys = (ulong)_settings.Repeatings * (ulong)_settings.Iterations * (ulong)(_settings.MaxLength - _settings.MinLength + 1);
            ulong doneKeys = 0;

            stop = false;

            for (int keylength = _settings.MinLength; keylength <= _settings.MaxLength; keylength++) 
            {
                if (stop)
                {
                    break;
                }

                for (int repeating = 0; repeating < _settings.Repeatings; repeating++)
                {
                    ROUNDLIST.Clear();

                    char[] key = RandomArray(keylength);
                    char[] oldkey = new char[keylength];

                    for (int iteration = 0; iteration < _settings.Iterations; iteration++)
                    {
                        if (stop)
                        {
                            break;
                        }

                        Array.Copy(key, oldkey, key.Length);

                        int r = _random.Next(100);
                        if (r < 50)
                        {
                            for (int i = 0; i < _random.Next(10); i++)
                            {
                                Swap(key, _random.Next(key.Length), _random.Next(key.Length));
                            }
                        }
                        else if (r < 70)
                        {
                            for (int i = 0; i < _random.Next(3); i++)
                            {
                                int l = _random.Next(key.Length - 1) + 1;
                                int f = _random.Next(key.Length);
                                int t = (f + l + _random.Next(key.Length - l)) % key.Length;
                                Blockswap(key, f, t, l);
                            }
                        }
                        else if (r < 90)
                        {
                            int l = 1 + _random.Next(key.Length - 1);
                            int f = _random.Next(key.Length);
                            int t = (f + 1 + _random.Next(key.Length - 1)) % key.Length;
                            Blockshift(key, f, t, l);
                        }
                        else
                        {
                            Pivot(key, _random.Next(key.Length - 1) + 1);
                        }

                        Decrypt(vk, new string(key));

                        if (ROUNDLIST.Add(vk))
                        {
                            if (_highscoreList.isBetter(vk))
                            {
                                _highscoreList.Add(vk);
                                Output = vk.plaintext;
                            }
                        }
                        else
                        {
                            Array.Copy(oldkey, key, key.Length);
                        }

                        doneKeys++;

                        if (DateTime.Now >= nextUpdate)
                        {
                            _highscoreList.Merge(ROUNDLIST);
                            UpdatePresentationList(totalKeys, doneKeys, startTime, keylength);
                            nextUpdate = DateTime.Now.AddMilliseconds(1000);
                        }
                    }
                }

                _highscoreList.Merge(ROUNDLIST);
                UpdatePresentationList(totalKeys, doneKeys, startTime, _settings.MaxLength);
            }
        }

        #endregion

        private void Swap(char[] arr, int i, int j)
        {
            char tmp = arr[i]; arr[i] = arr[j]; arr[j] = tmp;
        }

        private void Blockswap(char[] arr, int f, int t, int l)
        {
            for (int i = 0; i < l; i++)
            {
                Swap(arr, (f + i) % arr.Length, (t + i) % arr.Length);
            }
        }

        private void Pivot(char[] arr, int p)
        {
            char[] tmp = new char[arr.Length];
            Array.Copy(arr, tmp, arr.Length);

            Array.Copy(tmp, p, arr, 0, arr.Length - p);
            Array.Copy(tmp, 0, arr, arr.Length - p, p);
        }

        private void Blockshift(char[] arr, int f, int t, int l)
        {
            char[] tmp = new char[arr.Length];
            Array.Copy(arr, tmp, arr.Length);

            int t0 = (t - f + arr.Length) % arr.Length;
            int n = (t0 + l) % arr.Length;

            for (int i = 0; i < n; i++)
            {
                int ff = (f + i) % arr.Length;
                int tt = (((t0 + i) % n) + f) % arr.Length;
                arr[tt] = tmp[ff];
            }
        }

        private char[] RandomArray(int length)
        {
            char[] result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = (char)(i + 1);
            }
            for (int i = 0; i < length; i++)
            {
                Swap(result, _random.Next(length), _random.Next(length));
            }
            return result;
        }

        private void Decrypt(ValueKey valueKey, string key, string word = null)
        {
            if (costMaster is CostFunctionControl control)
            {
                //we normalize our ngrams between 0 and 10_000_000 with all analyzers for classical ciphers
                //calling the normalize ngram method will normalize it; if called again and normalization was already performed, nothing happens
                ((CostFunctionControl)costMaster).NormalizeNGrams();
            }

            valueKey.key = key;
            valueKey.word = word;
            valueKey.plaintext = controlMaster.Decrypt(_input, valueKey.key);
            valueKey.score = costMaster.CalculateCost(Encoding.UTF8.GetBytes(valueKey.plaintext));
            valueKey.mode = (((ReadInMode)controlMaster.getSettings("ReadIn") == ReadInMode.byColumn) ? Properties.Resources.CharacterForColumn : Properties.Resources.CharacterForRow) + "-"
                    + (((PermutationMode)controlMaster.getSettings("Permute") == PermutationMode.byColumn) ? Properties.Resources.CharacterForColumn : Properties.Resources.CharacterForRow) + "-"
                    + (((ReadOutMode)controlMaster.getSettings("ReadOut") == ReadOutMode.byColumn) ? Properties.Resources.CharacterForColumn : Properties.Resources.CharacterForRow);
        }

        private ValueKey CreateKey(char[] key)
        {
            ValueKey result = new ValueKey();
            Decrypt(result, new string((char[])key.Clone()));
            return result;
        }

        private void DictionaryAnalysis()
        {
            if (_dictionary == null)
            {
                GuiLogMessage("Please attach dictionary to component", NotificationLevel.Error);
                return;
            }

            List<string> words = new List<string>();
            foreach (string word in _dictionary)
            {
                if (word.Length >= _settings.MinLength && word.Length <= _settings.MaxLength)
                {
                    if (_settings.CaseSensitive)
                    {
                        words.Add(word);
                    }
                    else
                    {
                        words.Add(word.ToUpper());
                    }
                }
            }

            DateTime startTime = DateTime.Now;
            DateTime nextUpdate = DateTime.Now.AddMilliseconds(100);

            int[] set = GetBruteforceSettings();

            ulong totalKeys = (ulong)(words.Count * set.Length);
            ulong doneKeys = 0;

            stop = false;

            for (int index = 0; index < words.Count; index++)
            {
                for (int s = 0; s < set.Length; s++)
                {
                    if (stop)
                    {
                        break;
                    }

                    switch (set[s])
                    {
                        case 0:
                            controlMaster.changeSettings("ReadIn", ReadInMode.byColumn);
                            controlMaster.changeSettings("Permute", PermutationMode.byColumn);
                            controlMaster.changeSettings("ReadOut", ReadOutMode.byColumn);
                            break;
                        case 1:
                            controlMaster.changeSettings("ReadIn", ReadInMode.byColumn);
                            controlMaster.changeSettings("Permute", PermutationMode.byColumn);
                            controlMaster.changeSettings("ReadOut", ReadOutMode.byRow);
                            break;
                        case 2:
                            controlMaster.changeSettings("ReadIn", ReadInMode.byRow);
                            controlMaster.changeSettings("Permute", PermutationMode.byColumn);
                            controlMaster.changeSettings("ReadOut", ReadOutMode.byColumn);
                            break;
                        case 3:
                            controlMaster.changeSettings("ReadIn", ReadInMode.byRow);
                            controlMaster.changeSettings("Permute", PermutationMode.byColumn);
                            controlMaster.changeSettings("ReadOut", ReadOutMode.byRow);
                            break;
                    }

                    char[] keyFromWord = GenerateKeyFromWord(words[index]);

                    ValueKey valueKey = new ValueKey();
                    Decrypt(valueKey, new string(keyFromWord), words[index]);

                    if (_highscoreList.isBetter(valueKey))
                    {
                        _highscoreList.Add(valueKey);
                        Output = valueKey.plaintext;
                    }

                    doneKeys++;

                    if (DateTime.Now >= nextUpdate)
                    {
                        UpdatePresentationList(totalKeys, doneKeys, startTime, _settings.MaxLength);
                        nextUpdate = DateTime.Now.AddMilliseconds(1000);
                    }
                }
            }
            UpdatePresentationList(totalKeys, doneKeys, startTime, _settings.MaxLength);
        }

        private char[] GenerateKeyFromWord(string word)
        {
            char[] key = new char[word.Length];
            for (int i = 0; i < word.Length; i++)
            {
                key[i] = char.MaxValue;
            }
            for (int i = 0; i < word.Length; i++)
            {
                int smallestIndex = -1;
                int smallestValue = int.MaxValue;

                for (int j = 0; j < word.Length; j++)
                {
                    if (key[j] == byte.MaxValue && word[j] < smallestValue)
                    {
                        smallestValue = word[j];
                        smallestIndex = j;
                    }
                }
                key[smallestIndex] = (char)(i + 1);
            }
            return key;
        }
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
        public int KeyLength
        {
            get
            {
                return KeyArray != null ? KeyArray.Length : 0;
            }
        }
        public char[] KeyArray { get; set; }
        public string Mode { get; set; }
        public string Text { get; set; }

        public string ClipboardValue => DisplayValue;
        public string ClipboardKey => Key;
        public string ClipboardText => Text;

        public string ClipboardEntry
        {
            get
            {
                string keyword = GetKeyword(KeyArray);

                string key = string.IsNullOrEmpty(keyword)
                    ? "Key: " + Key
                    : "Key (numeric): " + Key + Environment.NewLine + "Key (alphabetic): " + keyword;

                return "Rank: " + Ranking + Environment.NewLine +
                    "Value: " + Value + Environment.NewLine +
                    key + Environment.NewLine +
                    "Mode: " + Mode + Environment.NewLine +
                    "Text: " + Text;
            }
        }

        // Alphabets used for converting the numeric key to a key word
        private const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"; // for key sizes <= 52
        private const string alphabet2 = "0123456789" + alphabet;                               // for key sizes <= 62 (use numbers only if letters do not suffice)

        // Convert the numeric key to a keyword based upon the alphabet string
        private string GetKeyword(char[] key)
        {
            if (key.Length >= alphabet2.Length)
            {
                return null;
            }
            string abc = (key.Length <= alphabet.Length) ? alphabet : alphabet2;
            string keyword = String.Empty;
            foreach (char i in key)
            {
                keyword += abc[i];
            }
            return keyword;
        }
    }
}
