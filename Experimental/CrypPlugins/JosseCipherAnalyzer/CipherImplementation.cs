using System.Collections.Generic;
using System.Text;

namespace CrypTool.JosseCipherAnalyzer
{
    /// <summary>
    /// Josse decryption working on letter numbers instead of strings, so the analyzer can test
    /// keys without allocating anything. A letter number is the index of the letter in <see cref="Alphabet"/>.
    /// </summary>
    public class CipherImplementation
    {
        private readonly int[] _board;          // board position -> letter
        private readonly int[] _boardNumber;    // letter -> board position + 1
        private readonly int[] _orderedLetters; // key letters first, then the rest of the alphabet
        private readonly bool[] _inKey;
        private readonly bool[] _isWhiteSpace;

        public string Alphabet { get; }

        public CipherImplementation(string alphabet)
        {
            Alphabet = alphabet;
            int length = alphabet.Length;
            _board = new int[length];
            _boardNumber = new int[length];
            _orderedLetters = new int[length];
            _inKey = new bool[length];
            _isWhiteSpace = new bool[length];
            for (int i = 0; i < length; i++)
            {
                _isWhiteSpace[i] = char.IsWhiteSpace(alphabet[i]);
            }
        }

        /// <summary>
        /// Builds the board for the given key (letter numbers). White space and repeated letters are skipped.
        /// </summary>
        public void SetKey(IReadOnlyList<int> key)
        {
            int length = Alphabet.Length;
            for (int i = 0; i < length; i++)
            {
                _inKey[i] = false;
            }

            // Key letters first, then the remaining alphabet
            int count = 0;
            foreach (int letter in key)
            {
                if (letter < 0 || _isWhiteSpace[letter] || _inKey[letter])
                {
                    continue;
                }
                _inKey[letter] = true;
                _orderedLetters[count++] = letter;
            }
            int columns = count == 0 ? 1 : count;
            for (int letter = 0; letter < length; letter++)
            {
                if (!_inKey[letter])
                {
                    _orderedLetters[count++] = letter;
                }
            }

            // Read the letters column by column into the board
            int position = 0;
            for (int column = 0; column < columns; column++)
            {
                for (int row = column; row < length; row += columns)
                {
                    int letter = _orderedLetters[row];
                    _board[position] = letter;
                    _boardNumber[letter] = position + 1;
                    position++;
                }
            }
        }

        /// <summary>
        /// Deciphers the letter numbers in <paramref name="ciphertext"/> into <paramref name="plaintext"/>,
        /// which must be at least as long. As in the Josse cipher component, board numbers run from 1 to n,
        /// so a result of 0 (mod n) stands for n.
        /// </summary>
        public void Decipher(int[] ciphertext, int[] plaintext)
        {
            int length = _board.Length;
            int previous = 0;
            for (int i = 0; i < ciphertext.Length; i++)
            {
                int current = _boardNumber[ciphertext[i]];
                int number;
                if (i == 0)
                {
                    number = length - current;
                }
                else if (i == 1)
                {
                    // the plaintext number of the first letter is n - previous
                    number = (current + previous) % length;
                }
                else
                {
                    number = (current - previous) % length;
                }
                if (number <= 0)
                {
                    number += length;
                }
                plaintext[i] = _board[number - 1];
                previous = current;
            }
        }

        /// <summary>
        /// Converts text into letter numbers, leaving out characters that are not in the alphabet.
        /// </summary>
        public int[] MapToNumbers(string text)
        {
            List<int> numbers = new List<int>(text.Length);
            foreach (char c in text)
            {
                int number = Alphabet.IndexOf(c);
                if (number >= 0)
                {
                    numbers.Add(number);
                }
            }
            return numbers.ToArray();
        }

        public string MapToText(IReadOnlyList<int> numbers)
        {
            StringBuilder builder = new StringBuilder(numbers.Count);
            foreach (int number in numbers)
            {
                builder.Append(Alphabet[number]);
            }
            return builder.ToString();
        }
    }
}
