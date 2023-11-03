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

namespace CrypTool.Plugins.HagelinMachine
{
    public partial class HagelinMachineSettings : ISettings
    {
        #region Controls

        [TaskPane("CaptionDownWheel1", "TipDownWheel", "RotateDownLabel", 3, false, ControlType.Button)]
        public void RotateWheelDown_1()
        {
            RotateWheelDown(0);

        }

        [TaskPane("CaptionUpWheel1", "TipUpWheel", "RotateUpLabel", 3, false, ControlType.Button)]
        public void RotateWheelUp_1()
        {
            RotateWheelUp(0);
        }

        [TaskPane("CaptionDownWheel2", "TipDownWheel", "RotateDownLabel", 5, false, ControlType.Button)]
        public void RotateWheelDown_2() { RotateWheelDown(1); }


        [TaskPane("CaptionDownWheel3", "TipDownWheel", "RotateDownLabel", 6, false, ControlType.Button)]
        public void RotateWheelDown_3() { RotateWheelDown(2); }

        [TaskPane("CaptionDownWheel4", "TipDownWheel", "RotateDownLabel", 7, false, ControlType.Button)]
        public void RotateWheelDown_4() { RotateWheelDown(3); }

        [TaskPane("CaptionDownWheel5", "TipDownWheel", "RotateDownLabel", 8, false, ControlType.Button)]
        public void RotateWheelDown_5() { RotateWheelDown(4); }

        [TaskPane("CaptionDownWheel6", "TipDownWheel", "RotateDownLabel", 9, false, ControlType.Button)]
        public void RotateWheelDown_6() { RotateWheelDown(5); }

        [TaskPane("CaptionDownWheel7", "TipDownWheel", "RotateDownLabel", 10, false, ControlType.Button)]
        public void RotateWheelDown_7() { RotateWheelDown(6); }

        [TaskPane("CaptionDownWheel8", "TipDownWheel", "RotateDownLabel", 11, false, ControlType.Button)]
        public void RotateWheelDown_8() { RotateWheelDown(7); }

        [TaskPane("CaptionDownWheel9", "TipDownWheel", "RotateDownLabel", 12, false, ControlType.Button)]
        public void RotateWheelDown_9() { RotateWheelDown(8); }

        [TaskPane("CaptionDownWheel10", "TipDownWheel", "RotateDownLabel", 13, false, ControlType.Button)]
        public void RotateWheelDown_10() { RotateWheelDown(9); }

        [TaskPane("CaptionDownWheel11", "TipDownWheel", "RotateDownLabel", 14, false, ControlType.Button)]
        public void RotateWheelDown_11() { RotateWheelDown(10); }

        [TaskPane("CaptionDownWheel12", "TipDownWheel", "RotateDownLabel", 15, false, ControlType.Button)]
        public void RotateWheelDown_12() { RotateWheelDown(11); }






        /// <summary>

        /// </summary>

        [TaskPane("CaptionUpWheel2", "TipUpWheel", "RotateUpLabel", 4, false, ControlType.Button)]
        public void RotateWheelUp_2() { RotateWheelUp(1); }


        [TaskPane("CaptionUpWheel3", "TipUpWheel", "RotateUpLabel", 5, false, ControlType.Button)]
        public void RotateWheelUp_3() { RotateWheelUp(2); }

        [TaskPane("CaptionUpWheel4", "TipUpWheel", "RotateUpLabel", 6, false, ControlType.Button)]
        public void RotateWheelUp_4() { RotateWheelUp(3); }

        [TaskPane("CaptionUpWheel5", "TipUpWheel", "RotateUpLabel", 7, false, ControlType.Button)]
        public void RotateWheelUp_5() { RotateWheelUp(4); }

        [TaskPane("CaptionUpWheel6", "TipUpWheel", "RotateUpLabel", 8, false, ControlType.Button)]
        public void RotateWheelUp_6() { RotateWheelUp(5); }

        [TaskPane("CaptionUpWheel7", "TipUpWheel", "RotateUpLabel", 9, false, ControlType.Button)]
        public void RotateWheelUp_7() { RotateWheelUp(6); }

        //  [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane("CaptionUpWheel8", "TipUpWheel", "RotateUpLabel", 10, false, ControlType.Button)]
        public void RotateWheelUp_8() { RotateWheelUp(7); }

        [TaskPane("CaptionUpWheel9", "TipUpWheel", "RotateUpLabel", 11, false, ControlType.Button)]
        public void RotateWheelUp_9() { RotateWheelUp(8); }

        [TaskPane("CaptionUpWheel10", "TipUpWheel", "RotateUpLabel", 12, false, ControlType.Button)]
        public void RotateWheelUp_10() { RotateWheelUp(9); }

