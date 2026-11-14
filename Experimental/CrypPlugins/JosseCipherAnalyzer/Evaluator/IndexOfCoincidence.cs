namespace CrypTool.JosseCipherAnalyzer.Evaluator
{
    public class IndexOfCoincidence : IEvaluator
    {
        private readonly long[] _counts;

        public IndexOfCoincidence(string alphabet)
        {
            _counts = new long[alphabet.Length];
        }

        public double Evaluate(int[] plaintext)
        {
            System.Array.Clear(_counts, 0, _counts.Length);
            foreach (int letter in plaintext)
            {
                _counts[letter]++;
            }

            long value = 0;
            foreach (long count in _counts)
            {
                value += count * (count - 1);
            }

            long n = plaintext.Length;
            return (double)value / (n * (n - 1));
        }
    }
}
