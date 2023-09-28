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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace CrypTool.Plugins.GRANIT160Cipher
{

    [Author("Nils Kopal", "kopal@CrypTool.org", "CrypTool 2 Team", "https://www.CrypTool.org")]
    [PluginInfo("CrypTool.Plugins.GRANIT160Cipher.Properties.Resources", "PluginCaption", "PluginTooltip", "GRANIT160Cipher/userdoc.xml", new[] { "GRANIT160Cipher/Images/icon.png" })]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class GRANIT160Cipher : ICrypComponent
    {
        #region Private Variables

        private const string Alphabet = "ABCDEFGHIKLMNOPQRSTUVWXYZ";
        private const string AlphabetWithJ = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private const string digits = "0123456789";

        private readonly GRANIT160CipherSettings _settings = new GRANIT160CipherSettings();

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputTextCaption", "InputTextTooltip", true)]
        public string InputText
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "Keyword1Caption", "Keyword1Tooltip", false)]
        public string Keyword1
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "Keyword2Caption", "Keyword2Tooltip", false)]
        public string Keyword2
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "Keyword3Caption", "Keyword3Tooltip", false)]
        public string Keyword3
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

        #endregion

        #region IPlugin Members

        public ISettings Settings => _settings;

        public UserControl Presentation => null;

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
            ProgressChanged(0, 1);

            //set used alphabet (including J or not)
            string alphabet = AlphabetWithJ;
            if (_settings.ReplaceLetterJ)
            {
                alphabet = Alphabet;
            }
            if (_settings.EncryptSpace)
            {
                alphabet += " ";
            }

            string keyword1 = Keyword1;
            string keyword2 = CleanTranspositionKeyword(Keyword2, alphabet);
            string keyword3 = CleanTranspositionKeyword(Keyword3, alphabet);

            switch (_settings.Action)
            {
                case Action.Encrypt:
                    //format plaintext according to rules
                    string plaintext = FormatPlaintextBeforeEncryption(InputText, alphabet);
                    OutputText = EncryptGRANIT160(plaintext, keyword1, keyword2, keyword3, alphabet);
                    break;

                case Action.Decrypt:

                    OutputText = DecryptGRANIT160(InputText, keyword1, keyword2, keyword3, alphabet);
                    //format plaintext according to rules
                    OutputText = FormatPlaintextAfterDecryption(OutputText);
                    break;
            }
            OnPropertyChanged(nameof(OutputText));

            ProgressChanged(1, 1);
        }

        #region GRANIT 160 encryption and decryption

        /// <summary>
        /// Encrypts a given plaintext with GRANIT 160 using keyword1, keyword2, and keyword3
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="keyword1"></param>
        /// <param name="keyword2"></param>
        /// <param name="keyword3"></param>
        /// <param name="alphabet"></param>
        /// <returns></returns>
        public string EncryptGRANIT160(string plaintext, string keyword1, string keyword2, string keyword3, string alphabet)
        {
            //prepare keys
            Dictionary<string, string> encryptionDictionary = GenerateStraddlingCheckerboardKey(keyword1, true, alphabet);
            keyword2 = CleanTranspositionKeyword(keyword2, alphabet);
            keyword3 = CleanTranspositionKeyword(keyword3, alphabet);

            //encrypt
            string ciphertext = EncryptStraddlingCheckerboard(plaintext, encryptionDictionary);
            ciphertext = EncryptTransposition(ciphertext, keyword2);
            ciphertext = EncryptTransposition(ciphertext, keyword3);

            return ciphertext;
        }

        /// <summary>
        /// Decrypts a given ciphertext with GRANIT 160 using keyword1, keyword2, and keyword3
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="keyword1"></param>
        /// <param name="keyword2"></param>
        /// <param name="keyword3"></param>
        /// <param name="alphabet"></param>
        /// <returns></returns>
        public string DecryptGRANIT160(string ciphertext, string keyword1, string keyword2, string keyword3, string alphabet)
        {
            //prepare keys
            Dictionary<string, string> decryptionDictionary = GenerateStraddlingCheckerboardKey(keyword1, false, alphabet);
            keyword2 = CleanTranspositionKeyword(keyword2, alphabet);
            keyword3 = CleanTranspositionKeyword(keyword3, alphabet);

            //decrypt
            string plaintext = DecryptTransposition(ciphertext, keyword3);
            plaintext = DecryptTransposition(plaintext, keyword2);
            plaintext = DecryptStraddlingCheckerboard(plaintext, decryptionDictionary);

            return plaintext;
        }

        #endregion

        #region plaintext formatting

        /// <summary>
        /// Formats the plaintext according to rules described in 
        /// "Gebrauchsanweisung für das Chiffrierverfahren GRANIT / 160"
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string FormatPlaintextBeforeEncryption(string text, string alphabet)
        {
            text = text.ToUpper();

            //for J they used two I
            if (_settings.ReplaceLetterJ)
            {
                text = text.Replace("J", "II");
            }

            //replace umlauts and ß
            if (_settings.ReplaceUmlauts)
            {                
                text = text.Replace("Ä", "AE");
                text = text.Replace("Ü", "UE");
                text = text.Replace("Ö", "OE");
                text = text.Replace("ß", "SS");                
            }

            text = text.Replace("?", "..");

            StringBuilder stringBuilder = new StringBuilder();
            int offset = 0;
            bool collectingNumber = false;

            while (offset < text.Length)
            {
                char letter = text[offset];

                if (alphabet.Contains(letter) || letter == '.' || letter == ',')
                {
                    if (collectingNumber)
                    {
                        stringBuilder.Append("zs");
                        collectingNumber = false;
                    }
                    stringBuilder.Append(letter);
                }
                else if (digits.Contains(letter))
                {
                    if (!collectingNumber)
                    {
                        stringBuilder.Append("zs");
                        collectingNumber = true;
                    }
                    //digits are repeated 3 times
                    stringBuilder.Append(letter);
                    stringBuilder.Append(letter);
                    stringBuilder.Append(letter);
                }

                offset++;
            }
            if (collectingNumber)
            {
                //it could be, that the plaintext ended with a digit
                stringBuilder.Append("zs");
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Formats the plaintext according to rules described in 
        /// "Gebrauchsanweisung für das Chiffrierverfahren GRANIT / 160"
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string FormatPlaintextAfterDecryption(string text)
        {
            StringBuilder builder = new StringBuilder(text);

            //for J they used two I
            if (_settings.ReplaceLetterJ)
            {
                builder.Replace("II", "J");
            }

            //replace umlauts and ß
            if (_settings.ReplaceUmlauts)
            {                               
                builder.Replace("AE", "Ä");
                builder.Replace("UE", "Ü");
                builder.Replace("OE", "Ö");
                builder.Replace("SS", "ß");
            }

            //remove number signal (Ger. "Zahlensignal")
            builder.Replace("zs", string.Empty);

            //digits are repeated 3 times; so we replace each 3 digts by 1 digit
            for (int i = 0; i < 10; i++)
            {
                builder.Replace(string.Format("{0}{0}{0}", i), string.Format("{0}", i));
            }
            return builder.ToString();
        }

        #endregion

        #region Straddling checkerboard cipher

        /// <summary>
        /// Generates a lookup table for the straddling checkerboard
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="forEncryption"></param>
        /// <param name="alphabet"></param>
        /// <returns></returns>
        public Dictionary<string, string> GenerateStraddlingCheckerboardKey(string keyword, bool forEncryption, string alphabet)
        {
            keyword = keyword.ToUpper();
            Dictionary<string, string> key = new Dictionary<string, string>();
            List<string> symbols = new List<string>();
            foreach (char c in keyword)
            {
                if (!symbols.Contains(c.ToString()))
                {
                    symbols.Add(c.ToString());
                }
            }
            foreach (char c in alphabet)
            {
                if (!symbols.Contains(c.ToString()))
                {
                    symbols.Add(c.ToString());
                }
            }
            symbols.Add("zs"); //number signal (Ger. "Zahlensignal")
            symbols.Add(".");
            symbols.Add(",");

            for (int y = 7; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    if (y == 7 && x < 8)
                    {
                        if (forEncryption)
                        {
                            key.Add(symbols.First(), string.Format("{0}", x));
                        }
                        else
                        {
                            key.Add(string.Format("{0}", x), symbols.First());
                        }
                        symbols.RemoveAt(0);
                    }
                    else if (y > 7)
                    {
                        if (forEncryption)
                        {
                            key.Add(symbols.First(), string.Format("{0}{1}", y, x));
                        }
                        else
                        {
                            key.Add(string.Format("{0}{1}", y, x), symbols.First());
                        }
                        symbols.RemoveAt(0);
                    }
                }
            }
            return key;
        }

        /// <summary>
        /// Encrypts a given plaintext with a straddling checkerboard using the given keyword
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public string EncryptStraddlingCheckerboard(string plaintext, Dictionary<string, string> encryptionDictionary)
        {
            StringBuilder ciphertextBuilder = new StringBuilder();

            int offset = 0;
            while (offset < plaintext.Length)
            {
                char letter = plaintext[offset];

                //we have an alphabet symbol
                if (encryptionDictionary.ContainsKey(letter.ToString()))
                {
                    ciphertextBuilder.Append(encryptionDictionary[letter.ToString()]);
                }
                else
                {
                    //we have number signal (Ger. "Zahlensignal")
                    if (letter == 'z' && offset < plaintext.Length - 1 && plaintext[offset + 1] == 's')
                    {
                        ciphertextBuilder.Append(encryptionDictionary["zs"]);
                        offset += 2;
                        continue;
                    }
                    //we have a digit
                    if (digits.Contains(letter))
                    {
                        ciphertextBuilder.Append(letter);
                    }
                }
                offset++;
            }

            if (_settings.AddNullsIfNeeded)
            {
                Random random = new Random();
                while(ciphertextBuilder.Length % 5 != 0)
                {
                    ciphertextBuilder.Append(random.Next(0, 9));
                }
            }

            return ciphertextBuilder.ToString();
        }

        /// <summary>
        /// Decrypts a given plaintext with a straddling checkerboard using the given keyword
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="decryptionDictionary"></param>
        /// <returns></returns>
        public string DecryptStraddlingCheckerboard(string ciphertext, Dictionary<string, string> decryptionDictionary)
        {
            StringBuilder plaintextBuilder = new StringBuilder();

            int offset = 0;
            bool collectingNumber = false;

            while (offset < ciphertext.Length)
            {
                string symbol = string.Format("{0}", ciphertext[offset].ToString());
                if (!collectingNumber && decryptionDictionary.ContainsKey(symbol))
                {
                    plaintextBuilder.Append(decryptionDictionary[symbol]);
                    offset++;
                    continue;
                }
                if (offset < ciphertext.Length - 1)
                {
                    symbol = string.Format("{0}{1}", ciphertext[offset].ToString(), ciphertext[offset + 1]);
                    if (decryptionDictionary.ContainsKey(symbol) && decryptionDictionary[symbol].Equals("zs"))
                    {
                        plaintextBuilder.Append("zs");
                        collectingNumber = !collectingNumber;
                        offset += 2;
                        continue;
                    }
                    if (!collectingNumber && decryptionDictionary.ContainsKey(symbol))
                    {
                        plaintextBuilder.Append(decryptionDictionary[symbol]);
                        offset += 2;
                        continue;
                    }
                }
                plaintextBuilder.Append(ciphertext[offset].ToString());
                offset++;
            }
            return plaintextBuilder.ToString();
        }

        #endregion

        #region Transposition cipher

        /// <summary>
        /// Cleans a given keyword by only using letters from A to Z (including J)
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="alphabet"></param>
        /// <returns></returns>
        public string CleanTranspositionKeyword(string keyword, string alphabet)
        {
            StringBuilder builder = new StringBuilder();
            keyword = keyword.ToUpper();
            foreach (char letter in keyword)
            {
                if (alphabet.Contains(letter))
                {
                    builder.Append(letter);
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Generates a numeric transposition key from a given keyword
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public int[] GenerateTranspositionLookupKey(string keyword)
        {
            if (keyword.Length == 0)
            {
                return new int[] { 0 };
            }
            char[] keywordLetters = keyword.ToCharArray();
            char[] originalKeywordLetters = keyword.ToCharArray();
            int[] lookupKey = new int[keywordLetters.Length];
            Array.Sort(keywordLetters);
            for (int i = 0; i < originalKeywordLetters.Length; i++)
            {
                lookupKey[Array.IndexOf(keywordLetters, originalKeywordLetters[i])] = i;
                keywordLetters[Array.IndexOf(keywordLetters, originalKeywordLetters[i])] = (char)0;
            }
            return lookupKey;
        }

        /// <summary>
        /// Encrypts with columnar transposition cipher using the given keyword
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public string EncryptTransposition(string plaintext, string keyword)
        {
            int[] key = GenerateTranspositionLookupKey(keyword);
            StringBuilder ciphertextBuilder = new StringBuilder();
            for (int i = 0; i < key.Length; i++)
            {
                int offset = key[i];
                while (offset < plaintext.Length)
                {
                    ciphertextBuilder.Append(plaintext[offset]);
                    offset += key.Length;
                }
            }
            return ciphertextBuilder.ToString();
        }

        /// <summary>
        /// Decrypts with columnar transposition cipher using the given keyword
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public string DecryptTransposition(string ciphertext, string keyword)
        {
            int[] key = GenerateTranspositionLookupKey(keyword);
            StringBuilder plaintextBuilder = new StringBuilder(new string('_', ciphertext.Length));
            int position = 0;
            for (int i = 0; i < key.Length; i++)
            {
                int offset = key[i];
                while (offset < ciphertext.Length)
                {
                    plaintextBuilder[offset] = ciphertext[position];
                    offset += key.Length;
                    position++;
                }
            }
            return plaintextBuilder.ToString();
        }
        #endregion       

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
