using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrypTool.Plugins.PlayfairAnalyzer
{
    public class Transform
    {
        PlayfairAnalyzer solver;

        private bool T_SWAP_2_POSITIONS = true;
        private bool T_SWAP_3_POSITIONS = false; // Not effective, and too many!.
        private bool T_SWAP_2_ROWS_OR_2_COLS = true;
        private bool T_PERM_ALL_ROWS_OR_ALL_COLS = true;
        private bool T_PERM_1_ROW_OR_1_COL = true; // Not sure it really helps.

        private int T_SWAP_2_POSITIONS_START;
        private int T_SWAP_2_POSITIONS_END;

        private int T_SWAP_2_ROWS_START;
        private int T_SWAP_2_ROWS_END;

        private int T_SWAP_2_COLS_START;
        private int T_SWAP_2_COLS_END;

        private int T_PERM_ROWS_START;
        private int T_PERM_ROWS_END;

        private int T_PERM_COLS_START;
        private int T_PERM_COLS_END;

        private int T_SWAP_3_POSITIONS_START;
        private int T_SWAP_3_POSITIONS_END;

        private int T_PERM_ROWS_OF_1_COL_START;
        private int T_PERM_ROWS_OF_1_COL_END;

        private int T_PERM_COLS_OF_1_ROW_START;
        private int T_PERM_COLS_OF_1_ROW_END;

        private int TOTAL_NUMBER_OF_PERMUTATIONS;

        private int[] REMAPPING;

        public Transform(PlayfairAnalyzer solver)
        {
            this.solver = solver;

            T_SWAP_2_POSITIONS_START = 0;
            T_SWAP_2_POSITIONS_END = T_SWAP_2_POSITIONS_START + (T_SWAP_2_POSITIONS ? solver.SQUARE * solver.SQUARE : 0);

            T_SWAP_2_ROWS_START = T_SWAP_2_POSITIONS_END;
            T_SWAP_2_ROWS_END = T_SWAP_2_ROWS_START + (T_SWAP_2_ROWS_OR_2_COLS ? solver.SQUARE : 0);

            T_SWAP_2_COLS_START = T_SWAP_2_ROWS_END;
            T_SWAP_2_COLS_END = T_SWAP_2_COLS_START + (T_SWAP_2_ROWS_OR_2_COLS ? solver.SQUARE : 0);

            T_PERM_ROWS_START = T_SWAP_2_COLS_END;
            T_PERM_ROWS_END = T_PERM_ROWS_START + (T_PERM_ALL_ROWS_OR_ALL_COLS ? solver.permutations.Length : 0);

            T_PERM_COLS_START = T_PERM_ROWS_END;
            T_PERM_COLS_END = T_PERM_COLS_START + (T_PERM_ALL_ROWS_OR_ALL_COLS ? solver.permutations.Length : 0);

            T_SWAP_3_POSITIONS_START = T_PERM_COLS_END;
            T_SWAP_3_POSITIONS_END = T_SWAP_3_POSITIONS_START + (T_SWAP_3_POSITIONS ? solver.SQUARE * solver.SQUARE * solver.SQUARE : 0);

            T_PERM_ROWS_OF_1_COL_START = T_SWAP_3_POSITIONS_END;
            T_PERM_ROWS_OF_1_COL_END = T_PERM_ROWS_OF_1_COL_START + (T_PERM_1_ROW_OR_1_COL ? solver.permutations.Length * solver.DIM : 0);

            T_PERM_COLS_OF_1_ROW_START = T_PERM_ROWS_OF_1_COL_END;
            T_PERM_COLS_OF_1_ROW_END = T_PERM_COLS_OF_1_ROW_START + (T_PERM_1_ROW_OR_1_COL ? solver.permutations.Length * solver.DIM : 0);

            TOTAL_NUMBER_OF_PERMUTATIONS = T_PERM_COLS_OF_1_ROW_END;

            REMAPPING = initialMap();
        }

        public string printTransformationsCounts()
        {
            return "_______________\n"
                + "Transformations\n"
                + String.Format("Swap 2 positions:         {0:D5}\n", T_SWAP_2_POSITIONS_END - T_SWAP_2_POSITIONS_START)
                + String.Format("Swap 3 positions:         {0:D5}\n", T_SWAP_3_POSITIONS_END - T_SWAP_3_POSITIONS_START)
                + String.Format("Swap 2 rows:              {0:D5}\n", T_SWAP_2_COLS_END - T_SWAP_2_COLS_START)
                + String.Format("Swap 2 columns:           {0:D5}\n", T_SWAP_2_ROWS_END - T_SWAP_2_ROWS_START)
                + String.Format("Permute all rows:         {0:D5}\n", T_PERM_ROWS_END - T_PERM_ROWS_START)
                + String.Format("Permute all columns:      {0:D5}\n", T_PERM_COLS_END - T_PERM_COLS_START)
                + String.Format("Permute rows of 1 column: {0:D5}\n", T_PERM_ROWS_OF_1_COL_END - T_PERM_ROWS_OF_1_COL_START)
                + String.Format("Permute columns of 1 row: {0:D5}\n", T_PERM_COLS_OF_1_ROW_END - T_PERM_COLS_OF_1_ROW_START)
                + String.Format("________________________________\n")
                + String.Format("Total:                    {0:D5}\n", TOTAL_NUMBER_OF_PERMUTATIONS)
                + "\n";
        }

        private int[] initialMap()
        {
            int[] map = new int[TOTAL_NUMBER_OF_PERMUTATIONS];
            for (int i = 0; i < TOTAL_NUMBER_OF_PERMUTATIONS; i++)
                map[i] = i;
            return map;
        }

        public void randomize()
        {
            for (int i = 0; i < TOTAL_NUMBER_OF_PERMUTATIONS - 1; i++)
            {
                int j = i + Utils.random.Next(TOTAL_NUMBER_OF_PERMUTATIONS - i);
                int keep = REMAPPING[i];
                REMAPPING[i] = REMAPPING[j];
                REMAPPING[j] = keep;
            }
        }

        public void applyTransformation(Key parent, Key child, long serialFull)
        {
            int serial = (int)(serialFull % TOTAL_NUMBER_OF_PERMUTATIONS);
            if (serial < 0)
                serial = (-serial) % TOTAL_NUMBER_OF_PERMUTATIONS;

            serial = REMAPPING[serial];

            parent.copy(child);

            if (T_PERM_1_ROW_OR_1_COL && serial >= T_PERM_COLS_OF_1_ROW_START && serial < T_PERM_COLS_OF_1_ROW_END)
            {
                serial -= T_PERM_COLS_OF_1_ROW_START;
                int perm = serial / solver.DIM;
                int r = serial % solver.DIM;
                parent.permuteRowCols(child, r, perm);
                return;
            }

            if (T_PERM_1_ROW_OR_1_COL && serial >= T_PERM_ROWS_OF_1_COL_START && serial < T_PERM_ROWS_OF_1_COL_END)
            {
                serial -= T_PERM_ROWS_OF_1_COL_START;
                int perm = serial / solver.DIM;
                int c = serial % solver.DIM;
                parent.permuteColRows(child, c, perm);
                return;
            }

            if (T_SWAP_3_POSITIONS && serial >= T_SWAP_3_POSITIONS_START && serial < T_SWAP_3_POSITIONS_END)
            {
                serial -= T_SWAP_3_POSITIONS_START;
                int p1 = serial % solver.SQUARE;
                serial /= solver.SQUARE;
                int p2 = serial % solver.SQUARE;
                if (p1 == p2) return;
                serial /= solver.SQUARE;
                int p3 = serial % solver.SQUARE;
                if (p3 == p1 || p3 == p2) return;
                child.swap( p1, p2, p3);
                return;
            }

            if (T_PERM_ALL_ROWS_OR_ALL_COLS && serial >= T_PERM_COLS_START && serial < T_PERM_COLS_END)
            {
                serial -= T_PERM_COLS_START;
                parent.permuteCols(child, serial);
                return;
            }

            if (T_PERM_ALL_ROWS_OR_ALL_COLS && serial >= T_PERM_ROWS_START && serial < T_PERM_ROWS_END)
            {
                serial -= T_PERM_ROWS_START;
                parent.permuteRows(child, serial);
                return;
            }

            if (T_SWAP_2_ROWS_OR_2_COLS && serial >= T_SWAP_2_COLS_START && serial < T_SWAP_2_COLS_END)
            {
                serial -= T_SWAP_2_COLS_START;
                int c1 = serial % solver.DIM;
                serial /= solver.SQUARE;
                int c2 = serial % solver.DIM;
                if (c1 == c2) return;
                parent.swapCols(child, c1, c2);
            }

            if (T_SWAP_2_ROWS_OR_2_COLS && serial >= T_SWAP_2_ROWS_START && serial < T_SWAP_2_ROWS_END)
            {
                serial -= T_SWAP_2_ROWS_START;
                int r1 = serial % solver.DIM;
                serial /= solver.SQUARE;
                int r2 = serial % solver.DIM;
                if (r1 == r2) return;
                parent.swapRows(child, r1, r2);
            }

            if (T_SWAP_2_POSITIONS & serial >= T_SWAP_2_POSITIONS_START && serial < T_SWAP_2_POSITIONS_END)
            {
                serial -= T_SWAP_2_POSITIONS_START;
                int p1 = serial % solver.SQUARE;
                serial /= solver.SQUARE;
                int p2 = serial % solver.SQUARE;
                if (p1 == p2) return;
                child.swap(p1, p2);
            }
        }
    }
}