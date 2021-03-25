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
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using VoluntLib2.ComputationLayer;
using VoluntLib2.ManagementLayer.Messages;
using VoluntLib2.Tools;

namespace VoluntLib2.ManagementLayer
{
    /// <summary>
    /// Abstract super class of all operations:
    /// Operations are state machines that send messages and work on received message using the JobManager
    /// If an operation's IsFinished equals true, it can be deleted by JobManager
    /// </summary>
    internal abstract class Operation
    {
        /// <summary>
        /// Needed by each operation for message sending, etc
        /// </summary>
        public JobManager JobManager { get; set; }

        /// <summary>
        /// Tells the worker thread if this operation is finished. If it is, it can be deleted
        /// </summary>
        public abstract bool IsFinished { get; }

        /// <summary>
        /// Called by the worker thread (cooperative multitasking)
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// Each message is handed by the JobManager to this operation calling this method
        /// </summary>
        /// <param name="message"></param>
        public abstract void HandleMessage(Message message);
    }

    /// <summary>
    /// Operation for sharing JobLists and Jobs every 5 minutes
    /// </summary>
    internal class ShareJobListAndJobsOperation : Operation
    {
        private Logger Logger = Logger.GetLogger();        
        private DateTime LastExecutionTime = DateTime.MinValue;

        /// <summary>
        /// The ShareJobListOperation never finishes
        /// </summary>
        public override bool IsFinished
        {
            get { return false; }
        }

        /// <summary>
        /// Execute this operation
        /// </summary>
        public override void Execute()
        {
            if (DateTime.Now > LastExecutionTime.AddMilliseconds(Constants.SHAREJOBLISTANDJOBSOPERATION_SHARE_INTERVAL))
            {
                Logger.LogText("Sending ResponseJobListMessages to all neighbors", this, Logtype.Debug);
                //Send a ResponseJobListMessage to every neighbor
                JobManager.SendResponseJobListMessages(null);
                //Send every job to every neighbor
                foreach (Job job in JobManager.JobList)
                {
                    JobManager.SendResponseJobMessage(null, job);
                    Logger.LogText(String.Format("Sending job with id {0} to all neighbors",BitConverter.ToString(job.JobId.ToByteArray())), this, Logtype.Debug);
                }                
                LastExecutionTime = DateTime.Now;
            }
            
        }

        /// <summary>
        /// Handles an incoming Message
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {
            //this operation does nothing with messages
        }
    }

    /// <summary>
    /// Operation for requesting JobLists every minute
    /// </summary>
    internal class RequestJobListOperation : Operation
    {
        private Logger Logger = Logger.GetLogger();               
        private DateTime LastExecutionTime = DateTime.Now;

        /// <summary>
        /// The RequestJobListOperation never finishes
        /// </summary>
        public override bool IsFinished
        {
            get { return false; }
        }

