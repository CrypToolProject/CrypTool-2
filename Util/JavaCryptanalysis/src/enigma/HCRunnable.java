package enigma;

class HCRunnable implements Runnable {
    public final Key key = new Key();
    private int var[];
    private byte[] ciphertext;
    private int len;
    private boolean firstPass;
    private int[] stb;

    // public high level algos
    public static void hillClimbSingleKeySinglePass(Key ckey, int[] stb, int[] var, byte[] ciphertext, int len, boolean firstPass) {
        long triscore;
        String realStb = "";
        boolean detailHcPrint = false;

        if (ckey.model != Key.Model.M4) {
            ckey.initPathLookupHandM3(len);
        } else {
            ckey.initPathLookupAll(len);
        }

        //realStb = "ACBXDOEFGKHTJMLRQSUY"; // w6
        //realStb = "ACBXDOEFGKHTJMLRQSUY"; // w2
        //realStb = "AECFGLHIKPMSNROUQYTW"; // w1
        //realStb = "BXCHDJFQGNIWKYLSMUOZ"; // w3
        //realStb = "BCDEFGHIJKLXMQNOSTVZ"; // w_nkmow
        //realStb = "AYBXCWDNETGRHQJOKULZ"; // w4

        //realStb = "BQCRDIEJGHKWMTOSPXUZ"; //
        //realStb = "ADBHFGIJKNLZMROSPWQV";
        //realStb = "BICWEQFXHZJNKYMTOVPR"; //w_xtmsy
        //realStb = "ADBHFGIJKNLZMROSPWQV";
        //realStb = "CTEMFIGJHKNQORSWUYVX"; //lyaso
        //detailHcPrint = true;


        ckey.setStecker(stb); // restore because the ones on ckey were changed in previous keys/passes
        // if stb not empty and first pass, and skip IC phase, maybe we have a winner with Tri
        if (firstPass && (ckey.stbCount != 0)) {

            triscore = hillClimbPhase2(ckey, var, ciphertext, len, realStb, detailHcPrint);

        } else {

            long steepestTriscore = 0;
            int[] keepStb = new int[26];
            if (firstPass) {
                ckey.setStecker("");
                hillClimbPhase1Steepest(ckey, ciphertext, len, realStb, detailHcPrint);
                steepestTriscore = hillClimbPhase2(ckey, var, ciphertext, len, realStb, detailHcPrint);
                System.arraycopy(ckey.stbrett, 0, keepStb, 0, 26);
            }

            ckey.setStecker("");
            hillClimbPhase1Slow(ckey, var, ciphertext, len, realStb, detailHcPrint);
            long slowTriscore = hillClimbPhase2(ckey, var, ciphertext, len, realStb, detailHcPrint);

            if (steepestTriscore != 0) {
                if (steepestTriscore > slowTriscore) {
                    ckey.setStecker(keepStb);
                    triscore = steepestTriscore;
                } else if (slowTriscore > steepestTriscore) {
                    triscore = slowTriscore;
                } else {
                    triscore = slowTriscore;
                }
            } else {
                triscore = slowTriscore;
            }


        }

        //triscore = hillClimbPhase3(ckey,  ciphertext, len, detailHcPrint) ;

        ckey.score = (int) triscore;
    }

