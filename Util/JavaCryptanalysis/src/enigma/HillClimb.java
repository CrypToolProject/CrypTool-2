package enigma;

import common.CtBestList;
import common.CtAPI;
import common.Runnables;

import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Random;


class HillClimb {

    public static int hillClimbRange(Key from, Key to, int cycles, int THREADS,
                                     int minScoreToPrint, MRingScope lRingSettingScope, int rRingSpacing,
                                     byte[] ciphertext, int len, HcSaRunnable.Mode mode, int rounds) {


        int globalscore = 0;

        long count = 0;


        Key ckey = new Key(from);
        Key low = new Key(from);
        Key high = new Key(to);

        if (low.lRing != high.lRing && mode == HcSaRunnable.Mode.SA) {
            //CtAPI.goodbyeFatalError("Range of left ring settings not allowed for simulated annealing. %s - %s", from.getRotorSettingsString(), to.getRotorSettingsString());

        }
        if (low.mRing != high.mRing && mode == HcSaRunnable.Mode.SA) {
            //CtAPI.goodbyeFatalError("Range of middle settings not allowed for simulated annealing. %s - %s", from.getRotorSettingsString(), to.getRotorSettingsString());

        }

        if (low.mRing == high.mRing)
            lRingSettingScope = MRingScope.ALL;
        long totalKeysPerCycle = Key.numberOfPossibleKeys(low, high, len, lRingSettingScope, rRingSpacing, false);

        int maxRate = 2800;
        int minRate = 2000;
        String modeString = "HILLCLIMBING-" + rounds;
        if (mode == HcSaRunnable.Mode.SA) {
            maxRate = 140;
            minRate = 70;
            modeString = "ANNEALING-" + rounds;
        } else if (mode == HcSaRunnable.Mode.EStecker) {
            maxRate = 50;
            minRate = 35;
            modeString = "OSTWALD-" + rounds;
        }

        long normalizedNkeys = totalKeysPerCycle * rounds;

        String message = String.format("\n\nStarting %s search: Number of settings: %,d x %,d cycles = %,d total settings to check.    \n\nEstimated search time: %s per cycle.\n\n",
                modeString, totalKeysPerCycle, cycles, cycles * totalKeysPerCycle, Utils.getEstimatedTimeString(normalizedNkeys, minRate, maxRate));
        if (totalKeysPerCycle > 1) {
            CtAPI.displayBestPlaintext(message);
        }
        CtAPI.print(message);


        final int MAXKEYS = 26 * 26;



        HcSaRunnable processes[] = new HcSaRunnable[MAXKEYS];
        for (int i = 0; i < MAXKEYS; i++)
            processes[i] = new HcSaRunnable();


        int rejected = 0;
        long startTime = System.currentTimeMillis();


        for (int cycle = 0; cycle < cycles; cycle++) {

            long keyCountInCycle = 0;
            if (((cycle >= 100) && (cycle % 100 == 0) && (globalscore > minScoreToPrint)) || (normalizedNkeys > 1000000))
                CtAPI.printf("%s Cycle %,d best %,d (elapsed %,d seconds)\n", modeString,
                        cycle + 1, globalscore, (System.currentTimeMillis() - startTime) / 1000);


            for (ckey.ukwNum = low.ukwNum; ckey.ukwNum <= high.ukwNum; ckey.ukwNum++) {
                for (ckey.gSlot = low.gSlot; ckey.gSlot <= high.gSlot; ckey.gSlot++) {
                    for (ckey.lSlot = low.lSlot; ckey.lSlot <= high.lSlot; ckey.lSlot++) {
                        for (ckey.mSlot = low.mSlot; ckey.mSlot <= high.mSlot; ckey.mSlot++) {
                            if (ckey.mSlot == ckey.lSlot)
                                continue;
                            for (ckey.rSlot = low.rSlot; ckey.rSlot <= high.rSlot; ckey.rSlot++) {
                                if (ckey.rSlot == ckey.lSlot || ckey.rSlot == ckey.mSlot)
                                    continue;
                                for (ckey.gRing = low.gRing; ckey.gRing <= high.gRing; ckey.gRing++) {
                                    for (ckey.lRing = low.lRing; ckey.lRing <= high.lRing; ckey.lRing++) {
                                        if (cycle < 2) {
                                            String g_mesgS = "";
                                            if (ckey.model == Key.Model.M4)
                                                g_mesgS = "" + Utils.getChar(ckey.gMesg);
                                            CtAPI.printf("%s cycle %,d: %s:%s:%s%s%s:%s (elapsed %,d seconds) - Best %,d\n",
                                                    modeString,
                                                    cycle + 1,
                                                    Utils.getChar(ckey.ukwNum),
                                                    "" + ckey.lSlot + ckey.mSlot + ckey.rSlot,
                                                    Utils.getChar(ckey.lRing), Utils.getChar(ckey.mRing), Utils.getChar(ckey.rRing),
                                                    g_mesgS,
                                                    (System.currentTimeMillis() - startTime) / 1000,
                                                    globalscore);
                                        }
                                        for (ckey.mRing = low.mRing; ckey.mRing <= high.mRing; ckey.mRing++) {
                                            if (cycle < 2) {
                                                System.out.print(".");
                                            }
                                            //      if (ckey.m_slot > 5 && ckey.m_ring > 12) continue;
                                            for (ckey.rRing = low.rRing; ckey.rRing <= high.rRing; ckey.rRing++) {
                                                //       if (ckey.r_slot > 5 && ckey.r_ring > 12) continue;
                                                for (ckey.gMesg = low.gMesg; ckey.gMesg <= high.gMesg; ckey.gMesg++) {
                                                    for (ckey.lMesg = low.lMesg; ckey.lMesg <= high.lMesg; ckey.lMesg++) {

                                                        Runnables runnables = new Runnables();
                                                        int numberOfKeys = 0;
                                                        for (ckey.mMesg = low.mMesg; ckey.mMesg <= high.mMesg; ckey.mMesg++) {
                                                            for (ckey.rMesg = low.rMesg; ckey.rMesg <= high.rMesg; ckey.rMesg++) {
                                                                if ((ckey.rRing % rRingSpacing) != (cycle % rRingSpacing)) {
                                                                    rejected++;
                                                                    continue;
                                                                }
                                                                if (lRingSettingScope != MRingScope.ALL) {
                                                                    int mRingSteppingPos = ckey.getLeftRotorSteppingPosition(len);
                                                                    if (!Key.CheckValidWheelsState(len, mRingSteppingPos, lRingSettingScope)) {
                                                                        rejected++;
                                                                        continue;
                                                                    }
                                                                }

                                                                processes[numberOfKeys].setup(ckey, from.stbrett, ciphertext, len, (cycle == 0), mode, rounds, rRingSpacing);
                                                                runnables.addRunnable(processes[numberOfKeys]);

                                                                numberOfKeys++;
                                                                keyCountInCycle ++;
                                                            }
                                                        }


                                                        if (numberOfKeys == 0) {
                                                            continue;
                                                        }

                                                        runnables.run(THREADS);

                                                        ReportResult.displayProgress(keyCountInCycle, totalKeysPerCycle);
                                                        for (int k = 0; k < numberOfKeys; k++) {

                                                            count++;

                                                            ckey.clone(processes[k].key);

                                                            if (ckey.score > globalscore) {
                                                                globalscore = ckey.score;
                                                            }
                                                            if (globalscore > minScoreToPrint) {

                                                                if (CtBestList.shouldPushResult(ckey.score)) {

                                                                    ckey.addRightRotorOffset(processes[k].bestOffset);

                                                                    ckey.initPathLookupAll(len);

                                                                    String plainStr = ckey.plaintextString(ciphertext, len);

                                                                    Date dNow = new Date();
                                                                    SimpleDateFormat ft = new SimpleDateFormat("kk:mm:ss");
                                                                    String timeStr = ft.format(dNow);

                                                                    long elapsed = System.currentTimeMillis() - startTime;
                                                                    String desc = String.format("%s [%,6dK][%2d: %,5dK/%,5dK][%,4d/sec][%,4d/sec][%,4d Sec][%s][Offset: %,2d]", modeString,
                                                                            count/1000, cycle + 1, keyCountInCycle/1000, totalKeysPerCycle/1000, count  * 1000/ elapsed, (count + rejected)  * 1000/ elapsed, elapsed / 1000, timeStr, processes[k].bestOffset);

                                                                    //ckey.printKeyString("Hillclimbing " + desc);
                                                                    ReportResult.reportResult(0, ckey, ckey.score, plainStr, desc);
                                                                    ckey.substractRightRotorOffset(processes[k].bestOffset);

                                                                }
                                                                //String logs = "" + ft.format(dNow) + ckey.getKeyStringLong() + " " + ckey.plaintextString(ciphertext, len);
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

        if (globalscore > minScoreToPrint) {
            long elapsed = System.currentTimeMillis() - startTime;

            if (elapsed > 1000)
                CtAPI.printf("\n%s - FINISHED AFTER %,d cycles - Best: %,d Rate: %,d/sec Total Checked: %,d in %.1f Seconds (%,d filtered)\n", modeString,
                        cycles, globalscore, 1000 * count / elapsed, count, elapsed / 1000.0, rejected);
            else
                CtAPI.printf("\n%s - FINISHED AFTER %,d cycles - Best: %,d Total Checked: %,d (%,d filtered)\n", modeString,
                        cycles, globalscore, count, rejected);

        }

        return globalscore;


    }

    public static int hillClimbBatch(Key[] keys, int nKeys, int hcMaxPass, int THREADS, int minScoreToPrint, byte[] ciphertext, int len, int rRingSpacing) {


        int count = 0;

        int bestscore = 0;

        final int BATCHSIZE = 26 * 26 * 26;

        HcSaRunnable processes[] = new HcSaRunnable[BATCHSIZE];
        for (int i = 0; i < BATCHSIZE; i++)
            processes[i] = new HcSaRunnable();

        long startTime = System.currentTimeMillis();

        for (int pass = 0; pass < hcMaxPass; pass++) {

            int countInPass = 0;
            if (nKeys * hcMaxPass > 10000)
                System.out.printf("HILL CLIMBING BATCH OF %,d Keys - Pass %,d of %,d\n", nKeys, pass + 1, hcMaxPass);

            for (int k = 0; k < nKeys; k += BATCHSIZE) {

                if ((hcMaxPass == 1) && (nKeys < 20)) {
                    System.out.printf("HILL CLIMBING BATCH - Pass %,d of %,d\n", (pass + 1), hcMaxPass);
                    keys[k].printKeyString("");
                    System.out.print("\n");
                }
                int actualBatchSize;

                actualBatchSize = Math.min(BATCHSIZE, nKeys - k);
                Runnables r = new Runnables();

                for (int b = 0; b < actualBatchSize; b++) {
                    processes[b].setup(keys[k + b], keys[k + b].stbrett, ciphertext, len, (pass == 0), HcSaRunnable.Mode.SA, 1, rRingSpacing);
                    r.addRunnable(processes[b]);
                }

               r.run(THREADS);

                for (int b = 0; b < actualBatchSize; b++) {

                    count++;
                    countInPass++;

                    //Key ckey = new Key(processes[b].theKey);
                    Key ckey = processes[b].key;


                    if (ckey.score > minScoreToPrint) {

                        if (CtBestList.shouldPushResult(ckey.score)) {
                            ckey.initPathLookupAll(len);

                            String plainStr = ckey.plaintextString(ciphertext, len);

                            Date dNow = new Date();
                            SimpleDateFormat ft = new SimpleDateFormat("kk:mm:ss");
                            String timeStr = ft.format(dNow);

                            long elapsed = System.currentTimeMillis() - startTime;
                            String desc = String.format("HILLCLIMBING TOP [%,6d][%2d: %,5d/%,5d][%,4d Sec][%,4d/sec]][%s]",
                                    count / 1000, pass + 1, countInPass, nKeys, elapsed / 1000, count * 1000 / elapsed, timeStr);

                            ReportResult.reportResult(0, ckey, ckey.score, plainStr, desc);
                            //ckey.printKeyString("Hillclimbing " + desc);
                        }
                        if (ckey.score > bestscore) {
                            bestscore = ckey.score;
                            byte[] steppings = new byte[Key.MAXLEN];
                            ckey.showSteppings(steppings, len);
                            String steppingsS = Utils.getCiphertextStringNoXJ(steppings, len);

                            System.out.printf("\n%s\n%s\n\n", ckey.plaintextString(ciphertext, len), steppingsS);
                        }
                    }
                }
            }
        }

        return bestscore;

    }


}
