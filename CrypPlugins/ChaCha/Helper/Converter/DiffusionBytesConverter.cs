using System;
using System.Globalization;
using System.Windows.Data;

namespace CrypTool.Plugins.ChaCha.Helper.Converter
{
    internal class DiffusionBytesConverter : IValueConverter
    {
        /// <summary>
        /// Specifies how long the hex string should be. If hex string is too small, zero-left-padding is applied.
        /// </summary>
        private int Padding { get; set; }

        public DiffusionBytesConverter(int bytes) : base()
        {
            Padding = bytes * 2;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string hex = Formatter.HexString((byte[])value);
            return hex.PadLeft(Padding).Replace(" ", "0");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string inputText = (string)value;
            inputText = inputText.PadLeft(Padding).Replace(" ", "0");
            return Formatter.Bytes(inputText);
        }
    }
}