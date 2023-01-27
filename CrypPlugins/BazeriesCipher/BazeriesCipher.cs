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
using System.Numerics;
using System.Text;
using System.Windows.Controls;

namespace CrypTool.Plugins.BazeriesCipher
{

    [Author("Nils Kopal", "kopal@CrypTool.org", "CrypTool 2 Team", "https://www.CrypTool.org")]
    [PluginInfo("CrypTool.Plugins.BazeriesCipher.Properties.Resources", "PluginCaption", "PluginTooltip", "BazeriesCipher/userdoc.xml", new[] { "BazeriesCipher/Images/icon.png" })]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class BazeriesCipher : ICrypComponent
    {
        #region Private Variables

        private const string ALPHABET25 = "ABCDEFGHIKLMNOPQRSTUVWXYZ";
        private const string ALPHABET36 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";   
        private char[,] _plainSquare;
        private char[,] _polybiusSquare;
        private readonly BazeriesCipherSettings _settings = new BazeriesCipherSettings();
        private string _alphabet = ALPHABET25;
        private int _squareWidth = 5;

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


        [PropertyInfo(Direction.InputData, "NumberKeyCaption", "NumberKeyTooltip", false)]
        public BigInteger NumberKey
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

            if (string.IsNullOrEmpty(Key) || string.IsNullOrWhiteSpace(Key))
            {
                Key = String.Empty;
            }
            if(NumberKey <= 0)
            {
                NumberKey = 1;
            }
            if (string.IsNullOrEmpty(InputText) || string.IsNullOrWhiteSpace(InputText))
            {
                InputText = string.Empty;
            }

            //initialize everything needed for en- and decryption
            (_alphabet, _squareWidth) = GetAlphabetAndSquareWidth();

            //remove all non-alphabet letters from the keyword
            Key = CleanInputString(Key.ToUpper());
            _plainSquare = GeneratePolybiusSquare(string.Empty, true);
            _polybiusSquare = GeneratePolybiusSquare(Key, false);

            //we only work with uppercase letters
            InputText = InputText.ToUpper();

            //with the 25-letter alphabet, J == I            
            if (_settings.AlphabetVersion == AlphabetVersion.Twentyfive)
            {
                //thus, we replace J with I here:
                InputText = InputText.Replace('J', 'I');
            }

            //remove all non-alphabet letters from the input text
            InputText = CleanInputString(InputText);

            //perform en- or decryption
            switch (_settings.Action)
            {
                default:
                case Action.Encrypt:                    
                    OutputText = Encrypt(InputText);                    
                    break;
                case Action.Decrypt:
                    OutputText = Decrypt(InputText);                    
                    break;
            }

            OnPropertyChanged(nameof(OutputText));

            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Return the alphabet and square width based on the selected alphabet in the settings
        /// </summary>
        private (string alphabet, int squareWidth) GetAlphabetAndSquareWidth()
        {
            switch (_settings.AlphabetVersion)
            {
                default:
                case AlphabetVersion.Twentyfive:
                    return (ALPHABET25, 5);
                case AlphabetVersion.Thirtysix:
                    return (ALPHABET36, 6);
            }
        }

        /// <summary>
        /// Cleans the given string; i.e. remove all invalid letters not part of the alphabet
        /// </summary>
        private string CleanInputString(string key)
        {
            StringBuilder keyBuilder = new StringBuilder();
            foreach (char c in key)
            {
                if (_alphabet.Contains(c.ToString()))
                {
                    keyBuilder.Append(c);
                }
            }
            return keyBuilder.ToString();
        }

        /// <summary>
        /// Generates the polybius square based on the given keyword
        /// </summary>
        private char[,] GeneratePolybiusSquare(string key, bool plain)
        {
            int x = 0;
            int y = 0;

            char[,] polybiusSquare = new char[_squareWidth, _squareWidth];

            HashSet<char> usedSymbols = new HashSet<char>();

            //add key to polybius square
            foreach (char c in key)
            {
                if (!usedSymbols.Contains(c))
                {
                    if (plain)
                    {
                        polybiusSquare[y, x] = c;
                    }
                    else
                    {
                        polybiusSquare[x, y] = c;
                    }
                    usedSymbols.Add(c);
                    x++;
                    if (x == _squareWidth)
                    {
                        x = 0;
                        y++;
                    }
                }
            }

            //add remaining alphabet letters to polybius square
            foreach (char c in _alphabet)
            {
                if (!usedSymbols.Contains(c))
                {
                    if (plain)
                    {
                        polybiusSquare[y, x] = c;
                    }
                    else
                    {
                        polybiusSquare[x, y] = c;
                    }
                    usedSymbols.Add(c);
                    x++;
                    if (x == _squareWidth)
                    {
                        x = 0;
                        y++;
                    }
                }
            }
            return polybiusSquare;
        }

        /// <summary>
        /// Searches for the character in the square and returns the coordinates
        /// </summary>
        /// <param name="chr"></param>
        /// <returns></returns>
        private (int, int) GetCharacterCoordinates(char[,] square, char chr)
        {
            for (int x = 0; x < _squareWidth; x++)
            {
                for (int y = 0; y < _squareWidth; y++)
                {
                    if (square[y, x] == chr)
                    {
                        return (x, y);
                    }
                }
            }
            return (-1, -1);
        }

        /// <summary>
        /// Encrypts plaintext
        /// </summary>
        private string Encrypt(string plaintext)
        {
            StringBuilder ciphertextBuilder = new StringBuilder();
            //substitute letters
            foreach (char c in plaintext)
            {
                (int x, int y) coordinates = GetCharacterCoordinates(_plainSquare, c);
                if (coordinates != (-1, -1))
                {
                    ciphertextBuilder.Append(_polybiusSquare[coordinates.y, coordinates.x]);
                }
            }
            return ReverseTextpartsBasedOnDigitKeyword(ciphertextBuilder.ToString());
        }

        /// <summary>
        /// Decrypts ciphertext
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <returns></returns>
        private string Decrypt(string ciphertext)
        {
            StringBuilder plaintextBuilder = new StringBuilder();
            //substitute letters
            foreach (char c in ciphertext)
            {
                (int x, int y) coordinates = GetCharacterCoordinates(_polybiusSquare, c);
                if (coordinates != (-1, -1))
                {
                    plaintextBuilder.Append(_plainSquare[coordinates.y, coordinates.x]);
                }
            }
            return ReverseTextpartsBasedOnDigitKeyword(plaintextBuilder.ToString());
        }

        /// <summary>
        /// Reverses the given text based on the digit keyword
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string ReverseTextpartsBasedOnDigitKeyword(string text)
        {
            StringBuilder stringBuilder = new StringBuilder();            
            int[] patterns = GenerateDigitPattern(NumberKey);
            int patternOffset = 0;
            int i;
            for (i = 0; i < text.Length && i + patterns[patternOffset] < text.Length; i+= patterns[patternOffset], patternOffset = (patternOffset + 1) % patterns.Length)
            {
                string block = text.Substring(i, patterns[patternOffset]);
                stringBuilder.Append(new string(block.Reverse().ToArray()));                
            }
            //reverse final block if necessary
            if(i < text.Length)
            {
                int length = text.Length - i;
                string block = text.Substring(i, length);
                stringBuilder.Append(new string(block.Reverse().ToArray()));
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Generates a pattern of digits (0 is changed to 1)
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        private int[] GenerateDigitPattern(BigInteger number)
        {
            if (number == 0)
            {
                throw new Exception("Invalid key; number was zero");
            }
            List<int> digits = new List<int>();
            while(number > 0)
            {
                int digit = (int)(number % 10);
                number = number / 10;
                if (digit != 0)
                {
                    digits.Add(digit);
                }
            }            
            digits.Reverse();
            return digits.ToArray();
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
