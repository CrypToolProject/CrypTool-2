using CrypTool.JosseCipherAnalyzer.Evaluator;
using System;

namespace CrypTool.JosseCipherAnalyzer.AttackTypes
{
    public abstract class AttackType
    {
        public IEvaluator Evaluator { get; set; }
        public abstract ResultEntry Start(string cipherTextInput);
        public bool ShouldStop { get; set; }
        public JosseCipherAnalyzerPresentation Presentation { get; set; }
        public event EventHandler<double> ProcessChanged;
        protected const int MaxBestListEntries = 100;

        protected virtual void OnProcessChanged(double e)
        {
            ProcessChanged?.Invoke(this, e);
        }
    }
}