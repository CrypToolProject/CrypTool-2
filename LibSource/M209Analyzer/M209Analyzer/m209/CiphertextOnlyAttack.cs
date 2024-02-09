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
using M209AnalyzerLib.Enums;
using System;
using System.Diagnostics;
using System.Threading;

namespace M209AnalyzerLib.M209
{
    /// <summary>
    /// Ciphertext only attack
    /// </summary>
    public class CiphertextOnlyAttack
    {
        #region Properties
        private M209AttackManager _attackManager { get; set; }
        public Key Key { get; set; }
        public LocalState LocalState { get; set; }
        #endregion
        public CiphertextOnlyAttack(Key key, M209AttackManager attackManager, LocalState localState)
        {
            Key = key;
            _attackManager = attackManager;
            LocalState = localState;

            _attackManager.SimulatedAnnealingPins = new SimulatedAnnealingPins(key, attackManager, localState);
            _attackManager.HillClimbLugs = new HillClimbLugs(key, attackManager, localState);
        }

        public void Solve()
        {
            _attackManager.ProgressChanged("Ciphertext-Only", "", 1, _attackManager.Phase1Trials);

            LocalState.BestTypeCount = Key.Lugs.CreateTypeCountCopy();
            LocalState.BestPins = Key.Pins.CreateCopy();

            if (_attackManager.Cycles == 0)
            {
                _attackManager.Cycles = int.MaxValue;
                _attackManager.Cycles = 10_000_000;
            }

            for (int cycle = 0; cycle < _attackManager.Cycles; cycle++)
            {
                Console.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId}] ########## {cycle} / {_attackManager.Cycles} ##########");

                // never used?
                if (_attackManager.SearchSlide)
                {
                    Key.Slide = cycle % 26;
                    Key.InvalidateDecryption();
                }

                LocalState.CurrentCycle = cycle;
                LocalState.BestScore = 0.0;
                LocalState.Improved = false;

                Stopwatch watch = Stopwatch.StartNew();
                RandomTrialPhase();
                if (_attackManager.ShouldStop)
                {
                    return;
                }
                watch.Stop();

                Console.WriteLine($"\n -- [Thread {Thread.CurrentThread.ManagedThreadId}] After phaseTrials  [{watch.ElapsedMilliseconds}]" +
                    $"[{Key.GetCountIncorrectLugs()}L/{Key.GetCountIncorrectPins()}P]" +
                    $"[2**{(long)(Math.Log(_attackManager.EvaluationCount) / Math.Log(2))} ({_attackManager.EvaluationCount})][{(_attackManager.ElapsedTime.TotalMilliseconds == 0 ? 0 : _attackManager.EvaluationCount / _attackManager.ElapsedTime.TotalMilliseconds)} K/s]", "info");

                do
                {
                    LocalState.Improved = false;

                    HillClimbingFirstTransformation();
                    if (_attackManager.ShouldStop)
                    {
                        return;
                    }

                    HillClimbingSecondTransformation();
                    if (_attackManager.ShouldStop)
                    {
                        return;
                    }

                    if (!LocalState.Improved)
                    {
                        // Complex Transformation (?)
                        HillClimbingThirdTransformation();
                        if (_attackManager.ShouldStop)
                        {
                            return;
                        }

                        HillClimbingFourthTransformation();
                        if (_attackManager.ShouldStop)
                        {
                            return;
                        }
                    }

                } while (LocalState.Improved && !_attackManager.ShouldStop);

                if (_attackManager.ShouldStop)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Phase of getting the best key of a boundle of randomly choosen keys.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="roundLayers"></param>
        /// <param name="layers"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public void RandomTrialPhase()
        {
            double bestRandomScore = int.MinValue;

            for (int r = 0; r < _attackManager.Phase1Trials; r++)
            {
                _attackManager.ProgressChanged("Ciphertext-Only", "Random trial phase", r + 1, _attackManager.Phase1Trials);

                Key.Lugs.Randomize();

                double newEval = _attackManager.SimulatedAnnealingPins.SA(EvalType.MONO, 1);
                if (newEval > bestRandomScore)
                {
                    bestRandomScore = newEval;
                    Key.Lugs.GetTypeCount(LocalState.BestTypeCount);
                    Key.Pins.Get(LocalState.BestPins);
                }
                if (bestRandomScore > LocalState.BestScore)
                {
                    LocalState.BestScore = bestRandomScore;
                    Key.UpdateDecryption();
                    _attackManager.AddNewBestListEntry(LocalState.BestScore, Key, Key.Decryption, LocalState.TaskId);
                }

                if (_attackManager.ShouldStop)
                {
                    return;
                }
            }
        }

