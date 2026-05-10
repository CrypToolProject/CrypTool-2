using System;
using System.Collections.Generic;
using System.Linq;

namespace CrypTool.Plugins.UbchiPermutationCryptanalysis
{
    /// <summary>
    /// CSP-based crib cryptanalysis of the Übchi double columnar transposition cipher.
    /// Uses the "all-letter row-multiple" approach.
    ///
    /// The cipher uses the SAME permutation ? for both transpositions (?1 = ?2 = ?).
    ///
    /// Key idea:
    ///   After a columnar transposition, a letter from row r in column c ends up at a
    ///   ciphertext position that is approximately r + k*R (a multiple of the row count
    ///   plus offset). Even common letters like E don't appear at every such position
    ///   in the ciphertext. By checking ALL crib letters (not just rare Q,J,Z,V,W,X,Y,P),
    ///   we restrict domains for ALL columns — not just the 2-3 anchor columns.
    ///
    ///   For double transposition: after the first transposition, a crib letter moves to
    ///   an intermediate position ? (?(c)-1)*R1 + row1. In the second grid, it lands in
    ///   some (row2, col2). After the second transposition with ?(col2)=w, it ends up at
    ///   ciphertext position ? (w-1)*R2 + row2. We check if the letter actually appears
    ///   at that ciphertext position. Multiple letters per column ? intersect valid sets.
    ///
    /// Algorithm:
    ///   1. For EVERY crib letter in EVERY column, compute which ?(c) values are
    ///      compatible with that letter appearing in the ciphertext after the double
    ///      transposition. Multiple letters in the same column ? intersect domains.
    ///   2. Solve the CSP using backtracking with MRV heuristic.
    ///   3. During backtracking, check consistency of all crib letters through both
    ///      transpositions whenever the full path is computable.
    ///   4. Full permutation ? decrypt and verify crib match.
    /// </summary>
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

                // Build crib letter info sorted row-0 first
                CribLetterInfo[] cribLetters = BuildCribLetters(currentCrib, keyLength);
                Array.Sort(cribLetters, (a, b) => a.Row1.CompareTo(b.Row1));

                // Build ciphertext position lookup (HashSet for O(1) lookup)
                Dictionary<char, HashSet<int>> ctPosLookup = BuildLetterPositionSets(ciphertext);

                // Sort crib letters by increasing frequency in the CURRENT ciphertext
                // so rare letters prune domains and backtracking earlier.
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

                // Compute restricted domains for ALL columns using ALL crib letters
                List<int>[] domains = ComputeAllColumnDomains(ciphertext, ctPosLookup, cribLetters, keyLength, gp);

                // Arc consistency: cross-column constraint propagation
                if (!RefineDomainsIteratively(ciphertext, ctPosLookup, cribLetters, keyLength, gp, domains))
                    continue;

                // Check for empty domains — this key length is impossible
                bool impossible = false;
                for (int c = 0; c < keyLength; c++)
                {
                    if (domains[c].Count == 0) { impossible = true; break; }
                }
                if (impossible) continue;

                // Column assignment order: smallest domain first (MRV heuristic)
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

