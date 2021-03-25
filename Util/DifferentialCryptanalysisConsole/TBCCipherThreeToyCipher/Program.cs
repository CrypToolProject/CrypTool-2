using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;

namespace TBCCipherThreeToyCipher
{
    class Program
    {
        static void Main(string[] args)
        {
            AttackAllRounds();

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        static void AttackAllRounds()
        {
            Encryption encryption = new Encryption();
            Analysis analysis = new Analysis();

            Stopwatch stopWatch = new Stopwatch();

            Console.WriteLine("Current keys: " + encryption.PrintKeys());

            //analysis the sbox
            List<SBoxCharacteristic> diffList = analysis.CountDifferentialsSingleSBox();

            bool[] attackSBox1 = new bool[] { true };

            int success = 0;
            int failure = 0;

            double avg = 0.0;

            stopWatch.Start();

            for (int i = 0; i < 1000; i++)
            {
                encryption.GenerateRandomKeys();

                //Check the attack new
                CipherThreeDifferentialKeyRecoveryAttack keyRecoveryConfiguration = new CipherThreeDifferentialKeyRecoveryAttack();

                SearchPolicy curSearchPolicy = SearchPolicy.FirstBestCharacteristicDepthSearch;
                AbortingPolicy curAbortingPolicy = AbortingPolicy.Threshold;

                //attack round 3
                DifferentialAttackRoundConfiguration configRound3SBox1 = analysis.GenerateConfigurationAttack(3, attackSBox1, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
                DifferentialAttackRoundResult resultRound3SBox1 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configRound3SBox1, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound3SBox1);
                keyRecoveryConfiguration.RoundResults.Add(resultRound3SBox1);

                //Result
                keyRecoveryConfiguration.Subkey3 = resultRound3SBox1.PossibleKey;
                keyRecoveryConfiguration.RecoveredSubkey3 = true;

                DifferentialAttackRoundConfiguration configRound2SBox1 = analysis.GenerateConfigurationAttack(2, attackSBox1, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
                DifferentialAttackRoundResult resultRound2SBox1 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configRound2SBox1, encryption);
                keyRecoveryConfiguration.RoundConfigurations.Add(configRound2SBox1);
                keyRecoveryConfiguration.RoundResults.Add(resultRound2SBox1);

                //Result
                keyRecoveryConfiguration.Subkey2 = resultRound2SBox1.PossibleKey;
                keyRecoveryConfiguration.RecoveredSubkey2 = true;

                try
                {
                    DifferentialAttackLastRoundResult lastRoundResult =
                        analysis.AttackFirstRound(keyRecoveryConfiguration, encryption);
                    keyRecoveryConfiguration.LastRoundResult = lastRoundResult;

                    keyRecoveryConfiguration.RecoveredSubkey1 = true;
                    keyRecoveryConfiguration.Subkey1 = lastRoundResult.SubKey1;
                    keyRecoveryConfiguration.RecoveredSubkey0 = true;
                    keyRecoveryConfiguration.Subkey0 = lastRoundResult.SubKey0;

                    Console.WriteLine("Success");
                    success++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Not recovered");
                    failure++;
                }
            }

            Console.WriteLine("Success: " + success + " Failure: " + failure);

            using (StreamWriter sw = File.AppendText("Cipher4_SuccessRate.txt"))
            {
                sw.WriteLine("" + success + ";" + failure);
            }

            /*
            DifferentialAttackRoundConfiguration configRound3SBox2 = analysis.GenerateConfigurationAttack(3, attackSBox2, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
            DifferentialAttackRoundResult resultRound3SBox2 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configRound3SBox2, encryption);
            keyRecoveryConfiguration.RoundConfigurations.Add(configRound3SBox2);
            keyRecoveryConfiguration.RoundResults.Add(resultRound3SBox2);
            */



            //24.05 Masterarbeit besprechen

            /*

            Console.WriteLine("Key to attack in round 1: " + encryption.PrintKeyBits(1));
            Console.WriteLine("Key to attack in round 1: " + encryption.PrintKeyBits(0));

            DifferentialAttackLastRoundResult lastRoundResult = analysis.AttackFirstRound(keyRecoveryConfiguration, encryption);
            keyRecoveryConfiguration.LastRoundResult = lastRoundResult;

            keyRecoveryConfiguration.RecoveredSubkey1 = true;
            keyRecoveryConfiguration.Subkey1 = lastRoundResult.SubKey1;
            keyRecoveryConfiguration.RecoveredSubkey0 = true;
            keyRecoveryConfiguration.Subkey0 = lastRoundResult.SubKey0;
            Console.WriteLine(keyRecoveryConfiguration.printRecoveredSubkeyBits());
            */
        }
    }
}
