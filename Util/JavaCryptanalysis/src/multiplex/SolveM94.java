package multiplex;

import common.CtBestList;
import common.CtAPI;
import common.Runnables;
import common.Utils;

import java.util.ArrayList;
import java.util.concurrent.atomic.AtomicInteger;

class SolveM94 {


    static long saKnownOffsets(M94 m94, long realMultiplexScore, int saCycles, boolean displayProgress) {

        long bestScore = 0;
        for (int saCycle = 0; saCycle < saCycles || saCycles == 0; saCycle++) {
            bestScore = SolveMultiplex.simulatedAnnealingCycle(m94, realMultiplexScore, 200, 2_000, saCycle);
            if (displayProgress) {
                CtAPI.updateProgress(saCycle, saCycles);
            }
        }

        return bestScore;
    }

    static void saUnknownOffsets(final int[] cipher, final String cribStr, final int saCycles, final int internalSaCycles, final int threads, final long realMultiplexScore) {
        if (cipher.length > 75) {
            CtAPI.goodbyeFatalError("With M94 and unknown offsets, search works only for ciphertext with exactly 75 symbols)");
        }
        final AtomicInteger countOffsetsChecked = new AtomicInteger();
        final Runnables runnables = new Runnables();
        for (int offset0_ = 0; offset0_ < 26; offset0_++) {
            final int offset0 = offset0_;
            runnables.addRunnable(() -> {
                long bestScore = 0;
                M94 m94 = new M94(3);
                m94.setCipherAndCrib(cipher, cribStr);
                for (int offset1 = 0; offset1 < 26; offset1++) {
                    for (int offset2 = 0; offset2 < 26; offset2++) {
                        m94.setOffset(0, offset0).setOffset(1, offset1).setOffset(2, offset2);
                        bestScore = Math.max(bestScore, saKnownOffsets(m94, realMultiplexScore, internalSaCycles, false));
                    }
                }
                CtAPI.printf("Completed offsets starting with %s, best score: %,8d \n", m94.offsetString(0) + ",..,.. ", bestScore);
                synchronized (countOffsetsChecked) {
                    CtAPI.updateProgress(countOffsetsChecked.incrementAndGet(), 25);
                }

            });
        }


        for (int saCycle = 0; (saCycle < saCycles) || (saCycles == 0); saCycle++) {
            countOffsetsChecked.set(0);
            CtAPI.updateProgress(countOffsetsChecked.intValue(), 25);
            runnables.run(threads);
        }
    }


    static void solveKnownOffsets(String cipherStr, String cribStr, int saCycles, ArrayList<Integer> offsets) {

        M94 m94 = new M94(offsets.size());
        m94.setCipherAndCrib(Utils.getText(cipherStr), cribStr);

        int requiredOffsets = (cipherStr.length() + 24) / 25;
        if (offsets.size() < requiredOffsets) {
            CtAPI.goodbyeFatalError("Not enough known offsets - need " + requiredOffsets + " only got " + offsets.size());
        }
        for (int i = 0; i < offsets.size(); i++) {
            m94.setOffset(i, offsets.get(i));
        }

        saKnownOffsets(m94, -1, saCycles, true);

    }

    static void solveUnknownOffsets(String cipherStr, String cribStr, int saCycles, int internalSaCycles, int threads) {

        int[] c = Utils.getText(cipherStr);
        saUnknownOffsets(c, cribStr, saCycles, internalSaCycles, threads, -1);
    }

