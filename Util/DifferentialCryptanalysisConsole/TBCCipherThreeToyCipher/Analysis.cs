using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfaces;

namespace TBCCipherThreeToyCipher
{
    class Analysis : IDifferentialCryptanalysis
    {
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly Random _random = new Random();


        public int BuildBlockFromPartialBlocks(int pb3, int pb2, int pb1, int pb0)
        {
            throw new NotImplementedException();
        }

        public int GetSubBlockFromBlock(int block, int subBlockNum)
        {
            BitArray bitsOfBlock = new BitArray(BitConverter.GetBytes(block));
            BitArray resultBits = new BitArray(4);

            switch (subBlockNum)
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
            }

            byte[] resultBytes = new byte[4];
            resultBits.CopyTo(resultBytes, 0);

            int resultInt = BitConverter.ToInt32(resultBytes, 0);
            return resultInt;
        }

        /// <summary>
        /// Decrypts a single round with a given key
        /// </summary>
        /// <param name="block"></param>
        /// <param name="key"></param>
        /// <param name="beforeLast"></param>
        /// <param name="isLast"></param>
        /// <returns></returns>
        public int DecryptSingleRound(int block, int key, bool beforeLast, bool isLast)
        {
            int result = block ^ key;
            result = ReverseSBoxBlock(result);
            //Console.WriteLine("Reversed " + result);
            return result;
        }

