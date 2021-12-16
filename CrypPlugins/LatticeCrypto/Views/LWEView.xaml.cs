using LatticeCrypto.Properties;
using LatticeCrypto.Utilities;
using LatticeCrypto.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace LatticeCrypto.Views
{
    /// <summary>
    /// Interaktionslogik für GGHView.xaml
    /// </summary>
    public partial class LWEView : ILatticeCryptoUserControl
    {
        private LWEViewModel viewModel;

        public LWEView()
        {
            Initialized += delegate
            {
                History.Document.Blocks.Clear();
                viewModel = (LWEViewModel)DataContext;
                viewModel.History = History;
                viewModel.GridS = gridS;
                viewModel.GridA = gridA;
                viewModel.GridB = gridB;
                viewModel.GenerateNewLWE((int)scrollBar.Value, (int)scrollBar2.Value);

                viewModel.UpdateTextBoxes();
            };

            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            viewModel.GenerateNewLWE((int)scrollBar.Value, (int)scrollBar2.Value);
            viewModel.UpdateTextBoxes();
        }

        private void GridSplitter_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (rowLWE.Height == new GridLength(0))
            {
                rowLWE.Height = new GridLength(1, GridUnitType.Star);
                rowLog.Height = new GridLength(55);
            }
            else
            {
                rowLWE.Height = new GridLength(0);
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
