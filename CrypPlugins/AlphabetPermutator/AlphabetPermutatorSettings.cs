/*                              
   Copyright 2013 Nils Kopal, Universität Kassel

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

namespace AlphabetPermutator
{
    public class AlphabetPermutatorSettings : ISettings
    {
        private int _order = 0;
        private int _offset = 0;
        private string _password = null;

        public AlphabetPermutatorSettings()
        {
            
        }

        #region INotifyPropertyChanged Members


        [TaskPane("OrderCaption", "OrderTooltip", null, 1, false, ControlType.ComboBox, new[] { "Ascending", "Descending", "LeaveAsIs" })]
        public int Order
        {
            get
            {
                return _order;
            }
            set
            {
                _order = value;
                OnPropertyChanged("Order");
            }
        }

        [TaskPane("OffsetCaption", "OffsetCaption", null, 2, false, ControlType.NumericUpDown,ValidationType.RangeInteger, 0, int.MaxValue-1)]
        public int Offset
        {
            get
            {
                return _offset;
            }
            set
            {
                _offset = value;
                OnPropertyChanged("Offset");
            }
        }

        [TaskPane("PasswordCaption", "PasswordTooltip", null, 3, false, ControlType.TextBox)]
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
                OnPropertyChanged("Password");
            }
        }

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
