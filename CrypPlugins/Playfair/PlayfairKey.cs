using System;
using System.Linq;

namespace CrypTool.Playfair
{
    public static class PlayfairKey
    {
        public const string SmallAlphabet = "ABCDEFGHIKLMNOPQRSTUVWXYZ";
        public const string LargeAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public enum MatrixSize { Five_Five = 0, Six_Six = 1 }

        public static string CreateKey(string keyPhrase, MatrixSize matrixSize, bool ignoreDuplicates)
        {
            string alphabet = GetAlphabet(matrixSize);

            keyPhrase = string.Join("", keyPhrase.Where(c => alphabet.Contains(c)));  //Filter all chars which are in the alphabet

            if (!ignoreDuplicates)
            {
                keyPhrase = DeduplicateKeyPhrase(keyPhrase, alphabet);
            }
            return RemoveEqualChars(keyPhrase + alphabet);
        }

        private static string GetAlphabet(MatrixSize matrixSize)
        {
            switch (matrixSize)
            {
                case MatrixSize.Five_Five:
                    return SmallAlphabet;
                case MatrixSize.Six_Six:
                    return LargeAlphabet;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string DeduplicateKeyPhrase(string keyPhrase, string alphabet)
        {
            string result = string.Empty;
            foreach (char phraseChar in keyPhrase)
            {
                char nextChar = phraseChar;
                if (result.Contains(phraseChar))
                {
                    char? nextFreeChar = alphabet
                        .SkipWhile(c => c != phraseChar)
                        .Select(c => (char?)c)
                        .FirstOrDefault(c => !keyPhrase.Contains(c.Value) && !result.Contains(c.Value));
                    if (nextFreeChar.HasValue)
                    {
                        nextChar = nextFreeChar.Value;
                    }
                }
                result += nextChar;
            }
            return result;
        }

        private static string RemoveEqualChars(string value)
        {
            int length = value.Length;

            for (int i = 0; i < length; i++)
            {
                for (int j = i + 1; j < length; j++)
                {
                    if (value[i] == value[j])
                    {
                        value = value.Remove(j, 1);
                        j--;
                        length--;
                    }
                }
            }
            return value;
        }
    }
}
