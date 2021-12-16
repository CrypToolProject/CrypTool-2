/*
   Copyright 2014 Olga Groh

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
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Transcriptor
{
    internal class Symbol
    {
        private int id;
        private double xCordinate, yCordinate;
        private double probability = 100;
        private Rectangle rectangle = new Rectangle();
        private char letter;
        private BitmapSource image = new BitmapImage();

        # region Constructor
        public Symbol(int symbolId, char symbolLetter, BitmapSource symbolImage)
        {
            id = symbolId;
            letter = symbolLetter;
            image = symbolImage;
        }
        #endregion

        # region Get/Set
        public int Id
        {
            get => id;
            set => id = value;
        }

        public double Probability
        {
            get => probability;
            set => probability = value;
        }

        public double X
        {
            get => xCordinate;
            set => xCordinate = value;
        }

        public double Y
        {
            get => yCordinate;
            set => yCordinate = value;
        }

        public Rectangle Rectangle
        {
            get => rectangle;
            set => rectangle = value;
        }

        public char Letter
        {
            get => letter;
            set => letter = value;
        }

        public BitmapSource Image
        {
            get => image;
            set => image = value;
        }
        #endregion

        public override string ToString()
        {
            return string.Format("{0}", Letter);
        }

        /// <summary>
        /// Serializes a given symbol
        /// </summary>
        /// <returns></returns>
        public static byte[] Serialize(Symbol symbol, bool serializeImage = true)
        {
            //serialize all needed members
            byte[] idBytes = BitConverter.GetBytes(symbol.id);
            byte[] xCordinateBytes = BitConverter.GetBytes(symbol.xCordinate);
            byte[] yCordinateBytes = BitConverter.GetBytes(symbol.yCordinate);
            byte[] imageBytes = new byte[0];
            if (serializeImage)
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(symbol.Image));
                using (MemoryStream stream = new MemoryStream())
                {
                    encoder.Frames.Add(BitmapFrame.Create(symbol.Image));
                    encoder.Save(stream);
                    imageBytes = stream.ToArray();
                    stream.Close();
                }
            }
            byte[] imageSizeBytes = BitConverter.GetBytes(imageBytes.Length);
            byte[] charBytes = BitConverter.GetBytes(symbol.letter);
            byte[] charSizeBytes = BitConverter.GetBytes(charBytes.Length);
            byte[] rectangleWidthBytes = BitConverter.GetBytes(symbol.rectangle.Width);
            byte[] rectangleHeightBytes = BitConverter.GetBytes(symbol.rectangle.Height);
            // Serialized "Symbol" byte array structure:
            // id               integer     4 bytes
            // xCoordinate      double      8 bytes
            // yCoordinate      double      8 bytes
            // imageSize        int         4 bytes
            // image            byte[]      imageSize bytes
            // charSize         int         4 bytes
            // char             byte[]      charSize bytes
            // rectangleWidth   double      8 bytes
            // rectangleHeight  double      8 bytes            
            //generate byte array for data
            byte[] data = new byte[4 + 8 + 8 + 4 + imageBytes.Length + 4 + charBytes.Length + 8 + 8];
            //copy everything to our byte array
            Array.Copy(idBytes, 0, data, 0, 4);
            Array.Copy(xCordinateBytes, 0, data, 4, 8);
            Array.Copy(yCordinateBytes, 0, data, 12, 8);
            Array.Copy(imageSizeBytes, 0, data, 20, 4);
            Array.Copy(imageBytes, 0, data, 24, imageBytes.Length);
            Array.Copy(charSizeBytes, 0, data, 24 + imageBytes.Length, 4);
            Array.Copy(charBytes, 0, data, 24 + imageBytes.Length + 4, charBytes.Length);
            Array.Copy(rectangleWidthBytes, 0, data, 24 + imageBytes.Length + 4 + charBytes.Length, 8);
            Array.Copy(rectangleHeightBytes, 0, data, 24 + imageBytes.Length + 4 + charBytes.Length + 8, 8);
            return data;
        }

        /// <summary>
        /// Deserializes a given Symbol
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Symbol Deserialize(byte[] data)
        {
            Symbol symbol = new Symbol(0, char.MinValue, null)
            {
                Id = BitConverter.ToInt32(data, 0),
                xCordinate = BitConverter.ToDouble(data, 4),
                yCordinate = BitConverter.ToDouble(data, 12)
            };
            int imageSize = BitConverter.ToInt32(data, 20);
            byte[] imageBytes = new byte[imageSize];
            Array.Copy(data, 24, imageBytes, 0, imageSize);
            MemoryStream stream = new MemoryStream(imageBytes);
            if (imageBytes.Length != 0)
            {
                stream.Seek(0, SeekOrigin.Begin);
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
                symbol.Image = image;
            }
            int charsize = BitConverter.ToInt32(data, 24 + imageBytes.Length);
            byte[] chararray = new byte[charsize];
            Array.Copy(data, 24 + imageBytes.Length + 4, chararray, 0, charsize);
            symbol.Letter = BitConverter.ToChar(chararray, 0);
            Rectangle rectangle = new Rectangle
            {
                Width = BitConverter.ToDouble(data, 24 + imageBytes.Length + 4 + chararray.Length),
                Height = BitConverter.ToDouble(data, 24 + imageBytes.Length + 4 + chararray.Length + 8)
            };
            symbol.Rectangle = rectangle;
            return symbol;
        }
    }
}
