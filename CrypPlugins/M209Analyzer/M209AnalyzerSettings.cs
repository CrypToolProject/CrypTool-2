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
using CrypTool.PluginBase.Utils;
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
        private string _languageCode = "en";
        private int _gramsType = 4;
        private KeyFormat _keyFormat = KeyFormat.Digits;

        // number of possible monograms, with c = 26 for English
        private int _c = 26;

        private double _minRatio = Math.Log(0.0085);
        private double _startTemperature = 1000.0;
        private double _endTemperature = 1.0;
        private double _decrement = 1.1;

        #endregion

        #region TaskPane Settings

        /// <summary>
        /// HOWTO: This is an example for a setting entity shown in the settings pane on the right of the CT2 main window.
        /// This example setting uses a number field input, but there are many more input types available, see ControlType enumeration.
        /// </summary>
        [TaskPane("AttackMode", "Change the attack mode", null, 0, false, ControlType.ComboBox, new string[] { "CiphertextOnly", "KnownPlaintext" })]
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

        [TaskPane("LanguageCaption", "LanguageTooltip", null, 0, false, ControlType.LanguageSelector)]
        public int Language
        {
            get => LanguageStatistics.LanguageId(_languageCode);
            set
            {
                if (value != LanguageStatistics.LanguageId(_languageCode))
                {
                    _languageCode = LanguageStatistics.LanguageCode(value);
                    OnPropertyChanged("Language");
                }
            }
        }

        [TaskPane("GramsTypeCaption", "GramsTypeTooltip", null, 1, false, ControlType.ComboBox,
            new string[] { "Unigrams", "Bigrams", "Trigrams", "Tetragrams", "Pentagrams" })]
        public int GramsType
        {
            get => _gramsType;
            set
            {
                if (value != _gramsType)
                {
                    _gramsType = value;
                    OnPropertyChanged("GramsType");
                }
            }
        }

        [TaskPane("KeyFormatCaption", "KeyFormatTooltip", null, 2, false, ControlType.ComboBox,
          new string[] { "Digits", "LatinLetters" })]
        public KeyFormat KeyFormat
        {
            get => _keyFormat;
            set
            {
                if (value != _keyFormat)
                {
                    _keyFormat = value;
                    OnPropertyChanged("KeyFormat");
                }
            }
        }

        [TaskPane("SA MinRatio", "Simulated Annealing - MinRatio", null, 0, false, ControlType.TextBox, ValidationType.RangeDouble, 0, 50)]
        public double MinRatio
        {
            get
            {
                return _minRatio;
            }
            set
            {
                if (_minRatio != value)
                {
                    _minRatio = Math.Log(value);
                    OnPropertyChanged("MinRatio");
                }
            }
        }

        [TaskPane("SA StartTemperature", "Simulated Annealing - StartTemperature", null, 0, false, ControlType.TextBox, ValidationType.RangeDouble, 0, 1000000)]
        public double StartTemperature
        {
            get
            {
                return _startTemperature;
            }
            set
            {
                if (_startTemperature != value)
                {
                    _startTemperature = value;
                    OnPropertyChanged("StartTemperature");
                }
            }
        }

        [TaskPane("SA EndTemperature", "Simulated Annealing - EndTemperature", null, 0, false, ControlType.TextBox, ValidationType.RangeDouble, 0, 1000000)]
        public double EndTemperature
        {
            get
            {
                return _endTemperature;
            }
            set
            {
                if (_endTemperature != value)
                {
                    _endTemperature = value;
                    OnPropertyChanged("EndTemperature");
                }
            }
        }

        [TaskPane("SA Decrement", "Simulated Annealing - Decrement", null, 0, false, ControlType.TextBox, ValidationType.RangeDouble, 0, 1000000)]
        public double Decrement
        {
            get
            {
                return _decrement;
            }
            set
            {
                if (_decrement != value)
                {
                    _decrement = value;
                    OnPropertyChanged("Decrement");
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
