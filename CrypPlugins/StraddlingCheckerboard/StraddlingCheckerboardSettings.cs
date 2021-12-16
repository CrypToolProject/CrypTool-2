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
using System.ComponentModel;

namespace CrypTool.StraddlingCheckerboard
{
    public class StraddlingCheckerboardSettings : ISettings
    {
        #region Private Variables

        private string _alphabet;
        private string _rowsColumns;
        private StraddlingCheckerBoardMode _cipher;

        public enum StraddlingCheckerBoardMode
        {
            Encrypt = 0,
            Decrypt = 1
        }

        #endregion

        #region TaskPane Settings

        [PropertySaveOrder(20)]
        [TaskPane("ContentCaption", "ContentTooltip", null, 1, false, ControlType.TextBox)]
        public string Content
        {
            get => _alphabet ?? string.Empty;
            set
            {
                if (_alphabet == value)
                {
                    return;
                }

                _alphabet = value;
                OnPropertyChanged(nameof(Content));
            }
        }

        [PropertySaveOrder(30)]
        [TaskPane("RowsColumnsCaption", "RowsColumnsTooltip", null, 2, false, ControlType.TextBox)]
        public string RowsColumns
        {
            get => _rowsColumns ?? string.Empty;
            set
            {
                if (_rowsColumns == value)
                {
                    return;
                }

                _rowsColumns = value;
                OnPropertyChanged(nameof(RowsColumns));
            }
        }

        [PropertySaveOrder(50)]
        [TaskPane("ModeCaption", "ModeTooltip", null, 4, false,
            ControlType.ComboBox, new string[] { "ModeActionEncrypt", "ModeActionDecrypt" })]
        public StraddlingCheckerBoardMode Cipher
        {
            get => _cipher;
            set
            {
                if (_cipher == value)
                {
                    return;
                }

                _cipher = value;
                OnPropertyChanged(nameof(Cipher));
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