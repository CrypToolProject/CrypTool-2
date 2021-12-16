/*
   Copyright 2020 George Lasry
   Converted in 2020 from Java to C# by Nils Kopal

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
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EnigmaAnalyzerLib.Common
{
    public class Utils
    {

        public static string HEXA_FILE = "hexa.bin";
        public static string NGRAMS7_FILE = "english_7grams.bin";
        public static string NGRAMS8_FILE = "english_8grams.bin";
        public static string BOOK_FILE = "book.txt";

        public static int A = getTextSymbol('A');
        public static int X = getTextSymbol('X');
        public static int Z = getTextSymbol('Z');
        public static int J = getTextSymbol('J');
        public static int I = getTextSymbol('I');
        public static int K = getTextSymbol('K');

        public static string TEXT_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static int TEXT_ALPHABET_SIZE = TEXT_ALPHABET.Length;

        private static readonly int[][] perms6 = createPerms6();

        public static class Arrays
        {
            public static int[] CopyOf(int[] array, int size)
            {
                int[] array2 = new int[size];
                Array.Copy(array, 0, array2, 0, size);
                return array2;
            }

            public static void fill(int[] array, int value)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = value;
                }
            }
        }

        public static int getTextSymbol(char c)
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

        public static char getTextChar(int symbol)
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

        public static int[] getText(string textstring)
        {
            int[] text = new int[textstring.Length];
            int len = 0;
            for (int i = 0; i < textstring.Length; i++)
            {
                int c = getTextSymbol(textstring[i]);
                if (c == -1)
                {
                    //continue;
                }
                text[len++] = c;
            }
            return Arrays.CopyOf(text, len);
        }

        private static readonly string from = "èéìùòàëáöæëüãþôâäíûóšøůěňïçñíàçèìåáßŕúµýˆ^άλêéąîőčžâªªºžńάλληφοράθęźðöżõřáěšďťˇי".ToUpper();
        private static readonly string to = "eeiuoaeaoaeuapoaaiuosouenicniaceiaasrupyxxageeaioczaaaoznxxxxxxxxxxzoozoraesdtxe".ToUpper();

        public static string readTextFile(string fileName)
        {

            int[] text = new int[1000000];
            string line;
            int len = 0;

            try
            {
                FileStream fileReader = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                StreamReader bufferedReader = new StreamReader(fileReader);

                while ((line = bufferedReader.ReadLine()) != null)
                {
                    foreach (char c in line.ToCharArray())
                    {
                        int rep = from.IndexOf(c);
                        char c2 = c;
                        if (rep != -1)
                        {
                            c2 = to[rep];
                        }
                        int index = getTextSymbol(c2);
                        if (index != -1)
                        {
                            text[len] = index;
                            len++;
                        }
                    }
                }

                bufferedReader.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine("Unable to read text file {0} - {1}", fileName, ex.ToString());
            }

            string cipherStr = getstring(Arrays.CopyOf(text, len));

            Console.WriteLine("Text file read: {0}, length = {1}{2}", fileName, len, cipherStr);

            return cipherStr;
        }

        public static int readTextSegmentFromFile(string filename, int startPosition, int[] text)
        {

            int length = 0;

            try
            {
                FileStream fileReader = new FileStream(filename, FileMode.Open, FileAccess.Read);

                StreamReader bufferedReader = new StreamReader(fileReader);

                int position = 0;
                string line = "";

                while (((line = bufferedReader.ReadLine()) != null) && (length < text.Length))
                {
                    if (position > startPosition)
                    {
                        for (int i = 0; i < line.Length; i++)
                        {
                            char c = line[i];
                            int rep = from.IndexOf(c);
                            if (rep != -1)
                            {
                                c = to[rep];
                            }
                            int index = getTextSymbol(c);
                            if ((index != -1) && (length < text.Length))
                            {
                                text[length] = index;
                                length++;
                            }
                        }
                    }
                    position += line.Length;
                }

                bufferedReader.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine("Unable to read text file {0} - {1}", filename, ex.ToString());
            }
            Console.WriteLine("Read segment from file: {0}, Position: {1} , Length: {2}", filename, startPosition, length);
            Console.WriteLine("{0}", getstring(text));

            return length;
        }

        public static int readRandomSentenceFromFile(string filename)
        {


            List<HashSet<string>> lists = new List<HashSet<string>>();
            for (int l = 0; l < 10000; l++)
            {
                lists.Add(new HashSet<string>());
            }
            try
            {
                FileStream fileReader = new FileStream(filename, FileMode.Open, FileAccess.Read);

                StreamReader bufferedReader = new StreamReader(fileReader);

                StringBuilder s = new StringBuilder();
                string line = "";

                while ((line = bufferedReader.ReadLine()) != null)
                {
                    line += ' ';
                    for (int i = 0; i < line.Length; i++)
                    {
                        char c = line[i];
                        int rep = from.IndexOf(c);
                        if (rep != -1)
                        {
                            c = to[rep];
                        }
                        if (c == ' ')
                        {
                            if (s.Length > 0 && s[s.Length - 1] != ' ')
                            {
                                s.Append(c);
                            }
                        }
                        else if (c == '.' || c == ';' || c == ':' || c == '\"')
                        {
                            if (s.Length >= 6 && s.Length <= 50 && s[0] >= 'A' && s[0] <= 'Z')
                            {
                                string clean = s.ToString().Replace(" ", "").ToUpper();
                                lists[clean.Length].Add(clean);
                            }
                            s.Clear();
                        }
                        else if (c >= 'a' && c <= 'z')
                        {
                            s.Append(c);
                        }
                        else if (c >= 'A' && c <= 'Z')
                        {
                            s.Append(c);
                        }
                    }

                }

                bufferedReader.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine("Unable to read text file {0} - {1}", filename, ex.ToString());
            }
            for (int l = 0; l < lists.Count; l++)
            {
                if (lists[l].Count > 10)
                {
                    Console.WriteLine("{0} {1}", l, lists[l].Count);
                }
                foreach (string s in lists[l])
                {
                    int[] t = Utils.getText(s);
                    if (t.Length >= 6)
                    {
                        long score = Stats.evalPlaintextHexagram(t);
                        Console.WriteLine("%,5d {0} %,d", l, s, score);
                    }

                }

            }
            return 0;
        }

        public static int[] readRandomSentenceFromFile(string filename, string prefix, int length, bool playfair)
        {


            List<string> list = new List<string>();

            try
            {
                FileStream fileReader = new FileStream(filename, FileMode.Open, FileAccess.Read);

                StreamReader bufferedReader = new StreamReader(fileReader);

                StringBuilder s = new StringBuilder();
                string line = "";

                while ((line = bufferedReader.ReadLine()) != null)
                {
                    line += ' ';
                    for (int i = 0; i < line.Length; i++)
                    {
                        char c = line[i];
                        int rep = from.IndexOf(c);
                        if (rep != -1)
                        {
                            c = to[rep];
                        }
                        if (c == ' ')
                        {
                            if (s.Length > 0 && s[s.Length - 1] != ' ')
                            {
                                s.Append(c);
                            }
                        }
                        else if (c == '.' || c == ';' || c == ':' || c == '\"')
                        {
                            if (s.Length >= 6 && s.Length <= 1000 && s[0] >= 'A' && s[0] <= 'Z')
                            {
                                string clean = prefix + s.ToString().Replace(" ", "").ToUpper();

                                if (clean.Length == length && (!playfair || !clean.Contains("J")))
                                {
                                    int[] t = Utils.getText(clean);
                                    long score = Stats.evalPlaintextHexagram(t);
                                    if (score > 2_200_000)
                                    {
                                        list.Add(clean);
                                    }
                                }
                            }
                            s.Clear();
                        }
                        else if (c >= 'a' && c <= 'z')
                        {
                            s.Append(c);
                        }
                        else if (c >= 'A' && c <= 'Z')
                        {
                            s.Append(c);
                        }
                    }

                }

                bufferedReader.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine("Unable to read file {0} - {1}", filename, ex.ToString());
            }
            if (list.Count == 0)
            {
                Console.WriteLine("Unable to read sentence from text file {0} with {1} letters", filename, length);
            }
            return Utils.getText(list[new Random().Next(list.Count)]);
        }

        public static string getstring(int[] text)
        {
            return getstring(text, text.Length);
        }

        public static string getstring(int[] text, int length)
        {
            StringBuilder m = new StringBuilder();
            for (int i = 0; i < Math.Min(text.Length, length); i++)
            {
                m.Append(getTextChar(text[i]));
            }
            return m.ToString();
        }

        private static readonly DateTime startTime = DateTime.Now;
        public static long getElapsedMillis()
        {
            return (long)((DateTime.Now - startTime).TotalMilliseconds + 1);
        }

        public static Random random = new Random();
        public static int randomNextInt(int range)
        {
            return random.Next(range);
        }
        public static int randomNextInt()
        {
            return random.Next();
        }
        public static double randomNextDouble()
        {
            return random.NextDouble();
        }
        public static float randomNextFloat()
        {
            return (float)random.NextDouble();
        }

        public static int sum(int[] a)
        {
            int sum = 0;
            foreach (int i in a)
            {
                sum += i;
            }
            return sum;
        }

        public static long sum(long[] a)
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

        public static int[] randomPerm6()
        {
            return perms6[random.Next(perms6.Length)];
        }

        private static int[][] createPerms6()
        {
            int[][] perms6 = new int[6 * 5 * 4 * 3 * 2 * 1][];
            int index = 0;
            for (int i0 = 0; i0 < 6; i0++)
            {
                for (int i1 = 0; i1 < 6; i1++)
                {
                    if (i1 == i0)
                    {
                        continue;
                    }
                    for (int i2 = 0; i2 < 6; i2++)
                    {
                        if (i2 == i0 || i2 == i1)
                        {
                            continue;
                        }
                        for (int i3 = 0; i3 < 6; i3++)
                        {
                            if (i3 == i0 || i3 == i1 || i3 == i2)
                            {
                                continue;
                            }
                            for (int i4 = 0; i4 < 6; i4++)
                            {
                                if (i4 == i0 || i4 == i1 || i4 == i2 || i4 == i3)
                                {
                                    continue;
                                }
                                for (int i5 = 0; i5 < 6; i5++)
                                {
                                    if (i5 == i0 || i5 == i1 || i5 == i2 || i5 == i3 || i5 == i4)
                                    {
                                        continue;
                                    }
                                    perms6[index] = new int[6];
                                    perms6[index][0] = i0;
                                    perms6[index][1] = i1;
                                    perms6[index][2] = i2;
                                    perms6[index][3] = i3;
                                    perms6[index][4] = i4;
                                    perms6[index][5] = i5;
                                    index++;

                                }
                            }
                        }
                    }
                }
            }
            return perms6;
        }

        public static string readPlaintextSegmentFromFile(string dirname, Language language, int from, int requiredLength, bool m209)
        {
            string filename = null;
            switch (language)
            {
                case Language.ENGLISH:
                    filename = "book.txt";
                    break;
                case Language.FRENCH:
                    filename = "frenchbook.txt";
                    break;
                case Language.ITALIAN:
                    filename = "italianbook.txt";
                    break;
                case Language.GERMAN:
                    filename = "germanbook.txt";
                    break;
            }
            return readPlaintextSegmentFromFile(dirname + "/" + filename, from, requiredLength, m209);
        }
        // read a plain text segment from a file at a given position and length
        private static string readPlaintextSegmentFromFile(string fileName, int startPosition, int requiredLength, bool m209)
        {

            StringBuilder text = new StringBuilder();
            string line;
            int position = 0;
            int fileLength = (int)new FileInfo(fileName).Length;
            if (fileLength == 0)
            {
                Console.WriteLine("Cannot open file " + fileName);
            }
            if (startPosition < 0)
            {
                startPosition = randomNextInt(80 * fileLength / 100);
            }

            try
            {
                // FileStream reads text files in the default encoding.
                FileStream fileReader = new FileStream(fileName, FileMode.Open, FileAccess.Read);

                // Always wrap FileStream in StreamReader.
                StreamReader bufferedReader = new StreamReader(fileReader);

                while (((line = bufferedReader.ReadLine()) != null) && (text.Length < requiredLength))
                {
                    line += " ";
                    if (position > startPosition)
                    {
                        //Console.WriteLineln(line);
                        line = line.ToUpper();
                        for (int i = 0; (i < line.Length) && (text.Length < requiredLength); i++)
                        {
                            char c = line[i];
                            int rep = from.IndexOf(c);
                            if (rep != -1)
                            {
                                c = to[rep];
                            }
                            if (getTextSymbol(c) == -1)
                            {
                                if (m209)
                                {
                                    if (text.Length > 0 && text[text.Length - 1] == 'Z')
                                    {
                                        continue;
                                    }
                                    c = 'Z';
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            text.Append(c);
                        }
                    }
                    position += line.Length;
                }

                // Always close files.
                bufferedReader.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine("Unable to read book file {0} - {0}", fileName, ex.ToString());
            }

            Console.WriteLine("Generated Random Plaintext - Book: {0}, Position: {1} , Length: {2}", fileName, startPosition, text.Length);
            Console.WriteLine("{0}", text.ToString().Replace("Z", " "));

            return text.ToString();
        }
    }
}