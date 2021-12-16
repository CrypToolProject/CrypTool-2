/*
   Copyright 2020 George Lasry
   Converted in 2020 from Java to C# by Nils Kopal

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
using System.Collections.Generic;
using System.Threading;

namespace EnigmaAnalyzerLib.Common
{
    public abstract class Runnable
    {
        public abstract void run();
        public abstract void stop();
    }

    public class Runnables
    {
        private readonly List<Runnable> runnables = new List<Runnable>();
        private CountdownEvent countdownEvent;
        private readonly object lockObject = new object();

        public int Count => runnables.Count;

        public void addRunnable(Runnable runnable)
        {
            runnables.Add(runnable);
        }

        public void run(int threads, ResultReporter resultReporter, bool showProgress)
        {
            DateTime start = DateTime.Now;
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(threads, threads);

            using (countdownEvent = new CountdownEvent(runnables.Count))
            {
                int workerid = 0;
                foreach (Runnable runnable in runnables)
                {
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        try
                        {
                            runnable.run();
                            workerid++;
                        }
                        catch (Exception ex)
                        {
                            resultReporter.WriteException(string.Format("Exception occured during execution of thread: {0}", ex.Message), ex);
                        }
                        countdownEvent.Signal();
                        lock (lockObject)
                        {
                            if (showProgress)
                            {
                                resultReporter.displayProgress(countdownEvent.InitialCount - countdownEvent.CurrentCount, countdownEvent.InitialCount);
                            }
                        }
                    });
                }

                //wait for all threads to terminate                     
                while (countdownEvent.CurrentCount > 0)
                {
                    try
                    {
                        countdownEvent.Wait(1000);
                    }
                    catch (Exception)
                    {
                        //do nothing
                    }
                    //Check, if user wants to stop
                    if (resultReporter.ShouldTerminate)
                    {
                        Console.WriteLine("resultReporters signals that we should terminate. We inform all workers");
                        //inform all workers
                        foreach (Runnable runnable in runnables)
                        {
                            runnable.stop();
                        }
                    }
                }
            }
        }
    }
}