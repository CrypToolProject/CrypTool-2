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
* Converted and modified by Alexander Hirsch <hirsch@CrypTool.org>
*/
using System;
using System.Numerics;

namespace FormatPreservingEncryptionWeydstone
{
    public class FF3Parameters : FFXParameters
    {
        /**
         * Construct a new FF3Parameters instance with the specified radix.
         * 
         * @param radix
         *            the radix for FF3 operations
         */
        public FF3Parameters(int radix)
        {
            this.radix = radix;

            // 2 <= minlen <= maxlen <= 2 * Common.floor(log(2^96)/log(radix))
            minlen = Math.Max(2, Common.ceiling(Math.Log(100) / Math.Log(radix)));
            maxlen = Math.Max(minlen, 2 * Common.floor(Math.Log(Math.Pow(2, 96)) / Math.Log(radix)));

            ciphers = new Ciphers();
            ff3Round = new FF3RoundFunction(radix, ciphers);
            ff3ArithmeticFunction = new FF3ArithmeticFunction(radix);
            ff3Splitter = new FF3SplitFunction(minlen, maxlen);

        }

        /**
	     * The radix specified in this parameter set.
	     */
        private readonly int radix;

        /**
	     * The minimum input length allowed by this parameter set.
	     */
        private readonly int minlen;

        /**
	     * The maximum input length allowed by this parameter set.
	     */
        private readonly int maxlen;

        /**
	     * Instances of AES ciphers for PRF and CIPH algorithms.
	     */
        private readonly Ciphers ciphers;

        /**
	     * Arithmetic functions for the operations C = A [+] F K (n, T, i, B) in the
	     * encryption rounds and A = C [-] F K (n, T, i, B) in the decryption
	     * rounds.
	     * <p>
	     * The FF3 algorithm requires a little more generality than the FFX
	     * algorithm allows, because it uses a modified form of the arithmetic
	     * operations in the Feistel rounds.
	     * <p>
	     * FFX defines two types of arithmetic operators: blockwise arithmetic,
	     * which treats each input array as an integer [0..radix<sup>m</sup>], where
	     * m is the length of the array, and which returns a result within the same
	     * range; and characterwise arithmetic, which adds each array element
	     * individually and produces a resulting element within the range
	     * [0..radix].
	     * <p>
	     * FF3 uses a modified form of blockwise arithmetic, where the sequence of
	     * elements in the first array is reversed before the arithmetic operation,
	     * then the sequence of elements in the resulting array is reversed before
	     * it is returned.
	     */
        private readonly ArithmeticFunction ff3ArithmeticFunction;
        private class FF3ArithmeticFunction : ArithmeticFunction
        {

            public FF3ArithmeticFunction(int radix)
            {
                this.radix = radix;
            }

            private readonly int radix;

            public int[] Subtract(int[] X, int[] Y)
            {
                /*
			     * This corresponds to the following two steps from the FF3
			     * algorithm where X is B and Z is C:
			     * 
			     * v. Let c = (NUMradix (REV(B))â€“y) mod radix m .
			     * 
			     * vi. Let C = REV(STR m radix (c)).
			     */
                BigInteger x = Common.num(Common.rev(X), radix);
                BigInteger y = Common.num(Y, radix);
                BigInteger z = Common.mod(x - y, BigInteger.Pow(radix, X.Length));
                int[] Z = Common.str(z, radix, X.Length);
                return Common.rev(Z);
            }

            public int[] Add(int[] X, int[] Y)
            {
                /*
			     * This corresponds to the following two steps from the FF3
			     * algorithm where X is A and Z is C:
			     * 
			     * Step 6.v. Let c = (NUMradix (REV(A)) + y) mod radix.
			     * 
			     * Step 6.vi. Let C = REV(STR m radix (c)).
			     */
                BigInteger x = Common.num(Common.rev(X), radix);
                BigInteger y = Common.num(Y, radix);
                BigInteger z = Common.mod(x + y, BigInteger.Pow(radix, X.Length));
                int[] Z = Common.str(z, radix, X.Length);
                return Common.rev(Z);
            }
        };

        /**
	     * Split function for FF3.
	     */
        private readonly SplitFunction ff3Splitter;
        private class FF3SplitFunction : SplitFunction
        {

            public FF3SplitFunction(int minlen, int maxlen)
            {
                this.minlen = minlen;
                this.maxlen = maxlen;
            }

