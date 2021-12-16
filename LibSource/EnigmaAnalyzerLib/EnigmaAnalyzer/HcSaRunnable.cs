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
using static EnigmaAnalyzerLib.Common.Utils;

namespace EnigmaAnalyzerLib
{
    public class HcSaRunnable : Runnable
    {
        public enum Action { NO_CHANGE, IandK, SIandSK, IandSK, KandSI, IandK_SIandSK, IandSK_KandSI }

        private static readonly int[] FREQUENT = { 4, 13, 23, 17, 18, 0, 8, 19, 20, 14, 11, 3, 5, 6, 12, 1, 7, 10, 25, 22, 16, 21, 2, 15, 9, 24 };

        public Key key = new Key();
        public int bestOffset;
        private readonly short[] ciphertext = new short[1000];
        private int len;
        private bool firstPass;
        private readonly int[] stb = new int[26];
        private readonly int[] var = Arrays.CopyOf(FREQUENT, FREQUENT.Length);
        private Mode mode;
        private int rounds;
        private int rRingSpacing;
        private bool stopped = false;
        private EnigmaStats enigmaStats;

        public void setup(Key key, int[] stb, short[] ciphertext, int len, bool firstPass, Mode mode, int rounds, int rRingSpacing, EnigmaStats enigmaStats)
        {
            this.key.clone(key);
            Array.Copy(ciphertext, 0, this.ciphertext, 0, len);
            Array.Copy(stb, 0, this.stb, 0, 26);
            this.len = len;
            this.firstPass = firstPass;
            this.key.score = -1; // to mark that no HC has been done on this one.
            this.rounds = rounds;
            this.mode = mode;
            this.rRingSpacing = rRingSpacing;
            this.enigmaStats = enigmaStats;
        }

        public override void run()
        {
            if (stopped)
            {
                return;
            }

            if (key.model != Key.Model.M4)
            {
                key.initPathLookupHandM3(len);
            }
            else
            {
                key.initPathLookupAll(len);
            }
            Key.randVar(var);

            key.setStecker(stb); // restore because the ones on key were changed in previous keys/passes
            if (firstPass && (key._stbCount != 0))
            {
                HCStep(Key.EVAL.TRI);
            }
            else
            {
                switch (mode)
                {
                    case Mode.HC:
                        HC();
                        break;
                    case Mode.SA:
                        SA();
                        break;
                    case Mode.EStecker:
                        EStecker();
                        break;
                }
            }
        }

