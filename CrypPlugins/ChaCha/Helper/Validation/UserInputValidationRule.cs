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