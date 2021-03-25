package multiplex;

import common.Stats;
import common.Utils;

import java.util.Arrays;

abstract class Multiplex {
    final int NUMBER_OF_STRIPS;
    final int STRIP_LENGTH;
    final int NUMBER_OF_STRIPS_USED_IN_KEY;
    private final int[][][] ENCRYPTION_TABLE; // strip, offset, char
    private final int[][][] DECRYPTION_TABLE;

    int[] key;
    private int[] cipher;
    private int[] crib;
    int[] decryption;

    boolean decryptionValid;

    Multiplex(String[] strips, int numberOfStripsInKey) {
        this.NUMBER_OF_STRIPS_USED_IN_KEY = numberOfStripsInKey;
        NUMBER_OF_STRIPS = strips.length;
        STRIP_LENGTH = strips[0].length();
        ENCRYPTION_TABLE = encryptionTable(strips); // strip, offset, char
        DECRYPTION_TABLE = decryptionTable();
        key = new int[NUMBER_OF_STRIPS];
        this.key = new int[NUMBER_OF_STRIPS];
        for (int i = 0; i < NUMBER_OF_STRIPS; i++) {
            this.key[i] = i;
        }
        this.decryptionValid = false;
    }

    abstract int offset(int i);

    abstract String offsetString();

    void setKey(int[] key) {
        System.arraycopy(key, 0, this.key, 0, NUMBER_OF_STRIPS_USED_IN_KEY);
        for (int readyCount = NUMBER_OF_STRIPS_USED_IN_KEY; readyCount < NUMBER_OF_STRIPS; readyCount++) {
            for (int nextValue = 0; nextValue < NUMBER_OF_STRIPS; nextValue++) {
                boolean found = false;
                for (int readyIndex = 0; readyIndex < readyCount; readyIndex++) {
                    if (this.key[readyIndex] == nextValue) {
                        found = true;
                        break;
                    }
                }
                if (!found) {
                    this.key[readyCount] = nextValue;
                    break;
                }
            }
        }
    }

    long score() {
        if (!decryptionValid) {
            decrypt();
        }
        if (crib == null) {
            return Stats.evalPlaintextHexagram(decryption);
        }
        long cribMatch = 0;
        for (int i = 0; i < crib.length; i++) {
            if (crib[i] == decryption[i]) {
                cribMatch++;
            }
        }
        return ((3_000_000 * cribMatch) + (Stats.evalPlaintextHexagram(decryption) * (decryption.length - crib.length))) / decryption.length;
    }

    boolean matchesFullCrib() {
        if (!decryptionValid) {
            decrypt();
        }
        if (crib == null || crib.length != decryption.length) {
            return false;
        }
        for (int i = 0; i < crib.length; i++) {
            if (crib[i] != decryption[i]) {
                return false;
            }
        }
        return true;
    }

    Multiplex setCipher(int[] c) {
        this.cipher = Arrays.copyOf(c, c.length);
        this.decryption = new int[c.length];
        decryptionValid = false;
        return this;
    }

    Multiplex setCrib(String cribS) {
        if (cribS != null && !cribS.isEmpty()) {
            this.crib = Utils.getText(cribS);
        }
        return this;
    }

    Multiplex setCipherAndCrib(int[] c, String cribS) {
        return setCipher(c).setCrib(cribS);
    }
    Multiplex randomizeKey() {
        for (int i = 0; i < NUMBER_OF_STRIPS; i++) {
            this.key[i] = i;
        }
        for (int i = 0; i < NUMBER_OF_STRIPS - 1 - 1; i++) {
            int next = i + 1;
            int j = Utils.randomNextInt(NUMBER_OF_STRIPS - next) + next;
            swapInKey(i, j);
        }
        decryptionValid = false;
        return this;
    }

    Multiplex swapInKey(int i, int j) {
        int temp = key[i];
        key[i] = key[j];
        key[j] = temp;
        if (decryptionValid) {
            if (i < NUMBER_OF_STRIPS_USED_IN_KEY) {
                for (int pi = i; pi < cipher.length; pi += NUMBER_OF_STRIPS_USED_IN_KEY) {
                    decrypt(pi);
                }
            }
            if (j < NUMBER_OF_STRIPS_USED_IN_KEY) {
                for (int pj = j; pj < cipher.length; pj += NUMBER_OF_STRIPS_USED_IN_KEY) {
                    decrypt(pj);
                }
            }
        }
        return this;
    }

    void encrypt(int[] plain, int[] cipher) {
        for (int i = 0; i < plain.length; i++) {
            cipher[i] = ENCRYPTION_TABLE[key[i % NUMBER_OF_STRIPS_USED_IN_KEY]][offset(i)][plain[i]];
        }
    }

    public String toString() {
        StringBuilder str = new StringBuilder(offsetString() + "|");
        for (int i = 0; i < NUMBER_OF_STRIPS_USED_IN_KEY; i++) {
            str.append((i == 0) ? "" : ",");
            str.append(String.format("%02d", key[i]));
        }
        return str.toString();
    }

    private void decrypt() {
        for (int i = 0; i < cipher.length; i++) {
            decrypt(i);
        }
        decryptionValid = true;
    }

    private void decrypt(int i) {
        decryption[i] = DECRYPTION_TABLE[key[i % NUMBER_OF_STRIPS_USED_IN_KEY]][offset(i)][cipher[i]];
    }

    private int[][][] encryptionTable(String[] strips) {
        int[][][] encryptionTable = new int[NUMBER_OF_STRIPS][STRIP_LENGTH][STRIP_LENGTH];
        for (int stripIndex = 0; stripIndex < NUMBER_OF_STRIPS; stripIndex++) {
            for (int plainSymbol = 0; plainSymbol < STRIP_LENGTH; plainSymbol++) {
                int plainCharPos = 0;
                while (Utils.getTextSymbol(strips[stripIndex].charAt(plainCharPos)) != plainSymbol) plainCharPos++;
                for (int offset = 0; offset < STRIP_LENGTH; offset++) {
                    encryptionTable[stripIndex][offset][plainSymbol] = Utils.getTextSymbol(strips[stripIndex].charAt((plainCharPos + offset) % STRIP_LENGTH));
                }
            }
        }
        return encryptionTable;
    }

    private int[][][] decryptionTable() {
        int[][][] decryptionTable = new int[NUMBER_OF_STRIPS][STRIP_LENGTH][STRIP_LENGTH];
        for (int stripIndex = 0; stripIndex < NUMBER_OF_STRIPS; stripIndex++) {
            for (int plainSymbol = 0; plainSymbol < STRIP_LENGTH; plainSymbol++) {
                for (int offset = 0; offset < STRIP_LENGTH; offset++) {
                    decryptionTable[stripIndex][offset][ENCRYPTION_TABLE[stripIndex][offset][plainSymbol]] = plainSymbol;
                }
            }
        }
        return decryptionTable;
    }
}