        private void EStecker()
        {
            //                      E   N  X    R  S   I  A  T    O  U
            int[] FREQUENT = { 4, 13, 23, 17, 18, 8, 0, 19, 14, 20, 11, 3, 5, 6, 12, 1, 7, 10, 25, 22, 16, 21, 2, 15, 9, 24 };

            long bestScore = 0;
            string bestStb = "";

            for (int xIndexInMostFrequentLetters = 0; xIndexInMostFrequentLetters < Math.Min(26, rounds) && !stopped; xIndexInMostFrequentLetters++)
            {
                int x = FREQUENT[xIndexInMostFrequentLetters];
                for (int yIndexInMostFrequentLetters = xIndexInMostFrequentLetters; yIndexInMostFrequentLetters < 26 && !stopped; yIndexInMostFrequentLetters++)
                {
                    int y = FREQUENT[yIndexInMostFrequentLetters];
                    key.setStecker("");
                    long currentScore = key.eval(Key.EVAL.IC, ciphertext, len, enigmaStats);
#if DEBUG
                    Console.WriteLine("(Start  {0}{1}) {2} {3}", EnigmaUtils.getChar(x), EnigmaUtils.getChar(y), key.stbstring(), currentScore);
#endif
                    if (y != x)
                    {
                        key.stbConnect(x, y);
                    }

                    phiMarsch();
                    currentScore = triSturm(true);
                    currentScore = bereinigeBrett(currentScore);

#if DEBUG
                    Console.WriteLine("(Summary  ) {0} {1} ({2}) {3}", key.stbstring(), currentScore, (int)enigmaStats.triSchwelle(len), currentScore > enigmaStats.triSchwelle(len) ? "!!!!!" : "");
#endif
                    int bestOffsetForCycle = 0;

                    if (currentScore > enigmaStats.triSchwelle(len))
                    {
                        long bestOffsetScore = currentScore;
                        for (int offset = -rRingSpacing + 1; offset < rRingSpacing; offset++)
                        {
                            key.addRightRotorOffset(offset);
                            long score = key.triScoreWithoutLookupBuild(ciphertext, len, enigmaStats);
                            if (score > bestOffsetScore)
                            {
                                bestOffsetScore = score;
                                bestOffsetForCycle = offset;
                            }
                            key.substractRightRotorOffset(offset);
                        }
                        key.addRightRotorOffset(bestOffsetForCycle);
                        currentScore = triSturm(false);
                        key.substractRightRotorOffset(bestOffsetForCycle);

#if DEBUG
                        Console.WriteLine("(Krabben  ) {0} {1}", key.stbstring(), currentScore);
#endif
                    }
                    if (currentScore > bestScore)
                    {
                        bestStb = key.stbstring();
                        bestScore = currentScore;
                        bestOffset = bestOffsetForCycle;
                    }
                }
            }
            key.setStecker(bestStb);
            if (bestOffset == 0)
            {
                if (key.eval(Key.EVAL.TRI, ciphertext, len, enigmaStats) != bestScore)
                {
                    throw new Exception("Inconsistent!!! " + key.score + " != " + bestScore);
                }
            }
            else
            {
                key.score = (int)bestScore;
            }
        }
        private long phiMarsch()
        {
            long currentScore;
            long newScore;
            currentScore = key.eval(Key.EVAL.IC, ciphertext, len, enigmaStats);
#if DEBUG
            Console.WriteLine("(Start    ) {0} {1}", key.stbstring(), currentScore);
#endif

            for (int m = 0; m < 8; m++)
            {
                int bestI = -1;
                int bestJ = -1;
                long bestIJScore = 0;
                for (int i = 0; i < 26; i++)
                {
                    if (key.stbrett[i] != i)
                    {
                        continue;
                    }
                    for (int j = i + 1; j < 26; j++)
                    {
                        if (key.stbrett[j] != j)
                        {
                            continue;
                        }
                        key.stbConnect(i, j);
                        newScore = key.eval(Key.EVAL.IC, ciphertext, len, enigmaStats);
                        if (newScore > bestIJScore)
                        {
                            bestI = i;
                            bestJ = j;
                            bestIJScore = newScore;
                        }
                        key.stbDisconnect(i, j);
                    }
                }
                if (bestIJScore <= currentScore)
                {
                    break;
                }
                key.stbConnect(bestI, bestJ);
                currentScore = key.eval(Key.EVAL.IC, ciphertext, len, enigmaStats);
#if DEBUG
                Console.WriteLine("(Marsch {0}) {1} {2}", m, key.stbstring(), currentScore);
#endif
            }
            return currentScore;
        }

        private long bereinigeBrett(long currentScore)
        {
            long newScore;
            for (int c = 0; ; c++)
            {
                bool changed = false;
                for (int i = 0; i < 26; i++)
                {
                    int partnerOfI = key.stbrett[i];
                    if (partnerOfI == i)
                    {
                        continue;
                    }
                    key.stbDisconnect(i, partnerOfI);
                    newScore = key.eval(Key.EVAL.TRI, ciphertext, len, enigmaStats);
                    if (newScore > currentScore)
                    {
                        currentScore = newScore;
#if DEBUG
                        Console.WriteLine("(Clean {0}) {1} {2}", c, key.stbstring(), currentScore);
#endif
                        changed = true;
                    }
                    else
                    {
                        key.stbConnect(i, partnerOfI);
                    }
                }
                if (!changed)
                {
                    break;
                }
            }
#if DEBUG
            Console.WriteLine("(Clean end) {0} {1}", key.stbstring(), currentScore);
#endif
            return currentScore;
        }

