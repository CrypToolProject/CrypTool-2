/*
   Copyright 2009 Sören Rinne, Ruhr-Universität Bochum, Germany

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
// for Visibility
using System.Windows;

namespace CrypTool.CLK
{
    public class CLKSettings : ISettings
    {
        #region private variables
        private int setClockToTrue = 1;
        private bool useEvent = false;
        private int clkTimeout = 2000;
        #endregion private variables

        #region ISettings Members

        [ContextMenu("SetClockToTrueCaption", "SetClockToTrueTooltip", 0, ContextMenuControlType.ComboBox, null, new string[] { "SetClockToTrueList1", "SetClockToTrueList2" })]
        [TaskPaneAttribute("SetClockToTrueCaption", "SetClockToTrueTooltip", "", 0, true, ControlType.RadioButton, new string[] { "SetClockToTrueList1", "SetClockToTrueList2" })]
        public int SetClockToTrue
        {
            get => setClockToTrue;
            set
            {
                setClockToTrue = value;
                OnPropertyChanged("SetClockToTrue");
            }
        }
        /*[ContextMenu("Set clock to...", " true / false ", 2, ContextMenuControlType.ComboBox, null, new string[] { "true", "false"})]
        [TaskPane("Set clock to...", " true / false ", null, 2, false, ControlType.RadioButton, new string[] { "true", "false" })]
        public bool SetClockToTrue
        {
            get
            {
                return (this.setClockToTrue == "true");
            }
            set
            {
                //if (this.setClockToTrue != setClockToTrue)
                {
                    this.setClockToTrue = value;
                    OnPropertyChanged("SetClockToTrue");
                }
            }
        }*/

        [TaskPaneAttribute("CLKTimeoutCaption", "CLKTimeoutTooltip", "", 1, false, ControlType.TextBox, null)]
        public int CLKTimeout
        {
            get => clkTimeout;
            set
            {
                clkTimeout = value;
                OnPropertyChanged("CLKTimeout");
            }

        }

        [ContextMenu("UseEventCaption", "UseEventTooltip", 0, ContextMenuControlType.CheckBox, null, "UseEventList1")]
        [TaskPaneAttribute("UseEventCaption", "UseEventTooltip", "", 0, true, ControlType.CheckBox, null)]
        public bool UseEvent
        {
            get => useEvent;
            set
            {
                useEvent = value;
                OnPropertyChanged("UseEvent");
                if (useEvent)
                {
                    SettingChanged("CLKTimeout", Visibility.Collapsed);
                }
                else
                {
                    SettingChanged("CLKTimeout", Visibility.Visible);
                }
            }

        }

        private int rounds = 10; //how many bits will be generated
                                 //[ContextMenu("Rounds", "How many bits shall be generated?", 1, ContextMenuControlType.ComboBox, new int[] { 10, 50, 100 }, "10 bits", "50 bits", "100 bits")]
                                 //[TaskPane("Rounds", "How many bits shall be generated?", null, 1, false, ControlType.TextBox)]
        [TaskPane("RoundsCaption", "RoundsTooltip", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int Rounds
        {
            get => rounds;
            set
            {
                /*if (value <= 0)
                    this.rounds = 1;
                else*/
                rounds = value;
                OnPropertyChanged("Rounds");
            }
        }

        internal void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
            {
                return;
            }

            if (useEvent)
            {
                SettingChanged("CLKTimeout", Visibility.Collapsed);
            }
            else
            {
                SettingChanged("CLKTimeout", Visibility.Visible);
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        // this event is for disabling stuff in the settings pane
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
            }
        }

        /* SettingChanged(MEM_USAGE_PROPERTY, Visibility.Visible);
        SettingChanged(BUTTON_MEM_USAGE_PROPERTY, Visibility.Visible, new TaskPaneAttribute(Properties.Visuals.settingMemUsageOff,
            Properties.Visuals.settingMemUsageOff_ToolTip, Properties.Visuals.settingGroupMisc, 5, false, ControlType.Button));
         */

        // these 2 functions are for disabling stuff in the settings pane
        private void SettingChanged(string setting, Visibility vis)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(setting, vis)));
            }
        }

        private void SettingChanged(string setting, Visibility vis, TaskPaneAttribute tpa)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(setting, vis, tpa)));
            }
        }

        #endregion
    }
}
