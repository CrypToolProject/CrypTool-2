/*
   Copyright CrypTool 2 Team josef.matwich@gmail.com

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
using M209AnalyzerLib.Common;
using M209AnalyzerLib.Enums;
using System;
using System.Collections.Generic;

namespace M209AnalyzerLib.M209
{
    public class M209Scoring : IScoring
    {
        private double increment = 10_000.0;
        public Stats Stats;
        public M209Scoring(Stats stats)
        {
            Stats = stats;
        }
        public double Evaluate(EvalType evalType, int[] decryptedText, int[] crib)
        {
            double evalValue = 0.0;
            switch (evalType)
            {
                case EvalType.CRIB:
                    if (crib == null)
                    {
                        throw new Exception("Crib not given");
                    }
                    evalValue = EvalCrib(decryptedText, crib);
                    break;
                case EvalType.MONO:
                    evalValue = EvalMono(decryptedText);
                    break;
                case EvalType.PINS_SA_CRIB:
                    break;
                default:
                    break;
            }
            return evalValue;
        }
        private double EvalMono(int[] decryptedText)
        {
            int[] decryptionFrequency = CountDecryptionFrequency(decryptedText);
            List<int> monos = new List<int>();
            List<int> monof = new List<int>();

            int mono = 0;
            int fSum = 0;
            for (int i = 0; i < 26; i++)
            {
                int f = decryptionFrequency[i];
                mono += (int)Stats.MonogramStats[i] * f;
                monos.Add((int)Stats.MonogramStats[i] * f);
                monof.Add(f);
            }
            double test = mono / decryptedText.Length;
            if (test > 200_000.0)
            {
                Console.WriteLine(monos);
                Console.WriteLine(test);
                Console.WriteLine(fSum);
            }
            return test;
        }

        private double EvalCrib(int[] decryptedText, int[] crib)
        {
            int sumScore = 0;
            int missing = 0;

            int actual;
            int expected;

            for (int cribIndex = 0; cribIndex < crib.Length; cribIndex++)
            {
                expected = crib[cribIndex];
                if (expected == -1)
                {
                    missing++;
                    continue;
                }
                actual = decryptedText[cribIndex];

                // This is used instead of Math.Abs(), because of performance issues.
                int dist1 = expected - actual;
                dist1 = (dist1 ^ (dist1 >> 31)) - (dist1 >> 31);
                int dist2 = expected + 26 - actual;
                dist2 = (dist2 ^ (dist2 >> 31)) - (dist2 >> 31);

                sumScore += 26 - Math.Min(dist1, dist2);
            }
            double result = 5000 * sumScore / (crib.Length - missing);
            return result;
        }

        private int[] CountDecryptionFrequency(int[] cipherIntText)
        {
            int[] decryptionFrequency = new int[26];

            for (int i = 0; i < cipherIntText.Length; i++)
            {
                int symbol = cipherIntText[i];

                decryptionFrequency[symbol]++;
            }
            return decryptionFrequency;
        }

    }
}
