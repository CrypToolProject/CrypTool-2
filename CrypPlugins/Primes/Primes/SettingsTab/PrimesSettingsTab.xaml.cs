using CrypTool.PluginBase.Attributes;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Primes.SettingsTab
{
    /// <summary>
    /// Interaction logic for WizardSettingsTab.xaml
    /// </summary>
    [Localization("Primes.Properties.Resources")]
    [SettingsTab("PrimesSettings", "/PluginSettings/")]
    public partial class PrimesSettingsTab : UserControl
    {
        public PrimesSettingsTab(Style settingsStyle)
        {
            Resources.Add("settingsStyle", settingsStyle);
            InitializeComponent();

            Properties.Settings.Default.PropertyChanged += delegate { Properties.Settings.Default.Save(); };
        }

        private void Label_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Simpson_Lin);
            e.Handled = true;
            OnlineHelp.OnlineHelpAccess.Activate();
        }

        private void Label_MouseLeftButtonDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Pari_Lin);
            e.Handled = true;
            OnlineHelp.OnlineHelpAccess.Activate();
        }

        private void btnChooseGpexe_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "gp.exe|gp.exe",
                Multiselect = false,
                InitialDirectory = File.Exists(tbGpExe.Text) ? tbGpExe.Text : string.Empty
            };

            if (ofd.ShowDialog().HasValue && File.Exists(ofd.FileName))
            {
                tbGpExe.Text = ofd.FileName;
            }
        }
    }
}