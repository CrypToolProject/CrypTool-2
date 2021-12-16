using CrypCloud.Core;
using KeySearcher.CrypCloud.statistics;
using KeySearcher.KeyPattern;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using VoluntLib2.Tools;

namespace KeySearcher.CrypCloud
{
    public class P2PPresentationVM : INotifyPropertyChanged
    {
        private string jobName;
        private string jobDesc;
        private BigInteger jobID;
        private BigInteger totalAmountOfChunks;
        private BigInteger keysPerBlock;

        private double globalProgress;
        private BigInteger keysPerSecondGlobal;
        private TimeSpan avgTimePerChunkGlobal;

        private BigInteger localFinishedChunks;
        private BigInteger finishedNumberOfBlocks;
        private BigInteger localAbortChunks;
        private BigInteger keysPerSecond;
        private string currentOperation = "idle";
        private TimeSpan avgTimePerChunk;

        private DateTime startDate;
        private TimeSpan elapsedTime;
        private TimeSpan remainingTime;
        private TimeSpan remainingTimeTotal;
        private DateTime estimatedFinishDate;
        private BigInteger numberOfLeftBlocks;
        private BigInteger localThroughputInBytes;
        private BigInteger globalThroughputInBytes;

        public SpeedStatistics GlobalSpeedStatistics { get; set; }
        public SpeedStatistics LocalSpeedStatistics { get; set; }

        public TaskFactory UiContext { get; set; }

        public ObservableCollection<BigInteger> CurrentChunks { get; }
        public ObservableCollection<KeyResultEntry> TopList { get; }


        public P2PPresentationVM()
        {
            CurrentOperation = "Idle";
            AvgTimePerChunk = new TimeSpan(0);
            TopList = new ObservableCollection<KeyResultEntry>();
            CurrentChunks = new ObservableCollection<BigInteger>();

        }
        public void UpdateStaticView(BigInteger jobId, KeySearcher keySearcher, KeySearcherSettings keySearcherSettings)
        {
            if (!CrypCloudCore.Instance.IsRunning)
            {
                return;
            }

            VoluntLib2.ManagementLayer.Job job = CrypCloudCore.Instance.GetJobById(jobId);
            if (job == null)
            {
                return;
            }

            JobName = job.JobName;

            if (job.NumberOfBlocks != 0)
            {
                FinishedNumberOfBlocks = CrypCloudCore.Instance.GetCalculatedBlocksOfJob(jobId);
                double progress = 100 * finishedNumberOfBlocks.DivideAndReturnDouble(job.NumberOfBlocks);
                GlobalProgress = progress;
                OnPropertyChanged("GlobalProgressString");
            }


        }

        public void UpdateSettings(KeySearcher keySearcher, KeySearcherSettings keySearcherSettings)
        {
            if (CannotUpdateView(keySearcher, keySearcherSettings))
            {
                return;
            }

            KeyPattern.KeyPattern keyPattern = new KeyPattern.KeyPattern(keySearcher.ControlMaster.GetKeyPattern()) { WildcardKey = keySearcherSettings.Key };
            BigInteger keysPerChunk = keyPattern.size() / BigInteger.Pow(2, keySearcherSettings.NumberOfBlocks);
            if (keysPerChunk < 1)
            {
                keySearcherSettings.NumberOfBlocks = (int)BigInteger.Log(keyPattern.size(), 2);
            }

            KeyPatternPool keyPatternPool = new KeyPatternPool(keyPattern, keysPerChunk);

            TotalAmountOfChunks = keyPatternPool.Length;
            KeysPerBlock = keysPerChunk;
            JobID = keySearcher.JobId;

        }

        private static bool CannotUpdateView(KeySearcher keySearcher, KeySearcherSettings keySearcherSettings)
        {
            return keySearcher.Pattern == null || !keySearcher.Pattern.testWildcardKey(keySearcherSettings.Key) || keySearcherSettings.NumberOfBlocks == 0;
        }

        #region local calculation

        public void StartedLocalCalculation(BigInteger blockId)
        {
            if (CurrentChunks.Contains(blockId))
            {
                return;
            }

            CurrentChunks.Add(blockId);

            lock (this)
            {
                if (CurrentChunks.Count > CrypCloudCore.Instance.AmountOfWorker)
                {
                    IEnumerable<BigInteger> tasksToRemove = CurrentChunks.Take(CurrentChunks.Count - CrypCloudCore.Instance.AmountOfWorker);
                    foreach (BigInteger bigInteger in tasksToRemove)
                    {
                        CurrentChunks.Remove(bigInteger);
                    }
                }
            }


            OnPropertyChanged("CurrentChunks");
        }

