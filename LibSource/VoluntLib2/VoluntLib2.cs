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
using System.Collections.ObjectModel;
using System.Net;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using VoluntLib2.ComputationLayer;
using VoluntLib2.ConnectionLayer;
using VoluntLib2.ManagementLayer;
using VoluntLib2.Tools;

namespace VoluntLib2
{
    /// <summary>
    /// Main class of VoluntLib
    /// </summary>
    public class VoluntLib
    {
        private readonly CertificateService CertificateService = CertificateService.GetCertificateService();
        internal ConnectionManager ConnectionManager;
        internal JobManager JobManager;
        internal ComputationManager ComputationManager;
        private readonly Logger Logger = Logger.GetLogger();

        public event EventHandler<JobProgressEventArgs> JobProgress;
        public event EventHandler<JobProgressEventArgs> JobFinished;
        public event EventHandler<TaskEventArgs> TaskStarted;
        public event EventHandler<TaskEventArgs> TaskProgress;
        public event EventHandler<TaskEventArgs> TaskStopped;
        public event EventHandler<ConnectionsNumberChangedEventArgs> ConnectionsNumberChanged;

        /// <summary>
        /// Creates a new instance of VoluntLib
        /// </summary>
        public VoluntLib()
        {
        }

        /// <summary>
        /// Path where all jobs will be serialized to and deserialized from
        /// </summary>
        public string LocalStoragePath { get; set; }

        /// <summary>
        /// Returns the name of the user currently logged into VoluntLib
        /// </summary>
        public string CertificateName { get; internal set; }

        /// <summary>
        /// Returns true if VoluntLib is started
        /// </summary>
        public bool IsStarted { get; internal set; }

        /// <summary>
        /// This list should contain at least one bootstrap server to connect to at startup
        /// </summary>
        public List<string> WellKnownPeers = new List<string>();

        /// <summary>
        /// Starts VoluntLib2.
        /// Creates a JobManager, a ConnectionManager, and a ComputationManager and starts everything.
        /// Needs certificates and a defined listen port to listen on
        /// </summary>
        /// <param name="caCertificate"></param>
        /// <param name="ownCertificate"></param>
        /// <param name="listenport"></param>
        public void Start(X509Certificate2 caCertificate, X509Certificate2 ownCertificate, ushort listenport = 10000, bool initCertificateService = true)
        {
            //1) Init Certificate ervice
            if (initCertificateService)
            {
                CertificateService.Init(caCertificate, ownCertificate);                
            }
            CertificateName = CertificateService.OwnName;

            //2) Create, initialize, and start ConnectionManager
            ConnectionManager = new ConnectionManager(this, listenport);
            foreach (string wellknownpeer in WellKnownPeers)
            {
                try
                {
                    string[] ipport = wellknownpeer.Split(new char[] { ':' });
                    if (ipport.Length != 2)
                    {
                        Logger.LogText(string.Format("List of WellKnownPeers contained invalid entry (wrong parameter count): {0}. Ignore it", wellknownpeer), this, Logtype.Warning);
                        continue;
                    }
                    IPAddress ip;
                    ushort port;
                    try
                    {
                        ip = IPAddress.Parse(ipport[0]);
                    }
                    catch (Exception)
                    {
                        try
                        {
                            //if its not an ip, it may be a dns name, thus we look it up
                            IPAddress[] ips = Dns.GetHostAddresses(ipport[0]);
                            try
                            {
                                port = ushort.Parse(ipport[1]);
                            }
                            catch (Exception)
                            {
                                Logger.LogText(string.Format("List of WellKnownPeers contained invalid entry (invalid port): {0}. Ignore it", wellknownpeer), this, Logtype.Warning);
                                continue;
                            }
                            ConnectionManager.AddWellknownPeer(ips[0], port);
                            continue;
                        }
                        catch (Exception)
                        {
                            Logger.LogText(string.Format("List of WellKnownPeers contained invalid entry (invalid ip/hostname): {0}. Ignore it", wellknownpeer), this, Logtype.Warning);
                        }
                        continue;
                    }
                    try
                    {
                        port = ushort.Parse(ipport[1]);
                    }
                    catch (Exception)
                    {
                        Logger.LogText(string.Format("List of WellKnownPeers contained invalid entry (invalid port): {0}. Ignore it", wellknownpeer), this, Logtype.Warning);
                        continue;
                    }
                    ConnectionManager.AddWellknownPeer(ip, port);
                }
                catch (Exception ex)
                {
                    Logger.LogText(string.Format("List of WellKnownPeers contained invalid entry (Exception: {0}): {1}. Ignore it", ex.Message, wellknownpeer), this, Logtype.Warning);
                    continue;
                }
            }
            ConnectionManager.ConnectionsNumberChanged += ConnectionsNumberChanged;
            ConnectionManager.Start();

            //3) Create, initialize, and start JobManager
            JobManager = new JobManager(this, ConnectionManager, LocalStoragePath);
            JobManager.Start();

            //4) Create, initialize, and start ComputationManager
            ComputationManager = new ComputationManager(this, JobManager);
            ComputationManager.Start();

            IsStarted = true;
        }

