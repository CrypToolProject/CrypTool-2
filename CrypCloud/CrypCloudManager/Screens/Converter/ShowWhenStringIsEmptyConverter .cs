using System;
using System.Windows;
using System.Windows.Data;

namespace CrypCloud.Manager.Screens.Converter
{
    public class ShowWhenStringIsEmptyConverter : System.Windows.Markup.MarkupExtension, IValueConverter
    {
        public ShowWhenStringIsEmptyConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.IsNullOrEmpty(value as string) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
