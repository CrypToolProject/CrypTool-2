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

namespace EnigmaAnalyzerLib
{
    public class BombeCrib
    {
        public const int MAXCRIBL = 100;
        // todo - add setters and getters
        private readonly short[] crib = new short[MAXCRIBL];
        public const double BADSCORE = 1000.0;
        private readonly int cribLen;
        private readonly int cribPos; // position of the crib in the cipher text
        public BombeMenu menu;

        public BombeCrib(short[] ciphertext, short[] cr, int crl, int cpos, bool print)
        {

            cribPos = cpos;
            Array.Copy(cr, 0, crib, 0, crl);
            cribLen = crl;

            int[][] links = new int[26][];  // positions in which letters i and j correspond to input/output of scramblers (earliest position)
            for (int i = 0; i < 26; i++)
            {
                links[i] = new int[26];
            }

            if (print)
            {
                for (int i = 0; i < cribLen; i++)
                {
                    Console.WriteLine("{0}", ((i + cpos) / 10) % 10);
                }
                Console.WriteLine("");
                for (int i = 0; i < cribLen; i++)
                {
                    Console.WriteLine("{0}", (i + cpos) % 10);
                }
                Console.WriteLine("");
                for (int i = 0; i < cribLen; i++)
                {
                    Console.WriteLine("{0}", EnigmaUtils.getChar(ciphertext[i + cpos]));
                }
                Console.WriteLine("");
                for (int i = 0; i < cribLen; i++)
                {
                    Console.WriteLine("{0}", EnigmaUtils.getChar(crib[i]));
                }
                Console.WriteLine("");
                for (int i = 0; i < cribLen; i++)
                {
                    Console.WriteLine("{0}", i % 10);
                }
                Console.WriteLine("");
            }
            for (int i = 0; i < 26; i++)
            {
                for (int j = 0; j < 26; j++)
                {
                    links[i][j] = -1;
                }
            }

            for (int j = 0; j < cribLen; j++)
            {
                int ctletter = ciphertext[cpos + j];
                int crletter = crib[j];
                if (crletter != -1)
                {
                    if (links[ctletter][crletter] == -1)
                    {
                        links[ctletter][crletter] = cpos + j;
                    }
                }
            }
            createMenu(links, print);
        }

        // public static utilities
        public static double score(int closures, int count)
        {
            double score;
            switch (count)
            {
                case 0:
                    score = 10000000.0;
                    break;
                case 1:
                    score = 1600000.0;
                    break;
                case 2:
                    score = 800000.0;
                    break;
                case 3:
                    score = 450000.0;
                    break;
                case 4:
                    score = 300000.0;
                    break;
                case 5:
                    score = 180000.0;
                    break;
                case 6:
                    score = 120000.0;
                    break;
                case 7:
                    score = 70000.0;
                    break;
                case 8:
                    score = 40000.0;
                    break;
                case 9:
                    score = 19000.0;
                    break;
                case 10:
                    score = 7300.0;
                    break;
                case 11:
                    score = 2700.0;
                    break;
                case 12:
                    score = 820.0;
                    break;
                case 13:
                    score = 200.0;
                    break;
                case 14:
                    score = 43.0;
                    break;
                case 15:
                    score = 7.3;
                    break;
                case 16:
                    score = 1.0;
                    break;
                case 17:
                    score = 0.125;
                    break;
                case 18:
                    score = 0.015;
                    break;

                default:
                    score = 0.001;
                    break;
            }
            for (int i = 0; i < closures; i++)
            {
                score /= 26.0;
            }

            if (score < 0.001)
            {
                score = 0.001;
            }
            return score;
        }

        public static int nextValidPosition(short[] ct, int ctl, short[] cr, int crl, int pos)
        {
            int validPos = -1;
            for (int i = pos; (i <= (ctl - crl)) && (validPos == -1); i++)
            {
                validPos = i;
                for (int j = 0; j < crl; j++)
                {
                    if (ct[i + j] == cr[j])
                    {
                        validPos = -1;
                        break;
                    }
                }
            }
            return validPos;
        }

