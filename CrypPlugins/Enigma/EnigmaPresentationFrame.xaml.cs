using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace CrypTool.Enigma
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class EnigmaPresentationFrame : UserControl
    {
        private readonly Enigma facade;
        public EnigmaPresentation EnigmaPresentation;
        public EnigmaPresentationFrame(Enigma facade)
        {
            this.facade = facade;
            facade.Settings.PropertyChanged += settings_OnPropertyChange;

            InitializeComponent();
            visbileCheckbox.Content = Properties.Resources.PresentationActivation;
            EnigmaPresentation = new EnigmaPresentation(facade);

            dockPanel1.Children.Add(EnigmaPresentation);

            Binding disableBoolBinding = new Binding("DisabledBoolProperty")
            {
                Mode = BindingMode.TwoWay,
                Source = EnigmaPresentation.PresentationDisabled
            };
            visbileCheckbox.SetBinding(CheckBox.IsCheckedProperty, disableBoolBinding);

            Binding myBinding2 = new Binding("IsChecked")
            {
                Source = visbileCheckbox,
                Mode = BindingMode.TwoWay
            };
            BooleanToVisibilityConverter booleanToHidden = new BooleanToVisibilityConverter();
            myBinding2.Converter = booleanToHidden;
            EnigmaPresentation.SetBinding(VisibilityProperty, myBinding2);

            IsVisibleChanged += new DependencyPropertyChangedEventHandler(EnigmaPresentationFrame_IsVisibleChanged);
        }

        public void settings_OnPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            EnigmaSettings settings = sender as EnigmaSettings;

            if (e.PropertyName == "Model")
            {
                if (settings.Model == 2)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        visbileCheckbox.
                                                                      IsEnabled = true;

                    }, null);

                }
                else if (settings.Model == 3)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                    {
                        visbileCheckbox.
                                                                      IsEnabled = true;

                    }, null);

                }
                else
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                                                                              {
                                                                                                  visbileCheckbox.
                                                                                                      IsEnabled = false;
                                                                                                  visbileCheckbox.
                                                                                                      IsChecked = false;
                                                                                              }, null);
                }

            }
        }

        public void ChangeStatus(bool isrunning, bool isvisible)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                                                                                 {
                                                                                     EnigmaPresentation.disablePresentation(isrunning, isvisible);
                                                                                     if (isrunning && isvisible)
                                                                                     {
                                                                                         enigmaStatus.Text = Properties.Resources.EnigmaPresentationFrame_ChangeStatus_Presentation_aktive;
                                                                                         enigmaStatus.Background = Brushes.LawnGreen;
                                                                                     }
                                                                                     else if (!isrunning && isvisible)
                                                                                     {
                                                                                         enigmaStatus.Text = Properties.Resources.EnigmaPresentationFrame_ChangeStatus_Presentation_ready;
                                                                                         enigmaStatus.Background = Brushes.LawnGreen;
                                                                                     }
                                                                                     else if (isrunning && !isvisible)
                                                                                     {
                                                                                         enigmaStatus.Text = Properties.Resources.EnigmaPresentationFrame_ChangeStatus;
                                                                                         visbileCheckbox.IsChecked = false;
                                                                                         enigmaStatus.Background = Brushes.Tomato;
                                                                                     }
                                                                                     else
                                                                                     {
                                                                                         enigmaStatus.Text = Properties.Resources.EnigmaPresentationFrame_ChangeStatus_Präsentation_turned_off;
                                                                                         enigmaStatus.Background = Brushes.Tomato;
                                                                                     }
                                                                                 }, null);
        }

        private void EnigmaPresentationFrame_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }

        private void visbileCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            enigmaStatus.Text = Properties.Resources.EnigmaPresentationFrame_ChangeStatus_Präsentation_turned_off;
            enigmaStatus.Background = Brushes.Tomato;

            EnigmaPresentation.giveFeedbackAndDie();

            dockPanel1.Background.Opacity = 1;
        }

        private void visbileCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (facade._isrunning)
            {
                enigmaStatus.Text = Properties.Resources.EnigmaPresentationFrame_visbileCheckbox_Checked_Restart_Workspace;
            }
            else
            {
                enigmaStatus.Text = Properties.Resources.EnigmaPresentationFrame_ChangeStatus_Presentation_ready;
            }
            enigmaStatus.Background = Brushes.LawnGreen;

            dockPanel1.Background.Opacity = 0;
        }

    }
    public class BooleanToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(Visibility))
            {
                bool visible = System.Convert.ToBoolean(value, culture);
                if (InvertVisibility)
                {
                    visible = !visible;
                }

                return visible ? Visibility.Visible : Visibility.Hidden;
            }
            throw new InvalidOperationException("Converter can only convert to value of type Visibility.");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("Converter cannot convert back.");
        }

        public bool InvertVisibility { get; set; }

    }
}
