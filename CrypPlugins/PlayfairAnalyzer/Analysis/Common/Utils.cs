using System;
using System.Text;

namespace PlayfairAnalysis.Common
{
    public enum Language { ENGLISH, FRENCH, GERMAN, ITALIAN }

    public class Utils
    {
        private readonly Random random;
        private readonly long startTime = DateTime.Now.Ticks;

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

        public Utils(int seed)
        {
            random = new Random(seed);
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

        public static int[] getText(string textString)
        {
            int[] text = new int[textString.Length];
            int len = 0;
            for (int i = 0; i < textString.Length; i++)
            {
                int c = getTextSymbol(textString[i]);
                if (c == -1)
                {
                    //continue;
                }
                text[len++] = c;
            }
            return Arrays.copyOf(text, len);
        }

        private static readonly string from = "èéìùòàëáöæëüãþôâäíûóšøůěňïçñíàçèìåáßŕúµýˆ^άλêéąîőčžâªªºžńάλληφοράθęźðöżõřáěšďťˇי".ToUpper();
        private static readonly string to = "eeiuoaeaoaeuapoaaiuosouenicniaceiaasrupyxxageeaioczaaaoznxxxxxxxxxxzoozoraesdtxe".ToUpper();

        public static string getString(int[] text)
        {
            return getString(text, text.Length);
        }

        public static string getString(int[] text, int length)
        {
            StringBuilder m = new StringBuilder();
            for (int i = 0; i < Math.Min(text.Length, length); i++)
            {
                m.Append(getTextChar(text[i]));
            }
            return m.ToString();
        }

        public TimeSpan getElapsed()
        {
            return new TimeSpan(DateTime.Now.Ticks - startTime + 1);
        }

        public int randomNextInt(int range)
        {
            return random.Next(range);
        }
        public int randomNextInt()
        {
            return random.Next();
        }
        public double randomNextDouble()
        {
            return random.NextDouble();
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
    }
}
