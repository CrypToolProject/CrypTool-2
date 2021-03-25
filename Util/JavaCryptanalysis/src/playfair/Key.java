package playfair;

import common.CtAPI;
import common.NGrams;
import common.Stats;
import common.Utils;

import java.util.Arrays;

public class Key {

    int[] key;
    int[] inverseKey;
    private int[] cipher;
    private int[] crib;
    int[] decryptionRemoveNulls;
    int decryptionRemoveNullsLength;
    int[] fullDecryption;
    long score;
    private String keyword;


    Key() {
        key = new int[Playfair.SQUARE];
        inverseKey = new int[Playfair.SQUARE + ((Playfair.DIM == 5) ? 1 : 0)];
        crib = null;
        decryptionRemoveNullsLength = 0;
        fullDecryption = null;
        decryptionRemoveNulls = null;
        score = 0;
        keyword = "";
    }


    void copy(Key key) {
        System.arraycopy(key.key, 0, this.key, 0, Playfair.SQUARE);
    }

    long evalNgrams() {
        decrypt();

        //long ngrams = Stats.evalPlaintextHexagram(decryptionRemoveNulls, decryptionRemoveNullsLength);
        long ngrams = NGrams.eval8(decryptionRemoveNulls, decryptionRemoveNullsLength);
        if (crib == null) {
            score = ngrams;
        } else {
            long cribMismatch = 0;
            for (int i = 0; i < crib.length; i++) {
                if (crib[i] != fullDecryption[i]) {
                    cribMismatch++;
                }
            }
            if (crib.length == cipher.length) {
                return 3_000_000 * (cipher.length - cribMismatch)/cipher.length;
            }
            score = - 100_000 * cribMismatch + ngrams;
        }
        return score;
    }
    long eval() {
        decrypt();

        long ngrams = Stats.evalPlaintextHexagram(decryptionRemoveNulls, decryptionRemoveNullsLength);
        //long ngrams = NGrams.eval7(decryptionRemoveNulls, decryptionRemoveNullsLength);
        if (crib == null) {
            score = ngrams;
        } else {
            long cribMatch = 0;
            for (int i = 0; i < crib.length; i++) {
                if (crib[i] == fullDecryption[i]) {
                    cribMatch++;
                }
            }
            if (crib.length == cipher.length) {
                score = 3_000_000 * cribMatch/cipher.length;
            } else {
                score = ((3_000_000 * cribMatch) + (ngrams * (cipher.length - crib.length))) / cipher.length;
            }
        }
        return score;
    }

    boolean matchesFullCrib() {
        decrypt();
        if (crib == null || crib.length != cipher.length) {
            return false;
        }
        for (int i = 0; i < crib.length; i++) {
            if (crib[i] != fullDecryption[i]) {
                return false;
            }
        }
        return true;
    }

    void setCipher(int[] c) {
        this.cipher = Arrays.copyOf(c, c.length);
        this.decryptionRemoveNulls = new int[c.length];
        this.fullDecryption = new int[c.length];
        this.decryptionRemoveNullsLength = 0;
    }

    void setCrib(String cribS) {
        if (cribS != null && cribS.length() > 1) {
            this.crib = Utils.getText(cribS);
        }
    }

    void computeInverse() {
        Arrays.fill(inverseKey, -1);
        boolean good = true;
        for (int position = 0; position < Playfair.SQUARE; position++) {
            int value = key[position];
            if (value < 0 || value > Playfair.SQUARE || (Playfair.DIM == 5 && value == Utils.J) || inverseKey[value] != -1) {
                good = false;
                break;
            }
            inverseKey[value] = position;
        }
        if (!good) {
            CtAPI.goodbyeFatalError("Invalid key " + toString());
        }
    }

    void random() {
        if (Playfair.DIM == 6) {
            for (int symbol = 0; symbol < Playfair.SQUARE; symbol++) {
                key[symbol] = symbol;
            }
        } else {
            for (int symbol = 0; symbol < Utils.J; symbol++) {
                key[symbol] = symbol;
            }
            for (int symbol = Utils.K; symbol < Playfair.ALPHABET_SIZE; symbol++) {
                key[symbol - 1] = symbol;
            }
        }
        for (int i = 0; i < Playfair.SQUARE - 1; i++) {
            int j = i + Utils.randomNextInt(Playfair.SQUARE - i);
            swap(i, j);
        }
        computeInverse();
    }

    int encrypt(int[] plain, int[] cipher) {
       return Playfair.encrypt(this, plain, cipher);
    }
    int decrypt(int[] cipher, int[] plainRemoveNulls, int[] plain) {
        return Playfair.decrypt(this, cipher, plain, plainRemoveNulls);
    }

    @Override
    public String toString() {
        String s = Utils.getString(key);
        StringBuilder ps = new StringBuilder();
        for (int i = 0; i < Playfair.SQUARE; i += Playfair.DIM ) {
            if (i != 0) {
                ps.append("|");
            }
            ps.append(s.substring(i, i + Playfair.DIM));
        }
        if (keyword.isEmpty()) {
            return ps.toString();
        } else {
            return ps.toString() + " (" + keyword + ")";
        }
    }

    void decrypt() {
        decryptionRemoveNullsLength = Playfair.decrypt(this, cipher, fullDecryption, decryptionRemoveNulls);
    }

