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

using System;
using System.Collections.Generic;
using System.Text;
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
            get { return this.selectedAction; }
            set
            {
                if (value != selectedAction)
                {
                    this.selectedAction = value;
                    OnPropertyChanged("Action");                    
                }
            }
        }

        [PropertySaveOrder(3)]
        [TaskPane( "KeyCaption", "KeyTooltip",null,2,false,ControlType.TextBox,null)]
        public string KeyPhraseString
        {
            get 
            {
              if (keyPhraseString != null)
                return keyPhraseString.ToUpper();
              else
                return null;
            }
            set
            {
                if (value != null && value.ToUpper() != keyPhraseString)
                {
                    this.keyPhraseString = value.ToUpper();
                    setKeyMatrix();
                    OnPropertyChanged("KeyPhraseString");
                    OnPropertyChanged("KeyString");
                }
            }
        }

        [PropertySaveOrder(4)]
        [TaskPane( "AlphabetMatrixCaption", "AlphabetMatrixTooltip", null, 4, false, ControlType.TextBox, "")]
        public string KeyString
        {
            get { return this.keyString; }
            set
            {
                if (value != this.keyString)
                {
                    this.keyString = value;
                    OnPropertyChanged("KeyString");                    
                }
            }
        }

        [PropertySaveOrder(5)]
        [ContextMenu("PreFormatTextCaption", "PreFormatTextTooltip", 5, ContextMenuControlType.CheckBox, null, "PreFormatTextList1")]
        [TaskPane( "PreFormatTextCaption", "PreFormatTextTooltip",null,5,false,ControlType.CheckBox,"")]
        public bool PreFormatText
        {
            get { return this.preFormatText; }
            set
            {
                if (value != this.preFormatText)
                {
                    this.preFormatText = value;
                    OnPropertyChanged("PreFormatText");                    
                }
            }
        }

        [PropertySaveOrder(6)]
        [ContextMenu("IgnoreDuplicatesCaption", "IgnoreDuplicatesTooltip", 3, ContextMenuControlType.CheckBox, null, "IgnoreDuplicatesList1")]
        [TaskPane( "IgnoreDuplicatesCaption", "IgnoreDuplicatesTooltip",null,3,false,ControlType.CheckBox,"")]
        public bool IgnoreDuplicates
        {
            get { return this.ignoreDuplicates; }
            set
            {
                if (value != this.ignoreDuplicates)
                {
                    this.ignoreDuplicates = value;
                    OnPropertyChanged("IgnoreDuplicates");
                    setKeyMatrix();                    
                }
            }
        }

        [PropertySaveOrder(7)]
        [ContextMenu( "MatrixSizeCaption", "MatrixSizeTooltip",6,ContextMenuControlType.ComboBox,null,new string[]{"MatrixSizeList1", "MatrixSizeList2"})]
        [TaskPane( "MatrixSizeCaption", "MatrixSizeTooltip", null, 6,false, ControlType.ComboBox, "MatrixSizeList1", "MatrixSizeList2")]
        public PlayfairKey.MatrixSize MatrixSize
        {
            get { return this.matrixSize; }
            set 
            {
                if (value != this.matrixSize)
                {
                    this.matrixSize = value;
                    setKeyMatrix();
                    OnPropertyChanged("MatrixSize");                    
                }
            }
        }

        [PropertySaveOrder(8)]
        [ContextMenu("SeparatePairsCaption", "SeparatePairsTooltip", 7, ContextMenuControlType.CheckBox, null, "SeparatePairsList1")]
        [TaskPane( "SeparatePairsTPCaption", "SeparatePairsTPTooltip", null, 7, false, ControlType.CheckBox, "")]
        public bool SeparatePairs
        {
            get { return this.separatePairs; }
            set
            {
                if (value != this.separatePairs)
                {
                    this.separatePairs = value;
                    OnPropertyChanged("SeparatePairs");                    
                }
            }
        }

        [PropertySaveOrder(9)]        
        [TaskPane( "SeparatorCaption", "SeparatorTooltip",null,8,false, ControlType.TextBox,"")]
        public char Separator
        {
            get { return char.ToUpper(this.separator); }
            set 
            {
                if (char.ToUpper(value) != this.separator)
                {
                    this.separator = char.ToUpper(value);
                    setSeparatorReplacement();
                    OnPropertyChanged("Separator");
                    OnPropertyChanged("SeparatorReplacement");                    
                }
            }
        }

        [PropertySaveOrder(10)]
        [TaskPane( "SeparatorReplacementCaption", "SeparatorReplacementTooltip", null, 9, false, ControlType.TextBox, "")]
        public char SeparatorReplacement
        {
            get { return char.ToUpper(this.separatorReplacement);}
            set
            {
                if (char.ToUpper(value) != this.separatorReplacement)
                {
                    this.separatorReplacement = char.ToUpper(value);
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
            if (this.separator == this.separatorReplacement)
            {
                int separatorReplacementPos = KeyString.IndexOf(this.separatorReplacement);
                int separatorPos = (separatorReplacementPos - 1 + KeyString.Length) % KeyString.Length;
                this.separator = KeyString[separatorPos];
            }
        }

        private void setSeparatorReplacement()
        {
            if (this.separator == this.separatorReplacement)
            {
                int separatorPos = KeyString.IndexOf(this.separator);
                int separatorReplacementPos = (separatorPos + 1) % KeyString.Length;
                this.separatorReplacement = KeyString[separatorReplacementPos];
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
