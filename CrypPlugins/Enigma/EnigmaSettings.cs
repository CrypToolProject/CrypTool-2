/*
   Copyright 2008-2017, Arno Wacker, University of Kassel

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


// CrypTool 2.0 specific includes
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Collections.ObjectModel;
//additionally needed libs
using System.ComponentModel;
using System.Text;
using System.Windows;


namespace CrypTool.Enigma
{
    public class EnigmaSettings : ISettings
    {
        #region Private variables

        private ObservableCollection<string> rotorAStrings = new ObservableCollection<string>();
        private ObservableCollection<string> rotorBStrings = new ObservableCollection<string>();
        private ObservableCollection<string> reflectorStrings = new ObservableCollection<string>();

        private int model = 3;
        private int unknownSymbolHandling = 0; // 0 = ignore, 1 = leave unmodified
        private int caseHandling = 0; // 0 = preserve, 1 = convert all to upper, 2 = convert all to lower
        private string _initialRotorPos = "AAA";
        private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private int rotor1 = 0;
        private int rotor2 = 1;
        private int rotor3 = 2;
        private int rotor4 = 0;

        private int ring1 = 1; // 01 = A, 02 = B ...
        private int ring2 = 1;
        private int ring3 = 1;
        private int ring4 = 1;

        private int reflector = 0;

        private int Presentation_Speed = 1;

        private StringBuilder plugBoard = new StringBuilder("ABCDEFGHIJKLMNOPQRSTUVWXYZ");

        #endregion

        #region Private methods

        private void checkRotorChange(int rotor, int was, int becomes)
        {
            if (rotor1 == becomes) { rotor1 = was; OnPropertyChanged("Rotor1"); }
            if (rotor2 == becomes) { rotor2 = was; OnPropertyChanged("Rotor2"); }
            if (rotor3 == becomes) { rotor3 = was; OnPropertyChanged("Rotor3"); }
        }

        private void setPlugBoard(int letterPos, int newIndex)
        {
            if (newIndex != alphabet.IndexOf(plugBoard[letterPos]))
            {
                char newChar = alphabet[newIndex];
                //int newCharIndex = plugBoard.ToString().IndexOf(newChar);
                char currentChar = plugBoard[letterPos];
                int currentIndex = alphabet.IndexOf(currentChar);
                int preconnect = alphabet.IndexOf(plugBoard[newIndex]);

                if (plugBoard[preconnect] != alphabet[preconnect])
                {
                    plugBoard[preconnect] = alphabet[preconnect];
                    OnPropertyChanged("PlugBoard" + alphabet[preconnect]);
                }
                plugBoard[newIndex] = alphabet[letterPos];
                OnPropertyChanged("PlugBoard" + alphabet[newIndex]);
                if (plugBoard[letterPos] != alphabet[letterPos])
                {
                    plugBoard[currentIndex] = alphabet[currentIndex];
                    OnPropertyChanged("PlugBoard" + alphabet[currentIndex]);

                }
                plugBoard[letterPos] = newChar;
                OnPropertyChanged("PlugBoard" + alphabet[letterPos]);
                //OnPropertyChanged("PlugBoardDisplay");
                OnPropertyChanged("PlugBoard");

            }
        }

        private void hidePlugBoard()
        {
            foreach (char c in alphabet)
            {
                hideSettingsElement("PlugBoard" + c);
            }

            hideSettingsElement("PlugBoard");
            hideSettingsElement("ResetPlugboard");
        }

        private void showPlugBoard()
        {
            foreach (char c in alphabet)
            {
                showSettingsElement("PlugBoard" + c);
            }

            showSettingsElement("PlugBoard");
            showSettingsElement("ResetPlugboard");
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

        #endregion

        #region Public Events and Methods

        /// <summary>
        /// This event is needed in order to render settings elements visible/invisible
        /// </summary>
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public int AlphabetIndexOf(char c)
        {
            return c - 'A';
        }

        public void HideAllBasicKeySettings()
        {
            hideSettingsElement("Rotor1");
            hideSettingsElement("Rotor2");
            hideSettingsElement("Rotor3");
            hideSettingsElement("Rotor4");
            hideSettingsElement("Ring1");
            hideSettingsElement("Ring2");
            hideSettingsElement("Ring3");
            hideSettingsElement("Ring4");
            hidePlugBoard();
        }

        public void SetKeySettings(string inputKey)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            // delete whitespaces
            inputKey = inputKey.Replace(" ", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);
            inputKey = inputKey.ToUpper();

            string[] keyElements = inputKey.Split(':');

            if (keyElements.Length != 4)
            {
                throw new Exception("Invalid key. Key has to be of the following format:  Reflector:Rotors:Ring:RotorPositions|Plugboard");
            }

            string reflectorString = keyElements[0];
            string rotorString = keyElements[1];
            string ringString = keyElements[2];
            string[] split = keyElements[3].Split('|');
            _initialRotorPos = split[0];
            string plugBoardString = null;
            if (split.Length == 2)
            {
                plugBoardString = split[1];
            }

            if (rotorString.Length < 3 || rotorString.Length > 4)
            {
                throw new Exception("Invalid key. You have to define 3 or 4 rotors, depending on selected machine model");
            }

            if (ringString.Length != rotorString.Length || _initialRotorPos.Length != rotorString.Length)
            {
                throw new Exception("Invalid key. You have to define same number of rotors, rings, and rotor positions");
            }

            SetReflectorByString(reflectorString);
            SetRotorsByString(rotorString);
            SetRingByString(ringString);
            if (plugBoardString != null)
            {
                SetPlugBoardByString(plugBoardString);
            }
            OnPropertyChanged("InitialRotorPos");
            foreach (char c in _initialRotorPos)
            {
                if (alphabet.IndexOf(c) < 0)
                {
                    throw new Exception("Invalid key. Rotor positions have to be defined from A to Z");
                }
            }
        }

        private void SetReflectorByString(string reflectorString)
        {
            const string reflectors = "ABC";
            Reflector = reflectors.IndexOf(reflectorString.ToUpper());
            if (Reflector < 0)
            {
                throw new Exception("Invalid key. Reflector has to be A,B, or C");
            }
        }

        private void SetRotorsByString(string rotorString)
        {
            const string rotorNumbers = "12345678";
            if (rotorString.Length == 3)
            {
                rotor1 = rotorNumbers.IndexOf(rotorString[2]);
                rotor2 = rotorNumbers.IndexOf(rotorString[1]);
                rotor3 = rotorNumbers.IndexOf(rotorString[0]);
                OnPropertyChanged("Rotor1");
                OnPropertyChanged("Rotor2");
                OnPropertyChanged("Rotor3");
            }
            else
            {
                rotor1 = rotorNumbers.IndexOf(rotorString[3]);
                rotor2 = rotorNumbers.IndexOf(rotorString[2]);
                rotor3 = rotorNumbers.IndexOf(rotorString[1]);
                rotor4 = rotorNumbers.IndexOf(rotorString[0]);
                OnPropertyChanged("Rotor1");
                OnPropertyChanged("Rotor2");
                OnPropertyChanged("Rotor3");
                OnPropertyChanged("Rotor4");
            }

            if (rotor1 < 0 || rotor2 < 0 || rotor3 < 0 || (rotorString.Length == 4 && rotor4 < 0))
            {
                throw new Exception("Invalid key. Rotors have to be defined from A to Z");
            }

        }

        private void SetRingByString(string ringString)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            ringString = ringString.ToUpper();
            if (ringString.Length == 3)
            {
                ring1 = alphabet.IndexOf(ringString[2]) + 1;
                ring2 = alphabet.IndexOf(ringString[1]) + 1;
                ring3 = alphabet.IndexOf(ringString[0]) + 1;
                OnPropertyChanged("Ring1");
                OnPropertyChanged("Ring2");
                OnPropertyChanged("Ring3");
            }
            else
            {
                ring1 = alphabet.IndexOf(ringString[3]) + 1;
                ring2 = alphabet.IndexOf(ringString[2]) + 1;
                ring3 = alphabet.IndexOf(ringString[1]) + 1;
                ring4 = alphabet.IndexOf(ringString[0]) + 1;
                OnPropertyChanged("Ring1");
                OnPropertyChanged("Ring2");
                OnPropertyChanged("Ring3");
                OnPropertyChanged("Ring4");
            }
            if (ring1 < 1 || ring2 < 1 || ring3 < 1 || (ringString.Length == 4 && ring4 < 1))
            {
                throw new Exception("Invalid key. Rings have to be defined from A to Z");
            }
        }

        private void SetPlugBoardByString(string plugBoardString)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            if (plugBoardString.Length % 2 != 0)
            {
                throw new Exception("Invalid key. Plugboard definition has to be of even length");
            }

            ResetPlugboard();
            plugBoardString = plugBoardString.ToUpper();

            for (int i = 0; i < plugBoardString.Length; i += 2)
            {
                int indexLetterOne = alphabet.IndexOf(plugBoardString[i]);
                int indexLetterTwo = alphabet.IndexOf(plugBoardString[i + 1]);
                if (indexLetterOne < 0 || indexLetterTwo < 0)
                {
                    throw new Exception("Invalid key. Plugboard has to be defined from A to Z");
                }
                setPlugBoard(indexLetterOne, indexLetterTwo);
                setPlugBoard(indexLetterTwo, indexLetterOne);
            }
        }

        #endregion

        #region Initialisation / Contructor

        public EnigmaSettings()
        {
            SetList(rotorAStrings, "RotorA1", "RotorA2", "RotorA3", "RotorA4", "RotorA5", "RotorA6", "RotorA7", "RotorA8");
            SetList(rotorBStrings, "RotorB1");
            SetList(reflectorStrings, "Reflector1", "Reflector2", "Reflector3");
        }

        public void Initialize()
        {
            SetSettingsVisibilityForModel(Model);
        }

        #endregion

        #region Public properties

        public string Alphabet
        {
            get => alphabet;
            set => alphabet = value;
        }

        #endregion

        #region Taskpane settings

        private void SetList(ObservableCollection<string> coll, params string[] keys)
        {
            coll.Clear();
            foreach (string key in keys)
            {
                coll.Add(typeof(Enigma).GetPluginStringResource(key));
            }
        }

        [TaskPane("ModelTPCaption", "ModelTPTooltip", null, 0, false, ControlType.ComboBox, new string[] { "ModelList1", "ModelList2", "ModelList3", "ModelList4", "ModelList5", "ModelList6", "ModelList7" })]
        [PropertySaveOrder(1)]
        public int Model
        {
            get => model;
            set
            {
                if (value == model)
                {
                    return;
                }

                model = value;
                OnPropertyChanged("Model");

                switch (model)
                {
                    case 0: // Enigma A/B
                        SetList(rotorAStrings, "RotorA9", "RotorA10", "RotorA11");
                        SetList(rotorBStrings, "RotorB1");
                        SetList(reflectorStrings, "Reflector10");

                        if (_initialRotorPos.Length > 3)
                        {
                            _initialRotorPos = _initialRotorPos.Remove(0, 1);
                        }

                        rotor1 = 0; rotor2 = 1; rotor3 = 2; rotor4 = 0;
                        reflector = 0;
                        break;

                    case 1: // Enigma D
                        SetList(rotorAStrings, "RotorA12", "RotorA13", "RotorA14");
                        SetList(rotorBStrings, "RotorB1");
                        SetList(reflectorStrings, "Reflector4");

                        if (_initialRotorPos.Length > 3)
                        {
                            _initialRotorPos = _initialRotorPos.Remove(0, 1);
                        }

                        rotor1 = 0; rotor2 = 1; rotor3 = 2; rotor4 = 0;
                        reflector = 0;
                        break;

                    case 2: // Reichsbahn (Rocket)
                        SetList(rotorAStrings, "RotorA15", "RotorA16", "RotorA17"); // "RotorA4"); //you must add a  Rotor 4 for the challenge on MTC3 (Cascading encryption - Part 3/3)
                        SetList(rotorBStrings, "RotorB1");
                        SetList(reflectorStrings, "Reflector5");

                        if (_initialRotorPos.Length > 3)
                        {
                            _initialRotorPos = _initialRotorPos.Remove(0, 1);
                        }

                        rotor1 = 0; rotor2 = 1; rotor3 = 2; rotor4 = 0;
                        reflector = 0;
                        break;

                    case 3: // Enigma I / M3
                        SetList(rotorAStrings, "RotorA1", "RotorA2", "RotorA3", "RotorA4", "RotorA5", "RotorA6", "RotorA7", "RotorA8");
                        SetList(rotorBStrings, "RotorB1");
                        SetList(reflectorStrings, "Reflector1", "Reflector2", "Reflector3");

                        if (_initialRotorPos.Length > 3)
                        {
                            _initialRotorPos = _initialRotorPos.Remove(0, 1);
                        }

                        rotor1 = 0; rotor2 = 1; rotor3 = 2; rotor4 = 0;
                        reflector = 0;
                        break;

                    case 4: // Enigma M4 "Shark"
                        SetList(rotorAStrings, "RotorA1", "RotorA2", "RotorA3", "RotorA4", "RotorA5", "RotorA6", "RotorA7", "RotorA8");
                        SetList(rotorBStrings, "RotorB2", "RotorB3");
                        SetList(reflectorStrings, "Reflector6", "Reflector7");

                        if (_initialRotorPos.Length < 4)
                        {
                            _initialRotorPos = "A" + _initialRotorPos;
                        }

                        rotor1 = 0; rotor2 = 1; rotor3 = 2; rotor4 = 0;
                        reflector = 0;

                        break;

                    case 5: // Enigma K-Model
                        SetList(rotorAStrings, "RotorA18", "RotorA19", "RotorA20");
                        SetList(rotorBStrings, "RotorB1");
                        SetList(reflectorStrings, "Reflector8");

                        if (_initialRotorPos.Length > 3)
                        {
                            _initialRotorPos = _initialRotorPos.Remove(0, 1);
                        }

                        rotor1 = 0; rotor2 = 1; rotor3 = 2; rotor4 = 0;
                        reflector = 0;

                        break;

                    case 6: // Enigam G / Abwehr
                        SetList(rotorAStrings, "RotorA21", "RotorA22", "RotorA23");
                        SetList(rotorBStrings, "RotorB1");
                        SetList(reflectorStrings, "Reflector9");

                        if (_initialRotorPos.Length > 3)
                        {
                            _initialRotorPos = _initialRotorPos.Remove(0, 1);
                        }

                        rotor1 = 0; rotor2 = 1; rotor3 = 2; rotor4 = 0;
                        reflector = 0;

                        break;
                }

                SetSettingsVisibilityForModel(value);

                OnPropertyChanged("InitialRotorPos");
                OnPropertyChanged("Rotor1");
                OnPropertyChanged("Rotor2");
                OnPropertyChanged("Rotor3");
                OnPropertyChanged("Rotor4");
                OnPropertyChanged("Reflector");
            }
        }

        public void SetSettingsVisibilityForModel(int model)
        {
            showSettingsElement("Rotor1");
            showSettingsElement("Rotor2");
            showSettingsElement("Rotor3");
            showSettingsElement("Ring1");
            showSettingsElement("Ring2");
            showSettingsElement("Ring3");
            showPlugBoard();

            switch (model)
            {
                case 0: // Enigma A/B
                    hideSettingsElement("Rotor4"); hideSettingsElement("Ring4"); hideSettingsElement("Reflector");
                    break;
                case 4: // Enigma M4 "Shark"
                    showSettingsElement("Rotor4"); showSettingsElement("Ring4"); showSettingsElement("Reflector");
                    break;
                case 1: // Enigma D
                case 2: // Reichsbahn (Rocket)
                case 3: // Enigma I / M3
                case 5: // Enigma K-Model
                case 6: // Enigam G / Abwehr
                    hideSettingsElement("Rotor4"); hideSettingsElement("Ring4"); showSettingsElement("Reflector");
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown Enigma model '{model}'.");
            }
        }

        [TaskPane("InitialRotorPosCaption", "InitialRotorPosTooltip",
            null, 1, false, ControlType.TextBox, ValidationType.RegEx, "^[A-Za-z]{3,4}$")]
        public string InitialRotorPos
        {
            get => _initialRotorPos.ToUpper();
            set
            {
                if (value != _initialRotorPos)
                {
                    _initialRotorPos = value.ToUpper();
                    OnPropertyChanged("InitialRotorPos");
                }
            }
        }

        #endregion

        #region Used rotor settings

        [TaskPane("Rotor1Caption", "Rotor1Tooltip",
            "UsedRotorsGroup", 13, false, ControlType.DynamicComboBox, new string[] { "RotorAStrings" })]
        public int Rotor1
        {
            get => rotor1;
            set
            {
                if (value != rotor1)
                {
                    checkRotorChange(1, rotor1, value);
                    rotor1 = value;
                    OnPropertyChanged("Rotor1");
                }
            }
        }

        [TaskPane("Rotor2Caption", "Rotor2Tooltip",
            "UsedRotorsGroup", 12, false, ControlType.DynamicComboBox, new string[] { "RotorAStrings" })]
        public int Rotor2
        {
            get => rotor2;
            set
            {
                if (value != rotor2)
                {
                    checkRotorChange(2, rotor2, value);
                    rotor2 = value;
                    OnPropertyChanged("Rotor2");
                }
            }
        }

        [TaskPane("Rotor3Caption", "Rotor3Tooltip",
            "UsedRotorsGroup", 11, false, ControlType.DynamicComboBox, new string[] { "RotorAStrings" })]
        public int Rotor3
        {
            get => rotor3;
            set
            {
                if (value != rotor3)
                {
                    checkRotorChange(3, rotor3, value);
                    rotor3 = value;
                    OnPropertyChanged("Rotor3");
                }
            }
        }

        [TaskPane("Rotor4Caption", "Rotor4Tooltip",
            "UsedRotorsGroup", 10, false, ControlType.DynamicComboBox, new string[] { "RotorBStrings" })]
        public int Rotor4
        {
            get => rotor4;
            set
            {
                if (value != rotor4)
                {
                    rotor4 = value;
                    OnPropertyChanged("Rotor4");
                }
            }
        }


        [TaskPane("ReflectorCaption", "ReflectorTooltip",
            "UsedRotorsGroup", 14, false, ControlType.DynamicComboBox, new string[] { "ReflectorStrings" })]
        public int Reflector
        {
            get => reflector;
            set
            {
                if (value != reflector)
                {
                    reflector = value;
                    OnPropertyChanged("Reflector");
                }
            }
        }

        /// <summary>
        /// This collection contains the values for the Rotor 1-3 comboboxes.
        /// </summary>
        [DontSave]
        public ObservableCollection<string> RotorAStrings
        {
            get => rotorAStrings;
            set
            {
                if (value != rotorAStrings)
                {
                    rotorAStrings = value;
                    OnPropertyChanged("RotorAStrings");
                }
            }
        }

        /// <summary>
        /// This collection contains the values for the Rotor 4 combobox.
        /// </summary>
        [DontSave]
        public ObservableCollection<string> RotorBStrings
        {
            get => rotorBStrings;
            set
            {
                if (value != rotorBStrings)
                {
                    rotorBStrings = value;
                    OnPropertyChanged("RotorBStrings");
                }
            }
        }

        /// <summary>
        /// This collection contains the values for the Rotor 1-3 comboboxes.
        /// </summary>
        [DontSave]
        public ObservableCollection<string> ReflectorStrings
        {
            get => reflectorStrings;
            set
            {
                if (value != reflectorStrings)
                {
                    reflectorStrings = value;
                    OnPropertyChanged("ReflectorStrings");
                }
            }
        }

        #endregion

        #region Used ring settings

        [TaskPane("Ring1Caption", "Ring1Tooltip", "RingSettingsGroup", 23, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 26)]
        public int Ring1
        {
            get => ring1;
            set
            {
                if (value < ring1)
                {
                    if (value + 1 == ring1 && false) // TODO: always false
                    {
                        ring1 = value;
                        OnPropertyChanged("Ring1down");
                    }
                    else
                    {
                        ring1 = value;
                        OnPropertyChanged("Ring1NewValue");
                    }
                }
                if (value > ring1)
                {
                    if (value == ring1 + 1 && false) // TODO: always false
                    {
                        ring1 = value;
                        OnPropertyChanged("Ring1up");
                    }
                    else
                    {
                        ring1 = value;
                        OnPropertyChanged("Ring1NewValue");
                    }
                }
                OnPropertyChanged("Ring1");
            }
        }

        [TaskPane("Ring2Caption", "Ring2Tooltip", "RingSettingsGroup", 22, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 26)]
        public int Ring2
        {
            get => ring2;
            set
            {
                if (value < ring2)
                {
                    if (value + 1 == ring2 && false) // TODO: always false
                    {
                        ring2 = value;
                        OnPropertyChanged("Ring2down");
                    }
                    else
                    {
                        ring2 = value;
                        OnPropertyChanged("Ring2NewValue");

                    }

                }
                if (value > ring2)
                {
                    if (value == ring2 + 1 && false) // TODO: always false
                    {
                        ring2 = value;
                        OnPropertyChanged("Ring2up");
                    }
                    else
                    {
                        ring2 = value;
                        OnPropertyChanged("Ring2NewValue");
                    }
                }
                OnPropertyChanged("Ring2");
            }
        }

        [TaskPane("Ring3Caption", "Ring3Tooltip", "RingSettingsGroup", 21, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 26)]
        public int Ring3
        {
            get => ring3;
            set
            {
                if (value < ring3)
                {
                    if (value + 1 == ring3 && false) // TODO: always false
                    {
                        ring3 = value;
                        OnPropertyChanged("Ring3down");
                    }
                    else
                    {
                        ring3 = value;
                        OnPropertyChanged("Ring3NewValue");
                    }
                }
                if (value > ring3)
                {
                    if (value == ring3 + 1 && false) // TODO: always false
                    {
                        ring3 = value;
                        OnPropertyChanged("Ring3up");
                    }
                    else
                    {
                        ring3 = value;
                        OnPropertyChanged("Ring3NewValue");
                    }
                }
                OnPropertyChanged("Ring3");
            }
        }

        [TaskPane("Ring4Caption", "Ring4Tooltip", "RingSettingsGroup", 20, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 26)]
        public int Ring4
        {
            get => ring4;
            set
            {
                if (value < ring4)
                {
                    ring4 = value;
                    OnPropertyChanged("Ring4down");
                }
                if (value > ring4)
                {
                    ring4 = value;
                    OnPropertyChanged("Ring4up");
                }
            }
        }

        #endregion

        #region Plugboard settings

        [TaskPane("PlugBoardCaption", "PlugBoardTooltip", "PlugboardGroup", 30, false, ControlType.TextBoxReadOnly)]
        public string PlugBoard
        {
            get => plugBoard.ToString();
            set
            {
                plugBoard = new StringBuilder(value);
                OnPropertyChanged("PlugBoard");
            }
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
        [TaskPane("A=", "PlugBoardLetterTooltip", "PlugboardGroup", 40, false, ControlType.ComboBox,
            new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardA
        {
            get => alphabet.IndexOf(plugBoard[0]);
            set => setPlugBoard(0, value);
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
        [TaskPane("B=", "PlugBoardLetterTooltip", "PlugboardGroup", 41, false, ControlType.ComboBox,
            new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardB
        {
            get => alphabet.IndexOf(plugBoard[1]);
            set => setPlugBoard(1, value);
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Eins")]
        [TaskPane("C=", "PlugBoardLetterTooltip", "PlugboardGroup", 42, false, ControlType.ComboBox,
            new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardC
        {
            get => alphabet.IndexOf(plugBoard[2]);
            set => setPlugBoard(2, value);
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("D=", "PlugBoardLetterTooltip", "PlugboardGroup", 43, false, ControlType.ComboBox,
            new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardD
        {
            get => alphabet.IndexOf(plugBoard[3]);
            set => setPlugBoard(3, value);
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("E=", "PlugBoardLetterTooltip", "PlugboardGroup", 44, false, ControlType.ComboBox,
            new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardE
        {
            get => alphabet.IndexOf(plugBoard[4]);
            set => setPlugBoard(4, value);
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Zwei")]
        [TaskPane("F=", "PlugBoardLetterTooltip", "PlugboardGroup", 45, false, ControlType.ComboBox,
            new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardF
        {
            get => alphabet.IndexOf(plugBoard[5]);
            set => setPlugBoard(5, value);
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Drei")]
        [TaskPane("G=", "PlugBoardLetterTooltip", "PlugboardGroup", 46, false, ControlType.ComboBox,
            new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardG
        {
            get => alphabet.IndexOf(plugBoard[6]);
            set => setPlugBoard(6, value);
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Drei")]
        [TaskPane("H=", "PlugBoardLetterTooltip", "PlugboardGroup", 47, false, ControlType.ComboBox,
            new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardH
        {
            get => alphabet.IndexOf(plugBoard[7]);
            set => setPlugBoard(7, value);
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Drei")]
        [TaskPane("I=", "PlugBoardLetterTooltip", "PlugboardGroup", 48, false, ControlType.ComboBox,
            new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardI
        {
            get => alphabet.IndexOf(plugBoard[8]);
            set => setPlugBoard(8, value);
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Vier")]
        [TaskPane("J=", "PlugBoardLetterTooltip", "PlugboardGroup", 49, false, ControlType.ComboBox,
            new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardJ
        {
            get => alphabet.IndexOf(plugBoard[9]);
            set => setPlugBoard(9, value);
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Vier")]
        [TaskPane("K=", "PlugBoardLetterTooltip", "PlugboardGroup", 50, false, ControlType.ComboBox,
            new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardK
        {
            get => alphabet.IndexOf(plugBoard[10]);
            set => setPlugBoard(10, value);
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Vier")]
        [TaskPane("L=", "PlugBoardLetterTooltip", "PlugboardGroup", 51, false, ControlType.ComboBox,
            new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardL
        {
            get => alphabet.IndexOf(plugBoard[11]);
            set => setPlugBoard(11, value);
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Fuenf")]
        [TaskPane("M=", "PlugBoardLetterTooltip", "PlugboardGroup", 52, false, ControlType.ComboBox,
            new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardM
        {
            get => alphabet.IndexOf(plugBoard[12]);
            set => setPlugBoard(12, value);
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Fuenf")]
        [TaskPane("N=", "PlugBoardLetterTooltip", "PlugboardGroup", 53, false, ControlType.ComboBox,
            new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardN
        {
            get => alphabet.IndexOf(plugBoard[13]);
            set => setPlugBoard(13, value);
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Fuenf")]
        [TaskPane("O=", "PlugBoardLetterTooltip", "PlugboardGroup", 54, false, ControlType.ComboBox,
            new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardO
        {
            get => alphabet.IndexOf(plugBoard[14]);
            set => setPlugBoard(14, value);
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sechs")]
        [TaskPane("P=", "PlugBoardLetterTooltip", "PlugboardGroup", 55, false, ControlType.ComboBox,
            new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardP
        {
            get => alphabet.IndexOf(plugBoard[15]);
            set => setPlugBoard(15, value);
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sechs")]
        [TaskPane("Q=", "PlugBoardLetterTooltip", "PlugboardGroup", 56, false, ControlType.ComboBox,
            new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardQ
        {
            get => alphabet.IndexOf(plugBoard[16]);
            set => setPlugBoard(16, value);
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sechs")]
        [TaskPane("R=", "PlugBoardLetterTooltip", "PlugboardGroup", 57, false, ControlType.ComboBox,
            new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardR
        {
            get => alphabet.IndexOf(plugBoard[17]);
            set => setPlugBoard(17, value);
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sieben")]
        [TaskPane("S=", "PlugBoardLetterTooltip", "PlugboardGroup", 58, false, ControlType.ComboBox,
            new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardS
        {
            get => alphabet.IndexOf(plugBoard[18]);
            set => setPlugBoard(18, value);
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sieben")]
        [TaskPane("T=", "PlugBoardLetterTooltip", "PlugboardGroup", 59, false, ControlType.ComboBox,
            new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardT
        {
            get => alphabet.IndexOf(plugBoard[19]);
            set => setPlugBoard(19, value);
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Sieben")]
        [TaskPane("U=", "PlugBoardLetterTooltip", "PlugboardGroup", 60, false, ControlType.ComboBox,
            new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardU
        {
            get => alphabet.IndexOf(plugBoard[20]);
            set => setPlugBoard(20, value);
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Acht")]
        [TaskPane("V=", "PlugBoardLetterTooltip", "PlugboardGroup", 61, false, ControlType.ComboBox,
            new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardV
        {
            get => alphabet.IndexOf(plugBoard[21]);
            set => setPlugBoard(21, value);
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Acht")]
        [TaskPane("W=", "PlugBoardLetterTooltip", "PlugboardGroup", 62, false, ControlType.ComboBox,
            new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardW
        {
            get => alphabet.IndexOf(plugBoard[22]);
            set => setPlugBoard(22, value);
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Acht")]
        [TaskPane("X=", "PlugBoardLetterTooltip", "PlugboardGroup", 63, false, ControlType.ComboBox,
            new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardX
        {
            get => alphabet.IndexOf(plugBoard[23]);
            set => setPlugBoard(23, value);
        }

        [SettingsFormat(0, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Neun")]
        [TaskPane("Y=", "PlugBoardLetterTooltip", "PlugboardGroup", 64, false, ControlType.ComboBox,
            new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardY
        {
            get => alphabet.IndexOf(plugBoard[24]);
            set => setPlugBoard(24, value);
        }

        [SettingsFormat(1, "Normal", "Normal", "Black", "White", System.Windows.Controls.Orientation.Horizontal, "Auto", "*", "Neun")]
        [TaskPane("Z=", "PlugBoardLetterTooltip", "PlugboardGroup", 65, false, ControlType.ComboBox,
            new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" })]
        public int PlugBoardZ
        {
            get => alphabet.IndexOf(plugBoard[25]);
            set => setPlugBoard(25, value);
        }


        [TaskPane("ResetPlugboardCaption", "ResetPlugboardTooltip", "PlugboardGroup", 70, false, ControlType.Button)]
        public void ResetPlugboard()
        {
            plugBoard = new StringBuilder("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            foreach (char c in alphabet)
            {
                OnPropertyChanged("PlugBoard" + c);
            }

            OnPropertyChanged("PlugBoard");

            // Are the following needed? For the presentation? indeed
            //OnPropertyChanged("PlugBoardDisplay");
            //OnPropertyChanged("Remove all Plugs");
        }

        [TaskPane("PresentationSpeedCaption", "PresentationSpeedTooltip", "PresentationGroup", 71, true, ControlType.Slider, 2, 25)]
        public int PresentationSpeed
        {
            get => Presentation_Speed;
            set
            {
                if ((value) != Presentation_Speed)
                {
                    Presentation_Speed = value;
                    OnPropertyChanged("PresentationSpeed");
                }
            }
        }

        #endregion

        #region Text options

        [TaskPane("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip",
            "TextOptionsGroup", 3, false, ControlType.ComboBox,
            new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2", "UnknownSymbolHandlingList3" })]
        public int UnknownSymbolHandling
        {
            get => unknownSymbolHandling;
            set
            {
                if (value != unknownSymbolHandling)
                {
                    unknownSymbolHandling = value;
                    OnPropertyChanged("UnknownSymbolHandling");
                }
            }
        }

        [TaskPane("CaseHandlingCaption", "CaseHandlingTooltip",
            "TextOptionsGroup", 4, false, ControlType.ComboBox,
            new string[] { "CaseHandlingList1", "CaseHandlingList2", "CaseHandlingList3" })]
        public int CaseHandling
        {
            get => caseHandling;
            set
            {
                if (value != caseHandling)
                {
                    caseHandling = value;
                    OnPropertyChanged("CaseHandling");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Member

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}