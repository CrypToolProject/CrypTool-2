using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrypTool.ACACiphersLib
{
    internal class NumberedkeyAlgorithm : Cipher
    {
        public override int[] Decrypt(int[] ciphertext, int[] key)
        {
            List<int> result = new List<int>(); 
            return result.ToArray();
        }

        public override int[] Encrypt(int[] plaintext, int[] key)
        {

            //keyalphabet


            List<int> result = new List<int>();

            Random random = new Random();

            for (int i = 0; i< plaintext.Length;i++)
            {
                //int[] pos = key.Where(x => x == plaintext[i]).ToArray();
                int[] pos = (from number in key where number == plaintext[i] select number).ToArray();

                int position = pos[random.Next(0, pos.Length -1)];

                result.Add(LATIN_ALPHABET[position/10]);
                result.Add(LATIN_ALPHABET[Tools.mod(position,10)]);
            }

            return result.ToArray();
        }
    }
}
