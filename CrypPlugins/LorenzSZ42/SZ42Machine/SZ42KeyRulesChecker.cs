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
using System.Text;

namespace CrypTool.LorenzSZ42.SZ42Machine
{
    public class SZ42KeyRulesChecker
    {
        /// <summary>
        /// Rules for chi wheels
        /// </summary>
        public static ChiRule[] ChiRules { get; } = new[]
        {
            new ChiRule(new int[]{20, 21}, new int[]{20    }, 5, 5),
            new ChiRule(new int[]{15, 16}, new int[]{16    }, 5, 5),
            new ChiRule(new int[]{14, 15}, new int[]{14    }, 5, 5),
            new ChiRule(new int[]{13    }, new int[]{12, 14}, 5, 5),
            new ChiRule(new int[]{11, 12}, new int[]{12    }, 5, 5)
        };

        /// <summary>
        /// Rules for psi wheels
        /// </summary>
        public static PsiRule[] PsiRules { get; } = new[]
        {
            /* dots in Mu2 = 14 */ new PsiRule(new int[]{22, 24, 26, 27, 30}, new int[]{26}, new int[]{28},      new int[]{32},     new int[]{32},  new int[]{36}),
            /* dots in Mu2 = 15 */ new PsiRule(new int[]{22, 24, 26, 27, 30}, new int[]{26}, new int[]{30},      new int[]{32},     new int[]{34},  new int[]{38}),
            /* dots in Mu2 = 16 */ new PsiRule(new int[]{22, 24, 26, 27, 30}, new int[]{28}, new int[]{30},      new int[]{32},     new int[]{34},  new int[]{38}),
            /* dots in Mu2 = 17 */ new PsiRule(new int[]{22, 24, 26, 27, 30}, new int[]{28}, new int[]{30},      new int[]{32, 34}, new int[]{34},  new int[]{38}),
            /* dots in Mu2 = 18 */ new PsiRule(new int[]{22, 24, 26, 27, 30}, new int[]{28}, new int[]{32},      new int[]{34},     new int[]{36},  new int[]{38}),
            /* dots in Mu2 = 19 */ new PsiRule(new int[]{22, 24, 26, 27, 30}, new int[]{28}, new int[]{32},      new int[]{34},     new int[]{36},  new int[]{40}),
            /* dots in Mu2 = 20 */ new PsiRule(new int[]{22, 24, 26, 27, 30}, new int[]{30}, new int[]{32},      new int[]{34, 36}, new int[]{36},  new int[]{40}),
            /* dots in Mu2 = 21 */ new PsiRule(new int[]{22, 24, 26, 27, 30}, new int[]{30}, new int[]{32},      new int[]{36},     new int[]{38},  new int[]{42}),
            /* dots in Mu2 = 22 */ new PsiRule(new int[]{22, 24, 26, 27, 30}, new int[]{30}, new int[]{34},      new int[]{36},     new int[]{38},  new int[]{42}),
            /* dots in Mu2 = 23 */ new PsiRule(new int[]{22, 24, 26, 27, 30}, new int[]{32}, new int[]{34},      new int[]{38},     new int[]{38},  new int[]{42}),
            /* dots in Mu2 = 24 */ new PsiRule(new int[]{22, 24, 26, 27, 30}, new int[]{32}, new int[]{34},      new int[]{38},     new int[]{40},  new int[]{44}),
            /* dots in Mu2 = 25 */ new PsiRule(new int[]{22, 24, 26, 27, 30}, new int[]{32}, new int[]{36},      new int[]{38},     new int[]{40},  new int[]{44}),
            /* dots in Mu2 = 26 */ new PsiRule(new int[]{22, 24, 26, 27, 30}, new int[]{34}, new int[]{36},      new int[]{40},     new int[]{40},  new int[]{46}),
            /* dots in Mu2 = 27 */ new PsiRule(new int[]{22, 24, 26, 27, 30}, new int[]{34}, new int[]{36, 38 }, new int[]{40},     new int[]{42},  new int[]{46}),
            /* dots in Mu2 = 28 */ new PsiRule(new int[]{22, 24, 26, 27, 30}, new int[]{34}, new int[]{38},      new int[]{42},     new int[]{42 }, new int[]{48})
        };