        private long triSturm(bool useLookup)
        {
            long currentScore;
            long newScore;
            currentScore = useLookup ? key.eval(Key.EVAL.TRI, ciphertext, len, enigmaStats) : key.triScoreWithoutLookupBuild(ciphertext, len, enigmaStats);
#if DEBUG
            Console.WriteLine("(Tri      ) {0} {1}", key.stbstring(), currentScore);
#endif
            for (int s = 0; ; s++)
            {
                int bestI = -1;
                int bestJ = -1;
                long bestIJScore = currentScore;
                for (int i = 0; i < 26; i++)
                {
                    for (int j = i + 1; j < 26; j++)
                    {
                        int partnerOfI = key.stbrett[i];
                        int partnerOfJ = key.stbrett[j];
                        if (key.stbCount() == Key.MAX_STB_PLUGS && partnerOfI == i && partnerOfJ == j)
                        {
                            continue;
                        }

                        if (partnerOfI != i)
                        {
                            key.stbDisconnect(i, partnerOfI);
                        }
                        if (partnerOfJ != j)
                        {
                            key.stbDisconnect(j, partnerOfJ);
                        }
                        key.stbConnect(i, j);
                        newScore = useLookup ? key.eval(Key.EVAL.TRI, ciphertext, len, enigmaStats) : key.triScoreWithoutLookupBuild(ciphertext, len, enigmaStats);
                        if (newScore > bestIJScore)
                        {
                            bestI = i;
                            bestJ = j;
                            bestIJScore = newScore;
                        }
                        key.stbDisconnect(i, j);
                        if (partnerOfJ != j)
                        {
                            key.stbConnect(j, partnerOfJ);
                        }
                        if (partnerOfI != i)
                        {
                            key.stbConnect(i, partnerOfI);
                        }
                    }
                }
                if (bestI == -1)
                {
                    break;
                }
                int partnerOfBestI = key.stbrett[bestI];
                int partnerOfBestJ = key.stbrett[bestJ];
                if (partnerOfBestI != bestI)
                {
                    key.stbDisconnect(bestI, partnerOfBestI);
                }
                if (partnerOfBestJ != bestJ)
                {
                    key.stbDisconnect(bestJ, partnerOfBestJ);
                }
                key.stbConnect(bestI, bestJ);
                currentScore = useLookup ? key.eval(Key.EVAL.TRI, ciphertext, len, enigmaStats) : key.triScoreWithoutLookupBuild(ciphertext, len, enigmaStats);
#if DEBUG
                Console.WriteLine("(Sturm  {0}) {1} {2}", s, key.stbstring(), currentScore);
#endif
                if (currentScore != bestIJScore)
                {
                    throw new Exception("Inconsistent!!! " + currentScore + " != " + bestIJScore);
                }
            }
#if DEBUG
            Console.WriteLine("(Sturm end) {0} {1}", key.stbstring(), currentScore);
#endif
            return currentScore;
        }

        private void HC()
        {
            key.setStecker("");
            string bestStbS = "";
            long bestScore = 0;
            int noImprove = 0;
            for (int i = 0; i < rounds * 2 && noImprove < 3 && !stopped; i++)
            {
                HCStep(Key.EVAL.IC);
                HCStep(Key.EVAL.BI);
                HCStep(Key.EVAL.TRI);
                if (key.score > bestScore)
                {
                    bestScore = key.score;
                    bestStbS = key.stbstring();
                    noImprove = 0;
                }
                else
                {
                    noImprove++;
                }
            }
            key.setStecker(bestStbS);
            long currentScore = key.score = key.eval(Key.EVAL.TRI, ciphertext, len, enigmaStats);
            checkOffsets(currentScore);
        }

        private void checkOffsets(long currentScore)
        {
            if (currentScore > enigmaStats.triSchwelle(len))
            {
                long bestOffsetScore = currentScore;
                int bestOffsetForCycle = 0;
                for (int offset = -rRingSpacing + 1; offset < rRingSpacing; offset++)
                {
                    key.addRightRotorOffset(offset);
                    long score = key.triScoreWithoutLookupBuild(ciphertext, len, enigmaStats);
                    if (score > bestOffsetScore)
                    {
                        bestOffsetScore = score;
                        bestOffsetForCycle = offset;
                    }
                    key.substractRightRotorOffset(offset);
                }
                key.addRightRotorOffset(bestOffsetForCycle);
                key.score = (int)triSturm(false);
                key.substractRightRotorOffset(bestOffsetForCycle);
                bestOffset = bestOffsetForCycle;
            }
        }

        private void SA()
        {
            key.setStecker("");
            for (int i = 0; i < rounds * 2 && !stopped; i++)
            {
                SAStep(Key.EVAL.BI);
            }
            //SAStep(Key.EVAL.TRI);
            long currentScore = key.score = key.eval(Key.EVAL.TRI, ciphertext, len, enigmaStats);
            checkOffsets(currentScore);
        }

