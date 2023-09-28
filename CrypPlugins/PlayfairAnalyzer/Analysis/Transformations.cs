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

namespace PlayfairAnalysis
{
    public class Transformations
    {
        private readonly AnalysisInstance instance;
        private readonly Utils utils;

        private static readonly bool T_SWAP_2_POSITIONS = true;
        private static readonly bool T_SWAP_3_POSITIONS = false; // Not effective, and too many!.
        private static readonly bool T_SWAP_2_ROWS_OR_2_COLS = true;
        private static readonly bool T_PERM_ALL_ROWS_OR_ALL_COLS = true;
        private static readonly bool T_PERM_1_ROW_OR_1_COL = true; // Not sure it really helps.


        private static readonly int T_SWAP_2_POSITIONS_START = 0;
        private static readonly int T_SWAP_2_POSITIONS_END = T_SWAP_2_POSITIONS_START + (T_SWAP_2_POSITIONS ? Playfair.SQUARE * Playfair.SQUARE : 0);

        private static readonly int T_SWAP_2_ROWS_START = T_SWAP_2_POSITIONS_END;
        private static readonly int T_SWAP_2_ROWS_END = T_SWAP_2_ROWS_START + (T_SWAP_2_ROWS_OR_2_COLS ? Playfair.SQUARE : 0);

        private static readonly int T_SWAP_2_COLS_START = T_SWAP_2_ROWS_END;
        private static readonly int T_SWAP_2_COLS_END = T_SWAP_2_COLS_START + (T_SWAP_2_ROWS_OR_2_COLS ? Playfair.SQUARE : 0);

        private static readonly int T_PERM_ROWS_START = T_SWAP_2_COLS_END;
        private static readonly int T_PERM_ROWS_END = T_PERM_ROWS_START + (T_PERM_ALL_ROWS_OR_ALL_COLS ? Playfair.PERMUTATIONS.GetLength(0) : 0);

        private static readonly int T_PERM_COLS_START = T_PERM_ROWS_END;
        private static readonly int T_PERM_COLS_END = T_PERM_COLS_START + (T_PERM_ALL_ROWS_OR_ALL_COLS ? Playfair.PERMUTATIONS.GetLength(0) : 0);

        private static readonly int T_SWAP_3_POSITIONS_START = T_PERM_COLS_END;
        private static readonly int T_SWAP_3_POSITIONS_END = T_SWAP_3_POSITIONS_START + (T_SWAP_3_POSITIONS ? Playfair.SQUARE * Playfair.SQUARE * Playfair.SQUARE : 0);

        private static readonly int T_PERM_ROWS_OF_1_COL_START = T_SWAP_3_POSITIONS_END;
        private static readonly int T_PERM_ROWS_OF_1_COL_END = T_PERM_ROWS_OF_1_COL_START + (T_PERM_1_ROW_OR_1_COL ? Playfair.PERMUTATIONS.GetLength(0) * Playfair.DIM : 0);

        private static readonly int T_PERM_COLS_OF_1_ROW_START = T_PERM_ROWS_OF_1_COL_END;
        private static readonly int T_PERM_COLS_OF_1_ROW_END = T_PERM_COLS_OF_1_ROW_START + (T_PERM_1_ROW_OR_1_COL ? Playfair.PERMUTATIONS.GetLength(0) * Playfair.DIM : 0);

        private static readonly int TOTAL_NUMBER_OF_TRANSFORMATIONS = T_PERM_COLS_OF_1_ROW_END;

        private readonly int[] TRANSFORMATIONS = Allocate();

        public Transformations(AnalysisInstance instance, Utils utils)
        {
            this.instance = instance;
            this.utils = utils;
        }       

        private static int[] Allocate()
        {
            int[] map = new int[TOTAL_NUMBER_OF_TRANSFORMATIONS];
            for (int i = 0; i < TOTAL_NUMBER_OF_TRANSFORMATIONS; i++)
            {
                map[i] = i;
            }
            return map;
        }

        public void Randomize()
        {
            for (int i = 0; i < TOTAL_NUMBER_OF_TRANSFORMATIONS - 1; i++)
            {
                int j = i + utils.RandomNextInt(TOTAL_NUMBER_OF_TRANSFORMATIONS - i);
                int keep = TRANSFORMATIONS[i];
                TRANSFORMATIONS[i] = TRANSFORMATIONS[j];
                TRANSFORMATIONS[j] = keep;
            }
        }

