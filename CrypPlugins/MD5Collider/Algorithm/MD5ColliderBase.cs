using System;
using System.Timers;

namespace CrypTool.Plugins.MD5Collider.Algorithm
{
    /// <summary>
    /// An abstract base class for regular implementations of <c>IMD5ColliderAlgorithm</c>
    /// </summary>
    /// <seealso cref="IMD5ColliderAlgorithm"/>
    internal abstract class MD5ColliderBase : IMD5ColliderAlgorithm
    {
        /// <summary>
        /// First resulting block retrievable after collision is found
        /// </summary>
        public byte[] FirstCollidingData { get; protected set; }

        /// <summary>
        /// Second resulting block retrievable after collision is found
        /// </summary>
        public byte[] SecondCollidingData { get; protected set; }

        /// <summary>
        /// Byte array containing arbitrary data used to initialize the RNG
        /// </summary>
        public byte[] RandomSeed { protected get; set; }

        /// <summary>
        /// IHV (intermediate hash value) for the start of the collision, must be initialized if prefix is desired
        /// </summary>
        public byte[] IHV { protected get; set; }

        /// <summary>
        /// Initializes progress to default values and sets up timers
        /// </summary>
        public MD5ColliderBase()
        {
            MatchProgressMax = 1;
            MatchProgress = 0;

            progressUpdateTimer.Interval = 100;
            progressUpdateTimer.Elapsed += progressUpdateTimer_Tick;

            timer.Interval = 1000;
            timer.Elapsed += new ElapsedEventHandler(timer_Tick);
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

        /// <summary>
        /// Maximum possible value for match progress
        /// </summary>
        public int MatchProgressMax { get; set; }

        /// <summary>
        /// Indicates how far conditions for a valid collision block were satisfied in last attempt
        /// </summary>
        public int MatchProgress { get; set; }

        /// <summary>
        /// Timer to regularily trigger a <c>PropertyChanged</c> event for public properties
        /// </summary>
        private readonly Timer progressUpdateTimer = new Timer();

        /// <summary>
        /// Tick event handler for the timer object <c>progressUpdateTimer</c>, calls <c>UpdateProgress</c>
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void progressUpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateProgress();
        }

        /// <summary>
        /// Performs the collision search, to be implemented in subclasses.
        /// </summary>
        protected abstract void PerformFindCollision();

        /// <summary>
        /// Stops the collision search, to be implemented in subclasses.
        /// </summary>
        protected abstract void PerformStop();

        /// <summary>
        /// The time at which the search was started
        /// </summary>
        private DateTime startTime;

        /// <summary>
        /// The timer for updating the elapsed time
        /// </summary>
        private readonly Timer timer = new Timer();

        /// <summary>
        /// Starts the two timers
        /// </summary>
        private void StartTimer()
        {
            startTime = DateTime.Now;
            ElapsedTime = TimeSpan.Zero;

            timer.Start();
            progressUpdateTimer.Start();
        }

        /// <summary>
        /// Tick event of <c>timer</c> object, updating the elapsed time
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void timer_Tick(object sender, EventArgs e)
        {
            ElapsedTime = DateTime.Now - startTime;
        }

        /// <summary>
        /// Stops the two timers
        /// </summary>
        private void StopTimer()
        {
            timer.Stop();
            progressUpdateTimer.Stop();

            UpdateProgress();
        }

        /// <summary>
        /// Triggers <c>PropertyChanged</c> for progress indicator properties
        /// </summary>
        private void UpdateProgress()
        {
            OnPropertyChanged("MatchProgressMax");
            OnPropertyChanged("MatchProgress");
            OnPropertyChanged("CombinationsTried");
        }

        /// <summary>
        /// Starts the collision search
        /// </summary>
        public void FindCollision()
        {
            CombinationsTried = 0;

            CheckRandomSeed();
            CheckIHV();

            StartTimer();
            PerformFindCollision();
            StopTimer();
        }

        /// <summary>
        /// Allows subclasses to register a return because of a failed condition
        /// </summary>
        /// <param name="progress">Number indicating how late the failed condition was</param>
        public void LogReturn(int progress)
        {
            MatchProgress = progress;
            CombinationsTried++;
        }

        /// <summary>
        /// Determines if a valid IHV has been given, if not, assumes the standard IHV
        /// </summary>
        private void CheckIHV()
        {
            if (IHV == null || IHV.Length != 16)
            {
                IHV = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10 };
            }
        }

        /// <summary>
        /// Determines if a random seed has been given, if not, generate one
        /// </summary>
        private void CheckRandomSeed()
        {
            if (RandomSeed == null)
            {
                RandomSeed = new byte[35];
                new Random().NextBytes(RandomSeed);
            }
        }


        /// <summary>
        /// Stops the collision search
        /// </summary>       
        public void Stop()
        {
            PerformStop();
            StopTimer();
        }

        /// <summary>
        /// Contains the elapsed time
        /// </summary>
        private TimeSpan _elapsedTime;

        /// <summary>
        /// Property exposing elapsed time
        /// </summary>
        public TimeSpan ElapsedTime
        {
            get => _elapsedTime;
            set { _elapsedTime = value; OnPropertyChanged("ElapsedTime"); }
        }

        /// <summary>
        /// Number of conditions which have failed
        /// </summary>
        public long CombinationsTried { get; protected set; }
    }
}
