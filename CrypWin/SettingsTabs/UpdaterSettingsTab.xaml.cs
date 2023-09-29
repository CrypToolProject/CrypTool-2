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
using CrypTool.CrypWin.Properties;
using CrypTool.PluginBase.Attributes;
using System.Windows;
using System.Windows.Controls;

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
            minutesInput.Text = Settings.Default.CheckInterval + "";
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
                {
                    checkStartup.IsChecked = isChecked;
                }
            }
        }

        private void minutesInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool isInteger = int.TryParse(minutesInput.Text, out int minutes);

            if (isInteger && minutes > 0)
            {
                Settings.Default.CheckInterval = minutes;
                checkPeriodicallyWarning.Visibility = Visibility.Hidden;
            }
            else
            {
                checkPeriodicallyWarning.Visibility = Visibility.Visible;
            }
        }

    }

}
