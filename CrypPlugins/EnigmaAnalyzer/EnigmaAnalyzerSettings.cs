/*
   Copyright 2020 George Lasry, Nils Kopal, CrypTool project

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
using EnigmaAnalyzerLib.Common;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using static EnigmaAnalyzerLib.Key;

namespace CrypTool.EnigmaAnalyzer
{
    #region some setting enums

    public enum Rotor
    {
        One = 0,
        Two = 1,
        Three = 2,
        Four = 3,
        Five = 4,
        Six = 5,
        Seven = 6,
        Eight = 7
    }

    public enum GreekRotor
    {
        Beta = 8,
        Gamma = 9
    }

    public enum RingRotorPosition
    {
        A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z
    }

    public enum Reflector
    {
        A = 0,
        B = 1,
        C = 2
    }

    public enum AnalysisMode
    {
        GILLOGLY,
        BOMBE,
        IC_SEARCH,
        TRIGRAM_SEARCH,
        HILLCLIMBING,
        SIMULATED_ANNEALING,
    }

    #endregion

    public class EnigmaAnalyzerSettings : ISettings
    {
        private Model _model = Model.M3;
        private AnalysisMode _analysisMode = AnalysisMode.GILLOGLY;
        private Language _analysisLanguage = Language.ENGLISH;
        private ObservableCollection<string> coresAvailable = new ObservableCollection<string>();
        private int _coresUsed = 0; // 0 means 1 core, 1 means 2 cores, etc...

        private int _unknownSymbolHandling = 0; // 0=ignore, leave unmodified
        private int _caseHandling = 0; // 0=preserve, 1, convert all to upper, 2= convert all to lower        

        private Reflector _reflectorFrom = Reflector.B;
        private Reflector _reflectorTo = Reflector.B;

        private GreekRotor _greekRotorFrom = GreekRotor.Beta;
        private GreekRotor _greekRotorTo = GreekRotor.Gamma;
        private Rotor _leftRotorFrom = Rotor.One;
        private Rotor _leftRotorTo = Rotor.Five;
        private Rotor _middleRotorFrom = Rotor.One;
        private Rotor _middleRotorTo = Rotor.Five;
        private Rotor _rightRotorFrom = Rotor.One;
        private Rotor _rightRotorTo = Rotor.Five;

        private RingRotorPosition _greekRingFrom = RingRotorPosition.A;
        private RingRotorPosition _greekRingTo = RingRotorPosition.Z;
        private RingRotorPosition _leftRingFrom = RingRotorPosition.A;
        private RingRotorPosition _leftRingTo = RingRotorPosition.Z;
        private RingRotorPosition _middleRingFrom = RingRotorPosition.A;
        private RingRotorPosition _middleRingTo = RingRotorPosition.Z;
        private RingRotorPosition _rightRingFrom = RingRotorPosition.A;
        private RingRotorPosition _rightRingTo = RingRotorPosition.Z;

        private RingRotorPosition _greekRotorPositionFrom = RingRotorPosition.A;
        private RingRotorPosition _greekRotorPositionTo = RingRotorPosition.Z;
        private RingRotorPosition _leftRotorPositionFrom = RingRotorPosition.A;
        private RingRotorPosition _leftRotorPositionTo = RingRotorPosition.Z;
        private RingRotorPosition _middleRotorPositionFrom = RingRotorPosition.A;
        private RingRotorPosition _middleRotorPositionTo = RingRotorPosition.Z;
        private RingRotorPosition _rightRotorPositionFrom = RingRotorPosition.A;
        private RingRotorPosition _rightRotorPositionTo = RingRotorPosition.Z;

        private int _cribPositionFrom = 0;
        private int _cribPositionTo = 0;

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public EnigmaAnalyzerSettings()
        {
            CoresAvailable.Clear();
            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                CoresAvailable.Add((i + 1).ToString());
            }
        }

        public void Initialize()
        {
            UpdateSettingsVisibility();
        }

        private void UpdateSettingsVisibility()
        {
            //change visibility of rotor, ring, and rotor position settings
            ShowHideSetting("GreekRotorFrom", false);
            ShowHideSetting("GreekRotorTo", false);
            ShowHideSetting("GreekRingFrom", false);
            ShowHideSetting("GreekRingTo", false);
            ShowHideSetting("GreekRotorPositionFrom", false);
            ShowHideSetting("GreekRotorPositionTo", false);
            if (_model == Model.M4) //only M4 has the Greek rotor
            {
                ShowHideSetting("GreekRotorFrom", true);
                ShowHideSetting("GreekRotorTo", true);
                ShowHideSetting("GreekRingFrom", true);
                ShowHideSetting("GreekRingTo", true);
                ShowHideSetting("GreekRotorPositionFrom", true);
                ShowHideSetting("GreekRotorPositionTo", true);
            }

            //change visibility of crib position
            if (_analysisMode != AnalysisMode.BOMBE)
            {
                ShowHideSetting("CribPositionFrom", false);
                ShowHideSetting("CribPositionTo", false);
            }
            else //only Bombe analysis needs a crib position
            {
                ShowHideSetting("CribPositionFrom", true);
                ShowHideSetting("CribPositionTo", true);
            }
        }

        /// <summary>
        /// Get the number of cores in a collection, used for the selection of cores
        /// </summary>        
        [DontSave]
        public ObservableCollection<string> CoresAvailable
        {
            get => coresAvailable;
            set
            {
                if (value != coresAvailable)
                {
                    coresAvailable = value;
                    OnPropertyChanged("CoresAvailable");
                }
            }
        }

        #region Analysis options

        [TaskPane("MachineModelCaption", "MachineModelTooltip", "AnalysisTypeGroup", 0, false, ControlType.ComboBox, new string[] { "H", "M3", "M4", "A16081", "A16101" })]
        public Model Model
        {
            get => _model;
            set
            {
                if (value != _model)
                {
                    _model = value;
                    OnPropertyChanged("Model");
                    UpdateSettingsVisibility();
                }
            }
        }

        [TaskPane("AnalysisModeCaption", "AnalysisModeTooltip", "AnalysisTypeGroup", 1, false, ControlType.ComboBox, new string[] { "Gillogly", "Turing Bombe", "IoC Search", "Trigram Search", "Hillclimbing", "Simulated Annealing" })]
        public AnalysisMode AnalysisMode
        {
            get => _analysisMode;
            set
            {
                if (value != _analysisMode)
                {
                    _analysisMode = value;
                    OnPropertyChanged("AnalysisMode");
                    UpdateSettingsVisibility();
                }
            }
        }

        [TaskPane("CribPositionFromCaption", "CribPositionFromTooltip", "AnalysisTypeGroup", 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 10000)]
        public int CribPositionFrom
        {
            get => _cribPositionFrom;
            set
            {
                if (value != _cribPositionFrom)
                {
                    _cribPositionFrom = value;
                    OnPropertyChanged("CribPositionFrom");
                }
            }
        }

        [TaskPane("CribPositionToCaption", "CribPositionToTooltip", "AnalysisTypeGroup", 3, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 10000)]
        public int CribPositionTo
        {
            get => _cribPositionTo;
            set
            {
                if (value != _cribPositionTo)
                {
                    _cribPositionTo = value;
                    OnPropertyChanged("CribPositionTo");
                }
            }
        }


        [TaskPane("AnalysisLanguageCaption", "AnalysisLanguageTooltip", "AnalysisTypeGroup", 4, false, ControlType.ComboBox, new string[] { "English", "French", "German", "Italian" })]
        public Language AnalysisLanguage
        {
            get => _analysisLanguage;
            set
            {
                if (value != _analysisLanguage)
                {
                    _analysisLanguage = value;
                    OnPropertyChanged("AnalysisLanguage");
                }
            }
        }

        /// <summary>
        /// Getter/Setter for the number of cores which should be used by ADFGVXAnalyzer
        /// </summary>
        [TaskPane("ThreadsCaption", "ThreadsToolTip", "AnalysisTypeGroup", 5, false, ControlType.DynamicComboBox, new string[] { "CoresAvailable" })]
        public int CoresUsed
        {
            get => _coresUsed;
            set
            {
                if (value != _coresUsed)
                {
                    _coresUsed = value;
                    OnPropertyChanged("CoresUsed");
                }
            }
        }

        #endregion

        #region Key options

        #region reflector settings

        [TaskPane("ReflectorFromCaption", "ReflectorFromTooltip", "ReflectorGroup", 2, false, ControlType.ComboBox, new string[] { "A", "B", "C" })]
        public Reflector ReflectorFrom
        {
            get => _reflectorFrom;
            set
            {
                if (value != _reflectorFrom)
                {
                    _reflectorFrom = value;
                    OnPropertyChanged("ReflectorFrom");
                }
            }
        }

        [TaskPane("ReflectorToCaption", "ReflectorToTooltip", "ReflectorGroup", 3, false, ControlType.ComboBox, new string[] { "A", "B", "C" })]
        public Reflector ReflectorTo
        {
            get => _reflectorTo;
            set
            {
                if (value != _reflectorTo)
                {
                    _reflectorTo = value;
                    OnPropertyChanged("ReflectorTo");
                }
            }
        }

        #endregion

        #region rotor settings

        [TaskPane("GreekRotorFromCaption", "GreekRotorFromTooltip", "RotorGroup", 1, false, ControlType.ComboBox, new string[] { "Beta", "Gamma" })]
        public GreekRotor GreekRotorFrom
        {
            get => _greekRotorFrom;
            set
            {
                if (value != _greekRotorFrom)
                {
                    _greekRotorFrom = value;
                    OnPropertyChanged("GreekRotorFrom");
                }
            }
        }

        [TaskPane("GreekRotorToCaption", "GreekRotorToTooltip", "RotorGroup", 1, false, ControlType.ComboBox, new string[] { "Beta", "Gamma" })]
        public GreekRotor GreekRotorTo
        {
            get => _greekRotorTo;
            set
            {
                if (value != _greekRotorTo)
                {
                    _greekRotorTo = value;
                    OnPropertyChanged("GreekRotorTo");
                }
            }
        }

        [TaskPane("LeftRotorFromCaption", "LeftRotorFromTooltip", "RotorGroup", 2, false, ControlType.ComboBox, new string[] { "I", "II", "III", "IV", "V", "VI", "VII", "VIII" })]
        public Rotor LeftRotorFrom
        {
            get => _leftRotorFrom;
            set
            {
                if (value != _leftRotorFrom)
                {
                    _leftRotorFrom = value;
                    OnPropertyChanged("LeftRotorFrom");
                }
            }
        }

        [TaskPane("LeftRotorToCaption", "LeftRotorToTooltip", "RotorGroup", 2, false, ControlType.ComboBox, new string[] { "I", "II", "III", "IV", "V", "VI", "VII", "VIII" })]
        public Rotor LeftRotorTo
        {
            get => _leftRotorTo;
            set
            {
                if (value != _leftRotorTo)
                {
                    _leftRotorTo = value;
                    OnPropertyChanged("LeftRotorTo");
                }
            }
        }

        [TaskPane("MiddleRotorFromCaption", "MiddleRotorFromTooltip", "RotorGroup", 3, false, ControlType.ComboBox, new string[] { "I", "II", "III", "IV", "V", "VI", "VII", "VIII" })]
        public Rotor MiddleRotorFrom
        {
            get => _middleRotorFrom;
            set
            {
                if (value != _middleRotorFrom)
                {
                    _middleRotorFrom = value;
                    OnPropertyChanged("MiddleRotorFrom");
                }
            }
        }

        [TaskPane("MiddleRotorToCaption", "MiddleRotorToTooltip", "RotorGroup", 3, false, ControlType.ComboBox, new string[] { "I", "II", "III", "IV", "V", "VI", "VII", "VIII" })]
        public Rotor MiddleRotorTo
        {
            get => _middleRotorTo;
            set
            {
                if (value != _middleRotorTo)
                {
                    _middleRotorTo = value;
                    OnPropertyChanged("MiddleRotorTo");
                }
            }
        }

        [TaskPane("RightRotorFromCaption", "RightRotorFromTooltip", "RotorGroup", 4, false, ControlType.ComboBox, new string[] { "I", "II", "III", "IV", "V", "VI", "VII", "VIII" })]
        public Rotor RightRotorFrom
        {
            get => _rightRotorFrom;
            set
            {
                if (value != _rightRotorFrom)
                {
                    _rightRotorFrom = value;
                    OnPropertyChanged("RightRotorFrom");
                }
            }
        }

        [TaskPane("RightRotorToCaption", "RightRotorToTooltip", "RotorGroup", 4, false, ControlType.ComboBox, new string[] { "I", "II", "III", "IV", "V", "VI", "VII", "VIII" })]
        public Rotor RightRotorTo
        {
            get => _rightRotorTo;
            set
            {
                if (value != _rightRotorTo)
                {
                    _rightRotorTo = value;
                    OnPropertyChanged("RightRotorTo");
                }
            }
        }

        #endregion

        #region ring settings

        [TaskPane("GreekRingFromCaption", "GreekRingFromTooltip", "RingGroup", 1, false, ControlType.ComboBox, new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public RingRotorPosition GreekRingFrom
        {
            get => _greekRingFrom;
            set
            {
                if (value != _greekRingFrom)
                {
                    _greekRingFrom = value;
                    OnPropertyChanged("GreekRingFrom");
                }
            }
        }

        [TaskPane("GreekRingToCaption", "GreekRingToTooltip", "RingGroup", 1, false, ControlType.ComboBox, new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public RingRotorPosition GreekRingTo
        {
            get => _greekRingTo;
            set
            {
                if (value != _greekRingTo)
                {
                    _greekRingTo = value;
                    OnPropertyChanged("GreekRingTo");
                }
            }
        }

        [TaskPane("LeftRingFromCaption", "LeftRingFromTooltip", "RingGroup", 2, false, ControlType.ComboBox, new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public RingRotorPosition LeftRingFrom
        {
            get => _leftRingFrom;
            set
            {
                if (value != _leftRingFrom)
                {
                    _leftRingFrom = value;
                    OnPropertyChanged("LeftRingFrom");
                }
            }
        }

        [TaskPane("LeftRingToCaption", "LeftRingToTooltip", "RingGroup", 2, false, ControlType.ComboBox, new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public RingRotorPosition LeftRingTo
        {
            get => _leftRingTo;
            set
            {
                if (value != _leftRingTo)
                {
                    _leftRingTo = value;
                    OnPropertyChanged("LeftRingTo");
                }
            }
        }

        [TaskPane("MiddleRingFromCaption", "MiddleRingFromTooltip", "RingGroup", 3, false, ControlType.ComboBox, new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public RingRotorPosition MiddleRingFrom
        {
            get => _middleRingFrom;
            set
            {
                if (value != _middleRingFrom)
                {
                    _middleRingFrom = value;
                    OnPropertyChanged("MiddleRingFrom");
                }
            }
        }

        [TaskPane("MiddleRingToCaption", "MiddleRingToTooltip", "RingGroup", 3, false, ControlType.ComboBox, new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public RingRotorPosition MiddleRingTo
        {
            get => _middleRingTo;
            set
            {
                if (value != _middleRingTo)
                {
                    _middleRingTo = value;
                    OnPropertyChanged("MiddleRingTo");
                }
            }
        }

        [TaskPane("RightRingFromCaption", "RightRingFromTooltip", "RingGroup", 4, false, ControlType.ComboBox, new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public RingRotorPosition RightRingFrom
        {
            get => _rightRingFrom;
            set
            {
                if (value != _rightRingFrom)
                {
                    _rightRingFrom = value;
                    OnPropertyChanged("RightRingFrom");
                }
            }
        }

        [TaskPane("RightRingToCaption", "RightRingToTooltip", "RingGroup", 4, false, ControlType.ComboBox, new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public RingRotorPosition RightRingTo
        {
            get => _rightRingTo;
            set
            {
                if (value != _rightRingTo)
                {
                    _rightRingTo = value;
                    OnPropertyChanged("RightRingTo");
                }
            }
        }

        #endregion

        #region rotor position settings

        [TaskPane("GreekRotorPositionFromCaption", "GreekRotorPositionFromPositionTooltip", "RotorPositionGroup", 1, false, ControlType.ComboBox, new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public RingRotorPosition GreekRotorPositionFrom
        {
            get => _greekRotorPositionFrom;
            set
            {
                if (value != _greekRotorPositionFrom)
                {
                    _greekRotorPositionFrom = value;
                    OnPropertyChanged("GreekRotorPositionFrom");
                }
            }
        }

        [TaskPane("GreekRotorPositionToCaption", "GreekRotorPositionToPositionTooltip", "RotorPositionGroup", 1, false, ControlType.ComboBox, new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public RingRotorPosition GreekRotorPositionTo
        {
            get => _greekRotorPositionTo;
            set
            {
                if (value != _greekRotorPositionTo)
                {
                    _greekRotorPositionTo = value;
                    OnPropertyChanged("GreekRotorPositionTo");
                }
            }
        }

        [TaskPane("LeftRotorPositionFromCaption", "LeftRotorPositionFromPositionTooltip", "RotorPositionGroup", 2, false, ControlType.ComboBox, new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public RingRotorPosition LeftRotorPositionFrom
        {
            get => _leftRotorPositionFrom;
            set
            {
                if (value != _leftRotorPositionFrom)
                {
                    _leftRotorPositionFrom = value;
                    OnPropertyChanged("LeftRotorPositionFrom");
                }
            }
        }

        [TaskPane("LeftRotorPositionToCaption", "LeftRotorPositionToPositionTooltip", "RotorPositionGroup", 2, false, ControlType.ComboBox, new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public RingRotorPosition LeftRotorPositionTo
        {
            get => _leftRotorPositionTo;
            set
            {
                if (value != _leftRotorPositionTo)
                {
                    _leftRotorPositionTo = value;
                    OnPropertyChanged("LeftRotorPositionTo");
                }
            }
        }

        [TaskPane("MiddleRotorPositionFromCaption", "MiddleRotorPositionFromPositionTooltip", "RotorPositionGroup", 3, false, ControlType.ComboBox, new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public RingRotorPosition MiddleRotorPositionFrom
        {
            get => _middleRotorPositionFrom;
            set
            {
                if (value != _middleRotorPositionFrom)
                {
                    _middleRotorPositionFrom = value;
                    OnPropertyChanged("MiddleRotorPositionFrom");
                }
            }
        }

        [TaskPane("MiddleRotorPositionToCaption", "MiddleRotorPositionToPositionTooltip", "RotorPositionGroup", 3, false, ControlType.ComboBox, new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public RingRotorPosition MiddleRotorPositionTo
        {
            get => _middleRotorPositionTo;
            set
            {
                if (value != _middleRotorPositionTo)
                {
                    _middleRotorPositionTo = value;
                    OnPropertyChanged("MiddleRotorPositionTo");
                }
            }
        }

        [TaskPane("RightRotorPositionFromCaption", "RightRotorPositionFromPositionTooltip", "RotorPositionGroup", 4, false, ControlType.ComboBox, new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public RingRotorPosition RightRotorPositionFrom
        {
            get => _rightRotorPositionFrom;
            set
            {
                if (value != _rightRotorPositionFrom)
                {
                    _rightRotorPositionFrom = value;
                    OnPropertyChanged("RightRotorPositionFrom");
                }
            }
        }

        [TaskPane("RightRotorPositionToCaption", "RightRotorPositionToPositionTooltip", "RotorPositionGroup", 4, false, ControlType.ComboBox, new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public RingRotorPosition RightRotorPositionTo
        {
            get => _rightRotorPositionTo;
            set
            {
                if (value != _rightRotorPositionTo)
                {
                    _rightRotorPositionTo = value;
                    OnPropertyChanged("RightRotorPositionTo");
                }
            }
        }

        #endregion

        #endregion

        #region Text options

        [TaskPane("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip", "TextOptionsGroup", 20, false, ControlType.ComboBox, new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2", "UnknownSymbolHandlingList3" })]
        public int UnknownSymbolHandling
        {
            get => _unknownSymbolHandling;
            set
            {
                if (value != _unknownSymbolHandling)
                {
                    _unknownSymbolHandling = value;
                    OnPropertyChanged("UnknownSymbolHandling");
                }
            }
        }

        [TaskPane("CaseHandlingCaption", "CaseHandlingTooltip", "TextOptionsGroup", 21, false, ControlType.ComboBox, new string[] { "CaseHandlingList1", "CaseHandlingList2", "CaseHandlingList3" })]
        public int CaseHandling
        {
            get => _caseHandling;
            set
            {
                if (value != _caseHandling)
                {
                    _caseHandling = value;
                    OnPropertyChanged("CaseHandling");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }


        public static string GetReflectString(Reflector reflector)
        {
            switch (reflector)
            {
                case Reflector.A:
                    return "A";
                case Reflector.B:
                    return "B";
                case Reflector.C:
                    return "C";
                default:
                    return "B";
            }
        }

        public static string GetRotorString(Rotor rotor)
        {
            switch (rotor)
            {
                case Rotor.One:
                    return "1";
                case Rotor.Two:
                    return "2";
                case Rotor.Three:
                    return "3";
                case Rotor.Four:
                    return "4";
                case Rotor.Five:
                    return "5";
                case Rotor.Six:
                    return "6";
                case Rotor.Seven:
                    return "7";
                case Rotor.Eight:
                    return "8";
                default:
                    return "1";
            }
        }

        public static string GetRotorString(GreekRotor rotor)
        {
            switch (rotor)
            {
                case GreekRotor.Beta:
                    return "B";
                case GreekRotor.Gamma:
                    return "G";
                default:
                    return "B";
            }
        }

        public static string GetRotorRingPositionString(RingRotorPosition position)
        {
            switch (position)
            {
                case RingRotorPosition.A:
                    return "A";
                case RingRotorPosition.B:
                    return "B";
                case RingRotorPosition.C:
                    return "C";
                case RingRotorPosition.D:
                    return "D";
                case RingRotorPosition.E:
                    return "E";
                case RingRotorPosition.F:
                    return "F";
                case RingRotorPosition.G:
                    return "G";
                case RingRotorPosition.H:
                    return "H";
                case RingRotorPosition.I:
                    return "I";
                case RingRotorPosition.J:
                    return "J";
                case RingRotorPosition.K:
                    return "K";
                case RingRotorPosition.L:
                    return "L";
                case RingRotorPosition.M:
                    return "M";
                case RingRotorPosition.N:
                    return "N";
                case RingRotorPosition.O:
                    return "O";
                case RingRotorPosition.P:
                    return "P";
                case RingRotorPosition.Q:
                    return "Q";
                case RingRotorPosition.R:
                    return "R";
                case RingRotorPosition.S:
                    return "S";
                case RingRotorPosition.T:
                    return "T";
                case RingRotorPosition.U:
                    return "U";
                case RingRotorPosition.V:
                    return "V";
                case RingRotorPosition.W:
                    return "W";
                case RingRotorPosition.X:
                    return "X";
                case RingRotorPosition.Y:
                    return "Y";
                case RingRotorPosition.Z:
                    return "Z";
                default:
                    return "A";
            }
        }

        #endregion

        private void ShowHideSetting(string propertyName, bool show)
        {
            if (TaskPaneAttributeChanged == null)
            {
                return;
            }

            TaskPaneAttribteContainer container = new TaskPaneAttribteContainer(propertyName, show == true ? Visibility.Visible : Visibility.Collapsed);
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(container));
        }
    }
}