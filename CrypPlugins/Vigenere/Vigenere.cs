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
using System.ComponentModel;
using System.Text;

namespace CrypTool.Vigenere
{
    [Author("Sebastian Przybylski", "sebastian@przybylski.org", "Uni Siegen", "http://www.uni-siegen.de")]
    [PluginInfo("CrypTool.Vigenere.Properties.Resources", "PluginCaption", "PluginTooltip", "Vigenere/DetailedDescription/doc.xml",
      "Vigenere/Images/icon.png", "Vigenere/Images/encrypt.png", "Vigenere/Images/decrypt.png")]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class Vigenere : ICrypComponent
    {
        #region Private variables

        private VigenereSettings settings;
        private string inputString;
        private string outputString;
        private enum VigenereMode { encrypt, decrypt, autoencrypt, autodecrypt };

        private readonly string topAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        #endregion

        #region Public interface

        /// <summary>
        /// Constructor
        /// </summary>
        public Vigenere()
        {
            settings = new VigenereSettings();
            settings.LogMessage += Vigenere_LogMessage;
        }

        /// <summary>
        /// Get or set all settings for this algorithm
        /// </summary>
        public ISettings Settings
        {
            get => settings;
            set => settings = (VigenereSettings)value;
        }

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

        [PropertyInfo(Direction.InputData, "InputAlphabetCaption", "InputAlphabetTooltip", false)]
        public string AlphabetSymbols
        {
            get => settings.AlphabetSymbols;
            set
            {
                if (value != null && value != settings.AlphabetSymbols)
                {
                    settings.AlphabetSymbols = value;
                    OnPropertyChanged("AlphabetSymbols");
                }
            }
        }
        [PropertyInfo(Direction.InputData, "ShiftValueCaption", "ShiftValueTooltip", false)]
        public string ShiftValue
        {
            get => settings.ShiftChar;
            set
            {
                if (value != settings.ShiftChar)
                {
                    settings.ShiftChar = value;
                    OnPropertyChanged("ShiftValue");

                }
            }
        }

        /// <summary>
        /// Vigenere encryption
        /// </summary>
        public void Encrypt()
        {
            ProcessVigenere(VigenereMode.encrypt);
        }

        public void AutoKeyEncrypt()
        {
            ProcessVigenere(VigenereMode.autoencrypt);
        }

        /// <summary>
        /// Vigenere decryption
        /// </summary>
        public void Decrypt()
        {
            ProcessVigenere(VigenereMode.decrypt);
        }

        public void AutoKeyDecrypt()
        {
            ProcessVigenere(VigenereMode.autodecrypt);
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
        /// Fire if progress bar status was changed
        /// </summary>
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        private void Progress(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        /// <summary>
        /// Fire if a new message has to be shown in the status bar
        /// </summary>
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public System.Windows.Controls.UserControl Presentation => null;

        public void Stop()
        {
        }

        public void PostExecution()
        {
        }

        public void PreExecution()
        {
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion



        #region Private methods

        /// <summary>
        /// Does the actual Vigenere processing, i.e. encryption or decryption
        /// </summary>
        /// <param name="mode"></param>
        private void ProcessVigenere(VigenereMode mode)
        {
            VigenereSettings cfg = settings;
            StringBuilder output = new StringBuilder(string.Empty);
            string alphabet = cfg.AlphabetSymbols;
            int autopos = 0;

            if (!cfg.CaseSensitiveAlphabet)
            {
                alphabet = cfg.AlphabetSymbols.ToUpper();
            }
            if (inputString != null)
            {
                int shiftPos = 0;
                for (int i = 0; i < inputString.Length; i++)
                {
                    //get plaintext char which is currently processed
                    char currentChar = inputString[i];

                    //remember if it is upper case (otherwise lowercase is assumed)
                    bool uppercase = char.IsUpper(currentChar);

                    //get the position of the plaintext character in the alphabet
                    int ppos = alphabet.IndexOf((cfg.CaseSensitiveAlphabet) ? currentChar : char.ToUpper(currentChar));

                    if (ppos >= 0)
                    {
                        //found the plaintext character in the alphabet, begin shifting
                        int cpos = 0;
                        switch (mode)
                        {
                            case VigenereMode.encrypt:

                                cpos = (ppos + cfg.ShiftKey[shiftPos]) % alphabet.Length;

                                //increment shiftPos to map inputString whith all keys
                                //if shiftPos > ShiftKey.Length, begin again at the beginning
                                shiftPos++;
                                if (shiftPos >= cfg.ShiftKey.Length)
                                {
                                    shiftPos = 0;
                                }

                                break;

                            case VigenereMode.decrypt:

                                cpos = (ppos - cfg.ShiftKey[shiftPos] + alphabet.Length) % alphabet.Length;

                                //increment shiftPos to map inputString whith all keys
                                //if shiftPos > ShiftKey.Length, begin again at the beginning
                                shiftPos++;
                                if (shiftPos >= cfg.ShiftKey.Length)
                                {
                                    shiftPos = 0;
                                }

                                break;

                            case VigenereMode.autoencrypt:

                                //key still used
                                if (shiftPos < cfg.ShiftKey.Length)
                                {
                                    cpos = (ppos + cfg.ShiftKey[shiftPos]) % alphabet.Length;
                                    shiftPos++;
                                }
                                else //using plaintext
                                {
                                    //taking the plaintextchar from the next position
                                    int pkey = alphabet.IndexOf(char.ToUpper(inputString[autopos]));
                                    //check if the next plaintextchar is in the alphabet
                                    while (pkey < 0)
                                    {
                                        autopos++;
                                        pkey = alphabet.IndexOf(char.ToUpper(inputString[autopos]));
                                    }

                                    cpos = (ppos + pkey) % alphabet.Length;
                                    autopos++;
                                }
                                break;


                            case VigenereMode.autodecrypt:

                                //key still used
                                if (shiftPos < cfg.ShiftKey.Length)
                                {
                                    cpos = (ppos - cfg.ShiftKey[shiftPos] + alphabet.Length) % alphabet.Length;
                                    shiftPos++;
                                }
                                else //using plaintext
                                {
                                    outputString = output.ToString();

                                    //taking the deciphered plaintextchar from the next position
                                    int pkey = alphabet.IndexOf(char.ToUpper(outputString[autopos]));
                                    //check if the next deciphered plaintextchar is in the alphabet
                                    while (pkey < 0)
                                    {
                                        autopos++;
                                        try
                                        {
                                            pkey = alphabet.IndexOf(char.ToUpper(outputString[autopos]));
                                        }
                                        catch
                                        {
                                            //there is an internal failure that doesn't make sense
                                            //supposly it has something to do with the threads -.-'/
                                        }
                                    }

                                    cpos = (ppos - pkey + alphabet.Length) % alphabet.Length;
                                    autopos++;
                                }
                                break;
                        }


                        //we have the position of the ciphertext character, now we have to output it in the right case
                        char c = alphabet[cpos];
                        if (!cfg.CaseSensitiveAlphabet)
                        {
                            c = uppercase ? char.ToUpper(c) : char.ToLower(c);
                        }

                        output.Append(c);
                    }
                    else
                    {
                        //the plaintext character was not found in the alphabet, begin handling with unknown characters
                        switch ((VigenereSettings.UnknownSymbolHandlingMode)cfg.UnknownSymbolHandling)
                        {
                            case VigenereSettings.UnknownSymbolHandlingMode.Ignore:
                                output.Append(inputString[i]);
                                break;
                            case VigenereSettings.UnknownSymbolHandlingMode.Replace:
                                output.Append('?');
                                break;
                        }
                    }

                    //show the progress
                    Progress(i, inputString.Length - 1);
                }

                outputString = settings.AlphabetCase | settings.MemorizeCase ? output.ToString() : output.ToString().ToUpper();
                OnPropertyChanged("OutputString");
            }
        }

        /// <summary>
        /// Handles log messages from the settings class
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        /// <param name="logeLevel"></param>
        private void Vigenere_LogMessage(string msg, NotificationLevel logLevel)
        {
            if (OnGuiLogNotificationOccured != null)
            {
                OnGuiLogNotificationOccured(this, new GuiLogEventArgs(msg, this, logLevel));
            }
        }

        #endregion

        #region IPlugin Members

#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore

        public void Execute()
        {
            switch (settings.Mode)
            {
                //Classic Mode
                case 0:

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
                    break;

                //Autokey Mode
                case 1:

                    switch (settings.Action)
                    {
                        case 0:
                            AutoKeyEncrypt();
                            break;
                        case 1:
                            AutoKeyDecrypt();
                            break;
                        default:
                            break;
                    }
                    break;
            }

        }

        #endregion

    }
}
