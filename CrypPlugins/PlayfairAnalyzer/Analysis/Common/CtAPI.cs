using System;
using System.Collections.Generic;

namespace PlayfairAnalysis.Common
{
    public class CtAPI
    {
        public Stats Stats { get; }
        public CtBestList CtBestList { get; }

        public delegate void BestResultChangedHandler(CtBestList.Result bestResult);
        public event BestResultChangedHandler BestResultChangedEvent;

        public delegate void BestListChangedHandler(IList<CtBestList.Result> bestList);
        public event BestListChangedHandler BestListChangedEvent;

        public delegate void GoodbyeHandler();
        public event GoodbyeHandler GoodbyeEvent;

        public delegate void ProgressChangedHandler(double currentValue, double maxValue, long evaluations);
        public event ProgressChangedHandler ProgressChangedEvent;

        public CtAPI(AnalysisInstance instance)
        {
            Stats = instance.Stats;
            CtBestList = instance.CtBestList;
        }

        internal void goodbye()
        {
            GoodbyeEvent?.Invoke();
        }

        internal void goodbyeFatalError(string format, params object[] objects)
        {
            GoodbyeEvent?.Invoke();
            throw new Exception(string.Format(format, objects));
        }

        internal void printf(string format, params object[] objects)
        {
            Console.Out.WriteLine(format, objects);
        }

        internal void println(string s)
        {
            Console.Out.WriteLine(s);
        }

        internal void print(string s)
        {
            Console.Out.Write(s);
        }

        internal void shutdownIfNeeded()
        {
        }

        internal void updateProgress(long value, int maxValue, long evaluations)
        {
            if (maxValue <= 0)
            {
                ProgressChangedEvent?.Invoke(value % 100, 100, evaluations);
            }
            else
            {
                ProgressChangedEvent?.Invoke(value, maxValue, evaluations);
            }
        }

        internal void openAndReadInputValues(string attackName, string attackVersion)
        {
        }

        internal void displayBestList(IList<CtBestList.Result> bestList)
        {
            BestListChangedEvent?.Invoke(bestList);
        }

        internal void displayBestResult(CtBestList.Result bestResult)
        {
            BestResultChangedEvent?.Invoke(bestResult);
            Console.Out.WriteLine($"Score: {bestResult.score}");
            Console.Out.WriteLine($"Key: {bestResult.keyString}");
            Console.Out.WriteLine($"Plaintext: {plaintextCapped(bestResult.plaintextString)}");
        }

        internal void displayBestResult(CtBestList.Result bestResult, CtBestList.Result originalResult)
        {
            displayBestResult(bestResult);
        }

        private string plaintextCapped(string plaintext)
        {
            if (plaintext.Length <= 1000)
            {
                return plaintext;
            }
            return plaintext.Substring(0, Math.Min(100, plaintext.Length)) + "...";
        }
    }
}
