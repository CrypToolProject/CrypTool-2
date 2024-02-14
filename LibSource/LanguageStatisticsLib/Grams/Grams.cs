/*                              
   Copyright 2024 Nils Kopal, CrypTool Team

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
using System.IO;
using System.Linq;

namespace LanguageStatisticsLib
{

    /// <summary>
    /// Abstract super class for nGrams classes
    /// </summary>
    public abstract class Grams
    {
        public float MaxValue { get; protected set; }

        public bool IsNormalized { get; private set; }

        public Grams(string language, string languageStatisticsDirectory, bool useSpaces)
        {
            string filename = string.Format("{0}-{1}gram-nocs{2}.gz", language, GramSize(), useSpaces ? "-sp" : "");
            try
            {
                LoadGZ(filename, languageStatisticsDirectory);
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                throw new Exception(string.Format("Did not find the specified language statistics file for language={0} and useSpaces={1}: {2}", language, useSpaces, filename), fileNotFoundException);
            }
        }

        /// <summary>
        /// Alphabet of this Grams object
        /// </summary>
        public string Alphabet { get; protected set; }

        protected int[] addLetterIndicies = null;

        /// <summary>
        /// Calculates the cost value of the given text stored in the array of integers
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public abstract double CalculateCost(int[] text);

        /// <summary>
        /// Calculates the cost value of the given text stored in the list of integers
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public abstract double CalculateCost(List<int> text);

        /// <summary>
        /// Calculates the cost value of the given text. Uses the alphabet of this Gram object to convert
        /// from string to numbers before calculation
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public double CalculateCost(string text)
        {
            return CalculateCost(MapTextIntoNumberSpace(text, Alphabet));
        }

        /// <summary>
        /// Returns the size of this Grams, e.g. 4 for tetragrams
        /// </summary>
        /// <returns></returns>
        public abstract int GramSize();

        /// <summary>
        /// Returns the type of this Grams object
        /// </summary>
        /// <returns></returns>
        public abstract GramsType GramsType();

        /// <summary>
        /// Method which loads the frequencies from a CT2 language statistics file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="languageStatisticsDirectory"></param>
        public abstract void LoadGZ(string filename, string languageStatisticsDirectory);

        /// <summary>
        /// This method reduces the alphabet by "blending out" letters that are not in this alphabet
        /// Letters in the used alphabet have to be in the original alphabet of this Grams
        /// 
        /// example
        /// 
        /// original ABCDEFGH
        /// new      ABCEFH
        /// 
        /// addLetterIndicies will be 000122
        /// These add values are then added during cost calculation
        /// This "fixes" the letter indices to be compatible with smaller alphabets
        /// 
        /// </summary>
        /// <param name="newAlphabet"></param>
        public void ReduceAlphabet(string newAlphabet)
        {
            if (newAlphabet.Length == Alphabet.Length)
            {
                addLetterIndicies = null;
                return;
            }
            addLetterIndicies = new int[newAlphabet.Length];
            int addValue = 0;
            for (int i = 0; i < newAlphabet.Length; i++)
            {
                if (!newAlphabet.Contains(Alphabet[i]))
                {
                    addValue++;
                }
                addLetterIndicies[i] = addValue;
            }
        }

        /// <summary>
        /// Normalizes this Unigrams between 0 and maxValue
        /// Throws an exception, if it has already been normalized
        /// </summary>
        public virtual void Normalize(float maxValue)
        {
            if (IsNormalized)
            {
                throw new InvalidOperationException("This Gram object has already been normalized!");
            }
            IsNormalized = true;
        }

        /// <summary>
        /// Maps a given string into the "numberspace" defined by the alphabet
        /// </summary>
        /// <param name="text"></param>
        /// <param name="alphabet"></param>
        /// <returns></returns>
        private static int[] MapTextIntoNumberSpace(string text, string alphabet)
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
    }
}