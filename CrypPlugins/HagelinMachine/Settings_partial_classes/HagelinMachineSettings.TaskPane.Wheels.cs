/*
   Copyright 2022 Vasily Mikhalev, CrypTool 2 Team

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
using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text.RegularExpressions;
using static HagelinMachine.HagelinConstants;
using static HagelinMachine.HagelinEnums;

namespace CrypTool.Plugins.HagelinMachine
{
    public partial class HagelinMachineSettings : ISettings
    {
        #region Wheels       

        private ObservableCollection<string> supportedWheelTypeNames = new ObservableCollection<string>(KNOWN_WheelTYPES);
        private int a = 12;

        [DontSave]
        public ObservableCollection<string> SupportedWheelTypeNames
        {
            get { return supportedWheelTypeNames; }
            set
            {
                if (value != supportedWheelTypeNames)
                {
                    supportedWheelTypeNames = value;
                    OnPropertyChanged("SupportedWheelTypes");
                }
            }
        }

        private int DetectWheelTypeNumberByWheelName(string wheelName)
        {
            int result = 0;
            string[] allNames = Enum.GetNames(typeof(WheelType));
            for (int i = 0; i < allNames.Length; i++)
            {
                if (allNames[i].Contains(wheelName))
                {
                    result = i;
                    break;
                };
            }
            return result;
        }

        private string GetWheelNameFromWheelType(WheelType wheelType)
        {
            return wheelType.ToString().Substring(1, 2);
        }

        #region Wheel1
        [SettingsFormat(1, "Bold", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "1")]
        [TaskPane("WheelTypeCaption", "WheelTypeTip", "WheelNum1", 1, false, ControlType.DynamicComboBox, new string[] { "SupportedWheelTypeNames" })]

        public int Wheel1TypeName
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return SupportedWheelTypeNames.IndexOf(_wheelTypeNames[thisWheelIndex]);
            }
            set
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                if (((int)value != -1) && _model != ModelType.M209) //In M209 the wheels are not interchangable
                {
                    _wheelTypeNames[thisWheelIndex] = SupportedWheelTypeNames[value];
                    int selectedWheelType = DetectWheelTypeNumberByWheelName(_wheelTypeNames[thisWheelIndex]);
                    SetWheelType(thisWheelIndex, selectedWheelType);
                }
            }

        }

        [TaskPane("WheelLabelsCaption", "WheelLabelsTip", "WheelNum1", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "[A-Z0-9, ]*")]
        public string Wheel1InitialState
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1; //Just to know which wheel we are currently setting. In order to have the same Set method everywhere
                return _wheelsInitialStates[thisWheelIndex];
            }
            set
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1; //Just to know which wheel we are currently setting. In order to have the same Set method everywhere
                setWheelState(thisWheelIndex, value);
            }
        }


        [TaskPane("WheelActivePinsCaption", "WheelActivePinsTip", "WheelNum1", 3, false, ControlType.TextBox, ValidationType.RegEx, "^[A-Z0-9, ]*$")]
        public string Wheel1Pins
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _wheelsPins[thisWheelIndex];
            }
            set
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                setWheelPins(thisWheelIndex, value);
            }
        }



        #endregion

        #region Wheel2
        [SettingsFormat(10, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("WheelTypeCaption", "WheelTypeTip", "WheelNum2", 4, false, ControlType.DynamicComboBox, new string[] { "SupportedWheelTypeNames" })]
        public int Wheel2TypeName
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return SupportedWheelTypeNames.IndexOf(_wheelTypeNames[thisWheelIndex]);
            }
            set
            {


                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                if (((int)value != -1) && _model != ModelType.M209) //In M209 the wheels are not interchangable
                {
                    _wheelTypeNames[thisWheelIndex] = SupportedWheelTypeNames[value];
                    int selectedWheelType = DetectWheelTypeNumberByWheelName(_wheelTypeNames[thisWheelIndex]);
                    SetWheelType(thisWheelIndex, selectedWheelType);
                }
            }

        }

        [TaskPane("WheelLabelsCaption", "WheelLabelsTip", "WheelNum2", 5, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "[A-Z0-9, ]*")]
        public string Wheel2InitialState
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _wheelsInitialStates[thisWheelIndex];
            }
            set
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                setWheelState(thisWheelIndex, value);
            }
        }

        [TaskPane("WheelActivePinsCaption", "WheelActivePinsTip", "WheelNum2", 6, false, ControlType.TextBox, ValidationType.RegEx, "[A-Z0-9, ]*")]
        public string Wheel2Pins
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _wheelsPins[thisWheelIndex];
            }
            set
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                setWheelPins(thisWheelIndex, value);
            }
        }

        #endregion

        #region Wheel3
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("WheelTypeCaption", "WheelTypeTip", "WheelNum3", 7, false, ControlType.DynamicComboBox, new string[] { "SupportedWheelTypeNames" })]
        public int Wheel3TypeName
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return SupportedWheelTypeNames.IndexOf(_wheelTypeNames[thisWheelIndex]);
            }
            set
            {


                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                if (((int)value != -1) && _model != ModelType.M209) //In M209 the wheels are not interchangable
                {
                    _wheelTypeNames[thisWheelIndex] = SupportedWheelTypeNames[value];
                    int selectedWheelType = DetectWheelTypeNumberByWheelName(_wheelTypeNames[thisWheelIndex]);
                    SetWheelType(thisWheelIndex, selectedWheelType);
                }
            }

        }

        [TaskPane("WheelLabelsCaption", "WheelLabelsTip", "WheelNum3", 8, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "[A-Z0-9, ]*")]
        public string Wheel3InitialState
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _wheelsInitialStates[thisWheelIndex];
            }
            set
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                setWheelState(thisWheelIndex, value);
            }
        }

        [TaskPane("WheelActivePinsCaption", "WheelActivePinsTip", "WheelNum3", 9, false, ControlType.TextBox, ValidationType.RegEx, "[A-Z0-9, ]*")]
        public string Wheel3Pins
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _wheelsPins[thisWheelIndex];
            }
            set
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                setWheelPins(thisWheelIndex, value);
            }
        }

        #endregion

        #region Wheel4
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "4")]
        [TaskPane("WheelTypeCaption", "WheelTypeTip", "WheelNum4", 10, false, ControlType.DynamicComboBox, new string[] { "SupportedWheelTypeNames" })]
        public int Wheel4TypeName
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return SupportedWheelTypeNames.IndexOf(_wheelTypeNames[thisWheelIndex]);
            }
            set
            {


                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                if (((int)value != -1) && _model != ModelType.M209) //In M209 the wheels are not interchangable
                {
                    _wheelTypeNames[thisWheelIndex] = SupportedWheelTypeNames[value];
                    int selectedWheelType = DetectWheelTypeNumberByWheelName(_wheelTypeNames[thisWheelIndex]);
                    SetWheelType(thisWheelIndex, selectedWheelType);
                }
            }
        }

        [TaskPane("WheelLabelsCaption", "WheelLabelsTip", "WheelNum4", 11, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "[A-Z0-9, ]*")]
        public string Wheel4InitialState
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _wheelsInitialStates[thisWheelIndex];
            }
            set
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                setWheelState(thisWheelIndex, value);
            }
        }

        [TaskPane("WheelActivePinsCaption", "WheelActivePinsTip", "WheelNum4", 12, false, ControlType.TextBox, ValidationType.RegEx, "[A-Z0-9, ]*")]
        public string Wheel4Pins
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _wheelsPins[thisWheelIndex];
            }
            set
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                setWheelPins(thisWheelIndex, value);
            }
        }
        #endregion

        #region Wheel5
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "5")]
        [TaskPane("WheelTypeCaption", "WheelTypeTip", "WheelNum5", 13, false, ControlType.DynamicComboBox, new string[] { "SupportedWheelTypeNames" })]
        public int Wheel5TypeName
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return SupportedWheelTypeNames.IndexOf(_wheelTypeNames[thisWheelIndex]);
            }
            set
            {


                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                if (((int)value != -1) && _model != ModelType.M209) //In M209 the wheels are not interchangable
                {
                    _wheelTypeNames[thisWheelIndex] = SupportedWheelTypeNames[value];
                    int selectedWheelType = DetectWheelTypeNumberByWheelName(_wheelTypeNames[thisWheelIndex]);
                    SetWheelType(thisWheelIndex, selectedWheelType);
                }
            }

        }

        [TaskPane("WheelLabelsCaption", "WheelLabelsTip", "WheelNum5", 14, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "[A-Z0-9, ]*")]
        public string Wheel5InitialState
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _wheelsInitialStates[thisWheelIndex];
            }
            set
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                setWheelState(thisWheelIndex, value);
            }
        }

        [TaskPane("WheelActivePinsCaption", "WheelActivePinsTip", "WheelNum5", 15, false, ControlType.TextBox, ValidationType.RegEx, "[A-Z0-9, ]*")]
        public string Wheel5Pins
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _wheelsPins[thisWheelIndex];
            }
            set
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                setWheelPins(thisWheelIndex, value);
            }
        }

        #endregion

        #region Wheel6
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "6")]
        [TaskPane("WheelTypeCaption", "WheelTypeTip", "WheelNum6", 16, false, ControlType.DynamicComboBox, new string[] { "SupportedWheelTypeNames" })]
        public int Wheel6TypeName
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return SupportedWheelTypeNames.IndexOf(_wheelTypeNames[thisWheelIndex]);
            }
            set
            {


                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                if (((int)value != -1) && _model != ModelType.M209) //In M209 the wheels are not interchangable
                {
                    _wheelTypeNames[thisWheelIndex] = SupportedWheelTypeNames[value];
                    int selectedWheelType = DetectWheelTypeNumberByWheelName(_wheelTypeNames[thisWheelIndex]);
                    SetWheelType(thisWheelIndex, selectedWheelType);
                }
            }

        }

        [TaskPane("WheelLabelsCaption", "WheelLabelsTip", "WheelNum6", 17, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "[A-Z0-9, ]*")]
        public string Wheel6InitialState
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _wheelsInitialStates[thisWheelIndex];
            }
            set
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                setWheelState(thisWheelIndex, value);
            }
        }

        [TaskPane("WheelActivePinsCaption", "WheelActivePinsTip", "WheelNum6", 18, false, ControlType.TextBox, ValidationType.RegEx, "[A-Z0-9, ]*")]
        public string Wheel6Pins
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _wheelsPins[thisWheelIndex];
            }
            set
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                setWheelPins(thisWheelIndex, value);
            }
        }

        #endregion

        /*

        #region Wheel7
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "7")]
        [TaskPane("WheelTypeCaption", "WheelTypeTip", "Wheel7", 19, false, ControlType.DynamicComboBox, new string[] { "SupportedWheelTypeNames" })]
        public int Wheel7TypeName
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return SupportedWheelTypeNames.IndexOf(_wheelTypeNames[thisWheelIndex]);
            }
            set
            {


                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                if (((int)value != -1) && _model != ModelType.M209) //In M209 the wheels are not interchangable
                {
                    _wheelTypeNames[thisWheelIndex] = SupportedWheelTypeNames[value];
                    int selectedWheelType = DetectWheelTypeNumberByWheelName(_wheelTypeNames[thisWheelIndex]);
                    SetWheelType(thisWheelIndex, selectedWheelType);
                }
            }

        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "7")]
        [TaskPane("WheelLabelsCaption", "WheelLabelsTip", "Wheel7", 20, false, ControlType.TextBox, ValidationType.RegEx, "[A-Z0-9, ]*")]
        public string Wheel7InitialState
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _wheelsInitialStates[thisWheelIndex];
            }
            set
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                setWheelState(thisWheelIndex, value);
            }
        }


        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "7")]
        [TaskPane("WheelActivePinsCaption", "WheelActivePinsTip", "Wheel7", 21, false, ControlType.TextBox, ValidationType.RegEx, "^[A-Z0-9 ,]*$")]
        public string Wheel7Pins
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _wheelsPins[thisWheelIndex];
            }
            set
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                setWheelPins(thisWheelIndex, value);
            }
        }

        #endregion

        #region Wheel8
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "8")]
        [TaskPane("WheelTypeCaption", "WheelTypeTip", "Wheel8", 22, false, ControlType.DynamicComboBox, new string[] { "SupportedWheelTypeNames" })]
        public int Wheel8TypeName
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return SupportedWheelTypeNames.IndexOf(_wheelTypeNames[thisWheelIndex]);
            }
            set
            {


                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                if (((int)value != -1) && _model != ModelType.M209) //In M209 the wheels are not interchangable
                {
                    _wheelTypeNames[thisWheelIndex] = SupportedWheelTypeNames[value];
                    int selectedWheelType = DetectWheelTypeNumberByWheelName(_wheelTypeNames[thisWheelIndex]);
                    SetWheelType(thisWheelIndex, selectedWheelType);
                }
            }

        }
        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "8")]
        [TaskPane("WheelLabelsCaption", "WheelLabelsTip", "Wheel8", 23, false, ControlType.TextBox, ValidationType.RegEx, "[A-Z0-9, ]*")]
        public string Wheel8InitialState
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _wheelsInitialStates[thisWheelIndex];
            }
            set
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                setWheelState(thisWheelIndex, value);
            }
        }


        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "8")]
        [TaskPane("WheelActivePinsCaption", "WheelActivePinsTip", "Wheel8", 24, false, ControlType.TextBox, ValidationType.RegEx, "[A-Z0-9, ]*")]
        public string Wheel8Pins
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _wheelsPins[thisWheelIndex];
            }
            set
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                setWheelPins(thisWheelIndex, value);
            }
        }



        #endregion

        #region Wheel9
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "9")]
        [TaskPane("WheelTypeCaption", "WheelTypeTip", "Wheel9", 25, false, ControlType.DynamicComboBox, new string[] { "SupportedWheelTypeNames" })]
        public int Wheel9TypeName
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return SupportedWheelTypeNames.IndexOf(_wheelTypeNames[thisWheelIndex]);
            }
            set
            {


                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                if (((int)value != -1) && _model != ModelType.M209) //In M209 the wheels are not interchangable
                {
                    _wheelTypeNames[thisWheelIndex] = SupportedWheelTypeNames[value];
                    int selectedWheelType = DetectWheelTypeNumberByWheelName(_wheelTypeNames[thisWheelIndex]);
                    SetWheelType(thisWheelIndex, selectedWheelType);
                }
            }

        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "9")]
        [TaskPane("WheelLabelsCaption", "WheelLabelsTip", "Wheel9", 26, false, ControlType.TextBox, ValidationType.RegEx, "[A-Z0-9, ]*")]
        public string Wheel9InitialState
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _wheelsInitialStates[thisWheelIndex];
            }
            set
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                setWheelState(thisWheelIndex, value);
            }
        }


        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "9")]
        [TaskPane("WheelActivePinsCaption", "WheelActivePinsTip", "Wheel9", 27, false, ControlType.TextBox, ValidationType.RegEx, "[A-Z0-9, ]*")]
        public string Wheel9Pins
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _wheelsPins[thisWheelIndex];
            }
            set
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                setWheelPins(thisWheelIndex, value);
            }
        }



        #endregion

        #region Wheel10
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "10")]
        [TaskPane("WheelTypeCaption", "WheelTypeTip", "Wheel10", 28, false, ControlType.DynamicComboBox, new string[] { "SupportedWheelTypeNames" })]
        public int Wheel10TypeName
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return SupportedWheelTypeNames.IndexOf(_wheelTypeNames[thisWheelIndex]);
            }
            set
            {


                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                if (((int)value != -1) && _model != ModelType.M209) //In M209 the wheels are not interchangable
                {
                    _wheelTypeNames[thisWheelIndex] = SupportedWheelTypeNames[value];
                    int selectedWheelType = DetectWheelTypeNumberByWheelName(_wheelTypeNames[thisWheelIndex]);
                    SetWheelType(thisWheelIndex, selectedWheelType);
                }
            }

        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "10")]
        [TaskPane("WheelLabelsCaption", "WheelLabelsTip", "Wheel10", 29, false, ControlType.TextBox, ValidationType.RegEx, "[A-Z0-9, ]*")]
        public string Wheel10InitialState
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _wheelsInitialStates[thisWheelIndex];
            }
            set
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                setWheelState(thisWheelIndex, value);
            }
        }


        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "10")]
        [TaskPane("WheelActivePinsCaption", "WheelActivePinsTip", "Wheel10", 30, false, ControlType.TextBox, ValidationType.RegEx, "[A-Z0-9, ]*")]
        public string Wheel10Pins
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _wheelsPins[thisWheelIndex];
            }
            set
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                setWheelPins(thisWheelIndex, value);
            }
        }



        #endregion

        #region Wheel11
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "11")]
        [TaskPane("WheelTypeCaption", "WheelTypeTip", "Wheel11", 31, false, ControlType.DynamicComboBox, new string[] { "SupportedWheelTypeNames" })]
        public int Wheel11TypeName
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return SupportedWheelTypeNames.IndexOf(_wheelTypeNames[thisWheelIndex]);
            }
            set
            {


                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                if (((int)value != -1) && _model != ModelType.M209) //In M209 the wheels are not interchangable
                {
                    _wheelTypeNames[thisWheelIndex] = SupportedWheelTypeNames[value];
                    int selectedWheelType = DetectWheelTypeNumberByWheelName(_wheelTypeNames[thisWheelIndex]);
                    SetWheelType(thisWheelIndex, selectedWheelType);
                }
            }

        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "11")]
        [TaskPane("WheelLabelsCaption", "WheelLabelsTip", "Wheel11", 32, false, ControlType.TextBox, ValidationType.RegEx, "[A-Z0-9, ]*")]
        public string Wheel11InitialState
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _wheelsInitialStates[thisWheelIndex];
            }
            set
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                setWheelState(thisWheelIndex, value);
            }
        }


        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "11")]
        [TaskPane("WheelActivePinsCaption", "WheelActivePinsTip", "Wheel11", 33, false, ControlType.TextBox, ValidationType.RegEx, "[A-Z0-9, ]*")]
        public string Wheel11Pins
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _wheelsPins[thisWheelIndex];
            }
            set
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                setWheelPins(thisWheelIndex, value);
            }
        }



        #endregion

        #region Wheel12
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "12")]
        [TaskPane("WheelTypeCaption", "WheelTypeTip", "Wheel12", 34, false, ControlType.DynamicComboBox, new string[] { "SupportedWheelTypeNames" })]
        public int Wheel12TypeName
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return SupportedWheelTypeNames.IndexOf(_wheelTypeNames[thisWheelIndex]);
            }
            set
            {


                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                if (((int)value != -1) && _model != ModelType.M209) //In M209 the wheels are not interchangable
                {
                    _wheelTypeNames[thisWheelIndex] = SupportedWheelTypeNames[value];
                    int selectedWheelType = DetectWheelTypeNumberByWheelName(_wheelTypeNames[thisWheelIndex]);
                    SetWheelType(thisWheelIndex, selectedWheelType);
                }
            }

        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "12")]
        [TaskPane("WheelLabelsCaption", "WheelLabelsTip", "Wheel12", 35, false, ControlType.TextBox, ValidationType.RegEx, "[A-Z0-9, ]*")]
        public string Wheel12InitialState
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _wheelsInitialStates[thisWheelIndex];
            }
            set
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                setWheelState(thisWheelIndex, value);
            }
        }


        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "12")]
        [TaskPane("WheelActivePinsCaption", "WheelActivePinsTip", "Wheel12", 36, false, ControlType.TextBox, ValidationType.RegEx, "[A-Z0-9, ]*")]
        public string Wheel12Pins
        {
            get
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _wheelsPins[thisWheelIndex];
            }
            set
            {
                int thisWheelIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                setWheelPins(thisWheelIndex, value);
            }
        }        
        #endregion
        */
        #endregion
    }
}
