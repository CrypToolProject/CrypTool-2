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
using M209AnalyzerLib.Common;
using M209AnalyzerLib.Enums;
using System;

namespace M209AnalyzerLib.M209
{
    public class ReportManager
    {
        private static long startTimeMillis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        public static bool simulation = false;
        public static bool knownPlaintext = false;
        private static int pushed = 0;
        public static bool silent = true;
        public static CtBestList CtBestList { get; set; }

        public static void ReportResult(int task, int[] roundLayers, int layers, Key key, double currScore, string desc)
        {
            if (CtBestList.ShouldPushResult(currScore))
            {
                string decryption = key.EncryptDecrypt(key.CipherText, false);

                long elapsedMillis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - ReportManager.startTimeMillis;
                string comment;
                if (simulation)
                {

                    comment = $"[{key.GetCountIncorrectLugs()}/{key.GetCountIncorrectPins()}]" +
                        $"[2**{(long)(Math.Log(Key.evaluations) / Math.Log(2))}][{(elapsedMillis == 0 ? 0 : Key.evaluations / elapsedMillis)} K/s]" +
                        $"[{task}: {roundLayers[0]}/{(layers > 1 ? roundLayers[1] : 0)}/{(layers > 2 ? roundLayers[2] : 0)}/{(layers > 3 ? roundLayers[3] : 0)} {desc}] ";
                }
                else
                {
                    comment = $"[{(long)(Math.Log(Key.evaluations) / Math.Log(2))}]" +
                        $"[{(elapsedMillis == 0 ? 0 : Key.evaluations / elapsedMillis)} K/s]" +
                        $"[{task}: {roundLayers[0]}/{(layers > 1 ? roundLayers[1] : 0)}/{(layers > 2 ? roundLayers[2] : 0)}/{(layers > 3 ? roundLayers[3] : 0)} " +
                        $"{desc}] ";

                }

                if (CtBestList.PushResult(currScore, key, key.Decryption))
                {
                    pushed++;
                    if (simulation)
                    {
                        int error = key.GetCountIncorrectLugs() * 5 + key.GetCountIncorrectPins();
                        //     CtAPI.updateProgress(Math.max(100 - error, 0), 100);
                    }
                    else if (knownPlaintext)
                    {
                        //    CtAPI.updateProgress(currScore - 120000, 10000);
                    }
                    else
                    {
                        //    CtAPI.updateProgress(currScore, 58000);
                    }
                }
            }
            if (currScore == key.OriginalScore && !silent)
            {
                long elapsedSec = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - ReportManager.startTimeMillis) / 1000;
                Console.WriteLine($"Found key - Task {task} - {Key.evaluations} decryptions - elapsed {elapsedSec} seconds - reported {pushed} results\n");
                //CtAPI.goodbye();
            }
        }

        public static void setThreshold(EvalType evalType)
        {
            switch (evalType)
            {
                case EvalType.CRIB:
                    CtBestList.SetScoreThreshold(127_500);
                    break;
                case EvalType.MONO:
                    CtBestList.SetScoreThreshold(40_000);
                    break;
            }
        }
    }
}
