using System;
using System.Collections;

namespace IDPAnalyser
{
    public class ValueKey : IComparable, ICloneable
    {
        public byte[] key;
        public double score;
        public byte[] plaintext;
        public string keyphrase;

        public int CompareTo(object obj)
        {
            ValueKey v = (ValueKey)obj;

            int result;

            if ((result = score.CompareTo(v.score)) == 0)
            {
                if ((result = key.Length.CompareTo(v.key.Length)) == 0)
                {
                    for (int i = 0; i < key.Length; i++)
                    {
                        if ((result = key[i].CompareTo(v.key[i])) != 0)
                        {
                            break;
                        }
                    }
                }
            }

            return result;
        }

        public object Clone()
        {
            ValueKey v = new ValueKey
            {
                score = score,
                key = (byte[])key.Clone(),
                keyphrase = (string)keyphrase.Clone(),
                plaintext = (byte[])plaintext.Clone()
            };

            return v;
        }
    }

    public class ValueKeyComparer : IComparer
    {
        private readonly bool sortDescending;

        public ValueKeyComparer(bool sortDescending = true)
        {
            this.sortDescending = sortDescending;
        }

        public int Compare(object x, object y)
        {
            int result = ((ValueKey)x).CompareTo((ValueKey)y);
            return sortDescending ? -result : result;
        }
    }
}
