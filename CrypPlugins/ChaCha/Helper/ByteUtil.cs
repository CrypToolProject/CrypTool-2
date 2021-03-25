using System;
using System.Linq;

namespace CrypTool.Plugins.ChaCha.Helper
{
    /// <summary>
    /// This class handles all byte operations.
    ///
    /// It makes sure that the byte order returned by the BitConverter class is consistent across
    /// all system architectures.
    ///
    /// For example, on a system with little-endian architecture, the uint32 0x12345678 would be
    /// returned in this order: [0x78, 0x56, 0x34, 0x12]
    ///
    /// We want to always expect byte arrays in big-endian order, thus byte order of the uin32
    /// 0x12345678 should be [0x12, 0x34, 0x56, x78]
    ///
    /// This is solved by reversing byte arrays when using `BitConverter.GetBytes`
    /// on little-endian architectures.
    /// </summary>
    public static class ByteUtil
    {
        /// <summary>
        /// Reverse the byte array if running system has little-endian architecture.
        /// </summary>
        /// <param name="bytes">The byte array we want to conditionally reverse.</param>
        public static void ReverseBytesOnLittleEndianArch(ref byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                bytes = bytes.Reverse().ToArray();
            }
        }

        /// <summary>
        /// Reverse the byte array if running system has big-endian architecture.
        /// </summary>
        /// <param name="bytes">The byte array we want to conditionally reverse.</param>
        public static void ReverseBytesOnBigEndianArch(ref byte[] bytes)
        {
            if (!BitConverter.IsLittleEndian)
            {
                bytes = bytes.Reverse().ToArray();
            }
        }

        /// <summary>
        /// Return bytes in big-endian, independent of system architecture.
        /// </summary>
        public static byte[] GetBytesBE(ulong x)
        {
            byte[] b = BitConverter.GetBytes(x);
            ReverseBytesOnLittleEndianArch(ref b);
            return b;
        }

        /// <summary>
        /// Return bytes in little-endian, independent of system architecture.
        /// </summary>
        public static byte[] GetBytesLE(ulong x)
        {
            byte[] bytes = BitConverter.GetBytes(x);
            ReverseBytesOnBigEndianArch(ref bytes);
            return bytes;
        }

        /// <summary>
        /// Return bytes in big-endian, independent of system architecture.
        /// </summary>
        public static byte[] GetBytesBE(uint x)
        {
            byte[] bytes = BitConverter.GetBytes(x);
            ReverseBytesOnLittleEndianArch(ref bytes);
            return bytes;
        }

        /// <summary>
        /// Return bytes in little-endian, independent of system architecture.
        /// </summary>
        public static byte[] GetBytesLE(uint x)
        {
            byte[] bytes = BitConverter.GetBytes(x);
            ReverseBytesOnBigEndianArch(ref bytes);
            return bytes;
        }

        /// <summary>
        /// Assuming byte input is in big-endian, return a UInt32 with little-endian byte order, starting at the offset, independent of system architecture.
        /// </summary>
        /// <param name="x">Byte array of which we want to return the UInt32 in reversed byte order.</param>
        /// <param name="offset">Array offset for first byte of UInt32.</param>
        public static uint ToUInt32LE(byte[] x, int offset)
        {
            byte b1 = x[offset];
            byte b2 = x[offset + 1];
            byte b3 = x[offset + 2];
            byte b4 = x[offset + 3];

            return (uint)(b4 << 24 | b3 << 16 | b2 << 8 | b1);
        }

        /// <summary>
        /// Reverse byte order of UInt32.
        /// </summary>
        public static uint ToUInt32LE(uint x)
        {
            byte[] b = ByteUtil.GetBytesBE(x);
            return (uint)(b[3] << 24 | b[2] << 16 | b[1] << 8 | b[0]);
        }

        /// <summary>
        /// Assume byte input is in big-endian, return a UInt32 Array by combining each consecutive four bytes into a UInt32.
        /// </summary>
        public static uint[] ToUInt32Array(byte[] b)
        {
            if (b.Length % 4 != 0)
            {
                throw new ArgumentOutOfRangeException("b", b.Length, "Input bytes length must be multiple of four.");
            }

            uint[] u = new uint[b.Length / 4];
            for (int i = 0; i < u.Length; ++i)
            {
                // most significant byte is b[i] because byte array is in big-endian thus we shift it the most to the left.
                u[i] = (uint)(b[i] << 24 | b[i + 1] << 16 | b[i + 2] << 8 | b[i + 3]);
            }
            return u;
        }

        /// <summary>
        /// Return a byte array of the UInt32 array by creating a byte array in big-endian notation for each UInt32.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public static byte[] ToByteArray(uint[] u)
        {
            byte[] b = new byte[u.Length * 4];
            for (int i = 0; i < u.Length; ++i)
            {
                byte[] uBytes = ByteUtil.GetBytesBE(u[i]);
                // most significant byte comes first
                b[i * 4] = uBytes[0];
                b[i * 4 + 1] = uBytes[1];
                b[i * 4 + 2] = uBytes[2];
                b[i * 4 + 3] = uBytes[3];
            }
            return b;
        }

        /// <summary>
        /// Circular left shift.
        /// </summary>
        /// <param name="x">The value we want to circular shift</param>
        /// <param name="shift">amount of shifts</param>
        public static uint RotateLeft(uint x, int shift)
        {
            return (x << shift) | (x >> -shift);
        }

        /// <summary>
        /// Bytewise XOR of both arrays.
        /// </summary>
        /// <param name="a">First byte array</param>
        /// <param name="b">Second byte array</param>
        /// <param name="count">How many bytes should be XOR'ed</param>
        public static byte[] XOR(byte[] a, byte[] b, int count)
        {
            // output buffer
            byte[] c = new byte[count];

            for (int i = 0; i < count; ++i)
            {
                c[i] = (byte)(a[i] ^ b[i]);
            }
            return c;
        }

        /// <summary>
        /// Bytewise XOR of both arrays. They must be of same size.
        /// </summary>
        public static byte[] XOR(byte[] a, byte[] b)
        {
            a = LeftPad(a, b.Length);
            b = LeftPad(b, a.Length);
            return XOR(a, b, a.Length);
        }

        /// <summary>
        /// Left pad the input bytes array with zeroes. If the input array is already bigger, nothing happens.
        /// </summary>
        /// <param name="bytes">The byte array to left-pad with zeroes.</param>
        /// <param name="size">How long the byte array should be at least in the end.</param>
        /// <returns></returns>
        public static byte[] LeftPad(byte[] b, int size)
        {
            if (b.Length >= size)
            {
                return b;
            }
            byte[] padded = new byte[size];
            int leftPadAmount = size - b.Length;
            for (int i = 0; i < leftPadAmount; ++i)
            {
                padded[i] = 0x00;
            }
            b.CopyTo(padded, leftPadAmount);
            return padded;
        }
    }
}