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
    public class HillClimbPins
    {
        private static int MAX_COUNT;
        private static int MIN_COUNT;

        private Key _key;
        private M209AttackManager _attackManager;
        private LocalState _localState;

        public HillClimbPins(Key key, M209AttackManager attackManager, LocalState localState)
        {
            _key = key;
            _attackManager = attackManager;
            _localState = localState;
        }
        public double HillClimb(EvalType evalType, bool singleIteration)
        {

            _localState.BestScore = _key.Eval(evalType);
            _localState.BestScore = _attackManager.Evaluate(evalType, _key.Decryption, _key.CribArray, _localState.TaskId);
            _localState.BestPins = _key.Pins.CreateCopy();

            MAX_COUNT = _key.Pins.MaxCount();
            MIN_COUNT = _key.Pins.MinCount();

            int round = 0;
            do
            {
                _localState.Improved = false;

                Toggle1PinOn1Wheel(evalType);

                Inverse1Wheel(evalType);

                if (_localState.Improved)
                {
                    continue;
                }

                Toggle2PinsOn1Wheel(evalType);

                if (_localState.Improved)
                {
                    continue;
                }

                InverseWheelBitmap(evalType);

                round++;
                if (_attackManager.ShouldStop)
                {
                    return _localState.BestScore;
                }
            } while (_localState.Improved && !singleIteration && !_attackManager.ShouldStop);


            _key.Pins.Set(_localState.BestPins);
            return _localState.BestScore;

        }

        public void Toggle1PinOn1Wheel(EvalType evalType)
        {
            double newEval;
            int count;

            for (int wheel = 1; wheel <= _key.WHEELS; wheel++)
            {
                for (int pin = 0; pin < _key.WHEELS_SIZE[wheel]; pin++)
                {
                    _key.Pins.Toggle(wheel, pin);
                    count = _key.Pins.CountActivePins();
                    if (count <= MIN_COUNT || count >= MAX_COUNT || _key.Pins.LongSeq(wheel, pin))
                    {
                        _key.Pins.Toggle(wheel, pin);
                        continue;
                    }
                    _key.UpdateDecryption(wheel, pin);

                    newEval = _attackManager.Evaluate(evalType, _key.Decryption, _key.CribArray, _localState.TaskId);
                    if (newEval > _localState.BestScore)
                    {
                        _localState.BestScore = newEval;
                        _key.Pins.Get(_localState.BestPins);
                        _localState.Improved = true;
                        _attackManager.AddNewBestListEntry(_localState.BestScore, _key, _key.Decryption, _localState.TaskId);
                    }
                    else
                    {
                        _key.Pins.Toggle(wheel, pin);
                        _key.UpdateDecryption(wheel, pin);
                    }
                    if (_attackManager.ShouldStop)
                    {
                        return;
                    }
                }

                if (_localState.Improved)
                {
                    wheel--;
                    _localState.Improved = false;
                }
            }
        }

        public void Inverse1Wheel(EvalType evalType)
        {
            double newEval;
            int count;

            for (int wheel = 1; wheel <= _key.WHEELS; wheel++)
            {

                _key.Pins.Inverse(wheel);
                count = _key.Pins.CountActivePins();
                if (count <= MIN_COUNT || count >= MAX_COUNT)
                {
                    _key.Pins.Inverse(wheel);
                    continue;
                }

                newEval = _attackManager.Evaluate(evalType, _key.Decryption, _key.CribArray, _localState.TaskId);
                if (newEval > _localState.BestScore)
                {
                    _localState.BestScore = newEval;
                    _key.Pins.Get(_localState.BestPins);
                    _localState.Improved = true;
                    _attackManager.AddNewBestListEntry(_localState.BestScore, _key, _key.Decryption, _localState.TaskId);
                }
                else
                {
                    _key.Pins.Inverse(wheel);
                }

                if (_attackManager.ShouldStop)
                {
                    return;
                }
            }
        }

        public void Toggle2PinsOn1Wheel(EvalType evalType)
        {
            double newEval;

            for (int wheel = 1; wheel <= _key.WHEELS; wheel++)
            {
                for (int pin1 = 0; pin1 < _key.WHEELS_SIZE[wheel]; pin1++)
                {
                    for (int pin2 = pin1 + 1; pin2 < _key.WHEELS_SIZE[wheel]; pin2++)
                    {
                        if (_key.Pins.Compare(wheel, pin1, pin2))
                        {
                            continue;
                        }
                        // Because the two places have a different value, we do not check the count.
                        _key.Pins.Toggle(wheel, pin1, pin2);

                        if (_key.Pins.LongSeq(wheel, pin1, pin2))
                        {
                            _key.Pins.Toggle(wheel, pin1, pin2);
                            continue;
                        }
                        _key.UpdateDecryption(wheel, pin1, pin2);

                        newEval = _attackManager.Evaluate(evalType, _key.Decryption, _key.CribArray, _localState.TaskId);

                        if (newEval > _localState.BestScore)
                        {
                            _localState.BestScore = newEval;
                            _key.Pins.Get(_localState.BestPins);
                            _localState.Improved = true;
                            _attackManager.AddNewBestListEntry(_localState.BestScore, _key, _key.Decryption, _localState.TaskId);
                        }
                        else
                        {
                            _key.Pins.Toggle(wheel, pin1, pin2);
                            _key.UpdateDecryption(wheel, pin1, pin2);
                        }

                        if (_attackManager.ShouldStop)
                        {
                            return;
                        }
                    }
                }
            }
        }

        public void InverseWheelBitmap(EvalType evalType)
        {
            int bestV = -1;
            double bestVscore = 0;
            for (int v = 0; v <= 63; v++)
            {
                _key.Pins.InverseWheelBitmap(v);
                double score = _attackManager.Evaluate(evalType, _key.Decryption, _key.CribArray, _localState.TaskId);
                if (score > bestVscore)
                {
                    bestVscore = score;
                    bestV = v;
                }
                _key.Pins.InverseWheelBitmap(v);

                if (_attackManager.ShouldStop)
                {
                    return;
                }
            }
            if (bestVscore > _localState.BestScore)
            {
                _localState.BestScore = bestVscore;
                _key.Pins.InverseWheelBitmap(bestV);
                _key.Pins.Get(_localState.BestPins);
                _localState.Improved = true;
                _attackManager.AddNewBestListEntry(_localState.BestScore, _key, _key.Decryption, _localState.TaskId);
            }
        }

    }
}
