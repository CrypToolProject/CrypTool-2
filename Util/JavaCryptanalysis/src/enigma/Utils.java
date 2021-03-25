/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
package enigma;

import common.CtAPI;

import java.io.*;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Random;

class Utils {
    public static byte getIndex(char c) {

        int val;
        if ((c >= 'a') && (c <= 'z'))
            val = c - 'a';
        else if ((c >= 'A') && (c <= 'Z'))
            val = c - 'A';
        else
            val = -1;
        return (byte) val;

    }

    public static int getDigitIndex(char c) {

        int val;
        if ((c >= '0') && (c <= '9'))
            val = c - '0';
        else
            val = -1;
        return val;

    }

    public static char getChar(int i) {

        if ((i >= 0) && (i <= 25))
            return ((char) ('A' + i));
        else
            return '?';
    }

    public static String getString(byte[] crV, int clen) {

        String m = "";

        for (int i = 0; i < clen; i++)
            m += getChar(crV[i]);

        return m;
    }

    public static String getCiphertextStringInGroups(byte[] crV, int clen) {

        String m = "";


        for (int p = 0; p < clen; p++) {
            if ((p % 5) == 0)
                m += " ";
            m += Utils.getChar(crV[p]);
            if ((p % 25) == 24)
                m += "\n";

        }

        return m;

    }

    public static String getCiphertextStringNoXJ(byte[] crV, int clen) {

        String m = "";

        int X = Utils.getIndex('X');
        int J = Utils.getIndex('J');
        int Z = Utils.getIndex('Z');

        for (int i = 0; i < clen; i++) {
            int c = crV[i];
            if ((c == X) || (c == J)) {
                m += " ";
            } else if ((c == Z) && ((i + 1) < clen) && (crV[i + 1] == Z)) {
                m += "  ";
                i++;
            } else {
                m += getChar(c);
            }

        }

        return m;
    }

    public static int getText(String s, byte[] crV) {

        int len = 0;
        for (int i = 0; (i < 1000) && (i < s.length()); i++)
            crV[len++] = (byte) getIndex(s.charAt(i));
        return len;

    }

    private static String estimatedTimeString(long time) {
        Date dtime = new Date(System.currentTimeMillis() + time * 1000);
        SimpleDateFormat hourFt = new SimpleDateFormat("kk:mm:ss");
        SimpleDateFormat dayFt = new SimpleDateFormat("d/M kk:mm:ss");

        String timeString;
        if (time > 3600 * 24)
            timeString = String.format("%d Days %d Hours (%s) ", (time / 3600) / 24, (time / 3600) % 24, dayFt.format(dtime));
        else if (time > 3600)
            timeString = String.format("%.1f Hours (%s) ", time / 3600.0, hourFt.format(dtime));
        else if (time > 60)
            timeString = String.format("%.1f Minutes", time / 60.0);
        else
            timeString = String.format("%.1f Seconds", time / 1.0);

        return timeString;
    }

    public static String getEstimatedTimeString(long cases, int minRate, int maxRate) {

        long minSeconds = cases / maxRate;

        long maxSeconds = cases / minRate;

        if (maxSeconds < 10)
            return "Less than 10 seconds";


        String minS = estimatedTimeString(minSeconds);
        String maxS = estimatedTimeString(maxSeconds);

        return "" + minS + " - " + maxS;
    }

    public static int loadCipherText(String fileName, byte[] text, boolean print) {

        String line;
        int k = 0;

        int lc[] = new int[26];

        try {
            // FileReader reads text files in the default encoding.
            FileReader fileReader = new FileReader(fileName);

            // Always wrap FileReader in BufferedReader.
            BufferedReader bufferedReader = new BufferedReader(fileReader);

            while (((line = bufferedReader.readLine()) != null) && (k < Key.MAXLEN)) {
                for (int i = 0; (i < line.length()) && (k < Key.MAXLEN); i++) {
                    byte index = (byte) Utils.getIndex(line.charAt(i));
                    if (index != -1) {
                        text[k] = index;
                        k++;
                        lc[index]++;
                    }
                }
            }

            // Always close files.
            bufferedReader.close();
        } catch (IOException ex) {
            CtAPI.goodbyeFatalError("Unable to read file %s - %s", fileName, ex.toString());
        }


        if (print)
            CtAPI.printf("Read file %s\n", fileName);

        return k;
    }

