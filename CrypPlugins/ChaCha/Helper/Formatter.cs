using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CrypTool.Plugins.ChaCha.Helper
{
    internal static class Formatter
    {
        /// <summary>
        /// Return hex representation of bytes.
        /// </summary>
        public static string HexString(byte[] bytes, int offset, int length)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = offset; i < offset + length; ++i)
            {
                sb.Append(bytes[i].ToString("X2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Return hex representation of bytes.
        /// </summary>
        public static string HexString(byte[] bytes)
        {
            return HexString(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Return hex representation of UInt64.
        /// </summary>
        public static string HexString(ulong u)
        {
            return HexString(ByteUtil.GetBytesBE(u));
        }

        /// <summary>
        /// Return hex representation of UInt32.
        /// </summary>
        public static string HexString(uint u)
        {
            return HexString(ByteUtil.GetBytesBE(u));
        }

        /// <summary>
        /// Return bytes of hex string.
        /// </summary>
        public static byte[] Bytes(string hex)
        {
            // Left-pad hex string with zero such that is has an even amount of characters.
            if (hex.Length % 2 == 1)
            {
                hex = $"0{hex}";
            }
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }

        /// <summary>
        /// Return BigInteger of hex string.
        /// </summary>
        public static System.Numerics.BigInteger BigInteger(string hex)
        {
            string unsignedHex = $"0{hex}";
            return System.Numerics.BigInteger.Parse(unsignedHex, NumberStyles.HexNumber);
        }

        /// <summary>
        /// Insert a space after every n characters.
        /// </summary>
        public static string Chunkify(string s, int n)
        {
            return Regex.Replace(s, $".{{{n}}}", "$0 ");
        }

        /// <summary>
        /// Reverse order of every 4 bytes.
        /// </summary>
        public static byte[] LittleEndian(byte[] b)
        {
            if (b.Length % 4 != 0)
            {
                throw new ArgumentException("Byte length must be divisible by four.");
            }

            byte[] le = new byte[b.Length];
            for (int i = 0; i < b.Length; i += 4)
            {
                le[i] = b[i + 3];
                le[i + 1] = b[i + 2];
                le[i + 2] = b[i + 1];
                le[i + 3] = b[i];
            }
            return le;
        }

        /// <summary>
        /// Reverse order of every 4 bytes in UInt64.
        /// </summary>
        public static byte[] LittleEndian(ulong u)
        {
            return LittleEndian(ByteUtil.GetBytesBE(u));
        }

        /// <summary>
        /// Reverse order of every 4 bytes in UInt32.
        /// </summary>
        public static byte[] LittleEndian(uint u)
        {
            return LittleEndian(ByteUtil.GetBytesBE(u));
        }

        /// <summary>
        /// Reverse byte order of array
        /// </summary>
        public static byte[] ReverseBytes(byte[] b)
        {
            return b.Reverse().ToArray();
        }

        /// <summary>
        /// Reverse byte order of UInt32.
        /// </summary>
        public static byte[] ReverseBytes(uint u)
        {
            return ReverseBytes(ByteUtil.GetBytesBE(u));
        }

        /// <summary>
        /// Reverse byte order of UInt64.
        /// </summary>
        public static byte[] ReverseBytes(ulong u)
        {
            return ReverseBytes(ByteUtil.GetBytesBE(u));
        }
    }
}