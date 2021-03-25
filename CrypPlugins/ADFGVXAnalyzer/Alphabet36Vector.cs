using common;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace ADFGVXAnalyzer
{
    public class Alphabet36Vector : Vector
    {
        private static readonly ADFGVXANalyzerSettings settings = new ADFGVXANalyzerSettings();
        static String ALPHABET = settings.Alphabet;
        static int ALPHABET_SIZE = ALPHABET.Length;
        private static StringBuilder ALPHABET_BUILDER = new StringBuilder(ALPHABET);

        public Alphabet36Vector(String s, bool withStats)
            : base(ALPHABET_BUILDER, s, withStats)
        { }

        public Alphabet36Vector(int length, bool withStats)

            : base(ALPHABET_BUILDER, length, withStats)
        { }

        public Alphabet36Vector(bool withStats)

            : this(ALPHABET_SIZE, withStats)
        { }

        public Alphabet36Vector()

            : this(ALPHABET_SIZE, false)
        { }

        public String toCleanString()
        {
            return Regex.Replace(ToString(), "[0-9XY]", " ");
            //return toString().replaceAll("[0-9XY]", " ");
        }
        public String toCleanString(int length)
        {
            string buffer = Regex.Replace(ToString(), "[0-9XY]", " ");
            return buffer.Substring(0, Math.Min(length, ToString().Length));
            //return toString().replaceAll("[0-9XY]", " ").substring(0, Math.Min(length, toString().Length));
        }
    }
}
