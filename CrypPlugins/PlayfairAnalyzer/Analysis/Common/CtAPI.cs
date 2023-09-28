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

        internal void Goodbye()
        {
            GoodbyeEvent?.Invoke();
        }

        internal void GoodbyeFatalError(string format, params object[] objects)
        {
            GoodbyeEvent?.Invoke();
            throw new Exception(string.Format(format, objects));
        }

        internal void Printf(string format, params object[] objects)
        {
            Console.Out.WriteLine(format, objects);
        }

        internal void Println(string s)
        {
            Console.Out.WriteLine(s);
        }

        internal void Print(string s)
        {
            Console.Out.Write(s);
        }

        internal void ShutdownIfNeeded()
        {
        }

        internal void UpdateProgress(long value, int maxValue, long evaluations)
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

        internal void DisplayBestList(IList<CtBestList.Result> bestList)
        {
            BestListChangedEvent?.Invoke(bestList);
        }

        internal void DisplayBestResult(CtBestList.Result bestResult)
        {
            BestResultChangedEvent?.Invoke(bestResult);
            Console.Out.WriteLine($"Score: {bestResult.score}");
            Console.Out.WriteLine($"Key: {bestResult.keyString}");
            Console.Out.WriteLine($"Plaintext: {PlaintextCapped(bestResult.plaintextString)}");
        }

        internal void DisplayBestResult(CtBestList.Result bestResult, CtBestList.Result originalResult)
        {
            DisplayBestResult(bestResult);
        }

        private string PlaintextCapped(string plaintext)
        {
            if (plaintext.Length <= 1000)
            {
                return plaintext;
            }
            return plaintext.Substring(0, Math.Min(100, plaintext.Length)) + "...";
        }
    }
}
