using LatticeCrypto.Properties;
using LatticeCrypto.Utilities;
using LatticeCrypto.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LatticeCrypto.Views
{
    /// <summary>
    /// Interaktionslogik für MerkleHellman.xaml
    /// </summary>
    public partial class RSAView : ILatticeCryptoUserControl
    {
        private RSAViewModel viewModel;

        public RSAView()
        {
            Initialized += delegate
            {
                History.Document.Blocks.Clear();
                viewModel = (RSAViewModel)DataContext;
                viewModel.History = History;
                viewModel.GenerateNewRSA((int)scrollBar.Value);
                message.Text = Languages.defaultMessageRSA;
            };

            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            viewModel.GenerateNewRSA((int)scrollBar.Value);
        }

        //private void ButtonMarkAsKnown(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    ((RSAViewModel)DataContext).SetKnownMessageRange(message.SelectionStart, message.SelectionLength);
        //}

        private void ButtonMarkAsUnknown(object sender, RoutedEventArgs e)
        {
            viewModel.SetUnknownMessageRange(message.SelectionStart, message.SelectionLength);
        }

        private void message_SelectionChanged(object sender, RoutedEventArgs e)
        {
            buttonMarkAsUnknown.IsEnabled = message.SelectionLength > 0;
        }

        private void GridSplitter_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (rowRSA.Height == new GridLength(0))
            {
                rowRSA.Height = new GridLength(1, GridUnitType.Star);
                rowLog.Height = new GridLength(55);
            }
            else
            {
                rowRSA.Height = new GridLength(0);
                rowLog.Height = new GridLength(1, GridUnitType.Star);
            }
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
                //Cryptanalysis
                case 2:
                    if (viewModel.unknownStart == 0 && viewModel.unknownLength == 0)
                    {
                        MessageBox.Show(Languages.infoMarkKnownPlaintext, Languages.information, MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
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


        private void message_KeyDown(object sender, KeyEventArgs e)
        {
            if (((TextBox)sender).Text.Length >= ((RSAViewModel)DataContext).BlockSize)
            {
                e.Handled = true;
            }
        }

        private void message_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).Text.Length >= ((RSAViewModel)DataContext).BlockSize)
            {
                ((TextBox)sender).Text = ((TextBox)sender).Text.Substring(0, ((RSAViewModel)DataContext).BlockSize);
                ToolTip toolTip = (ToolTip)((TextBox)sender).ToolTip ?? new ToolTip();
                toolTip.Content = "Für diesen Angriff muss die Nachricht kleiner als N sein!";
                toolTip.IsOpen = true;
                ToolTipService.SetShowDuration(toolTip, 5);
                ((TextBox)sender).ToolTip = toolTip;
            }
            else if (((TextBox)sender).ToolTip != null)
            {
                ((ToolTip)((TextBox)sender).ToolTip).IsOpen = false;
            }
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
    }
}
