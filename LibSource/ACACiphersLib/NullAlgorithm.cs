using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CrypTool.ACACiphersLib
{
    public class NullAlgorithm : Cipher
    {
        public NullAlgorithm(string[] dictionary, List<string> parameters)
        {
            Dictionary = dictionary;
            Mode = parameters[0];
        }
        public string[] Dictionary { get; }
        public string Mode { get; }

        //string[] middleLetterArray = { "COAST", "", "", "", "", "", "", "", "", "", "", "", "", "ONE", "PHONE", "APPLE", "", "", "", "EXTRA", "", "", "", "AXE", "", "" };
        //string[] firstLetterArray = { "APPLE", "BANANA", "COAST", "DOUBLE", "EGG", "FIRST", "GARDEN", "HOUSE", "ILLINOIS", "JUNKFOOD", "KINDERGARDEN", "LOST", "MAN", "NAME", "OLD", "PAYLOAD", "QUEUE", "REST", "SHEEP", "TAKEN", "UNFAIR", "VAGUENESS", "WAY", "XENON", "YOU", "ZEBRA" };

        public override int[] Decrypt(int[] ciphertext, int[] key)
        {
            int[] plaintext = new int[ciphertext.Length];
            string text = Tools.MapNumbersIntoTextSpace(ciphertext, LATIN_ALPHABET);
            string[] text_array = text.Split();
            string[] intermediate_result = new string[text_array.Length];
            for (int j = 0; j < text_array.Length-1; j++)
            {
                    var offset = text_array[j].Length % 2 == 0 ? 1 : 0;
                    var middle = text_array[j].Substring(text_array[j].Length / 2 - offset, offset + 1);
                    intermediate_result[j] = middle;
            }

            plaintext = Tools.MapTextIntoNumberSpace(String.Join("",intermediate_result),LATIN_ALPHABET);

            return plaintext;
        }

        public override int[] Encrypt(int[] plaintext, int[] key)
        {
            var random = new Random();
            int[] ciphertext = new int[plaintext.Length];
            string final_text = String.Empty;

            for (int i = 0; i< plaintext.Length;i++)
            {
                List<string> validWords = GenerateWord(plaintext[i]);
                int index = random.Next(validWords.Count);
                string randomWord = validWords[index];
                final_text = final_text + randomWord + " ";
            }

            ciphertext = Tools.MapTextIntoNumberSpace(final_text,LATIN_ALPHABET);

            return ciphertext;
        }

        public List<string> GenerateWord(int letter)
        {

            List<string> validWords = new List<string>();

            //only odd number of letters
            for (int i=0; i<Dictionary.Length;i++)
            {
                if (Dictionary[i].Length % 2 == 1 && !(Dictionary[i].Contains("-")))
                {
                    validWords.Add(Dictionary[i].ToUpper());
                }

            }

            //remove special letters
            for (int i = 0; i < validWords.Count; i++)
            {
                Regex.Replace(validWords[i], "[^a-zA-Z0-9% ._-]", string.Empty);
            }

            List<string> finalWords = new List<string>();

            //check if same letter
            for (int j = 0; j < validWords.Count; j++)
            {
                var offset = validWords[j].Length % 2 == 0 ? 1 : 0;
                var middle = validWords[j].Substring(validWords[j].Length / 2 - offset, offset + 1);
                int[] middleLetter = Tools.MapTextIntoNumberSpace(middle, LATIN_ALPHABET);
                var first = validWords[j].Substring(0,1);
                int[] firstLetter = Tools.MapTextIntoNumberSpace(first, LATIN_ALPHABET);
                var last = validWords[j][validWords[j].Length - 1].ToString();
                int[] lastLetter = Tools.MapTextIntoNumberSpace(last,LATIN_ALPHABET);
                if (Mode == "MiddleLetter") {
                    if (middleLetter[0] == letter && isValid(validWords[j]))
                    {
                        finalWords.Add(validWords[j]);
                    }
                }else if (Mode == "FirstLetter")
                {
                    if (firstLetter[0] == letter && isValid(validWords[j]))
                    {
                        finalWords.Add(validWords[j]);
                    }

                }else if (Mode == "LastLetter")
                {
                    if (lastLetter[0] == letter && isValid(validWords[j]))
                    {
                        finalWords.Add(validWords[j]);
                    }
                }
            }

            return finalWords;
        }

        private static bool isValid(String str)
        {
            return Regex.IsMatch(str, @"^[A-Z]+$");
        }
    }
}
