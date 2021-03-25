using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace CrypTool.Plugins.HomophonicAnalyzer
{
    /// <summary>
    /// Representation of an alphabet
    /// </summary>
    class Alphabet : IEnumerable
    {
        #region Private Variables

        private Dictionary<int,string> alphabet = new Dictionary<int,string>();
        private Dictionary<string, int> re_alphabet = new Dictionary<string, int>();
        private int identifier;

        #endregion

        #region Properties

        public int Identifier
        {
            get { return this.identifier; }
            set { this.identifier = value; }
        }

        public int Length
        {
            get { return this.alphabet.Count; }
            private set { ; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Regularly split string after length characters 
        /// </summary>
        public Alphabet(string alphabet,int length, int id)
        {
            int index = 0;
            string cur= "";
            this.identifier = id;

            for (int i = 0; i < alphabet.Length; i = i + length)
            {
                try
                {
                    cur = alphabet.Substring(i, length);
                }
                catch
                {
                    Console.WriteLine("4");
                }
                if (!this.re_alphabet.ContainsKey(cur))
                {
                    this.alphabet.Add(index, cur);
                    this.re_alphabet.Add(cur,index);
                    index++;
                }
            }
        }

        /// <summary>
        /// Split string after separator 
        /// </summary>
        public Alphabet(string alphabet, char separator = ';')
        {
            string[] let = alphabet.Split(separator);
            int index = 0;

            for (int i = 0; i < let.Length; i++)
            {
                if (!this.re_alphabet.ContainsKey(let[i]))
                {
                    this.alphabet.Add(index, let[i]);
                    this.re_alphabet.Add(let[i], index);
                    index++;
                }
            }
        }

        /// <summary>
        /// Use list with strings 
        /// </summary>
        public Alphabet(List<string> alphabet)
        {
            int index = 0;

            for (int i =0 ;i<alphabet.Count;i++)
            {
                if (!this.re_alphabet.ContainsKey(alphabet[i]))
                {
                    this.alphabet.Add(index, alphabet[i]);
                    this.re_alphabet.Add(alphabet[i], index);
                    index++;
                }
            }
        }

        /// <summary>
        /// Use string array
        /// </summary>
        public Alphabet(string[] alphabet)
        {
            int index = 0;

            for (int i = 0; i < alphabet.Length; i++)
            {
                if (!this.re_alphabet.ContainsKey(alphabet[i]))
                {
                    this.alphabet.Add(index, alphabet[i]);
                    this.re_alphabet.Add(alphabet[i], index);
                    index++;
                }
            }
        }

        /// <summary>
        /// Create empty alphabet 
        /// </summary>
        public Alphabet()
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// Get position of letter
        /// </summary>
        public int GetPositionOfLetter(string letter)
        {
            return this.re_alphabet.ContainsKey(letter) ? this.re_alphabet[letter] : -1;
        }

        /// <summary>
        /// Get letter from position
        /// </summary>
        public string GetLetterFromPosition(int position)
        {
            return this.alphabet.ContainsKey(position) ? this.alphabet[position] : null;
        }

        /// <summary>
        /// Get letters from positions 
        /// </summary>
        public string[] GetLettersFromPositions(int[] positions)
        {
            string[] res = new string[positions.Length];

            for (int i = 0; i < positions.Length; i++)
            {
                if (this.alphabet.ContainsKey(positions[i]))
                {
                    res[i] = this.alphabet[positions[i]];
                } else
                {
                    res[i] = null;
                }
            }
            return res;
        }

        /// <summary>
        /// Change letter in alphabet
        /// </summary>
        public bool ChangeLetterAt(int position, string letter)
        {
            if ((!this.alphabet.ContainsKey(position)) || (this.re_alphabet.ContainsKey(letter)))
            {
                return false;
            }
            else
            {
                this.re_alphabet.Remove(this.alphabet[position]);
                this.alphabet[position] = letter;
                this.re_alphabet.Add(letter, position);

                return true;
            }
        }

        /// <summary>
        /// Swap positions of two letters by reference to positions
        /// </summary>
        public bool SwapLettersAt(int pos1, int pos2)
        {
            if (this.alphabet.ContainsKey(pos1) && this.alphabet.ContainsKey(pos2))
            {
                string helper = this.alphabet[pos1];
                this.alphabet[pos1] = this.alphabet[pos2];
                this.re_alphabet[this.alphabet[pos1]] = pos2;
                this.alphabet[pos2] = helper;
                this.re_alphabet[this.alphabet[pos2]] = pos1;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Get number of letters that start with symbols
        /// </summary>
        public int GetNumberOfLettersStartingWith(string symbols)
        {
            int nr = 0;
            
            for (int i=0;i<this.alphabet.Count;i++)
            {
                if (this.alphabet[i].IndexOf(symbols)==0)
                {
                    nr++;
                }
            }
            return nr;
        }

        /// <summary>
        /// Check if letters are prefix-free .. to be implemented
        /// </summary>
        public bool CheckPrefixOfLetters()
        {

            return true;
        }

        /// <summary>
        /// Add letter to alphabet
        /// </summary>
        public bool AddLetter(string letter)
        {
            if (!this.re_alphabet.ContainsKey(letter))
            {
                int index = this.alphabet.Count;
                this.alphabet.Add(index, letter);
                this.re_alphabet.Add(letter, index);

                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Remove letter from alphabet
        /// </summary>
        public bool RemoveLetter(string letter)
        {
            if (this.re_alphabet.ContainsKey(letter))
            {
                this.alphabet.Remove(this.re_alphabet[letter]);
                this.re_alphabet.Remove(letter);
                this.RebuildIndex();

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Remove letter from alphabet
        /// </summary>
        public bool RemoveLetter(int position)
        {
            if (this.alphabet.ContainsKey(position))
            {
                this.re_alphabet.Remove(this.alphabet[position]);
                this.alphabet.Remove(position);
                this.RebuildIndex();

                return true;
            }
            else 
            {
                return false;
            }

        }

        /// <summary>
        /// Copy alphabet 
        /// </summary>
        public Alphabet CopyTo()
        {
            Alphabet res = new Alphabet();

            foreach (KeyValuePair<int, string> pair in this.alphabet)
            {
                res.AddLetter(pair.Value);
            }

            return res;
        }

        /// <summary>
        /// Get number of letters
        /// </summary>
        public int GetAlphabetQuantity()
        {
            return this.alphabet.Count;
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
        private void RebuildIndex()
        {
            int index = 0;
            this.alphabet = new Dictionary<int,string>();
            foreach (KeyValuePair<string, int> pair in this.re_alphabet)
            {
                this.re_alphabet[pair.Key] = index;
                this.alphabet.Add(index,pair.Key);
                index++;
            }
        }

        #endregion
    }

    /// <summary>
    /// Alphabet enumerator
    /// </summary>
    class AlphabetEnum : IEnumerator<string>
    {
        private Dictionary<int, string> alphabet;
        private int cur_position = -1;
        private string cur_letter = null;

        public AlphabetEnum(Dictionary<int,string> alpha)
        {
            this.alphabet = alpha;
            this.cur_letter = default(string);
        }

        public bool MoveNext()
        {
            this.cur_position++;

            if (this.cur_position >= this.alphabet.Count)
            {
                return false;
            } else
            {
                this.cur_letter = this.alphabet[this.cur_position];
                return true;
            }
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
