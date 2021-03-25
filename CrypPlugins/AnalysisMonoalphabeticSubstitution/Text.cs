using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrypTool.AnalysisMonoalphabeticSubstitution
{
    class Text
    {
        #region Private Variables

        private List<int> text = new List<int>();
        private List<string> notInAlphabet = new List<string>();
        private List<bool> orgCapital = new List<bool>();
        private int invalidCharProcess;
        private bool caseSensitive = false;
        private Alphabet alphabet;

        #endregion

        #region Constructor

        public Text(string text, Alphabet alpha, int treatmentInvalidChars, bool caseSensitive = false)
        {
            this.invalidCharProcess = treatmentInvalidChars;
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
                        this.orgCapital.Add(status);
                    }
                    else if (alpha.GetNumberOfLettersStartingWith(c) == 0)
                    {
                        this.notInAlphabet.Add(curString);
                        this.text.Add(-this.notInAlphabet.Count);
                        this.orgCapital.Add(false);
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
                        this.orgCapital.Add(false);
                    }
                    else if (alpha.GetNumberOfLettersStartingWith(c) == 0)
                    {
                        this.notInAlphabet.Add(curString);
                        this.text.Add(-this.notInAlphabet.Count);
                        this.orgCapital.Add(false);
                    }
                    i += j - 1;
                }
            }
        }

        public Text(int treatmentInvalidChars)
        {
            this.invalidCharProcess = treatmentInvalidChars;
        }

        #endregion

        #region Properties

        public int Length
        {
            get { return this.text.Count; }
        }

        public int[] ValidLetterArray 
        {
            get { return text.Where(c => c >= 0).ToArray(); }
        }

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
            String s;

            for (int i = 0; i < this.text.Count; i++)
            {
                int letter = this.text[i];

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
                        default: s = this.notInAlphabet[-letter - 1]; break;
                    }
                }

                sb.Append(s);
            }

            return sb.ToString();
        }

        public List<Byte[]> ToSingleWords(Alphabet alpha)
        {
            int space = alpha.GetPositionOfLetter(" ");

            List<Byte[]> result = new List<Byte[]>();

            List<Byte> word = new List<Byte>();
            for (int i = 0; i < this.text.Count; i++)
            {
                if (this.text[i] != space)
                {
                    word.Add((byte)this.text[i]);
                }
                else
                {
                    if (word != null && word.Count != 0)
                    {
                        result.Add(word.ToArray());
                        word = new List<Byte>();
                    }
                }
            }
            if (word != null && word.Count != 0)
            {
                result.Add(word.ToArray());
                word = new List<Byte>();
            }

            return result;
        }

        /// <summary>
        /// Get letter at position
        /// </summary>
        public int GetLetterAt(int position)
        {
            if ((position < 0) || (position >= this.text.Count)) return -1;
            return this.text[position];
        }

        /// <summary>
        /// Get letter that is not in alphabet at position
        /// </summary>
        public string GetLetterNotInAlphabetAt(int position)
        {
            if (position >= 0 || -position > this.notInAlphabet.Count) return "";
            return this.notInAlphabet[-position-1];
        }
        
        /// <summary>
        /// Add letter to the end of the text
        /// </summary>
        public void AddLetter(string letter, Alphabet alpha)
        {
            int position;

            if (this.caseSensitive == false)
            {
                this.orgCapital.Add(char.IsUpper(letter.ToCharArray()[0]));
                position = alpha.GetPositionOfLetter(letter.ToLower());
            }
            else
            {
                position = alpha.GetPositionOfLetter(letter);
                this.orgCapital.Add(false);
            }
 
            if (position >= 0)
            {
                this.text.Add(position);
            }
            else
            {
                this.notInAlphabet.Add(letter);
                this.text.Add(-this.notInAlphabet.Count);
            }
        }

        private void AddLetter(int letter)
        {
            this.text.Add(letter);
        }

        /// <summary>
        /// Change letter at position
        /// </summary>
        public bool ChangeLetterAt(int position, int letter)
        {
            if ((position < 0) || (position >= this.text.Count) || (this.text[position] == -1))
                return false;

            this.text[position] = letter;
            return true;
        }

        /// <summary>
        /// Copy text
        /// </summary>
        public Text CopyTo()
        {
            Text res = new Text(this.invalidCharProcess);

            for (int i = 0; i < this.text.Count; i++)
                res.AddLetter(this.text[i]);

            for (int i = 0; i < this.notInAlphabet.Count; i++)
                res.AddLetterNotInAlphabet(this.notInAlphabet[i]);

            for (int i = 0; i < this.orgCapital.Count; i++)
                res.AddOrgCapital(this.orgCapital[i]);
            
            return res;
        }

        private void AddLetterNotInAlphabet(string letter)
        {
            this.notInAlphabet.Add(letter);
        }

        private void AddOrgCapital(bool val)
        {
            this.orgCapital.Add(val);
        }

        public int[] ToIntArray()
        {
            return this.text.ToArray();
        }

        #endregion
    }
}