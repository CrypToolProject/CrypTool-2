using Org.BouncyCastle.Math;
using PKCS1.Library;
using PKCS1.Resources.lang.Gui;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PKCS1.WpfControls.Components
{
    /// <summary>
    /// Interaction logic for KeyInputControl.xaml
    /// </summary>
    public partial class KeyInputControl : UserControl, IPkcs1UserControl
    {
        private bool m_bPrivKeyValid = false;
        private bool m_bModulusValid = false;
        private int m_radixModulus = 16;
        private int m_radixPrivKey = 16;

        private enum ParameterName
        {
            PrivKey,
            PubKey,
            Modulus
        }

        public KeyInputControl()
        {
            InitializeComponent();
            this.btnValInput.IsEnabled = false;
            this.btnPrivKeyHexadec.IsChecked = true;
            this.btnModHexadec.IsChecked = true;
        }

        private void tbPubKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            /*
            per Binding an RsaKey gebunden; zulässiger Eingabebereich wird per .xaml gesteuert
             */
        }

        private void tbPrivKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_bPrivKeyValid = this.checkInputTextBox(tbPrivKey.Text, m_radixPrivKey, lblErrorPrivKey, ParameterName.PrivKey);
            testAndEnableButton();
        }

        private void tbModulus_TextChanged(object sender, TextChangedEventArgs e)
        {
            m_bModulusValid = this.checkInputTextBox(tbModulus.Text, m_radixModulus, lblErrorModulus, ParameterName.Modulus);
            testAndEnableButton();
        }

        private bool checkInputTextBox(string inputText, int radix, TextBlock outputLabel, ParameterName paramName)
        {
            if (inputText != string.Empty)
            {
                if (isInputInRightFormat(inputText, radix))
                {
                    BigInteger tmp = new BigInteger(inputText, radix);

                    if (tmp.BitLength > Convert.ToInt32(tbKeyLength.Text))
                    {
                        outputLabel.Text = RsaKeyInputCtrl.errorBitLengthShorter;
                        return false;
                    }
                    else
                    {
                        if (paramName == ParameterName.PrivKey) { RsaKey.Instance.setPrivKey(inputText, radix); }
                        if (paramName == ParameterName.Modulus) { RsaKey.Instance.setModulus(inputText, radix); }
                        outputLabel.Text = string.Empty;
                        return true;
                    }
                }
                else
                {
                    outputLabel.Text = RsaKeyInputCtrl.errorValidSignsOnly;
                    return false;
                }
            }
            else
            {
                outputLabel.Text = RsaKeyInputCtrl.errorInsertNumber;
                return false;
            }
            return false;
        }

        #region IPkcs1UserControl Member

        void IPkcs1UserControl.Dispose()
        {
            //throw new NotImplementedException();
        }

        void IPkcs1UserControl.Init()
        {
            //throw new NotImplementedException();
        }

        void IPkcs1UserControl.SetTab(int i)
        {
            //throw new NotImplementedException();
        }

        #endregion

        private void btnValInput_Click(object sender, RoutedEventArgs e)
        {
            RsaKey.Instance.setInputParams();
        }

        private bool isInputInRightFormat(string input, int radix)
        {
            if (10 == radix)
            {
                Match invalid_chars = Regex.Match(input, "[^0-9]");
                return !invalid_chars.Success;
            }
            else if (16 == radix)
            {
                Match invalid_chars = Regex.Match(input, "[^0-9a-fA-F]");
                return !invalid_chars.Success;
            }
            return false;
        }

        private void testAndEnableButton()
        {
            if (m_bModulusValid &&
                m_bPrivKeyValid &&
                tbPubKey.Text != string.Empty)
            {
                this.btnValInput.IsEnabled = true;
                this.lblResult.Text = string.Empty;
            }
            else
            {
                this.btnValInput.IsEnabled = false;
            }
        }

        private void btnDecimal_Click(object sender, RoutedEventArgs e)
        {
            if (sender == btnModDecimal)
            {
                m_radixModulus = 10;
                m_bModulusValid = this.checkInputTextBox(tbModulus.Text, m_radixModulus, lblErrorModulus, ParameterName.Modulus);
            }
            else if (sender == btnPrivKeyDecimal)
            {
                m_radixPrivKey = 10;
                m_bPrivKeyValid = this.checkInputTextBox(tbPrivKey.Text, m_radixPrivKey, lblErrorPrivKey, ParameterName.PrivKey);
            }
            testAndEnableButton();
        }

        private void btnHexadec_Click(object sender, RoutedEventArgs e)
        {
            if (sender == btnModHexadec)
            {
                m_radixModulus = 16;
                m_bModulusValid = this.checkInputTextBox(tbModulus.Text, m_radixModulus, lblErrorModulus, ParameterName.Modulus);
            }
            else if (sender == btnPrivKeyHexadec)
            {
                m_radixPrivKey = 16;
                m_bPrivKeyValid = this.checkInputTextBox(tbPrivKey.Text, m_radixPrivKey, lblErrorPrivKey, ParameterName.PrivKey);
            }
            testAndEnableButton();
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
