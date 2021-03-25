using System.Globalization;
using System.Windows.Controls;

namespace CrypTool.Plugins.ChaCha.Helper.Validation
{
    internal class StringLengthValidationRule : ValidationRule
    {
        public RangeChecker ValidRange { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string inputText = (string)value;

            if (inputText.Length > ValidRange.Maximum)
            {
                return new ValidationResult(false, $"Text of length {inputText.Length} exceeds maximum length of {ValidRange.Maximum}");
            }
            return ValidationResult.ValidResult;
        }
    }
}