using System;
using System.Collections.Generic;
using System.Linq;

namespace CrypTool.Plugins.UbchiPermutationCryptanalysis
{
    public static class AllLetterAnagramming
    {
        private static readonly double[] GermanFreq = new double[]
        {
            4.414, 1.596, 2.600, 4.560, 14.534, 1.318, 2.603, 4.257,
            6.754, 0.164, 0.971, 3.018, 2.166, 8.695, 1.892, 0.544,
            0.039, 5.823, 5.570, 4.994, 3.094, 0.694, 1.310, 0.633,
            0.127, 0.960
        };

        public static List<ScoredCandidate> Analyze(
            string ciphertext, string crib, int keyLength, int nullCount,
            int maxResults, Func<bool> isStopped, Action<long> reportProgress,
            DateTime deadline = default(DateTime))
        {
            if (deadline == default(DateTime))
                deadline = DateTime.MaxValue;

            List<ScoredCandidate> results = new List<ScoredCandidate>();

            if (string.IsNullOrEmpty(ciphertext) || ciphertext.Length < keyLength * 2)
                return results;

            List<string> cribsToTry = new List<string>();
            if (!string.IsNullOrEmpty(crib))
            {
                cribsToTry.Add(crib);
            }

            long tested = 0;

            foreach (string currentCrib in cribsToTry)
            {
                if (isStopped != null && isStopped()) break;
                if (currentCrib.Length < 2 || currentCrib.Length > ciphertext.Length) continue;

                int ptLen = ciphertext.Length - nullCount;
                if (ptLen < keyLength || ptLen < currentCrib.Length) continue;

                GridParams gp = new GridParams(keyLength, ptLen, ciphertext.Length);

                CribLetterInfo[] cribLetters = BuildCribLetters(currentCrib, keyLength);
                Array.Sort(cribLetters, (a, b) => a.Row1.CompareTo(b.Row1));

                Dictionary<char, HashSet<int>> ctPosLookup = BuildLetterPositionSets(ciphertext);

                Array.Sort(cribLetters, (a, b) =>
                {
                    int freqA = GetCipherLetterFrequency(ctPosLookup, a.Letter);
                    int freqB = GetCipherLetterFrequency(ctPosLookup, b.Letter);
                    int cmp = freqA.CompareTo(freqB);
                    if (cmp != 0) return cmp;
                    cmp = a.Row1.CompareTo(b.Row1);
                    if (cmp != 0) return cmp;
                    return a.Col1.CompareTo(b.Col1);
                });

                List<int>[] domains = ComputeAllColumnDomains(ciphertext, ctPosLookup, cribLetters, keyLength, gp);

                if (!RefineDomainsIteratively(ciphertext, ctPosLookup, cribLetters, keyLength, gp, domains))
                    continue;

                bool impossible = false;
                for (int c = 0; c < keyLength; c++)
                {
                    if (domains[c].Count == 0) { impossible = true; break; }
                }
                if (impossible) continue;

                int[] colOrder = Enumerable.Range(0, keyLength).ToArray();
                Array.Sort(colOrder, (a, b) => domains[a].Count.CompareTo(domains[b].Count));

                int[] perm = new int[keyLength];
                bool[] usedValues = new bool[keyLength + 1];

                BacktrackCSP(ciphertext, currentCrib, keyLength, nullCount,
                    cribLetters, gp, domains, perm, usedValues, colOrder, 0,
                    results, maxResults, ref tested, isStopped, reportProgress, deadline);
            }

            if (reportProgress != null)
                reportProgress(tested);

            results.Sort((a, b) => b.Score.CompareTo(a.Score));
            if (results.Count > maxResults)
                results.RemoveRange(maxResults, results.Count - maxResults);

            return results;
        }

        private static CribLetterInfo[] BuildCribLetters(string crib, int keyLength)
        {
            CribLetterInfo[] info = new CribLetterInfo[crib.Length];
            for (int i = 0; i < crib.Length; i++)
            {
                info[i] = new CribLetterInfo
                {
                    Letter = crib[i],
                    Row1 = i / keyLength,
                    Col1 = i % keyLength
                };
            }
            return info;
        }

