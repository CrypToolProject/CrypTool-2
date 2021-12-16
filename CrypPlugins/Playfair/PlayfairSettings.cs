/*
   Copyright 2008 Sebastian Przybylski, University of Siegen

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

namespace CrypTool.Playfair
{
    public class PlayfairSettings : ISettings
    {
        #region Public Playfair specific interface

        /// <summary>
        /// We use this delegate to send log messages from the settings class to the Playfair plugin
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="logLevel"></param>
        public delegate void PlayfairLogMessage(string msg, NotificationLevel logLevel);

        #endregion

        #region Private variables

        private bool separatePairs = true;
        private int selectedAction = 0;
        private bool preFormatText = true;
        private bool ignoreDuplicates = true;
        private PlayfairKey.MatrixSize matrixSize = PlayfairKey.MatrixSize.Five_Five;
        private string keyString = PlayfairKey.SmallAlphabet;
        private string keyPhraseString;
        private char separator = 'X';
        private char separatorReplacement = 'Y';

        #endregion

        #region Algorithm settings properties (visible in the Settings pane)

        [PropertySaveOrder(2)]
        [ContextMenu("ActionCaption", "ActionTooltip", 1, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, "ActionList1", "ActionList2")]
        [TaskPane("ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
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

        [PropertySaveOrder(3)]
        [TaskPane("KeyCaption", "KeyTooltip", null, 2, false, ControlType.TextBox, null)]
        public string KeyPhraseString
        {
            get
            {
                if (keyPhraseString != null)
                {
                    return keyPhraseString.ToUpper();
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value != null && value.ToUpper() != keyPhraseString)
                {
                    keyPhraseString = value.ToUpper();
                    setKeyMatrix();
                    OnPropertyChanged("KeyPhraseString");
                    OnPropertyChanged("KeyString");
                }
            }
        }

        [PropertySaveOrder(4)]
        [TaskPane("AlphabetMatrixCaption", "AlphabetMatrixTooltip", null, 4, false, ControlType.TextBox, "")]
        public string KeyString
        {
            get => keyString;
            set
            {
                if (value != keyString)
                {
                    keyString = value;
                    OnPropertyChanged("KeyString");
                }
            }
        }

        [PropertySaveOrder(5)]
        [ContextMenu("PreFormatTextCaption", "PreFormatTextTooltip", 5, ContextMenuControlType.CheckBox, null, "PreFormatTextList1")]
        [TaskPane("PreFormatTextCaption", "PreFormatTextTooltip", null, 5, false, ControlType.CheckBox, "")]
        public bool PreFormatText
        {
            get => preFormatText;
            set
            {
                if (value != preFormatText)
                {
                    preFormatText = value;
                    OnPropertyChanged("PreFormatText");
                }
            }
        }

        [PropertySaveOrder(6)]
        [ContextMenu("IgnoreDuplicatesCaption", "IgnoreDuplicatesTooltip", 3, ContextMenuControlType.CheckBox, null, "IgnoreDuplicatesList1")]
        [TaskPane("IgnoreDuplicatesCaption", "IgnoreDuplicatesTooltip", null, 3, false, ControlType.CheckBox, "")]
        public bool IgnoreDuplicates
        {
            get => ignoreDuplicates;
            set
            {
                if (value != ignoreDuplicates)
                {
                    ignoreDuplicates = value;
                    OnPropertyChanged("IgnoreDuplicates");
                    setKeyMatrix();
                }
            }
        }

        [PropertySaveOrder(7)]
        [ContextMenu("MatrixSizeCaption", "MatrixSizeTooltip", 6, ContextMenuControlType.ComboBox, null, new string[] { "MatrixSizeList1", "MatrixSizeList2" })]
        [TaskPane("MatrixSizeCaption", "MatrixSizeTooltip", null, 6, false, ControlType.ComboBox, "MatrixSizeList1", "MatrixSizeList2")]
        public PlayfairKey.MatrixSize MatrixSize
        {
            get => matrixSize;
            set
            {
                if (value != matrixSize)
                {
                    matrixSize = value;
                    setKeyMatrix();
                    OnPropertyChanged("MatrixSize");
                }
            }
        }

        [PropertySaveOrder(8)]
        [ContextMenu("SeparatePairsCaption", "SeparatePairsTooltip", 7, ContextMenuControlType.CheckBox, null, "SeparatePairsList1")]
        [TaskPane("SeparatePairsTPCaption", "SeparatePairsTPTooltip", null, 7, false, ControlType.CheckBox, "")]
        public bool SeparatePairs
        {
            get => separatePairs;
            set
            {
                if (value != separatePairs)
                {
                    separatePairs = value;
                    OnPropertyChanged("SeparatePairs");
                }
            }
        }

        [PropertySaveOrder(9)]
        [TaskPane("SeparatorCaption", "SeparatorTooltip", null, 8, false, ControlType.TextBox, "")]
        public char Separator
        {
            get => char.ToUpper(separator);
            set
            {
                if (char.ToUpper(value) != separator)
                {
                    separator = char.ToUpper(value);
                    setSeparatorReplacement();
                    OnPropertyChanged("Separator");
                    OnPropertyChanged("SeparatorReplacement");
                }
            }
        }

        [PropertySaveOrder(10)]
        [TaskPane("SeparatorReplacementCaption", "SeparatorReplacementTooltip", null, 9, false, ControlType.TextBox, "")]
        public char SeparatorReplacement
        {
            get => char.ToUpper(separatorReplacement);
            set
            {
                if (char.ToUpper(value) != separatorReplacement)
                {
                    separatorReplacement = char.ToUpper(value);
                    setSeparator();
                    OnPropertyChanged("Separator");
                    OnPropertyChanged("SeparatorReplacement");
                }
            }
        }

        #endregion

        #region Private Members

        private void setKeyMatrix()
        {
            KeyString = PlayfairKey.CreateKey(KeyPhraseString, MatrixSize, IgnoreDuplicates);
        }

        private void setSeparator()
        {
            if (separator == separatorReplacement)
            {
                int separatorReplacementPos = KeyString.IndexOf(separatorReplacement);
                int separatorPos = (separatorReplacementPos - 1 + KeyString.Length) % KeyString.Length;
                separator = KeyString[separatorPos];
            }
        }

        private void setSeparatorReplacement()
        {
            if (separator == separatorReplacement)
            {
                int separatorPos = KeyString.IndexOf(separator);
                int separatorReplacementPos = (separatorPos + 1) % KeyString.Length;
                separatorReplacement = KeyString[separatorReplacementPos];
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
