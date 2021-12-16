/*
   Copyright 2011 CrypTool 2 Team <ct2contact@CrypTool.org>

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

namespace CrypTool.Plugins.PaddingOracle
{
    public class PaddingOracleSettings : ISettings
    {
        #region Private Variables

        private readonly int viewByte;

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>

        /*[TaskPane("ViewByte", "Viewed byte range", null, 2, true, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, Int32.MaxValue)]
        public int ViewByte
        {
            get { return viewByte; }
            set
            {
                viewByte = value;

                OnPropertyChanged("ViewByte");
            }
        }*/

        /*[TaskPane("Padding Method", "Select the Padding Method that was used", null, 1, false, ControlType.ComboBox, new string[] { "RFC5652", "NULL" })]
        public int PaddingMethod
        {
            get { return paddingMethod; }
            set
            {
                paddingMethod = value;
                OnPropertyChanged("PaddingMethod");
            }
        }*/

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        public void Initialize()
        {

        }

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion
    }
}
