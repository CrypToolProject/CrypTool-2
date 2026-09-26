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
using System.Windows.Controls;

namespace CrypTool.Plugins.ChaCha.Helper.Validation
{
    internal class UserInputValidationRule : ValidationRule
    {
        public UserInputValidationRule(int max) : base()
        {
            Min = 0;
            Max = max;
        }

        public UserInputValidationRule(int min, int max) : base()
        {
            Min = min;
            Max = max;
        }

        private int Min { get; set; }

        private int Max { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int input;
            try
            {
                input = int.Parse((string)value);
            }
            catch (Exception e)
            {
                return new ValidationResult(false, $"Illegal characters or {e.Message}");
            }

            if ((input < Min) || (input > Max))
            {
                return new ValidationResult(false,
                    $"Please enter a value between {Min} and {Max}.");
            }
            return ValidationResult.ValidResult;
        }
    }
}