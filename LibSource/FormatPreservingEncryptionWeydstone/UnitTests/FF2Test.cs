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

namespace FPETests
{
    [TestClass]
    public class FF2Test
    {

        public FF2Test()
        {
        }


        [TestMethod]
        public void testFF2EquivalentToFF2Parameters()
        {
            FFX ffx = new FFX(new FF2Parameters(10,10));
            byte[] validKey = { (byte) 0x2B, (byte) 0x7E, (byte) 0x15, (byte) 0x16, (byte) 0x28, (byte) 0xAE, (byte) 0xD2,
                    (byte) 0xA6, (byte) 0xAB, (byte) 0xF7, (byte) 0x15, (byte) 0x88, (byte) 0x09, (byte) 0xCF, (byte) 0x4F,
                    (byte) 0x3C };

            byte[] T = new byte[] { 0, 0, 1, 0 };

            int[] X = new int[] { 0, 1, 2, 3 };
            int[] CT1 = ffx.encrypt(validKey, T, X);
            Console.WriteLine(Common.intArrayToString(CT1));


            FF2 ff2 = new FF2(10, 10);
            int[] CT2 = ff2.encrypt(validKey, T, X);
            Console.WriteLine(Common.intArrayToString(CT2));

            CollectionAssert.AreEqual(CT1, CT2);
        }

        /**
	     * Stress test for encrypt() and decrypt() methods
	     * <p>
	     * 
	     * @throws InvalidKeyException
	     *             Only if there's a programming error in the test case.
	     */
        [TestMethod]
        public void testStress()
        {
            byte[] validKey = { (byte) 0x2B, (byte) 0x7E, (byte) 0x15, (byte) 0x16, (byte) 0x28, (byte) 0xAE, (byte) 0xD2,
                    (byte) 0xA6, (byte) 0xAB, (byte) 0xF7, (byte) 0x15, (byte) 0x88, (byte) 0x09, (byte) 0xCF, (byte) 0x4F,
                    (byte) 0x3C };

            byte[] T = new byte[] { 0, 0, 1, 0 };
            int tweakRadix = 2;
            int radix;
            int[] PT;

            // part 1: each supported radix and plaintext length combined with fixed tweak and fixed tweakRadix
            // only for power of 2 radix (else it would take too long)
            for (radix = Constants.MINRADIX_FF2; radix <= Constants.MAXRADIX_FF2; radix *= 2)
            {
                FF2 ff2 = new FF2(radix, tweakRadix);
                PT = new int[] { 0 };

                // for each permitted plaintext length
                for (int testLength = ff2.getMinlen(); testLength <= ff2.getMaxlen(); testLength++)
                {
                    // extend PT to the length testLength
                    while (PT.Length != testLength && PT.Length < testLength)
                    {
                        int[] PExtension = { radix - 1 };
                        PT = Common.concatenate(PT, PExtension);
                    }

                    //skip if format is too small 
                    if (Math.Pow(radix, testLength) < 100) continue;
                    // encrypt the plaintext
                    int[] CT = ff2.encrypt(validKey, T, PT);

                    // verify decrypted ciphertext against original plaintext
                    CollectionAssert.AreEqual(PT, ff2.decrypt(validKey, T, CT));

                    // verify format has been preserved. CT has same length as PT and consists of integers < radix
                    Assert.AreEqual(PT.Length, CT.Length);
                    foreach (int ctchar in CT)
                    {
                        Assert.IsTrue(ctchar < radix && ctchar >= 0);
                    }
                    // use the ciphertext as the new plaintext
                    PT = CT;
                }

            }

            //part 2: each supported tweakRadix and tweakLength combined with fixed radix and fixed PlaintextLength
            radix = 10;
            PT = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9};

            //for each tweakRadix
            for (tweakRadix = Constants.MINRADIX_FF2; tweakRadix <= Constants.MAXRADIX_FF2; tweakRadix *= 2)
            {
                FF2 ff2 = new FF2(radix, tweakRadix);
                T = new byte[] {};

                // for each permitted tweak length
                for (int testLength = 0; testLength < ff2.getMaxTlen(); testLength++)
                {
                    // extend Tweak to the length testLength
                    while (T.Length != testLength && T.Length < testLength)
                    {
                        byte[] TExtension = { (byte) (tweakRadix - 1) };
                        T = Common.concatenate(T, TExtension);
                    }

                    // encrypt the plaintext
                    int[] CT = ff2.encrypt(validKey, T, PT);

                    // verify decrypted ciphertext against original plaintext
                    CollectionAssert.AreEqual(PT, ff2.decrypt(validKey, T, CT));

                    // verify format has been preserved. CT has same length as PT and consists of integers < radix
                    Assert.AreEqual(PT.Length, CT.Length);
                    foreach (int ctchar in CT)
                    {
                        Assert.IsTrue(ctchar < radix && ctchar >= 0);
                    }
                    // use the ciphertext as the new plaintext
                    PT = CT;
                }

            }

        }
    }
}
