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

namespace M209AnalyzerLib.M209
{
    public class SimulatedAnnealingPins
    {
        private M209AttackManager _attackManager;
        private Key _key;
        private LocalState _localState;
        public SimulatedAnnealingPins(Key key, M209AttackManager attackManager, LocalState localState)
        {
            _key = key;
            _attackManager = attackManager;
            _localState = localState;
        }
        private double Step(EvalType evalType,
                                bool[][] bestSAPins, double bestSAScore,
                                double temperature)
        {
            double currLocalScore = 0;
            int MAX_COUNT = _key.Pins.MaxCount();
            int MIN_COUNT = _key.Pins.MinCount();

            double newScore;
            int count;

            bool changeAccepted = false;

            // T1
            for (int w = 1; w <= _key.WHEELS; w++)
            {
                for (int p = 0; p < _key.WHEELS_SIZE[w]; p++)
                {
                    _key.Pins.Toggle(w, p);
                    count = _key.Pins.CountActivePins();
                    if (count < MIN_COUNT || count > MAX_COUNT || _key.Pins.LongSeq(w, p))
                    {
                        _key.Pins.Toggle(w, p);
                        continue;
                    }
                    _key.UpdateDecryption(w, p);

                    newScore = _attackManager.Evaluate(evalType, _key.Decryption, _key.CribArray, _localState.TaskId);
                    if (_attackManager.SimulatedAnnealing.Accept(newScore, currLocalScore, temperature))
                    {
                        currLocalScore = newScore;
                        changeAccepted = true;
                        if (newScore > bestSAScore)
                        {
                            _key.Pins.Get(bestSAPins);
                            bestSAScore = newScore;
                        }
                        _attackManager.AddNewBestListEntry(currLocalScore, _key, _key.Decryption, _localState.TaskId);
                    }
                    else
                    {
                        _key.Pins.Toggle(w, p);
                        _key.UpdateDecryption(w, p);
                    }
                    if (_attackManager.ShouldStop)
                    {
                        return bestSAScore;
                    }
                }
            }

            // T4
            for (int w = 1; w <= _key.WHEELS; w++)
            {

                _key.Pins.Inverse(w);
                count = _key.Pins.CountActivePins();
                if (count < MIN_COUNT || count > MAX_COUNT)
                {
                    _key.Pins.Inverse(w);
                    continue;
                }
                _key.UpdateDecryptionIfInvalid();
                newScore = _attackManager.Evaluate(evalType, _key.Decryption, _key.CribArray, _localState.TaskId);
                if (_attackManager.SimulatedAnnealing.Accept(newScore, currLocalScore, temperature))
                {
                    currLocalScore = newScore;
                    changeAccepted = true;
                    if (newScore > bestSAScore)
                    {
                        _key.Pins.Get(bestSAPins);
                        bestSAScore = newScore;
                    }
                    _attackManager.AddNewBestListEntry(currLocalScore, _key, _key.Decryption, _localState.TaskId);

                }
                else
                {
                    _key.Pins.Inverse(w);
                }
                if (_attackManager.ShouldStop)
                {
                    return bestSAScore;
                }
            }

            if (changeAccepted | _attackManager.ShouldStop)
            {
                return bestSAScore;
            }

            // T2
            for (int w = 1; w <= _key.WHEELS; w++)
            {
                for (int p1 = 0; p1 < _key.WHEELS_SIZE[w]; p1++)
                {
                    for (int p2 = p1 + 1; p2 < _key.WHEELS_SIZE[w]; p2++)
                    {
                        if (_key.Pins.Compare(w, p1, p2))
                        {
                            continue;
                        }
                        // Because the two places have a different value, we do not check the count.

                        _key.Pins.Toggle(w, p1, p2);

                        if (_key.Pins.LongSeq(w, p1, p2))
                        {
                            _key.Pins.Toggle(w, p1, p2);
                            continue;
                        }
                        _key.UpdateDecryption(w, p1, p2);

                        newScore = _attackManager.Evaluate(evalType, _key.Decryption, _key.CribArray, _localState.TaskId);
                        if (_attackManager.SimulatedAnnealing.Accept(newScore, currLocalScore, temperature))
                        {
                            currLocalScore = newScore;
                            changeAccepted = true;
                            if (newScore > bestSAScore)
                            {
                                _key.Pins.Get(bestSAPins);
                                bestSAScore = newScore;
                            }
                            _attackManager.AddNewBestListEntry(currLocalScore, _key, _key.Decryption, _localState.TaskId);
                        }
                        else
                        {
                            _key.Pins.Toggle(w, p1, p2);
                            _key.UpdateDecryption(w, p1, p2);
                        }

                        if (_attackManager.ShouldStop)
                        {
                            return bestSAScore;
                        }
                    }
                }
            }

            if (changeAccepted | _attackManager.ShouldStop)
            {
                return bestSAScore;
            }

            // T?
            int bestV = -1;
            double bestVscore = 0;
            for (int v = 0; v <= 63; v++)
            {
                _key.Pins.InverseWheelBitmap(v);
                double score = _key.Eval(evalType);
                score = _attackManager.Evaluate(evalType, _key.Decryption, _key.CribArray, _localState.TaskId);
                if (score > bestVscore)
                {
                    bestVscore = score;
                    bestV = v;
                }
                _key.Pins.InverseWheelBitmap(v);
            }
            newScore = bestVscore;
            if (_attackManager.SimulatedAnnealing.Accept(newScore, currLocalScore, temperature))
            {
                currLocalScore = newScore;
                _key.Pins.InverseWheelBitmap(bestV);
                changeAccepted = true;
                if (newScore > bestSAScore)
                {
                    _key.Pins.Get(bestSAPins);
                    bestSAScore = newScore;
                }
                _attackManager.AddNewBestListEntry(currLocalScore, _key, _key.Decryption, _localState.TaskId);
            }

            return bestSAScore;

        }


        public double SA(EvalType evalType, int cycles)
        {
            _key.UpdateDecryptionIfInvalid();
            double bestSAScore = _attackManager.Evaluate(evalType, _key.Decryption, _key.CribArray, _localState.TaskId);
            bool[][] bestSAPins = _key.Pins.CreateCopy();

            for (int cycle = 0; cycle < cycles && !_attackManager.ShouldStop; cycle++)
            {
                _key.Pins.Randomize();

                for (double temperature = _attackManager.SAParameters.StartTemperature; temperature >= _attackManager.SAParameters.EndTemperature && !_attackManager.ShouldStop; temperature /= _attackManager.SAParameters.Decrement)
                {
                    bestSAScore = Step(evalType, bestSAPins, bestSAScore, temperature);
                }
                double previous;
                do
                {
                    previous = bestSAScore;
                    bestSAScore = Step(evalType, bestSAPins, bestSAScore, 0.0);
                } while (bestSAScore > previous && !_attackManager.ShouldStop);
            }

            _key.Pins.Set(bestSAPins);
            return bestSAScore;
        }
    }
}
