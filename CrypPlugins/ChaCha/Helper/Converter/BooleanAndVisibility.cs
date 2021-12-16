using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace CrypTool.Plugins.ChaCha.Helper.Converter
{
    internal class BooleanAndVisibility : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Any(v => v == DependencyProperty.UnsetValue))
            {
                return DependencyProperty.UnsetValue;
            }

            return values.All(v => (bool)v == true) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}