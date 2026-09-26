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