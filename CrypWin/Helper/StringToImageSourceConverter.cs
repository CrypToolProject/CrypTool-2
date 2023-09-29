/*
   Copyright 2008 - 2022 CrypTool Team

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
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace CrypWin.Helper
{
    public class StringToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // Value is of type enum (LogLevel)
            string strValue = value.ToString();
            if (strValue != null)
            {
                return new BitmapImage(new Uri(@"/CrypWin;Component/images/" + strValue + ".png", UriKind.Relative));
            }
            throw new InvalidOperationException("Unexpected value in converter");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new InvalidOperationException("The method or operation is not implemented.");
        }
    }
}
