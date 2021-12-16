/*
   Copyright 2016, Eugen Antal and Tomáš Sovič, FEI STU Bratislava

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
using System;
using System.ComponentModel;
using System.Windows;

namespace CrypTool.Fialka
{
    public class FialkaSettings : ISettings
    {

        #region Variables

        private FialkaInternalState _internalState;
        public FialkaInternalState internalState
        {
            get
            {
                if (_internalState == null)
                {
                    _internalState = new FialkaInternalState();
                }
                return _internalState;
            }
            set => _internalState = value;
        }

        /*
         * Rotor settings (also the daily key) is set up in FialkaInternalState (model) these variables are only "bridge" references to the internalState.
         */
        public int[] RotorOrder { get => internalState.rotorOrders; set => internalState.rotorOrders = value; }
        public int[] RotorOffsets { get => internalState.rotorOffsets; set => internalState.rotorOffsets = value; }
        public int[] RotorRingOffsets { get => internalState.ringOffsets; set => internalState.ringOffsets = value; }
        public int[] RotorCoreOrders { get => internalState.coreOrders; set => internalState.coreOrders = value; }
        public int[] RotorCoreOffsets { get => internalState.coreOffsets; set => internalState.coreOffsets = value; }
        public int[] PunchCard { get => internalState.punchCard; set => internalState.punchCard = value; }
        public int[] RotorCoreOrientations { get => internalState.coreOrientation; set => internalState.coreOrientation = value; }

        private FialkaEnums.rotorSeries rotorSeries { get => internalState.rotorSeries; set => internalState.rotorSeries = value; }
        private FialkaEnums.numLockType numlockType { get => internalState.numlockType; set => internalState.numlockType = value; }
        private FialkaEnums.printHeadMapping printHeadMapping { get => internalState.printHead; set => internalState.printHead = value; }
        private FialkaEnums.countryLayout countryLayout { get => internalState.countryLayout; set => internalState.countryLayout = value; }
        private FialkaEnums.rotorTypes rotorType { get => internalState.rotorType; set => internalState.rotorType = value; }
        private FialkaEnums.operationMode operationMode { get => internalState.opMode; set => internalState.opMode = value; }
        private FialkaEnums.machineModel machineModel { get => internalState.model; set => internalState.model = value; }
        private FialkaEnums.textOperationmMode txtOpmode { get => internalState.txtOpMode; set => internalState.txtOpMode = value; }
        private FialkaEnums.handleInvalidInput handleInput { get => internalState.inputHandler; set => internalState.inputHandler = value; }


        /**/
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        #endregion

        #region Settings not included in FialkaInternalState
        /// <summary>
        /// When will the initial FialkaInternalState restored into state before execution
        /// </summary>
        public enum RestoreInitialSettings { AfterExecution = 0, WhenStopped = 1, WhenInputChanged = 2, Never = 3 };
        public RestoreInitialSettings settingsRestore = RestoreInitialSettings.AfterExecution;

        #endregion

        #region Methods
        private void RotorSwaps(int rotorNumber, int current, int next)
        {
            int rotorNumber2 = Array.IndexOf(RotorOrder, next);
            RotorOrder[rotorNumber] = next;
            RotorOrder[rotorNumber2] = current;
            OnPropertyChanged("Rotor" + (rotorNumber + 1));
            OnPropertyChanged("Rotor" + (rotorNumber2 + 1));
        }

        private void RotorCoreSwaps(int rotorCoreNumber, int current, int next)
        {
            int rotorCoreNumber2 = Array.IndexOf(RotorCoreOrders, next);
            RotorCoreOrders[rotorCoreNumber] = next;
            RotorCoreOrders[rotorCoreNumber2] = current;
            OnPropertyChanged("RotorCore" + (rotorCoreNumber + 1));
            OnPropertyChanged("RotorCore" + (rotorCoreNumber2 + 1));
        }


        private void showSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Visible)));
            }
        }

        private void hideSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Collapsed)));
            }
        }

        private bool workspaceGroupHide = true;
        private bool machineGroupHide = false;
        private bool rotorOrderGroupHide = true;
        private bool rotorOffsetGroupHide = false;
        private bool ringOffsetGroupHide = true;
        private bool rotorCoreOrderGroupHide = true;
        private bool rotorCoreSidesGroupHide = true;
        private bool rotorCoreOffsetGroupHide = true;
        private bool punchCardGroupHide = true;

        private void showProperties()
        {
            setSettingsVisibilityWorkspaceGroup();
            setSettingsVisibilityMachineGroup();
            setSettingsVisibilityRotorOrderGroup();
            setSettingsVisibilityRotorOffsetGroup();
            setSettingsVisibilityRingOffsetGroup();
            setSettingsVisibilityRotorCoreOrderGroup();
            setSettingsVisibilityRotorCoreSidesGroup();
            setSettingsVisibilityRotorCoreOffsetGroup();
            setSettingsVisibilityPunchCardGroup();
        }

        private void setSettingsVisibilityWorkspaceGroup()
        {
            // workspace group
            if (workspaceGroupHide)
            {
                hideSettingsElement("SettingsRestore");
                hideSettingsElement("InputHandler");
                hideSettingsElement("OutputMode");
            }
            else
            {
                showSettingsElement("SettingsRestore");
                showSettingsElement("InputHandler");
                showSettingsElement("OutputMode");
            }
        }

        private void setSettingsVisibilityMachineGroup()
        {
            // machine settings group
            if (machineGroupHide)
            {
                hideSettingsElement("MachineModel");
                hideSettingsElement("OperationMode");
                hideSettingsElement("RotorType");
                hideSettingsElement("CountryLayout");
                hideSettingsElement("KeyboardMapping");
                hideSettingsElement("PrintHeadMapping");
                hideSettingsElement("RotorSeries");
                hideSettingsElement("NumLockType");
                hideSettingsElement("TextOperationMode");
            }
            else
            {
                showSettingsElement("MachineModel");
                showSettingsElement("OperationMode");
                showSettingsElement("RotorType");
                showSettingsElement("CountryLayout");
                showSettingsElement("KeyboardMapping");
                showSettingsElement("PrintHeadMapping");
                showSettingsElement("RotorSeries");

                if (machineModel == FialkaEnums.machineModel.M125)
                {
                    hideSettingsElement("NumLockType");
                    hideSettingsElement("TextOperationMode");
                    // are automatically changed in the FialkaInternalState
                    // refresh the elements
                    OnPropertyChanged("NumLockType");
                    OnPropertyChanged("TextOperationMode");
                }
                else
                {
                    showSettingsElement("NumLockType");
                    showSettingsElement("TextOperationMode");
                }
            }
        }

        private void setSettingsVisibilityRotorOrderGroup()
        {
            if (rotorOrderGroupHide)
            {
                for (int i = 1; i <= FialkaConstants.numberOfRotors; i++)
                {
                    hideSettingsElement("Rotor" + i);
                }
                hideSettingsElement("ResetRotorOrder");
            }
            else
            {
                for (int i = 1; i <= FialkaConstants.numberOfRotors; i++)
                {
                    showSettingsElement("Rotor" + i);
                }
                showSettingsElement("ResetRotorOrder");
            }
        }

        private void setSettingsVisibilityRotorOffsetGroup()
        {
            if (rotorOffsetGroupHide)
            {
                for (int i = 1; i <= FialkaConstants.numberOfRotors; i++)
                {
                    hideSettingsElement("Rotor" + i + "Offset");
                }
                hideSettingsElement("ResetRotorOffsetNull");
                hideSettingsElement("ResetRotorOffsetBase");
            }
            else
            {
                for (int i = 1; i <= FialkaConstants.numberOfRotors; i++)
                {
                    showSettingsElement("Rotor" + i + "Offset");
                }
                showSettingsElement("ResetRotorOffsetNull");
                showSettingsElement("ResetRotorOffsetBase");
            }
        }

        private void setSettingsVisibilityRingOffsetGroup()
        {
            if (ringOffsetGroupHide)
            {
                for (int i = 1; i <= FialkaConstants.numberOfRotors; i++)
                {
                    hideSettingsElement("RotorRing" + i + "Offset");
                }
                hideSettingsElement("ResetRingOffset");
            }
            else
            {
                for (int i = 1; i <= FialkaConstants.numberOfRotors; i++)
                {
                    showSettingsElement("RotorRing" + i + "Offset");
                }
                showSettingsElement("ResetRingOffset");
            }
        }

        private void setSettingsVisibilityRotorCoreOrderGroup()
        {
            if (rotorType == FialkaEnums.rotorTypes.PROTON_I)
            {
                rotorCoreOrderGroupHide = true;
                hideSettingsElement("RotorCoreOrderGroupVisibility");
            }
            else
            {
                showSettingsElement("RotorCoreOrderGroupVisibility");
            }

            if (rotorCoreOrderGroupHide)
            {
                for (int i = 1; i <= FialkaConstants.numberOfRotors; i++)
                {
                    hideSettingsElement("RotorCore" + i);
                }
                hideSettingsElement("ResetRotorCoreOrder");
            }
            else
            {

                for (int i = 1; i <= FialkaConstants.numberOfRotors; i++)
                {
                    showSettingsElement("RotorCore" + i);
                }
                showSettingsElement("ResetRotorCoreOrder");
            }
        }

        private void setSettingsVisibilityRotorCoreSidesGroup()
        {
            if (rotorType == FialkaEnums.rotorTypes.PROTON_I)
            {
                rotorCoreOffsetGroupHide = true;
                hideSettingsElement("RotorCoreSidesGroupVisibility");
            }
            else
            {
                showSettingsElement("RotorCoreSidesGroupVisibility");
            }

            if (rotorCoreSidesGroupHide)
            {
                for (int i = 1; i <= FialkaConstants.numberOfRotors; i++)
                {
                    hideSettingsElement("Rotor" + i + "CoreSide");
                }
                hideSettingsElement("ResetRotorCoreSides");
            }
            else if (rotorType == FialkaEnums.rotorTypes.PROTON_II)
            {
                for (int i = 1; i <= FialkaConstants.numberOfRotors; i++)
                {
                    showSettingsElement("Rotor" + i + "CoreSide");
                }
                showSettingsElement("ResetRotorCoreSides");
            }
        }

        private void setSettingsVisibilityRotorCoreOffsetGroup()
        {
            if (rotorType == FialkaEnums.rotorTypes.PROTON_I)
            {
                rotorCoreOffsetGroupHide = true;
                hideSettingsElement("RotorCoreOffsetGroupVisibility");
            }
            else
            {
                showSettingsElement("RotorCoreOffsetGroupVisibility");
            }

            if (rotorCoreOffsetGroupHide)
            {
                for (int i = 1; i <= FialkaConstants.numberOfRotors; i++)
                {
                    hideSettingsElement("Rotor" + i + "CoreOffset");
                }
                hideSettingsElement("ResetRotorCoreOffset");
            }
            else
            {

                for (int i = 1; i <= FialkaConstants.numberOfRotors; i++)
                {
                    showSettingsElement("Rotor" + i + "CoreOffset");
                }
                showSettingsElement("ResetRotorCoreOffset");
            }
        }

        private void setSettingsVisibilityPunchCardGroup()
        {

            if (punchCardGroupHide)
            {
                for (int i = 1; i <= FialkaConstants.alphabetSize; i++)
                {
                    hideSettingsElement("PunchCard" + i);
                }
                hideSettingsElement("ResetPunchCard");
            }
            else
            {

                for (int i = 1; i <= FialkaConstants.alphabetSize; i++)
                {
                    showSettingsElement("PunchCard" + i);
                }
                showSettingsElement("ResetPunchCard");
            }
        }

        #endregion


        #region TaskPane Settings

        #region Workspace settings

        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane("VisibilityCaption", "VisibilityTooltip", "Workspace", 1, false, ControlType.CheckBox, "", null)]
        public bool WorkspaceGroupVisibility
        {
            get => workspaceGroupHide;
            set
            {
                if (value != workspaceGroupHide)
                {
                    workspaceGroupHide = value;
                    OnPropertyChanged("WorkspaceGroupVisibility");
                    setSettingsVisibilityWorkspaceGroup();
                }
            }
        }

        [TaskPane("SettingsRestoreCaption", "SettingsRestoreTooltip", "Workspace", 2, false, ControlType.ComboBox, new string[] { "SettingsRestoreList1", "SettingsRestoreList2", "SettingsRestoreList3", "SettingsRestoreList4" })]
        public RestoreInitialSettings SettingsRestore
        {
            get => settingsRestore;
            set
            {
                if (value != settingsRestore)
                {
                    settingsRestore = value;
                    OnPropertyChanged("SettingsRestore");
                }
            }
        }

        [TaskPane("SettingsInputhandlerCaption", "SettingsInputhandlerTooltip", "Workspace", 3, false, ControlType.ComboBox, new string[] { "SettingsInputhandlerList1", "SettingsInputhandlerList2", "SettingsInputhandlerList3" })]
        public FialkaEnums.handleInvalidInput InputHandler
        {
            get => handleInput;
            set
            {
                if (value != handleInput)
                {
                    handleInput = value;
                    OnPropertyChanged("InputHandler");
                }
            }
        }


        #endregion

        #region Machine settings

        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane("VisibilityCaption", "VisibilityTooltip", "MachineSettings", 5, false, ControlType.CheckBox, "", null)]
        public bool MachineGroupVisibility
        {
            get => machineGroupHide;
            set
            {
                if (value != machineGroupHide)
                {
                    machineGroupHide = value;
                    OnPropertyChanged("MachineGroupVisibility");
                    setSettingsVisibilityMachineGroup();
                }
            }
        }

        [TaskPane("SelectMachineModelCaption", "SelectMachineModelTooltip", "MachineSettings", 6, false, ControlType.ComboBox, new string[] { "M125", "M125-3" })]
        public FialkaEnums.machineModel MachineModel
        {
            get => machineModel;
            set
            {
                if (value != machineModel)
                {
                    machineModel = value;
                    OnPropertyChanged("MachineModel");
                    setSettingsVisibilityMachineGroup();
                }
            }
        }

        [TaskPane("SelectMachineOperationModeCaption", "SelectMachineOperationModeTooltip", "MachineSettings", 7, false, ControlType.ComboBox, new string[] { "OperationModeList1", "OperationModeList2", "OperationModeList3" })]
        public FialkaEnums.operationMode OperationMode
        {
            get => operationMode;
            set
            {
                if (value != operationMode)
                {
                    operationMode = value;
                    OnPropertyChanged("OperationMode");
                }
            }
        }

        [TaskPane("SelectRotorTypesCaption", "SelectRotorTypesTooltip", "MachineSettings", 8, false, ControlType.ComboBox, new string[] { "PROTON I", "PROTON II" })]
        public FialkaEnums.rotorTypes RotorType
        {
            get => rotorType;
            set
            {
                if (value != rotorType)
                {
                    rotorType = value;
                    OnPropertyChanged("RotorType");
                    setSettingsVisibilityRotorCoreOrderGroup();
                    setSettingsVisibilityRotorCoreSidesGroup();
                    setSettingsVisibilityRotorCoreOffsetGroup();
                }
                for (int i = 1; i <= FialkaConstants.numberOfRotors; i++)
                {
                    // auto change in internal state - refresh the GUI
                    OnPropertyChanged("RotorCore" + i);
                    OnPropertyChanged("Rotor" + i + "CoreSide");
                    OnPropertyChanged("Rotor" + i + "CoreOffset");
                }
            }
        }

        [TaskPane("SelectMachineCountryLayoutCaption", "SelectMachineCountryLayoutTooltip", "MachineSettings", 9, false, ControlType.ComboBox, new string[] { "CountryLayoutList1", "CountryLayoutList2", "CountryLayoutList3" })]
        public FialkaEnums.countryLayout CountryLayout
        {
            get => countryLayout;
            set
            {
                if (value != countryLayout)
                {
                    countryLayout = value;
                    OnPropertyChanged("MachineModel");
                }
            }
        }


        [TaskPane("SelectMachinePrintHeadMappingCaption", "SelectMachinePrintHeadMappingTooltip", "MachineSettings", 11, false, ControlType.ComboBox, new string[] { "PrintHeadMappingList1", "PrintHeadMappingList2" })]
        public FialkaEnums.printHeadMapping PrintHeadMapping
        {
            get => printHeadMapping;
            set
            {
                if (value != printHeadMapping)
                {
                    printHeadMapping = value;
                    OnPropertyChanged("PrintHeadMapping");
                }
            }
        }

        [TaskPane("SelectRotorSeriesCaption", "SelectRotorSeriesTooltip", "MachineSettings", 12, false, ControlType.ComboBox, new string[] { "3K", "5K", "6K" })]
        public FialkaEnums.rotorSeries RotorSeries
        {
            get => rotorSeries;
            set
            {
                if (value != rotorSeries)
                {
                    rotorSeries = value;
                    OnPropertyChanged("RotorSeries");
                }
            }
        }


        [TaskPane("SelectMachineNumLockTypeCaption", "SelectMachineNumLockTypeTooltip", "MachineSettings", 13, false, ControlType.ComboBox, new string[] { "NumLock 10", "NumLock 30" })]
        public FialkaEnums.numLockType NumLockType
        {
            get => numlockType;
            set
            {
                if (value != numlockType)
                {
                    numlockType = value;
                    OnPropertyChanged("NumLockType");
                    // also changed in the FialkaInternalState
                    if (value == FialkaEnums.numLockType.NumLock10)
                    {
                        OnPropertyChanged("TextOperationMode");
                        OnPropertyChanged("PrintHeadMapping");
                    }
                }
            }
        }

        [TaskPane("SelectMachineTxtOpModeCaption", "SelectMachineTxtOpModeTooltip", "MachineSettings", 14, false, ControlType.ComboBox, new string[] { "TextOperationModeList1", "TextOperationModeList2", "TextOperationModeList3" })]
        public FialkaEnums.textOperationmMode TextOperationMode
        {
            get => txtOpmode;
            set
            {
                if (value != txtOpmode)
                {
                    txtOpmode = value;
                    OnPropertyChanged("TextOperationMode");
                }
            }
        }
        #endregion

        #region Rotor order selection
        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane("VisibilityCaption", "VisibilityTooltip", "RotorOrderSelection", 15, false, ControlType.CheckBox, "", null)]
        public bool RotorOrderGroupVisibility
        {
            get => rotorOrderGroupHide;
            set
            {
                if (value != rotorOrderGroupHide)
                {
                    rotorOrderGroupHide = value;
                    OnPropertyChanged("RotorOrderGroupVisibility");
                    setSettingsVisibilityRotorOrderGroup();
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "1")]
        [TaskPane("Rotor1Caption", "SelectRotor1OrderTooltip", "RotorOrderSelection", 16, false, ControlType.ComboBox,
            new string[] { "А", "Б", "В", "Г", "Д", "Е", "Ж", "З", "И", "К" })]
        public int Rotor1
        {
            get => RotorOrder[0];
            set
            {
                if (value != RotorOrder[0])
                {
                    RotorSwaps(0, RotorOrder[0], value);
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("Rotor2Caption", "SelectRotor2OrderTooltip", "RotorOrderSelection", 17, false, ControlType.ComboBox,
            new string[] { "А", "Б", "В", "Г", "Д", "Е", "Ж", "З", "И", "К" })]
        public int Rotor2
        {
            get => RotorOrder[1];
            set
            {
                if (value != RotorOrder[1])
                {
                    RotorSwaps(1, RotorOrder[1], value);
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("Rotor3Caption", "SelectRotor3OrderTooltip", "RotorOrderSelection", 18, false, ControlType.ComboBox,
            new string[] { "А", "Б", "В", "Г", "Д", "Е", "Ж", "З", "И", "К" })]
        public int Rotor3
        {
            get => RotorOrder[2];
            set
            {
                if (value != RotorOrder[2])
                {
                    RotorSwaps(2, RotorOrder[2], value);

                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "4")]
        [TaskPane("Rotor4Caption", "SelectRotor4OrderTooltip", "RotorOrderSelection", 19, false, ControlType.ComboBox,
            new string[] { "А", "Б", "В", "Г", "Д", "Е", "Ж", "З", "И", "К" })]
        public int Rotor4
        {
            get => RotorOrder[3];
            set
            {
                if (value != RotorOrder[3])
                {
                    RotorSwaps(3, RotorOrder[3], value);

                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "5")]
        [TaskPane("Rotor5Caption", "SelectRotor5OrderTooltip", "RotorOrderSelection", 20, false, ControlType.ComboBox,
            new string[] { "А", "Б", "В", "Г", "Д", "Е", "Ж", "З", "И", "К" })]
        public int Rotor5
        {
            get => RotorOrder[4];
            set
            {
                if (value != RotorOrder[4])
                {
                    RotorSwaps(4, RotorOrder[4], value);

                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "1")]
        [TaskPane("Rotor6Caption", "SelectRotor6OrderTooltip", "RotorOrderSelection", 21, false, ControlType.ComboBox,
            new string[] { "А", "Б", "В", "Г", "Д", "Е", "Ж", "З", "И", "К" })]
        public int Rotor6
        {
            get => RotorOrder[5];
            set
            {
                if (value != RotorOrder[5])
                {
                    RotorSwaps(5, RotorOrder[5], value);

                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("Rotor7Caption", "SelectRotor7OrderTooltip", "RotorOrderSelection", 22, false, ControlType.ComboBox,
            new string[] { "А", "Б", "В", "Г", "Д", "Е", "Ж", "З", "И", "К" })]
        public int Rotor7
        {
            get => RotorOrder[6];
            set
            {
                if (value != RotorOrder[6])
                {
                    RotorSwaps(6, RotorOrder[6], value);

                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("Rotor8Caption", "SelectRotor8OrderTooltip", "RotorOrderSelection", 23, false, ControlType.ComboBox,
            new string[] { "А", "Б", "В", "Г", "Д", "Е", "Ж", "З", "И", "К" })]
        public int Rotor8
        {
            get => RotorOrder[7];
            set
            {
                if (value != RotorOrder[7])
                {
                    RotorSwaps(7, RotorOrder[7], value);

                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "4")]
        [TaskPane("Rotor9Caption", "SelectRotor9OrderTooltip", "RotorOrderSelection", 24, false, ControlType.ComboBox,
            new string[] { "А", "Б", "В", "Г", "Д", "Е", "Ж", "З", "И", "К" })]
        public int Rotor9
        {
            get => RotorOrder[8];
            set
            {
                if (value != RotorOrder[8])
                {
                    RotorSwaps(8, RotorOrder[8], value);

                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "5")]
        [TaskPane("Rotor10Caption", "SelectRotor10OrderTooltip", "RotorOrderSelection", 25, false, ControlType.ComboBox,
            new string[] { "А", "Б", "В", "Г", "Д", "Е", "Ж", "З", "И", "К" })]
        public int Rotor10
        {
            get => RotorOrder[9];
            set
            {
                if (value != RotorOrder[9])
                {
                    RotorSwaps(9, RotorOrder[9], value);
                }
            }
        }

        [TaskPane("ResetRotorOrderCaption", "ResetRotorOrderTooltip", "RotorOrderSelection", 26, false, ControlType.Button)]
        public void ResetRotorOrder()
        {
            RotorOrder = FialkaConstants.baseRotorPositions();
            for (int i = 1; i <= FialkaConstants.numberOfRotors; i++)
            {
                OnPropertyChanged("Rotor" + i);
            }

            OnPropertyChanged("RotorOrder");
        }
        #endregion

        #region Rotor offset selection

        public void internalStateChanged()
        {
            for (int i = 1; i <= FialkaConstants.numberOfRotors; i++)
            {
                OnPropertyChanged("Rotor" + i + "Offset");
            }
            OnPropertyChanged("RotorOffsets");
        }

        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane("VisibilityCaption", "VisibilityTooltip", "RotorOffsetSelection", 27, false, ControlType.CheckBox, "", null)]
        public bool RotorOffsetGroupVisibility
        {
            get => rotorOffsetGroupHide;
            set
            {
                if (value != rotorOffsetGroupHide)
                {
                    rotorOffsetGroupHide = value;
                    OnPropertyChanged("RotorOffsetGroupVisibility");
                    setSettingsVisibilityRotorOffsetGroup();
                }
            }
        }


        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "1")]
        [TaskPane("Rotor1Caption", "SelectRotor1OffsetTooltip", "RotorOffsetSelection", 28, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int Rotor1Offset
        {
            get => RotorOffsets[0];
            set
            {
                if (value != RotorOffsets[0])
                {
                    RotorOffsets[0] = value;
                    OnPropertyChanged("Rotor1Offset");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("Rotor2Caption", "SelectRotor2OffsetTooltip", "RotorOffsetSelection", 29, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int Rotor2Offset
        {
            get => RotorOffsets[1];
            set
            {
                if (value != RotorOffsets[1])
                {
                    RotorOffsets[1] = value;
                    OnPropertyChanged("Rotor2Offset");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("Rotor3Caption", "SelectRotor3OffsetTooltip", "RotorOffsetSelection", 30, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int Rotor3Offset
        {
            get => RotorOffsets[2];
            set
            {
                if (value != RotorOffsets[2])
                {
                    RotorOffsets[2] = value;
                    OnPropertyChanged("Rotor3Offset");

                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "4")]
        [TaskPane("Rotor4Caption", "SelectRotor4OffsetTooltip", "RotorOffsetSelection", 31, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int Rotor4Offset
        {
            get => RotorOffsets[3];
            set
            {
                if (value != RotorOffsets[3])
                {
                    RotorOffsets[3] = value;
                    OnPropertyChanged("Rotor4Offset");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "5")]
        [TaskPane("Rotor5Caption", "SelectRotor5OffsetTooltip", "RotorOffsetSelection", 32, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int Rotor5Offset
        {
            get => RotorOffsets[4];
            set
            {
                if (value != RotorOffsets[4])
                {
                    RotorOffsets[4] = value;
                    OnPropertyChanged("Rotor5Offset");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "1")]
        [TaskPane("Rotor6Caption", "SelectRotor6OffsetTooltip", "RotorOffsetSelection", 33, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int Rotor6Offset
        {
            get => RotorOffsets[5];
            set
            {
                if (value != RotorOffsets[5])
                {
                    RotorOffsets[5] = value;
                    OnPropertyChanged("Rotor6Offset");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("Rotor7Caption", "SelectRotor7OffsetTooltip", "RotorOffsetSelection", 34, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int Rotor7Offset
        {
            get => RotorOffsets[6];
            set
            {
                if (value != RotorOffsets[6])
                {
                    RotorOffsets[6] = value;
                    OnPropertyChanged("Rotor7Offset");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("Rotor8Caption", "SelectRotor8OffsetTooltip", "RotorOffsetSelection", 35, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int Rotor8Offset
        {
            get => RotorOffsets[7];
            set
            {
                if (value != RotorOffsets[7])
                {
                    RotorOffsets[7] = value;
                    OnPropertyChanged("Rotor8Offset");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "4")]
        [TaskPane("Rotor9Caption", "SelectRotor9OffsetTooltip", "RotorOffsetSelection", 36, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int Rotor9Offset
        {
            get => RotorOffsets[8];
            set
            {
                if (value != RotorOffsets[8])
                {
                    RotorOffsets[8] = value;
                    OnPropertyChanged("Rotor9Offset");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "5")]
        [TaskPane("Rotor10Caption", "SelectRotor10OffsetTooltip", "RotorOffsetSelection", 37, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int Rotor10Offset
        {
            get => RotorOffsets[9];
            set
            {
                if (value != RotorOffsets[9])
                {
                    RotorOffsets[9] = value;
                    OnPropertyChanged("Rotor10Offset");
                }
            }
        }

        [TaskPane("ResetRotorOffsetNullCaption", "ResetRotorOffsetNullTooltip", "RotorOffsetSelection", 38, false, ControlType.Button)]
        public void ResetRotorOffsetNull()
        {
            RotorOffsets = FialkaConstants.nullOffset();
            for (int i = 1; i <= FialkaConstants.numberOfRotors; i++)
            {
                OnPropertyChanged("Rotor" + i + "Offset");
            }

            OnPropertyChanged("RotorOffsets");
        }

        [TaskPane("ResetRotorOffsetBaseCaption", "ResetRotorOffsetBaseTooltip", "RotorOffsetSelection", 39, false, ControlType.Button)]
        public void ResetRotorOffsetBase()
        {
            RotorOffsets = FialkaConstants.baseRotorPositions();
            for (int i = 1; i <= FialkaConstants.numberOfRotors; i++)
            {
                OnPropertyChanged("Rotor" + i + "Offset");
            }

            OnPropertyChanged("RotorOffsets");
        }

        #endregion

        #region Ring offset selection

        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane("VisibilityCaption", "VisibilityTooltip", "RotorRingOffsetSelection", 40, false, ControlType.CheckBox, "", null)]
        public bool RingOffsetGroupVisibility
        {
            get => ringOffsetGroupHide;
            set
            {
                if (value != ringOffsetGroupHide)
                {
                    ringOffsetGroupHide = value;
                    OnPropertyChanged("RingOffsetGroupVisibility");
                    setSettingsVisibilityRingOffsetGroup();
                }
            }
        }


        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "1")]
        [TaskPane("Rotor1Caption", "RotorRing1OffsetTooltip", "RotorRingOffsetSelection", 41, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int RotorRing1Offset
        {
            get => RotorRingOffsets[0];
            set
            {
                if (value != RotorRingOffsets[0])
                {
                    RotorRingOffsets[0] = value;
                    OnPropertyChanged("RotorRing1Offset");
                }
            }
        }


        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("Rotor2Caption", "RotorRing2OffsetTooltip", "RotorRingOffsetSelection", 42, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int RotorRing2Offset
        {
            get => RotorRingOffsets[1];
            set
            {
                if (value != RotorRingOffsets[1])
                {
                    RotorRingOffsets[1] = value;
                    OnPropertyChanged("RotorRing2Offset");
                }
            }
        }


        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("Rotor3Caption", "RotorRing3OffsetTooltip", "RotorRingOffsetSelection", 43, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int RotorRing3Offset
        {
            get => RotorRingOffsets[2];
            set
            {
                if (value != RotorRingOffsets[2])
                {
                    RotorRingOffsets[2] = value;
                    OnPropertyChanged("RotorRing3Offset");
                }
            }
        }


        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "4")]
        [TaskPane("Rotor4Caption", "RotorRing4OffsetTooltip", "RotorRingOffsetSelection", 44, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int RotorRing4Offset
        {
            get => RotorRingOffsets[3];
            set
            {
                if (value != RotorRingOffsets[3])
                {
                    RotorRingOffsets[3] = value;
                    OnPropertyChanged("RotorRing4Offset");
                }
            }
        }


        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "5")]
        [TaskPane("Rotor5Caption", "RotorRing5OffsetTooltip", "RotorRingOffsetSelection", 45, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int RotorRing5Offset
        {
            get => RotorRingOffsets[4];
            set
            {
                if (value != RotorRingOffsets[4])
                {
                    RotorRingOffsets[4] = value;
                    OnPropertyChanged("RotorRing5Offset");
                }
            }
        }




        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "1")]
        [TaskPane("Rotor6Caption", "RotorRing6OffsetTooltip", "RotorRingOffsetSelection", 46, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int RotorRing6Offset
        {
            get => RotorRingOffsets[5];
            set
            {
                if (value != RotorRingOffsets[5])
                {
                    RotorRingOffsets[5] = value;
                    OnPropertyChanged("RotorRing6Offset");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("Rotor7Caption", "RotorRing7OffsetTooltip", "RotorRingOffsetSelection", 47, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int RotorRing7Offset
        {
            get => RotorRingOffsets[6];
            set
            {
                if (value != RotorRingOffsets[6])
                {
                    RotorRingOffsets[6] = value;
                    OnPropertyChanged("RotorRing7Offset");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("Rotor8Caption", "RotorRing8OffsetTooltip", "RotorRingOffsetSelection", 48, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int RotorRing8Offset
        {
            get => RotorRingOffsets[7];
            set
            {
                if (value != RotorRingOffsets[7])
                {
                    RotorRingOffsets[7] = value;
                    OnPropertyChanged("RotorRing8Offset");
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "4")]
        [TaskPane("Rotor9Caption", "RotorRing9OffsetTooltip", "RotorRingOffsetSelection", 49, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int RotorRing9Offset
        {
            get => RotorRingOffsets[8];
            set
            {
                if (value != RotorRingOffsets[8])
                {
                    RotorRingOffsets[8] = value;
                    OnPropertyChanged("RotorRing9Offset");
                }
            }
        }


        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "5")]
        [TaskPane("Rotor10Caption", "RotorRing10OffsetTooltip", "RotorRingOffsetSelection", 50, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int RotorRing10Offset
        {
            get => RotorRingOffsets[9];
            set
            {
                if (value != RotorRingOffsets[9])
                {
                    RotorRingOffsets[9] = value;
                    OnPropertyChanged("RotorRing10Offset");
                }
            }
        }

        [TaskPane("ResetRotorOffsetNullCaption", "ResetRotorOffsetNullTooltip", "RotorRingOffsetSelection", 51, false, ControlType.Button)]
        public void ResetRingOffset()
        {
            RotorRingOffsets = FialkaConstants.nullOffset();
            for (int i = 1; i <= FialkaConstants.numberOfRotors; i++)
            {
                OnPropertyChanged("RotorRing" + i + "Offset");
            }

            OnPropertyChanged("RotorRingOffsets");
        }

        #endregion

        #region Rotor Core Order

        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane("VisibilityCaption", "VisibilityTooltip", "RotorCoreSelection", 52, false, ControlType.CheckBox, "", null)]
        public bool RotorCoreOrderGroupVisibility
        {
            get => rotorCoreOrderGroupHide;
            set
            {
                if (value != rotorCoreOrderGroupHide)
                {
                    rotorCoreOrderGroupHide = value;
                    OnPropertyChanged("RotorCoreOrderGroupVisibility");
                    setSettingsVisibilityRotorCoreOrderGroup();
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "1")]
        [TaskPane("Rotor1Caption", "SelectRotorCore1Tooltip", "RotorCoreSelection", 53, false, ControlType.ComboBox,
            new string[] { "А", "Б", "В", "Г", "Д", "Е", "Ж", "З", "И", "К" })]
        public int RotorCore1
        {
            get => RotorCoreOrders[0];
            set
            {
                if (value != RotorCoreOrders[0])
                {
                    RotorCoreSwaps(0, RotorCoreOrders[0], value);
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("Rotor2Caption", "SelectRotorCore2Tooltip", "RotorCoreSelection", 54, false, ControlType.ComboBox,
            new string[] { "А", "Б", "В", "Г", "Д", "Е", "Ж", "З", "И", "К" })]
        public int RotorCore2
        {
            get => RotorCoreOrders[1];
            set
            {
                if (value != RotorCoreOrders[1])
                {
                    RotorCoreSwaps(1, RotorCoreOrders[1], value);
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("Rotor3Caption", "SelectRotorCore3Tooltip", "RotorCoreSelection", 55, false, ControlType.ComboBox,
            new string[] { "А", "Б", "В", "Г", "Д", "Е", "Ж", "З", "И", "К" })]
        public int RotorCore3
        {
            get => RotorCoreOrders[2];
            set
            {
                if (value != RotorCoreOrders[2])
                {
                    RotorCoreSwaps(2, RotorCoreOrders[2], value);
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "4")]
        [TaskPane("Rotor4Caption", "SelectRotorCore4Tooltip", "RotorCoreSelection", 56, false, ControlType.ComboBox,
            new string[] { "А", "Б", "В", "Г", "Д", "Е", "Ж", "З", "И", "К" })]
        public int RotorCore4
        {
            get => RotorCoreOrders[3];
            set
            {
                if (value != RotorCoreOrders[3])
                {
                    RotorCoreSwaps(3, RotorCoreOrders[3], value);
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "5")]
        [TaskPane("Rotor5Caption", "SelectRotorCore5Tooltip", "RotorCoreSelection", 57, false, ControlType.ComboBox,
            new string[] { "А", "Б", "В", "Г", "Д", "Е", "Ж", "З", "И", "К" })]
        public int RotorCore5
        {
            get => RotorCoreOrders[4];
            set
            {
                if (value != RotorCoreOrders[4])
                {
                    RotorCoreSwaps(4, RotorCoreOrders[4], value);
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "1")]
        [TaskPane("Rotor6Caption", "SelectRotorCore6Tooltip", "RotorCoreSelection", 58, false, ControlType.ComboBox,
            new string[] { "А", "Б", "В", "Г", "Д", "Е", "Ж", "З", "И", "К" })]
        public int RotorCore6
        {
            get => RotorCoreOrders[5];
            set
            {
                if (value != RotorCoreOrders[5])
                {
                    RotorCoreSwaps(5, RotorCoreOrders[5], value);
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("Rotor7Caption", "SelectRotorCore7Tooltip", "RotorCoreSelection", 59, false, ControlType.ComboBox,
            new string[] { "А", "Б", "В", "Г", "Д", "Е", "Ж", "З", "И", "К" })]
        public int RotorCore7
        {
            get => RotorCoreOrders[6];
            set
            {
                if (value != RotorCoreOrders[6])
                {
                    RotorCoreSwaps(6, RotorCoreOrders[6], value);
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("Rotor8Caption", "SelectRotorCore8Tooltip", "RotorCoreSelection", 60, false, ControlType.ComboBox,
            new string[] { "А", "Б", "В", "Г", "Д", "Е", "Ж", "З", "И", "К" })]
        public int RotorCore8
        {
            get => RotorCoreOrders[7];
            set
            {
                if (value != RotorCoreOrders[7])
                {
                    RotorCoreSwaps(7, RotorCoreOrders[7], value);
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "4")]
        [TaskPane("Rotor9Caption", "SelectRotorCore9Tooltip", "RotorCoreSelection", 61, false, ControlType.ComboBox,
            new string[] { "А", "Б", "В", "Г", "Д", "Е", "Ж", "З", "И", "К" })]
        public int RotorCore9
        {
            get => RotorCoreOrders[8];
            set
            {
                if (value != RotorCoreOrders[8])
                {
                    RotorCoreSwaps(8, RotorCoreOrders[8], value);
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "5")]
        [TaskPane("Rotor10Caption", "SelectRotorCore10Tooltip", "RotorCoreSelection", 62, false, ControlType.ComboBox,
            new string[] { "А", "Б", "В", "Г", "Д", "Е", "Ж", "З", "И", "К" })]
        public int RotorCore10
        {
            get => RotorCoreOrders[9];
            set
            {
                if (value != RotorCoreOrders[9])
                {
                    RotorCoreSwaps(9, RotorCoreOrders[9], value);
                }
            }
        }

        [TaskPane("ResetRotorCoreOrderCaption", "ResetRotorCoreOrderTooltip", "RotorCoreSelection", 63, false, ControlType.Button)]
        public void ResetRotorCoreOrder()
        {
            RotorCoreOrders = FialkaConstants.baseRotorPositions();
            for (int i = 1; i <= FialkaConstants.numberOfRotors; i++)
            {
                OnPropertyChanged("RotorCore" + i);
            }

            OnPropertyChanged("RotorCoreOrders");
        }
        #endregion

        #region Rotor Core Side
        /// <summary>
        /// Helper method. Conersion from 0/1 (UI settings) into 1/-1 (internal state). In InternalState 1 represents normal side (\pi) and -1 represents flipped size (\pi_{-1}).
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private int setInternalStateCoreOrientation(int input)
        {
            return input == 1 ? -1 : 1;
        }
        /// <summary>
        /// Helper method. Conersion from 1/-1 (internal state) into 0/1 (UI settings). In InternalState 1 represents normal side (\pi) and -1 represents flipped size (\pi^{-1}).
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private int getInternalStateCoreOrientation(int input)
        {
            return input == -1 ? 1 : 0;
        }

        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane("VisibilityCaption", "VisibilityTooltip", "RotorCoreSideSelection", 64, false, ControlType.CheckBox, "", null)]
        public bool RotorCoreSidesGroupVisibility
        {
            get => rotorCoreSidesGroupHide;
            set
            {
                if (value != rotorCoreSidesGroupHide)
                {
                    rotorCoreSidesGroupHide = value;
                    OnPropertyChanged("RotorCoreSidesGroupVisibility");
                    setSettingsVisibilityRotorCoreSidesGroup();
                }
            }
        }


        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "1")]
        [TaskPane("Rotor1Caption", "SelectRotor1CoreSideTooltip", "RotorCoreSideSelection", 65, false, ControlType.ComboBox, new string[] { "1", "2" })]
        public int Rotor1CoreSide
        {
            get => getInternalStateCoreOrientation(RotorCoreOrientations[0]);
            set
            {
                if (value != getInternalStateCoreOrientation(RotorCoreOrientations[0]))
                {
                    RotorCoreOrientations[0] = setInternalStateCoreOrientation(value);
                    OnPropertyChanged("Rotor1CoreSide");
                }
            }
        }


        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("Rotor2Caption", "SelectRotor2CoreSideTooltip", "RotorCoreSideSelection", 66, false, ControlType.ComboBox, new string[] { "1", "2" })]
        public int Rotor2CoreSide
        {
            get => getInternalStateCoreOrientation(RotorCoreOrientations[1]);
            set
            {
                if (value != getInternalStateCoreOrientation(RotorCoreOrientations[1]))
                {
                    RotorCoreOrientations[1] = setInternalStateCoreOrientation(value);
                    OnPropertyChanged("Rotor2CoreSide");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("Rotor3Caption", "SelectRotor3CoreSideTooltip", "RotorCoreSideSelection", 67, false, ControlType.ComboBox, new string[] { "1", "2" })]
        public int Rotor3CoreSide
        {
            get => getInternalStateCoreOrientation(RotorCoreOrientations[2]);
            set
            {
                if (value != getInternalStateCoreOrientation(RotorCoreOrientations[2]))
                {
                    RotorCoreOrientations[2] = setInternalStateCoreOrientation(value); ;
                    OnPropertyChanged("Rotor3CoreSide");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "4")]
        [TaskPane("Rotor4Caption", "SelectRotor4CoreSideTooltip", "RotorCoreSideSelection", 68, false, ControlType.ComboBox, new string[] { "1", "2" })]
        public int Rotor4CoreSide
        {
            get => getInternalStateCoreOrientation(RotorCoreOrientations[3]);
            set
            {
                if (value != getInternalStateCoreOrientation(RotorCoreOrientations[3]))
                {
                    RotorCoreOrientations[3] = setInternalStateCoreOrientation(value); ;
                    OnPropertyChanged("Rotor4CoreSide");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "5")]
        [TaskPane("Rotor5Caption", "SelectRotor5CoreSideTooltip", "RotorCoreSideSelection", 69, false, ControlType.ComboBox, new string[] { "1", "2" })]
        public int Rotor5CoreSide
        {
            get => getInternalStateCoreOrientation(RotorCoreOrientations[4]);
            set
            {
                if (value != getInternalStateCoreOrientation(RotorCoreOrientations[4]))
                {
                    RotorCoreOrientations[4] = setInternalStateCoreOrientation(value); ;
                    OnPropertyChanged("Rotor5CoreSide");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "1")]
        [TaskPane("Rotor6Caption", "SelectRotor6CoreSideTooltip", "RotorCoreSideSelection", 70, false, ControlType.ComboBox, new string[] { "1", "2" })]
        public int Rotor6CoreSide
        {
            get => getInternalStateCoreOrientation(RotorCoreOrientations[5]);
            set
            {
                if (value != getInternalStateCoreOrientation(RotorCoreOrientations[5]))
                {
                    RotorCoreOrientations[5] = setInternalStateCoreOrientation(value); ;
                    OnPropertyChanged("Rotor6CoreSide");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("Rotor7Caption", "SelectRotor7CoreSideTooltip", "RotorCoreSideSelection", 71, false, ControlType.ComboBox, new string[] { "1", "2" })]
        public int Rotor7CoreSide
        {
            get => getInternalStateCoreOrientation(RotorCoreOrientations[6]);
            set
            {
                if (value != getInternalStateCoreOrientation(RotorCoreOrientations[6]))
                {
                    RotorCoreOrientations[6] = setInternalStateCoreOrientation(value); ;
                    OnPropertyChanged("Rotor7CoreSide");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("Rotor8Caption", "SelectRotor8CoreSideTooltip", "RotorCoreSideSelection", 72, false, ControlType.ComboBox, new string[] { "1", "2" })]
        public int Rotor8CoreSide
        {
            get => getInternalStateCoreOrientation(RotorCoreOrientations[7]);
            set
            {
                if (value != getInternalStateCoreOrientation(RotorCoreOrientations[7]))
                {
                    RotorCoreOrientations[7] = setInternalStateCoreOrientation(value); ;
                    OnPropertyChanged("Rotor8CoreSide");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "4")]
        [TaskPane("Rotor9Caption", "SelectRotor9CoreSideTooltip", "RotorCoreSideSelection", 73, false, ControlType.ComboBox, new string[] { "1", "2" })]
        public int Rotor9CoreSide
        {
            get => getInternalStateCoreOrientation(RotorCoreOrientations[8]);
            set
            {
                if (value != getInternalStateCoreOrientation(RotorCoreOrientations[8]))
                {
                    RotorCoreOrientations[8] = setInternalStateCoreOrientation(value); ;
                    OnPropertyChanged("Rotor9CoreSide");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "5")]
        [TaskPane("Rotor10Caption", "SelectRotor10CoreSideTooltip", "RotorCoreSideSelection", 74, false, ControlType.ComboBox, new string[] { "1", "2" })]
        public int Rotor10CoreSide
        {
            get => getInternalStateCoreOrientation(RotorCoreOrientations[9]);
            set
            {
                if (value != getInternalStateCoreOrientation(RotorCoreOrientations[9]))
                {
                    RotorCoreOrientations[9] = setInternalStateCoreOrientation(value);
                    OnPropertyChanged("Rotor10CoreSide");
                }
            }
        }

        [TaskPane("ResetRotorCoreSidesCaption", "ResetRotorCoreSidesTooltip", "RotorCoreSideSelection", 75, false, ControlType.Button)]
        public void ResetRotorCoreSides()
        {
            RotorCoreOrientations = FialkaConstants.deafultCoreOrientation();
            for (int i = 1; i <= FialkaConstants.numberOfRotors; i++)
            {
                OnPropertyChanged("Rotor" + i + "CoreSide");
            }

            OnPropertyChanged("RotorCoreOrientations");
        }
        #endregion

        #region Rotor Core Offset Selection

        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane("VisibilityCaption", "VisibilityTooltip", "RotorCoreOffsetSelection", 76, false, ControlType.CheckBox, "", null)]
        public bool RotorCoreOffsetGroupVisibility
        {
            get => rotorCoreOffsetGroupHide;
            set
            {
                if (value != rotorCoreOffsetGroupHide)
                {
                    rotorCoreOffsetGroupHide = value;
                    OnPropertyChanged("RotorCoreOffsetGroupVisibility");
                    setSettingsVisibilityRotorCoreOffsetGroup();
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "1")]
        [TaskPane("Rotor1Caption", "SelectRotor1CoreOffsetTooltip", "RotorCoreOffsetSelection", 77, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int Rotor1CoreOffset
        {
            get => RotorCoreOffsets[0];
            set
            {
                if (value != RotorCoreOffsets[0])
                {
                    RotorCoreOffsets[0] = value;
                    OnPropertyChanged("Rotor1CoreOffset");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("Rotor2Caption", "SelectRotor2CoreOffsetTooltip", "RotorCoreOffsetSelection", 78, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int Rotor2CoreOffset
        {
            get => RotorCoreOffsets[1];
            set
            {
                if (value != RotorCoreOffsets[1])
                {
                    RotorCoreOffsets[1] = value;
                    OnPropertyChanged("Rotor2CoreOffset");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("Rotor3Caption", "SelectRotor3CoreOffsetTooltip", "RotorCoreOffsetSelection", 79, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int Rotor3CoreOffset
        {
            get => RotorCoreOffsets[2];
            set
            {
                if (value != RotorCoreOffsets[2])
                {
                    RotorCoreOffsets[2] = value;
                    OnPropertyChanged("Rotor3CoreOffset");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "4")]
        [TaskPane("Rotor4Caption", "SelectRotor4CoreOffsetTooltip", "RotorCoreOffsetSelection", 80, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int Rotor4CoreOffset
        {
            get => RotorCoreOffsets[3];
            set
            {
                if (value != RotorCoreOffsets[3])
                {
                    RotorCoreOffsets[3] = value;
                    OnPropertyChanged("Rotor4CoreOffset");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "5")]
        [TaskPane("Rotor5Caption", "SelectRotor5CoreOffsetTooltip", "RotorCoreOffsetSelection", 81, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int Rotor5CoreOffset
        {
            get => RotorCoreOffsets[4];
            set
            {
                if (value != RotorCoreOffsets[4])
                {
                    RotorCoreOffsets[4] = value;
                    OnPropertyChanged("Rotor5CoreOffset");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "1")]
        [TaskPane("Rotor6Caption", "SelectRotor6CoreOffsetTooltip", "RotorCoreOffsetSelection", 82, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int Rotor6CoreOffset
        {
            get => RotorCoreOffsets[5];
            set
            {
                if (value != RotorCoreOffsets[5])
                {
                    RotorCoreOffsets[5] = value;
                    OnPropertyChanged("Rotor6CoreOffset");
                    RotorCoreOffsets[5] = value;

                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("Rotor7Caption", "SelectRotor7CoreOffsetTooltip", "RotorCoreOffsetSelection", 83, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int Rotor7CoreOffset
        {
            get => RotorCoreOffsets[6];
            set
            {
                if (value != RotorCoreOffsets[6])
                {
                    RotorCoreOffsets[6] = value;
                    OnPropertyChanged("Rotor7CoreOffset");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("Rotor8Caption", "SelectRotor8CoreOffsetTooltip", "RotorCoreOffsetSelection", 84, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int Rotor8CoreOffset
        {
            get => RotorCoreOffsets[7];
            set
            {
                if (value != RotorCoreOffsets[7])
                {
                    RotorCoreOffsets[7] = value;
                    OnPropertyChanged("Rotor8CoreOffset");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "4")]
        [TaskPane("Rotor9Caption", "SelectRotor9CoreOffsetTooltip", "RotorCoreOffsetSelection", 85, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int Rotor9CoreOffset
        {
            get => RotorCoreOffsets[8];
            set
            {
                if (value != RotorCoreOffsets[8])
                {
                    RotorCoreOffsets[8] = value;
                    OnPropertyChanged("Rotor9CoreOffset");
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "5")]
        [TaskPane("Rotor10Caption", "SelectRotor10CoreOffsetTooltip", "RotorCoreOffsetSelection", 86, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 29)]
        public int Rotor10CoreOffset
        {
            get => RotorCoreOffsets[9];
            set
            {
                if (value != RotorCoreOffsets[9])
                {
                    RotorCoreOffsets[9] = value;
                    OnPropertyChanged("Rotor10CoreOffset");
                    RotorCoreOffsets[9] = value;

                }
            }
        }

        [TaskPane("ResetRotorCoreOffsetCaption", "ResetRotorCoreOffsetTooltip", "RotorCoreOffsetSelection", 87, false, ControlType.Button)]
        public void ResetRotorCoreOffset()
        {
            RotorCoreOffsets = FialkaConstants.nullOffset();
            for (int i = 1; i <= FialkaConstants.numberOfRotors; i++)
            {
                OnPropertyChanged("Rotor" + i + "CoreOffset");
            }

            OnPropertyChanged("RotorCoreOffsets");
        }
        #endregion

        #region Punch Card

        public void swapOffsets(int characterNumber, int current, int next)
        {
            int characterNumber2 = Array.IndexOf(PunchCard, next);
            PunchCard[characterNumber] = next;
            PunchCard[characterNumber2] = current;
            OnPropertyChanged("PunchCard" + (characterNumber + 1));
            OnPropertyChanged("PunchCard" + (characterNumber2 + 1));
        }

        [SettingsFormat(1, "Normal", "Normal")]
        [TaskPane("VisibilityCaption", "VisibilityTooltip", "PunchCard", 88, false, ControlType.CheckBox, "", null)]
        public bool PunchCardGroupVisibility
        {
            get => punchCardGroupHide;
            set
            {
                if (value != punchCardGroupHide)
                {
                    punchCardGroupHide = value;
                    OnPropertyChanged("PunchCardGroupVisibility");
                    setSettingsVisibilityPunchCardGroup();
                }
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "1")]
        [TaskPane("PunchCard1Caption", "PunchCard1Tooltip", "PunchCard", 89, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard1
        {
            get => PunchCard[0];
            set
            {
                if (value != PunchCard[0])
                {
                    swapOffsets(0, PunchCard[0], value);
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "1")]
        [TaskPane("PunchCard2Caption", "PunchCard2Tooltip", "PunchCard", 90, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard2
        {
            get => PunchCard[1];
            set
            {
                if (value != PunchCard[1])
                {
                    swapOffsets(1, PunchCard[1], value);
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "1")]
        [TaskPane("PunchCard3Caption", "PunchCard3Tooltip", "PunchCard", 91, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard3
        {
            get => PunchCard[2];
            set
            {
                if (value != PunchCard[2])
                {
                    swapOffsets(2, PunchCard[2], value);
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("PunchCard4Caption", "PunchCard4Tooltip", "PunchCard", 92, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard4
        {
            get => PunchCard[3];
            set
            {
                if (value != PunchCard[3])
                {
                    swapOffsets(3, PunchCard[3], value);
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("PunchCard5Caption", "PunchCard5Tooltip", "PunchCard", 93, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard5
        {
            get => PunchCard[4];
            set
            {
                if (value != PunchCard[4])
                {
                    swapOffsets(4, PunchCard[4], value);
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "2")]
        [TaskPane("PunchCard6Caption", "PunchCard6Tooltip", "PunchCard", 94, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard6
        {
            get => PunchCard[5];
            set
            {
                if (value != PunchCard[5])
                {
                    swapOffsets(5, PunchCard[5], value);
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("PunchCard7Caption", "PunchCard7Tooltip", "PunchCard", 95, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard7
        {
            get => PunchCard[6];
            set
            {
                if (value != PunchCard[6])
                {
                    swapOffsets(6, PunchCard[6], value);
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("PunchCard8Caption", "PunchCard8Tooltip", "PunchCard", 96, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard8
        {
            get => PunchCard[7];
            set
            {
                if (value != PunchCard[7])
                {
                    swapOffsets(7, PunchCard[7], value);
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "3")]
        [TaskPane("PunchCard9Caption", "PunchCard9Tooltip", "PunchCard", 97, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard9
        {
            get => PunchCard[8];
            set
            {
                if (value != PunchCard[8])
                {
                    swapOffsets(8, PunchCard[8], value);
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "4")]
        [TaskPane("PunchCard10Caption", "PunchCard10Tooltip", "PunchCard", 98, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard10
        {
            get => PunchCard[9];
            set
            {
                if (value != PunchCard[9])
                {
                    swapOffsets(9, PunchCard[9], value);
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "4")]
        [TaskPane("PunchCard11Caption", "PunchCard11Tooltip", "PunchCard", 99, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard11
        {
            get => PunchCard[10];
            set
            {
                if (value != PunchCard[10])
                {
                    swapOffsets(10, PunchCard[10], value);
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "4")]
        [TaskPane("PunchCard12Caption", "PunchCard12Tooltip", "PunchCard", 100, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard12
        {
            get => PunchCard[11];
            set
            {
                if (value != PunchCard[11])
                {
                    swapOffsets(11, PunchCard[11], value);
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "5")]
        [TaskPane("PunchCard13Caption", "PunchCard13Tooltip", "PunchCard", 101, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard13
        {
            get => PunchCard[12];
            set
            {
                if (value != PunchCard[12])
                {
                    swapOffsets(12, PunchCard[12], value);
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "5")]
        [TaskPane("PunchCard14Caption", "PunchCard14Tooltip", "PunchCard", 102, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard14
        {
            get => PunchCard[13];
            set
            {
                if (value != PunchCard[13])
                {
                    swapOffsets(13, PunchCard[13], value);
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "5")]
        [TaskPane("PunchCard15Caption", "PunchCard15Tooltip", "PunchCard", 103, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard15
        {
            get => PunchCard[14];
            set
            {
                if (value != PunchCard[14])
                {
                    swapOffsets(14, PunchCard[14], value);
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "6")]
        [TaskPane("PunchCard16Caption", "PunchCard16Tooltip", "PunchCard", 104, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard16
        {
            get => PunchCard[15];
            set
            {
                if (value != PunchCard[15])
                {
                    swapOffsets(15, PunchCard[15], value);
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "6")]
        [TaskPane("PunchCard17Caption", "PunchCard17Tooltip", "PunchCard", 105, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard17
        {
            get => PunchCard[16];
            set
            {
                if (value != PunchCard[16])
                {
                    swapOffsets(16, PunchCard[16], value);
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "6")]
        [TaskPane("PunchCard18Caption", "PunchCard18Tooltip", "PunchCard", 106, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard18
        {
            get => PunchCard[17];
            set
            {
                if (value != PunchCard[17])
                {
                    swapOffsets(17, PunchCard[17], value);
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "7")]
        [TaskPane("PunchCard19Caption", "PunchCard19Tooltip", "PunchCard", 107, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard19
        {
            get => PunchCard[18];
            set
            {
                if (value != PunchCard[18])
                {
                    swapOffsets(18, PunchCard[18], value);
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "7")]
        [TaskPane("PunchCard20Caption", "PunchCard20Tooltip", "PunchCard", 108, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard20
        {
            get => PunchCard[19];
            set
            {
                if (value != PunchCard[19])
                {
                    swapOffsets(19, PunchCard[19], value);
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "7")]
        [TaskPane("PunchCard21Caption", "PunchCard21Tooltip", "PunchCard", 109, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard21
        {
            get => PunchCard[20];
            set
            {
                if (value != PunchCard[20])
                {
                    swapOffsets(20, PunchCard[20], value);
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "8")]
        [TaskPane("PunchCard22Caption", "PunchCard22Tooltip", "PunchCard", 110, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard22
        {
            get => PunchCard[21];
            set
            {
                if (value != PunchCard[21])
                {
                    swapOffsets(21, PunchCard[21], value);
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "8")]
        [TaskPane("PunchCard23Caption", "PunchCard23Tooltip", "PunchCard", 111, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard23
        {
            get => PunchCard[22];
            set
            {
                if (value != PunchCard[22])
                {
                    swapOffsets(22, PunchCard[22], value);
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "8")]
        [TaskPane("PunchCard24Caption", "PunchCard24Tooltip", "PunchCard", 112, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard24
        {
            get => PunchCard[23];
            set
            {
                if (value != PunchCard[23])
                {
                    swapOffsets(23, PunchCard[23], value);
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "9")]
        [TaskPane("PunchCard25Caption", "PunchCard25Tooltip", "PunchCard", 113, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard25
        {
            get => PunchCard[24];
            set
            {
                if (value != PunchCard[24])
                {
                    swapOffsets(24, PunchCard[24], value);
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "9")]
        [TaskPane("PunchCard26Caption", "PunchCard26Tooltip", "PunchCard", 114, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard26
        {
            get => PunchCard[25];
            set
            {
                if (value != PunchCard[25])
                {
                    swapOffsets(25, PunchCard[25], value);
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "9")]
        [TaskPane("PunchCard27Caption", "PunchCard27Tooltip", "PunchCard", 115, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard27
        {
            get => PunchCard[26];
            set
            {
                if (value != PunchCard[26])
                {
                    swapOffsets(26, PunchCard[26], value);
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "10")]
        [TaskPane("PunchCard28Caption", "PunchCard28Tooltip", "PunchCard", 116, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard28
        {
            get => PunchCard[27];
            set
            {
                if (value != PunchCard[27])
                {
                    swapOffsets(27, PunchCard[27], value);
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "10")]
        [TaskPane("PunchCard29Caption", "PunchCard29Tooltip", "PunchCard", 117, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard29
        {
            get => PunchCard[28];
            set
            {
                if (value != PunchCard[28])
                {
                    swapOffsets(28, PunchCard[28], value);
                }
            }
        }
        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "10")]
        [TaskPane("PunchCard30Caption", "PunchCard30Tooltip", "PunchCard", 118, false, ControlType.ComboBox, new string[]
{ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
 "21", "22", "23", "24", "25", "26", "27", "28", "29", "30"})]
        public int PunchCard30
        {
            get => PunchCard[29];
            set
            {
                if (value != PunchCard[29])
                {
                    swapOffsets(29, PunchCard[29], value);
                }
            }
        }

        [TaskPane("ResetPunchCardCaption", "ResetPunchCardTooltip", "PunchCard", 119, false, ControlType.Button)]
        public void ResetPunchCard()
        {
            PunchCard = FialkaConstants.punchCardIdentity();
            for (int i = 1; i <= FialkaConstants.alphabetSize; i++)
            {
                OnPropertyChanged("PunchCard" + i);
            }

            OnPropertyChanged("PunchCard");
        }
        #endregion

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion

        public void Initialize()
        {
            showProperties();
        }

    }
}