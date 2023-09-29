using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CrypTool.Plugins.PlayfairAnalyzer
{
    class Utils
    {
        public static byte X = (byte)getCharIndex('X');
        public static byte Z = (byte)getCharIndex('Z');
        public static byte J = (byte)getCharIndex('J');
        public static byte K = (byte)getCharIndex('K');

        public static Random random = new Random();

        public static String getString(byte[] text, int len)
        {
            StringBuilder sb = new StringBuilder();

            for (int m = 0; m < len; m++)
            {
                if (text[m] < 26)
                {
                    char c = (char)(text[m] + 'A');
                    sb.Append(c);
                }
                else
                {
                    char c = (char)(text[m] + '0' - 26);
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        public static byte[] getText(String s)
        {
            return Regex.Replace(s.ToUpper(), "[^A-Z0-9]", "").Select(c => (byte)(c - (c >= 'A' ? 'A' : '0' - 26))).ToArray();
            /*
            int cipherTextLength = 0;

            for (int i = 0; i < s.Length; i++)
            {
                int index = getCharIndex(s[i]);
                if (index != -1)
                    cipherText[cipherTextLength++] = (byte)index;
            }

            return cipherTextLength;
            */
        }

        public static int getCharIndex(char c)
        {
            if ((c >= 'a') && (c <= 'z'))
                return (byte)(c - 'a');

            if ((c >= 'A') && (c <= 'Z'))
                return (byte)(c - 'A');

            if ((c >= '0') && (c <= '9'))
                return (byte)(c - '0' + 26);

            return -1;
        }

        /*
        public static int readCipherText(String fileName, byte[] cipherText, bool print)
        {
            String text = "";
            int cipherTextLength = 0;

            try
            {
                // FileReader reads text files in the default encoding.
                FileReader fileReader = new FileReader(fileName);

                // Always wrap FileReader in BufferedReader.
                BufferedReader bufferedReader = new BufferedReader(fileReader);

                File f = new File(fileName);

                byte[] bytes = Files.readAllBytes(f.toPath());
                text = new String(bytes, "UTF-8");

                // Always close files.
                bufferedReader.close();

                cipherTextLength = getText(text.ToString(), cipherText);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("Unable to open file '" + fileName + "'");
                return -1;
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error reading file '" + fileName + "'");
                return -1;
            }

            if (print)
            {
                Console.WriteLine(String.Format("File %s read successfully, length = %d \n%s\n", fileName, cipherTextLength, text));
            }

            return cipherTextLength;
        }
        */
    }
}