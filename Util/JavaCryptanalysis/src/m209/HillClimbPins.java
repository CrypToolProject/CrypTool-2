package m209;

public class HillClimbPins {


    public static int hillClimb(int task, int[] roundLayers, int layers, Key key, EvalType evalType, boolean singleIteration) {

        int bestLocal = key.eval(evalType);
        boolean[][] bestLocalPins = key.pins.createCopy();

        final int MAX_COUNT = key.pins.maxCount();
        final int MIN_COUNT = key.pins.minCount();

        int newEval;
        int count;

        int round = 0;
        boolean improved;
        do {
            roundLayers[layers] = round;
            improved = false;

            for (int w = 1; w <= Key.WHEELS; w++) {
                for (int p = 0; p < Key.WHEELS_SIZE[w]; p++) {
                    key.pins.toggle(w, p);
                    count = key.pins.count();
                    if (count <= MIN_COUNT || count >= MAX_COUNT || key.pins.longSeq(w, p)) {
                        key.pins.toggle(w, p);
                        continue;
                    }
                    key.updateDecryption(w, p);

                    newEval = key.eval(evalType);
                    if (newEval > bestLocal) {
                        bestLocal = newEval;
                        key.pins.get(bestLocalPins);
                        improved = true;
                        ReportResult.reportResult(task, roundLayers, layers + 1, key, bestLocal, "HC P. x1");
                    } else {
                        key.pins.toggle(w, p);
                        key.updateDecryption(w, p);
                    }
                }

                if (improved) {
                    w--;
                    improved = false;
                }
            }

            for (int w = 1; w <= Key.WHEELS; w++) {

                key.pins.inverse(w);
                count = key.pins.count();
                if (count <= MIN_COUNT || count >= MAX_COUNT) {
                    key.pins.inverse(w);
                    continue;
                }

                newEval = key.eval(evalType);
                if (newEval > bestLocal) {
                    bestLocal = newEval;
                    key.pins.get(bestLocalPins);
                    improved = true;
                    ReportResult.reportResult(task, roundLayers, layers + 1, key, bestLocal, "HC P. xw");
                } else {
                    key.pins.inverse(w);
                }
            }

            if (improved) {
                continue;
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

                        newEval = key.eval(evalType);

                        if (newEval > bestLocal) {
                            bestLocal = newEval;
                            key.pins.get(bestLocalPins);
                            improved = true;
                            ReportResult.reportResult(task, roundLayers, layers + 1, key, bestLocal, "HC P. x2");
                        } else {
                            key.pins.toggle(w, p1, p2);
                            key.updateDecryption(w, p1, p2);
                        }
                    }
                }
            }

            if (improved) {
                continue;
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
            if (bestVscore > bestLocal) {
                bestLocal = bestVscore;
                key.pins.inverseWheelBitmap(bestV);
                key.pins.get(bestLocalPins);
                improved = true;
                ReportResult.reportResult(task, roundLayers, layers + 1, key, bestLocal, "HC P. xn");
            }

            round++;
        } while (improved && !singleIteration);


        key.pins.set(bestLocalPins);
        return bestLocal;

    }

}
