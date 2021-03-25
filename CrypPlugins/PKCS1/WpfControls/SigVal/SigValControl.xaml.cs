using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using PKCS1.Library;
using PKCS1.Resources.lang.Gui;

namespace PKCS1.WpfControls.SigVal
{
    /// <summary>
    /// Interaktionslogik für SigVal.xaml
    /// </summary>
    public partial class SigValControl : UserControl, IPkcs1UserControl
    {
        private bool m_bValidateCorrect = true;
        private SigValidator validator = null;
        Signature signature = null;

        public SigValControl()
        {
            InitializeComponent();
            Init();
        }

        #region IPkcs1UserControl Member

        public void Dispose()
        {
        }

        public void Init()
        {
            //TODO: dreifaches anmelden anders lösen?
            SignatureHandler.getInstance().getSignature().RaiseSigGenEvent += handleSigGenEvent;
            SignatureHandler.getInstance().getBleichenbSig().RaiseSigGenEvent += handleSigGenEvent;
            SignatureHandler.getInstance().getKuehnSig().RaiseSigGenEvent += handleSigGenEvent;

            this.rbVal.IsChecked = true;
            this.validator = new SigValidator();

            this.rbSigBlei.IsEnabled = false;
            this.rbSigBlei.Content = SigValCtrl.bleichenbSig + " " + SigValCtrl.sigNotGeneratedYet;
            this.rbSigPkcs.IsEnabled = false;
            this.rbSigPkcs.Content = SigValCtrl.rsaSig + " " + SigValCtrl.sigNotGeneratedYet;
            this.rbSigKuehn.IsEnabled = false;
            this.rbSigKuehn.Content = SigValCtrl.kuehnSig + " " + SigValCtrl.sigNotGeneratedYet;

            
            if (SignatureHandler.getInstance().isRsaSigGenerated() )
            {
                this.signature = (RsaSig)SignatureHandler.getInstance().getSignature();
                this.setEnabled();
                this.rbSigPkcs.IsEnabled = true;
                this.rbSigPkcs.IsChecked = true;
                this.rbSigPkcs.Content = SigValCtrl.rsaSig;
            }
            else if (SignatureHandler.getInstance().isBleichenbSigGenerated())
            {
                this.signature = (BleichenbacherSig)SignatureHandler.getInstance().getBleichenbSig();
                this.setEnabled();
                this.rbSigBlei.IsEnabled = true;
                this.rbSigBlei.IsChecked = true;
                this.rbSigBlei.Content = SigValCtrl.bleichenbSig;
            }
            else if (SignatureHandler.getInstance().isKuehnSigGenerated())
            {
                this.signature = (KuehnSig)SignatureHandler.getInstance().getKuehnSig();
                this.setEnabled();
                this.rbSigKuehn.IsEnabled = true;
                this.rbSigKuehn.IsChecked = true;
                this.rbSigKuehn.Content = SigValCtrl.kuehnSig;
            }
            else
            {
                this.setDisabled();
            }
        }

        private void handleSigGenEvent(SignatureType type)
        {
            if (type == SignatureType.Pkcs1)
            {
                this.signature = (RsaSig)SignatureHandler.getInstance().getSignature();
                this.setEnabled();
                this.rbSigPkcs.IsEnabled = true;
                this.rbSigPkcs.IsChecked = true;
                this.rbSigPkcs.Content = SigValCtrl.rsaSig;
                this.ResultEmpty();
            }
            else if (type == SignatureType.Bleichenbacher)
            {
                this.signature = (BleichenbacherSig)SignatureHandler.getInstance().getBleichenbSig();
                this.setEnabled();
                this.rbSigBlei.IsEnabled = true;
                this.rbSigBlei.IsChecked = true;
                this.rbSigBlei.Content = SigValCtrl.bleichenbSig;
                this.ResultEmpty();
            }
            else if (type == SignatureType.Kuehn)
            {
                this.signature = (KuehnSig)SignatureHandler.getInstance().getKuehnSig();
                this.setEnabled();
                this.rbSigKuehn.IsEnabled = true;
                this.rbSigKuehn.IsChecked = true;
                this.rbSigKuehn.Content = SigValCtrl.kuehnSig;
                this.ResultEmpty();
            }
        }

