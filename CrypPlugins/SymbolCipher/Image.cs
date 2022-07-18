/*
   Copyright 2022 Nils Kopal <Nils.Kopal<at>CrypTool.org

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
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace CrypTool.Plugins.SymbolCipher
{
    /// <summary>
    /// Image which can be drawn on using some comfort methods
    /// </summary>
    public class Image
    {
        /// <summary>
        /// Creates a new image
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public Image(int width, int height)
        {
            Width = width;
            Height = height;
            Data = new byte[Width * Height * 3]; // * 3 <= we have three channels: red, green, and blue
        }
        public int Width { get; }
        public int Height { get; }
        public byte[] Data { get; }

        /// <summary>
        /// Converts this Image to a Bitmap
        /// </summary>
        /// <returns></returns>
        public Bitmap ToBitmap()
        {
            Bitmap bitmap = new Bitmap(Width, Height);
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            IntPtr ptr = bitmapData.Scan0;
            Marshal.Copy(Data, 0, ptr, bitmapData.Stride * bitmap.Height);
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }

        /// <summary>
        /// Clears the Image using the defined color
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        public void ClearImage(byte red, byte green, byte blue)
        {
            for (int y = 0; y < Height; y++)
            {
                int offset = y * Width * 3;
                for (int x = 0; x < Width; x++)
                {
                    int address = offset + x * 3;
                    Data[address + 0] = blue;
                    Data[address + 1] = green;
                    Data[address + 2] = red;
                }
            }
        }

        /// <summary>
        /// Draws a line into the Bitmap using the defined coordinates and color
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="lineThickness"></param>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        public void DrawLine(int x0, int y0, int x1, int y1, int lineThickness, byte red, byte green, byte blue)
        {
            //code from https://de.wikipedia.org/wiki/Bresenham-Algorithmus
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -1 * Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy, e2;
            int width_3 = Width * 3;            

            if(lineThickness == 0)
            {
                lineThickness = 1;
            }

            //here we draw a line with thickness = 1 using standard Bresenham
            while (lineThickness == 1)
            {
                if (x0 >= 0 && x0 < Width && y0 >= 0 && y0 < Height)
                {
                    int address = y0 * width_3 + x0 * 3;
                    Data[address] = blue;
                    address++;
                    Data[address] = green;
                    address++;
                    Data[address] = red;
                }
                if (x0 == x1 && y0 == y1)
                {
                    break;
                }
                e2 = 2 * err;
                if (e2 > dy)
                {
                    err += dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }

            //here we draw a line using Bresenham with circles (slow but good enough!)
            //todo: replace with faster line drawing algorithm for faster image generation with 
            //      higher resolutions
            while (lineThickness > 1)
            {
                if (x0 >= 0 && x0 < Width && y0 >= 0 && y0 < Height)
                {
                    DrawFilledCircle(x0, y0, lineThickness, red, green, blue);
                }
                if (x0 == x1 && y0 == y1)
                {
                    break;
                }
                e2 = 2 * err;
                if (e2 > dy)
                {
                    err += dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        /// <summary>
        /// Draws a filled circle using the defined coordinates, radius, and color
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="radius"></param>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        public void DrawFilledCircle(int x, int y, int radius, byte red, byte green, byte blue)
        {
            int diameter = radius * radius;
            int width_3 = 3 * Width;
            if(radius < 1)
            {
                radius = 1;
            }
            for (int x2 = -radius; x2 < radius; x2++)
            {
                int xtest = x + x2;
                if (xtest < 0)
                {
                    continue;
                }
                if (xtest >= Width)
                {
                    break;
                }
                int height2 = (int)Math.Sqrt(diameter - x2 * x2);
                int x2_plus_x_3 = (x2 + x) * 3;
                for (int y2 = -height2; y2 < height2; y2++)
                {
                    int ytest = y + y2;
                    if (ytest < 0)
                    {
                        continue;
                    }
                    if (ytest >= Height)
                    {
                        break;
                    }
                    int address1 = (y2 + y) * width_3 + x2_plus_x_3;
                    Data[address1] = blue;
                    address1++;
                    Data[address1] = green;
                    address1++;
                    Data[address1] = red;
                }
            }
        }

        /// <summary>
        /// Draws a rotated ellipse using the defined coordinates, widht, height, radius, angle, and color
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="angle"></param>
        /// <param name="lineThickness"></param>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        public void DrawEllipse(int x, int y, double width, double height, double angle, int lineThickness, byte red, byte green, byte blue)
        {
            const double PI = 3.14159265;
            double sin_angle;
            double cos_angle;
            double theta;
            double dtheta;
            double x2;
            double y2;
            double rx;
            double ry;

            angle = angle * PI / 180.0;
            sin_angle = Math.Sin(angle);
            cos_angle = Math.Cos(angle);
            theta = 0;
            dtheta = 2 * PI / 20;

            while (theta <= 2 * PI)
            {
                x2 = width * Math.Cos(theta);
                y2 = height * Math.Sin(theta);
                rx = x + x2 * cos_angle + y2 * sin_angle;
                ry = y - x2 * sin_angle + y2 * cos_angle;
                DrawFilledCircle((int)rx, (int)ry, lineThickness, red, green, blue);
                theta += dtheta;
            }
        }

        /// <summary>
        /// Draws a rotated filled ellipse using the defined coordinates, widht, height, radius, angle, and color
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="angle"></param>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        public void DrawFilledEllipse(int x, int y, double width, double height, double angle, byte red, byte green, byte blue)
        {
            const double PI = 3.14159265;
            double sin_angle;
            double cos_angle;
            double theta;
            double dtheta;
            double x1, x2;
            double y1, y2;
            double rx1, rx2;
            double ry1, ry2;

            angle = angle * PI / 180.0;
            sin_angle = Math.Sin(angle);
            cos_angle = Math.Cos(angle);
            theta = 0;
            dtheta = 1 * PI / 20;

            while (theta < 2 * PI)
            {
                theta += dtheta;
                x1 = width * Math.Cos(theta);
                y1 = height * Math.Sin(theta);
                rx1 = x + x1 * cos_angle + y1 * sin_angle;
                ry1 = y - x1 * sin_angle + y1 * cos_angle;

                x2 = width * Math.Cos(theta + PI);
                y2 = height * Math.Sin(theta + PI);
                rx2 = x + x2 * cos_angle + y2 * sin_angle;
                ry2 = y - x2 * sin_angle + y2 * cos_angle;

                DrawLine((int)rx1, (int)ry1, (int)rx2, (int)ry2, 1, red, green, blue);
            }
        }
    }
}