    public static int loadRandomText(String fileName, byte[] randomText, int len, boolean generateXs, int garbledLettersPercentage) {

        String file = "";
        String line;
        final int BOOKSIZE = 50000;
        int[] text = new int[BOOKSIZE];
        int k = 0;
        Random random = new Random();

        try {
            // FileReader reads text files in the default encoding.
            FileReader fileReader = new FileReader(fileName);

            // Always wrap FileReader in BufferedReader.
            BufferedReader bufferedReader = new BufferedReader(fileReader);

            while (((line = bufferedReader.readLine()) != null) && (k < BOOKSIZE)) {
                file += line;
                line = line + "X";
                for (int i = 0; (i < line.length()) && (k < BOOKSIZE); i++) {
                    if (line.charAt(i) == ' ')
                        continue;
                    int index = Utils.getIndex(line.charAt(i));
                    if (index == -1)
                        index = Utils.getIndex('X');
                    if (index == Utils.getIndex('X')) {
                        if (k == 0)
                            continue;
                        if (text[k - 1] == Utils.getIndex('X'))
                            continue;
                    }

                    text[k++] = index;

                }
            }

            // Always close files.
            bufferedReader.close();
        } catch (IOException ex) {
            CtAPI.goodbyeFatalError("Failed to read %s - %s", fileName, ex.toString());
        }

        if (len >= k) {
            CtAPI.println("Input file too short to extract " + len + " characters " + fileName + "'");
            return 0;

        }

        int X = Utils.getIndex('X');

        int pos = (int) (System.currentTimeMillis() % (k - len));

        int start = -1;
        for (int i = pos; i >= 0; i--)
            if (i == 0) {
                start = 0;
                break;
            } else if (text[i] == X) {
                start = i + 1;
                break;

            }

        int end = start + len;
        for (int i = k - 1; i > (start + len); i--)
            if (text[i] == X)
                end = i;


        if ((end - start) < len / 2) {
            CtAPI.goodbyeFatalError("Could not create a coherent message (from X to X, or from beginning of a line to end of another line) with " + len + " characters from " + fileName + "'");
        }


        int finalLength = end - start + 1;

        String cleanText = "";
        String garbledText = "";
        for (int i = 0; i < finalLength; i++) {

            int letter = text[start + i];
            cleanText += Utils.getChar(letter);
            if (garbledLettersPercentage > 0) {
                int rand = random.nextInt(100);
                if (rand < garbledLettersPercentage) {
                    rand = random.nextInt(25);
                    letter = (letter + rand) % 26;
                    String garbledLetter = "" + Utils.getChar(letter);
                    garbledText += garbledLetter.toLowerCase();

                } else
                    garbledText += Utils.getChar(letter);

            } else
                garbledText += Utils.getChar(letter);

            randomText[i] = (byte) letter;

        }

        CtAPI.printf("Random text file extracted from %s (length: %d, start: %d, end: %d)\n",
                fileName, finalLength, start, end);
        if (garbledLettersPercentage > 0) {
            CtAPI.printf("Generated %d percent of garbled letters. \nClean Version:\n%s\nWith Garbles:\n%s\n",
                    garbledLettersPercentage, cleanText, garbledText);
        }
        return finalLength;
    }

    public static void saveToFile(String fileName, String string) {

        try {
            // Assume default encoding.
            FileWriter fileWriter = new FileWriter(fileName);
            // Always wrap FileWriter in BufferedWriter.
            BufferedWriter bufferedWriter = new BufferedWriter(fileWriter);
            bufferedWriter.write(string);
            bufferedWriter.newLine();
            bufferedWriter.flush();
            bufferedWriter.close();

        } catch (IOException | NullPointerException ex) {
            CtAPI.goodbyeFatalError("Error writing file %s - %s\n", fileName, ex.toString());
        }

    }

    public static boolean isTextEncrypted(Key key, byte[] text1, int clen, String indicatorS) {

        byte[] text2 = new byte[Key.MAXLEN];
        Key finalKey = new Key(key);
        String indicPlainS;

        if (indicatorS.length() != 0) {
            byte indicCiphertext[] = new byte[3];
            byte indicPlain[] = new byte[3];

            int ilen = Utils.getText(indicatorS, indicCiphertext);
            key.encipherDecipherAll(indicCiphertext, indicPlain, ilen);
            indicPlainS = Utils.getString(indicPlain, ilen);
            finalKey.setMesg(indicPlainS);

        }
        finalKey.encipherDecipherAll(text1, text2, clen);

        long score1 = 0, score2 = 0;
        for (int i = 0; i < clen; i++) {
            for (int j = 0; j < clen; j++) {
                if (text1[i] == text1[j])
                    score1++;
                if (text2[i] == text2[j])
                    score2++;
            }
        }

        return score1 < score2;
    }

} 