 using System.IO;
using System.Windows;
using System.Windows.Controls; 
using CrypTool.PluginBase;
using CrypTool.PluginBase.IO;
using OnlineDocumentationGenerator.Generators.HtmlGenerator;
using System.Windows.Documents;
using System;
using CrypTool.PluginBase.Editor;

namespace Startcenter
{
    /// <summary>
    /// Interaction logic for Buttons.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("Startcenter.Properties.Resources")]
    public partial class Buttons : UserControl
    {
        public event OpenEditorHandler OnOpenEditor;

        public Buttons()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Helper method to open a new editor tab with appropriate title, tooltip, and icon
        /// </summary>
        /// <param name="editorType"></param>
        private void DoOpenEditor(Type editorType)
        {
            Span tooltip = new Span();
            tooltip.Inlines.Add(editorType.GetPluginInfoAttribute().ToolTip);
            TabInfo tab = new TabInfo() { Title = editorType.GetPluginInfoAttribute().Caption, Icon = editorType.GetImage(0).Source, Tooltip = tooltip };
            OnOpenEditor(editorType, null);
        }

        private void WizardButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DoOpenEditor(typeof(Wizard.Wizard));
            }
            catch (Exception)
            {
                //do nothing
            }
        }
            
        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OnlineHelp.InvokeShowDocPage(null);
            }
            catch (Exception)
            {
                //do nothing
            }
        }

        private void WorkspaceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DoOpenEditor(typeof(WorkspaceManager.WorkspaceManagerClass));
            }
            catch (Exception)
            {
                //do nothing
            }
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

        private void CrypToolStoreButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DoOpenEditor(typeof(CrypTool.CrypToolStore.CrypToolStoreEditor));                
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
