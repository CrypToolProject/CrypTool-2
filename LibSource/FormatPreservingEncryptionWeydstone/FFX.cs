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
    public class FFX
    {
        /**
	     * The range of values for symbols in plaintexts and ciphertexts, 0..radix
	     * for each symbol.
	     */
        private readonly int radix;

        /**
	     * The minimum Length of plaintext and ciphertext inputs.
	     */
        private readonly int minlen;

        /**
	     * The maximum Length of plaintext and ciphertext inputs.
	     */
        private readonly int maxlen;

        /**
	     * The minimum Length of tweak inputs.
	     */
        private readonly int minTlen;

        /**
	     * The maximum Length of tweak inputs.
	     */
        private readonly int maxTlen;

        /**
	     * The arithmetic functions for the [+] and [-] operations in the Feistel
	     * rounds.
	     */
        private readonly ArithmeticFunction arithmeticFunction;

        /**
	     * The Feistel method, either ONE where the array is re-partitioned on each
	     * round, or TWO where the array partitions are swapped on each round.
	     */
        private readonly FeistelMethod feistelMethod;

        /**
	     * Function to determine where to split input arrays.
	     */
        private readonly SplitFunction splitter;

        /**
	     * Function to determine the number of Feistel rounds.
	     */
        private readonly RoundCounter roundCounter;

        /**
	     * Pseudorandom function for Feistel rounds.
	     */
        private readonly RoundFunction roundFunction;

        private class BlockwiseArithmeticFunction : ArithmeticFunction
        {
            public BlockwiseArithmeticFunction(int radix)
            {
                this.radix = radix;
            }

            private readonly int radix;

            public int[] Add(int[] X, int[] Y)
            {
                // validate X
                if (X == null)
                {
                    throw new NullReferenceException("X must not be null.");
                }

                if (X.Length == 0)
                {
                    throw new ArgumentException("X must not be empty");
                }

                // validate Y
                if (Y == null)
                {
                    throw new NullReferenceException("Y must not be null.");
                }

                if (Y.Length == 0)
                {
                    throw new ArgumentException("Y must not be empty");
                }

                if (X.Length != Y.Length)
                {
                    throw new ArgumentException("X and Y must be the same Length.");
                }

                // numeric value of X
                BigInteger x = Common.num(X, radix);

                // numeric value of Y
                BigInteger y = Common.num(Y, radix);


                // numeric value of (x + y) mod radix^m
                BigInteger z = Common.mod(x + y, BigInteger.Pow(radix, X.Length));

                // convert result to an array
                int[] Z = Common.str(z, radix, X.Length);

                return Z;
            }

            public int[] Subtract(int[] X, int[] Y)
            {
                // validate X
                if (X == null)
                {
                    throw new NullReferenceException("X must not be null.");
                }

                if (X.Length == 0)
                {
                    throw new ArgumentException("X must not be empty");
                }

                // validate Y
                if (Y == null)
                {
                    throw new NullReferenceException("Y must not be null.");
                }

                if (Y.Length == 0)
                {
                    throw new ArgumentException("Y must not be empty");
                }

                if (X.Length != Y.Length)
                {
                    throw new ArgumentException("X and Y must be the same Length.");
                }

                // numeric value of X
                BigInteger x = Common.num(X, radix);

                // numeric value of Y
                BigInteger y = Common.num(Y, radix);

                // numeric value of (x - y) mod radix^m
                BigInteger z = Common.mod(x - y, BigInteger.Pow(radix, X.Length));

                // convert result to an array
                int[] Z = Common.str(z, radix, X.Length);
                return Z;
            }
        };



        public class CharwiseArithmeticFunction : ArithmeticFunction
        {
            public CharwiseArithmeticFunction(int radix)
            {
                this.radix = radix;
            }

            private readonly int radix;

            public int[] Add(int[] X, int[] Y)
            {
                // validate X
                if (X == null)
                {
                    throw new NullReferenceException("X must not be null.");
                }

                if (X.Length == 0)
                {
                    throw new ArgumentException("X must not be empty");
                }

                // validate Y
                if (Y == null)
                {
                    throw new NullReferenceException("Y must not be null.");
                }

                if (Y.Length == 0)
                {
                    throw new ArgumentException("Y must not be empty");
                }

                if (X.Length != Y.Length)
                {
                    throw new ArgumentException("X and Y must be the same Length.");
                }

                // create the result array
                int[] Z = new int[X.Length];

                // for each element in the input arrays
                for (int i = 0; i < X.Length; i++)
                {
                    // z = (x + y) mod radix
                    Z[i] = Common.mod(X[i] + Y[i], radix);
                }
                return Z;
            }



            public int[] Subtract(int[] X, int[] Y)
            {
                // validate X
                if (X == null)
                {
                    throw new NullReferenceException("X must not be null.");
                }

                if (X.Length == 0)
                {
                    throw new ArgumentException("X must not be empty");
                }

                // validate Y
                if (Y == null)
                {
                    throw new NullReferenceException("Y must not be null.");
                }

                if (Y.Length == 0)
                {
                    throw new ArgumentException("Y must not be empty");
                }

                if (X.Length != Y.Length)
                {
                    throw new ArgumentException("X and Y must be the same Length.");
                }

                // create the result array
                int[] Z = new int[X.Length];

                // for each element in the input arrays
                for (int i = 0; i < X.Length; i++)
                {
                    // z = (x - y) mod radix
                    Z[i] = Common.mod(X[i] - Y[i], radix);
                }
                return Z;
            }
        }




        /**
	     * Returns an ArithmeticFunction instance which implements blockwise
	     * arithmetic, treating each input array as an integer
	     * [0..radix<sup>m</sup>], where m is the Length of the array, and returning
	     * an array representing a result within the same range.
	     * 
	     * @param radix
	     *            the radix
	     * @return an ArithmeticFunction which implements blockwise addition for the
	     *         specified radix
	     * 
	     */
        public static ArithmeticFunction getBlockwiseArithmeticFunction(int radix)
        {
            // validate radix
            if (radix < Constants.MINRADIX)
            {
                throw new ArgumentException("Radix must be greater than " + Constants.MINRADIX);
            }
            //TODO
            return new BlockwiseArithmeticFunction(radix);
        }

        /**
         * Returns an ArithmeticFunction instance which implements characterwise
         * arithmetic, performing modulo arithmetic on the corresponding elements of
         * each input array, and returning an array of the same Length as the input
         * arrays with elements in the range [0..radix].
         * 
         * @param radix
         *            the radix
         * @return an ArithmeticFunction which implements blockwise addition for the
         *         specified radix
         * 
         */
        public static ArithmeticFunction getCharwiseArithmeticFunction(int radix)
        {
            // validate radix
            if (radix < Constants.MINRADIX)
            {
                throw new ArgumentException("Radix must be greater than " + Constants.MINRADIX);
            }
            //TODO 
            return new CharwiseArithmeticFunction(radix);
        }

        /**
         * Construct a new FFX instance from explicit parameters.
         * 
         * @param radix
         *            The range of values for symbols in plaintexts and ciphertexts,
         *            0..radix for each symbol.
         * @param minlen
         *            The minimum Length of plaintext and ciphertext inputs.
         * @param maxlen
         *            The maximum Length of plaintext and ciphertext inputs.
         * @param maxTlen
         *            The maximum Length of tweak inputs.
         * @param blockwise
         *            True if the functions for the [+] and [-] operations in the
         *            Feistel rounds are blockwise arithmetic, false if the
         *            functions use charwise arithmetic
         * @param method
         *            The Feistel method, either ONE where the array is
         *            re-partitioned on each round, or TWO where the array
         *            partitions are swapped on each round.
         * @param split
         *            Function to determine where to split input arrays.
         * @param rnds
         *            Function to determine the number of Feistel rounds.
         * @param F
         *            Pseudorandom function for Feistel rounds.
         */
        public FFX(int radix, int minlen, int maxlen, int maxTlen, bool blockwise, FeistelMethod method,
                    SplitFunction split, RoundCounter rnds, RoundFunction F)
        {
            // validate radix
            if (radix < Constants.MINRADIX)
            {
                throw new ArgumentException(
                        "radix must be greater than or equal to " + Constants.MINRADIX + ": " + radix);
            }

            // validate minlen
            if (minlen < Constants.MINLEN)
            {
                throw new ArgumentException(
                        "minlen must be greater than or equal to " + Constants.MINLEN + ": " + minlen);
            }

            if (Math.Pow(radix, minlen) < 100)
            {
                throw new ArgumentException(
                        "radix^minlen must be greater than or equal to 100: " + Math.Pow(radix, minlen));
            }

            // validate maxlen
            if (maxlen < minlen)
            {
                throw new ArgumentException("maxlen must be greater than or equal to minlen: " + maxlen);
            }

            // validate maxTlen;
            if (maxTlen < 0)
            {
                throw new ArgumentException("maxTlen must be greater than or equal to zero: " + maxTlen);
            }

            // validate method
            if (method == null)
            {
                throw new NullReferenceException("method must not be null.");
            }

            // validate split function
            if (split == null)
            {
                throw new NullReferenceException("Split function must not be null.");
            }

            // validate round count function
            if (rnds == null)
            {
                throw new NullReferenceException("Round count function must not be null.");
            }

            // validate round function
            if (F == null)
            {
                throw new NullReferenceException("F must not be null.");
            }

            // initialize instance variables
            this.radix = radix;
            this.minlen = minlen;
            this.maxlen = maxlen;
            minTlen = 0;
            this.maxTlen = maxTlen;
            if (blockwise)
            {
                arithmeticFunction = getBlockwiseArithmeticFunction(radix);
            }
            else
            {
                arithmeticFunction = getCharwiseArithmeticFunction(radix);
            }
            feistelMethod = method;
            splitter = split;
            roundCounter = rnds;
            roundFunction = F;
        }

        /**
	     * Construct a new FFX instance from an FFXParameter object.
	     * 
	     * @param params
	     *            the parameters for the FFX instance
	     */
        public FFX(FFXParameters ffxParams)
        {
            if (ffxParams == null)
            {
                throw new NullReferenceException("Params must not be null.");
            }

            // initialize instance variables
            radix = ffxParams.getRadix();
            minlen = ffxParams.getMinLen();
            maxlen = ffxParams.getMaxLen();
            minTlen = ffxParams.getMinTLen();
            maxTlen = ffxParams.getMaxTLen();
            arithmeticFunction = ffxParams.getArithmeticFunction();
            feistelMethod = ffxParams.getFeistelMethod();
            splitter = ffxParams.getSplitter();
            roundCounter = ffxParams.getRoundCounter();
            roundFunction = ffxParams.getRoundFunction();

            // validate radix
            if (radix < 2)
            {
                throw new ArgumentException("radix must be greater than or equal to 2: " + radix);
            }

            // validate minlen
            if (minlen < 2)
            {
                throw new ArgumentException("minlen must be greater than or equal to 2: " + minlen);
            }

            if (Math.Pow(radix, minlen) < 100)
            {
                throw new ArgumentException(
                        "radix^minlen must be greater than or equal to 100: " + Math.Pow(radix, minlen));
            }

            // validate maxlen
            if (maxlen < minlen)
            {
                throw new ArgumentException("maxlen must be greater than or equal to minlen: " + maxlen);
            }

            // validate maxTlen;
            if (maxTlen < 0)
            {
                throw new ArgumentException("maxTlen must be greater than or equal to zero: " + maxTlen);
            }

            // validate method
            if (feistelMethod == null)
            {
                throw new NullReferenceException("method must not be null.");
            }

            // validate arithmetic function
            if (arithmeticFunction == null)
            {
                throw new NullReferenceException("Arithmetic function must not be null.");
            }

            // validate split function
            if (splitter == null)
            {
                throw new NullReferenceException("Split function must not be null.");
            }

            // validate round count function
            if (roundCounter == null)
            {
                throw new NullReferenceException("Round count function must not be null.");
            }

            // validate round function
            if (roundFunction == null)
            {
                throw new NullReferenceException("F must not be null.");
            }
        }

        /**
	     * FFX.Encrypt(K, T, X) - Encrypts a plaintext string of numerals and
	     * produces a ciphertext string of numerals of the same Length and radix.
	     * <p>
	     * 
	     * @param K
	     *            The 128-, 192- or 256-bit AES key.
	     * @param T
	     *            The tweak with Length in the range [minTlen..maxTlen].
	     * @param X
	     *            The plaintext numeral string with Length in the range
	     *            [minlen..maxlen] and element values in the range [0..radix].
	     * @return The ciphertext numeral string of the same Length with element
	     *         values in the same range.
	     * @throws NullReferenceException
	     *             If any of the arguments are null.
	     * @throws ArgumentException
	     *             If the Length of T is not within the range of
	     *             [minTlen..maxTlen]; the Length of X is not within the range
	     *             [minlen..maxlen]; radix<sup>X.Length</sup> is less than 100;
	     *             or any value X[i] is not in the range [0..radix].
	     * @throws InvalidKeyException
	     *             If K is not a valid key for the underlying cipher.
	     */
        public int[] encrypt(byte[] K, byte[] T, int[] X)
        {
            // if K is not in the set of Keys or T is not in the set of Tweaks or X
            // is not in the set of Chars* or |X| is not in the set of Lengths then
            // return null (i.e. throw an appropriate exception)

            // validate K
            if (K == null)
            {
                throw new NullReferenceException("K must not be null.");
            }

            if (!roundFunction.validKey(K))
            {
                throw new ArgumentException("K is not a valid key for F.");
            }

            // validate T
            if (T == null)
            {
                throw new NullReferenceException("T must not be null.");
            }

            if (T.Length < minTlen || T.Length > maxTlen)
            {
                throw new ArgumentException(
                        "The Length of T must be in the range [" + minTlen + ".." + maxTlen + "]: " + T.Length);
            }

            // validate X
            if (X == null)
            {
                throw new NullReferenceException("X must not be null.");
            }

            if (X.Length < minlen)
            {
                throw new ArgumentException(
                        "The Length of X must be greater than or equal to " + minlen + ": " + X.Length);
            }

            if (X.Length > maxlen)
            {
                throw new ArgumentException(
                        "The Length of X must be less than or equal to " + maxlen + ": " + X.Length);
            }

            foreach (int x in X)
            {
                if (x < 0 || x >= radix)
                {
                    throw new ArgumentException("The elements of X must be in the range 0.." + (radix - 1));
                }
            }

            // n <- |X|; l <- split(n); r <- rnds(n)
            int n = X.Length;
            int l = splitter.split(n);
            int r = roundCounter.rnds(n);

            /*
		     * To avoid known attacks, we require that rnds(n) >= 8 if n = 2 *
		     * split(n) or if method = 2 and n = 2 * split(n) + 1, and we require
		     * that rnds(n) >= 4n/split(n) otherwise.
		     */

            // validate rounds
            if ((n == 2 * l || feistelMethod == FeistelMethod.TWO) && r < 8)
            {
                throw new ArgumentException(
                        "FFX requires a minimum of eight rounds for balanced splits or method two: " + r);
            }
            else if (r < 4 * n / l)
            {
                throw new ArgumentException(
                        "FFX requires a minimum of " + 4 * n / l + " rounds for method one with imbalanced splits.");
            }

            // if method = 1 then
            if (feistelMethod == FeistelMethod.ONE)
            {
                // for i <- 0 to r - 1 do
                for (int i = 0; i < r; i++)
                {
                    // A <- X[1..l]; B <- X[l + 1..n]
                    int[] A = new int[l];// Arrays.copyOfRange(X, 0, l);
                    int[] B = new int[n - l]; //Arrays.copyOfRange(X, l, n);
                    //TODO CRITIAL changed to:
                    Array.Copy(X, 0, A, 0, l);
                    Array.Copy(X, l, B, 0, n - l);

                    // C <- A [+] F K (n, T, i, B)
                    int[] C = arithmeticFunction.Add(A, roundFunction.F(K, n, T, i, B));

                    // X <- B || C
                    X = Common.concatenate(B, C);
                }
                // return X
                return X;

            }
            else /* if method = 2 then */
            {
                // A <- X[1..l]; B <- X[l + 1..n]
                int[] A = new int[l]; // Arrays.copyOfRange(X, 0, l);
                int[] B = new int[n - l]; // Arrays.copyOfRange(X, l, n);
                //TODO CRITIAL changed to:
                Array.Copy(X, 0, A, 0, l);
                Array.Copy(X, l, B, 0, n - l);

                // for i <- 0 to r - 1 do
                for (int i = 0; i < r; i++)
                {

                    // C <- A [+] F K (n, T, i, B)
                    int[] C = arithmeticFunction.Add(A, roundFunction.F(K, n, T, i, B));

                    // A <- B; B <- C
                    A = B;
                    B = C;
                }
                // return A || B
                return Common.concatenate(A, B);
            }
        }

        /**
	     * FFX.Decrypt(K, T, Y) - Decrypts a ciphertext string of numerals and
	     * produces a plaintext string of numerals of the same Length and radix.
	     * <p>
	     * 
	     * @param K
	     *            The 128-, 192- or 256-bit AES key.
	     * @param T
	     *            The tweak with Length in the range [minTlen..maxTlen].
	     * @param Y
	     *            The ciphertext numeral string with Length in the range
	     *            [minlen..maxlen] and element values in the range [0..radix].
	     * @return The plaintext numeral string of the same Length with element
	     *         values in the same range.
	     * @throws NullReferenceException
	     *             If any of the arguments are null.
	     * @throws ArgumentException
	     *             If the Length of T is not within the range of
	     *             [minTlen..maxTlen]; the Length of X is not within the range
	     *             [minlen..maxlen]; radix<sup>X.Length</sup> is less than 100;
	     *             or any value X[i] is not in the range [0..radix].
	     * @throws InvalidKeyException
	     *             If K is not a valid key for the underlying cipher.
	     */
        public int[] decrypt(byte[] K, byte[] T, int[] Y)
        {
            // algorithm FFX.Decrypt(K, T, Y )

            // end if

            // if K is not in the set of Keys or T is not in the set of Tweaks or Y
            // is not in the set of Chars or |Y| is not in the set of Lengths then
            // return null (i.e. throw an appropriate exception)

            // validate K
            if (K == null)
            {
                throw new NullReferenceException("K must not be null.");
            }

            if (!roundFunction.validKey(K))
            {
                throw new ArgumentException("K is not a valid key for F.");
            }

            // validate T
            if (T == null)
            {
                throw new NullReferenceException("T must not be null.");
            }

            if (T.Length < minTlen || T.Length > maxTlen)
            {
                throw new ArgumentException(
                        "The Length of T must be in the range [" + minTlen + ".." + maxTlen + "]: " + T.Length);
            }

            // validate X
            if (Y == null)
            {
                throw new NullReferenceException("X must not be null.");
            }

            if (Y.Length < minlen)
            {
                throw new ArgumentException(
                        "The Length of X must be greater than or equal to " + minlen + ": " + Y.Length);
            }

            if (Y.Length > maxlen)
            {
                throw new ArgumentException(
                        "The Length of X must be less than or equal to " + maxlen + ": " + Y.Length);
            }

            foreach (int x in Y)
            {
                if (x < 0 || x >= radix)
                {
                    throw new ArgumentException("The elements of X must be in the range 0.." + (radix - 1));
                }
            }

            // n <- |Y| ; l <- split(n); r <- rnds(n)
            int n = Y.Length;
            int l = splitter.split(n);
            int r = roundCounter.rnds(n);

            /*
		     * To avoid known attacks, we require that rnds(n) >= 8 if n = 2 *
		     * split(n) or if method = 2 and n = 2 * split(n) + 1, and we require
		     * that rnds(n) >= 4n/split(n) otherwise.
		     */

            // validate rounds
            if ((n == 2 * l || feistelMethod == FeistelMethod.TWO) && r < 8)
            {
                throw new ArgumentException(
                        "FFX requires a minimum of eight rounds for balanced splits or method two: " + r);
            }
            else if (r < 4 * n / l)
            {
                throw new ArgumentException(
                        "FFX requires a minimum of " + 4 * n / l + " rounds for method one with imbalanced splits.");
            }

            // if method = 1 then
            if (feistelMethod == FeistelMethod.ONE)
            {
                // for i <- r - 1 downto 0 do
                for (int i = r - 1; i >= 0; i--)
                {
                    // B <- Y [1..n - l]; C <- Y [n - l + 1..n]
                    int[] B = new int[n - l]; // Arrays.copyOfRange(Y, 0, n - l);
                    int[] C = new int[l]; // Arrays.copyOfRange(Y, n - l, n);
                    //TODO CRITICAL changed to:
                    Array.Copy(Y, 0, B, 0, n - l);
                    Array.Copy(Y, n - l, C, 0, l);


                    // A <- C [-] F K (n, T, i, B)
                    int[] A = arithmeticFunction.Subtract(C, roundFunction.F(K, n, T, i, B));

                    // Y <- A || B
                    Y = Common.concatenate(A, B);
                }
                // return Y
                return Y;
            }
            else /* if method = 2 then */
            {
                // A <- Y [1..l]; B <- Y [l + 1..n]

                int[] A = new int[l];// = Arrays.copyOfRange(Y, 0, l);
                int[] B = new int[n - l];// = Arrays.copyOfRange(Y, l, n);
                //TODO CRITICAL change to:
                Array.Copy(Y, 0, A, 0, l);
                Array.Copy(Y, l, B, 0, n - l);

                // for i <- r - 1 downto 0 do
                for (int i = r - 1; i >= 0; i--)
                {
                    // C <- B; B <- A
                    int[] C = B;
                    B = A;

                    // A <- C [-] F K (n, T, i, B)
                    A = arithmeticFunction.Subtract(C, roundFunction.F(K, n, T, i, B));
                }
                // return A || B
                return Common.concatenate(A, B);
            }
        }

    }


    /**
     * The arithmetic functions for the [+] and [-] operations in the Feistel
     * rounds.
     * 
     * @author Kai Johnson
     *
     */
    public interface ArithmeticFunction
    {
        /**
         * Add Y to X
         * 
         * @param X
         *            the first operand with elements in the range [0..radix]
         * @param Y
         *            the second operand with elements in the range [0..radix]
         * @return the result of X [+] Y with Length = X.Length = Y.Length and
         *         elements each in the range [0..radix]
         * @throws NullReferenceException
         *             if X or Y is null
         * @throws ArgumentException
         *             if X.Length != Y.Length, or if any X[i] or Y[i] is not in
         *             the range [0..radix]
         */
        int[] Add(int[] X, int[] Y);

        /**
         * Subtract Y from X
         * 
         * @param X
         *            the first operand with elements in the range [0..radix]
         * @param Y
         *            the second operand with elements in the range [0..radix]
         * @return the result of X [-] Y with Length = X.Length = Y.Length and
         *         elements each in the range [0..radix]
         * @throws ArgumentException
         *             if X.Length != Y.Length, or if any X[i] or Y[i] is not in
         *             the range [0..radix]
         */
        int[] Subtract(int[] X, int[] Y);
    }


    /**
	* FFX parameter set.
	* 
	* @author Kai Johnson
	*
	*/
    public interface FFXParameters
    {
        /**
         * @return the radix
         */
        int getRadix();

        /**
         * @return the minimum Length for plaintext and ciphertext inputs
         */
        int getMinLen();

        /**
         * @return the maximum Length for plaintext and ciphertext inputs
         */
        int getMaxLen();

        /**
         * @return the minimum Length for tweaks
         */
        int getMinTLen();

        /**
         * @return the maximum Length for tweaks
         */
        int getMaxTLen();

        /**
         * @return the arithmetic functions for the [+] and [-] operations in
         *         the Feistel rounds
         */
        ArithmeticFunction getArithmeticFunction();

        /**
         * @return the Feistel method
         */
        FeistelMethod getFeistelMethod();

        /**
         * @return the function to determine where to split input arrays
         */
        SplitFunction getSplitter();

        /**
         * @return the function to determine the number of Feistel rounds
         */
        RoundCounter getRoundCounter();

        /**
         * @return the Feistel round function
         */
        RoundFunction getRoundFunction();
    }

    /**
     * Function to determine where to split input arrays.
     * 
     * @author Kai Johnson
     *
     */
    public interface SplitFunction
    {
        /**
         * The imbalance, a function that takes a permitted Length n in the
         * range [minlen..maxlen] and returns a number 1 &lt;= split(n) &lt;=
         * n/2.
         * 
         * @param n
         *            the Length
         * @return the degree of imbalance
         * @throws ArgumentException
         *             if n is not in the range [minlen..maxlen]
         */
        int split(int n);
    }

    /**
     * Function to determine the number of Feistel rounds.
     * 
     * @author Kai Johnson
     *
     */
    public interface RoundCounter
    {
        /**
         * Returns the number of Feistel rounds for an input of Length n.
         * 
         * @param n
         *            the Length of the input
         * @return the number of Feistel rounds
         * @throws ArgumentException
         *             if n is not in the range [minlen..maxlen]
         */
        int rnds(int n);
    }

    /**
     * Pseudorandom function for Feistel rounds.
     * 
     * @author Kai Johnson
     *
     */
    public interface RoundFunction
    {
        /**
         * Pseudorandom function for Feistel rounds.
         * 
         * @param K
         *            the encryption key of a type compatible with the
         *            underlying cipher
         * @param n
         *            the original Length of the input
         * @param T
         *            the tweak
         * @param i
         *            the index of the current round, in the range [0..rnds(n)]
         * @param B
         *            the array partition to transform
         * @return an array of the same Length as A (i.e. n - B.Length) with
         *         pseudorandom values in the range [0..radix]
         * @throws InvalidKeyException
         *             if the key is invalid or not compatible with the
         *             underlying cipher
         * @throws ArgumentException
         *             If n is not within the range [minlen..maxlen]; the Length
         *             of T is not within the range of [minTlen..maxTlen]; the
         *             Length of B is not within the range [1..ceiling(n/2)]; ;
         *             or any value B[i] is not in the range [0..radix].
         */
        int[] F(byte[] K, int n, byte[] T, int i, int[] B);

        /**
         * Validates the key for the psuedorandom function.
         * 
         * @param K
         *            the key
         * @return true if the key is compatible with the underlying cipher.
         */
        bool validKey(byte[] K);
    }

    /**
    * Types of Feistel methods
    * 
    * @author Kai Johnson
    *
    */
    public enum FeistelMethod
    {
        /**
         * Array is re-partitioned on each round.
         */
        ONE,

        /**
         * Array partitions are swapped on each round.
         */
        TWO
    }
}
