/* Original Project can be found at https://code.google.com/p/dct-watermark/
* Ported to C# to be used within CrypTool 2 by Nils Rehwald
* Thanks to cgaffa, ZXing and everyone else who worked on the original Project for making the original Java sources available publicly
* Thanks to Nils Kopal for Support and Bugfixing 
* 
* Copyright 2007 ZXing authors Licensed under the Apache License, Version 2.0 (the "License"); you may not use this
* file except in compliance with the License. You may obtain a copy of the License at
* http://www.apache.org/licenses/LICENSE-2.0 Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
* either express or implied. See the License for the specific language governing permissions and limitations under the
* License.
*/

using System;

namespace com.google.zxing.common.reedsolomon
{

    /// <summary>
    /// <para>
    /// This class contains utility methods for performing mathematical operations over the Galois Fields. Operations use a
    /// given primitive polynomial in calculations.
    /// </para>
    /// <para>
    /// Throughout this package, elements of the GF are represented as an <code>int</code> for convenience and speed (but at
    /// the cost of memory).
    /// </para>
    /// 
    /// @author Sean Owen
    /// @author David Olivier
    /// @author Ported to C# by Nils Rehwald
    /// </summary>
    public sealed class GenericGF
    {

        public static readonly GenericGF AZTEC_DATA_12 = new GenericGF(0x1069, 4096); // x^12 + x^6 + x^5 + x^3 + 1

        public static readonly GenericGF AZTEC_DATA_10 = new GenericGF(0x409, 1024); // x^10 + x^3 + 1

        public static readonly GenericGF AZTEC_DATA_6 = new GenericGF(0x43, 64); // x^6 + x + 1

        public static readonly GenericGF AZTEC_PARAM = new GenericGF(0x13, 16); // x^4 + x + 1

        public static readonly GenericGF QR_CODE_FIELD_256 = new GenericGF(0x011D, 256); // x^8 + x^4 + x^3 + x^2 + 1

        public static readonly GenericGF DATA_MATRIX_FIELD_256 = new GenericGF(0x012D, 256); // x^8 + x^5 + x^3 + x^2 + 1

        public static readonly GenericGF AZTEC_DATA_8 = DATA_MATRIX_FIELD_256;

        private const int INITIALIZATION_THRESHOLD = 0;

        /// <summary>
        /// Implements both addition and subtraction -- they are the same in GF(size).
        /// </summary>
        /// <returns> sum/difference of a and b </returns>
        internal static int addOrSubtract(int a, int b)
        {
            return a ^ b;
        }

        private int[] expTable;

        private int[] logTable;

        private GenericGFPoly zero;

        private GenericGFPoly one;

        private readonly int size;

        private readonly int primitive;

        private bool initialized = false;

        /// <summary>
        /// Create a representation of GF(size) using the given primitive polynomial.
        /// </summary>
        /// <param name="primitive"> irreducible polynomial whose coefficients are represented by the bits of an int, where the
        ///            least-significant bit represents the constant coefficient </param>
        public GenericGF(int primitive, int size)
        {
            this.primitive = primitive;
            this.size = size;

            if (size <= INITIALIZATION_THRESHOLD)
            {
                initialize();
            }
        }

        /// <returns> the monomial representing coefficient * x^degree </returns>
        internal GenericGFPoly buildMonomial(int degree, int coefficient)
        {
            checkInit();

            if (degree < 0)
            {
                throw new ArgumentException();
            }
            if (coefficient == 0)
            {
                return zero;
            }
            int[] coefficients = new int[degree + 1];
            coefficients[0] = coefficient;
            return new GenericGFPoly(this, coefficients);
        }

        private void checkInit()
        {
            if (!initialized)
            {
                initialize();
            }
        }

        /// <returns> 2 to the power of a in GF(size) </returns>

        internal int exp(int a)
        {
            checkInit();

            return expTable[a];
        }

        internal GenericGFPoly One
        {
            get
            {
                checkInit();

                return one;
            }
        }

        public int Size => size;

        internal GenericGFPoly Zero
        {
            get
            {
                checkInit();

                return zero;
            }
        }

        private void initialize()
        {
            expTable = new int[size];
            logTable = new int[size];
            int x = 1;
            for (int i = 0; i < size; i++)
            {
                expTable[i] = x;
                x <<= 1; // x = x * 2; we're assuming the generator alpha is 2
                if (x >= size)
                {
                    x ^= primitive;
                    x &= size - 1;
                }
            }
            for (int i = 0; i < size - 1; i++)
            {
                logTable[expTable[i]] = i;
            }
            zero = new GenericGFPoly(this, new int[] { 0 });
            one = new GenericGFPoly(this, new int[] { 1 });
            initialized = true;
        }

        /// <returns> multiplicative inverse of a </returns>
        internal int inverse(int a)
        {
            checkInit();

            if (a == 0)
            {
                throw new ArithmeticException();
            }
            return expTable[size - logTable[a] - 1];
        }

        /// <returns> base 2 log of a in GF(size) </returns>

        internal int log(int a)
        {
            checkInit();

            if (a == 0)
            {
                throw new ArgumentException();
            }
            return logTable[a];
        }

        /// <param name="a"> </param>
        /// <param name="b"> </param>
        /// <returns> product of a and b in GF(size) </returns>

        internal int multiply(int a, int b)
        {
            checkInit();

            if (a == 0 || b == 0)
            {
                return 0;
            }

            if (a < 0 || b < 0 || a >= size || b >= size)
            {
                a++;
            }

            int logSum = logTable[a] + logTable[b];
            return expTable[logSum % size + logSum / size];
        }

    }

}