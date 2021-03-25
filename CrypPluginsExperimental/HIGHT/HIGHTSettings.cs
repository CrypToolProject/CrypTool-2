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

using System;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.HIGHT
{
    public class HIGHTSettings : ISettings
    {
        #region ISettings Members

        private int action = 0; //0=encrypt, 1=decrypt
        private int padding = 0; //0="Zeros"=default, 1="None", 2="PKCS7"

        [ContextMenu("ActionCaption", "ActionTooltip", 1, ContextMenuControlType.ComboBox, new int[] { 1, 2 }, "ActionList1", "ActionList2")]
        [TaskPane("ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public int Action
        {
            get { return this.action; }
            set
            {
                if (((int)value) != padding)
                {
                    this.action = (int)value;
                    OnPropertyChanged("Action");
                }
            }
        }

        [ContextMenu("PaddingCaption", "PaddingTooltip", 2, ContextMenuControlType.ComboBox, null, "PaddingList1", "PaddingList2", "PaddingList3")]
        [TaskPane("PaddingTPCaption", "PaddingTPTooltip", null, 2, false, ControlType.ComboBox, new String[] { "PaddingList1", "PaddingList2", "PaddingList3" })]
        public int Padding
        {
            get { return this.padding; }
            set
            {
                if (((int)value) != padding)
                {
                    this.padding = (int)value;
                    OnPropertyChanged("Padding");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            
        }

        private void OnPropertyChanged(string propName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propName);
        }

        #endregion
    }
}
