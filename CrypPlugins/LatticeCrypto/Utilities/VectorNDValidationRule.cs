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
using LatticeCrypto.Models;
using LatticeCrypto.Properties;
using System;
using System.Globalization;
using System.Windows.Controls;

namespace LatticeCrypto.Utilities
{
    public class VectorNDValidationRule : ValidationRule
    {
        #region Overrides of ValidationRule

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                VectorND test = Util.ConvertStringToLatticeND((string)value).Vectors[0];
                return new ValidationResult(true, "");
            }
            catch (Exception)
            {
                return new ValidationResult(false, SettingsLanguages.errorInputPositiveInteger);
            }
        }

        #endregion
    }
}
