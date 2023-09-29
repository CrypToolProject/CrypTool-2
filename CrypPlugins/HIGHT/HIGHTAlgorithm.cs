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

using System;
using System.Runtime.CompilerServices;

namespace CrypTool.Plugins.HIGHT
{
    /// <summary>
    /// Implementation of the HIGHT encryption algorithm as specified in:
    /// "The HIGHT Encryption Algorithm"
    /// "draft-kisa-hight-00"
    /// see: https://datatracker.ietf.org/doc/html/draft-kisa-hight-00
    /// </summary>
    public class HIGHTAlgorithm
    {
        /// <summary>
        /// Pre-calculated table for function F0
        /// </summary>
        private readonly byte[] _F0 = new byte[256]
        {
            0x00, 0x86, 0x0D, 0x8B, 0x1A, 0x9C, 0x17, 0x91, 0x34, 0xB2, 0x39, 0xBF, 0x2E, 0xA8, 0x23, 0xA5,
            0x68, 0xEE, 0x65, 0xE3, 0x72, 0xF4, 0x7F, 0xF9, 0x5C, 0xDA, 0x51, 0xD7, 0x46, 0xC0, 0x4B, 0xCD,
            0xD0, 0x56, 0xDD, 0x5B, 0xCA, 0x4C, 0xC7, 0x41, 0xE4, 0x62, 0xE9, 0x6F, 0xFE, 0x78, 0xF3, 0x75,
            0xB8, 0x3E, 0xB5, 0x33, 0xA2, 0x24, 0xAF, 0x29, 0x8C, 0x0A, 0x81, 0x07, 0x96, 0x10, 0x9B, 0x1D,
            0xA1, 0x27, 0xAC, 0x2A, 0xBB, 0x3D, 0xB6, 0x30, 0x95, 0x13, 0x98, 0x1E, 0x8F, 0x09, 0x82, 0x04,
            0xC9, 0x4F, 0xC4, 0x42, 0xD3, 0x55, 0xDE, 0x58, 0xFD, 0x7B, 0xF0, 0x76, 0xE7, 0x61, 0xEA, 0x6C,
            0x71, 0xF7, 0x7C, 0xFA, 0x6B, 0xED, 0x66, 0xE0, 0x45, 0xC3, 0x48, 0xCE, 0x5F, 0xD9, 0x52, 0xD4,
            0x19, 0x9F, 0x14, 0x92, 0x03, 0x85, 0x0E, 0x88, 0x2D, 0xAB, 0x20, 0xA6, 0x37, 0xB1, 0x3A, 0xBC,
            0x43, 0xC5, 0x4E, 0xC8, 0x59, 0xDF, 0x54, 0xD2, 0x77, 0xF1, 0x7A, 0xFC, 0x6D, 0xEB, 0x60, 0xE6,
            0x2B, 0xAD, 0x26, 0xA0, 0x31, 0xB7, 0x3C, 0xBA, 0x1F, 0x99, 0x12, 0x94, 0x05, 0x83, 0x08, 0x8E,
            0x93, 0x15, 0x9E, 0x18, 0x89, 0x0F, 0x84, 0x02, 0xA7, 0x21, 0xAA, 0x2C, 0xBD, 0x3B, 0xB0, 0x36,
            0xFB, 0x7D, 0xF6, 0x70, 0xE1, 0x67, 0xEC, 0x6A, 0xCF, 0x49, 0xC2, 0x44, 0xD5, 0x53, 0xD8, 0x5E,
            0xE2, 0x64, 0xEF, 0x69, 0xF8, 0x7E, 0xF5, 0x73, 0xD6, 0x50, 0xDB, 0x5D, 0xCC, 0x4A, 0xC1, 0x47,
            0x8A, 0x0C, 0x87, 0x01, 0x90, 0x16, 0x9D, 0x1B, 0xBE, 0x38, 0xB3, 0x35, 0xA4, 0x22, 0xA9, 0x2F,
            0x32, 0xB4, 0x3F, 0xB9, 0x28, 0xAE, 0x25, 0xA3, 0x06, 0x80, 0x0B, 0x8D, 0x1C, 0x9A, 0x11, 0x97,
            0x5A, 0xDC, 0x57, 0xD1, 0x40, 0xC6, 0x4D, 0xCB, 0x6E, 0xE8, 0x63, 0xE5, 0x74, 0xF2, 0x79, 0xFF
        };

