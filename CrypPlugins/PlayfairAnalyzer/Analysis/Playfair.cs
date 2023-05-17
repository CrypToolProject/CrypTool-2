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

        private static readonly int[][] POSITIONS_OF_PLAINTEXT_SYMBOL_1 = PositionsOfPlainTextSymbol1();
        private static readonly int[][] POSITIONS_OF_PLAINTEXT_SYMBOL_2 = PositionsOfPlainTextSymbol2();
        private static readonly int[][] POSITIONS_OF_CIPHERTEXT_SYMBOL_1 = PositionsOfCipherTextSymbol1();
        private static readonly int[][] POSITIONS_OF_CIPHERTEXT_SYMBOL_2 = PositionsOfCipherTextSymbol2();

        private static int Row(int position)
        {
            return position / DIM;
        }
        private static int Column(int position)
        {
            return position % DIM;
        }
        private static int Position(int row, int column)
        {
            if (row >= DIM)
            {
                row -= DIM;
            }
            else if (row < 0)
            {
                row += DIM;
            }
            if (column >= DIM)
            {
                column -= DIM;
            }
            else if (column < 0)
            {
                column += DIM;
            }
            return DIM * row + column;
        }
        private static int PositionOfPlainTextSymbol1(int cipherPositionOfSymbol1, int cipherPositionOfSymbol2)
        {
            int c1 = Column(cipherPositionOfSymbol1);
            int r1 = Row(cipherPositionOfSymbol1);
            int c2 = Column(cipherPositionOfSymbol2);
            int r2 = Row(cipherPositionOfSymbol2);

            if (r1 == r2)
            {
                return Position(r1, c1 - 1);
            }
            else if (c1 == c2)
            {
                return Position(r1 - 1, c1);
            }
            return Position(r1, c2);
        }

        private static int[][] PositionsOfPlainTextSymbol1()
        {
            int[][] positions = new int[SQUARE][];
            for (int p1 = 0; p1 < SQUARE; p1++)
            {
                positions[p1] = new int[SQUARE];
                for (int p2 = 0; p2 < SQUARE; p2++)
                {
                    positions[p1][p2] = PositionOfPlainTextSymbol1(p1, p2);
                }
            }
            return positions;
        }

        private static int PositionOfPlainTextSymbol2(int cipherPositionOfSymbol1, int cipherPositionOfSymbol2)
        {
            int c1 = Column(cipherPositionOfSymbol1);
            int r1 = Row(cipherPositionOfSymbol1);
            int c2 = Column(cipherPositionOfSymbol2);
            int r2 = Row(cipherPositionOfSymbol2);

            if (r1 == r2)
            {
                return Position(r2, c2 - 1);
            }
            else if (c1 == c2)
            {
                return Position(r2 - 1, c1);
            }
            return Position(r2, c1);
        }

        private static int[][] PositionsOfPlainTextSymbol2()
        {
            int[][] positions = new int[SQUARE][];
            for (int p1 = 0; p1 < SQUARE; p1++)
            {
                positions[p1] = new int[SQUARE];
                for (int p2 = 0; p2 < SQUARE; p2++)
                {
                    positions[p1][p2] = PositionOfPlainTextSymbol2(p1, p2);
                }
            }
            return positions;
        }

        private static int PositionOfCipherTextSymbol1(int plainTextPositionOfSymbol1, int plainTextPositionOfSymbol2)
        {
            int c1 = Column(plainTextPositionOfSymbol1);
            int r1 = Row(plainTextPositionOfSymbol1);
            int c2 = Column(plainTextPositionOfSymbol2);
            int r2 = Row(plainTextPositionOfSymbol2);

            if (r1 == r2)
            {
                return Position(r1, c1 + 1);
            }
            else if (c1 == c2)
            {
                return Position(r1 + 1, c1);
            }
            return Position(r1, c2);
        }

        private static int[][] PositionsOfCipherTextSymbol1()
        {
            int[][] positions = new int[SQUARE][];
            for (int p1 = 0; p1 < SQUARE; p1++)
            {
                positions[p1] = new int[SQUARE];
                for (int p2 = 0; p2 < SQUARE; p2++)
                {
                    positions[p1][p2] = PositionOfCipherTextSymbol1(p1, p2);
                }
            }
            return positions;
        }

        private static int PositionOfCipherTextSymbol2(int plainTextPositionOfSymbol1, int plainTextPositionOfSymbol2)
        {
            int c1 = Column(plainTextPositionOfSymbol1);
            int r1 = Row(plainTextPositionOfSymbol1);
            int c2 = Column(plainTextPositionOfSymbol2);
            int r2 = Row(plainTextPositionOfSymbol2);

            if (r1 == r2)
            {
                return Position(r2, c2 + 1);
            }
            else if (c1 == c2)
            {
                return Position(r2 + 1, c1);
            }
            return Position(r2, c1);
        }

        private static int[][] PositionsOfCipherTextSymbol2()
        {
            int[][] positions = new int[SQUARE][];
            for (int p1 = 0; p1 < SQUARE; p1++)
            {
                positions[p1] = new int[SQUARE];
                for (int p2 = 0; p2 < SQUARE; p2++)
                {
                    positions[p1][p2] = PositionOfCipherTextSymbol2(p1, p2);
                }
            }
            return positions;
        }
     
        public static int Decrypt(Key key, int[] cipherText, int[] plainText, int[] plainTextRemoveNulls)
        {
            key.ComputeInverse();

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

        public static int Encrypt(Key key, int[] plainText, int[] cipherText)
        {
            key.ComputeInverse();

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
    }
}
