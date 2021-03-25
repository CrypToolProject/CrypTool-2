package m209;

import common.CtAPI;
import common.Utils;

import java.util.Arrays;
import static common.CtAPI.printf;

public class Pins {

    //ISOMORPHIC pins, take into account the indicator and the active offset
    //for a given message at pos P and for wheel W, look at
    // isoPinsReal[W][POS%WHEEL_SIZE[W]]
    public boolean[][] isoPins = new boolean[Key.WHEELS + 1][26];
    public String indicator = Key.NULL_INDICATOR;
    private Key parentKey = null;

    public boolean[] wheelPins1 = isoPins[1];
    public boolean[] wheelPins2 = isoPins[2];
    public boolean[] wheelPins3 = isoPins[3];
    public boolean[] wheelPins4 = isoPins[4];
    public boolean[] wheelPins5 = isoPins[5];
    public boolean[] wheelPins6 = isoPins[6];


    private Pins(Key parentKey) {
        for (int w = 1; w <= Key.WHEELS; w++) {
            isoPins[w] = new boolean[Key.WHEELS_SIZE[w]];
        }
        wheelPins1 = isoPins[1];
        wheelPins2 = isoPins[2];
        wheelPins3 = isoPins[3];
        wheelPins4 = isoPins[4];
        wheelPins5 = isoPins[5];
        wheelPins6 = isoPins[6];
        this.parentKey = parentKey;
    }

    // Absolute PIN settings, as defined in the key
    Pins(Key parentKey, String[] absolutePins, String indicator) {
        this(parentKey);
        set(absolutePins, indicator);
    }

    public void toggle(int w, int pos) {
        isoPins[w][pos] ^= true;
    }

    public void toggle(int w, int pos1, int pos2) {
        isoPins[w][pos1] ^= true;
        isoPins[w][pos2] ^= true;
    }

    public boolean compare(int w, int pos1, int pos2) {
        return isoPins[w][pos1] == isoPins[w][pos2];
    }

    private String absolutePinString(int w) {

        boolean[] pinsW = isoPins[w];
        StringBuilder s = new StringBuilder();
        for (int index = 0; index < Key.WHEELS_SIZE[w]; index++) {
            int isoIndex = isoIndex(w, index);
            if (pinsW[isoIndex]) {
                s.append(Key.WHEEL_LETTERS[w].charAt(index));
            }
        }
        return s.toString();
    }

    private String absolutePinString0or1(int w) {

        StringBuilder s = new StringBuilder();
        for (int index = 0; index < Key.WHEELS_SIZE[w]; index++) {
            int isoIndex = isoIndex(w, index);
            if (isoPins[w][isoIndex]) {
                s.append("1");
            } else {
                s.append("0");
            }
        }
        return s.toString();
    }

    public String absolutePinStringAll() {
        StringBuilder s = new StringBuilder();
        s.append(indicator);
        s.append(" ");
        for (int w = 1; w <= Key.WHEELS; w++) {
            s.append(" ").append(w).append(": ");
            s.append(absolutePinString(w));
            s.append(" (").append(absolutePinString0or1(w)).append(") ");
            s.append(",");
        }
        return s.toString();
    }
    public String absolutePinStringAll01() {
        StringBuilder s = new StringBuilder();
        s.append(indicator);
        s.append(" ");
        for (int w = 1; w <= Key.WHEELS; w++) {
            s.append(" ").append(w).append(": ");
            s.append(absolutePinString0or1(w)).append(" ");
        }
        return s.toString();
    }

    public String[] absolutePinsStringArray() {

        String[] pins = new String[Key.WHEELS];
        for (int w = 1; w <= Key.WHEELS; w++) {
            pins[w - 1] = this.absolutePinString(w);
        }
        return pins;

    }

    public boolean[][] createCopy() {
        boolean[][] isoPins = new boolean[Key.WHEELS + 1][];
        for (int w = 1; w <= Key.WHEELS; w++) {
            isoPins[w] = new boolean[this.isoPins[w].length];
            System.arraycopy(this.isoPins[w], 0, isoPins[w], 0, this.isoPins[w].length);
        }
        return isoPins;
    }

