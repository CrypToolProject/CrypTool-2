/*
    Matrix class in C#
    Written by Ivan Kuckir (ivan.kuckir@gmail.com, http://blog.ivank.net)
    Faculty of Mathematics and Physics
    Charles University in Prague
    (C) 2010
    - updated on 14.6.2012 - parsing improved. Thanks to Andy!
    - updated on 3.10.2012 - there was a terrible bug in LU, SoLE and Inversion. Thanks to Danilo Neves Cruz for reporting that!
	
    This code is distributed under MIT licence.
	
	
		Permission is hereby granted, free of charge, to any person
		obtaining a copy of this software and associated documentation
		files (the "Software"), to deal in the Software without
		restriction, including without limitation the rights to use,
		copy, modify, merge, publish, distribute, sublicense, and/or sell
		copies of the Software, and to permit persons to whom the
		Software is furnished to do so, subject to the following
		conditions:

		The above copyright notice and this permission notice shall be
		included in all copies or substantial portions of the Software.

		THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
		EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
		OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
		NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
		HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
		WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
		FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
		OTHER DEALINGS IN THE SOFTWARE.
*/

using LatticeCrypto.Utilities;
using System;
using System.Numerics;

namespace LatticeCrypto.Models
{
    public class MatrixND
    {
        public int rows;
        public int cols;
        public double[,] mat;

        public MatrixND L;
        public MatrixND U;
        private int[] pi;
        private double detOfP = 1;

        public MatrixND(int iRows, int iCols)         // Matrix Class constructor
        {
            rows = iRows;
            cols = iCols;
            mat = new double[rows, cols];
        }

        public bool IsSquare()
        {
            return (rows == cols);
        }

        public double this[int iRow, int iCol]      // Access this matrix as a 2D array
        {
            get => mat[iRow, iCol];
            set => mat[iRow, iCol] = value;
        }

        public MatrixND GetCol(int k)
        {
            MatrixND m = new MatrixND(rows, 1);
            for (int i = 0; i < rows; i++)
            {
                m[i, 0] = mat[i, k];
            }

            return m;
        }

        public void SetCol(MatrixND v, int k)
        {
            for (int i = 0; i < rows; i++)
            {
                mat[i, k] = v[i, 0];
            }
        }

        public void MakeLU()                        // Function for LU decomposition
        {
            if (!IsSquare())
            {
                throw new MException("The matrix is not square!");
            }

            L = IdentityMatrix(rows, cols);
            U = Duplicate();

            pi = new int[rows];
            for (int i = 0; i < rows; i++)
            {
                pi[i] = i;
            }

            int k0 = 0;

            for (int k = 0; k < cols - 1; k++)
            {
                double p = 0;
                for (int i = k; i < rows; i++)      // find the row with the biggest pivot
                {
                    if (Math.Abs(U[i, k]) > p)
                    {
                        p = Math.Abs(U[i, k]);
                        k0 = i;
                    }
                }
                if (p == 0) // samé nuly ve sloupci
                {
                    throw new MException("The matrix is singular!");
                }

                int pom1 = pi[k]; pi[k] = pi[k0]; pi[k0] = pom1;    // switch two rows in permutation matrix

                double pom2;
                for (int i = 0; i < k; i++)
                {
                    pom2 = L[k, i]; L[k, i] = L[k0, i]; L[k0, i] = pom2;
                }

                if (k != k0)
                {
                    detOfP *= -1;
                }

                for (int i = 0; i < cols; i++)                  // Switch rows in U
                {
                    pom2 = U[k, i]; U[k, i] = U[k0, i]; U[k0, i] = pom2;
                }

                for (int i = k + 1; i < rows; i++)
                {
                    L[i, k] = U[i, k] / U[k, k];
                    for (int j = k; j < cols; j++)
                    {
                        U[i, j] = U[i, j] - L[i, k] * U[k, j];
                    }
                }
            }
        }


        public MatrixND SolveWith(MatrixND v)                        // Function solves Ax = v in confirmity with solution vector "v"
        {
            if (rows != cols)
            {
                throw new MException("The matrix is not square!");
            }

            if (rows != v.rows)
            {
                throw new MException("Wrong number of results in solution vector!");
            }

            if (L == null)
            {
                MakeLU();
            }

            MatrixND b = new MatrixND(rows, 1);
            for (int i = 0; i < rows; i++)
            {
                b[i, 0] = v[pi[i], 0];   // switch two items in "v" due to permutation matrix
            }

            MatrixND z = SubsForth(L, b);
            MatrixND x = SubsBack(U, z);

            return x;
        }

