package m209;

import common.CtAPI;
import common.Utils;

import java.util.Arrays;
import static common.CtAPI.printf;

public class Lugs {

    public static final int TYPE_COUNT_ARRAY_SIZE = 22;

    public int[] typeCount = new int[TYPE_COUNT_ARRAY_SIZE];
    // from 0 to 63 - based on lugs, for each wheel pin engagement vector, compute the decryption
    // 001001 means wheels 3 and 6 have an active pin, the rest inactive
    public int[] displacementVector = new int[64];
    private Key parentKey = null;

    private static final int[][] INDICES_MATRIX = {
            /* 0 */ {0, 1, 2, 3, 4, 5, 6},
            /* 1 */ {1, -1, 7, 8, 9, 10, 11},
            /* 2 */ {2, 7, -1, 12, 13, 14, 15},
            /* 3 */ {3, 8, 12, -1, 16, 17, 18},
            /* 4 */ {4, 9, 13, 16, -1, 19, 20},
            /* 5 */ {5, 10, 14, 17, 19, -1, 21},
            /* 6 */ {6, 11, 15, 18, 20, 21, -1},
    };
    private static final int TYPE_COUNT_W1 = getTypeCountIndex(1);
    private static final int TYPE_COUNT_W2 = getTypeCountIndex(2);
    private static final int TYPE_COUNT_W3 = getTypeCountIndex(3);
    private static final int TYPE_COUNT_W4 = getTypeCountIndex(4);
    private static final int TYPE_COUNT_W5 = getTypeCountIndex(5);
    private static final int TYPE_COUNT_W6 = getTypeCountIndex(6);

    private static final int TYPE_COUNT_W1W2 = getTypeCountIndex(1, 2);
    private static final int TYPE_COUNT_W1W3 = getTypeCountIndex(1, 3);
    private static final int TYPE_COUNT_W1W4 = getTypeCountIndex(1, 4);
    private static final int TYPE_COUNT_W1W5 = getTypeCountIndex(1, 5);
    private static final int TYPE_COUNT_W1W6 = getTypeCountIndex(1, 6);

    private static final int TYPE_COUNT_W2W3 = getTypeCountIndex(2, 3);
    private static final int TYPE_COUNT_W2W4 = getTypeCountIndex(2, 4);
    private static final int TYPE_COUNT_W2W5 = getTypeCountIndex(2, 5);
    private static final int TYPE_COUNT_W2W6 = getTypeCountIndex(2, 6);

    private static final int TYPE_COUNT_W3W4 = getTypeCountIndex(3, 4);
    private static final int TYPE_COUNT_W3W5 = getTypeCountIndex(3, 5);
    private static final int TYPE_COUNT_W3W6 = getTypeCountIndex(3, 6);

    private static final int TYPE_COUNT_W4W5 = getTypeCountIndex(4, 5);
    private static final int TYPE_COUNT_W4W6 = getTypeCountIndex(4, 6);

    private static final int TYPE_COUNT_W5W6 = getTypeCountIndex(5, 6);

    public static final int[] TYPES_WITHOUT_OVERLAP = {
            TYPE_COUNT_W1,
            TYPE_COUNT_W2,
            TYPE_COUNT_W3,
            TYPE_COUNT_W4,
            TYPE_COUNT_W5,
            TYPE_COUNT_W6
    };
    private static final int[] TYPES_WITH_OVERLAP = {
            TYPE_COUNT_W1W2,
            TYPE_COUNT_W1W3,
            TYPE_COUNT_W1W4,
            TYPE_COUNT_W1W5,
            TYPE_COUNT_W1W6,
            TYPE_COUNT_W2W3,
            TYPE_COUNT_W2W4,
            TYPE_COUNT_W2W5,
            TYPE_COUNT_W2W6,
            TYPE_COUNT_W3W4,
            TYPE_COUNT_W3W5,
            TYPE_COUNT_W3W6,
            TYPE_COUNT_W4W5,
            TYPE_COUNT_W4W6,
            TYPE_COUNT_W5W6,
    };

    static final int[] TYPES = {
            TYPE_COUNT_W1,
            TYPE_COUNT_W2,
            TYPE_COUNT_W3,
            TYPE_COUNT_W4,
            TYPE_COUNT_W5,
            TYPE_COUNT_W6,
            TYPE_COUNT_W1W2,
            TYPE_COUNT_W1W3,
            TYPE_COUNT_W1W4,
            TYPE_COUNT_W1W5,
            TYPE_COUNT_W1W6,
            TYPE_COUNT_W2W3,
            TYPE_COUNT_W2W4,
            TYPE_COUNT_W2W5,
            TYPE_COUNT_W2W6,
            TYPE_COUNT_W3W4,
            TYPE_COUNT_W3W5,
            TYPE_COUNT_W3W6,
            TYPE_COUNT_W4W5,
            TYPE_COUNT_W4W6,
            TYPE_COUNT_W5W6,
    };

