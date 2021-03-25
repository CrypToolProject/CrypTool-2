using System.Windows.Controls;
using System.Globalization;

namespace PKCS1.WpfResources
{
    class NumberValidationRule : ValidationRule
    {
        private int m_lowerBound = 0;
        public int LowerBound
        {
            set { this.m_lowerBound = (int)value;  }
            get { return this.m_lowerBound; }
        }

        private int m_upperBound = 10;
        public int UpperBound
        {
            set { this.m_upperBound = (int)value; }
            get { return this.m_upperBound; }
        }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            var str = value as string;
            //int val = (int)value;
            int val;

            if (!int.TryParse(str, NumberStyles.Integer,
          cultureInfo.NumberFormat, out val))
            {
                return new ValidationResult(false, "Es sind nur Zahlen zulässig.");
            }

            if( val <= m_lowerBound || val >= m_upperBound)
            {
                return new ValidationResult(false, string.Format("Bitte eine Zahl zwischen {0} und {1} eingeben.", LowerBound, UpperBound));
            }

            return ValidationResult.ValidResult;
        }
    }
}
