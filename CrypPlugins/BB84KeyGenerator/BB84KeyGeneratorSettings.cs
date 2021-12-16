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

namespace CrypTool.Plugins.BB84KeyGenerator
{
    public class BB84KeyGeneratorSettings : ISettings
    {
        #region Private Variables

        private double speedSetting;

        #endregion

        #region TaskPane Settings

        [TaskPane("res_AnimationSpeedCaption", "res_AnimationSpeedTooltip", null, 1, false, ControlType.Slider, 0.5, 2.5)]
        public double SpeedSetting
        {
            get => speedSetting;
            set
            {
                if (speedSetting != (value))
                {
                    speedSetting = (value);
                    OnPropertyChanged("SpeedSetting");
                }
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}
