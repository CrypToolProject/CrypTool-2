using CrypTool.PluginBase.Attributes;
using System.Windows.Controls;

namespace CrypTool.CrypWin
{
    /// <summary>
    /// Interaction logic for SystemInfos.xaml
    /// </summary>
    [TabColor("White")]
    [Localization("CrypTool.CrypWin.Properties.Resources")]
    public partial class LicensesTab : UserControl
    {
        public LicensesTab()
        {
            InitializeComponent();
            Tag = FindResource("Icon");
            LicenseTextbox.Text += (Properties.Resources.CrypToolLicenses + ":\r\n \r\n");
            LicenseTextbox.Text += Properties.Resources.ApacheLicense2;
        }
    }
}