        // Create and populate a menu and its subgraphs
        private void createMenu(int[][] links, bool print)
        {
            if (print)
            {
                Console.WriteLine("Creating menu subgraphs - Links for crib at position {0}: ", cribPos);
                for (int i = 0; i < 26; i++)
                {
                    for (int j = 0; j < 26; j++)
                    {
                        if (links[i][j] != -1)
                        {
                            Console.WriteLine("{0}->{1} at {2}, ", EnigmaUtils.getChar(i), EnigmaUtils.getChar(j), links[i][j]);

                        }
                        Console.WriteLine("");
                    }
                }
            }

            menu = new BombeMenu(cribPos, cribLen, crib);

            // Count how many times letter has been traverse, for the whole graph.
            int[] graphLetterUsage = new int[26];
            for (int letter = 0; letter < 26; letter++)
            {
                graphLetterUsage[letter] = 0;
            }

            for (int letter = 0; letter < 26; letter++)
            {
                // Start subgraph traversal with unused letter.
                if (graphLetterUsage[letter] > 0)
                {
                    continue;
                }

                // Count how many times letter has been traversed, for the subgraph.
                int[] subgraphLetterUsage = new int[26];
                for (int k = 0; k < 26; k++)
                {
                    subgraphLetterUsage[k] = 0;
                }

                traverseSubgraph(links, 0, "", letter, subgraphLetterUsage, -1);

                int subgraphLetterCount = 0;
                int subgraphClosures = 0;
                for (int k = 0; k < 26; k++)
                {
                    if (subgraphLetterUsage[k] > 0)
                    {
                        graphLetterUsage[k] += subgraphLetterUsage[k];
                        subgraphLetterCount++;
                        if (subgraphLetterUsage[k] > 1)
                        {
                            subgraphClosures += (subgraphLetterUsage[k] - 1);
                        }
                    }
                }
                subgraphClosures /= 2; //they were counted twice ....

                // A subgraph should have at least 2 letters.
                if (subgraphLetterCount < 2)
                {
                    continue;
                }

                menu.addSubgraph(links, subgraphClosures, subgraphLetterUsage, print);


            } // for letter ...

            // Compute the Turing score for the whole graph.
            menu.score = score(menu.totalClosures, menu.totalItems);

            if (print)
            {
                Console.WriteLine("Total {0} subgraphs found, total {1} closures and {2} links, Turing score: {3} ",
                        menu.nSubgraphs, menu.totalClosures, menu.totalItems, menu.score);
            }

            // Sort the subgraphs by score. When running the bombe to test a certain settings, we will start checking the
            // subgraph with the highest score.
            menu.sortSubgraphs(print);

        }

        // Count the usages of the letters in a subgraph starting from a given letter.
        // stopPosition is specified to avoid endless loops in recursive call. If we get back to that position, no need to continue.
        private void traverseSubgraph(int[][] links, int level, string prefix, int startLetter, int[] letterUsage, int stopPositiom)
        {

            // Level and prefix used for debug only.
            prefix += EnigmaUtils.getChar(startLetter);

            //Count how many times a letter has been used (traversed) - if twice, then we have closure.
            letterUsage[startLetter]++;
            if (letterUsage[startLetter] > 1)
            {
                return;
            }

            for (int nextLetter = 0; nextLetter < 26; nextLetter++)
            {
                // Look at links from an to the start letter.
                int pos1 = links[startLetter][nextLetter];
                int pos2 = links[nextLetter][startLetter];

                if ((pos1 != -1) && (pos1 != stopPositiom))
                {
                    traverseSubgraph(links, level + 1, prefix, nextLetter, letterUsage, pos1);
                }
                else if ((pos2 != -1) && (pos2 != stopPositiom))
                {
                    traverseSubgraph(links, level + 1, prefix, nextLetter, letterUsage, pos2);
                }
            }
        }
    }
}