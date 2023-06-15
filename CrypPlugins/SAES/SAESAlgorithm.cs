/*
   Copyright 2023 Nils Kopal, CrypTool project

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
using System.Runtime.CompilerServices;

namespace CrypTool.Plugins.SAES
{
    /// <summary>
    /// Implementation of "A Simplified AES Algorithm" (S-AES) as defined in the article 
    ///  Musa, Mohammad A., Edward F. Schaefer, and Stephen Wedig. 
    ///  "A simplified AES algorithm and its linear and differential cryptanalyses." 
    ///  Cryptologia 27.2 (2003): 148-177.
    /// </summary>
    public class SAESAlgorithm
    {
        #region lookup tables

        /// <summary>
        /// Lookup table for s-box (works on nibbles (=4 bit))
        /// </summary>
        private readonly byte[] _sbox =
        {
            0b1001, 0b0100, 0b1010, 0b1011,
            0b1101, 0b0001, 0b1000, 0b0101,
            0b0110, 0b0010, 0b0000, 0b0011,
            0b1100, 0b1110, 0b1111, 0b0111
        };

        /// <summary>
        /// Lookup table for inverse s-box (works on nibbles (=4 bit))
        /// </summary>
        private readonly byte[] _sboxInverse =
        {
            0b1010, 0b0101, 0b1001, 0b1011,
            0b0001, 0b0111, 0b1000, 0b1111,
            0b0110, 0b0000, 0b0010, 0b0011,
            0b1100, 0b0100, 0b1101, 0b1110,
        };

        /// <summary>
        /// Lookup table for multiplication in GF(2^4) with irreducible polynomial P(x) = x^4 + x + 1
        /// </summary>
        private readonly byte[,] _gf16_multiplication =
        {
            { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 },
            { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF },
            { 0x0, 0x2, 0x4, 0x6, 0x8, 0xA, 0xC, 0xE, 0x3, 0x1, 0x7, 0x5, 0xB, 0x9, 0xF, 0xD },
            { 0x0, 0x3, 0x6, 0x5, 0xC, 0xF, 0xA, 0x9, 0xB, 0x8, 0xD, 0xE, 0x7, 0x4, 0x1, 0x2 },
            { 0x0, 0x4, 0x8, 0xC, 0x3, 0x7, 0xB, 0xF, 0x6, 0x2, 0xE, 0xA, 0x5, 0x1, 0xD, 0x9 },
            { 0x0, 0x5, 0xA, 0xF, 0x7, 0x2, 0xD, 0x8, 0xE, 0xB, 0x4, 0x1, 0x9, 0xC, 0x3, 0x6 },
            { 0x0, 0x6, 0xC, 0xA, 0xB, 0xD, 0x7, 0x1, 0x5, 0x3, 0x9, 0xF, 0xE, 0x8, 0x2, 0x4 },
            { 0x0, 0x7, 0xE, 0x9, 0xF, 0x8, 0x1, 0x6, 0xD, 0xA, 0x3, 0x4, 0x2, 0x5, 0xC, 0xB },
            { 0x0, 0x8, 0x3, 0xB, 0x6, 0xE, 0x5, 0xD, 0xC, 0x4, 0xF, 0x7, 0xA, 0x2, 0x9, 0x1 },
            { 0x0, 0x9, 0x1, 0x8, 0x2, 0xB, 0x3, 0xA, 0x4, 0xD, 0x5, 0xC, 0x6, 0xF, 0x7, 0xE },
            { 0x0, 0xA, 0x7, 0xD, 0xE, 0x4, 0x9, 0x3, 0xF, 0x5, 0x8, 0x2, 0x1, 0xB, 0x6, 0xC },
            { 0x0, 0xB, 0x5, 0xE, 0xA, 0x1, 0xF, 0x4, 0x7, 0xC, 0x2, 0x9, 0xD, 0x6, 0x8, 0x3 },
            { 0x0, 0xC, 0xB, 0x7, 0x5, 0x9, 0xE, 0x2, 0xA, 0x6, 0x1, 0xD, 0xF, 0x3, 0x4, 0x8 },
            { 0x0, 0xD, 0x9, 0x4, 0x1, 0xC, 0x8, 0x5, 0x2, 0xF, 0xB, 0x6, 0x3, 0xE, 0xA, 0x7 },
            { 0x0, 0xE, 0xF, 0x1, 0xD, 0x3, 0x2, 0xC, 0x9, 0x7, 0x6, 0x8, 0x4, 0xA, 0xB, 0x5 },
            { 0x0, 0xF, 0xD, 0x2, 0x9, 0x6, 0x4, 0xB, 0x1, 0xE, 0xC, 0x3, 0x8, 0x7, 0x5, 0xA }
        };

        #endregion

        #region public interface for encryption and decryption

        /// <summary>
        /// Encrypts a 16-bit block plaintext to a 16-bit block ciphertext
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public byte[] EncryptBlock(byte[] plaintext, byte[] key)
        {
            //create round keys
            byte[][] roundkeys = KeyExpansion(key);

            //we work on a state array and not in-place
            byte[] state = new byte[] { plaintext[0], plaintext[1] };

            //initial add round key
            AddRoundKey(state, roundkeys[0]);

            //round 1
            SubstituteNibbles(state);
            ShiftRows(state);
            MixColums(state);
            AddRoundKey(state, roundkeys[1]);

            //round 2 (no mix columns in last round)
            SubstituteNibbles(state);
            ShiftRows(state);
            AddRoundKey(state, roundkeys[2]);

            return state;
        }

        /// <summary>
        /// Decrypts a 16-bit block ciphertext to a 16-bit block plaintext
        /// </summary>
        /// <param name="block"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public byte[] DecryptBlock(byte[] ciphertext, byte[] key)
        {
            //create round keys
            byte[][] roundkeys = KeyExpansion(key);

            //we work on a state array and not in-place
            byte[] state = new byte[] { ciphertext[0], ciphertext[1] };

            //reverse round 2 (no mix columns in last round)
            AddRoundKey(state, roundkeys[2]);
            ShiftRows(state);
            SubstituteNibblesInverse(state);

            //reverse round 1
            AddRoundKey(state, roundkeys[1]);
            MixColumsInverse(state);
            ShiftRows(state);
            SubstituteNibblesInverse(state);

            //reverse initial add round key
            AddRoundKey(state, roundkeys[0]);

            return state;
        }

        #endregion

        #region encryption primitives

        /// <summary>
        /// Multiplies the column vectors with the matrix
        /// | 1 4 |  <=>  |  1   x^2 |
        /// | 4 1 |       | x^2   1  |
        /// </summary>
        /// <param name="state"></param>
        private void MixColums(byte[] state)
        {
            //byte to nibble vectors
            byte s00 = (byte)(state[0] & 0b1111);
            byte s10 = (byte)(state[0] >> 4);
            byte s01 = (byte)(state[1] & 0b1111);
            byte s11 = (byte)(state[1] >> 4);

            //matrix multiplication
            byte s00_ = (byte)(_gf16_multiplication[s00, 1] ^ _gf16_multiplication[s10, 4]);
            byte s10_ = (byte)(_gf16_multiplication[s00, 4] ^ _gf16_multiplication[s10, 1]);
            byte s01_ = (byte)(_gf16_multiplication[s01, 1] ^ _gf16_multiplication[s11, 4]);
            byte s11_ = (byte)(_gf16_multiplication[s01, 4] ^ _gf16_multiplication[s11, 1]);

            //write result back into state
            state[0] = (byte)(s00_ | (s10_ << 4));
            state[1] = (byte)(s01_ | (s11_ << 4));
        }

        /// <summary>
        /// Multiplies the column vectors with the inverse matrix
        /// | 9 2 |  <=>  | x^3 + 1    x  |
        /// | 2 9 |       |     x  x^3 + 1|
        /// </summary>
        /// <param name="state"></param>
        private void MixColumsInverse(byte[] state)
        {
            //byte to nibble vectors
            byte s00 = (byte)(state[0] & 0b1111);
            byte s10 = (byte)(state[0] >> 4);
            byte s01 = (byte)(state[1] & 0b1111);
            byte s11 = (byte)(state[1] >> 4);

            //matrix multiplication
            byte s00_ = (byte)(_gf16_multiplication[s00, 9] ^ _gf16_multiplication[s10, 2]);
            byte s10_ = (byte)(_gf16_multiplication[s00, 2] ^ _gf16_multiplication[s10, 9]);
            byte s01_ = (byte)(_gf16_multiplication[s01, 9] ^ _gf16_multiplication[s11, 2]);
            byte s11_ = (byte)(_gf16_multiplication[s01, 2] ^ _gf16_multiplication[s11, 9]);

            //write result back into state
            state[0] = (byte)(s00_ | (s10_ << 4));
            state[1] = (byte)(s01_ | (s11_ << 4));
        }

        /// <summary>
        /// Swap last nibbles of the two bytes.
        /// In contrast to the original AES, this ShiftRows here is also its own inverse.
        /// Thus, there is no need for a ShiftRowsInverse method
        /// </summary>
        /// <param name="state"></param>
        private void ShiftRows(byte[] state)
        {
            (state[0], state[1]) = ((byte)((state[1] & 0b1111) | (state[0] & 0b11110000)), (byte)((state[0] & 0b1111) | (state[1] & 0b11110000)));
        }

        /// <summary>
        /// Apply s-box on nibbles
        /// </summary>
        /// <param name="state"></param>
        private void SubstituteNibbles(byte[] state)
        {
            state[0] = (byte)(_sbox[state[0] >> 4] << 4 | _sbox[state[0] & 0b1111]);
            state[1] = (byte)(_sbox[state[1] >> 4] << 4 | _sbox[state[1] & 0b1111]);
        }

        /// <summary>
        /// Apply s-box inverse on nibbles
        /// </summary>
        /// <param name="state"></param>
        private void SubstituteNibblesInverse(byte[] state)
        {
            state[0] = (byte)(_sboxInverse[state[0] >> 4] << 4 | _sboxInverse[state[0] & 0b1111]);
            state[1] = (byte)(_sboxInverse[state[1] >> 4] << 4 | _sboxInverse[state[1] & 0b1111]);
        }

        /// <summary>
        /// XORs the state with a round key
        /// </summary>
        /// <param name="state"></param>
        /// <param name="roundkey"></param>
        private void AddRoundKey(byte[] state, byte[] roundkey)
        {
            state[0] = (byte)(state[0] ^ roundkey[0]);
            state[1] = (byte)(state[1] ^ roundkey[1]);
        }

        #endregion

        #region key scheduling

        /// <summary>
        /// Creates six subkeys based on the given key
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private byte[][] KeyExpansion(byte[] key)
        {
            byte[][] roundkeys = new byte[3][];

            roundkeys[0] = new byte[2];
            roundkeys[0][0] = key[0];
            roundkeys[0][1] = key[1];

            roundkeys[1] = new byte[2];
            roundkeys[1][0] = (byte)(roundkeys[0][0] ^ g(roundkeys[0][1], 1));
            roundkeys[1][1] = (byte)(roundkeys[1][0] ^ roundkeys[0][1]);

            roundkeys[2] = new byte[2];
            roundkeys[2][0] = (byte)(roundkeys[1][0] ^ g(roundkeys[1][1], 2));
            roundkeys[2][1] = (byte)(roundkeys[2][0] ^ roundkeys[1][1]);

            return roundkeys;
        }

        /// <summary>
        /// g-function
        /// </summary>
        /// <param name="word"></param>
        /// <param name="roundNo"></param>
        /// <returns></returns>
        private byte g(byte word, int roundNo)
        {
            //rotate word
            word = (byte)((word >> 4) | (word << 4));

            //substitute word
            word = (byte)(_sbox[word >> 4] << 4 | _sbox[word & 0b1111]);

            //xor round constant x^(j+2)
            //we just use a "lookup table" here
            if (roundNo == 1)
            {
                //x^3 and 0b0000
                word = (byte)(word ^ 0b10000000);
            }
            else if (roundNo == 2)
            {
                //x^4 and 0b0000
                word = (byte)(word ^ 0b00110000);
            }
            return word;
        }
        #endregion
    }
}