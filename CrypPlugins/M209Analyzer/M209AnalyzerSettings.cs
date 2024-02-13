/*
   Copyright CrypTool 2 Team josef.matwich@gmail.com

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
using LanguageStatisticsLib;
using System;
using System.ComponentModel;
using System.Windows;

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
        private bool _expertMode = false;

        // number of possible monograms, with c = 26 for English
        private int _letterCount = 26;

        private int _coresUsed = 1;

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
        [TaskPane("AttackMode", "AttackModeCaption", "GeneralCaption", 0, false, ControlType.ComboBox, new string[] { "Ciphertext-only", "Known-plaintext" })]
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

        [TaskPane("LetterCount", "LetterCountCaption", "GeneralCaption", 0, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 50)]
        public int LetterCount
        {
            get
            {
                return _letterCount;
            }
            set
            {
                if (_letterCount != value)
                {
                    _letterCount = value;
                    // HOWTO: MUST be called every time a property value changes with correct parameter name
                    OnPropertyChanged("LetterCount");
                }
            }
        }

        [TaskPane("Cores used", "CoresUsedCaption", "GeneralCaption", 0, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 50)]
        public int CoresUsed
        {
            get
            {
                return _coresUsed;
            }
            set
            {
                if (_coresUsed != value)
                {
                    _coresUsed = value;
                    // HOWTO: MUST be called every time a property value changes with correct parameter name
                    OnPropertyChanged("CoresUsed");
                }
            }
        }

        [TaskPane("Language", "LanguageCaption", "GeneralCaption", 0, false, ControlType.LanguageSelector)]
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

        [TaskPane("Grams type", "GramsTypeCaption", "GeneralCaption", 1, false, ControlType.ComboBox,
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

        [TaskPane("Key format", "KeyFormatCaption", "GeneralCaption", 2, false, ControlType.ComboBox,
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

        [TaskPane("Expert mode", "ExpertModeCaption", "GeneralCaption", 2, true, ControlType.CheckBox)]
        public bool ExpertMode
        {
            get => _expertMode;
            set
            {
                if (_expertMode != value)
                {
                    _expertMode = value;
                    OnPropertyChanged("ExpertMode");
                    UpdateSettingsVisibility();
                }
            }
        }


        [TaskPane("Simulated Annealing - MinRatio", "Simulated Annealing - MinRatio", "Expert mode", 0, false, ControlType.TextBox, ValidationType.RangeDouble, 0, 50)]
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
                    _minRatio = value;
                    OnPropertyChanged("MinRatio");
                }
            }
        }

        [TaskPane("Simulated Annealing - StartTemperature", "Simulated Annealing - StartTemperature", "Expert mode", 0, false, ControlType.TextBox, ValidationType.RangeDouble, 0, 1000000)]
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

        [TaskPane("Simulated Annealing - EndTemperature", "Simulated Annealing - EndTemperature", "Expert mode", 0, false, ControlType.TextBox, ValidationType.RangeDouble, 0, 1000000)]
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

        [TaskPane("Simulated Annealing - Decrement", "Simulated Annealing - Decrement", "Expert mode", 0, false, ControlType.TextBox, ValidationType.RangeDouble, 0, 1000000)]
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

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        #endregion

        private void ShowHideSetting(string propertyName, bool show)
        {
            if (TaskPaneAttributeChanged == null)
            {
                return;
            }

            TaskPaneAttribteContainer container = new TaskPaneAttribteContainer(propertyName, show == true ? Visibility.Visible : Visibility.Collapsed);
            TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(container));
        }

        public void Initialize()
        {
            UpdateSettingsVisibility();
        }

        private void UpdateSettingsVisibility()
        {
            ShowHideSetting("Decrement", _expertMode);
            ShowHideSetting("EndTemperature", _expertMode);
            ShowHideSetting("StartTemperature", _expertMode);
            ShowHideSetting("MinRatio", _expertMode);
        }
    }
}
