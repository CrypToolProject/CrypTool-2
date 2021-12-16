using System;

namespace CrypTool.Plugins.MD5Collider.Algorithm
{
    /// <summary>
    /// Calculates the IHV (intermediate hash value) from applying the MD5 compression function to a given byte array
    /// </summary>
    internal class IHVCalculator
    {
        /// <summary>
        /// The byte array which is processed
        /// </summary>
        private readonly byte[] data;

        /// <summary>
        /// Constructs the calculator, specifying data to run through the compression function
        /// </summary>
        /// <param name="data">The data to run through the compression function, must be a multiple of 64 bytes long</param>
        public IHVCalculator(byte[] data)
        {
            this.data = data;
        }

        /// <summary>
        /// Chains the MD5 compression function with default IHV on the given input
        /// </summary>
        /// <returns>The resulting IHV</returns>
        public byte[] GetIHV()
        {
            int offset = 0;

            uint[] hv = new uint[] { 0x67452301, 0xEFCDAB89, 0x98BADCFE, 0x10325476 };
            while (data.Length > offset)
            {
                uint[] dataBlock = toLittleEndianIntegerArray(data, offset, 16);
                md5_compress(hv, dataBlock);
                offset += 64;
            }

            byte[] result = new byte[16];
            dumpLittleEndianIntegers(hv, result, 0);

            return result;
        }

        /// <summary>
        /// Internally used function applying the compression function once
        /// </summary>
        /// <param name="ihv">IHV</param>
        /// <param name="block">The data to compress, must be exactly 64 bytes</param>
        private void md5_compress(uint[] ihv, uint[] block)
        {
            uint a = ihv[0];
            uint b = ihv[1];
            uint c = ihv[2];
            uint d = ihv[3];

            MD5_STEP(FF, ref a, b, c, d, block[0], 0xd76aa478, 7);
            MD5_STEP(FF, ref d, a, b, c, block[1], 0xe8c7b756, 12);
            MD5_STEP(FF, ref c, d, a, b, block[2], 0x242070db, 17);
            MD5_STEP(FF, ref b, c, d, a, block[3], 0xc1bdceee, 22);
            MD5_STEP(FF, ref a, b, c, d, block[4], 0xf57c0faf, 7);
            MD5_STEP(FF, ref d, a, b, c, block[5], 0x4787c62a, 12);
            MD5_STEP(FF, ref c, d, a, b, block[6], 0xa8304613, 17);
            MD5_STEP(FF, ref b, c, d, a, block[7], 0xfd469501, 22);
            MD5_STEP(FF, ref a, b, c, d, block[8], 0x698098d8, 7);
            MD5_STEP(FF, ref d, a, b, c, block[9], 0x8b44f7af, 12);
            MD5_STEP(FF, ref c, d, a, b, block[10], 0xffff5bb1, 17);
            MD5_STEP(FF, ref b, c, d, a, block[11], 0x895cd7be, 22);
            MD5_STEP(FF, ref a, b, c, d, block[12], 0x6b901122, 7);
            MD5_STEP(FF, ref d, a, b, c, block[13], 0xfd987193, 12);
            MD5_STEP(FF, ref c, d, a, b, block[14], 0xa679438e, 17);
            MD5_STEP(FF, ref b, c, d, a, block[15], 0x49b40821, 22);
            MD5_STEP(GG, ref a, b, c, d, block[1], 0xf61e2562, 5);
            MD5_STEP(GG, ref d, a, b, c, block[6], 0xc040b340, 9);
            MD5_STEP(GG, ref c, d, a, b, block[11], 0x265e5a51, 14);
            MD5_STEP(GG, ref b, c, d, a, block[0], 0xe9b6c7aa, 20);
            MD5_STEP(GG, ref a, b, c, d, block[5], 0xd62f105d, 5);
            MD5_STEP(GG, ref d, a, b, c, block[10], 0x02441453, 9);
            MD5_STEP(GG, ref c, d, a, b, block[15], 0xd8a1e681, 14);
            MD5_STEP(GG, ref b, c, d, a, block[4], 0xe7d3fbc8, 20);
            MD5_STEP(GG, ref a, b, c, d, block[9], 0x21e1cde6, 5);
            MD5_STEP(GG, ref d, a, b, c, block[14], 0xc33707d6, 9);
            MD5_STEP(GG, ref c, d, a, b, block[3], 0xf4d50d87, 14);
            MD5_STEP(GG, ref b, c, d, a, block[8], 0x455a14ed, 20);
            MD5_STEP(GG, ref a, b, c, d, block[13], 0xa9e3e905, 5);
            MD5_STEP(GG, ref d, a, b, c, block[2], 0xfcefa3f8, 9);
            MD5_STEP(GG, ref c, d, a, b, block[7], 0x676f02d9, 14);
            MD5_STEP(GG, ref b, c, d, a, block[12], 0x8d2a4c8a, 20);
            MD5_STEP(HH, ref a, b, c, d, block[5], 0xfffa3942, 4);
            MD5_STEP(HH, ref d, a, b, c, block[8], 0x8771f681, 11);
            MD5_STEP(HH, ref c, d, a, b, block[11], 0x6d9d6122, 16);
            MD5_STEP(HH, ref b, c, d, a, block[14], 0xfde5380c, 23);
            MD5_STEP(HH, ref a, b, c, d, block[1], 0xa4beea44, 4);
            MD5_STEP(HH, ref d, a, b, c, block[4], 0x4bdecfa9, 11);
            MD5_STEP(HH, ref c, d, a, b, block[7], 0xf6bb4b60, 16);
            MD5_STEP(HH, ref b, c, d, a, block[10], 0xbebfbc70, 23);
            MD5_STEP(HH, ref a, b, c, d, block[13], 0x289b7ec6, 4);
            MD5_STEP(HH, ref d, a, b, c, block[0], 0xeaa127fa, 11);
            MD5_STEP(HH, ref c, d, a, b, block[3], 0xd4ef3085, 16);
            MD5_STEP(HH, ref b, c, d, a, block[6], 0x04881d05, 23);
            MD5_STEP(HH, ref a, b, c, d, block[9], 0xd9d4d039, 4);
            MD5_STEP(HH, ref d, a, b, c, block[12], 0xe6db99e5, 11);
            MD5_STEP(HH, ref c, d, a, b, block[15], 0x1fa27cf8, 16);
            MD5_STEP(HH, ref b, c, d, a, block[2], 0xc4ac5665, 23);
            MD5_STEP(II, ref a, b, c, d, block[0], 0xf4292244, 6);
            MD5_STEP(II, ref d, a, b, c, block[7], 0x432aff97, 10);
            MD5_STEP(II, ref c, d, a, b, block[14], 0xab9423a7, 15);
            MD5_STEP(II, ref b, c, d, a, block[5], 0xfc93a039, 21);
            MD5_STEP(II, ref a, b, c, d, block[12], 0x655b59c3, 6);
            MD5_STEP(II, ref d, a, b, c, block[3], 0x8f0ccc92, 10);
            MD5_STEP(II, ref c, d, a, b, block[10], 0xffeff47d, 15);
            MD5_STEP(II, ref b, c, d, a, block[1], 0x85845dd1, 21);
            MD5_STEP(II, ref a, b, c, d, block[8], 0x6fa87e4f, 6);
            MD5_STEP(II, ref d, a, b, c, block[15], 0xfe2ce6e0, 10);
            MD5_STEP(II, ref c, d, a, b, block[6], 0xa3014314, 15);
            MD5_STEP(II, ref b, c, d, a, block[13], 0x4e0811a1, 21);
            MD5_STEP(II, ref a, b, c, d, block[4], 0xf7537e82, 6);
            MD5_STEP(II, ref d, a, b, c, block[11], 0xbd3af235, 10);
            MD5_STEP(II, ref c, d, a, b, block[2], 0x2ad7d2bb, 15);
            MD5_STEP(II, ref b, c, d, a, block[9], 0xeb86d391, 21);

            ihv[0] += a;
            ihv[1] += b;
            ihv[2] += c;
            ihv[3] += d;
        }

