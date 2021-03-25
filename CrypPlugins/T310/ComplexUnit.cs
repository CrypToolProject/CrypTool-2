using System;
using System.Collections.Generic;

namespace CrypTool.Plugins.T310
{

    /// <summary>
    /// This class derives the random bits from the key used for encryption
    /// </summary>
    /// 
    /// The structure of this algorithm resembles a block cipher, as it operates on the
    /// register U, which is 36 bits long. Therefore some vocabulary from block ciphers may appear
    /// here and there in the names. Needed for the operation is the long term key,
    /// the key and the initialisation vector. The complex unit is called once per character and returns
    /// 13 bits, which are then processed to encrypt a character.
    class ComplexUnit
    {

        /// <summary>
        /// A dictionary containing a selection of long term keys. 
        /// </summary>
        /// There are mainly keys actually used in daily operation,
        /// including they keys with number 14, 15, 16, 21, 26, 29, 30, 31, 32, 33
        private static Dictionary<LongTermKeyEnum, byte[]> longTermKeys = new Dictionary<LongTermKeyEnum, byte[]>
        {
            { LongTermKeyEnum.LZS14 , new byte[] { 24, 34, 33, 32, 14, 4, 5, 28, 9, 26, 27, 18, 36, 16, 21, 15, 20, 25, 35, 8, 1,
                6, 23, 29, 19, 12, 13, 0, 27, 23, 11, 15, 31, 35, 3, 19, 30 } },
            { LongTermKeyEnum.LZS15 , new byte[] { 15, 13, 33, 34, 6, 8, 5, 3, 9, 18, 14, 22, 28, 30, 21, 31, 7, 25, 26, 16, 27,
                11, 23, 29, 19, 1, 36, 0, 3, 16, 11, 34, 31, 1, 23, 19, 10 } },
            { LongTermKeyEnum.LZS16, new byte[] { 14, 19, 33, 18, 23, 15, 5, 6, 9, 2, 34, 1, 30, 11, 21, 3, 22, 25, 17, 7, 32, 10,
                27, 29, 26, 35, 13, 0, 34, 18, 22, 26, 10, 2, 14, 30, 18 } },
            { LongTermKeyEnum.LZS17, new byte[] {22, 23, 33, 11 ,26, 12, 5, 4, 9, 3, 2, 1, 19, 10, 21, 8, 7, 25, 6, 35, 32, 31,
                30, 29, 17, 17, 34, 0, 3, 7, 11, 15, 19, 23, 27, 31, 36 } },
            { LongTermKeyEnum.LZS21, new byte[] { 36, 4, 33, 11, 1, 20, 5, 26, 9, 24, 32, 7, 12, 2, 21, 3, 28, 25, 34, 8, 31, 13,
                18, 29, 16, 19, 6, 0, 23, 35, 3, 15, 27, 11, 19, 31, 1 } },
            { LongTermKeyEnum.LZS26, new byte[] { 8, 4, 33, 16, 31, 20, 5, 35, 9, 3, 19, 18, 12, 7, 21, 13, 23, 25, 28, 36, 24,
                15, 26, 29, 27, 32, 11, 0, 27, 1, 31, 23, 7, 11, 19, 15, 4} },
            { LongTermKeyEnum.LZS29, new byte[] { 28, 8, 33, 23, 11, 12, 5, 10, 9, 30, 19, 18, 4, 31, 21, 24, 13, 25, 22, 32, 20,
                36, 27, 29, 7, 16, 15, 0, 35, 27, 19, 23, 15, 3, 11, 7, 1} },
            { LongTermKeyEnum.LZS30, new byte[] { 8, 28, 33, 3, 27, 20, 5, 16, 9, 1, 19, 23, 4, 2, 21, 36, 30, 25, 11, 24, 12,
                18, 7, 29, 32, 6, 35, 0, 35, 7, 27, 11, 31, 3, 19, 15, 3} },
            { LongTermKeyEnum.LZS31, new byte[] { 7, 4, 33, 30, 18, 36, 5, 35, 9, 16, 23, 26, 32, 12, 21, 1, 13, 25, 20, 8, 24, 15,
                22, 29, 10, 28, 6, 0, 15, 3, 23, 11, 27, 31, 35, 19, 2} },
            { LongTermKeyEnum.LZS32, new byte[] { 27, 30, 33, 24, 11, 36, 5, 20, 9, 23, 1, 34, 16, 14, 21, 8, 28, 25, 22, 32, 4,
                10, 13, 29, 15, 12, 18, 0, 19, 23, 3, 27, 7, 15, 35, 11, 4} },
            { LongTermKeyEnum.LZS33, new byte[] { 24, 3, 33, 30, 2, 8, 5, 12, 9, 1, 10, 6, 32, 22, 21, 18, 28, 25, 16, 20, 36, 13,
                17, 29, 26, 4, 35, 0, 11, 23, 35, 27, 15, 31, 7, 3, 3} }
    };


