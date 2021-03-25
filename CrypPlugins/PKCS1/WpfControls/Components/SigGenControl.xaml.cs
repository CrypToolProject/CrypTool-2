using System;
using System.Windows;
using System.Windows.Controls;
using PKCS1.Library;
using PKCS1.Resources.lang.Gui;

namespace PKCS1.WpfControls.Components
{
    /// <summary>
    /// Interaktionslogik für SigGenControl.xaml
    /// </summary>
    public partial class SigGenControl : UserControl
    {
        private RsaSig m_RSASignature;
        
        public SigGenControl()
        {
            InitializeComponent();
            // zeile muss weg; Signatur muss sich bei RsaKey anmelden
            RsaKey.Instance.RaiseKeyGeneratedEvent += handleCustomEvent; // bei KeyGen-Listener anmelden 
            this.handleCustomEvent(ParameterChangeType.RsaKey);
        }

        private void handleCustomEvent(ParameterChangeType type)
        {
            this.tbResultEncrypted.Text = String.Empty;
            this.lblPublicKeyRes.Text = RsaKey.Instance.PubExponent.ToString();
            this.lblRsaKeySizeRes.Text = RsaKey.Instance.RsaKeySize.ToString();
        }

        private void bExecute_Click(object sender, RoutedEventArgs e)
        {
            this.m_RSASignature = (RsaSig) SignatureHandler.getInstance().getSignature();
            this.m_RSASignature.GenerateSignature();
            UserControlHelper.loadRtbColoredSig(this.rtbResult, this.m_RSASignature.GetSignatureDecToHexString());
            this.tbResultEncrypted.Text = this.m_RSASignature.GetSignatureToHexString();

            // nur temp
            //SignatureHandler.getInstance().setSignature(this.m_RSASignature);
        }

        private void tbResultEncrypted_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.lblEncryptedSignatureLength.Text = String.Format( Common.length, this.tbResultEncrypted.Text.Length * 4 );
        }

        private void rtbResult_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.lblSignatureLength.Text = String.Format(Common.length, UserControlHelper.GetRtbTextLength(this.rtbResult) * 4);
        }
    }
}
