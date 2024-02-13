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
using System.Collections.Generic;
using System.IO;

namespace LanguageStatisticsLib
{
    public class Trigrams : Grams
    {
        public float[,,] Frequencies;

        public Trigrams(string language,string languageStatisticsDirectory, bool useSpaces = false) : base(language, languageStatisticsDirectory, useSpaces)
        {
        }

        public override void LoadGZ(string filename, string languageStatisticsDirectory)
        {
            LanguageStatisticsFile file = new LanguageStatisticsFile(Path.Combine(languageStatisticsDirectory, filename));
            Frequencies = (float[,,])file.LoadFrequencies(3);
            Alphabet = file.Alphabet;
            MaxValue = float.MinValue;
            foreach (float value in Frequencies)
            {
                if (value > MaxValue)
                {
                    MaxValue = value;
                }
            }
        }

        public override double CalculateCost(int[] text)
        {
            int end = text.Length - 2;
            if (end <= 0)
            {
                return 0;
            }

            double value = 0;
            int alphabetLength = Alphabet.Length;

            for (int i = 0; i < end; i++)
            {
                int a = text[i];
                int b = text[i + 1];
                int c = text[i + 2];

                if (addLetterIndicies != null)
                {
                    a += addLetterIndicies[a];
                    b += addLetterIndicies[b];
                    c += addLetterIndicies[c];
                }

                if (a >= alphabetLength ||
                    b >= alphabetLength ||
                    c >= alphabetLength ||
                    a < 0 ||
                    b < 0 ||
                    c < 0)
                {
                    continue;
                }
                value += Frequencies[a, b, c];
            }
            return value / end;
        }

        public override int GramSize()
        {
            return 3;
        }

        public override double CalculateCost(List<int> text)
        {
            int end = text.Count - 2;
            if (end <= 0)
            {
                return 0;
            }

            double value = 0;
            int alphabetLength = Alphabet.Length;

            for (int i = 0; i < end; i++)
            {
                int a = text[i];
                int b = text[i + 1];
                int c = text[i + 2];

                if (addLetterIndicies != null)
                {
                    a += addLetterIndicies[a];
                    b += addLetterIndicies[b];
                    c += addLetterIndicies[c];
                }

                if (a >= alphabetLength ||
                    b >= alphabetLength ||
                    c >= alphabetLength ||
                    a < 0 ||
                    b < 0 ||
                    c < 0)
                {
                    continue;
                }
                value += Frequencies[a, b, c];
            }
            return value / end;
        }

        public override GramsType GramsType()
        {
            return LanguageStatisticsLib.GramsType.Trigrams;
        }

        public override void Normalize(float maxValue)
        {
            base.Normalize(maxValue);
            float adjustValue = MaxValue * maxValue;
            for (int a = 0; a < Alphabet.Length; a++)
            {
                for (int b = 0; b < Alphabet.Length; b++)
                {
                    for (int c = 0; c < Alphabet.Length; c++)
                    {
                        Frequencies[a, b, c] = adjustValue / Frequencies[a, b, c];
                    }
                }
            }
        }
    }
}