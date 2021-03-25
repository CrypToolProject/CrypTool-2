using System;
using System.Globalization;
using System.Windows.Data;

namespace CrypTool.Plugins.ChaCha.Helper.Converter
{
    /// <summary>
    /// Converter which maps the key length (128-bit or 256-bit) into a RichTextBox Width.
    /// Used for state matrix initialization. The RichTextBoxes need a specified width because they are in viewboxes
    /// and the width depends on the key size.
    /// </summary>
    internal class KeyLengthToWidth : IValueConverter
    {
        public static int KEY_256_WIDTH = 780;
        public static int KEY_128_WIDTH = 395;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int keyBytesLength = (int)value;
            if (keyBytesLength == 32)
            {
                // 256-bit key
                return KEY_256_WIDTH;
            }
            else
            {
                // 128-bit key
                return KEY_128_WIDTH;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}