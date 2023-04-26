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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace CrypTool.Plugins.ElsieFourCipher
{

    [Author("Nils Kopal", "kopal@CrypTool.org", "CrypTool 2 Team", "https://www.CrypTool.org")]
    [PluginInfo("CrypTool.Plugins.ElsieFourCipher.Properties.Resources", "PluginCaption", "PluginTooltip", "ElsieFourCipher/userdoc.xml", new[] { "ElsieFourCipher/Images/icon.png" })]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class ElsieFourCipher : ICrypComponent
    {
        #region Private Variables

        public const string _alphabet = "#_23456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private readonly ElsieFourCipherSettings _settings = new ElsieFourCipherSettings();

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

            //Check & generate key
            if (string.IsNullOrEmpty(Key) || string.IsNullOrWhiteSpace(Key))
            {
                Key = _alphabet;
            }
            Key = Key.Replace('0', '#');
            Key = Key.Replace('1', '_');
            StringBuilder keyBuilder = new StringBuilder();
            foreach(char c in Key.ToUpper())
            {
                if (_alphabet.Contains(c))
                {
                    keyBuilder.Append(c);
                }
            }
            //removes all double letters of the key and appends remaining alphabet letters 
            //to create final key for usage in the cipher
            string key = string.Concat((keyBuilder.ToString() + _alphabet).Distinct());

            //perform en- or decryption
            switch (_settings.Action)
            {
                default:
                case Action.Encrypt:
                    InputText = InputText.ToUpper();                    
                    InputText = InputText.Replace('0', '#');
                    InputText = InputText.Replace('1', '_');
                    if (_settings.UnderscoreisSpace)
                    {
                        InputText = InputText.Replace(' ', '_');
                    }
                    OutputText = Encrypt(InputText, key);
                    break;
                case Action.Decrypt:
                    OutputText = Decrypt(InputText.ToUpper(), key);
                    if (_settings.UnderscoreisSpace)
                    {
                        OutputText = OutputText.Replace('_', ' ');
                    }
                    break;
            }

            OnPropertyChanged(nameof(OutputText));

            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Encrypts the given plaintext
        /// </summary>
        private string Encrypt(string plaintext, string key)
        {
            int[,] stateMatrix = KeySchedule(key);
            StringBuilder ciphertextBuilder = new StringBuilder();

            int i = 0, j = 0;
            foreach (char plaintextSymbol in plaintext)
            {
                int P = _alphabet.IndexOf(plaintextSymbol);

                if (P == -1)
                {
                    //handle unknown symbols
                    switch (_settings.UnknownSymbolHandling)
                    {
                        case UnknownSymbolHandlingMode.Ignore:
                            ciphertextBuilder.Append(plaintextSymbol);
                            break;
                        case UnknownSymbolHandlingMode.Remove:
                            //do nothing;
                            break;
                        case UnknownSymbolHandlingMode.Replace:
                            ciphertextBuilder.Append("?");
                            break;
                    }
                    continue;
                }

                (int r, int c) = GetPositionOfLetter(P, stateMatrix);
                int x = (r + (stateMatrix[i, j] / 6)) % 6;
                int y = (c + (stateMatrix[i, j] % 6)) % 6;
                int C = stateMatrix[x, y];

                ciphertextBuilder.Append(_alphabet[C]);

                RightRotate(stateMatrix, ref r, ref c, ref x, ref y, ref i, ref j);
                DownRotate(stateMatrix, ref r, ref c, ref x, ref y, ref i, ref j);

                i = (i + (C / 6)) % 6;
                j = (j + (C % 6)) % 6;
            }
            return ciphertextBuilder.ToString();
        }

        /// <summary>
        /// Decrypts the given ciphertext
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <returns></returns>
        private string Decrypt(string ciphertext, string key)
        {
            int[,] stateMatrix = KeySchedule(key);
            StringBuilder plaintextBuilder = new StringBuilder();

            int i = 0, j = 0;
            foreach (char ciphertextSymbol in ciphertext)
            {
                int C = _alphabet.IndexOf(ciphertextSymbol);

                if (C == -1)
                {
                    //handle unknown symbols
                    switch (_settings.UnknownSymbolHandling)
                    {
                        case UnknownSymbolHandlingMode.Ignore:
                            plaintextBuilder.Append(ciphertextSymbol);
                            break;
                        case UnknownSymbolHandlingMode.Remove:
                            //do nothing;
                            break;
                        case UnknownSymbolHandlingMode.Replace:
                            plaintextBuilder.Append("?");
                            break;
                    }
                    continue;
                }

                (int x, int y) = GetPositionOfLetter(C, stateMatrix);
                int r = Mod(x - (stateMatrix[i, j] / 6), 6);
                int c = Mod(y - (stateMatrix[i, j] % 6), 6);
                int P = stateMatrix[r, c];

                plaintextBuilder.Append(_alphabet[P]);

                RightRotate(stateMatrix, ref r, ref c, ref x, ref y, ref i, ref j);
                DownRotate(stateMatrix, ref r, ref c, ref x, ref y, ref i, ref j);

                i = (i + (C / 6)) % 6;
                j = (j + (C % 6)) % 6;
            }
            return plaintextBuilder.ToString();
        }

        /// <summary>
        /// Initializes a state matrix using the given 36-letter permutated key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static int[,] KeySchedule(string key)
        {
            int[,] stateMatrix = new int[6, 6];
            for (int k = 0; k < 36; k++)
            {
                stateMatrix[k / 6, k % 6] = _alphabet.IndexOf(key[k]);
            }
            return stateMatrix;
        }

        /// <summary>
        /// Returns Coordinates of the given Letter
        /// </summary>
        /// <param name="c"></param>
        /// <param name="stateMatrix"></param>
        /// <returns></returns>
        private static (int row, int colum) GetPositionOfLetter(int letter, int[,] stateMatrix)
        {
            for (int row = 0; row < 6; row++)
            {
                for (int column = 0; column < 6; column++)
                {
                    if (stateMatrix[row, column] == letter)
                    {
                        return (row, column);
                    }
                }
            }
            return (-1, -1);
        }

        /// <summary>
        /// Rotates row y
        /// </summary>
        /// <param name="stateMatrix"></param>
        /// <param name="r"></param>
        /// <param name="c"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        private static void RightRotate(int[,] stateMatrix, ref int r, ref int c, ref int x, ref int y, ref int i, ref int j)
        {
            (stateMatrix[r, 0], stateMatrix[r, 1], stateMatrix[r, 2], stateMatrix[r, 3], stateMatrix[r, 4], stateMatrix[r, 5]) =
                (stateMatrix[r, 5], stateMatrix[r, 0], stateMatrix[r, 1], stateMatrix[r, 2], stateMatrix[r, 3], stateMatrix[r, 4]);
            c = (c + 1) % 6;
            if (x == r) y = (y + 1) % 6;
            if (i == r) j = (j + 1) % 6;
        }

        /// <summary>
        /// Rotates column c
        /// </summary>
        /// <param name="stateMatrix"></param>
        /// <param name="r"></param>
        /// <param name="c"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        private static void DownRotate(int[,] stateMatrix, ref int r, ref int c, ref int x, ref int y, ref int i, ref int j)
        {
            (stateMatrix[0, y], stateMatrix[1, y], stateMatrix[2, y], stateMatrix[3, y], stateMatrix[4, y], stateMatrix[5, y]) =
                (stateMatrix[5, y], stateMatrix[0, y], stateMatrix[1, y], stateMatrix[2, y], stateMatrix[3, y], stateMatrix[4, y]);
            //The following two lines make no sense, since values of x and row are overwritten at the beginning of the main loop.
            //But these lines are part of the original specification of the cipher:
            //x = (x + 1) % 6;
            //if(column == y) row = (row + 1) % 6;
            if (j == y) i = (i + 1) % 6;
        }

        /// <summary>
        /// Computes math modulo
        /// </summary>
        /// <param name="i"></param>
        /// <param name="mod"></param>
        /// <returns></returns>
        private static int Mod(int i, int mod)
        {
            return ((i % mod) + mod) % mod;
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
