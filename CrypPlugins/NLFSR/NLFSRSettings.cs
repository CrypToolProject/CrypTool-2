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

namespace CrypTool.NLFSR
{
    public class NLFSRSettings : ISettings
    {
        #region ISettings Members

        private string currentState;
        public string CurrentState
        {
            get => currentState;
            set
            {
                if (value != currentState)
                {
                    currentState = value;
                    OnPropertyChanged("CurrentState");
                }
            }
        }

        [TaskPane("initNLFSRCaption", "initNLFSRTooltip", null, 0, false, ControlType.Button)]
        public void initNLFSR()
        {
            OnPropertyChanged("InitNLFSR");
        }

        private int rounds = 1; //how many bits will be generated
        //[ContextMenu("Rounds", "How many bits shall be generated?", 1, ContextMenuControlType.ComboBox, new int[] { 10, 50, 100 }, "10 bits", "50 bits", "100 bits")]
        //[TaskPane("Rounds", "How many bits shall be generated?", null, 1, false, ControlType.TextBox)]
        [TaskPane("RoundsCaption", "RoundsTooltip", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int Rounds
        {
            get => rounds;
            set
            {
                if (value != rounds)
                {
                    rounds = value;
                    OnPropertyChanged("Rounds");
                }
            }
        }

        private string polynomial;
        [TaskPane("PolynomialCaption", "PolynomialTooltip", null, 0, false, ControlType.TextBox)]
        public string Polynomial
        {
            get => polynomial;
            set
            {
                if (value != polynomial)
                {
                    polynomial = value;
                    OnPropertyChanged("Polynomial");
                }
            }
        }

        private string seed;
        [TaskPane("SeedCaption", "SeedTooltip", null, 1, false, ControlType.TextBox)]
        public string Seed
        {
            get => seed;
            set
            {
                if (value != seed)
                {
                    seed = value;
                    OnPropertyChanged("Seed");
                }
            }
        }

        private bool noQuickwatch = false;
        [ContextMenu("NoQuickwatchCaption", "NoQuickwatchTooltip", 0, ContextMenuControlType.CheckBox, null, new string[] { "NoQuickwatchList1" })]
        [TaskPane("NoQuickwatchCaption", "NoQuickwatchTooltip", null, 3, true, ControlType.CheckBox, "", null)]
        public bool NoQuickwatch
        {
            get => noQuickwatch;
            set
            {
                if (value != noQuickwatch)
                {
                    noQuickwatch = value;
                    OnPropertyChanged("NoQuickwatch");
                }
            }
        }

        private bool saveCurrentState = false;
        [ContextMenu("SaveCurrentStateCaption", "SaveCurrentStateTooltip", 0, ContextMenuControlType.CheckBox, null, new string[] { "SaveCurrentStateList1" })]
        [TaskPane("SaveCurrentStateCaption", "SaveCurrentStateTooltip", null, 3, true, ControlType.CheckBox, "", null)]
        public bool SaveCurrentState
        {
            get => saveCurrentState;
            set
            {
                if (value != saveCurrentState)
                {
                    saveCurrentState = value;
                    OnPropertyChanged("SaveCurrentState");
                }
            }
        }

        private bool useClockingBit = false;
        [ContextMenu("UseClockingBitCaption", "UseClockingBitTooltip", 0, ContextMenuControlType.CheckBox, null, new string[] { "UseClockingBitList1" })]
        [TaskPane("UseClockingBitCaption", "UseClockingBitTooltip", "ClockingBitGroup", 0, false, ControlType.CheckBox, "", null)]
        public bool UseClockingBit
        {
            get => useClockingBit;
            set
            {

                if (value != useClockingBit)
                {
                    useClockingBit = value;
                    OnPropertyChanged("UseClockingBit");

                    if (useClockingBit)
                    {
                        SettingChanged("ClockingBit", Visibility.Visible);
                    }
                    else
                    {
                        SettingChanged("ClockingBit", Visibility.Collapsed);
                    }
                }
            }
        }

        private int clockingBit = 0;
        [TaskPane("ClockingBitCaption", "ClockingBitTooltip", "ClockingBitGroup", 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int ClockingBit
        {
            get => clockingBit;
            set
            {
                if (value != clockingBit)
                {
                    clockingBit = value;
                    OnPropertyChanged("ClockingBit");
                }
            }
        }

        private bool useBoolClock = false;
        [ContextMenu("UseBoolClockCaption", "UseBoolClockTooltip", 0, ContextMenuControlType.CheckBox, null, new string[] { "UseBoolClockList1" })]
        [TaskPane("UseBoolClockCaption", "UseBoolClockTooltip", "ClockGroup", 0, false, ControlType.CheckBox, "", null)]
        public bool UseBoolClock
        {
            get => useBoolClock;
            set
            {
                if (value != useBoolClock)
                {
                    useBoolClock = value;
                    OnPropertyChanged("UseBoolClock");

                    if (useBoolClock)
                    {
                        SettingChanged("Rounds", Visibility.Collapsed);
                    }
                    else
                    {
                        SettingChanged("Rounds", Visibility.Visible);
                    }
                }
            }
        }

        private bool alwaysCreateOutput = false;
        [ContextMenu("AlwaysCreateOutputCaption", "AlwaysCreateOutputTooltip", 1, ContextMenuControlType.CheckBox, null, new string[] { "AlwaysCreateOutputList1" })]
        [TaskPane("AlwaysCreateOutputCaption", "AlwaysCreateOutputTooltip", "ClockGroup", 1, false, ControlType.CheckBox, "", null)]
        public bool AlwaysCreateOutput
        {
            get => alwaysCreateOutput;
            set
            {
                if (value != alwaysCreateOutput)
                {
                    alwaysCreateOutput = value;
                    OnPropertyChanged("AlwaysCreateOutput");
                }
            }
        }

        public bool PluginIsRunning { get; set; }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        // this event is for disabling stuff in the settings pane
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null && PluginIsRunning == false)
            {
                EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
            }
        }

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
