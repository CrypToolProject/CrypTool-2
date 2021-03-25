using System;
using System.Text;
using common;

namespace ADFGVXAnalyzer
{
    public class ADFGVXVector : Vector
    {
        private static readonly ADFGVXANalyzerSettings settings = new ADFGVXANalyzerSettings();
        static String ALPHABET = settings.EncryptAlphabet;
        //static String ALPHABET = "ADFGVXZ";
        static StringBuilder ALPHABET_BUILDER = new StringBuilder(ALPHABET);

        static int ALPHABET_SIZE = ALPHABET.Length;

        public ADFGVXVector(String s, bool withStats)
            : base(ALPHABET_BUILDER, s.ToUpper(), withStats)
        { }

        public ADFGVXVector(int length, bool withStats)
            : base(ALPHABET_BUILDER, length, withStats)
        { }

        public ADFGVXVector(bool withStats)
           : this(ALPHABET_SIZE, withStats)
        { }

        public ADFGVXVector()
            : this(ALPHABET_SIZE, false)
        { }
    }
}
