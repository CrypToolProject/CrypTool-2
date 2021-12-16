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
using Primes.WpfControls.Components;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;

namespace Primes.WpfControls.Primegeneration.Function
{
    public class SecondDegreePolynom : IPolynom, ICloneable
    {
        #region constants

        protected static readonly string A = "a";
        protected static readonly string B = "b";
        protected static readonly string C = "c";

        #endregion

        #region Properties

        private System.Windows.Controls.Image m_Image;
        protected PrimesBigInteger a;
        protected PrimesBigInteger b;
        protected PrimesBigInteger c;
        protected IDictionary<string, PolynomFactor> m_list;
        protected string m_StrImageUri;

        #endregion

        public SecondDegreePolynom()
        {
            a = PrimesBigInteger.One;
            b = PrimesBigInteger.One;
            c = PrimesBigInteger.Zero;
            m_list = new Dictionary<string, PolynomFactor>
            {
                { A, new PolynomFactor(A, a) },
                { B, new PolynomFactor(B, b) },
                { C, new PolynomFactor(C, c) }
            };
            m_StrImageUri = "pack://application:,,,/Primes;Component/Resources/icons/polynomdegree2.jpg";
        }

        #region IPolynom Members

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

        public virtual ICollection<PolynomFactor> Factors => m_list.Values;

        public virtual string Name => Resources.lang.WpfControls.Generation.PrimesGeneration.polynomname_polynom;

        #endregion

        #region IExpression Members

        public virtual PrimesBigInteger Execute(PrimesBigInteger input)
        {
            PrimesBigInteger a = (m_list[A] as PolynomFactor).Value;
            PrimesBigInteger b = (m_list[B] as PolynomFactor).Value;
            PrimesBigInteger c = (m_list[C] as PolynomFactor).Value;
            return (input.Pow(2).Multiply(a).Add(b.Multiply(input))).Add(c);
        }

        public void SetParameter(string name, PrimesBigInteger value)
        {
            if (A.Equals(name))
            {
                (m_list[A] as PolynomFactor).Value = value;
            }
            else if (B.Equals(name))
            {
                (m_list[B] as PolynomFactor).Value = value;
            }
            else if (C.Equals(name))
            {
                (m_list[C] as PolynomFactor).Value = value;
            }
            else
            {
                throw new ArgumentException(string.Format("Name {0} is no valid Parameter", name));
            }
        }

        protected void SetParameter(string name, PolynomFactor value)
        {
            PolynomFactor _value = null;
            if (value != null && value.GetType() == typeof(PolynomFactor))
            {
                _value = value as PolynomFactor;
            }
            if (A.Equals(name))
            {
                m_list[A] = _value;
            }
            else if (B.Equals(name))
            {
                m_list[B] = _value;
            }
            else if (C.Equals(name))
            {
                m_list[C] = _value;
            }
            else
            {
                throw new ArgumentException(string.Format("Name {0} is no valid Parameter", name));
            }
        }

        public void Reset()
        {
            a = PrimesBigInteger.One;
            b = PrimesBigInteger.One;
            c = PrimesBigInteger.Zero;
        }

        #endregion

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            result.Append((m_list[A].Value));
            result.Append("*x² + ");
            result.Append((m_list[B].Value));
            result.Append("*x + ");
            result.Append((m_list[C].Value));

            return result.ToString();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #region ICloneable Members

        public object Clone()
        {
            SecondDegreePolynom result = new SecondDegreePolynom();
            result.SetParameter(SecondDegreePolynom.A, (m_list[A] as PolynomFactor).Value);
            result.SetParameter(SecondDegreePolynom.B, (m_list[B] as PolynomFactor).Value);
            result.SetParameter(SecondDegreePolynom.C, (m_list[C] as PolynomFactor).Value);
            return result;
        }

        #endregion
    }
}
