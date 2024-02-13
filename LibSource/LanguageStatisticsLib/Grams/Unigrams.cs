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
    public class Unigrams : Grams
    {
        public float[] Frequencies;

        public Unigrams(string language, string languageStatisticsDirectory, bool useSpaces = false) : base(language, languageStatisticsDirectory, useSpaces)
        {
        }

        public override void LoadGZ(string filename, string languageStatisticsDirectory)
        {
            LanguageStatisticsFile file = new LanguageStatisticsFile(Path.Combine(languageStatisticsDirectory, filename));
            Frequencies = (float[])file.LoadFrequencies(1);
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
            int end = text.Length;
            if (end <= 0)
            {
                return 0;
            }

            double value = 0;

            for (int i = 0; i < end; i++)
            {
                int a = text[i];

                if (addLetterIndicies != null)
                {
                    a += addLetterIndicies[a];
                }

                if (a >= Alphabet.Length ||
                    a < 0)
                {
                    continue;
                }
                value += Frequencies[a];

            }
            return value / end;
        }

        public override int GramSize()
        {
            return 1;
        }

        public override double CalculateCost(List<int> text)
        {
            int end = text.Count;
            if (end <= 0)
            {
                return 0;
            }

            double value = 0;

            for (int i = 0; i < end; i++)
            {
                int a = text[i];

                if (addLetterIndicies != null)
                {
                    a += addLetterIndicies[a];
                }

                if (a >= Alphabet.Length ||
                    a < 0)
                {
                    continue;
                }
                value += Frequencies[a];

            }
            return value / end;
        }

        public override GramsType GramsType()
        {
            return LanguageStatisticsLib.GramsType.Unigrams;
        }

        public override void Normalize(float maxValue)
        {
            base.Normalize(maxValue);
            float adjustValue = MaxValue * maxValue;
            for (int a = 0; a < Alphabet.Length; a++)
            {
                Frequencies[a] = adjustValue / Frequencies[a];
            }
        }
    }
}