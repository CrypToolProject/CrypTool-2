/*
   Copyright 2022 George Lasry, Nils Kopal, CrypTool 2 Team

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
using PlayfairAnalysis.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PlayfairAnalysis
{
    public class SolvePlayfair
    {
        private static void solve(int taskNumber, int saCycles, int innerRounds /* 200000 */, int multiplier/*1500*/, int[] cipherText, string crib, Key simulationKey, AnalysisInstance instance, long[] evaluationsPerThread)
        {
            try
            {
                CancellationToken ct = instance.CancellationToken;
                Utils utils = new Utils((int)DateTime.Now.Ticks + taskNumber * 100);  //Different seed for every thread
                Transformations transformations = new Transformations(instance, utils);
                SimulatedAnnealing simulatedAnnealing = new SimulatedAnnealing(utils);

                long simulationOriginalScore = (simulationKey == null) ? long.MinValue : simulationKey.score;
                Key currentKey = new Key(instance, utils);
                Key newKey = new Key(instance, utils);
                Key bestKey = new Key(instance, utils);
                currentKey.setCipher(cipherText);
                newKey.setCipher(cipherText);
                bestKey.setCipher(cipherText);
                currentKey.setCrib(crib);
                newKey.setCrib(crib);
                bestKey.setCrib(crib);

                long serialCounter = 0;

                for (int cycle = 0; cycle < saCycles || saCycles == 0; cycle++)
                {
                    ct.ThrowIfCancellationRequested();
                    if (taskNumber == 0)
                    {
                        instance.CtAPI.updateProgress(cycle, saCycles, evaluationsPerThread.Sum());
                    }

                    transformations.randomize();
                    currentKey.random();

                    long currentScore = currentKey.eval();

                    bestKey.copy(currentKey);
                    long bestScore = bestKey.eval();
                    for (int innerRound = 0; innerRound < innerRounds; innerRound++)
                    {
                        ct.ThrowIfCancellationRequested();
                        transformations.apply(currentKey, newKey, serialCounter++);

                        long newScore = newKey.eval();
                        evaluationsPerThread[taskNumber]++;

                        if (simulatedAnnealing.acceptHexaScore(newScore, currentScore, multiplier))
                        {
                            currentKey.copy(newKey);
                            currentScore = newScore;

                            if (currentScore > bestScore)
                            {                              
                                bestScore = currentScore;
                                bestKey.copy(currentKey);
                                bestKey.decrypt();
                            }
                        }
                    }

                    if (instance.CtBestList.shouldPushResult(bestScore))
                    {
                        bestKey.alignAlphabet();
                        (TimeSpan elapsed, long evaluations) = instance.Stats.EvaluationsSummary();
                        evaluations = evaluationsPerThread.Sum();
                        instance.CtBestList.pushResult(bestScore,
                                bestKey.ToString(),
                                bestKey.ToString(),
                                Utils.getString(bestKey.fullDecryption),
                                elapsed,
                                evaluations,
                                instance.Stats.EvaluationsSummary() +
                                        $"[{bestKey.decryptionRemoveNullsLength}/{cipherText.Length}][Task: {taskNumber,2}][Mult.: {multiplier:N0}]");
                        if (currentScore == simulationOriginalScore || newKey.matchesFullCrib())
                        {
                            instance.CtAPI.printf("Key found");
                            instance.CtAPI.goodbye();
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                //Let thread terminate
            }
        }

        public static long solveMultithreaded(int[] cipherText, string cribString, int threads, int cycles, Key simulationKey, AnalysisInstance instance)
        {
            const int innerRounds = 200_000;
            List<TaskCompletionSource<bool>> threadCompletions = new List<TaskCompletionSource<bool>>();
            Key simulationKey_ = simulationKey;
            long[] evaluationsPerThread = new long[threads];
            for (int t_ = 0; t_ < threads; t_++)
            {
                int t = t_;
                double factor = (cribString.Length > cipherText.Length / 2) ? 0.1 : 1.0;
                int multiplier = (int)(factor * 150_000) / cipherText.Length;

                TaskCompletionSource<bool> threadCompletion = new TaskCompletionSource<bool>();
                threadCompletions.Add(threadCompletion);
                new Thread(
                        delegate ()
                        {
                            try
                            {
                                solve(t, cycles, innerRounds, multiplier, cipherText, cribString, simulationKey_, instance, evaluationsPerThread);
                            }
                            finally
                            {
                                threadCompletion.SetResult(true);
                            }
                        }
                ).Start();
            }

            WatchThreadCompletions(threadCompletions, instance, evaluationsPerThread);

            return innerRounds * cycles * threads;
        }

        private static async void WatchThreadCompletions(List<TaskCompletionSource<bool>> threadCompletions, AnalysisInstance instance, long[] evaluationsPerThread)
        {
            await Task.WhenAll(threadCompletions.Select(t => t.Task));

            instance.CtAPI.updateProgress(100, 100, evaluationsPerThread.Sum());
            //call "goodbye" after all threads completed to let CT2 know about it:
            instance.CtAPI.goodbye();
        }
    }
}
