using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace KeySearcherConverter
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class FalseToVisibleConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
                              CultureInfo culture)
        {
            if (targetType != typeof(Visibility))
            {
                throw new InvalidOperationException("The target must be of Visibility");
            }

            if ((bool)value)
            {
                return Visibility.Hidden;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}