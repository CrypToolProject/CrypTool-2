package enigma;

import common.CtBestList;
import common.CommandLine;
import common.CtAPI;
import common.Flag;

public class BombeSearch {

    static void bombeSearch(String CRIB, byte[] ciphertext, int clen, boolean range, Key lowKey, Key highKey, Key key, String indicatorS, String indicatorMessageKeyS, int HILLCLIMBING_CYCLES, int RIGHT_ROTOR_SAMPLING, MRingScope MIDDLE_RING_SCOPE, boolean VERBOSE, String CRIB_POSITION, int THREADS) {
        byte[] crib = new byte[BombeCrib.MAXCRIBL];
        int maxCribLen = Math.min(BombeCrib.MAXCRIBL, clen);
        if (CRIB.length() > maxCribLen) {
            CtAPI.goodbyeFatalError("Crib too long (%,d letters) - should not be longer than %,d letters\n", CRIB.length(), maxCribLen);
        }
        int crlen = Utils.getText(CRIB, crib);

        int minPos = 0;
        int maxPos = (clen - crlen);
        if (CRIB_POSITION.length() != 0 && !CRIB_POSITION.equalsIgnoreCase("*")) {
            int separator = CRIB_POSITION.indexOf("-");
            if (separator == -1) {
                minPos = getIntValue(CRIB_POSITION, 0, maxPos, Flag.CRIB_POSITION);
                if (minPos == -1)
                    return;
                else
                    maxPos = minPos;
            } else {
                minPos = getIntValue(CRIB_POSITION.substring(0, separator), 0, maxPos, Flag.CRIB_POSITION);
                if (minPos == -1)
                    return;
                maxPos = getIntValue(CRIB_POSITION.substring(separator + 1), minPos, maxPos, Flag.CRIB_POSITION);
                if (maxPos == -1)
                    return;


            }
        }

        int pos = minPos;

        BombeMenu menus[] = new BombeMenu[1000];
        int nMenus = 0;

        while (((pos = BombeCrib.nextValidPosition(ciphertext, clen, crib, crlen, pos)) != -1) && (pos <= maxPos)) {

            BombeCrib bombeCrib = new BombeCrib(ciphertext, crib, crlen, pos, VERBOSE && (minPos == maxPos));

            if ((bombeCrib.menu.score < BombeCrib.BADSCORE) || ((minPos == maxPos))) {
                menus[nMenus++] = bombeCrib.menu;
                CtAPI.printf("Creating Bombe Menu at Position %,d (Links: %,d, Closures:%,d, Score:%.3f)\n",
                        bombeCrib.menu.cribStartPos, bombeCrib.menu.totalItems, bombeCrib.menu.totalClosures, bombeCrib.menu.score);
                if (bombeCrib.menu.score > BombeCrib.BADSCORE)
                    CtAPI.printf("Warning: Turing Score (%.3f) is high (higher means worse) for Bombe menu. This may create many false stops. A longer crib may help.\n",
                            bombeCrib.menu.score);


            }
            pos++;
        }
        if (nMenus > 0) {
            CtAPI.printf("\n %,d Bombe menus created - Starting search using Turing Bombe\n\n", nMenus);
            if (!range) {
                lowKey = highKey = key;
            }
            searchCribMenus(menus, nMenus, lowKey, highKey, MIDDLE_RING_SCOPE, RIGHT_ROTOR_SAMPLING,
                    HILLCLIMBING_CYCLES, THREADS, ciphertext, clen, VERBOSE, indicatorS, indicatorMessageKeyS);

        } else
            CtAPI.printf("No good Bombe menu (Turing Score less than %.3f) found for Crib - Either not enough links/closures, or letters encrypted to themselves\n", BombeCrib.BADSCORE);
    }

    private static int scoreForMenus(double tri, double ic) {



        int res;
        if (tri > 10000.0)
            res = (int) tri;
        else if (ic > 0.050)
            res = (int) (10000.0 * ic / 0.050);
        else
            res = (int) (tri * ic / 0.050);
        if (res < 3000)
            return 0;
        else
            return res;

    }

