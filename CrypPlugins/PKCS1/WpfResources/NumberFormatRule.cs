using System.Windows.Controls;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PKCS1.WpfResources
{
    class NumberFormatRule : ValidationRule
    {
        private int m_radix = 10;
        public int Radix
        {
            set { this.m_radix = (int)value; }
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var str = value as string;
            int val;

            if (10 == this.m_radix)
            {
                if (!int.TryParse(str, NumberStyles.Integer, cultureInfo.NumberFormat, out val))
                {
                    return new ValidationResult(false, "Es sind nur Zahlen zulässig!");
                }
                else
                {
                    return ValidationResult.ValidResult;
                }
            }

            if (16 == this.m_radix)
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
