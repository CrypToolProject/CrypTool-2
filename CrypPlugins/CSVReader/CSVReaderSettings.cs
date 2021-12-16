/*                              
   Copyright 2013 Nils Kopal, Universität Kassel

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

namespace CSVReader
{

    public class CSVReaderSettings : ISettings
    {
        private int _columndID = 0;
        private string _rowSeparator = ";";
        private string _columnSeparator = ",";
        private string _resultSeparator = "\\n";


        [TaskPane("RowSeparatorCaption", "RowSeparatorTooltip", null, 0, false, ControlType.TextBox)]
        public string RowSeparator
        {
            get => _rowSeparator;
            set
            {
                if (value != _rowSeparator)
                {
                    _rowSeparator = value;
                    OnPropertyChanged("RowSeparator");
                }
            }
        }

        [TaskPane("ColumnSeparatorCaption", "ColumnSeparatorTooltip", null, 1, false, ControlType.TextBox)]
        public string ColumnSeparator
        {
            get => _columnSeparator;
            set
            {
                if (value != _columnSeparator)
                {
                    _columnSeparator = value;
                    OnPropertyChanged("ColumnSeparator");
                }
            }
        }

        [TaskPane("ColumnIDCaption", "ColumnIDTooltip", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, 999)]
        public int ComlumnID
        {
            get => _columndID;
            set
            {
                if (value != _columndID)
                {
                    _columndID = value;
                    OnPropertyChanged("ComlumnID");
                }
            }
        }

        [TaskPane("ResultSeparatorCaption", "ResultSeparatorTooltip", null, 3, false, ControlType.TextBox)]
        public string ResultSeparator
        {
            get => _resultSeparator;
            set
            {
                if (value != _resultSeparator)
                {
                    _resultSeparator = value;
                    OnPropertyChanged("ResultSeparator");
                }
            }
        }

        #region INotifyPropertyChanged Member

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        protected void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        public event StatusChangedEventHandler OnPluginStatusChanged;
        private void ChangePluginIcon(int iconIndex)
        {
            if (OnPluginStatusChanged != null)
            {
                OnPluginStatusChanged(null, new StatusEventArgs(StatusChangedMode.ImageUpdate, iconIndex));
            }
        }

        #endregion
    }
}
