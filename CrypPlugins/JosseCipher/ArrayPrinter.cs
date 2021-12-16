using System;
using System.Linq;

namespace CrypTool.JosseCipher
{
    internal class ArrayPrinter
    {
        #region Declarations

        private static readonly bool isLeftAligned = false;
        private const string CellLeftTop = "┌";
        private const string CellRightTop = "┐";
        private const string CellLeftBottom = "└";
        private const string CellRightBottom = "┘";
        private const string CellHorizontalJointTop = "┬";
        private const string CellHorizontalJointbottom = "┴";
        private const string CellVerticalJointLeft = "├";
        private const string CellTJoint = "┼";
        private const string CellVerticalJointRight = "┤";
        private const string CellHorizontalLine = "─";
        private const string CellVerticalLine = "│";

        #endregion

        #region Private Methods

        private static int[] GetMaxCellWidthForColumns(string[,] arrValues)
        {
            int[] maxWidth = new int[arrValues.GetLength(1)];

            for (int i = 0; i < arrValues.GetLength(0); i++)
            {
                for (int j = 0; j < arrValues.GetLength(1); j++)
                {
                    int length = string.IsNullOrEmpty(arrValues[i, j]) ? 0 : arrValues[i, j].Length;
                    if (length > maxWidth[j])
                    {
                        maxWidth[j] = length;
                    }
                }
            }

            return maxWidth;
        }

        private static string GetDataInTableFormat(string[,] arrValues)
        {
            string formattedString = string.Empty;

            if (arrValues == null)
            {
                return formattedString;
            }

            int dimension1Length = arrValues.GetLength(0);
            int dimension2Length = arrValues.GetLength(1);

            int[] maxCellWidth = GetMaxCellWidthForColumns(arrValues);
            int indentLength = maxCellWidth.Sum() + (dimension2Length - 1);
            //printing top line;
            formattedString = $"{CellLeftTop}{Indent(indentLength)}{CellRightTop}{Environment.NewLine}";

            for (int i = 0; i < dimension1Length; i++)
            {
                string lineWithValues = CellVerticalLine;
                string line = CellVerticalJointLeft;
                for (int j = 0; j < dimension2Length; j++)
                {
                    string value = PadBoth(arrValues[i, j], maxCellWidth[j]);

                    lineWithValues += $"{value}{CellVerticalLine}";
                    line += Indent(maxCellWidth[j]);
                    if (j < dimension2Length - 1)
                    {
                        line += CellTJoint;
                    }
                }
                line += CellVerticalJointRight;
                formattedString += $"{lineWithValues}{Environment.NewLine}";
                if (i < dimension1Length - 1)
                {
                    formattedString += $"{line}{Environment.NewLine}";
                }
            }

            //printing bottom line
            formattedString += $"{CellLeftBottom}{Indent(indentLength)}{CellRightBottom}{Environment.NewLine}";
            return formattedString;
        }

        private static string Indent(int count)
        {
            return string.Empty.PadLeft(count, '─');
        }

        public static string PadBoth(string source, int length)
        {
            if (string.IsNullOrEmpty(source))
            {
                return new string(' ', length);
            }

            int spaces = length - source.Length;
            int padLeft = spaces / 2 + source.Length;
            return source.PadLeft(padLeft).PadRight(length);

        }

        #endregion

        #region Public Methods

        public static string PrintToConsole(string[,] arrValues)
        {
            //remark [row, column]
            return arrValues == null ? string.Empty : GetDataInTableFormat(arrValues);
        }

        #endregion
    }
}