    Lugs(Key parentKey) {
        this.parentKey = parentKey;
    }

    Lugs(Key parentKey, String lugsString) {
        this(parentKey);
        setLugsString(lugsString, false);
    }

    public static int overlaps(int[] typeCount) {
        int overlaps = 0;
        for (int type : TYPES_WITH_OVERLAP) {
            overlaps += typeCount[type];
        }
        return overlaps;
    }

    private int overlaps() {
        return overlaps(this.typeCount);
    }

    void setLugsString(String lugsString, boolean checkRules) {

        Arrays.fill(typeCount, 0);

        while (lugsString.contains("  ")) {
            lugsString = lugsString.replace("  ", " ");
        }

        String[] barsString = lugsString.split(" ");
        if (barsString.length > Key.BARS || ( barsString.length < Key.BARS && Global.VERSION != Version.UNRESTRICTED)) {
            CtAPI.goodbyeFatalError("Wrong lug string: " + lugsString + " has " + barsString.length + " bars");
        }
        for (String barString : barsString) {
            String[] barSplit = barString.split("-");
            if (barSplit.length != Key.LUGS_PER_BAR) {
                CtAPI.goodbyeFatalError("Wrong lug settings - too many lugs on one bar: " + barString);
            }

            try {

                int w1 = Integer.valueOf(barSplit[0]);
                int w2 = Integer.valueOf(barSplit[1]);


                if ((w1 > Key.WHEELS) || (w2 > Key.WHEELS)) {
                    CtAPI.goodbyeFatalError("Wrong lug settings - wrong wheel number: " + barString);
                }
                if ((w1 == w2) && (w1 != 0)) {
                    CtAPI.goodbyeFatalError("Wrong lug settings - wheel appears twice on same bar: " + barString);
                }

                if (w2 == 0) {
                    if (Global.VERSION != Version.UNRESTRICTED) {
                        CtAPI.goodbyeFatalError("Wrong lug settings - 0-0 not valid: " + barString);
                    } else {
                        continue;
                    }
                }

                if (w1 > w2) {
                    int temp = w1;
                    w1 = w2;
                    w2 = temp;
                }
                if (w1 == 0) {
                    typeCount[getTypeCountIndex(w2)]++;
                } else {
                    typeCount[getTypeCountIndex(w1, w2)]++;
                }

            } catch (NumberFormatException e) {
                CtAPI.goodbyeFatalError("Wrong lug settings - wrong wheel: " + barString);
            }
        }

        if (!LugsRules.isTypeCountCompliant(typeCount)) {
            if (checkRules) {
                CtAPI.goodbyeFatalError("Lug settings do not match the lug count rules: " + lugsString);
            }
        }

        computeVector();
    }

    public String getLugsString() {

        StringBuilder s = new StringBuilder();
        for (int w1 = 1; w1 <= Key.WHEELS; w1++) {
            for (int w2 = w1 + 1; w2 <= Key.WHEELS; w2++) {
                for (int i = 0; i < typeCount[getTypeCountIndex(w1, w2)]; i++) {
                    s.append(w1).append("-").append(w2).append(" ");
                }
            }
        }
        for (int w = 1; w <= Key.WHEELS; w++) {
            for (int i = 0; i < typeCount[getTypeCountIndex(w)]; i++) {
                s.append("0-").append(w).append(" ");
            }
        }
        return s.toString();
    }


    public void get(int[] typeCount) {
        System.arraycopy(this.typeCount, 0, typeCount, 0, TYPE_COUNT_ARRAY_SIZE);
    }

    public int[] createCopy() {
        return Arrays.copyOf(this.typeCount, TYPE_COUNT_ARRAY_SIZE);
    }

