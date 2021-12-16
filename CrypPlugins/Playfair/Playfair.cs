/*
   Copyright 2008 Sebastian Przybylski, University of Siegen

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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace CrypTool.Playfair
{
    [Author("Sebastian Przybylski", "sebastian@przybylski.org", "Uni Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("Playfair.Properties.Resources", "PluginCaption", "PluginTooltip", "Playfair/DetailedDescription/doc.xml",
      "Playfair/Images/icon.png", "Playfair/Images/encrypt.png", "Playfair/Images/decrypt.png")]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class Playfair : ICrypComponent
    {
        #region Private variables

        private readonly PlayfairSettings settings;
        private string inputString;
        private string keyPhraseString;
        private string outputString;
        private string keyString;
        private string preFormatedInputString;
        private int matrixSize;

        #endregion

        #region Public interface

        /// <summary>
        /// Constructor
        /// </summary>
        public Playfair()
        {
            settings = new PlayfairSettings();
        }

        /// <summary>
        /// Get or set all settings for this algorithm
        /// </summary>
        public ISettings Settings => settings;

        [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip", true)]
        public string InputString
        {
            get => inputString;
            set
            {
                if (value != inputString)
                {
                    inputString = value;
                    OnPropertyChanged("InputString");
                }
            }
        }

        [PropertyInfo(Direction.InputData, "KeyCaption", "KeyTooltip", false)]
        public string KeyPhraseString
        {
            get => keyPhraseString;
            set
            {
                if (value != keyPhraseString)
                {
                    keyPhraseString = value;
                    OnPropertyChanged("KeyPhraseString");
                }
            }
        }

        [PropertyInfo(Direction.OutputData, "PreFormatedInputStringCaption", "PreFormatedInputStringTooltip", false)]
        public string PreFormatedInputString
        {
            get => preFormatedInputString;
            set
            {
                preFormatedInputString = value;
                OnPropertyChanged("PreFormatedInputString");
            }
        }

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", false)]
        public string OutputString
        {
            get => outputString;
            set
            {
                outputString = value;
                OnPropertyChanged("OutputString");
            }
        }

        [PropertyInfo(Direction.OutputData, "AlphabetMatrixCaption", "AlphabetMatrixTooltip", false)]
        public string KeyString
        {
            get => keyString;
            set
            {
                keyString = value;
                OnPropertyChanged("KeyString");
            }
        }

        /// <summary>
        /// Playfair encryption
        /// </summary>
        public void Encrypt()
        {
            if (inputString != null)
            {
                StringBuilder output = new StringBuilder();
                //set selected matrix size
                if (settings.MatrixSize == 0)
                {
                    matrixSize = 5;
                }
                else
                {
                    matrixSize = 6;
                }

                //pre-format input text, if user activated this property
                if (settings.PreFormatText)
                {
                    preFormatedInputString = preFormatText();
                }
                else
                {
                    preFormatedInputString = inputString;
                }

                OnPropertyChanged("PreFormatedInputString");

                //begin the encryption
                for (int i = 0; i < preFormatedInputString.Length - 1; i++)
                {
                    int indexCh1 = KeyString.IndexOf(preFormatedInputString[i]);
                    i++;
                    int indexCh2 = KeyString.IndexOf(preFormatedInputString[i]);

                    //first, get new char index from cipher alphabet
                    int newIndexCh1;
                    int newIndexCh2;

                    int rowCh1 = indexCh1 / matrixSize;
                    int rowCh2 = indexCh2 / matrixSize;
                    int colCh1 = indexCh1 % matrixSize;
                    int colCh2 = indexCh2 % matrixSize;

                    if (rowCh1 == rowCh2)
                    {
                        newIndexCh1 = getRightNeighbour(indexCh1);
                        newIndexCh2 = getRightNeighbour(indexCh2);
                    }
                    else if (colCh1 == colCh2)
                    {
                        newIndexCh1 = getLowerNeighbour(indexCh1);
                        newIndexCh2 = getLowerNeighbour(indexCh2);
                    }
                    else
                    {
                        newIndexCh1 = getSubstitute(rowCh1, colCh2);
                        newIndexCh2 = getSubstitute(rowCh2, colCh1);
                    }
                    output.Append(KeyString[newIndexCh1]);
                    output.Append(KeyString[newIndexCh2]);
                }
                outputString = output.ToString();
                OnPropertyChanged("OutputString");
            }
        }

        /// <summary>
        /// Playfair decryption
        /// </summary>
        public void Decrypt()
        {
            if (inputString != null)
            {
                StringBuilder output = new StringBuilder();
                //set selected matrix size
                if (settings.MatrixSize == 0)
                {
                    matrixSize = 5;
                }
                else
                {
                    matrixSize = 6;
                }

                // Decryption does not require preformat, otherwise the ciphertext format is wrong
                // We attempt to preformat nevertheless.

                //pre-format input text, if user activated this property
                if (settings.PreFormatText)
                {
                    preFormatedInputString = preFormatText();
                }
                else
                {
                    preFormatedInputString = inputString;
                }

                OnPropertyChanged("PreFormatedInputString");

                //begin the encryption
                for (int i = 0; i < preFormatedInputString.Length - 1; i++)
                {
                    int indexCh1 = KeyString.IndexOf(preFormatedInputString[i]);
                    i++;
                    int indexCh2 = KeyString.IndexOf(preFormatedInputString[i]);

                    //first, get new char index from cipher alphabet
                    int newIndexCh1;
                    int newIndexCh2;

                    int rowCh1 = indexCh1 / matrixSize;
                    int rowCh2 = indexCh2 / matrixSize;
                    int colCh1 = indexCh1 % matrixSize;
                    int colCh2 = indexCh2 % matrixSize;

                    if (rowCh1 == rowCh2)
                    {
                        newIndexCh1 = getLeftNeighbour(indexCh1);
                        newIndexCh2 = getLeftNeighbour(indexCh2);
                    }
                    else if (colCh1 == colCh2)
                    {
                        newIndexCh1 = getUpperNeighbour(indexCh1);
                        newIndexCh2 = getUpperNeighbour(indexCh2);
                    }
                    else
                    {
                        newIndexCh1 = getSubstitute(rowCh1, colCh2);
                        newIndexCh2 = getSubstitute(rowCh2, colCh1);
                    }
                    output.Append(KeyString[newIndexCh1]);
                    output.Append(KeyString[newIndexCh2]);
                }
                outputString = output.ToString();
                OnPropertyChanged("OutputString");
            }
        }


        #endregion

        #region IPlugin members

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Fire, if status has to be shown in the progress bar
        /// </summary>
#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore


        /// <summary>
        /// Fire, if a message has to be shonw in the status bar
        /// </summary>
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public UserControl Presentation => null;

        public void Stop()
        {
        }

        public void PreExecution()
        {
        }

        public void PostExecution()
        {
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        #region Private methods

        private int getRightNeighbour(int index)
        {
            if (index % matrixSize < matrixSize - 1)
            {
                index++;
            }
            else if (index % matrixSize == matrixSize - 1)
            {
                index = index - matrixSize + 1;
            }
            else
            {
                index = -1;
            }

            return index;
        }

        private int getLowerNeighbour(int index)
        {
            if (index + matrixSize < KeyString.Length)
            {
                index = index + matrixSize;
            }
            else
            {
                index = (index + matrixSize) % KeyString.Length;
            }

            return index;
        }

        private int getUpperNeighbour(int index)
        {
            if (index < matrixSize)
            {
                index = KeyString.Length - (matrixSize - index);
            }
            else
            {
                index = index - matrixSize;
            }

            return index;
        }

        private int getLeftNeighbour(int index)
        {
            if (index % matrixSize > 0)
            {
                index--;
            }
            else
            {
                index = index + matrixSize - 1;
            }

            return index;
        }

        private int getSubstitute(int row, int col)
        {
            return matrixSize * row + col;
        }

        private string preFormatText()
        {
            StringBuilder sb = new StringBuilder();

            //remove or replace nonalphabet char
            for (int i = 0; i < inputString.Length; i++)
            {
                char c = char.ToUpper(inputString[i]);

                if (KeyString.Contains(c))
                {
                    sb.Append(c);
                }
                else
                {
                    if (c == 'J')
                    {
                        sb.Append("I");
                    }

                    if (c == 'Ä')
                    {
                        sb.Append("AE");
                    }

                    if (c == 'Ö')
                    {
                        sb.Append("OE");
                    }

                    if (c == 'Ü')
                    {
                        sb.Append("UE");
                    }

                    if (c == 'ß')
                    {
                        sb.Append("SS");
                    }
                }
            }

            //if separate char is enabled begin with separating
            if (settings.SeparatePairs)
            {
                for (int i = 0; i <= sb.Length - 2; i += 2)
                {
                    if (sb[i] == sb[i + 1]) // same chars, insert X
                    {
                        if (sb[i] == settings.Separator) // avoid XX, use XY instead
                        {
                            sb.Insert(i + 1, settings.SeparatorReplacement);
                        }
                        else
                        {
                            sb.Insert(i + 1, settings.Separator);
                        }
                    }
                }
            }

            // does the input end with a single letter?
            if (sb.Length % 2 != 0)
            {
                if (sb[sb.Length - 1] == settings.Separator) // avoid XX, use XY instead
                {
                    sb.Append(settings.SeparatorReplacement);
                }
                else
                {
                    sb.Append(settings.Separator);
                }
            }

            return sb.ToString();
        }

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        #endregion

        #region IPlugin Members

#pragma warning disable 67
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
#pragma warning restore

        public void Execute()
        {
            ProgressChanged(0, 1);

            if (!string.IsNullOrWhiteSpace(KeyPhraseString))
            {
                KeyString = PlayfairKey.CreateKey(KeyPhraseString, settings.MatrixSize, settings.IgnoreDuplicates);
            }
            else
            {
                KeyString = settings.KeyString;
            }

            if (!IsKeyValid(KeyString))
            {
                GuiLogMessage("Invalid playfair key.", NotificationLevel.Error);
                return;
            }

            switch (settings.Action)
            {
                case 0:
                    Encrypt();
                    break;
                case 1:
                    Decrypt();
                    break;
                default:
                    break;
            }

            ProgressChanged(1, 1);
        }

        /// <summary>
        /// Checks if the passed key parameter is a valid playfair key (square).
        /// </summary>
        private bool IsKeyValid(string key)
        {
            if (key == null)
            {
                return false;
            }

            //Check if key contains unique characters.
            if (key.GroupBy(c => c).Any(g => g.Count() > 1))
            {
                return false;
            }

            //Check size:
            int expectedSize;
            switch (settings.MatrixSize)
            {
                case PlayfairKey.MatrixSize.Five_Five:
                    expectedSize = 5 * 5;
                    break;
                case PlayfairKey.MatrixSize.Six_Six:
                    expectedSize = 6 * 6;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return key.Length == expectedSize;
        }

        #endregion

        #region Event Handling

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion
    }
}