        public void HillClimbingFirstTransformation()
        {
            int[] types = new int[4];
            double newEval;

            for (int i = 0; i < Key.Lugs.TYPES.Length; i++)
            {
                int c1 = Key.Lugs.TYPES[i];

                for (int j = 0; j < Key.Lugs.TYPES.Length; j++)
                {
                    _attackManager.ProgressChanged("Ciphertext-Only", "HillClimbing - first loop", (i * Key.Lugs.TYPES.Length) + j + 1, (Key.Lugs.TYPES.Length * Key.Lugs.TYPES.Length));

                    int c2 = Key.Lugs.TYPES[j];

                    if (c1 >= c2)
                    {
                        continue;
                    }
                    types[0] = c1;
                    types[1] = c2;
                    for (int changesIndex = 0; changesIndex < _attackManager.HillClimbLugs.changes2Types.Length; changesIndex++)
                    {
                        int[] changes = _attackManager.HillClimbLugs.changes2Types[changesIndex];

                        if (!_attackManager.HillClimbLugs.DoChangesIfValid(LocalState.BestTypeCount, types, changes))
                        {
                            continue;
                        }
                        if (!Key.Lugs.SetTypeCount(LocalState.BestTypeCount, false))
                        {
                            _attackManager.HillClimbLugs.UndoChanges(LocalState.BestTypeCount, types, changes);
                            continue;
                        }

                        newEval = _attackManager.SimulatedAnnealingPins.SA(EvalType.MONO, 1);
                        if (newEval > LocalState.BestScore)
                        {
                            LocalState.Improved = true;
                            LocalState.BestScore = newEval;
                            Key.Pins.Get(LocalState.BestPins);
                            if (Key.Decryption == null)
                                _attackManager.AddNewBestListEntry(LocalState.BestScore, Key, Key.Decryption, LocalState.TaskId);
                        }
                        else
                        {
                            _attackManager.HillClimbLugs.UndoChanges(LocalState.BestTypeCount, types, changes);
                            Key.Lugs.SetTypeCount(LocalState.BestTypeCount, false);
                            Key.Pins.Set(LocalState.BestPins);
                        }

                        if (_attackManager.ShouldStop)
                        {
                            return;
                        }
                    }
                }
            }
        }

        public void HillClimbingSecondTransformation()
        {
            int[] types = new int[4];
            double newEval;

            for (int i1 = 0; i1 < Key.Lugs.TYPES.Length && !LocalState.Improved; i1++)
            {
                types[0] = Key.Lugs.TYPES[i1];
                for (int i2 = i1 + 1; i2 < Key.Lugs.TYPES.Length && !LocalState.Improved; i2++)
                {
                    types[1] = Key.Lugs.TYPES[i2];
                    for (int changesIndex = 0; changesIndex < _attackManager.HillClimbLugs.changes2Types.Length; changesIndex++)
                    {
                        _attackManager.ProgressChanged("Ciphertext-Only", "HillClimbing - second loop", (i1 * Key.Lugs.TYPES.Length) + i2 + 1, Key.Lugs.TYPES.Length * Key.Lugs.TYPES.Length);

                        int[] changes = _attackManager.HillClimbLugs.changes2Types[changesIndex];

                        if (!_attackManager.HillClimbLugs.DoChangesIfValid(LocalState.BestTypeCount, types, changes))
                        {
                            continue;
                        }
                        if (!Key.Lugs.SetTypeCount(LocalState.BestTypeCount, false))
                        {
                            _attackManager.HillClimbLugs.UndoChanges(LocalState.BestTypeCount, types, changes);
                            continue;
                        }

                        // Evaluate the decryption using Monograms.
                        Key.UpdateDecryptionIfInvalid();
                        newEval = _attackManager.Evaluate(EvalType.MONO, Key.Decryption, Key.CribArray, LocalState.TaskId);
                        if (newEval > LocalState.BestScore)
                        {
                            LocalState.Improved = true;
                            LocalState.BestScore = newEval;
                            _attackManager.AddNewBestListEntry(LocalState.BestScore, Key, Key.Decryption, LocalState.TaskId);
                            Key.Pins.Get(LocalState.BestPins);
                            break;
                        }
                        else
                        {
                            _attackManager.HillClimbLugs.UndoChanges(LocalState.BestTypeCount, types, changes);
                            Key.Lugs.SetTypeCount(LocalState.BestTypeCount, false);
                        }

                        if (_attackManager.ShouldStop)
                        {
                            return;
                        }
                    }
                }

            }
            _attackManager.LogMessage($"\n After second HC loop - improved:{LocalState.Improved}  (loop break)", "info");
        }

