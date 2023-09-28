/*
   Copyright 2008 - 2022 CrypTool Team

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
using System.Diagnostics;
using System.Timers;

namespace CrypTool.CrypWin.Helper
{
    public class PriorityChangedListener
    {
        private static ProcessPriorityClass lastPriorityClass;
        public delegate void PriorityChangedDelegate(ProcessPriorityClass newPriority);

        public static event PriorityChangedDelegate PriorityChanged;

        static PriorityChangedListener()
        {
            lastPriorityClass = Process.GetCurrentProcess().PriorityClass;

            System.Timers.Timer timer = new Timer(1000);
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Start();
        }

        private static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Process.GetCurrentProcess().PriorityClass != lastPriorityClass)
            {
                lastPriorityClass = Process.GetCurrentProcess().PriorityClass;
                if (PriorityChanged != null)
                {
                    PriorityChanged(lastPriorityClass);
                }
            }
        }

    }
}
