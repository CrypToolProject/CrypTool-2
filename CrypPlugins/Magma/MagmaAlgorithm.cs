/*
   Copyright 2022 Nils Kopal, CrypTool project

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
using System.Runtime.CompilerServices;
using System;

namespace CrypTool.Plugins.Magma
{
    /// <summary>
    /// Implementation of the GOST block cipher "Magma", defined in the standard: GOST R 34.12-2015: Block Cipher "Magma"
    /// Standard:  https://datatracker.ietf.org/doc/html/rfc8891
    /// Wikipedia: https://en.wikipedia.org/wiki/GOST_(block_cipher)
    /// Magma is a Soviet and Russian government standard symmetric key block cipher with a block size of 64 bits and a keysize of 256 bits
    /// </summary>
    public class MagmaAlgorithm
    {
        //This is used to store intermediate data during execution of Magma. We use this to display the data in the
        //presentation of the component
        private readonly MagmaCompleteData _magmaCompleteDate = new MagmaCompleteData();

        #region 4bit S-boxes definitions

        /// <summary>
        /// S-box definitions of: GOST R 34.12-2015
        /// </summary>     
        public static readonly byte[,] SBOX_GOST_R_34_12_2015N = new byte[,]
        {
            {0xC, 0x4, 0x6, 0x2, 0xA, 0x5, 0xB, 0x9, 0xE, 0x8, 0xD, 0x7, 0x0, 0x3, 0xF, 0x1}, // 4bit S-box 7
            {0x6, 0x8, 0x2, 0x3, 0x9, 0xA, 0x5, 0xC, 0x1, 0xE, 0x4, 0x7, 0xB, 0xD, 0x0, 0xF}, // 4bit S-box 6
            {0xB, 0x3, 0x5, 0x8, 0x2, 0xF, 0xA, 0xD, 0xE, 0x1, 0x7, 0x4, 0xC, 0x9, 0x6, 0x0}, // 4bit S-box 5
            {0xC, 0x8, 0x2, 0x1, 0xD, 0x4, 0xF, 0x6, 0x7, 0x0, 0xA, 0x5, 0x3, 0xE, 0x9, 0xB}, // 4bit S-box 4
            {0x7, 0xF, 0x5, 0xA, 0x8, 0x1, 0x6, 0xD, 0x0, 0x9, 0x3, 0xE, 0xB, 0x4, 0x2, 0xC}, // 4bit S-box 3
            {0x5, 0xD, 0xF, 0x6, 0x9, 0x2, 0xC, 0xA, 0xB, 0x7, 0x8, 0x1, 0x4, 0x3, 0xE, 0x0}, // 4bit S-box 2
            {0x8, 0xE, 0x2, 0x5, 0x6, 0x9, 0x1, 0xC, 0xF, 0x4, 0xB, 0x0, 0xD, 0xA, 0x3, 0x7}, // 4bit S-box 1
            {0x1, 0x7, 0xE, 0xD, 0x0, 0x5, 0x8, 0x3, 0x4, 0xF, 0xA, 0x6, 0x9, 0xC, 0xB, 0x2}  // 4bit S-box 0
        };

        /// <summary>
        /// S-Box definitions of: Central Bank of Russian Federation:
        /// </summary>
        public static readonly byte[,] SBOX_CENTRAL_BANK_OF_RUSSIAN_FEDERATION = new byte[,]
        {
            {0x4, 0xA, 0x9, 0x2, 0xD, 0x8, 0x0, 0xE, 0x6, 0xB, 0x1, 0xC, 0x7, 0xF, 0x5, 0x3}, // 4bit S-box 7
            {0xE, 0xB, 0x4, 0xC, 0x6, 0xD, 0xF, 0xA, 0x2, 0x3, 0x8, 0x1, 0x0, 0x7, 0x5, 0x9}, // 4bit S-box 6
            {0x5, 0x8, 0x1, 0xD, 0xA, 0x3, 0x4, 0x2, 0xE, 0xF, 0xC, 0x7, 0x6, 0x0, 0x9, 0xB}, // 4bit S-box 5
            {0x7, 0xD, 0xA, 0x1, 0x0, 0x8, 0x9, 0xF, 0xE, 0x4, 0x6, 0xC, 0xB, 0x2, 0x5, 0x3}, // 4bit S-box 4
            {0x6, 0xC, 0x7, 0x1, 0x5, 0xF, 0xD, 0x8, 0x4, 0xA, 0x9, 0xE, 0x0, 0x3, 0xB, 0x2}, // 4bit S-box 3
            {0x4, 0xB, 0xA, 0x0, 0x7, 0x2, 0x1, 0xD, 0x3, 0x6, 0x8, 0x5, 0x9, 0xC, 0xF, 0xE}, // 4bit S-box 2
            {0xD, 0xB, 0x4, 0x1, 0x3, 0xF, 0x5, 0x9, 0x0, 0xA, 0xE, 0x7, 0x6, 0x8, 0x2, 0xC}, // 4bit S-box 1
            {0x1, 0xF, 0xD, 0x0, 0x5, 0x7, 0xA, 0x4, 0x9, 0x2, 0x3, 0xE, 0x6, 0xB, 0x8, 0xC}  // 4bit S-box 0
        };

        #endregion

        private readonly byte[,] _sboxes;
        private readonly UInt32[] _roundkeys;

        /// <summary>
        /// Creates a GOST cipher using the SBOX_GOST_R_34_12_2015N as default S-boxes
        /// </summary>
        public MagmaAlgorithm(UInt32[] roundkeys) : this(sboxes: SBOX_GOST_R_34_12_2015N, roundkeys)
        {
        }

        /// <summary>
        /// Creates a GOST cipher using the given 4bit S-boxes
        /// </summary>
        /// <param name="sboxes"></param>
        /// <exception cref="ArgumentException"></exception>
        public MagmaAlgorithm(byte[,] sboxes, UInt32[] roundkeys)
        {
            if (sboxes.Rank != 2)
            {
                throw new ArgumentException(string.Format("Invald sboxes rank: {0}. Has to be 2.", sboxes.Rank));
            }
            if (sboxes.Length != 128)
            {
                throw new ArgumentException(string.Format("Invald sboxes length: {0}. Has to be 128.", sboxes.Length));
            }
            if (roundkeys.Length != 8)
            {
                throw new ArgumentException(string.Format("Invald roundkeys length: {0}. Has to be 8.", roundkeys.Length));
            }

            //instead of using 8x 4bit S-boxes, we generate 4x 8bit S-boxes to increase encryption and decryption performance
            _sboxes = Generate8BitSboxes(sboxes);
            _roundkeys = roundkeys;
        }

        /// <summary>
        /// Creates a GOST cipher and extracts the 4bit S-boxes from the byte array
        /// </summary>
        /// <param name="sboxesArray"></param>
        /// <param name="roundKeys"></param>
        public MagmaAlgorithm(byte[] sboxesArray, uint[] roundKeys)
        {
            if (sboxesArray.Length != 128)
            {
                throw new ArgumentException(string.Format("Invald sboxes array length: {0}. Has to be 128.", sboxesArray.Length));
            }

            //convert byte array to two-dimensional byte array
            byte[,] sboxes = new byte[8, 16];
            int offset = 0;
            for(int i = 0; i < 8; i++)
            {
                for(int j = 0; j < 16; j++)
                {
                    sboxes[i, j] = sboxesArray[offset];
                    offset++;
                }
            }

            //instead of using 8x 4bit S-boxes, we generate 4x 8bit S-boxes to increase encryption and decryption performance
            _sboxes = Generate8BitSboxes(sboxes);
            _roundkeys = roundKeys;
        }

        /// <summary>
        /// Encrypts a 64bit block
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public UInt64 EncryptBlock(UInt64 block)
        {
            //Feistel cipher: so split into left and right part of block
            UInt32 left = (UInt32)(block >> 32);
            UInt32 right = (UInt32)(block & 0xFFFFFFFF);

            //round 1 to 32
            for (int round = 0; round < 32; round++)
            {
                uint roundkey = _roundkeys[round < 24 ? round % 8 : 31 - round];
                
                //store intermedia input left, input right, and round key values
                _magmaCompleteDate.LeftIn[round] = string.Format("{0:x}",left);
                _magmaCompleteDate.RightIn[round] = string.Format("{0:x}", right);
                _magmaCompleteDate.RoundKey[round] = string.Format("K{0}={1:x}", round < 24 ? round % 8 : 31 - round, roundkey);

                left ^= RoundFunction(right, roundkey);

                //store intermedia output left and output right values
                _magmaCompleteDate.LeftOut[round] = string.Format("{0:x}", left);
                _magmaCompleteDate.RightOut[round] = string.Format("{0:x}", right);

                if (round != 31)
                {
                    (left, right) = (right, left);
                }
            }

            //merge left and right and output
            block = (UInt64)((((UInt64)left) << 32) | right);
            return block;
        }

        /// <summary>
        /// Encrypts the given byte array
        /// </summary>
        /// <param name="block"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public byte[] EncryptBlock(byte[] block, byte[] key)
        {
            //hint: interface needs a key, but we key the cipher already in its constructor
            //thus, we don't do anything with the key here

            // byte array --> UInt64
            UInt64 plaintext = (UInt64)           
            (   ((UInt64)block[0]) << 56 |
                ((UInt64)block[1]) << 48 |
                ((UInt64)block[2]) << 40 |
                ((UInt64)block[3]) << 32 |
                ((UInt64)block[4]) << 24 |
                ((UInt64)block[5]) << 16 |
                ((UInt64)block[6]) << 8  |
                ((UInt64)block[7]) << 0
            );

            UInt64 ciphertext = EncryptBlock(plaintext);

            // UInt64 --> byte array
            byte[] bytes = new byte[]
            {
                (byte)((ciphertext >> 56) & 0xFF),
                (byte)((ciphertext >> 48) & 0xFF),
                (byte)((ciphertext >> 40) & 0xFF),
                (byte)((ciphertext >> 32) & 0xFF),
                (byte)((ciphertext >> 24) & 0xFF),
                (byte)((ciphertext >> 16) & 0xFF),
                (byte)((ciphertext >>  8) & 0xFF), 
                (byte)((ciphertext >>  0) & 0xFF)
            };
            return bytes;
        }

        /// <summary>
        /// Decrypts a 64bit block
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public UInt64 DecryptBlock(UInt64 block)
        {
            //Feistel cipher: so split into left and right part of block
            UInt32 left = (UInt32)(block >> 32);
            UInt32 right = (UInt32)(block & 0xFFFFFFFF);

            //round 32 to 1
            for (int round = 31; round >= 0; round--)
            {
                uint roundkey = _roundkeys[round < 24 ? round % 8 : 31 - round];                

                if (round != 31)
                {
                    (left, right) = (right, left);
                }

                //store intermedia input left, input right, and round key values
                _magmaCompleteDate.LeftIn[31 - round] = string.Format("{0:x}", left);
                _magmaCompleteDate.RightIn[31 - round] = string.Format("{0:x}", right);
                _magmaCompleteDate.RoundKey[31 - round] = string.Format("K{0}={1:x}", round < 24 ? round % 8 : 31 - round, roundkey);

                left ^= RoundFunction(right, roundkey);

                //store intermedia output left and output right values
                _magmaCompleteDate.LeftOut[31 - round] = string.Format("{0:x}", left);
                _magmaCompleteDate.RightOut[31 - round] = string.Format("{0:x}", right);
            }

            //merge left and right and output
            block = (UInt64)((((UInt64)left) << 32) | right);
            return block;
        }

        /// <summary>
        /// Decrypts the given byte array
        /// </summary>
        /// <param name="block"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public byte[] DecryptBlock(byte[] block, byte[] key)
        {
            //hint: interface needs a key, but we key the cipher already in its constructor
            //thus, we don't do anything with the key here

            // byte array --> UInt64
            UInt64 plaintext = (UInt64)
            (
                ((UInt64)block[0]) << 56 |
                ((UInt64)block[1]) << 48 |
                ((UInt64)block[2]) << 40 |
                ((UInt64)block[3]) << 32 |
                ((UInt64)block[4]) << 24 |
                ((UInt64)block[5]) << 16 |
                ((UInt64)block[6]) << 8 |
                ((UInt64)block[7]) << 0
            );

            UInt64 ciphertext = DecryptBlock(plaintext);

            // UInt64 --> byte array
            byte[] bytes = new byte[]
            {
                (byte)((ciphertext >> 56) & 0xFF),
                (byte)((ciphertext >> 48) & 0xFF),
                (byte)((ciphertext >> 40) & 0xFF),
                (byte)((ciphertext >> 32) & 0xFF),
                (byte)((ciphertext >> 24) & 0xFF),
                (byte)((ciphertext >> 16) & 0xFF),
                (byte)((ciphertext >>  8) & 0xFF),
                (byte)((ciphertext >>  0) & 0xFF)
            };
            return bytes;
        }

        /// <summary>
        /// Round function of Magma:
        /// - 1) 16bit add value with round key
        /// - 2) Put through 8 4bit S-boxes (we use 4 8bit S-boxes instead)
        /// - 3) Left rotate 11 bits
        /// </summary>
        /// <param name="value"></param>
        /// <param name="roundkey"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UInt32 RoundFunction(UInt32 value, UInt32 roundkey)
        {
            return LeftRotate(SBox8(value + roundkey));
        }

        /// <summary>
        /// Left rotate 11 bits
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UInt32 LeftRotate(UInt32 value)
        {
            return ((value << 11) | (value >> (32 - 11)));
        }        

        /// <summary>
        /// S-Box lookup (all four 8bit S-boxes)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private UInt32 SBox8(UInt32 value)
        {
            value = (UInt32)
            (
                _sboxes[3, (value >> 24) & 0xFF] << 24 |
                _sboxes[2, (value >> 16) & 0xFF] << 16 |
                _sboxes[1, (value >> 8) & 0xFF] << 8 |
                _sboxes[0, (value >> 0) & 0xFF] << 0
            );
            return value;
        }

        /// <summary>
        /// Generates 8bit S-boxes out of the given 4bit S-boxes
        /// </summary>
        /// <param name="sboxes4bit"></param>
        /// <returns></returns>
        private byte[,] Generate8BitSboxes(byte[,] sboxes4bit)
        {
            byte[,] sboxes8bit = new byte[4, 256];
            for (byte i = 0; i < 4; i++)
            {
                for (byte a = 0; a < 16; a++)
                {
                    for (byte b = 0; b < 16; b++)
                    {
                        sboxes8bit[i, a * 16 + b] = (byte)
                        (
                            sboxes4bit[i * 2 + 1, a] << 4 |
                            sboxes4bit[i * 2 + 0, b] << 0
                        );
                    }
                }
            }
            return sboxes8bit;
        }

        /// <summary>
        /// Returns the complete intermediate stored data of the last encryption or decryption
        /// performed with Magma. This is needed for the presentation of the CT2 component
        /// </summary>
        public MagmaCompleteData MagmaCompleteData
        {
            get => _magmaCompleteDate;
        }
    }

    /// <summary>
    /// Class to store all intermediate values of a round of Magma
    /// </summary>
    public class MagmaCompleteData
    {
        /// <summary>
        /// All left input halves of all 32 rounds
        /// </summary>
        public string[] LeftIn = new string[32];

        /// <summary>
        /// All right input halves of all 32 rounds
        /// </summary>
        public string[] RightIn = new string[32];

        /// <summary>
        /// All left output halves of all 32 rounds
        /// </summary>
        public string[] LeftOut = new string[32];

        /// <summary>
        /// All right output halves of all 32 rounds
        /// </summary>
        public string[] RightOut = new string[32];

        /// <summary>
        /// All used round keys of all 32 rounds
        /// </summary>
        public string[] RoundKey = new string[32];       
    }
}