    public boolean set(int[] typeCount, boolean checkRules) {
        /*
        if (typeCount.length != TYPE_COUNT_ARRAY_SIZE) {
            CtAPI.goodbyeFatalError("Invalid length of simpleCount array");
        }
        */
        int overlaps = overlaps(typeCount);

        if ((overlaps > Global.MAX_OVERLAP) || (overlaps < Global.MIN_OVERLAP)) {
            return false;
        }

        if (checkRules && !LugsRules.isTypeCountCompliant(typeCount)) {
            return false;
        }
        /*
        int bars = 0;
        for (int typeCountEntry : typeCount) {
            if ((typeCountEntry < 0) || (typeCountEntry > Global.MAX_KICK)) {
                CtAPI.goodbyeFatalError("Invalid kick in simpleCount array");
            }
            bars += typeCountEntry;
        }
        if (bars != Key.BARS) {
            CtAPI.goodbyeFatalError("Wrong number of bars: " + bars);
        }
        */


        System.arraycopy(typeCount, 0, this.typeCount, 0, TYPE_COUNT_ARRAY_SIZE);

        computeVector();

        return true;

    }

    private int[] overlapPerW = new int[7];
    private int[] dispRepetition = new int[28];

    private boolean compliesWithUserManualRules(int[] typeCount) {

        if (Global.VERSION == Version.SWEDISH) {
            return common.Utils.sum(typeCount) == Key.BARS && overlaps() <= Global.MAX_OVERLAP;
        }
        Arrays.fill(overlapPerW, 7);
        int overlaps = 0;
        int adjacentOverlaps = 0;
        int involvedWheels = 0;
        for (int w1 = 1; w1 <= Key.WHEELS; w1++) {
            for (int w2 = w1 + 1; w2 <= Key.WHEELS; w2++) {
                int count = typeCount[getTypeCountIndex(w1, w2)];
                if (count > Global.MAX_SAME_OVERLAP) {
                    return false;
                }
                overlapPerW[w1] += count;
                overlapPerW[w2] += count;
                overlaps += count;
                if (w2 == w1 + 1) {
                    adjacentOverlaps += count;
                }
            }
        }


        if (overlaps > 1) {

            if (Global.MIN_INVOLVED_WHEELS > 0) {
                for (int w = 1; w <= Key.WHEELS; w++) {
                    if (overlapPerW[w] > 0) {
                        involvedWheels++;
                    }
                }
                if (involvedWheels < Global.MIN_INVOLVED_WHEELS) {
                    return false;
                }
            }

            if (Global.OVERLAPS_SIDEBYSIDE_SEPARATED) {
                int nonAdjacentOverlaps = overlaps - adjacentOverlaps;
                if (nonAdjacentOverlaps == 0) {
                    return false;
                }
                if (adjacentOverlaps == 0) {
                    return false;
                }
            }
        }

        if ( Global.MAX_TOTAL_OVERLAP < 6) {
            int wheelsWithCompleteOverlap = 0;
            for (int w = 1; w <= Key.WHEELS; w++) {
                if (typeCount[getTypeCountIndex(w)] == 0) {
                    wheelsWithCompleteOverlap++;
                }
            }
            if (wheelsWithCompleteOverlap > Global.MAX_TOTAL_OVERLAP) {
                return false;
            }
        }
        if (Global.MAX_KICK_REPETITION_64 <= 64) {
            Arrays.fill(dispRepetition, 0);
            for (int i = 0; i < 64; i++) {
                dispRepetition[displacementVector[i]]++;
                if (dispRepetition[displacementVector[i]] > Global.MAX_KICK_REPETITION_64) {
                    return false;
                }
            }
        }
        return true;
    }


    private static int findWheelWithEnoughCountLeft(int[] targetCount, int[] actualCount, int notThisWheel) {
        int[] weighted = new int[100];

        int items = 0;

        for (int w = 1; w <= Key.WHEELS; w++) {
            if (w == notThisWheel) {
                continue;
            }
            int leftCount = targetCount[w] - actualCount[w];
            for (int i = 0; i < leftCount; i++) {
                weighted[items++] = w;
            }
        }

        if (items == 0) {
            return 0;
        }
        int rand = common.Utils.randomNextInt(items);

        return  weighted[rand];

    }

    public void randomize() {
        randomize(0);
    }

    public void randomize(int overlaps) {
        do {
            randomizePrivate(overlaps);
        } while (!compliesWithUserManualRules(typeCount));
    }

    public void randomizeNoOverlap() {
        typeCount = new int[TYPE_COUNT_ARRAY_SIZE];
        int count = 0;
        for (int w1 = 1; w1 <= Key.WHEELS; w1++) {
            typeCount[getTypeCountIndex(w1)] = 1;
            count++;
        }
        while (count < Key.BARS) {
            int w1 = common.Utils.randomNextInt(6) + 1;
            typeCount[getTypeCountIndex(w1)]++;
            count++;
        }
        computeVector();
    }

