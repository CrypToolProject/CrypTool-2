using System;

namespace net.watermark
{

    /* Original Project can be found at https://code.google.com/p/dct-watermark/
    * Ported to C# to be used within CrypTool 2 by Nils Rehwald
    * Thanks to cgaffa, ZXing and everyone else who worked on the original Project for making the original Java sources available publicly
    * Thanks to Nils Kopal for Support and Bugfixing */

    // Program name: Qt.java
    // Program features: Qt category, including WaterQt (quantization) and WaterDeQt (inverse quantization) two methods
    // Ported to C# by Nils Rehwald

    internal class Qt
    {
        internal static int N = 4;

        public double[][] qTable = new double[][]
        {
            new double[] {20, 30, 30, 35},
            new double[] {30, 30, 35, 45},
            new double[] {30, 35, 45, 50},
            new double[] {35, 45, 50, 60}
        };

        public double[][] filter = new double[][]
        {
            new double[] {0.2, 0.6, 0.6, 1},
            new double[] {0.6, 0.6, 1, 1.1},
            new double[] {0.6, 1, 1.1, 1.2},
            new double[] {1, 1.1, 1.2, 1.3}
        };

        internal Qt()
        {
        }

        /// <summary>
        /// Quantization </summary>
        internal virtual void WaterDeQt(int[][] input, int[][] output)
        {
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    output[i][j] = (int)(input[i][j] * (qTable[i][j] * filter[i][j]));
                }
            }
        }

        /// <summary>
        /// De Quantization </summary>
        internal virtual void WaterQt(int[][] input, int[][] output)
        {
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    output[i][j] = (int)Math.Round(input[i][j] / (qTable[i][j] * filter[i][j]));
                }
            }
        }

    }

}