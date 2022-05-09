/*                              
   Copyright 2022 CrypToolTeam

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
using System.Collections.Generic;

namespace TranspositionAnalyser
{
    internal class PermutationGenerator
    {
        private readonly int[] a;
        private long numLeft;
        private readonly long total;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="n">n < 20 </param>
        public PermutationGenerator(int n)
        {
            a = new int[n];
            total = getFactorial(n);
            reset();
        }

        /// <summary>
        /// Reset
        /// </summary>
        public void reset()
        {
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = i + 1;
            }
            numLeft = total;
        }

        /// <summary>
        /// Return number of permutations not yet generated
        /// </summary>
        public long getNumLeft()
        {
            return numLeft;
        }

        /// <summary>
        /// Return total number of permutations
        /// </summary>
        public long getTotal()
        {
            return total;
        }

        /// <summary>
        /// Are there more permutations?
        /// </summary>
        public bool hasMore()
        {
            long nu = 0;
            return !(numLeft.Equals(nu));
        }

        public long getFactorial(int n)
        {
            long fact = 1;
            for (int i = n; i > 1; i--)
            {
                fact = fact * i;
            }
            return fact;
        }


        public int[] getNext()
        {

            if (numLeft.Equals(total))
            {
                numLeft = numLeft - 1;
                return a;
            }

            int temp;

            // Find largest index j with a[j] < a[j+1]

            int j = a.Length - 2;
            while (a[j] > a[j + 1])
            {
                j--;
            }

            // Find index k such that a[k] is smallest integer
            // greater than a[j] to the right of a[j]

            int k = a.Length - 1;
            while (a[j] > a[k])
            {
                k--;
            }

            // Interchange a[j] and a[k]

            temp = a[k];
            a[k] = a[j];
            a[j] = temp;

            // Put tail end of permutation after jth position in increasing order

            int r = a.Length - 1;
            int s = j + 1;

            while (r > s)
            {
                temp = a[s];
                a[s] = a[r];
                a[r] = temp;
                r--;
                s++;
            }

            numLeft = numLeft - 1;
            return a;

        }

        private List<List<int>> listohneein;
        public List<List<int>> returnlogicper(int[,] key)
        {
            List<List<int>> listlist = erzeugen(key);
            List<int> listxein1 = xein1(erzeugen(key));
            List<int> list = rest(listxein1);
            listohneein = ohneein(erzeugen(key));
            int[] x = new int[list.Count];

            for (int i = 0; i < list.Count; i++)
            {
                x[i] = list[i];

            }

            List<List<int>> allpermute = allper(x);

            List<List<int>> finalkey1 = finalkey(allpermute, listohneein);
            List<List<int>> final1 = finale(finalkey1, listxein1);
            return final1;
        }

        private List<List<int>> finale(List<List<int>> finalkey1, List<int> listxein)
        {
            List<List<int>> keysum = new List<List<int>>();
            for (int i = 0; i < finalkey1.Count; i++)
            {
                List<int> dummy = new List<int>();
                for (int ix = 0; ix < listxein.Count; ix++)
                {
                    dummy.Add(listxein[ix]);
                }
                for (int ix = 0; ix < dummy.Count; ix++)
                {
                    Console.Write(listxein[ix]);
                    if (dummy[ix] == 0)
                    {
                        dummy[ix] = finalkey1[i][0];
                        finalkey1[i].RemoveAt(0);
                    }
                }
                //Console.WriteLine();
                keysum.Add(dummy);
            }
            return keysum;
        }

        private List<List<int>> finalkey(List<List<int>> allpermute, List<List<int>> listohneein)
        {
            for (int i = 0; i < allpermute.Count; i++)
            {
                bool b = false;
                for (int ix = 0; ix < allpermute[i].Count; ix++)
                {
                    if (!listohneein[ix].Contains(allpermute[i][ix]))
                    {
                        // Console.Write(listohneein[ix][0]);
                        //Console.WriteLine(allpermute[i][ix]);
                        b = true;
                    }
                }

                if (b)
                {
                    allpermute.RemoveAt(i);
                    i--;
                }
            }
            return allpermute;
        }

        private List<List<int>> allper(int[] x)
        {

            List<List<int>> allpermute = new List<List<int>>();

            for (int i = 0; i < fak(x.Length); i++)
            {
                bool b = false;
                allpermute.Add(new List<int>());
                for (int ix = 0; ix < x.Length; ix++)
                {
                    allpermute[allpermute.Count - 1].Add(x[ix]);
                }

                for (int ix = 0; ix < allpermute[allpermute.Count - 1].Count; ix++)
                {
                    if (!listohneein[ix].Contains(allpermute[allpermute.Count - 1][ix]))
                    {
                        //GuiLogMessage("You have to connect the Transposition Plugin to the Transpostion Analyzer Control!", NotificationLevel.Warning);
                        // Console.Write(listohneein[ix][0]);
                        //Console.WriteLine(allpermute[i][ix]);
                        b = true;
                    }
                }
                if (b)
                {
                    if (allpermute.Count > 0)
                    {
                        allpermute.RemoveAt(allpermute.Count - 1);
                    }
                }
                nextLexiPermutation(x);
            }
            return allpermute;
        }
        private int fak(int a)
        {
            int b = 1;
            for (int i = 1; i <= a; i++)
            {
                b = b * i;
            }

            return b;
        }

        private List<List<int>> xein(List<List<int>> listlist1)
        {
            List<int> list = new List<int>
            {
                0
            };
            for (int i = 0; i < listlist1.Count; i++)
            {
                if (listlist1[i].Count > 1)
                {
                    listlist1[i] = list;
                }

            }
            return listlist1;

        }

        private List<int> xein1(List<List<int>> listlist1)
        {
            List<int> list = new List<int>();

            for (int i = 0; i < listlist1.Count; i++)
            {
                if (listlist1[i].Count > 1)
                {
                    list.Add(0);
                }
                else { list.Add(listlist1[i][0]); }
            }
            return list;

        }

        private List<List<int>> ohneein(List<List<int>> listlist2)
        {
            for (int i = 0; i < listlist2.Count; i++)
            {
                if (listlist2[i].Count == 1)
                {
                    listlist2.RemoveAt(i);
                    i--;
                }

            }
            return listlist2;

        }

        private List<List<int>> erzeugen(int[,] key)
        {
            List<List<int>> listlist = new List<List<int>>();
            for (int i = 0; i < key.GetLength(0); i++)
            {
                List<int> list = new List<int>();

                for (int ix = 0; ix < key.GetLength(1); ix++)
                {
                    if (key[i, ix] != 0)
                    {
                        list.Add(key[i, ix]);
                    }

                }
                listlist.Add(list);
            }
            return listlist;
        }

        private List<int> rest(List<int> key)
        {
            List<int> list = new List<int>();
            for (int i = 1; i <= key.Count; i++)
            {
                bool b = true;
                for (int ix = 0; ix < key.Count; ix++)
                {
                    if (i == key[ix])
                    {
                        b = false;
                    }
                }
                if (b)
                {
                    list.Add(i);
                }
            }
            return list;

        }

        private int[] nextLexiPermutation(int[] s)
        {
            int i = -1, j = 0;

            for (int x = s.Length - 2; x >= 0; x--)
            {
                if (s[x] < s[x + 1])
                {
                    i = x;
                    break;
                }
            }

            if (-1 == i)
            {
                return null;
            }

            for (int x = s.Length - 1; x > i; x--)
            {
                if (s[x] > s[i])
                {
                    j = x;
                    break;
                }
            }

            // Swapping elements pointed by i and j;
            int temp = s[i];
            s[i] = s[j];
            s[j] = temp;

            // Reversing elements after i
            Array.Reverse(s, i + 1, s.Length - (i + 1));
            return s;
        }

    }



}
