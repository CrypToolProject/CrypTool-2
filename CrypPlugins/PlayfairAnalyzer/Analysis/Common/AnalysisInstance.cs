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
using System.Threading;
using CrypTool.PluginBase.Utils;

namespace PlayfairAnalysis.Common
{
    public class AnalysisInstance
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public CtAPI CtAPI { get; }
        public CtBestList CtBestList { get; }
        public Stats Stats { get; }

        public CancellationToken CancellationToken => cts.Token;

        public AnalysisInstance(bool discardSamePlaintexts, Grams grams)
        {
            CtAPI = new CtAPI(this);
            CtBestList = new CtBestList(this);
            Stats = new Stats(this, grams);

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
