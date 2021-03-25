package enigma;

import common.Runnables;

import java.text.SimpleDateFormat;
import java.util.*;

class HcSaRunnable implements Runnable {

    enum Action {NO_CHANGE, IandK, SIandSK, IandSK, KandSI, IandK_SIandSK, IandSK_KandSI}

    private static final int[] FREQUENT = {4, 13, 23, 17, 18, 0, 8, 19, 20, 14, 11, 3, 5, 6, 12, 1, 7, 10, 25, 22, 16, 21, 2, 15, 9, 24};

    public final Key key = new Key();
    public int bestOffset;
    private byte[] ciphertext = new byte[1000];
    private int len;
    private boolean firstPass;
    private int[] stb = new int[26];
    private int[] var = Arrays.copyOf(FREQUENT, FREQUENT.length);
    private Mode mode;
    private int rounds;
    private int rRingSpacing;

    public void setup(Key key, int[] stb, byte[] ciphertext, int len, boolean firstPass, Mode mode, int rounds, int rRingSpacing) {
        this.key.clone(key);
        System.arraycopy(ciphertext, 0, this.ciphertext, 0, len);
        System.arraycopy(stb, 0, this.stb, 0, 26);
        this.len = len;
        this.firstPass = firstPass;
        this.key.score = -1; // to mark that no HC has been done on this one.
        this.rounds = rounds;
        this.mode = mode;
        this.rRingSpacing = rRingSpacing;
    }

    @Override
    public void run() {

        if (key.model != Key.Model.M4) {
            key.initPathLookupHandM3(len);
        } else {
            key.initPathLookupAll(len);
        }
        Key.randVar(var);

        key.setStecker(stb); // restore because the ones on key were changed in previous keys/passes
        if (firstPass && (key.stbCount != 0)) {
            HCStep(Key.EVAL.TRI);
        } else {
            switch (mode) {
                case HC:
                    HC();
                    break;
                case SA:
                    SA();
                    break;
                case EStecker:
                    EStecker();
                    break;
            }
        }
    }

    private void EStecker() {


        //                      E   N  X    R  S   I  A  T    O  U
        final int[] FREQUENT = {4, 13, 23, 17, 18, 8, 0, 19, 14, 20, 11, 3, 5, 6, 12, 1, 7, 10, 25, 22, 16, 21, 2, 15, 9, 24};


        long bestScore = 0;
        String bestStb = "";

        boolean debug = false;
        for (int xIndexInMostFrequentLetters = 0; xIndexInMostFrequentLetters < Math.min(26, rounds); xIndexInMostFrequentLetters++) {
            int x = FREQUENT[xIndexInMostFrequentLetters];
            for (int yIndexInMostFrequentLetters = xIndexInMostFrequentLetters; yIndexInMostFrequentLetters < 26; yIndexInMostFrequentLetters++) {
                int y = FREQUENT[yIndexInMostFrequentLetters];
                key.setStecker("");
                long currentScore = key.eval(Key.EVAL.IC, ciphertext, len);
                if (debug)
                    System.out.printf("(Start  %c%c) %-22s %,8d\n", Utils.getChar(x), Utils.getChar(y), key.stbString(), currentScore);
                if (y != x) {
                    key.stbConnect(x, y);
                }

                phiMarsch(debug);
                currentScore = triSturm(debug, true);
                currentScore = bereinigeBrett(debug, currentScore);

                if (debug) {
                    System.out.printf("(Summary  ) %-22s %,8d (%,8d) %s\n", key.stbString(), currentScore, (int) Stats.triSchwelle(len), currentScore > Stats.triSchwelle(len) ? "!!!!!" : "");
                }

                int bestOffsetForCycle = 0;

                if (currentScore > Stats.triSchwelle(len)) {
                    long bestOffsetScore = currentScore;
                    for (int offset = -rRingSpacing + 1; offset < rRingSpacing; offset++) {
                        key.addRightRotorOffset(offset);
                        long score = key.triScoreWithoutLookupBuild(ciphertext, len);
                        if (score > bestOffsetScore) {
                            bestOffsetScore = score;
                            bestOffsetForCycle = offset;
                        }
                        key.substractRightRotorOffset(offset);
                    }
                    key.addRightRotorOffset(bestOffsetForCycle);
                    currentScore = triSturm(debug, false);
                    key.substractRightRotorOffset(bestOffsetForCycle);

                    if (debug) System.out.printf("(Krabben  ) %-22s %,8d\n", key.stbString(), currentScore);

                }
                if (currentScore > bestScore) {
                    bestStb = key.stbString();
                    bestScore = currentScore;
                    bestOffset = bestOffsetForCycle;
                }
            }
        }
        key.setStecker(bestStb);
        if (bestOffset == 0) {
            if (key.eval(Key.EVAL.TRI, ciphertext, len) != bestScore) {
                throw new RuntimeException("Inconsistent!!! " + key.score + " != " + bestScore);
            }
        } else {
            key.score = (int) bestScore;
        }


    }

