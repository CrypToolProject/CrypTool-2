/*
   Copyright (C) CrypTool 2 Team

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
using CrypTool.PluginBase.IO;
using CrypTool.T9Code.Enums;
using LanguageStatisticsLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrypTool.T9Code.Services
{
    public class GramService
    {
        private readonly Dictionary<int, Grams> _gramsCache =
            new Dictionary<int, Grams>();

        private int? _languageIndex;

        private void FillCache()
        {
            foreach (GramsType enumValue in Enum.GetValues(typeof(GramsType))
                .Cast<GramsType>())
            {
                try
                {
                    if (_languageIndex == null)
                    {
                        continue;
                    }

                    Grams grams = LanguageStatistics.CreateGrams(_languageIndex.Value, DirectoryHelper.DirectoryLanguageStatistics, enumValue, false);
                    _gramsCache.Add(grams.GramSize(), grams);
                }
                catch
                {
                    //ignore
                }
            }
        }

        public double CalculateCostOfNGram(int gramSize, string value)
        {
            return _gramsCache.ContainsKey(gramSize) ? _gramsCache[gramSize].CalculateCost(value) : double.MinValue;
        }

        public string FindWordWithHighestScore(IList<string> possibleStrings, InternalGramType gramSize)
        {
            double[] score = new double[possibleStrings.Count];
            for (int index = 0; index < possibleStrings.Count; index++)
            {
                string possibleString = possibleStrings[index];
                int possibleStringIndex = 0;
                while (possibleStringIndex < possibleString.Length)
                {
                    int value = GramTypeToInt(ConvertGramsType(gramSize));
                    if (possibleString.Length < value)
                    {
                        value = possibleString.Length;
                    }

                    if (value > 0 && value < 4)
                    {
                        score[index] = CalculateCostOfNGram(value,
                            possibleString.ToUpper());
                        possibleStringIndex += value;
                    }
                    else
                    {
                        score[index] = CalculateCostOfNGram(2,
                            possibleString.ToUpper());
                        possibleStringIndex += 2;
                    }
                }
            }

            int maxIndex = score.ToList().IndexOf(score.Max());
            return possibleStrings[maxIndex];
        }

        public void SetGramLanguage(int language)
        {
            if (language == _languageIndex)
            {
                // We can return early, since language has not changed.
                return;
            }
            _languageIndex = language;
            FillCache();
        }

        private int GramTypeToInt(GramsType gramsType)
        {
            switch (gramsType)
            {
                case GramsType.Undefined:
                    return 0;
                case GramsType.Unigrams:
                    return 1;
                case GramsType.Bigrams:
                    return 2;
                case GramsType.Trigrams:
                    return 3;
                case GramsType.Tetragrams:
                    return 4;
                case GramsType.Pentragrams:
                    return 5;
                case GramsType.Hexagrams:
                    return 6;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gramsType), gramsType, null);
            }
        }

        private GramsType ConvertGramsType(InternalGramType gramType)
        {
            return (GramsType)Enum.Parse(typeof(GramsType), gramType.ToString());
        }
    }
}