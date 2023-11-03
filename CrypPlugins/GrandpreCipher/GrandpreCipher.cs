/*
   Copyright 2023 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace CrypTool.Plugins.GrandpreCipher
{    
    [Author("Nils Kopal", "kopal@CrypTool.org", "CrypTool 2 Team", "https://www.CrypTool.org")]
    [PluginInfo("CrypTool.Plugins.GrandpreCipher.Properties.Resources", "PluginCaption", "PluginTooltip", "GrandpreCipher/userdoc.xml", new[] { "GrandpreCipher/Images/icon.png" })]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class GrandpreCipher : ICrypComponent
    {
        #region Private Variables

        private const string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string DIGITS = "0123456789";

        private readonly GrandpreCipherSettings _settings = new GrandpreCipherSettings();

        private string _alphabet = null;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputTextCaption", "InputTextTooltip", true)]
        public string InputText
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "WordsCaption", "WordsTooltip", true)]
        public string[] Words
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "KeywordCaption", "KeywordTooltip", true)]
        public string Keyword
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "AlphabetCaption", "AlphabetTooltip", false)]
        public string Alphabet
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputTextCaption", "OutputTextTooltip", false)]
        public string OutputText
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "WordsCaption", "WordsTooltip", false)]
        public string[] OutputWords
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings => _settings;

        public UserControl Presentation => null;

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            InputText = null;
            Keyword = null;
            Words = null;
            _alphabet = ALPHABET;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);

            if (Words == null)
            {
                GuiLogMessage(Properties.Resources.NoWordsGiven, NotificationLevel.Error);
                return;
            }
            if (string.IsNullOrEmpty(Keyword) || string.IsNullOrWhiteSpace(Keyword))
            {
                GuiLogMessage(Properties.Resources.NoKeywordGiven, NotificationLevel.Error);
                return;
            }
            if (Keyword.Length < 6 || Keyword.Length > 10)
            {
                GuiLogMessage(Properties.Resources.InvalidKeywordLength, NotificationLevel.Error);
                return;
            }
            if (string.IsNullOrEmpty(InputText) || string.IsNullOrWhiteSpace(InputText))
            {
                InputText = string.Empty;
            }

            if(!string.IsNullOrEmpty(Alphabet))
            {
                _alphabet = Alphabet.ToUpper();
            }

            List<string> words = GetWordsOfSpecifiedLength(Words, Keyword.Length, _alphabet);

            OutputWords = words.ToArray();
            OnPropertyChanged(nameof(OutputWords));

            //perform en- or decryption
            switch (_settings.Action)
            {
                default:
                case Action.Encrypt:
                    try
                    {
                        Dictionary<char, List<string>> key = GetEncryptionDictionary(Keyword.ToUpper(), words);
                        OutputText = Encrypt(InputText, key);
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage(ex.Message, NotificationLevel.Error);
                        return;
                    }                    
                    break;
                case Action.Decrypt:
                    try
                    {
                        Dictionary<string, char> key = GetDecryptionDictionary(Keyword.ToUpper(), words);
                        OutputText = Decrypt(InputText, key);
                    }
                    catch (Exception ex)
                    {
                        GuiLogMessage(ex.Message, NotificationLevel.Error);
                        return;
                    }                    
                    break;
            }

            OnPropertyChanged(nameof(OutputText));

            ProgressChanged(1, 1);
        }        

        /// <summary>
        /// Creates a lookup dictionary for encryption
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="words"></param>
        /// <returns></returns>
        private Dictionary<char, List<string>> GetEncryptionDictionary(string keyword, List<string> words)
        {
            List<string> encryptionwords = new List<string>();
            HashSet<string> alreadyUsedWords = new HashSet<string>();

            //Generate our list of keywords
            foreach (char charOfKeyword in keyword)
            {
                bool found = false;
                //Search for a keyword starting with the character c which has not been used before
                foreach (string word in words)
                {
                    if (word[0] == charOfKeyword && !alreadyUsedWords.Contains(word))
                    {
                        alreadyUsedWords.Add(word);
                        encryptionwords.Add(word);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    throw new Exception(string.Format(Properties.Resources.CouldNotFindAWord, keyword.Length, charOfKeyword));
                }
            }

            Dictionary<char, List<string>> encryptionDictionary = new Dictionary<char, List<string>>();

            for (int i = 0; i < keyword.Length; i++)
            {
                string word = encryptionwords[i];
                for (int j = 0; j < keyword.Length; j++)
                {
                    if (!encryptionDictionary.ContainsKey(word[j]))
                    {
                        encryptionDictionary.Add(word[j], new List<string>());
                    }
                    string digitString = string.Format("{0}{1}", (i + 1) % 10, (j + 1) % 10);
                    encryptionDictionary[word[j]].Add(digitString);
                }
            }

            return encryptionDictionary;
        }

        /// <summary>
        /// Creates a lookup dictionary for decryption
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="words"></param>
        /// <returns></returns>
        private Dictionary<string, char> GetDecryptionDictionary(string keyword, List<string> words)
        {
            List<string> encryptionwords = new List<string>();
            HashSet<string> alreadyUsedWords = new HashSet<string>();

            //Generate our list of keywords
            foreach (char charOfKeyword in keyword)
            {
                bool found = false;
                //Search for a keyword starting with the character c which has not been used before
                foreach (string word in words)
                {
                    if (word[0] == charOfKeyword && !alreadyUsedWords.Contains(word))
                    {
                        alreadyUsedWords.Add(word);
                        encryptionwords.Add(word);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    throw new Exception(string.Format(Properties.Resources.CouldNotFindAWord, keyword.Length, charOfKeyword));
                }
            }

            Dictionary<string, char> decryptionDictionary = new Dictionary<string, char>();

            for (int i = 0; i < keyword.Length; i++)
            {
                string word = encryptionwords[i];
                for (int j = 0; j < keyword.Length; j++)
                {
                    string digitString = string.Format("{0}{1}", (i + 1) % 10, (j + 1) % 10);
                    decryptionDictionary.Add(digitString, word[j]);
                }
            }

            return decryptionDictionary;
        }

        /// <summary>
        /// Returns a list of all words with the specified length found in the given array consisting only of alphabet letters
        /// </summary>
        /// <param name="words"></param>
        /// <param name="length"></param>
        /// <param name="alphabet"></param>
        /// <returns></returns>
        private List<string> GetWordsOfSpecifiedLength(string[] words, int length, string alphabet)
        {
            List<string> wordlist = new List<string>();
            //only take valid letters from the alphabet
            StringBuilder wordBilder = new StringBuilder();
            foreach (string word in words)
            {
                wordBilder.Clear();
                foreach (char c in word.ToUpper())
                {
                    if (alphabet.Contains(c))
                    {
                        wordBilder.Append(c);
                    }
                }
                if (_settings.WordSelection == WordSelectionMode.AlsoUseLongerWords && wordBilder.Length >= length)
                {
                    wordlist.Add(wordBilder.ToString().Substring(0, length));
                }
                else if(_settings.WordSelection == WordSelectionMode.OnlyUseExactLength && wordBilder.Length == length)
                {
                    wordlist.Add(wordBilder.ToString());
                }
            }
            return wordlist;
        }

        /// <summary>
        /// Encrypts the given plaintext using the given dictionary
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private string Encrypt(string plaintext, Dictionary<char, List<string>> key)
        {
            StringBuilder ciphertextBuilder = new StringBuilder();
            Random random = new Random();

            //we store each warning message onces, so we later don't spam the log
            HashSet<string> warnings = new HashSet<string>();

            foreach (char c in plaintext.ToUpper())
            {
                if (_alphabet.Contains(c))
                {
                    if (key.ContainsKey(c))
                    {
                        List<string> digitCombinations = key[c];
                        string digits = digitCombinations[random.Next(0, digitCombinations.Count)];
                        ciphertextBuilder.Append(digits);
                        continue;
                    }
                    warnings.Add(string.Format(Properties.Resources.TableDoesNotContainPlaintextLetter, c));
                }

                //handle unknown symbol
                switch (_settings.UnknownSymbolHandling)
                {
                    case UnknownSymbolHandlingMode.Ignore:
                        ciphertextBuilder.Append(c);
                        break;
                    case UnknownSymbolHandlingMode.Remove:
                        //do nothing;
                        break;
                    case UnknownSymbolHandlingMode.Replace:
                        ciphertextBuilder.Append("?");
                        break;
                }
            }

            //show warning messages in log
            foreach(string warning in warnings)
            {
                GuiLogMessage(warning, NotificationLevel.Warning);
            }

            return ciphertextBuilder.ToString();
        }

        /// <summary>
        /// Decrypts the given ciphertext using the given dictionary
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private string Decrypt(string ciphertext, Dictionary<string, char> key)
        {
            StringBuilder plaintextBuilder = new StringBuilder();
            StringBuilder digitsBuilder = new StringBuilder();

            foreach(var c in ciphertext)
            {
                if (DIGITS.Contains(c))
                {
                    digitsBuilder.Append(c);

                    if(digitsBuilder.Length == 2)
                    {
                        string digits = digitsBuilder.ToString();
                        
                        //we found the two-digit ciphertext symbol
                        if (key.ContainsKey(digits))
                        {
                            plaintextBuilder.Append(key[digits]);
                            digitsBuilder.Clear();
                            continue;
                        }

                        //we did not find the two-digit ciphertext symbol,
                        //therefore we handle the first digit as unknown symbol
                        switch (_settings.UnknownSymbolHandling)
                        {
                            case UnknownSymbolHandlingMode.Ignore:
                                plaintextBuilder.Append(digits[0]);
                                break;
                            case UnknownSymbolHandlingMode.Remove:
                                //do nothing;
                                break;
                            case UnknownSymbolHandlingMode.Replace:
                                plaintextBuilder.Append("?");
                                break;
                        }
                        digitsBuilder.Remove(0, 1);
                    }
                    continue;
                }

                //handle unknown symbol
                switch (_settings.UnknownSymbolHandling)
                {
                    case UnknownSymbolHandlingMode.Ignore:
                        plaintextBuilder.Append(c);
                        break;
                    case UnknownSymbolHandlingMode.Remove:
                        //do nothing;
                        break;
                    case UnknownSymbolHandlingMode.Replace:
                        plaintextBuilder.Append("?");
                        break;
                }
            }            
            return plaintextBuilder.ToString();
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

        #endregion
    }
}
