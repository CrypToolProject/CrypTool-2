using LatticeCrypto.Models;
using LatticeCrypto.Properties;
using LatticeCrypto.Utilities;
using LatticeCrypto.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;

namespace LatticeCrypto.Views
{
    /// <summary>
    /// Interaktionslogik für LatticeManualInputView.xaml
    /// </summary>
    public partial class LatticeManualInputView
    {
        public int n;
        public int m;
        public int mod;
        public LatticeND oldLattice;
        public LatticeND returnLattice;
        private readonly bool allowOnly2x2;
        private LatticeManualInputViewModel viewModel;
        private bool useRowVectors;
        private readonly bool notAllowDetZero;
        private readonly List<BigInteger> allowedNumbers;

        public LatticeManualInputView(int newM, int newMod, VectorND currentVector, List<BigInteger> allowedNumbers)
            : this(1, newM, new LatticeND(new[] { currentVector }, false), false, false, newMod, allowedNumbers)
        {
            ButtonTranspose.Visibility = Visibility.Hidden;
        }

        public LatticeManualInputView(int newN, int newM, int newMod, MatrixND currentMatrix, bool notAllowDetZero, List<BigInteger> allowedNumbers)
            : this(newN, newM, currentMatrix.ToLatticeND(), false, notAllowDetZero, newMod, allowedNumbers)
        { }

        public LatticeManualInputView(int newN, int newM, LatticeND currentLattice, bool allowOnly2x2, bool notAllowDetZero, int newMod, List<BigInteger> allowedNumbers)
        {
            n = newN;
            m = newM;
            mod = newMod;
            this.allowOnly2x2 = allowOnly2x2;
            this.notAllowDetZero = notAllowDetZero;
            this.allowedNumbers = allowedNumbers;

            Initialized += delegate
            {
                viewModel = (LatticeManualInputViewModel)DataContext;
                viewModel.NewLattice(n, m);

                if (n == currentLattice.N && m == currentLattice.M)
                {
                    oldLattice = currentLattice;
                    CBRowVectors.IsChecked = oldLattice.UseRowVectors;
                    BuildLatticeGrid(currentLattice, false);
                }
                else
                {
                    oldLattice = null;
                    BuildLatticeGrid(null, false);
                }
            };
            InitializeComponent();
        }

        private void BuildLatticeGrid(LatticeND lattice, bool justUpdate)
        {
            int cols = !useRowVectors ? n : m;
            int rows = !useRowVectors ? m : n;

            if (!justUpdate)
            {
                latticeGrid.RowDefinitions.Clear();
                latticeGrid.ColumnDefinitions.Clear();
                latticeGrid.Children.Clear();

                for (int i = 0; i < cols; i++)
                {
                    latticeGrid.ColumnDefinitions.Add(new ColumnDefinition());

                    //Zusatzspalten für Zwischenräume
                    if (i < cols - 1 && !useRowVectors)
                    {
                        latticeGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    }
                }

                for (int i = 0; i < rows; i++)
                {
                    latticeGrid.RowDefinitions.Add(new RowDefinition());

                    //Zusatzzeilen für Zwischenräume
                    if (i < rows - 1 && useRowVectors)
                    {
                        latticeGrid.RowDefinitions.Add(new RowDefinition());
                    }
                }

                //Leere Labels für den Zwischenraum
                for (int i = 1; i < cols * 2 - 1; i = i + 2)
                {
                    for (int j = 0; j < rows; j++)
                    {
                        Label label = new Label();
                        if (!useRowVectors)
                        {
                            label.MinWidth = 10;
                        }
                        else
                        {
                            label.MinHeight = 10;
                        }

                        Grid.SetColumn(label, !useRowVectors ? i : j);
                        Grid.SetRow(label, !useRowVectors ? j : i);
                        latticeGrid.Children.Add(label);
                    }
                }

                for (int i = 0; i < cols; i++)
                {
                    for (int j = 0; j < rows; j++)
                    {
                        TextBox textBox = new TextBox { MinHeight = 25, MinWidth = 50 };
                        textBox.GotKeyboardFocus += (sender, args) => ((TextBox)sender).SelectAll();
                        textBox.TabIndex = !useRowVectors ? n * i + j : m * j + i;

                        if (lattice != null)
                        {
                            textBox.Text = lattice.Vectors[!useRowVectors ? i : j].values[!useRowVectors ? j : i].ToString(CultureInfo.InvariantCulture);
                        }

                        textBox.TextChanged += ValidateLattice;
                        Grid.SetColumn(textBox, !useRowVectors ? 2 * i : i);
                        Grid.SetRow(textBox, !useRowVectors ? j : 2 * j);
                        latticeGrid.Children.Add(textBox);
                    }
                }
            }
            else
            {
                //Beim Transponieren muss nicht die komplette Grid neu gebaut werden, es reicht ein Update
                foreach (TextBox textBox in latticeGrid.Children.OfType<TextBox>())
                {
                    textBox.TextChanged -= ValidateLattice;
                    int col = Grid.GetColumn(textBox);
                    int row = Grid.GetRow(textBox);
                    textBox.Text = lattice.Vectors[!useRowVectors ? col / 2 : row / 2].values[!useRowVectors ? row : col].ToString(CultureInfo.InvariantCulture);
                    textBox.TextChanged += ValidateLattice;
                }
            }

            foreach (TextBox textBox in latticeGrid.Children.OfType<TextBox>().Where(textBox => textBox.TabIndex == 0))
            {
                textBox.Focus();
                break;
            }
        }

