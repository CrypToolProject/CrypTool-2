/*
   Copyright 2018 Nils Kopal <Nils.Kopal<AT>Uni-Kassel.de>

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
using System.Linq;
using VoluntLib2.Tools;

namespace VoluntLib2.ComputationLayer
{

    /// <summary>
    /// Abstract super class for calculations.
    /// Contains a worker logic and a merge function
    /// </summary>
    public abstract class ACalculationTemplate
    {
        /// <summary>
        /// Worker logic
        /// </summary>
        public AWorker WorkerLogic { get; protected set; }

        /// <summary>
        /// Merge method
        /// </summary>
        /// <param name="oldResultList"></param>
        /// <param name="newResultList"></param>
        /// <returns></returns>
        public abstract List<byte[]> MergeResults(IEnumerable<byte[]> oldResultList, IEnumerable<byte[]> newResultList);
    }

    /// <summary>
    /// Abstract generic super class for calculations.
    /// Contains a worker logic and a merge function
    /// </summary>
    public abstract class ACalculationTemplate<T> : ACalculationTemplate where T : IVoluntLibSerializable, new()
    {
        /// <summary>
        /// Merge method
        /// </summary>
        /// <param name="oldResultList"></param>
        /// <param name="newResultList"></param>
        /// <returns></returns>
        public override List<byte[]> MergeResults(IEnumerable<byte[]> oldResultList, IEnumerable<byte[]> newResultList)
        {
            Func<byte[], T> byteToT = entry =>
            {
                T t = new T();
                t.Deserialize(entry);
                return t;
            };

            IEnumerable<T> oldResultsAsT = oldResultList.Select(byteToT);
            IEnumerable<T> newResultsAsT = newResultList.Select(byteToT);

            List<T> mergeResultsAsT = MergeResults(oldResultsAsT, newResultsAsT);

            List<byte[]> mergeResults = mergeResultsAsT.Select(entry => entry.Serialize()).ToList();
            return mergeResults;
        }

        /// <summary>
        /// Generic merge method
        /// </summary>
        /// <param name="oldResultList"></param>
        /// <param name="newResultList"></param>
        /// <returns></returns>
        public abstract List<T> MergeResults(IEnumerable<T> oldResultList, IEnumerable<T> newResultList);

    }
}
