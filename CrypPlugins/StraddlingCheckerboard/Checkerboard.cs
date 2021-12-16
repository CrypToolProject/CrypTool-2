using CrypTool.StraddlingCheckerboard.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CrypTool.StraddlingCheckerboard
{
    public class Checkerboard
    {
        private readonly Map<(int? row, int column), string> _checkerboard = new Map<(int?, int), string>();
        public List<int?> Rows { get; }
        public List<int> Columns { get; }
        public DataTable TableContent { get; }


        public Checkerboard(string rows, string columns)
        {
            Rows = new List<int?> { null };
            Rows.AddRange(SplitStringToNumbers(rows).Cast<int?>());
            Columns = SplitStringToNumbers(columns);
            TableContent = new DataTable("StraddlingCheckerboard");
            foreach (char column in columns)
            {
                TableContent.Columns.Add(char.ToString(column));
            }
        }

        public string this[string rowString, string columnString]
        {
            get => _checkerboard.Forward[(int.Parse(rowString), int.Parse(columnString))];
            set
            {
                int row = int.Parse(rowString);
                int column = int.Parse(columnString);
                if (!Rows.Contains(row) || !Columns.Contains(column))
                {
                    throw new ArgumentOutOfRangeException(string.Format(Resources.ErrorRowColumnIndexOutOfRange, row,
                        column));
                }

                _checkerboard.Add((row, column), value);
            }
        }

        public string this[string columnString]
        {
            get => _checkerboard.Forward[(null, int.Parse(columnString))];
            set
            {
                int column = int.Parse(columnString);
                if (!Columns.Contains(column))
                {
                    throw new ArgumentOutOfRangeException(string.Format(Resources.ValueOutOfRange, column));
                }

                _checkerboard.Add((null, column), value);
            }
        }

        private static List<int> SplitStringToNumbers(string src)
        {
            return src.Select(digit => int.Parse(digit.ToString())).ToList();
        }

        public bool GetRowAndColumnByChar(string plainChar, out (string row, string column) result)
        {
            try
            {
                (int? row, int column) = _checkerboard.Reverse[plainChar];
                result = row.HasValue
                    ? (row.ToString(), column.ToString())
                    : (string.Empty, column.ToString());
            }
            catch
            {
                result = (null, null);
                return false;
            }

            return true;
        }

        public void FillTable(string content)
        {
            int index = 0;
            for (int i = 0; i < Rows.Count; i++)
            {
                DataRow row = TableContent.NewRow();
                for (int j = 0; j < Columns.Count; j++)
                {
                    if (Rows.Contains(Columns[j]) && i == 0)
                    {
                        continue;
                    }

                    if (index >= content.Length)
                    {
                        TableContent.Rows.Add(row);
                        return;
                    }

                    string charInTable = content[index].ToString();
                    _checkerboard.Add(i == 0 ? (null, Columns[j]) : (Rows[i], Columns[j]), charInTable);
                    row[j] = charInTable;
                    index += 1;
                }

                TableContent.Rows.Add(row);
            }
        }

        private class Map<T1, T2>
        {
            private readonly Dictionary<T1, T2> _forward = new Dictionary<T1, T2>();
            private readonly Dictionary<T2, T1> _reverse = new Dictionary<T2, T1>();

            public Map()
            {
                Forward = new Indexer<T1, T2>(_forward);
                Reverse = new Indexer<T2, T1>(_reverse);
            }

            public class Indexer<T3, T4>
            {
                private readonly Dictionary<T3, T4> _dictionary;

                public Indexer(Dictionary<T3, T4> dictionary)
                {
                    _dictionary = dictionary;
                }

                public T4 this[T3 index]
                {
                    get => _dictionary[index];
                    set => _dictionary[index] = value;
                }
            }

            public void Add(T1 t1, T2 t2)
            {
                _forward.Add(t1, t2);
                _reverse.Add(t2, t1);
            }

            public Indexer<T1, T2> Forward { get; }
            public Indexer<T2, T1> Reverse { get; }
        }
    }
}