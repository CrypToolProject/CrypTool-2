using System;
using System.Globalization;
using System.Windows.Data;

namespace CrypCloud.Manager.Screens.Converter
{
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "-";
            }

            DateTime dateTime = (DateTime)value;

            if (dateTime.Ticks == 0)
            {
                return "~";
            }

            return dateTime.ToLocalTime().ToString("g");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
