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
using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;

namespace Splitter
{
    public class SplitterSettings : ISettings
    {

        private int fireOnValue;
        [ContextMenu("FireOnValueCaption", "FireOnValueTooltip", 0, ContextMenuControlType.ComboBox, null, "FireOnValueList1", "FireOnValueList2")]
        [TaskPane("FireOnValueCaption", "FireOnValueTooltip", null, 0, false, ControlType.ComboBox, new string[] { "FireOnValueList1", "FireOnValueList2" })]
        public int FireOnValue
        {
            get => fireOnValue;
            set
            {
                if (value != fireOnValue)
                {
                    fireOnValue = value;
                    OnPropertyChanged("FireOnValue");
                }
            }
        }

        private string delimiterDictionary = " ";
        [TaskPaneAttribute("DelimiterDictionaryCaption", "DelimiterDictionaryTooltip", null, 1, false, ControlType.TextBox)]
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

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        protected void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}
