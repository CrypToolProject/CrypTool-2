/*
   Copyright 2020 Christian Bender christian1.bender@student.uni-siegen.de

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
using System.Collections;

namespace Speck
{
    public static class SpeckCiphers
    {
        private static int _wordSize;
        private static int _blockLength;
        private static int _keyLength;
        private static int _rounds;

        #region Speck32/xx

        /// <summary>
        /// Speck32/64 with encryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck32_64_Encryption(byte[] text, byte[] key)
        {
            _wordSize = 16;
            _blockLength = 32;
            _keyLength = 64;
            _rounds = 22;

            //convert plaintext bytes to plaintext words
            ushort[] ptWords = BytesToWords16(text, 2);

            //convert key bytes to key words
            ushort[] k = BytesToWords16(key, 4);

            //calculate round keys
            ushort[] rk = KeySchedule(k);

            //encrypt
            ushort[] ctWords = Speck32Encrypt(ptWords, rk);

            //convert ciphertext to bytes
            byte[] ctBytes = Words16ToBytes(ctWords, 2);

            return ctBytes;
        }

        /// <summary>
        /// Speck32/64 with decryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck32_64_Decryption(byte[] text, byte[] key)
        {
            _wordSize = 16;
            _blockLength = 32;
            _keyLength = 64;
            _rounds = 22;

            //convert ciphertext bytes to ciphertext words
            ushort[] ctWords = BytesToWords16(text, 2);

            //convert key bytes to key words
            ushort[] k = BytesToWords16(key, 4);

            //calculate round keys
            ushort[] rk = KeySchedule(k);

            //decrypt
            ushort[] ptWords = Speck32Decrypt(ctWords, rk);

            //convert plaintext to bytes
            byte[] ptBytes = Words16ToBytes(ptWords, 2);

            return ptBytes;
        }

        #endregion

        #region Speck48/xx

        /// <summary>
        /// Speck48/72 with encryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck48_72_Encryption(byte[] text, byte[] key)
        {
            _wordSize = 24;
            _blockLength = 48;
            _keyLength = 72;
            _rounds = 22;

            //convert plaintext bytes to plaintext words
            uint[] ptWords = BytesToWords24(text, 2);

            //convert key bytes to key words
            uint[] k = BytesToWords24(key, 3);

            //calculate round keys
            uint[] rk = KeySchedule(k);

            //encrypt
            uint[] ctWords = Speck48Encrypt(ptWords, rk);

            //convert ciphertext to bytes
            byte[] ctBytes = Words24ToBytes(ctWords, 2);

            return ctBytes;
        }

        /// <summary>
        /// Speck48/72 with decryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck48_72_Decryption(byte[] text, byte[] key)
        {
            _wordSize = 24;
            _blockLength = 48;
            _keyLength = 72;
            _rounds = 22;

            //convert ciphertext bytes to ciphertext words
            uint[] ctWords = BytesToWords24(text, 2);

            //convert key bytes to key words
            uint[] k = BytesToWords24(key, 3);

            //calculate round keys
            uint[] rk = KeySchedule(k);

            //decrypt
            uint[] ptWords = Speck48Decrypt(ctWords, rk);

            //convert plaintext to bytes
            byte[] ptBytes = Words24ToBytes(ptWords, 2);

            return ptBytes;
        }

        /// <summary>
        /// Speck48/96 with encryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck48_96_Encryption(byte[] text, byte[] key)
        {
            _wordSize = 24;
            _blockLength = 48;
            _keyLength = 96;
            _rounds = 23;

            //convert plaintext bytes to plaintext words
            uint[] ptWords = BytesToWords24(text, 2);

            //convert key bytes to key words
            uint[] k = BytesToWords24(key, 4);

            //calculate round keys
            uint[] rk = KeySchedule(k);

            //encrypt
            uint[] ctWords = Speck48Encrypt(ptWords, rk);

            //convert ciphertext to bytes
            byte[] ctBytes = Words24ToBytes(ctWords, 2);

            return ctBytes;
        }

        /// <summary>
        /// Speck48/96 with decryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck48_96_Decryption(byte[] text, byte[] key)
        {
            _wordSize = 24;
            _blockLength = 48;
            _keyLength = 96;
            _rounds = 23;

            //convert ciphertext bytes to ciphertext words
            uint[] ctWords = BytesToWords24(text, 2);

            //convert key bytes to key words
            uint[] k = BytesToWords24(key, 4);

            //calculate round keys
            uint[] rk = KeySchedule(k);

            //decrypt
            uint[] ptWords = Speck48Decrypt(ctWords, rk);

            //convert plaintext to bytes
            byte[] ptBytes = Words24ToBytes(ptWords, 2);

            return ptBytes;
        }

        #endregion

        #region Speck64/xx

        /// <summary>
        /// Speck64/96 with encryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck64_96_Encryption(byte[] text, byte[] key)
        {
            _wordSize = 32;
            _blockLength = 64;
            _keyLength = 96;
            _rounds = 26;

            //convert plaintext bytes to plaintext words
            uint[] ptWords = BytesToWords32(text, 2);

            //convert key bytes to key words
            uint[] k = BytesToWords32(key, 3);

            //calculate round keys
            uint[] rk = KeySchedule(k);

            //encrypt
            uint[] ctWords = Speck64Encrypt(ptWords, rk);

            //convert ciphertext to bytes
            byte[] ctBytes = Words32ToBytes(ctWords, 2);

            return ctBytes;
        }

        /// <summary>
        /// Speck64/96 with decryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck64_96_Decryption(byte[] text, byte[] key)
        {
            _wordSize = 32;
            _blockLength = 64;
            _keyLength = 96;
            _rounds = 26;

            //convert ciphertext bytes to ciphertext words
            uint[] ctWords = BytesToWords32(text, 2);

            //convert key bytes to key words
            uint[] k = BytesToWords32(key, 3);

            //calculate round keys
            uint[] rk = KeySchedule(k);

            //decrypt
            uint[] ptWords = Speck64Decrypt(ctWords, rk);

            //convert plaintext to bytes
            byte[] ptBytes = Words32ToBytes(ptWords, 2);

            return ptBytes;
        }

        /// <summary>
        /// Speck64/128 with encryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck64_128_Encryption(byte[] text, byte[] key)
        {
            _wordSize = 32;
            _blockLength = 64;
            _keyLength = 128;
            _rounds = 27;

            //convert plaintext bytes to plaintext words
            uint[] ptWords = BytesToWords32(text, 2);

            //convert key bytes to key words
            uint[] k = BytesToWords32(key, 4);

            //calculate round keys
            uint[] rk = KeySchedule(k);

            //encrypt
            uint[] ctWords = Speck64Encrypt(ptWords, rk);

            //convert ciphertext to bytes
            byte[] ctBytes = Words32ToBytes(ctWords, 2);

            return ctBytes;
        }

        /// <summary>
        /// Speck64/128 with decryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck64_128_Decryption(byte[] text, byte[] key)
        {
            _wordSize = 32;
            _blockLength = 64;
            _keyLength = 128;
            _rounds = 27;

            //convert ciphertext bytes to ciphertext words
            uint[] ctWords = BytesToWords32(text, 2);

            //convert key bytes to key words
            uint[] k = BytesToWords32(key, 4);

            //calculate round keys
            uint[] rk = KeySchedule(k);

            //decrypt
            uint[] ptWords = Speck64Decrypt(ctWords, rk);

            //convert plaintext to bytes
            byte[] ptBytes = Words32ToBytes(ptWords, 2);

            return ptBytes;
        }

        #endregion

        #region Speck96/xx

        /// <summary>
        /// Speck96/96 with encryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck96_96_Encryption(byte[] text, byte[] key)
        {
            _wordSize = 48;
            _blockLength = 96;
            _keyLength = 96;
            _rounds = 28;

            //convert plaintext bytes to plaintext words
            ulong[] ptWords = BytesToWords48(text, 2);

            //convert key bytes to key words
            ulong[] k = BytesToWords48(key, 2);

            //calculate round keys
            ulong[] rk = KeySchedule(k);

            //encrypt
            ulong[] ctWords = Speck96Encrypt(ptWords, rk);

            //convert ciphertext to bytes
            byte[] ctBytes = Words48ToBytes(ctWords, 2);

            return ctBytes;
        }

        /// <summary>
        /// Speck96/96 with decryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck96_96_Decryption(byte[] text, byte[] key)
        {
            _wordSize = 48;
            _blockLength = 96;
            _keyLength = 96;
            _rounds = 28;

            //convert ciphertext bytes to ciphertext words
            ulong[] ctWords = BytesToWords48(text, 2);

            //convert key bytes to key words
            ulong[] k = BytesToWords48(key, 2);

            //calculate round keys
            ulong[] rk = KeySchedule(k);

            //decrypt
            ulong[] ptWords = Speck96Decrypt(ctWords, rk);

            //convert plaintext to bytes
            byte[] ptBytes = Words48ToBytes(ptWords, 2);

            return ptBytes;
        }

        /// <summary>
        /// Speck96/144 with encryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck96_144_Encryption(byte[] text, byte[] key)
        {
            _wordSize = 48;
            _blockLength = 96;
            _keyLength = 144;
            _rounds = 29;

            //convert plaintext bytes to plaintext words
            ulong[] ptWords = BytesToWords48(text, 2);

            //convert key bytes to key words
            ulong[] k = BytesToWords48(key, 3);

            //calculate round keys
            ulong[] rk = KeySchedule(k);

            //encrypt
            ulong[] ctWords = Speck96Encrypt(ptWords, rk);

            //convert ciphertext to bytes
            byte[] ctBytes = Words48ToBytes(ctWords, 2);

            return ctBytes;
        }

        /// <summary>
        /// Speck96/144 with decryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck96_144_Decryption(byte[] text, byte[] key)
        {
            _wordSize = 48;
            _blockLength = 96;
            _keyLength = 144;
            _rounds = 29;

            //convert ciphertext bytes to ciphertext words
            ulong[] ctWords = BytesToWords48(text, 2);

            //convert key bytes to key words
            ulong[] k = BytesToWords48(key, 3);

            //calculate round keys
            ulong[] rk = KeySchedule(k);

            //decrypt
            ulong[] ptWords = Speck96Decrypt(ctWords, rk);

            //convert plaintext to bytes
            byte[] ptBytes = Words48ToBytes(ptWords, 2);

            return ptBytes;
        }

        #endregion

        #region Speck128/xx

        /// <summary>
        /// Speck128/128 with encryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck128_128_Encryption(byte[] text, byte[] key)
        {
            _wordSize = 64;
            _blockLength = 128;
            _keyLength = 128;
            _rounds = 32;

            //convert plaintext bytes to plaintext words
            ulong[] ptWords = BytesToWords64(text, 2);

            //convert key bytes to key words
            ulong[] k = BytesToWords64(key, 2);

            //calculate round keys
            ulong[] rk = KeySchedule(k);

            //encrypt
            ulong[] ctWords = Speck128Encrypt(ptWords, rk);

            //convert ciphertext to bytes
            byte[] ctBytes = Words64ToBytes(ctWords, 2);

            return ctBytes;
        }

        /// <summary>
        /// Speck128/128 with decryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck128_128_Decryption(byte[] text, byte[] key)
        {
            _wordSize = 64;
            _blockLength = 128;
            _keyLength = 128;
            _rounds = 32;

            //convert ciphertext bytes to ciphertext words
            ulong[] ctWords = BytesToWords64(text, 2);

            //convert key bytes to key words
            ulong[] k = BytesToWords64(key, 2);

            //calculate round keys
            ulong[] rk = KeySchedule(k);

            //decrypt
            ulong[] ptWords = Speck128Decrypt(ctWords, rk);

            //convert plaintext to bytes
            byte[] ptBytes = Words64ToBytes(ptWords, 2);

            return ptBytes;
        }

        /// <summary>
        /// Speck128/192 with encryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck128_192_Encryption(byte[] text, byte[] key)
        {
            _wordSize = 64;
            _blockLength = 128;
            _keyLength = 192;
            _rounds = 33;

            //convert plaintext bytes to plaintext words
            ulong[] ptWords = BytesToWords64(text, 2);

            //convert key bytes to key words
            ulong[] k = BytesToWords64(key, 3);

            //calculate round keys
            ulong[] rk = KeySchedule(k);

            //encrypt
            ulong[] ctWords = Speck128Encrypt(ptWords, rk);

            //convert ciphertext to bytes
            byte[] ctBytes = Words64ToBytes(ctWords, 2);

            return ctBytes;
        }

        /// <summary>
        /// Speck128/192 with decryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck128_192_Decryption(byte[] text, byte[] key)
        {
            _wordSize = 64;
            _blockLength = 128;
            _keyLength = 192;
            _rounds = 33;

            //convert ciphertext bytes to ciphertext words
            ulong[] ctWords = BytesToWords64(text, 2);

            //convert key bytes to key words
            ulong[] k = BytesToWords64(key, 3);

            //calculate round keys
            ulong[] rk = KeySchedule(k);

            //decrypt
            ulong[] ptWords = Speck128Decrypt(ctWords, rk);

            //convert plaintext to bytes
            byte[] ptBytes = Words64ToBytes(ptWords, 2);

            return ptBytes;
        }

        /// <summary>
        /// Speck128/256 with encryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck128_256_Encryption(byte[] text, byte[] key)
        {
            _wordSize = 64;
            _blockLength = 128;
            _keyLength = 256;
            _rounds = 34;

            //convert plaintext bytes to plaintext words
            ulong[] ptWords = BytesToWords64(text, 2);

            //convert key bytes to key words
            ulong[] k = BytesToWords64(key, 4);

            //calculate round keys
            ulong[] rk = KeySchedule(k);

            //encrypt
            ulong[] ctWords = Speck128Encrypt(ptWords, rk);

            //convert ciphertext to bytes
            byte[] ctBytes = Words64ToBytes(ctWords, 2);

            return ctBytes;
        }

        /// <summary>
        /// Speck128/256 with decryption mode
        /// </summary>
        /// <param name="text"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Speck128_256_Decryption(byte[] text, byte[] key)
        {
            _wordSize = 64;
            _blockLength = 128;
            _keyLength = 256;
            _rounds = 34;

            //convert ciphertext bytes to ciphertext words
            ulong[] ctWords = BytesToWords64(text, 2);

            //convert key bytes to key words
            ulong[] k = BytesToWords64(key, 4);

            //calculate round keys
            ulong[] rk = KeySchedule(k);

            //decrypt
            ulong[] ptWords = Speck128Decrypt(ctWords, rk);

            //convert plaintext to bytes
            byte[] ptBytes = Words64ToBytes(ptWords, 2);

            return ptBytes;
        }

        #endregion



        #region BytesToWordsRegion

        /// <summary>
        /// Converts bytes to wordCount input words
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="wordCount"></param>
        /// <returns></returns>
        private static ushort[] BytesToWords16(byte[] bytes, int wordCount)
        {
            ushort[] words = new ushort[wordCount];

            int i, j = 0;
            for (i = 0; i < bytes.Length / 2; i++)
            {
                words[i] = (ushort)(bytes[j] | (bytes[j + 1] << 8));
                j += 2;
            }

            return words;
        }

        /// <summary>
        /// Converts bytes to wordCount input words
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="wordCount"></param>
        /// <returns></returns>
        private static uint[] BytesToWords24(byte[] bytes, int wordCount)
        {
            uint[] words = new uint[wordCount];

            int i, j = 0;
            for (i = 0; i < bytes.Length / 3; i++)
            {
                words[i] = bytes[j] | ((uint)bytes[j + 1] << 8) | ((uint)bytes[j + 2] << 16);
                j += 3;
            }

            return words;
        }

        /// <summary>
        /// Converts bytes to wordCount input words
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="wordCount"></param>
        /// <returns></returns>
        private static uint[] BytesToWords32(byte[] bytes, int wordCount)
        {
            uint[] words = new uint[wordCount];

            int i, j = 0;
            for (i = 0; i < bytes.Length / 4; i++)
            {
                words[i] = bytes[j] | ((uint)bytes[j + 1] << 8) | ((uint)bytes[j + 2] << 16) |
                           ((uint)bytes[j + 3] << 24);
                j += 4;
            }

            return words;
        }

        /// <summary>
        /// Converts bytes to wordCount input words
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="wordCount"></param>
        /// <returns></returns>
        private static ulong[] BytesToWords48(byte[] bytes, int wordCount)
        {
            ulong[] words = new ulong[wordCount];

            int i, j = 0;
            for (i = 0; i < (bytes.Length / 6); i++)
            {
                words[i] = bytes[j] | ((ulong)bytes[j + 1] << 8) | ((ulong)bytes[j + 2] << 16) |
                           ((ulong)bytes[j + 3] << 24) | ((ulong)bytes[j + 4] << 32) |
                           ((ulong)bytes[j + 5] << 40);
                j += 6;
            }

            return words;
        }

        /// <summary>
        /// Converts bytes to wordCount input words
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="wordCount"></param>
        /// <returns></returns>
        private static ulong[] BytesToWords64(byte[] bytes, int wordCount)
        {
            ulong[] words = new ulong[wordCount];

            int i, j = 0;
            for (i = 0; i < (bytes.Length / 8); i++)
            {
                words[i] = bytes[j] | ((ulong)bytes[j + 1] << 8) | ((ulong)bytes[j + 2] << 16) |
                           ((ulong)bytes[j + 3] << 24) | ((ulong)bytes[j + 4] << 32) |
                           ((ulong)bytes[j + 5] << 40) | ((ulong)bytes[j + 6] << 48) |
                           ((ulong)bytes[j + 7] << 56);
                j += 8;
            }

            return words;
        }

        #endregion

        #region WordsToBytesRegion

        /// <summary>
        /// Converts numWords words to a byte array
        /// </summary>
        /// <param name="words"></param>
        /// <param name="numWords"></param>
        /// <returns></returns>
        private static byte[] Words16ToBytes(ushort[] words, int numWords)
        {
            byte[] result = new byte[numWords * sizeof(ushort)];

            int i, j = 0;
            for (i = 0; i < numWords; i++)
            {
                result[j] = (byte)words[i];
                result[j + 1] = (byte)(words[i] >> 8);
                j += 2;
            }

            return result;
        }

        /// <summary>
        /// Converts numWords words to a byte array
        /// </summary>
        /// <param name="words"></param>
        /// <param name="numWords"></param>
        /// <returns></returns>
        private static byte[] Words24ToBytes(uint[] words, int numWords)
        {
            byte[] result = new byte[numWords * 3];

            int i, j = 0;
            for (i = 0; i < numWords; i++)
            {
                result[j] = (byte)words[i];
                result[j + 1] = (byte)(words[i] >> 8);
                result[j + 2] = (byte)(words[i] >> 16);
                j += 3;
            }

            return result;
        }

        /// <summary>
        /// Converts numWords words to a byte array
        /// </summary>
        /// <param name="words"></param>
        /// <param name="numWords"></param>
        /// <returns></returns>
        private static byte[] Words32ToBytes(uint[] words, int numWords)
        {
            byte[] result = new byte[numWords * sizeof(uint)];

            int i, j = 0;
            for (i = 0; i < numWords; i++)
            {
                result[j] = (byte)words[i];
                result[j + 1] = (byte)(words[i] >> 8);
                result[j + 2] = (byte)(words[i] >> 16);
                result[j + 3] = (byte)(words[i] >> 24);
                j += 4;
            }

            return result;
        }

        /// <summary>
        /// Converts numWords words to a byte array
        /// </summary>
        /// <param name="words"></param>
        /// <param name="numWords"></param>
        /// <returns></returns>
        private static byte[] Words48ToBytes(ulong[] words, int numWords)
        {
            byte[] result = new byte[numWords * 6];

            int i, j = 0;
            for (i = 0; i < numWords; i++)
            {
                result[j] = (byte)words[i];
                result[j + 1] = (byte)(words[i] >> 8);
                result[j + 2] = (byte)(words[i] >> 16);
                result[j + 3] = (byte)(words[i] >> 24);
                result[j + 4] = (byte)(words[i] >> 32);
                result[j + 5] = (byte)(words[i] >> 40);
                j += 6;
            }

            return result;
        }

        /// <summary>
        /// Converts numWords words to a byte array
        /// </summary>
        /// <param name="words"></param>
        /// <param name="numWords"></param>
        /// <returns></returns>
        private static byte[] Words64ToBytes(ulong[] words, int numWords)
        {
            byte[] result = new byte[numWords * sizeof(ulong)];

            int i, j = 0;
            for (i = 0; i < numWords; i++)
            {
                result[j] = (byte)words[i];
                result[j + 1] = (byte)(words[i] >> 8);
                result[j + 2] = (byte)(words[i] >> 16);
                result[j + 3] = (byte)(words[i] >> 24);
                result[j + 4] = (byte)(words[i] >> 32);
                result[j + 5] = (byte)(words[i] >> 40);
                result[j + 6] = (byte)(words[i] >> 48);
                result[j + 7] = (byte)(words[i] >> 56);
                j += 8;
            }

            return result;
        }

        #endregion

        #region RoundFunctions

        /// <summary>
        /// Round function with 64 bit data types for encryption
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private static ulong[] EncryptionRoundFunction(ulong x, ulong y, ulong z)
        {
            byte[] xBytes = BitConverter.GetBytes(x);
            byte[] xBytesTrimmed = new byte[_wordSize / 8];

            Buffer.BlockCopy(xBytes, 0, xBytesTrimmed, 0, xBytesTrimmed.Length);

            BitArray arrayRotated = new BitArray(xBytesTrimmed);
            arrayRotated = RotateRightBitArray(arrayRotated, 8);

            if (xBytes.Length != xBytesTrimmed.Length)
            {
                xBytesTrimmed = new byte[8];
            }

            arrayRotated.CopyTo(xBytesTrimmed, 0);
            ulong xIntVal = BitConverter.ToUInt64(xBytesTrimmed, 0);

            xIntVal = xIntVal + y;
            xIntVal = xIntVal ^ z;

            byte[] yBytes = BitConverter.GetBytes(y);
            byte[] yBytesTrimmed = new byte[_wordSize / 8];

            Buffer.BlockCopy(yBytes, 0, yBytesTrimmed, 0, yBytesTrimmed.Length);

            arrayRotated = new BitArray(yBytesTrimmed);
            arrayRotated = RotateLeftBitArray(arrayRotated, 3);

            if (yBytes.Length != yBytesTrimmed.Length)
            {
                yBytesTrimmed = new byte[8];
            }

            arrayRotated.CopyTo(yBytesTrimmed, 0);
            ulong yIntVal = BitConverter.ToUInt64(yBytesTrimmed, 0);

            yIntVal = yIntVal ^ xIntVal;

            return new[] { xIntVal, yIntVal };
        }

        /// <summary>
        /// Round function with 32 bit data types for encryption
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private static uint[] EncryptionRoundFunction(uint x, uint y, uint z)
        {
            byte[] xBytes = BitConverter.GetBytes(x);
            byte[] xBytesTrimmed = new byte[_wordSize / 8];

            Buffer.BlockCopy(xBytes, 0, xBytesTrimmed, 0, xBytesTrimmed.Length);

            BitArray arrayRotated = new BitArray(xBytesTrimmed);
            arrayRotated = RotateRightBitArray(arrayRotated, 8);

            if (xBytes.Length != xBytesTrimmed.Length)
            {
                xBytesTrimmed = new byte[4];
            }

            arrayRotated.CopyTo(xBytesTrimmed, 0);

            uint xIntVal = BitConverter.ToUInt32(xBytesTrimmed, 0);

            xIntVal = xIntVal + y;
            xIntVal = xIntVal ^ z;

            byte[] yBytes = BitConverter.GetBytes(y);
            byte[] yBytesTrimmed = new byte[_wordSize / 8];

            Buffer.BlockCopy(yBytes, 0, yBytesTrimmed, 0, yBytesTrimmed.Length);

            arrayRotated = new BitArray(yBytesTrimmed);
            arrayRotated = RotateLeftBitArray(arrayRotated, 3);


            if (yBytes.Length != yBytesTrimmed.Length)
            {
                yBytesTrimmed = new byte[4];
            }


            arrayRotated.CopyTo(yBytesTrimmed, 0);
            uint yIntVal = BitConverter.ToUInt32(yBytesTrimmed, 0);

            yIntVal = yIntVal ^ xIntVal;

            return new[] { xIntVal, yIntVal };
        }

        /// <summary>
        /// Round function with 16 bit data types for encryption
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private static ushort[] EncryptionRoundFunction(ushort x, ushort y, ushort z)
        {
            byte[] xBytes = BitConverter.GetBytes(x);
            byte[] xBytesTrimmed = new byte[_wordSize / 8];

            Buffer.BlockCopy(xBytes, 0, xBytesTrimmed, 0, xBytesTrimmed.Length);

            BitArray arrayRotated = new BitArray(xBytesTrimmed);
            arrayRotated = RotateRightBitArray(arrayRotated, 7);

            arrayRotated.CopyTo(xBytesTrimmed, 0);
            ushort xIntVal = BitConverter.ToUInt16(xBytesTrimmed, 0);

            xIntVal = (ushort)(xIntVal + y);
            xIntVal = (ushort)(xIntVal ^ z);

            byte[] yBytes = BitConverter.GetBytes(y);
            byte[] yBytesTrimmed = new byte[_wordSize / 8];

            Buffer.BlockCopy(yBytes, 0, yBytesTrimmed, 0, yBytesTrimmed.Length);

            arrayRotated = new BitArray(yBytesTrimmed);
            arrayRotated = RotateLeftBitArray(arrayRotated, 2);

            arrayRotated.CopyTo(yBytesTrimmed, 0);
            ushort yIntVal = BitConverter.ToUInt16(yBytesTrimmed, 0);

            yIntVal = (ushort)(yIntVal ^ xIntVal);

            return new[] { xIntVal, yIntVal };
        }

        /// <summary>
        /// Round function with 64 bit data types for decryption
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private static ulong[] DecryptionRoundFunction(ulong x, ulong y, ulong z)
        {
            y = y ^ x;

            byte[] yBytes = BitConverter.GetBytes(y);
            byte[] yBytesTrimmed = new byte[_wordSize / 8];

            Buffer.BlockCopy(yBytes, 0, yBytesTrimmed, 0, yBytesTrimmed.Length);

            BitArray arrayRotated = new BitArray(yBytesTrimmed);
            arrayRotated = RotateRightBitArray(arrayRotated, 3);

            if (yBytes.Length != yBytesTrimmed.Length)
            {
                yBytesTrimmed = new byte[8];
            }

            arrayRotated.CopyTo(yBytesTrimmed, 0);
            ulong yIntVal = BitConverter.ToUInt64(yBytesTrimmed, 0);

            x = x ^ z;
            x = x - yIntVal;

            byte[] xBytes = BitConverter.GetBytes(x);
            byte[] xBytesTrimmed = new byte[_wordSize / 8];

            Buffer.BlockCopy(xBytes, 0, xBytesTrimmed, 0, xBytesTrimmed.Length);

            arrayRotated = new BitArray(xBytesTrimmed);
            arrayRotated = RotateLeftBitArray(arrayRotated, 8);

            if (xBytes.Length != xBytesTrimmed.Length)
            {
                xBytesTrimmed = new byte[8];
            }

            arrayRotated.CopyTo(xBytesTrimmed, 0);
            ulong xIntVal = BitConverter.ToUInt64(xBytesTrimmed, 0);

            return new[] { xIntVal, yIntVal };
        }

        /// <summary>
        /// Round function with 32 bit data types for decryption
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private static uint[] DecryptionRoundFunction(uint x, uint y, uint z)
        {
            y = y ^ x;

            byte[] yBytes = BitConverter.GetBytes(y);
            byte[] yBytesTrimmed = new byte[_wordSize / 8];

            Buffer.BlockCopy(yBytes, 0, yBytesTrimmed, 0, yBytesTrimmed.Length);

            BitArray arrayRotated = new BitArray(yBytesTrimmed);
            arrayRotated = RotateRightBitArray(arrayRotated, 3);

            if (yBytes.Length != yBytesTrimmed.Length)
            {
                yBytesTrimmed = new byte[4];
            }

            arrayRotated.CopyTo(yBytesTrimmed, 0);
            uint yIntVal = BitConverter.ToUInt32(yBytesTrimmed, 0);

            x = x ^ z;
            x = x - yIntVal;

            byte[] xBytes = BitConverter.GetBytes(x);
            byte[] xBytesTrimmed = new byte[_wordSize / 8];

            Buffer.BlockCopy(xBytes, 0, xBytesTrimmed, 0, xBytesTrimmed.Length);

            arrayRotated = new BitArray(xBytesTrimmed);
            arrayRotated = RotateLeftBitArray(arrayRotated, 8);

            if (xBytes.Length != xBytesTrimmed.Length)
            {
                xBytesTrimmed = new byte[4];
            }

            arrayRotated.CopyTo(xBytesTrimmed, 0);
            uint xIntVal = BitConverter.ToUInt32(xBytesTrimmed, 0);

            return new[] { xIntVal, yIntVal };
        }

        /// <summary>
        /// Round function with 16 bit data types for decryption
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        private static ushort[] DecryptionRoundFunction(ushort x, ushort y, ushort z)
        {
            y = (ushort)(y ^ x);

            byte[] yBytes = BitConverter.GetBytes(y);
            byte[] yBytesTrimmed = new byte[_wordSize / 8];

            Buffer.BlockCopy(yBytes, 0, yBytesTrimmed, 0, yBytesTrimmed.Length);

            BitArray arrayRotated = new BitArray(yBytesTrimmed);
            arrayRotated = RotateRightBitArray(arrayRotated, 2);

            arrayRotated.CopyTo(yBytesTrimmed, 0);
            ushort yIntVal = BitConverter.ToUInt16(yBytesTrimmed, 0);

            x = (ushort)(x ^ z);
            x = (ushort)(x - yIntVal);

            byte[] xBytes = BitConverter.GetBytes(x);
            byte[] xBytesTrimmed = new byte[_wordSize / 8];

            Buffer.BlockCopy(xBytes, 0, xBytesTrimmed, 0, xBytesTrimmed.Length);

            arrayRotated = new BitArray(xBytesTrimmed);
            arrayRotated = RotateLeftBitArray(arrayRotated, 7);

            arrayRotated.CopyTo(xBytesTrimmed, 0);
            ushort xIntVal = BitConverter.ToUInt16(xBytesTrimmed, 0);

            return new[] { xIntVal, yIntVal };
        }

        #endregion

        #region KeySchedules

        /// <summary>
        /// Key Schedule with 16 bit data types
        /// </summary>
        /// <param name="k"></param>
        /// <returns></returns>
        private static ushort[] KeySchedule(ushort[] k)
        {
            ushort rounds = (ushort)_rounds;
            ushort i, d = k[3], c = k[2], b = k[1], a = k[0];
            ushort[] rk = new ushort[rounds];

            for (i = 0; i < rounds - 1; i += 3)
            {
                rk[i] = a;
                ushort[] res = EncryptionRoundFunction(b, a, i);
                a = res[1];
                b = res[0];

                rk[i + 1] = a;
                res = EncryptionRoundFunction(c, a, (ushort)(i + 1));
                a = res[1];
                c = res[0];

                rk[i + 2] = a;
                res = EncryptionRoundFunction(d, a, (ushort)(i + 2));
                a = res[1];
                d = res[0];
            }

            rk[rounds - 1] = a;
            return rk;
        }

        /// <summary>
        /// Key Schedule with 32 bit data types
        /// </summary>
        /// <param name="k"></param>
        /// <returns></returns>
        private static uint[] KeySchedule(uint[] k)
        {
            switch (_blockLength)
            {
                case 48:

                    if (_keyLength == 72)
                    {
                        uint rounds = (uint)_rounds;
                        uint i, c = k[2], b = k[1], a = k[0];
                        uint[] rk = new uint[rounds];

                        for (i = 0; i < rounds - 1;)
                        {
                            rk[i] = a;
                            uint[] res = EncryptionRoundFunction(b, a, i++);
                            a = res[1];
                            b = res[0];

                            rk[i] = a;
                            res = EncryptionRoundFunction(c, a, i++);
                            a = res[1];
                            c = res[0];
                        }

                        return rk;
                    }
                    else if (_keyLength == 96)
                    {
                        uint rounds = (uint)_rounds;
                        uint i, d = k[3], c = k[2], b = k[1], a = k[0];
                        uint[] rk = new uint[rounds];

                        for (i = 0; i < rounds - 1;)
                        {
                            rk[i] = a;
                            uint[] res = EncryptionRoundFunction(b, a, i++);
                            a = res[1];
                            b = res[0];

                            rk[i] = a;
                            res = EncryptionRoundFunction(c, a, i++);
                            a = res[1];
                            c = res[0];

                            if (i == rounds)
                            {
                                break;
                            }

                            rk[i] = a;
                            res = EncryptionRoundFunction(d, a, i++);
                            a = res[1];
                            d = res[0];
                        }

                        return rk;
                    }

                    break;

                case 64:

                    if (_keyLength == 96)
                    {
                        uint rounds = (uint)_rounds;
                        uint i, c = k[2], b = k[1], a = k[0];
                        uint[] rk = new uint[rounds];

                        for (i = 0; i < rounds - 1;)
                        {
                            rk[i] = a;
                            uint[] res = EncryptionRoundFunction(b, a, i++);
                            a = res[1];
                            b = res[0];

                            rk[i] = a;
                            res = EncryptionRoundFunction(c, a, i++);
                            a = res[1];
                            c = res[0];
                        }

                        return rk;
                    }
                    else if (_keyLength == 128)
                    {
                        uint rounds = (uint)_rounds;
                        uint i, d = k[3], c = k[2], b = k[1], a = k[0];
                        uint[] rk = new uint[rounds];

                        for (i = 0; i < rounds - 1;)
                        {
                            rk[i] = a;
                            uint[] res = EncryptionRoundFunction(b, a, i++);
                            a = res[1];
                            b = res[0];

                            rk[i] = a;
                            res = EncryptionRoundFunction(c, a, i++);
                            a = res[1];
                            c = res[0];

                            rk[i] = a;
                            res = EncryptionRoundFunction(d, a, i++);
                            a = res[1];
                            d = res[0];
                        }

                        return rk;
                    }

                    break;
            }

            throw new ArgumentException("One or more parameter is invalid. Check configuration of the Cipher.");
        }

        /// <summary>
        /// Key Schedule with 64 bit data types
        /// </summary>
        /// <param name="k"></param>
        /// <returns></returns>
        private static ulong[] KeySchedule(ulong[] k)
        {
            switch (_blockLength)
            {
                case 96:

                    if (_keyLength == 96)
                    {
                        ulong rounds = (ulong)_rounds;
                        ulong i, b = k[1], a = k[0];
                        ulong[] rk = new ulong[rounds];

                        for (i = 0; i < rounds - 1;)
                        {
                            rk[i] = a;
                            ulong[] res = EncryptionRoundFunction(b, a, i++);
                            a = res[1];
                            b = res[0];
                        }

                        rk[rounds - 1] = a;
                        return rk;
                    }
                    else if (_keyLength == 144)
                    {
                        ulong rounds = (ulong)_rounds;
                        ulong i, c = k[2], b = k[1], a = k[0];
                        ulong[] rk = new ulong[rounds];

                        for (i = 0; i < rounds - 2;)
                        {
                            rk[i] = a;
                            ulong[] res = EncryptionRoundFunction(b, a, i++);
                            a = res[1];
                            b = res[0];

                            rk[i] = a;
                            res = EncryptionRoundFunction(c, a, i++);
                            a = res[1];
                            c = res[0];
                        }

                        rk[i] = a;
                        return rk;
                    }

                    break;

                case 128:

                    if (_keyLength == 128)
                    {
                        ulong rounds = (ulong)_rounds;
                        ulong i, b = k[1], a = k[0];
                        ulong[] rk = new ulong[rounds];

                        for (i = 0; i < rounds - 1;)
                        {
                            rk[i] = a;
                            ulong[] res = EncryptionRoundFunction(b, a, i++);
                            a = res[1];
                            b = res[0];
                        }

                        rk[rounds - 1] = a;
                        return rk;
                    }
                    else if (_keyLength == 192)
                    {
                        ulong rounds = (ulong)_rounds;
                        ulong i, c = k[2], b = k[1], a = k[0];
                        ulong[] rk = new ulong[rounds];

                        for (i = 0; i < rounds - 2;)
                        {
                            rk[i] = a;
                            ulong[] res = EncryptionRoundFunction(b, a, i++);
                            a = res[1];
                            b = res[0];

                            rk[i] = a;
                            res = EncryptionRoundFunction(c, a, i++);
                            a = res[1];
                            c = res[0];
                        }

                        rk[i] = a;
                        return rk;
                    }
                    else if (_keyLength == 256)
                    {
                        ulong rounds = (ulong)_rounds;
                        ulong i, d = k[3], c = k[2], b = k[1], a = k[0];
                        ulong[] rk = new ulong[rounds];

                        for (i = 0; i < rounds - 1; i += 3)
                        {
                            rk[i] = a;
                            ulong[] res = EncryptionRoundFunction(b, a, i);
                            a = res[1];
                            b = res[0];

                            rk[i + 1] = a;
                            res = EncryptionRoundFunction(c, a, i + 1);
                            a = res[1];
                            c = res[0];

                            rk[i + 2] = a;
                            res = EncryptionRoundFunction(d, a, i + 2);
                            a = res[1];
                            d = res[0];
                        }

                        rk[rounds - 1] = a;
                        return rk;
                    }

                    break;
            }

            throw new ArgumentException("One or more parameter is invalid. Check configuration of the Cipher.");
        }

        #endregion

        #region Encryption

        /// <summary>
        /// Encryption with 16 bit word length
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="rk"></param>
        /// <returns></returns>
        private static ushort[] Speck32Encrypt(ushort[] pt, ushort[] rk)
        {
            ushort i;
            int rounds = _rounds;
            ushort[] ct = new ushort[2];

            ct[0] = pt[0];
            ct[1] = pt[1];

            for (i = 0; i < rounds; i++)
            {
                ushort[] res = EncryptionRoundFunction(ct[1], ct[0], rk[i]);
                ct[1] = res[0];
                ct[0] = res[1];
            }

            return ct;
        }

        /// <summary>
        /// Encryption with 24 bit word length
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="rk"></param>
        /// <returns></returns>
        private static uint[] Speck48Encrypt(uint[] pt, uint[] rk)
        {
            int i;
            int rounds = _rounds;
            uint[] ct = new uint[2];
            ct[0] = pt[0]; ct[1] = pt[1];

            for (i = 0; i < rounds; i++)
            {
                uint[] res = EncryptionRoundFunction(ct[1], ct[0], rk[i]);
                ct[1] = res[0];
                ct[0] = res[1];
            }

            return ct;
        }

        /// <summary>
        /// Encryption with 32 bit word length
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="rk"></param>
        /// <returns></returns>
        private static uint[] Speck64Encrypt(uint[] pt, uint[] rk)
        {
            uint i;
            int rounds = _rounds;
            uint[] ct = new uint[2];

            ct[0] = pt[0];
            ct[1] = pt[1];

            for (i = 0; i < rounds; i++)
            {
                uint[] res = EncryptionRoundFunction(ct[1], ct[0], rk[i]);
                ct[1] = res[0];
                ct[0] = res[1];
            }

            return ct;
        }

        /// <summary>
        /// Encryption with 48 bit word length
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="rk"></param>
        /// <returns></returns>
        private static ulong[] Speck96Encrypt(ulong[] pt, ulong[] rk)
        {
            int i;
            int rounds = _rounds;
            ulong[] ct = new ulong[2];

            ct[0] = pt[0];
            ct[1] = pt[1];

            for (i = 0; i < rounds; i++)
            {

                ulong[] res = EncryptionRoundFunction(ct[1], ct[0], rk[i]);
                ct[1] = res[0];
                ct[0] = res[1];
            }

            return ct;
        }

        /// <summary>
        /// Encryption with 64 bit word length
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="rk"></param>
        /// <returns></returns>
        private static ulong[] Speck128Encrypt(ulong[] pt, ulong[] rk)
        {
            int i;
            int rounds = _rounds;
            ulong[] ct = new ulong[2];

            ct[0] = pt[0];
            ct[1] = pt[1];

            for (i = 0; i < rounds; i++)
            {

                ulong[] res = EncryptionRoundFunction(ct[1], ct[0], rk[i]);
                ct[1] = res[0];
                ct[0] = res[1];
            }

            return ct;
        }

        #endregion

        #region Decryption

        /// <summary>
        /// Decryption with 16 bit word length
        /// </summary>
        /// <param name="ctWords"></param>
        /// <param name="rk"></param>
        /// <returns></returns>
        private static ushort[] Speck32Decrypt(ushort[] ctWords, ushort[] rk)
        {
            int i = _rounds - 1;

            ushort[] pt = new ushort[2];
            pt[0] = ctWords[0];
            pt[1] = ctWords[1];

            for (; i >= 0; i--)
            {
                ushort[] res = DecryptionRoundFunction(pt[1], pt[0], rk[i]);
                pt[1] = res[0];
                pt[0] = res[1];
            }

            return pt;
        }

        /// <summary>
        /// Decryption with 24 bit word length
        /// </summary>
        /// <param name="ctWords"></param>
        /// <param name="rk"></param>
        /// <returns></returns>
        private static uint[] Speck48Decrypt(uint[] ctWords, uint[] rk)
        {
            int i = _rounds - 1;

            uint[] pt = new uint[2];
            pt[0] = ctWords[0];
            pt[1] = ctWords[1];

            for (; i >= 0;)
            {
                uint[] res = DecryptionRoundFunction(pt[1], pt[0], rk[i--]);
                pt[1] = res[0];
                pt[0] = res[1];
            }

            return pt;
        }

        /// <summary>
        /// Decryption with 32 bit word length
        /// </summary>
        /// <param name="ctWords"></param>
        /// <param name="rk"></param>
        /// <returns></returns>
        private static uint[] Speck64Decrypt(uint[] ctWords, uint[] rk)
        {
            int i = _rounds - 1;

            uint[] pt = new uint[2];
            pt[0] = ctWords[0];
            pt[1] = ctWords[1];

            for (; i >= 0;)
            {
                uint[] res = DecryptionRoundFunction(pt[1], pt[0], rk[i--]);
                pt[1] = res[0];
                pt[0] = res[1];
            }

            return pt;
        }

        /// <summary>
        /// Decryption with 48 bit word length
        /// </summary>
        /// <param name="ctWords"></param>
        /// <param name="rk"></param>
        /// <returns></returns>
        private static ulong[] Speck96Decrypt(ulong[] ctWords, ulong[] rk)
        {
            int i = _rounds - 1;

            ulong[] pt = new ulong[2];
            pt[0] = ctWords[0];
            pt[1] = ctWords[1];

            for (; i >= 0;)
            {
                ulong[] res = DecryptionRoundFunction(pt[1], pt[0], rk[i--]);
                pt[1] = res[0];
                pt[0] = res[1];
            }

            return pt;
        }

        /// <summary>
        /// Decryption with 64 bit word length
        /// </summary>
        /// <param name="ctWords"></param>
        /// <param name="rk"></param>
        /// <returns></returns>
        private static ulong[] Speck128Decrypt(ulong[] ctWords, ulong[] rk)
        {
            int i = _rounds - 1;

            ulong[] pt = new ulong[2];
            pt[0] = ctWords[0];
            pt[1] = ctWords[1];

            for (; i >= 0;)
            {
                ulong[] res = DecryptionRoundFunction(pt[1], pt[0], rk[i--]);
                pt[1] = res[0];
                pt[0] = res[1];
            }

            return pt;
        }

        #endregion

        #region misc

        /// <summary>
        /// Circular left shift of BitArray arr with amount shiftToLeft
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="shiftToLeft"></param>
        /// <returns></returns>
        private static BitArray RotateLeftBitArray(BitArray arr, int shiftToLeft)
        {
            BitArray result = new BitArray(arr.Length, false);
            BitArray tmp = new BitArray(shiftToLeft, false);

            shiftToLeft = shiftToLeft % arr.Length;

            int j = shiftToLeft - 1;
            for (int i = (arr.Length - 1); i >= (arr.Length - shiftToLeft); i--)
            {
                tmp[j] = arr[i];
                j--;
            }

            for (int i = 0; i < arr.Length - shiftToLeft; i++)
            {
                result[i + shiftToLeft] = arr[i];
            }

            for (int i = 0; i < shiftToLeft; i++)
            {
                result[i] = tmp[i];
            }

            return result;
        }

        /// <summary>
        /// Circular right shift of BitArray arr with amount shiftToRight
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="shiftToRight"></param>
        /// <returns></returns>
        private static BitArray RotateRightBitArray(BitArray arr, int shiftToRight)
        {
            return RotateLeftBitArray(arr, arr.Length - shiftToRight);
        }

        /// <summary>
        /// Calculates a XOR b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static byte[] Xor(byte[] a, byte[] b)
        {
            byte[] c = new byte[a.Length];
            for (int i = 0; i < c.Length; i++)
            {
                c[i] = (byte)(a[i] ^ b[i]);
            }

            return c;
        }

        #endregion
    }
}
