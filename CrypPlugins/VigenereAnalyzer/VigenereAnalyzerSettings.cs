/*
   Copyright 2022 Nils Kopal, CrypTool project

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
using LanguageStatisticsLib;
using System.ComponentModel;

namespace CrypTool.VigenereAnalyzer
{
    public enum Mode
    {
        Vigenere = 0,
        VigenereAutokey = 1,
        Beaufort,
        BeaufortAutokey,
    };

    public enum UnknownSymbolHandlingMode
    {
        Ignore = 0,
        Remove = 1,
        Replace = 2
    };

    public enum KeyStyle
    {
        Random = 0,
        NaturalLanguage = 1
    }

    internal class VigenereAnalyzerSettings : ISettings
    {
        private Mode _mode = Mode.Vigenere;
        private int _fromKeylength;
        private int _toKeyLength = 20;
        private int _restarts = 50;
        private KeyStyle _keyStyle;
        private string _languageCode = "en"; //default language is English
        private int _gramsType = 4; // Pentagrams

        public void Initialize()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;

        [TaskPane("ModeCaption", "ModeTooltip", null, 1, false, ControlType.ComboBox, new[] { "Vigenere", "VigenereAutokey", "Beaufort", "BeaufortAutokey" })]
        public Mode Mode
        {
            get => _mode;
            set
            {
                if (value != _mode)
                {
                    _mode = value;
                    OnPropertyChanged("Mode");
                }
            }
        }

        [TaskPane("FromKeylengthCaption", "FromKeylengthTooltip", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 100)]
        public int FromKeylength
        {
            get => _fromKeylength;
            set
            {
                if (value != _fromKeylength)
                {
                    _fromKeylength = value;
                    OnPropertyChanged("FromKeyLength");
                }
            }
        }

        [TaskPane("ToKeylengthCaption", "ToKeylengthTooltip", null, 3, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 100)]
        public int ToKeyLength
        {
            get => _toKeyLength;
            set
            {
                if (value != _toKeyLength)
                {
                    _toKeyLength = value;
                    OnPropertyChanged("ToKeyLength");
                }
            }
        }

        [TaskPane("RestartsCaption", "RestartsTooltip", null, 4, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 10000)]
        public int Restarts
        {
            get => _restarts;
            set
            {
                if (value != _restarts)
                {
                    _restarts = value;
                    OnPropertyChanged("Restarts");
                }
            }
        }

        [TaskPane("LanguageCaption", "LanguageTooltip", null, 5, false, ControlType.LanguageSelector)]
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

        [TaskPane("GramsTypeCaption", "GramsTypeTooltip", null, 6, false, ControlType.ComboBox,
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


        [TaskPane("KeyStyleCaption", "KeyStyleTooltip", null, 7, false, ControlType.ComboBox, new string[] { "Random", "NaturalLanguage" })]
        public KeyStyle KeyStyle
        {
            get => _keyStyle;
            set
            {
                if (value != _keyStyle)
                {
                    _keyStyle = value;
                    OnPropertyChanged("KeyStyle");
                }
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