        /// <summary>
        /// Execute this operation
        /// </summary>
        public override void Execute()
        {
            if (DateTime.Now > LastExecutionTime.AddMilliseconds(Constants.REQUESTJOBLISTOPERATION_REQUEST_INTERVAL))
            {
                Logger.LogText("Sending RequestJobListMessages to all neighbors", this, Logtype.Debug);
                //Send a RequestJobListMessage to every neighbor
                JobManager.SendRequestJobListMessage(null);
                LastExecutionTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Handles an incoming Message
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {
            //this operation does nothing with messages
        }

        /// <summary>
        /// Sets the last execution time to min value forcing it to be executed
        /// </summary>
        public void ForceExecution()
        {
            LastExecutionTime = DateTime.MinValue;
        }
    }

    /// <summary>
    /// Operation for answering RequestJobListMessages
    /// </summary>
    internal class ResponseJobListOperation : Operation
    {
        private Logger Logger = Logger.GetLogger();

        /// <summary>
        /// The ResponseJobListOperation never finishes
        /// </summary>
        public override bool IsFinished
        {
            get { return false; }
        }

        /// <summary>
        /// Execute this operation
        /// </summary>
        public override void Execute()
        {
        }

        /// <summary>
        /// Handles an incoming Message
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {
            if (message is RequestJobListMessage)
            {
                Logger.LogText(String.Format("Received a RequestJobListMessage from peer {0}. Answering now.", BitConverter.ToString(message.PeerId)), this, Logtype.Debug);
                JobManager.SendResponseJobListMessages(message.PeerId);
                foreach(Job job in JobManager.JobList)
                {
                    JobManager.SendResponseJobMessage(message.PeerId, job);
                }
            }
        }
    }

    /// <summary>
    /// Operation for handling ResponseJobListMessage
    /// </summary>
    internal class HandleResponseJobListMessageOperation : Operation
    {
        private Logger Logger = Logger.GetLogger();

        /// <summary>
        /// The HandleJobListResponseOperation never finishes
        /// </summary>
        public override bool IsFinished
        {
            get { return false; }
        }

        /// <summary>
        /// Execute this operation
        /// </summary>
        public override void Execute()
        {
        }

        /// <summary>
        /// Handles an incoming Message
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {
            if (message is ResponseJobListMessage)
            {
                Logger.LogText(String.Format("Received a ResponseJobListMessage from peer {0}. Updating my jobs", BitConverter.ToString(message.PeerId)), this, Logtype.Debug);
                ResponseJobListMessage responseJobListMessage = (ResponseJobListMessage)message;
                bool jobListChanged = false;
                foreach (var job in responseJobListMessage.Jobs)
                {
                    if (!job.HasValidCreatorSignature)
                    {
                        Logger.LogText(String.Format("Received job {0} with invalid creator signature from {1} ", BitConverter.ToString(job.JobId.ToByteArray()), BitConverter.ToString(message.PeerId)), this, Logtype.Warning);
                        continue;
                    }

                    if (job.HasValidDeletionSignature)
                    {                        
                        job.IsDeleted = true;
                    }
                    else
                    {
                        job.IsDeleted = false;
                        job.JobDeletionSignatureData = new byte[0];
                    }

                    //we dont know the job, then just add it
                    if (!JobManager.Jobs.ContainsKey(job.JobId))
                    {
                        Logger.LogText(String.Format("Added new job {0} to our job list", BitConverter.ToString(job.JobId.ToByteArray())), this, Logtype.Debug);
                        JobManager.Jobs.TryAdd(job.JobId, job);
                        jobListChanged = true;
                        continue;
                    }
                    else
                    {
                        //Check, if the job is deleted and our local not. if yes, delete our local, too
                        if (JobManager.Jobs[job.JobId].IsDeleted == false && job.IsDeleted == true)
                        {
                            Logger.LogText(String.Format("Received job {0} has a valid deletion signature. Mark it as deleted now", BitConverter.ToString(job.JobId.ToByteArray())), this, Logtype.Debug);
                            JobManager.Jobs[job.JobId].IsDeleted = true;
                            JobManager.Jobs[job.JobId].JobDeletionSignatureData = job.JobDeletionSignatureData;
                            jobListChanged = true;
                        }
                    }                   
                }
                //we received at least one new job. Thus, we inform that the job list changed
                if (jobListChanged)
                {
                    JobManager.OnJobListChanged();
                }
            }
        }
    }

    /// <summary>
    /// This message handles ResponseJobMessages. It adds received payloads and delete messages to our jobs
    /// </summary>
    internal class HandleResponseJobMessageOperation : Operation
    {
        private Logger Logger = Logger.GetLogger();

        /// <summary>
        /// HandleResponseJobMessageOperation never finishes
        /// </summary>
        public override bool IsFinished
        {
            get { return false; }
        }

        /// <summary>
        /// Does nothing
        /// </summary>
        public override void Execute()
        {
        }

        /// <summary>
        /// Handles the receving of a ResponseJobMessage
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {
            if (message is ResponseJobMessage)
            {
                Job job = ((ResponseJobMessage)message).Job;

                //We don't know the job. Thus, we add it
                if (!JobManager.Jobs.ContainsKey(job.JobId))
                {
                    if (!job.HasValidCreatorSignature)
                    {
                        Logger.LogText(String.Format("Received job {0} with invalid Creator Signature from {1}. Ignore it", BitConverter.ToString(job.JobId.ToByteArray()), message.PeerId), this, Logtype.Warning);
                        return;
                    }
                    if (job.HasValidDeletionSignature)
                    {
                        Logger.LogText(String.Format("Received job {0} has a valid deletion signature", BitConverter.ToString(job.JobId.ToByteArray())), this, Logtype.Warning);
                        job.IsDeleted = true;
                    }
                    else
                    {
                        Logger.LogText(String.Format("Received job {0} has an invalid deletion signature. Remove it", BitConverter.ToString(job.JobId.ToByteArray())), this, Logtype.Warning);
                        job.JobDeletionSignatureData = new byte[0];
                        job.IsDeleted = false;
                    }

                    if (job.HasPayload)
                    {
                        //check, if the hash of the payload is ok
                        var jobPayloadHash = CertificateService.GetCertificateService().ComputeHash(job.JobPayload);
                        bool validPayload = true;
                        for (int i = 0; i < jobPayloadHash.Length; i++)
                        {
                            if (jobPayloadHash[i] != job.JobPayloadHash[i])
                            {
                                validPayload = false;
                                break;
                            }
                        }
                        if (validPayload)
                        {
                            //when we receive a valid payload, we add it to the appropriate job
                            Logger.LogText(String.Format("Received job {0} has a valid payload", BitConverter.ToString(job.JobId.ToByteArray())), this, Logtype.Debug);                          
                        }
                        else
                        {
                            //when we receive an invalid payload, we ignore it and give a warning
                            Logger.LogText(String.Format("Received job {0} has an invalid payload. Remove it now", BitConverter.ToString(job.JobId.ToByteArray())), this, Logtype.Warning);
                            job.JobPayload = new byte[0];
                        }
                    }
                    JobManager.Jobs.TryAdd(job.JobId, job);
                    Logger.LogText(String.Format("Added new job {0} to our job list", BitConverter.ToString(job.JobId.ToByteArray())), this, Logtype.Debug);
                    JobManager.OnJobListChanged();
                    return;
                }

                //We know the job but have no payload but the job has payload
                if (!JobManager.Jobs[job.JobId].HasPayload && job.HasPayload)
                {
                    //check, if the creator signature is valid
                    if (!job.HasValidCreatorSignature)
                    {
                        Logger.LogText(String.Format("Received job {0} with invalid creator signature from {1}. ", BitConverter.ToString(job.JobId.ToByteArray()), message.PeerId), this, Logtype.Warning);
                        return;
                    }

                    //check, if the hash of the payload is ok
                    var jobPayloadHash = CertificateService.GetCertificateService().ComputeHash(job.JobPayload);
                    bool validPayload = true;
                    for (int i = 0; i < jobPayloadHash.Length; i++)
                    {
                        if (jobPayloadHash[i] != JobManager.Jobs[job.JobId].JobPayloadHash[i])
                        {
                            validPayload = false;
                            break;
                        }
                    }
                    if (validPayload)
                    {
                        //when we receive a valid payload, we add it to the appropriate job
                        Logger.LogText(String.Format("Added received payload to job {0}", BitConverter.ToString(job.JobId.ToByteArray())), this, Logtype.Debug);
                        JobManager.Jobs[job.JobId].JobPayload = job.JobPayload;
                        JobManager.Jobs[job.JobId].OnPropertyChanged("HasPayload");
                        JobManager.Jobs[job.JobId].OnPropertyChanged("JobSize");
                    }
                    else
                    {
                        //when we receive an invalid payload, we ignore it and give a warning
                        Logger.LogText(String.Format("Received payload of job {0} is invalid. The computed hash is not equal to the JobPayloadHash we already know!", BitConverter.ToString(job.JobId.ToByteArray())), this, Logtype.Warning);
                    }
                }

                //here, we add a valid deletion signature to our local job
                if (job.HasValidDeletionSignature)
                {                    
                    Logger.LogText(String.Format("Received valid deletion signature for job {0}", BitConverter.ToString(job.JobId.ToByteArray())), this, Logtype.Debug);
                    JobManager.Jobs[job.JobId].JobDeletionSignatureData = job.JobDeletionSignatureData;
                    JobManager.Jobs[job.JobId].IsDeleted = true;
                    JobManager.OnJobListChanged();                    
                }               

                //case 1: we have no epoch state but received one
                if (job.JobEpochState != null && JobManager.Jobs[job.JobId].JobEpochState == null)
                {
                    JobManager.Jobs[job.JobId].JobEpochState = (EpochState)job.JobEpochState.Clone();                    
                }
                //case 2: we have an epoch state and received one
                else if (job.JobEpochState != null && JobManager.Jobs[job.JobId].JobEpochState != null)
                {
                    //to don't get into trouble with race conditions, we created an operation for ComputationManager...
                    MergeResultsOperation mergeResultsOperation = new MergeResultsOperation(JobManager.Jobs[job.JobId], job) { ComputationManager = JobManager.VoluntLib.ComputationManager };
                    JobManager.VoluntLib.ComputationManager.Operations.Enqueue(mergeResultsOperation);
                }
            }
        }
    }

    /// <summary>
    /// Operation for checking JobPayloads and getting them from the other peers
    /// </summary>
    internal class CheckJobsPayloadOperation : Operation
    {
        private Logger Logger = Logger.GetLogger();        

        /// <summary>
        /// CheckJobsPayloadOperation never finishes
        /// </summary>
        public override bool IsFinished
        {
            get { return false; }
        }

        /// <summary>
        /// Sends for every job that has no payload a RequestJobMessage
        /// Does this every 2 minutes for each job
        /// </summary>
        public override void Execute()
        {
            foreach (Job job in JobManager.Jobs.Values)
            {
                if (!job.HasPayload &&!job.IsDeleted && DateTime.Now > job.LastPayloadRequestTime.AddMilliseconds(Constants.CHECKJOBSPAYLOADOPERATION_REQUEST_INTERVAL))
                {
                    Logger.LogText(String.Format("Do not have payload for job {0}. Asking my neighbors now", BitConverter.ToString(job.JobId.ToByteArray())), this, Logtype.Debug);
                    JobManager.SendRequestJobMessage(null, job.JobId);
                    job.LastPayloadRequestTime = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// CheckJobsPayloadOperation does not handle any message
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {            
        }
    }

    /// <summary>
    /// Operation for answering RequestJobMessages
    /// Sends the job if we have it WITH payload
    /// </summary>
    internal class HandleRequestJobMessage : Operation
    {
        private Logger Logger = Logger.GetLogger();
        public override bool IsFinished
        {
            get { return false; }
        }

        public override void Execute()
        {
        }

        public override void HandleMessage(Message message)
        {
            if (message is RequestJobMessage)
            {
                //1. check, if we have the job AND its payload
                RequestJobMessage requestJobMessage = (RequestJobMessage)message;
                if (!JobManager.Jobs.ContainsKey(requestJobMessage.JobId) || !JobManager.Jobs[requestJobMessage.JobId].HasPayload)
                {
                    Logger.LogText(String.Format("Peer {0} requests job payload for job with jobId = {1}. But we don't have it!", BitConverter.ToString(message.PeerId), BitConverter.ToString(requestJobMessage.JobId.ToByteArray())), this, Logtype.Debug);
                    return;
                }
                else 
                { 
                    //2. we have the job AND the payload; thus, we send an answer
                    Logger.LogText(String.Format("Peer {0} requests job payload for job with jobId = {1}. Send it now!", BitConverter.ToString(message.PeerId), BitConverter.ToString(requestJobMessage.JobId.ToByteArray())), this, Logtype.Debug);
                    JobManager.SendResponseJobMessage(message.PeerId, JobManager.Jobs[requestJobMessage.JobId]);
                }
            }
        }
    }

    /// <summary>
    /// Serializes all messages every 5 minutes to file
    /// </summary>
    internal class JobsSerializationOperation : Operation
    {
        private Logger Logger = Logger.GetLogger();        
        private DateTime LastSerializationTime = DateTime.Now;

        /// <summary>
        /// JobSerializationOperation never finishes
        /// </summary>
        public override bool IsFinished
        {
            get { return false; }
        }

        /// <summary>
        /// Serializes all messages every 5 minutes to file
        /// </summary>
        public override void Execute()
        {
            if (DateTime.Now > LastSerializationTime.AddMilliseconds(Constants.JOBSSERIALIZATIONOPERATION_SERIALIZATION_INTERVAL))
            {
                if (!Directory.Exists(JobManager.LocalStoragePath))
                {
                    Directory.CreateDirectory(JobManager.LocalStoragePath);
                    Logger.LogText(String.Format("Created folder for serializing jobs: {0}", JobManager.LocalStoragePath), this, Logtype.Debug);
                }
                Logger.LogText("Serializing Jobs now...", this, Logtype.Debug);

                foreach (Job job in JobManager.Jobs.Values)
                {
                    try
                    {
                        FileStream stream = new FileStream(JobManager.LocalStoragePath + Path.DirectorySeparatorChar + BitConverter.ToString(job.JobId.ToByteArray()) + ".job", FileMode.Create, FileAccess.Write);
                        byte[] data = job.Serialize();
                        stream.Write(data,0,data.Length);
                        stream.Flush();
                        stream.Close();
                        Logger.LogText(String.Format("Job {0} serialized", BitConverter.ToString(job.JobId.ToByteArray())), this, Logtype.Debug);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogText(String.Format("Job {0} could not be serialized!", BitConverter.ToString(job.JobId.ToByteArray())), this, Logtype.Error);
                        Logger.LogException(ex, this, Logtype.Error);
                    }
                }
                LastSerializationTime = DateTime.Now;
            }
        }

        /// <summary>
        /// Does nothing with a message
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {            
        }

        /// <summary>
        /// Forces serialization by setting LastSerializationTime to min value
        /// </summary>
        public void ForceSerialization()
        {
            LastSerializationTime = DateTime.MinValue;
        }
    }

    /// <summary>
    /// Deserializes all stored jobs from APP DATA folder
    /// </summary>
    internal class JobsDeserializationOperation : Operation
    {
        private Logger Logger = Logger.GetLogger();
        private bool executed = false;

        /// <summary>
        /// JobDeserializationOperation never finishes
        /// </summary>
        public override bool IsFinished
        {
            get { return executed; }
        }

        /// <summary>
        /// Deserializes all jobs, also checks for valid creator and deletion signatures
        /// </summary>
        public override void Execute()
        {
            if (!Directory.Exists(JobManager.LocalStoragePath))
            {
                Directory.CreateDirectory(JobManager.LocalStoragePath);
                Logger.LogText(String.Format("Created folder for serializing jobs: {0}", JobManager.LocalStoragePath), this, Logtype.Debug);
                return;
            }
            foreach (string file in Directory.GetFiles(JobManager.LocalStoragePath,"*.job"))
            {
                try
                {
                    Job job = new Job(BigInteger.MinusOne);
                    byte[] data = File.ReadAllBytes(file);
                    job.Deserialize(data);
                    if (!job.HasValidCreatorSignature)
                    {
                        Logger.LogText(String.Format("Job {0} has no valid Creator Signature. File may be corrupted. Delete it now!", BitConverter.ToString(job.JobId.ToByteArray())), this, Logtype.Warning);
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception ex2)
                        {
                            Logger.LogText(String.Format("File {0} could not be deleted!", file), this, Logtype.Error);
                            Logger.LogException(ex2, this, Logtype.Error);
                        }
                    }

                    //Check payload signature when job has a payload
                    if (job.HasPayload)
                    {                        
                        byte[] signature = CertificateService.GetCertificateService().ComputeHash(job.JobPayload);
                        bool validPayloadSignature = true;
                        for (int i = 0; i < signature.Length; i++)
                        {
                            if (signature[i] != job.JobPayloadHash[i])
                            {
                                validPayloadSignature = false;
                                Logger.LogText(String.Format("Job {0} has no valid JobPayloadHash. File may be corrupted. Delete it now!", BitConverter.ToString(job.JobId.ToByteArray())), this, Logtype.Warning);
                                try
                                {
                                    File.Delete(file);
                                }
                                catch (Exception ex2)
                                {
                                    Logger.LogText(String.Format("File {0} could not be deleted!", file), this, Logtype.Error);
                                    Logger.LogException(ex2, this, Logtype.Error);
                                }                                
                                break;
                            }
                        }
                        if (!validPayloadSignature)
                        {
                            continue;
                        }
                    }
                    
                    if (job.HasValidDeletionSignature)
                    {
                        job.IsDeleted = true;
                    }
                    else
                    {
                        job.IsDeleted = false;
                        job.JobDeletionSignatureData = new byte[0];
                    }                    

                    JobManager.Jobs.TryAdd(job.JobId, job);
                    Logger.LogText(String.Format("Job {0} deserialized", BitConverter.ToString(job.JobId.ToByteArray())), this, Logtype.Debug);
                }
                catch (Exception ex)
                {
                    Logger.LogText(String.Format("Job from file {0} could not be serialized! Delete it now...", file), this, Logtype.Error);
                    Logger.LogException(ex, this, Logtype.Error);
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex2)
                    {
                        Logger.LogText(String.Format("File {0} could not be deleted!", file), this, Logtype.Error);
                        Logger.LogException(ex2, this, Logtype.Error);
                    }
                }
            }
            executed = true;
            JobManager.OnJobListChanged();
        }

        /// <summary>
        /// Does nothing with messages
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {            
        }
    }

    /// <summary>
    /// This operation updates the progress for the UI of each job
    /// </summary>
    internal class UpdateJobsProgressOperation : Operation
    {        
        private DateTime LastUpdateTime = DateTime.MinValue;

        /// <summary>
        /// UpdateJobsProgressOperation never finishes
        /// </summary>
        public override bool IsFinished
        {
            get { return false; }
        }

        /// <summary>
        /// Updates each job's progress and epoch progress and visualization
        /// </summary>
        public override void Execute()
        {
            if (DateTime.Now > LastUpdateTime.AddMilliseconds(Constants.UPDATEJOBSPROGRESSOPERATION_UPDATE_TIME_INTERVAL))
            {               
                foreach (Job job in new List<Job>(JobManager.JobList))
                {
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            job.UpdateProgessAndEpochProgress();
                        }
                        catch (Exception)
                        {
                            //if something wents wrong it is not so important
                        }
                    });
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            job.UpdateEpochVisualization();
                        }
                        catch (Exception)
                        {
                            //if something wents wrong it is not so important
                        }
                    });
                }
                LastUpdateTime = DateTime.Now;              
            }
        }

        /// <summary>
        /// Does nothing
        /// </summary>
        /// <param name="message"></param>
        public override void HandleMessage(Message message)
        {            
        }
    }
}