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
using System.Security.Cryptography;

namespace FormatPreservingEncryptionWeydstone
{
    public class Ciphers
    {
        /**
        * Instance of the AES cipher in ECB mode with no padding.
        */
        private readonly SymmetricAlgorithm mAesEcbCipher;

        /**
	     * Instance of the AES cipher in CBC mode with no padding.
	     */
        private readonly SymmetricAlgorithm mAesCbcCipher;

        /**
	     * Constructs a Ciphers instance with the required AES ciphers.
	     */
        public Ciphers()
        {
            try
            {
                //TODO CRITICAL
                //mAesEcbCipher = Cipher.getInstance("AES/ECB/NoPadding");
                //mAesCbcCipher = Cipher.getInstance("AES/CBC/NoPadding");
                //changed to
                mAesEcbCipher = new AesCryptoServiceProvider
                {
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.None,
                    KeySize = 128
                };

                mAesCbcCipher = new AesCryptoServiceProvider
                {
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.None,
                    KeySize = 128
                };

            }
            catch (PlatformNotSupportedException)
            {
                throw new SystemException();
            }
        }

        /**
	     * NIST SP 800-38G Algorithm 6: PRF(X) - Applies the pseudorandom function
	     * to the input using the supplied key.
	     * <p>
	     * Prerequisites:<br>
	     * Designated cipher function, CIPH, of an approved 128-bit block
	     * cipher;<br>
	     * Key, K, for the block cipher.
	     * <p>
	     * Input:<br>
	     * Block string, X.
	     * <p>
	     * Output:<br>
	     * Block, Y.
	     * 
	     * @param K
	     *            The AES key for the cipher function.
	     * @param X
	     *            The block string input.
	     * @return The output of the function PRF applied to the block X; PRF is
	     *         defined in terms of a given designated cipher function.
	     * @throws InvalidKeyException
	     *             If the key is not a valid AES key.
	     */
        public byte[] prf(byte[] K, byte[] X)
        {
            // validate K
            if (K == null)
            {
                throw new NullReferenceException("K must not be null");
            }

            if (!mAesEcbCipher.ValidKeySize(K.Length * 8))
            {
                throw new ArgumentException("K must contain an AES key");
            }

            // validate X
            if (X == null)
            {
                throw new NullReferenceException("X must not be null");
            }

            if (X.Length < 1 || X.Length > Constants.MAXLEN)
            {
                throw new ArgumentException(
                        "The length of X is not within the permitted range of 1.." + Constants.MAXLEN + ": " + X.Length);
            }

            // 1. Let m = LEN(X)/128.
            // i.e. BYTELEN(X)/16
            int m = X.Length / 16;

            // 2. Let X[1], ..., X[m] be the blocks for which X = X[1] || ... || X[m].
            // we extract the blocks inside the for loop

            // 3. Let Y(0) = bitstring(0,128), and
            byte[] Y = Common.bitstring(false, 128);

            // for j from 1 to m let Y(j) = CIPH(K,Y(j-1) xor X[j]).
            for (int j = 0; j < m; j++)
            {
                //TODO CRITICAL 
                //dest, from, to
                //byte[] Xj = Arrays.copyOfRange(X, j * 16, j * 16 + 16);
                //src, srcFrom, dest, destFrom, length
                byte[] Xj = new byte[16];
                Array.Copy(X, j * 16, Xj, 0, j * 16 + 16 - j * 16);
                //TODO

                //TODO CRITICAL
                //mAesEcbCipher.init(Cipher.ENCRYPT_MODE, K);
                //Y = mAesEcbCipher.doFinal(Common.xor(Y, Xj));


                mAesEcbCipher.Key = K;
                // IV set to zero
                mAesEcbCipher.IV = new byte[16];


                ICryptoTransform encryptor = mAesEcbCipher.CreateEncryptor();
                Y = encryptor.TransformFinalBlock(Common.xor(Y, Xj), 0, 16);

            }

            // 4. Return Y(m).
            return Y;
        }

