/*
   Copyright (C) CrypTool 2 Team

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

namespace FriedmanTest
{
    internal class FriedmanTestSettings : ISettings
    {

        private int kappa = 0; //0="English", 1="German", 2="French", 3="Spanish", 4="Italian",5="Portugeese"
        #region ISettings Members

        [ContextMenu("KappaCaption", "KappaTooltip", 2, ContextMenuControlType.ComboBox, null, new string[] { "KappaList1", "KappaList2", "KappaList3", "KappaList4", "KappaList5", "KappaList6" })]
        [TaskPane("KappaCaption", "KappaTooltip", null, 2, false, ControlType.ComboBox, new string[] { "KappaList1", "KappaList2", "KappaList3", "KappaList4", "KappaList5", "KappaList6" })]
        public int Kappa
        {
            get => kappa;
            set
            {
                if (value != kappa)
                {
                    kappa = value;
                    OnPropertyChanged("Kappa");
                }
            }
        }

        #endregion

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
