using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrypTool.ACACiphersLib
{
    internal class GromarkAlgorithm : Cipher
    {
        public override int[] Decrypt(int[] ciphertext, int[] key)
        {
            List<int> result = new List<int>();

            return result.ToArray();
        }

        public override int[] Encrypt(int[] plaintext, int[] key)
        {
            List<int> result = new List<int>();
            //int[] primer = key[0];
            int i = 0;
            /*
            while()
            {
                primer.Add(Tools.mod(primer[i] + primer[i+1],LATIN_ALPHABET.Length));
                i += 1;
            }
            */
            return result.ToArray();
        }
    }
}
