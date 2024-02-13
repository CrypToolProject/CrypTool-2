/*                              
   Copyright 2024 Nils Kopal, CrypTool Team

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
using System.IO.Compression;
using System.IO;
using System.Linq;

namespace LanguageStatisticsLib
{
    public class LanguageStatisticsFile
    {
        private readonly string filePath;

        /// <summary>
        /// Magic number (ASCII string 'CTLS') for this file format.
        /// </summary>
        public const uint FileFormatMagicNumber = 'C' + ('T' << 8) + ('L' << 16) + ('S' << 24);

        public string Alphabet { get; private set; }

        public string LanguageCode { get; private set; }

        public LanguageStatisticsFile(string filePath)
        {
            this.filePath = filePath;
        }

        public Array LoadFrequencies(int arrayDimensions)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (GZipStream gz = new GZipStream(fs, CompressionMode.Decompress))
            using (BinaryReader br = new BinaryReader(gz))
            {
                uint magicNumber = br.ReadUInt32();
                if (magicNumber != FileFormatMagicNumber)
                {
                    throw new Exception("File does not start with the expected magic number for language statistics.");
                }

                //read the stored language code, e.g. EN for English
                LanguageCode = br.ReadString();

                int gramLength = br.ReadInt32();
                if (gramLength != arrayDimensions)
                {
                    throw new Exception("Gram length of statistics file differs from required dimensions of frequency array.");
                }

                Alphabet = br.ReadString();
                int alphabetLength = Alphabet.Length;

                int frequencyEntries = 1;
                //frequencyEntries = exp(alphabetLength, gramLength) 
                for (int i = 0; i < gramLength; i++)
                {
                    frequencyEntries *= alphabetLength;
                }

                //Instantiate array with "arrayDimensions" dimensions of length "alphabetLength":
                int[] arrayLengths = Enumerable.Repeat(alphabetLength, arrayDimensions).ToArray();
                Array frequencyArray = Array.CreateInstance(typeof(float), arrayLengths);

                //Read whole block of frequency floats and do a block copy for efficiency reasons:
                byte[] frequencyData = br.ReadBytes(sizeof(float) * frequencyEntries);
                Buffer.BlockCopy(frequencyData, 0, frequencyArray, 0, frequencyData.Length);
                return frequencyArray;
            }
        }
    }

}