        public DifferentialAttackRoundResult RecoverKeyInformation(DifferentialKeyRecoveryAttack attack,
            DifferentialAttackRoundConfiguration configuration, IEncryption encryption)
        {
            if (configuration.ActiveSBoxes == null)
            {
                throw new ArgumentException("activeSBoxes should contain at least one active SBox");
            }

            DifferentialAttackRoundResult roundResult = new DifferentialAttackRoundResult();

            double avg = 0;

            foreach (var curPair in configuration.FilteredPairList)
            {
                roundResult = new DifferentialAttackRoundResult();
                //Generate border for the loop
                int loopBorder = CalculateLoopBorder(configuration.ActiveSBoxes);

            for(int i = 0; i < loopBorder; i++)
            //Parallel.For(0, loopBorder, i =>
            {
                int guessedKey = GenerateValue(configuration.ActiveSBoxes, i);

                KeyProbability curTry = new KeyProbability() { Counter = 0, Key = guessedKey };

                    Pair encryptedPair = new Pair() { LeftMember = encryption.EncryptBlock(curPair.LeftMember), RightMember = encryption.EncryptBlock(curPair.RightMember) };

                    //encryptedPair.LeftMember = PartialDecrypt(attack, encryptedPair.LeftMember);
                    //encryptedPair.RightMember = PartialDecrypt(attack, encryptedPair.RightMember);

                    //reverse round with the guessed key
                    //int leftMemberSingleDecrypted = DecryptSingleRound(encryptedPair.LeftMember, curTry.Key, configuration.IsBeforeLast, configuration.IsLast);
                    //int rightMemberSingleDecrypted = DecryptSingleRound(encryptedPair.RightMember, curTry.Key, configuration.IsBeforeLast, configuration.IsLast);

                    int leftMemberSingleDecrypted = ReverseSBoxBlock(encryptedPair.LeftMember ^ curTry.Key);
                    int rightMemberSingleDecrypted = ReverseSBoxBlock(encryptedPair.RightMember ^ curTry.Key);

                    //int leftMemberSingleDecrypted = encryptedPair.LeftMember;
                    //int rightMemberSingleDecrypted = encryptedPair.RightMember;

                    int differentialToCompare = (leftMemberSingleDecrypted ^ rightMemberSingleDecrypted);

                    //differentialToCompare = ReverseSBoxBlock(differentialToCompare);
                    //differentialToCompare = GetSubBlockFromBlock(differentialToCompare, 0);

                    //Console.WriteLine(differentialToCompare);

                    if (differentialToCompare == configuration.ExpectedDifference)
                    {
                        curTry.Counter++;
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
            }

            foreach (var a in roundResult.KeyCandidateProbabilities)
            {
                if (a.Counter > 0)
                {
                    avg++;
                }
            }

            }//);

            avg = avg / configuration.FilteredPairList.Count;

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
            foreach (var curKeyProbability in roundResult.KeyCandidateProbabilities)//.GetRange(0,3))
            {
                Console.WriteLine("Guessed key: " + curKeyProbability.Key + " Count: " + curKeyProbability.Counter + " Probability: {0:N4}", ((curKeyProbability.Counter / (double)configuration.UnfilteredPairList.Count)));
            }
#endif

            return roundResult;
        }

        /// <summary>
        /// Reverses the SBox
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int ReverseSBoxBlock(int data)
        {
            BitArray bitsOfBlock = new BitArray(BitConverter.GetBytes(data));

            BitArray zeroToThree = new BitArray(4);
            BitArray fourToSeven = new BitArray(4);

            for (int i = 0; i < 4; i++)
            {
                zeroToThree[i] = bitsOfBlock[i];
                fourToSeven[i] = bitsOfBlock[i + 4];
            }

            byte[] zeroToThreeBytes = new byte[4];
            zeroToThree.CopyTo(zeroToThreeBytes, 0);

            byte[] fourToSevenBytes = new byte[4];
            fourToSeven.CopyTo(fourToSevenBytes, 0);

            int zeroToThreeInt = BitConverter.ToInt32(zeroToThreeBytes, 0);
            int fourToSevenInt = BitConverter.ToInt32(fourToSevenBytes, 0);

            //use sbox
            zeroToThreeInt = CipherThreeConfiguration.SBOXREVERSE[zeroToThreeInt];
            fourToSevenInt = CipherThreeConfiguration.SBOXREVERSE[fourToSevenInt];

            bitsOfBlock = new BitArray(16);

            zeroToThree = new BitArray(BitConverter.GetBytes(zeroToThreeInt));
            fourToSeven = new BitArray(BitConverter.GetBytes(fourToSevenInt));

            for (int i = 0; i < 4; i++)
            {
                bitsOfBlock[i] = zeroToThree[i];
                //bitsOfBlock[4 + i] = fourToSeven[i];
            }

            byte[] bytes = new byte[4];
            bitsOfBlock.CopyTo(bytes, 0);
            int combined = BitConverter.ToInt32(bytes, 0);

            return combined;
        }

        public int ReversePBoxBlock(int data)
        {
            throw new NotImplementedException();
        }

        public int ApplySingleSBox(int data)
        {
            BitArray bitsOfBlock = new BitArray(BitConverter.GetBytes(data));
            BitArray zeroToThree = new BitArray(4);

            for (int i = 0; i < 4; i++)
            {
                zeroToThree[i] = bitsOfBlock[i];
            }

            byte[] zeroToThreeBytes = new byte[4];
            zeroToThree.CopyTo(zeroToThreeBytes, 0);

            int zeroToThreeInt = BitConverter.ToInt32(zeroToThreeBytes, 0);

            //use sbox
            zeroToThreeInt = CipherThreeConfiguration.SBOX[zeroToThreeInt];

            return zeroToThreeInt;
        }

        public int ApplyPBoxToBlock(int data)
        {
            throw new NotImplementedException();
        }

        public int ApplySBoxToBlock(int data)
        {
            BitArray bitsOfBlock = new BitArray(BitConverter.GetBytes(data));

            BitArray zeroToThree = new BitArray(4);

            for (int i = 0; i < 4; i++)
            {
                zeroToThree[i] = bitsOfBlock[i];
            }

            byte[] zeroToThreeBytes = new byte[2];
            zeroToThree.CopyTo(zeroToThreeBytes, 0);

            int zeroToThreeInt = BitConverter.ToInt32(zeroToThreeBytes, 0);

            //use sbox
            zeroToThreeInt = CipherThreeConfiguration.SBOX[zeroToThreeInt];

            bitsOfBlock = new BitArray(16);

            zeroToThree = new BitArray(BitConverter.GetBytes(zeroToThreeInt));

            for (int i = 0; i < 4; i++)
            {
                bitsOfBlock[i] = zeroToThree[i];
            }

            byte[] bytes = new byte[4];
            bitsOfBlock.CopyTo(bytes, 0);
            int combined = BitConverter.ToInt32(bytes, 0);

            return combined;
        }

        public BitArray GetBitsOfInt(int value)
        {
            throw new NotImplementedException();
        }

        public int MergeBytes(int pb3, int pb2, int pb1, int pb0)
        {
            throw new NotImplementedException();
        }

        public List<Characteristic>[] FindAllDifferentialsDepthSearch(DifferentialAttackRoundConfiguration roundConfiguration, List<SBoxCharacteristic> differentialsList)
        {
            throw new NotImplementedException();
        }

        public List<Characteristic> FindBestCharacteristicsHeuristic(DifferentialAttackRoundConfiguration roundConfiguration, List<SBoxCharacteristic> differentialsList)
        {
            throw new NotImplementedException();
        }

        public List<Characteristic> FindSpecifiedDifferentialDepthSearch(int inputDiff, int outputDiff, int round, List<SBoxCharacteristic> differentialNumList)
        {
            //Decrement round
            round--;

            //Starting object
            Characteristic inputObj = new CipherThreeCharacteristic();

            //calculate previous difference
            int outputDiffPreviousRound = (outputDiff);

            //start depth-first search
            List<Characteristic> retVal = FindAllCharacteristics(round, differentialNumList, outputDiffPreviousRound, inputObj);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            retVal.RemoveAll(item => ((item.Probability == -1.0) || (item.InputDifferentials[0] != inputDiff)));

            foreach (var curItem in retVal)
            {
                curItem.InputDifferentials[round] = outputDiff;
            }

#if DEBUG
            //Console.WriteLine("Found " + retVal.Count + " paths with inputDifference = " + inputDiff + " and outputDifference = " + outputDiff);
#endif

            return retVal;
        }

        public List<Characteristic> FindBestCharacteristicsDepthSearch(DifferentialAttackRoundConfiguration roundConfiguration,
            List<SBoxCharacteristic> differentialsList)
        {
            int round = roundConfiguration.Round;

            //Decrement round for recursive call
            round--;

            //result list
            List<Characteristic> resultList = new List<Characteristic>();

            //calculate loop border
            int loopBorder = CalculateLoopBorder(roundConfiguration.ActiveSBoxes);

            for(int i = 1; i < loopBorder;i++)
            //Parallel.For(1, loopBorder, i =>
            {
                Characteristic inputObj = new CipherThreeCharacteristic();

                //expected difference
                int expectedDifference = GenerateValue(roundConfiguration.ActiveSBoxes, i);
                int outputDifferencePreviousRound = expectedDifference;

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
                        //Console.WriteLine("Case " + roundConfiguration.GetActiveSBoxes() + " finished iteration i = " + i + " / " + loopBorder);
                        //Console.WriteLine(retVal.ToString());
#endif
                        resultList.Add(retVal);
                    }
                    finally
                    {
                        _semaphoreSlim.Release();
                    }
                }

            }//);

