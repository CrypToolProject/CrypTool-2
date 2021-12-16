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
using DCAToyCiphers;
using System.ComponentModel;

namespace CrypTool.Plugins.DCAToyCiphers
{
    public class DCAToyCiphersSettings : ISettings
    {
        #region Private Variables

        private Algorithms _algorithm = Algorithms.Cipher1;
        private Mode _mode = Mode.Encrypt;

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// Selection of the operating mode
        /// </summary>
        [TaskPane("ChoiceOfMode", "ChoiceOfModeToolTop", "OperatingOptions", 1, false, ControlType.ComboBox, new string[] { "Mode1", "Mode2" })]
        public Mode CurrentMode
        {
            get => _mode;
            set
            {
                _mode = value;
                OnPropertyChanged("CurrentMode");
            }
        }

        /// <summary>
        /// Selection of the toy cipher algorithm
        /// </summary>
        //[TaskPane("ChoiceOfAlgorithm", "ChoiceOfAlgorithmToolTop", null, 1, false, ControlType.ComboBox, new string[]{ "Cipher1", "Cipher2", "Cipher3", "Cipher4", "Cipher5" })]
        [TaskPane("ChoiceOfAlgorithm", "ChoiceOfAlgorithmToolTop", "OperatingOptions", 2, false, ControlType.ComboBox, new string[] { "Cipher1", "Cipher2", "Cipher3" })]
        public Algorithms CurrentAlgorithm
        {
            get => _algorithm;
            set
            {
                if (_algorithm != value)
                {
                    _algorithm = value;
                    OnPropertyChanged("CurrentAlgorithm");
                }
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
