using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrypTool.ACACiphersLib
{
    internal class MyszkowskiAlgorithm : Cipher
    {
        public override int[] Decrypt(int[] ciphertext, int[] key)
        {
            List<int> result = new List<int>();

            for (int i =0;i<ciphertext.Length;i++)
            {
                result.Add(0);
            }

            int pos = 0;

            for (int i = 0; i < key.Max() + 1; i++)
            {
                if (!key.Contains(i))
                {
                    continue;
                }
                for (int p = 0; p < ciphertext.Length; p++)
                {
                    if (key[Tools.mod(p, key.Length)] == i)
                    {
                        result[p] = ciphertext[pos];
                        pos+=1;
                    }
                }
            }

            return result.ToArray();
        }

        public override int[] Encrypt(int[] plaintext, int[] key)
        {
            List<int> result = new List<int>();

            for (int i =0;i<key.Max()+1; i++)
            {
                if (!key.Contains(i))
                {
                    continue;
                }
                for (int p =0; p<plaintext.Length;p++)
                {
                    if (key[Tools.mod(p,key.Length)] == i)
                    {
                        result.Add(plaintext[p]);
                    }
                }
            }

            return result.ToArray();
        }
    }
}
