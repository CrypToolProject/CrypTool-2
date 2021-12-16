using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace PKCS1.WpfResources
{
    internal class NumberFormatRule : ValidationRule
    {
        private int m_radix = 10;
        public int Radix
        {
            set => m_radix = value;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string str = value as string;

            if (10 == m_radix)
            {
                if (!int.TryParse(str, NumberStyles.Integer, cultureInfo.NumberFormat, out int val))
                {
                    return new ValidationResult(false, "Es sind nur Zahlen zulässig!");
                }
                else
                {
                    return ValidationResult.ValidResult;
                }
            }

            if (16 == m_radix)
            {
                string hex_string = value as string;
                Match invalid_chars = Regex.Match(hex_string, "[^0-9a-fA-F]");
                bool is_valid = (invalid_chars.Success == false);

                if (is_valid == false)
                {
                    return new ValidationResult(false, "Bitte nur Zeichen von 0-9 und a-f eingeben!");
                }
                else
                {
                    return ValidationResult.ValidResult;
                }
            }

            return new ValidationResult(false, "Es ist ein Fehler aufgetreten!");
        }
    }
}
