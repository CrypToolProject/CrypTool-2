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
using System.Text;
using System.Windows.Controls;

namespace CrypTool.Plugins.BifidCipher
{

    [Author("Nils Kopal", "kopal@CrypTool.org", "CrypTool 2 Team", "https://www.CrypTool.org")]
    [PluginInfo("CrypTool.Plugins.BifidCipher.Properties.Resources", "PluginCaption", "PluginTooltip", "BifidCipher/userdoc.xml", new[] { "BifidCipher/Images/icon.png" })]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class BifidCipher : ICrypComponent
    {
        #region Private Variables

        private const string ALPHABET25 = "ABCDEFGHIKLMNOPQRSTUVWXYZ";
        private const string ALPHABET36 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private char[,] _polybiusSquare;
        private readonly BifidCipherSettings _settings = new BifidCipherSettings();
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
                Key = string.Empty;
            }
            if (string.IsNullOrEmpty(InputText) || string.IsNullOrWhiteSpace(InputText))
            {
                InputText = string.Empty;
            }

            //initialize everything needed for en- and decryption
            (_alphabet, _squareWidth) = GetAlphabetAndSquareWidth();

            //remove all non-alphabet letters from the keyword
            Key = CleanInputString(Key.ToUpper());
            _polybiusSquare = GeneratePolybiusSquare();

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
        private char[,] GeneratePolybiusSquare()
        {
            int x = 0;
            int y = 0;

            char[,] polybiusSquare = new char[_squareWidth, _squareWidth];

            HashSet<char> usedSymbols = new HashSet<char>();

            //add key to polybius square
            foreach (char c in Key)
            {
                if (!usedSymbols.Contains(c))
                {
                    polybiusSquare[y, x] = c;
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
                    polybiusSquare[y, x] = c;
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
        private (int, int) GetCharacterCoordinates(char chr)
        {
            for (int x = 0; x < _squareWidth; x++)
            {
                for (int y = 0; y < _squareWidth; y++)
                {
                    if (_polybiusSquare[y, x] == chr)
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
            //Step 1: substitute letters to coordinates using polybius square

            int[,] intermediateCiphertext = new int[2, plaintext.Length];
            int position = 0;

            foreach (char c in plaintext)
            {
                (int x, int y) coordinates = GetCharacterCoordinates(c);
                if (coordinates != (-1, -1))
                {
                    intermediateCiphertext[0, position] = coordinates.y;
                    intermediateCiphertext[1, position] = coordinates.x;
                    position++;
                }
            }

            //step 2: transpose coordinates

            int[] intermediateCiphertext2 = new int[plaintext.Length * 2];
            for (int y = 0; y < 2; y++)
            {
                for (int x = 0; x < intermediateCiphertext.Length / 2; x++)
                {
                    intermediateCiphertext2[y * plaintext.Length + x] = intermediateCiphertext[y, x];
                }
            }

            //step 3: substitute coordinates using polybius square

            StringBuilder ciphertextBuilder = new StringBuilder();
            for (int x = 0; x < intermediateCiphertext2.Length - 1; x += 2)
            {
                ciphertextBuilder.Append(_polybiusSquare[intermediateCiphertext2[x], intermediateCiphertext2[x + 1]]);
            }

            return ciphertextBuilder.ToString();
        }

        private string Decrypt(string ciphertext)
        {
            //Step 1: substitute letters to coordinates using polybius square

            int[] intermediateCiphertext = new int[ciphertext.Length * 2];
            int position = 0;

            foreach (char c in ciphertext)
            {
                (int x, int y) coordinates = GetCharacterCoordinates(c);
                if (coordinates != (-1, -1))
                {
                    intermediateCiphertext[position + 0] = coordinates.y;
                    intermediateCiphertext[position + 1] = coordinates.x;
                    position += 2;
                }
            }

            //Step 2: decrypt using coordinates (left half for x and right half for y)
            StringBuilder plaintextBuilder = new StringBuilder();
            for (int x = 0; x < ciphertext.Length; x++)
            {
                (int x, int y) coordinates = (intermediateCiphertext[x], intermediateCiphertext[x + ciphertext.Length]);
                plaintextBuilder.Append(_polybiusSquare[coordinates.x, coordinates.y]);
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
