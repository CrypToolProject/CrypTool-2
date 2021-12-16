using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CrypTool.Plugins.MD5.Presentation.Converters
{
    public class HighlightIndexToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null || !(value is int) || (!(parameter is string)))
            {
                return null;
            }

            int selectedIndex = (int)value;
            int referenceIndex = System.Convert.ToInt32((string)parameter);

            return selectedIndex == referenceIndex ? Brushes.Yellow : Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
