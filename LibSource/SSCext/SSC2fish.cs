//////////////////////////////////////////////////////////////////////////////////////////////////
// CrypTool V2
// © 2008 - Gerhard Junker
// Apache License see http://www.apache.org/licenses/
//
// $HeadURL: https://svn.CrypTool.org/CrypTool2/trunk/LibSource/SSCext/SSC2fish.cs $
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
    /// 
    /// </summary>
    [ComVisibleAttribute(true)]
    public partial class TwofishManaged : SymmetricAlgorithm
    {
        private const int BLOCK_SIZE = 128;
        private const int ROUNDS = 16;
        private const int MAX_KEY_BITS = 256;
        private const int MIN_KEY_BITS = 128;

        private int blockSize = BLOCK_SIZE;
        private int keySize = MIN_KEY_BITS;

        private byte[] key = null;
        private byte[] iv = null;

        private readonly CipherMode cipherMode = CipherMode.CBC;
        private PaddingMode paddingMode = PaddingMode.Zeros;


        /// <summary>
        /// Initializes a new instance of the <see cref="TwofishManaged"/> class.
        /// </summary>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">
        /// The implementation of the class derived from the symmetric algorithm is not valid.
        /// </exception>
        public TwofishManaged()
        {
            key = new byte[KeySize / 8]; // zeroed by default
            iv = new byte[BlockSize / 8]; // zeroed by default
            ModeValue = cipherMode;
        }

        public static new TwofishManaged Create()
        {
            TwofishManaged fm = new TwofishManaged();
            fm.GenerateKey();
            fm.GenerateIV();
            return fm;
        }

        /// <summary>
        /// Creates a symmetric decryptor object with the specified 
        /// <see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Key"/> property and 
        /// initialization vector (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.IV"/>).
        /// </summary>
        /// <param name="rgbKey">The secret key to use for the symmetric algorithm.</param>
        /// <param name="rgbIV">The initialization vector to use for the symmetric algorithm.</param>
        /// <returns>A symmetric decryptor object.</returns>
        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
        {
            key = rgbKey;

            Array.Clear(iv, 0, iv.Length);

            if (null != rgbIV)
            {
                for (int i = 0; i < iv.Length && i < rgbIV.Length; i++)
                {
                    iv[i] = rgbIV[i];
                }
            }

            int kl = rgbKey.Length * 8;
            if (ValidKeySize(kl))
            {
                keySize = kl;
            }

            return new TwofishEncryption(keySize, ref key, ref iv, Mode,
        TwofishManaged.EncryptionDirection.Decrypting);
        }

        /// <summary>
        /// Creates a symmetric encryptor object with the specified 
        /// <see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Key"/> property and 
        /// initialization vector (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.IV"/>).
        /// </summary>
        /// <param name="rgbKey">The secret key to use for the symmetric algorithm.</param>
        /// <param name="rgbIV">The initialization vector to use for the symmetric algorithm.</param>
        /// <returns>A symmetric encryptor object.</returns>
        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
        {
            key = rgbKey; // this appears to make a new copy

            Array.Clear(iv, 0, iv.Length);

            if (null != rgbIV)
            {
                for (int i = 0; i < iv.Length && i < rgbIV.Length; i++)
                {
                    iv[i] = rgbIV[i];
                }
            }

            int kl = rgbKey.Length * 8;
            if (ValidKeySize(kl))
            {
                keySize = kl;
            }

            return new TwofishEncryption(keySize, ref key, ref iv, Mode,
        TwofishManaged.EncryptionDirection.Encrypting);
        }

        /// <summary>
        /// Generates a random initialization vector 
        /// (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.IV"/>) to use for the algorithm.
        /// </summary>
        public override void GenerateIV()
        {
            if ((iv == null) || (iv.Length == 0))
            {
                iv = new byte[16]; // zeroed by default
            }
            else
            {
                Array.Clear(iv, 0, iv.Length);
            }
        }

        /// <summary>
        /// Generates a random key 
        /// (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.Key"/>) to use for the algorithm.
        /// </summary>
        public override void GenerateKey()
        {
            if ((key == null) || (key.Length == 0))
            {
                key = new byte[keySize / 8]; // zeroed by default
            }
            else
            {
                Array.Clear(key, 0, Key.Length);
            }
        }


        /// <summary>
        /// Gets or sets the cipherMode for operation of the symmetric algorithm.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The cipherMode for operation of the symmetric algorithm. 
        /// The default is <see cref="F:System.Security.Cryptography.CipherMode.CBC"/>.
        /// </returns>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">
        /// The cipher cipherMode is not one of the <see cref="T:System.Security.Cryptography.CipherMode"/> values.
        /// </exception>
        public override CipherMode Mode
        {
            set
            {
                switch (value)
                {
                    case CipherMode.CBC:
                    case CipherMode.ECB:
                        break;

                    default:
                        throw new CryptographicException("CipherMode is not supported.");
                }
                ModeValue = value;
            }
        }


        /// <summary>
        /// Gets or sets the block size, in bits, of the cryptographic operation.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The block size, in bits.
        /// </returns>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">
        /// The block size is invalid.
        /// </exception>
        public override int BlockSize
        {
            get => blockSize;
            set
            {
                blockSize = value;
                base.BlockSize = blockSize;
            }
        }


        /// <summary>
        /// Gets or sets the keysize.
        /// </summary>
        /// <value>The keysize.</value>
        public int Keysize
        {
            get => keySize;
            set
            {
                if (ValidKeySize(value))
                {
                    keySize = value;
                }
            }
        }


        /// <summary>
        /// Valids the size of the key.
        /// </summary>
        /// <param name="keysize">The keysize.</param>
        /// <returns></returns>
        public new bool ValidKeySize(int keysize)
        {
            switch (keysize)
            {
                case 128:
                case 192:
                case 256:
                    return true;
                default:
                    return false;
            }
        }
        /// <summary>
        /// Gets or sets the secret key for the symmetric algorithm.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The secret key to use for the symmetric algorithm.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// An attempt was made to set the key to null.
        /// </exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">
        /// The key size is invalid.
        /// </exception>
        public override byte[] Key
        {
            set => key = value;
            get => key;
        }


        /// <summary>
        /// Gets or sets the initialization vector 
        /// (<see cref="P:System.Security.Cryptography.SymmetricAlgorithm.IV"/>) for the symmetric algorithm.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The initialization vector.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">
        /// An attempt was made to set the initialization vector to null.
        /// </exception>
        /// <exception cref="T:System.Security.Cryptography.CryptographicException">
        /// An attempt was made to set the initialization vector to an invalid size.
        /// </exception>
        public override byte[] IV
        {
            set
            {
                Array.Clear(iv, 0, iv.Length);
                for (int i = 0; i < iv.Length && i < value.Length; i++)
                {
                    iv[i] = value[i];
                }
            }
            get => iv;
        }


        /// <summary>
        /// Gets or sets the padding cipherMode.
        /// </summary>
        /// <value>The padding cipherMode.</value>
        public PaddingMode PaddingMode
        {
            get => paddingMode;
            set => paddingMode = value;
        }


        /// <summary>
        /// Gets the key sizes, in bits, that are supported by the symmetric algorithm.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// An array that contains the key sizes supported by the algorithm.
        /// </returns>
        public override KeySizes[] LegalKeySizes
        {
            get
            {
                KeySizes[] ks = new KeySizes[1];
                ks[0] = new KeySizes(128, 256, 64);
                return ks;
            }
        }
    }
}
