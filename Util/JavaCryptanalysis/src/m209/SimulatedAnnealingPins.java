package m209;

import common.SimulatedAnnealing;

public class SimulatedAnnealingPins {

    private static int step(int task, int[] roundLayers, int layers, Key key, EvalType evalType,
                            boolean[][] bestSAPins, int bestSAScore,
                            double temperature) {


        int currLocalScore = 0;

        final int MAX_COUNT = key.pins.maxCount();
        final int MIN_COUNT = key.pins.minCount();

        int newScore;
        int count;

        boolean changeAccepted = false;

        for (int w = 1; w <= Key.WHEELS; w++) {
            for (int p = 0; p < Key.WHEELS_SIZE[w]; p++) {
                key.pins.toggle(w, p);
                count = key.pins.count();
                if (count < MIN_COUNT || count > MAX_COUNT || key.pins.longSeq(w, p)) {
                    key.pins.toggle(w, p);
                    continue;
                }
                key.updateDecryption(w, p);

                newScore = key.eval(evalType);
                if (SimulatedAnnealing.accept(newScore, currLocalScore, temperature)) {
                    currLocalScore = newScore;
                    changeAccepted = true;
                    if (newScore > bestSAScore) {
                        key.pins.get(bestSAPins);
                        bestSAScore = newScore;
                    }
                    ReportResult.reportResult(task, roundLayers, layers, key, currLocalScore, "SA P. x1");
                } else {
                    key.pins.toggle(w, p);
                    key.updateDecryption(w, p);
                }
            }
        }

        for (int w = 1; w <= Key.WHEELS; w++) {

            key.pins.inverse(w);
            count = key.pins.count();
            if (count < MIN_COUNT || count > MAX_COUNT) {
                key.pins.inverse(w);
                continue;
            }
            //key.updateDecryption();

            newScore = key.eval(evalType);
            if (SimulatedAnnealing.accept(newScore, currLocalScore, temperature)) {
                currLocalScore = newScore;
                changeAccepted = true;
                if (newScore > bestSAScore) {
                    key.pins.get(bestSAPins);
                    bestSAScore = newScore;
                }
                ReportResult.reportResult(task, roundLayers, layers, key, currLocalScore, "SA P. xw");

            } else {
                key.pins.inverse(w);
                //key.updateDecryption();
            }
        }

        if (changeAccepted) {
            return bestSAScore;
        }

        for (int w = 1; w <= Key.WHEELS; w++) {
            for (int p1 = 0; p1 < Key.WHEELS_SIZE[w]; p1++) {
                for (int p2 = p1 + 1; p2 < Key.WHEELS_SIZE[w]; p2++) {
                    if (key.pins.compare(w, p1, p2)) {
                        continue;
                    }
                    // Because the two places have a different value, we do not check the count.

                    key.pins.toggle(w, p1, p2);

                    if (key.pins.longSeq(w, p1, p2)) {
                        key.pins.toggle(w, p1, p2);
                        continue;
                    }
                    key.updateDecryption(w, p1, p2);

                    newScore = key.eval(evalType);

                    if (SimulatedAnnealing.accept(newScore, currLocalScore, temperature)) {
                        currLocalScore = newScore;
                        changeAccepted = true;
                        if (newScore > bestSAScore) {
                            key.pins.get(bestSAPins);
                            bestSAScore = newScore;
                        }
                        ReportResult.reportResult(task, roundLayers, layers, key, currLocalScore, "SA P. x2");
                    } else {
                        key.pins.toggle(w, p1, p2);
                        key.updateDecryption(w, p1, p2);
                    }

                }
            }
        }

        if (changeAccepted) {
            return bestSAScore;
        }
        int bestV = -1;
        int bestVscore = 0;
        for (int v = 0; v <= 63; v++) {
            key.pins.inverseWheelBitmap(v);
            int score = key.eval(evalType);
            if (score > bestVscore) {
                bestVscore = score;
                bestV = v;
            }
            key.pins.inverseWheelBitmap(v);
        }
        newScore = bestVscore;
        if (SimulatedAnnealing.accept(newScore, currLocalScore, temperature)) {
            currLocalScore = newScore;
            key.pins.inverseWheelBitmap(bestV);
            changeAccepted = true;
            if (newScore > bestSAScore) {
                key.pins.get(bestSAPins);
                bestSAScore = newScore;
            }
            ReportResult.reportResult(task, roundLayers, layers, key, currLocalScore, "SA P. xn");
        }

        return bestSAScore;

    }


    public static int SA(int task, int[] roundLayers, int layers, Key key, EvalType evalType, int cycles) {

        int bestSAScore = key.eval(evalType);
        boolean[][] bestSAPins = key.pins.createCopy();

        for (int cycle = 0; cycle < cycles; cycle++) {
            roundLayers[layers] = cycle;
            key.pins.randomize();

            roundLayers[layers + 1] = 0;
            for (double temperature = 1000.0; temperature >= 1.0; temperature /= 1.1, roundLayers[layers + 1]++) {
                bestSAScore = step(task, roundLayers, layers + 2, key, evalType, bestSAPins, bestSAScore, temperature);
            }
            long previous;
            do {
                previous = bestSAScore;
                bestSAScore = step(task, roundLayers, layers + 2, key, evalType, bestSAPins, bestSAScore, 0.0);
            } while (bestSAScore > previous);
        }

        key.pins.set(bestSAPins);
        return bestSAScore;

    }

}
