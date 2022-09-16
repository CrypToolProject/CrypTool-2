/*
   Copyright 2022 Nils Kopal <Nils.Kopal<at>CrypTool.org

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

namespace CrypTool.Nihilist
{
    [Author("Nils Kopal", "kopal@CrypTool.org", "CrypTool 2 Team", "https://www.CrypTool.org")]
    [PluginInfo("CrypTool.Nihilist.Properties.Resources", "PluginCaption", "PluginTooltip", "Nihilist/DetailedDescription/doc.xml", new[] { "Nihilist/Images/icon.png" })]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class Nihilist : ICrypComponent
    {
        private readonly NihilistSettings _settings = new NihilistSettings();

        private const string ALPHABET25 = "ABCDEFGHIKLMNOPQRSTUVWXYZ";
        private const string ALPHABET36 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const string DIGITS = "0123456789";
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

        [PropertyInfo(Direction.InputData, "Key1Caption", "Key1Tooltip", true)]
        public string Key1
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "Key2Caption", "Key2Tooltip", true)]
        public string Key2
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
            switch (_settings.Action)
            {
                case Action.Encrypt:
                    OutputText = EncryptText(Key1.ToUpper(), Key2.ToUpper(), InputText.ToUpper());
                    break;
                case Action.Decrypt:
                    OutputText = DecryptText(Key1.ToUpper(), Key2.ToUpper(), InputText);
                    break;
            }
            OnPropertyChanged("OutputText");
        }

        /// <summary>
        /// Encrypts the input text using the Nihilist cipher
        /// See https://en.wikipedia.org/wiki/Nihilist_cipher
        /// </summary>
        /// <param name="key1"></param>
        /// <param name="key2"></param>
        /// <param name="plaintext"></param>
        /// <returns></returns>
        private string EncryptText(string key1, string key2, string plaintext)
        {            
            //Step 1: Setup our alphabet for the polybius square
            string alphabet = _settings.AlphabetVersion == AlphabetVersion.Twentyfive ? ALPHABET25 : ALPHABET36;
            int sqrtAlphabetLength = (int)Math.Sqrt(alphabet.Length);

            //Step 2: Generate polybiusSquareMapping from character to numbers based on key1            
            StringBuilder keyBuilder = new StringBuilder();
            foreach(char symbol in key1)
            {
                if (alphabet.Contains(symbol))
                {
                    keyBuilder.Append(symbol);
                }
            }
            keyBuilder.Append(alphabet);
            string keyalphabet = string.Concat(keyBuilder.ToString().Distinct());
            Dictionary<char, int> polybiusSquareMapping = new Dictionary<char, int>();
            int x = 1;
            int y = 10;
            foreach(char symbol in keyalphabet)
            {
                polybiusSquareMapping[symbol] = x + y;
                x++;
                if (x == sqrtAlphabetLength + 1)
                {
                    x = 1;
                    y += 10;
                }
            }
           
            //Step 3: Encrypt plaintext
            StringBuilder ciphertextBuilder = new StringBuilder();
            int keyOffset = 0;
            for (int i = 0; i < plaintext.Length; i++)
            {
                if (!polybiusSquareMapping.ContainsKey(plaintext[i]))
                {
                    switch (_settings.UnknownSymbolHandling)
                    {
                        case UnknownSymbolHandlingMode.Ignore:
                            ciphertextBuilder.Append(plaintext[i]);
                            break;                       
                        case UnknownSymbolHandlingMode.Replace:
                            ciphertextBuilder.Append("?");
                            break;
                        case UnknownSymbolHandlingMode.Remove:
                        default:
                            continue;
                    }
                    if (i != plaintext.Length - 1)
                    {
                        ciphertextBuilder.Append(" ");
                    }
                    continue;
                }

                //we encrypt by adding the current key 2 number to the current plaintext number
                int plaintextnumber = polybiusSquareMapping[plaintext[i]];
                int keyNumber = 0;
                if (key2.Length > 0) 
                {
                    keyNumber = polybiusSquareMapping[key2[keyOffset % key2.Length]];                    
                }
                ciphertextBuilder.Append(plaintextnumber + keyNumber);

                if (i != plaintext.Length - 1)
                {
                    ciphertextBuilder.Append(" ");
                }

                keyOffset++;
            }

            return ciphertextBuilder.ToString();
        }

        /// <summary>
        /// Decrypt the input text using the Nihilist cipher
        /// </summary>
        /// <param name="key1"></param>
        /// <param name="key2"></param>
        /// <param name="ciphertext"></param>
        /// <returns></returns>
        private string DecryptText(string key1, string key2, string ciphertext)
        {
            //Step 1: Setup our alphabet for the polybius square
            string alphabet = _settings.AlphabetVersion == AlphabetVersion.Twentyfive ? ALPHABET25 : ALPHABET36;
            int sqrtAlphabetLength = (int)Math.Sqrt(alphabet.Length);

            //Step 2: Generate polybiusSquareMapping from character to numbers based on key1
            StringBuilder keyBuilder = new StringBuilder();
            foreach (char symbol in key1)
            {
                if (alphabet.Contains(symbol))
                {
                    keyBuilder.Append(symbol);
                }
            }
            keyBuilder.Append(alphabet);
            string keyalphabet = string.Concat(keyBuilder.ToString().Distinct());
            Dictionary<char, int> polybiusSquareMapping = new Dictionary<char, int>();
            Dictionary<int, char> polybiusSquareMappingInverse = new Dictionary<int, char>();
            int x = 1;
            int y = 10;
            foreach (char symbol in keyalphabet)
            {
                polybiusSquareMapping[symbol] = x + y;
                polybiusSquareMappingInverse[x + y] = symbol;
                x++;
                if (x == sqrtAlphabetLength + 1)
                {
                    x = 1;
                    y += 10;
                }
            }

            //Step 3: Decrypt ciphertext
            StringBuilder plaintextBuilder = new StringBuilder();
            int textOffset = 0;
            int keyOffset = 0;
            while (textOffset < ciphertext.Length)
            {
                //collect number to decrypt
                int ciphertextNumber = 0;
                bool numberCollected = false;
                while (textOffset < ciphertext.Length && DIGITS.Contains(ciphertext[textOffset]))
                {
                    ciphertextNumber = ciphertextNumber * 10;
                    ciphertextNumber += DIGITS.IndexOf(ciphertext[textOffset]);
                    textOffset++;
                    numberCollected = true;
                }

                //try to decrypt the collected number
                if (numberCollected)
                {
                    int keyNumber = 0;
                    if (key2.Length > 0)
                    {
                        keyNumber = polybiusSquareMapping[key2[keyOffset % key2.Length]];
                    }
                    int plaintextNumber = ciphertextNumber - keyNumber;

                    if (polybiusSquareMappingInverse.ContainsKey(plaintextNumber))
                    {
                        plaintextBuilder.Append(polybiusSquareMappingInverse[plaintextNumber]);
                    }
                    else
                    {
                        switch (_settings.UnknownSymbolHandling)
                        {
                            case UnknownSymbolHandlingMode.Ignore:
                                plaintextBuilder.Append(keyNumber);
                                break;
                            case UnknownSymbolHandlingMode.Replace:
                                plaintextBuilder.Append("?");
                                break;
                            case UnknownSymbolHandlingMode.Remove:
                            default:
                                break;
                        }
                    }
                    keyOffset++;
                }

                //collect non digit symbols
                while (textOffset < ciphertext.Length && !DIGITS.Contains(ciphertext[textOffset]))
                {
                    switch (_settings.UnknownSymbolHandling)
                    {
                        case UnknownSymbolHandlingMode.Ignore:
                            plaintextBuilder.Append(ciphertext[textOffset]);
                            break;
                        case UnknownSymbolHandlingMode.Replace:
                            plaintextBuilder.Append("?");
                            break;
                        case UnknownSymbolHandlingMode.Remove:
                        default:
                            break;
                    }
                    textOffset++;
                }                
            }

            return plaintextBuilder.ToString();
        }


        public void Initialize()
        {
            
        }

        public void PostExecution()
        {
            
        }

        public void PreExecution()
        {
            
        }

        public void Stop()
        {
            
        }

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }
    }
}