        /// <summary>
        /// Pre-calculated table for function F1
        /// </summary>
        public readonly byte[] _F1 = new byte[256]
        {
            0x00, 0x58, 0xB0, 0xE8, 0x61, 0x39, 0xD1, 0x89, 0xC2, 0x9A, 0x72, 0x2A, 0xA3, 0xFB, 0x13, 0x4B,
            0x85, 0xDD, 0x35, 0x6D, 0xE4, 0xBC, 0x54, 0x0C, 0x47, 0x1F, 0xF7, 0xAF, 0x26, 0x7E, 0x96, 0xCE,
            0x0B, 0x53, 0xBB, 0xE3, 0x6A, 0x32, 0xDA, 0x82, 0xC9, 0x91, 0x79, 0x21, 0xA8, 0xF0, 0x18, 0x40,
            0x8E, 0xD6, 0x3E, 0x66, 0xEF, 0xB7, 0x5F, 0x07, 0x4C, 0x14, 0xFC, 0xA4, 0x2D, 0x75, 0x9D, 0xC5,
            0x16, 0x4E, 0xA6, 0xFE, 0x77, 0x2F, 0xC7, 0x9F, 0xD4, 0x8C, 0x64, 0x3C, 0xB5, 0xED, 0x05, 0x5D,
            0x93, 0xCB, 0x23, 0x7B, 0xF2, 0xAA, 0x42, 0x1A, 0x51, 0x09, 0xE1, 0xB9, 0x30, 0x68, 0x80, 0xD8,
            0x1D, 0x45, 0xAD, 0xF5, 0x7C, 0x24, 0xCC, 0x94, 0xDF, 0x87, 0x6F, 0x37, 0xBE, 0xE6, 0x0E, 0x56,
            0x98, 0xC0, 0x28, 0x70, 0xF9, 0xA1, 0x49, 0x11, 0x5A, 0x02, 0xEA, 0xB2, 0x3B, 0x63, 0x8B, 0xD3,
            0x2C, 0x74, 0x9C, 0xC4, 0x4D, 0x15, 0xFD, 0xA5, 0xEE, 0xB6, 0x5E, 0x06, 0x8F, 0xD7, 0x3F, 0x67,
            0xA9, 0xF1, 0x19, 0x41, 0xC8, 0x90, 0x78, 0x20, 0x6B, 0x33, 0xDB, 0x83, 0x0A, 0x52, 0xBA, 0xE2,
            0x27, 0x7F, 0x97, 0xCF, 0x46, 0x1E, 0xF6, 0xAE, 0xE5, 0xBD, 0x55, 0x0D, 0x84, 0xDC, 0x34, 0x6C,
            0xA2, 0xFA, 0x12, 0x4A, 0xC3, 0x9B, 0x73, 0x2B, 0x60, 0x38, 0xD0, 0x88, 0x01, 0x59, 0xB1, 0xE9,
            0x3A, 0x62, 0x8A, 0xD2, 0x5B, 0x03, 0xEB, 0xB3, 0xF8, 0xA0, 0x48, 0x10, 0x99, 0xC1, 0x29, 0x71,
            0xBF, 0xE7, 0x0F, 0x57, 0xDE, 0x86, 0x6E, 0x36, 0x7D, 0x25, 0xCD, 0x95, 0x1C, 0x44, 0xAC, 0xF4,
            0x31, 0x69, 0x81, 0xD9, 0x50, 0x08, 0xE0, 0xB8, 0xF3, 0xAB, 0x43, 0x1B, 0x92, 0xCA, 0x22, 0x7A,
            0xB4, 0xEC, 0x04, 0x5C, 0xD5, 0x8D, 0x65, 0x3D, 0x76, 0x2E, 0xC6, 0x9E, 0x17, 0x4F, 0xA7, 0xFF
        };

        /// <summary>
        /// Whitening keys
        /// </summary>
        private readonly byte[] _WK = new byte[8];

        /// <summary>
        /// Round keys
        /// </summary>
        private readonly byte[] _SK = new byte[128];

        public HIGHTAlgorithm()
        {
        }

        /// <summary>
        /// Creates a HIGHT algorithm using the given key
        /// </summary>
        /// <param name="key"></param>
        public HIGHTAlgorithm(byte[] key)
        {
            KeySchedule(key);
        }

