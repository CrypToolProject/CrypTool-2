using LatticeCrypto.Models;
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
    /// Interaktionslogik für SvpGaussView.xaml
    /// </summary>
    public partial class SvpGaussView : ILatticeCryptoUserControl
    {
        private Point point;
        private bool isGrabbing;
        private SvpGaussViewModel viewModel;

        public SvpGaussView()
        {
            Initialized += delegate
            {
                History.Document.Blocks.Clear();
                viewModel = (SvpGaussViewModel)DataContext;
                viewModel.History = History;
                viewModel.canvas = canvas;
                viewModel.SetInitialNDLattice();
                UpdateTextBoxes();
            };

            InitializeComponent();

            SizeChanged += delegate
            {
                if (rowLattice.Height != new GridLength(0))
                {
                    viewModel.CalculatePixelsPerPoint();
                }
            };
            canvas.SizeChanged += delegate
            {
                viewModel.GenerateLatticePoints(false, true);
            };
        }

        private void UpdateTextBoxes()
        {
            LatticeND lattice = viewModel.Lattice;
            textBasisLength0.Text = Util.FormatDoubleGUI(lattice.Vectors[0].Length);
            textBasisLength1.Text = Util.FormatDoubleGUI(lattice.Vectors[1].Length);
            textReducedLength0.Text = Util.FormatDoubleGUI(lattice.ReducedVectors[0].Length);
            textReducedLength1.Text = Util.FormatDoubleGUI(lattice.ReducedVectors[1].Length);
            textBasis00.Text = Util.FormatBigInt(lattice.Vectors[0].values[0]);
            textBasis11.Text = Util.FormatBigInt(lattice.Vectors[1].values[1]);
            textReduced00.Text = Util.FormatBigInt(lattice.ReducedVectors[0].values[0]);
            textReduced11.Text = Util.FormatBigInt(lattice.ReducedVectors[1].values[1]);

            if (!lattice.UseRowVectors)
            {
                textBasis01.Text = Util.FormatBigInt(lattice.Vectors[0].values[1]);
                textBasis10.Text = Util.FormatBigInt(lattice.Vectors[1].values[0]);
                textReduced01.Text = Util.FormatBigInt(lattice.ReducedVectors[0].values[1]);
                textReduced10.Text = Util.FormatBigInt(lattice.ReducedVectors[1].values[0]);
                SepColumn.Width = new GridLength(10);
                SepColumnRed.Width = new GridLength(10);
                SepRow.Height = new GridLength(0);
                SepRowRed.Height = new GridLength(0);
            }
            else
            {
                textBasis01.Text = Util.FormatBigInt(lattice.Vectors[1].values[0]);
                textBasis10.Text = Util.FormatBigInt(lattice.Vectors[0].values[1]);
                textReduced01.Text = Util.FormatBigInt(lattice.ReducedVectors[1].values[0]);
                textReduced10.Text = Util.FormatBigInt(lattice.ReducedVectors[0].values[1]);
                SepColumn.Width = new GridLength(0);
                SepColumnRed.Width = new GridLength(0);
                SepRow.Height = new GridLength(10);
                SepRowRed.Height = new GridLength(10);
            }

            textDeterminant.Text = Util.FormatBigInt(lattice.Determinant);
        }

        private void ButtonCamera_Click(object sender, RoutedEventArgs e)
        {
            Util.CreateSaveBitmap(canvas);
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (toggleScrollLattice.IsChecked == true)
            {
                if (!latticeGrid.IsMouseCaptured)
                {
                    return;
                }

                viewModel.SetCanvasPosition(point, e.GetPosition(this));
                viewModel.GenerateLatticePoints(false, true);
            }
            else if (!isGrabbing)
            {
                ToggleMouseGrabbing(e);
            }
            else
            {
                viewModel.ChangeVectorToSelectedPoint(e.GetPosition(canvas));
                UpdateTextBoxes();
            }
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (toggleChangeLattice.IsChecked == true && viewModel.IsPointBasisLatticeVector(e.GetPosition(canvas)))
            {
                latticeGrid.CaptureMouse();
                System.Windows.Resources.StreamResourceInfo info = Application.GetResourceStream(new Uri("/LatticeCrypto;component/Utilities/Cursor/grabbing.cur", UriKind.Relative));
                if (info != null)
                {
                    Cursor = new Cursor(info.Stream);
                }

                viewModel.StopLatticeBasisPointsBlink();
                isGrabbing = true;
            }
            else if (toggleScrollLattice.IsChecked == true)
            {
                point = e.GetPosition(this);
                latticeGrid.CaptureMouse();
                Cursor = Cursors.SizeAll;
            }
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (toggleScrollLattice.IsChecked == true)
            {
                viewModel.SetCanvasTransform(canvas.RenderTransform);
                Cursor = Cursors.Arrow;
            }
            else
            {
                viewModel.SetLatticeManually(viewModel.Lattice);
                viewModel.CalculatePixelsPerPoint();
                viewModel.GenerateLatticePoints(true, true);
                viewModel.BeginnLatticeBasisPointsBlink();
                ToggleMouseGrabbing(e);
                isGrabbing = false;
            }
            latticeGrid.ReleaseMouseCapture();
        }

        private void ToggleMouseGrabbing(MouseEventArgs e)
        {
            if (viewModel.IsPointBasisLatticeVector(e.GetPosition(canvas)))
            {
                System.Windows.Resources.StreamResourceInfo info = Application.GetResourceStream(new Uri("/LatticeCrypto;component/Utilities/Cursor/grab.cur", UriKind.Relative));
                if (info != null)
                {
                    Cursor = new Cursor(info.Stream);
                }
            }
            else
            {
                Cursor = Cursors.Arrow;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                viewModel.GenerateNewLattice(2, 2, BigInteger.Parse(textRangeStart.Text), BigInteger.Parse(textRangeEnd.Text));
                UpdateTextBoxes();
                if (rowLattice.Height == new GridLength(0))
                {
                    return;
                }

                viewModel.ResetCanvasPosition();
                viewModel.CalculatePixelsPerPoint();
                viewModel.GenerateLatticePoints(true, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Languages.error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0 && zoomIn.Command.CanExecute(null))
            {
                zoomIn.Command.Execute(null);
            }
            else if (e.Delta < 0 && zoomOut.Command.CanExecute(null))
            {
                zoomOut.Command.Execute(null);
            }
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            LatticeManualInputView inputView = new LatticeManualInputView(2, 2, viewModel.Lattice, true, true, 0, null);
            if (inputView.ShowDialog() != true)
            {
                return;
            }

            viewModel.ResetCanvasPosition();
            viewModel.SetLatticeManually(inputView.returnLattice);
            viewModel.CalculatePixelsPerPoint();
            viewModel.GenerateLatticePoints(true, true);
            UpdateTextBoxes();
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
            if (tryParseStart >= tryParseEnd)
            {
                errorText.Text = Languages.errorFromBiggerThanTo;
                buttonGenerate.IsEnabled = false;
                errorText.Visibility = Visibility.Visible;
                return;
            }
            BigInteger maxCodomain = BigInteger.Max(BigInteger.Abs(tryParseStart), BigInteger.Abs(tryParseEnd));
            BigInteger range = BigInteger.Abs(tryParseEnd - tryParseStart);
            if (range * 5 >= maxCodomain)
            {
                return;
            }

            errorText.Text = Languages.errorCodomainTooSmall;
            errorText.Visibility = Visibility.Visible;
        }

        private void History_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (History.Text.EndsWith("\r\n"))
            //    History.ScrollToEnd();
        }

        private void toggleChangeLattice_Checked(object sender, RoutedEventArgs e)
        {
            toggleScrollLattice.IsChecked = false;
            toggleScrollLattice.IsEnabled = true;
            toggleChangeLattice.IsEnabled = false;
            if (viewModel != null)
            {
                viewModel.IsBlinking = true;
            }
        }

        private void toggleScrollLattice_Checked(object sender, RoutedEventArgs e)
        {
            toggleChangeLattice.IsChecked = false;
            toggleChangeLattice.IsEnabled = true;
            toggleScrollLattice.IsEnabled = false;
            if (viewModel != null)
            {
                viewModel.IsBlinking = false;
            }
        }

        private void ButtonLog_Click(object sender, RoutedEventArgs e)
        {
            rowLog.Height = rowLog.Height == new GridLength(0) ? new GridLength(55) : new GridLength(0);
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public void Init()
        {
            //throw new NotImplementedException();
        }

        public void SetTab(int i)
        {
            //throw new NotImplementedException();
        }

        private void ButtonLoad_Click(object sender, RoutedEventArgs e)
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
                viewModel.ResetCanvasPosition();
                viewModel.CalculatePixelsPerPoint();
                viewModel.GenerateLatticePoints(true, true);
                UpdateTextBoxes();
            }
            catch (Exception)
            {
                MessageBox.Show(Languages.errorParsingLattice, Languages.error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                string str = Clipboard.GetText();
                viewModel.SetLatticeManually(Util.ConvertStringToLatticeND(str));
                viewModel.ResetCanvasPosition();
                viewModel.CalculatePixelsPerPoint();
                viewModel.GenerateLatticePoints(true, true);
                UpdateTextBoxes();
            }
            catch (Exception)
            {
                MessageBox.Show(Languages.errorParsingLattice, Languages.error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Help_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Equals(sender, btnHelpCodomain))
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(OnlineHelp.OnlineHelpActions.Codomain);
            }
            else if (Equals(sender, btnHelpAngleVectors) || Equals(sender, btnHelpAngleReducedVectors))
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(OnlineHelp.OnlineHelpActions.AngleVectors);
            }
            else if (Equals(sender, btnHelpVectorLength))
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(OnlineHelp.OnlineHelpActions.LengthVector);
            }
            else if (Equals(sender, btnHelpDeterminantLattice))
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(OnlineHelp.OnlineHelpActions.DeterminantLattice);
            }
            else if (Equals(sender, btnHelpSuccessiveMinima))
            {
                OnlineHelp.OnlineHelpAccess.ShowOnlineHelp(OnlineHelp.OnlineHelpActions.SuccessiveMinima);
            }

            e.Handled = true;
        }
    }
}