    private static long hillClimbStep(String title, Key.EVAL eval, Key ckey, int[] var, byte[] ciphertext, int len, String stbCompare, boolean print) {


        if (print)
            System.out.printf("\n%s ==================\n", title);

        ckey.resetCounter();

        SearchAction action;

        long newScore;
        long bestScore = ckey.eval(eval, ciphertext, len);

        byte[] invVar = new byte[26];
        for (byte i = 0; i < 26; i++) {
            invVar[var[i]] = i;
        }

        for (int i = 0; i < 26; i++) {
            int vi = var[i]; // invariant
            for (int k = i + 1; k < 26; k++) {
                int vk = var[k];
                int vsk = ckey.stbrett[vk];
                if (vsk == vi) {
                    continue;
                }
                int sk = invVar[vsk];
                int vsi = ckey.stbrett[vi]; // not an invariant
                int si = invVar[vsi];

                action = SearchAction.NO_CHANGE;
                long prevScore = bestScore;

                if (vi == vsi && vk == vsk) {

                    if (ckey.stbCount() == Key.MAX_STB_PLUGS) {
                        continue;
                    }

                    //ckey.assertStbSelfAndCount( vi, vsi, vk, vsk);

                    //ckey.swap(vi, vk);
                    ckey.stbConnect(vi, vk);

                    newScore = ckey.eval(eval, ciphertext, len);
                    if (newScore > bestScore) {
                        bestScore = newScore;
                        action = SearchAction.IandK;
                    }
                    if (action == SearchAction.NO_CHANGE) {
                        //ckey.swap(vi, vk);
                        ckey.stbDisconnect(vi, vk);
                        //ckey.assertStbSelfAndCount( vi, vsi, vk, vsk);
                    }

                    //ckey.assertStbCount();


                } else if ((vi == vsi) && (vk != vsk)) {

                    if ((sk > i) && (sk < k)) {
                        continue;
                    }

                    //ckey.swap(vk, vsk);
                    ckey.stbDisconnect(vk, vsk);

                    //all self
                    //ckey.assertStbSelfAndCount( vi, vsi, vk, vsk);

                    //ckey.swap(vi, vk);
                    ckey.stbConnect(vi, vk);

                    newScore = ckey.eval(eval, ciphertext, len);
                    if (newScore > bestScore) {
                        bestScore = newScore;
                        action = SearchAction.IandK;
                    }
                    //ckey.swap(vi, vk);
                    ckey.stbDisconnect(vi, vk);
                    // all self
                    //ckey.assertStbSelfAndCount( vi, vsi, vk, vsk);

                    //ckey.swap(vi, vsk);
                    ckey.stbConnect(vi, vsk);

                    newScore = ckey.eval(eval, ciphertext, len);
                    if (newScore > bestScore) {
                        bestScore = newScore;
                        action = SearchAction.IandSK;
                    }
                    //ckey.swap(vi, vsk);
                    ckey.stbDisconnect(vi, vsk);
                    // all self now
                    //ckey.assertStbSelfAndCount( vi, vsi, vk, vsk);

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
                    //ckey.assertStbCount();


                } else if (vk == vsk && vi != vsi) {

                    if ((si < k) && (si < i)) {
                        continue;
                    }
                    //ckey.swap(vi, vsi);
                    ckey.stbDisconnect(vi, vsi);
                    // all self
                    //ckey.assertStbSelfAndCount( vi, vsi, vk, vsk);

                    //ckey.swap(vk, vi);
                    ckey.stbConnect(vk, vi);

                    newScore = ckey.eval(eval, ciphertext, len);
                    if (newScore > bestScore) {
                        bestScore = newScore;
                        action = SearchAction.IandK;
                    }
                    //ckey.swap(vk, vi);
                    ckey.stbDisconnect(vk, vi);

                    // all self
                    //ckey.assertStbSelfAndCount( vi, vsi, vk, vsk);

                    //ckey.swap(vk, vsi);
                    ckey.stbConnect(vk, vsi);

                    newScore = ckey.eval(eval, ciphertext, len);
                    if (newScore > bestScore) {
                        bestScore = newScore;
                        action = SearchAction.KandSI;
                    }
                    //ckey.swap(vk, vsi);
                    ckey.stbDisconnect(vk, vsi);
                    // all self
                    //ckey.assertStbSelfAndCount( vi, vsi, vk, vsk);

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
                    //ckey.assertStbCount();

                } else if ((vi != vsi) && (vk != vsk)) {

                    if ((si < i) || (sk < k)) {
                        continue;
                    }

                    //ckey.swap(vi, vsi);
                    ckey.stbDisconnect(vi, vsi);

                    //ckey.swap(vk, vsk);
                    ckey.stbDisconnect(vk, vsk);

                    // all Self now
                    //ckey.assertStbSelfAndCount( vi, vsi, vk, vsk);


                    //ckey.swap(vi, vsi);
                    ckey.stbConnect(vi, vsi);

                    //ckey.swap(vi, vsi);
                    ckey.stbDisconnect(vi, vsi);

                    // all Self now
                    //ckey.assertStbSelfAndCount( vi, vsi, vk, vsk);

                    //ckey.swap(vi, vk);
                    ckey.stbConnect(vi, vk);

                    newScore = ckey.eval(eval, ciphertext, len);
                    if (newScore > bestScore) {
                        bestScore = newScore;
                        action = SearchAction.IandK;
                    }
                    //ckey.swap(vsi, vsk);
                    ckey.stbConnect(vsi, vsk);

                    newScore = ckey.eval(eval, ciphertext, len);
                    if (newScore > bestScore) {
                        bestScore = newScore;
                        action = SearchAction.IandK_SIandSK;
                    }
                    //ckey.swap(vi, vk);
                    ckey.stbDisconnect(vi, vk);

                    //ckey.swap(vsi, vsk);
                    ckey.stbDisconnect(vsi, vsk);

                    // all Self now
                    //ckey.assertStbSelfAndCount( vi, vsi, vk, vsk);


                    //ckey.swap(vi, vsk);
                    ckey.stbConnect(vi, vsk);


                    newScore = ckey.eval(eval, ciphertext, len);
                    if (newScore > bestScore) {
                        bestScore = newScore;
                        action = SearchAction.IandSK;
                    }
                    //ckey.swap(vsi, vk);
                    ckey.stbConnect(vsi, vk);


                    newScore = ckey.eval(eval, ciphertext, len);
                    if (newScore > bestScore) {
                        bestScore = newScore;
                        action = SearchAction.IandSK_KandSI;
                    }
                    //ckey.swap(vi, vsk);
                    ckey.stbDisconnect(vi, vsk);

                    //ckey.swap(vsi, vk);
                    ckey.stbDisconnect(vsi, vk);

                    // all Self now
                    //ckey.assertStbSelfAndCount( vi, vsi, vk, vsk);


                    switch (action) {

                        case IandK:
                            //ckey.swap(vi, vk);
                            ckey.stbConnect(vi, vk);
                            break;
                        case IandSK:
                            //ckey.swap(vi, vsk);
                            ckey.stbConnect(vi, vsk);
                            break;
                        case IandK_SIandSK:
                            //ckey.swap(vi, vk);
                            //ckey.swap(vsi, vsk);
                            ckey.stbConnect(vi, vk);
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
                    //ckey.assertStbCount();

                }
                if (print && (action != SearchAction.NO_CHANGE)) {

                    System.out.printf("%s\tI [%s] SI [%s] K [%s] SK [%s] \t%d ==> %d\tIC: %.4f \tBI: %d\tTRI: %d\t>%s<(%d)\t%s\n",
                            title,
                            Utils.getChar(vi), Utils.getChar(vsi), Utils.getChar(vk), Utils.getChar(vsk),
                            prevScore,
                            bestScore,
                            ckey.icscore(ciphertext, len),
                            ckey.biscore(ciphertext, len),
                            ckey.triscore(ciphertext, len),
                            ckey.stbString(),
                            ckey.compareStecker(stbCompare),
                            action
                    );

                }

            }
        }

        if (print) {
            System.out.printf("== End of %s ================== (Count: %d)\n\n", title, ckey.getCounter());
        }

        if (ckey.eval(eval, ciphertext, len) != bestScore) {
            throw new RuntimeException("Best result is not consistent");
        }
        return bestScore;

    }

    private static long hillClimbStepSteepestPartial(String title, Key.EVAL eval, Key ckey, byte[] ciphertext, int len, String stbCompare, boolean print) {


        long bestScore, score;


        if (print)
            System.out.printf("\n%s - STEEPEST =====================\n", title);

        ckey.resetCounter();

        bestScore = ckey.eval(eval, ciphertext, len);

        boolean improved = true;

        while (improved && (ckey.stbCount() < Key.MAX_STB_PLUGS)) {

            int bvi = -1, bvk = -1;
            SearchAction baction = SearchAction.NO_CHANGE;
            long prevScore = bestScore;
            for (int vi = 0; vi < 26; vi++) {
                if (vi != ckey.stbrett[vi]) {
                    continue;
                }
                for (int vk = vi + 1; vk < 26; vk++) {
                    if (vk != ckey.stbrett[vk]) {
                        continue;
                    }

                    ckey.stbConnect(vi, vk);
                    score = ckey.eval(eval, ciphertext, len);
                    if (score - bestScore > 0) {

                        bestScore = score;
                        baction = SearchAction.IandK;

                        bvi = vi;
                        bvk = vk;

                    }
                    ckey.stbDisconnect(vi, vk);

                }
            }
            switch (baction) {
                case IandK:
                    ckey.stbConnect(bvi, bvk);
                    break;
                case NO_CHANGE:
                    break;
                default:
                    break;
            }
            if (print && (baction != SearchAction.NO_CHANGE)) {

                System.out.printf("%s\tI [%s] K [%s] \t%d ==> %d\tIC: %.4f \tBI: %d\tTRI: %d\t>%s<(%d)\t%s\n",
                        title,
                        Utils.getChar(bvi), Utils.getChar(bvk),
                        prevScore,
                        bestScore,
                        ckey.icscore(ciphertext, len),
                        ckey.biscore(ciphertext, len),
                        ckey.triscore(ciphertext, len),
                        ckey.stbString(),
                        ckey.compareStecker(stbCompare),
                        baction
                );

            }
            improved = baction != SearchAction.NO_CHANGE;


        }

        if (print) {
            System.out.printf("== End of %s ================== (Count: %d)\n\n", title, ckey.getCounter());
        }

        return bestScore;

    }

    private static long hillClimbStepSteepestFull(String title, Key.EVAL eval, Key ckey, byte[] ciphertext, int len, String stbCompare, boolean print) {


        long bestScore, score;


        if (print)
            System.out.printf("\n%s - STEEPEST =====================\n", title);

        ckey.resetCounter();

        bestScore = ckey.eval(eval, ciphertext, len);

        boolean improved = true;

        while (improved) {

            int bvi = -1, bvk = -1, bsvi = -1, bvsk = -1;
            SearchAction baction = SearchAction.NO_CHANGE;
            long prevScore = bestScore;
            for (int i = 0; i < 26; i++) {
                int vi = i; // invariant
                int vsi = ckey.stbrett[vi]; // invariant
                for (int k = i + 1; k < 26; k++) {
                    int vk = k;
                    int vsk = ckey.stbrett[vk];

                    if ((vi == vsi && vk == vsk) || (vi == vsk && vk == vsi)) {
                        ckey.swap(vi, vk);
                        score = ckey.eval(eval, ciphertext, len);
                        if (score - bestScore > 0) {

                            bestScore = score;
                            baction = SearchAction.IandK;

                            bvi = vi;
                            bvk = vk;
                            bsvi = vsi;
                            bvsk = vsk;
                        }
                        ckey.swap(vi, vk);
                    } else if (vi == vsi && vk != vsk) {
                        ckey.swap(vk, vsk);

                        // all self

                        ckey.swap(vi, vk);
                        score = ckey.eval(eval, ciphertext, len);
                        if (score - bestScore > 0) {
                            bestScore = score;
                            baction = SearchAction.IandK;
                            bvi = vi;
                            bvk = vk;
                            bsvi = vsi;
                            bvsk = vsk;
                        }
                        ckey.swap(vi, vk);

                        // all self

                        ckey.swap(vi, vsk);
                        score = ckey.eval(eval, ciphertext, len);
                        if (score - bestScore > 0) {
                            bestScore = score;
                            baction = SearchAction.IandSK;
                            bvi = vi;
                            bvk = vk;
                            bsvi = vsi;
                            bvsk = vsk;
                        }
                        ckey.swap(vi, vsk);

                        // all self
                        ckey.swap(vk, vsk);
                        // back to square one

                    } else if (vk == vsk && vi != vsi) {
                        ckey.swap(vi, vsi);

                        // all self
                        ckey.swap(vk, vi);
                        score = ckey.eval(eval, ciphertext, len);
                        if (score - bestScore > 0) {
                            bestScore = score;
                            baction = SearchAction.IandK;
                            bvi = vi;
                            bvk = vk;
                            bsvi = vsi;
                            bvsk = vsk;
                        }
                        ckey.swap(vk, vi);
                        // all self
                        ckey.swap(vk, vsi);
                        score = ckey.eval(eval, ciphertext, len);
                        if (score - bestScore > 0) {
                            bestScore = score;
                            baction = SearchAction.KandSI;
                            bvi = vi;
                            bvk = vk;
                            bsvi = vsi;
                            bvsk = vsk;
                        }
                        ckey.swap(vk, vsi);
                        // all self


                        ckey.swap(vi, vsi);
                        //back to square one

                    } else if (vi != vsi && vk != vsk) {
                        ckey.swap(vi, vsi);

                        score = ckey.eval(eval, ciphertext, len);
                        if (score - bestScore > 0) {
                            bestScore = score;
                            baction = SearchAction.IandSI;
                            bvi = vi;
                            bvk = vk;
                            bsvi = vsi;
                            bvsk = vsk;
                        }
                        ckey.swap(vk, vsk);
                        // all Self now

                        ckey.swap(vi, vsi);
                        score = ckey.eval(eval, ciphertext, len);
                        if (score - bestScore > 0) {
                            bestScore = score;
                            baction = SearchAction.KandSK;
                            bvi = vi;
                            bvk = vk;
                            bsvi = vsi;
                            bvsk = vsk;
                        }
                        ckey.swap(vi, vsi);

                        // all Self now

                        ckey.swap(vi, vk);
                        score = ckey.eval(eval, ciphertext, len);
                        if (score - bestScore > 0) {
                            bestScore = score;
                            baction = SearchAction.IandK;
                            bvi = vi;
                            bvk = vk;
                            bsvi = vsi;
                            bvsk = vsk;
                        }
                        ckey.swap(vsi, vsk);
                        score = ckey.eval(eval, ciphertext, len);
                        if (score - bestScore > 0) {
                            bestScore = score;
                            baction = SearchAction.IandK_SIandSK;
                            bvi = vi;
                            bvk = vk;
                            bsvi = vsi;
                            bvsk = vsk;
                        }
                        ckey.swap(vi, vk);

                        score = ckey.eval(eval, ciphertext, len);
                        if (score - bestScore > 0) {
                            bestScore = score;
                            baction = SearchAction.SIandSK;
                            bvi = vi;
                            bvk = vk;
                            bsvi = vsi;
                            bvsk = vsk;
                        }

                        ckey.swap(vsi, vsk);

                        // all Self now


                        ckey.swap(vi, vsk);
                        score = ckey.eval(eval, ciphertext, len);
                        if (score - bestScore > 0) {
                            bestScore = score;
                            baction = SearchAction.IandSK;
                            bvi = vi;
                            bvk = vk;
                            bsvi = vsi;
                            bvsk = vsk;
                        }
                        ckey.swap(vsi, vk);
                        score = ckey.eval(eval, ciphertext, len);
                        if (score - bestScore > 0) {
                            bestScore = score;
                            bvi = vi;
                            bvk = vk;
                            bsvi = vsi;
                            bvsk = vsk;
                            baction = SearchAction.IandSK_KandSI;
                        }
                        ckey.swap(vi, vsk);

                        score = ckey.eval(eval, ciphertext, len);
                        if (score - bestScore > 0) {
                            bestScore = score;
                            baction = SearchAction.KandSI;
                            bvi = vi;
                            bvk = vk;
                            bsvi = vsi;
                            bvsk = vsk;
                        }

                        ckey.swap(vsi, vk);

                        // all Self now

                        ckey.swap(vi, vsi);
                        ckey.swap(vk, vsk);
                        // back to square one
                    }
                }
            }
            switch (baction) {
                case KandSK:
                    ckey.swap(bvk, bvsk);
                    break;
                case IandSI:
                    ckey.swap(bvi, bsvi);
                    break;
                case IandK:
                    if (bvi != bsvi)
                        ckey.swap(bvi, bsvi);
                    if (bvk != bvsk)
                        ckey.swap(bvk, bvsk);

                    ckey.swap(bvi, bvk);
                    break;
                case IandSK:
                    if (bvi != bsvi)
                        ckey.swap(bvi, bsvi);

                    ckey.swap(bvk, bvsk);

                    ckey.swap(bvi, bvsk);
                    break;
                case KandSI:
                    if (bvk != bvsk)
                        ckey.swap(bvk, bvsk);
                    ckey.swap(bvi, bsvi);
                    ckey.swap(bvk, bsvi);
                    break;
                case IandK_SIandSK:
                    ckey.swap(bvi, bsvi);
                    ckey.swap(bvk, bvsk);
                    ckey.swap(bvi, bvk);
                    ckey.swap(bsvi, bvsk);
                    break;
                case SIandSK:
                    ckey.swap(bvi, bsvi);
                    ckey.swap(bvk, bvsk);
                    ckey.swap(bsvi, bvsk);
                    break;
                case IandSK_KandSI:
                    ckey.swap(bvi, bsvi);
                    ckey.swap(bvk, bvsk);

                    ckey.swap(bvi, bvsk);
                    ckey.swap(bvk, bsvi);
                    break;
                case NO_CHANGE:
                    break;
                default:
                    break;
            }
            if (print && (baction != SearchAction.NO_CHANGE)) {

                System.out.printf("%s\tI [%s] SI [%s] K [%s] SK [%s] \t%d ==> %d\tIC: %.4f \tBI: %d\tTRI: %d\t>%s<(%d)\t%s\n",
                        title,
                        Utils.getChar(bvi), Utils.getChar(bsvi), Utils.getChar(bvk), Utils.getChar(bvsk),
                        prevScore,
                        bestScore,
                        ckey.icscore(ciphertext, len),
                        ckey.biscore(ciphertext, len),
                        ckey.triscore(ciphertext, len),
                        ckey.stbString(),
                        ckey.compareStecker(stbCompare),
                        baction
                );

            }
            improved = baction != SearchAction.NO_CHANGE;


        }

        if (print) {
            System.out.printf("== End of %s ================== (Count: %d)\n\n", title, ckey.getCounter());
        }

        return bestScore;

    }

    private static long jointScore(Key ckey, byte[] ciphertext, int len) {

        int bigramWeightOf10 = 5;

        long icfactor = (long) (ckey.icscore(ciphertext, len) * 100000.0);

        long jointScore = ((10 - bigramWeightOf10) * ckey.triscore(ciphertext, len) + bigramWeightOf10 * ckey.biscore(ciphertext, len)) / 10;

        jointScore += icfactor;

        return jointScore;

    }

    // private sub algorithms
    private static long hillClimbPhase2(Key ckey, int[] var, byte[] ciphertext, int len, String stbCompare, boolean print) {


        long jbestscore = jointScore(ckey, ciphertext, len);
        String keepStb = ckey.stbString();


        if (print)
            System.out.print("\nHC PHASE 2 - START\n");


        boolean improved;

        do {

            improved = false;

            hillClimbStep("HC PHASE 2 - NEWTOP - BIGRAMS", Key.EVAL.BI, ckey, var, ciphertext, len, stbCompare, print);

            hillClimbStep("HC PHASE 2 - NEWTOP - TRIGRAMS", Key.EVAL.TRI, ckey, var, ciphertext, len, stbCompare, print);

            long newScore = jointScore(ckey, ciphertext, len);

            if (newScore > jbestscore) {
                jbestscore = newScore;
                improved = true;
                keepStb = ckey.stbString();
                if (print) {
                    //if (ckey.triscore( ciphertext, len)>15000)
                    System.out.printf("PHASE 2 - RMSG %d - SCORE %d (%d + %d) IC: %.3f\n", ckey.rMesg, newScore,
                            ckey.triscore(ciphertext, len), ckey.biscore(ciphertext, len), ckey.icscore(ciphertext, len));
                }
            }


        } while (improved);

        ckey.setStecker(keepStb);
        return ckey.triscore(ciphertext, len);

    }

    private static long hillClimbPhase2Steepest(Key ckey, byte[] ciphertext, int len, String stbCompare, boolean print) {


        long a;

        int bigramWeightOf10 = 5;

        boolean newtop;

        long icfactor = (long) (ckey.icscore(ciphertext, len) * 100000.0);
        long jbestscore = ((10 - bigramWeightOf10) * ckey.triscore(ciphertext, len) +
                bigramWeightOf10 * ckey.biscore(ciphertext, len)) / 10;

        jbestscore += icfactor;

        if (print)
            System.out.print("\nHC PHASE 2 - STEEPEST- START\n");


        do {

            newtop = false;


            if (print)
                ckey.printKeyString("HC PHASE 2 - STEEPEST - NEWTOP");

            hillClimbPhase2BiscoreSteepest(ckey, ciphertext, len, stbCompare, print);

            long besttriscore = hillClimbPhase2TriscoreSteepest(ckey, ciphertext, len, stbCompare, print);

            ckey.score = (int) besttriscore;

            icfactor = (long) (ckey.icscore(ciphertext, len) * 100000.0);

            a = ((10 - bigramWeightOf10) * besttriscore + bigramWeightOf10 * ckey.biscore(ciphertext, len)) / 10;
            a += icfactor;


            if (a > jbestscore) {
                jbestscore = a;
                newtop = true;
                if (print)
                    //if (ckey.triscore( ciphertext, len)>15000)
                    System.out.printf("PHASE 2 - STEEPEST %d - SCORE %d (%d + %d) IC: %.3f\n", ckey.rMesg, a,
                            ckey.triscore(ciphertext, len), ckey.biscore(ciphertext, len), ckey.icscore(ciphertext, len));
            }


        } while (newtop);

        return ckey.score;

    }

    private static long hillClimbPhase2SlowOriginal(Key ckey, int[] var, byte[] ciphertext, int len, String stbCompare, boolean print) {


        long a;

        long bestbiscore;
        long besttriscore = 0;
        int bigramWeightOf10 = 5;

        int newtop;

        SearchAction action;


        long icfactor = (long) (ckey.icscore(ciphertext, len) * 100000.0);
        long jbestscore = ((10 - bigramWeightOf10) * ckey.triscore(ciphertext, len) +
                bigramWeightOf10 * ckey.biscore(ciphertext, len)) / 10;

        jbestscore += icfactor;

        newtop = 1;

        if (print)
            System.out.print("\nHC PHASE 2 - START\n");

        while (newtop == 1) {

            newtop = 0;

            ckey.score = (int) jbestscore;

            if (print)
                ckey.printKeyString("HC PHASE 2 - NEWTOP");

            bestbiscore = ckey.biscore(ciphertext, len);


            for (int i = 0; i < 26; i++) {
                for (int k = i + 1; k < 26; k++) {
                    int vi = var[i];
                    int vk = var[k];
                    int vsi = ckey.stbrett[vi];
                    int vsk = ckey.stbrett[vk];
                    action = SearchAction.NO_CHANGE;
                    long prevbiscore = bestbiscore;


                    if ((vi == vsi && vk == vsk) || (vi == vsk && vk == vsk)) {
                        ckey.swap(vi, vk);
                        a = ckey.biscore(ciphertext, len);
                        if (a > bestbiscore) {
                            bestbiscore = a;
                            action = SearchAction.IandK;
                        }
                        if (action == SearchAction.NO_CHANGE)
                            ckey.swap(vi, vk);
                    } else if ((vi == vsi) && (vk != vsk)) {
                        ckey.swap(vk, vsk);

                        // all self
                        ckey.swap(vi, vk);
                        a = ckey.biscore(ciphertext, len);
                        if (a > bestbiscore) {
                            bestbiscore = a;
                            action = SearchAction.IandK;
                        }
                        ckey.swap(vi, vk);

                        // all self
                        ckey.swap(vi, vsk);
                        a = ckey.biscore(ciphertext, len);
                        if (a > bestbiscore) {
                            bestbiscore = a;
                            action = SearchAction.IandSK;
                        }
                        ckey.swap(vi, vsk);

                        // all self
                        switch (action) {
                            case IandK:
                                ckey.swap(vi, vk);
                                break;
                            case IandSK:
                                ckey.swap(vi, vsk);
                                break;
                            case NO_CHANGE:
                                ckey.swap(vk, vsk);
                                break;
                            default:
                                break;
                        }
                    } else if (vk == vsk && vi != vsi) {
                        ckey.swap(vi, vsi);

                        ckey.swap(vk, vi);
                        a = ckey.biscore(ciphertext, len);
                        if (a > bestbiscore) {
                            bestbiscore = a;
                            action = SearchAction.IandK;
                        }
                        ckey.swap(vk, vi);

                        ckey.swap(vk, vsi);
                        a = ckey.biscore(ciphertext, len);
                        if (a > bestbiscore) {
                            bestbiscore = a;
                            action = SearchAction.KandSI;
                        }
                        ckey.swap(vk, vsi);

                        switch (action) {
                            case IandK:
                                ckey.swap(vk, vi);
                                break;
                            case KandSI:
                                ckey.swap(vk, vsi);
                                break;
                            case NO_CHANGE:
                                ckey.swap(vi, vsi);
                                break;
                            default:
                                break;
                        }
                    } else if ((vi != vsi) && (vk != vsk)) {

                        ckey.swap(vi, vsi);
                        ckey.swap(vk, vsk);

                        // all self

                        ckey.swap(vi, vsi);
                        ckey.swap(vi, vsi);

                        // all Self now

                        ckey.swap(vi, vk);

                        a = ckey.biscore(ciphertext, len);
                        if (a > bestbiscore) {
                            bestbiscore = a;
                            action = SearchAction.IandK;
                        }

                        ckey.swap(vsi, vsk);


                        a = ckey.biscore(ciphertext, len);
                        if (a > bestbiscore) {
                            bestbiscore = a;
                            action = SearchAction.IandK_SIandSK;
                        }


                        ckey.swap(vi, vk);
                        ckey.swap(vsi, vsk);

                        // all Self now


                        ckey.swap(vi, vsk);


                        a = ckey.biscore(ciphertext, len);
                        if (a > bestbiscore) {
                            bestbiscore = a;
                            action = SearchAction.IandSK;
                        }

                        ckey.swap(vsi, vk);


                        a = ckey.biscore(ciphertext, len);
                        if (a > bestbiscore) {
                            bestbiscore = a;
                            action = SearchAction.IandSK_KandSI;
                        }


                        ckey.swap(vi, vsk);
                        ckey.swap(vsi, vk);

                        // all Self now


                        switch (action) {
                            case IandK:
                                ckey.swap(vi, vk);
                                break;
                            case IandSK:
                                ckey.swap(vi, vsk);
                                break;
                           case IandK_SIandSK:
                                ckey.swap(vi, vk);
                                ckey.swap(vsi, vsk);
                                break;
                           case IandSK_KandSI:
                                ckey.swap(vi, vsk);
                                ckey.swap(vsi, vk);
                                break;
                            case NO_CHANGE:
                                ckey.swap(vi, vsi);
                                ckey.swap(vk, vsk);
                                break;
                            default:
                                break;
                        }

                    }
                    if (print && (action != SearchAction.NO_CHANGE))

                        System.out.printf("HC PHASE 2 BI  - \tI [%s] SI [%s] K [%s] SK [%s] \tBISCORE \t%d (%d) (%d)\t>%s<(%d)\t" + action + "\t%.3f\n",
                                Utils.getChar(vi), Utils.getChar(vsi), Utils.getChar(vk), Utils.getChar(vsk),
                                bestbiscore,
                                bestbiscore - prevbiscore, ckey.triscore(ciphertext, len),
                                ckey.stbString(), ckey.compareStecker(stbCompare),
                                ckey.icscore(ciphertext, len));

                }
            }


            int trigramPasses = 1;
            besttriscore = ckey.triscore(ciphertext, len);
            for (int i = 0; i < 26 * trigramPasses; i++) {
                for (int k = (i % 26) + 1; k < 26; k++) {
                    int vi = var[i % 26];
                    int vk = var[k];
                    int vsi = ckey.stbrett[vi];
                    int vsk = ckey.stbrett[vk];
                    action = SearchAction.NO_CHANGE;

                    long prevtriscore = besttriscore;


                    if (vi == vsi && vk != vsk) {
                        ckey.swap(vk, vsk);

                        ckey.swap(vi, vk);
                        a = ckey.triscore(ciphertext, len);
                        if (a > besttriscore) {
                            besttriscore = a;
                            action = SearchAction.IandK;
                        }
                        ckey.swap(vi, vk);

                        ckey.swap(vi, vsk);
                        a = ckey.triscore(ciphertext, len);
                        if (a > besttriscore) {
                            besttriscore = a;
                            action = SearchAction.IandSK;
                        }
                        ckey.swap(vi, vsk);

                        switch (action) {
                            case IandK:
                                ckey.swap(vi, vk);
                                break;
                            case IandSK:
                                ckey.swap(vi, vsk);
                                break;
                            case NO_CHANGE:
                                ckey.swap(vk, vsk);
                                break;
                            default:
                                break;
                        }
                    } else if (vk == vsk && vi != vsi) {
                        ckey.swap(vi, vsi);

                        ckey.swap(vk, vi);
                        a = ckey.triscore(ciphertext, len);
                        if (a > besttriscore) {
                            besttriscore = a;
                            action = SearchAction.IandK;
                        }
                        ckey.swap(vk, vi);

                        ckey.swap(vk, vsi);
                        a = ckey.triscore(ciphertext, len);
                        if (a > besttriscore) {
                            besttriscore = a;
                            action = SearchAction.KandSI;
                        }
                        ckey.swap(vk, vsi);

                        switch (action) {
                            case IandK:
                                ckey.swap(vk, vi);
                                break;
                            case KandSI:
                                ckey.swap(vk, vsi);
                                break;
                            case NO_CHANGE:
                                ckey.swap(vi, vsi);
                                break;
                            default:
                                break;
                        }
                    } else if (vi != vsi && vk != vsk) {


                        ckey.swap(vi, vsi);
                        ckey.swap(vk, vsk);

                        // all self
                        ckey.swap(vi, vsi);
                        ckey.swap(vi, vsi);


                        // all Self now

                        ckey.swap(vi, vk);
                        a = ckey.triscore(ciphertext, len);
                        if (a > besttriscore) {
                            besttriscore = a;
                            action = SearchAction.IandK;
                        }
                        ckey.swap(vsi, vsk);

                        a = ckey.triscore(ciphertext, len);
                        if (a > besttriscore) {
                            besttriscore = a;
                            action = SearchAction.IandK_SIandSK;
                        }

                        ckey.swap(vi, vk);
/*
                 a = ckey.triscore(ciphertext, len);
                 if (a > besttriscore) {
                     besttriscore = a;
                     action = ACTION.SIandSK;
                 }
*/
                        ckey.swap(vsi, vsk);

                        // all Self now


                        ckey.swap(vi, vsk);
                        a = ckey.triscore(ciphertext, len);
                        if (a > besttriscore) {
                            besttriscore = a;
                            action = SearchAction.IandSK;
                        }
                        ckey.swap(vsi, vk);

                        a = ckey.triscore(ciphertext, len);
                        if (a > besttriscore) {
                            besttriscore = a;
                            action = SearchAction.IandSK_KandSI;
                        }

                        ckey.swap(vi, vsk);
/*
                 a = ckey.triscore(ciphertext, len);
                 if (a > besttriscore) {
                     besttriscore = a;
                     action = ACTION.KandSI;
                 }
*/
                        ckey.swap(vsi, vk);

                        // all Self now


                        switch (action) {
                            case IandK:
                                ckey.swap(vi, vk);
                                break;
                            case IandSK:
                                ckey.swap(vi, vsk);
                                break;
                            case IandK_SIandSK:
                                ckey.swap(vi, vk);
                                ckey.swap(vsi, vsk);
                                break;
                            case IandSK_KandSI:
                                ckey.swap(vi, vsk);
                                ckey.swap(vsi, vk);
                                break;
                            case NO_CHANGE:
                                ckey.swap(vi, vsi);
                                ckey.swap(vk, vsk);
                                break;
                            default:
                                break;
                        }


                    }
                    if (print && (action != SearchAction.NO_CHANGE))
                        System.out.printf("HC PHASE 2 TRI - \tI [%s] SI [%s] K [%s] SK [%s] \tTRISCORE \t%d (%d) (%d)\t>%s<(%d)\t" + action + "\t%.3f \n",
                                Utils.getChar(vi), Utils.getChar(vsi), Utils.getChar(vk), Utils.getChar(vsk),
                                besttriscore, besttriscore - prevtriscore, ckey.biscore(ciphertext, len),
                                ckey.stbString(), ckey.compareStecker(stbCompare), ckey.icscore(ciphertext, len));
                }


            }

            icfactor = (long) (ckey.icscore(ciphertext, len) * 100000.0);


            a = ((10 - bigramWeightOf10) * ckey.triscore(ciphertext, len) +
                    bigramWeightOf10 * ckey.biscore(ciphertext, len)) / 10;
            a += icfactor;


            if (a > jbestscore) {
                jbestscore = a;
                newtop = 1;
                if (print)
                    //if (ckey.triscore( ciphertext, len)>15000)
                    System.out.printf("PHASE 2 - RMSG %d - SCORE %d (%d + %d) IC: %.3f\n", ckey.rMesg, a,
                            ckey.triscore(ciphertext, len), ckey.biscore(ciphertext, len), ckey.icscore(ciphertext, len));
            }


        }

        return besttriscore;

    }

    private static long hillClimbPhase1SlowOriginal(Key ckey, int[] var, byte[] ciphertext, int len, String stbCompare, boolean print) {


        double bestic, ic;


        final double DBL_EPSILON = 0.000000001;

        SearchAction action;


        if (print)
            System.out.print("\n\nHC PHASE 1 SLOW ==============\n");

        bestic = ckey.icscore(ciphertext, len);
        double prev = bestic;
        for (int i = 0; i < 26; i++) {
            for (int k = i + 1; k < 26; k++) {
                int vi = var[i];
                int vk = var[k];
                int vsi = ckey.stbrett[vi];
                int vsk = ckey.stbrett[vk];
                action = SearchAction.NO_CHANGE;


                if ((vi == vsi && vk == vsk) || (vi == vsk && vk == vsi)) {
                    ckey.swap(vi, vk);
                    ic = ckey.icscore(ciphertext, len);
                    if (ic - bestic > DBL_EPSILON) {
                        bestic = ic;
                        action = SearchAction.IandK;

                    }
                    if (action == SearchAction.NO_CHANGE)
                        ckey.swap(vi, vk);
                } else if (vi == vsi && vk != vsk) {
                    ckey.swap(vk, vsk);

                    ckey.swap(vi, vk);
                    ic = ckey.icscore(ciphertext, len);
                    if (ic - bestic > DBL_EPSILON) {
                        bestic = ic;
                        action = SearchAction.IandK;
                    }
                    ckey.swap(vi, vk);

                    ckey.swap(vi, vsk);
                    ic = ckey.icscore(ciphertext, len);
                    if (ic - bestic > DBL_EPSILON) {
                        bestic = ic;
                        action = SearchAction.IandSK;
                    }
                    ckey.swap(vi, vsk);

                    switch (action) {
                        case IandK:
                            ckey.swap(vi, vk);
                            break;
                        case IandSK:
                            ckey.swap(vi, vsk);
                            break;
                        case NO_CHANGE:
                            ckey.swap(vk, vsk);
                            break;
                        default:
                            break;
                    }
                } else if (vk == vsk && vi != vsi) {
                    ckey.swap(vi, vsi);

                    ckey.swap(vk, vi);
                    ic = ckey.icscore(ciphertext, len);
                    if (ic - bestic > DBL_EPSILON) {
                        bestic = ic;
                        action = SearchAction.IandK;
                    }
                    ckey.swap(vk, vi);

                    ckey.swap(vk, vsi);
                    ic = ckey.icscore(ciphertext, len);
                    if (ic - bestic > DBL_EPSILON) {
                        bestic = ic;
                        action = SearchAction.KandSI;
                    }
                    ckey.swap(vk, vsi);

                    switch (action) {
                        case IandK:
                            ckey.swap(vk, vi);
                            break;
                        case KandSI:
                            ckey.swap(vk, vsi);
                            break;
                        case NO_CHANGE:
                            ckey.swap(vi, vsi);
                            break;
                        default:
                            break;
                    }
                } else if (vi != vsi && vk != vsk) {


                    ckey.swap(vi, vsi);
             /*
             if (newStates) {
                 ic = ckey.icscore(ciphertext, len);
                 if (ic-bestic > DBL_EPSILON) {
                   bestic = ic;
                   action = ACTION.KandSK;
                 }
             } */
                    ckey.swap(vk, vsk);
                    // all Self now

                    ckey.swap(vi, vsi);

             /*
             if (newStates) {
                 ic = ckey.icscore(ciphertext, len);
                 if (ic-bestic > DBL_EPSILON) {
                   bestic = ic;
                   action = ACTION.IandSI;
                 }
             }

              */

                    ckey.swap(vi, vsi);

                    // all Self now

                    ckey.swap(vi, vk);
                    ic = ckey.icscore(ciphertext, len);
                    if (ic - bestic > DBL_EPSILON) {
                        bestic = ic;
                        action = SearchAction.IandK;
                    }
                    ckey.swap(vsi, vsk);


                    ic = ckey.icscore(ciphertext, len);
                    if (ic - bestic > DBL_EPSILON) {
                        bestic = ic;
                        action = SearchAction.IandK_SIandSK;
                    }


                    ckey.swap(vi, vk);

             /*
             if (newStates) {
                 ic = ckey.icscore(ciphertext, len);
                 if (ic-bestic > DBL_EPSILON) {
                   bestic = ic;
                   action = ACTION.SIandSK;
                 }
             } */

                    ckey.swap(vsi, vsk);

                    // all Self now


                    ckey.swap(vi, vsk);
                    ic = ckey.icscore(ciphertext, len);
                    if (ic - bestic > DBL_EPSILON) {
                        bestic = ic;
                        action = SearchAction.IandSK;
                    }
                    ckey.swap(vsi, vk);

                    ic = ckey.icscore(ciphertext, len);
                    if (ic - bestic > DBL_EPSILON) {
                        bestic = ic;
                        action = SearchAction.IandSK_KandSI;
                    }

                    ckey.swap(vi, vsk);

             /*
             if (newStates) {
                 ic = ckey.icscore(ciphertext, len);
                 if (ic-bestic > DBL_EPSILON) {
                   bestic = ic;
                   action = ACTION.KandSI;
                 }
             } */

                    ckey.swap(vsi, vk);

                    // all Self now


                    switch (action) {
                        case KandSK:
                            ckey.swap(vk, vsk);
                            break;
                        case IandSI:
                            ckey.swap(vi, vsi);
                            break;
                        case IandK:
                            ckey.swap(vi, vk);
                            break;
                        case IandSK:
                            ckey.swap(vi, vsk);
                            break;
                        case KandSI:
                            ckey.swap(vk, vsi);
                            break;
                        case IandK_SIandSK:
                            ckey.swap(vi, vk);
                            ckey.swap(vsi, vsk);
                            break;
                        case SIandSK:
                            ckey.swap(vsi, vsk);
                            break;
                        case IandSK_KandSI:
                            ckey.swap(vi, vsk);
                            ckey.swap(vsi, vk);
                            break;
                        case NO_CHANGE:
                            ckey.swap(vi, vsi);
                            ckey.swap(vk, vsk);
                            break;
                        default:
                            break;
                    }
                }
                if (print && (action != SearchAction.NO_CHANGE))

                    System.out.printf("HC PHASE 1 IC  - \tI [%s] SI [%s] K [%s] SK [%s] \tICSCORE \t%f (%f) \t>%s<(%d)\t" + action + "\n",
                            Utils.getChar(vi), Utils.getChar(vsi), Utils.getChar(vk), Utils.getChar(vsk), bestic, bestic - prev, ckey.stbString(), ckey.compareStecker(stbCompare));


            }
        }


        return (int) (bestic * 100000.0);

    }

    private static long hillClimbPhase1Slow(Key ckey, int[] var, byte[] ciphertext, int len, String stbCompare, boolean print) {

        String title = "HC PHASE 1 IC";

        return hillClimbStep(title, Key.EVAL.IC, ckey, var, ciphertext, len, stbCompare, print);

    }

    private static long hillClimbPhase1RandomIc(Key ckey, byte[] ciphertext, int len, boolean print) {


        double bestic, ic;

        if (print)
            System.out.print("\n\nHC PHASE 1 RANDOM ==============\n");

        String bestStb = "";

        bestic = ckey.icscore(ciphertext, len);
        for (int i = 0; i < 100; i++) {
            ckey.setRandomStb(8);
            ic = ckey.icscore(ciphertext, len);
            if (ic > bestic) {
                bestic = ic;
                bestStb = ckey.stbString();
            }
        }

        ckey.setStecker(bestStb);


        return (int) (bestic * 100000.0);

    }

    private static long hillClimbPhase1Uniscore(Key ckey, int[] var, byte[] ciphertext, int len, String stbCompare, boolean print) {

        String title = "HC PHASE 1 UNISCORE";

        return hillClimbStep(title, Key.EVAL.UNI, ckey, var, ciphertext, len, stbCompare, print);

    }

    private static long hillClimbPhase1DotteryPlusYoxall(Key ckey, byte[] ciphertext, int len, boolean print) {

        int[][] stats = new int[26][26];
        int[] colStats = new int[26]; // output letters

        // dottery phase
        ckey.dottery(ciphertext, len, stats, colStats);

        byte Estecker = 0;
        for (byte i = 1; i < 26; i++)
            if (colStats[i] > colStats[Estecker])
                Estecker = i;
        ckey.swap(Utils.getIndex('E'), Estecker);


        if (print) {
            for (int col = 0; col < 26; col++)
                System.out.printf("\t %s", Utils.getChar(col));
            System.out.print("\n");

            for (int row = 0; row < 26; row++) {
                System.out.printf("%s:", Utils.getChar(row));

                for (int col = 0; col < 26; col++) {
                    System.out.printf("\t %d", stats[row][col]);
                }
                System.out.print("\n");
            }
            for (int col = 0; col < 26; col++)
                System.out.printf("\t %d", colStats[col]);
            System.out.print("\n");
        }
        boolean locked[] = new boolean[26];
        locked[Utils.getIndex('E')] = locked[Estecker] = true;

        String selfS = "";
        if (Utils.getIndex('E') == Estecker)
            selfS += "E";
        // lock the 5 best rows (except E Stecker) as self-steckered
        for (int i = 0; i < 5; i++) {
            int bestRow;
            for (bestRow = 0; bestRow < 26; bestRow++)
                if (!locked[bestRow])
                    break;
            for (int k = bestRow + 1; k < 26; k++)
                if (!locked[k] && (stats[k][Estecker] > stats[bestRow][Estecker]))
                    bestRow = k;
            if (stats[bestRow][Estecker] < 2)
                break;
            locked[bestRow] = true;
            selfS += Utils.getChar(bestRow);

        }
        if (print)
            ckey.printKeyString("HC PHASE 1 - DOTTERY Self:" + selfS);

        // end of dottery


        // Start of Yoxallismus
        byte[] Yciphertext = new byte[len];
        byte[] Yplaintext = new byte[len];

        for (int i = 0; i < len; i++)
            Yciphertext[i] = Estecker;
        ckey.encipherDecipherAll(Yciphertext, Yplaintext, len);

        int Ystats[][] = new int[26][26];
        for (int i = 0; i < len; i++) {
            int l1 = Yplaintext[i];
            if (locked[l1])
                continue;
            int l2 = ciphertext[i];
            if (locked[l2])
                continue;
            // the matrix keeps only for l1 <= l2
            if (l1 > l2) {
                int temp = l1;
                l1 = l2;
                l2 = temp;
            }
            Ystats[l1][l2]++;
        }

        if (print) {
            for (int col = 0; col < 26; col++)
                System.out.printf("\t %s", Utils.getChar(col));
            System.out.print("\n");

            for (int row = 0; row < 26; row++) {
                System.out.printf("%s:", Utils.getChar(row));

                for (int col = 0; col < 26; col++) {
                    System.out.printf("\t %d", Ystats[row][col]);
                }
                System.out.print("\n");
            }

        }
        // find the 5  most likely (unlocked) plugs with stats 3 or more
        for (int i = 0; i < 5; i++) {
            int bestRow = -1;
            int bestCol = -1;
            int bestY = 0;
            for (int row = 0; row < 26; row++) {
                if (locked[row])
                    continue;
                for (int col = 0; col < 26; col++) {
                    if (locked[col])
                        continue;
                    if (Ystats[row][col] > bestY) {
                        bestRow = row;
                        bestCol = col;
                        bestY = Ystats[row][col];
                    }
                }

            }
            if (bestY < 3)
                break;
            locked[bestRow] = locked[bestCol] = true;
            if (bestRow != bestCol)
                ckey.swap(bestRow, bestCol);


        }

        if (print)
            ckey.printKeyString("HC PHASE 1 - YOXALL");

        return 0;

    }

    private static long hillClimbPhase1Yoxall(Key ckey, byte[] ciphertext, int len, String stbCompare, boolean print) {

        boolean detailedPrint = false;

        int bestSteckerE = 0;
        int bestbiscore = 0;
        String bestStb = "";


        for (byte Estecker = 0; Estecker < 26; Estecker++) {
            boolean locked[] = new boolean[26];

            locked[Utils.getIndex('E')] = locked[Estecker] = true;

            ckey.setStecker("");

            ckey.swap(Utils.getIndex('E'), Estecker);

            byte[] Yciphertext = new byte[len];
            byte[] Yplaintext = new byte[len];

            for (int i = 0; i < len; i++)
                Yciphertext[i] = Estecker;
            ckey.encipherDecipherAll(Yciphertext, Yplaintext, len);

            int Ystats[][] = new int[26][26];
            for (int i = 0; i < len; i++) {
                int l1 = Yplaintext[i];
                if (locked[l1])
                    continue;
                int l2 = ciphertext[i];
                if (locked[l2])
                    continue;
                // the matrix keeps only for l1 <= l2
                if (l1 > l2) {
                    int temp = l1;
                    l1 = l2;
                    l2 = temp;
                }
                Ystats[l1][l2]++;
            }

            if (print && detailedPrint) {
                for (int col = 0; col < 26; col++)
                    System.out.printf("\t %s", Utils.getChar(col));
                System.out.print("\n");

                for (int row = 0; row < 26; row++) {
                    System.out.printf("%s:", Utils.getChar(row));

                    for (int col = 0; col < 26; col++) {
                        System.out.printf("\t %d", Ystats[row][col]);
                    }
                    System.out.print("\n");
                }

            }
            // find the 5  most likely (unlocked) plugs with stats 4 or more
            for (int i = 0; i < 5; i++) {
                int bestRow = -1;
                int bestCol = -1;
                int bestY = 0;
                for (int row = 0; row < 26; row++) {
                    if (locked[row])
                        continue;
                    for (int col = 0; col < 26; col++) {
                        if (locked[col])
                            continue;
                        if (Ystats[row][col] > bestY) {
                            bestRow = row;
                            bestCol = col;
                            bestY = Ystats[row][col];
                        }
                    }

                }
                if (bestY < 4)
                    break;
                locked[bestRow] = locked[bestCol] = true;
                if (bestRow != bestCol)
                    ckey.swap(bestRow, bestCol);


            }
            ckey.score = ckey.biscore(ciphertext, len);
            if (print)
                ckey.printKeyString("HC PHASE 1 - YOXALL: " + Utils.getChar(Estecker));
            if (ckey.score > bestbiscore) {
                bestSteckerE = Estecker;
                bestbiscore = ckey.score;
                bestStb = ckey.stbString();
            }

        }
        ckey.setStecker(bestStb);
        ckey.score = bestbiscore;
        if (print)
            ckey.printKeyString("HC PHASE 1 - YOXALL - BEST: " + Utils.getChar(bestSteckerE));

        return 0;

    }

    private static long hillClimbPhase1UniscoreSteepest(Key ckey, byte[] ciphertext, int len, String stbCompare, boolean print) {

        String title = "HC PHASE 1 UNISCORE";

        return hillClimbStepSteepestPartial(title, Key.EVAL.UNI, ckey, ciphertext, len, stbCompare, print);


    }

    private static long hillClimbPhase2TriscoreSteepest(Key ckey, byte[] ciphertext, int len, String stbCompare, boolean print) {

        String title = "HC PHASE 2 TRI";

        return hillClimbStepSteepestFull(title, Key.EVAL.TRI, ckey, ciphertext, len, stbCompare, print);


    }

    private static long hillClimbPhase2BiscoreSteepest(Key ckey, byte[] ciphertext, int len, String stbCompare, boolean print) {

        String title = "HC PHASE 2 BI";

        return hillClimbStepSteepestFull(title, Key.EVAL.BI, ckey, ciphertext, len, stbCompare, print);

    }

    private static long hillClimbPhase1Steepest(Key ckey, byte[] ciphertext, int len, String stbCompare, boolean print) {

        String title = "HC PHASE 1 IC";

        return hillClimbStepSteepestPartial(title, Key.EVAL.IC, ckey, ciphertext, len, stbCompare, print);

    }

    private static long hillClimbPhase3(Key ckey, byte[] ciphertext, int len, boolean print) {

        class Change {
            int s1;         /* positions of letters to be swapped */
            int s2;
            int u1;         /* positions of letters to be unswapped */
            int u2;
        }
        Change ch = new Change();

        int a;

        ckey.getSteckerToSf();

        long bestscore = ckey.triscore(ciphertext, len);

        int newtop = 1;


        while (newtop == 1) {

            newtop = 0;
            SearchAction action = SearchAction.NO_CHANGE;
            long prevscore = bestscore;

         /* try reswapping each self-steckered with each pair,
          * steepest ascent */
            for (int i = 0; i < ckey.stbCount; i += 2) {
                ckey.swap(ckey.sf[i], ckey.sf[i + 1]);
                for (int k = ckey.stbCount; k < 26; k++) {
                    ckey.swap(ckey.sf[i], ckey.sf[k]);
                    a = ckey.triscore(ciphertext, len);

                    if (a > bestscore) {
                        newtop = 1;
                        action = SearchAction.RESWAP1;
                        bestscore = a;
                        ch.u1 = i;
                        ch.u2 = i + 1;
                        ch.s1 = k;
                        ch.s2 = i;

                    }
                    ckey.swap(ckey.sf[i], ckey.sf[k]);
                    ckey.swap(ckey.sf[i + 1], ckey.sf[k]);
                    a = ckey.triscore(ciphertext, len);
                    if (a > bestscore) {
                        newtop = 1;
                        action = SearchAction.RESWAP2;
                        bestscore = a;
                        ch.u1 = i;
                        ch.u2 = i + 1;
                        ch.s1 = k;
                        ch.s2 = i + 1;
                    }
                    ckey.swap(ckey.sf[i + 1], ckey.sf[k]);
                }
                ckey.swap(ckey.sf[i], ckey.sf[i + 1]);
            }
            if (action != SearchAction.NO_CHANGE) {
                ckey.swap(ckey.sf[ch.u1], ckey.sf[ch.u2]);
                ckey.swap(ckey.sf[ch.s1], ckey.sf[ch.s2]);
                ckey.getSteckerToSf();
            }
            if (print && (action != SearchAction.NO_CHANGE))

                System.out.printf("HC PHASE 3 BI  - \tU1 [%s] U2 [%s] S1 [%s] S2 [%s] \tTRISCORE \t%d (%d)\t" + action + "\n",
                        ch.u1, ch.u2, ch.s1, ch.s2, bestscore, bestscore - prevscore);


        }

        return bestscore;

    }

    private static void hillClimbSingleKeySinglePassTest(Key ckey, int[] stb, int[] var, byte[] ciphertext, int len, boolean firstPass) {
        long besttriscore;
        long besticscore = 0;
        long p1Score;
        String realStb = "";
        boolean detailHcPrint = false;
        boolean testAll;
        boolean testPrint;

        if (ckey.model != Key.Model.M4)
            ckey.initPathLookupHandM3(len);
        else
            ckey.initPathLookupAll(len);

        //realStb ="AXBDCVEQFUGNISLMORTZ";
        //detailHcPrint = true;
        testAll = true;
        testPrint = true;
        int origLoops = 100;


        ckey.setStecker(stb); // restore because the ones on ckey were changed in previous keys/passes
        // for the first pass, use existing stb (if not empty) and skip IC phase, maybe we have a winner with Tri
        if (firstPass && (ckey.stbCount != 0)) {

            besttriscore = hillClimbPhase2(ckey, var, ciphertext, len, realStb, detailHcPrint);

        } else {

            if (testPrint)
                System.out.printf("\nHC PASS %s - TESTING ALL METHODS\n", firstPass ? "1" : "n");

            // this one always done
            ckey.setStecker("");
            besticscore = hillClimbPhase1Slow(ckey, var, ciphertext, len, realStb, detailHcPrint);
            p1Score = ckey.triscore(ciphertext, len);
            besttriscore = hillClimbPhase2(ckey, var, ciphertext, len, realStb, detailHcPrint);
            String bestMethodStb = ckey.stbString();
            if (testPrint)
                System.out.printf("SLOW:      \t%d\t%d\n", besttriscore, p1Score);

/*
          if (testAll) {
              ckey.SetStecker("");
              besticscore = hillClimbPhase1SlowOriginal(false,ckey,  var, ciphertext, len, realStb,detailHcPrint);
              p1Score = ckey.triscore(ciphertext, len);
              long originalMethodTriScore = hillClimbPhase2Original(false, ckey,  var, ciphertext, len, realStb, detailHcPrint) ;
              if (originalMethodTriScore < besttriscore)
                  ckey.SetStecker(bestMethodStb);
              else {
                  besttriscore = originalMethodTriScore;
                  bestMethodStb = new String(ckey.StbString());
                  if (testPrint)
                     System.out.printf("ORIGINAL: \t%d\t%d\n",originalMethodTriScore, p1Score);

              }

          }
*/

/*
          if (testAll) {
              ckey.SetStecker("");
              besticscore = hillClimbPhase1Steepest(ckey,  var, ciphertext, len, realStb,detailHcPrint);
              p1Score = ckey.triscore(ciphertext, len);
              long slowAndSteepestPhase2MethodTriScore = hillClimbPhase2Steepest(ckey,  var, ciphertext, len, realStb, false) ;
              if (slowAndSteepestPhase2MethodTriScore < besttriscore)
                  ckey.SetStecker(bestMethodStb);
              else {
                  besttriscore = slowAndSteepestPhase2MethodTriScore;
                  bestMethodStb = new String(ckey.StbString());
                  if (testPrint)
                  System.out.printf("P2 STEEP: \t%d\t%d\n",slowAndSteepestPhase2MethodTriScore, p1Score);
              }


          }
  */

            if (testAll) {
                ckey.setStecker("");
                besticscore = hillClimbPhase1Steepest(ckey, ciphertext, len, realStb, detailHcPrint);
                p1Score = ckey.triscore(ciphertext, len);
                //long steepMethodTriScore = hillClimbPhase2(ckey,  var, ciphertext, len, realStb, detailHcPrint) ;
                long steepMethodTriScore = hillClimbPhase2SlowOriginal(ckey, var, ciphertext, len, realStb, detailHcPrint);
                if (steepMethodTriScore < besttriscore)
                    ckey.setStecker(bestMethodStb);
                else {
                    besttriscore = steepMethodTriScore;
                    bestMethodStb = ckey.stbString();
                    if (testPrint)
                        System.out.printf("STEEPEST: \t%d\t%d\n", steepMethodTriScore, p1Score);
                }


            }


            if (testAll) {
                ckey.setStecker("");
                besticscore = hillClimbPhase1RandomIc(ckey, ciphertext, len, detailHcPrint);
                p1Score = ckey.triscore(ciphertext, len);
                long randomIcMethodTriScore = hillClimbPhase2SlowOriginal(ckey, var, ciphertext, len, realStb, detailHcPrint);
                if (randomIcMethodTriScore < besttriscore)
                    ckey.setStecker(bestMethodStb);
                else {
                    besttriscore = randomIcMethodTriScore;
                    bestMethodStb = ckey.stbString();
                    if (testPrint)
                        System.out.printf("RANDOMIC: \t%d\t%d\n", randomIcMethodTriScore, p1Score);

                }

            }


            if (testAll) {
                ckey.setStecker("");
                p1Score = ckey.triscore(ciphertext, len);
                long noPhase1MethodTriScore = hillClimbPhase2SlowOriginal(ckey, var, ciphertext, len, realStb, detailHcPrint);
                if (noPhase1MethodTriScore < besttriscore)
                    ckey.setStecker(bestMethodStb);
                else {
                    besttriscore = noPhase1MethodTriScore;
                    bestMethodStb = ckey.stbString();
                    if (testPrint)
                        System.out.printf("NO PHASE 1: \t%d\t%d\n", noPhase1MethodTriScore, p1Score);

                }

            }

            if (testAll) {
                ckey.setStecker("");
                besticscore = hillClimbPhase1DotteryPlusYoxall(ckey, ciphertext, len, detailHcPrint);
                p1Score = ckey.triscore(ciphertext, len);
                long yoxallPhase1MethodTriScore = hillClimbPhase2SlowOriginal(ckey, var, ciphertext, len, realStb, detailHcPrint);
                if (yoxallPhase1MethodTriScore < besttriscore)
                    ckey.setStecker(bestMethodStb);
                else {
                    besttriscore = yoxallPhase1MethodTriScore;
                    bestMethodStb = ckey.stbString();
                    if (testPrint)
                        System.out.printf("DOT& YOXALL: \t%d\t%d\n", yoxallPhase1MethodTriScore, p1Score);

                }
            }

            if (testAll) {
                ckey.setStecker("");
                hillClimbPhase1UniscoreSteepest(ckey, ciphertext, len, realStb, detailHcPrint);
                p1Score = ckey.triscore(ciphertext, len);
                long uniPhase1MethodUniscoreSteepest = hillClimbPhase2SlowOriginal(ckey, var, ciphertext, len, realStb, detailHcPrint);
                if (uniPhase1MethodUniscoreSteepest < besttriscore)
                    ckey.setStecker(bestMethodStb);
                else {
                    besttriscore = uniPhase1MethodUniscoreSteepest;
                    bestMethodStb = ckey.stbString();
                    if (testPrint)
                        System.out.printf("UNI STEEPEST: \t%d\t%d\n", uniPhase1MethodUniscoreSteepest, p1Score);

                }
            }
            if (testAll) {
                ckey.setStecker("");
                hillClimbPhase1Uniscore(ckey, var, ciphertext, len, realStb, detailHcPrint);
                p1Score = ckey.triscore(ciphertext, len);
                long uniPhase1MethodUniscore = hillClimbPhase2SlowOriginal(ckey, var, ciphertext, len, realStb, detailHcPrint);
                if (uniPhase1MethodUniscore < besttriscore)
                    ckey.setStecker(bestMethodStb);
                else {
                    besttriscore = uniPhase1MethodUniscore;
                    bestMethodStb = ckey.stbString();
                    if (testPrint)
                        System.out.printf("UNISCORE: \t%d\t%d\n", uniPhase1MethodUniscore, p1Score);

                }
            }


            if (testAll) {
                int offset = len / 4;
                byte[] offsetCipherText = new byte[Key.MAXLEN];
                System.arraycopy(ciphertext, offset, offsetCipherText, 0, len - 1);
                ckey.setStecker("");
                besticscore = hillClimbPhase1Slow(ckey, var, offsetCipherText, len - offset, realStb, detailHcPrint);
                p1Score = ckey.triscore(ciphertext, len);
                long offsetPhase1MethodTriScore = hillClimbPhase2SlowOriginal(ckey, var, ciphertext, len, realStb, detailHcPrint);
                if (offsetPhase1MethodTriScore < besttriscore)
                    ckey.setStecker(bestMethodStb);
                else {
                    besttriscore = offsetPhase1MethodTriScore;
                    bestMethodStb = ckey.stbString();
                    if (testPrint)
                        System.out.printf("OFFSET 1/4: \t%d\t%d\n", offsetPhase1MethodTriScore, p1Score);

                }
            }

            if (testAll) {
                int cutlen = len - len / 4;

                ckey.setStecker("");
                besticscore = hillClimbPhase1Slow(ckey, var, ciphertext, cutlen, realStb, detailHcPrint);
                p1Score = ckey.triscore(ciphertext, len);
                long trimPhase1MethodTriScore = hillClimbPhase2SlowOriginal(ckey, var, ciphertext, len, realStb, detailHcPrint);
                if (trimPhase1MethodTriScore < besttriscore)
                    ckey.setStecker(bestMethodStb);
                else {
                    besttriscore = trimPhase1MethodTriScore;
                    bestMethodStb = ckey.stbString();
                    if (testPrint)
                        System.out.printf("TRIM 1/4: \t%d\t%d\n", trimPhase1MethodTriScore, p1Score);

                }
            }

            if (testAll) {
                int count = 0;
                long originalMethodTriScore = 0;
                for (int i = 0; i < origLoops; i++) {
                    ckey.setStecker("");

                    if (i == 0)
                        Key.setToCtFreq(var, ciphertext, len);
                    else
                        Key.randVar(var);
                    besticscore = hillClimbPhase1SlowOriginal(ckey, var, ciphertext, len, realStb, detailHcPrint);
                    p1Score = ckey.triscore(ciphertext, len);
                    originalMethodTriScore = hillClimbPhase2SlowOriginal(ckey, var, ciphertext, len, realStb, detailHcPrint);

                    if (originalMethodTriScore < besttriscore)
                        ckey.setStecker(bestMethodStb);
                    else {
                        besttriscore = originalMethodTriScore;
                        bestMethodStb = ckey.stbString();
                        if (testPrint && (i < 5))
                            System.out.printf("ORIGINAL(%d): \t%d\t%d\n", i, originalMethodTriScore, p1Score);
                        count++;

                    }
                }
                if (testPrint)
                    System.out.printf("ORIGINAL: \t(%d times max or improved )\n", count);

            }


            if (testAll) {
                int count = 0;
                long testMethodTriScore = 0;
                for (int i = 0; i < origLoops; i++) {
                    ckey.setStecker("");
                    if (i == 0)
                        Key.setToCtFreq(var, ciphertext, len);
                    else
                        Key.randVar(var);

                    besticscore = hillClimbPhase1Steepest(ckey, ciphertext, len, realStb, detailHcPrint);
                    p1Score = ckey.triscore(ciphertext, len);
                    testMethodTriScore = hillClimbPhase2Steepest(ckey, ciphertext, len, realStb, detailHcPrint);

                    if (testMethodTriScore < besttriscore)
                        ckey.setStecker(bestMethodStb);
                    else {
                        besttriscore = testMethodTriScore;
                        bestMethodStb = ckey.stbString();
                        if (testPrint && (i < 5))
                            System.out.printf("ISN_STO (%d): \t%d\t%d\n", i, testMethodTriScore, p1Score);
                        count++;

                    }
                }
                if (testPrint)
                    System.out.printf("ISN_STO: \t(%d times max or improved )\n", count);

            }

        }

        //besttriscore = hillClimbPhase3(ckey,  ciphertext, len, detailHcPrint) ;

        ckey.score = (int) besttriscore;
    }

    public void setup(Key key, int[] nStb, int[] nVar, byte[] nCiphertext,
                      int nLen, boolean nFirstPass) {
        this.key.clone(key);

        this.stb = nStb;

        this.var = nVar;
        this.ciphertext = nCiphertext;
        this.len = nLen;
        this.firstPass = nFirstPass;
        this.key.score = -1; // to mark that no HC has been done on this one.

    }

    @Override
    public void run() {
        hillClimbSingleKeySinglePass(key, stb, var, ciphertext, len, firstPass);

    }

}
