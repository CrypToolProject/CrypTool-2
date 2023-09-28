using CrypCloud.Core;
using CrypTool.PluginBase;
using CrypTool.PluginBase.Control;
using KeySearcher.CrypCloud;
using KeySearcher.CrypCloud.statistics;
using KeySearcherPresentation.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Timers;
using VoluntLib2.Tools;
using Timer = System.Timers.Timer;

namespace KeySearcher
{
    internal class CloudKeySearcher
    {
        public int UpdateInterval = 2000;
        private readonly string LogfilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CrypCloud");

        private readonly KeySearcher keySearcher;
        private readonly CalculationTemplate calculationTemplate;
        private readonly BigInteger jobId;
        private readonly P2PPresentationVM viewModel;
        private readonly TaskFactory uiContext;

        private readonly SpeedStatistics globalSpeedStatistics = new SpeedStatistics(30, 2);
        private readonly SpeedStatistics localSpeedStatistics = new SpeedStatistics(5, 0);

        private Timer updateTimer;
        private StreamWriter Logfile = null;

        private BigInteger LastNumberOfCalculatedBlocks = BigInteger.Zero;

        public CloudKeySearcher(JobDataContainer jobDataContainer, KeyPattern.KeyPattern pattern, P2PQuickWatchPresentation presentation, KeySearcher keySearcher)
        {
            this.keySearcher = keySearcher;

            jobId = jobDataContainer.JobId;
            calculationTemplate = new CalculationTemplate(jobDataContainer, pattern, SortAscending(), keySearcher);

            uiContext = presentation.UiContext;
            viewModel = presentation.ViewModel;
            viewModel.GlobalSpeedStatistics = globalSpeedStatistics;
            viewModel.LocalSpeedStatistics = localSpeedStatistics;

            try
            {
                NetworkJobData jobData = CrypCloudCore.Instance.GetJobDataById(jobId);
                RunInUiContext(() =>
                {
                    viewModel.JobID = jobId;
                    UpdatePresentation(presentation, keySearcher);
                    keySearcher.ProgressChanged(Math.Floor(viewModel.GlobalProgress), 100);
                });
            }
            catch (Exception) { }
        }

        private void UpdateKeyPerSecond(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            BigInteger localApproximateKeysPerSecond = localSpeedStatistics.ApproximateKeysPerSecond();
            BigInteger globalApproximateKeysPerSecond = globalSpeedStatistics.ApproximateKeysPerSecond();

            if (localApproximateKeysPerSecond > globalApproximateKeysPerSecond)
            {
                globalApproximateKeysPerSecond = localApproximateKeysPerSecond;
            }

            RunInUiContext(() =>
            {
                viewModel.UpdateGlobalSpeed(globalApproximateKeysPerSecond);
                viewModel.UpdateLocalSpeed(localApproximateKeysPerSecond);
            });

            if (CrypCloudCore.Instance.WritePerformanceLog)
            {
                Logfile.WriteLine("{0};{1};{2};{3};{4};{5}", DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss:ffff"),
                    localApproximateKeysPerSecond,
                    globalApproximateKeysPerSecond,
                    viewModel.LocalFinishedChunks,
                    viewModel.LocalAbortChunks,
                    viewModel.FinishedNumberOfBlocks,
                    viewModel.GlobalProgress);
                Logfile.Flush();
            }
        }

        private DateTime _lastUpdateTime = DateTime.Now;
        private void TaskProgress(object sender, TaskEventArgs e)
        {
            if (e.JobId != jobId)
            {
                return;
            }
            localSpeedStatistics.AddEntry(e.TaskProgress);

            if (DateTime.Now >= _lastUpdateTime.AddSeconds(1))
            {
                BigInteger localApproximateKeysPerSecond = localSpeedStatistics.ApproximateKeysPerSecond();
                RunInUiContext(() =>
                {
                    viewModel.UpdateLocalSpeed(localApproximateKeysPerSecond);
                });
                _lastUpdateTime = DateTime.Now;
            }
        }

        private void NewTaskStarted(object sender, TaskEventArgs taskArgs)
        {
            if (taskArgs.JobId != jobId)
            {
                return;
            }

            if (keySearcher.WorkspaceHasBeenModified())
            {
                CrypCloudCore.Instance.StopLocalCalculation(jobId);
                keySearcher.GuiLogMessage("Calculation has been aborted due to changes in the workplace.", NotificationLevel.Error);
                return;
            }

            RunInUiContext(
                () => viewModel.StartedLocalCalculation(taskArgs.BlockID)
            );
        }

        private void TaskEnded(object sender, TaskEventArgs taskArgs)
        {
            if (taskArgs.JobId != jobId)
            {
                return;
            }

            RunInUiContext(() =>
            {
                viewModel.EndedLocalCalculation(taskArgs);
                keySearcher.ProgressChanged(Math.Floor(viewModel.GlobalProgress), 100);
            });
        }

