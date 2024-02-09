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
using M209AnalyzerLib.Enums;
using System;
using System.Linq;
using System.Text;

namespace M209AnalyzerLib.M209
{
    public class Lugs
    {
        public static readonly int TYPE_COUNT_ARRAY_SIZE = 22; // Why 22, there are only 21 possible type counts

        public int[] TypeCount = new int[TYPE_COUNT_ARRAY_SIZE];

        // from 0 to 63 - based on lugs, for each wheel pin engagement vector, compute the decryption
        // 001001 means wheels 3 and 6 have an active pin, the rest inactive
        public int[] DisplacementVector = new int[64];
        private Key _parentKey = null;

        #region Variables

        private readonly int[][] INDICES_MATRIX = {
            /* 0 */ new int[] { 0, 1, 2, 3, 4, 5, 6 },
            /* 1 */ new int[] { 1, -1, 7, 8, 9, 10, 11 },
            /* 2 */ new int[] { 2, 7, -1, 12, 13, 14, 15 },
            /* 3 */ new int[] { 3, 8, 12, -1, 16, 17, 18 },
            /* 4 */ new int[] { 4, 9, 13, 16, -1, 19, 20 },
            /* 5 */ new int[] { 5, 10, 14, 17, 19, -1, 21 },
            /* 6 */ new int[] { 6, 11, 15, 18, 20, 21, -1 },
        };
        private readonly int TYPE_COUNT_W1;
        private readonly int TYPE_COUNT_W2;
        private readonly int TYPE_COUNT_W3;
        private readonly int TYPE_COUNT_W4;
        private readonly int TYPE_COUNT_W5;
        private readonly int TYPE_COUNT_W6;

        private readonly int TYPE_COUNT_W1W2;
        private readonly int TYPE_COUNT_W1W3;
        private readonly int TYPE_COUNT_W1W4;
        private readonly int TYPE_COUNT_W1W5;
        private readonly int TYPE_COUNT_W1W6;

        private readonly int TYPE_COUNT_W2W3;
        private readonly int TYPE_COUNT_W2W4;
        private readonly int TYPE_COUNT_W2W5;
        private readonly int TYPE_COUNT_W2W6;

        private readonly int TYPE_COUNT_W3W4;
        private readonly int TYPE_COUNT_W3W5;
        private readonly int TYPE_COUNT_W3W6;

        private readonly int TYPE_COUNT_W4W5;
        private readonly int TYPE_COUNT_W4W6;

        private readonly int TYPE_COUNT_W5W6;

        public readonly int[] TYPES_WITHOUT_OVERLAP;

        private readonly int[] TYPES_WITH_OVERLAP;

        #endregion


        private int[] actualLugsCountSeq;
        private int[] targetLugsCountSeq;

        public readonly int[] TYPES;
        public int[][] PERMS6;

        private static long startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        public Random random = new Random((int)startTime);

        public LugsRules LugsRules;


