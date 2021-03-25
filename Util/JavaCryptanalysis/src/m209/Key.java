package m209;

import common.CtAPI;
import common.Stats;

import java.util.Arrays;
import static common.CtAPI.printf;

enum EvalType {CRIB, MONO, PINS_SA_CRIB}

public class Key {
    public static final int WHEELS = 6;
    public static int BARS = 27;
    public static final int LUGS_PER_BAR = 2;
    public static final String[] WHEEL_LETTERS = {
            "????????????????????????????",
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
            //"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvxyz",
            //   ZYXWVUTSRQPONMLKJIHGFEDCBA
            "ABCDEFGHIJKLMNOPQRSTUVXYZ",
            //   ZYXWVUTSRQPONMLKJIHGFEDCBA
            "ABCDEFGHIJKLMNOPQRSTUVX",
            //   ZYXWVUTSRQPONMLKJIHGFEDCBA
            "ABCDEFGHIJKLMNOPQRSTU",
            //   ZYXWVUTSRQPONMLKJIHGFEDCBA
            "ABCDEFGHIJKLMNOPQRS",
            //   ZYXWVUTSRQPONMLKJIHGFEDCBA
            "ABCDEFGHIJKLMNOPQ",
            //   ZYXWVUTSRQPONMLKJIHGFEDCBA

    };

    //38 41 42 43 46 47
    public static final String[] WHEEL_LETTERS_C52 = {
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ6789012345[]{}<>@#$&*",
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ6789012345[]{}<>@#$&*",
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ6789012345[]{}<>@#$&",
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ6789012345[]{}<>@",
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ6789012345[]{}<>",
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ6789012345[]{}<",
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ6789012345[]",
    };

    public static final int[] WHEELS_SIZE = {-1, WHEEL_LETTERS[1].length(), WHEEL_LETTERS[2].length(),
            WHEEL_LETTERS[3].length(), WHEEL_LETTERS[4].length(),
            WHEEL_LETTERS[5].length(), WHEEL_LETTERS[6].length()};
    public static final String Ax6 = "AAAAAA";
    public static final String Ax26 = "AAAAAAAAAAAAAAAAAAAAAAAAAA";
    public static final char[] WHEELS_ACTIVE_PINS = "?PONMLK".toCharArray();
    public static final String NULL_INDICATOR = "LLKJIH";
    private static int SPACE = common.Utils.Z;
    private final static int S1 = WHEELS_SIZE[1];
    private final static int S2 = WHEELS_SIZE[2];
    private final static int S3 = WHEELS_SIZE[3];
    private final static int S4 = WHEELS_SIZE[4];
    private final static int S5 = WHEELS_SIZE[5];
    private final static int S6 = WHEELS_SIZE[6];

    private final static int M1 = 0x1;
    private final static int M2 = M1 << 1;
    private final static int M3 = M2 << 1;
    private final static int M4 = M3 << 1;
    private final static int M5 = M4 << 1;
    private final static int M6 = M5 << 1;

    public Pins pins;
    public Lugs lugs;
    public int slide = 0;

    private int[] decryption = null;
    private boolean decryptionValid = false;
    private int decryptionFrequency[] = new int[26];

    public String cipher = "";
    public String crib = "";
    public int[] cipherArray;
    private int[] cribArray;

    private Key originalKey;
    public long originalScore;

    static long evaluations = 0;

    public Key(String[] pinsString, String indicator, String lugsString) {
        pins = new Pins(this, pinsString, indicator);
        if ((lugsString == null) || lugsString.isEmpty()) {
            lugs = new Lugs(this);
        } else {
            lugs = new Lugs(this, lugsString);
        }
        invalidateDecryption();
    }

    public Key(String[] pinsString, String lugsString) {
        this(pinsString, Ax6, lugsString);
    }

    public Key() {
        this(null, NULL_INDICATOR, null);
    }

    public Key(Key key) {
        this(key.pins.absolutePinsStringArray(), key.pins.indicator, key.lugs.getLugsString());
        this.cipherArray = key.cipherArray;
        this.cribArray = key.cribArray;
        this.slide = key.slide;
        this.crib = key.crib;
        this.cipher = key.cipher;
        this.originalKey = key.originalKey;
        this.originalScore = key.originalScore;
    }

    public String encryptDecrypt(String in, boolean encrypt) {

        StringBuilder s = new StringBuilder();
        for (int pos = 0; pos < in.length(); pos++) {
            char c = in.charAt(pos);
            if (c == '?') {
                s.append('?');
            } else {
                int cin;
                if (encrypt && (c == ' ')) {
                    cin = SPACE;
                } else {
                    cin = c - 'A';
                }
                int disp = displacement(pos);
                int cout = encryptSymbol(cin, disp);
                if (!encrypt && (cout == SPACE)) {
                    s.append(' ');
                } else {
                    s.append((char) ('A' + cout));
                }
            }
        }

        return s.toString();
    }

