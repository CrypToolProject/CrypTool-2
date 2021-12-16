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

namespace CrypTool.Plugins.BB84PhotonDecoder
{
    public class BB84PhotonDecoderSettings : ISettings
    {
        #region Private Variables

        private string plusVerticallyDecoding;
        private string xTopRightDiagonallyDecoding;
        private string xTopLeftDiagonallyDecoding;
        private string plusHorizontallyDecoding;
        private bool errorsEnabled;
        private int errorRatio;
        private int waitingIterations;
        #endregion

        public BB84PhotonDecoderSettings()
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

        [TaskPane("res_Setting1Caption", "res_Setting1Tooltip", null, 1, false, ControlType.ComboBox, new string[] { "0", "1" })]
        public string PlusVerticallyDecoding
        {
            get => plusVerticallyDecoding;
            set
            {
                if (plusVerticallyDecoding != value)
                {
                    if (value.Equals("0"))
                    {
                        plusVerticallyDecoding = "0";
                        plusHorizontallyDecoding = "1";
                        OnPropertyChanged("PlusHorizontallyDecoding");
                    }
                    else
                    {
                        plusVerticallyDecoding = "1";
                        plusHorizontallyDecoding = "0";
                        OnPropertyChanged("PlusHorizontallyDecoding");

                    }
                }
            }
        }
        [TaskPane("res_Setting2Caption", "res_Setting2Tooltip", null, 2, false, ControlType.ComboBox, new string[] { "0", "1" })]
        public string PlusHorizontallyDecoding
        {
            get => plusHorizontallyDecoding;
            set
            {
                if (plusHorizontallyDecoding != value)
                {
                    if (value.Equals("0"))
                    {
                        plusHorizontallyDecoding = "0";
                        plusVerticallyDecoding = "1";
                        OnPropertyChanged("PlusVerticallyDecoding");
                    }
                    else
                    {
                        plusHorizontallyDecoding = "1";
                        plusVerticallyDecoding = "0";
                        OnPropertyChanged("PlusVerticallyDecoding");
                    }
                }
            }
        }
        [TaskPane("res_Setting3Caption", "res_Setting3Tooltip", null, 3, false, ControlType.ComboBox, new string[] { "0", "1" })]
        public string XTopRightDiagonallyDecoding
        {
            get => xTopRightDiagonallyDecoding;
            set
            {
                if (xTopRightDiagonallyDecoding != value)
                {
                    if (value.Equals("0"))
                    {
                        xTopRightDiagonallyDecoding = "0";
                        xTopLeftDiagonallyDecoding = "1";
                        OnPropertyChanged("XTopLeftDiagonallyDecoding");
                    }
                    else
                    {
                        xTopRightDiagonallyDecoding = "1";
                        xTopLeftDiagonallyDecoding = "0";
                        OnPropertyChanged("XTopLeftDiagonallyDecoding");
                    }
                }
            }
        }
        [TaskPane("res_Setting4Caption", "res_Setting4Tooltip", null, 4, false, ControlType.ComboBox, new string[] { "0", "1" })]
        public string XTopLeftDiagonallyDecoding
        {
            get => xTopLeftDiagonallyDecoding;
            set
            {
                if (xTopLeftDiagonallyDecoding != value)
                {
                    if (value.Equals("0"))
                    {
                        xTopLeftDiagonallyDecoding = "0";
                        xTopRightDiagonallyDecoding = "1";
                        OnPropertyChanged("XTopRightDiagonallyDecoding");
                    }
                    else
                    {
                        xTopLeftDiagonallyDecoding = "1";
                        xTopRightDiagonallyDecoding = "0";
                        OnPropertyChanged("XTopRightDiagonallyDecoding");
                    }
                }
            }
        }

        [TaskPane("res_AnimationSpeedCaption", "res_AnimationSpeedTooltip", null, 5, false, ControlType.Slider, 0.5, 10.0)]
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

        [TaskPane("res_ErrosCaption", "res_ErrorsTooltip", null, 5, false, ControlType.ComboBox, new string[] { "res_Disabled", "res_Enabled" })]
        public int ErrorsEnabled
        {
            get
            {
                if (errorsEnabled)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                if (value == 0)
                {
                    errorsEnabled = false;
                    OnPropertyChanged("ErrorsEnabled");
                }
                else
                {
                    errorsEnabled = true;
                    OnPropertyChanged("ErrorsEnabled");
                }
            }
        }

        [TaskPane("res_ErrorRatioCaption", "res_ErrorRatioTooltip", null, 6, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 100)]
        public int ErrorRatio
        {
            get => errorRatio;
            set
            {
                if (errorRatio != value)
                {
                    errorRatio = value;
                    OnPropertyChanged("ErrorRatio");
                }
            }
        }

        [TaskPane("res_WaitingIterationsCaption", "res_WaitingIterationsTooltip", null, 7, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 100)]
        public int WaitingIterations
        {
            get => waitingIterations;
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
