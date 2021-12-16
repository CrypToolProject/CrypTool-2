using System;
using System.Text;

namespace common
{
    public abstract class Vector
    {

        public int alphabetSize;
        public int[] TextInInt;
        public int length;
        public int cursor;

        public bool withStats;
        public bool acceptErrors = false;

        private readonly StringBuilder alphabet;

        //calculation of IoC with monograms
        public long[] counts1;
        public double[] freqs1;
        public double IoC1 = 0.0;
        //calculation of IoC with bigrams
        private readonly long[,] counts2;
        private readonly double[,] freqs2;
        public double IoC2 = 0.0;



        public static Random r = new Random();

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                s.Append("").Append(Chr(TextInInt[i]));
            }
            return s.ToString();
        }

        public Vector(StringBuilder alphabet, string s, bool withStats)

           : this(alphabet, s.Length, withStats)
        {
            for (int i = 0; i < length; i++)
            {
                TextInInt[i] = Index(s[i]);
                if (TextInInt[i] == -1)
                {
                    //Console.WriteLine("Bad %d = %s\n", i, s[i]);
                }
            }
            stats();
        }

        public Vector(StringBuilder alphabet, int length, bool withStats)
        {
            this.alphabet = alphabet;
            this.length = length;
            this.withStats = withStats;
            alphabetSize = alphabet.Length;
            TextInInt = new int[length];
            if (this.withStats)
            {
                counts1 = new long[alphabetSize];
                freqs1 = new double[alphabetSize];
                counts2 = new long[alphabetSize, alphabetSize];
                freqs2 = new double[alphabetSize, alphabetSize];
            }
            if (length == alphabetSize)
            {
                Identity();
            }
        }

        public int Index(char c)
        {
            return alphabet.ToString().IndexOf(c);
        }

        public char Chr(int index)
        {
            return (index >= 0 && index < alphabetSize) ? alphabet[index] : '!';
        }

        public void Swap(int i, int j)
        {
            if (i == j)
            {
                return;
            }
            int keep = TextInInt[i];
            TextInInt[i] = TextInInt[j];
            TextInInt[j] = keep;
        }

        public void Identity()
        {
            for (int i = 0; i < length; i++)
            {
                TextInInt[i] = i;
            }
        }

        public Vector FromTranspositionKey(string keyS)
        {

            if (keyS.Length > TextInInt.Length)
            {
                //Console.WriteLine("Cannot create transposition key; adding now Z at the end");
                while (keyS.Length > TextInInt.Length)
                {
                    keyS = keyS + "Z";
                }
            }

            for (int i = 0; i < TextInInt.Length; i++)
            {
                TextInInt[i] = -1;
            }
            //Arrays.fill(v, -1);

            length = keyS.Length;

            for (int i = 0; i < length; i++)
            {
                int minJ = -1;
                for (int j = 0; j < length; j++)
                {
                    if (TextInInt[j] != -1)
                    {
                        continue;
                    }
                    if ((minJ == -1) || (keyS[j] < keyS[minJ]))
                    {
                        minJ = j;
                    }
                }
                TextInInt[minJ] = i;

            }
            return this;
        }

        public Vector randomPermutation()
        {
            Identity();
            for (int i = 0; i < length - 1; i++)
            {
                int j = i + r.Next(length - i);
                Swap(i, j);
            }
            return this;
        }

        public void append(Vector v)
        {
            Array.Copy(v.TextInInt, 0, TextInInt, length, v.length);
            length += v.length;
        }

        public void append(params Vector[] TextInInts)
        {
            foreach (Vector TextInInt in TextInInts)
            {
                append(TextInInt);
            }
        }

        public Vector copy(Vector v)
        {
            length = 0;
            append(v);
            return this;
        }

        public Vector copy(Vector v, int from, int length)
        {
            Array.Copy(v.TextInInt, from, TextInInt, 0, length);
            this.length = length;
            return this;
        }

        public void copy(params Vector[] TextInInts)
        {
            length = 0;
            append(TextInInts);
        }

        public bool copy(string s)
        {
            length = 0;
            return append(s);
        }

        public bool copy(params string[] strings)
        {
            length = 0;
            return append(strings);
        }

        public bool append(string s)
        {
            return append(s.ToCharArray());
        }

        public bool append(params string[] strings)
        {
            foreach (string s in strings)
            {
                if (!append(s))
                {
                    return false;
                }
            }
            return true;
        }

        public bool copy(char c)
        {
            length = 0;
            return append(c);
        }

        public bool copy(params char[] chars)
        {
            length = 0;
            return append(chars);
        }

        public bool append(char c)
        {
            int i = Index(c);
            if (i == -1 && !acceptErrors)
            {
                return false;
            }
            TextInInt[length++] = i;
            return true;
        }

        public bool append(params char[] chars)
        {
            foreach (char c in chars)
            {
                if (!append(c))
                {
                    return false;
                }
            }
            return true;
        }

        public bool copy(int i)
        {
            length = 0;
            return append(i);
        }

        public bool copy(params int[] ints)
        {
            length = 0;
            return append(ints);
        }

        public bool append(int i)
        {
            TextInInt[length++] = i;
            return true;
        }

        public bool append(params int[] ints)
        {
            foreach (int i in ints)
            {
                if (!append(i))
                {
                    return false;
                }
            }
            return true;
        }

        public bool removeIndex(int index)
        {
            if (index >= length)
            {
                return false;
            }
            else if (index == length - 1)
            {
                length--;
                return true;
            }

            Array.Copy(TextInInt, index + 1, TextInInt, index, length - index - 1);
            length--;
            return true;
        }

        public bool removeElement(int element)
        {
            for (int i = 0; i < length; i++)
            {
                if (TextInInt[i] == element)
                {
                    return removeIndex(i);
                }
            }
            return false;
        }

        public bool removeElement(char element)
        {
            return removeElement(Index(element));
        }

        public bool valid()
        {
            for (int i = 0; i < length; i++)
            {
                if (TextInInt[i] == -1)
                {
                    return false;
                }
            }
            return true;
        }

        public long hash()
        {
            long index = 0;
            for (int i = 0; i < length; i++)
            {
                index = index * alphabetSize + TextInInt[i];
            }
            return index;
        }

        public int hashShift5()
        {
            int index = 0;
            for (int i = 0; i < length; i++)
            {
                index = (index << 5) + TextInInt[i];
            }
            return index;
        }

        public void inverseOf(Vector v)
        {
            if (v.length != alphabetSize)
            {
                //throw new RuntimeException("Length is not standard");
            }
            if (v.length != length)
            {
                throw new System.Exception("Length is not equal");
            }

            for (int i = 0; i < TextInInt.Length; i++)
            {
                TextInInt[i] = -1;
            }


            for (int i = 0; i < length; i++)
            {
                if (v.TextInInt[i] != -1)
                {
                    TextInInt[v.TextInInt[i]] = i;
                }
            }

        }

        public void stats()
        {
            stats(false);
        }

        public void stats(bool bigramEvenOnly)
        {
            if (!withStats)
            {
                return;
            }
            for (int i = 0; i < alphabetSize; i++)
            {
                freqs1[i] = 0.0;
                counts1[i] = 0;
            }

            IoC1 = 0.0;
            IoC2 = 0.0;

            for (int i = 0; i < alphabetSize; i++)
            {
                for (int j = 0; j < alphabetSize; j++)
                {
                    counts2[i, j] = 0;
                }
            }


            for (int i = 0; i < alphabetSize; i++)
            {
                for (int j = 0; j < alphabetSize; j++)
                {
                    freqs2[i, j] = 0.0;
                }
            }


            int lastSymbol = -1;
            for (int i = 0; i < length; i++)
            {
                int symbol = TextInInt[i];
                if (symbol != -1)
                {
                    counts1[symbol]++;
                    if (lastSymbol != -1)
                    {
                        if (bigramEvenOnly)
                        {
                            if (i % 2 == 1)
                            {
                                counts2[lastSymbol, symbol]++;
                                counts2[lastSymbol, symbol]++;
                            }
                        }
                        else
                        {
                            counts2[lastSymbol, symbol]++;
                        }
                    }
                }
                lastSymbol = symbol;
            }

            double freq;
            for (int f1 = 0; f1 < alphabetSize; f1++)
            {
                //freq = freqs1[f1] = 1.0 * counts1[f1] / length;
                //IoC1 += freq * freq;
                freq = freqs1[f1] = counts1[f1] * (counts1[f1] - 1);
                IoC1 += freq;

                //-------------------------------------------------------------------------
                double[] freqs2F1 = new double[freqs2.GetLength(1)];

                for (int i = 0; i < freqs2.GetLength(1); i++)
                {
                    freqs2F1[i] = freqs2[f1, i];
                }

                //-------------------------------------------------------------------------
                long[] counts2F1 = new long[counts2.GetLength(1)];

                for (int i = 0; i < counts2.GetLength(1); i++)
                {
                    counts2F1[i] = counts2[f1, i];
                }

                //-------------------------------------------------------------------------

                for (int f2 = 0; f2 < alphabetSize; f2++)
                {
                    //freq = freqs2F1[f2] = 1.0 * counts2F1[f2] / length;
                    freq = freqs2F1[f2] = counts2F1[f2] * (counts2F1[f2] - 1);
                    if (freq == 0)
                    {
                        continue;
                    }
                    //IoC2 += freq * freq;
                    IoC2 += freq;

                }
            }

            IoC1 = IoC1 / (length * (length - 1));
            IoC2 = IoC2 / (length * (length - 1));
            double dsize = 100;
            IoC1 *= dsize;
            IoC2 *= dsize;
        }

    }

}
