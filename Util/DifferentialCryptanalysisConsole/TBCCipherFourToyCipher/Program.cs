using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Interfaces;
using Newtonsoft.Json;

namespace TBCCipherFourToyCipher
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("The Block Cipher Companion CipherFour");

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        static void CountTestedKeys()
        {
            Encryption encryption = new Encryption();
            Analysis analysis = new Analysis();

            //analyse the sbox
            List<SBoxCharacteristic> diffList = analysis.CountDifferentialsSingleSBox();

            int testCount = 100;

            int testedKeys = 0;
            int pairs = 10000;

            string fileName = "Cipher3_Full_TestedKeys.txt";

            //to change way of pair-generation, edit GenerateInputPairList() method in Analysis-Class
            for (int i = 0; i < testCount; i++)
            {
                encryption.GenerateRandomKeys();
                bool success = true;

                CipherFourDifferentialKeyRecoveryAttack keyRecoveryConfiguration = new CipherFourDifferentialKeyRecoveryAttack();

                //attack round 5 sbox4
                DifferentialAttackRoundConfiguration configRound5SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox4.json");
                configRound5SBox4 = analysis.RefreshPairLists(configRound5SBox4, diffList, keyRecoveryConfiguration, encryption, pairs);
                DifferentialAttackRoundResult resultRound5SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound5SBox4, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound5SBox4);
                keyRecoveryConfiguration.RoundResults.Add(resultRound5SBox4);
                testedKeys += 16;

                //attack round 5 sbox3
                DifferentialAttackRoundConfiguration configRound5SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox3.json");
                configRound5SBox3 = analysis.RefreshPairLists(configRound5SBox3, diffList, keyRecoveryConfiguration, encryption, pairs);
                DifferentialAttackRoundResult resultRound5SBox3 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound5SBox3, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound5SBox3);
                keyRecoveryConfiguration.RoundResults.Add(resultRound5SBox3);
                testedKeys += 16;

                //attack round 5 sbox1
                DifferentialAttackRoundConfiguration configRound5SBox1 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1SBox4.json");
                configRound5SBox1 = analysis.RefreshPairLists(configRound5SBox1, diffList, keyRecoveryConfiguration, encryption, pairs);
                DifferentialAttackRoundResult resultRound5SBox1 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound5SBox1, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound5SBox1);
                keyRecoveryConfiguration.RoundResults.Add(resultRound5SBox1);
                testedKeys += 256;

                //attack round 5 sbox 2
                DifferentialAttackRoundConfiguration configRound5SBox1SBox2SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1SBox2SBox3.json");
                configRound5SBox1SBox2SBox3 = analysis.RefreshPairLists(configRound5SBox1SBox2SBox3, diffList, keyRecoveryConfiguration, encryption, pairs);
                DifferentialAttackRoundResult resultRound5SBox2 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound5SBox1SBox2SBox3, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound5SBox1SBox2SBox3);
                keyRecoveryConfiguration.RoundResults.Add(resultRound5SBox2);
                testedKeys += 4096;


                //Result
                keyRecoveryConfiguration.subkey6 = analysis.MergeBytes(resultRound5SBox1.PossibleKey, resultRound5SBox2.PossibleKey, resultRound5SBox3.PossibleKey, resultRound5SBox4.PossibleKey);
                keyRecoveryConfiguration.recoveredSubkey6 = true;


                //attack round 4 sbox4
                DifferentialAttackRoundConfiguration configRound4SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound4SBox4.json");
                configRound4SBox4 = analysis.RefreshPairLists(configRound4SBox4, diffList, keyRecoveryConfiguration, encryption, pairs);
                DifferentialAttackRoundResult resultRound4SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound4SBox4, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound4SBox4);
                keyRecoveryConfiguration.RoundResults.Add(resultRound4SBox4);
                testedKeys += 16;

                //attack round 4 sbox3
                DifferentialAttackRoundConfiguration configRound4SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound4SBox3.json");
                configRound4SBox3 = analysis.RefreshPairLists(configRound4SBox3, diffList, keyRecoveryConfiguration, encryption, pairs);
                DifferentialAttackRoundResult resultRound4SBox3 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound4SBox3, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound4SBox3);
                keyRecoveryConfiguration.RoundResults.Add(resultRound4SBox3);
                testedKeys += 16;

                //attack round 4 sbox1
                DifferentialAttackRoundConfiguration configRound4SBox1 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound4SBox1SBox4.json");
                configRound4SBox1 = analysis.RefreshPairLists(configRound4SBox1, diffList, keyRecoveryConfiguration, encryption, pairs);
                DifferentialAttackRoundResult resultRound4SBox1 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound4SBox1, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound4SBox1);
                keyRecoveryConfiguration.RoundResults.Add(resultRound4SBox1);
                testedKeys += 256;

                //attack round 4 sbox 2
                DifferentialAttackRoundConfiguration configRound4SBox1SBox2SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound4SBox1SBox2SBox3.json");
                configRound4SBox1SBox2SBox3 = analysis.RefreshPairLists(configRound4SBox1SBox2SBox3, diffList, keyRecoveryConfiguration, encryption, pairs);
                DifferentialAttackRoundResult resultRound4SBox2 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound4SBox1SBox2SBox3, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound4SBox1SBox2SBox3);
                keyRecoveryConfiguration.RoundResults.Add(resultRound4SBox2);
                testedKeys += 4096;

                BitArray bitsSBox1 = new BitArray(BitConverter.GetBytes(resultRound4SBox1.PossibleKey));
                BitArray bitsSBox2 = new BitArray(BitConverter.GetBytes(resultRound4SBox2.PossibleKey));
                BitArray bitsOfKey = new BitArray(BitConverter.GetBytes(resultRound4SBox4.PossibleKey ^ resultRound4SBox3.PossibleKey));

                bitsOfKey[15] = bitsSBox1[15];
                bitsOfKey[11] = bitsSBox1[11];
                bitsOfKey[7] = bitsSBox1[7];
                bitsOfKey[3] = bitsSBox1[3];

                bitsOfKey[14] = bitsSBox2[14];
                bitsOfKey[10] = bitsSBox2[10];
                bitsOfKey[6] = bitsSBox2[6];
                bitsOfKey[2] = bitsSBox2[2];

                byte[] bytes = new byte[4];
                bitsOfKey.CopyTo(bytes, 0);

                int outputBlock = BitConverter.ToInt32(bytes, 0);

                //Result
                keyRecoveryConfiguration.subkey5 = (outputBlock);
                keyRecoveryConfiguration.recoveredSubkey5 = true;


                //attack round 3 sbox4
                DifferentialAttackRoundConfiguration configRound3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox4.json");
                configRound3SBox4 = analysis.RefreshPairLists(configRound3SBox4, diffList, keyRecoveryConfiguration, encryption, pairs);
                DifferentialAttackRoundResult resultRound3SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound3SBox4, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound3SBox4);
                keyRecoveryConfiguration.RoundResults.Add(resultRound3SBox4);
                testedKeys += 16;

                //attack round 3 sbox3
                DifferentialAttackRoundConfiguration configRound3SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox3.json");
                configRound3SBox3 = analysis.RefreshPairLists(configRound3SBox3, diffList, keyRecoveryConfiguration, encryption, pairs);
                DifferentialAttackRoundResult resultRound3SBox3 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound3SBox3, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound3SBox3);
                keyRecoveryConfiguration.RoundResults.Add(resultRound3SBox3);
                testedKeys += 16;

                //attack round 3 sbox1
                DifferentialAttackRoundConfiguration configRound3SBox1 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox1SBox4.json");
                configRound3SBox1 = analysis.RefreshPairLists(configRound3SBox1, diffList, keyRecoveryConfiguration, encryption, pairs);
                DifferentialAttackRoundResult resultRound3SBox1 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound3SBox1, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound3SBox1);
                keyRecoveryConfiguration.RoundResults.Add(resultRound3SBox1);
                testedKeys += 256;

                //attack round 3 sbox 2
                DifferentialAttackRoundConfiguration configRound3SBox1SBox2SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox1SBox2SBox3.json");
                configRound3SBox1SBox2SBox3 = analysis.RefreshPairLists(configRound3SBox1SBox2SBox3, diffList, keyRecoveryConfiguration, encryption, pairs);
                DifferentialAttackRoundResult resultRound3SBox2 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound3SBox1SBox2SBox3, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound3SBox1SBox2SBox3);
                keyRecoveryConfiguration.RoundResults.Add(resultRound3SBox2);
                testedKeys += 4096;

                bitsSBox1 = new BitArray(BitConverter.GetBytes(resultRound3SBox1.PossibleKey));
                bitsSBox2 = new BitArray(BitConverter.GetBytes(resultRound3SBox2.PossibleKey));
                bitsOfKey = new BitArray(BitConverter.GetBytes(resultRound3SBox4.PossibleKey ^ resultRound3SBox3.PossibleKey));

                bitsOfKey[15] = bitsSBox1[15];
                bitsOfKey[11] = bitsSBox1[11];
                bitsOfKey[7] = bitsSBox1[7];
                bitsOfKey[3] = bitsSBox1[3];

                bitsOfKey[14] = bitsSBox2[14];
                bitsOfKey[10] = bitsSBox2[10];
                bitsOfKey[6] = bitsSBox2[6];
                bitsOfKey[2] = bitsSBox2[2];

                bytes = new byte[4];
                bitsOfKey.CopyTo(bytes, 0);

                outputBlock = BitConverter.ToInt32(bytes, 0);

                //Result
                keyRecoveryConfiguration.subkey4 = (outputBlock);
                keyRecoveryConfiguration.recoveredSubkey4 = true;



                //attack round 2 sbox4
                DifferentialAttackRoundConfiguration configRound2SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound2SBox4.json");
                configRound2SBox4 = analysis.RefreshPairLists(configRound2SBox4, diffList, keyRecoveryConfiguration, encryption, pairs);
                DifferentialAttackRoundResult resultRound2SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound2SBox4, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound2SBox4);
                keyRecoveryConfiguration.RoundResults.Add(resultRound2SBox4);
                testedKeys += 16;

                //attack round 2 sbox3
                DifferentialAttackRoundConfiguration configRound2SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound2SBox3.json");
                configRound2SBox3 = analysis.RefreshPairLists(configRound2SBox3, diffList, keyRecoveryConfiguration, encryption, pairs);
                DifferentialAttackRoundResult resultRound2SBox3 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound2SBox3, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound2SBox3);
                keyRecoveryConfiguration.RoundResults.Add(resultRound2SBox3);
                testedKeys += 16;

                //attack round 2 sbox1
                DifferentialAttackRoundConfiguration configRound2SBox1 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound2SBox1SBox4.json");
                configRound2SBox1 = analysis.RefreshPairLists(configRound2SBox1, diffList, keyRecoveryConfiguration, encryption, pairs);
                DifferentialAttackRoundResult resultRound2SBox1 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound2SBox1, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound2SBox1);
                keyRecoveryConfiguration.RoundResults.Add(resultRound2SBox1);
                testedKeys += 256;

                //attack round 2 sbox 2
                DifferentialAttackRoundConfiguration configRound2SBox1SBox2SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound2SBox1SBox2SBox3.json");
                configRound2SBox1SBox2SBox3 = analysis.RefreshPairLists(configRound2SBox1SBox2SBox3, diffList, keyRecoveryConfiguration, encryption, pairs);
                DifferentialAttackRoundResult resultRound2SBox2 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound2SBox1SBox2SBox3, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound2SBox1SBox2SBox3);
                keyRecoveryConfiguration.RoundResults.Add(resultRound2SBox2);
                testedKeys += 4096;

                bitsSBox1 = new BitArray(BitConverter.GetBytes(resultRound2SBox1.PossibleKey));
                bitsSBox2 = new BitArray(BitConverter.GetBytes(resultRound2SBox2.PossibleKey));
                bitsOfKey = new BitArray(BitConverter.GetBytes(resultRound2SBox4.PossibleKey ^ resultRound2SBox3.PossibleKey));

                bitsOfKey[15] = bitsSBox1[15];
                bitsOfKey[11] = bitsSBox1[11];
                bitsOfKey[7] = bitsSBox1[7];
                bitsOfKey[3] = bitsSBox1[3];

                bitsOfKey[14] = bitsSBox2[14];
                bitsOfKey[10] = bitsSBox2[10];
                bitsOfKey[6] = bitsSBox2[6];
                bitsOfKey[2] = bitsSBox2[2];

                bytes = new byte[4];
                bitsOfKey.CopyTo(bytes, 0);

                outputBlock = BitConverter.ToInt32(bytes, 0);

                //Result
                keyRecoveryConfiguration.subkey3 = (outputBlock);
                keyRecoveryConfiguration.recoveredSubkey3 = true;


                try
                {
                    DifferentialAttackLastRoundResult lastRoundResult = analysis.AttackFirstRound(keyRecoveryConfiguration, encryption);
                    keyRecoveryConfiguration.LastRoundResult = lastRoundResult;
                    keyRecoveryConfiguration.recoveredSubkey2 = true;
                    keyRecoveryConfiguration.subkey2 = lastRoundResult.SubKey1;
                    keyRecoveryConfiguration.recoveredSubkey1 = true;
                    keyRecoveryConfiguration.subkey1 = lastRoundResult.SubKey0;
                    testedKeys += lastRoundResult.testedKeys;
                }
                catch (Exception e)
                {
                    success = false;
                }

                if (success)
                {
                    Console.WriteLine("Success.");
                }
                else
                {
                    Console.WriteLine("Key not recovered.");
                }
            }

            using (StreamWriter sw = File.AppendText(fileName))
            {
                sw.WriteLine("" + (testedKeys / testCount));
            }

            Console.WriteLine((testedKeys / testCount));
        }

        static void Cipher1MessageCount()
        {
            Encryption encryption = new Encryption();
            Analysis analysis = new Analysis();

            //analyse the sbox
            List<SBoxCharacteristic> diffList = analysis.CountDifferentialsSingleSBox();

            int testCount = 100;

            //counter to trace the success rate
            int successCounter = 0;
            int failureCounter = 0;

            //do the experiment
            for (int i = 0; i < testCount; i++)
            {
                bool success = true;

                //get new keys for encryption
                encryption.GenerateRandomKeys();

                CipherFourDifferentialKeyRecoveryAttack c1attack = new CipherFourDifferentialKeyRecoveryAttack();

                //set the keys 3-6 as recovered to attack last round
                c1attack.recoveredSubkey6 = true;
                c1attack.subkey6 = encryption._keys[5];

                c1attack.recoveredSubkey5 = true;
                c1attack.subkey5 = encryption._keys[4];

                c1attack.recoveredSubkey4 = true;
                c1attack.subkey4 = encryption._keys[3];

                c1attack.recoveredSubkey3 = true;
                c1attack.subkey3 = encryption._keys[2];

                //attack last round
                try
                {
                    DifferentialAttackLastRoundResult lastRoundResult = analysis.AttackFirstRound(c1attack, encryption);
                    c1attack.LastRoundResult = lastRoundResult;

                    //save the keys
                    c1attack.recoveredSubkey2 = true;
                    c1attack.subkey2 = lastRoundResult.SubKey1;
                    c1attack.recoveredSubkey1 = true;
                    c1attack.subkey1 = lastRoundResult.SubKey0;
                }
                catch (Exception e)
                {
                    success = false;
                }

                if (success)
                {
                    successCounter++;
                    Console.WriteLine("Success");
                }
                else
                {
                    failureCounter++;
                    Console.WriteLine("Failure");
                }
            }
        }

        static void PrintDifferentials()
        {
            string fileName = "Cipher3_Differentials_R5.txt";

            //round 5 sbox4: 0001
            DifferentialAttackRoundConfiguration configRound5SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox4.json");

            using (StreamWriter sw = File.AppendText(fileName))
            {
                sw.WriteLine(configRound5SBox4.Probability);
            }

            //round 5 sbox3 0010
            DifferentialAttackRoundConfiguration configRound5SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox3.json");

            using (StreamWriter sw = File.AppendText(fileName))
            {
                sw.WriteLine(configRound5SBox3.Probability);
            }

            //round 5 sbox3 and sbox4 0011
            DifferentialAttackRoundConfiguration configRound5SBox3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox3SBox4.json");

            using (StreamWriter sw = File.AppendText(fileName))
            {
                sw.WriteLine(configRound5SBox3SBox4.Probability);
            }

            //round 5 sbox2 0100
            DifferentialAttackRoundConfiguration configRound5SBox2 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox2.json");

            using (StreamWriter sw = File.AppendText(fileName))
            {
                sw.WriteLine(configRound5SBox2.Probability);
            }

            //round 5 sbox2 and sbox4 0101
            DifferentialAttackRoundConfiguration configRound5SBox2SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox2SBox4.json");

            using (StreamWriter sw = File.AppendText(fileName))
            {
                sw.WriteLine(configRound5SBox2SBox4.Probability);
            }

            //round 5 sbox2 and sbox3 0110
            DifferentialAttackRoundConfiguration configRound5SBox2SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox2SBox3.json");

            using (StreamWriter sw = File.AppendText(fileName))
            {
                sw.WriteLine(configRound5SBox2SBox3.Probability);
            }

            //round 5 sbox2 and sbox3 and sbox4 0111
            DifferentialAttackRoundConfiguration configRound5SBox2SBox3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox2SBox3SBox4.json");

            using (StreamWriter sw = File.AppendText(fileName))
            {
                sw.WriteLine(configRound5SBox2SBox3SBox4.Probability);
            }

            //round 5 sbox1 1000
            DifferentialAttackRoundConfiguration configRound5SBox1 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1.json");

            using (StreamWriter sw = File.AppendText(fileName))
            {
                sw.WriteLine(configRound5SBox1.Probability);
            }

            //round 5 sbox1 and sbox4 1001
            DifferentialAttackRoundConfiguration configRound5SBox1SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1SBox4.json");

            using (StreamWriter sw = File.AppendText(fileName))
            {
                sw.WriteLine(configRound5SBox1SBox4.Probability);
            }
            
            //round 5 sbox1 and sbox3 1010
            DifferentialAttackRoundConfiguration configRound5SBox1SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1SBox3.json");

            using (StreamWriter sw = File.AppendText(fileName))
            {
                sw.WriteLine(configRound5SBox1SBox3.Probability);
            }

            //round 5 sbox1 and sbox3 and sbox4 1011
            DifferentialAttackRoundConfiguration configRound5SBox1SBox3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1SBox3SBox4.json");

            using (StreamWriter sw = File.AppendText(fileName))
            {
                sw.WriteLine(configRound5SBox1SBox3SBox4.Probability);
            }
            
            //round 5 sbox1 and sbox2 1100
            DifferentialAttackRoundConfiguration configRound5SBox1SBox2 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1SBox2.json");

            using (StreamWriter sw = File.AppendText(fileName))
            {
                sw.WriteLine(configRound5SBox1SBox2.Probability);
            }
            
            //round 5 sbox1 and sbox2 and sbox4 1101
            DifferentialAttackRoundConfiguration configRound5SBox1SBox2SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1SBox2SBox4.json");

            using (StreamWriter sw = File.AppendText(fileName))
            {
                sw.WriteLine(configRound5SBox1SBox2SBox4.Probability);
            }

            //round 5 sbox1 and sbox2 and sbox3 1110
            DifferentialAttackRoundConfiguration configRound5SBox1SBox2SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1SBox2SBox3.json");

            using (StreamWriter sw = File.AppendText(fileName))
            {
                sw.WriteLine(configRound5SBox1SBox2SBox3.Probability);
            }

            //round 5 sbox1 sbox2 and sbox3 and sbox4 1111
            DifferentialAttackRoundConfiguration configRound5SBox1SBox2SBox3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1SBox2SBox3SBox4.json");

            using (StreamWriter sw = File.AppendText(fileName))
            {
                sw.WriteLine(configRound5SBox1SBox2SBox3SBox4.Probability);
            }

        }

        static void BenchmarkTimeKeyRecoveryDifferentCountOfSubKeys()
        {
            Encryption encryption = new Encryption();
            Analysis analysis = new Analysis();

            Stopwatch stopWatch = new Stopwatch();

            //analyse the sbox
            List<SBoxCharacteristic> diffList = analysis.CountDifferentialsSingleSBox();

            int testCount = 1;
            int[] pairCount = new[] { 5000, 10000, 15000, 20000 };

            for (int j = 0; j < pairCount.Length; j++)
            {
                double[] times = new double[16];

                for (int i = 0; i < testCount; i++)
                {
                    encryption.GenerateRandomKeys();

                    CipherFourDifferentialKeyRecoveryAttack keyRecoveryConfiguration = new CipherFourDifferentialKeyRecoveryAttack();

                    //test filter round 5 sbox4: 0001
                    DifferentialAttackRoundConfiguration configRound5SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox4.json");
                    configRound5SBox4 = analysis.RefreshPairLists(configRound5SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configRound5SBox4.FilteredPairList = analysis.FilterPairs(configRound5SBox4, diffList, keyRecoveryConfiguration, encryption, configRound5SBox4.ExpectedDifference);
                    DifferentialAttackRoundResult resultRound5SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound5SBox4, encryption);
                    stopWatch.Stop();
                    times[1] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();

                    //test filter round 5 sbox3 0010
                    DifferentialAttackRoundConfiguration configRound5SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox3.json");
                    configRound5SBox3 = analysis.RefreshPairLists(configRound5SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configRound5SBox3.FilteredPairList = analysis.FilterPairs(configRound5SBox3, diffList, keyRecoveryConfiguration, encryption, configRound5SBox3.ExpectedDifference);
                    DifferentialAttackRoundResult resultRound5SBox3 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound5SBox3, encryption);
                    stopWatch.Stop();
                    times[2] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();

                    //test filter round 5 sbox2 0100
                    DifferentialAttackRoundConfiguration configRound5SBox2 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox2.json");
                    configRound5SBox2 = analysis.RefreshPairLists(configRound5SBox2, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configRound5SBox2.FilteredPairList = analysis.FilterPairs(configRound5SBox2, diffList, keyRecoveryConfiguration, encryption, configRound5SBox2.ExpectedDifference);
                    DifferentialAttackRoundResult resultRound5SBox2 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound5SBox2, encryption);
                    stopWatch.Stop();
                    times[4] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();

                    //test filter round 5 sbox1 1000
                    DifferentialAttackRoundConfiguration configRound5SBox1 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1.json");
                    configRound5SBox1 = analysis.RefreshPairLists(configRound5SBox1, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configRound5SBox1.FilteredPairList = analysis.FilterPairs(configRound5SBox1, diffList, keyRecoveryConfiguration, encryption, configRound5SBox1.ExpectedDifference);
                    DifferentialAttackRoundResult resultRound5SBox1 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound5SBox1, encryption);
                    stopWatch.Stop();
                    times[8] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();


                    //test filter round 5 sbox3 and sbox4 0011
                    DifferentialAttackRoundConfiguration configRound5SBox3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox3SBox4.json");
                    configRound5SBox3SBox4 = analysis.RefreshPairLists(configRound5SBox3SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configRound5SBox3SBox4.FilteredPairList = analysis.FilterPairs(configRound5SBox3SBox4, diffList, keyRecoveryConfiguration, encryption, configRound5SBox3SBox4.ExpectedDifference);
                    DifferentialAttackRoundResult resultRound5SBox3SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound5SBox3SBox4, encryption);
                    stopWatch.Stop();
                    times[3] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();

                    //test filter round 5 sbox2 and sbox4 0101
                    DifferentialAttackRoundConfiguration configRound5SBox2SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox2SBox4.json");
                    configRound5SBox2SBox4 = analysis.RefreshPairLists(configRound5SBox2SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configRound5SBox2SBox4.FilteredPairList = analysis.FilterPairs(configRound5SBox2SBox4, diffList, keyRecoveryConfiguration, encryption, configRound5SBox2SBox4.ExpectedDifference);
                    DifferentialAttackRoundResult resultRound5SBox2SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound5SBox2SBox4, encryption);
                    stopWatch.Stop();
                    times[5] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();


                    //test filter round 5 sbox1 and sbox4 1001
                    DifferentialAttackRoundConfiguration configRound5SBox1SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1SBox4.json");
                    configRound5SBox1SBox4 = analysis.RefreshPairLists(configRound5SBox1SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configRound5SBox1SBox4.FilteredPairList = analysis.FilterPairs(configRound5SBox1SBox4, diffList, keyRecoveryConfiguration, encryption, configRound5SBox1SBox4.ExpectedDifference);
                    DifferentialAttackRoundResult resultRound5SBox1SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound5SBox1SBox4, encryption);
                    stopWatch.Stop();
                    times[9] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();


                    //test filter round 5 sbox2 and sbox3 0110
                    DifferentialAttackRoundConfiguration configRound5SBox2SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox2SBox3.json");
                    configRound5SBox2SBox3 = analysis.RefreshPairLists(configRound5SBox2SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configRound5SBox2SBox3.FilteredPairList = analysis.FilterPairs(configRound5SBox2SBox3, diffList, keyRecoveryConfiguration, encryption, configRound5SBox2SBox3.ExpectedDifference);
                    DifferentialAttackRoundResult resultRound5SBox2SBox3 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound5SBox2SBox3, encryption);
                    stopWatch.Stop();
                    times[6] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();


                    //test filter round 5 sbox1 and sbox3 1010
                    DifferentialAttackRoundConfiguration configRound5SBox1SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1SBox3.json");
                    configRound5SBox1SBox3 = analysis.RefreshPairLists(configRound5SBox1SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configRound5SBox1SBox3.FilteredPairList = analysis.FilterPairs(configRound5SBox1SBox3, diffList, keyRecoveryConfiguration, encryption, configRound5SBox1SBox3.ExpectedDifference);
                    DifferentialAttackRoundResult resultRound5SBox1SBox3 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound5SBox1SBox3, encryption);
                    stopWatch.Stop();
                    times[10] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();


                    //test filter round 5 sbox1 and sbox2 1100
                    DifferentialAttackRoundConfiguration configRound5SBox1SBox2 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1SBox2.json");
                    configRound5SBox1SBox2 = analysis.RefreshPairLists(configRound5SBox1SBox2, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configRound5SBox1SBox2.FilteredPairList = analysis.FilterPairs(configRound5SBox1SBox2, diffList, keyRecoveryConfiguration, encryption, configRound5SBox1SBox2.ExpectedDifference);
                    DifferentialAttackRoundResult resultRound5SBox1SBox2 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound5SBox1SBox2, encryption);
                    stopWatch.Stop();
                    times[12] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();


                    //test filter round 5 sbox2 and sbox3 and sbox4 0111
                    DifferentialAttackRoundConfiguration configRound5SBox2SBox3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox2SBox3SBox4.json");
                    configRound5SBox2SBox3SBox4 = analysis.RefreshPairLists(configRound5SBox2SBox3SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configRound5SBox2SBox3SBox4.FilteredPairList = analysis.FilterPairs(configRound5SBox2SBox3SBox4, diffList, keyRecoveryConfiguration, encryption, configRound5SBox2SBox3SBox4.ExpectedDifference);
                    DifferentialAttackRoundResult resultRound5SBox2SBox3SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound5SBox2SBox3SBox4, encryption);
                    stopWatch.Stop();
                    times[7] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();


                    //test filter round 5 sbox1 and sbox2 and sbox4 1101
                    DifferentialAttackRoundConfiguration configRound5SBox1SBox2SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1SBox2SBox4.json");
                    configRound5SBox1SBox2SBox4 = analysis.RefreshPairLists(configRound5SBox1SBox2SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configRound5SBox1SBox2SBox4.FilteredPairList = analysis.FilterPairs(configRound5SBox1SBox2SBox4, diffList, keyRecoveryConfiguration, encryption, configRound5SBox1SBox2SBox4.ExpectedDifference);
                    DifferentialAttackRoundResult resultRound5SBox1SBox2SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound5SBox1SBox2SBox4, encryption);
                    stopWatch.Stop();
                    times[13] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();


                    //test filter round 5 sbox1 and sbox3 and sbox4 1011
                    DifferentialAttackRoundConfiguration configRound5SBox1SBox3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1SBox3SBox4.json");
                    configRound5SBox1SBox3SBox4 = analysis.RefreshPairLists(configRound5SBox1SBox3SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configRound5SBox1SBox3SBox4.FilteredPairList = analysis.FilterPairs(configRound5SBox1SBox3SBox4, diffList, keyRecoveryConfiguration, encryption, configRound5SBox1SBox3SBox4.ExpectedDifference);
                    DifferentialAttackRoundResult resultRound5SBox1SBox3SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound5SBox1SBox3SBox4, encryption);
                    stopWatch.Stop();
                    times[11] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();


                    //test filter round 5 sbox1 and sbox2 and sbox3 1110
                    DifferentialAttackRoundConfiguration configRound5SBox1SBox2SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1SBox2SBox3.json");
                    configRound5SBox1SBox2SBox3 = analysis.RefreshPairLists(configRound5SBox1SBox2SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configRound5SBox1SBox2SBox3.FilteredPairList = analysis.FilterPairs(configRound5SBox1SBox2SBox3, diffList, keyRecoveryConfiguration, encryption, configRound5SBox1SBox2SBox3.ExpectedDifference);
                    DifferentialAttackRoundResult resultRound5SBox1SBox2SBox3 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound5SBox1SBox2SBox3, encryption);
                    stopWatch.Stop();
                    times[14] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();


                    //test filter round 5 sbox1 sbox2 and sbox3 and sbox4 1111
                    DifferentialAttackRoundConfiguration configRound5SBox1SBox2SBox3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1SBox2SBox3SBox4.json");
                    configRound5SBox1SBox2SBox3SBox4 = analysis.RefreshPairLists(configRound5SBox1SBox2SBox3SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configRound5SBox1SBox2SBox3SBox4.FilteredPairList = analysis.FilterPairs(configRound5SBox1SBox2SBox3SBox4, diffList, keyRecoveryConfiguration, encryption, configRound5SBox1SBox2SBox3SBox4.ExpectedDifference);
                    DifferentialAttackRoundResult resultRound5SBox1SBox2SBox3SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound5SBox1SBox2SBox3SBox4, encryption);
                    stopWatch.Stop();
                    times[15] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();

                    Console.WriteLine("Iteration " + (i+1) + "/" + testCount + " finished ");
                }

                string fileName = "Cipher3_KeyRecoveryTime_DifferentCountOfKeybits_WithFilter_" + pairCount[j] + ".txt";

                for (int i = 1; i < times.Length; i++)
                {
                    Console.WriteLine("SBoxes: " + Convert.ToString(i, 2).PadLeft(4, '0') + " Average Time: " + (times[i] / testCount));
                    using (StreamWriter sw = File.AppendText(fileName))
                    {
                        //sw.WriteLine((times[i] / testCount));
                    }
                }
            }
        }

        static void TestFilter()
        {
            Encryption encryption = new Encryption();
            Analysis analysis = new Analysis();

            //analyse the sbox
            List<SBoxCharacteristic> diffList = analysis.CountDifferentialsSingleSBox();

            int testCount = 1000;
            int pairCount = 65536;
            int[] filteredPairs = new int[16];
            double[] probabilities = new double[16];

            string fileName = "Cipher3_Filter_R5_Differentials.txt";

            for (int i = 0; i < testCount; i++)
            {
                encryption.GenerateRandomKeys();

                CipherFourDifferentialKeyRecoveryAttack keyRecoveryConfiguration = new CipherFourDifferentialKeyRecoveryAttack();

                //test filter round 5 sbox4: 0001
                DifferentialAttackRoundConfiguration configRound5SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox4.json");
                configRound5SBox4 = analysis.RefreshPairLists(configRound5SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[1] += configRound5SBox4.FilteredPairList.Count;
                probabilities[1] = configRound5SBox4.Probability;

                /*
                Pair p = configRound5SBox4.UnfilteredPairList[0];
                Console.WriteLine("Left: " + p.LeftMember + " Right: " + p.RightMember);
                foreach (var pair in configRound5SBox4.UnfilteredPairList)
                {
                    if (pair.LeftMember == p.LeftMember || pair.LeftMember == p.RightMember)
                    {
                        Console.WriteLine("Left: " + pair.LeftMember + " Right: " + pair.RightMember);
                    }
                }
                */

                //test filter round 5 sbox3 0010
                DifferentialAttackRoundConfiguration configRound5SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox3.json");
                configRound5SBox3 = analysis.RefreshPairLists(configRound5SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[2] += configRound5SBox3.FilteredPairList.Count;
                probabilities[2] = configRound5SBox3.Probability;

                //test filter round 5 sbox2 0100
                DifferentialAttackRoundConfiguration configRound5SBox2 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox2.json");
                configRound5SBox2 = analysis.RefreshPairLists(configRound5SBox2, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[4] += configRound5SBox2.FilteredPairList.Count;
                probabilities[4] = configRound5SBox2.Probability;

                //test filter round 5 sbox1 1000
                DifferentialAttackRoundConfiguration configRound5SBox1 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1.json");
                configRound5SBox1 = analysis.RefreshPairLists(configRound5SBox1, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[8] += configRound5SBox1.FilteredPairList.Count;
                probabilities[8] = configRound5SBox1.Probability;

                //test filter round 5 sbox3 and sbox4 0011
                DifferentialAttackRoundConfiguration configRound5SBox3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox3SBox4.json");
                configRound5SBox3SBox4 = analysis.RefreshPairLists(configRound5SBox3SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[3] += configRound5SBox3SBox4.FilteredPairList.Count;
                probabilities[3] = configRound5SBox3SBox4.Probability;

                //test filter round 5 sbox2 and sbox4 0101
                DifferentialAttackRoundConfiguration configRound5SBox2SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox2SBox4.json");
                configRound5SBox2SBox4 = analysis.RefreshPairLists(configRound5SBox2SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[5] += configRound5SBox2SBox4.FilteredPairList.Count;
                probabilities[5] = configRound5SBox2SBox4.Probability;

                //test filter round 5 sbox1 and sbox4 1001
                DifferentialAttackRoundConfiguration configRound5SBox1SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1SBox4.json");
                configRound5SBox1SBox4 = analysis.RefreshPairLists(configRound5SBox1SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[9] += configRound5SBox1SBox4.FilteredPairList.Count;
                probabilities[9] = configRound5SBox1SBox4.Probability;

                //test filter round 5 sbox2 and sbox3 0110
                DifferentialAttackRoundConfiguration configRound5SBox2SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox2SBox3.json");
                configRound5SBox2SBox3 = analysis.RefreshPairLists(configRound5SBox2SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[6] += configRound5SBox2SBox3.FilteredPairList.Count;
                probabilities[6] = configRound5SBox2SBox3.Probability;

                //test filter round 5 sbox1 and sbox3 1010
                DifferentialAttackRoundConfiguration configRound5SBox1SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1SBox3.json");
                configRound5SBox1SBox3 = analysis.RefreshPairLists(configRound5SBox1SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[10] += configRound5SBox1SBox3.FilteredPairList.Count;
                probabilities[10] = configRound5SBox1SBox3.Probability;

                //test filter round 5 sbox1 and sbox2 1100
                DifferentialAttackRoundConfiguration configRound5SBox1SBox2 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1SBox2.json");
                configRound5SBox1SBox2 = analysis.RefreshPairLists(configRound5SBox1SBox2, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[12] += configRound5SBox1SBox2.FilteredPairList.Count;
                probabilities[12] = configRound5SBox1SBox2.Probability;


                //test filter round 5 sbox2 and sbox3 and sbox4 0111
                DifferentialAttackRoundConfiguration configRound5SBox2SBox3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox2SBox3SBox4.json");
                configRound5SBox2SBox3SBox4 = analysis.RefreshPairLists(configRound5SBox2SBox3SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[7] += configRound5SBox2SBox3SBox4.FilteredPairList.Count;
                probabilities[7] = configRound5SBox2SBox3SBox4.Probability;

                //test filter round 5 sbox1 and sbox2 and sbox4 1101
                DifferentialAttackRoundConfiguration configRound5SBox1SBox2SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1SBox2SBox4.json");
                configRound5SBox1SBox2SBox4 = analysis.RefreshPairLists(configRound5SBox1SBox2SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[13] += configRound5SBox1SBox2SBox4.FilteredPairList.Count;
                probabilities[13] = configRound5SBox1SBox2SBox4.Probability;

                //test filter round 5 sbox1 and sbox3 and sbox4 1011
                DifferentialAttackRoundConfiguration configRound5SBox1SBox3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1SBox3SBox4.json");
                configRound5SBox1SBox3SBox4 = analysis.RefreshPairLists(configRound5SBox1SBox3SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[11] += configRound5SBox1SBox3SBox4.FilteredPairList.Count;
                probabilities[11] = configRound5SBox1SBox3SBox4.Probability;

                //test filter round 5 sbox1 and sbox2 and sbox3 1110
                DifferentialAttackRoundConfiguration configRound5SBox1SBox2SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1SBox2SBox3.json");
                configRound5SBox1SBox2SBox3 = analysis.RefreshPairLists(configRound5SBox1SBox2SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[14] += configRound5SBox1SBox2SBox3.FilteredPairList.Count;
                probabilities[14] = configRound5SBox1SBox2SBox3.Probability;


                //test filter round 5 sbox1 sbox2 and sbox3 and sbox4 1111
                DifferentialAttackRoundConfiguration configRound5SBox1SBox2SBox3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1SBox2SBox3SBox4.json");
                configRound5SBox1SBox2SBox3SBox4 = analysis.RefreshPairLists(configRound5SBox1SBox2SBox3SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[15] += configRound5SBox1SBox2SBox3SBox4.FilteredPairList.Count;
                probabilities[15] = configRound5SBox1SBox2SBox3SBox4.Probability;
            }

            for (int i = 1; i < filteredPairs.Length; i++)
            {
                Console.WriteLine("SBoxes: " + Convert.ToString(i, 2).PadLeft(4, '0') + " Average survived Pairs: " + (filteredPairs[i] / testCount));
                using (StreamWriter sw = File.AppendText(fileName))
                {
                    //sw.WriteLine((filteredPairs[i] / testCount) + ";" + (probabilities[i] * 65536) + ";" + 65536);
                }
            }
        }

        static void BenchmarkTimeKeyRecoveryWithAndWithoutFilter()
        {
            Encryption encryption = new Encryption();
            Analysis analysis = new Analysis();

            Stopwatch stopWatch = new Stopwatch();

            //analyse the sbox
            List<SBoxCharacteristic> diffList = analysis.CountDifferentialsSingleSBox();

            int testCount = 10;
            int[] pairCount = new[] {5000, 10000, 15000, 20000, 25000, 30000, 35000, 40000, 45000, 50000, 55000, 60000, 65536};

            bool filter = true;

            for (int j = 0; j < pairCount.Length; j++)
            {
                int successCounter = 0;
                int failureCounter = 0;

                TimeSpan ts = new TimeSpan(0, 0, 0);

                //to enable/disable the filter, edit the RefreshPairLists() method in Analysis-Class

                string fileName = "Cipher3_KeyRecoveryTime_WithFilter.txt";
                //string fileName = "Cipher3_KeyRecoveryTime_NoFilter.txt";

                for (int i = 0; i < testCount; i++)
                {
                    encryption.GenerateRandomKeys();
                    bool success = true;


                    CipherFourDifferentialKeyRecoveryAttack keyRecoveryConfiguration = new CipherFourDifferentialKeyRecoveryAttack();

                    /*
                    //attack round 5 sbox4
                    DifferentialAttackRoundConfiguration configRound5SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox4.json");
                    configRound5SBox4 = analysis.RefreshPairLists(configRound5SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    //configRound5SBox4.FilteredPairList = analysis.FilterPairs(configRound5SBox4, diffList, keyRecoveryConfiguration, encryption, configRound5SBox4.ExpectedDifference);
                    DifferentialAttackRoundResult resultRound5SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound5SBox4, encryption);
                    stopWatch.Stop();
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound5SBox4);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound5SBox4);

                    //attack round 5 sbox3 sbox2 sbox1
                    DifferentialAttackRoundConfiguration configRound5SBox3Sbox2Sbox1 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1SBox2SBox3.json");
                    configRound5SBox3Sbox2Sbox1 = analysis.RefreshPairLists(configRound5SBox3Sbox2Sbox1, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    //configRound5SBox3Sbox2Sbox1.FilteredPairList = analysis.FilterPairs(configRound5SBox3Sbox2Sbox1, diffList, keyRecoveryConfiguration, encryption, configRound5SBox3Sbox2Sbox1.ExpectedDifference);
                    DifferentialAttackRoundResult resultRound5SBox3SBox2SBox1 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound5SBox3Sbox2Sbox1, encryption);
                    stopWatch.Stop();
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound5SBox3Sbox2Sbox1);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound5SBox3SBox2SBox1);

                    //Result
                    keyRecoveryConfiguration.subkey6 = analysis.MergeBytes(resultRound5SBox3SBox2SBox1.PossibleKey, resultRound5SBox3SBox2SBox1.PossibleKey, resultRound5SBox3SBox2SBox1.PossibleKey, resultRound5SBox4.PossibleKey);
                    keyRecoveryConfiguration.recoveredSubkey6 = true;

                    //attack round 4 sbox4
                    DifferentialAttackRoundConfiguration configRound4SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound4SBox4.json");
                    configRound4SBox4 = analysis.RefreshPairLists(configRound4SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    //configRound4SBox4.FilteredPairList = analysis.FilterPairs(configRound4SBox4, diffList, keyRecoveryConfiguration, encryption, configRound4SBox4.ExpectedDifference);
                    DifferentialAttackRoundResult resultRound4SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound4SBox4, encryption);
                    stopWatch.Stop();
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound4SBox4);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound4SBox4);

                    //attack round 4 sbox3 sbox2 sbox1
                    DifferentialAttackRoundConfiguration configRound4SBox3Sbox2Sbox1 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound4SBox1SBox2SBox3.json");
                    configRound4SBox3Sbox2Sbox1 = analysis.RefreshPairLists(configRound4SBox3Sbox2Sbox1, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    //configRound4SBox3Sbox2Sbox1.FilteredPairList = analysis.FilterPairs(configRound4SBox3Sbox2Sbox1, diffList, keyRecoveryConfiguration, encryption, configRound4SBox3Sbox2Sbox1.ExpectedDifference);
                    DifferentialAttackRoundResult resultRound4SBox3SBox2SBox1 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound4SBox3Sbox2Sbox1, encryption);
                    stopWatch.Stop();
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound4SBox3Sbox2Sbox1);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound4SBox3SBox2SBox1);

                    //Result
                    keyRecoveryConfiguration.subkey5 = analysis.MergeBytes(resultRound4SBox3SBox2SBox1.PossibleKey, resultRound4SBox3SBox2SBox1.PossibleKey, resultRound4SBox3SBox2SBox1.PossibleKey, resultRound4SBox4.PossibleKey);
                    keyRecoveryConfiguration.recoveredSubkey5 = true;
                    */

                    //attack round 5 sbox4
                    DifferentialAttackRoundConfiguration configRound5SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox4.json");
                    configRound5SBox4 = analysis.RefreshPairLists(configRound5SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    if (filter)
                    {
                        configRound5SBox4.FilteredPairList = analysis.FilterPairs(configRound5SBox4, diffList, keyRecoveryConfiguration, encryption, configRound5SBox4.ExpectedDifference);
                    }
                    DifferentialAttackRoundResult resultRound5SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound5SBox4, encryption);
                    stopWatch.Stop();
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound5SBox4);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound5SBox4);

                    //attack round 5 sbox3
                    DifferentialAttackRoundConfiguration configRound5SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox3.json");
                    configRound5SBox3 = analysis.RefreshPairLists(configRound5SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    if (filter)
                    {
                        configRound5SBox3.FilteredPairList = analysis.FilterPairs(configRound5SBox3, diffList, keyRecoveryConfiguration, encryption, configRound5SBox3.ExpectedDifference);
                    }
                    DifferentialAttackRoundResult resultRound5SBox3 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound5SBox3, encryption);
                    stopWatch.Stop();
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound5SBox3);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound5SBox3);

                    //attack round 5 sbox2
                    DifferentialAttackRoundConfiguration configRound5SBox2 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox2.json");
                    configRound5SBox2 = analysis.RefreshPairLists(configRound5SBox2, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    if (filter)
                    {
                        configRound5SBox2.FilteredPairList = analysis.FilterPairs(configRound5SBox2, diffList, keyRecoveryConfiguration, encryption, configRound5SBox2.ExpectedDifference);
                    }
                    DifferentialAttackRoundResult resultRound5SBox2 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound5SBox2, encryption);
                    stopWatch.Stop();
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound5SBox2);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound5SBox2);

                    //attack round 5 sbox1
                    DifferentialAttackRoundConfiguration configRound5SBox1 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1.json");
                    configRound5SBox1 = analysis.RefreshPairLists(configRound5SBox1, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    if (filter)
                    {
                        configRound5SBox1.FilteredPairList = analysis.FilterPairs(configRound5SBox1, diffList, keyRecoveryConfiguration, encryption, configRound5SBox1.ExpectedDifference);
                    }
                    DifferentialAttackRoundResult resultRound5SBox1 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound5SBox1, encryption);
                    stopWatch.Stop();
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound5SBox1);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound5SBox1);

                    //Result
                    keyRecoveryConfiguration.subkey6 = analysis.MergeBytes(resultRound5SBox1.PossibleKey, resultRound5SBox2.PossibleKey, resultRound5SBox3.PossibleKey, resultRound5SBox4.PossibleKey);
                    keyRecoveryConfiguration.recoveredSubkey6 = true;
               

                    //attack round 4 sbox4
                    DifferentialAttackRoundConfiguration configRound4SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound4SBox4.json");
                    configRound4SBox4 = analysis.RefreshPairLists(configRound4SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    if (filter)
                    {
                        configRound4SBox4.FilteredPairList = analysis.FilterPairs(configRound4SBox4, diffList, keyRecoveryConfiguration, encryption, configRound4SBox4.ExpectedDifference);
                    }
                    DifferentialAttackRoundResult resultRound4SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound4SBox4, encryption);
                    stopWatch.Stop();
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound4SBox4);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound4SBox4);

                    //attack round 4 sbox3
                    DifferentialAttackRoundConfiguration configRound4SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound4SBox3.json");
                    configRound4SBox3 = analysis.RefreshPairLists(configRound4SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    if (filter)
                    {
                        configRound4SBox3.FilteredPairList = analysis.FilterPairs(configRound4SBox3, diffList, keyRecoveryConfiguration, encryption, configRound4SBox3.ExpectedDifference);
                    }
                    DifferentialAttackRoundResult resultRound4SBox3 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound4SBox3, encryption);
                    stopWatch.Stop();
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound4SBox3);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound4SBox3);

                    //attack round 4 sbox2
                    DifferentialAttackRoundConfiguration configRound4SBox2 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound4SBox2.json");
                    configRound4SBox2 = analysis.RefreshPairLists(configRound4SBox2, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    if (filter)
                    {
                        configRound4SBox2.FilteredPairList = analysis.FilterPairs(configRound4SBox2, diffList, keyRecoveryConfiguration, encryption, configRound4SBox2.ExpectedDifference);
                    }
                    DifferentialAttackRoundResult resultRound4SBox2 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound4SBox2, encryption);
                    stopWatch.Stop();
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound4SBox2);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound4SBox2);

                    //attack round 4 sbox1
                    DifferentialAttackRoundConfiguration configRound4SBox1 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound4SBox1.json");
                    configRound4SBox1 = analysis.RefreshPairLists(configRound4SBox1, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    if (filter)
                    {
                        configRound4SBox1.FilteredPairList = analysis.FilterPairs(configRound4SBox1, diffList, keyRecoveryConfiguration, encryption, configRound4SBox1.ExpectedDifference);
                    }
                    DifferentialAttackRoundResult resultRound4SBox1 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound4SBox1, encryption);
                    stopWatch.Stop();
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound4SBox1);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound4SBox1);

                    //Result
                    keyRecoveryConfiguration.subkey5 = resultRound4SBox1.PossibleKey ^ resultRound4SBox2.PossibleKey ^ resultRound4SBox3.PossibleKey ^ resultRound4SBox4.PossibleKey;
                    keyRecoveryConfiguration.recoveredSubkey5 = true;


                    //attack round 3 sbox4
                    DifferentialAttackRoundConfiguration configRound3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox4.json");
                    configRound3SBox4 = analysis.RefreshPairLists(configRound3SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    if (filter)
                    {
                        configRound3SBox4.FilteredPairList = analysis.FilterPairs(configRound3SBox4, diffList, keyRecoveryConfiguration, encryption, configRound3SBox4.ExpectedDifference);
                    }
                    DifferentialAttackRoundResult resultRound3SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound3SBox4, encryption);
                    stopWatch.Stop();
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound3SBox4);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound3SBox4);

                    //attack round 3 sbox3
                    DifferentialAttackRoundConfiguration configRound3SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox3.json");
                    configRound3SBox3 = analysis.RefreshPairLists(configRound3SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    if (filter)
                    {
                        configRound3SBox3.FilteredPairList = analysis.FilterPairs(configRound3SBox3, diffList, keyRecoveryConfiguration, encryption, configRound3SBox3.ExpectedDifference);
                    }
                    DifferentialAttackRoundResult resultRound3SBox3 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound3SBox3, encryption);
                    stopWatch.Stop();
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound3SBox3);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound3SBox3);

                    //attack round 3 sbox2
                    DifferentialAttackRoundConfiguration configRound3SBox2 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox2.json");
                    configRound3SBox2 = analysis.RefreshPairLists(configRound3SBox2, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    if (filter)
                    {
                        configRound3SBox2.FilteredPairList = analysis.FilterPairs(configRound3SBox2, diffList, keyRecoveryConfiguration, encryption, configRound3SBox2.ExpectedDifference);
                    }
                    DifferentialAttackRoundResult resultRound3SBox2 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound3SBox2, encryption);
                    stopWatch.Stop();
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound3SBox2);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound3SBox2);

                    //attack round 3 sbox1
                    DifferentialAttackRoundConfiguration configRound3SBox1 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox1.json");
                    configRound3SBox1 = analysis.RefreshPairLists(configRound3SBox1, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    if (filter)
                    {
                        configRound3SBox1.FilteredPairList = analysis.FilterPairs(configRound3SBox1, diffList, keyRecoveryConfiguration, encryption, configRound3SBox1.ExpectedDifference);
                    }
                    DifferentialAttackRoundResult resultRound3SBox1 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound3SBox1, encryption);
                    stopWatch.Stop();
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound3SBox1);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound3SBox1);

                    //Result
                    keyRecoveryConfiguration.subkey4 = (resultRound3SBox4.PossibleKey ^ resultRound3SBox3.PossibleKey ^ resultRound3SBox2.PossibleKey ^ resultRound3SBox1.PossibleKey);
                    keyRecoveryConfiguration.recoveredSubkey4 = true;


                    //attack round 2 sbox4
                    DifferentialAttackRoundConfiguration configRound2SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound2SBox4.json");
                    configRound2SBox4 = analysis.RefreshPairLists(configRound2SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    if (filter)
                    {
                        configRound2SBox4.FilteredPairList = analysis.FilterPairs(configRound2SBox4, diffList, keyRecoveryConfiguration, encryption, configRound2SBox4.ExpectedDifference);
                    }
                    DifferentialAttackRoundResult resultRound2SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound2SBox4, encryption);
                    stopWatch.Stop();
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound2SBox4);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound2SBox4);

                    //attack round 2 sbox3
                    DifferentialAttackRoundConfiguration configRound2SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound2SBox3.json");
                    configRound2SBox3 = analysis.RefreshPairLists(configRound2SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    if (filter)
                    {
                        configRound2SBox3.FilteredPairList = analysis.FilterPairs(configRound2SBox3, diffList, keyRecoveryConfiguration, encryption, configRound2SBox3.ExpectedDifference);
                    }
                    DifferentialAttackRoundResult resultRound2SBox3 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound2SBox3, encryption);
                    stopWatch.Stop();
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound2SBox3);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound2SBox3);

                    //attack round 2 sbox2
                    DifferentialAttackRoundConfiguration configRound2SBox2 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound2SBox2.json");
                    configRound2SBox2 = analysis.RefreshPairLists(configRound2SBox2, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    if (filter)
                    {
                        configRound2SBox2.FilteredPairList = analysis.FilterPairs(configRound2SBox2, diffList, keyRecoveryConfiguration, encryption, configRound2SBox2.ExpectedDifference);
                    }
                    DifferentialAttackRoundResult resultRound2SBox2 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound2SBox2, encryption);
                    stopWatch.Stop();
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound2SBox2);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound2SBox2);

                    //attack round 2 sbox1
                    DifferentialAttackRoundConfiguration configRound2SBox1 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound2SBox1.json");
                    configRound2SBox1 = analysis.RefreshPairLists(configRound2SBox1, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    if (filter)
                    {
                        configRound2SBox1.FilteredPairList = analysis.FilterPairs(configRound2SBox1, diffList, keyRecoveryConfiguration, encryption, configRound2SBox1.ExpectedDifference);
                    }
                    DifferentialAttackRoundResult resultRound2SBox1 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound2SBox1, encryption);
                    stopWatch.Stop();
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound2SBox1);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound2SBox1);

                    //Result
                    keyRecoveryConfiguration.subkey3 = (resultRound2SBox4.PossibleKey ^ resultRound2SBox3.PossibleKey ^ resultRound2SBox2.PossibleKey ^ resultRound2SBox1.PossibleKey);
                    keyRecoveryConfiguration.recoveredSubkey3 = true;

                    try
                    {
                        stopWatch.Start();
                        DifferentialAttackLastRoundResult lastRoundResult = analysis.AttackFirstRound(keyRecoveryConfiguration, encryption);
                        stopWatch.Stop();
                        keyRecoveryConfiguration.LastRoundResult = lastRoundResult;
                        keyRecoveryConfiguration.recoveredSubkey2 = true;
                        keyRecoveryConfiguration.subkey2 = lastRoundResult.SubKey1;
                        keyRecoveryConfiguration.recoveredSubkey1 = true;
                        keyRecoveryConfiguration.subkey1 = lastRoundResult.SubKey0;
                    }
                    catch (Exception e)
                    {
                        success = false;
                        stopWatch.Stop();
                    }

                    if (success)
                    {
                        successCounter++;
                    }
                    else
                    {
                        failureCounter++;
                    }

                    ts += stopWatch.Elapsed;
                    stopWatch.Reset();

                    Console.WriteLine("Finished iteration " + (i+1) + " / " + testCount + " Pairs: " + pairCount[j]);
                }

                //string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                string elapsedTime = "" + (ts.TotalSeconds / testCount);
                Console.WriteLine("Average RunTime " + elapsedTime + " seconds. Success: " + successCounter + " Failure: " + failureCounter);

                using (StreamWriter sw = File.AppendText(fileName))
                {
                    sw.WriteLine(elapsedTime);
                }
            }
        }

        static void BenchmarkFrequencyDistribution()
        {
            for (int i = 0; i < 1; i++)
            {
                Encryption encryption = new Encryption();
                Analysis analysis = new Analysis();

                List<SBoxCharacteristic> diffList = analysis.CountDifferentialsSingleSBox();

                bool[] SBoxToAttack = new bool[] { false, true, false, false };

                CipherFourDifferentialKeyRecoveryAttack keyRecoveryConfiguration = new CipherFourDifferentialKeyRecoveryAttack();

                AbortingPolicy ap = AbortingPolicy.Threshold;
                SearchPolicy sp = SearchPolicy.FirstBestCharacteristicDepthSearch;

                DifferentialAttackRoundConfiguration configRoundSBox = analysis.GenerateConfigurationAttack(5, SBoxToAttack, ap, sp, diffList, keyRecoveryConfiguration, encryption);

                List<Pair> unfilteredList = new List<Pair>();

                //copy pair list
                foreach (var curPair in configRoundSBox.UnfilteredPairList)
                {
                    unfilteredList.Add(new Pair
                    {
                        LeftMember = curPair.LeftMember,
                        RightMember = curPair.RightMember
                    });
                }

                //attack with 100 pairs without filter
                configRoundSBox.FilteredPairList = unfilteredList.GetRange(0, 100).ToList();
                Console.WriteLine("Pairs to check: " + configRoundSBox.FilteredPairList.Count);
                DifferentialAttackRoundResult result = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRoundSBox, encryption);
                foreach (var curKeyProb in result.KeyCandidateProbabilities.OrderBy(var => var.Key))
                {
                    using (StreamWriter sw = File.AppendText("100_Pairs_NoFilter.txt"))
                    {
                        sw.WriteLine(curKeyProb.Counter);
                    }
                }

                //attack with 100 pairs with filter
                configRoundSBox.UnfilteredPairList = unfilteredList.GetRange(0, 100).ToList();
                configRoundSBox.FilteredPairList = analysis.FilterPairs(configRoundSBox, diffList, keyRecoveryConfiguration, encryption, configRoundSBox.ExpectedDifference);
                Console.WriteLine("Pairs to check: " + configRoundSBox.FilteredPairList.Count);
                result = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRoundSBox, encryption);
                foreach (var curKeyProb in result.KeyCandidateProbabilities.OrderBy(var => var.Key))
                {
                    using (StreamWriter sw = File.AppendText("100_Pairs_Filter.txt"))
                    {
                        sw.WriteLine(curKeyProb.Counter);
                    }
                }

                //attack with 200 pairs without filter
                configRoundSBox.FilteredPairList = unfilteredList.GetRange(0, 200).ToList();
                Console.WriteLine("Pairs to check: " + configRoundSBox.FilteredPairList.Count);
                result = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRoundSBox, encryption);
                foreach (var curKeyProb in result.KeyCandidateProbabilities.OrderBy(var => var.Key))
                {
                    using (StreamWriter sw = File.AppendText("200_Pairs_NoFilter.txt"))
                    {
                        sw.WriteLine(curKeyProb.Counter);
                    }
                }

                //attack with 200 pairs with filter
                configRoundSBox.UnfilteredPairList = unfilteredList.GetRange(0, 200).ToList();
                configRoundSBox.FilteredPairList = analysis.FilterPairs(configRoundSBox, diffList, keyRecoveryConfiguration, encryption, configRoundSBox.ExpectedDifference);
                Console.WriteLine("Pairs to check: " + configRoundSBox.FilteredPairList.Count);
                result = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRoundSBox, encryption);
                foreach (var curKeyProb in result.KeyCandidateProbabilities.OrderBy(var => var.Key))
                {
                    using (StreamWriter sw = File.AppendText("200_Pairs_Filter.txt"))
                    {
                        sw.WriteLine(curKeyProb.Counter);
                    }
                }

                //attack with 400 pairs without filter
                configRoundSBox.FilteredPairList = unfilteredList.GetRange(0, 400).ToList();
                Console.WriteLine("Pairs to check: " + configRoundSBox.FilteredPairList.Count);
                result = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRoundSBox, encryption);
                foreach (var curKeyProb in result.KeyCandidateProbabilities.OrderBy(var => var.Key))
                {
                    using (StreamWriter sw = File.AppendText("400_Pairs_NoFilter.txt"))
                    {
                        sw.WriteLine(curKeyProb.Counter);
                    }
                }

                //attack with 400 pairs with filter
                configRoundSBox.UnfilteredPairList = unfilteredList.GetRange(0, 400).ToList();
                configRoundSBox.FilteredPairList = analysis.FilterPairs(configRoundSBox, diffList, keyRecoveryConfiguration, encryption, configRoundSBox.ExpectedDifference);
                Console.WriteLine("Pairs to check: " + configRoundSBox.FilteredPairList.Count);
                result = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRoundSBox, encryption);
                foreach (var curKeyProb in result.KeyCandidateProbabilities.OrderBy(var => var.Key))
                {
                    using (StreamWriter sw = File.AppendText("400_Pairs_Filter.txt"))
                    {
                        sw.WriteLine(curKeyProb.Counter);
                    }
                }

                //attack with 500 pairs without filter
                configRoundSBox.FilteredPairList = unfilteredList.GetRange(0, 500).ToList();
                Console.WriteLine("Pairs to check: " + configRoundSBox.FilteredPairList.Count);
                result = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRoundSBox, encryption);
                foreach (var curKeyProb in result.KeyCandidateProbabilities.OrderBy(var => var.Key))
                {
                    using (StreamWriter sw = File.AppendText("500_Pairs_NoFilter.txt"))
                    {
                        sw.WriteLine(curKeyProb.Counter);
                    }
                }

                //attack with 500 pairs with filter
                configRoundSBox.UnfilteredPairList = unfilteredList.GetRange(0, 500).ToList();
                configRoundSBox.FilteredPairList = analysis.FilterPairs(configRoundSBox, diffList, keyRecoveryConfiguration, encryption, configRoundSBox.ExpectedDifference);
                Console.WriteLine("Pairs to check: " + configRoundSBox.FilteredPairList.Count);
                result = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRoundSBox, encryption);
                foreach (var curKeyProb in result.KeyCandidateProbabilities.OrderBy(var => var.Key))
                {
                    using (StreamWriter sw = File.AppendText("500_Pairs_Filter.txt"))
                    {
                        sw.WriteLine(curKeyProb.Counter);
                    }
                }
            }
        }

        static void SearchDifferentials()
        {
            Encryption encryption = new Encryption();
            Analysis analysis = new Analysis();

            //analyse the sbox
            List<SBoxCharacteristic> diffList = analysis.CountDifferentialsSingleSBox();

            bool[] attackSBox4 = new bool[] { true, false, false, false };
            bool[] attackSBox3 = new bool[] { false, true, false, false };
            bool[] attackSBox2 = new bool[] { false, false, true, false };
            bool[] attackSBox1 = new bool[] { false, false, false, true };

            bool[] attackSBox4AndSBox2 = new bool[] { true, false, true, false };
            bool[] attackSBox3AndSBox1 = new bool[] { false, true, false, true };
            bool[] attackSBox4AndSBox3 = new bool[] { true, true, false, false };
            bool[] attackSBox2AndSBox1 = new bool[] { false, false, true, true };
            bool[] attackSBox3AndSBox2 = new bool[] { false, true, true, false };
            bool[] attackSBox4AndSBox1 = new bool[] { true, false, false, true };

            for (int i = 5; i >= 3; i--)
            {
                CipherFourDifferentialKeyRecoveryAttack keyRecoveryConfigurationTest = new CipherFourDifferentialKeyRecoveryAttack();
                AbortingPolicy ap = AbortingPolicy.Threshold;
                SearchPolicy sp = SearchPolicy.FirstAllCharacteristicsDepthSearch;

                DifferentialAttackRoundConfiguration configRoundSBox4AndSBox2 = analysis.GenerateConfigurationAttack(i, attackSBox4AndSBox2, ap, sp, diffList, keyRecoveryConfigurationTest, encryption);
                Console.WriteLine("Round " + i + " attackSBox4AndSBox2 Probability: " + configRoundSBox4AndSBox2.Probability);

                DifferentialAttackRoundConfiguration configRoundSBox3AndSBox1 = analysis.GenerateConfigurationAttack(i, attackSBox3AndSBox1, ap, sp, diffList, keyRecoveryConfigurationTest, encryption);
                Console.WriteLine("Round " + i + " attackSBox3AndSBox1 Probability: " + configRoundSBox3AndSBox1.Probability);

                DifferentialAttackRoundConfiguration configRoundSBox4AndSBox3 = analysis.GenerateConfigurationAttack(i, attackSBox4AndSBox3, ap, sp, diffList, keyRecoveryConfigurationTest, encryption);
                Console.WriteLine("Round " + i + " attackSBox4AndSBox3 Probability: " + configRoundSBox4AndSBox3.Probability);

                DifferentialAttackRoundConfiguration configRoundSBox2AndSBox1 = analysis.GenerateConfigurationAttack(i, attackSBox2AndSBox1, ap, sp, diffList, keyRecoveryConfigurationTest, encryption);
                Console.WriteLine("Round " + i + " attackSBox2AndSBox1 Probability: " + configRoundSBox2AndSBox1.Probability);

                DifferentialAttackRoundConfiguration configRoundSBox3AndSBox2 = analysis.GenerateConfigurationAttack(i, attackSBox3AndSBox2, ap, sp, diffList, keyRecoveryConfigurationTest, encryption);
                Console.WriteLine("Round " + i + " attackSBox3AndSBox2 Probability: " + configRoundSBox3AndSBox2.Probability);

                DifferentialAttackRoundConfiguration configRoundSBox4AndSBox1 = analysis.GenerateConfigurationAttack(i, attackSBox4AndSBox1, ap, sp, diffList, keyRecoveryConfigurationTest, encryption);
                Console.WriteLine("Round " + i + " attackSBox4AndSBox1 Probability: " + configRoundSBox4AndSBox1.Probability);
            }
        }

        static void AttackAllRoundsBenchmark()
        {
            Encryption encryption = new Encryption();
            Analysis analysis = new Analysis();

            Stopwatch stopWatch = new Stopwatch();

            //analyse the sbox
            List<SBoxCharacteristic> diffList = analysis.CountDifferentialsSingleSBox();

            int successCounter = 0;
            int failureCounter = 0;
            int testCount = 1000;

            for (int i = 1; i <= testCount; i++)
            {
                encryption.GenerateRandomKeys();
                stopWatch.Start();
                bool success = true;

                Console.WriteLine("Running test " + i + " / " + testCount + " success-rate: " + successCounter + " / " + testCount + " failure-rate: " + failureCounter + " / " + testCount);

                //Check the attack new
                CipherFourDifferentialKeyRecoveryAttack keyRecoveryConfiguration = new CipherFourDifferentialKeyRecoveryAttack();

                bool[] attackSBox4 = new bool[] { true, false, false, false };
                bool[] attackSBox3 = new bool[] { false, true, false, false };
                bool[] attackSBox2 = new bool[] { false, false, true, false };
                bool[] attackSBox1 = new bool[] { false, false, false, true };

                bool[] attackSBox4AndSBox2 = new bool[] { true, false, true, false };
                bool[] attackSBox3AndSBox1 = new bool[] { false, true, false, true };

                bool[] attackSBox4AndSBox3AndSBox2AndSBox1 = new bool[] { true, true, true, true };

                SearchPolicy curSearchPolicy = SearchPolicy.FirstBestCharacteristicDepthSearch;
                AbortingPolicy curAbortingPolicy = AbortingPolicy.Threshold;

                Console.WriteLine("Key to attack in round 5: " + encryption.PrintKeyBits(5));

                /* */
                //attack round 5 sbox4
                DifferentialAttackRoundConfiguration configRound5SBox4 = analysis.GenerateConfigurationAttack(5, attackSBox4AndSBox2, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
                //DataLoader.saveDifferentialAttackRoundConfiguration(configRound5SBox4, "configRound5SBox4.json");
                //DifferentialAttackRoundConfiguration configRound4SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration();
                DifferentialAttackRoundResult resultRound5SBox4 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configRound5SBox4, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound5SBox4);
                keyRecoveryConfiguration.RoundResults.Add(resultRound5SBox4);

                //attack round 5 sbox3
                DifferentialAttackRoundConfiguration configRound5SBox3 = analysis.GenerateConfigurationAttack(5, attackSBox3AndSBox1, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
                //DataLoader.saveDifferentialAttackRoundConfiguration(configRound5SBox3, "configRound5SBox3.json");
                DifferentialAttackRoundResult resultRound5SBox3 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configRound5SBox3, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound5SBox3);
                keyRecoveryConfiguration.RoundResults.Add(resultRound5SBox3);

                //attack round 5 sbox2
                DifferentialAttackRoundConfiguration configRound5SBox2 = analysis.GenerateConfigurationAttack(5, attackSBox2, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
                //DataLoader.saveDifferentialAttackRoundConfiguration(configRound5SBox2, "configRound5SBox2.json");
                DifferentialAttackRoundResult resultRound5SBox2 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configRound5SBox2, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound5SBox2);
                keyRecoveryConfiguration.RoundResults.Add(resultRound5SBox2);

                //attack round 5 sbox1
                DifferentialAttackRoundConfiguration configRound5SBox1 = analysis.GenerateConfigurationAttack(5, attackSBox1, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
                //DataLoader.saveDifferentialAttackRoundConfiguration(configRound5SBox1, "configRound5SBox1.json");
                DifferentialAttackRoundResult resultRound5SBox1 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configRound5SBox1, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound5SBox1);
                keyRecoveryConfiguration.RoundResults.Add(resultRound5SBox1);

                //Result
                keyRecoveryConfiguration.subkey6 = analysis.MergeBytes(resultRound5SBox1.PossibleKey, resultRound5SBox2.PossibleKey, resultRound5SBox3.PossibleKey, resultRound5SBox4.PossibleKey);
                keyRecoveryConfiguration.recoveredSubkey6 = true;
                Console.WriteLine(keyRecoveryConfiguration.printRecoveredSubkeyBits());



                /* 
                keyRecoveryConfiguration.recoveredSubkey6 = true;
                keyRecoveryConfiguration.subkey6 = encryption._keys[5];

                Console.WriteLine("Real key bits: " + encryption.PrintKeyBits(5));
                Console.WriteLine(keyRecoveryConfiguration.printRecoveredSubkeyBits());
                */

                /* */
                Console.WriteLine("Key to attack in round 4: " + encryption.PrintKeyBits(4));

                //attack round 4 sbox4
                DifferentialAttackRoundConfiguration configRound4SBox4 = analysis.GenerateConfigurationAttack(4, attackSBox4, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
                DataLoader.saveDifferentialAttackRoundConfiguration(configRound4SBox4, "configRound4SBox4.json");
                DifferentialAttackRoundResult resultRound4SBox4 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configRound4SBox4, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound4SBox4);
                keyRecoveryConfiguration.RoundResults.Add(resultRound4SBox4);

                //attack round 4 sbox3
                DifferentialAttackRoundConfiguration configRound4SBox3 = analysis.GenerateConfigurationAttack(4, attackSBox3, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
                //DataLoader.saveDifferentialAttackRoundConfiguration(configRound4SBox3, "configRound4SBox3.json");
                DifferentialAttackRoundResult resultRound4SBox3 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configRound4SBox3, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound4SBox3);
                keyRecoveryConfiguration.RoundResults.Add(resultRound4SBox3);

                //attack round 4 sbox2
                DifferentialAttackRoundConfiguration configRound4SBox2 = analysis.GenerateConfigurationAttack(4, attackSBox2, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
                //DataLoader.saveDifferentialAttackRoundConfiguration(configRound4SBox2, "configRound4SBox2.json");
                DifferentialAttackRoundResult resultRound4SBox2 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configRound4SBox2, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound4SBox2);
                keyRecoveryConfiguration.RoundResults.Add(resultRound4SBox2);

                //attack round 4 sbox1
                DifferentialAttackRoundConfiguration configRound4SBox1 = analysis.GenerateConfigurationAttack(4, attackSBox1, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
                //DataLoader.saveDifferentialAttackRoundConfiguration(configRound4SBox1, "configRound4SBox1.json");
                DifferentialAttackRoundResult resultRound4SBox1 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configRound4SBox1, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound4SBox1);
                keyRecoveryConfiguration.RoundResults.Add(resultRound4SBox1);

                //Result
                keyRecoveryConfiguration.subkey5 = (resultRound4SBox4.PossibleKey ^ resultRound4SBox3.PossibleKey ^ resultRound4SBox2.PossibleKey ^ resultRound4SBox1.PossibleKey);
                keyRecoveryConfiguration.recoveredSubkey5 = true;
                Console.WriteLine(keyRecoveryConfiguration.printRecoveredSubkeyBits());



                /* 
                keyRecoveryConfiguration.recoveredSubkey5 = true;
                keyRecoveryConfiguration.subkey5 = encryption._keys[4];

                Console.WriteLine("Real key bits: " + encryption.PrintKeyBits(4));
                Console.WriteLine(keyRecoveryConfiguration.printRecoveredSubkeyBits());
                */

                /*  */
                Console.WriteLine("Key to attack in round 3: " + encryption.PrintKeyBits(3));

                //attack round 3 sbox4
                DifferentialAttackRoundConfiguration configRound3SBox4 = analysis.GenerateConfigurationAttack(3, attackSBox4, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
                DataLoader.saveDifferentialAttackRoundConfiguration(configRound3SBox4, "configRound3SBox4.json");
                DifferentialAttackRoundResult resultRound3SBox4 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configRound3SBox4, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound3SBox4);
                keyRecoveryConfiguration.RoundResults.Add(resultRound3SBox4);

                //attack round 3 sbox3
                DifferentialAttackRoundConfiguration configRound3SBox3 = analysis.GenerateConfigurationAttack(3, attackSBox3, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
                DataLoader.saveDifferentialAttackRoundConfiguration(configRound3SBox3, "configRound3SBox3.json");
                DifferentialAttackRoundResult resultRound3SBox3 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configRound3SBox3, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound3SBox3);
                keyRecoveryConfiguration.RoundResults.Add(resultRound3SBox3);

                //attack round 3 sbox2
                DifferentialAttackRoundConfiguration configRound3SBox2 = analysis.GenerateConfigurationAttack(3, attackSBox2, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
                DataLoader.saveDifferentialAttackRoundConfiguration(configRound3SBox2, "configRound3SBox2.json");
                DifferentialAttackRoundResult resultRound3SBox2 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configRound3SBox2, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound3SBox2);
                keyRecoveryConfiguration.RoundResults.Add(resultRound3SBox2);

                //attack round 3 sbox1
                DifferentialAttackRoundConfiguration configRound3SBox1 = analysis.GenerateConfigurationAttack(3, attackSBox1, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
                DataLoader.saveDifferentialAttackRoundConfiguration(configRound3SBox1, "configRound3SBox1.json");
                DifferentialAttackRoundResult resultRound3SBox1 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configRound3SBox1, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound3SBox1);
                keyRecoveryConfiguration.RoundResults.Add(resultRound3SBox1);

                //Result
                keyRecoveryConfiguration.subkey4 = (resultRound3SBox4.PossibleKey ^ resultRound3SBox3.PossibleKey ^ resultRound3SBox2.PossibleKey ^ resultRound3SBox1.PossibleKey);
                keyRecoveryConfiguration.recoveredSubkey4 = true;
                Console.WriteLine(keyRecoveryConfiguration.printRecoveredSubkeyBits());



                /* 
                keyRecoveryConfiguration.recoveredSubkey4 = true;
                keyRecoveryConfiguration.subkey4 = encryption._keys[3];

                Console.WriteLine("Real key bits: " + encryption.PrintKeyBits(3));
                Console.WriteLine(keyRecoveryConfiguration.printRecoveredSubkeyBits());
                */

                /*  */
                Console.WriteLine("Key to attack in round 2: " + encryption.PrintKeyBits(2));

                //attack round 2 sbox4
                DifferentialAttackRoundConfiguration configRound2SBox4 = analysis.GenerateConfigurationAttack(2, attackSBox4, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
                DataLoader.saveDifferentialAttackRoundConfiguration(configRound2SBox4, "configRound2SBox4.json");
                DifferentialAttackRoundResult resultRound2SBox4 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configRound2SBox4, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound2SBox4);
                keyRecoveryConfiguration.RoundResults.Add(resultRound2SBox4);

                //attack round 2 sbox3
                DifferentialAttackRoundConfiguration configRound2SBox3 = analysis.GenerateConfigurationAttack(2, attackSBox3, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
                DataLoader.saveDifferentialAttackRoundConfiguration(configRound2SBox3, "configRound2SBox3.json");
                DifferentialAttackRoundResult resultRound2SBox3 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configRound2SBox3, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound2SBox3);
                keyRecoveryConfiguration.RoundResults.Add(resultRound2SBox3);

                //attack round 2 sbox2
                DifferentialAttackRoundConfiguration configRound2SBox2 = analysis.GenerateConfigurationAttack(2, attackSBox2, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
                DataLoader.saveDifferentialAttackRoundConfiguration(configRound2SBox2, "configRound2SBox2.json");
                DifferentialAttackRoundResult resultRound2SBox2 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configRound2SBox2, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound2SBox2);
                keyRecoveryConfiguration.RoundResults.Add(resultRound2SBox2);

                //attack round 2 sbox1
                DifferentialAttackRoundConfiguration configRound2SBox1 = analysis.GenerateConfigurationAttack(2, attackSBox1, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
                DataLoader.saveDifferentialAttackRoundConfiguration(configRound2SBox1, "configRound2SBox1.json");
                DifferentialAttackRoundResult resultRound2SBox1 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configRound2SBox1, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound2SBox1);
                keyRecoveryConfiguration.RoundResults.Add(resultRound2SBox1);

                //Result
                keyRecoveryConfiguration.subkey3 = (resultRound2SBox4.PossibleKey ^ resultRound2SBox3.PossibleKey ^ resultRound2SBox2.PossibleKey ^ resultRound2SBox1.PossibleKey);
                keyRecoveryConfiguration.recoveredSubkey3 = true;
                Console.WriteLine(keyRecoveryConfiguration.printRecoveredSubkeyBits());

                /* 
                keyRecoveryConfiguration.recoveredSubkey3 = true;
                keyRecoveryConfiguration.subkey3 = encryption._keys[2];

                Console.WriteLine("Real key bits: " + encryption.PrintKeyBits(2));
                Console.WriteLine(keyRecoveryConfiguration.printRecoveredSubkeyBits());
                */

                /*  */
                Console.WriteLine("Key to attack in round 1: " + encryption.PrintKeyBits(1));
                Console.WriteLine("Key to attack in round 1: " + encryption.PrintKeyBits(0));

                try
                {
                    DifferentialAttackLastRoundResult lastRoundResult = analysis.AttackFirstRound(keyRecoveryConfiguration, encryption);
                    keyRecoveryConfiguration.LastRoundResult = lastRoundResult;
                    keyRecoveryConfiguration.recoveredSubkey2 = true;
                    keyRecoveryConfiguration.subkey2 = lastRoundResult.SubKey1;
                    keyRecoveryConfiguration.recoveredSubkey1 = true;
                    keyRecoveryConfiguration.subkey1 = lastRoundResult.SubKey0;
                    Console.WriteLine(keyRecoveryConfiguration.printRecoveredSubkeyBits());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Something went wrong. Key not recovered.");
                    success = false;
                }

                if (success)
                {
                    successCounter++;
                }
                else
                {
                    failureCounter++;
                }

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);
                stopWatch.Reset();
            }
        }

        static void BenchmarkSuccessRate()
        {
            Encryption encryption = new Encryption();
            Analysis analysis = new Analysis();

            //analyse the sbox
            List<SBoxCharacteristic> diffList = analysis.CountDifferentialsSingleSBox();

            int testCount = 100;

            //paircount for addition test, because incremented pairs do not work good
            //int[] pairCount = new[] { 2000, 4000, 6000, 8000, 10000, 12000, 14000, 16000, 18000, 20000 };

            //paircount for generell test
            int[] pairCount = new[] { 200, 400, 600, 800, 1000, 1200, 1400, 1600, 1800, 2000, 2200, 2400, 2600, 2800, 3000, 3200, 3400, 3600, 3800, 4000 };

            for (int j = 0; j < pairCount.Length; j++)
            {
                int successCounter = 0;
                int failureCounter = 0;

                //string fileName = "Cipher3_SuccessRate_IncrementedPairs.txt";
                string fileName = "Cipher3_SuccessRate_RandomPairs.txt";

                //to change way of pair-generation, edit GenerateInputPairList() method in Analysis-Class
                for (int i = 0; i < testCount; i++)
                {
                    encryption.GenerateRandomKeys();
                    bool success = true;

                    //Für die Schlüsselbits von S - Boxr4 wurde die aktive S-Box 0001 gewählt, 
                    //für die Schlüsselbits von S - Boxr3     die aktive S - Box 0010, 
                    //für die Schlüsselbits von S - Boxr1  die aktiven S - Boxen 1001 und 
                    //für die Schlüsselbits von S-Boxr2    die aktiven S - Boxen 1110 verwendet

                    CipherFourDifferentialKeyRecoveryAttack keyRecoveryConfiguration = new CipherFourDifferentialKeyRecoveryAttack();

                    //attack round 5 sbox4
                    DifferentialAttackRoundConfiguration configRound5SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox4.json");
                    configRound5SBox4 = analysis.RefreshPairLists(configRound5SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    DifferentialAttackRoundResult resultRound5SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound5SBox4, encryption);
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound5SBox4);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound5SBox4);

                    //attack round 5 sbox3
                    DifferentialAttackRoundConfiguration configRound5SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox3.json");
                    configRound5SBox3 = analysis.RefreshPairLists(configRound5SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    DifferentialAttackRoundResult resultRound5SBox3 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound5SBox3, encryption);
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound5SBox3);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound5SBox3);

                    //attack round 5 sbox1
                    DifferentialAttackRoundConfiguration configRound5SBox1 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1SBox4.json");
                    configRound5SBox1 = analysis.RefreshPairLists(configRound5SBox1, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    DifferentialAttackRoundResult resultRound5SBox1 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound5SBox1, encryption);
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound5SBox1);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound5SBox1);

                    //attack round 5 sbox 2
                    DifferentialAttackRoundConfiguration configRound5SBox1SBox2SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound5SBox1SBox2SBox3.json");
                    configRound5SBox1SBox2SBox3 = analysis.RefreshPairLists(configRound5SBox1SBox2SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    DifferentialAttackRoundResult resultRound5SBox2 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound5SBox1SBox2SBox3, encryption);
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound5SBox1SBox2SBox3);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound5SBox2);


                    //Result
                    keyRecoveryConfiguration.subkey6 = analysis.MergeBytes(resultRound5SBox1.PossibleKey, resultRound5SBox2.PossibleKey, resultRound5SBox3.PossibleKey, resultRound5SBox4.PossibleKey);
                    keyRecoveryConfiguration.recoveredSubkey6 = true;


                    //attack round 4 sbox4
                    DifferentialAttackRoundConfiguration configRound4SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound4SBox4.json");
                    configRound4SBox4 = analysis.RefreshPairLists(configRound4SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    DifferentialAttackRoundResult resultRound4SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound4SBox4, encryption);
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound4SBox4);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound4SBox4);

                    //attack round 4 sbox3
                    DifferentialAttackRoundConfiguration configRound4SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound4SBox3.json");
                    configRound4SBox3 = analysis.RefreshPairLists(configRound4SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    DifferentialAttackRoundResult resultRound4SBox3 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound4SBox3, encryption);
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound4SBox3);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound4SBox3);

                    //attack round 4 sbox1
                    DifferentialAttackRoundConfiguration configRound4SBox1 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound4SBox1SBox4.json");
                    configRound4SBox1 = analysis.RefreshPairLists(configRound4SBox1, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    DifferentialAttackRoundResult resultRound4SBox1 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound4SBox1, encryption);
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound4SBox1);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound4SBox1);

                    //attack round 4 sbox 2
                    DifferentialAttackRoundConfiguration configRound4SBox1SBox2SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound4SBox1SBox2SBox3.json");
                    configRound4SBox1SBox2SBox3 = analysis.RefreshPairLists(configRound4SBox1SBox2SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    DifferentialAttackRoundResult resultRound4SBox2 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound4SBox1SBox2SBox3, encryption);
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound4SBox1SBox2SBox3);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound4SBox2);

                    BitArray bitsSBox1 = new BitArray(BitConverter.GetBytes(resultRound4SBox1.PossibleKey));
                    BitArray bitsSBox2 = new BitArray(BitConverter.GetBytes(resultRound4SBox2.PossibleKey));
                    BitArray bitsOfKey = new BitArray(BitConverter.GetBytes(resultRound4SBox4.PossibleKey ^ resultRound4SBox3.PossibleKey));

                    bitsOfKey[15] = bitsSBox1[15];
                    bitsOfKey[11] = bitsSBox1[11];
                    bitsOfKey[7] = bitsSBox1[7];
                    bitsOfKey[3] = bitsSBox1[3];

                    bitsOfKey[14] = bitsSBox2[14];
                    bitsOfKey[10] = bitsSBox2[10];
                    bitsOfKey[6] = bitsSBox2[6];
                    bitsOfKey[2] = bitsSBox2[2];

                    byte[] bytes = new byte[4];
                    bitsOfKey.CopyTo(bytes, 0);

                    int outputBlock = BitConverter.ToInt32(bytes, 0);

                    //Result
                    keyRecoveryConfiguration.subkey5 = (outputBlock);
                    keyRecoveryConfiguration.recoveredSubkey5 = true;


                    //attack round 3 sbox4
                    DifferentialAttackRoundConfiguration configRound3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox4.json");
                    configRound3SBox4 = analysis.RefreshPairLists(configRound3SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    DifferentialAttackRoundResult resultRound3SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound3SBox4, encryption);
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound3SBox4);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound3SBox4);

                    //attack round 3 sbox3
                    DifferentialAttackRoundConfiguration configRound3SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox3.json");
                    configRound3SBox3 = analysis.RefreshPairLists(configRound3SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    DifferentialAttackRoundResult resultRound3SBox3 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound3SBox3, encryption);
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound3SBox3);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound3SBox3);

                    //attack round 3 sbox1
                    DifferentialAttackRoundConfiguration configRound3SBox1 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox1SBox4.json");
                    configRound3SBox1 = analysis.RefreshPairLists(configRound3SBox1, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    DifferentialAttackRoundResult resultRound3SBox1 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound3SBox1, encryption);
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound3SBox1);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound3SBox1);

                    //attack round 3 sbox 2
                    DifferentialAttackRoundConfiguration configRound3SBox1SBox2SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox1SBox2SBox3.json");
                    configRound3SBox1SBox2SBox3 = analysis.RefreshPairLists(configRound3SBox1SBox2SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    DifferentialAttackRoundResult resultRound3SBox2 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound3SBox1SBox2SBox3, encryption);
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound3SBox1SBox2SBox3);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound3SBox2);

                    bitsSBox1 = new BitArray(BitConverter.GetBytes(resultRound3SBox1.PossibleKey));
                    bitsSBox2 = new BitArray(BitConverter.GetBytes(resultRound3SBox2.PossibleKey));
                    bitsOfKey = new BitArray(BitConverter.GetBytes(resultRound3SBox4.PossibleKey ^ resultRound3SBox3.PossibleKey));

                    bitsOfKey[15] = bitsSBox1[15];
                    bitsOfKey[11] = bitsSBox1[11];
                    bitsOfKey[7] = bitsSBox1[7];
                    bitsOfKey[3] = bitsSBox1[3];

                    bitsOfKey[14] = bitsSBox2[14];
                    bitsOfKey[10] = bitsSBox2[10];
                    bitsOfKey[6] = bitsSBox2[6];
                    bitsOfKey[2] = bitsSBox2[2];

                    bytes = new byte[4];
                    bitsOfKey.CopyTo(bytes, 0);

                    outputBlock = BitConverter.ToInt32(bytes, 0);

                    //Result
                    keyRecoveryConfiguration.subkey4 = (outputBlock);
                    keyRecoveryConfiguration.recoveredSubkey4 = true;

                    //Result
                    //keyRecoveryConfiguration.subkey4 = (resultRound3SBox4.PossibleKey ^ resultRound3SBox3.PossibleKey ^ resultRound3SBox2.PossibleKey ^ resultRound3SBox1.PossibleKey);
                    //keyRecoveryConfiguration.recoveredSubkey4 = true;


                    //attack round 2 sbox4
                    DifferentialAttackRoundConfiguration configRound2SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound2SBox4.json");
                    configRound2SBox4 = analysis.RefreshPairLists(configRound2SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    DifferentialAttackRoundResult resultRound2SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound2SBox4, encryption);
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound2SBox4);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound2SBox4);

                    //attack round 2 sbox3
                    DifferentialAttackRoundConfiguration configRound2SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound2SBox3.json");
                    configRound2SBox3 = analysis.RefreshPairLists(configRound2SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    DifferentialAttackRoundResult resultRound2SBox3 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound2SBox3, encryption);
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound2SBox3);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound2SBox3);

                    //attack round 2 sbox1
                    DifferentialAttackRoundConfiguration configRound2SBox1 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound2SBox1SBox4.json");
                    configRound2SBox1 = analysis.RefreshPairLists(configRound2SBox1, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    DifferentialAttackRoundResult resultRound2SBox1 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound2SBox1, encryption);
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound2SBox1);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound2SBox1);

                    //attack round 2 sbox 2
                    DifferentialAttackRoundConfiguration configRound2SBox1SBox2SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound2SBox1SBox2SBox3.json");
                    configRound2SBox1SBox2SBox3 = analysis.RefreshPairLists(configRound2SBox1SBox2SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    DifferentialAttackRoundResult resultRound2SBox2 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound2SBox1SBox2SBox3, encryption);
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound2SBox1SBox2SBox3);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound2SBox2);

                    bitsSBox1 = new BitArray(BitConverter.GetBytes(resultRound2SBox1.PossibleKey));
                    bitsSBox2 = new BitArray(BitConverter.GetBytes(resultRound2SBox2.PossibleKey));
                    bitsOfKey = new BitArray(BitConverter.GetBytes(resultRound2SBox4.PossibleKey ^ resultRound2SBox3.PossibleKey));

                    bitsOfKey[15] = bitsSBox1[15];
                    bitsOfKey[11] = bitsSBox1[11];
                    bitsOfKey[7] = bitsSBox1[7];
                    bitsOfKey[3] = bitsSBox1[3];

                    bitsOfKey[14] = bitsSBox2[14];
                    bitsOfKey[10] = bitsSBox2[10];
                    bitsOfKey[6] = bitsSBox2[6];
                    bitsOfKey[2] = bitsSBox2[2];

                    bytes = new byte[4];
                    bitsOfKey.CopyTo(bytes, 0);

                    outputBlock = BitConverter.ToInt32(bytes, 0);

                    //Result
                    keyRecoveryConfiguration.subkey3 = (outputBlock);
                    keyRecoveryConfiguration.recoveredSubkey3 = true;

                    //Result
                    //keyRecoveryConfiguration.subkey3 = (resultRound2SBox4.PossibleKey ^ resultRound2SBox3.PossibleKey ^ resultRound2SBox2.PossibleKey ^ resultRound2SBox1.PossibleKey);
                    //keyRecoveryConfiguration.recoveredSubkey3 = true;
                    //Console.WriteLine(keyRecoveryConfiguration.printRecoveredSubkeyBits());

                    try
                    {
                        DifferentialAttackLastRoundResult lastRoundResult = analysis.AttackFirstRound(keyRecoveryConfiguration, encryption);
                        keyRecoveryConfiguration.LastRoundResult = lastRoundResult;
                        keyRecoveryConfiguration.recoveredSubkey2 = true;
                        keyRecoveryConfiguration.subkey2 = lastRoundResult.SubKey1;
                        keyRecoveryConfiguration.recoveredSubkey1 = true;
                        keyRecoveryConfiguration.subkey1 = lastRoundResult.SubKey0;
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

                Console.WriteLine("Ran tests: " + testCount + " success-rate: " + successCounter + " / " + testCount + " failure-rate: " + failureCounter + " / " + testCount + " with " + pairCount[j] + " pairs.");
            }


        }



    }
}
