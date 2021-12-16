using System;
using System.Windows.Data;

namespace CrypTool.CrypWin.Helper
{
    internal class PanelWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double x = (double)value;
            return x - 20;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