        /// <summary>
        /// Compute domains for ALL columns using ALL crib letters (not just rare ones).
        ///
        /// For each column c, collect every crib letter that falls in that column.
        /// For each such letter, determine which ?(c) values allow it to end up at
        /// a matching position in the ciphertext through the double transposition.
        /// Multiple letters in the same column ? intersect their valid value sets.
        ///
        /// The key insight:
        ///   After a columnar transposition, a character from row r in column c
        ///   ends up at position ? (?(c)-1)*R + r. Even common letters like E
        ///   don't appear at every such "approximately multiple of R" position
        ///   in the ciphertext. So checking ALL letters restricts ALL columns,
        ///   not just the rare-letter columns.
        /// </summary>
        private static List<int>[] ComputeAllColumnDomains(
            string ciphertext, Dictionary<char, HashSet<int>> ctPosLookup,
            CribLetterInfo[] cribLetters, int keyLength, GridParams gp)
        {
            List<int>[] domains = new List<int>[keyLength];
            for (int c = 0; c < keyLength; c++)
                domains[c] = Enumerable.Range(1, keyLength).ToList();

            // Group crib letters by column
            List<CribLetterInfo>[] lettersByCol = new List<CribLetterInfo>[keyLength];
            for (int c = 0; c < keyLength; c++)
                lettersByCol[c] = new List<CribLetterInfo>();
            for (int i = 0; i < cribLetters.Length; i++)
            {
                int col = cribLetters[i].Col1;
                if (col < keyLength)
                    lettersByCol[col].Add(cribLetters[i]);
            }

            int totalShort2 = keyLength - gp.FullCols2;

            // Process each column
            for (int c = 0; c < keyLength; c++)
            {
                List<CribLetterInfo> lettersInCol = lettersByCol[c];
                if (lettersInCol.Count == 0) continue;

                HashSet<int> validForCol = null;

                foreach (CribLetterInfo cribLetter in lettersInCol)
                {
                    int row1 = cribLetter.Row1;
                    char letter = cribLetter.Letter;

                    int col1Len = (c < gp.FullCols1) ? gp.R1 : (gp.R1 - 1);
                    if (row1 >= col1Len) continue;

                    HashSet<int> ctPosSet;
                    if (!ctPosLookup.TryGetValue(letter, out ctPosSet) || ctPosSet.Count == 0)
                    {
                        validForCol = new HashSet<int>();
                        break;
                    }

                    HashSet<int> validForLetter = new HashSet<int>();

                    // First transposition offset ranges for column c
                    int totalShort1 = keyLength - gp.FullCols1;
                    int selfShort1 = (c >= gp.FullCols1) ? 1 : 0;
                    int otherShort1 = totalShort1 - selfShort1;
                    int otherFull1 = (keyLength - 1) - otherShort1;

                    for (int v = 1; v <= keyLength; v++)
                    {
                        int numBefore1 = v - 1;
                        int minS1 = Math.Max(0, numBefore1 - otherFull1);
                        int maxS1 = Math.Min(numBefore1, otherShort1);

                        bool foundForV = false;
                        for (int ns1 = minS1; ns1 <= maxS1 && !foundForV; ns1++)
                        {
                            int interPos = numBefore1 * gp.R1 - ns1 + row1;
                            if (interPos < 0 || interPos >= gp.PtLen) continue;

                            int row2 = interPos / keyLength;
                            int col2 = interPos % keyLength;
                            int col2Len = (col2 < gp.FullCols2) ? gp.R2 : (gp.R2 - 1);
                            if (row2 >= col2Len) continue;

                            // Second transposition: try all w for col2
                            int selfShort2 = (col2 >= gp.FullCols2) ? 1 : 0;
                            int otherShort2 = totalShort2 - selfShort2;
                            int otherFull2 = (keyLength - 1) - otherShort2;

                            for (int w = 1; w <= keyLength && !foundForV; w++)
                            {
                                int numBefore2 = w - 1;
                                int minS2 = Math.Max(0, numBefore2 - otherFull2);
                                int maxS2 = Math.Min(numBefore2, otherShort2);

                                for (int ns2 = minS2; ns2 <= maxS2 && !foundForV; ns2++)
                                {
                                    int ctPos = numBefore2 * gp.R2 - ns2 + row2;
                                    if (ctPos >= 0 && ctPos < ciphertext.Length && ctPosSet.Contains(ctPos))
                                    {
                                        foundForV = true;
                                    }
                                }
                            }
                        }

                        if (foundForV) validForLetter.Add(v);
                    }

                    // Intersect with other letters in the same column
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

        /// <summary>
        /// Arc consistency propagation.
        /// Uses the CURRENT RESTRICTED domain(col2) when checking validity of v for col1.
        /// When domain(col2) shrinks (due to other crib letters), v values that relied
        /// on removed w values are invalidated. Iterates until stable.
        /// Returns false if any domain becomes empty.
        /// </summary>
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

        /// <summary>
        /// Backtracking CSP solver with MRV heuristic, forward-checking, and
        /// double-transposition consistency checking.
        /// </summary>
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

                // Full permutation found — decrypt and verify crib match
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

        /// <summary>
        /// Combined consistency check with forward-checking.
        /// 
        /// Two levels of checking:
        /// 1. EXACT CHECK: When both col1 and col2 are assigned AND all preceding columns
        ///    in both reading orders are assigned, verify ciphertext[ctPos] == letter.
        /// 2. FORWARD CHECK: When col1 is assigned to ?(col1)=v, compute the range
        ///    of possible ciphertext positions using offset ranges.
        ///    Check if the letter appears at ANY of those positions. This works even when
        ///    not all preceding columns are assigned yet.
        /// </summary>
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

                // Try exact check first
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

                // Forward check with offset ranges
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

        /// <summary>
        /// Compute the exact read offset for a column at the given reading order.
        /// Sums the actual lengths of all columns read before this one (orders 0..readOrder-1).
        /// Returns -1 if any preceding column in the reading order is not yet assigned.
        /// </summary>
        private static int ComputeExactReadOffset(int[] orderToCol, int readOrder, int gridRows, int fullCols)
        {
            int offset = 0;
            for (int ord = 0; ord < readOrder; ord++)
            {
                int col = orderToCol[ord];
                if (col < 0) return -1; // preceding column not assigned yet
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

    /// <summary>
    /// Precomputed grid dimensions for both transpositions.
    /// </summary>
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

