package enigma;

import common.*;

import java.util.Random;

public class Main {
    private enum Mode {
        HILLCLIMBING,
        ANNEALING,
        OSTWALD,
        IC,
        TRIGRAMS,
        BOMBE,
        INDICATORS,
        INDICATORS1938,
        SCENARIO,
        DECRYPT
    }

    public static void main(String[] args) {

        createCommandLineArguments();
        //Argument.printUsage();
        CtBestList.setDiscardSamePlaintexts(false);
        CtBestList.setThrottle(false);
        CtBestList.setSize(10);

        CtAPI.openAndReadInputValues("Enigma attacks", "1.0");

        CommandLine.parseAndPrintCommandLineArgs(args);

        final String RESOURCE_PATH = CommandLine.getStringValue(Flag.RESOURCE_PATH);
        final String SCENARIO_PATH = CommandLine.getStringValue(Flag.SCENARIO_PATH);
        final int HC_SA_CYCLES = CommandLine.getIntegerValue(Flag.HC_SA_CYCLES);
        final int THREADS = CommandLine.getIntegerValue(Flag.THREADS);
        final String CRIB = CommandLine.getStringValue(Flag.CRIB);
        String CIPHERTEXT = CommandLine.getStringValue(Flag.CIPHERTEXT);
        if (CIPHERTEXT.endsWith("txt")) {
            CIPHERTEXT = common.Utils.readTextFile(CIPHERTEXT).toUpperCase().replaceAll("[^A-Z]", "");
        }

        final Key.Model MODEL = Key.Model.valueOf(CommandLine.getStringValue(Flag.MODEL));
        final Language LANGUAGE = Language.valueOf(CommandLine.getStringValue(Flag.LANGUAGE));
        final String INDICATORS_FILE = CommandLine.getStringValue(Flag.INDICATORS_FILE);
        final String KEY = CommandLine.getStringValue(Flag.KEY);
        final String MESSAGE_INDICATOR = CommandLine.getStringValue(Flag.MESSAGE_INDICATOR);
        final String SCENARIO = CommandLine.getStringValue(Flag.SCENARIO);

        final int STRENGTH = CommandLine.getIntegerValue(Flag.STRENGTH);
        final int RIGHT_RING_SAMPLING = CommandLine.getIntegerValue(Flag.RIGHT_RING_SAMPLING);
        final MRingScope MIDDLE_RING_SCOPE = MRingScope.valueOf(CommandLine.getIntegerValue(Flag.MIDDLE_RING_SCOPE));
        final boolean VERBOSE = CommandLine.getBooleanValue(Flag.VERBOSE);
        final String CRIB_POSITION = CommandLine.getStringValue(Flag.CRIB_POSITION);
        final Mode MODE = Mode.valueOf(CommandLine.getStringValue(Flag.MODE));

        String[] keyAndStecker = KEY.split("[|]");
        String steckerS = (keyAndStecker.length == 2) ? keyAndStecker[1] : "";
        String[] keyParts = keyAndStecker[0].split("[\\-]");
        boolean range = keyParts.length == 2;
        String rangeLowS = keyParts[0];
        String rangeHighS = range ? keyParts[1] : "";

        String keyS = range ? "" : rangeLowS;

        String indicatorS = "";
        String indicatorMessageKeyS = "";

        switch (MODE) {
            case HILLCLIMBING:
                required(MODE, new Flag[]{Flag.CIPHERTEXT});
                incompatible(MODE, new Flag[]{Flag.SCENARIO, Flag.CRIB, Flag.CRIB_POSITION, Flag.INDICATORS_FILE});
                break;
            case IC:
                required(MODE, new Flag[]{Flag.CIPHERTEXT});
                incompatible(MODE, new Flag[]{Flag.SCENARIO, Flag.CRIB, Flag.CRIB_POSITION, Flag.INDICATORS_FILE, Flag.STRENGTH});
                break;
            case TRIGRAMS:
                required(MODE, new Flag[]{Flag.CIPHERTEXT});
                incompatible(MODE, new Flag[]{Flag.SCENARIO, Flag.CRIB, Flag.CRIB_POSITION, Flag.INDICATORS_FILE, Flag.STRENGTH});
                break;
            case BOMBE:
                required(MODE, new Flag[]{Flag.CIPHERTEXT, Flag.CRIB});
                incompatible(MODE, new Flag[]{Flag.SCENARIO, Flag.INDICATORS_FILE, Flag.STRENGTH});
                break;
            case INDICATORS:
                required(MODE, new Flag[]{Flag.INDICATORS_FILE});
                incompatible(MODE, new Flag[]{Flag.SCENARIO, Flag.CRIB, Flag.CRIB_POSITION, Flag.MESSAGE_INDICATOR, Flag.STRENGTH});
                required(MODE, MODEL, new Key.Model[]{Key.Model.H});
                break;
            case INDICATORS1938:
                required(MODE, new Flag[]{Flag.INDICATORS_FILE});
                incompatible(MODE, new Flag[]{Flag.SCENARIO, Flag.CRIB, Flag.CRIB_POSITION, Flag.MESSAGE_INDICATOR, Flag.MIDDLE_RING_SCOPE, Flag.RIGHT_RING_SAMPLING, Flag.STRENGTH});
                required(MODE, MODEL, new Key.Model[]{Key.Model.H});
                break;
            case SCENARIO:
                required(MODE, new Flag[]{Flag.SCENARIO});
                incompatible(MODE, new Flag[]{Flag.LANGUAGE, Flag.HC_SA_CYCLES, Flag.CRIB, Flag.CRIB_POSITION, Flag.INDICATORS_FILE, Flag.MESSAGE_INDICATOR, Flag.MIDDLE_RING_SCOPE, Flag.RIGHT_RING_SAMPLING, Flag.STRENGTH});
                break;
            case DECRYPT:
                incompatible(MODE, new Flag[]{Flag.CRIB, Flag.CRIB_POSITION, Flag.INDICATORS_FILE, Flag.MESSAGE_INDICATOR, Flag.MIDDLE_RING_SCOPE, Flag.RIGHT_RING_SAMPLING, Flag.STRENGTH});
                break;
        }


        if (range) {
            incompatibleWithRangeOkKeys(MODE, new Mode[]{Mode.DECRYPT});
            incompatibleWithRangeOkKeys(new Flag[]{});
        } else {
            incompatibleWithSingleKey(new Flag[]{Flag.MIDDLE_RING_SCOPE, Flag.RIGHT_RING_SAMPLING});
            incompatibleWithSingleKey(MODE, new Mode[]{Mode.IC, Mode.TRIGRAMS, Mode.INDICATORS, Mode.INDICATORS1938});
        }


        if (!MESSAGE_INDICATOR.isEmpty()) {
            Key dummyKey = new Key();
            boolean indicatorError = false;
            switch (MESSAGE_INDICATOR.length()) {
                case 3:
                    dummyKey.initDefaults(Key.Model.H);
                    indicatorS = MESSAGE_INDICATOR;
                    if (dummyKey.setMesg(indicatorS) != 1)
                        indicatorError = true;
                    break;
                case 4:
                    dummyKey.initDefaults(Key.Model.M4);
                    indicatorS = MESSAGE_INDICATOR;
                    if (dummyKey.setMesg(indicatorS) != 1)
                        indicatorError = true;
                    break;
                case 7:
                    dummyKey.initDefaults(Key.Model.H);
                    indicatorMessageKeyS = MESSAGE_INDICATOR.substring(0, 3);
                    indicatorS = MESSAGE_INDICATOR.substring(4, 7);
                    if (dummyKey.setMesg(indicatorS) != 1)
                        indicatorError = true;
                    if (dummyKey.setMesg(indicatorMessageKeyS) != 1)
                        indicatorError = true;
                    break;
                case 9:
                    dummyKey.initDefaults(Key.Model.M4);
                    indicatorMessageKeyS = MESSAGE_INDICATOR.substring(0, 4);
                    indicatorS = MESSAGE_INDICATOR.substring(5, 9);
                    if (dummyKey.setMesg(indicatorS) != 1)
                        indicatorError = true;
                    if (dummyKey.setMesg(indicatorMessageKeyS) != 1)
                        indicatorError = true;
                    break;

                default:
                    indicatorError = true;
                    break;
            }

            if (indicatorError) {
                CtAPI.goodbyeFatalError("Invalid Indicator (-%s): Either XXX or XXX:YYY for Model H/M3, or XXXX:YYYY for Model M4\n", Flag.MESSAGE_INDICATOR);
            } else if (indicatorMessageKeyS.length() == 0) { // xxx format
                if (range) {
                    CtAPI.goodbyeFatalError("Invalid Indicator: When range of keys selected, then only -%s XXX:YYY or XXXX:YYYY (for M4) is allowed \n", Flag.MESSAGE_INDICATOR);
                }
            } else {// xxx:yyy format
                if (!range) {
                    CtAPI.goodbyeFatalError("Invalid Indicator (-w): If single key selected, then only -%s XXX (or XXXX for M4) is allowed \n", Flag.MESSAGE_INDICATOR);
                }
            }
        }

        int res;

        byte plaintext[] = new byte[Key.MAXLEN];
        byte ciphertext[] = new byte[Key.MAXLEN];
        int clen = 0;
        Key lowKey = new Key();
        Key highKey = new Key();
        Key key = new Key();

        String trigramFile = "enigma_logtrigrams.txt";
        String bigramFile = "enigma_logbigrams.txt";
        //String trigramFile = "3WH.txt";
        //String bigramFile = "2WH.txt";
        if (LANGUAGE == Language.ENGLISH) {
            bigramFile = "english_logbigrams.txt";
            trigramFile = "english_logtrigrams.txt";
        }
        bigramFile = RESOURCE_PATH +"\\" +bigramFile;
        trigramFile = RESOURCE_PATH +"\\" +trigramFile;
        res = Stats.loadTridict(trigramFile);
        if (res != 1) {
            CtAPI.goodbyeFatalError("Load (log) trigrams file %s failed\n", trigramFile);
        }
        res = Stats.loadBidict(bigramFile);
        if (res != 1) {
            CtAPI.goodbyeFatalError("Load (log) bigrams file %s failed\n", bigramFile);
        }

        if (!CommandLine.isSet(Flag.SCENARIO)) {
            clen = Utils.getText(CIPHERTEXT, ciphertext);
//            for (int i = 0; i < clen; i++) {
//                ciphertext[i] = (byte) (new Random()).nextInt(26);
//            }
            CtAPI.printf("Ciphertext (Length = %d) %s\n", clen, CIPHERTEXT);
        }

        if (!range) {
            res = key.setKey(keyS, MODEL, false);
            if (res != 1) {
                CtAPI.goodbyeFatalError("Invalid key: %s\n", keyS);
            }

            res = key.setStecker(steckerS);
            if (res != 1) {
                CtAPI.goodbyeFatalError("invalid stecker board settings: %s - Should include pairs of letters with no repetitions, or may be omitted\n", steckerS);
            }

            if (indicatorS.length() != 0) {
                Key dumpKey = new Key(key);
                res = dumpKey.setMesg(indicatorS);
                if (res == 0) {
                    CtAPI.goodbyeFatalError("Invalid message message_indicator: %s \n", indicatorS);
                }
                if (steckerS.length() == 0) {
                    CtAPI.goodbyeFatalError("Stecker board mandatory when -%s is specified\n", Flag.MESSAGE_INDICATOR);
                }
            }

        } else {

            String fminS;
            String tmaxS;
            switch (MODEL) {
                case M3:
                    fminS = "B:111:AAA:AAA";
                    tmaxS = "C:888:ZZZ:ZZZ";
                    break;
                case M4:
                    fminS = "B:B111:AAAA:AAAA";
                    tmaxS = "C:G888:ZZZ:ZZZZ";

                    break;
                case H:
                default:
                    fminS = "A:111:AAA:AAA";
                    tmaxS = "C:555:ZZZ:ZZZ";
                    break;
            }

            if (rangeLowS.length() == 0)
                rangeLowS = fminS;

            if (rangeHighS.length() == 0)
                rangeHighS = tmaxS;


            res = Key.setRange(lowKey, highKey, rangeLowS, rangeHighS, MODEL);
            if (res != 1) {
                CtAPI.goodbyeFatalError("Invalid key range:  %s-%s  - Invalid key format, or first has higher value than last \n", rangeLowS, rangeHighS);
            }

            if ((lowKey.lRing != highKey.lRing) && (indicatorS.length() == 0) && (MODE == Mode.HILLCLIMBING || MODE == Mode.ANNEALING || MODE == Mode.OSTWALD))
                System.out.print("\n\n\nWARNING: Setting a range (different values) for the Left Ring settings is usually not necessary and will significant slow Hill Climbing searche\n\n\n");

            if (steckerS.length() != 0) {
                res = lowKey.setStecker(steckerS) * highKey.setStecker(steckerS);
                if (res != 1) {
                    CtAPI.goodbyeFatalError("Invalid steckers: %s - Should include pairs of letters with no repetitions, or be omitted\n", steckerS);
                }
            }


            if ((indicatorS.length() != 0) || (indicatorMessageKeyS.length() != 0)) {


                if ((indicatorS.length() == 0) || (indicatorMessageKeyS.length() == 0)) {
                    CtAPI.goodbyeFatalError("Invalid message_indicator (-%s) - Only XXX:YYY (or XXXX:YYYY for M4), which must include the Message Key for encrypting the Indicator, is allowed for this mode %s\n",
                            Flag.MESSAGE_INDICATOR, MODE);
                }
                Key tempKey = new Key(lowKey);
                res = tempKey.setMesg(indicatorS);
                if (res == 0) {
                    CtAPI.goodbyeFatalError("Invalid message indicator (-%s): %s \n", Flag.MESSAGE_INDICATOR, indicatorS);
                }
                res = tempKey.setMesg(indicatorMessageKeyS);
                if (res == 0) {
                    CtAPI.goodbyeFatalError("Invalid message key for message_indicator (-%s): %s \n", Flag.MESSAGE_INDICATOR, indicatorMessageKeyS);
                }
                if (steckerS.length() == 0) {
                    CtAPI.goodbyeFatalError("Stecker board settings mandatory for -%s  \n", Flag.MESSAGE_INDICATOR);
                }
                if (HC_SA_CYCLES > 0) {
                    CtAPI.goodbyeFatalError("Invalid settings - When specifying -%s , -%s 0 (no hillclimbing/simulated annealing on search results) must also be selected. \n",
                            Flag.MESSAGE_INDICATOR, Flag.HC_SA_CYCLES);
                }
            }

            if (MIDDLE_RING_SCOPE != MRingScope.ALL) {
                boolean fullRangeMRing = (lowKey.mRing == Utils.getIndex('A')) && (highKey.mRing == Utils.getIndex('Z'));
                if (!fullRangeMRing) {
                    CtAPI.goodbyeFatalError("Range of middle ring (%s to %s) imcompatible with -%s %d selection: Only -%s 0 (or not specifying -%s and leaving the default 0) is allowed \n" +
                                    " when specifying a partial range. Use range from A to Z for the middle ring, e.g. -%s b:231:aaa:aaa-b:231:azz:zzz, or use -%s 0.\n",
                            Utils.getChar(lowKey.mRing),
                            Utils.getChar(highKey.mRing),
                            Flag.MIDDLE_RING_SCOPE,
                            MIDDLE_RING_SCOPE.ordinal(),
                            Flag.MIDDLE_RING_SCOPE,
                            Flag.MIDDLE_RING_SCOPE,
                            Flag.KEY,
                            Flag.MIDDLE_RING_SCOPE
                    );
                }
                if (clen > 400) {
                    CtAPI.goodbyeFatalError("Message too long for -%s selection - Length is %d, -%s allowed only for messages shorter than 400\n",
                            Flag.MIDDLE_RING_SCOPE, clen, Flag.MIDDLE_RING_SCOPE);
                }
            }


            if (RIGHT_RING_SAMPLING != 1) {

                boolean fullRangeRRing = (lowKey.rRing == Utils.getIndex('A')) && (highKey.rRing == Utils.getIndex('Z'));
                if (!fullRangeRRing) {
                    CtAPI.goodbyeFatalError("Partial right ring range (%s to %s) incompatible with -%s %d. Only -%s 1 (or not specifying -%s and leaving the default 1) is allowed \n" +
                                    " when specifying a partial range. Use range from A to Z for the right ring, e.g. -%s b:231:gta:xxa-b:231:gtz:xxz, or use -%s 1.\n",
                            Utils.getChar(lowKey.rRing),
                            Utils.getChar(highKey.rRing),
                            Flag.RIGHT_RING_SAMPLING,
                            RIGHT_RING_SAMPLING,
                            Flag.RIGHT_RING_SAMPLING,
                            Flag.RIGHT_RING_SAMPLING,
                            Flag.KEY,
                            Flag.RIGHT_RING_SAMPLING
                    );
                }

                boolean fullRangeRMseg = (lowKey.rMesg == Utils.getIndex('A')) && (highKey.rMesg == Utils.getIndex('Z'));
                if (!fullRangeRMseg) {
                    CtAPI.goodbyeFatalError("Partial right rotor range (%s to %s) incompatible with -%s %d. Only -%s 1 (or not specifying -%s and leaving the default 1) is allowed \n" +
                                    " when specifying a partial range. Use range from A to Z for the right rotor, e.g. -%s b:231:gta:xxa-b:231:gtz:xxz, or use -%s 1.\n",
                            Utils.getChar(lowKey.rMesg),
                            Utils.getChar(highKey.rMesg),
                            Flag.RIGHT_RING_SAMPLING,
                            RIGHT_RING_SAMPLING,
                            Flag.RIGHT_RING_SAMPLING,
                            Flag.RIGHT_RING_SAMPLING,
                            Flag.KEY,
                            Flag.RIGHT_RING_SAMPLING
                    );
                }

            }

        }


        if (MODE == Mode.BOMBE) {
            BombeSearch.bombeSearch(CRIB, ciphertext, clen, range, lowKey, highKey, key, indicatorS, indicatorMessageKeyS, HC_SA_CYCLES, RIGHT_RING_SAMPLING, MIDDLE_RING_SCOPE, VERBOSE, CRIB_POSITION, THREADS);
        } else if (MODE == Mode.DECRYPT && !range) {
            encryptDecrypt(indicatorS, plaintext, ciphertext, clen, key);
        } else if (MODE == Mode.IC) {
            TrigramICSearch.searchTrigramIC(lowKey, highKey, true, MIDDLE_RING_SCOPE, RIGHT_RING_SAMPLING, false, HC_SA_CYCLES, 0, THREADS, ciphertext, clen, indicatorS, indicatorMessageKeyS);
        } else if (MODE == Mode.TRIGRAMS) {
            TrigramICSearch.searchTrigramIC(lowKey, highKey, false, MIDDLE_RING_SCOPE, RIGHT_RING_SAMPLING, false, HC_SA_CYCLES, 0, THREADS, ciphertext, clen, indicatorS, indicatorMessageKeyS);
        } else if (MODE == Mode.HILLCLIMBING) {
            HillClimb.hillClimbRange(range ? lowKey : key, range ? highKey : key, HC_SA_CYCLES, THREADS, 0,
                    MIDDLE_RING_SCOPE, RIGHT_RING_SAMPLING, ciphertext, clen, HcSaRunnable.Mode.HC, STRENGTH);
        } else if (MODE == Mode.ANNEALING) {
            HillClimb.hillClimbRange(range ? lowKey : key, range ? highKey : key, HC_SA_CYCLES, THREADS, 0,
                    MIDDLE_RING_SCOPE, RIGHT_RING_SAMPLING, ciphertext, clen, HcSaRunnable.Mode.SA, STRENGTH);
        } else if (MODE == Mode.OSTWALD) {
            HillClimb.hillClimbRange(range ? lowKey : key, range ? highKey : key, HC_SA_CYCLES, THREADS, 0,
                    MIDDLE_RING_SCOPE, RIGHT_RING_SAMPLING, ciphertext, clen, HcSaRunnable.Mode.EStecker, STRENGTH);
        } else if (MODE == Mode.SCENARIO) {
            new RandomChallenges(SCENARIO_PATH, RESOURCE_PATH + "\\faust.txt", lowKey, highKey, SCENARIO);
        } else if (MODE == Mode.INDICATORS) { // cycles
            IndicatorsSearch.indicatorsSearch(INDICATORS_FILE, lowKey, highKey, steckerS, ciphertext, clen, RIGHT_RING_SAMPLING, MIDDLE_RING_SCOPE, HC_SA_CYCLES, THREADS);
        } else if (MODE == Mode.INDICATORS1938) {
            Indicators1938Search.indicators1938Search(INDICATORS_FILE, lowKey, highKey, steckerS, ciphertext, clen);
        }

        CtAPI.goodbye();

    }

