/*
   Copyright 2024 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace CrypTool.Plugins.CryptoBoxCipher
{
    /// <summary>
    /// Cipher implementation of the CryptoBoxCipher algorithm as defined by Dirk Rijmenants (see https://www.ciphermachinesandcryptology.com/en/cryptobox.htm)
    /// </summary>
    [Author("Nils Kopal", "kopal@CrypTool.org", "CrypTool 2 Team", "https://www.CrypTool.org")]
    [PluginInfo("CrypTool.Plugins.CryptoBoxCipher.Properties.Resources", "PluginCaption", "PluginTooltip", "CryptoBoxCipher/userdoc.xml", new[] { "CryptoBoxCipher/icon.png" })]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class CryptoBoxCipher : ICrypComponent
    {
        #region Private Variables

        private readonly CryptoBoxCipherSettings _settings = new CryptoBoxCipherSettings();
        private bool _stop = false;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "TextInputCaption", "TextInputTooltip", true)]
        public string TextInput
        {
            get;
            set;
        }


        [PropertyInfo(Direction.InputData, "KeyInputCaption", "KeyInputTooltip", true)]
        public string KeyInput
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "TextOutputCaption", "TextOutputTooltip")]
        public string TextOutput
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings
        {
            get { return _settings; }
        }

        public UserControl Presentation
        {
            get { return null; }
        }

        public void PreExecution()
        {
        }

        /// <summary>
        /// Executes the CryptoBoxCipher algorithm
        /// </summary>
        public void Execute()
        {
            _stop = false;
            switch (_settings.Action)
            {
                case Action.Encrypt:
                    TextOutput = Crypt(TextInput, KeyInput, true);
                    break;
                case Action.Decrypt:
                    TextOutput = Crypt(TextInput, KeyInput, false);
                    break;
            }
            if (TextOutput != null) // if we have null, an error occured or the user pressed stop, so we don't want to show the output
            {
                OnPropertyChanged(nameof(TextOutput));
            }
        }

        /// <summary>
        /// Encrypts the plaintext or decrypts the ciphertext using the given key with the CryptoBoxCipher algorithm
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private string Crypt(string inputText, string key, bool encrypt)
        {
            //clean text, only leave letters, digits, full stops, commas, exclamations and question marks, colons, semi-colons, and spaces
            //also convert to uppercase string
            inputText = new string(inputText.Where(c => char.IsLetterOrDigit(c) || c == '.' || c == ',' || c == '!' || c == '?' || c == ':' || c == ';' || c == ' ').ToArray()).ToUpper();

            //space handling of input text
            switch (_settings.SpaceHandling)
            {
                case SpaceHandling.ReplaceX:
                    inputText = inputText.Replace(' ', 'X');
                    break;
                case SpaceHandling.Remove:
                    inputText = inputText.Replace(" ", "");
                    break;
                default:
                    //do nothing
                    break;
            }

            string[,] state = new string[10, 10];
            StringBuilder outputTextBuilder = new StringBuilder();

            //cut textInput into 100-character chunks
            List<string> chunks = new List<string>();
            for (int i = 0; i < inputText.Length; i += 100)
            {
                chunks.Add(inputText.Substring(i, System.Math.Min(100, inputText.Length - i)));
            }

            //read the key into a list of strings, each length 2
            List<string> keyElements = new List<string>();
            key = key.ToUpper();
            if (key.Length % 2 != 0)
            {
                GuiLogMessage(Properties.Resources.Keymusthaveanevennumberofcharacters, NotificationLevel.Error);
                return null;
            }
            for (int keyOffset = 0; keyOffset < key.Length; keyOffset += 2)
            {
                //read in key element character
                char keyElement = char.ToUpper(key[keyOffset]);
                if (keyElement < '0' && keyElement > '9' && keyElement < 'A' && keyElement > 'J')
                {
                    GuiLogMessage(string.Format(Properties.Resources.Invalidkeyelementcharacter0inkeyatoffset1, keyElement, keyOffset), NotificationLevel.Error);
                    return null;
                }
                //read in shift direction
                char direction = key[keyOffset + 1];
                if (direction != 'L' && direction != 'R' && direction != 'U' && direction != 'D')
                {
                    GuiLogMessage(string.Format(Properties.Resources.Invaliddirectioncharacter0inkeyatoffset1, direction, keyOffset), NotificationLevel.Error);
                    return null;
                }
                //add to key list
                keyElements.Add(keyElement.ToString() + direction.ToString());
            }

            //now encrypt each chunk
            foreach (string chunk in chunks)
            {
                if(_stop) //user pressed stop button, so we stop the execution
                {
                    return null;
                }

                //initialize state with symbol defined in settings
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        switch (_settings.Padding)
                        {
                            default:
                            case Padding.X:
                                state[i, j] = "X";
                                break;
                            case Padding.SPACE:
                                state[i, j] = " ";
                                break;
                        }
                    }
                }

                //fill state with chunk
                for (int i = 0; i < chunk.Length; i++)
                {
                    state[i / 10, i % 10] = chunk[i].ToString();
                }

                //initialize keyElementOffset
                //with encrypt, we start at the beginning of the key and go forward through the key
                //with decrypt, we start at the end of the key and go reversed through the key
                int keyElementOffset;
                if (encrypt)
                {
                    keyElementOffset = 0;
                }
                else
                {
                    keyElementOffset = keyElements.Count - 1;
                }

                //process key elements
                bool endOfKeyReached = false;
                while (!endOfKeyReached)
                {
                    char keyLetter = keyElements[keyElementOffset][0];
                    char direction = keyElements[keyElementOffset][1];

                    //check direction and move
                    if (keyLetter >= '0' && keyLetter <= '9')
                    {
                        //check if direction is valid
                        if (direction != 'L' && direction != 'R')
                        {
                            GuiLogMessage(string.Format(Properties.Resources.Invaliddirectioncharacter0inkeyatoffset1ValidforroshiftsareLorR, direction, keyElementOffset), NotificationLevel.Error);
                            return null;
                        }

                        int row = keyLetter - '0';
                        // shift row to the left
                        if ((direction == 'L' && encrypt) || (direction == 'R' && !encrypt))
                        {
                            //we use tuple swapping to shift the row
                            (state[row, 0], state[row, 1], state[row, 2], state[row, 3], state[row, 4],
                             state[row, 5], state[row, 6], state[row, 7], state[row, 8], state[row, 9]) =
                            (state[row, 1], state[row, 2], state[row, 3], state[row, 4], state[row, 5],
                             state[row, 6], state[row, 7], state[row, 8], state[row, 9], state[row, 0]);
                        }
                        // shift row to the right
                        else
                        {
                            //we use tuple swapping to shift the row
                            (state[row, 1], state[row, 2], state[row, 3], state[row, 4], state[row, 5],
                             state[row, 6], state[row, 7], state[row, 8], state[row, 9], state[row, 0]) =
                            (state[row, 0], state[row, 1], state[row, 2], state[row, 3], state[row, 4],
                             state[row, 5], state[row, 6], state[row, 7], state[row, 8], state[row, 9]);
                        }
                    }
                    else if (keyLetter >= 'A' && keyLetter <= 'J')
                    {
                        //check if direction is valid
                        if (direction != 'U' && direction != 'D')
                        {
                            GuiLogMessage(string.Format(Properties.Resources.Invaliddirectioncharacter0inkeyatoffset1ValidforcolumnshiftsareUandD, direction, keyElementOffset), NotificationLevel.Error);
                            return null;
                        }

                        int column = keyLetter - 'A';
                        // shift column up
                        if ((direction == 'U' && encrypt) || (direction == 'D' && !encrypt))
                        {
                            //we use tuple swapping to shift the column
                            (state[0, column], state[1, column], state[2, column], state[3, column], state[4, column],
                             state[5, column], state[6, column], state[7, column], state[8, column], state[9, column]) =
                            (state[1, column], state[2, column], state[3, column], state[4, column], state[5, column],
                             state[6, column], state[7, column], state[8, column], state[9, column], state[0, column]);

                        }
                        //shift column down
                        else
                        {
                            //we use tuple swapping to shift the column
                            (state[1, column], state[2, column], state[3, column], state[4, column], state[5, column],
                             state[6, column], state[7, column], state[8, column], state[9, column], state[0, column]) =
                            (state[0, column], state[1, column], state[2, column], state[3, column], state[4, column],
                             state[5, column], state[6, column], state[7, column], state[8, column], state[9, column]);
                        }
                    }

                    //move keyElementOffset to next key element
                    if (encrypt)
                    {
                        //while encrypting, we move forward through the key
                        keyElementOffset++;
                        if (keyElementOffset == keyElements.Count)
                        {
                            endOfKeyReached = true;
                        }
                    }
                    else
                    {
                        //while decrypting, we move backwards through the key
                        keyElementOffset--;
                        if (keyElementOffset == -1)
                        {
                            endOfKeyReached = true;
                        }
                    }
                }

                //write encrypted chunk to ciphertextBuilder
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        outputTextBuilder.Append(state[i, j]);
                    }
                }
            }
            return outputTextBuilder.ToString();
        }

        public void PostExecution()
        {
        }

        /// <summary>
        /// Called when the user presses the stop button
        /// </summary>
        public void Stop()
        {
            _stop = true;
        }

        public void Initialize()
        {
        }

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