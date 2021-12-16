using System;
using System.Collections.Generic;
using System.Windows.Data;

namespace CrypTool.Plugins.ChaCha.Helper.Converter
{
    /// <summary>
    /// IValueConverter which combines multiple IValueConverters.
    /// This enables piping of converters.
    /// For example, we can now do this:
    ///   input | encode ascii | split 4 bytes | reverse byte order
    /// </summary>
    /// <remarks>
    /// https://web.archive.org/web/20130622171857/http://www.garethevans.com/linking-multiple-value-converters-in-wpf-and-silverlight
    /// </remarks>
    internal class SequentialValueConverter : List<IValueConverter>, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object returnValue = value;

            foreach (IValueConverter converter in this)
            {
                returnValue = converter.Convert(returnValue, targetType, parameter, culture);
            }

            return returnValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}