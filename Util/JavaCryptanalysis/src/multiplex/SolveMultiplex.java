package multiplex;

import common.*;

class SolveMultiplex {

    static long simulatedAnnealingCycle(Multiplex multiplex, long realMultiplexScore, int maxRounds, int multiplier, int saCycle) {

        long bestScore = 0;

        long currentScore = multiplex.randomizeKey().score();

        for (int round = 0; round < maxRounds; round++) {
            int randomShift = Utils.randomNextInt(multiplex.NUMBER_OF_STRIPS);
            for (int i = 0; i < multiplex.NUMBER_OF_STRIPS; i++) {
                int pi = (randomShift + i) % multiplex.NUMBER_OF_STRIPS;
                for (int j = i + 1; j < multiplex.NUMBER_OF_STRIPS; j++) {
                    int pj = (randomShift + j) % multiplex.NUMBER_OF_STRIPS;

                    // Skip if both strips are unused.
                    if (pi >= multiplex.NUMBER_OF_STRIPS_USED_IN_KEY && pj >= multiplex.NUMBER_OF_STRIPS_USED_IN_KEY) {
                        continue;
                    }
                    long newScore = multiplex.swapInKey(pi, pj).score();

                    if (SimulatedAnnealing.acceptHexaScore(newScore, currentScore, multiplier)) {
                        currentScore = newScore;
                        if (currentScore > bestScore) {
                            bestScore = currentScore;
                        }
                        if (CtBestList.shouldPushResult(newScore)) {
                            CtBestList.pushResult(newScore,
                                    multiplex.toString(),
                                    multiplex.toString(),
                                    Utils.getString(multiplex.decryption),
                                    Stats.evaluationsSummary() +
                                             String.format("[SA Cycle: %,5d, Round: %,5d]", saCycle, round));
                            if (currentScore == realMultiplexScore || multiplex.matchesFullCrib()) {
                                CtAPI.printf("Key found");
                                CtAPI.goodbye();
                            }
                        }
                    } else {
                        multiplex.swapInKey(pi, pj);
                    }
                }
            }
        }
        return bestScore;
    }
}
