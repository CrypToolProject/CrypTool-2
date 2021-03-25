using System;
using System.Collections.Generic;
using System.Linq;

namespace CrypTool.PluginBase.Utils
{
    public class Alphabet
    {
        private char[] characters;
        private Dictionary<char, int> char2num;

        public Alphabet(string alphabet)
        {
            if (alphabet.Distinct().Count() != alphabet.Length)
                throw new Exception("Can't create alphabet from the given string as it contains duplicates.");

            this.characters = alphabet.ToCharArray();
            char2num = Enumerable.Range(0, characters.Length).ToDictionary(i => characters[i]);
        }

        public int[] StringToNumbers(string text, bool includeUnknowns = false)
        {
            if(includeUnknowns)
                return text.Select(c => char2num.ContainsKey(c) ? char2num[c] : -1).ToArray();
            return text.Select(c => char2num[c]).ToArray();
        }

        public string NumbersToString(int[] numbers)
        {
            return new string(numbers.Select(n => characters[n]).ToArray());
        }

        public char this[int i]
        {
            get { return characters[i]; }
        }

        public static implicit operator Alphabet(string s)
        {
            return new Alphabet(s);
        }

        public int Length
        {
            get { return this.characters.Length; }
        }
    }
}