        private static Dictionary<char, HashSet<int>> BuildLetterPositionSets(string ciphertext)
        {
            Dictionary<char, HashSet<int>> positions = new Dictionary<char, HashSet<int>>();
            for (int i = 0; i < ciphertext.Length; i++)
            {
                char ch = ciphertext[i];
                HashSet<int> set;
                if (!positions.TryGetValue(ch, out set))
                {
                    set = new HashSet<int>();
                    positions[ch] = set;
                }
                set.Add(i);
            }
            return positions;
        }

        private static int GetCipherLetterFrequency(Dictionary<char, HashSet<int>> ctPosLookup, char letter)
        {
            HashSet<int> set;
            if (ctPosLookup.TryGetValue(letter, out set))
            {
                return set.Count;
            }
            return 0;
        }

        private static List<int>[] ComputeAllColumnDomains(
            string ciphertext, Dictionary<char, HashSet<int>> ctPosLookup,
            CribLetterInfo[] cribLetters, int keyLength, GridParams gp)
        {
            List<int>[] domains = new List<int>[keyLength];
            for (int c = 0; c < keyLength; c++)
                domains[c] = Enumerable.Range(1, keyLength).ToList();

            List<CribLetterInfo>[] lettersByCol = new List<CribLetterInfo>[keyLength];
            for (int c = 0; c < keyLength; c++)
                lettersByCol[c] = new List<CribLetterInfo>();
            for (int i = 0; i < cribLetters.Length; i++)
            {
                int col = cribLetters[i].Col1;
                if (col < keyLength)
                    lettersByCol[col].Add(cribLetters[i]);
            }

            for (int c = 0; c < keyLength; c++)
            {
                List<CribLetterInfo> lettersInCol = lettersByCol[c];
                if (lettersInCol.Count == 0) continue;

                HashSet<int> validForCol = null;

                foreach (CribLetterInfo cribLetter in lettersInCol)
                {
                    int row1 = cribLetter.Row1;
                    char letter = cribLetter.Letter;

                    int colLen = (c < gp.FullCols1) ? gp.R1 : (gp.R1 - 1);
                    if (row1 >= colLen) continue;

                    HashSet<int> ctPosSet;
                    if (!ctPosLookup.TryGetValue(letter, out ctPosSet) || ctPosSet.Count == 0)
                    {
                        validForCol = new HashSet<int>();
                        break;
                    }

                    HashSet<int> validForLetter = new HashSet<int>();

                    foreach (int p in ctPosSet)
                    {
                        BackTraceFromCTPosition(p, row1, c, letter, keyLength, gp, validForLetter);
                    }

                    if (validForCol == null)
                        validForCol = validForLetter;
                    else
                        validForCol.IntersectWith(validForLetter);

                    if (validForCol.Count == 0) break;
                }

                if (validForCol != null && validForCol.Count < keyLength)
                {
                    domains[c] = new List<int>(validForCol);
                    domains[c].Sort();
                }
            }

            return domains;
        }

