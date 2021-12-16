using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace KeySearcher.Converter
{
    [ValueConversion(typeof(byte[]), typeof(string))]
    public class ByteArrayToUtf8StringConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string input;
            if (value is byte[])
            {
                input = Encoding.UTF8.GetString((byte[])value);
            }
            else if (value is string)
            {
                input = (string)value;
            }
            else
            {
                throw new ArgumentException();
            }

            input = new string(input.Where(c => !char.IsControl(c)).ToArray());
            return Regex.Replace(input, @"\r\n?|\n", "");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
