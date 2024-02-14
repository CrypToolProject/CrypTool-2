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
using LanguageStatisticsLib;
using System;

namespace PlayfairAnalysis.Common
{
    public class Stats
    {
        private readonly AnalysisInstance _instance;
        private readonly Utils _utils;
        public long Evaluations { get; private set; }

        public Grams Grams { get; private set; }

        public Stats(AnalysisInstance instance, Grams grams)
        {
            _instance = instance;
            _utils = new Utils(0);
            Grams = grams;
            Evaluations = 0;
        }

        public long EvalPlaintext(int[] plaintext, int plaintextLength)
        {
            _instance.CtAPI.ShutdownIfNeeded();
            Evaluations++;
            return (long)Grams.CalculateCost(plaintext);                        
        }

        public (TimeSpan elapsed, long evaluations) EvaluationsSummary()
        {
            TimeSpan elapsed = _utils.GetElapsed();
            return (elapsed, Evaluations);            
        }
    }
}
