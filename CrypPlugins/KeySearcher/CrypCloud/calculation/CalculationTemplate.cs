/*
   Copyright (C) CrypTool 2 Team

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
using KeySearcher.CrypCloud;
using KeySearcher.KeyPattern;
using System.Collections.Generic;
using System.Linq;
using VoluntLib2.ComputationLayer;

namespace KeySearcher
{
    internal class CalculationTemplate : ACalculationTemplate<KeyResultEntry>
    {
        private readonly bool sortAscending;

        public CalculationTemplate(JobDataContainer jobData, KeyPattern.KeyPattern pattern, bool sortAscending, KeySearcher keysearcher)
        {
            this.sortAscending = sortAscending;
            System.Numerics.BigInteger keysPerChunk = pattern.size() / jobData.NumberOfBlocks;
            KeyPatternPool keyPool = new KeyPatternPool(pattern, keysPerChunk);
            WorkerLogic = new Worker(jobData, keyPool, keysearcher);
        }


        public override List<KeyResultEntry> MergeResults(IEnumerable<KeyResultEntry> oldResultList, IEnumerable<KeyResultEntry> newResultList)
        {
            IEnumerable<KeyResultEntry> results = newResultList
                .Concat(oldResultList)
                .Distinct();

            results = sortAscending
                ? results.OrderBy(it => it)
                : results.OrderByDescending(it => it);

            return results.Take(10).ToList();
        }
    }
}