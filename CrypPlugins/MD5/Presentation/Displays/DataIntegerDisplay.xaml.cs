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
using System.Windows;
using System.Windows.Controls;

namespace CrypTool.MD5.Presentation.Displays
{
    /// <summary>
    /// Interaction logic for DataIntegerDisplay.xaml
    /// </summary>
    public partial class DataIntegerDisplay : UserControl
    {
        public int HighlightedValue
        {
            get => (int)GetValue(HighlightedValueProperty);
            set => SetValue(HighlightedValueProperty, value);
        }

        public uint[] DisplayedValues
        {
            get => (uint[])GetValue(DisplayedValuesProperty);
            set => SetValue(DisplayedValuesProperty, value);
        }

        // Using a DependencyProperty as the backing store for DisplayedValues.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisplayedValuesProperty =
            DependencyProperty.Register("DisplayedValues", typeof(uint[]), typeof(DataIntegerDisplay), new UIPropertyMetadata(new uint[16]));

        // Using a DependencyProperty as the backing store for HighlightedValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HighlightedValueProperty =
            DependencyProperty.Register("HighlightedValue", typeof(int), typeof(DataIntegerDisplay), new UIPropertyMetadata(-1));


        public DataIntegerDisplay()
        {
            InitializeComponent();

            Width = double.NaN;
            Height = double.NaN;
        }
    }
}