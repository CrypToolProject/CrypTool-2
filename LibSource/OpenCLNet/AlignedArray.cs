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

    public unsafe class AlignedArray<T> where T : struct
    {
        protected IntPtr UnmanagedMemory;
        protected IntPtr AlignedMemory;

        public long Length { get; protected set; }
        public long ByteLength { get; protected set; }
        protected long ByteAlignment;
        protected long AlignedArraySize;
        protected int TStride = Marshal.SizeOf(typeof(T));

        // Track whether Dispose has been called.
        private bool disposed = false;

        public AlignedArray(long size, long byteAlignment)
        {
            long alignmentMask;

            Length = size;
            ByteLength = size * TStride;
            AlignedArraySize = size * TStride;
            ByteAlignment = byteAlignment;
            UnmanagedMemory = Marshal.AllocHGlobal(new IntPtr(AlignedArraySize + byteAlignment - 1));
            alignmentMask = ByteAlignment - 1;
            AlignedMemory = new IntPtr((UnmanagedMemory.ToInt64() + byteAlignment - 1) & ~alignmentMask);
        }

        ~AlignedArray()
        {
            Dispose(false);
        }

        #region IDisposable Members

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                Marshal.FreeHGlobal(UnmanagedMemory);

                // Note disposing has been done.
                disposed = true;
            }
        }

        #endregion
    }


    #region AlignedArrayByte

    /// <summary>
    /// Aligned 1D array class for bytes
    /// </summary>
    public unsafe class AlignedArrayByte : AlignedArray<byte>
    {
        private readonly byte* pAlignedArray;

        public AlignedArrayByte(long size, long byteAlignment)
            : base(size, byteAlignment)
        {
            pAlignedArray = (byte*)AlignedMemory.ToPointer();
        }

        public IntPtr GetPtr(long index)
        {
            if (index >= Length || index < 0)
            {
                throw new IndexOutOfRangeException();
            }

            return new IntPtr(pAlignedArray + index);
        }

        public void Extract(long index, byte[] destinationArray, long destinationIndex, long length)
        {
            if (index + length >= Length || index + length < 0)
            {
                throw new IndexOutOfRangeException();
            }

            for (long i = 0; i < length; i++)
            {
                destinationArray[destinationIndex + i] = pAlignedArray[index + i];
            }
        }

        public void Insert(long index, byte[] sourceArray, long sourceIndex, long length)
        {
            if (index + length >= Length || index + length < 0)
            {
                throw new IndexOutOfRangeException();
            }

            for (long i = 0; i < length; i++)
            {
                pAlignedArray[index + i] = sourceArray[sourceIndex + i];
            }
        }

        public byte this[long index]
        {
            get
            {
                if (index < 0 || index >= Length)
                {
                    throw new IndexOutOfRangeException();
                }

                return pAlignedArray[index];
            }
            set
            {
                if (index < 0 || index >= Length)
                {
                    throw new IndexOutOfRangeException();
                }

                pAlignedArray[index] = value;
            }
        }

        public static implicit operator IntPtr(AlignedArrayByte array)
        {
            return new IntPtr(array.pAlignedArray);
        }
    }

    #endregion

    #region AlignedArrayInt

    /// <summary>
    /// Aligned 1D array class for ints
    /// </summary>
    public unsafe class AlignedArrayInt : AlignedArray<int>
    {
        private readonly int* pAlignedArray;

        public AlignedArrayInt(long size, long byteAlignment)
            : base(size, byteAlignment)
        {
            pAlignedArray = (int*)AlignedMemory.ToPointer();
        }

        public IntPtr GetPtr(long index)
        {
            if (index >= Length || index < 0)
            {
                throw new IndexOutOfRangeException();
            }

            return new IntPtr(pAlignedArray + index);
        }

        public void Extract(long index, int[] destinationArray, long destinationIndex, long length)
        {
            if (index + length >= Length || index + length < 0)
            {
                throw new IndexOutOfRangeException();
            }

            for (long i = 0; i < length; i++)
            {
                destinationArray[destinationIndex + i] = pAlignedArray[index + i];
            }
        }

        public void Insert(long index, int[] sourceArray, long sourceIndex, long length)
        {
            if (index + length >= Length || index + length < 0)
            {
                throw new IndexOutOfRangeException();
            }

            for (long i = 0; i < length; i++)
            {
                pAlignedArray[index + i] = sourceArray[sourceIndex + i];
            }
        }

        public int this[long index]
        {
            get
            {
                if (index < 0 || index >= Length)
                {
                    throw new IndexOutOfRangeException();
                }

                return pAlignedArray[index];
            }
            set
            {
                if (index < 0 || index >= Length)
                {
                    throw new IndexOutOfRangeException();
                }

                pAlignedArray[index] = value;
            }
        }

        public static implicit operator IntPtr(AlignedArrayInt array)
        {
            return new IntPtr(array.pAlignedArray);
        }
    }

    #endregion

    #region AlignedArrayLong

    /// <summary>
    /// Aligned 1D array class for longs
    /// </summary>
    public unsafe class AlignedArrayLong : AlignedArray<long>
    {
        private readonly long* pAlignedArray;

        public AlignedArrayLong(long size, long byteAlignment)
            : base(size, byteAlignment)
        {
            pAlignedArray = (long*)AlignedMemory.ToPointer();
        }

        public IntPtr GetPtr(long index)
        {
            if (index >= Length || index < 0)
            {
                throw new IndexOutOfRangeException();
            }

            return new IntPtr(pAlignedArray + index);
        }

        public void Extract(long index, long[] destinationArray, long destinationIndex, long length)
        {
            if (index + length >= Length || index + length < 0)
            {
                throw new IndexOutOfRangeException();
            }

            for (long i = 0; i < length; i++)
            {
                destinationArray[destinationIndex + i] = pAlignedArray[index + i];
            }
        }

        public void Insert(long index, long[] sourceArray, long sourceIndex, long length)
        {
            if (index + length >= Length || index + length < 0)
            {
                throw new IndexOutOfRangeException();
            }

            for (long i = 0; i < length; i++)
            {
                pAlignedArray[index + i] = sourceArray[sourceIndex + i];
            }
        }

        public long this[long index]
        {
            get
            {
                if (index < 0 || index >= Length)
                {
                    throw new IndexOutOfRangeException();
                }

                return pAlignedArray[index];
            }
            set
            {
                if (index < 0 || index >= Length)
                {
                    throw new IndexOutOfRangeException();
                }

                pAlignedArray[index] = value;
            }
        }

        public static implicit operator IntPtr(AlignedArrayLong array)
        {
            return new IntPtr(array.pAlignedArray);
        }
    }

    #endregion

    #region AlignedArrayFloat

    /// <summary>
    /// Aligned 1D array class for floats
    /// </summary>
    public unsafe class AlignedArrayFloat : AlignedArray<float>
    {
        private readonly float* pAlignedArray;

        public AlignedArrayFloat(long size, long byteAlignment)
            : base(size, byteAlignment)
        {
            pAlignedArray = (float*)AlignedMemory.ToPointer();
        }

        public IntPtr GetPtr(long index)
        {
            if (index >= Length || index < 0)
            {
                throw new IndexOutOfRangeException();
            }

            return new IntPtr(pAlignedArray + index);
        }

        public void Extract(long index, float[] destinationArray, long destinationIndex, long length)
        {
            if (index + length >= Length || index + length < 0)
            {
                throw new IndexOutOfRangeException();
            }

            for (long i = 0; i < length; i++)
            {
                destinationArray[destinationIndex + i] = pAlignedArray[index + i];
            }
        }

        public void Insert(long index, float[] sourceArray, long sourceIndex, long length)
        {
            if (index + length >= Length || index + length < 0)
            {
                throw new IndexOutOfRangeException();
            }

            for (long i = 0; i < length; i++)
            {
                pAlignedArray[index + i] = sourceArray[sourceIndex + i];
            }
        }

        public float this[long index]
        {
            get
            {
                if (index < 0 || index >= Length)
                {
                    throw new IndexOutOfRangeException();
                }

                return pAlignedArray[index];
            }
            set
            {
                if (index < 0 || index >= Length)
                {
                    throw new IndexOutOfRangeException();
                }

                pAlignedArray[index] = value;
            }
        }

        public static implicit operator IntPtr(AlignedArrayFloat array)
        {
            return new IntPtr(array.pAlignedArray);
        }
    }

    #endregion

}
