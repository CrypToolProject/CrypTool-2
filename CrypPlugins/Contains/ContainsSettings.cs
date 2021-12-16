/*
   Copyright 2008 Thomas Schmid, University of Siegen

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

namespace Contains
{
    public class ContainsSettings : ISettings
    {
        public enum SearchType
        {
            Hashtable,
            AhoCorasick
        }

        private SearchType search = SearchType.Hashtable;
        public SearchType Search => search;


        [ContextMenu("SearchSettingCaption", "SearchSettingTooltip", 1, ContextMenuControlType.ComboBox, null, new string[] { "SearchSettingList1", "SearchSettingList2" })]
        [TaskPane("SearchSettingCaption", "SearchSettingTooltip", null, 1, false, ControlType.ComboBox, new string[] { "SearchSettingList1", "SearchSettingList2" })]
        public int SearchSetting
        {
            get => (int)search;
            set
            {
                if (value != (int)search)
                {
                    search = (SearchType)value;
                    OnPropertyChanged("SearchSetting");
                }
            }
        }

        private int hits = 1;
        [TaskPane("HitsCaption", "HitsTooltip", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, int.MaxValue)]
        public int Hits
        {
            get => hits;
            set
            {
                if (value != hits)
                {
                    hits = value;
                    OnPropertyChanged("Hits");
                }
            }
        }

        private string delimiter = " ";
        [TaskPaneAttribute("DelimiterInputStringCaption", "DelimiterInputStringTooltip", null, 3, false, ControlType.TextBox, ValidationType.RegEx, "^.?$")] // [a-zA-Z]|[0-9]|\\s
        public string DelimiterInputString
        {
            get => delimiter;
            set
            {
                if (value != delimiter)
                {
                    delimiter = value;
                }
                OnPropertyChanged("DelimiterInputString");
            }
        }

        private string delimiterDictionary = " ";
        [TaskPaneAttribute("DelimiterDictionaryCaption", "DelimiterDictionaryTooltip", null, 4, false, ControlType.TextBox, ValidationType.RegEx, "^.?$")] // [a-zA-Z]|[0-9]|\\s
        public string DelimiterDictionary
        {
            get => delimiterDictionary;
            set
            {
                if (value != delimiterDictionary)
                {
                    delimiterDictionary = value;
                }
                OnPropertyChanged("DelimiterDictionary");
            }
        }

        private bool toLower;
        [ContextMenu("ToLowerCaption", "ToLowerTooltip", 5, ContextMenuControlType.CheckBox, null)]
        [TaskPaneAttribute("ToLowerCaption", "ToLowerTooltip", null, 5, false, ControlType.CheckBox)]
        public bool ToLower
        {
            get => toLower;
            set
            {
                if (toLower != value)
                {
                    toLower = value;
                    OnPropertyChanged("ToLower");
                }
            }
        }

        private bool ignoreDiacritics;
        [ContextMenu("IgnoreDiacriticsCaption", "IgnoreDiacriticsTooltip", 6, ContextMenuControlType.CheckBox, null)]
        [TaskPaneAttribute("IgnoreDiacriticsCaption", "IgnoreDiacriticsTooltip", null, 6, false, ControlType.CheckBox)]
        public bool IgnoreDiacritics
        {
            get => ignoreDiacritics;
            set
            {
                if (ignoreDiacritics != value)
                {
                    ignoreDiacritics = value;
                    OnPropertyChanged("IgnoreDiacritics");
                }
            }
        }

        private bool hitPercentFromInputString = false;
        [ContextMenu("HitPercentFromInputStringCaption", "HitPercentFromInputStringTooltip", 7, ContextMenuControlType.CheckBox, null)]
        [TaskPaneAttribute("HitPercentFromInputStringCaption", "HitPercentFromInputStringTooltip", null, 7, false, ControlType.CheckBox)]
        public bool HitPercentFromInputString
        {
            get => hitPercentFromInputString;
            set
            {
                if (hitPercentFromInputString != value)
                {
                    hitPercentFromInputString = value;
                    OnPropertyChanged("HitPercentFromInputString");
                }
            }
        }

        private bool countWordsOnlyOnce = true;
        [ContextMenu("CountWordsOnlyOnceCaption", "CountWordsOnlyOnceTooltip", 8, ContextMenuControlType.CheckBox, null)]
        [TaskPaneAttribute("CountWordsOnlyOnceCaption", "CountWordsOnlyOnceTooltip", null, 8, false, ControlType.CheckBox)]
        public bool CountWordsOnlyOnce
        {
            get => countWordsOnlyOnce;
            set
            {
                if (countWordsOnlyOnce != value)
                {
                    countWordsOnlyOnce = value;
                    if (countWordsOnlyOnce && search == SearchType.AhoCorasick)
                    {
                        countWordsOnlyOnce = false;
                    }

                    OnPropertyChanged("CountWordsOnlyOnce");
                }
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
