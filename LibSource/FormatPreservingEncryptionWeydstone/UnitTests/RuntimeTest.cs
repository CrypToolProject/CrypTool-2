/*
   Copyright 2018 CrypTool 2 Team <ct2contact@cryptool.org>

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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using FormatPreservingEncryptionWeydstone;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Numerics;

namespace FPETests
{
    /*
     * Notice: Constants.CONFORMANCE_OUTPUT should be set to false.
     * Warning: Test execution > 3min
     */
     
    //[TestClass]
    public class RuntimeTest
    {

        public Testvector[] testvectors;
        const int times = 1000;

        public struct Testvector
        {
            public byte[] K;
            public byte[] T;
            public int radix;
            public int tweakRadix;
            public int[] P;
        }

        public RuntimeTest()
        {
            byte[] key = { (byte) 0x2B, (byte) 0x7E, (byte) 0x15, (byte) 0x16, (byte) 0x28, (byte) 0xAE, (byte) 0xD2,
                    (byte) 0xA6, (byte) 0xAB, (byte) 0xF7, (byte) 0x15, (byte) 0x88, (byte) 0x09, (byte) 0xCF, (byte) 0x4F,
                    (byte) 0x3C };

            byte[] tweak ={ (byte) 0x2B, (byte) 0x7E, (byte) 0x15, (byte) 0x16, (byte) 0x28, (byte) 0xAE, (byte) 0xD2,
                    (byte) 0xA6 };

            int radix = 10;

            //create four plaintexts
            int[] P2 = new int[2] { 0, 5};
            int[] P30 = new int[30];
            int[] P56 = new int[56];
            int[] P1024 = new int[1024];

            //init
            for (int i = 0; i < 1024; i++)
            {
                int value = i % radix;
                if (i < 30)
                {
                    P30[i] = value;
                }
                if(i < 56)
                {
                    P56[i] = value;
                }
                P1024[i] = value;
            }

            testvectors = new Testvector[] {
                // m = P.Length
                //smallest domain possbile where radix^m >= 100:  m = 10
                new Testvector() {K=key, T=tweak, radix=10, tweakRadix=256, P = P2},
                //medium domain:  m = 30 
                new Testvector() {K=key, T=tweak, radix=10, tweakRadix=256, P = P30},
                //largest domain possible for FF2/DFF and FF3:  m = 56 
                new Testvector() {K=key, T=tweak, radix=10, tweakRadix=256, P = P56},
                //very large domain for FF1 only: m = 1024
                new Testvector() {K=key, T=tweak, radix=10, tweakRadix=256, P = P1024}
                };
        }

        [TestMethod]
        public void TestAESCBCRuntime()
        {
            SymmetricAlgorithm mAesCbcCipher = new AesCryptoServiceProvider();
            mAesCbcCipher.Mode = CipherMode.CBC;
            mAesCbcCipher.Padding = PaddingMode.Zeros;
            mAesCbcCipher.KeySize = 128;
            byte[] IV = { (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00,
                        (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00,
                        (byte) 0x00, (byte) 0x00 };


            Stopwatch watch;
            long elapsedMs;

            for (int i = 0; i < testvectors.Length; i++) {
                mAesCbcCipher.Key = testvectors[i].K;
                mAesCbcCipher.IV = IV;

                watch = System.Diagnostics.Stopwatch.StartNew();
                for (int j = 0; j < times; j++)
                {
                    BigInteger plaintextAsBigInteger = Common.num(testvectors[i].P, testvectors[i].radix);
                    byte[] plaintextAsByteArray = plaintextAsBigInteger.ToByteArray();

                    ICryptoTransform encryptor = mAesCbcCipher.CreateEncryptor();
                    encryptor.TransformFinalBlock(plaintextAsByteArray, 0, plaintextAsByteArray.Length);
                }
                elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine("AES time for testvector " + i + " : " + elapsedMs + " ms.");

            }
        } 

        [TestMethod]
        public void TestFPERuntime()
        {

            Stopwatch watch;
            long elapsedMs;
            int[] PT;
            FFX ffx;
            Testvector tv;


            for (int i = 0; i < 3; i++)
            {
                tv = testvectors[i];
                FF1 ff1 = new FF1(tv.radix, 100);

                PT = tv.P;

                watch = System.Diagnostics.Stopwatch.StartNew();
                for (int j = 0; j < times; j++)
                {
                    PT = ff1.encrypt(tv.K, tv.T, PT);
                }
                watch.Stop();
                elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine("FF1 time for testvector " + i + " : " + elapsedMs + " ms.");

                FF2 ff2 = new FF2(tv.radix, tv.tweakRadix);

                PT = tv.P;
                watch = System.Diagnostics.Stopwatch.StartNew();
                for (int j = 0; j < times; j++)
                {
                    PT = ff2.encrypt(tv.K, tv.T, PT);
                }
                watch.Stop();
                elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine("FF2 time for testvector " + i + " : " + elapsedMs + " ms.");

                FF3 ff3 = new FF3(tv.radix);

                PT = tv.P;
                watch = System.Diagnostics.Stopwatch.StartNew();
                for (int j = 0; j < times; j++)
                {
                    PT = ff3.encrypt(tv.K, tv.T, PT);
                }
                watch.Stop();
                elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine("FF3 time for testvector " + i + " : " + elapsedMs + " ms.");

                ffx = new FFX(new FF1Parameters(tv.radix, new Ciphers()));
                PT = tv.P;
                watch = System.Diagnostics.Stopwatch.StartNew();
                for (int j = 0; j < times; j++)
                {
                    PT = ffx.encrypt(tv.K, tv.T, PT);
                }
                watch.Stop();
                elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine("FF1-Parameterset time for testvector " + i + " : " + elapsedMs + " ms.");

                ffx = new FFX(new FF2Parameters(tv.radix, tv.tweakRadix));
                PT = tv.P;
                watch = System.Diagnostics.Stopwatch.StartNew();
                for (int j = 0; j < times; j++)
                {
                    PT = ffx.encrypt(tv.K, tv.T, PT);
                }
                watch.Stop();
                elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine("FF2-Parameterset time for testvector " + i + " : " + elapsedMs + " ms.");

                ffx = new FFX(new FF3Parameters(tv.radix));
                PT = tv.P;
                watch = System.Diagnostics.Stopwatch.StartNew();
                for (int j = 0; j < times; j++)
                {
                    PT = ffx.encrypt(tv.K, tv.T, PT);
                }
                watch.Stop();
                elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine("FF3-Parameterset time for testvector " + i + " : " + elapsedMs + " ms.");
            }

            //testvector 3 is only for FF1

            int k = 3;
            tv = testvectors[k];
            FF1 ff11 = new FF1(tv.radix, 100);
            PT = tv.P;

            watch = System.Diagnostics.Stopwatch.StartNew();
            for (int j = 0; j < times; j++)
            {
                PT = ff11.encrypt(tv.K, tv.T, PT);
            }
            watch.Stop();
            elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine("FF1 time for testvector " + k + " : " + elapsedMs + " ms.");

            ffx = new FFX(new FF1Parameters(tv.radix, new Ciphers()));
            PT = tv.P;
            watch = System.Diagnostics.Stopwatch.StartNew();
            for (int j = 0; j < times; j++)
            {
                PT = ffx.encrypt(tv.K, tv.T, PT);
            }
            watch.Stop();
            elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine("FF1-Parameterset time for testvector " + k + " : " + elapsedMs + " ms.");


            Assert.IsTrue(true);
        }

    }
}
