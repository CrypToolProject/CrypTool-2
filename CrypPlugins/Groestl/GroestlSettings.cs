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

namespace CrypTool.Plugins.Groestl
{
    public enum GroestlVariant { Groestl224, Groestl256, Groestl384, Groestl512 };
    public class GroestlSettings : ISettings
    {
        #region Private Variables

        private GroestlVariant _selectedVariant = GroestlVariant.Groestl224;

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// Groestl variant.
        /// </summary>
        [TaskPane("GroestlVariantCaption", "GroestlVariantTooltip", null, 1, false, ControlType.ComboBox, new string[] {"GroestlVariantList1",
            "GroestlVariantList2", "GroestlVariantList3", "GroestlVariantList4"})]
        public int SelectedVariant
        {
            get => (int)_selectedVariant;
            set
            {
                _selectedVariant = (GroestlVariant)value;
                OnPropertyChanged("SelectedVariant");
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
