package common;


import java.io.*;
import java.util.*;

import static common.CtAPI.printf;

public class Utils {

    public static final String HEXA_FILE = "hexa.bin";
    public static final String NGRAMS7_FILE = "english_7grams.bin";
    public static final String NGRAMS8_FILE = "english_8grams.bin";
    public static final String BOOK_FILE = "book.txt";

    public static final int A = getTextSymbol('A');
    public static final int X = getTextSymbol('X');
    public static final int Z = getTextSymbol('Z');
    public static final int J = getTextSymbol('J');
    public static final int I = getTextSymbol('I');
    public static final int K = getTextSymbol('K');

    static final String TEXT_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    static final int TEXT_ALPHABET_SIZE = TEXT_ALPHABET.length();

    private static int[][] perms6 = createPerms6();

    public static int getTextSymbol(char c) {

        if (c >= 'a' && c <= 'z') {
            return c - 'a';
        }
        if (c >= 'A' && c <= 'Z') {
            return c - 'A';
        }
        return -1;
    }

    public static char getTextChar(int symbol) {

        if ((symbol >= 0) && (symbol <= (TEXT_ALPHABET_SIZE - 1)))
            return (TEXT_ALPHABET.charAt(symbol));
        else
            return '?';

    }

    public static int[] getText(String textString) {
        int[] text = new int[textString.length()];
        int len = 0;
        for (int i = 0; i < textString.length(); i++) {
            int c = getTextSymbol(textString.charAt(i));
            if (c == -1) {
                //continue;
            }
            text[len++] = c;
        }
        return Arrays.copyOf(text, len);
    }

    private static String from = "èéìùòàëáöæëüãþôâäíûóšøůěňïçñíàçèìåáßŕúµýˆ^άλêéąîőčžâªªºžńάλληφοράθęźðöżõřáěšďťˇי".toUpperCase();
    private static String to = "eeiuoaeaoaeuapoaaiuosouenicniaceiaasrupyxxageeaioczaaaoznxxxxxxxxxxzoozoraesdtxe".toUpperCase();

    public static String readTextFile(String fileName) {

        int[] text = new int[1000000];
        String line = "";
        int len = 0;

        try {
            FileReader fileReader = new FileReader(fileName);

            BufferedReader bufferedReader = new BufferedReader(fileReader);

            while ((line = bufferedReader.readLine()) != null) {
                for (char c : line.toCharArray()) {
                    int rep = from.indexOf(c);
                    if (rep != -1) {
                        c = to.charAt(rep);
                    }
                    int index = getTextSymbol(c);
                    if (index != -1) {
                        text[len] = index;
                        len++;
                    }
                }
            }

            bufferedReader.close();
        } catch (IOException ex) {
            CtAPI.goodbyeFatalError("Unable to read text file %s - %s", fileName, ex.toString());
        }

        String cipherStr = getString(Arrays.copyOf(text, len));

        printf("Text file read: %s, length = %d \n%s\n", fileName, len, cipherStr);

        return cipherStr;
    }

    public static int readTextSegmentFromFile(String filename, int startPosition, int[] text) {

        int length = 0;

        try {
            FileReader fileReader = new FileReader(filename);

            BufferedReader bufferedReader = new BufferedReader(fileReader);

            int position = 0;
            String line = "";

            while (((line = bufferedReader.readLine()) != null) && (length < text.length)) {
                if (position > startPosition) {
                    for (int i = 0; i < line.length(); i++) {
                        char c = line.charAt(i);
                        int rep = from.indexOf(c);
                        if (rep != -1) {
                            c = to.charAt(rep);
                        }
                        int index = getTextSymbol(c);
                        if ((index != -1) && (length < text.length)) {
                            text[length] = index;
                            length++;
                        }
                    }
                }
                position += line.length();
            }

            bufferedReader.close();
        } catch (IOException ex) {
            CtAPI.goodbyeFatalError("Unable to read text file %s - %s", filename, ex.toString());
        }
        printf("Read segment from file: %s, Position: %d , Length: %d\n", filename, startPosition, length);
        printf("%s\n\n", getString(text));

        return length;
    }

