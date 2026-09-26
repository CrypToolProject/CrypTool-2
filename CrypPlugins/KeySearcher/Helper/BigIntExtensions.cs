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
using System;
using System.Numerics;

namespace KeySearcher.Helper
{
    public static class BigIntExtensions
    {

        public static double DivideAndReturnDouble(this BigInteger x, BigInteger y)
        {
            // The Double value type represents a double-precision 64-bit number with
            // values ranging from -1.79769313486232e308 to +1.79769313486232e308
            // values that do not fit into this range are returned as +/-Infinity
            if (SafeCastToDouble(x) && SafeCastToDouble(y))
            {
                return (double)x / (double)y;
            }

            // kick it old-school and figure out the sign of the result
            bool isNegativeResult = ((x.Sign < 0 && y.Sign > 0) || (x.Sign > 0 && y.Sign < 0));

            // scale the numerator to preseve the fraction part through the integer division
            BigInteger denormalized = (x * s_bnDoublePrecision) / y;
            if (denormalized.IsZero)
            {
                return isNegativeResult ? BitConverter.Int64BitsToDouble(unchecked((long)0x8000000000000000)) : 0d; // underflow to -+0
            }

            double result = 0;
            bool isDouble = false;
            int scale = DoubleMaxScale;

            while (scale > 0)
            {
                if (!isDouble)
                {
                    if (SafeCastToDouble(denormalized))
                    {
                        result = (double)denormalized;
                        isDouble = true;
                    }
                    else
                    {
                        denormalized = denormalized / 10;
                    }
                }
                result = result / 10;
                scale--;
            }

            if (!isDouble)
            {
                return isNegativeResult ? double.NegativeInfinity : double.PositiveInfinity;
            }
            else
            {
                return result;
            }

        }

        private const int DoubleMaxScale = 308;
        private static readonly BigInteger s_bnDoublePrecision = BigInteger.Pow(10, DoubleMaxScale);
        private static readonly BigInteger s_bnDoubleMaxValue = (BigInteger)double.MaxValue;
        private static readonly BigInteger s_bnDoubleMinValue = (BigInteger)double.MinValue;

        private static bool SafeCastToDouble(BigInteger value)
        {
            return s_bnDoubleMinValue <= value && value <= s_bnDoubleMaxValue;
        }

    }
}
