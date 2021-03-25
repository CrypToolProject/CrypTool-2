using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Interfaces;

namespace HeysToyCipher
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Heys ToyCipher Tutorial Cipher");

            AttackAllRounds();

            Console.WriteLine("Press any key to exit...");
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


            bool[] attackSBox4AndSBox2 = new bool[] { false, true, false, true };
            bool[] attackSBox3AndSBox1 = new bool[] { true, false, true, false };

            stopWatch.Start();

            //Check the attack new
            HeysToyCipherKeyRecoveryAttack keyRecoveryConfiguration = new HeysToyCipherKeyRecoveryAttack();

            SearchPolicy curSearchPolicy = SearchPolicy.FirstBestCharacteristicDepthSearch;
            AbortingPolicy curAbortingPolicy = AbortingPolicy.Threshold;

            /*  */
            //attack round 4 SBox3 and SBox1
            DifferentialAttackRoundConfiguration configRound4SBox3AndSBox1 = analysis.GenerateConfigurationAttack(4, attackSBox3AndSBox1, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
            DifferentialAttackRoundResult resultRound4SBox3AndSBox1 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configRound4SBox3AndSBox1, encryption);
            keyRecoveryConfiguration.RoundConfigurations.Add(configRound4SBox3AndSBox1);
            keyRecoveryConfiguration.RoundResults.Add(resultRound4SBox3AndSBox1);

            //attack round 4 SBox4 and SBox2
            DifferentialAttackRoundConfiguration configRound4SBox4AndSBox2 = analysis.GenerateConfigurationAttack(4, attackSBox4AndSBox2, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
            DifferentialAttackRoundResult resultRound4SBox4AndSBox2 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configRound4SBox4AndSBox2, encryption);
            keyRecoveryConfiguration.RoundConfigurations.Add(configRound4SBox4AndSBox2);
            keyRecoveryConfiguration.RoundResults.Add(resultRound4SBox4AndSBox2);

            //Result
            keyRecoveryConfiguration.subkey5 = resultRound4SBox3AndSBox1.PossibleKey ^ resultRound4SBox4AndSBox2.PossibleKey;
            keyRecoveryConfiguration.recoveredSubkey5 = true;
            Console.WriteLine(keyRecoveryConfiguration.printRecoveredSubkeyBits());


            //attack round 3 SBox3 and SBox1
            DifferentialAttackRoundConfiguration configRound3SBox3AndSBox1 = analysis.GenerateConfigurationAttack(3, attackSBox3AndSBox1, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
            DifferentialAttackRoundResult resultRound3SBox3AndSBox1 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configRound3SBox3AndSBox1, encryption);
            keyRecoveryConfiguration.RoundConfigurations.Add(configRound3SBox3AndSBox1);
            keyRecoveryConfiguration.RoundResults.Add(resultRound3SBox3AndSBox1);

            //attack round 3 SBox4 and SBox2
            DifferentialAttackRoundConfiguration configRound3SBox4AndSBox2 = analysis.GenerateConfigurationAttack(3, attackSBox4AndSBox2, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
            DifferentialAttackRoundResult resultRound3SBox4AndSBox2 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configRound3SBox4AndSBox2, encryption);
            keyRecoveryConfiguration.RoundConfigurations.Add(configRound3SBox4AndSBox2);
            keyRecoveryConfiguration.RoundResults.Add(resultRound3SBox4AndSBox2);

            //Result
            keyRecoveryConfiguration.subkey4 = resultRound3SBox4AndSBox2.PossibleKey ^ resultRound3SBox3AndSBox1.PossibleKey;
            keyRecoveryConfiguration.recoveredSubkey4 = true;
            Console.WriteLine(keyRecoveryConfiguration.printRecoveredSubkeyBits());


            //attack round 2 SBox3 and SBox1
            DifferentialAttackRoundConfiguration configRound2SBox3AndSBox1 = analysis.GenerateConfigurationAttack(2, attackSBox3AndSBox1, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
            DifferentialAttackRoundResult resultRound2SBox3AndSBox1 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configRound2SBox3AndSBox1, encryption);
            keyRecoveryConfiguration.RoundConfigurations.Add(configRound2SBox3AndSBox1);
            keyRecoveryConfiguration.RoundResults.Add(resultRound2SBox3AndSBox1);

            //attack round 2 SBox4 and SBox2
            DifferentialAttackRoundConfiguration configRound2SBox4AndSBox2 = analysis.GenerateConfigurationAttack(2, attackSBox4AndSBox2, curAbortingPolicy, curSearchPolicy, diffList, keyRecoveryConfiguration, encryption);
            DifferentialAttackRoundResult resultRound2SBox4AndSBox2 = analysis.RecoverKeyInformation(keyRecoveryConfiguration, configRound2SBox4AndSBox2, encryption);
            keyRecoveryConfiguration.RoundConfigurations.Add(configRound2SBox4AndSBox2);
            keyRecoveryConfiguration.RoundResults.Add(resultRound2SBox4AndSBox2);

            //Result
            keyRecoveryConfiguration.subkey3 = resultRound2SBox4AndSBox2.PossibleKey ^ resultRound2SBox3AndSBox1.PossibleKey;
            keyRecoveryConfiguration.recoveredSubkey3 = true;
            Console.WriteLine(keyRecoveryConfiguration.printRecoveredSubkeyBits());


            Console.WriteLine("Key to attack in round 1: " + encryption.PrintKeyBits(1));
            Console.WriteLine("Key to attack in round 1: " + encryption.PrintKeyBits(0));

            DifferentialAttackLastRoundResult lastRoundResult = analysis.AttackFirstRound(keyRecoveryConfiguration, encryption);
            keyRecoveryConfiguration.LastRoundResult = lastRoundResult;

            keyRecoveryConfiguration.recoveredSubkey2 = true;
            keyRecoveryConfiguration.subkey2 = lastRoundResult.SubKey1;
            keyRecoveryConfiguration.recoveredSubkey1 = true;
            keyRecoveryConfiguration.subkey1 = lastRoundResult.SubKey0;
            Console.WriteLine(keyRecoveryConfiguration.printRecoveredSubkeyBits());
        }
    }
}
