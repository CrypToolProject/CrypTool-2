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

namespace CrypTool.Plugins.BB84PhotonEncoder
{
    public class BB84PhotonEncoderSettings : ISettings
    {
        #region Private Variables

        private int plusZeroEncoding;
        private int plusOneEncoding;
        private int xZeroEncoding;
        private int xOneEncoding;


        #endregion

        public BB84PhotonEncoderSettings()
        {
            CrypTool.PluginBase.Properties.Settings.Default.PropertyChanged += new PropertyChangedEventHandler(Default_PropertyChanged);
        }

        private void Default_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName.Equals("BB84_AnimationSpeed"))
            {
                OnPropertyChanged("SpeedSetting");
            }
        }


        #region TaskPane Settings

        [TaskPane("res_settings1", "res_settings1Tooltip", null, 1, false, ControlType.ComboBox, new string[] { "|", "-" })]
        public int PlusZeroEncoding
        {
            get => plusZeroEncoding;
            set
            {
                if (plusZeroEncoding != value)
                {
                    if (value == 0)
                    {
                        plusZeroEncoding = 0;
                        PlusOneEncoding = 1;
                        OnPropertyChanged("PlusOneEncoding");
                    }
                    else
                    {
                        plusZeroEncoding = 1;
                        plusOneEncoding = 0;
                        OnPropertyChanged("PlusOneEncoding");
                    }
                }
            }
        }
        [TaskPane("res_settings2", "res_settings2Tooltip", null, 2, false, ControlType.ComboBox, new string[] { "|", "-" })]
        public int PlusOneEncoding
        {
            get => plusOneEncoding;
            set
            {
                if (plusOneEncoding != value)
                {
                    if (value == 0)
                    {
                        plusOneEncoding = 0;
                        plusZeroEncoding = 1;
                        OnPropertyChanged("PlusZeroEncoding");
                    }
                    else
                    {
                        plusOneEncoding = 1;
                        plusZeroEncoding = 0;
                        OnPropertyChanged("PlusZeroEncoding");
                    }
                }
            }
        }
        [TaskPane("res_settings3", "res_settings3Tooltip", null, 3, false, ControlType.ComboBox, new string[] { "\\", "/" })]
        public int XZeroEncoding
        {
            get => xZeroEncoding;
            set
            {
                if (xZeroEncoding != value)
                {
                    if (value == 0)
                    {
                        xZeroEncoding = 0;
                        xOneEncoding = 1;
                        OnPropertyChanged("XOneEncoding");
                    }
                    else
                    {
                        xZeroEncoding = 1;
                        xOneEncoding = 0;
                        OnPropertyChanged("XOneEncoding");
                    }
                }
            }
        }
        [TaskPane("res_settings4", "res_settings4Tooltip", null, 4, false, ControlType.ComboBox, new string[] { "\\", "/" })]
        public int XOneEncoding
        {
            get => xOneEncoding;
            set
            {
                if (xOneEncoding != value)
                {
                    if (value == 0)
                    {
                        xOneEncoding = 0;
                        xZeroEncoding = 1;
                        OnPropertyChanged("XZeroEncoding");
                    }
                    else
                    {
                        xOneEncoding = 1;
                        xZeroEncoding = 0;
                        OnPropertyChanged("XZeroEncoding");
                    }
                }
            }
        }

        [TaskPane("res_animationSpeed", "res_animationSpeedTooltip", null, 5, false, ControlType.Slider, 0.5, 10.0)]
        public double SpeedSetting
        {
            get => CrypTool.PluginBase.Properties.Settings.Default.BB84_AnimationSpeed;
            set
            {
                if (CrypTool.PluginBase.Properties.Settings.Default.BB84_AnimationSpeed != value)
                {
                    CrypTool.PluginBase.Properties.Settings.Default.BB84_AnimationSpeed = value;
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
