using PlayfairAnalysis.Common;
using System;
using System.Text;

namespace PlayfairAnalysis
{

    public class Key
    {
        private readonly AnalysisInstance instance;
        private readonly Utils utils;
        private readonly NGrams nGrams;

        internal int[] key;
        internal int[] inverseKey;
        private int[] cipher;
        private int[] crib;
        internal int[] decryptionRemoveNulls;
        internal int decryptionRemoveNullsLength;
        internal int[] fullDecryption;
        internal long score;
        private String keyword;


        public Key(AnalysisInstance instance, Utils utils)
        {
            key = new int[Playfair.SQUARE];
            inverseKey = new int[Playfair.SQUARE + ((Playfair.DIM == 5) ? 1 : 0)];
            crib = null;
            decryptionRemoveNullsLength = 0;
            fullDecryption = null;
            decryptionRemoveNulls = null;
            score = 0;
            keyword = "";
            this.instance = instance;
            this.utils = utils;
            nGrams = new NGrams(instance);
        }


        public void copy(Key key)
        {
            Arrays.arraycopy(key.key, 0, this.key, 0, Playfair.SQUARE);
        }

        long evalNgrams()
        {
            decrypt();

            //long ngrams = Stats.evalPlaintextHexagram(decryptionRemoveNulls, decryptionRemoveNullsLength);
            long ngrams = nGrams.eval8(decryptionRemoveNulls, decryptionRemoveNullsLength);
            if (crib == null)
            {
                score = ngrams;
            }
            else
            {
                long cribMismatch = 0;
                for (int i = 0; i < crib.Length; i++)
                {
                    if (crib[i] != fullDecryption[i])
                    {
                        cribMismatch++;
                    }
                }
                if (crib.Length == cipher.Length)
                {
                    return 3_000_000 * (cipher.Length - cribMismatch) / cipher.Length;
                }
                score = -100_000 * cribMismatch + ngrams;
            }
            return score;
        }
        public long eval()
        {
            decrypt();

            long ngrams = instance.Stats.evalPlaintextHexagram(decryptionRemoveNulls, decryptionRemoveNullsLength);
            //long ngrams = NGrams.eval7(decryptionRemoveNulls, decryptionRemoveNullsLength);
            if (crib == null)
            {
                score = ngrams;
            }
            else
            {
                long cribMatch = 0;
                for (int i = 0; i < crib.Length; i++)
                {
                    if (crib[i] == fullDecryption[i])
                    {
                        cribMatch++;
                    }
                }
                if (crib.Length == cipher.Length)
                {
                    score = 3_000_000 * cribMatch / cipher.Length;
                }
                else
                {
                    score = ((3_000_000 * cribMatch) + (ngrams * (cipher.Length - crib.Length))) / cipher.Length;
                }
            }
            return score;
        }

        public bool matchesFullCrib()
        {
            decrypt();
            if (crib == null || crib.Length != cipher.Length)
            {
                return false;
            }
            for (int i = 0; i < crib.Length; i++)
            {
                if (crib[i] != fullDecryption[i])
                {
                    return false;
                }
            }
            return true;
        }

        public void setCipher(int[] c)
        {
            this.cipher = Arrays.copyOf(c, c.Length);
            this.decryptionRemoveNulls = new int[c.Length];
            this.fullDecryption = new int[c.Length];
            this.decryptionRemoveNullsLength = 0;
        }

        public void setCrib(String cribS)
        {
            if (cribS != null && cribS.Length > 1)
            {
                this.crib = Utils.getText(cribS);
            }
        }

        public void computeInverse()
        {
            Arrays.fill(inverseKey, -1);
            bool good = true;
            for (int position = 0; position < Playfair.SQUARE; position++)
            {
                int value = key[position];
                if (value < 0 || value > Playfair.SQUARE || (Playfair.DIM == 5 && value == Utils.J) || inverseKey[value] != -1)
                {
                    good = false;
                    break;
                }
                inverseKey[value] = position;
            }
            if (!good)
            {
                instance.CtAPI.goodbyeFatalError("Invalid key " + ToString());
            }
        }

        public void random()
        {
            if (Playfair.DIM == 6)
            {
                for (int symbol = 0; symbol < Playfair.SQUARE; symbol++)
                {
                    key[symbol] = symbol;
                }
            }
            else
            {
                for (int symbol = 0; symbol < Utils.J; symbol++)
                {
                    key[symbol] = symbol;
                }
                for (int symbol = Utils.K; symbol < Playfair.ALPHABET_SIZE; symbol++)
                {
                    key[symbol - 1] = symbol;
                }
            }
            for (int i = 0; i < Playfair.SQUARE - 1; i++)
            {
                int j = i + utils.randomNextInt(Playfair.SQUARE - i);
                swap(i, j);
            }
            computeInverse();
        }

        public int encrypt(int[] plain, int[] cipher)
        {
            return Playfair.encrypt(this, plain, cipher);
        }
        public int decrypt(int[] cipher, int[] plainRemoveNulls, int[] plain)
        {
            return Playfair.decrypt(this, cipher, plain, plainRemoveNulls);
        }

