/*                              
   Copyright 2009 Team CrypTool (Sven Rech,Dennis Nolte,Raoul Falk,Nils Kopal), Uni Duisburg-Essen

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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace CrypTool.Plugins.Numbers
{
    public class NumberInputSettings : ISettings
    {
        public static string DEFAULT_FONT_FAMILY = "Segoe UI";  // default CT2 font

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;
        private ObservableCollection<string> fonts = new ObservableCollection<string>();
        private bool manualFontSettings = false;
        private string _font;
        private double fontsize;

        #region Number
        private string number = "";
        public string Number
        {
            get => number;
            set
            {
                number = value;
                OnPropertyChanged("Number");
            }
        }
        #endregion

        [DontSave]
        public ObservableCollection<string> Fonts
        {
            get => fonts;
            set
            {
                if (value != fonts)
                {
                    fonts = value;
                    OnPropertyChanged("Fonts");
                }
            }
        }

        public NumberInputSettings()
        {
            Fonts.Clear();
            ResetFont();
        }

        public void ResetFont()
        {
            foreach (System.Windows.Media.FontFamily font in System.Windows.Media.Fonts.SystemFontFamilies)
            {
                Fonts.Add(font.ToString());
                if (PluginBase.Properties.Settings.Default.FontFamily == font)
                {
                    _font = font.ToString();
                }
            }
            fontsize = PluginBase.Properties.Settings.Default.FontSize;
            OnPropertyChanged("Font");
            OnPropertyChanged("FontSize");
        }


        #region ShowDigits
        private bool showDigits = true;
        [TaskPane("ShowDigitsCaption", "ShowDigitsTooltip", "ShowDigitsGroup", 1, true, ControlType.CheckBox, "", null)]
        public bool ShowDigits
        {
            get => showDigits;
            set
            {
                if (value != showDigits)
                {
                    showDigits = value;
                    OnPropertyChanged("ShowDigits");
                }
            }
        }

        [TaskPane("ManualFontSettingsCaption", "ManualFontSettingsTooltip", "FontGroup", 3, true, ControlType.CheckBox, "")]
        public bool ManualFontSettings
        {
            get => manualFontSettings;
            set
            {
                if (value != manualFontSettings)
                {

                    if (value == false)
                    {
                        CollapseSettingsElement("Font");
                        CollapseSettingsElement("FontSize");
                        ResetFont();
                    }
                    else
                    {
                        ShowSettingsElement("Font");
                        ShowSettingsElement("FontSize");
                    }
                    manualFontSettings = value;
                    OnPropertyChanged("ManualFontSettings");
                }
            }
        }

        [TaskPane("FontCaption", "FontTooltip", "FontGroup", 4, true, ControlType.DynamicComboBox, new string[] { "Fonts" })]
        public int Font
        {
            get
            {
                int fontIndex = Fonts.IndexOf(_font);
                if (fontIndex != -1)
                {
                    return fontIndex;
                }
                else
                {
                    return Fonts.IndexOf(DEFAULT_FONT_FAMILY); //standard CT2 font type
                }
            }
            set
            {
                if (manualFontSettings && value < Fonts.Count && Fonts[value] != _font)
                {
                    _font = Fonts[value];
                    OnPropertyChanged("Font");
                }
            }
        }

        [TaskPane("FontSizeCaption", "FontSizeTooltip", "FontGroup", 5, true, ControlType.NumericUpDown, ValidationType.RangeInteger, 8, 72)]
        public double FontSize
        {
            get => fontsize;
            set
            {
                if (value != fontsize)
                {
                    if (manualFontSettings)
                    {
                        fontsize = value;
                    }
                    OnPropertyChanged("FontSize");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {
            if (manualFontSettings)
            {
                ShowSettingsElement("Font");
                ShowSettingsElement("FontSize");
            }
            else
            {
                CollapseSettingsElement("Font");
                CollapseSettingsElement("FontSize");
            }
        }

        private void ShowSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Visible)));
            }
        }

        private void CollapseSettingsElement(string element)
        {
            if (TaskPaneAttributeChanged != null)
            {
                TaskPaneAttributeChanged(this, new TaskPaneAttributeChangedEventArgs(new TaskPaneAttribteContainer(element, Visibility.Collapsed)));
            }
        }


        private void OnPropertyChanged(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }

        #endregion
    }
}
