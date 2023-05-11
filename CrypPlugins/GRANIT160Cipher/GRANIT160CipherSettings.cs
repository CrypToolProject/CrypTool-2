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

namespace CrypTool.Plugins.GRANIT160Cipher
{
    public enum Action
    {
        Encrypt,
        Decrypt
    }

    public class GRANIT160CipherSettings : ISettings
    {
        #region Private Variables

        private Action _action = Action.Encrypt;
        private bool _addNullsIfNeeded = false;
        private bool _replaceUmlauts = true;
        private bool _replaceLetterJ = true;
        private bool _encryptSpace = false;

        #endregion

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        #region TaskPane Settings

        [TaskPane("ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "Encrypt", "Decrypt" })]
        public Action Action
        {
            get
            {
                return _action;
            }
            set
            {
                if (_action != value)
                {
                    _action = value;
                    OnPropertyChanged(nameof(Action));
                }
            }
        }

        [TaskPane("AddNullsIfNeededCaption", "AddNullsIfNeededTooltip", null, 2, false, ControlType.CheckBox)]
        public bool AddNullsIfNeeded
        {
            get
            {
                return _addNullsIfNeeded;
            }
            set
            {
                if (_addNullsIfNeeded != value)
                {
                    _addNullsIfNeeded = value;
                    OnPropertyChanged(nameof(AddNullsIfNeeded));
                }
            }
        }

        [TaskPane("ReplaceUmlautsCaption", "ReplaceUmlautsTooltip", null, 3, false, ControlType.CheckBox)]
        public bool ReplaceUmlauts
        {
            get
            {
                return _replaceUmlauts;
            }
            set
            {
                if (_replaceUmlauts != value)
                {
                    _replaceUmlauts = value;
                    OnPropertyChanged(nameof(ReplaceUmlauts));
                }
            }
        }

        [TaskPane("ReplaceLetterJCaption", "ReplaceLetterJTooltip", null, 4, false, ControlType.CheckBox)]
        public bool ReplaceLetterJ
        {
            get
            {
                return _replaceLetterJ;
            }
            set
            {
                if (_replaceLetterJ != value)
                {
                    _replaceLetterJ = value;
                    OnPropertyChanged(nameof(ReplaceLetterJ));
                }
            }
        }

        [TaskPane("EncryptSpaceCaption", "EncryptSpaceTooltip", null, 5, false, ControlType.CheckBox)]
        public bool EncryptSpace
        {
            get
            {
                return _encryptSpace;
            }
            set
            {
                if (_encryptSpace != value)
                {
                    _encryptSpace = value;
                    OnPropertyChanged(nameof(EncryptSpace));
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
