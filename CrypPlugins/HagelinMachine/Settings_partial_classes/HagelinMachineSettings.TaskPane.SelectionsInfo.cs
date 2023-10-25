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
using System.Text;
using static HagelinMachine.HagelinEnums;

namespace CrypTool.Plugins.HagelinMachine
{
    public partial class HagelinMachineSettings : ISettings
    {
        #region SelectedSettings
        [TaskPane("SelectedModelCaption", "SelectedModelTip", "InfoAndControlGroup", 1, true, ControlType.TextBoxReadOnly)]
        public string SelectedModel
        {
            get
            {
                return _selectedModel;
            }
            set
            {
                switch (_model)
                {
                    case ModelType.CX52a:
                        _selectedModel ="CX-52a";
                         break;
                    case ModelType.CX52b:
                        _selectedModel = "CX-52b";
                        break;
                    case ModelType.CX52c:
                        _selectedModel = "CX-52c";
                        break;
                    case ModelType.CXM:
                        _selectedModel = "CX-M";
                        break;
                    case ModelType.CXM_LATE_VERSION:
                        _selectedModel = "CX-M late version";
                        break;
                    case ModelType.C52d:
                        _selectedModel = "C-52d";
                        break;
                }


                OnPropertyChanged("SelectedModel");
            }
        }

        //        [SettingsFormat(0, "Bold", "Normal", "Red", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "10")]
        [TaskPane("SelectedWheelsCaption", "SelectedWheelsTip", "InfoAndControlGroup", 2, true, ControlType.TextBoxReadOnly)]
        public string SelectedWheels
        {
            get
            {
                return _selectedWheels;
            }
            set
            {
                StringBuilder a = new StringBuilder("");
                for (int i = 0; i < NumberOfWheels; i++)
                {
                    a.Append(_wheelsTypes[i].ToString());
                }
                if (_selectedWheels != a.ToString())
                {
                    _selectedWheels = a.ToString();
                    OnPropertyChanged("SelectedWheels");
                }
            }
        }

        [TaskPane("SelectedBarsCaption", "SelectedBarsTip", "InfoAndControlGroup", 3, true, ControlType.TextBoxReadOnly)]
        public string SelectedBars
        {
            get
            {
                return _selectedBars;
            }
            set
            {
                if (_selectedBars != a.ToString())
                {
                    UpdateSelecteBarsString();
                }
            }
        }

        [TaskPane("WheelsStateCaption", "WheelsStateTip", "InfoAndControlGroup", 4, true, ControlType.TextBoxReadOnly, ValidationType.RegEx, "[A-Z0-9, ")]
        public string WheelsState
        {
            get
            {
                return _wheelsInitialPositions;
            }
            set
            {


                if (_wheelsInitialPositions != value)
                {
                    _wheelsInitialPositions = value;
                    OnPropertyChanged("WheelsState");
                }
            }
        }


        [TaskPane("InitialOffsetCaption", "InitialOffsetTip", "InfoAndControlGroup", 5, false, ControlType.TextBoxReadOnly)]
        public string SelectedInitOffset
        {
            get
            {
                return _selectedInitOffset;
            }
            set
            {
                _selectedInitOffset = value;
                OnPropertyChanged("SelectedInitOffset");
            }
        }

        [TaskPane("FVFeatureCaption", "FVFeatureTip", "InfoAndControlGroup", 6, false, ControlType.TextBoxReadOnly)]
        public string SelectedFVFeature
        {
            get
            {
                return _selectedFVFeature;
            }
            set
            {

                _selectedFVFeature = value;

                OnPropertyChanged("SelectedFVFeature");
            }
        }



        #endregion
    }
}
