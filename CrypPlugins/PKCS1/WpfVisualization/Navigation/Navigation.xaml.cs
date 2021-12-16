using PKCS1.Library;
using System.Windows;
using System.Windows.Controls;

namespace PKCS1.WpfVisualization.Navigation
{
    /// <summary>
    /// Interaktionslogik für Navigation.xaml
    /// </summary>
    public partial class Navigation : UserControl
    {
        public event Navigate OnNavigate;
        public Navigation()
        {
            InitializeComponent();
        }

        private void link_Click(object sender, RoutedEventArgs e)
        {
            if (null != OnNavigate)
            {
                NavigationCommandType commandtype = NavigationCommandType.None;

                if (sender == link_SignatureGenerate)
                {
                    commandtype = NavigationCommandType.SigGen;
                }
                else if (sender == link_RsaKeyGenerate)
                {
                    commandtype = NavigationCommandType.RsaKeyGen;
                }
                else if (sender == link_AttackBleichenbacher)
                {
                    commandtype = NavigationCommandType.SigGenFakeBleichenb;
                }
                else if (sender == link_AttackShortKeysVariant)
                {
                    commandtype = NavigationCommandType.SigGenFakeShort;
                }
                else if (sender == link_SignatureValidate)
                {
                    commandtype = NavigationCommandType.SigVal;
                }
                else if (sender == link_Start)
                {
                    commandtype = NavigationCommandType.Start;
                }

                OnNavigate(commandtype);
            }
        }
    }
}
