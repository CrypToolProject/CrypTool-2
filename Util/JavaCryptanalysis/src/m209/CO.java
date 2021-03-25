package m209;

import common.*;

public class CO {

    private static void solve(int task, int[] roundLayers, int layers, Key key, int cycles, boolean searchSlide) {

        int[] bestLocalTypeCount = key.lugs.createCopy();
        boolean[][] bestLocalPins = key.pins.createCopy();

        if (cycles == 0) {
            cycles = Integer.MAX_VALUE;
        }

        for (int cycle = 0; cycle < cycles; cycle++) {
            roundLayers[layers] = cycle;
            int bestRandom = Integer.MIN_VALUE;

            if (searchSlide) {
                key.slide = cycle % 26;
                key.invalidateDecryption();
            }
            int bestLocal = 0;
            int phase1Trials = 200000 / key.cipherArray.length;
            for (int r = 0; r < phase1Trials; r++) {
                roundLayers[layers+1] = r;
                key.lugs.randomize();

                int newEval = SimulatedAnnealingPins.SA(task, roundLayers, layers + 2, key, EvalType.MONO, 1);
                if (newEval > bestRandom) {
                    bestRandom = newEval;
                    key.lugs.get(bestLocalTypeCount);
                    key.pins.get(bestLocalPins);
                }
                if (bestRandom > bestLocal) {
                    bestLocal = bestRandom;
                    ReportResult.reportResult(task, roundLayers, layers + 2, key, bestLocal,"HC L. rn");
                }
            }
            key.lugs.set(bestLocalTypeCount, false);
            key.pins.set(bestLocalPins);
            bestLocal = bestRandom;

            int round = 0;
            boolean improved;
            do {
                roundLayers[layers + 1] = round;
                round++;
                improved = false;
                int[] types = new int[4];

                int newEval;

                for (int c1 : Lugs.TYPES) {
                    for (int c2 : Lugs.TYPES) {
                        if (c1 >= c2) {
                            continue;
                        }
                        types[0] = c1;
                        types[1] = c2;
                        for (int[] changes : HillClimbLugs.changes2Types) {

                            if (!HillClimbLugs.doChangesIfValid(bestLocalTypeCount, types, changes)) {
                                continue;
                            }
                            if (!key.lugs.set(bestLocalTypeCount, false)) {
                                HillClimbLugs.undoChanges(bestLocalTypeCount, types, changes);
                                continue;
                            }
                            newEval = SimulatedAnnealingPins.SA(task, roundLayers, layers + 2, key, EvalType.MONO, 1);
                            if (newEval > bestLocal) {
                                improved = true;
                                bestLocal = newEval;
                                key.pins.get(bestLocalPins);
                                ReportResult.reportResult(task, roundLayers, layers + 2, key, bestLocal, "HC L. s" + changes.length);
                            } else {
                                HillClimbLugs.undoChanges(bestLocalTypeCount, types, changes);
                                key.lugs.set(bestLocalTypeCount, false);
                                key.pins.set(bestLocalPins);
                            }
                        }
                    }
                }

                for (int i1 = 0; i1 < Lugs.TYPES.length && !improved; i1++) {
                    types[0] = Lugs.TYPES[i1];
                    for (int i2 = i1 + 1; i2 < Lugs.TYPES.length && !improved; i2++) {
                        types[1] = Lugs.TYPES[i2];
                        for (int[] changes : HillClimbLugs.changes2Types) {
                            if (!HillClimbLugs.doChangesIfValid(bestLocalTypeCount, types, changes)) {
                                continue;
                            }
                            if (!key.lugs.set(bestLocalTypeCount, false)) {
                                HillClimbLugs.undoChanges(bestLocalTypeCount, types, changes);
                                continue;
                            }
                            newEval = key.eval(EvalType.MONO);

                            if (newEval > bestLocal) {
                                improved = true;
                                bestLocal = newEval;
                                ReportResult.reportResult(task, roundLayers, layers + 2, key, bestLocal, "HCL. s" + changes.length);
                                key.pins.get(bestLocalPins);
                                break;
                            } else {
                                HillClimbLugs.undoChanges(bestLocalTypeCount, types, changes);
                                key.lugs.set(bestLocalTypeCount, false);
                            }
                        }
                    }

                }
                if (!improved) {
                    for (int i1 = 0; i1 < Lugs.TYPES.length && !improved; i1++) {
                        types[0] = Lugs.TYPES[i1];
                        for (int i2 = i1 + 1; i2 < Lugs.TYPES.length && !improved; i2++) {
                            types[1] = Lugs.TYPES[i2];
                            for (int i3 = i2 + 1; i3 < Lugs.TYPES.length && !improved; i3++) {
                                types[2] = Lugs.TYPES[i3];
                                for (int i4 = i3 + 1; i4 < Lugs.TYPES.length && !improved; i4++) {
                                    types[3] = Lugs.TYPES[i4];

                                    for (int[] changes : HillClimbLugs.changes4Types) {
                                        if (!HillClimbLugs.doChangesIfValid(bestLocalTypeCount, types, changes)) {
                                            continue;
                                        }
                                        if (!key.lugs.set(bestLocalTypeCount, false)) {
                                            HillClimbLugs.undoChanges(bestLocalTypeCount, types, changes);
                                            continue;
                                        }
                                        newEval = key.eval(EvalType.MONO);

                                        if (newEval > bestLocal) {
                                            improved = true;
                                            bestLocal = newEval;
                                            ReportResult.reportResult(task, roundLayers, layers + 2, key, bestLocal, "HC L. s" + changes.length);
                                            key.pins.get(bestLocalPins);
                                            break;
                                        } else {
                                            HillClimbLugs.undoChanges(bestLocalTypeCount, types, changes);
                                            key.lugs.set(bestLocalTypeCount, false);
                                        }
                                    }
                                }
                            }
                        }
                    }


                    for (int i1 = 0; i1 < Lugs.TYPES.length && !improved; i1++) {
                        types[0] = Lugs.TYPES[i1];
                        for (int i2 = i1 + 1; i2 < Lugs.TYPES.length && !improved; i2++) {
                            types[1] = Lugs.TYPES[i2];
                            for (int i3 = i2 + 1; i3 < Lugs.TYPES.length && !improved; i3++) {
                                types[2] = Lugs.TYPES[i3];

                                for (int[] changes : HillClimbLugs.changes3Types) {
                                    if (!HillClimbLugs.doChangesIfValid(bestLocalTypeCount, types, changes)) {
                                        continue;
                                    }
                                    if (!key.lugs.set(bestLocalTypeCount, false)) {
                                        HillClimbLugs.undoChanges(bestLocalTypeCount, types, changes);
                                        continue;
                                    }
                                    newEval = key.eval(EvalType.MONO);

                                    if (newEval > bestLocal) {
                                        improved = true;
                                        bestLocal = newEval;
                                        ReportResult.reportResult(task, roundLayers, layers + 2, key, bestLocal, "HC L. s" + changes.length);
                                        key.pins.get(bestLocalPins);
                                        break;
                                    } else {
                                        HillClimbLugs.undoChanges(bestLocalTypeCount, types, changes);
                                        key.lugs.set(bestLocalTypeCount, false);
                                    }
                                }
                            }
                        }
                    }

                }

            } while (improved);

        }


    }

    public static void solveMultithreaded(String resourceDir, Language language, String cipher, Key simulationKey, int cycles, int threads) {
        Stats.load(resourceDir, language, true);

        ReportResult.setThreshold(EvalType.MONO);

        if (cipher == null || cipher.isEmpty()) {
            CtAPI.goodbyeFatalError("cipher is empty");
        }
        if (simulationKey != null) {
            ReportResult.setOriginalKey(simulationKey, EvalType.MONO);
        }
        Runnables runnables = new Runnables();
        for (int i = 0; i < threads; i++) {
            final int[] roundLayers = new int[4];
            final int task = i;
            final Key key = new Key();
            key.setCipher(cipher);
            if (simulationKey != null) {
                key.setOriginalKey(simulationKey);
                key.setOriginalScore(simulationKey.eval(EvalType.MONO));
            }

            runnables.addRunnable(() -> solve(task, roundLayers, 0, key, cycles, false));
        }
        runnables.run(threads);
    }


}


