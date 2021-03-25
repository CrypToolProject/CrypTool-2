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
using System;
using System.ComponentModel;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.Plugins.BB84ManInTheMiddle
{
    // HOWTO: rename class (click name, press F2)
    public class BB84ManInTheMiddleSettings : ISettings
    {
        #region Private Variables

        private string plusVerticallyDecoding;
        private string xTopRightDiagonallyDecoding;
        private string xTopLeftDiagonallyDecoding;
        private string plusHorizontallyDecoding;
        private bool isListening;
        private int waitingIterations;
        #endregion

        public BB84ManInTheMiddleSettings()
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


        [TaskPane("res_StatusCaption", "res_StatusTooltip", null, 0, false, ControlType.ComboBox, new String[] { "res_StatusListen", "res_StatusSleep" })]
        public int IsListening
        {
            get
            {
                if (this.isListening)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            set
            {
                if (value == 0)
                {
                    this.isListening = true;
                }
                else
                {
                    this.isListening = false;
                }
            }
        }


        [TaskPane("res_Settings1Caption", "res_Settings1Tooltip", null, 1, false, ControlType.ComboBox, new string[] { "0", "1" })]
        public string PlusVerticallyDecoding
        {
            get
            {
                return plusVerticallyDecoding;
            }
            set
            {
                if (plusVerticallyDecoding != value)
                {
                    if (value.Equals("0"))
                    {
                        this.plusVerticallyDecoding = "0";
                        this.plusHorizontallyDecoding = "1";
                        OnPropertyChanged("PlusHorizontallyDecoding");
                    }
                    else
                    {
                        this.plusVerticallyDecoding = "1";
                        this.plusHorizontallyDecoding = "0";
                        OnPropertyChanged("PlusHorizontallyDecoding");

                    }
                }
            }
        }
        [TaskPane("res_Settings2Caption", "res_Settings2Tooltip", null, 2, false, ControlType.ComboBox, new string[] { "0", "1" })]
        public string PlusHorizontallyDecoding
        {
            get
            {
                return plusHorizontallyDecoding;
            }
            set
            {
                if (plusHorizontallyDecoding != value)
                {
                    if (value.Equals("0"))
                    {
                        this.plusHorizontallyDecoding = "0";
                        this.plusVerticallyDecoding = "1";
                        OnPropertyChanged("PlusVerticallyDecoding");
                    }
                    else
                    {
                        this.plusHorizontallyDecoding = "1";
                        this.plusVerticallyDecoding = "0";
                        OnPropertyChanged("PlusVerticallyDecoding");
                    }
                }
            }
        }
        [TaskPane("res_Settings3Caption", "res_Setting3Tooltip", null, 3, false, ControlType.ComboBox, new string[] { "0", "1" })]
        public string XTopRightDiagonallyDecoding
        {
            get
            {
                return xTopRightDiagonallyDecoding;
            }
            set
            {
                if (xTopRightDiagonallyDecoding != value)
                {
                    if (value.Equals("0"))
                    {
                        this.xTopRightDiagonallyDecoding = "0";
                        this.xTopLeftDiagonallyDecoding = "1";
                        OnPropertyChanged("XTopLeftDiagonallyDecoding");
                    }
                    else
                    {
                        this.xTopRightDiagonallyDecoding = "1";
                        this.xTopLeftDiagonallyDecoding = "0";
                        OnPropertyChanged("XTopLeftDiagonallyDecoding");
                    }
                }
            }
        }
        [TaskPane("res_Settings4Caption", "res_Settings4Tooltip", null, 4, false, ControlType.ComboBox, new string[] { "0", "1" })]
        public string XTopLeftDiagonallyDecoding
        {
            get
            {
                return xTopLeftDiagonallyDecoding;
            }
            set
            {
                if (xTopLeftDiagonallyDecoding != value)
                {
                    if (value.Equals("0"))
                    {
                        this.xTopLeftDiagonallyDecoding = "0";
                        this.xTopRightDiagonallyDecoding = "1";
                        OnPropertyChanged("XTopRightDiagonallyDecoding");
                    }
                    else
                    {
                        this.xTopLeftDiagonallyDecoding = "1";
                        this.xTopRightDiagonallyDecoding = "0";
                        OnPropertyChanged("XTopRightDiagonallyDecoding");
                    }
                }
            }
        }

        [TaskPane("res_AnimationSpeedCaption", "res_AnimationSpeedTooltip", null, 5, false, ControlType.Slider, 0.5, 10.0)]
        public double SpeedSetting
        {
            get
            {
                return CrypTool.PluginBase.Properties.Settings.Default.BB84_AnimationSpeed;
            }
            set
            {
                if (CrypTool.PluginBase.Properties.Settings.Default.BB84_AnimationSpeed != value)
                {
                    CrypTool.PluginBase.Properties.Settings.Default.BB84_AnimationSpeed = value;
                    OnPropertyChanged("SpeedSetting");
                }
            }
        }

        [TaskPane("res_WaitingIterationsCaption", "res_WaitingIterationsTooltip", null, 6, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 100)]
        public int WaitingIterations
        {
            get
            {
                return waitingIterations;
            }
            set
            {
                if (waitingIterations != value)
                {
                    waitingIterations = value;
                    OnPropertyChanged("WaitingIterations");
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
