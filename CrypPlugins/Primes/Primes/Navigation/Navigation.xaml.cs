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
using System.Windows.Controls;
using System.Windows.Input;

namespace Primes.Navigation
{
    /// <summary>
    /// Interaction logic for Navigation.xaml
    /// </summary>
    public partial class Navigation : UserControl
    {
        public Navigation()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            stackPanel2.Width = ActualWidth;
            expander1.Width = stackPanel1.ActualWidth;
            expander2.Width = stackPanel1.ActualWidth;
            btnFactor2.Width = stackPanel1.ActualWidth - 10;
            btnFactor3.Width = stackPanel1.ActualWidth - 10;
            btnRadiantPrimes.Width = stackPanel1.ActualWidth - 10;
            btnFaktorisierung.Width = stackPanel1.ActualWidth - 10;
        }

        private void expander1_Collapsed(object sender, RoutedEventArgs e)
        {
            if (sender.GetType() == typeof(Expander))
            {
                HandleNavigation((sender as Expander));
            }
        }

        public void HandleNavigation(Expander exp)
        {
            StackPanel spTarget = null;

            switch (exp.Name)
            {
                case "expander1":
                    spTarget = stackPanel2;
                    break;
                case "expander2":
                    spTarget = stackPanel3;
                    break;
            }

            if (spTarget != null)
            {
                spTarget.Visibility = (exp.IsExpanded) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void myItemsControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void button1_MouseMove(object sender, MouseEventArgs e)
        {
            object s = (sender as Button).Template;
        }
    }
}
