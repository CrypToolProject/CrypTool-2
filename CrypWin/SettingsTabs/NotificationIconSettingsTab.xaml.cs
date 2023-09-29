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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace CrypTool.CrypWin.SettingsTabs
{
    /// <summary>
    /// Interaction logic for NotificationIconSettingsTab.xaml
    /// </summary>
    [Localization("CrypTool.CrypWin.SettingsTabs.Resources.res")]
    [SettingsTab("NotificationIconSettings", "/MainSettings/", 1.0)]
    public partial class NotificationIconSettingsTab : UserControl
    {
        public NotificationIconSettingsTab(Style settingsStyle)
        {
            Resources.Add("settingsStyle", settingsStyle);
            InitializeComponent();

            Settings.Default.PropertyChanged += delegate (object sender, PropertyChangedEventArgs e)
                                                    {
                                                        if (e.PropertyName == "BallonVisibility_ms")
                                                        {
                                                            SetBalloonTimeToSettings();
                                                        }
                                                    };
            SetBalloonTimeToSettings();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            if (double.TryParse(balloonTime.Text, out double val))
            {
                Settings.Default.BallonVisibility_ms = (int)(val * 1000);
            }
            SetBalloonTimeToSettings();
        }

        private void SetBalloonTimeToSettings()
        {
            balloonTime.Text = string.Format("{0:F2}", Settings.Default.BallonVisibility_ms / 1000.0);
        }
    }
}
