using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrypTool.Plugins.PlayfairAnalyzer
{
    public class Key
    {
        public byte[] key;
        PlayfairAnalyzer solver;

        public Key(PlayfairAnalyzer solver)
        {
            this.solver = solver;
            key = new byte[solver.SQUARE];
        }

        public override string ToString()
        {
            String s = Utils.getString(key, key.Length);
            StringBuilder ps = new StringBuilder();

            for (int i = 0; i < solver.SQUARE; i += solver.DIM)
            {
                ps.Append(s.Substring(i, solver.DIM));
                ps.Append(Environment.NewLine);
            }

            return ps.ToString();
        }

        public void simple()
        {
            if (solver.DIM == 6)
            {
                for (byte symbol = 0; symbol < solver.SQUARE; symbol++)
                    key[symbol] = symbol;
            }
            else
            {
                for (byte symbol = 0; symbol < Utils.J; symbol++)
                    key[symbol] = symbol;

                for (byte symbol = Utils.K; symbol < solver.ALPHABET_SIZE; symbol++)
                    key[symbol - 1] = symbol;
            }
        }

        public void copy(Key toKey)
        {
            Array.Copy(this.key, 0, toKey.key, 0, solver.SQUARE);
        }

        public void random()
        {
            simple();

            for (int i = 0; i < solver.SQUARE - 1; i++)
                swap(i, i + Utils.random.Next(solver.SQUARE - i));
        }

        public void swap(int p1, int p2)
        {
            byte keep = key[p1];
            key[p1] = key[p2];
            key[p2] = keep;
        }

        public void swap(int p1, int p2, int p3)
        {
            byte keep = key[p1];
            key[p1] = key[p2];
            key[p2] = key[p3];
            key[p3] = keep;
        }

        public void swapRows(Key child, int r1, int r2)
        {
            copy(child);

            int start1 = r1 * solver.DIM;
            int start2 = r2 * solver.DIM;

            for (int m = 0; m < solver.DIM; m++)
                child.swap(start1 + m, start2 + m);
        }

        public void swapCols(Key child, int c1, int c2)
        {
            copy(child);

            for (int m = 0; m < solver.DIM; m++)
                child.swap(c1 + m * solver.DIM, c2 + m * solver.DIM);
        }

        public void permuteCols(Key child, int perm)
        {
            int[] permute = solver.permutations[perm];

            for (int r = 0; r < solver.DIM; r++)
                for (int c = 0; c < solver.DIM; c++)
                    child.key[r * solver.DIM + c] = key[r * solver.DIM + permute[c]];
        }

        public void permuteRowCols(Key child, int r, int perm)
        {
            int[] permute = solver.permutations[perm];

            for (int c = 0; c < solver.DIM; c++)
                child.key[r * solver.DIM + c] = key[r * solver.DIM + permute[c]];
        }

        public void permuteRows(Key child, int perm)
        {
            int[] permute = solver.permutations[perm];

            for (int r = 0; r < solver.DIM; r++)
                Array.Copy(key, permute[r] * solver.DIM, child.key, r * solver.DIM, solver.DIM);
        }

        public void permuteColRows(Key child, int c, int perm)
        {
            int[] permute = solver.permutations[perm];

            for (int r = 0; r < solver.DIM; r++)
                child.key[r * solver.DIM + c] = key[permute[r] * solver.DIM + c];
        }
    }
}