        public void Apply(Key parent, Key child, long serialFull)
        {
            int serial = (int)(serialFull % TOTAL_NUMBER_OF_TRANSFORMATIONS);
            if (serial < 0)
            {
                serial = (-serial) % TOTAL_NUMBER_OF_TRANSFORMATIONS;
            }
            serial = TRANSFORMATIONS[serial];

            parent.ComputeInverse();

            if (T_PERM_1_ROW_OR_1_COL && serial >= T_PERM_COLS_OF_1_ROW_START && serial < T_PERM_COLS_OF_1_ROW_END)
            {
                serial -= T_PERM_COLS_OF_1_ROW_START;
                int perm = serial / Playfair.DIM;
                int r = serial % Playfair.DIM;
                child.PermuteRowColumns(parent, r, perm);
                return;
            }
            if (T_PERM_1_ROW_OR_1_COL && serial >= T_PERM_ROWS_OF_1_COL_START && serial < T_PERM_ROWS_OF_1_COL_END)
            {
                serial -= T_PERM_ROWS_OF_1_COL_START;
                int perm = serial / Playfair.DIM;
                int c = serial % Playfair.DIM;
                child.PermuteColumnsRows(parent, c, perm);
                return;
            }
            if (T_SWAP_3_POSITIONS && serial >= T_SWAP_3_POSITIONS_START && serial < T_SWAP_3_POSITIONS_END)
            {
                serial -= T_SWAP_3_POSITIONS_START;
                int p1 = serial % Playfair.SQUARE;
                serial /= Playfair.SQUARE;
                int p2 = serial % Playfair.SQUARE;
                if (p1 == p2)
                {
                    child.Copy(parent);
                    return;
                }
                serial /= Playfair.SQUARE;
                int p3 = serial % Playfair.SQUARE;
                if (p3 == p1 || p3 == p2)
                {
                    child.Copy(parent);
                    return;
                }
                child.Swap(parent, p1, p2, p3);
                return;
            }

            if (T_PERM_ALL_ROWS_OR_ALL_COLS && serial >= T_PERM_COLS_START && serial < T_PERM_COLS_END)
            {
                serial -= T_PERM_COLS_START;
                child.OermuteColumns(parent, serial);
                return;
            }
            if (T_PERM_ALL_ROWS_OR_ALL_COLS && serial >= T_PERM_ROWS_START && serial < T_PERM_ROWS_END)
            {
                serial -= T_PERM_ROWS_START;
                child.PermuteRows(parent, serial);
                return;
            }

            if (T_SWAP_2_ROWS_OR_2_COLS && serial >= T_SWAP_2_COLS_START && serial < T_SWAP_2_COLS_END)
            {
                serial -= T_SWAP_2_COLS_START;
                int c1 = serial % Playfair.DIM;
                serial /= Playfair.SQUARE;
                int c2 = serial % Playfair.DIM;
                if (c1 == c2)
                {
                    child.Copy(parent);
                    return;
                }
                child.SwapColumns(parent, c1, c2);
                return;
            }
            if (T_SWAP_2_ROWS_OR_2_COLS && serial >= T_SWAP_2_ROWS_START && serial < T_SWAP_2_ROWS_END)
            {
                serial -= T_SWAP_2_ROWS_START;
                int r1 = serial % Playfair.DIM;
                serial /= Playfair.SQUARE;
                int r2 = serial % Playfair.DIM;
                if (r1 == r2)
                {
                    child.Copy(parent);
                    return;
                }
                child.SwapRows(parent, r1, r2);
                return;
            }

            if (T_SWAP_2_POSITIONS & serial >= T_SWAP_2_POSITIONS_START && serial < T_SWAP_2_POSITIONS_END)
            {
                serial -= T_SWAP_2_POSITIONS_START;
                int p1 = serial % Playfair.SQUARE;
                serial /= Playfair.SQUARE;
                int p2 = serial % Playfair.SQUARE;
                if (p1 == p2)
                {
                    child.Copy(parent);
                    return;
                }
                child.Swap(parent, p1, p2);
                return;
            }
        }
    }
}