    private void swap(int p1, int p2) {
        if (p1 == p2) {
            return;
        }
        int keep = key[p1];
        key[p1] = key[p2];
        key[p2] = keep;
    }

    private void swap(int p1, int p2, int p3) {
        int keep = key[p1];
        key[p1] = key[p2];
        key[p2] = key[p3];
        key[p3] = keep;
    }

    void swap(Key parent, int p1, int p2) {
        copy(parent);
        swap(p1, p2);
    }

    void swap(Key parent, int p1, int p2, int p3) {
        copy(parent);
        swap(p1, p2, p3);
    }

    void swapRows(Key parent,  int r1, int r2) {
        copy(parent);
        int start1 = r1 * Playfair.DIM;
        int start2 = r2 * Playfair.DIM;
        for (int m = 0; m < Playfair.DIM; m++) {
            swap(start1 + m, start2 + m);
        }
    }

    void swapCols(Key parent,  int c1, int c2) {
        copy(parent);
        for (int m = 0; m < Playfair.DIM; m++) {
            swap(c1 + m * Playfair.DIM, c2 + m * Playfair.DIM);
        }
    }

    void permuteCols(Key parent, int perm) {
        copy(parent);
        int[] permute = Playfair.PERMUTATIONS[perm];
        for (int r = 0; r < Playfair.DIM; r++) {
            for (int c = 0; c < Playfair.DIM; c++) {
                key[r * Playfair.DIM + c] = parent.key[r * Playfair.DIM + permute[c]];
            }
        }
    }

    void permuteRowCols(Key parent, int r, int perm) {
        copy(parent);
        int[] permute = Playfair.PERMUTATIONS[perm];
        for (int c = 0; c < Playfair.DIM; c++) {
            key[r * Playfair.DIM + c] = parent.key[r * Playfair.DIM + permute[c]];
        }
    }

    void permuteRows(Key parent, int perm) {
        copy(parent);
        int[] permute = Playfair.PERMUTATIONS[perm];
        for (int r = 0; r < Playfair.DIM; r++) {
            System.arraycopy(parent.key, permute[r] * Playfair.DIM, key, r * Playfair.DIM, Playfair.DIM);
        }
    }

    void permuteColRows(Key parent, int c, int perm) {
        copy(parent);
        int[] permute = Playfair.PERMUTATIONS[perm];
        for (int r = 0; r < Playfair.DIM; r++) {
            key[r * Playfair.DIM + c] = parent.key[permute[r] * Playfair.DIM + c];
        }
    }

    private int[] buffer = new int[Playfair.SQUARE];
    void alignAlphabet() {
        int bestR = -1;
        int bestC = -1;
        int bestCount = 0;
        for (int r = 0; r < Playfair.DIM; r++) {
            for (int c = 0; c < Playfair.DIM; c++) {
                int last = key[r * Playfair.DIM + c];
                int count = 0;
                internal:
                for (int r1 = 0; r1 < Playfair.DIM; r1++) {
                    for (int c1 = 0; c1 < Playfair.DIM; c1++) {
                        int r2 = (r - r1 + Playfair.DIM) % Playfair.DIM;
                        int c2 = (c - c1 + Playfair.DIM) % Playfair.DIM;
                        int previous = key[r2 * Playfair.DIM + c2];
                        if (previous > last) {
                            break internal;
                        }
                        last = previous;
                        count++;
                        //System.out.printf("%2d %2d = %3d\n", r, c, count);
                        if (count > bestCount) {
                            bestCount = count;
                            bestC = c;
                            bestR = r;
                        }
                    }
                }
            }
        }
        //System.out.printf("%2d %2d = %3d\n", bestR, bestC, bestCount);

        System.arraycopy(key, 0, buffer, 0, Playfair.SQUARE);
        for (int r = 0; r < Playfair.DIM; r++) {
            for (int c = 0; c < Playfair.DIM; c++) {
                int r2 = (r - bestR - 1 + Playfair.DIM) % Playfair.DIM;
                int c2 = (c - bestC - 1 + Playfair.DIM) % Playfair.DIM;
                key[r2 * Playfair.DIM + c2] = buffer[r * Playfair.DIM + c];
            }
        }

    }

    boolean keyFromSentence(int[] phrase) {

        boolean[] used = new boolean[26];
        int length = 0;

        for (int symbol : phrase) {
            if (used[symbol]) {
                continue;
            }
            key[length++] = symbol;
            used[symbol] = true;
        }

        for (int symbol = 0; symbol < 26; symbol++) {
            if (used[symbol] || symbol == Utils.getTextSymbol('J')) {
                continue;
            }
            key[length++] = symbol;
        }
        computeInverse();
        keyword = Utils.getString(phrase);
        return length == 25;
    }


    public static void main(String[] args) {
        //VWXQZLMOPYBRUTSANDCEFGHIK
        Key key = new Key();
        key.key = Utils.getText("VWXYZLMOPQBRUTSANDCEFGHIK");
        //key.keyFromSentence(Utils.getText("BRUTUSANDCAESAR"));
        System.out.printf("%s\n", key);
        key.alignAlphabet();
        System.out.printf("%s\n", key);
    }
}
