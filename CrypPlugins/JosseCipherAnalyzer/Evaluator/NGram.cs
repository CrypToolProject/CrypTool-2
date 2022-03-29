using CrypTool.PluginBase.Utils;

namespace CrypTool.JosseCipherAnalyzer.Evaluator
{
    public class NGram : IEvaluator
    {
        private readonly Grams _grams;

        public NGram(LanguageStatistics.GramsType gramsType, int language, bool useSpaces)
        {
            _grams = LanguageStatistics.CreateGrams(gramsType, LanguageStatistics.LanguageCode(language), useSpaces);
        }

        public double Evaluate(string input)
        {
            return _grams.CalculateCost(input);
        }
    }
}