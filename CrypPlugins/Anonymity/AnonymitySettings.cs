/*
   Copyright Mikail Sarier 2023, University of Mannheim

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

namespace CrypTool.Plugins.Anonymity
{
    public class AnonymitySettings : ISettings
    {
        #region Private Variables

        private string _rowSeparator = "\\n";
        private string _columnSeparator = ",";
        #endregion

        #region TaskPane Settings
        [TaskPane("Row separator", "This is the Row separator", null, 0, false, ControlType.TextBox)]
        public string RowSeparator
        {
            get
            {
                return _rowSeparator;
            }
            set
            {
                if (_rowSeparator != value)
                {
                    _rowSeparator = value;
                    OnPropertyChanged("_rowSeparator;");
                }
            }
        }

        [TaskPane("Column separator", "This is the Column separator", null, 1, false, ControlType.TextBox)]
        public string ColumnSeparator
        {

            get
            {
                return _columnSeparator;
            }
            set
            {
                if (_columnSeparator != value)
                {
                    _columnSeparator = value;
                    OnPropertyChanged("ColumnSeparator");
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