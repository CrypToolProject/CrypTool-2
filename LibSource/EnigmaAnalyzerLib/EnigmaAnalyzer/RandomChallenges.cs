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

namespace EnigmaAnalyzerLib
{
    public class RandomChallenges
    {
        private static readonly int MAXMSG = 10;
        private readonly int opFormat = 1;
        private readonly int opLen = 200;
        private readonly int opSplit = 1;
        private readonly int opPlugs = 10;
        private readonly int opCrib = 0;
        private readonly int garbledLettersPercentage = 0;
        private readonly int nMessageKeys;

        public RandomChallenges(string SCENARIO_PATH, string plainInputFile, Key lowKey, Key highKey, string randomCryptOptions, EnigmaStats enigmaStats, ResultReporter resultReporter)
        {
            int[] values = new int[15];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = -1;
            }

            randomCryptOptions += "::::::::::"; // to simplify parsing

            string[] parts = randomCryptOptions.Split(':');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length == 0)
                {
                    values[i] = -1;
                    continue;
                }
                try
                {
                    values[i] = int.Parse(parts[i]);
                }
                catch (Exception)
                {
                    Console.WriteLine("RANDOM SCENARIO: {0} - invalid - should include numbers separated by", randomCryptOptions);
                }
            }
            if (values[0] != -1)
            {
                opFormat = values[0];
            }
            if (values[1] != -1)
            {
                opLen = values[1];
            }
            if (values[2] != -1)
            {
                opSplit = values[2];
            }
            if (values[3] != -1)
            {
                opPlugs = values[3];
            }
            if (values[4] != -1)
            {
                garbledLettersPercentage = values[4];
            }
            if (values[5] != -1)
            {
                opCrib = values[5];
            }
            if (opFormat == 1)
            {
                nMessageKeys = MAXMSG;
            }
            else
            {
                nMessageKeys = opSplit;
                opSplit = 1;
            }

            Console.WriteLine("RANDOM SCENARIO: Format = {0}, Len = {1}, Split = {2}, Plugs = {3}, Garbled Percent = {4}, Crib. length= {5}",
                    opFormat, opLen, opSplit, opPlugs, garbledLettersPercentage, opCrib);

            if ((opFormat < 1) || (opFormat > 3))
            {
                Console.WriteLine("RANDOM SCENARIO: INVALID OPTIONS {0} , Format must be 0, 1, 2 or 3", randomCryptOptions);
                return;
            }
            if ((opLen < 5) || (opLen > 2500))
            {
                Console.WriteLine("RANDOM SCENARIO: INVALID OPTIONS {0} , Length must be 5 to 2500", randomCryptOptions);
                return;
            }
            if ((opFormat == 1) || (opFormat == 0))
            {
                if ((opSplit < 1) || (opSplit > 5))
                {
                    Console.WriteLine("RANDOM SCENARIO: INVALID OPTIONS {0} , Number of (split) messages must be 1 to 5 (in Format 1)", randomCryptOptions);
                    return;
                }
            }
            else
            {
                if ((opSplit < 1) || (opSplit > 500))
                {
                    Console.WriteLine("RANDOM SCENARIO: INVALID OPTIONS {0} , Number of indicators must be between 1 to 500 (in Format 2 and 3)", randomCryptOptions);
                    return;
                }
            }

            if ((opPlugs < 0) || (opPlugs > 13))
            {
                Console.WriteLine("RANDOM SCENARIO: INVALID OPTIONS {0} , Number of plugs must be 0 to 13", randomCryptOptions);
                return;
            }
            if ((opCrib < 0) || (opCrib > 50))
            {
                Console.WriteLine("RANDOM SCENARIO: INVALID OPTIONS {0} , Crib length must be 0 to 50", randomCryptOptions);
                return;
            }

            if ((garbledLettersPercentage < 0) || (garbledLettersPercentage > 50))
            {
                Console.WriteLine("RANDOM SCENARIO: INVALID OPTIONS {0} , Percentage of garbled letters should be 0 to 50", randomCryptOptions);
                return;
            }


            bool MessageKeyFullValueRange = true;

            int A = EnigmaUtils.getIndex('A');
            int Z = EnigmaUtils.getIndex('Z');
            if ((lowKey.lMesg != A) || (lowKey.mMesg != A) || (lowKey.rMesg != A))
            {
                MessageKeyFullValueRange = false;
            }
            if ((highKey.lMesg != Z) || (highKey.mMesg != Z) || (highKey.rMesg != Z))
            {
                MessageKeyFullValueRange = false;
            }

            if ((opFormat != 1) && !MessageKeyFullValueRange)
            {
                Console.WriteLine("RANDOM CIPHER: For formats 2 and 3, the full range of values for Message Key should be specified (aaa to zzz) ");
                return;
            }

            bool MessageKeySingleValueInRange = false;

            if ((lowKey.lMesg == highKey.lMesg) &&
                    (lowKey.mMesg == highKey.mMesg) &&
                    (lowKey.rMesg == highKey.rMesg))
            {
                MessageKeySingleValueInRange = true;
            }

            if ((opSplit > 1) && MessageKeySingleValueInRange)
            {
                Console.WriteLine("RANDOM CIPHER: A range of values for Message Key should be specified, if Split Number is more than 1");
                return;
            }

            short[] plaintext = new short[Key.MAXLEN];

            int actualLen;

            actualLen = EnigmaUtils.loadRandomText(plainInputFile, plaintext, opLen, true /*bool generateXs */, garbledLettersPercentage);
            if (actualLen == 0)
            {
                Console.WriteLine("ERROR in Random cryptogram generation mode");
                return;
            }

            // message keys for formats (2) and (3) - random + stereotyped
            string[] lines = { "QWERTZUIO", "ASDFGHJK", "PYXCVBNML" };
            string[] stereotypedMessageKeys = new string[250];
            int nStereotypedMessageKeys = 0;

            for (int i = 0; i < 26; i++)
            { //  26+24
                stereotypedMessageKeys[nStereotypedMessageKeys++] = "" + EnigmaUtils.getChar(i) + EnigmaUtils.getChar(i) + EnigmaUtils.getChar(i);
                if (i < 24)
                {
                    stereotypedMessageKeys[nStereotypedMessageKeys++] = "" + EnigmaUtils.getChar(i) + EnigmaUtils.getChar(i + 1) + EnigmaUtils.getChar(i + 2);
                }
            }
            for (int i = 0; i < 9; i++)
            {// 18
                if (i < 8)
                {
                    stereotypedMessageKeys[nStereotypedMessageKeys++] = "" + lines[0][i] + lines[1][i] + lines[2][i + 1];
                }
                if (i > 0)
                {
                    stereotypedMessageKeys[nStereotypedMessageKeys++] = "" + lines[0][i] + lines[1][i - 1] + lines[2][i - 1];
                }
            }

            Random random = new Random();

            short[][] messagePlainText = new short[MAXMSG][]; // plain text for split message.
            for (int i = 0; i < messagePlainText.Length; i++)
            {
                messagePlainText[i] = new short[Key.MAXLEN];
            }
            string[] messagePlainTextInLines = new string[MAXMSG]; // string plain text for split messages, separated into lines.
            int[] messageTri = new int[MAXMSG];
            double[] messageIc = new double[MAXMSG];
            string[] messageKeys = new string[500]; // also used for the messages/message splits

            int[] messageLength = new int[MAXMSG];
            string[] messageCipherInGroups = new string[MAXMSG];
            string[] plainIndicator = new string[500]; // also used for the message/message splits
            string[] encipheredDoubledMessageKeys = new string[500]; // for modes 2 and 3. (3 - all enciphered using daily key)
            string cribS = "";

            Key dailyKey = new Key();  // format 1 & 2 - it does not contain message settings
            dailyKey.initRandom(lowKey, highKey, opPlugs);// generate the daily key for all cases
            dailyKey.setRandomMesg(); // random daily Message Key

            //!!!!!!!!!!!!!!!!!!!!!!!!!!
            dailyKey.lMesg = 0;

            // in case format 1 we need only a limited number of message keys, in the range provided
            Key key = new Key();
            if (opFormat == 1)
            {
                for (int i = 0; i < nMessageKeys; i++)
                {
                    key.initRandom(lowKey, highKey, opPlugs); // here we want the message key in the range
                    messageKeys[i] = key.getMesg();
                }
            }
            else
            {
                for (int i = 0; i < nMessageKeys; i++)
                {
                    int rand1 = random.Next(100);
                    int rand2 = random.Next(nStereotypedMessageKeys);

                    if ((rand1 < 30) || // at lest 30% non stereotyped
                            (i == 0) ||     // the one used for the message will be non-stereotyped
                            (stereotypedMessageKeys[rand2] == null))
                    { // stereotyped but alreadu used ....
                        key.setRandomMesg();
                        messageKeys[i] = key.getMesg();
                    }
                    else
                    {
                        messageKeys[i] = stereotypedMessageKeys[rand2];
                        stereotypedMessageKeys[rand2] = null; // we dont want to use twice....
                    }
                }
            }

            for (int i = 0; i < nMessageKeys; i++)
            {
                if (opFormat == 3)
                {
                    // plain indicators are full random....
                    // could add herivel tip on multiple part messages
                    key.setRandomMesg();
                    plainIndicator[i] = key.getMesg();
                }
                else
                {
                    plainIndicator[i] = dailyKey.getMesg();
                }
            }

            for (int i = 0; i < nMessageKeys; i++)
            {
                short[] plain = new short[6];
                short[] crypt = new short[6];
                EnigmaUtils.getText(messageKeys[i] + messageKeys[i], plain);

                key = new Key(dailyKey); // to get everything except the message key.
                key.setMesg(plainIndicator[i]); // note - in mode 3 the message key is always the same (the daily message key)
                key.encipherDecipherAll(plain, crypt, 6);

                encipheredDoubledMessageKeys[i] = EnigmaUtils.getstring(crypt, 6);
            }

            if (actualLen < 30)
            {
                opSplit = 1;
            }

            if ((opSplit != 1) && (actualLen / opSplit >= 250)) // unless specified as 1, we split messages if bigger than 250
            {
                opSplit = actualLen / 250 + 1;
            }


            // splits messages if needed
            if (opSplit == 1)
            {
                messageLength[0] = actualLen;
                Array.Copy(plaintext, 0, messagePlainText[0], 0, actualLen);
            }
            else
            {
                messageLength[opSplit - 1] = Math.Min(actualLen / 3, 20 + random.Next(50));
                int splitLen = (actualLen - messageLength[opSplit - 1]) / (opSplit - 1);
                int left = actualLen - messageLength[opSplit - 1];
                for (int i = opSplit - 2; i >= 0; i--)
                {
                    if (i == 0)
                    {
                        messageLength[i] = left;
                    }
                    else
                    {
                        messageLength[i] = splitLen;
                        left -= splitLen;
                    }
                }
                int pos = 0;
                for (int i = 0; i < opSplit; i++)
                {
                    Array.Copy(plaintext, pos, messagePlainText[i], 0, messageLength[i]);
                    pos += messageLength[i];
                }
            }
            if (opCrib > 0)
            {
                if (opCrib > opLen)
                {
                    opCrib = opLen;
                }

                for (int i = 0; i < opCrib; i++)
                {
                    cribS += EnigmaUtils.getChar(plaintext[i]);
                }
            }

            key = new Key(dailyKey); // to get everything except the message key.
                                     // encipher all the messages/splits, calc tri and IC
            for (int i = 0; i < opSplit; i++)
            {
                short[] messageCipherText = new short[Key.MAXLEN]; // cipher text for each message.
                key.setMesg(messageKeys[i]);
                key.encipherDecipherAll(messagePlainText[i], messageCipherText, messageLength[i]);
                // tri and ic must be done after encryption since the functions first decrypt
                messageTri[i] = key.triScoreWithoutLookupBuild(messageCipherText, messageLength[i], enigmaStats);
                messageIc[i] = key.icScoreWithoutLookupBuild(messageCipherText, messageLength[i]);

                messageCipherInGroups[i] = "";
                for (int p = 0; p < messageLength[i]; p++)
                {
                    if ((p % 5) == 0)
                    {
                        messageCipherInGroups[i] += " ";
                    }
                    messageCipherInGroups[i] += EnigmaUtils.getChar(messageCipherText[p]);
                    if ((p % 25) == 24)
                    {
                        messageCipherInGroups[i] += "";
                    }
                }

                messagePlainTextInLines[i] = "";
                int lastNewLinePos = 0;
                for (int p = 0; p < messageLength[i]; p++)
                {
                    messagePlainTextInLines[i] += EnigmaUtils.getChar(messagePlainText[i][p]);
                    if (((p - lastNewLinePos) > 100) && (EnigmaUtils.getChar(messagePlainText[i][p]) == 'X'))
                    {
                        messagePlainTextInLines[i] += "";
                        lastNewLinePos = p;
                    }
                }
                if (i == 0)
                {
                    resultReporter.displayBestKey(key.getKeystringlong());
                }
            }

            string challengeFileS = "";
            string solutionFileS = "";
            string indicatorFileS = "";

            string formatStr = "";
            if (opFormat == 1)
            {
                formatStr = "Regular Procedure - Plain indicator followed by the encrypted Message Key - no doubling (from 1940)";
            }
            else if (opFormat == 2)
            {
                formatStr = "Plain indicator followed by encrypted DOUBLED Message Key - in use from 1938 until 1940. Can be solved using Zygalski Sheets";
            }
            else if (opFormat == 3)
            {
                formatStr = "DOUBLED encrypted Message Key (encrypted using a daily Message Key) - in use until 1938. Can be solved using Cycle Patterns";
            }

            int id = random.Next(1000000);
            string cipherFilePath = SCENARIO_PATH + "\\" + "S" + id + "cipher.txt";
            string indicatorsFilePath = SCENARIO_PATH + "\\" + "S" + id + "indicators.txt"; ;
            string plaintextFilePath = SCENARIO_PATH + "\\" + "S" + id + "plaintext.txt"; ;
            string challengeFilePath = SCENARIO_PATH + "\\" + "S" + id + "challenge.txt"; ;
            string solutionFilePath = SCENARIO_PATH + "\\" + "S" + id + "solution.txt"; ;

            solutionFileS +=
                    "Challenge ID: " + id + " " +
                            "Indicator Procedure: " + formatStr + "" +
                            "Total Length: " + opLen + " split into  " + opSplit + " messages. " +
                            "Stecker Board Plugs: " + opPlugs + "";
            if (opCrib > 0)
            {
                solutionFileS += "Crib for first message using " + opCrib + " numbers in words followed by XCrib: " + cribS + "";
            }
            solutionFileS += "Full plain text:" + EnigmaUtils.getCiphertextstringNoXJ(plaintext, actualLen) + "";


            if (opFormat == 3)
            {
                dailyKey.rMesg = dailyKey.lMesg = dailyKey.mMesg = -1;
            }
            solutionFileS += "Daily Key: " + dailyKey.getKeystringlong() + "";

            challengeFileS +=
                    "Challenge ID: " + id + " " +
                            "Indicator Procedure: " + formatStr + "" +
                            "Total Length: " + opLen + " split into  " + opSplit + " messages. ";

            for (int split = 0; split < opSplit; split++)
            {
                string challengeIndicS = "";
                if (opFormat == 1)
                {
                    challengeIndicS = " Indicator: " + plainIndicator[split] + " " + encipheredDoubledMessageKeys[split].JavaSubstring(0, 3);
                }
                else if (opFormat == 3)
                {
                    challengeIndicS = " Indicator: " + plainIndicator[split] + " " + encipheredDoubledMessageKeys[split].JavaSubstring(0, 3) + " " +
                            encipheredDoubledMessageKeys[split].JavaSubstring(3, 6);
                    challengeIndicS += "(=<Plain Indicator> + <Message Key Encrypted twice by plain indicator>)";
                }
                else if (opFormat == 2)
                {
                    challengeIndicS = " Indicator: " + encipheredDoubledMessageKeys[split].JavaSubstring(0, 3) + " " + encipheredDoubledMessageKeys[split].JavaSubstring(3, 6);
                    challengeIndicS += "(=<Message Key encrypted twice using daily Message Key>)";

                }

                solutionFileS += "Message Number: " + ("" + (split + 1)) + "";
                if (opFormat == 1)
                {
                    solutionFileS += "Message Key: " + messageKeys[split] + " Message Key is encrypted with plain indicator: " + plainIndicator[split] + " " + " (Encrypted Msg Key: " + encipheredDoubledMessageKeys[split].JavaSubstring(0, 3) + ") ";
                }
                else
                {
                    solutionFileS += "Message Key: " + messageKeys[split] + " Message Key is encrypted with plain indicator: " + plainIndicator[split] + " " + " (Encrypted Doubled Msg Key: " + encipheredDoubledMessageKeys[split].JavaSubstring(0, 6) + ") ";
                }

                solutionFileS += "Trigram Score: " + messageTri[split] + " IC Score: " + messageIc[split] + "";
                solutionFileS += messagePlainTextInLines[split] + "";

                challengeFileS += "Message Number: " + ("" + (split + 1)) + " of " + opSplit + " Length: " + messageLength[split];
                challengeFileS += challengeIndicS;
                challengeFileS += "" + messageCipherInGroups[split];

                challengeFileS += "";

                if ((opCrib > 0) && (split == 0))
                {
                    challengeFileS += "Crib provided for this message (message 1): " + cribS + " at position: 0";
                    solutionFileS += "Crib: " + cribS + " at position: 0";
                }


                if (split == 0)
                {
                    EnigmaUtils.saveToFile(cipherFilePath, messageCipherInGroups[split]);
                    EnigmaUtils.saveToFile(plaintextFilePath, messagePlainTextInLines[split].Replace("", ""));
                    EnigmaUtils.saveToFile(cipherFilePath, messageCipherInGroups[split]);
                }

                string cipherSplitFilePath = SCENARIO_PATH + "\\" + "S" + id + "cipher" + ("" + (split + 1)) + ".txt";
                EnigmaUtils.saveToFile(cipherSplitFilePath, messageCipherInGroups[split]);
            }

            solutionFileS += "Indicators (non encrypted)";

            if (opFormat != 1)
            {
                for (int i = 0; i < nMessageKeys; i++)
                {
                    if (opFormat == 3)
                    {
                        indicatorFileS += plainIndicator[i] + " ";
                    }
                    indicatorFileS += encipheredDoubledMessageKeys[i] + "     ";
                    if (((i % 10) == 9) || (i == (nMessageKeys - 1)))
                    {
                        indicatorFileS += "";
                    }

                    if (opFormat == 3)
                    {
                        solutionFileS += plainIndicator[i] + " ";
                    }
                    solutionFileS += messageKeys[i] + "    ";
                    if (((i % 10) == 9) || (i == (nMessageKeys - 1)))
                    {
                        solutionFileS += "";
                    }

                }

            }

            if (opFormat > 1)
            {
                Console.WriteLine("======= solution{0}====== end of solution=======challenge{1}======== end of challenge======= indicators{2}======= end of indicators",
                        solutionFileS, challengeFileS, indicatorFileS);
            }
            else
            {
                Console.WriteLine("======= solution{0}====== end of solution=======challenge{1}======== end of challenge.txt",
                        solutionFileS, challengeFileS);
            }

            EnigmaUtils.saveToFile(challengeFilePath, challengeFileS);
            if (opFormat > 1)
            {
                EnigmaUtils.saveToFile(indicatorsFilePath, indicatorFileS);
            }

            EnigmaUtils.saveToFile(solutionFilePath, solutionFileS + challengeFileS + indicatorFileS);

            Console.WriteLine("Saved {0}", cipherFilePath);
            Console.WriteLine("Saved {0}", plaintextFilePath);
            if (opFormat > 1)
            {
                Console.WriteLine("Saved {0}", indicatorsFilePath);
            }
            Console.WriteLine("Saved {0}", challengeFilePath);
            Console.WriteLine("Saved {0}", solutionFilePath);
            resultReporter.displayBestPlaintext("Saved " + cipherFilePath + ", " + plaintextFilePath + ", "
                    + ((opFormat > 1) ? indicatorsFilePath + ", " : "") + challengeFilePath + ", " + solutionFilePath + ", " + "Crib at pos 0: " + cribS);

            if (opFormat == 2)
            {
                short[] steppings = new short[Key.MAXLEN];
                dailyKey.showSteppings(steppings, 6);
                string steppingsS = EnigmaUtils.getCiphertextstringNoXJ(steppings, 6);
                Console.WriteLine("123456{0}", steppingsS);
                dailyKey.printKeystring("Daily Key: ");
            }
            else if (opFormat == 3)
            {
                dailyKey.printKeystring("Daily Key: ");
            }
        }
    }
}