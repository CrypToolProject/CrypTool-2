/*                              
   Copyright 2009 Team CrypTool (Sven Rech,Dennis Nolte,Raoul Falk,Nils Kopal), Uni Duisburg-Essen

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

namespace CrypTool.Plugins.BooleanOperators
{
    internal class BooleanInputSettings : ISettings
    {
        #region ISettings Members

        private int bool_value = 0; //0 false; 1 true

        [TaskPane("BI_ValueCaption", "BI_ValueTooltip", null, 1, false, ControlType.ComboBox, new string[] { "BI_ValueList1", "BI_ValueList2" })]
        public int Value
        {
            get => bool_value;
            set
            {
                if ((value) != bool_value)
                {
                    bool_value = value;
                    OnPropertyChanged("Value");
                }
            }
        }

        //Workarround for AES-PKCS5-Base64-Template - should be removed
        public int Action
        {
            get => bool_value;
            set
            {
                if ((value) != bool_value)
                {
                    bool_value = value;
                    OnPropertyChanged("Value");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void Initialize()
        {
        }

        #endregion

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;
    }
}