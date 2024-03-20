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
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrypTool.AnalysisMonoalphabeticSubstitution.Utils
{
    public class Alphabet
    {
        private readonly char[] characters;
        private readonly Dictionary<char, int> char2num;

        public Alphabet(string alphabet)
        {
            if (alphabet.Distinct().Count() != alphabet.Length)
            {
                throw new Exception("Can't create alphabet from the given string as it contains duplicates.");
            }

            characters = alphabet.ToCharArray();
            char2num = Enumerable.Range(0, characters.Length).ToDictionary(i => characters[i]);
        }

        public int[] StringToNumbers(string text, bool includeUnknowns = false)
        {
            if (includeUnknowns)
            {
                return text.Select(c => char2num.ContainsKey(c) ? char2num[c] : -1).ToArray();
            }

            return text.Select(c => char2num[c]).ToArray();
        }

        public string NumbersToString(int[] numbers)
        {
            return new string(numbers.Select(n => characters[n]).ToArray());
        }

        public char this[int i] => characters[i];

        public static implicit operator Alphabet(string s)
        {
            return new Alphabet(s);
        }

        public int Length => characters.Length;
    }
}