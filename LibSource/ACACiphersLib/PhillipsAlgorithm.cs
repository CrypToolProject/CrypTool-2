using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrypTool.ACACiphersLib
{
    internal class PhillipsAlgorithm : Cipher
    {
        public override int[] Decrypt(int[] ciphertext, int[] key)
        {
            List<int> result = new List<int>(); 
            return result.ToArray();
        }

        public override int[] Encrypt(int[] plaintext, int[] key)
        {
            List<int> result = new List<int>();
            return result.ToArray();
        }

        public (List<int>,int) shiftKey(int i, int[] key, int key_shift_cntr, List<int> new_key)
        {
            int row_shift = 5 * key_shift_cntr;
            int[] tmp = new_key.Skip(row_shift).Take(row_shift+5).ToArray();
            
            
            return (new_key, key_shift_cntr);
        }
    }
}
