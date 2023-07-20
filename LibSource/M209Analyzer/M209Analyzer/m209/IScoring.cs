using M209AnalyzerLib.Enums;

namespace M209AnalyzerLib.M209
{
    public interface IScoring
    {
        double Evaluate(EvalType evalType, int[] decryptedText, int[] crib);
    }
}
