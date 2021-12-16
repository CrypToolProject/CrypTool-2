using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace CrypTool.Plugins.ChaCha.Helper.Converter
{
    /// <summary>
    /// Extension of SequentialValueConverter.
    /// First converter is IMultiValueConverter. Next converters are IValueConverter and modify the output.
    /// </summary>
    /// <remarks>
    /// https://stackoverflow.com/a/5534535/13555687
    /// </remarks>
    public class MultiSequentialValueConverter : List<IValueConverter>, IValueConverter, IMultiValueConverter
    {
        public IMultiValueConverter MultiValueConverter { get; set; }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GroupConvert(value, this);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GroupConvertBack(value, ToArray().Reverse());
        }

        private static object GroupConvert(object value, IEnumerable<IValueConverter> converters)
        {
            return converters.Aggregate(value, (acc, conv) => { return conv.Convert(acc, typeof(object), null, null); });
        }

        private static object GroupConvertBack(object value, IEnumerable<IValueConverter> converters)
        {
            return converters.Aggregate(value, (acc, conv) => { return conv.ConvertBack(acc, typeof(object), null, null); });
        }

        #endregion IValueConverter Members

        #region IMultiValueConverter Members

        private readonly InvalidOperationException _multiValueConverterUnsetException =
            new InvalidOperationException("To use the converter as a MultiValueConverter the MultiValueConverter property needs to be set.");

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (MultiValueConverter == null)
            {
                throw _multiValueConverterUnsetException;
            }

            object firstConvertedValue = MultiValueConverter.Convert(values, targetType, parameter, culture);
            return GroupConvert(firstConvertedValue, this);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            if (MultiValueConverter == null)
            {
                throw _multiValueConverterUnsetException;
            }

            object tailConverted = GroupConvertBack(value, ToArray().Reverse());
            return MultiValueConverter.ConvertBack(tailConverted, targetTypes, parameter, culture);
        }

        #endregion IMultiValueConverter Members
    }
}