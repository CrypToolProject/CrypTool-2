/*
   Copyright 2020 George Lasry
   Converted in 2020 from Java to C# by Nils Kopal

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
using EnigmaAnalyzerLib.Common;
using System;
using System.Text;

namespace EnigmaAnalyzerLib
{
    public class Key
    {

        public static int MAXLEN = 1800;
        public static int MAX_STB_PLUGS = 20;

        private static readonly short[] etw =
                   {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25,
                    0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25,
                    0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25};

        private static readonly short[] rev_etw =
                   {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25,
                    0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25,
                    0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25};

        /* Walzen 1-8, B and G (M4): forward path */
        private static readonly short[][] wal = {

            /* null substitution for no greek wheel */
            new short[]
                   {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25,
                    0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25,
                    0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25},

            new short[]
                   {4, 10, 12, 5, 11, 6, 3, 16, 21, 25, 13, 19, 14, 22, 24, 7, 23, 20, 18, 15, 0, 8, 1, 17, 2, 9,
                    4, 10, 12, 5, 11, 6, 3, 16, 21, 25, 13, 19, 14, 22, 24, 7, 23, 20, 18, 15, 0, 8, 1, 17, 2, 9,
                    4, 10, 12, 5, 11, 6, 3, 16, 21, 25, 13, 19, 14, 22, 24, 7, 23, 20, 18, 15, 0, 8, 1, 17, 2, 9},

            new short[]
                   {0, 9, 3, 10, 18, 8, 17, 20, 23, 1, 11, 7, 22, 19, 12, 2, 16, 6, 25, 13, 15, 24, 5, 21, 14, 4,
                    0, 9, 3, 10, 18, 8, 17, 20, 23, 1, 11, 7, 22, 19, 12, 2, 16, 6, 25, 13, 15, 24, 5, 21, 14, 4,
                    0, 9, 3, 10, 18, 8, 17, 20, 23, 1, 11, 7, 22, 19, 12, 2, 16, 6, 25, 13, 15, 24, 5, 21, 14, 4},

            new short[]
                   {1, 3, 5, 7, 9, 11, 2, 15, 17, 19, 23, 21, 25, 13, 24, 4, 8, 22, 6, 0, 10, 12, 20, 18, 16, 14,
                    1, 3, 5, 7, 9, 11, 2, 15, 17, 19, 23, 21, 25, 13, 24, 4, 8, 22, 6, 0, 10, 12, 20, 18, 16, 14,
                    1, 3, 5, 7, 9, 11, 2, 15, 17, 19, 23, 21, 25, 13, 24, 4, 8, 22, 6, 0, 10, 12, 20, 18, 16, 14},

            new short[]
                   {4, 18, 14, 21, 15, 25, 9, 0, 24, 16, 20, 8, 17, 7, 23, 11, 13, 5, 19, 6, 10, 3, 2, 12, 22, 1,
                    4, 18, 14, 21, 15, 25, 9, 0, 24, 16, 20, 8, 17, 7, 23, 11, 13, 5, 19, 6, 10, 3, 2, 12, 22, 1,
                    4, 18, 14, 21, 15, 25, 9, 0, 24, 16, 20, 8, 17, 7, 23, 11, 13, 5, 19, 6, 10, 3, 2, 12, 22, 1},

            new short[]
                   {21, 25, 1, 17, 6, 8, 19, 24, 20, 15, 18, 3, 13, 7, 11, 23, 0, 22, 12, 9, 16, 14, 5, 4, 2, 10,
                    21, 25, 1, 17, 6, 8, 19, 24, 20, 15, 18, 3, 13, 7, 11, 23, 0, 22, 12, 9, 16, 14, 5, 4, 2, 10,
                    21, 25, 1, 17, 6, 8, 19, 24, 20, 15, 18, 3, 13, 7, 11, 23, 0, 22, 12, 9, 16, 14, 5, 4, 2, 10},

            new short[]
                   {9, 15, 6, 21, 14, 20, 12, 5, 24, 16, 1, 4, 13, 7, 25, 17, 3, 10, 0, 18, 23, 11, 8, 2, 19, 22,
                    9, 15, 6, 21, 14, 20, 12, 5, 24, 16, 1, 4, 13, 7, 25, 17, 3, 10, 0, 18, 23, 11, 8, 2, 19, 22,
                    9, 15, 6, 21, 14, 20, 12, 5, 24, 16, 1, 4, 13, 7, 25, 17, 3, 10, 0, 18, 23, 11, 8, 2, 19, 22},

            new short[]
                   {13, 25, 9, 7, 6, 17, 2, 23, 12, 24, 18, 22, 1, 14, 20, 5, 0, 8, 21, 11, 15, 4, 10, 16, 3, 19,
                    13, 25, 9, 7, 6, 17, 2, 23, 12, 24, 18, 22, 1, 14, 20, 5, 0, 8, 21, 11, 15, 4, 10, 16, 3, 19,
                    13, 25, 9, 7, 6, 17, 2, 23, 12, 24, 18, 22, 1, 14, 20, 5, 0, 8, 21, 11, 15, 4, 10, 16, 3, 19},

            new short[]
                   {5, 10, 16, 7, 19, 11, 23, 14, 2, 1, 9, 18, 15, 3, 25, 17, 0, 12, 4, 22, 13, 8, 20, 24, 6, 21,
                    5, 10, 16, 7, 19, 11, 23, 14, 2, 1, 9, 18, 15, 3, 25, 17, 0, 12, 4, 22, 13, 8, 20, 24, 6, 21,
                    5, 10, 16, 7, 19, 11, 23, 14, 2, 1, 9, 18, 15, 3, 25, 17, 0, 12, 4, 22, 13, 8, 20, 24, 6, 21},

            new short[]
                   {11, 4, 24, 9, 21, 2, 13, 8, 23, 22, 15, 1, 16, 12, 3, 17, 19, 0, 10, 25, 6, 5, 20, 7, 14, 18,
                    11, 4, 24, 9, 21, 2, 13, 8, 23, 22, 15, 1, 16, 12, 3, 17, 19, 0, 10, 25, 6, 5, 20, 7, 14, 18,
                    11, 4, 24, 9, 21, 2, 13, 8, 23, 22, 15, 1, 16, 12, 3, 17, 19, 0, 10, 25, 6, 5, 20, 7, 14, 18},

            new short[]
                   {5, 18, 14, 10, 0, 13, 20, 4, 17, 7, 12, 1, 19, 8, 24, 2, 22, 11, 16, 15, 25, 23, 21, 6, 9, 3,
                    5, 18, 14, 10, 0, 13, 20, 4, 17, 7, 12, 1, 19, 8, 24, 2, 22, 11, 16, 15, 25, 23, 21, 6, 9, 3,
                    5, 18, 14, 10, 0, 13, 20, 4, 17, 7, 12, 1, 19, 8, 24, 2, 22, 11, 16, 15, 25, 23, 21, 6, 9, 3}

        };
        /* Umkehrwalzen A, B, C, B_THIN, C_THIN */
        private static readonly short[][] ukw = {

            //   EJMZALYXVBWFCRQUONTSPIKHGD
            ////////4, 9, 12, 25, 0, 11, 24, 23, 21, 1, 22, 5, 2, 17, 16, 20, 14, 13, 19, 18, 15, 8, 10, 7, 6, 3,
            new short[]
                   {4, 9, 12, 25, 0, 11, 24, 23, 21, 1, 22, 5, 2, 17, 16, 20, 14, 13, 19, 18, 15, 8, 10, 7, 6, 3,
                    4, 9, 12, 25, 0, 11, 24, 23, 21, 1, 22, 5, 2, 17, 16, 20, 14, 13, 19, 18, 15, 8, 10, 7, 6, 3},

            new short[]
                   {24, 17, 20, 7, 16, 18, 11, 3, 15, 23, 13, 6, 14, 10, 12, 8, 4, 1, 5, 25, 2, 22, 21, 9, 0, 19,
                    24, 17, 20, 7, 16, 18, 11, 3, 15, 23, 13, 6, 14, 10, 12, 8, 4, 1, 5, 25, 2, 22, 21, 9, 0, 19},

            new short[]
                   {5, 21, 15, 9, 8, 0, 14, 24, 4, 3, 17, 25, 23, 22, 6, 2, 19, 10, 20, 16, 18, 1, 13, 12, 7, 11,
                    5, 21, 15, 9, 8, 0, 14, 24, 4, 3, 17, 25, 23, 22, 6, 2, 19, 10, 20, 16, 18, 1, 13, 12, 7, 11},

            new short[]
                   {4, 13, 10, 16, 0, 20, 24, 22, 9, 8, 2, 14, 15, 1, 11, 12, 3, 23, 25, 21, 5, 19, 7, 17, 6, 18,
                    4, 13, 10, 16, 0, 20, 24, 22, 9, 8, 2, 14, 15, 1, 11, 12, 3, 23, 25, 21, 5, 19, 7, 17, 6, 18},

            new short[]
                   {17, 3, 14, 1, 9, 13, 19, 10, 21, 4, 7, 12, 11, 5, 2, 22, 25, 0, 23, 6, 24, 8, 15, 18, 20, 16,
                    17, 3, 14, 1, 9, 13, 19, 10, 21, 4, 7, 12, 11, 5, 2, 22, 25, 0, 23, 6, 24, 8, 15, 18, 20, 16}

        };

        /* Walzen 1-8, B and G (M4): reverse path */
        private static readonly short[][] rev_wal = {

            /* null substitution for no greek wheel */
            new short[]
                   {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25,
                    0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25,
                    0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25},

            new short[]
                   {20, 22, 24, 6, 0, 3, 5, 15, 21, 25, 1, 4, 2, 10, 12, 19, 7, 23, 18, 11, 17, 8, 13, 16, 14, 9,
                    20, 22, 24, 6, 0, 3, 5, 15, 21, 25, 1, 4, 2, 10, 12, 19, 7, 23, 18, 11, 17, 8, 13, 16, 14, 9,
                    20, 22, 24, 6, 0, 3, 5, 15, 21, 25, 1, 4, 2, 10, 12, 19, 7, 23, 18, 11, 17, 8, 13, 16, 14, 9},

            new short[]
                   {0, 9, 15, 2, 25, 22, 17, 11, 5, 1, 3, 10, 14, 19, 24, 20, 16, 6, 4, 13, 7, 23, 12, 8, 21, 18,
                    0, 9, 15, 2, 25, 22, 17, 11, 5, 1, 3, 10, 14, 19, 24, 20, 16, 6, 4, 13, 7, 23, 12, 8, 21, 18,
                    0, 9, 15, 2, 25, 22, 17, 11, 5, 1, 3, 10, 14, 19, 24, 20, 16, 6, 4, 13, 7, 23, 12, 8, 21, 18},

            new short[]
                   {19, 0, 6, 1, 15, 2, 18, 3, 16, 4, 20, 5, 21, 13, 25, 7, 24, 8, 23, 9, 22, 11, 17, 10, 14, 12,
                    19, 0, 6, 1, 15, 2, 18, 3, 16, 4, 20, 5, 21, 13, 25, 7, 24, 8, 23, 9, 22, 11, 17, 10, 14, 12,
                    19, 0, 6, 1, 15, 2, 18, 3, 16, 4, 20, 5, 21, 13, 25, 7, 24, 8, 23, 9, 22, 11, 17, 10, 14, 12},

            new short[]
                   {7, 25, 22, 21, 0, 17, 19, 13, 11, 6, 20, 15, 23, 16, 2, 4, 9, 12, 1, 18, 10, 3, 24, 14, 8, 5,
                    7, 25, 22, 21, 0, 17, 19, 13, 11, 6, 20, 15, 23, 16, 2, 4, 9, 12, 1, 18, 10, 3, 24, 14, 8, 5,
                    7, 25, 22, 21, 0, 17, 19, 13, 11, 6, 20, 15, 23, 16, 2, 4, 9, 12, 1, 18, 10, 3, 24, 14, 8, 5},

            new short[]
                   {16, 2, 24, 11, 23, 22, 4, 13, 5, 19, 25, 14, 18, 12, 21, 9, 20, 3, 10, 6, 8, 0, 17, 15, 7, 1,
                    16, 2, 24, 11, 23, 22, 4, 13, 5, 19, 25, 14, 18, 12, 21, 9, 20, 3, 10, 6, 8, 0, 17, 15, 7, 1,
                    16, 2, 24, 11, 23, 22, 4, 13, 5, 19, 25, 14, 18, 12, 21, 9, 20, 3, 10, 6, 8, 0, 17, 15, 7, 1},

            new short[]
                   {18, 10, 23, 16, 11, 7, 2, 13, 22, 0, 17, 21, 6, 12, 4, 1, 9, 15, 19, 24, 5, 3, 25, 20, 8, 14,
                    18, 10, 23, 16, 11, 7, 2, 13, 22, 0, 17, 21, 6, 12, 4, 1, 9, 15, 19, 24, 5, 3, 25, 20, 8, 14,
                    18, 10, 23, 16, 11, 7, 2, 13, 22, 0, 17, 21, 6, 12, 4, 1, 9, 15, 19, 24, 5, 3, 25, 20, 8, 14},

            new short[]
                   {16, 12, 6, 24, 21, 15, 4, 3, 17, 2, 22, 19, 8, 0, 13, 20, 23, 5, 10, 25, 14, 18, 11, 7, 9, 1,
                    16, 12, 6, 24, 21, 15, 4, 3, 17, 2, 22, 19, 8, 0, 13, 20, 23, 5, 10, 25, 14, 18, 11, 7, 9, 1,
                    16, 12, 6, 24, 21, 15, 4, 3, 17, 2, 22, 19, 8, 0, 13, 20, 23, 5, 10, 25, 14, 18, 11, 7, 9, 1},

            new short[]
                   {16, 9, 8, 13, 18, 0, 24, 3, 21, 10, 1, 5, 17, 20, 7, 12, 2, 15, 11, 4, 22, 25, 19, 6, 23, 14,
                    16, 9, 8, 13, 18, 0, 24, 3, 21, 10, 1, 5, 17, 20, 7, 12, 2, 15, 11, 4, 22, 25, 19, 6, 23, 14,
                    16, 9, 8, 13, 18, 0, 24, 3, 21, 10, 1, 5, 17, 20, 7, 12, 2, 15, 11, 4, 22, 25, 19, 6, 23, 14},

            new short[]
                   {17, 11, 5, 14, 1, 21, 20, 23, 7, 3, 18, 0, 13, 6, 24, 10, 12, 15, 25, 16, 22, 4, 9, 8, 2, 19,
                    17, 11, 5, 14, 1, 21, 20, 23, 7, 3, 18, 0, 13, 6, 24, 10, 12, 15, 25, 16, 22, 4, 9, 8, 2, 19,
                    17, 11, 5, 14, 1, 21, 20, 23, 7, 3, 18, 0, 13, 6, 24, 10, 12, 15, 25, 16, 22, 4, 9, 8, 2, 19},

            new short[]
                   {4, 11, 15, 25, 7, 0, 23, 9, 13, 24, 3, 17, 10, 5, 2, 19, 18, 8, 1, 12, 6, 22, 16, 21, 14, 20,
                    4, 11, 15, 25, 7, 0, 23, 9, 13, 24, 3, 17, 10, 5, 2, 19, 18, 8, 1, 12, 6, 22, 16, 21, 14, 20,
                    4, 11, 15, 25, 7, 0, 23, 9, 13, 24, 3, 17, 10, 5, 2, 19, 18, 8, 1, 12, 6, 22, 16, 21, 14, 20}

        };

        //private  static short walocalLturn[] = {0, 16, 4, 21, 9, 25, 12, 12, 12};

        private static readonly short[][] wal_turn =
        {
            new short[]{-1, -1, -1, -1, -1},
            new short[]{16, -1, -1, -1, -1},
            new short[]{4, -1, -1, -1, -1},
            new short[]{21, -1, -1, -1, -1},
            new short[]{9, -1, -1, -1, -1},
            new short[]{25, -1, -1, -1, -1},
            new short[]{12, 25, -1, -1, -1},
            new short[]{12, 25, -1, -1, -1},
            new short[]{12, 25, -1, -1, -1}
        };

        private static void initArray(string val, short[] array, int len, int repetition, bool inverse)
        {
            for (short i = 0; i < len; i++)
            {
                short c = i < val.Length ? EnigmaUtils.getIndex(val[i]) : (short)-1;
                if (inverse)
                {
                    array[c] = i;
                    if (repetition > 1)
                    {
                        array[len + c] = i;
                    }
                    if (repetition > 2)
                    {
                        array[len * 2 + c] = i;
                    }

                }
                else
                {
                    array[i] = c;
                    if (repetition > 1)
                    {
                        array[len + i] = c;
                    }
                    if (repetition > 2)
                    {
                        array[len * 2 + i] = c;
                    }
                }
            }
        }

        private static bool[] getTurnoverPoints(int slot, int ring)
        {
            bool[] turnovers = new bool[26];
            for (int i = 0; i < 5; i++)
            {
                if (wal_turn[slot][i] != -1)
                {
                    int turn = (26 + wal_turn[slot][i] - ring) % 26;
                    turnovers[turn] = true;
                }
                else
                {
                    break;
                }
            }
            return turnovers;
        }

        public static void initDatabase(Model model)
        {
            initArray("ABCDEFGHIJKLMNOPQRSTUVWXYZ", etw, 26, 3, false);
            initArray("ABCDEFGHIJKLMNOPQRSTUVWXYZ", rev_etw, 26, 3, true);

            initArray("ABCDEFGHIJKLMNOPQRSTUVWXYZ", wal[0], 26, 3, false);
            initArray("EKMFLGDQVZNTOWYHXUSPAIBRCJ", wal[1], 26, 3, false);
            initArray("AJDKSIRUXBLHWTMCQGZNPYFVOE", wal[2], 26, 3, false);
            initArray("BDFHJLCPRTXVZNYEIWGAKMUSQO", wal[3], 26, 3, false);
            initArray("ESOVPZJAYQUIRHXLNFTGKDCMWB", wal[4], 26, 3, false);
            initArray("VZBRGITYUPSDNHLXAWMJQOFECK", wal[5], 26, 3, false);
            initArray("JPGVOUMFYQBENHZRDKASXLICTW", wal[6], 26, 3, false);
            initArray("NZJHGRCXMYSWBOUFAIVLPEKQDT", wal[7], 26, 3, false);
            initArray("FKQHTLXOCBJSPDZRAMEWNIUYGV", wal[8], 26, 3, false);
            initArray("LEYJVCNIXWPBQMDRTAKZGFUHOS", wal[9], 26, 3, false);
            initArray("FSOKANUERHMBTIYCWLQPZXVGJD", wal[10], 26, 3, false);

            initArray("ABCDEFGHIJKLMNOPQRSTUVWXYZ", rev_wal[0], 26, 3, true);
            initArray("EKMFLGDQVZNTOWYHXUSPAIBRCJ", rev_wal[1], 26, 3, true);
            initArray("AJDKSIRUXBLHWTMCQGZNPYFVOE", rev_wal[2], 26, 3, true);
            initArray("BDFHJLCPRTXVZNYEIWGAKMUSQO", rev_wal[3], 26, 3, true);
            initArray("ESOVPZJAYQUIRHXLNFTGKDCMWB", rev_wal[4], 26, 3, true);
            initArray("VZBRGITYUPSDNHLXAWMJQOFECK", rev_wal[5], 26, 3, true);
            initArray("JPGVOUMFYQBENHZRDKASXLICTW", rev_wal[6], 26, 3, true);
            initArray("NZJHGRCXMYSWBOUFAIVLPEKQDT", rev_wal[7], 26, 3, true);
            initArray("FKQHTLXOCBJSPDZRAMEWNIUYGV", rev_wal[8], 26, 3, true);
            initArray("LEYJVCNIXWPBQMDRTAKZGFUHOS", rev_wal[9], 26, 3, true);
            initArray("FSOKANUERHMBTIYCWLQPZXVGJD", rev_wal[10], 26, 3, true);

            initArray("EJMZALYXVBWFCRQUONTSPIKHGD", ukw[0], 26, 2, false);
            initArray("YRUHQSLDPXNGOKMIEBFZCWVJAT", ukw[1], 26, 2, false);
            initArray("FVPJIAOYEDRZXWGCTKUQSBNMHL", ukw[2], 26, 2, false);
            initArray("ENKQAUYWJICOPBLMDXZVFTHRGS", ukw[3], 26, 2, false);
            initArray("RDOBJNTKVEHMLFCWZAXGYIPSUQ", ukw[4], 26, 2, false);

            initArray("", wal_turn[0], 5, 1, false);
            initArray("Q", wal_turn[1], 5, 1, false);
            initArray("E", wal_turn[2], 5, 1, false);
            initArray("V", wal_turn[3], 5, 1, false);
            initArray("J", wal_turn[4], 5, 1, false);
            initArray("Z", wal_turn[5], 5, 1, false);
            initArray("MZ", wal_turn[6], 5, 1, false);
            initArray("MZ", wal_turn[7], 5, 1, false);
            initArray("MZ", wal_turn[8], 5, 1, false);


            /*
            OCHQZMJPFIWEXTYLGVBKDNURAS Y Q
            II HWBEOSZFQMTXRKIGVJYPUCLNAD M E
            III XTKFJRMLGYVQWUBIEHANPDSOZC D V
            IV IGTCNQWJMHXFEZVSYBLPROKDAU R J
            V NXEKUZMQLVCTIRJYHSDGAFBPWO H Z
            UKW YIOGTXDKBSHVWRCQPNJEZLMFAU
            ETW ABCDEFGHIJKLMNOPQRSTUVWXYZ
            Figure 19. Wheel wiring for A 16101, A 17314S, A 17315S and A 17316S.
            */

            if (model == Model.A16101)
            {

                initArray("OCHQZMJPFIWEXTYLGVBKDNURAS", wal[1], 26, 3, false);
                initArray("HWBEOSZFQMTXRKIGVJYPUCLNAD", wal[2], 26, 3, false);
                initArray("XTKFJRMLGYVQWUBIEHANPDSOZC", wal[3], 26, 3, false);
                initArray("IGTCNQWJMHXFEZVSYBLPROKDAU", wal[4], 26, 3, false);
                initArray("NXEKUZMQLVCTIRJYHSDGAFBPWO", wal[5], 26, 3, false);

                initArray("OCHQZMJPFIWEXTYLGVBKDNURAS", rev_wal[1], 26, 3, true);
                initArray("HWBEOSZFQMTXRKIGVJYPUCLNAD", rev_wal[2], 26, 3, true);
                initArray("XTKFJRMLGYVQWUBIEHANPDSOZC", rev_wal[3], 26, 3, true);
                initArray("IGTCNQWJMHXFEZVSYBLPROKDAU", rev_wal[4], 26, 3, true);
                initArray("NXEKUZMQLVCTIRJYHSDGAFBPWO", rev_wal[5], 26, 3, true);

                initArray("YIOGTXDKBSHVWRCQPNJEZLMFAU", ukw[0], 26, 2, false);
                initArray("YIOGTXDKBSHVWRCQPNJEZLMFAU", ukw[1], 26, 2, false);
                initArray("YIOGTXDKBSHVWRCQPNJEZLMFAU", ukw[2], 26, 2, false);
                initArray("YIOGTXDKBSHVWRCQPNJEZLMFAU", ukw[3], 26, 2, false);
                initArray("YIOGTXDKBSHVWRCQPNJEZLMFAU", ukw[4], 26, 2, false);


            }
            else if (model == Model.A16081)
            {
                /*
                CVFWJOBXANQTDZUMEYRPSKGILH Y Q
                II XJGURHZMYDLATWKSEPNCQFOIBV M E
                III SYIGXELDUKBVOAWTZHQNFCRMJP D V
                IV HKTZDSRFWPCQJIYXNVMUGELAOB R J
                V WMGRKEJUAZFTOXINDYBQVHLCPS H Z
                UKW DONAJUXTQELKSCBZIVMHFRYGWP
                         */
                initArray("CVFWJOBXANQTDZUMEYRPSKGILH", wal[1], 26, 3, false);
                initArray("XJGURHZMYDLATWKSEPNCQFOIBV", wal[2], 26, 3, false);
                initArray("SYIGXELDUKBVOAWTZHQNFCRMJP", wal[3], 26, 3, false);
                initArray("HKTZDSRFWPCQJIYXNVMUGELAOB", wal[4], 26, 3, false);
                initArray("WMGRKEJUAZFTOXINDYBQVHLCPS", wal[5], 26, 3, false);

                initArray("CVFWJOBXANQTDZUMEYRPSKGILH", rev_wal[1], 26, 3, true);
                initArray("XJGURHZMYDLATWKSEPNCQFOIBV", rev_wal[2], 26, 3, true);
                initArray("SYIGXELDUKBVOAWTZHQNFCRMJP", rev_wal[3], 26, 3, true);
                initArray("HKTZDSRFWPCQJIYXNVMUGELAOB", rev_wal[4], 26, 3, true);
                initArray("WMGRKEJUAZFTOXINDYBQVHLCPS", rev_wal[5], 26, 3, true);

                initArray("DONAJUXTQELKSCBZIVMHFRYGWP", ukw[0], 26, 2, false);
                initArray("DONAJUXTQELKSCBZIVMHFRYGWP", ukw[1], 26, 2, false);
                initArray("DONAJUXTQELKSCBZIVMHFRYGWP", ukw[2], 26, 2, false);
                initArray("DONAJUXTQELKSCBZIVMHFRYGWP", ukw[3], 26, 2, false);
                initArray("DONAJUXTQELKSCBZIVMHFRYGWP", ukw[4], 26, 2, false);
            }
        }

        /* Turnover points:  Walzen 1-5, Walzen 6-8 (/first/ turnover points) */
        private static readonly Random random = new Random();
        public int[] stbrett = new int[26];
        public int _stbCount;      /* number of swapped letters */
        public int[] sf = new int[26];     /* swapped/free letters */
        public Model model;
        public int ukwNum;
        public int gSlot;
        public int lSlot;     /* greek, left, middle, right slot */
        public int mSlot;
        public int rSlot;
        public int gRing;
        public int lRing;     /* ringstellungen */
        public int mRing;
        public int rRing;
        public int gMesg;
        public int lMesg;     /* message settings */
        public int mMesg;
        public int rMesg;
        public int score;      /* HillClimbing score */
        public short[] lookup;
        private static int counter = 0;
        //Warning - this will not work if several threads use icscore
        private readonly int[] f = new int[26];

        public Key()
        {

        }

        public Key(Key copyKey)
        {
            clone(copyKey);
        }

        // static public utilites
        public static bool CheckValidWheelsState(int len, int mRingSteppingPos, MRingScope mRingScope)
        {
            bool valid = false;
            switch (mRingScope)
            {
                case MRingScope.ALL:
                    valid = true;
                    break;
                case MRingScope.ONE_NON_STEPPING:
                    if (mRingSteppingPos >= len)
                    {
                        valid = true;
                    }
                    break;
                case MRingScope.ALL_STEPPING_INSIDE_MSG:
                    if ((mRingSteppingPos >= 0) && (mRingSteppingPos < len))
                    {
                        valid = true;
                    }
                    break;
                case MRingScope.ALL_NON_STEPPING:
                    if (mRingSteppingPos == -1)
                    {
                        valid = true;
                    }
                    break;
                case MRingScope.ALL_STEPPING_INSIDE_MSG_AND_ONE_NON_STEPPING:
                    if (mRingSteppingPos >= 0)
                    {
                        valid = true;
                    }
                    break;
                case MRingScope.STEPPING_INSIDE_MSG_WITH_SMALL_IMPACT_AND_ONE_NON_STEPPING:
                    if ((mRingSteppingPos >= 4 * len / 5) || ((mRingSteppingPos >= 0) && (mRingSteppingPos < len / 5)))
                    {
                        valid = true;
                    }
                    break;
                default:

                    break;
            }
            return valid;
        }

        public static int setRange(Key from, Key to, string kf, string kt, Model model)
        {

            if (from.setKey(kf, model, true) != 1)
            {
                return 0;
            }
            if (to.setKey(kt, model, true) != 1)
            {
                return 0;
            }
            if (compare(from, to) == 1)
            {
                return 0;
            }
            return 1;
        }

        public static long numberOfPossibleKeys(Key lo, Key high, int len,
                                                MRingScope mRingScope, int rRingSpacing, bool checkIndicator)
        {
            long numberOfKeysToCheck = (high.ukwNum - lo.ukwNum + 1) * (high.gSlot - lo.gSlot + 1) *
                    (high.gRing - lo.gRing + 1) * (high.lRing - lo.lRing + 1);

            if (!checkIndicator)
            {
                numberOfKeysToCheck *= (high.gMesg - lo.gMesg + 1) * (high.lMesg - lo.lMesg + 1) *
                        (high.mMesg - lo.mMesg + 1) * (high.rMesg - lo.rMesg + 1);
            }

            int rRingOptions = (high.rRing - lo.rRing + 1);
            if (rRingOptions == 26)
            {
                rRingOptions = (26 + rRingSpacing - 1) / rRingSpacing;
            }

            numberOfKeysToCheck *= rRingOptions;

            int mRingOptions = 1;
            switch (mRingScope)
            {
                case MRingScope.ALL:
                    mRingOptions = high.mRing - lo.mRing + 1;
                    break;
                case MRingScope.ONE_NON_STEPPING:
                    mRingOptions = 1;
                    break;
                case MRingScope.ALL_STEPPING_INSIDE_MSG:
                    mRingOptions = Math.Min(Math.Max(len / 26, 1), 26);
                    break;
                case MRingScope.ALL_NON_STEPPING:
                    mRingOptions = Math.Max(26 - Math.Min(len / 26, 26), 1);
                    break;
                case MRingScope.ALL_STEPPING_INSIDE_MSG_AND_ONE_NON_STEPPING:
                    mRingOptions = Math.Min(len / 26 + 1, 26);
                    break;
                case MRingScope.STEPPING_INSIDE_MSG_WITH_SMALL_IMPACT_AND_ONE_NON_STEPPING:
                    mRingOptions = Math.Min((3 * len / 5) / 26 + 1, 26);
                    break;
                default:
                    break;
            }

            if (high.mRing == lo.mRing)
            {
                mRingOptions = 1;
            }

            numberOfKeysToCheck *= mRingOptions;

            long wheelPossibilities = 0;
            Key ckey = new Key();
            for (ckey.lSlot = lo.lSlot; ckey.lSlot <= high.lSlot; ckey.lSlot++)
            {
                for (ckey.mSlot = lo.mSlot; ckey.mSlot <= high.mSlot; ckey.mSlot++)
                {
                    if (ckey.mSlot == ckey.lSlot)
                    {
                        continue;
                    }
                    for (ckey.rSlot = lo.rSlot; ckey.rSlot <= high.rSlot; ckey.rSlot++)
                    {
                        if (ckey.rSlot == ckey.lSlot || ckey.rSlot == ckey.mSlot)
                        {
                            continue;
                        }

                        wheelPossibilities++;
                    }
                }
            }
            if (wheelPossibilities == 0)
            {
                wheelPossibilities = 1;
            }

            numberOfKeysToCheck *= wheelPossibilities;
            return numberOfKeysToCheck;
        }

        private static int compare(Key k1, Key k2)
        {
            /* compares ukwnum thru rMesg, omits gRing, lRing        */
            /* returns -1 for k1 < k2, 0 for k1 == k2, 1 for k1 > k2    */
            if (k1.ukwNum != k2.ukwNum)
            {
                if (k1.ukwNum > k2.ukwNum)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            if (k1.gSlot != k2.gSlot)
            {
                if (k1.gSlot > k2.gSlot)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            if (k1.lSlot != k2.lSlot)
            {
                if (k1.lSlot > k2.lSlot)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            if (k1.mSlot != k2.mSlot)
            {
                if (k1.mSlot > k2.mSlot)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            if (k1.rSlot != k2.rSlot)
            {
                if (k1.rSlot > k2.rSlot)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            if (k1.mRing != k2.mRing)
            {
                if (k1.mRing > k2.mRing)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            if (k1.rRing != k2.rRing)
            {
                if (k1.rRing > k2.rRing)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            if (k1.gMesg != k2.gMesg)
            {
                if (k1.gMesg > k2.gMesg)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            if (k1.lMesg != k2.lMesg)
            {
                if (k1.lMesg > k2.lMesg)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            if (k1.mMesg != k2.mMesg)
            {
                if (k1.mMesg > k2.mMesg)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            if (k1.rMesg != k2.rMesg)
            {
                if (k1.rMesg > k2.rMesg)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            return 0;
        }

        public static void randVar(int[] var)
        {
            int count;
            int store;
            int i;

            for (count = 25; count > 0; count--)
            {
                i = random.Next(10000) % (count + 1);
                store = var[count];
                var[count] = var[i];
                var[i] = store;
            }
        }

        public static void setToCtFreq(int[] var, short[] ciphertext, int len)
        {
            /* arrange var[] in order of frequency of letters in ciphertext */
            int[] f = new int[26];
            int i, k, c;
            int max, pos = -1;
            int n = 0;

            for (i = 0; i < len; i++)
            {
                c = ciphertext[i];
                f[c]++;
            }

            for (i = 0; i < 26; i++)
            {
                max = 0;
                for (k = 0; k < 26; k++)
                {
                    if (f[k] >= max)
                    {
                        max = f[k];
                        pos = k;
                    }
                }
                if (pos == -1)
                {
                    return;
                }
                f[pos] = -1;
                var[n++] = pos;
            }

        }

        public static Model getModelFromstring(string s)
        {
            if (s.Equals("H") || s.Equals("h"))
            {
                return Model.H;
            }
            if (s.Equals("M3") || s.Equals("m3"))
            {
                return Model.M3;
            }
            if (s.Equals("M4") || s.Equals("m4"))
            {
                return Model.M4;
            }
            return Model.NONE;
        }

        public static bool buildCycles(int[][] links, int[][] keyCycleSizes, bool print)
        {
            for (int pos = 0; pos < 3; pos++)
            {
                string cycles = "";

                if (print)
                {
                    string linksS = "";
                    for (int l = 0; l < 26; l++)
                    {
                        if (links[pos][l] != -1)
                        {
                            linksS += EnigmaUtils.getChar(links[pos][l]);
                        }
                        else
                        {
                            linksS += '?';
                        }
                    }

                    Console.WriteLine("Cycle Match Search: Links for position {0}: ", pos);

                    for (int l = 0; l < 26; l++)
                    {
                        Console.WriteLine("{0}", EnigmaUtils.getChar(l));
                    }
                    Console.WriteLine("{0}", linksS);

                }
                // if only one missing autocorrect it
                bool[] used = new bool[26];
                int missing = -1;
                int missingCount = 0;
                for (int l = 0; l < 26; l++)
                {
                    if (links[pos][l] != -1)
                    {
                        used[links[pos][l]] = true;
                    }
                    else
                    {
                        missing = l;
                        missingCount++;
                    }
                }

                if (missingCount > 2)
                {
                    Console.WriteLine("Cycle Match Search: Too many missing links ({0}) for pos {1} - cannot autocorrect", missingCount, pos);
                    return false;
                }
                else if (missingCount == 1)
                {

                    int free = -1;
                    for (int l = 0; l < 26; l++)
                    {
                        if (!used[l])
                        {
                            free = l;
                            break;
                        }
                    }
                    links[pos][missing] = free;
                    Console.WriteLine("Cycle Match Search: Broken cycle for letter: [{0}] in pos {1} has been auto-corrected with [{2}]",
                            EnigmaUtils.getChar(missing), pos, EnigmaUtils.getChar(free));
                }
                int startLetter = 0;
                while (startLetter != -1)
                {
                    int cycleSize = 0;
                    int letter = startLetter;
                    if (print)
                    {
                        cycles += "(";
                    }
                    do
                    {
                        if (print)
                        {
                            cycles += EnigmaUtils.getChar(letter);
                        }
                        cycleSize++;
                        int nextLetter = links[pos][letter];
                        if (nextLetter == -1)
                        {
                            Console.WriteLine("BROKEN CYCLE for Letter: [{0}] in pos {1}", EnigmaUtils.getChar(letter), pos);
                            return false;
                        }
                        else
                        {
                            links[pos][letter] = -1;
                            letter = nextLetter;
                        }
                    } while (letter != startLetter);

                    if (print)
                    {
                        cycles += "[" + cycleSize + "])";
                    }

                    keyCycleSizes[pos][cycleSize]++;

                    startLetter = -1;
                    for (int l = 0; l < 26; l++)
                    {
                        if (links[pos][l] != -1)
                        {
                            startLetter = l;
                            break;
                        }
                    }
                }

                if (print)
                {
                    string cyclesS = "";
                    for (int i = 25; i >= 0; i--)
                    {
                        if (keyCycleSizes[pos][i] > 0)
                        {
                            cyclesS += keyCycleSizes[pos][i] + "x" + i + ",";
                        }
                    }
                    Console.WriteLine("Cycle Match Search: Cycles for pos {0}: {1}", pos, cyclesS);
                    Console.WriteLine("Detailed cycled for pos {0}: {1}", pos, cycles);
                }
                /*
                    string cyclesS = "";
                    for (int i = 25; i >=0; i--)
                    {
                            if (keyCycleSizes[pos][i] > 0)
                            {
                                cyclesS += keyCycleSizes[pos][i]+"x"+i+",";
                            }
                    }
                    while (cyclesS.Length< 10)
                    {
                        cyclesS+=" ";
                    }
                    Console.WriteLine("\t{0}=>{1}: {2}", pos+1,pos+4, cyclesS);
                */
            }
            return true;
        }

        public static void checkStecker(short[] stb, string desc)
        {
            for (int i = 0; i < 26; i++)
            {
                if (stb[i] == -1)
                {
                    continue;
                }
                if (stb[i] > i)
                {
                    if (stb[stb[i]] != i)
                    {
                        string stbS = string.Empty;
                        for (int k = 0; k < 26; k++)
                        {
                            if (stb[k] == -1)
                            {
                                stbS += "?";
                            }
                            else
                            {
                                stbS += "" + EnigmaUtils.getChar(stb[k]);
                            }

                            Console.WriteLine("Invalid Stecker - {0} - I = {1} [{2}] Stb[i] = {3} [{4}], stb[stb[i]] = {5} [{6}]  Stb Full {7}",
                                    desc,
                                    i, EnigmaUtils.getChar(i),
                                    stb[i], EnigmaUtils.getChar(stb[i]),
                                    stb[stb[i]], EnigmaUtils.getChar(stb[stb[i]]),
                                    stbS);
                            break;
                        }
                    }
                }
            }
        }

        public void clone(Key copyKey)
        {
            model = copyKey.model;
            ukwNum = copyKey.ukwNum;
            gSlot = copyKey.gSlot;
            lSlot = copyKey.lSlot;     /* greek, left, middle, right slot */
            mSlot = copyKey.mSlot;
            rSlot = copyKey.rSlot;
            gRing = copyKey.gRing;
            lRing = copyKey.lRing;     /* ringstellungen */
            mRing = copyKey.mRing;
            rRing = copyKey.rRing;
            gMesg = copyKey.gMesg;
            lMesg = copyKey.lMesg;     /* message settings */
            mMesg = copyKey.mMesg;
            rMesg = copyKey.rMesg;
            Array.Copy(copyKey.stbrett, 0, stbrett, 0, 26);
            Array.Copy(copyKey.sf, 0, sf, 0, 26);

            _stbCount = copyKey._stbCount;      /* number of swapped letters */
            score = copyKey.score;

            // path_lookup is not copied!
        }

        private int getRandomInRange(int from, int to)
        {
            if (from >= to)
            {
                return from;
            }
            int range = to - from;

            int r = random.Next(range + 1);

            return from + r;
        }

        public int initDefaults(Model model)
        {
            this.model = model;
            lSlot = 1;
            mSlot = 2;
            rSlot = 3;
            gRing = lRing = mRing = rRing = 0;
            gMesg = lMesg = rMesg = 0;
            _stbCount = 0;
            score = 0;

            switch (model)
            {
                case Model.H:
                    ukwNum = 1;
                    gSlot = 0;
                    break;
                case Model.M3:
                    ukwNum = 1;
                    gSlot = 0;
                    break;
                case Model.M4:
                    ukwNum = 3;
                    gSlot = 9;
                    break;
                default:
                    return 0;

            }

            for (int i = 0; i < 26; i++)
            {
                stbrett[i] = sf[i] = i;
            }

            return 1;
        }

        public int initRandom(Key from, Key to, int stbPlugs)
        {

            if (stbPlugs > 13)
            {
                stbPlugs = 13;
            }

            model = from.model;
            gSlot = getRandomInRange(from.gSlot, to.gSlot);

            lSlot = getRandomInRange(from.lSlot, to.lSlot);
            do
            {
                mSlot = getRandomInRange(from.mSlot, to.mSlot);
            } while (mSlot == lSlot);

            do
            {
                rSlot = getRandomInRange(from.rSlot, to.rSlot);
            } while (rSlot == lSlot || rSlot == mSlot);

            gRing = getRandomInRange(from.gRing, to.gRing);
            lRing = getRandomInRange(from.lRing, to.lRing);
            mRing = getRandomInRange(from.mRing, to.mRing);
            rRing = getRandomInRange(from.rRing, to.rRing);

            gMesg = getRandomInRange(from.gMesg, to.gMesg);
            lMesg = getRandomInRange(from.lMesg, to.lMesg);
            mMesg = getRandomInRange(from.mMesg, to.mMesg);
            rMesg = getRandomInRange(from.rMesg, to.rMesg);

            ukwNum = getRandomInRange(from.ukwNum, to.ukwNum);

            return setRandomStb(stbPlugs);
        }

        public int setRandomStb(int stbPlugs)
        {

            if (stbPlugs > 13)
            {
                stbPlugs = 13;
            }

            for (int i = 0; i < 26; i++)
            {
                stbrett[i] = i;
            }

            int swaps = 0;

            while (swaps < stbPlugs)
            {
                int p1 = random.Next(26);
                if (stbrett[p1] != p1)
                {
                    continue;
                }
                int p2 = random.Next(26);
                if (p1 == p2)
                {
                    continue;
                }
                if (stbrett[p2] != p2)
                {
                    continue;
                }
                swap(p1, p2);
                swaps++;

            }
            return 1;
        }

        public void setRandomMesg()
        {
            gMesg = getRandomInRange(0, 25);
            lMesg = getRandomInRange(0, 25);
            mMesg = getRandomInRange(0, 25);
            rMesg = getRandomInRange(0, 25);
        }

        public string stbstring()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 26; i++)
            {
                int sti = stbrett[i];
                if (sti > i)
                {
                    sb.Append(EnigmaUtils.getChar(i));
                    sb.Append(EnigmaUtils.getChar(sti));
                }
            }
            return sb.ToString();
        }

        public static void resetCounter()
        {
            counter = 0;
        }

        public static int getCounter()
        {
            return counter;
        }

        public int stbCount()
        {
            int plugs = 0;
            for (int i = 0; i < 26; i++)
            {
                if (stbrett[i] != i)
                {
                    plugs++;
                }
            }
            return plugs;
        }

        public void assertStbSelfAndCount(int vi, int vsi, int vk, int vsk)
        {
            bool viSelf = stbrett[vi] == vi;
            bool vsiSelf = stbrett[vsi] == vsi;
            bool vkSelf = stbrett[vk] == vk;
            bool vskSelf = stbrett[vsk] == vsk;
            if (!viSelf || !vsiSelf || !vkSelf || !vskSelf)
            {
                throw new Exception("Should be all self, it isnt");
            }
            if (stbCount() >= Key.MAX_STB_PLUGS)
            {
                throw new Exception("Should have less than " + Key.MAX_STB_PLUGS + " plugs " + stbCount() + " " + stbstring());
            }
        }

        public void assertstbCount()
        {

            if (stbCount() > Key.MAX_STB_PLUGS)
            {
                throw new Exception("Should have no more than \"+ MAX_STB_PLUGS + \" plugs " + stbCount() + " " + stbstring());
            }
        }

        public void stbConnect(int x, int y)
        {
            stbrett[x] = y;
            stbrett[y] = x;
            counter++;

            /*
                    for (int i = 0; i < 26; i++) {
                        if (stbrett[i] == -1)
                            continue;
                        if (stbrett[stbrett[i]] != i)
                            throw new RuntimeException("stbConnect: we have a big problem");
                    }
            */
        }

        public void stbDisconnect(int x, int y)
        {
            stbrett[x] = x;
            stbrett[y] = y;
            counter++;

            /*
                    for (int i = 0; i < 26; i++) {
                        if (stbrett[i] == -1)
                            continue;
                        if (stbrett[stbrett[i]] != i)
                            throw new RuntimeException("stbDisconnect: we have a big problem");
                    }
            */
        }

        public void swap(int x, int y)
        {
            counter++;

            if (x == y)
            {
                throw new Exception("Not a real swap");
            }

            /*
            for (int i = 0; i < 26; i++) {
                if (stbrett[i] == -1)
                    continue;
                if (stbrett[stbrett[i]] != i)
                    Console.WriteLine("SWAP: we have a big problem");
            } */

            int store = stbrett[x];
            stbrett[x] = stbrett[y];
            stbrett[y] = store;

            /*
            for (int i = 0; i < 26; i++) {
                if (stbrett[i] == -1)
                    continue;
                if (stbrett[stbrett[i]] != i)
                    Console.WriteLine("SWAP: we have a big problem");
            }
            */
        }

        public void getSteckerToSf()
        {
            int i, k = 25;

            for (i = 0; i < 26; i++)
            {
                if (stbrett[i] == -1)
                {
                    continue;
                }
                if (stbrett[stbrett[i]] != i)
                {
                    Console.WriteLine("TO SF: we have a big problem");
                }
            }

            _stbCount = 0;
            for (i = 0; i < 26; i++)
            {
                if (stbrett[i] > i)
                {
                    if (_stbCount == 26)
                    {
                        Console.WriteLine("we have a problem");
                    }
                    sf[_stbCount++] = i;
                    sf[_stbCount++] = stbrett[i];
                }
                else if (stbrett[i] == i)
                {
                    sf[k--] = i;
                }
            }
        }

        public double icscore(short[] ciphertext, int len)
        {

            if (len < 2)
            {
                return 0;
            }

            //Arrays.fill(f, 0);
            for (int i = 0; i < f.Length; i++)
            {
                f[i] = 0;
            }

            int c;

            for (int i = 0; i < len; i++)
            {
                c = stbrett[ciphertext[i]];
                c = lookup[(i << 5) + c];
                //c = stbrett[c];
                f[c]++;
            }

            double ic = 0;
            foreach (int fi in f)
            {
                ic += fi * (fi - 1);
            }
            ic /= len * (len - 1);

            return ic;
        }

        public int uniscore(short[] ciphertext, int len, EnigmaStats enigmaStats)
        {
            int i;
            int c;
            int s = 0;

            for (i = 0; i < len; i++)
            {
                c = stbrett[ciphertext[i]];
                c = lookup[(i << 5) + c];
                c = stbrett[c];

                s += enigmaStats.unidict[c];
            }
            return s / len;
        }

        public int biscore(short[] ciphertext, int len, EnigmaStats enigmaStats)
        {
            int c1, c2;
            int s = 0;

            int i = 0;
            c1 = stbrett[ciphertext[i]];
            c1 = lookup[c1];
            c1 = stbrett[c1];
            i++;

            while (i < len)
            {
                c2 = stbrett[ciphertext[i]];
                c2 = lookup[(i << 5) + c2];
                c2 = stbrett[c2];
                s += enigmaStats.biflat[(c1 << 5) + c2];
                c1 = c2;
                i++;
            }

            return s / (len - 1);
        }


        public int eval(EVAL eval, short[] ciphertext, int len, EnigmaStats enigmaStats)
        {
            counter++;
            switch (eval)
            {
                case EVAL.IC:
                    return (int)(300000.0 * icscore(ciphertext, len));
                case EVAL.BI:
                    return (int)(biscore(ciphertext, len, enigmaStats) * 0.50);
                case EVAL.TRI:
                    return triscore(ciphertext, len, enigmaStats);
                case EVAL.UNI:
                    return 30 * uniscore(ciphertext, len, enigmaStats);
                default:
                    throw new Exception("Invalid eval type");
            }
        }

        public int triscore(short[] ciphertext, int len, EnigmaStats enigmaStats)
        {
            int c;
            int s = 0;
            int triIndex = 0;
            int i = 0;
            c = stbrett[ciphertext[i]];
            c = lookup[(i << 5) + c];
            c = stbrett[c];
            triIndex = ((triIndex & 0x3ff) << 5) + c;
            i++;

            c = stbrett[ciphertext[i]];
            c = lookup[(i << 5) + c];
            c = stbrett[c];
            triIndex = ((triIndex & 0x3ff) << 5) + c;
            i++;

            while (i < len)
            {
                c = stbrett[ciphertext[i]];
                c = lookup[(i << 5) + c];
                c = stbrett[c];
                triIndex = ((triIndex & 0x3ff) << 5) + c;
                s += enigmaStats.triflat[triIndex];
                i++;

            }
            return s / (len - 2);
            //return s;
        }

        public int indicScore(short[][] indicMsgKeys, short[][] indicCiphertext, int nIndics)
        {
            short[] indicPlaintext = new short[6];
            int newScore = 0;

            for (int i = 0; i < nIndics; i++)
            {
                if (indicMsgKeys == null)
                {
                    encipherDecipherAll(indicCiphertext[i], indicPlaintext, 6);
                }
                else
                {
                    Key indicKey = new Key(this)
                    {
                        lMesg = indicMsgKeys[i][0],
                        mMesg = indicMsgKeys[i][1],
                        rMesg = indicMsgKeys[i][2]
                    };
                    indicKey.encipherDecipherAll(indicCiphertext[i], indicPlaintext, 6);
                }
                int matchCount = 0;
                for (int k = 0; k < 3; k++)
                {
                    if (indicPlaintext[k] == indicPlaintext[k + 3])
                    {
                        matchCount++;
                    }
                }
                newScore += matchCount;

            }
            return 1000 * newScore / (3 * nIndics);
        }

        public void substractRightRotorOffset(int offset)
        {
            rRing = (rRing - offset + 26) % 26;
            rMesg = (rMesg - offset + 26) % 26;
        }

        public void addRightRotorOffset(int offset)
        {
            rRing = (rRing + offset + 26) % 26;
            rMesg = (rMesg + offset + 26) % 26;
        }

        public bool checkFemales(bool[] indicFemales, bool print)
        {
            initPathLookupAll(6);
            if (print)
            {
                printKeystring("KEY TESTED");
            }
            for (int pos = 0; pos < 3; pos++)
            {
                bool keyFemale = false;
                if (print)
                {
                    Console.WriteLine("POS {0} {1}: ", pos, indicFemales[pos] ? "Y" : "N");
                }
                for (int plainLetter = 0; plainLetter < 26; plainLetter++)
                {
                    int encryptedLetterAtPos = lookup[((pos + 0) << 5) + plainLetter];
                    int encryptedLetterAtPosPlus3 = lookup[((pos + 3) << 5) + plainLetter];

                    if (print)
                    {
                        Console.WriteLine("{0}", "" + EnigmaUtils.getChar(encryptedLetterAtPos) + EnigmaUtils.getChar(encryptedLetterAtPosPlus3) + ",");
                    }
                    if (encryptedLetterAtPos == encryptedLetterAtPosPlus3)
                    {
                        keyFemale = true;
                        break;
                    }
                }
                if (print)
                {
                    Console.WriteLine("");
                }
                if (indicFemales[pos] && !keyFemale)
                {
                    return false;
                }

            }
            return true;
        }

        public bool buildAndCompareCycles(int[][] dbCycleSizes, bool print)
        {

            int[][] links = new int[][] { new int[26], new int[26], new int[26] };
            int[][] keyCycleSizes = new int[][] { new int[26], new int[26], new int[26] };

            initPathLookupAll(6);
            for (int pos = 0; pos < 3; pos++)
            {
                for (int l = 0; l < 26; l++)
                {
                    links[pos][l] = -1;
                }
            }

            for (int pos = 0; pos < 3; pos++)
            {
                for (int plainLetter = 0; plainLetter < 26; plainLetter++)
                {
                    int encryptedLetterAtPos = lookup[((pos + 0) << 5) + plainLetter];
                    int encryptedLetterAtPosPlus3 = lookup[((pos + 3) << 5) + plainLetter];

                    if (links[pos][encryptedLetterAtPos] != -1)
                    {
                        Console.WriteLine("BuildAndCompare - double link!");
                        return false;
                    }
                    links[pos][encryptedLetterAtPos] = encryptedLetterAtPosPlus3;
                }
            }

            //PrintKeystring("Build and Compare Cycles");
            if (!buildCycles(links, keyCycleSizes, false))
            {
                Console.WriteLine("BuildCycles FAILED!");
                return false;
            }

            for (int pos = 0; pos < 3; pos++)
            {
                for (int i = 0; i < 26; i++)
                {
                    if (keyCycleSizes[pos][i] != dbCycleSizes[pos][i])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public string plaintextstring(short[] ciphertext, int len)
        {
            int i;
            int c;

            string s = "";

            for (i = 0; i < len; i++)
            {
                c = stbrett[ciphertext[i]];
                c = lookup[(i << 5) + c];
                c = stbrett[c];
                s += EnigmaUtils.getChar(c);
            }

            return s;
        }

        public void printKeystring(string desc)
        {
            string stecker = "";
            string azStecker = "";
            int i;

            getSteckerToSf();

            for (i = 0; i < _stbCount; i++)
            {
                stecker += EnigmaUtils.getChar(sf[i]);
            }
            for (i = 0; i < 26; i++)
            {
                azStecker += EnigmaUtils.getChar(stbrett[i]);
            }

            if (stecker.Length == 0)
            {
                azStecker = "";
            }

            if (desc.Length > 40)
            {
                desc += "";
            }
            else if (desc.Length > 0)
            {
                desc += " ";
            }
            string scoreS = "";

            if (score > 0)
            {
                scoreS = "Score: " + score + " ";
            }

            if (model != Model.M4)
            {
                Console.WriteLine("{0}{1} UKW: {2} Wheels: {3}{4}{5} Rings: {6}{7}{8} Message Key: {9}{10}{11} Stecker: {12} ({13}) ",
                        desc, scoreS, EnigmaUtils.getChar(ukwNum), lSlot, mSlot, rSlot,
                        EnigmaUtils.getChar(lRing), EnigmaUtils.getChar(mRing), EnigmaUtils.getChar(rRing),
                        EnigmaUtils.getChar(lMesg), EnigmaUtils.getChar(mMesg), EnigmaUtils.getChar(rMesg),
                        stecker, azStecker);
            }
            else
            {
                Console.WriteLine("{0}{1} UKW: {2} Wheels: {3}{4}{5}{6} Rings: {7}{8}{9}{10} Message key: {11}{12}{13}{14} Stecker: {15} ({16}) ",
                        desc, scoreS, ukwNum == 3 ? "B" : "C", gSlot == 9 ? "B" : "G", lSlot, mSlot, rSlot,
                        EnigmaUtils.getChar(gRing), EnigmaUtils.getChar(lRing), EnigmaUtils.getChar(mRing), EnigmaUtils.getChar(rRing),
                        EnigmaUtils.getChar(gMesg), EnigmaUtils.getChar(lMesg), EnigmaUtils.getChar(mMesg), EnigmaUtils.getChar(rMesg),
                        stecker, azStecker);
            }

        }

        public string getKeystringlong()
        {

            string s;

            string stecker = "";
            string azStecker = "";
            int i;


            getSteckerToSf();


            for (i = 0; i < _stbCount; i++)
            {
                stecker += EnigmaUtils.getChar(sf[i]);
            }
            for (i = 0; i < 26; i++)
            {
                azStecker += EnigmaUtils.getChar(stbrett[i]);
            }

            if (stecker.Length == 0)
            {
                azStecker = "";
            }

            string scoreS = "";

            if (score > 0)
            {
                scoreS = string.Format("Score: {0}", score);
            }

            if (model != Model.M4)
            {
                s = scoreS + " UKW:" + EnigmaUtils.getChar(ukwNum) + " Wheels:" + lSlot + mSlot + rSlot + " Ring:" +
                        EnigmaUtils.getChar(lRing) + EnigmaUtils.getChar(mRing) + EnigmaUtils.getChar(rRing) + " Msg:" +
                        EnigmaUtils.getChar(lMesg) + EnigmaUtils.getChar(mMesg) + EnigmaUtils.getChar(rMesg) + " Stb:" +
                        stecker + "(" + azStecker + ")";

            }
            else
            {
                s = scoreS + " UKW:" + (ukwNum == 3 ? "B" : "C") + " Wheels:" + (gSlot == 9 ? "B" : "G") +
                        lSlot + mSlot + rSlot + " Ring:" +
                        EnigmaUtils.getChar(gRing) + EnigmaUtils.getChar(lRing) + EnigmaUtils.getChar(mRing) + EnigmaUtils.getChar(rRing) + " Msg:" +
                        EnigmaUtils.getChar(gMesg) + EnigmaUtils.getChar(lMesg) + EnigmaUtils.getChar(mMesg) + EnigmaUtils.getChar(rMesg) + " Stb:" +
                        stecker + "(" + azStecker + ")";

            }
            return s;
        }

        public string getKeystringShort(bool includeSteckers = true)
        {

            string s = "";

            if (model != Model.M4)
            {
                s += "" + EnigmaUtils.getChar(ukwNum) + ":" + lSlot + mSlot + rSlot + ":" +
                        EnigmaUtils.getChar(lRing) + EnigmaUtils.getChar(mRing) + EnigmaUtils.getChar(rRing) + ":" +
                        EnigmaUtils.getChar(lMesg) + EnigmaUtils.getChar(mMesg) + EnigmaUtils.getChar(rMesg);

            }
            else
            {
                s += "" + (ukwNum == 3 ? "B" : "C") + ":" + (gSlot == 9 ? "B" : "G") +
                        lSlot + mSlot + rSlot + ":" +
                        EnigmaUtils.getChar(gRing) + EnigmaUtils.getChar(lRing) + EnigmaUtils.getChar(mRing) + EnigmaUtils.getChar(rRing) + ":" +
                        EnigmaUtils.getChar(gMesg) + EnigmaUtils.getChar(lMesg) + EnigmaUtils.getChar(mMesg) + EnigmaUtils.getChar(rMesg);
            }
            if (includeSteckers)
            {
                getSteckerToSf();
                if (_stbCount > 0)
                {
                    s += "|";
                    for (int i = 0; i < _stbCount; i++)
                    {
                        s += EnigmaUtils.getChar(sf[i]);
                    }
                }
            }
            return s;
        }

        public string getRotorSettingsstring()
        {

            string s = "";

            if (model != Model.M4)
            {
                s += "" + EnigmaUtils.getChar(ukwNum) + ":" + lSlot + mSlot + rSlot + ":" +
                        EnigmaUtils.getChar(lRing) + EnigmaUtils.getChar(mRing) + EnigmaUtils.getChar(rRing) + ":" +
                        EnigmaUtils.getChar(lMesg) + EnigmaUtils.getChar(mMesg) + EnigmaUtils.getChar(rMesg);

            }
            else
            {
                s += "" + (ukwNum == 3 ? "B" : "C") + ":" + (gSlot == 9 ? "B" : "G") +
                        lSlot + mSlot + rSlot + ":" +
                        EnigmaUtils.getChar(gRing) + EnigmaUtils.getChar(lRing) + EnigmaUtils.getChar(mRing) + EnigmaUtils.getChar(rRing) + ":" +
                        EnigmaUtils.getChar(gMesg) + EnigmaUtils.getChar(lMesg) + EnigmaUtils.getChar(mMesg) + EnigmaUtils.getChar(rMesg);
            }
            return s;

        }

        private int setUkw(string s)
        {
            if (s.Equals("A") || s.Equals("a"))
            {
                switch (model)
                {
                    case Model.H:
                        ukwNum = 0;
                        break;
                    default:
                        return 0;
                }
                return 1;
            }
            if (s.Equals("B") || s.Equals("b"))
            {
                switch (model)
                {
                    case Model.M4:
                        ukwNum = 3;
                        break;
                    default:
                        ukwNum = 1;
                        break;
                }
                return 1;
            }
            if (s.Equals("C") || s.Equals("c"))
            {
                switch (model)
                {
                    case Model.M4:
                        ukwNum = 4;
                        break;
                    default:
                        ukwNum = 2;
                        break;
                }
                return 1;
            }


            return 0;
        }

        private int setWalze(string s, bool allowWheelRepetitions)
        {
            string lmr;
            switch (model)
            {
                case Model.M4:
                    if (s.Length != 4)
                    {
                        return 0;
                    }
                    if ((s[0] == 'B') || (s[0] == 'b'))
                    {
                        gSlot = 9;
                    }
                    else if ((s[0] == 'G') || (s[0] == 'g'))
                    {
                        gSlot = 10;
                    }
                    else
                    {
                        return 0;
                    }
                    lmr = s.Substring(1);
                    break;
                default:
                    if (s.Length != 3)
                    {
                        return 0;
                    }
                    lmr = s;
                    break;
            }

            int l = EnigmaUtils.getDigitIndex(lmr[0]);
            int m = EnigmaUtils.getDigitIndex(lmr[1]);
            int r = EnigmaUtils.getDigitIndex(lmr[2]);

            if (!allowWheelRepetitions)
            {
                if ((l == m) || (l == r) || (m == r))
                {
                    return 0;
                }
            }
            switch (model)
            {

                case Model.H:
                    /* digits 1-5, no repetitions */
                    if ((l < 1) || (l > 5))
                    {
                        return 0;
                    }
                    else if ((m < 1) || (m > 5))
                    {
                        return 0;
                    }
                    else if ((r < 1) || (r > 5))
                    {
                        return 0;
                    }
                    break;

                case Model.M4:
                case Model.M3:
                    /* digits 1-8, no repetitions */
                    if ((l < 1) || (l > 8))
                    {
                        return 0;
                    }
                    else if ((m < 1) || (m > 8))
                    {
                        return 0;
                    }
                    else if ((r < 1) || (r > 8))
                    {
                        return 0;
                    }
                    break;
                default:
                    return 0;
            }

            lSlot = l;
            mSlot = m;
            rSlot = r;

            return 1;
        }

        private int setRing(string s)
        {
            int g = -1;
            string lmr;

            switch (model)
            {
                case Model.M4:
                    if (s.Length != 4)
                    {
                        return 0;
                    }
                    g = EnigmaUtils.getIndex(s[0]);
                    if (g == -1)
                    {
                        return 0;
                    }
                    lmr = s.Substring(1);
                    break;
                default:
                    if (s.Length != 3)
                    {
                        return 0;
                    }
                    lmr = s;
                    break;
            }

            int l = EnigmaUtils.getIndex(lmr[0]);
            int m = EnigmaUtils.getIndex(lmr[1]);
            int r = EnigmaUtils.getIndex(lmr[2]);

            if ((l == -1) || (m == -1) || (r == -1))
            {
                return 0;
            }

            if (model == Model.M4)
            {
                gRing = g;
            }

            lRing = l;
            mRing = m;
            rRing = r;

            return 1;
        }

        public int setMesg(string s)
        {
            int g = -1;
            string lmr;

            switch (model)
            {
                case Model.M4:
                    if (s.Length != 4)
                    {
                        return 0;
                    }

                    g = EnigmaUtils.getIndex(s[0]);
                    if (g == -1)
                    {
                        return 0;
                    }
                    lmr = s.Substring(1);
                    break;
                default:
                    if (s.Length != 3)
                    {
                        return 0;
                    }
                    lmr = s;
                    break;
            }

            int l = EnigmaUtils.getIndex(lmr[0]);
            int m = EnigmaUtils.getIndex(lmr[1]);
            int r = EnigmaUtils.getIndex(lmr[2]);

            if ((l == -1) || (m == -1) || (r == -1))
            {
                return 0;
            }

            if (model == Model.M4)
            {
                gMesg = g;
            }

            lMesg = l;
            mMesg = m;
            rMesg = r;

            return 1;
        }

        public string getMesg()
        {
            string s = "";
            if (model == Model.M4)
            {
                s += EnigmaUtils.getChar(gMesg);
            }

            s += EnigmaUtils.getChar(lMesg);
            s += EnigmaUtils.getChar(mMesg);
            s += EnigmaUtils.getChar(rMesg);

            return s;
        }

        public Key getKeyFromIndicator(string indicatorS, string indicatorMessageKeyS)
        {
            short[] indicCrypt = new short[3];
            short[] indicPlain = new short[3];
            Key tempKey = new Key(this);
            if (tempKey.setMesg(indicatorMessageKeyS) != 1)
            {
                return null;
            }
            int ilen = EnigmaUtils.getText(indicatorS, indicCrypt);
            if (ilen != 3)
            {
                return null;
            }
            tempKey.encipherDecipherAll(indicCrypt, indicPlain, ilen);
            if (tempKey.setMesg(EnigmaUtils.getstring(indicPlain, ilen)) != 1)
            {
                return null;
            }
            return tempKey;
        }

        public int setStecker(string s)
        {
            for (int i = 0; i < 26; i++)
            {
                stbrett[i] = sf[i] = i;
            }

            if (s == null)
            {
                return 1;
            }
            int len = s.Length;
            if (len == 0)
            {
                return 1;
            }

            int[] letters = new int[26];

            /* max 26 chars, even number */
            if ((len > 26) || (len % 2 != 0))
            {
                return 0;
            }

            /* alphabetic, no repetitions */

            for (int i = 0; i < len; i++)
            {
                letters[i] = EnigmaUtils.getIndex(s[i]);
                if (letters[i] == -1)
                {
                    return 0;
                }
                for (int k = 0; k < i; k++)
                {
                    if (letters[i] == letters[k])
                    {
                        return 0;
                    }
                }
            }

            /* swap appropriate letters */

            for (int i = 0; i < len; i += 2)
            {
                swap(letters[i], letters[i + 1]);
            }

            getSteckerToSf();

            return 1;
        }

        public int compareStecker(string s)
        {
            int len = s.Length;

            int[] letters = new int[26];

            /* max 26 chars, even number */
            if ((len > 26) || (len % 2 != 0))
            {
                return 0;
            }

            /* alphabetic, no repetitions */

            for (int i = 0; i < len; i++)
            {
                letters[i] = EnigmaUtils.getIndex(s[i]);
                if (letters[i] == -1)
                {
                    return 0;
                }
                for (int k = 0; k < i; k++)
                {
                    if (letters[i] == letters[k])
                    {
                        return 0;
                    }
                }
            }


            int count = 0;
            int[] compareStb = new int[26];
            for (int i = 0; i < 26; i++)
            {
                compareStb[i] = i;
            }

            for (int i = 0; i < len; i += 2)
            {
                int l1 = letters[i];
                int l2 = letters[i + 1];
                compareStb[l1] = l2;
                compareStb[l2] = l1;
            }

            for (int i = 0; i < 26; i++)
            {
                if (compareStb[i] == stbrett[i])
                {
                    count++;
                }
            }

            return count;
        }

        public int setStecker(int[] stb)
        {
            _stbCount = 0;
            for (int i = 0; i < 26; i++)
            {
                if (stb[i] > i)
                {
                    _stbCount++;
                }
                stbrett[i] = stb[i];
                sf[i] = i;
            }
            if (_stbCount > 0)
            {
                getSteckerToSf();
            }

            return 1;
        }

        public int setKey(string s, Model model, bool adjust)
        {
            if (initDefaults(model) != 1)
            {
                return 0;
            }

            if (s.Length == 0)
            {
                return 0;
            }

            string ukwS, walzeS, ringS, mesgS;

            if (model == Model.M4)
            {
                if (s.Length != 16)
                {
                    return 0;
                }
                if ((s[1] != ':') || (s[6] != ':') || (s[11] != ':'))
                {
                    return 0;
                }
                ukwS = s.JavaSubstring(0, 1);
                walzeS = s.JavaSubstring(2, 6);
                ringS = s.JavaSubstring(7, 11);
                mesgS = s.JavaSubstring(12, 16);
            }
            else
            {
                if (s.Length == 13)
                {
                    if ((s[1] != ':') || (s[5] != ':') || (s[9] != ':'))
                    {
                        return 0;
                    }
                    ukwS = s.JavaSubstring(0, 1);
                    walzeS = s.JavaSubstring(2, 5);
                    ringS = s.JavaSubstring(6, 9);
                    mesgS = s.JavaSubstring(10, 13);
                }
                else if (s.Length == 16)
                {
                    if ((s[1] != ':') || (s[5] != ':') || (s[12] != ':'))
                    {
                        return 0;
                    }
                    ukwS = s.JavaSubstring(0, 1);
                    walzeS = s.JavaSubstring(2, 5);

                    try
                    {
                        ringS = "" + EnigmaUtils.getChar(-1 + int.Parse(s.JavaSubstring(6, 7)) * 10 + int.Parse(s.JavaSubstring(7, 8)));
                        ringS += "" + EnigmaUtils.getChar(-1 + int.Parse(s.JavaSubstring(8, 9)) * 10 + int.Parse(s.JavaSubstring(9, 10)));
                        ringS += "" + EnigmaUtils.getChar(-1 + int.Parse(s.JavaSubstring(10, 11)) * 10 + int.Parse(s.JavaSubstring(11, 12)));
                    }
                    catch (Exception)
                    {
                        return 0;
                    }
                    mesgS = s.JavaSubstring(13, 16);

                }
                else
                {
                    return 0;
                }
            }

            if (setUkw(ukwS) != 1)
            {
                return 0;
            }

            if (setWalze(walzeS, true) != 1)
            {
                return 0;
            }

            if (setRing(ringS) != 1)
            {
                return 0;
            }

            if (setMesg(mesgS) != 1)
            {
                return 0;
            }

            /* error checking for rings */
            if ((mSlot > 5) && (mRing > 12))
            {
                /*    if (adjust) {
                        mRing = (mRing + 13) % 26;
                        mMesg = (mMesg + 13) % 26;
                      }
                      else {
                        Utils.erlocalRinput_fatal(ERR.RING_SHORTCUT);
                        return 0;
                      }

                 */
            }
            if ((rSlot > 5) && (rRing > 12))
            {
                /*    if (adjust) {
                        rRing = (rRing + 13) % 26;
                        rMesg = (rMesg + 13) % 26;
                      }
                      else {
                        Utils.erlocalRinput_fatal(ERR.RING_SHORTCUT);
                        return 0;
                      }

                */
            }

            return 1;
        }

        public void initPathLookupHandM3(int len)
        {
            if ((lookup == null) || (lookup.Length != 32 * len))
            {
                lookup = new short[32 * len];
            }

            int i;
            short k;
            short c;

            /* calculate effective offset from ring and message settings */
            int offsetR = (26 + rMesg - rRing) % 26;
            int offsetM = (26 + mMesg - mRing) % 26;
            int offsetL = (26 + lMesg - lRing) % 26;

            /* calculate turnover points from ring settings */
            bool[] r_turnovers = getTurnoverPoints(rSlot, rRing);
            bool[] m_turnovers = getTurnoverPoints(mSlot, mRing);

            short[] walRslot = wal[rSlot];
            short[] walMslot = wal[mSlot];
            short[] walLslot = wal[lSlot];
            short[] ukwUkwnum = ukw[ukwNum];
            short[] rev_walLslot = rev_wal[lSlot];
            short[] rev_walMslot = rev_wal[mSlot];
            short[] rev_walRslot = rev_wal[rSlot];

            for (i = 0; i < len; i++)
            {
                if (m_turnovers[offsetM])
                {
                    offsetR = (offsetR + 1) % 26;
                    offsetM = (offsetM + 1) % 26;
                    offsetL = (offsetL + 1) % 26;
                }
                else if (r_turnovers[offsetR])
                {
                    offsetR = (offsetR + 1) % 26;
                    offsetM = (offsetM + 1) % 26;
                }
                else
                {
                    offsetR = (offsetR + 1) % 26;
                }

                for (k = 0; k < 26; k++)
                {
                    c = k;
                    c = etw[c];
                    c = walRslot[c + offsetR + 26];
                    c = walMslot[c - offsetR + offsetM + 26];
                    c = walLslot[c - offsetM + offsetL + 26];
                    c = ukwUkwnum[c - offsetL + 26];
                    c = rev_walLslot[c + offsetL + 26];
                    c = rev_walMslot[c + offsetM - offsetL + 26];
                    c = rev_walRslot[c + offsetR - offsetM + 26];
                    c = rev_etw[c - offsetR + 26];
                    lookup[(i << 5) + k] = c;
                }
            }
        }

        public void initPathLookupHandM3Range(int from, int len)
        {
            if (from == 0)
            {
                initPathLookupHandM3(len);
                return;
            }
            int lookup_length = 32 * (len + from);
            if ((lookup == null) || (lookup.Length != lookup_length))
            {
                lookup = new short[lookup_length];
            }


            int i;
            short k;
            short c;

            /* calculate effective offset from ring and message settings */
            int offsetR = (26 + rMesg - rRing) % 26;
            int offsetM = (26 + mMesg - mRing) % 26;
            int offsetL = (26 + lMesg - lRing) % 26;

            /* calculate turnover points from ring settings */
            bool[] rTurnovers = getTurnoverPoints(rSlot, rRing);
            bool[] mTurnovers = getTurnoverPoints(mSlot, mRing);

            short[] walRslot = wal[rSlot];
            short[] walMslot = wal[mSlot];
            short[] walLslot = wal[lSlot];
            short[] ukwUkwnum = ukw[ukwNum];
            short[] rev_walLslot = rev_wal[lSlot];
            short[] rev_walMslot = rev_wal[mSlot];
            short[] rev_walRslot = rev_wal[rSlot];

            for (i = 0; i < (from + len); i++)
            {

                if (mTurnovers[offsetM])
                {
                    offsetR = (offsetR + 1) % 26;
                    offsetM = (offsetM + 1) % 26;
                    offsetL = (offsetL + 1) % 26;
                }
                else if (rTurnovers[offsetR])
                {
                    offsetR = (offsetR + 1) % 26;
                    offsetM = (offsetM + 1) % 26;
                }
                else
                {
                    offsetR = (offsetR + 1) % 26;
                }

                if (i >= from)
                {
                    for (k = 0; k < 26; k++)
                    {
                        c = k;
                        c = etw[c];
                        c = walRslot[c + offsetR + 26];
                        c = walMslot[c - offsetR + offsetM + 26];
                        c = walLslot[c - offsetM + offsetL + 26];
                        c = ukwUkwnum[c - offsetL + 26];
                        c = rev_walLslot[c + offsetL + 26];
                        c = rev_walMslot[c + offsetM - offsetL + 26];
                        c = rev_walRslot[c + offsetR - offsetM + 26];
                        c = rev_etw[c - offsetR + 26];
                        lookup[(i << 5) + k] = c;
                    }
                }
            }
        }

        public void initPathLookupHandM4Range(int from, int len)
        {
            if (from == 0)
            {
                initPathLookupAll(len);
                return;
            }
            int lookup_length = 32 * (len + from);
            if ((lookup == null) || (lookup.Length != lookup_length))
            {
                lookup = new short[lookup_length];
            }

            int i;
            short k;
            short c;

            /* calculate effective offset from ring and message settings */
            int offsetR = (26 + rMesg - rRing) % 26;
            int offsetM = (26 + mMesg - mRing) % 26;
            int offsetL = (26 + lMesg - lRing) % 26;
            int offsetG = (26 + gMesg - gRing) % 26;

            /* calculate turnover points from ring settings */
            /* calculate turnover points from ring settings */
            bool[] rTurnovers = getTurnoverPoints(rSlot, rRing);
            bool[] mTurnovers = getTurnoverPoints(mSlot, mRing);

            short[] walRslot = wal[rSlot];
            short[] walMslot = wal[mSlot];
            short[] walLslot = wal[lSlot];
            short[] walGslot = wal[gSlot];
            short[] ukwUkwnum = ukw[ukwNum];
            short[] rev_walGslot = rev_wal[gSlot];
            short[] rev_walLslot = rev_wal[lSlot];
            short[] rev_walMslot = rev_wal[mSlot];
            short[] rev_walRslot = rev_wal[rSlot];

            for (i = 0; i < (from + len); i++)
            {

                if (mTurnovers[offsetM])
                {
                    offsetR = (offsetR + 1) % 26;
                    offsetM = (offsetM + 1) % 26;
                    offsetL = (offsetL + 1) % 26;
                }
                else if (rTurnovers[offsetR])
                {
                    offsetR = (offsetR + 1) % 26;
                    offsetM = (offsetM + 1) % 26;
                }
                else
                {
                    offsetR = (offsetR + 1) % 26;
                }

                if (i >= from)
                {
                    int base_ = i << 5;
                    for (k = 0; k < 26; k++)
                    {
                        c = k;
                        c = etw[c];
                        c = walRslot[c + offsetR + 26];
                        c = walMslot[c - offsetR + offsetM + 26];
                        c = walLslot[c - offsetM + offsetL + 26];
                        c = walGslot[c - offsetL + offsetG + 26];
                        c = ukwUkwnum[c - offsetG + 26];
                        c = rev_walGslot[c + offsetG];
                        c = rev_walLslot[c - offsetG + offsetL + 26];
                        c = rev_walMslot[c - offsetL + offsetM + 26];
                        c = rev_walRslot[c - offsetM + offsetR + 26];
                        c = rev_etw[c - offsetR + 26];
                        lookup[base_ + k] = c;
                    }
                }
            }
        }

        public void initPathLookupAll(int len)
        {
            if ((lookup == null) || (lookup.Length != 32 * len))
            {
                lookup = new short[32 * len];
            }

            int i;
            short k;
            short c;

            /* calculate effective offset from ring and message settings */
            int offsetR = (26 + rMesg - rRing) % 26;
            int offsetM = (26 + mMesg - mRing) % 26;
            int offsetL = (26 + lMesg - lRing) % 26;
            int offsetG = (26 + gMesg - gRing) % 26;

            /* calculate turnover points from ring settings */
            bool[] rTurnovers = getTurnoverPoints(rSlot, rRing);
            bool[] mTurnovers = getTurnoverPoints(mSlot, mRing);

            short[] walRslot = wal[rSlot];
            short[] walMslot = wal[mSlot];
            short[] walLslot = wal[lSlot];
            short[] walGslot = wal[gSlot];
            short[] ukwUkwnum = ukw[ukwNum];
            short[] rev_walGslot = rev_wal[gSlot];
            short[] rev_walLslot = rev_wal[lSlot];
            short[] rev_walMslot = rev_wal[mSlot];
            short[] rev_walRslot = rev_wal[rSlot];


            for (i = 0; i < len; i++)
            {
                if (mTurnovers[offsetM])
                {
                    offsetR = (offsetR + 1) % 26;
                    offsetM = (offsetM + 1) % 26;
                    offsetL = (offsetL + 1) % 26;
                }
                else if (rTurnovers[offsetR])
                {
                    offsetR = (offsetR + 1) % 26;
                    offsetM = (offsetM + 1) % 26;
                }
                else
                {
                    offsetR = (offsetR + 1) % 26;
                }

                int base_ = i << 5;
                for (k = 0; k < 26; k++)
                {
                    c = k;
                    c = etw[c];
                    c = walRslot[c + offsetR + 26];
                    c = walMslot[c - offsetR + offsetM + 26];
                    c = walLslot[c - offsetM + offsetL + 26];
                    c = walGslot[c - offsetL + offsetG + 26];
                    c = ukwUkwnum[c - offsetG + 26];
                    c = rev_walGslot[c + offsetG];
                    c = rev_walLslot[c - offsetG + offsetL + 26];
                    c = rev_walMslot[c - offsetL + offsetM + 26];
                    c = rev_walRslot[c - offsetM + offsetR + 26];
                    c = rev_etw[c - offsetR + 26];
                    lookup[base_ + k] = c;
                }
            }
        }

        public void initPathLookupRange(int from, int len)
        {
            if (from == 0)
            {
                initPathLookupAll(len);
                return;
            }

            if ((lookup == null) || (lookup.Length != 32 * len))
            {
                lookup = new short[32 * len];
            }

            int i;
            short k;
            short c;

            /* calculate effective offset from ring and message settings */
            int offsetR = (26 + rMesg - rRing) % 26;
            int offsetM = (26 + mMesg - mRing) % 26;
            int offsetL = (26 + lMesg - lRing) % 26;
            int offsetG = (26 + gMesg - gRing) % 26;

            /* calculate turnover points from ring settings */
            bool[] rTurnovers = getTurnoverPoints(rSlot, rRing);
            bool[] mTurnovers = getTurnoverPoints(mSlot, mRing);

            short[] walRslot = wal[rSlot];
            short[] walMslot = wal[mSlot];
            short[] walLslot = wal[lSlot];
            short[] walGslot = wal[gSlot];
            short[] ukwUkwnum = ukw[ukwNum];
            short[] rev_walGslot = rev_wal[gSlot];
            short[] rev_walLslot = rev_wal[lSlot];
            short[] rev_walMslot = rev_wal[mSlot];
            short[] rev_walRslot = rev_wal[rSlot];


            for (i = 0; i < (from + len); i++)
            {

                if (mTurnovers[offsetM])
                {
                    offsetM = (offsetM + 1) % 26;
                    offsetL = (offsetL + 1) % 26;
                }
                else if (rTurnovers[offsetR])
                {
                    offsetM = (offsetM + 1) % 26;
                }
                offsetR = (offsetR + 1) % 26;

                if (i >= from)
                {
                    for (k = 0; k < 26; k++)
                    {
                        c = k;
                        c = etw[c];
                        c = walRslot[c + offsetR + 26];
                        c = walMslot[c - offsetR + offsetM + 26];
                        c = walLslot[c - offsetM + offsetL + 26];
                        c = walGslot[c - offsetL + offsetG + 26];
                        c = ukwUkwnum[c - offsetG + 26];
                        c = rev_walGslot[c + offsetG + 26];
                        c = rev_walLslot[c + offsetL - offsetG + 26];
                        c = rev_walMslot[c + offsetM - offsetL + 26];
                        c = rev_walRslot[c + offsetR - offsetM + 26];
                        c = rev_etw[c - offsetR + 26];
                        lookup[(i << 5) + k] = c;
                    }
                }
            }
        }

        public double calcIcNoStb(short[] ciphertext, int len)
        {
            int[] f = new int[26];
            double S = 0;

            int i;
            int c;

            if (len < 2)
            {
                return 0;
            }

            /* calculate effective offset from ring and message settings */
            int offsetR = (26 + rMesg - rRing) % 26;
            int offsetM = (26 + mMesg - mRing) % 26;
            int offsetL = (26 + lMesg - lRing) % 26;
            int offsetG = (26 + gMesg - gRing) % 26;

            /* calculate turnover points from ring settings */
            bool[] rTurnovers = getTurnoverPoints(rSlot, rRing);
            bool[] mTurnovers = getTurnoverPoints(mSlot, mRing);

            for (i = 0; i < len; i++)
            {

                if (mTurnovers[offsetM])
                {
                    offsetR = (offsetR + 1) % 26;
                    offsetM = (offsetM + 1) % 26;
                    offsetL = (offsetL + 1) % 26;
                }
                else if (rTurnovers[offsetR])
                {
                    offsetR = (offsetR + 1) % 26;
                    offsetM = (offsetM + 1) % 26;
                }
                else
                {
                    offsetR = (offsetR + 1) % 26;
                }

                c = ciphertext[i];  /* no plugboard */
                c = etw[c];
                c = wal[rSlot][c + offsetR + 26];
                c = wal[mSlot][c - offsetR + offsetM + 26];
                c = wal[lSlot][c - offsetM + offsetL + 26];
                c = wal[gSlot][c - offsetL + offsetG + 26];
                c = ukw[ukwNum][c - offsetG + 26];
                c = rev_wal[gSlot][c + offsetG + 26];
                c = rev_wal[lSlot][c + offsetL - offsetG + 26];
                c = rev_wal[mSlot][c + offsetM - offsetL + 26];
                c = rev_wal[rSlot][c + offsetR - offsetM + 26];
                c = rev_etw[c - offsetR + 26];
                f[c]++;
            }

            for (i = 0; i < 26; i++)
            {
                S += f[i] * (f[i] - 1);
            }
            S /= len * (len - 1);

            return S;
        }

        public double icScoreWithoutLookupBuild(short[] ciphertext, int len)
        {
            int[] f = new int[26];
            double S = 0;

            int i;
            int c;

            if (len < 2)
            {
                return 0;
            }

            /* calculate effective offset from ring and message settings */
            int offsetR = (26 + rMesg - rRing) % 26;
            int offsetM = (26 + mMesg - mRing) % 26;
            int offsetL = (26 + lMesg - lRing) % 26;
            int offsetG = (26 + gMesg - gRing) % 26;

            /* calculate turnover points from ring settings */
            bool[] rTurnovers = getTurnoverPoints(rSlot, rRing);
            bool[] mTurnovers = getTurnoverPoints(mSlot, mRing);

            for (i = 0; i < len; i++)
            {

                if (mTurnovers[offsetM])
                {
                    offsetR = (offsetR + 1) % 26;
                    offsetM = (offsetM + 1) % 26;
                    offsetL = (offsetL + 1) % 26;
                }
                else if (rTurnovers[offsetR])
                {
                    offsetR = (offsetR + 1) % 26;
                    offsetM = (offsetM + 1) % 26;
                }
                else
                {
                    offsetR = (offsetR + 1) % 26;
                }

                c = ciphertext[i];  /* no plugboard */

                c = stbrett[c];
                c = etw[c];
                c = wal[rSlot][c + offsetR + 26];
                c = wal[mSlot][c - offsetR + offsetM + 26];
                c = wal[lSlot][c - offsetM + offsetL + 26];
                c = wal[gSlot][c - offsetL + offsetG + 26];
                c = ukw[ukwNum][c - offsetG + 26];
                c = rev_wal[gSlot][c + offsetG + 26];
                c = rev_wal[lSlot][c + offsetL - offsetG + 26];
                c = rev_wal[mSlot][c + offsetM - offsetL + 26];
                c = rev_wal[rSlot][c + offsetR - offsetM + 26];
                c = rev_etw[c - offsetR + 26];
                c = stbrett[c];
                f[c]++;
            }

            for (i = 0; i < 26; i++)
            {
                S += f[i] * (f[i] - 1);
            }
            S /= len * (len - 1);

            return S;

        }

        public void encipherDecipherAll(short[] input, short[] output, int len)
        {
            /* calculate effective offset from ring and message settings */
            int offsetR = (26 + rMesg - rRing) % 26;
            int offsetM = (26 + mMesg - mRing) % 26;
            int offsetL = (26 + lMesg - lRing) % 26;
            int offsetG = (26 + gMesg - gRing) % 26;

            /* calculate turnover points from ring settings */
            bool[] rTurnovers = getTurnoverPoints(rSlot, rRing);
            bool[] mTurnovers = getTurnoverPoints(mSlot, mRing);

            for (int i = 0; i < len; i++)
            {
                if (mTurnovers[offsetM])
                {
                    offsetR = (offsetR + 1) % 26;
                    offsetM = (offsetM + 1) % 26;
                    offsetL = (offsetL + 1) % 26;
                }
                else if (rTurnovers[offsetR])
                {
                    offsetR = (offsetR + 1) % 26;
                    offsetM = (offsetM + 1) % 26;
                }
                else
                {
                    offsetR = (offsetR + 1) % 26;
                }

                /* thru steckerbrett to scramblers */
                int c = input[i];
                c = stbrett[c];
                c = etw[c];
                /* thru scramblers and back */
                c = wal[rSlot][c + offsetR + 26];
                c = wal[mSlot][c - offsetR + offsetM + 26];
                c = wal[lSlot][c - offsetM + offsetL + 26];
                c = wal[gSlot][c - offsetL + offsetG + 26];
                c = ukw[ukwNum][c - offsetG + 26];
                c = rev_wal[gSlot][c + offsetG + 26];
                c = rev_wal[lSlot][c + offsetL - offsetG + 26];
                c = rev_wal[mSlot][c + offsetM - offsetL + 26];
                c = rev_wal[rSlot][c + offsetR - offsetM + 26];
                c = rev_etw[c - offsetR + 26];

                /* thru steckerbrett to lamp */
                output[i] = (short)stbrett[c];
            }
        }

        public void dottery(short[] input, int len, int[][] stats, int[] colStats)
        {
            /* calculate effective offset from ring and message settings */
            int offsetR = (26 + rMesg - rRing) % 26;
            int offsetM = (26 + mMesg - mRing) % 26;
            int offsetL = (26 + lMesg - lRing) % 26;
            int offsetG = (26 + gMesg - gRing) % 26;

            /* calculate turnover points from ring settings */
            bool[] rTurnovers = getTurnoverPoints(rSlot, rRing);
            bool[] mTurnovers = getTurnoverPoints(mSlot, mRing);

            for (int i = 0; i < len; i++)
            {
                if (mTurnovers[offsetM])
                {
                    offsetR = (offsetR + 1) % 26;
                    offsetM = (offsetM + 1) % 26;
                    offsetL = (offsetL + 1) % 26;
                }
                else if (rTurnovers[offsetR])
                {
                    offsetR = (offsetR + 1) % 26;
                    offsetM = (offsetM + 1) % 26;
                }
                else
                {
                    offsetR = (offsetR + 1) % 26;
                }

                /* thru steckerbrett to scramblers */
                int c = input[i];
                c = stbrett[c];
                c = etw[c];
                /* thru scramblers and back */
                c = wal[rSlot][c + offsetR + 26];
                c = wal[mSlot][c - offsetR + offsetM + 26];
                c = wal[lSlot][c - offsetM + offsetL + 26];
                c = wal[gSlot][c - offsetL + offsetG + 26];
                c = ukw[ukwNum][c - offsetG + 26];
                c = rev_wal[gSlot][c + offsetG + 26];
                c = rev_wal[lSlot][c + offsetL - offsetG + 26];
                c = rev_wal[mSlot][c + offsetM - offsetL + 26];
                c = rev_wal[rSlot][c + offsetR - offsetM + 26];
                c = rev_etw[c - offsetR + 26];

                c = stbrett[c];
                /* thru steckerbrett to lamp */
                stats[input[i]][c]++;
                colStats[c]++;
            }
        }

        public void showSteppings(short[] output, int len)
        {
            /* calculate effective offset from ring and message settings */
            int offsetR = (26 + rMesg - rRing) % 26;
            int offsetM = (26 + mMesg - mRing) % 26;
            int offsetL = (26 + lMesg - lRing) % 26;
            int offsetG = (26 + gMesg - gRing) % 26;

            /* calculate turnover points from ring settings */
            /* calculate turnover points from ring settings */
            bool[] rTurnovers = getTurnoverPoints(rSlot, rRing);
            bool[] mTurnovers = getTurnoverPoints(mSlot, mRing);

            short X = EnigmaUtils.getIndex('X');
            short M = EnigmaUtils.getIndex('M');
            short L = EnigmaUtils.getIndex('L');

            for (int i = 0; i < len; i++)
            {
                if (mTurnovers[offsetM])
                {
                    offsetR = (offsetR + 1) % 26;
                    offsetM = (offsetM + 1) % 26;
                    offsetL = (offsetL + 1) % 26;
                    output[i] = L;
                }
                else if (rTurnovers[offsetR])
                {
                    offsetR = (offsetR + 1) % 26;
                    offsetM = (offsetM + 1) % 26;
                    output[i] = M;
                }
                else
                {
                    offsetR = (offsetR + 1) % 26;
                    output[i] = X;
                }
            }
        }

        public int getLeftRotorSteppingPosition(int len)
        {
            /* calculate effective offset from ring and message settings */
            int offsetR = (26 + rMesg - rRing) % 26;
            int offsetM = (26 + mMesg - mRing) % 26;

            /* calculate turnover points from ring settings */
            /* calculate turnover points from ring settings */
            bool[] rTurnovers = getTurnoverPoints(rSlot, rRing);
            bool[] mTurnovers = getTurnoverPoints(mSlot, mRing);

            for (int i = 0; i < (len + 26); i++)
            {
                if (mTurnovers[offsetM])
                {
                    return i;
                }
                else if (rTurnovers[offsetR])
                {
                    offsetR = (offsetR + 1) % 26;
                    offsetM = (offsetM + 1) % 26;
                }
                else
                {
                    offsetR = (offsetR + 1) % 26;
                }
            }
            return -1;
        }

        public int triScoreWithoutLookupBuild(short[] ciphertext, int len, EnigmaStats enigmaStats)
        {
            int[] plaintext = new int[len];

            /* calculate effective offset from ring and message settings */
            int offsetR = (26 + rMesg - rRing) % 26;
            int offsetM = (26 + mMesg - mRing) % 26;
            int offsetL = (26 + lMesg - lRing) % 26;
            int offsetG = (26 + gMesg - gRing) % 26;

            /* calculate turnover points from ring settings */
            /* calculate turnover points from ring settings */
            bool[] rTurnovers = getTurnoverPoints(rSlot, rRing);
            bool[] mTurnovers = getTurnoverPoints(mSlot, mRing);

            for (int j = 0; j < len; j++)
            {

                int c = ciphertext[j];

                if (mTurnovers[offsetM])
                {
                    offsetR = (offsetR + 1) % 26;
                    offsetM = (offsetM + 1) % 26;
                    offsetL = (offsetL + 1) % 26;
                }
                else if (rTurnovers[offsetR])
                {
                    offsetR = (offsetR + 1) % 26;
                    offsetM = (offsetM + 1) % 26;
                }
                else
                {
                    offsetR = (offsetR + 1) % 26;
                }
                /* thru steckerbrett to scramblers */

                c = stbrett[c];
                c = etw[c];
                /* thru scramblers and back */
                c = wal[rSlot][c + offsetR];
                c = wal[mSlot][c - offsetR + offsetM + 26];
                c = wal[lSlot][c - offsetM + offsetL + 26];
                c = wal[gSlot][c - offsetL + offsetG + 26];
                c = ukw[ukwNum][c - offsetG + 26];
                c = rev_wal[gSlot][c + offsetG];
                c = rev_wal[lSlot][c + offsetL - offsetG + 26];
                c = rev_wal[mSlot][c + offsetM - offsetL + 26];
                c = rev_wal[rSlot][c + offsetR - offsetM + 26];
                c = rev_etw[c - offsetR + 26];

                /* thru steckerbrett to lamp */
                plaintext[j] = stbrett[c];
            }

            int i;
            int c1, c2, c3;
            int s = 0;

            c1 = plaintext[0];
            c2 = plaintext[1];

            for (i = 2; i < len; i++)
            {
                c3 = plaintext[i];

                //s += Main.tridict[c1][c2][c3];
                s += enigmaStats.triflat[(((c1 << 5) + c2) << 5) + c3];
                c1 = c2;
                c2 = c3;
            }

            return s / (len - 2);
            //return s;
        }

        public int uniScoreWithoutLookupBuild(short[] ciphertext, int len, EnigmaStats enigmaStats)
        {
            int[] plaintext = new int[len];

            /* calculate effective offset from ring and message settings */
            int offsetR = (26 + rMesg - rRing) % 26;
            int offsetM = (26 + mMesg - mRing) % 26;
            int offsetL = (26 + lMesg - lRing) % 26;
            int offsetG = (26 + gMesg - gRing) % 26;

            /* calculate turnover points from ring settings */
            /* calculate turnover points from ring settings */
            bool[] rTurnovers = getTurnoverPoints(rSlot, rRing);
            bool[] mTurnovers = getTurnoverPoints(mSlot, mRing);

            for (int i = 0; i < len; i++)
            {
                int c = ciphertext[i];

                if (mTurnovers[offsetM])
                {
                    offsetR = (offsetR + 1) % 26;
                    offsetM = (offsetM + 1) % 26;
                    offsetL = (offsetL + 1) % 26;
                }
                else if (rTurnovers[offsetR])
                {
                    offsetR = (offsetR + 1) % 26;
                    offsetM = (offsetM + 1) % 26;
                }
                else
                {
                    offsetR = (offsetR + 1) % 26;
                }

                /* thru steckerbrett to scramblers */

                c = stbrett[c];
                c = etw[c];
                /* thru scramblers and back */
                c = wal[rSlot][c + offsetR + 26];
                c = wal[mSlot][c - offsetR + offsetM + 26];
                c = wal[lSlot][c - offsetM + offsetL + 26];
                c = wal[gSlot][c - offsetL + offsetG + 26];
                c = ukw[ukwNum][c - offsetG + 26];
                c = rev_wal[gSlot][c + offsetG + 26];
                c = rev_wal[lSlot][c + offsetL - offsetG + 26];
                c = rev_wal[mSlot][c + offsetM - offsetL + 26];
                c = rev_wal[rSlot][c + offsetR - offsetM + 26];
                c = rev_etw[c - offsetR + 26];

                /* thru steckerbrett to lamp */
                plaintext[i] = stbrett[c];
            }

            int s = 0;

            for (int i = 0; i < len; i++)
            {
                int c = plaintext[i];
                s += enigmaStats.unidict[c];
            }

            return s / len;
        }

        public int mainLetterCountWithoutLookupBuild(short[] ciphertext, int len)
        {

            int[] plaintext = new int[len];

            /* calculate effective offset from ring and message settings */
            int offsetR = (26 + rMesg - rRing) % 26;
            int offsetM = (26 + mMesg - mRing) % 26;
            int offsetL = (26 + lMesg - lRing) % 26;
            int offsetG = (26 + gMesg - gRing) % 26;

            /* calculate turnover points from ring settings */
            /* calculate turnover points from ring settings */
            bool[] rTurnovers = getTurnoverPoints(rSlot, rRing);
            bool[] mTurnovers = getTurnoverPoints(mSlot, mRing);

            for (int i = 0; i < len; i++)
            {
                if (mTurnovers[offsetM])
                {
                    offsetM = (offsetM + 1) % 26;
                    offsetL = (offsetL + 1) % 26;
                }
                else if (rTurnovers[offsetR])
                {
                    offsetM = (offsetM + 1) % 26;
                }

                offsetR = (offsetR + 1) % 26;

                int c = ciphertext[i];

                /* thru steckerbrett to scramblers */

                c = stbrett[c];
                /* thru scramblers and back */
                c = etw[c];
                c = wal[rSlot][c + offsetR + 26];
                c = wal[mSlot][c - offsetR + offsetM + 26];
                c = wal[lSlot][c - offsetM + offsetL + 26];
                c = wal[gSlot][c - offsetL + offsetG + 26];
                c = ukw[ukwNum][c - offsetG + 26];
                c = rev_wal[gSlot][c + offsetG + 26];
                c = rev_wal[lSlot][c + offsetL - offsetG + 26];
                c = rev_wal[mSlot][c + offsetM - offsetL + 26];
                c = rev_wal[rSlot][c + offsetR - offsetM + 26];
                c = rev_etw[c - offsetR + 26];

                /* thru steckerbrett to lamp */
                plaintext[i] = stbrett[c];

            }

            int count = 0;
            int X = EnigmaUtils.getIndex('X');
            int J = EnigmaUtils.getIndex('J');
            int E = EnigmaUtils.getIndex('E');

            for (int i = 0; i < len; i++)
            {
                int c = plaintext[i];
                if (c == X)
                {
                    count++;
                }
                if (c == J)
                {
                    count++;
                }
                if (c == E)
                {
                    count++;
                }
            }

            return count;
        }

        public enum Model
        {
            H,
            M3,
            M4,
            A16081,
            A16101,
            NONE
        }

        public static class ModelParser
        {
            public static Model parse(string model)
            {
                if (model != null)
                {
                    model = model.ToUpper();
                }
                switch (model)
                {
                    case "H":
                        return Model.H;
                    case "M3":
                        return Model.M3;
                    case "M4":
                        return Model.M4;
                    case "A16081":
                        return Model.A16081;
                    case "A16101":
                        return Model.A16101;
                    default:
                        return Model.NONE;
                }

            }
        }

        public enum EVAL
        {
            IC,
            BI,
            TRI,
            UNI
        }
    }
}