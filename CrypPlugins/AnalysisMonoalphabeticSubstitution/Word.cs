using System.Collections.Generic;

namespace CrypTool.AnalysisMonoalphabeticSubstitution
{
    internal class Word
    {
        #region Variables

        private readonly byte[] byteValue;
        private readonly byte[] pattern;
        private Candidate[] candidates;
        private bool enabled = true;

        #endregion

        # region Constructor

        public Word()
        {

        }

        public Word(int wordnum, byte[] byteValue)
        {
            this.byteValue = byteValue;

            pattern = makePattern(byteValue);

            List<byte> f = new List<byte>();
            for (int i = 0; i < byteValue.Length; i++)
            {
                if (!f.Contains(byteValue[i]))
                {
                    f.Add(byteValue[i]);
                }
            }
        }

        #endregion

        #region Properties

        public byte[] ByteValue
        {
            get => byteValue;
            set {; }
        }

        public Candidate[] Candidates
        {
            get => candidates;
            set => candidates = value;
        }

        public byte[] Pattern
        {
            get => pattern;
            set {; }
        }

        public bool Enabled
        {
            get => enabled;
            set => enabled = value;
        }

        #endregion

        #region Helper Methods

        public static byte[] makePattern(byte[] word)
        {
            byte[] pattern = new byte[word.Length];

            byte next = 1;

            byte[] map = new byte[256];

            for (int i = 0; i < word.Length; i++)
            {
                int c = word[i] & 0xff;

                if (map[c] == 0)
                {
                    map[c] = next;
                    next++;
                }

                pattern[i] = (byte)(map[c] - 1);
            }

            return pattern;
        }

        public static int compareArrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return a.Length - b.Length;
            }

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return a[i] - b[i];
                }
            }
            return 0;
        }

        #endregion
    }

    internal class WordComparer : IComparer<Word>
    {
        public int Compare(Word a, Word b)
        {
            if (b == null)
            {
                return -1;
            }
            return Word.compareArrays(a.ByteValue, b.ByteValue);
        }
    }

    internal class PatternComparer : IComparer<Word>
    {
        public int Compare(Word a, Word b)
        {
            return Word.compareArrays(a.Pattern, b.Pattern);
        }
    }

    internal class Candidate
    {
        #region Variables

        private readonly double fitness;
        private readonly byte[] byteValue;

        #endregion

        #region Properties

        public double Fitness
        {
            get => fitness;
            set {; }
        }

        public byte[] ByteValue
        {
            get => byteValue;
            set {; }
        }

        #endregion

        #region Constructor

        public Candidate(byte[] byteValue, double fitness)
        {
            this.byteValue = byteValue;
            this.fitness = fitness;
        }

        #endregion
    }

}
