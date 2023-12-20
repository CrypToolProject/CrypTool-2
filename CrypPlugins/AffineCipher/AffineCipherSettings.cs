/*                          
   Copyright 2023 Nils Kopal, CrypTool Project

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

namespace CrypTool.Plugins.AffineCipher
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

    public class AffineCipherSettings : ISettings
    {
        #region Private Variables

        private Action _action = Action.Encrypt;
        private UnknownSymbolHandlingMode _unknownSymbolHandlingMode = UnknownSymbolHandlingMode.Ignore;
        private int _a = 0;
        private int _b = 0;
        private bool _caseSensitive = false;

        #endregion

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

        [TaskPane("CaseSensitiveCaption", "CaseSensitiveTooltip", null, 5, false, ControlType.CheckBox)]
        public bool CaseSensitive
        {
            get => _caseSensitive;
            set
            {
                if (value != _caseSensitive)
                {
                    _caseSensitive = value;
                    OnPropertyChanged(nameof(CaseSensitive));
                }
            }
        }

        [TaskPane("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip", null, 6, false, ControlType.ComboBox, new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2", "UnknownSymbolHandlingList3" })]
        public UnknownSymbolHandlingMode UnknownSymbolHandling
        {
            get => _unknownSymbolHandlingMode;
            set
            {
                if (value != _unknownSymbolHandlingMode)
                {
                    _unknownSymbolHandlingMode = value;
                    OnPropertyChanged(nameof(UnknownSymbolHandlingMode));
                }
            }
        }

        [TaskPane("ACaption", "ATooltip", null, 7, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int A
        {
            get
            {
                return _a;
            }
            set
            {
                if (_a != value)
                {
                    _a = value;
                    OnPropertyChanged(nameof(A));
                }
            }
        }

        [TaskPane("BCaption", "BTooltip", null, 8, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, Int32.MaxValue)]
        public int B
        {
            get
            {
                return _b;
            }
            set
            {
                if (_b != value)
                {
                    _b = value;
                    OnPropertyChanged(nameof(B));
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