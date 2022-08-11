using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrypTool.ACACiphersLib
{
    internal class BaconianAlgorithm : Cipher
    {

        int[][] baconian = new int[][] { new int[]{ 0, 0, 0, 0, 0 }, new int[] { 0, 0, 0, 0, 1 }, new int[] { 0, 0, 0, 1, 0 }, new int[] { 0,0,0,1,1}, new int[] { 0,0,1,0,0}, new int[] { 0,0,1,0,1}, new int[] { 0,0,1,1,0}, new int[] { 0,0,1,1,1}, new int[] { 0,1,0,0,0}, new int[] { 0, 1, 0, 0, 0 }, new int[] { 0,1,0,0,1}, new int[] { 0,1,0,1,0}, new int[] { 0, 1, 0, 1, 1 }, new int[] { 0,1,1,0,0}, new int[] { 0,1,1,0,1}, new int[] { 0,1,1,1,0}, new int[] { 0, 1, 1, 1, 1 }, new int[] { 1,0,0,0,0}, new int[] { 1,0,0,0,1}, new int[] { 1,0,0,1,0}, new int[] { 1,0,0,1,1}, new int[] { 1,0,0,1,1}, new int[] { 1,0,1,0,0}, new int[] { 1,0,1,0,1}, new int[] { 1,0,1,1,0}, new int[] { 1,0,1,1,1} };
        public override int[] Decrypt(int[] ciphertext, int[] key)
        {
            List<int> result = new List<int>();
            List<int> tmp = new List<int>();

            foreach (int c in ciphertext)
            {
                tmp.Add(c/13);
                if (tmp.Count == 5)
                {
                    for (int i = 0; i<baconian.Length;i++)
                    {
                        if (baconian[i] == tmp.ToArray())
                        {
                            result.Add(Array.FindIndex(baconian,row => row == tmp.ToArray()));
                        }
                        tmp.Clear();
                    }
                }
            }

            return result.ToArray();
        }

        public override int[] Encrypt(int[] plaintext, int[] key)
        {
            List<int> result = new List<int>();
            var random = new Random();

            foreach (int p in plaintext)
            {
                foreach (int k in baconian[p]) {
                    int r = 0;
                    if (p < 13)
                    {
                        r = random.Next(0, 12);
                    }
                    else
                    {
                        r = random.Next(13, 25);
                    }

                    if (r == 9 || r == 21)
                    {
                        r -= 1;
                    }
                    result.Add(r);
                }
               
            }

            return result.ToArray();
        }
    }
}
