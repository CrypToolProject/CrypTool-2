using CrypCloud.Core.CloudComponent;
using CrypCloud.Core.Properties;
using CrypCloud.Core.utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using VoluntLib2;
using VoluntLib2.ComputationLayer;
using VoluntLib2.ConnectionLayer;
using VoluntLib2.ManagementLayer;
using VoluntLib2.Tools;
using WorkspaceManager.Model;

namespace CrypCloud.Core
{
    public class CrypCloudCore
    {
        public string DefaultWorld = "CrypCloud";

        #region singleton

        private static CrypCloudCore instance;

        public static CrypCloudCore Instance => instance ?? (instance = new CrypCloudCore());

        #endregion

        private VoluntLib voluntLib;

        #region properties

        public bool IsRunning => voluntLib.IsStarted;

        public int AmountOfWorker { get; set; }
        public bool WritePerformanceLog { get; set; }

        #endregion

        protected CrypCloudCore()
        {
            AmountOfWorker = 2;
            voluntLib = InitVoluntLib();
        }

        private VoluntLib InitVoluntLib()
        {
            VoluntLib vlib = new VoluntLib
            {
                LocalStoragePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CrypCloud" + Path.DirectorySeparatorChar + "Jobs"),
            };

            try
            {
                vlib.JobFinished -= OnJobFinished;
                vlib.TaskProgress -= OnTaskProgress;
                vlib.TaskStopped -= OnTaskHasStopped;
                vlib.TaskStarted -= OnTaskHasStarted;
                vlib.JobProgress -= OnJobStateChanged;
            }
            finally
            {
                vlib.JobProgress += OnJobStateChanged;
                vlib.TaskStarted += OnTaskHasStarted;
                vlib.TaskStopped += OnTaskHasStopped;
                vlib.TaskProgress += OnTaskProgress;
                vlib.JobFinished += OnJobFinished;
            }

            return vlib;
        }

        #region login/Logout/start/stop

