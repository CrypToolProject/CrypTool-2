//////////////////////////////////////////////////////////////////////////////////////////////////
// CrypTool V2
// © 2008 - Gerhard Junker
// Apache License see http://www.apache.org/licenses/
//
// $HeadURL: https://svn.CrypTool.org/CrypTool2/trunk/LibSource/SSCext/SSCpkcs5.cs $
//////////////////////////////////////////////////////////////////////////////////////////////////
// $Revision:: 8109                                                                           $://
// $Author:: kopal                                                                            $://
// $Date:: 2019-05-13 12:33:48 +0200 (Mo, 13 Mai 2019)                                        $://
//////////////////////////////////////////////////////////////////////////////////////////////////

using System.Runtime.InteropServices;

#if DEBUG

#endif

namespace System.Security.Cryptography
{
    /// <summary>
    /// insert PKSC #5 v2 into default namespace
    /// </summary>
    [ComVisibleAttribute(true)]
    public class PKCS5MaskGenerationMethod : System.Security.Cryptography.MaskGenerationMethod
    {

        /// <summary>
        /// implemented hash functions
        /// </summary>
        public enum ShaFunction
        {
            MD5,
            SHA1,
            SHA256,
            SHA384,
            SHA512,
            TIGER,
            WHIRLPOOL
        };

        private ShaFunction selectedShaFunction = ShaFunction.SHA256;

        /// <summary>
        /// Gets or sets the selected sha function.
        /// </summary>
        /// <value>The selected sha function.</value>
        public ShaFunction SelectedShaFunction
        {
            get => selectedShaFunction;
            set => selectedShaFunction = value;
        }


        /// <summary>
        /// Gets the length of the hash in Bytes.
        /// </summary>
        /// <param name="SelectedShaFunction">The selected sha function.</param>
        /// <returns></returns>
        public static int GetHashLength(ShaFunction SelectedShaFunction)
        {
            int[] length =
            {
        16, // MD5
        20, // SHA1
        32, // SHA256
        48, // SHA384
        64, // SHA512
        24, // TIGER
        64  // WHIRLPOOL
      };

            return length[(int)SelectedShaFunction];
        }

        /// <summary>
        /// Gets the sha hash.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="salt">The salt.</param>
        /// <returns></returns>
        private byte[] GetShaHash(byte[] key, byte[] salt)
        {
            HMAC ha;

            switch (selectedShaFunction)
            {
                case ShaFunction.MD5:
                    ha = new System.Security.Cryptography.HMACMD5();
                    break;

                case ShaFunction.SHA1:
                    ha = new System.Security.Cryptography.HMACSHA1();
                    break;

                case ShaFunction.SHA256:
                    ha = new System.Security.Cryptography.HMACSHA256();
                    break;

                case ShaFunction.SHA384:
                    ha = new System.Security.Cryptography.HMACSHA384();
                    break;

                case ShaFunction.SHA512:
                    ha = new System.Security.Cryptography.HMACSHA512();
                    break;

                case ShaFunction.TIGER:
                    ha = new System.Security.Cryptography.HMACTIGER2();
                    break;

                case ShaFunction.WHIRLPOOL:
                    ha = new System.Security.Cryptography.HMACWHIRLPOOL();
                    break;

                default:
                    throw new ArgumentOutOfRangeException("SelectedShaFunction");
            }

            ha.Key = key;
            byte[] h = ha.ComputeHash(salt);
            ha.Clear();

            return h;
        }

