using System;
using System.Globalization;
using System.Numerics;
using System.Windows.Data;

namespace CrypTool.Plugins.ChaCha.Helper.Converter
{
    internal class BigIntegerToHexWithVersion : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            object value = values[0];
            if (value is BigInteger bigInteger)
            {
                Version version = ((ChaChaSettings)values[1]).Version;
                if (version.CounterBits == 64)
                {
                    return Formatter.HexString((ulong)bigInteger);
                }
                else
                {
                    return Formatter.HexString((uint)bigInteger);
                }
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            string hex = (string)value;
            // 64-bit are 8 bytes thus if the hex string has 16 characters, it has 8 bytes and is the counter of version DJB.
            Version v = hex.Length == 16 ? Version.DJB : Version.IETF;
            return new object[] { Formatter.BigInteger((string)value), v };
        }
    }
}