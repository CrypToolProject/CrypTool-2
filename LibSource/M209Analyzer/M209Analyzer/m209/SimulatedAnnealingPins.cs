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

namespace M209AnalyzerLib.M209
{
    public class SimulatedAnnealingPins
    {

        private static double Step(Key key, EvalType evalType,
                                bool[][] bestSAPins, double bestSAScore,
                                double temperature, M209AttackManager attackManager)
        {
            SimulatedAnnealing.SAParameters = attackManager.SAParameters;

            double currLocalScore = 0;

            /*final*/
            int MAX_COUNT = key.Pins.MaxCount();
            /*final*/
            int MIN_COUNT = key.Pins.MinCount();

            double newScore;
            int count;

            bool changeAccepted = false;

            // T1
            for (int w = 1; w <= Key.WHEELS; w++)
            {
                for (int p = 0; p < Key.WHEELS_SIZE[w]; p++)
                {
                    key.Pins.Toggle(w, p);
                    count = key.Pins.Count();
                    if (count < MIN_COUNT || count > MAX_COUNT || key.Pins.LongSeq(w, p))
                    {
                        key.Pins.Toggle(w, p);
                        continue;
                    }
                    key.UpdateDecryption(w, p);

                    newScore = attackManager.Evaluate(evalType, key.Decryption, key.CribArray);
                    if (SimulatedAnnealing.Accept(newScore, currLocalScore, temperature))
                    {
                        currLocalScore = newScore;
                        changeAccepted = true;
                        if (newScore > bestSAScore)
                        {
                            key.Pins.Get(bestSAPins);
                            bestSAScore = newScore;
                        }
                        attackManager.AddNewBestListEntry(currLocalScore, key, key.Decryption);
                    }
                    else
                    {
                        key.Pins.Toggle(w, p);
                        key.UpdateDecryption(w, p);
                    }
                }
            }

            // T4
            for (int w = 1; w <= Key.WHEELS; w++)
            {

                key.Pins.Inverse(w);
                count = key.Pins.Count();
                if (count < MIN_COUNT || count > MAX_COUNT)
                {
                    key.Pins.Inverse(w);
                    continue;
                }
                //key.updateDecryption();

                key.UpdateDecryptionIfInvalid();
                newScore = attackManager.Evaluate(evalType, key.Decryption, key.CribArray);
                if (SimulatedAnnealing.Accept(newScore, currLocalScore, temperature))
                {
                    currLocalScore = newScore;
                    changeAccepted = true;
                    if (newScore > bestSAScore)
                    {
                        key.Pins.Get(bestSAPins);
                        bestSAScore = newScore;
                    }
                    attackManager.AddNewBestListEntry(currLocalScore, key, key.Decryption);

                }
                else
                {
                    key.Pins.Inverse(w);
                    //key.updateDecryption();
                }
            }

            if (changeAccepted)
            {
                return bestSAScore;
            }

            // T2
            for (int w = 1; w <= Key.WHEELS; w++)
            {
                for (int p1 = 0; p1 < Key.WHEELS_SIZE[w]; p1++)
                {
                    for (int p2 = p1 + 1; p2 < Key.WHEELS_SIZE[w]; p2++)
                    {
                        if (key.Pins.Compare(w, p1, p2))
                        {
                            continue;
                        }
                        // Because the two places have a different value, we do not check the count.

                        key.Pins.Toggle(w, p1, p2);

                        if (key.Pins.LongSeq(w, p1, p2))
                        {
                            key.Pins.Toggle(w, p1, p2);
                            continue;
                        }
                        key.UpdateDecryption(w, p1, p2);

                        //newScore = key.Eval(evalType);
                        newScore = attackManager.Evaluate(evalType, key.Decryption, key.CribArray);
                        if (SimulatedAnnealing.Accept(newScore, currLocalScore, temperature))
                        {
                            currLocalScore = newScore;
                            changeAccepted = true;
                            if (newScore > bestSAScore)
                            {
                                key.Pins.Get(bestSAPins);
                                bestSAScore = newScore;
                            }
                            attackManager.AddNewBestListEntry(currLocalScore, key, key.Decryption);
                        }
                        else
                        {
                            key.Pins.Toggle(w, p1, p2);
                            key.UpdateDecryption(w, p1, p2);
                        }

                    }
                }
            }

            if (changeAccepted)
            {
                return bestSAScore;
            }

            // T?
            int bestV = -1;
            double bestVscore = 0;
            for (int v = 0; v <= 63; v++)
            {
                key.Pins.InverseWheelBitmap(v);
                double score = key.Eval(evalType);
                score = attackManager.Evaluate(evalType, key.Decryption, key.CribArray);
                if (score > bestVscore)
                {
                    bestVscore = score;
                    bestV = v;
                }
                key.Pins.InverseWheelBitmap(v);
            }
            newScore = bestVscore;
            if (SimulatedAnnealing.Accept(newScore, currLocalScore, temperature))
            {
                currLocalScore = newScore;
                key.Pins.InverseWheelBitmap(bestV);
                changeAccepted = true;
                if (newScore > bestSAScore)
                {
                    key.Pins.Get(bestSAPins);
                    bestSAScore = newScore;
                }
                attackManager.AddNewBestListEntry(currLocalScore, key, key.Decryption);
            }

            return bestSAScore;

        }


        public static double SA(Key key, EvalType evalType, int cycles, M209AttackManager attackManager)
        {
            key.UpdateDecryptionIfInvalid();
            double bestSAScore = attackManager.Evaluate(evalType, key.Decryption, key.CribArray);
            bool[][] bestSAPins = key.Pins.CreateCopy();

            for (int cycle = 0; cycle < cycles; cycle++)
            {
                key.Pins.Randomize();

                for (double temperature = attackManager.SAParameters.StartTemperature; temperature >= attackManager.SAParameters.EndTemperature && !attackManager.ShouldStop; temperature /= attackManager.SAParameters.Decrement)
                {
                    bestSAScore = Step(key, evalType, bestSAPins, bestSAScore, temperature, attackManager);
                }
                double previous;
                do
                {
                    previous = bestSAScore;
                    bestSAScore = Step(key, evalType, bestSAPins, bestSAScore, 0.0, attackManager);
                } while (bestSAScore > previous && !attackManager.ShouldStop);
            }

            key.Pins.Set(bestSAPins);
            return bestSAScore;

        }
    }
}
