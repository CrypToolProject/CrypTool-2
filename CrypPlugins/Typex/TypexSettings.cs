/*
   Copyright 2023 Nils Kopal <kopal<AT>cryptool.org>

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
using CrypTool.Typex.TypexMachine;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace CrypTool.Typex
{
    public enum UnknownSymbolHandlingMode
    {
        Ignore = 0,
        Remove = 1,
        Replace = 2
    }

    public class TypexSettings : ISettings
    {
        private TypexMachineType _typexMachineType = TypexMachineType.CyberChef;
        private UnknownSymbolHandlingMode _unknownSymbolHandlingMode = UnknownSymbolHandlingMode.Ignore;

        // Dynamic rotor definitions
        private ObservableCollection<string> _stators1 = new ObservableCollection<string>();
        private ObservableCollection<string> _stators2 = new ObservableCollection<string>();
        private ObservableCollection<string> _rotors1 = new ObservableCollection<string>();
        private ObservableCollection<string> _rotors2 = new ObservableCollection<string>();
        private ObservableCollection<string> _rotors3 = new ObservableCollection<string>();
        private ObservableCollection<string> _alphabet = new ObservableCollection<string>();

        //Rotor start positions
        private string _rotorPositions = "AAAAA";

        //Ring positions of rotors
        private string _ringPositions = "AAAAA";

        //Stator rotor selections
        private int _stator1 = 4;
        private int _stator2 = 3;
        private int _rotor1 = 2;
        private int _rotor2 = 1;
        private int _rotor3 = 0;

        private bool _stator1IsReversed = false;
        private bool _stator2IsReversed = false;
        private bool _rotor1IsReversed = false;
        private bool _rotor2IsReversed = false;
        private bool _rotor3IsReversed = false;

        //Reflector selection
        private TypexReflector _typexReflector = TypexReflector.CyberChef;
        private string _customReflector = "XEUWBIKSFOGPYRJLZNHVCTDAMQ";

        //Plugboard definition
        private string _plugSettings = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        #region dynamic collections for stator and rotor selection

        public ObservableCollection<string> Stators1
        {
            get => _stators1;
            set
            {
                if (value != _stators1)
                {
                    _stators1 = value;
                    OnPropertyChanged(nameof(Stators1));
                }
            }
        }

        public ObservableCollection<string> Stators2
        {
            get => _stators2;
            set
            {
                if (value != _stators2)
                {
                    _stators2 = value;
                    OnPropertyChanged(nameof(Stators2));
                }
            }
        }

        public ObservableCollection<string> Rotors1
        {
            get => _rotors1;
            set
            {
                if (value != _rotors1)
                {
                    _rotors1 = value;
                    OnPropertyChanged(nameof(Rotors1));
                }
            }
        }

        public ObservableCollection<string> Rotors2
        {
            get => _rotors2;
            set
            {
                if (value != _rotors2)
                {
                    _rotors2 = value;
                    OnPropertyChanged(nameof(Rotors2));
                }
            }
        }

        public ObservableCollection<string> Rotors3
        {
            get => _rotors3;
            set
            {
                if (value != _rotors3)
                {
                    _rotors3 = value;
                    OnPropertyChanged(nameof(Rotors3));
                }
            }
        }

        public ObservableCollection<string> Alphabet
        {
            get => _alphabet;
            set
            {
                if (value != _alphabet)
                {
                    _alphabet = value;
                    OnPropertyChanged(nameof(Alphabet));
                }
            }
        }

        /// <summary>
        /// Updates the observableCollections of the rotors and stators
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void UpdateRotorAndStatorCollections()
        {
            _stators1.Clear();
            _stators2.Clear();
            _rotors1.Clear();
            _rotors2.Clear();
            _rotors3.Clear();
            string[] rotorsStators;

            switch (_typexMachineType)
            {
                case TypexMachineType.CyberChef:
                    rotorsStators = Typex_CyberChef.GenerateRotorNameArray();
                    break;
                case TypexMachineType.TestRotors:
                    rotorsStators = Typex_TestRotors.GenerateRotorNamesArray();
                    break;
                case TypexMachineType.EnigmaI:
                    rotorsStators = Typex_EnigmaI.GenerateRotorNamesArray();
                    break;
                case TypexMachineType.Y296:
                    rotorsStators = Typex_Y296.GenerateRotorNamesArray();
                    break;
                case TypexMachineType.SP02390:
                    rotorsStators = Typex_SP02390.GenerateRotorNamesArray();
                    break;
                case TypexMachineType.SP02391:
                    rotorsStators = Typex_SP02391.GenerateRotorNamesArray();
                    break;
                default:
                    throw new Exception(string.Format("Invalid Typex machine type: {0}", _typexMachineType));
            }

            foreach (string rotorStator in rotorsStators)
            {
                _stators1.Add(rotorStator);
                _stators2.Add(rotorStator);
                _rotors1.Add(rotorStator);
                _rotors2.Add(rotorStator);
                _rotors3.Add(rotorStator);
            }
        }

        /// <summary>
        /// Generates alphabet letters for the selection of the rotor rotations
        /// </summary>
        private void GenerateAlphabetLettersForStartPositions()
        {
            foreach (char c in Typex.Alphabet)
            {
                _alphabet.Add(string.Format("{0}", c));
            }
        }

        /// <summary>
        /// Sets the machine's settings to default values
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void SetDefaultValues()
        {
            switch (_typexMachineType)
            {
                case TypexMachineType.CyberChef:
                case TypexMachineType.TestRotors:
                case TypexMachineType.Y296:
                case TypexMachineType.SP02390:
                case TypexMachineType.SP02391:
                    _rotorPositions = "AAAAA";
                    _stator1 = 4;
                    _stator2 = 3;
                    _rotor1 = 2;
                    _rotor2 = 1;
                    _rotor3 = 0;
                    break;
                case TypexMachineType.EnigmaI:
                    _rotorPositions = "AAAAA";
                    _stator1 = 6;
                    _stator2 = 5;
                    _rotor1 = 2;
                    _rotor2 = 1;
                    _rotor3 = 0;
                    break;
                default:
                    throw new Exception(string.Format("Invalid Typex machine type: {0}", _typexMachineType));
            }

            _stator1IsReversed = false;
            _stator2IsReversed = false;
            _rotor1IsReversed = false;
            _rotor2IsReversed = false;
            _rotor3IsReversed = false;

            OnPropertyChanged(nameof(RotorPositions));
            OnPropertyChanged(nameof(Stator1));
            OnPropertyChanged(nameof(Stator2));
            OnPropertyChanged(nameof(Rotor1));
            OnPropertyChanged(nameof(Rotor2));
            OnPropertyChanged(nameof(Rotor3));
            OnPropertyChanged(nameof(Stator1IsReversed));
            OnPropertyChanged(nameof(Stator2IsReversed));
            OnPropertyChanged(nameof(Rotor1IsReversed));
            OnPropertyChanged(nameof(Rotor2IsReversed));
            OnPropertyChanged(nameof(Rotor3IsReversed));
        }

        #endregion

        [TaskPane("TypexMachineTypeCaption", "TypexMachineTypeTooltip", null, 1, false, ControlType.ComboBox, new string[] { "CyberChef", "Test rotors", "Enigma I", "Y296", "SP02390", "SP02391" })]
        public TypexMachineType TypexMachineType
        {
            get
            {
                return _typexMachineType;
            }
            set
            {
                if (value != _typexMachineType)
                {
                    _typexMachineType = value;
                    UpdateRotorAndStatorCollections();
                    SetDefaultValues();
                    OnPropertyChanged(nameof(TypexMachineType));
                }
            }
        }

        [TaskPane("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip", null, 2, false, ControlType.ComboBox, new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2", "UnknownSymbolHandlingList3" })]
        public UnknownSymbolHandlingMode UnknownSymbolHandling
        {
            get => _unknownSymbolHandlingMode;
            set
            {
                if (value != _unknownSymbolHandlingMode)
                {
                    _unknownSymbolHandlingMode = value;
                    OnPropertyChanged(nameof(UnknownSymbolHandling));
                }
            }
        }

        [TaskPane("RotorPositionsCaption", "RotorPositionsTooltip", "RotorPositionsGroupName", 3, false, ControlType.TextBox, ValidationType.RegEx, "[A-Za-z]{5}$")]
        public string RotorPositions
        {
            get => _rotorPositions;
            set
            {
                if (value != _rotorPositions)
                {
                    _rotorPositions = value;
                    OnPropertyChanged(nameof(RotorPositions));
                }
            }
        }

        [TaskPane("RingPositionsCaption", "RingPositionsTooltip", "RotorPositionsGroupName", 4, false, ControlType.TextBox, ValidationType.RegEx, "[A-Za-z]{5}$")]
        public string RingPositions
        {
            get => _ringPositions;
            set
            {
                if (value != _ringPositions)
                {
                    _ringPositions = value;
                    OnPropertyChanged(nameof(RingPositions));
                }
            }
        }

        [TaskPane("Rotor3Caption", "Rotor3Tooltip", "RotorSelectionGroupName", 5, false, ControlType.DynamicComboBox, new string[] { "Rotors3" })]
        public int Rotor3
        {
            get
            {
                return _rotor3;
            }
            set
            {
                if (value != _rotor3)
                {
                    _rotor3 = value;
                    OnPropertyChanged(nameof(Rotor3));
                }
            }
        }

        [TaskPane("IsReversedCaption", "IsReversedTooltip", "RotorSelectionGroupName", 6, false, ControlType.CheckBox)]
        public bool Rotor3IsReversed
        {
            get
            {
                return _rotor3IsReversed;
            }
            set
            {
                if (value != _rotor3IsReversed)
                {
                    _rotor3IsReversed = value;
                    OnPropertyChanged(nameof(Rotor3IsReversed));
                }
            }
        }

        [TaskPane("Rotor2Caption", "Rotor2Tooltip", "RotorSelectionGroupName", 7, false, ControlType.DynamicComboBox, new string[] { "Rotors2" })]
        public int Rotor2
        {
            get
            {
                return _rotor2;
            }
            set
            {
                if (value != _rotor2)
                {
                    _rotor2 = value;
                    OnPropertyChanged(nameof(Rotor2));
                }
            }
        }

        [TaskPane("IsReversedCaption", "IsReversedTooltip", "RotorSelectionGroupName", 8, false, ControlType.CheckBox)]
        public bool Rotor2IsReversed
        {
            get
            {
                return _rotor2IsReversed;
            }
            set
            {
                if (value != _rotor2IsReversed)
                {
                    _rotor2IsReversed = value;
                    OnPropertyChanged(nameof(Rotor2IsReversed));
                }
            }
        }

        [TaskPane("Rotor1Caption", "Rotor1Tooltip", "RotorSelectionGroupName", 9, false, ControlType.DynamicComboBox, new string[] { "Rotors1" })]
        public int Rotor1
        {
            get
            {
                return _rotor1;
            }
            set
            {
                if (value != _rotor1)
                {
                    _rotor1 = value;
                    OnPropertyChanged(nameof(Rotor1));
                }
            }
        }

        [TaskPane("IsReversedCaption", "IsReversedTooltip", "RotorSelectionGroupName", 10, false, ControlType.CheckBox)]
        public bool Rotor1IsReversed
        {
            get
            {
                return _rotor1IsReversed;
            }
            set
            {
                if (value != _rotor1IsReversed)
                {
                    _rotor1IsReversed = value;
                    OnPropertyChanged(nameof(Rotor1IsReversed));
                }
            }
        }

        [TaskPane("Stator2Caption", "Stator2Tooltip", "RotorSelectionGroupName", 11, false, ControlType.DynamicComboBox, new string[] { "Stators2" })]
        public int Stator2
        {
            get
            {
                return _stator2;
            }
            set
            {
                if (value != _stator2)
                {
                    _stator2 = value;
                    OnPropertyChanged(nameof(Stator2));
                }
            }
        }

        [TaskPane("IsReversedCaption", "IsReversedTooltip", "RotorSelectionGroupName", 12, false, ControlType.CheckBox)]
        public bool Stator2IsReversed
        {
            get
            {
                return _stator2IsReversed;
            }
            set
            {
                if (value != _stator2IsReversed)
                {
                    _stator2IsReversed = value;
                    OnPropertyChanged(nameof(Stator2IsReversed));
                }
            }
        }

        [TaskPane("Stator1Caption", "Stator1Tooltip", "RotorSelectionGroupName", 13, false, ControlType.DynamicComboBox, new string[] { "Stators1" })]
        public int Stator1
        {
            get
            {
                return _stator1;
            }
            set
            {
                if (value != _stator1)
                {
                    _stator1 = value;
                    OnPropertyChanged(nameof(Stator1));
                }
            }
        }

        [TaskPane("IsReversedCaption", "IsReversedTooltip", "RotorSelectionGroupName", 14, false, ControlType.CheckBox)]
        public bool Stator1IsReversed
        {
            get
            {
                return _stator1IsReversed;
            }
            set
            {
                if (value != _stator1IsReversed)
                {
                    _stator1IsReversed = value;
                    OnPropertyChanged(nameof(Stator1IsReversed));
                }
            }
        }

        [TaskPane("TypexReflectorCaption", "TypexReflectorTooltip", "TypexReflectorGroupName", 15, false, ControlType.ComboBox, new string[] { "CyberChef", "Test Reflector", "Enigma I UKWA", "Enigma I UKWB", "Enigma I UKWC", "Custom" })]
        public TypexReflector TypexReflector
        {
            get
            {
                return _typexReflector;
            }
            set
            {
                if (value != _typexReflector)
                {
                    _typexReflector = value;
                    OnPropertyChanged(nameof(TypexReflector));
                    UpdateCustomReflectorVisibility();
                }
            }
        }

        [TaskPane("CustomReflectorCaption", "CustomReflectorTooltip", "TypexReflectorGroupName", 16, false, ControlType.TextBox, ValidationType.RegEx, "[A-Za-z]{26}$")]
        public string CustomReflector
        {
            get
            {
                return _customReflector;
            }
            set
            {
                if (value != _customReflector)
                {
                    _customReflector = value;
                    OnPropertyChanged(nameof(CustomReflector));
                }
            }
        }

        [TaskPane("PlugSettingsCaption", "PlugSettingsToolip", "PlugSettingsGroupName", 17, false, ControlType.TextBox, ValidationType.RegEx, "[A-Za-z]{26}$")]
        public string PlugSettings
        {
            get => _plugSettings;
            set
            {
                if (value != _plugSettings)
                {
                    _plugSettings = value;
                    OnPropertyChanged(nameof(PlugSettings));
                }
            }
        }

        #region INotifyPropertyChanged Members        

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            UpdateRotorAndStatorCollections();
            GenerateAlphabetLettersForStartPositions();
            UpdateCustomReflectorVisibility();
        }

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        /// <summary>
        /// This event is needed in order to render settings elements visible/invisible
        /// </summary>
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        private void ShowSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Visible)));
            }
        }

        private void HideSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Collapsed)));
            }
        }

        private void UpdateCustomReflectorVisibility()
        {
            if (_typexReflector == TypexReflector.Custom)
            {
                ShowSettingsElement(nameof(CustomReflector));
            }
            else
            {
                HideSettingsElement(nameof(CustomReflector));
            }
        }
    }
}