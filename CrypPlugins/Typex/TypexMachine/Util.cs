/*
   Copyright 2023 Nils Kopal <kopal<AT>cryptool.org>

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

namespace CrypTool.Typex.TypexMachine
{
    /// <summary>
    /// Static class with some helper functions and some extensions
    /// for string and int array
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Computes mathematical modulo operation
        /// </summary>
        /// <param name="a"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int Mod(int a, int n)
        {
            return ((a % n) + n) % n;
        }

        /// <summary>
        /// Maps a given string into the "numberspace" defined by the alphabet
        /// </summary>
        /// <param name="text"></param>
        /// <param name="alphabet"></param>
        /// <returns></returns>
        public static int[] MapTextIntoNumberSpace(string text, string alphabet)
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
        /// Extension for int array: Returns index of the element in the integer array. If element is not in the array, it returns -1
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static int IndexOf(this int[] array, int element)
        {
            for (int index = 0; index < array.Length; index++)
            {
                if (array[index] == element)
                {
                    return index;
                }
            }
            return -1;
        }

        /// <summary>
        /// Extension for int array: Returns true, if given element is in the integer array
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Contains(this int[] array, int value)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == value)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Extension for int array: Returns the reversed integer array
        /// </summary>
        /// <returns></returns>
        public static int[] Reverse(this int[] array)
        {
            int[] newarray = new int[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                newarray[i] = array[array.Length - i - 1];
            }
            return newarray;
        }
    }
}
