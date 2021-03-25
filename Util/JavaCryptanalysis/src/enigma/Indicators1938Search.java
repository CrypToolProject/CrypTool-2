package enigma;

import common.CommandLine;
import common.CtAPI;
import common.Flag;

public class Indicators1938Search {

    static void indicators1938Search(String INDICATORS_FILE, Key lowKey, Key highKey, String steckerS, byte[] ciphertext, int clen) {
        Key key;
        int A = Utils.getIndex('A');
        int Z = Utils.getIndex('Z');
        if ((lowKey.lMesg != A) || (lowKey.mMesg != A) || (lowKey.rMesg != A) ||
                (highKey.lMesg != Z) || (highKey.mMesg != Z) || (highKey.rMesg != Z)) {
            System.out.print("WARNING: Z. Sheets Search (-D): Ignoring Message Key settings. \n\n");
        }

        if (steckerS.length() != 0) {
            System.out.print("WARNING: Z. Sheets Search (-D): Ignoring Stecker Settings. \n\n");
            lowKey.setStecker("");
            highKey.setStecker("");
        }

        byte indicData[] = new byte[Key.MAXLEN];
        int flen = -1;
        if (INDICATORS_FILE.length() != 0)
            flen = Utils.loadCipherText(INDICATORS_FILE, indicData, false);
        if ((flen < 9) || (flen % 9 != 0)) {
            CtAPI.goodbyeFatalError("Z. Sheets Search (-%s INDICATORS1938): Failed to load indicators data from file %s (%d characters found).\n",
                    Flag.MODE, INDICATORS_FILE, flen);
        }
        CtAPI.printf("Zygalski Sheets Search: Read database - File %s Indicators %d \nFirst Indicator: %s\n",
                INDICATORS_FILE, flen / 9, Utils.getString(indicData, 9));

        key = searchZSheets(indicData, flen, lowKey, highKey, ciphertext, clen);

        if (key == null) {
            CtAPI.print("\nZ. Sheets Search: No match found. \n");
            CtAPI.goodbyeFatalError("Zygalski Sheets search failed");
        }
    }

