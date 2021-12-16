/*
   Copyright CrypTool 2 Team <ct2contact@cryptool.org>

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

using CrypTool.Chaocipher.Utils;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;

namespace CrypTool.Chaocipher
{
    public class ChaocipherSettings : ISettings
    {
        #region Private Variables

        private ChaoCipherCodeMode _cipher;
        private string _key;
        private int _speed = Constants.InitialWaitTime;
        internal event SpeedValueChanged OnSpeedChanged;
        public delegate void SpeedValueChanged(int value);



        public enum ChaoCipherCodeMode
        {
            Encrypt = 0,
            Decrypt = 1
        }

        #endregion

        #region TaskPane Settings

        [PropertySaveOrder(30)]
        [TaskPane("Mode", "ModeTooltip", null, 10, false,
            ControlType.ComboBox, new string[] { "ModeAction1", "ModeAction2" })]
        public ChaoCipherCodeMode Cipher
        {
            get => _cipher;
            set
            {
                if (_cipher == value)
                {
                    return;
                }

                _cipher = value;
                OnPropertyChanged(nameof(Cipher));
            }
        }

        [PropertySaveOrder(20)]
        [TaskPane("Speed", "SpeedTooltip", null, 20, true,
            ControlType.Slider, Constants.MinWaitTime, Constants.MaxWaitTime)]
        public int Speed
        {
            get => _speed;
            set
            {
                _speed = value;
                OnSpeedChanged?.Invoke(value);
            }
        }

        [PropertySaveOrder(10)]
        [TaskPane("KeyCaption", "KeyTooltip", null, 30, false, ControlType.TextBox)]
        public string Key
        {
            get => _key ?? string.Empty;
            set
            {
                if (_key == value)
                {
                    return;
                }

                _key = value;
                OnPropertyChanged(nameof(Key));
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
