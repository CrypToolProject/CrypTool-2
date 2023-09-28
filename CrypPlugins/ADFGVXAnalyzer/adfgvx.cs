/*
   Copyright 2018 Dominik Vogt <ct2contact@CrypTool.org>

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
using common;

namespace ADFGVXAnalyzer
{
    public class ADFGVX
    {
        public string name = "unknown";
        public AlphabetVector transpositionKey;
        private readonly AlphabetVector transpositionInverseKey;
        public Alphabet36Vector substitutionKey;
        public Alphabet36Vector substitutionInverseKey;

        public ADFGVX(string name, int transpositionKeyLength)
        {
            this.name = name;
            transpositionKey = new AlphabetVector(transpositionKeyLength, false);
            transpositionInverseKey = new AlphabetVector(transpositionKeyLength, false);
            substitutionKey = new Alphabet36Vector();
            substitutionInverseKey = new Alphabet36Vector();
            substitutionKey.acceptErrors = true;
            substitutionInverseKey.acceptErrors = true;
            ResetTranspositionKey();
            ResetSubstitutionKey();
        }

        public ADFGVX(string name, string transpositionKeyStr, string substitutionKeyStr)

            : this(name, transpositionKeyStr, substitutionKeyStr, false)
        { }

        public ADFGVX(string name, string transpositionKeyStr, string substitutionKeyStr, bool inverseSubstitution)

            : this(name, transpositionKeyStr.Length)
        {
            SetTranspositionKey(transpositionKeyStr);
            if (inverseSubstitution)
            {
                SetSubstitutionInverseKey(substitutionKeyStr);
            }
            else
            {
                SetSubstitutionKey(substitutionKeyStr);
            }
        }
        public void RandomTranspositionKey()
        {
            transpositionKey.randomPermutation();
            UpdateTranspositionInverseKey();
        }

        private void UpdateTranspositionInverseKey() { transpositionInverseKey.inverseOf(transpositionKey); }
        private void UpdateTranspositionKeyFromInverse() { transpositionKey.inverseOf(transpositionInverseKey); }
        private void UpdateSubstitutionInverseKey() { substitutionInverseKey.inverseOf(substitutionKey); }
        private void UpdateSubstitutionKeyFromInverse() { substitutionKey.inverseOf(substitutionInverseKey); }


        public void ResetTranspositionKey()
        {
            transpositionKey.Identity();
            transpositionInverseKey.Identity();
        }

        public void SetTranspositionKey(string transpositionKeyStr)
        {
            transpositionKey.copy(transpositionKeyStr);
            UpdateTranspositionInverseKey();
        }

        public string GetTranspositionKey()
        {
            return transpositionKey.ToString();
        }

        public void SetTranspositionKey(AlphabetVector transpositionKey)
        {
            this.transpositionKey.copy(transpositionKey);
            UpdateTranspositionInverseKey();
        }

        public void SwapInTranspositionKey(int i, int j)
        {
            transpositionKey.Swap(i, j);
            UpdateTranspositionInverseKey();
        }

        public void SetTranspositionInverseKey(AlphabetVector transpositionInverseKey)
        {
            this.transpositionInverseKey.copy(transpositionInverseKey);
            UpdateTranspositionKeyFromInverse();
        }

        public void SetTranspositionInverseKey(string transpositionInverseKeyStr)
        {
            transpositionInverseKey.copy(transpositionInverseKeyStr);
            UpdateTranspositionKeyFromInverse();
        }

        public void SwapInTranspositionInverseKey(int i, int j)
        {
            transpositionInverseKey.Swap(i, j);
            UpdateTranspositionKeyFromInverse();
        }

        public void RandomSubstitutionKey()
        {
            substitutionKey.randomPermutation();
            UpdateSubstitutionInverseKey();
        }

        public void ResetSubstitutionKey()
        {
            substitutionKey.Identity();
            substitutionInverseKey.Identity();
        }

        public void SetSubstitutionKey(string substitutionKeyStr)
        {
            substitutionKey.copy(substitutionKeyStr);
            UpdateSubstitutionInverseKey();
        }

        public void SetSubstitutionKey(Alphabet36Vector substitutionKey)
        {
            this.substitutionKey.copy(substitutionKey);
            UpdateSubstitutionInverseKey();
        }

        public void SwapInSubstitutionKey(int i, int j)
        {
            substitutionKey.Swap(i, j);
            UpdateSubstitutionInverseKey();
        }

        public void SetSubstitutionInverseKey(string substitutionInverseKeyStr)
        {
            substitutionInverseKey.copy(substitutionInverseKeyStr);
            UpdateSubstitutionKeyFromInverse();
        }

        public void SetSubstitutionInverseKey(Alphabet36Vector substitutionInverseKey)
        {
            this.substitutionInverseKey.copy(substitutionInverseKey);
            UpdateSubstitutionKeyFromInverse();
        }

        public void SwapInSubstitutionInverseKey(int i, int j)
        {
            substitutionInverseKey.Swap(i, j);
            UpdateSubstitutionKeyFromInverse();
        }

        public void DecryptSubstitution(ADFGVXVector interim, Alphabet36Vector plain)
        {
            if (interim.length % 2 != 0)
            {
                //hack, to allow uneven length adfgvx texts
                interim.append("A");
            }
            plain.length = 0;
            for (int i = 0; i < interim.length; i += 2)
            {
                int v1 = interim.TextInInt[i];
                int v2 = interim.TextInInt[i + 1];
                if (v1 == -1 || v2 == -1)
                {
                    plain.append(-1);
                }
                else
                {
                    plain.append(substitutionInverseKey.TextInInt[v1 * 6 + v2]);
                }
            }
        }

        public void Decrypt(ADFGVXVector cipher, ADFGVXVector interim, Alphabet36Vector plain)
        {
            Transposition.decodeWithInverseKey(transpositionInverseKey, cipher, interim);
            DecryptSubstitution(interim, plain);
        }
    }
}
