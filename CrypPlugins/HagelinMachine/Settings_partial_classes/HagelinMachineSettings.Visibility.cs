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
using System.Linq;
using System.Text;
using System.Windows;
using static HagelinMachine.HagelinConstants;
using static HagelinMachine.HagelinEnums;

namespace CrypTool.Plugins.HagelinMachine
{
    public partial class HagelinMachineSettings : ISettings
    {
        #region Visibility
        private void SetPluginToState(PluginStates value)
        {
            switch (value)
            {
                case PluginStates.ModelSelection:
                    _hintMessage = CrypTool.Plugins.HagelinMachine.Properties.Resources.Step11Caption;
                    OnPropertyChanged("HintMessage");
                    HideAll();
                    ShowModelGroup();
                    HideSettingsElement("Back");
                    break;

                case PluginStates.WheelsSelection:
                    _hintMessage = CrypTool.Plugins.HagelinMachine.Properties.Resources.Step12Caption;
                    OnPropertyChanged("HintMessage");
                    HideAll();
                    ShowWheelsGroup();
                    ShowSettingsElement("SelectedModel");
                    ShowSettingsElement("SelectedWheels");
                    ShowSettingsElement("Back");
                    break;

                case PluginStates.BarsSelection:
                    _hintMessage = CrypTool.Plugins.HagelinMachine.Properties.Resources.Step13Caption;
                    OnPropertyChanged("HintMessage");
                    HideAll();
                    ShowModelBars();
                    ShowSettingsElement("SelectedModel");
                    ShowSettingsElement("SelectedWheels");
                    ShowSettingsElement("SelectedBars");
                    break;

                case PluginStates.InnerKeySetupPins:
                    _hintMessage = CrypTool.Plugins.HagelinMachine.Properties.Resources.Step21Caption;
                    HideAll();
                    ShowModelWheels();
                    OnPropertyChanged("HintMessage");
                    ShowSettingsElement("SelectedModel");
                    ShowSettingsElement("SelectedWheels");
                    UpdateSelecteBarsString();
                    ShowSettingsElement("SelectedBars");
                    break;

                case PluginStates.InnerKeySetupLugs:
                    _hintMessage = CrypTool.Plugins.HagelinMachine.Properties.Resources.Step22Caption;
                    OnPropertyChanged("HintMessage");
                    HideAll();
                    ShowModelLugs();
                    ShowSettingsElement("SelectedModel");
                    ShowSettingsElement("SelectedWheels");
                    ShowSettingsElement("SelectedBars");

                    break;
                case PluginStates.ExternalKeySetup:
                    _hintMessage = CrypTool.Plugins.HagelinMachine.Properties.Resources.Step3Caption;
                    OnPropertyChanged("HintMessage");
                    HideAll();
                    ShowExternalKeyGroup();
                    ShowSettingsElement("SelectedModel");
                    ShowSettingsElement("SelectedWheels");
                    ShowSettingsElement("SelectedBars");
                    ShowSettingsElement("WheelsState");
                    break;

                case PluginStates.ModeOpGroup:
                    _hintMessage = CrypTool.Plugins.HagelinMachine.Properties.Resources.Step4Caption;
                    OnPropertyChanged("HintMessage");
                    HideAll();
                    ShowOtherOptionsGroup();
                    ShowSettingsElement("SelectedModel");
                    ShowSettingsElement("SelectedWheels");
                    ShowSettingsElement("SelectedBars");
                    ShowSettingsElement("WheelsState");
                    ShowSettingsElement("Apply");
                    break;

                case PluginStates.Encryption:
                    HideAll();
                    _hintMessage = CrypTool.Plugins.HagelinMachine.Properties.Resources.ReadyToCaption + TranslateMode(_mode) + ". "+ CrypTool.Plugins.HagelinMachine.Properties.Resources.CanStartCaption;
                    OnPropertyChanged("HintMessage");
                    ShowSettingsElement("SelectedModel");
                    ShowSettingsElement("SelectedWheels");
                    ShowSettingsElement("SelectedBars");
                    ShowSettingsElement("SelectedInitOffset");
                    ShowSettingsElement("WheelsState");
                    HideSettingsElement("Apply");
                    if (FVFeatureIsActive)
                    {
                        ShowSettingsElement("SelectedFVFeature");
                        ShowSettingsElement("SelectedFVOffset");
                    }
                    break;

                case PluginStates.EncryptionDone:
                    _hintMessage = "EncryptionDoneCaption";
                    OnPropertyChanged("HintMessage");
                    break;

                default:
                    break;
            }
            OnPropertyChanged("PluginState");
        }

