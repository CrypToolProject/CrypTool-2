/*
   Copyright 2011 CrypTool 2 Team <ct2contact@CrypTool.org>

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

namespace CrypTool.Plugins.CoinFlipping
{
    public class CoinFlippingSettings : ISettings
    {
        #region Private Variables

        private int settingsHonest = 0;

        #endregion

        #region Private init

        public void Initialize()
        {
        }

        #endregion

        #region TaskPane Settings

        [TaskPane("SettingsHonestCaption", "SettingsHonestTooltip", null, 1, false, ControlType.ComboBox, new string[] { "SettingsHonestList1", "SettingsHonestList2" })]
        public int SettingsHonest
        {
            get => settingsHonest;
            set
            {
                if (settingsHonest != value)
                {
                    settingsHonest = value;

                    OnPropertyChanged("SettingsHonest");
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
    }
}
