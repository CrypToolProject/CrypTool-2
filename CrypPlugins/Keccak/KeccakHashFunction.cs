#define _DEBUG_

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace CrypTool.Plugins.Keccak
{
    public static class KeccakHashFunction
    {

        public static byte[] Hash(byte[] input, int outputLength, int rate, int capacity, ref KeccakPres pres, Keccak plugin, KeccakSettings settings, StreamWriter debugWriter)
        {
#if _DEBUG_
            debugWriter.WriteLine("#Keccak: running Keccak with the following parameters:");
            debugWriter.WriteLine(string.Format("#Keccak: {0}: {1} bit", "output length", outputLength));
            debugWriter.WriteLine(string.Format("#Keccak: {0}: {1} bit", "state size", rate + capacity));
            debugWriter.WriteLine(string.Format("#Keccak: {0}: {1} bit", "bit rate", rate));
            debugWriter.WriteLine(string.Format("#Keccak: {0}: {1} bit", "capacity", capacity));
            debugWriter.WriteLine();
#endif


            /* map each bit of the input to a byte */
            byte[] inputInBitsWithoutSuffix = ByteArrayToBitArray(input, plugin);

            /* append domain separation suffix bits */
            byte[] inputInBits = appendSuffixBits(settings.SuffixBits, inputInBitsWithoutSuffix);

            /* for presentation: estimate number of keccak-f executions */
            int progressionSteps = (int)Math.Ceiling((double)(inputInBits.Length + 8) / rate) + ((int)Math.Ceiling((double)outputLength / rate) - 1);

            /* create sponge instance */
            Sponge sponge = new Sponge(rate, capacity, ref pres, plugin, progressionSteps, debugWriter);

            /* absorb input */
            sponge.Absorb(inputInBits);

            /* squeeze sponge to obtain output */
            Debug.Assert(outputLength % 8 == 0);
            byte[] outputInBits = sponge.Squeeze(outputLength);

            /* reverse 'bit to byte' mapping */
            byte[] output = BitArrayToByteArray(outputInBits);

#if _DEBUG_
            debugWriter.WriteLine("#Keccak: successfully hashed {0} input bits to {1} output bits!", inputInBits.Length, outputInBits.Length);
            debugWriter.WriteLine("#Keccak: all work is done!");
#endif


            return output;
        }

        private static byte[] appendSuffixBits(string suffixBits, byte[] inputInBitsWithoutSuffix)
        {
            if (suffixBits.Length == 0)
            {
                return inputInBitsWithoutSuffix;
            }

            int newSize = inputInBitsWithoutSuffix.Length + suffixBits.Length;
            byte[] inputInBits = new byte[newSize];

            if (inputInBitsWithoutSuffix.Length > 0)
            {
                Array.Copy(inputInBitsWithoutSuffix, inputInBits, inputInBitsWithoutSuffix.Length);
            }

            char[] suffixBitsArray = suffixBits.ToCharArray();
            for (int i = 0; i < suffixBitsArray.Length; i++)
            {
                byte b = suffixBitsArray[i] == '1' ? (byte)0x01 : (byte)0x00;
                inputInBits[inputInBitsWithoutSuffix.Length + i] = b;
            }
            return inputInBits;
        }

        #region helper methods

        public static byte[] SubArray(byte[] data, int index, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static string ByteArrayToBase64(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        public static string ByteArrayToIntString(byte[] bytes)
        {
            StringBuilder hex = new StringBuilder(bytes.Length);
            foreach (byte b in bytes)
            {
                hex.Append((int)b);
            }
            return hex.ToString();
        }

        public static void PrintByteArray(byte[] bytes, StreamWriter debugWriter)
        {
            StringBuilder hex = new StringBuilder(bytes.Length * 2);

            foreach (byte b in bytes)
            {
                hex.AppendFormat("{0:x2} ", b);
            }
            debugWriter.WriteLine(hex.ToString());
            debugWriter.WriteLine(" - " + bytes.Length + " bytes");
        }

        public static void PrintBits(byte[] bytes, StreamWriter debugWriter)
        {
            string hexStr = ByteArrayToIntString(bytes);
            debugWriter.WriteLine(hexStr + " - " + hexStr.Length + " bits");
        }

        public static void PrintBits(byte[] bytes, int laneSize, StreamWriter debugWriter)
        {
            /* only print bit state if lane size is small enough*/
            if (laneSize >= 16)
            {
                return;
            }

            string hexStr = "";
            StringBuilder hex = new StringBuilder(bytes.Length);
            int j = 0;

            foreach (byte b in bytes)
            {
                if (j % laneSize == 0)
                {
                    hex.AppendFormat("{0:00}: ", j / laneSize);
                }

                hex.Append((int)b);

                j++;
                if (j % laneSize == 0)
                {
                    hex.Append(Environment.NewLine);
                }
            }

            hexStr = hex.ToString();
            hex.Clear();
            debugWriter.WriteLine(hexStr); // + " - " + (hexStr.Length - (j / value)) + " bits");
        }

        public static void PrintBytes(byte[] bytes, int laneSize, StreamWriter debugWriter)
        {
            /* only print byte state if lane size is large enough*/
            if (laneSize < 16 && laneSize % 8 != 0)
            {
                return;
            }

            StringBuilder binaryBytes = new StringBuilder(bytes.Length);
            StringBuilder bitString = new StringBuilder(8);
            char[] bitChars = new char[8];

            for (int i = 0; i < bytes.Length; i += 8)
            {
                if (i % laneSize == 0)
                {
                    binaryBytes.AppendFormat(Environment.NewLine + "{0:00}: ", i / laneSize);
                }

                for (int j = 0; j < 8; j++)
                {
                    bitString.Append((int)bytes[i + j]);
                }
                for (int j = 0; j < 8; j++)
                {
                    bitChars[j] = bitString.ToString().ElementAt(8 - 1 - j);
                }

                binaryBytes.AppendFormat("{0:X2} ", Convert.ToByte(new string(bitChars), 2));
                bitString.Clear();

            }
            debugWriter.WriteLine(binaryBytes.ToString());
        }

        /** 
         * returns a hex string presentation of the byte array `bytes`
         * the parameter `laneSize` determines after how many bytes a line break is inserted           
         */
        public static string GetByteArrayAsString(byte[] bytes, int laneSize, StreamWriter debugWriter)
        {
            /* get bit state if lane size is small */
            if (laneSize < 16) // && laneSize % 8 != 0)
            {
                string hexStr = "";
                StringBuilder hex = new StringBuilder(bytes.Length);
                int j = 0;

                foreach (byte b in bytes)
                {
                    //if (j % laneSize == 0)
                    //{
                    //    hex.AppendFormat("{0:00}: ", j / laneSize);
                    //}

                    hex.Append((int)b);

                    j++;
                    if (j % laneSize == 0)
                    {
                        hex.Append(Environment.NewLine);
                    }
                }

                hexStr = hex.ToString();
                hex.Clear();

                return hexStr;
            }
            /* get byte presentation of state otherwise (lane size at least 2 bytes) */
            else
            {
                StringBuilder binaryBytes = new StringBuilder(bytes.Length);
                StringBuilder bitString = new StringBuilder(8);
                char[] bitChars = new char[8];

                for (int i = 0; i < bytes.Length; i += 8)
                {
                    /* append line break at the end of a lane */
                    if (i != 0 && i % laneSize == 0)
                    {
                        binaryBytes.Append(Environment.NewLine);
                    }

                    for (int j = 0; j < 8; j++)
                    {
                        bitString.Append((int)bytes[i + j]);
                    }
                    for (int j = 0; j < 8; j++)
                    {
                        bitChars[j] = bitString.ToString().ElementAt(8 - 1 - j);
                    }

                    binaryBytes.AppendFormat("{0:X2} ", Convert.ToByte(new string(bitChars), 2));
                    debugWriter.WriteLine(new string(bitChars));
                    bitString.Clear();
                }

                return binaryBytes.ToString();
            }
        }

        /**
         * Converts a byte array to an another byte array. The returned byte array contains the bit representation of the input byte array
         * where each byte represents a bit of the input byte array
         * */
        public static byte[] ByteArrayToBitArray(byte[] bytes, Keccak plugin = null)
        {
            List<byte> bitsInBytes = new List<byte>(bytes.Length * 8);
            foreach (byte b in bytes)
            {
                byte byt = b;
                for (int i = 0; i < 8; i++)
                {
                    if ((byt & 0x01) > 0)
                    {
                        bitsInBytes.Add(1);
                    }
                    else
                    {
                        bitsInBytes.Add(0);
                    }

                    byt = (byte)(byt >> 1);
                }
            }
            return bitsInBytes.ToArray();
        }

        public static byte[] BitArrayToByteArray(byte[] bitsInBytes)
        {
            string c;
            char[] bitChars = new char[8];
            StringBuilder bitString = new StringBuilder(8);

            Debug.Assert(bitsInBytes.Length % 8 == 0);
            byte[] bytes = new byte[bitsInBytes.Length / 8];

            for (int i = 0; i < bytes.Length; i++)
            {
                bitString.Clear();
                for (int j = 0; j < 8; j++)
                {
                    c = bitsInBytes[i * 8 + j] == 0x01 ? "1" : "0";
                    bitString.Append(c);
                }

                /* swap back every bit to get the right order of bits in a byte */
                for (int k = 0; k < 8; k++)
                {
                    bitChars[k] = bitString.ToString().ElementAt(8 - 1 - k);
                }

                bytes[i] = Convert.ToByte(new string(bitChars), 2);
            }

            return bytes;
        }

        #endregion

    }
}
