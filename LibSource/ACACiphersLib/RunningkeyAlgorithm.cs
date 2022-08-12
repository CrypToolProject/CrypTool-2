using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace CrypTool.ACACiphersLib
{
    public class RunningkeyAlgorithm : Cipher
    {
        public override int[] Decrypt(int[] ciphertext, int[] key)
        {
            throw new NotImplementedException(string.Format("Ciphertext can not be decrypted!"));
        }

        public override int[] Encrypt(int[] plaintext, int[] key)
        {
            int split = (plaintext.Length / 2) + (Tools.mod(plaintext.Length, 2));
            //Array.Copy(key,1,plaintext,0,key.Length);
            //Array.Copy(plaintext,split,plaintext,0,key.Length);
            key = plaintext.Skip(0).Take(split).ToArray();
            plaintext = plaintext.Skip(split).Take(plaintext.Length - split).ToArray();
            VigenereAlgorithm va = new VigenereAlgorithm();
            return va.Encrypt(plaintext,key);
        }
    }
}
