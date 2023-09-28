using CrypTool.PluginBase.Utils;

namespace CrypTool.JosseCipherAnalyzer.Evaluator
{
    public class IndexOfCoincidence : IEvaluator
    {
        private string Alphabet { get; }

        public IndexOfCoincidence(string alphabet)
        {
            Alphabet = alphabet;
        }

        public double Evaluate(string input)
        {
            return LanguageStatistics.CalculateIoC(LanguageStatistics.MapTextIntoNumberSpace(input, Alphabet));
        }
    }
}