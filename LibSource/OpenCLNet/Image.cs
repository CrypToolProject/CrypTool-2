/*
 * Copyright (c) 2009 Olav Kalgraf(olav.kalgraf@gmail.com)
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Runtime.InteropServices;

namespace OpenCLNet
{
    public unsafe class Image : Mem
    {
        internal Image(Context context, IntPtr memID)
            : base(context,memID)
        {
        }

        #region Properties

        public ImageFormat ImageFormat
        {
            get
            {
                IntPtr size = GetPropertySize((uint)ImageInfo.FORMAT);
                byte* pBuffer = stackalloc byte[(int)size];

                ReadProperty((uint)ImageInfo.FORMAT, size, pBuffer);
                return (ImageFormat)Marshal.PtrToStructure((IntPtr)pBuffer, typeof(ImageFormat));
            }
        }
        public IntPtr ElementSize { get { return InteropTools.ReadIntPtr(this, (uint)ImageInfo.ELEMENT_SIZE); } }
        public IntPtr RowPitch { get { return InteropTools.ReadIntPtr(this, (uint)ImageInfo.ROW_PITCH); } }
        public IntPtr SlicePitch { get { return InteropTools.ReadIntPtr(this, (uint)ImageInfo.SLICE_PITCH); } }
        public IntPtr Width { get { return InteropTools.ReadIntPtr(this, (uint)ImageInfo.WIDTH); } }
        public IntPtr Height { get { return InteropTools.ReadIntPtr(this, (uint)ImageInfo.HEIGHT); } }
        public IntPtr Depth { get { return InteropTools.ReadIntPtr(this, (uint)ImageInfo.DEPTH); } }

        #endregion

        // Override the IPropertyContainer interface of the Mem class.
        #region IPropertyContainer Members

        public override unsafe IntPtr GetPropertySize(uint key)
        {
            IntPtr size;
            ErrorCode result;

            result = (ErrorCode)OpenCL.GetImageInfo(MemID, key, IntPtr.Zero, null, out size);
            if (result != ErrorCode.SUCCESS)
            {
                size = base.GetPropertySize(key);
            }
            return size;
        }

        public override unsafe void ReadProperty(uint key, IntPtr keyLength, void* pBuffer)
        {
            IntPtr size;
            ErrorCode result;

            result = (ErrorCode)OpenCL.GetImageInfo(MemID, key, keyLength, pBuffer, out size);
            if (result != ErrorCode.SUCCESS)
            {
                base.ReadProperty(key, keyLength, pBuffer);
            }
        }

        #endregion
    }
}
