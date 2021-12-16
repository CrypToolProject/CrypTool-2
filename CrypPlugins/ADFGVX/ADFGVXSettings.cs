/*
   Copyright 2011 CrypTool 2 Team <ct2contact@CrypTool.org>

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


using CrypTool.PluginBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CrypTool.ADFGVX
{
    public class ADFGVXSettings : ISettings
    {
        #region Public ADFGVX specific interface

        private const string ALPHABET25 = "ABCDEFGHIKLMNOPQRSTUVWXYZ";
        private const string ALPHABET36 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const string ALPHABET49 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 .,:;?()-+><=";

        private const string CIPHER_ALPHABET5 = "ADFGX";
        private const string CIPHER_ALPHABET6 = "ADFGVX";
        private const string CIPHER_ALPHABET7 = "ADFGVXZ";

        private CipherTypeEnum cipherType = CipherTypeEnum.ADFGVX;
        private ActionEnum selectedAction = ActionEnum.Encrypt;
        private string substitutionPass = string.Empty;
        private string substitutionMatrix = string.Empty;
        private string transpositionPass = string.Empty;
        private string cleanTranspositionPass = string.Empty;

        public string Alphabet
        {
            get; set;
        }

        public string CipherAlphabet
        {
            get; set;
        }

        public ADFGVXSettings()
        {
            updateAlphabet();

            SubstitutionPass = "SUBSTITUTION";
            TranspositionPass = "TRANSPOSITION";
        }

        public enum ActionEnum
        {
            Encrypt, Decrypt
        }

        public enum CipherTypeEnum
        {
            ADFGX, ADFGVX, ADFGVXZ
        }

        #endregion

        #region private methods

        private readonly Random random = new Random();

        // with given alphabet
        private string createRandomPassword()
        {
            StringBuilder newPassword = new StringBuilder();
            StringBuilder remainingChars = new StringBuilder(Alphabet);

            while (remainingChars.Length > 0)
            {
                int pos = random.Next(remainingChars.Length);
                newPassword.Append(remainingChars[pos]);
                remainingChars.Remove(pos, 1);
            }

            return newPassword.ToString();
        }

        private void rebuildSubstitutionMatrix()
        {
            string value = SubstitutionPass.ToUpperInvariant();

            StringBuilder sb = new StringBuilder();
            HashSet<char> seen = new HashSet<char>();

            foreach (char c in value)
            {
                // add character to matrix if unique and part of alphabet
                if (!seen.Contains(c) && Alphabet.Contains(c))
                {
                    sb.Append(c);
                    seen.Add(c);
                }
            }

            // fill matrix with remaining characters
            foreach (char c in Alphabet)
            {
                if (!seen.Contains(c))
                {
                    sb.Append(c);
                }
            }

            SubstitutionMatrix = sb.ToString();
            Debug.Assert(sb.Length == Alphabet.Length, "Matrix length != Alphabet length");
        }

        private void RebuildTranspositionCleanPassword()
        {
            string value = TranspositionPass.ToUpperInvariant();
            //if no transposition password was given, we use a default password of A, meaning, we have no transposition
            if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
            {
                value = "A";
            }

            // remove characters not part of alphabet
            List<char> cleanPassword = new List<char>();
            foreach (char c in value)
            {
                if (Alphabet.Contains(c))
                {
                    cleanPassword.Add(c);
                }
            }

            // copy and sort characters
            char[] keyChars = cleanPassword.ToArray();
            Array.Sort(keyChars);

            // determine column order
            int[] newColumnOrder = new int[keyChars.Length];
            for (int i = 0; i < keyChars.Length; i++)
            {
                int column = Array.IndexOf(keyChars, cleanPassword[i]);
                newColumnOrder[i] = column;
                keyChars[column] = (char)0; // make sure the same character won't be found again
            }
            KeyColumnOrder = newColumnOrder;

            // build nice looking string for output (note: column numbers start with 0 in array, but 1 in string)
            StringBuilder keyWord = new StringBuilder();
            if (newColumnOrder.Length >= 1)
            {
                keyWord.Append((newColumnOrder[0] + 1));
                for (int i = 1; i < newColumnOrder.Length; i++)
                {
                    keyWord.Append("-" + (newColumnOrder[i] + 1));
                }
            }
            CleanTranspositionPass = keyWord.ToString();
        }

        private void updateAlphabet()
        {
            switch (cipherType)
            {
                case CipherTypeEnum.ADFGX:
                    Alphabet = ALPHABET25;
                    CipherAlphabet = CIPHER_ALPHABET5;
                    break;
                case CipherTypeEnum.ADFGVX:
                default:
                    Alphabet = ALPHABET36;
                    CipherAlphabet = CIPHER_ALPHABET6;
                    break;
                case CipherTypeEnum.ADFGVXZ:
                    Alphabet = ALPHABET49;
                    CipherAlphabet = CIPHER_ALPHABET7;
                    break;
            }

            rebuildSubstitutionMatrix();
        }

        #endregion

        #region Algorithm settings properties (visible in the Settings pane)

        [TaskPane("ActionCaption", "ActionTooltip", null, 1, false, ControlType.ComboBox, new string[] { "ActionList1", "ActionList2" })]
        public ActionEnum Action
        {
            get => selectedAction;
            set
            {
                if (value != selectedAction)
                {
                    selectedAction = value;
                    OnPropertyChanged("Action");
                }
            }
        }


        [TaskPane("CipherVariantCaption", "CipherVariantTooltip", null, 2, false, ControlType.ComboBox, new string[] { "CipherTypeList1", "CipherTypeList2", "CipherTypeList3" })]
        public CipherTypeEnum CipherType
        {
            get => cipherType;
            set
            {
                if (value != cipherType)
                {
                    cipherType = value;
                    OnPropertyChanged("CipherType");

                    updateAlphabet();
                }
            }
        }

        [TaskPane("SubstitutionPassCaption", "SubstitutionPassTooltip", null, 3, false, ControlType.TextBox)]
        public string SubstitutionPass
        {
            get => substitutionPass;
            set
            {
                if (value != substitutionPass)
                {
                    substitutionPass = value;
                    OnPropertyChanged("SubstitutionPass");

                    rebuildSubstitutionMatrix();
                }
            }
        }

        [TaskPane("SubstitutionMatrixCaption", "SubstitutionMatrixTooltip", null, 4, false, ControlType.TextBoxReadOnly)]
        public string SubstitutionMatrix
        {
            get => substitutionMatrix;
            set
            {
                if (value != substitutionMatrix)
                {
                    substitutionMatrix = value;
                    OnPropertyChanged("SubstitutionMatrix");
                }
            }
        }


        [TaskPane("TranspositionPassCaption", "TranspositionPassTooltip", null, 5, false, ControlType.TextBox)]
        public string TranspositionPass
        {
            get => transpositionPass;
            set
            {
                if (value != transpositionPass)
                {
                    transpositionPass = value;
                    OnPropertyChanged("TranspositionPass");
                    RebuildTranspositionCleanPassword();
                }
            }
        }

        [TaskPane("CleanTranspositionPassCaption", "CleanTranspositionPassTooltip", null, 6, false, ControlType.TextBoxReadOnly)]
        public string CleanTranspositionPass
        {
            get => cleanTranspositionPass;
            set
            {
                if (value != cleanTranspositionPass)
                {
                    cleanTranspositionPass = value;
                    OnPropertyChanged("CleanTranspositionPass");
                }
            }
        }

        [TaskPane("RandomMatrixCaption", "RandomMatrixTooltip", null, 7, false, ControlType.Button)]
        public void RandomKeyButton()
        {
            SubstitutionPass = createRandomPassword();
            TranspositionPass = createRandomPassword();
        }

        [TaskPane("StandardMatrixCaption", "StandardMatrixTooltip", null, 8, false, ControlType.Button)]
        public void ResetKeyButton()
        {
            SubstitutionPass = string.Empty;
            TranspositionPass = string.Empty;
        }

        // Not a user setting, but used by ADFGVX processing
        public int[] KeyColumnOrder
        {
            get;
            set;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void Initialize()
        {

        }

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
