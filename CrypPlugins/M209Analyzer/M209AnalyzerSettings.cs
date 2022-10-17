/*
   Copyright CrypTool 2 Team <ct2contact@cryptool.org>

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
using System;
using System.ComponentModel;

namespace CrypTool.Plugins.M209Analyzer
{
    public enum AttackMode
    {
        CiphertextOnly,
        KnownPlaintext
    }
    public class M209AnalyzerSettings : ISettings
    {
        #region Private Variables

        private AttackMode _attackMode = AttackMode.CiphertextOnly;

        // number of possible monograms, with c = 26 for English
        private int _c = 26;

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [TaskPane("AttackMode", "Change the attack mode", null, 0, false, ControlType.ComboBox, new string[] {"CiphertextOnly", "KnownPlaintext" })]
        public AttackMode AttackMode
        {
            get
            {
                return _attackMode;
            }
            set
            {
                if (_attackMode != value)
                {
                    _attackMode = value;
                    // HOWTO: MUST be called every time a property value changes with correct parameter name
                    OnPropertyChanged("AttackMode");
                }
            }
        }

        [TaskPane("c", "Change the attack mode", null, 0, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 50)]
        public int c
        {
            get
            {
                return _c;
            }
            set
            {
                if (_c != value)
                {
                    _c = value;
                    // HOWTO: MUST be called every time a property value changes with correct parameter name
                    OnPropertyChanged("c");
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
