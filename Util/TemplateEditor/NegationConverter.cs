using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TemplateEditor
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class NegationConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
                              CultureInfo culture)
        {
            if (value is bool)
            {
                return !(bool) value;
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                                  CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}