using System;
using System.Globalization;
using System.Windows.Data;

namespace KeySearcherConverter
{
    internal class LongToSizeMetricString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            long bytes = (long)value;
            const long scale = 1024;
            string[] orders = new[] { "EiB", "PiB", "TiB", "GiB", "MiB", "KiB", "Bytes" };
            long max = (long)Math.Pow(scale, (orders.Length - 1));

            foreach (string order in orders)
            {
                if (bytes > max)
                {
                    return string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), order);
                }

                max /= scale;
            }

            return "0 Bytes";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