        /// <summary>
        /// Checks the wheel settings of the given sz42 for rules compliance
        /// and logs to logBuilder if there are any wrong settings at any wheel
        /// </summary>
        /// <param name="sz42"></param>
        /// <param name="stringBuilder"></param>
        /// <returns></returns>
        public static bool CheckRules(SZ42Machine sz42, StringBuilder stringBuilder, bool writeCategoryReportLine = false)
        {
            bool rulesOk = true;
            if (CheckChiRules(sz42.ChiWheels, stringBuilder) == true)
            {
                if (writeCategoryReportLine)
                {
                    stringBuilder.AppendLine(Properties.Resources.ChiWeelPinsSetAccordingToRules);
                }
            }
            else
            {
                rulesOk = false;
                if (writeCategoryReportLine)
                {
                    stringBuilder.AppendLine(Properties.Resources.ChiWeelPinsNotSetAccordingToRules);
                }
            }

            if (CheckPsiRules(sz42.PsiWheels, sz42.MuWheels[1].GetDotCount(), stringBuilder) == true)
            {
                if (writeCategoryReportLine)
                {
                    stringBuilder.AppendLine(Properties.Resources.PsiWeelPinsSetAccordingToRules);
                }
            }
            else
            {
                rulesOk = false;
                if (writeCategoryReportLine)
                {
                    stringBuilder.AppendLine(Properties.Resources.PsiWeelPinsNotSetAccordingToRules);
                }
            }

            if (CheckMu1WheelRules(sz42.MuWheels[0], stringBuilder) == true)
            {
                if (writeCategoryReportLine)
                {
                    stringBuilder.AppendLine(Properties.Resources.Mu1WeelPinsSetAccordingToRules);
                }
            }
            else
            {
                rulesOk = false;
                if (writeCategoryReportLine)
                {
                    stringBuilder.AppendLine(Properties.Resources.Mu1WeelPinsNotSetAccordingToRules);
                }
            }

            if (CheckMu2WheelRules(sz42.MuWheels[1], stringBuilder) == true)
            {
                if (writeCategoryReportLine)
                {
                    stringBuilder.AppendLine(Properties.Resources.Mu2WeelPinsSetAccordingToRules);
                }
            }
            else
            {
                rulesOk = false;
                if (writeCategoryReportLine)
                {
                    stringBuilder.AppendLine(Properties.Resources.Mu2WeelPinsSetAccordingToRules);
                }
            }

            return rulesOk;
        }

        /// <summary>
        /// Checks if the chi rules are fulfilled and logs to logBuilder if there are any wrong settings
        /// </summary>
        /// <param name="chiWheels"></param>
        /// <param name="logBuilder"></param>
        /// <returns></returns>
        public static bool CheckChiRules(SZ42Wheel[] chiWheels, StringBuilder logBuilder = null)
        {
            bool rulesOk = true;
            //Step 1: Check number of crosses
            for (int i = 0; i < chiWheels.Length; i++)
            {
                if (!CheckNumberOfCrosses(chiWheels[i], ChiRules[i], logBuilder, false))
                {
                    rulesOk = false;
                }
            }
            //Step 2: Check number of crosses in delta
            for (int i = 0; i < chiWheels.Length; i++)
            {
                if (!CheckNumberOfCrosses(chiWheels[i], ChiRules[i], logBuilder, true))
                {
                    rulesOk = false;
                }
            }
            //Step 3: Check number of consecutive crosses and dots
            for (int i = 0; i < chiWheels.Length; i++)
            {
                if (!CheckNumberOfConsecutiveCrossesAndDots(chiWheels[i], logBuilder))
                {
                    rulesOk = false;
                }
            }


            return rulesOk;
        }

