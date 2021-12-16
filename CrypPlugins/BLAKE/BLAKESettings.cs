/*
   Copyright 2014 Diego Alejandro Gómez <diego.gomezy@udea.edu.co>

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

namespace CrypTool.Plugins.BLAKE
{
    public enum BLAKEFunction { BLAKE224, BLAKE256, BLAKE384, BLAKE512 };
    public class BLAKESettings : ISettings
    {
        #region Private Variables

        private BLAKEFunction _selectedFunction = BLAKEFunction.BLAKE224;

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// BLAKE Function.
        /// </summary>
        [TaskPane("BLAKEFunctionCaption", "BLAKEFunctionTooltip", null, 1, false, ControlType.ComboBox, new string[] {"BLAKEFunctionList1",
            "BLAKEFunctionList2", "BLAKEFunctionList3", "BLAKEFunctionList4"})]
        public int SelectedFunction
        {
            get => (int)_selectedFunction;
            set
            {
                _selectedFunction = (BLAKEFunction)value;
                OnPropertyChanged("SelectedFunction");
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion

        public void Initialize() { }
    }
}
