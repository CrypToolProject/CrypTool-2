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
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace CrypTool.Plugins.RAPPOR.View
{
    /// <summary>
    /// Interaction logic for Overview.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.Plugins.RAPPOR.Properties.Resources")]
    public partial class Overview : UserControl
    {
        //private ArrayDrawer arrayDrawer;
        //private RAPPOR rappor;
        /// <summary>
        /// Initializes the Ovewview view model.
        /// </summary>
        public Overview()
        {
            InitializeComponent();
        }
        /// <summary>
        /// This method is used to validate the input of the textboxes, insuring that only strings 
        /// are entered which can be transformed into integers.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">The text composition event arg.</param>
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}