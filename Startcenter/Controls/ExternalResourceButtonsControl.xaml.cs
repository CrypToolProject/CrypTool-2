/*
   Copyright 2008-2022 CrypTool Team

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
using CrypTool.PluginBase;
using CrypTool.PluginBase.IO;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Startcenter.Controls
{
    /// <summary>
    /// Interaction logic for ExternalResourceButtons.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("Startcenter.Properties.Resources")]
    public partial class ExternalResourceButtonsControl : UserControl
    {
        public event OpenEditorHandler OnOpenEditor;

        public ExternalResourceButtonsControl()
        {
            InitializeComponent();
        }

        private void WebpageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://www.CrypTool.org/CrypTool2");
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void YouTubeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://www.youtube.com/channel/UC8_FqvQWJfZYxcSoEJ5ob-Q");
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void FacebookButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://www.facebook.de/CrypTool2");
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void BookButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //open the CrypTool book webpage in the default browser
                System.Diagnostics.Process.Start(Properties.Resources.CTBookURL);
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void CrypTool2Tutorial_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://youtu.be/dYaUe4BKQhc");
            }
            catch (Exception)
            {
                //do nothing
            }
        }
    }
}
