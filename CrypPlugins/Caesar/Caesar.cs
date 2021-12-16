/*                              
   Copyright 2009-2012 Arno Wacker, University of Kassel

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
using System.Windows.Controls;

namespace CrypTool.Caesar
{
    [Author("Arno Wacker", "arno.wacker@CrypTool.org", "Universität Kassel", "http://www.uc.uni-kassel.de")]
    [PluginInfo("CrypTool.Caesar.Properties.Resources", "PluginCaption", "PluginTooltip", "Caesar/DetailedDescription/doc.xml",
        new[] { "Caesar/Images/Caesar.png", "Caesar/Images/encrypt.png", "Caesar/Images/decrypt.png" })]
    [ComponentCategory(ComponentCategory.CiphersClassic)]
    public class Caesar : ICrypComponent
    {
        #region Private elements

        private readonly CaesarSettings settings;
        private string _inputString;
        private bool isRunning;

        #endregion

        #region Public interface

        /// <summary>
        /// Constructor
        /// </summary>
        public Caesar()
        {
            settings = new CaesarSettings();
            settings.LogMessage += GuiLogMessage;
        }

        /// <summary>
        /// Get or set all settings for this algorithm.
        /// </summary>
        public ISettings Settings => settings;

        [PropertyInfo(Direction.InputData, "InputStringCaption", "InputStringTooltip", true)]
        public string InputString
        {
            get => _inputString;
            set => _inputString = value;
        }

        [PropertyInfo(Direction.OutputData, "OutputStringCaption", "OutputStringTooltip", false)]
        public string OutputString
        {
            get;
            set;
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

        [PropertyInfo(Direction.InputData, "ShiftKeyCaption", "ShiftKeyTooltip", false)]
        public int ShiftKey
        {
            get => settings.ShiftKey;
            set
            {
                if (isRunning)
                {
                    settings.SetKeyByValue(value, false);
                }
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
        /// Fires events to indicate progress bar changes.
        /// </summary>
        public event PluginProgressChangedEventHandler OnPluginProgressChanged;
        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        /// <summary>
        /// Fires events to indicate log messages.
        /// </summary>
        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;
        private void GuiLogMessage(string p, NotificationLevel notificationLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(p, this, notificationLevel));
        }

        /// <summary>
        /// No algorithm visualization
        /// </summary>
        public UserControl Presentation => null;

        public void Stop()
        {
            isRunning = false;
        }

        public void PostExecution()
        {
            isRunning = false;
        }

        public void PreExecution()
        {
            isRunning = true;
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion


        #region IPlugin Members

#pragma warning disable 67
        public event StatusChangedEventHandler OnPluginStatusChanged;
#pragma warning restore

        public void Execute()
        {
            StringBuilder output = new StringBuilder();

            // If we are working in case-insensitive mode, we will use only
            // capital letters, hence we must transform the whole alphabet
            // to uppercase.
            string alphabet = settings.CaseSensitive ? settings.AlphabetSymbols : settings.AlphabetSymbols.ToUpper();

            if (!string.IsNullOrEmpty(InputString))
            {
                for (int i = 0; i < InputString.Length; i++)
                {
                    // Get the plaintext char currently being processed.
                    char currentchar = InputString[i];

                    // Store whether it is upper case.
                    bool uppercase = char.IsUpper(currentchar);

                    // Get the position of the plaintext character in the alphabet.
                    int ppos = alphabet.IndexOf(settings.CaseSensitive ? currentchar : char.ToUpper(currentchar));

                    if (ppos >= 0)
                    {
                        // We found the plaintext character in the alphabet,
                        // hence we will commence shifting.
                        int cpos = 0; ;
                        switch (settings.Action)
                        {
                            case CaesarSettings.CaesarMode.Encrypt:
                                cpos = (ppos + settings.ShiftKey) % alphabet.Length;
                                break;
                            case CaesarSettings.CaesarMode.Decrypt:
                                cpos = (ppos - settings.ShiftKey + alphabet.Length) % alphabet.Length;
                                break;
                        }

                        // We have the position of the ciphertext character,
                        // hence just output it in the correct case.
                        if (settings.CaseSensitive)
                        {
                            output.Append(alphabet[cpos]);
                        }
                        else
                        {
                            output.Append(uppercase ? char.ToUpper(alphabet[cpos]) : char.ToLower(alphabet[cpos]));
                        }

                    }
                    else
                    {
                        // The plaintext character was not found in the alphabet,
                        // hence proceed with handling unknown characters.
                        switch (settings.UnknownSymbolHandling)
                        {
                            case CaesarSettings.UnknownSymbolHandlingMode.Ignore:
                                output.Append(InputString[i]);
                                break;
                            case CaesarSettings.UnknownSymbolHandlingMode.Replace:
                                output.Append('?');
                                break;
                        }
                    }

                    // Show the progress.
                    ProgressChanged(i, InputString.Length - 1);

                }
                OutputString = settings.CaseSensitive | settings.MemorizeCase ? output.ToString() : output.ToString().ToUpper();
                OnPropertyChanged("OutputString");
            }
        }

        #endregion

    }
}
