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
using CrypTool.T9Code.Enums;
using LanguageStatisticsLib;
using System.ComponentModel;

namespace CrypTool.T9Code
{
    public class T9CodeSettings : ISettings
    {
        #region Private Variables

        private T9Mode _mode;
        private InternalGramType _gramSize = InternalGramType.Bigrams;
        private string _language = "en";

        #endregion

        #region TaskPane Settings

        [TaskPane("ModeCaption", "ModeTooltip", null, 4, false,
            ControlType.ComboBox, new string[] { "ModeEncode", "ModeDecode" })]
        public T9Mode Mode
        {
            get => _mode;
            set
            {
                if (_mode == value)
                {
                    return;
                }

                _mode = value;
                OnPropertyChanged(nameof(Mode));
            }
        }

        [TaskPane("GramSizeCaption", "GramSizeTooltip", null, 4, false,
            ControlType.ComboBox,
            new string[] { "Unigram", "Bigram", "Trigrams", "Tetragrams", "Pentragrams", "Hexagrams" })]
        public InternalGramType GramSize
        {
            get => _gramSize;
            set
            {
                if (_gramSize == value)
                {
                    return;
                }

                _gramSize = value;
                OnPropertyChanged(nameof(GramSize));
            }
        }

        [TaskPane("LanguageCaption", "LanguageTooltip", null, 5, false, ControlType.LanguageSelector)]
        public int Language
        {
            get => LanguageStatistics.LanguageId(_language);
            set
            {
                if (value == LanguageStatistics.LanguageId(_language))
                {
                    return;
                }
                _language = LanguageStatistics.LanguageCode(value);
                OnPropertyChanged("Language");
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