        /// <summary>
        /// Encrypts a given block
        /// </summary>
        /// <param name="P"></param>
        /// <param name="unused"></param>
        /// <returns></returns>
        public byte[] EncryptBlock(byte[] P, byte[] unused)
        {
            //states in encryption
            byte[] X = new byte[8];
            byte[] X_old = new byte[8]; //state of previous round

            //whitening
            X[0] = (byte)(Mod(P[7] + _WK[0], 256));
            X[1] = P[6];
            X[2] = (byte)(P[5] ^ _WK[1]);
            X[3] = P[4];
            X[4] = (byte)(Mod(P[3] + _WK[2], 256));
            X[5] = P[2];
            X[6] = (byte)(P[1] ^ _WK[3]);            
            X[7] = P[0];

            for (int i = 0; i < 31; i++)
            {
#if DEBUG
                Console.WriteLine("{8}: {7:x} {6:x} {5:x} {4:x} {3:x} {2:x} {1:x} {0:x}", X[0], X[1], X[2], X[3], X[4], X[5], X[6], X[7], i);
#endif
                Array.Copy(X, X_old, 8);
                X[0] = (byte)(X_old[7] ^ Mod(_F0[X_old[6]] + _SK[4 * i + 3], 256));
                X[1] = X_old[0];
                X[2] = (byte)Mod(X_old[1] + (_F1[X_old[0]] ^ _SK[4 * i]), 256);
                X[3] = X_old[2];
                X[4] = (byte)(X_old[3] ^ Mod(_F0[X_old[2]] + _SK[4 * i + 1], 256));
                X[5] = X_old[4];
                X[6] = (byte)Mod(X_old[5] + (_F1[X_old[4]] ^ _SK[4 * i + 2]), 256);
                X[7] = X_old[6];
            }
#if DEBUG
            Console.WriteLine("31: {7:x} {6:x} {5:x} {4:x} {3:x} {2:x} {1:x} {0:x}", X[0], X[1], X[2], X[3], X[4], X[5], X[6], X[7]);
#endif
            Array.Copy(X, X_old, 8);
            X[0] = X_old[0];
            X[1] = (byte)Mod(X_old[1] + (_F1[X_old[0]] ^ _SK[124]), 256);
            X[2] = X_old[2];
            X[3] = (byte)(X_old[3] ^ Mod(_F0[X_old[2]] + _SK[125], 256));
            X[4] = X_old[4];
            X[5] = (byte)Mod(X_old[5] + (_F1[X_old[4]] ^ _SK[126]), 256);
            X[6] = X_old[6];
            X[7] = (byte)(X_old[7] ^ Mod(_F0[X_old[6]] + _SK[127], 256));
#if DEBUG
            Console.WriteLine("32: {7:x} {6:x} {5:x} {4:x} {3:x} {2:x} {1:x} {0:x}", X[0], X[1], X[2], X[3], X[4], X[5], X[6], X[7]);
#endif
            //whitening
            byte[] C = new byte[8];
            C[0] = X[7];
            C[1] = (byte)(X[6] ^ _WK[7]);
            C[2] = X[5];
            C[3] = (byte)Mod(X[4] + _WK[6], 256);
            C[4] = X[3];
            C[5] = (byte)(X[2] ^ _WK[5]);
            C[6] = X[1];
            C[7] = (byte)Mod(X[0] + _WK[4], 256);            
            return C;
        }

