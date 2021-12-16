using LatticeCrypto.Models;
using LatticeCrypto.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LatticeCrypto.Utilities
{
    public static class Util
    {
        public static double doubleMaxSize = Math.Pow(10, 8);
        private static readonly List<char> chars = new List<char> { '-', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ',', ';', ' ', '\t' };

        public static bool IsCharFigureOrSeparator(char c)
        {
            return chars.Contains(c);
        }

        public static void Swap<T>(ref T left, ref T right)
        {
            T temp = left;
            left = right;
            right = temp;
        }

        public static string FormatBigInt(BigInteger bigInt)
        {
            if (bigInt.ToString().TrimStart('-').Length == 1)
            {
                return bigInt.ToString();
            }

            return bigInt.ToString().TrimStart('-').Length > 8 ? string.Format("{0:e}", bigInt) : string.Format("{0:0,0}", bigInt);
        }

        public static string FormatDoubleGUI(double d)
        {
            return Math.Abs(d) > doubleMaxSize ? string.Format("{0:e}", d) : string.Format("{0:n}", d);
        }

        public static string FormatDoubleLog(double d)
        {
            return string.Format(string.Format("{{0:f{0}}}", Settings.Default.maxDecimalPlaces), d);
        }

        public static string FormatDoubleToPercentageLog(double d)
        {
            return string.Format(string.Format("{{0:f{0}}}", Settings.Default.maxDecimalPlaces), d * 100) + "%";
        }

        public static BigInteger Sum(List<BigInteger> bigInts)
        {
            BigInteger sum = 0;
            bigInts.ForEach(x => sum += x);
            return sum;
        }

        public static BigInteger ComputeRandomBigInt(BigInteger min, BigInteger max)
        {
            BigInteger temp;
            int maxByteLength = Math.Max(min.ToByteArray().Length, max.ToByteArray().Length);
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] bytes = new byte[maxByteLength];

            do
            {
                rng.GetBytes(bytes);
                temp = new BigInteger(bytes);
            }
            while (temp < min || temp > max);

            return temp;
        }

        public static void CreateSaveBitmap(FrameworkElement control)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog { Filter = "png files (*.png)|*.png", FileName = "screenshot" };
            if (saveFileDialog.ShowDialog() == false)
            {
                return;
            }

            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)control.ActualWidth, (int)control.ActualHeight, 96d, 96d, PixelFormats.Pbgra32);
            // needed otherwise the image output is black
            control.Measure(new Size((int)control.ActualWidth, (int)control.ActualHeight));
            control.Arrange(new Rect(new Size((int)control.ActualWidth, (int)control.ActualHeight)));

            renderBitmap.Render(control);

            //JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using (FileStream file = File.Create(saveFileDialog.FileName))
            {
                encoder.Save(file);
            }
        }

        public static VectorND ConvertStringToVectorND(string str)
        {
            string adjustedLine = str.Where(IsCharFigureOrSeparator).Aggregate("", (current, c) => current + c).Trim(' ');

            adjustedLine = adjustedLine.Replace(';', ',');
            adjustedLine = adjustedLine.Replace(' ', ',');
            adjustedLine = adjustedLine.Replace('\t', ',');
            adjustedLine = adjustedLine.Replace(",,", ",");

            string[] splittedLine = adjustedLine.Split(',');

            VectorND vector = new VectorND(splittedLine.Length);
            for (int i = 0; i < splittedLine.Length; i++)
            {
                vector.values[i] = BigInteger.Parse(splittedLine[i]);
            }

            return vector;
        }

        public static LatticeND ConvertStringToLatticeND(string str)
        {
            string adjustedLine;
            if (str.Contains(Environment.NewLine))
            {
                adjustedLine = str.Remove(str.IndexOf(Environment.NewLine, StringComparison.Ordinal));
            }
            else if (str.Contains(" "))
            {
                adjustedLine = str.Replace(" ", "");
            }
            else
            {
                adjustedLine = str;
            }

            if (adjustedLine.StartsWith(Languages.labelLatticeBasis))
            {
                adjustedLine = adjustedLine.Substring(Languages.labelLatticeBasis.Length + 1);
            }

            if (!IsCharFigureOrSeparator(adjustedLine[1]))
            {
                adjustedLine = adjustedLine.Substring(1);
                adjustedLine = adjustedLine.Remove(adjustedLine.Length - 1);
            }

            int columns = adjustedLine.Count(t => t == adjustedLine[0]);

            adjustedLine = adjustedLine.Where(IsCharFigureOrSeparator).Aggregate("", (current, c) => current + c).Trim(' ');

            if (adjustedLine.Contains(";"))
            {
                adjustedLine = adjustedLine.Replace(';', ',');
            }
            else if (adjustedLine.Contains(" "))
            {
                adjustedLine = adjustedLine.Replace(' ', ',');
            }
            else if (adjustedLine.Contains("\t"))
            {
                adjustedLine = adjustedLine.Replace('\t', ',');
            }

            string[] splittedLine = adjustedLine.Split(',');

            int rows = splittedLine.Length / columns;

            VectorND[] vectors = new VectorND[columns];
            for (int i = 0; i < columns; i++)
            {
                vectors[i] = new VectorND(rows);
                for (int j = 0; j < rows; j++)
                {
                    vectors[i].values[j] = BigInteger.Parse(splittedLine[rows * i + j]);
                }
            }
            return new LatticeND(vectors, false);
        }

        /// <summary>
        ///   Generates normally distributed numbers. Each operation makes two Gaussians for the price of one, and apparently they can be cached or something for better performance, but who cares.
        /// </summary>
        /// <param name="r"></param>
        /// <param name = "mu">Mean of the distribution</param>
        /// <param name = "sigma">Standard deviation</param>
        /// <returns></returns>
        public static double NextGaussian(this Random r, double mu = 0, double sigma = 1)
        {
            double u1 = r.NextDouble();
            double u2 = r.NextDouble();

            double rand_std_normal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

            double rand_normal = mu + sigma * rand_std_normal;

            return rand_normal;
        }

        public static int CountChar(string s, char search)
        {
            return s.Count(t => t == search);
        }
    }
}
