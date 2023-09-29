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

namespace CrypTool.Plugins.ElsieFourCipher
{
    public enum Action
    {
        Encrypt,
        Decrypt
    }

    public enum UnknownSymbolHandlingMode
    {
        Ignore = 0,
        Remove = 1,
        Replace = 2
    }

    public class ElsieFourCipherSettings : ISettings
    {
        #region Private Variables

        private Action _action = Action.Encrypt;
        private UnknownSymbolHandlingMode unknownSymbolHandling = UnknownSymbolHandlingMode.Ignore;
        private bool _underscoreIsSpace = false;

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

        [TaskPane("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip", null, 2, false, ControlType.ComboBox, new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2", "UnknownSymbolHandlingList3" })]
        public UnknownSymbolHandlingMode UnknownSymbolHandling
        {
            get => unknownSymbolHandling;
            set
            {
                if (value != unknownSymbolHandling)
                {
                    unknownSymbolHandling = value;
                    OnPropertyChanged(nameof(UnknownSymbolHandling));
                }
            }
        }

        [TaskPane("UnderscoreisSpaceCaption", "UnderscoreisSpaceTooltip", null, 3, false, ControlType.CheckBox)]
        public bool UnderscoreisSpace
        {
            get => _underscoreIsSpace;
            set
            {
                if (value != _underscoreIsSpace)
                {
                    _underscoreIsSpace = value;
                    OnPropertyChanged(nameof(UnderscoreisSpace));
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
