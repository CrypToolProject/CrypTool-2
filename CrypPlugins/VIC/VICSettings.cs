/*
   Copyright CrypTool 2 Team <ct2contact@CrypTool.org>

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

namespace CrypTool.Plugins.VIC
{
    public enum ActionType
    {
        Encrypt,
        Decrypt
    }
    public enum AlphabetType
    {
        Latin,
        Cyrillic
    }
    public class VICSettings : ISettings
    {
        #region Private Variables

        private ActionType _action = ActionType.Encrypt;
        private AlphabetType _alphabet = AlphabetType.Cyrillic;

        #endregion

        #region TaskPane Settings

        [TaskPane("EncryptionTypeSwitch", "EncryptionTypeSwitchCaption", null, 0, false, ControlType.ComboBox, new string[] { "Encrypt", "Decrypt" })]
        public int Action
        {
            get => (int)_action;
            set
            {
                if (_action != (ActionType)value)
                {
                    _action = (ActionType)value;
                    OnPropertyChanged("Action");
                }
            }
        }

        [TaskPane("AlphabetTypeSwitch", "AlphabetTypeSwitchCaption", null, 0, false, ControlType.ComboBox, new string[] { "Latin", "Cyrillic" })]
        public int Alphabet
        {
            get => (int)_alphabet;
            set
            {
                if (_alphabet != (AlphabetType)value)
                {
                    _alphabet = (AlphabetType)value;
                    OnPropertyChanged("Alphabet");
                }
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion

        public void Initialize()
        {

        }
    }
}
