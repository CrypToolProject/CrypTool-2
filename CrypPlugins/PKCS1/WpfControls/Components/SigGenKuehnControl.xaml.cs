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
using System.Windows.Input;

namespace PKCS1.WpfControls.Components
{
    /// <summary>
    /// Interaktionslogik für SigGenKuehnControl.xaml
    /// </summary>
    public partial class SigGenKuehnControl : UserControl
    {
        private KuehnSig m_signature = (KuehnSig)SignatureHandler.getInstance().getKuehnSig();
        public KuehnSig Signature
        {
            get => m_signature;
            set => m_signature = value;
        }

        public SigGenKuehnControl()
        {
            InitializeComponent();
            RsaKey.Instance.RaiseKeyGeneratedEvent += handleCustomEvent; // listen
            handleCustomEvent(ParameterChangeType.RsaKey);
        }

        private void handleCustomEvent(ParameterChangeType type)
        {
            this.lblPublicKeyRes.Text = RsaKey.Instance.PubExponent.ToString();
            this.lblRsaKeySizeRes.Text = RsaKey.Instance.RsaKeySize.ToString();
        }

        private void bExecute_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;

            if (Signature.GenerateSignature())
            {
                UserControlHelper.loadRtbColoredSig(this.rtbResult, Signature.GetSignatureDecToHexString());
                this.tbResultEncrypted.Text = Signature.GetSignatureToHexString();
                SignatureHandler.getInstance().setKuehnSig(Signature);
            }
            else
            {
                this.tbError.Text = SigGenKuehnCtrl.genSigErrorMaxIter;
            }

            Cursor = Cursors.Arrow;
        }

        private void tbResultEncrypted_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.lblEncryptedSignatureLength.Text = string.Format(Common.length, this.tbResultEncrypted.Text.Length * 4);
        }

        private void rtbResult_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.lblSignatureLength.Text = string.Format(Common.length, UserControlHelper.GetRtbTextLength(this.rtbResult) * 4);
        }

        private void btn_Help_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender == btnHelpIterations)
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(PKCS1.OnlineHelp.OnlineHelpActions.Gen_Kuehn_Iterations);
            }
            e.Handled = true;
        }
    }
}
