using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfaces;

namespace HeysToyCipher
{
    class Analysis : IDifferentialCryptanalysis
    {
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly Random _random = new Random();

        public int ApplySBoxToBlock(int data)
        {
            BitArray bitsOfBlock = new BitArray(BitConverter.GetBytes(data));

            BitArray zeroToThree = new BitArray(4);
            BitArray fourToSeven = new BitArray(4);
            BitArray eightToEleven = new BitArray(4);
            BitArray twelveToFifteen = new BitArray(4);

            for (int i = 0; i < 4; i++)
            {
                zeroToThree[i] = bitsOfBlock[i];
                fourToSeven[i] = bitsOfBlock[i + 4];
                eightToEleven[i] = bitsOfBlock[i + 8];
                twelveToFifteen[i] = bitsOfBlock[i + 12];
            }

            byte[] zeroToThreeBytes = new byte[2];
            zeroToThree.CopyTo(zeroToThreeBytes, 0);

            byte[] fourToSevenBytes = new byte[2];
            fourToSeven.CopyTo(fourToSevenBytes, 0);

            byte[] eightToElevenBytes = new byte[2];
            eightToEleven.CopyTo(eightToElevenBytes, 0);

            byte[] twelveToFifteenBytes = new byte[2];
            twelveToFifteen.CopyTo(twelveToFifteenBytes, 0);

            int zeroToThreeInt = BitConverter.ToInt16(zeroToThreeBytes, 0);
            int fourToSevenInt = BitConverter.ToInt16(fourToSevenBytes, 0);
            int eightToElevenInt = BitConverter.ToInt16(eightToElevenBytes, 0);
            int twelveToFifteenInt = BitConverter.ToInt16(twelveToFifteenBytes, 0);

            //use sbox
            zeroToThreeInt = HeysToyCipherConfiguration.SBOX[zeroToThreeInt];
            fourToSevenInt = HeysToyCipherConfiguration.SBOX[fourToSevenInt];
            eightToElevenInt = HeysToyCipherConfiguration.SBOX[eightToElevenInt];
            twelveToFifteenInt = HeysToyCipherConfiguration.SBOX[twelveToFifteenInt];

            bitsOfBlock = new BitArray(16);

            zeroToThree = new BitArray(BitConverter.GetBytes(zeroToThreeInt));
            fourToSeven = new BitArray(BitConverter.GetBytes(fourToSevenInt));
            eightToEleven = new BitArray(BitConverter.GetBytes(eightToElevenInt));
            twelveToFifteen = new BitArray(BitConverter.GetBytes(twelveToFifteenInt));

            for (int i = 0; i < 4; i++)
            {
                bitsOfBlock[i] = zeroToThree[i];
                bitsOfBlock[4 + i] = fourToSeven[i];
                bitsOfBlock[8 + i] = eightToEleven[i];
                bitsOfBlock[12 + i] = twelveToFifteen[i];
            }

            byte[] bytes = new byte[4];
            bitsOfBlock.CopyTo(bytes, 0);
            int combined = BitConverter.ToInt32(bytes, 0);

            return combined;
        }

        public int ApplySingleSBox(int data)
        {
            BitArray bitsOfBlock = new BitArray(BitConverter.GetBytes(data));
            BitArray zeroToThree = new BitArray(4);

            for (int i = 0; i < 4; i++)
            {
                zeroToThree[i] = bitsOfBlock[i];
            }

            byte[] zeroToThreeBytes = new byte[2];
            zeroToThree.CopyTo(zeroToThreeBytes, 0);

            int zeroToThreeInt = BitConverter.ToInt16(zeroToThreeBytes, 0);

            //use sbox
            zeroToThreeInt = HeysToyCipherConfiguration.SBOX[zeroToThreeInt];

            return zeroToThreeInt;
        }

        /// <summary>
        /// Returns a single block from the given partial blocks
        /// </summary>
        /// <param name="pb3"></param>
        /// <param name="pb2"></param>
        /// <param name="pb1"></param>
        /// <param name="pb0"></param>
        /// <returns></returns>
        public int BuildBlockFromPartialBlocks(int pb3, int pb2, int pb1, int pb0)
        {
            BitArray zeroToThree = new BitArray(BitConverter.GetBytes(pb0));
            BitArray fourToSeven = new BitArray(BitConverter.GetBytes(pb1));
            BitArray eightToEleven = new BitArray(BitConverter.GetBytes(pb2));
            BitArray twelveToFifteen = new BitArray(BitConverter.GetBytes(pb3));

            BitArray resultBits = new BitArray(16);

            for (int i = 0; i < 4; i++)
            {
                resultBits[i] = zeroToThree[i];
                resultBits[i + 4] = fourToSeven[i];
                resultBits[i + 8] = eightToEleven[i];
                resultBits[i + 12] = twelveToFifteen[i];
            }

            byte[] resultBytes = new byte[4];
            resultBits.CopyTo(resultBytes, 0);

            int resultInt = BitConverter.ToInt32(resultBytes, 0);
            return resultInt;
        }

        /// <summary>
        /// Counts the differentials for a single sbox
        /// </summary>
        /// <returns></returns>
        public List<SBoxCharacteristic> CountDifferentialsSingleSBox()
        {
            List<SBoxCharacteristic> result = new List<SBoxCharacteristic>();
            SBoxCharacteristic diffToEdit = null;

            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    int inputDiff = i ^ j;
                    int outputDiff = ApplySingleSBox(i) ^ ApplySingleSBox(j);

                    bool found = false;

                    foreach (var curDiff in result)
                    {
                        if (curDiff.InputDifferential == inputDiff && curDiff.OutputDifferential == outputDiff)
                        {
                            curDiff.Count++;
                            found = true;
                            diffToEdit = curDiff;

                            break;
                        }
                    }

                    if (!found)
                    {
                        result.Add(new SBoxCharacteristic()
                        {
                            Count = 1,
                            InputDifferential = inputDiff,
                            OutputDifferential = outputDiff,
                            InputPairList = new List<Pair>()
                            {
                                new Pair(){LeftMember = i, RightMember = j}
                            },
                            OutputPairList = new List<Pair>()
                            {
                                new Pair(){LeftMember = ApplySingleSBox(i), RightMember = ApplySingleSBox(j)}
                            }
                        });
                    }
                    else
                    {
                        diffToEdit.InputPairList.Add(new Pair() { LeftMember = i, RightMember = j });
                        diffToEdit.OutputPairList.Add(new Pair() { LeftMember = ApplySingleSBox(i), RightMember = ApplySingleSBox(j) });
                    }
                }
            }

            foreach (var curDiff in result)
            {
                curDiff.Probability = curDiff.Count / 16.0;
            }

