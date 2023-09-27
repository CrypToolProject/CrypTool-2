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
        public static readonly int[][] changes4Types = {
            new int[] { -1, -1, 1, 1, },
            new int[] { -1, 1, -1, 1, },
            new int[] { -1, 1, 1, -1, },
            new int[] { 1, -1, -1, 1, },
            new int[] { 1, -1, 1, -1, },
            new int[] { 1, 1, -1, -1, },
        };

        public static readonly int[][] changes3Types = {
            new int[] { -2, 1, 1, },
            new int[] { 1, -2, 1, },
            new int[] { 1, 1, -2, },

            new int[] { 2, -1, -1, },
            new int[] { -1, 2, -1, },
            new int[] { -1, -1, 2, },
        };

        public static readonly int[][] changes2Types = {
            new int[] { -1, 1},
            new int[] { 1, -1,},
            new int[] { -2, 2,},
            new int[] { 2, -2,},
        };

        static double Eval(Key key, EvalType evalType, M209AttackManager attackManager)
        {

            if (evalType == EvalType.PINS_SA_CRIB)
            {
                return SimulatedAnnealingPins.SA(key, EvalType.CRIB, 10, attackManager);
            }
            key.UpdateDecryptionIfInvalid();
            return attackManager.Evaluate(evalType, key.Decryption, key.CribArray);
        }

        public static double HillClimb(Key key, EvalType evalType, M209AttackManager attackManager, LocalState localState)
        {

            localState.BestTypeCount = key.Lugs.CreateTypeCountCopy();
            localState.BestPins = key.Pins.CreateCopy();
            localState.BestScore = Eval(key, evalType, attackManager);

            do
            {
                localState.Improved = false;

                Change2Types(key, evalType, attackManager, localState);

                if (localState.Quick)
                {
                    continue;
                }

                if (!localState.Improved)
                {
                    Changes4Types(key, evalType, attackManager, localState);
                }

                if (!localState.Improved)
                {
                    Change3Types(key, evalType, attackManager, localState);
                }


            } while (localState.Improved && !localState.SingleIteration);

            key.Pins.Set(localState.BestPins);
            return localState.BestScore;

        }

        public static void Change2Types(Key key, EvalType evalType, M209AttackManager attackManager, LocalState localState)
        {
            int[] types = new int[4];

            double newEval;

            for (int c1Index = 0; c1Index < Lugs.TYPES.Length; c1Index++)
            {
                int c1 = Lugs.TYPES[c1Index];

                for (int c2Index = 0; c2Index < Lugs.TYPES.Length; c2Index++)
                {
                    int c2 = Lugs.TYPES[c2Index];

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
                        if (!key.Lugs.SetTypeCount(localState.BestTypeCount, false))
                        {
                            UndoChanges(localState.BestTypeCount, types, changes);
                            continue;
                        }
                        newEval = Eval(key, evalType, attackManager);
                        if (newEval > localState.BestScore)
                        {
                            localState.Improved = true;
                            localState.BestScore = newEval;
                            key.Pins.Get(localState.BestPins);
                            attackManager.AddNewBestListEntry(localState.BestScore, key, key.Decryption);
                            //ReportManager.ReportResult(task, roundLayers, layers, key, bestLocal, $"HC L. s{changes.Length}");
                        }
                        else
                        {
                            UndoChanges(localState.BestTypeCount, types, changes);
                            key.Lugs.SetTypeCount(localState.BestTypeCount, false);
                        }
                    }
                }
            }
        }

        public static void Changes4Types(Key key, EvalType evalType, M209AttackManager attackManager, LocalState localState)
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

                            foreach (var changes in changes4Types)
                            {
                                if (!DoChangesIfValid(localState.BestTypeCount, types, changes))
                                {
                                    continue;
                                }
                                if (!key.Lugs.SetTypeCount(localState.BestTypeCount, false))
                                {
                                    UndoChanges(localState.BestTypeCount, types, changes);
                                    continue;
                                }
                                newEval = Eval(key, evalType, attackManager);

                                if (newEval > localState.BestScore)
                                {
                                    localState.Improved = true;
                                    localState.BestScore = newEval;
                                    attackManager.AddNewBestListEntry(localState.BestScore, key, key.Decryption);
                                    //ReportManager.ReportResult(task, roundLayers, layers, key, bestLocal, $"HC L. s{changes.Length}");
                                    key.Pins.Get(localState.BestPins);
                                    break;
                                }
                                else
                                {
                                    UndoChanges(localState.BestTypeCount, types, changes);
                                    key.Lugs.SetTypeCount(localState.BestTypeCount, false);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void Change3Types(Key key, EvalType evalType, M209AttackManager attackManager, LocalState localState)
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

                        foreach (var changes in changes3Types)
                        {
                            if (!DoChangesIfValid(localState.BestTypeCount, types, changes))
                            {
                                continue;
                            }
                            if (!key.Lugs.SetTypeCount(localState.BestTypeCount, false))
                            {
                                UndoChanges(localState.BestTypeCount, types, changes);
                                continue;
                            }
                            newEval = Eval(key, evalType, attackManager);

                            if (newEval > localState.BestScore)
                            {
                                localState.Improved = true;
                                localState.BestScore = newEval;
                                attackManager.AddNewBestListEntry(localState.BestScore, key, key.Decryption);
                                //ReportManager.ReportResult(attackManager.TaskNr, attackManager.RoundLayers, attackManager.Layers, key, attackManager.BestLocal, $"HC L. s{changes.Length}");
                                key.Pins.Get(localState.BestPins);
                                break;
                            }
                            else
                            {
                                UndoChanges(localState.BestTypeCount, types, changes);
                                key.Lugs.SetTypeCount(localState.BestTypeCount, false);
                            }
                        }
                    }
                }
            }
        }

        /**
         *
         * @param typeCount
         * @param types
         * @param changes
         * @return
         */
        public static bool DoChangesIfValid(int[] typeCount, int[] types, int[] changes)
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

            int overlaps = Lugs.GetOverlaps(typeCount);

            if ((overlaps > Global.MAX_OVERLAP) || (overlaps < Global.MIN_OVERLAP) || (Global.VERSION == MachineVersion.SWEDISH && Common.Utils.Sum(typeCount) != Key.BARS))
            {
                UndoChanges(typeCount, types, changes);
                return false;
            }
            return true;
        }

        public static void UndoChanges(int[] typeCount, int[] types, int[] changes)
        {
            for (int i = 0; i < changes.Length; i++)
            {
                int change = changes[i];
                int type = types[i];
                typeCount[type] -= change;
            }
        }

        private static void DoChanges(int[] typeCount, int[] types, int[] changes)
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
