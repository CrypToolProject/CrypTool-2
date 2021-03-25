package enigma;

import common.CtAPI;

import java.io.BufferedReader;
import java.io.FileReader;
import java.io.IOException;

class Stats {

    public static final int[] triflat = new int[32 * 32 * 32];
    public static final int[] biflat = new int[32 * 32];
    public static final int unidict[] = {609, 220, 72, 290, 1291, 303, 281, 188, 616, 41, 199, 390, 272,
            841, 442, 147, 202, 687, 623, 541, 447, 138, 168, 698, 89, 205};

    private static double triMult = 0;
    private static boolean newTrigrams = false;


    public static void unidictConvertToLog() {
        int minUni = Integer.MAX_VALUE;

        for (int i = 0; i < 26; i++)
            minUni = Math.min(unidict[i], minUni);

        for (int i = 0; i < 26; i++)
            unidict[i] = (int) (10000.0 * Math.log((Math.E * unidict[i]) / minUni));


    }

    public static int loadBidictConvertToLog(String fileName, boolean print) {

        String line;
        int items = 0;


        for (int l1 = 0; l1 < 26; l1++) {

            for (int l2 = 0; l2 < 26; l2++) {
                //bidict[l1][l2] =0;
                biflat[(l1 << 5) + l2] = 0;

            }
        }


        try {
            // FileReader reads text files in the default encoding.
            FileReader fileReader = new FileReader(fileName);

            // Always wrap FileReader in BufferedReader.
            BufferedReader bufferedReader = new BufferedReader(fileReader);

            while (((line = bufferedReader.readLine()) != null) && (line.length() > 0) && (items < 400000)) {


                int freq = 0;
                int l1 = Utils.getIndex(line.charAt(0));
                int l2 = Utils.getIndex(line.charAt(1));
                if ((l1 == -1) || (l2 == -1))
                    continue;
                for (int i = 3; i < line.length(); i++)
                    freq = freq * 10 + Utils.getDigitIndex(line.charAt(i));

                //bidict[l1][l2] = freq;
                biflat[(l1 << 5) + l2] = freq;
                items++;
                //CtAPI.printf("Bigram %d (%c) %d (%c) = %d \n",l1, line.charAt(0),l2, line.charAt(1),freq);
            }

            // Always close files.
            bufferedReader.close();
        } catch (IOException ex) {
            CtAPI.goodbyeFatalError("Unable to read bigram file %s - %s", fileName, ex.toString());
        }

        if (print)
            CtAPI.printf("Bigram file read: %s, items  = %d, converted to log frequencies  \n", fileName, items);

        long minbi = 1000000000;
        for (int l1 = 0; l1 < 26; l1++) {
            for (int l2 = 0; l2 < 26; l2++) {
                //long currbi= bidict[l1][l2];
                long currbi = biflat[(l1 << 5) + l2];
                if ((currbi != 0) && (currbi < minbi))
                    minbi = currbi;

            }
        }
        for (int l1 = 0; l1 < 26; l1++) {
            for (int l2 = 0; l2 < 26; l2++) {
                long currbi = biflat[(l1 << 5) + l2];
                if (currbi != 0) {
                     biflat[(l1 << 5) + l2] = (int) (10000.0 * Math.log((Math.E * currbi) / minbi));
                }
            }
        }

        return items;

    }

    public static int loadTridictConvertToLog(String fileName, boolean print) {

        String line;
        int items = 0;

        for (int l1 = 0; l1 < 26; l1++) {
            for (int l2 = 0; l2 < 26; l2++) {
                for (int l3 = 0; l3 < 26; l3++) {
                    triflat[triIndex(l1, l2, l3)] = 0;
                }
            }
        }

        try {
            // FileReader reads text files in the default encoding.
            FileReader fileReader = new FileReader(fileName);

            // Always wrap FileReader in BufferedReader.
            BufferedReader bufferedReader = new BufferedReader(fileReader);
            int count = 0;
            while (((line = bufferedReader.readLine()) != null) && (line.length() > 0) && (count++ < 20000000)) {
                int freq = 0;
                int l1 = Utils.getIndex(line.charAt(0));
                int l2 = Utils.getIndex(line.charAt(1));
                int l3 = Utils.getIndex(line.charAt(2));
                if ((l1 == -1) || (l2 == -1) || (l3 == -1))
                    continue;
                for (int i = 4; i < line.length(); i++)
                    freq = freq * 10 + Utils.getDigitIndex(line.charAt(i));

                triflat[triIndex(l1, l2, l3)] = freq;

                items++;
            }

            // Always close files.
            bufferedReader.close();
        } catch (IOException ex) {
            CtAPI.goodbyeFatalError("Unable to read trigram file %s - %s", fileName, ex.toString());
        }

        if (print)
            CtAPI.printf("Trigram file read: %s, items  = %d, converted to log frequencies  \n", fileName, items);

        long mintri = 1000000000;
        for (int l1 = 0; l1 < 26; l1++) {
            for (int l2 = 0; l2 < 26; l2++) {
                for (int l3 = 0; l3 < 26; l3++) {
                    long currtri = triflat[triIndex(l1, l2, l3)];
                    if ((currtri != 0) && (currtri < mintri))
                        mintri = currtri;
                }
            }
        }
        for (int l1 = 0; l1 < 26; l1++) {
            for (int l2 = 0; l2 < 26; l2++) {
                for (int l3 = 0; l3 < 26; l3++) {
                    long currtri = triflat[triIndex(l1, l2, l3)];
                    if (currtri != 0) {

                        triflat[triIndex(l1, l2, l3)] = (int) (10000.0 * Math.log((Math.E * currtri) / mintri));

                    }
                }
            }
        }


        return items;

    }

