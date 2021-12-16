/* Original Project can be found at https://code.google.com/p/dct-watermark/
 * Ported to C# to be used within CrypTool 2 by Nils Rehwald
 * Thanks to cgaffa, ZXing and everyone else who worked on the original Project for making the original Java sources available publicly
 * Thanks to Nils Kopal for Support and Bugfixing 

 * Copyright 2012 by Christoph Gaffga licensed under the Apache License, Version 2.0 (the "License"); you may not use
 * this file except in compliance with the License. You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0 Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
 * either express or implied. See the License for the specific language governing permissions and limitations under the
 * License.
 */
using net.util;
using System;
using System.Drawing;
using System.Text;

namespace net.watermark
{


    using Bits = Bits;

    /// <summary>
	/// Implementation of a watermarking also. See https://code.google.com/p/dct-watermark/
	/// 
	/// @author Christoph Gaffga
    /// @author Ported to C# by Nils Rehwald
	/// </summary>
	public class Watermark
    {
        private bool _stopped = false;
        /// <summary>
        /// Valid characters and their order in our 6-bit charset. </summary>
        public static readonly string VALID_CHARS = " abcdefghijklmnopqrstuvwxyz0123456789.-,:/()?!\"'#*+_%$&=<>[];@§\n";

        /// <summary>
        /// The width and height of our quantization box in pixels (n-times n pixel per bit). </summary>
        internal int bitBoxSize = 10;

        /// <summary>
        /// Number of bytes used for Reed-Solomon error correction. No error correction if zero. </summary>
        internal int byteLenErrorCorrection = 0;

        /// <summary>
        /// Number of bits that could be stored, total, including error correction bits. </summary>
        internal int maxBitsTotal;

        /// <summary>
        /// Number of bits for data (excluding error correction). </summary>
        internal int maxBitsData;

        /// <summary>
        /// Maximal length in of characters for text messages. </summary>
        internal int maxTextLen;

        /// <summary>
        /// Opacity of the marks when added to the image. </summary>
        internal double opacity = 1.0; // 1.0 is strongest watermark

        /// <summary>
        /// Seed for randomization of the watermark. </summary>
        private readonly long randomizeWatermarkSeed = 19;

        /// <summary>
        /// Seed for randomization of the embedding. </summary>
        private readonly long randomizeEmbeddingSeed = 24;

        public Watermark()
        {
            calculateSizes();
        }

        public Watermark(int boxSize, int errorCorrectionBytes, double opacity)
        {
            bitBoxSize = boxSize;
            byteLenErrorCorrection = errorCorrectionBytes;
            this.opacity = opacity;
            calculateSizes();
        }

        public Watermark(int boxSize, int errorCorrectionBytes, double opacity, long seed1, long seed2)
        {
            bitBoxSize = boxSize;
            byteLenErrorCorrection = errorCorrectionBytes;
            this.opacity = opacity;
            randomizeEmbeddingSeed = seed1;
            randomizeWatermarkSeed = seed2;
            calculateSizes();
        }

        public Watermark(long seed1, long seed2)
        {
            randomizeEmbeddingSeed = seed1;
            randomizeWatermarkSeed = seed2;
            calculateSizes();
        }

        private string bits2String(Bits bits)
        {
            StringBuilder buf = new StringBuilder();
            {
                for (int i = 0; i < maxTextLen; i++)
                {
                    int c = (int)bits.getValue(i * 6, 6);
                    buf.Append(VALID_CHARS[c]);
                }
            }
            return buf.ToString();
        }

        private void calculateSizes()
        {
            maxBitsTotal = 128 / bitBoxSize * (128 / bitBoxSize);
            maxBitsData = maxBitsTotal - byteLenErrorCorrection * 8;
            maxTextLen = maxBitsData / 6;
        }

