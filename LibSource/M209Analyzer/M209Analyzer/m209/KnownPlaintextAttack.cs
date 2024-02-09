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
        private M209AttackManager _attackManager { get; set; }
        public Key Key { get; set; }
        public LocalState LocalState { get; set; }
        public KnownPlaintextAttack(Key key, M209AttackManager attackManager, LocalState localState)
        {
            Key = key;
            _attackManager = attackManager;
            LocalState = localState;

            _attackManager.SimulatedAnnealingPins = new SimulatedAnnealingPins(key, attackManager, localState);
            _attackManager.HillClimbLugs = new HillClimbLugs(key, attackManager, localState);
            _attackManager.HillClimbPins = new HillClimbPins(key, attackManager, localState);
        }
        private double RandomizePinsAndLugs(bool lugs, bool pins, bool searchSlide, int randomCycles, int primaryCycle)
        {

            double bestLocal = Double.MinValue;
            bool[][] bestLocalPins = Key.Pins.CreateCopy();
            int[] bestLocalLugsTypeCount = Key.Lugs.CreateTypeCountCopy();

            if (searchSlide)
            {
                if (Key.Slide != primaryCycle % 26)
                {
                    Key.Slide = primaryCycle % 26;
                    Key.InvalidateDecryption();
                }
            }

            for (int r = 0; r < randomCycles; r++)
            {
                if (lugs)
                {
                    Key.Lugs.Randomize();
                }
                if (pins)
                {
                    Key.Pins.Randomize();
                }
                if (!lugs && !pins)
                {
                    Key.Pins.Randomize(RandomGen.NextInt(Key.WHEELS) + 1);
                }
                Key.UpdateDecryptionIfInvalid();
                if (_attackManager.ShouldStop)
                {
                    return bestLocal;
                }

                double newEval = _attackManager.Evaluate(EvalType.CRIB, Key.Decryption, Key.CribArray, LocalState.TaskId);
                if (newEval > bestLocal)
                {
                    bestLocal = newEval;
                    Key.Lugs.GetTypeCount(bestLocalLugsTypeCount);
                    Key.Pins.Get(bestLocalPins);
                }

            }

            Key.Lugs.SetTypeCount(bestLocalLugsTypeCount, false);
            Key.Pins.Set(bestLocalPins);

            return bestLocal;
        }

        public void Solve()
        {
            int randomCycles = 100;
            _attackManager.ProgressChanged("Known-Plaintext", "", 1, randomCycles);

            if (_attackManager.Cycles == 0)
            {
                _attackManager.Cycles = int.MaxValue;
            }

            double old;


            for (int cycle = 0; cycle < _attackManager.Cycles; cycle++)
            {

                int semiRestarts = 8;

                double newEval = RandomizePinsAndLugs(true, true, _attackManager.SearchSlide, randomCycles, cycle);
                if (_attackManager.ShouldStop)
                {
                    return;
                }

                do
                {
                    LocalState.Improved = false;

                    old = newEval;
                    LocalState.Restarts = cycle;
                    LocalState.SingleIteration = false;
                    LocalState.Quick = false;

                    _attackManager.ProgressChanged("Known-Plaintext", "First hill climbing loop", 1, 1);
                    newEval = _attackManager.HillClimbLugs.HillClimb(EvalType.CRIB, _attackManager, LocalState);
                    if (_attackManager.ShouldStop)
                    {
                        return;
                    }
                    if (newEval > old)
                    {
                        LocalState.Improved = true;
                    }

                    newEval = _attackManager.SimulatedAnnealingPins.SA(EvalType.CRIB, newEval > 128000 ? 20 : 5);
                    LocalState.SingleIteration = false;

                    _attackManager.ProgressChanged("Known-Plaintext", "Second hill climbing loop", 1, 1);
                    newEval = _attackManager.HillClimbPins.HillClimb(EvalType.CRIB, false);
                    if (_attackManager.ShouldStop)
                    {
                        return;
                    }
                    if (newEval > old)
                    {
                        LocalState.Improved = true;
                    }

                    if (LocalState.Improved)
                    {
                        semiRestarts = 8;
                    }

                    if (!LocalState.Improved && semiRestarts > 0)
                    {
                        double last = _attackManager.Evaluate(EvalType.CRIB, Key.Decryption, Key.CribArray, LocalState.TaskId);

                        if (last >= 129000 && last < Key.OriginalScore)
                        {
                            int deepCycles = 0;
                            do
                            {

                                LocalState.Improved = false;
                                LocalState.Restarts = cycle;
                                LocalState.SingleIteration = true;
                                LocalState.Quick = false;
                                _attackManager.ProgressChanged("Known-Plaintext", "Third hill climbing loop", 1, 1);
                                newEval = _attackManager.HillClimbLugs.HillClimb(EvalType.PINS_SA_CRIB, _attackManager, LocalState); // Single iteration, print
                                if (_attackManager.ShouldStop)
                                {
                                    return;
                                }
                                if (newEval > last)
                                {
                                    last = newEval;
                                    LocalState.Improved = true;
                                }
                                deepCycles++;

                            } while (LocalState.Improved || deepCycles < 3);


                            semiRestarts /= 2;

                        }
                        else
                        {

                            if ((semiRestarts % 8) != 7)
                            {
                                RandomizePinsAndLugs(true, false, _attackManager.SearchSlide, randomCycles, cycle);
                                LocalState.SingleIteration = true;
                                _attackManager.ProgressChanged("Known-Plaintext", "Third hill climbing loop", 1, 1);
                                newEval = _attackManager.HillClimbPins.HillClimb(EvalType.CRIB, true);
                                if (_attackManager.ShouldStop)
                                {
                                    return;
                                }
                            }
                            else
                            {
                                RandomizePinsAndLugs(false, true, _attackManager.SearchSlide, randomCycles, cycle);
                                if (_attackManager.ShouldStop)
                                {
                                    return;
                                }
                                LocalState.Restarts = cycle;
                                LocalState.SingleIteration = true;
                                LocalState.Quick = true;
                                _attackManager.ProgressChanged("Known-Plaintext", "Third hill climbing loop", 1, 1);
                                newEval = _attackManager.HillClimbLugs.HillClimb(EvalType.CRIB, _attackManager, LocalState); // Single iteration, print
                                if (_attackManager.ShouldStop)
                                {
                                    return;
                                }
                            }

                            LocalState.Improved = true;
                            semiRestarts--;

                        }
                    }
                } while (LocalState.Improved && !_attackManager.ShouldStop);

                if (_attackManager.ShouldStop)
                {
                    return;
                }
            }
        }
    }
}
