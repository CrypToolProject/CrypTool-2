using CrypTool.PluginBase;
using CrypTool.PluginBase.IO;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Startcenter
{
    /// <summary>
    /// Interaction logic for ExternalResourceButtons.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("Startcenter.Properties.Resources")]
    public partial class ExternalResourceButtons : UserControl
    {
        public event OpenEditorHandler OnOpenEditor;

        public ExternalResourceButtons()
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
                System.Diagnostics.Process.Start(Path.Combine(DirectoryHelper.BaseDirectory, Properties.Resources.CTBookFilename));
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
