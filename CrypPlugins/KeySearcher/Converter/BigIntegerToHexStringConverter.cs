using System;
using System.Numerics;
using System.Windows.Data;

namespace KeySearcher.Converter
{
    [ValueConversion(typeof(BigInteger), typeof(string))]
    public class BigIntegerToHexStringConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            BigInteger bigint = (BigInteger)value;
            return bigint.ToString("X");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
