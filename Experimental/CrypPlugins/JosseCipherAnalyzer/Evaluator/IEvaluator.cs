namespace CrypTool.JosseCipherAnalyzer.Evaluator
{
    public interface IEvaluator
    {
        /// <summary>
        /// Rates a plaintext given as letter numbers (indices into the analyzer alphabet); higher is better.
        /// </summary>
        double Evaluate(int[] plaintext);
    }
}
