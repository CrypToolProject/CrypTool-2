/*
   Copyright 2014 Nils Rehwald 

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

namespace CrypTool.Plugins.WatermarkCreator
{
    public class WatermarkCreatorSettings : ISettings
    {
        #region Private Variables

        private int _watermarkAlgorithm = 0;
        private int _textSize = 50;
        private int _font = 3;
        private int _location = 1;
        private int _opacity = 1000;
        private int _boxSize = 10;
        private readonly int _errorCorrection = 0;
        private long _s1 = 19;
        private long _s2 = 24;
        private int _locPercentage = 0;
        private int _advanced = 0;

        #endregion

        #region TaskPane Settings

        [TaskPane("ModificationTypeCap", "ModificationTypeDes", null, 0, false, ControlType.ComboBox, new string[] { "WatermarkCreatorSettings_ModificationType_EmbedText", "WatermarkCreatorSettings_ModificationType_EmbedInvisibleText", "WatermarkCreatorSettings_ModificationType_ExtractText" })]
        public int ModificationType
        {
            get => _watermarkAlgorithm;
            set
            {
                if (_watermarkAlgorithm != value)
                {
                    _watermarkAlgorithm = value;
                    OnPropertyChanged("ModificationType");
                }
            }
        }


        [TaskPane("TextSizeMaxCap", "TextSizeMaxDes", null, 10, false, ControlType.TextBox)]
        public int TextSizeMax
        {
            get => _textSize;
            set
            {
                if (_textSize != value)
                {
                    _textSize = value;
                    OnPropertyChanged("TextSizeMax");
                }
            }
        }

        [TaskPane("FontTypeCap", "FontTypeDes", null, 11, false, ControlType.ComboBox, new string[] { "Aharoni",
            "Andalus", "Arabic Typesetting", "Arial", "Arial Black", "Calibri", "Buxton Sketch",
        "Cambria Math", "Comic Sans MS", "DFKai-SB", "Franklin Gothic Medium", "Lucida Console",
        "Simplified Arabic", "SketchFlow Print", "Symbol", "Times New Roman", "Traditional Arabic",
        "Webdings", "Wingdings"})]
        public int FontType
        {
            get => _font;
            set
            {
                if (_font != value)
                {
                    _font = value;
                    OnPropertyChanged("FontType");
                }
            }
        }

        [TaskPane("WatermarkLocationCap", "WatermarkLocationDes", null, 12, false, ControlType.ComboBox, new string[] { "TopLoc", "BotLoc", "OtherLoc" })]
        public int WatermarkLocation
        {
            get => _location;
            set
            {
                if (_location != value)
                {
                    _location = value;
                    OnPropertyChanged("WatermarkLocation");
                }
            }
        }

        [TaskPane("LocationPercentageCap", "LocationPercentageDes", null, 13, true, ControlType.Slider, 5, 95)]
        public int LocationPercentage
        {
            get => _locPercentage;
            set
            {
                if (_locPercentage != value)
                {
                    _locPercentage = value;
                    OnPropertyChanged("LocationPercentage");
                }
            }
        }

        [TaskPane("OpacityCap", "OpacityDes", null, 11, false, ControlType.TextBox)]
        public int Opacity
        {
            get => _opacity;
            set
            {
                if (_opacity != value)
                {
                    _opacity = value;
                    OnPropertyChanged("Opacity");
                }
            }
        }

        [TaskPane("BoxSizeCap", "BoxSizeDes", null, 10, false, ControlType.TextBox)]
        public int BoxSize
        {
            get => _boxSize;
            set
            {
                if (_boxSize != value)
                {
                    _boxSize = value;
                    OnPropertyChanged("BoxSize");
                }
            }
        }

        [TaskPane("Seed1", "Seed", null, 14, false, ControlType.TextBox)]
        public long Seed1
        {
            get => _s1;
            set
            {
                if (_s1 != value)
                {
                    _s1 = value;
                    OnPropertyChanged("Seed1");
                }
            }
        }

        [TaskPane("Seed2", "Seed", null, 15, false, ControlType.TextBox)]
        public long Seed2
        {
            get => _s2;
            set
            {
                if (_s2 != value)
                {
                    _s2 = value;
                    OnPropertyChanged("Seed2");
                }
            }
        }

        [TaskPane("AdvancedModeCap", "AdvancedModeDes", null, 5, false, ControlType.ComboBox, new string[] { "AdvancedModeList1", "AdvancedModeList2" })]
        public int AdvancedMode
        {
            get => _advanced;
            set
            {
                if (_advanced != value)
                {
                    _advanced = value;
                    OnPropertyChanged("AdvancedMode");
                }
            }
        }

        //Managing visibility of options
        public void UpdateTaskPaneVisibility()
        {
            SettingChanged("ModificationType", Visibility.Visible);
            SettingChanged("TextSizeMax", Visibility.Collapsed);
            SettingChanged("FontType", Visibility.Collapsed);
            SettingChanged("WatermarkLocation", Visibility.Collapsed);
            SettingChanged("Opacity", Visibility.Collapsed);
            SettingChanged("BoxSize", Visibility.Collapsed);
            SettingChanged("Seed1", Visibility.Collapsed);
            SettingChanged("Seed2", Visibility.Collapsed);
            SettingChanged("LocationPercentage", Visibility.Collapsed);
            SettingChanged("AdvancedMode", Visibility.Collapsed);

            switch (ModificationType)
            {
                case 0: //Visible Watermark (embedding)
                    SettingChanged("TextSizeMax", Visibility.Visible);
                    SettingChanged("FontType", Visibility.Visible);
                    SettingChanged("WatermarkLocation", Visibility.Visible);
                    break;
                case 1: //Invisible Watermark (embedding)
                    SettingChanged("AdvancedMode", Visibility.Visible);
                    break;
                case 2: //Invisible Watermark (extracting)
                    SettingChanged("AdvancedMode", Visibility.Visible);
                    break;
            }

            switch (WatermarkLocation)
            {
                case 2:
                    SettingChanged("LocationPercentage", Visibility.Visible);
                    break;
                default:
                    SettingChanged("LocationPercentage", Visibility.Collapsed);
                    break;
            }

            //only show this settings for invisible water marks and when we are in "advanced mode"
            if (AdvancedMode == 1 && ModificationType > 0)
            {
                SettingChanged("Opacity", Visibility.Visible);
                SettingChanged("BoxSize", Visibility.Visible);
                SettingChanged("Seed1", Visibility.Visible);
                SettingChanged("Seed2", Visibility.Visible);
            }
        }
        private void SettingChanged(string setting, Visibility vis)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(setting, vis)));
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }

        public void Initialize()
        {
            UpdateTaskPaneVisibility();
        }

        #endregion
    }
}
