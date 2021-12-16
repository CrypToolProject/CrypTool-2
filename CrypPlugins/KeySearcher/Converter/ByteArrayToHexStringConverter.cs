using System;
using System.Windows.Data;

namespace KeySearcher.Converter
{
    [ValueConversion(typeof(byte[]), typeof(string))]
    public class ByteArrayToHexStringConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            byte[] bytes = (byte[])value;
            return BitConverter.ToString(bytes);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
