/* HOWTO: Change year, author name and organization.
   Copyright 2010 Your Name, University of Duckburg

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

namespace CrypTool.Plugins.Purple
{
    [Author("Martin Jedrychowski, Martin Switek", "jedry@gmx.de, Martin_Switek@gmx.de", "Uni Duisburg-Essen", "http://www.uni-due.de")]
    public class PurpleSettings : ISettings
    {
        #region Private Variables

        private bool hasChanges = false;
        private int motion = 123;
        public int sechserPos = 1;
        public int zwanzigerPos1 = 1;
        public int zwanzigerPos2 = 1;
        public int zwanzigerPos3 = 1;
        private int unknownSymbolHandling = 2; // 0=ignore, 1=leave unmodified, 2=placeholder
        private int caseHandling = 0; // 0=preserve, 1=convert all to upper, 2= convert all to lower
        private int outputFormatting = 0; // format of output
        public string hardcodedAlphabet = "AEIOUYBCDFGHJKLMNPQRSTVWXZ";
        private string alphabet = "AEIOUYBCDFGHJKLMNPQRSTVWXZ";

        public enum actions { encrypt = 0, decrypt };
        private actions selectedAction = actions.encrypt;

        #endregion

        #region TaskPane Settings
        [PropertySaveOrder(1)]
        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [ContextMenu("ActionCaption", "ActionTooltip", 1, ContextMenuControlType.ComboBox, null, "EncryptCaption", "DecryptCaption")]
        [TaskPane("ActionCaption", "ActionTooltip", null, 1, true, ControlType.ComboBox, new string[] { "EncryptCaption", "DecryptCaption" })]
        public actions Action
        {
            get => selectedAction;
            set
            {
                if (value != selectedAction)
                {
                    HasChanges = true;
                }

                selectedAction = value;
                OnPropertyChanged("Action");

                //if (ReExecute != null) ReExecute();
            }
        }

        #region PlugBoard Setting
        [TaskPane("PlugBoardCaption", "PlugBoardTooltip", "PlugBoardGroup", 2, true, ControlType.TextBoxReadOnly)]
        public string PlugBoard
        {
            get => hardcodedAlphabet;
            set { }
        }

        [TaskPaneAttribute("AlphabetCaption", "AlphabetTooltip", "PlugBoardGroup", 3, true, ControlType.TextBox, ValidationType.RegEx, "^[A-Z]{26}$")]
        public string Alphabet
        {
            get => alphabet;
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (alphabet != value)
                {
                    alphabet = value;
                    hasChanges = true;
                }
            }
        }
        #endregion

        #region Position Setting
        [TaskPaneAttribute("PositionSixesCaption", "StartwertTooltip", "PositionGroup", 4, true, ControlType.TextBox, ValidationType.RegEx, "^[0-9]{1,2}$")]
        public int Sixes
        {
            get => sechserPos;
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (sechserPos != value)
                {
                    sechserPos = value;
                    hasChanges = true;
                }
            }
        }


        [TaskPaneAttribute("PositionTwentiesCaption1", "StartwertTooltip", "PositionGroup", 6, true, ControlType.TextBox, ValidationType.RegEx, "^[0-9]{1,2}$")]
        public int Twenties
        {
            get => zwanzigerPos1;
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (zwanzigerPos1 != value)
                {
                    zwanzigerPos1 = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPaneAttribute("PositionTwentiesCaption2", "StartwertTooltip", "PositionGroup", 7, true, ControlType.TextBox, ValidationType.RegEx, "^[0-9]{1,2}$")]
        public int Twenties2
        {
            get => zwanzigerPos2;
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (zwanzigerPos2 != value)
                {
                    zwanzigerPos2 = value;
                    hasChanges = true;
                }
            }
        }

        [TaskPaneAttribute("PositionTwentiesCaption3", "StartwertTooltip", "PositionGroup", 8, true, ControlType.TextBox, ValidationType.RegEx, "^[0-9]{1,2}$")]
        public int Twenties3
        {
            get => zwanzigerPos3;
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (zwanzigerPos3 != value)
                {
                    zwanzigerPos3 = value;
                    hasChanges = true;
                }
            }
        }


        #endregion

        #region Motion Setting
        [TaskPaneAttribute("MotionCaption", "MotionTooltip", "MotionCaption", 9, true, ControlType.TextBox, ValidationType.RegEx, "^[1-3]{3}$")]
        public int Motion
        {
            get => motion;
            set
            {
                // HOWTO: If a setting changes, you must set hasChanges manually to true.
                if (motion != value)
                {
                    motion = value;
                    hasChanges = true;
                }
            }
        }

        #endregion


        #endregion

        #region Text options

        [ContextMenu("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip", 10, ContextMenuControlType.ComboBox, null, new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2", "UnknownSymbolHandlingList3" })]
        [TaskPane("UnknownSymbolHandlingCaption", "UnknownSymbolHandlingTooltip", "TextOptionsGroup", 10, false, ControlType.ComboBox, new string[] { "UnknownSymbolHandlingList1", "UnknownSymbolHandlingList2", "UnknownSymbolHandlingList3" })]
        public int UnknownSymbolHandling
        {
            get => unknownSymbolHandling;
            set
            {
                if (value != unknownSymbolHandling)
                {
                    HasChanges = true;
                }

                unknownSymbolHandling = value;
                OnPropertyChanged("UnknownSymbolHandling");
            }
        }

        [ContextMenu("CaseHandlingCaption", "CaseHandlingTooltip", 11, ContextMenuControlType.ComboBox, null, new string[] { "CaseHandlingList1", "CaseHandlingList2", "CaseHandlingList3" })]
        [TaskPane("CaseHandlingCaption", "CaseHandlingTooltip", "TextOptionsGroup", 11, false, ControlType.ComboBox, new string[] { "CaseHandlingList1", "CaseHandlingList2", "CaseHandlingList3" })]
        public int CaseHandling
        {
            get => caseHandling;
            set
            {
                if (value != caseHandling)
                {
                    HasChanges = true;
                }

                caseHandling = value;
                OnPropertyChanged("CaseHandling");
            }
        }

        [ContextMenu("OutputFormattingCaption", "OutputFormattingTooltip", 11, ContextMenuControlType.ComboBox, null, new string[] { "OutputFormattingList1", "OutputFormattingList2", "OutputFormattingList3" })]
        [TaskPane("OutputFormattingCaption", "OutputFormattingTooltip", "TextOptionsGroup", 11, false, ControlType.ComboBox, new string[] { "OutputFormattingList1", "OutputFormattingList2", "OutputFormattingList3" })]
        public int OutputFormatting
        {
            get => outputFormatting;
            set
            {
                if (value != outputFormatting)
                {
                    HasChanges = true;
                }

                outputFormatting = value;
                OnPropertyChanged("OutputFormatting");
            }
        }

        #endregion

        #region ISettings Members

        /// <summary>
        /// HOWTO: This flags indicates whether some setting has been changed since the last save.
        /// If a property was changed, this becomes true, hence CrypTool will ask automatically if you want to save your changes.
        /// </summary>
        public bool HasChanges
        {
            get => hasChanges;
            set => hasChanges = value;
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
