using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using KeySearcher.KeyPattern;
using KeySearcherPresentation.Controls;
using Timer = System.Timers.Timer;

namespace KeySearcher.P2P.Presentation
{
    class StatisticsGenerator
    {
        public readonly BigInteger TotalAmountOfChunks;
        private readonly StatusContainer status;
        private readonly P2PQuickWatchPresentation quickWatch;
        private readonly KeySearcher keySearcher;
        private readonly DistributedBruteForceManager distributedBruteForceManager;
        private readonly Stopwatch stopWatch;
        private readonly Timer elapsedTimeTimer;
        private readonly Timer trafficUpdateTimer;

        public BigInteger HighestChunkCalculated;
        private DateTime lastDateOfGlobalStatistics;
        private BigInteger totalRequestsAtStartOfNodeSearch;

        public StatisticsGenerator(StatusContainer status, P2PQuickWatchPresentation quickWatch, KeySearcher keySearcher, KeySearcherSettings settings, DistributedBruteForceManager distributedBruteForceManager)
        {
            this.status = status;
            this.quickWatch = quickWatch;
            this.keySearcher = keySearcher;
            this.distributedBruteForceManager = distributedBruteForceManager;

            lastDateOfGlobalStatistics = DateTime.Now;
            HighestChunkCalculated = -1;
            stopWatch = new Stopwatch();

            var keyPattern = new KeyPattern.KeyPattern(keySearcher.ControlMaster.GetKeyPattern())
                                 {WildcardKey = settings.Key};
            var keysPerChunk = Math.Pow(2, settings.NumberOfBlocks);
            var keyPatternPool = new KeyPatternPool(keyPattern, new BigInteger(keysPerChunk));

            TotalAmountOfChunks = keyPatternPool.Length;

            status.PropertyChanged += StatusPropertyChanged;

            elapsedTimeTimer = new Timer(1000);
            elapsedTimeTimer.Elapsed += ElapsedTimeTimerTick;
            elapsedTimeTimer.Start();

            trafficUpdateTimer = new Timer(10000);
            trafficUpdateTimer.Elapsed += TrafficUpdateTimerTick;
            trafficUpdateTimer.Start();
        }

        public void CalculationStopped()
        {
            elapsedTimeTimer.Stop();
            trafficUpdateTimer.Stop();
        }

        void TrafficUpdateTimerTick(object sender, System.Timers.ElapsedEventArgs e)
        {
            UpdateTrafficStatistics();
        }

        void ElapsedTimeTimerTick(object sender, EventArgs e)
        {
            if (status.StartDate != DateTime.MinValue)
                status.ElapsedTime = DateTime.Now.Subtract(status.StartDate);

            if (status.RemainingTimeTotal > new TimeSpan(0))
                status.RemainingTimeTotal = status.RemainingTimeTotal.Subtract(TimeSpan.FromSeconds(1));
        }

