/*
   Copyright 2020 Nils Kopal
   based on the paper: Ciphertext-only Cryptanalysis from James. G. Gillogly and George Lasry's code
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
    public class GilloglyAttack
    {
        public void PerformAttack(Key from, Key to,
                                int rRingSpacing,
                                int hcMaxPass, int THREADS, short[] ciphertext, int len,
                                EnigmaStats enigmaStats, ResultReporter resultReporter)
        {
            Key ckey = new Key(from);
            Key lo = new Key(from);
            Key high = new Key(to);

            Key bestKey = null;

            double best = 0.0;

            short[] plaintext = new short[Key.MAXLEN];

            long counter = 0;

            long totalKeys = Key.numberOfPossibleKeys(new Key(lo) { gRing = 0, rRing = 0, mRing = 0, lRing = 0 },
                                                      new Key(high) { gRing = 0, rRing = 0, mRing = 0, lRing = 0 },
                                                      len, MRingScope.ONE_NON_STEPPING, rRingSpacing, false);

            DateTime startTime = DateTime.Now;

            //List<HillClimbParameterSet> hillClimbParameterSetsPhase1 = new List<HillClimbParameterSet>();
            //List<HillClimbParameterSet> hillClimbParameterSetsPhase2 = new List<HillClimbParameterSet>();

            // Gillogly Phase 1: Search for Rotors and set rings fix to 0000
            ckey.gRing = 0;
            ckey.lRing = 0;
            ckey.mRing = 0;
            ckey.rRing = 0;

            for (ckey.ukwNum = lo.ukwNum; ckey.ukwNum <= high.ukwNum; ckey.ukwNum++)
            {
                for (ckey.gSlot = lo.gSlot; ckey.gSlot <= high.gSlot; ckey.gSlot++)
                {
                    for (ckey.lSlot = lo.lSlot; ckey.lSlot <= high.lSlot; ckey.lSlot++)
                    {
                        for (ckey.mSlot = lo.mSlot; ckey.mSlot <= high.mSlot; ckey.mSlot++)
                        {
                            if (ckey.mSlot == ckey.lSlot)
                            {
                                continue;
                            }
                            for (ckey.rSlot = lo.rSlot; ckey.rSlot <= high.rSlot; ckey.rSlot++)
                            {
                                if (ckey.rSlot == ckey.lSlot || ckey.rSlot == ckey.mSlot)
                                {
                                    continue;
                                }
                                resultReporter.UpdateCryptanalysisStep(string.Format("Gillogly Phase 1 ({0})", ckey.getKeystringShort(false)));

                                for (ckey.gMesg = lo.gMesg; ckey.gMesg <= high.gMesg; ckey.gMesg++)
                                {
                                    for (ckey.lMesg = lo.lMesg; ckey.lMesg <= high.lMesg; ckey.lMesg++)
                                    {
                                        for (ckey.mMesg = lo.mMesg; ckey.mMesg <= high.mMesg; ckey.mMesg++)
                                        {
                                            for (ckey.rMesg = lo.rMesg; ckey.rMesg <= high.rMesg; ckey.rMesg++)
                                            {
                                                counter++;
                                                resultReporter.displayProgress(counter, totalKeys);
                                                ckey.score = (int)(100000.0 * ckey.icScoreWithoutLookupBuild(ciphertext, len));

                                                if (ckey.score - best >= 0)
                                                {
                                                    best = ckey.score;
                                                    bestKey = new Key(ckey);

                                                    /*HillClimbParameterSet hillClimbParameterSet = new HillClimbParameterSet()
                                                    {
                                                        KeyFrom = bestKey,
                                                        KeyTo = bestKey,
                                                        Cycles = hcMaxPass,
                                                        Ciphertext = ciphertext,
                                                        Len = len,
                                                        LRingSettingScope = MRingScope.ALL,
                                                        MinScoreToPrint = 0,
                                                        Mode = HcSaRunnable.Mode.HC,
                                                        Rounds = 5,
                                                        RRingSpacing = 1
                                                    };

                                                    hillClimbParameterSetsPhase1.Add(hillClimbParameterSet);*/

                                                    if (resultReporter.shouldPushResult(ckey.score))
                                                    {
                                                        ckey.encipherDecipherAll(ciphertext, plaintext, len);
                                                        string plains = EnigmaUtils.getstring(plaintext, len);

                                                        long elapsed = (long)(DateTime.Now - startTime).TotalMilliseconds;
                                                        if (elapsed <= 0)
                                                        {
                                                            elapsed = 1;
                                                        }
                                                        string desc = string.Format("{0} [{1}K/{2}K][{3}K/sec][{4} Sec]",
                                                                "IC", counter / 1000, totalKeys / 1000, counter / elapsed, elapsed / 1000);

                                                        resultReporter.reportResult(ckey, ckey.score, plains, desc);
                                                    }
                                                }
                                                if (resultReporter.ShouldTerminate)
                                                {
                                                    return;
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

            resultReporter.WriteMessage(string.Format("PHASE 1 ENDED: Total {0} in {1} Seconds ({2}/Sec)",
                    counter,
                    (DateTime.Now - startTime).TotalMilliseconds / 1000.0,
                    1000 * counter / (DateTime.Now - startTime).TotalMilliseconds + 1));

            if (bestKey == null)
            {
                return;
            }

            //sort best list of phase 1
            //hillClimbParameterSetsPhase1.Sort((a, b) => a.KeyFrom.score.CompareTo(b.KeyFrom.score));

            best = (int)(100000.0 * bestKey.icScoreWithoutLookupBuild(ciphertext, len));
            //best = (int)(100000.0 * ckey.icScoreWithoutLookupBuild(ciphertext, len));
            //best = bestKey.triScoreWithoutLookupBuild(ciphertext, len, enigmaStats);

            // Gillogly Phase 2: Search for Ring settings and Rotor settings
            //foreach (HillClimbParameterSet currentHillClimbParameterSet in hillClimbParameterSetsPhase1)
            //{
            //ckey = currentHillClimbParameterSet.KeyFrom;
            ckey = bestKey;

            totalKeys = Key.numberOfPossibleKeys(new Key(lo) { gSlot = 0, rSlot = 0, mSlot = 0, lSlot = 0 },
                                                      new Key(high) { gSlot = 0, rSlot = 0, mSlot = 0, lSlot = 0 },
                                                      len, MRingScope.ALL, rRingSpacing, false);
            counter = 0;

            for (ckey.gRing = lo.gRing; ckey.gRing <= high.gRing; ckey.gRing++)
            {
                for (ckey.lRing = lo.lRing; ckey.lRing <= high.lRing; ckey.lRing++)
                {
                    for (ckey.mRing = lo.mRing; ckey.mRing <= high.mRing; ckey.mRing++)
                    {
                        for (ckey.rRing = lo.rRing; ckey.rRing <= high.rRing; ckey.rRing++)
                        {
                            resultReporter.UpdateCryptanalysisStep(string.Format("Gillogly Phase 2 ({0})", ckey.getKeystringShort(false)));
                            resultReporter.displayProgress(counter, totalKeys);

                            for (ckey.gMesg = lo.gMesg; ckey.gMesg <= high.gMesg; ckey.gMesg++)
                            {
                                for (ckey.lMesg = lo.lMesg; ckey.lMesg <= high.lMesg; ckey.lMesg++)
                                {
                                    for (ckey.mMesg = lo.mMesg; ckey.mMesg <= high.mMesg; ckey.mMesg++)
                                    {
                                        for (ckey.rMesg = lo.rMesg; ckey.rMesg <= high.rMesg; ckey.rMesg++)
                                        {
                                            counter++;
                                            //ckey.score = ckey.triScoreWithoutLookupBuild(ciphertext, len, enigmaStats);
                                            ckey.score = (int)(100000.0 * ckey.icScoreWithoutLookupBuild(ciphertext, len));

                                            if (ckey.score - best >= 0)
                                            {
                                                best = ckey.score;
                                                bestKey = new Key(ckey);

                                                //if (ckey.score > 10000)
                                                if (ckey.score > 3500)
                                                {
                                                    /*HillClimbParameterSet hillClimbParameterSet = new HillClimbParameterSet()
                                                    {
                                                        KeyFrom = bestKey,
                                                        KeyTo = bestKey,
                                                        Cycles = hcMaxPass,
                                                        Ciphertext = ciphertext,
                                                        Len = len,
                                                        LRingSettingScope = MRingScope.ALL,
                                                        MinScoreToPrint = 0,
                                                        Mode = HcSaRunnable.Mode.HC,
                                                        Rounds = 5,
                                                        RRingSpacing = 1
                                                    };
                                                    hillClimbParameterSetsPhase2.Add(hillClimbParameterSet);*/
                                                    new HillClimb().hillClimbRange(bestKey, bestKey, hcMaxPass, THREADS, 0, MRingScope.ALL, 1, ciphertext, len, HcSaRunnable.Mode.HC, 5, enigmaStats, resultReporter, false);
                                                }
                                                if (resultReporter.shouldPushResult(ckey.score))
                                                {
                                                    ckey.encipherDecipherAll(ciphertext, plaintext, len);
                                                    string plains = EnigmaUtils.getstring(plaintext, len);

                                                    long elapsed = (long)(DateTime.Now - startTime).TotalMilliseconds;
                                                    if (elapsed <= 0)
                                                    {
                                                        elapsed = 1;
                                                    }
                                                    string desc = string.Format("{0} [{1}K/{2}K][{3}K/sec][{4} Sec]",
                                                            "TRIGRAMS", counter / 1000, totalKeys / 1000, counter / elapsed, elapsed / 1000);

                                                    resultReporter.reportResult(ckey, ckey.score, plains, desc);
                                                }
                                            }

                                            if (resultReporter.ShouldTerminate)
                                            {
                                                return;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //}

            if (bestKey != null)
            {
                new HillClimb().hillClimbRange(bestKey, bestKey, hcMaxPass, THREADS, 0, MRingScope.ALL, 1, ciphertext, len, HcSaRunnable.Mode.SA, 5, enigmaStats, resultReporter);
            }
        }
    }

    /*
    public class HillClimbParameterSet
    {
        public Key KeyFrom { get; set; }
        public Key KeyTo { get; set; }
        public int Cycles { get; set; }
        public int Threads { get; set; }
        public int MinScoreToPrint { get; set; }
        public MRingScope LRingSettingScope { get; set; }
        public int RRingSpacing { get; set; }
        public short[] Ciphertext { get; set; }
        public int Len { get; set; }
        public HcSaRunnable.Mode Mode { get; set; }
        public int Rounds { get; set; }
    }*/
}
