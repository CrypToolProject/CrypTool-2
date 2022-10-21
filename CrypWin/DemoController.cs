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
using CrypTool.PluginBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace CrypTool.CrypWin
{
    public class DemoController
    {
        private readonly MainWindow window;
        private Thread thread;

        private bool isRunning;
        private bool shallStop;

        private string[] filelist;
        private int currentFile;

        private StreamWriter logWriter;

        public DemoController(MainWindow window)
        {
            this.window = window;
        }

        public bool IsRunning => isRunning;

        public void Stop()
        {
            shallStop = true;

            if (thread != null)
            {
                lock (thread)
                {
                    Monitor.PulseAll(thread);
                }
            }
        }

        private void DemoControl()
        {
            isRunning = true;

            while (!shallStop)
            {
                string file = filelist[currentFile];
                try
                {
                    window.OpenProjectInGuiThread(file);
                    lock (thread)
                    {
                        Monitor.Wait(thread, 5000);
                    }

                    // re-evaluate run condition
                    if (shallStop)
                    {
                        continue;
                    }

                    window.PlayProjectInGuiThread();
                    lock (thread)
                    {
                        Monitor.Wait(thread, Properties.Settings.Default.DemoInterval * 1000);
                    }

                    //Force garbage collection.
                    GC.Collect();
                    GC.WaitForFullGCComplete();

                    if (logWriter != null)
                    {
                        logWriter.WriteLine("*** Testing " + filelist[currentFile]);
                        logWriter.Flush();
                        IList<string> log = window.GetAllMessagesFromGuiThread(NotificationLevel.Error,
                                                                                NotificationLevel.Warning);
                        foreach (string msg in log)
                        {
                            logWriter.WriteLine(msg);
                            logWriter.Flush();
                        }
                    }

                    window.DeleteAllMessagesInGuiThread();
                    window.StopProjectInGuiThread();
                    window.CloseProjectInGuiThread();

                    if (++currentFile >= filelist.Length)
                    {
                        if (logWriter != null) // autotest running?
                        {
                            shallStop = true; // stop in test mode
                        }
                        else
                        {
                            currentFile = 0; // loop in demo mode
                        }
                    }
                }
                catch (Exception ex)
                {
                    logWriter.WriteLine("Exception occured during execution of {0}: {1}", file, ex.Message);
                }
            }

            if (logWriter != null)
            {
                logWriter.WriteLine("Log closed at " + DateTime.Now);
                logWriter.Close();
                logWriter = null;
            }

            window.SetRibbonControlEnabledInGuiThread(true);

            isRunning = false;
        }

        /// <summary>
        /// Start demo or test mode
        /// </summary>
        /// <param name="filelist">required</param>
        /// <param name="logFile">optional (for test mode), may be null</param>
        public void Start(string[] filelist, string logFile)
        {
            if (!isRunning)
            {
                this.filelist = filelist;
                currentFile = 0;

                if (!string.IsNullOrWhiteSpace(logFile))
                {
                    logWriter = new StreamWriter(logFile);
                    logWriter.WriteLine("Started log at " + DateTime.Now);
                }

                shallStop = false;
                thread = new Thread(DemoControl);
                thread.Start();
            }
        }
    }
}
