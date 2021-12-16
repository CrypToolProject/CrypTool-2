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
    public class TrigramICSearch
    {
        public void searchTrigramIC(Key from, Key to, bool findSettingsIc,
                                           MRingScope lRingSettingScope, int rRingSpacing,
                                           bool hcEveryBest, int hcMaxPass, int minTrigramsScoreToPrint, int THREADS, short[] ciphertext, int len,
                                           string indicatorS, string indicatorMessageKeyS, EnigmaStats enigmaStats, ResultReporter resultReporter)
        {
            Key ckey = new Key(from);
            Key lo = new Key(from);
            Key bestKey = null;

            Key high = new Key(to);

            double best = 0.0;

            short[] plaintext = new short[Key.MAXLEN];

            long counter = 0;

            bool checkForIndicatorMatch = false;
            if (indicatorS.Length * indicatorMessageKeyS.Length != 0)
            {
                checkForIndicatorMatch = true;
            }

            if (lo.mRing == high.mRing)
            {
                lRingSettingScope = MRingScope.ALL;
            }
            long totalKeys = Key.numberOfPossibleKeys(lo, high, len, lRingSettingScope, rRingSpacing, checkForIndicatorMatch);

            long normalizedNkeys = (totalKeys * len) / 250;

            int minRate;
            int maxRate;

            if (lRingSettingScope == MRingScope.ALL)
            {
                minRate = 100000;
                maxRate = 150000;
            }
            else if (lRingSettingScope == MRingScope.ONE_NON_STEPPING)
            {
                minRate = 20000;
                maxRate = 30000;
            }
            else
            {
                minRate = 50000;
                maxRate = 75000;
            }

            Console.WriteLine("STARTING {0} SEARCH: Number of Keys to search: {1} ", findSettingsIc ? "IC" : "TRIGRAM", totalKeys);
            Console.WriteLine("Estimated TrigramICSearch Time: {0}", EnigmaUtils.getEstimatedTimestring(normalizedNkeys, minRate, maxRate));

            DateTime startTime = DateTime.Now;

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
                                for (ckey.gRing = lo.gRing; ckey.gRing <= high.gRing; ckey.gRing++)
                                {
                                    for (ckey.lRing = lo.lRing; ckey.lRing <= high.lRing; ckey.lRing++)
                                    {
                                        for (ckey.mRing = lo.mRing; ckey.mRing <= high.mRing; ckey.mRing++)
                                        {
                                            for (ckey.rRing = lo.rRing; ckey.rRing <= high.rRing; ckey.rRing++)
                                            {
                                                if ((ckey.rRing % rRingSpacing) != 0)
                                                {
                                                    continue;
                                                }
                                                if (findSettingsIc)
                                                {
                                                    resultReporter.UpdateCryptanalysisStep(string.Format("IoC Search ({0})", ckey.getKeystringShort(false)));
                                                }
                                                else
                                                {
                                                    resultReporter.UpdateCryptanalysisStep(string.Format("Trigram Search ({0})", ckey.getKeystringShort(false)));
                                                }
                                                Key keyFromIndicator = null;
                                                if (checkForIndicatorMatch)
                                                {
                                                    keyFromIndicator = ckey.getKeyFromIndicator(indicatorS, indicatorMessageKeyS);
                                                }
                                                for (ckey.gMesg = lo.gMesg; ckey.gMesg <= high.gMesg; ckey.gMesg++)
                                                {
                                                    for (ckey.lMesg = lo.lMesg; ckey.lMesg <= high.lMesg; ckey.lMesg++)
                                                    {
                                                        if (checkForIndicatorMatch && (ckey.lMesg != keyFromIndicator.lMesg))
                                                        {
                                                            continue;
                                                        }
                                                        for (ckey.mMesg = lo.mMesg; ckey.mMesg <= high.mMesg; ckey.mMesg++)
                                                        {
                                                            if (checkForIndicatorMatch && (ckey.mMesg != keyFromIndicator.mMesg))
                                                            {
                                                                continue;
                                                            }
                                                            for (ckey.rMesg = lo.rMesg; ckey.rMesg <= high.rMesg; ckey.rMesg++)
                                                            {
                                                                if ((checkForIndicatorMatch) && (ckey.rMesg != keyFromIndicator.rMesg))
                                                                {
                                                                    continue;
                                                                }

                                                                if (lRingSettingScope != MRingScope.ALL)
                                                                {
                                                                    int mRingSteppingPos = ckey.getLeftRotorSteppingPosition(len);
                                                                    if (!Key.CheckValidWheelsState(len, mRingSteppingPos, lRingSettingScope))
                                                                    {
                                                                        continue;
                                                                    }
                                                                }

                                                                counter++;
                                                                resultReporter.displayProgress(counter, totalKeys);

                                                                if (findSettingsIc)
                                                                {
                                                                    ckey.score = (int)(100000.0 * ckey.icScoreWithoutLookupBuild(ciphertext, len));
                                                                }
                                                                else
                                                                {
                                                                    ckey.score = ckey.triScoreWithoutLookupBuild(ciphertext, len, enigmaStats);
                                                                }

                                                                if (ckey.score < minTrigramsScoreToPrint)
                                                                {
                                                                    continue;
                                                                }
                                                                if (ckey.score - best >= 0)
                                                                {
                                                                    best = ckey.score;
                                                                    bestKey = new Key(ckey);

                                                                    if (hcEveryBest && (hcMaxPass > 0))
                                                                    {
                                                                        if ((findSettingsIc && (ckey.score > 3500)) || (!findSettingsIc && (ckey.score > 10000)))
                                                                        {
                                                                            new HillClimb().hillClimbRange(bestKey, bestKey, hcMaxPass, THREADS,
                                                                                    minTrigramsScoreToPrint, MRingScope.ALL, 1, ciphertext, len, HcSaRunnable.Mode.SA, 5, enigmaStats, resultReporter);
                                                                        }
                                                                    }
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
                                                                            findSettingsIc ? "IC" : "TRIGRAMS", counter / 1000, totalKeys / 1000, counter / elapsed, elapsed / 1000);

                                                                    resultReporter.reportResult(ckey, ckey.score, plains, desc);
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
                        }
                    }
                }
            }
            resultReporter.WriteMessage(string.Format("SEARCH ENDED: Total {0} in {1} Seconds ({2}/Sec)",
                    counter,
                    (DateTime.Now - startTime).TotalMilliseconds / 1000.0,
                    1000 * counter / (DateTime.Now - startTime).TotalMilliseconds + 1));

            if ((bestKey != null) && (hcMaxPass > 0))
            {
                new HillClimb().hillClimbRange(bestKey, bestKey, hcMaxPass, THREADS,
                        findSettingsIc ? 0 : minTrigramsScoreToPrint, MRingScope.ALL, 1, ciphertext, len, HcSaRunnable.Mode.SA, 5, enigmaStats, resultReporter);
            }
        }
    }
}