        /// <summary>
        /// Delegate for the MD5 inner round function (F / G / H / I)
        /// </summary>
        /// <param name="b">first parameter to inner round function</param>
        /// <param name="c">second parameter to inner round function</param>
        /// <param name="d">third parameter to inner round function</param>
        /// <returns>Result of F, G, H or I</returns>
        private delegate uint RoundFunctionDelegate(uint b, uint c, uint d);

        /// <summary>
        /// Performs one step of the compression function
        /// </summary>
        /// <param name="f">The round function to use for this step</param>
        /// <param name="a">Accumulator variable A</param>
        /// <param name="b">Accumulator variable B</param>
        /// <param name="c">Accumulator variable C</param>
        /// <param name="d">Accumulator variable D</param>
        /// <param name="m">The part of the data used in this step</param>
        /// <param name="ac">Addition constant used in this step</param>
        /// <param name="rc">Rotation constant used in this step</param>
        private void MD5_STEP(RoundFunctionDelegate f, ref uint a, uint b, uint c, uint d, uint m, uint ac, int rc)
        {
            a += f(b, c, d) + m + ac;
            a = (a << rc | a >> (32 - rc)) + b;
        }

        /// <summary>
        /// Writes out an array of integers as bytes in little-endian representation
        /// </summary>
        /// <param name="sourceArray">Integer array to convert</param>
        /// <param name="targetArray">Byte array to write result into</param>
        /// <param name="targetOffset">Offset at which result is written into target array</param>
        private void dumpLittleEndianIntegers(uint[] sourceArray, byte[] targetArray, int targetOffset)
        {
            for (int i = 0; i < sourceArray.Length; i++)
            {
                dumpLittleEndianInteger(sourceArray[i], targetArray, targetOffset + i * 4);
            }
        }

