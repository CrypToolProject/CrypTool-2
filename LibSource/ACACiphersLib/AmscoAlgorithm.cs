using System;
using System.Collections.Generic;
using System.Linq;


namespace CrypTool.ACACiphersLib
{
    public class AmscoAlgorithm : Cipher
    {
        public override int[] Decrypt(int[] ciphertext, int[] key)
        {
            throw new NotImplementedException();
        }

        public override int[] Encrypt(int[] plaintext, int[] key)
        {
            var key_length = key.Length;
            var text_length = plaintext.Length;
            int counter = text_length;
            int char_num = 2;
            int [] chopped_text = new int[text_length];
            int key_counter = 0;
            int[] ciphertext = new int[text_length];
            int[] col_num = CalcColNumber(text_length,key_length);

            while (counter > 0) {
                for (int i = 0; i<= char_num; i++)
                {
                    chopped_text.Append(plaintext[i]);
                }
                counter -= char_num;
                key_counter = (key_counter + 1) % key_length;
                if (char_num == 1) 
                {
                    char_num = 2;
                }
                else 
                {
                    char_num = 1;
                }

                if(char_num == 2 && counter == 1){
                    char_num = 1;
                }
            }
            for (int i = 0; i<=key_length;i++)
            {
                ciphertext.Append(chopped_text[i]);
            }

            return ciphertext;
        }

        public int[] CalcColNumber(int textlength, int keylength)
        {
            int[] col_num = new int[keylength];
            for (int i = 0; i<=keylength;i++)
            {
                col_num[i] = 0;
            }
            int char_num = 2;
            int index_col = 0;
            int counter = textlength;

            while (counter > 0)
            {
                col_num[index_col] += char_num;
                counter -= char_num;
                index_col = Tools.mod(index_col+1,keylength);

                if (char_num == 1)
                {
                    char_num = 2;
                }
                else
                {
                    char_num = 1;
                }

                if (counter == 1 && char_num == 2)
                {
                    char_num = 1;
                }
            }

            return col_num;
        }
    }
}
