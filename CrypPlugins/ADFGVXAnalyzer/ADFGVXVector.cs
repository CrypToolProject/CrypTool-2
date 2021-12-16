using common;
using System.Text;

namespace ADFGVXAnalyzer
{
    public class ADFGVXVector : Vector
    {
        private static readonly ADFGVXANalyzerSettings settings = new ADFGVXANalyzerSettings();
        private static readonly string ALPHABET = settings.EncryptAlphabet;

        //static String ALPHABET = "ADFGVXZ";
        private static readonly StringBuilder ALPHABET_BUILDER = new StringBuilder(ALPHABET);
        private static readonly int ALPHABET_SIZE = ALPHABET.Length;

        public ADFGVXVector(string s, bool withStats)
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
