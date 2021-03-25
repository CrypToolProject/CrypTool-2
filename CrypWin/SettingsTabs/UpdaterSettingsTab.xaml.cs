using System;
using System.Windows;
using System.Windows.Controls;
using CrypTool.PluginBase.Attributes;
using CrypTool.CrypWin.Properties;

namespace CrypTool.CrypWin.SettingsTabs
{
    /// <summary>
    /// Interaction logic for UpdaterSettingsTab.xaml
    /// </summary>
    [Localization("CrypTool.CrypWin.SettingsTabs.Resources.res")]
    [SettingsTab("UpdaterSettings", "/MainSettings/", 0.9)]
    public partial class UpdaterSettingsTab : UserControl
    {

        public UpdaterSettingsTab(Style settingsStyle)
        {
            Resources.Add("settingsStyle", settingsStyle);
            InitializeComponent();
            minutesInput.Text = Settings.Default.CheckInterval+"";
            SetPeriodicallyDependencies();
        }

        private void checkPeriodically_Changed(object sender, RoutedEventArgs e)
        {
            SetPeriodicallyDependencies();
        }

        private void SetPeriodicallyDependencies()
        {
            if (checkPeriodically.IsChecked != null && checkStartup != null && minutesInput != null)
            {
                bool isChecked = (bool)checkPeriodically.IsChecked;
                minutesInput.IsEnabled = isChecked;
                checkStartup.IsEnabled = !isChecked;
                if (isChecked)
                    checkStartup.IsChecked = isChecked;
            }
        }

        private void minutesInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            int minutes = 0;
            bool isInteger = Int32.TryParse(minutesInput.Text, out minutes);

            if (isInteger && minutes > 0)
            {
                Settings.Default.CheckInterval = minutes;
                checkPeriodicallyWarning.Visibility = Visibility.Hidden;
            }
            else
                checkPeriodicallyWarning.Visibility = Visibility.Visible;
        }

    }

}
