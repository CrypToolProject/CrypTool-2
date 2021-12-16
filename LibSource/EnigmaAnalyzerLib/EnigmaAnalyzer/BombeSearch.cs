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
using System.Text;

namespace EnigmaAnalyzerLib
{
    public class BombeSearch
    {
        public void bombeSearch(string CRIB, short[] ciphertext, int clen, bool range, Key lowKey, Key highKey, Key key, string indicatorS, string indicatorMessageKeyS, int HILLCLIMBING_CYCLES, int RIGHT_ROTOR_SAMPLING, MRingScope MIDDLE_RING_SCOPE, bool VERBOSE, string CRIB_POSITION, int THREADS,
            EnigmaStats enigmaStats, ResultReporter resultReporter)
        {
            resultReporter.UpdateCryptanalysisStep("Starting Bomb Search");

            short[] crib = new short[BombeCrib.MAXCRIBL];
            int maxCribLen = Math.Min(BombeCrib.MAXCRIBL, clen);
            if (CRIB.Length > maxCribLen)
            {
                throw new Exception(string.Format("Crib too long ({0} letters) - should not be longer than {1} letters", CRIB.Length, maxCribLen));
            }
            int crlen = EnigmaUtils.getText(CRIB, crib);

            int minPos = 0;
            int maxPos = (clen - crlen);
            if (CRIB_POSITION.Length != 0 && !CRIB_POSITION.EqualsIgnoreCase("*"))
            {
                int separator = CRIB_POSITION.IndexOf("-");
                if (separator == -1)
                {
                    minPos = getIntValue(CRIB_POSITION, 0, maxPos, Flag.CRIB_POSITION);
                    if (minPos == -1)
                    {
                        return;
                    }
                    else
                    {
                        maxPos = minPos;
                    }
                }
                else
                {
                    minPos = getIntValue(CRIB_POSITION.Substring(0, separator), 0, maxPos, Flag.CRIB_POSITION);
                    if (minPos == -1)
                    {
                        return;
                    }
                    maxPos = getIntValue(CRIB_POSITION.Substring(separator + 1), minPos, maxPos, Flag.CRIB_POSITION);
                    if (maxPos == -1)
                    {
                        return;
                    }
                }
            }

            int position = minPos;

            BombeMenu[] menus = new BombeMenu[1000];
            int nMenus = 0;

            while (((position = BombeCrib.nextValidPosition(ciphertext, clen, crib, crlen, position)) != -1) && (position <= maxPos))
            {
                BombeCrib bombeCrib = new BombeCrib(ciphertext, crib, crlen, position, VERBOSE && (minPos == maxPos));

                if ((bombeCrib.menu.score < BombeCrib.BADSCORE) || minPos == maxPos)
                {
                    menus[nMenus++] = bombeCrib.menu;
                    resultReporter.WriteMessage(string.Format("Creating Bombe Menu at Position {0} (Links:{1}, Closures:{2}, Score:{3})", bombeCrib.menu.cribStartPos, bombeCrib.menu.totalItems, bombeCrib.menu.totalClosures, bombeCrib.menu.score));
                    if (bombeCrib.menu.score > BombeCrib.BADSCORE)
                    {
                        resultReporter.WriteWarning(string.Format("Warning: Turing Score ({0}) is high (higher means worse) for Bombe menu. This may create many false stops. A longer crib may help.",
                                bombeCrib.menu.score));
                    }
                }
                position++;
                if (resultReporter.ShouldTerminate)
                {
                    return;
                }
            }
            if (nMenus > 0)
            {
                resultReporter.WriteMessage(string.Format("{0} Bombe menus created - Starting search using Turing Bombe", nMenus));
                if (!range)
                {
                    lowKey = highKey = key;
                }
                searchCribMenus(menus, nMenus, lowKey, highKey, MIDDLE_RING_SCOPE, RIGHT_ROTOR_SAMPLING,
                        HILLCLIMBING_CYCLES, THREADS, ciphertext, clen, VERBOSE, indicatorS, indicatorMessageKeyS, enigmaStats, resultReporter);
            }
            else
            {
                resultReporter.WriteWarning(string.Format("No good Bombe menu (Turing Score less than {0}) found for Crib - Either not enough links/closures, or letters encrypted to themselves", BombeCrib.BADSCORE));
            }

        }