            private readonly int minlen;
            private readonly int maxlen;

            public int split(int n)
            {
                // validate n
                if (n < minlen || n > maxlen)
                {
                    throw new ArgumentException("n must be in the range [" + minlen + ".." + maxlen + "].");
                }

                return Common.ceiling(n / 2.0);
            }
        };

        /**
	     * Function to determine the number of Feistel rounds for FF3.
	     */
        private readonly RoundCounter ff3RoundCounter = new FF3RoundCounter();
        private class FF3RoundCounter : RoundCounter
        {

            public int rnds(int n)
            {
                return 8;
            }
        };

        /**
	     * Round function F for FF1, derived from NIST SP 800-38G.
	     */
        private readonly RoundFunction ff3Round;
        private class FF3RoundFunction : RoundFunction
        {

            public FF3RoundFunction(int radix, Ciphers ciphers)
            {
                this.ciphers = ciphers;
                this.radix = radix;
            }

            private readonly Ciphers ciphers;
            private readonly int radix;

            public bool validKey(byte[] K)
            {
                // validate K
                if (K == null)
                {
                    return false;
                }

                return true;
            }

            public int[] F(byte[] K, int n, byte[] T, int i, int[] B)
            {

                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("Round #" + i + "\n");
                }

                // value of REVB(K) for readability
                byte[] revK = Common.revb(K);


                // 1. Let u = Common.ceiling(n/2); v = n â€“ u.
                int u = Common.ceiling(n / 2.0);
                int v = n - u;
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("Step 1\n\tu is <" + u + ">, and v is <" + v + ">");
                }

                // 2. Let A = X[1..u]; B = X[u + 1..n].
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("Step 2\n\tB is " + Common.intArrayToString(B));
                }

                // 3. Let T_L = T[0..31] and T_R = T[32..63]
                //TODO
                byte[] T_L = new byte[4];// Arrays.copyOfRange(T, 0, 4);
                byte[] T_R = new byte[4];// Arrays.copyOfRange(T, 4, 8);
                Array.Copy(T, 0, T_L, 0, 4);
                Array.Copy(T, 4, T_R, 0, 4);

                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine(
                            "Step 3\n\tT_L is " + Common.byteArrayToHexString(T_L) + "\n\tT_R is " + Common.byteArrayToHexString(T_R));
                }

                // i. If i is even, let m = u and W = T_R ,
                // else let m = v and W = T_L .
                int m = i % 2 == 0 ? u : v;
                byte[] W = i % 2 == 0 ? T_R : T_L;
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("Step 4.i\n\tm is <" + m + ">\n\tW is " + Common.byteArrayToHexString(W));
                }

                // ii. Let P = W xor [i] 4 || [NUMradix (REV(B))] 12 .
                byte[] P = Common.concatenate(Common.xor(W, Common.bytestring(i, 4)), Common.bytestring(Common.num(Common.rev(B), radix), 12));
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("Step 4.ii\n\tP is " + Common.unsignedByteArrayToString(P));
                }

                // iii Let S = REVB(CIPH REVB(K) REVB(P)).
                byte[] S = Common.revb(ciphers.ciph(revK, Common.revb(P)));
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("Step 4.iii\n\tS is " + Common.byteArrayToHexString(S));
                }

                // iv. Let y = NUM(S).
                BigInteger y = Common.num(S);
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("Step 4.iv\n\ty is " + y);
                }

                // constrain y to the range [0..radix^m]
                y = Common.mod(y, BigInteger.Pow(radix, m));

                // 5. Let Y = STR m radix (y).
                int[] Y = Common.str(y, radix, m);
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    Console.WriteLine("Step 5.\n\tY is " + Common.intArrayToString(Y) + "\n");
                }

                return Y;
            }
        };

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
            return 8;
        }

        public int getMaxTLen()
        {
            return 8;
        }

        public ArithmeticFunction getArithmeticFunction()
        {
            return ff3ArithmeticFunction;
        }

        public FeistelMethod getFeistelMethod()
        {
            return FeistelMethod.TWO;
        }

        public SplitFunction getSplitter()
        {
            return ff3Splitter;
        }

        public RoundCounter getRoundCounter()
        {
            return ff3RoundCounter;
        }

        public RoundFunction getRoundFunction()
        {
            return ff3Round;
        }
    }
}
