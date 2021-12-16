/*
   Copyright 2008 Timm Korte, University of Siegen

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

//
// Mono.Security.Cryptography.SymmetricTransform implementation
//
// Authors:
//	Thomas Neidhart (tome@sbox.tugraz.at)
//	Sebastien Pouliot <sebastien@ximian.com>
//  Timm Korte <korte@easycrypt.de>
//
// Portions (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2007 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Security.Cryptography;

namespace CrypTool.PRESENT
{

    // This class implement most of the common code required for symmetric
    // algorithm transforms, like:
    // - CipherMode: Builds CBC and CFB on top of (descendant supplied) ECB
    // - PaddingMode, transform properties, multiple blocks, reuse...
    //
    // Descendants MUST:
    // - intialize themselves (like key expansion, ...)
    // - override the ECB (Electronic Code Book) method which will only be
    //   called using BlockSize byte[] array.
    internal abstract class SymmetricTransform : ICryptoTransform
    {
        protected SymmetricAlgorithm algo;
        protected bool encrypt;
        private readonly int BlockSizeByte;
        private byte[] temp;
        private byte[] temp2;
        private readonly byte[] workBuff;
        private readonly byte[] workout;
        private readonly int FeedBackByte;
        private readonly int FeedBackIter;
        private bool m_disposed = false;
        private bool lastBlock;

        public SymmetricTransform(SymmetricAlgorithm symmAlgo, bool encryption, byte[] rgbIV)
        {
            algo = symmAlgo;
            encrypt = encryption;
            BlockSizeByte = (algo.BlockSize >> 3);

            if (rgbIV == null)
            {
                rgbIV = new byte[BlockSizeByte];
                RNGCryptoServiceProvider sRandom = new RNGCryptoServiceProvider();
                sRandom.GetBytes(rgbIV);
            }
            else
            {
                rgbIV = (byte[])rgbIV.Clone();
            }

            // mode buffers
            temp = new byte[BlockSizeByte];
            Buffer.BlockCopy(rgbIV, 0, temp, 0, System.Math.Min(BlockSizeByte, rgbIV.Length));
            temp2 = new byte[BlockSizeByte];
            FeedBackByte = (algo.FeedbackSize >> 3);
            if (FeedBackByte != 0)
            {
                FeedBackIter = BlockSizeByte / FeedBackByte;
            }
            // transform buffers
            workBuff = new byte[BlockSizeByte];
            workout = new byte[BlockSizeByte];
        }

        ~SymmetricTransform()
        {
            Dispose(false);
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);  // Finalization is now unnecessary
        }
        // MUST be overriden by classes using unmanaged ressources
        // the override method must call the base class
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    // dispose managed object: zeroize and free
                    Array.Clear(temp, 0, BlockSizeByte);
                    temp = null;
                    Array.Clear(temp2, 0, BlockSizeByte);
                    temp2 = null;
                }
                m_disposed = true;
            }
        }

        public virtual bool CanTransformMultipleBlocks => true;

        public virtual bool CanReuseTransform => false;

        public virtual int InputBlockSize => BlockSizeByte;

        public virtual int OutputBlockSize => BlockSizeByte;

        // note: Each block MUST be BlockSizeValue in size!!!
        // i.e. Any padding must be done before calling this method
        protected virtual void Transform(byte[] input, byte[] output)
        {
            switch (algo.Mode)
            {
                case CipherMode.ECB:
                    ECB(input, output);
                    break;
                case CipherMode.CBC:
                    CBC(input, output);
                    break;
                case CipherMode.CFB:
                    CFB(input, output);
                    break;
                case CipherMode.OFB:
                    OFB(input, output);
                    break;
                case CipherMode.CTS:
                    CTS(input, output);
                    break;
                default:
                    throw new NotImplementedException("Unkown CipherMode" + algo.Mode.ToString());
            }
        }

        // Electronic Code Book (ECB)
        protected abstract void ECB(byte[] input, byte[] output);

        // Cipher-Block-Chaining (CBC)
        protected virtual void CBC(byte[] input, byte[] output)
        {
            if (encrypt)
            {
                for (int i = 0; i < BlockSizeByte; i++)
                {
                    temp[i] ^= input[i];
                }

                ECB(temp, output);
                Buffer.BlockCopy(output, 0, temp, 0, BlockSizeByte);
            }
            else
            {
                Buffer.BlockCopy(input, 0, temp2, 0, BlockSizeByte);
                ECB(input, output);
                for (int i = 0; i < BlockSizeByte; i++)
                {
                    output[i] ^= temp[i];
                }

                Buffer.BlockCopy(temp2, 0, temp, 0, BlockSizeByte);
            }
        }

        // Cipher-FeedBack (CFB)
        protected virtual void CFB(byte[] input, byte[] output)
        {
            if (encrypt)
            {
                for (int x = 0; x < FeedBackIter; x++)
                {
                    // temp is first initialized with the IV
                    ECB(temp, temp2);

                    for (int i = 0; i < FeedBackByte; i++)
                    {
                        output[i + x] = (byte)(temp2[i] ^ input[i + x]);
                    }

                    Buffer.BlockCopy(temp, FeedBackByte, temp, 0, BlockSizeByte - FeedBackByte);
                    Buffer.BlockCopy(output, x, temp, BlockSizeByte - FeedBackByte, FeedBackByte);
                }
            }
            else
            {
                for (int x = 0; x < FeedBackIter; x++)
                {
                    // we do not really decrypt this data!
                    encrypt = true;
                    // temp is first initialized with the IV
                    ECB(temp, temp2);
                    encrypt = false;

                    Buffer.BlockCopy(temp, FeedBackByte, temp, 0, BlockSizeByte - FeedBackByte);
                    Buffer.BlockCopy(input, x, temp, BlockSizeByte - FeedBackByte, FeedBackByte);
                    for (int i = 0; i < FeedBackByte; i++)
                    {
                        output[i + x] = (byte)(temp2[i] ^ input[i + x]);
                    }
                }
            }
        }

        // Output-FeedBack (OFB)
        protected virtual void OFB(byte[] input, byte[] output)
        {
            //throw new CryptographicException("OFB isn't supported by the framework");
            //Added by Timm Korte, 13.03.2008
            if (encrypt)
            {
                // temp is first initialized with the IV
                ECB(temp, temp2);
                Buffer.BlockCopy(temp2, 0, temp, 0, BlockSizeByte);
                for (int i = 0; i < BlockSizeByte; i++)
                {
                    temp2[i] ^= input[i];
                }

                Buffer.BlockCopy(temp2, 0, output, 0, BlockSizeByte);
            }
            else
            {
                // we do not really decrypt this data!
                encrypt = true;
                // temp is first initialized with the IV
                ECB(temp, temp2);
                encrypt = false;
                Buffer.BlockCopy(temp2, 0, temp, 0, BlockSizeByte);
                for (int i = 0; i < BlockSizeByte; i++)
                {
                    temp2[i] ^= input[i];
                }

                Buffer.BlockCopy(temp2, 0, output, 0, BlockSizeByte);
            }
        }

        // Cipher Text Stealing (CTS)
        protected virtual void CTS(byte[] input, byte[] output)
        {
            throw new CryptographicException("CTS isn't supported by the framework");
        }

        private void CheckInput(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            if (inputBuffer == null)
            {
                throw new ArgumentNullException("inputBuffer");
            }

            if (inputOffset < 0)
            {
                throw new ArgumentOutOfRangeException("inputOffset", "< 0");
            }

            if (inputCount < 0)
            {
                throw new ArgumentOutOfRangeException("inputCount", "< 0");
            }
            // ordered to avoid possible integer overflow
            if (inputOffset > inputBuffer.Length - inputCount)
            {
                throw new ArgumentException("inputBuffer", "Overflow");
            }
        }

        // this method may get called MANY times so this is the one to optimize
        public virtual int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException("Object is disposed");
            }

            CheckInput(inputBuffer, inputOffset, inputCount);
            // check output parameters
            if (outputBuffer == null)
            {
                throw new ArgumentNullException("outputBuffer");
            }

            if (outputOffset < 0)
            {
                throw new ArgumentOutOfRangeException("outputOffset", "< 0");
            }

            // ordered to avoid possible integer overflow
            int len = outputBuffer.Length - inputCount - outputOffset;
            if (!encrypt && (0 > len) && ((algo.Padding == PaddingMode.None) || (algo.Padding == PaddingMode.Zeros)))
            {
                throw new CryptographicException("outputBuffer", "Overflow");
            }
            else if (KeepLastBlock)
            {
                if (0 > len + BlockSizeByte)
                {
                    throw new IndexOutOfRangeException("Overflow");
                }
            }
            else
            {
                if (0 > len)
                {
                    // there's a special case if this is the end of the decryption process
                    if (inputBuffer.Length - inputOffset - outputBuffer.Length == BlockSizeByte)
                    {
                        inputCount = outputBuffer.Length - outputOffset;
                    }
                    else
                    {
                        throw new CryptographicException("outputBuffer", "Overflow");
                    }
                }
            }
            return InternalTransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
        }

        private bool KeepLastBlock => ((!encrypt) && (algo.Padding != PaddingMode.None) && (algo.Padding != PaddingMode.Zeros));

        private int InternalTransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            int offs = inputOffset;
            int full;

            // this way we don't do a modulo every time we're called
            // and we may save a division
            if (inputCount != BlockSizeByte)
            {
                if ((inputCount % BlockSizeByte) != 0)
                {
                    throw new CryptographicException("Invalid input block size.");
                }

                full = inputCount / BlockSizeByte;
            }
            else
            {
                full = 1;
            }

            if (KeepLastBlock)
            {
                full--;
            }

            int total = 0;

            if (lastBlock)
            {
                Transform(workBuff, workout);
                Buffer.BlockCopy(workout, 0, outputBuffer, outputOffset, BlockSizeByte);
                outputOffset += BlockSizeByte;
                total += BlockSizeByte;
                lastBlock = false;
            }

            for (int i = 0; i < full; i++)
            {
                Buffer.BlockCopy(inputBuffer, offs, workBuff, 0, BlockSizeByte);
                Transform(workBuff, workout);
                Buffer.BlockCopy(workout, 0, outputBuffer, outputOffset, BlockSizeByte);
                offs += BlockSizeByte;
                outputOffset += BlockSizeByte;
                total += BlockSizeByte;
            }

            if (KeepLastBlock)
            {
                Buffer.BlockCopy(inputBuffer, offs, workBuff, 0, BlockSizeByte);
                lastBlock = true;
            }

            return total;
        }

        private byte[] FinalEncrypt(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            // are there still full block to process ?
            int full = (inputCount / BlockSizeByte) * BlockSizeByte;
            int rem = inputCount - full;
            int total = full;

            switch (algo.Padding)
            {
                case PaddingMode.PKCS7:
                    // we need to add an extra block for padding
                    total += BlockSizeByte;
                    break;
                default:
                    if (inputCount == 0)
                    {
                        return new byte[0];
                    }

                    if (rem != 0)
                    {
                        if (algo.Padding == PaddingMode.None)
                        {
                            throw new CryptographicException("invalid block length");
                        }
                        // zero padding the input (by adding a block for the partial data)
                        byte[] paddedInput = new byte[full + BlockSizeByte];
                        Buffer.BlockCopy(inputBuffer, inputOffset, paddedInput, 0, inputCount);
                        inputBuffer = paddedInput;
                        inputOffset = 0;
                        inputCount = paddedInput.Length;
                        total = inputCount;
                    }
                    break;
            }

            byte[] res = new byte[total];
            int outputOffset = 0;

            // process all blocks except the last (final) block
            while (total > BlockSizeByte)
            {
                InternalTransformBlock(inputBuffer, inputOffset, BlockSizeByte, res, outputOffset);
                inputOffset += BlockSizeByte;
                outputOffset += BlockSizeByte;
                total -= BlockSizeByte;
            }

            // now we only have a single last block to encrypt
            byte padding = (byte)(BlockSizeByte - rem);
            switch (algo.Padding)
            {
                case PaddingMode.PKCS7:
                    // XX 07 07 07 07 07 07 07 (padding length)
                    for (int i = res.Length; --i >= (res.Length - padding);)
                    {
                        res[i] = padding;
                    }

                    Buffer.BlockCopy(inputBuffer, inputOffset, res, full, rem);
                    // the last padded block will be transformed in-place
                    InternalTransformBlock(res, full, BlockSizeByte, res, full);
                    break;
                default:
                    InternalTransformBlock(inputBuffer, inputOffset, BlockSizeByte, res, outputOffset);
                    break;
            }
            return res;
        }

        private byte[] FinalDecrypt(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            if ((inputCount % BlockSizeByte) > 0)
            {
                throw new CryptographicException("Invalid input block size.");
            }

            int total = inputCount;
            if (lastBlock)
            {
                total += BlockSizeByte;
            }

            byte[] res = new byte[total];
            int outputOffset = 0;

            while (inputCount > 0)
            {
                int len = InternalTransformBlock(inputBuffer, inputOffset, BlockSizeByte, res, outputOffset);
                inputOffset += BlockSizeByte;
                outputOffset += len;
                inputCount -= BlockSizeByte;
            }

            if (lastBlock)
            {
                Transform(workBuff, workout);
                Buffer.BlockCopy(workout, 0, res, outputOffset, BlockSizeByte);
                outputOffset += BlockSizeByte;
                lastBlock = false;
            }

            // total may be 0 (e.g. PaddingMode.None)
            byte padding = ((total > 0) ? res[total - 1] : (byte)0);
            switch (algo.Padding)
            {
                case PaddingMode.PKCS7:
                    total -= padding;
                    break;
                case PaddingMode.None:  // nothing to do - it's a multiple of block size
                case PaddingMode.Zeros: // nothing to do - user must unpad himself
                    break;
            }

            // return output without padding
            if (total > 0)
            {
                byte[] data = new byte[total];
                Buffer.BlockCopy(res, 0, data, 0, total);
                // zeroize decrypted data (copy with padding)
                Array.Clear(res, 0, res.Length);
                return data;
            }
            else
            {
                return new byte[0];
            }
        }

        public virtual byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            if (m_disposed)
            {
                throw new ObjectDisposedException("Object is disposed");
            }

            CheckInput(inputBuffer, inputOffset, inputCount);

            if (encrypt)
            {
                return FinalEncrypt(inputBuffer, inputOffset, inputCount);
            }
            else
            {
                return FinalDecrypt(inputBuffer, inputOffset, inputCount);
            }
        }
    }
}
