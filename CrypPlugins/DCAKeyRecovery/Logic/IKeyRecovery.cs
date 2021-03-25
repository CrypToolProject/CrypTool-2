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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DCAKeyRecovery.Logic
{
    interface IKeyRecovery
    {
        event EventHandler<EventArgs> NeedMessagePairOccured;

        event EventHandler<ResultViewAnyRoundEventArgs> AnyRoundResultViewRefreshOccured;
        event EventHandler<ResultViewAnyRoundKeyResultEventArgs> AnyRoundResultViewKeyResultsRefreshOccured;

        event EventHandler<ResultViewLastRoundRoundResultEventArgs> ResultViewRefreshRoundFinishedOccured;
        event EventHandler<ResultViewLastRoundEventArgs> LastRoundResultViewRefreshOccured;

        event EventHandler<ProgressEventArgs> ProgressChangedOccured;

        int CalculateLoopBorder(bool[] activeSBoxes);
        UInt16 GenerateValue(bool[] activeSBoxes, UInt16 data);
        void AddNewPairs(Pair plaintext, Pair cipherText);
        DifferentialAttackLastRoundResult AttackFirstRound(DifferentialKeyRecoveryAttack attack);
        UInt16 ReverseSBoxBlock(UInt16 data);
        UInt16 ApplyPBoxToBlock(UInt16 data);
        UInt16 ReversePBoxBlock(UInt16 data);
        UInt16 SBox(UInt16 data);
        DifferentialAttackRoundResult RecoverKeyInformation(DifferentialKeyRecoveryAttack attack, DifferentialAttackRoundConfiguration configuration);
        UInt16 PartialDecrypt(DifferentialKeyRecoveryAttack attack, UInt16 block);
        UInt16 DecryptSingleRound(UInt16 block, UInt16 key, bool beforeLast, bool isLast);
    }
}