    private long phiMarsch(boolean debug) {
        long currentScore;
        long newScore;
        currentScore = key.eval(Key.EVAL.IC, ciphertext, len);
        if (debug)  System.out.printf("(Start    ) %-22s %,8d\n", key.stbString(), currentScore);


        for (int m = 0; m < 8; m++) {
            int bestI = -1;
            int bestJ = -1;
            long bestIJScore = 0;
            for (int i = 0; i < 26; i++) {
                if (key.stbrett[i] != i) {
                    continue;
                }
                for (int j = i + 1; j < 26; j++) {
                    if (key.stbrett[j] != j) {
                        continue;
                    }
                    key.stbConnect(i, j);
                    newScore = key.eval(Key.EVAL.IC, ciphertext, len);
                    if (newScore > bestIJScore) {
                        bestI = i;
                        bestJ = j;
                        bestIJScore = newScore;
                    }
                    key.stbDisconnect(i, j);
                }
            }
            if (bestIJScore <= currentScore) {
                break;
            }
            key.stbConnect(bestI, bestJ);
            currentScore = key.eval(Key.EVAL.IC, ciphertext, len);
            if (debug)  System.out.printf("(Marsch %,2d) %-22s %,8d\n", m, key.stbString(), currentScore);
        }
        return currentScore;
    }

    private long bereinigeBrett(boolean debug, long currentScore) {
        long newScore;
        for (int c = 0; ; c++) {

            boolean changed = false;
            for (int i = 0; i < 26; i++) {
                int partnerOfI = key.stbrett[i];
                if (partnerOfI == i) {
                    continue;
                }
                key.stbDisconnect(i, partnerOfI);
                newScore = key.eval(Key.EVAL.TRI, ciphertext, len);
                if (newScore > currentScore) {
                    currentScore = newScore;
                    if (debug)  System.out.printf("(Clean %,2d) %-22s %,8d\n", c, key.stbString(), currentScore);
                    changed = true;
                } else {
                    key.stbConnect(i, partnerOfI);
                }
            }
            if (!changed) {
                break;
            }
        }
        if (debug) {
            System.out.printf("(Clean end) %-22s %,8d\n", key.stbString(), currentScore);
        }

        return currentScore;
    }

    private long triSturm(boolean debug, boolean useLookup) {
        long currentScore;
        long newScore;
        currentScore = useLookup ? key.eval(Key.EVAL.TRI, ciphertext, len) : key.triScoreWithoutLookupBuild(ciphertext, len);
        if (debug)  System.out.printf("(Tri      ) %-22s %,8d\n", key.stbString(), currentScore);

        for (int s = 0; ; s++) {
            int bestI = -1;
            int bestJ = -1;
            long bestIJScore = currentScore;
            for (int i = 0; i < 26; i++) {
                for (int j = i + 1; j < 26; j++) {
                    int partnerOfI = key.stbrett[i];
                    int partnerOfJ = key.stbrett[j];
                    if (key.stbCount() == Key.MAX_STB_PLUGS && partnerOfI == i && partnerOfJ == j) {
                        continue;
                    }

                    if (partnerOfI != i) {
                        key.stbDisconnect(i, partnerOfI);
                    }
                    if (partnerOfJ != j) {
                        key.stbDisconnect(j, partnerOfJ);
                    }
                    key.stbConnect(i, j);
                    newScore = useLookup ? key.eval(Key.EVAL.TRI, ciphertext, len) : key.triScoreWithoutLookupBuild(ciphertext, len);
                    if (newScore > bestIJScore) {
                        bestI = i;
                        bestJ = j;
                        bestIJScore = newScore;
                    }
                    key.stbDisconnect(i, j);
                    if (partnerOfJ != j) {
                        key.stbConnect(j, partnerOfJ);
                    }
                    if (partnerOfI != i) {
                        key.stbConnect(i, partnerOfI);
                    }
                }
            }
            if (bestI == -1) {
                break;
            }
            int partnerOfBestI = key.stbrett[bestI];
            int partnerOfBestJ = key.stbrett[bestJ];
            if (partnerOfBestI != bestI) {
                key.stbDisconnect(bestI, partnerOfBestI);
            }
            if (partnerOfBestJ != bestJ) {
                key.stbDisconnect(bestJ, partnerOfBestJ);
            }
            key.stbConnect(bestI, bestJ);
            currentScore = useLookup ? key.eval(Key.EVAL.TRI, ciphertext, len) : key.triScoreWithoutLookupBuild(ciphertext, len);
            if (debug)  System.out.printf("(Sturm  %,2d) %-22s %,8d\n", s, key.stbString(), currentScore);
            if (currentScore != bestIJScore) {
                throw new RuntimeException("Inconsistent!!! " + currentScore + " != " + bestIJScore);
            }
        }

        if (debug)  System.out.printf("(Sturm end) %-22s %,8d\n", key.stbString(), currentScore);
        return currentScore;
    }

