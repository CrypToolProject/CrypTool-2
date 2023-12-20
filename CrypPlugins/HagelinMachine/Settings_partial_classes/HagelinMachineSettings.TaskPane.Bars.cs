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
using System.Reflection;
using System.Text.RegularExpressions;
using static HagelinMachine.HagelinEnums;

namespace CrypTool.Plugins.HagelinMachine
{
    public partial class HagelinMachineSettings : ISettings
    {
        #region Drum

        #region Bar1
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum1", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar1Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum1", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar1HasLugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum1", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar1CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum1", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar1Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum1", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar1ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }

        #endregion

        #region Bar2
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum2", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar2Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum2", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar2HasLugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum2", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar2CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum2", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar2Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum2", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar2ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar3
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum3", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar3Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum3", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar3HasLugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "Selected values represent how cams influence each of the wheels. That is: 0 - never, A - when displaced, B - when not displaced,  C - always", "BarNum3", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar3CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum3", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar3Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum3", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar3ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar4
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum4", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar4Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum4", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar4HasLugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum4", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar4CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum4", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar4Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum4", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar4ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar5
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum5", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar5Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum5", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar5HasLugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum5", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar5CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum5", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar5Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum5", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar5ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar6
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum6", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar6Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum6", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar6HasLugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum6", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar6CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }

        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum6", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar6Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum6", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar6ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar7
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum7", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar7Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum7", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar7HasLugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum7", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar7CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum7", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar7Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum7", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar7ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar8
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum8", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar8Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum8", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar8HasLugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum8", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar8CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum8", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar8Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum8", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar8ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar9
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum9", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar9Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum9", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar9HasLugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum9", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar9CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum9", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar9Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum9", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar9ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar10
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum10", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar10Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum10", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar10HasLugs


        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum10", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar10CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum10", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar10Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum10", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar10ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar11
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum11", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar11Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum11", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar11HasLugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum11", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar11CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum11", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar11Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum11", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar11ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar12
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum12", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar12Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum12", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar12HasLugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum12", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar12CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum12", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar12Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum12", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar12ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar13
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum13", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar13Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum13", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar13HasLugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum13", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar13CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum13", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar13Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum13", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar13ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar14
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum14", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar14Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum14", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar14HasLugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum14", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar14CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum14", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar14Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum14", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar14ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar15
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum15", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar15Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum15", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar15HasLugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum15", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar15CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum15", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar15Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum15", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar15ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar16
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum16", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar16Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum16", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar16HasLugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum16", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar16CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum16", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar16Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum16", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar16ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar17
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum17", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar17Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum17", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar17HasLugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum17", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar17CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum17", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar17Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum17", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar17ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar18
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum18", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar18Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum18", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar18HasLugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum18", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar18CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum18", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar18Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum18", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar18ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar19
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum19", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar19Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum19", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar19HasLugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum19", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar19CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum19", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar19Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum19", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar19ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar20
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum20", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar20Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum20", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar20HasLugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum20", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar20CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum20", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar20Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum20", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar20ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar21
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum21", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar21Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum21", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar21HasLugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum21", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar21CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum21", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar21Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum21", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar21ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar22
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum22", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar22Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum22", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar22HasLugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum22", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar22CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum22", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar22Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum22", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar22ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar23
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum23", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar23Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum23", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar23HasLugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum23", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar23CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum23", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar23Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum23", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar23ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar24
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum24", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar24Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum24", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar24HasLugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum24", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar24CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum24", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar24Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum24", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar24ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar25
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum25", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar25Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum25", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar25HasLugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum25", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar25CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum25", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar25Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum25", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar25ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar26
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum26", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar26Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum26", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar26HasLugs

        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum26", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar26CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum26", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar26Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum26", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar26ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar27
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum27", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar27Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum27", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar27HasLugs

        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum27", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar27CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum27", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar27Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum27", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar27ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar28
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum28", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar28Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum28", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar28HasLugs

        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum28", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar28CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum28", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar28Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum28", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar28ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar29
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum29", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar29Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum29", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar29HasLugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum29", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar29CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum29", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar29Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum29", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar29ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar30
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum30", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar30Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum30", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar30HasLugs

        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum30", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar30CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum30", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar30Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum30", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar30ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar31
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum31", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar31Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum31", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar31HasLugs

        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum31", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar31CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum31", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar31Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum31", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar31ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #region Bar32
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarTypeCaption", "BarTypeTip", "BarNum32", 0, false, ControlType.ComboBox, new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "35", "54", "56", "57", "60", "64", "68", "105", "106", "107", "108", "109", "111", "157" })]

        public int Bar32Type
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barType[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarType(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "0")]
        [TaskPane("BarHasLugsCaption", "BarHasLugsTip", "BarNum32", 1, false, ControlType.ComboBox, new string[] { "False", "True" })]

        public bool Bar32HasLugs

        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barHasLugs[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarHasLugs(thisBarIndex, value);
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarCamTypesCaption", "BarCamTypesTip", "BarNum32", 2, false, ControlType.TextBoxReadOnly, ValidationType.RegEx, "^[0,A,B,C]{0,12}")]
        public string Bar32CamTypes
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;

                return _barCamsAsString[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetCamTypes(thisBarIndex, value);
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("BarLugsCaption", "BarLugsTip", "BarNum32", 3, false, ControlType.TextBox, ValidationType.RegEx, "[1-6 ;,]*$")]
        public string Bar32Lugs
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barLug[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarLugs(thisBarIndex, value);
            }
        }

        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("BarToothTypeCaption", "BarToothTypeTip", "BarNum32", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]

        public ToothType Bar32ToothType
        {
            get
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                return _barTooth[thisBarIndex];
            }
            set
            {
                int thisBarIndex = Int32.Parse(Regex.Match(MethodBase.GetCurrentMethod().Name, @"\d+").Value) - 1;
                SetBarToothType(thisBarIndex, value);
            }
        }
        #endregion

        #endregion
    }
}