        /// <summary>
        /// Checks if the psi rules are fulfilled and logs to logBuilder if there are any wrong settings
        /// </summary>
        /// <param name="psiWheels"></param>
        /// <param name="dotsInMu2"></param>
        /// <param name="logBuilder"></param>
        /// <returns></returns>
        public static bool CheckPsiRules(SZ42Wheel[] psiWheels, int dotsInMu2, StringBuilder logBuilder = null)
        {
            PsiRule psiRule = GetPsiRule(dotsInMu2);
            if (psiRule == null)
            {
                logBuilder?.AppendLine(string.Format(Properties.Resources.InvalidNumberOfDotsInMu2, dotsInMu2));
                logBuilder?.AppendLine(Properties.Resources.CannotSelectPsiRuleBasedOnDotsOfMu2);
                return false;
            }

            bool rulesOk = true;

            //Step 1: Check number of crosses
            for (int i = 0; i < psiWheels.Length; i++)
            {
                if (!CheckNumberOfCrosses(psiWheels[i], i, psiRule, logBuilder, false))
                {
                    rulesOk = false;
                }
            }
            //Step 2: Check number of crosses in delta
            for (int i = 0; i < psiWheels.Length; i++)
            {
                if (!CheckNumberOfCrosses(psiWheels[i], i, psiRule, logBuilder, true))
                {
                    rulesOk = false;
                }
            }

            return rulesOk;
        }

        /// <summary>
        /// Checks the wheel Mu1 according to rules
        /// 30 <= k <= 50, k != 37 [k = number of crosses]
        /// <=5 consecutive dots
        /// <=15 consecutive crosses
        /// </summary>
        /// <param name="muWheels"></param>
        /// <param name="logBuilder"></param>
        /// <returns></returns>
        public static bool CheckMu1WheelRules(SZ42Wheel mu1Wheel, StringBuilder logBuilder = null)
        {
            bool rulesOk = true;

            //Step 1: check number of crosses
            int crosses = 0;
            for (int j = 0; j < mu1Wheel.Pins.Length; j++)
            {
                if (mu1Wheel.Pins[j] == SZ42Wheel.ACTIVE_PIN)
                {
                    crosses++;
                }
            }
            if (crosses < 5 || crosses > 50 || crosses == 37)
            {
                logBuilder?.AppendLine(string.Format(Properties.Resources.InvalidNumberOfCrossesInWheel, mu1Wheel.Name, crosses));
                rulesOk = false;
            }

            //Step 2: check number of consecutive crosses and dots
            int consecutiveCrosses = 0;
            int consecutiveDots = 0;
            int maxCountConsecutiveCrosses = 0;
            int maxCountConsecutiveDots = 0;

            //determining the numbers of maxCountConsecutiveCrosses > 15 and maxCountConsecutiveDots > 5
            for (int i = -15; i < mu1Wheel.Pins.Length; i++)
            {
                switch (mu1Wheel.Pins[Mod(i, mu1Wheel.Pins.Length)])
                {
                    case SZ42Wheel.ACTIVE_PIN:
                        consecutiveDots = 0;
                        consecutiveCrosses++;
                        break;
                    case SZ42Wheel.INACTIVE_PIN:
                        consecutiveCrosses = 0;
                        consecutiveDots++;
                        break;
                }
                if (consecutiveCrosses > maxCountConsecutiveCrosses)
                {
                    maxCountConsecutiveCrosses = consecutiveCrosses;
                }
                if (consecutiveDots > maxCountConsecutiveDots)
                {
                    maxCountConsecutiveDots = consecutiveDots;
                }
            }
            if (maxCountConsecutiveDots > 5)
            {
                logBuilder?.AppendLine(string.Format(Properties.Resources.InvalidNumberOfConsecutiveDotsInWheel, mu1Wheel.Name, maxCountConsecutiveDots));
                rulesOk = false;
            }
            if (maxCountConsecutiveCrosses > 15)
            {
                logBuilder?.AppendLine(string.Format(Properties.Resources.InvalidNumberOfConsecutiveCrossesInWheel, mu1Wheel.Name, maxCountConsecutiveCrosses));
                rulesOk = false;
            }

            return rulesOk;
        }