    private void HC() {

        key.setStecker("");
        String bestStbS = "";
        long bestScore = 0;
        int noImprove = 0;
        for (int i = 0; i < rounds * 2 && noImprove < 3; i++) {//Math.min(1, 1000/len)

            HCStep(Key.EVAL.IC);
            HCStep(Key.EVAL.BI);
            HCStep(Key.EVAL.TRI);
            if (key.score > bestScore) {
                bestScore = key.score;
                bestStbS = key.stbString();
                noImprove = 0;
            } else {
                noImprove++;
            }
        }
        key.setStecker(bestStbS);
        long currentScore = key.score = key.eval(Key.EVAL.TRI, ciphertext, len);

        checkOffsets(currentScore);
    }

    private void checkOffsets(long currentScore) {
        if (currentScore > Stats.triSchwelle(len)) {
            long bestOffsetScore = currentScore;
            int bestOffsetForCycle = 0;
            for (int offset = -rRingSpacing + 1; offset < rRingSpacing; offset++) {
                key.addRightRotorOffset(offset);
                long score = key.triScoreWithoutLookupBuild(ciphertext, len);
                if (score > bestOffsetScore) {
                    bestOffsetScore = score;
                    bestOffsetForCycle = offset;
                }
                key.substractRightRotorOffset(offset);
            }
            key.addRightRotorOffset(bestOffsetForCycle);
            key.score = (int) triSturm(false, false);
            key.substractRightRotorOffset(bestOffsetForCycle);
            bestOffset = bestOffsetForCycle;
        }
    }

    private void SA() {
        key.setStecker("");
        for (int i = 0; i < rounds * 2; i++) {
            SAStep(Key.EVAL.BI);
        }
        //SAStep(Key.EVAL.TRI);

        long currentScore = key.score = key.eval(Key.EVAL.TRI, ciphertext, len);
        checkOffsets(currentScore);
    }

