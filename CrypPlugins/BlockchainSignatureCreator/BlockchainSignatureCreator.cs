/*
   Copyright Eduard Scherf, 2021

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
using CrypTool.PluginBase;
using CrypTool.PluginBase.Miscellaneous;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Controls;

namespace CrypTool.Plugins.BlockchainSignatureCreator
{
    [Author("Eduard Scherf", "eduard-scherf@gmx.de", "CrypTool 2 Team", "https://www.cryptool.org")]
    [PluginInfo("CrypTool.Plugins.BlockchainSignatureCreator.Properties.Resources", "BlockchainSignatureCreatorCaption", "BlockchainSignatureCreatorTooltip", "BlockchainSignatureCreator/userdoc.xml", "BlockchainSignatureCreator/icon.png")]
    [ComponentCategory(ComponentCategory.Protocols)]
    public class Blockchain_Transaction_Signature_Creator : ICrypComponent
    {
        #region Private Variables

        private readonly BlockchainSignatureCreatorSettings _settings = new BlockchainSignatureCreatorSettings();

        private string senderName;
        private string senderN;
        private string senderE;
        private string senderD;

        private string recipientName;
        private string recipientN;
        private string recipientE;
        private string recipientD;

        private int _hashAlgorithmWrapperWidth;
        private HashAlgorithmWrapper _hashAlgorithmWrapper;

        #endregion

        #region Data Properties

        [PropertyInfo(Direction.InputData, "SenderCaption", "SenderTooltip", true)]
        public string Sender
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "RecipientCaption", "RecipientTooltip", true)]
        public string Recipient
        {
            get;
            set;
        }

        [PropertyInfo(Direction.InputData, "AmountCaption", "AmountTooltip", true)]
        public double Amount
        {
            get;
            set;
        }

        [PropertyInfo(Direction.OutputData, "SignatureCaption", "SignatureOutputTooltip")]
        public string Signature
        {
            get;
            set;
        }

        #endregion

        #region IPlugin Members

        public ISettings Settings => _settings;

        public UserControl Presentation => null;

        public void PreExecution()
        {
        }

        public void Execute()
        {
            try
            {
                ProgressChanged(0, 1);

                CreateHashAlgorithmWrapper();

                string sender = Sender;
                string recipient = Recipient;
                ReadInput();
                StringBuilder stringBuilder = new StringBuilder();

                BigInteger sig = CreateSignature(senderName, recipientName, Amount, senderN, senderD);
                stringBuilder.Append(senderName);
                stringBuilder.Append(";");
                stringBuilder.Append(recipientName);
                stringBuilder.Append(";");
                stringBuilder.Append(Amount.ToString(CultureInfo.InvariantCulture));
                stringBuilder.Append(";");
                stringBuilder.Append(sig);
                Signature = stringBuilder.ToString();
                OnPropertyChanged("Signature");

                ProgressChanged(1, 1);
            }
            catch (Exception ex)
            {
                GuiLogMessage(string.Format(Properties.Resources.ExceptionMessage, ex.Message), NotificationLevel.Error);
            }
        }

        public void PostExecution()
        {
        }

        public void Stop()
        {
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
        }

        #endregion

        #region Event Handling

        public event StatusChangedEventHandler OnPluginStatusChanged;

        public event GuiLogNotificationEventHandler OnGuiLogNotificationOccured;

        public event PluginProgressChangedEventHandler OnPluginProgressChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        private void GuiLogMessage(string message, NotificationLevel logLevel)
        {
            EventsHelper.GuiLogMessage(OnGuiLogNotificationOccured, this, new GuiLogEventArgs(message, this, logLevel));
        }

        private void OnPropertyChanged(string name)
        {
            EventsHelper.PropertyChanged(PropertyChanged, this, new PropertyChangedEventArgs(name));
        }

        private void ProgressChanged(double value, double max)
        {
            EventsHelper.ProgressChanged(OnPluginProgressChanged, this, new PluginProgressEventArgs(value, max));
        }

        #endregion

        #region Helper methods

        public void ReadInput()
        {
            string sender = Sender;
            string recipient = Recipient;

            StringReader sr1 = new StringReader(sender);
            StringReader sr2 = new StringReader(recipient);
            string line1;
            string line2;

            while ((line1 = sr1.ReadLine()) != null)
            {
                if (!line1.StartsWith("#"))
                {
                    string[] data1 = line1.Split(';');
                    senderName = data1[0];
                    senderN = data1[1];
                    senderE = data1[2];
                    senderD = data1[3];
                }
            }

            while ((line2 = sr2.ReadLine()) != null)
            {
                if (!line2.StartsWith("#"))
                {
                    string[] data2 = line2.Split(';');
                    recipientName = data2[0];
                    recipientN = data2[1];
                    recipientE = data2[2];
                    recipientD = data2[3];
                }
            }
        }

        public BigInteger CreateSignature(string real_from, string real_to, double real_amount, string real_N, string real_d)
        {
            string from = real_from;
            string to = real_to;
            BigInteger N = BigInteger.Parse(real_N);
            BigInteger d = BigInteger.Parse(real_d);

            byte[] preImage = Encoding.UTF8.GetBytes(from + to + real_amount.ToString(CultureInfo.InvariantCulture));
            byte[] hash = _hashAlgorithmWrapper.ComputeHash(preImage);
            BigInteger hashBigInt = new BigInteger(hash);

            if (hashBigInt < BigInteger.Zero)
            {
                hashBigInt = hashBigInt * BigInteger.MinusOne;
            }
            hashBigInt = BigInteger.ModPow(hashBigInt, BigInteger.One, N);
            return BigInteger.ModPow(hashBigInt, d, N);
        }

        private void CreateHashAlgorithmWrapper()
        {
            _hashAlgorithmWrapperWidth = _settings.HashAlgorithmWidth;
            if (_hashAlgorithmWrapperWidth <= 0)
            {
                _hashAlgorithmWrapperWidth = 1;
                GuiLogMessage(Properties.Resources.HashWidthErrorCaption, NotificationLevel.Warning);
            }
            switch (_settings.HashAlgorithm)
            {
                default:
                case HashAlgorithms.SHA1:
                    _hashAlgorithmWrapper = new HashAlgorithmWrapper(new SHA1Managed(), _hashAlgorithmWrapperWidth);
                    break;

                case HashAlgorithms.SHA256:
                    _hashAlgorithmWrapper = new HashAlgorithmWrapper(new SHA256Managed(), _hashAlgorithmWrapperWidth);
                    break;

                case HashAlgorithms.SHA512:
                    _hashAlgorithmWrapper = new HashAlgorithmWrapper(new SHA512Managed(), _hashAlgorithmWrapperWidth);
                    break;
            }
        }
        #endregion
    }

    /// <summary>
    /// The hash algorithm wrapper wraps our hash function and allows to reduce the length
    /// of the hash value based on the user's choice
    /// </summary>
    public class HashAlgorithmWrapper
    {
        private readonly HashAlgorithm _hashAlgorithm;
        private readonly int _hashAlgorithmWidth;

        public HashAlgorithmWrapper(HashAlgorithm hashAlgorithm, int hashAlgorithmWidth)
        {
            _hashAlgorithm = hashAlgorithm;
            _hashAlgorithmWidth = hashAlgorithmWidth;
        }

        public byte[] ComputeHash(byte[] data)
        {
            byte[] hash = _hashAlgorithm.ComputeHash(data);
            if (hash.Length != _hashAlgorithmWidth)
            {
                //reduce length of hashvalue based on settings
                byte[] reduced_hash = new byte[_hashAlgorithmWidth];
                Array.Copy(hash, 0, reduced_hash, 0, _hashAlgorithmWidth);
                return reduced_hash;
            }
            return hash;
        }
    }
}
