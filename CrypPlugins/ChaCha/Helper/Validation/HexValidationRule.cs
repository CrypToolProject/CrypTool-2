using System.Globalization;
using System.Windows.Controls;

namespace CrypTool.Plugins.ChaCha.Helper.Validation
{
    internal class HexValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string inputText = (string)value;
            if (!System.Text.RegularExpressions.Regex.IsMatch(inputText, @"\A\b[0-9a-fA-F]+\b\Z"))
            {
                return new ValidationResult(false, $"Input is not a valid hex string.");
            }
            return ValidationResult.ValidResult;
        }
    }
}