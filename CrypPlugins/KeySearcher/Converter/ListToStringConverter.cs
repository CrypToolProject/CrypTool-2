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
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Windows.Data;

namespace KeySearcher.Converter
{
    [ValueConversion(typeof(ObservableCollection<BigInteger>), typeof(string))]
    public class ListToStringConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollection<BigInteger> list = (ObservableCollection<BigInteger>)value;

            if (list.Count == 0)
            {
                return "-";
            }

            string convert = string.Join(", ", list.ToArray());

            //the list of currently computed blocks can get really long; thus, we cut the length to 32 characters here
            if (convert.Length > 32)
            {
                convert = convert.Substring(0, 28) + "... ";
            }
            return convert;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
