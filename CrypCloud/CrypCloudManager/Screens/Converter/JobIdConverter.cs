using System;
using System.Numerics;
using System.Windows.Data;

namespace CrypCloud.Manager.Screens.Converter
{
    public class JobIdConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            byte[] idbytes = ((BigInteger)value).ToByteArray();
            return BitConverter.ToString(idbytes).Replace("-", "");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}