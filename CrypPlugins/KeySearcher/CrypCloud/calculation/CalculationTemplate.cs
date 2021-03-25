using System.Collections.Generic;
using System.Linq;
using KeySearcher.CrypCloud;
using KeySearcher.KeyPattern;
using VoluntLib2.ComputationLayer;

namespace KeySearcher
{
    internal class CalculationTemplate : ACalculationTemplate<KeyResultEntry>
    {
        private readonly bool sortAscending;

        public CalculationTemplate(JobDataContainer jobData, KeyPattern.KeyPattern pattern, bool sortAscending, KeySearcher keysearcher, bool enableOpenCL, int openCLDevice)
        {
            this.sortAscending = sortAscending;
            var keysPerChunk = pattern.size() / jobData.NumberOfBlocks;
            var keyPool = new KeyPatternPool(pattern, keysPerChunk);
            WorkerLogic = new Worker(jobData, keyPool, keysearcher, enableOpenCL, openCLDevice);
        }


        public override List<KeyResultEntry> MergeResults(IEnumerable<KeyResultEntry> oldResultList, IEnumerable<KeyResultEntry> newResultList)
        {
            var results = newResultList
                .Concat(oldResultList) 
                .Distinct(); 

            results = sortAscending 
                ? results.OrderBy(it => it) 
                : results.OrderByDescending(it => it); 

            return results.Take(10).ToList();
        }
    }
}