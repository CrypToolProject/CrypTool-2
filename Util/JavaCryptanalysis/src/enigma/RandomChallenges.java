package enigma;

import common.CtAPI;

import java.util.Arrays;
import java.util.Random;


class RandomChallenges {
    final static int MAXMSG = 10;

    int opFormat = 1;
    int opLen = 200;
    int opSplit = 1;
    int opPlugs = 10;
    int opCrib = 0;
    int garbledLettersPercentage = 0;

    int nMessageKeys;

    RandomChallenges(String SCENARIO_PATH, String plainInputFile, Key lowKey, Key highKey, String randomCryptOptions) {
        
        int values[] = new int[15];
        Arrays.fill(values, -1);

        randomCryptOptions += "::::::::::"; // to simplify parsing

        String[] parts = randomCryptOptions.split(":");
        for (int i = 0; i < parts.length; i++) {
            if (parts[i].isEmpty()) {
                values[i] = -1;
                continue;
            }
            try {
                values[i] = Integer.valueOf(parts[i]);
            } catch (NumberFormatException e) {
                CtAPI.goodbyeFatalError("RANDOM SCENARIO: %s - invalid - should include numbers separated by\n", randomCryptOptions);
            }
        }
        if (values[0] != -1)
            opFormat = values[0];
        if (values[1] != -1)
            opLen = values[1];
        if (values[2] != -1)
            opSplit = values[2];
        if (values[3] != -1)
            opPlugs = values[3];
        if (values[4] != -1)
            garbledLettersPercentage = values[4];
        if (values[5] != -1)
            opCrib = values[5];

        if (opFormat == 1)
            nMessageKeys = MAXMSG;
        else {
            nMessageKeys = opSplit;
            opSplit = 1;
        }


        CtAPI.printf("RANDOM SCENARIO: Format = %d, Len = %d, Split = %d, Plugs = %d, Garbled Percent = %d, Crib. length= %d\n",
                opFormat, opLen, opSplit, opPlugs, garbledLettersPercentage, opCrib);
        
        if ((opFormat < 1) || (opFormat > 3)) {
            CtAPI.goodbyeFatalError("RANDOM SCENARIO: INVALID OPTIONS %s , Format must be 0, 1, 2 or 3\n", randomCryptOptions);
            return;
        }
        if ((opLen < 5) || (opLen > 2500)) {
            CtAPI.goodbyeFatalError("RANDOM SCENARIO: INVALID OPTIONS %s , Length must be 5 to 2500\n", randomCryptOptions);
            return;
        }
        if ((opFormat == 1) || (opFormat == 0)) {
            if ((opSplit < 1) || (opSplit > 5)) {
                CtAPI.goodbyeFatalError("RANDOM SCENARIO: INVALID OPTIONS %s , Number of (split) messages must be 1 to 5 (in Format 1)\n", randomCryptOptions);
                return;
            }
        } else {
            if ((opSplit < 1) || (opSplit > 500)) {
                CtAPI.goodbyeFatalError("RANDOM SCENARIO: INVALID OPTIONS %s , Number of indicators must be between 1 to 500 (in Format 2 and 3)\n", randomCryptOptions);
                return;
            }
        }

        if ((opPlugs < 0) || (opPlugs > 13)) {
            CtAPI.goodbyeFatalError("RANDOM SCENARIO: INVALID OPTIONS %s , Number of plugs must be 0 to 13\n", randomCryptOptions);
            return;
        }
        if ((opCrib < 0) || (opCrib > 50)) {
            CtAPI.goodbyeFatalError("RANDOM SCENARIO: INVALID OPTIONS %s , Crib length must be 0 to 50\n", randomCryptOptions);
            return;
        }

        if ((garbledLettersPercentage < 0) || (garbledLettersPercentage > 50)) {
            CtAPI.goodbyeFatalError("RANDOM SCENARIO: INVALID OPTIONS %s , Percentage of garbled letters should be 0 to 50\n", randomCryptOptions);
            return;
        }


        boolean MessageKeyFullValueRange = true;

        int A = Utils.getIndex('A');
        int Z = Utils.getIndex('Z');
        if ((lowKey.lMesg != A) || (lowKey.mMesg != A) || (lowKey.rMesg != A))
            MessageKeyFullValueRange = false;
        if ((highKey.lMesg != Z) || (highKey.mMesg != Z) || (highKey.rMesg != Z))
            MessageKeyFullValueRange = false;

        if ((opFormat != 1) && !MessageKeyFullValueRange) {
            CtAPI.goodbyeFatalError("RANDOM CIPHER: For formats 2 and 3, the full range of values for Message Key should be specified (aaa to zzz) \n");
            return;

        }

        boolean MessageKeySingleValueInRange = false;

        if ((lowKey.lMesg == highKey.lMesg) &&
                (lowKey.mMesg == highKey.mMesg) &&
                (lowKey.rMesg == highKey.rMesg))
            MessageKeySingleValueInRange = true;

        if ((opSplit > 1) && MessageKeySingleValueInRange) {
            CtAPI.goodbyeFatalError("RANDOM CIPHER: A range of values for Message Key should be specified, if Split Number is more than 1\n");
            return;

        }


        byte[] plaintext = new byte[Key.MAXLEN];
        
       int actualLen;

        actualLen = Utils.loadRandomText(plainInputFile, plaintext, opLen, true /*boolean generateXs */, garbledLettersPercentage);
        if (actualLen == 0) {
            CtAPI.goodbyeFatalError("ERROR in Random cryptogram generation mode\n");
            return;
        }

        // message keys for formats (2) and (3) - random + stereotyped
        String lines[] = {"QWERTZUIO", "ASDFGHJK", "PYXCVBNML"};
        String[] stereotypedMessageKeys = new String[250];
        int nStereotypedMessageKeys = 0;

        for (int i = 0; i < 26; i++) { //  26+24
            stereotypedMessageKeys[nStereotypedMessageKeys++] = "" + Utils.getChar(i) + Utils.getChar(i) + Utils.getChar(i);
            if (i < 24)
                stereotypedMessageKeys[nStereotypedMessageKeys++] = "" + Utils.getChar(i) + Utils.getChar(i + 1) + Utils.getChar(i + 2);
        }
        for (int i = 0; i < 9; i++) {// 18
            if (i < 8)
                stereotypedMessageKeys[nStereotypedMessageKeys++] = "" + lines[0].charAt(i) + lines[1].charAt(i) + lines[2].charAt(i + 1);
            if (i > 0)
                stereotypedMessageKeys[nStereotypedMessageKeys++] = "" + lines[0].charAt(i) + lines[1].charAt(i - 1) + lines[2].charAt(i - 1);
        }

        Random random = new Random();

        byte[][] messagePlainText = new byte[MAXMSG][Key.MAXLEN]; // plain text for split message.
        String[] messagePlainTextInLines = new String[MAXMSG]; // string plain text for split messages, separated into lines.
        int[] messageTri = new int[MAXMSG];
        double[] messageIc = new double[MAXMSG];
        String[] messageKeys = new String[500]; // also used for the messages/message splits

        int[] messageLength = new int[MAXMSG];
        String[] messageCipherInGroups = new String[MAXMSG];
        String plainIndicator[] = new String[500]; // also used for the message/message splits
        String encipheredDoubledMessageKeys[] = new String[500]; // for modes 2 and 3. (3 - all enciphered using daily key)
        String cribS = "";

        Key dailyKey = new Key();  // format 1 & 2 - it does not contain message settings
        dailyKey.initRandom(lowKey, highKey, opPlugs);// generate the daily key for all cases
        dailyKey.setRandomMesg(); // random daily Message Key

        //!!!!!!!!!!!!!!!!!!!!!!!!!!
        dailyKey.lMesg = 0;

        // in case format 1 we need only a limited number of message keys, in the range provided
        Key key = new Key();
        if (opFormat == 1) {
            for (int i = 0; i < nMessageKeys; i++) {
                key.initRandom(lowKey, highKey, opPlugs); // here we want the message key in the range
                messageKeys[i] = key.getMesg();
            }
        } else {
            for (int i = 0; i < nMessageKeys; i++) {
                int rand1 = random.nextInt(100);
                int rand2 = random.nextInt(nStereotypedMessageKeys);

                if ((rand1 < 30) || // at lest 30% non stereotyped
                        (i == 0) ||     // the one used for the message will be non-stereotyped
                        (stereotypedMessageKeys[rand2] == null)) { // stereotyped but alreadu used ....
                    key.setRandomMesg();
                    messageKeys[i] = key.getMesg();
                } else {
                    messageKeys[i] = stereotypedMessageKeys[rand2];
                    stereotypedMessageKeys[rand2] = null; // we dont want to use twice....
                }
            }

        }

        for (int i = 0; i < nMessageKeys; i++) {
            if (opFormat == 3) {
                // plain indicators are full random....
                // could add herivel tip on multiple part messages
                key.setRandomMesg();
                plainIndicator[i] = key.getMesg();
            } else {
                plainIndicator[i] = dailyKey.getMesg();
            }
        }

        for (int i = 0; i < nMessageKeys; i++) {
            byte plain[] = new byte[6];
            byte crypt[] = new byte[6];
            Utils.getText(messageKeys[i] + messageKeys[i], plain);

            key = new Key(dailyKey); // to get everything except the message key.
            key.setMesg(plainIndicator[i]); // note - in mode 3 the message key is always the same (the daily message key)
            key.encipherDecipherAll(plain, crypt, 6);

            encipheredDoubledMessageKeys[i] = Utils.getString(crypt, 6);
        }

        if (actualLen < 30)
            opSplit = 1;

        if ((opSplit != 1) && (actualLen / opSplit >= 250)) // unless specified as 1, we split messages if bigger than 250
            opSplit = actualLen / 250 + 1;


        // splits messages if needed
        if (opSplit == 1) {
            messageLength[0] = actualLen;
            System.arraycopy(plaintext, 0, messagePlainText[0], 0, actualLen);
        } else {
            messageLength[opSplit - 1] = Math.min(actualLen / 3, 20 + random.nextInt(50));
            int splitLen = (actualLen - messageLength[opSplit - 1]) / (opSplit - 1);
            int left = actualLen - messageLength[opSplit - 1];
            for (int i = opSplit - 2; i >= 0; i--) {
                if (i == 0)
                    messageLength[i] = left;
                else {
                    messageLength[i] = splitLen;
                    left -= splitLen;
                }
            }
            int pos = 0;
            for (int i = 0; i < opSplit; i++) {
                System.arraycopy(plaintext, pos, messagePlainText[i], 0, messageLength[i]);
                pos += messageLength[i];
            }
        }
        if (opCrib > 0) {
            if (opCrib > opLen)
                opCrib = opLen;

            for (int i = 0; i < opCrib; i++)
                cribS += Utils.getChar(plaintext[i]);
        }


        key = new Key(dailyKey); // to get everything except the message key.
        // encipher all the messages/splits, calc tri and IC
        for (int i = 0; i < opSplit; i++) {
            byte[] messageCipherText = new byte[Key.MAXLEN]; // cipher text for each message.
            key.setMesg(messageKeys[i]);
            key.encipherDecipherAll(messagePlainText[i], messageCipherText, messageLength[i]);
            // tri and ic must be done after encryption since the functions first decrypt
            messageTri[i] = key.triScoreWithoutLookupBuild(messageCipherText, messageLength[i]);
            messageIc[i] = key.icScoreWithoutLookupBuild(messageCipherText, messageLength[i]);

            messageCipherInGroups[i] = "";
            for (int p = 0; p < messageLength[i]; p++) {
                if ((p % 5) == 0)
                    messageCipherInGroups[i] += " ";
                messageCipherInGroups[i] += Utils.getChar(messageCipherText[p]);
                if ((p % 25) == 24)
                    messageCipherInGroups[i] += "\n";
            }

            messagePlainTextInLines[i] = "";
            int lastNewLinePos = 0;
            for (int p = 0; p < messageLength[i]; p++) {
                messagePlainTextInLines[i] += Utils.getChar(messagePlainText[i][p]);
                if (((p - lastNewLinePos) > 100) && (Utils.getChar(messagePlainText[i][p]) == 'X')) {
                    messagePlainTextInLines[i] += "\n";
                    lastNewLinePos = p;
                }
            }
            if (i == 0) {
                CtAPI.displayBestKey(key.getKeyStringLong());
            }
        }

        String challengeFileS = "";
        String solutionFileS = "";
        String indicatorFileS = "";

        String formatStr = "";
        if (opFormat == 1)
            formatStr = "Regular Procedure - Plain indicator followed by the encrypted Message Key - no doubling (from 1940)";
        else if (opFormat == 2)
            formatStr = "Plain indicator followed by encrypted DOUBLED Message Key - in use from 1938 until 1940. Can be solved using Zygalski Sheets";
        else if (opFormat == 3)
            formatStr = "DOUBLED encrypted Message Key (encrypted using a daily Message Key) - in use until 1938. Can be solved using Cycle Patterns";

        int id = (int) (System.currentTimeMillis() % 1000000);
        String cipherFilePath = SCENARIO_PATH + "\\"+"S" + id +"cipher.txt";
        String indicatorsFilePath = SCENARIO_PATH + "\\"+ "S" + id + "indicators.txt";;
        String plaintextFilePath = SCENARIO_PATH + "\\"+"S" + id +"plaintext.txt";;
        String challengeFilePath = SCENARIO_PATH + "\\"+"S" + id +"challenge.txt";;
        String solutionFilePath = SCENARIO_PATH + "\\"+"S" + id +"solution.txt";;

        solutionFileS +=
                "Challenge ID: " + id + " \n" +
                        "Indicator Procedure: " + formatStr + "\n" +
                        "Total Length: " + opLen + " split into  " + opSplit + " messages. \n" +
                        "Stecker Board Plugs: " + opPlugs + "\n";
        if (opCrib > 0)
            solutionFileS += "Crib for first message using " + opCrib + " numbers in words followed by X\nCrib: " + cribS + "\n";
        solutionFileS += "Full plain text:\n" + Utils.getCiphertextStringNoXJ(plaintext, actualLen) + "\n\n\n";


        if (opFormat == 3)
            dailyKey.rMesg = dailyKey.lMesg = dailyKey.mMesg = -1;
        solutionFileS += "Daily Key: " + dailyKey.getKeyStringLong() + "\n\n";

        challengeFileS +=
                "Challenge ID: " + id + " \n" +
                        "Indicator Procedure: " + formatStr + "\n" +
                        "Total Length: " + opLen + " split into  " + opSplit + " messages. \n\n";

        for (int split = 0; split < opSplit; split++) {

            String challengeIndicS = "";

            if (opFormat == 1) {
                challengeIndicS = " Indicator: " + plainIndicator[split] + " " + encipheredDoubledMessageKeys[split].substring(0, 3);
            } else if (opFormat == 3) {
                challengeIndicS = " Indicator: " + plainIndicator[split] + " " + encipheredDoubledMessageKeys[split].substring(0, 3) + " " +
                        encipheredDoubledMessageKeys[split].substring(3, 6);
                challengeIndicS += "(=<Plain Indicator> + <Message Key Encrypted twice by plain indicator>)\n";

            } else if (opFormat == 2) {
                challengeIndicS = " Indicator: " + encipheredDoubledMessageKeys[split].substring(0, 3) + " " + encipheredDoubledMessageKeys[split].substring(3, 6);
                challengeIndicS += "(=<Message Key encrypted twice using daily Message Key>)\n";

            }


            solutionFileS += "\n\n\nMessage Number: " + ("" + (split + 1)) + "\n";
            if (opFormat == 1)
                solutionFileS += "Message Key: " + messageKeys[split] + " Message Key is encrypted with plain indicator: " + plainIndicator[split] + " " + " (Encrypted Msg Key: " + encipheredDoubledMessageKeys[split].substring(0, 3) + ") ";
            else
                solutionFileS += "Message Key: " + messageKeys[split] + " Message Key is encrypted with plain indicator: " + plainIndicator[split] + " " + " (Encrypted Doubled Msg Key: " + encipheredDoubledMessageKeys[split].substring(0, 6) + ") ";

            solutionFileS += "Trigram Score: " + messageTri[split] + " IC Score: " + messageIc[split] + "\n\n";
            solutionFileS += messagePlainTextInLines[split] + "\n";

            challengeFileS += "\nMessage Number: " + ("" + (split + 1)) + " of " + opSplit + " Length: " + messageLength[split];
            challengeFileS += challengeIndicS;
            challengeFileS += "\n\n" + messageCipherInGroups[split];

            challengeFileS += "\n\n";

            if ((opCrib > 0) && (split == 0)) {
                challengeFileS += "\nCrib provided for this message (message 1): " + cribS + " at position: 0\n\n\n";
                solutionFileS += "\nCrib: " + cribS + " at position: 0\n\n";
            }


            if (split == 0) {
                Utils.saveToFile(cipherFilePath, messageCipherInGroups[split]);
                Utils.saveToFile(plaintextFilePath, messagePlainTextInLines[split].replaceAll("\n", ""));
                Utils.saveToFile(cipherFilePath, messageCipherInGroups[split]);
            }

            String cipherSplitFilePath = SCENARIO_PATH + "\\"+ "S" + id + "cipher" + ("" + (split + 1)) + ".txt";
            Utils.saveToFile(cipherSplitFilePath, messageCipherInGroups[split]);


        }

        solutionFileS += "\n\nIndicators (non encrypted)\n\n";

        if (opFormat != 1) {
            for (int i = 0; i < nMessageKeys; i++) {
                if (opFormat == 3)
                    indicatorFileS += plainIndicator[i] + " ";
                indicatorFileS += encipheredDoubledMessageKeys[i] + "     ";
                if (((i % 10) == 9) || (i == (nMessageKeys - 1)))
                    indicatorFileS += "\n";

                if (opFormat == 3)
                    solutionFileS += plainIndicator[i] + " ";
                solutionFileS += messageKeys[i] + "    ";
                if (((i % 10) == 9) || (i == (nMessageKeys - 1)))
                    solutionFileS += "\n";

            }

        }

        if (opFormat > 1)
            CtAPI.printf("\n======= solution\n\n%s\n====== end of solution\n\n\n=======challenge\n%s\n\n\n======== end of challenge\n\n\n======= indicators\n\n%s\n\n======= end of indicators\n",
                    solutionFileS, challengeFileS, indicatorFileS);
        else
            CtAPI.printf("\n======= solution\n\n%s\n====== end of solution\n\n\n=======challenge\n%s\n\n\n======== end of challenge.txt\n\n",
                    solutionFileS, challengeFileS);

        Utils.saveToFile(challengeFilePath, challengeFileS);
        if (opFormat > 1)
            Utils.saveToFile(indicatorsFilePath, indicatorFileS);

        Utils.saveToFile(solutionFilePath, solutionFileS + challengeFileS + indicatorFileS);

        CtAPI.printf("Saved %s\n", cipherFilePath);
        CtAPI.printf("Saved %s\n", plaintextFilePath);
        if (opFormat > 1)
            CtAPI.printf("Saved %s\n", indicatorsFilePath);
        CtAPI.printf("Saved %s\n", challengeFilePath);
        CtAPI.printf("Saved %s\n", solutionFilePath);
        CtAPI.displayBestPlaintext("Saved " + cipherFilePath + ", "+ plaintextFilePath + ", "
                + ((opFormat > 1) ? indicatorsFilePath + ", " : "") + challengeFilePath + ", "+ solutionFilePath + ", \n" +  "Crib at pos 0: " + cribS);

        if (opFormat == 2) {
            byte[] steppings = new byte[Key.MAXLEN];
            dailyKey.showSteppings(steppings, 6);
            String steppingsS = Utils.getCiphertextStringNoXJ(steppings, 6);
            System.out.printf("123456\n%s\n", steppingsS);
            dailyKey.printKeyString("Daily Key: ");
        } else if (opFormat == 3) {
            dailyKey.printKeyString("Daily Key: ");
        }
    }

}
