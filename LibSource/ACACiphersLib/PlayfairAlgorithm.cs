using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrypTool.ACACiphersLib
{
    internal class PlayfairAlgorithm : Cipher
    {
        public override int[] Decrypt(int[] ciphertext, int[] key)
        {
            List<int> result = new List<int>();

            for (int i = 1; i<ciphertext.Length;i+=2)
            {

            }

            return result.ToArray();
        }

        public override int[] Encrypt(int[] plaintext, int[] key)
        {
            List<int> result = new List<int>();

            for (int i = 1; i < plaintext.Length; i += 2)
            {
                int p0 = plaintext[i-1];
                int p1 = plaintext[i];
            }

            return result.ToArray();
        }

        public int get_right_neighbour(int index)
        {
            if (Tools.mod(index,5) <4)
            {
                return index + 1;
            }
            if (Tools.mod(index,5)==4)
            {
                return index - 4;
            }
            return -1;
        }

        public int get_lower_neighbour(int index)
        {
            return Tools.mod(index+5,25);
        }

        public int get_substitute(int row, int col)
        {
            return 5 * row + col;
        }

        public int get_left_neighbour(int index)
        {
            if (Tools.mod(index,5)>0)
            {
                return index - 1;
            }
            if (Tools.mod(index, 5) == 0)
            {
                return index +4;
            }
            return -1;
        }

        public int get_upper_neighbour(int index)
        {
            return Tools.mod(index-5,25);
        }
    }
}
