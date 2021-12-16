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

namespace EnigmaAnalyzerLib
{
    /// <summary>
    /// Abstract basic class of a "Result Reporter"
    /// The result reporter is responsible for reporting everythign to the "execution environment"
    /// If the analysis is run as console application, use the ConsoleResultReporter
    /// CrypTool 2 implements its own ResultReporter which "reports" to the user interface
    /// Also, the ResultReporter can stop the execution by setting the "ShouldTerminate" flag to true.
    /// This enables CrypTool 2 to stop the analysis process
    /// </summary>
    public abstract class ResultReporter
    {
        public bool ShouldTerminate
        {
            get;
            set;
        }

        public abstract void reportResult(Key key, int currScore, string plaintext, string desc, int cribPosition = -1);
        public abstract void displayProgress(long count, long max);
        public abstract bool shouldPushResult(int score);
        public abstract void displayBestKey(string key);
        public abstract void displayBestPlaintext(string plaintext);
        public abstract void WriteMessage(string message);
        public abstract void WriteWarning(string message);
        public abstract void WriteException(string message, Exception ex);
        public abstract void UpdateCryptanalysisStep(string step);
    }

    /// <summary>
    /// Default result reporter who writes everything to the console
    /// </summary>
    public class ConsoleResultReporter : ResultReporter
    {
        private DateTime lastProgressUpdate = DateTime.Now;
        private int lastScore = int.MinValue;
        private long lastCount = 0;

        public int UpdateIntervalInSeconds
        {
            get;
            set;
        }

        public ConsoleResultReporter()
        {
            UpdateIntervalInSeconds = 1;
        }

        public override void reportResult(Key key, int currScore, string plaintext, string desc, int cribPosition = -1)
        {
            //only report Score if it was better than the previous one
            if (currScore > lastScore)
            {
                lastScore = currScore;
                if (cribPosition != -1)
                {
                    Console.WriteLine("Best : {0} {1} {2} {3} {4} {5}", currScore, key.getKeystringlong(), key.getKeystringShort(), plaintext, desc, cribPosition);
                }
                else
                {
                    Console.WriteLine("Best : {0} {1} {2} {3} {4}", currScore, key.getKeystringlong(), key.getKeystringShort(), plaintext, desc);
                }

            }
        }

        public override void displayProgress(long count, long max)
        {
            if (DateTime.Now > lastProgressUpdate.AddSeconds(UpdateIntervalInSeconds))
            {
                float speed = (count - lastCount) / (float)UpdateIntervalInSeconds;
                float totalSeconds = (max - count) / speed;
                TimeSpan timeSpan = new TimeSpan(0, 0, 0, (int)totalSeconds);
                Console.WriteLine(string.Format("Progress: {0:###.##}%", 100 * count / (float)max));
                Console.WriteLine(string.Format("Estimated time left: {0}", timeSpan));
                lastProgressUpdate = DateTime.Now;
                lastCount = count;
            }
        }

        public override bool shouldPushResult(int score)
        {
            return true;
        }

        public override void displayBestKey(string key)
        {
            Console.WriteLine("Best key: {0}", key);
        }

        public override void displayBestPlaintext(string plaintext)
        {
            Console.WriteLine("Best plaintext: {0}", plaintext);
        }

        public override void WriteMessage(string message)
        {
            Console.WriteLine("Message: {0}", message);
        }

        public override void WriteWarning(string message)
        {
            Console.WriteLine("Warning: {0}", message);
        }

        public override void WriteException(string message, Exception ex)
        {
            Console.WriteLine("Exception: {0}", message);
            Console.WriteLine("Stacktrace: {0}", ex.StackTrace);
        }

        public override void UpdateCryptanalysisStep(string step)
        {
            Console.WriteLine("Update Cryptanalysis Step: {0}", step);
        }
    }
}