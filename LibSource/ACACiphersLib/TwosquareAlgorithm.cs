using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrypTool.ACACiphersLib
{
    internal class TwosquareAlgorithm : Cipher
    {
        public override int[] Decrypt(int[] ciphertext, int[] key)
        {
            throw new NotImplementedException();
        }

        public override int[] Encrypt(int[] plaintext, int[] key)
        {
            List<int> result = new List<int>();

            List<int> pt = plaintext.ToList();

            if (plaintext.Length % 2 != 0)
            {
                pt.Add(23);
            }



            return result.ToArray();
        }
    }
}
