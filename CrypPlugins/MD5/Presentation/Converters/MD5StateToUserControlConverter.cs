using System;
using System.Windows.Data;
using CrypTool.MD5.Presentation.Helpers;
using CrypTool.MD5.Algorithm;

namespace CrypTool.MD5.Presentation.Converters
{
    class MD5StateToUserControlConverter : IValueConverter
    {
        private PresentationControlFactory controlFactory = new PresentationControlFactory();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PresentableMD5State md5State = (PresentableMD5State) value;
            return controlFactory.GetPresentationControlForState(md5State.State);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
