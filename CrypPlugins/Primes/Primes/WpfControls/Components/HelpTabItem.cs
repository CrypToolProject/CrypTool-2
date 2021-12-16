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

namespace Primes.WpfControls.Components
{
    public class HelpTabItem : ResettableTabItem
    {
        static HelpTabItem()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HelpTabItem),
                  new FrameworkPropertyMetadata(typeof(HelpTabItem)));
        }

        public static readonly RoutedEvent HelpButtonClickEvent =
            EventManager.RegisterRoutedEvent("HelpButtonClick", RoutingStrategy.Direct,
                typeof(RoutedEventHandler), typeof(HelpTabItem));

        public event RoutedEventHandler HelpButtonClick
        {
            add { AddHandler(HelpButtonClickEvent, value); }
            remove { RemoveHandler(HelpButtonClickEvent, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Image helpButton = base.GetTemplateChild("PART_Close") as Image;
            if (helpButton != null)
            {
                helpButton.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(helpButton_MouseLeftButtonDown);
            }
        }

        private void helpButton_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(HelpButtonClickEvent, this));
            e.Handled = true;
        }
    }
}