    private static Key searchZSheets(byte[] indicData, int flen, Key from, Key to, byte[] ciphertext, int clen) {


        int nIndics = flen / 9;  // initial value for buffer allocation

        //nIndics = 10;

        boolean[][] females = new boolean[nIndics][3];
        byte[][] indicCiphertextWithFemales = new byte[nIndics][6];
        byte[][] indicMsgKeysWithFemales = new byte[nIndics][3];
        int indicsWithFemales = 0;

        byte[][] indicCiphertext = new byte[nIndics][6];
        byte[][] indicMsgKeys = new byte[nIndics][3];

        for (int i = 0; i < nIndics; i++) {
            boolean hasFemales[] = new boolean[3];
            for (int pos = 0; pos < 3; pos++) {
                int letterAtPos = indicData[i * 9 + 3 + pos];
                int letterAtPosPlus3 = indicData[i * 9 + 3 + pos + 3];
                if (letterAtPos == letterAtPosPlus3)
                    hasFemales[pos] = true;


            }
            if (hasFemales[0] || hasFemales[1] || hasFemales[2]) {
                System.arraycopy(indicData, i * 9, indicMsgKeysWithFemales[indicsWithFemales], 0, 3);
                System.arraycopy(indicData, i * 9 + 3, indicCiphertextWithFemales[indicsWithFemales], 0, 6);
                System.arraycopy(hasFemales, 0, females[indicsWithFemales], 0, 3);

                indicsWithFemales++;

            }
            System.arraycopy(indicData, i * 9, indicMsgKeys[i], 0, 3);
            System.arraycopy(indicData, i * 9 + 3, indicCiphertext[i], 0, 6);
        }

        int nRingsToTest = (to.lRing - from.lRing + 1) * (to.mRing - from.mRing + 1) * (to.rRing - from.rRing + 1);
        if ((indicsWithFemales < 5) || ((indicsWithFemales < 10) && (nRingsToTest > 10))) {
            CtAPI.printf("\n\nINDICATORS1938 Zygalski Sheets Search: Only %d indicators with 'females' (out of %d) were found - not enough (best 10 or more) or will take too much time.... \n\n", indicsWithFemales, nIndics);
        }
        CtAPI.printf("\n\nINDICATORS1938 Zygalski Sheets Search: %d indicators with 'females' (out of %d) - Starting search.... \n\n", indicsWithFemales, nIndics);


        Key ckey = new Key(from);
        Key lo = new Key(from);
        Key high = new Key(to);
        lo.lMesg = high.lMesg = lo.mMesg = high.mMesg = lo.rMesg = high.rMesg = 0;

        long counterKeys = 0;
        long totalKeys = Key.numberOfPossibleKeys(lo, high, 6, MRingScope.ALL, 1, false);

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
//                                    if (indicsWithFemales > 10) {
//                                        ckey.mRing = ckey.lRing = 0;
//                                        ckey.score = 0;
//                                        ckey.setStecker("");
//                                        ckey.printKeyString("Candidate");
//                                    }
                                    for (ckey.mRing = lo.mRing; ckey.mRing <= high.mRing; ckey.mRing++) {
                                        for (ckey.rRing = lo.rRing; ckey.rRing <= high.rRing; ckey.rRing++) {
                                            counterKeys++;
                                            ReportResult.displayProgress(counterKeys, totalKeys);
                                            boolean valid = true;
                                            for (int indic = 0; indic < indicsWithFemales; indic++) {

                                                ckey.lMesg = indicMsgKeysWithFemales[indic][0];
                                                ckey.mMesg = indicMsgKeysWithFemales[indic][1];
                                                ckey.rMesg = indicMsgKeysWithFemales[indic][2];

                                                if (!ckey.checkFemales(females[indic], false)) {
                                                    valid = false;
                                                    break;
                                                }

                                            }
                                            if (valid) {



                                                // clean up stecker info from previous runs.
                                                ckey.setStecker("");
                                                int runs;
                                                for (runs = 0; runs < 5; runs++) {
                                                    if (runs > 0) {
                                                        ckey.setRandomStb(6);
                                                    }
                                                    int prevScore = ckey.score = 0;
                                                    while (true) {
                                                        IndicatorsSearch.hillClimbIndicator(ckey, indicMsgKeys, indicCiphertext, nIndics, false);
                                                        if ((ckey.score <= prevScore) || (ckey.score == 1000)) {
                                                            break;
                                                        }
                                                        prevScore = ckey.score;
                                                    }
                                                    if (ckey.score == 1000) {
                                                        break;
                                                    }
                                                }

                                                if (ckey.score > 350) {
                                                    String all = indics1938String(indicCiphertext, indicMsgKeys, nIndics, ckey);

                                                    if (ckey.score > 700) {
                                                        CtAPI.printf("\nINDICATORS1938 Zygalski Sheets Search: Found Wheels Order (%d%d%d) and Ring Settings (%s) which matches the female cases (%d keys tested)\n\n",
                                                                ckey.lSlot, ckey.mSlot, ckey.rSlot,
                                                                "" + Utils.getChar(ckey.lRing) + Utils.getChar(ckey.mRing) + Utils.getChar(ckey.rRing),
                                                                counterKeys);
                                                        CtAPI.printf("\nINDICATORS1938 Zygalski Sheets Search: Found Stecker Board settings matching (score %d) the double-encrypted indicators: %s\n\n", ckey.score, ckey.stbString());

                                                        CtAPI.printf("\nINDICATORS1938 Zygalski Sheets Search: Showing only deciphered indicators with females (%d):\n\n", indicsWithFemales);
                                                        String withFemales = indics1938String(indicCiphertextWithFemales, indicMsgKeysWithFemales, indicsWithFemales, ckey);
                                                        CtAPI.printf(withFemales);
                                                        CtAPI.printf("\nINDICATORS1938 Zygalski Sheets Search: Showing all deciphered indicators (%d):\n\n", nIndics);
                                                        CtAPI.printf(all);
                                                    }
                                                    ckey.lMesg = ckey.mMesg = ckey.rMesg = 0;
                                                    long elapsed = System.currentTimeMillis() - startTime;
                                                    String desc = String.format("INDICATORS1938 DAILY KEY [%,5dK/%,5dK][%,4dK/sec][%,4d Sec]",
                                                            counterKeys / 1000, totalKeys / 1000, counterKeys / elapsed, elapsed / 1000);
                                                    ReportResult.reportResult(0, ckey, ckey.score, all.replaceAll("\n", ""), desc);

                                                    // Use the first indicator (plain) as message key to decipher the actual message key.
                                                    if (ckey.score >= 900 && clen > 0) {
                                                        byte[] indicPlaintext = new byte[6];
                                                        Key indicKey = new Key(ckey);
                                                        indicKey.lMesg = indicData[0];
                                                        indicKey.mMesg = indicData[1];
                                                        indicKey.rMesg = indicData[2];
                                                        indicKey.score = 0;
                                                        CtAPI.printf("\nINDICATORS1938 Zygalski Sheets Search: Using the non-encrypted indicator (%s) of the first indicator set (%s) in the the original file to decipher the (double encrypted) message key.\n\n",
                                                                "" + Utils.getChar(indicKey.lMesg) + Utils.getChar(indicKey.mMesg) + Utils.getChar(indicKey.rMesg),
                                                                Utils.getString(indicData, 9));
                                                        indicKey.printKeyString("");
                                                        byte doubleKeyText[] = new byte[6];
                                                        System.arraycopy(indicData, 3, doubleKeyText, 0, 6);
                                                        indicKey.encipherDecipherAll(doubleKeyText, indicPlaintext, 6);
                                                        CtAPI.printf("\nINDICATORS1938 Zygalski Sheets Search: Message Key deciphered  - Making sure it is doubled .... (%s) \n",
                                                                Utils.getString(indicPlaintext, 6));

                                                        if ((indicPlaintext[0] != indicPlaintext[3]) ||
                                                                (indicPlaintext[1] != indicPlaintext[4]) ||
                                                                (indicPlaintext[2] != indicPlaintext[5])) {
                                                            CtAPI.print("\n\nINDICATORS1938 Zygalski Sheets Search: Problem - the first indicator is not doubled \n");
                                                            continue;
                                                        }

                                                        ckey.lMesg = indicPlaintext[0];
                                                        ckey.mMesg = indicPlaintext[1];
                                                        ckey.rMesg = indicPlaintext[2];
                                                        CtAPI.printf("\nINDICATORS1938 Zygalski Sheets Search: Obtained the message key for the message (%s after decryption) - Updating the key.\n\n",
                                                                "" + Utils.getChar(ckey.lMesg) + Utils.getChar(ckey.mMesg) + Utils.getChar(ckey.rMesg));

                                                        ckey.printKeyString("INDICATORS1938 Zygalski Sheets Search: Key to decipher the message");

                                                        byte[] plaintext = new byte[Key.MAXLEN];

                                                        ckey.encipherDecipherAll(ciphertext, plaintext, clen);
                                                        String plainS = Utils.getCiphertextStringNoXJ(plaintext, clen);
                                                        CtAPI.printf("\nWithout X/J\n\n%s\n", plainS);

                                                        plainS = Utils.getString(plaintext, clen);

                                                        CtAPI.printf("Z. Sheets Search Successful - Plaintext is: \n\n%s\n", plainS);
                                                        byte[] steppings = new byte[Key.MAXLEN];
                                                        ckey.showSteppings(steppings, clen);
                                                        String steppingsS = Utils.getCiphertextStringNoXJ(steppings, clen);
                                                        System.out.println(steppingsS);
                                                        ckey.score = ckey.triScoreWithoutLookupBuild(ciphertext, clen);
                                                        ReportResult.reportResult(0, ckey, ckey.score,
                                                                plainS + "     Indicators:" + all.replaceAll("\n", ""),
                                                                desc.replaceAll("INDICATORS1938 DAILY KEY", "TRIGRAMS"));

                                                       return ckey;
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


        long elapsed = (System.currentTimeMillis() - startTime + 1);


        CtAPI.printf("INDICATORS1938 Zygalski Sheets Search: No matching keys found \n %d Total Keys Tested in %.1f Seconds(%d/sec)\n\n",
                counterKeys, elapsed / 1000.0, 1000 * counterKeys / elapsed);

        return null;


    }

    private static String indics1938String(byte[][] indicCiphertext, byte[][] indicMsgKeys, int indicsWith, Key ckey) {
        StringBuilder s = new StringBuilder();
        Key indicKey = new Key(ckey);
        for (int i = 0; i < indicsWith; i++) {
            byte[] indicPlaintext = new byte[6];

            indicKey.lMesg = indicMsgKeys[i][0];
            indicKey.mMesg = indicMsgKeys[i][1];
            indicKey.rMesg = indicMsgKeys[i][2];
            indicKey.encipherDecipherAll(indicCiphertext[i], indicPlaintext, 6);

            s.append(String.format("%s:%s ==> %s, ",
                    Utils.getString(indicMsgKeys[i], 3),
                    Utils.getString(indicCiphertext[i], 6),
                    Utils.getString(indicPlaintext, 6)));
            if ((i % 6) == 5)
                s.append("\n");
        }
        s.append("\n");
        return s.toString();
    }

}
