using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace KeySearcher.P2P.Storage
{
    class StorageKeyGenerator
    {
        private readonly KeySearcher keySearcher;
        private readonly KeySearcherSettings settings;

        public StorageKeyGenerator(KeySearcher keySearcher, KeySearcherSettings settings)
        {
            this.keySearcher = keySearcher;
            this.settings = settings;
        }

        public virtual String Generate()
        {
            var bytesToUse = keySearcher.CostMaster.GetBytesToUse();
            var bytesOffset = keySearcher.CostMaster.GetBytesOffset();

            // Add simple data
            var rawIdentifier = "P2PJOB";
            rawIdentifier += settings.NumberOfBlocks + settings.Key;
            rawIdentifier += keySearcher.ControlMaster.GetType();
            rawIdentifier += keySearcher.CostMaster.GetType();
            rawIdentifier += bytesToUse;
            rawIdentifier += keySearcher.CostMaster.GetRelationOperator();

            // wander 2011-04-11: use bytesOffset if set
            // bytesOffset will be used to modify/optimize input data and IV, but below the non-optimized
            // ones are being used
            if (bytesOffset > 0)
            {
                rawIdentifier += bytesOffset;
            }

            // Add initialization vector when available (non-optimized one)
            if (keySearcher.InitVector != null)
            {
                rawIdentifier += Encoding.ASCII.GetString(keySearcher.InitVector);
            }

            // Add input data with the amount of used bytes (non-optimized/non-shortened input data)
            var inputData = keySearcher.EncryptedData;
            if (inputData.Length > bytesToUse)
                Array.Copy(inputData, inputData, bytesToUse);

            rawIdentifier += Encoding.ASCII.GetString(inputData);
            
            // Add cost of input data to preserve cost master settings
            rawIdentifier += keySearcher.CostMaster.CalculateCost(inputData).ToString("N2", CultureInfo.InvariantCulture);

            /*
            // Add decrypted input data to preserve encryption settings
            var keyLength = keySearcher.Pattern.giveInputPattern().Length / 3;
            var decryptedData = keySearcher.ControlMaster.Decrypt(inputData, new byte[keyLength], new byte[8]);
            rawIdentifier += Encoding.ASCII.GetString(decryptedData);
            */

            var hashAlgorithm = new SHA1CryptoServiceProvider();
            var hash = hashAlgorithm.ComputeHash(Encoding.ASCII.GetBytes(rawIdentifier));
            return BitConverter.ToString(hash).Replace("-", "");
        }

        public string GenerateStatusKey()
        {
            return Generate() + "-status";
        }
    }
}
