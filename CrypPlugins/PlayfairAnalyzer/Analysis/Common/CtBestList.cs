using System;
using System.Collections.Generic;
using System.Text;

namespace PlayfairAnalysis.Common
{
    public class CtBestList
    {
        /**
         * Class encapulating a result in best list.
         */
        public class Result
        {
            public long score;
            public string keyString;
            public string keyStringShort;   // short version of the key
            public string plaintextString;
            public string commentString;
            public TimeSpan elapsed;
            public long evaluations;

            public Result(long score, string keyString, string keyStringShort, string plaintextString, TimeSpan elapsed, long evaluations, string commentString)
            {
                set(score, keyString, keyStringShort, plaintextString, elapsed, evaluations, commentString);
            }

            public void set(long score, string keyString, string keyStringShort, string plaintextString, TimeSpan elapsed, long evaluations, string commentString)
            {
                this.score = score;
                this.keyString = keyString;
                this.keyStringShort = keyStringShort;
                this.plaintextString = plaintextString;
                this.elapsed = elapsed;
                this.evaluations = evaluations;
                this.commentString = commentString;
            }
            public string ToString(int rank)
            {
                return string.Format("{0,2};{1,12:N0};{2};{3};{4}\n", rank, score, keyStringShort, plaintextString, commentString);
            }

        }

        private readonly object mutex = new object();
        private readonly List<Result> bestResults = new List<Result>();
        private readonly Result originalResult = null;
        private long lastBestListUpdateMillis = 0;
        private bool shouldUpdateBestList = false;

        private int maxNumberOfResults = 10;
        private long scoreThreshold = 0;
        private bool discardSamePlaintexts = false;
        private bool throttle = false;

        public CtAPI CtAPI { get; }

        public CtBestList(AnalysisInstance instance)
        {
            CtAPI = instance.CtAPI;
        }

        /**
         * Set best list size.
         * @param size - max number of elements in best list.
         */
        public void setSize(int size)
        {
            lock (mutex)
            {
                maxNumberOfResults = size;
                clean();
            }
        }

        /**
         * Set a score threshold, below which result will not be included in best list.
         * @param scoreThreshold - threshold value
         */
        public void setScoreThreshold(long scoreThreshold)
        {
            lock (mutex)
            {
                this.scoreThreshold = scoreThreshold;
                clean();
            }
        }

        /**
         * If set to yes, ignore results with plaintext already seen (possibly with a different key).
         * @param discardSamePlaintexts
         */
        public void setDiscardSamePlaintexts(bool discardSamePlaintexts)
        {
            lock (mutex)
            {
                this.discardSamePlaintexts = discardSamePlaintexts;
                clean();
            }
        }

        /**
         * If set to true, best list will be send to CrypTool no more than once every second.
         * This is useful in case there are many new results, in a short period of time, that would be one of the top best.
         * This can happen very often in hillclimbing processes which slowly progress.
         * @param throttle - if yes, throttle updates to CrypTool.
         */
        public void setThrottle(bool throttle)
        {
            lock (mutex)
            {
                this.throttle = throttle;
                clean();
            }
        }

        /**
         * Resets the best list.
         */
        public void clear()
        {
            lock (mutex)
            {
                bestResults.Clear();
                CtAPI.displayBestList(new Result[0]);
            }
        }

        /**
         * Check whether a new result has a score that would allow it to be added to the best list.
         * Useful when some processing is required before pushing the result (e.g. formatting the key string). After formatting, then
         * pushResult should be called to push the result.
         * @param score - score of a new result.
         * @return - score is higher than the lower score in the best list.
         */
        public bool shouldPushResult(long score)
        {
            lock (mutex)
            {
                if (throttle)
                {
                    if (shouldUpdateBestList && DateTime.Now.Ticks - lastBestListUpdateMillis > TimeSpan.FromSeconds(1000).Ticks)
                    {
                        lastBestListUpdateMillis = DateTime.Now.Ticks;
                        shouldUpdateBestList = false;
                        display();
                    }
                }

                if (score < scoreThreshold)
                {
                    return false;
                }
                int size = bestResults.Count;
                return size < maxNumberOfResults || score > bestResults[size - 1].score;
            }
        }

        /**
         * Push a new result to the task list, if its score is highes that the lowest score in the best list.
         * Discard duplicate keys (and if the relevant option is set, keyes resulting in an already seen plaintext).
         * @param score
         * @param keyString
         * @param keyStringShort
         * @param plaintextString
         * @param commentString
         * @return
         */
        public bool pushResult(long score, string keyString, string keyStringShort, string plaintextString, TimeSpan elapsed, long evaluations, string commentString)
        {
            lock (mutex)
            {
                if (discardSamePlaintexts)
                {
                    foreach (Result be in bestResults)
                    {
                        if (be.plaintextString == plaintextString)
                        {
                            return false;
                        }
                    }
                }
                foreach (Result be in bestResults)
                {
                    if (be.keyString == keyString)
                    {
                        return false;
                    }
                }
                int size = bestResults.Count;
                bool bestChanged = false;
                if (size == 0 || score > bestResults[0].score)
                {
                    bestChanged = true;
                }
                if (size < maxNumberOfResults)
                {
                    bestResults.Add(new Result(score, keyString, keyStringShort, plaintextString, elapsed, evaluations, commentString));
                }
                else if (score > bestResults[size - 1].score)
                {
                    bestResults[size - 1].set(score, keyString, keyStringShort, plaintextString, elapsed, evaluations, commentString);
                }
                else
                {
                    return false;
                }
                sort();
                if (bestChanged)
                {
                    Result bestResult = bestResults[0];
                    if (originalResult == null)
                    {
                        CtAPI.displayBestResult(bestResult);
                    }
                    else
                    {
                        CtAPI.displayBestResult(bestResult, originalResult);
                    }
                }
                if (throttle)
                {
                    shouldUpdateBestList = true;
                }
                else
                {
                    display();
                }
                return true;
            }
        }

        // Package private.
        private void display()
        {
            lock (mutex)
            {
                StringBuilder s = new StringBuilder();
                sort();
                CtAPI.displayBestList(bestResults);
            }
        }

        // Private.
        private void clean()
        {
            lock (mutex)
            {
                sort();
                while (bestResults.Count > maxNumberOfResults)
                {
                    bestResults.RemoveAt(bestResults.Count - 1);
                }
                while (bestResults.Count > 0 && bestResults[bestResults.Count - 1].score < scoreThreshold)
                {
                    bestResults.RemoveAt(bestResults.Count - 1);
                }
            }
        }

        private class ResultComparer : IComparer<Result>
        {
            public int Compare(Result o1, Result o2)
            {
                return (int)(o2.score - o1.score);
            }
        }

        private void sort()
        {
            lock (mutex)
            {
                bestResults.Sort(new ResultComparer());
            }
        }
    }
}