        public override String ToString()
        {
            String s = Utils.getString(key);
            StringBuilder ps = new StringBuilder();
            for (int i = 0; i < Playfair.SQUARE; i += Playfair.DIM)
            {
                //Do not append | character here, because CT2 uses "flat" notation for keys
                /*
                if (i != 0)
                {
                    ps.Append("|");
                }
                */
                ps.Append(s.Substring(i, Playfair.DIM));
            }
            if (keyword.Length == 0)
            {
                return ps.ToString();
            }
            else
            {
                return ps.ToString() + " (" + keyword + ")";
            }
        }

        public void decrypt()
        {
            decryptionRemoveNullsLength = Playfair.decrypt(this, cipher, fullDecryption, decryptionRemoveNulls);
        }

        private void swap(int p1, int p2)
        {
            if (p1 == p2)
            {
                return;
            }
            int keep = key[p1];
            key[p1] = key[p2];
            key[p2] = keep;
        }

        private void swap(int p1, int p2, int p3)
        {
            int keep = key[p1];
            key[p1] = key[p2];
            key[p2] = key[p3];
            key[p3] = keep;
        }

        public void swap(Key parent, int p1, int p2)
        {
            copy(parent);
            swap(p1, p2);
        }

        public void swap(Key parent, int p1, int p2, int p3)
        {
            copy(parent);
            swap(p1, p2, p3);
        }

        public void swapRows(Key parent, int r1, int r2)
        {
            copy(parent);
            int start1 = r1 * Playfair.DIM;
            int start2 = r2 * Playfair.DIM;
            for (int m = 0; m < Playfair.DIM; m++)
            {
                swap(start1 + m, start2 + m);
            }
        }

        public void swapCols(Key parent, int c1, int c2)
        {
            copy(parent);
            for (int m = 0; m < Playfair.DIM; m++)
            {
                swap(c1 + m * Playfair.DIM, c2 + m * Playfair.DIM);
            }
        }

        public void permuteCols(Key parent, int perm)
        {
            copy(parent);
            for (int r = 0; r < Playfair.DIM; r++)
            {
                for (int c = 0; c < Playfair.DIM; c++)
                {
                    key[r * Playfair.DIM + c] = parent.key[r * Playfair.DIM + Playfair.PERMUTATIONS[perm,c]];
                }
            }
        }

        public void permuteRowCols(Key parent, int r, int perm)
        {
            copy(parent);
            for (int c = 0; c < Playfair.DIM; c++)
            {
                key[r * Playfair.DIM + c] = parent.key[r * Playfair.DIM + Playfair.PERMUTATIONS[perm,c]];
            }
        }

        public void permuteRows(Key parent, int perm)
        {
            copy(parent);
            for (int r = 0; r < Playfair.DIM; r++)
            {
                Arrays.arraycopy(parent.key, Playfair.PERMUTATIONS[perm,r] * Playfair.DIM, key, r * Playfair.DIM, Playfair.DIM);
            }
        }

        public void permuteColRows(Key parent, int c, int perm)
        {
            copy(parent);
            for (int r = 0; r < Playfair.DIM; r++)
            {
                key[r * Playfair.DIM + c] = parent.key[Playfair.PERMUTATIONS[perm,r] * Playfair.DIM + c];
            }
        }

        private int[] buffer = new int[Playfair.SQUARE];        

        private (int bestR, int bestC) getBestRC()
        {
            int bestR = -1;
            int bestC = -1;
            int bestCount = 0;
            for (int r = 0; r < Playfair.DIM; r++)
            {
                for (int c = 0; c < Playfair.DIM; c++)
                {
                    int last = key[r * Playfair.DIM + c];
                    int count = 0;

                    for (int r1 = 0; r1 < Playfair.DIM; r1++)
                    {
                        for (int c1 = 0; c1 < Playfair.DIM; c1++)
                        {
                            int r2 = (r - r1 + Playfair.DIM) % Playfair.DIM;
                            int c2 = (c - c1 + Playfair.DIM) % Playfair.DIM;
                            int previous = key[r2 * Playfair.DIM + c2];
                            if (previous > last)
                            {
                                return (bestR, bestC);
                            }
                            last = previous;
                            count++;
                            //Console.Out.WriteLine("%2d %2d = %3d\n", r, c, count);
                            if (count > bestCount)
                            {
                                bestCount = count;
                                bestC = c;
                                bestR = r;
                            }
                        }
                    }

                }
            }

            return (bestR, bestC);
        }

        public void alignAlphabet()
        {
            var (bestR, bestC) = getBestRC();
            Arrays.arraycopy(key, 0, buffer, 0, Playfair.SQUARE);
            for (int r = 0; r < Playfair.DIM; r++)
            {
                for (int c = 0; c < Playfair.DIM; c++)
                {
                    int r2 = (r - bestR - 1 + Playfair.DIM) % Playfair.DIM;
                    int c2 = (c - bestC - 1 + Playfair.DIM) % Playfair.DIM;
                    key[r2 * Playfair.DIM + c2] = buffer[r * Playfair.DIM + c];
                }
            }

        }

        bool keyFromSentence(int[] phrase)
        {

            bool[] used = new bool[26];
            int length = 0;

            foreach (int symbol in phrase)
            {
                if (used[symbol])
                {
                    continue;
                }
                key[length++] = symbol;
                used[symbol] = true;
            }

            for (int symbol = 0; symbol < 26; symbol++)
            {
                if (used[symbol] || symbol == Utils.getTextSymbol('J'))
                {
                    continue;
                }
                key[length++] = symbol;
            }
            computeInverse();
            keyword = Utils.getString(phrase);
            return length == 25;
        }
    }
}
