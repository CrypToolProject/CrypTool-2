using System;
using System.Globalization;
using System.Windows.Data;

namespace CrypTool.PlayfairAnalyzer.Converters
{
    public class GridViewColumnVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value as bool? ?? false)
            {
                return parameter;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
