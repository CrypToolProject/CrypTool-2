/*
   Copyright 2022 George Lasry, Nils Kopal, CrypTool 2 Team

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using System.Text;

namespace PlayfairAnalysis.Common
{   
    public class Utils
    {
        private readonly Random random;
        private readonly long startTime = DateTime.Now.Ticks;

        public static string HEXA_FILE = "hexa.bin";
        public static string NGRAMS7_FILE = "english_7grams.bin";
        public static string NGRAMS8_FILE = "english_8grams.bin";
        public static string BOOK_FILE = "book.txt";

        public static int A = GetTextSymbol('A');
        public static int X = GetTextSymbol('X');
        public static int Z = GetTextSymbol('Z');
        public static int J = GetTextSymbol('J');
        public static int I = GetTextSymbol('I');
        public static int K = GetTextSymbol('K');

        public static string TEXT_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static int TEXT_ALPHABET_SIZE = TEXT_ALPHABET.Length;

        public Utils(int seed)
        {
            random = new Random(seed);
        }

        public static int GetTextSymbol(char c)
        {

            if (c >= 'a' && c <= 'z')
            {
                return c - 'a';
            }
            if (c >= 'A' && c <= 'Z')
            {
                return c - 'A';
            }
            return -1;
        }

        public static char GetTextChar(int symbol)
        {

            if ((symbol >= 0) && (symbol <= (TEXT_ALPHABET_SIZE - 1)))
            {
                return (TEXT_ALPHABET[symbol]);
            }
            else
            {
                return '?';
            }
        }

        public static int[] GetText(string textString)
        {
            int[] text = new int[textString.Length];
            int len = 0;
            for (int i = 0; i < textString.Length; i++)
            {
                int c = GetTextSymbol(textString[i]);
                if (c == -1)
                {
                    //continue;
                }
                text[len++] = c;
            }
            return Arrays.CopyOf(text, len);
        }

        public static string GetString(int[] text)
        {
            return GetString(text, text.Length);
        }

        public static string GetString(int[] text, int length)
        {
            StringBuilder m = new StringBuilder();
            for (int i = 0; i < Math.Min(text.Length, length); i++)
            {
                m.Append(GetTextChar(text[i]));
            }
            return m.ToString();
        }

        public TimeSpan GetElapsed()
        {
            return new TimeSpan(DateTime.Now.Ticks - startTime + 1);
        }

        public int RandomNextInt(int range)
        {
            return random.Next(range);
        }

        public int RandomNextInt()
        {
            return random.Next();
        }

        public double RandomNextDouble()
        {
            return random.NextDouble();
        }

        public static int Sum(int[] a)
        {
            int sum = 0;
            foreach (int i in a)
            {
                sum += i;
            }
            return sum;
        }

        public static long Sum(long[] a)
        {
            long sum = 0;
            foreach (long i in a)
            {
                sum += i;
            }
            return sum;
        }

        public static bool In(int x, params int[] a)
        {
            foreach (int i in a)
            {
                if (i == x)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