    public void get(boolean[][] isoPins) {
        for (int w = 1; w <= Key.WHEELS; w++) {
            System.arraycopy(this.isoPins[w], 0, isoPins[w], 0, this.isoPins[w].length);
        }
    }

    public void set(boolean[][] isoPins) {
        for (int w = 1; w <= Key.WHEELS; w++) {
            System.arraycopy(isoPins[w], 0, this.isoPins[w], 0, this.isoPins[w].length);
        }
        if (parentKey != null){
            parentKey.invalidateDecryption();
        }
    }

    public boolean[] createCopy(int w) {
        boolean[] isoPinsW = new boolean[this.isoPins[w].length];
        System.arraycopy(this.isoPins[w], 0, isoPinsW, 0, this.isoPins[w].length);
        return isoPinsW;
    }

    public void get(int w, boolean[] isoPinsW) {
        System.arraycopy(this.isoPins[w], 0, isoPinsW, 0, this.isoPins[w].length);
    }

    public void set(int w, boolean[] isoPinsW) {
        System.arraycopy(isoPinsW, 0, this.isoPins[w], 0, this.isoPins[w].length);
        if (parentKey != null){
            parentKey.invalidateDecryption();
        }
    }


    public void setIndicator(String indicator) {
        String[] pinsString = new String[Key.WHEELS];
        for (int w = 1; w <= Key.WHEELS; w++) {
            pinsString[w - 1] = absolutePinString(w);
        }
        set(pinsString, indicator);
    }

    public void print() {

        /*
        for (int index = 0; index < Key.WHEELS_SIZE[1]; index++) {
            StringBuilder s = new StringBuilder();
            for (int w = 1; w <= Key.WHEELS; w++) {
                if (index >= Key.WHEELS_SIZE[w]) {
                    continue;
                }
                int isoIndex = isoIndex(w, index);
                if (isoPins[w][isoIndex]) {
                    s.append(Key.WHEEL_LETTERS[w].charAt(index));
                } else {
                    s.append("-");
                }
            }
            printf("\"%s\",\n", s.toString());
        }
        */
        printf("%s\n", absolutePinStringAll());
    }

    private void set(String[] absolutePinsStringArray, String indicator) {
        if (indicator.length() != Key.WHEELS) {
            CtAPI.goodbyeFatalError("Wrong indicator length: " + indicator);
        }
        for (int w = 1; w <= Key.WHEELS; w++) {
            char ic = indicator.charAt(w - 1);
            int indicatorIndex = Key.WHEEL_LETTERS[w].indexOf(ic);
            if (indicatorIndex == -1) {
                CtAPI.goodbyeFatalError("Invalid indicator letter [" + ic + " ] for wheel: " + w + " - Does not appear in Wheel letters");
            }
        }

        this.indicator = indicator;

        if (absolutePinsStringArray == null) {
            return;
        }

        if (absolutePinsStringArray.length == Key.WHEELS) {
            for (int w = 1; w <= Key.WHEELS; w++) {

                // need clean!
                Arrays.fill(isoPins[w], false);

                StringBuilder pinw = new StringBuilder();

                for (char c : absolutePinsStringArray[w - 1].toCharArray()) {
                    if (c != ' ') {
                        pinw.append(c);
                    }
                }
                if (pinw.length() > Key.WHEELS_SIZE[w]) {
                    CtAPI.goodbyeFatalError("Too many isoPinsReal for wheel: " + w + " (" + absolutePinsStringArray[w] + ")");
                }
                for (char c : pinw.toString().toCharArray()) {
                    int index = Key.WHEEL_LETTERS[w].indexOf(c);
                    if (index == -1) {
                        CtAPI.goodbyeFatalError("Invalid letter [" + c + " ]for wheel: " + w + " (" + absolutePinsStringArray[w] + ")");
                    }

                    int isoIndex = isoIndex(w, index);

                    if (isoPins[w][isoIndex]) {
                        CtAPI.goodbyeFatalError("Duplicate letter [" + c + " ]for wheel: " + w + " (" + absolutePinsStringArray[w] + ")");
                    }
                    isoPins[w][isoIndex] = true;
                }
            }
        } else if (absolutePinsStringArray.length == Key.WHEELS_SIZE[1]) {
            for (int w = 1; w <= Key.WHEELS; w++) {
                // need clean!
                Arrays.fill(isoPins[w], false);

                StringBuilder pinw = new StringBuilder();

                for (String s : absolutePinsStringArray) {
                    if ((s.length() >= w) && (s.charAt(w - 1) != '-')) {
                        pinw.append(s.charAt(w - 1));
                    }
                }

                if (pinw.length() > Key.WHEELS_SIZE[w]) {
                    CtAPI.goodbyeFatalError("Too many isoPinsReal for wheel: " + w + " (" + pinw + ")");
                }
                for (char c : pinw.toString().toCharArray()) {
                    int index = Key.WHEEL_LETTERS[w].indexOf(c);
                    if (index == -1) {
                        CtAPI.goodbyeFatalError("Invalid letter [" + c + " ]for wheel: " + w + " (" + pinw + ")");
                    }

                    int isoIndex = isoIndex(w, index);

                    if (isoPins[w][isoIndex]) {
                        CtAPI.goodbyeFatalError("Duplicate letter [" + c + " ]for wheel: " + w + " (" + pinw + ")");
                    }
                    isoPins[w][isoIndex] = true;
                }
            }
        } else {
            CtAPI.goodbyeFatalError("Wrong pins - size of array : " + absolutePinsStringArray.length);
        }
        if (parentKey != null){
            parentKey.invalidateDecryption();
        }
    }

