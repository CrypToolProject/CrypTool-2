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
using System.Collections;
using System.Collections.Generic;

namespace CrypTool.AnalysisMonoalphabeticSubstitution
{
    /// <summary>
    /// Representation of an alphabet
    /// </summary>
    internal class Alphabet : IEnumerable
    {
        #region Private Variables

        //private Dictionary<int, string> alphabet = new Dictionary<int, string>();
        private readonly string[] alphabet;
        private readonly Dictionary<string, int> re_alphabet = new Dictionary<string, int>();
        //private int identifier;

        #endregion

        #region Properties

        //public int Identifier
        //{
        //    get { return this.identifier; }
        //    set { this.identifier = value; }
        //}

        public int Length => alphabet.Length;

        #endregion

        #region Constructors

        /// <summary>
        /// Regularly split string after length characters 
        /// </summary>
        public Alphabet(string alphabet, int length = 1)
        {
            List<string> alpha = new List<string>();

            for (int i = 0; i + length <= alphabet.Length; i += length)
            {
                string c = alphabet.Substring(i, length);
                if (!re_alphabet.ContainsKey(c))
                {
                    re_alphabet.Add(c, alpha.Count);
                    alpha.Add(c);
                }
            }

            this.alphabet = alpha.ToArray();
        }

        /// <summary>
        /// Use string array
        /// </summary>
        public Alphabet(string[] alphabet)
        {
            List<string> alpha = new List<string>();

            foreach (string c in alphabet)
            {
                if (!re_alphabet.ContainsKey(c))
                {
                    re_alphabet.Add(c, alpha.Count);
                    alpha.Add(c);
                }
            }

            this.alphabet = alpha.ToArray();
        }

        /// <summary>
        /// Split string after separator 
        /// </summary>
        public Alphabet(string alphabet, char separator) : this(alphabet.Split(separator)) { }

        /// <summary>
        /// Use list with strings 
        /// </summary>
        public Alphabet(List<string> alphabet) : this(alphabet.ToArray()) { }

        /// <summary>
        /// Create empty alphabet 
        /// </summary>
        public Alphabet() { }

        #endregion

        #region Methods

        /// <summary>
        /// Get position of letter
        /// </summary>
        public int GetPositionOfLetter(string letter)
        {
            if (!re_alphabet.ContainsKey(letter))
            {
                return -1;
            }

            return re_alphabet[letter];
        }

        /// <summary>
        /// Get letter from position
        /// </summary>
        public string GetLetterFromPosition(int position)
        {
            if (position < 0 || position >= alphabet.Length)
            {
                return null;
            }

            return alphabet[position];
        }

        //public string this[int position]
        //{
        //    get
        //    {
        //        if (position < 0 || position >= alphabet.Length)
        //            return null;
        //        return alphabet[position];
        //    }
        //}

        /// <summary>
        /// Get letters from positions 
        /// </summary>
        //public string[] GetLettersFromPositions(int[] positions)
        //{
        //    string[] res = new string[positions.Length];

        //    for (int i = 0; i < positions.Length; i++)
        //        res[i] = GetLetterFromPosition(positions[i]);

        //    return res;
        //}

        /// <summary>
        /// Change letter in alphabet
        /// </summary>
        public bool ChangeLetterAt(int position, string letter)
        {
            string oldletter = GetLetterFromPosition(position);
            if (oldletter == null)
            {
                return false;
            }

            if (re_alphabet.ContainsKey(letter))
            {
                return false;
            }

            re_alphabet.Remove(oldletter);
            re_alphabet.Add(letter, position);
            alphabet[position] = letter;

            return true;
        }

        /// <summary>
        /// Swap positions of two letters by reference to positions
        /// </summary>
        public bool SwapLettersAt(int pos1, int pos2)
        {
            string letter1 = GetLetterFromPosition(pos1);
            if (letter1 == null)
            {
                return false;
            }

            string letter2 = GetLetterFromPosition(pos2);
            if (letter2 == null)
            {
                return false;
            }

            alphabet[pos1] = letter2;
            alphabet[pos2] = letter1;

            return true;
        }

        /// <summary>
        /// Get number of letters that start with symbols
        /// </summary>
        public int GetNumberOfLettersStartingWith(string symbols)
        {
            int count = 0;

            for (int i = 0; i < alphabet.Length; i++)
            {
                if (alphabet[i].IndexOf(symbols) == 0)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Check if letters are prefix-free .. to be implemented
        /// </summary>
        //public bool CheckPrefixOfLetters()
        //{
        //    return true;
        //}

        /// <summary>
        /// Add letter to alphabet
        /// </summary>
        //public bool AddLetter(string letter)
        //{
        //    if (this.re_alphabet.ContainsKey(letter))
        //        return false;

        //    int index = this.alphabet.Length;
        //    this.alphabet.Add(index, letter);
        //    this.re_alphabet.Add(letter, index);

        //    return true;
        //}

        /// <summary>
        /// Remove letter from alphabet
        /// </summary>
        //public bool RemoveLetter(string letter)
        //{
        //    if (this.re_alphabet.ContainsKey(letter))
        //    {
        //        this.alphabet.Remove(this.re_alphabet[letter]);
        //        this.re_alphabet.Remove(letter);
        //        this.RebuildIndex();

        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        /// <summary>
        /// Remove letter from alphabet
        /// </summary>
        //public bool RemoveLetter(int position)
        //{
        //    if (this.alphabet.ContainsKey(position))
        //    {
        //        this.re_alphabet.Remove(this.alphabet[position]);
        //        this.alphabet.Remove(position);
        //        this.RebuildIndex();

        //        return true;
        //    }
        //    else 
        //    {
        //        return false;
        //    }

        //}

        /// <summary>
        /// Copy alphabet 
        /// </summary>
        //public Alphabet CopyTo()
        //{
        //    Alphabet res = new Alphabet();

        //    foreach (KeyValuePair<int, string> pair in this.alphabet)
        //    {
        //        res.AddLetter(pair.Value);
        //    }

        //    return res;
        //}

        /// <summary>
        /// Get number of letters
        /// </summary>
        public int GetAlphabetQuantity()
        {
            return alphabet.Length;
        }

        #endregion Methods

        #region Enumerator

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public AlphabetEnum GetEnumerator()
        {
            return new AlphabetEnum(alphabet);
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Close numeration gaps, for instance if letters have been removed
        /// </summary>
        //private void RebuildIndex()
        //{
        //    int index = 0;
        //    this.alphabet = new string[];
        //    foreach (KeyValuePair<string, int> pair in this.re_alphabet)
        //    {
        //        this.re_alphabet[pair.Key] = index;
        //        this.alphabet.Add(index,pair.Key);
        //        index++;
        //    }
        //}

        #endregion
    }

    /// <summary>
    /// Alphabet enumerator
    /// </summary>
    internal class AlphabetEnum : IEnumerator<string>
    {
        private readonly string[] alphabet;
        private int cur_position = -1;
        private string cur_letter = null;

        public AlphabetEnum(string[] alpha)
        {
            alphabet = alpha;
            cur_letter = default(string);
        }

        public bool MoveNext()
        {
            cur_position++;

            if (cur_position >= alphabet.Length)
            {
                return false;
            }

            cur_letter = alphabet[cur_position];
            return true;
        }

        public void Reset()
        {
            cur_position = -1;
            cur_letter = default(string);
        }

        public string Current => cur_letter;

        object IEnumerator.Current => Current;

        void IDisposable.Dispose()
        {

        }
    }
}
