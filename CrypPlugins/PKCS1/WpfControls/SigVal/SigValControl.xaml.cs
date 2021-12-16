using PKCS1.Library;
using PKCS1.Resources.lang.Gui;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace PKCS1.WpfControls.SigVal
{
    /// <summary>
    /// Interaktionslogik für SigVal.xaml
    /// </summary>
    public partial class SigValControl : UserControl, IPkcs1UserControl
    {
        private bool m_bValidateCorrect = true;
        private SigValidator validator = null;
        private Signature signature = null;

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
            validator = new SigValidator();

            this.rbSigBlei.IsEnabled = false;
            this.rbSigBlei.Content = SigValCtrl.bleichenbSig + " " + SigValCtrl.sigNotGeneratedYet;
            this.rbSigPkcs.IsEnabled = false;
            this.rbSigPkcs.Content = SigValCtrl.rsaSig + " " + SigValCtrl.sigNotGeneratedYet;
            this.rbSigKuehn.IsEnabled = false;
            this.rbSigKuehn.Content = SigValCtrl.kuehnSig + " " + SigValCtrl.sigNotGeneratedYet;


            if (SignatureHandler.getInstance().isRsaSigGenerated())
            {
                signature = (RsaSig)SignatureHandler.getInstance().getSignature();
                setEnabled();
                this.rbSigPkcs.IsEnabled = true;
                this.rbSigPkcs.IsChecked = true;
                this.rbSigPkcs.Content = SigValCtrl.rsaSig;
            }
            else if (SignatureHandler.getInstance().isBleichenbSigGenerated())
            {
                signature = (BleichenbacherSig)SignatureHandler.getInstance().getBleichenbSig();
                setEnabled();
                this.rbSigBlei.IsEnabled = true;
                this.rbSigBlei.IsChecked = true;
                this.rbSigBlei.Content = SigValCtrl.bleichenbSig;
            }
            else if (SignatureHandler.getInstance().isKuehnSigGenerated())
            {
                signature = (KuehnSig)SignatureHandler.getInstance().getKuehnSig();
                setEnabled();
                this.rbSigKuehn.IsEnabled = true;
                this.rbSigKuehn.IsChecked = true;
                this.rbSigKuehn.Content = SigValCtrl.kuehnSig;
            }
            else
            {
                setDisabled();
            }
        }

        private void handleSigGenEvent(SignatureType type)
        {
            if (type == SignatureType.Pkcs1)
            {
                signature = (RsaSig)SignatureHandler.getInstance().getSignature();
                setEnabled();
                this.rbSigPkcs.IsEnabled = true;
                this.rbSigPkcs.IsChecked = true;
                this.rbSigPkcs.Content = SigValCtrl.rsaSig;
                ResultEmpty();
            }
            else if (type == SignatureType.Bleichenbacher)
            {
                signature = (BleichenbacherSig)SignatureHandler.getInstance().getBleichenbSig();
                setEnabled();
                this.rbSigBlei.IsEnabled = true;
                this.rbSigBlei.IsChecked = true;
                this.rbSigBlei.Content = SigValCtrl.bleichenbSig;
                ResultEmpty();
            }
            else if (type == SignatureType.Kuehn)
            {
                signature = (KuehnSig)SignatureHandler.getInstance().getKuehnSig();
                setEnabled();
                this.rbSigKuehn.IsEnabled = true;
                this.rbSigKuehn.IsChecked = true;
                this.rbSigKuehn.Content = SigValCtrl.kuehnSig;
                ResultEmpty();
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
            this.tbSignatureDec.Text = signature.GetSignatureDecToHexString();
            this.tbSignatureEnc.Text = signature.GetSignatureToHexString();

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
            if (m_bValidateCorrect == true)
            {
                if (validator.verifyRsaSignature(Datablock.getInstance().Message, signature.GetSignature()))
                {
                    ResultValid();
                }
                else
                {
                    ResultNotValid();
                }
            }
            else if (m_bValidateCorrect == false)
            {
                if (validator.verifyRsaSignatureWithFlaw(Datablock.getInstance().Message, signature.GetSignature()))
                {
                    ResultValid();
                }
                else
                {
                    ResultNotValid();
                }
            }
        }

        private void rbVal_Checked(object sender, RoutedEventArgs e)
        {
            // korrekte Implementierung der Validierung
            m_bValidateCorrect = true;
            ResultEmpty();
        }

        private void rbValWithFlaw_Checked(object sender, RoutedEventArgs e)
        {
            // fehlerhafte Implementierung der Validierung
            m_bValidateCorrect = false;
            ResultEmpty();
        }

        private void ResultValid()
        {
            this.lblResult.Text = SigValCtrl.resSigValid;
            this.imgResult.Source = new BitmapImage(new Uri("../../Resources/icons/equal.png", UriKind.Relative));
            this.imgResult.Visibility = Visibility.Visible;
            this.lblHashAlgo.Text = SigValCtrl.resIdentifiedHash + " " + validator.getHashFunctionName();
        }

        private void ResultNotValid()
        {
            this.lblResult.Text = SigValCtrl.resSigNotValid;
            this.imgResult.Source = new BitmapImage(new Uri("../../Resources/icons/unequal.png", UriKind.Relative));
            this.imgResult.Visibility = Visibility.Visible;
            if (validator.getHashFunctionName() != string.Empty)
            {
                this.lblHashAlgo.Text = SigValCtrl.resIdentifiedHash + " " + validator.getHashFunctionName();
            }
            else
            {
                this.lblHashAlgo.Text = " " + SigValCtrl.resHashNotReadable;
            }
        }

        private void ResultEmpty()
        {
            this.lblResult.Text = string.Empty;
            this.imgResult.Visibility = Visibility.Hidden;
            this.lblHashAlgo.Text = string.Empty;
        }

        private void rbSigPkcs_Checked(object sender, RoutedEventArgs e)
        {
            ResultEmpty();
            signature = (RsaSig)SignatureHandler.getInstance().getSignature();
            setEnabled();
        }

        private void rbSigBlei_Checked(object sender, RoutedEventArgs e)
        {
            ResultEmpty();
            signature = (BleichenbacherSig)SignatureHandler.getInstance().getBleichenbSig();
            setEnabled();
        }

        private void rbSigKuehn_Checked(object sender, RoutedEventArgs e)
        {
            ResultEmpty();
            signature = (KuehnSig)SignatureHandler.getInstance().getKuehnSig();
            setEnabled();
        }
    }
}
