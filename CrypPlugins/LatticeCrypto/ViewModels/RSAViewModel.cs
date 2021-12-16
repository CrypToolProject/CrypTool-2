using LatticeCrypto.Models;
using LatticeCrypto.Properties;
using LatticeCrypto.Utilities;
using System;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace LatticeCrypto.ViewModels
{
    public class RSAViewModel : BaseViewModel
    {
        public RSAModel RSAModel { get; set; }
        public RichTextBox History { get; set; }
        private string knownMessage;
        private string unknownMessageResult;
        public int unknownStart;
        public int unknownLength;
        private BigInteger cipher;
        public string Cipher { get; set; }

        public void GenerateNewRSA(int bitSize)
        {
            UiServices.SetBusyState();
            try
            {
                RSAModel = new RSAModel(bitSize);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Languages.error, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + Languages.buttonGenerateNewCryptosystem + " **\r\n"))));
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelPrimeP)));
            paragraph.Inlines.Add(" " + PrimP + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelPrimeQ)));
            paragraph.Inlines.Add(" " + PrimQ + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelModulus)));
            paragraph.Inlines.Add(" " + ModulusN + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelPrivateExponentD)));
            paragraph.Inlines.Add(" " + ExpD + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelPublicExponentE)));
            paragraph.Inlines.Add(" " + ExpE + "\r\n");

            if (History.Document.Blocks.FirstBlock != null)
            {
                History.Document.Blocks.InsertBefore(History.Document.Blocks.FirstBlock, paragraph);
            }
            else
            {
                History.Document.Blocks.Add(paragraph);
            }

            NotifyPropertyChanged("PrimP");
            NotifyPropertyChanged("PrimQ");
            NotifyPropertyChanged("ModulusN");
            NotifyPropertyChanged("ExpD");
            NotifyPropertyChanged("ExpE");
            NotifyPropertyChanged("ValidationInfo");
        }

        public void Encrypt()
        {
            UiServices.SetBusyState();
            Cipher = RSAModel.Encrypt(message).ToString();

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
            Message = RSAModel.Decrypt(cipher);

            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + Languages.buttonDecrypt + " **\r\n"))));
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelCiphertext)));
            paragraph.Inlines.Add(" " + Cipher + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelPlainText)));
            paragraph.Inlines.Add(" " + Message + "\r\n");

            if (History.Document.Blocks.FirstBlock != null)
            {
                History.Document.Blocks.InsertBefore(History.Document.Blocks.FirstBlock, paragraph);
            }
            else
            {
                History.Document.Blocks.Add(paragraph);
            }

            NotifyPropertyChanged("Message");
        }

        public void Cryptanalysis()
        {
            UiServices.SetBusyState();
            Paragraph paragraph = new Paragraph();

            try
            {
                paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + Languages.buttonCryptanalysis + " **\r\n"))));
                paragraph.Inlines.Add(new Bold(new Run(Languages.labelCiphertext)));
                paragraph.Inlines.Add(" " + Cipher + "\r\n");
                paragraph.Inlines.Add(new Bold(new Run(Languages.labelKnownPlainText)));
                paragraph.Inlines.Add(" " + KnownMessage + "\r\n");

                string left = message.Substring(0, unknownStart);
                string right = message.Substring(unknownStart + unknownLength);
                UnknownMessageResult = RSAModel.StereotypedAttack(left, right, unknownLength, cipher, "4");

                paragraph.Inlines.Add(new Bold(new Run(Languages.labelResultUnknownPlainText)));
                paragraph.Inlines.Add(" " + UnknownMessageResult + "\r\n");
            }
            catch (Exception ex)
            {
                UnknownMessageResult = "";

                paragraph.Inlines.Add(new Bold(new Run(Languages.labelAbort)));
                paragraph.Inlines.Add(" " + ex.Message + "\r\n");

                MessageBox.Show(ex.Message, Languages.error, MessageBoxButton.OK, MessageBoxImage.Error);
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

        //public void SetKnownMessageRange (int start, int length)
        //{
        //    String newKnownMessage = "";
        //    for (int i = 0; i < start; i++)
        //        newKnownMessage += "*";
        //    newKnownMessage += message.Substring(start, length);
        //    for (int i = 0; i < message.Length - start - length; i++)
        //        newKnownMessage += "*";

        //    KnownMessage = newKnownMessage;
        //}

        public void SetUnknownMessageRange(int start, int length)
        {
            unknownStart = start;
            unknownLength = length;
            string newKnownMessage = "";
            if (start > 0)
            {
                newKnownMessage += message.Substring(0, start);
            }

            for (int i = 0; i < length; i++)
            {
                newKnownMessage += "*";
            }

            if (start + length < message.Length)
            {
                newKnownMessage += message.Substring(start + length);
            }

            KnownMessage = newKnownMessage;
            UnknownMessageResult = "";
            NotifyPropertyChanged("ValidationInfo");
        }

        private string message;
        public string Message
        {
            get => message;
            set
            {
                message = value;
                KnownMessage = "";
                UnknownMessageResult = "";
                unknownStart = 0;
                unknownLength = 0;
                NotifyPropertyChanged("ValidationInfo");
            }
        }

        public string KnownMessage
        {
            get => !string.IsNullOrEmpty(knownMessage) ? knownMessage : "";
            set
            {
                if (knownMessage == value)
                {
                    return;
                }

                knownMessage = value;
                NotifyPropertyChanged("KnownMessage");
            }
        }

        public string UnknownMessageResult
        {
            get => unknownMessageResult;
            set
            {
                if (unknownMessageResult == value)
                {
                    return;
                }

                unknownMessageResult = value;
                NotifyPropertyChanged("UnknownMessageResult");
            }
        }

        public bool CheckCipherFormat()
        {
            return BigInteger.TryParse(Cipher, out cipher);
        }

        public string ValidationInfo
        {
            get
            {
                if (!string.IsNullOrEmpty(message) && message.Length >= BlockSize)
                {
                    return Languages.errorMessageTooLong;
                }

                if (!string.IsNullOrEmpty(message) && string.IsNullOrEmpty(knownMessage))
                {
                    return Languages.infoMarkKnownPlaintext;
                }

                return "";
            }
        }

        public string PrimP => RSAModel == null ? "" : RSAModel.GetPrimPToString();

        public string PrimQ => RSAModel == null ? "" : RSAModel.GetPrimQToString();

        public string ModulusN => RSAModel == null ? "" : RSAModel.GetModulusNToString();

        public string ExpD => RSAModel == null ? "" : RSAModel.GetPrivateExponentToString();

        public string ExpE => RSAModel == null ? "" : RSAModel.GetPublicExponentToString();

        public int BlockSize => RSAModel == null ? 0 : RSAModel.GetBlockSize();
    }
}
