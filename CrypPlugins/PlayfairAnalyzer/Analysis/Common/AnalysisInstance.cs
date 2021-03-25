using System;
using System.Threading;

namespace PlayfairAnalysis.Common
{
    public class AnalysisInstance
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public CtAPI CtAPI { get; }
        public CtBestList CtBestList { get; }
        public Stats Stats { get; }

        public CancellationToken CancellationToken => cts.Token;

        public AnalysisInstance(bool discardSamePlaintexts)
        {
            CtAPI = new CtAPI(this);
            CtBestList = new CtBestList(this);
            Stats = new Stats(this);

            CtAPI.GoodbyeEvent += () =>
            {
                cts?.Cancel();
            };

            CtBestList.clear();
            CtBestList.setScoreThreshold(0);
            CtBestList.setDiscardSamePlaintexts(discardSamePlaintexts);
            CtBestList.setSize(10);
            CtBestList.setThrottle(false);
        }

        public void Cancel()
        {
            cts.Cancel();
        }
    }
}
