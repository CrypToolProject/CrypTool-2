using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrypTool.ACACiphersLib
{
    public class PortaAlgorithm : Cipher
    {
        public override int[] Decrypt(int[] ciphertext, int[] key)
        {
            return EncDec(ciphertext, key);
        }

        public override int[] Encrypt(int[] plaintext, int[] key)
        {
            return EncDec(plaintext, key);
        }

        public int[] EncDec(int[] text, int[] key)
        {
            List<int> result = new List<int>();
            int val = 0;
            for (int i =0; i<text.Length;i++)
            {
                if (text[i]<13)
                {
                    val = text[i] + key[i % key.Length] / 2;
                    if (val < 13)
                    {
                        val += 13;
                    }
                }
                else
                {
                    val = text[i] - key[i % key.Length] / 2;
                    if (val >= 13)
                    {
                        val -= 13;
                    }
                }
                result.Add(val);
            }

            return result.ToArray();
        }
    }
}
