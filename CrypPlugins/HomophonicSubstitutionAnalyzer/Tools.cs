/*
   Copyright 2019 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrypTool.Plugins.HomophonicSubstitutionAnalyzer
{
    public class Tools
    {
        /// <summary>
        /// Maps the homophones into number space
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static int[] MapHomophoneTextNumbersIntoNumberSpace(string text)
        {
            int[] numbers = new int[text.Length / 2];
            int position = 0;
            for (int i = 0; i < text.Length; i += 2)
            {
                numbers[position] = int.Parse(text.Substring(i, 2));
                position++;
            }
            return numbers;
        }

        /// <summary>
        /// Convert homophones to list of strings
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static List<string> ConvertHomophoneTextNumbersToListOfStrings(string text)
        {
            List<string> list = new List<string>();
            int position = 0;
            for (int i = 0; i < text.Length; i += 2)
            {
                list.Add("" + int.Parse(text.Substring(i, 2)));
                position++;
            }
            return list;
        }

        /// <summary>
        /// Maps an arry of numbers into text space usnig the given alphabet
        /// </summary>
        /// <param name="numbers"></param>
        /// <param name="alphabet"></param>
        /// <returns></returns>
        public static string MapNumbersIntoTextSpace(int[] numbers, string alphabet)
        {
            StringBuilder builder = new StringBuilder();
            foreach (int i in numbers)
            {
                //null value                
                try
                {
                    builder.Append(alphabet[i]);
                }
                catch (IndexOutOfRangeException)
                {
                    //do nothing; letter is not in alphabet; thus, it is ignored
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Maps the given string into numberspace using the given alphabet
        /// </summary>
        /// <param name="text"></param>
        /// <param name="alphabet"></param>
        /// <returns></returns>
        public static int[] MapIntoNumberSpace(string text, string alphabet)
        {
            int[] numbers = new int[text.Length];
            int position = 0;
            foreach (char c in text)
            {
                numbers[position] = alphabet.IndexOf(c);
                position++;
            }
            return numbers;
        }

        /// <summary>
        /// Returns a string only containing each letter once based on the given string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static int[] Distinct(int[] text)
        {
            HashSet<int> symbols = new HashSet<int>();

            foreach (char c in text)
            {
                if (!symbols.Contains(c))
                {
                    symbols.Add(c);
                }
            }
            return symbols.ToArray();
        }

        /// <summary>
        /// Changes an integer array to "consecutive number"
        /// for example
        /// 9,5,6,6,1,1,1,5,3,3,9,9 is converted to
        /// 0,1,2,2,3,3,3,1,4,4,0,0
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static int[] ChangeToConsecutiveNumbers(int[] text)
        {
            int number = 0;
            int[] newtext = new int[text.Length];
            Dictionary<int, int> mapping = new Dictionary<int, int>();

            for (int i = 0; i < text.Length; i++)
            {
                if (!mapping.Keys.Contains(text[i]))
                {
                    mapping.Add(text[i], number);
                    number++;
                }
                newtext[i] = mapping[text[i]];
            }
            return newtext;
        }

        /// <summary>
        /// Removes all chars from the given string that are not in the given alphabet
        /// </summary>
        /// <param name="text"></param>
        /// <param name="alphabet"></param>
        /// <returns></returns>
        public static string RemoveInvalidChars(string text, string alphabet)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char c in text)
            {
                if (alphabet.Contains(c))
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Maps the text into numberspace
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static int[] MapHomophonesIntoNumberSpace(string text)
        {
            int[] numbers = new int[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                numbers[i] = text[i];
            }
            return numbers;
        }

        /// <summary>
        /// Converts the text to a list of string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static List<string> ConvertHomophonesToListOfString(string text)
        {
            List<string> list = new List<string>();
            for (int i = 0; i < text.Length; i++)
            {
                list.Add("" + text[i]);
            }
            return list;
        }

        /// <summary>
        /// Maps a string containing "blocks" of homophones into numberspace separated by separator
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <returns></returns>
        public static int[] MapHomophoneCommaSeparatedIntoNumberSpace(string text, char separator = ' ')
        {
            string[] blocks = text.Split(separator);
            //list to generate number array
            List<int> numbers = new List<int>();
            //dictionary to memorize homophones
            Dictionary<string, int> homophones = new Dictionary<string, int>();
            int counter = 0;

            foreach (string block in blocks)
            {
                string trimmedblock = block.Trim();
                //here, we check if the block is empty
                //since we want to have spaces in our ciphertext, we need to check if the block contains a space or tab and add it to the trimmed block
                if(trimmedblock.Length == 0 )
                {
                    if(block.Contains(" ") || block.Contains("\t"))
                    {
                        trimmedblock = " ";
                    }
                    else
                    {
                        //no space, so we ignore the empty trimmed block
                        continue;
                    }
                }                
                if (!homophones.ContainsKey(trimmedblock))
                {
                    homophones.Add(trimmedblock, counter);
                    counter++;
                }
                numbers.Add(homophones[trimmedblock]);
            }
            return numbers.ToArray();
        }

        /// <summary>
        /// Maps a string containing "blocks" of homophones into numberspace separated by separator
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <returns></returns>
        public static List<string> ConvertHomophoneCommaSeparatedToListOfStrings(string text, char separator = ' ')
        {
            string[] blocks = text.Split(separator);
            List<string> list = new List<string>();

            foreach (string block in blocks)
            {
                string trimmedblock = block.Trim();
                list.Add(trimmedblock);
            }
            return list;
        }
    }
}
