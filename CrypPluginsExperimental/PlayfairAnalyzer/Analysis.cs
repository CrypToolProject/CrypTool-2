using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrypTool.Plugins.PlayfairAnalyzer
{
    class Analysis
    {
        private PlayfairAnalyzer solver;

        public Analysis(PlayfairAnalyzer solver)
        {
            this.solver = solver;
        }

        public void main(String[] args)
        {
            changeAnalysis();
        }

        private int analyzeEffectOfTransformationOnBigrams(Key parent, Key child, String desc)
        {
            byte[] cipherText = new byte[2000];
            int cipherTextLength = 0;
            for (byte i = 0; i < solver.SQUARE; i++)
            {
                for (byte j = 0; j < solver.SQUARE; j++)
                {
                    if (i == j) continue;
                    cipherText[cipherTextLength++] = i;
                    cipherText[cipherTextLength++] = j;
                }
            }

            byte[] plainTextBefore = new byte[cipherTextLength];
            int plainTextLength1 = solver.decrypt(cipherText, cipherTextLength, plainTextBefore, false, parent);

            byte[] plainTextAfter = new byte[cipherTextLength];
            int plainTextLength2 = solver.decrypt(cipherText, cipherTextLength, plainTextAfter, false, child);

            int diff = 0;
            for (int i = 0; i < Math.Min(plainTextLength1, plainTextLength2); i++)
                if (plainTextAfter[i] != plainTextBefore[i])
                    diff++;

            diff += Math.Max(plainTextLength1, plainTextLength2) - Math.Min(plainTextLength1, plainTextLength2);
            solver.GuiLogMessage(String.Format("{0}\n{1}{2}: {3}/{4}\n", parent, child, desc, diff, cipherTextLength), PluginBase.NotificationLevel.Debug);
            return diff;
        }

        private void changeAnalysis()
        {
            Key child = new Key(solver);
            Key parent = new Key(solver);

            parent.simple();

            parent.copy(child);
            child.swap(2, 4);
            child.swap(2 + 10, 4 + 10);
            analyzeEffectOfTransformationOnBigrams(parent, child, "Swap 4 corners");

            parent.copy(child);
            child.swap(2, 4);
            analyzeEffectOfTransformationOnBigrams(parent, child, "Swap two same row");

            parent.copy(child);
            child.swap(15, 15 + 1);
            child.swap(15 + 1, 15 + 2);
            child.swap(15 + 2, 15 + 3);
            child.swap(15 + 3, 15 + 4);
            child.swap(15 + 4, 15);
            analyzeEffectOfTransformationOnBigrams(parent, child, "rotate row 3");

            parent.copy(child);
            child.swap(2, 2 + solver.DIM);
            analyzeEffectOfTransformationOnBigrams(parent, child, "Swap two same col");

            parent.copy(child);
            child.swap(2, 9);
            analyzeEffectOfTransformationOnBigrams(parent, child, "Swap two any");

            parent.swapCols(child, 1, 3);
            analyzeEffectOfTransformationOnBigrams(parent, child, "Swap two cols");

            parent.swapRows(child, 1, 3);
            analyzeEffectOfTransformationOnBigrams(parent, child, "Swap two rows");

            parent.swapRows(child, 2, 3);
            analyzeEffectOfTransformationOnBigrams(parent, child, "Swap two adjacent rows");

            parent.swapCols(child, 2, 3);
            analyzeEffectOfTransformationOnBigrams(parent, child, "Swap two adjacent columns");

            parent.copy(child);
            for (int c = 0; c < solver.DIM; c++)
            {
                byte temp = child.key[c];
                child.key[c] = child.key[solver.DIM + c];
                child.key[solver.DIM + c] = child.key[2 * solver.DIM + c];
                child.key[2 * solver.DIM + c] = temp;
            }

            analyzeEffectOfTransformationOnBigrams(parent, child, "Slide two adjacent rows by 1");

            for (int p = 0; p < solver.permutations.Length; p++)
            {
                parent.permuteCols(child, p);
                analyzeEffectOfTransformationOnBigrams(parent, child, "Permute Cols: " + p);
                parent.permuteRows(child, p);
                analyzeEffectOfTransformationOnBigrams(parent, child, "Permute Rows: " + p);
            }
        }
    }
}