        private SynchronizationUnit synchronizationUnit;
        private ControlUnit controlUnit;

        //Long term keys, default is key 31
        public byte[] D = new byte[] { 0, 15, 3, 23, 11, 27, 31, 35, 19 };
        public byte[] P = new byte[] { 7, 4, 33, 30, 18, 36, 5, 35, 9, 16, 23, 26, 32, 12, 21, 1, 13, 25, 20, 8, 24, 15, 22, 29, 10, 28, 6 };
        public byte[] alpha = new byte[] { 2 };


        public ComplexUnit(SynchronizationUnit synchronizationUnit, ControlUnit controlUnit)
        {
            this.synchronizationUnit = synchronizationUnit;
            this.controlUnit = controlUnit;
        }

        /// <summary>
        /// Select a long term key given 
        /// </summary>
        /// <param name="longTermKey"></param>
        public void SetLongTermKey(LongTermKeyEnum longTermKey)
        {
            byte[] tmpKey;
            longTermKeys.TryGetValue(longTermKey, out tmpKey);

            Array.Copy(tmpKey, 0, P, 0, 27);
            Array.Copy(tmpKey, 27, D, 0, 9);
            Array.Copy(tmpKey, 35, alpha, 0, 1);
        }


        /// <summary>
        /// The 36 bit register U is the main component of the complex unit
        /// </summary>
        /// 
        /// Its initialization value is 0110 1001 1100 0111 1100 1000 0101 1010 0011
        /// which translates to 0x69:C7:85:C3
        private ulong u = 0x69C7C85A3;
        private uint a;

        /// <summary>
        /// Derives 13 bits from the key in a way similar to a block cipher
        /// </summary>
        /// 
        ///  Get a value from the T-310 block cipher - after 1651 rounds 13 bits
        /// are complete. In the actual machine it takes another 14th round to add the final bit to the result but does
        /// not use the last bit. Therefore we call the function with 1778 rounds.
        /// 
        /// <returns>an int filled with 13 bits (at the lower position)</returns>
        public uint DeriveBitsFromKey()
        {
            a = 0;
            commitRounds(1778);
            return a;
        }

        /// <summary>
        /// Take a specific amount of rounds at the beginning of the algorithm
        /// </summary>
        /// For an uncertain reason the first bits of the complex units after initialitation are not used.
        /// Those 133 rounds can be taken through this function.
        public void initialRounds()
        {
            commitRounds(133);
            a = 0;
        }