        public virtual void embed(Bitmap image, Bits data)
        {
            Bits bits;
            // make the size fit...
            if (data.size() > maxBitsData)
            {
                bits = new Bits(data.getBits(0, maxBitsData));
            }
            else
            {
                bits = new Bits(data);
                while (bits.size() < maxBitsData)
                {
                    bits.addBit(false);
                }
            }

            // add error correction...
            if (byteLenErrorCorrection > 0)
            {
                bits = Bits.bitsReedSolomonEncode(bits, byteLenErrorCorrection);
            }

            // create watermark image...
            int[][] watermarkBitmap = RectangularArrays.ReturnRectangularIntArray(128, 128);
            for (int y = 0; y < 128 / bitBoxSize * bitBoxSize; y++)
            //Yes, for some reason a/b*b != a...don't ask me why, it's also not equal to a/b^2, but seems it needs to  be this way
            {
                for (int x = 0; x < 128 / bitBoxSize * bitBoxSize; x++)
                {
                    if (bits.size() > x / bitBoxSize + y / bitBoxSize * (128 / bitBoxSize))
                    {
                        if (_stopped)
                        {
                            return;
                        }
                        watermarkBitmap[y][x] = bits.getBit(x / bitBoxSize + y / bitBoxSize * (128 / bitBoxSize)) ? 255 : 0;
                    }
                }
            }

            // embedding...
            int[][] grey = embed(image, watermarkBitmap);

            // added computed data to original image...
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    if (_stopped)
                    {
                        return;
                    }
                    Color color = image.GetPixel(x, y);
                    double[] hsb = RGBtoHSB(color);
                    // adjust brightness of the pixel...
                    hsb[2] = (float)(hsb[2] * (1.0 - opacity) + grey[y][x] * opacity / 255.0);
                    Color colorNew = HSBtoRGB(hsb[0], hsb[1], hsb[2]);
                    image.SetPixel(x, y, colorNew);

                }
            }
        }

        private int[][] embed(Bitmap src, int[][] water1)
        {
            int width = (src.Width + 7) / 8 * 8;
            int height = (src.Height + 7) / 8 * 8;

            // original image process
            const int N = 8;
            int[][] buff1 = RectangularArrays.ReturnRectangularIntArray(height, width); // Original image
            int[][] buff2 = RectangularArrays.ReturnRectangularIntArray(height, width); // DCT Original image coefficients
            int[][] buff3 = RectangularArrays.ReturnRectangularIntArray(height, width); // IDCT Original image coefficients
            int[][] b1 = RectangularArrays.ReturnRectangularIntArray(N, N); // DCT input
            int[][] b2 = RectangularArrays.ReturnRectangularIntArray(N, N); // DCT output
            int[][] b3 = RectangularArrays.ReturnRectangularIntArray(N, N); // IDCT input
            int[][] b4 = RectangularArrays.ReturnRectangularIntArray(N, N); // IDCT output
                                                                            // watermark image process
            const int W = 4;
            int[][] water2 = RectangularArrays.ReturnRectangularIntArray(128, 128); // random watermark image
            int[][] water3 = RectangularArrays.ReturnRectangularIntArray(128, 128); // DCT watermark image coefficients
            int[][] w1 = RectangularArrays.ReturnRectangularIntArray(W, W); // DCT input
            int[][] w2 = RectangularArrays.ReturnRectangularIntArray(W, W); // DCT output
            int[][] w3 = RectangularArrays.ReturnRectangularIntArray(W, W); // quantization output
            int[][] mfbuff1 = RectangularArrays.ReturnRectangularIntArray(128, 128); // embed coefficients
            int[] mfbuff2 = new int[width * height]; // 2 to 1
                                                     // random process...
            int a, b, c;
            int[] tmp = new int[128 * 128];
            // random embed...
            int c1;
            int cc = 0;
            int[] tmp1 = new int[128 * 128];
            // divide 8x8 block...
            int k = 0, l = 0;
            // init buf1 from src image...
            for (int y = 0; y < src.Height; y++)
            {
                for (int x = 0; x < src.Width; x++)
                {
                    if (_stopped)
                    {
                        return null;
                    }
                    Color color = new Color();
                    color = src.GetPixel(x, y);
                    double[] hsb = RGBtoHSB(color);
                    // use brightness of the pixel...
                    buff1[y][x] = (int)(hsb[2] * 255.0);
                }
            }
            for (int y = 0; y < height; y += N)
            {
                for (int x = 0; x < width; x += N)
                {
                    for (int i = y; i < y + N; i++)
                    {
                        for (int j = x; j < x + N; j++)
                        {
                            b1[k][l] = buff1[i][j];
                            l++;
                        }
                        l = 0;
                        k++;
                    }
                    k = 0;
                    DCT o1 = new DCT();
                    o1.ForwardDCT(b1, b2);

                    for (int p = y; p < y + N; p++)
                    {
                        for (int q = x; q < x + N; q++)
                        {
                            buff2[p][q] = b2[k][l];
                            l++;
                        }
                        l = 0;
                        k++;
                    }
                    k = 0;

                }
            }
            Random r = new Random((int)randomizeWatermarkSeed);
            for (int i = 0; i < 128; i++)
            {
                for (int j = 0; j < 128; j++)
                {
                    while (true)
                    {
                        c = r.Next(128 * 128);
                        if (tmp[c] == 0)
                        {
                            break;
                        }
                    }
                    a = c / 128;
                    b = c % 128;
                    water2[i][j] = water1[a][b];
                    tmp[c] = 1;
                }
            }

            k = 0;
            l = 0;
            for (int y = 0; y < 128; y += W)
            {
                for (int x = 0; x < 128; x += W)
                {
                    for (int i = y; i < y + W; i++)
                    {
                        for (int j = x; j < x + W; j++)
                        {
                            w1[k][l] = water2[i][j];
                            l++;
                        }
                        l = 0;
                        k++;
                    }
                    k = 0;

                    DCT wm1 = new DCT(4);

                    wm1.ForwardDCT(w1, w2);

                    Qt qw1 = new Qt();
                    qw1.WaterQt(w2, w3);

                    for (int p = y; p < y + W; p++)
                    {
                        for (int q = x; q < x + W; q++)
                        {
                            water3[p][q] = w3[k][l];
                            l++;
                        }
                        l = 0;
                        k++;
                    }
                    k = 0;
                }
            }

            // Random Embedding
            Random r1 = new Random((int)randomizeEmbeddingSeed);
            for (int i = 0; i < 128; i++)
            {
                for (int j = 0; j < 128; j++)
                {
                    while (true)
                    {
                        c1 = r1.Next(128 * 128);
                        if (tmp1[c1] == 0)
                        {
                            break;
                        }
                    }
                    a = c1 / 128;
                    b = c1 % 128;
                    mfbuff1[i][j] = water3[a][b];
                    tmp1[c1] = 1;
                }
            }

            ZigZag scan = new ZigZag();
            scan.two2one(mfbuff1, mfbuff2);

            // WriteBack coefficients
            for (int i = 0; i < height; i += N)
            {
                for (int j = 0; j < width; j += N)
                {
                    buff2[i + 1][j + 4] = mfbuff2[cc];
                    cc++;
                    buff2[i + 2][j + 3] = mfbuff2[cc];
                    cc++;
                    buff2[i + 3][j + 2] = mfbuff2[cc];
                    cc++;
                    buff2[i + 4][j + 1] = mfbuff2[cc];
                    cc++;
                }
            }
            cc = 0;
            k = 0;
            l = 0;
            for (int y = 0; y < height; y += N)
            {
                for (int x = 0; x < width; x += N)
                {
                    for (int i = y; i < y + N; i++)
                    {
                        for (int j = x; j < x + N; j++)
                        {
                            b3[k][l] = buff2[i][j];
                            l++;
                        }
                        l = 0;
                        k++;
                    }
                    k = 0;

                    DCT o2 = new DCT();
                    o2.InverseDCT(b3, b4);

                    for (int p = y; p < y + N; p++)
                    {
                        for (int q = x; q < x + N; q++)
                        {
                            buff3[p][q] = b4[k][l];
                            l++;
                        }
                        l = 0;
                        k++;
                    }
                    k = 0;
                }
            }

            return buff3;
        }

        public virtual void embed(Bitmap image, string data)
        {
            embed(image, string2Bits(data));
        }

        public virtual Bits extractData(Bitmap image)
        {
            int[][] extracted = extractRaw(image);
            if (extracted == null)
            {
                return null;
            }

            // black/white the extracted result...
            for (int y = 0; y < 128 / bitBoxSize * bitBoxSize; y += bitBoxSize)
            {
                for (int x = 0; x < 128 / bitBoxSize * bitBoxSize; x += bitBoxSize)
                {
                    int sum = 0;
                    for (int y2 = y; y2 < y + bitBoxSize; y2++)
                    {
                        for (int x2 = x; x2 < x + bitBoxSize; x2++)
                        {
                            sum += extracted[y2][x2];
                        }
                    }
                    sum = sum / (bitBoxSize * bitBoxSize);
                    for (int y2 = y; y2 < y + bitBoxSize; y2++)
                    {
                        for (int x2 = x; x2 < x + bitBoxSize; x2++)
                        {
                            extracted[y2][x2] = sum > 127 ? 255 : 0;
                        }
                    }
                }
            }

            // reconstruct bits...
            Bits bits = new Bits();
            for (int y = 0; y < 128 / bitBoxSize * bitBoxSize; y += bitBoxSize)
            {
                for (int x = 0; x < 128 / bitBoxSize * bitBoxSize; x += bitBoxSize)
                {
                    bits.addBit(extracted[y][x] > 127);
                }
            }
            bits = new Bits(bits.getBits(0, maxBitsTotal));

            // apply error correction...
            if (byteLenErrorCorrection > 0)
            {
                bits = Bits.bitsReedSolomonDecode(bits, byteLenErrorCorrection);
            }

            return bits;
        }

        private int[][] extractRaw(Bitmap src)
        {
            int width = (src.Width + 7) / 8 * 8;
            int height = (src.Height + 7) / 8 * 8;

            // original image
            const int N = 8;
            int[][] buff1 = RectangularArrays.ReturnRectangularIntArray(height, width); // watermarked image
            int[][] buff2 = RectangularArrays.ReturnRectangularIntArray(height, width); // DCT watermarked image coefficients
            int[][] b1 = RectangularArrays.ReturnRectangularIntArray(N, N); // DCT input
            int[][] b2 = RectangularArrays.ReturnRectangularIntArray(N, N); // DCT output

            // watermark
            const int W = 4;
            int[][] water1 = RectangularArrays.ReturnRectangularIntArray(128, 128); // extract watermark image
            int[][] water2 = RectangularArrays.ReturnRectangularIntArray(128, 128); // DCT watermark image coefficients
            int[][] water3 = RectangularArrays.ReturnRectangularIntArray(128, 128); // random watermark image
            int[][] w1 = RectangularArrays.ReturnRectangularIntArray(W, W); // dequantization output
            int[][] w2 = RectangularArrays.ReturnRectangularIntArray(W, W); // DCT input
            int[][] w3 = RectangularArrays.ReturnRectangularIntArray(W, W); // DCT output

            // random process
            int a, b, c, c1;
            int[] tmp = new int[128 * 128];
            int[] tmp1 = new int[128 * 128];
            int cc = 0;

            // middle frequency coefficients
            // final int mfbuff1[] = new int[128 * 128];
            int[] mfbuff1 = new int[width * height];
            int[][] mfbuff2 = RectangularArrays.ReturnRectangularIntArray(128, 128); // 1 to 2

            // divide 8x8 block
            int k = 0, l = 0;

            // init buf1 from watermarked image src...
            for (int y = 0; y < src.Height; y++)
            {
                for (int x = 0; x < src.Width; x++)
                {
                    if (_stopped)
                    {
                        return null;
                    }
                    Color color = new Color();
                    color = src.GetPixel(x, y);
                    double[] hsb = RGBtoHSB(color);
                    // use brightness of the pixel...
                    buff1[y][x] = (int)(hsb[2] * 255.0);
                }
            }

            for (int y = 0; y < height; y += N)
            {
                for (int x = 0; x < width; x += N)
                {
                    for (int i = y; i < y + N; i++)
                    {
                        for (int j = x; j < x + N; j++)
                        {
                            b1[k][l] = buff1[i][j];
                            l++;
                        }
                        l = 0;
                        k++;
                    }
                    k = 0;

                    DCT o1 = new DCT();
                    o1.ForwardDCT(b1, b2);

                    for (int p = y; p < y + N; p++)
                    {
                        for (int q = x; q < x + N; q++)
                        {
                            buff2[p][q] = b2[k][l];
                            l++;
                        }
                        l = 0;
                        k++;
                    }
                    k = 0;
                }
            }

            for (int i = 0; i < height; i += N)
            {
                for (int j = 0; j < width; j += N)
                {
                    mfbuff1[cc] = buff2[i + 1][j + 4];
                    cc++;
                    mfbuff1[cc] = buff2[i + 2][j + 3];
                    cc++;
                    mfbuff1[cc] = buff2[i + 3][j + 2];
                    cc++;
                    mfbuff1[cc] = buff2[i + 4][j + 1];
                    cc++;
                }
            }
            cc = 0;

            ZigZag scan = new ZigZag();
            scan.one2two(mfbuff1, mfbuff2);

            // random extracting
            Random r1 = new Random((int)randomizeEmbeddingSeed);
            for (int i = 0; i < 128; i++)
            {
                for (int j = 0; j < 128; j++)
                {
                    while (true)
                    {
                        c1 = r1.Next(128 * 128);
                        if (tmp1[c1] == 0)
                        {
                            break;
                        }
                    }
                    a = c1 / 128;
                    b = c1 % 128;
                    water1[a][b] = mfbuff2[i][j];
                    tmp1[c1] = 1;
                }
            }

            k = 0;
            l = 0;
            for (int y = 0; y < 128; y += W)
            {
                for (int x = 0; x < 128; x += W)
                {

                    for (int i = y; i < y + W; i++)
                    {
                        for (int j = x; j < x + W; j++)
                        {
                            w1[k][l] = water1[i][j];
                            l++;
                        }
                        l = 0;
                        k++;
                    }
                    k = 0;

                    Qt qw2 = new Qt();
                    qw2.WaterDeQt(w1, w2);

                    DCT wm2 = new DCT(4);
                    wm2.InverseDCT(w2, w3);
                    for (int p = y; p < y + W; p++)
                    {
                        for (int q = x; q < x + W; q++)
                        {
                            water2[p][q] = w3[k][l];
                            l++;
                        }
                        l = 0;
                        k++;
                    }
                    k = 0;
                }
            }

            Random r = new Random((int)randomizeWatermarkSeed);
            for (int i = 0; i < 128; i++)
            {
                for (int j = 0; j < 128; j++)
                {
                    while (true)
                    {
                        c = r.Next(128 * 128);
                        if (tmp[c] == 0)
                        {
                            break;
                        }
                    }
                    a = c / 128;
                    b = c % 128;
                    water3[a][b] = water2[i][j];
                    tmp[c] = 1;
                }
            }

            return water3;
        }

        public virtual string extractText(Bitmap image)
        {
            Bits b = extractData(image);
            if (b == null)
            {
                return "";
            }
            return bits2String(b).Trim();
        }

        public virtual int BitBoxSize => bitBoxSize;

        public virtual int ByteLenErrorCorrection => byteLenErrorCorrection;

        public virtual int MaxBitsData => maxBitsData;

        public virtual int MaxBitsTotal => maxBitsTotal;

        public virtual int MaxTextLen => maxTextLen;

        public virtual double Opacity => opacity;

        public virtual long RandomizeEmbeddingSeed => randomizeEmbeddingSeed;

        public virtual long RandomizeWatermarkSeed => randomizeWatermarkSeed;

        private Bits string2Bits(string s)
        {
            Bits bits = new Bits();

            // remove invalid characters...
            s = s.ToLower();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (VALID_CHARS.IndexOf(c) < 0)
                {
                    s = s.Substring(0, i) + s.Substring(i + 1);
                    i--;
                }
            }

            // shorten if needed...
            if (s.Length > maxTextLen)
            {
                s = s.Substring(0, maxTextLen);
            }
            // padding if needed...
            while (s.Length < maxTextLen)
            {
                s += " ";
            }

            // create watermark bits...
            for (int j = 0; j < s.Length; j++)
            {
                bits.addValue(VALID_CHARS.IndexOf(s[j]), 6);
            }

            return bits;
        }

        public static double[] RGBtoHSB(Color color)
        {
            double[] hsb = new double[3];
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));
            hsb[0] = color.GetHue();
            hsb[1] = (max == 0) ? 0 : 1d - (1d * min / max);
            hsb[2] = max / 255d;
            return hsb;
        }

        public static Color HSBtoRGB(double hue, double saturation, double value)
        {
            int hi = (int)(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = (int)(value);
            int p = (int)(value * (1 - saturation));
            int q = (int)(value * (1 - f * saturation));
            int t = (int)(value * (1 - (1 - f) * saturation));

            if (hi == 0)
            {
                return Color.FromArgb(255, v, t, p);
            }
            else if (hi == 1)
            {
                return Color.FromArgb(255, q, v, p);
            }
            else if (hi == 2)
            {
                return Color.FromArgb(255, p, v, t);
            }
            else if (hi == 3)
            {
                return Color.FromArgb(255, p, q, v);
            }
            else if (hi == 4)
            {
                return Color.FromArgb(255, t, p, v);
            }
            else
            {
                return Color.FromArgb(255, v, p, q);
            }
        }

        public void Stop()
        {
            _stopped = true;
        }

    }

}