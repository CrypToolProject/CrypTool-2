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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DCAKeyRecovery.Logic.Cipher1
{
    public class Cipher1KeyRecovery : IKeyRecovery
    {
        public event EventHandler<EventArgs> NeedMessagePairOccured;
        public event EventHandler<ResultViewLastRoundRoundResultEventArgs> ResultViewRefreshRoundFinishedOccured;
        public event EventHandler<ResultViewLastRoundEventArgs> LastRoundResultViewRefreshOccured;
        public event EventHandler<ResultViewAnyRoundEventArgs> AnyRoundResultViewRefreshOccured;
        public event EventHandler<ResultViewAnyRoundKeyResultEventArgs> AnyRoundResultViewKeyResultsRefreshOccured;
        public event EventHandler<ProgressEventArgs> ProgressChangedOccured;

        public AutoResetEvent DataReceivedEvent;
        private readonly List<Pair> _plainTextPairList;
        private readonly List<Pair> _cipherTextPairList;
        private int usedPairCount = 0;
        public bool stop;
        public bool refreshUi;

        /// <summary>
        /// Constructor
        /// </summary>
        public Cipher1KeyRecovery()
        {
            stop = false;
            _plainTextPairList = new List<Pair>();
            _cipherTextPairList = new List<Pair>();
            DataReceivedEvent = new AutoResetEvent(false);
        }

        /// <summary>
        /// Adds new pairs
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="cipherText"></param>
        public void AddNewPairs(Pair plaintext, Pair cipherText)
        {
            usedPairCount++;
            usedPairCount++;
            _plainTextPairList.Add(plaintext);
            _cipherTextPairList.Add(cipherText);
            DataReceivedEvent.Set();
        }

        /// <summary>
        /// Recovers k0 and k1
        /// </summary>
        /// <param name="attack"></param>
        /// <returns></returns>
        public DifferentialAttackLastRoundResult AttackFirstRound(DifferentialKeyRecoveryAttack attack)
        {
            Cipher1DifferentialKeyRecoveryAttack cipher1Attack = attack as Cipher1DifferentialKeyRecoveryAttack;
            DifferentialAttackLastRoundResult result = new DifferentialAttackLastRoundResult();
            ResultViewLastRoundEventArgs lastRoundEventArgsIterationResultViewLastRound = null;
            int decryptionCounter = 0;
            int keyCounter = 0;
            usedPairCount = 2;
            int roundCounter = 0;
            double progress = 0.0;
            ProgressEventArgs e;

            List<int> possibleKeyList = new List<int>();
            for (int i = 0; i <= 65535; i++)
            {
                possibleKeyList.Add(i);
            }

            //exit thread
            if (stop)
            {
                return null;
            }

            bool found = false;

            if (cipher1Attack == null)
            {
                throw new ArgumentNullException(nameof(attack));
            }

            DateTime startTime = DateTime.Now;

            //recover k1
            while (!found)
            {
                //exit thread
                if (stop)
                {
                    return null;
                }

                roundCounter++;

                if (_plainTextPairList.Count == 0)
                {
                    DataReceivedEvent.Reset();
                    if (NeedMessagePairOccured != null)
                    {
                        NeedMessagePairOccured.Invoke(this, null);
                    }

                    DataReceivedEvent.WaitOne();
                }

                if (stop)
                {
                    return null;
                }

                //generate plaintext pair
                Pair inputPair = new Pair()
                {
                    LeftMember = _plainTextPairList[0].LeftMember,
                    RightMember = _plainTextPairList[0].RightMember
                };

                //generate ciphertext pair
                Pair encryptedPair = new Pair()
                {
                    LeftMember = _cipherTextPairList[0].LeftMember,
                    RightMember = _cipherTextPairList[0].RightMember
                };

                _plainTextPairList.Clear();
                _cipherTextPairList.Clear();

                ushort expectedDiff = (ushort)(inputPair.LeftMember ^ inputPair.RightMember);
                List<int> temp = possibleKeyList.ToList();
                int keysToTest = temp.Count;
                int refreshCounter = 0;
                foreach (int item in temp)
                {
                    //exit thread
                    if (stop)
                    {
                        return null;
                    }

                    refreshCounter++;
                    ushort decryptedLeftMember = (ushort)(encryptedPair.LeftMember ^ item);
                    ushort decryptedRightMember = (ushort)(encryptedPair.RightMember ^ item);
                    decryptionCounter++;
                    decryptionCounter++;
                    keyCounter++;

                    decryptedLeftMember = ReverseSBoxBlock(decryptedLeftMember);
                    decryptedRightMember = ReverseSBoxBlock(decryptedRightMember);

                    int decryptedBlocksDiff = decryptedLeftMember ^ decryptedRightMember;

                    if (refreshUi && (refreshCounter % 100 == 0 || (refreshCounter) == keysToTest))
                    {
                        //refresh UI
                        lastRoundEventArgsIterationResultViewLastRound = new ResultViewLastRoundEventArgs()
                        {
                            startTime = startTime,
                            endTime = DateTime.MinValue,
                            currentPlainText = "m0: " + inputPair.LeftMember.ToString("X") + " m1: " + inputPair.RightMember.ToString("X"),
                            currentCipherText = "c0: " + encryptedPair.LeftMember.ToString("X") + " c1: " + encryptedPair.RightMember.ToString("X"),
                            currentKeyCandidate = Convert.ToString((ushort)item, 2).PadLeft(16, '0'),
                            expectedDifference = expectedDiff.ToString("X"),
                            round = 1,
                            currentKeysToTestThisRound = keysToTest,
                            remainingKeyCandidates = possibleKeyList.Count,
                            examinedPairCount = usedPairCount
                        };

                        if (LastRoundResultViewRefreshOccured != null)
                        {
                            LastRoundResultViewRefreshOccured.Invoke(this,
                                lastRoundEventArgsIterationResultViewLastRound);
                        }
                    }

                    if (decryptedBlocksDiff != expectedDiff)
                    {
                        possibleKeyList.Remove(item);
                    }

                    if (refreshUi && possibleKeyList.Count % 800 == 0)
                    {
                        progress += 0.01;

                        e = new ProgressEventArgs()
                        {
                            Increment = 0.01
                        };
                        if (ProgressChangedOccured != null)
                        {
                            ProgressChangedOccured.Invoke(this, e);
                        }
                    }
                }

                //Prepare event args
                RoundResult roundResult = new RoundResult()
                {
                    RemainingKeys = possibleKeyList.Count,
                    RoundNumber = roundCounter
                };

                ResultViewLastRoundRoundResultEventArgs eventArgsLastRoundRound = new ResultViewLastRoundRoundResultEventArgs()
                {
                    RoundResult = roundResult
                };

                //invoke event
                if (ResultViewRefreshRoundFinishedOccured != null)
                {
                    ResultViewRefreshRoundFinishedOccured.Invoke(this, eventArgsLastRoundRound);
                }

                //check if k1 is recovered
                if (possibleKeyList.Count == 1)
                {
                    found = true;

                    lastRoundEventArgsIterationResultViewLastRound = new ResultViewLastRoundEventArgs()
                    {
                        startTime = startTime,
                        endTime = DateTime.MinValue,
                        currentPlainText = "m0: " + inputPair.LeftMember.ToString("X") + " m1: " + inputPair.RightMember.ToString("X"),
                        currentCipherText = "c0: " + encryptedPair.LeftMember.ToString("X") + " c1: " + encryptedPair.RightMember.ToString("X"),
                        currentKeyCandidate = Convert.ToString((ushort)possibleKeyList[0], 2).PadLeft(16, '0'),
                        expectedDifference = expectedDiff.ToString("X"),
                        round = 1,
                        currentKeysToTestThisRound = 1,
                        remainingKeyCandidates = 1,
                        examinedPairCount = usedPairCount
                    };

                    if (LastRoundResultViewRefreshOccured != null)
                    {
                        LastRoundResultViewRefreshOccured.Invoke(this, lastRoundEventArgsIterationResultViewLastRound);
                    }
                }
                else if (possibleKeyList.Count == 0)
                {
                    return null;
                }
            }

            double inc = 0.9 - progress;
            progress = 0.9;
            e = new ProgressEventArgs()
            {
                Increment = inc
            };

            if (ProgressChangedOccured != null)
            {
                ProgressChangedOccured.Invoke(this, e);
            }

            result.SubKey1 = (ushort)possibleKeyList[0];

            if (_plainTextPairList.Count == 0)
            {
                DataReceivedEvent.Reset();
                if (NeedMessagePairOccured != null)
                {
                    NeedMessagePairOccured.Invoke(this, null);
                }

                DataReceivedEvent.WaitOne();
            }

            Pair inputPair2 = new Pair()
            {
                LeftMember = _plainTextPairList[0].LeftMember,
                RightMember = _plainTextPairList[0].RightMember
            };

            Pair encryptedPair2 = new Pair()
            {
                LeftMember = _cipherTextPairList[0].LeftMember,
                RightMember = _cipherTextPairList[0].RightMember
            };

            //exit thread
            if (stop)
            {
                return null;
            }

            //recover k0
            ushort plainText = inputPair2.LeftMember;
            ushort cipherText = encryptedPair2.LeftMember;

            cipherText = (ushort)(cipherText ^ result.SubKey1);
            cipherText = ReverseSBoxBlock(cipherText);

            result.SubKey0 = (ushort)(cipherText ^ plainText);

            decryptionCounter++;
            keyCounter++;

            inc = 1.0 - progress;
            progress = 1.0;
            e = new ProgressEventArgs()
            {
                Increment = inc
            };

            if (ProgressChangedOccured != null)
            {
                ProgressChangedOccured.Invoke(this, e);
            }

            //refresh UI
            if (lastRoundEventArgsIterationResultViewLastRound != null)
            {
                lastRoundEventArgsIterationResultViewLastRound.endTime = DateTime.Now;
                if (LastRoundResultViewRefreshOccured != null)
                {
                    LastRoundResultViewRefreshOccured.Invoke(this, lastRoundEventArgsIterationResultViewLastRound);
                }
            }

            result.DecryptionCounter = decryptionCounter;
            result.KeyCounter = keyCounter;

            return result;
        }

        /// <summary>
        /// not needed for cipher1
        /// </summary>
        /// <param name="activeSBoxes"></param>
        /// <returns></returns>
        public int CalculateLoopBorder(bool[] activeSBoxes)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// not needed for cipher1
        /// </summary>
        /// <param name="activeSBoxes"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ushort GenerateValue(bool[] activeSBoxes, ushort data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// not needed for cipher1
        /// </summary>
        /// <param name="attack"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public DifferentialAttackRoundResult RecoverKeyInformation(DifferentialKeyRecoveryAttack attack, DifferentialAttackRoundConfiguration configuration)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reverses a 16 bit SBox block
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ushort ReverseSBoxBlock(ushort data)
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

            ushort zeroToThreeInt = BitConverter.ToUInt16(zeroToThreeBytes, 0);
            ushort fourToSevenInt = BitConverter.ToUInt16(fourToSevenBytes, 0);
            ushort eightToElevenInt = BitConverter.ToUInt16(eightToElevenBytes, 0);
            ushort twelveToFifteenInt = BitConverter.ToUInt16(twelveToFifteenBytes, 0);

            //use sbox
            zeroToThreeInt = Cipher1Configuration.SBOXREVERSE[zeroToThreeInt];
            fourToSevenInt = Cipher1Configuration.SBOXREVERSE[fourToSevenInt];
            eightToElevenInt = Cipher1Configuration.SBOXREVERSE[eightToElevenInt];
            twelveToFifteenInt = Cipher1Configuration.SBOXREVERSE[twelveToFifteenInt];

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
            ushort combined = BitConverter.ToUInt16(bytes, 0);

            return combined;
        }

        /// <summary>
        /// Applies the SBox
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ushort SBox(ushort data)
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

            ushort zeroToThreeInt = BitConverter.ToUInt16(zeroToThreeBytes, 0);
            ushort fourToSevenInt = BitConverter.ToUInt16(fourToSevenBytes, 0);
            ushort eightToElevenInt = BitConverter.ToUInt16(eightToElevenBytes, 0);
            ushort twelveToFifteenInt = BitConverter.ToUInt16(twelveToFifteenBytes, 0);

            //use sbox
            zeroToThreeInt = Cipher1Configuration.SBOX[zeroToThreeInt];
            fourToSevenInt = Cipher1Configuration.SBOX[fourToSevenInt];
            eightToElevenInt = Cipher1Configuration.SBOX[eightToElevenInt];
            twelveToFifteenInt = Cipher1Configuration.SBOX[twelveToFifteenInt];

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
            ushort combined = BitConverter.ToUInt16(bytes, 0);

            return combined;
        }

        public ushort PartialDecrypt(DifferentialKeyRecoveryAttack attack, ushort block)
        {
            throw new NotImplementedException();
        }

        public ushort DecryptSingleRound(ushort block, ushort key, bool beforeLast, bool isLast)
        {
            throw new NotImplementedException();
        }

        public ushort ReversePBoxBlock(ushort data)
        {
            throw new NotImplementedException();
        }

        public ushort ApplyPBoxToBlock(ushort data)
        {
            throw new NotImplementedException();
        }
    }
}
