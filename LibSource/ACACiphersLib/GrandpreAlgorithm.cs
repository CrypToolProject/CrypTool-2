using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrypTool.ACACiphersLib
{
    internal class GrandpreAlgorithm : Cipher
    {
        public override int[] Decrypt(int[] ciphertext, int[] key)
        {
            List<int> result = new List<int>();



            return result.ToArray();
        }

        public override int[] Encrypt(int[] plaintext, int[] key)
        {
            List<int> result = new List<int>();
            Random random = new Random();

            for (int p = 0;p<plaintext.Length;p++)
            {
                int rand = random.Next(0, key[p] -1);
            }


            return result.ToArray();
        }
    }
}