    public static int readRandomSentenceFromFile(String filename) {


        ArrayList<Set<String>> lists = new ArrayList<>();
        for (int l = 0; l < 10000; l++) {
            lists.add(new HashSet<>());
        }
        try {
            FileReader fileReader = new FileReader(filename);

            BufferedReader bufferedReader = new BufferedReader(fileReader);

            StringBuilder s = new StringBuilder();
            String line = "";

            while ((line = bufferedReader.readLine()) != null) {
                line += ' ';
                for (int i = 0; i < line.length(); i++) {
                    char c = line.charAt(i);
                    int rep = from.indexOf(c);
                    if (rep != -1) {
                        c = to.charAt(rep);
                    }
                    if (c == ' ') {
                        if (s.length() > 0 && s.charAt(s.length() - 1) != ' ') {
                            s.append(c);
                        }
                    } else if (c == '.' || c == ';' || c == ':'|| c == '\"') {
                        if (s.length() >= 6 && s.length() <= 50 && s.charAt(0) >= 'A' && s.charAt(0) <= 'Z') {
                            String clean = s.toString().replaceAll(" ", "").toUpperCase();
                            lists.get(clean.length()).add(clean);
                        }
                        s.setLength(0);
                    } else if (c >= 'a' && c <= 'z') {
                        s.append(c);
                    } else if (c >= 'A' && c <= 'Z') {
                        s.append(c);
                    }
                }

            }

            bufferedReader.close();
        } catch (IOException ex) {
            CtAPI.goodbyeFatalError("Unable to read text file %s - %s", filename, ex.toString());
        }
        for (int l = 0; l < lists.size(); l++) {
            if (lists.get(l).size() > 10) {
                System.out.printf("%,5d %,5d\n", l, lists.get(l).size());
            }
            for (String s : lists.get(l)) {
                int[] t = Utils.getText(s);
                if (t.length >= 6) {
                    long score = Stats.evalPlaintextHexagram(t);
                    System.out.printf("%,5d %s %,d\n", l, s, score);
                }

            }

        }
        return 0;
    }

    public static int[] readRandomSentenceFromFile(String filename, String prefix, int length, boolean playfair) {


        ArrayList<String> list = new ArrayList<>();

        try {
            FileReader fileReader = new FileReader(filename);

            BufferedReader bufferedReader = new BufferedReader(fileReader);

            StringBuilder s = new StringBuilder();
            String line = "";

            while ((line = bufferedReader.readLine()) != null) {
                line += ' ';
                for (int i = 0; i < line.length(); i++) {
                    char c = line.charAt(i);
                    int rep = from.indexOf(c);
                    if (rep != -1) {
                        c = to.charAt(rep);
                    }
                    if (c == ' ') {
                        if (s.length() > 0 && s.charAt(s.length() - 1) != ' ') {
                            s.append(c);
                        }
                    } else if (c == '.' || c == ';' || c == ':'|| c == '\"') {
                        if (s.length() >= 6 && s.length() <= 1000 && s.charAt(0) >= 'A' && s.charAt(0) <= 'Z') {
                            String clean = prefix + s.toString().replaceAll(" ", "").toUpperCase();

                            if (clean.length() == length && (!playfair || !clean.contains("J"))) {
                                int[] t = Utils.getText(clean);
                                long score = Stats.evalPlaintextHexagram(t);
                                if (score > 2_200_000) {
                                    list.add(clean);
                                }
                            }
                        }
                        s.setLength(0);
                    } else if (c >= 'a' && c <= 'z') {
                        s.append(c);
                    } else if (c >= 'A' && c <= 'Z') {
                        s.append(c);
                    }
                }

            }

            bufferedReader.close();
        } catch (IOException ex) {
            CtAPI.goodbyeFatalError("Unable to read file %s - %s", filename, ex.toString());
        }
        if (list.isEmpty()) {
            CtAPI.goodbyeFatalError("Unable to read sentence from text file %s with %d letters", filename, length);
        }
        return Utils.getText(list.get(new Random().nextInt(list.size())));
    }

    public static String getString(int[] text) {
        return getString(text, text.length);
    }

    public static String getString(int[] text, int length) {
        StringBuilder m = new StringBuilder();
        for (int i = 0; i < Math.min(text.length, length); i++) {
            m.append(getTextChar(text[i]));
        }
        return m.toString();
    }

    private static long startTime = System.currentTimeMillis();
    public static long getElapsedMillis() {
        return System.currentTimeMillis() - startTime + 1;
    }

    public static Random random = new Random(startTime);
    public static int randomNextInt(int range) {
        return random.nextInt(range);
    }
    public static int randomNextInt() {
        return random.nextInt();
    }
    public static double randomNextDouble() {
        return random.nextDouble();
    }
    public static float randomNextFloat() {
        return random.nextFloat();
    }

