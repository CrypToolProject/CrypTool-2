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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CrypTool.PluginBase.Miscellaneous
{
    public class StringUtil
    {
        public static string StripUnknownSymbols(string alphabet, string input)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in input)
            {
                if (alphabet.Contains(c))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }

    public class KeyValueReader : Dictionary<string, string>
    {
        public KeyValueReader(FileInfo file)
        {
            ReadLines(file.OpenText());
        }

        private void ReadLines(StreamReader sr)
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                // ignore comment lines
                if (line.StartsWith("#"))
                {
                    continue;
                }

                string[] tokens = line.Split(new char[] { '=' }, 2);
                if (tokens.Length < 1 || tokens[0] == null)
                {
                    continue;
                }

                string key = tokens[0].Trim();
                string value = tokens.Length < 2 ? null : tokens[1];

                if (value != null)
                {
                    value = value.Trim();
                }

                if (!ContainsKey(key))
                {
                    Add(key, value);
                }
            }
        }
    }

    public class WordTokenizer : IEnumerable<string>
    {
        /// <summary>
        /// Tokenize an input string to words.
        /// </summary>
        /// <param name="input">some input string containing human-readable text</param>
        /// <returns></returns>
        public static WordTokenizer tokenize(string input)
        {
            return new WordTokenizer(input);
        }

        /// <summary>
        /// Tokenize an an input file to words.
        /// </summary>
        /// <param name="file">some input file containing human-readable text</param>
        /// <returns></returns>
        public static IEnumerable<string> tokenize(FileInfo file)
        {
            StreamReader sr = file.OpenText();
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                foreach (string token in new WordTokenizer(line))
                {
                    yield return token;
                }
            }
        }

        private readonly string input;

        private WordTokenizer(string input)
        {
            this.input = input;
        }

        /// <summary>
        /// Returns enumerator with access to some special methods.
        /// </summary>
        /// <returns></returns>
        public WordEnumerator GetCustomEnumerator()
        {
            return new WordEnumerator(input);
        }

        #region IEnumerable<string> Members

        public IEnumerator<string> GetEnumerator()
        {
            return GetCustomEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetCustomEnumerator();
        }

        #endregion
    }

    public class WordEnumerator : IEnumerator<string>
    {
        private readonly string input;
        private int offset = -1;
        private string token = null;

        public WordEnumerator(string input)
        {
            this.input = input;
        }

        #region IEnumerator<string> Members

        /// <summary>
        /// According to IEnumerator contract this property throws an exception if Current is not pointing on a valid element.
        /// </summary>
        public string Current
        {
            get
            {
                if (token == null)
                {
                    throw new InvalidOperationException("Enumerator does not point on a valid token");
                };

                return token;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        #region IEnumerator Members

        // explicit implementation of non-generic interface
        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            StringBuilder sb = new StringBuilder();

            bool foundWord = false;
            bool feeding = true;

            do
            {
                if (++offset >= input.Length) // end of string
                {
                    if (foundWord)
                    {
                        break; // stop loop gracefully
                    }
                    else
                    {
                        token = null;
                        return false; // abort
                    }
                }

                switch (input[offset])
                {
                    case '\r':
                    case '\n':
                    case ' ':
                    case '\t':
                        if (foundWord) // found delimiter at the end of a word
                        {
                            feeding = false;
                        }

                        break;
                    default: // got letter
                        foundWord = true;
                        sb.Append(input[offset]);
                        break;
                }
            } while (feeding);

            token = sb.ToString();
            return true;
        }

        public void Reset()
        {
            offset = -1;
            token = null;
        }

        #endregion

        #region Additional properties

        /// <summary>
        /// Returns current position in processing input string.
        /// </summary>
        public int Position => Math.Max(offset, 0);

        /// <summary>
        /// Returns length of input string.
        /// </summary>
        public int Length => input.Length;

        #endregion
    }

    public class GramTokenizer : IEnumerable<string>
    {
        public static IEnumerable<string> tokenize(string word, int stepsize = 1)
        {
            return new GramTokenizer(word, 1, false, stepsize);
        }

        public static IEnumerable<string> tokenize(string word, int gramLength, int stepsize = 1)
        {
            return new GramTokenizer(word, gramLength, false, stepsize);
        }

        public static IEnumerable<string> tokenize(string word, int gramLength, bool includeFragments, int stepsize = 1)
        {
            return new GramTokenizer(word, gramLength, includeFragments, stepsize);
        }

        private readonly string word;
        private readonly int gramLength;
        private readonly bool includeFragments;
        private readonly int stepsize = 1;

        private GramTokenizer(string word, int gramLength, bool includeFragments, int stepsize = 1)
        {
            if (word == null || word.Length < 1)
            {
                throw new ArgumentException("word length must be > 0");
            }
            if (gramLength < 1)
            {
                throw new ArgumentOutOfRangeException("gram length must be > 0");
            }
            if (stepsize < 1)
            {
                throw new ArgumentOutOfRangeException("stepsize must be > 0");
            }

            if (includeFragments)
            {
                string underline = new string('_', gramLength - 1);
                this.word = underline + word + underline;
            }
            else
            {
                this.word = word;
            }

            this.gramLength = gramLength;
            this.includeFragments = includeFragments;
            this.stepsize = stepsize;
        }

        /// <summary>
        /// Tokenizes the whole input and returns a dictionary with all found grams and their quantity
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, int> ToDictionary()
        {
            IDictionary<string, int> dict = new Dictionary<string, int>();

            foreach (string gram in this)
            {
                if (dict.ContainsKey(gram))
                {
                    dict[gram]++;
                }
                else
                {
                    dict[gram] = 1;
                }
            }

            return dict;
        }

        #region IEnumerable<string> Members

        public IEnumerator<string> GetEnumerator()
        {
            return new GramEnumerator(word, gramLength, stepsize);
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new GramEnumerator(word, gramLength, stepsize);
        }

        #endregion
    }

    /// <summary>
    /// A tokenizer that splits a string into text blocks based on a set of delimiters.
    /// </summary>
    public class SymbolTokenizer : IEnumerable<string>
    {
        public static IEnumerable<string> tokenize(string input, string delimiters)
        {
            return new SymbolTokenizer(input, delimiters);
        }

        private readonly string input;
        private readonly string delimiters;

        private SymbolTokenizer(string input, string delimiters)
        {
            this.input = input;
            this.delimiters = delimiters;
        }

        #region IEnumerable<string> Members

        public IEnumerator<string> GetEnumerator()
        {
            return new SymbolEnumerator(input, delimiters);
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SymbolEnumerator(input, delimiters);
        }

        #endregion
    }

    /// <summary>
    /// An enumerator that delivers text blocks based on a set of delimiters.
    /// </summary>
    public class SymbolEnumerator : IEnumerator<string>
    {
        private readonly string input;
        private readonly string delimiters;

        private int offset = -1;
        private string token = null;

        public SymbolEnumerator(string input, string delimiters)
        {
            this.input = input;
            this.delimiters = delimiters;
        }

        #region IEnumerator<string> Members

        /// <summary>
        /// According to IEnumerator contract this property throws an exception if Current is not pointing on a valid element.
        /// </summary>
        public string Current
        {
            get
            {
                if (token == null)
                {
                    throw new InvalidOperationException("Enumerator does not point on a valid token");
                };

                return token;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        #region IEnumerator Members

        // explicit implementation of non-generic interface
        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            StringBuilder sb = new StringBuilder();

            bool foundWord = false;
            bool feeding = true;

            do
            {
                if (++offset >= input.Length) // end of string
                {
                    if (foundWord)
                    {
                        break; // stop loop gracefully
                    }
                    else
                    {
                        token = null;
                        return false; // abort
                    }
                }

                if (delimiters.Contains(input[offset]))
                {
                    if (foundWord) // found delimiter at the end of a word
                    {
                        feeding = false;
                    }
                }
                else // got letter
                {
                    foundWord = true;
                    sb.Append(input[offset]);
                }
            } while (feeding);

            token = sb.ToString();
            return true;
        }

        public void Reset()
        {
            offset = -1;
            token = null;
        }

        #endregion

        #region Additional properties

        ///<summary>
        /// Returns current position in processing input string.
        /// </summary>
        public int Position => Math.Max(offset, 0);

        ////<summary>
        /// Returns length of input string.
        /// </summary>
        public int Length => input.Length;

        #endregion
    }

    public class GramEnumerator : IEnumerator<string>
    {
        private readonly string word;
        private readonly int gramLength;
        private readonly int stepsize = 1;

        private int position = -1;

        public GramEnumerator(string word, int gramLength, int stepsize = 1)
        {
            this.word = word;
            this.gramLength = gramLength;
            this.stepsize = stepsize;
        }

        #region IEnumerator<string> Members

        public string Current => word.Substring(position, gramLength);

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            //word = null; // readonly
        }

        #endregion

        #region IEnumerator Members

        object System.Collections.IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (position == -1)
            {
                position = 0;
                return position < (word.Length - gramLength + 1);
            }
            return (position = position + stepsize) < (word.Length - gramLength + 1);
        }

        public void Reset()
        {
            position = 0;
        }

        #endregion
    }

}