        public Lugs(Key parentKey)
        {
            _parentKey = parentKey;
            LugsRules = new LugsRules(parentKey);

            PERMS6 = CreatePerms6();
            TYPE_COUNT_W1 = GetTypeCountIndex(1);
            TYPE_COUNT_W2 = GetTypeCountIndex(2);
            TYPE_COUNT_W3 = GetTypeCountIndex(3);
            TYPE_COUNT_W4 = GetTypeCountIndex(4);
            TYPE_COUNT_W5 = GetTypeCountIndex(5);
            TYPE_COUNT_W6 = GetTypeCountIndex(6);

            TYPE_COUNT_W1W2 = GetTypeCountIndex(1, 2);
            TYPE_COUNT_W1W3 = GetTypeCountIndex(1, 3);
            TYPE_COUNT_W1W4 = GetTypeCountIndex(1, 4);
            TYPE_COUNT_W1W5 = GetTypeCountIndex(1, 5);
            TYPE_COUNT_W1W6 = GetTypeCountIndex(1, 6);

            TYPE_COUNT_W2W3 = GetTypeCountIndex(2, 3);
            TYPE_COUNT_W2W4 = GetTypeCountIndex(2, 4);
            TYPE_COUNT_W2W5 = GetTypeCountIndex(2, 5);
            TYPE_COUNT_W2W6 = GetTypeCountIndex(2, 6);

            TYPE_COUNT_W3W4 = GetTypeCountIndex(3, 4);
            TYPE_COUNT_W3W5 = GetTypeCountIndex(3, 5);
            TYPE_COUNT_W3W6 = GetTypeCountIndex(3, 6);

            TYPE_COUNT_W4W5 = GetTypeCountIndex(4, 5);
            TYPE_COUNT_W4W6 = GetTypeCountIndex(4, 6);

            TYPE_COUNT_W5W6 = GetTypeCountIndex(5, 6);

            TYPES = new int[] {
                TYPE_COUNT_W1,
                TYPE_COUNT_W2,
                TYPE_COUNT_W3,
                TYPE_COUNT_W4,
                TYPE_COUNT_W5,
                TYPE_COUNT_W6,
                TYPE_COUNT_W1W2,
                TYPE_COUNT_W1W3,
                TYPE_COUNT_W1W4,
                TYPE_COUNT_W1W5,
                TYPE_COUNT_W1W6,
                TYPE_COUNT_W2W3,
                TYPE_COUNT_W2W4,
                TYPE_COUNT_W2W5,
                TYPE_COUNT_W2W6,
                TYPE_COUNT_W3W4,
                TYPE_COUNT_W3W5,
                TYPE_COUNT_W3W6,
                TYPE_COUNT_W4W5,
                TYPE_COUNT_W4W6,
                TYPE_COUNT_W5W6, };

            TYPES_WITH_OVERLAP = new int[]{
                TYPE_COUNT_W1W2,
                TYPE_COUNT_W1W3,
                TYPE_COUNT_W1W4,
                TYPE_COUNT_W1W5,
                TYPE_COUNT_W1W6,
                TYPE_COUNT_W2W3,
                TYPE_COUNT_W2W4,
                TYPE_COUNT_W2W5,
                TYPE_COUNT_W2W6,
                TYPE_COUNT_W3W4,
                TYPE_COUNT_W3W5,
                TYPE_COUNT_W3W6,
                TYPE_COUNT_W4W5,
                TYPE_COUNT_W4W6,
                TYPE_COUNT_W5W6,
            };
            TYPES_WITHOUT_OVERLAP = new int[]{
                TYPE_COUNT_W1,
                TYPE_COUNT_W2,
                TYPE_COUNT_W3,
                TYPE_COUNT_W4,
                TYPE_COUNT_W5,
                TYPE_COUNT_W6
            };

            actualLugsCountSeq = new int[_parentKey.WHEELS + 1];
            targetLugsCountSeq = new int[_parentKey.WHEELS + 1];
        }

        public Lugs(Key parentKey, string lugsString) : this(parentKey)
        {
            SetLugsString(lugsString, false);
        }

        public int GetOverlaps(int[] typeCount)
        {
            int overlaps = 0;
            for (int i = 0; i < TYPES_WITH_OVERLAP.Length; i++)
            {
                overlaps += typeCount[TYPES_WITH_OVERLAP[i]];
            }
            return overlaps;
        }

        void SetLugsString(string lugsString, bool checkRules)
        {
            for (int i = 0; i < TypeCount.Length; i++)
            {
                TypeCount[i] = 0;
            }

            while (lugsString.Contains("  "))
            {
                lugsString = lugsString.Replace("  ", " ");
            }


            string[] barsString = lugsString.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (barsString.Length > _parentKey.BARS || (barsString.Length < _parentKey.BARS && Global.VERSION != MachineVersion.UNRESTRICTED))
            {
                throw new Exception($"Wrong lug string: {lugsString} has {barsString.Length} bars");
            }
            for (int i = 0; i < barsString.Length; i++)
            {
                string barString = barsString[i];

                string[] barSplit = barString.Split('-');
                if (barSplit.Length != _parentKey.LUGS_PER_BAR)
                {
                    throw new Exception($"Wrong lug settings - too many lugs on one bar: {barString}");
                }

                int w1 = int.Parse(barSplit[0]);
                int w2 = int.Parse(barSplit[1]);

                if ((w1 > _parentKey.WHEELS) || (w2 > _parentKey.WHEELS))
                {
                    throw new Exception($"Wrong lug settings - wrong wheel number: {barString}");
                }
                if ((w1 == w2) && (w1 != 0))
                {
                    throw new Exception($"Wrong lug settings - wheel appears twice on same bar: {barString}");
                }

                if (w2 == 0)
                {
                    if (Global.VERSION != MachineVersion.UNRESTRICTED)
                    {
                        throw new Exception($"Wrong lug settings - 0-0 not valid: {barString}");
                    }
                    else
                    {
                        continue;
                    }
                }

                if (w1 > w2)
                {
                    int temp = w1;
                    w1 = w2;
                    w2 = temp;
                }
                if (w1 == 0)
                {
                    TypeCount[GetTypeCountIndex(w2)]++;
                }
                else
                {
                    TypeCount[GetTypeCountIndex(w1, w2)]++;
                }
            }

            if (!LugsRules.isTypeCountCompliant(TypeCount))
            {
                if (checkRules)
                {
                    throw new Exception($"Lug settings do not match the lug count rules: {lugsString}");
                }
            }

            ComputeVector();
        }

