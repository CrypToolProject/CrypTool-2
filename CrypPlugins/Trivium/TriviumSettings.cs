/*
   Copyright 2022 Nils Kopal, CrypTool project

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

namespace CrypTool.Trivium
{
    public class TriviumSettings : ISettings
    {
        #region ISettings Members        

        private int _initRounds = 1152; //default value for Trivium is 1152 init rounds

        [TaskPane("InitRoundsCaption", "InitRoundsTooltip", null, 0, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int InitRounds
        {
            get => _initRounds;
            set
            {
                _initRounds = value;
                OnPropertyChanged("InitRounds");
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        private void OnPropertyChanged(string prop)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, prop);
        }

        #endregion
    }
}
