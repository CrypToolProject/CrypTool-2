using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace WordPatterns
{
    /*
     * Proposed changes and enhancements:
     * - multiple word search with one TextInput (split words at whitespace)
     * - enter max match number
     * - enter pattern in number format (like 1-2-2-1)
     * - add filter function (see Borland C++ tool)
     * - save last input words and propose them to user
     * - improve performance
     * - support wildcard (*)
     */
    [Author("Matthäus Wander", "wander@CrypTool.org", "Fachgebiet Verteilte Systeme, Universität Duisburg-Essen", "http://www.vs.uni-due.de")]
    [PluginInfo("WordPatterns.Properties.Resources", "PluginCaption", "PluginTooltip", "WordPatterns/DetailedDescription/doc.xml", "WordPatterns/Images/icon.png")]
    [ComponentCategory(ComponentCategory.CryptanalysisGeneric)]
    public class WordPatterns : ICrypComponent
    {
        #region Private stuff

        private WordPatternsSettings settings = new WordPatternsSettings();

        private string inputText;
        private string[] inputDict;
        private string outputText;

        private readonly List<string> results = new List<string>();
        private List<List<Pattern>> inputPatterns;
        private Dictionary<int, Dictionary<Pattern, IList<string>>> PatternsOfSize = null;
        private string[] inputWords;
        private int[] sizes, sorted, sortedInverse;

        private bool stop = false;

        #endregion

        #region Properties

        [PropertyInfo(Direction.InputData, "InputTextCaption", "InputTextTooltip", true)]
        public string InputText
        {
            get => inputText;
            set
            {
                inputText = value;
                OnPropertyChanged("InputText");
            }
        }

        [PropertyInfo(Direction.InputData, "InputDictCaption", "InputDictTooltip", true)]
        public string[] InputDict
        {
            get => inputDict;
            set
            {
                inputDict = value;
                PatternsOfSize = null;
                OnPropertyChanged("InputDict");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputTextCaption", "OutputTextTooltip", false)]
        public string OutputText
        {
            get => outputText;
            private set
            {
                outputText = value;
                OnPropertyChanged("OutputText");
            }
        }

        public bool CaseSensitive => settings.CaseSelection == Case.Sensitive;

        #endregion

        #region IPlugin Members

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        public ISettings Settings
        {
            get => settings;
            set => settings = (WordPatternsSettings)value;
        }

        public System.Windows.Controls.UserControl Presentation => null;

        public void PreExecution()
        {
            stop = false;
        }

        public void Execute()
        {
            if (inputText == null)
            {
                OutputText = "";
                return;
            }

            if (inputDict == null)
            {
                return;
            }

            // If not already done, calculate pattern for each dictionary word

            if (PatternsOfSize == null)
            {
                PatternsOfSize = new Dictionary<int, Dictionary<Pattern, IList<string>>>();

                foreach (string word in inputDict)
                {
                    if (stop)
                    {
                        break;
                    }

                    Pattern p = new Pattern(word, CaseSensitive);

                    if (!PatternsOfSize.ContainsKey(word.Length))
                    {
                        PatternsOfSize[word.Length] = new Dictionary<Pattern, IList<string>>();
                    }

                    if (!PatternsOfSize[word.Length].ContainsKey(p))
                    {
                        PatternsOfSize[word.Length][p] = new List<string>();
                    }

                    PatternsOfSize[word.Length][p].Add(word);
                }
            }

            // remove separator characters from inputText

            foreach (char c in settings.Separators)
            {
                inputText = inputText.Replace(c.ToString(), "");
            }

            // get input words and their patterns

            inputWords = inputText.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            inputPatterns = inputWords.Select(w => new List<Pattern>()).ToList();

            for (int i = 0; i < inputWords.Length; i++)
            {
                if (PatternsOfSize.ContainsKey(inputWords[i].Length))
                {
                    foreach (KeyValuePair<Pattern, IList<string>> p in PatternsOfSize[inputWords[i].Length])
                    {
                        if (new CharacterMap().add(p.Value[0], inputWords[i], settings.Homophonic))
                        {
                            inputPatterns[i].Add(p.Key);
                        }
                    }
                }
            }

            // sort patterns according to the number of possible words in order to minimize the recursion calls

            sizes = Enumerable.Range(0, inputWords.Length).Select(i => inputPatterns[i].Select(p => PatternsOfSize[inputWords[i].Length][p].Count).Sum()).ToArray();
            if (sizes.Length == 0 || sizes[0] == 0)
            {
                OutputText = "";
                return;
            }

            sorted = Enumerable.Range(0, inputWords.Length).ToArray();
            if (settings.Sort)
            {
                Array.Sort(sorted, (i, j) => sizes[i].CompareTo(sizes[j]));
            }

            sortedInverse = new int[sorted.Length];
            for (int i = 0; i < sorted.Length; i++)
            {
                sortedInverse[sorted[i]] = i;
            }

            results.Clear();
            recmatch(new List<string>(), new CharacterMap());
            results.Sort();

            OutputText = string.Join("\r\n", results);
        }

        private void recmatch(List<string> w, CharacterMap cm)
        {
            if (stop)
            {
                return;
            }

            int depth = w.Count;

            if (depth == inputPatterns.Count)
            {
                results.Add(string.Join(" ", sortedInverse.Select(j => w[j])));
                return;
            }

            int index = sorted[depth];
            string cipher = inputWords[index];

            int i = 0;

            foreach (Pattern pp in inputPatterns[index])
            {
                foreach (string plain in PatternsOfSize[cipher.Length][pp])
                {
                    if (depth == 0)
                    {
                        ProgressChanged(++i, sizes[index]);
                    }

                    CharacterMap new_cm = new CharacterMap(cm);
                    if (new_cm.add(plain, cipher, settings.Homophonic))
                    {
                        w.Add(plain);
                        recmatch(w, new_cm);
                        w.RemoveAt(depth);
                    }
                }
            }
        }

        internal class CharacterMap
        {
            public Dictionary<char, HashSet<char>> plain2cipher = new Dictionary<char, HashSet<char>>();
            public Dictionary<char, char> cipher2plain = new Dictionary<char, char>();

            public CharacterMap()
            {
            }

            public CharacterMap(CharacterMap cm)
            {
                cipher2plain = new Dictionary<char, char>(cm.cipher2plain);
                plain2cipher = new Dictionary<char, HashSet<char>>();
                foreach (KeyValuePair<char, HashSet<char>> x in cm.plain2cipher)
                {
                    plain2cipher[x.Key] = new HashSet<char>(x.Value);
                }
            }

            public bool add(string plain, string cipher, bool homophonic)
            {
                for (int i = 0; i < plain.Length; i++)
                {
                    if (!add(plain[i], cipher[i], homophonic))
                    {
                        return false;
                    }
                }

                return true;
            }

            public bool add(char plain, char cipher, bool homophonic)
            {
                if (cipher2plain.ContainsKey(cipher))
                {
                    return cipher2plain[cipher] == plain;
                }

                if (!plain2cipher.ContainsKey(plain))
                {
                    plain2cipher.Add(plain, new HashSet<char>());
                }
                else if (!homophonic)
                {
                    return plain2cipher[plain].Contains(cipher);
                }

                plain2cipher[plain].Add(cipher);
                cipher2plain.Add(cipher, plain);

                return true;
            }
        }

        internal struct Pattern
        {
            private const int PRIME = 16777619;

            private readonly int[] patternArray;
            private readonly int hashCode;
            public Dictionary<char, int> seenLetters;
            public static string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            internal Pattern(string word, bool caseSensitive)
            {
                if (!caseSensitive)
                {
                    word = word.ToLower();
                }

                patternArray = new int[word.Length];
                hashCode = -2128831035; // int32 counterpart of uint32 2166136261

                seenLetters = new Dictionary<char, int>(15);
                int letterNumber = 0;

                for (int i = 0; i < word.Length; i++)
                {
                    if (seenLetters.ContainsKey(word[i])) // letter already seen?
                    {
                        patternArray[i] = seenLetters[word[i]]; // get letter number
                    }
                    else
                    {
                        seenLetters[word[i]] = patternArray[i] = ++letterNumber; // create new letter number
                    }

                    // FNV-1 hashing
                    hashCode = (hashCode * PRIME) ^ patternArray[i];
                }

                seenLetters = null;
            }

            public string proxy()
            {
                return new string(patternArray.Select(i => alphabet[i - 1]).ToArray());
            }

            /// <summary>
            /// Returns pre-calculated hash code.
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return hashCode;
            }

            /// <summary>
            /// In-depth comparison of pattern array contents.
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public override bool Equals(object right)
            {
                if (right == null)
                {
                    return false;
                }

                // Never true for value types
                //if (object.ReferenceEquals(this, right))
                //    return true;

                // Using the as/is operators can break symmetry requirement for reference types.
                // However this does not apply for value types.
                //if (this.GetType() != right.GetType())
                //    return false;
                if (!(right is Pattern))
                {
                    return false;
                }

                return this == (Pattern)right;
            }

            public static bool operator ==(Pattern left, Pattern right)
            {
                if (left.hashCode != right.hashCode)
                {
                    return false;
                }

                if (left.patternArray.Length != right.patternArray.Length)
                {
                    return false;
                }

                for (int i = 0; i < left.patternArray.Length; i++)
                {
                    // uneven pattern content
                    if (left.patternArray[i] != right.patternArray[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            public static bool operator !=(Pattern left, Pattern right)
            {
                return !(left == right);
            }
        }

        public void PostExecution()
        {
            GuiLogMessage("PostExecution has been called. Cleaning pattern dictionary...", NotificationLevel.Info);
            PatternsOfSize = null;
        }

        public void Stop()
        {
            stop = true;
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string p)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(p));
        }

        #endregion

    }
}