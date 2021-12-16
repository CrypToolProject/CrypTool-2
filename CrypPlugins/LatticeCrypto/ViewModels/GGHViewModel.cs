using CrypTool.PluginBase.Miscellaneous;
using LatticeCrypto.Models;
using LatticeCrypto.Properties;
using LatticeCrypto.Utilities;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace LatticeCrypto.ViewModels
{
    public class GGHViewModel : BaseViewModel
    {
        public RichTextBox History { get; set; }
        public ReductionMethods ReductionMethod { get; set; }
        public GGHModel GGH { get; set; }
        public Grid LeftGrid { get; set; }
        public Grid RightGrid { get; set; }
        public string Message { get; set; }
        private VectorND cipher;
        public string Cipher { get; set; }

        public GGHViewModel()
        {
            ReductionMethod = ReductionMethods.reduceLLL;
        }

        public void GenerateNewGGH(int dim, int l)
        {
            UiServices.SetBusyState();
            GGH = GGH != null && GGH.dim == dim ? new GGHModel(dim, l, GGH.errorVector) : new GGHModel(dim, l);

            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + Languages.buttonGenerateNewCryptosystem + " **\r\n"))));
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelPrivateKeyR + ":")));
            paragraph.Inlines.Add(" " + GGH.privateKeyR + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelPrivateKeyR1 + ":")));
            paragraph.Inlines.Add(" " + GGH.privateKeyR1.ToStringLog() + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelPublicKeyB + ":")));
            paragraph.Inlines.Add(" " + GGH.publicKeyB + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelPublicKeyB1 + ":")));
            paragraph.Inlines.Add(" " + GGH.publicKeyB1.ToStringLog() + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelUnimodularTransformationMatrix + ":")));
            paragraph.Inlines.Add(" " + Lattice.LatticeTransformationToString() + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelParameterL)));
            paragraph.Inlines.Add(" " + GGH.l + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelErrorVector)));
            paragraph.Inlines.Add(" " + GGH.errorVector + "\r\n");

            if (History.Document.Blocks.FirstBlock != null)
            {
                History.Document.Blocks.InsertBefore(History.Document.Blocks.FirstBlock, paragraph);
            }
            else
            {
                History.Document.Blocks.Add(paragraph);
            }

            NotifyPropertyChanged("ErrorVector");
        }

        public LatticeND Lattice
        {
            get => GGH.lattice;
            set => GGH.lattice = value;
        }

        public int Dim
        {
            get => GGH != null ? GGH.dim : 2;
            set => GGH.dim = value;
        }

        public MatrixND PrivateKeyR
        {
            get => GGH.privateKeyR;
            set
            {
                GGH.SetPrivateKeyManually(value, true);

                Paragraph paragraph = new Paragraph();
                paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + Languages.buttonSetPrivateKeyR + " **\r\n"))));
                paragraph.Inlines.Add(new Bold(new Run(Languages.labelPrivateKeyR)));
                paragraph.Inlines.Add(" " + GGH.privateKeyR + "\r\n");

                History.Document.Blocks.InsertBefore(History.Document.Blocks.FirstBlock, paragraph);

                GGH.GenerateLattice();
            }
        }

        public MatrixND PublicKeyB
        {
            get => GGH.publicKeyB;
            set
            {
                GGH.SetPublicKeyManually(value);

                Paragraph paragraph = new Paragraph();
                paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + Languages.buttonSetPublicKeyB + " **\r\n"))));
                paragraph.Inlines.Add(new Bold(new Run(Languages.labelPublicKeyB)));
                paragraph.Inlines.Add(" " + GGH.publicKeyB + "\r\n");

                History.Document.Blocks.InsertBefore(History.Document.Blocks.FirstBlock, paragraph);

                GGH.GenerateLattice();
            }
        }

        public VectorND ErrorVector
        {
            get => GGH != null ? GGH.errorVector : new VectorND(0);
            set
            {
                GGH.SetErrorVectorManually(value);

                Paragraph paragraph = new Paragraph();
                paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + Languages.buttonSetErrorVector + " **\r\n"))));
                paragraph.Inlines.Add(new Bold(new Run(Languages.labelErrorVector)));
                paragraph.Inlines.Add(" " + GGH.errorVector + "\r\n");

                History.Document.Blocks.InsertBefore(History.Document.Blocks.FirstBlock, paragraph);

                NotifyPropertyChanged("ErrorVector");
            }
        }

        private RelayCommand generateErrorVectorCommand;
        public RelayCommand GenerateErrorVectorCommand
        {
            get
            {
                if (generateErrorVectorCommand != null)
                {
                    return generateErrorVectorCommand;
                }

                generateErrorVectorCommand = new RelayCommand(
                    parameter1 =>
                        {
                            UiServices.SetBusyState();
                            GGH.GenerateErrorVector();

                            Paragraph paragraph = new Paragraph();
                            paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + Languages.buttonGenerateNewErrorVector + " **\r\n"))));
                            paragraph.Inlines.Add(new Bold(new Run(Languages.labelErrorVector)));
                            paragraph.Inlines.Add(" " + GGH.errorVector + "\r\n");

                            History.Document.Blocks.InsertBefore(History.Document.Blocks.FirstBlock, paragraph);

                            NotifyPropertyChanged("ErrorVector");
                        });
                return generateErrorVectorCommand;
            }
        }

        private bool ValidateCryptosystem()
        {
            if (PrivateKeyR.cols != Dim || PublicKeyB.cols != Dim || ErrorVector.dim != Dim)
            {
                MessageBox.Show(string.Format(Languages.errorWrongGGHCryptosystem, Dim), Languages.error, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (!GGH.DoTheKeysFit())
            {
                MessageBox.Show(Languages.errorPrivateAndPublicKeyDoNotFit, Languages.error, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        public void Encrypt()
        {
            if (!ValidateCryptosystem())
            {
                return;
            }

            UiServices.SetBusyState();
            Message = Message.TrimEnd('\0');
            Cipher = GGH.Encrypt(Message).ToString();

            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + Languages.buttonEncrypt + " **\r\n"))));
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelPlainText)));
            paragraph.Inlines.Add(" " + Message + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelCiphertext)));
            paragraph.Inlines.Add(" " + Cipher + "\r\n");

            History.Document.Blocks.InsertBefore(History.Document.Blocks.FirstBlock, paragraph);

            NotifyPropertyChanged("Cipher");
        }

        public void Decrypt()
        {
            if (!ValidateCryptosystem())
            {
                return;
            }

            UiServices.SetBusyState();
            Paragraph paragraph = new Paragraph();
            try
            {
                Message = GGH.Decrypt(cipher);
                paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + Languages.buttonDecrypt + " **\r\n"))));
                paragraph.Inlines.Add(new Bold(new Run(Languages.labelCiphertext)));
                paragraph.Inlines.Add(" " + Cipher + "\r\n");
                paragraph.Inlines.Add(new Bold(new Run(Languages.labelPlainText)));
                paragraph.Inlines.Add(" " + Message + "\r\n");
                NotifyPropertyChanged("Message");
            }
            catch (Exception ex)
            {

                paragraph.Inlines.Add(new Bold(new Run(Languages.labelAbort)));
                paragraph.Inlines.Add(" " + ex.Message + "\r\n");

                MessageBox.Show(string.Format(Languages.errorDecryptionError, ex.Message), Languages.error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (History.Document.Blocks.FirstBlock != null)
                {
                    History.Document.Blocks.InsertBefore(History.Document.Blocks.FirstBlock, paragraph);
                }
                else
                {
                    History.Document.Blocks.Add(paragraph);
                }
            }
        }

        public bool CheckCipherFormat()
        {
            try
            {
                cipher = Util.ConvertStringToLatticeND(Cipher).Vectors[0];
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void UpdateTextBoxes()
        {
            if (LeftGrid.RowDefinitions.Count != Lattice.N)
            {
                LeftGrid.RowDefinitions.Clear();
                LeftGrid.ColumnDefinitions.Clear();
                RightGrid.RowDefinitions.Clear();
                RightGrid.ColumnDefinitions.Clear();

                for (int i = 0; i < Lattice.N; i++)
                {
                    LeftGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35) });
                    LeftGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    RightGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35) });
                    RightGrid.ColumnDefinitions.Add(new ColumnDefinition());
                }

                LeftGrid.Children.Clear();
                RightGrid.Children.Clear();

                for (int i = 0; i < Lattice.N; i++)
                {
                    for (int j = 0; j < Lattice.M; j++)
                    {
                        TextBlock leftTextBlock = new SelectableTextBlock
                        {
                            Text = Util.FormatBigInt(Lattice.Vectors[i].values[j]),
                            Margin = new Thickness(10, 0, 10, 0),
                            TextAlignment = TextAlignment.Right
                        };
                        Grid.SetColumn(leftTextBlock, i);
                        Grid.SetRow(leftTextBlock, j);
                        LeftGrid.Children.Add(leftTextBlock);

                        TextBlock rightTextBlock = new SelectableTextBlock
                        {
                            Text = Util.FormatBigInt(Lattice.ReducedVectors[i].values[j]),
                            Margin = new Thickness(10, 0, 10, 0),
                            TextAlignment = TextAlignment.Right
                        };
                        Grid.SetColumn(rightTextBlock, i);
                        Grid.SetRow(rightTextBlock, j);
                        RightGrid.Children.Add(rightTextBlock);
                    }
                }
            }
            else
            {
                foreach (TextBlock textBlock in LeftGrid.Children)
                {
                    textBlock.Text = Util.FormatBigInt(Lattice.Vectors[Grid.GetColumn(textBlock)].values[Grid.GetRow(textBlock)]);
                }

                foreach (TextBlock textBlock in RightGrid.Children)
                {
                    textBlock.Text = Util.FormatBigInt(Lattice.ReducedVectors[Grid.GetColumn(textBlock)].values[Grid.GetRow(textBlock)]);
                }
            }
        }
    }
}
