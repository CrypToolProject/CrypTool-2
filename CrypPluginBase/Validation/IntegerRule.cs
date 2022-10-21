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
using System.Globalization;
using System.Windows.Controls;

namespace CrypTool.PluginBase.Validation
{
    public class IntegerRule : ValidationRule
    {
        // public IntegerRangeAttribute IntegerRangeAttribute { get; set; }
        public int LowerEnd { get; set; }
        public int UppperEnd { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int intValue;
            try
            {
                intValue = int.Parse(value.ToString());
                if (intValue >= LowerEnd && intValue <= UppperEnd)
                {
                    return ValidationResult.ValidResult;
                }
            }
            catch (Exception)
            {
                return new ValidationResult(false, "\"" + value.ToString() + "\" is not a valid integer.");
            }

            return new ValidationResult(false,
              "Integer has to be in range from " + LowerEnd +
              " to " + UppperEnd);
        }
    }
}
