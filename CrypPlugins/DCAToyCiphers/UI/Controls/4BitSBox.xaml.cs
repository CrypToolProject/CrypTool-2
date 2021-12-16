/*
   Copyright 2019 Christian Bender christian1.bender@student.uni-siegen.de

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

using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DCAToyCiphers.UI.Controls
{
    /// <summary>
    /// Interaktionslogik für _4BitSBox.xaml
    /// </summary>
    public partial class _4BitSBox : UserControl
    {
        public _4BitSBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the mouse click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleSBoxClick(object sender, MouseButtonEventArgs e)
        {
            Rectangle elem = (Rectangle)sender;
            if (elem.Stroke == Brushes.Black)
            {
                elem.Stroke = Brushes.Red;
            }
            else
            {
                elem.Stroke = Brushes.Black;
            }

        }
    }
}
