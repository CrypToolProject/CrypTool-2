/*                              
   Copyright 2010 Nils Kopal, Viktor M.

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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CrypTool.PluginBase.Attributes;
using WorkspaceManager.Model;

namespace WorkspaceManager
{
    /// <summary>
    /// Interaction logic for WorkspaceManagerSettingsTab.xaml
    /// </summary>
    [Localization("WorkspaceManager.Properties.Resources")]
    [SettingsTab("WorkspaceManagerSettings", "/MainSettings/", 1.1)]
    public partial class WorkspaceManagerSettingsTab : UserControl
    {
        private readonly ICollection<System.Windows.Media.FontFamily> _fontFamilies;
        private readonly List<double> _fontSizes;

        public ICollection<FontFamily> FontFamilies => _fontFamilies;

        public List<double> FontSizes => _fontSizes;

        public WorkspaceManagerSettingsTab(Style settingsStyle)
        {
            _fontFamilies = Fonts.SystemFontFamilies;
            _fontSizes = new List<double>();
            for (int i = 3; i < 64; i++)
            {
                _fontSizes.Add(i);
            }
            Resources.Add("settingsStyle", settingsStyle);
            InitializeComponent();

            if (CrypTool.PluginBase.Properties.Settings.Default.FontFamily != null)
            {
                FontFamilyComboBox.SelectedItem = _fontFamilies.First(x => Equals(CrypTool.PluginBase.Properties.Settings.Default.FontFamily, x));
            }
            else
            {
                FontFamilyComboBox.SelectedItem = _fontFamilies.First(x => Equals(FontFamily, x));
            }
            FontSizeComboBox.SelectedItem = CrypTool.PluginBase.Properties.Settings.Default.FontSize;

            InitializeColors();
            CrypTool.PluginBase.Properties.Settings.Default.PropertyChanged += delegate { CrypTool.PluginBase.Miscellaneous.ApplicationSettingsHelper.SaveApplicationsSettings(); };
        }

        private void InitializeColors()
        {
            IntegerColor.SelectedColor = (ColorHelper.IntegerColor);
            ByteColor.SelectedColor = (ColorHelper.ByteColor);
            DoubleColor.SelectedColor = (ColorHelper.DoubleColor);
            BoolColor.SelectedColor = (ColorHelper.BoolColor);
            StreamColor.SelectedColor = (ColorHelper.StreamColor);
            StringColor.SelectedColor = (ColorHelper.StringColor);
            ObjectColor.SelectedColor = (ColorHelper.ObjectColor);
            BigIntegerColor.SelectedColor = (ColorHelper.BigIntegerColor);
            DefaultColor.SelectedColor = (ColorHelper.DefaultColor);

            AsymmetricColor.SelectedColor = (ColorHelper.AsymmetricColor);
            ClassicColor.SelectedColor = (ColorHelper.ClassicColor);
            SymmetricblockColor.SelectedColor = (ColorHelper.SymmetricColor);
            ToolsColor.SelectedColor = (ColorHelper.ToolsColor);
            SteganographyColor.SelectedColor = (ColorHelper.SteganographyColor);
            HashColor.SelectedColor = (ColorHelper.HashColor);
            AnalysisGenericColor.SelectedColor = (ColorHelper.AnalysisGenericColor);
            AnalysisSpecificColor.SelectedColor = (ColorHelper.AnalysisSpecificColor);
            ProtocolsColor.SelectedColor = (ColorHelper.ProtocolColor);
        }

        private void ResetColorButton_Click(object sender, RoutedEventArgs e)
        {
            ColorHelper.SetDefaultColors();
            InitializeColors();
        }

        private void CrPickerSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            if (sender == IntegerColor)
            {
                ColorHelper.IntegerColor = (IntegerColor.SelectedColor);
            }
            else if (sender == ByteColor)
            {
                ColorHelper.ByteColor = (ByteColor.SelectedColor);
            }
            else if (sender == DoubleColor)
            {
                ColorHelper.DoubleColor = (DoubleColor.SelectedColor);
            }
            else if (sender == BoolColor)
            {
                ColorHelper.BoolColor = (BoolColor.SelectedColor);
            }
            else if (sender == StreamColor)
            {
                ColorHelper.StreamColor = (StreamColor.SelectedColor);
            }
            else if (sender == StringColor)
            {
                ColorHelper.StringColor = (StringColor.SelectedColor);
            }
            else if (sender == ObjectColor)
            {
                ColorHelper.ObjectColor = (ObjectColor.SelectedColor);
            }
            else if (sender == BigIntegerColor)
            {
                ColorHelper.BigIntegerColor = (BigIntegerColor.SelectedColor);
            }
            else if (sender == DefaultColor)
            {
                ColorHelper.DefaultColor = (DefaultColor.SelectedColor);
            }
            else if (sender == AsymmetricColor)
            {
                ColorHelper.AsymmetricColor = (AsymmetricColor.SelectedColor);
            }
            else if (sender == ClassicColor)
            {
                ColorHelper.ClassicColor = (ClassicColor.SelectedColor);
            }
            else if (sender == SymmetricblockColor)
            {
                ColorHelper.SymmetricColor = (SymmetricblockColor.SelectedColor);
            }
            else if (sender == SteganographyColor)
            {
                ColorHelper.SteganographyColor = (SteganographyColor.SelectedColor);
            }
            else if (sender == ProtocolsColor)
            {
                ColorHelper.ProtocolColor = (ProtocolsColor.SelectedColor);
            }
            else if (sender == ToolsColor)
            {
                ColorHelper.ToolsColor = (ToolsColor.SelectedColor);
            }
            else if (sender == HashColor)
            {
                ColorHelper.HashColor = (HashColor.SelectedColor);
            }
            else if (sender == AnalysisGenericColor)
            {
                ColorHelper.AnalysisGenericColor = (AnalysisGenericColor.SelectedColor);
            }
            else if (sender == AnalysisSpecificColor)
            {
                ColorHelper.AnalysisSpecificColor = (AnalysisSpecificColor.SelectedColor);
            }
        }

        private void ButtonResetCCS_Click(object sender, RoutedEventArgs e)
        {
            CrypTool.PluginBase.Editor.ComponentConnectionStatistics.Reset();
        }

        private void FontFamilyComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CrypTool.PluginBase.Properties.Settings.Default.FontFamily = (FontFamily)FontFamilyComboBox.SelectedItem;
        }

        private void FontSizeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CrypTool.PluginBase.Properties.Settings.Default.FontSize = (double)FontSizeComboBox.SelectedItem;
        }
    }
}