    public static int loadTridict(String fileName) {

        newTrigrams = fileName.endsWith("3WH.txt");

        long minNonZero = Long.MAX_VALUE;

        String line;
        int items = 0;
        try {
            // FileReader reads text files in the default encoding.
            FileReader fileReader = new FileReader(fileName);

            // Always wrap FileReader in BufferedReader.
            BufferedReader bufferedReader = new BufferedReader(fileReader);
            while (((line = bufferedReader.readLine()) != null)) {
                int freq = 0;
                int l1 = Utils.getIndex(line.charAt(0));
                int l2 = Utils.getIndex(line.charAt(1));
                int l3 = Utils.getIndex(line.charAt(2));
                if ((l1 == -1) || (l2 == -1) || (l3 == -1))
                    continue;
                for (int i = 4; i < line.length(); i++) {
                    int dig = Utils.getDigitIndex(line.charAt(i));
                    if (dig != -1)
                        freq = freq * 10 + dig;
                }

                items++;
                triflat[triIndex(l1, l2, l3)] = freq;
                if ((freq > 0) && (freq < minNonZero))
                    minNonZero = freq;

            }

            // Always close files.
            bufferedReader.close();
        } catch (IOException ex) {
            CtAPI.goodbyeFatalError("Unable to read trigram file %s - %s", fileName, ex.toString());
        }


        if (minNonZero < 1000) {
            triMult = (newTrigrams? 1500.0 : 1000.0)/minNonZero;
            //triMult = 1;
            for (int l1 = 0; l1 < 26; l1++) {
                for (int l2 = 0; l2 < 26; l2++) {
                    for (int l3 = 0; l3 < 26; l3++) {
                        int freq = triflat[triIndex(l1, l2, l3)];
                        if (freq != 0) {
                            freq = (int) (freq * triMult);
                            triflat[triIndex(l1, l2, l3)] = freq;
                        }
                    }
                }
            }
        }

        CtAPI.printf("Trigram file read: %s  (%,d items)\n", fileName, items);


        return 1;

    }

    public static double triSchwelle(double length) {
        //return triMult * 70 * Math.sqrt(length) / (length - 2);
        //return triMult * 70 * Math.sqrt(length);

        if (newTrigrams) {
            if (length <= 50) {
                return 13000;
            }
            if (length <= 75) {
                return 11000;
            }
            if (length <= 100) {
                return 11000;
            }
            return 10000;


        }

        return 10000;
    }

    private static int triIndex(int l1, int l2, int l3) {
        return (((l1 << 5) + l2) << 5) + l3;
    }

    public static int loadBidict(String fileName) {

        long minNonZero = Long.MAX_VALUE;
        String line;
        try {
            // FileReader reads text files in the default encoding.
            FileReader fileReader = new FileReader(fileName);

            // Always wrap FileReader in BufferedReader.
            BufferedReader bufferedReader = new BufferedReader(fileReader);
            while (((line = bufferedReader.readLine()) != null)) {
                int freq = 0;
                int l1 = Utils.getIndex(line.charAt(0));
                int l2 = Utils.getIndex(line.charAt(1));
                if ((l1 == -1) || (l2 == -1))
                    continue;
                for (int i = 3; i < line.length(); i++) {
                    int dig = Utils.getDigitIndex(line.charAt(i));
                    if (dig != -1)
                        freq = freq * 10 + dig;
                }


                //bidict[l1][l2] = freq;
                biflat[(l1 << 5) + l2] = freq;
                if ((freq > 0) && (freq < minNonZero))
                    minNonZero = freq;

            }

            // Always close files.
            bufferedReader.close();
        } catch (IOException ex) {
            CtAPI.goodbyeFatalError("Unable to read bigram file %s - %s", fileName, ex.toString());
        }


        if (minNonZero < 1000) {
            for (int l1 = 0; l1 < 26; l1++)
                for (int l2 = 0; l2 < 26; l2++) {
                    long freq = biflat[(l1 << 5) + l2];
                    if (freq != 0) {
                        if (freq == minNonZero) {
                            freq = 1000;
                        } else {
                            freq = (freq * 1000) / minNonZero;
                        }
                    }  //do nothing

                    biflat[(l1 << 5) + l2] = (int) freq;
                }
        }
        CtAPI.printf("Bigram file read: %s  \n", fileName);

        return 1;

    }
}

