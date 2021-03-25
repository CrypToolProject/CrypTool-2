using System.Globalization;
using System.Windows.Controls;
using LatticeCrypto.Properties;

namespace LatticeCrypto.Utilities
{
    public class IntegerValidationRule : ValidationRule
    {
        #region Overrides of ValidationRule

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int number;
            if (value == null || !int.TryParse(value.ToString(), out number) || number < 0 )
                return new ValidationResult(false, SettingsLanguages.errorInputPositiveInteger);
            return new ValidationResult(true, "");
        }

        #endregion
    }
}
