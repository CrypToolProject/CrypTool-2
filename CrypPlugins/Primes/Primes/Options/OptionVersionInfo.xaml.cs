using CrypTool.PluginBase.Miscellaneous;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

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
            Version version = AssemblyHelper.GetVersion(Assembly.GetAssembly(GetType()));
            string strVersion = string.Format("{0}.{1}.{2}", new object[] { version.Major - 1, version.Minor, version.Revision });
            tbVersionInfo.Text = strVersion;
            tbBuildInfo.Text = version.Build.ToString();
        }
    }
}