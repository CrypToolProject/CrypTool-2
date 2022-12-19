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
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CrypTool.JosseCipher
{
    [Author("Nils Kopal", "nils.kopal@cryptool.org", "CrypTool 2 Team", "https://www.cryptool.org")]
    [PluginInfo("CrypTool.JosseCipher.Properties.Resources", "PluginCaption", "PluginTooltip", "JosseCipher/userdoc.xml", new[] { "JosseCipher/Images/icon.png" })]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class JosseCipher : ICrypComponent
    {
        #region Private Variables

        private const string JOSSE_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVXYZ";        
        private readonly JosseCipherSettings _settings = new JosseCipherSettings();
        private readonly JosseCipherPresentation _josseCipherPresentation = new JosseCipherPresentation();
        private readonly Dictionary<char, int> _charToIntDictionary = new Dictionary<char, int>();
        private readonly Dictionary<int, char> _intToCharDictionary = new Dictionary<int, char>();
        private char[,] _rectangle;
        private int _rectangleWidth;
        private int _rectangleHeight;
        private string _alphabet;        

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "InputTextCaption", "InputTextTooltip", true)]
        public string InputText
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "KeyCaption", "KeyTooltip", false)]
        public string Key
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "InputAlphabetCaption", "InputAlphabetTooltip", false)]
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

        #endregion

        #region IPlugin Members

        public ISettings Settings => _settings;

        public UserControl Presentation => _josseCipherPresentation;

        /// <summary>
        /// Called once when workflow execution starts.
        /// </summary>
        public void PreExecution()
        {
            _alphabet = JOSSE_ALPHABET;
        }

        /// <summary>
        /// Called every time this plugin is run in the workflow execution.
        /// </summary>
        public void Execute()
        {
            ProgressChanged(0, 1);

            if (string.IsNullOrEmpty(Key) || string.IsNullOrWhiteSpace(Key))
            {
                Key = string.Empty;
            }
            if (string.IsNullOrEmpty(InputText) || string.IsNullOrWhiteSpace(InputText))
            {
                InputText = string.Empty;
            }
            if (!string.IsNullOrEmpty(Alphabet) && !string.IsNullOrWhiteSpace(Alphabet))
            {
                _alphabet = Alphabet.ToUpper();
                HashSet<char> letters = new HashSet<char>();
                foreach(char chr in _alphabet)
                {
                    if (letters.Contains(chr))
                    {
                        GuiLogMessage("Invalid alphabet given. It contains at least one of the letters multiple times", NotificationLevel.Error);
                        return;
                    }
                    letters.Add(chr);
                }
            }

            //remove all non-alphabet letters from the keyword
            Key = CleanInputString(Key.ToUpper());

            if (!GeneraterectangleAndDictionaries())
            {
                return;
            }

            DataTable characterMappingTable = BuildCharacterMappingTable();
            DataTable rectangleTable = BuildRectangleTable();
            Presentation.Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate 
            {
                _josseCipherPresentation.BuildCharacterMappingTable(characterMappingTable);
                _josseCipherPresentation.BuildRectangleTable(rectangleTable);
            }, null);

            //we only work with uppercase letters
            InputText = InputText.ToUpper();

            //remove all non-alphabet letters from the input text
            InputText = CleanInputString(InputText);

            //perform en- or decryption
            switch (_settings.Action)
            {
                default:
                case Action.Encrypt:
                    if (_settings.EnablePeriod)
                    {
                        StringBuilder ciphertextBuilder = new StringBuilder();
                        for (int position = 0; position < InputText.Length; position += _settings.Period)
                        {
                            string block = InputText.Substring(position, Math.Min(_settings.Period, InputText.Length - position));
                            ciphertextBuilder.Append(Encrypt(block));
                        }
                        OutputText = ciphertextBuilder.ToString();
                    }
                    else
                    {
                        OutputText = Encrypt(InputText);
                    }
                    break;
                case Action.Decrypt:
                    if (_settings.EnablePeriod)
                    {
                        StringBuilder plaintextBuilder = new StringBuilder();
                        for (int position = 0; position < InputText.Length; position += _settings.Period)
                        {
                            string block = InputText.Substring(position, Math.Min(_settings.Period, InputText.Length - position));
                            plaintextBuilder.Append(Decrypt(block));
                        }
                        OutputText = plaintextBuilder.ToString();
                    }
                    else
                    {
                        OutputText = Decrypt(InputText);
                    }
                    break;
            }

            OnPropertyChanged(nameof(OutputText));

            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Generates the Polybius Rectangle and the two lookup dictionaries using the given key
        /// </summary>
        /// <returns></returns>
        private bool GeneraterectangleAndDictionaries()
        {
            _intToCharDictionary.Clear();
            _charToIntDictionary.Clear();

            char[] keyArray = Key.Distinct().ToArray();
            _rectangleWidth = keyArray.Length;
            _rectangleHeight = _alphabet.Length / _rectangleWidth + (_alphabet.Length % _rectangleWidth > 0 ? 1 : 0);

            int x = 0;
            int y = 0;

            _rectangle = new char[_rectangleHeight, _rectangleWidth];

            HashSet<char> usedSymbols = new HashSet<char>();

            //add key to polybius rectangle
            foreach (char c in keyArray)
            {
                if (!usedSymbols.Contains(c))
                {
                    _rectangle[y, x] = c;
                    usedSymbols.Add(c);
                    x++;
                    if (x == _rectangleWidth)
                    {
                        x = 0;
                        y++;
                    }
                }
            }

            //add remaining alphabet letters to polybius rectangle
            foreach (char c in _alphabet)
            {
                if (!usedSymbols.Contains(c))
                {
                    _rectangle[y, x] = c;
                    usedSymbols.Add(c);
                    x++;
                    if (x == _rectangleWidth)
                    {
                        x = 0;
                        y++;
                    }
                }
            }

            int number = 1;
            for (x = 0; x < _rectangleWidth; x++)
            {
                for (y = 0; y < _rectangleHeight; y++)
                {
                    if(_rectangle[y, x] != 0)
                    {
                        _intToCharDictionary.Add(number, _rectangle[y, x]);
                        _charToIntDictionary.Add(_rectangle[y, x], number);
                        number++;
                    }
                    
                }
            }

            return true;
        }

        /// <summary>
        /// Cleans the given string; i.e. remove all invalid letters not part of the alphabet
        /// </summary>
        private string CleanInputString(string text)
        {

            StringBuilder keyBuilder = new StringBuilder();
            foreach (char c in text)
            {
                if (_alphabet.Contains(c.ToString()))
                {
                    keyBuilder.Append(c);
                }
            }
            return keyBuilder.ToString();
        }   

        /// <summary>
        /// Encrypts plaintext
        /// </summary>
        private string Encrypt(string plaintext)
        {            
            //No text given
            if (plaintext.Length == 0)
            {
                return string.Empty;
            }

            StringBuilder ciphertextBuilder = new StringBuilder();
            
            //Encrypt first letter
            int number = _charToIntDictionary[plaintext[0]];
            number = Mod(_alphabet.Length - number, _alphabet.Length);
            ciphertextBuilder.Append(_intToCharDictionary[number]);

            //Encrypt remaining letters
            for (int i = 1; i < plaintext.Length; i++)
            {
                number = _charToIntDictionary[plaintext[i]];
                int previous_number = _charToIntDictionary[i == 1 ? plaintext[i - 1] : ciphertextBuilder[i - 1]];
                number = Mod(number + previous_number, _alphabet.Length);
                ciphertextBuilder.Append(_intToCharDictionary[number]);
            }

            return ciphertextBuilder.ToString();
        }

        private string Decrypt(string ciphertext)
        {
            //No text given
            if (ciphertext.Length == 0)
            {
                return string.Empty;
            }

            StringBuilder plaintextBuilder = new StringBuilder();

            //Decrypt first letter
            int number = _charToIntDictionary[ciphertext[0]];
            number = Mod(_alphabet.Length - number, _alphabet.Length);
            plaintextBuilder.Append(_intToCharDictionary[number]);

            //Decrypt remaining letters
            for (int i = 1; i < ciphertext.Length; i++)
            {
                number = _charToIntDictionary[ciphertext[i]];
                int previous_number = _charToIntDictionary[i == 1 ? plaintextBuilder[i - 1] : ciphertext[i - 1]];
                number = Mod(number - previous_number, _alphabet.Length);
                plaintextBuilder.Append(_intToCharDictionary[number]);
            }

            return plaintextBuilder.ToString();
        }

        /// <summary>
        /// Calculates number MOD module
        /// But if the number is then 0, we add module
        /// </summary>
        /// <param name="number"></param>
        /// <param name="module"></param>
        /// <returns></returns>
        private int Mod(int number, int module)
        {
            number = ((number % module) + module) % module;
            if(number == 0)
            {
                number = module;
            }
            return number;
        }

        /// <summary>
        /// Builds a table for showing the mapping of numbers to alphabet elements
        /// </summary>
        /// <returns></returns>
        private DataTable BuildCharacterMappingTable()
        {
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("Character");
            dataTable.Columns.Add("Number");
            for (int i = 1; i <= _intToCharDictionary.Count; i++)
            {
                DataRow row = dataTable.NewRow();
                row[0] = i;
                row[1] = _intToCharDictionary[i];
                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        /// <summary>
        /// Builds a table for showing the mapping of numbers to alphabet elements
        /// </summary>
        /// <returns></returns>
        private DataTable BuildRectangleTable()
        {
            DataTable dataTable = new DataTable();

            for (int x = 0; x < _rectangleWidth; x++)
            {
                dataTable.Columns.Add(string.Empty);
            }
            for (int y = 0; y < _rectangleHeight; y++)
            {
                DataRow row = dataTable.Rows.Add(string.Empty);
                for (int x = 0; x < _rectangleWidth; x++)
                {
                    row[x] = _rectangle[y, x];

                }
            }

            return dataTable;
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