        /// <summary>
        /// Stops VoluntLib 2, stops all managers, blocks until all are stopped
        /// </summary>
        public void Stop()
        {
            //do nothing if not running
            if (!IsStarted)
            {
                return;
            }
            //we have to first stop the connection manager; otherwise the JobManager remains 
            //blocked in its receiving thread and cannot be stopped
            ConnectionManager.Stop();
            JobManager.Stop();
            ComputationManager.Stop();
            IsStarted = false;
        }

        /// <summary>
        /// Joins the given job, using the joibd, template, and amount of workers
        /// </summary>
        /// <param name="jobID"></param>
        /// <param name="template"></param>
        /// <param name="amountOfWorker"></param>
        /// <returns></returns>
        public bool JoinJob(BigInteger jobID, ACalculationTemplate template, int amountOfWorker)
        {
            //do nothing if not running
            if (!IsStarted)
            {
                return false;
            }
            return ComputationManager.JoinJob(jobID, template, amountOfWorker);
        }

        /// <summary>
        /// Stops the job with the given jobid
        /// </summary>
        /// <param name="jobID"></param>
        public void StopJob(BigInteger jobID)
        {
            //do nothing if not running
            if (!IsStarted)
            {
                return;
            }
            ComputationManager.StopJob(jobID);
        }

        /// <summary>
        /// Refreshes the internal job list by asking all neighbors for their job lists
        /// </summary>
        public void RefreshJobList()
        {
            //do nothing if not running
            if (!IsStarted)
            {
                return;
            }
            JobManager.RefreshJobList();
        }

        /// <summary>
        /// Requests data of the given job by asking all neighbors
        /// </summary>
        /// <param name="job"></param>
        public void RequestJob(Job job)
        {
            //do nothing if not running
            if (!IsStarted)
            {
                return;
            }
            JobManager.RequestJob(job.JobId);
        }

        /// <summary>
        /// Deletes the job with the given jobid; only works if
        /// A) the job is owned by this user or
        /// B) this user is an admin
        /// </summary>
        /// <param name="jobID"></param>
        public void DeleteJob(BigInteger jobID)
        {
            //do nothing if not running
            if (!IsStarted)
            {
                return;
            }
            JobManager.DeleteJob(jobID);
        }

        /// <summary>
        /// Creates a new job signed by this user; Returns -1 if it fails
        /// </summary>
        /// <param name="worldName"></param>
        /// <param name="jobType"></param>
        /// <param name="jobName"></param>
        /// <param name="jobDescription"></param>
        /// <param name="payload"></param>
        /// <param name="numberOfBlocks"></param>
        /// <returns></returns>
        public BigInteger CreateJob(string worldName, string jobType, string jobName, string jobDescription, byte[] payload, BigInteger numberOfBlocks)
        {
            //do nothing if not running
            if (!IsStarted)
            {
                return BigInteger.MinusOne;
            }
            return JobManager.CreateJob(worldName, jobType, jobName, jobDescription, payload, numberOfBlocks);
        }

