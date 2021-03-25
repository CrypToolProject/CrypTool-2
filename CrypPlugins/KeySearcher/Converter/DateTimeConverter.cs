using System;
using System.Globalization;
using System.Threading;
using System.Windows.Data;

namespace KeySearcherConverter
{
    public class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "-";
            }

            var dateTime = (DateTime)value;

            if (dateTime.Ticks == 0)
            {
                return "~";
            }

            return dateTime.ToString("g", Thread.CurrentThread.CurrentCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
