using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace CrypTool.Plugins.ChaCha.Helper.Converter
{
    internal class BytesToHex : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            if (value is List<byte> valueList)
            {
                return Formatter.HexString(valueList.ToArray());
            }
            return Formatter.HexString((byte[])value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string inputText = (string)value;
            return Formatter.Bytes(inputText);
        }
    }
}