/*
   Copyright 2008 Timo Eckhardt, University of Siegen

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

using Primes.Bignum;
using Primes.Library;
using Primes.WpfControls.Components;
using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace Primes.WpfControls.Primegeneration.Function
{
    public class SecondDegreePolynomRange : IPolynomRange
    {
        #region constants

        protected static readonly string A = "a";
        protected static readonly string B = "b";
        protected static readonly string C = "c";

        #endregion

        #region Properties

        protected Range a;
        protected Range b;
        protected Range c;
        protected IDictionary<string, RangePolynomFactor> m_list;
        protected string m_StrImageUri;

        #endregion

        #region Initialization

        public SecondDegreePolynomRange()
        {
            a = new Range(0, 1);
            b = new Range(0, 2);
            c = new Range(0, 3);
            m_list = new Dictionary<string, RangePolynomFactor>
            {
                { A, new RangePolynomFactor(A, a) },
                { B, new RangePolynomFactor(B, b) },
                { C, new RangePolynomFactor(C, c) }
            };
            m_StrImageUri = "pack://application:,,,/Primes;Component/Resources/icons/polynomdegree2.jpg";
        }

        #endregion

        #region IFormular<RangePolynomFactor> Members

        private System.Windows.Controls.Image m_Image;

        public System.Windows.Controls.Image Image
        {
            get
            {
                if (m_Image == null)
                {
                    BitmapImage bmpi = new BitmapImage(new Uri(m_StrImageUri));
                    m_Image = new System.Windows.Controls.Image
                    {
                        Source = bmpi,
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                        VerticalAlignment = System.Windows.VerticalAlignment.Top
                    };
                }

                return m_Image;
            }
        }

        public ICollection<RangePolynomFactor> Factors => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();

        public PrimesBigInteger Execute(PrimesBigInteger input)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
