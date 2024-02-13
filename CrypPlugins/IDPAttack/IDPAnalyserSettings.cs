/*
   Copyright 2023 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using CrypTool.PluginBase.Utils;
using LanguageStatisticsLib;
using System.ComponentModel;

namespace IDPAnalyser
{
    internal class IDPAnalyserSettings : ISettings
    {
        #region settings
        private int _selected_method = 0;
        private string _languageCode = "en"; //default language is English

        [TaskPane("Analysis_methodCaption", "Analysis_methodTooltip", null, 1, false, ControlType.ComboBox, new string[] { "Analysis_methodList1", "Analysis_methodList2" })]
        public int Analysis_method
        {
            get => _selected_method;

            set
            {
                if (value != _selected_method)
                {
                    _selected_method = value;
                    OnPropertyChanged("Analysis_method");
                }
            }
        }

        [TaskPane("LanguageCaption", "LanguageTooltip", null, 2, false, ControlType.LanguageSelector)]
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

        private int key1Min = 8;
        [PropertySaveOrder(3)]
        [TaskPaneAttribute("Key1MinCaption", "Key1MinTooltip", "KeyGroup", 5, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 2, 500)]
        public int Key1Min
        {
            get => key1Min;
            set
            {
                if (value != key1Min)
                {
                    key1Min = value;
                    OnPropertyChanged("Key1Min");
                }
            }
        }

        private int key1Max = 10;
        [PropertySaveOrder(3)]
        [TaskPaneAttribute("Key1MaxCaption", "Key1MaxTooltip", "KeyGroup", 6, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 2, 500)]
        public int Key1Max
        {
            get => key1Max;
            set
            {
                if (value != key1Max)
                {
                    key1Max = value;
                    OnPropertyChanged("Key1Max");
                }
            }
        }

        private int key2Min = 8;
        [PropertySaveOrder(3)]
        [TaskPaneAttribute("Key2MinCaption", "Key2MinTooltip", "KeyGroup", 7, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 2, 500)]
        public int Key2Min
        {
            get => key2Min;
            set
            {
                if (value != key2Min)
                {
                    key2Min = value;
                    OnPropertyChanged("Key2Min");
                }
            }
        }

        private int key2Max = 10;
        [PropertySaveOrder(3)]
        [TaskPaneAttribute("Key2MaxCaption", "Key2MaxTooltip", "KeyGroup", 8, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 2, 500)]
        public int Key2Max
        {
            get => key2Max;
            set
            {
                if (value != key2Max)
                {
                    key2Max = value;
                    OnPropertyChanged("Key2Max");
                }
            }
        }

        private int repeatings = 10;
        [PropertySaveOrder(8)]
        [TaskPaneAttribute("RepeatingsCaption", "RepeatingsTooltip", null, 9, true, ControlType.TextBox, ValidationType.RegEx, "[0-9]{1,2}")]
        public int Repeatings
        {
            get => repeatings;
            set
            {
                if (value != repeatings)
                {
                    repeatings = value;
                    OnPropertyChanged("Repeatings");
                }
            }
        }

        private int iterations = 5000;
        [PropertySaveOrder(9)]
        [TaskPaneAttribute("IterationsCaption", "IterationsTooltip", null, 10, true, ControlType.TextBox, ValidationType.RegEx, "[0-9]{1,2}")]
        public int Iterations
        {
            get => iterations;
            set
            {
                if (value != iterations)
                {
                    iterations = value;
                    OnPropertyChanged("Iterations");
                }
            }
        }

        #endregion

        #region Events

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
        }

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}