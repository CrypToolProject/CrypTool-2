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
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace CrypTool.PluginBase.Validation
{
    public class RegExRule : ValidationRule
    {
        public string RegExValue { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                string Value = (string)value;
                Regex regEx = new Regex(RegExValue);

                bool check = regEx.IsMatch(Value);
                if (check)
                {
                    return ValidationResult.ValidResult;
                }
            }
            catch (Exception ex)
            {
                return new ValidationResult(false, "Validation Error:" + ex.Message);
            }
            return new ValidationResult(false, "\"" + value.ToString() + "\" does not match criterias.");
        }
    }
}