        public MatrixND Invert()                                   // Function returns the inverted matrix
        {
            if (L == null)
            {
                MakeLU();
            }

            MatrixND inv = new MatrixND(rows, cols);

            for (int i = 0; i < rows; i++)
            {
                MatrixND Ei = ZeroMatrix(rows, 1);
                Ei[i, 0] = 1;
                MatrixND col = SolveWith(Ei);
                inv.SetCol(col, i);
            }
            return inv;
        }


        public double Det()                         // Function for determinant
        {
            if (L == null)
            {
                MakeLU();
            }

            double det = detOfP;
            for (int i = 0; i < rows; i++)
            {
                det *= U[i, i];
            }

            return det;
        }

        public MatrixND GetP()                        // Function returns permutation matrix "P" due to permutation vector "pi"
        {
            if (L == null)
            {
                MakeLU();
            }

            MatrixND matrixNd = ZeroMatrix(rows, cols);
            for (int i = 0; i < rows; i++)
            {
                matrixNd[pi[i], i] = 1;
            }

            return matrixNd;
        }

        public MatrixND Duplicate()                   // Function returns the copy of this matrix
        {
            MatrixND matrixNd = new MatrixND(rows, cols);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    matrixNd[i, j] = mat[i, j];
                }
            }

            return matrixNd;
        }

        public static MatrixND SubsForth(MatrixND A, MatrixND b)          // Function solves Ax = b for A as a lower triangular matrix
        {
            if (A.L == null)
            {
                A.MakeLU();
            }

            int n = A.rows;
            MatrixND x = new MatrixND(n, 1);

            for (int i = 0; i < n; i++)
            {
                x[i, 0] = b[i, 0];
                for (int j = 0; j < i; j++)
                {
                    x[i, 0] -= A[i, j] * x[j, 0];
                }

                x[i, 0] = x[i, 0] / A[i, i];
            }
            return x;
        }

        public static MatrixND SubsBack(MatrixND A, MatrixND b)           // Function solves Ax = b for A as an upper triangular matrix
        {
            if (A.L == null)
            {
                A.MakeLU();
            }

            int n = A.rows;
            MatrixND x = new MatrixND(n, 1);

            for (int i = n - 1; i > -1; i--)
            {
                x[i, 0] = b[i, 0];
                for (int j = n - 1; j > i; j--)
                {
                    x[i, 0] -= A[i, j] * x[j, 0];
                }

                x[i, 0] = x[i, 0] / A[i, i];
            }
            return x;
        }

        public static MatrixND ZeroMatrix(int iRows, int iCols)       // Function generates the zero matrix
        {
            MatrixND matrixNd = new MatrixND(iRows, iCols);
            for (int i = 0; i < iRows; i++)
            {
                for (int j = 0; j < iCols; j++)
                {
                    matrixNd[i, j] = 0;
                }
            }

            return matrixNd;
        }

        public static MatrixND IdentityMatrix(int iRows, int iCols)   // Function generates the identity matrix
        {
            MatrixND matrixNd = ZeroMatrix(iRows, iCols);
            for (int i = 0; i < Math.Min(iRows, iCols); i++)
            {
                matrixNd[i, i] = 1;
            }

            return matrixNd;
        }

        public static MatrixND RandomMatrix(int iRows, int iCols, int dispersion)       // Function generates the random matrix
        {
            Random random = new Random();
            MatrixND matrixNd = new MatrixND(iRows, iCols);
            for (int i = 0; i < iRows; i++)
            {
                for (int j = 0; j < iCols; j++)
                {
                    matrixNd[i, j] = random.Next(-dispersion, dispersion);
                }
            }

            return matrixNd;
        }

        //public static MatrixND Parse(string ps)                        // Function parses the matrix from string
        //{
        //    string s = NormalizeMatrixString(ps);
        //    string[] rows = Regex.Split(s, "\r\n");
        //    string[] nums = rows[0].Split(' ');
        //    MatrixND matrixNd = new MatrixND(rows.Length, nums.Length);
        //    try
        //    {
        //        for (int i = 0; i < rows.Length; i++)
        //        {
        //            nums = rows[i].Split(' ');
        //            for (int j = 0; j < nums.Length; j++) matrixNd[i, j] = double.Parse(nums[j]);
        //        }
        //    }
        //    catch (FormatException exc) { throw new MException("Wrong input format!"); }
        //    return matrixNd;
        //}

        public override string ToString()
        {
            string matrix = "";
            for (int i = 0; i < cols; i++)
            {
                matrix += FormatSettings.VectorTagOpen;
                for (int j = 0; j < rows; j++)
                {
                    matrix += this[j, i];
                    if (j < rows - 1)
                    {
                        matrix += FormatSettings.CoordinateSeparator;
                    }
                }
                matrix += FormatSettings.VectorTagClosed;
                if (i < cols - 1)
                {
                    matrix += FormatSettings.VectorSeparator;
                }
            }
            return FormatSettings.LatticeTagOpen + matrix + FormatSettings.LatticeTagClosed;
        }

        public string ToStringLog()
        {
            string matrix = "";
            for (int i = 0; i < cols; i++)
            {
                matrix += FormatSettings.VectorTagOpen;
                for (int j = 0; j < rows; j++)
                {
                    matrix += Util.FormatDoubleLog(this[j, i]);
                    if (j < rows - 1)
                    {
                        matrix += FormatSettings.CoordinateSeparator;
                    }
                }
                matrix += FormatSettings.VectorTagClosed;
                if (i < cols - 1)
                {
                    matrix += FormatSettings.VectorSeparator;
                }
            }
            return FormatSettings.LatticeTagOpen + matrix + FormatSettings.LatticeTagClosed;
        }

        public static MatrixND Transpose(MatrixND m)              // Matrix transpose, for any rectangular matrix
        {
            MatrixND t = new MatrixND(m.cols, m.rows);
            for (int i = 0; i < m.rows; i++)
            {
                for (int j = 0; j < m.cols; j++)
                {
                    t[j, i] = m[i, j];
                }
            }

            return t;
        }

        public static MatrixND Power(MatrixND m, int pow)           // Power matrix to exponent
        {
            if (pow == 0)
            {
                return IdentityMatrix(m.rows, m.cols);
            }

            if (pow == 1)
            {
                return m.Duplicate();
            }

            if (pow == -1)
            {
                return m.Invert();
            }

            MatrixND x;
            if (pow < 0) { x = m.Invert(); pow *= -1; }
            else
            {
                x = m.Duplicate();
            }

            MatrixND ret = IdentityMatrix(m.rows, m.cols);
            while (pow != 0)
            {
                if ((pow & 1) == 1)
                {
                    ret *= x;
                }

                x *= x;
                pow >>= 1;
            }
            return ret;
        }

        private static void SafeAplusBintoC(MatrixND A, int xa, int ya, MatrixND B, int xb, int yb, MatrixND C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
            {
                for (int j = 0; j < size; j++)     // cols
                {
                    C[i, j] = 0;
                    if (xa + j < A.cols && ya + i < A.rows)
                    {
                        C[i, j] += A[ya + i, xa + j];
                    }

                    if (xb + j < B.cols && yb + i < B.rows)
                    {
                        C[i, j] += B[yb + i, xb + j];
                    }
                }
            }
        }

        private static void SafeAminusBintoC(MatrixND A, int xa, int ya, MatrixND B, int xb, int yb, MatrixND C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
            {
                for (int j = 0; j < size; j++)     // cols
                {
                    C[i, j] = 0;
                    if (xa + j < A.cols && ya + i < A.rows)
                    {
                        C[i, j] += A[ya + i, xa + j];
                    }

                    if (xb + j < B.cols && yb + i < B.rows)
                    {
                        C[i, j] -= B[yb + i, xb + j];
                    }
                }
            }
        }

        private static void SafeACopytoC(MatrixND A, int xa, int ya, MatrixND C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
            {
                for (int j = 0; j < size; j++)     // cols
                {
                    C[i, j] = 0;
                    if (xa + j < A.cols && ya + i < A.rows)
                    {
                        C[i, j] += A[ya + i, xa + j];
                    }
                }
            }
        }

        private static void AplusBintoC(MatrixND A, int xa, int ya, MatrixND B, int xb, int yb, MatrixND C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
            {
                for (int j = 0; j < size; j++)
                {
                    C[i, j] = A[ya + i, xa + j] + B[yb + i, xb + j];
                }
            }
        }

        private static void AminusBintoC(MatrixND A, int xa, int ya, MatrixND B, int xb, int yb, MatrixND C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
            {
                for (int j = 0; j < size; j++)
                {
                    C[i, j] = A[ya + i, xa + j] - B[yb + i, xb + j];
                }
            }
        }

        private static void ACopytoC(MatrixND A, int xa, int ya, MatrixND C, int size)
        {
            for (int i = 0; i < size; i++)          // rows
            {
                for (int j = 0; j < size; j++)
                {
                    C[i, j] = A[ya + i, xa + j];
                }
            }
        }

        private static MatrixND StrassenMultiply(MatrixND A, MatrixND B)                // Smart matrix multiplication
        {
            if (A.cols != B.rows)
            {
                throw new MException("Wrong dimension of matrix!");
            }

            MatrixND R;

            int msize = Math.Max(Math.Max(A.rows, A.cols), Math.Max(B.rows, B.cols));

            if (msize < 32)
            {
                R = ZeroMatrix(A.rows, B.cols);
                for (int i = 0; i < R.rows; i++)
                {
                    for (int j = 0; j < R.cols; j++)
                    {
                        for (int k = 0; k < A.cols; k++)
                        {
                            R[i, j] += A[i, k] * B[k, j];
                        }
                    }
                }

                return R;
            }

            int size = 1; int n = 0;
            while (msize > size) { size *= 2; n++; }
            int h = size / 2;


            MatrixND[,] mField = new MatrixND[n, 9];

            /*
         *  8x8, 8x8, 8x8, ...
         *  4x4, 4x4, 4x4, ...
         *  2x2, 2x2, 2x2, ...
         *  . . .
         */

            for (int i = 0; i < n - 4; i++)          // rows
            {
                int z = (int)Math.Pow(2, n - i - 1);
                for (int j = 0; j < 9; j++)
                {
                    mField[i, j] = new MatrixND(z, z);
                }
            }

            SafeAplusBintoC(A, 0, 0, A, h, h, mField[0, 0], h);
            SafeAplusBintoC(B, 0, 0, B, h, h, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 1], 1, mField); // (A11 + A22) * (B11 + B22);

            SafeAplusBintoC(A, 0, h, A, h, h, mField[0, 0], h);
            SafeACopytoC(B, 0, 0, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 2], 1, mField); // (A21 + A22) * B11;

            SafeACopytoC(A, 0, 0, mField[0, 0], h);
            SafeAminusBintoC(B, h, 0, B, h, h, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 3], 1, mField); //A11 * (B12 - B22);

            SafeACopytoC(A, h, h, mField[0, 0], h);
            SafeAminusBintoC(B, 0, h, B, 0, 0, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 4], 1, mField); //A22 * (B21 - B11);

            SafeAplusBintoC(A, 0, 0, A, h, 0, mField[0, 0], h);
            SafeACopytoC(B, h, h, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 5], 1, mField); //(A11 + A12) * B22;

            SafeAminusBintoC(A, 0, h, A, 0, 0, mField[0, 0], h);
            SafeAplusBintoC(B, 0, 0, B, h, 0, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 6], 1, mField); //(A21 - A11) * (B11 + B12);

            SafeAminusBintoC(A, h, 0, A, h, h, mField[0, 0], h);
            SafeAplusBintoC(B, 0, h, B, h, h, mField[0, 1], h);
            StrassenMultiplyRun(mField[0, 0], mField[0, 1], mField[0, 1 + 7], 1, mField); // (A12 - A22) * (B21 + B22);

            R = new MatrixND(A.rows, B.cols);                  // result

            // C11
            for (int i = 0; i < Math.Min(h, R.rows); i++)          // rows
            {
                for (int j = 0; j < Math.Min(h, R.cols); j++)     // cols
                {
                    R[i, j] = mField[0, 1 + 1][i, j] + mField[0, 1 + 4][i, j] - mField[0, 1 + 5][i, j] + mField[0, 1 + 7][i, j];
                }
            }

            // C12
            for (int i = 0; i < Math.Min(h, R.rows); i++)          // rows
            {
                for (int j = h; j < Math.Min(2 * h, R.cols); j++)     // cols
                {
                    R[i, j] = mField[0, 1 + 3][i, j - h] + mField[0, 1 + 5][i, j - h];
                }
            }

            // C21
            for (int i = h; i < Math.Min(2 * h, R.rows); i++)          // rows
            {
                for (int j = 0; j < Math.Min(h, R.cols); j++)     // cols
                {
                    R[i, j] = mField[0, 1 + 2][i - h, j] + mField[0, 1 + 4][i - h, j];
                }
            }

            // C22
            for (int i = h; i < Math.Min(2 * h, R.rows); i++)          // rows
            {
                for (int j = h; j < Math.Min(2 * h, R.cols); j++)     // cols
                {
                    R[i, j] = mField[0, 1 + 1][i - h, j - h] - mField[0, 1 + 2][i - h, j - h] + mField[0, 1 + 3][i - h, j - h] + mField[0, 1 + 6][i - h, j - h];
                }
            }

            return R;
        }

        // function for square matrix 2^N x 2^N

        private static void StrassenMultiplyRun(MatrixND A, MatrixND B, MatrixND C, int l, MatrixND[,] f)    // A * B into C, level of recursion, matrix field
        {
            int size = A.rows;
            int h = size / 2;

            if (size < 32)
            {
                for (int i = 0; i < C.rows; i++)
                {
                    for (int j = 0; j < C.cols; j++)
                    {
                        C[i, j] = 0;
                        for (int k = 0; k < A.cols; k++)
                        {
                            C[i, j] += A[i, k] * B[k, j];
                        }
                    }
                }

                return;
            }

            AplusBintoC(A, 0, 0, A, h, h, f[l, 0], h);
            AplusBintoC(B, 0, 0, B, h, h, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 1], l + 1, f); // (A11 + A22) * (B11 + B22);

            AplusBintoC(A, 0, h, A, h, h, f[l, 0], h);
            ACopytoC(B, 0, 0, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 2], l + 1, f); // (A21 + A22) * B11;

            ACopytoC(A, 0, 0, f[l, 0], h);
            AminusBintoC(B, h, 0, B, h, h, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 3], l + 1, f); //A11 * (B12 - B22);

            ACopytoC(A, h, h, f[l, 0], h);
            AminusBintoC(B, 0, h, B, 0, 0, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 4], l + 1, f); //A22 * (B21 - B11);

            AplusBintoC(A, 0, 0, A, h, 0, f[l, 0], h);
            ACopytoC(B, h, h, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 5], l + 1, f); //(A11 + A12) * B22;

            AminusBintoC(A, 0, h, A, 0, 0, f[l, 0], h);
            AplusBintoC(B, 0, 0, B, h, 0, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 6], l + 1, f); //(A21 - A11) * (B11 + B12);

            AminusBintoC(A, h, 0, A, h, h, f[l, 0], h);
            AplusBintoC(B, 0, h, B, h, h, f[l, 1], h);
            StrassenMultiplyRun(f[l, 0], f[l, 1], f[l, 1 + 7], l + 1, f); // (A12 - A22) * (B21 + B22);

            // C11
            for (int i = 0; i < h; i++)          // rows
            {
                for (int j = 0; j < h; j++)     // cols
                {
                    C[i, j] = f[l, 1 + 1][i, j] + f[l, 1 + 4][i, j] - f[l, 1 + 5][i, j] + f[l, 1 + 7][i, j];
                }
            }

            // C12
            for (int i = 0; i < h; i++)          // rows
            {
                for (int j = h; j < size; j++)     // cols
                {
                    C[i, j] = f[l, 1 + 3][i, j - h] + f[l, 1 + 5][i, j - h];
                }
            }

            // C21
            for (int i = h; i < size; i++)          // rows
            {
                for (int j = 0; j < h; j++)     // cols
                {
                    C[i, j] = f[l, 1 + 2][i - h, j] + f[l, 1 + 4][i - h, j];
                }
            }

            // C22
            for (int i = h; i < size; i++)          // rows
            {
                for (int j = h; j < size; j++)     // cols
                {
                    C[i, j] = f[l, 1 + 1][i - h, j - h] - f[l, 1 + 2][i - h, j - h] + f[l, 1 + 3][i - h, j - h] + f[l, 1 + 6][i - h, j - h];
                }
            }
        }

        public static MatrixND StupidMultiply(MatrixND m1, MatrixND m2)                  // Stupid matrix multiplication
        {
            if (m1.cols != m2.rows)
            {
                throw new MException("Wrong dimensions of matrix!");
            }

            MatrixND result = ZeroMatrix(m1.rows, m2.cols);
            for (int i = 0; i < result.rows; i++)
            {
                for (int j = 0; j < result.cols; j++)
                {
                    for (int k = 0; k < m1.cols; k++)
                    {
                        result[i, j] += m1[i, k] * m2[k, j];
                    }
                }
            }

            return result;
        }
        private static MatrixND Multiply(double n, MatrixND m)                          // Multiplication by constant n
        {
            MatrixND r = new MatrixND(m.rows, m.cols);
            for (int i = 0; i < m.rows; i++)
            {
                for (int j = 0; j < m.cols; j++)
                {
                    r[i, j] = m[i, j] * n;
                }
            }

            return r;
        }
        private static MatrixND Add(MatrixND m1, MatrixND m2)         // Sčítání matic
        {
            if (m1.rows != m2.rows || m1.cols != m2.cols)
            {
                throw new MException("Matrices must have the same dimensions!");
            }

            MatrixND r = new MatrixND(m1.rows, m1.cols);
            for (int i = 0; i < r.rows; i++)
            {
                for (int j = 0; j < r.cols; j++)
                {
                    r[i, j] = m1[i, j] + m2[i, j];
                }
            }

            return r;
        }

        private static MatrixND Mod(MatrixND m, int q)
        {
            MatrixND r = new MatrixND(m.rows, m.cols);
            for (int i = 0; i < r.rows; i++)
            {
                for (int j = 0; j < r.cols; j++)
                {
                    r[i, j] = m[i, j] % q;
                    if (r[i, j] < 0)
                    {
                        r[i, j] += q;
                    }
                }
            }

            return r;
        }

        public static string NormalizeMatrixString(string matStr)	// From Andy - thank you! :)
        {
            // Remove any multiple spaces
            while (matStr.IndexOf("  ") != -1)
            {
                matStr = matStr.Replace("  ", " ");
            }

            // Remove any spaces before or after newlines
            matStr = matStr.Replace(" \r\n", "\r\n");
            matStr = matStr.Replace("\r\n ", "\r\n");

            // If the data ends in a newline, remove the trailing newline.
            // Make it easier by first replacing \r\n’s with |’s then
            // restore the |’s with \r\n’s
            matStr = matStr.Replace("\r\n", "|");
            while (matStr.LastIndexOf("|") == (matStr.Length - 1))
            {
                matStr = matStr.Substring(0, matStr.Length - 1);
            }

            matStr = matStr.Replace("|", "\r\n");
            return matStr;
        }

        public LatticeND ToLatticeND()
        {
            LatticeND latticeND = new LatticeND(cols, rows, false);
            for (int i = 0; i < cols; i++)
            {
                latticeND.Vectors[i] = new VectorND(rows);
                for (int j = 0; j < rows; j++)
                {
                    latticeND.Vectors[i].values[j] = new BigInteger(mat[j, i]);
                }
            }
            return latticeND;
        }

        public override bool Equals(object obj)
        {
            MatrixND m = (MatrixND)obj;
            if (cols != m.cols || rows != m.rows)
            {
                return false;
            }

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (this[i, j] != m[i, j])
                    {
                        return false;
                    }
                }
            }

            return true;
        }



        //   O P E R A T O R S

        public static MatrixND operator -(MatrixND m)
        { return Multiply(-1, m); }

        public static MatrixND operator +(MatrixND m1, MatrixND m2)
        { return Add(m1, m2); }

        public static MatrixND operator -(MatrixND m1, MatrixND m2)
        { return Add(m1, -m2); }

        public static MatrixND operator *(MatrixND m1, MatrixND m2)
        { return StrassenMultiply(m1, m2); }

        public static MatrixND operator *(double n, MatrixND m)
        { return Multiply(n, m); }

        public static MatrixND operator *(MatrixND m, double n)
        { return Multiply(n, m); }

        public static MatrixND operator %(MatrixND m, int q)
        { return Mod(m, q); }
    }

    //  The class for exceptions

    public class MException : Exception
    {
        public MException(string Message)
            : base(Message)
        { }
    }
}