/*
   Copyright 2008 Thomas Schmid, University of Siegen

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
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace TextOutput
{
    public class TextOutputSettings : ISettings
    {
        public static string DEFAULT_FONT_FAMILY = "Segoe UI";  // default CT2 font

        public enum LineBreaksEnum
        {
            DontChange,
            Windows,
            UNIX
        }

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


        #region Private variables

        private ObservableCollection<string> fonts = new ObservableCollection<string>();
        private const int maxmaxLength = int.MaxValue;
        private int maxLength = 15728640; //15 Megabyte
        private string _font;
        private double fontsize;
        private bool manualFontSettings = false;

        #endregion

        public TextOutputSettings(TextOutput textOutput)
        {
            Fonts.Clear();
            ResetFont();
            if (textOutput == null)
            {
                throw new ArgumentException("textOutput");
            }
        }

        public void ResetFont()
        {
            foreach (System.Windows.Media.FontFamily font in System.Windows.Media.Fonts.SystemFontFamilies)
            {
                Fonts.Add(font.ToString());
                if (CrypTool.PluginBase.Properties.Settings.Default.FontFamily == font)
                {
                    _font = font.ToString();
                }
            }
            fontsize = CrypTool.PluginBase.Properties.Settings.Default.FontSize;
            OnPropertyChanged("Font");
            OnPropertyChanged("FontSize");
        }

        #region settings

        /// <summary>
        /// Maximum size property used in the settings pane.
        /// </summary>
        [TaskPane("MaxLengthCaption", "MaxLengthTooltip", null, 0, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 1, maxmaxLength)]
        public int MaxLength
        {
            get => maxLength;
            set
            {
                if (value != maxLength)
                {
                    maxLength = value;
                    OnPropertyChanged("MaxLength");
                }
            }
        }

        private bool append = false;
        [TaskPane("AppendCaption", "AppendTooltip", null, 1, false, ControlType.CheckBox, "", null)]
        public bool Append
        {
            get => append;
            set
            {
                if (value != append)
                {
                    append = value;
                }
            }
        }

        private int appendBreaks = 1;
        [TaskPane("AppendBreaksCaption", "AppendBreaksTooltip", null, 2, false, ControlType.NumericUpDown, ValidationType.RangeInteger, 0, int.MaxValue)]
        public int AppendBreaks
        {
            get => appendBreaks;
            set
            {
                if (value != appendBreaks)
                {
                    appendBreaks = value;
                }
            }
        }

        private LineBreaksEnum linebreaks = LineBreaksEnum.DontChange;
        [TaskPane("LineBreaksCaption", "LineBreaksTooltip", null, 3, false, ControlType.ComboBox, new string[] { "DontChange", "Windows", "Unix" })]
        public LineBreaksEnum LineBreaks
        {
            get => linebreaks;
            set
            {
                if (value != linebreaks)
                {
                    linebreaks = value;
                }
            }
        }

        private bool showChars = true;
        [TaskPane("ShowCharsCaption", "ShowCharsTooltip", "ShowCharsGroup", 4, true, ControlType.CheckBox, "", null)]
        public bool ShowChars
        {
            get => showChars;
            set
            {
                if (value != showChars)
                {
                    showChars = value;
                    OnPropertyChanged("ShowChars");
                }
            }
        }

        private bool showLines = true;
        [TaskPane("ShowLinesCaption", "ShowLinesTooltip", "ShowCharsGroup", 5, true, ControlType.CheckBox, "", null)]
        public bool ShowLines
        {
            get => showLines;
            set
            {
                if (value != showLines)
                {
                    showLines = value;
                    OnPropertyChanged("ShowLines");
                }
            }
        }

        private bool showDigits = false;
        [TaskPane("ShowDigitsCaption", "ShowDigitsTooltip", "ShowCharsGroup", 6, true, ControlType.CheckBox, "", null)]
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

        private int showChanges = 0;
        [TaskPane("ShowChangesCaption", "ShowChangesTooltip", "ChangesGroup", 7, true, ControlType.ComboBox, new string[] { "DontShow", "ShowOnlyDifferences", "ShowInsertsAndDeletions", "ShowChangedSymbols" })]
        public int ShowChanges
        {
            get => showChanges;
            set
            {
                if (value != showChanges)
                {
                    showChanges = value;
                    OnPropertyChanged("ShowChanges");
                }
            }
        }

        [TaskPane("ManualFontSettingsCaption", "ManualFontSettingsTooltip", "FontGroup", 8, true, ControlType.CheckBox, "")]
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

        [TaskPane("FontCaption", "FontTooltip", "FontGroup", 9, true, ControlType.DynamicComboBox, new string[] { "Fonts" })]
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

        [TaskPane("FontSizeCaption", "FontSizeTooltip", "FontGroup", 10, true, ControlType.NumericUpDown, ValidationType.RangeInteger, 8, 72)]
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

        # endregion settings

        public event PropertyChangedEventHandler PropertyChanged;
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

        public event TaskPaneAttributeChangedHandler TaskPaneAttributeChanged;

        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
            }
        }
    }
}