    private static boolean acceptMultipleSimilarOverlaps(int same) {
        if (same >= Global.MAX_OVERLAP) {
            return false;
        }
        if (!Global.OVERLAPS_EVENLY) {
            return true;
        }
        int ref = 64;
        long rand = common.Utils.randomNextInt(ref);

        for (int i = 0; i < same; i++) {
            ref /= 2;
        }
        return rand < ref;

    }

    private int[] actualLugsCountSeq = new int[Key.WHEELS + 1];
    private int[] targetLugsCountSeq = new int[Key.WHEELS + 1];

    private void randomizePrivate(int requiredOverlap) {

        Arrays.fill(typeCount, 0);
        if (Global.VERSION == Version.UNRESTRICTED) {
            for (int i = 0; i < Key.BARS; i++) {
                int w1 = common.Utils.randomNextInt(7);
                int w2 = common.Utils.randomNextInt(7);
                if (w1 == 0) {
                    typeCount[0]++;
                } else if (w2 == 0 || w1 == w2) {
                    typeCount[getTypeCountIndex(w1)]++;
                } else {
                    typeCount[getTypeCountIndex(w1, w2)]++;
                }
            }
            computeVector();
            return;
        }
        if (Global.VERSION == Version.SWEDISH) {
            for (int i = 0; i < Key.BARS; i++) {
                int w1 = 1 + common.Utils.randomNextInt(6);
                int w2 = common.Utils.randomNextInt(7);
                if (w2 == 0 || w1 == w2 || overlaps(typeCount) >= Global.MAX_OVERLAP) {
                    typeCount[getTypeCountIndex(w1)]++;
                } else {
                    typeCount[getTypeCountIndex(w1, w2)]++;
                }
            }
            computeVector();
            return;
        }

        if (Global.VERSION == Version.NO_OVERLAP) {
            for (int i = 0; i < Key.BARS; i++) {
                int w = common.Utils.randomNextInt(6)+1;
                typeCount[getTypeCountIndex(w)]++;
            }
            computeVector();
            return;
        }

        if ((requiredOverlap != 0) && ((requiredOverlap > Global.MAX_OVERLAP) || (requiredOverlap < Global.MIN_OVERLAP))) {
            CtAPI.goodbyeFatalError("Failure generating random lugs. Overlap outside limits" + requiredOverlap);
        }

        int lugCountSeqOverlaps;
        int lugCountSeqTotal;
        int[] lugsCountSeq;
        while (true) {
            int seqIndex = common.Utils.randomNextInt(LugsRules.validLugCountSequences.size());
            lugsCountSeq = LugsRules.validLugCountSequences.get(seqIndex);
            lugCountSeqTotal = common.Utils.sum(lugsCountSeq);
            lugCountSeqOverlaps = lugCountSeqTotal - Key.BARS;
            if (requiredOverlap != 0 && lugCountSeqOverlaps == requiredOverlap) {
                break;
            }
            if (requiredOverlap == 0 && lugCountSeqOverlaps >= Global.MIN_OVERLAP && lugCountSeqOverlaps <= Global.MAX_OVERLAP){
                break;
            }
        }

        int [] perm6 = Utils.randomPerm6();

        for (int w = 1; w <= Key.WHEELS; w++) {
            targetLugsCountSeq[perm6[w - 1] + 1] = lugsCountSeq[w];
        }
        // Fill the bars with overlaps.
        int barsCount = 0;
        Arrays.fill(actualLugsCountSeq, 0);

        while (lugCountSeqOverlaps > 0) {
            int w1 = findWheelWithEnoughCountLeft(targetLugsCountSeq, actualLugsCountSeq, 0);
            actualLugsCountSeq[w1]++;
            int w2 = findWheelWithEnoughCountLeft(targetLugsCountSeq, actualLugsCountSeq, w1);
            actualLugsCountSeq[w2]++;

            if (!acceptMultipleSimilarOverlaps(typeCount[getTypeCountIndex(w1, w2)])) {
                actualLugsCountSeq[w1]--;
                actualLugsCountSeq[w2]--;
                continue;
            }

            if (w2 == 0) {
                CtAPI.goodbyeFatalError("Failure generating random lugs (2)");
                //break;
            }
            if (barsCount >= Key.BARS) {
                CtAPI.goodbyeFatalError("Failure generating random lugs (3)");
            }
            barsCount++;

            typeCount[getTypeCountIndex(w1, w2)]++;

            lugCountSeqOverlaps--;

        }


        // Bars without overlaps.
        for (int w = 1; w <= Key.WHEELS; w++) {
            while (actualLugsCountSeq[w] < targetLugsCountSeq[w]) {
                barsCount++;
                typeCount[getTypeCountIndex(w)]++;
                actualLugsCountSeq[w]++;
            }
        }

        int actualOverlaps = overlaps(typeCount);
        if (requiredOverlap != 0 && actualOverlaps != requiredOverlap) {
            CtAPI.goodbyeFatalError("Failure generating random lugs (4).  Required overlaps: " + requiredOverlap + " Actual: "+ actualOverlaps);
        }
        if (barsCount > Key.BARS) {
            CtAPI.goodbyeFatalError("Failure generating random lugs (5)");
        }

        computeVector();
    }

