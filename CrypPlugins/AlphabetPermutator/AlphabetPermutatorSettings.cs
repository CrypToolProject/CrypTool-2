/*                              
   Copyright 2022 Nils Kopal, CrypTool Project

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
using System;
using System.ComponentModel;
using System.Windows;

namespace AlphabetPermutator
{
    public enum ACAKeyingScheme
    {
        /// <summary>
        /// The plaintext alphabet is keyed, the ciphertext alphabet is not
        /// </summary>
        K1,
        /// <summary>
        /// The plaintext alphabet is not keyed, the ciphertext alphabet is
        /// </summary>
        K2,
        /// <summary>
        /// Both alphabets are keyed using the same keyword
        /// </summary>
        K3,
        /// <summary>
        /// Both alphabets are keyed using different keywords
        /// </summary>
        K4
    }

    public enum AlphabetsOutputFormat
    {
        /// <summary>
        /// Alphabets are output in the format they were generated
        /// </summary>
        ACAStyle,
        /// <summary>
        /// Alphabets are normalized, meaning that the plaintext alphabet is sorted alphabetically and the ciphertext alphabet is sorted accordingly
        /// </summary>
        Normalized        
    }

    public enum AlphabetOrder
    {
        Ascending,
        Descending,
        LeaveAsIs
    }

    public class AlphabetPermutatorSettings : ISettings
    {
        private ACAKeyingScheme _ACAKeyingScheme = ACAKeyingScheme.K1;
        private AlphabetsOutputFormat _AlphabetsOutputFormat = AlphabetsOutputFormat.ACAStyle;
        private AlphabetOrder _plaintextAlphabetOrder = AlphabetOrder.Ascending;
        private AlphabetOrder _ciphertextAlphabetOrder = AlphabetOrder.Ascending;
        private int _Shift = 0;
        private int _Shift2 = 0;
        private string _Keyword = string.Empty;
        private string _Keyword2 = string.Empty;

        /// <summary>
        /// This event is needed in order to render settings elements visible/invisible
        /// </summary>
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public AlphabetPermutatorSettings()
        {

        }

        #region INotifyPropertyChanged Members

        [TaskPane("ACAKeyingSchemeCaption", "ACAKeyingSchemeTooltip", null, 0, false, ControlType.ComboBox, new[] { "K1", "K2", "K3", "K4" })]
        public ACAKeyingScheme ACAKeyingScheme
        {
            get => _ACAKeyingScheme;
            set
            {
                _ACAKeyingScheme = value;                
                OnPropertyChanged("ACAKeyingScheme");
                UpdateSettingsVisibility();
            }
        }

        [TaskPane("AlphabetsOutputFormatCaption", "AlphabetsOutputFormatTooltip", null, 1, false, ControlType.ComboBox, new[] { "ACAStyle", "Normalized" })]
        public AlphabetsOutputFormat AlphabetsOutputFormat
        {
            get => _AlphabetsOutputFormat;
            set
            {
                _AlphabetsOutputFormat = value;
                OnPropertyChanged("AlphabetsOutputFormat");
            }
        }

        [TaskPane("PlaintextAlphabetOrderCaption", "PlaintextAlphabetOrderTooltip", "PlaintextAlphabetGroup", 2, false, ControlType.ComboBox, new[] { "Ascending", "Descending", "LeaveAsIs" })]
        public AlphabetOrder PlaintextAlphabetOrder
        {
            get => _plaintextAlphabetOrder;
            set
            {
                _plaintextAlphabetOrder = value;
                OnPropertyChanged("PlaintextAlphabetOrder");
            }
        }

        [TaskPane("ShiftCaption", "ShiftTooltip", "PlaintextAlphabetGroup", 3, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue - 1)]
        public int Shift
        {
            get => _Shift;
            set
            {
                _Shift = value;
                OnPropertyChanged("Shift");
            }
        }

        [TaskPane("KeywordCaption", "KeywordTooltip", "PlaintextAlphabetGroup", 4, false, ControlType.TextBox)]
        public string Keyword
        {
            get => _Keyword;
            set
            {
                _Keyword = value;
                OnPropertyChanged("Keyword");
            }
        }

        [TaskPane("CiphertextAlphabetOrderCaption", "CiphertextAlphabetOrderTooltip", "CiphertextAlphabetGroup", 5, false, ControlType.ComboBox, new[] { "Ascending", "Descending", "LeaveAsIs" })]
        public AlphabetOrder CiphertextAlphabetOrder
        {
            get => _ciphertextAlphabetOrder;
            set
            {
                _ciphertextAlphabetOrder = value;
                OnPropertyChanged("CiphertextAlphabetOrder");
            }
        }     

        [TaskPane("Shift2Caption", "Shift2Tooltip", "CiphertextAlphabetGroup", 6, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue - 1)]
        public int Shift2
        {
            get => _Shift2;
            set
            {
                _Shift2 = value;
                OnPropertyChanged("Shift2");
            }
        }       

        [TaskPane("Keyword2Caption", "Keyword2Tooltip", "CiphertextAlphabetGroup", 7, false, ControlType.TextBox)]
        public string Keyword2
        { 
            get => _Keyword2;
            set
            {
                _Keyword2 = value;
                OnPropertyChanged("Keyword2");
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void Initialize()
        {
            UpdateSettingsVisibility();
        }

        private void UpdateSettingsVisibility()
        {           
            switch (_ACAKeyingScheme)
            {
                case ACAKeyingScheme.K1:
                    ShowSettingsElement("Keyword");
                    HideSettingsElement("Keyword2");
                    break;
                case ACAKeyingScheme.K2:
                    HideSettingsElement("Keyword");
                    ShowSettingsElement("Keyword2");
                    break;
                case ACAKeyingScheme.K3:
                    ShowSettingsElement("Keyword");
                    HideSettingsElement("Keyword2");
                    break;
                case ACAKeyingScheme.K4:
                    ShowSettingsElement("Keyword");
                    ShowSettingsElement("Keyword2");
                    break;
            }
        }

        private void ShowSettingsElement(string element)
        {
            TaskPaneAttributeChanged?.Invoke(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Visible)));
        }

        private void HideSettingsElement(string element)
        {
            TaskPaneAttributeChanged?.Invoke(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Collapsed)));
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}
