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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using FormatPreservingEncryptionWeydstone;

namespace FPETests
{
    [TestClass]
    public class FF1PaddingTest
    {

        public FF1PaddingTest()
        {

        }

        /**
	     * Tests the FF1 implementation to confirm a fix for an exception that was
	     * erroneously thrown when zero padding bytes were required.
	     * <p>
	     * java.lang.IllegalArgumentException: s is not within the permitted range
	     * of 1..4096: 0 at org.fpe4j.common.Common.bytestring(Common.java:528)
	     * <p>
	     * Submitted by WU Jianqiang (james7wu@users.sf.net)
	     * 
	     * @throws InvalidKeyException
	     *             if the FF1 implementation does not properly generate padding
	     *             when (T.length + B.length + 1) mod 16 == 0
	     */
        [TestMethod]
        public void testPaddingBug()
        {
            int radix = 21093;
            int maxTlen = 16;

            FF1 ff1 = new FF1(radix, maxTlen);
            Assert.IsNotNull(ff1);

            byte[] K = { (byte) 0x2B, (byte) 0x7E, (byte) 0x15, (byte) 0x16, (byte) 0x28, (byte) 0xAE, (byte) 0xD2,
                    (byte) 0xA6, (byte) 0xAB, (byte) 0xF7, (byte) 0x15, (byte) 0x88, (byte) 0x09, (byte) 0xCF, (byte) 0x4F,
                    (byte) 0x3C };

            byte[] T = { 0, 0, 0, 0, 0, 0, 0, 0 };
            int[] PT = { 1115, 217, 5439, 6638, 10563, 10963, 1680, 5439, 6638, 1895, 16033, 5439, 6638, 5735, 4414, 6604,
                    1376, 12734, 1493, 4260, 11466, 6268, 4260, 12734, 1493, 11872, 9925, 6604, 1376, 1115, 539, 9512,
                    13680, 1544, 6747, 6864, 10563, 11404, 7116, 205, 12686, 14052, 13718, 6631, 10563, 10206, 358, 12686,
                    14052, 7923, 358, 1082, 7765, 205, 18367, 2029, 19358, 10215, 1115, 358, 1920, 10563, 18367, 2029, 1115,
                    217, 15489, 1032, 6604, 1376, 10, 604, 8084, 19258, 12686, 5432, 1157, 10563, 19256, 10669, 12686,
                    10679, 1074, 17319, 18599, 5432, 1157, 1741, 6264, 1710, 4543, 3860, 12686, 14052, 8186, 1383 };
            int[] CT = ff1.encrypt(K, T, PT);
            CollectionAssert.AreEqual(PT, ff1.decrypt(K, T, CT));
        }
    }
}
