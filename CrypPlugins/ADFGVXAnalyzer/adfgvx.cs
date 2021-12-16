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
            resetTranspositionKey();
            resetSubstitutionKey();
        }

        public ADFGVX(string name, string transpositionKeyStr, string substitutionKeyStr)

            : this(name, transpositionKeyStr, substitutionKeyStr, false)
        { }

        public ADFGVX(string name, string transpositionKeyStr, string substitutionKeyStr, bool inverseSubstitution)

            : this(name, transpositionKeyStr.Length)
        {
            setTranspositionKey(transpositionKeyStr);
            if (inverseSubstitution)
            {
                setSubstitutionInverseKey(substitutionKeyStr);
            }
            else
            {
                setSubstitutionKey(substitutionKeyStr);
            }
        }
        public void randomTranspositionKey()
        {
            transpositionKey.randomPermutation();
            updateTranspositionInverseKey();
        }

        private void updateTranspositionInverseKey() { transpositionInverseKey.inverseOf(transpositionKey); }
        private void updateTranspositionKeyFromInverse() { transpositionKey.inverseOf(transpositionInverseKey); }
        private void updateSubstitutionInverseKey() { substitutionInverseKey.inverseOf(substitutionKey); }
        private void updateSubstitutionKeyFromInverse() { substitutionKey.inverseOf(substitutionInverseKey); }


        public void resetTranspositionKey()
        {
            transpositionKey.Identity();
            transpositionInverseKey.Identity();
        }

        public void setTranspositionKey(string transpositionKeyStr)
        {
            transpositionKey.copy(transpositionKeyStr);
            updateTranspositionInverseKey();
        }

        public string getTranspositionKey()
        {
            return transpositionKey.ToString();
        }

        public void setTranspositionKey(AlphabetVector transpositionKey)
        {
            this.transpositionKey.copy(transpositionKey);
            updateTranspositionInverseKey();
        }
        public void swapInTranspositionKey(int i, int j)
        {
            transpositionKey.Swap(i, j);
            updateTranspositionInverseKey();
        }

        public void setTranspositionInverseKey(AlphabetVector transpositionInverseKey)
        {
            this.transpositionInverseKey.copy(transpositionInverseKey);
            updateTranspositionKeyFromInverse();
        }
        public void setTranspositionInverseKey(string transpositionInverseKeyStr)
        {
            transpositionInverseKey.copy(transpositionInverseKeyStr);
            updateTranspositionKeyFromInverse();
        }
        public void swapInTranspositionInverseKey(int i, int j)
        {
            transpositionInverseKey.Swap(i, j);
            updateTranspositionKeyFromInverse();
        }

        public void randomSubstitutionKey()
        {
            substitutionKey.randomPermutation();
            updateSubstitutionInverseKey();
        }

        public void resetSubstitutionKey()
        {
            substitutionKey.Identity();
            substitutionInverseKey.Identity();
        }

        public void setSubstitutionKey(string substitutionKeyStr)
        {
            substitutionKey.copy(substitutionKeyStr);
            updateSubstitutionInverseKey();
        }
        public void setSubstitutionKey(Alphabet36Vector substitutionKey)
        {
            this.substitutionKey.copy(substitutionKey);
            updateSubstitutionInverseKey();
        }
        public void swapInSubstitutionKey(int i, int j)
        {
            substitutionKey.Swap(i, j);
            updateSubstitutionInverseKey();
        }

        public void setSubstitutionInverseKey(string substitutionInverseKeyStr)
        {
            substitutionInverseKey.copy(substitutionInverseKeyStr);
            updateSubstitutionKeyFromInverse();
        }
        public void setSubstitutionInverseKey(Alphabet36Vector substitutionInverseKey)
        {
            this.substitutionInverseKey.copy(substitutionInverseKey);
            updateSubstitutionKeyFromInverse();
        }
        public void swapInSubstitutionInverseKey(int i, int j)
        {
            substitutionInverseKey.Swap(i, j);
            updateSubstitutionKeyFromInverse();
        }

        public void decodeSubstitution(ADFGVXVector interim, Alphabet36Vector plain)
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

        public void decode(ADFGVXVector cipher, ADFGVXVector interim, Alphabet36Vector plain)
        {
            Transposition.decodeWithInverseKey(transpositionInverseKey, cipher, interim);
            decodeSubstitution(interim, plain);
        }
    }
}
