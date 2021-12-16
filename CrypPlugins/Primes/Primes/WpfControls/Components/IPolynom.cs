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
using System;
using System.Windows.Controls;

namespace Primes.WpfControls.Components
{
    public interface IFormular<T>
    {
        Image Image { get; }
        System.Collections.Generic.ICollection<T> Factors { get; }
        string Name { get; }
    }

    public interface IPolynom : Primes.WpfControls.Components.IExpression, IFormular<PolynomFactor>
    {
    }

    public interface IPolynomRange : IFormular<RangePolynomFactor>
    {
        PrimesBigInteger Execute(PrimesBigInteger input);
    }

    public class PolynomFactor
    {
        public PolynomFactor(string name, PrimesBigInteger value)
        {
            Name = name;
            Value = value;
        }

        public PolynomFactor(string name, PrimesBigInteger value, bool isReadOnly)
        {
            Name = name;
            Value = value;
            Readonly = isReadOnly;
        }

        #region Properties

        private string m_Name;

        public string Name
        {
            get => m_Name;
            set => m_Name = value;
        }

        private PrimesBigInteger m_Value;

        public PrimesBigInteger Value
        {
            get => m_Value;
            set => m_Value = value;
        }

        private bool m_Readonly;

        public bool Readonly
        {
            get => m_Readonly;
            set => m_Readonly = value;
        }

        #endregion
    }

    public class RangePolynomFactor
    {
        public RangePolynomFactor(string name, PrimesBigInteger from, PrimesBigInteger to)
            : this(name, new RangeX(from, to))
        {
        }

        public RangePolynomFactor(string name, Range range)
        {
            if (range == null)
            {
                throw new ArgumentNullException("range");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            m_Range = range;
            Name = name;
        }

        #region IPolynomFactor Members

        private string m_Name;

        public string Name
        {
            get => m_Name;
            set => m_Name = value;
        }

        #endregion

        private readonly Range m_Range;

        public PrimesBigInteger From => m_Range.From;

        public PrimesBigInteger To => m_Range.To;
    }
}