    private void HCStep(Key.EVAL eval) {

        Action action;

        long newScore;
        long bestScore = key.eval(eval, ciphertext, len);

        byte[] invVar = new byte[26];
        for (byte i = 0; i < 26; i++) {
            invVar[var[i]] = i;
        }
        boolean improved;
        do {
            improved = false;
            for (int i = 0; i < 26; i++) {
                int vi = var[i]; // invariant
               for (int k = i + 1; k < 26; k++) {
                    int vk = var[k];
                    int vsk = key.stbrett[vk];
                    int vsi = key.stbrett[vi]; // not an invariant
                    if (vsk == vi) {
                        continue;
                    }
                    int sk = invVar[vsk];
                    int si = invVar[vsi];

                    action = Action.NO_CHANGE;

                    if (vi == vsi && vk == vsk) {

                        if (key.stbCount() == Key.MAX_STB_PLUGS) {
                            continue;
                        }
                        key.stbConnect(vi, vk);

                        newScore = key.eval(eval, ciphertext, len);
                        if (newScore > bestScore) {
                            bestScore = newScore;
                            improved = true;
                            action = Action.IandK;
                        }
                        if (action == Action.NO_CHANGE) {
                            key.stbDisconnect(vi, vk);
                        }
                    } else if (vi == vsi) {

                        if ((sk > i) && (sk < k)) {
                            continue;
                        }
                        key.stbDisconnect(vk, vsk);
                        //all self
                        key.stbConnect(vi, vk);

                        newScore = key.eval(eval, ciphertext, len);
                        if (newScore > bestScore) {
                            bestScore = newScore;
                            improved = true;
                            action = Action.IandK;
                        }
                        key.stbDisconnect(vi, vk);
                        // all self
                        key.stbConnect(vi, vsk);

                        newScore = key.eval(eval, ciphertext, len);
                        if (newScore > bestScore) {
                            bestScore = newScore;
                            improved = true;
                            action = Action.IandSK;
                        }
                        key.stbDisconnect(vi, vsk);
                        // all self now
                        switch (action) {
                            case IandK:
                                key.stbConnect(vi, vk);
                                break;
                            case IandSK:
                                key.stbConnect(vi, vsk);
                                break;
                            case NO_CHANGE:
                                key.stbConnect(vk, vsk);
                                break;
                        }
                    } else if (vk == vsk) {

                        if ((si < k) && (si < i)) {
                            continue;
                        }
                        key.stbDisconnect(vi, vsi);
                        // all self
                        key.stbConnect(vk, vi);
                        newScore = key.eval(eval, ciphertext, len);
                        if (newScore > bestScore) {
                            bestScore = newScore;
                            improved = true;
                            action = Action.IandK;
                        }
                        key.stbDisconnect(vk, vi);
                        // all self
                        key.stbConnect(vk, vsi);
                        newScore = key.eval(eval, ciphertext, len);
                        if (newScore > bestScore) {
                            bestScore = newScore;
                            improved = true;
                            action = Action.KandSI;
                        }
                        key.stbDisconnect(vk, vsi);
                        // all self
                        switch (action) {
                            case IandK:
                                key.stbConnect(vi, vk);
                                break;
                            case KandSI:
                                key.stbConnect(vk, vsi);
                                break;
                            case NO_CHANGE:
                                key.stbConnect(vi, vsi);
                                break;
                        }
                    } else {
                        if ((si < i) || (sk < k)) {
                            continue;
                        }

                        key.stbDisconnect(vi, vsi);
                        key.stbDisconnect(vk, vsk);
                        // all Self now
                        key.stbConnect(vi, vk);
                        newScore = key.eval(eval, ciphertext, len);
                        if (newScore > bestScore) {
                            bestScore = newScore;
                            action = Action.IandK;
                        }
                        key.stbConnect(vsi, vsk);
                        newScore = key.eval(eval, ciphertext, len);
                        if (newScore > bestScore) {
                            bestScore = newScore;
                            action = Action.IandK_SIandSK;
                        }
                        key.stbDisconnect(vi, vk);
                        key.stbDisconnect(vsi, vsk);
                        // all Self now
                        key.stbConnect(vi, vsk);
                        newScore = key.eval(eval, ciphertext, len);
                        if (newScore > bestScore) {
                            bestScore = newScore;
                            action = Action.IandSK;
                        }
                        key.stbConnect(vsi, vk);
                        newScore = key.eval(eval, ciphertext, len);
                        if (newScore > bestScore) {
                            bestScore = newScore;
                            action = Action.IandSK_KandSI;
                        }
                        key.stbDisconnect(vi, vsk);
                        key.stbDisconnect(vsi, vk);

                        // all Self now

                        switch (action) {
                            case IandK:
                                key.stbConnect(vi, vk);
                                break;
                            case IandSK:
                                key.stbConnect(vi, vsk);
                                break;
                            case IandK_SIandSK:
                                key.stbConnect(vi, vk);
                                key.stbConnect(vsi, vsk);
                                break;
                            case IandSK_KandSI:
                                key.stbConnect(vi, vsk);
                                key.stbConnect(vsi, vk);
                                break;
                            case NO_CHANGE:
                                key.stbConnect(vi, vsi);
                                key.stbConnect(vk, vsk);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        } while (improved);

        if (key.eval(eval, ciphertext, len) != bestScore) {
            throw new RuntimeException("Best result is not consistent");
        }
        key.score = key.eval(eval, ciphertext, len);

    }

    private void SAStep(Key.EVAL eval) {

        Action action;

        String bestStb = key.stbString();
        long bestScore = key.eval(eval, ciphertext, len);

        long newScore;
        long currScore = key.eval(eval, ciphertext, len);

        boolean changed;
        int roundsWithoutChange = 0;
        double temp;
        if (len <= 30) {
            temp = 400.0;
        } else if (len <= 50) {
            temp = 400.0 - (400.0 - 315.0) * (len - 30)/(50 - 30);
        } else if (len <= 75) {
            temp = 315.0 - (315.0 - 240.0) * (len - 50)/(75 - 50);
        } else if (len <= 100) {
            temp = 240.0 - (240.0 - 220.0) * (len - 75)/(100 - 75);
        } else if (len <= 150) {
            temp = 220.0 - (220.0 - 200.0) * (len - 100)/(150 - 100);
        }else {
            temp = 200.0;
        }
        int ROUNDS = 200;
        for (int round = 0; round < ROUNDS && roundsWithoutChange < 10; round++) {
            Key.randVar(var);

            changed = false;
            for (int i = 0; i < 26; i++) {
                int vi = var[i]; // invariant

                for (int k = i + 1; k < 26; k++) {

                    int vk = var[k];
                    int vsk = key.stbrett[vk];
                    int vsi = key.stbrett[vi]; // not an invariant

                    if (vsk == vi) {
                        continue;
                    }

                    action = Action.NO_CHANGE;

                    if (vi == vsi && vk == vsk) {
                        if (key.stbCount() == Key.MAX_STB_PLUGS) {
                            continue;
                        }
                        key.stbConnect(vi, vk);
                        newScore = key.eval(eval, ciphertext, len);
                        if (accept(newScore, currScore, temp)) {
                            currScore = newScore;
                            changed = true;
                            if (newScore > bestScore) {
                                bestScore = newScore;
                                bestStb = key.stbString();
                            }
                        } else {
                            key.stbDisconnect(vi, vk);
                        }
                    } else if (vi == vsi) { // vk != vsk

                        if (vsk < vk) {
                            continue;
                        }

                        key.stbDisconnect(vk, vsk);
                        key.stbConnect(vi, vk);

                        newScore = key.eval(eval, ciphertext, len);
                        if (accept(newScore, currScore, temp)) {
                            currScore = newScore;
                            changed = true;
                            action = Action.IandK;
                            if (newScore > bestScore) {
                                bestScore = newScore;
                                bestStb = key.stbString();
                            }
                        }
                        key.stbDisconnect(vi, vk);
                        // all self
                        key.stbConnect(vi, vsk);

                        newScore = key.eval(eval, ciphertext, len);
                        if (accept(newScore, currScore, temp)) {
                            currScore = newScore;
                            changed = true;
                            action = Action.IandSK;
                            if (newScore > bestScore) {
                                bestScore = newScore;
                                bestStb = key.stbString();
                            }
                        }
                        key.stbDisconnect(vi, vsk);
                        // all self now

                        switch (action) {
                            case IandK:
                                key.stbConnect(vi, vk);
                                break;
                            case IandSK:
                                key.stbConnect(vi, vsk);
                                break;
                            case NO_CHANGE:
                                key.stbConnect(vk, vsk);
                                break;
                            default:
                                break;
                        }

                    } else if (vk == vsk) {

                        if (vsi < vi) {
                            continue;
                        }
                        key.stbDisconnect(vi, vsi);
                        // all self
                        key.stbConnect(vk, vi);
                        newScore = key.eval(eval, ciphertext, len);
                        if (accept(newScore, currScore, temp)) {
                            currScore = newScore;
                            changed = true;
                            action = Action.IandK;
                            if (newScore > bestScore) {
                                bestScore = newScore;
                                bestStb = key.stbString();
                            }
                        }
                        key.stbDisconnect(vk, vi);

                        // all self
                        key.stbConnect(vk, vsi);
                        newScore = key.eval(eval, ciphertext, len);
                        if (accept(newScore, currScore, temp)) {
                            currScore = newScore;
                            changed = true;
                            action = Action.KandSI;
                            if (newScore > bestScore) {
                                bestScore = newScore;
                                bestStb = key.stbString();
                            }
                        }
                        key.stbDisconnect(vk, vsi);
                        // all self
                        switch (action) {
                            case IandK:
                                key.stbConnect(vi, vk);
                                break;
                            case KandSI:
                                key.stbConnect(vk, vsi);
                                break;
                            case NO_CHANGE:
                                key.stbConnect(vi, vsi);
                                break;
                            default:
                                break;
                        }
                    } else {

                        if ((vsi < vi) || (vsk < vk)) {
                            continue;
                        }

                        key.stbDisconnect(vi, vsi);
                        key.stbDisconnect(vk, vsk);
                        // all Self now

                        key.stbConnect(vi, vk);
                        newScore = key.eval(eval, ciphertext, len);
                        if (accept(newScore, currScore, temp)) {
                            currScore = newScore;
                            changed = true;
                            action = Action.IandK;
                            if (newScore > bestScore) {
                                bestScore = newScore;
                                bestStb = key.stbString();
                            }
                        }


                        key.stbConnect(vsi, vsk);
                        newScore = key.eval(eval, ciphertext, len);
                        if (accept(newScore, currScore, temp)) {
                            currScore = newScore;
                            changed = true;
                            action = Action.IandK_SIandSK;
                            if (newScore > bestScore) {
                                bestScore = newScore;
                                bestStb = key.stbString();
                            }
                        }
                        key.stbDisconnect(vsi, vsk);


                        key.stbDisconnect(vi, vk);
                        // all Self now
                        key.stbConnect(vi, vsk);
                        newScore = key.eval(eval, ciphertext, len);
                        if (accept(newScore, currScore, temp)) {
                            currScore = newScore;
                            changed = true;
                            action = Action.IandSK;
                            if (newScore > bestScore) {
                                bestScore = newScore;
                                bestStb = key.stbString();
                            }
                        }


                        key.stbConnect(vsi, vk);
                        newScore = key.eval(eval, ciphertext, len);
                        if (accept(newScore, currScore, temp)) {
                            currScore = newScore;
                            changed = true;
                            action = Action.IandSK_KandSI;
                            if (newScore > bestScore) {
                                bestScore = newScore;
                                bestStb = key.stbString();
                            }
                        }
                        key.stbDisconnect(vi, vsk);
                        key.stbDisconnect(vsi, vk);

                        // all Self now

                        switch (action) {
                            case IandK:
                                key.stbConnect(vi, vk);
                                break;
                            case IandSK:
                                key.stbConnect(vi, vsk);
                                break;
                            case SIandSK:
                                key.stbConnect(vsi, vsk);
                                break;
                            case IandK_SIandSK:
                                key.stbConnect(vi, vk);
                                key.stbConnect(vsi, vsk);
                                break;
                            case IandSK_KandSI:
                                key.stbConnect(vi, vsk);
                                key.stbConnect(vsi, vk);
                                break;
                            case NO_CHANGE:
                                key.stbConnect(vi, vsi);
                                key.stbConnect(vk, vsk);
                                break;
                            default:
                                throw new RuntimeException("Impossible change " + action);
                        }
                    }
                }
            }
            if (!changed) {
                roundsWithoutChange++;
            } else {
                roundsWithoutChange = 0;
            }
        }

        if (key.eval(eval, ciphertext, len) != currScore) {
            throw new RuntimeException("Best result is not consistent");
        }

        key.setStecker(bestStb);

        HCStep(eval);
        key.score = key.eval(eval, ciphertext, len);

    }
    private static Random random = new Random();
    private static boolean accept(long newScore, long currLocalScore, double temperature) {

        long diffScore = newScore - currLocalScore;
        if (diffScore > 0) {
            return true;
        }
        if (temperature == 0.0) {
            return false;
        }
        double prob = Math.pow(Math.E, diffScore / temperature);
        return prob > 0.0085 && prob > random.nextFloat();
    }


    private static void test(int len, int garbles, boolean generateX, int trials, Mode mode, int rounds, boolean badSearchKey) {


        class Round {
            Key key;
            HcSaRunnable process;
            byte[] ciphertext;
            byte[] plaintext;
            int len;
            Round(int len, Key from, Key to, boolean generateX, int garbles, Mode mode, int rounds, boolean badSearchKey) {
                this.len = len;
                process = new HcSaRunnable();
                ciphertext = new byte[len];
                plaintext = new byte[len];
                key = new Key();
                Utils.loadRandomText("faust.txt", plaintext, len, generateX, garbles);
                key.initRandom(from, to, 10);
                key.encipherDecipherAll(plaintext, ciphertext, len);
                key.score = key.triScoreWithoutLookupBuild(ciphertext, len);
                if (badSearchKey) {
                    key.initRandom(from, to, 10);
                }
                process.setup(key, key.stbrett, ciphertext, len, false, mode, rounds, 0);
            }
            boolean check(int pass, boolean print) {
                String s1 = process.key.plaintextString(ciphertext, len);
                String s2 = Utils.getString(plaintext, len);

                int errors = 0;
                for (int i = 0; i < len; i++) {
                    if (s1.charAt(i) != s2.charAt(i)) {
                        errors++;
                    }
                }

                if (print) {
                    System.out.printf("Expected: %,8d Found: %,8d Errors: %,2d %s  \n", key.score, process.key.score, errors, process.key.score > key.score ? "!!!!!" : "");
                    if (errors < len / 3) {
                        if (errors > 0) {
                            System.out.printf("%,8d %-22s %s\n", key.score, key.stbString(), s2);
                            System.out.printf("%,8d %-22s %s\n", process.key.score, process.key.stbString(), s1);
                        }

                        System.out.printf("FOUND %d %s\n", (pass + 1), new SimpleDateFormat("kk:mm:ss").format(new Date()));
                        key.printKeyString("");
                        System.out.printf("\n%s\n\n", process.key.plaintextString(ciphertext, len));

                        return true;
                    }
                }

                return errors < len / 3;

            }
        }

        Key from = new Key();
        Key to = new Key();
        Key.setRange(from, to, "B:111:AAA:AAA", "B:555:ZZZ:ZZZ", Key.Model.H);

        Key.resetCounter();

        Round[] r = new Round[trials];

        int ok = 0;

        Runnables runnables = new Runnables();
        for (int trial = 0; trial < trials; trial++) {
            r[trial] = new Round(len, from, to, generateX, garbles, mode, rounds, badSearchKey);
            runnables.addRunnable(r[trial].process);
        }

        long startTime = System.currentTimeMillis();
        runnables.run(7);
        long elapsed = System.currentTimeMillis() - startTime;

        for (int trial = 0; trial < trials; trial++) {
            ok += r[trial].check(trial, false) ? 1 : 0;
        }

        if (badSearchKey) {

            System.out.printf("%,3d - %,2d%%, %s (%,2d),                      Rate: %,5d/sec,  Elapsed: %.1f sec, Evals: %,d \n",
                    len, garbles, mode, rounds, 1000 * trials / elapsed, elapsed / 1000.0, Key.getCounter());
        } else {
            System.out.printf("%,3d - %,2d%%, %s (%,2d), Solved: %4.1f%%\n",
                    len, garbles, mode, rounds, ok * 100.0 / trials);

        }

    }
    static enum Mode {EStecker, HC, SA}

    public static void main(String[] args) {
        String bigramFile = "enigma_logbigrams.txt";
        //String trigramFile = "3WH.txt";
        String trigramFile = "enigma_logtrigrams.txt";
        int res = Stats.loadTridict(trigramFile);
        if (res != 1) {
            System.out.print("Load Log TriGram File Failed\n");
            return;
        }
        res = Stats.loadBidict(bigramFile);
        if (res != 1) {
            System.out.print("Load Log BiGram File Failed\n");
            return;
        }
        /*
         */
        for (int len : new int[]{75}) {
            for (Mode mode : new Mode[]{Mode.SA}) {
                for (int rounds : new int[]{1}) {

                    for (int garbles : new int[]{15, 5}) {
                        if (garbles == 15) {
                            test(len, garbles, true, 1000, mode, rounds, true);
                        }
                        test(len, garbles, true, 1000, mode, rounds, false);
                    }
                }
            }
        }
    }
    /*

 for (int len : new int[]{30, 50, 75}) {
            for (Mode mode : new Mode[]{Mode.SA}) {
                for (int rounds : new int[]{1, 5, 10}) {

                    for (int garbles : new int[]{15, 5}) {
                        if (garbles == 15) {
                            test(len, garbles, true, 1000, mode, rounds, true);
                        }
                        test(len, garbles, true, 1000, mode, rounds, false);
                    }
                }
            }
        }

 30 - 15%, SA ( 1),                      Rate:   110/sec,  Elapsed: 9.0 sec, Evals: 61,214,456
 30 - 15%, SA ( 1), Solved:  0.0%
 30 -  5%, SA ( 1), Solved:  0.7%
 30 - 15%, SA ( 2),                      Rate:    67/sec,  Elapsed: 14.9 sec, Evals: 104,138,023
 30 - 15%, SA ( 2), Solved:  0.5%
 30 -  5%, SA ( 2), Solved:  1.0%
 30 - 15%, SA ( 3),                      Rate:    44/sec,  Elapsed: 22.4 sec, Evals: 151,787,156
 30 - 15%, SA ( 3), Solved:  0.7%
 30 -  5%, SA ( 3), Solved:  1.2%
 30 - 15%, SA ( 5),                      Rate:    29/sec,  Elapsed: 34.0 sec, Evals: 236,088,408
 30 - 15%, SA ( 5), Solved:  0.1%
 30 -  5%, SA ( 5), Solved:  1.0%
 30 - 15%, SA (10),                      Rate:    15/sec,  Elapsed: 65.5 sec, Evals: 443,626,363
 30 - 15%, SA (10), Solved:  0.3%
 30 -  5%, SA (10), Solved:  1.3%

 50 - 15%, SA ( 1),                      Rate:   101/sec,  Elapsed: 9.9 sec, Evals: 66,447,432
 50 - 15%, SA ( 1), Solved:  4.1%
 50 -  5%, SA ( 1), Solved:  5.4%
 50 - 15%, SA ( 2),                      Rate:    58/sec,  Elapsed: 17.0 sec, Evals: 115,889,580
 50 - 15%, SA ( 2), Solved:  4.0%
 50 -  5%, SA ( 2), Solved: 13.5%
 50 - 15%, SA ( 3),                      Rate:    41/sec,  Elapsed: 23.8 sec, Evals: 163,098,928
 50 - 15%, SA ( 3), Solved:  5.5%
 50 -  5%, SA ( 3), Solved: 16.9%
 50 - 15%, SA ( 5),                      Rate:    26/sec,  Elapsed: 38.3 sec, Evals: 260,841,115
 50 - 15%, SA ( 5), Solved:  5.4%
 50 -  5%, SA ( 5), Solved: 20.9%
 50 - 15%, SA (10),                      Rate:    13/sec,  Elapsed: 72.6 sec, Evals: 486,883,591
 50 - 15%, SA (10), Solved:  9.1%
 50 -  5%, SA (10), Solved: 28.9%

 75 - 15%, SA ( 1),                      Rate:    89/sec,  Elapsed: 11.2 sec, Evals: 74,349,276
 75 - 15%, SA ( 1), Solved: 21.4%
 75 -  5%, SA ( 1), Solved: 48.6%
 75 - 15%, SA ( 2),                      Rate:    55/sec,  Elapsed: 18.1 sec, Evals: 125,011,973
 75 - 15%, SA ( 2), Solved: 27.0%
 75 -  5%, SA ( 2), Solved: 59.6%
 75 - 15%, SA ( 3),                      Rate:    38/sec,  Elapsed: 25.7 sec, Evals: 177,085,365
 75 - 15%, SA ( 3), Solved: 33.6%
 75 -  5%, SA ( 3), Solved: 65.2%
 75 - 15%, SA ( 5),                      Rate:    20/sec,  Elapsed: 47.7 sec, Evals: 294,626,078
 75 - 15%, SA ( 5), Solved: 37.0%
 75 -  5%, SA ( 5), Solved: 71.2%
 75 - 15%, SA (10),                      Rate:    10/sec,  Elapsed: 93.4 sec, Evals: 569,941,751
 75 - 15%, SA (10), Solved: 39.8%
 75 -  5%, SA (10), Solved: 78.9%

     */

/*
        for (int len : new int[] {30, 50, 75, 100, 125, 150, 200}) {
            for (Mode mode : new Mode[]{Mode.HC, Mode.SA}) {
                for (int garbles : new int[]{15, 5}) {
                    if (garbles == 15) {
                        test(len, garbles, true, 1000, mode, 1, true);
                    }
                    test(len, garbles, true, 1000, mode, 1, false);
                }
            }
        }
30 - 15%, HC ( 1),                      Rate: 1,893/sec,  Elapsed: 0.5 sec, Evals: 3,148,447
30 - 15%, HC ( 1), Solved:  0.0%
30 -  5%, HC ( 1), Solved:  0.0%
30 - 15%, SA ( 1),                      Rate:   114/sec,  Elapsed: 8.7 sec, Evals: 62,504,122
30 - 15%, SA ( 1), Solved:  0.4%
30 -  5%, SA ( 1), Solved:  0.6%

50 - 15%, HC ( 1),                      Rate: 3,802/sec,  Elapsed: 0.3 sec, Evals: 1,527,660
50 - 15%, HC ( 1), Solved:  0.3%
50 -  5%, HC ( 1), Solved:  2.0%
50 - 15%, SA ( 1),                      Rate:   112/sec,  Elapsed: 8.9 sec, Evals: 67,472,231
50 - 15%, SA ( 1), Solved:  2.4%
50 -  5%, SA ( 1), Solved:  8.0%

75 - 15%, HC ( 1),                      Rate: 3,401/sec,  Elapsed: 0.3 sec, Evals: 1,783,797
75 - 15%, HC ( 1), Solved:  3.2%
75 -  5%, HC ( 1), Solved:  9.2%
75 - 15%, SA ( 1),                      Rate:   107/sec,  Elapsed: 9.3 sec, Evals: 72,314,575
75 - 15%, SA ( 1), Solved: 20.3%
75 -  5%, SA ( 1), Solved: 46.6%

100 - 15%, HC ( 1),                      Rate: 3,030/sec,  Elapsed: 0.3 sec, Evals: 1,988,652
100 - 15%, HC ( 1), Solved: 12.6%
100 -  5%, HC ( 1), Solved: 28.0%
100 - 15%, SA ( 1),                      Rate:   107/sec,  Elapsed: 9.3 sec, Evals: 74,584,477
100 - 15%, SA ( 1), Solved: 45.0%
100 -  5%, SA ( 1), Solved: 80.0%

125 - 15%, HC ( 1),                      Rate: 2,932/sec,  Elapsed: 0.3 sec, Evals: 2,407,764
125 - 15%, HC ( 1), Solved: 26.2%
125 -  5%, HC ( 1), Solved: 52.0%
125 - 15%, SA ( 1),                      Rate:   105/sec,  Elapsed: 9.5 sec, Evals: 74,938,333
125 - 15%, SA ( 1), Solved: 65.0%
125 -  5%, SA ( 1), Solved: 92.0%

150 - 15%, HC ( 1),                      Rate: 2,717/sec,  Elapsed: 0.4 sec, Evals: 2,978,049
150 - 15%, HC ( 1), Solved: 45.8%
150 -  5%, HC ( 1), Solved: 70.8%
150 - 15%, SA ( 1),                      Rate:   107/sec,  Elapsed: 9.3 sec, Evals: 73,430,269
150 - 15%, SA ( 1), Solved: 73.9%
150 -  5%, SA ( 1), Solved: 96.8%

200 - 15%, HC ( 1),                      Rate: 2,298/sec,  Elapsed: 0.4 sec, Evals: 3,739,689
200 - 15%, HC ( 1), Solved: 76.6%
200 -  5%, HC ( 1), Solved: 90.8%
200 - 15%, SA ( 1),                      Rate:   106/sec,  Elapsed: 9.4 sec, Evals: 70,473,191
200 - 15%, SA ( 1), Solved: 86.5%
200 -  5%, SA ( 1), Solved: 99.1%

 */
}