        private void JobStateChanged(object sender, JobProgressEventArgs progress)
        {
            if (progress.JobId != jobId)
            {
                return;
            }

            List<KeyResultEntry> keyResultList = ExtractResultList(progress);
            RunInUiContext(() =>
            {
                //for each "new block", we call BlockHasBeenFinished for the global speed calculation
                lock (this)
                {
                    if (LastNumberOfCalculatedBlocks == BigInteger.Zero)
                    {
                        LastNumberOfCalculatedBlocks = progress.NumberOfCalculatedBlocks;
                    }
                    if (progress.NumberOfCalculatedBlocks > LastNumberOfCalculatedBlocks)
                    {
                        for (int i = 0; i < progress.NumberOfCalculatedBlocks - LastNumberOfCalculatedBlocks; i++)
                        {
                            viewModel.BlockHasBeenFinished();
                        }
                        LastNumberOfCalculatedBlocks = progress.NumberOfCalculatedBlocks;
                    }
                }
                viewModel.UpdateGlobalProgress(progress, keyResultList.ToList());
                keySearcher.ProgressChanged(Math.Floor(viewModel.GlobalProgress), 100);
            });

            if (keyResultList.Any())
            {
                keySearcher.SetTop1Entry(keyResultList.First());
            }
        }

        private List<KeyResultEntry> ExtractResultList(JobProgressEventArgs progress)
        {
            IEnumerable<KeyResultEntry> keyResultEntries = progress.ResultList
                .Select(it => new KeyResultEntry(it))
                .Distinct();

            keyResultEntries = SortAscending()
                ? keyResultEntries.OrderBy(it => it)
                : keyResultEntries.OrderByDescending(it => it);

            List<KeyResultEntry> keyResultList = keyResultEntries.ToList();
            return keyResultList;
        }

        private bool SortAscending()
        {
            return keySearcher.CostMaster.GetRelationOperator().Equals(RelationOperator.LessThen);
        }


        private void UpdatePresentation(P2PQuickWatchPresentation presentation, KeySearcher keySearcher)
        {
            presentation.UpdateSettings(keySearcher, (KeySearcherSettings)keySearcher.Settings); ;
        }

        protected void RunInUiContext(Action action)
        {
            if (uiContext != null)
            {
                uiContext.StartNew(action);
            }
            else
            {
                action.Invoke();
            }
        }

        public void Start()
        {
            if (keySearcher.WorkspaceHasBeenModified())
            {
                keySearcher.GuiLogMessage("Calculation cannot be started since the workspace was changed.", NotificationLevel.Error);
                return;
            }

            if (CrypCloudCore.Instance.WritePerformanceLog)
            {
                Logfile = new StreamWriter(System.IO.Path.Combine(LogfilePath, "logfile_" + jobId + ".csv").ToString(), true);
                Logfile.WriteLine("timestamp;localspeed;globalspeed;finishedlocalchunks;abortedlocalchunks;finishedglobalchunks;globalprocess");
            }

            try
            {
                CrypCloudCore.Instance.TaskProgress -= TaskProgress;
                CrypCloudCore.Instance.TaskHasStopped -= TaskEnded;
                CrypCloudCore.Instance.TaskHasStarted -= NewTaskStarted;
                CrypCloudCore.Instance.JobStateChanged -= JobStateChanged;
            }
            finally
            {
                CrypCloudCore.Instance.JobStateChanged += JobStateChanged;
                CrypCloudCore.Instance.TaskHasStarted += NewTaskStarted;
                CrypCloudCore.Instance.TaskHasStopped += TaskEnded;
                CrypCloudCore.Instance.TaskProgress += TaskProgress;
            }

            CrypCloudCore.Instance.StartLocalCalculation(jobId, calculationTemplate);

            updateTimer = new Timer(UpdateInterval);
            updateTimer.Elapsed += UpdateKeyPerSecond;
            updateTimer.Interval = UpdateInterval;
            updateTimer.Enabled = true;
        }

        public void Stop()
        {

            viewModel.CurrentChunks.Clear();
            viewModel.OnPropertyChanged("CurrentChunks");

            try
            {
                updateTimer.Enabled = false;
                updateTimer.Stop();
            }
            catch (Exception) { }

            try
            {
                CrypCloudCore.Instance.TaskProgress -= TaskProgress;
                CrypCloudCore.Instance.TaskHasStopped -= TaskEnded;
                CrypCloudCore.Instance.TaskHasStarted -= NewTaskStarted;
                CrypCloudCore.Instance.JobStateChanged -= JobStateChanged;
            }
            catch (Exception) { }

            try
            {
                CrypCloudCore.Instance.StopLocalCalculation(jobId);
            }
            catch (Exception) { }

            if (CrypCloudCore.Instance.WritePerformanceLog && Logfile != null)
            {
                Logfile.Close();
                Logfile = null;
            }
        }
    }
}