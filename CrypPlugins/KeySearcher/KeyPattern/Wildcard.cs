/*                              
   Copyright 2009 Sven Rech (svenrech at googlemail dot com), Uni Duisburg-Essen

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

using System;
namespace KeySearcher.KeyPattern
{
    internal class Wildcard
    {
        private readonly char[] values = new char[256];
        private int length;
        private int counter;
        public bool isSplit
        {
            get;
            private set;
        }

        public Wildcard(string valuePattern, Wildcard referenceWildcard = null)
        {
            isSplit = false;
            counter = 0;
            if (valuePattern.Length == 1)
            {
                length = 1;
                values[0] = valuePattern[0];
            }
            else
            {
                length = 0;
                int i = 1;
                while (valuePattern[i] != ']')
                {
                    if (valuePattern[i + 1] == '-')
                    {
                        char startChar = valuePattern[i];
                        char endChar = valuePattern[i + 2];
                        if (startChar > endChar)
                        {
                            throw new Exception("Invalid wildcard format!");
                        }

                        if (referenceWildcard != null)
                        {
                            TakeWildcardRangeFromReference(referenceWildcard, startChar, endChar);
                        }
                        else
                        {
                            for (char c = startChar; c <= endChar; c++)
                            {
                                values[length++] = c;
                            }
                        }

                        i += 2;
                    }
                    else
                    {
                        values[length++] = valuePattern[i];
                    }
                    i++;
                }
            }

            Array.Sort(values, 0, length);
        }

        private void TakeWildcardRangeFromReference(Wildcard referenceWildcard, char startChar, char endChar)
        {
            bool startFound = false;
            bool endFound = false;
            for (int ri = 0; ri < referenceWildcard.length; ri++)
            {
                char currentRC = referenceWildcard.values[ri];
                if (!startFound && currentRC == startChar)
                {
                    startFound = true;
                }
                if (startFound && !endFound)
                {
                    values[length++] = currentRC;
                }
                if (currentRC == endChar)
                {
                    endFound = true;
                    break;
                }
            }

            if (!startFound || !endFound)
            {
                throw new Exception("Invalid wildcard format with respect to reference wildcard!");
            }
        }

        public Wildcard(Wildcard wc)
        {
            isSplit = wc.isSplit;
            length = wc.length;
            counter = wc.counter;
            for (int i = 0; i < 256; i++)
            {
                values[i] = wc.values[i];
            }
        }

        public Wildcard(char[] values, int length)
        {
            isSplit = false;
            this.length = length;
            this.values = values;
            counter = 0;
        }

        private Wildcard()
        {
        }

        public Wildcard[] split()
        {
            if (length <= 1)
            {
                return null;
            }

            int length1 = length - counter;
            Wildcard[] wcs = new Wildcard[2];
            wcs[0] = new Wildcard
            {
                counter = 0,
                length = length1 / 2
            };
            wcs[1] = new Wildcard
            {
                counter = 0,
                length = length1 - wcs[0].length
            };
            for (int i = 0; i < wcs[0].length; i++)
            {
                wcs[0].values[i] = values[counter + i];
            }

            for (int i = 0; i < wcs[1].length; i++)
            {
                wcs[1].values[i] = values[i + counter + wcs[0].length];
            }

            wcs[0].isSplit = true;
            wcs[1].isSplit = true;
            return wcs;
        }

        public char getChar()
        {
            return values[counter];
        }

        public char getChar(int add)
        {
            return values[(counter + add) % length];
        }

        public bool succ()
        {
            counter++;
            if (counter >= length)
            {
                counter = 0;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds "add" to the counter and returns the carry.
        /// </summary>
        /// <param name="add">The carry</param>
        /// <returns></returns>
        public int add(int add)
        {
            counter += add;
            if (counter >= length)
            {
                int result = counter / length;
                counter %= length;
                return result;
            }
            return 0;
        }

        public int size()
        {
            return length;
        }

        public int count()
        {
            return counter;
        }

        public void resetCounter()
        {
            counter = 0;
        }

        public string getRepresentationString()
        {
            if (length == 1)
            {
                return "" + values[0];
            }

            string res = "[";
            int begin = 0;
            for (int i = 1; i < length; i++)
            {
                if (values[i - 1] != values[i] - 1)
                {
                    if (begin == i - 1)
                    {
                        res += values[begin];
                    }
                    else
                    {
                        if (i - 1 - begin == 1)
                        {
                            res += values[begin] + "" + values[i - 1];
                        }
                        else
                        {
                            res += values[begin] + "-" + values[i - 1];
                        }
                    }
                    begin = i;
                }
            }
            if (begin == length - 1)
            {
                res += values[begin];
            }
            else
            {
                if (length - 1 - begin == 1)
                {
                    res += values[begin] + "" + values[length - 1];
                }
                else
                {
                    res += values[begin] + "-" + values[length - 1];
                }
            }

            res += "]";
            return res;
        }

        public bool contains(Wildcard wc)
        {
            if (wc == null)
            {
                return false;
            }

            for (int i = 0; i < wc.length; i++)
            {
                bool contains = false;
                for (int j = 0; j < length; j++)
                {
                    if (values[j] == wc.values[i])
                    {
                        contains = true;
                        break;
                    }
                }
                if (!contains)
                {
                    return false;
                }
            }
            return true;
        }

        internal int getLength()
        {
            return length;
        }

        internal char[] getChars()
        {
            return values;
        }
    }
}
