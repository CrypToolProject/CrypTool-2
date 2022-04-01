/*
   Copyright 2019 Nils Kopal <Nils.Kopal<at>CrypTool.org

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;

namespace CrypTool.Plugins.FEAL
{
    /// <summary>
    /// Implementation of the Fast Data Encipherement Algorithm (FEAL)
    ///
    /// based on the specification written in the paper:
    /// Shimizu, Akihiro, and Shoji Miyaguchi.
    /// "Fast data encipherment algorithm FEAL."
    /// Workshop on the Theory and Application of of Cryptographic Techniques.
    /// Springer, Berlin, Heidelberg, 1987.
    ///
    /// Test vector for standard FEAL8 rounds:
    ///  P = 00 00 00 00 00 00 00 00
    ///  K = 01 23 45 67 89 AB CD EF
    ///  C = CE EF 2C 86 F2 49 07 52   (without parity bits set in key)
    ///  C = 6A 72 2D 1C 46 B3 93 36   (with parity bits set in key)
    /// </summary>
    public class FEAL_Algorithms
    {

        /// <summary>
        /// Encrypts one block plaintext with FEAL using the given key and 4 rounds
        /// </summary>
        /// <param name="P"></param>
        /// <param name="K"></param>
        /// <returns></returns>
        public static byte[] FEAL4_EncryptBlock(byte[] P, byte[] key)
        {
            if (P == null || P.Length != 8)
            {
                throw new ArgumentException("P has to be byte[8]");
            }

            if (key == null || key.Length != 8)
            {
                throw new ArgumentException("key has to be byte[8]");
            }

            byte[][] K = FEAL_Algorithms.K(key, 6);

            byte[][] L = new byte[4 + 1][];
            byte[][] R = new byte[4 + 1][];

            L[0] = new byte[4];
            R[0] = new byte[4];
            Array.Copy(P, 0, L[0], 0, 4);
            Array.Copy(P, 4, R[0], 0, 4);

            L[0] = XOR(L[0], Concat(K[4], K[5]));
            R[0] = XOR(R[0], Concat(K[6], K[7]));

            R[0] = XOR(R[0], L[0]);

            for (uint r = 1; r <= 4; r++)
            {
                R[r] = XOR(L[r - 1], f(R[r - 1], K[r - 1]));
                L[r] = R[r - 1];
            }

            L[4] = XOR(L[4], R[4]);
            R[4] = XOR(R[4], Concat(K[8], K[9]));
            L[4] = XOR(L[4], Concat(K[10], K[11]));

            return Concat(R[4], L[4]);
        }

        /// <summary>
        /// Decrypts one block ciphertext with FEAL using the given key and 4 rounds
        /// </summary>
        /// <param name="C"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] FEAL4_DecryptBlock(byte[] C, byte[] key)
        {
            if (C == null || C.Length != 8)
            {
                throw new ArgumentException("C has to be byte[8]");
            }

            if (key == null || key.Length != 8)
            {
                throw new ArgumentException("key has to be byte[8]");
            }

            byte[][] K = FEAL_Algorithms.K(key, 6);

            byte[][] R = new byte[4 + 1][];
            byte[][] L = new byte[4 + 1][];

            R[4] = new byte[4];
            L[4] = new byte[4];
            Array.Copy(C, 0, R[4], 0, 4);
            Array.Copy(C, 4, L[4], 0, 4);

            R[4] = XOR(R[4], Concat(K[8], K[9]));
            L[4] = XOR(L[4], Concat(K[10], K[11]));

            L[4] = XOR(L[4], R[4]);

            for (uint r = 4; r >= 1; r--)
            {
                L[r - 1] = XOR(R[r], f(L[r], K[r - 1]));
                R[r - 1] = L[r];
            }

            R[0] = XOR(R[0], L[0]);

            L[0] = XOR(L[0], Concat(K[4], K[5]));
            R[0] = XOR(R[0], Concat(K[6], K[7]));

            return Concat(L[0], R[0]);
        }

        /// <summary>
        /// Encrypts one block plaintext with FEAL using the given key and 8 rounds
        /// </summary>
        /// <param name="P"></param>
        /// <param name="K"></param>
        /// <returns></returns>
        public static byte[] FEAL8_EncryptBlock(byte[] P, byte[] key)
        {
            if (P == null || P.Length != 8)
            {
                throw new ArgumentException("P has to be byte[8]");
            }

            if (key == null || key.Length != 8)
            {
                throw new ArgumentException("key has to be byte[8]");
            }

            byte[][] K = FEAL_Algorithms.K(key, 8);

            byte[][] L = new byte[8 + 1][];
            byte[][] R = new byte[8 + 1][];

            L[0] = new byte[4];
            R[0] = new byte[4];
            Array.Copy(P, 0, L[0], 0, 4);
            Array.Copy(P, 4, R[0], 0, 4);

            L[0] = XOR(L[0], Concat(K[8], K[9]));
            R[0] = XOR(R[0], Concat(K[10], K[11]));

            R[0] = XOR(R[0], L[0]);

            for (uint r = 1; r <= 8; r++)
            {
                R[r] = XOR(L[r - 1], f(R[r - 1], K[r - 1]));
                L[r] = R[r - 1];
            }

            L[8] = XOR(L[8], R[8]);
            R[8] = XOR(R[8], Concat(K[12], K[13]));
            L[8] = XOR(L[8], Concat(K[14], K[15]));

            return Concat(R[8], L[8]);
        }

        /// <summary>
        /// Decrypts one block ciphertext with FEAL using the given key and 8 rounds
        /// </summary>
        /// <param name="C"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] FEAL8_DecryptBlock(byte[] C, byte[] key)
        {
            if (C == null || C.Length != 8)
            {
                throw new ArgumentException("C has to be byte[8]");
            }

            if (key == null || key.Length != 8)
            {
                throw new ArgumentException("key has to be byte[8]");
            }

            byte[][] K = FEAL_Algorithms.K(key, 8);

            byte[][] R = new byte[8 + 1][];
            byte[][] L = new byte[8 + 1][];

            R[8] = new byte[4];
            L[8] = new byte[4];
            Array.Copy(C, 0, R[8], 0, 4);
            Array.Copy(C, 4, L[8], 0, 4);

            R[8] = XOR(R[8], Concat(K[12], K[13]));
            L[8] = XOR(L[8], Concat(K[14], K[15]));

            L[8] = XOR(L[8], R[8]);

            for (uint r = 8; r >= 1; r--)
            {
                L[r - 1] = XOR(R[r], f(L[r], K[r - 1]));
                R[r - 1] = L[r];
            }

            R[0] = XOR(R[0], L[0]);

            L[0] = XOR(L[0], Concat(K[8], K[9]));
            R[0] = XOR(R[0], Concat(K[10], K[11]));

            return Concat(L[0], R[0]);
        }

        /// <summary>
        /// S-function of FEAL
        /// Computes (X1 + X2 + delta) mod 256
        /// </summary>
        /// <param name="X1"></param>
        /// <param name="X2"></param>
        /// <param name="delta"></param>
        /// <returns></returns>
        public static byte S(byte X1, byte X2, byte delta)
        {
            byte T = (byte)((X1 + X2 + delta) % 256);
            return ROT2(T);
        }

        /// <summary>
        /// Rotate a byte by 2 bits to the left
        /// </summary>
        /// <param name="value"></param>
        public static byte ROT2(byte value)
        {
            byte low = (byte)(value & 0xC0);
            low = (byte)(low >> 6);
            return (byte)(((value << 2) & 255) | low);
        }

        /// <summary>
        /// FEAL fk-function
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <returns></returns>
        public static byte[] fk(byte[] alpha, byte[] beta)
        {
            byte[] fk = new byte[4];
            fk[1] = (byte)(alpha[1] ^ alpha[0]);
            fk[2] = (byte)(alpha[2] ^ alpha[3]);
            fk[1] = S(fk[1], (byte)(fk[2] ^ beta[0]), 1);
            fk[2] = S(fk[2], (byte)(fk[1] ^ beta[1]), 0);
            fk[0] = S(alpha[0], (byte)(fk[1] ^ beta[2]), 0);
            fk[3] = S(alpha[3], (byte)(fk[2] ^ beta[3]), 1);
            return fk;
        }

        /// <summary>
        /// FEAL f-function
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <returns></returns>
        public static byte[] f(byte[] alpha, byte[] beta)
        {
            byte[] f = new byte[4];
            f[1] = (byte)(alpha[1] ^ beta[0] ^ alpha[0]);
            f[2] = (byte)(alpha[2] ^ beta[1] ^ alpha[3]);
            f[1] = S(f[1], f[2], 1);
            f[2] = S(f[2], f[1], 0);
            f[0] = S(alpha[0], f[1], 0);
            f[3] = S(alpha[3], f[2], 1);
            return f;
        }

        /// <summary>
        /// Calculates a XOR b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static byte[] XOR(byte[] a, byte[] b)
        {
            byte[] c = new byte[a.Length];
            for (int i = 0; i < c.Length; i++)
            {
                c[i] = (byte)(a[i] ^ b[i]);
            }

            return c;
        }

        /// <summary>
        /// Key processing of FEAL
        /// </summary>
        /// <param name="key"></param>
        /// <param name="rounds"></param>
        /// <returns></returns>
        public static byte[][] K(byte[] key, uint rounds = 8)
        {
            byte[][] K = new byte[rounds * 2][];
            byte[][] A = new byte[rounds + 1][];
            byte[][] B = new byte[rounds + 1][];
            byte[][] D = new byte[rounds + 1][];

            A[0] = new byte[4];
            B[0] = new byte[4];
            D[0] = new byte[4];
            Array.Copy(key, 0, A[0], 0, 4);
            Array.Copy(key, 4, B[0], 0, 4);

            for (uint r = 1; r <= rounds; r++)
            {
                D[r] = A[r - 1];
                A[r] = B[r - 1];
                B[r] = fk(A[r - 1], XOR(B[r - 1], D[r - 1]));
                K[2 * (r - 1)] = new byte[] { B[r][0], B[r][1] };
                K[2 * (r - 1) + 1] = new byte[] { B[r][2], B[r][3] };
            }

            return K;
        }

        /// <summary>
        /// Concatenates 2 byte arrays
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static byte[] Concat(byte[] a, byte[] b)
        {
            byte[] c = new byte[a.Length + b.Length];
            Array.Copy(a, c, a.Length);
            Array.Copy(b, 0, c, a.Length, b.Length);
            return c;
        }
    }
}
