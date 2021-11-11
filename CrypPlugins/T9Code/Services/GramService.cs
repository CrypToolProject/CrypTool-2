using System;
using System.Collections.Generic;
using System.Linq;
using CrypTool.PluginBase.Utils;
using CrypTool.T9Code.Enums;

namespace CrypTool.T9Code.Services
{
    public class GramService
    {
        private Dictionary<int, Grams> _gramsCache =
            new Dictionary<int, Grams>();

        private int? _languageIndex;

        private void FillCache()
        {
            foreach (var enumValue in Enum.GetValues(typeof(LanguageStatistics.GramsType))
                .Cast<LanguageStatistics.GramsType>())
            {
                try
                {
                    if (_languageIndex == null) continue;
                    var grams = LanguageStatistics.CreateGrams(_languageIndex.Value, enumValue, false);
                    _gramsCache.Add(grams.GramSize(), grams);
                }
                catch
                {
                    //ignore
                }
            }
        }

        public double CalculateCostOfNGram(int gramSize, string value) =>
            _gramsCache.ContainsKey(gramSize) ? _gramsCache[gramSize].CalculateCost(value) : double.MinValue;

        public string FindWordWithHighestScore(IList<string> possibleStrings, InternalGramType gramSize)
        {
            var score = new double[possibleStrings.Count];
            for (var index = 0; index < possibleStrings.Count; index++)
            {
                var possibleString = possibleStrings[index];
                var possibleStringIndex = 0;
                while (possibleStringIndex < possibleString.Length)
                {
                    var value = GramTypeToInt(ConvertGramsType(gramSize));
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

            var maxIndex = score.ToList().IndexOf(score.Max());
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

        private int GramTypeToInt(LanguageStatistics.GramsType gramsType)
        {
            switch (gramsType)
            {
                case LanguageStatistics.GramsType.Undefined:
                    return 0;
                case LanguageStatistics.GramsType.Unigrams:
                    return 1;
                case LanguageStatistics.GramsType.Bigrams:
                    return 2;
                case LanguageStatistics.GramsType.Trigrams:
                    return 3;
                case LanguageStatistics.GramsType.Tetragrams:
                    return 4;
                case LanguageStatistics.GramsType.Pentragrams:
                    return 5;
                case LanguageStatistics.GramsType.Hexagrams:
                    return 6;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gramsType), gramsType, null);
            }
        }

        private LanguageStatistics.GramsType ConvertGramsType(InternalGramType gramType) =>
            (LanguageStatistics.GramsType) Enum.Parse(typeof(LanguageStatistics.GramsType), gramType.ToString());
    }
}