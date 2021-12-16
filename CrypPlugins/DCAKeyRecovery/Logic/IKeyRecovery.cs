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

namespace DCAKeyRecovery.Logic
{
    internal interface IKeyRecovery
    {
        event EventHandler<EventArgs> NeedMessagePairOccured;

        event EventHandler<ResultViewAnyRoundEventArgs> AnyRoundResultViewRefreshOccured;
        event EventHandler<ResultViewAnyRoundKeyResultEventArgs> AnyRoundResultViewKeyResultsRefreshOccured;

        event EventHandler<ResultViewLastRoundRoundResultEventArgs> ResultViewRefreshRoundFinishedOccured;
        event EventHandler<ResultViewLastRoundEventArgs> LastRoundResultViewRefreshOccured;

        event EventHandler<ProgressEventArgs> ProgressChangedOccured;

        int CalculateLoopBorder(bool[] activeSBoxes);
        ushort GenerateValue(bool[] activeSBoxes, ushort data);
        void AddNewPairs(Pair plaintext, Pair cipherText);
        DifferentialAttackLastRoundResult AttackFirstRound(DifferentialKeyRecoveryAttack attack);
        ushort ReverseSBoxBlock(ushort data);
        ushort ApplyPBoxToBlock(ushort data);
        ushort ReversePBoxBlock(ushort data);
        ushort SBox(ushort data);
        DifferentialAttackRoundResult RecoverKeyInformation(DifferentialKeyRecoveryAttack attack, DifferentialAttackRoundConfiguration configuration);
        ushort PartialDecrypt(DifferentialKeyRecoveryAttack attack, ushort block);
        ushort DecryptSingleRound(ushort block, ushort key, bool beforeLast, bool isLast);
    }
}