    private static void encryptDecrypt(String indicatorS, byte[] plaintext, byte[] ciphertext, int clen, Key key) {
        boolean encrypted = Utils.isTextEncrypted(key, ciphertext, clen, indicatorS);

        if (encrypted) {
            Key finalKey = new Key(key);
            if (indicatorS.length() != 0) {
                byte indicCrypt[] = new byte[3];
                byte indicPlain[] = new byte[3];
                String indicPlainS;
                String indicCryptS = indicatorS.toUpperCase();

                CtAPI.printf("Encrypted Indicator: \t%s\n\n", indicCryptS);
                key.printKeyString("Key for Indicator:\t");
                int ilen = Utils.getText(indicCryptS, indicCrypt);
                key.encipherDecipherAll(indicCrypt, indicPlain, ilen);
                indicPlainS = Utils.getString(indicPlain, ilen);
                CtAPI.printf("\nPlain Indicator: %s\n\n", indicPlainS);

                finalKey.setMesg(indicPlainS);
            }


            finalKey.encipherDecipherAll(ciphertext, plaintext, clen);
            finalKey.printKeyString("Decryption Key:\t");

            byte[] steppings = new byte[Key.MAXLEN];
            finalKey.showSteppings(steppings, clen);
            String steppingsS = Utils.getCiphertextStringNoXJ(steppings, clen);
            CtAPI.printf("\nPlain Text (Rotor stepping information below plain text): \n\n%s\n%s\n\n", Utils.getString(plaintext, clen), steppingsS);
            CtAPI.printf("Removing Xs and Js: \n\n%s\n\n", Utils.getCiphertextStringNoXJ(plaintext, clen));

            finalKey.initPathLookupAll(clen);
            CtAPI.printf("Plaintext Trigrams Score: %d, Bigrams Score %d, IC: %.5f\n",
                    finalKey.triscore(ciphertext, clen),
                    finalKey.biscore(ciphertext, clen),
                    finalKey.icscore(ciphertext, clen));


        } else {
            Key finalKey = new Key(key);

            System.arraycopy(ciphertext, 0, plaintext, 0, clen); // just for clarity
            if (indicatorS.length() != 0) {
                byte indicCrypt[] = new byte[3];
                byte indicPlain[] = new byte[3];
                String indicCryptS;
                String indicPlainS = indicatorS.toUpperCase();

                key.printKeyString("Key for Indicator:\t");
                CtAPI.printf("\nPlain Indicator:     \t%s\n\n", indicPlainS);
                int ilen = Utils.getText(indicPlainS, indicPlain);
                key.encipherDecipherAll(indicPlain, indicCrypt, ilen);
                indicCryptS = Utils.getString(indicCrypt, ilen);
                CtAPI.printf("Encrypted Indicator: \t%s\n\n", indicCryptS);

                finalKey.setMesg(indicPlainS);

                finalKey.printKeyString("Encrytion Key:\t");
                finalKey.encipherDecipherAll(plaintext, ciphertext, clen);


                CtAPI.printf("\n\nEncrypted Message: \t%d %s %s \n\n%s\n\n",
                        clen, key.getMesg(), indicCryptS, Utils.getCiphertextStringInGroups(ciphertext, clen));
            } else {
                finalKey.printKeyString("Encrytion Key:\t");
                finalKey.encipherDecipherAll(plaintext, ciphertext, clen);


                CtAPI.printf("\nEncrypted Message: \t%d \n\n%s\n\n",
                        clen, Utils.getCiphertextStringInGroups(ciphertext, clen));
            }


        }
    }