        /// <summary>
        /// Translates the mode to a string
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private string TranslateMode(ModeType mode)
        {
            switch (mode)
            {
                case ModeType.Encrypt:
                    return Properties.Resources.EncryptCaption;
                case ModeType.Decrypt:
                    return Properties.Resources.DecryptCaption;
                default:
                    throw new Exception("There is nothing besides encryption and decryption...");
            }
        }

        private void ShowSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Visible)));
                OnPropertyChanged(element);
            }
        }

        private void HideSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Collapsed)));
                OnPropertyChanged(element);
            }
        }

        private void ShowModelWheels()
        {
            for (int i = 1; i < NumberOfWheels + 1; i++)
            {
                ShowSettingsElement("Wheel" + i + "Pins");
                ShowSettingsElement("Wheel" + i + "InitialState");
            }
        }

        private void HideModelWheels()
        {
            for (int i = 1; i < maxNumberOfWheels + 1; i++)
            {
                HideSettingsElement("Wheel" + i + "TypeName");
                HideSettingsElement("Wheel" + i + "Pins");
                HideSettingsElement("Wheel" + i + "InitialState");
            }
        }

        public void SetExternalKeyShow()
        {
            StringBuilder externalKey = new StringBuilder();
            for (int i = 0; i < this.NumberOfWheels; i++)
            {
                string cur = _wheelsInitialStates[i];
                if (cur != null)
                {
                    externalKey.Append(cur.Substring(0, cur.IndexOf(",") + 1));
                }
            }
            _wheelsInitialPositions = externalKey.ToString();
        }

        private void ShowWheelsGroup()
        {
            for (int i = 1; i < _numberOfWheels + 1; i++)
            {
                ShowSettingsElement("Wheel" + i + "TypeName");
                ShowSettingsElement("Wheel" + i + "InitialState");
            }

        }
        private void HideWheelsGroup()
        {
            for (int i = 1; i < maxNumberOfWheels + 1; i++)
            {
                HideSettingsElement("Wheel" + i + "TypeName");
                HideSettingsElement("Wheel" + i + "InitialState");
            }
        }

        private void ShowModelBars()
        {
            ShowSettingsElement("NumberOfBars");
            for (int i = 1; i < _numberOfBars + 1; i++)
            {
                ShowSettingsElement("Bar" + i + "Type");
                ShowSettingsElement("Bar" + i + "HasLugs");
                ShowSettingsElement("Bar" + i + "CamTypes");
                ShowSettingsElement("Bar" + i + "ToothType");
            }
        }

        private void ShowModelLugs()
        {
            for (int i = 1; i < _numberOfBars + 1; i++)
            {
                ShowSettingsElement("Bar" + i + "CamTypes");
                if (_barHasLugs[i - 1])
                {
                    ShowSettingsElement("Bar" + i + "Lugs");
                }
                else
                {
                    HideSettingsElement("Bar" + i + "Lugs");
                }
            }
        }

        private void HideModelBars()
        {
            HideSettingsElement("NumberOfBars");
            for (int i = 1; i < maxNumberOfBars + 1; i++)
            {
                HideSettingsElement("Bar" + i + "Type");
                HideSettingsElement("Bar" + i + "HasLugs");
                HideSettingsElement("Bar" + i + "CamTypes");
                HideSettingsElement("Bar" + i + "Lugs");
                HideSettingsElement("Bar" + i + "ToothType");
                _barsShown = false;
            }
        }

        private void HideBarsGroups()
        {
            for (int i = 1; i < maxNumberOfBars + 1; i++)
            {
                HideSettingsElement("Bar" + i + "Type");
                HideSettingsElement("Bar" + i + "HasLugs");
                HideSettingsElement("Bar" + i + "CamTypes");
                HideSettingsElement("Bar" + i + "Lugs");
                HideSettingsElement("Bar" + i + "ToothType");
            }
        }

        private void ShowBarsGroups()
        {
            for (int i = 1; i < _numberOfBars + 1; i++)
            {
                ShowSettingsElement("Bar" + i + "Type");
                ShowSettingsElement("Bar" + i + "HasLugs");
                ShowSettingsElement("Bar" + i + "CamTypes");
                ShowSettingsElement("Bar" + i + "Lugs");
                ShowSettingsElement("Bar" + i + "ToothType");
            }
        }

        private void ShowModelGroup()
        {
            ShowSettingsElement("Model");
            ShowSettingsElement("Apply");
            ShowSettingsElement("Mode");
        }

        private void HideModelGroup()
        {
            HideSettingsElement("Model");
        }

        private void ShowExternalKeyGroup()
        {
            for (int i = 1; i < this.NumberOfWheels + 1; i++)
            {
                ShowSettingsElement("RotateWheelDown_" + i);
                ShowSettingsElement("RotateWheelUp_" + i);
            }
        }

        private void HideExternalKeyGroup()
        {
            for (int i = 1; i < maxNumberOfWheels + 1; i++)
            {
                HideSettingsElement("RotateWheelDown_" + i);
                HideSettingsElement("RotateWheelUp_" + i);
            }
        }

        private void ShowOtherOptionsGroup()
        {

            ShowSettingsElement("InitOffset");
            ShowSettingsElement("UseZAsSpace");
            ShowSettingsElement("UnknownSymbolHandling");

            bool ModelHasFVFeature = new[] { ModelType.CX52a, ModelType.CX52b, ModelType.CX52c, ModelType.Custom }.Contains(_model);
            if (ModelHasFVFeature)
            {
            //    ShowSettingsElement("Mode");
                ShowSettingsElement("FVFeatureIsActive");
                SetVisibility();
            }
        }

        private void HideModeGroup()
        {
            HideSettingsElement("Mode");
            HideSettingsElement("InitOffset");
            HideSettingsElement("FVFeatureIsActive");
            HideSettingsElement("FVOffset");
            HideSettingsElement("UseZAsSpace");
            HideSettingsElement("UnknownSymbolHandling");
        }

        private void ShowModeGroup()
        {

            ShowSettingsElement("Mode");
            ShowSettingsElement("InitOffset");
            ShowSettingsElement("FVFeatureIsActive");
            ShowSettingsElement("FVOffset");
            ShowSettingsElement("UseZAsSpace");
            ShowSettingsElement("UnknownSymbolHandling");
        }

        private void ShowUtilitiesGroup()
        {
            ShowSettingsElement("WheelsShown");
            ShowSettingsElement("BarsShown");
            ShowSettingsElement("OnlyLugsShown");
            ShowSettingsElement("RandomizeLugs");
            ShowSettingsElement("RandomizePins");
            ShowSettingsElement("ResetWheels");
        }

        private void HideUtilitiesGroup()
        {
            HideSettingsElement("WheelsShown");
            HideSettingsElement("BarsShown");
            HideSettingsElement("OnlyLugsShown");
            HideSettingsElement("RandomizeLugs");
            HideSettingsElement("RandomizePins");
            HideSettingsElement("ResetWheels");
        }

        private void HideInfoGroup()
        {
            HideSettingsElement("SelectedModel");
            HideSettingsElement("SelectedWheels");
            HideSettingsElement("SelectedBars");
            HideSettingsElement("WheelsState");
            HideSettingsElement("SelectedFVOffset");
            HideSettingsElement("SelectedInitOffset");
            HideSettingsElement("SelectedFVFeature");
        }

        private void ShowInfoGroup()
        {
            ShowSettingsElement("SelectedModel");
            ShowSettingsElement("SelectedWheels");
            ShowSettingsElement("SelectedBars");
            ShowSettingsElement("WheelsState");
            ShowSettingsElement("SelectedFVOffset");
            ShowSettingsElement("SelectedInitOffset");
            ShowSettingsElement("SelectedFVFeature");
        }

        private void HideAll()
        {
            HideBarsGroups();
            HideModelWheels();
            HideWheelsGroup();
            HideModelGroup();
            HideExternalKeyGroup();
            HideUtilitiesGroup();
            HideInfoGroup();
            HideModelBars();
            HideModeGroup();
        }

        private void ShowAll()
        {
            ShowBarsGroups();
            ShowModelWheels();
            ShowWheelsGroup();
            ShowModelGroup();
            ShowExternalKeyGroup();
            ShowUtilitiesGroup();
            ShowInfoGroup();
            ShowModelBars();
            ShowModeGroup();
        }
        private void ShowOnlyLugsOfBars()
        {
            for (int i = 1; i < maxNumberOfBars + 1; i++)
            {
                HideSettingsElement("Bar" + i + "Type");
                HideSettingsElement("Bar" + i + "HasLugs");
                HideSettingsElement("Bar" + i + "CamTypes");
                ShowSettingsElement("Bar" + i + "Lugs");
                HideSettingsElement("Bar" + i + "ToothType");
            }
        }

        private void SetVisibility()
        {
            if (_wheelsShown)
            {
                ShowModelWheels();
            }
            else
            {
                HideModelWheels();
            }

            if (_barsShown)
            {
                ShowModelBars();
            }
            else
            {
                HideBarsGroups();
            }

            if (_numberOfWheelsAndBarsShown)
            {
                return;
            }
            else
            {
                HideSettingsElement("NumberOfBars");
                HideSettingsElement("NumberOfWheels");

            }

            if (_FVfeatureIsActive)
            {
                ShowSettingsElement("FVOffset");

            }
            else
            {
                HideSettingsElement("FVOffset");

            }

        }
        #endregion
    }
}
