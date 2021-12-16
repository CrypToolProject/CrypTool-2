/*
Copyright (c) 2010 Alberto Fajardo

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/

using System;

namespace CrypTool.Plugins.Blowfish.Threefish
{
    internal class Threefish512 : ThreefishAlgorithm
    {
        private const int CipherSize = 512;
        private const int CipherQwords = CipherSize / 64;
        private const int ExpandedKeySize = CipherQwords + 1;

        public Threefish512()
        {
            // Create the expanded key array
            ExpandedKey = new ulong[ExpandedKeySize];
            ExpandedKey[ExpandedKeySize - 1] = KeyScheduleConst;
        }

        public void Encrypt(ulong[] input, ulong[] output)
        {
            // Cache the block, key, and tweak
            ulong b0 = input[0], b1 = input[1],
                  b2 = input[2], b3 = input[3],
                  b4 = input[4], b5 = input[5],
                  b6 = input[6], b7 = input[7];
            ulong k0 = ExpandedKey[0], k1 = ExpandedKey[1],
                  k2 = ExpandedKey[2], k3 = ExpandedKey[3],
                  k4 = ExpandedKey[4], k5 = ExpandedKey[5],
                  k6 = ExpandedKey[6], k7 = ExpandedKey[7],
                  k8 = ExpandedKey[8];
            ulong t0 = ExpandedTweak[0], t1 = ExpandedTweak[1],
                  t2 = ExpandedTweak[2];

            Mix(ref b0, ref b1, 46, k0, k1);
            Mix(ref b2, ref b3, 36, k2, k3);
            Mix(ref b4, ref b5, 19, k4, k5 + t0);
            Mix(ref b6, ref b7, 37, k6 + t1, k7);
            Mix(ref b2, ref b1, 33);
            Mix(ref b4, ref b7, 27);
            Mix(ref b6, ref b5, 14);
            Mix(ref b0, ref b3, 42);
            Mix(ref b4, ref b1, 17);
            Mix(ref b6, ref b3, 49);
            Mix(ref b0, ref b5, 36);
            Mix(ref b2, ref b7, 39);
            Mix(ref b6, ref b1, 44);
            Mix(ref b0, ref b7, 9);
            Mix(ref b2, ref b5, 54);
            Mix(ref b4, ref b3, 56);
            Mix(ref b0, ref b1, 39, k1, k2);
            Mix(ref b2, ref b3, 30, k3, k4);
            Mix(ref b4, ref b5, 34, k5, k6 + t1);
            Mix(ref b6, ref b7, 24, k7 + t2, k8 + 1);
            Mix(ref b2, ref b1, 13);
            Mix(ref b4, ref b7, 50);
            Mix(ref b6, ref b5, 10);
            Mix(ref b0, ref b3, 17);
            Mix(ref b4, ref b1, 25);
            Mix(ref b6, ref b3, 29);
            Mix(ref b0, ref b5, 39);
            Mix(ref b2, ref b7, 43);
            Mix(ref b6, ref b1, 8);
            Mix(ref b0, ref b7, 35);
            Mix(ref b2, ref b5, 56);
            Mix(ref b4, ref b3, 22);
            Mix(ref b0, ref b1, 46, k2, k3);
            Mix(ref b2, ref b3, 36, k4, k5);
            Mix(ref b4, ref b5, 19, k6, k7 + t2);
            Mix(ref b6, ref b7, 37, k8 + t0, k0 + 2);
            Mix(ref b2, ref b1, 33);
            Mix(ref b4, ref b7, 27);
            Mix(ref b6, ref b5, 14);
            Mix(ref b0, ref b3, 42);
            Mix(ref b4, ref b1, 17);
            Mix(ref b6, ref b3, 49);
            Mix(ref b0, ref b5, 36);
            Mix(ref b2, ref b7, 39);
            Mix(ref b6, ref b1, 44);
            Mix(ref b0, ref b7, 9);
            Mix(ref b2, ref b5, 54);
            Mix(ref b4, ref b3, 56);
            Mix(ref b0, ref b1, 39, k3, k4);
            Mix(ref b2, ref b3, 30, k5, k6);
            Mix(ref b4, ref b5, 34, k7, k8 + t0);
            Mix(ref b6, ref b7, 24, k0 + t1, k1 + 3);
            Mix(ref b2, ref b1, 13);
            Mix(ref b4, ref b7, 50);
            Mix(ref b6, ref b5, 10);
            Mix(ref b0, ref b3, 17);
            Mix(ref b4, ref b1, 25);
            Mix(ref b6, ref b3, 29);
            Mix(ref b0, ref b5, 39);
            Mix(ref b2, ref b7, 43);
            Mix(ref b6, ref b1, 8);
            Mix(ref b0, ref b7, 35);
            Mix(ref b2, ref b5, 56);
            Mix(ref b4, ref b3, 22);
            Mix(ref b0, ref b1, 46, k4, k5);
            Mix(ref b2, ref b3, 36, k6, k7);
            Mix(ref b4, ref b5, 19, k8, k0 + t1);
            Mix(ref b6, ref b7, 37, k1 + t2, k2 + 4);
            Mix(ref b2, ref b1, 33);
            Mix(ref b4, ref b7, 27);
            Mix(ref b6, ref b5, 14);
            Mix(ref b0, ref b3, 42);
            Mix(ref b4, ref b1, 17);
            Mix(ref b6, ref b3, 49);
            Mix(ref b0, ref b5, 36);
            Mix(ref b2, ref b7, 39);
            Mix(ref b6, ref b1, 44);
            Mix(ref b0, ref b7, 9);
            Mix(ref b2, ref b5, 54);
            Mix(ref b4, ref b3, 56);
            Mix(ref b0, ref b1, 39, k5, k6);
            Mix(ref b2, ref b3, 30, k7, k8);
            Mix(ref b4, ref b5, 34, k0, k1 + t2);
            Mix(ref b6, ref b7, 24, k2 + t0, k3 + 5);
            Mix(ref b2, ref b1, 13);
            Mix(ref b4, ref b7, 50);
            Mix(ref b6, ref b5, 10);
            Mix(ref b0, ref b3, 17);
            Mix(ref b4, ref b1, 25);
            Mix(ref b6, ref b3, 29);
            Mix(ref b0, ref b5, 39);
            Mix(ref b2, ref b7, 43);
            Mix(ref b6, ref b1, 8);
            Mix(ref b0, ref b7, 35);
            Mix(ref b2, ref b5, 56);
            Mix(ref b4, ref b3, 22);
            Mix(ref b0, ref b1, 46, k6, k7);
            Mix(ref b2, ref b3, 36, k8, k0);
            Mix(ref b4, ref b5, 19, k1, k2 + t0);
            Mix(ref b6, ref b7, 37, k3 + t1, k4 + 6);
            Mix(ref b2, ref b1, 33);
            Mix(ref b4, ref b7, 27);
            Mix(ref b6, ref b5, 14);
            Mix(ref b0, ref b3, 42);
            Mix(ref b4, ref b1, 17);
            Mix(ref b6, ref b3, 49);
            Mix(ref b0, ref b5, 36);
            Mix(ref b2, ref b7, 39);
            Mix(ref b6, ref b1, 44);
            Mix(ref b0, ref b7, 9);
            Mix(ref b2, ref b5, 54);
            Mix(ref b4, ref b3, 56);
            Mix(ref b0, ref b1, 39, k7, k8);
            Mix(ref b2, ref b3, 30, k0, k1);
            Mix(ref b4, ref b5, 34, k2, k3 + t1);
            Mix(ref b6, ref b7, 24, k4 + t2, k5 + 7);
            Mix(ref b2, ref b1, 13);
            Mix(ref b4, ref b7, 50);
            Mix(ref b6, ref b5, 10);
            Mix(ref b0, ref b3, 17);
            Mix(ref b4, ref b1, 25);
            Mix(ref b6, ref b3, 29);
            Mix(ref b0, ref b5, 39);
            Mix(ref b2, ref b7, 43);
            Mix(ref b6, ref b1, 8);
            Mix(ref b0, ref b7, 35);
            Mix(ref b2, ref b5, 56);
            Mix(ref b4, ref b3, 22);
            Mix(ref b0, ref b1, 46, k8, k0);
            Mix(ref b2, ref b3, 36, k1, k2);
            Mix(ref b4, ref b5, 19, k3, k4 + t2);
            Mix(ref b6, ref b7, 37, k5 + t0, k6 + 8);
            Mix(ref b2, ref b1, 33);
            Mix(ref b4, ref b7, 27);
            Mix(ref b6, ref b5, 14);
            Mix(ref b0, ref b3, 42);
            Mix(ref b4, ref b1, 17);
            Mix(ref b6, ref b3, 49);
            Mix(ref b0, ref b5, 36);
            Mix(ref b2, ref b7, 39);
            Mix(ref b6, ref b1, 44);
            Mix(ref b0, ref b7, 9);
            Mix(ref b2, ref b5, 54);
            Mix(ref b4, ref b3, 56);
            Mix(ref b0, ref b1, 39, k0, k1);
            Mix(ref b2, ref b3, 30, k2, k3);
            Mix(ref b4, ref b5, 34, k4, k5 + t0);
            Mix(ref b6, ref b7, 24, k6 + t1, k7 + 9);
            Mix(ref b2, ref b1, 13);
            Mix(ref b4, ref b7, 50);
            Mix(ref b6, ref b5, 10);
            Mix(ref b0, ref b3, 17);
            Mix(ref b4, ref b1, 25);
            Mix(ref b6, ref b3, 29);
            Mix(ref b0, ref b5, 39);
            Mix(ref b2, ref b7, 43);
            Mix(ref b6, ref b1, 8);
            Mix(ref b0, ref b7, 35);
            Mix(ref b2, ref b5, 56);
            Mix(ref b4, ref b3, 22);
            Mix(ref b0, ref b1, 46, k1, k2);
            Mix(ref b2, ref b3, 36, k3, k4);
            Mix(ref b4, ref b5, 19, k5, k6 + t1);
            Mix(ref b6, ref b7, 37, k7 + t2, k8 + 10);
            Mix(ref b2, ref b1, 33);
            Mix(ref b4, ref b7, 27);
            Mix(ref b6, ref b5, 14);
            Mix(ref b0, ref b3, 42);
            Mix(ref b4, ref b1, 17);
            Mix(ref b6, ref b3, 49);
            Mix(ref b0, ref b5, 36);
            Mix(ref b2, ref b7, 39);
            Mix(ref b6, ref b1, 44);
            Mix(ref b0, ref b7, 9);
            Mix(ref b2, ref b5, 54);
            Mix(ref b4, ref b3, 56);
            Mix(ref b0, ref b1, 39, k2, k3);
            Mix(ref b2, ref b3, 30, k4, k5);
            Mix(ref b4, ref b5, 34, k6, k7 + t2);
            Mix(ref b6, ref b7, 24, k8 + t0, k0 + 11);
            Mix(ref b2, ref b1, 13);
            Mix(ref b4, ref b7, 50);
            Mix(ref b6, ref b5, 10);
            Mix(ref b0, ref b3, 17);
            Mix(ref b4, ref b1, 25);
            Mix(ref b6, ref b3, 29);
            Mix(ref b0, ref b5, 39);
            Mix(ref b2, ref b7, 43);
            Mix(ref b6, ref b1, 8);
            Mix(ref b0, ref b7, 35);
            Mix(ref b2, ref b5, 56);
            Mix(ref b4, ref b3, 22);
            Mix(ref b0, ref b1, 46, k3, k4);
            Mix(ref b2, ref b3, 36, k5, k6);
            Mix(ref b4, ref b5, 19, k7, k8 + t0);
            Mix(ref b6, ref b7, 37, k0 + t1, k1 + 12);
            Mix(ref b2, ref b1, 33);
            Mix(ref b4, ref b7, 27);
            Mix(ref b6, ref b5, 14);
            Mix(ref b0, ref b3, 42);
            Mix(ref b4, ref b1, 17);
            Mix(ref b6, ref b3, 49);
            Mix(ref b0, ref b5, 36);
            Mix(ref b2, ref b7, 39);
            Mix(ref b6, ref b1, 44);
            Mix(ref b0, ref b7, 9);
            Mix(ref b2, ref b5, 54);
            Mix(ref b4, ref b3, 56);
            Mix(ref b0, ref b1, 39, k4, k5);
            Mix(ref b2, ref b3, 30, k6, k7);
            Mix(ref b4, ref b5, 34, k8, k0 + t1);
            Mix(ref b6, ref b7, 24, k1 + t2, k2 + 13);
            Mix(ref b2, ref b1, 13);
            Mix(ref b4, ref b7, 50);
            Mix(ref b6, ref b5, 10);
            Mix(ref b0, ref b3, 17);
            Mix(ref b4, ref b1, 25);
            Mix(ref b6, ref b3, 29);
            Mix(ref b0, ref b5, 39);
            Mix(ref b2, ref b7, 43);
            Mix(ref b6, ref b1, 8);
            Mix(ref b0, ref b7, 35);
            Mix(ref b2, ref b5, 56);
            Mix(ref b4, ref b3, 22);
            Mix(ref b0, ref b1, 46, k5, k6);
            Mix(ref b2, ref b3, 36, k7, k8);
            Mix(ref b4, ref b5, 19, k0, k1 + t2);
            Mix(ref b6, ref b7, 37, k2 + t0, k3 + 14);
            Mix(ref b2, ref b1, 33);
            Mix(ref b4, ref b7, 27);
            Mix(ref b6, ref b5, 14);
            Mix(ref b0, ref b3, 42);
            Mix(ref b4, ref b1, 17);
            Mix(ref b6, ref b3, 49);
            Mix(ref b0, ref b5, 36);
            Mix(ref b2, ref b7, 39);
            Mix(ref b6, ref b1, 44);
            Mix(ref b0, ref b7, 9);
            Mix(ref b2, ref b5, 54);
            Mix(ref b4, ref b3, 56);
            Mix(ref b0, ref b1, 39, k6, k7);
            Mix(ref b2, ref b3, 30, k8, k0);
            Mix(ref b4, ref b5, 34, k1, k2 + t0);
            Mix(ref b6, ref b7, 24, k3 + t1, k4 + 15);
            Mix(ref b2, ref b1, 13);
            Mix(ref b4, ref b7, 50);
            Mix(ref b6, ref b5, 10);
            Mix(ref b0, ref b3, 17);
            Mix(ref b4, ref b1, 25);
            Mix(ref b6, ref b3, 29);
            Mix(ref b0, ref b5, 39);
            Mix(ref b2, ref b7, 43);
            Mix(ref b6, ref b1, 8);
            Mix(ref b0, ref b7, 35);
            Mix(ref b2, ref b5, 56);
            Mix(ref b4, ref b3, 22);
            Mix(ref b0, ref b1, 46, k7, k8);
            Mix(ref b2, ref b3, 36, k0, k1);
            Mix(ref b4, ref b5, 19, k2, k3 + t1);
            Mix(ref b6, ref b7, 37, k4 + t2, k5 + 16);
            Mix(ref b2, ref b1, 33);
            Mix(ref b4, ref b7, 27);
            Mix(ref b6, ref b5, 14);
            Mix(ref b0, ref b3, 42);
            Mix(ref b4, ref b1, 17);
            Mix(ref b6, ref b3, 49);
            Mix(ref b0, ref b5, 36);
            Mix(ref b2, ref b7, 39);
            Mix(ref b6, ref b1, 44);
            Mix(ref b0, ref b7, 9);
            Mix(ref b2, ref b5, 54);
            Mix(ref b4, ref b3, 56);
            Mix(ref b0, ref b1, 39, k8, k0);
            Mix(ref b2, ref b3, 30, k1, k2);
            Mix(ref b4, ref b5, 34, k3, k4 + t2);
            Mix(ref b6, ref b7, 24, k5 + t0, k6 + 17);
            Mix(ref b2, ref b1, 13);
            Mix(ref b4, ref b7, 50);
            Mix(ref b6, ref b5, 10);
            Mix(ref b0, ref b3, 17);
            Mix(ref b4, ref b1, 25);
            Mix(ref b6, ref b3, 29);
            Mix(ref b0, ref b5, 39);
            Mix(ref b2, ref b7, 43);
            Mix(ref b6, ref b1, 8);
            Mix(ref b0, ref b7, 35);
            Mix(ref b2, ref b5, 56);
            Mix(ref b4, ref b3, 22);

            // Final key schedule
            output[0] = b0 + k0;
            output[1] = b1 + k1;
            output[2] = b2 + k2;
            output[3] = b3 + k3;
            output[4] = b4 + k4;
            output[5] = b5 + k5 + t0;
            output[6] = b6 + k6 + t1;
            output[7] = b7 + k7 + 18;
        }

        public void Decrypt(ulong[] input, ulong[] output)
        {
            // Cache the block, key, and tweak
            ulong b0 = input[0], b1 = input[1],
                  b2 = input[2], b3 = input[3],
                  b4 = input[4], b5 = input[5],
                  b6 = input[6], b7 = input[7];
            ulong k0 = ExpandedKey[0], k1 = ExpandedKey[1],
                  k2 = ExpandedKey[2], k3 = ExpandedKey[3],
                  k4 = ExpandedKey[4], k5 = ExpandedKey[5],
                  k6 = ExpandedKey[6], k7 = ExpandedKey[7],
                  k8 = ExpandedKey[8];
            ulong t0 = ExpandedTweak[0], t1 = ExpandedTweak[1],
                  t2 = ExpandedTweak[2];


            b0 -= k0;
            b1 -= k1;
            b2 -= k2;
            b3 -= k3;
            b4 -= k4;
            b5 -= k5 + t0;
            b6 -= k6 + t1;
            b7 -= k7 + 18;
            UnMix(ref b4, ref b3, 22);
            UnMix(ref b2, ref b5, 56);
            UnMix(ref b0, ref b7, 35);
            UnMix(ref b6, ref b1, 8);
            UnMix(ref b2, ref b7, 43);
            UnMix(ref b0, ref b5, 39);
            UnMix(ref b6, ref b3, 29);
            UnMix(ref b4, ref b1, 25);
            UnMix(ref b0, ref b3, 17);
            UnMix(ref b6, ref b5, 10);
            UnMix(ref b4, ref b7, 50);
            UnMix(ref b2, ref b1, 13);
            UnMix(ref b6, ref b7, 24, k5 + t0, k6 + 17);
            UnMix(ref b4, ref b5, 34, k3, k4 + t2);
            UnMix(ref b2, ref b3, 30, k1, k2);
            UnMix(ref b0, ref b1, 39, k8, k0);
            UnMix(ref b4, ref b3, 56);
            UnMix(ref b2, ref b5, 54);
            UnMix(ref b0, ref b7, 9);
            UnMix(ref b6, ref b1, 44);
            UnMix(ref b2, ref b7, 39);
            UnMix(ref b0, ref b5, 36);
            UnMix(ref b6, ref b3, 49);
            UnMix(ref b4, ref b1, 17);
            UnMix(ref b0, ref b3, 42);
            UnMix(ref b6, ref b5, 14);
            UnMix(ref b4, ref b7, 27);
            UnMix(ref b2, ref b1, 33);
            UnMix(ref b6, ref b7, 37, k4 + t2, k5 + 16);
            UnMix(ref b4, ref b5, 19, k2, k3 + t1);
            UnMix(ref b2, ref b3, 36, k0, k1);
            UnMix(ref b0, ref b1, 46, k7, k8);
            UnMix(ref b4, ref b3, 22);
            UnMix(ref b2, ref b5, 56);
            UnMix(ref b0, ref b7, 35);
            UnMix(ref b6, ref b1, 8);
            UnMix(ref b2, ref b7, 43);
            UnMix(ref b0, ref b5, 39);
            UnMix(ref b6, ref b3, 29);
            UnMix(ref b4, ref b1, 25);
            UnMix(ref b0, ref b3, 17);
            UnMix(ref b6, ref b5, 10);
            UnMix(ref b4, ref b7, 50);
            UnMix(ref b2, ref b1, 13);
            UnMix(ref b6, ref b7, 24, k3 + t1, k4 + 15);
            UnMix(ref b4, ref b5, 34, k1, k2 + t0);
            UnMix(ref b2, ref b3, 30, k8, k0);
            UnMix(ref b0, ref b1, 39, k6, k7);
            UnMix(ref b4, ref b3, 56);
            UnMix(ref b2, ref b5, 54);
            UnMix(ref b0, ref b7, 9);
            UnMix(ref b6, ref b1, 44);
            UnMix(ref b2, ref b7, 39);
            UnMix(ref b0, ref b5, 36);
            UnMix(ref b6, ref b3, 49);
            UnMix(ref b4, ref b1, 17);
            UnMix(ref b0, ref b3, 42);
            UnMix(ref b6, ref b5, 14);
            UnMix(ref b4, ref b7, 27);
            UnMix(ref b2, ref b1, 33);
            UnMix(ref b6, ref b7, 37, k2 + t0, k3 + 14);
            UnMix(ref b4, ref b5, 19, k0, k1 + t2);
            UnMix(ref b2, ref b3, 36, k7, k8);
            UnMix(ref b0, ref b1, 46, k5, k6);
            UnMix(ref b4, ref b3, 22);
            UnMix(ref b2, ref b5, 56);
            UnMix(ref b0, ref b7, 35);
            UnMix(ref b6, ref b1, 8);
            UnMix(ref b2, ref b7, 43);
            UnMix(ref b0, ref b5, 39);
            UnMix(ref b6, ref b3, 29);
            UnMix(ref b4, ref b1, 25);
            UnMix(ref b0, ref b3, 17);
            UnMix(ref b6, ref b5, 10);
            UnMix(ref b4, ref b7, 50);
            UnMix(ref b2, ref b1, 13);
            UnMix(ref b6, ref b7, 24, k1 + t2, k2 + 13);
            UnMix(ref b4, ref b5, 34, k8, k0 + t1);
            UnMix(ref b2, ref b3, 30, k6, k7);
            UnMix(ref b0, ref b1, 39, k4, k5);
            UnMix(ref b4, ref b3, 56);
            UnMix(ref b2, ref b5, 54);
            UnMix(ref b0, ref b7, 9);
            UnMix(ref b6, ref b1, 44);
            UnMix(ref b2, ref b7, 39);
            UnMix(ref b0, ref b5, 36);
            UnMix(ref b6, ref b3, 49);
            UnMix(ref b4, ref b1, 17);
            UnMix(ref b0, ref b3, 42);
            UnMix(ref b6, ref b5, 14);
            UnMix(ref b4, ref b7, 27);
            UnMix(ref b2, ref b1, 33);
            UnMix(ref b6, ref b7, 37, k0 + t1, k1 + 12);
            UnMix(ref b4, ref b5, 19, k7, k8 + t0);
            UnMix(ref b2, ref b3, 36, k5, k6);
            UnMix(ref b0, ref b1, 46, k3, k4);
            UnMix(ref b4, ref b3, 22);
            UnMix(ref b2, ref b5, 56);
            UnMix(ref b0, ref b7, 35);
            UnMix(ref b6, ref b1, 8);
            UnMix(ref b2, ref b7, 43);
            UnMix(ref b0, ref b5, 39);
            UnMix(ref b6, ref b3, 29);
            UnMix(ref b4, ref b1, 25);
            UnMix(ref b0, ref b3, 17);
            UnMix(ref b6, ref b5, 10);
            UnMix(ref b4, ref b7, 50);
            UnMix(ref b2, ref b1, 13);
            UnMix(ref b6, ref b7, 24, k8 + t0, k0 + 11);
            UnMix(ref b4, ref b5, 34, k6, k7 + t2);
            UnMix(ref b2, ref b3, 30, k4, k5);
            UnMix(ref b0, ref b1, 39, k2, k3);
            UnMix(ref b4, ref b3, 56);
            UnMix(ref b2, ref b5, 54);
            UnMix(ref b0, ref b7, 9);
            UnMix(ref b6, ref b1, 44);
            UnMix(ref b2, ref b7, 39);
            UnMix(ref b0, ref b5, 36);
            UnMix(ref b6, ref b3, 49);
            UnMix(ref b4, ref b1, 17);
            UnMix(ref b0, ref b3, 42);
            UnMix(ref b6, ref b5, 14);
            UnMix(ref b4, ref b7, 27);
            UnMix(ref b2, ref b1, 33);
            UnMix(ref b6, ref b7, 37, k7 + t2, k8 + 10);
            UnMix(ref b4, ref b5, 19, k5, k6 + t1);
            UnMix(ref b2, ref b3, 36, k3, k4);
            UnMix(ref b0, ref b1, 46, k1, k2);
            UnMix(ref b4, ref b3, 22);
            UnMix(ref b2, ref b5, 56);
            UnMix(ref b0, ref b7, 35);
            UnMix(ref b6, ref b1, 8);
            UnMix(ref b2, ref b7, 43);
            UnMix(ref b0, ref b5, 39);
            UnMix(ref b6, ref b3, 29);
            UnMix(ref b4, ref b1, 25);
            UnMix(ref b0, ref b3, 17);
            UnMix(ref b6, ref b5, 10);
            UnMix(ref b4, ref b7, 50);
            UnMix(ref b2, ref b1, 13);
            UnMix(ref b6, ref b7, 24, k6 + t1, k7 + 9);
            UnMix(ref b4, ref b5, 34, k4, k5 + t0);
            UnMix(ref b2, ref b3, 30, k2, k3);
            UnMix(ref b0, ref b1, 39, k0, k1);
            UnMix(ref b4, ref b3, 56);
            UnMix(ref b2, ref b5, 54);
            UnMix(ref b0, ref b7, 9);
            UnMix(ref b6, ref b1, 44);
            UnMix(ref b2, ref b7, 39);
            UnMix(ref b0, ref b5, 36);
            UnMix(ref b6, ref b3, 49);
            UnMix(ref b4, ref b1, 17);
            UnMix(ref b0, ref b3, 42);
            UnMix(ref b6, ref b5, 14);
            UnMix(ref b4, ref b7, 27);
            UnMix(ref b2, ref b1, 33);
            UnMix(ref b6, ref b7, 37, k5 + t0, k6 + 8);
            UnMix(ref b4, ref b5, 19, k3, k4 + t2);
            UnMix(ref b2, ref b3, 36, k1, k2);
            UnMix(ref b0, ref b1, 46, k8, k0);
            UnMix(ref b4, ref b3, 22);
            UnMix(ref b2, ref b5, 56);
            UnMix(ref b0, ref b7, 35);
            UnMix(ref b6, ref b1, 8);
            UnMix(ref b2, ref b7, 43);
            UnMix(ref b0, ref b5, 39);
            UnMix(ref b6, ref b3, 29);
            UnMix(ref b4, ref b1, 25);
            UnMix(ref b0, ref b3, 17);
            UnMix(ref b6, ref b5, 10);
            UnMix(ref b4, ref b7, 50);
            UnMix(ref b2, ref b1, 13);
            UnMix(ref b6, ref b7, 24, k4 + t2, k5 + 7);
            UnMix(ref b4, ref b5, 34, k2, k3 + t1);
            UnMix(ref b2, ref b3, 30, k0, k1);
            UnMix(ref b0, ref b1, 39, k7, k8);
            UnMix(ref b4, ref b3, 56);
            UnMix(ref b2, ref b5, 54);
            UnMix(ref b0, ref b7, 9);
            UnMix(ref b6, ref b1, 44);
            UnMix(ref b2, ref b7, 39);
            UnMix(ref b0, ref b5, 36);
            UnMix(ref b6, ref b3, 49);
            UnMix(ref b4, ref b1, 17);
            UnMix(ref b0, ref b3, 42);
            UnMix(ref b6, ref b5, 14);
            UnMix(ref b4, ref b7, 27);
            UnMix(ref b2, ref b1, 33);
            UnMix(ref b6, ref b7, 37, k3 + t1, k4 + 6);
            UnMix(ref b4, ref b5, 19, k1, k2 + t0);
            UnMix(ref b2, ref b3, 36, k8, k0);
            UnMix(ref b0, ref b1, 46, k6, k7);
            UnMix(ref b4, ref b3, 22);
            UnMix(ref b2, ref b5, 56);
            UnMix(ref b0, ref b7, 35);
            UnMix(ref b6, ref b1, 8);
            UnMix(ref b2, ref b7, 43);
            UnMix(ref b0, ref b5, 39);
            UnMix(ref b6, ref b3, 29);
            UnMix(ref b4, ref b1, 25);
            UnMix(ref b0, ref b3, 17);
            UnMix(ref b6, ref b5, 10);
            UnMix(ref b4, ref b7, 50);
            UnMix(ref b2, ref b1, 13);
            UnMix(ref b6, ref b7, 24, k2 + t0, k3 + 5);
            UnMix(ref b4, ref b5, 34, k0, k1 + t2);
            UnMix(ref b2, ref b3, 30, k7, k8);
            UnMix(ref b0, ref b1, 39, k5, k6);
            UnMix(ref b4, ref b3, 56);
            UnMix(ref b2, ref b5, 54);
            UnMix(ref b0, ref b7, 9);
            UnMix(ref b6, ref b1, 44);
            UnMix(ref b2, ref b7, 39);
            UnMix(ref b0, ref b5, 36);
            UnMix(ref b6, ref b3, 49);
            UnMix(ref b4, ref b1, 17);
            UnMix(ref b0, ref b3, 42);
            UnMix(ref b6, ref b5, 14);
            UnMix(ref b4, ref b7, 27);
            UnMix(ref b2, ref b1, 33);
            UnMix(ref b6, ref b7, 37, k1 + t2, k2 + 4);
            UnMix(ref b4, ref b5, 19, k8, k0 + t1);
            UnMix(ref b2, ref b3, 36, k6, k7);
            UnMix(ref b0, ref b1, 46, k4, k5);
            UnMix(ref b4, ref b3, 22);
            UnMix(ref b2, ref b5, 56);
            UnMix(ref b0, ref b7, 35);
            UnMix(ref b6, ref b1, 8);
            UnMix(ref b2, ref b7, 43);
            UnMix(ref b0, ref b5, 39);
            UnMix(ref b6, ref b3, 29);
            UnMix(ref b4, ref b1, 25);
            UnMix(ref b0, ref b3, 17);
            UnMix(ref b6, ref b5, 10);
            UnMix(ref b4, ref b7, 50);
            UnMix(ref b2, ref b1, 13);
            UnMix(ref b6, ref b7, 24, k0 + t1, k1 + 3);
            UnMix(ref b4, ref b5, 34, k7, k8 + t0);
            UnMix(ref b2, ref b3, 30, k5, k6);
            UnMix(ref b0, ref b1, 39, k3, k4);
            UnMix(ref b4, ref b3, 56);
            UnMix(ref b2, ref b5, 54);
            UnMix(ref b0, ref b7, 9);
            UnMix(ref b6, ref b1, 44);
            UnMix(ref b2, ref b7, 39);
            UnMix(ref b0, ref b5, 36);
            UnMix(ref b6, ref b3, 49);
            UnMix(ref b4, ref b1, 17);
            UnMix(ref b0, ref b3, 42);
            UnMix(ref b6, ref b5, 14);
            UnMix(ref b4, ref b7, 27);
            UnMix(ref b2, ref b1, 33);
            UnMix(ref b6, ref b7, 37, k8 + t0, k0 + 2);
            UnMix(ref b4, ref b5, 19, k6, k7 + t2);
            UnMix(ref b2, ref b3, 36, k4, k5);
            UnMix(ref b0, ref b1, 46, k2, k3);
            UnMix(ref b4, ref b3, 22);
            UnMix(ref b2, ref b5, 56);
            UnMix(ref b0, ref b7, 35);
            UnMix(ref b6, ref b1, 8);
            UnMix(ref b2, ref b7, 43);
            UnMix(ref b0, ref b5, 39);
            UnMix(ref b6, ref b3, 29);
            UnMix(ref b4, ref b1, 25);
            UnMix(ref b0, ref b3, 17);
            UnMix(ref b6, ref b5, 10);
            UnMix(ref b4, ref b7, 50);
            UnMix(ref b2, ref b1, 13);
            UnMix(ref b6, ref b7, 24, k7 + t2, k8 + 1);
            UnMix(ref b4, ref b5, 34, k5, k6 + t1);
            UnMix(ref b2, ref b3, 30, k3, k4);
            UnMix(ref b0, ref b1, 39, k1, k2);
            UnMix(ref b4, ref b3, 56);
            UnMix(ref b2, ref b5, 54);
            UnMix(ref b0, ref b7, 9);
            UnMix(ref b6, ref b1, 44);
            UnMix(ref b2, ref b7, 39);
            UnMix(ref b0, ref b5, 36);
            UnMix(ref b6, ref b3, 49);
            UnMix(ref b4, ref b1, 17);
            UnMix(ref b0, ref b3, 42);
            UnMix(ref b6, ref b5, 14);
            UnMix(ref b4, ref b7, 27);
            UnMix(ref b2, ref b1, 33);
            UnMix(ref b6, ref b7, 37, k6 + t1, k7);
            UnMix(ref b4, ref b5, 19, k4, k5 + t0);
            UnMix(ref b2, ref b3, 36, k2, k3);
            UnMix(ref b0, ref b1, 46, k0, k1);

            output[7] = b7;
            output[6] = b6;
            output[5] = b5;
            output[4] = b4;
            output[3] = b3;
            output[2] = b2;
            output[1] = b1;
            output[0] = b0;
        }

        public override byte[] Encrypt(byte[] input, byte[] key)
        {
            const int number_count = 8;

            byte[] output = new byte[number_count * 8];
            ulong[] o = new ulong[number_count];
            ulong[] b = new ulong[number_count];

            for (int i = 0; i < number_count; i++)
            {
                b[i] = BitConverter.ToUInt64(input, i * 8);
            }

            Encrypt(b, o);

            for (int i = 0; i < number_count; i++)
            {
                byte[] tmp = BitConverter.GetBytes(o[i]);
                for (int j = 0; j < 8; j++)
                {
                    output[i * 8 + j] = tmp[j];
                }
            }
            return output;
        }

        public override byte[] Decrypt(byte[] input, byte[] key)
        {
            const int number_count = 8;

            byte[] output = new byte[number_count * 8];
            ulong[] o = new ulong[number_count];
            ulong[] b = new ulong[number_count];

            for (int i = 0; i < number_count; i++)
            {
                b[i] = BitConverter.ToUInt64(input, i * 8);
            }

            Decrypt(b, o);

            for (int i = 0; i < number_count; i++)
            {
                byte[] tmp = BitConverter.GetBytes(o[i]);
                for (int j = 0; j < 8; j++)
                {
                    output[i * 8 + j] = tmp[j];
                }
            }
            return output;
        }
    }
}
