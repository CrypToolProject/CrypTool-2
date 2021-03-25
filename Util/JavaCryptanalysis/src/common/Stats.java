package common;

import java.io.*;
import java.nio.ByteBuffer;
import java.nio.ShortBuffer;


public class Stats {

    public static long[] monogramStats = new long[Utils.TEXT_ALPHABET_SIZE];
    public static long[] bigramStats = new long[Utils.TEXT_ALPHABET_SIZE * 32];
    private static short[] hexagramStats = null;
    public static long evaluations = 0;

    public static boolean readHexagramStatsFile(String filename) {
        long start = System.currentTimeMillis();

        CtAPI.printf("Loading hexagram stats file %s (%,d free bytes before loading)\n",
                filename, Runtime.getRuntime().freeMemory());

        int totalShortRead = 0;

        try {
            FileInputStream is = new FileInputStream(new File(filename));

            hexagramStats = new short[26 * 26 * 26 * 26 * 26 * 26];

            final int CHUNK_SIZE = 65536;

            short[] hexagramStatsBuffer = new short[CHUNK_SIZE];
            byte[] bytes = new byte[CHUNK_SIZE * 2];

            int read;
            while ((read = is.read(bytes)) > 0) {
                ByteBuffer myByteBuffer = ByteBuffer.wrap(bytes);
                ShortBuffer myShortBuffer = myByteBuffer.asShortBuffer();
                myShortBuffer.get(hexagramStatsBuffer);
                System.arraycopy(hexagramStatsBuffer, 0, hexagramStats, totalShortRead, read / 2);
                totalShortRead += read / 2;
            }
            is.close();

            /*
            int[] hist = new int[100000];
            for (short h : hexagramStats) {
                hist[h]++;
            }
            for (int i = 0; i < hist.length; i++) {
                if (hist[i] > 0) {
                    System.out.printf("%,8d %,10d/%,10d\n", i, hist[i], hexagramStats.length);
                }
            }
            */


        } catch (IOException ex) {
            CtAPI.goodbyeFatalError("Unable to read hexa file %s - %s", filename, ex.toString());
        }
        CtAPI.printf("Hexagram stats file %s loaded successfully (%f seconds), size = %,d bytes (%,d free bytes after loading)\n",
                filename, (System.currentTimeMillis() - start) / 1_000.0, totalShortRead * 2, Runtime.getRuntime().freeMemory());
        CtAPI.println("");
        CtAPI.println("");
        return true;
    }

    private final static int POWER_26_5 = 26 * 26 * 26 * 26 * 26;

    public static long evalPlaintextHexagram(int[] plaintext, int plaintextLength) {

        CtAPI.shutdownIfNeeded();
        Stats.evaluations++;

        int index = (((((((plaintext[0] * 26) + plaintext[1]) * 26) + plaintext[2]) * 26) + plaintext[3]) * 26 + plaintext[4]);
        long val = 0;
        for (int i = 5; i < plaintextLength; i++) {
            index = (index % POWER_26_5) * 26 + plaintext[i];
            val += hexagramStats[index];
        }
        return (val * 1000) / (plaintextLength - 5);

    }

    public static long evalPlaintextHexagram(int[] plaintext) {
        return evalPlaintextHexagram(plaintext, plaintext.length);
    }

    public static String evaluationsSummary() {
        long elapsed = Utils.getElapsedMillis();
        return String.format("[%,d sec.][%,dK decryptions (%,dK/sec.)]", elapsed / 1000, Stats.evaluations / 1000, Stats.evaluations / elapsed);
    }

    static int readBigramFile(String fileName) {

        String line = "";
        int items = 0;

        try {
            FileReader fileReader = new FileReader(fileName);

            BufferedReader bufferedReader = new BufferedReader(fileReader);

            while ((line = bufferedReader.readLine()) != null) {

                line = line.toUpperCase();
                String[] split = line.split("[ ]+");
                int l1 = Utils.TEXT_ALPHABET.indexOf(split[0].charAt(0));
                int l2 = Utils.TEXT_ALPHABET.indexOf(split[0].charAt(1));
                if (l1 < 0 || l2 < 0) {
                    continue;
                }
                long freq = Long.valueOf(split[1]);

                bigramStats[(l1 << 5) + l2] += freq;
                items++;
            }

            bufferedReader.close();
        } catch (IOException ex) {
            CtAPI.goodbyeFatalError("Unable to read bigram file %s - %s", fileName, ex.toString());
        }

        CtAPI.printf("Bigram file read: %s, items  = %d  \n", fileName, items);

        convertToLog(bigramStats);

        return items;

    }

