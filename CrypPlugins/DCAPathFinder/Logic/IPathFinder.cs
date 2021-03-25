/*
   Copyright 2019 Christian Bender christian1.bender@student.uni-siegen.de

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

using System;
using System.Collections.Generic;

namespace DCAPathFinder.Logic
{
    interface IPathFinder
    {
        event EventHandler<SearchResult> AttackSearchResultOccured;
        event EventHandler<ProgressEventArgs> ProgressChangedOccured;
        UInt16 ApplySingleSBox(UInt16 data);
        int CalculateLoopBorder(bool[] activeSBoxes);
        UInt16 GenerateValue(bool[] activeSBoxes, UInt16 data);
        UInt16 GetSubBlockFromBlock(UInt16 block, UInt16 subblockNum);
        UInt16 ReversePBoxBlock(UInt16 data);
        List<Differential> CountDifferentialsSingleSBox();
        DifferentialAttackRoundConfiguration GenerateConfigurationAttack(int round, bool[] sBoxesToAttack, bool useOfflinePaths,
            AbortingPolicy abortingPolicy, SearchPolicy searchPolicy, List<Differential> diffListOfSBox);
        List<Characteristic>[] FindAllCharacteristicsDepthSearch(DifferentialAttackRoundConfiguration roundConfiguration,
            List<Differential> differentialsList);
        List<Characteristic> FindBestCharacteristicsHeuristic(DifferentialAttackRoundConfiguration roundConfiguration,
            List<Differential> differentialsList);
        List<Characteristic> FindSpecifiedCharacteristicsDepthSearch(UInt16 inputDiff, UInt16 outputDiff, UInt16 round,
            List<Differential> differentialNumList);
        List<Characteristic> FindBestCharacteristicsDepthSearch(DifferentialAttackRoundConfiguration roundConfiguration,
            List<Differential> differentialsList, AbortingPolicy abortingPolicy);
    }
}