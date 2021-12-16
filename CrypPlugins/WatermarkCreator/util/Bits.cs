using System;
using System.Collections.Generic;
using System.Text;

/* Original Project can be found at https://code.google.com/p/dct-watermark/
* Ported to C# to be used within CrypTool 2 by Nils Rehwald
* Thanks to cgaffa, ZXing and everyone else who worked on the original Project for making the original Java sources available publicly
* Thanks to Nils Kopal for Support and Bugfixing 
* 
* Copyright 2012 by Christoph Gaffga licensed under the Apache License, Version 2.0 (the "License"); you may not use
* this file except in compliance with the License. You may obtain a copy of the License at
* http://www.apache.org/licenses/LICENSE-2.0 Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
* either express or implied. See the License for the specific language governing permissions and limitations under the
* License.
*/

namespace net.util
{


    using GenericGF = com.google.zxing.common.reedsolomon.GenericGF;
    using ReedSolomonDecoder = com.google.zxing.common.reedsolomon.ReedSolomonDecoder;
    using ReedSolomonEncoder = com.google.zxing.common.reedsolomon.ReedSolomonEncoder;

    /// <summary>
	/// Some helper to work with an array of bits.
	/// 
	/// @author Christoph Gaffga
    /// @author Ported to C# by Nils Rehwald
	/// </summary>
	public class Bits
    {

        /// <summary>
        /// Unzip the bits </summary>
        public static Bits bitsGZIPDecode(Bits bits)
        {
            sbyte[] sdata = bits.Data;
            byte[] bdata = (byte[])(Array)sdata;
            System.IO.MemoryStream byteIn = new System.IO.MemoryStream(bdata);
            System.IO.Compression.GZipStream zipIn = new System.IO.Compression.GZipStream(byteIn, System.IO.Compression.CompressionMode.Compress);
            int b;
            Bits result = new Bits();
            while ((b = zipIn.ReadByte()) >= 0)
            {
                result.addValue(b, 8);
            }
            return result;
        }

        /// <summary>
        /// Zip the bits </summary>
        public static Bits bitsGZIPEncode(Bits bits)
        {
            try
            {
                System.IO.MemoryStream byteOut = new System.IO.MemoryStream();
                System.IO.Compression.GZipStream zipOut = new System.IO.Compression.GZipStream(byteOut, System.IO.Compression.CompressionMode.Decompress);
                byte[] tempBits = (byte[])(Array)bits.Data;
                zipOut.Write(tempBits, 0, bits.Data.Length);
                zipOut.Close();
                Bits result = new Bits();
                byte[] bdata = byteOut.ToArray();
                sbyte[] sdata = (sbyte[])(Array)bdata;
                result.addData(sdata);
                return result;
            }
            catch (System.IO.IOException)
            {
                //Console.WriteLine(e.GetType().Name);
                return null;
            }
        }

        /// <summary>
        /// Decode using Reed-Solomon error correction (with n bytes at the end of bits). </summary>
        public static Bits bitsReedSolomonDecode(Bits bits, int n)
        {
            int[] tmpData = (new Bits(bits.getBits(0, bits.size() - n * 8))).Bytes;
            int[] data = new int[tmpData.Length + n];
            Array.Copy(tmpData, data, tmpData.Length);
            for (int i = 0; i < n; i++)
            {
                data[data.Length - n + i] = (int)bits.getValue(bits.size() - n * 8 + i * 8, 8);
            }
            ReedSolomonDecoder dec = new ReedSolomonDecoder(GenericGF.QR_CODE_FIELD_256);
            dec.decode(data, n);
            Bits result = new Bits();
            Array.Copy(data, 0, data, 0, data.Length - n);
            result.addBytes(data);
            return result;
        }

        /// <summary>
        /// Encode using Reed-Solomon error correction (with n bytes added to the end of bits). </summary>
        public static Bits bitsReedSolomonEncode(Bits bits, int n)
        {
            int tmpSize = (bits.size() + 7) / 8 + n;
            int[] data = new int[tmpSize];
            if (tmpSize > bits.Bytes.Length)
            {
                tmpSize = bits.Bytes.Length;
            }
            Array.Copy(bits.Bytes, 0, data, 0, tmpSize);

            ReedSolomonEncoder enc = new ReedSolomonEncoder(GenericGF.QR_CODE_FIELD_256);
            enc.encode(data, n);

            Bits result = new Bits(bits);
            for (int i = data.Length - n; i < data.Length; i++)
            {
                result.addValue(data[i], 8);
            }
            return result;
        }

        /// <summary>
        /// Internal array with bits. </summary>
        private readonly IList<bool?> bits;

        /// <summary>
        /// The read-counter for pop-methods. </summary>
        private int readPosition = 0;

