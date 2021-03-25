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
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FormatPreservingEncryptionWeydstone;

namespace FPETests
{
    [TestClass]
    public class CommonTest
    {
        public CommonTest()
        {
        }

        /**
	     * Test method for {@link org.fpe4j.Common#byteArrayToHexString(byte[])}.
	     */
        [TestMethod]
        public void testByteArrayToHexString()
        {
            // null input
            try
            {
                Common.byteArrayToHexString(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            // empty input
            byte[] X1 = { };
            Assert.AreEqual("", Common.byteArrayToHexString(X1));

            // one element
            byte[] X2 = { 1 };
            Assert.AreEqual("01", Common.byteArrayToHexString(X2));

            // many elements
            byte[] X3 = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2,
                3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Assert.AreEqual(
                    "000102030405060708090001020304050607080900010203040506070809000102030405060708090001020304050607080900010203040506070809",
                    Common.byteArrayToHexString(X3));

            // range of values
            byte[] X4 = { (byte)0x00, (byte)0x7F, (byte)0x80, (byte)0xFF };
            Assert.AreEqual("007F80FF", Common.byteArrayToHexString(X4));
        }

        /**
         * Test method for {@link org.fpe4j.Common#intArrayToString(int[])}.
         */
        [TestMethod]
        public void testIntArrayToString()
        {
            // null input
            try
            {
                Common.intArrayToString(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            // empty input
            int[] X1 = { };
            Assert.AreEqual("", Common.intArrayToString(X1));

            // one element
            int[] X2 = { 1 };
            Assert.AreEqual("1", Common.intArrayToString(X2));

            // many elements
            int[] X3 = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2,
                3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Assert.AreEqual(
                    "0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9",
                    Common.intArrayToString(X3));
        }

        /**
         * Test method for
         * {@link org.fpe4j.Common#unsignedByteArrayToString(byte[])}.
         */
        [TestMethod]
        public void testUnsignedByteArrayToString()
        {
            // null input
            try
            {
                Common.unsignedByteArrayToString(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            // empty input
            byte[] X1 = { };
            Assert.AreEqual("[ ]", Common.unsignedByteArrayToString(X1));

            // one element
            byte[] X2 = { 1 };
            Assert.AreEqual("[ 1 ]", Common.unsignedByteArrayToString(X2));

            // many elements
            byte[] X3 = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2,
                3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Assert.AreEqual(
                    "[ 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 ]",
                    Common.unsignedByteArrayToString(X3));

            // range of values
            byte[] X4 = { (byte)0x00, (byte)0x7F, (byte)0x80, (byte)0xFF };
            Assert.AreEqual("[ 0, 127, 128, 255 ]", Common.unsignedByteArrayToString(X4));
        }

        /**
         * Test method for {@link org.fpe4j.Common#num(int[], int)}.
         */
        [TestMethod]
        public void testNumIntArrayInt()
        {
            // example from NIST SP 800-38G
            int[] X1 = { 0, 0, 0, 1, 1, 0, 1, 0 };
            Assert.IsTrue(Common.num(X1, 5).CompareTo(new BigInteger(755)) == 0);

            // null input
            try
            {
                Common.num(null, 10);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            // input array too short
            try
            {
                int[] X = { };
                Common.num(X, 10);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // input array too long
            try
            {
                int[] X = new int[Constants.MAXLEN + 1];
                Common.num(X, 10);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // radix too small
            try
            {
                int[] X = { 0, 1, 2, 3, 4, 5 };
                Common.num(X, 1);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // radix too large
            try
            {
                int[] X = { 0, 1, 2, 3, 4, 5 };
                Common.num(X, 65537);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // values outside the range of the radix
            try
            {
                int[] X = { 0, 1, 2, 3, 4, 5 };
                Common.num(X, 4);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // negative values
            try
            {
                int[] X = { 0, 1, -2, 3, 4, 5 };
                Common.num(X, 4);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // byte value
            int[] X2 = { 1, 1, 1, 1, 1, 1, 1, 1 };
            Assert.IsTrue(Common.num(X2, 2).CompareTo(new BigInteger(255)) == 0);

            // short value
            int[] X3 = { 15, 15, 15, 15 };
            Assert.IsTrue(Common.num(X3, 16).CompareTo(new BigInteger(65535)) == 0);

            // int value
            int[] X4 = { 127, 255, 255, 255 };
            Assert.IsTrue(Common.num(X4, 256).CompareTo(new BigInteger(int.MaxValue)) == 0);

            // long value
            int[] X5 = { 255, 255, 255, 255 };
            Assert.IsTrue(Common.num(X5, 256).CompareTo(new BigInteger(4294967295L)) == 0);

            // yotta
            int[] X6 = { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            Assert.IsTrue(Common.num(X6, 256).CompareTo(BigInteger.Pow(2,80)) == 0);
        }

        /**
         * test method for {@link org.fpe4j.Common#num(byte[])}.
         */
        [TestMethod]
        public void testnumbytearray()
        {
            // null input
            try
            {
                byte[] x = null;
                Common.num(x);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            // input array too short
            try
            {
                byte[] x = { };
                Common.num(x);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // input array too long
            try
            {
                byte[] x = new byte[Constants.MAXLEN + 1];
                Common.num(x);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // one byte values
            byte[] x1 = { (byte)0x00 };
            Assert.IsTrue(new BigInteger(0).Equals(Common.num(x1)));
            byte[] x2 = { (byte)0x01 };
            Assert.IsTrue(new BigInteger(1).Equals(Common.num(x2)));
            byte[] x3 = { (byte)0x80 };
            Assert.IsTrue(new BigInteger(128).Equals(Common.num(x3)));
            byte[] x4 = { (byte)0xff };
            Assert.IsTrue(new BigInteger(255).Equals(Common.num(x4)));

            // two byte values
            byte[] x5 = { (byte)0x00, (byte)0x00 };
            Assert.IsTrue(new BigInteger(0).Equals(Common.num(x5)));
            byte[] x6 = { (byte)0x00, (byte)0x01 };
            Assert.IsTrue(new BigInteger(1).Equals(Common.num(x6)));
            byte[] x7 = { (byte)0x80, (byte)0x00 };
            Assert.IsTrue(new BigInteger(32768).Equals(Common.num(x7)));
            byte[] x8 = { (byte)0xff, (byte)0xff };
            Assert.IsTrue(new BigInteger(65535).Equals(Common.num(x8)));

            // four byte values
            byte[] x9 = { (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00 };
            Assert.IsTrue(new BigInteger(0).Equals(Common.num(x9)));
            byte[] x10 = { (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x01 };
            Assert.IsTrue(new BigInteger(1).Equals(Common.num(x10)));
            byte[] x11 = { (byte)0x80, (byte)0x00, (byte)0x00, (byte)0x00 };
            Assert.IsTrue(new BigInteger(2147483648l).Equals(Common.num(x11)));
            byte[] x12 = { (byte)0xff, (byte)0xff, (byte)0xff, (byte)0xff };
            Assert.IsTrue(new BigInteger(4294967295l).Equals(Common.num(x12)));

            // yotta
            byte[] x13 = { (byte) 0x01, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00,
                (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00 };
            Assert.IsTrue(BigInteger.Pow(2,80).Equals(Common.num(x13)));
        }

        /**
         * test method for
         * {@link org.fpe4j.common#str(java.math.biginteger, int, int)}.
         */
        [TestMethod]
        public void teststr()
        {
            // example from nist sp 800-38g
            int[] expected1 = { 0, 3, 10, 7 };
            CollectionAssert.AreEqual(expected1, Common.str(new BigInteger(559), 12, 4));

            // m is too small
            try
            {
                Common.str(BigInteger.One, 10, 0);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // m is too small
            try
            {
                Common.str(BigInteger.One, 10, Constants.MAXLEN + 1);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // radix is too small
            try
            {
                Common.str(BigInteger.One, 1, 4);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // radix is too large
            try
            {
                Common.str(BigInteger.One, 65537, 4);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // null input
            //[TODO]try
            //{
            //    Common.str(null, 10, 4);
            //    Assert.Fail();
            //}
            //catch (Exception e)
            //{
            //    Assert.IsTrue(e is NullReferenceException);
            //}

            // x is negative
            try
            {
                Common.str(BigInteger.MinusOne, 10, 4);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // x is too large
            try
            {
                int[] x = { 0, 0, 0, 0 };
                CollectionAssert.AreEqual(x, Common.str(BigInteger.Pow(10, 4), 10, 4));
                Assert.Fail();

            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
                //Assert.Fail();
            }
            /*
             * note that this test case is modified to accommodate the ffx
             * algorithms.
             */

            // byte value
            int[] x2 = { 1, 1, 1, 1, 1, 1, 1, 1 };
            CollectionAssert.AreEqual(x2, Common.str(new BigInteger(255), 2, 8));

            // short value
            int[] x3 = { 15, 15, 15, 15 };
            CollectionAssert.AreEqual(x3, Common.str(new BigInteger(65535), 16, 4));

            // int value
            int[] x4 = { 127, 255, 255, 255 };
            CollectionAssert.AreEqual(x4, Common.str(new BigInteger(int.MaxValue), 256, 4));

            // long value
            int[] x5 = { 255, 255, 255, 255 };
            CollectionAssert.AreEqual(x5, Common.str(new BigInteger(4294967295l), 256, 4));

            // yotta
            int[] x6 = { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            CollectionAssert.AreEqual(x6, Common.str(BigInteger.Pow(2, 80), 256, 11));
        }

        /**
         * Test method for {@link org.fpe4j.Common#rev(int[])}.
         */
        [TestMethod]
        public void testRev()
        {
            // example from NIST SP 800-38G
            int[] X1 = { 1, 3, 5, 7, 9 };
            int[] Y1 = { 9, 7, 5, 3, 1 };
            CollectionAssert.AreEqual(Y1, Common.rev(X1));

            // null input
            try
            {
                Common.rev(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            // empty array
            int[] X2 = { };
            int[] Y2 = { };
            CollectionAssert.AreEqual(Y2, Common.rev(X2));

            // one element
            int[] X3 = { 5 };
            int[] Y3 = { 5 };
            CollectionAssert.AreEqual(Y3, Common.rev(X3));

            // many elements
            int[] X4 = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2,
                3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            int[] Y4 = { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 9, 8, 7,
                6, 5, 4, 3, 2, 1, 0, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };
            CollectionAssert.AreEqual(Y4, Common.rev(X4));
        }

        /**
         * Test method for {@link org.fpe4j.Common#revb(byte[])}.
         */
        [TestMethod]
        public void testRevb()
        {
            // example from NIST SP 800-38G
            byte[] X1 = { (byte)1, (byte)2, (byte)3 };
            byte[] Y1 = { (byte)3, (byte)2, (byte)1 };
            CollectionAssert.AreEqual(Y1, Common.revb(X1));

            // null input
            try
            {
                Common.rev(null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            // empty array
            byte[] X2 = { };
            byte[] Y2 = { };
            CollectionAssert.AreEqual(Y2, Common.revb(X2));

            // one element
            byte[] X3 = { 5 };
            byte[] Y3 = { 5 };
            CollectionAssert.AreEqual(Y3, Common.revb(X3));

            // many elements
            byte[] X4 = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2,
                3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            byte[] Y4 = { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0, 9, 8, 7,
                6, 5, 4, 3, 2, 1, 0, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };
            CollectionAssert.AreEqual(Y4, Common.revb(X4));
        }

        /**
         * Test method for {@link org.fpe4j.Common#xor(byte[], byte[])}.
         */
        [TestMethod]
        public void testXor()
        {
            // example from NIST SP 800-38G
            byte[] X1 = { (byte)0x13 };
            byte[] Y1 = { (byte)0x15 };
            byte[] Z1 = { (byte)0x06 };
            CollectionAssert.AreEqual(Z1, Common.xor(X1, Y1));

            // null input
            try
            {
                byte[] X = null;
                byte[] Y = { (byte)0xA5 };
                Common.xor(X, Y);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }
            try
            {
                byte[] X = { (byte)0x0F };
                byte[] Y = null;
                Common.xor(X, Y);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            // mismatched length
            try
            {
                byte[] X = { (byte)0x0F, (byte)0xF0 };
                byte[] Y = { (byte)0xA5 };
                Common.xor(X, Y);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // empty arrays
            try
            {
                byte[] X = { };
                byte[] Y = { (byte)0xA5 };
                Common.xor(X, Y);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }
            try
            {
                byte[] X = { (byte)0x0F };
                byte[] Y = { };
                Common.xor(X, Y);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // arrays too long
            try
            {
                byte[] X = new byte[Constants.MAXLEN + 1];
                byte[] Y = { (byte)0xA5 };
                Common.xor(X, Y);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }
            try
            {
                byte[] X = { (byte)0x0F };
                byte[] Y = new byte[Constants.MAXLEN + 1];
                Common.xor(X, Y);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // one element
            byte[] X2 = { (byte)0x0F };
            byte[] Y2 = { (byte)0xA5 };
            byte[] Z2 = { (byte)0xAA };
            CollectionAssert.AreEqual(Z2, Common.xor(X2, Y2));

            // many elements
            byte[] X3 = { (byte)0x0F, (byte)0xF0, (byte)0xFF, (byte)0x00 };
            byte[] Y3 = { (byte)0xA5, (byte)0xA5, (byte)0xA5, (byte)0xA5 };
            byte[] Z3 = { (byte)0xAA, (byte)0x55, (byte)0x5A, (byte)0xA5 };
            CollectionAssert.AreEqual(Z3, Common.xor(X3, Y3));
        }

        /**
         * Test method for {@link org.fpe4j.Common#log2(int)}.
         */
        [TestMethod]
        public void testLog2()
        {
            // examples from NIST SP 800-38G
            Assert.IsTrue(Common.log2(64) == 6);
            Assert.IsTrue(Common.log2(10) == Math.Log(10, 2));

            // negative
            try
            {
                Common.log2(-1);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // zero
            try
            {
                Common.log2(0);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // integer result
            Assert.IsTrue(Common.log2(1024) == 10);

            // real result
            Assert.IsTrue(Common.log2(1023) < 10);
            Assert.IsTrue(Common.log2(1025) > 10);
        }

        /**
         * Test method for {@link org.fpe4j.Common#floor(double)}.
         */
        [TestMethod]
        public void testFloor()
        {
            // examples from NIST SP 800-38G
            Assert.AreEqual(2, Common.floor(2.1));
            Assert.AreEqual(4, Common.floor((double)4));

            // correct usage
            Assert.AreEqual(2, Common.floor(7 / (double)3));
            Assert.AreEqual(2, Common.floor(7 / 3.0));

            // incorrect usage
            try
            {
                Assert.AreEqual(2, Common.floor(7 / 3));
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // native integer division
            Assert.AreEqual(2, 7 / 3);
        }

        /**
         * Test method for {@link org.fpe4j.Common#ceiling(double)}.
         */
        [TestMethod]
        public void testCeiling()
        {
            // examples from NIST SP 800-38G
            Assert.AreEqual(3, Common.ceiling(2.1));
            Assert.AreEqual(4, Common.ceiling((double)4));

            // correct usage
            Assert.AreEqual(3, Common.ceiling(7 / (double)3));
            Assert.AreEqual(3, Common.ceiling(7 / 3.0));

            // incorrect usage
            try
            {
                Assert.AreEqual(3, Common.ceiling(7 / 3));
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }
        }

        /**
         * Test method for {@link org.fpe4j.Common#mod(int, int)}.
         */
        [TestMethod]
        public void testModIntInt()
        {
            // examples from NIST SP 800-38G
            Assert.AreEqual(4, Common.mod(-3, 7));
            Assert.AreEqual(6, Common.mod(13, 7));

            // negative modulus
            try
            {
                Common.mod(13, -7);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArithmeticException);
            }
            //Assert.AreEqual(-1, Math.floorMod(13, -7));
            /*
             * Note that Math.floorMod() permits negative moduli where NIST SP
             * 800-38G does not.
             */

            // zero modulus
            try
            {
                Common.mod(13, 0);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArithmeticException);
            }

        }

        /**
         * Test method for {@link org.fpe4j.Common#mod(BigInteger, BigInteger)}.
         */
        [TestMethod]
        public void testModBigIntegerBigInteger()
        {
            // examples from NIST SP 800-38G
            Assert.IsTrue(Common.mod(new BigInteger(-3), new BigInteger(7)).Equals(new BigInteger(4)));
            Assert.IsTrue(Common.mod(new BigInteger(13), new BigInteger(7)).Equals(new BigInteger(6)));


            // negative modulus
            try
            {
                Common.mod(new BigInteger(13), new BigInteger(-7));
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArithmeticException);
            }

            // zero modulus
            try
            {
                Common.mod(new BigInteger(13), BigInteger.Zero);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArithmeticException);
            }

        }

        /**
         * Test method for {@link org.fpe4j.Common#bytestring(int, int)}.
         */
        [TestMethod]
        public void testBytestringIntInt()
        {
            // example from NIST SP 800-38G
            byte[] expected1 = { (byte)0x01 };
            CollectionAssert.AreEqual(expected1, Common.bytestring(1, 1));

            // s too small
            try
            {
                Common.bytestring(1, -1);
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // s too big
            try
            {
                Common.bytestring(1, Constants.MAXLEN + 1);
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // x too small
            try
            {
                Common.bytestring(-1, 1);
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // zero byte value
            byte[] expected2 = { };
            CollectionAssert.AreEqual(expected2, Common.bytestring(0, 0));

            // one byte value
            byte[] expected3 = { (byte)0xFF };
            CollectionAssert.AreEqual(expected3, Common.bytestring(255, 1));

            // overflow one byte
            byte[] expectedTruncated = { (byte)0x00};
            CollectionAssert.AreEqual(expectedTruncated, Common.bytestring(256, 1));

            // two byte values
            byte[] expected4 = { (byte)0x00, (byte)0x01 };
            CollectionAssert.AreEqual(expected4, Common.bytestring(1, 2));

            byte[] expected5 = { (byte)0xFF, (byte)0xFF };
            CollectionAssert.AreEqual(expected5, Common.bytestring(65535, 2));

            // overflow two bytes

            byte[] expectedTruncated2 = { (byte)0xFF, (byte)0xFF };
            Common.bytestring(65536, 2);
            CollectionAssert.AreEqual(expectedTruncated2, Common.bytestring(65535, 2));


            // extension to 16 bytes
            byte[] expected7 = { (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00,
                (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00,
                (byte) 0x01 };
            CollectionAssert.AreEqual(expected7, Common.bytestring(1, 16));

            byte[] expected8 = { (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00,
                (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x7F, (byte) 0xFF, (byte) 0xFF,
                (byte) 0xFF };
            CollectionAssert.AreEqual(expected8, Common.bytestring(int.MaxValue, 16));
        }

        /**
         * Test method for {@link org.fpe4j.Common#bytestring(BigInteger, int)}.
         */
        [TestMethod]
        public void testBytestringBigIntegerInt()
        {
            // example from NIST SP 800-38G
            byte[] expected1 = { (byte)0x01 };
            Assert.IsTrue(expected1.SequenceEqual(Common.bytestring(BigInteger.One, 1)));

            // s too small
            try
            {
                Common.bytestring(BigInteger.One, 0);
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // x too small
            try
            {
                Common.bytestring(BigInteger.MinusOne, 1);
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // one byte value
            byte[] expected2 = { (byte)0xFF };
            CollectionAssert.AreEqual(expected2, Common.bytestring(new BigInteger(255), 1));

            // overflow one byte
            try
            {
                Common.bytestring(new BigInteger(256), 1);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // two byte values
            byte[] expected4 = { (byte)0x00, (byte)0x01 };
            CollectionAssert.AreEqual(expected4, Common.bytestring(BigInteger.One, 2));

            byte[] expected5 = { (byte)0xFF, (byte)0xFF };
            CollectionAssert.AreEqual(expected5, Common.bytestring(new BigInteger(65535), 2));

            // overflow two bytes
            try
            {
                Common.bytestring(new BigInteger(65536), 2);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // extension to 16 byte values
            byte[] expected7 = { (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00,
                (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00,
                (byte) 0x01 };
            CollectionAssert.AreEqual(expected7, Common.bytestring(BigInteger.One, 16));

            byte[] expected8 = { (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00,
                (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x7F, (byte) 0xFF, (byte) 0xFF,
                (byte) 0xFF };
            CollectionAssert.AreEqual(expected8, Common.bytestring(new BigInteger(int.MaxValue), 16));
        }

        /**
         * Test method for {@link org.fpe4j.Common#bitstring(boolean, int)}.
         */
        [TestMethod]
        public void testBitstring()
        {
            // example from NIST SP 800-38G
            byte[] expected1 = { (byte)0 };
            CollectionAssert.AreEqual(expected1, Common.bitstring(false, 8));

            // s is negative
            try
            {
                Common.bitstring(false, -8);
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // s is not a multiple of 8
            try
            {
                Common.bitstring(false, 4);
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // two byte values
            byte[] expected2 = { (byte)0, (byte)0 };
            CollectionAssert.AreEqual(expected2, Common.bitstring(false, 16));
            byte[] expected3 = { (byte)0xFF, (byte)0xFF };
            CollectionAssert.AreEqual(expected3, Common.bitstring(true, 16));

            // 16 byte values
            byte[] expected4 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            CollectionAssert.AreEqual(expected4, Common.bitstring(false, 128));
            byte[] expected5 = { (byte) 0xFF, (byte) 0xFF, (byte) 0xFF, (byte) 0xFF, (byte) 0xFF, (byte) 0xFF, (byte) 0xFF,
                (byte) 0xFF, (byte) 0xFF, (byte) 0xFF, (byte) 0xFF, (byte) 0xFF, (byte) 0xFF, (byte) 0xFF, (byte) 0xFF,
                (byte) 0xFF };
            CollectionAssert.AreEqual(expected5, Common.bitstring(true, 128));
        }

        /**
         * Test method for {@link org.fpe4j.Common#concatenate(int[], int[])}.
         */
        [TestMethod]
        public void testConcatenateIntArrayIntArray()
        {
            // example from NIST SP 800-38G
            int[] X1 = { 3, 1 };
            int[] Y1 = { 31, 8, 10 };
            int[] Z1 = { 3, 1, 31, 8, 10 };
            CollectionAssert.AreEqual(Z1, Common.concatenate(X1, Y1));

            // null input
            try
            {
                int[] X = { 1, 2, 3 };
                Common.concatenate(X, null);
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }
            try
            {
                int[] Y = { 4, 5, 6 };
                Common.concatenate(null, Y);
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            // empty input
            int[] X2 = { 1, 2, 3 };
            int[] Y2 = { };
            int[] Z2 = { 1, 2, 3 };
            CollectionAssert.AreEqual(Z2, Common.concatenate(X2, Y2));
            int[] X3 = { };
            int[] Y3 = { 4, 5, 6 };
            int[] Z3 = { 4, 5, 6 };
            CollectionAssert.AreEqual(Z3, Common.concatenate(X3, Y3));
        }

        [TestMethod]
        public void TestConcatenateByteArrayByteArray()
        {
            // example from NIST SP 800-38G
            byte[] X1 = { 3, 1 };
            byte[] Y1 = { 31, 8, 10 };
            byte[] Z1 = { 3, 1, 31, 8, 10 };
            CollectionAssert.AreEqual(Z1, Common.concatenate(X1, Y1));
            byte[] result = Common.concatenate(X1, Y1);

            // null input
            try
            {
                byte[] X = { 1, 2, 3 };
                Common.concatenate(X, null);
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is System.NullReferenceException);
            }
            try
            {
                byte[] Y = { 4, 5, 6 };
                Common.concatenate(null, Y);
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            // empty input
            byte[] X2 = { 1, 2, 3 };
            byte[] Y2 = { };
            byte[] Z2 = { 1, 2, 3 };
            Console.WriteLine(Common.concatenate(X2, Y2).ToString());
            CollectionAssert.AreEqual(Z2, Common.concatenate(X2, Y2));
            byte[] X3 = { };
            byte[] Y3 = { 4, 5, 6 };
            byte[] Z3 = { 4, 5, 6 };
            Console.WriteLine(Common.concatenate(X3, Y3).ToString());
            CollectionAssert.AreEqual(Z3, Common.concatenate(X3, Y3));

        }

    }
}