    private int displacement(int pos) {

        return lugs.displacementVector[(pins.wheelPins1[pos % S1] ? M1 : 0)
                + (pins.wheelPins2[pos % S2] ? M2 : 0)
                + (pins.wheelPins3[pos % S3] ? M3 : 0)
                + (pins.wheelPins4[pos % S4] ? M4 : 0)
                + (pins.wheelPins5[pos % S5] ? M5 : 0)
                + (pins.wheelPins6[pos % S6] ? M6 : 0)];

    }

    public int evalMono() {
        updateDecryptionIfInvalid();

        int mono = 0;
        for (int i = 0; i < 26; i++) {
            int f = decryptionFrequency[i];
            mono += Stats.monogramStats[i] * f;
            //mono += Stats.monogramStatsLinear[i] * f;
        }
        return mono / cipherArray.length;
    }

    private int evalADE() {

        updateDecryptionIfInvalid();

        int sumScore = 0;
        int missing = 0;

        int actual;
        int expected;

        for (int cribIndex = 0; cribIndex < cribArray.length; cribIndex++) {

            expected = cribArray[cribIndex];
            if (expected == -1) {
                missing++;
                continue;
            }
            actual = decryption[cribIndex];

            int dist1 = Math.abs(expected - actual);
            int dist2 = Math.abs(expected + 26 - actual);
            sumScore += 26 - Math.min(dist1, dist2);
        }

        return 5000 * sumScore / (cribArray.length - missing);

    }

    public int eval(EvalType evalType) {

        evaluations++;

        switch (evalType) {
            case CRIB:
                return evalADE();
            case MONO:
                return evalMono();
            default:
                CtAPI.goodbyeFatalError("Unsupported eval type " + evalType);
                return 0;
        }

    }

    public static void printCounter() {
        printf("Number of evaluations: %,d\n", evaluations);
    }

    public static void resetCounter() {
        evaluations = 0;
    }

    public static boolean getWheelBit(int bitmap, int w) {
        return ((bitmap >> (w - 1)) & 0x1) == 0x1;
    }

    public void invalidateDecryption() {
        decryptionValid = false;
    }

    private void updateDecryptionIfInvalid() {
        if (!decryptionValid) {
            updateDecryption();
        }
    }

    private void updateDecryption() {

        if (cipherArray == null) {
            return;
        }
        if ((decryption == null) || (decryption.length != cipherArray.length)) {
            decryption = new int[cipherArray.length];
        }
        Arrays.fill(decryptionFrequency, 0);
        final int len = (cribArray != null) ? cribArray.length : cipherArray.length;
        int pos1 = 0, pos2 = 0, pos3 = 0, pos4 = 0, pos5 = 0, pos6 = 0;
        int displacement;
        int vector;
        int slidePlus25 = (slide + 25) % 26;
        int symbol;
        for (int pos = 0; pos < len; pos++) {
            vector = 0;
            if (pins.wheelPins1[pos1]) vector += M1;
            if (pins.wheelPins2[pos2]) vector += M2;
            if (pins.wheelPins3[pos3]) vector += M3;
            if (pins.wheelPins4[pos4]) vector += M4;
            if (pins.wheelPins5[pos5]) vector += M5;
            if (pins.wheelPins6[pos6]) vector += M6;


            symbol = slidePlus25 - cipherArray[pos] + lugs.displacementVector[vector];
            if (symbol >= 26) {
                symbol -= 26;
            }

            //displacement = lugs.displacementVector[vector];
            //symbol = encryptSymbol(cipherArray[pos], displacement);

            decryption[pos] = symbol;
            decryptionFrequency[symbol]++;

            if (++pos1 == Key.S1) pos1 = 0;
            if (++pos2 == Key.S2) pos2 = 0;
            if (++pos3 == Key.S3) pos3 = 0;
            if (++pos4 == Key.S4) pos4 = 0;
            if (++pos5 == Key.S5) pos5 = 0;
            if (++pos6 == Key.S6) pos6 = 0;
        }

        decryptionValid = true;

    }

    public void updateDecryption(int w1, int p1) {

        if (!decryptionValid) {
            updateDecryption();
            return;
        }

        if (cipherArray == null) {
            return;
        }
        if ((decryption == null) || (decryption.length != cipherArray.length)) {
            updateDecryption();
            return;
        }

        updateDecryptionSelectedPin(w1, p1);

    }

    public void updateDecryption(int w, int p1, int p2) {

        if (!decryptionValid) {
            updateDecryption();
            return;
        }

        if (cipherArray == null) {
            return;
        }
        if ((decryption == null) || (decryption.length != cipherArray.length)) {
            decryption = new int[cipherArray.length];
        }

        updateDecryptionSelectedPin(w, p1);
        updateDecryptionSelectedPin(w, p2);

    }

