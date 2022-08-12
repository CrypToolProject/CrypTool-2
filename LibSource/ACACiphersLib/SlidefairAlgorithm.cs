using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrypTool.ACACiphersLib
{
    internal class SlidefairAlgorithm : Cipher
    {
        public override int[] Decrypt(int[] ciphertext, int[] key)
        {
            List<int> result = new List<int>();
            int key_pos = 0;
            for (int i = 0; i < ciphertext.Length; i += 2)
            {
                int p1 = Tools.mod((ciphertext[i + 1] - key[key_pos]),LATIN_ALPHABET.Length);
                int p2 = Tools.mod((ciphertext[i] + key[key_pos]),LATIN_ALPHABET.Length);
                if (p1 == ciphertext[i] && p2 == ciphertext[i + 1])
                {
                    p1 = Tools.mod((p1 - 1), LATIN_ALPHABET.Length);
                    p2 = Tools.mod((p2 - 1),LATIN_ALPHABET.Length);
                }
                result.Add(p1);
                result.Add(p2);
                key_pos = Tools.mod((key_pos + 1),key.Length);
            }

            return result.ToArray();
        }

        public override int[] Encrypt(int[] plaintext, int[] key)
        {
            List<int> result = new List<int>();
            int key_pos = 0;
            for (int i = 0; i<plaintext.Length;i+=2)
            {
                int c1 = Tools.mod((plaintext[i + 1] - key[key_pos]), LATIN_ALPHABET.Length);
                int c2 = Tools.mod((plaintext[i] + key[key_pos]), LATIN_ALPHABET.Length);
                if (c1 == plaintext[i] && c2 == plaintext[i+1])
                {
                    c1 = Tools.mod((c1 + 1) , LATIN_ALPHABET.Length);
                    c2 = Tools.mod((c2 + 1) , LATIN_ALPHABET.Length);
                }
                result.Add(c1);
                result.Add(c2);
                key_pos = Tools.mod((key_pos + 1), key.Length);
            }

            return result.ToArray();
        }
    }
}
