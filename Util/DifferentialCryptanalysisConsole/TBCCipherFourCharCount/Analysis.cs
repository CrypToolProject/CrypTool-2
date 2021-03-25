using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Interfaces;

namespace TBCCipherFourCharCount
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

            int zeroToThreeInt = BitConverter.ToInt32(zeroToThreeBytes, 0);
            int fourToSevenInt = BitConverter.ToInt32(fourToSevenBytes, 0);
            int eightToElevenInt = BitConverter.ToInt32(eightToElevenBytes, 0);
            int twelveToFifteenInt = BitConverter.ToInt32(twelveToFifteenBytes, 0);

            //use sbox
            zeroToThreeInt = CipherFourConfiguration.SBOX[zeroToThreeInt];
            fourToSevenInt = CipherFourConfiguration.SBOX[fourToSevenInt];
            eightToElevenInt = CipherFourConfiguration.SBOX[eightToElevenInt];
            twelveToFifteenInt = CipherFourConfiguration.SBOX[twelveToFifteenInt];

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

            byte[] zeroToThreeBytes = new byte[4];
            zeroToThree.CopyTo(zeroToThreeBytes, 0);

            int zeroToThreeInt = BitConverter.ToInt32(zeroToThreeBytes, 0);

            //use sbox
            zeroToThreeInt = CipherFourConfiguration.SBOX[zeroToThreeInt];

            return zeroToThreeInt;
        }

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
        /// Statistic analysis of the used sbox
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

        /// <summary>
        /// Decrypts a single round with the given key
        /// </summary>
        /// <param name="block"></param>
        /// <param name="key"></param>
        /// <param name="beforeLast"></param>
        /// <param name="isLast"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Filters out pair, which does not follow the expected differential flow
        /// </summary>
        /// <param name="roundConfig"></param>
        /// <param name="diffListOfSBox"></param>
        /// <param name="attack"></param>
        /// <param name="encryption"></param>
        /// <param name="expectedDifferential"></param>
        /// <returns></returns>
        public List<Pair> FilterPairs(DifferentialAttackRoundConfiguration roundConfig, List<SBoxCharacteristic> diffListOfSBox, DifferentialKeyRecoveryAttack attack, IEncryption encryption, int expectedDifferential)
        {
            //cast to use the object
            CipherFourDifferentialKeyRecoveryAttack cipherFourAttack = attack as CipherFourDifferentialKeyRecoveryAttack;

            if (cipherFourAttack == null)
            {
                throw new ArgumentNullException(nameof(attack));
            }

            //contains the filtered pairs
            List<Pair> resultList = new List<Pair>();

            List<int>[] arrayOfPossibleDifferentialsForSBoxes = new List<int>[CipherFourConfiguration.SBOXNUM];
            int[] arrayOfExpectedInputDifferentialsForSBoxes = new int[CipherFourConfiguration.SBOXNUM];

            for (int i = 0; i < CipherFourConfiguration.SBOXNUM; i++)
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

                for (int i = 0; i < CipherFourConfiguration.SBOXNUM; i++)
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

                if ((cipherFourAttack.recoveredSubkey6) && (!cipherFourAttack.recoveredSubkey5))
                {
                    cipherTextLeftMember = ReverseSBoxBlock(cipherTextLeftMember);
                    cipherTextRightMember = ReverseSBoxBlock(cipherTextRightMember);
                }
                else if ((cipherFourAttack.recoveredSubkey6) && (cipherFourAttack.recoveredSubkey5))
                {
                    cipherTextLeftMember = ReverseSBoxBlock(ReversePBoxBlock(cipherTextLeftMember));
                    cipherTextRightMember = ReverseSBoxBlock(ReversePBoxBlock(cipherTextRightMember));
                }

                int diffOfCipherText = cipherTextLeftMember ^ cipherTextRightMember;

                if (cipherFourAttack.recoveredSubkey6)
                {
                    diffOfCipherText = ReversePBoxBlock(diffOfCipherText);
                }

                int[] diffOfCipherTextSBoxes = new int[CipherFourConfiguration.SBOXNUM];
                bool[] conditionsOfSBoxes = new bool[CipherFourConfiguration.SBOXNUM];
                for (int i = 0; i < CipherFourConfiguration.SBOXNUM; i++)
                {
                    diffOfCipherTextSBoxes[i] = GetSubBlockFromBlock(diffOfCipherText, i);
                    conditionsOfSBoxes[i] = false;
                }

                for (int i = 0; i < CipherFourConfiguration.SBOXNUM; i++)
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
                for (int i = 0; i < CipherFourConfiguration.SBOXNUM; i++)
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

        /// <summary>
        /// Generates a list with possible good pairs with specified inputDifferential
        /// </summary>
        /// <param name="inputDifferential"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public List<Pair> GenerateInputPairList(int inputDifferential, int count)
        {
            List<Pair> result = new List<Pair>();

            //generate count pairs with specified inputDifferential
            for (int i = 0; i < count; i++)
            {
                /* 
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                byte[] rand = new byte[4];
                rng.GetBytes(rand);
                int j = Math.Abs(BitConverter.ToInt32(rand, 0));
                int p0 = j % 65535;
                */

                int p0 = _random.Next(0, 65535);
                int p1 = p0 ^ inputDifferential;

                /*
                Console.WriteLine("P0: " + Convert.ToString(p0, 2));
                Console.WriteLine("P1: " + Convert.ToString(p1, 2));
                Console.WriteLine("Expected Diff: " + Convert.ToString(inputDifferential, 2));
                Console.WriteLine("Current  Diff: "+ Convert.ToString((p0 ^ p1), 2));

                if (p0 > 65535)
                {
                    Console.WriteLine("Hit!");
                }
                */

                result.Add(new Pair()
                {
                    LeftMember = p0,
                    RightMember = p1
                });
            }



            return result;
        }

        /// <summary>
        /// Returns an array of Lists of characteristics for all possible output differentials
        /// </summary>
        /// <param name="roundConfiguration"></param>
        /// <param name="differentialsList"></param>
        /// <returns></returns>
        public List<Characteristic>[] FindAllDifferentialsDepthSearch(DifferentialAttackRoundConfiguration roundConfiguration, List<SBoxCharacteristic> differentialsList)
        {
            int round = roundConfiguration.Round;

            //calculate loop border
            int loopBorder = CalculateLoopBorder(roundConfiguration.ActiveSBoxes);

            //result list
            List<Characteristic>[] resultList = new List<Characteristic>[loopBorder];

            //for(int i = 1; i < loopBorder;i++)
            Parallel.For(1, loopBorder, i =>
            {
                //expected difference
                int expectedDifference = GenerateValue(roundConfiguration.ActiveSBoxes, i);

                //start depth-first search
                List<Characteristic> retVal = FindCharacteristics(expectedDifference, round, differentialsList);
                resultList[i] = retVal;

#if DEBUG
                Console.WriteLine("Case " + roundConfiguration.GetActiveSBoxes() + " finished iteration i = " + i + " / " + loopBorder + " with " + retVal.Count + " differentials for expected difference " + expectedDifference);
#endif

            });

            return resultList;
        }

        /// <summary>
        /// Starts to search for good differential characteristics with depth-first search
        /// </summary>
        /// <param name="roundConfiguration"></param>
        /// <param name="differentialsList"></param>
        /// <returns></returns>
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
                Characteristic inputObj = new CipherFourCharacteristic();

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

        /// <summary>
        /// Recursive depth-first search implementation to get good paths. Returns if probability is the value falls below a lower bound.
        /// </summary>
        /// <param name="round"></param>
        /// <param name="differentialsList"></param>
        /// <param name="outputDiff"></param>
        /// <param name="res"></param>
        /// <returns></returns>
        public Characteristic FindBestCharacteristic(int round, List<SBoxCharacteristic> differentialsList, int outputDiff, Characteristic res)
        {
            //break if probability is not good enough
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if ((res.Probability < CipherFourConfiguration.PROBABILITYBOUNDBESTCHARACTERISTICSEARCH) && (res.Probability != -1))
                return null;

            //end of rekursion
            if (round == 0)
                return res;

            //contains the active SBoxes in the round
            bool[] activeSBoxes = new bool[CipherFourConfiguration.SBOXNUM];

            //check active SBoxes
            int[] outputDiffs = new int[CipherFourConfiguration.SBOXNUM];
            for (int i = 0; i < CipherFourConfiguration.SBOXNUM; i++)
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
            List<SBoxCharacteristic>[] arrayOfDifferentialLists = new List<SBoxCharacteristic>[CipherFourConfiguration.SBOXNUM];           
            int comb = 1;
            for (int b = 0; b < CipherFourConfiguration.SBOXNUM; b++)
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
                SBoxCharacteristic[] curDiffSBoxes = new SBoxCharacteristic[CipherFourConfiguration.SBOXNUM];

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
                    for (int i = 0; i < CipherFourConfiguration.SBOXNUM; i++)
                    {
                        if (activeSBoxes[i])
                        {
                            curDiffSBoxes[i] = arrayOfDifferentialLists[i][0];
                        }
                    }
                }

                //check null values
                for (int z = 0; z < CipherFourConfiguration.SBOXNUM; z++)
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
                for (int i = 0; i < CipherFourConfiguration.SBOXNUM; i++)
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
                for (int i = CipherFourConfiguration.SBOXNUM - 1; i >= 0; i--)
                {
                    inputDiff = inputDiff ^ curDiffSBoxes[i].InputDifferential;
                    if ((i - 1) >= 0)
                    {
                        inputDiff = inputDiff << CipherFourConfiguration.BITWIDTHCIPHERFOUR;
                    }
                }

                //outputDifference for previous round
                int outputDiffPreviousRound = ReversePBoxBlock(inputDiff);

                //calc new prob
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (characteristic != null && characteristic.Probability != -1)
                {
                    for (int i = 0; i < CipherFourConfiguration.SBOXNUM; i++)
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
                    for (int i = 0; i < CipherFourConfiguration.SBOXNUM; i++)
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
            Characteristic best = new CipherFourCharacteristic();
            foreach (var curDiffs in diffList)
            {
                if (best.Probability < curDiffs.Probability)
                {
                    best = curDiffs;
                }
            }

            return best;
        }

        /// <summary>
        /// Returns all characteristics with specified outputDiff (expectedDiff)
        /// </summary>
        /// <param name="expectedDiff"></param>
        /// <param name="round"></param>
        /// <param name="differentialNumList"></param>
        /// <returns></returns>
        public List<Characteristic> FindCharacteristics(int expectedDiff, int round, List<SBoxCharacteristic> differentialNumList)
        {
            //Decrement round
            round--;

            //Starting object
            Characteristic inputObj = new CipherFourCharacteristic();
            inputObj.InputDifferentials[round] = expectedDiff;

            //calculate previous difference
            int outputDiffPreviousRound = ReversePBoxBlock(expectedDiff);

            //start depth-first search
            List<Characteristic> retVal = new List<Characteristic>();//FindAllCharacteristics(round, differentialNumList, outputDiffPreviousRound, inputObj);

#if DEBUG
            Console.WriteLine("Found " + retVal.Count + " paths with expectedDifference = " + expectedDiff);
#endif

            return retVal;
        }

        /// <summary>
        /// Returns all characteristics with specified inputDiff and outputDiff
        /// </summary>
        /// <param name="inputDiff"></param>
        /// <param name="outputDiff"></param>
        /// <param name="round"></param>
        /// <param name="differentialNumList"></param>
        /// <returns></returns>
        public List<Characteristic> FindSpecifiedDifferentialDepthSearch(int inputDiff, int outputDiff, int round, List<SBoxCharacteristic> differentialNumList)
        {
            //Decrement round
            round--;

            //Starting object
            Characteristic inputObj = new CipherFourCharacteristic();

            //calculate previous difference
            int outputDiffPreviousRound = ReversePBoxBlock(outputDiff);

            //start depth-first search
            List<Characteristic> retVal = new List<Characteristic>(); //FindAllCharacteristics(round, differentialNumList, outputDiffPreviousRound, inputObj);
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

        public BigInteger countCharacteristics(DifferentialAttackRoundConfiguration roundConfiguration, List<SBoxCharacteristic> differentialsList)
        {
            int round = roundConfiguration.Round;

            BigInteger result = CalculateLoopBorder(roundConfiguration.ActiveSBoxes);

            round--;

            //calculate loop border
            int loopBorder = CalculateLoopBorder(roundConfiguration.ActiveSBoxes);

            //for(int i = 1; i < loopBorder; i++)
            Parallel.For(1, loopBorder, i =>
            {
                Characteristic inputObj = new CipherFourCharacteristic();

                //expected difference
                int expectedDifference = GenerateValue(roundConfiguration.ActiveSBoxes, i);

                inputObj.InputDifferentials[round] = expectedDifference;
                //inputObj.OutputDifferentials[round - 1] = ReversePBoxBlock(expectedDifference);
                //no permutation for cipher 2
                inputObj.OutputDifferentials[round - 1] = (expectedDifference);

                BigInteger retVal = FindAllCharacteristics(round, differentialsList, inputObj.OutputDifferentials[round - 1], inputObj);

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                    _semaphoreSlim.Wait();
                    try
                    {
                        result += retVal;
                        //Console.WriteLine("Refreshed value: " + result.ToString());
                    }
                    finally
                    {
                        _semaphoreSlim.Release();
                    }
            });

            Console.WriteLine("Result: " + result.ToString());
            return result;
        }

        /// <summary>
        /// Recursive depth-first search implementation to return all paths
        /// </summary>
        /// <param name="round"></param>
        /// <param name="differentialsList"></param>
        /// <param name="outputDiff"></param>
        /// <param name="res"></param>
        /// <returns></returns>
        private BigInteger FindAllCharacteristics(int round, List<SBoxCharacteristic> differentialsList, int outputDiff, Characteristic res)
        {
            //break if probability is not good enough
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if ((res.Probability < CipherFourConfiguration.PROBABILITYBOUNDDIFFERENTIALSEARCH) && (res.Probability != -1))
                return 0;

            //end of rekursion
            if (round == 0)
            {
                List<Characteristic> resList = new List<Characteristic>();
                resList.Add(res);
                return resList.Count;
            }

            //contains the active SBoxes in the round
            bool[] activeSBoxes = new bool[CipherFourConfiguration.SBOXNUM];

            //check active SBoxes
            int[] outputDiffs = new int[CipherFourConfiguration.SBOXNUM];
            for (int i = 0; i < CipherFourConfiguration.SBOXNUM; i++)
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
            BigInteger diffList = 0;

            //prepare the arrayOfDifferentialLists
            List<SBoxCharacteristic>[] arrayOfDifferentialLists = new List<SBoxCharacteristic>[CipherFourConfiguration.SBOXNUM];
            int comb = 1;
            for (int b = 0; b < CipherFourConfiguration.SBOXNUM; b++)
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
                SBoxCharacteristic[] curDiffSBoxes = new SBoxCharacteristic[CipherFourConfiguration.SBOXNUM];

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
                    for (int i = 0; i < CipherFourConfiguration.SBOXNUM; i++)
                    {
                        if (activeSBoxes[i])
                        {
                            curDiffSBoxes[i] = arrayOfDifferentialLists[i][0];
                        }
                    }
                }

                //check null values
                for (int z = 0; z < CipherFourConfiguration.SBOXNUM; z++)
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
                for (int i = 0; i < CipherFourConfiguration.SBOXNUM; i++)
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
                for (int i = CipherFourConfiguration.SBOXNUM - 1; i >= 0; i--)
                {
                    inputDiff = inputDiff ^ curDiffSBoxes[i].InputDifferential;
                    if ((i - 1) >= 0)
                    {
                        inputDiff = inputDiff << CipherFourConfiguration.BITWIDTHCIPHERFOUR;
                    }
                }

                //outputDifference for previous round
                int outputDiffPreviousRound = ReversePBoxBlock(inputDiff);

                //calc new prob
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (characteristic != null && characteristic.Probability != -1)
                {
                    for (int i = 0; i < CipherFourConfiguration.SBOXNUM; i++)
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
                    for (int i = 0; i < CipherFourConfiguration.SBOXNUM; i++)
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
                    BigInteger retval = FindAllCharacteristics(round - 1, differentialsList,
                        outputDiffPreviousRound, characteristic);

                    //check if there is a result
                    if (retval != 0)
                    {
                        diffList += (retval);
                    }
                }
            }

            return diffList;
        }

        /// <summary>
        /// Search for good characteristic with heuristic
        /// </summary>
        /// <param name="roundConfiguration"></param>
        /// <param name="differentialsList"></param>
        /// <returns></returns>
        public List<Characteristic> FindBestCharacteristicsHeuristic(DifferentialAttackRoundConfiguration roundConfiguration, List<SBoxCharacteristic> differentialsList)
        {
            int round = roundConfiguration.Round;

            List<Characteristic> resultList = new List<Characteristic>();

            round--;

            //calculate loop border
            int loopBorder = CalculateLoopBorder(roundConfiguration.ActiveSBoxes);

            //for(int i = 1; i < loopBorder; i++)
            Parallel.For(1, loopBorder, i =>
            {
                Characteristic inputObj = new CipherFourCharacteristic();

                //expected difference
                int expectedDifference = GenerateValue(roundConfiguration.ActiveSBoxes, i);

                inputObj.InputDifferentials[round] = expectedDifference;
                inputObj.OutputDifferentials[round - 1] = ReversePBoxBlock(expectedDifference);

                Characteristic retVal = FindBestPredecessorDifference(round, inputObj, differentialsList);

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

            //Sort by probability
            var sorted = resultList.OrderByDescending(elem => elem.Probability).ToList();
#if DEBUG
            foreach (var curRetVAl in sorted)
            {
                Console.WriteLine(curRetVAl.ToString());
            }
#endif
            return sorted;
        }

        /// <summary>
        /// Implements the heuristic of "find best probability in the current round"
        /// </summary>
        /// <param name="round"></param>
        /// <param name="inputCharacteristic"></param>
        /// <param name="differentialsList"></param>
        /// <returns></returns>
        private Characteristic FindBestPredecessorDifference(int round, Characteristic inputCharacteristic, List<SBoxCharacteristic> differentialsList)
        {
            //end of rekursion
            if (round == 0)
                return inputCharacteristic;

            //check active sboxes
            int zeroToThreeRoundOutput = GetSubBlockFromBlock(inputCharacteristic.OutputDifferentials[round - 1], 0);
            int fourToSevenRoundOutput = GetSubBlockFromBlock(inputCharacteristic.OutputDifferentials[round - 1], 1);
            int eightToElevenRoundOutput = GetSubBlockFromBlock(inputCharacteristic.OutputDifferentials[round - 1], 2);
            int twelveToFifteenRoundOutput = GetSubBlockFromBlock(inputCharacteristic.OutputDifferentials[round - 1], 3);

            //resultList
            List<Characteristic> charList = new List<Characteristic>();

            //copy object
            Characteristic characteristic = inputCharacteristic.Clone() as Characteristic;

            double bestValueSBox4 = 0.0;
            double bestValueSBox3 = 0.0;
            double bestValueSBox2 = 0.0;
            double bestValueSBox1 = 0.0;

            double probabilityAccumulated = 1.0;

            int inputDiffSBox4 = 0;
            int inputDiffSBox3 = 0;
            int inputDiffSBox2 = 0;
            int inputDiffSBox1 = 0;

            //check if SBox4 is active
            if (zeroToThreeRoundOutput > 0)
            {
                //find best Diff in that list
                foreach (var curDiff in differentialsList)
                {
                    if (curDiff.OutputDifferential == zeroToThreeRoundOutput)
                    {
                        if ((curDiff.Count / 16.0) > bestValueSBox4)
                        {
                            bestValueSBox4 = curDiff.Count / 16.0;
                            inputDiffSBox4 = curDiff.InputDifferential;
                        }
                    }
                }
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (bestValueSBox4 != 0)
            {
                probabilityAccumulated *= bestValueSBox4;
            }

            //check if SBox3 is active
            if (fourToSevenRoundOutput > 0)
            {
                //find best Diff in that list
                foreach (var curDiff in differentialsList)
                {
                    if (curDiff.OutputDifferential == fourToSevenRoundOutput)
                    {
                        if ((curDiff.Count / 16.0) > bestValueSBox3)
                        {
                            bestValueSBox3 = curDiff.Count / 16.0;
                            inputDiffSBox3 = curDiff.InputDifferential;
                        }
                    }
                }
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (bestValueSBox3 != 0)
            {
                probabilityAccumulated *= bestValueSBox3;
            }

            //check if SBox2 is active
            if (eightToElevenRoundOutput > 0)
            {
                //find best Diff in that list
                foreach (var curDiff in differentialsList)
                {
                    if (curDiff.OutputDifferential == eightToElevenRoundOutput)
                    {
                        if ((curDiff.Count / 16.0) > bestValueSBox2)
                        {
                            bestValueSBox2 = curDiff.Count / 16.0;
                            inputDiffSBox2 = curDiff.InputDifferential;
                        }
                    }
                }
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (bestValueSBox2 != 0)
            {
                probabilityAccumulated *= bestValueSBox2;
            }

            //check if SBox1 is active
            if (twelveToFifteenRoundOutput > 0)
            {
                //find best Diff in that list
                foreach (var curDiff in differentialsList)
                {
                    if (curDiff.OutputDifferential == twelveToFifteenRoundOutput)
                    {
                        if ((curDiff.Count / 16.0) > bestValueSBox1)
                        {
                            bestValueSBox1 = curDiff.Count / 16.0;
                            inputDiffSBox1 = curDiff.InputDifferential;
                        }
                    }
                }
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (bestValueSBox1 != 0)
            {
                probabilityAccumulated *= bestValueSBox1;
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (characteristic != null && characteristic.Probability != -1)
            {
                characteristic.Probability *= probabilityAccumulated;
            }
            else
            {
                if (characteristic != null) characteristic.Probability = probabilityAccumulated;
            }

            if (characteristic != null)
            {
                characteristic.InputDifferentials[round - 1] =
                    BuildBlockFromPartialBlocks(inputDiffSBox1, inputDiffSBox2, inputDiffSBox3, inputDiffSBox4);

                if (round - 2 >= 0)
                {
                    int t = ReversePBoxBlock(characteristic.InputDifferentials[round - 1]);
                    characteristic.OutputDifferentials[round - 2] = t;
                }

                Characteristic retVal = FindBestPredecessorDifference(round - 1, characteristic, differentialsList);

                charList.Add(retVal);
            }

            //search for the best result
            Characteristic best = new CipherFourCharacteristic();
            foreach (var curChar in charList)
            {
                if (best.Probability < curChar.Probability)
                {
                    best = curChar;
                }
            }

            return best;
        }

        /// <summary>
        /// Returns the specified subblock (4 bit) of the given block (16 bit)
        /// </summary>
        /// <param name="block"></param>
        /// <param name="subBlockNum"></param>
        /// <returns></returns>
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
                pboxedArray[CipherFourConfiguration.PBOXREVERSE[i]] = bitsOfBlock[i];
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
            zeroToThreeInt = CipherFourConfiguration.SBOXREVERSE[zeroToThreeInt];
            fourToSevenInt = CipherFourConfiguration.SBOXREVERSE[fourToSevenInt];
            eightToElevenInt = CipherFourConfiguration.SBOXREVERSE[eightToElevenInt];
            twelveToFifteenInt = CipherFourConfiguration.SBOXREVERSE[twelveToFifteenInt];

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

        /// <summary>
        /// Decrypts a block with all recovered subkeys
        /// </summary>
        /// <param name="attack"></param>
        /// <param name="block"></param>
        /// <returns></returns>
        public int PartialDecrypt(DifferentialKeyRecoveryAttack attack, int block)
        {
            CipherFourDifferentialKeyRecoveryAttack cipherFourAttack = attack as CipherFourDifferentialKeyRecoveryAttack;
            int result = block;

            if (cipherFourAttack != null && cipherFourAttack.recoveredSubkey6)
            {
                result = DecryptSingleRound(result, cipherFourAttack.subkey6, false, true);
            }

            if (cipherFourAttack != null && cipherFourAttack.recoveredSubkey5)
            {
                result = DecryptSingleRound(result, cipherFourAttack.subkey5, true, false);
            }

            if (cipherFourAttack != null && cipherFourAttack.recoveredSubkey4)
            {
                result = DecryptSingleRound(result, cipherFourAttack.subkey4, false, false);
            }

            if (cipherFourAttack != null && cipherFourAttack.recoveredSubkey3)
            {
                result = DecryptSingleRound(result, cipherFourAttack.subkey3, false, false);
            }

            if (cipherFourAttack != null && cipherFourAttack.recoveredSubkey2)
            {
                result = DecryptSingleRound(result, cipherFourAttack.subkey2, false, false);
            }

            return result;
        }

        /// <summary>
        /// Recovers some bits of a subkey
        /// </summary>
        /// <param name="attack"></param>
        /// <param name="configuration"></param>
        /// <param name="encryption"></param>
        /// <returns></returns>
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
                    Pair encryptedPair = new Pair() { LeftMember = encryption.EncryptBlock(curPair.LeftMember), RightMember = encryption.EncryptBlock(curPair.RightMember) };

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

        /// <summary>
        /// Converts an integer to bitarray
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public BitArray GetBitsOfInt(int value)
        {
            return new BitArray(BitConverter.GetBytes(value));
        }

        /// <summary>
        /// Merges the four blocks so that the result consists of pb3(bit 12 - 15) || pb2(bit 8 - 11) || pb1(bit 4 - 7) || pb0(bit 0 - 3) 
        /// </summary>
        /// <param name="pb3"></param>
        /// <param name="pb2"></param>
        /// <param name="pb1"></param>
        /// <param name="pb0"></param>
        /// <returns></returns>
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

        /// <summary>
        /// returns the input as bitstring
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Generates a DifferentialAttackRoundConfiguration to attack a given round of the cipher
        /// </summary>
        /// <param name="round"></param>
        /// <param name="sBoxesToAttack"></param>
        /// <param name="abortingPolicy"></param>
        /// <param name="searchPolicy"></param>
        /// <param name="diffListOfSBox"></param>
        /// <param name="attack"></param>
        /// <param name="encryption"></param>
        /// <returns></returns>
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

            if (result.Round == 5)
            {
                result.IsLast = true;
            }
            else if (result.Round == 4)
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

                            for (int j = 0; j < CipherFourConfiguration.SBOXNUM; j++)
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
                        bool[] conditionArray = new bool[CipherFourConfiguration.SBOXNUM];

                        for (int i = 0; i < CipherFourConfiguration.SBOXNUM; i++)
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

                        for (int i = 0; i < CipherFourConfiguration.SBOXNUM; i++)
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
                        bool[] conditionArray = new bool[CipherFourConfiguration.SBOXNUM];

                        for (int i = 0; i < CipherFourConfiguration.SBOXNUM; i++)
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

                        for (int i = 0; i < CipherFourConfiguration.SBOXNUM; i++)
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
            double temp = CipherFourConfiguration.PAIRMULTIPLIER / probabilityAccumulated;
            int pairCount = (int)temp;
            */

#if DEBUG
            Console.WriteLine("Needed PairCount = " + pairCount);
#endif

            result.InputDifference = inputDifference;
            result.ExpectedDifference = expectedDifference;
            result.Characteristics = bestCharacteristics;
            result.Probability = probabilityAccumulated;

            result.UnfilteredPairList = GenerateInputPairList(inputDifference, pairCount);
            //result.FilteredPairList = FilterPairs(result, diffListOfSBox, attack, encryption, expectedDifference);
            result.FilteredPairList = result.UnfilteredPairList;

            /*
            while (result.FilteredPairList.Count < 32)
            {
                result.UnfilteredPairList = GenerateInputPairList(inputDifference, pairCount);
                result.FilteredPairList = FilterPairs(result, diffListOfSBox, attack, encryption, expectedDifference);
                //result.FilteredPairList = result.UnfilteredPairList;
            }
            */

#if DEBUG
            Console.WriteLine("Generated " + result.UnfilteredPairList.Count + " pairs and there stayed " + result.FilteredPairList.Count + " filtered pairs");
#endif

            return result;
        }

        /// <summary>
        /// Refresh the pair lists
        /// </summary>
        /// <param name="roundConfig"></param>
        /// <param name="diffListOfSBox"></param>
        /// <param name="attack"></param>
        /// <param name="encryption"></param>
        /// <returns></returns>
        public DifferentialAttackRoundConfiguration RefreshPairLists(DifferentialAttackRoundConfiguration roundConfig, List<SBoxCharacteristic> diffListOfSBox, DifferentialKeyRecoveryAttack attack, IEncryption encryption)
        {
            int pairCount = 10000;//(int) (CipherFourConfiguration.PAIRMULTIPLIER / roundConfig.Probability);
            if (pairCount < 512)
            {
                pairCount = 512;
            }
            roundConfig.FilteredPairList = new List<Pair>();
            while (roundConfig.FilteredPairList.Count < 32)
            {
                roundConfig.UnfilteredPairList = GenerateInputPairList(roundConfig.InputDifference, pairCount);
                roundConfig.FilteredPairList = FilterPairs(roundConfig, diffListOfSBox, attack, encryption, roundConfig.ExpectedDifference);
                

                if (roundConfig.FilteredPairList.Count < 32)
                {
                    pairCount += 1000;
                }

                
            }

            //Console.WriteLine("In round " + roundConfig.Round + " generated " + roundConfig.UnfilteredPairList.Count + " pairs and there stayed " + roundConfig.FilteredPairList.Count + " filtered pairs");

            return roundConfig;
        }

        /// <summary>
        /// Attacks the last round, recovers k1 and k0
        /// </summary>
        /// <param name="attack"></param>
        /// <param name="encryption"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public DifferentialAttackLastRoundResult AttackFirstRound(DifferentialKeyRecoveryAttack attack, IEncryption encryption)
        {
            CipherFourDifferentialKeyRecoveryAttack cipherFourAttack = attack as CipherFourDifferentialKeyRecoveryAttack;
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
                    LeftMember = _random.Next(0, 65535),
                    RightMember = _random.Next(0, 65535)
                };

                Pair encryptedPair = new Pair()
                {
                    LeftMember = encryption.EncryptBlock(inputPair.LeftMember),
                    RightMember = encryption.EncryptBlock(inputPair.RightMember)
                };

                if (cipherFourAttack.recoveredSubkey6)
                {
                    encryptedPair.LeftMember = DecryptSingleRound(encryptedPair.LeftMember,
                        cipherFourAttack.subkey6, false, true);

                    encryptedPair.RightMember = DecryptSingleRound(encryptedPair.RightMember,
                        cipherFourAttack.subkey6, false, true);

                }

                if (cipherFourAttack.recoveredSubkey5)
                {
                    encryptedPair.LeftMember = DecryptSingleRound(encryptedPair.LeftMember,
                        cipherFourAttack.subkey5, true, false);

                    encryptedPair.RightMember = DecryptSingleRound(encryptedPair.RightMember,
                        cipherFourAttack.subkey5, true, false);
                }

                if (cipherFourAttack.recoveredSubkey4)
                {
                    encryptedPair.LeftMember = DecryptSingleRound(encryptedPair.LeftMember,
                        cipherFourAttack.subkey4, false, false);

                    encryptedPair.RightMember = DecryptSingleRound(encryptedPair.RightMember,
                        cipherFourAttack.subkey4, false, false);
                }

                if (cipherFourAttack.recoveredSubkey3)
                {
                    encryptedPair.LeftMember = DecryptSingleRound(encryptedPair.LeftMember,
                        cipherFourAttack.subkey3, false, false);

                    encryptedPair.RightMember = DecryptSingleRound(encryptedPair.RightMember,
                        cipherFourAttack.subkey3, false, false);
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
                }else if (candidatesK1.Count == 0)
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
            cipherText = DecryptSingleRound(cipherText, cipherFourAttack.subkey6, false, true);
            cipherText = DecryptSingleRound(cipherText, cipherFourAttack.subkey5, true, false);
            cipherText = DecryptSingleRound(cipherText, cipherFourAttack.subkey4, false, false);
            cipherText = DecryptSingleRound(cipherText, cipherFourAttack.subkey3, false, false);
            cipherText = DecryptSingleRound(cipherText, result.SubKey1, false, false);
            cipherText = ReverseSBoxBlock(ReversePBoxBlock(cipherText));
            result.SubKey0 = cipherText ^ plainText;

#if DEBUG
            Console.WriteLine("Recovering k1 and k0 finished");
#endif
            return result;
        }

        /// <summary>
        /// Applies the permutation to data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int ApplyPBoxToBlock(int data)
        {
            BitArray bits = new BitArray(BitConverter.GetBytes(data));
            BitArray pboxedArray = new BitArray(16);

            //use pbox
            for (int i = 0; i < 16; i++)
            {
                pboxedArray[CipherFourConfiguration.PBOX[i]] = bits[i];
            }

            byte[] bytes = new byte[4];
            pboxedArray.CopyTo(bytes, 0);

            int outputBlock = BitConverter.ToInt32(bytes, 0);

            return outputBlock;
        }

        /// <summary>
        /// calculates the border for a loop depending on the active sboxes
        /// </summary>
        /// <param name="activeSBoxes"></param>
        /// <returns></returns>
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
            border = (int)Math.Pow(2, (border * CipherFourConfiguration.BITWIDTHCIPHERFOUR));
            return border;
        }

        /// <summary>
        /// Translates a value depending on the active SBoxes
        /// </summary>
        /// <param name="activeSBoxes"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public int GenerateValue(bool[] activeSBoxes, int data)
        {
            BitArray bitsOfValue = new BitArray(BitConverter.GetBytes(data));
            BitArray result = new BitArray(32, false);

            int currentActiveBitPosition = 0;
            for (int i = 0; i < CipherFourConfiguration.SBOXNUM; i++)
            {
                if (activeSBoxes[i])
                {
                    for (int j = 0; j < CipherFourConfiguration.BITWIDTHCIPHERFOUR; j++, currentActiveBitPosition++)
                    {
                        result[(i * CipherFourConfiguration.BITWIDTHCIPHERFOUR) + j] = bitsOfValue[currentActiveBitPosition];
                    }
                }
            }

            byte[] bytesOfResult = new byte[4];
            result.CopyTo(bytesOfResult, 0);

            return BitConverter.ToInt32(bytesOfResult, 0);
        }

    }
}