    private void updateDecryptionSelectedPin(int w, int p) {
        final int len = (cribArray != null) ? cribArray.length : cipherArray.length;
        int wheelSize = WHEELS_SIZE[w];
        int vector;
        int displacement;
        int symbol;
        int pos1 = p, pos2 = p, pos3 = p, pos4 = p, pos5 = p, pos6 = p;
        while (pos1 >= Key.S1) {
            pos1 -= Key.S1;
        }
        while (pos2 >= Key.S2) {
            pos2 -= Key.S2;
        }
        while (pos3 >= Key.S3) {
            pos3 -= Key.S3;
        }
        while (pos4 >= Key.S4) {
            pos4 -= Key.S4;
        }
        while (pos5 >= Key.S5) {
            pos5 -= Key.S5;
        }
        while (pos6 >= Key.S6) {
            pos6 -= Key.S6;
        }

        int slidePlus25 = (slide + 25) % 26;

        for (int pos = p; pos < len; pos += wheelSize) {

            decryptionFrequency[decryption[pos]]--;

            vector = 0;
            if (pins.wheelPins1[pos1]) vector += M1;
            if (pins.wheelPins2[pos2]) vector += M2;
            if (pins.wheelPins3[pos3]) vector += M3;
            if (pins.wheelPins4[pos4]) vector += M4;
            if (pins.wheelPins5[pos5]) vector += M5;
            if (pins.wheelPins6[pos6]) vector += M6;
            //displacement = lugs.displacementVector[vector];

            //symbol = encryptSymbol(cipherArray[pos], displacement);
            symbol = slidePlus25 - cipherArray[pos] + lugs.displacementVector[vector];
            if (symbol >= 26) {
                symbol -= 26;
            }

            decryption[pos] = symbol;
            decryptionFrequency[symbol]++;

            for (pos1 += wheelSize; pos1 >= Key.S1; pos1 -= Key.S1) ;
            for (pos2 += wheelSize; pos2 >= Key.S2; pos2 -= Key.S2) ;
            for (pos3 += wheelSize; pos3 >= Key.S3; pos3 -= Key.S3) ;
            for (pos4 += wheelSize; pos4 >= Key.S4; pos4 -= Key.S4) ;
            for (pos5 += wheelSize; pos5 >= Key.S5; pos5 -= Key.S5) ;
            for (pos6 += wheelSize; pos6 >= Key.S6; pos6 -= Key.S6) ;
        }
    }

    public void setCipherAndCrib(String cipher, String crib) {
        invalidateDecryption();
        setCipher(cipher);

        this.crib = crib;
        cribArray = new int[crib.length()];
        for (int pos = 0; pos < crib.length(); pos++) {
            int ccrib = crib.charAt(pos) - 'A';
            if ((ccrib < 0) || (ccrib > 25)) {
                ccrib = -1;
            }
            cribArray[pos] = ccrib;
        }
    }

    public void setCipher(String cipher) {
        invalidateDecryption();
        this.cipher = cipher;
        cipherArray = new int[cipher.length()];
        for (int pos = 0; pos < cipher.length(); pos++) {
            int ccipher = cipher.charAt(pos) - 'A';
            if ((ccipher < 0) || (ccipher > 25)) {
                ccipher = -1;
            }
            cipherArray[pos] = ccipher;
        }
    }

    public void setOriginalKey(Key originalKey) {
        this.originalKey = new Key(originalKey);
    }

    public void setOriginalScore(long originalScore) {
        this.originalScore = originalScore;
    }

    public int getCountIncorrectPins() {
        if (originalKey == null) {
            return 0;
        }
        int total = 0;
        for (int w = 1; w <= Key.WHEELS; w++) {
            total += getCountIncorrectPins(w);
        }
        return total;
    }

    public int getCountIncorrectPins(int w1) {
        if (originalKey == null) {
            return 0;
        }
        int incorrect = 0;

        for (int i = 0; i < Key.WHEELS_SIZE[w1]; i++) {
            if (pins.isoPins[w1][i] != originalKey.pins.isoPins[w1][i]) {
                incorrect++;
            }
        }

        return Math.min(incorrect, Key.WHEELS_SIZE[w1] - incorrect);
    }

    public int getCountIncorrectLugs() {
        if (originalKey == null) {
            return 0;
        }
        int incorrect = 0;
        int[] typeCount = lugs.createCopy();
        for (int i = 0; i < Lugs.TYPE_COUNT_ARRAY_SIZE; i++) {
            int error = Math.abs(originalKey.lugs.typeCount[i] - typeCount[i]);
            incorrect += error;
        }
        return incorrect / 2;
    }


    private int encryptSymbol(int input, int disp) {
        int val;
        for (val = 25 - input + slide + disp; val >= 26; val -= 26) ;
        return val;
    }

    @Override
    public String toString() {
        return String.format("[Slide %2d] [%s] [%s]", slide, lugs.getLugsString(), pins.absolutePinStringAll01());
    }

}
