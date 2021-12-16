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
using System.Numerics;
using System.Threading;
using VoluntLib2.Tools;

namespace VoluntLib2.ComputationLayer
{
    /// <summary>
    /// Worker class containing the "work logic"
    /// </summary>
    public abstract class AWorker
    {
        /// <summary>
        /// JobId; is set by VoluntLib
        /// </summary>
        public BigInteger JobId { get; set; }

        /// <summary>
        /// DoWork method; Has to be overwritten by user code
        /// </summary>
        /// <param name="jobPayload"></param>
        /// <param name="blockId"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public abstract CalculationResult DoWork(byte[] jobPayload, BigInteger blockId, CancellationToken cancelToken);

        /// <summary>
        /// ProgessChanged event
        /// </summary>
        public event EventHandler<TaskEventArgs> ProgressChanged;

        /// <summary>
        /// Helper method to invoke ProgessChanged event
        /// </summary>
        /// <param name="blockID"></param>
        /// <param name="progress"></param>
        protected virtual void OnProgressChanged(BigInteger blockID, int progress)
        {
            if (ProgressChanged != null)
            {
                ProgressChanged.Invoke(this, new TaskEventArgs(JobId, blockID, TaskEventArgType.Progress) { TaskProgress = progress });
            }
        }
    }
}