        private void ValidateLattice(object sender, TextChangedEventArgs e)
        {
            errorText.Text = "";
            buttonOK.IsEnabled = true;
            buttonCopy.IsEnabled = true;

            foreach (TextBox textBox in latticeGrid.Children.Cast<Control>().Where(control => control is TextBox && !((TextBox)control).Text.Equals("")).Cast<TextBox>())
            {
                textBox.Background = System.Windows.Media.Brushes.White;

                if (!BigInteger.TryParse(textBox.Text, out BigInteger tryParse))
                {
                    errorText.Text = Languages.errorOnlyIntegersAllowed;
                    buttonOK.IsEnabled = false;
                    buttonCopy.IsEnabled = false;
                    textBox.Background = System.Windows.Media.Brushes.Red;
                    return;
                }
                if (mod != 0 && tryParse >= mod)
                {
                    errorText.Text = string.Format("Es sind nur Zahlen bis {0} erlaubt", mod);
                    buttonOK.IsEnabled = false;
                    buttonCopy.IsEnabled = false;
                    textBox.Background = System.Windows.Media.Brushes.Red;
                    return;
                }
                if (allowedNumbers != null && !allowedNumbers.Exists(x => x == tryParse))
                {
                    string numbers = "";
                    for (int i = 0; i < allowedNumbers.Count; i++)
                    {
                        numbers += allowedNumbers[i];
                        if (i < allowedNumbers.Count - 1)
                        {
                            numbers += ", ";
                        }
                    }
                    errorText.Text = string.Format("Es sind nur die Zahlen {0} erlaubt", numbers);
                    buttonOK.IsEnabled = false;
                    buttonCopy.IsEnabled = false;
                    textBox.Background = System.Windows.Media.Brushes.Red;
                    return;
                }
            }
            if (latticeGrid.Children.Cast<Control>().Any(control => control is TextBox && ((TextBox)control).Text.Equals("")))
            {
                errorText.Text = Languages.errorNoLatticeEntered;
                buttonOK.IsEnabled = false;
                buttonCopy.IsEnabled = false;
                return;
            }
            LatticeND newLattice = viewModel.SetLattice(latticeGrid, CBRowVectors.IsChecked != null && (bool)CBRowVectors.IsChecked);
            if (oldLattice != null && newLattice.Equals(oldLattice))
            {
                errorText.Text = Languages.errorSameLattice;
                return;
            }
            if (n == m && notAllowDetZero && newLattice.Determinant == 0)
            {
                errorText.Text = Languages.errorVectorsDependent;
                buttonOK.IsEnabled = false;
                buttonCopy.IsEnabled = false;
            }
            if (n > 1 && m > 1 && ((newLattice.Vectors[0].Length > newLattice.Vectors[1].Length && newLattice.Vectors[0].Length > 1000 * newLattice.Vectors[1].Length)
                || newLattice.Vectors[1].Length > newLattice.Vectors[0].Length && newLattice.Vectors[1].Length > 1000 * newLattice.Vectors[0].Length))
            {
                errorText.Text = Languages.errorBadLattice;
                //buttonOK.IsEnabled = false;
            }
        }

        private void Button_ClipboardInput(object sender, RoutedEventArgs e)
        {
            try
            {
                string str = Clipboard.GetText();
                LatticeND tmp = Util.ConvertStringToLatticeND(str);
                if (allowOnly2x2 && (tmp.N != 2 || tmp.M != 2))
                {
                    throw new Exception();
                }

                viewModel.Lattice = tmp;
                n = viewModel.Lattice.N;
                m = viewModel.Lattice.M;
                BuildLatticeGrid(viewModel.Lattice, false);
            }
            catch (Exception)
            {
                MessageBox.Show(Languages.errorParsingLattice, Languages.error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            returnLattice = viewModel.SetLattice(latticeGrid, useRowVectors);
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ButtonTranspose_Click(object sender, RoutedEventArgs e)
        {
            LatticeND tempLattice = viewModel.SetLattice(latticeGrid, useRowVectors);
            tempLattice.Transpose();
            int tmp = n;
            n = m;
            m = tmp;
            BuildLatticeGrid(tempLattice, false);
        }

        private void CBRowVectors_Checked(object sender, RoutedEventArgs e)
        {
            useRowVectors = true;
            BuildLatticeGrid(viewModel.SetLattice(latticeGrid, false), false);
        }

        private void CBRowVectors_Unchecked(object sender, RoutedEventArgs e)
        {
            useRowVectors = false;
            BuildLatticeGrid(viewModel.SetLattice(latticeGrid, true), false);
        }

        private void Button_ClipboardOutput(object sender, RoutedEventArgs e)
        {
            string latticeInfos = viewModel.SetLattice(latticeGrid, useRowVectors).LatticeToString();
            Clipboard.SetText(latticeInfos);
        }
    }
}
