using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CrypTool.ACACiphersLib
{
    public class AutokeyAlgorithm : Cipher
    {
        public override int[] Decrypt(int[] ciphertext, int[] key)
        {
            List<int> result = new List<int>();

            int k_index = 0;
            List<int> key_list = new List<int>(key);

            for (int i = 0; i < ciphertext.Length; i++)
            {
                int text = ciphertext[i];
                text -= key_list[k_index];
                key_list.Add(text);
                k_index+=1;
                text = Tools.mod(text, LATIN_ALPHABET.Length);
                result.Add(text);
            }

            return result.ToArray();
        }

        public override int[] Encrypt(int[] plaintext, int[] key)
        {
            List<int> result = new List<int>();

            int k_index = 0;
            List<int> key_list = new List<int>(key);

            for (int i = 0; i<plaintext.Length;i++)
            {
                int text = plaintext[i];
                text += key_list[k_index];
                key_list.Add(plaintext[i]);
                k_index+=1;
                text = Tools.mod(text,LATIN_ALPHABET.Length);
                result.Add(text);
            }

            return result.ToArray();
        }
    }
}
