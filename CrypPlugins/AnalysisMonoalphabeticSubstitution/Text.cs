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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrypTool.AnalysisMonoalphabeticSubstitution
{
    internal class Text
    {
        #region Private Variables

        private readonly List<int> text = new List<int>();
        private readonly List<string> notInAlphabet = new List<string>();
        private readonly List<bool> orgCapital = new List<bool>();
        private readonly int invalidCharProcess;
        private readonly bool caseSensitive = false;
        private readonly Alphabet alphabet;

        #endregion

        #region Constructor

        public Text(string text, Alphabet alpha, int treatmentInvalidChars, bool caseSensitive = false)
        {
            invalidCharProcess = treatmentInvalidChars;
            this.caseSensitive = caseSensitive;

            string curString = "";
            string c = "";

            string prep_text = text;

            if (!this.caseSensitive)
            {
                for (int i = 0; i < prep_text.Length; i++)
                {
                    bool status = false;

                    int j = 0;
                    do
                    {
                        j++;
                        curString = prep_text.Substring(i, j);

                        c = curString;
                        if (char.IsUpper(c.ToCharArray()[0]))
                        {
                            status = true;
                            c = c.ToLower();
                        }
                    }
                    while (alpha.GetNumberOfLettersStartingWith(c) > 1);

                    if (alpha.GetNumberOfLettersStartingWith(c) == 1)
                    {
                        this.text.Add(alpha.GetPositionOfLetter(c));
                        orgCapital.Add(status);
                    }
                    else if (alpha.GetNumberOfLettersStartingWith(c) == 0)
                    {
                        notInAlphabet.Add(curString);
                        this.text.Add(-notInAlphabet.Count);
                        orgCapital.Add(false);
                    }
                    i += j - 1;
                }
            }
            else
            {
                for (int i = 0; i < prep_text.Length; i++)
                {
                    int j = 0;
                    do
                    {
                        j++;
                        curString = prep_text.Substring(i, j);
                        c = curString;
                    }
                    while (alpha.GetNumberOfLettersStartingWith(c) > 1);

                    if (alpha.GetNumberOfLettersStartingWith(c) == 1)
                    {
                        this.text.Add(alpha.GetPositionOfLetter(c));
                        orgCapital.Add(false);
                    }
                    else if (alpha.GetNumberOfLettersStartingWith(c) == 0)
                    {
                        notInAlphabet.Add(curString);
                        this.text.Add(-notInAlphabet.Count);
                        orgCapital.Add(false);
                    }
                    i += j - 1;
                }
            }
        }

        public Text(int treatmentInvalidChars)
        {
            invalidCharProcess = treatmentInvalidChars;
        }

        #endregion

        #region Properties

        public int Length => text.Count;

        public int[] ValidLetterArray => text.Where(c => c >= 0).ToArray();

        #endregion

        #region Methods

        /// <summary>
        /// Convert text to string
        /// </summary>
        public override string ToString()
        {
            return ToString(alphabet);
        }

        public string ToString(Alphabet alpha)
        {
            StringBuilder sb = new StringBuilder();
            string s;

            for (int i = 0; i < text.Count; i++)
            {
                int letter = text[i];

                if (letter >= 0)
                {
                    s = (!caseSensitive || orgCapital[i])
                      ? alpha.GetLetterFromPosition(letter).ToUpper()
                      : alpha.GetLetterFromPosition(letter).ToLower();
                }
                else
                {
                    switch (invalidCharProcess)
                    {
                        case 1: s = " "; break;
                        case 2: s = "?"; break;
                        default: s = notInAlphabet[-letter - 1]; break;
                    }
                }

                sb.Append(s);
            }

            return sb.ToString();
        }

        public List<byte[]> ToSingleWords(Alphabet alpha)
        {
            int space = alpha.GetPositionOfLetter(" ");

            List<byte[]> result = new List<byte[]>();

            List<byte> word = new List<byte>();
            for (int i = 0; i < text.Count; i++)
            {
                if (text[i] != space)
                {
                    word.Add((byte)text[i]);
                }
                else
                {
                    if (word != null && word.Count != 0)
                    {
                        result.Add(word.ToArray());
                        word = new List<byte>();
                    }
                }
            }
            if (word != null && word.Count != 0)
            {
                result.Add(word.ToArray());
                word = new List<byte>();
            }

            return result;
        }

        /// <summary>
        /// Get letter at position
        /// </summary>
        public int GetLetterAt(int position)
        {
            if ((position < 0) || (position >= text.Count))
            {
                return -1;
            }

            return text[position];
        }

        /// <summary>
        /// Get letter that is not in alphabet at position
        /// </summary>
        public string GetLetterNotInAlphabetAt(int position)
        {
            if (position >= 0 || -position > notInAlphabet.Count)
            {
                return "";
            }

            return notInAlphabet[-position - 1];
        }

        /// <summary>
        /// Add letter to the end of the text
        /// </summary>
        public void AddLetter(string letter, Alphabet alpha)
        {
            int position;

            if (caseSensitive == false)
            {
                orgCapital.Add(char.IsUpper(letter.ToCharArray()[0]));
                position = alpha.GetPositionOfLetter(letter.ToLower());
            }
            else
            {
                position = alpha.GetPositionOfLetter(letter);
                orgCapital.Add(false);
            }

            if (position >= 0)
            {
                text.Add(position);
            }
            else
            {
                notInAlphabet.Add(letter);
                text.Add(-notInAlphabet.Count);
            }
        }

        private void AddLetter(int letter)
        {
            text.Add(letter);
        }

        /// <summary>
        /// Change letter at position
        /// </summary>
        public bool ChangeLetterAt(int position, int letter)
        {
            if ((position < 0) || (position >= text.Count) || (text[position] == -1))
            {
                return false;
            }

            text[position] = letter;
            return true;
        }

        /// <summary>
        /// Copy text
        /// </summary>
        public Text CopyTo()
        {
            Text res = new Text(invalidCharProcess);

            for (int i = 0; i < text.Count; i++)
            {
                res.AddLetter(text[i]);
            }

            for (int i = 0; i < notInAlphabet.Count; i++)
            {
                res.AddLetterNotInAlphabet(notInAlphabet[i]);
            }

            for (int i = 0; i < orgCapital.Count; i++)
            {
                res.AddOrgCapital(orgCapital[i]);
            }

            return res;
        }

        private void AddLetterNotInAlphabet(string letter)
        {
            notInAlphabet.Add(letter);
        }

        private void AddOrgCapital(bool val)
        {
            orgCapital.Add(val);
        }

        public int[] ToIntArray()
        {
            return text.ToArray();
        }

        #endregion
    }
}