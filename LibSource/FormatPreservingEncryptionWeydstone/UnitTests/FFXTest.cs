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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FormatPreservingEncryptionWeydstone;

namespace FPETests
{
    [TestClass]
    public class FFXTest
    {
        public FFXTest()
        {
        }

        /**
        * Template class for FFX test parameters.
        * 
        * @author Kai Johnson
        *
        */
        private class FFXTestParameters : FFXParameters
        {

            public int radix;
            public int minlen;
            public int maxlen;
            public int minTlen;
            public int maxTlen;
            public ArithmeticFunction arithmeticFunction;
            public FeistelMethod feistelMethod;
            public SplitFunction splitFunction;
            public RoundCounter roundCounter;
            public RoundFunction roundFunction;

            public FFXTestParameters()
            {
            }


            public int getRadix()
            {
                return radix;
            }


            public int getMinLen()
            {
                return minlen;
            }


            public int getMaxLen()
            {
                return maxlen;
            }


            public int getMinTLen()
            {
                return minTlen;
            }


            public int getMaxTLen()
            {
                return maxTlen;
            }


            public ArithmeticFunction getArithmeticFunction()
            {
                return arithmeticFunction;
            }


            public FeistelMethod getFeistelMethod()
            {
                return feistelMethod;
            }


            public SplitFunction getSplitter()
            {
                return splitFunction;
            }


            public RoundCounter getRoundCounter()
            {
                return roundCounter;
            }


            public RoundFunction getRoundFunction()
            {
                return roundFunction;
            }
        }

        /**
         * Split function for FFX tests.
         */
        private SplitFunction splitFunction1 = new SplitFunction1();
        class SplitFunction1 : SplitFunction
        {
            public int split(int n)
            {
                return n / 2;
            }
        }


        /**
         * Round counter function that returns a sufficient number of rounds.
         */
        private RoundCounter roundCounter1 = new RoundCounter1();
        class RoundCounter1 : RoundCounter
        {
            public int rnds(int n)
            {
                return 10;
            }
        }

        /**
         * Round counter function that returns an insufficient number of rounds.
         */
        private RoundCounter roundCounter2 = new RoundCounter2();
        class RoundCounter2 : RoundCounter
        {
            public int rnds(int n)
            {
                return 7;
            }
        }

