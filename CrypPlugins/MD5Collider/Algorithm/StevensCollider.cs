/*
 * This class is based on the code of Marc Stevens'
 * "fast collision generator", version 1.0.0.5,
 * which is used with his permission.
 * 
 * The disclaimer of the original source code is below:
 
Version
=======
version 1.0.0.5, April 2006.

Copyright
=========
© M. Stevens, 2006. All rights reserved.

Disclaimer
==========
This software is provided as is. Use is at the user's risk.
No guarantee whatsoever is given on how it may function or malfunction.
Support cannot be expected.
This software is meant for scientific and educational purposes only.
It is forbidden to use it for other than scientific or educational purposes.
In particular, commercial and malicious use is not allowed.
Further distribution of this software, by whatever means, is not allowed
without our consent.
This includes publication of source code or executables in printed form,
on websites, newsgroups, CD-ROM's, etc.
Changing the (source) code without our consent is not allowed.
In all versions of the source code this disclaimer, the copyright
notice and the version number should be present.

*/

using System;
using System.Security.Cryptography;


namespace CrypTool.Plugins.MD5Collider.Algorithm
{
    internal class StevensCollider : MD5ColliderBase
    {
        protected override void PerformFindCollision()
        {
            IsStopped = false;

            byte[] seedBytesMd5 = MD5.Create().ComputeHash(RandomSeed);
            seed32_1 = (uint)(BitConverter.ToInt32(seedBytesMd5, 0) ^ BitConverter.ToInt32(seedBytesMd5, 4));
            seed32_2 = (uint)(BitConverter.ToInt32(seedBytesMd5, 8) ^ BitConverter.ToInt32(seedBytesMd5, 12));

            uint[] startIV = new uint[4];
            startIV[0] = toLittleEndianInteger(IHV, 0);
            startIV[1] = toLittleEndianInteger(IHV, 4);
            startIV[2] = toLittleEndianInteger(IHV, 8);
            startIV[3] = toLittleEndianInteger(IHV, 12);

            uint[] m1b0 = new uint[16], m1b1 = new uint[16], m2b0 = new uint[16], m2b1 = new uint[16];
            find_collision(startIV, m1b0, m1b1, m2b0, m2b1);

            if (IsStopped)
            {
                return;
            }

            byte[] firstMessage = new byte[128];
            byte[] secondMessage = new byte[128];

            dumpLittleEndianIntegers(m1b0, firstMessage, 0);
            dumpLittleEndianIntegers(m1b1, firstMessage, 64);

            dumpLittleEndianIntegers(m2b0, secondMessage, 0);
            dumpLittleEndianIntegers(m2b1, secondMessage, 64);

            FirstCollidingData = firstMessage;
            SecondCollidingData = secondMessage;
        }

        private void dumpLittleEndianIntegers(uint[] sourceArray, byte[] targetArray, int targetOffset)
        {
            for (int i = 0; i < sourceArray.Length; i++)
            {
                dumpLittleEndianInteger(sourceArray[i], targetArray, targetOffset + i * 4);
            }
        }

        private void dumpLittleEndianInteger(uint integerValue, byte[] targetArray, int targetOffset)
        {
            byte[] result = BitConverter.GetBytes(integerValue);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(result);
            }

            Array.Copy(result, 0, targetArray, targetOffset, 4);
        }

