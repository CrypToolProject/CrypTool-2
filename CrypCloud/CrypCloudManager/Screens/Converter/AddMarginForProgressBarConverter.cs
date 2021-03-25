using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace CrypCloud.Manager.Screens.Converter
{
    public class AddMarginForProgressBarConverter : MarkupExtension, IValueConverter
    {
        public AddMarginForProgressBarConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int val = System.Convert.ToInt32(value);
            return val - 50;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return 0;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