    private static void createCommandLineArguments() {
        
        CommandLine.add(new CommandLine.Argument(
                Flag.KEY,
                "Key range or key",
                "Range of keys, or specific key. Examples: range of M3 keys B:532:AAC:AAA-B:532:AAC:ZZZ,\n" +
                        "\t\tsingle M4 key B:B532:AAAC:AJKH, single H key with stecker B:532:AAC:JKH|ACFEHJKOLZ, \n " +
                        "\t\tkey range with stecker B:532:AAC:AAA-B:532:AAC:ZZZ|ACFEHJKOLZ. When a range is specified, \n" +
                        "\t\tthe program will sweep for each field in the key (right to left), from the value specified on the left side of the range \n" +
                        "\t\tuntil it reaches the upper value specified in the right side of the range.\n" +
                        "\t\tFor example, to sweep all ring settings from AAA to AZZ (Model H): \n" +
                        "\t\t-" + Flag.KEY + " C:321:AAA:AAA-C:321:AZZ:AAA\n" +
                        "\t\tOr, to test all wheel combinations -" + Flag.KEY + " A:111:AAA:AAA-C:555:AZZ:ZZZ\n" +
                        "\t\tOr, to sweep only values for the middle message settings: -" + Flag.KEY + " C:321:ABC:HAK-C:321:ABC:HZK\n" +
                        "\t\tOr, to sweep only values for the middle and right wheels (other settings known): -" + Flag.KEY + " C:521:ABC:DEF-C:521:ABC:DEF\n" +
                        "\t\tNote that in a range, wheel numbers can be repeated (e.g. -" + Flag.KEY + " B:B111:AAAA:AAAA-B:B555:AAAA:ZZZZ)\n" +
                        "\t\twhile in a single key this is not allowed (-" + Flag.KEY + " B:522:AAC:JKH is invalid).\n" +
                        "\t\tKey format for Model H: u:www:rrr:mmm\n" +
                        "\t\t    where u is the reflector (A, B, or C), www are the 3 wheels from left to right (1 to 5, e.g. 321)  \n" +
                        "\t\t    rrr are the ring settings (e.g. AZC) and mmm are the message settings \n" +
                        "\t\tFor Model M3 = u:www:rrr:mmm \n" +
                        "\t\t    where u is the reflector (B or C), www are the 3 wheels from left to right (1 to 8, e.g. 851)  \n" +
                        "\t\t    rrr are the ring settings (e.g. AZC) and mmm are the message settings \n" +
                        "\t\tfor Model H4 = u:gwww:rrrr:mmmm \n" +
                        "\t\t    where u is the reflector (B), g is the greek wheel (B or G) \n" +
                        "\t\t    www are the wheels from left to right (1 to 8, e.g. 821)  \n" +
                        "\t\t    rrrr are the ring settings (e.g. AAZC) and mmmm are the message settings \n" +
                        "\t\tNote: For models H and M3, it is also possible to specify rings settings with numbers 01 to 26 (instead of A to Z).    \n" +
                        "\t\t    for example, -" + Flag.KEY + " b:413:021221:abc is equivalent to -" + Flag.KEY + " b:413:BLU:abc.   \n" +
                        " ",
                true,
                ""));

        CommandLine.add(new CommandLine.Argument(
                Flag.MODEL,
                "Enigma model",
                "Enigma Model. H (Army Model), M3 (Navy 3 rotors) or M4 (Navy 4 rotors).",
                false,
                "H",
                new String[]{"H", "M3", "M4",}));

        CommandLine.add(new CommandLine.Argument(
                Flag.MODE,
                "Search mode",
                "Search mode (for the case these is no crib). \n" +
                        "\t\t\tHILLCLIMBING for hillclimbing search for steckers at each possible rotor setting - about 2-3,000 keys/sec. Effective with ciphertext with 125 or more letters.\n" +
                        "\t\t\t   Use -" + Flag.STRENGTH + " for a slower but more sensitive search.\n" +
                        "\t\t\tANNEALING for simulated annealing search - much slower than HILLCLIMBING (about 70-130 keys/sec), effective with short ciphertexts between 50 and 150 letters.\n" +
                        "\t\t\t   Use -" + Flag.STRENGTH + " for a slower but more sensitive search.\n" +
                        "\t\t\tOSTWALD for Ostwald method (about 30-60 keys/sec), use for short ciphertexts between 30 to 100 letters.\n" +
                        "\t\t\t   Use -" + Flag.STRENGTH + " for a slower but more sensitive search (1 for E-Stecker, 2 for N-Stecker, 3 for X-Stecker, 4 for R, 5 for S, 6 for I, 7 for A, 8 for T, 9 for O, 10 for U)\n" +
                        "\t\t\tTRIGRAMS look for rotor settings with best trigram score. The steckers must be specified in -" + Flag.KEY + ",\n" +
                        "\t\t\t   e.g. -" + Flag.KEY + " B:132:AAC:AAA-B:132:AAC:ZZZ|ACFEHJKOLZ.\n" +
                        "\t\t\tIC look for rotor settings with best Index of Coincidence. For cryptograms less than 500 letters, \n" +
                        "\t\t\t   the steckers must be specified in -" + Flag.KEY + ", e.g. -" + Flag.KEY + " B:132:AAC:AAA-B:132:AAC:ZZZ|ACFEHJKOLZ.\n" +
                        "\t\t\tBOMBE for crib/known-plaintext attach (extension of the Turing Bombe). \n" +
                        "\t\t\tINDICATORS for an attack on 1930-1938 double indicators (extension of Rejewski's method).\n" +
                        "\t\t\tINDICATORS1938 for an attack on 1938-1940 double indicators (extension of Zygalski's method).\n" +
                        "\t\t\tSCENARIO to create a simulated ciphertext/plaintext/indicators scenario (see -" + Flag.SCENARIO + "and -" +  Flag.SCENARIO_PATH +" options).\n" +
                        "\t\t\tDECRYPT for simple decryption.\n",
                false,
                "DECRYPT",
                new String[]{"HILLCLIMBING", "IC", "TRIGRAMS", "BOMBE", "INDICATORS", "INDICATORS1938", "SCENARIO", "DECRYPT", "ANNEALING", "OSTWALD"}));

        CommandLine.add(new CommandLine.Argument(
                Flag.STRENGTH,
                "Strength of search",
                "Strength of hillclimbing/annealing/Ostwald method search. A higher number means a deeper search, but slower.",
                false,
                1, 12, 1));

        CommandLine.add(new CommandLine.Argument(
                Flag.CIPHERTEXT,
                "Ciphertext or ciphertext file",
                "Ciphertext string, or full path for the file with the cipher, ending with .txt.",
                false,
                ""));

        CommandLine.add(new CommandLine.Argument(
                Flag.CRIB,
                "Crib (known plaintext)",
                "Known plaintext (crib) for attack using extended Turing Bombe.\n" +
                        "\t\tThe position of the crib may be specified with -" + Flag.CRIB_POSITION + ". \n" +
                        "\t\tTo exclude one or more of the letters from menus, replace each unknown crib letter with a ? symbol\n" +
                        "\t\tFor example -" + Flag.CRIB + " eins???zwo specifies a crib of 10 letters but no menu links will be created for the 3 letters marked as ?.\n" +
                        "\t\tThe details of the menus can be printed using-" + Flag.VERBOSE + " (only if a single key is given with -" + Flag.KEY + ", and not a range).",
                false,
                ""));

        CommandLine.add(new CommandLine.Argument(
                Flag.CRIB_POSITION,
                "Crib start position",
                "Starting position of crib, or range of possible starting positions. 0 means first letter. Examples: \n" +
                        "\t\t\t-" + Flag.CRIB_POSITION + " 0 if crib starts at first letter,\n" +
                        "\t\t\t-" + Flag.CRIB_POSITION + " 10 if crib starts at the 11th letter, \n+" +
                        "\t\t\t-" + Flag.CRIB_POSITION + " 0-9 if crib may start at any of the first 10 positions,\n" +
                        "\t\t\t-" + Flag.CRIB_POSITION + " * if crib may start at any position.\n" +
                        "\t\tPosition(s) generating a menu conflict (letter encrypted to itself) are discarded. \n",
                false,
                "0"));

        CommandLine.add(new CommandLine.Argument(
                Flag.INDICATORS_FILE,
                "Full file path for indicators file.",
                "File with set of indicators. The file should contain either groups of 6 letters (INDICATORS mode), or groups of 9 letters (INIDCATORS1938 mode). \n" +
                        "\t\tIf groups of encrypted double indicators with 6 letters are given, searches key according to the Cycle Characteristic method developed by the Rejewski before WWII.\n" +
                        "\t\t  Finds the daily key which creates cycles which match those of the database. Then finds stecker plugs which match the all indicators \n" +
                        "\t\t  If a ciphertext is provided, using the first (decrypted) indicator as the Message Key for that message  \n" +
                        "\t\t  perform a trigram-based search to find the Ring Settings and to decipher the message. \n" +
                        "\t\tIf groups of encrypted double indicators with of 3+6=9 letters are given, \n" +
                        "\t\t  finds daily key according to the Zygalski's Sheets method developed by the Poles before WWII.\n" +
                        "\t\t  Indicators include 3 letters for the key to encrypt the double message key, and 6 letters of the doubled encrypted message.\n" +
                        "\t\t  Will search for keys (wheel order, wing settings) which together with the keys in the indicator groups,\n" +
                        "\t\t  create 'female' patterns which match the database (those keys with females). Stecker settings are also detected, and the first key indicator (from the file)\n" +
                        "\t\t is used to decipher the ciphertext (if a ciphertext was provided).\n",
                false,
                ""));

        CommandLine.add(new CommandLine.Argument(
                Flag.MESSAGE_INDICATOR,
                "Message indicator options",
                "Indicator sent with the ciphertext. Has two distinct purposes and forms: \n" +
                        "\t\t-w {3-letter encrypted indicator} e.g.-" + Flag.MESSAGE_INDICATOR + " STG.  This must be used together with a single key in -" + Flag.KEY + " in which the steckers were specified (e.g. -" + Flag.KEY + " B:532:AAC:JKH:ACFEHJKOLZ). \n" +
                        "\t\t    First, this indicator is decrypted using the given key (daily key), then the decrypted indicator is used as the message key to decrypt the full message. \n" +
                        "\t\t-w {3-letter message key}:{3-letter encrypted indicator} e.g.-" + Flag.MESSAGE_INDICATOR + " OWL:STG. In this form, this is used as an additional filter when searching for the best key.\n" +
                        "\t\t   Only messages keys which are a result of decrypting the encrypted indicator with the given message key are considered for the search.\n" +
                        "\t\t   Stecker board settings must be known and specified (e.g. B:532:AAA:AAA-B:532:AAZ:ZZZ|ACFEHJKOLZ). Not compatible with HILLCLIMBING/ANNEALING modes\n",

                false,
                ""));

        CommandLine.add(new CommandLine.Argument(
                Flag.RESOURCE_PATH,
                "Resource directory",
                "Full path of directory for resources (e.g. stats files).",
                false,
                "."));

        CommandLine.add(new CommandLine.Argument(
                Flag.THREADS,
                "Number of processing threads",
                "Number of threads, for multithreading. 1 for no multithreading.",
                false,
                1, 20, 7));

        CommandLine.add(new CommandLine.Argument(
                Flag.HC_SA_CYCLES,
                "Number of hillclimbing/simulated annealing cycles",
                "Number of hillclimbing/simulated annealing cycles. 0 for no cycles.",
                false,
                0, 1000, 2));

        CommandLine.add(new CommandLine.Argument(
                Flag.LANGUAGE,
                "Language",
                "Language used for statistics and for simulation random text.",
                false,
                "GERMAN",
                new String[]{"GERMAN", "ENGLISH",}));

        CommandLine.add(new CommandLine.Argument(
                Flag.VERBOSE,
                "Verbose",
                "Show details of crib attack."));

        CommandLine.add(new CommandLine.Argument(
                Flag.RIGHT_RING_SAMPLING,
                "Left rotor sampling interval.",
                "Check only a sample of right ring positions.-" + Flag.RIGHT_RING_SAMPLING + " {right ring interval value} {default - 1 - no sampling, check all positions in range}.\n" +
                        "\t\tIf the interval > 1, test only a sample of right ring positions in search.\n" +
                        "\t\tFor example -" + Flag.RIGHT_RING_SAMPLING + " 3 means that only one in three right ring positions will be tested.  \n" +
                        "\t\tThis is likely to still produce a partial or full decryption. \n" +
                        "\t\tShould be used with caution together with mode BOMBE (Bombe search for menu stops) as this may cause stops to be missed. \n",
                false,
                1, 7, 1));
        CommandLine.add(new CommandLine.Argument(
                Flag.MIDDLE_RING_SCOPE,
                "Optimize middle rotor moves",
                "Optimize middle rotor moves.-" + Flag.MIDDLE_RING_SCOPE + " {option} {default - 0 - no optimization}.\n" +
                        "\t\tReduce the number of middle rotor settings to be tested. \n" +
                        "\t\t-" + Flag.MIDDLE_RING_SCOPE + " 0 - No reduction, all middle rotor settings specified in the range will be tested.\n" +
                        "\t\t-" + Flag.MIDDLE_RING_SCOPE + " 1 - Test all middle rotor settings which generate a stepping of the left rotor, plus one settings which does NOT. \n" +
                        "\t\t       Reliable, no valid solutions will be missed, and reduces scope from 26 to {message length}/26+1 \n" +
                        "\t\t-" + Flag.MIDDLE_RING_SCOPE + " 2 - Test all middle rotor settings which generate a stepping of the left rotor affecting the first 1/5 or last 1/5 of the message, plus one more \n" +
                        "\t\t       setting which is not generating a stepping. This is a good compromise between speed and accuracy. Reduces scope from 26 to {message length}*0.40/26+1 \n" +
                        "\t\t-" + Flag.MIDDLE_RING_SCOPE + " 3 - Test one middle rotor setting which does NOT generate a stepping of the left rotor. Fastest and most agressive option since only one middle rotor setting\n" +
                        "\t\t       will be tested, but part of the message may be garbled if there was such a stepping originally. Good for short messages since probablity \n" +
                        "\t\t       for left rotor stepping is {message length}/676. Can save a lot of search time if successful. \n" +
                        "\t\t-" + Flag.MIDDLE_RING_SCOPE + " 4 - Test only all middle rotor settings which generate a stepping of the left rotor. \n" +
                        "\t\t       Usually not needed except for testing purposes. Reduces scope from 26 to {message length}/26 \n" +
                        "\t\t-" + Flag.MIDDLE_RING_SCOPE + " 5 - Test all middle rotor settings which do NOT generate a stepping of the left rotor. \n" +
                        "\t\tNote: The key range should specify the full range (A to Z) for the middle rotor, for any option other than 0. \n",
                false,
                0, 5, 0));

        CommandLine.add(new CommandLine.Argument(
                Flag.SCENARIO,
                "Generate random scenario",
                "Generate simulated ciphertext and indicators.\n" +
                        "\t\tA range of keys must be selected (-" + Flag.KEY + ") from which a key is randomly selected for simulation. \n" +
                        "\t\tUsage: -"+ Flag.SCENARIO + " {f}:{l}:{n}:{s}:{g}:{c}. \n" +
                        "\t\t{f} is the selected scenario: 1 to only generate a ciphertext, 2 for pre-1938 indicators (and a ciphertext), 3 to generate post 1938 doubled indicators (and a ciphertext). \n" +
                        "\t\t    Default is 1. Scenario 2 and 3 are not compatible with the {n} option.\n" +
                        "\t\t{l} is the length of the random ciphertext, or the combined length of all ciphertexts (default 150).\n" +
                        "\t\t{n} is the number of messages (a longer text will be split) for scenario 1, for scenario 2  and 3 this is the number of indicators to be generated.\n" +
                        "\t\t{s} is the number of Stecker Plugs (default 10).\n"+
                        "\t\t{g} is the percentage of garbled letters (default 0).\n"+
                        "\t\t{c} is the length of a crib (the plaintext at the beginning of the message).\n" +
                        "\t\t\n" +
                        "\t\tThe following files are created (<id> is the randomly generated scenario id): \n" +
                        "\t\t -'S<id>cipher.txt' ciphertext for a single message (not split)  \n" +
                        "\t\t   In case of long message which has been split several files (S<id>cipher1.txt, S<id>cipher2.txt etc.. will also be created). \n" +
                        "\t\t - S<id>indicators.txt with indicators, for scenario 2 (1938-1940) or 3 (pre-1938). \n" +
                        "\t\t   With scenario 2, the file contains groups of 6 letters (encrypted doubled keys).  \n" +
                        "\t\t   With scenario 3, it contains groups of 9 letters (indicator plus encrypted doubled key) are kept \n" +
                        "\t\t   The indicator used for the generated ciphertext is the first in that set. \n" +
                        "\t\t - S<id>plaintext.txt contains the plaintext. \n" +
                        "\t\t - S<id>challenge.txt contains all the elements of the challenge (messages with headers, crib, etc) without the solution.\n" +
                        "\t\t - S<id>solution.txt contains all the elements of the solution. \n" +
                        "\t\tExample: -" + Flag.SCENARIO + " 1:500:3:10:3:25 - generate plaintexts/ciphertexts, with total length of 500 split into 3 messages.\n" +
                        "\t\t   The stecker board has 10 plugs, 3 percent of the letters are garbled, a crib of 25 letters is given\n" +
                        "\t\tExample: -" + Flag.SCENARIO + " 2:50::6::0 - generate 50 pre-1938 doubled indicators, a single plaintext/ciphertext with 150 letters (default). \n" +
                        "\t\t   The stecker board has 6 plugs, no letters are garbled (default), no crib is given.\n",

                false,
                ""));
        CommandLine.add(new CommandLine.Argument(
                Flag.SCENARIO_PATH,
                "Directory for scenario output files",
                "Full path of directory for files created in SCENARIO mode (using the -" + Flag.SCENARIO + " option to specify the parameters).",
                false,
                "."));

        CommandLine.add(new CommandLine.Argument(
                Flag.CYCLES,
                "Reserved",
                "",
                false,
                0, 1000, 0));

    }


