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
    public class LWEViewModel : BaseViewModel
    {
        public RichTextBox History { get; set; }
        public LWEModel LWE { get; set; }
        public Grid GridS { get; set; }
        public Grid GridA { get; set; }
        public Grid GridB { get; set; }
        private MatrixND cipher;
        public string Message { get; set; }
        public string Cipher { get; set; }

        public void GenerateNewLWE(int dim, int q)
        {
            UiServices.SetBusyState();

            LWE = new LWEModel(dim, 1, q, true);
            LWE.GenerateNewRandomVector();

            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + Languages.buttonGenerateNewCryptosystem + " **\r\n"))));
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelPrivateKeyS + ":")));
            paragraph.Inlines.Add(" " + LWE.S + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelPublicKeyA + ":")));
            paragraph.Inlines.Add(" " + LWE.A + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelAlpha)));
            paragraph.Inlines.Add(" " + Util.FormatDoubleLog(LWE.alpha) + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelPublicKeyB2 + ":")));
            paragraph.Inlines.Add(" " + LWE.B + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelModuloQ)));
            paragraph.Inlines.Add(" " + LWE.q + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelRandomVectorR)));
            paragraph.Inlines.Add(" " + MatrixND.Transpose(LWE.r) + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelSubsetU)));
            paragraph.Inlines.Add(" " + MatrixND.Transpose(LWE.u) + "\r\n");

            if (History.Document.Blocks.FirstBlock != null)
            {
                History.Document.Blocks.InsertBefore(History.Document.Blocks.FirstBlock, paragraph);
            }
            else
            {
                History.Document.Blocks.Add(paragraph);
            }

            NotifyPropertyChanged("RandomVectorR");
            NotifyPropertyChanged("SubsetU");
        }

        private RelayCommand generateRandomVectorCommand;
        public RelayCommand GenerateRandomVectorCommand
        {
            get
            {
                if (generateRandomVectorCommand != null)
                {
                    return generateRandomVectorCommand;
                }

                generateRandomVectorCommand = new RelayCommand(
                    parameter1 =>
                    {
                        UiServices.SetBusyState();
                        LWE.GenerateNewRandomVector();

                        Paragraph paragraph = new Paragraph();
                        paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + Languages.buttonGenerateNewRandomVector + " **\r\n"))));
                        paragraph.Inlines.Add(new Bold(new Run(Languages.labelRandomVectorR)));
                        paragraph.Inlines.Add(" " + MatrixND.Transpose(LWE.r) + "\r\n");
                        paragraph.Inlines.Add(new Bold(new Run(Languages.labelSubsetU)));
                        paragraph.Inlines.Add(" " + MatrixND.Transpose(LWE.u) + "\r\n");

                        if (History.Document.Blocks.FirstBlock != null)
                        {
                            History.Document.Blocks.InsertBefore(History.Document.Blocks.FirstBlock, paragraph);
                        }
                        else
                        {
                            History.Document.Blocks.Add(paragraph);
                        }

                        NotifyPropertyChanged("RandomVectorR");
                        NotifyPropertyChanged("SubsetU");
                    });
                return generateRandomVectorCommand;
            }
        }

        public void Encrypt()
        {
            UiServices.SetBusyState();
            MatrixND mesMat = new MatrixND(1, 1);
            mesMat[0, 0] = int.Parse(Message);
            Cipher = LWE.Encrypt(mesMat)[0, 0].ToString();

            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + Languages.buttonEncrypt + " **\r\n"))));
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelPlainText)));
            paragraph.Inlines.Add(" " + Message + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelCiphertext)));
            paragraph.Inlines.Add(" " + Cipher + "\r\n");

            if (History.Document.Blocks.FirstBlock != null)
            {
                History.Document.Blocks.InsertBefore(History.Document.Blocks.FirstBlock, paragraph);
            }
            else
            {
                History.Document.Blocks.Add(paragraph);
            }

            NotifyPropertyChanged("Cipher");
        }

        public void Decrypt()
        {
            UiServices.SetBusyState();
            Paragraph paragraph = new Paragraph();
            try
            {
                Message = LWE.Decrypt(cipher)[0, 0].ToString();
                paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + Languages.buttonDecrypt + " **\r\n"))));
                paragraph.Inlines.Add(new Bold(new Run(Languages.labelCiphertext)));
                paragraph.Inlines.Add(" " + Cipher + "\r\n");
                paragraph.Inlines.Add(new Bold(new Run(Languages.labelPlainText)));
                paragraph.Inlines.Add(" " + Message + "\r\n");
                NotifyPropertyChanged("Message");
            }
            catch (Exception ex)
            {
                paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + Languages.buttonDecrypt + " **\r\n"))));
                paragraph.Inlines.Add(new Bold(new Underline(new Run(Languages.labelAbort))));
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
                cipher = new MatrixND(1, 1);
                cipher[0, 0] = Convert.ToDouble(Cipher);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void UpdateTextBoxes()
        {
            GridS.RowDefinitions.Clear();
            GridS.ColumnDefinitions.Clear();
            GridS.Children.Clear();

            for (int i = 0; i < LWE.n; i++)
            {
                GridS.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35) });
            }

            for (int i = 0; i < LWE.l; i++)
            {
                GridS.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < LWE.n; i++)
            {
                for (int j = 0; j < LWE.l; j++)
                {
                    TextBlock textBlock = new SelectableTextBlock
                    {
                        Text = LWE.S[i, j].ToString(),
                        Margin = new Thickness(10, 0, 10, 0),
                        TextAlignment = TextAlignment.Right
                    };
                    Grid.SetColumn(textBlock, j);
                    Grid.SetRow(textBlock, i);
                    GridS.Children.Add(textBlock);
                }
            }

            GridA.RowDefinitions.Clear();
            GridA.ColumnDefinitions.Clear();
            GridA.Children.Clear();

            for (int i = 0; i < LWE.m; i++)
            {
                GridA.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35) });
            }

            for (int i = 0; i < LWE.n; i++)
            {
                GridA.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < LWE.m; i++)
            {
                for (int j = 0; j < LWE.n; j++)
                {
                    TextBlock textBlock = new SelectableTextBlock
                    {
                        Text = LWE.A[i, j].ToString(),
                        Margin = new Thickness(10, 0, 10, 0),
                        TextAlignment = TextAlignment.Right
                    };
                    Grid.SetColumn(textBlock, j);
                    Grid.SetRow(textBlock, i);
                    GridA.Children.Add(textBlock);
                }
            }

            GridB.RowDefinitions.Clear();
            GridB.ColumnDefinitions.Clear();
            GridB.Children.Clear();

            for (int i = 0; i < LWE.m; i++)
            {
                GridB.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35) });
            }

            for (int i = 0; i < LWE.n; i++)
            {
                GridB.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < LWE.m; i++)
            {
                for (int j = 0; j < LWE.l; j++)
                {
                    TextBlock textBlock = new SelectableTextBlock
                    {
                        Text = LWE.B[i, j].ToString(),
                        Margin = new Thickness(10, 0, 10, 0),
                        TextAlignment = TextAlignment.Right
                    };
                    Grid.SetColumn(textBlock, j);
                    Grid.SetRow(textBlock, i);
                    GridB.Children.Add(textBlock);
                }
            }
        }

        public string RandomVectorR => LWE == null ? "" : MatrixND.Transpose(LWE.r).ToString();
        public string SubsetU => LWE == null ? "" : MatrixND.Transpose(LWE.u).ToString();
    }
}
