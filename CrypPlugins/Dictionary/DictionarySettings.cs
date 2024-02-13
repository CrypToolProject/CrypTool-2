/*                              
   Copyright 2023 Nils Kopal, CrypTool Project

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

namespace Dictionary
{
    public enum Case
    {
        Lowercase,
        Uppercase        
    }

    public class CrypToolDictionarySettings : ISettings
    {
        #region private_variables
        private string _languageCode = "en"; //default language is English
        private Case _capitalization = Case.Uppercase;
        #endregion private_variables

        public delegate void ExecuteCallback();

        [TaskPane("DictionaryCaption", "DictionaryTooltip", null, 0, false, ControlType.LanguageSelector)]
        public int Language
        {
            get => LanguageStatistics.LanguageId(_languageCode);
            set
            {
                if (value != LanguageStatistics.LanguageId(_languageCode))
                {
                    _languageCode = LanguageStatistics.LanguageCode(value);
                    OnPropertyChanged(nameof(Language));
                }
            }
        }

        [TaskPane("CapitalizationCaption", "CapitalizationTooltip", null, 1, false, ControlType.ComboBox, new string[] { "Lowercase", "Uppercase" })]
        public Case Capitalization
        {
            get
            {
                return _capitalization;
            }
            set
            {
                _capitalization = value;
                OnPropertyChanged(nameof(Capitalization));
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

    }
}
