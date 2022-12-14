using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CrypTool.PluginBase.Attributes;

namespace CrypTool.CrypTutorials
{
    /// <summary>
    /// Interaction logic for CrypTutorialsSettingsTab.xaml
    /// </summary>
    [SettingsTab("VideoTutorialsSettings", "/MainSettings/", 0.5)]
    [Localization("CrypTool.CrypTutorials.Properties.Resources")]
    public partial class CrypTutorialsSettingsTab : UserControl
    {
        public CrypTutorialsSettingsTab(Style settingsStyle)
        {
            Resources.Add("settingsStyle", settingsStyle);
            InitializeComponent();
            CrypTool.PluginBase.Properties.Settings.Default.PropertyChanged += delegate { CrypTool.PluginBase.Miscellaneous.ApplicationSettingsHelper.SaveApplicationsSettings(); };
        }
    }
}