        public string GetLugsString()
        {

            StringBuilder s = new StringBuilder();
            for (int w1 = 1; w1 <= _parentKey.WHEELS; w1++)
            {
                for (int w2 = w1 + 1; w2 <= _parentKey.WHEELS; w2++)
                {
                    for (int i = 0; i < TypeCount[GetTypeCountIndex(w1, w2)]; i++)
                    {
                        s.Append(w1).Append("-").Append(w2).Append(" ");
                    }
                }
            }
            for (int w = 1; w <= _parentKey.WHEELS; w++)
            {
                for (int i = 0; i < TypeCount[GetTypeCountIndex(w)]; i++)
                {
                    s.Append("0-").Append(w).Append(" ");
                }
            }
            return s.ToString();
        }


        public void GetTypeCount(int[] typeCount)
        {
            Array.Copy(TypeCount, 0, typeCount, 0, TYPE_COUNT_ARRAY_SIZE);
        }

        public int[] CreateTypeCountCopy()
        {
            int[] typeCountCopy = new int[TYPE_COUNT_ARRAY_SIZE];
            Array.Copy(TypeCount, typeCountCopy, TYPE_COUNT_ARRAY_SIZE);
            return typeCountCopy;
        }

        public bool SetTypeCount(int[] typeCount, bool checkRules)
        {
            int overLaps = GetOverlaps(typeCount);

            if ((overLaps > Global.MAX_OVERLAP) || (overLaps < Global.MIN_OVERLAP))
            {
                return false;
            }

            if (checkRules && !LugsRules.isTypeCountCompliant(typeCount))
            {
                return false;
            }

            Array.Copy(typeCount, 0, TypeCount, 0, TYPE_COUNT_ARRAY_SIZE);

            ComputeVector();

            return true;

        }

        private bool CompliesWithUserManualRules(int[] typeCount)
        {
            int[] overlapPerW = new int[7];
            int[] dispRepetition = new int[28];

            if (Global.VERSION == MachineVersion.SWEDISH)
            {
                return Common.Utils.Sum(typeCount) == _parentKey.BARS && GetOverlaps(typeCount) <= Global.MAX_OVERLAP;
            }

            for (int i = 0; i < overlapPerW.Length; i++)
            {
                overlapPerW[i] = 7;
            }

            int overLaps = 0;
            int adjacentOverlaps = 0;
            int involvedWheels = 0;

            for (int w1 = 1; w1 <= _parentKey.WHEELS; w1++)
            {
                for (int w2 = w1 + 1; w2 <= _parentKey.WHEELS; w2++)
                {
                    int count = typeCount[GetTypeCountIndex(w1, w2)];
                    if (count > Global.MAX_SAME_OVERLAP)
                    {
                        return false;
                    }
                    overlapPerW[w1] += count;
                    overlapPerW[w2] += count;
                    overLaps += count;
                    if (w2 == w1 + 1)
                    {
                        adjacentOverlaps += count;
                    }
                }
            }


            if (overLaps > 1)
            {

                if (Global.MIN_INVOLVED_WHEELS > 0)
                {
                    for (int w = 1; w <= _parentKey.WHEELS; w++)
                    {
                        if (overlapPerW[w] > 0)
                        {
                            involvedWheels++;
                        }
                    }
                    if (involvedWheels < Global.MIN_INVOLVED_WHEELS)
                    {
                        return false;
                    }
                }

                if (Global.OVERLAPS_SIDEBYSIDE_SEPARATED)
                {
                    int nonAdjacentOverlaps = overLaps - adjacentOverlaps;
                    if (nonAdjacentOverlaps == 0)
                    {
                        return false;
                    }
                    if (adjacentOverlaps == 0)
                    {
                        return false;
                    }
                }
            }

            if (Global.MAX_TOTAL_OVERLAP < 6)
            {
                int wheelsWithCompleteOverlap = 0;
                for (int w = 1; w <= _parentKey.WHEELS; w++)
                {
                    if (typeCount[GetTypeCountIndex(w)] == 0)
                    {
                        wheelsWithCompleteOverlap++;
                    }
                }
                if (wheelsWithCompleteOverlap > Global.MAX_TOTAL_OVERLAP)
                {
                    return false;
                }
            }
            if (Global.MAX_KICK_REPETITION_64 <= 64)
            {
                for (int i = 0; i < dispRepetition.Length; i++)
                {
                    dispRepetition[i] = 0;
                }
                for (int i = 0; i < 64; i++)
                {
                    dispRepetition[DisplacementVector[i]]++;
                    if (dispRepetition[DisplacementVector[i]] > Global.MAX_KICK_REPETITION_64)
                    {
                        return false;
                    }
                }
            }
            return true;
        }


