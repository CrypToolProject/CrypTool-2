/*
   Copyright 2021 by Nils Kopal

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
using System.ComponentModel;

namespace CrypTool.Plugins.SIGABA
{
    public enum SIGABAAction
    {
        Encrypt,
        Decrypt
    }

    public enum CipherControlRotor
    {
        Zero = 0,
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
    }

    public enum IndexRotor
    {
        Zero = 0,
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
    }

    public enum UnknownSymbolHandling
    {
        Ignore,
        Remove,
        Replace
    }

    public class SIGABASettings : ISettings
    {
        private SIGABAAction _action = SIGABAAction.Encrypt;
        private SIGABAModel _model = SIGABAModel.CSP889;
        private bool _useZAsSpace = true;
        private UnknownSymbolHandling _unknownSymbolHandling = UnknownSymbolHandling.Ignore;

        private CipherControlRotor _cipherRotor0 = CipherControlRotor.Zero;
        private CipherControlRotor _cipherRotor1 = CipherControlRotor.One;
        private CipherControlRotor _cipherRotor2 = CipherControlRotor.Two;
        private CipherControlRotor _cipherRotor3 = CipherControlRotor.Three;
        private CipherControlRotor _cipherRotor4 = CipherControlRotor.Four;

        private bool _cipherRotor0Reversed = false;
        private bool _cipherRotor1Reversed = false;
        private bool _cipherRotor2Reversed = false;
        private bool _cipherRotor3Reversed = false;
        private bool _cipherRotor4Reversed = false;

        private string _cipherRotorPositions = "ABCDE";

        private CipherControlRotor _controlRotor0 = CipherControlRotor.Zero;
        private CipherControlRotor _controlRotor1 = CipherControlRotor.One;
        private CipherControlRotor _controlRotor2 = CipherControlRotor.Two;
        private CipherControlRotor _controlRotor3 = CipherControlRotor.Three;
        private CipherControlRotor _controlRotor4 = CipherControlRotor.Four;

        private bool _controlRotor0Reversed = false;
        private bool _controlRotor1Reversed = false;
        private bool _controlRotor2Reversed = false;
        private bool _controlRotor3Reversed = false;
        private bool _controlRotor4Reversed = false;

        private string _controlRotorPositions = "ABCDE";

        private IndexRotor _indexRotor0 = IndexRotor.Zero;
        private IndexRotor _indexRotor1 = IndexRotor.One;
        private IndexRotor _indexRotor2 = IndexRotor.Two;
        private IndexRotor _indexRotor3 = IndexRotor.Three;
        private IndexRotor _indexRotor4 = IndexRotor.Four;

        private string _indexRotorPositions = "12345";

        public event PropertyChangedEventHandler PropertyChanged;

        public void Initialize()
        {

        }

        [TaskPane("ModelCaption", "ModelTooltip", null, 1, false, ControlType.ComboBox, new string[] { "CSP-889", "CSP-2900" })]
        public SIGABAModel Model
        {
            get => _model;
            set
            {
                if (value != _model)
                {
                    _model = value;
                    OnPropertyChanged("Model");
                }
            }
        }

        [TaskPane("ActionCaption", "ActionTooltip", null, 2, false, ControlType.ComboBox, new string[] { "Encrypt", "Decrypt" })]
        public SIGABAAction Action
        {
            get => _action;
            set
            {
                if (value != _action)
                {
                    _action = value;
                    OnPropertyChanged("Action");
                }
            }
        }

        [TaskPane("UseZAsSpaceCaption", "UseZAsSpaceTooltip", null, 3, false, ControlType.CheckBox)]
        public bool UseZAsSpace
        {
            get => _useZAsSpace;
            set
            {
                if (value != _useZAsSpace)
                {
                    _useZAsSpace = value;
                    OnPropertyChanged("UseZAsSpace");
                }
            }
        }

        [TaskPane("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip", null, 4, false, ControlType.ComboBox, new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2", "UnknownSymbolHandlingList3" })]
        public UnknownSymbolHandling UnknownSymbolHandling
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

        #region start positions

        [TaskPane("CipherRotorPositionsCaption", "CipherRotorPositionsTooltip", "PositionsGroup", 5, false, ControlType.TextBox, ValidationType.RegEx, "[A-Za-z]{5}$")]
        public string CipherRotorPositions
        {
            get => _cipherRotorPositions;
            set
            {
                if (value != _cipherRotorPositions)
                {
                    _cipherRotorPositions = value;
                    OnPropertyChanged("CipherRotorPositions");
                }
            }
        }

        [TaskPane("ControlRotorPositionsCaption", "ControlRotorPositionsTooltip", "PositionsGroup", 6, false, ControlType.TextBox, ValidationType.RegEx, "[A-Za-z]{5}$")]
        public string ControlRotorPositions
        {
            get => _controlRotorPositions;
            set
            {
                if (value != _controlRotorPositions)
                {
                    _controlRotorPositions = value;
                    OnPropertyChanged("ControlRotorPositions");
                }
            }
        }

        [TaskPane("IndexRotorPositionsCaption", "IndexRotorPositionsTooltip", "PositionsGroup", 7, false, ControlType.TextBox, ValidationType.RegEx, @"\d{5}$")]
        public string IndexRotorPositions
        {
            get => _indexRotorPositions;
            set
            {
                if (value != _indexRotorPositions)
                {
                    _indexRotorPositions = value;
                    OnPropertyChanged("IndexRotorPositions");
                }
            }
        }

        #endregion

        #region cipher rotors

        [TaskPane("CipherRotor0Caption", "CipherRotorTooltip", "CipherRotorGroup", 8, false, ControlType.ComboBox, new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" })]
        public CipherControlRotor CipherRotor0
        {
            get => _cipherRotor0;
            set
            {
                if (value != _cipherRotor0)
                {
                    _cipherRotor0 = value;
                    OnPropertyChanged("CipherRotor0");
                }
            }
        }

        [TaskPane("Reversed", "ReversedTooltip", "CipherRotorGroup", 9, false, ControlType.CheckBox)]
        public bool CipherRotor0Reversed
        {
            get => _cipherRotor0Reversed;
            set
            {
                if (value != _cipherRotor0Reversed)
                {
                    _cipherRotor0Reversed = value;
                    OnPropertyChanged("CipherRotor0Reversed");
                }
            }
        }

        [TaskPane("CipherRotor1Caption", "CipherRotorTooltip", "CipherRotorGroup", 10, false, ControlType.ComboBox, new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" })]
        public CipherControlRotor CipherRotor1
        {
            get => _cipherRotor1;
            set
            {
                if (value != _cipherRotor1)
                {
                    _cipherRotor1 = value;
                    OnPropertyChanged("CipherRotor1");
                }
            }
        }

        [TaskPane("Reversed", "ReversedTooltip", "CipherRotorGroup", 11, false, ControlType.CheckBox)]
        public bool CipherRotor1Reversed
        {
            get => _cipherRotor1Reversed;
            set
            {
                if (value != _cipherRotor1Reversed)
                {
                    _cipherRotor1Reversed = value;
                    OnPropertyChanged("CipherRotor1Reversed");
                }
            }
        }

        [TaskPane("CipherRotor2Caption", "CipherRotorTooltip", "CipherRotorGroup", 12, false, ControlType.ComboBox, new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" })]
        public CipherControlRotor CipherRotor2
        {
            get => _cipherRotor2;
            set
            {
                if (value != _cipherRotor2)
                {
                    _cipherRotor2 = value;
                    OnPropertyChanged("CipherRotor2");
                }
            }
        }

        [TaskPane("Reversed", "ReversedTooltip", "CipherRotorGroup", 13, false, ControlType.CheckBox)]
        public bool CipherRotor2Reversed
        {
            get => _cipherRotor2Reversed;
            set
            {
                if (value != _cipherRotor2Reversed)
                {
                    _cipherRotor2Reversed = value;
                    OnPropertyChanged("CipherRotor2Reversed");
                }
            }
        }

        [TaskPane("CipherRotor3Caption", "CipherRotorTooltip", "CipherRotorGroup", 14, false, ControlType.ComboBox, new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" })]
        public CipherControlRotor CipherRotor3
        {
            get => _cipherRotor3;
            set
            {
                if (value != _cipherRotor3)
                {
                    _cipherRotor3 = value;
                    OnPropertyChanged("CipherRotor3");
                }
            }
        }

        [TaskPane("Reversed", "ReversedTooltip", "CipherRotorGroup", 15, false, ControlType.CheckBox)]
        public bool CipherRotor3Reversed
        {
            get => _cipherRotor3Reversed;
            set
            {
                if (value != _cipherRotor3Reversed)
                {
                    _cipherRotor3Reversed = value;
                    OnPropertyChanged("CipherRotor3Reversed");
                }
            }
        }

        [TaskPane("CipherRotor4Caption", "CipherRotorTooltip", "CipherRotorGroup", 16, false, ControlType.ComboBox, new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" })]
        public CipherControlRotor CipherRotor4
        {
            get => _cipherRotor4;
            set
            {
                if (value != _cipherRotor4)
                {
                    _cipherRotor4 = value;
                    OnPropertyChanged("CipherRotor4");
                }
            }
        }

        [TaskPane("Reversed", "ReversedTooltip", "CipherRotorGroup", 17, false, ControlType.CheckBox)]
        public bool CipherRotor4Reversed
        {
            get => _cipherRotor4Reversed;
            set
            {
                if (value != _cipherRotor4Reversed)
                {
                    _cipherRotor4Reversed = value;
                    OnPropertyChanged("CipherRotor4Reversed");
                }
            }
        }

        #endregion

        #region control rotors       

        [TaskPane("ControlRotor0Caption", "ControlRotorTooltip", "ControlRotorGroup", 18, false, ControlType.ComboBox, new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" })]
        public CipherControlRotor ControlRotor0
        {
            get => _controlRotor0;
            set
            {
                if (value != _controlRotor0)
                {
                    _controlRotor0 = value;
                    OnPropertyChanged("ControlRotor0");
                }
            }
        }

        [TaskPane("Reversed", "ReversedTooltip", "ControlRotorGroup", 19, false, ControlType.CheckBox)]
        public bool ControlRotor0Reversed
        {
            get => _controlRotor0Reversed;
            set
            {
                if (value != _controlRotor0Reversed)
                {
                    _controlRotor0Reversed = value;
                    OnPropertyChanged("ControlRotor0Reversed");
                }
            }
        }

        [TaskPane("ControlRotor1Caption", "ControlRotorTooltip", "ControlRotorGroup", 20, false, ControlType.ComboBox, new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" })]
        public CipherControlRotor ControlRotor1
        {
            get => _controlRotor1;
            set
            {
                if (value != _controlRotor1)
                {
                    _controlRotor1 = value;
                    OnPropertyChanged("ControlRotor1");
                }
            }
        }

        [TaskPane("Reversed", "ReversedTooltip", "ControlRotorGroup", 21, false, ControlType.CheckBox)]
        public bool ControlRotor1Reversed
        {
            get => _controlRotor1Reversed;
            set
            {
                if (value != _controlRotor1Reversed)
                {
                    _controlRotor1Reversed = value;
                    OnPropertyChanged("ControlRotor1Reversed");
                }
            }
        }

        [TaskPane("ControlRotor2Caption", "ControlRotorTooltip", "ControlRotorGroup", 22, false, ControlType.ComboBox, new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" })]
        public CipherControlRotor ControlRotor2
        {
            get => _controlRotor2;
            set
            {
                if (value != _controlRotor2)
                {
                    _controlRotor2 = value;
                    OnPropertyChanged("ControlRotor2");
                }
            }
        }

        [TaskPane("Reversed", "ReversedTooltip", "ControlRotorGroup", 23, false, ControlType.CheckBox)]
        public bool ControlRotor2Reversed
        {
            get => _controlRotor2Reversed;
            set
            {
                if (value != _controlRotor2Reversed)
                {
                    _controlRotor2Reversed = value;
                    OnPropertyChanged("ControlRotor2Reversed");
                }
            }
        }

        [TaskPane("ControlRotor3Caption", "ControlRotorTooltip", "ControlRotorGroup", 24, false, ControlType.ComboBox, new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" })]
        public CipherControlRotor ControlRotor3
        {
            get => _controlRotor3;
            set
            {
                if (value != _controlRotor3)
                {
                    _controlRotor3 = value;
                    OnPropertyChanged("ControlRotor3");
                }
            }
        }

        [TaskPane("Reversed", "ReversedTooltip", "ControlRotorGroup", 25, false, ControlType.CheckBox)]
        public bool ControlRotor3Reversed
        {
            get => _controlRotor3Reversed;
            set
            {
                if (value != _controlRotor3Reversed)
                {
                    _controlRotor3Reversed = value;
                    OnPropertyChanged("ControlRotor3Reversed");
                }
            }
        }

        [TaskPane("ControlRotor4Caption", "ControlRotorTooltip", "ControlRotorGroup", 26, false, ControlType.ComboBox, new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" })]
        public CipherControlRotor ControlRotor4
        {
            get => _controlRotor4;
            set
            {
                if (value != _controlRotor4)
                {
                    _controlRotor4 = value;
                    OnPropertyChanged("ControlRotor4");
                }
            }
        }

        [TaskPane("Reversed", "ReversedTooltip", "ControlRotorGroup", 27, false, ControlType.CheckBox)]
        public bool ControlRotor4Reversed
        {
            get => _controlRotor4Reversed;
            set
            {
                if (value != _controlRotor4Reversed)
                {
                    _controlRotor4Reversed = value;
                    OnPropertyChanged("ControlRotor4Reversed");
                }
            }
        }

        #endregion

        #region control rotors       

        [TaskPane("IndexRotor0Caption", "IndexRotorTooltip", "IndexRotorGroup", 28, false, ControlType.ComboBox, new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" })]
        public IndexRotor IndexRotor0
        {
            get => _indexRotor0;
            set
            {
                if (value != _indexRotor0)
                {
                    _indexRotor0 = value;
                    OnPropertyChanged("IndexRotor0");
                }
            }
        }

        [TaskPane("IndexRotor1Caption", "IndexRotorTooltip", "IndexRotorGroup", 29, false, ControlType.ComboBox, new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" })]
        public IndexRotor IndexRotor1
        {
            get => _indexRotor1;
            set
            {
                if (value != _indexRotor1)
                {
                    _indexRotor1 = value;
                    OnPropertyChanged("IndexRotor1");
                }
            }
        }

        [TaskPane("IndexRotor2Caption", "IndexRotorTooltip", "IndexRotorGroup", 30, false, ControlType.ComboBox, new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" })]
        public IndexRotor IndexRotor2
        {
            get => _indexRotor2;
            set
            {
                if (value != _indexRotor2)
                {
                    _indexRotor2 = value;
                    OnPropertyChanged("IndexRotor2");
                }
            }
        }

        [TaskPane("IndexRotor3Caption", "IndexRotorTooltip", "IndexRotorGroup", 31, false, ControlType.ComboBox, new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" })]
        public IndexRotor IndexRotor3
        {
            get => _indexRotor3;
            set
            {
                if (value != _indexRotor3)
                {
                    _indexRotor3 = value;
                    OnPropertyChanged("IndexRotor3");
                }
            }
        }

        [TaskPane("IndexRotor4Caption", "IndexRotorTooltip", "IndexRotorGroup", 32, false, ControlType.ComboBox, new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" })]
        public IndexRotor IndexRotor4
        {
            get => _indexRotor4;
            set
            {
                if (value != _indexRotor4)
                {
                    _indexRotor4 = value;
                    OnPropertyChanged("IndexRotor4");
                }
            }
        }

        #endregion

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
