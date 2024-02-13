namespace LanguageStatisticsLib
{
    /// <summary>
    /// Enum of types of ngrams
    /// </summary>
    public enum GramsType
    {
        Undefined = 0,               // invalid type
        Unigrams = 1,                // 1-grams
        Bigrams = 2,                 // 2-grams
        Trigrams = 3,                // 3-grams
        Tetragrams = 4,              // 4-grams
        Pentragrams = 5,             // 5-grams
        Hexagrams = 6                // 6-grams
    }
}