        /// <summary>
        /// Checks the wheel Mu2 according to rules
        /// 14 <= d <= 28, [d = number of dots]
        /// <= 5 consecutive dots
        /// <= 6 consecutive crosses
        /// </summary>
        /// <param name="muWheels"></param>
        /// <param name="logBuilder"></param>
        /// <returns></returns>
        public static bool CheckMu2WheelRules(SZ42Wheel mu2Wheel, StringBuilder logBuilder = null)
        {
            bool rulesOk = true;

            //Step 1: check number of dots
            int dots = 0;
            for (int j = 0; j < mu2Wheel.Pins.Length; j++)
            {
                if (mu2Wheel.Pins[j] == SZ42Wheel.INACTIVE_PIN)
                {
                    dots++;
                }
            }
            if (dots < 14 || dots > 28)
            {
                logBuilder?.AppendLine(string.Format(Properties.Resources.InvalidNumberOfDotsInWheel, mu2Wheel.Name, dots));
                rulesOk = false;
            }

            //Step 2: check number of consecutive crosses and dots
            int consecutiveCrosses = 0;
            int consecutiveDots = 0;
            int maxCountConsecutiveCrosses = 0;
            int maxCountConsecutiveDots = 0;

            //determining the numbers of maxCountConsecutiveCrosses > 6 and maxCountConsecutiveDots > 5
            for (int i = -15; i < mu2Wheel.Pins.Length; i++)
            {
                switch (mu2Wheel.Pins[Mod(i, mu2Wheel.Pins.Length)])
                {
                    case SZ42Wheel.ACTIVE_PIN:
                        consecutiveDots = 0;
                        consecutiveCrosses++;
                        break;
                    case SZ42Wheel.INACTIVE_PIN:
                        consecutiveCrosses = 0;
                        consecutiveDots++;
                        break;
                }
                if (consecutiveCrosses > maxCountConsecutiveCrosses)
                {
                    maxCountConsecutiveCrosses = consecutiveCrosses;
                }
                if (consecutiveDots > maxCountConsecutiveDots)
                {
                    maxCountConsecutiveDots = consecutiveDots;
                }
            }
            if (maxCountConsecutiveDots > 5)
            {
                logBuilder?.AppendLine(string.Format(Properties.Resources.InvalidNumberOfConsecutiveDotsInWheel, mu2Wheel.Name, maxCountConsecutiveDots));
                rulesOk = false;
            }
            if (maxCountConsecutiveCrosses > 6)
            {
                logBuilder?.AppendLine(string.Format(Properties.Resources.InvalidNumberOfConsecutiveCrossesInWheel, mu2Wheel.Name, maxCountConsecutiveCrosses));
                rulesOk = false;
            }

            return rulesOk;
        }

        /// <summary>
        /// Checks the number of crosses of the given chi wheel
        /// If delta is true, then it checks the number of crosses in delta of the chi given wheel
        /// </summary>
        /// <param name="wheel"></param>
        /// <param name="chiRule"></param>
        /// <param name="logBuilder"></param>
        /// <param name="delta"></param>
        /// <returns></returns>
        public static bool CheckNumberOfCrosses(SZ42Wheel wheel, ChiRule chiRule, StringBuilder logBuilder, bool delta)
        {
            bool rulesOk = true;
            int crosses = 0;
            char[] pins = wheel.Pins;
            int[] possibleCrossNumber = chiRule.PossibleNumberOfCrosses;
            if (delta)
            {
                pins = GeneratePinsDelta(pins);
                possibleCrossNumber = chiRule.PossibleNumberOfCrossesInDelta;
            }

            for (int j = 0; j < pins.Length; j++)
            {
                if (pins[j] == SZ42Wheel.ACTIVE_PIN)
                {
                    crosses++;
                }
            }
            bool crossesOk = false;
            foreach (int possibleCrosses in possibleCrossNumber)
            {
                if (possibleCrosses == crosses)
                {
                    crossesOk = true;
                }
            }
            if (!crossesOk)
            {
                if (delta)
                {
                    logBuilder?.AppendLine(string.Format(Properties.Resources.InvalidNumberInDeltaCrossesInWheel, wheel.Name, crosses));
                }
                else
                {
                    logBuilder?.AppendLine(string.Format(Properties.Resources.InvalidNumberOfCrossesInWheel, wheel.Name, crosses));
                }
                rulesOk = false;
            }
            return rulesOk;
        }

