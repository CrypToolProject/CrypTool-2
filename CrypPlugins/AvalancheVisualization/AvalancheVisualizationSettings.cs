/*
   Copyright 2011 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using System.Windows;

namespace CrypTool.Plugins.AvalancheVisualization
{
    public class AvalancheVisualizationSettings : ISettings
    {
        #region Private Variables

        private int _keyLength;
        private int _prepSelection;
        private int _unprepSelection;
        private int _contrast;
        private Category _selectedCategory;

        #endregion

        public enum Category
        {
            Prepared = 0,
            Unprepared = 1,
        };

        #region TaskPane Settings       

        [TaskPane("Category", "CategoryTooltip", "GroupName", 0, false, ControlType.ComboBox, new string[] { "PreparedCaption", "UnpreparedCaption" })]
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {

                if (value != _selectedCategory)
                {
                    _selectedCategory = value;
                    OnPropertyChanged("SelectedCategory");
                    EnableDisableSettingsElements();
                }
            }
        }

        [TaskPane("Selection", "SelectionTooltipPrep", "GroupName", 1, false, ControlType.ComboBox, new string[] { "AES", "DES" })]
        public int PrepSelection
        {
            get => _prepSelection;
            set
            {

                if (value != _prepSelection)
                {
                    _prepSelection = value;
                    OnPropertyChanged("PrepSelecction");
                    EnableDisableSettingsElements();
                }
            }
        }

        [TaskPane("Selection", "SelectionTooltipUnprep", "GroupName", 2, false, ControlType.ComboBox, new string[] { "HashFunction", "ClassicCipher", "ModernCipher" })]
        public int UnprepSelection
        {
            get => _unprepSelection;
            set
            {

                if (value != _unprepSelection)
                {
                    _unprepSelection = value;
                    OnPropertyChanged("UnprepSelecction");
                    EnableDisableSettingsElements();
                }
            }
        }

        [TaskPane("KeyLength", "KeyLengthTooltip", "GroupName", 2, false, ControlType.ComboBox, new string[] { "128 bits", "192 bits", "256 bits" })]
        public int KeyLength
        {
            get => _keyLength;
            set
            {
                if (value != _keyLength)
                {
                    _keyLength = value;
                    OnPropertyChanged("KeyLength");
                }
            }
        }

        [TaskPane("Contrast", "ContrastTooltip", "GroupName", 3, false, ControlType.ComboBox, new string[] { "red_green", "black_white" })]
        public int Contrast
        {
            get => _contrast;
            set
            {
                if (value != _contrast)
                {
                    _contrast = value;
                    OnPropertyChanged("Contrast");
                }
            }
        }

        #endregion

        #region Private methods

        private void EnableDisableSettingsElements()
        {
            switch (SelectedCategory)
            {
                case Category.Unprepared:
                    DisableSettingsElements("KeyLength");
                    DisableSettingsElements("PrepSelection");
                    EnableSettingsElements("UnprepSelection");
                    break;

                case Category.Prepared:
                    DisableSettingsElements("UnprepSelection");
                    EnableSettingsElements("PrepSelection");
                    if (_prepSelection == 0)
                    {
                        EnableSettingsElements("KeyLength");
                    }
                    else
                    {
                        DisableSettingsElements("KeyLength");
                    }
                    break;

                default:
                    break;
            }
        }
        #endregion

        #region Events and Event handlers

        public event PropertyChangedEventHandler PropertyChanged;
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        private void EnableSettingsElements(string element)
        {
            TaskPaneAttributeChanged?.Invoke(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Visible)));
        }

        private void DisableSettingsElements(string element)
        {
            TaskPaneAttributeChanged?.Invoke(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Collapsed)));
        }

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        #endregion

        public void Initialize()
        {
            EnableDisableSettingsElements();
        }
    }
}
