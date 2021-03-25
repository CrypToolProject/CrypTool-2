package playfair;

import common.Permutations;
import common.Utils;

public class Playfair {
    static final int DIM = 5;  // DIM = 6 works only with crib.
    static final int[][] PERMUTATIONS = DIM == 5 ? Permutations.PERMUTATIONS5 : Permutations.PERMUTATIONS6;
    static final int ALPHABET_SIZE = DIM == 5 ? 26 : 36;
    static final int SQUARE = DIM * DIM;

    private static final int[][] POSITIONS_OF_PLAINTEXT_SYMBOL_1 = positionsOfPlainTextSymbol1();
    private static final int[][] POSITIONS_OF_PLAINTEXT_SYMBOL_2 = positionsOfPlainTextSymbol2();
    private static final int[][] POSITIONS_OF_CIPHERTEXT_SYMBOL_1 = positionsOfCipherTextSymbol1();
    private static final int[][] POSITIONS_OF_CIPHERTEXT_SYMBOL_2 = positionsOfCipherTextSymbol2();

    private static int row(int pos) {
        return (int) (pos / DIM);
    }
    private static int col(int pos) {
        return (int) (pos % DIM);
    }
    private static int pos(int r, int c) {
        if (r >= DIM) {
            r -= DIM;
        } else if (r < 0) {
            r += DIM;
        }
        if (c >= DIM) {
            c -= DIM;
        } else if (c < 0) {
            c += DIM;
        }
        return (int) (DIM * r + c);
    }
    private static int positionOfPlainTextSymbol1(int cipherPositionOfSymbol1, int cipherPositionOfSymbol2) {
        int c1 = col(cipherPositionOfSymbol1);
        int r1 = row(cipherPositionOfSymbol1);
        int c2 = col(cipherPositionOfSymbol2);
        int r2 = row(cipherPositionOfSymbol2);

        if (r1 == r2) {
            return pos(r1, c1 - 1);
        } else if (c1 == c2) {
            return pos(r1 - 1, c1);
        }
        return pos(r1, c2);
    }

    private static int[][] positionsOfPlainTextSymbol1() {
        int[][] positions = new int[SQUARE][SQUARE];
        for (int p1 = 0; p1 < SQUARE; p1++) {
            for (int p2 = 0; p2 < SQUARE; p2++) {
                positions[p1][p2] = positionOfPlainTextSymbol1(p1, p2);
            }
        }
        return positions;
    }

    private static int positionOfPlainTextSymbol2(int cipherPositionOfSymbol1, int cipherPositionOfSymbol2) {
        int c1 = col(cipherPositionOfSymbol1);
        int r1 = row(cipherPositionOfSymbol1);
        int c2 = col(cipherPositionOfSymbol2);
        int r2 = row(cipherPositionOfSymbol2);

        if (r1 == r2) {
            return pos (r2, c2 - 1);
        } else if (c1 == c2) {
            return pos (r2 - 1, c1);
        }
        return pos(r2, c1);
    }

    private static int[][] positionsOfPlainTextSymbol2() {
        int[][] positions = new int[SQUARE][SQUARE];
        for (int p1 = 0; p1 < SQUARE; p1++) {
            for (int p2 = 0; p2 < SQUARE; p2++) {
                positions[p1][p2] = positionOfPlainTextSymbol2(p1, p2);
            }
        }
        return positions;
    }

    private static int positionOfCipherTextSymbol1(int plainTextPositionOfSymbol1, int plainTextPositionOfSymbol2) {
        int c1 = col(plainTextPositionOfSymbol1);
        int r1 = row(plainTextPositionOfSymbol1);
        int c2 = col(plainTextPositionOfSymbol2);
        int r2 = row(plainTextPositionOfSymbol2);

        if (r1 == r2) {
            return pos(r1, c1 + 1);
        } else if (c1 == c2) {
            return pos(r1 + 1, c1);
        }
        return pos(r1, c2);
    }

    private static int[][] positionsOfCipherTextSymbol1() {
        int[][] positions = new int[SQUARE][SQUARE];
        for (int p1 = 0; p1 < SQUARE; p1++) {
            for (int p2 = 0; p2 < SQUARE; p2++) {
                positions[p1][p2] = positionOfCipherTextSymbol1(p1, p2);
            }
        }
        return positions;
    }

    private static int positionOfCipherTextSymbol2(int plainTextPositionOfSymbol1, int plainTextPositionOfSymbol2) {
        int c1 = col(plainTextPositionOfSymbol1);
        int r1 = row(plainTextPositionOfSymbol1);
        int c2 = col(plainTextPositionOfSymbol2);
        int r2 = row(plainTextPositionOfSymbol2);

        if (r1 == r2) {
            return pos (r2, c2 + 1);
        } else if (c1 == c2) {
            return pos (r2 + 1, c1);
        }
        return pos(r2, c1);
    }

    private static int[][] positionsOfCipherTextSymbol2() {
        int[][] positions = new int[SQUARE][SQUARE];
        for (int p1 = 0; p1 < SQUARE; p1++) {
            for (int p2 = 0; p2 < SQUARE; p2++) {
                positions[p1][p2] = positionOfCipherTextSymbol2(p1, p2);
            }
        }
        return positions;
    }

