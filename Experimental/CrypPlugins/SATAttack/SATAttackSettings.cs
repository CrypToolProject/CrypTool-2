/*
   Copyright 2011 CrypTool 2 Team <ct2contact@cryptool.org>

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
using System.Windows;

namespace CrypTool.Plugins.SATAttack
{
    public class SATAttackSettings : ISettings
    {
        #region Private Variables

        private int attackMode;
        private int inputSelection;
        private string openFilename;
        private string inputHashValue;
        private string secondPreimage;
        private string plaintext;
        private string ciphertext;
        private bool showFileInputSelection = false;
        private bool showPreimageAttackSettings = true;
        private bool showSecondPreimageAttackSettings;
        private bool showKeyRecoveryAttackSettings;
        private string mainFunctionName;
        private string cnfFileName;
        private bool onlyCnfOutput = false;
        private bool fixBits = false;
        private bool showFixBitsSettings = false;
        private string fixedBits;

        #endregion

        #region TaskPane Settings

        #region General Settings

        [TaskPane("AttackModeCaption", "AttackModeTooltip", null, 1, false, ControlType.ComboBox,
            new string[] { "PreimageAttack", "SecondPreimageAttack", "KeyRecoveryAttack"})]
        public int AttackMode
        {
            get
            {
                return attackMode;
            }
            set
            {
                if (attackMode != value)
                {
                    attackMode = value;
                    OnPropertyChanged("AttackMode");

                    if (attackMode == 0) // Preimage Attack
                    {
                        showPreimageAttackSettings = true;
                        showSecondPreimageAttackSettings = false;
                        showKeyRecoveryAttackSettings = false;
                    }
                    else if (attackMode == 1) // Second Preimage Attack
                    {
                        showPreimageAttackSettings = false;
                        showSecondPreimageAttackSettings = true;
                        showKeyRecoveryAttackSettings = false;
                    }
                    else if (attackMode == 2) // Key Recovery Attack
                    {
                        showPreimageAttackSettings = false;
                        showSecondPreimageAttackSettings = false;
                        showKeyRecoveryAttackSettings = true;
                    }

                    UpdateTaskPaneVisibility();
                }
            }
        }

        [TaskPane("InputSelectionCaption", "InputSelectionTooltip", null, 2, false, ControlType.ComboBox,
            new String[] { "InputSelectionTextInput", "InputSelectionFileInput" })]
        public int InputSelection
        {
            get
            {
                return inputSelection;
            }
            set
            {
                if (inputSelection != value)
                {
                    inputSelection = value;
                    OnPropertyChanged("InputSelection");

                    if (inputSelection == 0) // Text Input
                    {
                        showFileInputSelection = false;
                    }
                    else if (inputSelection == 1) // File Input
                    {
                        showFileInputSelection = true;
                    }

                    UpdateTaskPaneVisibility();
                }
            }
        }

        [TaskPane("FileInputCaption", "FileInputTooltip", null, 3, false, ControlType.OpenFileDialog,
            FileExtension = "All Files (*.*)|*.*")]
        public string InputFile
        {
            get { return openFilename; }
            set
            {
                if (value != openFilename)
                {
                    openFilename = value;
                    OnPropertyChanged("InputFile");
                }
            }
        }

        [TaskPane("MainFunctionCaption", "MainFunctionTooltip", null, 4, false, ControlType.TextBox)]
        public string MainFunctionName
        {
            get
            {
                return mainFunctionName;
            }
            set
            {
                if (value != mainFunctionName)
                {
                    mainFunctionName = value;
                    OnPropertyChanged("MainFunctionName");
                }
            }
        }

        [TaskPane("CnfFileCaption", "CnfFileTooltip", null, 5, false, ControlType.SaveFileDialog, "All Files (*.*)|*.*")]
        public string CnfFileName
        {
            get { return cnfFileName; }
            set
            {
                cnfFileName = value;
                OnPropertyChanged("CnfFileName");
                           
            }        
        }

        [TaskPane("ClearFileNameCaption", "ClearFileNameTooltip", null, 6, false, ControlType.Button)]
        public void ClearFileName()
        {
            CnfFileName = null;
        }

        [TaskPane("OnlyCnfOutputCaption", "OnlyCnfOutputTooltip", null, 7, false, ControlType.CheckBox)]
        public bool OnlyCnfOutput
        {
            get { return onlyCnfOutput; }
            set
            {
                if (value != onlyCnfOutput)
                {
                    onlyCnfOutput = value;
                    OnPropertyChanged("OnlyCnfOutput");
                }
            }        
        }

        [TaskPane("FixBitsCaption", "FixBitsTooltip", null, 8, false, ControlType.CheckBox)]
        public bool FixBits
        {
            get 
            {
                return fixBits; 
            }
            set
            {
                if (value != fixBits)
                {
                    fixBits = value;
                    OnPropertyChanged("FixBits");

                    if (fixBits == true)
                    {
                        showFixBitsSettings = true;
                    }
                    else
                    {
                        showFixBitsSettings = false;                    
                    }

                    UpdateTaskPaneVisibility();
                }
            }
        }

        #endregion

        #region Preimage Attack Options

        [TaskPane("InputHashValueCaption", "InputHashValueTooltip", "PreimageAttackOptions", 1, false, ControlType.TextBox)]
        public string InputHashValue
        {
            get
            {
                return inputHashValue;
            }
            set
            {
                if (value != inputHashValue)
                {
                    inputHashValue = value;
                    OnPropertyChanged("InputHashValue");
                }
            }
        }

        #endregion

        #region Second-Preimage Attack Options

        [TaskPane("InputMessageCaption", "InputMessageTooltip", "SecondPreimageAttackOptions", 1, false, ControlType.TextBox)]
        public string SecondPreimage
        {
            get
            {
                return secondPreimage;
            }
            set
            {
                if (value != secondPreimage)
                {
                    secondPreimage = value;
                    OnPropertyChanged("SecondPreimage");
                }
            }
        }

        #endregion

        #region Key Recovery Attack Options

        [TaskPane("PlaintextCaption", "PlaintextTooltip", "KeyRecoveryAttackOptions", 1, false, ControlType.TextBox)]
        public string Plaintext
        {
            get
            {
                return plaintext;
            }
            set
            {
                if (value != plaintext)
                {
                    plaintext = value;
                    OnPropertyChanged("Plaintext");
                }
            }
        }

        [TaskPane("CiphertextCaption", "CiphertextTooltip", "KeyRecoveryAttackOptions", 2, false, ControlType.TextBox)]
        public string Ciphertext
        {
            get
            {
                return ciphertext;
            }
            set
            {
                if (value != ciphertext)
                {
                    ciphertext = value;
                    OnPropertyChanged("Ciphertext");
                }
            }
        }

        #endregion

        #region Fix Bits Settings

        [TaskPane("FixedBitsCaption", "FixedBitsTooltip", "FixBitsOptions", 1, false, ControlType.TextBox)]
        public string FixedBits
        {
            get 
            {
                return fixedBits;
            }
            set
            {
                if (value != fixedBits)
                {
                    fixedBits = value;
                    OnPropertyChanged("FixedBits");
                }
            }
        
        }
        
        #endregion

        #endregion

        #region Events

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        private void settingChanged(string setting, Visibility vis)
        {
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(setting, vis)));
        }

        internal void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
                return;

            if (showPreimageAttackSettings)
            {
                settingChanged("InputHashValue", Visibility.Visible);
                settingChanged("SecondPreimage", Visibility.Collapsed);
                settingChanged("Plaintext", Visibility.Collapsed);
                settingChanged("Ciphertext", Visibility.Collapsed);
            }
            else if (showSecondPreimageAttackSettings)
            {
                settingChanged("InputHashValue", Visibility.Visible);
                settingChanged("SecondPreimage", Visibility.Visible);
                settingChanged("Plaintext", Visibility.Collapsed);
                settingChanged("Ciphertext", Visibility.Collapsed);
            }
            else if (showKeyRecoveryAttackSettings)
            {
                settingChanged("InputHashValue", Visibility.Collapsed);
                settingChanged("SecondPreimage", Visibility.Collapsed);
                settingChanged("Plaintext", Visibility.Visible);
                settingChanged("Ciphertext", Visibility.Visible);
            }

            if (showFileInputSelection)
            {
                settingChanged("InputFile", Visibility.Visible);
            }
            else if (!showFileInputSelection)
            {
                settingChanged("InputFile", Visibility.Collapsed);
            }

            if (showFixBitsSettings)
            {
                settingChanged("FixedBits", Visibility.Visible);
            }
            else if (!showFixBitsSettings)
            {
                settingChanged("FixedBits", Visibility.Collapsed);            
            }
        }

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