        [TaskPane("CaptionUpWheel11", "TipUpWheel", "RotateUpLabel", 13, false, ControlType.Button)]
        public void RotateWheelUp_11() { RotateWheelUp(10); }

        [TaskPane("CaptionUpWheel12", "TipUpWheel", "RotateUpLabel", 14, false, ControlType.Button)]
        public void RotateWheelUp_12() { RotateWheelUp(11); }








        [TaskPane("ApplyButtonCaption", "ApplyButtonTip", "InfoAndControlGroup", 8, true, ControlType.Button)]
        public void Apply() { ApplyButton(); }

        private void ApplyButton()
        {

            bool notLastStep = (_pluginState != PluginStates.Encryption);
            if (notLastStep)
            {
                _pluginState += 1;
                if ((_pluginState == PluginStates.BarsSelection) && (_model != ModelType.Custom)) // We ignore selection of the bars for all the models, except custom
                    _pluginState += 1;

                SetPluginToState(_pluginState);
            }

        }

        [TaskPane("BackButtonCaption", "Return to the previous window", "InfoAndControlGroup", 9, true, ControlType.Button)]
        public void Back() { BackButton(); }
        private void BackButton()
        {

            bool notFirstStep = (_pluginState != PluginStates.ModelSelection);
            if (notFirstStep)
            {
                _pluginState -= 1;
                if ((_pluginState == PluginStates.BarsSelection) && (_model != ModelType.Custom)) // We ignore selection of the bars for all the models, except custom
                    _pluginState -= 1;
                SetPluginToState(_pluginState);
            }

        }


        [TaskPane("Reset", "Reset the model and start from Step 1", "InfoAndControlGroup", 10, false, ControlType.Button)]
        public void Reset() { ResetButton(); }

        private void ResetButton()
        {
            _model = ModelType.CX52a;
            ApplyModel();
            _initOffset = 0;
            OnPropertyChanged("InitOffset");
            InitOffset = 0;
            _FVfeatureIsActive = false;
            OnPropertyChanged("FVfeatureIsActive");
            _unknownSymbolHandling = 0;
            OnPropertyChanged("UnknownSymbolHandling");
            _mode = ModeType.Encrypt;
            OnPropertyChanged("Mode");
            _useZAsSpace = false;
            OnPropertyChanged("UseZAsSpace");
            //    OnPropertyChanged("Model");
            OnPropertyChanged("SelectedModel");
            _pluginState = PluginStates.ModelSelection;

            OnPropertyChanged("PluginState");
            SetPluginToState(_pluginState);
        }


        [TaskPane("ShowAllCaption", "ShowAllTip", "InfoAndControlGroup", 11, true, ControlType.CheckBox)]
        public bool ShowAllCheckBox
        {
            get
            {
                return _showAllSettings;
            }
            set
            {
                _showAllSettings = value;
                OnPropertyChanged("ShowAllCheckBox");
                if (value)
                    ShowAll();
                else
                    SetPluginToState(_pluginState);
            }
        }

        #endregion

        #region Utilities
        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane("ShowWheelsCaption", "", "UtilitiesGroup", 100, true, ControlType.CheckBox, "", null)]
        public bool WheelsShown
        {
            get { return this._wheelsShown; }
            set
            {
                if (value != this._wheelsShown)
                {
                    this._wheelsShown = value;
                    OnPropertyChanged("WheelsShown");
                    SetVisibility();
                }
            }
        }

        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane("ShowBarsCaption", "", "UtilitiesGroup", 101, true, ControlType.CheckBox, "", null)]
        public bool BarsShown
        {
            get { return this._barsShown; }
            set
            {
                if (value != this._barsShown)
                {
                    this._barsShown = value;
                    OnPropertyChanged("BarsShown");
                    SetVisibility();
                }
            }
        }

        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane("Show only lugs", "ShowOnlyLugsTip", "UtilitiesGroup", 102, false, ControlType.CheckBox, "", null)]
        public bool OnlyLugsShown
        {
            get { return this._onlyLugsShown; }
            set
            {
                ShowOnlyLugsOfBars();
                this._onlyLugsShown = value;
                OnPropertyChanged("OnlyLugsShown");
            }
        }

        [TaskPane("RandomizePinsCaption", "RandomizePinsTip", "UtilitiesGroup", 103, false, ControlType.Button)]
        public void RandomizePins() { SetAllWheelsPinsToRandom(); }

        [TaskPane("RandomizeLugsCaption", "RandomizeLugsTip", "UtilitiesGroup", 104, false, ControlType.Button)]
        public void RandomizeLugs() { SetAllLugsToRandom(); }

        [TaskPane("ResetWheelsCaption", "ResetWheelsTip", "UtilitiesGroup", 105, false, ControlType.Button)]
        public void ResetWheels() { SetAllWheelsToFirstPosition(); }

        #endregion
    }
}
