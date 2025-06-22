/*                              
   Copyright 2025 Nils Kopal, CrypTool Project

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

namespace CrypTool.Plugins.EllipticCurveCryptography
{
    /// <summary>
    /// Needed BigInteger extensions for ECC
    /// </summary>
    public static class BigIntegerExtensions
    {
        public static BigInteger Add(this BigInteger left, BigInteger right) => left + right;
        public static BigInteger Subtract(this BigInteger left, BigInteger right) => left - right;
        public static BigInteger Multiply(this BigInteger left, BigInteger right) => left * right;
        public static BigInteger Pow(this BigInteger value, int exponent) => BigInteger.Pow(value, exponent);
        public static BigInteger And(this BigInteger left, BigInteger right) => left & right;
        public static BigInteger ShiftRight(this BigInteger value, int shift) => value >> shift;
        public static BigInteger Negate(this BigInteger value) => -value;

        public static BigInteger Mod(this BigInteger value, BigInteger modulus)
        {
            var r = value % modulus;
            return r < 0 ? r + modulus : r;
        }

        /// <summary>
        /// Computs the mod inverse of a given value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="modulus"></param>
        /// <returns></returns>
        public static BigInteger ModInverse(this BigInteger value, BigInteger mod)
        {
            if (mod <= 0)
            {
                throw new ArgumentException("Modulus must be positive.");
            }

            BigInteger a = value.Mod(mod);
            if (a.IsZero)
            {
                throw new DivideByZeroException("Inverse of 0 does not exist.");
            }

            BigInteger r0 = mod, r1 = a;
            BigInteger t0 = 0, t1 = 1;

            while (!r1.IsZero)
            {
                BigInteger q = r0 / r1;

                (r0, r1) = (r1, r0 - q * r1);
                (t0, t1) = (t1, t0 - q * t1);
            }

            if (r0 != 1)
            {
                throw new ArgumentException("Value and modulus are not coprime.");
            }

            if (t0 < 0)
            {
                t0 += mod;
            }
            return t0;
        }
    }
}