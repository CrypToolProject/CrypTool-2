/*                              
   Copyright 2024 Nils Kopal, CrypTool Team

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
using System.Linq;

namespace CrypTool.Plugins.SM4
{
    public class SM4Cipher
    {
        /// <summary>
        /// The Sbox
        /// </summary>
        private readonly static byte[] Sbox = new byte[]
        {
            0xd6 ,0x90 ,0xe9 ,0xfe ,0xcc ,0xe1 ,0x3d ,0xb7 ,0x16 ,0xb6 ,0x14 ,0xc2 ,0x28 ,0xfb ,0x2c ,0x05,
            0x2b ,0x67 ,0x9a ,0x76 ,0x2a ,0xbe ,0x04 ,0xc3 ,0xaa ,0x44 ,0x13 ,0x26 ,0x49 ,0x86 ,0x06 ,0x99,
            0x9c ,0x42 ,0x50 ,0xf4 ,0x91 ,0xef ,0x98 ,0x7a ,0x33 ,0x54 ,0x0b ,0x43 ,0xed ,0xcf ,0xac ,0x62,
            0xe4 ,0xb3 ,0x1c ,0xa9 ,0xc9 ,0x08 ,0xe8 ,0x95 ,0x80 ,0xdf ,0x94 ,0xfa ,0x75 ,0x8f ,0x3f ,0xa6,
            0x47 ,0x07 ,0xa7 ,0xfc ,0xf3 ,0x73 ,0x17 ,0xba ,0x83 ,0x59 ,0x3c ,0x19 ,0xe6 ,0x85 ,0x4f ,0xa8,
            0x68 ,0x6b ,0x81 ,0xb2 ,0x71 ,0x64 ,0xda ,0x8b ,0xf8 ,0xeb ,0x0f ,0x4b ,0x70 ,0x56 ,0x9d ,0x35,
            0x1e ,0x24 ,0x0e ,0x5e ,0x63 ,0x58 ,0xd1 ,0xa2 ,0x25 ,0x22 ,0x7c ,0x3b ,0x01 ,0x21 ,0x78 ,0x87,
            0xd4 ,0x00 ,0x46 ,0x57 ,0x9f ,0xd3 ,0x27 ,0x52 ,0x4c ,0x36 ,0x02 ,0xe7 ,0xa0 ,0xc4 ,0xc8 ,0x9e,
            0xea ,0xbf ,0x8a ,0xd2 ,0x40 ,0xc7 ,0x38 ,0xb5 ,0xa3 ,0xf7 ,0xf2 ,0xce ,0xf9 ,0x61 ,0x15 ,0xa1,
            0xe0 ,0xae ,0x5d ,0xa4 ,0x9b ,0x34 ,0x1a ,0x55 ,0xad ,0x93 ,0x32 ,0x30 ,0xf5 ,0x8c ,0xb1 ,0xe3,
            0x1d ,0xf6 ,0xe2 ,0x2e ,0x82 ,0x66 ,0xca ,0x60 ,0xc0 ,0x29 ,0x23 ,0xab ,0x0d ,0x53 ,0x4e ,0x6f,
            0xd5 ,0xdb ,0x37 ,0x45 ,0xde ,0xfd ,0x8e ,0x2f ,0x03 ,0xff ,0x6a ,0x72 ,0x6d ,0x6c ,0x5b ,0x51,
            0x8d ,0x1b ,0xaf ,0x92 ,0xbb ,0xdd ,0xbc ,0x7f ,0x11 ,0xd9 ,0x5c ,0x41 ,0x1f ,0x10 ,0x5a ,0xd8,
            0x0a ,0xc1 ,0x31 ,0x88 ,0xa5 ,0xcd ,0x7b ,0xbd ,0x2d ,0x74 ,0xd0 ,0x12 ,0xb8 ,0xe5 ,0xb4 ,0xb0,
            0x89 ,0x69 ,0x97 ,0x4a ,0x0c ,0x96 ,0x77 ,0x7e ,0x65 ,0xb9 ,0xf1 ,0x09 ,0xc5 ,0x6e ,0xc6 ,0x84,
            0x18 ,0xf0 ,0x7d ,0xec ,0x3a ,0xdc ,0x4d ,0x20 ,0x79 ,0xee ,0x5f ,0x3e ,0xd7 ,0xcb ,0x39 ,0x48,
        };

        /// <summary>
        /// The system parameter FK as byte arrays
        /// </summary>
        private readonly static byte[][] FK = new byte[][]
        {
            new byte[]{0xa3,0xb1, 0xba, 0xc6},
            new byte[]{0x56,0xaa, 0x33, 0x50},
            new byte[]{0x67,0x7d, 0x91, 0x97},
            new byte[]{0xb2,0x70, 0x22, 0xdc}
        };

        /// <summary>
        /// The constant parameters CK
        /// </summary>
        private readonly static byte[][] CK = new byte[][]
        {
            new byte[]{0x00,0x07,0x0e,0x15}, new byte[]{0x1c,0x23,0x2a,0x31}, new byte[]{0x38,0x3f,0x46,0x4d}, new byte[]{0x54,0x5b,0x62,0x69},
            new byte[]{0x70,0x77,0x7e,0x85}, new byte[]{0x8c,0x93,0x9a,0xa1}, new byte[]{0xa8,0xaf,0xb6,0xbd}, new byte[]{0xc4,0xcb,0xd2,0xd9},
            new byte[]{0xe0,0xe7,0xee,0xf5}, new byte[]{0xfc,0x03,0x0a,0x11}, new byte[]{0x18,0x1f,0x26,0x2d}, new byte[]{0x34,0x3b,0x42,0x49},
            new byte[]{0x50,0x57,0x5e,0x65}, new byte[]{0x6c,0x73,0x7a,0x81}, new byte[]{0x88,0x8f,0x96,0x9d}, new byte[]{0xa4,0xab,0xb2,0xb9},
            new byte[]{0xc0,0xc7,0xce,0xd5}, new byte[]{0xdc,0xe3,0xea,0xf1}, new byte[]{0xf8,0xff,0x06,0x0d}, new byte[]{0x14,0x1b,0x22,0x29},
            new byte[]{0x30,0x37,0x3e,0x45}, new byte[]{0x4c,0x53,0x5a,0x61}, new byte[]{0x68,0x6f,0x76,0x7d}, new byte[]{0x84,0x8b,0x92,0x99},
            new byte[]{0xa0,0xa7,0xae,0xb5}, new byte[]{0xbc,0xc3,0xca,0xd1}, new byte[]{0xd8,0xdf,0xe6,0xed}, new byte[]{0xf4,0xfb,0x02,0x09},
            new byte[]{0x10,0x17,0x1e,0x25}, new byte[]{0x2c,0x33,0x3a,0x41}, new byte[]{0x48,0x4f,0x56,0x5d}, new byte[]{0x64,0x6b,0x72,0x79}
        };

        /// <summary>
        /// The round function F
        /// </summary>
        /// <param name="X0"></param>
        /// <param name="X1"></param>
        /// <param name="X2"></param>
        /// <param name="X3"></param>
        /// <returns></returns>
        private static byte[] F(byte[] X0, byte[] X1, byte[] X2, byte[] X3, byte[] rk)
        {
            return X0.Xor(T(X1.Xor(X2).Xor(X3).Xor(rk)));
        }

        /// <summary>
        /// Mixer-substitution T
        /// </summary>
        /// <param name="X"></param>
        /// <returns></returns>
        private static byte[] T(byte[] X)
        {
            return L(Tau(X));
        }

        /// <summary>
        /// The Tdash function needed for key expansion
        /// </summary>
        /// <param name="B"></param>
        /// <returns></returns>
        private static byte[] TDash(byte[] B)
        {
            return LDash(Tau(B));
        }

        /// <summary>
        /// Non-linear substitution Tau
        /// </summary>
        /// <param name="X"></param>
        /// <returns></returns>
        private static byte[] Tau(byte[] X)
        {
            byte[] result = new byte[X.Length];
            for (int i = 0; i < X.Length; i++)
            {
                result[i] = Sbox[X[i]];
            }
            return result;
        }

        /// <summary>
        /// The linear transformation L using XOR and circular shifts method
        /// </summary>
        /// <param name="B"></param>
        /// <returns></returns>
        private static byte[] L(byte[] B)
        {
            return B.Xor((B.CircularLeftShift(2)).Xor(B.CircularLeftShift(10)).Xor(B.CircularLeftShift(18)).Xor((B.CircularLeftShift(24))));
        }

        /// <summary>
        /// The Ldash function needed for key expansion
        /// </summary>
        /// <param name="B"></param>
        /// <returns></returns>
        private static byte[] LDash(byte[] B)
        {
            return B.Xor((B.CircularLeftShift(13)).Xor(B.CircularLeftShift(23)));
        }

        /// <summary>
        /// The key expansion function of SM4
        /// </summary>
        /// <param name="MK"></param>
        /// <returns></returns>
        private static byte[][] KeyExpansion(byte[][] MK)
        {
            byte[][] rk = new byte[32][];
            byte[][] K = new byte[36][];

            K[0] = MK[0].Xor(FK[0]);
            K[1] = MK[1].Xor(FK[1]);
            K[2] = MK[2].Xor(FK[2]);
            K[3] = MK[3].Xor(FK[3]);

            for (int i = 0; i < 32; i++)
            {
                rk[i] = K[i + 4] = K[i].Xor(TDash(K[i + 1].Xor(K[i + 2]).Xor(K[i + 3]).Xor(CK[i])));
            }

            return rk;
        }

        /// <summary>
        /// Reverse substitution R
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public static (byte[], byte[], byte[], byte[]) R(byte[] A0, byte[] A1, byte[] A2, byte[] A3)
        {
            return (A3, A2, A1, A0);
        }

        /// <summary>
        /// The encryption function
        /// </summary>
        /// <param name="X0"></param>
        /// <param name="X1"></param>
        /// <param name="X2"></param>
        /// <param name="X3"></param>
        /// <param name="rk"></param>
        /// <returns></returns>
        private static (byte[], byte[], byte[], byte[]) EncryptBlock(byte[] X0, byte[] X1, byte[] X2, byte[] X3, byte[][] rk)
        {
            byte[] tmp;
            for (int i = 0; i < 32; i++)
            {
                tmp = F(X0, X1, X2, X3, rk[i]);
                X0 = X1;
                X1 = X2;
                X2 = X3;
                X3 = tmp;
            }
            return R(X0, X1, X2, X3);
        }

        /// <summary>
        /// Encrypts a block of 16 bytes using the given key
        /// </summary>
        /// <param name="block"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] EncryptBlock(byte[] block, byte[] key)
        {
            //generate round keys            
            byte[][] rk = KeyExpansion(key.ToWords());

            //convert block array to 4 words
            byte[][] words = block.ToWords();

            (byte[], byte[], byte[], byte[]) result = EncryptBlock(words[0], words[1], words[2], words[3], rk);

            //convert 4 words result to byte[] array and return it
            return result.Item1.Concat(result.Item2).Concat(result.Item3).Concat(result.Item4).ToArray();
        }

        /// <summary>
        /// The decryption function
        /// </summary>
        /// <param name="X0"></param>
        /// <param name="X1"></param>
        /// <param name="X2"></param>
        /// <param name="X3"></param>
        /// <param name="rk"></param>
        /// <returns></returns>
        private static (byte[], byte[], byte[], byte[]) DecryptBlock(byte[] X0, byte[] X1, byte[] X2, byte[] X3, byte[][] rk)
        {
            byte[] tmp;
            for (int i = 0; i < 32; i++)
            {
                tmp = F(X0, X1, X2, X3, rk[31 - i]);
                X0 = X1;
                X1 = X2;
                X2 = X3;
                X3 = tmp;
            }
            return R(X0, X1, X2, X3);
        }

        /// <summary>
        /// Decrypts a block of 16 bytes using the given key
        /// </summary>
        /// <param name="block"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] DecryptBlock(byte[] block, byte[] key)
        {
            //generate round keys            
            byte[][] rk = KeyExpansion(key.ToWords());

            //convert block array to 4 words
            byte[][] words = block.ToWords();

            (byte[], byte[], byte[], byte[]) result = DecryptBlock(words[0], words[1], words[2], words[3], rk);

            //convert 4 words result to byte[] array and return it
            return result.Item1.Concat(result.Item2).Concat(result.Item3).Concat(result.Item4).ToArray();
        }
    }

    /// <summary>
    /// Extension methods for byte arrays
    /// </summary>
    internal static class ByteArrayExtensions
    {
        /// <summary>
        /// Xor function for byte arrays
        /// </summary>
        /// <param name="arrayA"></param>
        /// <param name="arrayB"></param>
        /// <returns></returns>
        public static unsafe byte[] Xor(this byte[] arrayA, byte[] arrayB)
        {
            byte[] result = new byte[arrayA.Length];
            fixed (byte* a = arrayA)
            fixed (byte* b = arrayB)
            fixed (byte* r = result)
            {
                int* pA = (int*)a;
                int* pB = (int*)b;
                int* pR = (int*)r;

                *pR = *pA ^ *pB;
            }
            return result;
        }

        /// <summary>
        /// Circular shifts the bits of a byte array by the given shift value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="shift"></param>
        /// <returns></returns>
        public static byte[] CircularLeftShift(this byte[] array, int bits)
        {
            byte[] result = new byte[array.Length];

            if (array.Length > 0)
            {
                int nByteShift = bits / (sizeof(byte) * 8);
                int nBitShift = bits % (sizeof(byte) * 8);

                if (nByteShift >= array.Length)
                {
                    nByteShift %= array.Length;
                }

                int length = array.Length;
                int s = length - 1;
                int d = s - nByteShift;

                for (int nCnt = 0; nCnt < array.Length; nCnt++, d--, s--)
                {
                    while (d < 0)
                    {
                        d += length;
                    }
                    while (s < 0)
                    {
                        s += length;
                    }

                    byte byteS = array[s];

                    result[d] |= (byte)(byteS << nBitShift);
                    result[d > 0 ? d - 1 : length - 1] |= (byte)(byteS >> (sizeof(byte) * 8 - nBitShift));
                }
            }

            return result;
        }

        /// <summary>
        /// Converts a byte array to four words (4 bytes each)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static unsafe byte[][] ToWords(this byte[] value)
        {
            byte[][] result = new byte[value.Length / 4][];
            fixed (byte* p = value)
            {
                int* pValue = (int*)p;

                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = new byte[4];
                    fixed (byte* r = result[i])
                    {
                        int* pResult = (int*)r;
                        *pResult = *pValue;
                    }
                    pValue++;
                }
            }
            return result;
        }
    }
}