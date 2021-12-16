using System;

namespace net.watermark
{
    /* Original Project can be found at https://code.google.com/p/dct-watermark/
    * Ported to C# to be used within CrypTool 2 by Nils Rehwald
    * Thanks to cgaffa, ZXing and everyone else who worked on the original Project for making the original Java sources available publicly
    * Thanks to Nils Kopal for Support and Bugfixing */


    public class DCT
    {
        private readonly bool InstanceFieldsInitialized = false;

        private void InitializeInstanceFields()
        {
            c = RectangularArrays.ReturnRectangularDoubleArray(n, n);
            ct = RectangularArrays.ReturnRectangularDoubleArray(n, n);
        }

        private readonly int n;

        public double[][] c;

        public double[][] ct;

        public DCT() : this(8)
        {
            if (!InstanceFieldsInitialized)
            {
                InitializeInstanceFields();
                InstanceFieldsInitialized = true;
            }
        }

        public DCT(int N)
        {
            n = N;
            if (!InstanceFieldsInitialized)
            {
                InitializeInstanceFields();
                InstanceFieldsInitialized = true;
            }
            int i;
            int j;
            double pi = Math.Atan(1.0) * 4.0;
            for (j = 0; j < N; j++)
            {
                c[0][j] = 1.0 / Math.Sqrt(N);
                ct[j][0] = c[0][j];
            }
            for (i = 1; i < N; i++)
            {
                for (j = 0; j < N; j++)
                {
                    c[i][j] = Math.Sqrt(2.0 / N) * Math.Cos(pi * (2 * j + 1) * i / (2.0 * N));
                    ct[j][i] = c[i][j];
                }
            }
        }

        internal virtual void ForwardDCT(int[][] input, int[][] output)
        {

            double[][] temp = RectangularArrays.ReturnRectangularDoubleArray(n, n);
            double temp1;
            int i, j, k;
            for (i = 0; i < n; i++)
            {
                for (j = 0; j < n; j++)
                {
                    temp[i][j] = 0.0;
                    for (k = 0; k < n; k++)
                    {
                        temp[i][j] += (input[i][k] - 128) * ct[k][j];
                    }
                }
            }

            for (i = 0; i < n; i++)
            {
                for (j = 0; j < n; j++)
                {
                    temp1 = 0.0;
                    for (k = 0; k < n; k++)
                    {
                        temp1 += c[i][k] * temp[k][j];
                    }
                    output[i][j] = (int)Math.Round(temp1);
                }
            }
        }

        internal virtual void InverseDCT(int[][] input, int[][] output)
        {

            double[][] temp = RectangularArrays.ReturnRectangularDoubleArray(n, n);
            double temp1;
            int i, j, k;

            for (i = 0; i < n; i++)
            {
                for (j = 0; j < n; j++)
                {
                    temp[i][j] = 0.0;
                    for (k = 0; k < n; k++)
                    {
                        temp[i][j] += input[i][k] * c[k][j];
                    }
                }
            }

            for (i = 0; i < n; i++)
            {
                for (j = 0; j < n; j++)
                {
                    temp1 = 0.0;
                    for (k = 0; k < n; k++)
                    {
                        temp1 += ct[i][k] * temp[k][j];
                    }
                    temp1 += 128.0;
                    if (temp1 < 0)
                    {
                        output[i][j] = 0;
                    }
                    else if (temp1 > 255)
                    {
                        output[i][j] = 255;
                    }
                    else
                    {
                        output[i][j] = (int)Math.Round(temp1);
                    }
                }
            }
        }
    }

}