/*
   Copyright CrypTool 2 Team <ct2contact@CrypTool.org>

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
using System.ComponentModel;
using System.Windows;

namespace TextSteganography
{
    public enum ModeType { ZeroWidthSpace, CapitalLettersBinary, CapitalLettersText, MarkingLettersBinary, MarkingLettersText }
    public enum ActionType { Hide, Extract }
    public enum MarkingType { DotUnder, DotAbove }
    public class TextSteganographySettings : ISettings
    {
        #region Private Variables

        private ActionType action = ActionType.Hide;
        private ModeType mode = ModeType.ZeroWidthSpace;
        private MarkingType marking = MarkingType.DotUnder;
        private int offset = 0;
        #endregion

        #region TaskPane Settings

        [TaskPane("ActionCaption", "ActionTooltip", null, 0, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public ActionType Action
        {
            get => action;
            set
            {
                if (action != value)
                {
                    action = value;
                    OnPropertyChanged("Action");
                }
            }
        }

        [TaskPane("ModeCaption", "ModeTooltip", null, 1, false, ControlType.ComboBox, new string[] { "Zero Width Space", "Capital Letters Binary", "Capital Letters Text", "Marking Letters Binary", "Marking Letters Text" })]
        public ModeType Mode
        {
            get => mode;
            set
            {
                if (mode != value)
                {
                    mode = value;
                    if (mode == ModeType.ZeroWidthSpace)
                    {
                        ShowSettingsElement("Offset");
                    }
                    else
                    {
                        HideSettingsElement("Offset");
                    }
                    if (mode == ModeType.MarkingLettersBinary || mode == ModeType.MarkingLettersText)
                    {
                        ShowSettingsElement("Marking");
                    }
                    else
                    {
                        HideSettingsElement("Marking");
                    }
                    OnPropertyChanged("Mode");
                }
            }
        }

        [TaskPane("MarkingCaption", "MarkingTooltip", null, 2, false, ControlType.ComboBox, new string[] { "DotUnder", "DotAbove" })]
        public MarkingType Marking
        {
            get => marking;
            set
            {
                if (marking != value)
                {
                    marking = value;
                    OnPropertyChanged("Marking");
                }
            }
        }

        [TaskPane("Offset", "OffsetTooltip", null, 2, false, ControlType.TextBox)]
        public int Offset
        {
            get => offset;
            set
            {
                if (offset != value)
                {
                    offset = value;
                    OnPropertyChanged("Offset");
                }
            }
        }

        #endregion

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

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion

        public void Initialize()
        {
            // if template or saved workplace is used, display necessary settings elements and hide elements that are not relevant
            if (mode == ModeType.MarkingLettersBinary || mode == ModeType.MarkingLettersText)
            {
                ShowSettingsElement("Marking");
                HideSettingsElement("Offset");
            }
            else if (mode == ModeType.ZeroWidthSpace)
            {
                ShowSettingsElement("Offset");
                HideSettingsElement("Marking");
            }
            else
            {
                HideSettingsElement("Marking");
                HideSettingsElement("Offset");
            }
        }
    }
}
