package enigma;


import common.CtBestList;
import common.CtAPI;

class TrigramICSearch {

    public static void searchTrigramIC(Key from, Key to, boolean findSettingsIc,
                                       MRingScope lRingSettingScope, int rRingSpacing,
                                       boolean hcEveryBest, int hcMaxPass, int minTrigramsScoreToPrint, int THREADS, byte[] ciphertext, int len,
                                       String indicatorS, String indicatorMessageKeyS) {

        Key ckey = new Key(from);
        Key lo = new Key(from);
        Key bestKey = null;

        Key high = new Key(to);

        double best = 0.0;

        byte plaintext[] = new byte[Key.MAXLEN];

        long counter = 0;

        boolean checkForIndicatorMatch = false;
        if (indicatorS.length() * indicatorMessageKeyS.length() != 0)
            checkForIndicatorMatch = true;

        if (lo.mRing == high.mRing)
            lRingSettingScope = MRingScope.ALL;
        long totalKeys = Key.numberOfPossibleKeys(lo, high, len, lRingSettingScope, rRingSpacing, checkForIndicatorMatch);

        long normalizedNkeys = (totalKeys * len) / 250;

        int minRate;
        int maxRate;

        if (lRingSettingScope == MRingScope.ALL) {
            minRate = 100000;
            maxRate = 150000;
        } else if (lRingSettingScope == MRingScope.ONE_NON_STEPPING) {
            minRate = 20000;
            maxRate = 30000;
        } else {
            minRate = 50000;
            maxRate = 75000;
        }

        CtAPI.printf("\n\nSTARTING %s SEARCH: Number of Keys to search: %,d \n\n", findSettingsIc ? "IC" : "TRIGRAM", totalKeys);
        CtAPI.printf("Estimated TrigramICSearch Time: %s\n\n", Utils.getEstimatedTimeString(normalizedNkeys, minRate, maxRate));

        long startTime = System.currentTimeMillis();

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
                                            if ((ckey.rRing % rRingSpacing) != 0)
                                                continue;
                                            Key keyFromIndicator = null;
                                            if (checkForIndicatorMatch)
                                                keyFromIndicator = ckey.getKeyFromIndicator(indicatorS, indicatorMessageKeyS);
                                            for (ckey.gMesg = lo.gMesg; ckey.gMesg <= high.gMesg; ckey.gMesg++) {
                                                for (ckey.lMesg = lo.lMesg; ckey.lMesg <= high.lMesg; ckey.lMesg++) {
                                                    if ((checkForIndicatorMatch) && (ckey.lMesg != keyFromIndicator.lMesg))
                                                        continue;
                                                    for (ckey.mMesg = lo.mMesg; ckey.mMesg <= high.mMesg; ckey.mMesg++) {
                                                        if ((checkForIndicatorMatch) && (ckey.mMesg != keyFromIndicator.mMesg))
                                                            continue;
                                                        for (ckey.rMesg = lo.rMesg; ckey.rMesg <= high.rMesg; ckey.rMesg++) {
                                                            if ((checkForIndicatorMatch) && (ckey.rMesg != keyFromIndicator.rMesg))
                                                                continue;

                                                            if (lRingSettingScope != MRingScope.ALL) {
                                                                int mRingSteppingPos = ckey.getLeftRotorSteppingPosition(len);
                                                                if (!Key.CheckValidWheelsState(len, mRingSteppingPos, lRingSettingScope))
                                                                    continue;
                                                            }

                                                            counter++;
                                                            ReportResult.displayProgress(counter, totalKeys);

                                                            if (findSettingsIc)
                                                                ckey.score = (int) (100000.0 * ckey.icScoreWithoutLookupBuild(ciphertext, len));
                                                            else
                                                                ckey.score = ckey.triScoreWithoutLookupBuild(ciphertext, len);

                                                            if (ckey.score < minTrigramsScoreToPrint) {
                                                                continue;
                                                            }
                                                            if (ckey.score - best >= 0) {
                                                                best = ckey.score;
                                                                bestKey = new Key(ckey);

                                                                if ((hcEveryBest) && (hcMaxPass > 0)) {
                                                                    if ((findSettingsIc && (ckey.score > 3500)) || (!findSettingsIc && (ckey.score > 10000))) {
                                                                        HillClimb.hillClimbRange(bestKey, bestKey, hcMaxPass, THREADS,
                                                                                minTrigramsScoreToPrint, MRingScope.ALL, 1, ciphertext, len, HcSaRunnable.Mode.SA, 5);

                                                                    }
                                                                }
                                                            }


                                                            if (CtBestList.shouldPushResult(ckey.score)) {
                                                                ckey.encipherDecipherAll(ciphertext, plaintext, len);
                                                                String plains = Utils.getString(plaintext, len);

                                                                long elapsed = System.currentTimeMillis() - startTime;
                                                                String desc = String.format("%s [%,5dK/%,5dK][%,4dK/sec][%,4d Sec]",
                                                                        findSettingsIc ? "IC" : "TRIGRAMS", counter / 1000, totalKeys / 1000, counter / (elapsed + 1), elapsed / 1000);

                                                                ReportResult.reportResult(0, ckey, ckey.score, plains, desc);

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
        CtAPI.printf("\n\nSEARCH ENDED: Total %,d in %.1f Seconds (%,d/Sec)\n\n\n",
                counter,
                (System.currentTimeMillis() - startTime) / 1000.0,
                1000 * counter / (System.currentTimeMillis() - startTime + 1));


        if ((bestKey != null) && (hcMaxPass > 0))
            HillClimb.hillClimbRange(bestKey, bestKey, hcMaxPass, THREADS,
                    findSettingsIc ? 0 : minTrigramsScoreToPrint, MRingScope.ALL, 1, ciphertext, len, HcSaRunnable.Mode.SA, 5);

    }


}
