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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace KeySearcher.Converter
{
    [ValueConversion(typeof(byte[]), typeof(string))]
    public class ByteArrayToUtf8StringConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string input;
            if (value is byte[])
            {
                input = Encoding.UTF8.GetString((byte[])value);
            }
            else if (value is string)
            {
                input = (string)value;
            }
            else
            {
                throw new ArgumentException();
            }

            input = new string(input.Where(c => !char.IsControl(c)).ToArray());
            return Regex.Replace(input, @"\r\n?|\n", "");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
