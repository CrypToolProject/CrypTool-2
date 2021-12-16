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
using System.Linq;
using System.Numerics;

namespace FormatPreservingEncryptionWeydstone
{

    public class FF1
    {
        /**
         * The OutputChanged delegate used for registration and invocation of registered methods.
         */
        public event EventHandler<OutputChangedEventArgs> OutputChanged;

        protected virtual void OnOutputChanged(OutputChangedEventArgs e)
        {
            OnOutputChanged(e, true);
        }

        protected virtual void OnOutputChanged(OutputChangedEventArgs e, bool printToConsole)
        {
            if (printToConsole)
            {
                Console.WriteLine(e.Text);
            }

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
            Console.WriteLine("OnProgressChanged: " + e.Progress);
            if (ProgressChanged != null)
            {
                ProgressChanged(this, e);
            }
        }

        /**
	     * The maximum length of a tweak in bytes.
	     */
        private readonly int maxTlen;

        /**
	     * Ciphers instance to provide common cipher functions.
	     */
        private readonly Ciphers mCiphers;

        /**
	     * The radix for symbols to be processed by FF1.
	     */
        private readonly int radix;

        /**
	     * Construct a new FF1 instance with a given radix and maximum tweak length.
	     * 
	     * @param radix
	     *            The radix for symbols to be processed by this instance.
	     * @param maxTlen
	     *            The maximum length of tweaks accepted by this instance.
	     * @ArgumentException ArgumentException
	     *             If radix is not in the range
	     *             [{@value org.fpe4j.Constants#MINRADIX}..{@value org.fpe4j.Constants#MAXRADIX}];
	     *             or if maxTlen is not in the range
	     *             [0..{@value org.fpe4j.Constants#MAXLEN}].
	     */
        public FF1(int radix, int maxTlen)
        {
            // validate radix
            if (radix < Constants.MINRADIX || radix > Constants.MAXRADIX)
            {
                throw new ArgumentException(
                        "Radix must be in the range [" + Constants.MINRADIX + ".." + Constants.MAXRADIX + "]: " + radix);
            }

            // validate maxTlen
            if (maxTlen < 0 || maxTlen > Constants.MAXLEN)
            {
                throw new ArgumentException(
                        "maxTlen must be in the range [0.." + Constants.MAXLEN + "]: " + maxTlen);
            }

            this.radix = radix;
            this.maxTlen = maxTlen;
            mCiphers = new Ciphers();
        }

