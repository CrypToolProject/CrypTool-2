/*
   Copyright 2008 - 2022 CrypTool Team

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

namespace CrypTool.Core
{
    public static class FuzzySearch
    {
        private static int LevenshteinDistance(string src, string dest)
        {
            int[,] d = new int[src.Length + 1, dest.Length + 1];
            int i, j;
            char[] str1 = src.ToCharArray();
            char[] str2 = dest.ToCharArray();

            for (i = 0; i <= str1.Length; i++)
            {
                d[i, 0] = i;
            }
            for (j = 0; j <= str2.Length; j++)
            {
                d[0, j] = j;
            }
            for (i = 1; i <= str1.Length; i++)
            {
                for (j = 1; j <= str2.Length; j++)
                {
                    int cost = str1[i - 1] == str2[j - 1] ? 0 : 1;

                    d[i, j] =
                        Math.Min(
                            d[i - 1, j] + 1,					// Deletion
                            Math.Min(
                                d[i, j - 1] + 1,				// Insertion
                                d[i - 1, j - 1] + cost));		// Substitution

                    if ((i > 1) && (j > 1) && (str1[i - 1] == str2[j - 2]) && (str1[i - 2] == str2[j - 1]))
                    {
                        d[i, j] = Math.Min(d[i, j], d[i - 2, j - 2] + cost);
                    }
                }
            }

            return d[str1.Length, str2.Length];
        }

        public static List<string> Search(string word, List<string> wordList, double fuzzyness)
        {
            List<string> foundWords = new List<string>();

            foreach (string s in wordList)
            {
                // Calculate the Levenshtein-distance:
                int levenshteinDistance =
                    LevenshteinDistance(word, s);

                // Length of the longer string:
                int length = Math.Max(word.Length, s.Length);

                // Calculate the score:
                double score = 1.0 - (double)levenshteinDistance / length;

                // Match?
                if (score > fuzzyness)
                {
                    foundWords.Add(s);
                }
            }
            return foundWords;
        }
    }
}