        private int FindWheelWithEnoughCountLeft(int[] targetCount, int[] actualCount, int notThisWheel)
        {
            int[] weighted = new int[100];

            int items = 0;

            for (int w = 1; w <= _parentKey.WHEELS; w++)
            {
                if (w == notThisWheel)
                {
                    continue;
                }
                int leftCount = targetCount[w] - actualCount[w];
                for (int i = 0; i < leftCount; i++)
                {
                    weighted[items++] = w;
                }
            }

            if (items == 0)
            {
                return 0;
            }
            int rand = random.Next(items);

            return weighted[rand];

        }

        public void Randomize()
        {
            Randomize(0);
        }

        public void Randomize(int overlaps)
        {
            do
            {
                RandomizePrivate(overlaps);
            } while (!CompliesWithUserManualRules(TypeCount));
        }

        private bool AcceptMultipleSimilarOverlaps(int same)
        {
            if (same >= Global.MAX_OVERLAP)
            {
                return false;
            }
            if (!Global.OVERLAPS_EVENLY)
            {
                return true;
            }
            int reference = 64;
            long rand = random.Next(reference);

            for (int i = 0; i < same; i++)
            {
                reference /= 2;
            }
            return rand < reference;

        }

        private void RandomizePrivate(int requiredOverlap)
        {
            for (int i = 0; i < TypeCount.Length; i++)
            {
                TypeCount[i] = 0;
            }
            if (Global.VERSION == MachineVersion.UNRESTRICTED)
            {
                for (int i = 0; i < _parentKey.BARS; i++)
                {
                    int w1 = random.Next(7);
                    int w2 = random.Next(7);
                    if (w1 == 0)
                    {
                        TypeCount[0]++;
                    }
                    else if (w2 == 0 || w1 == w2)
                    {
                        TypeCount[GetTypeCountIndex(w1)]++;
                    }
                    else
                    {
                        TypeCount[GetTypeCountIndex(w1, w2)]++;
                    }
                }
                ComputeVector();
                return;
            }
            else if (Global.VERSION == MachineVersion.SWEDISH)
            {
                for (int i = 0; i < _parentKey.BARS; i++)
                {
                    int w1 = 1 + random.Next(6);
                    int w2 = random.Next(7);
                    if (w2 == 0 || w1 == w2 || GetOverlaps(TypeCount) >= Global.MAX_OVERLAP)
                    {
                        TypeCount[GetTypeCountIndex(w1)]++;
                    }
                    else
                    {
                        TypeCount[GetTypeCountIndex(w1, w2)]++;
                    }
                }
                ComputeVector();
                return;
            }
            else

            if (Global.VERSION == MachineVersion.NO_OVERLAP)
            {
                for (int i = 0; i < _parentKey.BARS; i++)
                {
                    int w = random.Next(6) + 1;
                    TypeCount[GetTypeCountIndex(w)]++;
                }
                ComputeVector();
                return;
            }

            if ((requiredOverlap != 0) && ((requiredOverlap > Global.MAX_OVERLAP) || (requiredOverlap < Global.MIN_OVERLAP)))
            {
                throw new Exception($"Failure generating random lugs. Overlap outside limits {requiredOverlap}");
            }

            int lugCountSeqOverlaps;
            int lugCountSeqTotal;
            int[] lugsCountSeq;
            while (true)
            {
                int seqIndex = random.Next(LugsRules.validLugCountSequences.Count()); // TODO:
                lugsCountSeq = LugsRules.validLugCountSequences.ElementAt(seqIndex);
                lugCountSeqTotal = Common.Utils.Sum(lugsCountSeq);
                lugCountSeqOverlaps = lugCountSeqTotal - _parentKey.BARS;
                if (requiredOverlap != 0 && lugCountSeqOverlaps == requiredOverlap)
                {
                    break;
                }
                if (requiredOverlap == 0 && lugCountSeqOverlaps >= Global.MIN_OVERLAP && lugCountSeqOverlaps <= Global.MAX_OVERLAP)
                {
                    break;
                }
            }

            int[] perm6 = RandomPerm6();

            for (int w = 1; w <= _parentKey.WHEELS; w++)
            {
                targetLugsCountSeq[perm6[w - 1] + 1] = lugsCountSeq[w];
            }
            // Fill the bars with overlaps.
            int barsCount = 0;

            for (int i = 0; i < actualLugsCountSeq.Length; i++)
            {
                actualLugsCountSeq[i] = 0;
            }

            while (lugCountSeqOverlaps > 0)
            {
                int w1 = FindWheelWithEnoughCountLeft(targetLugsCountSeq, actualLugsCountSeq, 0);
                actualLugsCountSeq[w1]++;
                int w2 = FindWheelWithEnoughCountLeft(targetLugsCountSeq, actualLugsCountSeq, w1);
                actualLugsCountSeq[w2]++;

                if (!AcceptMultipleSimilarOverlaps(TypeCount[GetTypeCountIndex(w1, w2)]))
                {
                    actualLugsCountSeq[w1]--;
                    actualLugsCountSeq[w2]--;
                    continue;
                }

                if (w2 == 0)
                {
                    throw new Exception("Failure generating random lugs (2)");
                }
                if (barsCount >= _parentKey.BARS)
                {
                    throw new Exception("Failure generating random lugs (3)");
                }
                barsCount++;

                TypeCount[GetTypeCountIndex(w1, w2)]++;

                lugCountSeqOverlaps--;

            }

            // Bars without overlaps.
            for (int w = 1; w <= _parentKey.WHEELS; w++)
            {
                while (actualLugsCountSeq[w] < targetLugsCountSeq[w])
                {
                    barsCount++;
                    TypeCount[GetTypeCountIndex(w)]++;
                    actualLugsCountSeq[w]++;
                }
            }

            int actualOverlaps = GetOverlaps(TypeCount);
            if (requiredOverlap != 0 && actualOverlaps != requiredOverlap)
            {
                throw new Exception($"Failure generating random lugs (4).  Required overlaps: {requiredOverlap} Actual: {actualOverlaps}");
            }
            if (barsCount > _parentKey.BARS)
            {
                throw new Exception("Failure generating random lugs (5)");
            }

            ComputeVector();
        }

