using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace PKCS1.WpfResources
{
    public class ErrorsToMessageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
          System.Globalization.CultureInfo culture)
        {
            StringBuilder sb = new StringBuilder();
            ReadOnlyCollection<ValidationError> errors = value as ReadOnlyCollection<ValidationError>;
            if (errors != null)
            {
                foreach (ValidationError e in errors.Where(e => e.ErrorContent != null))
                { sb.AppendLine(e.ErrorContent.ToString()); }
            }

            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter,
          System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
    }
}
