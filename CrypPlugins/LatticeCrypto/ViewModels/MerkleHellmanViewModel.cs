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
    public class MerkleHellmanViewModel : BaseViewModel
    {
        public MerkleHellmanModel MerkleHellman { get; set; }
        public RichTextBox History { get; set; }

        public void GenerateNewMerkleHellman(int dim)
        {
            UiServices.SetBusyState();
            MerkleHellman = new MerkleHellmanModel(dim);

            WriteHistoryForNewCryptosystem(Languages.buttonGenerateNewLattice);

            NotifyPropertyChanged("MerkleHellman");
            NotifyPropertyChanged("PrivateKey");
            NotifyPropertyChanged("PublicKey");
            NotifyPropertyChanged("Mod");
            NotifyPropertyChanged("R");
            NotifyPropertyChanged("RI");
        }

        public void SetCryptosystemManually(MerkleHellmanModel newMerkleHellman)
        {
            MerkleHellman = newMerkleHellman;

            WriteHistoryForNewCryptosystem(Languages.buttonDefineNewCryptosystem);

            NotifyPropertyChanged("MerkleHellman");
            NotifyPropertyChanged("PrivateKey");
            NotifyPropertyChanged("PublicKey");
            NotifyPropertyChanged("Mod");
            NotifyPropertyChanged("R");
            NotifyPropertyChanged("RI");
        }

        public void WriteHistoryForNewCryptosystem(string firstLine)
        {
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + firstLine + " **\r\n"))));
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelPrivateKey)));
            paragraph.Inlines.Add(" " + PrivateKey + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelPublicKey)));
            paragraph.Inlines.Add(" " + PublicKey + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelModulus)));
            paragraph.Inlines.Add(" " + Mod + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelMultiplier)));
            paragraph.Inlines.Add(" " + R + "\r\n");
            paragraph.Inlines.Add(new Bold(new Run(Languages.labelMultiplierInverse)));
            paragraph.Inlines.Add(" " + RI + "\r\n");

            if (History.Document.Blocks.FirstBlock != null)
            {
                History.Document.Blocks.InsertBefore(History.Document.Blocks.FirstBlock, paragraph);
            }
            else
            {
                History.Document.Blocks.Add(paragraph);
            }
        }

        public void Encrypt()
        {
            UiServices.SetBusyState();
            Cipher = MerkleHellman.Encrypt(Message).ToString();

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
            Message = MerkleHellman.Decrypt(cipher);

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
            try
            {
                UiServices.SetBusyState();
                Paragraph paragraph = new Paragraph();
                paragraph.Inlines.Add(new Bold(new Underline(new Run("** " + Languages.buttonCryptanalysis + " **\r\n"))));
                paragraph.Inlines.Add(new Bold(new Run(Languages.labelCiphertext)));
                paragraph.Inlines.Add(" " + Cipher + "\r\n");

                Message = MerkleHellman.Cryptanalysis(cipher, paragraph);

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
            catch (Exception)
            {
                MessageBox.Show(Languages.errorNoSolutionFound, Languages.error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            NotifyPropertyChanged("Message");
        }

        public string Message { get; set; }

        private VectorND cipher;
        public string Cipher { get; set; }

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

        public string PrivateKey => MerkleHellman == null ? "" : MerkleHellman.privateKey.ToString();

        public string PublicKey => MerkleHellman == null ? "" : MerkleHellman.publicKey.ToString();

        public BigInteger Mod => MerkleHellman == null ? 0 : MerkleHellman.mod;

        public BigInteger R => MerkleHellman == null ? 0 : MerkleHellman.r;

        public BigInteger RI => MerkleHellman == null ? 0 : MerkleHellman.rI;
    }
}