    public static int sum(int[] a) {
        int sum = 0;
        for (int i : a) {
            sum += i;
        }
        return sum;
    }

    public static long sum(long[] a) {
        long sum = 0;
        for (long i : a) {
            sum += i;
        }
        return sum;
    }

    public static boolean in(int x, int... a) {
        for (int i : a) {
            if (i == x) {
                return true;
            }
        }
        return false;
    }

    public static int[] randomPerm6(){
        return perms6[random.nextInt(perms6.length)];
    }

    private static int[][] createPerms6() {
        int[][] perms6 = new int[6 * 5 * 4 * 3 * 2 * 1][6];
        int index = 0;
        for (int i0 = 0; i0 < 6; i0++) {
            for (int i1 = 0; i1 < 6; i1++) {
                if (i1 == i0) {
                    continue;
                }
                for (int i2 = 0; i2 < 6; i2++) {
                    if (i2 == i0 || i2 == i1) {
                        continue;
                    }
                    for (int i3 = 0; i3 < 6; i3++) {
                        if (i3 == i0 || i3 == i1 || i3 == i2) {
                            continue;
                        }
                        for (int i4 = 0; i4 < 6; i4++) {
                            if (i4 == i0 || i4 == i1 || i4 == i2 || i4 == i3) {
                                continue;
                            }
                            for (int i5 = 0; i5 < 6; i5++) {
                                if (i5 == i0 || i5 == i1 || i5 == i2 || i5 == i3 || i5 == i4) {
                                    continue;
                                }

                                perms6[index][0] = i0;
                                perms6[index][1] = i1;
                                perms6[index][2] = i2;
                                perms6[index][3] = i3;
                                perms6[index][4] = i4;
                                perms6[index][5] = i5;
                                index++;

                            }
                        }
                    }
                }
            }
        }
        return perms6;
    }

    public static String readPlaintextSegmentFromFile(String dirname, Language language, int from, int requiredLength, boolean m209) {
        String filename = null;
        switch (language) {
            case ENGLISH:
                filename = "book.txt";
                break;
            case FRENCH:
                filename = "frenchbook.txt";
                break;
            case ITALIAN:
                filename = "italianbook.txt";
                break;
            case GERMAN:
                filename = "germanbook.txt";
                break;
        }
        return readPlaintextSegmentFromFile(dirname + "/" + filename, from, requiredLength, m209);
    }
    // read a plain text segment from a file at a given position and length
    private static String readPlaintextSegmentFromFile(String fileName, int startPosition, int requiredLength, boolean m209) {

        StringBuilder text = new StringBuilder();
        String line;
        int position = 0;
        int fileLength = (int) (new File(fileName).length());
        if (fileLength == 0) {
            CtAPI.goodbyeFatalError("Cannot open file " + fileName);
        }
        if (startPosition < 0) {
            startPosition = randomNextInt(80 * fileLength / 100);
        }

        try {
            // FileReader reads text files in the default encoding.
            FileReader fileReader = new FileReader(fileName);

            // Always wrap FileReader in BufferedReader.
            BufferedReader bufferedReader = new BufferedReader(fileReader);

            while (((line = bufferedReader.readLine()) != null) && (text.length() < requiredLength)) {
                line += " ";
                if (position > startPosition) {
                    //System.out.println(line);
                    line = line.toUpperCase();
                    for (int i = 0; (i < line.length()) && (text.length()  < requiredLength); i++) {
                        char c = line.charAt(i);
                        int rep = from.indexOf(c);
                        if (rep != -1) {
                            c = to.charAt(rep);
                        }
                        if (getTextSymbol(c) == -1) {
                            if (m209) {
                                if (text.length() > 0 && text.charAt(text.length() - 1) == 'Z') {
                                    continue;
                                }
                                c = 'Z';
                            } else {
                                continue;
                            }
                        }
                        text.append(c);
                    }
                }
                position += line.length();
            }

            // Always close files.
            bufferedReader.close();
        } catch (IOException ex) {
            CtAPI.goodbyeFatalError("Unable to read book file %s - %s", fileName, ex.toString());
        }

        printf("Generated Random Plaintext - Book: %s, Position: %d , Length: %d\n", fileName, startPosition, text.length());
        printf("%s\n\n", text.toString().replaceAll("Z"," "));


        return text.toString();
    }
}
