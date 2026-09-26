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
using LatticeCrypto.Properties;
using LatticeCrypto.Utilities;
using LatticeCrypto.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MessageBox = System.Windows.MessageBox;

namespace LatticeCrypto.Views
{
    /// <summary>
    /// Interaktionslogik für MerkleHellman.xaml
    /// </summary>
    public partial class MerkleHellmanView : ILatticeCryptoUserControl
    {
        private MerkleHellmanViewModel viewModel;

        public MerkleHellmanView()
        {
            Initialized += delegate
            {
                History.Document.Blocks.Clear();
                viewModel = (MerkleHellmanViewModel)DataContext;
                viewModel.History = History;
                viewModel.GenerateNewMerkleHellman((int)scrollBar.Value);
                message.Text = Languages.defaultMessageGGH;
            };

            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            viewModel.GenerateNewMerkleHellman((int)scrollBar.Value);
        }

        private void GridSplitter_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (rowMarkleHellman.Height == new GridLength(0))
            {
                rowMarkleHellman.Height = new GridLength(1, GridUnitType.Star);
                rowLog.Height = new GridLength(55);
            }
            else
            {
                rowMarkleHellman.Height = new GridLength(0);
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
                //Cryptanalysis
                case 2:
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
                    viewModel.Cryptanalysis();
                    break;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MerkleHellmanViewManualInputView inputView = new MerkleHellmanViewManualInputView(viewModel.MerkleHellman);
            if (inputView.ShowDialog() != true)
            {
                return;
            }

            viewModel.SetCryptosystemManually(inputView.GetMerkleHellman());
        }
    }
}
