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
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

namespace Transcriptor
{
    class Symbol
    {
        int id;
        double xCordinate, yCordinate;
        double probability = 100;
        Rectangle rectangle = new Rectangle();
        char letter;
        BitmapSource image = new BitmapImage();

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
            get { return id; }
            set { this.id = value ; }
        }

        public double Probability
        {
            get { return probability; }
            set { this.probability = value; }
        }

        public double X
        {
            get { return xCordinate; }
            set { this.xCordinate = value; }
        }

        public double Y
        {
            get { return yCordinate; }
            set { this.yCordinate = value; }
        }

        public Rectangle Rectangle
        {
            get { return rectangle; }
            set { this.rectangle = value; }
        }

        public char Letter
        {
            get { return letter; }
            set { this.letter = value; }
        }

        public BitmapSource Image
        {
            get { return image; }
            set { this.image = value; }
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
            var idBytes = BitConverter.GetBytes(symbol.id);
            var xCordinateBytes = BitConverter.GetBytes(symbol.xCordinate);
            var yCordinateBytes = BitConverter.GetBytes(symbol.yCordinate);
            var imageBytes = new byte[0];
            if (serializeImage)
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(symbol.Image));
                using (var stream = new MemoryStream())
                {
                    encoder.Frames.Add(BitmapFrame.Create(symbol.Image));
                    encoder.Save(stream);
                    imageBytes = stream.ToArray();
                    stream.Close();
                }                
            }
            var imageSizeBytes = BitConverter.GetBytes(imageBytes.Length);
            var charBytes = BitConverter.GetBytes(symbol.letter);
            var charSizeBytes = BitConverter.GetBytes(charBytes.Length);
            var rectangleWidthBytes = BitConverter.GetBytes(symbol.rectangle.Width);
            var rectangleHeightBytes = BitConverter.GetBytes(symbol.rectangle.Height);
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
            var data = new byte[4+8+8+4+imageBytes.Length+4+charBytes.Length+8+8];
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
            var symbol = new Symbol(0,char.MinValue,null);
            symbol.Id = BitConverter.ToInt32(data, 0);
            symbol.xCordinate = BitConverter.ToDouble(data, 4);
            symbol.yCordinate = BitConverter.ToDouble(data, 12);
            var imageSize = BitConverter.ToInt32(data, 20);
            var imageBytes = new byte[imageSize];
            Array.Copy(data, 24, imageBytes, 0, imageSize);
            var stream = new MemoryStream(imageBytes);
            if (imageBytes.Length != 0)
            {
                stream.Seek(0, SeekOrigin.Begin);
                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
                symbol.Image = image;
            }
            var charsize = BitConverter.ToInt32(data, 24 + imageBytes.Length);
            var chararray = new byte[charsize];
            Array.Copy(data, 24 + imageBytes.Length + 4, chararray, 0, charsize);
            symbol.Letter = BitConverter.ToChar(chararray, 0);
            var rectangle = new Rectangle
            {
                Width = BitConverter.ToDouble(data, 24 + imageBytes.Length + 4 + chararray.Length),
                Height = BitConverter.ToDouble(data, 24 + imageBytes.Length + 4 + chararray.Length + 8)
            };
            symbol.Rectangle = rectangle;
            return symbol;
        }
    }
}
