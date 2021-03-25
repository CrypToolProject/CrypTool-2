using System;
using System.ComponentModel;

namespace CrypTool.Plugins.MD5Collider.Algorithm
{
    /// <summary>
    /// Interface providing access to the important properties and methods of a collision search algorithm
    /// </summary>
    public interface IMD5ColliderAlgorithm : INotifyPropertyChanged
    {
        /// <summary>
        /// First resulting block retrievable after collision is found
        /// </summary>
        byte[] FirstCollidingData { get; }

        /// <summary>
        /// Second resulting block retrievable after collision is found
        /// </summary>
        byte[] SecondCollidingData { get; }

        /// <summary>
        /// Byte array containing arbitrary data used to initialize the RNG
        /// </summary>
        byte[] RandomSeed { set; }

        /// <summary>
        /// IHV (intermediate hash value) for the start of the collision, must be initialized if prefix is desired
        /// </summary>
        byte[] IHV { set; }

        /// <summary>
        /// Number of conditions which have failed
        /// </summary>
        long CombinationsTried { get; }

        /// <summary>
        /// Time elapsed since start of collision search
        /// </summary>
        TimeSpan ElapsedTime { get; }

        /// <summary>
        /// Starts the collision search
        /// </summary>
        void FindCollision();

        /// <summary>
        /// Stops the collision search
        /// </summary>
        void Stop();

        /// <summary>
        /// Maximum possible value for match progress
        /// </summary>
        int MatchProgressMax { get; }

        /// <summary>
        /// Indicates how far conditions for a valid collision block were satisfied in last attempt
        /// </summary>
        int MatchProgress { get; }
    }
}
