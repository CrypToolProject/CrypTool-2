using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrypTool.ACACiphersLib
{
    internal class ProgressiveKeyAlgorithm : Cipher
    {

        public override int[] Decrypt(int[] ciphertext, int[] key)
        {
            //int progression_index = Math.Max(key.Length / 2,1);
            int progression_index = 1;
            VigenereAlgorithm va = new VigenereAlgorithm();
            for (int i = progression_index;i>0;i--)
            {
                List<int> newKey = new List<int>();
                for (int j=0; j<ciphertext.Length;j++)
                {
                   newKey.Add(Tools.mod((j / key.Length) * i, LATIN_ALPHABET.Length));
                }
                ciphertext = va.Decrypt(ciphertext.ToArray(), newKey.ToArray());
            }
            return va.Decrypt(ciphertext,key);
        }

        public override int[] Encrypt(int[] plaintext, int[] key)
        {
            //int progression_index = Math.Max(key.Length / 2, 1);
            int progression_index = 1;
            List <int> result = new List<int>();
            VigenereAlgorithm va = new VigenereAlgorithm();
            
            result = va.Encrypt(plaintext, key).ToList();

            for (int i = 1; i<progression_index+1;i++)
            {
                List<int> newKey = new List<int>();
                for (int j = 0; j<plaintext.Length;j++)
                {
                    newKey.Add(Tools.mod((j/key.Length)*i,LATIN_ALPHABET.Length));
                }
                result = va.Encrypt(result.ToArray(), newKey.ToArray()).ToList();
            }
            return result.ToArray();
        }
    }
}
