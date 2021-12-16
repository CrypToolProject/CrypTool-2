using System.Globalization;
using System.Windows.Controls;

namespace PKCS1.WpfResources
{
    internal class NumberValidationRule : ValidationRule
    {
        private int m_lowerBound = 0;
        public int LowerBound
        {
            set => m_lowerBound = value;
            get => m_lowerBound;
        }

        private int m_upperBound = 10;
        public int UpperBound
        {
            set => m_upperBound = value;
            get => m_upperBound;
        }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            string str = value as string;
            //int val = (int)value;

            if (!int.TryParse(str, NumberStyles.Integer,
          cultureInfo.NumberFormat, out int val))
            {
                return new ValidationResult(false, "Es sind nur Zahlen zulässig.");
            }

            if (val <= m_lowerBound || val >= m_upperBound)
            {
                return new ValidationResult(false, string.Format("Bitte eine Zahl zwischen {0} und {1} eingeben.", LowerBound, UpperBound));
            }

            return ValidationResult.ValidResult;
        }
    }
}
