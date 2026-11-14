using System;

namespace CrypTool.JosseCipherAnalyzer.Evaluator
{
    /// <summary>
    /// Negative Shannon entropy of the letter distribution, so lower entropy rates higher.
    /// </summary>
    public class Entropy : IEvaluator
    {
        private readonly int[] _counts;

        public Entropy(string alphabet)
        {
            _counts = new int[alphabet.Length];
        }

        public double Evaluate(int[] plaintext)
        {
            Array.Clear(_counts, 0, _counts.Length);
            foreach (int letter in plaintext)
            {
                _counts[letter]++;
            }

            double value = 0;
            foreach (int count in _counts)
            {
                if (count > 0)
                {
                    double frequency = (double)count / plaintext.Length;
                    value += frequency * Math.Log(frequency, 2);
                }
            }
            return value;
        }
    }
}
