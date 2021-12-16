using System;
using System.Globalization;
using System.Windows.Data;

namespace CrypTool.Plugins.ChaCha.Helper.Converter
{
    internal class HexToBytes : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            return Formatter.Bytes((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Formatter.HexString((byte[])value);
        }
    }
}