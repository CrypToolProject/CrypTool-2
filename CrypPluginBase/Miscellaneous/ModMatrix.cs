/*
   Copyright 2008 - 2022 CrypTool Team

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
using System.Collections.Generic;
using System.Numerics;

namespace CrypTool.PluginBase.Miscellaneous
{
    public class ModMatrix
    {
        private readonly BigInteger[,] m;

        public ModMatrix(int dimension, BigInteger modulus)
        {
            if (dimension < 0)
            {
                throw new ArithmeticException("Matrix dimension must be >= 0.");
            }

            if (modulus < 2)
            {
                throw new ArithmeticException("Matrix modulus must be >= 2.");
            }

            Dimension = dimension;
            Modulus = modulus;
            m = new BigInteger[Dimension, Dimension];

            UnitMatrix();
        }

        public ModMatrix(ModMatrix mat)
        {
            Dimension = mat.Dimension;
            Modulus = mat.Modulus;
            m = new BigInteger[Dimension, Dimension];

            for (int y = 0; y < Dimension; y++)
            {
                for (int x = 0; x < Dimension; x++)
                {
                    m[x, y] = mat[x, y];
                }
            }
        }

        public void setElements(string values, string alphabet)
        {
            for (int y = 0; y < Dimension; y++)
            {
                for (int x = 0; x < Dimension; x++)
                {
                    m[x, y] = alphabet.IndexOf(values[x * Dimension + y]);
                }
            }
        }

        public int Dimension
        {
            get;
            private set;
        }

        public BigInteger Modulus
        {
            get;
            private set;
        }

        public BigInteger this[int x, int y]
        {
            get => m[x, y];
            set => m[x, y] = value % Modulus;
        }

        public void UnitMatrix()
        {
            for (int i = 0; i < Dimension; i++)
            {
                for (int j = 0; j < Dimension; j++)
                {
                    m[i, j] = (i == j) ? 1 : 0;
                }
            }
        }

        public BigInteger det()
        {
            if (Dimension == 0)
            {
                return 1;
            }

            BigInteger d = 0;
            int s = 1;

            for (int i = 0; i < Dimension; i++)
            {
                d += s * m[i, 0] * minor(i, 0);
                s = -s;
            }

            d = ((d % Modulus) + Modulus) % Modulus;

            return d;
        }

        public BigInteger minor(int x, int y)
        {
            return submatrix(x, y).det();
        }

        public ModMatrix submatrix(int x, int y)
        {
            ModMatrix submatrix = new ModMatrix(Dimension - 1, Modulus);

            for (int xx = 0, xi = 0; xx < Dimension; xx++)
            {
                if (xx == x)
                {
                    continue;
                }

                for (int yy = 0, yi = 0; yy < Dimension; yy++)
                {
                    if (yy == y)
                    {
                        continue;
                    }

                    submatrix[xi, yi] = m[xx, yy];
                    yi++;
                }
                xi++;
            }

            return submatrix;
        }

        public ModMatrix invert()
        {
            ModMatrix mi = new ModMatrix(Dimension, Modulus);

            BigInteger di = BigIntegerHelper.ModInverse(det(), Modulus);

            for (int y = 0; y < Dimension; y++)
            {
                for (int x = 0; x < Dimension; x++)
                {
                    int sign = 1 - 2 * ((x + y) % 2);       // sign = (-1) ^ (x+y)
                    mi[x, y] = (((sign * di * minor(y, x)) % Modulus) + Modulus) % Modulus;
                }
            }

            return mi;
        }

        public override string ToString()
        {
            List<BigInteger> lst = new List<BigInteger>();
            string s = "";
            for (int y = 0; y < Dimension; y++)
            {
                lst.Clear();
                for (int x = 0; x < Dimension; x++)
                {
                    lst.Add(this[x, y]);
                }

                s += "[" + string.Join(",", lst) + "]";
            }
            return "[" + s + "] (modulus=" + Modulus + ")";
        }

        public static implicit operator string(ModMatrix mat)
        {
            return mat.ToString();
        }

        public static ModMatrix operator *(ModMatrix matA, ModMatrix matB)
        {
            ModMatrix result = new ModMatrix(matA.Dimension, matA.Modulus);

            for (int y = 0; y < result.Dimension; y++)
            {
                for (int x = 0; x < result.Dimension; x++)
                {
                    result[x, y] = 0;
                    for (int i = 0; i < result.Dimension; i++)
                    {
                        result[x, y] += matA[i, y] * matB[x, i];
                        result[x, y] = ((result[x, y] % result.Modulus) + result.Modulus) % result.Modulus;
                    }
                }
            }

            return result;
        }

        public static BigInteger[] operator *(ModMatrix mat, BigInteger[] vector)
        {
            if (mat.Dimension != vector.Length)
            {
                return null;
            }

            BigInteger[] result = new BigInteger[mat.Dimension];

            for (int y = 0; y < mat.Dimension; y++)
            {
                result[y] = 0;
                for (int i = 0; i < mat.Dimension; i++)
                {
                    result[y] += mat[i, y] * vector[i];
                    result[y] = ((result[y] % mat.Modulus) + mat.Modulus) % mat.Modulus;
                }
            }

            return result;
        }

    }
}