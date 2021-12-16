/*
   Copyright 2020 George Lasry
   Converted in 2020 from Java to C# by Nils Kopal

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using EnigmaAnalyzerLib.Common;
using System;
using System.Text.RegularExpressions;
using static EnigmaAnalyzerLib.Key;

namespace EnigmaAnalyzerLib
{
    public class EnigmaMain
    {
        public enum Mode
        {
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

        public static class ModeParser
        {
            public static Mode parse(string mode)
            {
                if (mode != null)
                {
                    mode = mode.ToUpper();
                }
                switch (mode)
                {
                    case "HILLCLIMBING":
                        return Mode.HILLCLIMBING;
                    case "ANNEALING":
                        return Mode.ANNEALING;
                    case "OSTWALD":
                        return Mode.OSTWALD;
                    case "TRIGRAMS":
                        return Mode.TRIGRAMS;
                    case "BOMBE":
                        return Mode.BOMBE;
                    case "INDICATORS":
                        return Mode.INDICATORS;
                    case "INDICATORS1938":
                        return Mode.INDICATORS1938;
                    case "SCENARIO":
                        return Mode.SCENARIO;
                    case "DECRYPT":
                        return Mode.DECRYPT;
                    default:
                        return Mode.HILLCLIMBING;
                }
            }
        }

        public static void Main(string[] args)
        {
            createCommandLineArguments();
            //Argument.printUsage();
            //CtBestList.setDiscardSamePlaintexts(false);
            //CtBestList.setThrottle(false);
            //CtBestList.setSize(10);

            //CtAPI.openAndReadInputValues("Enigma attacks", "1.0");

            if (!CommandLine.parseAndPrintCommandLineArgs(args))
            {
                return;
            }

            string RESOURCE_PATH = CommandLine.getstringValue(Flag.RESOURCE_PATH);
            string SCENARIO_PATH = CommandLine.getstringValue(Flag.SCENARIO_PATH);
            int HC_SA_CYCLES = CommandLine.getintValue(Flag.HC_SA_CYCLES);
            int THREADS = CommandLine.getintValue(Flag.THREADS);
            string CRIB = CommandLine.getstringValue(Flag.CRIB);
            string CIPHERTEXT = CommandLine.getstringValue(Flag.CIPHERTEXT);

            if (CIPHERTEXT != null && CIPHERTEXT.EndsWith("txt"))
            {
                CIPHERTEXT = Utils.readTextFile(CIPHERTEXT);
            }
            if (CIPHERTEXT != null)
            {
                CIPHERTEXT = Regex.Replace(CIPHERTEXT.ToUpper(), "[^A-Z]", "");
            }

            Key.Model MODEL = Key.ModelParser.parse(CommandLine.getstringValue(Flag.MODEL));
            Language LANGUAGE = LanguageParser.parse(CommandLine.getstringValue(Flag.LANGUAGE));
            string INDICATORS_FILE = CommandLine.getstringValue(Flag.INDICATORS_FILE);
            string KEY = CommandLine.getstringValue(Flag.KEY);
            string MESSAGE_INDICATOR = CommandLine.getstringValue(Flag.MESSAGE_INDICATOR);
            string SCENARIO = CommandLine.getstringValue(Flag.SCENARIO);

            int STRENGTH = CommandLine.getintValue(Flag.STRENGTH);
            int RIGHT_RING_SAMPLING = CommandLine.getintValue(Flag.RIGHT_RING_SAMPLING);
            MRingScope MIDDLE_RING_SCOPE = MRingScopeParser.parse(CommandLine.getintValue(Flag.MIDDLE_RING_SCOPE));
            bool VERBOSE = CommandLine.getboolValue(Flag.VERBOSE);
            string CRIB_POSITION = CommandLine.getstringValue(Flag.CRIB_POSITION);
            Mode MODE = ModeParser.parse(CommandLine.getstringValue(Flag.MODE));

            string[] keyAndStecker = KEY.Split('[', '|', ']');
            string steckerS = (keyAndStecker.Length == 2) ? keyAndStecker[1] : "";
            string[] keyParts = keyAndStecker[0].Split('[', '\\', '-', '|', ']');
            bool range = keyParts.Length == 2;
            string rangeLowS = keyParts[0];
            string rangeHighS = range ? keyParts[1] : "";

            string keyS = range ? "" : rangeLowS;

            string indicatorS = "";
            string indicatorMessageKeyS = "";

            switch (MODE)
            {
                case Mode.HILLCLIMBING:
                    required(MODE, new Flag[] { Flag.CIPHERTEXT });
                    incompatible(MODE, new Flag[] { Flag.SCENARIO, Flag.CRIB, Flag.CRIB_POSITION, Flag.INDICATORS_FILE });
                    break;
                case Mode.IC:
                    required(MODE, new Flag[] { Flag.CIPHERTEXT });
                    incompatible(MODE, new Flag[] { Flag.SCENARIO, Flag.CRIB, Flag.CRIB_POSITION, Flag.INDICATORS_FILE, Flag.STRENGTH });
                    break;
                case Mode.TRIGRAMS:
                    required(MODE, new Flag[] { Flag.CIPHERTEXT });
                    incompatible(MODE, new Flag[] { Flag.SCENARIO, Flag.CRIB, Flag.CRIB_POSITION, Flag.INDICATORS_FILE, Flag.STRENGTH });
                    break;
                case Mode.BOMBE:
                    required(MODE, new Flag[] { Flag.CIPHERTEXT, Flag.CRIB });
                    incompatible(MODE, new Flag[] { Flag.SCENARIO, Flag.INDICATORS_FILE, Flag.STRENGTH });
                    break;
                case Mode.INDICATORS:
                    required(MODE, new Flag[] { Flag.INDICATORS_FILE });
                    incompatible(MODE, new Flag[] { Flag.SCENARIO, Flag.CRIB, Flag.CRIB_POSITION, Flag.MESSAGE_INDICATOR, Flag.STRENGTH });
                    required(MODE, MODEL, new Key.Model[] { Key.Model.H });
                    break;
                case Mode.INDICATORS1938:
                    required(MODE, new Flag[] { Flag.INDICATORS_FILE });
                    incompatible(MODE, new Flag[] { Flag.SCENARIO, Flag.CRIB, Flag.CRIB_POSITION, Flag.MESSAGE_INDICATOR, Flag.MIDDLE_RING_SCOPE, Flag.RIGHT_RING_SAMPLING, Flag.STRENGTH });
                    required(MODE, MODEL, new Key.Model[] { Key.Model.H });
                    break;
                case Mode.SCENARIO:
                    required(MODE, new Flag[] { Flag.SCENARIO });
                    incompatible(MODE, new Flag[] { Flag.LANGUAGE, Flag.HC_SA_CYCLES, Flag.CRIB, Flag.CRIB_POSITION, Flag.INDICATORS_FILE, Flag.MESSAGE_INDICATOR, Flag.MIDDLE_RING_SCOPE, Flag.RIGHT_RING_SAMPLING, Flag.STRENGTH });
                    break;
                case Mode.DECRYPT:
                    incompatible(MODE, new Flag[] { Flag.CRIB, Flag.CRIB_POSITION, Flag.INDICATORS_FILE, Flag.MESSAGE_INDICATOR, Flag.MIDDLE_RING_SCOPE, Flag.RIGHT_RING_SAMPLING, Flag.STRENGTH });
                    break;
            }

            if (range)
            {
                incompatibleWithRangeOkKeys(MODE, new Mode[] { Mode.DECRYPT });
                incompatibleWithRangeOkKeys(new Flag[] { });
            }
            else
            {
                incompatibleWithSingleKey(new Flag[] { Flag.MIDDLE_RING_SCOPE, Flag.RIGHT_RING_SAMPLING });
                incompatibleWithSingleKey(MODE, new Mode[] { Mode.IC, Mode.TRIGRAMS, Mode.INDICATORS, Mode.INDICATORS1938 });
            }

            if (!(MESSAGE_INDICATOR.Length == 0))
            {
                Key dummyKey = new Key();
                bool indicatorError = false;
                switch (MESSAGE_INDICATOR.Length)
                {
                    case 3:
                        dummyKey.initDefaults(Key.Model.H);
                        indicatorS = MESSAGE_INDICATOR;
                        if (dummyKey.setMesg(indicatorS) != 1)
                        {
                            indicatorError = true;
                        }

                        break;
                    case 4:
                        dummyKey.initDefaults(Key.Model.M4);
                        indicatorS = MESSAGE_INDICATOR;
                        if (dummyKey.setMesg(indicatorS) != 1)
                        {
                            indicatorError = true;
                        }

                        break;
                    case 7:
                        dummyKey.initDefaults(Key.Model.H);
                        indicatorMessageKeyS = MESSAGE_INDICATOR.JavaSubstring(0, 3);
                        indicatorS = MESSAGE_INDICATOR.JavaSubstring(4, 7);
                        if (dummyKey.setMesg(indicatorS) != 1)
                        {
                            indicatorError = true;
                        }

                        if (dummyKey.setMesg(indicatorMessageKeyS) != 1)
                        {
                            indicatorError = true;
                        }

                        break;
                    case 9:
                        dummyKey.initDefaults(Key.Model.M4);
                        indicatorMessageKeyS = MESSAGE_INDICATOR.JavaSubstring(0, 4);
                        indicatorS = MESSAGE_INDICATOR.JavaSubstring(5, 9);
                        if (dummyKey.setMesg(indicatorS) != 1)
                        {
                            indicatorError = true;
                        }

                        if (dummyKey.setMesg(indicatorMessageKeyS) != 1)
                        {
                            indicatorError = true;
                        }

                        break;

                    default:
                        indicatorError = true;
                        break;
                }

                if (indicatorError)
                {
                    Console.WriteLine("Invalid Indicator (-{0}): Either XXX or XXX:YYY for Model H/M3, or XXXX:YYYY for Model M4", Flag.MESSAGE_INDICATOR);
                }
                else if (indicatorMessageKeyS.Length == 0)
                { // xxx format
                    if (range)
                    {
                        Console.WriteLine("Invalid Indicator: When range of keys selected, then only -{0} XXX:YYY or XXXX:YYYY (for M4) is allowed ", Flag.MESSAGE_INDICATOR);
                    }
                }
                else
                {// xxx:yyy format
                    if (!range)
                    {
                        Console.WriteLine("Invalid Indicator (-w): If single key selected, then only -{0} XXX (or XXXX for M4) is allowed ", Flag.MESSAGE_INDICATOR);
                    }
                }
            }

            int res;

            short[] plaintext = new short[Key.MAXLEN];
            short[] ciphertext = new short[Key.MAXLEN];
            int clen = 0;
            Key lowKey = new Key();
            Key highKey = new Key();
            Key key = new Key();

            EnigmaStats enigmaStats = new EnigmaStats();
            switch (LANGUAGE)
            {
                case Language.ENGLISH:
                    enigmaStats.loadBidictFromResources(Properties.Resources.english_logbigrams);
                    enigmaStats.loadTridictFromResource(Properties.Resources.english_logtrigrams);
                    break;
                case Language.GERMAN:
                default:
                    enigmaStats.loadBidictFromResources(Properties.Resources.german_logbigrams);
                    enigmaStats.loadTridictFromResource(Properties.Resources.german_logtrigrams);
                    break;
            }

            /*string trigramFile = "enigma_logtrigrams.txt";
            string bigramFile = "enigma_logbigrams.txt";

            if (LANGUAGE == Language.ENGLISH)
            {
                bigramFile = "english_logbigrams.txt";
                trigramFile = "english_logtrigrams.txt";
            }
            if (!string.IsNullOrEmpty(RESOURCE_PATH))
                {
                bigramFile = RESOURCE_PATH + "\\" + bigramFile;
                trigramFile = RESOURCE_PATH + "\\" + trigramFile; 
            }
            res = EnigmaStats.loadTridict(trigramFile);
            if (res != 1)
            {
                Console.WriteLine("Load (log) trigrams file {0} failed", trigramFile);
            }
            res = EnigmaStats.loadBidict(bigramFile);
            if (res != 1)
            {
                Console.WriteLine("Load (log) bigrams file {0} failed", bigramFile);
            }
            */

            if (!CommandLine.isSet(Flag.SCENARIO))
            {
                clen = EnigmaUtils.getText(CIPHERTEXT, ciphertext);
                Console.WriteLine("Ciphertext (Length = {0}) {0}", clen, CIPHERTEXT);
            }

            if (!range)
            {
                res = key.setKey(keyS, MODEL, false);
                if (res != 1)
                {
                    Console.WriteLine("Invalid key: {0}", keyS);
                }

                res = key.setStecker(steckerS);
                if (res != 1)
                {
                    Console.WriteLine("invalid stecker board settings: {0} - Should include pairs of letters with no repetitions, or may be omitted", steckerS);
                }

                if (indicatorS.Length != 0)
                {
                    Key dumpKey = new Key(key);
                    res = dumpKey.setMesg(indicatorS);
                    if (res == 0)
                    {
                        Console.WriteLine("Invalid message message_indicator: {0} ", indicatorS);
                    }
                    if (steckerS.Length == 0)
                    {
                        Console.WriteLine("Stecker board mandatory when -{0} is specified", Flag.MESSAGE_INDICATOR);
                    }
                }
            }
            else
            {

                string fminS;
                string tmaxS;
                switch (MODEL)
                {
                    case Model.M3:
                        fminS = "B:111:AAA:AAA";
                        tmaxS = "C:888:ZZZ:ZZZ";
                        break;
                    case Model.M4:
                        fminS = "B:B111:AAAA:AAAA";
                        tmaxS = "C:G888:ZZZZ:ZZZZ";
                        break;
                    case Model.H:
                    default:
                        fminS = "A:111:AAA:AAA";
                        tmaxS = "C:555:ZZZ:ZZZ";
                        break;
                }

                if (rangeLowS.Length == 0)
                {
                    rangeLowS = fminS;
                }

                if (rangeHighS.Length == 0)
                {
                    rangeHighS = tmaxS;
                }

                res = Key.setRange(lowKey, highKey, rangeLowS, rangeHighS, MODEL);
                if (res != 1)
                {
                    Console.WriteLine("Invalid key range:  {0}-{1}  - Invalid key format, or first has higher value than last ", rangeLowS, rangeHighS);
                }

                if ((lowKey.lRing != highKey.lRing) && (indicatorS.Length == 0) && (MODE == Mode.HILLCLIMBING || MODE == Mode.ANNEALING || MODE == Mode.OSTWALD))
                {
                    Console.WriteLine("WARNING: Setting a range (different values) for the Left Ring settings is usually not necessary and will significant slow Hill Climbing searche");
                }

                if (steckerS.Length != 0)
                {
                    res = lowKey.setStecker(steckerS) * highKey.setStecker(steckerS);
                    if (res != 1)
                    {
                        Console.WriteLine("Invalid steckers: {0} - Should include pairs of letters with no repetitions, or be omitted", steckerS);
                    }
                }


                if ((indicatorS.Length != 0) || (indicatorMessageKeyS.Length != 0))
                {
                    if ((indicatorS.Length == 0) || (indicatorMessageKeyS.Length == 0))
                    {
                        Console.WriteLine("Invalid message_indicator (-{0}) - Only XXX:YYY (or XXXX:YYYY for M4), which must include the Message Key for encrypting the Indicator, is allowed for this mode {1}",
                                Flag.MESSAGE_INDICATOR, MODE);
                    }
                    Key tempKey = new Key(lowKey);
                    res = tempKey.setMesg(indicatorS);
                    if (res == 0)
                    {
                        Console.WriteLine("Invalid message indicator (-{0}): {1} ", Flag.MESSAGE_INDICATOR, indicatorS);
                    }
                    res = tempKey.setMesg(indicatorMessageKeyS);
                    if (res == 0)
                    {
                        Console.WriteLine("Invalid message key for message_indicator (-{0}): {1} ", Flag.MESSAGE_INDICATOR, indicatorMessageKeyS);
                    }
                    if (steckerS.Length == 0)
                    {
                        Console.WriteLine("Stecker board settings mandatory for -{0}  ", Flag.MESSAGE_INDICATOR);
                    }
                    if (HC_SA_CYCLES > 0)
                    {
                        Console.WriteLine("Invalid settings - When specifying -{0} , -{1} 0 (no hillclimbing/simulated annealing on search results) must also be selected. ",
                                Flag.MESSAGE_INDICATOR, Flag.HC_SA_CYCLES);
                    }
                }

                if (MIDDLE_RING_SCOPE != MRingScope.ALL)
                {
                    bool fullRangeMRing = (lowKey.mRing == EnigmaUtils.getIndex('A')) && (highKey.mRing == EnigmaUtils.getIndex('Z'));
                    if (!fullRangeMRing)
                    {
                        Console.WriteLine("Range of middle ring ({0} to {1}) imcompatible with -{2} {3} selection: Only -{4} 0 (or not specifying -{5} and leaving the default 0) is allowed " +
                                        " when specifying a partial range. Use range from A to Z for the middle ring, e.g. -{6} b:231:aaa:aaa-b:231:azz:zzz, or use -{7} 0.",
                                EnigmaUtils.getChar(lowKey.mRing),
                                EnigmaUtils.getChar(highKey.mRing),
                                Flag.MIDDLE_RING_SCOPE,
                                (int)MIDDLE_RING_SCOPE,
                                Flag.MIDDLE_RING_SCOPE,
                                Flag.MIDDLE_RING_SCOPE,
                                Flag.KEY,
                                Flag.MIDDLE_RING_SCOPE
                        );
                    }
                    if (clen > 400)
                    {
                        Console.WriteLine("Message too long for -{0} selection - Length is {1}, -{2} allowed only for messages shorter than 400",
                                Flag.MIDDLE_RING_SCOPE, clen, Flag.MIDDLE_RING_SCOPE);
                    }
                }


                if (RIGHT_RING_SAMPLING != 1)
                {
                    bool fullRangeRRing = (lowKey.rRing == EnigmaUtils.getIndex('A')) && (highKey.rRing == EnigmaUtils.getIndex('Z'));
                    if (!fullRangeRRing)
                    {
                        Console.WriteLine("Partial right ring range ({0} to {1}) incompatible with -{2} {3}. Only -{4} 1 (or not specifying -{5} and leaving the default 1) is allowed " +
                                        " when specifying a partial range. Use range from A to Z for the right ring, e.g. -{6} b:231:gta:xxa-b:231:gtz:xxz, or use -{7} 1.",
                                EnigmaUtils.getChar(lowKey.rRing),
                                EnigmaUtils.getChar(highKey.rRing),
                                Flag.RIGHT_RING_SAMPLING,
                                RIGHT_RING_SAMPLING,
                                Flag.RIGHT_RING_SAMPLING,
                                Flag.RIGHT_RING_SAMPLING,
                                Flag.KEY,
                                Flag.RIGHT_RING_SAMPLING
                        );
                    }

                    bool fullRangeRMseg = (lowKey.rMesg == EnigmaUtils.getIndex('A')) && (highKey.rMesg == EnigmaUtils.getIndex('Z'));
                    if (!fullRangeRMseg)
                    {
                        Console.WriteLine("Partial right rotor range ({0} to {1}) incompatible with -{2} {3}. Only -{4} 1 (or not specifying -{5} and leaving the default 1) is allowed " +
                                        " when specifying a partial range. Use range from A to Z for the right rotor, e.g. -{6} b:231:gta:xxa-b:231:gtz:xxz, or use -{7} 1.",
                                EnigmaUtils.getChar(lowKey.rMesg),
                                EnigmaUtils.getChar(highKey.rMesg),
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

            ResultReporter resultReporter = new ConsoleResultReporter();

            if (MODE == Mode.BOMBE)
            {
                new BombeSearch().bombeSearch(CRIB, ciphertext, clen, range, lowKey, highKey, key, indicatorS, indicatorMessageKeyS, HC_SA_CYCLES, RIGHT_RING_SAMPLING, MIDDLE_RING_SCOPE, VERBOSE, CRIB_POSITION, THREADS, enigmaStats, resultReporter);
            }
            else if (MODE == Mode.DECRYPT && !range)
            {
                encryptDecrypt(indicatorS, plaintext, ciphertext, clen, key, enigmaStats);
            }
            else if (MODE == Mode.IC)
            {
                new TrigramICSearch().searchTrigramIC(lowKey, highKey, true, MIDDLE_RING_SCOPE, RIGHT_RING_SAMPLING, false, HC_SA_CYCLES, 0, THREADS, ciphertext, clen, indicatorS, indicatorMessageKeyS, enigmaStats, resultReporter);
            }
            else if (MODE == Mode.TRIGRAMS)
            {
                new TrigramICSearch().searchTrigramIC(lowKey, highKey, false, MIDDLE_RING_SCOPE, RIGHT_RING_SAMPLING, false, HC_SA_CYCLES, 0, THREADS, ciphertext, clen, indicatorS, indicatorMessageKeyS, enigmaStats, resultReporter);
            }
            else if (MODE == Mode.HILLCLIMBING)
            {
                new HillClimb().hillClimbRange(range ? lowKey : key, range ? highKey : key, HC_SA_CYCLES, THREADS, 0,
                        MIDDLE_RING_SCOPE, RIGHT_RING_SAMPLING, ciphertext, clen, HcSaRunnable.Mode.HC, STRENGTH, enigmaStats, resultReporter);
            }
            else if (MODE == Mode.ANNEALING)
            {
                new HillClimb().hillClimbRange(range ? lowKey : key, range ? highKey : key, HC_SA_CYCLES, THREADS, 0,
                        MIDDLE_RING_SCOPE, RIGHT_RING_SAMPLING, ciphertext, clen, HcSaRunnable.Mode.SA, STRENGTH, enigmaStats, resultReporter);
            }
            else if (MODE == Mode.OSTWALD)
            {
                new HillClimb().hillClimbRange(range ? lowKey : key, range ? highKey : key, HC_SA_CYCLES, THREADS, 0,
                        MIDDLE_RING_SCOPE, RIGHT_RING_SAMPLING, ciphertext, clen, HcSaRunnable.Mode.EStecker, STRENGTH, enigmaStats, resultReporter);
            }
            else if (MODE == Mode.SCENARIO)
            {
                new RandomChallenges(SCENARIO_PATH, RESOURCE_PATH + "\\faust.txt", lowKey, highKey, SCENARIO, enigmaStats, resultReporter);
            }
            else if (MODE == Mode.INDICATORS)
            { // cycles
                Console.WriteLine("Indicators search not implemented in C#");
                //IndicatorsSearch.indicatorsSearch(INDICATORS_FILE, lowKey, highKey, steckerS, ciphertext, clen, RIGHT_RING_SAMPLING, MIDDLE_RING_SCOPE, HC_SA_CYCLES, THREADS);
            }
            else if (MODE == Mode.INDICATORS1938)
            {
                Console.WriteLine("Indicators search not implemented in C#");
                //Indicators1938Search.indicators1938Search(INDICATORS_FILE, lowKey, highKey, steckerS, ciphertext, clen);
            }
            //CtAPI.goodbye();
        }

        private static void encryptDecrypt(string indicatorS, short[] plaintext, short[] ciphertext, int clen, Key key, EnigmaStats enigmaStats)
        {
            bool encrypted = EnigmaUtils.isTextEncrypted(key, ciphertext, clen, indicatorS);

            if (encrypted)
            {
                Key Key = new Key(key);
                if (indicatorS.Length != 0)
                {
                    short[] indicCrypt = new short[3];
                    short[] indicPlain = new short[3];
                    string indicPlainS;
                    string indicCryptS = indicatorS.ToUpper();

                    Console.WriteLine("Encrypted Indicator: \t{0}", indicCryptS);
                    key.printKeystring("Key for Indicator:\t");
                    int ilen = EnigmaUtils.getText(indicCryptS, indicCrypt);
                    key.encipherDecipherAll(indicCrypt, indicPlain, ilen);
                    indicPlainS = EnigmaUtils.getstring(indicPlain, ilen);
                    Console.WriteLine("Plain Indicator: {0}", indicPlainS);

                    Key.setMesg(indicPlainS);
                }


                Key.encipherDecipherAll(ciphertext, plaintext, clen);
                Key.printKeystring("Decryption Key:\t");

                short[] steppings = new short[Key.MAXLEN];
                Key.showSteppings(steppings, clen);
                string steppingsS = EnigmaUtils.getCiphertextstringNoXJ(steppings, clen);
                Console.WriteLine("Plain Text (Rotor stepping information below plain text): {0}{1}", EnigmaUtils.getstring(plaintext, clen), steppingsS);
                Console.WriteLine("Removing Xs and Js: {0}", EnigmaUtils.getCiphertextstringNoXJ(plaintext, clen));

                Key.initPathLookupAll(clen);
                Console.WriteLine("Plaintext Trigrams Score: {0}, Bigrams Score {1}, IC: {2}",
                        Key.triscore(ciphertext, clen, enigmaStats),
                        Key.biscore(ciphertext, clen, enigmaStats),
                        Key.icscore(ciphertext, clen));


            }
            else
            {
                Key Key = new Key(key);

                Array.Copy(ciphertext, 0, plaintext, 0, clen); // just for clarity
                if (indicatorS.Length != 0)
                {
                    short[] indicCrypt = new short[3];
                    short[] indicPlain = new short[3];
                    string indicCryptS;
                    string indicPlainS = indicatorS.ToUpper();

                    key.printKeystring("Key for Indicator:\t");
                    Console.WriteLine("Plain Indicator:     \t{0}", indicPlainS);
                    int ilen = EnigmaUtils.getText(indicPlainS, indicPlain);
                    key.encipherDecipherAll(indicPlain, indicCrypt, ilen);
                    indicCryptS = EnigmaUtils.getstring(indicCrypt, ilen);
                    Console.WriteLine("Encrypted Indicator: \t{0}", indicCryptS);

                    Key.setMesg(indicPlainS);

                    Key.printKeystring("Encrytion Key:\t");
                    Key.encipherDecipherAll(plaintext, ciphertext, clen);


                    Console.WriteLine("Encrypted Message: \t{0} {1} {2} {3}",
                            clen, key.getMesg(), indicCryptS, EnigmaUtils.getCiphertextstringInGroups(ciphertext, clen));
                }
                else
                {
                    Key.printKeystring("Encrytion Key:\t");
                    Key.encipherDecipherAll(plaintext, ciphertext, clen);
                    Console.WriteLine("Encrypted Message: \t{0} {1}",
                            clen, EnigmaUtils.getCiphertextstringInGroups(ciphertext, clen));
                }
            }
        }

        private static void createCommandLineArguments()
        {

            CommandLine.add(new CommandLine.Argument(
                    Flag.KEY,
                    "Key range or key",
                    "Range of keys, or specific key. Examples: range of M3 keys B:532:AAC:AAA-B:532:AAC:ZZZ,\r\n" +
                            "\t\tsingle M4 key B:B532:AAAC:AJKH, single H key with stecker B:532:AAC:JKH|ACFEHJKOLZ,  \r\n" +
                            "\t\tkey range with stecker B:532:AAC:AAA-B:532:AAC:ZZZ|ACFEHJKOLZ. When a range is specified, \r\n" +
                            "\t\tthe program will sweep for each field in the key (right to left), from the value specified on the left side of the range \r\n" +
                            "\t\tuntil it reaches the upper value specified in the right side of the range.\r\n" +
                            "\t\tFor example, to sweep all ring settings from AAA to AZZ (Model H):\r\n " +
                            "\t\t-" + Flag.KEY + " C:321:AAA:AAA-C:321:AZZ:AAA\r\n" +
                            "\t\tOr, to test all wheel combinations -" + Flag.KEY + " A:111:AAA:AAA-C:555:AZZ:ZZZ\r\n" +
                            "\t\tOr, to sweep only values for the middle message settings: -" + Flag.KEY + " C:321:ABC:HAK-C:321:ABC:HZK\r\n" +
                            "\t\tOr, to sweep only values for the middle and right wheels (other settings known): -" + Flag.KEY + " C:521:ABC:DEF-C:521:ABC:DEF\r\n" +
                            "\t\tNote that in a range, wheel numbers can be repeated (e.g. -" + Flag.KEY + " B:B111:AAAA:AAAA-B:B555:AAAA:ZZZZ)\r\n" +
                            "\t\twhile in a single key this is not allowed (-" + Flag.KEY + " B:522:AAC:JKH is invalid).\r\n" +
                            "\t\tKey format for Model H: u:www:rrr:mmm\r\n" +
                            "\t\t    where u is the reflector (A, B, or C), www are the 3 wheels from left to right (1 to 5, e.g. 321)  \r\n" +
                            "\t\t    rrr are the ring settings (e.g. AZC) and mmm are the message settings \r\n" +
                            "\t\tFor Model M3 = u:www:rrr:mmm \r\n" +
                            "\t\t    where u is the reflector (B or C), www are the 3 wheels from left to right (1 to 8, e.g. 851)  \r\n" +
                            "\t\t    rrr are the ring settings (e.g. AZC) and mmm are the message settings \r\n" +
                            "\t\tfor Model H4 = u:gwww:rrrr:mmmm \r\n" +
                            "\t\t    where u is the reflector (B), g is the greek wheel (B or G) \r\n" +
                            "\t\t    www are the wheels from left to right (1 to 8, e.g. 821)  \r\n" +
                            "\t\t    rrrr are the ring settings (e.g. AAZC) and mmmm are the message settings \r\n" +
                            "\t\tNote: For models H and M3, it is also possible to specify rings settings with numbers 01 to 26 (instead of A to Z).    \r\n" +
                            "\t\t    for example, -" + Flag.KEY + " b:413:021221:abc is equivalent to -" + Flag.KEY + " b:413:BLU:abc.   \r\n" +
                            " ",
                    true,
                    ""));

            CommandLine.add(new CommandLine.Argument(
                    Flag.MODEL,
                    "Enigma model",
                    "Enigma Model. H (Army Model), M3 (Navy 3 rotors) or M4 (Navy 4 rotors).",
                    false,
                    "H",
                    new string[] { "H", "M3", "M4", }));

            CommandLine.add(new CommandLine.Argument(
                    Flag.MODE,
                    "Search mode",
                    "Search mode (for the case these is no crib). " +
                            "\t\t\tHILLCLIMBING for hillclimbing search for steckers at each possible rotor setting - about 2-3,000 keys/sec. Effective with ciphertext with 125 or more letters.\r\n" +
                            "\t\t\t   Use -" + Flag.STRENGTH + " for a slower but more sensitive search.\r\n" +
                            "\t\t\tANNEALING for simulated annealing search - much slower than HILLCLIMBING (about 70-130 keys/sec), effective with short ciphertexts between 50 and 150 letters.\r\n" +
                            "\t\t\t   Use -" + Flag.STRENGTH + " for a slower but more sensitive search.\r\n" +
                            "\t\t\tOSTWALD for Ostwald method (about 30-60 keys/sec), use for short ciphertexts between 30 to 100 letters.\r\n" +
                            "\t\t\t   Use -" + Flag.STRENGTH + " for a slower but more sensitive search (1 for E-Stecker, 2 for N-Stecker, 3 for X-Stecker, 4 for R, 5 for S, 6 for I, 7 for A, 8 for T, 9 for O, 10 for U)\r\n" +
                            "\t\t\tTRIGRAMS look for rotor settings with best trigram score. The steckers must be specified in -" + Flag.KEY + ",\r\n" +
                            "\t\t\t   e.g. -" + Flag.KEY + " B:132:AAC:AAA-B:132:AAC:ZZZ|ACFEHJKOLZ.\r\n" +
                            "\t\t\tIC look for rotor settings with best Index of Coincidence. For cryptograms less than 500 letters, \r\n" +
                            "\t\t\t   the steckers must be specified in -" + Flag.KEY + ", e.g. -" + Flag.KEY + " B:132:AAC:AAA-B:132:AAC:ZZZ|ACFEHJKOLZ.\r\n" +
                            "\t\t\tBOMBE for crib/known-plaintext attach (extension of the Turing Bombe). \r\n" +
                            "\t\t\tINDICATORS for an attack on 1930-1938 double indicators (extension of Rejewski's method).\r\n" +
                            "\t\t\tINDICATORS1938 for an attack on 1938-1940 double indicators (extension of Zygalski's method).\r\n" +
                            "\t\t\tSCENARIO to create a simulated ciphertext/plaintext/indicators scenario (see -" + Flag.SCENARIO + "and -" + Flag.SCENARIO_PATH + " options).\r\n" +
                            "\t\t\tDECRYPT for simple decryption.",
                    false,
                    "DECRYPT",
                    new string[] { "HILLCLIMBING", "IC", "TRIGRAMS", "BOMBE", "INDICATORS", "INDICATORS1938", "SCENARIO", "DECRYPT", "ANNEALING", "OSTWALD" }));

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
                    "Known plaintext (crib) for attack using extended Turing Bombe." +
                            "\t\tThe position of the crib may be specified with -" + Flag.CRIB_POSITION + ". \r\n" +
                            "\t\tTo exclude one or more of the letters from menus, replace each unknown crib letter with a ? symbol\r\n" +
                            "\t\tFor example -" + Flag.CRIB + " eins???zwo specifies a crib of 10 letters but no menu links will be created for the 3 letters marked as ?.\r\n" +
                            "\t\tThe details of the menus can be printed usng-" + Flag.VERBOSE + " (only if a single key is given with -" + Flag.KEY + ", and not a range).\r\n",
                    false,
                    ""));

            CommandLine.add(new CommandLine.Argument(
                    Flag.CRIB_POSITION,
                    "Crib start position",
                    "Starting position of crib, or range of possible starting positions. 0 means first letter. Examples: \r\n" +
                            "\t\t\t-" + Flag.CRIB_POSITION + " 0 if crib starts at first letter,\r\n" +
                            "\t\t\t-" + Flag.CRIB_POSITION + " 10 if crib starts at the 11th letter, +\r\n" +
                            "\t\t\t-" + Flag.CRIB_POSITION + " 0-9 if crib may start at any of the first 10 positions,\r\n" +
                            "\t\t\t-" + Flag.CRIB_POSITION + " * if crib may start at any position.\r\n" +
                            "\t\tPosition(s) generating a menu conflict (letter encrypted to itself) are discarded. \r\n",
                    false,
                    "0"));

            CommandLine.add(new CommandLine.Argument(
                    Flag.INDICATORS_FILE,
                    "Full file path for indicators file.",
                    "File with set of indicators. The file should contain either groups of 6 letters (INDICATORS mode), or groups of 9 letters (INIDCATORS1938 mode). \r\n" +
                            "\t\tIf groups of encrypted double indicators with 6 letters are given, searches key according to the Cycle Characteristic method developed by the Rejewski before WWII.\r\n" +
                            "\t\t  Finds the daily key which creates cycles which match those of the database. Then finds stecker plugs which match the all indicators \r\n" +
                            "\t\t  If a ciphertext is provided, using the first (decrypted) indicator as the Message Key for that message  \r\n" +
                            "\t\t  perform a trigram-based search to find the Ring Settings and to decipher the message. \r\n" +
                            "\t\tIf groups of encrypted double indicators with of 3+6=9 letters are given, \r\n" +
                            "\t\t  finds daily key according to the Zygalski's Sheets method developed by the Poles before WWII.\r\n" +
                            "\t\t  Indicators include 3 letters for the key to encrypt the double message key, and 6 letters of the doubled encrypted message.\r\n" +
                            "\t\t  Will search for keys (wheel order, wing settings) which together with the keys in the indicator groups,\r\n" +
                            "\t\t  create 'female' patterns which match the database (those keys with females). Stecker settings are also detected, and the first key indicator (from the file)\r\n" +
                            "\t\t is used to decipher the ciphertext (if a ciphertext was provided).\r\n",
                    false,
                    ""));

            CommandLine.add(new CommandLine.Argument(
                    Flag.MESSAGE_INDICATOR,
                    "Message indicator options",
                    "Indicator sent with the ciphertext. Has two distinct purposes and forms: \r\n" +
                            "\t\t-w {3-letter encrypted indicator} e.g.-" + Flag.MESSAGE_INDICATOR + " STG.  This must be used together with a single key in -" + Flag.KEY + " in which the steckers were specified (e.g. -" + Flag.KEY + " B:532:AAC:JKH:ACFEHJKOLZ). \r\n" +
                            "\t\t    First, this indicator is decrypted using the given key (daily key), then the decrypted indicator is used as the message key to decrypt the full message. \r\n" +
                            "\t\t-w {3-letter message key}:{3-letter encrypted indicator} e.g.-" + Flag.MESSAGE_INDICATOR + " OWL:STG. In this form, this is used as an additional filter when searching for the best key.\r\n" +
                            "\t\t   Only messages keys which are a result of decrypting the encrypted indicator with the given message key are considered for the search.\r\n" +
                            "\t\t   Stecker board settings must be known and specified (e.g. B:532:AAA:AAA-B:532:AAZ:ZZZ|ACFEHJKOLZ). Not compatible with HILLCLIMBING/ANNEALING modes\r\n",

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
                    new string[] { "GERMAN", "ENGLISH", }));

            CommandLine.add(new CommandLine.Argument(
                    Flag.VERBOSE,
                    "Verbose",
                    "Show details of crib attack."));

            CommandLine.add(new CommandLine.Argument(
                    Flag.RIGHT_RING_SAMPLING,
                    "Left rotor sampling interval.",
                    "Check only a sample of right ring positions.-" + Flag.RIGHT_RING_SAMPLING + " {right ring interval value} {default - 1 - no sampling, check all positions in range}.\r\n" +
                            "\t\tIf the interval > 1, test only a sample of right ring positions in search.\r\n" +
                            "\t\tFor example -" + Flag.RIGHT_RING_SAMPLING + " 3 means that only one in three right ring positions will be tested.  \r\n" +
                            "\t\tThis is likely to still produce a partial or full decryption. \r\n" +
                            "\t\tShould be used with caution together with mode BOMBE (Bombe search for menu stops) as this may cause stops to be missed.\r\n ",
                    false,
                    1, 7, 1));
            CommandLine.add(new CommandLine.Argument(
                    Flag.MIDDLE_RING_SCOPE,
                    "Optimize middle rotor moves",
                    "Optimize middle rotor moves.-" + Flag.MIDDLE_RING_SCOPE + " {option} {default - 0 - no optimization}.\r\n" +
                            "\t\tReduce the number of middle rotor settings to be tested. \r\n" +
                            "\t\t-" + Flag.MIDDLE_RING_SCOPE + " 0 - No reduction, all middle rotor settings specified in the range will be tested.\r\n" +
                            "\t\t-" + Flag.MIDDLE_RING_SCOPE + " 1 - Test all middle rotor settings which generate a stepping of the left rotor, plus one settings which does NOT. \r\n" +
                            "\t\t       Reliable, no valid solutions will be missed, and reduces scope from 26 to {message length}/26+1 \r\n" +
                            "\t\t-" + Flag.MIDDLE_RING_SCOPE + " 2 - Test all middle rotor settings which generate a stepping of the left rotor affecting the first 1/5 or last 1/5 of the message, plus one more \r\n" +
                            "\t\t       setting which is not generating a stepping. This is a good compromise between speed and accuracy. Reduces scope from 26 to {message length}*0.40/26+1 \r\n" +
                            "\t\t-" + Flag.MIDDLE_RING_SCOPE + " 3 - Test one middle rotor setting which does NOT generate a stepping of the left rotor. Fastest and most agressive option since only one middle rotor setting\r\n" +
                            "\t\t       will be tested, but part of the message may be garbled if there was such a stepping originally. Good for short messages since probablity \r\n" +
                            "\t\t       for left rotor stepping is {message length}/676. Can save a lot of search time if successful. \r\n" +
                            "\t\t-" + Flag.MIDDLE_RING_SCOPE + " 4 - Test only all middle rotor settings which generate a stepping of the left rotor. \r\n" +
                            "\t\t       Usually not needed except for testing purposes. Reduces scope from 26 to {message length}/26 \r\n" +
                            "\t\t-" + Flag.MIDDLE_RING_SCOPE + " 5 - Test all middle rotor settings which do NOT generate a stepping of the left rotor. \r\n" +
                            "\t\tNote: The key range should specify the full range (A to Z) for the middle rotor, for any option other than 0. \r\n",
                    false,
                    0, 5, 0));

            CommandLine.add(new CommandLine.Argument(
                    Flag.SCENARIO,
                    "Generate random scenario",
                    "Generate simulated ciphertext and indicators.\r\n" +
                            "\t\tA range of keys must be selected (-" + Flag.KEY + ") from which a key is randomly selected for simulation. \r\n" +
                            "\t\tUsage: -" + Flag.SCENARIO + " {f}:{l}:{n}:{s}:{g}:{c}. \r\n" +
                            "\t\t{f} is the selected scenario: 1 to only generate a ciphertext, 2 for pre-1938 indicators (and a ciphertext), 3 to generate post 1938 doubled indicators (and a ciphertext). \r\n" +
                            "\t\t    Default is 1. Scenario 2 and 3 are not compatible with the {n} option.\r\n" +
                            "\t\t{l} is the length of the random ciphertext, or the combined length of all ciphertexts (default 150).\r\n" +
                            "\t\t{n} is the number of messages (a longer text will be split) for scenario 1, for scenario 2  and 3 this is the number of indicators to be generated.\r\n" +
                            "\t\t{s} is the number of Stecker Plugs (default 10).\r\n" +
                            "\t\t{g} is the percentage of garbled letters (default 0).\r\n" +
                            "\t\t{c} is the length of a crib (the plaintext at the beginning of the message).\r\n" +
                            "\t\t\r\n" +
                            "\t\tThe following files are created (<id> is the randomly generated scenario id): \r\n" +
                            "\t\t -'S<id>cipher.txt' ciphertext for a single message (not split)  \r\n" +
                            "\t\t   In case of long message which has been split several files (S<id>cipher1.txt, S<id>cipher2.txt etc.. will also be created). \r\n" +
                            "\t\t - S<id>indicators.txt with indicators, for scenario 2 (1938-1940) or 3 (pre-1938). \r\n" +
                            "\t\t   With scenario 2, the file contains groups of 6 letters (encrypted doubled keys).  \r\n" +
                            "\t\t   With scenario 3, it contains groups of 9 letters (indicator plus encrypted doubled key) are kept \r\n" +
                            "\t\t   The indicator used for the generated ciphertext is the first in that set. \r\n" +
                            "\t\t - S<id>plaintext.txt contains the plaintext. \r\n" +
                            "\t\t - S<id>challenge.txt contains all the elements of the challenge (messages with headers, crib, etc) without the solution.\r\n" +
                            "\t\t - S<id>solution.txt contains all the elements of the solution. \r\n" +
                            "\t\tExample: -" + Flag.SCENARIO + " 1:500:3:10:3:25 - generate plaintexts/ciphertexts, with total length of 500 split into 3 messages.\r\n" +
                            "\t\t   The stecker board has 10 plugs, 3 percent of the letters are garbled, a crib of 25 letters is given\r\n" +
                            "\t\tExample: -" + Flag.SCENARIO + " 2:50::6::0 - generate 50 pre-1938 doubled indicators, a single plaintext/ciphertext with 150 letters (default). \r\n" +
                            "\t\t   The stecker board has 6 plugs, no letters are garbled (default), no crib is given.\r\n",

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


        private static void incompatible(Mode mode, Flag[] flags)
        {
            foreach (Flag flag in flags)
            {
                if (CommandLine.isSet(flag))
                {
                    Console.WriteLine("Option -{0} ({1}) not supported for mode {2}", flag, CommandLine.getShortDesc(flag), mode);
                }
            }
        }

        private static void required(Mode mode, Key.Model currentModel, Key.Model[] models)
        {
            foreach (Key.Model model in models)
            {
                if (model == currentModel)
                {
                    return;
                }
            }
            Console.WriteLine("Mode {0} not supported for model {1}", mode, currentModel);
        }

        private static void required(Mode mode, Flag[] flags)
        {
            foreach (Flag flag in flags)
            {
                if (!CommandLine.isSet(flag))
                {
                    Console.WriteLine("Option -{0} ({1}) is mandatory with mode {2}", flag, CommandLine.getShortDesc(flag), mode);
                }
            }
        }

        private static void incompatibleWithRangeOkKeys(Flag[] flags)
        {
            foreach (Flag flag in flags)
            {
                if (CommandLine.isSet(flag))
                {
                    Console.WriteLine("Option -{0} ({1}) not supported for key range", flag, CommandLine.getShortDesc(flag));
                }
            }
        }

        private static void incompatibleWithSingleKey(Flag[] flags)
        {
            foreach (Flag flag in flags)
            {
                if (CommandLine.isSet(flag))
                {
                    Console.WriteLine("Option -{0} ({1}) requires a key range", flag, CommandLine.getShortDesc(flag));
                }
            }
        }

        private static void incompatibleWithRangeOkKeys(Mode currentMode, Mode[] modes)
        {
            foreach (Mode mode in modes)
            {
                if (mode == currentMode)
                {
                    Console.WriteLine("Mode {0} is not allowed with a key range", mode);
                }
            }

        }

        private static void incompatibleWithSingleKey(Mode currentMode, Mode[] modes)
        {
            foreach (Mode mode in modes)
            {
                if (mode == currentMode)
                {
                    Console.WriteLine("Mode {0} requires a key range", mode);
                }
            }
        }
    }
}
