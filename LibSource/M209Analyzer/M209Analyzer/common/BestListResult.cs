using M209AnalyzerLib.M209;

namespace M209AnalyzerLib.Common
{
    public class BestListResult
    {
        public double Score { get; set; }
        public Key Key { get; set; }

        public string KeyString { get; set; }
        public string PlaintextString { get; set; }
        public BestListResult(double score, Key key, string plaintextString)
        {
            Set(score, key, plaintextString);
        }

        public void Set(double score, Key key, string plaintextString)
        {
            Score = score;
            Key = key;
            PlaintextString = plaintextString;
            KeyString = key.ToString();
        }
        public string ToString(int rank)
        {
            return $"{rank};[{Score}] ->{PlaintextString}\n";
        }
    }
}
