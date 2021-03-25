/*
   Copyright 2014 Olga Groh

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
using System;
using System.ComponentModel;
using System.Windows;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.Plugins.Transcriptor
{
    // HOWTO: rename class (click name, press F2)
    public class TranscriptorSettings : ISettings
    {
        #region Private Variables

        private int rectangleColor;
        private int selectedRectangleColor;
        private int mode = 1;
        private int threshold = 75;
        private String alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private String serializedData = String.Empty;

        #endregion

        #region TaskPane Settings
        
        [TaskPane("RectangleColorCaption", "RectangleColorTooltip", null, 1, false, ControlType.ComboBox, new String[] { "Blue", "Green", "Yellow" })]
        public int RectangleColor
        {
            get
            {
                return rectangleColor;
            }
            set
            {
                if (rectangleColor != value)
                {
                    rectangleColor = value;
                    OnPropertyChanged("RectangleColor");
                }
            }
        }

        [TaskPane("SelectedRectangleColorCaption", "SelectedColorTooltip", null, 2, false, ControlType.ComboBox, new String[] { "Red", "Black", "White" })]
        public int SelectedRectangleColor
        {
            get
            {
                return selectedRectangleColor;
            }
            set
            {
                if (selectedRectangleColor != value)
                {
                    selectedRectangleColor = value;
                    OnPropertyChanged("SelectedRectangleColor");
                }
            }
        }

        [TaskPane("AlphabetCaption", "AlphabetTooltip", null, 3, false, ControlType.TextBoxReadOnly)]
        public String Alphabet
        {
            get
            {
                return alphabet;
            }
            set
            {
                if (alphabet != value)
                {
                    alphabet = value;
                    OnPropertyChanged("Alphabet");
                }
            }

        }

        [TaskPane("ModeCaption", "ModeTooltip", "ModeGroup", 4, false, ControlType.ComboBox, new String[] { "Manually", "SemiAutomatic" })]
        public int Mode
        {
            get
            {
                return mode;
            }
            set
            {
                if (value != mode)
                {
                    mode = value;
                    UpdateTaskPaneVisibility();
                    OnPropertyChanged("Mode");
                }
            }
        }

        [TaskPane("ThresholdCaption", "ThresholdTooltip", "ModeGroup", 6, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, 100)]
        public int Threshold
        {
            get
            {
                return threshold;
            }
            set
            {
                if (threshold != value)
                {
                    threshold = value;
                    OnPropertyChanged("Threshold");
                }
            }
        }

        public string SerializedData
        {
            get
            {
                return serializedData;
            }
            set
            {
                if (!serializedData.Equals(value))
                {
                    serializedData = value;
                    OnPropertyChanged("SerializedData");
                }
            }
        }

        #endregion

        #region Events

        private void UpdateTaskPaneVisibility()
        {
            if (Mode == 1)
            {
                settingChanged("Threshold", Visibility.Visible);
            }
            else
            {
                settingChanged("Threshold", Visibility.Collapsed);
            }
        }

        private void settingChanged(string setting, Visibility visibility)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(setting, visibility)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;
        
        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        public void Initialize()
        {
        }

        #endregion
    }
}