        /**
	     * NIST SP 800-38G Algorithm 8: FF1.Decrypt(K, T, X) - Decrypt a ciphertext
	     * string of numerals and produce a plaintext string of numerals of the same
	     * length and radix.
	     * <p>
	     * Prerequisites: <br>
	     * Designated cipher function, CIPH, of an approved 128-bit block
	     * cipher;<br>
	     * Key, K, for the block cipher;<br>
	     * Base, radix;<br>
	     * Range of supported message lengths, [minlen..maxlen];<br>
	     * Maximum byte length for tweaks, maxTlen.
	     * <p>
	     * Inputs:<br>
	     * Numeral string, X, in base radix of length n, such that n is in the range
	     * [minlen..maxlen];<br>
	     * Tweak T, a byte string of byte length t, such that t is in the range
	     * [0..maxTlen].
	     * <p>
	     * Output:<br>
	     * Numeral string, Y, such that LEN(Y) = n.
	     * 
	     * @param K
	     *            The 128-, 192- or 256-bit AES key.
	     * @param T
	     *            The tweak with length in the range [0..maxTlen].
	     * @param X
	     *            The ciphertext numeral string.
	     * @return The plaintext numeral string of the same length and radix.
	     * @ArgumentException NullReferenceException
	     *             If any of the arguments are null.
	     * @throws ArgumentException
	     *             If the length of T is not within the range of [0..maxTlen];
	     *             the length of X is not within the range
	     *             [{@value org.fpe4j.Constants#MINLEN}..{@value org.fpe4j.Constants#MAXLEN}];
	     *             radix<sup>X.length</sup> is less than 100; or any value X[i]
	     *             is not in the range [0..radix].
	     * @throws ArgumentException
	     *             If K is not a valid AES key.
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
            // alternatively, we could initialize T to an empty array in this case

            if (T.Length > maxTlen)
            {
                throw new ArgumentException(
                        "The length of T is not within the permitted range of 1.." + maxTlen + ": " + T.Length);
            }

            // validate X
            if (X == null)
            {
                throw new NullReferenceException("X must not be null");
            }

            if (X.Length < Constants.MINLEN || X.Length > Constants.MAXLEN)
            {
                throw new ArgumentException("The length of X is not within the permitted range of "
                        + Constants.MINLEN + ".." + Constants.MAXLEN + ": " + X.Length);
            }

            if (Math.Pow(radix, X.Length) < 100)
            {
                throw new ArgumentException("The length of X must be such that radix ^ length > 100");
            }

            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("FF1.Decrypt()\n"));
                OnOutputChanged(new OutputChangedEventArgs("X is " + Common.intArrayToString(X)));
                OnOutputChanged(new OutputChangedEventArgs("Tweak is " + (T.Length > 0 ? Common.byteArrayToHexString(T) : "empty") + "\n"));
            }

            int n = X.Length;
            int t = T.Length;

            // 1. Let u = Common.floor(n/2); v = n - u.
            int u = Common.floor(n / 2.0);
            int v = n - u;
            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("Step 1\n\tu is " + u + ", v is " + v));
            }

            // 2. Let A = X[1..u]; B = X[u+1..n].


            //TODO CRITICAL
            int[] A = new int[u];// Arrays.copyOfRange(X, 0, u);
            int[] B = new int[n - u];// Arrays.copyOfRange(X, u, n);
            Array.Copy(X, 0, A, 0, u);
            Array.Copy(X, u, B, 0, n - u);

            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("Step 2\n\tA is " + Common.intArrayToString(A) + "\n\tB is " + Common.intArrayToString(B)));
            }

            // 3. Let b = Common.ceiling(ceiling(v * LOG(radix))/8).
            int b = Common.ceiling(Common.ceiling(v * Common.log2(radix)) / 8.0);
            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("Step 3\n\tb is " + b));
            }

            // 4. Let d = 4 * Common.ceiling(b/4)+4
            int d = 4 * Common.ceiling(b / 4.0) + 4;
            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("Step 4\n\td is " + d));
            }

            // 5. Let P = [1] 1 || [2] 1 || [1] 1 || [radix] 3 || [10] 1 ||[u mod
            // 256] 1 || [n] 4 || [t] 4 .
            byte[] tbr = Common.bytestring(radix, 3);
            byte[] fbn = Common.bytestring(n, 4);
            byte[] fbt = Common.bytestring(t, 4);
            byte[] P = {  0x01,  0x02,  0x01, tbr[0], tbr[1], tbr[2],  0x0A,
                    (byte) (Common.mod(u, 256) & 0xFF), fbn[0], fbn[1], fbn[2], fbn[3], fbt[0], fbt[1], fbt[2], fbt[3] };
            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("Step 5\n\tP is " + Common.unsignedByteArrayToString(P) + "\n"));
            }

            // 6. For i from 9 to 0:
            for (int i = 9; i >= 0; i--)
            {
                OnProgressChanged(new ProgressChangedEventArgs((10 - i) / 10d));
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("Round #" + i));
                }

                // i. Let Q = T || [0] (-t-b-1) mod 16 || [i] 1 || [NUMradix (A)] b
                byte[] Q = Common.concatenate(T, Common.bytestring(0, Common.mod(-t - b - 1, 16)));
                Q = Common.concatenate(Q, Common.bytestring(i, 1));
                Q = Common.concatenate(Q, Common.bytestring(Common.num(A, radix), b));
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 6.i.\n\t\tQ is " + Common.unsignedByteArrayToString(Q)));
                }

                // ii. Let R = PRF(P || Q).
                byte[] R = mCiphers.prf2(K, Common.concatenate(P, Q));
                // byte[] R = Common.concatenate(mCiphers.prf2(K, Common.concatenate(P, Q)), P);
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 6.ii.\n\t\tR is " + Common.unsignedByteArrayToString(R)));
                    /*
				     * Psuedocode in NIST SP 800-38G shows:
				     * 
				     * R = PRF(P || Q)
				     * 
				     * However, the sample data shows values that match:
				     * 
				     * R = PRF(P || Q) || P
				     * 
				     * The results are not different for the sample data sets, but
				     * step 6. iii. below would fail for inputs where d > 16 if we
				     * produced values of R that match the sample data.
				     *
				     */
                }

