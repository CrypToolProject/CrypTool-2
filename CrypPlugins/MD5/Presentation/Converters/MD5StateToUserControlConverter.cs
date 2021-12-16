using CrypTool.MD5.Algorithm;
using CrypTool.MD5.Presentation.Helpers;
using System;
using System.Windows.Data;

namespace CrypTool.MD5.Presentation.Converters
{
    internal class MD5StateToUserControlConverter : IValueConverter
    {
        private readonly PresentationControlFactory controlFactory = new PresentationControlFactory();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PresentableMD5State md5State = (PresentableMD5State)value;
            return controlFactory.GetPresentationControlForState(md5State.State);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
