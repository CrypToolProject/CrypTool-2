using System;
using System.Windows.Data;

namespace CrypCloud.Manager.Screens.Converter
{
    internal class PeerIdConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            byte[] idbytes = (byte[])value;
            return BitConverter.ToString(idbytes).Replace("-", "");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
