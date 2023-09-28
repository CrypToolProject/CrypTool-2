/*
   Copyright CrypTool 2 Team josef.matwich@gmail.com

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
using M209AnalyzerLib.M209;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace M209AnalyzerLib.Common
{
    /// <summary>
    /// Class encapulating a result in best list.
    /// </summary>
    public class CtBestList
    {
        #region EventHandling
        public static event EventHandler<OnNewResultEventArgs> OnNewResult;
        public class OnNewResultEventArgs : EventArgs
        {
            public BestListResult Result { get; set; }

            public OnNewResultEventArgs(BestListResult result)
            {
                Result = result;
            }
        }
        #endregion

        public static bool Silent { get; set; } = false;

        private List<BestListResult> BestResults = new List<BestListResult>();
        public BestListResult BestResult = null;
        private long LastBestListUpdateMillis = 0;
        private bool ShouldUpdateBestList = false;

        private int MaxNumberOfResults = 10;
        private long ScoreThreshold = 0;
        private bool DiscardSamePlaintexts = false;
        private bool Throttle = false;

        private Object LOCK = new Object();


        /// <summary>
        /// Set best list size.
        /// </summary>
        /// <param name="size">max number of elements in best list</param>
        public void SetSize(int size)
        {
            lock (LOCK)
            {
                MaxNumberOfResults = size;
                Clean();
            }
        }

        /// <summary>
        /// Set a score threshold, below which result will not be included in best list.
        /// </summary>
        /// <param name="scoreThreshold">threshold value</param>
        public void SetScoreThreshold(long scoreThreshold)
        {
            lock (LOCK)
            {
                ScoreThreshold = scoreThreshold;
                Clean();
            }
        }

        /// <summary>
        /// If set to yes, ignore results with plaintext already seen (possibly with a different key).
        /// </summary>
        /// <param name="discardSamePlaintexts"></param>
        public void SetDiscardSamePlaintexts(bool discardSamePlaintexts)
        {
            lock (LOCK)
            {
                DiscardSamePlaintexts = discardSamePlaintexts;
                Clean();
            }
        }

        /// <summary>
        /// If set to true, best list will be send to Cryptool no more than once every second.
        /// This is useful in case there are many new results, in a short period of time, that would be one of the top best.
        /// This can happen very often in hillclimbing processes which slowly progress.
        /// </summary>
        /// <param name="throttle">if yes, throttle updates to Cryptool.</param>
        public void SetThrottle(bool throttle)
        {
            lock (LOCK)
            {
                Throttle = throttle;
                Clean();
            }
        }

        /// <summary>
        /// Resets the best list.
        /// </summary>
        public void Clear()
        {
            lock (LOCK)
            {
                BestResults.Clear();
            }
        }

        /// <summary>
        /// Check whether a new result has a score that would allow it to be added to the best list.
        /// Useful when some processing is required before pushing the result (e.g formattin the key string). After formatting, then
        /// pushResult should be called to push the result.
        /// </summary>
        /// <param name="score">score of a new result</param>
        /// <returns>score is higher than the lower score in the best list</returns>
        public bool ShouldPushResult(double score)
        {
            lock (LOCK)
            {
                if (Throttle)
                {
                    if (ShouldUpdateBestList && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - LastBestListUpdateMillis > 1000)
                    {
                        LastBestListUpdateMillis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        ShouldUpdateBestList = false;
                        Display();
                    }
                }

                if (score < ScoreThreshold)
                {
                    return false;
                }
                int size = BestResults.Count();
                return size < MaxNumberOfResults || score > BestResults.ElementAt(size - 1).Score;
            }
        }

        /// <summary>
        /// Push a new result to the task list, if its score is highes that the lowest score in the best list.
        /// Discard duplicate keys (and if the relevant option is set, keyes resulting in an already seen plaintext).
        /// </summary>
        /// <param name="score"></param>
        /// <param name="key"></param>
        /// <param name="decryptedTextArray"></param>
        /// <returns></returns>
        public bool PushResult(double score, Key key, int[] decryptedTextArray)
        {
            lock (LOCK)
            {
                string decryptedText = Utils.GetString(decryptedTextArray);
                if (DiscardSamePlaintexts)
                {
                    foreach (BestListResult bestResult in BestResults)
                    {
                        if (bestResult.PlaintextString.Equals(decryptedText))
                        {
                            return false;
                        }
                    }
                }

                foreach (BestListResult bestResult in BestResults)
                {
                    if (bestResult.KeyString.Equals(key.ToString()))
                    {
                        return false;
                    }
                }

                int size = BestResults.Count();
                bool bestChanged = false;
                BestListResult result = null;
                if (size == 0 || score > BestResults.ElementAt(0).Score)
                {
                    bestChanged = true;
                    result = new BestListResult(score, key, decryptedText);
                    OnNewResult?.Invoke(null, new OnNewResultEventArgs(result));
                }
                if (size < MaxNumberOfResults)
                {
                    BestResults.Add(new BestListResult(score, key, decryptedText));
                }
                else if (score > BestResults.ElementAt(size - 1).Score)
                {
                    BestResults.ElementAt(size - 1).Set(score, key, decryptedText);
                }
                else
                {
                    return false;
                }
                Sort();
                if (bestChanged)
                {
                    BestResult = BestResults.ElementAt(0);
                }
                if (Throttle)
                {
                    ShouldUpdateBestList = true;
                }
                else
                {
                    Display();
                }
                return true;
            }
        }

        // Package private.
        private void Display()
        {
            lock (LOCK)
            {
                if (Silent)
                {
                    return;
                }
                else
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    Sort();
                    for (int i = 0; i < BestResults.Count(); i++)
                    {
                        stringBuilder.Append(BestResults.ElementAt(i).ToString(i + 1));
                    }
                    //CtAPI.displayBestList(s.toString());
                    Console.WriteLine(stringBuilder.ToString());
                }
            }
        }

        // Private.
        private void Clean()
        {
            lock (LOCK)
            {
                Sort();
                while (BestResults.Count() > MaxNumberOfResults)
                {
                    BestResults.RemoveAt(BestResults.Count() - 1);
                }
                while (!(BestResults.Count() == 0) && BestResults.ElementAt(BestResults.Count() - 1).Score < ScoreThreshold)
                {
                    BestResults.RemoveAt(BestResults.Count() - 1);
                }
            }
        }
        private void Sort()
        {
            lock (LOCK)
            {
                BestResults.Sort(delegate (BestListResult o1, BestListResult o2)
                {
                    return (int)(o2.Score - o1.Score);
                });
            }
        }
    }
}
