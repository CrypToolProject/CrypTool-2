﻿/*                              
   Copyright 2009-2012 Arno Wacker, University of Kassel

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

namespace CrypTool.Caesar
{
    public class CaesarSettings : ISettings
    {
        #region Public Caesar specific interface

        /// <summary>
        /// We use this delegate to send log messages from the settings class to the Caesar plugin
        /// </summary>
        public delegate void CaesarLogMessage(string msg, NotificationLevel loglevel);

        public enum CaesarMode { Encrypt = 0, Decrypt = 1 };

        /// <summary>
        /// An enumaration for the different modes of dealing with unknown characters
        /// </summary>
        public enum UnknownSymbolHandlingMode { Ignore = 0, Remove = 1, Replace = 2 };


        /// <summary>
        /// Feuern, wenn ein neuer Text im Statusbar angezeigt werden soll.
        /// </summary>
        public event CaesarLogMessage LogMessage;

        #endregion

        #region Private variables and public constructor

        private CaesarMode selectedAction = CaesarMode.Encrypt;
        private readonly string upperAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private readonly string lowerAlphabet = "abcdefghijklmnopqrstuvwxyz";
        private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private int shiftValue = 3;
        //private string shiftString;
        private UnknownSymbolHandlingMode unknownSymbolHandling = UnknownSymbolHandlingMode.Ignore;
        private bool caseSensitiveSensitive = false;
        private bool memorizeCase = false;

        public CaesarSettings()
        {
            SetKeyByValue(shiftValue);
        }

        #endregion

        #region Private methods

        private void OnLogMessage(string msg, NotificationLevel level)
        {
            if (LogMessage != null)
            {
                LogMessage(msg, level);
            }
        }

        private string removeEqualChars(string value)
        {
            int length = value.Length;

            for (int i = 0; i < length; i++)
            {
                for (int j = i + 1; j < length; j++)
                {
                    if ((value[i] == value[j]) || (!CaseSensitive & (char.ToUpper(value[i]) == char.ToUpper(value[j]))))
                    {
                        OnLogMessage("Removing duplicate letter: \'" + value[j] + "\' from alphabet!", NotificationLevel.Warning);
                        value = value.Remove(j, 1);
                        j--;
                        length--;
                    }
                }
            }

            return value;
        }

        /// <summary>
        /// Set the new shiftValue and the new shiftString to offset % alphabet.Length
        /// </summary>
        public void SetKeyByValue(int offset, bool firePropertyChangesShiftKey = true/*, bool firePropertyChangesShiftString = true*/)
        {
            // making sure the shift value lies within the alphabet range      
            shiftValue = ((offset % alphabet.Length) + alphabet.Length) % alphabet.Length;
            //shiftString = "A → " + alphabet[shiftValue];

            // Anounnce this to the settings pane
            if (firePropertyChangesShiftKey)
            {
                OnPropertyChanged(nameof(ShiftKey));                
            }
            /*if(firePropertyChangesShiftString)
            {
                OnPropertyChanged(nameof(ShiftString));
            }*/
        }

        #endregion

        #region Algorithm settings properties (visible in the Settings pane)

        [PropertySaveOrder(4)]
        [TaskPane("ActionTPCaption", "ActionTPTooltip", null, 1, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public CaesarMode Action
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
        [TaskPane("ShiftValueCaption", "ShiftValueTooltip", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, -1, 100)]
        public int ShiftKey
        {
            get => shiftValue;
            set => SetKeyByValue(value);
        }

        /*[PropertySaveOrder(6)]
        [TaskPane("ShiftStringCaption", "ShiftStringTooltip", null, 3, false, ControlType.TextBoxReadOnly)]
        public string ShiftString => shiftString;*/

        //[SettingsFormat(0, "Normal", "Normal", "Black", "White", Orientation.Vertical)]
        [PropertySaveOrder(7)]
        [TaskPane("AlphabetSymbolsCaption", "AlphabetSymbolsTooltip", "AlphabetGroup", 4, false, ControlType.TextBox, "")]
        public string AlphabetSymbols
        {
            get => alphabet;
            set
            {
                string a = removeEqualChars(value);
                if (a.Length == 0) // cannot accept empty alphabets
                {
                    OnLogMessage("Ignoring empty alphabet from user! Using previous alphabet: \"" + alphabet + "\" (" + alphabet.Length.ToString() + " Symbols)", NotificationLevel.Info);
                }
                else if (!alphabet.Equals(a))
                {
                    alphabet = a;
                    SetKeyByValue(shiftValue); //re-evaluate if the shiftvalue is still within the range
                    OnPropertyChanged(nameof(AlphabetSymbols));
                }
            }
        }

        [PropertySaveOrder(8)]
        [TaskPane("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip", "AlphabetGroup", 5, false, ControlType.ComboBox, new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2", "UnknownSymbolHandlingList3" })]
        public UnknownSymbolHandlingMode UnknownSymbolHandling
        {
            get => unknownSymbolHandling;
            set
            {
                if (value != unknownSymbolHandling)
                {
                    unknownSymbolHandling = value;
                    OnPropertyChanged(nameof(UnknownSymbolHandling));
                }
            }
        }

        /// <summary>
        /// Visible setting how to deal with alphabet case. false = case insensitive, true = case sensitive
        /// </summary
        [PropertySaveOrder(9)]
        [TaskPane("AlphabetCaseCaption", "AlphabetCaseTooltip", "AlphabetGroup", 6, false, ControlType.CheckBox)]
        public bool CaseSensitive
        {
            get => caseSensitiveSensitive;
            set
            {
                if (value == caseSensitiveSensitive)
                {
                    return;
                }

                if (value == true)
                {
                    memorizeCase = false;
                    OnPropertyChanged(nameof(MemorizeCase));
                }

                caseSensitiveSensitive = value;
                if (value)
                {
                    if (alphabet == upperAlphabet)
                    {
                        alphabet = upperAlphabet + lowerAlphabet;
                        OnPropertyChanged(nameof(AlphabetSymbols));
                    }
                }
                else
                {
                    if (alphabet == (upperAlphabet + lowerAlphabet))
                    {
                        alphabet = upperAlphabet;
                        OnPropertyChanged(nameof(AlphabetSymbols));
                        // re-set also the key (shiftvalue/shiftString to be in the range of the new alphabet
                        SetKeyByValue(shiftValue);
                    }
                }

                // remove equal characters from the current alphabet
                string a = alphabet;
                alphabet = removeEqualChars(alphabet);

                if (a != alphabet)
                {
                    OnPropertyChanged(nameof(AlphabetSymbols));
                }

                OnPropertyChanged(nameof(CaseSensitive));
            }
        }

        [PropertySaveOrder(10)]
        [TaskPane("MemorizeCaseCaption", "MemorizeCaseTooltip", "AlphabetGroup", 7, false, ControlType.CheckBox)]
        public bool MemorizeCase
        {
            get => memorizeCase;
            set
            {
                bool newMemorizeCase = CaseSensitive ? false : value;
                if (newMemorizeCase != memorizeCase)
                {
                    memorizeCase = newMemorizeCase;
                    OnPropertyChanged(nameof(MemorizeCase));
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
