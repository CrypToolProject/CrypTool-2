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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using VoluntLib2.ManagementLayer;
using VoluntLib2.Tools;

namespace VoluntLib2.ComputationLayer
{
    /// <summary>
    /// The ComputationManager is responsible for
    /// A) joining and leaving of jobs
    /// B) taking care, that workers are started and stopped
    /// </summary>
    internal class ComputationManager
    {
        private readonly Logger Logger = Logger.GetLogger();
        private bool Running = false;
        private Thread WorkerThread;

        internal ConcurrentDictionary<BigInteger, JobAssignment> JobAssignments = new ConcurrentDictionary<BigInteger, JobAssignment>();
        internal ConcurrentQueue<Operation> Operations = new ConcurrentQueue<Operation>();

        internal VoluntLib VoluntLib { get; set; }
        internal JobManager JobManager { get; set; }

        /// <summary>
        /// Creates a new ComputationManager
        /// </summary>
        /// <param name="voluntLib"></param>
        /// <param name="jobManager"></param>
        public ComputationManager(VoluntLib voluntLib, JobManager jobManager)
        {
            VoluntLib = voluntLib;
            JobManager = jobManager;
        }

        /// <summary>
        /// Start this ComputationManager
        /// </summary>
        public void Start()
        {
            if (Running)
            {
                throw new InvalidOperationException("The ComputationManager is already running!");
            }
            Logger.LogText("Starting the ComputationManager", this, Logtype.Info);

            Running = true;
            WorkerThread = new Thread(ComputationManagerWork)
            {
                Name = "ComputationManagerWorkerThread",
                IsBackground = true
            };
            WorkerThread.Start();
            //This operation deserializes all serialized jobs; then it terminates
            Operations.Enqueue(new CheckRunningWorkersAndJobsOperation() { ComputationManager = this });
            //This operation checks, which jobs are completed. If it finds one, it fires the JobFinished event of VoluntLib2
            Operations.Enqueue(new CheckJobsCompletionState() { ComputationManager = this });

            Logger.LogText("ComputationManager started", this, Logtype.Info);
        }