        public void Print()
        {
            Console.WriteLine($"{GetLugsString()}\n");
        }

        public int GetTypeCountIndex(int w)
        {
            return INDICES_MATRIX[0][w];
        }

        public int GetTypeCountIndex(int w1, int w2)
        {
            return INDICES_MATRIX[w1][w2];
        }

        private void ComputeVector()
        {
            int d1, d2, d3, d4, d5, d6;
            int d12, d13, d14, d15, d16, d23, d24, d25, d26, d34, d35, d36, d45, d46, d56;

            d1 = TypeCount[TYPE_COUNT_W1];
            d2 = TypeCount[TYPE_COUNT_W2];
            d3 = TypeCount[TYPE_COUNT_W3];
            d4 = TypeCount[TYPE_COUNT_W4];
            d5 = TypeCount[TYPE_COUNT_W5];
            d6 = TypeCount[TYPE_COUNT_W6];

            d12 = TypeCount[TYPE_COUNT_W1W2];
            d13 = TypeCount[TYPE_COUNT_W1W3];
            d14 = TypeCount[TYPE_COUNT_W1W4];
            d15 = TypeCount[TYPE_COUNT_W1W5];
            d16 = TypeCount[TYPE_COUNT_W1W6];

            d23 = TypeCount[TYPE_COUNT_W2W3];
            d24 = TypeCount[TYPE_COUNT_W2W4];
            d25 = TypeCount[TYPE_COUNT_W2W5];
            d26 = TypeCount[TYPE_COUNT_W2W6];

            d34 = TypeCount[TYPE_COUNT_W3W4];
            d35 = TypeCount[TYPE_COUNT_W3W5];
            d36 = TypeCount[TYPE_COUNT_W3W6];

            d45 = TypeCount[TYPE_COUNT_W4W5];
            d46 = TypeCount[TYPE_COUNT_W4W6];

            d56 = TypeCount[TYPE_COUNT_W5W6];

            for (int v = 0; v < DisplacementVector.Length; v++)
            {
                int displacement = 0;

                int vec = v;
                bool w1 = (vec & 0x1) == 0x1;

                vec >>= 1;
                bool w2 = (vec & 0x1) == 0x1;
                vec >>= 1;
                bool w3 = (vec & 0x1) == 0x1;
                vec >>= 1;
                bool w4 = (vec & 0x1) == 0x1;
                vec >>= 1;
                bool w5 = (vec & 0x1) == 0x1;
                vec >>= 1;
                bool w6 = (vec & 0x1) == 0x1;

                if (w1)
                {
                    displacement += d1;
                }

                if (w2)
                {
                    displacement += d2 + d12;
                }
                else if (w1)
                {
                    displacement += d12;
                }

                if (w3)
                {
                    displacement += d3 + d13 + d23;
                }
                else
                {
                    if (w1)
                    {
                        displacement += d13;
                    }
                    if (w2)
                    {
                        displacement += d23;
                    }
                }

                if (w4)
                {
                    displacement += d4 + d14 + d24 + d34;
                }
                else
                {
                    if (w1)
                    {
                        displacement += d14;
                    }
                    if (w2)
                    {
                        displacement += d24;
                    }
                    if (w3)
                    {
                        displacement += d34;
                    }
                }


                if (w5)
                {
                    displacement += d5 + d15 + d25 + d35 + d45;
                }
                else
                {
                    if (w1)
                    {
                        displacement += d15;
                    }
                    if (w2)
                    {
                        displacement += d25;
                    }
                    if (w3)
                    {
                        displacement += d35;
                    }
                    if (w4)
                    {
                        displacement += d45;
                    }
                }

                if (w6)
                {
                    displacement += d6 + d16 + d26 + d36 + d46 + d56;
                }
                else
                {
                    if (w1)
                    {
                        displacement += d16;
                    }
                    if (w2)
                    {
                        displacement += d26;
                    }
                    if (w3)
                    {
                        displacement += d36;
                    }
                    if (w4)
                    {
                        displacement += d46;
                    }
                    if (w5)
                    {
                        displacement += d56;
                    }
                }

                if (displacement >= 26)
                {
                    displacement -= 26;
                }
                DisplacementVector[v] = displacement;

            }
            if (_parentKey != null)
            {
                _parentKey.InvalidateDecryption();
            }
        }

