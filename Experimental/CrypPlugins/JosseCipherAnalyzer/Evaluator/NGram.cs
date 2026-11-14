using CrypTool.PluginBase.IO;
using LanguageStatisticsLib;

namespace CrypTool.JosseCipherAnalyzer.Evaluator
{
    public class NGram : IEvaluator
    {
        private readonly Grams _grams;
        private readonly int[] _gramsLetter; // analyzer letter -> letter number of the n-gram statistics
        private int[] _buffer = new int[0];

        public NGram(GramsType gramsType, int language, bool useSpaces, string alphabet)
        {
            _grams = LanguageStatistics.CreateGrams(language, DirectoryHelper.DirectoryLanguageStatistics, gramsType, useSpaces);
            _gramsLetter = new int[alphabet.Length];
            for (int i = 0; i < alphabet.Length; i++)
            {
                _gramsLetter[i] = _grams.Alphabet.IndexOf(alphabet[i]);
            }
        }

        public double Evaluate(int[] plaintext)
        {
            if (_buffer.Length != plaintext.Length)
            {
                _buffer = new int[plaintext.Length];
            }
            for (int i = 0; i < plaintext.Length; i++)
            {
                _buffer[i] = _gramsLetter[plaintext[i]];
            }
            return _grams.CalculateCost(_buffer);
        }
    }
}
