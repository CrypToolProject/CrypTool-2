/*
   Copyright 2023 Nils Kopal <Nils.Kopal<at>CrypTool.org

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

namespace CrypTool.LorenzSZ42.SZ42Machine
{
    public class SZ42KeyGenerator
    {
        /// <summary>
        /// Generates a key according to the rules
        /// </summary>
        /// <returns></returns>
        public static string GenerateKey()
        {
            SZ42Machine sz42 = new SZ42Machine();
            Random random = new Random();

            //Step 1: Generate mu wheels

            //Mu 1:            
            GenerateMu1Wheel(sz42.MuWheels[0], random);

            //Mu 2:            
            GenerateMu2Wheel(sz42.MuWheels[1], random);

            //Step 2: Generate chi wheels
            for (int i = 0; i < sz42.ChiWheels.Length; i++)
            {
                GenerateChiWheel(sz42.ChiWheels[i], i, random);
            }

            //Step3: Generate psi wheels (psi rule is based on number of dots in wheel Mu2)
            PsiRule psiRule = SZ42KeyRulesChecker.GetPsiRule(sz42.MuWheels[1].GetDotCount());
            for (int i = 0; i < sz42.PsiWheels.Length; i++)
            {
                GeneratePsiWheel(sz42.PsiWheels[i], i, psiRule, random);
            }

            //Step 4: Generate start positions
            foreach (SZ42Wheel wheel in sz42.PsiWheels)
            {
                wheel.Position = random.Next(wheel.Pins.Length);
            }
            foreach (SZ42Wheel wheel in sz42.ChiWheels)
            {
                wheel.Position = random.Next(wheel.Pins.Length);
            }
            foreach (SZ42Wheel wheel in sz42.MuWheels)
            {
                wheel.Position = random.Next(wheel.Pins.Length);
            }

            return sz42.ToString();
        }

        /// <summary>
        /// Generate wheel mu1 according to rules
        /// </summary>
        /// <param name="mu1wheel"></param>
        /// <param name="random"></param>
        private static void GenerateMu1Wheel(SZ42Wheel mu1wheel, Random random)
        {
            do
            {
                int crosses = 30 + random.Next(20);
                for (int position = 0; position < mu1wheel.Pins.Length; position++)
                {
                    mu1wheel.Pins[position] = SZ42Wheel.INACTIVE_PIN;
                }
                do
                {
                    int position = random.Next(mu1wheel.Pins.Length);
                    mu1wheel.Pins[position] = SZ42Wheel.ACTIVE_PIN;
                } while (mu1wheel.GetCrossCount() != crosses);
            } while (SZ42KeyRulesChecker.CheckMu1WheelRules(mu1wheel) == false);
        }

        /// <summary>
        /// Generate wheel mu2 according to rules
        /// </summary>
        /// <param name="mu2wheel"></param>
        /// <param name="random"></param>
        private static void GenerateMu2Wheel(SZ42Wheel mu2Wheel, Random random)
        {
            do
            {
                int crosses = 14 + random.Next(14);
                for (int position = 0; position < mu2Wheel.Pins.Length; position++)
                {
                    mu2Wheel.Pins[position] = SZ42Wheel.INACTIVE_PIN;
                }
                do
                {
                    int position = random.Next(mu2Wheel.Pins.Length);
                    mu2Wheel.Pins[position] = SZ42Wheel.ACTIVE_PIN;
                } while (mu2Wheel.GetCrossCount() != crosses);
            } while (SZ42KeyRulesChecker.CheckMu2WheelRules(mu2Wheel) == false);
        }

        /// <summary>
        /// Generate a chi wheel i [i = offset in chiWheel array]
        /// </summary>
        /// <param name="wheel"></param>
        /// <param name="i"></param>
        /// <param name="random"></param>
        private static void GenerateChiWheel(SZ42Wheel wheel, int i, Random random)
        {
            do
            {
                int r = random.Next(SZ42KeyRulesChecker.ChiRules[i].PossibleNumberOfCrosses.Length);
                int crosses = SZ42KeyRulesChecker.ChiRules[i].PossibleNumberOfCrosses[r];
                for (int position = 0; position < wheel.Pins.Length; position++)
                {
                    wheel.Pins[position] = SZ42Wheel.INACTIVE_PIN;
                }
                do
                {
                    int position = random.Next(wheel.Pins.Length);
                    wheel.Pins[position] = SZ42Wheel.ACTIVE_PIN;
                } while (wheel.GetCrossCount() != crosses);
            } while (!SZ42KeyRulesChecker.CheckNumberOfCrosses(wheel, SZ42KeyRulesChecker.ChiRules[i], null, false) ||
                     !SZ42KeyRulesChecker.CheckNumberOfCrosses(wheel, SZ42KeyRulesChecker.ChiRules[i], null, true) ||
                     !SZ42KeyRulesChecker.CheckNumberOfConsecutiveCrossesAndDots(wheel));
        }

        /// <summary>
        /// Generate a psi wheel i [i = offset in chiWheel array]
        /// </summary>
        /// <param name="wheel"></param>
        /// <param name="i"></param>
        /// <param name="psiRule"></param>
        /// <param name="random"></param>
        private static void GeneratePsiWheel(SZ42Wheel wheel, int i, PsiRule psiRule, Random random)
        {
            do
            {
                int r = random.Next(psiRule.PossibleNumberOfCrosses.Length);
                int crosses = psiRule.PossibleNumberOfCrosses[r];
                for (int position = 0; position < wheel.Pins.Length; position++)
                {
                    wheel.Pins[position] = SZ42Wheel.INACTIVE_PIN;
                }
                do
                {
                    int position = random.Next(wheel.Pins.Length);
                    wheel.Pins[position] = SZ42Wheel.ACTIVE_PIN;
                } while (wheel.GetCrossCount() != crosses);
            } while (!SZ42KeyRulesChecker.CheckNumberOfCrosses(wheel, i, psiRule, null, false) ||
                    !SZ42KeyRulesChecker.CheckNumberOfCrosses(wheel, i, psiRule, null, true));
        }

    }
}
