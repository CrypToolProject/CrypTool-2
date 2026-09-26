/*
   Copyright (C) CrypTool 2 Team

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
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