    private int isoIndex(int w, int index) {
        char ic = indicator.charAt(w - 1);
        int indicatorIndex = Key.WHEEL_LETTERS[w].indexOf(ic);

        char activeC = Key.WHEELS_ACTIVE_PINS[w];
        int activeIndex = Key.WHEEL_LETTERS[w].indexOf(activeC);

        return (index - activeIndex - indicatorIndex + 2 * Key.WHEELS_SIZE[w]) % Key.WHEELS_SIZE[w];
    }


    public void randomize(int w) {

        final boolean[] isoPinsW = isoPins[w];
        final int total = isoPins[w].length;
        final int maxActive = (Global.MAX_PERCENT_ACTIVE_PINS * total)/100;
        final int maxInactive = ((100 - Global.MIN_PERCENT_ACTIVE_PINS) * total)/100;
        boolean good;
        do {
            Arrays.fill(isoPinsW, false);
            good = true;
            int countActive = 0;
            int countInactive = 0;
            int rand = common.Utils.random.nextInt();
            int consecutiveActive = 0;
            int consecutiveInactive = 0;
            for (int p = 0; p < total; p++) {
                boolean newVal = (rand & 0x1) == 0x1;
                boolean cannotBeActive = consecutiveActive == Global.MAX_CONSECUTIVE_SAME_PINS || countActive == maxActive;
                boolean cannotBeInactive = consecutiveInactive == Global.MAX_CONSECUTIVE_SAME_PINS || countInactive == maxInactive;
                if (cannotBeActive && cannotBeInactive) {
                    good = false;
                    break;
                } else if (cannotBeActive) {
                    newVal = false;
                } else if (cannotBeInactive) {
                    newVal = true;
                }
                if (newVal) {
                    countActive++;
                    consecutiveActive++;
                    consecutiveInactive = 0;
                } else {
                    countInactive++;
                    consecutiveInactive++;
                    consecutiveActive = 0;
                }
                isoPinsW[p] = newVal;
                rand >>= 1;
            }

            if (!good) {
                continue;
            }
            if (Global.MAX_CONSECUTIVE_SAME_PINS < 10) {
                int rounds = 0;
                for (int p = 0, unchanged = 0; unchanged <= Global.MAX_CONSECUTIVE_SAME_PINS; p = (p == total - 1) ? 0 : (p + 1)) {
                    rounds++;
                    if (rounds > 100) {
                        good = false;
                        break;
                    }
                    boolean currentVal = isoPinsW[p];
                    boolean newVal = currentVal;
                    boolean cannotBeActive = consecutiveActive == Global.MAX_CONSECUTIVE_SAME_PINS;
                    boolean cannotBeInactive = consecutiveInactive == Global.MAX_CONSECUTIVE_SAME_PINS;
                    if (cannotBeActive && cannotBeInactive) {
                        good = false;
                        break;
                    } else if (cannotBeActive) {
                        newVal = false;
                    } else if (cannotBeInactive) {
                        newVal = true;
                    }
                    if (newVal == currentVal) {
                        // No change - only update consecutive counters.
                        if (currentVal) {
                            consecutiveActive++;
                            consecutiveInactive = 0;
                        } else {
                            consecutiveInactive++;
                            consecutiveActive = 0;
                        }
                        unchanged++;
                    } else {
                        // Change the sign.
                        cannotBeActive = countActive == maxActive;
                        cannotBeInactive = countInactive == maxInactive;
                        if ((newVal && cannotBeActive) || (!newVal && cannotBeInactive)) {
                            good = false;
                            break;
                        }
                        if (newVal) {
                            countActive++;
                            countInactive--; // We changed the sign!.
                            consecutiveActive++;
                            consecutiveInactive = 0;
                        } else {
                            countInactive++;
                            countActive--; // We changed the sign.
                            consecutiveInactive++;
                            consecutiveActive = 0;
                        }
                        isoPinsW[p] = newVal;
                        unchanged = 0;
                    }
                }

            }
        } while (!good);


        if (parentKey != null){
            parentKey.invalidateDecryption();
        }
    }

