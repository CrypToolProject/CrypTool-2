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
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;

namespace CrypTool.Plugins.FourSquareCipher
{

    [Author("Nils Kopal", "kopal@CrypTool.org", "CrypTool 2 Team", "https://www.CrypTool.org")]
    [PluginInfo("CrypTool.Plugins.FourSquareCipher.Properties.Resources", "PluginCaption", "PluginTooltip", "FourSquareCipher/userdoc.xml", new[] { "FourSquareCipher/Images/icon.png" })]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class FourSquareCipher : ICrypComponent
    {
        #region Private Variables

        private const string ALPHABET25 = "ABCDEFGHIKLMNOPQRSTUVWXYZ";
        private const string ALPHABET36 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private char[,,] _polybiusSquares;
        private readonly FourSquareCipherSettings _settings = new FourSquareCipherSettings();
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

        [PropertyInfo(Direction.InputData, "Key1Caption", "Key1Tooltip", false)]
        public string Key1
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "Key2Caption", "Key2Tooltip", false)]
        public string Key2
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

            if (string.IsNullOrEmpty(Key1) || string.IsNullOrWhiteSpace(Key1))
            {
                Key1 = string.Empty;
            }
            if (string.IsNullOrEmpty(Key2) || string.IsNullOrWhiteSpace(Key2))
            {
                Key2 = string.Empty;
            }
            if (string.IsNullOrEmpty(InputText) || string.IsNullOrWhiteSpace(InputText))
            {
                InputText = string.Empty;
            }

            //initialize everything needed for en- and decryption
            (_alphabet, _squareWidth) = GetAlphabetAndSquareWidth();

            //remove all non-alphabet letters from the keywords
            Key1 = CleanInputString(Key1.ToUpper());
            Key2 = CleanInputString(Key2.ToUpper());

            //generate all four used polybius squares based on the given two keywords
            _polybiusSquares = GeneratePolybiusSquares(Key1, Key2);

            //we only work with uppercase letters
            InputText = InputText.ToUpper();

            //with the 25-letter alphabet, J == I            
            if (_settings.AlphabetVersion == AlphabetVersion.Twentyfive)
            {
                //thus, we replace J with I here:
                InputText = InputText.Replace('J', 'I');
            }

            //remove all non-alphabet letters from the input text
            InputText = CleanInputString(InputText, true);

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
        private string CleanInputString(string str, bool makeEvenLength = false)
        {
            StringBuilder keyBuilder = new StringBuilder();
            foreach (char c in str)
            {
                if (_alphabet.Contains(c.ToString()))
                {
                    keyBuilder.Append(c);
                }
            }
            //if the given text is not of even length, we add an X
            if (makeEvenLength && keyBuilder.Length % 2 != 0)
            {
                keyBuilder.Append("X");
            }
            return keyBuilder.ToString();
        }

        /// <summary>
        /// Generates the four polybius squares based on the given two keywords
        /// </summary>
        private char[,,] GeneratePolybiusSquares(string key1, string key2)
        {
            char[,,] polybiusSquare = new char[4, _squareWidth, _squareWidth];

            GenerateSquare(0, string.Empty);
            GenerateSquare(1, Key1);
            GenerateSquare(2, Key2);
            GenerateSquare(3, string.Empty);

            return polybiusSquare;

            //Helper method to generate a single square based on a keyword
            void GenerateSquare(int index, string key)
            {
                int x = 0;
                int y = 0;
                HashSet<char> usedSymbols = new HashSet<char>();

                //add key to polybius square
                foreach (char c in key)
                {
                    if (!usedSymbols.Contains(c))
                    {
                        polybiusSquare[index, x, y] = c;
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
                        polybiusSquare[index, x, y] = c;
                        usedSymbols.Add(c);
                        x++;
                        if (x == _squareWidth)
                        {
                            x = 0;
                            y++;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Encrypts the given plaintext
        /// </summary>
        private string Encrypt(string plaintext)
        {
            StringBuilder ciphertextBuilder = new StringBuilder();
            for (int i = 0; i < plaintext.Length; i += 2)
            {
                //get two characters
                char c1 = plaintext[i];
                char c2 = plaintext[i + 1];

                //lookup coordinates in squares
                (int x, int y) coordinates0 = GetCharacterCoordinates(0, c1);
                (int x, int y) coordinates3 = GetCharacterCoordinates(3, c2);

                //get bigram from other polybius squares
                char c3 = _polybiusSquares[1, coordinates3.x, coordinates0.y];
                char c4 = _polybiusSquares[2, coordinates0.x, coordinates3.y];

                ciphertextBuilder.Append(c3);
                ciphertextBuilder.Append(c4);
            }
            return ciphertextBuilder.ToString();
        }

        /// <summary>
        /// Decrypts the given ciphertext
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <returns></returns>
        private string Decrypt(string ciphertext)
        {
            StringBuilder plaintextBuilder = new StringBuilder();
            for (int i = 0; i < ciphertext.Length; i += 2)
            {
                //get two characters
                char c1 = ciphertext[i];
                char c2 = ciphertext[i + 1];

                //lookup coordinates in squares
                (int x, int y) coordinates1 = GetCharacterCoordinates(1, c1);
                (int x, int y) coordinates2 = GetCharacterCoordinates(2, c2);

                //get bigram from other polybius squares
                char c3 = _polybiusSquares[0, coordinates2.x, coordinates1.y];
                char c4 = _polybiusSquares[3, coordinates1.x, coordinates2.y];

                plaintextBuilder.Append(c3);
                plaintextBuilder.Append(c4);
            }
            return plaintextBuilder.ToString();
        }

        /// <summary>
        /// Searches for the character in the square and returns the coordinates
        /// </summary>
        /// <param name="chr"></param>
        /// <returns></returns>
        private (int, int) GetCharacterCoordinates(int index, char chr)
        {
            for (int x = 0; x < _squareWidth; x++)
            {
                for (int y = 0; y < _squareWidth; y++)
                {
                    if (_polybiusSquares[index, x, y] == chr)
                    {
                        return (x, y);
                    }
                }
            }
            return (-1, -1);
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
