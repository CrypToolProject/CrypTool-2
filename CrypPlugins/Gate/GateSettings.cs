/*
   Copyright 2009 Matthäus Wander, Universität Duisburg-Essen

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

namespace Gate
{
    public enum Trigger
    {
        AlwaysOpen, AlwaysClosed, TrueValue, FalseValue, AnyEdge, PositiveEdge, NegativeEdge, Counter
    };

    public class GateSettings : ISettings
    {
        private Trigger trigger = 0;
        private int maxCounter = 100;

        [TaskPane("TriggerCaption", "TriggerTooltip", null, 1, true, ControlType.RadioButton,
            new string[] { "TriggerList1", "TriggerList2", "TriggerList3", "TriggerList4", "TriggerList5", "TriggerList6", "TriggerList7", "TriggerList8" })]
        public Trigger Trigger
        {
            get => trigger;
            set
            {
                if (trigger != value)
                {
                    trigger = value;
                    OnPropertyChanged("Trigger");
                }

                if (trigger == Trigger.Counter)
                {
                    showSettingsElement("MaxCounter");
                }
                else
                {
                    hideSettingsElement("MaxCounter");
                }
            }
        }
        #region INotifyPropertyChanged Members

        [TaskPane("MaxCounterCaption", "MaxCounterTooltiü", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, int.MaxValue)]
        public int MaxCounter
        {
            get => maxCounter;
            set => maxCounter = value;

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

        public void Initialize()
        {
            if (trigger == Trigger.Counter)
            {
                showSettingsElement("MaxCounter");
            }
            else
            {
                hideSettingsElement("MaxCounter");
            }
        }

        private void OnPropertyChanged(string p)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, p);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        #endregion
    }
}
