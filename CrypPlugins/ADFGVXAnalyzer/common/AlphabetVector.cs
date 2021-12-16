using System.Text;

namespace common
{
    public class AlphabetVector : Vector
    {
        public static string ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static StringBuilder ALPHABET_BUILDER = new StringBuilder(ALPHABET);

        public static int ALPHABET_SIZE = ALPHABET.Length;

        public AlphabetVector(string s, bool withStats)

            : base(ALPHABET_BUILDER, s, withStats)
        { }
        public AlphabetVector(int length, bool withStats)

            : base(ALPHABET_BUILDER, length, withStats)
        { }
        public AlphabetVector(bool withStats) : this(ALPHABET_SIZE, withStats) { }
        public AlphabetVector() : this(ALPHABET_SIZE, false) { }

    }

}
