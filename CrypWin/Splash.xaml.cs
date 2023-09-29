/*
   Copyright 2008 - 2022 CrypTool Team

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
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;

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
                MouseLeftButtonDown += EventMouseLeftButtonDown;
                this.pbInitProgress.Visibility = Visibility.Hidden;
                this.tbInitPercent.Visibility = Visibility.Hidden;
            }
        }

        public void ShowStatus(string message, double progress)
        {
            if (message != null && (progress >= 0 && progress <= 100))
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
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
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    logMessage.Text = message;
                }, null);
            }
        }

        public void ShowStatus(double value)
        {
            if (value >= 0 && value <= 100)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
                {
                    pbInitProgress.Value = value;
                    tbInitPercent.Text = ((int)value).ToString() + " %";
                }, null);
            }
        }

        private void EventMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(((Hyperlink)sender).NavigateUri.ToString());
            Close();
        }
    }

} // End namespace
