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
using CrypTool.MD5.Algorithm;
using CrypTool.MD5.Presentation.Helpers;
using System;
using System.Windows.Data;

namespace CrypTool.MD5.Presentation.Converters
{
    internal class MD5StateToUserControlConverter : IValueConverter
    {
        private readonly PresentationControlFactory controlFactory = new PresentationControlFactory();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PresentableMD5State md5State = (PresentableMD5State)value;
            return controlFactory.GetPresentationControlForState(md5State.State);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
