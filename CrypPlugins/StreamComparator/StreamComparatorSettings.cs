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

namespace CrypTool.StreamComparator
{
    public class StreamComparatorSettings : ISettings
    {
        private bool diff;

        [ContextMenu("DiffCaption", "DiffTooltip", 1, ContextMenuControlType.CheckBox, new int[] { 3 }, "DiffList1")]
        [TaskPaneAttribute("DiffCaption", "DiffTooltip", "", 1, false, ControlType.CheckBox, "", null)]
        public bool Diff
        {
            get => diff;
            set
            {
                if (diff != value)
                {
                    diff = value;
                    OnPropertyChanged("Diff");
                }
            }
        }

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
