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
using static HagelinMachine.HagelinEnums;
#pragma warning disable CS0105 // The using directive for 'HagelinEnums' appeared previously in this namespace
#pragma warning restore CS0105 // The using directive for 'HagelinEnums' appeared previously in this namespace

namespace CrypTool.Plugins.HagelinMachine
{
    public partial class HagelinMachineSettings : ISettings
    {
        [TaskPane("UseZCaption", "UseZTip", "TextOptionsGroup", 3, false, ControlType.CheckBox)]

        public bool UseZAsSpace
        {
            get
            {
                return _useZAsSpace;
            }
            set
            {
                if (value != _useZAsSpace)
                {
                    _useZAsSpace = value;
                    OnPropertyChanged("UseZAsSpace");
                }
            }
        }

        [TaskPane("UnknownSymbolCaption", "UnknownSymbolTip", "TextOptionsGroup", 4, false, ControlType.ComboBox, new string[] { "Ignore", "Remove", "ReplaceWithQuestionMark" })]
        public UnknownSymbolHandling UnknownSymbolHandling
        {
            get { return _unknownSymbolHandling; }
            set
            {
                if (value != _unknownSymbolHandling)
                {
                    _unknownSymbolHandling = value;
                    OnPropertyChanged("UnknownSymbolHandling");
                }
            }
        }

        #region OffSetPrintWheel
        
        [TaskPane("InitialOffsetSetCaption", "InitialOffsetSetTip", "PrinterOffsetGroup", 200, false, ControlType.ComboBox, new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25" })]

        public int InitOffset
        {
            get
            {
                return _initOffset;
            }
            set
            {
                _initOffset = (int)value;
                OnPropertyChanged("InitOffset");
                SelectedInitOffset = _initOffset.ToString();
            }
        }

        [TaskPane("FVFeatureCaption", "FVFeatureTip", "PrinterOffsetGroup", 201, false, ControlType.CheckBox, "", null)]
        public bool FVFeatureIsActive
        {
            get { return this._FVfeatureIsActive; }
            set
            {
                if (value)
                {
                    this._FVfeatureIsActive = value;
                    OnPropertyChanged("FVFeatureIsActive");
                    SetVisibility();
                    _selectedFVFeature = "On";
                    OnPropertyChanged("SelectedFVFeature");


                }
                else
                {
                    this._FVfeatureIsActive = value;
                    OnPropertyChanged("FVFeatureIsActive");
                    SetVisibility();
                    _selectedFVFeature = "Off";
                    OnPropertyChanged("SelectedFVFeature");

                }
            }
        }

        [TaskPane("OperationModeCaption", "OperationModeTip", "ModeOfOperationGroup", 24, false, ControlType.ComboBox, new string[] { "EncryptCaption", "DecryptCaption" })]
        public ModeType Mode
        {
            get
            {
                return _mode;
            }
            set
            {
                if (_mode != value)
                {
                    _mode = value;
                    // HOWTO: MUST be called every time a property value changes with correct parameter name
                    OnPropertyChanged("Mode");
                }
            }
        }
        #endregion
    }
}
