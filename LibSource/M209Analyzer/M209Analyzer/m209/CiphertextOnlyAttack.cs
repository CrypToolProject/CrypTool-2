using M209AnalyzerLib.Common;
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
        public static void Solve(Key key, M209AttackManager attackManager, LocalState localState)
        {
            localState.BestTypeCount = key.Lugs.CreateTypeCountCopy();
            localState.BestPins = key.Pins.CreateCopy();

            if (attackManager.Cycles == 0)
            {
                attackManager.Cycles = int.MaxValue;
                attackManager.Cycles = 10_000_000;
            }

            for (int cycle = 0; cycle < attackManager.Cycles; cycle++)
            {
                Console.WriteLine($"[Thread {Thread.CurrentThread.ManagedThreadId}] ########## {cycle} / {attackManager.Cycles} ##########");

                // never used?
                if (attackManager.SearchSlide)
                {
                    key.Slide = cycle % 26;
                    key.InvalidateDecryption();
                }

                localState.CurrentCycle = cycle;
                localState.BestScore = 0.0;
                localState.Improved = false;

                Stopwatch watch = Stopwatch.StartNew();
                RandomTrialPhase(key, attackManager, localState);
                watch.Stop();
                Console.WriteLine($"\n -- [Thread {Thread.CurrentThread.ManagedThreadId}] After phaseTrials  [{watch.ElapsedMilliseconds}]" +
                    $"[{key.GetCountIncorrectLugs()}L/{key.GetCountIncorrectPins()}P]" +
                    $"[2**{(long)(Math.Log(attackManager.EvaluationCount) / Math.Log(2))} ({attackManager.EvaluationCount})][{(attackManager.ElapsedTime.TotalMilliseconds == 0 ? 0 : attackManager.EvaluationCount / attackManager.ElapsedTime.TotalMilliseconds)} K/s]", "info");

                do
                {

                    localState.Improved = false;

                    HillClimbingFirstTransformation(key, attackManager, localState);

                    HillClimbingSecondTransformation(key, attackManager, localState);

                    if (!localState.Improved)
                    {
                        // Complex Transformation (?)
                        HillClimbingThirdTransformation(key, attackManager, localState);

                        HillClimbingFourthTransformation(key, attackManager, localState);
                    }

                } while (localState.Improved && !attackManager.ShouldStop);

                if (attackManager.ShouldStop)
                {
                    return;
                }
            }
        }


        public static void SolveMultithreaded(Key simulationKey, M209AttackManager attackManager)
        {
            Stats.Load(attackManager.ResourcePath, attackManager.Language, true);

            ReportManager.setThreshold(EvalType.MONO);

            if (attackManager.CipherText == null || string.IsNullOrEmpty(attackManager.CipherText))
            {
                attackManager.LogMessage("cipher is empty", "error");
                Console.Read();
                Environment.Exit(-1);
            }

            Runnables runnables = new Runnables();
            for (int i = 0; i < attackManager.Threads; i++)
            {
                Key key = new Key();
                key.SetCipherText(attackManager.CipherText);
                if (simulationKey != null)
                {
                    key.setOriginalKey(simulationKey);
                    key.setOriginalScore(attackManager.Evaluate(EvalType.MONO, simulationKey.CribArray, simulationKey.CribArray));
                }

                //runnables.AddRunnable(new Task(() => Solve(key, attackManager)));
            }
            runnables.Run();
        }
        /// <summary>
        /// Phase of getting the best key of a boundle of randomly choosen keys.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="roundLayers"></param>
        /// <param name="layers"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static void RandomTrialPhase(Key key, M209AttackManager attackManager, LocalState localState)
        {
            double bestRandomScore = int.MinValue;

            for (int r = 0; r < attackManager.Phase1Trials; r++)
            {
                attackManager.ProgressChanged("Ciphertext-Only", "Random trial phase", r + 1, attackManager.Phase1Trials);

                key.Lugs.Randomize();

                double newEval = SimulatedAnnealingPins.SA(key, EvalType.MONO, 1, attackManager);
                if (newEval > bestRandomScore)
                {
                    bestRandomScore = newEval;
                    key.Lugs.GetTypeCount(localState.BestTypeCount);
                    key.Pins.Get(localState.BestPins);
                }
                if (bestRandomScore > localState.BestScore)
                {
                    localState.BestScore = bestRandomScore;
                    attackManager.AddNewBestListEntry(localState.BestScore, key, key.Decryption);
                }

                if (attackManager.ShouldStop)
                {
                    return;
                }
            }
        }

        public static void HillClimbingFirstTransformation(Key key, M209AttackManager attackManager, LocalState localState)
        {
            int[] types = new int[4];
            double newEval;

            for (int i = 0; i < Lugs.TYPES.Length; i++)
            {
                int c1 = Lugs.TYPES[i];

                for (int j = 0; j < Lugs.TYPES.Length; j++)
                {
                    attackManager.ProgressChanged("Ciphertext-Only", "HillClimbing - first loop", (i * Lugs.TYPES.Length) + j + 1, (Lugs.TYPES.Length * Lugs.TYPES.Length));

                    int c2 = Lugs.TYPES[j];

                    if (c1 >= c2)
                    {
                        continue;
                    }
                    types[0] = c1;
                    types[1] = c2;
                    for (int changesIndex = 0; changesIndex < HillClimbLugs.changes2Types.Length; changesIndex++)
                    {
                        int[] changes = HillClimbLugs.changes2Types[changesIndex];

                        if (!HillClimbLugs.DoChangesIfValid(localState.BestTypeCount, types, changes))
                        {
                            continue;
                        }
                        if (!key.Lugs.SetTypeCount(localState.BestTypeCount, false))
                        {
                            HillClimbLugs.UndoChanges(localState.BestTypeCount, types, changes);
                            continue;
                        }

                        newEval = SimulatedAnnealingPins.SA(key, EvalType.MONO, 1, attackManager);
                        if (newEval > localState.BestScore)
                        {
                            localState.Improved = true;
                            localState.BestScore = newEval;
                            key.Pins.Get(localState.BestPins);
                            attackManager.AddNewBestListEntry(localState.BestScore, key, key.Decryption);
                        }
                        else
                        {
                            HillClimbLugs.UndoChanges(localState.BestTypeCount, types, changes);
                            key.Lugs.SetTypeCount(localState.BestTypeCount, false);
                            key.Pins.Set(localState.BestPins);
                        }

                        if (attackManager.ShouldStop)
                        {
                            return;
                        }
                    }
                }
            }
        }

        public static void HillClimbingSecondTransformation(Key key, M209AttackManager attackManager, LocalState localState)
        {
            int[] types = new int[4];
            double newEval;

            for (int i1 = 0; i1 < Lugs.TYPES.Length && !localState.Improved; i1++)
            {
                types[0] = Lugs.TYPES[i1];
                for (int i2 = i1 + 1; i2 < Lugs.TYPES.Length && !localState.Improved; i2++)
                {
                    types[1] = Lugs.TYPES[i2];
                    for (int changesIndex = 0; changesIndex < HillClimbLugs.changes2Types.Length; changesIndex++)
                    {
                        attackManager.ProgressChanged("Ciphertext-Only", "HillClimbing - second loop", (i1 * Lugs.TYPES.Length) + i2 + 1, Lugs.TYPES.Length * Lugs.TYPES.Length);

                        int[] changes = HillClimbLugs.changes2Types[changesIndex];

                        if (!HillClimbLugs.DoChangesIfValid(localState.BestTypeCount, types, changes))
                        {
                            continue;
                        }
                        if (!key.Lugs.SetTypeCount(localState.BestTypeCount, false))
                        {
                            HillClimbLugs.UndoChanges(localState.BestTypeCount, types, changes);
                            continue;
                        }

                        // Evaluate the decryption using Monograms.
                        key.UpdateDecryptionIfInvalid();
                        newEval = attackManager.Evaluate(EvalType.MONO, key.Decryption, key.CribArray);
                        if (newEval > localState.BestScore)
                        {
                            localState.Improved = true;
                            localState.BestScore = newEval;
                            attackManager.AddNewBestListEntry(localState.BestScore, key, key.Decryption);
                            key.Pins.Get(localState.BestPins);
                            break;
                        }
                        else
                        {
                            HillClimbLugs.UndoChanges(localState.BestTypeCount, types, changes);
                            key.Lugs.SetTypeCount(localState.BestTypeCount, false);
                        }

                        if (attackManager.ShouldStop)
                        {
                            return;
                        }
                    }
                }

            }
            attackManager.LogMessage($"\n After second HC loop - improved:{localState.Improved}  (loop break)", "info");
        }

        public static void HillClimbingThirdTransformation(Key key, M209AttackManager attackManager, LocalState localState)
        {
            int[] types = new int[4];
            double newEval;

            for (int i1 = 0; i1 < Lugs.TYPES.Length && !localState.Improved; i1++)
            {
                types[0] = Lugs.TYPES[i1];
                for (int i2 = i1 + 1; i2 < Lugs.TYPES.Length && !localState.Improved; i2++)
                {
                    types[1] = Lugs.TYPES[i2];
                    for (int i3 = i2 + 1; i3 < Lugs.TYPES.Length && !localState.Improved; i3++)
                    {
                        types[2] = Lugs.TYPES[i3];
                        for (int i4 = i3 + 1; i4 < Lugs.TYPES.Length && !localState.Improved; i4++)
                        {
                            types[3] = Lugs.TYPES[i4];

                            for (int changesIndex = 0; changesIndex < HillClimbLugs.changes3Types.Length; changesIndex++)
                            {
                                attackManager.ProgressChanged("Ciphertext-Only", "HillClimbing - third loop", (i1 * Lugs.TYPES.Length) + i2 + 1, Lugs.TYPES.Length * Lugs.TYPES.Length);

                                int[] changes = HillClimbLugs.changes3Types[changesIndex];

                                if (!HillClimbLugs.DoChangesIfValid(localState.BestTypeCount, types, changes))
                                {
                                    continue;
                                }
                                if (!key.Lugs.SetTypeCount(localState.BestTypeCount, false))
                                {
                                    HillClimbLugs.UndoChanges(localState.BestTypeCount, types, changes);
                                    continue;
                                }

                                key.UpdateDecryptionIfInvalid();
                                newEval = attackManager.Evaluate(EvalType.MONO, key.Decryption, key.CribArray);

                                if (newEval > localState.BestScore)
                                {
                                    localState.Improved = true;
                                    localState.BestScore = newEval;
                                    attackManager.AddNewBestListEntry(localState.BestScore, key, key.Decryption);
                                    key.Pins.Get(localState.BestPins);
                                    break;
                                }
                                else
                                {
                                    HillClimbLugs.UndoChanges(localState.BestTypeCount, types, changes);
                                    key.Lugs.SetTypeCount(localState.BestTypeCount, false);
                                }

                                if (attackManager.ShouldStop)
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            attackManager.LogMessage($"\n After third HC loop - improved:{localState.Improved}  (loop break)", "info");
        }

        public static void HillClimbingFourthTransformation(Key key, M209AttackManager attackManager, LocalState localState)
        {
            int[] types = new int[4];
            double newEval;

            for (int i1 = 0; i1 < Lugs.TYPES.Length && !localState.Improved; i1++)
            {
                types[0] = Lugs.TYPES[i1];
                for (int i2 = i1 + 1; i2 < Lugs.TYPES.Length && !localState.Improved; i2++)
                {
                    types[1] = Lugs.TYPES[i2];
                    for (int i3 = i2 + 1; i3 < Lugs.TYPES.Length && !localState.Improved; i3++)
                    {
                        types[2] = Lugs.TYPES[i3];

                        for (int changesIndex = 0; changesIndex < HillClimbLugs.changes4Types.Length; changesIndex++)
                        {
                            attackManager.ProgressChanged("Ciphertext-Only", "HillClimbing - third loop", (i1 * Lugs.TYPES.Length) + i2 + 1, Lugs.TYPES.Length * Lugs.TYPES.Length);

                            int[] changes = HillClimbLugs.changes4Types[changesIndex];

                            if (!HillClimbLugs.DoChangesIfValid(localState.BestTypeCount, types, changes))
                            {
                                continue;
                            }
                            if (!key.Lugs.SetTypeCount(localState.BestTypeCount, false))
                            {
                                HillClimbLugs.UndoChanges(localState.BestTypeCount, types, changes);
                                continue;
                            }

                            key.UpdateDecryptionIfInvalid();
                            newEval = attackManager.Evaluate(EvalType.MONO, key.Decryption, key.CribArray);
                            if (newEval > localState.BestScore)
                            {
                                localState.Improved = true;
                                localState.BestScore = newEval;
                                attackManager.AddNewBestListEntry(localState.BestScore, key, key.Decryption);
                                key.Pins.Get(localState.BestPins);
                                break;
                            }
                            else
                            {
                                HillClimbLugs.UndoChanges(localState.BestTypeCount, types, changes);
                                key.Lugs.SetTypeCount(localState.BestTypeCount, false);
                            }

                            if (attackManager.ShouldStop)
                            {
                                return;
                            }
                        }
                    }
                }
            }
            attackManager.LogMessage($"\n After fourth HC loop - improved:{localState.Improved} (loop break)", "info");
        }
    }
}
