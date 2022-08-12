using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrypTool.ACACiphersLib
{
    public static class Tools
    {
        public static string[] morse_codes = { ".-","-...","-.-.","-..",".","..-","--.","....","..",".---","-.-",".-..","--","-.","---",".--.","--.-",".-.,","...","-","..-","...-",".--","-..-","-.--","--.."};
        public static string MapNumbersIntoTextSpace(int[] numbers, string alphabet)
        {
            StringBuilder builder = new StringBuilder();
            foreach (int i in numbers)
            {
                if (i == 100)
                {
                    builder.Append(" ");
                }
                else {
                    builder.Append(alphabet[i]);
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Maps a given string into the "numberspace" defined by the alphabet
        /// </summary>
        /// <param name="text"></param>
        /// <param name="alphabet"></param>
        /// <returns></returns>
        public static int[] MapTextIntoNumberSpace(string text, string alphabet)
        {
            int[] numbers = new int[text.Length];
            int position = 0;
            foreach (char c in text)
            {
                if (c.ToString() == " ")
                {
                    numbers[position] = 100;
                }
                else {
                    numbers[position] = alphabet.IndexOf(c);
                }
                position++;
            }
            return numbers;
        }

        public static int mod(int x, int m)
        {
            return (x % m + m) % m;
        }

        public static string EncryptMorse(int[] plaintext)
        {
            string morse_code = String.Empty;

            for (int i = 0; i<plaintext.Length;i++)
            {
                if (plaintext[i] == 26)
                {
                    morse_code += 'x';
                    continue;
                }
                morse_code += morse_codes[i] + 'x';
            }

            morse_code += 'x';

            return morse_code;
        }

        public static int npWhere(int[] collection, int element)
        {
            /*
            int result = 0;
            for (int i =0;i<collection.Length;i++)
            {
                if (collection[i] == element)
                {
                    result = collection[i];
                    return result;
                }
            }
            return result;
            */
            return collection[element];
        }


    }
}
