using Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyOneCiphers
{
    public class Cipher16Bit16Analysis : IDifferentialCryptanalysis
    {
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly Random _random = new Random();
        public int paircount;

        public int ApplyPBoxToBlock(int data)
        {
            BitArray bits = new BitArray(BitConverter.GetBytes(data));
            BitArray pboxedArray = new BitArray(16);

            //use pbox
            for (int i = 0; i < 16; i++)
            {
                pboxedArray[Cipher16Bit16Configuration.PBOX[i]] = bits[i];
            }

            byte[] bytes = new byte[4];
            pboxedArray.CopyTo(bytes, 0);

            int outputBlock = BitConverter.ToInt32(bytes, 0);

            return outputBlock;
        }

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
            zeroToThreeInt = Cipher16Bit16Configuration.SBOX[zeroToThreeInt];
            fourToSevenInt = Cipher16Bit16Configuration.SBOX[fourToSevenInt];
            eightToElevenInt = Cipher16Bit16Configuration.SBOX[eightToElevenInt];
            twelveToFifteenInt = Cipher16Bit16Configuration.SBOX[twelveToFifteenInt];

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
            zeroToThreeInt = Cipher16Bit16Configuration.SBOX[zeroToThreeInt];

            return zeroToThreeInt;
        }

        public DifferentialAttackLastRoundResult AttackFirstRound(DifferentialKeyRecoveryAttack attack, IEncryption encryption)
        {
            Cipher16Bit16DifferentialKeyRecoveryAttack cipherFourAttack = attack as Cipher16Bit16DifferentialKeyRecoveryAttack;
            DifferentialAttackLastRoundResult result = new DifferentialAttackLastRoundResult();
            bool found = false;

            if (cipherFourAttack == null)
            {
                throw new ArgumentNullException(nameof(attack));
            }

            List<int> possibleKeyList = new List<int>();
            for (int i = 0; i < 65535; i++)
            {
                possibleKeyList.Add(i);
            }

            int testedKeys = 0;
            int decryptionCounter = 0;
            int pairCounter = 0;

            //recover k1
            while (!found)
            {
                //generate a pair
                Pair inputPair = new Pair()
                {
                    LeftMember = _random.Next(0, 65535),
                    RightMember = _random.Next(0, 65535)
                };

                //pairCounter++;

                Pair encryptedPair = new Pair()
                {
                    LeftMember = encryption.EncryptBlock(inputPair.LeftMember),
                    RightMember = encryption.EncryptBlock(inputPair.RightMember)
                };

                if (cipherFourAttack.recoveredSubkey3)
                {
                    encryptedPair.LeftMember = DecryptSingleRound(encryptedPair.LeftMember,
                        cipherFourAttack.subkey4, false, true);

                    encryptedPair.RightMember = DecryptSingleRound(encryptedPair.RightMember,
                        cipherFourAttack.subkey4, false, true);
                }

                if (cipherFourAttack.recoveredSubkey2)
                {
                    encryptedPair.LeftMember = DecryptSingleRound(encryptedPair.LeftMember,
                        cipherFourAttack.subkey3, true, false);

                    encryptedPair.RightMember = DecryptSingleRound(encryptedPair.RightMember,
                        cipherFourAttack.subkey3, true, false);
                }

                int expectedDiff = inputPair.LeftMember ^ inputPair.RightMember;

                var temp = possibleKeyList.ToList();
                int keysToTest = temp.Count;

                //check all possible keys
                foreach (var item in temp)
                {
                    int decryptedLeftMember = DecryptSingleRound(encryptedPair.LeftMember, item, false, false);
                    int decryptedRightMember = DecryptSingleRound(encryptedPair.RightMember, item, false, false);

                    decryptedLeftMember = ReverseSBoxBlock(ReversePBoxBlock(decryptedLeftMember));
                    decryptedRightMember = ReverseSBoxBlock(ReversePBoxBlock(decryptedRightMember));

                    int decryptedBlocksDiff = decryptedLeftMember ^ decryptedRightMember;

                    decryptionCounter++;
                    decryptionCounter++;

                    testedKeys++;

                    if (decryptedBlocksDiff != expectedDiff)
                    {
                        possibleKeyList.Remove(item);
                    }
                }



#if DEBUG
                //Console.WriteLine("Remaining key candidates: " + candidatesK1.Count);
#endif

                //check if k1 is recovered
                if (possibleKeyList.Count == 1)
                {
                    found = true;
                }
                else if (possibleKeyList.Count == 0)
                {
                    throw new Exception("Key not recovered");
                }
            }

            result.SubKey1 = possibleKeyList[0];

#if DEBUG
            //Console.WriteLine("Recovering k0...");
#endif

            //recover k0
            int plainText = _random.Next(0, 65535);
            int cipherText = encryption.EncryptBlock(plainText);
            cipherText = DecryptSingleRound(cipherText, cipherFourAttack.subkey4, false, true);
            cipherText = DecryptSingleRound(cipherText, cipherFourAttack.subkey3, true, false);
            cipherText = DecryptSingleRound(cipherText, result.SubKey1, false, false);
            cipherText = ReverseSBoxBlock(ReversePBoxBlock(cipherText));
            result.SubKey0 = cipherText ^ plainText;

            pairCounter++;

            testedKeys++;
            decryptionCounter++;

            result.testedKeys = testedKeys;

#if DEBUG
            //Console.WriteLine("Recovering k1 and k0 finished");
#endif
            return result;
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
            border = (int)Math.Pow(2, (border * Cipher16Bit16Configuration.BITWIDTHCIPHERFOUR));
            return border;
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

        public int DecryptSingleRound(int block, int key, bool beforeLast, bool isLast)
        {
            int result = block;

            //if is last round
            if (isLast)
            {
                //use the key to decrypt
                result = result ^ key;
                return result;
            }

            if (beforeLast)
            {
                result = ReverseSBoxBlock(result);
                result = result ^ key;
                return result;
            }

            //use the pbox
            result = ReversePBoxBlock(result);

            //undo the sbox
            result = ReverseSBoxBlock(result);

            //use the key to decrypt
            result = result ^ key;

            return result;
        }

        public List<Pair> FilterPairs(DifferentialAttackRoundConfiguration roundConfig, List<SBoxCharacteristic> diffListOfSBox, DifferentialKeyRecoveryAttack attack, IEncryption encryption, int expectedDifferential)
        {
            // cast to use the object
            Cipher16Bit16DifferentialKeyRecoveryAttack cipherFourAttack = attack as Cipher16Bit16DifferentialKeyRecoveryAttack;

            if (cipherFourAttack == null)
            {
                throw new ArgumentNullException(nameof(attack));
            }

            //contains the filtered pairs
            List<Pair> resultList = new List<Pair>();

            List<int>[] arrayOfPossibleDifferentialsForSBoxes = new List<int>[Cipher16Bit16Configuration.SBOXNUM];
            int[] arrayOfExpectedInputDifferentialsForSBoxes = new int[Cipher16Bit16Configuration.SBOXNUM];

            for (int i = 0; i < Cipher16Bit16Configuration.SBOXNUM; i++)
            {
                arrayOfPossibleDifferentialsForSBoxes[i] = new List<int>();
                arrayOfExpectedInputDifferentialsForSBoxes[i] = GetSubBlockFromBlock(expectedDifferential, i);

                if (arrayOfExpectedInputDifferentialsForSBoxes[i] == 0)
                {
                    arrayOfPossibleDifferentialsForSBoxes[i].Add(0);
                }
            }

            //iterate over the differentials
            /* */
            foreach (var curDiff in diffListOfSBox)
            {
                //Skip 0 InputDiff / OutputDiff
                if (curDiff.InputDifferential == 0)
                {
                    continue;
                }

                for (int i = 0; i < Cipher16Bit16Configuration.SBOXNUM; i++)
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

                if ((cipherFourAttack.recoveredSubkey3) && (!cipherFourAttack.recoveredSubkey2))
                {
                    cipherTextLeftMember = ReverseSBoxBlock(cipherTextLeftMember);
                    cipherTextRightMember = ReverseSBoxBlock(cipherTextRightMember);
                }
                else if ((cipherFourAttack.recoveredSubkey3) && (cipherFourAttack.recoveredSubkey2))
                {
                    cipherTextLeftMember = ReverseSBoxBlock(ReversePBoxBlock(cipherTextLeftMember));
                    cipherTextRightMember = ReverseSBoxBlock(ReversePBoxBlock(cipherTextRightMember));
                }

                int diffOfCipherText = cipherTextLeftMember ^ cipherTextRightMember;

                if (cipherFourAttack.recoveredSubkey3)
                {
                    diffOfCipherText = ReversePBoxBlock(diffOfCipherText);
                }

                int[] diffOfCipherTextSBoxes = new int[Cipher16Bit16Configuration.SBOXNUM];
                bool[] conditionsOfSBoxes = new bool[Cipher16Bit16Configuration.SBOXNUM];
                for (int i = 0; i < Cipher16Bit16Configuration.SBOXNUM; i++)
                {
                    diffOfCipherTextSBoxes[i] = GetSubBlockFromBlock(diffOfCipherText, i);
                    conditionsOfSBoxes[i] = false;
                }

                for (int i = 0; i < Cipher16Bit16Configuration.SBOXNUM; i++)
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
                for (int i = 0; i < Cipher16Bit16Configuration.SBOXNUM; i++)
                {
                    if (conditionsOfSBoxes[i] == false)
                    {
                        satisfied = false;
                    }
                }

                if (satisfied)
                {
                    resultList.Add(new Pair() { LeftMember = curPair.LeftMember, RightMember = curPair.RightMember });
                }
            }

            return resultList;
        }

        public List<Characteristic>[] FindAllDifferentialsDepthSearch(DifferentialAttackRoundConfiguration roundConfiguration, List<SBoxCharacteristic> differentialsList)
        {
            throw new NotImplementedException();
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
                Characteristic inputObj = new Cipher16Bit16Characteristic();

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

            });

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
            //break if probability is not good enough
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if ((res.Probability < Cipher16Bit16Configuration.PROBABILITYBOUNDBESTCHARACTERISTICSEARCH) && (res.Probability != -1))
                return null;

            //end of rekursion
            if (round == 0)
                return res;

            //contains the active SBoxes in the round
            bool[] activeSBoxes = new bool[Cipher16Bit16Configuration.SBOXNUM];

            //check active SBoxes
            int[] outputDiffs = new int[Cipher16Bit16Configuration.SBOXNUM];
            for (int i = 0; i < Cipher16Bit16Configuration.SBOXNUM; i++)
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
            List<SBoxCharacteristic>[] arrayOfDifferentialLists = new List<SBoxCharacteristic>[Cipher16Bit16Configuration.SBOXNUM];
            int comb = 1;
            for (int b = 0; b < Cipher16Bit16Configuration.SBOXNUM; b++)
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
                SBoxCharacteristic[] curDiffSBoxes = new SBoxCharacteristic[Cipher16Bit16Configuration.SBOXNUM];

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
                    for (int i = 0; i < Cipher16Bit16Configuration.SBOXNUM; i++)
                    {
                        if (activeSBoxes[i])
                        {
                            curDiffSBoxes[i] = arrayOfDifferentialLists[i][0];
                        }
                    }
                }

                //check null values
                for (int z = 0; z < Cipher16Bit16Configuration.SBOXNUM; z++)
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
                for (int i = 0; i < Cipher16Bit16Configuration.SBOXNUM; i++)
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
                for (int i = Cipher16Bit16Configuration.SBOXNUM - 1; i >= 0; i--)
                {
                    inputDiff = inputDiff ^ curDiffSBoxes[i].InputDifferential;
                    if ((i - 1) >= 0)
                    {
                        inputDiff = inputDiff << Cipher16Bit16Configuration.BITWIDTHCIPHERFOUR;
                    }
                }

                //outputDifference for previous round
                int outputDiffPreviousRound = ReversePBoxBlock(inputDiff);

                //calc new prob
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (characteristic != null && characteristic.Probability != -1)
                {
                    for (int i = 0; i < Cipher16Bit16Configuration.SBOXNUM; i++)
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
                    for (int i = 0; i < Cipher16Bit16Configuration.SBOXNUM; i++)
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
            Characteristic best = new Cipher16Bit16Characteristic();
            foreach (var curDiffs in diffList)
            {
                if (best.Probability < curDiffs.Probability)
                {
                    best = curDiffs;
                }
            }

            return best;
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
            Characteristic inputObj = new Cipher16Bit16Characteristic();

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
            //Console.WriteLine("Found " + retVal.Count + " paths with inputDifference = " + inputDiff + " and outputDifference = " + outputDiff);
#endif

            return retVal;
        }

        private List<Characteristic> FindAllCharacteristics(int round, List<SBoxCharacteristic> differentialsList, int outputDiff, Characteristic res)
        {
            //break if probability is not good enough
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if ((res.Probability < Cipher16Bit16Configuration.PROBABILITYBOUNDDIFFERENTIALSEARCH) && (res.Probability != -1))
                return null;

            //end of rekursion
            if (round == 0)
            {
                List<Characteristic> resList = new List<Characteristic>();
                resList.Add(res);
                return resList;
            }

            //contains the active SBoxes in the round
            bool[] activeSBoxes = new bool[Cipher16Bit16Configuration.SBOXNUM];

            //check active SBoxes
            int[] outputDiffs = new int[Cipher16Bit16Configuration.SBOXNUM];
            for (int i = 0; i < Cipher16Bit16Configuration.SBOXNUM; i++)
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
            List<SBoxCharacteristic>[] arrayOfDifferentialLists = new List<SBoxCharacteristic>[Cipher16Bit16Configuration.SBOXNUM];
            int comb = 1;
            for (int b = 0; b < Cipher16Bit16Configuration.SBOXNUM; b++)
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
                SBoxCharacteristic[] curDiffSBoxes = new SBoxCharacteristic[Cipher16Bit16Configuration.SBOXNUM];

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
                    for (int i = 0; i < Cipher16Bit16Configuration.SBOXNUM; i++)
                    {
                        if (activeSBoxes[i])
                        {
                            curDiffSBoxes[i] = arrayOfDifferentialLists[i][0];
                        }
                    }
                }

                //check null values
                for (int z = 0; z < Cipher16Bit16Configuration.SBOXNUM; z++)
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
                for (int i = 0; i < Cipher16Bit16Configuration.SBOXNUM; i++)
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
                for (int i = Cipher16Bit16Configuration.SBOXNUM - 1; i >= 0; i--)
                {
                    inputDiff = inputDiff ^ curDiffSBoxes[i].InputDifferential;
                    if ((i - 1) >= 0)
                    {
                        inputDiff = inputDiff << Cipher16Bit16Configuration.BITWIDTHCIPHERFOUR;
                    }
                }

                //outputDifference for previous round
                int outputDiffPreviousRound = ReversePBoxBlock(inputDiff);

                //calc new prob
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (characteristic != null && characteristic.Probability != -1)
                {
                    for (int i = 0; i < Cipher16Bit16Configuration.SBOXNUM; i++)
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
                    for (int i = 0; i < Cipher16Bit16Configuration.SBOXNUM; i++)
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
            Characteristic best = new Cipher16Bit16Characteristic();
            foreach (var curDiffs in diffList)
            {
                if (best.Probability < curDiffs.Probability)
                {
                    best = curDiffs;
                }
            }

            return diffList;
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
                bool[] conditionArray = new bool[Cipher16Bit16Configuration.SBOXNUM];

                for (int i = 0; i < Cipher16Bit16Configuration.SBOXNUM; i++)
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

                for (int i = 0; i < Cipher16Bit16Configuration.SBOXNUM; i++)
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
            //Console.WriteLine("Found " + characteristics.Count + " on SBoxes " + result.GetActiveSBoxes());
            //Console.WriteLine("Searching for the best differential...");
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
            //Console.WriteLine("Best differential probability: " + probabilityAccumulated);
            foreach (var curCharacteristic in bestCharacteristics)
            {
                //Console.WriteLine(curCharacteristic.ToString());
            }
#endif

            inputDifference = bestCharacteristics[0].InputDifferentials[0];
            expectedDifference = bestCharacteristics[0].InputDifferentials[round - 1];


            int pairCount = paircount;
            /* 
            double temp = CipherFourConfiguration.PAIRMULTIPLIER / probabilityAccumulated;
            int pairCount = (int)temp;
            */

#if DEBUG
            //Console.WriteLine("Needed PairCount = " + pairCount);
#endif

            result.InputDifference = inputDifference;
            result.ExpectedDifference = expectedDifference;
            result.Characteristics = bestCharacteristics;
            result.Probability = probabilityAccumulated;

            result.UnfilteredPairList = GenerateInputPairList(inputDifference, pairCount);
            result.FilteredPairList = FilterPairs(result, diffListOfSBox, attack, encryption, expectedDifference);
            //result.FilteredPairList = result.UnfilteredPairList;

            /*
            while (result.FilteredPairList.Count < 32)
            {
                result.UnfilteredPairList = GenerateInputPairList(inputDifference, pairCount);
                result.FilteredPairList = FilterPairs(result, diffListOfSBox, attack, encryption, expectedDifference);
                //result.FilteredPairList = result.UnfilteredPairList;
            }
            */


            Console.WriteLine("Generated " + result.UnfilteredPairList.Count + " pairs and there stayed " + result.FilteredPairList.Count + " filtered pairs");
#if DEBUG
#endif

            return result;






        }

        public List<Pair> GenerateInputPairList(int inputDifferential, int count)
        {
            List<Pair> result = new List<Pair>();

            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            bool checkDublette = true;

            if (!checkDublette)
            {
                //generate count pairs with specified inputDifferential
                for (int i = 1; i <= count; i++)
                {
                    /* 

                    */

                    //byte[] rand = new byte[4];
                    //rng.GetBytes(rand);

                    //int j = Math.Abs(BitConverter.ToInt32(rand, 0));
                    //int p0 = j % 65536;

                    int p0 = _random.Next(0, 65535);
                    //int p0 = 10000 + i;
                    int p1 = p0 ^ inputDifferential;

                    /*
                    int x = _random.Next(0, 99);
                    if( x > 50)
                        p1 = _random.Next(0, 65535);
                        */

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
            }
            else
            {
                int i = 0;
                while (result.Count < (count))
                {
                    //int p0 = _random.Next(1, 65534);
                    int p0 = i;
                    int p1 = p0 ^ inputDifferential;

                    Pair p = new Pair()
                    {
                        LeftMember = p0,
                        RightMember = p1
                    };

                    bool contains = false;
                    foreach (var pair in result)
                    {
                        //if (a,b) exists, (a,b) and (b,a) will be discarded
                        /*  
                        if (((pair.LeftMember == p.LeftMember) && (pair.RightMember == p.RightMember)) || ((pair.LeftMember == p.RightMember) && (pair.RightMember == p.LeftMember)))
                        {
                            contains = true;
                        }
                        */

                        //if (a,b) exists, (b,a) is allowed
                        /*  */
                        if ((pair.LeftMember == p.LeftMember) && (pair.RightMember == p.RightMember))
                        {
                            contains = true;
                        }
                        
                    }

                    if (!contains)
                    {
                        result.Add(p);
                    }

                    i++;
                }



                
            }

            return result;
        }

        public int GenerateValue(bool[] activeSBoxes, int data)
        {
            BitArray bitsOfValue = new BitArray(BitConverter.GetBytes(data));
            BitArray result = new BitArray(32, false);

            int currentActiveBitPosition = 0;
            for (int i = 0; i < Cipher16Bit16Configuration.SBOXNUM; i++)
            {
                if (activeSBoxes[i])
                {
                    for (int j = 0; j < Cipher16Bit16Configuration.BITWIDTHCIPHERFOUR; j++, currentActiveBitPosition++)
                    {
                        result[(i * Cipher16Bit16Configuration.BITWIDTHCIPHERFOUR) + j] = bitsOfValue[currentActiveBitPosition];
                    }
                }
            }

            byte[] bytesOfResult = new byte[4];
            result.CopyTo(bytesOfResult, 0);

            return BitConverter.ToInt32(bytesOfResult, 0);
        }

        public BitArray GetBitsOfInt(int value)
        {
            return new BitArray(BitConverter.GetBytes(value));
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

        public int PartialDecrypt(DifferentialKeyRecoveryAttack attack, int block)
        {
            Cipher16Bit16DifferentialKeyRecoveryAttack at = attack as Cipher16Bit16DifferentialKeyRecoveryAttack;
            int result = block;

            if (at != null && at.recoveredSubkey3)
            {
                result = DecryptSingleRound(result, at.subkey4, false, true);
            }

            if (at != null && at.recoveredSubkey2)
            {
                result = DecryptSingleRound(result, at.subkey3, true, false);
            }

            return result;
        }

        public string PrintKeyBits(int key)
        {
            throw new NotImplementedException();
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

            //for (int i = 0; i < loopBorder; i++)
            Parallel.For(0, loopBorder, i =>
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

                    bool[] conditions = new bool[] { true, true, true, true };
                    BitArray diff = GetBitsOfInt(configuration.ExpectedDifference);
                    BitArray leftMemberBits = GetBitsOfInt(leftMemberSingleDecrypted);
                    BitArray rightMemberBits = GetBitsOfInt(rightMemberSingleDecrypted);

                    for (int j = 0; j < Cipher16Bit16Configuration.SBOXNUM; j++)
                    {
                        if (configuration.ActiveSBoxes[j])
                        {
                            int shift = 15 << (4 * j);

                            int bit4diff = configuration.ExpectedDifference & shift;
                            int bit4left = leftMemberSingleDecrypted & shift;
                            int bit4right = rightMemberSingleDecrypted & shift;

                            if ((bit4left ^ bit4right) == bit4diff)
                            {
                                conditions[j] = true;
                            }
                            else
                            {
                                conditions[j] = false;
                            }
                        }
                    }

                    if (conditions[0] && conditions[1] && conditions[2] && conditions[3])
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
            });

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
            foreach (var curKeyProbability in roundResult.KeyCandidateProbabilities.GetRange(0, 4))
            {
                Console.WriteLine("Guessed key: " + curKeyProbability.Key + " Count: " + curKeyProbability.Counter + " Probability: {0:N4}", ((curKeyProbability.Counter / (double)configuration.UnfilteredPairList.Count)));
            }
#endif

            return roundResult;
        }

        public DifferentialAttackRoundConfiguration RefreshPairLists(DifferentialAttackRoundConfiguration roundConfig, List<SBoxCharacteristic> diffListOfSBox, DifferentialKeyRecoveryAttack attack, IEncryption encryption)
        {
            throw new NotImplementedException();
        }

        public int ReversePBoxBlock(int data)
        {
            BitArray bitsOfBlock = new BitArray(BitConverter.GetBytes(data));
            BitArray pboxedArray = new BitArray(16);

            //use pbox
            for (int i = 0; i < 16; i++)
            {
                pboxedArray[Cipher16Bit16Configuration.PBOXREVERSE[i]] = bitsOfBlock[i];
            }

            byte[] bytes = new byte[4];
            pboxedArray.CopyTo(bytes, 0);

            int outputBlock = BitConverter.ToInt32(bytes, 0);
            return outputBlock;
        }

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
            zeroToThreeInt = Cipher16Bit16Configuration.SBOXREVERSE[zeroToThreeInt];
            fourToSevenInt = Cipher16Bit16Configuration.SBOXREVERSE[fourToSevenInt];
            eightToElevenInt = Cipher16Bit16Configuration.SBOXREVERSE[eightToElevenInt];
            twelveToFifteenInt = Cipher16Bit16Configuration.SBOXREVERSE[twelveToFifteenInt];

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
    }
}
