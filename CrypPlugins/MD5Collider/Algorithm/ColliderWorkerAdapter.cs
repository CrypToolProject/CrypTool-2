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
using System.ComponentModel;

namespace CrypTool.Plugins.MD5Collider.Algorithm
{
    /// <summary>
    /// Manager for a worker thread that runs a <c>IMD5ColliderAlgorithm</c> belonging to a <c>MultiThreadedMD5Collider</c>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ColliderWorkerAdapter<T> where T : IMD5ColliderAlgorithm, new()
    {
        /// <summary>
        /// The collider class which is run
        /// </summary>
        private readonly IMD5ColliderAlgorithm wrappedCollider;

        /// <summary>
        /// The multi-threaded collider to which this adapter belongs
        /// </summary>
        private readonly MultiThreadedMD5Collider<T> multiThreadedCollider;

        /// <summary>
        /// The managed <c>BackgroundWorker</c> object
        /// </summary>
        private readonly BackgroundWorker worker;

        /// <summary>
        /// Indicates whether the worker is running
        /// </summary>
        public bool IsStarted => worker.IsBusy;

        /// <summary>
        /// Constructs the adapter, giving the multi-threaded collider it belongs to and the collider object to execute in the managed thread.
        /// </summary>
        /// <param name="multiThreadedCollider">The multi-threaded collider to which this adapter belongs</param>
        /// <param name="wrappedCollider">The collider which is executed in the managed thread</param>
        public ColliderWorkerAdapter(MultiThreadedMD5Collider<T> multiThreadedCollider, IMD5ColliderAlgorithm wrappedCollider)
        {
            this.multiThreadedCollider = multiThreadedCollider;
            this.wrappedCollider = wrappedCollider;

            worker = new BackgroundWorker();
            worker.DoWork += DoWork;
            worker.RunWorkerCompleted += RunWorkerCompleted;
        }

        /// <summary>
        /// Delegate which the managed <c>BackgroundWorker</c> object calls to perform work
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void DoWork(object sender, DoWorkEventArgs e)
        {
            wrappedCollider.FindCollision();
        }

        /// <summary>
        /// Delegate which the managed <c>BackgroundWorker</c> object calls when work is finished
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            multiThreadedCollider.SignalWorkIsFinished(wrappedCollider);
        }

        /// <summary>
        /// Starts the managed worker thread
        /// </summary>
        public void StartWork()
        {
            worker.RunWorkerAsync();
        }
    }
}
