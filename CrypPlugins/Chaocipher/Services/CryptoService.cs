using CrypTool.Chaocipher.Enums;
using CrypTool.Chaocipher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrypTool.Chaocipher.Services
{
    public class CryptoService
    {
        private List<PresentationState> _presentationStates = new List<PresentationState>();
        public string _inputCharInFocus = string.Empty;
        public string _outputCharInFocus = string.Empty;
        public bool Running { get; set; } = true;

        public CipherResult Encipher(string plainText, string initialLeftDisk, string initialRightDisk)
        {
            StringBuilder cipherTextStringBuilder = new StringBuilder();
            char[] plainWorkingAlphabet = initialRightDisk.ToCharArray();
            char[] cipherWorkingAlphabet = initialLeftDisk.ToCharArray();
            _inputCharInFocus = string.Empty;
            _outputCharInFocus = string.Empty;

            _presentationStates = new List<PresentationState>();

            AddPresentationState((char[])cipherWorkingAlphabet.Clone(),
                (char[])plainWorkingAlphabet.Clone(),
                Step.Begin, new object[] { plainText });

            for (int i = 0; i < plainText.Length && Running; i++)
            {
                _inputCharInFocus = char.ToString(plainText[i]);
                _outputCharInFocus = "?";
                if (Array.IndexOf(plainWorkingAlphabet, plainText[i]) == -1)
                {
                    AddPresentationState((char[])cipherWorkingAlphabet.Clone(),
                        (char[])plainWorkingAlphabet.Clone(),
                        Step.CharCanNotBeEnciphered, new object[] { plainText[i] });
                    cipherTextStringBuilder.Append(plainText[i]);
                    continue;
                }

                // Encipher
                BringPlainCharToZenith(plainText[i], plainWorkingAlphabet, cipherWorkingAlphabet);
                _outputCharInFocus = char.ToString(cipherWorkingAlphabet[0]);
                cipherTextStringBuilder.Append(cipherWorkingAlphabet[0]);
                // Do not permute disks in the last run
                if (i == plainText.Length - 1)
                {
                    AddPresentationState((char[])cipherWorkingAlphabet.Clone(),
                        (char[])plainWorkingAlphabet.Clone(),
                        Step.NotPermutateOnLastChar, null);
                    break;
                }

                PermuteCipherAlphabet(cipherWorkingAlphabet, plainWorkingAlphabet);
                PermutePlainAlphabet(plainWorkingAlphabet, cipherWorkingAlphabet);
            }

            _inputCharInFocus = string.Empty;
            _outputCharInFocus = string.Empty;
            AddPresentationState((char[])cipherWorkingAlphabet.Clone(),
                (char[])plainWorkingAlphabet.Clone(),
                Step.End, new object[] { cipherTextStringBuilder.ToString() });

            return new CipherResult(initialLeftDisk, initialRightDisk, cipherTextStringBuilder.ToString(),
                _presentationStates);
        }

        private void AddPresentationState(char[] cipherWorkingAlphabet, char[] plainWorkingAlphabet, Step step, object[] descriptionDetails)
        {
            _presentationStates.Add(new PresentationState(cipherWorkingAlphabet.ToList(),
                plainWorkingAlphabet.ToList(), step, descriptionDetails)
            {
                InputCharInFocus = _inputCharInFocus,
                OutputCharInFocus = _outputCharInFocus
            });
        }

        public CipherResult Decipher(string cipherText, string initialLeftDisk, string initialRightDisk)
        {
            StringBuilder plainTextStringBuilder = new StringBuilder();
            char[] plainWorkingAlphabet = initialRightDisk.ToCharArray();
            char[] cipherWorkingAlphabet = initialLeftDisk.ToCharArray();
            _presentationStates = new List<PresentationState>();
            _inputCharInFocus = "";
            _outputCharInFocus = "";

            AddPresentationState((char[])cipherWorkingAlphabet.Clone(),
                (char[])plainWorkingAlphabet.Clone(),
                Step.Begin, new object[] { cipherText });

            for (int i = 0; i < cipherText.Length && Running; i++)
            {
                _inputCharInFocus = char.ToString(cipherText[i]);
                _outputCharInFocus = "?";
                if (Array.IndexOf(plainWorkingAlphabet, cipherText[i]) == -1)
                {
                    AddPresentationState((char[])cipherWorkingAlphabet.Clone(),
                        (char[])plainWorkingAlphabet.Clone(),
                        Step.CharCanNotBeDeciphered, new object[] { cipherText[i] });
                    plainTextStringBuilder.Append(cipherText[i]);
                    continue;
                }
                // Encipher
                BringCipherCharToZenith(cipherText[i], plainWorkingAlphabet, cipherWorkingAlphabet);
                plainTextStringBuilder.Append(plainWorkingAlphabet[0]);
                _outputCharInFocus = char.ToString(plainWorkingAlphabet[0]);

                // Do not permute disks in the last run
                if (i == cipherText.Length - 1)
                {
                    break;
                }

                PermuteCipherAlphabet(cipherWorkingAlphabet, plainWorkingAlphabet);
                PermutePlainAlphabet(plainWorkingAlphabet, cipherWorkingAlphabet);
            }

            _inputCharInFocus = "";
            _outputCharInFocus = "";
            AddPresentationState((char[])cipherWorkingAlphabet.Clone(),
                (char[])plainWorkingAlphabet.Clone(),
                Step.End, new object[] { plainTextStringBuilder.ToString() });

            return new CipherResult(initialLeftDisk, initialRightDisk, plainTextStringBuilder.ToString(),
                _presentationStates);
        }

        public void BringPlainCharToZenith(char character, char[] plainWorkingAlphabet,
            char[] cipherWorkingAlphabet)
        {
            for (int i = 0; i < plainWorkingAlphabet.Length && !Equals(character, plainWorkingAlphabet[0]); i++)
            {
                AddPresentationState(cipherWorkingAlphabet, plainWorkingAlphabet, Step.BringCharToZenith,
                    new object[] { character, plainWorkingAlphabet[0] });
                MoveAllElementsByOne(cipherWorkingAlphabet);
                MoveAllElementsByOne(plainWorkingAlphabet);
            }

            AddPresentationState((char[])cipherWorkingAlphabet.Clone(),
                (char[])plainWorkingAlphabet.Clone(), Step.CharBroughtToZenith, new object[] { character });
        }

        public void BringCipherCharToZenith(char character, char[] plainWorkingAlphabet,
            char[] cipherWorkingAlphabet)
        {
            for (int i = 0; i < cipherWorkingAlphabet.Length && !Equals(character, cipherWorkingAlphabet[0]); i++)
            {
                AddPresentationState(cipherWorkingAlphabet, plainWorkingAlphabet, Step.BringCipherCharToZenith,
                    new object[] { character, cipherWorkingAlphabet[0] });
                MoveAllElementsByOne(cipherWorkingAlphabet);
                MoveAllElementsByOne(plainWorkingAlphabet);
            }

            AddPresentationState(cipherWorkingAlphabet, plainWorkingAlphabet, Step.CipherCharBroughtToZenith,
                new object[] { character });
        }

        public static void MoveAllElementsByOne(char[] arr)
        {
            char tempAlphabet = arr[0];
            for (int i = 0; i < arr.Length - 1; i++)
            {
                arr[i] = arr[i + 1];
            }

            arr[arr.Length - 1] = tempAlphabet;
        }

        public static void MoveElementToIndex(int fromIndex, int toIndex, char[] arr)
        {
            arr[toIndex] = arr[fromIndex];
            arr[fromIndex] = char.MinValue;
        }

        // Permute cipher alphabet (left disk)
        public void PermuteCipherAlphabet(char[] cipherWorkingAlphabet,
            char[] plainWorkingAlphabet)
        {
            AddPresentationState((char[])cipherWorkingAlphabet.Clone(),
                (char[])plainWorkingAlphabet.Clone(),
                Step.PermutateLeftDisk,
                null);

            // letter extracted from position 2
            char removedChar = cipherWorkingAlphabet[1];
            cipherWorkingAlphabet[1] = char.MinValue;
            AddPresentationState((char[])cipherWorkingAlphabet.Clone(),
                (char[])plainWorkingAlphabet.Clone(),
                Step.PermutateLeftDiskRemoveChar, new object[] { removedChar, 3 });

            // block 3-14 shifted to the left
            for (int i = 1; i < 13; i++)
            {
                MoveElementToIndex(i + 1, i, cipherWorkingAlphabet);
                AddPresentationState((char[])cipherWorkingAlphabet.Clone(),
                    (char[])plainWorkingAlphabet.Clone(),
                    Step.PermutateRightDiskMoveChars,
                    new object[] { cipherWorkingAlphabet[i], cipherWorkingAlphabet[i + 1] });
            }

            // extracted letter inserted at nadir
            cipherWorkingAlphabet[13] = removedChar;
            //BuildPlainCircle(PlainWorkingAlphabet);
            AddPresentationState((char[])cipherWorkingAlphabet.Clone(),
                (char[])plainWorkingAlphabet.Clone(),
                Step.PermutateLeftDiskInsertChar,
                new object[] { removedChar });
        }

        // Permute plain alphabet (Right disk)
        public void PermutePlainAlphabet(char[] plainWorkingAlphabet,
            char[] cipherWorkingAlphabet)
        {
            AddPresentationState((char[])cipherWorkingAlphabet.Clone(),
                (char[])plainWorkingAlphabet.Clone(),
                Step.PermutateRightDisk,
                null);

            MoveAllElementsByOne(plainWorkingAlphabet);
            AddPresentationState((char[])cipherWorkingAlphabet.Clone(),
                (char[])plainWorkingAlphabet.Clone(),
                Step.PermutateRightDiskMoveByOne,
                null);

            // letter extracted from position 2
            char removedChar = plainWorkingAlphabet[2];
            plainWorkingAlphabet[2] = char.MinValue;
            AddPresentationState((char[])cipherWorkingAlphabet.Clone(),
                (char[])plainWorkingAlphabet.Clone(),
                Step.PermutateRightDiskRemoveChar,
                new object[] { removedChar, 2 });

            // block 3-14 shifted to the left
            for (int i = 2; i < 13; i++)
            {
                MoveElementToIndex(i + 1, i, plainWorkingAlphabet);
                AddPresentationState((char[])cipherWorkingAlphabet.Clone(),
                    (char[])plainWorkingAlphabet.Clone(),
                    Step.PermutateLeftDiskMoveChars,
                    new object[] { plainWorkingAlphabet[i], plainWorkingAlphabet[i + 1] });
            }

            // extracted letter inserted at nadir
            plainWorkingAlphabet[13] = removedChar;
            AddPresentationState((char[])cipherWorkingAlphabet.Clone(),
                (char[])plainWorkingAlphabet.Clone(),
                Step.PermutateRightDiskInsertChar,
                new object[] { removedChar });
        }
    }
}