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
using static HagelinMachine.HagelinConstants;
using static HagelinMachine.HagelinEnums;


namespace CrypTool.Plugins.HagelinMachine
{

    public partial class HagelinMachineSettings : ISettings
    {
        #region Model
        [TaskPane("ModelCaption", "ModelTip", "Hagelin model", 19, false, ControlType.ComboBox, new string[] { "CX52a", "CX52b", "CX52c", "C52d", "CXM", "CXMLateVersion", "CX52FrenchVersion", "CX52EIRE", "M209", "Custom" })]
        public ModelType Model
        {
            get
            {
                return _model;
            }
            set
            {
                if (_model != value)
                {
                    _model = value;                   
                    ApplyModel();
                    HideModelBars();
                    OnPropertyChanged("Model");
                    OnPropertyChanged("SelectedModel");

                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "1")]
        [TaskPane("NumberOfWheelsCaption", "NumberOfWheelsTip", "Hagelin model", 20, false, ControlType.NumericUpDown, ValidationType.RangeInteger, minNumberOfWheels, maxNumberOfWheels)]
        public int NumberOfWheels
        {
            get
            {
                return _numberOfWheels;
            }
            set
            {
                if (_numberOfWheels != value)
                {
                    _numberOfWheels = value;
                    // HOWTO: MUST be called every time a property value changes with correct parameter name
                    //    _model = ModelType.Custom;
                    //    OnPropertyChanged("Model");
                    OnPropertyChanged("NumberOfWheels");
                    ApplyModel();
                    _wheelsShown = true;
                    OnPropertyChanged("WheelsShown");
                    SetVisibility();
                    AdaptBarSizesToNumberOfWheels();

                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "1")]
        [TaskPane("NumberOfBarsCaption", "NumberOfBarsTip", "InfoAndControlGroup", 21, false, ControlType.NumericUpDown, ValidationType.RangeInteger, minNumberOfBars, maxNumberOfBars)]
        public int NumberOfBars
        {
            get
            {
                return _numberOfBars;
            }
            set
            {
                if (_numberOfBars != value)
                {
                    _numberOfBars = value;
                    // HOWTO: MUST be called every time a property value changes with correct parameter name
                    OnPropertyChanged("NumberOfBars");
                    //       _model = ModelType.Custom;
                    OnPropertyChanged("NumberOfBars");
                    ApplyModel();
                    HideModelBars();
                    ShowModelBars();
                    UpdateSelecteBarsString();
                    //       SetVisibility();
                }
            }
        }







        //     [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        // [TaskPane("Bar 1 tooth type", "Sets the type of the tooth of bar 1", "Bar1", 4, false, ControlType.ComboBox, new string[] { "Displace when shifted", "Do not displace", "Displace when not shifted" })]




        //FixedVariableFeautre;
        #endregion
    }
}