    static int readMonogramFile(String fileName, boolean m209) {

        String line;
        int items = 0;

        try {
            FileReader fileReader = new FileReader(fileName);
            BufferedReader bufferedReader = new BufferedReader(fileReader);

            while ((line = bufferedReader.readLine()) != null) {

                line = line.toUpperCase();
                String[] split = line.split("[ ]+");
                int l1 = Utils.TEXT_ALPHABET.indexOf(split[0].charAt(0));
                if (l1 < 0) {
                    continue;
                }
                long freq = Long.valueOf(split[1]);

                monogramStats[l1] += freq;
                items++;
            }

            bufferedReader.close();
        } catch (IOException ex) {
            CtAPI.goodbyeFatalError("Unable to read mono file %s - %s", fileName, ex.toString());
        }

        CtAPI.printf("mono file read: %s, items  = %d  \n", fileName, items);

        convertToLog(monogramStats);

        return items;

    }

    static int readFileForStats(String fileName, boolean m209) {


        String line;
        int length = 0;
        String from = "èéìùòàëáöæëüãþôâäíûóšøůěňïçñíàçèìåáßŕúµýˆ^άλêéąîőčžâªªºžńάλληφοράθęźðöżõřáěšďťˇי".toUpperCase();
        String to = "eeiuoaeaoaeuapoaaiuosouenicniaceiaasrupyxxageeaioczaaaoznxxxxxxxxxxzoozoraesdtxe".toUpperCase();


        try {
            FileReader fileReader = new FileReader(fileName);

            BufferedReader bufferedReader = new BufferedReader(fileReader);
            int l2 = -1;
            while ((line = bufferedReader.readLine()) != null) {

                for (char c : line.toUpperCase().toCharArray()) {

                    if (m209) {
                        if (c == ' ' || c == ',' || c == '.') {
                            c = 'Z';
                        }
                    }

                    int rep = from.indexOf(c);
                    if (rep != -1) {
                        c = to.charAt(rep);
                    }
                    int l1 = l2;
                    l2 = Utils.TEXT_ALPHABET.indexOf(c);
                    if (l1 != -1 && l2 != -1) {
                        monogramStats[l1]++;
                        bigramStats[(l1 << 5) + l2]++;
                        length++;
                    }
                }
            }

            // Always close files.
            bufferedReader.close();
        } catch (IOException ex) {
            CtAPI.goodbyeFatalError("Unable to read text file for stats  %s - %s", fileName, ex.toString());
        }

        convertToLog(bigramStats);
        convertToLog(monogramStats);

        CtAPI.printf("Text file read for stats %s, length = %d \n", fileName, length);

        return length;
    }

    private static void convertToLog(long[] stats) {
        long minVal = Long.MAX_VALUE;
        for (long stat : stats) {
            if ((stat > 0) && (stat < minVal)) {
                minVal = stat;
            }
        }

        for (int i = 0; i < stats.length; i++) {
            if (stats[i] > 0) {
                stats[i] = (long) (10000.0 * Math.log((1.0 * stats[i]) / (1.0 * minVal)));
            }
        }

    }

    public static boolean load(String dirname, Language language, boolean m209) {
        int n = 1;
        switch (language) {
            case ENGLISH:
                //n *= readFileForStats("book.txt", m209);
                n *= readBigramFile(dirname + "/" + "english_bigrams.txt");
                n *= readMonogramFile(dirname + "/" + "english_monograms.txt", m209);
                break;
            case FRENCH:
                n *= readBigramFile(dirname + "/" + "french_bigrams.txt");
                n *= readMonogramFile(dirname + "/" + "french_monograms.txt", m209);
                break;
            case ITALIAN:
                n *= readFileForStats(dirname + "/" + "italianbook.txt", m209);
                break;
            case GERMAN:
                n *= readFileForStats(dirname + "/" + "germanbook.txt", m209);
                //n *= readBigramFile(dirname + "/" + "german_bigrams.txt");
                //n *= readMonogramFile(dirname + "/" + "german_monograms.txt", m209);
                break;
        }
        if (m209) {
            monogramStats['E' - 'A'] = Math.max(60000, monogramStats['E' - 'A']);
            monogramStats['Z' - 'A'] = Math.max(80000, monogramStats['Z' - 'A']);
        }

        if (n == 0) {
            CtAPI.goodbyeFatalError("Cannot load stats - language: " + language);
        }
        return true;
    }

}


