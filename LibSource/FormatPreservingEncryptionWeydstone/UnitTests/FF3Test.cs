/**
 * Format-Preserving Encryption
 * 
 * Copyright (c) 2016 Weydstone LLC dba Sutton Abinger
 * 
 * See the NOTICE file distributed with this work for additional information
 * regarding copyright ownership. Sutton Abinger licenses this file to you under
 * the Apache License, Version 2.0 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of the License
 * at http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations under
 * the License.
 */
/*
* Converted and modified by Alexander Hirsch <hirsch@cryptool.org>
*/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FormatPreservingEncryptionWeydstone;

namespace FPETests
{
    [TestClass]
    public class FF3Test
    {

        /**
	     * Test method for {@link org.fpe4j.FF3#FF3(int)}.
	     */
        [TestMethod]
        public void testFF3()
        {
            FF3 ff3 = new FF3(10);
            Assert.IsNotNull(ff3);

            // check values of minlen and maxlen
            int[] radixValues = { 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536 };
            int[] expectedMinlen = { 7, 4, 3, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
            int[] expectedMaxlen = { 192, 96, 64, 48, 38, 32, 26, 24, 20, 18, 16, 16, 14, 12, 12, 12 };

            for (int i = 0; i < radixValues.Length; i++)
            {
                try
                {
                    ff3 = new FF3(radixValues[i]);
                }
                catch (ArgumentException e)
                {
                    // shouldn't happen unless the values of Constants.MINRADIX and
                    // Constants.MAXRADIX have changed, and if they have we'll skip
                    // this radix value
                    continue;
                }

                int minlen = ff3.getMinlen();
                int maxlen = ff3.getMaxlen();

                Assert.IsTrue(2 <= minlen);
                Assert.IsTrue(minlen <= maxlen);
                Assert.IsTrue(maxlen <= 2 * Common.floor(Math.Log(Math.Pow(2, 96)) / Math.Log(radixValues[i])));
                Assert.AreEqual(expectedMinlen[i], minlen);
                Assert.AreEqual(expectedMaxlen[i], maxlen);
            }

            // radix too small
            try
            {
                new FF3(Constants.MINRADIX - 1);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // radix too big
            try
            {
                new FF3(Constants.MAXRADIX + 1);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }
        }

        /**
	     * Test method for {@link org.fpe4j.FF3#encrypt(SecretKey, byte[], int[])}.
	     * 
	     * @throws ArgumentException
	     *             Only if there's a programming error in the test case.
	     */
        [TestMethod]
        public void testEncrypt()
        {
            int radix = 8;

            FF3 ff3 = new FF3(radix);
            Assert.IsNotNull(ff3);

            // set up generic test inputs
            byte[] validKey = { (byte) 0x2B, (byte) 0x7E, (byte) 0x15, (byte) 0x16, (byte) 0x28, (byte) 0xAE, (byte) 0xD2,
                    (byte) 0xA6, (byte) 0xAB, (byte) 0xF7, (byte) 0x15, (byte) 0x88, (byte) 0x09, (byte) 0xCF, (byte) 0x4F,
                    (byte) 0x3C };
            byte[] longKey = { (byte) 0x2B, (byte) 0x7E, (byte) 0x15, (byte) 0x16, (byte) 0x28, (byte) 0xAE, (byte) 0xD2,
                    (byte) 0xA6, (byte) 0xAB, (byte) 0xF7, (byte) 0x15, (byte) 0x88, (byte) 0x09, (byte) 0xCF, (byte) 0x4F,
                    (byte) 0x3C, (byte) 0xAB, (byte) 0xAB, (byte) 0xAB, (byte) 0xAB };
            byte[] shortKey = { (byte) 0x2B, (byte) 0x7E, (byte) 0x15, (byte) 0x16, (byte) 0x28, (byte) 0xAE, (byte) 0xD2,
                    (byte) 0xA6, (byte) 0x88, (byte) 0x09, (byte) 0xCF, (byte) 0x4F, (byte) 0x3C };

            int[] plainText = { 0, 1, 2, 3, 4, 5, 6, 7 };

            // null inputs
            try
            {
                byte[] T = null;
                int[] PT = plainText;
                ff3.encrypt(validKey, T, PT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }
            try
            {
                byte[] T = { 0, 1, 2, 3, 4, 5, 6, 7 };
                int[] PT = null;
                ff3.encrypt(validKey, T, PT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            // key is too short
            try
            {
                byte[] T = { 0, 1, 2, 3, 4, 5, 6, 7 };
                int[] PT = plainText;
                ff3.encrypt(shortKey, T, PT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // key is too long
            try
            {
                byte[] T = { 0, 1, 2, 3, 4, 5, 6, 7 };
                int[] PT = plainText;
                ff3.encrypt(longKey, T, PT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // T is too short
            try
            {
                byte[] T = { 0, 1, 2, 3, 4, 5, 6 };
                int[] PT = plainText;
                ff3.encrypt(validKey, T, PT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // T is too long
            try
            {
                byte[] T = { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
                int[] PT = plainText;
                ff3.encrypt(validKey, T, PT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // X is too short
            try
            {
                byte[] T = { 0, 1, 2, 3, 4, 5, 6, 7 };
                int[] PT = new int[ff3.getMinlen() - 1];
                ff3.encrypt(validKey, T, PT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // X is too long
            try
            {
                byte[] T = { 0, 1, 2, 3, 4, 5, 6, 7 };
                int[] PT = new int[ff3.getMaxlen() + 1];
                ff3.encrypt(validKey, T, PT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // encrypt
            try
            {
                byte[] T = { 0, 1, 2, 3, 4, 5, 6, 7 };
                int[] PT = new int[ff3.getMinlen()];
                int[] CT = ff3.encrypt(validKey, T, PT);
                CollectionAssert.AreEqual(PT, ff3.decrypt(validKey, T, CT));
                PT = new int[ff3.getMaxlen()];
                CT = ff3.encrypt(validKey, T, PT);
                CollectionAssert.AreEqual(PT, ff3.decrypt(validKey, T, CT));
            }
            catch (Exception e)
            {
                Assert.Fail();
            }


        }

        /**
	     * Test method for {@link org.fpe4j.FF3#decrypt(SecretKey, byte[], int[])}.
	     * 
	     * @throws ArgumentException
	     *             Only if there's a programming error in the test case.
	     */
        [TestMethod]
        public void testDecrypt()
        {
            int radix = 8;

            FF3 ff3 = new FF3(radix);
            Assert.IsNotNull(ff3);

            // set up generic test inputs
            byte[] validKey = { (byte) 0x2B, (byte) 0x7E, (byte) 0x15, (byte) 0x16, (byte) 0x28, (byte) 0xAE, (byte) 0xD2,
                    (byte) 0xA6, (byte) 0xAB, (byte) 0xF7, (byte) 0x15, (byte) 0x88, (byte) 0x09, (byte) 0xCF, (byte) 0x4F,
                    (byte) 0x3C };
            byte[] longKey = { (byte) 0x2B, (byte) 0x7E, (byte) 0x15, (byte) 0x16, (byte) 0x28, (byte) 0xAE, (byte) 0xD2,
                    (byte) 0xA6, (byte) 0xAB, (byte) 0xF7, (byte) 0x15, (byte) 0x88, (byte) 0x09, (byte) 0xCF, (byte) 0x4F,
                    (byte) 0x3C, (byte) 0xAB, (byte) 0xAB, (byte) 0xAB, (byte) 0xAB };
            byte[] shortKey = { (byte) 0x2B, (byte) 0x7E, (byte) 0x15, (byte) 0x16, (byte) 0x28, (byte) 0xAE, (byte) 0xD2,
                    (byte) 0xA6, (byte) 0x88, (byte) 0x09, (byte) 0xCF, (byte) 0x4F, (byte) 0x3C };

            int[] cipherText = { 0, 1, 2, 3, 4, 5, 6, 7 };

            // null inputs
            try
            {
                byte[] T = null;
                int[] CT = cipherText;
                ff3.decrypt(validKey, T, CT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }
            try
            {
                byte[] T = { 0, 1, 2, 3, 4, 5, 6, 7 };
                int[] CT = null;
                ff3.decrypt(validKey, T, CT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            // Key is too long
            try
            {
                byte[] T = { 0, 1, 2, 3, 4, 5, 6, 7 };
                int[] CT = cipherText;
                ff3.decrypt(longKey, T, CT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // Key is too short
            try
            {
                byte[] T = { 0, 1, 2, 3, 4, 5, 6, 7 };
                int[] CT = cipherText;
                ff3.decrypt(shortKey, T, CT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // T is too short
            try
            {
                byte[] T = { 0, 1, 2, 3, 4, 5, 6 };
                int[] CT = cipherText;
                ff3.decrypt(validKey, T, CT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // T is too long
            try
            {
                byte[] T = { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
                int[] CT = cipherText;
                ff3.decrypt(validKey, T, CT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // X is too short
            try
            {
                byte[] T = { 0, 1, 2, 3, 4, 5, 6, 7 };
                int[] CT = new int[ff3.getMinlen() - 1];
                ff3.decrypt(validKey, T, CT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // X is too long
            try
            {
                byte[] T = { 0, 1, 2, 3, 4, 5, 6, 7 };
                int[] CT = new int[ff3.getMaxlen() + 1];
                ff3.decrypt(validKey, T, CT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // decrypt
            try
            {
                byte[] T = { 0, 1, 2, 3, 4, 5, 6, 7 };
                int[] CT = new int[ff3.getMinlen()];
                int[] PT = ff3.decrypt(validKey, T, CT);
                CollectionAssert.AreEqual(CT, ff3.encrypt(validKey, T, PT));
                CT = new int[ff3.getMaxlen()];
                PT = ff3.decrypt(validKey, T, CT);
                CollectionAssert.AreEqual(CT, ff3.encrypt(validKey, T, PT));
            }
            catch (Exception e)
            {
                Assert.Fail();
            }

        }

        /**
	     * Stress test for encrypt() and decrypt() methods.
	     * 
	     * This test exercises the encrypt and decrypt methods with radix values
	     * from {@value org.fpe4j.Constants#MINRADIX} to
	     * {@value org.fpe4j.Constants#MAXRADIX} with each of the permitted input
	     * lengths and key sizes.
	     * 
	     * @throws ArgumentException
	     *             Only if there's a programming error in the test case.
	     */
        [TestMethod]
        public void testStress()
        {
            byte[] validKey = { (byte) 0x2B, (byte) 0x7E, (byte) 0x15, (byte) 0x16, (byte) 0x28, (byte) 0xAE, (byte) 0xD2,
                    (byte) 0xA6, (byte) 0xAB, (byte) 0xF7, (byte) 0x15, (byte) 0x88, (byte) 0x09, (byte) 0xCF, (byte) 0x4F,
                    (byte) 0x3C };
            // for each radix power of 2
            for (int j = Constants.MINRADIX; j <= Constants.MAXRADIX; j *= 2)
            {

                FF3 ff3 = new FF3(j);

                // for each permitted plaintext length
                for (int i = ff3.getMinlen(); i <= ff3.getMaxlen(); i++)
                {

                    int[] PT = new int[i];

                    // create a new tweak array
                    byte[] T = Common.bytestring(i, 8);

                    // encrypt the plaintext
                    int[] CT = ff3.encrypt(validKey, T, PT);

                    // verify decrypted ciphertext against original plaintext
                    CollectionAssert.AreEqual(PT, ff3.decrypt(validKey, T, CT));

                    // use the ciphertext as the new plaintext
                    PT = CT;
                }
            }
        }
    }
}
