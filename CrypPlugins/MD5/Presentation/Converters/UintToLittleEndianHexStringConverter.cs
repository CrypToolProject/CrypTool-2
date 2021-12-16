using System;
using System.Windows.Data;

namespace CrypTool.MD5.Presentation.Converters
{
    internal class UintToLittleEndianHexStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            uint intValue = (uint)value;
            return BitConverter.ToString(BitConverter.GetBytes(intValue)).Replace('-', ' ');
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
