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
using System.Collections.Generic;
using System.Windows.Data;

namespace CrypTool.Plugins.ChaCha.Helper.Converter
{
    /// <summary>
    /// IValueConverter which combines multiple IValueConverters.
    /// This enables piping of converters.
    /// For example, we can now do this:
    ///   input | encode ascii | split 4 bytes | reverse byte order
    /// </summary>
    /// <remarks>
    /// https://web.archive.org/web/20130622171857/http://www.garethevans.com/linking-multiple-value-converters-in-wpf-and-silverlight
    /// </remarks>
    internal class SequentialValueConverter : List<IValueConverter>, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            object returnValue = value;

            foreach (IValueConverter converter in this)
            {
                returnValue = converter.Convert(returnValue, targetType, parameter, culture);
            }

            return returnValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}