    static int decrypt(Key key, int[] cipherText, int[] plainText, boolean removeXZ) {

        key.computeInverse();

        int plainTextLength = 0;

        int lastPlainTextSymbol1 = 100;
        int lastPlainTextSymbol2 = 100;
        int plainTextSymbol1, plainTextSymbol2;
        int cipherPositionOfSymbol1, cipherPositionOfSymbol2;

        for (int n = 0; n < cipherText.length; n += 2) {

            cipherPositionOfSymbol1 = key.inverseKey[cipherText[n]];
            cipherPositionOfSymbol2 = key.inverseKey[cipherText[n + 1]];

            plainTextSymbol1 = key.key[POSITIONS_OF_PLAINTEXT_SYMBOL_1[cipherPositionOfSymbol1][cipherPositionOfSymbol2]];
            plainTextSymbol2 = key.key[POSITIONS_OF_PLAINTEXT_SYMBOL_2[cipherPositionOfSymbol1][cipherPositionOfSymbol2]];

            if (removeXZ && (lastPlainTextSymbol1 == plainTextSymbol1 && (lastPlainTextSymbol2 == Utils.X || lastPlainTextSymbol2 == Utils.Z) && plainTextLength > 0)) {
                plainText[plainTextLength - 1] = plainTextSymbol1;
            } else {
                plainText[plainTextLength++] = plainTextSymbol1;
            }

            plainText[plainTextLength++] = plainTextSymbol2;

            lastPlainTextSymbol1 = plainTextSymbol1;
            lastPlainTextSymbol2 = plainTextSymbol2;
        }

        if (removeXZ) {
            while (plainText[plainTextLength - 1] == Utils.X || plainText[plainTextLength - 1] == Utils.Z) {
                plainTextLength--;
            }
        }
        return plainTextLength;
    }

    static int decrypt(Key key, int[] cipherText, int[] plainText, int[] plainTextRemoveNulls) {


        key.computeInverse();

        int plainTextLength = 0;
        int plainTextRemoveNullsLength = 0;

        int lastPlainTextSymbol1 = 100;
        int lastPlainTextSymbol2 = 100;
        int plainTextSymbol1, plainTextSymbol2;
        int cipherPositionOfSymbol1, cipherPositionOfSymbol2;

        for (int n = 0; n < cipherText.length; n += 2) {

            cipherPositionOfSymbol1 = key.inverseKey[cipherText[n]];
            cipherPositionOfSymbol2 = key.inverseKey[cipherText[n + 1]];

            plainTextSymbol1 = key.key[POSITIONS_OF_PLAINTEXT_SYMBOL_1[cipherPositionOfSymbol1][cipherPositionOfSymbol2]];
            plainTextSymbol2 = key.key[POSITIONS_OF_PLAINTEXT_SYMBOL_2[cipherPositionOfSymbol1][cipherPositionOfSymbol2]];

            if ((lastPlainTextSymbol1 == plainTextSymbol1 && (lastPlainTextSymbol2 == Utils.X || lastPlainTextSymbol2 == Utils.Z) && plainTextRemoveNullsLength > 0)) {
                plainTextRemoveNulls[plainTextRemoveNullsLength - 1] = plainText[plainTextLength++] =plainTextSymbol1;
            } else {
                plainTextRemoveNulls[plainTextRemoveNullsLength++] = plainText[plainTextLength++] =plainTextSymbol1;
            }
            plainTextRemoveNulls[plainTextRemoveNullsLength++] = plainText[plainTextLength++] = plainTextSymbol2;

            lastPlainTextSymbol1 = plainTextSymbol1;
            lastPlainTextSymbol2 = plainTextSymbol2;
        }

        while (plainText[plainTextRemoveNullsLength - 1] == Utils.X || plainText[plainTextRemoveNullsLength - 1] == Utils.Z) {
            plainTextRemoveNullsLength--;
        }

        return plainTextRemoveNullsLength;
    }

    static int encrypt(Key key, int[] plainText, int[] cipherText) {

        key.computeInverse();

        int cipherTextLength = 0;

        int cipherTextSymbol1, cipherTextSymbol2;
        int plainTextPositionOfSymbol1, plainTextPositionOfSymbol2;

        for (int n = 0; n < plainText.length; n += 2) {

            plainTextPositionOfSymbol1 = key.inverseKey[plainText[n]];
            if (n == plainText.length - 1 || plainText[n] == plainText[n + 1]) {
                plainTextPositionOfSymbol2 = key.inverseKey[Utils.X];
                n--;
            } else {
                plainTextPositionOfSymbol2 = key.inverseKey[plainText[n + 1]];
            }

            cipherTextSymbol1 = key.key[POSITIONS_OF_CIPHERTEXT_SYMBOL_1[plainTextPositionOfSymbol1][plainTextPositionOfSymbol2]];
            cipherTextSymbol2 = key.key[POSITIONS_OF_CIPHERTEXT_SYMBOL_2[plainTextPositionOfSymbol1][plainTextPositionOfSymbol2]];
            if (cipherTextLength < cipherText.length) {
                cipherText[cipherTextLength++] = cipherTextSymbol1;
                cipherText[cipherTextLength++] = cipherTextSymbol2;
            } else {
                cipherTextLength += 2;
            }

        }

        return cipherTextLength;
    }

    static void preparePlainText(int[] plaintext) {
        if (DIM == 6) {
            return;
        }
        for (int i = 0; i < plaintext.length; i++) {
            if (plaintext[i] == Utils.J) {
                plaintext[i] = Utils.I;
            }
        }
    }

    static String preparePlainText(String p) {
        StringBuilder sb = new StringBuilder();
        if (DIM == 6) {
            p = p.toUpperCase().replaceAll("[^A-Z0-9]*", "");
        } else {
            p = p.toUpperCase().replaceAll("[^A-Z]*", "").replaceAll("J", "I");
        }
        for (int i = 0; i < p.length(); i += 2) {
            sb.append(p.charAt(i));
            if (i + 1 < p.length()) {
                if (p.charAt(i) == p.charAt(i + 1)) {
                    sb.append("X");
                }
                sb.append(p.charAt(i + 1));
            }
        }
        return sb.toString();
    }
}