    public boolean longSeq(int w, int centerPos1, int centerPos2) {
        return longSeq(w, centerPos1) || longSeq(w, centerPos2);
    }

    public boolean longSeq(int w, int centerPos) {
        long maxSame = Global.MAX_CONSECUTIVE_SAME_PINS;
        final boolean[] isoPinsW = isoPins[w];
        final int total = isoPinsW.length;
        if (maxSame > total) {
            return false;
        }
        maxSame++; // Not strict.
        final boolean centerVal = isoPinsW[centerPos];

        int same;
        int pos;
        for(same = 1, pos = centerPos + 1; same <= maxSame; same++, pos++) {
            if (pos == total) {
                pos = 0;
            }
            if (isoPinsW[pos] != centerVal) {
                break;
            }
        }

        for (pos = centerPos - 1; same <= maxSame; same++, pos--) {
            if (pos < 0) {
                pos = total - 1;
            }
            if (isoPinsW[pos] != centerVal) {
                break;
            }
        }
        return same > maxSame;
    }

    public void randomize() {
        do {
            for (int w = 1; w <= Key.WHEELS; w++) {
                randomize(w);
            }
        } while ((count() < minCount()) || (count() > maxCount()));
        if (parentKey != null){
            parentKey.invalidateDecryption();
        }
    }

    public int maxCount() {
        return common.Utils.sum(Key.WHEELS_SIZE) * Global.MAX_PERCENT_ACTIVE_PINS / 100;
    }

    public int minCount() {
        return Utils.sum(Key.WHEELS_SIZE) * Global.MIN_PERCENT_ACTIVE_PINS / 100;
    }


    public void inverse(int w) {
        boolean [] isoPinsW = isoPins[w];
        for (int p = 0; p < Key.WHEELS_SIZE[w]; p++) {
            isoPinsW[p] ^= true;
        }
        if (parentKey != null){
            parentKey.invalidateDecryption();
        }
    }

    public void inverseWheelBitmap(int v) {
        for(int w = 1; w <= Key.WHEELS; w++) {
            if (Key.getWheelBit(v, w)) {
                inverse(w);
            }
        }
        if (parentKey != null){
            parentKey.invalidateDecryption();
        }
    }

    public int count() {
        int count = 0;
        for (boolean[] isoPinsW : isoPins) {
            for (boolean val : isoPinsW) {
                if (val) {
                    count++;
                }
            }
        }
        return count;
    }

}
