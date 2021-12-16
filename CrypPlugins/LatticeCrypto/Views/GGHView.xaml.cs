using LatticeCrypto.Properties;
using LatticeCrypto.Utilities;
using LatticeCrypto.ViewModels;
using System.Collections.Generic;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LatticeCrypto.Views
{
    /// <summary>
    /// Interaktionslogik für GGHView.xaml
    /// </summary>
    public partial class GGHView : ILatticeCryptoUserControl
    {
        private GGHViewModel viewModel;

        public GGHView()
        {
            Initialized += delegate
            {
                History.Document.Blocks.Clear();
                viewModel = (GGHViewModel)DataContext;
                viewModel.History = History;
                viewModel.LeftGrid = leftGrid;
                viewModel.RightGrid = rightGrid;
                viewModel.GenerateNewGGH((int)scrollBar.Value, (int)scrollBar2.Value);
                viewModel.UpdateTextBoxes();
                message.Text = Languages.defaultMessageGGH;
            };

            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            viewModel.GenerateNewGGH((int)scrollBar.Value, (int)scrollBar2.Value);
            viewModel.UpdateTextBoxes();
        }

        private void GridSplitter_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (rowGGH.Height == new GridLength(0))
            {
                rowGGH.Height = new GridLength(1, GridUnitType.Star);
                rowLog.Height = new GridLength(55);
            }
            else
            {
                rowGGH.Height = new GridLength(0);
                rowLog.Height = new GridLength(1, GridUnitType.Star);
            }
        }


        private void History_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (History.Text.EndsWith("\r\n"))
            //    History.ScrollToEnd();
        }

        #region Implementation of ILatticeCryptoUserControl

        public void Dispose()
        {
            //throw new System.NotImplementedException();
        }

        public void Init()
        {
            //throw new System.NotImplementedException();
        }

        public void SetTab(int i)
        {
            //throw new System.NotImplementedException();
        }

        #endregion

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            LatticeManualInputView inputView = new LatticeManualInputView((int)scrollBar.Value, (int)scrollBar.Value, 0, viewModel.PrivateKeyR, true, null);
            if (inputView.ShowDialog() != true)
            {
                return;
            }

            viewModel.Dim = (int)scrollBar.Value;
            viewModel.PrivateKeyR = inputView.returnLattice.ToMatrixND();
            viewModel.UpdateTextBoxes();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            LatticeManualInputView inputView = new LatticeManualInputView((int)scrollBar.Value, (int)scrollBar.Value, 0, viewModel.PublicKeyB, true, null);
            if (inputView.ShowDialog() != true)
            {
                return;
            }

            viewModel.Dim = (int)scrollBar.Value;
            viewModel.PublicKeyB = inputView.returnLattice.ToMatrixND();
            viewModel.UpdateTextBoxes();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            LatticeManualInputView inputView = new LatticeManualInputView((int)scrollBar.Value, 0, viewModel.ErrorVector, new List<BigInteger> { -1, 1 });
            if (inputView.ShowDialog() != true)
            {
                return;
            }

            viewModel.ErrorVector = inputView.returnLattice.Vectors[0];
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            switch (cbModus.SelectedIndex)
            {
                //Encrypt
                case 0:
                    if (string.IsNullOrEmpty(viewModel.Message))
                    {
                        MessageBox.Show(Languages.errorNoMessage, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    viewModel.Encrypt();
                    break;
                //Decrypt
                case 1:
                    if (string.IsNullOrEmpty(viewModel.Cipher))
                    {
                        MessageBox.Show(Languages.errorNoCipher, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    if (!viewModel.CheckCipherFormat())
                    {
                        MessageBox.Show(Languages.errorCipherWrongFormat, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    viewModel.Decrypt();
                    break;
            }
        }
    }
}
