using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrypTool.ACACiphersLib
{
    internal class CompleteColumnarTranspositionAlgorithm : Cipher
    {
        public override int[] Decrypt(int[] ciphertext, int[] key)
        {
            List<int> result = new List<int>();
            return result.ToArray();
        }

        public override int[] Encrypt(int[] plaintext, int[] key)
        {
            List<int> result = new List<int>();
            List<int> plaintext2 = new List<int>();

            while (Tools.mod(plaintext.Length,key.Length)!=0)
            {
                if (!(plaintext is List<int>))
                {
                    plaintext2 = plaintext.ToList();
                }
                plaintext2.Add(LATIN_ALPHABET.IndexOf("x"));
            }
            int position;
            for (int i = 0;i<key.Length;i++)
            {
                
            }

            return result.ToArray();
        }
    }
}
