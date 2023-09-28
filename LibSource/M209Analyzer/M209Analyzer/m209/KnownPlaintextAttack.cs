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

namespace M209AnalyzerLib.M209
{
    /// <summary>
    /// Known plaintext attack
    /// </summary>
    public class KnownPlaintextAttack
    {
        private static double RandomizePinsAndLugs(Key key, bool lugs, bool pins, bool searchSlide, int randomCycles, int primaryCycle, M209AttackManager attackManager)
        {

            double bestLocal = Double.MinValue;
            bool[][] bestLocalPins = key.Pins.CreateCopy();
            int[] bestLocalLugsTypeCount = key.Lugs.CreateTypeCountCopy();

            if (searchSlide)
            {
                if (key.Slide != primaryCycle % 26)
                {
                    key.Slide = primaryCycle % 26;
                    key.InvalidateDecryption();
                }
            }

            for (int r = 0; r < randomCycles; r++)
            {
                if (lugs)
                {
                    key.Lugs.Randomize();
                }
                if (pins)
                {
                    key.Pins.Randomize();
                }
                if (!lugs && !pins)
                {
                    key.Pins.Randomize(Utils.RandomNextInt(Key.WHEELS) + 1);
                }
                key.UpdateDecryptionIfInvalid();
                double newEval = attackManager.Evaluate(EvalType.CRIB, key.Decryption, key.CribArray);
                if (newEval > bestLocal)
                {
                    bestLocal = newEval;
                    key.Lugs.GetTypeCount(bestLocalLugsTypeCount);
                    key.Pins.Get(bestLocalPins);
                }
            }

            key.Lugs.SetTypeCount(bestLocalLugsTypeCount, false);
            key.Pins.Set(bestLocalPins);

            return bestLocal;
        }

        public static void Solve(Key key, M209AttackManager attackManager, LocalState localState)
        {
            int RANDOM_CYCLES = 100;

            if (attackManager.Cycles == 0)
            {
                attackManager.Cycles = int.MaxValue;
            }

            double old;


            for (int cycle = 0; cycle < attackManager.Cycles; cycle++)
            {

                int semiRestarts = 8;

                double newEval = RandomizePinsAndLugs(key, true, true, attackManager.SearchSlide, RANDOM_CYCLES, cycle, attackManager);

                do
                {
                    localState.Improved = false;

                    old = newEval;
                    localState.Restarts = cycle;
                    localState.SingleIteration = false;
                    localState.Quick = false;
                    newEval = HillClimbLugs.HillClimb(key, EvalType.CRIB, attackManager, localState);
                    if (newEval > old)
                    {
                        localState.Improved = true;
                    }

                    newEval = SimulatedAnnealingPins.SA(key, EvalType.CRIB, newEval > 128000 ? 20 : 5, attackManager);
                    localState.SingleIteration = false;
                    newEval = HillClimbPins.HillClimb(key, EvalType.CRIB, false, attackManager, localState);
                    if (newEval > old)
                    {
                        localState.Improved = true;
                    }

                    if (localState.Improved)
                    {
                        semiRestarts = 8;
                    }

                    if (!localState.Improved && semiRestarts > 0)
                    {
                        double last = attackManager.Evaluate(EvalType.CRIB, key.Decryption, key.CribArray);

                        if (last >= 129000 && last < key.OriginalScore)
                        {
                            int deepCycles = 0;
                            do
                            {

                                localState.Improved = false;
                                localState.Restarts = cycle;
                                localState.SingleIteration = true;
                                localState.Quick = false;
                                newEval = HillClimbLugs.HillClimb(key, EvalType.PINS_SA_CRIB, attackManager, localState); // Single iteration, print
                                if (newEval > last)
                                {
                                    last = newEval;
                                    localState.Improved = true;
                                }
                                deepCycles++;

                            } while (localState.Improved || deepCycles < 3);


                            semiRestarts /= 2;

                        }
                        else
                        {

                            if ((semiRestarts % 8) != 7)
                            {
                                RandomizePinsAndLugs(key, true, false, attackManager.SearchSlide, RANDOM_CYCLES, cycle, attackManager);
                                localState.SingleIteration = true;
                                newEval = HillClimbPins.HillClimb(key, EvalType.CRIB, true, attackManager, localState);
                            }
                            else
                            {
                                RandomizePinsAndLugs(key, false, true, attackManager.SearchSlide, RANDOM_CYCLES, cycle, attackManager);
                                localState.Restarts = cycle;
                                localState.SingleIteration = true;
                                localState.Quick = true;
                                newEval = HillClimbLugs.HillClimb(key, EvalType.CRIB, attackManager, localState); // Single iteration, print
                            }

                            localState.Improved = true;
                            semiRestarts--;

                        }
                    }
                } while (localState.Improved && !attackManager.ShouldStop);

                if (attackManager.ShouldStop)
                {
                    return;
                }
            }
        }
    }
}
