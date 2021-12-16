using System;
using System.Text;

namespace CrypTool.PluginBase.Miscellaneous
{
    public class Hex
    {
        public static string HexToString(byte[] buf)
        {
            return HexToString(buf, 0, buf.Length);
        }

        public static string HexToString(byte[] buf, int offset, int length)
        {
            if (offset + length > buf.Length)
            {
                throw new ArgumentException(string.Format("offset+length ({0}) must be < buf.Length ({1})", (offset + length), buf.Length));
            }

            if (length < 1)
            {
                throw new ArgumentOutOfRangeException("length", length, "must be >= 1");
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", offset, "must be >= 0");
            }

            StringBuilder sb = new StringBuilder(length * 2);
            for (int i = offset; i < (offset + length - 1); i++)
            {
                sb.AppendFormat("{0:X2} ", buf[i]);
            }

            sb.AppendFormat("{0:X2}", buf[offset + length - 1]);
            return sb.ToString();
        }
    }
}
