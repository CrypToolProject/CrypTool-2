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
using System;
using System.Numerics;

namespace FormatPreservingEncryptionWeydstone
{
    public class FF2
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
         * The minimum number of symbols permitted in plaintext and ciphertext
         * values.
         */
        private readonly int minlen;

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
	     * The radix for symbols to be processed by FF2.
	     */
        private readonly int radix;

        /**
         * The tweak radix for tweak symbols to be processed by FF2.
         */
        private readonly int tweakRadix;

        /**
	     * Construct a new FF2 instance with a given radix and maximum tweak length.
	     * 
	     * @param radix
	     *            The radix for symbols to be processed by this instance.
         * @param tweakRadix
	     *            The tweak radix for tweak symbols to be processed by this instance.
	     * @ArgumentException ArgumentException
	     *             If radix or tweak radix is not in the range
	     *             [{@value org.fpe4j.Constants#MINRADIX_FF2}..{@value org.fpe4j.Constants#MAXRADIX_FF2}];
	     */
        public FF2(int radix, int tweakRadix)
        {
            // validate radix
            if (radix < Constants.MINRADIX_FF2 || radix > Constants.MAXRADIX_FF2)
            {
                throw new ArgumentException(
                        "Radix must be in the range [" + Constants.MINRADIX_FF2 + ".." + Constants.MAXRADIX_FF2 + "]: " + radix);
            }

            // validate tweakRadix
            if (tweakRadix < Constants.MINRADIX_FF2 || tweakRadix > Constants.MAXRADIX_FF2)
            {
                throw new ArgumentException(
                        "Tweak radix must be in the range [" + Constants.MINRADIX_FF2 + ".." + Constants.MAXRADIX_FF2 + "]: " + tweakRadix);
            }

            minlen = 2;
            // if radix is power of 2
            if ((radix != 0) && ((radix & (radix - 1)) == 0))
            {
                maxlen = Math.Max(minlen, 2 * Common.floor(120 / (Math.Log(radix) / Math.Log(2))));
            }
            else
            {
                maxlen = Math.Max(minlen, 2 * Common.floor(98 / (Math.Log(radix) / Math.Log(2))));
            }

            maxTlen = Common.floor(104 / (Math.Log(tweakRadix) / Math.Log(2))) - 1;

            this.radix = radix;
            this.tweakRadix = tweakRadix;
            mCiphers = new Ciphers();
        }

        /**
	     * NIST SP 800-38G: FF2.Decrypt(K, T, X) - Decrypt a ciphertext
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
	     * @param Tweak
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
        public int[] decrypt(byte[] K, byte[] Tweak, int[] X)
        {
            // validate K
            if (K == null)
            {
                throw new NullReferenceException("K must not be null");
            }

            // validate T
            if (Tweak == null)
            {
                throw new NullReferenceException("T must not be null");
            }

            if (Tweak.Length > maxTlen)
            {
                throw new ArgumentException(
                        "The length of T is not within the permitted range of 1.." + maxTlen + ": " + Tweak.Length);
            }

            // validate X
            if (X == null)
            {
                throw new NullReferenceException("X must not be null");
            }

            if (X.Length < minlen || X.Length > maxlen)
            {
                throw new ArgumentException("The length of X is not within the permitted range of "
                        + minlen + ".." + maxlen + ": " + X.Length);
            }

            if (Math.Pow(radix, X.Length) < 100)
            {
                throw new ArgumentException(
                         "The length of X must be such that radix ^ length > 100 (radix ^ length ="
                                 + Math.Pow(radix, X.Length));
            }


            // Converts the Tweak byte array to an integer array, to be able to use integer specific methodes of the class Common (e.g. Common.num(int[],int)).
            // Alternativly these methods could be overloaded to process byte[] inputs. This conversion only occurs once per encryption, hence it shouldnt have a noticeable effect on the performance.
            int[] T = Array.ConvertAll(Tweak, c => (int)c);

            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("FF2.Decrypt()\n"));
                OnOutputChanged(new OutputChangedEventArgs("X is " + Common.intArrayToString(X)));
                OnOutputChanged(new OutputChangedEventArgs("Tweak is " + (T.Length > 0 ? Common.intArrayToString(X) : "empty") + "\n"));
            }

            // values of n and t for readability
            int n = X.Length;
            int t = T.Length;


            /* The FF2 Decrypt Algorithm
             * 1.  Let  u =  Common.floor( n /2 ) ;  v  =  n  –  u . 
             * 2.   Let  A  =  X [1 ..  u ]; B  =   X [ u  + 1 ..  n ] .  
             * 3.  If  t >0,  P = [ radix ] 1  || [ t ] 1  || [ n ] 1  || [ NUM tweak radix ( T )] 13 ; else P = [ radix ] 1  || [ 0] 1  || [ n ] 1  || [ 0] 13 .  
             * 4.  Let  J =   CIPH K ( P )  
             * 5.  For i from 0 to 9: 
             *      i.  Let  Q ←  [ i ] 1  ||  [ NUM  radix  ( B )] 15 
             *      ii.  Let  Y  ← CIPH J ( Q ).  
             *      iii  Let  y  ←  NUM 2 ( Y ).  
             *      iv.  If  i  is  even, let  m =  u ; else, let  m  =  v .   
             *      v. Let z = y mod radix^m   //This line seems to be obsolete, the variable is never used
             *      v.   Let  c  =  ( NUM radix ( A ) -  y ) mod  radix^m .  
             *      vi.  Let  C  =  STR m radix ( c ). 
             *      vii.   Let  A  =  B .  
             *      viii.    Let  B  =  C .  
             * 6.  Return A  ||  B .   
             * 
             */