    private static void incompatible(Mode mode, Flag[] flags) {
        for (Flag flag : flags) {
            if (CommandLine.isSet(flag)) {
                CtAPI.goodbyeFatalError("Option -%s (%s) not supported for mode %s\n", flag, CommandLine.getShortDesc(flag), mode);
            }
        }
    }

    private static void required(Mode mode, Key.Model currentModel, Key.Model[] models) {
        for (Key.Model model : models) {
            if (model == currentModel) {
                return;
            }
        }
        CtAPI.goodbyeFatalError("Mode %s not supported for model %s\n", mode, currentModel);
    }

    private static void required(Mode mode, Flag[] flags) {
        for (Flag flag : flags) {
            if (!CommandLine.isSet(flag)) {
                CtAPI.goodbyeFatalError("Option -%s (%s) is mandatory with mode %s\n", flag, CommandLine.getShortDesc(flag), mode);
            }
        }
    }

    private static void incompatibleWithRangeOkKeys(Flag[] flags) {
        for (Flag flag : flags) {
            if (CommandLine.isSet(flag)) {
                CtAPI.goodbyeFatalError("Option -%s (%s) not supported for key range\n", flag, CommandLine.getShortDesc(flag));
            }
        }
    }

    private static void incompatibleWithSingleKey(Flag[] flags) {
        for (Flag flag : flags) {
            if (CommandLine.isSet(flag)) {
                CtAPI.goodbyeFatalError("Option -%s (%s) requires a key range\n", flag, CommandLine.getShortDesc(flag));
            }
        }
    }

    private static void incompatibleWithRangeOkKeys(Mode currentMode, Mode[] modes) {
        for (Mode mode : modes) {
            if (mode == currentMode) {
                CtAPI.goodbyeFatalError("Mode %s is not allowed with a key range\n", mode);
            }
        }

    }

    private static void incompatibleWithSingleKey(Mode currentMode, Mode[] modes) {
        for (Mode mode : modes) {
            if (mode == currentMode) {
                CtAPI.goodbyeFatalError("Mode %s requires a key range\n", mode);
            }
        }
    }



}
