using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace _3Round16BitSPN
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("3Round16Bit SPN");

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        static void CountTestedKeys()
        {
            Encryption encryption = new Encryption();
            Analysis analysis = new Analysis();

            //analyse the sbox
            List<SBoxCharacteristic> diffList = analysis.CountDifferentialsSingleSBox();

            int testCount = 1000;

            int pairCounter = 0;
            int pairs = 1000;

            string fileName = "Cipher2_Full_TestedKeys.txt";

            //to change way of pair-generation, edit GenerateInputPairList() method in Analysis-Class
            for (int i = 0; i < testCount; i++)
            {
                encryption.GenerateRandomKeys();
                bool success = true;

                _3Round16BitSPNKeyRecoveryAttack keyRecoveryConfiguration = new _3Round16BitSPNKeyRecoveryAttack();

                //attack round 3 sbox4
                DifferentialAttackRoundConfiguration configRound3SBox2SBox3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox2SBox3SBox4.json");
                configRound3SBox2SBox3SBox4 = analysis.RefreshPairLists(configRound3SBox2SBox3SBox4, diffList, keyRecoveryConfiguration, encryption, pairs);
                DifferentialAttackRoundResult resultRound3SBox2SBox3SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound3SBox2SBox3SBox4, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound3SBox2SBox3SBox4);
                keyRecoveryConfiguration.RoundResults.Add(resultRound3SBox2SBox3SBox4);
                pairCounter += 4096;

                //attack round 3 sbox3
                DifferentialAttackRoundConfiguration configRound3SBox1SBox2SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox1SBox2SBox3.json");
                configRound3SBox1SBox2SBox3 = analysis.RefreshPairLists(configRound3SBox1SBox2SBox3, diffList, keyRecoveryConfiguration, encryption, pairs);
                DifferentialAttackRoundResult resultRound3SBox1SBox2SBox3 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound3SBox1SBox2SBox3, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound3SBox1SBox2SBox3);
                keyRecoveryConfiguration.RoundResults.Add(resultRound3SBox1SBox2SBox3);
                pairCounter += 4096;

                //Result
                keyRecoveryConfiguration.subkey3 = analysis.MergeBytes(resultRound3SBox1SBox2SBox3.PossibleKey, resultRound3SBox1SBox2SBox3.PossibleKey, resultRound3SBox1SBox2SBox3.PossibleKey, resultRound3SBox2SBox3SBox4.PossibleKey);
                keyRecoveryConfiguration.recoveredSubkey3 = true;


                //attack round 2 sbox4
                DifferentialAttackRoundConfiguration configRound2SBox2SBox3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound2SBox2SBox3SBox4.json");
                configRound2SBox2SBox3SBox4 = analysis.RefreshPairLists(configRound2SBox2SBox3SBox4, diffList, keyRecoveryConfiguration, encryption, pairs);
                DifferentialAttackRoundResult resultRound2SBox2SBox3SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound2SBox2SBox3SBox4, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound2SBox2SBox3SBox4);
                keyRecoveryConfiguration.RoundResults.Add(resultRound2SBox2SBox3SBox4);
                pairCounter += 4096;

                //attack round 2 sbox3
                DifferentialAttackRoundConfiguration configRound2SBox1SBox2SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound2SBox1SBox2SBox3.json");
                configRound2SBox1SBox2SBox3 = analysis.RefreshPairLists(configRound2SBox1SBox2SBox3, diffList, keyRecoveryConfiguration, encryption, pairs);
                DifferentialAttackRoundResult resultRound2SBox1SBox2SBox3 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound2SBox1SBox2SBox3, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound2SBox1SBox2SBox3);
                keyRecoveryConfiguration.RoundResults.Add(resultRound2SBox1SBox2SBox3);
                pairCounter += 4096;

                BitArray bitsZeroToThree = new BitArray(BitConverter.GetBytes(resultRound2SBox2SBox3SBox4.PossibleKey));
                BitArray bitsResult = new BitArray(BitConverter.GetBytes(resultRound2SBox1SBox2SBox3.PossibleKey));

                bitsResult[12] = bitsZeroToThree[12];
                bitsResult[9] = bitsZeroToThree[9];
                bitsResult[6] = bitsZeroToThree[6];
                bitsResult[3] = bitsZeroToThree[3];

                byte[] bytes = new byte[4];
                bitsResult.CopyTo(bytes, 0);

                int outputBlock = BitConverter.ToInt32(bytes, 0);

                //Result
                keyRecoveryConfiguration.subkey2 = (outputBlock);
                keyRecoveryConfiguration.recoveredSubkey2 = true;

                try
                {
                    DifferentialAttackLastRoundResult lastRoundResult = analysis.AttackFirstRound(keyRecoveryConfiguration, encryption);
                    keyRecoveryConfiguration.LastRoundResult = lastRoundResult;
                    keyRecoveryConfiguration.recoveredSubkey1 = true;
                    keyRecoveryConfiguration.subkey1 = lastRoundResult.SubKey1;
                    keyRecoveryConfiguration.recoveredSubkey0 = true;
                    keyRecoveryConfiguration.subkey0 = lastRoundResult.SubKey0;
                    pairCounter += lastRoundResult.testedKeys;
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
                sw.WriteLine("" + (pairCounter / testCount));
            }

        }

        static void BenchmarkTimeKeyRecoveryDifferentCountOfSubKeys()
        {
            Encryption encryption = new Encryption();
            Analysis analysis = new Analysis();

            Stopwatch stopWatch = new Stopwatch();

            //analyse the sbox
            List<SBoxCharacteristic> diffList = analysis.CountDifferentialsSingleSBox();

            // 

            int testCount = 1000;
            int[] pairCount = new[] { 5000, 10000, 15000, 20000 };

            for (int j = 0; j < pairCount.Length; j++)
            {
                double[] times = new double[16];

                for (int i = 0; i < testCount; i++)
                {
                    encryption.GenerateRandomKeys();

                    _3Round16BitSPNKeyRecoveryAttack keyRecoveryConfiguration = new _3Round16BitSPNKeyRecoveryAttack();

                    //test filter  round 3sbox4: 0001
                    DifferentialAttackRoundConfiguration configround3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox4.json");
                    configround3SBox4 = analysis.RefreshPairLists(configround3SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configround3SBox4.FilteredPairList = analysis.FilterPairs(configround3SBox4, diffList, keyRecoveryConfiguration, encryption, configround3SBox4.ExpectedDifference);
                    DifferentialAttackRoundResult resultround3SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configround3SBox4, encryption);
                    stopWatch.Stop();
                    times[1] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();

                    //test filter  round 3sbox3 0010
                    DifferentialAttackRoundConfiguration configround3SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox3.json");
                    configround3SBox3 = analysis.RefreshPairLists(configround3SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configround3SBox3.FilteredPairList = analysis.FilterPairs(configround3SBox3, diffList, keyRecoveryConfiguration, encryption, configround3SBox3.ExpectedDifference);
                    DifferentialAttackRoundResult resultround3SBox3 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configround3SBox3, encryption);
                    stopWatch.Stop();
                    times[2] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();

                    //test filter  round 3sbox2 0100
                    DifferentialAttackRoundConfiguration configround3SBox2 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox2.json");
                    configround3SBox2 = analysis.RefreshPairLists(configround3SBox2, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configround3SBox2.FilteredPairList = analysis.FilterPairs(configround3SBox2, diffList, keyRecoveryConfiguration, encryption, configround3SBox2.ExpectedDifference);
                    DifferentialAttackRoundResult resultround3SBox2 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configround3SBox2, encryption);
                    stopWatch.Stop();
                    times[4] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();

                    //test filter  round 3sbox1 1000
                    DifferentialAttackRoundConfiguration configround3SBox1 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox1.json");
                    configround3SBox1 = analysis.RefreshPairLists(configround3SBox1, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configround3SBox1.FilteredPairList = analysis.FilterPairs(configround3SBox1, diffList, keyRecoveryConfiguration, encryption, configround3SBox1.ExpectedDifference);
                    DifferentialAttackRoundResult resultround3SBox1 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configround3SBox1, encryption);
                    stopWatch.Stop();
                    times[8] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();


                    //test filter  round 3sbox3 and sbox4 0011
                    DifferentialAttackRoundConfiguration configround3SBox3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox3SBox4.json");
                    configround3SBox3SBox4 = analysis.RefreshPairLists(configround3SBox3SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configround3SBox3SBox4.FilteredPairList = analysis.FilterPairs(configround3SBox3SBox4, diffList, keyRecoveryConfiguration, encryption, configround3SBox3SBox4.ExpectedDifference);
                    DifferentialAttackRoundResult resultround3SBox3SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configround3SBox3SBox4, encryption);
                    stopWatch.Stop();
                    times[3] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();

                    //test filter  round 3sbox2 and sbox4 0101
                    DifferentialAttackRoundConfiguration configround3SBox2SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox2SBox4.json");
                    configround3SBox2SBox4 = analysis.RefreshPairLists(configround3SBox2SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configround3SBox2SBox4.FilteredPairList = analysis.FilterPairs(configround3SBox2SBox4, diffList, keyRecoveryConfiguration, encryption, configround3SBox2SBox4.ExpectedDifference);
                    DifferentialAttackRoundResult resultround3SBox2SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configround3SBox2SBox4, encryption);
                    stopWatch.Stop();
                    times[5] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();


                    //test filter  round 3sbox1 and sbox4 1001
                    DifferentialAttackRoundConfiguration configround3SBox1SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox1SBox4.json");
                    configround3SBox1SBox4 = analysis.RefreshPairLists(configround3SBox1SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configround3SBox1SBox4.FilteredPairList = analysis.FilterPairs(configround3SBox1SBox4, diffList, keyRecoveryConfiguration, encryption, configround3SBox1SBox4.ExpectedDifference);
                    DifferentialAttackRoundResult resultround3SBox1SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configround3SBox1SBox4, encryption);
                    stopWatch.Stop();
                    times[9] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();


                    //test filter  round 3sbox2 and sbox3 0110
                    DifferentialAttackRoundConfiguration configround3SBox2SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox2SBox3.json");
                    configround3SBox2SBox3 = analysis.RefreshPairLists(configround3SBox2SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configround3SBox2SBox3.FilteredPairList = analysis.FilterPairs(configround3SBox2SBox3, diffList, keyRecoveryConfiguration, encryption, configround3SBox2SBox3.ExpectedDifference);
                    DifferentialAttackRoundResult resultround3SBox2SBox3 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configround3SBox2SBox3, encryption);
                    stopWatch.Stop();
                    times[6] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();


                    //test filter  round 3sbox1 and sbox3 1010
                    DifferentialAttackRoundConfiguration configround3SBox1SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox1SBox3.json");
                    configround3SBox1SBox3 = analysis.RefreshPairLists(configround3SBox1SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configround3SBox1SBox3.FilteredPairList = analysis.FilterPairs(configround3SBox1SBox3, diffList, keyRecoveryConfiguration, encryption, configround3SBox1SBox3.ExpectedDifference);
                    DifferentialAttackRoundResult resultround3SBox1SBox3 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configround3SBox1SBox3, encryption);
                    stopWatch.Stop();
                    times[10] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();


                    //test filter  round 3sbox1 and sbox2 1100
                    DifferentialAttackRoundConfiguration configround3SBox1SBox2 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox1SBox2.json");
                    configround3SBox1SBox2 = analysis.RefreshPairLists(configround3SBox1SBox2, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configround3SBox1SBox2.FilteredPairList = analysis.FilterPairs(configround3SBox1SBox2, diffList, keyRecoveryConfiguration, encryption, configround3SBox1SBox2.ExpectedDifference);
                    DifferentialAttackRoundResult resultround3SBox1SBox2 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configround3SBox1SBox2, encryption);
                    stopWatch.Stop();
                    times[12] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();


                    //test filter  round 3sbox2 and sbox3 and sbox4 0111
                    DifferentialAttackRoundConfiguration configround3SBox2SBox3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox2SBox3SBox4.json");
                    configround3SBox2SBox3SBox4 = analysis.RefreshPairLists(configround3SBox2SBox3SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configround3SBox2SBox3SBox4.FilteredPairList = analysis.FilterPairs(configround3SBox2SBox3SBox4, diffList, keyRecoveryConfiguration, encryption, configround3SBox2SBox3SBox4.ExpectedDifference);
                    DifferentialAttackRoundResult resultround3SBox2SBox3SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configround3SBox2SBox3SBox4, encryption);
                    stopWatch.Stop();
                    times[7] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();


                    //test filter  round 3sbox1 and sbox2 and sbox4 1101
                    DifferentialAttackRoundConfiguration configround3SBox1SBox2SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox1SBox2SBox4.json");
                    configround3SBox1SBox2SBox4 = analysis.RefreshPairLists(configround3SBox1SBox2SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configround3SBox1SBox2SBox4.FilteredPairList = analysis.FilterPairs(configround3SBox1SBox2SBox4, diffList, keyRecoveryConfiguration, encryption, configround3SBox1SBox2SBox4.ExpectedDifference);
                    DifferentialAttackRoundResult resultround3SBox1SBox2SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configround3SBox1SBox2SBox4, encryption);
                    stopWatch.Stop();
                    times[13] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();


                    //test filter  round 3sbox1 and sbox3 and sbox4 1011
                    DifferentialAttackRoundConfiguration configround3SBox1SBox3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox1SBox3SBox4.json");
                    configround3SBox1SBox3SBox4 = analysis.RefreshPairLists(configround3SBox1SBox3SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configround3SBox1SBox3SBox4.FilteredPairList = analysis.FilterPairs(configround3SBox1SBox3SBox4, diffList, keyRecoveryConfiguration, encryption, configround3SBox1SBox3SBox4.ExpectedDifference);
                    DifferentialAttackRoundResult resultround3SBox1SBox3SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configround3SBox1SBox3SBox4, encryption);
                    stopWatch.Stop();
                    times[11] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();


                    //test filter  round 3sbox1 and sbox2 and sbox3 1110
                    DifferentialAttackRoundConfiguration configround3SBox1SBox2SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox1SBox2SBox3.json");
                    configround3SBox1SBox2SBox3 = analysis.RefreshPairLists(configround3SBox1SBox2SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configround3SBox1SBox2SBox3.FilteredPairList = analysis.FilterPairs(configround3SBox1SBox2SBox3, diffList, keyRecoveryConfiguration, encryption, configround3SBox1SBox2SBox3.ExpectedDifference);
                    DifferentialAttackRoundResult resultround3SBox1SBox2SBox3 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configround3SBox1SBox2SBox3, encryption);
                    stopWatch.Stop();
                    times[14] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();


                    //test filter  round 3sbox1 sbox2 and sbox3 and sbox4 1111
                    DifferentialAttackRoundConfiguration configround3SBox1SBox2SBox3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox1SBox2SBox3SBox4.json");
                    configround3SBox1SBox2SBox3SBox4 = analysis.RefreshPairLists(configround3SBox1SBox2SBox3SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configround3SBox1SBox2SBox3SBox4.FilteredPairList = analysis.FilterPairs(configround3SBox1SBox2SBox3SBox4, diffList, keyRecoveryConfiguration, encryption, configround3SBox1SBox2SBox3SBox4.ExpectedDifference);
                    DifferentialAttackRoundResult resultround3SBox1SBox2SBox3SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configround3SBox1SBox2SBox3SBox4, encryption);
                    stopWatch.Stop();
                    times[15] += stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Reset();

                    Console.WriteLine("Iteration " + (i+1) + "/" + testCount + " finished ");
                }

                string fileName = "Cipher2_KeyRecoveryTime_DifferentCountOfKeybits_WithFilter_" + pairCount[j] + ".txt";

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

        static void BenchmarkSuccessRate()
        {
            Encryption encryption = new Encryption();
            Analysis analysis = new Analysis();

            //analyse the sbox
            List<SBoxCharacteristic> diffList = analysis.CountDifferentialsSingleSBox();

            int testCount = 1000;

            //paircount for addition test, because incremented pairs do not work good
            //int[] pairCount = new[] { 2000, 4000, 6000, 8000, 10000, 12000, 14000, 16000, 18000, 20000 };

            //paircount for generell test
            int[] pairCount = new[] { 50, 100, 150, 200, 250, 300, 350, 400, 450, 500, 550, 600, 650, 700, 750, 800, 850, 900, 950, 1000 };
            //int[] pairCount = new[] { 200, 400, 600, 800, 1000, 1200, 1400, 1600, 1800, 2000, 2200, 2400, 2600, 2800, 3000, 3200, 3400, 3600, 3800, 4000 };

            for (int j = 0; j < pairCount.Length; j++)
            {
                int successCounter = 0;
                int failureCounter = 0;

                //string fileName = "Cipher2_SuccessRate_IncrementedPairs.txt";
                string fileName = "Cipher2_SuccessRate_RandomPairs.txt";

                //to change way of pair-generation, edit GenerateInputPairList() method in Analysis-Class
                for (int i = 0; i < testCount; i++)
                {
                    encryption.GenerateRandomKeys();
                    bool success = true;

                    _3Round16BitSPNKeyRecoveryAttack keyRecoveryConfiguration = new _3Round16BitSPNKeyRecoveryAttack();
                    
                    //attack round 3 sbox4
                    DifferentialAttackRoundConfiguration configRound3SBox2SBox3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox2SBox3SBox4.json");
                    configRound3SBox2SBox3SBox4 = analysis.RefreshPairLists(configRound3SBox2SBox3SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    DifferentialAttackRoundResult resultRound3SBox2SBox3SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound3SBox2SBox3SBox4, encryption);
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound3SBox2SBox3SBox4);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound3SBox2SBox3SBox4);

                    //attack round 3 sbox3
                    DifferentialAttackRoundConfiguration configRound3SBox1SBox2SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox1SBox2SBox3.json");
                    configRound3SBox1SBox2SBox3 = analysis.RefreshPairLists(configRound3SBox1SBox2SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    DifferentialAttackRoundResult resultRound3SBox1SBox2SBox3 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound3SBox1SBox2SBox3, encryption);
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound3SBox1SBox2SBox3);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound3SBox1SBox2SBox3);

                    //Result
                    keyRecoveryConfiguration.subkey3 = analysis.MergeBytes(resultRound3SBox1SBox2SBox3.PossibleKey, resultRound3SBox1SBox2SBox3.PossibleKey, resultRound3SBox1SBox2SBox3.PossibleKey, resultRound3SBox2SBox3SBox4.PossibleKey);
                    keyRecoveryConfiguration.recoveredSubkey3 = true;


                    //attack round 2 sbox4
                    DifferentialAttackRoundConfiguration configRound2SBox2SBox3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound2SBox2SBox3SBox4.json");
                    configRound2SBox2SBox3SBox4 = analysis.RefreshPairLists(configRound2SBox2SBox3SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    DifferentialAttackRoundResult resultRound2SBox2SBox3SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound2SBox2SBox3SBox4, encryption);
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound2SBox2SBox3SBox4);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound2SBox2SBox3SBox4);

                    //attack round 2 sbox3
                    DifferentialAttackRoundConfiguration configRound2SBox1SBox2SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound2SBox1SBox2SBox3.json");
                    configRound2SBox1SBox2SBox3 = analysis.RefreshPairLists(configRound2SBox1SBox2SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    DifferentialAttackRoundResult resultRound2SBox1SBox2SBox3 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound2SBox1SBox2SBox3, encryption);
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound2SBox1SBox2SBox3);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound2SBox1SBox2SBox3);

                    BitArray bitsZeroToThree = new BitArray(BitConverter.GetBytes(resultRound2SBox2SBox3SBox4.PossibleKey));
                    BitArray bitsResult = new BitArray(BitConverter.GetBytes(resultRound2SBox1SBox2SBox3.PossibleKey));

                    bitsResult[12] = bitsZeroToThree[12];
                    bitsResult[9] = bitsZeroToThree[9];
                    bitsResult[6] = bitsZeroToThree[6];
                    bitsResult[3] = bitsZeroToThree[3];

                    byte[] bytes = new byte[4];
                    bitsResult.CopyTo(bytes, 0);

                    int outputBlock = BitConverter.ToInt32(bytes, 0);

                    //Result
                    keyRecoveryConfiguration.subkey2 = (outputBlock);
                    keyRecoveryConfiguration.recoveredSubkey2 = true;

                    try
                    {
                        DifferentialAttackLastRoundResult lastRoundResult = analysis.AttackFirstRound(keyRecoveryConfiguration, encryption);
                        keyRecoveryConfiguration.LastRoundResult = lastRoundResult;
                        keyRecoveryConfiguration.recoveredSubkey1 = true;
                        keyRecoveryConfiguration.subkey1 = lastRoundResult.SubKey1;
                        keyRecoveryConfiguration.recoveredSubkey0 = true;
                        keyRecoveryConfiguration.subkey0 = lastRoundResult.SubKey0;
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
                    //sw.WriteLine("" + successCounter + ";" + failureCounter);
                }

                Console.WriteLine("Ran tests: " + testCount + " success-rate: " + successCounter + " / " + testCount + " failure-rate: " + failureCounter + " / " + testCount + " with " + pairCount[j] + " pairs.");
            }


        }

        static void BenchmarkTimeKeyRecoveryWithAndWithoutFilter()
        {
            Encryption encryption = new Encryption();
            Analysis analysis = new Analysis();

            Stopwatch stopWatch = new Stopwatch();

            //analyse the sbox
            List<SBoxCharacteristic> diffList = analysis.CountDifferentialsSingleSBox();

            //

            int testCount = 1000;
            int[] pairCount = new[] { 5000, 10000, 15000, 20000, 25000, 30000, 35000, 40000, 45000, 50000, 55000, 60000, 65536 };

            for (int j = 0; j < pairCount.Length; j++)
            {
                int successCounter = 0;
                int failureCounter = 0;

                TimeSpan ts = new TimeSpan(0, 0, 0);


                string fileName = "Cipher2_KeyRecoveryTime_WithFilter.txt";
                //string fileName = "Cipher2_KeyRecoveryTime_NoFilter.txt";

                for (int i = 0; i < testCount; i++)
                {
                    encryption.GenerateRandomKeys();
                    bool success = true;

                    _3Round16BitSPNKeyRecoveryAttack keyRecoveryConfiguration = new _3Round16BitSPNKeyRecoveryAttack();

                    //attack round 3 sbox4
                    DifferentialAttackRoundConfiguration configRound3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox4.json");
                    configRound3SBox4 = analysis.RefreshPairLists(configRound3SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configRound3SBox4.FilteredPairList = analysis.FilterPairs(configRound3SBox4, diffList, keyRecoveryConfiguration, encryption, configRound3SBox4.ExpectedDifference);
                    DifferentialAttackRoundResult resultRound3SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound3SBox4, encryption);
                    stopWatch.Stop();
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound3SBox4);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound3SBox4);

                    //attack round 3 sbox3
                    DifferentialAttackRoundConfiguration configRound3SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox3.json");
                    configRound3SBox3 = analysis.RefreshPairLists(configRound3SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configRound3SBox3.FilteredPairList = analysis.FilterPairs(configRound3SBox3, diffList, keyRecoveryConfiguration, encryption, configRound3SBox3.ExpectedDifference);
                    DifferentialAttackRoundResult resultRound3SBox3 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound3SBox3, encryption);
                    stopWatch.Stop();
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound3SBox3);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound3SBox3);

                    //attack round 3 sbox2
                    DifferentialAttackRoundConfiguration configRound3SBox2 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox2.json");
                    configRound3SBox2 = analysis.RefreshPairLists(configRound3SBox2, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configRound3SBox2.FilteredPairList = analysis.FilterPairs(configRound3SBox2, diffList, keyRecoveryConfiguration, encryption, configRound3SBox2.ExpectedDifference);
                    DifferentialAttackRoundResult resultRound3SBox2 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound3SBox2, encryption);
                    stopWatch.Stop();
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound3SBox2);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound3SBox2);

                    //attack round 3 sbox1
                    DifferentialAttackRoundConfiguration configRound3SBox1 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox1.json");
                    configRound3SBox1 = analysis.RefreshPairLists(configRound3SBox1, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configRound3SBox1.FilteredPairList = analysis.FilterPairs(configRound3SBox1, diffList, keyRecoveryConfiguration, encryption, configRound3SBox1.ExpectedDifference);
                    DifferentialAttackRoundResult resultRound3SBox1 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound3SBox1, encryption);
                    stopWatch.Stop();
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound3SBox1);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound3SBox1);

                    //Result
                    keyRecoveryConfiguration.subkey3 = analysis.MergeBytes(resultRound3SBox1.PossibleKey, resultRound3SBox2.PossibleKey, resultRound3SBox3.PossibleKey, resultRound3SBox4.PossibleKey);
                    keyRecoveryConfiguration.recoveredSubkey3 = true;


                    //attack round 2 sbox4
                    DifferentialAttackRoundConfiguration configRound2SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound2SBox4.json");
                    configRound2SBox4 = analysis.RefreshPairLists(configRound2SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configRound2SBox4.FilteredPairList = analysis.FilterPairs(configRound2SBox4, diffList, keyRecoveryConfiguration, encryption, configRound2SBox4.ExpectedDifference);
                    DifferentialAttackRoundResult resultRound2SBox4 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound2SBox4, encryption);
                    stopWatch.Stop();
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound2SBox4);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound2SBox4);

                    //attack round 2 sbox3
                    DifferentialAttackRoundConfiguration configRound2SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound2SBox3.json");
                    configRound2SBox3 = analysis.RefreshPairLists(configRound2SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configRound2SBox3.FilteredPairList = analysis.FilterPairs(configRound2SBox3, diffList, keyRecoveryConfiguration, encryption, configRound2SBox3.ExpectedDifference);
                    DifferentialAttackRoundResult resultRound2SBox3 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound2SBox3, encryption);
                    stopWatch.Stop();
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound2SBox3);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound2SBox3);

                    //attack round 2 sbox2
                    DifferentialAttackRoundConfiguration configRound2SBox2 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound2SBox2.json");
                    configRound2SBox2 = analysis.RefreshPairLists(configRound2SBox2, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configRound2SBox2.FilteredPairList = analysis.FilterPairs(configRound2SBox2, diffList, keyRecoveryConfiguration, encryption, configRound2SBox2.ExpectedDifference);
                    DifferentialAttackRoundResult resultRound2SBox2 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound2SBox2, encryption);
                    stopWatch.Stop();
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound2SBox2);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound2SBox2);

                    //attack round 2 sbox1
                    DifferentialAttackRoundConfiguration configRound2SBox1 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound2SBox1.json");
                    configRound2SBox1 = analysis.RefreshPairLists(configRound2SBox1, diffList, keyRecoveryConfiguration, encryption, pairCount[j]);
                    stopWatch.Start();
                    configRound2SBox1.FilteredPairList = analysis.FilterPairs(configRound2SBox1, diffList, keyRecoveryConfiguration, encryption, configRound2SBox1.ExpectedDifference);
                    DifferentialAttackRoundResult resultRound2SBox1 = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRound2SBox1, encryption);
                    stopWatch.Stop();
                    keyRecoveryConfiguration.RoundConfigurations.Add(configRound2SBox1);
                    keyRecoveryConfiguration.RoundResults.Add(resultRound2SBox1);

                    //Result
                    keyRecoveryConfiguration.subkey2 = (resultRound2SBox4.PossibleKey ^ resultRound2SBox3.PossibleKey ^ resultRound2SBox2.PossibleKey ^ resultRound2SBox1.PossibleKey);
                    keyRecoveryConfiguration.recoveredSubkey2 = true;

                    try
                    {
                        stopWatch.Start();
                        DifferentialAttackLastRoundResult lastRoundResult = analysis.AttackFirstRound(keyRecoveryConfiguration, encryption);
                        stopWatch.Stop();
                        keyRecoveryConfiguration.LastRoundResult = lastRoundResult;
                        keyRecoveryConfiguration.recoveredSubkey1 = true;
                        keyRecoveryConfiguration.subkey1 = lastRoundResult.SubKey1;
                        keyRecoveryConfiguration.recoveredSubkey0 = true;
                        keyRecoveryConfiguration.subkey0 = lastRoundResult.SubKey0;
                    }
                    catch (Exception e)
                    {
                        success = false;
                        stopWatch.Stop();
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

                    ts += stopWatch.Elapsed;
                    stopWatch.Reset();

                    Console.WriteLine("Finished iteration " + (i + 1) + " / " + testCount + " Pairs: " + pairCount[j]);
                }

                string elapsedTime = "" + (ts.TotalSeconds / testCount);
                Console.WriteLine("Average RunTime " + elapsedTime + " seconds. Success: " + successCounter + " Failure: " + failureCounter);

                using (StreamWriter sw = File.AppendText(fileName))
                {
                    //sw.WriteLine(elapsedTime);
                }
            }
        }

        static void BenchmarkFrequencyDistribution()
        {
            for (int i = 0; i < 1000; i++)
            {
                Encryption encryption = new Encryption();
                Analysis analysis = new Analysis();

                List<SBoxCharacteristic> diffList = analysis.CountDifferentialsSingleSBox();

                bool[] SBoxToAttack = new bool[] { false, true, false, false };

                _3Round16BitSPNKeyRecoveryAttack keyRecoveryConfiguration = new _3Round16BitSPNKeyRecoveryAttack();

                AbortingPolicy ap = AbortingPolicy.Threshold;
                SearchPolicy sp = SearchPolicy.FirstBestCharacteristicDepthSearch;

                DifferentialAttackRoundConfiguration configRoundSBox = analysis.GenerateConfigurationAttack(3, SBoxToAttack, ap, sp, diffList, keyRecoveryConfiguration, encryption);

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

                /* */
                //attack with 100 pairs without filter
                configRoundSBox.FilteredPairList = unfilteredList.GetRange(0, 100).ToList();
                Console.WriteLine("Pairs to check: " + configRoundSBox.FilteredPairList.Count);
                DifferentialAttackRoundResult result = analysis.RecoverKeyInformationBinary(keyRecoveryConfiguration, configRoundSBox, encryption);
                foreach (var curKeyProb in result.KeyCandidateProbabilities.OrderBy(var => var.Key))
                {
                    using (StreamWriter sw = File.AppendText("Cipher2_100_Pairs_NoFilter.txt"))
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
                    using (StreamWriter sw = File.AppendText("Cipher2_100_Pairs_Filter.txt"))
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
                    using (StreamWriter sw = File.AppendText("Cipher2_200_Pairs_NoFilter.txt"))
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
                    using (StreamWriter sw = File.AppendText("Cipher2_200_Pairs_Filter.txt"))
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
                    using (StreamWriter sw = File.AppendText("Cipher2_400_Pairs_NoFilter.txt"))
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
                    using (StreamWriter sw = File.AppendText("Cipher2_400_Pairs_Filter.txt"))
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
                    using (StreamWriter sw = File.AppendText("Cipher2_500_Pairs_NoFilter.txt"))
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
                    using (StreamWriter sw = File.AppendText("Cipher2_500_Pairs_Filter.txt"))
                    {
                        sw.WriteLine(curKeyProb.Counter);
                    }
                }
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
                _3Round16BitSPNKeyRecoveryAttack keyRecoveryConfiguration = new _3Round16BitSPNKeyRecoveryAttack();

                bool[] attackSBox4 = new bool[] { true, false, false, false };
                bool[] attackSBox3 = new bool[] { false, true, false, false };
                bool[] attackSBox2 = new bool[] { false, false, true, false };
                bool[] attackSBox1 = new bool[] { false, false, false, true };

                bool[] attackSBox4AndSBox2 = new bool[] { true, true, true, false };
                bool[] attackSBox3AndSBox1 = new bool[] { false, true, false, true };

                bool[] attackSBox4AndSBox3AndSBox2AndSBox1 = new bool[] { true, true, true, true };

                SearchPolicy curSearchPolicy = SearchPolicy.FirstBestCharacteristicDepthSearch;
                AbortingPolicy curAbortingPolicy = AbortingPolicy.Threshold;

                Console.WriteLine(encryption.PrintKeys());

                /* 
                //attack  round 3sbox4
                DifferentialAttackRoundConfiguration configround3SBox4 = analysis.GenerateConfigurationAttack(3, attackSBox4, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
                DifferentialAttackRoundResult resultround3SBox4 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configround3SBox4, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configround3SBox4);
                keyRecoveryConfiguration.RoundResults.Add(resultround3SBox4);

                //attack  round 3sbox3
                DifferentialAttackRoundConfiguration configround3SBox3 = analysis.GenerateConfigurationAttack(3, attackSBox3, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
                DifferentialAttackRoundResult resultround3SBox3 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configround3SBox3, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configround3SBox3);
                keyRecoveryConfiguration.RoundResults.Add(resultround3SBox3);

                //attack  round 3sbox2
                DifferentialAttackRoundConfiguration configround3SBox2 = analysis.GenerateConfigurationAttack(3, attackSBox2, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
                DifferentialAttackRoundResult resultround3SBox2 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configround3SBox2, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configround3SBox2);
                keyRecoveryConfiguration.RoundResults.Add(resultround3SBox2);

                //attack  round 3sbox1
                DifferentialAttackRoundConfiguration configround3SBox1 = analysis.GenerateConfigurationAttack(3, attackSBox1, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
                DifferentialAttackRoundResult resultround3SBox1 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configround3SBox1, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configround3SBox1);
                keyRecoveryConfiguration.RoundResults.Add(resultround3SBox1);

                //Result
                keyRecoveryConfiguration.subkey3 = analysis.MergeBytes(resultround3SBox1.PossibleKey, resultround3SBox2.PossibleKey, resultround3SBox3.PossibleKey, resultround3SBox4.PossibleKey);
                keyRecoveryConfiguration.recoveredSubkey3 = true;
                Console.WriteLine(keyRecoveryConfiguration.printRecoveredSubkeyBits());
                */


                /* */
                keyRecoveryConfiguration.recoveredSubkey3 = true;
                keyRecoveryConfiguration.subkey3 = encryption._keys[3];
                

                /* */

                //attack round 4 sbox4
                DifferentialAttackRoundConfiguration configRound4SBox4 = analysis.GenerateConfigurationAttack(2, attackSBox4, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
                DifferentialAttackRoundResult resultRound4SBox4 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configRound4SBox4, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound4SBox4);
                keyRecoveryConfiguration.RoundResults.Add(resultRound4SBox4);

                //attack round 4 sbox3
                DifferentialAttackRoundConfiguration configRound4SBox3 = analysis.GenerateConfigurationAttack(2, attackSBox3, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
                DifferentialAttackRoundResult resultRound4SBox3 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configRound4SBox3, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound4SBox3);
                keyRecoveryConfiguration.RoundResults.Add(resultRound4SBox3);

                //attack round 4 sbox2
                DifferentialAttackRoundConfiguration configRound4SBox2 = analysis.GenerateConfigurationAttack(2, attackSBox2, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
                DifferentialAttackRoundResult resultRound4SBox2 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configRound4SBox2, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound4SBox2);
                keyRecoveryConfiguration.RoundResults.Add(resultRound4SBox2);

                //attack round 4 sbox1
                DifferentialAttackRoundConfiguration configRound4SBox1 = analysis.GenerateConfigurationAttack(2, attackSBox1, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
                DifferentialAttackRoundResult resultRound4SBox1 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configRound4SBox1, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound4SBox1);
                keyRecoveryConfiguration.RoundResults.Add(resultRound4SBox1);

                //Result
                keyRecoveryConfiguration.subkey2 = (resultRound4SBox4.PossibleKey ^ resultRound4SBox3.PossibleKey ^ resultRound4SBox2.PossibleKey ^ resultRound4SBox1.PossibleKey);
                keyRecoveryConfiguration.recoveredSubkey2 = true;
                Console.WriteLine(keyRecoveryConfiguration.printRecoveredSubkeyBits());



                /* 
                keyRecoveryConfiguration.recoveredSubkey5 = true;
                keyRecoveryConfiguration.subkey5 = encryption._keys[4];

                Console.WriteLine("Real key bits: " + encryption.PrintKeyBits(4));
                Console.WriteLine(keyRecoveryConfiguration.printRecoveredSubkeyBits());
                */

                

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                Console.WriteLine("RunTime " + elapsedTime);
                stopWatch.Reset();
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

            string fileName = "Cipher2_Filter_R3_Differentials.txt";

            for (int i = 0; i < testCount; i++)
            {
                encryption.GenerateRandomKeys();

                _3Round16BitSPNKeyRecoveryAttack keyRecoveryConfiguration = new _3Round16BitSPNKeyRecoveryAttack();

                //test filter  round 3sbox4: 0001
                DifferentialAttackRoundConfiguration configRound3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox4.json");
                configRound3SBox4 = analysis.RefreshPairLists(configRound3SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[1] += configRound3SBox4.FilteredPairList.Count;
                probabilities[1] = configRound3SBox4.Probability;

                Console.WriteLine("Unfiltered Count: " + configRound3SBox4.UnfilteredPairList.Count +
                                  " Filtered Count: " + configRound3SBox4.FilteredPairList.Count);

                /*
                Pair p = configround3SBox4.UnfilteredPairList[0];
                Console.WriteLine("Left: " + p.LeftMember + " Right: " + p.RightMember);
                foreach (var pair in configround3SBox4.UnfilteredPairList)
                {
                    if (pair.LeftMember == p.LeftMember || pair.LeftMember == p.RightMember)
                    {
                        Console.WriteLine("Left: " + pair.LeftMember + " Right: " + pair.RightMember);
                    }
                }
                */

                //test filter  round 3sbox3 0010
                DifferentialAttackRoundConfiguration configRound3SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox3.json");
                configRound3SBox3 = analysis.RefreshPairLists(configRound3SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[2] += configRound3SBox3.FilteredPairList.Count;
                probabilities[2] = configRound3SBox3.Probability;

                //test filter  round 3sbox2 0100
                DifferentialAttackRoundConfiguration configRound3SBox2 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox2.json");
                configRound3SBox2 = analysis.RefreshPairLists(configRound3SBox2, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[4] += configRound3SBox2.FilteredPairList.Count;
                probabilities[4] = configRound3SBox2.Probability;

                //test filter  round 3sbox1 1000
                DifferentialAttackRoundConfiguration configRound3SBox1 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox1.json");
                configRound3SBox1 = analysis.RefreshPairLists(configRound3SBox1, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[8] += configRound3SBox1.FilteredPairList.Count;
                probabilities[8] = configRound3SBox1.Probability;

                //test filter  round 3sbox3 and sbox4 0011
                DifferentialAttackRoundConfiguration configRound3SBox3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox3SBox4.json");
                configRound3SBox3SBox4 = analysis.RefreshPairLists(configRound3SBox3SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[3] += configRound3SBox3SBox4.FilteredPairList.Count;
                probabilities[3] = configRound3SBox3SBox4.Probability;

                //test filter  round 3sbox2 and sbox4 0101
                DifferentialAttackRoundConfiguration configRound3SBox2SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox2SBox4.json");
                configRound3SBox2SBox4 = analysis.RefreshPairLists(configRound3SBox2SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[5] += configRound3SBox2SBox4.FilteredPairList.Count;
                probabilities[5] = configRound3SBox2SBox4.Probability;

                //test filter  round 3sbox1 and sbox4 1001
                DifferentialAttackRoundConfiguration configRound3SBox1SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox1SBox4.json");
                configRound3SBox1SBox4 = analysis.RefreshPairLists(configRound3SBox1SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[9] += configRound3SBox1SBox4.FilteredPairList.Count;
                probabilities[9] = configRound3SBox1SBox4.Probability;

                //test filter  round 3sbox2 and sbox3 0110
                DifferentialAttackRoundConfiguration configRound3SBox2SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox2SBox3.json");
                configRound3SBox2SBox3 = analysis.RefreshPairLists(configRound3SBox2SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[6] += configRound3SBox2SBox3.FilteredPairList.Count;
                probabilities[6] = configRound3SBox2SBox3.Probability;

                //test filter  round 3sbox1 and sbox3 1010
                DifferentialAttackRoundConfiguration configRound3SBox1SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox1SBox3.json");
                configRound3SBox1SBox3 = analysis.RefreshPairLists(configRound3SBox1SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[10] += configRound3SBox1SBox3.FilteredPairList.Count;
                probabilities[10] = configRound3SBox1SBox3.Probability;

                //test filter  round 3sbox1 and sbox2 1100
                DifferentialAttackRoundConfiguration configRound3SBox1SBox2 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox1SBox2.json");
                configRound3SBox1SBox2 = analysis.RefreshPairLists(configRound3SBox1SBox2, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[12] += configRound3SBox1SBox2.FilteredPairList.Count;
                probabilities[12] = configRound3SBox1SBox2.Probability;


                //test filter  round 3sbox2 and sbox3 and sbox4 0111
                DifferentialAttackRoundConfiguration configRound3SBox2SBox3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox2SBox3SBox4.json");
                configRound3SBox2SBox3SBox4 = analysis.RefreshPairLists(configRound3SBox2SBox3SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[7] += configRound3SBox2SBox3SBox4.FilteredPairList.Count;
                probabilities[7] = configRound3SBox2SBox3SBox4.Probability;

                //test filter  round 3sbox1 and sbox2 and sbox4 1101
                DifferentialAttackRoundConfiguration configRound3SBox1SBox2SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox1SBox2SBox4.json");
                configRound3SBox1SBox2SBox4 = analysis.RefreshPairLists(configRound3SBox1SBox2SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[13] += configRound3SBox1SBox2SBox4.FilteredPairList.Count;
                probabilities[13] = configRound3SBox1SBox2SBox4.Probability;

                //test filter  round 3sbox1 and sbox3 and sbox4 1011
                DifferentialAttackRoundConfiguration configRound3SBox1SBox3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox1SBox3SBox4.json");
                configRound3SBox1SBox3SBox4 = analysis.RefreshPairLists(configRound3SBox1SBox3SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[11] += configRound3SBox1SBox3SBox4.FilteredPairList.Count;
                probabilities[11] = configRound3SBox1SBox3SBox4.Probability;

                //test filter  round 3sbox1 and sbox2 and sbox3 1110
                DifferentialAttackRoundConfiguration configRound3SBox1SBox2SBox3 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox1SBox2SBox3.json");
                configRound3SBox1SBox2SBox3 = analysis.RefreshPairLists(configRound3SBox1SBox2SBox3, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[14] += configRound3SBox1SBox2SBox3.FilteredPairList.Count;
                probabilities[14] = configRound3SBox1SBox2SBox3.Probability;


                //test filter  round 3sbox1 sbox2 and sbox3 and sbox4 1111
                DifferentialAttackRoundConfiguration configRound3SBox1SBox2SBox3SBox4 = DataLoader.loadDifferentialAttackRoundConfiguration("configRound3SBox1SBox2SBox3SBox4.json");
                configRound3SBox1SBox2SBox3SBox4 = analysis.RefreshPairLists(configRound3SBox1SBox2SBox3SBox4, diffList, keyRecoveryConfiguration, encryption, pairCount);
                filteredPairs[15] += configRound3SBox1SBox2SBox3SBox4.FilteredPairList.Count;
                probabilities[15] = configRound3SBox1SBox2SBox3SBox4.Probability;
            }

            for (int i = 1; i < filteredPairs.Length; i++)
            {
                Console.WriteLine("SBoxes: " + Convert.ToString(i, 2).PadLeft(4, '0') + " Average survived Pairs: " + (filteredPairs[i] / testCount));
                using (StreamWriter sw = File.AppendText(fileName))
                {
                    sw.WriteLine((filteredPairs[i] / testCount) + ";" + (probabilities[i] * 65536) + ";" + 65536);
                }
            }
        }
    }
}
