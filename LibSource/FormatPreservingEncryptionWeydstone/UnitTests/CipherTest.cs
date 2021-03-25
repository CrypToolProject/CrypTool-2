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
    public class CiphersTest
    {


        public CiphersTest()
        {
        }

        /**
	     * Test method for {@link org.fpe4j.Ciphers#Ciphers()}.
	     */
        [TestMethod]
        public void testCiphers()
        {
            Ciphers c = new Ciphers();
            Assert.IsNotNull(c);
        }

        /**
         * Test method for
         * {@link org.fpe4j.Ciphers#prf(javax.crypto.SecretKey, byte[])}.
         */
        [TestMethod]
        public void testPrf()
        {
            Ciphers c = new Ciphers();
            Assert.IsNotNull(c);

            // null inputs
            try
            {
                byte[] K = null;
                byte[] P = { 1, 2, 1, 0, 0, 10, 10, 5, 0, 0, 0, 10, 0, 0, 0, 0 };
                byte[] Q = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (byte)0xDD, (byte)0xD5 };
                c.prf(K, Common.concatenate(P, Q));
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }
            try
            {
                byte[] K = { (byte) 0x2B, (byte) 0x7E, (byte) 0x15, (byte) 0x16, (byte) 0x28, (byte) 0xAE, (byte) 0xD2,
                    (byte) 0xA6, (byte) 0xAB, (byte) 0xF7, (byte) 0x15, (byte) 0x88, (byte) 0x09, (byte) 0xCF,
                    (byte) 0x4F, (byte) 0x3C };
                c.prf(K, null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            // wrong key type
            try
            {
                //TODO
                byte[] K = { (byte)0x2B };
                //KeyGenerator keygen = KeyGenerator.getInstance("DES");
                //SecretKey K = keygen.generateKey();
                byte[] P = { 1, 2, 1, 0, 0, 10, 10, 5, 0, 0, 0, 10, 0, 0, 0, 0 };
                byte[] Q = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (byte)0xDD, (byte)0xD5 };
                c.prf(K, Common.concatenate(P, Q));
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // X is too short
            try
            {
                byte[] K = { (byte) 0x2B, (byte) 0x7E, (byte) 0x15, (byte) 0x16, (byte) 0x28, (byte) 0xAE, (byte) 0xD2,
                    (byte) 0xA6, (byte) 0xAB, (byte) 0xF7, (byte) 0x15, (byte) 0x88, (byte) 0x09, (byte) 0xCF,
                    (byte) 0x4F, (byte) 0x3C };
                byte[] X = { };
                c.prf(K, X);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // X is too long
            try
            {
                byte[] K = { (byte) 0x2B, (byte) 0x7E, (byte) 0x15, (byte) 0x16, (byte) 0x28, (byte) 0xAE, (byte) 0xD2,
                    (byte) 0xA6, (byte) 0xAB, (byte) 0xF7, (byte) 0x15, (byte) 0x88, (byte) 0x09, (byte) 0xCF,
                    (byte) 0x4F, (byte) 0x3C };
                byte[] X = new byte[Constants.MAXLEN + 1];
                c.prf(K, X);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // validation against sample data
            try
            {
                byte[] K = { (byte) 0x2B, (byte) 0x7E, (byte) 0x15, (byte) 0x16, (byte) 0x28, (byte) 0xAE, (byte) 0xD2,
                    (byte) 0xA6, (byte) 0xAB, (byte) 0xF7, (byte) 0x15, (byte) 0x88, (byte) 0x09, (byte) 0xCF,
                    (byte) 0x4F, (byte) 0x3C };
                byte[] P = { 1, 2, 1, 0, 0, 10, 10, 5, 0, 0, 0, 10, 0, 0, 0, 0 };
                byte[] Q = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (byte)0xDD, (byte)0xD5 };
                byte[] R = { (byte) 0xC3, (byte) 0xB8, 41, (byte) 0xA1, (byte) 0xE8, 100, 43, 120, (byte) 0xCC, 41,
                    (byte) 0x94, 123, 59, (byte) 0x93, (byte) 0xDB, 99 };
                CollectionAssert.AreEqual(R, c.prf(K, Common.concatenate(P, Q)));
            }
            catch (ArgumentException e)
            {
                Assert.Fail(e.ToString());
            }
        }

        /**
         * Test method for
         * {@link org.fpe4j.Ciphers#prf2(javax.crypto.SecretKey, byte[])}.
         */
        [TestMethod]
        public void testPrf2()
        {
            Ciphers c = new Ciphers();
            Assert.IsNotNull(c);

            // null inputs
            try
            {
                byte[] K = null;
                byte[] P = { 1, 2, 1, 0, 0, 10, 10, 5, 0, 0, 0, 10, 0, 0, 0, 0 };
                byte[] Q = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (byte)0xDD, (byte)0xD5 };
                c.prf2(K, Common.concatenate(P, Q));
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }
            try
            {
                byte[] K = { (byte) 0x2B, (byte) 0x7E, (byte) 0x15, (byte) 0x16, (byte) 0x28, (byte) 0xAE, (byte) 0xD2,
                    (byte) 0xA6, (byte) 0xAB, (byte) 0xF7, (byte) 0x15, (byte) 0x88, (byte) 0x09, (byte) 0xCF,
                    (byte) 0x4F, (byte) 0x3C };
                c.prf2(K, null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            // wrong key type
            try
            {
                byte[] K = { (byte)0x2B };
                //KeyGenerator keygen = KeyGenerator.getInstance("DES");
                //SecretKey K = keygen.generateKey();
                byte[] P = { 1, 2, 1, 0, 0, 10, 10, 5, 0, 0, 0, 10, 0, 0, 0, 0 };
                byte[] Q = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (byte)0xDD, (byte)0xD5 };
                c.prf2(K, Common.concatenate(P, Q));
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // X is too short
            try
            {
                byte[] K = { (byte) 0x2B, (byte) 0x7E, (byte) 0x15, (byte) 0x16, (byte) 0x28, (byte) 0xAE, (byte) 0xD2,
                    (byte) 0xA6, (byte) 0xAB, (byte) 0xF7, (byte) 0x15, (byte) 0x88, (byte) 0x09, (byte) 0xCF,
                    (byte) 0x4F, (byte) 0x3C };
                byte[] X = { };
                c.prf2(K, X);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // X is too long
            try
            {
                byte[] K = { (byte) 0x2B, (byte) 0x7E, (byte) 0x15, (byte) 0x16, (byte) 0x28, (byte) 0xAE, (byte) 0xD2,
                    (byte) 0xA6, (byte) 0xAB, (byte) 0xF7, (byte) 0x15, (byte) 0x88, (byte) 0x09, (byte) 0xCF,
                    (byte) 0x4F, (byte) 0x3C };
                byte[] X = new byte[Constants.MAXLEN + 1];
                c.prf2(K, X);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // validation against sample data
            try
            {
                byte[] K = { (byte) 0x2B, (byte) 0x7E, (byte) 0x15, (byte) 0x16, (byte) 0x28, (byte) 0xAE, (byte) 0xD2,
                    (byte) 0xA6, (byte) 0xAB, (byte) 0xF7, (byte) 0x15, (byte) 0x88, (byte) 0x09, (byte) 0xCF,
                    (byte) 0x4F, (byte) 0x3C };
                byte[] P = { 1, 2, 1, 0, 0, 10, 10, 5, 0, 0, 0, 10, 0, 0, 0, 0 };
                byte[] Q = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (byte)0xDD, (byte)0xD5 };
                byte[] R = { (byte) 0xC3, (byte) 0xB8, 41, (byte) 0xA1, (byte) 0xE8, 100, 43, 120, (byte) 0xCC, 41,
                    (byte) 0x94, 123, 59, (byte) 0x93, (byte) 0xDB, 99 };
                byte[] result = c.prf2(K, Common.concatenate(P, Q));
                CollectionAssert.AreEqual(R, c.prf2(K, Common.concatenate(P, Q)));
            }
            catch (ArgumentException e)
            {
                Assert.Fail(e.ToString());
            }
        }

        /**
         * Test method for
         * {@link org.fpe4j.Ciphers#ciph(javax.crypto.SecretKey, byte[])}.
         */
        [TestMethod]
        public void testCiph()
        {
            Ciphers c = new Ciphers();
            Assert.IsNotNull(c);

            // null inputs
            try
            {
                byte[] X = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
                c.ciph(null, X);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }
            try
            {
                byte[] K = { (byte) 0x2B, (byte) 0x7E, (byte) 0x15, (byte) 0x16, (byte) 0x28, (byte) 0xAE, (byte) 0xD2,
                    (byte) 0xA6, (byte) 0xAB, (byte) 0xF7, (byte) 0x15, (byte) 0x88, (byte) 0x09, (byte) 0xCF,
                    (byte) 0x4F, (byte) 0x3C };
                //KeyGenerator keygen = KeyGenerator.getInstance("AES");
                //SecretKey K = keygen.generateKey();
                c.ciph(K, null);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is NullReferenceException);
            }

            // wrong key type
            try
            {
                byte[] K = { (byte)0x2B };
                //KeyGenerator keygen = KeyGenerator.getInstance("DES");
                //SecretKey K = keygen.generateKey();
                byte[] X = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
                c.ciph(K, X);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // X is too short
            try
            {
                byte[] K = { (byte) 0x2B, (byte) 0x7E, (byte) 0x15, (byte) 0x16, (byte) 0x28, (byte) 0xAE, (byte) 0xD2,
                    (byte) 0xA6, (byte) 0xAB, (byte) 0xF7, (byte) 0x15, (byte) 0x88, (byte) 0x09, (byte) 0xCF,
                    (byte) 0x4F, (byte) 0x3C };
                byte[] X = { };
                c.ciph(K, X);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // X is too long
            try
            {
                byte[] K = { (byte) 0x2B, (byte) 0x7E, (byte) 0x15, (byte) 0x16, (byte) 0x28, (byte) 0xAE, (byte) 0xD2,
                    (byte) 0xA6, (byte) 0xAB, (byte) 0xF7, (byte) 0x15, (byte) 0x88, (byte) 0x09, (byte) 0xCF,
                    (byte) 0x4F, (byte) 0x3C };
                byte[] X = new byte[Constants.MAXLEN + 1];
                c.ciph(K, X);
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }

            // NIST AES Core 128 sample 1
            try
            {
                byte[] K = { (byte) 0x2B, (byte) 0x7E, (byte) 0x15, (byte) 0x16, (byte) 0x28, (byte) 0xAE, (byte) 0xD2,
                    (byte) 0xA6, (byte) 0xAB, (byte) 0xF7, (byte) 0x15, (byte) 0x88, (byte) 0x09, (byte) 0xCF,
                    (byte) 0x4F, (byte) 0x3C };
                byte[] X = { (byte) 0x6B, (byte) 0xC1, (byte) 0xBE, (byte) 0xE2, (byte) 0x2E, (byte) 0x40, (byte) 0x9F,
                    (byte) 0x96, (byte) 0xE9, (byte) 0x3D, (byte) 0x7E, (byte) 0x11, (byte) 0x73, (byte) 0x93,
                    (byte) 0x17, (byte) 0x2A, (byte) 0xAE, (byte) 0x2D, (byte) 0x8A, (byte) 0x57, (byte) 0x1E,
                    (byte) 0x03, (byte) 0xAC, (byte) 0x9C, (byte) 0x9E, (byte) 0xB7, (byte) 0x6F, (byte) 0xAC,
                    (byte) 0x45, (byte) 0xAF, (byte) 0x8E, (byte) 0x51, (byte) 0x30, (byte) 0xC8, (byte) 0x1C,
                    (byte) 0x46, (byte) 0xA3, (byte) 0x5C, (byte) 0xE4, (byte) 0x11, (byte) 0xE5, (byte) 0xFB,
                    (byte) 0xC1, (byte) 0x19, (byte) 0x1A, (byte) 0x0A, (byte) 0x52, (byte) 0xEF, (byte) 0xF6,
                    (byte) 0x9F, (byte) 0x24, (byte) 0x45, (byte) 0xDF, (byte) 0x4F, (byte) 0x9B, (byte) 0x17,
                    (byte) 0xAD, (byte) 0x2B, (byte) 0x41, (byte) 0x7B, (byte) 0xE6, (byte) 0x6C, (byte) 0x37,
                    (byte) 0x10 };
                byte[] Y = { (byte) 0x3A, (byte) 0xD7, (byte) 0x7B, (byte) 0xB4, (byte) 0x0D, (byte) 0x7A, (byte) 0x36,
                    (byte) 0x60, (byte) 0xA8, (byte) 0x9E, (byte) 0xCA, (byte) 0xF3, (byte) 0x24, (byte) 0x66,
                    (byte) 0xEF, (byte) 0x97, (byte) 0xF5, (byte) 0xD3, (byte) 0xD5, (byte) 0x85, (byte) 0x03,
                    (byte) 0xB9, (byte) 0x69, (byte) 0x9D, (byte) 0xE7, (byte) 0x85, (byte) 0x89, (byte) 0x5A,
                    (byte) 0x96, (byte) 0xFD, (byte) 0xBA, (byte) 0xAF, (byte) 0x43, (byte) 0xB1, (byte) 0xCD,
                    (byte) 0x7F, (byte) 0x59, (byte) 0x8E, (byte) 0xCE, (byte) 0x23, (byte) 0x88, (byte) 0x1B,
                    (byte) 0x00, (byte) 0xE3, (byte) 0xED, (byte) 0x03, (byte) 0x06, (byte) 0x88, (byte) 0x7B,
                    (byte) 0x0C, (byte) 0x78, (byte) 0x5E, (byte) 0x27, (byte) 0xE8, (byte) 0xAD, (byte) 0x3F,
                    (byte) 0x82, (byte) 0x23, (byte) 0x20, (byte) 0x71, (byte) 0x04, (byte) 0x72, (byte) 0x5D,
                    (byte) 0xD4 };
                CollectionAssert.AreEqual(Y, c.ciph(K, X));
            }
            catch (ArgumentException e)
            {
                Assert.Fail(e.ToString());
            }
        }


    }
}
