/*
   Copyright 2019 Christian Bender christian1.bender@student.uni-siegen.de

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

namespace CrypTool.Plugins.DCAOracle
{
    public class DCAOracleSettings : ISettings
    {
        #region Private Variables

        private int _wordSize = 16;

        #endregion

        #region TaskPane Settings

        [TaskPane("WorSizeParameter", "WorSizeParameterToolTip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 16)]
        public int WordSize
        {
            get => _wordSize;
            set
            {
                if (_wordSize != value)
                {
                    _wordSize = value;
                    OnPropertyChanged("WordSize");
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