                // iii. Let S be the string of the first d bytes of the following
                // string of ceiling (d/16) blocks: R || CIPH K (R xor [1] 16 ) ||
                // CIPH K (R xor [2] 16 ) ... CIPH K (R xor [ceiling(d/16) - 1] 16 ).
                byte[] S = R;
                for (int j = 1; j <= Common.ceiling(d / 16.0) - 1; j++)
                {
                    S = Common.concatenate(S, mCiphers.ciph(K, Common.xor(R, Common.bytestring(j, 16))));
                }
                //TODO SUPER CRITICAL
                //S = Arrays.copyOf(S, d);
                //Array.Copy(S, S, d);
                S = S.Take(d).ToArray();
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 6.iii.\n\t\tS is " + Common.byteArrayToHexString(S)));
                }

                // iv. Let y = NUM(S).
                BigInteger y = Common.num(S);
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 6.iv.\n\t\ty is " + y));
                }

                // v. If i is even, let m = u; else, let m = v.
                int m = i % 2 == 0 ? u : v;
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 6.v.\n\t\tm is " + m));
                }

                // vi. Let c = (NUMradix (B)-y) mod radix m .
                BigInteger c = Common.mod(Common.num(B, radix) - y, BigInteger.Pow(new BigInteger(radix), m));
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 6.vi.\n\t\tc is " + c));
                }

                // vii. Let C = STR m radix (c).
                int[] C = Common.str(c, radix, m);
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 6.vii.\n\t\tC is " + Common.intArrayToString(C)));
                }

                // viii. Let B = A.
                B = A;
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 6.viii.\n\t\tB is " + Common.intArrayToString(B)));
                }

                // ix. Let A = C.
                A = C;
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 6.ix.\n\t\tA is " + Common.intArrayToString(A)));
                }
            }

            // 7. Return A || B.
            int[] AB = Common.concatenate(A, B);
            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("Step 7.\n\tA || B is " + Common.intArrayToString(AB) + "\n"));
            }
            return AB;
        }

        /**
	     * NIST SP 800-38G Algorithm 7: FF1.Encrypt(K, T, X) - Encrypt a plaintext
	     * string of numerals and produce a ciphertext string of numerals of the
	     * same length and radix.
	     * <p>
	     * Prerequisites:<br>
	     * Designated cipher function, CIPH, of an approved 128-bit block
	     * cipher;<br>
	     * Key, K, for the block cipher; <br>
	     * Base, radix;<br>
	     * Range of supported message lengths, [minlen..maxlen];<br>
	     * Maximum byte length for tweaks, maxTlen.<br>
	     * <p>
	     * Inputs:<br>
	     * Numeral string, X, in base radix of length n, such that n is in the range
	     * [minlen..maxlen];<br>
	     * Tweak T, a byte string of byte length t, such that t is in the range
	     * [0..maxTlen].<br>
	     * <p>
	     * Output:<br>
	     * Numeral string, Y, such that LEN(Y) = n.
	     * 
	     * @param K
	     *            The 128-, 192- or 256-bit AES key.
	     * @param T
	     *            The tweak with length in the range [0..maxTlen].
	     * @param X
	     *            The plaintext numeral string.
	     * @return The ciphertext numeral string of the same length and radix.
	     * @throws NullReferenceException
	     *             If any of the arguments are null.
	     * @throws ArgumentException
	     *             If the length of T is not within the range of [0..maxTlen];
	     *             the length of X is not within the range
	     *             [{@value org.fpe4j.Constants#MINLEN}..{@value org.fpe4j.Constants#MAXLEN}];
	     *             radix<sup>X.length</sup> is less than 100; or any value X[i]
	     *             is not in the range [0..radix].
	     * @throws ArgumentException
	     *             If K is not a valid AES key.
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

            if (T.Length > maxTlen)
            {
                throw new ArgumentException(
                        "The length of T is not within the permitted range of 1.." + maxTlen + ": " + T.Length);
            }

            // validate X
            if (X == null)
            {
                throw new NullReferenceException("X must not be null");
            }

            if (X.Length < Constants.MINLEN || X.Length > Constants.MAXLEN)
            {
                throw new ArgumentException("The length of X is not within the permitted range of "
                        + Constants.MINLEN + ".." + Constants.MAXLEN + ": " + X.Length);
            }

            if (Math.Pow(radix, X.Length) < 100)
            {
                throw new ArgumentException(
                        "The length of X must be such that radix ^ length > 100 (radix ^ length ="
                                + Math.Pow(radix, X.Length));
            }

            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("FF1.Encrypt()\n"));
                OnOutputChanged(new OutputChangedEventArgs("X is " + Common.intArrayToString(X)));
                OnOutputChanged(new OutputChangedEventArgs("Tweak is " + (T.Length > 0 ? Common.byteArrayToHexString(T) : "empty") + "\n"));
            }

            // values of n and t for readability
            int n = X.Length;
            int t = T.Length;

            // 1. Let u = Common.floor(n/2); v = n - u.
            int u = Common.floor(n / 2.0);
            int v = n - u;
            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("Step 1\n\tu is " + u + ", v is " + v));
            }

            // 2. Let A = X[1..u]; B = X[u + 1..n].

            //TODO CRITICAL
            int[] A = new int[u];// Arrays.copyOfRange(X, 0, u);
            int[] B = new int[n - u];// Arrays.copyOfRange(X, u, n);
            Array.Copy(X, 0, A, 0, u);
            Array.Copy(X, u, B, 0, n - u);

            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("Step 2\n\tA is " + Common.intArrayToString(A) + "\n\tB is " + Common.intArrayToString(B)));
            }

            // 3. Let b = Common.ceiling(ceiling(v * LOG(radix))/8).
            int b = Common.ceiling(Common.ceiling(v * Common.log2(radix)) / 8.0);
            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("Step 3\n\tb is " + b));
            }

            // 4. Let d = 4 * Common.ceiling(b/4) + 4.
            int d = 4 * Common.ceiling(b / 4.0) + 4;
            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("Step 4\n\td is " + d));
            }

            // 5. Let P = [1]^1 || [2]^1 || [1]^1 || [radix]^3 || [10]^1 || [u mod
            // 256]^1 || [n]^4 || [t]^4 .
            byte[] tbr = Common.bytestring(radix, 3);
            byte[] fbn = Common.bytestring(n, 4);
            byte[] fbt = Common.bytestring(t, 4);
            byte[] P = {  0x01,  0x02,  0x01, tbr[0], tbr[1], tbr[2],  0x0A,
                    (byte) (Common.mod(u, 256) & 0xFF), fbn[0], fbn[1], fbn[2], fbn[3], fbt[0], fbt[1], fbt[2], fbt[3] };
            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("Step 5\n\tP is " + Common.unsignedByteArrayToString(P) + "\n"));
            }

            // 6. For i from 0 to 9:
            for (int i = 0; i < 10; i++)
            {
                OnProgressChanged(new ProgressChangedEventArgs(i / 10d));
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("Round #" + i));
                }

                // i. Let Q = T || [0]^(-t-b-1) mod 16 || [i]^1 || [NUMradix (B)]^b
                byte[] Q = Common.concatenate(T, Common.bytestring(0, Common.mod(-t - b - 1, 16)));
                Q = Common.concatenate(Q, Common.bytestring(i, 1));
                Q = Common.concatenate(Q, Common.bytestring(Common.num(B, radix), b));
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 6.i.\n\t\tQ is " + Common.unsignedByteArrayToString(Q)));
                }

                // ii. Let R = PRF(P || Q).
                byte[] R = mCiphers.prf(K, Common.concatenate(P, Q));
                // byte[] R = Common.concatenate(prf(K, Common.concatenate(P, Q)), P);
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 6.ii.\n\t\tR is " + Common.unsignedByteArrayToString(R)));
                    /*
				     * Pseudocode in NIST SP 800-38G shows:
				     * 
				     * R = PRF(P || Q)
				     * 
				     * However, the sample data shows values that match:
				     * 
				     * R = PRF(P || Q) || P
				     * 
				     * The results are not different for the sample data sets, but
				     * step 6. iii. below would fail for inputs where d > 16 if we
				     * produced values of R that match the sample data.
				     */
                }

                // iii. Let S be the first d bytes of the following string of
                // Common.ceiling(d/16) blocks: R || CIPH K (R xor [1]^16 ) || CIPH K (R
                // xor [2]^16 ) ... CIPH K (R xor [ceiling(d/16)-1]^16 ).
                byte[] S = R;
                for (int j = 1; j <= Common.ceiling(d / 16.0) - 1; j++)
                {
                    S = Common.concatenate(S, mCiphers.ciph(K, Common.xor(R, Common.bytestring(j, 16))));
                }
                //TODO SUPER CRITICAL
                //S = Arrays.copyOf(S, d);
                //Array.Copy(S, S, d);
                S = S.Take(d).ToArray();
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 6.iii.\n\t\tS is " + Common.byteArrayToHexString(S)));
                }

                // iv. Let y = NUM(S).
                BigInteger y = Common.num(S);
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 6.iv.\n\t\ty is " + y));
                }

                // v. If i is even, let m = u; else, let m = v.
                int m = i % 2 == 0 ? u : v;
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 6.v.\n\t\tm is " + m));
                }

                // vi. Let c = (NUMradix (A)+y) mod radix^m .
                BigInteger c = Common.mod(Common.num(A, radix) + y, BigInteger.Pow(new BigInteger(radix), m));
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 6.vi.\n\t\tc is " + c));
                }

                // vii. Let C = STR m radix (c).
                int[] C = Common.str(c, radix, m);
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 6.vii.\n\t\tC is " + Common.intArrayToString(C)));
                }

                // viii. Let A = B.
                A = B;
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 6.viii.\n\t\tA is " + Common.intArrayToString(A)));
                }

                // ix. Let B = C.
                B = C;
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 6.ix.\n\t\tB is " + Common.intArrayToString(B)));
                }
            }
            // 7. Return A || B.
            int[] AB = Common.concatenate(A, B);
            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("Step 7.\n\tA || B is " + Common.intArrayToString(AB) + "\n"));
            }
            return AB;
        }

    }
}
