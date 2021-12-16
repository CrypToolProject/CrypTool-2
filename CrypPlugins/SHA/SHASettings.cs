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

namespace SHA
{
    public class SHASettings : ISettings
    {
        public enum ShaFunction { SHA1, SHA256, SHA384, SHA512 };

        private ShaFunction selectedShaFunction = ShaFunction.SHA1;

        [ContextMenu("SHAFunctionCaption", "SHAFunctionTooltip", 1, ContextMenuControlType.ComboBox, null, new string[] { "SHAFunctionList1", "SHAFunctionList2", "SHAFunctionList3", "SHAFunctionList4" })]
        [TaskPane("SHAFunctionCaption", "SHAFunctionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "SHAFunctionList1", "SHAFunctionList2", "SHAFunctionList3", "SHAFunctionList4" })]
        public int SHAFunction
        {
            get => (int)selectedShaFunction;
            set
            {
                selectedShaFunction = (ShaFunction)value;
                OnPropertyChanged("SHAFunction");
            }
        }

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
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
