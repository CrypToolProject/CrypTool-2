package m209;

public class HillClimbLugs {

    public static final int[][] changes4Types = {

            {-1, -1, 1, 1,},
            {-1, 1, -1, 1,},
            {-1, 1, 1, -1,},
            {1, -1, -1, 1,},
            {1, -1, 1, -1,},
            {1, 1, -1, -1,},

    };
    public static final int[][] changes3Types = {

            {-2, 1, 1,},
            {1, -2, 1,},
            {1, 1, -2, },

            {2, -1, -1,},
            {-1, 2, -1,},
            {-1, -1, 2, },

    };
    public static final int[][] changes2Types = {
            {-1, 1},
            {1, -1,},
            {-2, 2,},
            {2, -2,},
    };

     static int eval(int task, int[] roundLayers, int layers, Key key, EvalType evalType) {
        if (evalType == EvalType.PINS_SA_CRIB) {
            return SimulatedAnnealingPins.SA(task, roundLayers, layers, key, EvalType.CRIB, 10);
        }
        return key.eval(evalType);
    }

    public static int hillClimb(int task, int[] roundLayers, int layers, Key key, EvalType evalType, int restarts,
                                 boolean singleIteration, boolean quick) {

        int[] bestLocalTypeCount = key.lugs.createCopy();
        boolean[][] bestLocalPins = key.pins.createCopy();
        int bestLocal = eval(task, roundLayers, layers, key, evalType);

        boolean improved;
        do {
            improved = false;
            int[] types = new int[4];

            int newEval;


            for (int c1 : Lugs.TYPES) {
                for (int c2 : Lugs.TYPES) {
                    if (c1 == c2) {
                        continue;
                    }
                    types[0] = c1;
                    types[1] = c2;
                    for (int[] changes : changes2Types) {

                        if (!doChangesIfValid(bestLocalTypeCount, types, changes)) {
                            continue;
                        }
                        if (!key.lugs.set(bestLocalTypeCount, false)) {
                            undoChanges(bestLocalTypeCount, types, changes);
                            continue;
                        }
                        newEval = eval(task, roundLayers, layers, key, evalType);
                        if (newEval > bestLocal) {
                            improved = true;
                            bestLocal = newEval;
                            key.pins.get(bestLocalPins);
                            ReportResult.reportResult(task, roundLayers, layers, key, bestLocal, "HC L. s" + changes.length);
                        } else {
                            undoChanges(bestLocalTypeCount, types, changes);
                            key.lugs.set(bestLocalTypeCount, false);
                        }
                    }
                }
            }

            if (quick) {
                continue;
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

                                for (int[] changes : changes4Types) {
                                    if (!doChangesIfValid(bestLocalTypeCount, types, changes)) {
                                        continue;
                                    }
                                    if (!key.lugs.set(bestLocalTypeCount, false)) {
                                        undoChanges(bestLocalTypeCount, types, changes);
                                        continue;
                                    }
                                    newEval = eval(task, roundLayers, layers, key, evalType);

                                    if (newEval > bestLocal) {
                                        improved = true;
                                        bestLocal = newEval;
                                        ReportResult.reportResult(task, roundLayers, layers, key, bestLocal, "HC L. s" + changes.length);
                                        key.pins.get(bestLocalPins);
                                        break;
                                    } else {
                                        undoChanges(bestLocalTypeCount, types, changes);
                                        key.lugs.set(bestLocalTypeCount, false);
                                    }
                                }
                            }
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

                            for (int[] changes : changes3Types) {
                                if (!doChangesIfValid(bestLocalTypeCount, types, changes)) {
                                    continue;
                                }
                                if (!key.lugs.set(bestLocalTypeCount, false)) {
                                    undoChanges(bestLocalTypeCount, types, changes);
                                    continue;
                                }
                                newEval = eval(task, roundLayers, layers, key, evalType);

                                if (newEval > bestLocal) {
                                    improved = true;
                                    bestLocal = newEval;
                                    ReportResult.reportResult(task, roundLayers, layers, key, bestLocal, "HC L. s" + changes.length);
                                    key.pins.get(bestLocalPins);
                                    break;
                                } else {
                                    undoChanges(bestLocalTypeCount, types, changes);
                                    key.lugs.set(bestLocalTypeCount, false);
                                }
                            }
                        }
                    }
                }

            }


        } while (improved && !singleIteration);

        key.pins.set(bestLocalPins);
        return bestLocal;

    }

    public static boolean doChangesIfValid(int[] typeCount, int[] types, int[] changes) {

        for (int i = 0; i < changes.length; i++) {
            int change = changes[i];
            int type = types[i];
            int newCount = typeCount[type] + change;
            if ((newCount < 0) || (newCount > Global.MAX_KICK)) {
                return false;
            }
        }
        doChanges(typeCount, types, changes);

        int overlaps = Lugs.overlaps(typeCount);

        if ((overlaps > Global.MAX_OVERLAP) || (overlaps < Global.MIN_OVERLAP) || (Global.VERSION == Version.SWEDISH && common.Utils.sum(typeCount) != Key.BARS)) {
            undoChanges(typeCount, types, changes);
            return false;
        }
        return true;
    }

    public static void undoChanges(int[] typeCount, int[] types, int[] changes) {
        for (int i = 0; i < changes.length; i++) {
            int change = changes[i];
            int type = types[i];
            typeCount[type] -= change;
        }
    }

    private static void doChanges(int[] typeCount, int[] types, int[] changes) {
        for (int i = 0; i < changes.length; i++) {
            int change = changes[i];
            int type = types[i];
            typeCount[type] += change;
        }
    }

}
