using CrypTool.PluginBase.Attributes;
using System.Windows;
using System.Windows.Controls;

namespace Wizard
{
    /// <summary>
    /// Interaction logic for WizardSettingsTab.xaml
    /// </summary>
    [Localization("Wizard.Resources.settingsRes")]
    [SettingsTab("WizardSettings", "/MainSettings/")]
    public partial class WizardSettingsTab : UserControl
    {
        public WizardSettingsTab(Style settingsStyle)
        {
            Resources.Add("settingsStyle", settingsStyle);
            InitializeComponent();

            CrypTool.PluginBase.Properties.Settings.Default.PropertyChanged += delegate { CrypTool.PluginBase.Miscellaneous.ApplicationSettingsHelper.SaveApplicationsSettings(); };
        }
    }
}
