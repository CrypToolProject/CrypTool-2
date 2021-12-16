/*
   Copyright 2016, Eugen Antal and Tomáš Sovič, FEI STU Bratislava

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

using System.Linq;


namespace CrypTool.Fialka
{
    public class FialkaConstants
    {
        // helper constants
        public const int alphabetSize = 30;
        public const int mod = alphabetSize;
        public const int mod2 = mod * 2; // pre calculated mod multiply, used in rotor substitution
        public const int ED2REF = 1; // normal perm helper
        public const int REF2ED = -1; // inverse helper
        public const int coreSideNormal = 1;
        public const int coreSideReverse = -1;
        public const int numberOfRotors = 10;
        public const int rotorsMagicOffset = 3; // offset of the rotors input from the entryDisk or reflektor
        /// <summary>
        /// Check position of pins for even rotors that determines the rotor stepping flow. 
        /// </summary>
        public const int evenRotorsPinCheckPosition = 17; // np. + 17
        /// <summary>
        /// Check position of pins for odd rotors that determines the rotor stepping flow. 
        /// </summary>
        public const int oddRotorsPinCheckPosition = 20; // p. +20
        /// <summary>
        /// Maximal value of the counter is 999 * 5 letter group + 4, this should be used as mod.
        /// </summary>
        public const int counterMax = 5000;//(999*5)+5;
        /// <summary>
        /// Counter's first 3 digits counts the number of processed 5 letter groups. The rightmost digit counts to 4 (+1 for each key pressed).
        /// </summary>
        public const int counter5LetterDigitMod = 5;
        /// <summary>
        /// Invalid input.
        /// </summary>
        public const int InvalidInput = -1;
        public const char InvalidInputPrintSymbol = '@';

        /// <summary>
        /// Helper array used to set the rotor positions (rotor order) into base position (default order).
        /// New copy is nedded, because the array ref. can be assigned to multiple variables.
        /// </summary>
        public static int[] baseRotorPositions() { return new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }; }
        /// <summary>
        /// Helper array used to set the rotor offsets to 0.
        /// New copy is nedded, because the array ref. can be assigned to multiple variables.
        /// </summary>
        public static int[] nullOffset() { return new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; }
        /// <summary>
        /// Core orientation 1 - normal side, -1 is flipped size. Used when the rotor permuatation is calculated.
        /// This array represents the PROTON I compatible core orientations without flipping.
        /// New copy is nedded, because the array ref. can be assigned to multiple variables.
        /// </summary>
        public static int[] deafultCoreOrientation() { return new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }; }

        /*
         * Immutable internal substitutions (and inverses).
         */
        public static readonly int[] keyboard = new int[] { 16, 24, 29, 13, 25, 20, 5, 18, 0, 14, 28, 19, 3, 27, 23, 1, 21, 22, 17, 11, 6, 4, 26, 7, 9, 8, 15, 12, 10, 2 };
        public static readonly int[] keyboardInverse = new int[] { 8, 15, 29, 12, 21, 6, 20, 23, 25, 24, 28, 19, 27, 3, 9, 26, 0, 18, 7, 11, 5, 16, 17, 14, 1, 4, 22, 13, 10, 2 };
        public static readonly int[] entryDisk = new int[] { 27, 13, 19, 23, 1, 15, 0, 9, 20, 10, 16, 12, 18, 29, 4, 5, 7, 14, 22, 24, 26, 17, 2, 28, 25, 11, 21, 6, 8, 3 };
        public static readonly int[] entryDiskInverse = new int[] { 6, 4, 22, 29, 14, 15, 27, 16, 28, 7, 9, 25, 11, 1, 17, 5, 10, 21, 12, 2, 8, 26, 18, 3, 19, 24, 20, 0, 23, 13 };
        public static readonly int[] reflectorEncrypt = new int[] { 22, 5, 19, 27, 13, 1, 11, 16, 21, 10, 9, 6, 12, 4, 28, 17, 7, 23, 26, 2, 24, 8, 0, 15, 20, 29, 18, 3, 14, 25 };
        public static readonly int[] reflectorDecrypt = new int[] { 22, 5, 19, 27, 13, 1, 11, 16, 21, 10, 9, 6, 12, 4, 28, 23, 7, 15, 26, 2, 24, 8, 0, 17, 20, 29, 18, 3, 14, 25 };
        /*
         * We do not have direct access to substitution tables. Static getter methods are responsible
         * to manage to access control based on specified rotorSeries.
         */

        //TODO: if it will be available, add 0K, 1K, 4K
        #region Wiring 3K
        private static readonly int[,] wiring3K = new int[,]
        {
            {22, 21, 2, 6, 3, 7, 15, 5, 9, 19, 14, 16, 23, 8, 29, 11, 24, 10, 0, 27, 26, 4, 28, 25, 1, 17, 20, 13, 12, 18},
            {2, 23, 19, 1, 5, 20, 25, 6, 17, 3, 16, 22, 14, 18, 9, 29, 12, 27, 28, 10, 8, 24, 0, 13, 21, 7, 26, 4, 11, 15},
            {19, 4, 6, 14, 20, 26, 3, 0, 21, 16, 22, 12, 29, 5, 25, 9, 15, 13, 18, 17, 28, 23, 2, 11, 8, 10, 1, 27, 7, 24},
            {15, 20, 27, 10, 26, 2, 14, 11, 23, 29, 8, 16, 3, 19, 24, 7, 0, 28, 18, 17, 13, 9, 4, 22, 25, 6, 5, 21, 1, 12},
            {17, 14, 0, 21, 18, 15, 28, 7, 16, 3, 2, 13, 5, 29, 22, 4, 25, 12, 24, 9, 11, 20, 26, 19, 6, 10, 23, 8, 1, 27},
            {8, 13, 12, 19, 23, 7, 1, 5, 4, 18, 10, 27, 29, 2, 17, 14, 6, 24, 15, 0, 11, 22, 26, 28, 16, 9, 20, 3, 21, 25},
            {6, 8, 4, 25, 5, 3, 18, 2, 7, 27, 21, 11, 20, 23, 22, 9, 12, 0, 15, 28, 1, 24, 26, 14, 17, 10, 13, 16, 29, 19},
            {28, 26, 14, 12, 7, 1, 24, 11, 5, 22, 8, 17, 23, 0, 13, 20, 16, 9, 2, 10, 21, 6, 15, 3, 18, 25, 4, 29, 27, 19},
            {4, 18, 1, 26, 19, 25, 6, 10, 15, 17, 2, 12, 3, 22, 27, 20, 5, 23, 28, 29, 14, 16, 8, 11, 7, 21, 24, 9, 0, 13},
            {19, 23, 7, 24, 18, 0, 16, 4, 14, 26, 8, 11, 21, 9, 17, 2, 15, 29, 3, 13, 6, 22, 10, 1, 28, 25, 27, 20, 5, 12}
        };

        private static readonly int[,] inverseWiring3K = new int[,]
        {
            {18, 24, 2, 4, 21, 7, 3, 5, 13, 8, 17, 15, 28, 27, 10, 6, 11, 25, 29, 9, 26, 1, 0, 12, 16, 23, 20, 19, 22, 14},
            {22, 3, 0, 9, 27, 4, 7, 25, 20, 14, 19, 28, 16, 23, 12, 29, 10, 8, 13, 2, 5, 24, 11, 1, 21, 6, 26, 17, 18, 15},
            {7, 26, 22, 6, 1, 13, 2, 28, 24, 15, 25, 23, 11, 17, 3, 16, 9, 19, 18, 0, 4, 8, 10, 21, 29, 14, 5, 27, 20, 12},
            {16, 28, 5, 12, 22, 26, 25, 15, 10, 21, 3, 7, 29, 20, 6, 0, 11, 19, 18, 13, 1, 27, 23, 8, 14, 24, 4, 2, 17, 9},
            {2, 28, 10, 9, 15, 12, 24, 7, 27, 19, 25, 20, 17, 11, 1, 5, 8, 0, 4, 23, 21, 3, 14, 26, 18, 16, 22, 29, 6, 13},
            {19, 6, 13, 27, 8, 7, 16, 5, 0, 25, 10, 20, 2, 1, 15, 18, 24, 14, 9, 3, 26, 28, 21, 4, 17, 29, 22, 11, 23, 12},
            {17, 20, 7, 5, 2, 4, 0, 8, 1, 15, 25, 11, 16, 26, 23, 18, 27, 24, 6, 29, 12, 10, 14, 13, 21, 3, 22, 9, 19, 28},
            {13, 5, 18, 23, 26, 8, 21, 4, 10, 17, 19, 7, 3, 14, 2, 22, 16, 11, 24, 29, 15, 20, 9, 12, 6, 25, 1, 28, 0, 27},
            {28, 2, 10, 12, 0, 16, 6, 24, 22, 27, 7, 23, 11, 29, 20, 8, 21, 9, 1, 4, 15, 25, 13, 17, 26, 5, 3, 14, 18, 19},
            {5, 23, 15, 18, 7, 28, 20, 2, 10, 13, 22, 11, 29, 19, 8, 16, 6, 14, 4, 0, 27, 12, 21, 1, 3, 25, 9, 26, 24, 17}
        };

        private static readonly int[,] pinPositions3K = new int[,]
        {
            {0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 1, 0, 1, 0, 1, 0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 0, 1, 0, 1, 0},
            {0, 0, 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 1, 0, 1, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 1},
            {1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 1, 0},
            {1, 0, 1, 1, 1, 1, 1, 0, 1, 1, 0, 1, 0, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 0, 1},
            {0, 0, 1, 0, 1, 0, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0},
            {0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 1, 0, 1, 1, 1, 1, 0, 1},
            {1, 0, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 1, 1, 1, 0, 0, 1, 0, 1, 1, 1, 1, 0, 1, 1, 1, 1, 0, 1},
            {0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1},
            {1, 1, 1, 1, 0, 1, 0, 1, 0, 0, 1, 1, 0, 1, 0, 1, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 1, 0},
            {0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 1, 1, 1, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 1, 1, 1, 0}
        };

        private static readonly int[,] wiring3KFlippedCores = new int[,]
        {
            {8, 12, 18, 17, 10, 13, 29, 5, 2, 26, 4, 3, 0, 20, 6, 19, 1, 22, 7, 14, 16, 11, 21, 25, 15, 23, 27, 24, 28, 9},
            {28, 15, 19, 26, 4, 23, 9, 17, 0, 6, 22, 20, 2, 3, 18, 1, 21, 12, 16, 8, 14, 27, 13, 24, 5, 10, 25, 29, 11, 7},
            {11, 6, 23, 3, 29, 20, 22, 19, 28, 7, 2, 13, 12, 17, 15, 21, 5, 25, 1, 18, 8, 14, 9, 0, 27, 4, 10, 16, 24, 26},
            {15, 18, 29, 9, 25, 24, 5, 8, 26, 21, 17, 13, 12, 2, 0, 23, 6, 11, 27, 14, 22, 1, 7, 19, 16, 28, 4, 20, 3, 10},
            {13, 3, 29, 22, 7, 20, 24, 11, 4, 10, 19, 21, 6, 18, 5, 26, 8, 1, 25, 17, 28, 27, 14, 23, 2, 15, 12, 9, 0, 16},
            {22, 5, 9, 27, 10, 21, 14, 2, 4, 8, 19, 0, 15, 6, 24, 16, 13, 28, 1, 3, 20, 12, 26, 25, 29, 23, 7, 11, 18, 17},
            {24, 11, 1, 14, 17, 20, 13, 16, 4, 6, 29, 2, 15, 0, 18, 21, 8, 7, 10, 19, 9, 3, 23, 28, 12, 27, 25, 5, 26, 22},
            {2, 11, 3, 1, 26, 5, 12, 27, 15, 24, 9, 20, 28, 21, 14, 10, 17, 0, 7, 13, 22, 8, 25, 19, 6, 29, 23, 18, 16, 4},
            {26, 17, 0, 21, 6, 9, 23, 19, 22, 14, 16, 1, 2, 7, 25, 10, 3, 8, 27, 18, 28, 13, 15, 20, 24, 5, 11, 4, 29, 12},
            {11, 18, 25, 10, 3, 5, 2, 29, 20, 8, 24, 17, 27, 1, 15, 28, 13, 21, 9, 19, 22, 4, 16, 26, 14, 0, 12, 6, 23, 7}
        };

        private static readonly int[,] inverseWiring3KFlippedCores = new int[,]
        {
            {12, 16, 8, 11, 10, 7, 14, 18, 0, 29, 4, 21, 1, 5, 19, 24, 20, 3, 2, 15, 13, 22, 17, 25, 27, 23, 9, 26, 28, 6},
            {8, 15, 12, 13, 4, 24, 9, 29, 19, 6, 25, 28, 17, 22, 20, 1, 18, 7, 14, 2, 11, 16, 10, 5, 23, 26, 3, 21, 0, 27},
            {23, 18, 10, 3, 25, 16, 1, 9, 20, 22, 26, 0, 12, 11, 21, 14, 27, 13, 19, 7, 5, 15, 6, 2, 28, 17, 29, 24, 8, 4},
            {14, 21, 13, 28, 26, 6, 16, 22, 7, 3, 29, 17, 12, 11, 19, 0, 24, 10, 1, 23, 27, 9, 20, 15, 5, 4, 8, 18, 25, 2},
            {28, 17, 24, 1, 8, 14, 12, 4, 16, 27, 9, 7, 26, 0, 22, 25, 29, 19, 13, 10, 5, 11, 3, 23, 6, 18, 15, 21, 20, 2},
            {11, 18, 7, 19, 8, 1, 13, 26, 9, 2, 4, 27, 21, 16, 6, 12, 15, 29, 28, 10, 20, 5, 0, 25, 14, 23, 22, 3, 17, 24},
            {13, 2, 11, 21, 8, 27, 9, 17, 16, 20, 18, 1, 24, 6, 3, 12, 7, 4, 14, 19, 5, 15, 29, 22, 0, 26, 28, 25, 23, 10},
            {17, 3, 0, 2, 29, 5, 24, 18, 21, 10, 15, 1, 6, 19, 14, 8, 28, 16, 27, 23, 11, 13, 20, 26, 9, 22, 4, 7, 12, 25},
            {2, 11, 12, 16, 27, 25, 4, 13, 17, 5, 15, 26, 29, 21, 9, 22, 10, 1, 19, 7, 23, 3, 8, 6, 24, 14, 0, 18, 20, 28},
            {25, 13, 6, 4, 21, 5, 27, 29, 9, 18, 3, 0, 26, 16, 24, 14, 22, 11, 1, 19, 8, 17, 20, 28, 10, 2, 23, 12, 15, 7}
        };
        #endregion

        #region Wiring 5K
        private static readonly int[,] wiring5K = new int[,]
        {
            {12, 10, 9, 14, 0, 19, 23, 1, 3, 8, 26, 16, 25, 2, 11, 7, 4, 15, 18, 21, 22, 27, 13, 24, 17, 5, 29, 28, 6, 20},
            {5, 18, 21, 0, 20, 12, 27, 25, 29, 9, 8, 6, 11, 22, 16, 23, 24, 7, 19, 15, 3, 1, 14, 4, 17, 28, 2, 26, 10, 13},
            {24, 14, 27, 6, 18, 17, 13, 9, 23, 5, 15, 8, 3, 19, 2, 26, 21, 4, 7, 28, 0, 11, 20, 22, 16, 25, 12, 1, 29, 10},
            {22, 2, 23, 12, 3, 25, 17, 14, 10, 5, 28, 21, 24, 6, 8, 20, 19, 1, 13, 16, 18, 29, 7, 9, 11, 0, 15, 27, 4, 26},
            {15, 21, 8, 1, 5, 4, 0, 11, 3, 16, 13, 20, 17, 14, 27, 26, 7, 10, 28, 2, 24, 9, 18, 25, 22, 12, 23, 19, 6, 29},
            {9, 5, 7, 19, 22, 26, 25, 3, 28, 17, 16, 20, 23, 10, 13, 27, 0, 4, 12, 11, 21, 2, 15, 18, 6, 8, 24, 29, 1, 14},
            {20, 29, 25, 2, 15, 27, 19, 8, 24, 0, 6, 16, 9, 5, 26, 22, 11, 1, 21, 23, 13, 10, 7, 3, 18, 4, 12, 14, 28, 17},
            {23, 16, 10, 19, 29, 28, 18, 9, 6, 20, 5, 17, 1, 0, 4, 12, 26, 11, 14, 24, 3, 22, 13, 27, 8, 2, 25, 15, 7, 21},
            {16, 8, 1, 3, 19, 26, 15, 9, 20, 6, 14, 28, 25, 24, 17, 13, 10, 29, 21, 11, 27, 22, 12, 7, 0, 5, 4, 2, 23, 18},
            {1, 11, 4, 7, 12, 28, 9, 18, 24, 23, 15, 6, 21, 26, 25, 27, 29, 5, 3, 8, 10, 17, 16, 22, 0, 2, 13, 19, 14, 20}
        };

        private static readonly int[,] inverseWiring5K = new int[,]
        {
            {4, 7, 13, 8, 16, 25, 28, 15, 9, 2, 1, 14, 0, 22, 3, 17, 11, 24, 18, 5, 29, 19, 20, 6, 23, 12, 10, 21, 27, 26},
            {3, 21, 26, 20, 23, 0, 11, 17, 10, 9, 28, 12, 5, 29, 22, 19, 14, 24, 1, 18, 4, 2, 13, 15, 16, 7, 27, 6, 25, 8},
            {20, 27, 14, 12, 17, 9, 3, 18, 11, 7, 29, 21, 26, 6, 1, 10, 24, 5, 4, 13, 22, 16, 23, 8, 0, 25, 15, 2, 19, 28},
            {25, 17, 1, 4, 28, 9, 13, 22, 14, 23, 8, 24, 3, 18, 7, 26, 19, 6, 20, 16, 15, 11, 0, 2, 12, 5, 29, 27, 10, 21},
            {6, 3, 19, 8, 5, 4, 28, 16, 2, 21, 17, 7, 25, 10, 13, 0, 9, 12, 22, 27, 11, 1, 24, 26, 20, 23, 15, 14, 18, 29},
            {16, 28, 21, 7, 17, 1, 24, 2, 25, 0, 13, 19, 18, 14, 29, 22, 10, 9, 23, 3, 11, 20, 4, 12, 26, 6, 5, 15, 8, 27},
            {9, 17, 3, 23, 25, 13, 10, 22, 7, 12, 21, 16, 26, 20, 27, 4, 11, 29, 24, 6, 0, 18, 15, 19, 8, 2, 14, 5, 28, 1},
            {13, 12, 25, 20, 14, 10, 8, 28, 24, 7, 2, 17, 15, 22, 18, 27, 1, 11, 6, 3, 9, 29, 21, 0, 19, 26, 16, 23, 5, 4},
            {24, 2, 27, 3, 26, 25, 9, 23, 1, 7, 16, 19, 22, 15, 10, 6, 0, 14, 29, 4, 8, 18, 21, 28, 13, 12, 5, 20, 11, 17},
            {24, 0, 25, 18, 2, 17, 11, 3, 19, 6, 20, 1, 4, 26, 28, 10, 22, 21, 7, 27, 29, 12, 23, 9, 8, 14, 13, 15, 5, 16}
        };

        private static readonly int[,] pinPositions5K = new int[,]
        {
            {1, 1, 1, 1, 0, 1, 0, 0, 0, 1, 1, 0, 0, 1, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0},
            {0, 0, 0, 1, 0, 1, 0, 1, 0, 0, 1, 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0},
            {1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 0},
            {0, 1, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0},
            {0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 0, 0, 1, 0, 1, 0, 0, 0, 0, 1, 0, 0},
            {0, 0, 0, 1, 1, 1, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 1, 1, 0},
            {1, 0, 0, 0, 0, 1, 1, 0, 0, 1, 1, 1, 0, 1, 0, 0, 0, 1, 0, 0, 1, 1, 0, 0, 1, 0, 0, 0, 1, 1},
            {1, 1, 0, 1, 0, 1, 0, 1, 1, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 0},
            {0, 1, 1, 0, 0, 1, 1, 0, 0, 1, 1, 1, 1, 0, 1, 0, 0, 1, 0, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 1},
            {0, 0, 1, 1, 0, 0, 1, 0, 0, 1, 1, 1, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 0, 1, 1, 0, 1, 1}
        };

        private static readonly int[,] wiring5KFlippedCores = new int[,]
        {
            {18, 10, 24, 2, 1, 25, 13, 6, 17, 3, 8, 9, 12, 15, 26, 23, 19, 28, 5, 14, 4, 22, 27, 29, 7, 11, 0, 16, 21, 20},
            {25, 17, 20, 4, 28, 2, 13, 26, 16, 29, 27, 15, 11, 23, 6, 7, 14, 8, 19, 24, 22, 21, 1, 5, 3, 18, 10, 0, 9, 12},
            {6, 20, 1, 29, 18, 5, 14, 8, 10, 19, 0, 2, 23, 26, 9, 4, 28, 11, 27, 22, 15, 25, 7, 21, 17, 13, 12, 24, 3, 16},
            {8, 4, 26, 3, 15, 0, 19, 21, 23, 1, 12, 14, 17, 29, 11, 10, 22, 24, 6, 9, 2, 25, 20, 16, 13, 5, 27, 18, 7, 28},
            {15, 1, 24, 11, 7, 18, 8, 5, 12, 21, 6, 28, 2, 20, 23, 4, 3, 16, 13, 10, 17, 14, 27, 19, 0, 26, 25, 29, 22, 9},
            {21, 16, 29, 1, 6, 22, 24, 12, 15, 28, 9, 19, 18, 26, 0, 3, 17, 20, 7, 10, 14, 13, 2, 27, 5, 4, 8, 11, 23, 25},
            {10, 13, 2, 16, 18, 26, 12, 27, 23, 20, 17, 7, 9, 29, 19, 8, 4, 25, 21, 14, 24, 0, 6, 22, 11, 3, 15, 28, 5, 1},
            {7, 9, 23, 15, 5, 28, 22, 3, 17, 8, 27, 6, 16, 19, 4, 18, 26, 0, 29, 13, 25, 10, 24, 21, 12, 2, 1, 11, 20, 14},
            {14, 12, 7, 28, 26, 25, 0, 23, 18, 8, 3, 19, 9, 1, 20, 17, 13, 6, 5, 2, 16, 24, 10, 21, 15, 4, 11, 27, 29, 22},
            {29, 10, 16, 11, 17, 28, 0, 8, 14, 13, 20, 22, 27, 25, 1, 3, 5, 4, 9, 24, 15, 7, 6, 12, 21, 2, 18, 23, 26, 19}
        };
        private static readonly int[,] inverseWiring5KFlippedCores = new int[,]
        {
            {26, 4, 3, 9, 20, 18, 7, 24, 10, 11, 1, 25, 12, 6, 19, 13, 27, 8, 0, 16, 29, 28, 21, 15, 2, 5, 14, 22, 17, 23},
            {27, 22, 5, 24, 3, 23, 14, 15, 17, 28, 26, 12, 29, 6, 16, 11, 8, 1, 25, 18, 2, 21, 20, 13, 19, 0, 7, 10, 4, 9},
            {10, 2, 11, 28, 15, 5, 0, 22, 7, 14, 8, 17, 26, 25, 6, 20, 29, 24, 4, 9, 1, 23, 19, 12, 27, 21, 13, 18, 16, 3},
            {5, 9, 20, 3, 1, 25, 18, 28, 0, 19, 15, 14, 10, 24, 11, 4, 23, 12, 27, 6, 22, 7, 16, 8, 17, 21, 2, 26, 29, 13},
            {24, 1, 12, 16, 15, 7, 10, 4, 6, 29, 19, 3, 8, 18, 21, 0, 17, 20, 5, 23, 13, 9, 28, 14, 2, 26, 25, 22, 11, 27},
            {14, 3, 22, 15, 25, 24, 4, 18, 26, 10, 19, 27, 7, 21, 20, 8, 1, 16, 12, 11, 17, 0, 5, 28, 6, 29, 13, 23, 9, 2},
            {21, 29, 2, 25, 16, 28, 22, 11, 15, 12, 0, 24, 6, 1, 19, 26, 3, 10, 4, 14, 9, 18, 23, 8, 20, 17, 5, 7, 27, 13},
            {17, 26, 25, 7, 14, 4, 11, 0, 9, 1, 21, 27, 24, 19, 29, 3, 12, 8, 15, 13, 28, 23, 6, 2, 22, 20, 16, 10, 5, 18},
            {6, 13, 19, 10, 25, 18, 17, 2, 9, 12, 22, 26, 1, 16, 0, 24, 20, 15, 8, 11, 14, 23, 29, 7, 21, 5, 4, 27, 3, 28},
            {6, 14, 25, 15, 17, 16, 22, 21, 7, 18, 1, 3, 23, 9, 8, 20, 2, 4, 26, 29, 10, 24, 11, 27, 19, 13, 28, 12, 5, 0}
        };

        #endregion

        #region Wiring 6K
        private static readonly int[,] wiring6K = new int[,]
        {
            {12, 21, 7, 17, 19, 11, 27, 3, 14, 26, 2, 4, 15, 13, 22, 25, 0, 24, 16, 10, 29, 9, 23, 6, 5, 20, 28, 1, 8, 18},
            {19, 7, 4, 14, 3, 27, 20, 0, 23, 12, 28, 11, 13, 22, 24, 6, 8, 29, 26, 2, 10, 17, 16, 18, 21, 9, 1, 25, 5, 15},
            {28, 10, 3, 21, 23, 15, 17, 1, 22, 2, 16, 7, 19, 4, 27, 11, 14, 25, 29, 6, 20, 18, 12, 9, 26, 24, 8, 0, 13, 5},
            {3, 11, 18, 28, 23, 22, 6, 29, 14, 0, 19, 13, 17, 1, 15, 26, 9, 24, 16, 27, 5, 20, 10, 7, 21, 4, 8, 2, 25, 12},
            {17, 1, 14, 6, 19, 27, 7, 12, 22, 11, 18, 26, 3, 23, 9, 13, 10, 5, 29, 2, 16, 25, 21, 0, 28, 24, 15, 20, 4, 8},
            {15, 3, 13, 23, 22, 18, 29, 2, 0, 7, 26, 12, 8, 4, 28, 9, 14, 25, 21, 6, 24, 16, 19, 10, 1, 5, 20, 27, 17, 11},
            {25, 22, 6, 4, 12, 7, 23, 29, 28, 19, 21, 8, 11, 9, 24, 15, 2, 20, 18, 17, 3, 0, 27, 26, 5, 1, 14, 16, 10, 13},
            {15, 21, 13, 29, 23, 14, 16, 19, 3, 6, 26, 11, 5, 12, 24, 20, 0, 4, 25, 7, 10, 22, 28, 27, 2, 17, 9, 18, 1, 8},
            {11, 0, 16, 28, 5, 3, 6, 10, 14, 2, 20, 24, 8, 25, 29, 12, 21, 19, 9, 23, 26, 13, 27, 22, 1, 4, 18, 17, 15, 7},
            {8, 20, 7, 15, 24, 4, 19, 21, 3, 26, 13, 18, 0, 14, 29, 1, 9, 16, 6, 23, 11, 17, 28, 2, 22, 5, 12, 27, 25, 10}
        };

        private static readonly int[,] inverseWiring6K = new int[,]
        {
            {16, 27, 10, 7, 11, 24, 23, 2, 28, 21, 19, 5, 0, 13, 8, 12, 18, 3, 29, 4, 25, 1, 14, 22, 17, 15, 9, 6, 26, 20},
            {7, 26, 19, 4, 2, 28, 15, 1, 16, 25, 20, 11, 9, 12, 3, 29, 22, 21, 23, 0, 6, 24, 13, 8, 14, 27, 18, 5, 10, 17},
            {27, 7, 9, 2, 13, 29, 19, 11, 26, 23, 1, 15, 22, 28, 16, 5, 10, 6, 21, 12, 20, 3, 8, 4, 25, 17, 24, 14, 0, 18},
            {9, 13, 27, 0, 25, 20, 6, 23, 26, 16, 22, 1, 29, 11, 8, 14, 18, 12, 2, 10, 21, 24, 5, 4, 17, 28, 15, 19, 3, 7},
            {23, 1, 19, 12, 28, 17, 3, 6, 29, 14, 16, 9, 7, 15, 2, 26, 20, 0, 10, 4, 27, 22, 8, 13, 25, 21, 11, 5, 24, 18},
            {8, 24, 7, 1, 13, 25, 19, 9, 12, 15, 23, 29, 11, 2, 16, 0, 21, 28, 5, 22, 26, 18, 4, 3, 20, 17, 10, 27, 14, 6},
            {21, 25, 16, 20, 3, 24, 2, 5, 11, 13, 28, 12, 4, 29, 26, 15, 27, 19, 18, 9, 17, 10, 1, 6, 14, 0, 23, 22, 8, 7},
            {16, 28, 24, 8, 17, 12, 9, 19, 29, 26, 20, 11, 13, 2, 5, 0, 6, 25, 27, 7, 15, 1, 21, 4, 14, 18, 10, 23, 22, 3},
            {1, 24, 9, 5, 25, 4, 6, 29, 12, 18, 7, 0, 15, 21, 8, 28, 2, 27, 26, 17, 10, 16, 23, 19, 11, 13, 20, 22, 3, 14},
            {12, 15, 23, 8, 5, 25, 18, 2, 0, 16, 29, 20, 26, 10, 13, 3, 17, 21, 11, 6, 1, 7, 24, 19, 4, 28, 9, 27, 22, 14}
        };

        private static readonly int[,] pinPositions6K = new int[,]
        {
            {1, 1, 1, 0, 1, 0, 1, 0, 0, 0, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 0, 1, 1, 1, 1, 0, 1, 0, 1, 1},
            {0, 0, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 1, 0, 1, 0, 1, 0},
            {0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 1, 0},
            {1, 0, 1, 0, 0, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 0, 1, 0, 1, 1, 1, 0, 1, 0},
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 1, 0, 0, 0, 0, 1, 1},
            {0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 1, 0, 1, 1, 1, 0, 1, 0, 1},
            {1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 0, 1, 0, 1, 1, 1, 1, 1, 1, 0, 0, 1, 0},
            {0, 1, 1, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 1, 1, 1, 1, 1},
            {0, 1, 0, 0, 1, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0},
            {1, 1, 1, 0, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 0, 1, 1},
        };

        private static readonly int[,] wiring6KFlippedCores = new int[,]
        {
            {18, 12, 22, 29, 2, 10, 25, 24, 7, 21, 1, 20, 14, 6, 0, 5, 8, 17, 15, 26, 28, 4, 16, 27, 3, 19, 11, 13, 23, 9},
            {11, 15, 25, 5, 29, 21, 9, 12, 14, 13, 20, 28, 4, 1, 22, 24, 6, 8, 17, 19, 2, 18, 7, 0, 10, 3, 27, 16, 26, 23},
            {2, 25, 17, 0, 22, 6, 4, 21, 18, 12, 10, 24, 1, 5, 16, 19, 3, 26, 11, 23, 14, 28, 8, 29, 13, 15, 7, 9, 27, 20},
            {27, 18, 5, 28, 22, 26, 9, 23, 20, 10, 25, 3, 14, 6, 21, 4, 15, 29, 13, 17, 11, 0, 16, 1, 24, 8, 7, 2, 12, 19},
            {13, 22, 26, 10, 15, 6, 2, 0, 9, 5, 14, 28, 1, 25, 20, 17, 21, 7, 27, 4, 12, 19, 8, 18, 23, 3, 11, 24, 16, 29},
            {15, 19, 13, 3, 10, 25, 29, 20, 11, 14, 6, 24, 9, 5, 16, 21, 2, 26, 22, 18, 4, 23, 0, 28, 1, 12, 8, 7, 17, 27},
            {5, 17, 20, 14, 16, 29, 25, 4, 3, 0, 27, 13, 12, 10, 28, 15, 6, 21, 19, 22, 9, 11, 2, 1, 7, 23, 18, 26, 24, 8},
            {15, 22, 29, 12, 21, 13, 28, 3, 2, 8, 20, 23, 5, 26, 0, 10, 6, 18, 25, 19, 4, 24, 27, 11, 14, 16, 7, 1, 17, 9},
            {19, 23, 15, 13, 12, 26, 29, 8, 3, 17, 4, 7, 21, 11, 9, 18, 1, 5, 22, 6, 10, 28, 16, 20, 24, 27, 25, 2, 14, 0},
            {22, 20, 5, 3, 18, 25, 8, 28, 2, 13, 19, 7, 24, 14, 21, 29, 1, 16, 0, 12, 17, 4, 27, 9, 11, 26, 6, 15, 23, 10}
        };

        private static readonly int[,] inverseWiring6KFlippedCores = new int[,]
        {
            {14, 10, 4, 24, 21, 15, 13, 8, 16, 29, 5, 26, 1, 27, 12, 18, 22, 17, 0, 25, 11, 9, 2, 28, 7, 6, 19, 23, 20, 3},
            {23, 13, 20, 25, 12, 3, 16, 22, 17, 6, 24, 0, 7, 9, 8, 1, 27, 18, 21, 19, 10, 5, 14, 29, 15, 2, 28, 26, 11, 4},
            {3, 12, 0, 16, 6, 13, 5, 26, 22, 27, 10, 18, 9, 24, 20, 25, 14, 2, 8, 15, 29, 7, 4, 19, 11, 1, 17, 28, 21, 23},
            {21, 23, 27, 11, 15, 2, 13, 26, 25, 6, 9, 20, 28, 18, 12, 16, 22, 19, 1, 29, 8, 14, 4, 7, 24, 10, 5, 0, 3, 17},
            {7, 12, 6, 25, 19, 9, 5, 17, 22, 8, 3, 26, 20, 0, 10, 4, 28, 15, 23, 21, 14, 16, 1, 24, 27, 13, 2, 18, 11, 29},
            {22, 24, 16, 3, 20, 13, 10, 27, 26, 12, 4, 8, 25, 2, 9, 0, 14, 28, 19, 1, 7, 15, 18, 21, 11, 5, 17, 29, 23, 6},
            {9, 23, 22, 8, 7, 0, 16, 24, 29, 20, 13, 21, 12, 11, 3, 15, 4, 1, 26, 18, 2, 17, 19, 25, 28, 6, 27, 10, 14, 5},
            {14, 27, 8, 7, 20, 12, 16, 26, 9, 29, 15, 23, 3, 5, 24, 0, 25, 28, 17, 19, 10, 4, 1, 11, 21, 18, 13, 22, 6, 2},
            {29, 16, 27, 8, 10, 17, 19, 11, 7, 14, 20, 13, 4, 3, 28, 2, 22, 9, 15, 0, 23, 12, 18, 1, 24, 26, 5, 25, 21, 6},
            {18, 16, 8, 3, 21, 2, 26, 11, 6, 23, 29, 24, 19, 9, 13, 27, 17, 20, 4, 10, 1, 14, 0, 28, 12, 5, 25, 22, 7, 15}
        };

        #endregion


        /// <summary>
        /// Identity permutation.
        /// </summary>
        /// <returns></returns>
        public static int[] punchCardIdentity() { return new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29 }; }


        #region Class methods
        /// <summary>
        /// Helper method to manage the access to rotor wiring based on specified rotorSeries.
        /// </summary>
        /// <param name="rs">Rotor series.</param>
        /// <returns>Rotor wirings for base rotor positions.</returns>
        public static int[,] getRotorWiring(FialkaEnums.rotorSeries rs)
        {
            int[,] retval = null;
            switch (rs)
            {
                case FialkaEnums.rotorSeries.K6:
                    retval = wiring6K;
                    break;
                case FialkaEnums.rotorSeries.K5:
                    retval = wiring5K;
                    break;
                case FialkaEnums.rotorSeries.K3:
                    retval = wiring3K;
                    break;
            };

            return retval;
        }

        /// <summary>
        /// Helper method to manage the access to rotor wiring (for flipped cores) based on specified rotorSeries.
        /// </summary>
        /// <param name="rs">Rotor series.</param>
        /// <returns>Rotor wirings (for flipped cores) for base rotor positions.</returns>
        public static int[,] getRotorWiringFlippedCores(FialkaEnums.rotorSeries rs)
        {
            int[,] retval = null;
            switch (rs)
            {
                case FialkaEnums.rotorSeries.K6:
                    retval = wiring6KFlippedCores;
                    break;
                case FialkaEnums.rotorSeries.K5:
                    retval = wiring5KFlippedCores;
                    break;
                case FialkaEnums.rotorSeries.K3:
                    retval = wiring3KFlippedCores;
                    break;
            };

            return retval;
        }

        /// <summary>
        /// Helper method to manage the access to inverse rotor wiring based on specified rotorSeries. 
        /// </summary>
        /// <param name="rs">Rotor series.</param>
        /// <returns>Inverse rotor wirings for base rotor positions.</returns>
        public static int[,] getInverseRotorWiring(FialkaEnums.rotorSeries rs)
        {
            int[,] retval = null;
            switch (rs)
            {
                case FialkaEnums.rotorSeries.K6:
                    retval = inverseWiring6K;
                    break;
                case FialkaEnums.rotorSeries.K5:
                    retval = inverseWiring5K;
                    break;
                case FialkaEnums.rotorSeries.K3:
                    retval = inverseWiring3K;
                    break;
            };

            return retval;
        }

        /// <summary>
        /// Helper method to manage the access to inverse rotor wiring (for flipped cores) based on specified rotorSeries. 
        /// </summary>
        /// <param name="rs">Rotor series.</param>
        /// <returns>Inverse rotor wirings (for flipped cores) for base rotor positions.</returns>
        public static int[,] getInverseRotorWiringFlippedCores(FialkaEnums.rotorSeries rs)
        {
            int[,] retval = null;
            switch (rs)
            {
                case FialkaEnums.rotorSeries.K6:
                    retval = inverseWiring6KFlippedCores;
                    break;
                case FialkaEnums.rotorSeries.K5:
                    retval = inverseWiring5KFlippedCores;
                    break;
                case FialkaEnums.rotorSeries.K3:
                    retval = inverseWiring3KFlippedCores;
                    break;
            };

            return retval;
        }

        /// <summary>
        /// Pin positions of rotors placed in base rotor positions.
        /// </summary>
        /// <param name="rs">Rotor series.</param>
        /// <returns>Pin positions for base rotor positions.</returns>
        public static int[,] getPinPositions(FialkaEnums.rotorSeries rs)
        {
            int[,] retval = null;
            switch (rs)
            {
                case FialkaEnums.rotorSeries.K6:
                    retval = pinPositions6K;
                    break;
                case FialkaEnums.rotorSeries.K5:
                    retval = pinPositions5K;
                    break;
                case FialkaEnums.rotorSeries.K3:
                    retval = pinPositions3K;
                    break;
            };

            return retval;
        }


        /// <summary>
        /// Helper method to calculate inverse permutation, without input check.
        /// </summary>
        /// <param name="input">Permutation.</param>
        /// <returns>Inverse permutation.</returns>
        public static int[] getInversePermutation(int[] input)
        {
            int[] retVal = new int[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                //retVal[i] = Array.IndexOf(input, i);
                retVal[input[i]] = i; // performance boost
            }

            return retVal;
        }
        #endregion

        #region input output
        /*
         * Layout mappings (for keyboard and printHead): text and special for different countries + cyrillic print head.
         * We've made one modification to handle the printHead in mixed mode. For key 'A...' we inserted '[' as a special char.
         * For '1...' we inserted ']' as a special char. In real fialka there is no mapping, the pressed key controls some mechanical settings.
         */
        public static readonly string keyboardCyrillic = "АБВГДЕЖЗИКЛМНОПРСТУФХЦЧШЩЫЬЮЯЙ";
        public static readonly string keyboardCyrillicSpec = "58-3Ж,[+ЙN%.4°97Ъ62]0:\"1=Ф/Э; ";
        public static readonly string keyboardPolandLet = "FADULT5PBRKVZJGHCNE7QWXIOSM8Y2";
        public static readonly string keyboardPolandSpec = "Ą%-6)4[9?3(=5+ĘŁ:,2]01/78Ž.°' ";
        public static readonly string keyboardCzechoslovakianLet = "FADULT5PBRKVZJGHCNE7QWXIOSM8Y2";
        public static readonly string keyboardCzechoslovakianSpec = "ŘÄČ6Ě4[9\"3Ö=5ÜŽ+:,2]01/78Š.%- ";
        public static readonly string keyboardGDRLet = "F8DULT5PBRKVZ4GHCNEA327IOSM96J";
        public static readonly string keyboardGDRSpec = "V:W6)4[9/3(X5JÄÜY,2]01F78Q.-Ö*";
        // special constants used in mixed-mode
        public const string MixedModeLetterShiftChars = "[5Ж";
        public const string MixedModeNumberShiftChars = "]7AФ";
        public const int NumerShiftContact = 19;
        public const int LetterShiftContact = 6;

        /*
         * Space is specially handled:
         * Space key is allowed only in Plain and Encryption modes. In Decryption mode is disabled as input.
         * In encryption mode the space is swapped - replaced with 'Й' value (contact 30). 
         * In decryption mode the 'Й' input (also the '2' and 'j' keys - contact 30) is transformed back to space.
         */
        public const char space = ' ';
        public const int spaceContactInDecryptMode = 29; // index of spaceMappin in the layout
        public static readonly char spaceMappingCyr = 'Й';
        public static readonly string spaceMappingDDR = "J*";
        public static readonly char spaceMappingOther = '2';

        /// <summary>
        /// Helper method to handle the disabled char (space is used instead).
        /// </summary>
        /// <param name="printHead">Used print head.</param>
        /// <param name="country">used country layout.</param>
        /// <param name="key">Input key char.</param>
        /// <returns>If the key is disabled.</returns>
        public static bool spaceDisabledKey(FialkaEnums.printHeadMapping printHead, FialkaEnums.countryLayout country, char key)
        {
            bool blocked = false;
            if (printHead == FialkaEnums.printHeadMapping.Cyrillic)
            {
                blocked = (key == spaceMappingCyr);
            }
            else
            {
                switch (country)
                {
                    case FialkaEnums.countryLayout.GDR:
                        blocked = spaceMappingDDR.Contains(key);
                        break;
                    // same for CZK and POL
                    case FialkaEnums.countryLayout.Czechoslovakia:
                    case FialkaEnums.countryLayout.Poland:
                        blocked = (key == spaceMappingOther);
                        break;

                }
            }

            return blocked;
        }

        /// <summary>
        /// Returns the Z_30 mapping of the cyrillic input char. If invalid, returns -1.
        /// </summary>
        /// <param name="input">Input char.</param>
        /// <returns>Z_30 mapping.</returns>
        public static int mapCyrillicInput(char input)
        {
            int idx = InvalidInput;
            if (keyboardCyrillic.Contains(input))
            {
                idx = keyboardCyrillic.IndexOf(input);
            }
            return idx;
        }
        /// <summary>
        /// Returns the Z_30 mapping of the cyrillic special input char. If invalit, returns -1.
        /// </summary>
        /// <param name="input">Input char.</param>
        /// <returns>Z_30 mapping.</returns>
        public static int mapCyrillicSpecialInput(char input)
        {
            int idx = InvalidInput;
            if (keyboardCyrillicSpec.Contains(input))
            {
                idx = keyboardCyrillicSpec.IndexOf(input);
            }
            return idx;
        }

        /// <summary>
        /// Returns the country specific Z_30 mapping of the latin input char. If invalid, returns -1.
        /// </summary>
        /// <param name="country">Selected country.</param>
        /// <param name="input">Input char.</param>
        /// <returns>Z_30 mapping.</returns>
        public static int mapLatinInput(FialkaEnums.countryLayout country, char input)
        {
            int idx = InvalidInput;
            switch (country)
            {
                case FialkaEnums.countryLayout.Czechoslovakia:
                    if (keyboardCzechoslovakianLet.Contains(input))
                    {
                        idx = keyboardCzechoslovakianLet.IndexOf(input);
                    }
                    break;
                case FialkaEnums.countryLayout.GDR:
                    if (keyboardGDRLet.Contains(input))
                    {
                        idx = keyboardGDRLet.IndexOf(input);
                    }
                    break;
                case FialkaEnums.countryLayout.Poland:
                    if (keyboardPolandLet.Contains(input))
                    {
                        idx = keyboardPolandLet.IndexOf(input);
                    }
                    break;
            }
            return idx;
        }

        /// <summary>
        /// Returns the country specific Z_30 mapping of the latin special input char. For invalid input returns -1.
        /// </summary>
        /// <param name="country">Selected country.</param>
        /// <param name="input">Input char.</param>
        /// <returns>Z_30 mapping.</returns>
        public static int mapSpecialInput(FialkaEnums.countryLayout country, char input)
        {
            int idx = InvalidInput;
            switch (country)
            {
                case FialkaEnums.countryLayout.Czechoslovakia:
                    if (keyboardCzechoslovakianSpec.Contains(input))
                    {
                        idx = keyboardCzechoslovakianSpec.IndexOf(input);
                    }
                    break;
                case FialkaEnums.countryLayout.GDR:
                    if (keyboardGDRSpec.Contains(input))
                    {
                        idx = keyboardGDRSpec.IndexOf(input);
                    }
                    break;
                case FialkaEnums.countryLayout.Poland:
                    if (keyboardPolandSpec.Contains(input))
                    {
                        idx = keyboardPolandSpec.IndexOf(input);
                    }
                    break;
            }
            return idx;
        }



        /// <summary>
        /// Returns the country specific cyrillic char mapping of the Z_30 result from Fialka. 
        /// For invalid input returns 'InvalidInputPrintSymbol' (this should not happend if the input is directly from the machine).
        /// </summary>
        /// <param name="country">Selected country.</param>
        /// <param name="input">Input char mapping in Z_30.</param>
        /// <returns>Cyrillic char mapping.</returns>
        public static char getPrinterCharCyrillic(FialkaEnums.printHeadShift shiftMode, int input)
        {
            char output = FialkaConstants.InvalidInputPrintSymbol;
            switch (shiftMode)
            {
                case FialkaEnums.printHeadShift.LetterShift:
                    {
                        output = keyboardCyrillic[input];
                    }
                    break;
                case FialkaEnums.printHeadShift.NumberShift:
                    {
                        output = keyboardCyrillicSpec[input];
                    }
                    break;
            }

            return output;
        }

        /// <summary>
        /// Returns the country specific latin char mapping of the Z_30 result from Fialka. 
        /// For invalid input returns 'InvalidInputPrintSymbol' (this should not happend if the input is directly from the machine).
        /// </summary>
        /// <param name="country">Selected country.</param>
        /// <param name="input">Input char mapping in Z_30.</param>
        /// <returns>Latin char mapping.</returns>
        public static char getPrinterCharLatin(FialkaEnums.countryLayout country, FialkaEnums.printHeadShift shiftMode, int input)
        {
            char output = FialkaConstants.InvalidInputPrintSymbol;
            switch (country)
            {
                case FialkaEnums.countryLayout.Czechoslovakia:
                    {
                        switch (shiftMode)
                        {
                            case FialkaEnums.printHeadShift.LetterShift:
                                {
                                    output = keyboardCzechoslovakianLet[input];
                                }
                                break;
                            case FialkaEnums.printHeadShift.NumberShift:
                                {
                                    output = keyboardCzechoslovakianSpec[input];
                                }
                                break;
                        }
                    }
                    break;
                case FialkaEnums.countryLayout.GDR:
                    {
                        switch (shiftMode)
                        {
                            case FialkaEnums.printHeadShift.LetterShift:
                                {
                                    output = keyboardGDRLet[input];
                                }
                                break;
                            case FialkaEnums.printHeadShift.NumberShift:
                                {
                                    output = keyboardGDRSpec[input];
                                }
                                break;
                        }
                    }
                    break;
                case FialkaEnums.countryLayout.Poland:
                    {
                        switch (shiftMode)
                        {
                            case FialkaEnums.printHeadShift.LetterShift:
                                {
                                    output = keyboardPolandLet[input];
                                }
                                break;
                            case FialkaEnums.printHeadShift.NumberShift:
                                {
                                    output = keyboardPolandSpec[input];
                                }
                                break;
                        }
                    }
                    break;
            }
            return output;
        }

        #endregion

        #region NumLock10 stuff
        // Fialka ref. manual p. 116 - reflector
        public static readonly int[] reflectorActiveContactsNumLock10 = new[] { 8, 9, 10, 12, 15, 17, 21, 23, 25, 29 }; // 12,15,17,23 part of the "magic circuit"
        // Fialka ref. manual p. 108 - punch card reader circuit
        public static readonly int[] keyboardActiveContactsNumLock10 = new[] { 1, 3, 6, 7, 13, 16, 17, 22, 23, 24 };
        // additional substitutions when looping (-1 are contacts that are directly connected, so never called in this permutation)
        public static readonly int[] reflectorToPunchCardNumLock10 = new int[] { 9, 28, 8, 2, 27, 20, 21, 15, -1, -1, -1, 12, -1, 0, 19, -1, 4, -1, 29, 25, 5, -1, 18, -1, 14, -1, 11, 26, 10, -1 };
        public static readonly int[] reflectorToPunchCardNumLock10Inverse = new int[] { 13, -1, 3, -1, 16, 20, -1, -1, 2, 0, 28, 26, 11, -1, 24, 7, -1, -1, 22, 14, 5, 6, -1, -1, -1, 19, 27, 4, 1, 18 };

        #endregion

    }
}
