using CrypTool.CrypAnalysisViewControl;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
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
        private readonly Random _random = new Random(DateTime.Now.Millisecond);
        private readonly AutoResetEvent _autoResetEvent;
        private readonly IDPAnalyserSettings _settings;
        private readonly IDPAnalyserQuickWatchPresentation _presentation;
        private int[] Key1MinColEnd, Key1MaxColEnd;

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
            Bigrams.InitLanguage(_settings.Language);

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
                Output = UTF8Encoding.UTF8.GetString(_highscoreList[0].plaintext);
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

        private void showProgress(DateTime startTime, ulong totalKeys, ulong doneKeys)
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

                _presentation.StartTime.Value = "" + startTime;
                _presentation.KeysPerSecond.Value = string.Format(culture, "{0:##,#}", (ulong)keysPerSec);

                if (keysPerSec > 0)
                {
                    _presentation.TimeLeft.Value = "" + timeleft;
                    _presentation.ElapsedTime.Value = "" + elapsedtime;
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
                    ValueKey v = _highscoreList[i];
                    ResultEntry entry = new ResultEntry
                    {
                        Ranking = i + 1,
                        Value = string.Format("{0:0.00000}", v.score),
                        KeyArray = v.key,
                        KeyPhrase = v.keyphrase,
                        Key = "[" + string.Join(",", v.key) + "]",
                        Text = Encoding.GetEncoding(1252).GetString(v.plaintext)
                    };

                    _presentation.Entries.Add(entry);
                }
            }
            , null);
        }

        private void UpdatePresentationList(ulong totalKeys, ulong doneKeys, DateTime starttime)
        {
            showProgress(starttime, totalKeys, doneKeys);
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

        private const string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";   // used for converting the numeric key to a keyword

        // Convert the numeric key to a keyword based upon the alphabet string
        private string getKeywordFromKey(byte[] key)
        {
            string keyword = "";
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

            byte[] mybuffer = new byte[_input.Length];

            byte[] ciphertext = UTF8Encoding.UTF8.GetBytes(_input);

            for (int key1size = _settings.Key1Min; key1size <= _settings.Key1Max; key1size++)
            {
                computeKey1MinMaxColEnding(_input.Length, key1size);

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
                        decrypt2(vk.key, vk.key.Length, ciphertext, _input.Length, mybuffer);
                        vk.plaintext = mybuffer;
                        vk.score = evalIDPKey2(vk.plaintext, key1size);

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

            if (_settings.Key1Size < 2)
            {
                GuiLogMessage("The minimum size for key 1 is 2.", NotificationLevel.Error);
                return;
            }

            if (_settings.Key2Size < 2)
            {
                GuiLogMessage("The minimum size for key 2 is 2.", NotificationLevel.Error);
                return;
            }

            computeKey1MinMaxColEnding(_input.Length, _settings.Key1Size);
            byte[] mybuffer = new byte[_input.Length];

            DateTime startTime = DateTime.Now;
            DateTime nextUpdate = DateTime.Now.AddMilliseconds(100);

            HighscoreList ROUNDLIST = new HighscoreList(_valueKeyComparer, 10);

            ValueKey vk = new ValueKey();

            ulong totalKeys = (ulong)_settings.Repeatings * (ulong)_settings.Iterations;
            ulong doneKeys = 0;

            stop = false;

            byte[] ciphertext = UTF8Encoding.UTF8.GetBytes(_input);

            for (int repeating = 0; repeating < _settings.Repeatings; repeating++)
            {
                if (stop)
                {
                    break;
                }

                ROUNDLIST.Clear();

                byte[] key = randomArray(_settings.Key2Size);
                byte[] oldkey = new byte[_settings.Key2Size];

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
                            swap(key, _random.Next(key.Length), _random.Next(key.Length));
                        }
                    }
                    else if (r < 70)
                    {
                        for (int i = 0; i < _random.Next(3); i++)
                        {
                            int l = _random.Next(key.Length - 1) + 1;
                            int f = _random.Next(key.Length);
                            int t = (f + l + _random.Next(key.Length - l)) % key.Length;
                            blockswap(key, f, t, l);
                        }
                    }
                    else if (r < 90)
                    {
                        int l = 1 + _random.Next(key.Length - 1);
                        int f = _random.Next(key.Length);
                        int t = (f + 1 + _random.Next(key.Length - 1)) % key.Length;
                        blockshift(key, f, t, l);
                    }
                    else
                    {
                        pivot(key, _random.Next(key.Length - 1) + 1);
                    }

                    vk.key = key;
                    decrypt2(vk.key, vk.key.Length, ciphertext, _input.Length, mybuffer);
                    vk.plaintext = mybuffer;
                    vk.score = evalIDPKey2(vk.plaintext, _settings.Key1Size);
                    vk.keyphrase = getKeywordFromKey(vk.key);

                    if (ROUNDLIST.Add(vk))
                    {
                        if (_highscoreList.isBetter(vk))
                        {
                            _highscoreList.Add(vk);
                            //Output = vk.plaintext;
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

        private void computeKey1MinMaxColEnding(int ciphertextLength, int keylength)
        {
            Key1MinColEnd = new int[keylength];
            Key1MaxColEnd = new int[keylength];

            int fullRows = ciphertextLength / keylength;
            int numberOfLongColumns = ciphertextLength % keylength;

            for (int i = 0; i < keylength; i++)
            {
                Key1MinColEnd[i] = fullRows * (i + 1) - 1;
                if (i < numberOfLongColumns)
                {
                    Key1MaxColEnd[i] = fullRows * (i + 1) + i;
                }
                else
                {
                    Key1MaxColEnd[i] = Key1MinColEnd[i] + numberOfLongColumns;
                }
            }

            for (int i = 0; i < keylength; i++)
            {
                int index = keylength - 1 - i;
                Key1MaxColEnd[index] = Math.Min(Key1MaxColEnd[index], ciphertextLength - 1 - fullRows * i);
                if (i < numberOfLongColumns)
                {
                    Key1MinColEnd[index] = Math.Max(Key1MinColEnd[index], ciphertextLength - 1 - fullRows * i - i);
                }
                else
                {
                    Key1MinColEnd[index] = Math.Max(Key1MinColEnd[index], Key1MaxColEnd[index] - numberOfLongColumns);
                }
            }
        }

        // The core algorithm for IDP (index of Digraphic Potential)
        public long evalIDPKey2(byte[] ciphertext, int keylen)
        {
            long[,] p1p2Best = new long[keylen, keylen];

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

                        int p1 = Key1MinColEnd[c1];
                        int p2 = Key1MinColEnd[c2];

                        long sum = 0;

                        for (int l = 0; l < fullRows; l++)
                        {
                            sum += Bigrams.FlatList2[(ciphertext[p1 - l] << 8) + ciphertext[p2 - l]];
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
                long[] base_ = new long[fullRows + keylen];

                for (int c1 = 0; c1 < keylen; c1++)
                {
                    int minP1 = Key1MinColEnd[c1];
                    int maxP1 = Key1MaxColEnd[c1];

                    for (int c2 = 0; c2 < keylen; c2++)
                    {
                        if (c1 == c2)
                        {
                            continue;
                        }

                        int minP2 = Key1MinColEnd[c2];
                        int maxP2 = Key1MaxColEnd[c2];

                        int offset1 = maxP1 - minP1;
                        int offset2 = maxP2 - minP2;

                        int start1 = minP1 - fullRows + 1;
                        int start2 = minP2 - fullRows + 1;
                        long best = 0;

                        for (int offset = 0; offset <= offset2; offset++)
                        {
                            long sum = 0;

                            int p1 = start1;
                            int p2 = start2 + offset;

                            for (int i = 0; i < fullRows; i++)
                            {
                                long val = base_[i] = Bigrams.FlatList2[(ciphertext[p1++] << 8) + ciphertext[p2++]];
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
                                sum += Bigrams.FlatList2[(ciphertext[p1++] << 8) + ciphertext[p2++]];
                                if (best < sum)
                                {
                                    best = sum;
                                }
                            }
                        }

                        // we test only once with offset = 0;
                        for (int offset = 1; offset <= offset1; offset++)
                        {
                            long sum = 0;

                            int p1 = start1 + offset;
                            int p2 = start2;

                            for (int i = 0; i < fullRows; i++)
                            {
                                long val = base_[i] = Bigrams.FlatList2[(ciphertext[p1++] << 8) + ciphertext[p2++]];
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
                                sum += Bigrams.FlatList2[(ciphertext[p1++] << 8) + ciphertext[p2++]];
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

            return getMatrixScore(p1p2Best);
        }

        private static long getMatrixScore(long[,] matrix)
        {
            int dimension = matrix.GetLength(0);

            int[] left = new int[dimension];
            int[] right = new int[dimension];

            for (int i = 0; i < dimension; i++)
            {
                left[i] = right[i] = -1;
            }

            long sum = 0;

            for (int i = 1; i <= dimension; i++)
            {
                long best = 0;
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
                if (bestP1 == -1)
                {
                    Console.Write("-1\n");
                }
                else
                {
                    left[bestP2] = bestP1;
                    right[bestP1] = bestP2;
                }
            }

            return sum / dimension;
        }

        public static void decrypt2(byte[] key, int keylen, byte[] ciphertext, int ciphertextLength, byte[] plaintext)
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

        private void swap(byte[] arr, int i, int j)
        {
            byte tmp = arr[i]; arr[i] = arr[j]; arr[j] = tmp;
        }

        private void blockswap(byte[] arr, int f, int t, int l)
        {
            for (int i = 0; i < l; i++)
            {
                swap(arr, (f + i) % arr.Length, (t + i) % arr.Length);
            }
        }

        private void pivot(byte[] arr, int p)
        {
            byte[] tmp = new byte[arr.Length];
            Array.Copy(arr, tmp, arr.Length);

            Array.Copy(tmp, p, arr, 0, arr.Length - p);
            Array.Copy(tmp, 0, arr, arr.Length - p, p);
        }

        private void blockshift(byte[] arr, int f, int t, int l)
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

        private byte[] randomArray(int length)
        {
            byte[] result = new byte[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = (byte)i;
            }

            for (int i = 0; i < length; i++)
            {
                swap(result, _random.Next(length), _random.Next(length));
            }

            return result;
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

        public string Value { get; set; }
        public string Key { get; set; }
        public string KeyPhrase { get; set; }
        public byte[] KeyArray { get; set; }
        public string Text { get; set; }

        public string ClipboardValue => Value;
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
