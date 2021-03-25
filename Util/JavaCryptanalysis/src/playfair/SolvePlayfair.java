package playfair;

import common.*;

import java.util.Random;

public class SolvePlayfair {

    static void solve(int taskNumber, int saCycles, int innerRounds /* 200000 */, int multiplier/*1500*/, int[] cipherText, String crib, Key simulationKey) {

        long simulationOriginalScore = (simulationKey == null) ? Long.MIN_VALUE : simulationKey.score;
        Key currentKey = new Key();
        Key newKey = new Key();
        Key bestKey = new Key();
        currentKey.setCipher(cipherText);
        newKey.setCipher(cipherText);
        bestKey.setCipher(cipherText);
        currentKey.setCrib(crib);
        newKey.setCrib(crib);
        bestKey.setCrib(crib);

        long serialCounter = 0;

        for (int cycle = 0; cycle < saCycles || saCycles == 0; cycle++) {

            if (taskNumber == 0) {
                CtAPI.updateProgress(cycle, saCycles);
            }

            Transformations.randomize();
            currentKey.random();

            long currentScore = currentKey.eval();

            bestKey.copy(currentKey);
            long bestScore = bestKey.eval();
            for (int innerRound = 0; innerRound < innerRounds; innerRound++) {
                Transformations.apply(currentKey, newKey, serialCounter++);

                long newScore = newKey.eval();

                if (SimulatedAnnealing.acceptHexaScore(newScore, currentScore, multiplier)) {
                    currentKey.copy(newKey);
                    currentScore = newScore;

                    if (currentScore > bestScore) {
//                        currentKey.decrypt();
//                        long n8 = NGrams.eval8(currentKey.decryptionRemoveNulls, currentKey.decryptionRemoveNullsLength);
//
//                        if (CtBestList.shouldPushResult(n8)) {
//                            CtBestList.pushResult(n8,
//                                    currentKey.toString(),
//                                    currentKey.toString(),
//                                    Utils.getString(currentKey.fullDecryption),
//                                    Stats.evaluationsSummary() +
//                                            String.format("[%d/%d}[Task: %2d][Mult.: %,d]",
//                                                    currentKey.decryptionRemoveNullsLength, cipherText.length, taskNumber, multiplier));
//
//                        }
//
                        bestScore = currentScore;
                        bestKey.copy(currentKey);
                        bestKey.decrypt();
                    }

                }
            }

            if (CtBestList.shouldPushResult(bestScore)) {
                bestKey.alignAlphabet();
                CtBestList.pushResult(bestScore,
                        bestKey.toString(),
                        bestKey.toString(),
                        Utils.getString(bestKey.fullDecryption),
                        Stats.evaluationsSummary() +
                                String.format("[%d/%d}[Task: %2d][Mult.: %,d]",
                                        bestKey.decryptionRemoveNullsLength, cipherText.length, taskNumber, multiplier));
                if (currentScore == simulationOriginalScore || newKey.matchesFullCrib()) {
                    CtAPI.printf("Key found");
                    CtAPI.goodbye();
                }
            }
        }
    }

    static void solveMultithreaded(int[] cipherText, String cribString, int threads, int cycles, Key simulationKey) {
        Random r = new Random();
        final Key simulationKey_ = simulationKey;
        for (int t_ = 0; t_ < threads; t_++) {
            final int t = t_;
            final double factor = (cribString.length() > cipherText.length/2) ? 0.1 : 1.0;
            final int multiplier = (int) (factor* 150_000)/cipherText.length;

            new Thread(
                    () -> solve(t, cycles, 200_000, multiplier, cipherText, cribString, simulationKey_)
            ).start();
        }
    }
}
