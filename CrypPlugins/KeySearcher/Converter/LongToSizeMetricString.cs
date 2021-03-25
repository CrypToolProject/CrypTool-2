using System;
using System.Globalization;
using System.Windows.Data;

namespace KeySearcherConverter
{
    class LongToSizeMetricString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bytes = (long) value;
            const long scale = 1024;
            var orders = new[] { "EiB", "PiB", "TiB", "GiB", "MiB", "KiB", "Bytes" };
            var max = (long) Math.Pow(scale, (orders.Length - 1));

            foreach (var order in orders)
            {
                if (bytes > max)
                    return string.Format("{0:##.##} {1}", Decimal.Divide(bytes, max), order);
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
