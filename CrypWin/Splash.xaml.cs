using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading;
using CrypTool.PluginBase.Miscellaneous;

namespace CrypTool.CrypWin
{
    /// <summary>
    /// Interaction logic for Splash/About window
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("CrypTool.CrypWin.Properties.Resources")]
    public partial class Splash : Window
    {
        public Splash()
        {
            InitializeComponent();
            VersionInfoRun.Text = AssemblyHelper.BuildType.ToString() + " Build – Version " +
                                  AssemblyHelper.Version.ToString();

            if (DateTime.Now.Month == 10 && DateTime.Now.Day >= 28 || DateTime.Now.Month == 11 && DateTime.Now.Day == 1)
            {
                HwImage.Visibility = Visibility.Visible;
            }
            if (DateTime.Now.Month == 12 && DateTime.Now.Day >= 17 || DateTime.Now.Month == 1 && DateTime.Now.Day <= 3)
            {
                XmImage.Visibility = Visibility.Visible;
            }
        }

        public Splash(bool staticAboutWindow) : this()
        {
            // Window is being used as About dialog
            if (staticAboutWindow)
            {
                // Listen for close clicks and hide splash progress bar
                this.MouseLeftButtonDown += EventMouseLeftButtonDown;
                this.pbInitProgress.Visibility = Visibility.Hidden;
                this.tbInitPercent.Visibility = Visibility.Hidden;
            }
        }

        public void ShowStatus(string message, double progress)
        {
            if (message != null && (progress >= 0 && progress <= 100))
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    // show message
                    logMessage.Text = message;

                    // update progress
                    pbInitProgress.Value = progress;
                    tbInitPercent.Text = ((int)progress).ToString() + " %";
                }, null); 
            }
        }


        public void ShowStatus(string message)
        {
            if (message != null)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    logMessage.Text = message;
                }, null);
            }
        }

        public void ShowStatus(double value)
        {
            if (value >= 0 && value <= 100)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pbInitProgress.Value = value;
                    tbInitPercent.Text = ((int)value).ToString() + " %";
                }, null);
            }
        }

        private void EventMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(((Hyperlink)sender).NavigateUri.ToString());
            this.Close();
        }
    }

} // End namespace