            return result;
        }

        public int GetSubBlockFromBlock(int block, int subblockNum)
        {
            BitArray bitsOfBlock = new BitArray(BitConverter.GetBytes(block));
            BitArray resultBits = new BitArray(4);

            switch (subblockNum)
            {
                case 0:
                {
                    for (int i = 0; i < 4; i++)
                    {
                        resultBits[i] = bitsOfBlock[i];
                    }
                }
                    break;
                case 1:
                {
                    for (int i = 0; i < 4; i++)
                    {
                        resultBits[i] = bitsOfBlock[i + 4];
                    }
                }
                    break;
                case 2:
                {
                    for (int i = 0; i < 4; i++)
                    {
                        resultBits[i] = bitsOfBlock[i + 8];
                    }
                }
                    break;
                case 3:
                {
                    for (int i = 0; i < 4; i++)
                    {
                        resultBits[i] = bitsOfBlock[i + 12];
                    }
                }
                    break;
            }

            byte[] resultBytes = new byte[4];
            resultBits.CopyTo(resultBytes, 0);

            int resultInt = BitConverter.ToInt32(resultBytes, 0);
            return resultInt;
        }

        /// <summary>
        /// Reverses the permutation on a block
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int ReversePBoxBlock(int data)
        {
            BitArray bitsOfBlock = new BitArray(BitConverter.GetBytes(data));
            BitArray pboxedArray = new BitArray(16);

            //use pbox
            for (int i = 0; i < 16; i++)
            {
                pboxedArray[HeysToyCipherConfiguration.PBOXREVERSE[i]] = bitsOfBlock[i];
            }

            byte[] bytes = new byte[4];
            pboxedArray.CopyTo(bytes, 0);

            int outputBlock = BitConverter.ToInt32(bytes, 0);
            return outputBlock;
        }

        /// <summary>
        /// Reverses the sboxes on a block
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int ReverseSBoxBlock(int data)
        {
            BitArray bitsOfBlock = new BitArray(BitConverter.GetBytes(data));

            BitArray zeroToThree = new BitArray(4);
            BitArray fourToSeven = new BitArray(4);
            BitArray eightToEleven = new BitArray(4);
            BitArray twelveToFifteen = new BitArray(4);

            for (int i = 0; i < 4; i++)
            {
                zeroToThree[i] = bitsOfBlock[i];
                fourToSeven[i] = bitsOfBlock[i + 4];
                eightToEleven[i] = bitsOfBlock[i + 8];
                twelveToFifteen[i] = bitsOfBlock[i + 12];
            }

            byte[] zeroToThreeBytes = new byte[4];
            zeroToThree.CopyTo(zeroToThreeBytes, 0);

            byte[] fourToSevenBytes = new byte[4];
            fourToSeven.CopyTo(fourToSevenBytes, 0);

            byte[] eightToElevenBytes = new byte[4];
            eightToEleven.CopyTo(eightToElevenBytes, 0);

            byte[] twelveToFifteenBytes = new byte[4];
            twelveToFifteen.CopyTo(twelveToFifteenBytes, 0);

            int zeroToThreeInt = BitConverter.ToInt32(zeroToThreeBytes, 0);
            int fourToSevenInt = BitConverter.ToInt32(fourToSevenBytes, 0);
            int eightToElevenInt = BitConverter.ToInt32(eightToElevenBytes, 0);
            int twelveToFifteenInt = BitConverter.ToInt32(twelveToFifteenBytes, 0);

            //use sbox
            zeroToThreeInt = HeysToyCipherConfiguration.SBOXREVERSE[zeroToThreeInt];
            fourToSevenInt = HeysToyCipherConfiguration.SBOXREVERSE[fourToSevenInt];
            eightToElevenInt = HeysToyCipherConfiguration.SBOXREVERSE[eightToElevenInt];
            twelveToFifteenInt = HeysToyCipherConfiguration.SBOXREVERSE[twelveToFifteenInt];

            bitsOfBlock = new BitArray(16);

            zeroToThree = new BitArray(BitConverter.GetBytes(zeroToThreeInt));
            fourToSeven = new BitArray(BitConverter.GetBytes(fourToSevenInt));
            eightToEleven = new BitArray(BitConverter.GetBytes(eightToElevenInt));
            twelveToFifteen = new BitArray(BitConverter.GetBytes(twelveToFifteenInt));

            for (int i = 0; i < 4; i++)
            {
                bitsOfBlock[i] = zeroToThree[i];
                bitsOfBlock[4 + i] = fourToSeven[i];
                bitsOfBlock[8 + i] = eightToEleven[i];
                bitsOfBlock[12 + i] = twelveToFifteen[i];
            }

            byte[] bytes = new byte[4];
            bitsOfBlock.CopyTo(bytes, 0);
            int combined = BitConverter.ToInt32(bytes, 0);

            return combined;
        }

        public int DecryptSingleRound(int block, int key, bool beforeLast, bool isLast)
        {
            int result = block;

            //if is last round
            if (isLast)
            {
                //use the key to decrypt
                result = result ^ key;
#if DEBUG
                //Console.WriteLine("Decrypted with k5: " + result);
#endif
                return result;
            }

            if (beforeLast)
            {
                result = ReverseSBoxBlock(result);
                result = result ^ key;
#if DEBUG
                //Console.WriteLine("Decrypted with k4: " + result);
#endif
                return result;
            }

            //use the pbox
            result = ReversePBoxBlock(result);

            //undo the sbox
            result = ReverseSBoxBlock(result);

            //use the key to decrypt
            result = result ^ key;
#if DEBUG
            //Console.WriteLine("Decrypted block: " + result);
#endif

            return result;
        }

        public DifferentialAttackRoundResult RecoverKeyInformation(DifferentialKeyRecoveryAttack attack, DifferentialAttackRoundConfiguration configuration, IEncryption encryption)
        {
            if (configuration.ActiveSBoxes == null)
            {
                throw new ArgumentException("activeSBoxes should contain at least one active SBox");
            }

            DifferentialAttackRoundResult roundResult = new DifferentialAttackRoundResult();

            //Generate border for the loop
            int loopBorder = CalculateLoopBorder(configuration.ActiveSBoxes);

            for(int i = 0; i < loopBorder; i++)
            //Parallel.For(0, loopBorder, i =>
            {
                int guessedKey = GenerateValue(configuration.ActiveSBoxes, i);

                if (!configuration.IsLast)
                {
                    guessedKey = ApplyPBoxToBlock(guessedKey);
                }

                KeyProbability curTry = new KeyProbability() { Counter = 0, Key = guessedKey };

                foreach (var curPair in configuration.FilteredPairList)
                {
                    Pair encryptedPair = new Pair() { LeftMember = (curPair.LeftMember), RightMember = (curPair.RightMember) };

                    encryptedPair.LeftMember = PartialDecrypt(attack, encryptedPair.LeftMember);
                    encryptedPair.RightMember = PartialDecrypt(attack, encryptedPair.RightMember);

                    //reverse round with the guessed key
                    int leftMemberSingleDecrypted = DecryptSingleRound(encryptedPair.LeftMember, curTry.Key, configuration.IsBeforeLast, configuration.IsLast);
                    int rightMemberSingleDecrypted = DecryptSingleRound(encryptedPair.RightMember, curTry.Key, configuration.IsBeforeLast, configuration.IsLast);

                    if (configuration.IsLast)
                    {
                        leftMemberSingleDecrypted = ReverseSBoxBlock(leftMemberSingleDecrypted);
                        rightMemberSingleDecrypted = ReverseSBoxBlock(rightMemberSingleDecrypted);
                    }
                    else
                    {
                        leftMemberSingleDecrypted = ReversePBoxBlock(leftMemberSingleDecrypted);
                        leftMemberSingleDecrypted = ReverseSBoxBlock(leftMemberSingleDecrypted);

                        rightMemberSingleDecrypted = ReversePBoxBlock(rightMemberSingleDecrypted);
                        rightMemberSingleDecrypted = ReverseSBoxBlock(rightMemberSingleDecrypted);
                    }

                    int differentialToCompare = leftMemberSingleDecrypted ^ rightMemberSingleDecrypted;

                    if (differentialToCompare == configuration.ExpectedDifference)
                    {
                        curTry.Counter++;
                    }
                }

                //synchronize access to resultList
                _semaphoreSlim.Wait();
                try
                {
                    roundResult.KeyCandidateProbabilities.Add(curTry);
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
            }//);

            //sort by counter
            KeyProbability bestPossibleKey = new KeyProbability() { Counter = 0, Key = 0 };
            foreach (var curKey in roundResult.KeyCandidateProbabilities)
            {
                if (curKey.Counter > bestPossibleKey.Counter)
                {
                    bestPossibleKey = curKey;
                }
            }

            roundResult.PossibleKey = bestPossibleKey.Key;
            roundResult.Probability = bestPossibleKey.Counter / (double)configuration.UnfilteredPairList.Count;
            roundResult.ExpectedProbability = configuration.Probability;
            roundResult.KeyCandidateProbabilities = roundResult.KeyCandidateProbabilities.OrderByDescending(item => item.Counter).ToList();

#if DEBUG
            Console.WriteLine("Expected probability: {0:N4}" + " Expected count: " + (configuration.Probability * configuration.UnfilteredPairList.Count), configuration.Probability);
            foreach (var curKeyProbability in roundResult.KeyCandidateProbabilities)
            {
                Console.WriteLine("Guessed key: " + curKeyProbability.Key + " Count: " + curKeyProbability.Counter + " Probability: {0:N4}", ((curKeyProbability.Counter / (double)configuration.UnfilteredPairList.Count)));
            }
#endif

            return roundResult;
        }

        public int ApplyPBoxToBlock(int data)
        {
            BitArray bits = new BitArray(BitConverter.GetBytes(data));
            BitArray pboxedArray = new BitArray(16);

            //use pbox
            for (int i = 0; i < 16; i++)
            {
                pboxedArray[HeysToyCipherConfiguration.PBOX[i]] = bits[i];
            }

            byte[] bytes = new byte[4];
            pboxedArray.CopyTo(bytes, 0);

            int outputBlock = BitConverter.ToInt32(bytes, 0);

            return outputBlock;
        }

        public BitArray GetBitsOfInt(int value)
        {
            return new BitArray(BitConverter.GetBytes(value));
        }

        public int MergeBytes(int pb3, int pb2, int pb1, int pb0)
        {
            BitArray bitsZeroToThree = new BitArray(BitConverter.GetBytes(pb0));
            BitArray bitsFourToSeven = new BitArray(BitConverter.GetBytes(pb1));
            BitArray bitsEightToEleven = new BitArray(BitConverter.GetBytes(pb2));
            BitArray bitsTwelveToFifteen = new BitArray(BitConverter.GetBytes(pb3));

            BitArray combined = new BitArray(16);

            for (int i = 0; i < 4; i++)
            {
                combined[i] = bitsZeroToThree[i];
                combined[i + 4] = bitsFourToSeven[i + 4];
                combined[i + 8] = bitsEightToEleven[i + 8];
                combined[i + 12] = bitsTwelveToFifteen[i + 12];
            }

            byte[] bytesOfCombined = new byte[4];
            combined.CopyTo(bytesOfCombined, 0);

            return BitConverter.ToInt32(bytesOfCombined, 0);
        }

        public List<Characteristic> FindBestCharacteristicsHeuristic(DifferentialAttackRoundConfiguration roundConfiguration, List<SBoxCharacteristic> differentialsList)
        {
            throw new NotImplementedException();
        }

        private List<Characteristic> FindAllCharacteristics(int round, List<SBoxCharacteristic> differentialsList, int outputDiff, Characteristic res)
        {
            //break if probability is not good enough
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if ((res.Probability < HeysToyCipherConfiguration.PROBABILITYBOUNDDIFFERENTIALSEARCH) && (res.Probability != -1))
                return null;

            //end of rekursion
            if (round == 0)
            {
                List<Characteristic> resList = new List<Characteristic>();
                resList.Add(res);
                return resList;
            }

            //contains the active SBoxes in the round
            bool[] activeSBoxes = new bool[HeysToyCipherConfiguration.SBOXNUM];

            //check active SBoxes
            int[] outputDiffs = new int[HeysToyCipherConfiguration.SBOXNUM];
            for (int i = 0; i < HeysToyCipherConfiguration.SBOXNUM; i++)
            {
                outputDiffs[i] = GetSubBlockFromBlock(outputDiff, i);
                if (outputDiffs[i] > 0)
                {
                    activeSBoxes[i] = true;
                }
                else
                {
                    activeSBoxes[i] = false;
                }
            }

            //resultList
            List<Characteristic> diffList = new List<Characteristic>();

            //prepare the arrayOfDifferentialLists
            List<SBoxCharacteristic>[] arrayOfDifferentialLists = new List<SBoxCharacteristic>[HeysToyCipherConfiguration.SBOXNUM];
            int comb = 1;
            for (int b = 0; b < HeysToyCipherConfiguration.SBOXNUM; b++)
            {
                if (activeSBoxes[b])
                {
                    arrayOfDifferentialLists[b] = new List<SBoxCharacteristic>(differentialsList.Count);
                    differentialsList.ForEach((item) =>
                    {
                        arrayOfDifferentialLists[b].Add((SBoxCharacteristic)item.Clone());
                    });

                    List<SBoxCharacteristic> diffsToRemove = new List<SBoxCharacteristic>();
                    for (int j = 0; j < arrayOfDifferentialLists[b].Count; j++)
                    {
                        if (arrayOfDifferentialLists[b][j].OutputDifferential != outputDiffs[b])
                        {
                            diffsToRemove.Add(arrayOfDifferentialLists[b][j]);
                        }
                    }

                    foreach (var curDiff in diffsToRemove)
                    {
                        arrayOfDifferentialLists[b].Remove(curDiff);
                    }

                    comb *= arrayOfDifferentialLists[b].Count;
                }
                else
                {
                    arrayOfDifferentialLists[b] = new List<SBoxCharacteristic>();
                }
            }

            for (int c = 0; c < comb; c++)
            {
                SBoxCharacteristic[] curDiffSBoxes = new SBoxCharacteristic[HeysToyCipherConfiguration.SBOXNUM];

                //calc indices
                int indexNo = 0;
                int j = c;
                while (j > 0)
                {
                    if (arrayOfDifferentialLists[indexNo].Count > 0)
                    {
                        int index = j % arrayOfDifferentialLists[indexNo].Count;
                        j = j / arrayOfDifferentialLists[indexNo].Count;
                        curDiffSBoxes[indexNo] = arrayOfDifferentialLists[indexNo][index];
                    }
                    indexNo++;
                }

                //zero case
                if (c == 0)
                {
                    for (int i = 0; i < HeysToyCipherConfiguration.SBOXNUM; i++)
                    {
                        if (activeSBoxes[i])
                        {
                            curDiffSBoxes[i] = arrayOfDifferentialLists[i][0];
                        }
                    }
                }

                //check null values
                for (int z = 0; z < HeysToyCipherConfiguration.SBOXNUM; z++)
                {
                    if (curDiffSBoxes[z] == null)
                    {
                        curDiffSBoxes[z] = new SBoxCharacteristic()
                        {
                            Count = 0,
                            InputDifferential = 0,
                            OutputDifferential = 0,
                            Probability = -1
                        };
                    }
                }

                //calc conditions
                bool satisfied = true;
                for (int i = 0; i < HeysToyCipherConfiguration.SBOXNUM; i++)
                {
                    if (curDiffSBoxes[i].OutputDifferential != outputDiffs[i])
                    {
                        satisfied = false;
                    }
                }

                //check if conditions are satisfied
                if (!satisfied)
                {
                    continue;
                }

                //copy object
                Characteristic characteristic = res.Clone() as Characteristic;

                //calculate inputDifference
                int inputDiff = 0;
                for (int i = HeysToyCipherConfiguration.SBOXNUM - 1; i >= 0; i--)
                {
                    inputDiff = inputDiff ^ curDiffSBoxes[i].InputDifferential;
                    if ((i - 1) >= 0)
                    {
                        inputDiff = inputDiff << HeysToyCipherConfiguration.BITWIDTHCIPHERFOUR;
                    }
                }

                //outputDifference for previous round
                int outputDiffPreviousRound = ReversePBoxBlock(inputDiff);

                //calc new prob
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (characteristic != null && characteristic.Probability != -1)
                {
                    for (int i = 0; i < HeysToyCipherConfiguration.SBOXNUM; i++)
                    {
                        if (curDiffSBoxes[i].Count == 0)
                        {
                            continue;
                        }
                        characteristic.Probability = characteristic.Probability * (curDiffSBoxes[i].Count / 16.0);
                    }
                }
                else
                {
                    double value = 1.0;
                    for (int i = 0; i < HeysToyCipherConfiguration.SBOXNUM; i++)
                    {
                        if (curDiffSBoxes[i].Count == 0)
                        {
                            continue;
                        }
                        value = value * (curDiffSBoxes[i].Count / 16.0);
                    }

                    if (characteristic != null) characteristic.Probability = value;
                }

                //store result
                if (characteristic != null)
                {
                    characteristic.InputDifferentials[round - 1] = inputDiff;
                    characteristic.OutputDifferentials[round - 1] = outputDiff;

                    //go one round deeper
                    List<Characteristic> retval = FindAllCharacteristics(round - 1, differentialsList,
                        outputDiffPreviousRound, characteristic);

                    //check if there is a result
                    if (retval != null)
                    {
                        diffList.AddRange(retval);
                    }
                }
            }

            //search for the best result
            Characteristic best = new HeysToyCipherCharacteristic();
            foreach (var curDiffs in diffList)
            {
                if (best.Probability < curDiffs.Probability)
                {
                    best = curDiffs;
                }
            }

            return diffList;
        }

        public List<Characteristic> FindCharacteristics(int expectedDiff, int round, List<SBoxCharacteristic> differentialNumList)
        {
            //Decrement round
            round--;

            //Starting object
            Characteristic inputObj = new HeysToyCipherCharacteristic();
            inputObj.InputDifferentials[round] = expectedDiff;

            //calculate previous difference
            int outputDiffPreviousRound = ReversePBoxBlock(expectedDiff);

            //start depth-first search
            List<Characteristic> retVal = FindAllCharacteristics(round, differentialNumList, outputDiffPreviousRound, inputObj);

#if DEBUG
            Console.WriteLine("Found " + retVal.Count + " paths with expectedDifference = " + expectedDiff);
#endif

            return retVal;
        }

        public List<Characteristic> FindBestCharacteristicsDepthSearch(DifferentialAttackRoundConfiguration roundConfiguration, List<SBoxCharacteristic> differentialsList)
        {
            int round = roundConfiguration.Round;

            //Decrement round for recursive call
            round--;

            //result list
            List<Characteristic> resultList = new List<Characteristic>();

            //calculate loop border
            int loopBorder = CalculateLoopBorder(roundConfiguration.ActiveSBoxes);

            //for(int i = 1; i < loopBorder;i++)
            Parallel.For(1, loopBorder, i =>
            {
                Characteristic inputObj = new HeysToyCipherCharacteristic();

                //expected difference
                int expectedDifference = GenerateValue(roundConfiguration.ActiveSBoxes, i);
                int outputDifferencePreviousRound = ReversePBoxBlock(expectedDifference);

                inputObj.InputDifferentials[round] = expectedDifference;

                //start depth-first search
                Characteristic retVal = FindBestCharacteristic(round, differentialsList, outputDifferencePreviousRound, inputObj);

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (retVal.Probability != -1)
                {
                    _semaphoreSlim.Wait();
                    try
                    {
#if DEBUG
                        Console.WriteLine("Case " + roundConfiguration.GetActiveSBoxes() + " finished iteration i = " + i + " / " + loopBorder);
                        Console.WriteLine(retVal.ToString());
#endif
                        resultList.Add(retVal);
                    }
                    finally
                    {
                        _semaphoreSlim.Release();
                    }
                }

            });

            var sorted = resultList.OrderByDescending(elem => elem.Probability).ToList();
#if DEBUG
            foreach (var curRetVAl in sorted)
            {
                Console.WriteLine(curRetVAl.ToString());
            }
#endif
            return sorted;
        }

        private Characteristic FindBestCharacteristic(int round, List<SBoxCharacteristic> differentialsList, int outputDiff, Characteristic res)
        {
            //break if probability is not good enough
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if ((res.Probability < HeysToyCipherConfiguration.PROBABILITYBOUNDBESTCHARACTERISTICSEARCH) && (res.Probability != -1))
                return null;

            //end of rekursion
            if (round == 0)
                return res;

            //contains the active SBoxes in the round
            bool[] activeSBoxes = new bool[HeysToyCipherConfiguration.SBOXNUM];

            //check active SBoxes
            int[] outputDiffs = new int[HeysToyCipherConfiguration.SBOXNUM];
            for (int i = 0; i < HeysToyCipherConfiguration.SBOXNUM; i++)
            {
                outputDiffs[i] = GetSubBlockFromBlock(outputDiff, i);
                if (outputDiffs[i] > 0)
                {
                    activeSBoxes[i] = true;
                }
                else
                {
                    activeSBoxes[i] = false;
                }
            }

            //resultList
            List<Characteristic> diffList = new List<Characteristic>();

            //prepare the arrayOfDifferentialLists
            List<SBoxCharacteristic>[] arrayOfDifferentialLists = new List<SBoxCharacteristic>[HeysToyCipherConfiguration.SBOXNUM];
            int comb = 1;
            for (int b = 0; b < HeysToyCipherConfiguration.SBOXNUM; b++)
            {
                if (activeSBoxes[b])
                {
                    arrayOfDifferentialLists[b] = new List<SBoxCharacteristic>(differentialsList.Count);
                    differentialsList.ForEach((item) =>
                    {
                        arrayOfDifferentialLists[b].Add((SBoxCharacteristic)item.Clone());
                    });

                    List<SBoxCharacteristic> diffsToRemove = new List<SBoxCharacteristic>();
                    for (int j = 0; j < arrayOfDifferentialLists[b].Count; j++)
                    {
                        if (arrayOfDifferentialLists[b][j].OutputDifferential != outputDiffs[b])
                        {
                            diffsToRemove.Add(arrayOfDifferentialLists[b][j]);
                        }
                    }

                    foreach (var curDiff in diffsToRemove)
                    {
                        arrayOfDifferentialLists[b].Remove(curDiff);
                    }

                    comb *= arrayOfDifferentialLists[b].Count;
                }
                else
                {
                    arrayOfDifferentialLists[b] = new List<SBoxCharacteristic>();
                }
            }

            for (int c = 0; c < comb; c++)
            {
                SBoxCharacteristic[] curDiffSBoxes = new SBoxCharacteristic[HeysToyCipherConfiguration.SBOXNUM];

                //calc indices
                int indexNo = 0;
                int j = c;
                while (j > 0)
                {
                    if (arrayOfDifferentialLists[indexNo].Count > 0)
                    {
                        int index = j % arrayOfDifferentialLists[indexNo].Count;
                        j = j / arrayOfDifferentialLists[indexNo].Count;
                        curDiffSBoxes[indexNo] = arrayOfDifferentialLists[indexNo][index];
                    }
                    indexNo++;
                }

                //zero case
                if (c == 0)
                {
                    for (int i = 0; i < HeysToyCipherConfiguration.SBOXNUM; i++)
                    {
                        if (activeSBoxes[i])
                        {
                            curDiffSBoxes[i] = arrayOfDifferentialLists[i][0];
                        }
                    }
                }

                //check null values
                for (int z = 0; z < HeysToyCipherConfiguration.SBOXNUM; z++)
                {
                    if (curDiffSBoxes[z] == null)
                    {
                        curDiffSBoxes[z] = new SBoxCharacteristic()
                        {
                            Count = 0,
                            InputDifferential = 0,
                            OutputDifferential = 0,
                            Probability = -1
                        };
                    }
                }

                //calc conditions
                bool satisfied = true;
                for (int i = 0; i < HeysToyCipherConfiguration.SBOXNUM; i++)
                {
                    if (curDiffSBoxes[i].OutputDifferential != outputDiffs[i])
                    {
                        satisfied = false;
                    }
                }

                //check if conditions are satisfied
                if (!satisfied)
                {
                    continue;
                }

                //copy object
                Characteristic characteristic = res.Clone() as Characteristic;

                //calculate inputDifference
                int inputDiff = 0;
                for (int i = HeysToyCipherConfiguration.SBOXNUM - 1; i >= 0; i--)
                {
                    inputDiff = inputDiff ^ curDiffSBoxes[i].InputDifferential;
                    if ((i - 1) >= 0)
                    {
                        inputDiff = inputDiff << HeysToyCipherConfiguration.BITWIDTHCIPHERFOUR;
                    }
                }

                //outputDifference for previous round
                int outputDiffPreviousRound = ReversePBoxBlock(inputDiff);

                //calc new prob
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (characteristic != null && characteristic.Probability != -1)
                {
                    for (int i = 0; i < HeysToyCipherConfiguration.SBOXNUM; i++)
                    {
                        if (curDiffSBoxes[i].Count == 0)
                        {
                            continue;
                        }
                        characteristic.Probability = characteristic.Probability * (curDiffSBoxes[i].Count / 16.0);
                    }
                }
                else
                {
                    double value = 1.0;
                    for (int i = 0; i < HeysToyCipherConfiguration.SBOXNUM; i++)
                    {
                        if (curDiffSBoxes[i].Count == 0)
                        {
                            continue;
                        }
                        value = value * (curDiffSBoxes[i].Count / 16.0);
                    }

                    if (characteristic != null) characteristic.Probability = value;
                }

                //store result
                if (characteristic != null)
                {
                    characteristic.InputDifferentials[round - 1] = inputDiff;
                    characteristic.OutputDifferentials[round - 1] = outputDiff;

                    //go one round deeper
                    Characteristic retval = FindBestCharacteristic(round - 1, differentialsList,
                        outputDiffPreviousRound, characteristic);

                    //check if there is a result
                    if (retval != null)
                    {
                        diffList.Add(retval);
                    }
                }
            }

            //search for the best result
            Characteristic best = new HeysToyCipherCharacteristic();
            foreach (var curDiffs in diffList)
            {
                if (best.Probability < curDiffs.Probability)
                {
                    best = curDiffs;
                }
            }

            return best;
        }

        public List<Pair> FilterPairs(DifferentialAttackRoundConfiguration roundConfig, List<SBoxCharacteristic> diffListOfSBox, DifferentialKeyRecoveryAttack attack, IEncryption encryption, int expectedDifferential)
        {
            //cast to use the object
            HeysToyCipherKeyRecoveryAttack heysToyCipherAttack = attack as HeysToyCipherKeyRecoveryAttack;

            if (heysToyCipherAttack == null)
            {
                throw new ArgumentNullException(nameof(attack));
            }

            //contains the filtered pairs
            List<Pair> resultList = new List<Pair>();

            List<int>[] arrayOfPossibleDifferentialsForSBoxes = new List<int>[HeysToyCipherConfiguration.SBOXNUM];
            int[] arrayOfExpectedInputDifferentialsForSBoxes = new int[HeysToyCipherConfiguration.SBOXNUM];

            for (int i = 0; i < HeysToyCipherConfiguration.SBOXNUM; i++)
            {
                arrayOfPossibleDifferentialsForSBoxes[i] = new List<int>();
                arrayOfExpectedInputDifferentialsForSBoxes[i] = GetSubBlockFromBlock(expectedDifferential, i);

                if (arrayOfExpectedInputDifferentialsForSBoxes[i] == 0)
                {
                    arrayOfPossibleDifferentialsForSBoxes[i].Add(0);
                }
            }

            //iterate over the differentials
            foreach (var curDiff in diffListOfSBox)
            {
                //Skip 0 InputDiff / OutputDiff
                if (curDiff.InputDifferential == 0)
                {
                    continue;
                }

                for (int i = 0; i < HeysToyCipherConfiguration.SBOXNUM; i++)
                {
                    if (arrayOfExpectedInputDifferentialsForSBoxes[i] == curDiff.InputDifferential)
                    {
                        arrayOfPossibleDifferentialsForSBoxes[i].Add(curDiff.OutputDifferential);
                    }
                }
            }

            //check all pairs for the conditions
            foreach (var curPair in roundConfig.UnfilteredPairList)
            {
                int cipherTextLeftMember = (curPair.LeftMember);
                int cipherTextRightMember = (curPair.RightMember);

                cipherTextLeftMember = PartialDecrypt(attack, cipherTextLeftMember);
                cipherTextRightMember = PartialDecrypt(attack, cipherTextRightMember);

                if ((heysToyCipherAttack.recoveredSubkey5) && (!heysToyCipherAttack.recoveredSubkey4))
                {
                    cipherTextLeftMember = ReverseSBoxBlock(cipherTextLeftMember);
                    cipherTextRightMember = ReverseSBoxBlock(cipherTextRightMember);
                }
                else if ((heysToyCipherAttack.recoveredSubkey5) && (heysToyCipherAttack.recoveredSubkey4))
                {
                    cipherTextLeftMember = ReverseSBoxBlock(ReversePBoxBlock(cipherTextLeftMember));
                    cipherTextRightMember = ReverseSBoxBlock(ReversePBoxBlock(cipherTextRightMember));
                }

                int diffOfCipherText = cipherTextLeftMember ^ cipherTextRightMember;

                if (heysToyCipherAttack.recoveredSubkey5)
                {
                    diffOfCipherText = ReversePBoxBlock(diffOfCipherText);
                }

                int[] diffOfCipherTextSBoxes = new int[HeysToyCipherConfiguration.SBOXNUM];
                bool[] conditionsOfSBoxes = new bool[HeysToyCipherConfiguration.SBOXNUM];
                for (int i = 0; i < HeysToyCipherConfiguration.SBOXNUM; i++)
                {
                    diffOfCipherTextSBoxes[i] = GetSubBlockFromBlock(diffOfCipherText, i);
                    conditionsOfSBoxes[i] = false;
                }

                for (int i = 0; i < HeysToyCipherConfiguration.SBOXNUM; i++)
                {
                    foreach (var possibleOutputDiff in arrayOfPossibleDifferentialsForSBoxes[i])
                    {
                        if (possibleOutputDiff == diffOfCipherTextSBoxes[i])
                        {
                            conditionsOfSBoxes[i] = true;
                        }
                    }
                }

                bool satisfied = true;
                for (int i = 0; i < HeysToyCipherConfiguration.SBOXNUM; i++)
                {
                    if (conditionsOfSBoxes[i] == false)
                    {
                        satisfied = false;
                    }
                }

                if (satisfied)
                {
                    resultList.Add(curPair);
                }
            }

            return resultList;
        }

        public int PartialDecrypt(DifferentialKeyRecoveryAttack attack, int block)
        {
            HeysToyCipherKeyRecoveryAttack heysToyCipherAttack = attack as HeysToyCipherKeyRecoveryAttack;
            int result = block;

            if (heysToyCipherAttack != null && heysToyCipherAttack.recoveredSubkey5)
            {
                result = DecryptSingleRound(result, heysToyCipherAttack.subkey5, false, true);
            }

            if (heysToyCipherAttack != null && heysToyCipherAttack.recoveredSubkey4)
            {
                result = DecryptSingleRound(result, heysToyCipherAttack.subkey4, true, false);
            }

            if (heysToyCipherAttack != null && heysToyCipherAttack.recoveredSubkey3)
            {
                result = DecryptSingleRound(result, heysToyCipherAttack.subkey3, false, false);
            }

            if (heysToyCipherAttack != null && heysToyCipherAttack.recoveredSubkey2)
            {
                result = DecryptSingleRound(result, heysToyCipherAttack.subkey2, false, false);
            }

            return result;
        }

        public List<Pair> GenerateInputPairList(int inputDifferential, int count)
        {
            List<Pair> result = new List<Pair>();

            //generate count pairs with specified inputDifferential
            for (int i = 0; i < count; i++)
            {
                //int p0 = i % 65536;
                int p0 = _random.Next(0, 65535);
                int p1 = p0 ^ inputDifferential;

                result.Add(new Pair()
                {
                    LeftMember = p0,
                    RightMember = p1
                });
            }



            return result;
        }

        public DifferentialAttackRoundConfiguration GenerateConfigurationAttack(int round, bool[] sBoxesToAttack, AbortingPolicy abortingPolicy, SearchPolicy searchPolicy, List<SBoxCharacteristic> diffListOfSBox, DifferentialKeyRecoveryAttack attack, IEncryption encryption)
        {
            if (round <= 1)
            {
                throw new ArgumentException("To attack round 1, use AttackFirstRound()");
            }
            else if (sBoxesToAttack == null)
            {
                throw new ArgumentException("At least one SBox must be specified to attack");
            }

            DifferentialAttackRoundConfiguration result = new DifferentialAttackRoundConfiguration
            {
                ActiveSBoxes = sBoxesToAttack,
                Round = round,
                AbortingPolicy = abortingPolicy,
                SearchPolicy = searchPolicy
            };

            int inputDifference = -1;
            int expectedDifference = -1;
            double probabilityAccumulated = 0.0;
            List<Characteristic> bestCharacteristics = null;

            if (result.Round == 4)
            {
                result.IsLast = true;
            }
            else if (result.Round == 3)
            {
                result.IsBeforeLast = true;
            }

#if DEBUG
            Console.WriteLine("Using searchpolicy: " + result.SearchPolicy + " on SBoxes: " + result.GetActiveSBoxes());
#endif

            switch (result.SearchPolicy)
            {
                case SearchPolicy.FirstAllCharacteristicsDepthSearch:
                    {
                        List<Characteristic>[] allCharacteristics = FindAllDifferentialsDepthSearch(result, diffListOfSBox);

                        Parallel.For(1, allCharacteristics.Length, i =>
                        //for (int i = 1; i < AllCharacteristics.Length; i++)
                        {

#if DEBUG
                        Console.WriteLine("Searching on best differential " + i + " / " + allCharacteristics.Length);
#endif

                            foreach (var characteristicToComp in allCharacteristics[i])
                            {
                                bool possible = true;

                                for (int j = 0; j < HeysToyCipherConfiguration.SBOXNUM; j++)
                                {
                                    if (sBoxesToAttack[j])
                                    {
                                        if (GetSubBlockFromBlock(characteristicToComp.InputDifferentials[round - 1], j) ==
                                            0)
                                        {
                                            possible = false;
                                        }
                                    }
                                }

                                if (!possible)
                                {
                                    continue;
                                }

                                double roundProb = 0.0;
                                List<Characteristic> roundCharacteristics = new List<Characteristic>();

                                foreach (var characteristic in allCharacteristics[i])
                                {
                                    if (characteristicToComp.InputDifferentials[0] == characteristic.InputDifferentials[0])
                                    {
                                        roundProb += characteristic.Probability;
                                        roundCharacteristics.Add(characteristic);
                                    }
                                }

                                _semaphoreSlim.Wait();
                                try
                                {
                                    if (roundProb > probabilityAccumulated)
                                    {
                                        probabilityAccumulated = roundProb;
                                        bestCharacteristics = roundCharacteristics;
                                    }
                                }
                                finally
                                {
                                    _semaphoreSlim.Release();
                                }
                            }
                        });

#if DEBUG
                    Console.WriteLine("Best differential probability: " + probabilityAccumulated);
                    foreach (var curCharacteristic in bestCharacteristics)
                    {
                        Console.WriteLine(curCharacteristic.ToString());
                    }
#endif

                        inputDifference = bestCharacteristics[0].InputDifferentials[0];
                        expectedDifference = bestCharacteristics[0].InputDifferentials[round - 1];

                    }
                    break;
                case SearchPolicy.FirstBestCharacteristicDepthSearch:
                    {
                        List<Characteristic> characteristics = FindBestCharacteristicsDepthSearch(result, diffListOfSBox);

                        //Delete Characteristics which are not usable
                        List<Characteristic> toDelete = new List<Characteristic>();
                        foreach (var curCharacteristic in characteristics)
                        {
                            bool[] conditionArray = new bool[HeysToyCipherConfiguration.SBOXNUM];

                            for (int i = 0; i < HeysToyCipherConfiguration.SBOXNUM; i++)
                            {
                                conditionArray[i] = true;

                                if (sBoxesToAttack[i])
                                {
                                    if (GetSubBlockFromBlock(curCharacteristic.InputDifferentials[round - 1], i) == 0)
                                    {
                                        conditionArray[i] = false;
                                    }
                                }
                            }

                            for (int i = 0; i < HeysToyCipherConfiguration.SBOXNUM; i++)
                            {
                                if (conditionArray[i] == false)
                                {
                                    toDelete.Add(curCharacteristic);
                                }
                            }
                        }

                        //delete unusable characteristics
                        foreach (var characteristicToDelete in toDelete)
                        {
                            characteristics.Remove(characteristicToDelete);
                        }

#if DEBUG
                    Console.WriteLine("Found " + characteristics.Count + " on SBoxes " + result.GetActiveSBoxes());
                    Console.WriteLine("Searching for the best differential...");
#endif

                        //check for other useable characteristics
                        //foreach (Characteristic characteristic in characteristics)
                        Parallel.ForEach(characteristics, (characteristic) =>
                        {
                            List<Characteristic> differentialList = FindSpecifiedDifferentialDepthSearch(characteristic.InputDifferentials[0], characteristic.InputDifferentials[round - 1], round, diffListOfSBox);

                            if (differentialList.Count == 0)
                            {
                                return;
                            }

                            double testProbability = 0.0;

                            foreach (var curCharacteristic in differentialList)
                            {
                                testProbability += curCharacteristic.Probability;
                            }

                            _semaphoreSlim.Wait();
                            try
                            {
                                if (testProbability > probabilityAccumulated)
                                {
                                    probabilityAccumulated = testProbability;
                                    bestCharacteristics = differentialList;
                                }
                            }
                            finally
                            {
                                _semaphoreSlim.Release();
                            }
                        });

#if DEBUG
                    Console.WriteLine("Best differential probability: " + probabilityAccumulated);
                    foreach (var curCharacteristic in bestCharacteristics)
                    {
                        Console.WriteLine(curCharacteristic.ToString());
                    }
#endif

                        inputDifference = bestCharacteristics[0].InputDifferentials[0];
                        expectedDifference = bestCharacteristics[0].InputDifferentials[round - 1];

                    }
                    break;
                case SearchPolicy.FirstBestCharacteristicHeuristic:
                    {
                        List<Characteristic> characteristics = FindBestCharacteristicsHeuristic(result, diffListOfSBox);

                        //Delete Characteristics which are not usable
                        List<Characteristic> toDelete = new List<Characteristic>();
                        foreach (var curCharacteristic in characteristics)
                        {
                            bool[] conditionArray = new bool[HeysToyCipherConfiguration.SBOXNUM];

                            for (int i = 0; i < HeysToyCipherConfiguration.SBOXNUM; i++)
                            {
                                conditionArray[i] = true;

                                if (sBoxesToAttack[i])
                                {
                                    if (GetSubBlockFromBlock(curCharacteristic.InputDifferentials[round - 1], i) == 0)
                                    {
                                        conditionArray[i] = false;
                                    }
                                }
                            }

                            for (int i = 0; i < HeysToyCipherConfiguration.SBOXNUM; i++)
                            {
                                if (conditionArray[i] == false)
                                {
                                    toDelete.Add(curCharacteristic);
                                }
                            }
                        }

                        //delete unusable characteristics
                        foreach (var characteristicToDelete in toDelete)
                        {
                            characteristics.Remove(characteristicToDelete);
                        }

#if DEBUG
                    Console.WriteLine("Found " + characteristics.Count + " on SBoxes " + result.GetActiveSBoxes());
                    Console.WriteLine("Searching for the best differential...");
#endif

                        //check for other useable characteristics
                        //foreach (Characteristic characteristic in characteristics)
                        Parallel.ForEach(characteristics, (characteristic) =>
                        {
                            List<Characteristic> differentialList = FindSpecifiedDifferentialDepthSearch(characteristic.InputDifferentials[0], characteristic.InputDifferentials[round - 1], round, diffListOfSBox);

                            if (differentialList.Count == 0)
                            {
                                return;
                            }

                            double testProbability = 0.0;

                            foreach (var curCharacteristic in differentialList)
                            {
                                testProbability += curCharacteristic.Probability;
                            }

                            _semaphoreSlim.Wait();
                            try
                            {
                                if (testProbability > probabilityAccumulated)
                                {
                                    probabilityAccumulated = testProbability;
                                    bestCharacteristics = differentialList;
                                }
                            }
                            finally
                            {
                                _semaphoreSlim.Release();
                            }
                        });

#if DEBUG
                    Console.WriteLine("Best differential probablility: " + probabilityAccumulated);
                    foreach (var curCharacteristic in bestCharacteristics)
                    {
                        Console.WriteLine(curCharacteristic.ToString());
                    }
#endif

                        inputDifference = bestCharacteristics[0].InputDifferentials[0];
                        expectedDifference = bestCharacteristics[0].InputDifferentials[round - 1];
                    }
                    break;
                default:
                    {

                    }
                    break;
            }

            int pairCount = 5000;
            /* 
            double temp = HeysToyCipherConfiguration.PAIRMULTIPLIER / probabilityAccumulated;
            int pairCount = (int)temp;
            */


            result.InputDifference = inputDifference;
            result.ExpectedDifference = expectedDifference;
            result.Characteristics = bestCharacteristics;
            result.Probability = probabilityAccumulated;

            result.UnfilteredPairList = GenerateInputPairList(inputDifference, pairCount);

            List<Pair> encryptedPairList = new List<Pair>();

            foreach (var curPair in result.UnfilteredPairList)
            {
                Pair encryptedPair = new Pair()
                {
                    LeftMember = encryption.EncryptBlock(curPair.LeftMember),
                    RightMember = encryption.EncryptBlock(curPair.RightMember)
                };

                encryptedPairList.Add(encryptedPair);
            }

            result.UnfilteredPairList = encryptedPairList;


            result.FilteredPairList = FilterPairs(result, diffListOfSBox, attack, encryption, expectedDifference);
            //result.FilteredPairList = result.UnfilteredPairList;

#if DEBUG
            Console.WriteLine("Generated " + result.UnfilteredPairList.Count + " pairs and there stayed " + result.FilteredPairList.Count + " filtered pairs");
#endif

            return result;
        }

        public List<Characteristic>[] FindAllDifferentialsDepthSearch(DifferentialAttackRoundConfiguration result, List<SBoxCharacteristic> diffListOfSBox)
        {
            throw new NotImplementedException();
        }

        public DifferentialAttackLastRoundResult AttackFirstRound(DifferentialKeyRecoveryAttack attack, IEncryption encryption)
        {
            HeysToyCipherKeyRecoveryAttack heysToyCipherAttack = attack as HeysToyCipherKeyRecoveryAttack;
            List<int> candidatesK1 = null;
            DifferentialAttackLastRoundResult result = new DifferentialAttackLastRoundResult();
            bool found = false;

            if (heysToyCipherAttack == null)
            {
                throw new ArgumentNullException(nameof(attack));
            }

            //recover k1
            while (!found)
            {
                //generate a pair
                Pair inputPair = new Pair()
                {
                    LeftMember = _random.Next(0, 65535),
                    RightMember = _random.Next(0, 65535)
                };

                Pair encryptedPair = new Pair()
                {
                    LeftMember = encryption.EncryptBlock(inputPair.LeftMember),
                    RightMember = encryption.EncryptBlock(inputPair.RightMember)
                };

                if (heysToyCipherAttack.recoveredSubkey5)
                {
                    encryptedPair.LeftMember = DecryptSingleRound(encryptedPair.LeftMember,
                        heysToyCipherAttack.subkey5, false, true);

                    encryptedPair.RightMember = DecryptSingleRound(encryptedPair.RightMember,
                        heysToyCipherAttack.subkey5, false, true);
                }

                if (heysToyCipherAttack.recoveredSubkey4)
                {
                    encryptedPair.LeftMember = DecryptSingleRound(encryptedPair.LeftMember,
                        heysToyCipherAttack.subkey4, true, false);

                    encryptedPair.RightMember = DecryptSingleRound(encryptedPair.RightMember,
                        heysToyCipherAttack.subkey4, true, false);
                }

                if (heysToyCipherAttack.recoveredSubkey3)
                {
                    encryptedPair.LeftMember = DecryptSingleRound(encryptedPair.LeftMember,
                        heysToyCipherAttack.subkey3, false, false);

                    encryptedPair.RightMember = DecryptSingleRound(encryptedPair.RightMember,
                        heysToyCipherAttack.subkey3, false, false);
                }

                List<int> roundCandidates = new List<int>();
                int expectedDiff = inputPair.LeftMember ^ inputPair.RightMember;

                //check all possible keys
                for (int i = 1; i < 65535; i++)
                {
                    int decryptedLeftMember = DecryptSingleRound(encryptedPair.LeftMember, i, false, false);
                    int decryptedRightMember = DecryptSingleRound(encryptedPair.RightMember, i, false, false);

                    decryptedLeftMember = ReverseSBoxBlock(ReversePBoxBlock(decryptedLeftMember));
                    decryptedRightMember = ReverseSBoxBlock(ReversePBoxBlock(decryptedRightMember));

                    int decryptedBlocksDiff = decryptedLeftMember ^ decryptedRightMember;

                    if (decryptedBlocksDiff == expectedDiff)
                    {
                        roundCandidates.Add(i);
                    }
                }

#if DEBUG
                Console.WriteLine("Found " + roundCandidates.Count + " with the specific plaintext");
#endif
                //on first iteration set the candidates
                if (candidatesK1 == null)
                {
                    candidatesK1 = roundCandidates;
                }

                //check impossible values
                List<int> toRemove = new List<int>();
                foreach (var keyCandidate in candidatesK1)
                {
                    bool isCandidate = false;
                    foreach (var roundCandidate in roundCandidates)
                    {
                        if (roundCandidate == keyCandidate)
                        {
                            isCandidate = true;
                        }
                    }

                    if (!isCandidate)
                    {
                        toRemove.Add(keyCandidate);
                    }
                }

                //remove impossible values
                foreach (var i in toRemove)
                {
                    candidatesK1.Remove(i);
                }

#if DEBUG
                Console.WriteLine("Remaining key candidates: " + candidatesK1.Count);
#endif

                //check if k1 is recovered
                if (candidatesK1.Count == 1)
                {
                    found = true;
                }
                else if (candidatesK1.Count == 0)
                {
                    throw new Exception("Key not recovered");
                }
            }

            result.SubKey1 = candidatesK1[0];

#if DEBUG
            Console.WriteLine("Recovering k0...");
#endif

            //recover k0
            int plainText = _random.Next(0, 65535);
            int cipherText = encryption.EncryptBlock(plainText);
            cipherText = DecryptSingleRound(cipherText, heysToyCipherAttack.subkey5, false, true);
            cipherText = DecryptSingleRound(cipherText, heysToyCipherAttack.subkey4, true, false);
            cipherText = DecryptSingleRound(cipherText, heysToyCipherAttack.subkey3, false, false);
            cipherText = DecryptSingleRound(cipherText, result.SubKey1, false, false);
            cipherText = ReverseSBoxBlock(ReversePBoxBlock(cipherText));
            result.SubKey0 = cipherText ^ plainText;

#if DEBUG
            Console.WriteLine("Recovering k1 and k0 finished");
#endif
            return result;
        }

        public string PrintKeyBits(int key)
        {
            var sb = new StringBuilder();

            BitArray keyBits = new BitArray(BitConverter.GetBytes(key));

            for (int i = keyBits.Length - 17; i >= 0; i--)
            {
                if (((i + 1) % 4) == 0)
                    sb.Append(" ");

                char c = keyBits[i] ? '1' : '0';
                sb.Append(c);
            }

            return sb.ToString();
        }

        public int CalculateLoopBorder(bool[] activeSBoxes)
        {
            int border = 0;
            for (int i = 0; i < activeSBoxes.Length; i++)
            {
                if (activeSBoxes[i])
                {
                    border++;
                }
            }
            border = (int)Math.Pow(2, (border * HeysToyCipherConfiguration.BITWIDTHCIPHERFOUR));
            return border;
        }

        public int GenerateValue(bool[] activeSBoxes, int data)
        {
            BitArray bitsOfValue = new BitArray(BitConverter.GetBytes(data));
            BitArray result = new BitArray(32, false);

            int currentActiveBitPosition = 0;
            for (int i = 0; i < HeysToyCipherConfiguration.SBOXNUM; i++)
            {
                if (activeSBoxes[i])
                {
                    for (int j = 0; j < HeysToyCipherConfiguration.BITWIDTHCIPHERFOUR; j++, currentActiveBitPosition++)
                    {
                        result[(i * HeysToyCipherConfiguration.BITWIDTHCIPHERFOUR) + j] = bitsOfValue[currentActiveBitPosition];
                    }
                }
            }

            byte[] bytesOfResult = new byte[4];
            result.CopyTo(bytesOfResult, 0);

            return BitConverter.ToInt32(bytesOfResult, 0);
        }

        public List<Characteristic> FindSpecifiedDifferentialDepthSearch(int inputDiff, int outputDiff, int round, List<SBoxCharacteristic> differentialNumList)
        {
            //Decrement round
            round--;

            //Starting object
            Characteristic inputObj = new HeysToyCipherCharacteristic();

            //calculate previous difference
            int outputDiffPreviousRound = ReversePBoxBlock(outputDiff);

            //start depth-first search
            List<Characteristic> retVal = FindAllCharacteristics(round, differentialNumList, outputDiffPreviousRound, inputObj);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            retVal.RemoveAll(item => ((item.Probability == -1.0) || (item.InputDifferentials[0] != inputDiff)));

            foreach (var curItem in retVal)
            {
                curItem.InputDifferentials[round] = outputDiff;
            }

#if DEBUG
            Console.WriteLine("Found " + retVal.Count + " paths with inputDifference = " + inputDiff + " and outputDifference = " + outputDiff);
#endif

            return retVal;
        }

        public DifferentialAttackRoundConfiguration RefreshPairLists(DifferentialAttackRoundConfiguration roundConfig, List<SBoxCharacteristic> diffListOfSBox, DifferentialKeyRecoveryAttack attack, IEncryption encryption)
        {
            throw new NotImplementedException();
        }
    }
}
