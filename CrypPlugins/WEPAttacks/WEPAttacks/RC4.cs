namespace CrypTool.WEPAttacks
{
    public class RC4
    {
        public RC4()
        {
        }

        /// <summary>
        /// Encryptes an inputstream with the help of a 
        /// pseudo-random generation algorithm.
        /// </summary>
        /// <param name="input">Byte-Inputstream</param>
        /// <param name="key">WEP-Key</param>
        /// <returns></returns>
        public static byte[] rc4encrypt(byte[] input, byte[] key)
        {
            byte[] cipher = new byte[input.Length];

            byte[] sbox = new byte[256];
            int i, j = 0;
            byte b, keybyte;
            for (i = 0; i < 256; i++)
            {
                sbox[i] = (byte)i;
            }

            // initialization
            for (i = j = 0; i < 256; i++)
            {
                j = (j + sbox[i] + key[i % key.Length]) % 256;
                // swapping sbox[i] and sbox[j]
                // as you can see, swapping depends on key
                b = sbox[i];
                sbox[i] = sbox[j];
                sbox[j] = b;

            }

            i = 0;
            j = 0;
            for (int c = 0; c < input.Length; c++)
            {
                i = (i + 1) % 256;
                j = (j + sbox[i]) % 256;
                b = sbox[i];
                sbox[i] = sbox[j];
                sbox[j] = b;
                keybyte = sbox[((sbox[i] + sbox[j]) % 256)];
                // XOR
                cipher[c] = (byte)((input[c]) ^ keybyte);
            }
            return cipher;
        }
    }
}
