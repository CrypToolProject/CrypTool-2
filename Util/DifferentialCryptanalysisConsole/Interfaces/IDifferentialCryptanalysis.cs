using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Interfaces
{
    public interface IDifferentialCryptanalysis
    {
        int BuildBlockFromPartialBlocks(int pb3, int pb2, int pb1, int pb0);
        int GetSubBlockFromBlock(int block, int subblockNum);
        int DecryptSingleRound(int block, int key, bool beforeLast, bool isLast);
        DifferentialAttackRoundResult RecoverKeyInformation(DifferentialKeyRecoveryAttack attack, DifferentialAttackRoundConfiguration configuration, IEncryption encryption);
        int ReverseSBoxBlock(int data);
        int ReversePBoxBlock(int data);
        int ApplySingleSBox(int data);
        int ApplyPBoxToBlock(int data);
        int ApplySBoxToBlock(int data);
        BitArray GetBitsOfInt(int value);
        int MergeBytes(int pb3, int pb2, int pb1, int pb0);
        List<Characteristic>[] FindAllDifferentialsDepthSearch(DifferentialAttackRoundConfiguration roundConfiguration, List<SBoxCharacteristic> differentialsList);
        List<Characteristic> FindBestCharacteristicsHeuristic(DifferentialAttackRoundConfiguration roundConfiguration, List<SBoxCharacteristic> differentialsList);
        List<Characteristic> FindSpecifiedDifferentialDepthSearch(int inputDiff, int outputDiff, int round, List<SBoxCharacteristic> differentialNumList);
        List<Characteristic> FindBestCharacteristicsDepthSearch(DifferentialAttackRoundConfiguration roundConfiguration, List<SBoxCharacteristic> differentialsList);
        List<SBoxCharacteristic> CountDifferentialsSingleSBox();
        List<Pair> FilterPairs(DifferentialAttackRoundConfiguration roundConfig, List<SBoxCharacteristic> diffListOfSBox, DifferentialKeyRecoveryAttack attack, IEncryption encryption, int expectedDifferential);
        int PartialDecrypt(DifferentialKeyRecoveryAttack attack, int block);
        List<Pair> GenerateInputPairList(int inputDifferential, int count);
        DifferentialAttackRoundConfiguration GenerateConfigurationAttack(int round, bool[] sBoxesToAttack, AbortingPolicy abortingPolicy, SearchPolicy searchPolicy ,List<SBoxCharacteristic> diffListOfSBox, DifferentialKeyRecoveryAttack attack, IEncryption encryption);
        DifferentialAttackLastRoundResult AttackFirstRound(DifferentialKeyRecoveryAttack attack, IEncryption encryption);
        DifferentialAttackRoundConfiguration RefreshPairLists(DifferentialAttackRoundConfiguration roundConfig, List<SBoxCharacteristic> diffListOfSBox, DifferentialKeyRecoveryAttack attack, IEncryption encryption);
        string PrintKeyBits(int key);
        int CalculateLoopBorder(bool[] activeSBoxes);
        int GenerateValue(bool[] activeSBoxes, int data);

    }
}