            // 1. Let u = Common.floor(n/2); v = n - u.
            int u = Common.floor(n / 2.0);
            int v = n - u;
            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("Step 1\n\tu is " + u + ", v is " + v));
            }

            // 2. Let A = X[1..u]; B = X[u + 1..n].

            int[] A = new int[u];
            int[] B = new int[n - u];
            Array.Copy(X, 0, A, 0, u);
            Array.Copy(X, u, B, 0, n - u);

            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("Step 2\n\tA is " + Common.intArrayToString(A) + "\n\tB is " + Common.intArrayToString(B)));
            }

            // 3.If  t > 0,  P = [radix]^1 || [t]^1 || [n]^1 || [NUM tweakRadix(T)]^13
            //          else P = [radix]^1 || [0]^1 || [n]^1 || [0]^13 . 
            byte[] tbr = Common.bytestring(radix, 1);
            byte[] fbn = Common.bytestring(n, 1);

            byte[] P = { tbr[0] };
            if (T.Length > 0)
            {
                byte[] fbt = Common.bytestring(t, 1);
                P = Common.concatenate(P, new byte[] { fbt[0], fbn[0] });
                P = Common.concatenate(P, Common.bytestring(Common.num(T, tweakRadix), 13));
            }
            else
            {
                P = Common.concatenate(P, new byte[] { 0x00, fbn[0] });
                P = Common.concatenate(P, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00
                , 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00});
            }

            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("Step 3\n\tP is " + Common.unsignedByteArrayToString(P)));
            }

            // 4. Let  J = CIPH (K,P)
            // CIPHK is applied to P in  Step  4  to produce  a  128-bit subkey, J

            byte[] J = mCiphers.ciph(K, P);

            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("Step 4\n\tsubkey J is " + Common.unsignedByteArrayToString(J)));
            }


            for (int i = 9; i >= 0; i--)
            {
                OnProgressChanged(new ProgressChangedEventArgs(10 - i / 10d));
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("Round #" + i));
                }
                // i. Let  Q ←  [ i ] 1  ||  [ NUM  radix  ( B )]^15 
                byte[] Q = Common.bytestring(i, 1);
                Q = Common.concatenate(Q, Common.bytestring(Common.num(A, radix), 15));
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 4.i\n\t\tQ is " + Common.unsignedByteArrayToString(Q)));
                }

                // ii.  Let  Y  ← CIPH J ( Q ).  
                byte[] Y = mCiphers.ciph(J, Q);
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 4.ii\n\t\tY is " + Common.unsignedByteArrayToString(Y)));
                }

                // iii  Let  y  ←  NUM 2 ( Y ).  
                BigInteger y = Common.num(Y);
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 4.iii\n\t\ty is " + y));
                }

                // iv.  If  i  is  even, let  m =  u ; else, let  m  =  v .   
                int m = i % 2 == 0 ? u : v;
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 4.iv\n\t\tm is " + m));
                }

                // v.   Let  c  =  ( NUM radix ( B ) -  y ) mod  radix  m .  
                BigInteger c = Common.mod(Common.num(B, radix) - y, BigInteger.Pow(radix, m));
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 4.v\n\t\tc is " + c));
                }

                // vi.  Let  C  =  STR m radix ( c ). 
                int[] C = Common.str(c, radix, m);
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 4.vi\n\t\tC is " + Common.intArrayToString(C)));
                }

                // vii. Let B = A.
                B = A;
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 4.vii\n\t\tA is " + Common.intArrayToString(A)));
                }

                // viii. Let A = C.
                A = C;
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
	     * NIST SP 800-38G: FF2.Encrypt(K, T, X) - Encrypt a plaintext
	     * string of numerals and produce a ciphertext string of numerals of the
	     * same length and radix.
	     * <p>
	     * Prerequisites:<br>
	     * Designated cipher function, CIPH, of an approved 128-bit block
	     * cipher;<br>
	     * Key, K, for the block cipher; <br>
	     * Base, radix;<br>
	     * Range of supported message lengths, [minlen..maxlen];<br>
	     * Maximum length for tweaks, maxTlen.<br>
	     * <p>
	     * Inputs:<br>
	     * Numeral string, X, in base radix of length n, such that n is in the range
	     * [minlen..maxlen];<br>
	     * Tweak T, a numeral string in base tweak radix of length t, such that t is in the range
	     * [0..maxTlen].<br>
	     * <p>
	     * Output:<br>
	     * Numeral string, Y, such that LEN(Y) = n.
	     * 
	     * @param K
	     *            The 128-, 192- or 256-bit AES key.
	     * @param Tweak
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
        public int[] encrypt(byte[] K, byte[] Tweak, int[] X)
        {
            // validate K
            if (K == null)
            {
                throw new NullReferenceException("K must not be null");
            }

            // validate T
            if (Tweak == null)
            {
                throw new NullReferenceException("T must not be null");
            }

            if (Tweak.Length > maxTlen)
            {
                throw new ArgumentException(
                        "The length of T is not within the permitted range of 1.." + maxTlen + ": " + Tweak.Length);
            }

            // validate X
            if (X == null)
            {
                throw new NullReferenceException("X must not be null");
            }

            if (X.Length < minlen || X.Length > maxlen)
            {
                throw new ArgumentException("The length of X is not within the permitted range of "
                        + minlen + ".." + maxlen + ": " + X.Length);
            }

            if (Math.Pow(radix, X.Length) < 100)
            {
                throw new ArgumentException(
                        "The length of X must be such that radix ^ length > 100 (radix ^ length ="
                                + Math.Pow(radix, X.Length));
            }


            // Converts the Tweak byte array to an integer array, to be able to use integer specific methodes of the class Common (e.g. Common.num(int[], int)).
            // Alternativly these methods could be overloaded to process byte[] inputs. Since this conversion only occurs once per encryption, it shouldnt have a noticeable affect on the performance.
            int[] T = Array.ConvertAll(Tweak, c => (int)c);

            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("FF2.Encrypt()\n"));
                OnOutputChanged(new OutputChangedEventArgs("X is " + Common.intArrayToString(X)));
                OnOutputChanged(new OutputChangedEventArgs("Tweak is " + (Tweak.Length > 0 ? Common.intArrayToString(T) : "empty") + "\n"));
            }

            // values of n and t for readability
            int n = X.Length;
            int t = T.Length;

            /* The FF2 Encrypt Algorithm
             * 1.  Let  u =  Common.floor( n /2 ) ;  v  =  n  –  u . 
             * 2.   Let  A  =  X [1 ..  u ]; B  =   X [ u  + 1 ..  n ] .  
             * 3.  If  t >0,  P = [ radix ] 1  || [ t ] 1  || [ n ] 1  || [ NUM tweak radix ( T )] 13 ; else P = [ radix ] 1  || [ 0] 1  || [ n ] 1  || [ 0] 13 .  
             * 4.  Let  J =   CIPH K ( P )  
             * 5.  For i from 0 to 9: 
             *      i.  Let  Q ←  [ i ] 1  ||  [ NUM  radix  ( B )] 15 
             *      ii.  Let  Y  ← CIPH J ( Q ).  
             *      iii  Let  y  ←  NUM 2 ( Y ).  
             *      iv.  If  i  is  even, let  m =  u ; else, let  m  =  v .   
             *      v.   Let  c  =  ( NUM radix ( A ) +  y ) mod  radix  m .  
             *      vi.  Let  C  =  STR m radix ( c ). 
             *      vii.   Let  A  =  B .  
             *      viii.    Let  B  =  C .  
             * 6.  Return A  ||  B .   
             * 
             */

            // 1. Let u = Common.floor(n/2); v = n - u.
            int u = Common.floor(n / 2.0);
            int v = n - u;
            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("Step 1\n\tu is " + u + ", v is " + v));
            }

            // 2. Let A = X[1..u]; B = X[u + 1..n].

            int[] A = new int[u];
            int[] B = new int[n - u];
            Array.Copy(X, 0, A, 0, u);
            Array.Copy(X, u, B, 0, n - u);

            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("Step 2\n\tA is " + Common.intArrayToString(A) + "\n\tB is " + Common.intArrayToString(B)));
            }

            // 3.If  t > 0,  P = [radix]^1 || [t]^1 || [n]^1 || [NUM tweakRadix(T)]^13
            //          else P = [radix]^1 || [0]^1 || [n]^1 || [0]^13 . 
            byte[] tbr = Common.bytestring(radix, 1);
            byte[] fbn = Common.bytestring(n, 1);

            byte[] P = { tbr[0] };
            if (T.Length > 0)
            {
                byte[] fbt = Common.bytestring(t, 1);
                P = Common.concatenate(P, new byte[] { fbt[0], fbn[0] });
                P = Common.concatenate(P, Common.bytestring(Common.num(T, tweakRadix), 13));
            }
            else
            {
                P = Common.concatenate(P, new byte[] { 0x00, fbn[0] });
                P = Common.concatenate(P, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00
                , 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00});
            }

            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("Step 3\n\tP is " + Common.unsignedByteArrayToString(P)));
            }

            // 4. Let  J = CIPH (K,P)
            // CIPHK is applied to P in  Step  4  to produce  a  128-bit subkey, J

            byte[] J = mCiphers.ciph(K, P);

            if (Constants.CONFORMANCE_OUTPUT)
            {
                OnOutputChanged(new OutputChangedEventArgs("Step 4\n\tsubkey J is " + Common.unsignedByteArrayToString(J)));
            }


            for (int i = 0; i < 10; i++)
            {
                OnProgressChanged(new ProgressChangedEventArgs(i / 10d));
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("Round #" + i));
                }
                // i. Let  Q ←  [ i ] 1  ||  [ NUM  radix  ( B )]^15 
                byte[] Q = Common.bytestring(i, 1);
                Q = Common.concatenate(Q, Common.bytestring(Common.num(B, radix), 15));
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 4.ii\n\t\tQ is " + Common.unsignedByteArrayToString(Q)));
                }

                // ii.  Let  Y  ← CIPH J ( Q ).  
                byte[] Y = mCiphers.ciph(J, Q);
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 4.ii\n\t\tY is " + Common.unsignedByteArrayToString(Y)));
                }

                // iii  Let  y  ←  NUM 2 ( Y ).  
                BigInteger y = Common.num(Y);
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 4.iii\n\t\ty is " + y));
                }

                // iv.  If  i  is  even, let  m =  u ; else, let  m  =  v .   
                int m = i % 2 == 0 ? u : v;
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 4.iv\n\t\tm is " + m));
                }

                // v.   Let  c  =  ( NUM radix ( A ) +  y ) mod  radix  m .  
                BigInteger c = Common.mod(Common.num(A, radix) + y, BigInteger.Pow(radix, m));
                if (Constants.CONFORMANCE_OUTPUT)
                {
                    OnOutputChanged(new OutputChangedEventArgs("\tStep 4.v\n\t\tc is " + c));
                }

                // vi.  Let  C  =  STR m radix ( c ). 
                int[] C = Common.str(c, radix, m);
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

        /**
        * Returns the maximum length of plaintext and ciphertext inputs based on
        * the radix.
        * 
        * @return The maximum length of plaintext and ciphertext inputs based on
        *         the radix.
        */
        public int getMaxTlen()
        {
            return maxTlen;
        }
    }
}
