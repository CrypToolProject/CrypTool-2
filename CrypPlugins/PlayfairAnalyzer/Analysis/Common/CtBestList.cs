/*
   Copyright 2022 George Lasry, Nils Kopal, CrypTool 2 Team

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using System.Collections.Generic;
using System.Text;
using static PlayfairAnalysis.Common.CtBestList;

namespace PlayfairAnalysis.Common
{
    public class CtBestList
    {
        /// <summary>
        /// Class encapulating a result in best list.
        /// </summary>
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
                Set(score, keyString, keyStringShort, plaintextString, elapsed, evaluations, commentString);
            }

            public void Set(long score, string keyString, string keyStringShort, string plaintextString, TimeSpan elapsed, long evaluations, string commentString)
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

        /// <summary>
        /// Set best list size
        /// </summary>
        /// <param name="size"></param>
        public void SetBestListSize(int size)
        {
            lock (mutex)
            {
                maxNumberOfResults = size;
                Clean();
            }
        }

        /// <summary>
        /// Set a score threshold, below which result will not be included in best list.
        /// </summary>
        /// <param name="scoreThreshold"></param>
        public void SetScoreThreshold(long scoreThreshold)
        {
            lock (mutex)
            {
                this.scoreThreshold = scoreThreshold;
                Clean();
            }
        }

        /// <summary>
        /// If set to yes, ignore results with plaintext already seen (possibly with a different key).
        /// </summary>
        /// <param name="discardSamePlaintexts"></param>
        public void SetDiscardSamePlaintexts(bool discardSamePlaintexts)
        {
            lock (mutex)
            {
                this.discardSamePlaintexts = discardSamePlaintexts;
                Clean();
            }
        }

        /// <summary>
        /// If set to true, best list will be send to CrypTool no more than once every second.
        /// This is useful in case there are many new results, in a short period of time, that would be one of the top best.
        /// This can happen very often in hillclimbing processes which slowly progress.
        /// </summary>
        /// <param name="throttle"></param>
        public void SetThrottle(bool throttle)
        {
            lock (mutex)
            {
                this.throttle = throttle;
                Clean();
            }
        }

        /// <summary>
        /// Clears the best list
        /// </summary>
        public void ClearBestList()
        {
            lock (mutex)
            {
                bestResults.Clear();
                CtAPI.DisplayBestList(new Result[0]);
            }
        }

        /// <summary>
        /// Check whether a new result has a score that would allow it to be added to the best list.
        /// Useful when some processing is required before pushing the result(e.g.formatting the key string). After formatting, then
        /// pushResult should be called to push the result.
        /// </summary>
        /// <param name="score"></param>
        /// <returns></returns>
        public bool ShouldPushResult(long score)
        {
            lock (mutex)
            {
                if (throttle)
                {
                    if (shouldUpdateBestList && DateTime.Now.Ticks - lastBestListUpdateMillis > TimeSpan.FromSeconds(1000).Ticks)
                    {
                        lastBestListUpdateMillis = DateTime.Now.Ticks;
                        shouldUpdateBestList = false;
                        Display();
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

        /// <summary>
        ///  Push a new result to the task list, if its score is highes that the lowest score in the best list.
        ///  Discard duplicate keys(and if the relevant option is set, keyes resulting in an already seen plaintext).
        /// </summary>
        /// <param name="score"></param>
        /// <param name="keyString"></param>
        /// <param name="keyStringShort"></param>
        /// <param name="plaintextString"></param>
        /// <param name="elapsed"></param>
        /// <param name="evaluations"></param>
        /// <param name="commentString"></param>
        /// <returns></returns>
        public bool PushResult(long score, string keyString, string keyStringShort, string plaintextString, TimeSpan elapsed, long evaluations, string commentString)
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
                    bestResults[size - 1].Set(score, keyString, keyStringShort, plaintextString, elapsed, evaluations, commentString);
                }
                else
                {
                    return false;
                }
                Sort();
                if (bestChanged)
                {
                    Result bestResult = bestResults[0];
                    if (originalResult == null)
                    {
                        CtAPI.DisplayBestResult(bestResult);
                    }
                    else
                    {
                        CtAPI.DisplayBestResult(bestResult, originalResult);
                    }
                }
                if (throttle)
                {
                    shouldUpdateBestList = true;
                }
                else
                {
                    Display();
                }
                return true;
            }
        }

        private void Display()
        {
            lock (mutex)
            {
                StringBuilder s = new StringBuilder();
                Sort();
                CtAPI.DisplayBestList(bestResults);
            }
        }

        private void Clean()
        {
            lock (mutex)
            {
                Sort();
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

        private void Sort()
        {
            lock (mutex)
            {
                bestResults.Sort(new ResultComparer());
            }
        }
    }
}