        public bool Login(X509Certificate2 ownCertificate)
        {
            if (IsRunning)
            {
                return false;
            }
            try
            {
                if (LogLevel != null)
                {
                    if (LogLevel.Equals("Debug"))
                    {
                        VoluntLib2.Tools.Logger.SetLogLevel(Logtype.Debug);
                    }
                    else if (LogLevel.Equals("Info"))
                    {
                        VoluntLib2.Tools.Logger.SetLogLevel(Logtype.Info);
                    }
                    else if (LogLevel.Equals("Warning"))
                    {
                        VoluntLib2.Tools.Logger.SetLogLevel(Logtype.Warning);
                    }
                    else if (LogLevel.Equals("Error"))
                    {
                        VoluntLib2.Tools.Logger.SetLogLevel(Logtype.Error);
                    }
                }
                else
                {
                    VoluntLib2.Tools.Logger.SetLogLevel(Logtype.Warning);
                }

                //root certificate for checking signatures
                X509Certificate2 rootCertificate = new X509Certificate2(Resources.rootCA);
                //well known peers for boostrapping p2p network
                string wellKnownPeers = Resources.wellKnownPeers.Replace("\r", "");
                List<string> wellKnownPeersList = wellKnownPeers.Split('\n').ToList();
                voluntLib.WellKnownPeers.Clear();
                voluntLib.WellKnownPeers.AddRange(wellKnownPeersList);
                //start voluntlib now:
                voluntLib.Start(rootCertificate, ownCertificate);
                //When VoluntLib is started, the admin and banned lists are cleared
                //Thus, we here add the admin and banned certificates
                string adminCertificates = Resources.adminCertificates.Replace("\r", "");
                List<string> adminList = adminCertificates.Split('\n').ToList();
                string bannedCertificates = Resources.bannedCertificates.Replace("\r", "");
                List<string> bannedList = bannedCertificates.Split('\n').ToList();
                CertificateService.GetCertificateService().AdminCertificateList.AddRange(adminList);
                CertificateService.GetCertificateService().BannedCertificateList.AddRange(bannedList);

                OnConnectionStateChanged(true);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public void Logout()
        {
            if (!IsRunning)
            {
                return;
            }

            try
            {
                voluntLib.JobProgress -= OnJobStateChanged;
                voluntLib.TaskStarted -= OnTaskHasStarted;
                voluntLib.TaskStopped -= OnTaskHasStopped;
                voluntLib.TaskProgress -= OnTaskProgress;
                voluntLib.JobFinished -= OnJobFinished;
            }
            catch (Exception) { }

            try
            {
                voluntLib.Stop();
                OnConnectionStateChanged(false);
            }
            finally
            {
                voluntLib = InitVoluntLib();
            }

        }
        public void StartLocalCalculation(BigInteger jobId, ACalculationTemplate template)
        {
            voluntLib.JoinJob(jobId, template, AmountOfWorker);
        }

        public void StopLocalCalculation(BigInteger jobId)
        {
            voluntLib.StopJob(jobId);
        }
        #endregion


        #region job information

        public ObservableCollection<Job> GetJoblist()
        {
            if (!voluntLib.IsStarted)
            {
                return new ObservableCollection<Job>();
            }

            return voluntLib.GetJoblist();
        }


        public ObservableCollection<Contact> GetContacts()
        {
            if (!voluntLib.IsStarted)
            {
                return new ObservableCollection<Contact>();
            }

            return voluntLib.GetContactList();
        }

        public Job GetJobById(BigInteger jobid)
        {
            if (!voluntLib.IsStarted)
            {
                return new Job(jobid);
            }

            return voluntLib.GetJobByID(jobid);
        }

        public NetworkJobData GetJobDataById(BigInteger jobId)
        {
            if (!voluntLib.IsStarted)
            {
                return null;
            }

            return new NetworkJobData
            {
                Job = voluntLib.GetJobByID(jobId),
                CalculatedBlocks = () => GetCalculatedBlocksOfJob(jobId),
                Workspace = () => GetWorkspaceOfJob(jobId),
                CreationDate = () => GetCreationDateOfJob(jobId),
                Payload = () => GetPayloadOfJob(jobId),
                GetCurrentTopList = () => GetToplistOfJob(jobId)
            };
        }

        private List<byte[]> GetToplistOfJob(BigInteger jobId)
        {
            if (!voluntLib.IsStarted)
            {
                return new List<byte[]>();
            }

            Job job = voluntLib.GetJobByID(jobId);
            if (job == null)
            {
                return new List<byte[]>();
            }

            EpochState stateOfJob = voluntLib.GetStateOfJob(jobId);
            if (stateOfJob == null)
            {
                return new List<byte[]>();
            }

            return new List<byte[]>(stateOfJob.ResultList);
        }

        public BigInteger GetCalculatedBlocksOfJob(BigInteger jobID)
        {
            if (!voluntLib.IsStarted)
            {
                return new BigInteger(0);
            }

            return voluntLib.GetCalculatedBlocksOfJob(jobID);
        }

        public WorkspaceModel GetWorkspaceOfJob(BigInteger jobId)
        {
            JobPayload payloadOfJob = GetPayloadOfJob(jobId);
            if (payloadOfJob == null)
            {
                return null;
            }

            WorkspaceModel workspaceOfJob = payloadOfJob.WorkspaceModel;

            IEnumerable<PluginModel> cloudComponents = workspaceOfJob.GetAllPluginModels()
                .Where(pluginModel => pluginModel.Plugin is ACloudCompatible);
            foreach (PluginModel cloudComponent in cloudComponents)
            {
                ((ACloudCompatible)cloudComponent.Plugin).JobId = jobId;
                ((ACloudCompatible)cloudComponent.Plugin).ComputeWorkspaceHash = workspaceOfJob.ComputeWorkspaceHash;
                ((ACloudCompatible)cloudComponent.Plugin).ValidWorkspaceHash = workspaceOfJob.ComputeWorkspaceHash();
            }

            return workspaceOfJob;
        }

        public DateTime GetCreationDateOfJob(BigInteger jobId)
        {
            JobPayload payloadOfJob = GetPayloadOfJob(jobId);
            if (payloadOfJob == null)
            {
                return new DateTime(0);
            }

            return payloadOfJob.CreationTime;
        }

        public JobPayload GetPayloadOfJob(BigInteger jobId)
        {
            Job job = voluntLib.GetJobByID(jobId);
            if (job == null || !job.HasPayload || job.IsDeleted)
            {
                return null;
            }
            try
            {
                return new JobPayload().Deserialize(job.JobPayload);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool UserCanDeleteJob(Job job)
        {
            return voluntLib.CanUserDeleteJob(job);
        }

        #endregion

        public bool IsPartizipationOnJob()
        {
            if (voluntLib.GetCurrentRunningWorkersPerJob().Count > 0)
            {
                foreach (int workerNo in voluntLib.GetCurrentRunningWorkersPerJob().Values)
                {
                    if (workerNo > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsBannedCertificate(X509Certificate2 certificate)
        {
            return CertificateService.GetCertificateService().IsBannedCertificate(certificate);
        }

        public void RefreshJobList()
        {
            voluntLib.RefreshJobList();
        }


        public void DownloadWorkspaceOfJob(BigInteger jobId)
        {
            Job job = voluntLib.GetJobByID(jobId);
            if (job == null)
            {
                return;
            }

            if (job.HasPayload)
            {
                return;
            }

            voluntLib.RequestJob(job);
        }

        public void DeleteJob(BigInteger jobId)
        {
            if (UserCanDeleteJob(voluntLib.GetJobByID(jobId)))
            {
                voluntLib.DeleteJob(jobId);
            }
        }

        public bool CreateJob(string jobType, string jobName, string jobDescription, WorkspaceModel workspaceModel)
        {
            ACloudCompatible cloudComponent = GetCloudComponent(workspaceModel);
            if (cloudComponent == null)
            {
                return false;
            }

            BigInteger numberOfBlocks = cloudComponent.NumberOfBlocks;
            JobPayload jobPayload = new JobPayload
            {
                WorkspaceModel = workspaceModel,
                CreationTime = DateTime.Now
            };

            byte[] serialize = jobPayload.Serialize();

            BigInteger jobID = voluntLib.CreateJob(DefaultWorld, jobType, jobName, jobDescription, serialize,
                numberOfBlocks);
            return jobID != -1;
        }

        private static ACloudCompatible GetCloudComponent(WorkspaceModel workspaceModel)
        {
            try
            {
                PluginModel cloudModel =
                    workspaceModel.GetAllPluginModels().First(pluginModel => pluginModel.Plugin is ACloudCompatible);
                return cloudModel.Plugin as ACloudCompatible;
            }
            catch
            {
                return null;
            }
        }

        #region events  

        public event Action<bool> ConnectionStateChanged;
        public event EventHandler JobListChanged;
        public event EventHandler<JobProgressEventArgs> JobStateChanged;

        public event EventHandler<TaskEventArgs> TaskHasStarted;
        public event EventHandler<TaskEventArgs> TaskHasStopped;
        public event EventHandler<TaskEventArgs> TaskProgress;
        public event EventHandler<JobProgressEventArgs> JobFinished;

        protected virtual void OnTaskHasStarted(object sender, TaskEventArgs taskEventArgs)
        {
            EventHandler<TaskEventArgs> handler = TaskHasStarted;
            if (handler != null)
            {
                handler(this, taskEventArgs);
            }
        }

        protected virtual void OnTaskHasStopped(object sender, TaskEventArgs taskEventArgs)
        {
            EventHandler<TaskEventArgs> handler = TaskHasStopped;
            if (handler != null)
            {
                handler(this, taskEventArgs);
            }
        }

        protected virtual void OnTaskProgress(object sender, TaskEventArgs taskEventArgs)
        {
            EventHandler<TaskEventArgs> handler = TaskProgress;
            if (handler != null)
            {
                handler(this, taskEventArgs);
            }
        }

        protected virtual void OnJobFinished(object sender, JobProgressEventArgs jobProgressEventArgs)
        {
            EventHandler<JobProgressEventArgs> handler = JobFinished;
            if (handler != null)
            {
                handler(this, jobProgressEventArgs);
            }
        }

        protected virtual void OnConnectionStateChanged(bool connected)
        {
            Action<bool> handler = ConnectionStateChanged;
            if (handler != null)
            {
                handler(connected);
            }
        }

        protected virtual void OnJobStateChanged(object sender, JobProgressEventArgs jobProgressEventArgs)
        {
            EventHandler<JobProgressEventArgs> handler = JobStateChanged;
            if (handler != null)
            {
                handler(sender, jobProgressEventArgs);
            }
        }

        #endregion

        public BigInteger GetEpochOfJob(Job job)
        {
            EpochState stateOfJob = voluntLib.GetStateOfJob(job.JobId);
            return (stateOfJob != null) ? stateOfJob.EpochNumber : 0;
        }


        public string GetUsername()
        {
            return voluntLib.CertificateName;
        }

        public string LogLevel { get; set; }

    }

    public class NetworkJobData
    {
        public Job Job { get; set; }
        public Func<BigInteger> CalculatedBlocks { get; set; }
        public Func<bool> HasWorkspace { get; set; }
        public Func<WorkspaceModel> Workspace { get; set; }
        public Func<DateTime> CreationDate { get; set; }
        public Func<JobPayload> Payload { get; set; }
        public Func<List<byte[]>> GetCurrentTopList { get; set; }
    }
}
