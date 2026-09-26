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
using System.Numerics;
using System.Windows.Data;

namespace CrypTool.Plugins.ChaCha.Helper.Converter
{
    internal class BigIntegerToHexWithVersion : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            object value = values[0];
            if (value is BigInteger bigInteger)
            {
                Version version = ((ChaChaSettings)values[1]).Version;
                if (version.CounterBits == 64)
                {
                    return Formatter.HexString((ulong)bigInteger);
                }
                else
                {
                    return Formatter.HexString((uint)bigInteger);
                }
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            string hex = (string)value;
            // 64-bit are 8 bytes thus if the hex string has 16 characters, it has 8 bytes and is the counter of version DJB.
            Version v = hex.Length == 16 ? Version.DJB : Version.IETF;
            return new object[] { Formatter.BigInteger((string)value), v };
        }
    }
}