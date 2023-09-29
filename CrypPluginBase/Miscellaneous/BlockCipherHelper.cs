/*
   Copyright 2008 - 2022 CrypTool Team

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
using CrypTool.PluginBase.IO;
using System;

namespace CrypTool.PluginBase.Miscellaneous
{
    public static class BlockCipherHelper
    {
        public enum CipherAction
        {
            Encrypt,
            Decrypt
        }

        public enum BlockMode
        {
            ECB,
            CBC,
            CFB,
            OFB
        }

        public enum PaddingType
        {
            None,
            Zeros,
            PKCS7,
            ANSIX923,
            ISO10126,
            OneZeros

        };

        public delegate byte[] BlockCipher(byte[] block, byte[] key);
        public delegate void ProgressChanged(double value, double max);

        public static byte[] StreamToByteArray(ICrypToolStream stream)
        {
            byte[] buf = new byte[stream.Length];

            CStreamReader reader = stream.CreateReader();
            reader.WaitEof();
            reader.Seek(0, System.IO.SeekOrigin.Begin);
            reader.ReadFully(buf);
            reader.Close();

            return buf;
        }

        public static ICrypToolStream AppendPadding(ICrypToolStream input, PaddingType paddingtype, int blocksize)
        {
            return new CStreamWriter(AppendPadding(StreamToByteArray(input), paddingtype, blocksize));
        }

        public static byte[] AppendPadding(byte[] input, PaddingType paddingtype, int blocksize)
        {
            long l = blocksize - (input.Length % blocksize);

            if (paddingtype == PaddingType.None)
            {
                if (l % blocksize != 0)
                {
                    throw new Exception("Input must be a multiple of blocksize (" + blocksize + " bytes) if no padding is used.");
                }

                return input;
            }
            else if (paddingtype == PaddingType.Zeros)
            {
                l %= blocksize; // add no zeros if message length is multiple of blocksize
            }

            byte[] buf = new byte[input.Length + l];
            Array.Copy(input, buf, input.Length);

            switch (paddingtype)
            {
                case PaddingType.Zeros:
                    for (int i = 0; i < l; i++)
                    {
                        buf[input.Length + i] = 0x00;
                    }

                    break;

                case PaddingType.OneZeros:
                    buf[input.Length] = 0x01;
                    for (int i = 1; i < l; i++)
                    {
                        buf[input.Length + i] = 0x00;
                    }

                    break;

                case PaddingType.PKCS7:
                    for (int i = 0; i < l; i++)
                    {
                        buf[input.Length + i] = (byte)l;
                    }

                    break;

                case PaddingType.ISO10126:
                    Random random = new Random();
                    for (int i = 0; i < l; i++)
                    {
                        buf[input.Length + i] = (byte)random.Next(256);
                    }

                    buf[buf.Length - 1] = (byte)l;
                    break;

                case PaddingType.ANSIX923:
                    for (int i = 0; i < l; i++)
                    {
                        buf[input.Length + i] = 0;
                    }

                    buf[buf.Length - 1] = (byte)l;
                    break;
            }

            return buf;
        }

        public static int StripPadding(byte[] input, int bytesRead, PaddingType paddingtype, int blocksize)
        {
            //if (bytesRead != input.Length) throw new Exception("Unexpected size of padding");
            if (bytesRead % blocksize != 0)
            {
                throw new Exception("Unexpected blocksize (" + (bytesRead % blocksize) + " bytes) in padding (" + blocksize + " bytes expected)");
            }

            if (paddingtype == PaddingType.Zeros)   // ... | DD DD DD DD DD DD DD DD | DD DD DD DD 00 00 00 00 |
            {
                for (bytesRead--; bytesRead > 0; bytesRead--)
                {
                    if (input[bytesRead] != 0)
                    {
                        break;
                    }
                }

                bytesRead++;
                if (bytesRead == 0)
                {
                    throw new Exception("Error in Zeros padding");
                }
            }

            if (paddingtype == PaddingType.OneZeros)    // ... | DD DD DD DD DD DD DD DD | DD DD DD DD 01 00 00 00 |
            {
                for (bytesRead--; bytesRead > 0; bytesRead--)
                {
                    if (input[bytesRead] != 0)
                    {
                        break;
                    }
                }

                if (bytesRead < 0 || input[bytesRead] != 0x01)
                {
                    throw new Exception("Unexpected byte in 1-0 padding");
                }
            }

            if (paddingtype == PaddingType.PKCS7)   // ... | DD DD DD DD DD DD DD DD | DD DD DD DD 04 04 04 04 |
            {
                int l = input[input.Length - 1];
                if (l > blocksize)
                {
                    throw new Exception("Unexpected byte in PKCS7 padding");
                }

                for (int i = 1; i <= l; i++)
                {
                    if (input[input.Length - i] != l)
                    {
                        throw new Exception("Unexpected byte in PKCS7 padding");
                    }
                }

                bytesRead -= l;
            }

            if (paddingtype == PaddingType.ISO10126)    // ... | DD DD DD DD DD DD DD DD | DD DD DD DD 81 A6 23 04 |
            {
                int l = input[input.Length - 1];
                if (l > blocksize)
                {
                    throw new Exception("Unexpected byte in ISO10126 padding");
                }

                bytesRead -= l;
            }

            if (paddingtype == PaddingType.ANSIX923)    // ... | DD DD DD DD DD DD DD DD | DD DD DD DD 00 00 00 04 |
            {
                int l = input[input.Length - 1];
                if (l > blocksize)
                {
                    throw new Exception("Unexpected byte in ANSIX923 padding");
                }

                for (int i = 2; i <= l; i++)
                {
                    if (input[input.Length - i] != 0)
                    {
                        throw new Exception("Unexpected byte in ANSIX923 padding");
                    }
                }

                bytesRead -= l;
            }

            return bytesRead;
        }

        public static byte[] StripPadding(byte[] input, PaddingType paddingtype, int blocksize)
        {
            int validBytes = StripPadding(input, input.Length, paddingtype, blocksize);
            byte[] buf = new byte[validBytes];
            Array.Copy(input, buf, validBytes);
            return buf;
        }

        public static ICrypToolStream StripPadding(ICrypToolStream input, PaddingType paddingtype, int blocksize)
        {
            return new CStreamWriter(StripPadding(StreamToByteArray(input), paddingtype, blocksize));
        }

        /// <summary>
        /// Encrypts/Decrypts using ECB
        /// </summary>
        /// <param name="blockCipher"></param>
        /// <param name="cipherAction"></param>
        /// <param name="inputStream"></param>
        /// <param name="outputStream"></param>
        /// <param name="key"></param>
        /// <param name="padding"></param>
        /// <param name="stop"></param>
        /// <param name="progressChanged"></param>
        /// <param name="lastInputBlock"></param>
        /// <param name="blockSize"></param>
        public static void ExecuteECB(BlockCipher blockCipher,
                                CipherAction cipherAction,
                                ref ICrypToolStream inputStream,
                                ref ICrypToolStream outputStream,
                                byte[] key,
                                PaddingType padding,
                                ref bool stop,
                                ProgressChanged progressChanged,
                                ref byte[] lastInputBlock,
                                int blocksize = 8)
        {
            using (CStreamReader reader = inputStream.CreateReader())
            {
                using (CStreamWriter writer = new CStreamWriter())
                {
                    byte[] inputBlock = new byte[blocksize];
                    int readcount = 0;

                    while (reader.Position < reader.Length && !stop)
                    {
                        //we always try to read a complete block
                        readcount = 0;
                        while ((readcount += reader.Read(inputBlock, readcount, blocksize - readcount)) < blocksize &&
                               reader.Position < reader.Length && !stop)
                        {
                            ;
                        }

                        if (stop)
                        {
                            return;
                        }

                        //Show progress in UI
                        progressChanged(reader.Position, reader.Length);

                        byte[] outputblock = null;
                        //we read a complete block
                        if (readcount == blocksize)
                        {
                            outputblock = blockCipher(inputBlock, key);
                        }
                        //we read an incomplete block, thus, we are at the end of the stream
                        else if (readcount > 0)
                        {
                            byte[] block = new byte[blocksize];
                            Array.Copy(inputBlock, 0, block, 0, readcount);
                            outputblock = blockCipher(block, key);
                        }

                        //check if it is the last block and we decrypt, thus, we have to remove the padding
                        if (reader.Position == reader.Length && cipherAction == CipherAction.Decrypt && padding != PaddingType.None)
                        {
                            int valid = StripPadding(outputblock, blocksize, padding, blocksize);
                            if (valid != blocksize)
                            {
                                byte[] newoutputblock = new byte[valid];
                                Array.Copy(outputblock, 0, newoutputblock, 0, valid);
                                outputblock = newoutputblock;
                            }
                            else if (valid == 0)
                            {
                                outputblock = null;
                            }
                        }

                        //if we crypted something, we output it
                        if (outputblock != null)
                        {
                            writer.Write(outputblock, 0, outputblock.Length);
                            //if we wrote to the stream, we memorize the last input block for the visualization
                            lastInputBlock = inputBlock;
                        }
                    }

                    writer.Flush();
                    outputStream = writer;
                }
            }
        }

        /// <summary>
        /// Encrypts/Decrypts using CBC
        /// </summary>
        /// <param name="blockCipher"></param>
        /// <param name="cipherAction"></param>
        /// <param name="inputStream"></param>
        /// <param name="outputStream"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <param name="padding"></param>
        /// <param name="stop"></param>
        /// <param name="progressChanged"></param>
        /// <param name="lastInputBlock"></param>
        /// <param name="blocksize"></param>
        public static void ExecuteCBC(BlockCipher blockCipher,
                                CipherAction cipherAction,
                                ref ICrypToolStream inputStream,
                                ref ICrypToolStream outputStream,
                                byte[] key,
                                byte[] iv,
                                PaddingType padding,
                                ref bool stop,
                                ProgressChanged progressChanged,
                                ref byte[] lastInputBlock,
                                int blocksize = 8)
        {
            using (CStreamReader reader = inputStream.CreateReader())
            {
                using (CStreamWriter writer = new CStreamWriter())
                {

                    byte[] lastBlock = iv;
                    int readcount = 0;

                    while (reader.Position < reader.Length && !stop)
                    {
                        //we always try to read a complete block
                        byte[] inputBlock = new byte[blocksize];
                        readcount = 0;
                        while ((readcount += reader.Read(inputBlock, readcount, blocksize - readcount)) < blocksize &&
                               reader.Position < reader.Length && !stop)
                        {
                            ;
                        }

                        if (stop)
                        {
                            return;
                        }

                        //Show progress in UI
                        progressChanged(reader.Position, reader.Length);

                        byte[] outputblock = null;
                        //we read a complete block
                        if (readcount == blocksize)
                        {
                            //Compute XOR with lastblock for CBC mode
                            if (cipherAction == CipherAction.Encrypt)
                            {
                                inputBlock = XOR(inputBlock, lastBlock);
                                outputblock = blockCipher(inputBlock, key);
                                lastBlock = outputblock;
                            }
                            else
                            {
                                outputblock = blockCipher(inputBlock, key);
                                outputblock = XOR(outputblock, lastBlock);
                                lastBlock = inputBlock;
                            }
                        }
                        //we read an incomplete block, thus, we are at the end of the stream
                        else if (readcount > 0)
                        {
                            //Compute XOR with lastblock for CBC mode
                            if (cipherAction == CipherAction.Encrypt)
                            {
                                byte[] block = new byte[blocksize];
                                Array.Copy(inputBlock, 0, block, 0, readcount);
                                inputBlock = XOR(block, lastBlock);
                                outputblock = blockCipher(inputBlock, key);
                            }
                            else
                            {
                                outputblock = blockCipher(inputBlock, key);
                                outputblock = XOR(outputblock, lastBlock);
                            }
                        }

                        //check if it is the last block and we decrypt, thus, we have to remove the padding
                        if (reader.Position == reader.Length && cipherAction == CipherAction.Decrypt && padding != PaddingType.None)
                        {
                            int valid = StripPadding(outputblock, blocksize, padding, blocksize);
                            if (valid != blocksize)
                            {
                                byte[] newoutputblock = new byte[valid];
                                Array.Copy(outputblock, 0, newoutputblock, 0, valid);
                                outputblock = newoutputblock;
                            }
                            else if (valid == 0)
                            {
                                outputblock = null;
                            }
                        }

                        //if we crypted something, we output it
                        if (outputblock != null)
                        {
                            writer.Write(outputblock, 0, outputblock.Length);
                            //if we wrote to the stream, we memorize the last input block for the visualization
                            lastInputBlock = inputBlock;
                        }
                    }

                    writer.Flush();
                    outputStream = writer;
                }
            }
        }

        /// <summary>
        /// Encrypts/Decrypts using CFB
        /// </summary>
        /// <param name="blockCipher"></param>
        /// <param name="cipherAction"></param>
        /// <param name="inputStream"></param>
        /// <param name="outputStream"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <param name="padding"></param>
        /// <param name="stop"></param>
        /// <param name="progressChanged"></param>
        /// <param name="lastInputBlock"></param>
        /// <param name="blocksize"></param>
        public static void ExecuteCFB(BlockCipher blockCipher,
                                CipherAction cipherAction,
                                ref ICrypToolStream inputStream,
                                ref ICrypToolStream outputStream,
                                byte[] key,
                                byte[] iv,
                                PaddingType padding,
                                ref bool stop,
                                ProgressChanged progressChanged,
                                ref byte[] lastInputBlock,
                                int blocksize = 8)
        {
            using (CStreamReader reader = inputStream.CreateReader())
            {
                using (CStreamWriter writer = new CStreamWriter())
                {
                    byte[] lastBlock = iv;
                    int readcount = 0;

                    while (reader.Position < reader.Length && !stop)
                    {
                        //we always try to read a complete block
                        byte[] inputBlock = new byte[blocksize];
                        readcount = 0;
                        while ((readcount += reader.Read(inputBlock, readcount, blocksize - readcount)) < blocksize &&
                               reader.Position < reader.Length && !stop)
                        {
                            ;
                        }

                        if (stop)
                        {
                            return;
                        }

                        //Show progress in UI
                        progressChanged(reader.Position, reader.Length);

                        byte[] outputblock = null;
                        //we read a complete block
                        if (readcount == blocksize)
                        {
                            //Compute XOR with lastblock for CFB mode
                            if (cipherAction == CipherAction.Encrypt)
                            {
                                outputblock = blockCipher(lastBlock, key);
                                outputblock = XOR(outputblock, inputBlock);
                                lastBlock = outputblock;
                            }
                            else
                            {
                                outputblock = blockCipher(lastBlock, key);
                                outputblock = XOR(outputblock, inputBlock);
                                lastBlock = inputBlock;
                            }
                        }
                        //we read an incomplete block, thus, we are at the end of the stream
                        else if (readcount > 0)
                        {
                            //Compute XOR with lastblock for CFB mode
                            if (cipherAction == CipherAction.Encrypt)
                            {
                                byte[] block = new byte[blocksize];
                                Array.Copy(inputBlock, 0, block, 0, readcount);
                                outputblock = blockCipher(lastBlock, key);
                                outputblock = XOR(outputblock, block);
                            }
                            else
                            {
                                byte[] block = new byte[blocksize];
                                Array.Copy(inputBlock, 0, block, 0, readcount);
                                outputblock = blockCipher(inputBlock, key);
                                outputblock = XOR(outputblock, lastBlock);
                            }
                        }

                        //check if it is the last block and we decrypt, thus, we have to remove the padding
                        if (reader.Position == reader.Length && cipherAction == CipherAction.Decrypt && padding != PaddingType.None)
                        {
                            int valid = StripPadding(outputblock, blocksize, padding, blocksize);
                            if (valid != blocksize)
                            {
                                byte[] newoutputblock = new byte[valid];
                                Array.Copy(outputblock, 0, newoutputblock, 0, valid);
                                outputblock = newoutputblock;
                            }
                            else if (valid == 0)
                            {
                                outputblock = null;
                            }
                        }

                        //if we crypted something, we output it
                        if (outputblock != null)
                        {
                            writer.Write(outputblock, 0, outputblock.Length);
                            //if we wrote to the stream, we memorize the last input block for the visualization
                            lastInputBlock = inputBlock;
                        }
                    }

                    writer.Flush();
                    outputStream = writer;
                }
            }
        }

        /// <summary>
        /// Encrypts/Decrypts using OFB
        /// </summary>
        /// <param name="blockCipher"></param>
        /// <param name="cipherAction"></param>
        /// <param name="inputStream"></param>
        /// <param name="outputStream"></param>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        /// <param name="padding"></param>
        /// <param name="stop"></param>
        /// <param name="progressChanged"></param>
        /// <param name="lastInputBlock"></param>
        /// <param name="blocksize"></param>
        public static void ExecuteOFB(BlockCipher blockCipher,
                                CipherAction cipherAction,
                                ref ICrypToolStream inputStream,
                                ref ICrypToolStream outputStream,
                                byte[] key,
                                byte[] iv,
                                PaddingType padding,
                                ref bool stop,
                                ProgressChanged progressChanged,
                                ref byte[] lastInputBlock,
                                int blocksize = 8)
        {
            using (CStreamReader reader = inputStream.CreateReader())
            {
                using (CStreamWriter writer = new CStreamWriter())
                {
                    byte[] lastBlock = iv;
                    int readcount = 0;

                    while (reader.Position < reader.Length && !stop)
                    {
                        //we always try to read a complete block
                        byte[] inputBlock = new byte[blocksize];
                        readcount = 0;
                        while ((readcount += reader.Read(inputBlock, readcount, blocksize - readcount)) < blocksize &&
                               reader.Position < reader.Length && !stop)
                        {
                            ;
                        }

                        if (stop)
                        {
                            return;
                        }

                        //Show progress in UI
                        progressChanged(reader.Position, reader.Length);

                        byte[] outputblock = null;
                        //we read a complete block
                        if (readcount == blocksize)
                        {
                            //Compute XOR with lastblock for OFB mode
                            if (cipherAction == CipherAction.Encrypt)
                            {
                                outputblock = blockCipher(lastBlock, key);
                                lastBlock = outputblock;
                                outputblock = XOR(outputblock, inputBlock);
                            }
                            else
                            {
                                outputblock = blockCipher(lastBlock, key);
                                lastBlock = outputblock;
                                outputblock = XOR(outputblock, inputBlock);
                            }
                        }
                        //we read an incomplete block, thus, we are at the end of the stream
                        else if (readcount > 0)
                        {
                            //Compute XOR with lastblock for CFB mode
                            if (cipherAction == CipherAction.Encrypt)
                            {
                                byte[] block = new byte[blocksize];
                                Array.Copy(inputBlock, 0, block, 0, readcount);
                                outputblock = blockCipher(lastBlock, key);
                                outputblock = XOR(outputblock, block);
                            }
                            else
                            {
                                byte[] block = new byte[blocksize];
                                Array.Copy(inputBlock, 0, block, 0, readcount);
                                outputblock = blockCipher(inputBlock, key);
                                outputblock = XOR(outputblock, lastBlock);
                            }
                        }

                        //check if it is the last block and we decrypt, thus, we have to remove the padding
                        if (reader.Position == reader.Length && cipherAction == CipherAction.Decrypt && padding != PaddingType.None)
                        {
                            int valid = StripPadding(outputblock, blocksize, padding, blocksize);
                            if (valid != blocksize)
                            {
                                byte[] newoutputblock = new byte[valid];
                                Array.Copy(outputblock, 0, newoutputblock, 0, valid);
                                outputblock = newoutputblock;
                            }
                            else if (valid == 0)
                            {
                                outputblock = null;
                            }
                        }

                        //if we crypted something, we output it
                        if (outputblock != null)
                        {
                            writer.Write(outputblock, 0, outputblock.Length);
                            //if we wrote to the stream, we memorize the last input block for the visualization
                            lastInputBlock = inputBlock;
                        }
                    }

                    writer.Flush();
                    outputStream = writer;
                }
            }
        }

        /// <summary>
        /// Calculates a XOR b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static byte[] XOR(byte[] a, byte[] b)
        {
            byte[] c = new byte[a.Length];
            for (int i = 0; i < c.Length; i++)
            {
                c[i] = (byte)(a[i] ^ b[i]);
            }
            return c;
        }

    }
}
