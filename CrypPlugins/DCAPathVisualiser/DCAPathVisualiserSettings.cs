/*
   Copyright 2019 Christian Bender christian1.bender@student.uni-siegen.de

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
using DCAPathVisualiser;
using System.ComponentModel;

namespace CrypTool.Plugins.DCAPathVisualiser
{
    public class DCAPathVisualiserSettings : ISettings
    {
        #region Private Variables

        private Algorithms _currentAlgorithm = Algorithms.Cipher1;

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// Selection of the toy cipher algorithm
        /// </summary>
        [TaskPane("ChoiceOfAlgorithm", "ChoiceOfAlgorithmToolTop", null, 2, false, ControlType.ComboBox, new string[] { "Cipher1", "Cipher2", "Cipher3" })]
        public Algorithms CurrentAlgorithm
        {
            get => _currentAlgorithm;
            set
            {
                _currentAlgorithm = value;
                OnPropertyChanged("CurrentAlgorithm");
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

        public void Initialize()
        {

        }
    }
}