        private void HCStep(Key.EVAL eval)
        {
            Action action;
            long newScore;
            long bestScore = key.eval(eval, ciphertext, len, enigmaStats);
            short[] invVar = new short[26];
            for (short i = 0; i < 26; i++)
            {
                invVar[var[i]] = i;
            }
            bool improved;
            do
            {
                improved = false;
                for (int i = 0; i < 26 && !stopped; i++)
                {
                    int vi = var[i]; // invariant
                    for (int k = i + 1; k < 26 && !stopped; k++)
                    {
                        int vk = var[k];
                        int vsk = key.stbrett[vk];
                        int vsi = key.stbrett[vi]; // not an invariant
                        if (vsk == vi)
                        {
                            continue;
                        }
                        int sk = invVar[vsk];
                        int si = invVar[vsi];

                        action = Action.NO_CHANGE;

                        if (vi == vsi && vk == vsk)
                        {

                            if (key.stbCount() == Key.MAX_STB_PLUGS)
                            {
                                continue;
                            }
                            key.stbConnect(vi, vk);

                            newScore = key.eval(eval, ciphertext, len, enigmaStats);
                            if (newScore > bestScore)
                            {
                                bestScore = newScore;
                                improved = true;
                                action = Action.IandK;
                            }
                            if (action == Action.NO_CHANGE)
                            {
                                key.stbDisconnect(vi, vk);
                            }
                        }
                        else if (vi == vsi)
                        {
                            if ((sk > i) && (sk < k))
                            {
                                continue;
                            }
                            key.stbDisconnect(vk, vsk);
                            //all self
                            key.stbConnect(vi, vk);

                            newScore = key.eval(eval, ciphertext, len, enigmaStats);
                            if (newScore > bestScore)
                            {
                                bestScore = newScore;
                                improved = true;
                                action = Action.IandK;
                            }
                            key.stbDisconnect(vi, vk);
                            // all self
                            key.stbConnect(vi, vsk);

                            newScore = key.eval(eval, ciphertext, len, enigmaStats);
                            if (newScore > bestScore)
                            {
                                bestScore = newScore;
                                improved = true;
                                action = Action.IandSK;
                            }
                            key.stbDisconnect(vi, vsk);
                            // all self now
                            switch (action)
                            {
                                case Action.IandK:
                                    key.stbConnect(vi, vk);
                                    break;
                                case Action.IandSK:
                                    key.stbConnect(vi, vsk);
                                    break;
                                case Action.NO_CHANGE:
                                    key.stbConnect(vk, vsk);
                                    break;
                            }
                        }
                        else if (vk == vsk)
                        {
                            if ((si < k) && (si < i))
                            {
                                continue;
                            }
                            key.stbDisconnect(vi, vsi);
                            // all self
                            key.stbConnect(vk, vi);
                            newScore = key.eval(eval, ciphertext, len, enigmaStats);
                            if (newScore > bestScore)
                            {
                                bestScore = newScore;
                                improved = true;
                                action = Action.IandK;
                            }
                            key.stbDisconnect(vk, vi);
                            // all self
                            key.stbConnect(vk, vsi);
                            newScore = key.eval(eval, ciphertext, len, enigmaStats);
                            if (newScore > bestScore)
                            {
                                bestScore = newScore;
                                improved = true;
                                action = Action.KandSI;
                            }
                            key.stbDisconnect(vk, vsi);
                            // all self
                            switch (action)
                            {
                                case Action.IandK:
                                    key.stbConnect(vi, vk);
                                    break;
                                case Action.KandSI:
                                    key.stbConnect(vk, vsi);
                                    break;
                                case Action.NO_CHANGE:
                                    key.stbConnect(vi, vsi);
                                    break;
                            }
                        }
                        else
                        {
                            if ((si < i) || (sk < k))
                            {
                                continue;
                            }
                            key.stbDisconnect(vi, vsi);
                            key.stbDisconnect(vk, vsk);
                            // all Self now
                            key.stbConnect(vi, vk);
                            newScore = key.eval(eval, ciphertext, len, enigmaStats);
                            if (newScore > bestScore)
                            {
                                bestScore = newScore;
                                action = Action.IandK;
                            }
                            key.stbConnect(vsi, vsk);
                            newScore = key.eval(eval, ciphertext, len, enigmaStats);
                            if (newScore > bestScore)
                            {
                                bestScore = newScore;
                                action = Action.IandK_SIandSK;
                            }
                            key.stbDisconnect(vi, vk);
                            key.stbDisconnect(vsi, vsk);
                            // all Self now
                            key.stbConnect(vi, vsk);
                            newScore = key.eval(eval, ciphertext, len, enigmaStats);
                            if (newScore > bestScore)
                            {
                                bestScore = newScore;
                                action = Action.IandSK;
                            }
                            key.stbConnect(vsi, vk);
                            newScore = key.eval(eval, ciphertext, len, enigmaStats);
                            if (newScore > bestScore)
                            {
                                bestScore = newScore;
                                action = Action.IandSK_KandSI;
                            }
                            key.stbDisconnect(vi, vsk);
                            key.stbDisconnect(vsi, vk);
                            // all Self now
                            switch (action)
                            {
                                case Action.IandK:
                                    key.stbConnect(vi, vk);
                                    break;
                                case Action.IandSK:
                                    key.stbConnect(vi, vsk);
                                    break;
                                case Action.IandK_SIandSK:
                                    key.stbConnect(vi, vk);
                                    key.stbConnect(vsi, vsk);
                                    break;
                                case Action.IandSK_KandSI:
                                    key.stbConnect(vi, vsk);
                                    key.stbConnect(vsi, vk);
                                    break;
                                case Action.NO_CHANGE:
                                    key.stbConnect(vi, vsi);
                                    key.stbConnect(vk, vsk);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            } while (improved && !stopped);

            if (key.eval(eval, ciphertext, len, enigmaStats) != bestScore)
            {
                throw new Exception("Best result is not consistent");
            }
            key.score = key.eval(eval, ciphertext, len, enigmaStats);
        }

        private void SAStep(Key.EVAL eval)
        {
            Action action;

            string bestStb = key.stbstring();
            long bestScore = key.eval(eval, ciphertext, len, enigmaStats);

            long newScore;
            long currScore = key.eval(eval, ciphertext, len, enigmaStats);

            bool changed;
            int roundsWithoutChange = 0;
            double temp;
            if (len <= 30)
            {
                temp = 400.0;
            }
            else if (len <= 50)
            {
                temp = 400.0 - (400.0 - 315.0) * (len - 30) / (50 - 30);
            }
            else if (len <= 75)
            {
                temp = 315.0 - (315.0 - 240.0) * (len - 50) / (75 - 50);
            }
            else if (len <= 100)
            {
                temp = 240.0 - (240.0 - 220.0) * (len - 75) / (100 - 75);
            }
            else if (len <= 150)
            {
                temp = 220.0 - (220.0 - 200.0) * (len - 100) / (150 - 100);
            }
            else
            {
                temp = 200.0;
            }
            int ROUNDS = 200;
            for (int round = 0; round < ROUNDS && roundsWithoutChange < 10 && !stopped; round++)
            {
                Key.randVar(var);

                changed = false;
                for (int i = 0; i < 26 && !stopped; i++)
                {
                    int vi = var[i]; // invariant
                    for (int k = i + 1; k < 26 && !stopped; k++)
                    {

                        int vk = var[k];
                        int vsk = key.stbrett[vk];
                        int vsi = key.stbrett[vi]; // not an invariant

                        if (vsk == vi)
                        {
                            continue;
                        }

                        action = Action.NO_CHANGE;

                        if (vi == vsi && vk == vsk)
                        {
                            if (key.stbCount() == Key.MAX_STB_PLUGS)
                            {
                                continue;
                            }
                            key.stbConnect(vi, vk);
                            newScore = key.eval(eval, ciphertext, len, enigmaStats);
                            if (accept(newScore, currScore, temp))
                            {
                                currScore = newScore;
                                changed = true;
                                if (newScore > bestScore)
                                {
                                    bestScore = newScore;
                                    bestStb = key.stbstring();
                                }
                            }
                            else
                            {
                                key.stbDisconnect(vi, vk);
                            }
                        }
                        else if (vi == vsi)
                        { // vk != vsk

                            if (vsk < vk)
                            {
                                continue;
                            }

                            key.stbDisconnect(vk, vsk);
                            key.stbConnect(vi, vk);

                            newScore = key.eval(eval, ciphertext, len, enigmaStats);
                            if (accept(newScore, currScore, temp))
                            {
                                currScore = newScore;
                                changed = true;
                                action = Action.IandK;
                                if (newScore > bestScore)
                                {
                                    bestScore = newScore;
                                    bestStb = key.stbstring();
                                }
                            }
                            key.stbDisconnect(vi, vk);
                            // all self
                            key.stbConnect(vi, vsk);

                            newScore = key.eval(eval, ciphertext, len, enigmaStats);
                            if (accept(newScore, currScore, temp))
                            {
                                currScore = newScore;
                                changed = true;
                                action = Action.IandSK;
                                if (newScore > bestScore)
                                {
                                    bestScore = newScore;
                                    bestStb = key.stbstring();
                                }
                            }
                            key.stbDisconnect(vi, vsk);
                            // all self now

                            switch (action)
                            {
                                case Action.IandK:
                                    key.stbConnect(vi, vk);
                                    break;
                                case Action.IandSK:
                                    key.stbConnect(vi, vsk);
                                    break;
                                case Action.NO_CHANGE:
                                    key.stbConnect(vk, vsk);
                                    break;
                                default:
                                    break;
                            }

                        }
                        else if (vk == vsk)
                        {

                            if (vsi < vi)
                            {
                                continue;
                            }
                            key.stbDisconnect(vi, vsi);
                            // all self
                            key.stbConnect(vk, vi);
                            newScore = key.eval(eval, ciphertext, len, enigmaStats);
                            if (accept(newScore, currScore, temp))
                            {
                                currScore = newScore;
                                changed = true;
                                action = Action.IandK;
                                if (newScore > bestScore)
                                {
                                    bestScore = newScore;
                                    bestStb = key.stbstring();
                                }
                            }
                            key.stbDisconnect(vk, vi);

                            // all self
                            key.stbConnect(vk, vsi);
                            newScore = key.eval(eval, ciphertext, len, enigmaStats);
                            if (accept(newScore, currScore, temp))
                            {
                                currScore = newScore;
                                changed = true;
                                action = Action.KandSI;
                                if (newScore > bestScore)
                                {
                                    bestScore = newScore;
                                    bestStb = key.stbstring();
                                }
                            }
                            key.stbDisconnect(vk, vsi);
                            // all self
                            switch (action)
                            {
                                case Action.IandK:
                                    key.stbConnect(vi, vk);
                                    break;
                                case Action.KandSI:
                                    key.stbConnect(vk, vsi);
                                    break;
                                case Action.NO_CHANGE:
                                    key.stbConnect(vi, vsi);
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            if ((vsi < vi) || (vsk < vk))
                            {
                                continue;
                            }

                            key.stbDisconnect(vi, vsi);
                            key.stbDisconnect(vk, vsk);
                            // all Self now

                            key.stbConnect(vi, vk);
                            newScore = key.eval(eval, ciphertext, len, enigmaStats);
                            if (accept(newScore, currScore, temp))
                            {
                                currScore = newScore;
                                changed = true;
                                action = Action.IandK;
                                if (newScore > bestScore)
                                {
                                    bestScore = newScore;
                                    bestStb = key.stbstring();
                                }
                            }

                            key.stbConnect(vsi, vsk);
                            newScore = key.eval(eval, ciphertext, len, enigmaStats);
                            if (accept(newScore, currScore, temp))
                            {
                                currScore = newScore;
                                changed = true;
                                action = Action.IandK_SIandSK;
                                if (newScore > bestScore)
                                {
                                    bestScore = newScore;
                                    bestStb = key.stbstring();
                                }
                            }
                            key.stbDisconnect(vsi, vsk);

                            key.stbDisconnect(vi, vk);
                            // all Self now
                            key.stbConnect(vi, vsk);
                            newScore = key.eval(eval, ciphertext, len, enigmaStats);
                            if (accept(newScore, currScore, temp))
                            {
                                currScore = newScore;
                                changed = true;
                                action = Action.IandSK;
                                if (newScore > bestScore)
                                {
                                    bestScore = newScore;
                                    bestStb = key.stbstring();
                                }
                            }

                            key.stbConnect(vsi, vk);
                            newScore = key.eval(eval, ciphertext, len, enigmaStats);
                            if (accept(newScore, currScore, temp))
                            {
                                currScore = newScore;
                                changed = true;
                                action = Action.IandSK_KandSI;
                                if (newScore > bestScore)
                                {
                                    bestScore = newScore;
                                    bestStb = key.stbstring();
                                }
                            }
                            key.stbDisconnect(vi, vsk);
                            key.stbDisconnect(vsi, vk);

                            // all Self now

                            switch (action)
                            {
                                case Action.IandK:
                                    key.stbConnect(vi, vk);
                                    break;
                                case Action.IandSK:
                                    key.stbConnect(vi, vsk);
                                    break;
                                case Action.SIandSK:
                                    key.stbConnect(vsi, vsk);
                                    break;
                                case Action.IandK_SIandSK:
                                    key.stbConnect(vi, vk);
                                    key.stbConnect(vsi, vsk);
                                    break;
                                case Action.IandSK_KandSI:
                                    key.stbConnect(vi, vsk);
                                    key.stbConnect(vsi, vk);
                                    break;
                                case Action.NO_CHANGE:
                                    key.stbConnect(vi, vsi);
                                    key.stbConnect(vk, vsk);
                                    break;
                                default:
                                    throw new Exception("Impossible change " + action);
                            }
                        }
                    }
                }
                if (!changed)
                {
                    roundsWithoutChange++;
                }
                else
                {
                    roundsWithoutChange = 0;
                }
            }

            if (key.eval(eval, ciphertext, len, enigmaStats) != currScore)
            {
                throw new Exception("Best result is not consistent");
            }

            key.setStecker(bestStb);

            HCStep(eval);
            key.score = key.eval(eval, ciphertext, len, enigmaStats);

        }

        private readonly Random random = new Random();

        private bool accept(long newScore, long currLocalScore, double temperature)
        {

            long diffScore = newScore - currLocalScore;
            if (diffScore > 0)
            {
                return true;
            }
            if (temperature == 0.0)
            {
                return false;
            }
            double prob = Math.Pow(Math.E, diffScore / temperature);
            return prob > 0.0085 && prob > random.NextDouble();
        }

        public class Round
        {
            public Key key;
            public HcSaRunnable process;
            public short[] ciphertext;
            public short[] plaintext;
            public int len;
            private readonly EnigmaStats enigmaStats;

            public Round(int len, Key from, Key to, bool generateX, int garbles, Mode mode, int rounds, bool badSearchKey, EnigmaStats enigmaStats)
            {
                this.len = len;
                this.enigmaStats = enigmaStats;
                process = new HcSaRunnable();
                ciphertext = new short[len];
                plaintext = new short[len];
                key = new Key();
                EnigmaUtils.loadRandomText("faust.txt", plaintext, len, generateX, garbles);
                key.initRandom(from, to, 10);
                key.encipherDecipherAll(plaintext, ciphertext, len);
                key.score = key.triScoreWithoutLookupBuild(ciphertext, len, enigmaStats);
                if (badSearchKey)
                {
                    key.initRandom(from, to, 10);
                }
                process.setup(key, key.stbrett, ciphertext, len, false, mode, rounds, 0, enigmaStats);
            }

            public bool check(int pass, bool print)
            {
                string s1 = process.key.plaintextstring(ciphertext, len);
                string s2 = EnigmaUtils.getstring(plaintext, len);

                int errors = 0;
                for (int i = 0; i < len; i++)
                {
                    if (s1[i] != s2[i])
                    {
                        errors++;
                    }
                }

                if (print)
                {
                    Console.WriteLine("Expected: {0} Found: {1} Errors: {2} {3}  ", key.score, process.key.score, errors, process.key.score > key.score ? "!!!!!" : "");
                    if (errors < len / 3)
                    {
                        if (errors > 0)
                        {
                            Console.WriteLine("{0} {1} {2}", key.score, key.stbstring(), s2);
                            Console.WriteLine("{0} {1} {2}", process.key.score, process.key.stbstring(), s1);
                        }

                        Console.WriteLine("FOUND {0} {1}", (pass + 1), DateTime.Now);
                        key.printKeystring("");
                        Console.WriteLine("{0}", process.key.plaintextstring(ciphertext, len));
                        return true;
                    }
                }
                return errors < len / 3;
            }
        }

        public enum Mode
        {
            EStecker,
            HC,
            SA
        }

        public override void stop()
        {
            stopped = true;
        }
    }
}