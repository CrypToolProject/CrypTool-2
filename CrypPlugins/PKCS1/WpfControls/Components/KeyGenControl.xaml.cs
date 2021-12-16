using PKCS1.Library;
using PKCS1.Resources.lang.Gui;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PKCS1.WpfControls.Components
{
    /// <summary>
    /// Interaktionslogik für RsaKeyGenControl.xaml
    /// </summary>
    public partial class KeyGenControl : UserControl, IPkcs1UserControl
    {
        public KeyGenControl()
        {
            InitializeComponent();
        }

        #region IPkcs1UserControl Member

        public void Dispose()
        {
        }

        public void Init()
        {
        }

        public void SetTab(int i)
        {
        }

        #endregion

        private void btnGenRsaKey_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;
            RsaKey.Instance.genRsaKeyPair(25);
            Cursor = Cursors.Arrow;

            if (RsaKey.Instance.isKeyGenerated())
            {
                this.tbResultModulus.Text = RsaKey.Instance.getModulusToBigInt().ToString(16);
                this.tbResultPrivKey.Text = RsaKey.Instance.getPrivKeyToBigInt().ToString(16);
            }
        }

        private void tbResultPrivKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.lblPrivKeyLength.Text = string.Format(Common.length, this.tbResultPrivKey.Text.Length * 4);
        }

        private void tbResultModulus_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.lblModulusLength.Text = string.Format(Common.length, this.tbResultModulus.Text.Length * 4);
        }

        private void btn_Help_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender == btnHelpPubKey)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(PKCS1.OnlineHelp.OnlineHelpActions.KeyGen_PubExponent);
            }
            else if (sender == btnHelpBitSizeModulus)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(PKCS1.OnlineHelp.OnlineHelpActions.KeyGen_ModulusSize);
            }
            e.Handled = true;
        }
    }
}