        public Bits()
        {
            bits = new List<bool?>();
        }

        public Bits(Bits bits)
        {
            this.bits = new List<bool?>(bits.bits);
        }


        public Bits(ICollection<bool?> bits)
        {
            this.bits = new List<bool?>(bits);
        }

        public virtual void addBit(bool bit)
        {
            bits.Add(bit);
        }

        public virtual void addBits(bool[] bits)
        {
            foreach (bool bit in bits)
            {
                addBit(bit);
            }
        }

        public virtual void addBits(ICollection<bool?> bits)
        {
            foreach (bool bit in bits)
            {
                addBit(bit);
            }
        }

        public virtual void addBytes(int[] bytes)
        {
            addBytes(bytes, bytes.Length);
        }

        public virtual void addBytes(int[] bytes, int len)
        {
            for (int i = 0; i < len; i++)
            {
                int bit = 0x01;
                for (int j = 0; j < 8; j++)
                {
                    addBit((bytes[i] & bit) > 0);
                    bit <<= 1;
                }
            }
        }

        public virtual void addData(sbyte[] data)
        {
            addData(data, data.Length);
        }

        public virtual void addData(sbyte[] data, int len)
        {
            for (int i = 0; i < len; i++)
            {
                int bit = 0x01;
                for (int j = 0; j < 8; j++)
                {
                    addBit((data[i] & bit) > 0);
                    bit <<= 1;
                }
            }
        }

        public virtual void addValue(long bits, int len)
        {
            long bit = 0x01;
            for (int i = 0; i < len; i++)
            {
                addBit((bits & bit) > 0);
                bit <<= 1;
            }
        }

        public virtual bool getBit(int index)
        {
            return (bool)bits[index];
        }

        public virtual IList<bool?> getBits()
        {
            return bits;
        }

        /// <summary>
        /// Return a sublist of the bits.
        /// </summary>
        /// <param name="fromIndex"> Start position, inclusive. </param>
        /// <param name="toIndex"> End position, exclusive. </param>
        /// <returns> Bits for the specified range. </returns>

        public virtual IList<bool?> getBits(int fromIndex, int toIndex)
        {
            IList<bool?> returns = bits;
            for (int i = 0; i < fromIndex; i++)
            {
                returns.RemoveAt(i);
            }
            for (int i = toIndex; i < returns.Count; i++)
            {
                returns.RemoveAt(i);
            }
            return returns;
        }

        public virtual int[] Bytes
        {
            get
            {
                int[] bytes = new int[(bits.Count + 7) / 8];
                for (int i = 0; i < bytes.Length; i++)
                {
                    int bit = 0x01;
                    for (int j = 0; j < 8 && i * 8 + j < bits.Count; j++)
                    {
                        if ((bool)bits[i * 8 + j])
                        {
                            bytes[i] |= bit;
                        }
                        bit <<= 1;
                    }
                }
                return bytes;
            }
        }

        public virtual sbyte[] Data
        {
            get
            {
                sbyte[] data = new sbyte[(bits.Count + 7) / 8];
                for (int i = 0; i < data.Length; i++)
                {
                    int bit = 0x01;
                    for (int j = 0; j < 8 && i * 8 + j < bits.Count; j++)
                    {
                        if ((bool)bits[i * 8 + j])
                        {
                            data[i] |= (sbyte)bit;
                        }
                        bit <<= 1;
                    }
                }
                return data;
            }
        }

        public virtual long getValue(int index, int len)
        {
            long result = 0;
            long bit = 0x01;
            for (int i = index; i < index + len; i++)
            {
                if ((bool)bits[i])
                {
                    result |= bit;
                }
                bit <<= 1;
            }
            return result;
        }

        public virtual bool hasNext()
        {
            return readPosition < bits.Count;
        }



        public virtual bool hasNext(int len)
        {
            return readPosition + len < bits.Count + 1;
        }

        public virtual bool popBit()
        {
            return getBit(readPosition++);
        }


        public virtual IList<bool?> popBits(int len)
        {
            readPosition += len;
            return getBits(readPosition - len, readPosition);
        }

        public virtual long popValue(int len)
        {
            readPosition += len;
            return getValue(readPosition - len, len);
        }

        public virtual void reset()
        {
            bits.Clear();
            readPosition = 0;
        }

        public virtual void setBit(int index, bool bit)
        {
            bits[index] = bit;
        }

        public virtual int size()
        {
            return bits.Count;
        }

        public override string ToString()
        {

            StringBuilder buf = new StringBuilder(bits.Count);
            for (int i = 0; i < bits.Count; i++)
            {
                buf.Append((bool)bits[i] ? '1' : '0');
            }
            return buf.ToString();
        }

    }

}