/*
   Copyright (C) CrypTool 2 Team

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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using WorkspaceManager.View.Visuals;


namespace WorkspaceManager.View.VisualComponents
{
    /// <summary>
    /// Interaction logic for TextEditPanel.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("WorkspaceManager.Properties.Resources")]
    public partial class TextEditPanel : UserControl
    {
        private EditorVisual editor;

        public TextEditPanel()
        {
            DataContextChanged += new DependencyPropertyChangedEventHandler(TextEditPanelDataContextChanged);
            InitializeComponent();
        }

        private void TextEditPanelDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is EditorVisual)
            {
                editor = (EditorVisual)e.NewValue;
                editor.SelectedTextChanged += (x, s) =>
                {
                    if (editor.SelectedText == null)
                    {
                        return;
                    }

                    CrPicker.SelectedColor = editor.SelectedText.Color.Color;
                };
            }
        }

        private void CrPickerSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            if (editor.SelectedText == null)
            {
                return;
            }

            editor.SelectedText.Color = new SolidColorBrush(e.NewValue);
        }
    }

    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Color c;
            SolidColorBrush b;
            if (value is Color)
            {
                c = (Color)value;
                b = new SolidColorBrush(c);
                return b;
            }
            SolidColorBrush solidColorBrush = value as SolidColorBrush;
            if (solidColorBrush != null)
            {
                b = solidColorBrush;
                c = b.Color;
                return c;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //throw new NotImplementedException();
            return null;
        }
    }

}
