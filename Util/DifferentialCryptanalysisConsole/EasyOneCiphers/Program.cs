/*
   Copyright 2019 Nils Kopal, nils.kopal<at>cryptool.org

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
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyCipher;
using EasyOneCiphers;
using Interfaces;

namespace EasyCipherOne
{
    public class Program
    {
        private const bool test4bit4 = true;            // 4 * 4 = 16bit cipher
        private const bool test4bit8 = true;            // 4 * 4 = 16bit cipher
        private const bool test8bit8 = true;            // 4 * 8 = 32bit cipher
        private const bool test4bit16 = true;           // 4 * 4 = 16bit cipher
        private const bool test8bit16 = true;           // 4 * 8 = 32bit cipher
        private const bool test16bit16 = true;          // 4 * 16 = 64bit cipher

        /// <summary>
        /// Performs tests on all of the ciphers
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            diffCrypt();


            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        static void diffCrypt()
        {
            Cipher16Bit16Encryption encryption = new Cipher16Bit16Encryption();
            Cipher16Bit16Analysis analysis = new Cipher16Bit16Analysis();

            List<SBoxCharacteristic> diffList = analysis.CountDifferentialsSingleSBox();

            bool[] attackSBox4 = new bool[] { true, false, false, false };
            bool[] attackSBox3 = new bool[] { false, true, false, false };
            bool[] attackSBox2 = new bool[] { false, false, true, false };
            bool[] attackSBox1 = new bool[] { false, false, false, true };

            bool[] attackSBox1AndSBox2 = new bool[] { false, false, true, true };

            bool[] atk = new bool[] { true, true, true, false };

            int testCount = 1000;

            //paircount for generell test
            //int[] pairCount = new[] { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000, 1100, 1200, 1300, 1400, 1500, 1600, 1700, 1800, 1900, 2000 };
            int[] pairCount = new[] { 200, 400, 600, 800, 1000, 1200, 1400, 1600, 1800, 2000, 2200, 2400, 2600, 2800, 3000, 3200, 3400, 3600, 3800, 4000 };

            for (int j = 0; j < pairCount.Length; j++)
            {
                int successCounter = 0;
                int failureCounter = 0;

                //string fileName = "EasyCipher_SuccessRate_IncrementedPairs.txt";
                string fileName = "EasyCipher_SuccessRate_RandomPairs.txt";

                //to change way of pair-generation, edit GenerateInputPairList() method in Analysis-Class
                for (int i = 0; i < testCount; i++)
                {
                    //Console.Clear();
                    analysis.paircount = pairCount[j];
                    encryption.GenerateRandomKeys();
                    bool success = true;

                    Cipher16Bit16DifferentialKeyRecoveryAttack keyRecoveryConfigurationTest = new Cipher16Bit16DifferentialKeyRecoveryAttack();
                    AbortingPolicy ap = AbortingPolicy.Threshold;
                    SearchPolicy sp = SearchPolicy.FirstAllCharacteristicsDepthSearch;

                    DifferentialAttackRoundConfiguration configRound3SBox1 = analysis.GenerateConfigurationAttack(3, attackSBox1, ap, sp, diffList, keyRecoveryConfigurationTest, encryption);
                    DifferentialAttackRoundConfiguration configRound3SBox2 = analysis.GenerateConfigurationAttack(3, attackSBox2, ap, sp, diffList, keyRecoveryConfigurationTest, encryption);

                    //DifferentialAttackRoundConfiguration configRound3SBox2AndSBox1 = analysis.GenerateConfigurationAttack(3, attackSBox1AndSBox2, ap, sp, diffList, keyRecoveryConfigurationTest, encryption);
                    
                    DifferentialAttackRoundConfiguration configRound3SBox3 = analysis.GenerateConfigurationAttack(3, attackSBox3, ap, sp, diffList, keyRecoveryConfigurationTest, encryption);
                    DifferentialAttackRoundConfiguration configRound3SBox4 = analysis.GenerateConfigurationAttack(3, attackSBox4, ap, sp, diffList, keyRecoveryConfigurationTest, encryption);

                    DifferentialAttackRoundResult resultRound3SBox1 = analysis.RecoverKeyInformation(keyRecoveryConfigurationTest, configRound3SBox1, encryption);
                    DifferentialAttackRoundResult resultRound3SBox2 = analysis.RecoverKeyInformation(keyRecoveryConfigurationTest, configRound3SBox2, encryption);

                    //DifferentialAttackRoundResult resultRound3SBox2And1 = analysis.RecoverKeyInformation(keyRecoveryConfigurationTest, configRound3SBox2AndSBox1, encryption);

                    DifferentialAttackRoundResult resultRound3SBox3 = analysis.RecoverKeyInformation(keyRecoveryConfigurationTest, configRound3SBox3, encryption);
                    DifferentialAttackRoundResult resultRound3SBox4 = analysis.RecoverKeyInformation(keyRecoveryConfigurationTest, configRound3SBox4, encryption);

                    keyRecoveryConfigurationTest.subkey4 = analysis.MergeBytes(resultRound3SBox1.PossibleKey, resultRound3SBox2.PossibleKey, resultRound3SBox3.PossibleKey, resultRound3SBox4.PossibleKey);
                    keyRecoveryConfigurationTest.recoveredSubkey3 = true;

                    DifferentialAttackRoundConfiguration configRound2SBox1 = analysis.GenerateConfigurationAttack(2, attackSBox1, ap, sp, diffList, keyRecoveryConfigurationTest, encryption);
                    DifferentialAttackRoundConfiguration configRound2SBox2 = analysis.GenerateConfigurationAttack(2, attackSBox2, ap, sp, diffList, keyRecoveryConfigurationTest, encryption);
                    DifferentialAttackRoundConfiguration configRound2SBox3 = analysis.GenerateConfigurationAttack(2, attackSBox3, ap, sp, diffList, keyRecoveryConfigurationTest, encryption);
                    DifferentialAttackRoundConfiguration configRound2SBox4 = analysis.GenerateConfigurationAttack(2, attackSBox4, ap, sp, diffList, keyRecoveryConfigurationTest, encryption);

                    DifferentialAttackRoundResult resultRound2SBox1 = analysis.RecoverKeyInformation(keyRecoveryConfigurationTest, configRound2SBox1, encryption);
                    DifferentialAttackRoundResult resultRound2SBox2 = analysis.RecoverKeyInformation(keyRecoveryConfigurationTest, configRound2SBox2, encryption);
                    DifferentialAttackRoundResult resultRound2SBox3 = analysis.RecoverKeyInformation(keyRecoveryConfigurationTest, configRound2SBox3, encryption);
                    DifferentialAttackRoundResult resultRound2SBox4 = analysis.RecoverKeyInformation(keyRecoveryConfigurationTest, configRound2SBox4, encryption);

                    //Result
                    keyRecoveryConfigurationTest.subkey3 = (resultRound2SBox4.PossibleKey ^ resultRound2SBox3.PossibleKey ^ resultRound2SBox2.PossibleKey ^ resultRound2SBox1.PossibleKey);
                    keyRecoveryConfigurationTest.recoveredSubkey2 = true;

                    try
                    {
                        DifferentialAttackLastRoundResult lastRoundResult = analysis.AttackFirstRound(keyRecoveryConfigurationTest, encryption);
                        keyRecoveryConfigurationTest.LastRoundResult = lastRoundResult;
                        keyRecoveryConfigurationTest.recoveredSubkey1 = true;
                        keyRecoveryConfigurationTest.subkey2 = lastRoundResult.SubKey1;
                        keyRecoveryConfigurationTest.recoveredSubkey0 = true;
                        keyRecoveryConfigurationTest.subkey1 = lastRoundResult.SubKey0;
                    }
                    catch (Exception e)
                    {
                        success = false;
                    }

                    if (success)
                    {
                        successCounter++;
                        Console.WriteLine("Success.");
                    }
                    else
                    {
                        failureCounter++;
                        Console.WriteLine("Key not recovered.");
                    }
                }

                using (StreamWriter sw = File.AppendText(fileName))
                {
                    sw.WriteLine("" + successCounter + ";" + failureCounter);
                }
            }
        }

        static void testEasyCiphers()
        {
            Console.WriteLine("generating keys");
            var roundkeys4 = KeySchedule.Create_4_4BitRoundKeys(1203);
            var roundkeys8 = KeySchedule.Create_4_8_BitRoundKeys(1984);
            var roundkeys16 = KeySchedule.Create_4_16_BitRoundKeys(12031984);
            Console.WriteLine("done");

            //4bit4 => 4bit roundkeys with 4bit block
            if (test4bit4)
            {
                Console.WriteLine("4bit4");
                //4bit4 test
                var ok = true;
                for (var plaintext4 = 0; plaintext4 < 16; plaintext4++)
                {
                    var ciphertext4 = EasyCipherOne.Encrypt4Bit4(plaintext4, roundkeys4);
                    var plaintext = EasyCipherOne.Decrypt4Bit4(ciphertext4, roundkeys4);
                    if (plaintext4 != plaintext)
                    {
                        ok = false;
                    }
                }
                Console.WriteLine("Passed: {0}", ok);
            }

            //4bit8 => 4bit roundkeys with 8bit block
            if (test4bit8)
            {
                Console.WriteLine("4bit8");
                //4bit8 test
                var ok = true;
                for (var plaintext8 = 0; plaintext8 < 256; plaintext8++)
                {
                    var ciphertext8 = EasyCipherOne.Encrypt4Bit8(plaintext8, roundkeys4);
                    var plaintext = EasyCipherOne.Decrypt4Bit8(ciphertext8, roundkeys4);
                    if (plaintext8 != plaintext)
                    {
                        ok = false;
                    }
                }
                Console.WriteLine("Passed: {0}", ok);
            }

            //4bit8 => 8bit roundkeys with 8bit block
            if (test8bit8)
            {
                Console.WriteLine("8bit8");
                //8bit4 test
                var ok = true;
                for (var plaintext8 = 0; plaintext8 < 256; plaintext8++)
                {
                    var ciphertext4 = EasyCipherOne.Encrypt8Bit8(plaintext8, roundkeys8);
                    var plaintext = EasyCipherOne.Decrypt8Bit8(ciphertext4, roundkeys8);
                    if (plaintext8 != plaintext)
                    {
                        ok = false;
                    }
                }
                Console.WriteLine("Passed: {0}", ok);
            }

            //4bit16 => 4bit roundkeys with 16bit block
            if (test4bit16)
            {
                Console.WriteLine("4bit16");
                //8bit4 test‭
                var ok = true;
                for (int plaintext16 = 0; plaintext16 < 65535; plaintext16++)
                {
                    var ciphertext16 = EasyCipherOne.Encrypt4Bit16(plaintext16, roundkeys4);
                    var plaintext = EasyCipherOne.Decrypt4Bit16(ciphertext16, roundkeys4);
                    if (plaintext16 != plaintext)
                    {
                        ok = false;
                    }
                }
                Console.WriteLine("Passed: {0}", ok);
            }

            //8bit16 => 8bit roundkeys with 16bit block
            if (test8bit16)
            {
                Console.WriteLine("8bit16");
                //8bit4 test‭
                var ok = true;
                for (int plaintext16 = 0; plaintext16 < 65535; plaintext16++)
                {
                    var ciphertext16 = EasyCipherOne.Encrypt8bit16(plaintext16, roundkeys8);
                    var plaintext = EasyCipherOne.Decrypt8bit16(ciphertext16, roundkeys8);
                    if (plaintext16 != plaintext)
                    {
                        ok = false;
                    }
                }
                Console.WriteLine("Passed: {0}", ok);
            }

            //8bit16 => 8bit roundkeys with 16bit block
            if (test16bit16)
            {
                Console.WriteLine("16bit16");
                //8bit4 test‭
                var ok = true;
                for (int plaintext16 = 0; plaintext16 < 65535; plaintext16++)
                {
                    var ciphertext16 = EasyCipherOne.Encrypt16bit16(plaintext16, roundkeys16);
                    var plaintext = EasyCipherOne.Decrypt16bit16(ciphertext16, roundkeys16);
                    if (plaintext16 != plaintext)
                    {
                        ok = false;
                    }
                }
                Console.WriteLine("Passed: {0}", ok);
            }
        }
    }
}
