/*
   Copyright 2020 George Lasry
   Converted in 2020 from Java to C# by Nils Kopal

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
using System.Text;
using static EnigmaAnalyzerLib.Common.Utils;

namespace EnigmaAnalyzerLib
{
    public class SubGraphItem
    { // edge (link) between two letters

        /// <summary>
        /// Position in ciphertext
        /// </summary>
        public int pos
        {
            get;
            set;
        }

        /// <summary>
        /// Left side of link
        /// </summary>
        public short l1
        {
            get;
            set;
        }

        /// <summary>
        /// Right side of link
        /// </summary>
        public short l2
        {
            get;
            set;
        }

        /// <summary>
        /// Used to order the graph by distance (bfs)
        /// </summary>
        public int dist
        {
            get;
            set;
        }

        public SubGraphItem(int nPos, short nL1, short nL2, int nDist)
        {
            pos = nPos;
            l1 = nL1;
            l2 = nL2;
            dist = nDist;
        }
    }

    /// <summary>
    /// Connected subgraph (undirected)
    /// </summary>
    public class SubGraph
    {
        public List<SubGraphItem> items = new List<SubGraphItem>();   // links/edges
        public int closures;                                          // number of loops

        public void addItem(SubGraphItem item)
        {
            items.Add(item);
        }
    }

    public class BombeMenu
    {
        private const int MAXMENUL = 100;
        private const short UNASSIGNED = -1;

        // todo - add setters and getters
        public short[] crib = new short[BombeCrib.MAXCRIBL];
        public int cribLen;
        public int cribStartPos;

        private readonly SubGraph[] subGraphs = new SubGraph[MAXMENUL];
        public int nSubgraphs;

        public int totalClosures;
        public int totalItems;
        public double score;

        public BombeMenu(int nPos, int nCrlen, short[] nCrib)
        {
            cribStartPos = nPos;
            cribLen = nCrlen;
            Array.Copy(nCrib, 0, crib, 0, cribLen);

            nSubgraphs = 0;

            score = BombeCrib.BADSCORE;
            totalItems = 0;
            nSubgraphs = 0;
        }

        // Sort the subgraphs according to their score (the most discriminative with highest score first)
        public void sortSubgraphs(bool print)
        {
            if (nSubgraphs < 2)
            {
                return;
            }
            bool changed;
            do
            {
                changed = false;
                for (int i = 0; i < (nSubgraphs - 1); i++)
                {
                    if (BombeCrib.score(subGraphs[i].closures, subGraphs[i].items.Count) >
                            BombeCrib.score(subGraphs[i + 1].closures, subGraphs[i + 1].items.Count))
                    {

                        SubGraph temp = subGraphs[i];
                        subGraphs[i] = subGraphs[i + 1];
                        subGraphs[i + 1] = temp;

                        changed = true;
                    }
                }
            } while (changed);

            if (print)
            {
                Console.WriteLine("Sorting subgraphs for crib at position {0}(to optimize validity checks on ciphertext)", cribStartPos);
                Console.WriteLine("Total {0} subgraphs found, total {1} closures and {2} links, Turing score: {3} ",
                        nSubgraphs, totalClosures, totalItems, score);

                for (int subgraph = 0; subgraph < nSubgraphs; subgraph++)
                {
                    Console.WriteLine("Subgraph {0} with {1} Closures, {2} Links",
                            subgraph, subGraphs[subgraph].closures,
                            subGraphs[subgraph].items.Count);
                    foreach (SubGraphItem item in subGraphs[subgraph].items)
                    {
                        Console.WriteLine("({0}) Link {1}->{2} at pos {3}",
                                subgraph, EnigmaUtils.getChar(item.l1),
                                EnigmaUtils.getChar(item.l2), item.pos);
                    }
                }
                Console.WriteLine("");
            }
        }

        /// <summary>
        /// The main Bombe stopping algorithm - may be called recursively if more than one subgraph.
        /// Scrambler means the substitution alphabets for each position, for the given settings, taking into
        /// account the rotors-to-reflector and back path. Does not take into account stecker plugs, as the purpose of the
        /// Turing Bombe is not only to check for a stop, but also to retrieve most or all of the stecker plugs.
        /// </summary>
        /// <param name="sg"></param>
        /// <param name="scramblerLookupFlat"></param>
        /// <param name="stbAssumed"></param>
        /// <param name="stbStrength"></param>
        /// <param name="print"></param>
        /// <returns></returns>
        public bool testIfBombsStops(int sg, short[] scramblerLookupFlat,
                                    short[] stbAssumed, short[] stbStrength, bool print)
        {
            short firstLetter = subGraphs[sg].items[0].l1;
            List<short> pairedLettersToCheck = new List<short>();

            if (stbAssumed[firstLetter] == UNASSIGNED)
            {
                // This is what we get when called externally (not a recursive call)
                // always try first self steckered for the first letter - it may or may not be already defined as such
                pairedLettersToCheck.Add(firstLetter);
                //then add all unassigned
                for (short pairedLetter = 0; pairedLetter < 26; pairedLetter++)
                {
                    if (pairedLetter == firstLetter)
                    {
                        continue;
                    }
                    // add the letters not yet mapped
                    if (stbAssumed[pairedLetter] == UNASSIGNED)
                    {
                        pairedLettersToCheck.Add(pairedLetter);
                    }
                }
            }
            else
            {
                // This is what we get when called internally (recursion for subgraph)
                // stick with what we have in currently assumed stb (1 option)
                pairedLettersToCheck.Add(stbAssumed[firstLetter]);
            }

            bool valid;

            // out of the loop for performance
            short[] stbAssumedTemp = new short[26];  // Assumed stecker plugs so far
            short[] stbStrengthTemp = new short[26]; // For each letter and its assumed stecker mapping,
                                                     // how may times the assumption has been made.
                                                     // iterate on all assumptions for first letter
            foreach (short pairedLetter in pairedLettersToCheck)
            {

                // For printing...
                string assumptionS = "" + EnigmaUtils.getChar(pairedLetter) + EnigmaUtils.getChar(firstLetter);

                // create local copies, and copy back only if returning valid
                Array.Copy(stbStrength, 0, stbStrengthTemp, 0, 26);
                Array.Copy(stbAssumed, 0, stbAssumedTemp, 0, 26);

                if (stbAssumedTemp[firstLetter] == UNASSIGNED)
                {
                    stbAssumedTemp[pairedLetter] = firstLetter;
                    stbAssumedTemp[firstLetter] = pairedLetter;

                    if (print)
                    {
                        Console.WriteLine("(SG {0}) VALIDITY CHECK - Assuming [{1}]", sg, assumptionS);
                    }

                }
                else if (stbAssumedTemp[firstLetter] == pairedLetter)
                {

                    // do nothing
                    if (print)
                    {
                        Console.WriteLine("(SG {0}) VALIDITY CHECK - Assumption [{1}] - Already assumed from previous subgraph",
                                sg, assumptionS);
                    }

                }
                else
                {
                    // should never happen!
                    Console.WriteLine("(SG {0}) VALIDITY CHECK - CRITICAL ERROR Assumption [{1}] contradicts previous settings for first letter", sg, assumptionS);
                    return false;
                }

                valid = true;
                foreach (SubGraphItem item in subGraphs[sg].items)
                {
                    // EnigmaIn =>[STB]=> ScramblerIn =>[SCRAMBLER]=> ScramblerOut =>[STB]=> EnigmaOut
                    short enigmaIn;
                    short enigmaOut;
                    if (stbAssumedTemp[item.l1] != UNASSIGNED)
                    {
                        enigmaIn = item.l1;
                        enigmaOut = item.l2;
                    }
                    else if (stbAssumedTemp[item.l2] != UNASSIGNED)
                    {
                        enigmaIn = item.l2;
                        enigmaOut = item.l1;
                    }
                    else
                    {
                        Console.WriteLine("(SG: {0}) VALIDITY CHECK - CRITICAL ERROR - SUBGRAPH ITEMS NOT SORTED!", sg);
                        return false;
                    }

                    short scramblerIn = stbAssumedTemp[enigmaIn];
                    short scramblerOut = scramblerLookupFlat[(item.pos << 5) + scramblerIn];

                    //time consuming - compute only if to print
                    if (print)
                    {
                        Console.WriteLine("(SG {0}) CHECKING: {1}->{2}  at position {3}, Current STB: [{4} {5}]: [{6}] <-STB-> [{7}] <-Rotors-UKW-Rotors-> [{8}] <-STB-> [{9}]",
                                sg,
                                EnigmaUtils.getChar(item.l1),
                                EnigmaUtils.getChar(item.l2),
                                item.pos,
                                stbPairsString(stbAssumedTemp, null),
                                stbSelfsString(stbAssumedTemp, null),
                                EnigmaUtils.getChar(enigmaIn),
                                EnigmaUtils.getChar(scramblerIn),
                                EnigmaUtils.getChar(scramblerOut),
                                EnigmaUtils.getChar(enigmaOut));
                    }

                    if ((stbAssumedTemp[enigmaOut] == UNASSIGNED) && (stbAssumedTemp[scramblerOut] == UNASSIGNED))
                    {
                        if (enigmaOut == scramblerOut)
                        {
                            if (print)
                            {
                                Console.WriteLine("(SG {0}) PASSED - Self [{1}] valid - {2} was undefined in STB",
                                        sg, EnigmaUtils.getChar(scramblerOut), EnigmaUtils.getChar(scramblerOut));
                            }
                            stbAssumedTemp[scramblerOut] = scramblerOut;
                        }
                        else
                        {
                            if (print)
                            {
                                Console.WriteLine("(SG {0}) PASSED - Pair [{1}{2}] valid - Both {3} and {4} were undefined in STB",
                                        sg, EnigmaUtils.getChar(scramblerOut), EnigmaUtils.getChar(enigmaOut), EnigmaUtils.getChar(scramblerOut), EnigmaUtils.getChar(enigmaOut));
                            }
                            stbAssumedTemp[enigmaOut] = scramblerOut;
                            stbAssumedTemp[scramblerOut] = enigmaOut;

                        }
                    }
                    else if ((stbAssumedTemp[scramblerOut] != UNASSIGNED) && (stbAssumedTemp[scramblerOut] != enigmaOut))
                    {

                        if (print)
                        {
                            if (enigmaOut == scramblerOut)
                            {
                                Console.WriteLine("(SG {0}) FAILED - Assumption [{1}] rejected - Self [{2}] contradicts STB",
                                        sg, assumptionS, EnigmaUtils.getChar(scramblerOut));
                            }
                            else
                            {
                                Console.WriteLine("(SG {0}) FAILED - Assumption [{1}] rejected - Forward Pair [{2}{3}] contradicts STB",
                                        sg, assumptionS, EnigmaUtils.getChar(scramblerOut), EnigmaUtils.getChar(enigmaOut));
                            }
                        }
                        valid = false;
                        break;

                    }
                    else if ((stbAssumedTemp[enigmaOut] != UNASSIGNED) && (stbAssumedTemp[enigmaOut] != scramblerOut))
                    {

                        if (print)
                        {
                            if (enigmaOut == scramblerOut)
                            {
                                Console.WriteLine("(SG {0}) FAILED - Assumption [{1}] rejected - Self [{2}] contradicts STB",
                                        sg, assumptionS, EnigmaUtils.getChar(scramblerOut));
                            }
                            else
                            {
                                Console.WriteLine("(SG {0}) FAILED - Assumption [{1}] rejected - Backward Pair [{2}{3}] contradicts STB",
                                        sg, assumptionS, EnigmaUtils.getChar(scramblerOut), EnigmaUtils.getChar(enigmaOut));
                            }
                        }
                        valid = false;
                        break;

                    }
                    else
                    {
                        if (enigmaOut == scramblerOut)
                        {
                            if (print)
                            {
                                Console.WriteLine("(SG {0}) PASSED - Self [{1}] confirmed for STB",
                                        sg, EnigmaUtils.getChar(scramblerOut));
                            }
                            stbStrengthTemp[enigmaOut]++;
                        }
                        else
                        {
                            if (print)
                            {
                                Console.WriteLine("(SG {0}) PASSED - Pair [{1}{2}] confirmed for STB",
                                        sg, EnigmaUtils.getChar(scramblerOut), EnigmaUtils.getChar(enigmaOut));
                            }

                            // count only in one direction (the lowest of the pair)
                            if (enigmaOut < scramblerOut)
                            {
                                stbStrengthTemp[enigmaOut]++;
                            }
                            else
                            {
                                stbStrengthTemp[scramblerOut]++;
                            }
                        }
                    }
                }

                // No conflicts so far.
                if (valid)
                {
                    if (print)
                    {
                        Console.WriteLine("(SG {0}) PASS - Subgraph {1} check complete", sg, sg);
                    }

                    // Any more subgraph to check (recursively).
                    if (sg < (nSubgraphs - 1))
                    {

                        if (print)
                        {
                            Console.WriteLine("CHECK INTO SG {0} ", sg + 1);
                        }

                        valid = testIfBombsStops(sg + 1, scramblerLookupFlat, stbAssumedTemp, stbStrengthTemp, print);

                        if (print)
                        {
                            if (!valid)
                            {
                                Console.WriteLine("FAILED BACK FROM SG {0} ", sg + 1);
                            }
                            else
                            {
                                Console.WriteLine("SUCCESS BACK FROM SG {0} ", sg + 1);
                            }
                        }

                    }
                    // Subgraphs also validated
                    if (valid)
                    {
                        if (print)
                        {
                            // Are we at top level graph?
                            if (sg == 0)
                            {
                                Console.WriteLine("COMPLETE - STB [{0} {1}] - Pos {2} - Crib >{3}< - Tested {4} subgraphs",
                                        stbPairsString(stbAssumedTemp, stbStrengthTemp),
                                        stbSelfsString(stbAssumedTemp, stbStrengthTemp),
                                        cribStartPos, EnigmaUtils.getstring(crib, cribLen), nSubgraphs);
                            }
                            else
                            {
                                Console.WriteLine("(SG {0}) COMPLETE - STB [{1} {2}] ",
                                        sg,
                                        stbPairsString(stbAssumedTemp, stbStrengthTemp),
                                        stbSelfsString(stbAssumedTemp, stbStrengthTemp));
                            }
                        }

                        // Copy back the values to return.
                        Array.Copy(stbAssumedTemp, 0, stbAssumed, 0, 26);
                        Array.Copy(stbStrengthTemp, 0, stbStrength, 0, 26);

                        return true;
                    }
                }

            }

            // check nothing spoiled is returned
            Key.checkStecker(stbAssumed, string.Format("VALIDITY TEST - SG ={0} RETURNING FAILURE (GLOBAL)", sg));

            return false;
        }

        public void addSubgraph(int[][] links, int nClosures, int[] letterUsage, bool print)
        {
            SubGraphItem[] tempItems = new SubGraphItem[MAXMENUL];
            int numberOfItems = 0;
            SubGraph subGraph = subGraphs[nSubgraphs] = new SubGraph();
            subGraph.closures = nClosures;
            totalClosures += subGraph.closures;

            if (print)
            {
                StringBuilder letterUsageS = new StringBuilder();
                StringBuilder letterUsagePlusS = new StringBuilder();
                for (int j = 0; j < 26; j++)
                {
                    if (letterUsage[j] > 0)
                    {
                        letterUsageS.Append(EnigmaUtils.getChar(j));
                    }
                    if (letterUsage[j] > 1)
                    {
                        letterUsagePlusS.Append(EnigmaUtils.getChar(j));
                    }
                }
                Console.WriteLine("Found menu subgraph ({0} total) using letters [{1}] - Letters [{2}] traversed more than once",
                        nSubgraphs, letterUsageS.ToString(), letterUsagePlusS.ToString());
            }

            for (short i = 0; i < 26; i++)
            {
                for (short j = (short)(i + 1); j < 26; j++)
                {
                    if ((letterUsage[i] == 0) || (letterUsage[j] == 0))
                    {
                        continue;
                    }
                    if (links[i][j] != -1)
                    {
                        tempItems[numberOfItems++] = new SubGraphItem(links[i][j], i, j, -1);
                    }
                    else if (links[j][i] != -1)
                    {
                        tempItems[numberOfItems++] = new SubGraphItem(links[j][i], j, i, -1);
                    }
                }
            }

            for (int i = 0; i < numberOfItems; i++)
            {
                tempItems[i].dist = 1000;
            }

            int[] letterDist = new int[26];
            Arrays.fill(letterDist, 1000);

            letterDist[tempItems[0].l1] = 0;
            for (int dist = 0; dist < 26; dist++)
            {
                for (int i = 0; i < numberOfItems; i++)
                {
                    letterDist[tempItems[i].l1] =
                            Math.Min(letterDist[tempItems[i].l1], letterDist[tempItems[i].l2] + 1);
                    letterDist[tempItems[i].l2] =
                            Math.Min(letterDist[tempItems[i].l2], letterDist[tempItems[i].l1] + 1);
                    tempItems[i].dist = Math.Min(tempItems[i].dist, letterDist[tempItems[i].l1]);
                    tempItems[i].dist = Math.Min(tempItems[i].dist, letterDist[tempItems[i].l2]);

                }
            }

            for (int dist = 0; dist < 26; dist++)
            {
                for (int i = 0; i < numberOfItems; i++)
                {
                    if (tempItems[i].dist == dist)
                    {
                        subGraph.addItem(tempItems[i]);
                    }
                }
            }

            totalItems += subGraph.items.Count;
            nSubgraphs++;

            if (print)
            {
                foreach (SubGraphItem item in subGraph.items)
                {
                    Console.WriteLine("({0}) [Dist {1}] Link {2}->{3} AT {4}",
                            nSubgraphs - 1,
                            item.dist + 1,
                            EnigmaUtils.getChar(item.l1),
                            EnigmaUtils.getChar(item.l2),
                            item.pos);
                }
                Console.WriteLine("Summary for subgraph {0} (pos {1}): {2} Closures, {3} Links",
                        nSubgraphs - 1, cribStartPos, subGraph.closures,
                        subGraph.items.Count);
            }
        }

        private string stbPairsString(short[] assumedStecker, short[] strength)
        {
            StringBuilder stbs = new StringBuilder();
            for (int k = 0; k < 26; k++)
            {
                int s = assumedStecker[k];
                if (k < s)
                {
                    stbs.Append(EnigmaUtils.getChar(k)).Append(EnigmaUtils.getChar(s));
                    if ((strength != null) && (strength[k] > 0))
                    {
                        stbs.Append("{").Append("").Append(strength[k]).Append("}");
                    }
                }
            }
            if (stbs.Length == 0)
            {
                return "Pairs: None";
            }
            else
            {
                return "Pairs: " + stbs.ToString();
            }
        }

        private string stbSelfsString(short[] assumedStecker, short[] strength)
        {
            string sfS = "";
            for (int k = 0; k < 26; k++)
            {
                int s = assumedStecker[k];
                if (k < s)
                {

                }
                else if (k == s)
                {
                    sfS += "" + EnigmaUtils.getChar(k);
                    if ((strength != null) && (strength[k] > 0))
                    {
                        sfS += "{" + strength[k] + "}";
                    }
                }
            }
            if (sfS.Length != 0)
            {
                sfS = "Self: " + sfS;
            }
            return sfS;
        }
    }
}
