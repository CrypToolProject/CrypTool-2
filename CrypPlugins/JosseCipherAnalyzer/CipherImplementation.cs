using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrypTool.JosseCipherAnalyzer
{
    public class CipherImplementation
    {
        private char[] Board;
        public string Alphabet { get; set; }
        private StringBuilder Sb { get; } = new StringBuilder();

        public void BuildDictionaries(IEnumerable<char> key)
        {
            string alphabet = Alphabet;
            List<char> keyChars = key.Where(x => !char.IsWhiteSpace(x)).Distinct().ToList();

            // Remove chars in key from alphabet and add key at the beginning
            alphabet = string.Concat(keyChars) + string.Concat(alphabet.Where(c => !keyChars.Contains(c)));
            int alphabetLength = alphabet.Length;
            int keyCharsLength = keyChars.Count == 0 ? 1 : keyChars.Count;
            Board = new char[alphabetLength];

            // Build internal representation
            int count = 0;
            for (int columns = 0; columns < keyCharsLength; columns++)
            {
                for (int rows = columns; rows < alphabetLength; rows += keyCharsLength)
                {
                    Board[count] = alphabet[rows];
                    count++;
                }
            }
        }

        public string Decipher(string cypherText)
        {
            Sb.Clear();
            for (int i = 0; i < cypherText.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        Sb.Append(Int2Char(Board.Length - Char2Int(cypherText[i])));
                        continue;
                    case 1:
                        Sb.Append(Int2Char((Char2Int(cypherText[i]) + Char2Int(cypherText[i - 1])) % Board.Length));
                        break;
                    default:
                        int charNum = Mod(Char2Int(cypherText[i]) - Char2Int(cypherText[i - 1]), Board.Length);
                        if (charNum == 0)
                        {
                            charNum = Board.Length;
                        }
                        Sb.Append(Int2Char(charNum));
                        break;
                }
            }

            return Sb.ToString();
        }

        private int Char2Int(char c)
        {
            return Array.IndexOf(Board, c) + 1;
        }

        private char Int2Char(int index)
        {
            return Board[index != 0 ? index - 1 : 0];
        }

        private static int Mod(int x, int m)
        {
            return (x % m + m) % m;
        }
    }
}