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
