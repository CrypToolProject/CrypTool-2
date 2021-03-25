package m209;

import common.CtAPI;
import common.Runnables;

public class KP {

    private static int randomizePinsAndLugs(Key key, boolean lugs, boolean pins, boolean searchSlide, int randomCycles, int primaryCycle) {

        int bestLocal = Integer.MIN_VALUE;
        boolean[][] bestLocalPins = key.pins.createCopy();
        int[] bestLocalLugsTypeCount = key.lugs.createCopy();

        if (searchSlide) {
            if (key.slide != primaryCycle % 26) {
                key.slide = primaryCycle % 26;
                key.invalidateDecryption();
            }
        }

        for (int r = 0; r < randomCycles; r++) {
            if (lugs) {
                key.lugs.randomize();
            }
            if (pins) {
                key.pins.randomize();
            }
            if (!lugs && !pins) {
                key.pins.randomize(common.Utils.randomNextInt(Key.WHEELS) + 1);
            }
            int newEval = key.eval(EvalType.CRIB);
            if (newEval > bestLocal) {
                bestLocal = newEval;
                key.lugs.get(bestLocalLugsTypeCount);
                key.pins.get(bestLocalPins);
            }
        }

        key.lugs.set(bestLocalLugsTypeCount, false);
        key.pins.set(bestLocalPins);

        return bestLocal;
    }

    private static void solve(int task, int[] roundLayers, int layers, Key key, int cycles, boolean slide) {

        final int RANDOM_CYCLES = 100;

        if (cycles == 0) {
            cycles = Integer.MAX_VALUE;
        }

        int old;


        for (int cycle = 0; cycle < cycles; cycle++) {
            roundLayers[layers] = cycle;

            int semiRestarts = 8;

            int newEval = randomizePinsAndLugs(key, true, true, slide, RANDOM_CYCLES, cycle);

            boolean improved;
            do {
                roundLayers[layers + 1] = semiRestarts;

                improved = false;

                old = newEval;
                newEval = HillClimbLugs.hillClimb(task, roundLayers, layers + 2, key, EvalType.CRIB, cycle, false, false);
                if (newEval > old) {
                    improved = true;
                }

                newEval = SimulatedAnnealingPins.SA(task, roundLayers, layers + 2, key, EvalType.CRIB, newEval > 128000 ? 20 : 5);
                newEval = HillClimbPins.hillClimb(task, roundLayers, layers + 2, key, EvalType.CRIB, false);
                if (newEval > old) {
                    improved = true;
                }

                if (improved) {
                    semiRestarts = 8;
                }

                if (!improved && semiRestarts > 0) {
                    int last = key.eval(EvalType.CRIB);

                    if (last >= 129000 && last < key.originalScore) {
                        int deepCycles = 0;
                        do {

                            improved = false;
                            newEval = HillClimbLugs.hillClimb(task, roundLayers, layers + 2, key, EvalType.PINS_SA_CRIB, cycle, true, false); // Single iteration, print
                            if (newEval > last) {
                                last = newEval;
                                improved = true;
                            }
                            deepCycles++;

                        } while (improved || deepCycles < 3);


                        semiRestarts /= 2;

                    } else {

                        if ((semiRestarts % 8) != 7) {
                            randomizePinsAndLugs(key, true, false, slide, RANDOM_CYCLES, cycle);
                            newEval = HillClimbPins.hillClimb(task, roundLayers, layers + 2, key, EvalType.CRIB, true);
                        } else {
                            randomizePinsAndLugs(key, false, true, slide, RANDOM_CYCLES, cycle);
                            newEval = HillClimbLugs.hillClimb(task, roundLayers, layers + 2, key, EvalType.CRIB, cycle, true, true); // Single iteration, print
                        }

                        improved = true;
                        semiRestarts--;

                    }
                }
            } while (improved);
        }

    }
    public static void solveMultithreaded(String cipher, String crib, Key simulationKey, int cycles, int threads) {

        ReportResult.knownPlaintext = true;

        if (cipher == null || cipher.isEmpty()) {
            CtAPI.goodbyeFatalError("cipher is empty");
        }
        if (crib == null || crib.isEmpty()) {
            CtAPI.goodbyeFatalError("crib is empty");
        }
        ReportResult.setThreshold(EvalType.CRIB);

        if (simulationKey != null) {
            ReportResult.setOriginalKey(simulationKey, EvalType.CRIB);
        } else {
            ReportResult.setDummyOriginalKeyForCrib(crib);
        }
        Runnables runnables = new Runnables();
        for (int i = 0; i < threads; i++) {
            final int task = i;
            final int[] roundLayers = new int[4];
            final Key key = new Key();
            key.setCipherAndCrib(cipher, crib);
            if (simulationKey != null) {
                key.setOriginalKey(simulationKey);
            }
            key.setOriginalScore(130000);

            runnables.addRunnable(() -> solve(task, roundLayers, 0, key, cycles, false));
        }
        runnables.run(threads);
    }
}