        /// <summary>
        /// Writes out one integer value as bytes in little-endian representation
        /// </summary>
        /// <param name="integerValue">The integer to convert</param>
        /// <param name="targetArray">Byte array to write result into</param>
        /// <param name="targetOffset">Offset at which result is written into target array</param>
        private void dumpLittleEndianInteger(uint integerValue, byte[] targetArray, int targetOffset)
        {
            byte[] result = BitConverter.GetBytes(integerValue);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(result);
            }

            Array.Copy(result, 0, targetArray, targetOffset, 4);
        }

        /// <summary>
        /// Converts 4 bytes into an integer, assuming little-endian representation
        /// </summary>
        /// <param name="bytes">Array containing bytes to convert</param>
        /// <param name="offset">Offset at which bytes to convert are located within <c>bytes</c> array</param>
        /// <returns>The parsed integer value</returns>
        private uint toLittleEndianInteger(byte[] bytes, int offset)
        {
            byte[] bytesInProperOrder = new byte[4];
            Array.Copy(bytes, offset, bytesInProperOrder, 0, 4);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytesInProperOrder);
            }

            return BitConverter.ToUInt32(bytesInProperOrder, 0);
        }

        /// <summary>
        /// Converts 4 bytes into an integer, assuming little-endian representation
        /// </summary>
        /// <param name="bytes">Array containing bytes to convert at offset 0</param>
        /// <returns>The parsed integer value</returns>
        private uint toLittleEndianInteger(byte[] bytes)
        {
            return toLittleEndianInteger(bytes, 0);
        }

        /// <summary>
        /// Converts a byte array to an integer array, assuming little-endian representation
        /// </summary>
        /// <param name="bytes">Array containing bytes to convert</param>
        /// <param name="offset">Offset at which bytes to convert are located within <c>bytes</c> array</param>
        /// <param name="integerCount">The number of integers to convert</param>
        /// <returns>Array containing parsed integers</returns>
        private uint[] toLittleEndianIntegerArray(byte[] bytes, int offset, int integerCount)
        {
            uint[] result = new uint[integerCount];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = toLittleEndianInteger(bytes, offset + i * 4);
            }

            return result;
        }

        /// <summary>
        /// Definition of inner MD5 round function F
        /// </summary>
        /// <param name="b">First parameter for F</param>
        /// <param name="c">Second parameter for F</param>
        /// <param name="d">Third parameter for F</param>
        /// <returns>Result of F</returns>
        private uint FF(uint b, uint c, uint d)
        { return d ^ (b & (c ^ d)); }

        /// <summary>
        /// Definition of inner MD5 round function G
        /// </summary>
        /// <param name="b">First parameter for G</param>
        /// <param name="c">Second parameter for G</param>
        /// <param name="d">Third parameter for G</param>
        /// <returns>Result of G</returns>
        private uint GG(uint b, uint c, uint d)
        { return c ^ (d & (b ^ c)); }

        /// <summary>
        /// Definition of inner MD5 round function H
        /// </summary>
        /// <param name="b">First parameter for H</param>
        /// <param name="c">Second parameter for H</param>
        /// <param name="d">Third parameter for H</param>
        /// <returns>Result of H</returns>
        private uint HH(uint b, uint c, uint d)
        { return b ^ c ^ d; }

        /// <summary>
        /// Definition of inner MD5 round function I
        /// </summary>
        /// <param name="b">First parameter for I</param>
        /// <param name="c">Second parameter for I</param>
        /// <param name="d">Third parameter for I</param>
        /// <returns>Result of I</returns>
        private uint II(uint b, uint c, uint d)
        { return c ^ (b | ~d); }

        /// <summary>
        /// Left-rotates an integer by the given amount of bits
        /// </summary>
        /// <param name="x">Integer to rotate</param>
        /// <param name="n">Number of bit positions by which to rotate</param>
        /// <returns>Rotated integer</returns>
        private uint RL(uint x, int n)
        { return (x << n) | (x >> (32 - n)); }

        /// <summary>
        /// Right-rotates an integer by the given amount of bits
        /// </summary>
        /// <param name="x">Integer to rotate</param>
        /// <param name="n">Number of bit positions by which to rotate</param>
        /// <returns>Rotated integer</returns>
        private uint RR(uint x, int n)
        { return (x >> n) | (x << (32 - n)); }
    }
}
