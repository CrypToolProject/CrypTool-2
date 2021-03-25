package multiplex;


import common.CtBestList;
import common.CtAPI;
import common.Utils;

import java.util.ArrayList;

class SolveM138 {


    static long sa(M138 m138, long realMultiplexScore, boolean offsetKnown, int saCycles) {
        long bestScore = 0;
        for (int saCycle = 0; saCycle < saCycles || saCycles == 0; saCycle++) {
            if (!offsetKnown) {
                m138.setOffset(saCycle % m138.STRIP_LENGTH);
            }
            bestScore = Math.max(bestScore, SolveMultiplex.simulatedAnnealingCycle(m138, realMultiplexScore, 120, 1_500, saCycle));
            CtAPI.updateProgress(saCycle, saCycles + 1);
        }
        return bestScore;
    }


    static void solve(ArrayList<Integer> offset, String cipherStr, String cribStr, int maxCycles, long realMultiplexScore) {

        if (offset.size() > 1) {
            CtAPI.goodbyeFatalError("Too many offsets for M138");
        }

        CtAPI.printf("Ciphertext: %s\n", cipherStr);
        if (cribStr != null && cribStr.length() > 0) {
            CtAPI.printf("Crib:       %s\n", cribStr);
        }

        M138 m138 = new M138();
        m138.setCipherAndCrib(Utils.getText(cipherStr), cribStr);
        if (!offset.isEmpty()) {
            m138.setOffset(offset.get(0));
            CtAPI.printf("Offset:     %d\n", offset.get(0));
        } else {
            CtAPI.println("Offset unknown");
        }

        sa(m138, realMultiplexScore, !offset.isEmpty(), maxCycles);
    }

    static void solveSimulation(String bookFile, int len, boolean offsetKnown, boolean withCrib) {
        int[] p = new int[len];
        Utils.readTextSegmentFromFile(bookFile, Utils.randomNextInt(50000), p);
        String cribString = withCrib ? Utils.getString(p) : null;

        int[] c = new int[len];
        M138 encryptionKey = new M138();
        encryptionKey.randomizeOffset().randomizeKey();
        encryptionKey.encrypt(p, c);
        encryptionKey.setCipherAndCrib(c, cribString);
        CtAPI.printf("Encryption key for simulation: %s\n", encryptionKey.toString());

        long realMultiplexScore = encryptionKey.score();
        CtBestList.setOriginal(realMultiplexScore, encryptionKey.toString(), encryptionKey.toString(), Utils.getString(encryptionKey.decryption), "Original");

        ArrayList<Integer> offset = new ArrayList<>();
        if (offsetKnown) {
            offset.add(encryptionKey.offset);
        }

        solve(offset, Utils.getString(c), cribString, 0, realMultiplexScore);
    }

    /**
     * Klaus Schmeh challenge 3
     */
    private static void challenge3M138(int len, boolean offsetKnown, boolean withCrib) {
        String cipherStr = "RIGVRXIXRHZQOGDQYIXVHCZKJLCDKUSGNDPPIBCLGPZBRUTRFJXHTNQPHWXGQAXPKEEEKMDPWFKSDTLKPTFIXIRUXNTIMTZQQCQOSOPFBXFMMZPSIGZSANJKYHWIO".substring(0, len);
        String cribStr = withCrib ? "INTHEEARLYNINETEENTWENTIESTHECABLEBECAMETHEFAVOUREDMEANSOFCOMMUNICATIONFORLOVERSSEPARATEDBYTHOUSANDSOFMILESITSEEMEDMIRACULOUS".substring(0, len) : null;
        int[] key = {51, 23, 15, 62, 14, 22, 39, 21, 99, 12, 19, 24, 4, 73, 6, 18, 85, 20, 11, 25, 42, 38, 8, 26, 9};
        ArrayList<Integer> offset = new ArrayList<>();
        if (offsetKnown) {
            offset.add(6);
        }

        solve(offset, cipherStr, cribStr, 0, -1);
    }

    /**
     * Klaus Schemh challenge 4
     *
     */
    private static void challenge4M138(boolean offsetKnown, boolean withCrib) {
        String cipherStr = "PTIJJHDJPKYTMTKUVEPDHYKLHDEYMGLIJLNWKXVGZILQNCJRHWJNBJFUAQHNBJGXWZBESXNXPZH";
        String cribStr = withCrib ? "CRYPTOGRAPHYPROVIDESMEANSFORSECURECOMMUNICATIONSINTHEPRESENCEOFTHIRDPARTIES" : null;
        int[] key = {79, 62, 66, 12, 18, 88, 27, 54, 91, 85, 72, 90, 76, 78, 36, 28, 30, 41, 48, 2, 8, 22, 59, 98, 33};
        //                 79,42!,66,12,18,88,27,54,91,60!,72,90,76,78,36,28,30,41,48,02,08,22,59,98,33

        ArrayList<Integer> offset = new ArrayList<>();
        if (offsetKnown) {
            offset.add(22);
        }

        solve(offset, cipherStr, cribStr, 0, -1);
    }
}
