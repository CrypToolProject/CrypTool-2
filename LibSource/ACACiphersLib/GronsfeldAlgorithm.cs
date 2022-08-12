using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrypTool.ACACiphersLib
{
    internal class GronsfeldAlgorithm : Cipher
    {
        public override int[] Decrypt(int[] ciphertext, int[] key)
        {
            List<int> result = new List<int>();

            for(int i = 0; i < ciphertext.Length; i++)
            {
                result.Add(Tools.mod(ciphertext[i] + key[Tools.mod(i,key.Length)],LATIN_ALPHABET.Length));
            }

            return result.ToArray();
        }

        public override int[] Encrypt(int[] plaintext, int[] key)
        {
            List<int> result = new List<int>();

            for (int i = 0; i < plaintext.Length; i++)
            {
                result.Add(Tools.mod(plaintext[i] - key[Tools.mod(i, key.Length)], LATIN_ALPHABET.Length));
            }

            return result.ToArray();
        }
    }
}
