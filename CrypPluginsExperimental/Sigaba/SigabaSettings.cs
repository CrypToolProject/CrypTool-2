/*
   Copyright 2011 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using System.Collections.ObjectModel;
using System.ComponentModel;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

namespace Sigaba
{
    // HOWTO: rename class (click name, press F2)
    public class SigabaSettings : ISettings
    {
        #region Private Variables

        private ObservableCollection<string> _typeStrings = new ObservableCollection<string>();
        private ObservableCollection<string> _actionStrings = new ObservableCollection<string>();
        private ObservableCollection<string> _cipherControlRotorStrings = new ObservableCollection<string>();
        private ObservableCollection<string> _indexRotorStrings = new ObservableCollection<string>();

        public string _cipherKey = "OOOOO";
        public string _controlKey = "OOOOO";
        public string _indexKey = "00000";

        public int _type = 0;

        private int _presentationSpeed = 100;

        private int _unknownSymbolHandling = 0;
        private int _caseHandling = 0;

        private int[] _cipherRotor = new int[5] { 1, 2, 3, 4, 5 };
        private int[] _controlRotor = new int[5] { 6, 7, 8, 9, 10 };
        private int[] _indexRotor = new int[5] { 1, 2, 3, 4, 5 };

        private bool[] _cipherRotorReverse = new bool[5] { false, false, false, false, false };
        private bool[] _controlRotorReverse = new bool[5] { false, false, false, false, false };
        private bool[] _indexRotorReverse = new bool[5] { false, false, false, false, false };

        private string _alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private int _action = 0; // we will need crypt, decrypt, zerozize, Plaintext, O for off and Reset Position

        #endregion

        #region Initialisation / Contructor

        public SigabaSettings()
        {
            SetList(_typeStrings, "CSP888/889", "CSP2800");
            SetList(_actionStrings, "ActionList1", "ActionList2");
            SetList(_cipherControlRotorStrings, "TestRotor", "RotorA1", "RotorA2", "RotorA3", "RotorA4", "RotorA5", "RotorA6", "RotorA7", "RotorA8", "RotorA9", "RotorA10");
            SetList(_indexRotorStrings, "TestRotor", "RotorB1", "RotorB2", "RotorB3", "RotorB4", "RotorB5");
        }

        #endregion

        #region TaskPane Settings

        private void SetList(ObservableCollection<string> coll, params string[] keys)
        {
            coll.Clear();
            foreach (string key in keys)
                coll.Add(typeof(Sigaba).GetPluginStringResource(key));
        }

        [TaskPane("TypeCaption", "TypeTooltip", null, 0, false, ControlType.DynamicComboBox, new string[] { "TypeStrings" })]
        public int Type
        {
            get { return this._type; }
            set
            {
                if (((int)value) != _type)
                {
                    this._type = (int)value;
                    OnPropertyChanged("Type");
                }
            }
        }

        [TaskPane("ActionCaption", "ActionTooltip", null, 1, false, ControlType.DynamicComboBox, new string[] { "ActionStrings" })]
        public int Action
        {
            get { return this._action; }
            set
            {
                if (((int)value) != _action)
                {
                    this._action = (int)value;
                    OnPropertyChanged("Action");
                }
            }
        }

        [TaskPane("ControlKeyCaption", "ControlKeyTooltip", null, 2, false, ControlType.TextBox, ValidationType.RegEx, "^[A-Z]{5}$")]
        public string ControlKey
        {
            get { return this._controlKey; }
            set
            {
                if (value != _controlKey)
                {
                    this._controlKey = value;
                    OnPropertyChanged("ControlKey");
                }
            }
        }

        [TaskPane("CipherKeyCaption", "CipherKeyTooltip", null, 3, false, ControlType.TextBox, ValidationType.RegEx, "^[A-Z]{5}$")]
        public string CipherKey
        {
            get { return this._cipherKey; }
            set
            {
                if (value != _cipherKey)
                {
                    this._cipherKey = value;
                    OnPropertyChanged("CipherKey");
                }
            }
        }

        [TaskPane("IndexKeyCaption", "IndexKeyTooltip", null, 4, false, ControlType.TextBox, ValidationType.RegEx, "^[0-9]{5}$")]
        public string IndexKey
        {
            get { return this._indexKey; }
            set
            {
                if (value != _indexKey)
                {
                    this._indexKey = value;
                    OnPropertyChanged("IndexKey");
                }
            }
        }

        [ContextMenu("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip", 3, ContextMenuControlType.ComboBox, null, new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2", "UnknownSymbolHandlingList3" })]
        [TaskPane("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip", "TextOptionsGroup", 3, false, ControlType.ComboBox, new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2", "UnknownSymbolHandlingList3" })]
        public int UnknownSymbolHandling
        {
            get { return _unknownSymbolHandling; }
            set
            {
                if ((int)value != _unknownSymbolHandling)
                {
                    _unknownSymbolHandling = (int)value;
                    OnPropertyChanged("UnknownSymbolHandling");
                }
            }
        }

        [ContextMenu("CaseHandlingCaption", "CaseHandlingTooltip", 4, ContextMenuControlType.ComboBox, null, new string[] { "CaseHandlingList1", "CaseHandlingList2", "CaseHandlingList3" })]
        [TaskPane("CaseHandlingCaption", "CaseHandlingTooltip", "TextOptionsGroup", 4, false, ControlType.ComboBox, new string[] { "CaseHandlingList1", "CaseHandlingList2", "CaseHandlingList3" })]
        public int CaseHandling
        {
            get { return _caseHandling; }
            set
            {
                if ((int)value != _caseHandling)
                {
                    _caseHandling = (int)value;
                    OnPropertyChanged("CaseHandling");
                }
            }
        }

        [PropertySaveOrder(11)]
        public ObservableCollection<string> TypeStrings
        {
            get { return _typeStrings; }
            set
            {
                if (value != _typeStrings)
                {
                    _typeStrings = value;
                    OnPropertyChanged("TypeStrings");
                }
            }
        }

        [PropertySaveOrder(9)]
        public ObservableCollection<string> ActionStrings
        {
            get { return _actionStrings; }
            set
            {
                if (value != _actionStrings)
                {
                    _actionStrings = value;
                    OnPropertyChanged("ActionStrings");
                }
            }
        }

        [PropertySaveOrder(10)]
        [TaskPane("PresentationSpeedCaption", "PresentationSpeedTooltip", "PresentationGroup", 6, true, ControlType.Slider, 1, 40)]
        public int PresentationSpeed
        {
            get { return (int)_presentationSpeed; }
            set
            {
                if ((value) != _presentationSpeed)
                {
                    this._presentationSpeed = value;
                    OnPropertyChanged("PresentationSpeed");
                }
            }
        }

        #endregion

        #region Used rotor settings

        private void setReverse(bool[] array, int index, bool value, string name)
        {
            if (array[index] != value)
            {
                array[index] = value;
                OnPropertyChanged(name + "Rotor" + (index + 1) + "Reverse");
            }
        }

        private void setRotor(int[] array, int index, int value, string name)
        {
            if (array[index] != value)
            {
                CheckRotorChange(index, array[index], value);
                array[index] = value;
                OnPropertyChanged(name + "Rotor" + (index + 1));
            }
        }

        private void setIndexRotor(int rotor, int value)
        {
            if (_indexRotor[rotor] != value)
            {
                CheckIndexChange(rotor, value);
                _indexRotor[rotor] = value;
                OnPropertyChanged("IndexRotor" + (rotor + 1));
            }
        }

        private void CheckRotorChange(int rotor, int was, int becomes)
        {
            for (int r = 0; r < 5; r++)
            {
                if (_cipherRotor[r] == becomes)
                {
                    _cipherRotor[r] = was;
                    OnPropertyChanged("CipherRotor" + (r + 1));
                    break;
                }
                if (_controlRotor[r] == becomes)
                {
                    _controlRotor[r] = was;
                    OnPropertyChanged("ControlRotor" + (r + 1));
                    break;
                }
            }
        }

        private void CheckIndexChange(int rotor, int becomes)
        {
            for (int r = 0; r < 5; r++)
                if (_indexRotor[r] == becomes)
                {
                    _indexRotor[r] = _indexRotor[rotor];
                    OnPropertyChanged("IndexRotor" + (r + 1));
                    break;
                }
        }

        // Cipher Rotors

        [TaskPane("CipherRotor1Caption", "CipherRotor1Tooltip", "CipherGroup", 5, false, ControlType.DynamicComboBox, new string[] { "CipherControlRotorStrings" })]
        public int CipherRotor1
        {
            get { return _cipherRotor[0]; }
            set { setRotor(_cipherRotor, 0, value, "Cipher"); }
        }

        [TaskPane("RotorReverseCaption", "RotorReverseTooltip", "CipherGroup", 6, false, ControlType.CheckBox, "Reverse")]
        public Boolean CipherRotor1Reverse
        {
            get { return _cipherRotorReverse[0]; }
            set { setReverse(_cipherRotorReverse, 0, value, "Cipher"); }
        }

        [TaskPane("CipherRotor2Caption", "CipherRotor2Tooltip", "CipherGroup", 7, false, ControlType.DynamicComboBox, new string[] { "CipherControlRotorStrings" })]
        public int CipherRotor2
        {
            get { return _cipherRotor[1]; }
            set { setRotor(_cipherRotor, 1, value, "Cipher"); }
        }

        [TaskPane("RotorReverseCaption", "RotorReverseTooltip", "CipherGroup", 8, false, ControlType.CheckBox, "Reverse")]
        public Boolean CipherRotor2Reverse
        {
            get { return _cipherRotorReverse[1]; }
            set { setReverse(_cipherRotorReverse, 1, value, "Cipher"); }
        }

        [TaskPane("CipherRotor3Caption", "CipherRotor3Tooltip", "CipherGroup", 9, false, ControlType.DynamicComboBox, new string[] { "CipherControlRotorStrings" })]
        public int CipherRotor3
        {
            get { return _cipherRotor[2]; }
            set { setRotor(_cipherRotor, 2, value, "Cipher"); }
        }

        [TaskPane("RotorReverseCaption", "RotorReverseTooltip", "CipherGroup", 10, false, ControlType.CheckBox, "Reverse")]
        public Boolean CipherRotor3Reverse
        {
            get { return _cipherRotorReverse[2]; }
            set { setReverse(_cipherRotorReverse, 2, value, "Cipher"); }
        }

        [TaskPane("CipherRotor4Caption", "CipherRotor4Tooltip", "CipherGroup", 11, false, ControlType.DynamicComboBox, new string[] { "CipherControlRotorStrings" })]
        public int CipherRotor4
        {
            get { return _cipherRotor[3]; }
            set { setRotor(_cipherRotor, 3, value, "Cipher"); }
        }

        [TaskPane("RotorReverseCaption", "RotorReverseTooltip", "CipherGroup", 12, false, ControlType.CheckBox, "Reverse")]
        public Boolean CipherRotor4Reverse
        {
            get { return _cipherRotorReverse[3]; }
            set { setReverse(_cipherRotorReverse, 3, value, "Cipher"); }
        }

        [TaskPane("CipherRotor5Caption", "CipherRotor5Tooltip", "CipherGroup", 13, false, ControlType.DynamicComboBox, new string[] { "CipherControlRotorStrings" })]
        public int CipherRotor5
        {
            get { return _cipherRotor[4]; }
            set { setRotor(_cipherRotor, 4, value, "Cipher"); }
        }

        [TaskPane("RotorReverseCaption", "RotorReverseTooltip", "CipherGroup", 14, false, ControlType.CheckBox, "Reverse")]
        public Boolean CipherRotor5Reverse
        {
            get { return _cipherRotorReverse[4]; }
            set { setReverse(_cipherRotorReverse, 4, value, "Cipher"); }
        }

        // Control Rotors

        [TaskPane("ControlRotor1Caption", "ControlRotor1Tooltip", "ControlGroup", 15, false, ControlType.DynamicComboBox, new string[] { "CipherControlRotorStrings" })]
        public int ControlRotor1
        {
            get { return _controlRotor[0]; }
            set { setRotor(_controlRotor, 0, value, "Control"); }
        }

        [TaskPane("RotorReverseCaption", "RotorReverseTooltip", "ControlGroup", 16, false, ControlType.CheckBox, "Reverse")]
        public Boolean ControlRotor1Reverse
        {
            get { return _controlRotorReverse[0]; }
            set { setReverse(_controlRotorReverse, 0, value, "Control"); }
        }

        [TaskPane("ControlRotor2Caption", "ControlRotor2Tooltip", "ControlGroup", 17, false, ControlType.DynamicComboBox, new string[] { "CipherControlRotorStrings" })]
        public int ControlRotor2
        {
            get { return _controlRotor[1]; }
            set { setRotor(_controlRotor, 1, value, "Control"); }
        }

        [TaskPane("RotorReverseCaption", "RotorReverseTooltip", "ControlGroup", 18, false, ControlType.CheckBox, "Reverse")]
        public Boolean ControlRotor2Reverse
        {
            get { return _controlRotorReverse[1]; }
            set { setReverse(_controlRotorReverse, 1, value, "Control"); }
        }

        [TaskPane("ControlRotor3Caption", "ControlRotor3Tooltip", "ControlGroup", 19, false, ControlType.DynamicComboBox, new string[] { "CipherControlRotorStrings" })]
        public int ControlRotor3
        {
            get { return _controlRotor[2]; }
            set { setRotor(_controlRotor, 2, value, "Control"); }
        }

        [TaskPane("RotorReverseCaption", "RotorReverseTooltip", "ControlGroup", 20, false, ControlType.CheckBox, "Reverse")]
        public Boolean ControlRotor3Reverse
        {
            get { return _controlRotorReverse[2]; }
            set { setReverse(_controlRotorReverse, 2, value, "Control"); }
        }

        [TaskPane("ControlRotor4Caption", "ControlRotor4Tooltip", "ControlGroup", 21, false, ControlType.DynamicComboBox, new string[] { "CipherControlRotorStrings" })]
        public int ControlRotor4
        {
            get { return _controlRotor[3]; }
            set { setRotor(_controlRotor, 3, value, "Control"); }
        }

        [TaskPane("RotorReverseCaption", "RotorReverseTooltip", "ControlGroup", 22, false, ControlType.CheckBox, "Reverse")]
        public Boolean ControlRotor4Reverse
        {
            get { return _controlRotorReverse[3]; }
            set { setReverse(_controlRotorReverse, 3, value, "Control"); }
        }

        [TaskPane("ControlRotor5Caption", "ControlRotor5Tooltip", "ControlGroup", 23, false, ControlType.DynamicComboBox, new string[] { "CipherControlRotorStrings" })]
        public int ControlRotor5
        {
            get { return _controlRotor[4]; }
            set { setRotor(_controlRotor, 4, value, "Control"); }
        }

        [TaskPane("RotorReverseCaption", "RotorReverseTooltip", "ControlGroup", 24, false, ControlType.CheckBox, "Reverse")]
        public Boolean ControlRotor5Reverse
        {
            get { return _controlRotorReverse[4]; }
            set { setReverse(_controlRotorReverse, 4, value, "Control"); }
        }

        // Index Rotors

        [TaskPane("IndexRotor1Caption", "IndexRotor1Tooltip", "IndexGroup", 25, false, ControlType.DynamicComboBox, new string[] { "IndexRotorStrings" })]
        public int IndexRotor1
        {
            get { return _indexRotor[0]; }
            set { setIndexRotor(0, value); }
        }

        [TaskPane("RotorReverseCaption", "RotorReverseTooltip", "IndexGroup", 26, false, ControlType.CheckBox, "Reverse")]
        public Boolean IndexRotor1Reverse
        {
            get { return _indexRotorReverse[0]; }
            set { setReverse(_indexRotorReverse, 0, value, "Index"); }
        }

        [TaskPane("IndexRotor2Caption", "IndexRotor2Tooltip", "IndexGroup", 27, false, ControlType.DynamicComboBox, new string[] { "IndexRotorStrings" })]
        public int IndexRotor2
        {
            get { return _indexRotor[1]; }
            set { setIndexRotor(1, value); }
        }

        [TaskPane("RotorReverseCaption", "RotorReverseTooltip", "IndexGroup", 28, false, ControlType.CheckBox, "Reverse")]
        public Boolean IndexRotor2Reverse
        {
            get { return _indexRotorReverse[1]; }
            set { setReverse(_indexRotorReverse, 1, value, "Index"); }
        }

        [TaskPane("IndexRotor3Caption", "IndexRotor3Tooltip", "IndexGroup", 29, false, ControlType.DynamicComboBox, new string[] { "IndexRotorStrings" })]
        public int IndexRotor3
        {
            get { return _indexRotor[2]; }
            set { setIndexRotor(2, value); }
        }

        [TaskPane("RotorReverseCaption", "RotorReverseTooltip", "IndexGroup", 30, false, ControlType.CheckBox, "Reverse")]
        public Boolean IndexRotor3Reverse
        {
            get { return _indexRotorReverse[2]; }
            set { setReverse(_indexRotorReverse, 2, value, "Index"); }
        }

        [TaskPane("IndexRotor4Caption", "IndexRotor4Tooltip", "IndexGroup", 31, false, ControlType.DynamicComboBox, new string[] { "IndexRotorStrings" })]
        public int IndexRotor4
        {
            get { return _indexRotor[3]; }
            set { setIndexRotor(3, value); }
        }

        [TaskPane("RotorReverseCaption", "RotorReverseTooltip", "IndexGroup", 32, false, ControlType.CheckBox, "Reverse")]
        public Boolean IndexRotor4Reverse
        {
            get { return _indexRotorReverse[3]; }
            set { setReverse(_indexRotorReverse, 3, value, "Index"); }
        }

        [TaskPane("IndexRotor5Caption", "IndexRotor5Tooltip", "IndexGroup", 33, false, ControlType.DynamicComboBox, new string[] { "IndexRotorStrings" })]
        public int IndexRotor5
        {
            get { return _indexRotor[4]; }
            set { setIndexRotor(4, value); }
        }

        [TaskPane("RotorReverseCaption", "RotorReverseTooltip", "IndexGroup", 34, false, ControlType.CheckBox, "Reverse")]
        public Boolean IndexRotor5Reverse
        {
            get { return _indexRotorReverse[4]; }
            set { setReverse(_indexRotorReverse, 4, value, "Index"); }
        }
        
        /// <summary>
        /// This collection contains the values for the Rotor 1-3 comboboxes.
        /// </summary>
        public ObservableCollection<string> CipherControlRotorStrings
        {
            get { return _cipherControlRotorStrings; }
            set
            {
                if (value != _cipherControlRotorStrings)
                {
                    _cipherControlRotorStrings = value;
                    OnPropertyChanged("RotorAStrings");
                }
            }
        }

        /// <summary>
        /// This collection contains the values for the Rotor 4 combobox.
        /// </summary>
        public ObservableCollection<string> IndexRotorStrings
        {
            get { return _indexRotorStrings; }
            set
            {
                if (value != _indexRotorStrings)
                {
                    _indexRotorStrings = value;
                    OnPropertyChanged("RotorBStrings");
                }
            }
        }

        #endregion

        #region Public properties

        public string Alphabet
        {
            get { return _alphabet; }
            set { _alphabet = value; }
        }

        #endregion

        #region Events

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