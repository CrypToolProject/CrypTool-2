using System;
using System.Globalization;
using System.Windows.Data;

namespace CrypTool.Plugins.ChaCha.Helper.Converter
{
    internal class Add : IValueConverter
    {
        public int Value { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value + Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}