        private void setDisabled()
        {
            this.tbSignatureDec.Text = SigValCtrl.plsGenSigFirst;
            this.tbSignatureEnc.Text = SigValCtrl.plsGenSigFirst;

            this.bValidate.IsEnabled = false;
            this.rbVal.IsEnabled = false;
            this.rbValWithFlaw.IsEnabled = false;
        }

        private void setEnabled()
        {
            this.tbSignatureDec.Text = this.signature.GetSignatureDecToHexString();
            this.tbSignatureEnc.Text = this.signature.GetSignatureToHexString();

            this.bValidate.IsEnabled = true;
            this.rbVal.IsEnabled = true;
            this.rbValWithFlaw.IsEnabled = true;
        }

        public void SetTab(int i)
        {
        }

        #endregion

        private void bValidate_Click(object sender, RoutedEventArgs e)
        {           
            if (this.m_bValidateCorrect == true)
            {
                if (this.validator.verifyRsaSignature(Datablock.getInstance().Message, signature.GetSignature()))
                {
                    this.ResultValid();
                 }
                else
                {
                    this.ResultNotValid();
                }
            }
            else if( this.m_bValidateCorrect == false )
            {
                if (this.validator.verifyRsaSignatureWithFlaw(Datablock.getInstance().Message, signature.GetSignature()))
                {
                    this.ResultValid();
                }
                else
                {
                    this.ResultNotValid();
                }
            }
        }

        private void rbVal_Checked(object sender, RoutedEventArgs e)
        {
            // korrekte Implementierung der Validierung
            this.m_bValidateCorrect = true;
            this.ResultEmpty();
        }

        private void rbValWithFlaw_Checked(object sender, RoutedEventArgs e)
        {
            // fehlerhafte Implementierung der Validierung
            this.m_bValidateCorrect = false;
            this.ResultEmpty();
        }

        private void ResultValid()
        {
            this.lblResult.Text = SigValCtrl.resSigValid; 
            this.imgResult.Source = new BitmapImage(new Uri("../../Resources/icons/equal.png", UriKind.Relative));
            this.imgResult.Visibility = Visibility.Visible;
            this.lblHashAlgo.Text = SigValCtrl.resIdentifiedHash + " " + this.validator.getHashFunctionName();
        }

        private void ResultNotValid()
        {
            this.lblResult.Text = SigValCtrl.resSigNotValid;
            this.imgResult.Source = new BitmapImage(new Uri("../../Resources/icons/unequal.png", UriKind.Relative));
            this.imgResult.Visibility = Visibility.Visible;
            if (this.validator.getHashFunctionName() != String.Empty)
            {
                this.lblHashAlgo.Text = SigValCtrl.resIdentifiedHash + " " + this.validator.getHashFunctionName();
            }
            else
            {
                this.lblHashAlgo.Text = " " + SigValCtrl.resHashNotReadable;
            }
        }

        private void ResultEmpty()
        {
            this.lblResult.Text = String.Empty;
            this.imgResult.Visibility = Visibility.Hidden;
            this.lblHashAlgo.Text = String.Empty;
        }

        private void rbSigPkcs_Checked(object sender, RoutedEventArgs e)
        {
            this.ResultEmpty();
            this.signature = (RsaSig)SignatureHandler.getInstance().getSignature();
            this.setEnabled();
        }

        private void rbSigBlei_Checked(object sender, RoutedEventArgs e)
        {
            this.ResultEmpty();
            this.signature = (BleichenbacherSig)SignatureHandler.getInstance().getBleichenbSig();
            this.setEnabled();
        }

        private void rbSigKuehn_Checked(object sender, RoutedEventArgs e)
        {
            this.ResultEmpty();
            this.signature = (KuehnSig)SignatureHandler.getInstance().getKuehnSig();
            this.setEnabled();
        }
    }
}