        /// <summary>
        /// Checks the number of crosses of the given psi wheel
        /// If delta is true, then it checks the number of crosses in delta of the given psi wheel
        /// </summary>
        /// <param name="wheel"></param>
        /// <param name="chiRule"></param>
        /// <param name="logBuilder"></param>
        /// <param name="delta"></param>
        /// <returns></returns>
        public static bool CheckNumberOfCrosses(SZ42Wheel wheel, int wheelId, PsiRule psiRule, StringBuilder logBuilder, bool delta)
        {
            bool rulesOk = true;
            int crosses = 0;
            char[] pins = wheel.Pins;
            int[] possibleCrossNumber = psiRule.PossibleNumberOfCrosses;
            if (delta)
            {
                pins = GeneratePinsDelta(pins);
                possibleCrossNumber = psiRule.PossibleNumberOfCrossesInDelta[wheelId];
            }

            for (int j = 0; j < pins.Length; j++)
            {
                if (pins[j] == SZ42Wheel.ACTIVE_PIN)
                {
                    crosses++;
                }
            }
            bool crossesOk = false;
            foreach (int possibleCrosses in possibleCrossNumber)
            {
                if (possibleCrosses == crosses)
                {
                    crossesOk = true;
                }
            }
            if (!crossesOk)
            {
                if (delta)
                {
                    logBuilder?.AppendLine(string.Format(Properties.Resources.InvalidNumberInDeltaCrossesInWheel, wheel.Name, crosses));
                }
                else
                {
                    logBuilder?.AppendLine(string.Format(Properties.Resources.InvalidNumberOfCrossesInWheel, wheel.Name, crosses));
                }
                rulesOk = false;
            }
            return rulesOk;
        }

        /// <summary>
        /// Checks the numbers of consecutive crosses and dots
        /// </summary>
        /// <param name="wheel"></param>
        /// <param name="chiRule"></param>
        /// <param name="logBuilder"></param>
        /// <returns></returns>
        public static bool CheckNumberOfConsecutiveCrossesAndDots(SZ42Wheel wheel, StringBuilder logBuilder = null)
        {
            int consecutiveCrosses = 0;
            int consecutiveDots = 0;
            int maxCountConsecutiveCrosses = 0;
            int maxCountConsecutiveDots = 0;

            //determining the numbers of maxCountConsecutiveCrosses > 5 and maxCountConsecutiveDots > 5
            for (int i = -5; i < wheel.Pins.Length; i++)
            {
                switch (wheel.Pins[Mod(i, wheel.Pins.Length)])
                {
                    case SZ42Wheel.ACTIVE_PIN:
                        consecutiveDots = 0;
                        consecutiveCrosses++;
                        break;
                    case SZ42Wheel.INACTIVE_PIN:
                        consecutiveCrosses = 0;
                        consecutiveDots++;
                        break;
                }
                if (consecutiveCrosses > 5)
                {
                    if (consecutiveCrosses > maxCountConsecutiveCrosses)
                    {
                        maxCountConsecutiveCrosses = consecutiveCrosses;
                    }
                }
                if (consecutiveDots > 5)
                {
                    if (consecutiveDots > maxCountConsecutiveDots)
                    {
                        maxCountConsecutiveDots = consecutiveDots;
                    }
                }
            }
            if (maxCountConsecutiveCrosses > 0)
            {
                logBuilder?.AppendLine(string.Format(Properties.Resources.InvalidNumberOfConsecutiveCrossesInWheel, wheel.Name, maxCountConsecutiveCrosses));
            }
            if (maxCountConsecutiveDots > 0)
            {
                logBuilder?.AppendLine(string.Format(Properties.Resources.InvalidNumberOfConsecutiveDotsInWheel, wheel.Name, maxCountConsecutiveDots));
            }
            return maxCountConsecutiveCrosses == 0 && maxCountConsecutiveDots == 0;
        }