            var sorted = resultList.OrderByDescending(elem => elem.Probability).ToList();
#if DEBUG
            foreach (var curRetVAl in sorted)
            {
                //Console.WriteLine(curRetVAl.ToString());
            }
#endif
            return sorted;
        }



        private Characteristic FindBestCharacteristic(int round, List<SBoxCharacteristic> differentialsList, int outputDiff, Characteristic res)
        {
            //end of rekursion
            if (round == 0)
                return res;

            //contains the active SBoxes in the round
            bool[] activeSBoxes = new bool[CipherThreeConfiguration.SBOXNUM];

            //check active SBoxes
            int[] outputDiffs = new int[CipherThreeConfiguration.SBOXNUM];
            for (int i = 0; i < CipherThreeConfiguration.SBOXNUM; i++)
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
            List<SBoxCharacteristic>[] arrayOfDifferentialLists = new List<SBoxCharacteristic>[CipherThreeConfiguration.SBOXNUM];
            int comb = 1;
            for (int b = 0; b < CipherThreeConfiguration.SBOXNUM; b++)
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
                SBoxCharacteristic[] curDiffSBoxes = new SBoxCharacteristic[CipherThreeConfiguration.SBOXNUM];

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
                    for (int i = 0; i < CipherThreeConfiguration.SBOXNUM; i++)
                    {
                        if (activeSBoxes[i])
                        {
                            curDiffSBoxes[i] = arrayOfDifferentialLists[i][0];
                        }
                    }
                }

                //check null values
                for (int z = 0; z < CipherThreeConfiguration.SBOXNUM; z++)
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
                for (int i = 0; i < CipherThreeConfiguration.SBOXNUM; i++)
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
                for (int i = CipherThreeConfiguration.SBOXNUM - 1; i >= 0; i--)
                {
                    inputDiff = inputDiff ^ curDiffSBoxes[i].InputDifferential;
                    if ((i - 1) >= 0)
                    {
                        inputDiff = inputDiff << CipherThreeConfiguration.BITWIDTHCIPHERFOUR;
                    }
                }

                //outputDifference for previous round
                int outputDiffPreviousRound = inputDiff;

                //calc new prob
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (characteristic != null && characteristic.Probability != -1)
                {
                    for (int i = 0; i < CipherThreeConfiguration.SBOXNUM; i++)
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
                    for (int i = 0; i < CipherThreeConfiguration.SBOXNUM; i++)
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
            Characteristic best = new CipherThreeCharacteristic();
            foreach (var curDiffs in diffList)
            {
                if (best.Probability < curDiffs.Probability)
                {
                    best = curDiffs;
                }
            }

            return best;
        }


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
                    //int outputDiff = ApplySBoxToBlock(i) ^ ApplySBoxToBlock(j);

                    if(inputDiff == 0 || outputDiff == 0)
                        continue;

                    bool found = false;

                    foreach (var curDiff in result)
                    {
                        if ((curDiff.InputDifferential == inputDiff) && (curDiff.OutputDifferential == outputDiff))
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
                                //new Pair(){LeftMember = ApplySBoxToBlock(i), RightMember = ApplySBoxToBlock(j)}
                            }
                        });
                    }
                    else
                    {
                        diffToEdit.InputPairList.Add(new Pair() { LeftMember = i, RightMember = j });
                        diffToEdit.OutputPairList.Add(new Pair() { LeftMember = ApplySingleSBox(i), RightMember = ApplySingleSBox(j) });
                        //diffToEdit.OutputPairList.Add(new Pair() { LeftMember = ApplySBoxToBlock(i), RightMember = ApplySBoxToBlock(j) });
                    }
                }
            }

            foreach (var curDiff in result)
            {
                curDiff.Probability = curDiff.Count / 16.0;
                //curDiff.Probability = curDiff.Count / 256.0;
            }

            result = result.OrderByDescending(item => item.Probability).ToList();
            return result;
        }

        public List<Pair> FilterPairs(DifferentialAttackRoundConfiguration roundConfig, List<SBoxCharacteristic> diffListOfSBox,
            DifferentialKeyRecoveryAttack attack, IEncryption encryption, int expectedDifferential)
        {
            //cast to use the object
            CipherThreeDifferentialKeyRecoveryAttack cipherFourAttack = attack as CipherThreeDifferentialKeyRecoveryAttack;

            if (cipherFourAttack == null)
            {
                throw new ArgumentNullException(nameof(attack));
            }

            //contains the filtered pairs
            List<Pair> resultList = new List<Pair>();

            List<int>[] arrayOfPossibleDifferentialsForSBoxes = new List<int>[CipherThreeConfiguration.SBOXNUM];
            int[] arrayOfExpectedInputDifferentialsForSBoxes = new int[CipherThreeConfiguration.SBOXNUM];

            for (int i = 0; i < CipherThreeConfiguration.SBOXNUM; i++)
            {
                arrayOfPossibleDifferentialsForSBoxes[i] = new List<int>();
                arrayOfExpectedInputDifferentialsForSBoxes[i] = expectedDifferential;

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

                for (int i = 0; i < CipherThreeConfiguration.SBOXNUM; i++)
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
                int cipherTextLeftMember = encryption.EncryptBlock(curPair.LeftMember);
                int cipherTextRightMember = encryption.EncryptBlock(curPair.RightMember);

                cipherTextLeftMember = PartialDecrypt(attack, cipherTextLeftMember);
                cipherTextRightMember = PartialDecrypt(attack, cipherTextRightMember);

                int diffOfCipherText = (cipherTextLeftMember ^ cipherTextRightMember);

                int[] diffOfCipherTextSBoxes = new int[CipherThreeConfiguration.SBOXNUM];
                bool[] conditionsOfSBoxes = new bool[CipherThreeConfiguration.SBOXNUM];
                for (int i = 0; i < CipherThreeConfiguration.SBOXNUM; i++)
                {
                    diffOfCipherTextSBoxes[i] = diffOfCipherText;
                    conditionsOfSBoxes[i] = false;
                }

                for (int i = 0; i < CipherThreeConfiguration.SBOXNUM; i++)
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
                for (int i = 0; i < CipherThreeConfiguration.SBOXNUM; i++)
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
            CipherThreeDifferentialKeyRecoveryAttack cipherThreeAttack = attack as CipherThreeDifferentialKeyRecoveryAttack;
            int result = block;

            if (cipherThreeAttack != null && cipherThreeAttack.RecoveredSubkey3)
            {
                result = DecryptSingleRound(result, cipherThreeAttack.Subkey3, false, true);
            }

            if (cipherThreeAttack != null && cipherThreeAttack.RecoveredSubkey2)
            {
                result = DecryptSingleRound(result, cipherThreeAttack.Subkey2, true, false);
            }

            if (cipherThreeAttack != null && cipherThreeAttack.RecoveredSubkey1)
            {
                result = DecryptSingleRound(result, cipherThreeAttack.Subkey1, false, false);
            }

            return result;
        }

        public List<Pair> GenerateInputPairList(int inputDifferential, int count)
        {
            List<Pair> result = new List<Pair>();

            //generate count pairs with specified inputDifferential
            //while(result.Count < count)
            for (int i = 0; i < count; i++)
            {
                int p0 = i % 16;
                //int p0 = _random.Next(0, 15);
                int p1 = p0 ^ inputDifferential;

                result.Add(new Pair()
                {
                    LeftMember = p1,
                    RightMember = p0
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

            if (result.Round == 3)
            {
                result.IsLast = true;
            }
            else if (result.Round == 2)
            {
                result.IsBeforeLast = true;
            }



            List<Characteristic> characteristics = FindBestCharacteristicsDepthSearch(result, diffListOfSBox);

            //Delete Characteristics which are not usable
            List<Characteristic> toDelete = new List<Characteristic>();
            foreach (var curCharacteristic in characteristics)
            {
                bool[] conditionArray = new bool[CipherThreeConfiguration.SBOXNUM];

                for (int i = 0; i < CipherThreeConfiguration.SBOXNUM; i++)
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

                for (int i = 0; i < CipherThreeConfiguration.SBOXNUM; i++)
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




            foreach (var characteristic in characteristics)
            //Parallel.ForEach(characteristics, (characteristic) =>
            {
                List<Characteristic> differentialList = FindSpecifiedDifferentialDepthSearch(characteristic.InputDifferentials[0], characteristic.InputDifferentials[round - 1], round, diffListOfSBox);

                if (differentialList.Count == 0)
                {
                    //return;
                    continue;
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
                        //Console.WriteLine("Current best: InputDiff: " + bestCharacteristics[0].InputDifferentials[0] + " ExpectedDiff: " + bestCharacteristics[0].InputDifferentials[2] + " Probability: " + probabilityAccumulated);
                    }
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
            }//);



            /*  */
            inputDifference = characteristics[1].InputDifferentials[0];
            expectedDifference = characteristics[1].InputDifferentials[round - 1];

            result.InputDifference = inputDifference;
            result.ExpectedDifference = expectedDifference;
            result.Characteristics = characteristics;
            result.Probability = characteristics[1].Probability;
           

            /*
            inputDifference = characteristics[attackCount].InputDifferentials[0];
            expectedDifference = characteristics[attackCount].InputDifferentials[round - 1];

            result.InputDifference = inputDifference;
            result.ExpectedDifference = expectedDifference;
            result.Characteristics = characteristics;
            result.Probability = characteristics[attackCount].Probability;
            */

            /*  
            inputDifference = bestCharacteristics[0].InputDifferentials[0];
            expectedDifference = bestCharacteristics[0].InputDifferentials[round - 1];

            result.InputDifference = inputDifference;
            result.ExpectedDifference = expectedDifference;
            result.Characteristics = bestCharacteristics;
            */



            //result.Characteristics = new List<Characteristic>();
            //result.Characteristics.Add(bestCharacteristics[0]);
            //result.Probability = probabilityAccumulated;

            int pairCount = 16;

            result.UnfilteredPairList = GenerateInputPairList(inputDifference, pairCount);
            result.FilteredPairList = FilterPairs(result, diffListOfSBox, attack, encryption, expectedDifference);
            result.FilteredPairList = result.UnfilteredPairList;

#if DEBUG
            Console.WriteLine("Generated " + result.UnfilteredPairList.Count + " pairs and there stayed " + result.FilteredPairList.Count + " filtered pairs");
#endif

            return result;
        }

        public DifferentialAttackLastRoundResult AttackFirstRound(DifferentialKeyRecoveryAttack attack, IEncryption encryption)
        {
            CipherThreeDifferentialKeyRecoveryAttack cipherFourAttack = attack as CipherThreeDifferentialKeyRecoveryAttack;
            List<int> candidatesK1 = null;
            DifferentialAttackLastRoundResult result = new DifferentialAttackLastRoundResult();
            bool found = false;

            if (cipherFourAttack == null)
            {
                throw new ArgumentNullException(nameof(attack));
            }

            //recover k1
            while (!found)
            {
                //generate a pair
                Pair inputPair = new Pair()
                {
                    LeftMember = _random.Next(0, 15),
                    RightMember = _random.Next(0, 15)
                };

                Pair encryptedPair = new Pair()
                {
                    LeftMember = encryption.EncryptBlock(inputPair.LeftMember),
                    RightMember = encryption.EncryptBlock(inputPair.RightMember)
                };

                if (cipherFourAttack.RecoveredSubkey3)
                {
                    encryptedPair.LeftMember = DecryptSingleRound(encryptedPair.LeftMember,
                        cipherFourAttack.Subkey3, false, true);

                    encryptedPair.RightMember = DecryptSingleRound(encryptedPair.RightMember,
                        cipherFourAttack.Subkey3, false, true);
                }

                if (cipherFourAttack.RecoveredSubkey2)
                {
                    encryptedPair.LeftMember = DecryptSingleRound(encryptedPair.LeftMember,
                        cipherFourAttack.Subkey2, true, false);

                    encryptedPair.RightMember = DecryptSingleRound(encryptedPair.RightMember,
                        cipherFourAttack.Subkey2, true, false);
                }

                List<int> roundCandidates = new List<int>();
                int expectedDiff = inputPair.LeftMember ^ inputPair.RightMember;

                //check all possible keys
                for (int i = 0; i < 16; i++)
                {
                    int decryptedLeftMember = DecryptSingleRound(encryptedPair.LeftMember, i, false, false);
                    int decryptedRightMember = DecryptSingleRound(encryptedPair.RightMember, i, false, false);

                    decryptedLeftMember = ReverseSBoxBlock(decryptedLeftMember);
                    decryptedRightMember = ReverseSBoxBlock(decryptedRightMember);

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
                //Console.WriteLine("Remaining key candidates: " + candidatesK1.Count);
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
            //Console.WriteLine("Recovering k0...");
#endif

            //recover k0
            int plainText = _random.Next(0, 15);
            int cipherText = encryption.EncryptBlock(plainText);
            cipherText = DecryptSingleRound(cipherText, cipherFourAttack.Subkey3, false, true);
            cipherText = DecryptSingleRound(cipherText, cipherFourAttack.Subkey2, true, false);
            cipherText = DecryptSingleRound(cipherText, result.SubKey1, false, false);
            cipherText = ReverseSBoxBlock((cipherText));
            result.SubKey0 = cipherText ^ plainText;

#if DEBUG
            //Console.WriteLine("Recovering k1 and k0 finished");
#endif
            return result;
        }

        public DifferentialAttackRoundConfiguration RefreshPairLists(DifferentialAttackRoundConfiguration roundConfig,
            List<SBoxCharacteristic> diffListOfSBox, DifferentialKeyRecoveryAttack attack, IEncryption encryption)
        {
            throw new NotImplementedException();
        }

        public string PrintKeyBits(int key)
        {
            throw new NotImplementedException();
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
            border = (int)Math.Pow(2, (border * CipherThreeConfiguration.BITWIDTHCIPHERFOUR));
            return border;
        }

        public int GenerateValue(bool[] activeSBoxes, int data)
        {
            BitArray bitsOfValue = new BitArray(BitConverter.GetBytes(data));
            BitArray result = new BitArray(32, false);

            int currentActiveBitPosition = 0;
            for (int i = 0; i < CipherThreeConfiguration.SBOXNUM; i++)
            {
                if (activeSBoxes[i])
                {
                    for (int j = 0; j < CipherThreeConfiguration.BITWIDTHCIPHERFOUR; j++, currentActiveBitPosition++)
                    {
                        result[(i * CipherThreeConfiguration.BITWIDTHCIPHERFOUR) + j] = bitsOfValue[currentActiveBitPosition];
                    }
                }
            }

            byte[] bytesOfResult = new byte[4];
            result.CopyTo(bytesOfResult, 0);

            return BitConverter.ToInt32(bytesOfResult, 0);
        }

        private List<Characteristic> FindAllCharacteristics(int round, List<SBoxCharacteristic> differentialsList, int outputDiff, Characteristic res)
        {
            //end of rekursion
            if (round == 0)
            {
                List<Characteristic> resList = new List<Characteristic>();
                resList.Add(res);
                return resList;
            }

            //contains the active SBoxes in the round
            bool[] activeSBoxes = new bool[CipherThreeConfiguration.SBOXNUM];

            //check active SBoxes
            int[] outputDiffs = new int[CipherThreeConfiguration.SBOXNUM];
            for (int i = 0; i < CipherThreeConfiguration.SBOXNUM; i++)
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
            List<SBoxCharacteristic>[] arrayOfDifferentialLists = new List<SBoxCharacteristic>[CipherThreeConfiguration.SBOXNUM];
            int comb = 1;
            for (int b = 0; b < CipherThreeConfiguration.SBOXNUM; b++)
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
                SBoxCharacteristic[] curDiffSBoxes = new SBoxCharacteristic[CipherThreeConfiguration.SBOXNUM];

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
                    for (int i = 0; i < CipherThreeConfiguration.SBOXNUM; i++)
                    {
                        if (activeSBoxes[i])
                        {
                            curDiffSBoxes[i] = arrayOfDifferentialLists[i][0];
                        }
                    }
                }

                //check null values
                for (int z = 0; z < CipherThreeConfiguration.SBOXNUM; z++)
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
                for (int i = 0; i < CipherThreeConfiguration.SBOXNUM; i++)
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
                for (int i = CipherThreeConfiguration.SBOXNUM - 1; i >= 0; i--)
                {
                    inputDiff = inputDiff ^ curDiffSBoxes[i].InputDifferential;
                    if ((i - 1) >= 0)
                    {
                        inputDiff = inputDiff << CipherThreeConfiguration.BITWIDTHCIPHERFOUR;
                    }
                }

                //outputDifference for previous round
                int outputDiffPreviousRound = (inputDiff);

                //calc new prob
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (characteristic != null && characteristic.Probability != -1)
                {
                    for (int i = 0; i < CipherThreeConfiguration.SBOXNUM; i++)
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
                    for (int i = 0; i < CipherThreeConfiguration.SBOXNUM; i++)
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
            Characteristic best = new CipherThreeCharacteristic();
            foreach (var curDiffs in diffList)
            {
                if (best.Probability < curDiffs.Probability)
                {
                    best = curDiffs;
                }
            }

            return diffList;
        }
    }
}
