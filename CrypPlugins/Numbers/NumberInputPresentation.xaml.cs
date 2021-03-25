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

using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using UserControl = System.Windows.Controls.UserControl;
using System.ComponentModel;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.Plugins.Numbers
{
    /// <summary>
    /// Interaction logic for NumberInputPresentation.xaml
    /// </summary>
    public partial class NumberInputPresentation : UserControl, INotifyPropertyChanged
    {
        public event System.Windows.Input.KeyEventHandler UserKeyDown;

        public NumberInputPresentation()
        {
            InitializeComponent();
            Height = double.NaN;
            Width = double.NaN;
            DataContext = this;
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

        private FontFamily fontFamily;

        public FontFamily MyFontFamily
        {
            get
            {
                return fontFamily;
            }
            set
            {
                fontFamily = value;
                OnPropertyChanged("MyFontFamily");
            }
        }

        private double fontsize;

        public double MyFontSize
        {
            get
            {
                return fontsize;
            }
            set
            {
                fontsize = value;
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