        private uint toLittleEndianInteger(byte[] bytes, int offset)
        {
            byte[] bytesInProperOrder = new byte[4];
            Array.Copy(bytes, offset, bytesInProperOrder, 0, 4);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytesInProperOrder);
            }

            return BitConverter.ToUInt32(bytesInProperOrder, 0);
        }
        private uint toLittleEndianInteger(byte[] bytes)
        {
            return toLittleEndianInteger(bytes, 0);
        }

        private void find_collision(uint[] IV, uint[] msg1block0, uint[] msg1block1, uint[] msg2block0, uint[] msg2block1)
        {
            find_block0(msg1block0, IV);

            if (IsStopped)
            {
                return;
            }

            uint[] IHV = new[] { IV[0], IV[1], IV[2], IV[3] };
            md5_compress(IHV, msg1block0);

            find_block1(msg1block1, IHV);

            if (IsStopped)
            {
                return;
            }

            for (int t = 0; t < 16; ++t)
            {
                msg2block0[t] = msg1block0[t];
                msg2block1[t] = msg1block1[t];
            }

            msg2block0[4] += (uint)1 << 31; msg2block0[11] += 1 << 15; msg2block0[14] += (uint)1 << 31;
            msg2block1[4] += (uint)1 << 31; msg2block1[11] -= 1 << 15; msg2block1[14] += (uint)1 << 31;
        }

        private uint xrng64()
        {
            uint t = seed32_1 ^ (seed32_1 << 10);
            seed32_1 = seed32_2;
            seed32_2 = (seed32_2 ^ (seed32_2 >> 10)) ^ (t ^ (t >> 13));

            return seed32_1;
        }

        private uint seed32_1, seed32_2;

        private readonly int Qoff = 3;

        private delegate uint RoundFunctionDelegate(uint b, uint c, uint d);

        private void MD5_REVERSE_STEP(uint[] block, uint[] Q, int t, uint AC, int RC)
        {
            block[t] = Q[Qoff + t + 1] - Q[Qoff + t];
            block[t] = RR(block[t], RC) - FF(Q[Qoff + t], Q[Qoff + t - 1], Q[Qoff + t - 2]) - Q[Qoff + t - 3] - AC;
        }

        private void find_block0(uint[] block, uint[] IV)
        {
            MatchProgressMax = 28;
            MatchProgress = 0;

            uint[] Q = new uint[68];
            Q[0] = IV[0];
            Q[1] = IV[3];
            Q[2] = IV[2];
            Q[3] = IV[1];

            uint[] q4mask = new uint[1 << 4];
            for (uint k = 0; k < q4mask.Length; ++k)
            {
                q4mask[k] = ((k << 2) ^ (k << 26)) & 0x38000004;
            }

            uint[] q9q10mask = new uint[1 << 3];
            for (uint k = 0; k < q9q10mask.Length; ++k)
            {
                q9q10mask[k] = ((k << 13) ^ (k << 4)) & 0x2060;
            }

            uint[] q9mask = new uint[1 << 16];
            for (uint k = 0; k < q9mask.Length; ++k)
            {
                q9mask[k] = ((k << 1) ^ (k << 2) ^ (k << 5) ^ (k << 7) ^ (k << 8) ^ (k << 10) ^ (k << 11) ^ (k << 13)) & 0x0eb94f16;
            }

            while (true)
            {
                if (IsStopped)
                {
                    return;
                }

                Q[Qoff + 1] = xrng64();
                Q[Qoff + 3] = (xrng64() & 0xfe87bc3f) | 0x017841c0;
                Q[Qoff + 4] = (xrng64() & 0x44000033) | 0x000002c0 | (Q[Qoff + 3] & 0x0287bc00);
                Q[Qoff + 5] = 0x41ffffc8 | (Q[Qoff + 4] & 0x04000033);
                Q[Qoff + 6] = 0xb84b82d6;
                Q[Qoff + 7] = (xrng64() & 0x68000084) | 0x02401b43;
                Q[Qoff + 8] = (xrng64() & 0x2b8f6e04) | 0x005090d3 | (~Q[Qoff + 7] & 0x40000000);
                Q[Qoff + 9] = 0x20040068 | (Q[Qoff + 8] & 0x00020000) | (~Q[Qoff + 8] & 0x40000000);
                Q[Qoff + 10] = (xrng64() & 0x40000000) | 0x1040b089;
                Q[Qoff + 11] = (xrng64() & 0x10408008) | 0x0fbb7f16 | (~Q[Qoff + 10] & 0x40000000);
                Q[Qoff + 12] = (xrng64() & 0x1ed9df7f) | 0x00022080 | (~Q[Qoff + 11] & 0x40200000);
                Q[Qoff + 13] = (xrng64() & 0x5efb4f77) | 0x20049008;
                Q[Qoff + 14] = (xrng64() & 0x1fff5f77) | 0x0000a088 | (~Q[Qoff + 13] & 0x40000000);
                Q[Qoff + 15] = (xrng64() & 0x5efe7ff7) | 0x80008000 | (~Q[Qoff + 14] & 0x00010000);
                Q[Qoff + 16] = (xrng64() & 0x1ffdffff) | 0xa0000000 | (~Q[Qoff + 15] & 0x40020000);

                MD5_REVERSE_STEP(block, Q, 0, 0xd76aa478, 7);
                MD5_REVERSE_STEP(block, Q, 6, 0xa8304613, 17);
                MD5_REVERSE_STEP(block, Q, 7, 0xfd469501, 22);
                MD5_REVERSE_STEP(block, Q, 11, 0x895cd7be, 22);
                MD5_REVERSE_STEP(block, Q, 14, 0xa679438e, 17);
                MD5_REVERSE_STEP(block, Q, 15, 0x49b40821, 22);

                uint tt1 = FF(Q[Qoff + 1], Q[Qoff + 0], Q[Qoff - 1]) + Q[Qoff - 2] + 0xe8c7b756;
                uint tt17 = GG(Q[Qoff + 16], Q[Qoff + 15], Q[Qoff + 14]) + Q[Qoff + 13] + 0xf61e2562;
                uint tt18 = Q[Qoff + 14] + 0xc040b340 + block[6];
                uint tt19 = Q[Qoff + 15] + 0x265e5a51 + block[11];
                uint tt20 = Q[Qoff + 16] + 0xe9b6c7aa + block[0];
                uint tt5 = RR(Q[Qoff + 6] - Q[Qoff + 5], 12) - FF(Q[Qoff + 5], Q[Qoff + 4], Q[Qoff + 3]) - 0x4787c62a;

                // change q17 until conditions are met on q18, q19 and q20
                uint counter = 0;
                while (counter < (1 << 7))
                {
                    if (IsStopped)
                    {
                        return;
                    }

                    uint q16 = Q[Qoff + 16];
                    uint q17 = ((xrng64() & 0x3ffd7ff7) | (q16 & 0xc0008008)) ^ 0x40000000;
                    ++counter;

                    uint q18 = GG(q17, q16, Q[Qoff + 15]) + tt18;
                    q18 = RL(q18, 9); q18 += q17;
                    if (0x00020000 != ((q18 ^ q17) & 0xa0020000))
                    {
                        LogReturn(1);
                        continue;
                    }

                    uint q19 = GG(q18, q17, q16) + tt19;
                    q19 = RL(q19, 14); q19 += q18;
                    if (0x80000000 != (q19 & 0x80020000))
                    {
                        LogReturn(2);
                        continue;
                    }

                    uint q20 = GG(q19, q18, q17) + tt20;
                    q20 = RL(q20, 20); q20 += q19;
                    if (0x00040000 != ((q20 ^ q19) & 0x80040000))
                    {
                        LogReturn(3);
                        continue;
                    }

                    block[1] = q17 - q16; block[1] = RR(block[1], 5); block[1] -= tt17;
                    uint q2 = block[1] + tt1; q2 = RL(q2, 12); q2 += Q[Qoff + 1];
                    block[5] = tt5 - q2;

                    Q[Qoff + 2] = q2;
                    Q[Qoff + 17] = q17;
                    Q[Qoff + 18] = q18;
                    Q[Qoff + 19] = q19;
                    Q[Qoff + 20] = q20;
                    MD5_REVERSE_STEP(block, Q, 2, 0x242070db, 17);

                    counter = 0;
                    break;
                }
                if (counter != 0)
                {
                    LogReturn(4);
                    continue;
                }

                uint q4 = Q[Qoff + 4];
                uint q9backup = Q[Qoff + 9];
                uint tt21 = GG(Q[Qoff + 20], Q[Qoff + 19], Q[Qoff + 18]) + Q[Qoff + 17] + 0xd62f105d;

                // iterate over possible changes of q4 
                // while keeping all conditions on q1-q20 intact
                // this changes m3, m4, m5 and m7
                uint counter2 = 0;
                while (counter2 < (1 << 4))
                {
                    if (IsStopped)
                    {
                        return;
                    }

                    Q[Qoff + 4] = q4 ^ q4mask[counter2];
                    ++counter2;
                    MD5_REVERSE_STEP(block, Q, 5, 0x4787c62a, 12);
                    uint q21 = tt21 + block[5];
                    q21 = RL(q21, 5); q21 += Q[Qoff + 20];
                    if (0 != ((q21 ^ Q[Qoff + 20]) & 0x80020000))
                    {
                        LogReturn(5);
                        continue;
                    }

                    Q[Qoff + 21] = q21;
                    MD5_REVERSE_STEP(block, Q, 3, 0xc1bdceee, 22);
                    MD5_REVERSE_STEP(block, Q, 4, 0xf57c0faf, 7);
                    MD5_REVERSE_STEP(block, Q, 7, 0xfd469501, 22);

                    uint tt22 = GG(Q[Qoff + 21], Q[Qoff + 20], Q[Qoff + 19]) + Q[Qoff + 18] + 0x02441453;
                    uint tt23 = Q[Qoff + 19] + 0xd8a1e681 + block[15];
                    uint tt24 = Q[Qoff + 20] + 0xe7d3fbc8 + block[4];

                    uint tt9 = Q[Qoff + 6] + 0x8b44f7af;
                    uint tt10 = Q[Qoff + 7] + 0xffff5bb1;
                    uint tt8 = FF(Q[Qoff + 8], Q[Qoff + 7], Q[Qoff + 6]) + Q[Qoff + 5] + 0x698098d8;
                    uint tt12 = RR(Q[Qoff + 13] - Q[Qoff + 12], 7) - 0x6b901122;
                    uint tt13 = RR(Q[Qoff + 14] - Q[Qoff + 13], 12) - FF(Q[Qoff + 13], Q[Qoff + 12], Q[Qoff + 11]) - 0xfd987193;

                    // iterate over possible changes of q9 and q10
                    // while keeping conditions on q1-q21 intact
                    // this changes m8, m9, m10, m12 and m13 (and not m11!)
                    // the possible changes of q9 that also do not change m10 are used below
                    for (uint counter3 = 0; counter3 < (1 << 3);)
                    {
                        if (IsStopped)
                        {
                            return;
                        }

                        uint q10 = Q[Qoff + 10] ^ (q9q10mask[counter3] & 0x60);
                        Q[Qoff + 9] = q9backup ^ (q9q10mask[counter3] & 0x2000);
                        ++counter3;
                        uint m10 = RR(Q[Qoff + 11] - q10, 17);
                        m10 -= FF(q10, Q[Qoff + 9], Q[Qoff + 8]) + tt10;

                        uint aa = Q[Qoff + 21];
                        uint dd = tt22 + m10; dd = RL(dd, 9) + aa;
                        if (0x80000000 != (dd & 0x80000000))
                        {
                            LogReturn(6);
                            continue;
                        }

                        uint bb = Q[Qoff + 20];
                        uint cc = tt23 + GG(dd, aa, bb);
                        if (0 != (cc & 0x20000))
                        {
                            LogReturn(7);
                            continue;
                        }

                        cc = RL(cc, 14) + dd;
                        if (0 != (cc & 0x80000000))
                        {
                            LogReturn(8);
                            continue;
                        }

                        bb = tt24 + GG(cc, dd, aa); bb = RL(bb, 20) + cc;
                        if (0 == (bb & 0x80000000))
                        {
                            LogReturn(9);
                            continue;
                        }

                        block[10] = m10;
                        block[13] = tt13 - q10;

                        // iterate over possible changes of q9
                        // while keeping intact conditions on q1-q24
                        // this changes m8, m9 and m12 (but not m10!)
                        for (uint counter4 = 0; counter4 < (1 << 16); ++counter4)
                        {
                            if (IsStopped)
                            {
                                return;
                            }

                            uint q9 = Q[Qoff + 9] ^ q9mask[counter4];
                            block[12] = tt12 - FF(Q[Qoff + 12], Q[Qoff + 11], q10) - q9;
                            uint m8 = q9 - Q[Qoff + 8];
                            block[8] = RR(m8, 7) - tt8;
                            uint m9 = q10 - q9;
                            block[9] = RR(m9, 12) - FF(q9, Q[Qoff + 8], Q[Qoff + 7]) - tt9;

                            uint a = aa, b = bb, c = cc, d = dd;
                            MD5_STEP_GG(ref a, b, c, d, block[9], 0x21e1cde6, 5);
                            MD5_STEP_GG(ref d, a, b, c, block[14], 0xc33707d6, 9);
                            MD5_STEP_GG(ref c, d, a, b, block[3], 0xf4d50d87, 14);
                            MD5_STEP_GG(ref b, c, d, a, block[8], 0x455a14ed, 20);
                            MD5_STEP_GG(ref a, b, c, d, block[13], 0xa9e3e905, 5);
                            MD5_STEP_GG(ref d, a, b, c, block[2], 0xfcefa3f8, 9);
                            MD5_STEP_GG(ref c, d, a, b, block[7], 0x676f02d9, 14);
                            MD5_STEP_GG(ref b, c, d, a, block[12], 0x8d2a4c8a, 20);
                            MD5_STEP_HH(ref a, b, c, d, block[5], 0xfffa3942, 4);
                            MD5_STEP_HH(ref d, a, b, c, block[8], 0x8771f681, 11);

                            c += HH(d, a, b) + block[11] + 0x6d9d6122;
                            if (0 != (c & (1 << 15)))
                            {
                                LogReturn(10);
                                continue;
                            }
                            c = (c << 16 | c >> 16) + d;


                            MD5_STEP_HH(ref b, c, d, a, block[14], 0xfde5380c, 23);
                            MD5_STEP_HH(ref a, b, c, d, block[1], 0xa4beea44, 4);
                            MD5_STEP_HH(ref d, a, b, c, block[4], 0x4bdecfa9, 11);
                            MD5_STEP_HH(ref c, d, a, b, block[7], 0xf6bb4b60, 16);
                            MD5_STEP_HH(ref b, c, d, a, block[10], 0xbebfbc70, 23);
                            MD5_STEP_HH(ref a, b, c, d, block[13], 0x289b7ec6, 4);
                            MD5_STEP_HH(ref d, a, b, c, block[0], 0xeaa127fa, 11);
                            MD5_STEP_HH(ref c, d, a, b, block[3], 0xd4ef3085, 16);
                            MD5_STEP_HH(ref b, c, d, a, block[6], 0x04881d05, 23);
                            MD5_STEP_HH(ref a, b, c, d, block[9], 0xd9d4d039, 4);
                            MD5_STEP_HH(ref d, a, b, c, block[12], 0xe6db99e5, 11);
                            MD5_STEP_HH(ref c, d, a, b, block[15], 0x1fa27cf8, 16);
                            MD5_STEP_HH(ref b, c, d, a, block[2], 0xc4ac5665, 23);
                            if (0 != ((b ^ d) & 0x80000000))
                            {
                                LogReturn(11);
                                continue;
                            }


                            MD5_STEP_II(ref a, b, c, d, block[0], 0xf4292244, 6);
                            if (0 != ((a ^ c) >> 31))
                            {
                                LogReturn(12);
                                continue;
                            }
                            MD5_STEP_II(ref d, a, b, c, block[7], 0x432aff97, 10);
                            if (0 == ((b ^ d) >> 31))
                            {
                                LogReturn(13);
                                continue;
                            }
                            MD5_STEP_II(ref c, d, a, b, block[14], 0xab9423a7, 15);
                            if (0 != ((a ^ c) >> 31))
                            {
                                LogReturn(14);
                                continue;
                            }
                            MD5_STEP_II(ref b, c, d, a, block[5], 0xfc93a039, 21);
                            if (0 != ((b ^ d) >> 31))
                            {
                                LogReturn(15);
                                continue;
                            }
                            MD5_STEP_II(ref a, b, c, d, block[12], 0x655b59c3, 6);
                            if (0 != ((a ^ c) >> 31))
                            {
                                LogReturn(16);
                                continue;
                            }
                            MD5_STEP_II(ref d, a, b, c, block[3], 0x8f0ccc92, 10);
                            if (0 != ((b ^ d) >> 31))
                            {
                                LogReturn(17);
                                continue;
                            }
                            MD5_STEP_II(ref c, d, a, b, block[10], 0xffeff47d, 15);
                            if (0 != ((a ^ c) >> 31))
                            {
                                LogReturn(18);
                                continue;
                            }
                            MD5_STEP_II(ref b, c, d, a, block[1], 0x85845dd1, 21);
                            if (0 != ((b ^ d) >> 31))
                            {
                                LogReturn(19);
                                continue;
                            }
                            MD5_STEP_II(ref a, b, c, d, block[8], 0x6fa87e4f, 6);
                            if (0 != ((a ^ c) >> 31))
                            {
                                LogReturn(20);
                                continue;
                            }
                            MD5_STEP_II(ref d, a, b, c, block[15], 0xfe2ce6e0, 10);
                            if (0 != ((b ^ d) >> 31))
                            {
                                LogReturn(21);
                                continue;
                            }
                            MD5_STEP_II(ref c, d, a, b, block[6], 0xa3014314, 15);
                            if (0 != ((a ^ c) >> 31))
                            {
                                LogReturn(22);
                                continue;
                            }
                            MD5_STEP_II(ref b, c, d, a, block[13], 0x4e0811a1, 21);
                            if (0 == ((b ^ d) >> 31))
                            {
                                LogReturn(23);
                                continue;
                            }
                            MD5_STEP_II(ref a, b, c, d, block[4], 0xf7537e82, 6);
                            if (0 != ((a ^ c) >> 31))
                            {
                                LogReturn(24);
                                continue;
                            }
                            MD5_STEP_II(ref d, a, b, c, block[11], 0xbd3af235, 10);
                            if (0 != ((b ^ d) >> 31))
                            {
                                LogReturn(25);
                                continue;
                            }
                            MD5_STEP_II(ref c, d, a, b, block[2], 0x2ad7d2bb, 15);
                            if (0 != ((a ^ c) >> 31))
                            {
                                LogReturn(26);
                                continue;
                            }
                            MD5_STEP_II(ref b, c, d, a, block[9], 0xeb86d391, 21);

                            uint IHV1 = b + IV[1];
                            uint IHV2 = c + IV[2];
                            uint IHV3 = d + IV[3];

                            bool wang = true;
                            if (0x02000000 != ((IHV2 ^ IHV1) & 0x86000000))
                            {
                                wang = false;
                            }

                            if (0 != ((IHV1 ^ IHV3) & 0x82000000))
                            {
                                wang = false;
                            }

                            if (0 != (IHV1 & 0x06000020))
                            {
                                wang = false;
                            }

                            bool stevens = true;
                            if (((IHV1 ^ IHV2) >> 31) != 0 || ((IHV1 ^ IHV3) >> 31) != 0)
                            {
                                stevens = false;
                            }

                            if ((IHV3 & (1 << 25)) != 0 || (IHV2 & (1 << 25)) != 0 || (IHV1 & (1 << 25)) != 0
                                || ((IHV2 ^ IHV1) & 1) != 0)
                            {
                                stevens = false;
                            }

                            if (!(wang || stevens))
                            {
                                LogReturn(27);
                                continue;
                            }

                            uint[] IV1 = new uint[4], IV2 = new uint[4];
                            for (int t = 0; t < 4; ++t)
                            {
                                IV2[t] = IV1[t] = IV[t];
                            }

                            uint[] block2 = new uint[16];
                            for (int t = 0; t < 16; ++t)
                            {
                                block2[t] = block[t];
                            }

                            block2[4] += (uint)1 << 31;
                            block2[11] += 1 << 15;
                            block2[14] += (uint)1 << 31;

                            md5_compress(IV1, block);
                            md5_compress(IV2, block2);
                            if ((IV2[0] == IV1[0] + (1 << 31))
                                    && (IV2[1] == IV1[1] + (1 << 31) + (1 << 25))
                                    && (IV2[2] == IV1[2] + (1 << 31) + (1 << 25))
                                    && (IV2[3] == IV1[3] + (1 << 31) + (1 << 25)))
                            {
                                LogReturn(28);
                                return;
                            }

                            //if (IV2[0] != IV1[0] + (1<<31))
                            //	std::cout << "!" << std::flush;
                        }
                    }
                }
            }
        }

        private void md5_compress(uint[] ihv, uint[] block)
        {
            uint a = ihv[0];
            uint b = ihv[1];
            uint c = ihv[2];
            uint d = ihv[3];

            MD5_STEP_FF(ref a, b, c, d, block[0], 0xd76aa478, 7);
            MD5_STEP_FF(ref d, a, b, c, block[1], 0xe8c7b756, 12);
            MD5_STEP_FF(ref c, d, a, b, block[2], 0x242070db, 17);
            MD5_STEP_FF(ref b, c, d, a, block[3], 0xc1bdceee, 22);
            MD5_STEP_FF(ref a, b, c, d, block[4], 0xf57c0faf, 7);
            MD5_STEP_FF(ref d, a, b, c, block[5], 0x4787c62a, 12);
            MD5_STEP_FF(ref c, d, a, b, block[6], 0xa8304613, 17);
            MD5_STEP_FF(ref b, c, d, a, block[7], 0xfd469501, 22);
            MD5_STEP_FF(ref a, b, c, d, block[8], 0x698098d8, 7);
            MD5_STEP_FF(ref d, a, b, c, block[9], 0x8b44f7af, 12);
            MD5_STEP_FF(ref c, d, a, b, block[10], 0xffff5bb1, 17);
            MD5_STEP_FF(ref b, c, d, a, block[11], 0x895cd7be, 22);
            MD5_STEP_FF(ref a, b, c, d, block[12], 0x6b901122, 7);
            MD5_STEP_FF(ref d, a, b, c, block[13], 0xfd987193, 12);
            MD5_STEP_FF(ref c, d, a, b, block[14], 0xa679438e, 17);
            MD5_STEP_FF(ref b, c, d, a, block[15], 0x49b40821, 22);
            MD5_STEP_GG(ref a, b, c, d, block[1], 0xf61e2562, 5);
            MD5_STEP_GG(ref d, a, b, c, block[6], 0xc040b340, 9);
            MD5_STEP_GG(ref c, d, a, b, block[11], 0x265e5a51, 14);
            MD5_STEP_GG(ref b, c, d, a, block[0], 0xe9b6c7aa, 20);
            MD5_STEP_GG(ref a, b, c, d, block[5], 0xd62f105d, 5);
            MD5_STEP_GG(ref d, a, b, c, block[10], 0x02441453, 9);
            MD5_STEP_GG(ref c, d, a, b, block[15], 0xd8a1e681, 14);
            MD5_STEP_GG(ref b, c, d, a, block[4], 0xe7d3fbc8, 20);
            MD5_STEP_GG(ref a, b, c, d, block[9], 0x21e1cde6, 5);
            MD5_STEP_GG(ref d, a, b, c, block[14], 0xc33707d6, 9);
            MD5_STEP_GG(ref c, d, a, b, block[3], 0xf4d50d87, 14);
            MD5_STEP_GG(ref b, c, d, a, block[8], 0x455a14ed, 20);
            MD5_STEP_GG(ref a, b, c, d, block[13], 0xa9e3e905, 5);
            MD5_STEP_GG(ref d, a, b, c, block[2], 0xfcefa3f8, 9);
            MD5_STEP_GG(ref c, d, a, b, block[7], 0x676f02d9, 14);
            MD5_STEP_GG(ref b, c, d, a, block[12], 0x8d2a4c8a, 20);
            MD5_STEP_HH(ref a, b, c, d, block[5], 0xfffa3942, 4);
            MD5_STEP_HH(ref d, a, b, c, block[8], 0x8771f681, 11);
            MD5_STEP_HH(ref c, d, a, b, block[11], 0x6d9d6122, 16);
            MD5_STEP_HH(ref b, c, d, a, block[14], 0xfde5380c, 23);
            MD5_STEP_HH(ref a, b, c, d, block[1], 0xa4beea44, 4);
            MD5_STEP_HH(ref d, a, b, c, block[4], 0x4bdecfa9, 11);
            MD5_STEP_HH(ref c, d, a, b, block[7], 0xf6bb4b60, 16);
            MD5_STEP_HH(ref b, c, d, a, block[10], 0xbebfbc70, 23);
            MD5_STEP_HH(ref a, b, c, d, block[13], 0x289b7ec6, 4);
            MD5_STEP_HH(ref d, a, b, c, block[0], 0xeaa127fa, 11);
            MD5_STEP_HH(ref c, d, a, b, block[3], 0xd4ef3085, 16);
            MD5_STEP_HH(ref b, c, d, a, block[6], 0x04881d05, 23);
            MD5_STEP_HH(ref a, b, c, d, block[9], 0xd9d4d039, 4);
            MD5_STEP_HH(ref d, a, b, c, block[12], 0xe6db99e5, 11);
            MD5_STEP_HH(ref c, d, a, b, block[15], 0x1fa27cf8, 16);
            MD5_STEP_HH(ref b, c, d, a, block[2], 0xc4ac5665, 23);
            MD5_STEP_II(ref a, b, c, d, block[0], 0xf4292244, 6);
            MD5_STEP_II(ref d, a, b, c, block[7], 0x432aff97, 10);
            MD5_STEP_II(ref c, d, a, b, block[14], 0xab9423a7, 15);
            MD5_STEP_II(ref b, c, d, a, block[5], 0xfc93a039, 21);
            MD5_STEP_II(ref a, b, c, d, block[12], 0x655b59c3, 6);
            MD5_STEP_II(ref d, a, b, c, block[3], 0x8f0ccc92, 10);
            MD5_STEP_II(ref c, d, a, b, block[10], 0xffeff47d, 15);
            MD5_STEP_II(ref b, c, d, a, block[1], 0x85845dd1, 21);
            MD5_STEP_II(ref a, b, c, d, block[8], 0x6fa87e4f, 6);
            MD5_STEP_II(ref d, a, b, c, block[15], 0xfe2ce6e0, 10);
            MD5_STEP_II(ref c, d, a, b, block[6], 0xa3014314, 15);
            MD5_STEP_II(ref b, c, d, a, block[13], 0x4e0811a1, 21);
            MD5_STEP_II(ref a, b, c, d, block[4], 0xf7537e82, 6);
            MD5_STEP_II(ref d, a, b, c, block[11], 0xbd3af235, 10);
            MD5_STEP_II(ref c, d, a, b, block[2], 0x2ad7d2bb, 15);
            MD5_STEP_II(ref b, c, d, a, block[9], 0xeb86d391, 21);

            ihv[0] += a;
            ihv[1] += b;
            ihv[2] += c;
            ihv[3] += d;
        }

        private void MD5_STEP(RoundFunctionDelegate f, ref uint a, uint b, uint c, uint d, uint m, uint ac, int rc)
        {
            a += f(b, c, d) + m + ac;
            a = (a << rc | a >> (32 - rc)) + b;
        }

        private uint FF(uint b, uint c, uint d)
        { return d ^ (b & (c ^ d)); }

        private uint GG(uint b, uint c, uint d)
        { return c ^ (d & (b ^ c)); }

        private uint HH(uint b, uint c, uint d)
        { return b ^ c ^ d; }

        private uint II(uint b, uint c, uint d)
        { return c ^ (b | ~d); }

        private uint RL(uint x, int n)
        { return (x << n) | (x >> (32 - n)); }

        private uint RR(uint x, int n)
        { return (x >> n) | (x << (32 - n)); }

        private void MD5_STEP_FF(ref uint a, uint b, uint c, uint d, uint m, uint ac, int rc)
        {
            a += (d ^ (b & (c ^ d))) + m + ac;
            a = (a << rc | a >> (32 - rc)) + b;
        }

        private void MD5_STEP_GG(ref uint a, uint b, uint c, uint d, uint m, uint ac, int rc)
        {
            a += (c ^ (d & (b ^ c))) + m + ac;
            a = (a << rc | a >> (32 - rc)) + b;
        }

        private void MD5_STEP_HH(ref uint a, uint b, uint c, uint d, uint m, uint ac, int rc)
        {
            a += (b ^ c ^ d) + m + ac;
            a = (a << rc | a >> (32 - rc)) + b;
        }

        private void MD5_STEP_II(ref uint a, uint b, uint c, uint d, uint m, uint ac, int rc)
        {
            a += (c ^ (b | ~d)) + m + ac;
            a = (a << rc | a >> (32 - rc)) + b;
        }

        private void find_block1(uint[] block, uint[] IV)
        {
            if (((IV[1] ^ IV[2]) & (1 << 31)) == 0 && ((IV[1] ^ IV[3]) & (1 << 31)) == 0
                && (IV[3] & (1 << 25)) == 0 && (IV[2] & (1 << 25)) == 0 && (IV[1] & (1 << 25)) == 0 && ((IV[2] ^ IV[1]) & 1) == 0
               )
            {
                uint[] IV2 = new uint[] { (uint)(IV[0] + (1 << 31)), (uint)(IV[1] + (1 << 31) + (1 << 25)), (uint)(IV[2] + (1 << 31) + (1 << 25)), (uint)(IV[3] + (1 << 31) + (1 << 25)) };
                if ((IV[1] & (1 << 6)) != 0 && (IV[1] & 1) != 0)
                {
                    find_block1_stevens_11(block, IV2);
                }
                else if ((IV[1] & (1 << 6)) != 0 && (IV[1] & 1) == 0)
                {
                    find_block1_stevens_10(block, IV2);
                }
                else if ((IV[1] & (1 << 6)) == 0 && (IV[1] & 1) != 0)
                {
                    find_block1_stevens_01(block, IV2);
                }
                else
                {
                    find_block1_stevens_00(block, IV2);
                }
                block[4] += (uint)1 << 31;
                block[11] += 1 << 15;
                block[14] += (uint)1 << 31;
            }
            else
            {
                find_block1_wang(block, IV);
            }
        }

        private void find_block1_stevens_00(uint[] block, uint[] IV)
        {
            MatchProgressMax = 29;
            MatchProgress = 0;

            uint[] Q = new uint[68];
            Q[0] = IV[0];
            Q[1] = IV[3];
            Q[2] = IV[2];
            Q[3] = IV[1];

            uint[] q9q10mask = new uint[1 << 3];
            for (uint k = 0; k < q9q10mask.Length; ++k)
            {
                q9q10mask[k] = ((k << 5) ^ (k << 12) ^ (k << 25)) & 0x08002020;
            }

            uint[] q9mask = new uint[1 << 9];
            for (uint k = 0; k < q9mask.Length; ++k)
            {
                q9mask[k] = ((k << 1) ^ (k << 3) ^ (k << 6) ^ (k << 8) ^ (k << 11) ^ (k << 14) ^ (k << 18)) & 0x04310d12;
            }

            while (true)
            {
                if (IsStopped)
                {
                    return;
                }

                uint aa1 = Q[Qoff] & 0x80000000;

                Q[Qoff + 2] = (xrng64() & 0x49a0e73e) | 0x221f00c1 | aa1;
                Q[Qoff + 3] = (xrng64() & 0x0000040c) | 0x3fce1a71 | (Q[Qoff + 2] & 0x8000e000);
                Q[Qoff + 4] = (xrng64() & 0x00000004) | (0xa5f281a2 ^ (Q[Qoff + 3] & 0x80000008));
                Q[Qoff + 5] = (xrng64() & 0x00000004) | 0x67fd823b;
                Q[Qoff + 6] = (xrng64() & 0x00001044) | 0x15e5829a;
                Q[Qoff + 7] = (xrng64() & 0x00200806) | 0x950430b0;
                Q[Qoff + 8] = (xrng64() & 0x60050110) | 0x1bd29ca2 | (Q[Qoff + 7] & 0x00000004);
                Q[Qoff + 9] = (xrng64() & 0x40044000) | 0xb8820004;
                Q[Qoff + 10] = 0xf288b209 | (Q[Qoff + 9] & 0x00044000);
                Q[Qoff + 11] = (xrng64() & 0x12888008) | 0x85712f57;
                Q[Qoff + 12] = (xrng64() & 0x1ed98d7f) | 0xc0023080 | (~Q[Qoff + 11] & 0x00200000);
                Q[Qoff + 13] = (xrng64() & 0x0efb1d77) | 0x1000c008;
                Q[Qoff + 14] = (xrng64() & 0x0fff5d77) | 0xa000a288;
                Q[Qoff + 15] = (xrng64() & 0x0efe7ff7) | 0xe0008000 | (~Q[Qoff + 14] & 0x00010000);
                Q[Qoff + 16] = (xrng64() & 0x0ffdffff) | 0xf0000000 | (~Q[Qoff + 15] & 0x00020000);

                MD5_REVERSE_STEP(block, Q, 5, 0x4787c62a, 12);
                MD5_REVERSE_STEP(block, Q, 6, 0xa8304613, 17);
                MD5_REVERSE_STEP(block, Q, 7, 0xfd469501, 22);
                MD5_REVERSE_STEP(block, Q, 11, 0x895cd7be, 22);
                MD5_REVERSE_STEP(block, Q, 14, 0xa679438e, 17);
                MD5_REVERSE_STEP(block, Q, 15, 0x49b40821, 22);

                uint tt17 = GG(Q[Qoff + 16], Q[Qoff + 15], Q[Qoff + 14]) + Q[Qoff + 13] + 0xf61e2562;
                uint tt18 = Q[Qoff + 14] + 0xc040b340 + block[6];
                uint tt19 = Q[Qoff + 15] + 0x265e5a51 + block[11];

                uint tt0 = FF(Q[Qoff + 0], Q[Qoff - 1], Q[Qoff - 2]) + Q[Qoff - 3] + 0xd76aa478;
                uint tt1 = Q[Qoff - 2] + 0xe8c7b756;
                uint q1a = 0x02020801 | (Q[Qoff + 0] & 0x80000000);

                uint counter = 0;
                while (counter < (1 << 12))
                {
                    if (IsStopped)
                    {
                        return;
                    }

                    ++counter;

                    uint q1 = q1a | (xrng64() & 0x7dfdf7be);
                    uint m1 = Q[Qoff + 2] - q1;
                    m1 = RR(m1, 12) - FF(q1, Q[Qoff + 0], Q[Qoff - 1]) - tt1;

                    uint q16 = Q[Qoff + 16];
                    uint q17 = tt17 + m1;
                    q17 = RL(q17, 5) + q16;
                    if (0x80000000 != ((q17 ^ q16) & 0x80008008))
                    {
                        LogReturn(1);
                        continue;
                    }

                    if (0 != (q17 & 0x00020000))
                    {
                        LogReturn(2);
                        continue;
                    }

                    uint q18 = GG(q17, q16, Q[Qoff + 15]) + tt18;
                    q18 = RL(q18, 9); q18 += q17;
                    if (0x80020000 != ((q18 ^ q17) & 0xa0020000))
                    {
                        LogReturn(3);
                        continue;
                    }

                    uint q19 = GG(q18, q17, q16) + tt19;
                    q19 = RL(q19, 14); q19 += q18;
                    if (0x80000000 != (q19 & 0x80020000))
                    {
                        LogReturn(4);
                        continue;
                    }

                    uint m0 = q1 - Q[Qoff + 0];
                    m0 = RR(m0, 7) - tt0;

                    uint q20 = GG(q19, q18, q17) + q16 + 0xe9b6c7aa + m0;
                    q20 = RL(q20, 20); q20 += q19;
                    if (0x00040000 != ((q20 ^ q19) & 0x80040000))
                    {
                        LogReturn(5);
                        continue;
                    }

                    Q[Qoff + 1] = q1;
                    Q[Qoff + 17] = q17;
                    Q[Qoff + 18] = q18;
                    Q[Qoff + 19] = q19;
                    Q[Qoff + 20] = q20;

                    block[0] = m0;
                    block[1] = m1;

                    MD5_REVERSE_STEP(block, Q, 5, 0x4787c62a, 12);
                    uint q21 = GG(Q[Qoff + 20], Q[Qoff + 19], Q[Qoff + 18]) + Q[Qoff + 17] + 0xd62f105d + block[5];
                    q21 = RL(q21, 5); q21 += Q[Qoff + 20];
                    if (0 != ((q21 ^ Q[Qoff + 20]) & 0x80020000))
                    {
                        LogReturn(6);
                        continue;
                    }
                    Q[Qoff + 21] = q21;

                    counter = 0;
                    break;
                }
                if (counter != 0)
                {
                    LogReturn(7);
                    continue;
                }

                uint q9b = Q[Qoff + 9];
                uint q10b = Q[Qoff + 10];

                MD5_REVERSE_STEP(block, Q, 2, 0x242070db, 17);
                MD5_REVERSE_STEP(block, Q, 3, 0xc1bdceee, 22);
                MD5_REVERSE_STEP(block, Q, 4, 0xf57c0faf, 7);
                MD5_REVERSE_STEP(block, Q, 7, 0xfd469501, 22);

                uint tt10 = Q[Qoff + 7] + 0xffff5bb1;
                uint tt22 = GG(Q[Qoff + 21], Q[Qoff + 20], Q[Qoff + 19]) + Q[Qoff + 18] + 0x02441453;
                uint tt23 = Q[Qoff + 19] + 0xd8a1e681 + block[15];
                uint tt24 = Q[Qoff + 20] + 0xe7d3fbc8 + block[4];

                for (uint k10 = 0; k10 < (1 << 3); ++k10)
                {
                    if (IsStopped)
                    {
                        return;
                    }

                    uint q10 = q10b | (q9q10mask[k10] & 0x08000020);
                    uint m10 = RR(Q[Qoff + 11] - q10, 17);
                    uint q9 = q9b | (q9q10mask[k10] & 0x00002000);

                    m10 -= FF(q10, q9, Q[Qoff + 8]) + tt10;

                    uint aa = Q[Qoff + 21];
                    uint dd = tt22 + m10; dd = RL(dd, 9) + aa;
                    if (0 == (dd & 0x80000000))
                    {
                        LogReturn(8);
                        continue;
                    }

                    uint bb = Q[Qoff + 20];
                    uint cc = tt23 + GG(dd, aa, bb);
                    if (0 != (cc & 0x20000))
                    {
                        LogReturn(9);
                        continue;
                    }
                    cc = RL(cc, 14) + dd;
                    if (0 != (cc & 0x80000000))
                    {
                        LogReturn(10);
                        continue;
                    }

                    bb = tt24 + GG(cc, dd, aa); bb = RL(bb, 20) + cc;
                    if (0 == (bb & 0x80000000))
                    {
                        LogReturn(11);
                        continue;
                    }

                    block[10] = m10;
                    Q[Qoff + 9] = q9;
                    Q[Qoff + 10] = q10;
                    MD5_REVERSE_STEP(block, Q, 13, 0xfd987193, 12);

                    for (uint k9 = 0; k9 < (1 << 9); ++k9)
                    {
                        if (IsStopped)
                        {
                            return;
                        }

                        uint a = aa, b = bb, c = cc, d = dd;
                        Q[Qoff + 9] = q9 ^ q9mask[k9];
                        MD5_REVERSE_STEP(block, Q, 8, 0x698098d8, 7);
                        MD5_REVERSE_STEP(block, Q, 9, 0x8b44f7af, 12);
                        MD5_REVERSE_STEP(block, Q, 12, 0x6b901122, 7);

                        MD5_STEP_GG(ref a, b, c, d, block[9], 0x21e1cde6, 5);
                        MD5_STEP_GG(ref d, a, b, c, block[14], 0xc33707d6, 9);
                        MD5_STEP_GG(ref c, d, a, b, block[3], 0xf4d50d87, 14);
                        MD5_STEP_GG(ref b, c, d, a, block[8], 0x455a14ed, 20);
                        MD5_STEP_GG(ref a, b, c, d, block[13], 0xa9e3e905, 5);
                        MD5_STEP_GG(ref d, a, b, c, block[2], 0xfcefa3f8, 9);
                        MD5_STEP_GG(ref c, d, a, b, block[7], 0x676f02d9, 14);
                        MD5_STEP_GG(ref b, c, d, a, block[12], 0x8d2a4c8a, 20);
                        MD5_STEP_HH(ref a, b, c, d, block[5], 0xfffa3942, 4);
                        MD5_STEP_HH(ref d, a, b, c, block[8], 0x8771f681, 11);

                        c += HH(d, a, b) + block[11] + 0x6d9d6122;
                        if (0 != (c & (1 << 15)))
                        {
                            LogReturn(12);
                            continue;
                        }
                        c = (c << 16 | c >> 16) + d;

                        MD5_STEP_HH(ref b, c, d, a, block[14], 0xfde5380c, 23);
                        MD5_STEP_HH(ref a, b, c, d, block[1], 0xa4beea44, 4);
                        MD5_STEP_HH(ref d, a, b, c, block[4], 0x4bdecfa9, 11);
                        MD5_STEP_HH(ref c, d, a, b, block[7], 0xf6bb4b60, 16);
                        MD5_STEP_HH(ref b, c, d, a, block[10], 0xbebfbc70, 23);
                        MD5_STEP_HH(ref a, b, c, d, block[13], 0x289b7ec6, 4);
                        MD5_STEP_HH(ref d, a, b, c, block[0], 0xeaa127fa, 11);
                        MD5_STEP_HH(ref c, d, a, b, block[3], 0xd4ef3085, 16);
                        MD5_STEP_HH(ref b, c, d, a, block[6], 0x04881d05, 23);
                        MD5_STEP_HH(ref a, b, c, d, block[9], 0xd9d4d039, 4);
                        MD5_STEP_HH(ref d, a, b, c, block[12], 0xe6db99e5, 11);
                        MD5_STEP_HH(ref c, d, a, b, block[15], 0x1fa27cf8, 16);
                        MD5_STEP_HH(ref b, c, d, a, block[2], 0xc4ac5665, 23);
                        if (0 != ((b ^ d) & 0x80000000))
                        {
                            LogReturn(13);
                            continue;
                        }

                        MD5_STEP_II(ref a, b, c, d, block[0], 0xf4292244, 6);
                        if (0 != ((a ^ c) >> 31))
                        {
                            LogReturn(14);
                            continue;
                        }
                        MD5_STEP_II(ref d, a, b, c, block[7], 0x432aff97, 10);
                        if (0 == ((b ^ d) >> 31))
                        {
                            LogReturn(15);
                            continue;
                        }
                        MD5_STEP_II(ref c, d, a, b, block[14], 0xab9423a7, 15);
                        if (0 != ((a ^ c) >> 31))
                        {
                            LogReturn(16);
                            continue;
                        }
                        MD5_STEP_II(ref b, c, d, a, block[5], 0xfc93a039, 21);
                        if (0 != ((b ^ d) >> 31))
                        {
                            LogReturn(17);
                            continue;
                        }
                        MD5_STEP_II(ref a, b, c, d, block[12], 0x655b59c3, 6);
                        if (0 != ((a ^ c) >> 31))
                        {
                            LogReturn(18);
                            continue;
                        }
                        MD5_STEP_II(ref d, a, b, c, block[3], 0x8f0ccc92, 10);
                        if (0 != ((b ^ d) >> 31))
                        {
                            LogReturn(19);
                            continue;
                        }
                        MD5_STEP_II(ref c, d, a, b, block[10], 0xffeff47d, 15);
                        if (0 != ((a ^ c) >> 31))
                        {
                            LogReturn(20);
                            continue;
                        }
                        MD5_STEP_II(ref b, c, d, a, block[1], 0x85845dd1, 21);
                        if (0 != ((b ^ d) >> 31))
                        {
                            LogReturn(21);
                            continue;
                        }
                        MD5_STEP_II(ref a, b, c, d, block[8], 0x6fa87e4f, 6);
                        if (0 != ((a ^ c) >> 31))
                        {
                            LogReturn(22);
                            continue;
                        }
                        MD5_STEP_II(ref d, a, b, c, block[15], 0xfe2ce6e0, 10);
                        if (0 != ((b ^ d) >> 31))
                        {
                            LogReturn(23);
                            continue;
                        }
                        MD5_STEP_II(ref c, d, a, b, block[6], 0xa3014314, 15);
                        if (0 != ((a ^ c) >> 31))
                        {
                            LogReturn(24);
                            continue;
                        }
                        MD5_STEP_II(ref b, c, d, a, block[13], 0x4e0811a1, 21);
                        if (0 == ((b ^ d) >> 31))
                        {
                            LogReturn(25);
                            continue;
                        }
                        MD5_STEP_II(ref a, b, c, d, block[4], 0xf7537e82, 6);
                        if (0 != ((a ^ c) >> 31))
                        {
                            LogReturn(26);
                            continue;
                        }
                        MD5_STEP_II(ref d, a, b, c, block[11], 0xbd3af235, 10);
                        if (0 != ((b ^ d) >> 31))
                        {
                            LogReturn(27);
                            continue;
                        }
                        MD5_STEP_II(ref c, d, a, b, block[2], 0x2ad7d2bb, 15);
                        if (0 != ((a ^ c) >> 31))
                        {
                            LogReturn(28);
                            continue;
                        }
                        MD5_STEP_II(ref b, c, d, a, block[9], 0xeb86d391, 21);

                        uint[] block2 = new uint[16];
                        uint[] IV1 = new uint[4], IV2 = new uint[4];
                        for (int t = 0; t < 4; ++t)
                        {
                            IV1[t] = IV[t];
                            IV2[t] = IV[t] + ((uint)1 << 31);
                        }
                        IV2[1] -= (1 << 25);
                        IV2[2] -= (1 << 25);
                        IV2[3] -= (1 << 25);

                        for (int t = 0; t < 16; ++t)
                        {
                            block2[t] = block[t];
                        }

                        block2[4] += (uint)1 << 31;
                        block2[11] += 1 << 15;
                        block2[14] += (uint)1 << 31;

                        md5_compress(IV1, block);
                        md5_compress(IV2, block2);
                        if (IV2[0] == IV1[0] && IV2[1] == IV1[1] && IV2[2] == IV1[2] && IV2[3] == IV1[3])
                        {
                            LogReturn(29);
                            return;
                        }

                        //if (IV2[0] != IV1[0])
                        //		std::cout << "!" << std::flush;
                    }
                }
            }
        }

        private void find_block1_stevens_01(uint[] block, uint[] IV)
        {
            MatchProgressMax = 30;
            MatchProgress = 0;

            uint[] Q = new uint[68];
            Q[0] = IV[0];
            Q[1] = IV[3];
            Q[2] = IV[2];
            Q[3] = IV[1];

            uint[] q9q10mask = new uint[1 << 5];
            for (uint k = 0; k < q9q10mask.Length; ++k)
            {
                q9q10mask[k] = ((k << 4) ^ (k << 11) ^ (k << 24) ^ (k << 27)) & 0x88002030;
            }

            uint[] q9mask = new uint[1 << 9];
            for (uint k = 0; k < q9mask.Length; ++k)
            {
                q9mask[k] = ((k << 1) ^ (k << 7) ^ (k << 9) ^ (k << 12) ^ (k << 15) ^ (k << 19) ^ (k << 22)) & 0x44310d02;
            }

            while (true)
            {
                if (IsStopped)
                {
                    return;
                }

                uint aa1 = Q[Qoff] & 0x80000000;

                Q[Qoff + 2] = (xrng64() & 0x4db0e03e) | 0x32460441 | aa1;
                Q[Qoff + 3] = (xrng64() & 0x0c000008) | 0x123c3af1 | (Q[Qoff + 2] & 0x80800002);
                Q[Qoff + 4] = 0xe398f812 ^ (Q[Qoff + 3] & 0x88000000);
                Q[Qoff + 5] = (xrng64() & 0x82000000) | 0x4c66e99e;
                Q[Qoff + 6] = (xrng64() & 0x80000000) | 0x27180590;
                Q[Qoff + 7] = (xrng64() & 0x00010130) | 0x51ea9e47;
                Q[Qoff + 8] = (xrng64() & 0x40200800) | 0xb7c291e5;
                Q[Qoff + 9] = (xrng64() & 0x00044000) | 0x380002b4;
                Q[Qoff + 10] = 0xb282b208 | (Q[Qoff + 9] & 0x00044000);
                Q[Qoff + 11] = (xrng64() & 0x12808008) | 0xc5712f47;
                Q[Qoff + 12] = (xrng64() & 0x1ef18d7f) | 0x000a3080;
                Q[Qoff + 13] = (xrng64() & 0x1efb1d77) | 0x4004c008;
                Q[Qoff + 14] = (xrng64() & 0x1fff5d77) | 0x6000a288;
                Q[Qoff + 15] = (xrng64() & 0x1efe7ff7) | 0xa0008000 | (~Q[Qoff + 14] & 0x00010000);
                Q[Qoff + 16] = (xrng64() & 0x1ffdffff) | 0x20000000 | (~Q[Qoff + 15] & 0x00020000);

                MD5_REVERSE_STEP(block, Q, 5, 0x4787c62a, 12);
                MD5_REVERSE_STEP(block, Q, 6, 0xa8304613, 17);
                MD5_REVERSE_STEP(block, Q, 7, 0xfd469501, 22);
                MD5_REVERSE_STEP(block, Q, 11, 0x895cd7be, 22);
                MD5_REVERSE_STEP(block, Q, 14, 0xa679438e, 17);
                MD5_REVERSE_STEP(block, Q, 15, 0x49b40821, 22);

                uint tt17 = GG(Q[Qoff + 16], Q[Qoff + 15], Q[Qoff + 14]) + Q[Qoff + 13] + 0xf61e2562;
                uint tt18 = Q[Qoff + 14] + 0xc040b340 + block[6];
                uint tt19 = Q[Qoff + 15] + 0x265e5a51 + block[11];

                uint tt0 = FF(Q[Qoff + 0], Q[Qoff - 1], Q[Qoff - 2]) + Q[Qoff - 3] + 0xd76aa478;
                uint tt1 = Q[Qoff - 2] + 0xe8c7b756;

                uint q1a = 0x02000021 ^ (Q[Qoff + 0] & 0x80000020);

                uint counter = 0;
                while (counter < (1 << 12))
                {
                    if (IsStopped)
                    {
                        return;
                    }

                    ++counter;

                    uint q1 = q1a | (xrng64() & 0x7dfff39e);
                    uint m1 = Q[Qoff + 2] - q1;
                    m1 = RR(m1, 12) - FF(q1, Q[Qoff + 0], Q[Qoff - 1]) - tt1;

                    uint q16 = Q[Qoff + 16];
                    uint q17 = tt17 + m1;
                    q17 = RL(q17, 5) + q16;
                    if (0x80000000 != ((q17 ^ q16) & 0x80008008))
                    {
                        LogReturn(1);
                        continue;
                    }
                    if (0 != (q17 & 0x00020000))
                    {
                        LogReturn(2);
                        continue;
                    }

                    uint q18 = GG(q17, q16, Q[Qoff + 15]) + tt18;
                    q18 = RL(q18, 9); q18 += q17;
                    if (0x80020000 != ((q18 ^ q17) & 0xa0020000))
                    {
                        LogReturn(3);
                        continue;
                    }

                    uint q19 = GG(q18, q17, q16) + tt19;
                    q19 = RL(q19, 14); q19 += q18;
                    if (0 != (q19 & 0x80020000))
                    {
                        LogReturn(4);
                        continue;
                    }

                    uint m0 = q1 - Q[Qoff + 0];
                    m0 = RR(m0, 7) - tt0;

                    uint q20 = GG(q19, q18, q17) + q16 + 0xe9b6c7aa + m0;
                    q20 = RL(q20, 20); q20 += q19;
                    if (0x00040000 != ((q20 ^ q19) & 0x80040000))
                    {
                        LogReturn(5);
                        continue;
                    }

                    Q[Qoff + 1] = q1;
                    Q[Qoff + 17] = q17;
                    Q[Qoff + 18] = q18;
                    Q[Qoff + 19] = q19;
                    Q[Qoff + 20] = q20;

                    block[0] = m0;
                    block[1] = m1;

                    MD5_REVERSE_STEP(block, Q, 5, 0x4787c62a, 12);
                    uint q21 = GG(Q[Qoff + 20], Q[Qoff + 19], Q[Qoff + 18]) + Q[Qoff + 17] + 0xd62f105d + block[5];
                    q21 = RL(q21, 5); q21 += Q[Qoff + 20];
                    if (0 != ((q21 ^ Q[Qoff + 20]) & 0x80020000))
                    {
                        LogReturn(6);
                        continue;
                    }

                    Q[Qoff + 21] = q21;

                    counter = 0;

                    LogReturn(7);
                    break;
                }
                if (counter != 0)
                {
                    LogReturn(8);
                    continue;
                }

                uint q9b = Q[Qoff + 9];
                uint q10b = Q[Qoff + 10];

                MD5_REVERSE_STEP(block, Q, 2, 0x242070db, 17);
                MD5_REVERSE_STEP(block, Q, 3, 0xc1bdceee, 22);
                MD5_REVERSE_STEP(block, Q, 4, 0xf57c0faf, 7);
                MD5_REVERSE_STEP(block, Q, 7, 0xfd469501, 22);

                uint tt10 = Q[Qoff + 7] + 0xffff5bb1;
                uint tt22 = GG(Q[Qoff + 21], Q[Qoff + 20], Q[Qoff + 19]) + Q[Qoff + 18] + 0x02441453;
                uint tt23 = Q[Qoff + 19] + 0xd8a1e681 + block[15];
                uint tt24 = Q[Qoff + 20] + 0xe7d3fbc8 + block[4];

                for (uint k10 = 0; k10 < (1 << 5); ++k10)
                {
                    if (IsStopped)
                    {
                        return;
                    }

                    uint q10 = q10b | (q9q10mask[k10] & 0x08000030);
                    uint m10 = RR(Q[Qoff + 11] - q10, 17);
                    uint q9 = q9b | (q9q10mask[k10] & 0x80002000);

                    m10 -= FF(q10, q9, Q[Qoff + 8]) + tt10;

                    uint aa = Q[Qoff + 21];
                    uint dd = tt22 + m10; dd = RL(dd, 9) + aa;
                    if (0 != (dd & 0x80000000))
                    {
                        LogReturn(9);
                        continue;
                    }

                    uint bb = Q[Qoff + 20];
                    uint cc = tt23 + GG(dd, aa, bb);
                    if (0 != (cc & 0x20000))
                    {
                        LogReturn(10);
                        continue;
                    }
                    cc = RL(cc, 14) + dd;
                    if (0 != (cc & 0x80000000))
                    {
                        LogReturn(11);
                        continue;
                    }

                    bb = tt24 + GG(cc, dd, aa); bb = RL(bb, 20) + cc;
                    if (0 == (bb & 0x80000000))
                    {
                        LogReturn(12);
                        continue;
                    }

                    block[10] = m10;
                    Q[Qoff + 9] = q9;
                    Q[Qoff + 10] = q10;
                    MD5_REVERSE_STEP(block, Q, 13, 0xfd987193, 12);

                    for (uint k9 = 0; k9 < (1 << 9); ++k9)
                    {
                        if (IsStopped)
                        {
                            return;
                        }

                        uint a = aa, b = bb, c = cc, d = dd;
                        Q[Qoff + 9] = q9 ^ q9mask[k9];
                        MD5_REVERSE_STEP(block, Q, 8, 0x698098d8, 7);
                        MD5_REVERSE_STEP(block, Q, 9, 0x8b44f7af, 12);
                        MD5_REVERSE_STEP(block, Q, 12, 0x6b901122, 7);

                        MD5_STEP_GG(ref a, b, c, d, block[9], 0x21e1cde6, 5);
                        MD5_STEP_GG(ref d, a, b, c, block[14], 0xc33707d6, 9);
                        MD5_STEP_GG(ref c, d, a, b, block[3], 0xf4d50d87, 14);
                        MD5_STEP_GG(ref b, c, d, a, block[8], 0x455a14ed, 20);
                        MD5_STEP_GG(ref a, b, c, d, block[13], 0xa9e3e905, 5);
                        MD5_STEP_GG(ref d, a, b, c, block[2], 0xfcefa3f8, 9);
                        MD5_STEP_GG(ref c, d, a, b, block[7], 0x676f02d9, 14);
                        MD5_STEP_GG(ref b, c, d, a, block[12], 0x8d2a4c8a, 20);
                        MD5_STEP_HH(ref a, b, c, d, block[5], 0xfffa3942, 4);
                        MD5_STEP_HH(ref d, a, b, c, block[8], 0x8771f681, 11);

                        c += HH(d, a, b) + block[11] + 0x6d9d6122;
                        if (0 != (c & (1 << 15)))
                        {
                            LogReturn(13);
                            continue;
                        }
                        c = (c << 16 | c >> 16) + d;

                        MD5_STEP_HH(ref b, c, d, a, block[14], 0xfde5380c, 23);
                        MD5_STEP_HH(ref a, b, c, d, block[1], 0xa4beea44, 4);
                        MD5_STEP_HH(ref d, a, b, c, block[4], 0x4bdecfa9, 11);
                        MD5_STEP_HH(ref c, d, a, b, block[7], 0xf6bb4b60, 16);
                        MD5_STEP_HH(ref b, c, d, a, block[10], 0xbebfbc70, 23);
                        MD5_STEP_HH(ref a, b, c, d, block[13], 0x289b7ec6, 4);
                        MD5_STEP_HH(ref d, a, b, c, block[0], 0xeaa127fa, 11);
                        MD5_STEP_HH(ref c, d, a, b, block[3], 0xd4ef3085, 16);
                        MD5_STEP_HH(ref b, c, d, a, block[6], 0x04881d05, 23);
                        MD5_STEP_HH(ref a, b, c, d, block[9], 0xd9d4d039, 4);
                        MD5_STEP_HH(ref d, a, b, c, block[12], 0xe6db99e5, 11);
                        MD5_STEP_HH(ref c, d, a, b, block[15], 0x1fa27cf8, 16);
                        MD5_STEP_HH(ref b, c, d, a, block[2], 0xc4ac5665, 23);
                        if (0 != ((b ^ d) & 0x80000000))
                        {
                            LogReturn(14);
                            continue;
                        }

                        MD5_STEP_II(ref a, b, c, d, block[0], 0xf4292244, 6);
                        if (0 != ((a ^ c) >> 31))
                        {
                            LogReturn(15);
                            continue;
                        }
                        MD5_STEP_II(ref d, a, b, c, block[7], 0x432aff97, 10);
                        if (0 == ((b ^ d) >> 31))
                        {
                            LogReturn(16);
                            continue;
                        }
                        MD5_STEP_II(ref c, d, a, b, block[14], 0xab9423a7, 15);
                        if (0 != ((a ^ c) >> 31))
                        {
                            LogReturn(17);
                            continue;
                        }
                        MD5_STEP_II(ref b, c, d, a, block[5], 0xfc93a039, 21);
                        if (0 != ((b ^ d) >> 31))
                        {
                            LogReturn(18);
                            continue;
                        }
                        MD5_STEP_II(ref a, b, c, d, block[12], 0x655b59c3, 6);
                        if (0 != ((a ^ c) >> 31))
                        {
                            LogReturn(19);
                            continue;
                        }
                        MD5_STEP_II(ref d, a, b, c, block[3], 0x8f0ccc92, 10);
                        if (0 != ((b ^ d) >> 31))
                        {
                            LogReturn(20);
                            continue;
                        }
                        MD5_STEP_II(ref c, d, a, b, block[10], 0xffeff47d, 15);
                        if (0 != ((a ^ c) >> 31))
                        {
                            LogReturn(21);
                            continue;
                        }
                        MD5_STEP_II(ref b, c, d, a, block[1], 0x85845dd1, 21);
                        if (0 != ((b ^ d) >> 31))
                        {
                            LogReturn(22);
                            continue;
                        }
                        MD5_STEP_II(ref a, b, c, d, block[8], 0x6fa87e4f, 6);
                        if (0 != ((a ^ c) >> 31))
                        {
                            LogReturn(23);
                            continue;
                        }
                        MD5_STEP_II(ref d, a, b, c, block[15], 0xfe2ce6e0, 10);
                        if (0 != ((b ^ d) >> 31))
                        {
                            LogReturn(24);
                            continue;
                        }
                        MD5_STEP_II(ref c, d, a, b, block[6], 0xa3014314, 15);
                        if (0 != ((a ^ c) >> 31))
                        {
                            LogReturn(25);
                            continue;
                        }
                        MD5_STEP_II(ref b, c, d, a, block[13], 0x4e0811a1, 21);
                        if (0 == ((b ^ d) >> 31))
                        {
                            LogReturn(26);
                            continue;
                        }
                        MD5_STEP_II(ref a, b, c, d, block[4], 0xf7537e82, 6);
                        if (0 != ((a ^ c) >> 31))
                        {
                            LogReturn(27);
                            continue;
                        }
                        MD5_STEP_II(ref d, a, b, c, block[11], 0xbd3af235, 10);
                        if (0 != ((b ^ d) >> 31))
                        {
                            LogReturn(28);
                            continue;
                        }
                        MD5_STEP_II(ref c, d, a, b, block[2], 0x2ad7d2bb, 15);
                        if (0 != ((a ^ c) >> 31))
                        {
                            LogReturn(29);
                            continue;
                        }
                        MD5_STEP_II(ref b, c, d, a, block[9], 0xeb86d391, 21);

                        uint[] block2 = new uint[16];
                        uint[] IV1 = new uint[4], IV2 = new uint[4];
                        for (int t = 0; t < 4; ++t)
                        {
                            IV1[t] = IV[t];
                            IV2[t] = IV[t] + ((uint)1 << 31);
                        }
                        IV2[1] -= (1 << 25);
                        IV2[2] -= (1 << 25);
                        IV2[3] -= (1 << 25);

                        for (int t = 0; t < 16; ++t)
                        {
                            block2[t] = block[t];
                        }

                        block2[4] += (uint)1 << 31;
                        block2[11] += 1 << 15;
                        block2[14] += (uint)1 << 31;

                        md5_compress(IV1, block);
                        md5_compress(IV2, block2);
                        if (IV2[0] == IV1[0] && IV2[1] == IV1[1] && IV2[2] == IV1[2] && IV2[3] == IV1[3])
                        {
                            LogReturn(30);
                            return;
                        }

                        //if (IV2[0] != IV1[0])
                        //		std::cout << "!" << std::flush;
                    }
                }
            }
        }

        private void find_block1_stevens_10(uint[] block, uint[] IV)
        {
            MatchProgressMax = 30;
            MatchProgress = 0;

            uint[] Q = new uint[68];
            Q[0] = IV[0];
            Q[1] = IV[3];
            Q[2] = IV[2];
            Q[3] = IV[1];

            uint[] q9q10mask = new uint[1 << 4];
            for (uint k = 0; k < q9q10mask.Length; ++k)
            {
                q9q10mask[k] = ((k << 2) ^ (k << 8) ^ (k << 11) ^ (k << 25)) & 0x08004204;
            }

            uint[] q9mask = new uint[1 << 10];
            for (uint k = 0; k < q9mask.Length; ++k)
            {
                q9mask[k] = ((k << 1) ^ (k << 2) ^ (k << 3) ^ (k << 7) ^ (k << 12) ^ (k << 15) ^ (k << 18) ^ (k << 20)) & 0x2471042a;
            }

            while (true)
            {
                if (IsStopped)
                {
                    return;
                }

                uint aa1 = Q[Qoff] & 0x80000000;

                Q[Qoff + 2] = (xrng64() & 0x79b0c6ba) | 0x024c3841 | aa1;
                Q[Qoff + 3] = (xrng64() & 0x19300210) | 0x2603096d | (Q[Qoff + 2] & 0x80000082);
                Q[Qoff + 4] = (xrng64() & 0x10300000) | 0xe4cae30c | (Q[Qoff + 3] & 0x01000030);
                Q[Qoff + 5] = (xrng64() & 0x10000000) | 0x63494061 | (Q[Qoff + 4] & 0x00300000);
                Q[Qoff + 6] = 0x7deaff68;
                Q[Qoff + 7] = (xrng64() & 0x20444000) | 0x09091ee0;
                Q[Qoff + 8] = (xrng64() & 0x09040000) | 0xb2529f6d;
                Q[Qoff + 9] = (xrng64() & 0x00040000) | 0x10885184;
                Q[Qoff + 10] = (xrng64() & 0x00000080) | 0x428afb11 | (Q[Qoff + 9] & 0x00040000);
                Q[Qoff + 11] = (xrng64() & 0x128a8110) | 0x6571266b | (Q[Qoff + 10] & 0x0000080);
                Q[Qoff + 12] = (xrng64() & 0x3ef38d7f) | 0x00003080 | (~Q[Qoff + 11] & 0x00080000);
                Q[Qoff + 13] = (xrng64() & 0x3efb1d77) | 0x0004c008;
                Q[Qoff + 14] = (xrng64() & 0x5fff5d77) | 0x8000a288;
                Q[Qoff + 15] = (xrng64() & 0x1efe7ff7) | 0xe0008000 | (~Q[Qoff + 14] & 0x00010000);
                Q[Qoff + 16] = (xrng64() & 0x5ffdffff) | 0x20000000 | (~Q[Qoff + 15] & 0x00020000);

                MD5_REVERSE_STEP(block, Q, 5, 0x4787c62a, 12);
                MD5_REVERSE_STEP(block, Q, 6, 0xa8304613, 17);
                MD5_REVERSE_STEP(block, Q, 7, 0xfd469501, 22);
                MD5_REVERSE_STEP(block, Q, 11, 0x895cd7be, 22);
                MD5_REVERSE_STEP(block, Q, 14, 0xa679438e, 17);
                MD5_REVERSE_STEP(block, Q, 15, 0x49b40821, 22);

                uint tt17 = GG(Q[Qoff + 16], Q[Qoff + 15], Q[Qoff + 14]) + Q[Qoff + 13] + 0xf61e2562;
                uint tt18 = Q[Qoff + 14] + 0xc040b340 + block[6];
                uint tt19 = Q[Qoff + 15] + 0x265e5a51 + block[11];

                uint tt0 = FF(Q[Qoff + 0], Q[Qoff - 1], Q[Qoff - 2]) + Q[Qoff - 3] + 0xd76aa478;
                uint tt1 = Q[Qoff - 2] + 0xe8c7b756;

                uint q1a = 0x02000941 ^ (Q[Qoff + 0] & 0x80000000);

                uint counter = 0;
                while (counter < (1 << 12))
                {
                    if (IsStopped)
                    {
                        return;
                    }

                    ++counter;

                    uint q1 = q1a | (xrng64() & 0x7dfdf6be);
                    uint m1 = Q[Qoff + 2] - q1;
                    m1 = RR(m1, 12) - FF(q1, Q[Qoff + 0], Q[Qoff - 1]) - tt1;

                    uint q16 = Q[Qoff + 16];
                    uint q17 = tt17 + m1;
                    q17 = RL(q17, 5) + q16;
                    if (0x80000000 != ((q17 ^ q16) & 0x80008008)) { LogReturn(1); continue; }
                    if (0 != (q17 & 0x00020000)) { LogReturn(2); continue; }

                    uint q18 = GG(q17, q16, Q[Qoff + 15]) + tt18;
                    q18 = RL(q18, 9); q18 += q17;
                    if (0x80020000 != ((q18 ^ q17) & 0xa0020000)) { LogReturn(3); continue; }

                    uint q19 = GG(q18, q17, q16) + tt19;
                    q19 = RL(q19, 14); q19 += q18;
                    if (0 != (q19 & 0x80020000)) { LogReturn(4); continue; }

                    uint m0 = q1 - Q[Qoff + 0];
                    m0 = RR(m0, 7) - tt0;

                    uint q20 = GG(q19, q18, q17) + q16 + 0xe9b6c7aa + m0;
                    q20 = RL(q20, 20); q20 += q19;
                    if (0x00040000 != ((q20 ^ q19) & 0x80040000)) { LogReturn(5); continue; }

                    Q[Qoff + 1] = q1;
                    Q[Qoff + 17] = q17;
                    Q[Qoff + 18] = q18;
                    Q[Qoff + 19] = q19;
                    Q[Qoff + 20] = q20;

                    block[0] = m0;
                    block[1] = m1;

                    MD5_REVERSE_STEP(block, Q, 5, 0x4787c62a, 12);
                    uint q21 = GG(Q[Qoff + 20], Q[Qoff + 19], Q[Qoff + 18]) + Q[Qoff + 17] + 0xd62f105d + block[5];
                    q21 = RL(q21, 5); q21 += Q[Qoff + 20];
                    if (0 != ((q21 ^ Q[Qoff + 20]) & 0x80020000)) { LogReturn(6); continue; }
                    Q[Qoff + 21] = q21;

                    counter = 0;
                    LogReturn(7);
                    break;
                }
                if (counter != 0)
                { LogReturn(8); continue; }

                uint q9b = Q[Qoff + 9];
                uint q10b = Q[Qoff + 10];

                MD5_REVERSE_STEP(block, Q, 2, 0x242070db, 17);
                MD5_REVERSE_STEP(block, Q, 3, 0xc1bdceee, 22);
                MD5_REVERSE_STEP(block, Q, 4, 0xf57c0faf, 7);
                MD5_REVERSE_STEP(block, Q, 7, 0xfd469501, 22);

                uint tt10 = Q[Qoff + 7] + 0xffff5bb1;
                uint tt22 = GG(Q[Qoff + 21], Q[Qoff + 20], Q[Qoff + 19]) + Q[Qoff + 18] + 0x02441453;
                uint tt23 = Q[Qoff + 19] + 0xd8a1e681 + block[15];
                uint tt24 = Q[Qoff + 20] + 0xe7d3fbc8 + block[4];

                for (uint k10 = 0; k10 < (1 << 4); ++k10)
                {
                    if (IsStopped)
                    {
                        return;
                    }

                    uint q10 = q10b | (q9q10mask[k10] & 0x08000004);
                    uint m10 = RR(Q[Qoff + 11] - q10, 17);
                    uint q9 = q9b | (q9q10mask[k10] & 0x00004200);

                    m10 -= FF(q10, q9, Q[Qoff + 8]) + tt10;

                    uint aa = Q[Qoff + 21];
                    uint dd = tt22 + m10; dd = RL(dd, 9) + aa;
                    if (0 != (dd & 0x80000000)) { LogReturn(9); continue; }

                    uint bb = Q[Qoff + 20];
                    uint cc = tt23 + GG(dd, aa, bb);
                    if (0 != (cc & 0x20000)) { LogReturn(10); continue; }
                    cc = RL(cc, 14) + dd;
                    if (0 != (cc & 0x80000000)) { LogReturn(11); continue; }

                    bb = tt24 + GG(cc, dd, aa); bb = RL(bb, 20) + cc;
                    if (0 == (bb & 0x80000000)) { LogReturn(12); continue; }

                    block[10] = m10;
                    Q[Qoff + 9] = q9;
                    Q[Qoff + 10] = q10;
                    MD5_REVERSE_STEP(block, Q, 13, 0xfd987193, 12);

                    for (uint k9 = 0; k9 < (1 << 10); ++k9)
                    {
                        if (IsStopped)
                        {
                            return;
                        }

                        uint a = aa, b = bb, c = cc, d = dd;
                        Q[Qoff + 9] = q9 ^ q9mask[k9];
                        MD5_REVERSE_STEP(block, Q, 8, 0x698098d8, 7);
                        MD5_REVERSE_STEP(block, Q, 9, 0x8b44f7af, 12);
                        MD5_REVERSE_STEP(block, Q, 12, 0x6b901122, 7);

                        MD5_STEP_GG(ref a, b, c, d, block[9], 0x21e1cde6, 5);
                        MD5_STEP_GG(ref d, a, b, c, block[14], 0xc33707d6, 9);
                        MD5_STEP_GG(ref c, d, a, b, block[3], 0xf4d50d87, 14);
                        MD5_STEP_GG(ref b, c, d, a, block[8], 0x455a14ed, 20);
                        MD5_STEP_GG(ref a, b, c, d, block[13], 0xa9e3e905, 5);
                        MD5_STEP_GG(ref d, a, b, c, block[2], 0xfcefa3f8, 9);
                        MD5_STEP_GG(ref c, d, a, b, block[7], 0x676f02d9, 14);
                        MD5_STEP_GG(ref b, c, d, a, block[12], 0x8d2a4c8a, 20);
                        MD5_STEP_HH(ref a, b, c, d, block[5], 0xfffa3942, 4);
                        MD5_STEP_HH(ref d, a, b, c, block[8], 0x8771f681, 11);

                        c += HH(d, a, b) + block[11] + 0x6d9d6122;
                        if (0 != (c & (1 << 15)))
                        { LogReturn(13); continue; }
                        c = (c << 16 | c >> 16) + d;

                        MD5_STEP_HH(ref b, c, d, a, block[14], 0xfde5380c, 23);
                        MD5_STEP_HH(ref a, b, c, d, block[1], 0xa4beea44, 4);
                        MD5_STEP_HH(ref d, a, b, c, block[4], 0x4bdecfa9, 11);
                        MD5_STEP_HH(ref c, d, a, b, block[7], 0xf6bb4b60, 16);
                        MD5_STEP_HH(ref b, c, d, a, block[10], 0xbebfbc70, 23);
                        MD5_STEP_HH(ref a, b, c, d, block[13], 0x289b7ec6, 4);
                        MD5_STEP_HH(ref d, a, b, c, block[0], 0xeaa127fa, 11);
                        MD5_STEP_HH(ref c, d, a, b, block[3], 0xd4ef3085, 16);
                        MD5_STEP_HH(ref b, c, d, a, block[6], 0x04881d05, 23);
                        MD5_STEP_HH(ref a, b, c, d, block[9], 0xd9d4d039, 4);
                        MD5_STEP_HH(ref d, a, b, c, block[12], 0xe6db99e5, 11);
                        MD5_STEP_HH(ref c, d, a, b, block[15], 0x1fa27cf8, 16);
                        MD5_STEP_HH(ref b, c, d, a, block[2], 0xc4ac5665, 23);
                        if (0 != ((b ^ d) & 0x80000000))
                        { LogReturn(14); continue; }

                        MD5_STEP_II(ref a, b, c, d, block[0], 0xf4292244, 6);
                        if (0 != ((a ^ c) >> 31)) { LogReturn(15); continue; }
                        MD5_STEP_II(ref d, a, b, c, block[7], 0x432aff97, 10);
                        if (0 == ((b ^ d) >> 31)) { LogReturn(16); continue; }
                        MD5_STEP_II(ref c, d, a, b, block[14], 0xab9423a7, 15);
                        if (0 != ((a ^ c) >> 31)) { LogReturn(17); continue; }
                        MD5_STEP_II(ref b, c, d, a, block[5], 0xfc93a039, 21);
                        if (0 != ((b ^ d) >> 31)) { LogReturn(18); continue; }
                        MD5_STEP_II(ref a, b, c, d, block[12], 0x655b59c3, 6);
                        if (0 != ((a ^ c) >> 31)) { LogReturn(19); continue; }
                        MD5_STEP_II(ref d, a, b, c, block[3], 0x8f0ccc92, 10);
                        if (0 != ((b ^ d) >> 31)) { LogReturn(20); continue; }
                        MD5_STEP_II(ref c, d, a, b, block[10], 0xffeff47d, 15);
                        if (0 != ((a ^ c) >> 31)) { LogReturn(21); continue; }
                        MD5_STEP_II(ref b, c, d, a, block[1], 0x85845dd1, 21);
                        if (0 != ((b ^ d) >> 31)) { LogReturn(22); continue; }
                        MD5_STEP_II(ref a, b, c, d, block[8], 0x6fa87e4f, 6);
                        if (0 != ((a ^ c) >> 31)) { LogReturn(23); continue; }
                        MD5_STEP_II(ref d, a, b, c, block[15], 0xfe2ce6e0, 10);
                        if (0 != ((b ^ d) >> 31)) { LogReturn(24); continue; }
                        MD5_STEP_II(ref c, d, a, b, block[6], 0xa3014314, 15);
                        if (0 != ((a ^ c) >> 31)) { LogReturn(25); continue; }
                        MD5_STEP_II(ref b, c, d, a, block[13], 0x4e0811a1, 21);
                        if (0 == ((b ^ d) >> 31)) { LogReturn(26); continue; }
                        MD5_STEP_II(ref a, b, c, d, block[4], 0xf7537e82, 6);
                        if (0 != ((a ^ c) >> 31)) { LogReturn(27); continue; }
                        MD5_STEP_II(ref d, a, b, c, block[11], 0xbd3af235, 10);
                        if (0 != ((b ^ d) >> 31)) { LogReturn(28); continue; }
                        MD5_STEP_II(ref c, d, a, b, block[2], 0x2ad7d2bb, 15);
                        if (0 != ((a ^ c) >> 31)) { LogReturn(29); continue; }
                        MD5_STEP_II(ref b, c, d, a, block[9], 0xeb86d391, 21);

                        //std::cout << "." << std::flush;

                        uint[] block2 = new uint[16];
                        uint[] IV1 = new uint[4], IV2 = new uint[4];
                        for (int t = 0; t < 4; ++t)
                        {
                            IV1[t] = IV[t];
                            IV2[t] = IV[t] + ((uint)1 << 31);
                        }
                        IV2[1] -= (1 << 25);
                        IV2[2] -= (1 << 25);
                        IV2[3] -= (1 << 25);

                        for (int t = 0; t < 16; ++t)
                        {
                            block2[t] = block[t];
                        }

                        block2[4] += (uint)1 << 31;
                        block2[11] += 1 << 15;
                        block2[14] += (uint)1 << 31;

                        md5_compress(IV1, block);
                        md5_compress(IV2, block2);
                        if (IV2[0] == IV1[0] && IV2[1] == IV1[1] && IV2[2] == IV1[2] && IV2[3] == IV1[3])
                        {
                            LogReturn(30);
                            return;
                        }

                        //if (IV2[0] != IV1[0])
                        //		std::cout << "!" << std::flush;
                    }
                }
            }
        }

        private void find_block1_stevens_11(uint[] block, uint[] IV)
        {
            MatchProgressMax = 30;
            MatchProgress = 0;

            uint[] Q = new uint[68];
            Q[0] = IV[0];
            Q[1] = IV[3];
            Q[2] = IV[2];
            Q[3] = IV[1];

            uint[] q9q10mask = new uint[1 << 5];
            for (uint k = 0; k < q9q10mask.Length; ++k)
            {
                q9q10mask[k] = ((k << 5) ^ (k << 6) ^ (k << 7) ^ (k << 24) ^ (k << 27)) & 0x880002a0;
            }

            uint[] q9mask = new uint[1 << 9];
            for (uint k = 0; k < q9mask.Length; ++k)
            {
                q9mask[k] = ((k << 1) ^ (k << 3) ^ (k << 8) ^ (k << 12) ^ (k << 15) ^ (k << 18)) & 0x04710c12;
            }

            while (true)
            {
                if (IsStopped)
                {
                    return;
                }

                uint aa1 = Q[Qoff] & 0x80000000;

                Q[Qoff + 2] = (xrng64() & 0x75bef63e) | 0x0a410041 | aa1;
                Q[Qoff + 3] = (xrng64() & 0x10345614) | 0x0202a9e1 | (Q[Qoff + 2] & 0x84000002);
                Q[Qoff + 4] = (xrng64() & 0x00145400) | 0xe84ba909 | (Q[Qoff + 3] & 0x00000014);
                Q[Qoff + 5] = (xrng64() & 0x80000000) | 0x75e90b1d | (Q[Qoff + 4] & 0x00145400);
                Q[Qoff + 6] = 0x7c23ff5a | (Q[Qoff + 5] & 0x80000000);
                Q[Qoff + 7] = (xrng64() & 0x40000880) | 0x114bf41a;
                Q[Qoff + 8] = (xrng64() & 0x00002090) | 0xb352dd01;
                Q[Qoff + 9] = (xrng64() & 0x00044000) | 0x7a803124;
                Q[Qoff + 10] = (xrng64() & 0x00002000) | 0xf28a92c9 | (Q[Qoff + 9] & 0x00044000);
                Q[Qoff + 11] = (xrng64() & 0x128a8108) | 0xc5710ed7 | (Q[Qoff + 10] & 0x00002000);
                Q[Qoff + 12] = (xrng64() & 0x9edb8d7f) | 0x20003080 | (~Q[Qoff + 11] & 0x00200000);
                Q[Qoff + 13] = (xrng64() & 0x3efb1d77) | 0x4004c008 | (Q[Qoff + 12] & 0x80000000);
                Q[Qoff + 14] = (xrng64() & 0x1fff5d77) | 0x0000a288;
                Q[Qoff + 15] = (xrng64() & 0x1efe7ff7) | 0x20008000 | (~Q[Qoff + 14] & 0x00010000);
                Q[Qoff + 16] = (xrng64() & 0x1ffdffff) | 0x20000000 | (~Q[Qoff + 15] & 0x40020000);

                MD5_REVERSE_STEP(block, Q, 5, 0x4787c62a, 12);
                MD5_REVERSE_STEP(block, Q, 6, 0xa8304613, 17);
                MD5_REVERSE_STEP(block, Q, 7, 0xfd469501, 22);
                MD5_REVERSE_STEP(block, Q, 11, 0x895cd7be, 22);
                MD5_REVERSE_STEP(block, Q, 14, 0xa679438e, 17);
                MD5_REVERSE_STEP(block, Q, 15, 0x49b40821, 22);

                uint tt17 = GG(Q[Qoff + 16], Q[Qoff + 15], Q[Qoff + 14]) + Q[Qoff + 13] + 0xf61e2562;
                uint tt18 = Q[Qoff + 14] + 0xc040b340 + block[6];
                uint tt19 = Q[Qoff + 15] + 0x265e5a51 + block[11];

                uint tt0 = FF(Q[Qoff + 0], Q[Qoff - 1], Q[Qoff - 2]) + Q[Qoff - 3] + 0xd76aa478;
                uint tt1 = Q[Qoff - 2] + 0xe8c7b756;

                uint q1a = 0x02000861 ^ (Q[Qoff + 0] & 0x80000020);

                uint counter = 0;
                while (counter < (1 << 12))
                {
                    if (IsStopped)
                    {
                        return;
                    }

                    ++counter;

                    uint q1 = q1a | (xrng64() & 0x7dfff79e);
                    uint m1 = Q[Qoff + 2] - q1;
                    m1 = RR(m1, 12) - FF(q1, Q[Qoff + 0], Q[Qoff - 1]) - tt1;

                    uint q16 = Q[Qoff + 16];
                    uint q17 = tt17 + m1;
                    q17 = RL(q17, 5) + q16;
                    if (0x40000000 != ((q17 ^ q16) & 0xc0008008)) { LogReturn(1); continue; }
                    if (0 != (q17 & 0x00020000)) { LogReturn(2); continue; }

                    uint q18 = GG(q17, q16, Q[Qoff + 15]) + tt18;
                    q18 = RL(q18, 9); q18 += q17;
                    if (0x80020000 != ((q18 ^ q17) & 0xa0020000)) { LogReturn(3); continue; }

                    uint q19 = GG(q18, q17, q16) + tt19;
                    q19 = RL(q19, 14); q19 += q18;
                    if (0x80000000 != (q19 & 0x80020000)) { LogReturn(4); continue; }

                    uint m0 = q1 - Q[Qoff + 0];
                    m0 = RR(m0, 7) - tt0;

                    uint q20 = GG(q19, q18, q17) + q16 + 0xe9b6c7aa + m0;
                    q20 = RL(q20, 20); q20 += q19;
                    if (0x00040000 != ((q20 ^ q19) & 0x80040000)) { LogReturn(5); continue; }

                    Q[Qoff + 1] = q1;
                    Q[Qoff + 17] = q17;
                    Q[Qoff + 18] = q18;
                    Q[Qoff + 19] = q19;
                    Q[Qoff + 20] = q20;

                    block[0] = m0;
                    block[1] = m1;

                    MD5_REVERSE_STEP(block, Q, 5, 0x4787c62a, 12);
                    uint q21 = GG(Q[Qoff + 20], Q[Qoff + 19], Q[Qoff + 18]) + Q[Qoff + 17] + 0xd62f105d + block[5];
                    q21 = RL(q21, 5); q21 += Q[Qoff + 20];
                    if (0 != ((q21 ^ Q[Qoff + 20]) & 0x80020000)) { LogReturn(6); continue; }

                    Q[Qoff + 21] = q21;

                    counter = 0;
                    LogReturn(7);
                    break;
                }
                if (counter != 0)
                { LogReturn(8); continue; }

                uint q9b = Q[Qoff + 9];
                uint q10b = Q[Qoff + 10];

                MD5_REVERSE_STEP(block, Q, 2, 0x242070db, 17);
                MD5_REVERSE_STEP(block, Q, 3, 0xc1bdceee, 22);
                MD5_REVERSE_STEP(block, Q, 4, 0xf57c0faf, 7);
                MD5_REVERSE_STEP(block, Q, 7, 0xfd469501, 22);

                uint tt10 = Q[Qoff + 7] + 0xffff5bb1;
                uint tt22 = GG(Q[Qoff + 21], Q[Qoff + 20], Q[Qoff + 19]) + Q[Qoff + 18] + 0x02441453;
                uint tt23 = Q[Qoff + 19] + 0xd8a1e681 + block[15];
                uint tt24 = Q[Qoff + 20] + 0xe7d3fbc8 + block[4];

                for (uint k10 = 0; k10 < (1 << 5); ++k10)
                {
                    if (IsStopped)
                    {
                        return;
                    }

                    uint q10 = q10b | (q9q10mask[k10] & 0x08000040);
                    uint m10 = RR(Q[Qoff + 11] - q10, 17);
                    uint q9 = q9b | (q9q10mask[k10] & 0x80000280);

                    m10 -= FF(q10, q9, Q[Qoff + 8]) + tt10;

                    uint aa = Q[Qoff + 21];
                    uint dd = tt22 + m10; dd = RL(dd, 9) + aa;
                    if (0 == (dd & 0x80000000)) { LogReturn(9); continue; }

                    uint bb = Q[Qoff + 20];
                    uint cc = tt23 + GG(dd, aa, bb);
                    if (0 != (cc & 0x20000)) { LogReturn(10); continue; }
                    cc = RL(cc, 14) + dd;
                    if (0 != (cc & 0x80000000)) { LogReturn(11); continue; }

                    bb = tt24 + GG(cc, dd, aa); bb = RL(bb, 20) + cc;
                    if (0 == (bb & 0x80000000)) { LogReturn(12); continue; }

                    block[10] = m10;
                    Q[Qoff + 9] = q9;
                    Q[Qoff + 10] = q10;
                    MD5_REVERSE_STEP(block, Q, 13, 0xfd987193, 12);

                    for (uint k9 = 0; k9 < (1 << 9); ++k9)
                    {
                        if (IsStopped)
                        {
                            return;
                        }

                        uint a = aa, b = bb, c = cc, d = dd;
                        Q[Qoff + 9] = q9 ^ q9mask[k9];
                        MD5_REVERSE_STEP(block, Q, 8, 0x698098d8, 7);
                        MD5_REVERSE_STEP(block, Q, 9, 0x8b44f7af, 12);
                        MD5_REVERSE_STEP(block, Q, 12, 0x6b901122, 7);

                        MD5_STEP_GG(ref a, b, c, d, block[9], 0x21e1cde6, 5);
                        MD5_STEP_GG(ref d, a, b, c, block[14], 0xc33707d6, 9);
                        MD5_STEP_GG(ref c, d, a, b, block[3], 0xf4d50d87, 14);
                        MD5_STEP_GG(ref b, c, d, a, block[8], 0x455a14ed, 20);
                        MD5_STEP_GG(ref a, b, c, d, block[13], 0xa9e3e905, 5);
                        MD5_STEP_GG(ref d, a, b, c, block[2], 0xfcefa3f8, 9);
                        MD5_STEP_GG(ref c, d, a, b, block[7], 0x676f02d9, 14);
                        MD5_STEP_GG(ref b, c, d, a, block[12], 0x8d2a4c8a, 20);
                        MD5_STEP_HH(ref a, b, c, d, block[5], 0xfffa3942, 4);
                        MD5_STEP_HH(ref d, a, b, c, block[8], 0x8771f681, 11);

                        c += HH(d, a, b) + block[11] + 0x6d9d6122;
                        if (0 != (c & (1 << 15)))
                        { LogReturn(13); continue; }
                        c = (c << 16 | c >> 16) + d;

                        MD5_STEP_HH(ref b, c, d, a, block[14], 0xfde5380c, 23);
                        MD5_STEP_HH(ref a, b, c, d, block[1], 0xa4beea44, 4);
                        MD5_STEP_HH(ref d, a, b, c, block[4], 0x4bdecfa9, 11);
                        MD5_STEP_HH(ref c, d, a, b, block[7], 0xf6bb4b60, 16);
                        MD5_STEP_HH(ref b, c, d, a, block[10], 0xbebfbc70, 23);
                        MD5_STEP_HH(ref a, b, c, d, block[13], 0x289b7ec6, 4);
                        MD5_STEP_HH(ref d, a, b, c, block[0], 0xeaa127fa, 11);
                        MD5_STEP_HH(ref c, d, a, b, block[3], 0xd4ef3085, 16);
                        MD5_STEP_HH(ref b, c, d, a, block[6], 0x04881d05, 23);
                        MD5_STEP_HH(ref a, b, c, d, block[9], 0xd9d4d039, 4);
                        MD5_STEP_HH(ref d, a, b, c, block[12], 0xe6db99e5, 11);
                        MD5_STEP_HH(ref c, d, a, b, block[15], 0x1fa27cf8, 16);
                        MD5_STEP_HH(ref b, c, d, a, block[2], 0xc4ac5665, 23);
                        if (0 != ((b ^ d) & 0x80000000))
                        { LogReturn(14); continue; }

                        MD5_STEP_II(ref a, b, c, d, block[0], 0xf4292244, 6);
                        if (0 != ((a ^ c) >> 31)) { LogReturn(15); continue; }
                        MD5_STEP_II(ref d, a, b, c, block[7], 0x432aff97, 10);
                        if (0 == ((b ^ d) >> 31)) { LogReturn(16); continue; }
                        MD5_STEP_II(ref c, d, a, b, block[14], 0xab9423a7, 15);
                        if (0 != ((a ^ c) >> 31)) { LogReturn(17); continue; }
                        MD5_STEP_II(ref b, c, d, a, block[5], 0xfc93a039, 21);
                        if (0 != ((b ^ d) >> 31)) { LogReturn(18); continue; }
                        MD5_STEP_II(ref a, b, c, d, block[12], 0x655b59c3, 6);
                        if (0 != ((a ^ c) >> 31)) { LogReturn(19); continue; }
                        MD5_STEP_II(ref d, a, b, c, block[3], 0x8f0ccc92, 10);
                        if (0 != ((b ^ d) >> 31)) { LogReturn(20); continue; }
                        MD5_STEP_II(ref c, d, a, b, block[10], 0xffeff47d, 15);
                        if (0 != ((a ^ c) >> 31)) { LogReturn(21); continue; }
                        MD5_STEP_II(ref b, c, d, a, block[1], 0x85845dd1, 21);
                        if (0 != ((b ^ d) >> 31)) { LogReturn(22); continue; }
                        MD5_STEP_II(ref a, b, c, d, block[8], 0x6fa87e4f, 6);
                        if (0 != ((a ^ c) >> 31)) { LogReturn(23); continue; }
                        MD5_STEP_II(ref d, a, b, c, block[15], 0xfe2ce6e0, 10);
                        if (0 != ((b ^ d) >> 31)) { LogReturn(24); continue; }
                        MD5_STEP_II(ref c, d, a, b, block[6], 0xa3014314, 15);
                        if (0 != ((a ^ c) >> 31)) { LogReturn(25); continue; }
                        MD5_STEP_II(ref b, c, d, a, block[13], 0x4e0811a1, 21);
                        if (0 == ((b ^ d) >> 31)) { LogReturn(26); continue; }
                        MD5_STEP_II(ref a, b, c, d, block[4], 0xf7537e82, 6);
                        if (0 != ((a ^ c) >> 31)) { LogReturn(27); continue; }
                        MD5_STEP_II(ref d, a, b, c, block[11], 0xbd3af235, 10);
                        if (0 != ((b ^ d) >> 31)) { LogReturn(28); continue; }
                        MD5_STEP_II(ref c, d, a, b, block[2], 0x2ad7d2bb, 15);
                        if (0 != ((a ^ c) >> 31)) { LogReturn(29); continue; }
                        MD5_STEP_II(ref b, c, d, a, block[9], 0xeb86d391, 21);

                        //std::cout << "." << std::flush;

                        uint[] block2 = new uint[16];
                        uint[] IV1 = new uint[4], IV2 = new uint[4];
                        for (int t = 0; t < 4; ++t)
                        {
                            IV1[t] = IV[t];
                            IV2[t] = IV[t] + ((uint)1 << 31);
                        }
                        IV2[1] -= (1 << 25);
                        IV2[2] -= (1 << 25);
                        IV2[3] -= (1 << 25);

                        for (int t = 0; t < 16; ++t)
                        {
                            block2[t] = block[t];
                        }

                        block2[4] += (uint)1 << 31;
                        block2[11] += 1 << 15;
                        block2[14] += (uint)1 << 31;

                        md5_compress(IV1, block);
                        md5_compress(IV2, block2);
                        if (IV2[0] == IV1[0] && IV2[1] == IV1[1] && IV2[2] == IV1[2] && IV2[3] == IV1[3])
                        {
                            LogReturn(30);
                            return;
                        }

                        //if (IV2[0] != IV1[0])
                        //		std::cout << "!" << std::flush;
                    }
                }
            }
        }

        private void find_block1_wang(uint[] block, uint[] IV)
        {
            MatchProgressMax = 30;
            MatchProgress = 0;

            uint[] Q = new uint[68];
            Q[0] = IV[0];
            Q[1] = IV[3];
            Q[2] = IV[2];
            Q[3] = IV[1];

            uint[] q4mask = new uint[1 << 6];
            for (uint k = 0; k < q4mask.Length; ++k)
            {
                q4mask[k] = ((k << 13) ^ (k << 19)) & 0x01c0e000;
            }

            uint[] q9mask = new uint[1 << 5], q10mask = new uint[1 << 5];
            for (uint k = 0; k < q9mask.Length; ++k)
            {
                uint msk = (k << 5) ^ (k << 13) ^ (k << 17) ^ (k << 24);
                q9mask[k] = msk & 0x00084000;
                q10mask[k] = msk & 0x18000020;
            }

            uint[] q9mask2 = new uint[1 << 10];
            for (uint k = 0; k < q9mask2.Length; ++k)
            {
                q9mask2[k] = ((k << 1) ^ (k << 7) ^ (k << 14) ^ (k << 15) ^ (k << 22)) & 0x6074041c;
            }

            while (true)
            {
                if (IsStopped)
                {
                    return;
                }

                uint aa1 = Q[Qoff] & 0x80000000;
                uint bb1 = 0x80000000 ^ aa1;

                Q[Qoff + 2] = (xrng64() & 0x71de7799) | 0x0c008840 | bb1;
                Q[Qoff + 3] = (xrng64() & 0x01c06601) | 0x3e1f0966 | (Q[Qoff + 2] & 0x80000018);
                Q[Qoff + 4] = 0x3a040010 | (Q[Qoff + 3] & 0x80000601);
                Q[Qoff + 5] = (xrng64() & 0x03c0e000) | 0x482f0e50 | aa1;
                Q[Qoff + 6] = (xrng64() & 0x600c0000) | 0x05e2ec56 | aa1;
                Q[Qoff + 7] = (xrng64() & 0x604c203e) | 0x16819e01 | bb1 | (Q[Qoff + 6] & 0x01000000);
                Q[Qoff + 8] = (xrng64() & 0x604c7c1c) | 0x043283e0 | (Q[Qoff + 7] & 0x80000002);
                Q[Qoff + 9] = (xrng64() & 0x00002800) | 0x1c0101c1 | (Q[Qoff + 8] & 0x80001000);
                Q[Qoff + 10] = 0x078bcbc0 | bb1;
                Q[Qoff + 11] = (xrng64() & 0x07800000) | 0x607dc7df | bb1;
                Q[Qoff + 12] = (xrng64() & 0x00f00f7f) | 0x00081080 | (Q[Qoff + 11] & 0xe7000000);
                Q[Qoff + 13] = (xrng64() & 0x00701f77) | 0x3f0fe008 | aa1;
                Q[Qoff + 14] = (xrng64() & 0x00701f77) | 0x408be088 | aa1;
                Q[Qoff + 15] = (xrng64() & 0x00ff3ff7) | 0x7d000000;
                Q[Qoff + 16] = (xrng64() & 0x4ffdffff) | 0x20000000 | (~Q[Qoff + 15] & 0x00020000);

                MD5_REVERSE_STEP(block, Q, 5, 0x4787c62a, 12);
                MD5_REVERSE_STEP(block, Q, 6, 0xa8304613, 17);
                MD5_REVERSE_STEP(block, Q, 7, 0xfd469501, 22);
                MD5_REVERSE_STEP(block, Q, 11, 0x895cd7be, 22);
                MD5_REVERSE_STEP(block, Q, 14, 0xa679438e, 17);
                MD5_REVERSE_STEP(block, Q, 15, 0x49b40821, 22);

                uint tt17 = GG(Q[Qoff + 16], Q[Qoff + 15], Q[Qoff + 14]) + Q[Qoff + 13] + 0xf61e2562;
                uint tt18 = Q[Qoff + 14] + 0xc040b340 + block[6];
                uint tt19 = Q[Qoff + 15] + 0x265e5a51 + block[11];

                uint tt0 = FF(Q[Qoff + 0], Q[Qoff - 1], Q[Qoff - 2]) + Q[Qoff - 3] + 0xd76aa478;
                uint tt1 = Q[Qoff - 2] + 0xe8c7b756;

                uint q1a = 0x04200040 | (Q[Qoff + 2] & 0xf01e1080);

                uint counter = 0;
                while (counter < (1 << 12))
                {
                    if (IsStopped)
                    {
                        return;
                    }

                    ++counter;

                    uint q1 = q1a | (xrng64() & 0x01c0e71f);
                    uint m1 = Q[Qoff + 2] - q1;
                    m1 = RR(m1, 12) - FF(q1, Q[Qoff + 0], Q[Qoff - 1]) - tt1;

                    uint q16 = Q[Qoff + 16];
                    uint q17 = tt17 + m1;
                    q17 = RL(q17, 5) + q16;
                    if (0x40000000 != ((q17 ^ q16) & 0xc0008008)) { LogReturn(1); continue; }
                    if (0 != (q17 & 0x00020000)) { LogReturn(2); continue; }

                    uint q18 = GG(q17, q16, Q[Qoff + 15]) + tt18;
                    q18 = RL(q18, 9); q18 += q17;
                    if (0x00020000 != ((q18 ^ q17) & 0xa0020000)) { LogReturn(3); continue; }

                    uint q19 = GG(q18, q17, q16) + tt19;
                    q19 = RL(q19, 14); q19 += q18;
                    if (0 != (q19 & 0x80020000)) { LogReturn(4); continue; }

                    uint m0 = q1 - Q[Qoff + 0];
                    m0 = RR(m0, 7) - tt0;

                    uint q20 = GG(q19, q18, q17) + q16 + 0xe9b6c7aa + m0;
                    q20 = RL(q20, 20); q20 += q19;
                    if (0x00040000 != ((q20 ^ q19) & 0x80040000)) { LogReturn(5); continue; }

                    Q[Qoff + 1] = q1;
                    Q[Qoff + 17] = q17;
                    Q[Qoff + 18] = q18;
                    Q[Qoff + 19] = q19;
                    Q[Qoff + 20] = q20;

                    block[0] = m0;
                    block[1] = m1;
                    MD5_REVERSE_STEP(block, Q, 2, 0x242070db, 17);

                    counter = 0;
                    LogReturn(6);
                    break;
                }
                if (counter != 0)
                { LogReturn(7); continue; }

                uint q4b = Q[Qoff + 4];
                uint q9b = Q[Qoff + 9];
                uint q10b = Q[Qoff + 10];
                uint tt21 = GG(Q[Qoff + 20], Q[Qoff + 19], Q[Qoff + 18]) + Q[Qoff + 17] + 0xd62f105d;

                counter = 0;
                while (counter < (1 << 6))
                {
                    if (IsStopped)
                    {
                        return;
                    }

                    Q[Qoff + 4] = q4b ^ q4mask[counter];
                    ++counter;
                    MD5_REVERSE_STEP(block, Q, 5, 0x4787c62a, 12);
                    uint q21 = tt21 + block[5];
                    q21 = RL(q21, 5); q21 += Q[Qoff + 20];
                    if (0 != ((q21 ^ Q[Qoff + 20]) & 0x80020000)) { LogReturn(8); continue; }

                    Q[Qoff + 21] = q21;
                    MD5_REVERSE_STEP(block, Q, 3, 0xc1bdceee, 22);
                    MD5_REVERSE_STEP(block, Q, 4, 0xf57c0faf, 7);
                    MD5_REVERSE_STEP(block, Q, 7, 0xfd469501, 22);

                    uint tt10 = Q[Qoff + 7] + 0xffff5bb1;
                    uint tt22 = GG(Q[Qoff + 21], Q[Qoff + 20], Q[Qoff + 19]) + Q[Qoff + 18] + 0x02441453;
                    uint tt23 = Q[Qoff + 19] + 0xd8a1e681 + block[15];
                    uint tt24 = Q[Qoff + 20] + 0xe7d3fbc8 + block[4];

                    uint counter2 = 0;
                    while (counter2 < (1 << 5))
                    {
                        if (IsStopped)
                        {
                            return;
                        }

                        uint q10 = q10b ^ q10mask[counter2];
                        uint m10 = RR(Q[Qoff + 11] - q10, 17);
                        uint q9 = q9b ^ q9mask[counter2];
                        ++counter2;

                        m10 -= FF(q10, q9, Q[Qoff + 8]) + tt10;

                        uint aa = Q[Qoff + 21];
                        uint dd = tt22 + m10; dd = RL(dd, 9) + aa;
                        if (0 != (dd & 0x80000000)) { LogReturn(9); continue; }

                        uint bb = Q[Qoff + 20];
                        uint cc = tt23 + GG(dd, aa, bb);
                        if (0 != (cc & 0x20000)) { LogReturn(10); continue; }
                        cc = RL(cc, 14) + dd;
                        if (0 != (cc & 0x80000000)) { LogReturn(11); continue; }

                        bb = tt24 + GG(cc, dd, aa); bb = RL(bb, 20) + cc;
                        if (0 == (bb & 0x80000000)) { LogReturn(12); continue; }

                        block[10] = m10;
                        Q[Qoff + 9] = q9;
                        Q[Qoff + 10] = q10;
                        MD5_REVERSE_STEP(block, Q, 13, 0xfd987193, 12);

                        for (uint k9 = 0; k9 < (1 << 10);)
                        {
                            if (IsStopped)
                            {
                                return;
                            }

                            uint a = aa, b = bb, c = cc, d = dd;
                            Q[Qoff + 9] = q9 ^ q9mask2[k9]; ++k9;
                            MD5_REVERSE_STEP(block, Q, 8, 0x698098d8, 7);
                            MD5_REVERSE_STEP(block, Q, 9, 0x8b44f7af, 12);
                            MD5_REVERSE_STEP(block, Q, 12, 0x6b901122, 7);

                            MD5_STEP_GG(ref a, b, c, d, block[9], 0x21e1cde6, 5);
                            MD5_STEP_GG(ref d, a, b, c, block[14], 0xc33707d6, 9);
                            MD5_STEP_GG(ref c, d, a, b, block[3], 0xf4d50d87, 14);
                            MD5_STEP_GG(ref b, c, d, a, block[8], 0x455a14ed, 20);
                            MD5_STEP_GG(ref a, b, c, d, block[13], 0xa9e3e905, 5);
                            MD5_STEP_GG(ref d, a, b, c, block[2], 0xfcefa3f8, 9);
                            MD5_STEP_GG(ref c, d, a, b, block[7], 0x676f02d9, 14);
                            MD5_STEP_GG(ref b, c, d, a, block[12], 0x8d2a4c8a, 20);
                            MD5_STEP_HH(ref a, b, c, d, block[5], 0xfffa3942, 4);
                            MD5_STEP_HH(ref d, a, b, c, block[8], 0x8771f681, 11);

                            c += HH(d, a, b) + block[11] + 0x6d9d6122;
                            if (0 == (c & (1 << 15)))
                            { LogReturn(13); continue; }
                            c = (c << 16 | c >> 16) + d;

                            MD5_STEP_HH(ref b, c, d, a, block[14], 0xfde5380c, 23);
                            MD5_STEP_HH(ref a, b, c, d, block[1], 0xa4beea44, 4);
                            MD5_STEP_HH(ref d, a, b, c, block[4], 0x4bdecfa9, 11);
                            MD5_STEP_HH(ref c, d, a, b, block[7], 0xf6bb4b60, 16);
                            MD5_STEP_HH(ref b, c, d, a, block[10], 0xbebfbc70, 23);
                            MD5_STEP_HH(ref a, b, c, d, block[13], 0x289b7ec6, 4);
                            MD5_STEP_HH(ref d, a, b, c, block[0], 0xeaa127fa, 11);
                            MD5_STEP_HH(ref c, d, a, b, block[3], 0xd4ef3085, 16);
                            MD5_STEP_HH(ref b, c, d, a, block[6], 0x04881d05, 23);
                            MD5_STEP_HH(ref a, b, c, d, block[9], 0xd9d4d039, 4);
                            MD5_STEP_HH(ref d, a, b, c, block[12], 0xe6db99e5, 11);
                            MD5_STEP_HH(ref c, d, a, b, block[15], 0x1fa27cf8, 16);
                            MD5_STEP_HH(ref b, c, d, a, block[2], 0xc4ac5665, 23);
                            if (0 != ((b ^ d) & 0x80000000))
                            { LogReturn(14); continue; }

                            MD5_STEP_II(ref a, b, c, d, block[0], 0xf4292244, 6);
                            if (0 != ((a ^ c) >> 31)) { LogReturn(15); continue; }
                            MD5_STEP_II(ref d, a, b, c, block[7], 0x432aff97, 10);
                            if (0 == ((b ^ d) >> 31)) { LogReturn(16); continue; }
                            MD5_STEP_II(ref c, d, a, b, block[14], 0xab9423a7, 15);
                            if (0 != ((a ^ c) >> 31)) { LogReturn(17); continue; }
                            MD5_STEP_II(ref b, c, d, a, block[5], 0xfc93a039, 21);
                            if (0 != ((b ^ d) >> 31)) { LogReturn(18); continue; }
                            MD5_STEP_II(ref a, b, c, d, block[12], 0x655b59c3, 6);
                            if (0 != ((a ^ c) >> 31)) { LogReturn(19); continue; }
                            MD5_STEP_II(ref d, a, b, c, block[3], 0x8f0ccc92, 10);
                            if (0 != ((b ^ d) >> 31)) { LogReturn(20); continue; }
                            MD5_STEP_II(ref c, d, a, b, block[10], 0xffeff47d, 15);
                            if (0 != ((a ^ c) >> 31)) { LogReturn(21); continue; }
                            MD5_STEP_II(ref b, c, d, a, block[1], 0x85845dd1, 21);
                            if (0 != ((b ^ d) >> 31)) { LogReturn(22); continue; }
                            MD5_STEP_II(ref a, b, c, d, block[8], 0x6fa87e4f, 6);
                            if (0 != ((a ^ c) >> 31)) { LogReturn(23); continue; }
                            MD5_STEP_II(ref d, a, b, c, block[15], 0xfe2ce6e0, 10);
                            if (0 != ((b ^ d) >> 31)) { LogReturn(24); continue; }
                            MD5_STEP_II(ref c, d, a, b, block[6], 0xa3014314, 15);
                            if (0 != ((a ^ c) >> 31)) { LogReturn(25); continue; }
                            MD5_STEP_II(ref b, c, d, a, block[13], 0x4e0811a1, 21);
                            if (0 == ((b ^ d) >> 31)) { LogReturn(26); continue; }
                            MD5_STEP_II(ref a, b, c, d, block[4], 0xf7537e82, 6);
                            if (0 != ((a ^ c) >> 31)) { LogReturn(27); continue; }
                            MD5_STEP_II(ref d, a, b, c, block[11], 0xbd3af235, 10);
                            if (0 != ((b ^ d) >> 31)) { LogReturn(28); continue; }
                            MD5_STEP_II(ref c, d, a, b, block[2], 0x2ad7d2bb, 15);
                            if (0 != ((a ^ c) >> 31)) { LogReturn(29); continue; }
                            MD5_STEP_II(ref b, c, d, a, block[9], 0xeb86d391, 21);

                            //std::cout << "." << std::flush;

                            uint[] block2 = new uint[16];
                            uint[] IV1 = new uint[4], IV2 = new uint[4];
                            for (int t = 0; t < 4; ++t)
                            {
                                IV1[t] = IV[t];
                                IV2[t] = IV[t] + ((uint)1 << 31);
                            }
                            IV2[1] += (1 << 25);
                            IV2[2] += (1 << 25);
                            IV2[3] += (1 << 25);

                            for (int t = 0; t < 16; ++t)
                            {
                                block2[t] = block[t];
                            }

                            block2[4] += (uint)1 << 31;
                            block2[11] -= 1 << 15;
                            block2[14] += (uint)1 << 31;

                            md5_compress(IV1, block);
                            md5_compress(IV2, block2);
                            if (IV2[0] == IV1[0] && IV2[1] == IV1[1] && IV2[2] == IV1[2] && IV2[3] == IV1[3])
                            {
                                LogReturn(30);
                                return;
                            }

                            //if (IV2[0] != IV1[0])
                            //	std::cout << "!" << std::flush;
                        }
                    }
                }
            }
        }

        private bool IsStopped { get; set; }

        protected override void PerformStop()
        {
            IsStopped = true;
        }
    }
}
