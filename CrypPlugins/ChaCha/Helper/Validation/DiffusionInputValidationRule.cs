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
using System.Globalization;
using System.Windows.Controls;

namespace CrypTool.Plugins.ChaCha.Helper.Validation
{
    internal class DiffusionInputValidationRule : ValidationRule
    {
        private int MaxKeyBytesLength { get; set; }
        private int MaxHexKeyStringLength { get; set; }

        public DiffusionInputValidationRule(int maxKeyBytesLength) : base()
        {
            MaxKeyBytesLength = maxKeyBytesLength;
            MaxHexKeyStringLength = maxKeyBytesLength * 2;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string inputText = (string)value;
            if (!System.Text.RegularExpressions.Regex.IsMatch(inputText, @"\A\b[0-9a-fA-F]+\b\Z"))
            {
                return new ValidationResult(false, $"Input is not a valid hex string.");
            }

            if (inputText.Length > MaxHexKeyStringLength)
            {
                return new ValidationResult(false, $"Input must be {MaxKeyBytesLength * 8}-bit. Is {inputText.Length * 8 / 2}-bit.");
            }
            return ValidationResult.ValidResult;
        }
    }
}