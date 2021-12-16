/*
   Copyright 2009 Holger Pretzsch, University of Duisburg-Essen

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

namespace CrypTool.HMAC
{
    internal class HMACSettings : ISettings
    {
        public enum HashFunction { MD5, RIPEMD160, SHA1, SHA256, SHA384, SHA512 };

        private HashFunction selectedHashFunction = HashFunction.MD5;

        [ContextMenu("SelectedHashFunctionCaption", "SelectedHashFunctionTooltip", 1, ContextMenuControlType.ComboBox, null, new string[] { "SelectedHashFunctionList1", "SelectedHashFunctionList2", "SelectedHashFunctionList3", "SelectedHashFunctionList4", "SelectedHashFunctionList5", "SelectedHashFunctionList6" })]
        [TaskPane("SelectedHashFunctionCaption", "SelectedHashFunctionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "SelectedHashFunctionList1", "SelectedHashFunctionList2", "SelectedHashFunctionList3", "SelectedHashFunctionList4", "SelectedHashFunctionList5", "SelectedHashFunctionList6" })]
        public int SelectedHashFunction
        {
            get => (int)selectedHashFunction;
            set
            {
                selectedHashFunction = (HashFunction)value;
                OnPropertyChanged("SelectedHashFunction");
            }
        }

        #region INotifyPropertyChanged Members

#pragma warning disable 67
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }
#pragma warning restore

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        #region ISettings Members

        #endregion
    }
}