        /// <summary>
        /// Generates the delta of the pins by XORing the pins with the left-shifted 1 version of itself
        /// </summary>
        /// <param name="pins"></param>
        /// <returns></returns>
        private static char[] GeneratePinsDelta(char[] pins)
        {
            char[] delta = new char[pins.Length];
            for (int i = -1; i < pins.Length; i++)
            {
                if ((pins[Mod(i, pins.Length)] == SZ42Wheel.ACTIVE_PIN && pins[Mod(i + 1, pins.Length)] == SZ42Wheel.INACTIVE_PIN) ||
                    (pins[Mod(i, pins.Length)] == SZ42Wheel.INACTIVE_PIN && pins[Mod(i + 1, pins.Length)] == SZ42Wheel.ACTIVE_PIN))
                {
                    delta[Mod(i, pins.Length)] = SZ42Wheel.ACTIVE_PIN;
                }
                else
                {
                    delta[Mod(i, pins.Length)] = SZ42Wheel.INACTIVE_PIN;
                }
            }
            return delta;
        }

        /// <summary>
        /// Returns the psi rule for the given dotsInMu2
        /// </summary>
        /// <param name="dotsInMu2"></param>
        /// <returns></returns>
        public static PsiRule GetPsiRule(int dotsInMu2)
        {
            if (dotsInMu2 < 14 || dotsInMu2 > 28)
            {
                return null;
            }
            return PsiRules[dotsInMu2 - 14];
        }

        /// <summary>
        /// Mathematical modulo operator
        /// </summary>
        /// <param name="number"></param>
        /// <param name="mod"></param>
        /// <returns></returns>
        private static int Mod(int number, int mod)
        {
            return (number % mod + mod) % mod;
        }
    }

    /// <summary>
    /// Rule for a single Chi wheel
    /// </summary>
    public class ChiRule
    {
        public int[] PossibleNumberOfCrosses { get; }
        public int[] PossibleNumberOfCrossesInDelta { get; }
        public int MaxConsecutiveCrosses { get; }
        public int MaxConsecutiveDots { get; }

        public ChiRule(int[] possibleNumberOfCrosses,
            int[] possibleNumberOfCrossesInDelta,
            int maxConsecutiveCrosses,
            int maxConsecutiveDots)
        {
            PossibleNumberOfCrosses = possibleNumberOfCrosses;
            PossibleNumberOfCrossesInDelta = possibleNumberOfCrossesInDelta;
            MaxConsecutiveCrosses = maxConsecutiveCrosses;
            MaxConsecutiveDots = maxConsecutiveDots;
        }
    }

    /// <summary>
    /// Rule for all psi wheels (dependent on dotsInMu2)
    /// </summary>
    public class PsiRule
    {
        public int[] PossibleNumberOfCrosses { get; }
        public int[][] PossibleNumberOfCrossesInDelta { get; }

        public PsiRule(int[] possibleNumberOfCrosses,
            int[] possibleNumberOfCrossesInDelta_Wheel1,
            int[] possibleNumberOfCrossesInDelta_Wheel2,
            int[] possibleNumberOfCrossesInDelta_Wheel3,
            int[] possibleNumberOfCrossesInDelta_Wheel4,
            int[] possibleNumberOfCrossesInDelta_Wheel5)
        {
            PossibleNumberOfCrosses = possibleNumberOfCrosses;
            PossibleNumberOfCrossesInDelta = new int[5][];
            PossibleNumberOfCrossesInDelta[0] = possibleNumberOfCrossesInDelta_Wheel1;
            PossibleNumberOfCrossesInDelta[1] = possibleNumberOfCrossesInDelta_Wheel2;
            PossibleNumberOfCrossesInDelta[2] = possibleNumberOfCrossesInDelta_Wheel3;
            PossibleNumberOfCrossesInDelta[3] = possibleNumberOfCrossesInDelta_Wheel4;
            PossibleNumberOfCrossesInDelta[4] = possibleNumberOfCrossesInDelta_Wheel5;
        }
    }
}