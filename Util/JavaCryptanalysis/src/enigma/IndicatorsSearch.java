package enigma;

import common.CtBestList;
import common.CtAPI;
import common.Flag;

import static java.lang.Thread.sleep;

public class IndicatorsSearch {
    static void indicatorsSearch(String INDICATORS_FILE, Key lowKey, Key highKey, String steckerS, byte[] ciphertext, int clen, int RIGHT_ROTOR_SAMPLING, MRingScope MIDDLE_RING_SCOPE, int HILLCLIMBING_CYCLES, int THREADS) {
        int A = Utils.getIndex('A');
        int Z = Utils.getIndex('Z');
            /*
            if ((lowKey.l_ring != Z)|| (lowKey.m_ring != Z) || (lowKey.r_ring != Z) ||
                (highKey.l_ring != Z) || (highKey.m_ring != Z) || (highKey.r_ring != Z)) {
                CtAPI.printf("WARNING: Cycle Match Search (-O): Ignoring range ring settings. Will use ZZZ instead.\n\n");
//                lowKey.l_ring = lowKey.m_ring = lowKey.r_ring = Z;
//                highKey.l_ring = highKey.m_ring = highKey.r_ring = Z;
                lowKey.l_ring = lowKey.m_ring = lowKey.r_ring = A;
                highKey.r_ring = Z;
                highKey.l_ring = highKey.m_ring = A;
            }
            */
        if (steckerS.length() != 0) {
            System.out.print("INDICATORS WARNING: Cycle Match Search (-O): Ignoring Stecker Settings. \n\n");
            lowKey.setStecker("");
            highKey.setStecker("");
        }

        byte indicData[] = new byte[Key.MAXLEN];
        int flen = -1;
        if (INDICATORS_FILE.length() != 0)
            flen = Utils.loadCipherText(INDICATORS_FILE, indicData, false);
        if ((flen < 6) || (flen % 6 != 0)) {
            CtAPI.goodbyeFatalError("INDICATORS Cycle Match Search (-%s INDICATORS): Failed to load indicators data from file %s (%d characters found).\n",
                    Flag.MODE, INDICATORS_FILE, flen);
        }
        CtAPI.printf("INDICATORS Cycle Match Search: Read database file %s, %d Indicator groups found\nFirst Indicator: %s\n",
                INDICATORS_FILE, flen / 6, Utils.getString(indicData, 6));

        searchCycleMatch(indicData, flen, lowKey, highKey, HILLCLIMBING_CYCLES, THREADS, ciphertext, clen, MIDDLE_RING_SCOPE, RIGHT_ROTOR_SAMPLING);

    }
    private static void searchCycleMatch(byte[] indicData, int flen, Key from, Key to, int HILLCLIMBING_CYCLES, int THREADS, byte[] ciphertext, int clen,
                                        MRingScope lRingSettingScope, int rRingSpacing) {


        Key.MAX_STB_PLUGS = 12;
        int nIndics = flen / 6;

        //nIndics -= 4;

        int[][] dbCycleSizes = new int[3][26];
        int[][] links = new int[3][26];
        for (int pos = 0; pos < 3; pos++)
            for (int l = 0; l < 26; l++)
                links[pos][l] = -1;

        for (int i = 0; i < nIndics; i++) {
            for (int pos = 0; pos < 3; pos++) {
                int letterAtPos = indicData[i * 6 + pos];
                int letterAtPosPlus3 = indicData[i * 6 + pos + 3];
                if ((links[pos][letterAtPos] != -1) && (links[pos][letterAtPos] != letterAtPosPlus3)) {
                    CtAPI.printf("Cycle Match Search: Conflict with Indicator #%d with letter [%s] (position %d) and letter [%s] (position %d)\n",
                            i, Utils.getChar(letterAtPos), pos, Utils.getChar(letterAtPosPlus3), pos + 3);
                } else {
                    links[pos][letterAtPos] = letterAtPosPlus3;
                }

            }
        }
        Key ckey = new Key(from);
        Key lo = new Key(from);
        Key high = new Key(to);

        long counterKeys = 0;

        if (lo.mRing == high.mRing)
            lRingSettingScope = MRingScope.ALL;
        long totalKeys = Key.numberOfPossibleKeys(lo, high, 6, lRingSettingScope, rRingSpacing, false);


        boolean hillClimbOnly = false;
        if (!Key.buildCycles(links, dbCycleSizes, true)) {
            CtAPI.print("Cycle Match Search: Not enough indicators/cycles are not complete.\n");
            CtAPI.print("Cycle Match Search: Cycles are ignored - searching only via Hill Climbing.\n");
            hillClimbOnly = true;
        }

        if (lo.mRing == high.mRing)
            lRingSettingScope = MRingScope.ALL;

        CtAPI.print("\nCycle Match Search: Starting ....\n\n");
        int matchingKeys = 0;
        long startTime = System.currentTimeMillis();

        for (ckey.ukwNum = lo.ukwNum; ckey.ukwNum <= high.ukwNum; ckey.ukwNum++) {
            for (ckey.gSlot = lo.gSlot; ckey.gSlot <= lo.gSlot; ckey.gSlot++) {
                for (ckey.lSlot = lo.lSlot; ckey.lSlot <= high.lSlot; ckey.lSlot++) {
                    for (ckey.mSlot = lo.mSlot; ckey.mSlot <= high.mSlot; ckey.mSlot++) {
                        if (ckey.mSlot == ckey.lSlot) continue;
                        for (ckey.rSlot = lo.rSlot; ckey.rSlot <= high.rSlot; ckey.rSlot++) {
                            if (ckey.rSlot == ckey.lSlot || ckey.rSlot == ckey.mSlot) continue;

                            ckey.setStecker("");
                            ckey.lMesg = ckey.mMesg = ckey.rMesg = -1;
                            ckey.lRing = ckey.mRing = ckey.rRing = -1;
                            ckey.score = 0;
                            ckey.printKeyString("Progress....");

                            for (ckey.gRing = lo.gRing; ckey.gRing <= lo.gRing; ckey.gRing++) {
                                for (ckey.lRing = lo.lRing; ckey.lRing <= high.lRing; ckey.lRing++) {
                                    for (ckey.mRing = lo.mRing; ckey.mRing <= high.mRing; ckey.mRing++) {
                                        for (ckey.rRing = lo.rRing; ckey.rRing <= high.rRing; ckey.rRing++) {
                                            if ((ckey.rRing % rRingSpacing) != 0)
                                                continue;
                                            for (ckey.gMesg = lo.gMesg; ckey.gMesg <= lo.gMesg; ckey.gMesg++) {
                                                for (ckey.lMesg = lo.lMesg; ckey.lMesg <= high.lMesg; ckey.lMesg++) {
                                                    if (hillClimbOnly) {
                                                        ckey.score = 0;
                                                        ckey.setStecker("");
                                                        ckey.printKeyString("Progress....");
                                                    }
                                                    for (ckey.mMesg = lo.mMesg; ckey.mMesg <= high.mMesg; ckey.mMesg++) {
                                                        for (ckey.rMesg = lo.rMesg; ckey.rMesg <= high.rMesg; ckey.rMesg++) {


                                                            if (lRingSettingScope != MRingScope.ALL) {
                                                                int mRingSteppingPos = ckey.getLeftRotorSteppingPosition(6);
                                                                if (!Key.CheckValidWheelsState(6, mRingSteppingPos, lRingSettingScope))
                                                                    continue;
                                                            }
                                                            counterKeys++;
                                                            ReportResult.displayProgress(counterKeys, totalKeys);
                                                            boolean valid = false;

                                                            if (!hillClimbOnly)
                                                                valid = ckey.buildAndCompareCycles(dbCycleSizes, true);

                                                            if (valid || hillClimbOnly) {

                                                                if (!hillClimbOnly) {
                                                                    ckey.score = 0;
                                                                    ckey.setStecker("");
                                                                    //ckey.printKeyString("Candidate");
                                                                }
                                                                byte[][] indicCiphertext = new byte[nIndics][6];
                                                                for (int i = 0; i < nIndics; i++)
                                                                    System.arraycopy(indicData, i * 6, indicCiphertext[i], 0, 6);

                                                                // clean up stecker info from previous runs.
                                                                ckey.setStecker("");
                                                                int runs;
                                                                for (runs = 0; runs < (hillClimbOnly ? (nIndics <= 10 ? 5 : 2) : 100); runs++) {
                                                                    if (runs > 0) {
                                                                        ckey.setRandomStb(4);
                                                                    }
                                                                    int prevScore = ckey.score = 0;
                                                                    while (true) {
                                                                        hillClimbIndicator(ckey, null, indicCiphertext, nIndics, false);

                                                                        if ((ckey.score <= prevScore) || (ckey.score == 1000)) {
                                                                            break;
                                                                        }
                                                                        prevScore = ckey.score;
                                                                    }
                                                                    if (ckey.score == 1000) {
                                                                        break;
                                                                    }
                                                                }

                                                                if (ckey.score > 900) {
                                                                    //if (ckey.score > 660) {
                                                                    ckey.printKeyString("Candidate");

                                                                    long elapsed = System.currentTimeMillis() - startTime;
                                                                    String desc = String.format("INDICATORS DAILY KEY [%,5dK/%,5dK][%,4dK/sec][%,4d Sec]",
                                                                            counterKeys / 1000, totalKeys / 1000, counterKeys / elapsed, elapsed / 1000);
                                                                    byte[] indicPlaintext = new byte[6];

                                                                    StringBuilder allIndicsPlaintext = new StringBuilder();
                                                                    for (int i = 0; i < nIndics; i++) {
                                                                        ckey.encipherDecipherAll(indicCiphertext[i], indicPlaintext, 6);
                                                                        allIndicsPlaintext.append(Utils.getString(indicPlaintext, 6)).append("   ");
                                                                    }
                                                                    if (CtBestList.shouldPushResult(ckey.score)) {
                                                                        ReportResult.reportResult(0, ckey, ckey.score, allIndicsPlaintext.toString(), desc);
                                                                    }

                                                                    CtAPI.printf("\nINDICATORS Cycle Match Search: Found Message Key settings (%s) which matches the cycle patterns of the indicators (%d keys tested)\n\n",
                                                                            "" + Utils.getChar(ckey.lMesg) + Utils.getChar(ckey.mMesg) + Utils.getChar(ckey.rMesg),
                                                                            counterKeys);
                                                                    CtAPI.printf("INDICATORS Cycle Match Search: Found Stecker Board settings - Score %d/1000 (%d runs) - Stecker Settings: %s (%d plugs)\n\n",
                                                                            ckey.score, runs + 1, ckey.stbString(), ckey.stbString().length() / 2);
                                                                    System.out.print("INDICATORS Cycle Match Search: Showing the deciphered double indicators:\n\n");

                                                                    StringBuilder allIndicsWithNewlines = new StringBuilder();
                                                                    for (int i = 0; i < nIndics; i++) {
                                                                        ckey.encipherDecipherAll(indicCiphertext[i], indicPlaintext, 6);
                                                                        allIndicsWithNewlines.append(String.format("  %s ==> %s",
                                                                                Utils.getString(indicCiphertext[i], 6),
                                                                                Utils.getString(indicPlaintext, 6)));
                                                                        if ((i % 6) == 5) {
                                                                            allIndicsWithNewlines.append("\n");
                                                                        }
                                                                    }
                                                                    System.out.printf("%s\n", allIndicsWithNewlines.toString());

                                                                    if (ckey.score > 900) {

                                                                        if (clen > 0) {
                                                                            // use the first deciphered indicator as message key to return in the key.
                                                                            ckey.encipherDecipherAll(indicCiphertext[0], indicPlaintext, 6);
                                                                            ckey.lMesg = indicPlaintext[0];
                                                                            ckey.mMesg = indicPlaintext[1];
                                                                            ckey.rMesg = indicPlaintext[2];
                                                                            CtAPI.printf("\nINDICATORS Cycle Match Search: Using the first indicator (%s after decryption) as the Message Key (still need to find out the Ring Settings - not yet correct) \n\n",
                                                                                    "" + Utils.getChar(indicPlaintext[0]) + Utils.getChar(indicPlaintext[1]) + Utils.getChar(indicPlaintext[2]));
                                                                            ckey.score = 0;
                                                                            ckey.printKeyString("");

                                                                            // only sweep ring values (with no limitation
                                                                            Key lowKey = new Key(ckey);
                                                                            lowKey.lRing = lowKey.mRing = lowKey.rRing = Utils.getIndex('A');
                                                                            Key highKey = new Key(ckey);
                                                                            highKey.lRing = highKey.mRing = highKey.rRing = Utils.getIndex('Z');
                                                                            CtAPI.print("\nINDICATORS Cycle Match Search: Running a Trigram Search to find the exact Ring Settings and decipher the message.\n");
                                                                            TrigramICSearch.searchTrigramIC(lowKey, highKey, false, MRingScope.ALL, 1, false, HILLCLIMBING_CYCLES, 10000, THREADS, ciphertext, clen, "", "");
                                                                            ReportResult.displayProgress(counterKeys, totalKeys);

                                                                            try {
                                                                                sleep(2000);
                                                                            } catch (InterruptedException e) {
                                                                                e.printStackTrace();
                                                                            }
                                                                            ReportResult.displayProgress(counterKeys, totalKeys);

                                                                            return;

                                                                        }
                                                                        matchingKeys++;
                                                                    }
                                                                }
                                                            } // if valid
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

        long elapsed = (System.currentTimeMillis() - startTime + 1);

        CtAPI.printf("INDICATORS End of Cycle Search - %d matching keys found \n %d Total Keys Tested in %.1f Seconds(%d/sec)\n\n",
                matchingKeys, counterKeys, elapsed / 1000.0, 1000 * counterKeys / elapsed);

    }

    public static void hillClimbIndicator(Key ckey, byte[][] indicMsgKeys, byte[][] indicCiphertext, int nIndics, boolean print) {


        int[] var = new int[26];
        for (int i = 0; i < 26; i++) {
            var[i] = i;
        }
        Key.randVar(var);

        double bestindic, indic;

        final double DBL_EPSILON = 0.000000001;

        SearchAction action;

        if (print)
            System.out.print("Indicator Deciphering Hill Climbing\n");

        bestindic = ckey.indicScore(indicMsgKeys, indicCiphertext, nIndics);
        double prev = bestindic;
        for (int i = 0; i < 26; i++) {
            int vi = var[i];
            for (int k = i + 1; k < 26; k++) {
                int vk = var[k];
                int vsk = ckey.stbrett[vk];
                int vsi = ckey.stbrett[vi]; // not an invariant

                action = SearchAction.NO_CHANGE;

                if (vi == vsi && vk == vsk) {
                    //ckey.swap(vi, vk);
                    ckey.stbConnect(vi, vk);
                    indic = ckey.indicScore(indicMsgKeys, indicCiphertext, nIndics);
                    if (indic - bestindic > DBL_EPSILON) {
                        bestindic = indic;
                        action = SearchAction.IandK;

                    }
                    if (action == SearchAction.NO_CHANGE)
                        //ckey.swap(vi, vk);
                        ckey.stbDisconnect(vi, vk);
                } else if (vi == vsi && vk != vsk) {
                    //ckey.swap(vk, vsk);
                    ckey.stbDisconnect(vk, vsk);

                    // all self
                    //ckey.swap(vi, vk);
                    ckey.stbConnect(vi, vk);

                    indic = ckey.indicScore(indicMsgKeys, indicCiphertext, nIndics);
                    if (indic - bestindic > DBL_EPSILON) {
                        bestindic = indic;
                        action = SearchAction.IandK;
                    }
                    //ckey.swap(vi, vk);
                    ckey.stbDisconnect(vi, vk);
                    // all self now

                    //ckey.swap(vi, vsk);
                    ckey.stbConnect(vi, vsk);

                    indic = ckey.indicScore(indicMsgKeys, indicCiphertext, nIndics);
                    if (indic - bestindic > DBL_EPSILON) {
                        bestindic = indic;
                        action = SearchAction.IandSK;
                    }
                    //ckey.swap(vi, vsk);
                    ckey.stbDisconnect(vi, vsk);
                    // all self now

                    switch (action) {
                        case IandK:
                            //ckey.swap(vi, vk);
                            ckey.stbConnect(vi, vk);
                            break;
                        case IandSK:
                            //ckey.swap(vi, vsk);
                            ckey.stbConnect(vi, vsk);
                            break;
                        case NO_CHANGE:
                            //ckey.swap(vk, vsk);
                            ckey.stbConnect(vk, vsk);
                            break;
                        default:
                            break;
                    }
                } else if (vk == vsk && vi != vsi) {
                    //ckey.swap(vi, vsi);
                    ckey.stbDisconnect(vi, vsi);
                    // all self

                    //ckey.swap(vk, vi);
                    ckey.stbConnect(vk, vi);

                    indic = ckey.indicScore(indicMsgKeys, indicCiphertext, nIndics);
                    if (indic - bestindic > DBL_EPSILON) {
                        bestindic = indic;
                        action = SearchAction.IandK;
                    }
                    //ckey.swap(vk, vi);
                    ckey.stbDisconnect(vk, vi);
                    // all self

                    //ckey.swap(vk, vsi);
                    ckey.stbConnect(vk, vsi);

                    indic = ckey.indicScore(indicMsgKeys, indicCiphertext, nIndics);
                    if (indic - bestindic > DBL_EPSILON) {
                        bestindic = indic;
                        action = SearchAction.KandSI;
                    }
                    //ckey.swap(vk, vsi);
                    ckey.stbDisconnect(vk, vsi);
                    // all self

                    switch (action) {
                        case IandK:
                            //ckey.swap(vi, vk);
                            ckey.stbConnect(vi, vk);
                            break;
                        case KandSI:
                            //ckey.swap(vk, vsi);
                            ckey.stbConnect(vk, vsi);
                            break;
                        case NO_CHANGE:
                            //ckey.swap(vi, vsi);
                            ckey.stbConnect(vi, vsi);
                            break;
                        default:
                            break;
                    }
                } else if (vi != vsi && vk != vsk) {
                    //ckey.swap(vi, vsi);
                    ckey.stbDisconnect(vi, vsi);

                    indic = ckey.indicScore(indicMsgKeys, indicCiphertext, nIndics);
                    if (indic - bestindic > DBL_EPSILON) {
                        bestindic = indic;
                        action = SearchAction.KandSK;
                    }
                    //ckey.swap(vk, vsk);
                    ckey.stbDisconnect(vk, vsk);
                    // all Self now

                    //ckey.swap(vi, vsi);
                    ckey.stbConnect(vi, vsi);

                    indic = ckey.indicScore(indicMsgKeys, indicCiphertext, nIndics);
                    if (indic - bestindic > DBL_EPSILON) {
                        bestindic = indic;
                        action = SearchAction.IandSI;
                    }
                    //ckey.swap(vi, vsi);
                    ckey.stbDisconnect(vi, vsi);

                    // all Self now

                    //ckey.swap(vi, vk);
                    ckey.stbConnect(vi, vk);

                    indic = ckey.indicScore(indicMsgKeys, indicCiphertext, nIndics);
                    if (indic - bestindic > DBL_EPSILON) {
                        bestindic = indic;
                        action = SearchAction.IandK;
                    }

                    //ckey.swap(vsi, vsk);
                    ckey.stbConnect(vsi, vsk);

                    indic = ckey.indicScore(indicMsgKeys, indicCiphertext, nIndics);
                    if (indic - bestindic > DBL_EPSILON) {
                        bestindic = indic;
                        action = SearchAction.IandK_SIandSK;
                    }
                    //ckey.swap(vi, vk);
                    ckey.stbDisconnect(vi, vk);


                    indic = ckey.indicScore(indicMsgKeys, indicCiphertext, nIndics);
                    if (indic - bestindic > DBL_EPSILON) {
                        bestindic = indic;
                        action = SearchAction.SIandSK;
                    }

                    //ckey.swap(vsi, vsk);
                    ckey.stbDisconnect(vsi, vsk);

                    // all Self now


                    //ckey.swap(vi, vsk);
                    ckey.stbConnect(vi, vsk);

                    indic = ckey.indicScore(indicMsgKeys, indicCiphertext, nIndics);
                    if (indic - bestindic > DBL_EPSILON) {
                        bestindic = indic;
                        action = SearchAction.IandSK;
                    }
                    //ckey.swap(vsi, vk);
                    ckey.stbConnect(vsi, vk);

                    indic = ckey.indicScore(indicMsgKeys, indicCiphertext, nIndics);
                    if (indic - bestindic > DBL_EPSILON) {
                        bestindic = indic;
                        action = SearchAction.IandSK_KandSI;
                    }
                    //ckey.swap(vi, vsk);
                    ckey.stbDisconnect(vi, vsk);


                    indic = ckey.indicScore(indicMsgKeys, indicCiphertext, nIndics);
                    if (indic - bestindic > DBL_EPSILON) {
                        bestindic = indic;
                        action = SearchAction.KandSI;
                    }

                    //ckey.swap(vsi, vk);
                    ckey.stbDisconnect(vsi, vk);

                    // all Self now


                    switch (action) {
                        case KandSK:
                            //ckey.swap(vk, vsk);
                            ckey.stbConnect(vk, vsk);
                            break;
                        case IandSI:
                            //ckey.swap(vi, vsi);
                            ckey.stbConnect(vi, vsi);
                            break;
                        case IandK:
                            //ckey.swap(vi, vk);
                            ckey.stbConnect(vi, vk);
                            break;
                        case IandSK:
                            //ckey.swap(vi, vsk);
                            ckey.stbConnect(vi, vsk);
                            break;
                        case KandSI:
                            //ckey.swap(vk, vsi);
                            ckey.stbConnect(vk, vsi);
                            break;
                        case IandK_SIandSK:
                            //ckey.swap(vi, vk);
                            //ckey.swap(vsi, vsk);
                            ckey.stbConnect(vi, vk);
                            ckey.stbConnect(vsi, vsk);
                            break;
                        case SIandSK:
                            //ckey.swap(vsi, vsk);
                            ckey.stbConnect(vsi, vsk);
                            break;
                        case IandSK_KandSI:
                            //ckey.swap(vi, vsk);
                            //ckey.swap(vsi, vk);
                            ckey.stbConnect(vi, vsk);
                            ckey.stbConnect(vsi, vk);
                            break;
                        case NO_CHANGE:
                            //ckey.swap(vi, vsi);
                            //ckey.swap(vk, vsk);
                            ckey.stbConnect(vi, vsi);
                            ckey.stbConnect(vk, vsk);
                            break;
                        default:
                            break;
                    }
                }
                if (print && (action != SearchAction.NO_CHANGE))

                    System.out.printf("INDIC HC  - \tI [%s] SI [%s] K [%s] SK [%s] \tSCORE \t%f (%f) \t>%s<\t" + action + "\n",
                            Utils.getChar(vi), Utils.getChar(vsi), Utils.getChar(vk), Utils.getChar(vsk), bestindic, bestindic - prev, ckey.stbString());


            }
        }

        ckey.score = (int) bestindic;

    }
}