        private int[] RandomPerm6()
        {
            return PERMS6[random.Next(PERMS6.Length)];
        }

        private int[][] CreatePerms6()
        {
            int[][] perms6 = new int[6 * 5 * 4 * 3 * 2 * 1][];
            for (int i = 0; i < 6 * 5 * 4 * 3 * 2 * 1; i++)
            {
                perms6[i] = new int[6];
            }

            int index = 0;
            for (int i0 = 0; i0 < 6; i0++)
            {
                for (int i1 = 0; i1 < 6; i1++)
                {
                    if (i1 == i0)
                    {
                        continue;
                    }
                    for (int i2 = 0; i2 < 6; i2++)
                    {
                        if (i2 == i0 || i2 == i1)
                        {
                            continue;
                        }
                        for (int i3 = 0; i3 < 6; i3++)
                        {
                            if (i3 == i0 || i3 == i1 || i3 == i2)
                            {
                                continue;
                            }
                            for (int i4 = 0; i4 < 6; i4++)
                            {
                                if (i4 == i0 || i4 == i1 || i4 == i2 || i4 == i3)
                                {
                                    continue;
                                }
                                for (int i5 = 0; i5 < 6; i5++)
                                {
                                    if (i5 == i0 || i5 == i1 || i5 == i2 || i5 == i3 || i5 == i4)
                                    {
                                        continue;
                                    }

                                    perms6[index][0] = i0;
                                    perms6[index][1] = i1;
                                    perms6[index][2] = i2;
                                    perms6[index][3] = i3;
                                    perms6[index][4] = i4;
                                    perms6[index][5] = i5;
                                    index++;

                                }
                            }
                        }
                    }
                }
            }
            return perms6;
        }
    }
}
