using System;
using System.Collections.Generic;
using System.Text;

namespace CrypTool.Plugins.UbchiPermutationCryptanalysis
{
    public static class UbchiCore
    {
        public static string PrepareText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }

            StringBuilder result = new StringBuilder();
            foreach (char c in text.ToUpper())
            {
                if (c >= 'A' && c <= 'Z')
                {
                    result.Append(c);
                }
            }
            return result.ToString();
        }

        public static string Transpose(string text, int[] permutation)
        {
            int numCols = permutation.Length;
            int numRows = (text.Length + numCols - 1) / numCols;

            char[,] grid = new char[numRows, numCols];
            int idx = 0;
            for (int r = 0; r < numRows; r++)
            {
                for (int c = 0; c < numCols; c++)
                {
                    grid[r, c] = idx < text.Length ? text[idx++] : (char)0;
                }
            }

            int[] orderToCol = new int[numCols];
            for (int c = 0; c < numCols; c++)
            {
                orderToCol[permutation[c] - 1] = c;
            }

            StringBuilder result = new StringBuilder();
            for (int order = 0; order < numCols; order++)
            {
                int colIdx = orderToCol[order];
                for (int r = 0; r < numRows; r++)
                {
                    if (grid[r, colIdx] != (char)0)
                    {
                        result.Append(grid[r, colIdx]);
                    }
                }
            }
            return result.ToString();
        }

        public static string InverseTranspose(string text, int[] permutation)
        {
            int numCols = permutation.Length;
            int numRows = (text.Length + numCols - 1) / numCols;

            int[] orderToCol = new int[numCols];
            for (int c = 0; c < numCols; c++)
            {
                orderToCol[permutation[c] - 1] = c;
            }

            int baseLen = text.Length / numCols;
            int extraChars = text.Length % numCols;
            int[] colLengths = new int[numCols];
            for (int i = 0; i < numCols; i++)
            {
                colLengths[i] = baseLen + (orderToCol[i] < extraChars ? 1 : 0);
            }

            string[] columns = new string[numCols];
            int pos = 0;
            for (int order = 0; order < numCols; order++)
            {
                int length = colLengths[order];
                columns[order] = pos + length <= text.Length ? text.Substring(pos, length) : text.Substring(pos);
                pos += length;
            }

            char[,] grid = new char[numRows, numCols];
            for (int order = 0; order < numCols; order++)
            {
                int colIdx = orderToCol[order];
                string colData = columns[order];
                for (int r = 0; r < colData.Length && r < numRows; r++)
                {
                    grid[r, colIdx] = colData[r];
                }
            }

            StringBuilder result = new StringBuilder();
            for (int r = 0; r < numRows; r++)
            {
                for (int c = 0; c < numCols; c++)
                {
                    if (grid[r, c] != (char)0)
                    {
                        result.Append(grid[r, c]);
                    }
                }
            }
            return result.ToString();
        }

        public static int[] CreateRandomPermutation(int length)
        {
            Random rng = new Random();
            return CreateRandomPermutation(length, rng);
        }

        public static int[] CreateRandomPermutation(int length, Random rng)
        {
            int[] perm = new int[length];
            for (int i = 0; i < length; i++)
            {
                perm[i] = i + 1;
            }
            for (int i = length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                int temp = perm[i];
                perm[i] = perm[j];
                perm[j] = temp;
            }
            return perm;
        }

        public static string EncryptUbchi(string plaintext, int[] perm, int nullCount)
        {
            string clean = PrepareText(plaintext);
            string step1 = Transpose(clean, perm);
            string withNulls = step1 + GenerateRandomNulls(nullCount);
            string step2 = Transpose(withNulls, perm);
            return step2;
        }

        public static string DecryptUbchi(string ciphertext, int[] perm, int nullCount)
        {
            string step1 = InverseTranspose(ciphertext, perm);
            string withoutNulls = nullCount > 0 && step1.Length > nullCount
                ? step1.Substring(0, step1.Length - nullCount)
                : step1;
            string plaintext = InverseTranspose(withoutNulls, perm);
            return plaintext;
        }

        private static string GenerateRandomNulls(int count)
        {
            if (count <= 0) return "";
            Random rng = new Random();
            StringBuilder sb = new StringBuilder(count);
            for (int i = 0; i < count; i++)
            {
                sb.Append((char)('A' + rng.Next(26)));
            }
            return sb.ToString();
        }
    }
}
