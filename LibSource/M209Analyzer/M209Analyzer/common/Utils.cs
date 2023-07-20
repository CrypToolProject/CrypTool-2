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

        private static int[][] perms6 = CreatePerms6();

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

        public static int ReadRandomSentenceFromFile(string filename)
        {
            var lists = new List<HashSet<string>>();
            for (int l = 0; l < 10000; l++)
            {
                lists.Add(new HashSet<string>());
            }

            List<string> lines = File.ReadAllLines(filename).ToList();

            StringBuilder s = new StringBuilder();
            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                line += ' ';
                for (int j = 0; j < line.Length; j++)
                {
                    char c = line.ElementAt(j);
                    int rep = from.IndexOf(c);
                    if (rep != -1)
                    {
                        c = to.ElementAt(rep);
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
                        s.Length = 0;
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

            for (int l = 0; l < lists.Count(); l++)
            {
                if (lists[l].Count() > 10)
                {
                    Console.WriteLine($"{l} {lists[l].Count()}\n");
                }
                foreach (string str in lists[l])
                {
                    int[] t = Utils.GetText(str);
                    if (t.Length >= 6)
                    {
                        long score = Stats.EvalPlaintextHexagram(t);
                        Console.WriteLine($"{l} {str} {score}\n");
                    }

                }

            }
            return 0;
        }

        public static int[] ReadRandomSentenceFromFile(string filename, string prefix, int length, bool playfair)
        {
            List<string> list = new List<string>();

            List<string> lines = File.ReadAllLines(filename).ToList();

            StringBuilder s = new StringBuilder();
            string line = "";

            for (int i = 0; i < lines.Count; i++)
            {
                line = lines[i];

                line += ' ';
                for (int j = 0; j < line.Length; j++)
                {
                    char c = line.ElementAt(j);
                    int rep = from.IndexOf(c);
                    if (rep != -1)
                    {
                        c = to.ElementAt(rep);
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
                            String clean = prefix + s.ToString().Replace(" ", "").ToUpper();

                            if (clean.Length == length && (!playfair || !clean.Contains("J")))
                            {
                                int[] t = Utils.GetText(clean);
                                long score = Stats.EvalPlaintextHexagram(t);
                                if (score > 2_200_000)
                                {
                                    list.Add(clean);
                                }
                            }
                        }
                        s.Length = 0;
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

            if (list.Count == 0)
            {
                throw new IOException($"Unable to read sentence from text file {filename} with {length} letters");
            }

            return Utils.GetText(list[new Random().Next(list.Count)]);
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

        public static Random random = new Random((int)startTime);
        public static int RandomNextInt(int range)
        {
            return random.Next(range);
        }
        public static int RandomNextInt()
        {
            return random.Next();
        }
        public static double RandomNextDouble()
        {
            return random.NextDouble();
        }
        public static float RandomNextFloat()
        {
            // https://stackoverflow.com/questions/3365337/best-way-to-generate-a-random-float-in-c-sharp
            var result = (random.NextDouble()
                  * (Single.MaxValue - (double)Single.MinValue))
                  + Single.MinValue;
            return (float)result;
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

        //public static bool in(int x, int... a) {
        //    for (int i : a) {
        //        if (i == x) {
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        public static int[] RandomPerm6()
        {
            return perms6[random.Next(perms6.Length)];
        }

        private static int[][] CreatePerms6()
        {
            int[][] perms6 = new int[6 * 5 * 4 * 3 * 2 * 1][];
            for (int i = 0; i < 6 * 5 * 4 * 3 * 2 * 1; i++)
            {
                perms6[i] = new int[6];
            }

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
                startPosition = RandomNextInt(80 * fileLength / 100);
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
                startPosition = RandomNextInt(80 * fileLength / 100);
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
