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
    public class HillClimbLugs
    {
        public readonly int[][] changes4Types = {
            new int[] { -1, -1, 1, 1, },
            new int[] { -1, 1, -1, 1, },
            new int[] { -1, 1, 1, -1, },
            new int[] { 1, -1, -1, 1, },
            new int[] { 1, -1, 1, -1, },
            new int[] { 1, 1, -1, -1, },
        };

        public readonly int[][] changes3Types = {
            new int[] { -2, 1, 1, },
            new int[] { 1, -2, 1, },
            new int[] { 1, 1, -2, },

            new int[] { 2, -1, -1, },
            new int[] { -1, 2, -1, },
            new int[] { -1, -1, 2, },
        };

        public readonly int[][] changes2Types = {
            new int[] { -1, 1},
            new int[] { 1, -1,},
            new int[] { -2, 2,},
            new int[] { 2, -2,},
        };

        private M209AttackManager _attackManager;
        private Key _key;

        public HillClimbLugs(Key key, M209AttackManager attackManager, LocalState localState)
        {
            _attackManager = attackManager;
            _key = key;
        }

        double Eval(EvalType evalType, M209AttackManager attackManager, LocalState localState)
        {

            if (evalType == EvalType.PINS_SA_CRIB)
            {
                if (_attackManager.SimulatedAnnealingPins == null)
                {
                    _attackManager.SimulatedAnnealingPins = new SimulatedAnnealingPins(_key, attackManager, localState);
                }
                return _attackManager.SimulatedAnnealingPins.SA(EvalType.CRIB, 10);
            }
            _key.UpdateDecryptionIfInvalid();
            return attackManager.Evaluate(evalType, _key.Decryption, _key.CribArray, localState.TaskId);
        }

        public double HillClimb(EvalType evalType, M209AttackManager attackManager, LocalState localState)
        {

            localState.BestTypeCount = _key.Lugs.CreateTypeCountCopy();
            localState.BestPins = _key.Pins.CreateCopy();
            localState.BestScore = Eval(evalType, attackManager, localState);

            do
            {
                localState.Improved = false;

                Change2Types(evalType, attackManager, localState);
                if (attackManager.ShouldStop)
                {
                    return localState.BestScore;
                }

                if (localState.Quick)
                {
                    continue;
                }

                if (!localState.Improved)
                {
                    Changes4Types(evalType, attackManager, localState);
                    if (attackManager.ShouldStop)
                    {
                        return localState.BestScore;
                    }
                }

                if (!localState.Improved)
                {
                    Change3Types(evalType, attackManager, localState);
                    if (attackManager.ShouldStop)
                    {
                        return localState.BestScore;
                    }
                }

                if (attackManager.ShouldStop)
                {
                    return localState.BestScore;
                }


            } while (localState.Improved && !localState.SingleIteration && !attackManager.ShouldStop);

            _key.Pins.Set(localState.BestPins);
            return localState.BestScore;

        }

        public void Change2Types(EvalType evalType, M209AttackManager attackManager, LocalState localState)
        {
            int[] types = new int[4];

            double newEval;

            for (int c1Index = 0; c1Index < _key.Lugs.TYPES.Length; c1Index++)
            {
                int c1 = _key.Lugs.TYPES[c1Index];

                for (int c2Index = 0; c2Index < _key.Lugs.TYPES.Length; c2Index++)
                {
                    int c2 = _key.Lugs.TYPES[c2Index];

                    if (c1 == c2)
                    {
                        continue;
                    }
                    types[0] = c1;
                    types[1] = c2;
                    foreach (int[] changes in changes2Types)
                    {

                        if (!DoChangesIfValid(localState.BestTypeCount, types, changes))
                        {
                            continue;
                        }
                        if (!_key.Lugs.SetTypeCount(localState.BestTypeCount, false))
                        {
                            UndoChanges(localState.BestTypeCount, types, changes);
                            continue;
                        }
                        newEval = Eval(evalType, attackManager, localState);
                        if (newEval > localState.BestScore)
                        {
                            localState.Improved = true;
                            localState.BestScore = newEval;
                            _key.Pins.Get(localState.BestPins);
                            attackManager.AddNewBestListEntry(localState.BestScore, _key, _key.Decryption, localState.TaskId);
                        }
                        else
                        {
                            UndoChanges(localState.BestTypeCount, types, changes);
                            _key.Lugs.SetTypeCount(localState.BestTypeCount, false);
                        }

                        if (attackManager.ShouldStop)
                        {
                            return;
                        }
                    }
                }
            }
        }

        public void Changes4Types(EvalType evalType, M209AttackManager attackManager, LocalState localState)
        {
            int[] types = new int[4];

            double newEval;

            for (int i1 = 0; i1 < _key.Lugs.TYPES.Length && !localState.Improved; i1++)
            {
                types[0] = _key.Lugs.TYPES[i1];
                for (int i2 = i1 + 1; i2 < _key.Lugs.TYPES.Length && !localState.Improved; i2++)
                {
                    types[1] = _key.Lugs.TYPES[i2];
                    for (int i3 = i2 + 1; i3 < _key.Lugs.TYPES.Length && !localState.Improved; i3++)
                    {
                        types[2] = _key.Lugs.TYPES[i3];
                        for (int i4 = i3 + 1; i4 < _key.Lugs.TYPES.Length && !localState.Improved; i4++)
                        {
                            types[3] = _key.Lugs.TYPES[i4];

                            foreach (var changes in changes4Types)
                            {
                                if (!DoChangesIfValid(localState.BestTypeCount, types, changes))
                                {
                                    continue;
                                }
                                if (!_key.Lugs.SetTypeCount(localState.BestTypeCount, false))
                                {
                                    UndoChanges(localState.BestTypeCount, types, changes);
                                    continue;
                                }
                                newEval = Eval(evalType, attackManager, localState);

                                if (newEval > localState.BestScore)
                                {
                                    localState.Improved = true;
                                    localState.BestScore = newEval;
                                    attackManager.AddNewBestListEntry(localState.BestScore, _key, _key.Decryption, localState.TaskId);
                                    _key.Pins.Get(localState.BestPins);
                                    break;
                                }
                                else
                                {
                                    UndoChanges(localState.BestTypeCount, types, changes);
                                    _key.Lugs.SetTypeCount(localState.BestTypeCount, false);
                                }
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

        public void Change3Types(EvalType evalType, M209AttackManager attackManager, LocalState localState)
        {
            int[] types = new int[4];

            double newEval;

            for (int i1 = 0; i1 < _key.Lugs.TYPES.Length && !localState.Improved; i1++)
            {
                types[0] = _key.Lugs.TYPES[i1];
                for (int i2 = i1 + 1; i2 < _key.Lugs.TYPES.Length && !localState.Improved; i2++)
                {
                    types[1] = _key.Lugs.TYPES[i2];
                    for (int i3 = i2 + 1; i3 < _key.Lugs.TYPES.Length && !localState.Improved; i3++)
                    {
                        types[2] = _key.Lugs.TYPES[i3];

                        foreach (var changes in changes3Types)
                        {
                            if (!DoChangesIfValid(localState.BestTypeCount, types, changes))
                            {
                                continue;
                            }
                            if (!_key.Lugs.SetTypeCount(localState.BestTypeCount, false))
                            {
                                UndoChanges(localState.BestTypeCount, types, changes);
                                continue;
                            }
                            newEval = Eval(evalType, attackManager, localState);

                            if (newEval > localState.BestScore)
                            {
                                localState.Improved = true;
                                localState.BestScore = newEval;
                                attackManager.AddNewBestListEntry(localState.BestScore, _key, _key.Decryption, localState.TaskId);
                                _key.Pins.Get(localState.BestPins);
                                break;
                            }
                            else
                            {
                                UndoChanges(localState.BestTypeCount, types, changes);
                                _key.Lugs.SetTypeCount(localState.BestTypeCount, false);
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

        public bool DoChangesIfValid(int[] typeCount, int[] types, int[] changes)
        {

            for (int i = 0; i < changes.Length; i++)
            {
                int change = changes[i];
                int type = types[i];
                int newCount = typeCount[type] + change;
                if ((newCount < 0) || (newCount > Global.MAX_KICK))
                {
                    return false;
                }
            }
            DoChanges(typeCount, types, changes);

            int overlaps = _key.Lugs.GetOverlaps(typeCount);

            if ((overlaps > Global.MAX_OVERLAP) || (overlaps < Global.MIN_OVERLAP) || (Global.VERSION == MachineVersion.SWEDISH && Common.Utils.Sum(typeCount) != _key.BARS))
            {
                UndoChanges(typeCount, types, changes);
                return false;
            }
            return true;
        }

        public void UndoChanges(int[] typeCount, int[] types, int[] changes)
        {
            for (int i = 0; i < changes.Length; i++)
            {
                int change = changes[i];
                int type = types[i];
                typeCount[type] -= change;
            }
        }

        private void DoChanges(int[] typeCount, int[] types, int[] changes)
        {
            for (int i = 0; i < changes.Length; i++)
            {
                int change = changes[i];
                int type = types[i];
                typeCount[type] += change;
            }
        }

    }
}
