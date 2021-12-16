using System;
using System.Globalization;
using System.Windows.Data;

namespace KeySearcherConverter
{
    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "-";
            }

            TimeSpan timeSpan = (TimeSpan)value;
            if (timeSpan == new TimeSpan(-1) || timeSpan.Ticks == 0)
            {
                return "~";
            }

            if (timeSpan.Days > 365)
            {
                int years = (int)(timeSpan.Days / 365.25f);
                TimeSpan ts = new TimeSpan((int)(365.25 * years), 0, 0, 0);
                TimeSpan timeWithoutYears = timeSpan.Subtract(ts);
                return string.Format("{0} years, {1} days", years, timeWithoutYears.Days);
            }

            if (timeSpan.Days > 0)
            {
                return string.Format("{0:D2} days, {1:D2} hours", timeSpan.Days, timeSpan.Hours);
            }

            int seconds = timeSpan.Seconds;
            if (seconds == 0 && timeSpan.Milliseconds > 0)
            {
                seconds = 1;
            }

            return string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, seconds);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
