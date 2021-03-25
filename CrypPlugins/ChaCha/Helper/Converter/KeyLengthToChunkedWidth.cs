using System;
using System.Globalization;
using System.Windows.Data;

namespace CrypTool.Plugins.ChaCha.Helper.Converter
{
    /// <summary>
    /// Converter which maps the key length (128-bit or 256-bit) into a RichTextBox Width of a chunked value.
    /// Used for the chunk values in the state matrix initialization. The RichTextBoxes need a specified width because they are in viewboxes
    /// and the width depends on the key size.
    /// </summary>
    internal class KeyLengthToChunkedWidth : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int keyBytesLength = (int)value;
            if (keyBytesLength == 32)
            {
                // 256-bit key
                return KeyLengthToWidth.KEY_256_WIDTH + 95;
            }
            else
            {
                // 128-bit key
                return KeyLengthToWidth.KEY_128_WIDTH + 48;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}