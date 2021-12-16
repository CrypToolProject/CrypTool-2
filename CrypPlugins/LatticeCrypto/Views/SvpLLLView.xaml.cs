using CrypTool.PluginBase.Miscellaneous;
using LatticeCrypto.Properties;
using LatticeCrypto.Utilities;
using LatticeCrypto.ViewModels;
using Microsoft.Win32;
using System;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LatticeCrypto.Views
{
    /// <summary>
    /// Interaktionslogik für SvpLLLView.xaml
    /// </summary>
    public partial class SvpLLLView : ILatticeCryptoUserControl
    {
        private SvpLLLViewModel viewModel;

        public SvpLLLView()
        {
            Initialized += delegate
                               {
                                   History.Document.Blocks.Clear();
                                   viewModel = (SvpLLLViewModel)DataContext;
                                   viewModel.History = History;
                                   viewModel.SetInitialNDLattice();
                                   UpdateTextBoxes();
                               };
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            viewModel.GenerateNewLattice((int)scrollBarN.Value, (int)scrollBarM.Value, BigInteger.Parse(textRangeStart.Text), BigInteger.Parse(textRangeEnd.Text));
            UpdateTextBoxes();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            LatticeManualInputView inputView = new LatticeManualInputView((int)scrollBarN.Value, (int)scrollBarM.Value, viewModel.Lattice, false, false, 0, null);
            if (inputView.ShowDialog() != true)
            {
                return;
            }

            Cursor = Cursors.Wait;
            viewModel.SetLatticeManually(inputView.returnLattice);

            scrollBarN.Value = viewModel.Lattice.N;
            scrollBarM.Value = viewModel.Lattice.M;
            if (viewModel.Lattice.N == viewModel.Lattice.M)
            {
                scrollBarDim.Value = viewModel.Lattice.N;
            }

            UpdateTextBoxes();
            Cursor = Cursors.Arrow;
        }

        private void ButtonLoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*" };
            if (openFileDialog.ShowDialog() == false)
            {
                return;
            }

            string firstLine;
            try
            {
                firstLine = File.ReadAllLines(openFileDialog.FileName)[0];
            }
            catch (IOException)
            {
                MessageBox.Show(Languages.errorLoadingFile, Languages.error, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                viewModel.SetLatticeManually(Util.ConvertStringToLatticeND(firstLine));

                scrollBarN.Value = viewModel.Lattice.N;
                scrollBarM.Value = viewModel.Lattice.M;
                if (viewModel.Lattice.N == viewModel.Lattice.M)
                {
                    scrollBarDim.Value = viewModel.Lattice.N;
                }

                UpdateTextBoxes();
            }
            catch (Exception)
            {
                MessageBox.Show(Languages.errorParsingLattice, Languages.error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ButtonLoadFromClipboard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string str = Clipboard.GetText();
                viewModel.SetLatticeManually(Util.ConvertStringToLatticeND(str));

                scrollBarN.Value = viewModel.Lattice.N;
                scrollBarM.Value = viewModel.Lattice.M;
                if (viewModel.Lattice.N == viewModel.Lattice.M)
                {
                    scrollBarDim.Value = viewModel.Lattice.N;
                }

                UpdateTextBoxes();
            }
            catch (Exception)
            {
                MessageBox.Show(Languages.errorParsingLattice, Languages.error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void UpdateTextBoxes()
        {
            int cols = !viewModel.Lattice.UseRowVectors ? viewModel.Lattice.N : viewModel.Lattice.M;
            int rows = !viewModel.Lattice.UseRowVectors ? viewModel.Lattice.M : viewModel.Lattice.N;

            if (leftGrid.RowDefinitions.Count != rows || leftGrid.ColumnDefinitions.Count != cols)
            {
                leftGrid.RowDefinitions.Clear();
                leftGrid.ColumnDefinitions.Clear();
                rightGrid.RowDefinitions.Clear();
                rightGrid.ColumnDefinitions.Clear();

                for (int i = 0; i < cols; i++)
                {
                    leftGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    rightGrid.ColumnDefinitions.Add(new ColumnDefinition());
                }

                for (int i = 0; i < rows; i++)
                {
                    leftGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35) });
                    rightGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35) });
                }

                leftGrid.Children.Clear();
                rightGrid.Children.Clear();

                for (int i = 0; i < cols; i++)
                {
                    for (int j = 0; j < rows; j++)
                    {
                        TextBlock leftTextBlock = new SelectableTextBlock
                        {
                            Text = Util.FormatBigInt(viewModel.Lattice.Vectors[!viewModel.Lattice.UseRowVectors ? i : j].values[!viewModel.Lattice.UseRowVectors ? j : i]),
                            Margin = new Thickness(10, 0, 10, 0),
                            TextAlignment = TextAlignment.Right
                        };
                        Grid.SetColumn(leftTextBlock, i);
                        Grid.SetRow(leftTextBlock, j);
                        leftGrid.Children.Add(leftTextBlock);

                        TextBlock rightTextBlock = new SelectableTextBlock
                        {
                            Text = Util.FormatBigInt(viewModel.Lattice.ReducedVectors[!viewModel.Lattice.UseRowVectors ? i : j].values[!viewModel.Lattice.UseRowVectors ? j : i]),
                            Margin = new Thickness(10, 0, 10, 0),
                            TextAlignment = TextAlignment.Right
                        };
                        Grid.SetColumn(rightTextBlock, i);
                        Grid.SetRow(rightTextBlock, j);
                        rightGrid.Children.Add(rightTextBlock);
                    }
                }
            }
            else
            {
                if (!viewModel.Lattice.UseRowVectors)
                {
                    foreach (TextBlock textBlock in leftGrid.Children)
                    {
                        textBlock.Text = Util.FormatBigInt(viewModel.Lattice.Vectors[Grid.GetColumn(textBlock)].values[Grid.GetRow(textBlock)]);
                    }

                    foreach (TextBlock textBlock in rightGrid.Children)
                    {
                        textBlock.Text = Util.FormatBigInt(viewModel.Lattice.ReducedVectors[Grid.GetColumn(textBlock)].values[Grid.GetRow(textBlock)]);
                    }
                }
                else
                {
                    foreach (TextBlock textBlock in leftGrid.Children)
                    {
                        textBlock.Text = Util.FormatBigInt(viewModel.Lattice.Vectors[Grid.GetRow(textBlock)].values[Grid.GetColumn(textBlock)]);
                    }

                    foreach (TextBlock textBlock in rightGrid.Children)
                    {
                        textBlock.Text = Util.FormatBigInt(viewModel.Lattice.ReducedVectors[Grid.GetRow(textBlock)].values[Grid.GetColumn(textBlock)]);
                    }
                }
            }
        }


        private void ButtonLog_Click(object sender, RoutedEventArgs e)
        {
            rowLog.Height = rowLog.Height == new GridLength(0) ? new GridLength(55) : new GridLength(0);
        }

        private void ValidateCodomain(object sender, TextChangedEventArgs e)
        {
            if (errorText == null || buttonGenerate == null)
            {
                return;
            }

            errorText.Text = "";
            buttonGenerate.IsEnabled = true;
            errorText.Visibility = Visibility.Collapsed;

            if (textRangeStart.Text.Equals("") || textRangeEnd.Text.Equals(""))
            {
                errorText.Text = Languages.errorNoCodomain;
                buttonGenerate.IsEnabled = false;
                errorText.Visibility = Visibility.Visible;
                return;
            }

            BigInteger tryParseEnd;
            if (!BigInteger.TryParse(textRangeStart.Text, out BigInteger tryParseStart) || !BigInteger.TryParse(textRangeEnd.Text, out tryParseEnd))
            {
                errorText.Text = Languages.errorOnlyIntegersAllowed;
                buttonGenerate.IsEnabled = false;
                errorText.Visibility = Visibility.Visible;
                return;
            }
            if (tryParseStart < tryParseEnd)
            {
                return;
            }

            errorText.Text = Languages.errorFromBiggerThanTo;
            buttonGenerate.IsEnabled = false;
            errorText.Visibility = Visibility.Visible;
        }

        private void GridSplitter_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (rowLattice.Height == new GridLength(0))
            {
                rowLattice.Height = new GridLength(1, GridUnitType.Star);
                rowLog.Height = new GridLength(55);
            }
            else
            {
                rowLattice.Height = new GridLength(0);
                rowLog.Height = new GridLength(1, GridUnitType.Star);
            }
        }

        private void Button_Help_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Equals(sender, btnHelpCodomain))
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(OnlineHelp.OnlineHelpActions.CodomainLLL);
            }
            else if (Equals(sender, btnHelpDimension))
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(OnlineHelp.OnlineHelpActions.DimensionLLL);
            }
            e.Handled = true;
        }

        private void History_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (History.Text.EndsWith("\r\n"))
            //    History.ScrollToEnd();
        }

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

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            scrollBarM.Value = scrollBarDim.Value;
            scrollBarN.Value = scrollBarDim.Value;
        }

        private void checkBoxMxN_Checked(object sender, RoutedEventArgs e)
        {
            textDim.IsEnabled = false;
            scrollBarDim.IsEnabled = false;
            textM.IsEnabled = true;
            scrollBarM.IsEnabled = true;
            textN.IsEnabled = true;
            scrollBarN.IsEnabled = true;
        }

        private void checkBoxMxN_Unchecked(object sender, RoutedEventArgs e)
        {
            textDim.IsEnabled = true;
            scrollBarDim.IsEnabled = true;
            textM.IsEnabled = false;
            scrollBarM.IsEnabled = false;
            textN.IsEnabled = false;
            scrollBarN.IsEnabled = false;
        }
    }
}
