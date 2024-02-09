/*
   Copyright CrypTool 2 Team josef.matwich@gmail.com

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
using M209AnalyzerLib.Common;
using M209AnalyzerLib.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace M209AnalyzerLib.M209
{
    public class LugsRules
    {

        public bool print = false;
        public List<int[]> validLugCountSequences = null;

        private Key _key;

        public LugsRules(Key key)
        {
            _key = key;
            createValidLugCountSequences();
        }

        private int sameSucc(int[] lugSeq)
        {
            int sameSucc = 0;
            for (int w = 2; w <= Global.WHEELS; w++)
            {
                if (lugSeq[w] == lugSeq[w - 1])
                {
                    sameSucc++;
                }
            }
            return sameSucc;
        }

        private bool tripleSameSucc(int[] lugSeq)
        {
            for (int w = 3; w <= Global.WHEELS; w++)
            {
                if ((lugSeq[w] == lugSeq[w - 1]) && (lugSeq[w - 1] == lugSeq[w - 2]))
                {
                    return true;
                }
            }
            return false;
        }
        private int even(int[] lugSeq)
        {
            int even = 0;
            for (int w = 1; w <= Global.WHEELS; w++)
            {
                if ((lugSeq[w] % 2) == 0)
                {
                    even++;
                }
            }
            return even;
        }
        private int sum(int[] lugSeq)
        {
            int sum = 0;
            for (int w = 1; w <= Global.WHEELS; w++)
            {
                sum += lugSeq[w];
            }
            return sum;
        }

        private bool coverage(int[] lugSeq)
        {
            bool[] cover = new bool[28]; // cover[1] is not used
            for (int comb = 1; comb <= 63; comb++)
            {
                int sumComb = 0;
                for (int w = 1; w <= Global.WHEELS; w++)
                {
                    if (_key.GetWheelBit(comb, w))
                    {
                        sumComb += lugSeq[w];
                    }
                }
                if (sumComb <= Global.BARS)
                {
                    cover[sumComb] = true;
                }
            }
            for (int j = 1; j <= Global.BARS; j++)
            {
                if (!cover[j])
                {
                    return false;
                }
            }
            return true;
        }

        public void createValidLugCountSequences()
        {
            if (validLugCountSequences == null)
            {
                if (Global.FROM_TABLE)
                {
                    validLugCountSequences = new List<int[]>();
                    if (!Global.ONLY_TABLE_GROUP_B)
                    {
                        validLugCountSequences.AddRange(readCountsFromTable(groupA));
                    }
                    if (!Global.ONLY_TABLE_GROUP_A)
                    {
                        validLugCountSequences.AddRange(readCountsFromTable(groupB));
                    }
                    if (Global.ONLY_TABLE_GROUP_A && Global.ONLY_TABLE_GROUP_B)
                    {
                        Console.WriteLine("Cannot exclude both groups A and B");
                        Console.Read();
                        Environment.Exit(-1);
                    }
                }
                else
                {
                    validLugCountSequences = getValidLugCountSequences();
                }
            }
        }

        private List<int[]> getValidLugCountSequences()
        {
            List<int[]> lugCountSequences = new List<int[]>();

            int[] lugSeq = new int[7]; // from 1 to 6

            // the first one is always 1
            // each one is at least equal or greater than its predecessor
            for (lugSeq[1] = Global.MIN_KICK; lugSeq[1] <= Global.MAX_LOWEST_KICK; lugSeq[1]++)
            {
                for (lugSeq[2] = lugSeq[1]; lugSeq[2] <= Global.MAX_KICK; lugSeq[2]++)
                {
                    for (lugSeq[3] = lugSeq[2]; lugSeq[3] <= Global.MAX_KICK; lugSeq[3]++)
                    {
                        for (lugSeq[4] = lugSeq[3]; lugSeq[4] <= Global.MAX_KICK; lugSeq[4]++)
                        {
                            for (lugSeq[5] = lugSeq[4]; lugSeq[5] <= Global.MAX_KICK; lugSeq[5]++)
                            {
                                for (lugSeq[6] = lugSeq[5]; lugSeq[6] <= Global.MAX_KICK; lugSeq[6]++)
                                {
                                    if (tripleSameSucc(lugSeq))
                                    {
                                        continue;
                                    }
                                    if ((sameSucc(lugSeq) != 0) && !Global.SAME_SUCCESSOR_ALLOWED)
                                    {
                                        continue;
                                    }

                                    int overlaps = sum(lugSeq) - Global.BARS;
                                    if ((overlaps < Global.MIN_OVERLAP) || (overlaps > Global.MAX_OVERLAP))
                                    {
                                        continue;
                                    }
                                    if (Global.EVEN_3)
                                    {
                                        if (even(lugSeq) != 3)
                                        {
                                            continue;
                                        }
                                    }
                                    if (Global.EVEN_2_3_4)
                                    {
                                        if ((even(lugSeq) < 2) || (even(lugSeq) > 4))
                                        {
                                            continue;
                                        }
                                    }

                                    // check that out of the 2^6 combinations, all sums
                                    // from 1 to Key.BARS are covered.
                                    if (Global.COVERAGE_27 && !coverage(lugSeq))
                                    {
                                        continue;
                                    }

                                    // save the sequence
                                    int[] lugsCountSequence = new int[lugSeq.Length];
                                    Array.Copy(lugSeq, 0, lugsCountSequence, 0, lugSeq.Length);
                                    lugCountSequences.Add(lugsCountSequence);
                                }
                            }
                        }
                    }
                }
            }

            Console.WriteLine($"Lugs Valid Options: {lugCountSequences.Count()} \n");

            lugCountSequences.Sort(delegate (int[] o1, int[] o2)
            {

                int diff;

                // This order is to ease the comparison to the manual
                // First, show those like 1,2,.... before 1,1,.....
                diff = sameSucc(o2) - sameSucc(o1);
                if (diff != 0)
                {
                    return diff;
                }

                // first show low number of overlaps.
                diff = sum(o1) - sum(o2);
                if (diff != 0)
                {
                    return diff;
                }

                // then sort by natural order
                // first one is always 1... no need to compare

                diff = o1[2] - o2[2];
                if (diff != 0)
                {
                    return diff;
                }

                diff = o1[3] - o2[3];
                if (diff != 0)
                {
                    return diff;
                }

                diff = o1[4] - o2[4];
                if (diff != 0)
                {
                    return diff;
                }

                diff = o1[5] - o2[5];
                if (diff != 0)
                {
                    return -diff;
                }
                diff = o1[6] - o2[6];
                if (diff != 0)
                {
                    return diff;
                }

                return 0;
            });

            if (print)
            {
                for (int i = 0; i < lugCountSequences.Count; i++)
                {
                    int[] lugsCountSequence = lugCountSequences[i];

                    int sum = 0;
                    for (int w = 1; w <= Global.WHEELS; w++)
                    {
                        Console.WriteLine($"\t{lugsCountSequence[w]}");
                        sum += lugsCountSequence[w];
                    }
                    Console.WriteLine($"\t[{sum}\t{sum - Global.BARS}]\n");

                }
            }

            return lugCountSequences;
        }


        private bool isLugCountSequenceCompliant(int[] lugsCountSequence)
        {
            int[] sorted = new int[7];

            if (Global.VERSION == MachineVersion.UNRESTRICTED)
            {
                return true;
            }
            if (Global.VERSION == MachineVersion.NO_OVERLAP || Global.VERSION == MachineVersion.SWEDISH)
            {
                return Utils.Sum(lugsCountSequence) == Global.BARS;
            }

            Array.Copy(lugsCountSequence, 0, sorted, 0, lugsCountSequence.Length);
            Array.Sort(sorted);
            for (int i = 0; i < validLugCountSequences.Count; i++)
            {
                int[] validSeq = validLugCountSequences[i];

                bool found = true;
                for (int w = 1; w <= Global.WHEELS; w++)
                {
                    if (sorted[w] != validSeq[w])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return true;
                }
            }
            return false;
        }

        public bool isTypeCountCompliant(int[] simpleCount)
        {

            int[] sequence = new int[Global.WHEELS + 1];
            for (int w1 = 1; w1 <= Global.WHEELS; w1++)
            {
                sequence[w1] += simpleCount[_key.Lugs.GetTypeCountIndex(w1)];
                for (int w2 = w1 + 1; w2 <= Global.WHEELS; w2++)
                {
                    int count = simpleCount[_key.Lugs.GetTypeCountIndex(w1, w2)];
                    sequence[w1] += count;
                    sequence[w2] += count;
                }
            }
            return isLugCountSequenceCompliant(sequence);
        }

        private readonly int[][] groupA = {

            new int[] { 1, 2, 3, 4, 8, 10, 1 },
            new int[] {1, 2, 3, 4, 7, 11, 1},
            new int[] {1, 2, 3, 4, 6, 12, 1},
            new int[] {1, 2, 3, 4, 5, 13, 1},
            new int[] {1, 2, 3, 5, 8, 9, 1},
            new int[] {1, 2, 3, 5, 7, 10, 1},
            new int[] {1, 2, 3, 5, 6, 11, 1},
            new int[] {1, 2, 3, 6, 7, 9, 1},
            new int[] {1, 2, 4, 5, 7, 9, 1},
            new int[] {1, 2, 4, 5, 6, 10, 1},

            new int[] {1, 2, 3, 4, 9, 10, 2},
            new int[] {1, 2, 3, 4, 8, 11, 2},
            new int[] {1, 2, 3, 4, 7, 12, 2},
            new int[] {1, 2, 3, 4, 6, 13, 2},
            new int[] {1, 2, 3, 5, 8, 10, 2},
            new int[] {1, 2, 3, 5, 7, 11, 2},
            new int[] {1, 2, 3, 5, 6, 12, 2},
            new int[] {1, 2, 3, 6, 8, 9, 2},
            new int[] {1, 2, 3, 6, 7, 10, 2},
            new int[] {1, 2, 4, 5, 8, 9, 2},
            new int[] {1, 2, 4, 5, 7, 10, 2},
            new int[] {1, 2, 4, 5, 6, 11, 2},
            new int[] {1, 2, 4, 6, 7, 9, 2},
            new int[] {1, 2, 3, 4, 9, 11, 3},
            new int[] {1, 2, 3, 4, 8, 12, 3},
            new int[] {1, 2, 3, 4, 7, 13, 3},
            new int[] {1, 2, 3, 5, 9, 10, 3},
            new int[] {1, 2, 3, 5, 8, 11, 3},
            new int[] {1, 2, 3, 5, 7, 12, 3},
            new int[] {1, 2, 3, 5, 6, 13, 3},
            new int[] {1, 2, 3, 6, 8, 10, 3},
            new int[] {1, 2, 3, 6, 7, 11, 3},
            new int[] {1, 2, 3, 7, 8, 9, 3},
            new int[] {1, 2, 4, 5, 8, 10, 3},

            new int[] {1, 2, 4, 5, 7, 11, 3,},
            new int[] {1, 2, 4, 5, 6, 12, 3,},
            new int[] {1, 2, 4, 6, 8, 9, 3,},
            new int[] {1, 2, 4, 6, 7, 10, 3,},
            new int[] {1, 2, 3, 4, 10, 11, 4,},
            new int[] {1, 2, 3, 4, 9, 12, 4,},
            new int[] {1, 2, 3, 4, 8, 13, 4,},
            new int[] {1, 2, 3, 5, 9, 11, 4,},
            new int[] {1, 2, 3, 5, 8, 12, 4,},
            new int[] {1, 2, 3, 5, 7, 13, 4,},
            new int[] {1, 2, 3, 6, 9, 10, 4,},
            new int[] {1, 2, 3, 6, 8, 11, 4,},
            new int[] {1, 2, 3, 6, 7, 12, 4,},
            new int[] {1, 2, 3, 7, 8, 10, 4,},
            new int[] {1, 2, 4, 5, 9, 10, 4,},
            new int[] {1, 2, 4, 5, 8, 11, 4,},
            new int[] {1, 2, 4, 5, 7, 12, 4,},
            new int[] {1, 2, 4, 5, 6, 13, 4,},
            new int[] {1, 2, 4, 6, 7, 11, 4,},
            new int[] {1, 2, 4, 6, 8, 10, 4,},
            new int[] {1, 2, 4, 7, 8, 9, 4,},
            new int[] {1, 2, 3, 4, 10, 12, 5,},
            new int[] {1, 2, 3, 4, 9, 13, 5,},
            new int[] {1, 2, 3, 5, 10, 11, 5,},

            new int[] {1, 2, 3, 5, 9, 12, 5,},
            new int[] {1, 2, 3, 5, 8, 13, 5,},
            new int[] {1, 2, 3, 6, 9, 11, 5,},
            new int[] {1, 2, 3, 6, 8, 12, 5,},
            new int[] {1, 2, 3, 6, 7, 13, 5,},
            new int[] {1, 2, 3, 7, 9, 10, 5,},
            new int[] {1, 2, 3, 7, 8, 11, 5,},
            new int[] {1, 2, 4, 5, 9, 11, 5,},
            new int[] {1, 2, 4, 5, 8, 12, 5,},
            new int[] {1, 2, 4, 5, 7, 13, 5,},
            new int[] {1, 2, 4, 6, 9, 10, 5,},
            new int[] {1, 2, 4, 6, 8, 11, 5,},
            new int[] {1, 2, 4, 6, 7, 12, 5,},
            new int[] {1, 2, 4, 7, 8, 10, 5,},
            new int[] {1, 2, 3, 4, 11, 12, 6,},
            new int[] {1, 2, 3, 4, 10, 13, 6,},
            new int[] {1, 2, 3, 5, 10, 12, 6,},
            new int[] {1, 2, 3, 5, 9, 13, 6,},
            new int[] {1, 2, 3, 6, 10, 11, 6,},
            new int[] {1, 2, 3, 6, 9, 12, 6,},
            new int[] {1, 2, 3, 6, 8, 13, 6,},
            new int[] {1, 2, 3, 7, 9, 11, 6,},
            new int[] {1, 2, 3, 7, 8, 12, 6,},
            new int[] {1, 2, 4, 5, 10, 11, 6,},
            new int[] {1, 2, 4, 5, 9, 12, 6,},
            new int[] {1, 2, 4, 5, 8, 13, 6,},
            new int[] {1, 2, 4, 6, 8, 12, 6,},
            new int[] {1, 2, 4, 6, 9, 11, 6,},
            new int[] {1, 2, 4, 6, 7, 13, 6,},
            new int[] {1, 2, 4, 7, 9, 10, 6,},
            new int[] {1, 2, 4, 7, 8, 11, 6,},
            new int[] {1, 2, 3, 4, 11, 13, 7,},
            new int[] {1, 2, 3, 5, 11, 12, 7,},

            new int[] {1, 2, 3, 5, 10, 13, 7},
            new int[] {1, 2, 3, 6, 10, 12, 7},
            new int[] {1, 2, 3, 6, 9, 13, 7},
            new int[] {1, 2, 3, 7, 10, 11, 7},
            new int[] {1, 2, 3, 7, 9, 12, 7},
            new int[] {1, 2, 3, 7, 8, 13, 7},
            new int[] {1, 2, 4, 5, 10, 12, 7},
            new int[] {1, 2, 4, 5, 9, 13, 7},
            new int[] {1, 2, 4, 6, 8, 13, 7},
            new int[] {1, 2, 4, 6, 9, 12, 7},
            new int[] {1, 2, 4, 6, 10, 11, 7},
            new int[] {1, 2, 4, 7, 9, 11, 7},
            new int[] {1, 2, 4, 7, 8, 12, 7},
            new int[] {1, 2, 4, 8, 9, 10, 7},
            new int[] {1, 2, 3, 5, 11, 13, 8},
            new int[] {1, 2, 3, 6, 11, 12, 8},
            new int[] {1, 2, 3, 6, 10, 13, 8},
            new int[] {1, 2, 3, 7, 10, 12, 8},
            new int[] {1, 2, 3, 7, 9, 13, 8},
            new int[] {1, 2, 4, 5, 11, 12, 8},
            new int[] {1, 2, 4, 5, 10, 13, 8},
            new int[] {1, 2, 4, 6, 9, 13, 8},
            new int[] {1, 2, 4, 6, 10, 12, 8},
            new int[] {1, 2, 4, 7, 10, 11, 8},
            new int[] {1, 2, 4, 7, 9, 12, 8},
            new int[] {1, 2, 4, 7, 8, 13, 8},
            new int[] {1, 2, 4, 8, 9, 11, 8},
            new int[] {1, 2, 3, 5, 12, 13, 9},
            new int[] {1, 2, 3, 6, 11, 13, 9},
            new int[] {1, 2, 3, 7, 11, 12, 9},
            new int[] {1, 2, 3, 7, 10, 13, 9},
            new int[] {1, 2, 4, 5, 11, 13, 9},
            new int[] {1, 2, 4, 6, 10, 13, 9},

            new int[] {1, 2, 4, 6, 11, 12, 9,},
            new int[] {1, 2, 4, 7, 10, 12, 9,},
            new int[] {1, 2, 4, 7, 9, 13, 9,},
            new int[] {1, 2, 4, 8, 10, 11, 9,},
            new int[] {1, 2, 4, 8, 9, 12, 9,},
            new int[] {1, 2, 3, 6, 12, 13, 10,},
            new int[] {1, 2, 3, 7, 11, 13, 10,},
            new int[] {1, 2, 4, 5, 12, 13, 10,},
            new int[] {1, 2, 4, 6, 11, 13, 10,},
            new int[] {1, 2, 4, 7, 11, 12, 10,},

            new int[] {1, 2, 4, 7, 10, 13, 10,},
            new int[] {1, 2, 4, 8, 9, 13, 10,},
            new int[] {1, 2, 4, 8, 10, 12, 10,},
            new int[] {1, 2, 3, 7, 12, 13, 11,},
            new int[] {1, 2, 4, 6, 12, 13, 11,},
            new int[] {1, 2, 4, 7, 11, 13, 11,},
            new int[] {1, 2, 4, 8, 11, 12, 11,},
            new int[] {1, 2, 4, 8, 10, 13, 11,},
            new int[] {1, 2, 4, 7, 12, 13, 12,},
            new int[] {1, 2, 4, 8, 11, 13, 12,},
        };
        private readonly int[][] groupB = {
            new int[] {1, 1, 2, 3, 8, 13, 1},
            new int[] {1, 1, 2, 4, 9, 11, 1},
            new int[] {1, 1, 2, 4, 8, 12, 1},
            new int[] {1, 1, 2, 4, 7, 13, 1},
            new int[] {1, 1, 2, 5, 9, 10, 1},
            new int[] {1, 1, 2, 5, 8, 11, 1},
            new int[] {1, 1, 2, 5, 7, 12, 1},
            new int[] {1, 1, 2, 5, 6, 13, 1},

            new int[] {1, 1, 3, 4, 9, 10, 1},
            new int[] {1, 1, 3, 4, 8, 11, 1},
            new int[] {1, 1, 3, 4, 7, 12, 1},

            new int[] {1, 1, 3, 4, 6, 13, 1},
            new int[] {1, 1, 3, 5, 8, 10, 1},
            new int[] {1, 1, 3, 5, 7, 11, 1},
            new int[] {1, 1, 3, 5, 6, 12, 1},
            new int[] {1, 1, 3, 6, 8, 9, 1},
            new int[] {1, 1, 3, 6, 7, 10, 1},

            new int[] {1, 2, 2, 3, 9, 11, 1},
            new int[] {1, 2, 2, 3, 8, 12, 1},
            new int[] {1, 2, 2, 3, 7, 13, 1},
            new int[] {1, 2, 2, 4, 8, 11, 1},
            new int[] {1, 2, 2, 4, 7, 12, 1},

            new int[] {1, 2, 2, 4, 6, 13, 1},
            new int[] {1, 2, 2, 5, 8, 10, 1},
            new int[] {1, 2, 2, 5, 7, 11, 1},
            new int[] {1, 2, 2, 5, 6, 12, 1},
            new int[] {1, 2, 2, 6, 8, 9, 1},
            new int[] {1, 2, 2, 6, 7, 10, 1},

            new int[] {1, 2, 3, 3, 9, 10, 1},
            new int[] {1, 2, 3, 3, 8, 11, 1},
            new int[] {1, 2, 3, 3, 7, 12, 1},
            new int[] {1, 2, 3, 4, 9, 9, 1},
            new int[] {1, 2, 3, 5, 5, 12, 1},
            new int[] {1, 2, 3, 6, 6, 10, 1},

            new int[] {1, 2, 4, 4, 8, 9, 1},
            new int[] {1, 2, 4, 5, 5, 11, 1},
            new int[] {1, 2, 4, 6, 6, 9, 1},

            new int[] {1, 1, 2, 4, 9, 12, 2},
            new int[] {1, 1, 2, 4, 8, 13, 2},
            new int[] {1, 1, 2, 5, 9, 11, 2},
            new int[] {1, 1, 2, 5, 8, 12, 2},
            new int[] {1, 1, 2, 5, 7, 13, 2},
            new int[] {1, 1, 3, 4, 9, 11, 2},
            new int[] {1, 1, 3, 4, 8, 12, 2},
            new int[] {1, 1, 3, 4, 7, 13, 2},
            new int[] {1, 1, 3, 5, 9, 10, 2},
            new int[] {1, 1, 3, 5, 8, 11, 2},
            new int[] {1, 1, 3, 5, 7, 12, 2},
            new int[] {1, 1, 3, 5, 6, 13, 2},
            new int[] {1, 1, 3, 6, 8, 10, 2},
            new int[] {1, 1, 3, 6, 7, 11, 2},
            new int[] {1, 2, 2, 3, 9, 12, 2},
            new int[] {1, 2, 2, 3, 8, 13, 2},
            new int[] {1, 2, 2, 4, 9, 11, 2},
            new int[] {1, 2, 2, 4, 7, 13, 2},
            new int[] {1, 2, 2, 5, 9, 10, 2},

            new int[] {1, 2, 2, 5, 8, 11, 2},
            new int[] {1, 2, 2, 5, 7, 12, 2},
            new int[] {1, 2, 2, 5, 6, 13, 2},
            //new int[] {1, 2, 2, 6, 10, 11, 2}, //problem!
            new int[] {1, 2, 2, 6, 7, 11, 2},
            new int[] {1, 2, 3, 3, 9, 11, 2},
            new int[] {1, 2, 3, 3, 8, 12, 2},
            new int[] {1, 2, 3, 3, 7, 13, 2},
            new int[] {1, 2, 3, 5, 5, 13, 2},
            new int[] {1, 2, 3, 5, 9, 9, 2},
            new int[] {1, 2, 3, 6, 6, 11, 2},
            new int[] {1, 2, 3, 7, 7, 9, 2},
            new int[] {1, 2, 4, 4, 7, 11, 2},
            new int[] {1, 2, 4, 4, 5, 13, 2},
            new int[] {1, 2, 4, 5, 5, 12, 2},
            new int[] {1, 1, 2, 4, 9, 13, 3,},
            new int[] {1, 1, 2, 5, 10, 11, 3,},
            new int[] {1, 1, 2, 5, 9, 12, 3,},
            new int[] {1, 1, 2, 5, 8, 13, 3,},

            new int[] {1, 1, 3, 4, 10, 11, 3},
            new int[] {1, 1, 3, 4, 9, 12, 3},
            new int[] {1, 1, 3, 4, 8, 13, 3},
            new int[] {1, 1, 3, 5, 9, 11, 3},
            new int[] {1, 1, 3, 5, 8, 12, 3},
            new int[] {1, 1, 3, 5, 7, 13, 3},
            new int[] {1, 1, 3, 6, 9, 10, 3},
            new int[] {1, 1, 3, 6, 8, 11, 3},
            new int[] {1, 1, 3, 6, 7, 12, 3},
            new int[] {1, 2, 2, 3, 9, 13, 3},
            new int[] {1, 2, 2, 4, 10, 11, 3},
            new int[] {1, 2, 2, 4, 9, 12, 3},
            new int[] {1, 2, 2, 4, 8, 13, 3},
            new int[] {1, 2, 2, 5, 9, 11, 3},
            new int[] {1, 2, 2, 5, 8, 12, 3},
            new int[] {1, 2, 2, 5, 7, 13, 3},
            new int[] {1, 2, 2, 6, 9, 10, 3},
            new int[] {1, 2, 2, 6, 8, 11, 3},
            new int[] {1, 2, 2, 6, 7, 12, 3},
            new int[] {1, 2, 3, 3, 10, 11, 3},
            new int[] {1, 2, 3, 3, 9, 12, 3},
            new int[] {1, 2, 3, 3, 8, 13, 3},
            new int[] {1, 2, 3, 4, 10, 10, 3},
            new int[] {1, 2, 3, 6, 6, 12, 3},
            new int[] {1, 2, 3, 6, 9, 9, 3},
            new int[] {1, 2, 3, 7, 7, 10, 3},
            new int[] {1, 2, 4, 4, 9, 10, 3},
            new int[] {1, 2, 4, 4, 8, 11, 3},
            new int[] {1, 2, 4, 4, 7, 12, 3},
            new int[] {1, 2, 4, 4, 6, 13, 3},
            new int[] {1, 2, 4, 5, 5, 13, 3},
            new int[] {1, 2, 4, 5, 9, 9, 3},
            new int[] {1, 2, 4, 6, 6, 11, 3},

            new int[] {1, 2, 4, 7, 7, 9, 3,},
            new int[] {1, 1, 2, 5, 10, 12, 4,},
            new int[] {1, 1, 2, 5, 9, 13, 4,},
            new int[] {1, 1, 3, 4, 10, 12, 4,},
            new int[] {1, 1, 3, 4, 9, 13, 4,},
            new int[] {1, 1, 3, 5, 10, 11, 4,},
            new int[] {1, 1, 3, 5, 9, 12, 4,},
            new int[] {1, 1, 3, 5, 8, 13, 4,},
            new int[] {1, 1, 3, 6, 9, 11, 4,},
            new int[] {1, 1, 3, 6, 8, 12, 4,},
            new int[] {1, 1, 3, 6, 7, 13, 4,},
            new int[] {1, 2, 2, 4, 9, 13, 4,},
            new int[] {1, 2, 2, 5, 10, 11, 4,},
            new int[] {1, 2, 2, 5, 9, 12, 4,},
            new int[] {1, 2, 2, 5, 8, 13, 4,},
            new int[] {1, 2, 2, 6, 9, 11, 4,},
            new int[] {1, 2, 2, 6, 7, 13, 4,},
            new int[] {1, 2, 3, 3, 10, 12, 4,},
            new int[] {1, 2, 3, 3, 9, 13, 4,},
            new int[] {1, 2, 3, 5, 10, 10, 4,},
            new int[] {1, 2, 3, 6, 6, 13, 4,},
            new int[] {1, 2, 3, 7, 7, 11, 4,},
            new int[] {1, 2, 3, 7, 9, 9, 4,},
            new int[] {1, 2, 4, 4, 9, 11, 4,},
            new int[] {1, 2, 4, 4, 7, 13, 4,},
            new int[] {1, 2, 4, 6, 9, 9, 4,},
            new int[] {1, 2, 4, 7, 7, 10, 4,},
            new int[] {1, 1, 2, 5, 10, 13, 5,},
            new int[] {1, 1, 3, 4, 10, 13, 5,},
            new int[] {1, 1, 3, 5, 10, 12, 5,},
            new int[] {1, 1, 3, 5, 9, 13, 5,},
            new int[] {1, 1, 3, 6, 10, 11, 5,},
            new int[] {1, 1, 3, 6, 9, 12, 5,},

            new int[] {1, 1, 3, 6, 8, 13, 5},
            new int[] {1, 2, 2, 4, 10, 13, 5},
            new int[] {1, 2, 2, 5, 10, 12, 5},
            new int[] {1, 2, 2, 5, 9, 13, 5},
            new int[] {1, 2, 2, 6, 9, 12, 5},
            new int[] {1, 2, 2, 6, 8, 13, 5},
            new int[] {1, 2, 3, 3, 10, 13, 5},
            new int[] {1, 2, 3, 4, 11, 11, 5},
            new int[] {1, 2, 3, 6, 10, 10, 5},
            new int[] {1, 2, 3, 7, 7, 12, 5},
            new int[] {1, 2, 4, 4, 10, 11, 5},
            new int[] {1, 2, 4, 4, 9, 12, 5},
            new int[] {1, 2, 4, 4, 8, 13, 5},
            new int[] {1, 2, 4, 6, 6, 13, 5},
            new int[] {1, 2, 4, 7, 7, 11, 5},
            new int[] {1, 2, 4, 7, 9, 9, 5},
            new int[] {1, 2, 4, 8, 8, 9, 5},
            new int[] {1, 1, 3, 5, 11, 12, 6},
            new int[] {1, 1, 3, 5, 10, 13, 6},
            new int[] {1, 1, 3, 6, 10, 12, 6},
            new int[] {1, 1, 3, 6, 9, 13, 6},
            new int[] {1, 2, 2, 4, 11, 13, 6},
            new int[] {1, 2, 2, 5, 11, 12, 6},
            new int[] {1, 2, 2, 5, 10, 13, 6},
            new int[] {1, 2, 2, 6, 9, 13, 6},
            new int[] {1, 2, 3, 3, 11, 13, 6},
            new int[] {1, 2, 3, 5, 11, 11, 6},
            new int[] {1, 2, 3, 7, 7, 13, 6},
            new int[] {1, 2, 3, 7, 10, 10, 6},
            new int[] {1, 2, 4, 7, 7, 12, 6},
            new int[] {1, 2, 4, 8, 9, 9, 6},
            new int[] {1, 1, 3, 5, 11, 13, 7},
            new int[] {1, 1, 3, 6, 11, 12, 7},
            new int[] {1, 1, 3, 6, 10, 13, 7},
            new int[] {1, 2, 2, 4, 12, 13, 7},
            new int[] {1, 2, 2, 5, 11, 13, 7},
            new int[] {1, 2, 2, 6, 11, 12, 7},
            new int[] {1, 2, 2, 6, 10, 13, 7},
            new int[] {1, 2, 3, 6, 11, 11, 7},
            new int[] {1, 2, 4, 4, 11, 12, 7},
            new int[] {1, 2, 4, 4, 10, 13, 7},
            new int[] {1, 2, 4, 5, 11, 11, 7},
            new int[] {1, 2, 4, 7, 7, 13, 7},
            new int[] {1, 2, 4, 7, 10, 10, 7},
            new int[] {1, 2, 4, 8, 8, 11, 7},
            new int[] {1, 1, 3, 6, 11, 13, 8},
            new int[] {1, 2, 2, 6, 11, 13, 8},
            new int[] {1, 2, 3, 5, 12, 12, 8},
            new int[] {1, 2, 4, 4, 11, 13, 8},
            new int[] {1, 2, 4, 6, 11, 11, 8},
            new int[] {1, 1, 3, 6, 12, 13, 9},
            new int[] {1, 2, 2, 6, 12, 13, 9},
            new int[] {1, 2, 3, 6, 12, 12, 9},
            new int[] {1, 2, 4, 4, 12, 13, 9},
            new int[] {1, 2, 4, 5, 12, 12, 9},
            new int[] {1, 2, 4, 7, 11, 11, 9},
            new int[] {1, 2, 4, 8, 8, 13, 9},
            new int[] {1, 2, 2, 6, 13, 13, 10},
            new int[] {1, 2, 3, 5, 13, 13, 10},
            new int[] {1, 2, 4, 8, 11, 11, 10},
            new int[] {1, 2, 3, 6, 13, 13, 11},
            new int[] {1, 2, 4, 7, 12, 12, 11},
            new int[] {1, 2, 3, 7, 13, 13, 12},
        };

        private List<int[]> readCountsFromTable(int[][] table)
        {
            List<int[]> lugCountSequences = new List<int[]>();

            for (int seqIndex = 0; seqIndex < table.Length; seqIndex++)
            {

                int[] seq = table[seqIndex];

                int[] lugSeq = new int[7]; // from 1 to 6
                int sum = 0;
                for (int i = 0; i < 6; i++)
                {
                    if (seq[i] > Global.MAX_KICK)
                    {
                        continue;
                    }
                    sum += seq[i];
                    lugSeq[i + 1] = seq[i];
                }
                int overlaps = sum - Global.BARS;
                if (overlaps != seq[6])
                {
                    Console.WriteLine($"Error with seq. Expected {seq[6]} - actual {sum - Global.BARS} - ");

                    for (int i = 0; i < 6; i++)
                    {
                        Console.WriteLine($"{seq[i]}, ");
                    }
                    Console.WriteLine("\n");
                    continue;
                }
                if ((overlaps < Global.MIN_OVERLAP) || (overlaps > Global.MAX_OVERLAP))
                {
                    continue;
                }
                lugCountSequences.Add(lugSeq);

            }
            Console.WriteLine($"{lugCountSequences.Count()}\n");

            return lugCountSequences;
        }

    }
}
