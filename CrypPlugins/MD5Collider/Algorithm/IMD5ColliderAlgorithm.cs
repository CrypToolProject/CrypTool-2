/*
   Copyright (C) CrypTool 2 Team

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
