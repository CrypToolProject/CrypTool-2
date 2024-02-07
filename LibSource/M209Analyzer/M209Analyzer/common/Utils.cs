/*
   Copyright CrypTool 2 Team josef.matwich@gmail.com

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
using M209AnalyzerLib.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace M209AnalyzerLib.Common
{
    public class Utils
    {
        public static readonly string HEXA_FILE = "hexa.bin";
        public static readonly string NGRAMS7_FILE = "english_7grams.bin";
        public static readonly string NGRAMS8_FILE = "english_8grams.bin";
        public static readonly string BOOK_FILE = "book.txt";

        public static readonly int A = GetTextSymbol('A');
        public static readonly int X = GetTextSymbol('X');
        public static readonly int Z = GetTextSymbol('Z');
        public static readonly int J = GetTextSymbol('J');
        public static readonly int I = GetTextSymbol('I');
        public static readonly int K = GetTextSymbol('K');

        public static readonly string TEXT_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static readonly int TEXT_ALPHABET_SIZE = TEXT_ALPHABET.Length;

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
                return (TEXT_ALPHABET.ElementAt(symbol));
            else
                return '?';

        }

        public static int[] GetText(string textString)
        {
            int[] text = new int[textString.Length];
            int len = 0;
            for (int i = 0; i < textString.Length; i++)
            {
                int c = GetTextSymbol(textString.ElementAt(i));
                if (c == -1)
                {
                    //continue;
                }
                text[len++] = c;
            }
            int[] destination = new int[textString.Length];
            Array.Copy(text, destination, len);
            return destination;
        }

        private static string from = "èéìùòàëáöæëüãþôâäíûóšøůěňïçñíàçèìåáßŕúµýˆ^άλêéąîőčžâªªºžńάλληφοράθęźðöżõřáěšďťˇי".ToUpper();
        private static string to = "eeiuoaeaoaeuapoaaiuosouenicniaceiaasrupyxxageeaioczaaaoznxxxxxxxxxxzoozoraesdtxe".ToUpper();

        public static string ReadTextFile(string fileName)
        {

            int[] text = new int[1000000];
            string line = "";
            int len = 0;

            List<string> lines = File.ReadAllLines(fileName).ToList();

            foreach (string lineTmp in lines)
            {
                char[] charArray = line.ToCharArray();

                for (int i = 0; i < charArray[i]; i++)
                {
                    int rep = from.IndexOf(charArray[i]);
                    if (rep != -1)
                    {
                        charArray[i] = to.ElementAt(rep);
                    }
                    int index = GetTextSymbol(charArray[i]);
                    if (index != -1)
                    {
                        text[len] = index;
                        len++;
                    }
                }
            }

            int[] textCopy = new int[1000000];
            Array.Copy(text, textCopy, len);
            string cipherStr = GetString(textCopy);

            Console.WriteLine($"Text file read: {fileName}, length = {len} \n{cipherStr}\n");

            return cipherStr;
        }

        public static int ReadTextSegmentFromFile(string filename, int startPosition, int[] text)
        {

            int length = 0;

            List<string> lines = File.ReadAllLines(filename).ToList();

            int position = 0;
            string line = "";

            for (int i = 0; i < lines.Count && length < text.Length; i++)
            {
                line = lines[i];

                if (position > startPosition)
                {
                    for (int j = 0; j < line.Length; j++)
                    {
                        char c = line.ElementAt(j);
                        int rep = from.IndexOf(c);
                        if (rep != -1)
                        {
                            c = to.ElementAt(rep);
                        }
                        int index = GetTextSymbol(c);
                        if ((index != -1) && (length < text.Length))
                        {
                            text[length] = index;
                            length++;
                        }
                    }
                }
                position += line.Length;
            }
            Console.WriteLine($"Read segment from file: {filename}, Position: {startPosition} , Length: {length}\n");
            Console.WriteLine($"{GetString(text)}\n\n");

            return length;
        }

        public static string GetString(int[] text, bool displayWithSpaces = true)
        {
            return GetString(text, text.Length, displayWithSpaces);
        }

        public static string GetString(int[] text, int length, bool displayWithSpaces)
        {
            StringBuilder m = new StringBuilder();
            for (int i = 0; i < Math.Min(text.Length, length); i++)
            {
                m.Append(GetTextChar(text[i]));
            }
            string plainText = m.ToString();

            if (displayWithSpaces)
            {
                plainText = plainText.Replace("Z", " ");
            }
            return plainText;
        }

        private static long startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        public static long GgetElapsedMillis()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startTime + 1;
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

        public static string ReadPlaintextSegmentFromFile(string dirname, Language language, int from, int requiredLength, bool m209)
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
            return ReadPlaintextSegmentFromFile(dirname + "/" + filename, from, requiredLength, m209);
        }

        public static string ReadPlaintextSegmentFromResource(Language language, int from, int requiredLength, bool m209)
        {
            string resource = null;
            switch (language)
            {
                case Language.ENGLISH:
                    resource = Properties.Resources.book;
                    break;
                case Language.FRENCH:
                    resource = Properties.Resources.frenchbook;
                    break;
                case Language.ITALIAN:
                    resource = null;
                    break;
                case Language.GERMAN:
                    resource = Properties.Resources.germanbook;
                    break;

            }
            return ReadPlaintextSegmentFromResource(Properties.Resources.book, from, requiredLength, m209);
        }

        // read a plain text segment from a file at a given position and length
        private static string ReadPlaintextSegmentFromFile(string fileName, int startPosition, int requiredLength, bool m209)
        {

            StringBuilder text = new StringBuilder();
            string line;
            int position = 0;
            int fileLength = (int)new FileInfo(fileName).Length;
            if (fileLength == 0)
            {
                throw new IOException($"Unable to read text file {fileName}");
            }
            if (startPosition < 0)
            {
                startPosition = RandomGen.NextInt(80 * fileLength / 100);
            }

            List<string> lines = File.ReadAllLines(fileName).ToList();


            for (int i = 0; i < lines.Count && (text.Length < requiredLength); i++)
            {
                line = lines[i];

                line += " ";
                if (position > startPosition)
                {
                    line = line.ToUpper();
                    for (int j = 0; (j < line.Length) && (text.Length < requiredLength); j++)
                    {
                        char c = line.ElementAt(j);
                        int rep = from.IndexOf(c);
                        if (rep != -1)
                        {
                            c = to.ElementAt(rep);
                        }
                        if (GetTextSymbol(c) == -1)
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

            return text.ToString();
        }

        private static string ReadPlaintextSegmentFromResource(string resource, int startPosition, int requiredLength, bool m209)
        {

            StringBuilder text = new StringBuilder();
            string line;
            int position = 0;
            int fileLength = (int)resource.Length;

            if (startPosition < 0)
            {
                startPosition = RandomGen.NextInt(80 * fileLength / 100);
            }

            StringReader stringReader = new StringReader(resource);

            while (((line = stringReader.ReadLine()) != null) && (text.Length < requiredLength))
            {
                line += " ";
                if (position > startPosition)
                {
                    line = line.ToUpper();
                    for (int j = 0; (j < line.Length) && (text.Length < requiredLength); j++)
                    {
                        char c = line.ElementAt(j);
                        int rep = from.IndexOf(c);
                        if (rep != -1)
                        {
                            c = to.ElementAt(rep);
                        }
                        if (GetTextSymbol(c) == -1)
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

            return text.ToString();
        }
    }
}