    public void print() {
        printf("%s\n", getLugsString());
    }

    public static int getTypeCountIndex(int w) {
        return INDICES_MATRIX[0][w];
    }

    public static int getTypeCountIndex(int w1, int w2) {
        return INDICES_MATRIX[w1][w2];
    }

    private void computeVector() {
        int d1, d2, d3, d4, d5, d6;
        int d12, d13, d14, d15, d16, d23, d24, d25, d26, d34, d35, d36, d45, d46, d56;

        d1 = typeCount[TYPE_COUNT_W1];
        d2 = typeCount[TYPE_COUNT_W2];
        d3 = typeCount[TYPE_COUNT_W3];
        d4 = typeCount[TYPE_COUNT_W4];
        d5 = typeCount[TYPE_COUNT_W5];
        d6 = typeCount[TYPE_COUNT_W6];

        d12 = typeCount[TYPE_COUNT_W1W2];
        d13 = typeCount[TYPE_COUNT_W1W3];
        d14 = typeCount[TYPE_COUNT_W1W4];
        d15 = typeCount[TYPE_COUNT_W1W5];
        d16 = typeCount[TYPE_COUNT_W1W6];

        d23 = typeCount[TYPE_COUNT_W2W3];
        d24 = typeCount[TYPE_COUNT_W2W4];
        d25 = typeCount[TYPE_COUNT_W2W5];
        d26 = typeCount[TYPE_COUNT_W2W6];

        d34 = typeCount[TYPE_COUNT_W3W4];
        d35 = typeCount[TYPE_COUNT_W3W5];
        d36 = typeCount[TYPE_COUNT_W3W6];

        d45 = typeCount[TYPE_COUNT_W4W5];
        d46 = typeCount[TYPE_COUNT_W4W6];

        d56 = typeCount[TYPE_COUNT_W5W6];

        for (int v = 0; v < displacementVector.length; v++) {
            int displacement = 0;

            int vec = v;
            boolean w1 = (vec & 0x1) == 0x1;
            vec >>= 1;
            boolean w2 = (vec & 0x1) == 0x1;
            vec >>= 1;
            boolean w3 = (vec & 0x1) == 0x1;
            vec >>= 1;
            boolean w4 = (vec & 0x1) == 0x1;
            vec >>= 1;
            boolean w5 = (vec & 0x1) == 0x1;
            vec >>= 1;
            boolean w6 = (vec & 0x1) == 0x1;

            if (w1) {
                displacement += d1;
            }

            if (w2) {
                displacement += d2 + d12;
            } else if (w1) {
                displacement += d12;
            }

            if (w3) {
                displacement += d3 + d13 + d23;
            } else {
                if (w1) {
                    displacement += d13;
                }
                if (w2) {
                    displacement += d23;
                }
            }

            if (w4) {
                displacement += d4 + d14 + d24 + d34;
            } else {
                if (w1) {
                    displacement += d14;
                }
                if (w2) {
                    displacement += d24;
                }
                if (w3) {
                    displacement += d34;
                }
            }


            if (w5) {
                displacement += d5 + d15 + d25 + d35 + d45;
            } else {
                if (w1) {
                    displacement += d15;
                }
                if (w2) {
                    displacement += d25;
                }
                if (w3) {
                    displacement += d35;
                }
                if (w4) {
                    displacement += d45;
                }
            }

            if (w6) {
                displacement += d6 + d16 + d26 + d36 + d46 + d56;
            } else {
                if (w1) {
                    displacement += d16;
                }
                if (w2) {
                    displacement += d26;
                }
                if (w3) {
                    displacement += d36;
                }
                if (w4) {
                    displacement += d46;
                }
                if (w5) {
                    displacement += d56;
                }
            }

            if (displacement >= 26) {
                displacement -= 26;
            }
            displacementVector[v] = displacement;

        }
        if (parentKey != null) {
            parentKey.invalidateDecryption();
        }
    }
}
    