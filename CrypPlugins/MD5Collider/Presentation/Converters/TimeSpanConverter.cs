using System;
using System.Windows.Data;

namespace CrypTool.Plugins.MD5Collider.Presentation.Converters
{
    [ValueConversion(typeof(TimeSpan), typeof(string))]
    internal class TimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
        object parameter, System.Globalization.CultureInfo culture)
        {
            TimeSpan timeSpan = (TimeSpan)value;
            string result = string.Empty;
            if (timeSpan != null)
            {
                result = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType,
        object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

    }
}
