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

namespace DCAPathFinder.Logic.Cipher1
{
    internal class Cipher1PathFinder : IPathFinder
    {
        public event EventHandler<SearchResult> AttackSearchResultOccured;
        public event EventHandler<ProgressEventArgs> ProgressChangedOccured;

        public ushort ApplySingleSBox(ushort data)
        {
            throw new System.NotImplementedException();
        }

        public int CalculateLoopBorder(bool[] activeSBoxes)
        {
            throw new System.NotImplementedException();
        }

        public List<Differential> CountDifferentialsSingleSBox()
        {
            throw new System.NotImplementedException();
        }

        public List<Characteristic>[] FindAllCharacteristicsDepthSearch(
            DifferentialAttackRoundConfiguration roundConfiguration, List<Differential> differentialsList)
        {
            throw new System.NotImplementedException();
        }

        public List<Characteristic> FindBestCharacteristicsDepthSearch(
            DifferentialAttackRoundConfiguration roundConfiguration, List<Differential> differentialsList,
            AbortingPolicy abortingPolicy)
        {
            throw new System.NotImplementedException();
        }

        public List<Characteristic> FindBestCharacteristicsHeuristic(
            DifferentialAttackRoundConfiguration roundConfiguration, List<Differential> differentialsList)
        {
            throw new System.NotImplementedException();
        }

        public List<Characteristic> FindSpecifiedCharacteristicsDepthSearch(ushort inputDiff, ushort outputDiff,
            ushort round, List<Differential> differentialNumList)
        {
            throw new NotImplementedException();
        }

        public DifferentialAttackRoundConfiguration GenerateConfigurationAttack(int round, bool[] sBoxesToAttack, bool useOfflinePaths,
            AbortingPolicy abortingPolicy, SearchPolicy searchPolicy, List<Differential> diffListOfSBox)
        {
            throw new System.NotImplementedException();
        }

        public ushort GenerateValue(bool[] activeSBoxes, ushort data)
        {
            throw new System.NotImplementedException();
        }

        public ushort GetSubBlockFromBlock(ushort block, ushort subblockNum)
        {
            throw new System.NotImplementedException();
        }

        public ushort ReversePBoxBlock(ushort data)
        {
            throw new System.NotImplementedException();
        }
    }
}