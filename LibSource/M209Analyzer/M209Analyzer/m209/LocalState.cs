namespace M209AnalyzerLib.M209
{
    public class LocalState
    {
        public double BestScore { get; set; }
        public bool Improved { get; set; }
        public int[] BestTypeCount { get; set; }
        public bool[][] BestPins { get; set; }
        public int ThreadNr { get; set; }
        public int CurrentCycle { get; set; } = 0;

        public int Restarts { get; set; }
        public bool SingleIteration { get; set; }
        public bool Quick { get; set; }

        public LocalState(int threadNr)
        {
            ThreadNr = threadNr;
        }
    }
}
