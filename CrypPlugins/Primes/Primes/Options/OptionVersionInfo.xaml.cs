using System;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using CrypTool.PluginBase.Miscellaneous;

namespace Primes.Options
{
    /// <summary>
    /// Interaction logic for OptionVersionInfo.xaml
    /// </summary>
    public partial class OptionVersionInfo : UserControl
    {
        public OptionVersionInfo()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Version version = AssemblyHelper.GetVersion(Assembly.GetAssembly(this.GetType()));
            string strVersion = String.Format("{0}.{1}.{2}", new Object[] { version.Major - 1, version.Minor, version.Revision });
            tbVersionInfo.Text = strVersion;
            tbBuildInfo.Text = version.Build.ToString();
        }
    }
}