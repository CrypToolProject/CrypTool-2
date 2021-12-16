using System;
using System.Globalization;
using System.Windows.Data;

namespace CrypTool.Plugins.ChaCha.Helper.Converter
{
    /// <summary>
    /// Insert a space after every 8 characters.
    /// </summary>
    internal class Chunkify : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            return Formatter.Chunkify((string)value, 8);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}