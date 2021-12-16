/*
   Copyright 2008 Thomas Schmid, University of Siegen

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

namespace IncDec
{
    public class IncDecSettings : ISettings
    {
        #region private variables
        private int value = 1;
        private Operator currentMode = Operator.Increment;
        #endregion private variables

        public enum Operator
        {
            Increment,
            Decrement
        }

        public Operator CurrentMode => currentMode;

        [ContextMenu("ModeSelectCaption", "ModeSelectTooltip", 0, ContextMenuControlType.ComboBox, null, new string[] { "ModeSelectList1", "ModeSelectList2" })]
        [TaskPane("ModeSelectCaption", "ModeSelectTooltip", null, 0, false, ControlType.ComboBox, new string[] { "ModeSelectList1", "ModeSelectList2" })]
        public int ModeSelect
        {
            get => (int)currentMode;
            set
            {
                if (value != (int)currentMode)
                {
                    currentMode = (Operator)value;
                    OnPropertyChanged("ModeSelect");
                }
            }
        }


        [TaskPane("ValueCaption", "ValueTooltip", null, 1, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int Value
        {
            get => value;
            set
            {
                if (value != this.value)
                {
                    this.value = value;
                    OnPropertyChanged("Value");
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
