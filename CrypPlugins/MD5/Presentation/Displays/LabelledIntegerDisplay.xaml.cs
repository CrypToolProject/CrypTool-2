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
    /// Interaction logic for LabelledIntegerDisplay.xaml
    /// </summary>
    public partial class LabelledIntegerDisplay : UserControl
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(uint), typeof(LabelledIntegerDisplay), null);
        public uint Value { get => (uint)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }


        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register("Caption", typeof(string), typeof(LabelledIntegerDisplay), null);
        public string Caption { get => (string)GetValue(CaptionProperty); set => SetValue(CaptionProperty, value); }

        public LabelledIntegerDisplay()
        {
            InitializeComponent();

            Width = double.NaN;
            Height = double.NaN;
        }
    }
}