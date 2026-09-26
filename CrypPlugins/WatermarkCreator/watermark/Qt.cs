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