using LatticeCrypto.Properties;
using System.Globalization;
using System.Windows.Controls;

namespace LatticeCrypto.Utilities
{
    public class IntegerValidationRule : ValidationRule
    {
        #region Overrides of ValidationRule

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null || !int.TryParse(value.ToString(), out int number) || number < 0)
            {
                return new ValidationResult(false, SettingsLanguages.errorInputPositiveInteger);
            }

            return new ValidationResult(true, "");
        }

        #endregion
    }
}
