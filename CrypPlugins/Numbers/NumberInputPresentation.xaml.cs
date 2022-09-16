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

using CrypTool.PluginBase.Miscellaneous;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using UserControl = System.Windows.Controls.UserControl;

namespace CrypTool.Plugins.Numbers
{
    /// <summary>
    /// Interaction logic for NumberInputPresentation.xaml
    /// </summary>
    public partial class NumberInputPresentation : UserControl, INotifyPropertyChanged
    {
        private FontFamily _fontFamily;
        private double _fontSize;

        public event System.Windows.Input.KeyEventHandler UserKeyDown;

        public NumberInputPresentation(FontFamily defaultFontFamily, double defaultFontSize)
        {
            InitializeComponent();
            Height = double.NaN;
            Width = double.NaN;
            DataContext = this;
            _fontFamily = defaultFontFamily;
            _fontSize = defaultFontSize;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            char c = e.Text.ToUpper()[0];
            if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || "+-*/^ ()#!%,".Contains(c)))
            {
                e.Handled = true;
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, propertyName);
        }        

        public FontFamily MyFontFamily
        {
            get => _fontFamily;
            set
            {
                _fontFamily = value;
                OnPropertyChanged("MyFontFamily");
            }
        }        

        public double MyFontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                OnPropertyChanged("MyFontSize");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NumberInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((e.Key == Key.Add || e.Key == Key.Subtract) && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                if (UserKeyDown != null)
                {
                    UserKeyDown.Invoke(sender, e);
                }
            }
        }
    }
}
