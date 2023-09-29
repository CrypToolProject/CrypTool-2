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