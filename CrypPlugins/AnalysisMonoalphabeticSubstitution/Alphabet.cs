using System;
using System.Collections.Generic;
using System.Collections;

namespace CrypTool.AnalysisMonoalphabeticSubstitution
{
    /// <summary>
    /// Representation of an alphabet
    /// </summary>
    class Alphabet : IEnumerable
    {
        #region Private Variables

        //private Dictionary<int, string> alphabet = new Dictionary<int, string>();
        private string[] alphabet;
        private Dictionary<string, int> re_alphabet = new Dictionary<string, int>();
        //private int identifier;

        #endregion

        #region Properties

        //public int Identifier
        //{
        //    get { return this.identifier; }
        //    set { this.identifier = value; }
        //}

        public int Length
        {
            get { return this.alphabet.Length; }
        }

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
                var c = alphabet.Substring(i, length);
                if (!this.re_alphabet.ContainsKey(c))
                {
                    this.re_alphabet.Add(c, alpha.Count);
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

            foreach (var c in alphabet)
                if (!this.re_alphabet.ContainsKey(c))
                {
                    this.re_alphabet.Add(c, alpha.Count);
                    alpha.Add(c);
                }

            this.alphabet = alpha.ToArray();
        }

        /// <summary>
        /// Split string after separator 
        /// </summary>
        public Alphabet(string alphabet, char separator) : this(alphabet.Split(separator)) {}

        /// <summary>
        /// Use list with strings 
        /// </summary>
        public Alphabet(List<string> alphabet) : this(alphabet.ToArray()) {}

        /// <summary>
        /// Create empty alphabet 
        /// </summary>
        public Alphabet() {}

        #endregion

        #region Methods

        /// <summary>
        /// Get position of letter
        /// </summary>
        public int GetPositionOfLetter(string letter)
        {
            if (!this.re_alphabet.ContainsKey(letter))
                return -1;

            return this.re_alphabet[letter];
        }

        /// <summary>
        /// Get letter from position
        /// </summary>
        public string GetLetterFromPosition(int position)
        {
            if (position < 0 || position >= alphabet.Length)
                return null;

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
            var oldletter = GetLetterFromPosition(position);
            if (oldletter == null) return false;
            if (this.re_alphabet.ContainsKey(letter)) return false;

            this.re_alphabet.Remove(oldletter);
            this.re_alphabet.Add(letter, position);
            this.alphabet[position] = letter;

            return true;
        }

        /// <summary>
        /// Swap positions of two letters by reference to positions
        /// </summary>
        public bool SwapLettersAt(int pos1, int pos2)
        {
            var letter1 = GetLetterFromPosition(pos1);
            if (letter1 == null) return false;

            var letter2 = GetLetterFromPosition(pos2);
            if (letter2 == null) return false;

            this.alphabet[pos1] = letter2;
            this.alphabet[pos2] = letter1;

            return true;
        }

        /// <summary>
        /// Get number of letters that start with symbols
        /// </summary>
        public int GetNumberOfLettersStartingWith(string symbols)
        {
            int count = 0;

            for (int i = 0; i < this.alphabet.Length; i++)
                if (this.alphabet[i].IndexOf(symbols)==0)
                    count++;

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
            return this.alphabet.Length;
        }

        #endregion Methods

        #region Enumerator

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public AlphabetEnum GetEnumerator()
        {
            return new AlphabetEnum(this.alphabet);
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
    class AlphabetEnum : IEnumerator<string>
    {
        private string[] alphabet;
        private int cur_position = -1;
        private string cur_letter = null;

        public AlphabetEnum(string[] alpha)
        {
            this.alphabet = alpha;
            this.cur_letter = default(string);
        }

        public bool MoveNext()
        {
            this.cur_position++;

            if (this.cur_position >= this.alphabet.Length)
                return false;

            this.cur_letter = this.alphabet[this.cur_position];
            return true;
        }

        public void Reset()
        {
            this.cur_position = -1;
            this.cur_letter = default(string);
        }

        public string Current
        {
            get
            {
                return this.cur_letter;
            }
        }
      
        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        void IDisposable.Dispose()
        {

        }
    }
}
