/*
   Copyright 2008 Timo Eckhardt, University of Siegen

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
using System.Windows.Input;

namespace Primes.WpfControls.Components
{
    /// <summary>
    /// Interaction logic for ToolTipWindow.xaml
    /// </summary>
    public partial class ToolTipWindow : Window
    {
        public ToolTipWindow()
        {
            InitializeComponent();
            lineSpacer.Width = Width;
        }

        public string ToolTipTitle
        {
            get => lblTitle.Content as string;
            set => lblTitle.Content = value;
        }

        public string ToolTipContent
        {
            get => textBlockContent.Text;
            set => textBlockContent.Text = value;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            Close();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            Close();
        }
    }
}
