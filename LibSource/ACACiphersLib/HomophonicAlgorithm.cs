using System;
using System.Collections.Generic;

namespace CrypTool.ACACiphersLib
{
    public class HomophonicAlgorithm : Cipher
    {
        public override int[] Decrypt(int[] ciphertext, int[] key)
        {
            List<int> plaintext = new List<int>();
            var random = new Random();
            int p;
            
            for (int i = 0; i < ciphertext.Length; i+=2)
            {
                int ct = LATIN_ALPHABET_WITH_EQUAL_I_AND_J[ciphertext[i]] * 10 + LATIN_ALPHABET_WITH_EQUAL_I_AND_J[ciphertext[i+1]] -1;
                if (ct==-1)
                {
                    ct = 99;
                }
                int rand = random.Next(ct / 25);
                if (ct <= key[rand])
                {
                    p = ct + key[rand] - 25 + rand;
                }
                else
                {
                    p = Tools.mod((ct - LATIN_ALPHABET_WITH_EQUAL_I_AND_J.Length + key[rand] +25 * rand) , LATIN_ALPHABET_WITH_EQUAL_I_AND_J.Length);
                }
                plaintext.Add(p);
            }

            return plaintext.ToArray();
        }

        public override int[] Encrypt(int[] plaintext, int[] key)
        {
            List<int> ciphertext = new List<int>();
            var random = new Random();

            string text_plaintext = Tools.MapNumbersIntoTextSpace(plaintext, LATIN_ALPHABET_WITH_EQUAL_I_AND_J);
            text_plaintext = text_plaintext.ToUpper();
            text_plaintext = text_plaintext.Replace("J", "I");
            plaintext = Tools.MapTextIntoNumberSpace(text_plaintext,LATIN_ALPHABET_WITH_EQUAL_I_AND_J);

            for (int i = 0;i<plaintext.Length;i++)
            {

                int ct;
                int rand = random.Next(0, 3);
                if (plaintext[i] >=key[rand])
                {
                    ct = plaintext[i] - key[rand] + 25 * rand+1;
                }
                else
                {
                    ct = LATIN_ALPHABET_WITH_EQUAL_I_AND_J.Length + plaintext[i] - key[rand] + 25 * rand +1;
                }
                ct = Tools.mod(ct , 100);
                int ct1 = Tools.mod(ct , 10);
                int ct2 = ct/ 10;
                int[] ct1_array = {ct1};
                int[] ct2_array = { ct2 };
                ciphertext.Add(LATIN_ALPHABET.IndexOf(Tools.MapNumbersIntoTextSpace(ct2_array, LATIN_ALPHABET_WITH_EQUAL_I_AND_J)));
                ciphertext.Add(LATIN_ALPHABET.IndexOf(Tools.MapNumbersIntoTextSpace(ct1_array,LATIN_ALPHABET_WITH_EQUAL_I_AND_J)));
            }

            return ciphertext.ToArray();
        }
    }
}
