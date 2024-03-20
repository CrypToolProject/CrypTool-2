/*
   Copyright 2024 CrypTool 2 Team <ct2contact@CrypTool.org>

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
using CrypTool.PluginBase.IO;
using LanguageStatisticsLib;
using System.Collections.Generic;

namespace CrypTool.AnalysisMonoalphabeticSubstitution
{
    internal class Dictionary
    {
        private WordTree wordTree;

        /// <summary>
        /// Creates a new dictionary for the given language.
        /// </summary>
        /// <param name="language"></param>
        public Dictionary(int language)
        {
            wordTree = LanguageStatistics.LoadWordTree(LanguageStatistics.LanguageCode(language), DirectoryHelper.DirectoryLanguageStatistics);
        }

        /// <summary>
        /// Returns a list of words that match the given pattern.
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public List<byte[]> GetWordsFromPattern(byte[] pattern)
        {
            //get words of length pattern.Length
            List<string> wordsOfLength = wordTree.ToList(pattern.Length);

            //create list of byte[] from wordsOfLength
            List<byte[]> words = new List<byte[]>();
            foreach (string word in wordsOfLength)
            {
                char[] charArray = word.ToCharArray();
                byte[] byteArray = new byte[charArray.Length];
                for (int i = 0; i < charArray.Length; i++)
                {
                    //here, we assume that the input is in upper case
                    if (charArray[i] < 65 || charArray[i] > 90)
                    {
                        //if not a letter, set to 0 = first letter of the alphabet
                        byteArray[i] = 0;
                        continue;
                    }
                    byteArray[i] = (byte)(charArray[i] - 'A'); // hack: subtract 65 to let A=0, B=1, ...
                }
                words.Add(byteArray);
            }
            
            //remove all words from words list that do not match the pattern
            for (int i = words.Count - 1; i >= 0; i--)
            {
                if (!MatchPattern(words[i], pattern))
                {
                    words.RemoveAt(i);                
                }
                
            }

            return words;
        }

        /// <summary>
        /// This method checks if a given word matches a given pattern.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        private bool MatchPattern(byte[] word, byte[] pattern)
        {
            if(word.Length != pattern.Length)
            {
                return false;
            }
            
            Dictionary<byte, byte> mapping = new Dictionary<byte, byte>();

            for(int i=0;i<word.Length;i++)
            {
                if (mapping.ContainsKey(pattern[i]))
                {
                    if (mapping[pattern[i]] != word[i])
                    {
                        return false;
                    }
                }
                else
                {
                    mapping.Add(pattern[i], word[i]);
                }
            }            
            return true;
        }
    }
}
