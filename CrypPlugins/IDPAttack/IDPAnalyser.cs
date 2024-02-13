/*
   Copyright 2023 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace IDPAnalyser
{
    [Author("George Lasry, Armin Krauß", "krauss@CrypTool.org", "CrypTool", "http://www.uni-due.de")]
    [PluginInfo("IDPAnalyser.Properties.Resources", "PluginCaption", "PluginTooltip", "IDPAnalyser/DetailedDescription/doc.xml", "IDPAnalyser/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisSpecific)]
    public class IDPAnalyser : ICrypComponent
    {
        private string _input;
        private string _output;
        private string[] _keywords;
        private HighscoreList _highscoreList;
        private ValueKeyComparer _valueKeyComparer;
        private readonly Random _random = new Random();
        private readonly AutoResetEvent _autoResetEvent;
        private readonly IDPAnalyserSettings _settings;
        private readonly IDPAnalyserQuickWatchPresentation _presentation;
        private int[] _key1MinColEnd, _key1MaxColEnd;
        private Grams _grams = null;

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

        [PropertyInfo(Direction.InputData, "KeywordsCaption", "KeywordsTooltip", false)]
        public string[] Keywords
        {
            get => _keywords;

            set
            {
                _keywords = value;
                OnPropertyChange("Keywords");
            }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public IDPAnalyser()
        {
            _settings = new IDPAnalyserSettings();
            _presentation = new IDPAnalyserQuickWatchPresentation();
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

        [PropertyInfo(Direction.OutputData, "OutputCaption", "OutputTooltip")]
        public string Output
        {
            get => _output;
            set
            {
                _output = value;
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

        public event CrypTool.PluginBase.StatusChangedEventHandler OnPluginStatusChanged;

        public event CrypTool.PluginBase.GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event CrypTool.PluginBase.PluginProgressChangedEventHandler OnPluginProgressChanged;

        public ISettings Settings => _settings;

        public UserControl Presentation
        {
            get;
            private set;
        }

        public void PreExecution()
        {
        }

        public void Execute()
        {
            _grams = LanguageStatistics.CreateGrams(_settings.Language, DirectoryHelper.DirectoryLanguageStatistics, (GramsType)2, false);
            _grams.Normalize(10_000_000);

            if (_input == null)
            {
                GuiLogMessage("No input!", NotificationLevel.Error);
                return;
            }

            _valueKeyComparer = new ValueKeyComparer(false);
            _highscoreList = new HighscoreList(_valueKeyComparer, 10);

            _presentation.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate { _presentation.Entries.Clear(); }, null);

            switch (_settings.Analysis_method)
            {
                case 0: GuiLogMessage("Starting Dictionary Attack", NotificationLevel.Info); DictionaryAttack(); break;
                case 1: GuiLogMessage("Starting Hill Climbing Analysis", NotificationLevel.Info); HillClimbingAnalysis(); break;
            }

            if (_highscoreList.Count > 0)
            {
                Output = MapNumbersIntoTextSpace(_highscoreList[0].plaintext, LATIN_ALPHABET);
            }
            else
            {
                GuiLogMessage("No candidates found", NotificationLevel.Warning);
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

        private void ShowProgress(DateTime startTime, ulong totalKeys, ulong doneKeys)
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

            double keysPerSec = doneKeys / totalSeconds;
            if (keysPerSec > 0)
            {
                if (totalKeys < doneKeys)
                {
                    totalKeys = doneKeys;
                }

                secstodo = (totalKeys - doneKeys) / keysPerSec;
                timeleft = new TimeSpan((long)secstodo * ticksPerSecond);
                endTime = DateTime.Now.AddSeconds(secstodo);
                endTime = new DateTime(endTime.Ticks - (endTime.Ticks % ticksPerSecond));   // truncate to seconds
            }

            ((IDPAnalyserQuickWatchPresentation)Presentation).Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                System.Globalization.CultureInfo culture = System.Threading.Thread.CurrentThread.CurrentUICulture;

                _presentation.StartTime.Value = string.Empty + startTime;
                _presentation.KeysPerSecond.Value = string.Format(culture, "{0:##,#}", (ulong)keysPerSec);

                if (keysPerSec > 0)
                {
                    _presentation.TimeLeft.Value = string.Empty + timeleft;
                    _presentation.ElapsedTime.Value = string.Empty + elapsedtime;
                    _presentation.EndTime.Value = string.Empty + endTime;
                }
                else
                {
                    _presentation.TimeLeft.Value = "incalculable";
                    _presentation.EndTime.Value = "in a galaxy far, far away...";
                }

                _presentation.Entries.Clear();

                for (int i = 0; i < _highscoreList.Count; i++)
                {
                    ValueKey v = _highscoreList[i];
                    ResultEntry entry = new ResultEntry
                    {
                        Ranking = i + 1,
                        Value = v.score,
                        KeyArray = v.key,
                        KeyPhrase = v.keyphrase,
                        Key = "[" + string.Join(",", v.key) + "]",
                        Text = MapNumbersIntoTextSpace(v.plaintext, LATIN_ALPHABET)
                    };

                    _presentation.Entries.Add(entry);
                }
            }
            , null);
        }

        private void UpdatePresentationList(ulong totalKeys, ulong doneKeys, DateTime starttime)
        {
            ShowProgress(starttime, totalKeys, doneKeys);
            ProgressChanged(doneKeys, totalKeys);
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region DictionaryAttack

        public static void getKeyFromKeyword(string phrase, byte[] key, int keylen)
        {
            for (int i = 0; i < keylen; i++)
            {
                key[i] = 0xff;
            }

            for (int i = 0; i < keylen; i++)
            {
                int minJ = -1;

                for (int j = 0; j < keylen; j++)
                {
                    if (key[j] != 0xff)
                    {
                        continue;
                    }

                    if ((minJ == -1) || (phrase[j] < phrase[minJ]))
                    {
                        minJ = j;
                    }
                }

                key[minJ] = (byte)i;
            }
        }

        private const string LATIN_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";   // used for converting the numeric key to a keyword

        // Convert the numeric key to a keyword based upon the alphabet string
        private string getKeywordFromKey(byte[] key)
        {
            string keyword = string.Empty;
            foreach (byte i in key)
            {
                keyword += alphabet[i];
            }

            return keyword;
        }

        private void DictionaryAttack()
        {
            if (Keywords == null || Keywords.Length == 0)
            {
                GuiLogMessage("Check dictionary", NotificationLevel.Error);
                return;
            }

            if (_settings.Key1Min < 2)
            {
                GuiLogMessage("The minimum size for key 1 is 2.", NotificationLevel.Error);
                return;
            }

            if (_settings.Key1Max < _settings.Key1Min)
            {
                GuiLogMessage("The maximum size for key 1 must be bigger than the minimum size.", NotificationLevel.Error);
                return;
            }

            if (_settings.Key2Min < 2)
            {
                GuiLogMessage("The minimum size for key 2 is 2.", NotificationLevel.Error);
                return;
            }

            if (_settings.Key2Max < _settings.Key2Min)
            {
                GuiLogMessage("The maximum size for key 2 must be bigger than the minimum size.", NotificationLevel.Error);
                return;
            }

            DateTime startTime = DateTime.Now;
            DateTime nextUpdate = DateTime.Now.AddMilliseconds(100);

            ValueKey vk = new ValueKey();

            ulong totalKeys = 0;
            foreach (string keyword in Keywords)
            {
                if (keyword.Length >= _settings.Key2Min && keyword.Length <= _settings.Key2Max)
                {
                    totalKeys++;
                }
            }

            totalKeys *= (ulong)(_settings.Key1Max - _settings.Key1Min + 1);
            ulong doneKeys = 0;

            stop = false;

            int[] mybuffer = new int[_input.Length];

            //byte[] ciphertext = UTF8Encoding.UTF8.GetBytes(_input);
            int[] ciphertext = MapTextIntoNumberSpace(_input, LATIN_ALPHABET);

            for (int key1size = _settings.Key1Min; key1size <= _settings.Key1Max; key1size++)
            {
                ComputeKey1MinMaxColEnding(_input.Length, key1size);

                for (int key2size = _settings.Key2Min; key2size <= _settings.Key2Max; key2size++)
                {
                    byte[] key2 = new byte[key2size];

                    foreach (string keyword in Keywords)
                    {
                        if (stop)
                        {
                            break;
                        }

                        if (keyword.Length != key2size)
                        {
                            continue;
                        }

                        getKeyFromKeyword(keyword, key2, key2size);

                        //decrypt(vk, key);
                        vk.key = key2;
                        vk.keyphrase = keyword;
                        Decrypt2(vk.key, vk.key.Length, ciphertext, _input.Length, mybuffer);
                        vk.plaintext = mybuffer;
                        vk.score = EvalIDPKey2(vk.plaintext, key1size);

                        _highscoreList.Add(vk);

                        doneKeys++;

                        if (DateTime.Now >= nextUpdate)
                        {
                            UpdatePresentationList(totalKeys, doneKeys, startTime);
                            nextUpdate = DateTime.Now.AddMilliseconds(1000);
                        }
                    }

                    UpdatePresentationList(totalKeys, doneKeys, startTime);
                }
            }
        }

        #endregion

        #region Hill Climbing

        private void HillClimbingAnalysis()
        {
            if (_settings.Iterations < 2)
            {
                GuiLogMessage("Check iterations.", NotificationLevel.Error);
                return;
            }

            DateTime startTime = DateTime.Now;
            DateTime nextUpdate = DateTime.Now.AddMilliseconds(100);

            ulong keycombinations = (ulong)((_settings.Key1Max - _settings.Key1Min + 1) * (_settings.Key2Max - _settings.Key2Min + 1));
            ulong totalKeys = (ulong)_settings.Repeatings * (ulong)_settings.Iterations * keycombinations;
            ulong doneKeys = 0;
            HighscoreList ROUNDLIST = new HighscoreList(_valueKeyComparer, 10);
            stop = false;

            for (int key1size = _settings.Key1Min; key1size <= _settings.Key1Max; key1size++)
            {
                ComputeKey1MinMaxColEnding(_input.Length, key1size);

                for (int key2size = _settings.Key2Min; key2size <= _settings.Key2Max; key2size++)
                {

                    ComputeKey1MinMaxColEnding(_input.Length, key1size);
                    int[] mybuffer = new int[_input.Length];
                    ValueKey vk = new ValueKey();
                    int[] ciphertext = MapTextIntoNumberSpace(_input, LATIN_ALPHABET);

                    for (int repeating = 0; repeating < _settings.Repeatings; repeating++)
                    {
                        if (stop)
                        {
                            break;
                        }

                        ROUNDLIST.Clear();

                        byte[] key = RandomArray(key2size);
                        byte[] oldkey = new byte[key2size];

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
                                    BlockSwap(key, f, t, l);
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

                            vk.key = key;
                            Decrypt2(vk.key, vk.key.Length, ciphertext, _input.Length, mybuffer);
                            vk.plaintext = mybuffer;
                            vk.score = EvalIDPKey2(vk.plaintext, key1size);
                            vk.keyphrase = getKeywordFromKey(vk.key);

                            if (ROUNDLIST.Add(vk))
                            {
                                if (_highscoreList.isBetter(vk))
                                {
                                    _highscoreList.Add(vk);
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
                                UpdatePresentationList(totalKeys, doneKeys, startTime);
                                nextUpdate = DateTime.Now.AddMilliseconds(1000);
                            }
                        }
                    }
                    _highscoreList.Merge(ROUNDLIST);
                    UpdatePresentationList(totalKeys, doneKeys, startTime);
                }
            }
        }

        private void ComputeKey1MinMaxColEnding(int ciphertextLength, int keylength)
        {
            _key1MinColEnd = new int[keylength];
            _key1MaxColEnd = new int[keylength];

            int fullRows = ciphertextLength / keylength;
            int numberOfLongColumns = ciphertextLength % keylength;

            for (int i = 0; i < keylength; i++)
            {
                _key1MinColEnd[i] = fullRows * (i + 1) - 1;
                if (i < numberOfLongColumns)
                {
                    _key1MaxColEnd[i] = fullRows * (i + 1) + i;
                }
                else
                {
                    _key1MaxColEnd[i] = _key1MinColEnd[i] + numberOfLongColumns;
                }
            }

            for (int i = 0; i < keylength; i++)
            {
                int index = keylength - 1 - i;
                _key1MaxColEnd[index] = Math.Min(_key1MaxColEnd[index], ciphertextLength - 1 - fullRows * i);
                if (i < numberOfLongColumns)
                {
                    _key1MinColEnd[index] = Math.Max(_key1MinColEnd[index], ciphertextLength - 1 - fullRows * i - i);
                }
                else
                {
                    _key1MinColEnd[index] = Math.Max(_key1MinColEnd[index], _key1MaxColEnd[index] - numberOfLongColumns);
                }
            }
        }

        /// <summary>
        /// The core algorithm for IDP (index of Digraphic Potential)
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="keylen"></param>
        /// <returns></returns>
        public double EvalIDPKey2(int[] ciphertext, int keylen)
        {
            double[,] p1p2Best = new double[keylen, keylen];

            // CCT: All columns always start at multiples of nrows. No need to sweep for different
            // ending positions
            if ((ciphertext.Length % keylen) == 0)
            {
                int fullRows = ciphertext.Length / keylen;

                for (int c1 = 0; c1 < keylen; c1++)
                {
                    for (int c2 = 0; c2 < keylen; c2++)
                    {
                        if (c1 == c2)
                        {
                            continue;
                        }

                        int p1 = _key1MinColEnd[c1];
                        int p2 = _key1MinColEnd[c2];

                        double sum = 0;

                        for (int l = 0; l < fullRows; l++)
                        {
                            sum += ((Bigrams)_grams).Frequencies[ciphertext[p1 - l], ciphertext[p2 - l]];
                        }

                        p1p2Best[c1, c2] = sum / fullRows;
                    }
                }
            }
            else if ((ciphertext.Length % keylen) != 0)
            {
                // ICT - we sweep all possible C1-C2-C3 combinations as well
                // as all posible ending positions (P1, P2, P3).

                int fullRows = ciphertext.Length / keylen;
                double[] base_ = new double[fullRows + keylen];

                for (int c1 = 0; c1 < keylen; c1++)
                {
                    int minP1 = _key1MinColEnd[c1];
                    int maxP1 = _key1MaxColEnd[c1];

                    for (int c2 = 0; c2 < keylen; c2++)
                    {
                        if (c1 == c2)
                        {
                            continue;
                        }

                        int minP2 = _key1MinColEnd[c2];
                        int maxP2 = _key1MaxColEnd[c2];

                        int offset1 = maxP1 - minP1;
                        int offset2 = maxP2 - minP2;

                        int start1 = minP1 - fullRows + 1;
                        int start2 = minP2 - fullRows + 1;
                        double best = 0;

                        for (int offset = 0; offset <= offset2; offset++)
                        {
                            double sum = 0;

                            int p1 = start1;
                            int p2 = start2 + offset;

                            for (int i = 0; i < fullRows; i++)
                            {
                                double val = base_[i] = ((Bigrams)_grams).Frequencies[ciphertext[p1++], ciphertext[p2++]];
                                sum += val;
                            }

                            if (best < sum)
                            {
                                best = sum;
                            }

                            int iMinusFullRows = 0;
                            while ((p1 <= maxP1) && (p2 <= maxP2))
                            {
                                sum -= base_[iMinusFullRows++];
                                sum += ((Bigrams)_grams).Frequencies[ciphertext[p1++], ciphertext[p2++]];
                                if (best < sum)
                                {
                                    best = sum;
                                }
                            }
                        }

                        // we test only once with offset = 0;
                        for (int offset = 1; offset <= offset1; offset++)
                        {
                            double sum = 0;

                            int p1 = start1 + offset;
                            int p2 = start2;

                            for (int i = 0; i < fullRows; i++)
                            {
                                double val = base_[i] = ((Bigrams)_grams).Frequencies[ciphertext[p1++], ciphertext[p2++]];
                                sum += val;
                            }

                            if (best < sum)
                            {
                                best = sum;
                            }

                            int iMinusFullRows = 0;
                            while ((p1 <= maxP1) && (p2 <= maxP2))
                            {
                                sum -= base_[iMinusFullRows++];
                                sum += ((Bigrams)_grams).Frequencies[ciphertext[p1++], ciphertext[p2++]];
                                if (best < sum)
                                {
                                    best = sum;
                                }
                            }
                        }

                        p1p2Best[c1, c2] = best / fullRows;
                    }
                }
            }

            return GetMatrixScore(p1p2Best);
        }

        private static double GetMatrixScore(double[,] matrix)
        {
            int dimension = matrix.GetLength(0);

            int[] left = new int[dimension];
            int[] right = new int[dimension];

            for (int i = 0; i < dimension; i++)
            {
                left[i] = right[i] = -1;
            }

            double sum = 0;

            for (int i = 1; i <= dimension; i++)
            {
                double best = 0;
                int bestP1 = -1;
                int bestP2 = -1;

                for (int p1 = 0; p1 < dimension; p1++)
                {
                    if (right[p1] != -1)
                    {
                        continue;
                    }

                    bool[] inP1LeftCycle = new bool[dimension];

                    if (i != dimension)
                    {
                        int curr = p1;
                        while (left[curr] != -1)
                        {
                            curr = left[curr];
                            inP1LeftCycle[curr] = true;
                        }
                    }

                    for (int p2 = 0; p2 < dimension; p2++)
                    {
                        if (left[p2] != -1)
                        {
                            continue;
                        }

                        if (inP1LeftCycle[p2])
                        {
                            continue;
                        }

                        if (p1 == p2)
                        {
                            continue;
                        }

                        if (best < matrix[p1, p2])
                        {
                            best = matrix[p1, p2];
                            bestP1 = p1;
                            bestP2 = p2;
                        }
                    }
                }

                sum += best;
                if (bestP1 != -1)
                {
                    left[bestP2] = bestP1;
                    right[bestP1] = bestP2;
                }
            }

            return sum / dimension;
        }

        public static void Decrypt2(byte[] key, int keylen, int[] ciphertext, int ciphertextLength, int[] plaintext)
        {
            int[] invkey = new int[keylen];

            for (int i = 0; i < keylen; i++)
            {
                invkey[key[i]] = i;
            }

            int c = 0;
            for (int trcol = 0; trcol < keylen; trcol++)
            {
                for (int p = invkey[trcol]; p < ciphertextLength; p += keylen)
                {
                    plaintext[p] = ciphertext[c++];
                }
            }
        }

        #endregion

        private void Swap(byte[] arr, int i, int j)
        {
            byte tmp = arr[i]; arr[i] = arr[j]; arr[j] = tmp;
        }

        private void BlockSwap(byte[] arr, int f, int t, int l)
        {
            for (int i = 0; i < l; i++)
            {
                Swap(arr, (f + i) % arr.Length, (t + i) % arr.Length);
            }
        }

        private void Pivot(byte[] arr, int p)
        {
            byte[] tmp = new byte[arr.Length];
            Array.Copy(arr, tmp, arr.Length);
            Array.Copy(tmp, p, arr, 0, arr.Length - p);
            Array.Copy(tmp, 0, arr, arr.Length - p, p);
        }

        private void Blockshift(byte[] arr, int f, int t, int l)
        {
            byte[] tmp = new byte[arr.Length];
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

        private byte[] RandomArray(int length)
        {
            byte[] result = new byte[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = (byte)i;
            }

            for (int i = 0; i < length; i++)
            {
                Swap(result, _random.Next(length), _random.Next(length));
            }

            return result;
        }

        /// <summary>
        /// Maps a given array of numbers into the "textspace" defined by the alphabet
        /// </summary>
        /// <param name="numbers"></param>
        /// <param name="alphabet"></param>
        /// <returns></returns>
        public static string MapNumbersIntoTextSpace(int[] numbers, string alphabet)
        {
            StringBuilder builder = new StringBuilder();
            foreach (int i in numbers)
            {
                builder.Append(alphabet[i]);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Maps a given string into the "numberspace" defined by the alphabet
        /// </summary>
        /// <param name="text"></param>
        /// <param name="alphabet"></param>
        /// <returns></returns>
        public static int[] MapTextIntoNumberSpace(string text, string alphabet)
        {
            int[] numbers = new int[text.Length];
            int position = 0;
            foreach (char c in text)
            {
                numbers[position] = alphabet.IndexOf(c);
                position++;
            }
            return numbers;
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
        public string KeyPhrase { get; set; }
        public byte[] KeyArray { get; set; }
        public string Text { get; set; }

        public string ClipboardValue => DisplayValue;
        public string ClipboardKey => Key;
        public string ClipboardText => Text;
        public string ClipboardEntry =>
            "Rank: " + Ranking + Environment.NewLine +
            "Value: " + Value + Environment.NewLine +
            "Key (numeric): " + string.Join(" ", KeyArray) + Environment.NewLine +
            "Key (alphabetic): " + KeyPhrase + Environment.NewLine +
            "Text: " + Text;
    }
}
