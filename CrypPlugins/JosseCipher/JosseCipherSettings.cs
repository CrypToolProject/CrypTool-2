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

using System.ComponentModel;
using CrypTool.JosseCipher.Properties;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.JosseCipher
{
    public class JosseCipherSettings : ISettings
    {
        #region Private Variables

        private string _keyword = string.Empty;
        private string _alphabet = Resources.DefaultAlphabet;

        private JosseCipherMode _cipher;

        public enum JosseCipherMode
        {
            Encrypt = 0,
            Decrypt = 1
        }

        #endregion

        #region TaskPane Settings

        [PropertySaveOrder(10)]
        [TaskPane("Keyword", "KeywordToolTip", null, 30, false, ControlType.TextBox)]
        public string Keyword
        {
            get => _keyword ?? string.Empty;
            set
            {
                if (_keyword == value) return;
                _keyword = value;
                OnPropertyChanged(nameof(Keyword));
            }
        }

        [PropertySaveOrder(20)]
        [TaskPane("Alphabet", "AlphabetTooltip", null, 20, false, ControlType.TextBox)]
        public string Alphabet
        {
            get => _alphabet ?? string.Empty;
            set
            {
                if(_alphabet == value) return;
                _alphabet = value;
                OnPropertyChanged(nameof(Alphabet));
            }
        }

        [PropertySaveOrder(30)]
        [TaskPane("Mode", "ModeTooltip", null, 10, false,
            ControlType.ComboBox, new string[] { "ModeActionEncrypt", "ModeActionDecrypt" })]
        public JosseCipherMode Cipher {
            get => _cipher;
            set
            {
                if (_cipher == value) return;
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
