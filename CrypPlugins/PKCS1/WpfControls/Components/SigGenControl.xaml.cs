/*
   Copyright (C) CrypTool 2 Team

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
using PKCS1.Library;
using PKCS1.Resources.lang.Gui;
using System.Windows;
using System.Windows.Controls;

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
            handleCustomEvent(ParameterChangeType.RsaKey);
        }

        private void handleCustomEvent(ParameterChangeType type)
        {
            this.tbResultEncrypted.Text = string.Empty;
            this.lblPublicKeyRes.Text = RsaKey.Instance.PubExponent.ToString();
            this.lblRsaKeySizeRes.Text = RsaKey.Instance.RsaKeySize.ToString();
        }

        private void bExecute_Click(object sender, RoutedEventArgs e)
        {
            m_RSASignature = (RsaSig)SignatureHandler.getInstance().getSignature();
            m_RSASignature.GenerateSignature();
            UserControlHelper.loadRtbColoredSig(this.rtbResult, m_RSASignature.GetSignatureDecToHexString());
            this.tbResultEncrypted.Text = m_RSASignature.GetSignatureToHexString();

            // nur temp
            //SignatureHandler.getInstance().setSignature(this.m_RSASignature);
        }

        private void tbResultEncrypted_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.lblEncryptedSignatureLength.Text = string.Format(Common.length, this.tbResultEncrypted.Text.Length * 4);
        }

        private void rtbResult_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.lblSignatureLength.Text = string.Format(Common.length, UserControlHelper.GetRtbTextLength(this.rtbResult) * 4);
        }
    }
}
