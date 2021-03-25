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
    /**
     * Unit test cases for conformance with the NIST sample data provided at
     * <a href=
     * "http://csrc.nist.gov/groups/ST/toolkit/examples.html">http://csrc.nist.gov/groups/ST/toolkit/examples.html</a>.
     * <p>
     * 
     * https://csrc.nist.gov/CSRC/media/Projects/Cryptographic-Standards-and-Guidelines/documents/examples/FF1samples.pdf
     * To enable FF1 and FF3 to output the intermediate results shown in the sample
     * data, change {@link org.fpe4j.Constants#CONFORMANCE_OUTPUT} to true.
     * 
     * @author Kai Johnson
     *
     */

    /**
     * https://csrc.nist.gov/CSRC/media/Projects/Cryptographic-Standards-and-Guidelines/documents/examples/FF1samples.pdf
     */
    [TestClass]
    public class NISTConformanceTest
    {

        /**
        * Input parameters for a single test case.
        * 
        * @author Kai Johnson
        *
        */
        private class TestInput
        {

            public readonly String name;
            public readonly int radix;
            public readonly byte[] key;
            public readonly byte[] tweak;
            public readonly int[] plaintext;
            public readonly int[] ciphertext;

            /**
		     * Constructs a TestInput instance with the specified test parameters.
		     * 
		     * @param name
		     *            the name of the test case
		     * @param radix
		     *            the radix for FPE operations
		     * @param key
		     *            the raw key data for FPE operations
		     * @param tweak
		     *            the tweak for FPE operations
		     * @param plaintext
		     *            the original plaintext and expected result of decrypting
		     *            the ciphertext
		     * @param ciphertext
		     *            the original ciphertext and expected result of encrypting
		     *            the plaintext
		     */
            public TestInput(String name, int radix, byte[] key, byte[] tweak, int[] plaintext, int[] ciphertext)
            {
                this.name = name;
                this.radix = radix;
                this.key = key;
                this.tweak = tweak;
                this.plaintext = plaintext;
                this.ciphertext = ciphertext;
            }
        }

        /**
	     * Test {@link org.fpe4j.FF1} for conformance with the NIST sample data.
	     */
        [TestMethod]
        public void testFF1Conformance()
        {

            // set up the test inputs
            TestInput[] ff1Tests = {
				    // Sample #1
				    new TestInput("Sample #1", 10, HexStringToByteArray("2B7E151628AED2A6ABF7158809CF4F3C"),
                            HexStringToByteArray(""), IntStringToIntArray("0 1 2 3 4 5 6 7 8 9"),
                            IntStringToIntArray("2 4 3 3 4 7 7 4 8 4")),
				    // Sample #2
				    new TestInput("Sample #2", 10, HexStringToByteArray("2B7E151628AED2A6ABF7158809CF4F3C"),
                            HexStringToByteArray("39383736353433323130"),
                            IntStringToIntArray("0 1 2 3 4 5 6 7 8 9"),
                            IntStringToIntArray("6 1 2 4 2 0 0 7 7 3")),
				    // Sample #3
				    new TestInput("Sample #3", 36, HexStringToByteArray("2B7E151628AED2A6ABF7158809CF4F3C"),
                            HexStringToByteArray("3737373770717273373737"),
                            IntStringToIntArray("0 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18"),
                            IntStringToIntArray("10 9 29 31 4 0 22 21 21 9 20 13 30 5 0 9 14 30 22")),
				    // Sample #4
				    new TestInput("Sample #4", 10,
                            HexStringToByteArray("2B7E151628AED2A6ABF7158809CF4F3CEF4359D8D580AA4F"),
                            HexStringToByteArray(""), IntStringToIntArray("0 1 2 3 4 5 6 7 8 9"),
                            IntStringToIntArray("2 8 3 0 6 6 8 1 3 2")),
				    // Sample #5
				    new TestInput("Sample #5", 10,
                            HexStringToByteArray("2B7E151628AED2A6ABF7158809CF4F3CEF4359D8D580AA4F"),
                            HexStringToByteArray("39383736353433323130"),
                            IntStringToIntArray("0 1 2 3 4 5 6 7 8 9"),
                            IntStringToIntArray("2 4 9 6 6 5 5 5 4 9")),
				    // Sample #6
				    new TestInput("Sample #6", 36,
                            HexStringToByteArray("2B7E151628AED2A6ABF7158809CF4F3CEF4359D8D580AA4F"),
                            HexStringToByteArray("3737373770717273373737"),
                            IntStringToIntArray("0 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18"),
                            IntStringToIntArray("33 11 19 3 20 31 3 5 19 27 10 32 33 31 3 2 34 28 27")),
				    // Sample #7
				    new TestInput("Sample #7", 10,
                            HexStringToByteArray(
                                    "2B7E151628AED2A6ABF7158809CF4F3CEF4359D8D580AA4F7F036D6F04FC6A94"),
                            HexStringToByteArray(""), IntStringToIntArray("0 1 2 3 4 5 6 7 8 9"),
                            IntStringToIntArray("6 6 5 7 6 6 7 0 0 9")),
				    // Sample #8
				    new TestInput("Sample #8", 10,
                            HexStringToByteArray(
                                    "2B7E151628AED2A6ABF7158809CF4F3CEF4359D8D580AA4F7F036D6F04FC6A94"),
                            HexStringToByteArray("39383736353433323130"),
                            IntStringToIntArray("0 1 2 3 4 5 6 7 8 9"),
                            IntStringToIntArray("1 0 0 1 6 2 3 4 6 3")),
				    // Sample #9
				    new TestInput("Sample #9", 36,
                            HexStringToByteArray(
                                    "2B7E151628AED2A6ABF7158809CF4F3CEF4359D8D580AA4F7F036D6F04FC6A94"),
                            HexStringToByteArray("3737373770717273373737"),
                            IntStringToIntArray("0 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18"),
                            IntStringToIntArray("33 28 8 10 0 10 35 17 2 10 31 34 10 21 34 35 30 32 13")),
				    // Test Concatenation in Step 6. iii.
				    new TestInput("Test Concatenation in Step 6. iii.", 256,
                            HexStringToByteArray(
                                    "2B7E151628AED2A6ABF7158809CF4F3CEF4359D8D580AA4F7F036D6F04FC6A94"),
                            HexStringToByteArray("3737373770717273373737"),
                            IntStringToIntArray(
                                    "77 104 140 63 156 241 168 217 77 120 141 248 199 103 250 164 56 175 134 207 120 221 126 109 156 169 100 89 115 18 217 150 78 71 81 206 168 98 98 156 95 122 38 63 68 30 212 125 250 155 29 218 189 20 234 97 130 113 229 168 221 55 161 90 45 240 130 241 58 61 170 204 41 160 144 147 174 65 87 23"),
                            IntStringToIntArray(
                                    "68 111 39 159 6 189 255 68 203 183 154 249 35 48 199 152 118 215 63 117 164 44 164 195 236 192 41 33 25 92 8 156 151 239 253 22 223 23 228 167 170 8 34 25 11 181 38 5 111 145 154 135 59 238 62 185 132 63 216 218 107 179 121 95 87 20 239 2 80 133 216 171 142 192 139 64 105 203 160 125")),

            };

            // for each test input
            foreach (TestInput test in ff1Tests)
            {

                // create an FF1 instance
                FF1 ff1 = new FF1(test.radix, Constants.MAXLEN);
                Assert.IsNotNull(ff1);


                Console.WriteLine("\n==============================================================\n");
                Console.WriteLine(test.name + "\n");
                Console.WriteLine("FF1-AES" + test.key.Length * 8 + "\n");
                Console.WriteLine("Key is " + Common.byteArrayToHexString(test.key));
                Console.WriteLine("Radix = " + test.radix);
                Console.WriteLine("--------------------------------------------------------------\n");
                Console.WriteLine("PT is <" + Common.intArrayToString(test.plaintext) + ">\n");

                // perform the encryption
                int[] CT = ff1.encrypt(test.key, test.tweak, test.plaintext);

                Console.WriteLine("CT is <" + Common.intArrayToString(CT) + ">");

                // validate the ciphertext
                CollectionAssert.AreEqual(test.ciphertext, CT);

                Console.WriteLine("\n--------------------------------------------------------------\n");

                // perform the decryption
                int[] PT = ff1.decrypt(test.key, test.tweak, CT);

                Console.WriteLine("PT is <" + Common.intArrayToString(PT) + ">");

                // validate the recovered plaintext
                CollectionAssert.AreEqual(test.plaintext, PT);

            }
        }

        /**
	     * Test {@link org.fpe4j.FF3} for conformance with the NIST sample data.
	     */
        [TestMethod]
        public void testFF3Conformance()
        {

            // set up the test inputs
            TestInput[] ff3Tests = {
				// Sample #1
				new TestInput("Sample #1", 10, HexStringToByteArray("EF4359D8D580AA4F7F036D6F04FC6A94"),
                        HexStringToByteArray("D8E7920AFA330A73"),
                        IntStringToIntArray("8 9 0 1 2 1 2 3 4 5 6 7 8 9 0 0 0 0"),
                        IntStringToIntArray("7 5 0 9 1 8 8 1 4 0 5 8 6 5 4 6 0 7")),
				// Sample #2
				new TestInput("Sample #2", 10, HexStringToByteArray("EF4359D8D580AA4F7F036D6F04FC6A94"),
                        HexStringToByteArray("9A768A92F60E12D8"),
                        IntStringToIntArray("8 9 0 1 2 1 2 3 4 5 6 7 8 9 0 0 0 0"),
                        IntStringToIntArray("0 1 8 9 8 9 8 3 9 1 8 9 3 9 5 3 8 4")),
				// Sample #3
				new TestInput("Sample #3", 10, HexStringToByteArray("EF4359D8D580AA4F7F036D6F04FC6A94"),
                        HexStringToByteArray("D8E7920AFA330A73"),
                        IntStringToIntArray("8 9 0 1 2 1 2 3 4 5 6 7 8 9 0 0 0 0 0 0 7 8 9 0 0 0 0 0 0"),
                        IntStringToIntArray("4 8 5 9 8 3 6 7 1 6 2 2 5 2 5 6 9 6 2 9 3 9 7 4 1 6 2 2 6")),
				// Sample #4
				new TestInput("Sample #4", 10, HexStringToByteArray("EF4359D8D580AA4F7F036D6F04FC6A94"),
                        HexStringToByteArray("0000000000000000"),
                        IntStringToIntArray("8 9 0 1 2 1 2 3 4 5 6 7 8 9 0 0 0 0 0 0 7 8 9 0 0 0 0 0 0"),
                        IntStringToIntArray("3 4 6 9 5 2 2 4 8 2 1 7 3 4 5 3 5 1 2 2 6 1 3 7 0 1 4 3 4")),
				// Sample #5
				new TestInput("Sample #5", 26, HexStringToByteArray("EF4359D8D580AA4F7F036D6F04FC6A94"),
                        HexStringToByteArray("9A768A92F60E12D8"),
                        IntStringToIntArray("0 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18"),
                        IntStringToIntArray("16 2 25 20 4 0 18 9 9 2 15 23 2 0 12 19 10 20 11")),
				// Sample #6
				new TestInput("Sample #6", 10,
                        HexStringToByteArray("EF4359D8D580AA4F7F036D6F04FC6A942B7E151628AED2A6"),
                        HexStringToByteArray("D8E7920AFA330A73"),
                        IntStringToIntArray("8 9 0 1 2 1 2 3 4 5 6 7 8 9 0 0 0 0"),
                        IntStringToIntArray("6 4 6 9 6 5 3 9 3 8 7 5 0 2 8 7 5 5")),
				// Sample #7
				new TestInput("Sample #7", 10,
                        HexStringToByteArray("EF4359D8D580AA4F7F036D6F04FC6A942B7E151628AED2A6"),
                        HexStringToByteArray("9A768A92F60E12D8"),
                        IntStringToIntArray("8 9 0 1 2 1 2 3 4 5 6 7 8 9 0 0 0 0"),
                        IntStringToIntArray("9 6 1 6 1 0 5 1 4 4 9 1 4 2 4 4 4 6")),
				// Sample #8
				new TestInput("Sample #8", 10,
                        HexStringToByteArray("EF4359D8D580AA4F7F036D6F04FC6A942B7E151628AED2A6"),
                        HexStringToByteArray("D8E7920AFA330A73"),
                        IntStringToIntArray("8 9 0 1 2 1 2 3 4 5 6 7 8 9 0 0 0 0 0 0 7 8 9 0 0 0 0 0 0"),
                        IntStringToIntArray("5 3 0 4 8 8 8 4 0 6 5 3 5 0 2 0 4 5 4 1 7 8 6 3 8 0 8 0 7")),
				// Sample #9
				new TestInput("Sample #9", 10,
                        HexStringToByteArray("EF4359D8D580AA4F7F036D6F04FC6A942B7E151628AED2A6"),
                        HexStringToByteArray("0000000000000000"),
                        IntStringToIntArray("8 9 0 1 2 1 2 3 4 5 6 7 8 9 0 0 0 0 0 0 7 8 9 0 0 0 0 0 0"),
                        IntStringToIntArray("9 8 0 8 3 8 0 2 6 7 8 8 2 0 3 8 9 2 9 5 0 4 1 4 8 3 5 1 2")),
				// Sample #10
				new TestInput("Sample #10", 26,
                        HexStringToByteArray("EF4359D8D580AA4F7F036D6F04FC6A942B7E151628AED2A6"),
                        HexStringToByteArray("9A768A92F60E12D8"),
                        IntStringToIntArray("0 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18"),
                        IntStringToIntArray("18 0 18 17 14 2 19 15 19 7 10 9 24 25 15 9 25 8 8")),
				// Sample #11
				new TestInput("Sample #11", 10,
                        HexStringToByteArray(
                                "EF4359D8D580AA4F7F036D6F04FC6A942B7E151628AED2A6ABF7158809CF4F3C"),
                        HexStringToByteArray("D8E7920AFA330A73"),
                        IntStringToIntArray("8 9 0 1 2 1 2 3 4 5 6 7 8 9 0 0 0 0"),
                        IntStringToIntArray("9 2 2 0 1 1 2 0 5 5 6 2 7 7 7 4 9 5")),
				// Sample #12
				new TestInput("Sample #12", 10,
                        HexStringToByteArray(
                                "EF4359D8D580AA4F7F036D6F04FC6A942B7E151628AED2A6ABF7158809CF4F3C"),
                        HexStringToByteArray("9A768A92F60E12D8"),
                        IntStringToIntArray("8 9 0 1 2 1 2 3 4 5 6 7 8 9 0 0 0 0"),
                        IntStringToIntArray("5 0 4 1 4 9 8 6 5 5 7 8 0 5 6 1 4 0")),
				// Sample #13
				new TestInput("Sample #13", 10,
                        HexStringToByteArray(
                                "EF4359D8D580AA4F7F036D6F04FC6A942B7E151628AED2A6ABF7158809CF4F3C"),
                        HexStringToByteArray("D8E7920AFA330A73"),
                        IntStringToIntArray("8 9 0 1 2 1 2 3 4 5 6 7 8 9 0 0 0 0 0 0 7 8 9 0 0 0 0 0 0"),
                        IntStringToIntArray("0 4 3 4 4 3 4 3 2 3 5 7 9 2 5 9 9 1 6 5 7 3 4 6 2 2 6 9 9")),
				// Sample #14
				new TestInput("Sample #14", 10,
                        HexStringToByteArray(
                                "EF4359D8D580AA4F7F036D6F04FC6A942B7E151628AED2A6ABF7158809CF4F3C"),
                        HexStringToByteArray("0000000000000000"),
                        IntStringToIntArray("8 9 0 1 2 1 2 3 4 5 6 7 8 9 0 0 0 0 0 0 7 8 9 0 0 0 0 0 0"),
                        IntStringToIntArray("3 0 8 5 9 2 3 9 9 9 9 3 7 4 0 5 3 8 7 2 3 6 5 5 5 5 8 2 2")),
				// Sample #15
				new TestInput("Sample #15", 26,
                        HexStringToByteArray(
                                "EF4359D8D580AA4F7F036D6F04FC6A942B7E151628AED2A6ABF7158809CF4F3C"),
                        HexStringToByteArray("9A768A92F60E12D8"),
                        IntStringToIntArray("0 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18"),
                        IntStringToIntArray("25 0 11 2 16 24 13 15 19 10 9 11 17 11 7 11 20 3 8")),
				//
		};

            // for each test input
            foreach (TestInput test in ff3Tests)
            {
                try
                {
                    // create an FF3 instance
                    FF3 ff3 = new FF3(test.radix);
                    Assert.IsNotNull(ff3);

                    Console.WriteLine("\n==============================================================\n");
                    Console.WriteLine(test.name + "\n");
                    Console.WriteLine("FF3-AES" + test.key.Length * 8 + "\n");
                    Console.WriteLine("Key is " + Common.byteArrayToHexString(test.key));
                    Console.WriteLine("Radix = " + test.radix);
                    Console.WriteLine("--------------------------------------------------------------\n");
                    Console.WriteLine("PT is <" + Common.intArrayToString(test.plaintext) + ">\n");

                    // perform the encryption
                    int[] CT = ff3.encrypt(test.key, test.tweak, test.plaintext);

                    Console.WriteLine("CT is <" + Common.intArrayToString(CT) + ">");

                    // validate the ciphertext
                    CollectionAssert.AreEqual(test.ciphertext, CT);

                    Console.WriteLine("\n--------------------------------------------------------------\n");

                    // perform the decryption
                    int[] PT = ff3.decrypt(test.key, test.tweak, test.ciphertext);

                    Console.WriteLine("PT is <" + Common.intArrayToString(PT) + ">");

                    // validate the recovered plaintext
                    CollectionAssert.AreEqual(test.plaintext, PT);
                }
                catch (ArgumentException e)
                {
                    Assert.Fail(e.ToString());
                }
            }
        }

        [TestMethod]
        public void testIntStringToIntArray()
        {

            string s = "0 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18";
            int[] expected = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 };
            int[] result = IntStringToIntArray(s);
            CollectionAssert.AreEqual(expected, result);

        }

        /**
         * Replaces Utilities.HexStringToByteArray 
         */
        private byte[] HexStringToByteArray(String hex)
        {
            if (hex.Length % 2 == 1) hex = "0" + hex;
            byte[] bytes = new byte[hex.Length / 2];

            for (int i = 0; i < hex.Length; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

            return bytes;
        }

        /**
         * Replaces Utilities.IntStringToIntArray 
         */
        private int[] IntStringToIntArray(string intString)
        {
            string[] ints = intString.Split(' ');
            int[] intArray = new int[ints.Length];
            for (int i = 0; i < ints.Length; i++)
            {
                intArray[i] = Int32.Parse(ints[i]);
            }

            return intArray;
        }
    }
}
