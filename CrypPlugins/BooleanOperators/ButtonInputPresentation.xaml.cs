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
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace BooleanOperators
{
    /// <summary>
    /// Interaktionslogik für Button.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("BooleanOperators.Properties.Resources")]
    public partial class ButtonInputPresentation : UserControl
    {

        public event EventHandler StatusChanged;

        public ButtonInputPresentation()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Current value of the button
        /// </summary>
        public bool Value { get; set; }

        private void setButton()
        {
            if (Value)
            {
                this.myButton.Background = Brushes.LawnGreen;
                this.myButton.Content = Properties.Resources.True;
            }
            else
            {
                this.myButton.Background = Brushes.Tomato;
                this.myButton.Content = Properties.Resources.False;
            }
        }

        public void update()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {
                setButton();
            }, null);
        }

        public void ExecuteThisMethodWhenButtonIsClicked(object sender, EventArgs e)
        {
            Value = !Value;
            setButton();

            if (StatusChanged != null)
            {
                StatusChanged(this, EventArgs.Empty);
            }
        }
    }
}