        public void EndedLocalCalculation(TaskEventArgs taskArgs)
        {
            lock (this)
            {
                BigInteger itemInList = CurrentChunks.FirstOrDefault(it => it == taskArgs.BlockID);
                if (itemInList != default(BigInteger))
                {
                    CurrentChunks.Remove(itemInList);
                }

                if (itemInList == 0 && CurrentChunks.Contains(0))
                {
                    CurrentChunks.Remove(0);
                }
            }
            OnPropertyChanged("CurrentChunks");

            if (taskArgs.Type == TaskEventArgType.Finished)
            {
                LocalFinishedChunks++;
            }
            else
            {
                LocalAbortChunks++;
            }
        }

        #endregion

        public void BlockHasBeenFinished()
        {
            GlobalSpeedStatistics.AddEntry(KeysPerBlock);
        }

        /// <summary>
        /// Updates the global progress
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="keyResultEntries"></param>
        public void UpdateGlobalProgress(JobProgressEventArgs progress, List<KeyResultEntry> keyResultEntries)
        {
            FinishedNumberOfBlocks = progress.NumberOfCalculatedBlocks;
            GlobalProgress = 100 * progress.NumberOfCalculatedBlocks.DivideAndReturnDouble(progress.NumberOfBlocks);
            numberOfLeftBlocks = progress.NumberOfBlocks - progress.NumberOfCalculatedBlocks;
            FillTopList(keyResultEntries);
            OnPropertyChanged("GlobalProgressString");
        }

        public void UpdateGlobalSpeed(BigInteger keysPerSecond)
        {
            KeysPerSecondGlobal = keysPerSecond;
            if (keysPerSecond != 0)
            {
                double timePerBlock = (KeysPerBlock.DivideAndReturnDouble(KeysPerSecondGlobal / CrypCloudCore.Instance.AmountOfWorker));
                AvgTimePerChunkGlobal = TimeSpan.FromSeconds(timePerBlock);
                GlobalThroughputInBytes = keysPerSecond * BytesToUse;
            }

            if (numberOfLeftBlocks == -1)
            {
                return;
            }

            try
            {
                double remainingSeconds = (double)((numberOfLeftBlocks * KeysPerBlock) / keysPerSecond);
                RemainingTimeTotal = TimeSpan.FromSeconds(remainingSeconds);
                EstimatedFinishDate = DateTime.Now.Add(RemainingTimeTotal);
            }
            catch (Exception)
            {
                RemainingTimeTotal = TimeSpan.MaxValue;
                EstimatedFinishDate = DateTime.MaxValue;
            }
        }


        public void UpdateLocalSpeed(BigInteger localApproximateKeysPerSecond)
        {
            KeysPerSecond = localApproximateKeysPerSecond;
            if (keysPerSecond != 0)
            {
                double timePerBlock = (KeysPerBlock.DivideAndReturnDouble(localApproximateKeysPerSecond));
                AvgTimePerChunk = TimeSpan.FromSeconds(timePerBlock);
                LocalThroughputInBytes = KeysPerSecond * BytesToUse;
            }
        }


        private void FillTopList(List<KeyResultEntry> keyResultEntries)
        {
            TopList.Clear();
            //keyResultEntries.Sort();            
            foreach (KeyResultEntry key in keyResultEntries)
            {
                TopList.Add(key);
            }
        }

        #region properties with propChange handler

        public string JobName
        {
            get
            {
                if (jobName == null || jobName.Length < 40)
                {
                    return jobName;
                }

                return jobName.Substring(0, 40) + "...";
            }
            set
            {
                jobName = value;
                OnPropertyChanged("JobName");
            }
        }


        public string JobDesc
        {
            get => jobDesc;
            set
            {
                jobDesc = value;
                OnPropertyChanged("JobDesc");
            }
        }
        public string CurrentOperation
        {
            get => currentOperation;
            set
            {
                currentOperation = value;
                OnPropertyChanged("CurrentOperation");
            }
        }

        public BigInteger LocalFinishedChunks
        {
            get => localFinishedChunks;
            set
            {
                localFinishedChunks = value;
                OnPropertyChanged("LocalFinishedChunks");
            }
        }

        public BigInteger LocalAbortChunks
        {
            get => localAbortChunks;
            set
            {
                localAbortChunks = value;
                OnPropertyChanged("LocalAbortChunks");
            }
        }

        public BigInteger JobID
        {
            get => jobID;
            set
            {
                jobID = value;
                OnPropertyChanged("JobID");
            }
        }


        public double GlobalProgress
        {
            get => globalProgress;
            set
            {
                globalProgress = value;
                OnPropertyChanged("GlobalProgress");
            }
        }

