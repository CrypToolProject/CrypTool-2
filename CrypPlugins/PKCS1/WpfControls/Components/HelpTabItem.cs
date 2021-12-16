using System.Windows;
using System.Windows.Controls;

namespace PKCS1.WpfControls.Components
{
    public class HelpTabItem : ResettableTabItem
    {
        static HelpTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HelpTabItem), new FrameworkPropertyMetadata(typeof(HelpTabItem)));
        }

        public static readonly RoutedEvent HelpButtonClickEvent = EventManager.RegisterRoutedEvent(
            "HelpButtonClick", RoutingStrategy.Direct,
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