        /**
	     * Equivalent implementation of the PRF(X) algorithm using the AES CBC
	     * cipher with a zero initialization vector.
	     * <p>
	     * The PRF(X) algorithm is an implementation of CBC mode encryption with a
	     * zero initialization vector. PRF(X) then extracts the last block as the
	     * result. Instead of implementing CBC by hand, this method uses the Java
	     * libraries to perform the same operation, and to demonstrate the
	     * equivalence of the methods.
	     * 
	     * @param K
	     *            The AES key for the cipher function
	     * @param X
	     *            The block string input
	     * @return The output of the function PRF applied to the block X; PRF is
	     *         defined in terms of a given designated cipher function.
	     * @throws InvalidKeyException
	     *             If the key is not a valid AES key.
	     */
        public byte[] prf2(byte[] K, byte[] X)
        {
            // validate K
            if (K == null)
            {
                throw new NullReferenceException("K must not be null");
            }

            if (!mAesCbcCipher.ValidKeySize(K.Length * 8))
            {
                throw new ArgumentException("K must contain an AES key");
            }

            // validate X
            if (X == null)
            {
                throw new NullReferenceException("X must not be null");
            }

            if (X.Length < 1 || X.Length > Constants.MAXLEN)
            {
                throw new ArgumentException(
                        "The length of X is not within the permitted range of 1.." + Constants.MAXLEN + ": " + X.Length);
            }

            byte[] Z;

            byte[] Y = {  0x00,  0x00,  0x00,  0x00,  0x00,  0x00,  0x00,
                         0x00,  0x00,  0x00,  0x00,  0x00,  0x00,  0x00,
                         0x00,  0x00 };

            //TODO CRITICAL
            //mAesCbcCipher.init(Cipher.ENCRYPT_MODE, K, new IvParameterSpec(Y));
            //Z = mAesCbcCipher.doFinal(X);

            mAesCbcCipher.Key = K;
            mAesCbcCipher.IV = Y;
            ICryptoTransform encryptor = mAesCbcCipher.CreateEncryptor();
            Z = encryptor.TransformFinalBlock(X, 0, X.Length);



            //TODO CRITICAL
            //Arrays.copyOfRange(Z, Z.Length - 16, Z.Length);

            byte[] returnValue = new byte[16];
            Array.Copy(Z, Z.Length - 16, returnValue, 0, 16);
            return returnValue;

        }

        /**
	     * Encrypts the input using the AES block cipher in ECB mode using the
	     * specified key.
	     * <p>
	     * Although the ECB mode of operation is not explicitly mentioned in NIST SP
	     * 800-38G, it is implied by the use of the CIPH(X) function in FF1 and FF3.
	     * <p>
	     * To quote NIST SP 800-38G, "For both of the modes, the underlying block
	     * cipher shall be approved, and the block size shall be 128 bits.
	     * Currently, the AES block cipher, with key lengths of 128, 192, or 256
	     * bits, is the only block cipher that fits this profile."
	     * 
	     * @param K
	     *            The AES key for the cipher function
	     * @param X
	     *            The block string input
	     * @return The output of the cipher function applied to the block X.
	     * @throws InvalidKeyException
	     *             If the key is not a valid AES key.
	     */
        public byte[] ciph(byte[] K, byte[] X)
        {
            // validate K
            if (K == null)
            {
                throw new NullReferenceException("K must not be null");
            }

            if (!mAesEcbCipher.ValidKeySize(K.Length * 8))
            {
                throw new ArgumentException("K must contain an AES key");
            }

            // validate X
            if (X == null)
            {
                throw new NullReferenceException("X must not be null");
            }

            if (X.Length < 1 || X.Length > Constants.MAXLEN)
            {
                throw new ArgumentException(
                        "The length of X is not within the permitted range of 1.." + Constants.MAXLEN + ": " + X.Length);
            }

            byte[] cipherText;
            //TODO CRITICAL
            //mAesEcbCipher.init(Cipher.ENCRYPT_MODE, K);
            //cipherText = mAesEcbCipher.doFinal(X);

            mAesEcbCipher.Key = K;
            // IV set to zero
            mAesEcbCipher.IV = new byte[16];
            ICryptoTransform encryptor = mAesEcbCipher.CreateEncryptor();
            cipherText = encryptor.TransformFinalBlock(X, 0, X.Length);
            return cipherText;
        }
    }
}