        /// <summary>
        /// Rotate the T-310 block cipher
        /// </summary>
        /// 
        /// The core of this algorithm is the register U which shifts influenced by the key. After
        /// every 127th round a bit is taken from U and added to the result a. 
        /// <param name="rounds"></param>
        private void commitRounds(uint rounds)
        {
            byte[] T = new byte[10];

            uint unevenRound = rounds % 127;
            if (unevenRound > 0)
                rounds -= unevenRound;
            uint outerRoundMax = rounds / 13; 

            for (byte roundCount = 0; roundCount < outerRoundMax; roundCount++)
            {
                // Start 127 rounds to get one bit for the result. Every turn we get a bit from the key S1 which we shift afterwards
                for (byte innerRound = 0; innerRound < 127; innerRound++, controlUnit.ShiftS1(), controlUnit.ShiftS2())
                {
                    /*
                     * This part is quite important. We take some parts of the key S2, initialization vector and U and put them through the 
                     * Z functions and reduce them to 9 bit which we fill into T. Which part of U we take is determined by the
                     * mappings of the long term key.
                     * 
                     * Note that in the logical circuits T would not exist and it would all occur parallel in one clock pulse
                     */
                    T[9] = (byte)(T[8] ^ GetU(P[26]));
                    T[8] = (byte)(T[7] ^ Z(GetU(P[20]), GetU(P[21]), GetU(P[22]), GetU(P[23]), GetU(P[24]), GetU(P[25])));
                    T[7] = (byte)(T[6] ^ GetU(P[19]));
                    T[6] = (byte)(T[5] ^ controlUnit.GetS2Bit() ^ Z(GetU(P[13]), GetU(P[14]), GetU(P[15]), GetU(P[16]), GetU(P[17]), GetU(P[18])));
                    T[5] = (byte)(T[4] ^ GetU(P[12]));
                    T[4] = (byte)(T[3] ^ Z(GetU(P[6]), GetU(P[7]), GetU(P[8]), GetU(P[9]), GetU(P[10]), GetU(P[11])));
                    T[3] = (byte)(T[2] ^ GetU(P[5]));
                    T[2] = (byte)(T[1] ^ Z(controlUnit.GetS2Bit(), GetU(P[0]), GetU(P[1]), GetU(P[2]), GetU(P[3]), GetU(P[4])));
                    T[1] = synchronizationUnit.GetFBit();

                    //Copy u to make it easier to shift in the next step
                    ulong uOld = u;

                    /*
                     * Now the 9 bits of t T gets shifted back into U at every 4th position (36 / 9).
                     * The rest of the U bits shifts for one. 
                     *
                     * Note how small the amount of influence of the key per round is.
                     */
                    for (int j = 9; j >= 1; j--)
                    {
                        u &= ~(15ul << (4 * (j - 1)));
                        u |= (ulong)((GetU(D[9 - j]) ^ T[j])) << (4 * j - 1);
                        u |= ((uOld >> (4 * j - 1)) & 0x01ul) << (4 * j - 2);
                        u |= ((uOld >> (4 * j - 2)) & 0x01ul) << (4 * j - 3);
                        u |= ((uOld >> (4 * j - 3)) & 0x01ul) << (4 * j - 4);
                    }

                    /*
                     * After shifting the U register XOR the least significant digit (which is represented by the most significant bit) to the bit from S1 key 
                     */
                    u ^= (ulong)controlUnit.GetS1Bit() << 35;

                }

                // Take the bit which is applied in alpha and add it to the result
                a |= ((u >> (alpha[0] - 1) & 1ul) > 0 ? 1u : 0u) << roundCount;
            }
        }

        /// <summary>
        /// A non-linear boolean function which takes 6 bits as input.
        /// </summary>
        /// 
        /// 
        /// <param name="b1">bit 1</param>
        /// <param name="b2">bit 2</param>
        /// <param name="b3">bit 3</param>
        /// <param name="b4">bit 4</param>
        /// <param name="b5">bit 5</param>
        /// <param name="b6">bit 6</param>
        /// <returns> a byte which is either 0 or 1</returns>
        private byte Z(byte b1, byte b2, byte b3, byte b4, byte b5, byte b6)
        {
            byte res = 1;
            res = (byte)(res ^ b1 ^ b5 ^ b6
           ^ (b1 & b4) ^ (b2 & b3) ^ (b2 & b5) ^ (b4 & b5) ^ (b5 & b6)
           ^ (b1 & b3 & b4) ^ (b1 & b3 & b6) ^ (b1 & b4 & b5) ^ (b2 & b3 & b6) ^ (b2 & b4 & b6) ^ (b3 & b5 & b6)
           ^ (b1 & b2 & b3 & b4) ^ (b1 & b2 & b3 & b5) ^ (b1 & b2 & b5 & b6) ^ (b2 & b3 & b4 & b6)
           ^ (b1 & b2 & b3 & b4 & b5) ^ (b1 & b3 & b4 & b5 & b6));
            return res;
        }

        /// \brief Pass a mapping from the long term key and get a bit from U
        /// 
        /// Can either be from D, P or &alpha
        /// 
        /// \returns a byte which is either 0 or 1

        /// <summary>
        /// Pass a mapping from the long term key and get a bit from U
        /// </summary>
        /// <param name="p">the index of a bit in U</param>
        /// <returns>the requested bit</returns>
        private byte GetU(int p)
        {
            if (p == 0)
                return controlUnit.GetS1Bit();
            byte shift = (byte)(36 - p);
            byte result = (byte)((u >> shift) & 0x01);
            return (byte)((u >> (36 - p)) & 0x01);
        }

        /// <summary>
        /// Resets the unit by writing the default value into the U register
        /// </summary>
        public void ResetUnit()
        {
            u = 0x69C7C85A3;
        }

    }
}
