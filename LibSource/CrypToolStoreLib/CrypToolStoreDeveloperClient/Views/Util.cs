/*
   Copyright 2020 Nils Kopal <Nils.Kopal<AT>cryptool.org>

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

namespace CrypToolStoreDeveloperClient.Views
{
    [ValueConversion(typeof(string), typeof(string))]
    public class StringTrimmer : IValueConverter
    {
        private const int MAX_STRING_DISPLAY_LENGTH = 50;

        /// <summary>
        /// Removes linebreaks and cuts the string to a max length and also adds ...
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string theString = (string)value;

            bool appendFullstops = false;
            if (theString.Length > MAX_STRING_DISPLAY_LENGTH)
            {
                appendFullstops = true;
            }

            theString = theString.Replace("\r", " ");
            theString = theString.Replace("\n", " ");
            theString = theString.Replace("  ", " ");
            theString = theString.Substring(0, Math.Min(theString.Length, MAX_STRING_DISPLAY_LENGTH));

            if (appendFullstops)
            {
                theString = theString + "...";
            }

            return theString;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
        System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
