using Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TBCCipherFourCharCount
{
    class Program
    {
        static void Main(string[] args)
        {
            Analysis analysis = new Analysis();

            //analyse the sbox
            List<SBoxCharacteristic> diffList = analysis.CountDifferentialsSingleSBox();

            bool[] attackSBox = new bool[] { true, true, true, true };

            DifferentialAttackRoundConfiguration conf = new DifferentialAttackRoundConfiguration
            {
                ActiveSBoxes = attackSBox,
                Round = 3,
                AbortingPolicy = AbortingPolicy.GlobalMaximum,
                SearchPolicy = SearchPolicy.FirstAllCharacteristicsDepthSearch
            };
            
            Console.WriteLine("Running calc for round: " + conf.Round);
            Stopwatch sw = new Stopwatch();
            BigInteger res = analysis.countCharacteristics(conf, diffList);
            Console.WriteLine("Calculation finished with " + conf.Round + " rounds. Result: " + res);
            sw.Stop();
            int hours = sw.Elapsed.Hours;
            Console.WriteLine("Calculation finished with " + conf.Round + " rounds. Result: " + res + " in " + hours + " hours");
            using (StreamWriter writer = File.AppendText("Results.txt"))
            {
                writer.WriteLine("Calculation finished with " + conf.Round + " rounds. Result: " + res + " in " + hours + " hours");
                //writer.WriteLine("Throughput = " + res / seconds + " Characteristics / second");
                writer.WriteLine("----------------");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