        /// <summary>
        /// Main method of the thread of the ComputationManager
        /// </summary>
        private void ComputationManagerWork(object obj)
        {
            Logger.LogText("WorkerThread started", this, Logtype.Info);
            while (Running)
            {
                try
                {
                    if (Operations.TryDequeue(out Operation operation) == true)
                    {
                        // before we execute an operation, we check if it is finished
                        if (operation.IsFinished == false)
                        {
                            //operations that are not finished are enqueued again
                            Operations.Enqueue(operation);
                        }
                        else
                        {
                            Logger.LogText(string.Format("Operation {0}-{1} has finished. Removed it.", operation.GetType().FullName, operation.GetHashCode()), this, Logtype.Debug);
                            //we dont execute this operation since it is finished
                            continue;
                        }
                        try
                        {
                            operation.Execute();
                        }
                        catch (Exception ex)
                        {
                            Logger.LogText(string.Format("Exception during execution of operation {0}-{1}: {2}", operation.GetType().FullName, operation.GetHashCode(), ex.Message), this, Logtype.Error);
                            Logger.LogException(ex, this, Logtype.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogText(string.Format("Exception during handling of operation: {0}", ex.Message), this, Logtype.Error);
                    Logger.LogException(ex, this, Logtype.Error);
                }
                try
                {
                    Thread.Sleep(Constants.COMPUTATIONMANAGER_WORKER_THREAD_SLEEPTIME);
                }
                catch (Exception ex)
                {
                    Logger.LogText(string.Format("Exception during sleep of thread: {0}", ex.Message), this, Logtype.Error);
                    Logger.LogException(ex, this, Logtype.Error);
                }
            }
            Logger.LogText("WorkerThread terminated", this, Logtype.Info);
        }

        /// <summary>
        /// Stop this ComputationManager
        /// </summary>
        public void Stop()
        {
            if (!Running)
            {
                return;
            }
            Logger.LogText("Stop method was called...", this, Logtype.Info);
            Running = false;
            DateTime start = DateTime.Now;
            while ((WorkerThread.IsAlive) && DateTime.Now < start.AddMilliseconds(Constants.COMPUTATIONMANAGER_MAX_TERMINATION_WAIT_TIME))
            {
                Thread.Sleep(100);
            }
            if (WorkerThread.IsAlive)
            {
                Logger.LogText("WorkerThread did not end within 5 seconds", this, Logtype.Info);
                try
                {
                    WorkerThread.Abort();
                    Logger.LogText("Aborted WorkerThread", this, Logtype.Info);
                }
                catch (Exception ex)
                {
                    Logger.LogText(string.Format("Exception during abortion of WorkerThread: {0}", ex.Message), this, Logtype.Error);
                    Logger.LogException(ex, this, Logtype.Error);
                }
            }
            Logger.LogText("Terminated", this, Logtype.Info);
        }

        /// <summary>
        /// Joins the job with the given jobId, calculation template, and amount of workers
        /// If VoluntLib is stopped, it does nothing
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="calculationTemplate"></param>
        /// <param name="amountOfWorkers"></param>
        /// <returns></returns>
        internal bool JoinJob(BigInteger jobId, ACalculationTemplate calculationTemplate, int amountOfWorkers)
        {
            if (!Running)
            {
                return false;
            }

            if (!JobAssignments.ContainsKey(jobId))
            {
                if (!JobManager.Jobs.ContainsKey(jobId))
                {
                    Logger.LogText(string.Format("Can not join a non existing job with jobid {0}", BitConverter.ToString(jobId.ToByteArray())), this, Logtype.Warning);
                    return false;
                }
                Job job = JobManager.Jobs[jobId];
                if (!job.HasPayload)
                {
                    Logger.LogText(string.Format("Can not join job with jobid {0} since we have no JobPayload", BitConverter.ToString(jobId.ToByteArray())), this, Logtype.Warning);
                    return false;
                }

                try
                {
                    if (!JobAssignments.TryAdd(jobId, new JobAssignment() { Job = job, CalculationTemplate = calculationTemplate, AmountOfWorkers = amountOfWorkers }))
                    {
                        Logger.LogText(string.Format("Could not add job with jobid {0} to internal JobAssignments dictionary", BitConverter.ToString(jobId.ToByteArray())), this, Logtype.Warning);
                        return false;
                    }
                    if (job.JobEpochState != null)
                    {
                        VoluntLib.OnJobProgress(this, new JobProgressEventArgs(job.JobId, job.JobEpochState.ResultList.ToList(), job.NumberOfBlocks, job.NumberOfCalculatedBlocks));
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.LogText(string.Format("Could not add job with jobid {0} to internal JobAssignments dictionary", BitConverter.ToString(jobId.ToByteArray())), this, Logtype.Warning);
                    Logger.LogException(ex, this, Logtype.Error);
                }
            }
            return false;
        }

        /// <summary>
        /// Sops the job with the given jobId if it exists
        /// </summary>
        /// <param name="jobId"></param>
        internal void StopJob(BigInteger jobId)
        {
            if (JobAssignments.ContainsKey(jobId))
            {
                try
                {
                    if (!JobAssignments.TryRemove(jobId, out JobAssignment jobassignment))
                    {
                        Logger.LogText(string.Format("Could not remove job with jobid {0} from internal JobAssignments dictionary!", BitConverter.ToString(jobId.ToByteArray())), this, Logtype.Warning);
                    }
                    foreach (Worker worker in jobassignment.Workers)
                    {
                        if (worker.CancellationToken.CanBeCanceled)
                        {
                            worker.CancellationTokenSource.Cancel();
                        }
                        VoluntLib.OnTaskStopped(this, new TaskEventArgs(worker.Job.JobId, worker.BlockId, TaskEventArgType.Finished));
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogText(string.Format("Could not remove job with jobid {0} from internal JobAssignments dictionary!", BitConverter.ToString(jobId.ToByteArray())), this, Logtype.Error);
                    Logger.LogException(ex, this, Logtype.Error);
                }
            }
        }

        /// <summary>
        /// Returns a dictionary containing the nummber of current running workers per job
        /// </summary>
        /// <returns></returns>
        public Dictionary<BigInteger, int> GetCurrentRunningWorkersPerJob()
        {
            Dictionary<BigInteger, int> dict = new Dictionary<BigInteger, int>();
            try
            {
                foreach (Job job in new List<Job>(JobManager.JobList))
                {
                    if (JobAssignments.ContainsKey(job.JobId))
                    {
                        dict.Add(job.JobId, JobAssignments[job.JobId].Workers.Count);
                    }
                    else
                    {
                        dict.Add(job.JobId, 0);
                    }
                }
            }
            catch (Exception)
            {
                //not so important;
                //if an exception occurs, we only deliver the so-far created dictionary
            }
            return dict;
        }
    }

    /// <summary>
    /// A job assignment contains information of a joined job.
    /// The ComputationManager takes care, that always AmountOfWorkers workers are running
    /// </summary>
    internal class JobAssignment
    {
        public Job Job { get; set; }
        public ACalculationTemplate CalculationTemplate { get; set; }
        public int AmountOfWorkers { get; set; }
        public ArrayList Workers = ArrayList.Synchronized(new ArrayList());
    }

    /// <summary>
    /// A worker knows its worker thread and a CancellationToken to stop the worker 
    /// </summary>
    internal class Worker
    {
        private readonly Logger Logger = Logger.GetLogger();
        internal ACalculationTemplate ACalculationTemplate { get; set; }
        internal Job Job { get; set; }
        internal AWorker AWorker { get; set; }
        internal VoluntLib VoluntLib { get; set; }
        public Thread WorkerThread { get; private set; }

        /// <summary>
        /// Creates a new worker object
        /// </summary>
        /// <param name="job"></param>
        /// <param name="template"></param>
        /// <param name="voluntLib"></param>
        public Worker(Job job, ACalculationTemplate template, VoluntLib voluntLib)
        {
            Job = job;
            ACalculationTemplate = template;
            VoluntLib = voluntLib;
            AWorker = ACalculationTemplate.WorkerLogic;
            AWorker.JobId = Job.JobId;
            CancellationTokenSource = new CancellationTokenSource();
            CancellationToken = CancellationTokenSource.Token;
            AWorker.ProgressChanged += OnProgressChanged;
        }

        /// <summary>
        /// Helper method to invoke ProgressChanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="taskEventArgs"></param>
        private void OnProgressChanged(object sender, TaskEventArgs taskEventArgs)
        {
            //we are only responsible for progress changes of our own block id
            if (taskEventArgs.BlockID.Equals(BlockId))
            {
                VoluntLib.OnTaskProgessChanged(sender, taskEventArgs);
            }
        }

        /// <summary>
        /// Token to cancel the worker thread
        /// </summary>
        public CancellationToken CancellationToken { get; private set; }

        /// <summary>
        /// TokenSource to cancel the worker thread
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; private set; }

        /// <summary>
        /// BlockId this worker is working on
        /// </summary>
        public BigInteger BlockId { get; set; }

        /// <summary>
        /// CalculationResult is filled after termination of worker; it is null if the worker was killed
        /// </summary>
        public CalculationResult CalculationResult { get; set; }

        /// <summary>
        /// Internal work method of thread of this worker
        /// </summary>
        internal void DoWork()
        {
            try
            {
                CalculationResult = AWorker.DoWork(Job.JobPayload, BlockId, CancellationToken);
                Logger.LogText(string.Format("Worker-{0} who worked on block {1} of job {2} terminated after complete computation", GetHashCode(), BlockId, BitConverter.ToString(Job.JobId.ToByteArray())), this, Logtype.Info);
                VoluntLib.OnTaskStopped(this, new TaskEventArgs(Job.JobId, BlockId, TaskEventArgType.Finished));
            }
            catch (OperationCanceledException)
            {
                Logger.LogText(string.Format("Worker-{0} who worked on block {1} job {2} was stopped by CancellationToken", GetHashCode(), BlockId, BitConverter.ToString(Job.JobId.ToByteArray())), this, Logtype.Info);
                VoluntLib.OnTaskStopped(this, new TaskEventArgs(Job.JobId, BlockId, TaskEventArgType.Canceled));
            }
            catch (Exception ex)
            {
                Logger.LogText(string.Format("Exception during execution of Worker-{0} who worked on block {1} job {2} : {3}", GetHashCode(), BlockId, BitConverter.ToString(Job.JobId.ToByteArray()), ex.Message), this, Logtype.Error);
                Logger.LogException(ex, this, Logtype.Error);
            }
            AWorker.ProgressChanged -= OnProgressChanged;
        }

        /// <summary>
        /// Start the worker on the block defined by given blockid
        /// </summary>
        /// <param name="blockId"></param>
        internal void Start(BigInteger blockId)
        {
            BlockId = blockId;
            WorkerThread = new Thread(DoWork)
            {
                Name = "JobWorkerThread-" + blockId,
                IsBackground = true
            };
            WorkerThread.Start();
            Logger.LogText(string.Format("Started Worker-{0} on block {1} of job {2}", GetHashCode(), BlockId, BitConverter.ToString(Job.JobId.ToByteArray())), this, Logtype.Info);
        }
    }
}
