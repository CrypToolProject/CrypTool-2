using System;
using System.Windows.Data;

namespace CrypTool.Plugins.MD5Collider.Presentation.Converters
{
    [ValueConversion(typeof(long), typeof(string))]
    class LongToStringWithDecimalSeparatorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
        object parameter, System.Globalization.CultureInfo culture)
        {
            long? longValue = (long?)value;
            String result = String.Empty;
            if (longValue != null)
            {
                result = String.Format("{0:#,0}", longValue);
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
