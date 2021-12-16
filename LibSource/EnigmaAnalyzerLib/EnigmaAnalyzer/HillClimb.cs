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

using EnigmaAnalyzerLib.Common;
using System;

namespace EnigmaAnalyzerLib
{
    public class HillClimb
    {
        public int hillClimbRange(Key from, Key to, int cycles, int THREADS,
                                  int minScoreToPrint, MRingScope lRingSettingScope, int rRingSpacing,
                                  short[] ciphertext, int len, HcSaRunnable.Mode mode, int rounds,
                                  EnigmaStats enigmaStats,
                                  ResultReporter resultReporter,
                                  bool showProgress = true)
        {
            int globalscore = 0;
            long count = 0;
            Key ckey = new Key(from);
            Key low = new Key(from);
            Key high = new Key(to);

            if (low.lRing != high.lRing && mode == HcSaRunnable.Mode.SA)
            {
                //Console.WriteLine("Range of left ring settings not allowed for simulated annealing. {0} - {0}", from.getRotorSettingsstring(), to.getRotorSettingsstring());
            }
            if (low.mRing != high.mRing && mode == HcSaRunnable.Mode.SA)
            {
                //Console.WriteLine("Range of middle settings not allowed for simulated annealing. {0} - {0}", from.getRotorSettingsstring(), to.getRotorSettingsstring());
            }

            if (low.mRing == high.mRing)
            {
                lRingSettingScope = MRingScope.ALL;
            }
            long totalKeysPerCycle = Key.numberOfPossibleKeys(low, high, len, lRingSettingScope, rRingSpacing, false);

            int maxRate = 2800;
            int minRate = 2000;
            string modestring = "HILLCLIMBING-" + rounds;
            if (mode == HcSaRunnable.Mode.SA)
            {
                maxRate = 140;
                minRate = 70;
                modestring = "ANNEALING-" + rounds;
            }
            else if (mode == HcSaRunnable.Mode.EStecker)
            {
                maxRate = 50;
                minRate = 35;
                modestring = "OSTWALD-" + rounds;
            }

            long normalizedNkeys = totalKeysPerCycle * rounds;

            string message = string.Format("Starting {0} search: Number of settings: {1} x {2} cycles = {3} total settings to check.    Estimated search time: {4} per cycle.",
                    modestring, totalKeysPerCycle, cycles, cycles * totalKeysPerCycle, EnigmaUtils.getEstimatedTimestring(normalizedNkeys, minRate, maxRate));

            resultReporter.WriteMessage(message);

            int MAXKEYS = 26 * 26;

            HcSaRunnable[] processes = new HcSaRunnable[MAXKEYS];
            for (int i = 0; i < MAXKEYS; i++)
            {
                processes[i] = new HcSaRunnable();
            }

            int rejected = 0;
            DateTime startTime = DateTime.Now;

            for (int cycle = 0; cycle < cycles; cycle++)
            {

                long keyCountInCycle = 0;
                if (((cycle >= 100) && (cycle % 100 == 0) && (globalscore > minScoreToPrint)) || (normalizedNkeys > 1000000))
                {
                    resultReporter.WriteMessage(string.Format("{0} Cycle {1} best {2} (elapsed {3} seconds)", modestring,
                            cycle + 1, globalscore, (DateTime.Now - startTime).TotalMilliseconds / 1000));
                }

                for (ckey.ukwNum = low.ukwNum; ckey.ukwNum <= high.ukwNum; ckey.ukwNum++)
                {
                    for (ckey.gSlot = low.gSlot; ckey.gSlot <= high.gSlot; ckey.gSlot++)
                    {
                        for (ckey.lSlot = low.lSlot; ckey.lSlot <= high.lSlot; ckey.lSlot++)
                        {
                            for (ckey.mSlot = low.mSlot; ckey.mSlot <= high.mSlot; ckey.mSlot++)
                            {
                                if (ckey.mSlot == ckey.lSlot)
                                {
                                    continue;
                                }
                                for (ckey.rSlot = low.rSlot; ckey.rSlot <= high.rSlot; ckey.rSlot++)
                                {
                                    if (ckey.rSlot == ckey.lSlot || ckey.rSlot == ckey.mSlot)
                                    {
                                        continue;
                                    }
                                    for (ckey.gRing = low.gRing; ckey.gRing <= high.gRing; ckey.gRing++)
                                    {
                                        for (ckey.lRing = low.lRing; ckey.lRing <= high.lRing; ckey.lRing++)
                                        {
                                            if (cycle < 2)
                                            {
                                                string g_mesgS = "";
                                                if (ckey.model == Key.Model.M4)
                                                {
                                                    g_mesgS = "" + EnigmaUtils.getChar(ckey.gMesg);
                                                }
                                                resultReporter.WriteMessage(string.Format("{0} cycle {1}: {2}:{3}:{4}{5}{6}:{7} ({8} seconds) - Best {9}",
                                                        modestring,
                                                        cycle + 1,
                                                        EnigmaUtils.getChar(ckey.ukwNum),
                                                        "" + ckey.lSlot + ckey.mSlot + ckey.rSlot,
                                                        EnigmaUtils.getChar(ckey.lRing),
                                                        EnigmaUtils.getChar(ckey.mRing),
                                                        EnigmaUtils.getChar(ckey.rRing),
                                                        g_mesgS,
                                                        (DateTime.Now - startTime).TotalMilliseconds / 1000,
                                                        globalscore));
                                            }
                                            for (ckey.mRing = low.mRing; ckey.mRing <= high.mRing; ckey.mRing++)
                                            {
                                                for (ckey.rRing = low.rRing; ckey.rRing <= high.rRing; ckey.rRing++)
                                                {
                                                    if (showProgress && mode == HcSaRunnable.Mode.SA)
                                                    {
                                                        resultReporter.UpdateCryptanalysisStep(string.Format("Sim. Annealing ({0})", ckey.getKeystringShort(false)));
                                                    }
                                                    else if (showProgress && mode == HcSaRunnable.Mode.SA)
                                                    {
                                                        resultReporter.UpdateCryptanalysisStep(string.Format("Hillclimbing ({0})", ckey.getKeystringShort(false)));
                                                    }
                                                    //       if (ckey.r_slot > 5 && ckey.r_ring > 12) continue;
                                                    for (ckey.gMesg = low.gMesg; ckey.gMesg <= high.gMesg; ckey.gMesg++)
                                                    {
                                                        for (ckey.lMesg = low.lMesg; ckey.lMesg <= high.lMesg; ckey.lMesg++)
                                                        {

                                                            Runnables runnables = new Runnables();
                                                            int numberOfKeys = 0;
                                                            for (ckey.mMesg = low.mMesg; ckey.mMesg <= high.mMesg; ckey.mMesg++)
                                                            {
                                                                for (ckey.rMesg = low.rMesg; ckey.rMesg <= high.rMesg; ckey.rMesg++)
                                                                {
                                                                    if (resultReporter.ShouldTerminate)
                                                                    {
                                                                        return int.MinValue;
                                                                    }
                                                                    if ((ckey.rRing % rRingSpacing) != (cycle % rRingSpacing))
                                                                    {
                                                                        rejected++;
                                                                        continue;
                                                                    }
                                                                    if (lRingSettingScope != MRingScope.ALL)
                                                                    {
                                                                        int mRingSteppingPos = ckey.getLeftRotorSteppingPosition(len);
                                                                        if (!Key.CheckValidWheelsState(len, mRingSteppingPos, lRingSettingScope))
                                                                        {
                                                                            rejected++;
                                                                            continue;
                                                                        }
                                                                    }
                                                                    processes[numberOfKeys].setup(ckey, from.stbrett, ciphertext, len, (cycle == 0), mode, rounds, rRingSpacing, enigmaStats);
                                                                    runnables.addRunnable(processes[numberOfKeys]);
                                                                    numberOfKeys++;
                                                                    keyCountInCycle++;
                                                                }
                                                            }
                                                            if (numberOfKeys == 0)
                                                            {
                                                                continue;
                                                            }
                                                            if (THREADS < runnables.Count)
                                                            {
                                                                runnables.run(THREADS, resultReporter, false);
                                                            }
                                                            else
                                                            {
                                                                runnables.run(runnables.Count, resultReporter, false);
                                                            }
                                                            resultReporter.displayProgress(keyCountInCycle, totalKeysPerCycle);
                                                            for (int k = 0; k < numberOfKeys; k++)
                                                            {
                                                                count++;
                                                                ckey.clone(processes[k].key);
                                                                if (ckey.score > globalscore)
                                                                {
                                                                    globalscore = ckey.score;
                                                                }
                                                                if (globalscore > minScoreToPrint)
                                                                {
                                                                    if (resultReporter.shouldPushResult(ckey.score))
                                                                    {
                                                                        ckey.addRightRotorOffset(processes[k].bestOffset);
                                                                        ckey.initPathLookupAll(len);
                                                                        string plainStr = ckey.plaintextstring(ciphertext, len);


                                                                        long elapsed = (long)(DateTime.Now - startTime).TotalMilliseconds;
                                                                        if (elapsed <= 0)
                                                                        {
                                                                            elapsed = 1;
                                                                        }
                                                                        string desc = string.Format("{0} [{1}][{2}: {3}/{4}][{5}/sec][{6}/sec][{7} Sec][{8}][Offset: {9}]", modestring,
                                                                                count / 1000, cycle + 1, keyCountInCycle / 1000, totalKeysPerCycle / 1000, count * 1000 / elapsed, (count + rejected) * 1000 / elapsed, elapsed / 1000, DateTime.Now, processes[k].bestOffset);
                                                                        //ckey.printKeystring("Hillclimbing " + desc);
                                                                        resultReporter.reportResult(ckey, ckey.score, plainStr, desc);
                                                                        ckey.substractRightRotorOffset(processes[k].bestOffset);

                                                                    }
                                                                    //string logs = "" + ft.Format(dNow) + ckey.getKeystringlong() + " " + ckey.plaintextstring(ciphertext, len);
                                                                    //Utils.saveToFile("hc.txt", logs);
                                                                }
                                                            } // for k to numberOfKeys
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (globalscore > minScoreToPrint)
            {
                long elapsed = (long)(DateTime.Now - startTime).TotalMilliseconds;

                if (elapsed > 1000)
                {
                    resultReporter.WriteMessage(string.Format("{0} - FINISHED AFTER {1} cycles - Best: {2} Rate: {3}/sec Total Checked: {4} in {5} Seconds ({6} filtered)", modestring,
                            cycles, globalscore, 1000 * count / elapsed, count, elapsed / 1000.0, rejected));
                }
                else
                {
                    resultReporter.WriteMessage(string.Format("{0} - FINISHED AFTER {1} cycles - Best: {2} Total Checked: {3} ({4} filtered)", modestring,
                            cycles, globalscore, count, rejected));
                }
            }
            return globalscore;
        }

        /*public void hillClimbRange(HillClimbParameterSet hillClimbParameterSet, EnigmaStats enigmaStats, ResultReporter resultReporter)
        {
            hillClimbRange(hillClimbParameterSet.KeyFrom, hillClimbParameterSet.KeyTo, hillClimbParameterSet.Cycles, hillClimbParameterSet.Threads,
            hillClimbParameterSet.MinScoreToPrint, hillClimbParameterSet.LRingSettingScope, hillClimbParameterSet.RRingSpacing,
            hillClimbParameterSet.Ciphertext, hillClimbParameterSet.Len, hillClimbParameterSet.Mode, hillClimbParameterSet.Rounds,
            enigmaStats, resultReporter);
        }*/

        public int hillClimbBatch(Key[] keys, int nKeys, int hcMaxPass, int THREADS, int minScoreToPrint, short[] ciphertext, int len, int rRingSpacing,
            EnigmaStats enigmaStats,
            ResultReporter resultReporter)
        {
            int count = 0;
            int bestscore = 0;
            int BATCHSIZE = 26 * 26 * 26;

            HcSaRunnable[] processes = new HcSaRunnable[BATCHSIZE];
            for (int i = 0; i < BATCHSIZE; i++)
            {
                processes[i] = new HcSaRunnable();
            }

            DateTime startTime = DateTime.Now;

            for (int pass = 0; pass < hcMaxPass && !resultReporter.ShouldTerminate; pass++)
            {
                resultReporter.UpdateCryptanalysisStep(string.Format("Hill Climbing Batch Pass {0} of {1}", pass + 1, hcMaxPass));
                int countInPass = 0;
                if (nKeys * hcMaxPass > 10000)
                {
                    resultReporter.WriteMessage(string.Format("HILL CLIMBING BATCH OF {0} Keys - Pass {1} of {2}", nKeys, pass + 1, hcMaxPass));
                }

                for (int k = 0; k < nKeys && !resultReporter.ShouldTerminate; k += BATCHSIZE)
                {
                    if ((hcMaxPass == 1) && (nKeys < 20))
                    {
                        resultReporter.WriteMessage(string.Format("HILL CLIMBING BATCH - Pass {0} of {1}", pass + 1, hcMaxPass));
                        keys[k].printKeystring("");
                    }
                    int actualBatchSize;

                    actualBatchSize = Math.Min(BATCHSIZE, nKeys - k);
                    Runnables runnables = new Runnables();

                    for (int b = 0; b < actualBatchSize; b++)
                    {
                        processes[b].setup(keys[k + b], keys[k + b].stbrett, ciphertext, len, (pass == 0), HcSaRunnable.Mode.SA, 1, rRingSpacing, enigmaStats);
                        runnables.addRunnable(processes[b]);
                    }

                    runnables.run(THREADS, resultReporter, true);

                    for (int b = 0; b < actualBatchSize && !resultReporter.ShouldTerminate; b++)
                    {
                        count++;
                        countInPass++;

                        //Key ckey = new Key(processes[b].theKey);
                        Key ckey = processes[b].key;

                        if (ckey.score > minScoreToPrint)
                        {
                            if (resultReporter.shouldPushResult(ckey.score))
                            {
                                ckey.initPathLookupAll(len);
                                string plainStr = ckey.plaintextstring(ciphertext, len);

                                long elapsed = (long)(DateTime.Now - startTime).TotalMilliseconds;
                                if (elapsed <= 0)
                                {
                                    elapsed = 1;
                                }
                                string desc = string.Format("HILLCLIMBING TOP [{0}][{1}: {2}/{3}][{4} Sec][{5}/sec]][{6}]",
                                        count / 1000, pass + 1, countInPass, nKeys, elapsed / 1000, count * 1000 / elapsed, DateTime.Now);

                                resultReporter.reportResult(ckey, ckey.score, plainStr, desc);
                                //ckey.printKeystring("Hillclimbing " + desc);
                            }
                            if (ckey.score > bestscore)
                            {
                                bestscore = ckey.score;
                                short[] steppings = new short[Key.MAXLEN];
                                ckey.showSteppings(steppings, len);
                                string steppingsS = EnigmaUtils.getCiphertextstringNoXJ(steppings, len);
                                resultReporter.WriteMessage(string.Format("{0}{1}", ckey.plaintextstring(ciphertext, len), steppingsS));
                            }
                        }
                    }
                }
            }
            return bestscore;
        }
    }
}