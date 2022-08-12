using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrypTool.ACACiphersLib
{
    internal class VariantAlgorithm : Cipher
    {
        public override int[] Decrypt(int[] ciphertext, int[] key)
        {
            List<int> result = new List<int>();

            for (int i = 0; i < ciphertext.Length; i++)
            {
                int a = ciphertext[i] - LATIN_ALPHABET.Length + key[i % key.Length];
                int b = LATIN_ALPHABET.Length;
                result.Add(Tools.mod(a,b));
            }


            return result.ToArray();
        }

        public override int[] Encrypt(int[] plaintext, int[] key)
        {
            List<int> result = new List<int>();

            for(int i =0; i<plaintext.Length; i++)
            {
                result.Add((plaintext[i] + LATIN_ALPHABET.Length - key[i % key.Length])% LATIN_ALPHABET.Length);
            }

            return result.ToArray();
        }
    }
}
