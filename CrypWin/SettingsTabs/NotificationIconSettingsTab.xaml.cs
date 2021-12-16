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
