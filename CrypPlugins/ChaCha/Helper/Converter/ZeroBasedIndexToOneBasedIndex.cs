using System;
using System.Globalization;
using System.Windows.Data;

namespace CrypTool.Plugins.ChaCha.Helper.Converter
{
    internal class ZeroBasedIndexToOneBasedIndex : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // if value is null, we are at "no index", thus return null to show empty string in frontend
            if (value == null)
            {
                return null;
            }

            return (int)value + 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return int.Parse((string)value) - 1;
        }
    }
}