/*                              
   Copyright 2009 Fabian Enkler

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
using System.ComponentModel;

namespace CrypTool.CaesarAnalysisHelper
{
    public enum Language
    {
        German,
        English,
        French,
        Spanish
    }

    internal class CaesarAnalysisHelperSettings : ISettings
    {
        internal char FrequentChar = 'e';

        private Language Lang = Language.German;

        [ContextMenu("TextLanguageCaption", "TextLanguageTooltip", 0, ContextMenuControlType.ComboBox, null, new[] { "TextLanguageList1", "TextLanguageList2", "TextLanguageList3", "TextLanguageList4" })]
        [TaskPane("TextLanguageTPCaption", "TextLanguageTPTooltip", null, 0, false, ControlType.ComboBox, new[] { "TextLanguageList1", "TextLanguageList2", "TextLanguageList3", "TextLanguageList4" })]
        public int TextLanguage
        {
            get => (int)Lang;
            set
            {
                Lang = (Language)value;
                switch (Lang)
                {
                    case Language.German:
                    case Language.English:
                    case Language.French:
                    case Language.Spanish:
                        FrequentChar = 'e';
                        break;
                    default:
                        break;
                }
                OnPropertyChanged("TextLanguage");
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