        /// <summary>
        /// Decrypts a given block
        /// </summary>
        /// <param name="C"></param>
        /// <param name="unused"></param>
        /// <returns></returns>
        public byte[] DecryptBlock(byte[] C, byte[] unused)
        {
            //states in encryption
            byte[] X = new byte[8];
            byte[] X_old = new byte[8]; //state of previous round

            //whitening
            X[0] = (byte)(Mod(C[7] - _WK[4], 256));
            X[1] = C[6];
            X[2] = (byte)(C[5] ^ _WK[5]);
            X[3] = C[4];
            X[4] = (byte)(Mod(C[3] - _WK[6], 256));
            X[5] = C[2];
            X[6] = (byte)(C[1] ^ _WK[7]);            
            X[7] = C[0];
#if DEBUG
            Console.WriteLine("32: {7:x} {6:x} {5:x} {4:x} {3:x} {2:x} {1:x} {0:x}", X[0], X[1], X[2], X[3], X[4], X[5], X[6], X[7]);
#endif
            Array.Copy(X, X_old, 8);
            X[0] = X_old[0];
            X[1] = (byte)Mod(X_old[1] - (_F1[X_old[0]] ^ _SK[124]), 256);
            X[2] = X_old[2];
            X[3] = (byte)(X_old[3] ^ Mod(_F0[X_old[2]] + _SK[125], 256));
            X[4] = X_old[4];
            X[5] = (byte)Mod(X_old[5] - (_F1[X_old[4]] ^ _SK[126]), 256);
            X[6] = X_old[6];
            X[7] = (byte)(X_old[7] ^ Mod(_F0[X_old[6]] + _SK[127], 256));
#if DEBUG
            Console.WriteLine("31: {7:x} {6:x} {5:x} {4:x} {3:x} {2:x} {1:x} {0:x}", X[0], X[1], X[2], X[3], X[4], X[5], X[6], X[7]);
#endif
            for (int i = 30; i >= 0; i--)
            {               
                Array.Copy(X, X_old, 8);
                X[0] = X_old[1];
                X[1] = (byte)Mod(X_old[2] - (_F1[X_old[1]] ^ _SK[(4 * i + 0)]), 256);
                X[2] = X_old[3];
                X[3] = (byte)(X_old[4] ^ Mod(_F0[X_old[3]] + _SK[(4 * i + 1)], 256));
                X[4] = X_old[5];
                X[5] = (byte)Mod(X_old[6] - (_F1[X_old[5]] ^ _SK[(4 * i + 2)]), 256);
                X[6] = X_old[7];
                X[7] = (byte)(X_old[0] ^ Mod(_F0[X_old[7]] + _SK[(4 * i + 3)], 256));
#if DEBUG
                Console.WriteLine("{8}: {7:x} {6:x} {5:x} {4:x} {3:x} {2:x} {1:x} {0:x}", X[0], X[1], X[2], X[3], X[4], X[5], X[6], X[7], i);
#endif
            }

            //whitening
            byte[] P = new byte[8];
            P[0] = X[7];
            P[1] = (byte)(X[6] ^ _WK[3]);
            P[2] = X[5];
            P[3] = (byte)Mod(X[4] - _WK[2], 256);
            P[4] = X[3];
            P[5] = (byte)(X[2] ^ _WK[1]);
            P[6] = X[1];
            P[7] = (byte)Mod(X[0] - _WK[0], 256);            
            return P;
        }


        /// <summary>
        /// Performs the key schedule to create whitening keys and round keys
        /// </summary>
        /// <param name="inputKey"></param>
        public void KeySchedule(byte[] inputKey)
        {
            //copy and reverse input key
            byte[] key = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                key[i] = inputKey[15 - i];
            }            

            //whitening key generation
            for (int i = 0; i < 4; i++)
            {
                _WK[i] = key[i + 12];
            }
            for (int i = 4; i <= 7; i++)
            {
                _WK[i] = key[i - 4];
            }

            //constants generation
            byte[] delta = new byte[128];
            delta[0] = 0b1011010;
            for (int i = 1; i < 128; i++)
            {
                delta[i] = LFSR_h(delta[i - 1]);
            }

            //round keys generation
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    _SK[16 * i + j] = (byte)(key[Mod(j - i, 8)] + delta[16 * i + j]);
                }
                for (int j = 0; j < 8; j++)
                {
                    _SK[16 * i + j + 8] = (byte)(key[Mod(j - i, 8) + 8] + delta[16 * i + j + 8]);
                }
            }
        }

        /// <summary>
        /// Implementation of the LFSR "h"
        /// Computes the next state based on the given state
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte LFSR_h(byte state)
        {
            byte bits = (byte)(state & 0b0001001);
            if (bits == 0b0000000 || bits == 0b0001001)
            {
                bits = 0b0000000;
            }
            else
            {
                bits = 0b1000000;
            }
            return (byte)(bits | (state >> 1));
        }

        #region Helper methods

        /// <summary>
        /// Computes a number mod modulus
        /// </summary>
        /// <param name="number"></param>
        /// <param name="modulus"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Mod(int number, int modulus)
        {
            return ((number % modulus) + modulus) % modulus;
        }

        #endregion

#if DEBUG
        /// <summary>
        /// Debug method to print out all generated round keys
        /// </summary>
        public void WriteRoundKeys()
        {
            for (int i = 0; i < 128; i += 4)
            {
                Console.WriteLine("{3:x}{2:x}{1:x}{0:x}", _SK[i + 0], _SK[i + 1], _SK[i + 2], _SK[i + 3]);
            }
        }

#endif
    }
}