/*
   Copyright 2008 Sebastian Przybylski, University of Siegen
   2022: Added Beaufort and Beaufort Autokey, Nils Kopal, CrypTool project

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
using System.ComponentModel;
using System.Linq;

namespace CrypTool.Vigenere
{
    public class VigenereSettings : ISettings
    {
        #region Public Vigenere specific interface

        /// <summary>
        /// We use this delegate to send log messages from the settings class to the Vigenere plugin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        /// <param name="logLevel"></param>
        public delegate void VigenereLogMessage(string msg, NotificationLevel logLevel);

        /// <summary>
        /// An enumaration for the different modes of dealing with unknown characters
        /// </summary>
        public enum UnknownSymbolHandlingMode { Ignore = 0, Remove = 1, Replace = 2 };

        /// <summary>
        /// Fire if a new text has to be shown in the status bar
        /// </summary>
        public event VigenereLogMessage LogMessage;

        /// <summary>
        /// Retrieves the current shift values of Vigenere (i.e. the key), or sets it
        /// </summary>
        [PropertySaveOrder(0)]
        public int[] ShiftKey
        {
            get => keyShiftValues;
            set => setKeyByValue(value);
        }

        /// <summary>
        /// Retrieves the current setting whether the alphabet should be treated as case sensitive or not
        /// </summary>
        [PropertySaveOrder(1)]
        public bool CaseSensitiveAlphabet
        {
            get => caseSensitiveAlphabet;
            set { } //readonly
        }

        #endregion

        #region Private variables
        private int selectedAction = 0;
        private int _selectedMode = 0;
        private readonly string upperAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private readonly string lowerAlphabet = "abcdefghijklmnopqrstuvwxyz";
        private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private char[] keyChars = { 'B', 'C', 'D' };
        private int[] keyShiftValues = { 1, 2, 3 };
        private UnknownSymbolHandlingMode unknowSymbolHandling = UnknownSymbolHandlingMode.Ignore;
        private bool caseSensitiveAlphabet = false;
        private bool memorizeCase = false;
        #endregion

        #region Private methods
        private string removeEqualChars(string value)
        {
            int length = value.Length;

            for (int i = 0; i < length; i++)
            {
                for (int j = i + 1; j < length; j++)
                {
                    if ((value[i]) == value[j] || (!CaseSensitiveAlphabet & (char.ToUpper(value[i]) == char.ToUpper(value[j]))))
                    {
                        LogMessage("Removing duplicate letter: \'" + value[j] + "\' from alphabet!", NotificationLevel.Warning);
                        value = value.Remove(j, 1);
                        j--;
                        length--;
                    }
                }
            }
            return value;
        }

        /// <summary>
        /// Parse the offset string and set the shiftValue/ShiftChar accordingly
        /// </summary>
        /// <param name="offsetString"></param>
        private void setKeyByValue(string offsetString)
        {
            if (string.IsNullOrEmpty(offsetString))
            {
                return;
            }
            try
            {
                int[] offset = offsetString.Split(',').Select(s => int.Parse(s)).ToArray();
                setKeyByValue(offset);
            }
            catch (Exception e)
            {
                LogMessage("Bad input \"" + offsetString + "\"! (" + e.Message + ") Reverting to " + intArrayToString(keyShiftValues) + "!", NotificationLevel.Error);
                OnPropertyChanged("ShiftValue");
            }
        }

        /// <summary>
        /// Set the new shiftValue and the new shiftCharacter to offset % alphabet.Length
        /// </summary>
        /// <param name="offset"></param>
        public void setKeyByValue(int[] offset)
        {
            if (offset == null)
            {
                return;
            }
            try
            {
                //set the new shiftValue and shiftChar
                keyShiftValues = offset.Select(o => o % alphabet.Length).ToArray();
                keyChars = keyShiftValues.Select(o => alphabet[o]).ToArray();

                //Anounnce this to the settings pane
                OnPropertyChanged("ShiftValue");
                OnPropertyChanged("ShiftChar");

                //print some info in the log.
                LogMessage(
                    "Accepted new shift values " + intArrayToString(keyShiftValues) + "! (Adjusted key to '" +
                    charArrayToString(keyChars) + ")'", NotificationLevel.Info);
            }
            catch (Exception)
            {
                LogMessage("Bad shift value \"" + intArrayToString(keyShiftValues) + "\"!", NotificationLevel.Error);
            }
        }

        private void setKeyByCharacter(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            try
            {
                int[] offset = new int[value.Length];

                if (CaseSensitiveAlphabet)
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        offset[i] = alphabet.IndexOf(value[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        offset[i] = alphabet.ToUpper().IndexOf(char.ToUpper(value[i]));
                    }
                }

                for (int i = 0; i < offset.Length; i++)
                {
                    if (offset[i] >= 0)
                    {
                        keyShiftValues = new int[offset.Length];
                        for (int j = 0; j < offset.Length; j++)
                        {
                            keyShiftValues[j] = offset[j];
                        }

                        keyChars = new char[keyShiftValues.Length];
                        for (int j = 0; j < keyShiftValues.Length; j++)
                        {
                            keyChars[j] = alphabet[keyShiftValues[j]];
                        }

                        LogMessage("Accepted key \'" + charArrayToString(keyChars) + "\'! (Adjusted shift values to " + intArrayToString(keyShiftValues) + ")", NotificationLevel.Info);
                        OnPropertyChanged("ShiftValue");
                        OnPropertyChanged("ShiftChar");
                        break;
                    }
                    else
                    {
                        LogMessage("Bad input \"" + value + "\"! (Some character not in alphabet!) Reverting to " + charArrayToString(keyChars) + "!", NotificationLevel.Error);
                    }
                }
            }
            catch (Exception e)
            {
                LogMessage("Bad input \"" + value + "\"! (" + e.Message + ") Reverting to " + charArrayToString(keyChars) + "!", NotificationLevel.Error);
            }
        }

        private string charArrayToString(char[] chars)
        {
            return new string(chars);
        }

        private string intArrayToString(int[] ints)
        {
            return string.Join(",", ints);
        }
        #endregion

        #region Algorithm settings properties (visible in the Settings pane)

        [PropertySaveOrder(3)]
        [TaskPane("ModeCaption", "ModeTooltip", null, 1, false, ControlType.ComboBox, new string[] { "ModeList1", "ModeList2" , "ModeList3", "ModeList4" })]
        public int Mode
        {
            get => _selectedMode;
            set
            {
                if (value != _selectedMode)
                {
                    _selectedMode = value;
                    OnPropertyChanged("Mode");
                }
            }
        }

        [PropertySaveOrder(4)]
        [TaskPane("ActionCaption", "ActionTooltip", null, 2, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public int Action
        {
            get => selectedAction;
            set
            {
                if (value != selectedAction)
                {
                    selectedAction = value;
                    OnPropertyChanged("Action");
                }
            }
        }

        [PropertySaveOrder(5)]
        [TaskPane("ShiftValueTPCaption", "ShiftValueTPTooltip", null, 3, false, ControlType.TextBox, null)]
        public string ShiftValue
        {
            get => intArrayToString(keyShiftValues);
            set => setKeyByValue(value);
        }

        [PropertySaveOrder(6)]
        [TaskPane("ShiftCharCaption", "ShiftCharTooltip", null, 4, false, ControlType.TextBox, null)]
        public string ShiftChar
        {
            get => new string(keyChars);
            set => setKeyByCharacter(value);
        }

        //[SettingsFormat(0, "Normal", "Normal", "Black", "White", Orientation.Vertical)]
        [PropertySaveOrder(7)]
        [TaskPane("AlphabetSymbolsCaption", "AlphabetSymbolsTooltip", "AlphabetGroup", 5, false, ControlType.TextBox, null)]
        public string AlphabetSymbols
        {
            get => alphabet;
            set
            {
                string a = removeEqualChars(value);
                if (a.Length == 0) //cannot accept empty alphabets
                {
                    LogMessage("Ignoring empty alphabet from user! Using previous alphabet: \"" + alphabet + "\" (" + alphabet.Length.ToString() + " Symbols)", NotificationLevel.Info);
                }
                else if (!alphabet.Equals(a))
                {
                    alphabet = a;
                    setKeyByValue(keyShiftValues); //re-evaluate if the shiftvalue is still within the range
                    LogMessage("Accepted new alphabet from user: \"" + alphabet + "\" (" + alphabet.Length.ToString() + " Symbols)", NotificationLevel.Info);
                    OnPropertyChanged("AlphabetSymbols");
                }
            }
        }

        [PropertySaveOrder(8)]
        [TaskPane("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip", "AlphabetGroup", 6, false, ControlType.ComboBox, new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2", "UnknownSymbolHandlingList3" })]
        public int UnknownSymbolHandling
        {
            get => (int)unknowSymbolHandling;
            set
            {
                if ((UnknownSymbolHandlingMode)value != unknowSymbolHandling)
                {
                    unknowSymbolHandling = (UnknownSymbolHandlingMode)value;
                    OnPropertyChanged("UnknownSymbolHandling");
                }
            }
        }

        [PropertySaveOrder(9)]
        [TaskPane("AlphabetCaseCaption", "AlphabetCaseTooltip", "AlphabetGroup", 7, false, ControlType.CheckBox)]
        public bool AlphabetCase
        {
            get => caseSensitiveAlphabet;
            set
            {
                if (value != caseSensitiveAlphabet)
                {
                    caseSensitiveAlphabet = value;
                    if (!caseSensitiveAlphabet)
                    {
                        if (alphabet == (upperAlphabet + lowerAlphabet))
                        {
                            alphabet = upperAlphabet;
                            LogMessage("Changing alphabet to: \"" + alphabet + "\" (" + alphabet.Length.ToString() + "Symbols)", NotificationLevel.Info);
                            OnPropertyChanged("AlphabetSymbols");
                            //re-set also the key (shifvalue/shiftchar to be in the range of the new alphabet
                            setKeyByValue(keyShiftValues);
                        }
                    }
                    else
                    {
                        if (alphabet == upperAlphabet)
                        {
                            alphabet = upperAlphabet + lowerAlphabet;
                            LogMessage("Changing alphabet to: \"" + alphabet + "\" (" + alphabet.Length.ToString() + " Symbols)", NotificationLevel.Info);
                            OnPropertyChanged("AlphabetSymbols");
                        }
                    }

                    //remove equal characters from the current alphabet
                    string a = alphabet;
                    alphabet = removeEqualChars(alphabet);

                    if (a != alphabet)
                    {
                        OnPropertyChanged("AlphabetSymbols");
                        LogMessage("Changing alphabet to: \"" + alphabet + "\" (" + alphabet.Length.ToString() + " Symbols)", NotificationLevel.Info);
                    }
                    OnPropertyChanged("CaseSensitiveAlphabet");
                }
            }
        }

        [PropertySaveOrder(10)]
        [TaskPane("MemorizeCaseCaption", "MemorizeCaseTooltip", "AlphabetGroup", 8, false, ControlType.CheckBox)]
        public bool MemorizeCase
        {
            get => memorizeCase;
            set
            {
                memorizeCase = AlphabetCase ? false : value;
                OnPropertyChanged("MemorizeCase");
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}