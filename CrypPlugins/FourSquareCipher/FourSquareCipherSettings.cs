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

namespace CrypTool.Plugins.FourSquareCipher
{
    public enum Action
    {
        Encrypt,
        Decrypt
    }

    public enum AlphabetVersion
    {
        Twentyfive,
        Thirtysix
    }

    public class FourSquareCipherSettings : ISettings
    {
        #region Private Variables

        private Action _action = Action.Encrypt;
        private AlphabetVersion _alphabetVersion = AlphabetVersion.Twentyfive;

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

        [TaskPane("AlphabetVersionCaption", "AlphabetVersionTooltip", null, 4, false, ControlType.ComboBox, new string[] { "Twentyfive", "Thirtysix" })]
        public AlphabetVersion AlphabetVersion
        {
            get
            {
                return _alphabetVersion;
            }
            set
            {
                if (_alphabetVersion != value)
                {
                    _alphabetVersion = value;
                    OnPropertyChanged(nameof(AlphabetVersion));
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
