/*
   Copyright 2022 Nils Kopal, CrypTool project

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
using System.Security.Policy;
using System.Text;
using System.Windows.Controls;

namespace CrypTool.BookCipher
{
    [Author("Nils Kopal", "kopal@CrypTool.org", "CrypTool 2 Team", "https://www.CrypTool.org")]
    [PluginInfo("CrypTool.BookCipher.Properties.Resources", "PluginCaption", "PluginTooltip", "BookCipher/DetailedDescription/doc.xml", new[] { "BookCipher/Images/icon.png" })]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class BookCipher : ICrypComponent
    {
        private readonly BookCipherSettings _settings = new BookCipherSettings();
        private const string DEFAULT_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private readonly char[] LINE_SEPARATORS = new char[]
        {
            '\r', '\n'
        };
        private readonly char[] WORD_SEPARATORS = new char[]
        {
            ' ', ',', '.', ':', ';', '?', '!', '\t', '"', '“', '”', '„', '»', '«', '‚', '‘', '‹', '›'
        };

        public ISettings Settings => _settings;

        public UserControl Presentation => null;

        public event StatusChangedEventHandler OnPluginStatusChanged;
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        public event PropertyChangedEventHandler PropertyChanged;

        [PropertyInfo(Direction.InputData, "InputTextCaption", "InputStringTooltip", true)]
        public string InputText
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "KeyCaption", "KeyTooltip", true)]
        public string Key
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

        [PropertyInfo(Direction.OutputData, "OutputTextCaption", "OutputStringTooltip", false)]
        public string OutputText
        {
            get;
            set;
        }

        public void Dispose()
        {
        }

        public void Execute()
        {
            //If the user did not provide an alphabet, we set it to the default Latin alphabet here:
            if (string.IsNullOrEmpty(Alphabet) || string.IsNullOrWhiteSpace(Alphabet))
            {
                Alphabet = DEFAULT_ALPHABET;
            }

            if (_settings.Action == Action.Encrypt)
            {
                switch (_settings.EncodingMode)
                {
                    case EncodingMode.FirstLetter:
                        EncryptFirstLetterMode();
                        break;
                    case EncodingMode.CompleteWord:
                        EncryptCompleteWordMode();
                        break;
                }
            }
            else if (_settings.Action == Action.Decrypt)
            {
                switch (_settings.EncodingMode)
                {
                    case EncodingMode.FirstLetter:
                        Decrypt(true);
                        break;
                    case EncodingMode.CompleteWord:
                        Decrypt(false);
                        break;
                }
            }
            OnPropertyChanged("OutputText");
        }

        /// <summary>
        /// Creates a dictionary for looking up WordPositions in the given key text
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, List<WordPositions>> CreateLookupDictionary(bool letters)
        {
            Dictionary<string, List<WordPositions>> dictionary = new Dictionary<string, List<WordPositions>>();

            int globalWordNumber = 1;
            int globalPageNumber = 1;
            int globalLineNumber = 1;
            int wordInPageNumber = 1;
            int lineInPageNumber = 1;
            int wordInLineNumber;

            string pageSeparator = _settings.PageSeparator.ToUpper();

            string[] lines = Key.ToUpper().Split(LINE_SEPARATORS);

            foreach (string line in lines)
            {
                wordInLineNumber = 1;
                if (string.IsNullOrEmpty(line) || string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] words = line.Split(WORD_SEPARATORS);
                foreach (string word in words)
                {
                    if (string.IsNullOrEmpty(word) || string.IsNullOrWhiteSpace(word))
                    {
                        continue;
                    }
                    if (word.Equals(pageSeparator))
                    {
                        globalPageNumber++;
                        wordInPageNumber = 1;
                        wordInLineNumber = 1;
                        lineInPageNumber = 1;
                        continue;
                    }
                    if (IndexOfChar(Alphabet, word[0]) == -1)
                    {
                        continue;
                    }

                    string key;
                    if (letters)
                    {
                        key = word[0].ToString();
                    }
                    else
                    {
                        key = word;
                    }

                    if (!dictionary.ContainsKey(key))
                    {
                        dictionary[key] = new List<WordPositions>();
                    }

                    WordPositions positions = new WordPositions
                    {
                        GlobalWordNumber = globalWordNumber,
                        GlobalPageNumber = globalPageNumber,
                        GlobalLineNumber = globalLineNumber,
                        LineInPageNumber = lineInPageNumber,
                        WordInPageNumber = wordInPageNumber,
                        WordInLineNumber = wordInLineNumber
                    };
                    dictionary[key].Add(positions);

                    globalWordNumber++;
                    wordInPageNumber++;
                    wordInLineNumber++;
                }
                globalLineNumber++;
                lineInPageNumber++;
            }
            return dictionary;
        }

        /// <summary>
        /// Encrypts using the first letter of the words
        /// </summary>
        private void EncryptFirstLetterMode()
        {
            Random random = new Random();
            Dictionary<string, List<WordPositions>> dictionary = CreateLookupDictionary(true);
            StringBuilder ciphertextBuilder = new StringBuilder();           

            foreach (char c in InputText.ToUpper())
            {
                if (IndexOfChar(Alphabet, c) == -1)
                {
                    continue;
                }
                string character = c.ToString();
                if (dictionary.ContainsKey(character))
                {
                    ciphertextBuilder.Append(string.Format("{0} ", dictionary[character][random.Next(0, dictionary[character].Count - 1)].GetPosition(
                        _settings.EncodePages,
                        _settings.EncodeLines,
                        _settings.PageDigits,
                        _settings.LineDigits,
                        _settings.WordDigits,
                        _settings.EncodingStyle,
                        GetNumberSeparator())));
                }
                else
                {
                    ciphertextBuilder.Append("?");
                }
            }
            OutputText = ciphertextBuilder.ToString();
        }

        /// <summary>
        /// Get the selected number separator
        /// </summary>
        /// <returns></returns>
        private string GetNumberSeparator()
        {
            string separator = ".";
            switch (_settings.NumberSeparator)
            {
                case NumberSeparator.Fullstop:
                    separator = ".";
                    break;
                case NumberSeparator.Comma:
                    separator = ",";
                    break;
                case NumberSeparator.Colon:
                    separator = ";";
                    break;
                case NumberSeparator.Semicolon:
                    separator = ":";
                    break;
                case NumberSeparator.Dash:
                    separator = "-";
                    break;
                case NumberSeparator.Slash:
                    separator = "/";
                    break;
                case NumberSeparator.Backslash:
                    separator = "\\";
                    break;
                case NumberSeparator.Plus:
                    separator = "+";
                    break;
                case NumberSeparator.Asterisk:
                    separator = "*";
                    break;
            }
            return separator;
        }

        /// <summary>
        /// Encrypts using the complete words
        /// </summary>
        private void EncryptCompleteWordMode()
        {
            Random random = new Random();
            Dictionary<string, List<WordPositions>> dictionary = CreateLookupDictionary(false);
            StringBuilder ciphertextBuilder = new StringBuilder();

            string[] words = InputText.ToUpper().Split(Concat(WORD_SEPARATORS, LINE_SEPARATORS));
            foreach (string word in words)
            {
                if (string.IsNullOrEmpty(word))
                {
                    continue;
                }
                if (dictionary.ContainsKey(word))
                {
                    ciphertextBuilder.Append(string.Format("{0} ", dictionary[word][random.Next(0, dictionary[word].Count - 1)].GetPosition(
                        _settings.EncodePages,
                        _settings.EncodeLines,
                        _settings.PageDigits,
                        _settings.LineDigits,
                        _settings.WordDigits,
                        _settings.EncodingStyle,
                        GetNumberSeparator())));
                }
                else
                {
                    ciphertextBuilder.Append("? ");
                }
            }
            OutputText = ciphertextBuilder.ToString();
        }

        /// <summary>
        /// Decrypts the given ciphertext (single letter and complete word mode; defined by user setting)
        /// </summary>
        private void Decrypt(bool letters)
        {
            Dictionary<string, List<WordPositions>> dictionary = CreateLookupDictionary(letters);

            //create reverse lookup dictionary for decryption:
            Dictionary<string, string> reverseLookupDictionary = new Dictionary<string, string>();
            foreach (string key in dictionary.Keys)
            {
                foreach (WordPositions positions in dictionary[key])
                {
                    string inverseKey = positions.GetPosition(
                        _settings.EncodePages,
                        _settings.EncodeLines,
                        _settings.PageDigits,
                        _settings.LineDigits,
                        _settings.WordDigits,
                        _settings.EncodingStyle,
                        GetNumberSeparator());
                    if (!reverseLookupDictionary.ContainsKey(inverseKey))
                    {
                        reverseLookupDictionary.Add(inverseKey, key);
                    }
                }
            }

            //decrypt the ciphertext
            StringBuilder plaintextBuilder = new StringBuilder();

            char[] splitValus = Concat(WORD_SEPARATORS, LINE_SEPARATORS);
            if(_settings.EncodingStyle == EncodingStyle.SymbolSeparatedNumbers)
            {
                splitValus = Remove(splitValus, GetNumberSeparator()[0]);
            }

            string[] codeWords = InputText.ToUpper().Split(splitValus);

            foreach (string codeWord in codeWords)
            {
                if (string.IsNullOrEmpty(codeWord))
                {
                    continue;
                }
                if (reverseLookupDictionary.ContainsKey(codeWord))
                {
                    plaintextBuilder.Append(reverseLookupDictionary[codeWord]);
                    if (!letters)
                    {
                        plaintextBuilder.Append(" ");
                    }
                }
                else
                {
                    if (letters)
                    {
                        plaintextBuilder.Append("?");
                    }
                    else
                    {
                        plaintextBuilder.Append(" ? ");
                    }
                }
            }
            OutputText = plaintextBuilder.ToString();
        }      

        public void Initialize()
        {
        }

        public void PostExecution()
        {
        }

        public void PreExecution()
        {
            Alphabet = string.Empty;
        }

        public void Stop()
        {
        }

        /// <summary>
        /// Returns the first index of the given character in given string
        /// returns -1 if character is not in the string
        /// </summary>
        /// <param name="str"></param>
        /// <param name="chr"></param>
        /// <returns></returns>
        private int IndexOfChar(string str, char chr)
        {
            int index = 0;
            foreach (char chr2 in str)
            {
                if (chr == chr2)
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        /// <summary>
        /// Concats two character arrays
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <returns></returns>
        public static char[] Concat(char[] array1, char[] array2)
        {
            char[] concat_array = new char[array1.Length + array2.Length];
            for (int i = 0; i < array1.Length; i++)
            {
                concat_array[i] = array1[i];
            }
            for (int i = 0; i < array2.Length; i++)
            {
                concat_array[array1.Length + i] = array2[i];
            }
            return concat_array;
        }

        /// <summary>
        /// Removes the occurence of the character from the char array 
        /// </summary>
        /// <param name="values"></param>
        /// <param name="findValue"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private char[] Remove(char[] values, char findValue)
        {
            bool contains = false;
            foreach(char c in values)
            {
                if (c == findValue)
                {
                    contains = true;
                    break;
                }                
            }
            if (!contains)
            {
                return values;
            }
            char[] newValues = new char[values.Length - 1];
            int i = 0;
            foreach (char c in values)
            {
                if (!(c == findValue))
                {
                    newValues[i] = c;
                    i++;
                }
                
            }
            return newValues;
        }

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }
    }

    /// <summary>
    /// A class to store all positions of the word
    /// </summary>
    internal class WordPositions
    {
        /// <summary>
        /// Number of word in ALL words of document
        /// </summary>
        public int GlobalWordNumber
        {
            get;
            set;
        }

        /// <summary>
        /// Number of page in document
        /// </summary>
        public int GlobalPageNumber
        {
            get;
            set;
        }

        /// <summary>
        /// Number of line in document
        /// </summary>
        public int GlobalLineNumber
        {
            get;
            set;
        }

        /// <summary>
        /// Number of word in page
        /// </summary>
        public int WordInPageNumber
        {
            get;
            set;
        }

        /// <summary>
        /// Number of line in page
        /// </summary>
        public int LineInPageNumber
        {
            get;
            set;
        }

        /// <summary>
        /// Number of word in line
        /// </summary>
        public int WordInLineNumber
        {
            get;
            set;
        }

        /// <summary>
        /// Returns a string-encoded position based on "encodePage", "encodeLine", as well as digit definitions
        /// for page, line, and word
        /// </summary>
        /// <param name="encodePage"></param>
        /// <param name="encodeLine"></param>
        /// <param name="pageDigits"></param>
        /// <param name="lineDigits"></param>
        /// <param name="wordDigits"></param>
        /// <returns></returns>
        public string GetPosition(bool encodePage, bool encodeLine, int pageDigits, int lineDigits, int wordDigits, EncodingStyle style, string numberSeparator)
        {
            if (style == EncodingStyle.Digits)
            {
                if (!encodePage && !encodeLine)
                {
                    return string.Format("{0}", GlobalWordNumber);
                }
                else if (encodePage && !encodeLine)
                {
                    return string.Format("{0}{1}", GlobalPageNumber.ToString(string.Format("D{0}", pageDigits)), WordInPageNumber.ToString(string.Format("D{0}", wordDigits)));
                }
                else if (!encodePage && encodeLine)
                {
                    return string.Format("{0}{1}", GlobalLineNumber.ToString(string.Format("D{0}", lineDigits)), WordInLineNumber.ToString(string.Format("D{0}", wordDigits)));
                }
                else //if(encodePage && encodeLine)
                {
                    return string.Format("{0}{1}{2}", GlobalPageNumber.ToString(string.Format("D{0}", pageDigits)), LineInPageNumber.ToString(string.Format("D{0}", lineDigits)), WordInLineNumber.ToString(string.Format("D{0}", wordDigits)));
                }
            }
            else
            {               
                if (!encodePage && !encodeLine)
                {
                    return string.Format("{0}", GlobalWordNumber);
                }
                else if (encodePage && !encodeLine)
                {
                    return string.Format("{0}{2}{1}", GlobalPageNumber.ToString(), WordInPageNumber.ToString(), numberSeparator);
                }
                else if (!encodePage && encodeLine)
                {
                    return string.Format("{0}{2}{1}", GlobalLineNumber.ToString(), WordInLineNumber.ToString(), numberSeparator);
                }
                else //if(encodePage && encodeLine)
                {
                    return string.Format("{0}{3}{1}{3}{2}", GlobalPageNumber.ToString(), LineInPageNumber.ToString(), WordInLineNumber.ToString(), numberSeparator);
                }
            }
        }
    }
}