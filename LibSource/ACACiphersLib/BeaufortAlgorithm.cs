using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrypTool.ACACiphersLib
{
    internal class BeaufortAlgorithm : Cipher
    {
        public override int[] Decrypt(int[] ciphertext, int[] key)
        {
            return EncDec(ciphertext, key);
        }

        public override int[] Encrypt(int[] plaintext, int[] key)
        {
            return EncDec(plaintext, key);
        }

        public int[] EncDec(int[] text, int[] key)
        {
            List<int> result = new List<int>();

            for (int i = 0;i<text.Length;i++)
            {
                int character = text[i];
                int keychar = key[Tools.mod(i,key.Length)];
                result.Add(Tools.mod(keychar-character, LATIN_ALPHABET.Length));
            }

            return result.ToArray();
        }
    }
}