        public BigInteger KeysPerSecond
        {
            get => keysPerSecond;
            set
            {
                keysPerSecond = value;
                OnPropertyChanged("KeysPerSecond");
            }
        }

        public DateTime StartDate
        {
            get => startDate;
            set
            {
                startDate = value;
                OnPropertyChanged("StartDate");
            }
        }

        public TimeSpan ElapsedTime
        {
            get => elapsedTime;
            set
            {
                elapsedTime = value;
                OnPropertyChanged("ElapsedTime");
            }
        }

        public TimeSpan AvgTimePerChunk
        {
            get => avgTimePerChunk;
            set
            {
                avgTimePerChunk = value;
                OnPropertyChanged("AvgTimePerChunk");
            }
        }

        public TimeSpan AvgTimePerChunkGlobal
        {
            get => avgTimePerChunkGlobal;
            set
            {
                avgTimePerChunkGlobal = value;
                OnPropertyChanged("AvgTimePerChunkGlobal");
            }
        }

        public TimeSpan RemainingTime
        {
            get => remainingTime;
            set
            {
                remainingTime = value;
                OnPropertyChanged("RemainingTime");
            }
        }

        public TimeSpan RemainingTimeTotal
        {
            get => remainingTimeTotal;
            set
            {
                remainingTimeTotal = value;
                OnPropertyChanged("RemainingTimeTotal");
            }
        }

        public DateTime EstimatedFinishDate
        {
            get => estimatedFinishDate;
            set
            {
                estimatedFinishDate = value;
                OnPropertyChanged("EstimatedFinishDate");
            }
        }

        public BigInteger TotalAmountOfChunks
        {
            get => totalAmountOfChunks;
            set
            {
                totalAmountOfChunks = value;
                OnPropertyChanged("TotalAmountOfChunks");
            }
        }

        public BigInteger KeysPerBlock
        {
            get => keysPerBlock;
            set
            {
                keysPerBlock = value;
                OnPropertyChanged("KeysPerBlock");
                OnPropertyChanged("Dataspace");
            }
        }
        public BigInteger FinishedNumberOfBlocks
        {
            get => finishedNumberOfBlocks;
            set
            {
                finishedNumberOfBlocks = value;
                OnPropertyChanged("FinishedNumberOfBlocks");
            }
        }

        public string GlobalProgressString
        {
            get
            {
                if (TotalAmountOfChunks == 0)
                {
                    return "~";
                }

                string doneBlocks = FinishedNumberOfBlocks.ToString("N0", new CultureInfo("de-DE"));
                string totalBlocks = TotalAmountOfChunks.ToString("N0", new CultureInfo("de-DE"));

                return string.Format("{0} / {1}", doneBlocks, totalBlocks);
            }
            set { } //for binding only
        }

        public BigInteger KeysPerSecondGlobal
        {
            get => keysPerSecondGlobal;
            set
            {
                keysPerSecondGlobal = value;
                OnPropertyChanged("KeysPerSecondGlobal");
            }
        }

        public string Dataspace
        {
            get
            {
                BigInteger dataspace = KeysPerBlock * TotalAmountOfChunks * BytesToUse;
                return BytesToString(dataspace);
            }
            set { }
        }

        public int BytesToUse { get; set; }


        public BigInteger GlobalThroughputInBytes
        {
            get => globalThroughputInBytes;
            set
            {
                globalThroughputInBytes = value;
                OnPropertyChanged("GlobalThroughputInBytes");
                OnPropertyChanged("GlobalThroughput");
            }
        }
        public BigInteger LocalThroughputInBytes
        {
            get => localThroughputInBytes;
            set
            {
                localThroughputInBytes = value;
                OnPropertyChanged("LocalThroughputInBytes");
                OnPropertyChanged("LocalThroughput");
            }
        }

        public string LocalThroughput
        {
            get
            {
                if (LocalThroughputInBytes == 0)
                {
                    return "~";
                }

                return BytesToString(LocalThroughputInBytes) + "/sec";
            }
            set { } //for binding only
        }

        public string GlobalThroughput
        {
            get
            {
                if (GlobalThroughputInBytes == 0)
                {
                    return "~";
                }

                return BytesToString(GlobalThroughputInBytes) + "/sec";
            }
            set { } //for binding only
        }


        public static string BytesToString(BigInteger byteCount)
        {
            string[] suf = { "B", "kB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB", "HB" };
            if (byteCount == 0)
            {
                return "~";
            }

            BigInteger bytes = BigInteger.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(BigInteger.Log(bytes, 1024)));
            double num = bytes.DivideAndReturnDouble(new BigInteger(Math.Pow(1024, place)));
            return "" + Math.Round(num, 3) + " " + suf[place];
        }

        #endregion

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion


    }
}