        /// <summary>
        /// Returns the job with the specific jobid.
        /// returns null, if the job does not exist
        /// </summary>
        /// <param name="jobID"></param>
        /// <returns></returns>
        public Job GetJobByID(BigInteger jobId)
        {
            //do nothing if not running
            if (!IsStarted)
            {
                return null;
            }
            return JobManager.GetJobById(jobId);
        }

        /// <summary>
        /// Get the number of calculated jobs of this job
        /// </summary>
        /// <param name="jobID"></param>
        /// <returns></returns>
        public BigInteger GetCalculatedBlocksOfJob(BigInteger jobId)
        {
            //do nothing if not running
            if (!IsStarted)
            {
                return BigInteger.Zero;
            }
            return JobManager.GetCalculatedBlocksOfJob(jobId);
        }

        /// <summary>
        /// Returns the number of workers of each job
        /// </summary>
        /// <returns></returns>
        public Dictionary<BigInteger, int> GetCurrentRunningWorkersPerJob()
        {
            //do nothing if not running
            if (!IsStarted)
            {
                return new Dictionary<BigInteger, int>();
            }
            return ComputationManager.GetCurrentRunningWorkersPerJob();
        }

        /// <summary>
        /// Returns the epoch state of the job with the given jobid
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public EpochState GetStateOfJob(BigInteger jobId)
        {
            //do nothing if not running
            if (!IsStarted)
            {
                return null;
            }
            return JobManager.GetStateOfJob(jobId);
        }

        /// <summary>
        /// Returns true if the user is allowed to delete the given job
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public bool CanUserDeleteJob(Job job)
        {
            //do nothing if not running
            if (!IsStarted)
            {
                return false;
            }
            return (job.CreatorName.Equals(CertificateName)) ||
                CertificateService.GetCertificateService().IsAdminCertificate(CertificateService.GetCertificateService().OwnCertificate);
        }

        /// <summary>
        /// Returns a reference to an observable list of jobs
        /// WARNING: Do not modify this list from the outside of VoluntLib2
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<Job> GetJoblist()
        {
            //do nothing if not running
            if (!IsStarted)
            {
                return null;
            }
            return JobManager.GetJoblist();
        }

        /// <summary>
        /// Returns a reference to an observable list of contacts
        /// WARNING: Do not modify this list from the outside of VoluntLib2
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<Contact> GetContactList()
        {
            if (!IsStarted)
            {
                return null;
            }
            return ConnectionManager.GetContacts();
        }

        /// <summary>
        /// Returns peer id of this peer
        /// </summary>
        /// <returns></returns>
        public byte[] GetPeerId()
        {
            if (!IsStarted)
            {
                return null;
            }
            return ConnectionManager.GetPeerId();
        }

        /// <summary>
        /// Helper method to invoke TaskProgessChanged events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnTaskProgessChanged(object sender, TaskEventArgs e)
        {
            if (TaskProgress != null)
            {
                TaskProgress.Invoke(sender, e);
            }
        }

        /// <summary>
        /// Helper method to invoke TaskStarted events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnTaskStarted(object sender, TaskEventArgs e)
        {
            if (TaskStarted != null)
            {
                TaskStarted.Invoke(sender, e);
            }
        }

        /// <summary>
        /// Helper method to invoke TaskStopped events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnTaskStopped(object sender, TaskEventArgs e)
        {
            if (TaskStopped != null)
            {
                TaskStopped.Invoke(sender, e);
            }
        }

        /// <summary>
        /// Helper method to invoke JobProgress events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnJobProgress(object sender, JobProgressEventArgs e)
        {
            if (JobProgress != null)
            {
                JobProgress.Invoke(sender, e);
            }
        }

        /// <summary>
        /// Helper method to invoke JobFinished events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnJobFinished(object sender, JobProgressEventArgs e)
        {
            if (JobFinished != null)
            {
                JobFinished.Invoke(sender, e);
            }
        }
    }
}