        public void HillClimbingThirdTransformation()
        {
            int[] types = new int[4];
            double newEval;

            for (int i1 = 0; i1 < Key.Lugs.TYPES.Length && !LocalState.Improved; i1++)
            {
                types[0] = Key.Lugs.TYPES[i1];
                for (int i2 = i1 + 1; i2 < Key.Lugs.TYPES.Length && !LocalState.Improved; i2++)
                {
                    types[1] = Key.Lugs.TYPES[i2];
                    for (int i3 = i2 + 1; i3 < Key.Lugs.TYPES.Length && !LocalState.Improved; i3++)
                    {
                        types[2] = Key.Lugs.TYPES[i3];
                        for (int i4 = i3 + 1; i4 < Key.Lugs.TYPES.Length && !LocalState.Improved; i4++)
                        {
                            types[3] = Key.Lugs.TYPES[i4];

                            for (int changesIndex = 0; changesIndex < _attackManager.HillClimbLugs.changes3Types.Length; changesIndex++)
                            {
                                _attackManager.ProgressChanged("Ciphertext-Only", "HillClimbing - third loop", (i1 * Key.Lugs.TYPES.Length) + i2 + 1, Key.Lugs.TYPES.Length * Key.Lugs.TYPES.Length);

                                int[] changes = _attackManager.HillClimbLugs.changes3Types[changesIndex];

                                if (!_attackManager.HillClimbLugs.DoChangesIfValid(LocalState.BestTypeCount, types, changes))
                                {
                                    continue;
                                }
                                if (!Key.Lugs.SetTypeCount(LocalState.BestTypeCount, false))
                                {
                                    _attackManager.HillClimbLugs.UndoChanges(LocalState.BestTypeCount, types, changes);
                                    continue;
                                }

                                Key.UpdateDecryptionIfInvalid();
                                newEval = _attackManager.Evaluate(EvalType.MONO, Key.Decryption, Key.CribArray, LocalState.TaskId);

                                if (newEval > LocalState.BestScore)
                                {
                                    LocalState.Improved = true;
                                    LocalState.BestScore = newEval;
                                    _attackManager.AddNewBestListEntry(LocalState.BestScore, Key, Key.Decryption, LocalState.TaskId);
                                    Key.Pins.Get(LocalState.BestPins);
                                    break;
                                }
                                else
                                {
                                    _attackManager.HillClimbLugs.UndoChanges(LocalState.BestTypeCount, types, changes);
                                    Key.Lugs.SetTypeCount(LocalState.BestTypeCount, false);
                                }

                                if (_attackManager.ShouldStop)
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            _attackManager.LogMessage($"\n After third HC loop - improved:{LocalState.Improved}  (loop break)", "info");
        }

        public void HillClimbingFourthTransformation()
        {
            int[] types = new int[4];
            double newEval;

            for (int i1 = 0; i1 < Key.Lugs.TYPES.Length && !LocalState.Improved; i1++)
            {
                types[0] = Key.Lugs.TYPES[i1];
                for (int i2 = i1 + 1; i2 < Key.Lugs.TYPES.Length && !LocalState.Improved; i2++)
                {
                    types[1] = Key.Lugs.TYPES[i2];
                    for (int i3 = i2 + 1; i3 < Key.Lugs.TYPES.Length && !LocalState.Improved; i3++)
                    {
                        types[2] = Key.Lugs.TYPES[i3];

                        for (int changesIndex = 0; changesIndex < _attackManager.HillClimbLugs.changes4Types.Length; changesIndex++)
                        {
                            _attackManager.ProgressChanged("Ciphertext-Only", "HillClimbing - third loop", (i1 * Key.Lugs.TYPES.Length) + i2 + 1, Key.Lugs.TYPES.Length * Key.Lugs.TYPES.Length);

                            int[] changes = _attackManager.HillClimbLugs.changes4Types[changesIndex];

                            if (!_attackManager.HillClimbLugs.DoChangesIfValid(LocalState.BestTypeCount, types, changes))
                            {
                                continue;
                            }
                            if (!Key.Lugs.SetTypeCount(LocalState.BestTypeCount, false))
                            {
                                _attackManager.HillClimbLugs.UndoChanges(LocalState.BestTypeCount, types, changes);
                                continue;
                            }

                            Key.UpdateDecryptionIfInvalid();
                            newEval = _attackManager.Evaluate(EvalType.MONO, Key.Decryption, Key.CribArray, LocalState.TaskId);
                            if (newEval > LocalState.BestScore)
                            {
                                LocalState.Improved = true;
                                LocalState.BestScore = newEval;
                                _attackManager.AddNewBestListEntry(LocalState.BestScore, Key, Key.Decryption, LocalState.TaskId);
                                Key.Pins.Get(LocalState.BestPins);
                                break;
                            }
                            else
                            {
                                _attackManager.HillClimbLugs.UndoChanges(LocalState.BestTypeCount, types, changes);
                                Key.Lugs.SetTypeCount(LocalState.BestTypeCount, false);
                            }

                            if (_attackManager.ShouldStop)
                            {
                                return;
                            }
                        }
                    }
                }
            }
            _attackManager.LogMessage($"\n After fourth HC loop - improved:{LocalState.Improved} (loop break)", "info");
        }
    }
}
