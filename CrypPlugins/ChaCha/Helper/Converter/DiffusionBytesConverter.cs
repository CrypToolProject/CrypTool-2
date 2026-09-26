/*
   Copyright (C) CrypTool 2 Team

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
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