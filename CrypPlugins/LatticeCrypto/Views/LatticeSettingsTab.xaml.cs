using CrypTool.PluginBase.Attributes;
using LatticeCrypto.Models;
using System.Windows;
using System.Windows.Controls;

namespace LatticeCrypto.SettingsTab
{
    [Localization("LatticeCrypto.Properties.Languages")]
    [SettingsTab("LatticeSettings", "/PluginSettings/")]
    public partial class LatticeSettingsTab
    {
        public LatticeSettingsTab(Style settingsStyle)
        {
            Resources.Add("settingsStyle", settingsStyle);
            InitializeComponent();
            Properties.Settings.Default.PropertyChanged += delegate { Properties.Settings.Default.Save(); };
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (((ComboBox)sender).SelectedIndex)
            {
                case 0:
                    FormatSettings.LatticeTagOpen = "[";
                    FormatSettings.LatticeTagClosed = "]";
                    break;
                case 1:
                    FormatSettings.LatticeTagOpen = "{";
                    FormatSettings.LatticeTagClosed = "}";
                    break;
                case 2:
                    FormatSettings.LatticeTagOpen = "(";
                    FormatSettings.LatticeTagClosed = ")";
                    break;
                case 3:
                    FormatSettings.LatticeTagOpen = "";
                    FormatSettings.LatticeTagClosed = "";
                    break;
            }
        }

        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            switch (((ComboBox)sender).SelectedIndex)
            {
                case 0:
                    FormatSettings.VectorTagOpen = "[";
                    FormatSettings.VectorTagClosed = "]";
                    break;
                case 1:
                    FormatSettings.VectorTagOpen = "{";
                    FormatSettings.VectorTagClosed = "}";
                    break;
                case 2:
                    FormatSettings.VectorTagOpen = "(";
                    FormatSettings.VectorTagClosed = ")";
                    break;
                case 3:
                    FormatSettings.VectorTagOpen = "";
                    FormatSettings.VectorTagClosed = "";
                    break;
            }
        }

        private void ComboBox_SelectionChanged_2(object sender, SelectionChangedEventArgs e)
        {
            switch (((ComboBox)sender).SelectedIndex)
            {
                case 0:
                    FormatSettings.VectorSeparator = ',';
                    break;
                case 1:
                    FormatSettings.VectorSeparator = ';';
                    break;
                case 2:
                    FormatSettings.VectorSeparator = ' ';
                    break;
                case 3:
                    FormatSettings.VectorSeparator = '\t';
                    break;
            }
        }

        private void ComboBox_SelectionChanged_3(object sender, SelectionChangedEventArgs e)
        {
            switch (((ComboBox)sender).SelectedIndex)
            {
                case 0:
                    FormatSettings.CoordinateSeparator = ',';
                    break;
                case 1:
                    FormatSettings.CoordinateSeparator = ';';
                    break;
                case 2:
                    FormatSettings.CoordinateSeparator = ' ';
                    break;
                case 3:
                    FormatSettings.CoordinateSeparator = '\t';
                    break;
            }
        }

        //private void Label_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Simpson_Lin);
        //    e.Handled = true;
        //    OnlineHelp.OnlineHelpAccess.Activate();
        //}

        //private void Label_MouseLeftButtonDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(Primes.OnlineHelp.OnlineHelpActions.Pari_Lin);
        //    e.Handled = true;
        //    OnlineHelp.OnlineHelpAccess.Activate();
        //}
    }
}