    static void solveSimulation(String bookFile, int len, boolean knownOffsets, int saCycles, int threads) {
        if (!knownOffsets && len != 75) {
            CtAPI.goodbyeFatalError("Length must be 75 when 'knownOffsets' is true");
        }

        int[] p = new int[len];
        int[] c = new int[len];
        Utils.readTextSegmentFromFile(bookFile, Utils.randomNextInt(50000), p);
        M94 encryptionKey = new M94(p.length / 25);
        encryptionKey.randomizeKey();
        encryptionKey.randomizeOffsets();

        encryptionKey.encrypt(p, c);
        encryptionKey.setCipher(c);
        long realMultiplexScore = encryptionKey.score();
        CtBestList.setOriginal(encryptionKey.score(), encryptionKey.toString(), encryptionKey.toString(), Utils.getString(encryptionKey.decryption), "Original");
        CtAPI.printf("Simulation with M94\n%s\n%s\n%s\nLength: %,d\nOriginal Score: %,d\nOffsets are %s\n",
                Utils.getString(p),
                Utils.getString(c), encryptionKey.toString(),
                len,
                realMultiplexScore,
                knownOffsets ? "known" : "unknown");

        if (knownOffsets) {
            M94 m94 = new M94(encryptionKey.offsets);
            m94.setCipher(c);
            saKnownOffsets(m94, realMultiplexScore, saCycles, true);
        } else {
            saUnknownOffsets(c, null, saCycles, 1, threads, realMultiplexScore);
        }
    }

            /*
        String[] mauborgne = {"VFDJL QMMJB HSYVJ KCJTJ WDKNI".replaceAll(" ",""),
                "CGNJM ZVKQC JPRJR CGOXG UCZVC ".replaceAll(" ",""),
                "CSTDT SSDJN JDKKT IXVEX VHDVK ".replaceAll(" ",""),
                "OZBGF VTUEC UGTZD KYWJR VZSDG ".replaceAll(" ",""),
                "QIRMB FTKBY CGAQV DQCVQ AHZGY ".replaceAll(" ",""),
                "VQWRM IHDHB RQBWU LKJCS KEYUU ".replaceAll(" ",""),
                "SSEIQ DWHNH QHGIK HAADN GNFBY ".replaceAll(" ",""),
                "VXDVX NIGJO PCOTN GKWAX YTNWL ".replaceAll(" ",""),
                "QJRLH AWTWU CYXVM BGJCR SBHWF ".replaceAll(" ",""),
                "DULPK UXMVL XFUPS ULRZK PDALY ".replaceAll(" ",""),
                "DCAIY LUPMB NACQE OPTLH KKRGT ".replaceAll(" ",""),
                "MGODT VGUYX NHKBE WPOUR VTQOE ".replaceAll(" ",""),
                "TBVEB QDXGP LCPUY AVVBK ZEOZY ".replaceAll(" ",""),
                "FIJDW WBKTY GBSMB PZWYP RRZCW ".replaceAll(" ",""),
                "DYVPJ CLNXE SCMF0 YPIZF PEBHM ".replaceAll(" ",""),
                "MYYTJ RFMEP PHDXP ODFZO WLGLA ".replaceAll(" ",""),
                "EYKKD XHTEV TRXWK CJPSG MASCY ".replaceAll(" ",""),
                "LGQLV HTUIP YAUGJ PGDLH UZTKV ".replaceAll(" ",""),
                "BRKTJ RGGTB HMLXX FRHOA AZVWU ".replaceAll(" ",""),
                "CDUDV DBZUA ELRPO SPUJD XRZWA ".replaceAll(" ",""),
                "EUFBT TWNIY HHTNW QNFVE NYGBY ".replaceAll(" ",""),
                "TUTVY NGLPG TYOLI HXZQT XSGOJ ".replaceAll(" ",""),
                "PBTJC CJONJ UNIXB UAQBI WNIHL ".replaceAll(" ",""),
                "VHNKR XVZMD KFHUY XRNDD KXXVM ".replaceAll(" ",""),
                "NNHBF VQH0B LXCYM AKFLS SSJXG".replaceAll(" ","")};
        String plotz1M94 = "JUTHGFFHJTEUONGWZLIZAGOPIILLGZWCYPQNDZNICSWEILYSUALYRMEGKBUPUZCOSBCPIMSMRDW";


        solveUnknownOffsets(mauborgne[2] + mauborgne[6] + mauborgne[4], 3);
        solveUnknownOffsets(plotz1M94, 1);


        */

}
