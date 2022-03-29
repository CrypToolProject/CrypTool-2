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

    public class Playfair
    {
        internal static int DIM = 5;  // DIM = 6 works only with crib.
        internal static int[,] PERMUTATIONS = DIM == 5 ? Permutations.PERMUTATIONS5 : Permutations.PERMUTATIONS6;
        internal static int ALPHABET_SIZE = DIM == 5 ? 26 : 36;
        internal static int SQUARE = DIM * DIM;

        private static readonly int[][] POSITIONS_OF_PLAINTEXT_SYMBOL_1 = positionsOfPlainTextSymbol1();
        private static readonly int[][] POSITIONS_OF_PLAINTEXT_SYMBOL_2 = positionsOfPlainTextSymbol2();
        private static readonly int[][] POSITIONS_OF_CIPHERTEXT_SYMBOL_1 = positionsOfCipherTextSymbol1();
        private static readonly int[][] POSITIONS_OF_CIPHERTEXT_SYMBOL_2 = positionsOfCipherTextSymbol2();

        private static int row(int pos)
        {
            return pos / DIM;
        }
        private static int col(int pos)
        {
            return pos % DIM;
        }
        private static int pos(int r, int c)
        {
            if (r >= DIM)
            {
                r -= DIM;
            }
            else if (r < 0)
            {
                r += DIM;
            }
            if (c >= DIM)
            {
                c -= DIM;
            }
            else if (c < 0)
            {
                c += DIM;
            }
            return DIM * r + c;
        }
        private static int positionOfPlainTextSymbol1(int cipherPositionOfSymbol1, int cipherPositionOfSymbol2)
        {
            int c1 = col(cipherPositionOfSymbol1);
            int r1 = row(cipherPositionOfSymbol1);
            int c2 = col(cipherPositionOfSymbol2);
            int r2 = row(cipherPositionOfSymbol2);

            if (r1 == r2)
            {
                return pos(r1, c1 - 1);
            }
            else if (c1 == c2)
            {
                return pos(r1 - 1, c1);
            }
            return pos(r1, c2);
        }

        private static int[][] positionsOfPlainTextSymbol1()
        {
            int[][] positions = new int[SQUARE][];
            for (int p1 = 0; p1 < SQUARE; p1++)
            {
                positions[p1] = new int[SQUARE];
                for (int p2 = 0; p2 < SQUARE; p2++)
                {
                    positions[p1][p2] = positionOfPlainTextSymbol1(p1, p2);
                }
            }
            return positions;
        }

        private static int positionOfPlainTextSymbol2(int cipherPositionOfSymbol1, int cipherPositionOfSymbol2)
        {
            int c1 = col(cipherPositionOfSymbol1);
            int r1 = row(cipherPositionOfSymbol1);
            int c2 = col(cipherPositionOfSymbol2);
            int r2 = row(cipherPositionOfSymbol2);

            if (r1 == r2)
            {
                return pos(r2, c2 - 1);
            }
            else if (c1 == c2)
            {
                return pos(r2 - 1, c1);
            }
            return pos(r2, c1);
        }

        private static int[][] positionsOfPlainTextSymbol2()
        {
            int[][] positions = new int[SQUARE][];
            for (int p1 = 0; p1 < SQUARE; p1++)
            {
                positions[p1] = new int[SQUARE];
                for (int p2 = 0; p2 < SQUARE; p2++)
                {
                    positions[p1][p2] = positionOfPlainTextSymbol2(p1, p2);
                }
            }
            return positions;
        }

        private static int positionOfCipherTextSymbol1(int plainTextPositionOfSymbol1, int plainTextPositionOfSymbol2)
        {
            int c1 = col(plainTextPositionOfSymbol1);
            int r1 = row(plainTextPositionOfSymbol1);
            int c2 = col(plainTextPositionOfSymbol2);
            int r2 = row(plainTextPositionOfSymbol2);

            if (r1 == r2)
            {
                return pos(r1, c1 + 1);
            }
            else if (c1 == c2)
            {
                return pos(r1 + 1, c1);
            }
            return pos(r1, c2);
        }

        private static int[][] positionsOfCipherTextSymbol1()
        {
            int[][] positions = new int[SQUARE][];
            for (int p1 = 0; p1 < SQUARE; p1++)
            {
                positions[p1] = new int[SQUARE];
                for (int p2 = 0; p2 < SQUARE; p2++)
                {
                    positions[p1][p2] = positionOfCipherTextSymbol1(p1, p2);
                }
            }
            return positions;
        }

        private static int positionOfCipherTextSymbol2(int plainTextPositionOfSymbol1, int plainTextPositionOfSymbol2)
        {
            int c1 = col(plainTextPositionOfSymbol1);
            int r1 = row(plainTextPositionOfSymbol1);
            int c2 = col(plainTextPositionOfSymbol2);
            int r2 = row(plainTextPositionOfSymbol2);

            if (r1 == r2)
            {
                return pos(r2, c2 + 1);
            }
            else if (c1 == c2)
            {
                return pos(r2 + 1, c1);
            }
            return pos(r2, c1);
        }

        private static int[][] positionsOfCipherTextSymbol2()
        {
            int[][] positions = new int[SQUARE][];
            for (int p1 = 0; p1 < SQUARE; p1++)
            {
                positions[p1] = new int[SQUARE];
                for (int p2 = 0; p2 < SQUARE; p2++)
                {
                    positions[p1][p2] = positionOfCipherTextSymbol2(p1, p2);
                }
            }
            return positions;
        }

        private static int decrypt(Key key, int[] cipherText, int[] plainText, bool removeXZ)
        {

            key.computeInverse();

            int plainTextLength = 0;

            int lastPlainTextSymbol1 = 100;
            int lastPlainTextSymbol2 = 100;
            int plainTextSymbol1, plainTextSymbol2;
            int cipherPositionOfSymbol1, cipherPositionOfSymbol2;

            for (int n = 0; n < cipherText.Length; n += 2)
            {

                cipherPositionOfSymbol1 = key.inverseKey[cipherText[n]];
                cipherPositionOfSymbol2 = key.inverseKey[cipherText[n + 1]];

                plainTextSymbol1 = key.key[POSITIONS_OF_PLAINTEXT_SYMBOL_1[cipherPositionOfSymbol1][cipherPositionOfSymbol2]];
                plainTextSymbol2 = key.key[POSITIONS_OF_PLAINTEXT_SYMBOL_2[cipherPositionOfSymbol1][cipherPositionOfSymbol2]];

                if (removeXZ && (lastPlainTextSymbol1 == plainTextSymbol1 && (lastPlainTextSymbol2 == Utils.X || lastPlainTextSymbol2 == Utils.Z) && plainTextLength > 0))
                {
                    plainText[plainTextLength - 1] = plainTextSymbol1;
                }
                else
                {
                    plainText[plainTextLength++] = plainTextSymbol1;
                }

                plainText[plainTextLength++] = plainTextSymbol2;

                lastPlainTextSymbol1 = plainTextSymbol1;
                lastPlainTextSymbol2 = plainTextSymbol2;
            }

            if (removeXZ)
            {
                while (plainText[plainTextLength - 1] == Utils.X || plainText[plainTextLength - 1] == Utils.Z)
                {
                    plainTextLength--;
                }
            }
            return plainTextLength;
        }

        public static int decrypt(Key key, int[] cipherText, int[] plainText, int[] plainTextRemoveNulls)
        {


            key.computeInverse();

            int plainTextLength = 0;
            int plainTextRemoveNullsLength = 0;

            int lastPlainTextSymbol1 = 100;
            int lastPlainTextSymbol2 = 100;
            int plainTextSymbol1, plainTextSymbol2;
            int cipherPositionOfSymbol1, cipherPositionOfSymbol2;

            for (int n = 0; n < cipherText.Length; n += 2)
            {

                cipherPositionOfSymbol1 = key.inverseKey[cipherText[n]];
                cipherPositionOfSymbol2 = key.inverseKey[cipherText[n + 1]];

                plainTextSymbol1 = key.key[POSITIONS_OF_PLAINTEXT_SYMBOL_1[cipherPositionOfSymbol1][cipherPositionOfSymbol2]];
                plainTextSymbol2 = key.key[POSITIONS_OF_PLAINTEXT_SYMBOL_2[cipherPositionOfSymbol1][cipherPositionOfSymbol2]];

                if ((lastPlainTextSymbol1 == plainTextSymbol1 && (lastPlainTextSymbol2 == Utils.X || lastPlainTextSymbol2 == Utils.Z) && plainTextRemoveNullsLength > 0))
                {
                    plainTextRemoveNulls[plainTextRemoveNullsLength - 1] = plainText[plainTextLength++] = plainTextSymbol1;
                }
                else
                {
                    plainTextRemoveNulls[plainTextRemoveNullsLength++] = plainText[plainTextLength++] = plainTextSymbol1;
                }
                plainTextRemoveNulls[plainTextRemoveNullsLength++] = plainText[plainTextLength++] = plainTextSymbol2;

                lastPlainTextSymbol1 = plainTextSymbol1;
                lastPlainTextSymbol2 = plainTextSymbol2;
            }

            while (plainTextRemoveNullsLength > 1 && (plainText[plainTextRemoveNullsLength - 1] == Utils.X || plainText[plainTextRemoveNullsLength - 1] == Utils.Z))
            {
                plainTextRemoveNullsLength--;
            }

            return plainTextRemoveNullsLength;
        }

        public static int encrypt(Key key, int[] plainText, int[] cipherText)
        {

            key.computeInverse();

            int cipherTextLength = 0;

            int cipherTextSymbol1, cipherTextSymbol2;
            int plainTextPositionOfSymbol1, plainTextPositionOfSymbol2;

            for (int n = 0; n < plainText.Length; n += 2)
            {

                plainTextPositionOfSymbol1 = key.inverseKey[plainText[n]];
                if (n == plainText.Length - 1 || plainText[n] == plainText[n + 1])
                {
                    plainTextPositionOfSymbol2 = key.inverseKey[Utils.X];
                    n--;
                }
                else
                {
                    plainTextPositionOfSymbol2 = key.inverseKey[plainText[n + 1]];
                }

                cipherTextSymbol1 = key.key[POSITIONS_OF_CIPHERTEXT_SYMBOL_1[plainTextPositionOfSymbol1][plainTextPositionOfSymbol2]];
                cipherTextSymbol2 = key.key[POSITIONS_OF_CIPHERTEXT_SYMBOL_2[plainTextPositionOfSymbol1][plainTextPositionOfSymbol2]];
                if (cipherTextLength < cipherText.Length)
                {
                    cipherText[cipherTextLength++] = cipherTextSymbol1;
                    cipherText[cipherTextLength++] = cipherTextSymbol2;
                }
                else
                {
                    cipherTextLength += 2;
                }

            }

            return cipherTextLength;
        }

        public static void preparePlainText(int[] plaintext)
        {
            if (DIM == 6)
            {
                return;
            }
            for (int i = 0; i < plaintext.Length; i++)
            {
                if (plaintext[i] == Utils.J)
                {
                    plaintext[i] = Utils.I;
                }
            }
        }

        private static string preparePlainText(string p)
        {
            StringBuilder sb = new StringBuilder();
            if (DIM == 6)
            {
                p = p.ToUpper().Replace("[^A-Z0-9]*", "");
            }
            else
            {
                p = p.ToUpper().Replace("[^A-Z]*", "").Replace("J", "I");
            }
            for (int i = 0; i < p.Length; i += 2)
            {
                sb.Append(p[i]);
                if (i + 1 < p.Length)
                {
                    if (p[i] == p[i + 1])
                    {
                        sb.Append("X");
                    }
                    sb.Append(p[i + 1]);
                }
            }
            return sb.ToString();
        }
    }

}
