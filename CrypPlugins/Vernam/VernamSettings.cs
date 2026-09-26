/*
   Copyright (C) CrypTool 2 Team

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

namespace CrypTool.Plugins.Vernam
{

    public class VernamSettings : ISettings
    {
        public delegate void VernamLogMessage(string msg, NotificationLevel loglevel);
        public enum CipherMode { Encrypt = 0, Decrypt = 1 };
        public enum UnknownSymbolHandlingMode { Ignore = 0, Remove = 1, Replace = 2 };

        public CipherMode selectedCipherMode = CipherMode.Encrypt;
        public string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ 0123456789";

        private UnknownSymbolHandlingMode unknownSymbolHandling = UnknownSymbolHandlingMode.Ignore;

        public void Initialize() { }

        #region TaskPane Settings

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [PropertySaveOrder(4)]
        [TaskPane("CipherMode", "CipherModeTooltip", null, 1, true, ControlType.ComboBox, new string[] { "Encrypt", "Decrypt" })]
        public CipherMode Action
        {
            get => selectedCipherMode;
            set
            {
                if (value != selectedCipherMode)
                {
                    selectedCipherMode = value;
                    OnPropertyChanged("Action");
                }
            }
        }

        [PropertySaveOrder(5)]
        [TaskPane("UnknownSymbolHandle", "UnknownSymbolHandleTooltip", null, 4, true, ControlType.ComboBox, new string[] { "Ignore", "Remove", "Replace" })]
        public UnknownSymbolHandlingMode UnknownSymbolHandling
        {
            get => unknownSymbolHandling;
            set
            {
                if (value != unknownSymbolHandling)
                {
                    unknownSymbolHandling = value;
                    OnPropertyChanged("UnknownSymbolHandling");
                }
            }
        }

        [PropertySaveOrder(6)]
        [TaskPane("AlphabetInput", "AlphabetInputTooltip", null, 6, true, ControlType.TextBox, "")]
        public string AlphabetSymbols
        {
            get => alphabet;
            set
            {
                string distinctAlphabet = RemoveEqualChars(value);
                if (distinctAlphabet.Length == 0)
                {
                    OnLogMessage("no alphabet. using: \"" + alphabet + "\" (" + alphabet.Length + " Symbols)", NotificationLevel.Info);
                    return;
                }

                if (!distinctAlphabet.Equals(alphabet))
                {
                    alphabet = distinctAlphabet;
                    OnPropertyChanged("AlphabetSymbols");
                    OnLogMessage("new alphabet: \"" + alphabet + "\" (" + alphabet.Length + " Symbols)", NotificationLevel.Info);
                }
            }
        }

        public string RemoveEqualChars(string value)
        {
            return new string(value.ToCharArray().Distinct().ToArray());
        }


        #endregion

        #region Events

        public event VernamLogMessage LogMessage;
        private void OnLogMessage(string msg, NotificationLevel level)
        {
            if (LogMessage != null)
            {
                LogMessage(msg, level);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}
