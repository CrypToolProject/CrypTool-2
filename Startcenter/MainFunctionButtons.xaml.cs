using CrypTool.PluginBase;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Startcenter
{
    /// <summary>
    /// Interaction logic for MainFunctionButtons.xaml
    /// </summary>
    [CrypTool.PluginBase.Attributes.Localization("Startcenter.Properties.Resources")]
    public partial class MainFunctionButtons : UserControl
    {
        public event OpenEditorHandler OnOpenEditor;

        public MainFunctionButtons()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Helper method to open a new editor tab with appropriate title, tooltip, and icon
        /// </summary>
        /// <param name="editorType"></param>
        private void DoOpenEditor(Type editorType)
        {
            if (OnOpenEditor == null)
            {
                return;
            }
            Span tooltip = new Span();
            tooltip.Inlines.Add(editorType.GetPluginInfoAttribute().ToolTip);
            TabInfo tabInfo = new TabInfo { Title = editorType.GetPluginInfoAttribute().Caption, Icon = editorType.GetImage(0).Source, Tooltip = tooltip };
            OnOpenEditor(editorType, tabInfo);
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
    }
}
