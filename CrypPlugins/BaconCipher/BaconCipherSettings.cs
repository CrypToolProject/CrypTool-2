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

using CrypTool.BaconCipher.Properties;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;
using System.Windows;

namespace CrypTool.BaconCipher
{
    public class BaconCipherSettings : ISettings
    {
        #region Private Variables

        private BaconianMode _cipher;
        private string _alphabet = Resources.DefaultAlphabet;
        private bool _dynamicCodeLength;
        private OutputTypes _outputMode = OutputTypes.RandomChar;
        private int _codeLength = 5;

        public enum BaconianMode
        {
            Encrypt = 0,
            Decrypt = 1
        }

        public enum OutputTypes
        {
            Binary = 0,
            RandomChar = 1,
            ExternalInput = 2
        }

        #endregion

        #region TaskPane Settings

        [PropertySaveOrder(20)]
        [TaskPane("Alphabet", "AlphabetTooltip", null, 1, false,
            ControlType.TextBox)]
        public string Alphabet
        {
            get => _alphabet;
            set
            {
                if (_alphabet == value)
                {
                    return;
                }

                _alphabet = value;
                OnPropertyChanged(nameof(Alphabet));
            }
        }

        [PropertySaveOrder(30)]
        [TaskPane("Mode", "ModeTooltip", null, 2, false,
            ControlType.ComboBox, new string[] { "ModeEncrypt", "ModeDecrypt" })]
        public BaconianMode Cipher
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

        [PropertySaveOrder(50)]
        [TaskPane("OutputModeCaption", "OutputModeToolTip", null, 3, false, ControlType.ComboBox,
            new string[] { "OutputModeBinary", "OutputModeRandomChar", "OutputModeExternalInput" })]
        public OutputTypes OutputMode
        {
            get => _outputMode;
            set
            {
                if (_outputMode == value)
                {
                    return;
                }

                _outputMode = value;
                OnPropertyChanged(nameof(OutputMode));
            }
        }


        [PropertySaveOrder(40)]
        [TaskPane("DynamicCodeLengthCaption", "DynamicCodeLengthToolTip", "CodeLength", 4, false, ControlType.CheckBox)]
        public bool DynamicCodeLength
        {
            get => _dynamicCodeLength;
            set
            {
                if (_dynamicCodeLength == value)
                {
                    return;
                }

                _dynamicCodeLength = value;
                OnPropertyChanged(nameof(DynamicCodeLength));
            }
        }

        [PropertySaveOrder(60)]
        [TaskPane("CodeLengthCaption", "CodeLengthToolTip", "CodeLength", 5, false, ControlType.NumericUpDown,
            ValidationType.RangeInteger, 0, 100)]
        public int CodeLength
        {
            get => _codeLength;
            set
            {
                if (_codeLength == value)
                {
                    return;
                }

                _codeLength = value;
                OnPropertyChanged(nameof(CodeLength));
            }
        }


        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;


        private void OnPropertyChanged(string propertyName)
        {
            UpdateTaskPaneVisibility();
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        internal void UpdateTaskPaneVisibility()
        {
            if (TaskPaneAttributeChanged == null)
            {
                return;
            }

            if (DynamicCodeLength)
            {
                TaskPaneAttributeChanged(this,
                    new TaskPaneAttributeChangedEventArgs(
                        new TaskPaneAttribteContainer(nameof(CodeLength), Visibility.Collapsed)));
            }
            else if (!DynamicCodeLength)
            {
                TaskPaneAttributeChanged(this,
                    new TaskPaneAttributeChangedEventArgs(
                        new TaskPaneAttribteContainer(nameof(CodeLength), Visibility.Visible)));
            }
        }


        private void SetSettingVisible(string element)
        {
            TaskPaneAttributeChanged?.Invoke(this,
                new TaskPaneAttributeChangedEventArgs(
                    new TaskPaneAttribteContainer(element, Visibility.Visible)));
        }

        private void SetSettingHidden(string element)
        {
            TaskPaneAttributeChanged?.Invoke(this,
                new TaskPaneAttributeChangedEventArgs(
                    new TaskPaneAttribteContainer(element, Visibility.Collapsed)));
        }

        #endregion

        public void Initialize()
        {
        }
    }
}