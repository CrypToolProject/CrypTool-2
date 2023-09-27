/*
   Copyright CrypTool 2 Team josef.matwich@gmail.com

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
