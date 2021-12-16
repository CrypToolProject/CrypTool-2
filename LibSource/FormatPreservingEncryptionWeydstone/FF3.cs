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
    public class FF3
    {
        /**
         * The OutputChanged delegate used for registration and invocation of registered methods.
         */
        public event EventHandler<OutputChangedEventArgs> OutputChanged;

        protected virtual void OnOutputChanged(OutputChangedEventArgs e)
        {
            Console.WriteLine(e.Text);
            if (OutputChanged != null)
            {
                OutputChanged(this, e);
            }
        }

        /**
         * The ProgressChanged delegate used for registration and invocation of registered methods.
         */
        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        protected virtual void OnProgressChanged(ProgressChangedEventArgs e)
        {
            if (ProgressChanged != null)
            {
                ProgressChanged(this, e);
            }
        }

        /**
	     * The maximum number of symbols permitted in plaintext and ciphertext
	     * values.
	     */
        private readonly int maxlen;

        /**
	     * Ciphers instance to provide common cipher functions.
	     */
        private readonly Ciphers mCiphers;

        /**
	     * The minimum number of symbols permitted in plaintext and ciphertext
	     * values.
	     */
        private readonly int minlen;

        /**
	     * The radix for symbols to be processed by FF3.
	     */
        private readonly int radix;

        /**
	     * Construct a new FF3 instance with a given radix.
	     * 
	     * @param radix
	     *            The radix for symbols to be processed by this instance.
	     * @throws ArgumentException
	     *             If the radix is not in the range
	     *             [{@value org.fpe4j.Constants#MINRADIX}..{@value org.fpe4j.Constants#MAXRADIX}].
	     */
        public FF3(int radix)
        {
            if (radix < Constants.MINRADIX || radix > Constants.MAXRADIX)
            {
                throw new ArgumentException("Radix must be in the range 2..65536: " + radix);
            }

            this.radix = radix;

            // 2 <= minlen <= maxlen <= 2 * Common.floor(log(2^96)/log(radix))
            minlen = Math.Max(2, Common.ceiling(Math.Log(100) / Math.Log(radix)));
            maxlen = Math.Max(minlen, 2 * Common.floor(Math.Log(Math.Pow(2, 96)) / Math.Log(radix)));

            mCiphers = new Ciphers();
        }

        /**
	     * NIST SP 800-38G Algorithm 10: FF3.Decrypt(K, T, X) - Decrypt a ciphertext
	     * string of numerals and produce a plaintext string of numerals of the same
	     * length and radix.
	     * <p>
	     * Prerequisites:<br>
	     * Designated cipher function, CIPH, of an approved 128-bit block
	     * cipher;<br>
	     * Key, K, for the block cipher;<br>
	     * Base, radix;<br>
	     * Range of supported message lengths, [minlen..maxlen].
	     * <p>
	     * Inputs:<br>
	     * Numeral string, X, in base radix of length n, such that n is in the range
	     * [minlen..maxlen];<br>
	     * Tweak bit string, T, such that LEN(T) = 64.<br>
	     * <p>
	     * Output:<br>
	     * Numeral string, Y, such that LEN(Y) = n.
	     * 
	     * @param K
	     *            The 128-, 192- or 256-bit AES key.
	     * @param T
	     *            8-byte tweak array.
	     * @param X
	     *            The plaintext numeral string.
	     * @return The ciphertext numeral string of the same length and radix.
	     * @throws NullReferenceException
	     *             If any of the arguments are null.
	     * @throws ArgumentException
	     *             If K is not a valid AES key.
	     * @throws ArgumentException
	     *             If T is not 8 bytes long; the length of X is not within the
	     *             range [minlen..maxlen]; or any value X[i] is not in the range
	     *             [0..radix].
	     */
        public int[] decrypt(byte[] K, byte[] T, int[] X)
        {
            // validate K
            if (K == null)
            {
                throw new NullReferenceException("K must not be null");
            }

            // validate T
            if (T == null)
            {
                throw new NullReferenceException("T must not be null");
            }

            if (T.Length != 8)
            {
                throw new ArgumentException("T must be an array of 8 bytes: " + T.Length);
            }

            // validate X
            if (X == null)
            {
                throw new NullReferenceException("X must not be null");
            }

            if (X.Length < minlen || X.Length > maxlen)
            {
                throw new ArgumentException(
                        "The length of X is not within the permitted range of " + minlen + ".." + maxlen + ": " + X.Length);
            }

            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("FF3.Decrypt()\n"));
                OnOutputChanged(new OutputChangedEventArgs("X is " + Common.intArrayToString(X)));
                OnOutputChanged(new OutputChangedEventArgs("Tweak is " + Common.byteArrayToHexString(T) + "\n"));
            }

            // value of n for readability
            int n = X.Length;

            // value of REVB(K) for readability
            byte[] revK = Common.revb(K);

            // 1. Let u = Common.ceiling(n/2); v = n - u.
            int u = Common.ceiling(n / 2.0);
            int v = n - u;
            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("Step 1\n\tu is <" + u + ">, and v is <" + v + ">"));
            }

            // 2. Let A = X[1..u]; B = X[u + 1..n].
            //TODO
            int[] A = new int[u];// Arrays.copyOfRange(X, 0, u);
            int[] B = new int[n - u];// Arrays.copyOfRange(X, u, n);
            Array.Copy(X, 0, A, 0, u);
            Array.Copy(X, u, B, 0, n - u);

            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("Step 2\n\tA is " + Common.intArrayToString(A) + "\n\tB is " + Common.intArrayToString(B)));
            }

            // 3. Let T_L = T[0..31] and T_R = T[32..63]
            //TODO
            byte[] T_L = new byte[4];// Arrays.copyOfRange(T, 0, 4);
            byte[] T_R = new byte[4];// Arrays.copyOfRange(T, 4, 8);
            Array.Copy(T, 0, T_L, 0, 4);
            Array.Copy(T, 4, T_R, 0, 4);

            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs(
                        "Step 3\n\tT_L is " + Common.byteArrayToHexString(T_L) + "\n\tT_R is " + Common.byteArrayToHexString(T_R) + "\n"));
            }

            // 4. For i from 7 to 0:
            for (int i = 7; i >= 0; i--)
            {
                OnProgressChanged(new ProgressChangedEventArgs(8 - i / 8d));
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("Round #" + i));
                }

                // i. If i is even, let m = u and W = T_R ,
                // else let m = v and W =T_L .
                int m = i % 2 == 0 ? u : v;
                byte[] W = i % 2 == 0 ? T_R : T_L;
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 4.i\n\t\tm is <" + m + ">\n\t\tW is " + Common.byteArrayToHexString(W)));
                }

                // ii. Let P = W xor [i]^4 || [NUMradix (REV(A))]^12 .
                byte[] P = Common.concatenate(Common.xor(W, Common.bytestring(i, 4)), Common.bytestring(Common.num(Common.rev(A), radix), 12));
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 4.ii\n\t\tP is " + Common.unsignedByteArrayToString(P)));
                }

                // iii Let S = REVB(CIPH REVB(K) REVB(P)).
                byte[] S = Common.revb(mCiphers.ciph(revK, Common.revb(P)));
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 4.iii\n\t\tS is " + Common.byteArrayToHexString(S)));
                }

                // iv. Let y = NUM(S).
                BigInteger y = Common.num(S);
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 4.iv\n\t\ty is " + y));
                }

                // v. Let c = (NUMradix (REV(B))-y) mod radix m .
                BigInteger c = Common.mod(Common.num(Common.rev(B), radix) - y, BigInteger.Pow(radix, m));
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 4.v\n\t\tc is " + c));
                }

                // vi. Let C = REV(STR m radix (c)).
                int[] C = Common.rev(Common.str(c, radix, m));
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 4.vi\n\t\tC is " + Common.intArrayToString(C)));
                }

                // vii. Let B = A.
                B = A;
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 4.vii\n\t\tB is " + Common.intArrayToString(B)));
                }

                // viii. Let A = C.
                A = C;
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 4.viii\n\tA is " + Common.intArrayToString(A)));
                }
            }
            // 5. Return A || B.
            int[] AB = Common.concatenate(A, B);
            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("Step 5\n\tA || B is " + Common.intArrayToString(AB)));
            }
            return AB;
        }

        /**
	     * NIST SP 800-38G Algorithm 9: FF3.Encrypt(K, T, X) - Encrypt a plaintext
	     * string of numerals and produce a ciphertext string of numerals of the
	     * same length and radix.
	     * <p>
	     * Prerequisites:<br>
	     * Designated cipher function, CIPH, of an approved 128-bit block
	     * cipher;<br>
	     * Key, K, for the block cipher;<br>
	     * Base, radix;<br>
	     * Range of supported message lengths, [minlen..maxlen].<br>
	     * <p>
	     * Inputs: <br>
	     * Numeral string, X, in base radix of length n, such that n is in the range
	     * [minlen..maxlen]<br>
	     * Tweak bit string, T, such that LEN(T) = 64.
	     * <p>
	     * Output:<br>
	     * Numeral string, Y, such that LEN(Y) = n.
	     * 
	     * @param K
	     *            The 128-, 192- or 256-bit AES key.
	     * @param T
	     *            8-byte tweak array.
	     * @param X
	     *            The plaintext numeral string.
	     * @return The ciphertext numeral string of the same length and radix.
	     * @throws NullReferenceException
	     *             If any of the arguments are null.
	     * @throws ArgumentException
	     *             If K is not a valid AES key.
	     * @throws ArgumentException
	     *             If T is not 8 bytes long; the length of X is not within the
	     *             range [minlen..maxlen]; or any value X[i] is not in the range
	     *             [0..radix].
	     */
        public int[] encrypt(byte[] K, byte[] T, int[] X)
        {
            // validate K
            if (K == null)
            {
                throw new NullReferenceException("K must not be null");
            }

            // validate T
            if (T == null)
            {
                throw new NullReferenceException("T must not be null");
            }

            if (T.Length != 8)
            {
                throw new ArgumentException("T must be an array of 8 bytes: " + T.Length);
            }

            // validate X
            if (X == null)
            {
                throw new NullReferenceException("X must not be null");
            }

            if (X.Length < minlen || X.Length > maxlen)
            {
                throw new ArgumentException(
                        "The length of X is not within the permitted range of " + minlen + ".." + maxlen + ": " + X.Length);
            }

            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("FF3.Encrypt()\n"));
                OnOutputChanged(new OutputChangedEventArgs("X is " + Common.intArrayToString(X)));
                OnOutputChanged(new OutputChangedEventArgs("Tweak is " + Common.byteArrayToHexString(T) + "\n"));
            }

            // value of n for readability
            int n = X.Length;

            // value of REVB(K) for readability
            byte[] revK = Common.revb(K);
            /*
		     * Note that this only works if K is in RAW format.
		     */

            // 1. Let u = Common.ceiling(n/2); v = n - u.
            int u = Common.ceiling(n / 2.0);
            int v = n - u;
            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("Step 1\n\tu is <" + u + ">, and v is <" + v + ">"));
            }

            // 2. Let A = X[1..u]; B = X[u + 1..n].
            //TODO
            int[] A = new int[u];// Arrays.copyOfRange(X, 0, u);
            int[] B = new int[n - u];// Arrays.copyOfRange(X, u, n);
            Array.Copy(X, 0, A, 0, u);
            Array.Copy(X, u, B, 0, n - u);

            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("Step 2\n\tA is " + Common.intArrayToString(A) + "\n\tB is " + Common.intArrayToString(B)));
            }

            // 3. Let T_L = T[0..31] and T_R = T[32..63]
            //TODO
            byte[] T_L = new byte[4];// Arrays.copyOfRange(T, 0, 4);
            byte[] T_R = new byte[4];// Arrays.copyOfRange(T, 4, 8);
            Array.Copy(T, 0, T_L, 0, 4);
            Array.Copy(T, 4, T_R, 0, 4);

            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs(
                        "Step 3\n\tT_L is " + Common.byteArrayToHexString(T_L) + "\n\tT_R is " + Common.byteArrayToHexString(T_R) + "\n"));
            }

            // 4. For i from 0 to 7:
            for (int i = 0; i < 8; i++)
            {
                OnProgressChanged(new ProgressChangedEventArgs(i / 8d));
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("Round #" + i));
                }

                // i. If i is even, let m = u and W = T_R ,
                // else let m = v and W = T_L .
                int m = i % 2 == 0 ? u : v;
                byte[] W = i % 2 == 0 ? T_R : T_L;
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 4.i\n\t\tm is <" + m + ">\n\t\tW is " + Common.byteArrayToHexString(W)));
                }

                // ii. Let P = W xor [i] 4 || [NUMradix (REV(B))] 12 .
                byte[] P = Common.concatenate(Common.xor(W, Common.bytestring(i, 4)), Common.bytestring(Common.num(Common.rev(B), radix), 12));
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 4.ii\n\t\tP is " + Common.unsignedByteArrayToString(P)));
                }

                // iii Let S = REVB(CIPH REVB(K) REVB(P)).
                byte[] S = Common.revb(mCiphers.ciph(revK, Common.revb(P)));
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 4.iii\n\t\tS is " + Common.byteArrayToHexString(S)));
                }

                // iv. Let y = NUM(S).
                BigInteger y = Common.num(S);
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 4.iv\n\t\ty is " + y));
                }

                // v. Let c = (NUMradix (REV(A)) + y) mod radix m .
                BigInteger c = Common.mod(Common.num(Common.rev(A), radix) + y, BigInteger.Pow(radix, m));
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 4.v\n\t\tc is " + c));
                }

                // vi. Let C = REV(STR m radix (c)).
                int[] C = Common.rev(Common.str(c, radix, m));
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 4.vi\n\t\tC is " + Common.intArrayToString(C)));
                }

                // vii. Let A = B.
                A = B;
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 4.vii\n\t\tA is " + Common.intArrayToString(A)));
                }

                // viii. Let B = C.
                B = C;
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 4.viii\n\tB is " + Common.intArrayToString(B)));
                }
            }
            // 5. Return A || B.
            int[] AB = Common.concatenate(A, B);
            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("Step 5\n\tA || B is " + Common.intArrayToString(AB)));
            }
            return AB;
        }

        /**
	     * Returns the maximum length of plaintext and ciphertext inputs based on
	     * the radix.
	     * 
	     * @return The maximum length of plaintext and ciphertext inputs based on
	     *         the radix.
	     */
        public int getMaxlen()
        {
            return maxlen;
        }

        /**
	     * Returns the minimum length of plaintext and ciphertext inputs based on
	     * the radix.
	     * 
	     * @return The minimum length of plaintext and ciphertext inputs based on
	     *         the radix.
	     */
        public int getMinlen()
        {
            return minlen;
        }
    }
}
