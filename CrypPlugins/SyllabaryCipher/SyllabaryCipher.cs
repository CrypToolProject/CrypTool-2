/*
   Copyright 2022 Nils Kopal <kopal<AT>cryptool.org>

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Controls;
using SyllabaryCipher.Properties;
using SyllabaryCipher;

namespace CrypTool.SyllabaryCipher
{
    [Author("Nils Kopal", "nils.kopal@cryptool.org", "CrypTool project", "http://www.cryptool.org")]
    [PluginInfo("SyllabaryCipher.Properties.Resources", "PluginCaption", "PluginTooltip", "SyllabaryCipher/DetailedDescription/doc.xml", "SyllabaryCipher/icon.png")]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class SyllabaryCipher : ICrypComponent
    {
        private const string DIGITS = "0123456789";
        private const string DEFAULT_DIGIT_KEY = "12345678901234567890";

        private readonly SyllabaryCipherSettings _settings = new SyllabaryCipherSettings();
        private readonly SyllabaryCipherPresentation _presentation = new SyllabaryCipherPresentation();
        private readonly string[,] _table = new string[10, 10];        

        [PropertyInfo(Direction.InputData, "InputTextCaption", "InputTextTooltip", true)]
        public string InputText
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "DigitKeyCaption", "DigitKeyTooltip", false)]
        public string DigitKey
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "KeywordCaption", "KeywordTooltip", false)]
        public string Keyword
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "OutputTextCaption", "OutputTextTooltip", true)]
        public string OutputText
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "TableOutputCaption", "TableOutputTooltip", false)]
        public string TableOutput
        {
            get;
            set;
        }

        public void PreExecution()
        {
            
        }

        public void PostExecution()
        {
            
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public ISettings Settings => _settings;

        public UserControl Presentation => _presentation;

        public void Execute()
        {
            ProgressChanged(0, 1);

            if (string.IsNullOrEmpty(DigitKey))
            {
                DigitKey = DEFAULT_DIGIT_KEY;
            }
            //Check key. If it is invalid, we abort here
            if (!KeyIsValid(DigitKey))
            {                
                return;
            }           

            //Copy table based on selected language 
            CopyTable(_settings.TableLanguage);

            //apply keyword to table
            if (!string.IsNullOrEmpty(Keyword) && !string.IsNullOrWhiteSpace(Keyword))
            {
                ApplyKeywordToTable(Keyword.ToUpper());
            }

            //visualize the key in the plugin's prese
            //ntation:
            _presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                try
                {
                    _presentation.FillTable(DigitKey, _table);
                }
                catch (Exception ex)
                {
                    GuiLogMessage(string.Format(Resources.ExceptionWhileGuiCreation, ex.Message), NotificationLevel.Error);
                    return;
                }
            }, null);                           

            //encrypt the given text
            if (_settings.Action == Action.Encrypt)
            {

                //check, if the input text is not empty. If it is, we just output the empty string
                if (string.IsNullOrEmpty(InputText) || string.IsNullOrWhiteSpace(InputText))
                {
                    OutputText = String.Empty;
                    OnPropertyChanged(nameof(OutputText));
                    return;
                }

                OutputText = EncryptSyllabaryCipher(InputText.ToUpper(), DigitKey, _settings.EncryptionStrategy);
                OnPropertyChanged(nameof(OutputText));
            }
            //decrypt the given text
            else
            {
                //check, if the input text is not empty. If it is, we just output the empty string
                if (string.IsNullOrEmpty(InputText) || string.IsNullOrWhiteSpace(InputText))
                {
                    OutputText = String.Empty;
                    OnPropertyChanged(nameof(OutputText));
                    return;
                }                
                OutputText = DecryptSyllabaryCipher(InputText, DigitKey);
                OnPropertyChanged(nameof(OutputText));
            }

            GenerateTableOutput(DigitKey);

            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Outputs the table in a formatted way
        /// </summary>
        private void GenerateTableOutput(string key)
        {
            if(_settings.TableOutputFormat == TableOutputFormat.Formatted)
            {
                StringBuilder tableOutputBuilder = new StringBuilder();

                //output first row of digits
                tableOutputBuilder.Append("\t");
                for (int i = 0; i < 10; i++)
                {
                    tableOutputBuilder.Append(key[i]);
                    if (i < 9)
                    {
                        tableOutputBuilder.Append("\t");
                    }
                }
                tableOutputBuilder.AppendLine();

                //generate table
                for (int x = 0; x < 10; x++)
                {
                    tableOutputBuilder.Append(key[10 + x]);
                    tableOutputBuilder.Append("\t");
                    for (int y = 0; y < 10; y++)
                    {
                        tableOutputBuilder.Append(_table[x, y]);
                        if (y < 9)
                        {
                            tableOutputBuilder.Append("\t");
                        }
                    }
                    tableOutputBuilder.AppendLine();
                }

                TableOutput = tableOutputBuilder.ToString();
                OnPropertyChanged(nameof(TableOutput));
            }
            else if(_settings.TableOutputFormat == TableOutputFormat.CrypToolSubstitutionKey)
            {
                //Step 1: generate keylist of plaintext, ciphertext symbols
                int[] keylookupX = new int[10];
                int[] keylookupY = new int[10];

                int offset = 0;
                foreach (char c in key)
                {
                    if (offset < 10)
                    {
                        keylookupX[c - 48] = offset;

                    }
                    else
                    {
                        keylookupY[c - 48] = offset - 10;
                    }
                    offset++;
                }
                List<(string plainsymbol, string ciphersymbol)> keyList = new List<(string, string)>();
                for (int y = 0; y < 10; y++)
                {
                    for (int x = 0; x < 10; x++)
                    {
                        string cipher_symbol = string.Format("{0}{1}", y, x);
                        string plain_symbol = _table[keylookupY[y], keylookupX[x]];
                        keyList.Add((plain_symbol, cipher_symbol));
                    }
                }           
                
                //Step 2: Sort alphabetically based on plaintext symbols
                keyList.Sort((a, b) => a.plainsymbol.CompareTo(b.plainsymbol));

                //Step 3: Generate key in CrypTool format
                StringBuilder tableOutputBuilder = new StringBuilder();
                for (int i = 0; i < keyList.Count; i++)
                {
                    if (i < keyList.Count - 1)
                    {
                        tableOutputBuilder.AppendLine(string.Format("[{0}];[{1}]", keyList[i].plainsymbol, keyList[i].ciphersymbol));
                    }
                    else
                    {
                        //no linebreak after last element
                        tableOutputBuilder.Append(string.Format("[{0}];[{1}]", keyList[i].plainsymbol, keyList[i].ciphersymbol));
                    }
                }

                TableOutput = tableOutputBuilder.ToString();
                OnPropertyChanged(nameof(TableOutput));
            }
        }

        /// <summary>
        /// Applies keyword to the table
        /// </summary>
        /// <param name="keyword"></param>   
        private void ApplyKeywordToTable(string keyword)
        {            
            HashSet<string> allTableElements = new HashSet<string>();
            string symbol1, symbol2, symbol3;
            int offset = 0;
            int x;
            int y;            

            for (x = 0; x < 10; x++)
            {
                for(y = 0; y < 10; y++)
                {
                    allTableElements.Add(_table[x, y]);
                }
            }

            y = 0;
            x = 0;

            do
            {
                //we look at all possible table elements (1-,2-, or 3-gram) at current offset 
                symbol1 = keyword.Substring(offset, 1);
                symbol2 = offset + 2 <= keyword.Length ? keyword.Substring(offset, 2) : string.Empty;
                symbol3 = offset + 3 <= keyword.Length ? keyword.Substring(offset, 3) : string.Empty;

                //here, we take care that we always use the longest n-gram, if possible
                if (symbol3 != string.Empty && allTableElements.Contains(symbol3))
                {
                    _table[x, y] = symbol3;
                    allTableElements.Remove(symbol3);
                    offset += 3;                    
                }
                else if (symbol2 != string.Empty && allTableElements.Contains(symbol2))
                {
                    _table[x, y] = symbol2;
                    allTableElements.Remove(symbol2);
                    offset += 2;
                }
                else if (allTableElements.Contains(symbol1))
                {
                    _table[x, y] = symbol1;
                    allTableElements.Remove(symbol1);
                    offset += 1;
                }
                else //no n-gram found:
                {                   
                    offset += 1;
                    continue;
                }
                y++;
                if (y == 10)
                {
                    y = 0;
                    x = x + 1;
                }
            } while (offset < keyword.Length && x < 10);

            List<string> remainingTableParts = allTableElements.ToList();

            do
            {
                _table[x, y] = remainingTableParts.ElementAt(0);
                remainingTableParts.RemoveAt(0);
                y++;
                if (y == 10)
                {
                    y = 0;
                    x = x + 1;
                }
            } while (x < 10);
        }

        /// <summary>
        /// Copies the selected table into our table
        /// </summary>
        /// <param name="tableLanguage"></param>
        private void CopyTable(TableLanguage tableLanguage)
        {
            string[,] sourceTable;
            switch (tableLanguage)
            {
                case TableLanguage.English:
                    sourceTable = SyllabaryTables.English;
                    break;
                case TableLanguage.Italian:
                    sourceTable = SyllabaryTables.Italian;
                    break;
                case TableLanguage.French:
                    sourceTable = SyllabaryTables.French;
                    break;
                case TableLanguage.German:
                    sourceTable = SyllabaryTables.German;
                    break;
                case TableLanguage.Spanish:
                    sourceTable = SyllabaryTables.Spanish;
                    break;
                case TableLanguage.Latin:
                    sourceTable = SyllabaryTables.Latin;
                    break;
                default:
                    sourceTable = SyllabaryTables.English;
                    break;
            }

            //copy table
            for(int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    _table[i, j] = sourceTable[i, j];
                }
            }
        }

        /// <summary>
        /// Checks, if the given key is valid
        /// </summary>
        /// <returns></returns>
        private bool KeyIsValid(string key)
        {           
            if (key.Length != 20)
            {
                GuiLogMessage(Resources.InvalidKeyLength, NotificationLevel.Error);
                return false;
            }

            bool keyOK = true;
            int[] countarrayX = new int[10];
            int[] countarrayY = new int[10];

            //here, we check, if X and Y positions of the key contain each digit exactly once and nothing else
            for (int i = 0; i < 20; i++)
            {
                if (!DIGITS.Contains(key[i]))
                {
                    GuiLogMessage(string.Format(Resources.InvalidKeyElement, DigitKey[i]), NotificationLevel.Error);
                    keyOK = false;
                    continue;
                }
                if (i < 10)
                {
                    char c = key[i];
                    countarrayX[c - 48]++;
                }
                else
                {
                    char c = key[i];
                    countarrayY[c - 48]++;
                }
            }
            for (int i = 0; i < 10; i++)
            {
                if (countarrayX[i] == 0)
                {
                    GuiLogMessage(string.Format(Resources.MissingColumnKeyElement, (char)(i + 48)), NotificationLevel.Error);
                    keyOK = false;
                }
                if (countarrayX[i] > 1)
                {
                    GuiLogMessage(string.Format(Resources.DuplicateColumnKeyElement, (char)(i + 48)), NotificationLevel.Error);
                    keyOK = false;
                }
            }
            for (int i = 0; i < 10; i++)
            {
                if (countarrayY[i] == 0)
                {
                    GuiLogMessage(string.Format(Resources.MissingRowKeyElement, (char)(i + 48)), NotificationLevel.Error);
                    keyOK = false;
                }
                if (countarrayY[i] > 1)
                {
                    GuiLogMessage(string.Format(Resources.DuplicateRowKeyElement, (char)(i + 48)), NotificationLevel.Error);
                    keyOK = false;
                }
            }
            return keyOK;
        }       

        public void Stop()
        {
            
        }

        public void Initialize()
        {
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Dispose()
        {
            
        }

        /// <summary>
        /// Encrypts a given text using the given key with the "Crypto Number Table" cipher
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private string EncryptSyllabaryCipher(string plaintext, string key, EncryptionStrategy encryptionStrategy)
        {            
            //Step 1: Create lookup dictionary based on key 
            int[] keylookupX = new int[10];
            int[] keylookupY = new int[10];            
            
            int offset = 0;
            foreach (char c in key)
            {               
                if (offset < 10)
                {
                    keylookupX[c - 48] = offset;
                    
                }
                else
                {
                    keylookupY[c - 48] = offset - 10;
                }
                offset++;
            }

            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    string cipher_symbol = string.Format("{0}{1}", y, x);
                    string plain_symbol = _table[keylookupY[y], keylookupX[x]];
                    dictionary[plain_symbol] = cipher_symbol;
                }
            }

            //Step 2: Encrypt plaintext

            StringBuilder ciphertextBuilder = new StringBuilder();

            offset = 0;
            string symbol1, symbol2, symbol3;
            
            Random random = new Random();

            do
            {
                //we look at all possible ciphertext symbols (1-,2-, or 3-gram) at current offset 
                symbol1 = plaintext.Substring(offset, 1);
                symbol2 = offset + 2 <= plaintext.Length ? plaintext.Substring(offset, 2) : string.Empty;
                symbol3 = offset + 3 <= plaintext.Length ? plaintext.Substring(offset, 3) : string.Empty;

                bool symbol_found = false;
                if (encryptionStrategy == EncryptionStrategy.Longest)
                {
                    //here, we take care that we always use the longest n-gram is used, if possible
                    if (symbol3 != string.Empty && dictionary.ContainsKey(symbol3))
                    {
                        ciphertextBuilder.Append(dictionary[symbol3]);
                        offset += 3;
                        symbol_found = true;
                    }
                    else if (symbol2 != string.Empty && dictionary.ContainsKey(symbol2))
                    {
                        ciphertextBuilder.Append(dictionary[symbol2]);
                        offset += 2;
                        symbol_found = true;
                    }
                    else if (dictionary.ContainsKey(symbol1))
                    {
                        ciphertextBuilder.Append(dictionary[symbol1]);
                        offset += 1;
                        symbol_found = true;
                    }
                }
                else if (encryptionStrategy == EncryptionStrategy.Random)
                {
                    //here, we randomly select one of the possible n-grams for encryption
                    int r = -1;

                    //Case 1: all three n-grams are possible
                    if (symbol3 != string.Empty && dictionary.ContainsKey(symbol3) &&
                       symbol2 != string.Empty && dictionary.ContainsKey(symbol2) &&
                       dictionary.ContainsKey(symbol1))
                    {
                        r = random.Next(3);
                    }
                    //Case 2. only 1-gram and 2-gram are possible
                    else if (symbol2 != string.Empty && dictionary.ContainsKey(symbol2) &&
                            dictionary.ContainsKey(symbol1))
                    {
                        r = random.Next(2);
                    }
                    //Case 3: only 1-gram is possible
                    else if (dictionary.ContainsKey(symbol1))
                    {
                        r = 0;
                    }         
                    //Case 4: unknown symbol. Here, r is already equal to -1

                    switch (r)
                    {
                        case 2:
                            ciphertextBuilder.Append(dictionary[symbol3]);
                            offset += 3;
                            symbol_found = true;
                            break;
                        case 1:
                            ciphertextBuilder.Append(dictionary[symbol2]);
                            offset += 2;
                            symbol_found = true;
                            break;
                        case 0:
                            ciphertextBuilder.Append(dictionary[symbol1]);
                            offset += 1;
                            symbol_found = true;
                            break;    
                        //case -1: --> unknown sybmol; we do nothing here
                    }
                }
                //we did not find any mapping; so we have to take care of an unknown symobl
                if (!symbol_found)
                {
                    //handle unknown symbol
                    switch (_settings.UnknownSymbolHandling)
                    {
                        case UnknownSymbolHandlingMode.Ignore:
                            ciphertextBuilder.Append(symbol1);
                            break;
                        case UnknownSymbolHandlingMode.Remove:
                            //do nothing;
                            break;
                        case UnknownSymbolHandlingMode.Replace:
                            ciphertextBuilder.Append("?");
                            break;
                    }
                    offset += 1;
                }
            } while (offset < plaintext.Length);
            return ciphertextBuilder.ToString();
        }

        /// <summary>
        /// Decrypts a given text using the given key with the "Crypto Number Table" cipher
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private string DecryptSyllabaryCipher(string ciphertext, string key)
        {
            //Step 1: Create lookup dictionary based on key

            int[] keylookupX = new int[10];
            int[] keylookupY = new int[10];                   

            int offset = 0;
            foreach (char c in key)
            {
                if (offset < 10)
                {
                    keylookupX[c - 48] = offset;
                }
                else
                {
                    keylookupY[c - 48] = offset - 10;
                }
                offset++;
            }
            
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    string cipher_symbol = string.Format("{0}{1}", y, x);
                    string plain_symbol = _table[keylookupY[y], keylookupX[x]];
                    dictionary[cipher_symbol] = plain_symbol;
                }
            }

            //Step 2: Decrypt ciphertext

            StringBuilder plaintextBuilder = new StringBuilder();

            string symbol = string.Empty;
            int digits = 0;

            for (int i = 0; i < ciphertext.Length; i++)
            {
                char letter = ciphertext[i];
                if ((IndexOfChar(DIGITS, letter)) > -1)
                {
                    //collect two digit symbols
                    symbol += letter;
                    digits++;
                }
                else
                {
                    //handle unknown symbols
                    switch (_settings.UnknownSymbolHandling)
                    {
                        case UnknownSymbolHandlingMode.Ignore:
                            plaintextBuilder.Append(letter);
                            break;
                        case UnknownSymbolHandlingMode.Remove:
                            //do nothign;
                            break;
                        case UnknownSymbolHandlingMode.Replace:
                            plaintextBuilder.Append("?");
                            break;
                    }
                }

                if (digits == 2)
                {
                    //we have 2 digits (= complete symbol)
                    //add corresponding plaintext letter(s) to plaintext
                    if (dictionary.ContainsKey(symbol))
                    {
                        plaintextBuilder.Append(dictionary[symbol]);
                    }
                    else
                    {
                        //handle unknown symbols
                        switch (_settings.UnknownSymbolHandling)
                        {
                            case UnknownSymbolHandlingMode.Ignore:
                                plaintextBuilder.Append(symbol);
                                break;
                            case UnknownSymbolHandlingMode.Remove:
                                //do nothign;
                                break;
                            case UnknownSymbolHandlingMode.Replace:
                                plaintextBuilder.Append("?");
                                break;
                        }
                    }
                    digits = 0;
                    symbol = string.Empty;
                }
            }
            return plaintextBuilder.ToString();
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
        /// Property of plugin has new data
        /// </summary>
        /// <param name="name"></param>
        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Log to CT2
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="logLevel"></param>
        private void GuiLogMessage(string msg, NotificationLevel logLevel)
        {
            OnGuiLogNotificationOccured?.Invoke(this, new GuiLogEventArgs(msg, this, logLevel));
        }

        /// <summary>
        /// Set the progress of this component
        /// </summary>
        /// <param name="value"></param>
        /// <param name="max"></param>
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }
    }
}