        private static void BackTraceFromCTPosition(
            int p, int targetRow, int targetCol, char letter,
            int keyLength, GridParams gp,
            HashSet<int> validPiValues)
        {
            int L = keyLength;
            int totalShort2 = L - gp.FullCols2;
            int totalFull2 = gp.FullCols2;

            for (int w = 1; w <= L; w++)
            {
                int numBefore2 = w - 1;

                for (int blockIsFull2 = 0; blockIsFull2 <= 1; blockIsFull2++)
                {
                    int blockLen2 = blockIsFull2 == 1 ? gp.R2 : (gp.R2 - 1);
                    if (blockLen2 <= 0) continue;

                    int remainingFull2 = totalFull2 - blockIsFull2;
                    int remainingShort2 = totalShort2 - (1 - blockIsFull2);
                    if (remainingFull2 < 0 || remainingShort2 < 0) continue;

                    int minFB2 = Math.Max(0, numBefore2 - remainingShort2);
                    int maxFB2 = Math.Min(numBefore2, remainingFull2);

                    for (int fb2 = minFB2; fb2 <= maxFB2; fb2++)
                    {
                        int ctStart = fb2 * gp.R2 + (numBefore2 - fb2) * (gp.R2 - 1);
                        int row2 = p - ctStart;

                        if (row2 < 0 || row2 >= blockLen2) continue;

                        int col2Start = blockIsFull2 == 1 ? 0 : gp.FullCols2;
                        int col2End = blockIsFull2 == 1 ? gp.FullCols2 : L;

                        for (int col2 = col2Start; col2 < col2End; col2++)
                        {
                            int interPos = row2 * L + col2;
                            if (interPos < 0 || interPos >= gp.PtLen) continue;

                            int colLen1 = (targetCol < gp.FullCols1) ? gp.R1 : (gp.R1 - 1);
                            int totalShort1 = L - gp.FullCols1;
                            int totalFull1 = gp.FullCols1;
                            int selfFull1 = (targetCol < gp.FullCols1) ? 1 : 0;
                            int otherFull1 = totalFull1 - selfFull1;
                            int otherShort1 = totalShort1 - (1 - selfFull1);

                            for (int v = 1; v <= L; v++)
                            {
                                if (targetCol == col2 && v != w) continue;
                                if (targetCol != col2 && v == w) continue;

                                int numBefore1 = v - 1;
                                int minFB1 = Math.Max(0, numBefore1 - otherShort1);
                                int maxFB1 = Math.Min(numBefore1, otherFull1);

                                for (int fb1 = minFB1; fb1 <= maxFB1; fb1++)
                                {
                                    int interStart = fb1 * gp.R1 + (numBefore1 - fb1) * (gp.R1 - 1);
                                    int r1 = interPos - interStart;

                                    if (r1 == targetRow)
                                    {
                                        if (r1 >= 0 && r1 < colLen1)
                                        {
                                            validPiValues.Add(v);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool RefineDomainsIteratively(
            string ciphertext, Dictionary<char, HashSet<int>> ctPosLookup,
            CribLetterInfo[] cribLetters, int keyLength, GridParams gp,
            List<int>[] domains)
        {
            HashSet<int>[] domainSets = new HashSet<int>[keyLength];
            for (int c = 0; c < keyLength; c++)
                domainSets[c] = new HashSet<int>(domains[c]);

            int totalShort1 = keyLength - gp.FullCols1;
            int totalShort2 = keyLength - gp.FullCols2;

            List<CribLetterInfo>[] lettersByCol = new List<CribLetterInfo>[keyLength];
            for (int c = 0; c < keyLength; c++)
                lettersByCol[c] = new List<CribLetterInfo>();
            foreach (CribLetterInfo cl in cribLetters)
                if (cl.Col1 < keyLength) lettersByCol[cl.Col1].Add(cl);

            bool anyChanged = true;
            while (anyChanged)
            {
                anyChanged = false;

                // 1. Bijection (AllDifferent)
                for (int c = 0; c < keyLength; c++)
                {
                    if (domainSets[c].Count == 1)
                    {
                        int confirmedValue = domainSets[c].First();
                        for (int other = 0; other < keyLength; other++)
                        {
                            if (other == c) continue;
                            if (domainSets[other].Remove(confirmedValue))
                            {
                                anyChanged = true;
                                if (domainSets[other].Count == 0) return false;
                            }
                        }
                    }
                }

                // 2. Hidden Singles
                for (int val = 1; val <= keyLength; val++)
                {
                    int foundIn = -1;
                    int count = 0;
                    for (int c = 0; c < keyLength; c++)
                    {
                        if (domainSets[c].Contains(val))
                        {
                            foundIn = c;
                            count++;
                            if (count > 1) break;
                        }
                    }
                    if (count == 0) return false;
                    if (count == 1 && domainSets[foundIn].Count > 1)
                    {
                        domainSets[foundIn].Clear();
                        domainSets[foundIn].Add(val);
                        anyChanged = true;
                    }
                }

                // 3. Arc Consistency Propagation
                for (int col1 = 0; col1 < keyLength; col1++)
                {
                    if (lettersByCol[col1].Count == 0) continue;

                    List<int> toRemove = null;
                    foreach (int v in domainSets[col1])
                    {
                        bool vValid = true;
                        foreach (CribLetterInfo cribLetter in lettersByCol[col1])
                        {
                            int row1 = cribLetter.Row1;
                            char letter = cribLetter.Letter;

                            int col1Len = (col1 < gp.FullCols1) ? gp.R1 : (gp.R1 - 1);
                            if (row1 >= col1Len) continue;

                            HashSet<int> ctPosSet;
                            if (!ctPosLookup.TryGetValue(letter, out ctPosSet) || ctPosSet.Count == 0)
                            {
                                vValid = false;
                                break;
                            }

                            int selfShort1 = (col1 >= gp.FullCols1) ? 1 : 0;
                            int otherShort1 = totalShort1 - selfShort1;
                            int otherFull1 = (keyLength - 1) - otherShort1;

                            int numBefore = v - 1;
                            int minS1 = Math.Max(0, numBefore - otherFull1);
                            int maxS1 = Math.Min(numBefore, otherShort1);

                            bool foundForV = false;
                            for (int ns1 = minS1; ns1 <= maxS1 && !foundForV; ns1++)
                            {
                                int interPos = numBefore * gp.R1 - ns1 + row1;
                                if (interPos < 0 || interPos >= gp.PtLen) continue;

                                int row2 = interPos / keyLength;
                                int col2 = interPos % keyLength;
                                int col2Len = (col2 < gp.FullCols2) ? gp.R2 : (gp.R2 - 1);
                                if (row2 >= col2Len) continue;

                                int selfShort2 = (col2 >= gp.FullCols2) ? 1 : 0;
                                int otherShort2 = totalShort2 - selfShort2;
                                int otherFull2 = (keyLength - 1) - otherShort2;

                                foreach (int w in domainSets[col2])
                                {
                                    if (col1 == col2 && w != v) continue;
                                    if (col1 != col2 && w == v) continue;

                                    int numBefore2 = w - 1;
                                    int minS2 = Math.Max(0, numBefore2 - otherFull2);
                                    int maxS2 = Math.Min(numBefore2, otherShort2);
                                    for (int ns2 = minS2; ns2 <= maxS2 && !foundForV; ns2++)
                                    {
                                        int ctPos = numBefore2 * gp.R2 - ns2 + row2;
                                        if (ctPos >= 0 && ctPos < ciphertext.Length && ctPosSet.Contains(ctPos))
                                            foundForV = true;
                                    }
                                    if (foundForV) break;
                                }
                            }

                            if (!foundForV)
                            {
                                vValid = false;
                                break;
                            }
                        }

                        if (!vValid)
                        {
                            if (toRemove == null) toRemove = new List<int>();
                            toRemove.Add(v);
                        }
                    }

                    if (toRemove != null && toRemove.Count > 0)
                    {
                        foreach (int v in toRemove) domainSets[col1].Remove(v);
                        anyChanged = true;
                        if (domainSets[col1].Count == 0) return false;
                    }
                }
            }

            for (int c = 0; c < keyLength; c++)
            {
                domains[c] = new List<int>(domainSets[c]);
                domains[c].Sort();
            }
            return true;
        }

        private static void BacktrackCSP(
            string ciphertext, string crib, int keyLength, int nullCount,
            CribLetterInfo[] cribLetters, GridParams gp,
            List<int>[] domains,
            int[] perm, bool[] usedValues, int[] colOrder, int depth,
            List<ScoredCandidate> results, int maxResults,
            ref long tested, Func<bool> isStopped, Action<long> reportProgress,
            DateTime deadline)
        {
            if (results.Count >= maxResults) return;
            if (isStopped != null && isStopped()) return;
            if (DateTime.UtcNow >= deadline) return;

            if (depth == keyLength)
            {
                tested++;
                if (reportProgress != null && tested % 200 == 0)
                    reportProgress(tested);

                string plaintext = UbchiCore.DecryptUbchi(ciphertext, perm, nullCount);

                if (plaintext.Length >= crib.Length &&
                    plaintext.Substring(0, crib.Length) == crib)
                {
                    double freqScore = GermanFrequencyScore(plaintext);
                    double totalScore = crib.Length * 3.0 + freqScore * 2.0;

                    results.Add(new ScoredCandidate
                    {
                        Permutation = (int[])perm.Clone(),
                        Plaintext = plaintext,
                        Score = totalScore,
                        NullCount = nullCount,
                        CribMatched = true,
                        FrequencyScore = freqScore
                    });
                }
                return;
            }

            int col = colOrder[depth];

            foreach (int value in domains[col])
            {
                if (usedValues[value]) continue;

                perm[col] = value;
                usedValues[value] = true;

                if (CheckConsistencyWithForwardCheck(ciphertext, cribLetters, perm, keyLength, gp))
                {
                    BacktrackCSP(ciphertext, crib, keyLength, nullCount,
                        cribLetters, gp, domains,
                        perm, usedValues, colOrder, depth + 1,
                        results, maxResults, ref tested, isStopped, reportProgress, deadline);
                }

                perm[col] = 0;
                usedValues[value] = false;
            }
        }

        private static bool CheckConsistencyWithForwardCheck(
            string ciphertext, CribLetterInfo[] cribLetters, int[] perm, int keyLength,
            GridParams gp)
        {
            int[] orderToCol = new int[keyLength];
            for (int i = 0; i < keyLength; i++) orderToCol[i] = -1;
            for (int c = 0; c < keyLength; c++)
            {
                if (perm[c] != 0)
                    orderToCol[perm[c] - 1] = c;
            }

            int totalShort1 = keyLength - gp.FullCols1;
            int totalShort2 = keyLength - gp.FullCols2;

            for (int i = 0; i < cribLetters.Length; i++)
            {
                int col1 = cribLetters[i].Col1;
                int row1 = cribLetters[i].Row1;
                char letter = cribLetters[i].Letter;

                if (perm[col1] == 0) continue;

                int col1Len = (col1 < gp.FullCols1) ? gp.R1 : (gp.R1 - 1);
                if (row1 >= col1Len) continue;

                int v = perm[col1];

                int readOrder1 = v - 1;
                int offset1 = ComputeExactReadOffset(orderToCol, readOrder1, gp.R1, gp.FullCols1);

                if (offset1 >= 0)
                {
                    int interPos = offset1 + row1;
                    if (interPos < 0 || interPos >= gp.PtLen) continue;

                    int row2 = interPos / keyLength;
                    int col2 = interPos % keyLength;

                    if (perm[col2] != 0)
                    {
                        int w = perm[col2];
                        if (col1 == col2 && w != v) return false;
                        if (col1 != col2 && w == v) return false;

                        int col2Len = (col2 < gp.FullCols2) ? gp.R2 : (gp.R2 - 1);
                        if (row2 >= col2Len) continue;

                        int readOrder2 = perm[col2] - 1;
                        int offset2 = ComputeExactReadOffset(orderToCol, readOrder2, gp.R2, gp.FullCols2);
                        if (offset2 >= 0)
                        {
                            int ctPos = offset2 + row2;
                            if (ctPos >= 0 && ctPos < ciphertext.Length)
                            {
                                if (ciphertext[ctPos] != letter)
                                    return false;
                            }
                            continue;
                        }
                    }
                }

                int assignedOffset1 = 0;
                int unassignedBefore1 = 0;
                for (int ord = 0; ord < v - 1; ord++)
                {
                    int c = orderToCol[ord];
                    if (c >= 0)
                        assignedOffset1 += (c < gp.FullCols1) ? gp.R1 : (gp.R1 - 1);
                    else
                        unassignedBefore1++;
                }
                int minOffset1 = assignedOffset1 + unassignedBefore1 * (gp.R1 - 1);
                int maxOffset1 = assignedOffset1 + unassignedBefore1 * gp.R1;

                bool anyMatch = false;
                for (int off1 = minOffset1; off1 <= maxOffset1 && !anyMatch; off1++)
                {
                    int interPos = off1 + row1;
                    if (interPos < 0 || interPos >= gp.PtLen) continue;

                    int row2 = interPos / keyLength;
                    int col2 = interPos % keyLength;
                    int col2Len = (col2 < gp.FullCols2) ? gp.R2 : (gp.R2 - 1);
                    if (row2 >= col2Len) continue;

                    if (perm[col2] != 0)
                    {
                        int w = perm[col2];
                        if (col1 == col2 && w != v) continue;
                        if (col1 != col2 && w == v) continue;

                        int assignedOffset2 = 0;
                        int unassignedBefore2 = 0;
                        for (int ord = 0; ord < w - 1; ord++)
                        {
                            int c2 = orderToCol[ord];
                            if (c2 >= 0)
                                assignedOffset2 += (c2 < gp.FullCols2) ? gp.R2 : (gp.R2 - 1);
                            else
                                unassignedBefore2++;
                        }
                        int minOff2 = assignedOffset2 + unassignedBefore2 * (gp.R2 - 1);
                        int maxOff2 = assignedOffset2 + unassignedBefore2 * gp.R2;
                        for (int off2 = minOff2; off2 <= maxOff2 && !anyMatch; off2++)
                        {
                            int ctPos = off2 + row2;
                            if (ctPos >= 0 && ctPos < ciphertext.Length && ciphertext[ctPos] == letter)
                                anyMatch = true;
                        }
                    }
                    else
                    {
                        int selfShort2 = (col2 >= gp.FullCols2) ? 1 : 0;
                        int otherShort2 = totalShort2 - selfShort2;
                        int otherFull2 = (keyLength - 1) - otherShort2;
                        for (int w = 1; w <= keyLength && !anyMatch; w++)
                        {
                            if (col1 == col2 && w != v) continue;
                            if (col1 != col2 && w == v) continue;

                            int numBefore2 = w - 1;
                            int minS2 = Math.Max(0, numBefore2 - otherFull2);
                            int maxS2 = Math.Min(numBefore2, otherShort2);
                            for (int ns2 = minS2; ns2 <= maxS2 && !anyMatch; ns2++)
                            {
                                int ctPos = numBefore2 * gp.R2 - ns2 + row2;
                                if (ctPos >= 0 && ctPos < ciphertext.Length && ciphertext[ctPos] == letter)
                                    anyMatch = true;
                            }
                        }
                    }
                }

                if (!anyMatch)
                    return false;
            }

            return true;
        }

        private static int ComputeExactReadOffset(int[] orderToCol, int readOrder, int gridRows, int fullCols)
        {
            int offset = 0;
            for (int ord = 0; ord < readOrder; ord++)
            {
                int col = orderToCol[ord];
                if (col < 0) return -1;
                offset += (col < fullCols) ? gridRows : (gridRows - 1);
            }
            return offset;
        }

        public static double GermanFrequencyScore(string text)
        {
            double chi2 = ChiSquaredGerman(text);
            return Math.Max(0.0, 1.0 - chi2 / 200.0);
        }

        public static double ChiSquaredGerman(string text)
        {
            if (string.IsNullOrEmpty(text)) return double.MaxValue;

            int[] counts = new int[26];
            int total = 0;
            foreach (char c in text)
            {
                if (c >= 'A' && c <= 'Z')
                {
                    counts[c - 'A']++;
                    total++;
                }
            }
            if (total == 0) return double.MaxValue;

            double chi2 = 0;
            for (int i = 0; i < 26; i++)
            {
                double expected = (GermanFreq[i] / 100.0) * total;
                if (expected < 0.001) expected = 0.001;
                double diff = counts[i] - expected;
                chi2 += (diff * diff) / expected;
            }
            return chi2;
        }
    }

    public class ScoredCandidate
    {
        public int[] Permutation { get; set; }
        public string Plaintext { get; set; }
        public double Score { get; set; }
        public int NullCount { get; set; }
        public bool CribMatched { get; set; }
        public double FrequencyScore { get; set; }
    }

    internal struct CribLetterInfo
    {
        public char Letter;
        public int Row1;
        public int Col1;
    }

    internal struct GridParams
    {
        public int L;
        public int PtLen;
        public int CtLen;
        public int R1;
        public int FullCols1;
        public int R2;
        public int FullCols2;

        public GridParams(int keyLength, int ptLen, int ctLen)
        {
            L = keyLength;
            PtLen = ptLen;
            CtLen = ctLen;
            R1 = (ptLen + keyLength - 1) / keyLength;
            FullCols1 = ptLen % keyLength;
            if (FullCols1 == 0) FullCols1 = keyLength;
            R2 = (ctLen + keyLength - 1) / keyLength;
            FullCols2 = ctLen % keyLength;
            if (FullCols2 == 0) FullCols2 = keyLength;
        }
    }
}
