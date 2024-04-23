/*
   Copyright Mikail Sarier 2023, University of Mannheim

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
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CrypTool.Plugins.Anonymity
{
    public class EquivalenceClassColor : IValueConverter
    {
        private readonly List<Brush> colors = new List<Brush>
        {
            Brushes.LightBlue,
            Brushes.LightCoral,
        };

        /// <summary>
        ///  convert groupID to color
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        ///  <param name="parameter"></param>
        ///   <param name="culture"></param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value == null || (int)value == -1)
            {
                return Brushes.Transparent;
            }

            // get the index of the color in the listth
            int groupID = (int)value;
            return colors[groupID % colors.Count];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}