        void StatusPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "DhtOverheadInReadableTime":
                    HandleUpdateOfOverheadTime();
                    break;
                case "StoredBytes":
                    UpdateTrafficStatistics();
                    break;
                case "RetrievedBytes":
                    UpdateTrafficStatistics();
                    break;
            }
        }

        private void UpdateTrafficStatistics()
        {
          
        }

        private void HandleUpdateOfOverheadTime()
        {
            if (distributedBruteForceManager.StopWatch.Elapsed.Ticks == 0)
            {
                status.DhtOverheadInPercent = "0 %";
                return;
            }

            var overheadInTicks = (double)status.DhtOverheadInReadableTime.Ticks /
                           distributedBruteForceManager.StopWatch.Elapsed.Ticks;
            var overheadInPercent = overheadInTicks * 100;
            overheadInPercent = Math.Round(overheadInPercent, 2);
            
            status.DhtOverheadInPercent = overheadInPercent + " %";
        }

        public void MarkStartOfNodeSearch()
        {
            totalRequestsAtStartOfNodeSearch = status.TotalDhtRequests;
            stopWatch.Start();
        }

        public void MarkEndOfNodeSearch()
        {
            stopWatch.Stop();
            var elapsedTime = stopWatch.Elapsed.Add(status.DhtOverheadInReadableTime);
            status.DhtOverheadInReadableTime = new TimeSpan(((long) Math.Round((1.0*elapsedTime.Ticks/5))*5));
            stopWatch.Reset();
            
            var requestsForThisNode = status.TotalDhtRequests - totalRequestsAtStartOfNodeSearch;

            if (status.RequestsPerNode == 0)
            {
                status.RequestsPerNode = requestsForThisNode;
                return;
            }

            status.RequestsPerNode = (status.RequestsPerNode + requestsForThisNode)/2;
        }

        public void CalculateGlobalStatistics(BigInteger currentChunkCalculated)
        {
            if (HighestChunkCalculated == -1) HighestChunkCalculated = currentChunkCalculated;
            if (currentChunkCalculated < HighestChunkCalculated) return;

            var totalAmountOfParticipants = currentChunkCalculated - HighestChunkCalculated;
            if (totalAmountOfParticipants == 0)
                totalAmountOfParticipants = 1;
            
            status.TotalAmountOfParticipants = totalAmountOfParticipants;


            var timeUsedForLatestProgress = DateTime.Now.Subtract(lastDateOfGlobalStatistics);
            var secondsForOneChunk = timeUsedForLatestProgress.TotalSeconds/(double) totalAmountOfParticipants;
            var remainingChunks = TotalAmountOfChunks - currentChunkCalculated;
            var secondsRemaining = (double) remainingChunks*secondsForOneChunk;

            try
            {
                var estimatedFinishDate = DateTime.Now.AddSeconds(secondsRemaining);
                status.EstimatedFinishDate = estimatedFinishDate.ToString("g", Thread.CurrentThread.CurrentCulture);
                status.RemainingTimeTotal = estimatedFinishDate.Subtract(DateTime.Now);
            }
            catch (ArgumentOutOfRangeException)
            {
                status.RemainingTimeTotal = TimeSpan.MaxValue;
                var yearsRemaining = secondsRemaining/60/60/24/365;
                status.EstimatedFinishDate = string.Format("{0:0.00e+0} years", yearsRemaining);
            }

            lastDateOfGlobalStatistics = DateTime.Now;

            HighestChunkCalculated = currentChunkCalculated;
            status.GlobalProgress = (double) HighestChunkCalculated/(double) TotalAmountOfChunks;
            keySearcher.ProgressChanged(status.GlobalProgress, 1);
        }

        public void ProcessPatternResults(LinkedList<KeySearcher.ValueKey> result)
        {
            ProcessResultList(result);
        }

        public void ShowProgress(LinkedList<KeySearcher.ValueKey> bestResultList, BigInteger keysInThisChunk, BigInteger keysFinishedInThisChunk, long keysPerSecond)
        {
            status.ProgressOfCurrentChunk = (double) keysFinishedInThisChunk/(double) keysInThisChunk;
            status.KeysPerSecond = keysPerSecond;

            var time = (Math.Pow(10, BigInteger.Log((keysInThisChunk - keysFinishedInThisChunk), 10) - Math.Log10(keysPerSecond)));
            var timeleft = new TimeSpan(-1);

            try
            {
                if (time / (24 * 60 * 60) <= int.MaxValue)
                {
                    var days = (int)(time / (24 * 60 * 60));
                    time = time - (days * 24 * 60 * 60);
                    var hours = (int)(time / (60 * 60));
                    time = time - (hours * 60 * 60);
                    var minutes = (int)(time / 60);
                    time = time - (minutes * 60);
                    var seconds = (int)time;

                    timeleft = new TimeSpan(days, hours, minutes, seconds, 0);
                }
            }
            catch
            {
                //can not calculate time span
            }

            status.RemainingTime = timeleft;

            ProcessResultList(bestResultList);
        }

        private void ProcessResultList(LinkedList<KeySearcher.ValueKey> bestResultList)
        {
            quickWatch.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (SendOrPostCallback)delegate
            {

                var enc = Encoding.UTF8;
                LinkedListNode<KeySearcher.ValueKey> linkedListNode;
                status.TopList.Clear();
                linkedListNode = bestResultList.First;

                int i = 0;
                while (linkedListNode != null && i < 10)
                {
                    i++;

                    var entry = new ResultEntry();
                    entry.Ranking = i.ToString();
                    entry.Value = Math.Round(linkedListNode.Value.value, 2).ToString();
                    entry.Key = linkedListNode.Value.key;
                    var plainText = enc.GetString(linkedListNode.Value.decryption);

                    const string replaceWith = "";
                    plainText = plainText.Replace("\r\n", replaceWith).Replace("\n", replaceWith).Replace("\r", replaceWith);
                    if (plainText.Length > 30)
                        plainText = plainText.Substring(0, 30) + "...";

                    entry.Text = plainText;

                    entry.User = linkedListNode.Value.user;
                    entry.Time = linkedListNode.Value.time;
                    entry.Maschid = linkedListNode.Value.maschid;
                    entry.Maschname = linkedListNode.Value.maschname;

                    status.TopList.Add(entry);
                    linkedListNode = linkedListNode.Next;
                }
            }, null);
        }
    }
}
