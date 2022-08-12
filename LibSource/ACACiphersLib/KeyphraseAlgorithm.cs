using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrypTool.ACACiphersLib
{
    internal class KeyphraseAlgorithm : Cipher
    {
        public override int[] Decrypt(int[] ciphertext, int[] key)
        {
            throw new NotImplementedException(string.Format("Ciphertext can not be decrypted!"));
        }

        public override int[] Encrypt(int[] plaintext, int[] key)
        {
            List<int> result = new List<int>();

            for(int position =0; position < plaintext.Length; position++)
            {
                int p = plaintext[position];
                if(p == 100)
                {
                    result.Add(p);
                    continue;
                }
                result.Add(key[p]);
            }

            return result.ToArray();
        }
    }
}