        private int scoreForMenus(double tri, double ic)
        {
            int res;
            if (tri > 10000.0)
            {
                res = (int)tri;
            }
            else if (ic > 0.050)
            {
                res = (int)(10000.0 * ic / 0.050);
            }
            else
            {
                res = (int)(tri * ic / 0.050);
            }
            if (res < 3000)
            {
                return 0;
            }
            else
            {
                return res;
            }
        }

        private void searchCribMenus(BombeMenu[] bombeMenus, int nMenus, Key from, Key to,
                                            MRingScope lRingSettingScope, int rRingSpacing,
                                            int hcMaxPass, int THREADS, short[] ciphertext, int len, bool debugMenus,
                                            string indicatorS, string indicatorMessageKeyS, EnigmaStats enigmaStats, ResultReporter resultReporter)
        {
            Key ckey = new Key(from);
            Key lo = new Key(from);
            Key high = new Key(to);

            double ic;
            int tri;

            short[] plaintext = new short[Key.MAXLEN];
            int nStops = 0;
            int bestscore = 0;
            int bestMenu = 0;

            const int MAXTOPS = 100000;
            Key[] topKeys = new Key[MAXTOPS];
            int nTops = 0;

            short[] assumedSteckers = new short[26];
            short[] strength = new short[26];

            bool checkForIndicatorMatch = false;
            if (indicatorS.Length * indicatorMessageKeyS.Length != 0)
            {
                checkForIndicatorMatch = true;
            }

            long counter = 0;
            long counterSameMax = 0;
            long countKeys = 0;

            if (lo.mRing == high.mRing)
            {
                lRingSettingScope = MRingScope.ALL;
            }
            long totalKeys = Key.numberOfPossibleKeys(lo, high, len, lRingSettingScope, rRingSpacing, checkForIndicatorMatch);
            resultReporter.WriteMessage(string.Format("Start Bombe search: Number of menus: {0}, Number of keys: {1}, Total to Check: {2}", nMenus, totalKeys, nMenus * totalKeys));

            printEstimatedTimeBombeRun(totalKeys * bombeMenus[0].cribLen / 25, nMenus, lRingSettingScope, resultReporter);

            DateTime start = DateTime.Now;

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
                                                resultReporter.UpdateCryptanalysisStep(string.Format("Bombe Search ({0})", ckey.getKeystringShort(false)));
                                                for (ckey.gMesg = lo.gMesg; ckey.gMesg <= high.gMesg; ckey.gMesg++)
                                                {
                                                    Key keyFromIndicator = null;
                                                    if (checkForIndicatorMatch)
                                                    {
                                                        keyFromIndicator = ckey.getKeyFromIndicator(indicatorS, indicatorMessageKeyS);
                                                    }
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
                                                                if (checkForIndicatorMatch && (ckey.lMesg != keyFromIndicator.lMesg))
                                                                {
                                                                    continue;
                                                                }

                                                                if (lRingSettingScope != MRingScope.ALL)
                                                                {
                                                                    int lRingSteppingPos = ckey.getLeftRotorSteppingPosition(len);
                                                                    if (!Key.CheckValidWheelsState(len, lRingSteppingPos, lRingSettingScope))
                                                                    {
                                                                        continue;
                                                                    }
                                                                }

                                                                if (resultReporter.ShouldTerminate)
                                                                {
                                                                    return;
                                                                }

                                                                countKeys++;
                                                                resultReporter.displayProgress(countKeys, totalKeys);
                                                                bool foundForThisKey = false;
                                                                for (int m = 0; (m < nMenus) && !foundForThisKey; m++)
                                                                {
                                                                    counter++;

                                                                    if (ckey.model == Key.Model.M4)
                                                                    {
                                                                        ckey.initPathLookupHandM4Range(bombeMenus[m].cribStartPos, bombeMenus[m].cribLen);
                                                                    }
                                                                    else
                                                                    {
                                                                        ckey.initPathLookupHandM3Range(bombeMenus[m].cribStartPos, bombeMenus[m].cribLen);
                                                                    }

                                                                    for (int i = 0; i < 26; i++)
                                                                    {
                                                                        assumedSteckers[i] = -1;
                                                                        strength[i] = 0;
                                                                    }

                                                                    if (bombeMenus[m].testIfBombsStops(0, ckey.lookup, assumedSteckers, strength, false))
                                                                    {
                                                                        foundForThisKey = true;
                                                                        int[] stb = new int[26];
                                                                        for (int i = 0; i < 26; i++)
                                                                        {
                                                                            if (assumedSteckers[i] == -1)
                                                                            {
                                                                                stb[i] = i;
                                                                            }
                                                                            else
                                                                            {
                                                                                stb[i] = assumedSteckers[i];
                                                                            }
                                                                        }
                                                                        ckey.setStecker(stb);
                                                                        tri = ckey.triScoreWithoutLookupBuild(ciphertext, len, enigmaStats);
                                                                        ic = ckey.icScoreWithoutLookupBuild(ciphertext, len);
                                                                        ckey.score = scoreForMenus(tri, ic);

                                                                        if (ckey.score > 0)
                                                                        {
                                                                            nStops++;
                                                                            if (nStops == (MAXTOPS - 1))
                                                                            {
                                                                                resultReporter.WriteWarning(string.Format("WARNING: Too many stops - Only the top {0} keys (sorted by IC and Trigrams) will be kept for Hill Climbing", MAXTOPS));
                                                                                resultReporter.WriteWarning(string.Format("Bombe search with the current crib parameters (crib string and position/position range) may be inefficient and/or miss the right key."));
                                                                                resultReporter.WriteWarning(string.Format("It is recommended to either reduce the key range, use a longer crib, or specify fewer positions to search for the crib."));
                                                                            }
                                                                            bool sort = false;
                                                                            if (nTops < MAXTOPS)
                                                                            {
                                                                                topKeys[nTops++] = new Key(ckey);
                                                                                sort = true;
                                                                            }
                                                                            else if (ckey.score > topKeys[nTops - 1].score)
                                                                            {
                                                                                topKeys[nTops - 1] = new Key(ckey);
                                                                                sort = true;
                                                                            }

                                                                            if (sort)
                                                                            {
                                                                                for (int i = nTops - 1; i >= 1; i--)
                                                                                {
                                                                                    if (topKeys[i].score > topKeys[i - 1].score)
                                                                                    {
                                                                                        Key tempKey = topKeys[i];
                                                                                        topKeys[i] = topKeys[i - 1];
                                                                                        topKeys[i - 1] = tempKey;
                                                                                    }
                                                                                }
                                                                            }
                                                                        }

                                                                        if (ckey.score == bestscore)
                                                                        {
                                                                            counterSameMax++;
                                                                            if (counterSameMax == 100)
                                                                            {
                                                                                resultReporter.WriteWarning(string.Format("WARNING: Too many stops with same score ({0}). Only stops with higher scores will be displayed", bestscore));
                                                                            }
                                                                        }

                                                                        if ((ckey.score > bestscore) || ((ckey.score == bestscore) && (counterSameMax < 100)))
                                                                        {
                                                                            if (ckey.score > bestscore)
                                                                            {
                                                                                counterSameMax = 0;
                                                                            }
                                                                            bestMenu = m;
                                                                            bestscore = ckey.score;
                                                                            printStop(bombeMenus[m], ciphertext, len, ckey, ic, tri, plaintext, assumedSteckers, strength, start, totalKeys, countKeys, resultReporter);
                                                                        }
                                                                    } // if valid
                                                                } // for menus
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

            if ((nStops == 1) && debugMenus)
            {
                ckey = new Key(from);

                if (ckey.model == Key.Model.M4)
                {
                    ckey.initPathLookupRange(bombeMenus[0].cribStartPos, bombeMenus[0].cribLen);
                }
                else
                {
                    ckey.initPathLookupHandM3Range(bombeMenus[0].cribStartPos, bombeMenus[0].cribLen);
                }

                for (int i = 0; i < 26; i++)
                {
                    assumedSteckers[i] = -1;
                    strength[i] = 0;
                }

                //valid = bombeMenus[0].TestValidity(0,ckey.path_lookup, assumedSteckers, true);
                bombeMenus[0].testIfBombsStops(0, ckey.lookup, assumedSteckers, strength, true);

            }
            long elapsed = (long)(DateTime.Now - start).TotalMilliseconds + 1;
            if (elapsed <= 0)
            {
                elapsed = 1;
            }

            if (nMenus == 1)
            {
                resultReporter.WriteMessage(string.Format("End of Bombe Search >>{0}<< at Position: {1} (Turing Score: {2} Closures: {3} Links: {4}) FOUND {5} STOP(s) {6} Total Keys Tested in {7} Seconds({8}/sec)",
                        EnigmaUtils.getstring(bombeMenus[0].crib, bombeMenus[0].cribLen),
                        bombeMenus[0].cribStartPos, bombeMenus[0].score, bombeMenus[0].totalClosures, bombeMenus[0].totalItems,
                        nStops, counter, elapsed / 1000.0, 1000 * counter / elapsed));
            }
            else if (nStops > 0)
            {
                resultReporter.WriteMessage(string.Format("End of Bombe Search >>{0}<< for {1} Menus - Best menu found for Position: {2} (Turing Score: {3} Closures: {4} Links: {5}) FOUND {6} STOP(S)  {7} Total Keys/Menu Combinations Tested in {8} Seconds({9}/sec)",
                        EnigmaUtils.getstring(bombeMenus[bestMenu].crib, bombeMenus[bestMenu].cribLen),
                        nMenus,
                        bombeMenus[bestMenu].cribStartPos, bombeMenus[bestMenu].score, bombeMenus[bestMenu].totalClosures, bombeMenus[bestMenu].totalItems,
                        nStops, counter, elapsed / 1000.0, 1000 * counter / elapsed));
            }
            else
            {
                resultReporter.WriteMessage(string.Format("End of Bombe Search >>{0}<< for {1} Menus NO STOP FOUND! {2} Keys&Menu Combinations Tested in {3}f Seconds({4}/sec)",
                        EnigmaUtils.getstring(bombeMenus[0].crib, bombeMenus[0].cribLen),
                        nMenus, counter, elapsed / 1000.0, 1000 * counter / elapsed));
            }

            if ((nTops >= 10) && (hcMaxPass > 0))
            {
                resultReporter.WriteMessage(string.Format("Menu Bombe - Starting batch of {0} Keys; Min Score : {1}, Median Score: {2}, Max Score: {3}",
                        nTops, topKeys[nTops - 1].score, topKeys[nTops / 2].score, topKeys[0].score));
            }

            if ((nTops > 0) && (hcMaxPass > 0))
            {
                new HillClimb().hillClimbBatch(topKeys, nTops, hcMaxPass, THREADS, 10000, ciphertext, len, rRingSpacing, enigmaStats, resultReporter);
            }
        }

        private void printEstimatedTimeBombeRun(long normalizedNkeys1, int nMenus, MRingScope lRingSettingScope, ResultReporter resultReporter)
        {
            int minRate;
            int maxRate;

            if (lRingSettingScope == MRingScope.ALL)
            {
                minRate = 50000;
                maxRate = 100000;
            }
            else if (lRingSettingScope == MRingScope.ONE_NON_STEPPING)
            {
                minRate = 15000;
                maxRate = 30000;
            }
            else
            {
                minRate = 25000;
                maxRate = 50000;
            }
            resultReporter.WriteMessage(string.Format("Estimated Search Time: {0} for a small number of stops (more if many stops are found)", EnigmaUtils.getEstimatedTimestring(nMenus * normalizedNkeys1, minRate, maxRate)));
        }

        private void printStop(BombeMenu bombeMenu, short[] ciphertext, int len, Key ckey, double ic, int tri, short[] plaintext, short[] assumedSteckers, short[] strength, DateTime startTime, long totalKeys, long counterKeys, ResultReporter resultReporter)
        {
            string plains;
            StringBuilder stbs = new StringBuilder();
            string confirmedSelfS = "";
            StringBuilder strengthStbs = new StringBuilder();
            string strengthSelfS = "";

            for (int i = 0; i < 26; i++)
            {
                int s = assumedSteckers[i];
                if (i < s)
                {
                    stbs.Append("").Append(EnigmaUtils.getChar(i)).Append(EnigmaUtils.getChar(s));

                    if (strength[i] > 0)
                    {
                        strengthStbs.Append(" ").Append(EnigmaUtils.getChar(i)).Append(EnigmaUtils.getChar(s)).Append("{").Append(strength[i]).Append("}");
                    }

                }
                else if (i == s)
                {
                    confirmedSelfS += "" + EnigmaUtils.getChar(i);
                    if (strength[i] > 0)
                    {
                        strengthSelfS += " " + EnigmaUtils.getChar(i) + "{" + strength[i] + "}";
                    }
                }
            }

            ckey.setStecker(stbs.ToString());
            ckey.encipherDecipherAll(ciphertext, plaintext, len);

            plains = EnigmaUtils.getstring(plaintext, len);
            long elapsed = (long)(DateTime.Now - startTime).TotalMilliseconds;
            if (elapsed <= 0)
            {
                elapsed = 1;
            }
            string desc = string.Format("BOMBE [Pos: {0}][{1}K/{2}K][{3}K/sec][{4} Sec]",
                    bombeMenu.cribStartPos, counterKeys / 1000, totalKeys / 1000, counterKeys / elapsed, elapsed / 1000);

            if (resultReporter.shouldPushResult(ckey.score))
            {
                resultReporter.reportResult(ckey, ckey.score, plains, desc, bombeMenu.cribStartPos);

                resultReporter.WriteMessage(string.Format("MENU STOP NEW BEST - Pos: {0} Stop Score: {1} (Tri: {2} IC: {3}) - Crib Length: {4}, Crib: {5}",
                        bombeMenu.cribStartPos, ckey.score, tri, ic, bombeMenu.cribLen,
                        EnigmaUtils.getstring(bombeMenu.crib, bombeMenu.cribLen)));
                resultReporter.WriteMessage(string.Format("Stecker: [ Pairs: {0} ({1}) Self: {2} ({3}) Total: {4} ] - Confirmation Strength: Pairs: {5} Self: {6}",
                        stbs.ToString(), stbs.Length, confirmedSelfS, confirmedSelfS.Length, (stbs.Length + confirmedSelfS.Length),
                        strengthStbs.ToString(), strengthSelfS));
                resultReporter.WriteMessage(string.Format("Key: {0}", string.Format(ckey.getKeystringlong())));
                //Console.WriteLine("{0}", plains);
            }
        }

        private int getIntValue(string s, int min, int max, Flag flag)
        {

            for (int i = 0; i < s.Length; i++)
            {
                if (EnigmaUtils.getDigitIndex(s[i]) == -1)
                {
                    throw new Exception(string.Format("Invalid {0} ({1}) for {2} - Expecting number from {3} to {4} ", s, CommandLine.getShortDesc(flag), flag, min, max));
                }
            }

            int intValue = 0;
            bool parseExceptionOccured = false;
            try
            {
                intValue = int.Parse(s);
            }
            catch (Exception)
            {
                parseExceptionOccured = true;
            }

            if ((intValue >= min) && (intValue <= max) && !parseExceptionOccured)
            {
                return intValue;
            }
            throw new Exception(string.Format("Invalid {0} ({1}) for {2} - Expecting number from {3} to {4} ", s, CommandLine.getShortDesc(flag), flag, min, max));
        }
    }
}