        /// <summary>
        /// Gens the key block.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="salt">The salt.</param>
        /// <param name="count">The count.</param>
        /// <param name="blockIndex">Index of the block.</param>
        /// <returns></returns>
        private byte[] GenKeyBlock(byte[] password, byte[] salt, int count, int blockIndex)
        {
            int len = salt.Length;
            byte[] data = new byte[len + 4];

            for (int i = 0; i < len; i++)
            {
                data[i] = salt[i];
            }

            data[len] = data[len + 1] = data[len + 2] = 0;
            data[len + 3] = (byte)blockIndex;

            byte[] u1 = GetShaHash(password, data);

            byte[] result = new byte[u1.Length];
            for (int i = 0; i < u1.Length; i++)
            {
                result[i] = u1[i];
            }

            for (int c = 1; c < count; c++)
            {
                byte[] u2 = GetShaHash(password, u1);

                for (int i = 0; i < result.Length; i++)
                {
                    result[i] ^= u2[i];
                    u1[i] = u2[i];
                }
            }

            return result;
        }

        /// <summary>
        /// When overridden in a derived class, 
        /// generates a mask with the specified length using the specified random salt.
        /// </summary>
        /// <param name="rgbSalt">The random salt to use to compute the mask.</param>
        /// <param name="cbReturn">The length of the generated mask in bytes.</param>
        /// <returns>
        /// A randomly generated mask whose length is equal to the <paramref name="cbReturn"/> parameter.
        /// </returns>
        [ComVisibleAttribute(true)]
        public override byte[] GenerateMask
            (
                byte[] rgbSalt,
                int cbReturn
            )
        {
            // member implemented for compatibility only ....
            // throw new System.NotImplementedException("GenerateMask needs more parameters");

            // Computes masks according to PKCS #1 for use by key exchange algorithms. 
            // Generates and returns a mask from the specified random salt of the specified length. 
            PKCS1MaskGenerationMethod pkcs1 = new PKCS1MaskGenerationMethod();
            return pkcs1.GenerateMask(rgbSalt, cbReturn);
        }

        /// <summary>
        /// When overridden in a derived class, 
        /// generates a mask with the specified length using the specified password and random salt.
        /// Implementing PBKDF2 of PKCS #5 v2.1 Password-Basd Cryptography Standard
        /// </summary>
        /// <param name="password">The password to use to compute the mask.</param>
        /// <param name="rgbSalt">The random salt (seed) to use to compute the mask.</param>
        /// <param name="count">The iteration count, a positive integer</param>
        /// <param name="cbReturn">The length of the generated mask in bytes.</param>
        /// <returns>
        /// A randomly generated mask whose length is equal to the <paramref name="cbReturn"/> parameter.
        /// </returns>
        [ComVisibleAttribute(true)]
        public byte[] GenerateMask
            (
                byte[] password,
                byte[] rgbSalt,
                int count,
                int cbReturn
            )
        {
            if (cbReturn <= 0)
            {
                cbReturn = 1;
            }
            //throw new ArgumentOutOfRangeException("cbReturn", "cbReturn must be positive.");

            byte[] key = new byte[cbReturn];
            for (int i = 0; i < cbReturn; i++)
            {
                key[i] = 0;
            }

            if (count <= 0)
            {
                count = 1;
            }

            int hLen = GetHashLength(SelectedShaFunction);

            // Let blockCount be the number of hLen-bytes blocks in the derived key, rounding up,
            // let fillBytes be the number of bytes in the last block.
            int blockCount = cbReturn / hLen + 1;
            int fillBytes = cbReturn - (blockCount - 1) * hLen;

            if (blockCount > 255)
            {
                string msg = string.Format("cbReturn must be lesser than {0} by implementation limits.", hLen * 255);
                throw new ArgumentOutOfRangeException("cbReturn", "msg");
            }

            int outPos = 0;

            for (int blockIndex = 0; blockIndex < blockCount - 1; blockIndex++)
            {
                byte[] block = GenKeyBlock(password, rgbSalt, count, blockIndex + 1);
                for (int i = 0; i < hLen; i++)
                {
                    key[outPos++] = block[i];
                }
            }

            // last block
            if (fillBytes > 0)
            {
                byte[] block = GenKeyBlock(password, rgbSalt, count, blockCount);
                for (int i = 0; i < fillBytes; i++)
                {
                    key[outPos++] = block[i];
                }
            }

            return key;
        } // PBKDF2
    } // class
}

