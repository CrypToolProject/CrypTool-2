using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace KeySearcher.CrypCloud.statistics
{
    public class SpeedStatistics
    {
        private readonly DateTime _startTime = DateTime.Now;
        public int MinutesUntilEntryInvalidates { get; set; } = 30;
        public int MinutesUntilStartCollecting { get; set; } = 2;

        public SpeedStatistics(int minutesTillInvalidate, int minutesUntilStartCollecting)
        {
            MinutesUntilEntryInvalidates = minutesTillInvalidate;
            MinutesUntilStartCollecting = minutesUntilStartCollecting;
        }

        private readonly DateTime statisticsStartTime = DateTime.UtcNow;
        private readonly List<SpeedStatisticsEntry> calculations = new List<SpeedStatisticsEntry>();

        public void AddEntry(BigInteger numberOfKeysCalculated)
        {
            if (DateTime.Now < _startTime.AddMinutes(MinutesUntilStartCollecting))
            {
                //we ignore the received blocks for the statistic at start since we 
                //could get too many during the connection phase
                return;
            }
            SpeedStatisticsEntry entry = new SpeedStatisticsEntry
            {
                NumberOfKeysInBlock = numberOfKeysCalculated,
                InvalidatesAt = DateTime.UtcNow.AddMinutes(MinutesUntilEntryInvalidates)
            };

            lock (this)
            {
                calculations.Add(entry);
            }
        }


        /// <summary>
        /// Approximates the speed of the calcuation by constructing the avg of all entrys received within the last 30 minutes
        /// </summary>
        /// <returns></returns>
        public BigInteger ApproximateKeysPerSecond()
        {
            BigInteger calculatedKeys;
            lock (this)
            {
                calculations.RemoveAll(it => it.InvalidatesAt < DateTime.UtcNow);
                calculatedKeys = calculations.Aggregate(new BigInteger(0), (prev, it) => prev + it.NumberOfKeysInBlock);
            }

            int seconds = MinutesUntilEntryInvalidates * 60;
            if (statisticsStartTime.AddMinutes(MinutesUntilEntryInvalidates) > DateTime.UtcNow)
            {
                TimeSpan timeSpan = (DateTime.UtcNow - statisticsStartTime);
                seconds = (int)timeSpan.TotalSeconds;
            }

            if (seconds == 0)
            {
                return 0;
            }
            return calculatedKeys / seconds;
        }
    }

    internal class SpeedStatisticsEntry
    {
        public DateTime InvalidatesAt { get; set; }

        public BigInteger NumberOfKeysInBlock { get; set; }
    }

}
