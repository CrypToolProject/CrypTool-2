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
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CrypTool.MD5.Presentation.Displays
{
    /// <summary>
    /// Interaktionslogik für DataBlockDisplay.xaml
    /// </summary>
    public partial class DataBlockDisplay : UserControl
    {
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(IList<byte>), typeof(DataBlockDisplay), null);
        public IList<byte> Data { get => (IList<byte>)GetValue(DataProperty); set => SetValue(DataProperty, value); }

        public DataBlockDisplay()
        {
            InitializeComponent();
        }
    }
}
