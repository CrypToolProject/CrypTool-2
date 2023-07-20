using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace M209AnalyzerLib.Common
{
    public class Runnables
    {
        private List<Task> runnables = new List<Task>();

        public void AddRunnable(Task runnable)
        {
            runnables.Add(runnable);
        }

        public void Run()
        {
            long start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            foreach (Task runnable in runnables)
            {
                runnable.Start();
            }

            long end = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        }
    }
}
