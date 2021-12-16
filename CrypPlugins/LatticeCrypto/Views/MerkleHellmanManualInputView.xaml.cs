using LatticeCrypto.Models;
using LatticeCrypto.Properties;
using LatticeCrypto.Utilities;
using System;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;

namespace LatticeCrypto.Views
{
    /// <summary>
    /// Interaktionslogik für LatticeManualInputView.xaml
    /// </summary>
    public partial class MerkleHellmanViewManualInputView
    {
        private MerkleHellmanModel merkleHellman;

        public MerkleHellmanViewManualInputView(MerkleHellmanModel currentMerkleHellman)
        {
            InitializeComponent();
            merkleHellman = currentMerkleHellman;
            InitGui();
        }

        public void InitGui()
        {
            privateKey.Text = merkleHellman.privateKey.ToString();
            mod.Text = merkleHellman.mod.ToString();
            r.Text = merkleHellman.r.ToString();

            privateKey.TextChanged += ValidateInput;
            privateKey.GotKeyboardFocus += (sender, args) => ((TextBox)sender).SelectAll();
            mod.TextChanged += ValidateInput;
            mod.GotKeyboardFocus += (sender, args) => ((TextBox)sender).SelectAll();
            r.TextChanged += ValidateInput;
            r.GotKeyboardFocus += (sender, args) => ((TextBox)sender).SelectAll();

            privateKey.Focus();
        }

        private void ValidateInput(object sender, TextChangedEventArgs e)
        {
            errorText.Text = "";
            buttonOK.IsEnabled = true;

            VectorND newPrivateKey;

            try
            {
                newPrivateKey = Util.ConvertStringToVectorND(privateKey.Text);
            }
            catch (Exception)
            {
                errorText.Text = Languages.errorWrongVectorFormat;
                buttonOK.IsEnabled = false;
                return;
            }
            if (!MerkleHellmanModel.IsSuperincreasingSequence(newPrivateKey.values))
            {
                errorText.Text = Languages.errorPrivateKeyNotSuperincreasing;
                buttonOK.IsEnabled = false;
                return;
            }
            if (!BigInteger.TryParse(mod.Text, out BigInteger newMod))
            {
                errorText.Text = Languages.errorOnlyIntegersAllowed;
                buttonOK.IsEnabled = false;
                return;
            }
            if (newMod < newPrivateKey.values.Aggregate<BigInteger, BigInteger>(0, (current, value) => current + value))
            {
                errorText.Text = Languages.errorModBiggerSum;
                buttonOK.IsEnabled = false;
                return;
            }
            if (!BigInteger.TryParse(r.Text, out BigInteger newR))
            {
                errorText.Text = Languages.errorOnlyIntegersAllowed;
                buttonOK.IsEnabled = false;
                return;
            }
            if (MerkleHellmanModel.Euclid(newR, newMod) != 1)
            {
                errorText.Text = Languages.errorEuclidModR;
                buttonOK.IsEnabled = false;
                return;
            }
            if (cryptoGrid.Children.Cast<Control>().Any(control => control is TextBox && ((TextBox)control).Text.Equals("")))
            {
                errorText.Text = Languages.errorNoCryotosystemEntered;
                buttonOK.IsEnabled = false;
                return;
            }

            merkleHellman = new MerkleHellmanModel(newPrivateKey, newR, newMod);
        }


        public MerkleHellmanModel GetMerkleHellman()
        {
            return merkleHellman;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