        /**
         * Simple unciphered round function for FFX tests.
         */
        private RoundFunction roundFunction1 = new RoundFunction1();
        class RoundFunction1 : RoundFunction
        {
            public bool validKey(byte[] K)
            {
                //TODO
                //// validate K
                //if (K == null)
                //    return false;
                //if (!K.getAlgorithm().equals("AES"))
                //    return false;
                //if (!K.getFormat().equals("RAW"))
                //    return false;

                return true;
            }
            public int[] F(byte[] K, int n, byte[] T, int i, int[] B)
            {
                //TODO changed to:
                //int[] Y = new int[n - B.Length];
                //Array.Fill(Y, 1);

                int[] Y = Enumerable.Repeat(1, n - B.Length).ToArray();

                return Y;
            }
        }
        /**
	     * Test method for
	     * {@link org.fpe4j.FFX#getBlockwiseArithmeticFunction(int)}.
	     */
        [TestMethod]
        public void testGetBlockwiseArithmeticFunction()
        {
            // radix is too small
            try
            {
                FFX.getBlockwiseArithmeticFunction(Constants.MINRADIX - 1);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            ArithmeticFunction arithmeticFunction = FFX.getBlockwiseArithmeticFunction(10);

            // X is null
            try
            {
                arithmeticFunction.Add(null, new int[5]);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }
            try
            {
                arithmeticFunction.Subtract(null, new int[5]);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            // X is empty
            try
            {
                arithmeticFunction.Add(new int[0], new int[5]);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }
            try
            {
                arithmeticFunction.Subtract(new int[0], new int[5]);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // Y is null
            try
            {
                arithmeticFunction.Add(new int[5], null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }
            try
            {
                arithmeticFunction.Subtract(new int[5], null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            // Y is empty
            try
            {
                arithmeticFunction.Add(new int[5], new int[0]);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }
            try
            {
                arithmeticFunction.Subtract(new int[5], new int[0]);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // X.length != Y.length
            try
            {
                arithmeticFunction.Add(new int[5], new int[4]);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }
            try
            {
                arithmeticFunction.Subtract(new int[5], new int[4]);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            int[] X = { 8, 8, 8, 8, 8 };
            int[] Y = { 2, 2, 2, 2, 2 };

            // addition with overflow
            int[] Z1 = { 1, 1, 1, 1, 0 };
            CollectionAssert.AreEqual(Z1, arithmeticFunction.Add(X, Y));

            // Subtraction
            int[] Z2 = { 6, 6, 6, 6, 6 };
            CollectionAssert.AreEqual(Z2, arithmeticFunction.Subtract(X, Y));

            // Subtraction with underflow
            int[] Z3 = { 3, 3, 3, 3, 4 };
            CollectionAssert.AreEqual(Z3, arithmeticFunction.Subtract(Y, X));

            // addition
            int[] Z4 = { 4, 4, 4, 4, 4 };
            CollectionAssert.AreEqual(Z4, arithmeticFunction.Add(Y, Y));
        }

        /**
         * Test method for {@link org.fpe4j.FFX#getCharwiseArithmeticFunction(int)}.
         */
        [TestMethod]
        public void testGetCharwiseArithmeticFunction()
        {
            // radix is too small
            try
            {
                FFX.getCharwiseArithmeticFunction(Constants.MINRADIX - 1);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            ArithmeticFunction arithmeticFunction = FFX.getCharwiseArithmeticFunction(10);

            // X is null
            try
            {
                arithmeticFunction.Add(null, new int[5]);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }
            try
            {
                arithmeticFunction.Subtract(null, new int[5]);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            // X is empty
            try
            {
                arithmeticFunction.Add(new int[0], new int[5]);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }
            try
            {
                arithmeticFunction.Subtract(new int[0], new int[5]);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // Y is null
            try
            {
                arithmeticFunction.Add(new int[5], null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }
            try
            {
                arithmeticFunction.Subtract(new int[5], null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            // Y is empty
            try
            {
                arithmeticFunction.Add(new int[5], new int[0]);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }
            try
            {
                arithmeticFunction.Subtract(new int[5], new int[0]);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // X.length != Y.length
            try
            {
                arithmeticFunction.Add(new int[5], new int[4]);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }
            try
            {
                arithmeticFunction.Subtract(new int[5], new int[4]);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            int[] X = { 8, 8, 8, 8, 8 };
            int[] Y = { 2, 2, 2, 2, 2 };

            // Addition with overflow
            int[] Z1 = { 0, 0, 0, 0, 0 };
            CollectionAssert.AreEqual(Z1, arithmeticFunction.Add(X, Y));

            // subtraction
            int[] Z2 = { 6, 6, 6, 6, 6 };
            CollectionAssert.AreEqual(Z2, arithmeticFunction.Subtract(X, Y));

            // subtraction with underflow
            int[] Z3 = { 4, 4, 4, 4, 4 };
            CollectionAssert.AreEqual(Z3, arithmeticFunction.Subtract(Y, X));

            // Addition
            int[] Z4 = { 4, 4, 4, 4, 4 };
            CollectionAssert.AreEqual(Z4, arithmeticFunction.Add(Y, Y));
        }

        /**
         * Test method for
         * {@link org.fpe4j.FFX#FFX(int, int, int, int, boolean, org.fpe4j.FFX.FeistelMethod, org.fpe4j.FFX.SplitFunction, org.fpe4j.FFX.RoundCounter, org.fpe4j.FFX.RoundFunction)}.
         */
        [TestMethod]
        public void testFFXIntIntIntIntBooleanFeistelMethodSplitFunctionRoundCounterRoundFunction()
        {

            // radix is too small
            try
            {
                new FFX(Constants.MINRADIX - 1, Constants.MINLEN, Constants.MAXLEN, Constants.MAXLEN, false,
                  FeistelMethod.ONE, splitFunction1, roundCounter1, roundFunction1);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // minlen is too small
            try
            {
                new FFX(Constants.MINRADIX, Constants.MINLEN - 1, Constants.MAXLEN, Constants.MAXLEN, false,
                  FeistelMethod.ONE, splitFunction1, roundCounter1, roundFunction1);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // radix^minlen is too small
            try
            {
                new FFX(Constants.MINRADIX, 6, Constants.MAXLEN, Constants.MAXLEN, false, FeistelMethod.ONE, splitFunction1,
                  roundCounter1, roundFunction1);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // maxlen is too small
            try
            {
                new FFX(Constants.MINRADIX, 7, Constants.MINLEN, Constants.MAXLEN, false, FeistelMethod.ONE, splitFunction1,
                  roundCounter1, roundFunction1);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // maxTlen is too small
            try
            {
                new FFX(Constants.MINRADIX, 7, Constants.MAXLEN, -1, false, FeistelMethod.ONE, splitFunction1,
                  roundCounter1, roundFunction1);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // method is null
            //TODO null invalid
            //try
            //{
            //    new FFX(Constants.MINRADIX, 7, Constants.MAXLEN, Constants.MAXLEN, false, null, splitFunction1,
            //      roundCounter1, roundFunction1);
            //    Assert.Fail();
            //}
            //catch (Exception e)
            //{
            //    Assert.IsTrue(e is NullReferenceException);
            //}

            // split function is null
            try
            {
                new FFX(Constants.MINRADIX, 7, Constants.MAXLEN, Constants.MAXLEN, false, FeistelMethod.ONE, null,
                  roundCounter1, roundFunction1);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            // round count function is null
            try
            {
                new FFX(Constants.MINRADIX, 7, Constants.MAXLEN, Constants.MAXLEN, false, FeistelMethod.ONE, splitFunction1,
                  null, roundFunction1);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            // round function is null
            try
            {
                new FFX(Constants.MINRADIX, 7, Constants.MAXLEN, Constants.MAXLEN, false, FeistelMethod.ONE, splitFunction1,
                  roundCounter1, null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            // blockwise
            new FFX(Constants.MINRADIX, 7, Constants.MAXLEN, Constants.MAXLEN, true, FeistelMethod.ONE, splitFunction1,
              roundCounter1, roundFunction1);

            // charwise
            new FFX(Constants.MINRADIX, 7, Constants.MAXLEN, Constants.MAXLEN, false, FeistelMethod.ONE, splitFunction1,
              roundCounter1, roundFunction1);
        }

        /**
         * Test method for {@link org.fpe4j.FFX#FFX(org.fpe4j.FFX.FFXParameters)}.
         */
        [TestMethod]
        public void testFFXFFXParameters()
        {

            // params is null
            try
            {
                new FFX(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            FFXTestParameters ffxParams = new FFXTestParameters();

            ffxParams.radix = Constants.MINRADIX;
            ffxParams.minlen = Constants.MINLEN;
            ffxParams.maxlen = Constants.MAXLEN;
            ffxParams.minTlen = 0;
            ffxParams.maxTlen = Constants.MAXLEN;
            ffxParams.arithmeticFunction = FFX.getBlockwiseArithmeticFunction(ffxParams.radix);
            ffxParams.feistelMethod = FeistelMethod.ONE;
            ffxParams.splitFunction = splitFunction1;
            ffxParams.roundCounter = roundCounter1;
            ffxParams.roundFunction = roundFunction1;

            // radix is too small
            try
            {
                ffxParams.radix = Constants.MINRADIX - 1;
                new FFX(ffxParams);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
                ffxParams.radix = Constants.MINRADIX;
            }

            // minlen is too small
            try
            {
                ffxParams.minlen = Constants.MINLEN - 1;
                new FFX(ffxParams);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // radix^minlen is too small
            try
            {
                ffxParams.minlen = 6;
                new FFX(ffxParams);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
                ffxParams.minlen = 7;
            }

            // maxlen is too small
            try
            {
                ffxParams.maxlen = ffxParams.minlen - 1;
                new FFX(ffxParams);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
                ffxParams.maxlen = Constants.MAXLEN;
            }

            // maxTlen is too small
            try
            {
                ffxParams.maxTlen = -1;
                new FFX(ffxParams);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
                ffxParams.maxTlen = Constants.MAXLEN;
            }

            // method is null
            //TODO null not supported
            //try
            //{
            //    ffxParams.feistelMethod = null;
            //    new FFX(ffxParams);
            //    Assert.Fail();
            //}
            //catch (Exception e)
            //{
            //    Assert.IsTrue(e is NullReferenceException);
            //    ffxParams.feistelMethod = FeistelMethod.TWO;
            //}

            // arithmetic function is null
            try
            {
                ffxParams.arithmeticFunction = null;
                new FFX(ffxParams);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
                ffxParams.arithmeticFunction = FFX.getCharwiseArithmeticFunction(ffxParams.radix);
            }

            // split function is null
            try
            {
                ffxParams.splitFunction = null;
                new FFX(ffxParams);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
                ffxParams.splitFunction = splitFunction1;
            }

            // round count function is null
            try
            {
                ffxParams.roundCounter = null;
                new FFX(ffxParams);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
                ffxParams.roundCounter = roundCounter1;
            }

            // round function is null
            try
            {
                ffxParams.roundFunction = null;
                new FFX(ffxParams);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
                ffxParams.roundFunction = roundFunction1;
            }

            FFX ffx = new FFX(ffxParams);
            Assert.IsNotNull(ffx);
        }

        /**
         * Test method for
         * {@link org.fpe4j.FFX#encrypt(javax.crypto.byte[], byte[], int[])}.
         * 
         * @throws ArgumentException
         *             only if there is a programming error in the test
         */
        [TestMethod]
        public void testEncrypt()
        {
            FFXTestParameters ffxParams = new FFXTestParameters();
            ffxParams.radix = 10;
            ffxParams.minlen = 2;
            ffxParams.maxlen = 5;
            ffxParams.minTlen = 5;
            ffxParams.maxTlen = 5;
            ffxParams.arithmeticFunction = FFX.getBlockwiseArithmeticFunction(ffxParams.getRadix());
            ffxParams.feistelMethod = FeistelMethod.ONE;
            ffxParams.splitFunction = splitFunction1;
            ffxParams.roundCounter = roundCounter1;
            ffxParams.roundFunction = roundFunction1;
            FFX ffx = new FFX(ffxParams);

            byte[] validKey = { (byte) 0x2B, (byte) 0x7E, (byte) 0x15, (byte) 0x16, (byte) 0x28, (byte) 0xAE, (byte) 0xD2,
           (byte) 0xA6, (byte) 0xAB, (byte) 0xF7, (byte) 0x15, (byte) 0x88, (byte) 0x09, (byte) 0xCF, (byte) 0x4F,
           (byte) 0x3C };
            byte[] tweak = { 0, 1, 2, 3, 4 };
            int[] plaintext = { 0, 1, 2, 3, 4 };
            int[] ciphertext1 = { 4, 5, 6, 7, 8 };
            int[] ciphertext2 = { 5, 6, 7, 8, 9 };


            // T is null
            try
            {
                byte[] key = validKey;
                byte[] T = null;
                int[] PT = plaintext;
                ffx.encrypt(key, T, PT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            // T is too short
            try
            {
                byte[] key = validKey;
                byte[] T = { 0, 1, 2, 3 };
                int[] PT = plaintext;
                ffx.encrypt(key, T, PT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // T is too long
            try
            {
                byte[] key = validKey;
                byte[] T = { 0, 1, 2, 3, 4, 5 };
                int[] PT = plaintext;
                ffx.encrypt(key, T, PT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // X is null
            try
            {
                byte[] key = validKey;
                byte[] T = tweak;
                int[] PT = null;
                ffx.encrypt(key, T, PT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            // X is too short
            try
            {
                byte[] key = validKey;
                byte[] T = tweak;
                int[] PT = { 0 };
                ffx.encrypt(key, T, PT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // X is too long
            try
            {
                byte[] key = validKey;
                byte[] T = tweak;
                int[] PT = { 0, 1, 2, 3, 4, 5 };
                ffx.encrypt(key, T, PT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // X is too short for radix
            try
            {
                byte[] key = validKey;
                byte[] T = tweak;
                int[] PT = { 0 };
                ffx.encrypt(key, T, PT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // elements of X are not within the range 0..radix
            try
            {
                byte[] key = validKey;
                byte[] T = tweak;
                int[] PT = { 10, 11, 12, 13, 14 };
                ffx.encrypt(key, T, PT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }
            try
            {
                byte[] key = validKey;
                byte[] T = tweak;
                int[] PT = { 0, -1, -2, -3, -4 };
                ffx.encrypt(key, T, PT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // n == 2 * l && method == FeistelMethod.ONE && r < 8
            try
            {
                ffxParams.feistelMethod = FeistelMethod.ONE;
                ffxParams.roundCounter = roundCounter2;
                ffx = new FFX(ffxParams);
                byte[] key = validKey;
                byte[] T = tweak;
                int[] PT = { 0, 1, 2, 3 };
                ffx.encrypt(key, T, PT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // n != 2 * l && method == FeistelMethod.TWO && r < 8
            try
            {
                ffxParams.feistelMethod = FeistelMethod.TWO;
                ffxParams.roundCounter = roundCounter2;
                ffx = new FFX(ffxParams);
                byte[] key = validKey;
                byte[] T = tweak;
                int[] PT = { 0, 1, 2, 3, 4 };
                ffx.encrypt(key, T, PT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // n != 2 * l && method == FeistelMethod.ONE && r < 4 * n / l
            try
            {
                ffxParams.feistelMethod = FeistelMethod.ONE;
                ffxParams.roundCounter = roundCounter2;
                ffx = new FFX(ffxParams);
                byte[] key = validKey;
                byte[] T = tweak;
                int[] PT = { 0, 1, 2, 3, 4 };
                ffx.encrypt(key, T, PT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            byte[] K = validKey;

            // FeistelMethod.ONE
            ffxParams.feistelMethod = FeistelMethod.ONE;
            ffxParams.roundCounter = roundCounter1;
            ffx = new FFX(ffxParams);
            CollectionAssert.AreEqual(ciphertext1, ffx.encrypt(K, tweak, plaintext));

            // FeistelMethod.TWO
            ffxParams.feistelMethod = FeistelMethod.TWO;
            ffxParams.roundCounter = roundCounter1;
            ffx = new FFX(ffxParams);
            CollectionAssert.AreEqual(ciphertext2, ffx.encrypt(K, tweak, plaintext));
        }

        /**
         * Test method for
         * {@link org.fpe4j.FFX#decrypt(javax.crypto.byte[], byte[], int[])}.
         * 
         * @throws ArgumentException
         *             only if there is a programming error in the test
         */
        [TestMethod]
        public void testDecrypt()
        {
            FFXTestParameters ffxParams = new FFXTestParameters();
            ffxParams.radix = 10;
            ffxParams.minlen = 2;
            ffxParams.maxlen = 5;
            ffxParams.minTlen = 5;
            ffxParams.maxTlen = 5;
            ffxParams.arithmeticFunction = FFX.getBlockwiseArithmeticFunction(ffxParams.getRadix());
            ffxParams.feistelMethod = FeistelMethod.ONE;
            ffxParams.splitFunction = splitFunction1;
            ffxParams.roundCounter = roundCounter1;
            ffxParams.roundFunction = roundFunction1;
            FFX ffx = new FFX(ffxParams);

            byte[] validKey = { (byte) 0x2B, (byte) 0x7E, (byte) 0x15, (byte) 0x16, (byte) 0x28, (byte) 0xAE, (byte) 0xD2,
           (byte) 0xA6, (byte) 0xAB, (byte) 0xF7, (byte) 0x15, (byte) 0x88, (byte) 0x09, (byte) 0xCF, (byte) 0x4F,
           (byte) 0x3C };
            byte[] tweak = { 0, 1, 2, 3, 4 };
            int[] plaintext = { 0, 1, 2, 3, 4 };
            int[] ciphertext1 = { 4, 5, 6, 7, 8 };
            int[] ciphertext2 = { 5, 6, 7, 8, 9 };

            // T is null
            try
            {
                byte[] key = validKey;
                byte[] T = null;
                int[] CT = ciphertext1;
                ffx.decrypt(key, T, CT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            // T is too short
            try
            {
                byte[] key = validKey;
                byte[] T = { 0, 1, 2, 3 };
                int[] CT = ciphertext1;
                ffx.decrypt(key, T, CT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // T is too long
            try
            {
                byte[] key = validKey;
                byte[] T = { 0, 1, 2, 3, 4, 5 };
                int[] CT = ciphertext1;
                ffx.decrypt(key, T, CT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // X is null
            try
            {
                byte[] key = validKey;
                byte[] T = tweak;
                int[] CT = null;
                ffx.decrypt(key, T, CT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            // X is too short
            try
            {
                byte[] key = validKey;
                byte[] T = tweak;
                int[] CT = { 0 };
                ffx.decrypt(key, T, CT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // X is too long
            try
            {
                byte[] key = validKey;
                byte[] T = tweak;
                int[] CT = { 0, 1, 2, 3, 4, 5 };
                ffx.decrypt(key, T, CT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // X is too short for radix
            try
            {
                byte[] key = validKey;
                byte[] T = tweak;
                int[] CT = { 0 };
                ffx.decrypt(key, T, CT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // elements of X are not within the range 0..radix
            try
            {
                byte[] key = validKey;
                byte[] T = tweak;
                int[] CT = { 10, 11, 12, 13, 14 };
                ffx.decrypt(key, T, CT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }
            try
            {
                byte[] key = validKey;
                byte[] T = tweak;
                int[] CT = { 0, -1, -2, -3, -4 };
                ffx.decrypt(key, T, CT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // n == 2 * l && method == FeistelMethod.ONE && r < 8
            try
            {
                ffxParams.feistelMethod = FeistelMethod.ONE;
                ffxParams.roundCounter = roundCounter2;
                ffx = new FFX(ffxParams);
                byte[] key = validKey;
                byte[] T = tweak;
                int[] CT = { 0, 1, 2, 3 };
                ffx.decrypt(key, T, CT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // n != 2 * l && method == FeistelMethod.TWO && r < 8
            try
            {
                ffxParams.feistelMethod = FeistelMethod.TWO;
                ffxParams.roundCounter = roundCounter2;
                ffx = new FFX(ffxParams);
                byte[] key = validKey;
                byte[] T = tweak;
                int[] CT = { 0, 1, 2, 3, 4 };
                ffx.decrypt(key, T, CT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // n != 2 * l && method == FeistelMethod.ONE && r < 4 * n / l
            try
            {
                ffxParams.feistelMethod = FeistelMethod.ONE;
                ffxParams.roundCounter = roundCounter2;
                ffx = new FFX(ffxParams);
                byte[] key = validKey;
                byte[] T = tweak;
                int[] CT = { 0, 1, 2, 3, 4 };
                ffx.decrypt(key, T, CT);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            byte[] K = validKey;

            // FeistelMethod.ONE
            ffxParams.feistelMethod = FeistelMethod.ONE;
            ffxParams.roundCounter = roundCounter1;
            ffx = new FFX(ffxParams);
            CollectionAssert.AreEqual(plaintext, ffx.decrypt(K, tweak, ciphertext1));

            // FeistelMethod.TWO
            ffxParams.feistelMethod = FeistelMethod.TWO;
            ffxParams.roundCounter = roundCounter1;
            ffx = new FFX(ffxParams);
            CollectionAssert.AreEqual(plaintext, ffx.decrypt(K, tweak, ciphertext2));
        }
    }
}
