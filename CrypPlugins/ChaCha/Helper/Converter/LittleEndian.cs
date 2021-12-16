using System;
using System.Globalization;
using System.Windows.Data;

namespace CrypTool.Plugins.ChaCha.Helper.Converter
{
    /// <summary>
    /// Reverse byte order of each consecutive 4 bytes.
    /// </summary>
    internal class LittleEndian : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            byte[] b = (byte[])value;
            return Formatter.LittleEndian(b);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}