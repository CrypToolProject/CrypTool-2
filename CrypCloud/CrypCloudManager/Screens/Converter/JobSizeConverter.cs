using System;
using System.Windows.Data;

namespace CrypCloud.Manager.Screens.Converter
{
    internal class JobSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            long size = (long)value;
            return CalculateSizeString(size);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the given siza as long to a string with
        /// bytes, KiB, or MiB
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        private string CalculateSizeString(long size)
        {
            if (size < 1024)
            {
                return size + " bytes";
            }
            else if (size < 1024 * 1024)
            {
                return Math.Round(size / 1024.0, 2) + " KiB";
            }
            else
            {
                return Math.Round(size / 1024.0 * 1024.0, 2) + " MiB";
            }
        }
    }
}
