/*
   Copyright 2022 Nils Kopal <Nils.Kopal<at>CrypTool.org

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

namespace CrypTool.MexicanArmyCipherDiskAnalyzer
{  
    public enum UnknownSymbolHandlingMode
    {
        Ignore = 0,
        Remove = 1,
        Replace = 2
    }

    public enum KeyFormat
    {
        Digits,
        LatinLetters
    }

    public class MexicanArmyCipherDiskSettings : ISettings
    {
        private string _languageCode = "en";
        private int _gramsType = 4;
        private KeyFormat _keyFormat = KeyFormat.Digits;

        public MexicanArmyCipherDiskSettings()
        {

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

        public event PropertyChangedEventHandler PropertyChanged;
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
    }
}