    private static void searchCribMenus(BombeMenu[] bombeMenus, int nMenus, Key from, Key to,
                                        MRingScope lRingSettingScope, int rRingSpacing,
                                        int hcMaxPass, int THREADS, byte[] ciphertext, int len, boolean debugMenus,
                                        String indicatorS, String indicatorMessageKeyS) {

        Key ckey = new Key(from);
        Key lo = new Key(from);
        Key high = new Key(to);

        double ic;
        int tri;

        byte plaintext[] = new byte[Key.MAXLEN];
        int nStops = 0;
        int bestscore = 0;
        int bestMenu = 0;

        final int MAXTOPS = 100000;
        Key topKeys[] = new Key[MAXTOPS];
        int nTops = 0;

        byte assumedSteckers[] = new byte[26];
        byte strength[] = new byte[26];

        boolean checkForIndicatorMatch = false;
        if (indicatorS.length() * indicatorMessageKeyS.length() != 0)
            checkForIndicatorMatch = true;

        long counter = 0;
        long counterSameMax = 0;
        long countKeys = 0;

        if (lo.mRing == high.mRing)
            lRingSettingScope = MRingScope.ALL;
        long totalKeys = Key.numberOfPossibleKeys(lo, high, len, lRingSettingScope, rRingSpacing, checkForIndicatorMatch);
        CtAPI.printf("\n\nStart Bombe search: Number of menus: %,d, Number of keys: %,d, Total to Check: %,d\n\n", nMenus, totalKeys, nMenus * totalKeys);

        printEstimatedTimeBombeRun(totalKeys * bombeMenus[0].cribLen / 25, nMenus, lRingSettingScope);

        long start = System.currentTimeMillis();

        for (ckey.ukwNum = lo.ukwNum; ckey.ukwNum <= high.ukwNum; ckey.ukwNum++) {
            for (ckey.gSlot = lo.gSlot; ckey.gSlot <= high.gSlot; ckey.gSlot++) {
                for (ckey.lSlot = lo.lSlot; ckey.lSlot <= high.lSlot; ckey.lSlot++) {
                    for (ckey.mSlot = lo.mSlot; ckey.mSlot <= high.mSlot; ckey.mSlot++) {
                        if (ckey.mSlot == ckey.lSlot) continue;
                        for (ckey.rSlot = lo.rSlot; ckey.rSlot <= high.rSlot; ckey.rSlot++) {
                            if (ckey.rSlot == ckey.lSlot || ckey.rSlot == ckey.mSlot) continue;
                            for (ckey.gRing = lo.gRing; ckey.gRing <= high.gRing; ckey.gRing++) {
                                for (ckey.lRing = lo.lRing; ckey.lRing <= high.lRing; ckey.lRing++) {
                                    for (ckey.mRing = lo.mRing; ckey.mRing <= high.mRing; ckey.mRing++) {
                                        for (ckey.rRing = lo.rRing; ckey.rRing <= high.rRing; ckey.rRing++) {
                                            if ((ckey.rRing % rRingSpacing) != 0) continue;
                                            for (ckey.gMesg = lo.gMesg; ckey.gMesg <= high.gMesg; ckey.gMesg++) {
                                                Key keyFromIndicator = null;
                                                if (checkForIndicatorMatch)
                                                    keyFromIndicator = ckey.getKeyFromIndicator(indicatorS, indicatorMessageKeyS);
                                                for (ckey.lMesg = lo.lMesg; ckey.lMesg <= high.lMesg; ckey.lMesg++) {
                                                    if ((checkForIndicatorMatch) && (ckey.lMesg != keyFromIndicator.lMesg))
                                                        continue;
                                                    for (ckey.mMesg = lo.mMesg; ckey.mMesg <= high.mMesg; ckey.mMesg++) {
                                                        if ((checkForIndicatorMatch) && (ckey.mMesg != keyFromIndicator.mMesg))
                                                            continue;
                                                        for (ckey.rMesg = lo.rMesg; ckey.rMesg <= high.rMesg; ckey.rMesg++) {
                                                            if ((checkForIndicatorMatch) && (ckey.lMesg != keyFromIndicator.lMesg))
                                                                continue;


                                                            if (lRingSettingScope != MRingScope.ALL) {
                                                                int lRingSteppingPos = ckey.getLeftRotorSteppingPosition(len);
                                                                if (!Key.CheckValidWheelsState(len, lRingSteppingPos, lRingSettingScope))
                                                                    continue;
                                                            }

                                                            countKeys++;
                                                            ReportResult.displayProgress(countKeys, totalKeys);
                                                            boolean foundForThisKey = false;
                                                            for (int m = 0; (m < nMenus) && !foundForThisKey; m++) {
                                                                counter++;

                                                                if (ckey.model == Key.Model.M4)
                                                                    ckey.initPathLookupHandM4Range(bombeMenus[m].cribStartPos, bombeMenus[m].cribLen);
                                                                else
                                                                    ckey.initPathLookupHandM3Range(bombeMenus[m].cribStartPos, bombeMenus[m].cribLen);

                                                                for (int i = 0; i < 26; i++) {
                                                                    assumedSteckers[i] = -1;
                                                                    strength[i] = 0;
                                                                }


                                                                if (bombeMenus[m].testIfBombsStops(0, ckey.lookup, assumedSteckers, strength, false)) {
                                                                    foundForThisKey = true;
                                                                    int stb[] = new int[26];
                                                                    for (int i = 0; i < 26; i++)
                                                                        if (assumedSteckers[i] == -1)
                                                                            stb[i] = i;
                                                                        else
                                                                            stb[i] = assumedSteckers[i];
                                                                    ckey.setStecker(stb);
                                                                    tri = ckey.triScoreWithoutLookupBuild(ciphertext, len);
                                                                    ic = ckey.icScoreWithoutLookupBuild(ciphertext, len);
                                                                    ckey.score = scoreForMenus(tri, ic);


                                                                    if (ckey.score > 0) {
                                                                        nStops++;
                                                                        if (nStops == (MAXTOPS - 1)) {
                                                                            CtAPI.printf("\n\nWARNING: Too many stops - Only the top %,d keys (sorted by IC and Trigrams) will be kept for Hill Climbing\n", MAXTOPS);
                                                                            CtAPI.print("Bombe search with the current crib parameters (crib string and position/position range) may be inefficient and/or miss the right key.\n");
                                                                            CtAPI.print("It is recommended to either reduce the key range, use a longer crib, or specify fewer positions to search for the crib.\n\n");
                                                                        }
                                                                        boolean sort = false;
                                                                        if (nTops < MAXTOPS) {
                                                                            topKeys[nTops++] = new Key(ckey);
                                                                            sort = true;
                                                                        } else if (ckey.score > topKeys[nTops - 1].score) {
                                                                            topKeys[nTops - 1] = new Key(ckey);
                                                                            sort = true;
                                                                        }

                                                                        if (sort) {
                                                                            for (int i = (nTops - 1); i >= 1; i--)
                                                                                if (topKeys[i].score > topKeys[i - 1].score) {
                                                                                    Key tempKey = topKeys[i];
                                                                                    topKeys[i] = topKeys[i - 1];
                                                                                    topKeys[i - 1] = tempKey;
                                                                                }
                                                                        }
                                                                    }

                                                                    if (ckey.score == bestscore) {
                                                                        counterSameMax++;
                                                                        if (counterSameMax == 100)
                                                                            CtAPI.printf("WARNING: Too many stops with same score (%,d). Only stops with higher scores will be displayed\n", bestscore);


                                                                    }

                                                                    if ((ckey.score > bestscore) || ((ckey.score == bestscore) && (counterSameMax < 100))) {
                                                                        if (ckey.score > bestscore)
                                                                            counterSameMax = 0;


                                                                        bestMenu = m;
                                                                        bestscore = ckey.score;

                                                                        printStop(bombeMenus[m], ciphertext, len, ckey, ic, tri, plaintext, assumedSteckers, strength, start, totalKeys, countKeys);

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


        if ((nStops == 1) && (debugMenus)) {
            ckey = new Key(from);

            if (ckey.model == Key.Model.M4)
                ckey.initPathLookupRange(bombeMenus[0].cribStartPos, bombeMenus[0].cribLen);
            else
                ckey.initPathLookupHandM3Range(bombeMenus[0].cribStartPos, bombeMenus[0].cribLen);


            for (int i = 0; i < 26; i++) {
                assumedSteckers[i] = -1;
                strength[i] = 0;
            }

            //valid = bombeMenus[0].TestValidity(0,ckey.path_lookup, assumedSteckers, true);
            bombeMenus[0].testIfBombsStops(0, ckey.lookup, assumedSteckers, strength, true);

        }
        long elapsed = (System.currentTimeMillis() - start + 1);


        if (nMenus == 1)
            CtAPI.printf("End of Bombe Search >>%s<< at Position: %,d (Turing Score: %.3f Closures: %,d Links: %,d) \n\nFOUND %,d STOP(s) \n\n%,d Total Keys Tested in %.1f Seconds(%,d/sec)\n\n",
                    Utils.getString(bombeMenus[0].crib, bombeMenus[0].cribLen),
                    bombeMenus[0].cribStartPos, bombeMenus[0].score, bombeMenus[0].totalClosures, bombeMenus[0].totalItems,
                    nStops, counter, elapsed / 1000.0, 1000 * counter / elapsed);
        else if (nStops > 0)
            CtAPI.printf("End of Bombe Search >>%s<< for %,d Menus - Best menu found for Position: %,d (Turing Score: %.3f Closures: %,d Links: %,d) \n\nFOUND %,d STOP(S) \n %,d Total Keys/Menu Combinations Tested in %.1f Seconds(%,d/sec)\n\n",
                    Utils.getString(bombeMenus[bestMenu].crib, bombeMenus[bestMenu].cribLen),
                    nMenus,
                    bombeMenus[bestMenu].cribStartPos, bombeMenus[bestMenu].score, bombeMenus[bestMenu].totalClosures, bombeMenus[bestMenu].totalItems,
                    nStops, counter, elapsed / 1000.0, 1000 * counter / elapsed);

        else
            CtAPI.printf("End of Bombe Search >>%s<< for %,d Menus \n\nNO STOP FOUND! \n\n%,d Keys&Menu Combinations Tested in %.1f Seconds(%,d/sec)\n\n",
                    Utils.getString(bombeMenus[0].crib, bombeMenus[0].cribLen),
                    nMenus, counter, elapsed / 1000.0, 1000 * counter / elapsed);


        if ((nTops >= 10) && (hcMaxPass > 0))
            CtAPI.printf("Menu Bombe - Starting batch of %,d Keys; Min Score : %,d, Median Score: %,d, Max Score: %,d\n",
                    nTops, topKeys[nTops - 1].score, topKeys[nTops / 2].score, topKeys[0].score);

        if ((nTops > 0) && (hcMaxPass > 0))

            HillClimb.hillClimbBatch(topKeys, nTops, hcMaxPass, THREADS, 10000, ciphertext, len, rRingSpacing);


    }

    private static void printEstimatedTimeBombeRun(long normalizedNkeys1, int nMenus, MRingScope lRingSettingScope) {

        int minRate;
        int maxRate;

        if (lRingSettingScope == MRingScope.ALL) {
            minRate = 50000;
            maxRate = 100000;
        } else if (lRingSettingScope == MRingScope.ONE_NON_STEPPING) {
            minRate = 15000;
            maxRate = 30000;
        } else {
            minRate = 25000;
            maxRate = 50000;
        }

        CtAPI.printf("Estimated Search Time: %s for a small number of stops (more if many stops are found)\n\n", Utils.getEstimatedTimeString(nMenus * normalizedNkeys1, minRate, maxRate));
    }

    private static void printStop(BombeMenu bombeMenu, byte[] ciphertext, int len, Key ckey, double ic, int tri, byte[] plaintext, byte[] assumedSteckers, byte[] strength, long startTime, long totalKeys, long counterKeys) {
        String plains;

        StringBuilder stbs = new StringBuilder();
        String confirmedSelfS = "";
        StringBuilder strengthStbs = new StringBuilder();
        String strengthSelfS = "";

        for (int i = 0; i < 26; i++) {

            int s = assumedSteckers[i];
            if (i < s) {
                stbs.append("").append(Utils.getChar(i)).append(Utils.getChar(s));

                if (strength[i] > 0)
                    strengthStbs.append(" ").append(Utils.getChar(i)).append(Utils.getChar(s)).append("{").append(strength[i]).append("}");

            } else if (i == s) {
                confirmedSelfS += "" + Utils.getChar(i);
                if (strength[i] > 0)
                    strengthSelfS += " " + Utils.getChar(i) + "{" + strength[i] + "}";
            }

        }

        ckey.setStecker(stbs.toString());
        ckey.encipherDecipherAll(ciphertext, plaintext, len);

        plains = Utils.getString(plaintext, len);
        long elapsed = System.currentTimeMillis() - startTime;
        String desc = String.format("BOMBE [Pos: %3d][%,5dK/%,5dK][%,4dK/sec][%,4d Sec]",
                bombeMenu.cribStartPos, counterKeys / 1000, totalKeys / 1000, counterKeys / elapsed, elapsed / 1000);

        if (CtBestList.shouldPushResult(ckey.score)) {
            ReportResult.reportResult(0, ckey, ckey.score, plains, desc);

            CtAPI.printf("MENU STOP NEW BEST - Pos: %,d Stop Score: %,d (Tri: %,d IC: %.5f) - Crib Length: %,d, Crib: %s\n",
                    bombeMenu.cribStartPos, ckey.score, tri, ic, bombeMenu.cribLen,
                    Utils.getString(bombeMenu.crib, bombeMenu.cribLen));
            CtAPI.printf("Stecker: [ Pairs: %s (%,d) Self: %s (%,d) Total: %,d ] - Confirmation Strength: Pairs: %s Self: %s\n",
                    stbs.toString(), stbs.length(), confirmedSelfS, confirmedSelfS.length(), (stbs.length() + confirmedSelfS.length()),
                    strengthStbs.toString(), strengthSelfS);

            ckey.printKeyString("");
            CtAPI.printf("\n%s\n\n", plains);
        }
    }


    private static int getIntValue(String s, int min, int max, Flag flag) {

        for (int i = 0; i < s.length(); i++) {
            if (Utils.getDigitIndex(s.charAt(i)) == -1) {
                CtAPI.goodbyeFatalError("Invalid %s (%s) for %s - Expecting number from %,d to %,d \n", s, CommandLine.getShortDesc(flag), flag, min, max);
            }
        }

        int intValue = Integer.parseInt(s);

        if ((intValue >= min) && (intValue <= max)) {
            return intValue;
        }
        CtAPI.goodbyeFatalError("Invalid %s (%s) for %s - Expecting number from %,d to %,d \n", s, CommandLine.getShortDesc(flag), flag, min, max);
        return -1;

    }
}
