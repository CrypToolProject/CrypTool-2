using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Windows.Data;

namespace KeySearcher.Converter
{
    [ValueConversion(typeof(ObservableCollection<BigInteger>), typeof(string))]
    public class ListToStringConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollection<BigInteger> list = (ObservableCollection<BigInteger>)value;

            if (list.Count == 0)
            {
                return "-";
            }

            string convert = string.Join(", ", list.ToArray());

            //the list of currently computed blocks can get really long; thus, we cut the length to 32 characters here
            if (convert.Length > 32)
            {
                convert = convert.Substring(0, 28) + "... ";
            }
            return convert;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
