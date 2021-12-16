/*
   Copyright 2008 Timo Eckhardt, University of Siegen

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

namespace Primes.WpfControls.Threads
{
    using System.Threading;

    public abstract class SuspendableThread
    {
        #region Data

        private readonly ManualResetEvent suspendChangedEvent = new ManualResetEvent(false);

        private readonly ManualResetEvent terminateEvent = new ManualResetEvent(false);

        private long suspended;

        private Thread thread;

        protected ThreadPriority m_Priority;

        public Thread Thread
        {
            get => thread;
            set => thread = value;
        }

        private System.Threading.ThreadState failsafeThreadState = System.Threading.ThreadState.Unstarted;

        #endregion Data

        public SuspendableThread()
        {
            m_Priority = ThreadPriority.Normal;
        }

        private void ThreadEntry()
        {
            failsafeThreadState = System.Threading.ThreadState.Stopped;
            OnDoWork();
        }

        protected abstract void OnDoWork();

        #region Protected methods

        protected bool SuspendIfNeeded()
        {
            bool suspendEventChanged = suspendChangedEvent.WaitOne(0, true);

            if (suspendEventChanged)
            {
                bool needToSuspend = Interlocked.Read(ref suspended) != 0;

                suspendChangedEvent.Reset();

                if (needToSuspend)
                {
                    /// Suspending...

                    if (1 == WaitHandle.WaitAny(new WaitHandle[] { suspendChangedEvent, terminateEvent }))
                    {
                        return true;
                    }

                    /// ...Waking
                }
            }

            return false;
        }

        protected bool HasTerminateRequest()
        {
            return terminateEvent.WaitOne(0, true);
        }

        #endregion Protected methods

        public void Start()
        {
            thread = new Thread(new ThreadStart(ThreadEntry))
            {
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture,
                Priority = m_Priority,

                // make sure this thread won't be automaticaly
                // terminated by the runtime when the
                // application exits

                IsBackground = false
            };

            thread.Start();
        }

        public void Join()
        {
            if (thread != null)
            {
                thread.Join();
            }
        }

        public bool Join(int milliseconds)
        {
            if (thread != null)
            {
                return thread.Join(milliseconds);
            }

            return true;
        }

        /// <remarks>Not supported in .NET Compact Framework</remarks>

        public bool Join(TimeSpan timeSpan)
        {
            if (thread != null)
            {
                return thread.Join(timeSpan);
            }

            return true;
        }

        public void Terminate()
        {
            terminateEvent.Set();
            if (thread != null)
            {
                thread.Abort();
            }
        }

        public void TerminateAndWait()
        {
            terminateEvent.Set();
            thread.Join();
        }

        public void Suspend()
        {
            while (1 != Interlocked.Exchange(ref suspended, 1))
            {
            }

            suspendChangedEvent.Set();
        }

        public void Resume()
        {
            while (0 != Interlocked.Exchange(ref suspended, 0))
            {
            }

            suspendChangedEvent.Set();
        }

        public System.Threading.ThreadState ThreadState
        {
            get
            {
                if (null != thread)
                {
                    return thread.ThreadState;
                }

                return failsafeThreadState;
            }
        }
    }
}
