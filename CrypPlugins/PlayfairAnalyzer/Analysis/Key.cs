/*
   Copyright 2022 George Lasry, Nils Kopal, CrypTool 2 Team

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
using PlayfairAnalysis.Common;
using System.Text;

namespace PlayfairAnalysis
{

    public class Key
    {
        private readonly AnalysisInstance instance;
        private readonly Utils utils;

        internal int[] key;
        internal int[] inverseKey;
        private int[] cipher;
        private int[] crib;
        private int _cribOffset;
        internal int[] decryptionRemoveNulls;
        internal int decryptionRemoveNullsLength;
        internal int[] fullDecryption;
        internal long score;
        private string keyword;


        public Key(AnalysisInstance instance, Utils utils)
        {
            key = new int[Playfair.SQUARE];
            inverseKey = new int[Playfair.SQUARE + ((Playfair.DIM == 5) ? 1 : 0)];
            crib = null;
            decryptionRemoveNullsLength = 0;
            fullDecryption = null;
            decryptionRemoveNulls = null;
            score = 0;
            keyword = string.Empty;
            this.instance = instance;
            this.utils = utils;
        }


        public void Copy(Key key)
        {
            Arrays.Arraycopy(key.key, 0, this.key, 0, Playfair.SQUARE);
        }

        public long Eval()
        {
            Decrypt();

            long ngrams = instance.Stats.EvalPlaintext(decryptionRemoveNulls, decryptionRemoveNullsLength);
            if (crib == null)
            {
                score = ngrams;
            }
            else
            {
                long cribMatch = 0;
                for (int i = 0; i < crib.Length; i++)
                {
                    if (crib[i] == fullDecryption[i + _cribOffset])
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

        public bool MatchesFullCrib()
        {
            Decrypt();
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

        public void SetCipher(int[] c)
        {
            cipher = Arrays.CopyOf(c, c.Length);
            decryptionRemoveNulls = new int[c.Length];
            fullDecryption = new int[c.Length];
            decryptionRemoveNullsLength = 0;
        }

        public void SetCrib(string cribS, int cribOffset)
        {
            if (cribS != null && cribS.Length > 1)
            {
                crib = Utils.GetText(cribS);
                _cribOffset = cribOffset;
            }
        }

        public void ComputeInverse()
        {
            Arrays.Fill(inverseKey, -1);
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
                instance.CtAPI.GoodbyeFatalError("Invalid key " + ToString());
            }
        }

        public void Random()
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
                int j = i + utils.RandomNextInt(Playfair.SQUARE - i);
                Swap(i, j);
            }
            ComputeInverse();
        }

        public int Encrypt(int[] plain, int[] cipher)
        {
            return Playfair.Encrypt(this, plain, cipher);
        }
        public int Decrypt(int[] cipher, int[] plainRemoveNulls, int[] plain)
        {
            return Playfair.Decrypt(this, cipher, plain, plainRemoveNulls);
        }

        public override string ToString()
        {
            string s = Utils.GetString(key);
            StringBuilder ps = new StringBuilder();
            for (int i = 0; i < Playfair.SQUARE; i += Playfair.DIM)
            {               
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

        public void Decrypt()
        {
            decryptionRemoveNullsLength = Playfair.Decrypt(this, cipher, fullDecryption, decryptionRemoveNulls);
        }

        private void Swap(int p1, int p2)
        {
            if (p1 == p2)
            {
                return;
            }
            int keep = key[p1];
            key[p1] = key[p2];
            key[p2] = keep;
        }

        private void Swap(int p1, int p2, int p3)
        {
            int keep = key[p1];
            key[p1] = key[p2];
            key[p2] = key[p3];
            key[p3] = keep;
        }

        public void Swap(Key parent, int p1, int p2)
        {
            Copy(parent);
            Swap(p1, p2);
        }

        public void Swap(Key parent, int p1, int p2, int p3)
        {
            Copy(parent);
            Swap(p1, p2, p3);
        }

        public void SwapRows(Key parent, int r1, int r2)
        {
            Copy(parent);
            int start1 = r1 * Playfair.DIM;
            int start2 = r2 * Playfair.DIM;
            for (int m = 0; m < Playfair.DIM; m++)
            {
                Swap(start1 + m, start2 + m);
            }
        }

        public void SwapColumns(Key parent, int c1, int c2)
        {
            Copy(parent);
            for (int m = 0; m < Playfair.DIM; m++)
            {
                Swap(c1 + m * Playfair.DIM, c2 + m * Playfair.DIM);
            }
        }

        public void OermuteColumns(Key parent, int perm)
        {
            Copy(parent);
            for (int r = 0; r < Playfair.DIM; r++)
            {
                for (int c = 0; c < Playfair.DIM; c++)
                {
                    key[r * Playfair.DIM + c] = parent.key[r * Playfair.DIM + Playfair.PERMUTATIONS[perm, c]];
                }
            }
        }

        public void PermuteRowColumns(Key parent, int r, int perm)
        {
            Copy(parent);
            for (int c = 0; c < Playfair.DIM; c++)
            {
                key[r * Playfair.DIM + c] = parent.key[r * Playfair.DIM + Playfair.PERMUTATIONS[perm, c]];
            }
        }

        public void PermuteRows(Key parent, int perm)
        {
            Copy(parent);
            for (int r = 0; r < Playfair.DIM; r++)
            {
                Arrays.Arraycopy(parent.key, Playfair.PERMUTATIONS[perm, r] * Playfair.DIM, key, r * Playfair.DIM, Playfair.DIM);
            }
        }

        public void PermuteColumnsRows(Key parent, int c, int perm)
        {
            Copy(parent);
            for (int r = 0; r < Playfair.DIM; r++)
            {
                key[r * Playfair.DIM + c] = parent.key[Playfair.PERMUTATIONS[perm, r] * Playfair.DIM + c];
            }
        }

        private readonly int[] buffer = new int[Playfair.SQUARE];

        private (int bestR, int bestC) GetBestRC()
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

        public void AlignAlphabet()
        {
            (int bestR, int bestC) = GetBestRC();
            Arrays.Arraycopy(key, 0, buffer, 0, Playfair.SQUARE);
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
    }
}
