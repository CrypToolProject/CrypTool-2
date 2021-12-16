using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace CrypTool.Plugins.MD5Collider.Algorithm
{
    /// <summary>
    /// Wraps an existing <c>IMD5ColliderAlgorithm</c> implementation to execute it in parallel using multiple threads
    /// </summary>
    /// <typeparam name="T">Type of collider to run in parallel</typeparam>
    internal class MultiThreadedMD5Collider<T> : IMD5ColliderAlgorithm where T : IMD5ColliderAlgorithm, new()
    {
        /// <summary>
        /// A list of <c>ColliderWorkerAdapter</c> which manage the threads for the collider instances
        /// </summary>
        /// <seealso cref="ColliderWorkerAdapter"/>
        private readonly List<ColliderWorkerAdapter<T>> workers = new List<ColliderWorkerAdapter<T>>();

        /// <summary>
        /// The managed container instances
        /// </summary>
        private readonly List<IMD5ColliderAlgorithm> colliders = new List<IMD5ColliderAlgorithm>();

        /// <summary>
        /// The collider which finished first
        /// </summary>
        private IMD5ColliderAlgorithm successfulCollider;

        /// <summary>
        /// Timer periodically emitting <c>PropertyChanged</c> events for progress notification properties
        /// </summary>
        private readonly Timer progressUpdateTimer;

        /// <summary>
        /// Amount of worker threads managed
        /// </summary>
        private readonly int workerCount;

        /// <summary>
        /// Event triggered when the first collision has finished
        /// </summary>
        private readonly System.Threading.AutoResetEvent finishedEvent = new System.Threading.AutoResetEvent(false);

        /// <summary>
        /// Constructs as many managed colliders and worker threads as CPU cores are available and sets up the timer
        /// </summary>
        public MultiThreadedMD5Collider()
        {
            workerCount = Math.Max(Environment.ProcessorCount, 1);

            for (int i = 0; i < workerCount; i++)
            {
                IMD5ColliderAlgorithm collider = new T();
                colliders.Add(collider);

                ColliderWorkerAdapter<T> colliderWorkerAdapter = new ColliderWorkerAdapter<T>(this, collider);
                workers.Add(colliderWorkerAdapter);
            }

            progressUpdateTimer = new Timer
            {
                Interval = 250
            };
            progressUpdateTimer.Elapsed += progressUpdateTimer_Tick;
        }

        /// <summary>
        /// First resulting block as retrieved from finished collider
        /// </summary>
        public byte[] FirstCollidingData => successfulCollider != null ? successfulCollider.FirstCollidingData : null;

        /// <summary>
        /// Second resulting block as retrieved from finished collider
        /// </summary>
        public byte[] SecondCollidingData => successfulCollider != null ? successfulCollider.SecondCollidingData : null;

        /// <summary>
        /// Second resulting block as retrieved from finished collider
        /// </summary>
        public byte[] RandomSeed
        {
            set
            {
                if (value == null)
                {
                    foreach (IMD5ColliderAlgorithm collider in colliders)
                    {
                        collider.RandomSeed = null;
                    }

                    return;
                }


                byte[] randomSeedCopy = (byte[])value.Clone();

                foreach (IMD5ColliderAlgorithm collider in colliders)
                {
                    collider.RandomSeed = randomSeedCopy;

                    randomSeedCopy = (byte[])randomSeedCopy.Clone();

                    if (randomSeedCopy.Length == 0)
                    {
                        randomSeedCopy = new byte[1];
                    }

                    randomSeedCopy[0]++;
                }
            }
        }

        /// <summary>
        /// Mutex locked when a computation has finished
        /// </summary>
        private readonly object finishedLock = new object();

        /// <summary>
        /// Called by a <c>ColliderWorkerAdapter</c> when wrapped collider has finished
        /// </summary>
        /// <param name="successfulCollider"></param>
        internal void SignalWorkIsFinished(IMD5ColliderAlgorithm successfulCollider)
        {
            lock (finishedLock)
            {
                if (this.successfulCollider == null)
                {
                    this.successfulCollider = successfulCollider;
                    updateProgress();
                    Stop();

                    finishedEvent.Set();
                }
            }
        }

        /// <summary>
        /// IHV (intermediate hash value) for the start of the collision, must be initialized if prefix is desired
        /// </summary>
        public byte[] IHV
        {
            set
            {
                foreach (IMD5ColliderAlgorithm collider in colliders)
                {
                    collider.IHV = value;
                }
            }
        }

        /// <summary>
        /// Maximum possible value for match progress
        /// </summary>
        public int MatchProgressMax => colliders.Max(c => c.MatchProgressMax);

        /// <summary>
        /// Indicates how far conditions for a valid collision block were satisfied in last attempt
        /// </summary>
        public int MatchProgress
        {
            get => colliders.Max(c => c.MatchProgress);
            set { }
        }

        /// <summary>
        /// Number of conditions which have failed
        /// </summary>
        public long CombinationsTried => colliders.Sum(c => c.CombinationsTried);

        /// <summary>
        /// Time elapsed since start of collision search
        /// </summary>
        public TimeSpan ElapsedTime => colliders.Max(c => c.ElapsedTime);

        /// <summary>
        /// Starts the collision search
        /// </summary>
        public void FindCollision()
        {
            progressUpdateTimer.Start();

            finishedEvent.Reset();
            successfulCollider = null;

            foreach (ColliderWorkerAdapter<T> worker in workers)
            {
                worker.StartWork();
            }

            finishedEvent.WaitOne();

            OnPropertyChanged("FirstCollidingData");
            OnPropertyChanged("SecondCollidingData");
        }

        /// <summary>
        /// Stops the collision search
        /// </summary>
        public void Stop()
        {
            foreach (IMD5ColliderAlgorithm collider in colliders)
            {
                collider.Stop();
            }

            progressUpdateTimer.Stop();
        }

        /// <summary>
        /// The <c>PropertyChanged</c> event as prescribed by the <c>INotifyPropertyChanged</c> interface
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Helper function triggering <c>PropertyChanged</c> event for given property name
        /// </summary>
        /// <param name="propertyName">Property for which change event should be triggerd</param>
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }

        private void progressUpdateTimer_Tick(object sender, EventArgs e)
        {
            updateProgress();
        }

        private void updateProgress()
        {
            OnPropertyChanged("MatchProgressMax");
            OnPropertyChanged("MatchProgress");
            OnPropertyChanged("CombinationsTried");
            OnPropertyChanged("ElapsedTime");
        }
    }
}
