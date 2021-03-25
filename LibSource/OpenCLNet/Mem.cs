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

namespace OpenCLNet
{
    unsafe public class Mem : IDisposable, InteropTools.IPropertyContainer
    {
        // Track whether Dispose has been called.
        private bool disposed = false;

        private TextureInfo TxInfo;

        public IntPtr MemID { get; protected set; }
        public Context Context { get; protected set; }
        public MemObjectType MemType { get { return (MemObjectType)InteropTools.ReadUInt( this, (uint)MemInfo.TYPE ); } }
        public MemFlags MemFlags { get { return (MemFlags)InteropTools.ReadULong( this, (uint)MemInfo.FLAGS ); } }
        public IntPtr MemSize { get { return InteropTools.ReadIntPtr( this, (uint)MemInfo.SIZE ); } }
        public IntPtr HostPtr { get { return InteropTools.ReadIntPtr( this, (uint)MemInfo.HOST_PTR ); } }
        public uint MapCount { get { return InteropTools.ReadUInt( this, (uint)MemInfo.MAP_COUNT ); } }
        public uint ReferenceCount { get { return InteropTools.ReadUInt( this, (uint)MemInfo.REFERENCE_COUNT ); } }

        public uint TextureTarget { get { return InteropTools.ReadUInt(TxInfo, (uint)CLGLTextureInfo.TEXTURE_TARGET); } }
        public int MipMapLevel { get { return InteropTools.ReadInt(TxInfo, (uint)CLGLTextureInfo.MIPMAP_LEVEL);  } }


        #region Construction / Destruction

        internal Mem( Context context, IntPtr memID )
        {
            Context = context;
            MemID = memID;
            TxInfo = new TextureInfo(this);
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~Mem()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose( false );
        }

        #endregion

        #region IDisposable Members

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose( true );
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize( this );
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        private void Dispose( bool disposing )
        {
            // Check to see if Dispose has already been called.
            if( !this.disposed )
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if( disposing )
                {
                    // Dispose managed resources.
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.
                OpenCL.ReleaseMemObject( MemID );
                MemID = IntPtr.Zero;

                // Note disposing has been done.
                disposed = true;
            }
        }

        #endregion

        #region Utility functions

        #region Write

        public virtual void Write(CommandQueue cq, long dstOffset, byte[] srcData, int srcStartIndex, int count)
        {
            IntPtr p = cq.EnqueueMapBuffer(this, true, MapFlags.WRITE, dstOffset, count);
            byte* pBlock = (byte*)p.ToPointer();
            for (long i = 0; i < count; i++)
                pBlock[i] = srcData[i + srcStartIndex];
            cq.EnqueueUnmapMemObject(this, p);
            cq.Finish();
        }

        public virtual void Write(CommandQueue cq, long dstOffset, short[] srcData, int srcStartIndex, int count)
        {
            IntPtr p = cq.EnqueueMapBuffer(this, true, MapFlags.WRITE, dstOffset, (long)count * sizeof(short));
            short* pBlock = (short*)p.ToPointer();
            for (long i = 0; i < count; i++)
                pBlock[i] = srcData[i + srcStartIndex];
            cq.EnqueueUnmapMemObject(this, p);
            cq.Finish();
        }

        public virtual void Write(CommandQueue cq, long dstOffset, int[] srcData, int srcStartIndex, int count)
        {
            IntPtr p = cq.EnqueueMapBuffer(this, true, MapFlags.WRITE, dstOffset, (long)count * sizeof(int));
            int* pBlock = (int*)p.ToPointer();
            for (long i = 0; i < count; i++)
                pBlock[i] = srcData[i + srcStartIndex];
            cq.EnqueueUnmapMemObject(this, p);
            cq.Finish();
        }

        public virtual void Write(CommandQueue cq, long dstOffset, float[] srcData, int srcStartIndex, int count)
        {
            IntPtr p = cq.EnqueueMapBuffer(this, true, MapFlags.WRITE, dstOffset, (long)count * sizeof(float));
            float* pBlock = (float*)p.ToPointer();
            for (long i = 0; i < count; i++)
                pBlock[i] = srcData[i + srcStartIndex];
            cq.EnqueueUnmapMemObject(this, p);
            cq.Finish();
        }

        public virtual void Write(CommandQueue cq, long dstOffset, double[] srcData, int srcStartIndex, int count)
        {
            IntPtr p = cq.EnqueueMapBuffer(this, true, MapFlags.WRITE, dstOffset, (long)count * sizeof(double));
            double* pBlock = (double*)p.ToPointer();
            for (long i = 0; i < count; i++)
                pBlock[i] = srcData[i + srcStartIndex];
            cq.EnqueueUnmapMemObject(this, p);
            cq.Finish();
        }
        
        #endregion

        #region Read

        public virtual void Read(CommandQueue cq, long srcOffset, byte[] dstData, int dstStartIndex, int count)
        {
            IntPtr p = cq.EnqueueMapBuffer(this, true, MapFlags.READ, srcOffset, count);
            byte* pBlock = (byte*)p.ToPointer();
            for (long i = 0; i < count; i++)
                dstData[dstStartIndex + i] = pBlock[i];
            cq.EnqueueUnmapMemObject(this, p);
            cq.Finish();
        }

        public virtual void Read(CommandQueue cq, long srcOffset, short[] dstData, int dstStartIndex, int count)
        {
            IntPtr p = cq.EnqueueMapBuffer(this, true, MapFlags.READ, srcOffset, count * sizeof(short));
            short* pBlock = (short*)p.ToPointer();
            for (long i = 0; i < count; i++)
                dstData[dstStartIndex + i] = pBlock[i];
            cq.EnqueueUnmapMemObject(this, p);
            cq.Finish();
        }

        public virtual void Read(CommandQueue cq, long srcOffset, int[] dstData, int dstStartIndex, int count)
        {
            IntPtr p = cq.EnqueueMapBuffer(this, true, MapFlags.READ, srcOffset, count * sizeof(int));
            int* pBlock = (int*)p.ToPointer();
            for (long i = 0; i < count; i++)
                dstData[dstStartIndex + i] = pBlock[i];
            cq.EnqueueUnmapMemObject(this, p);
            cq.Finish();
        }

        public virtual void Read(CommandQueue cq, long srcOffset, float[] dstData, int dstStartIndex, int count)
        {
            IntPtr p = cq.EnqueueMapBuffer(this, true, MapFlags.READ, srcOffset, count * sizeof(float));
            float* pBlock = (float*)p.ToPointer();
            for (long i = 0; i < count; i++)
                dstData[dstStartIndex + i] = pBlock[i];
            cq.EnqueueUnmapMemObject(this, p);
            cq.Finish();
        }

        public virtual void Read(CommandQueue cq, long srcOffset, double[] dstData, int dstStartIndex, int count)
        {
            IntPtr p = cq.EnqueueMapBuffer(this, true, MapFlags.READ, srcOffset, count * sizeof(double));
            double* pBlock = (double*)p.ToPointer();
            for (long i = 0; i < count; i++)
                dstData[dstStartIndex+i] = pBlock[i];
            cq.EnqueueUnmapMemObject(this, p);
            cq.Finish();
        }

        #endregion

        #region MemSet

        public virtual void MemSet(CommandQueue cq, long dstByteOffset, byte value, long count)
        {
            IntPtr p = cq.EnqueueMapBuffer(this, true, MapFlags.WRITE, dstByteOffset, count);
            byte* pBlock = (byte*)p.ToPointer();
            for (long i = 0; i < count; i++)
                pBlock[i] = value;
            cq.EnqueueUnmapMemObject(this, p);
            cq.Finish();
        }

        public virtual void MemSet(CommandQueue cq, long dstByteOffset, short value, long count)
        {
            IntPtr p = cq.EnqueueMapBuffer(this, true, MapFlags.WRITE, dstByteOffset, count * sizeof(short));
            short* pBlock = (short*)p.ToPointer();
            for (long i = 0; i < count; i++)
                pBlock[i] = value;
            cq.EnqueueUnmapMemObject(this, p);
            cq.Finish();
        }

        public virtual void MemSet(CommandQueue cq, long dstByteOffset, int value, long count)
        {
            IntPtr p = cq.EnqueueMapBuffer(this, true, MapFlags.WRITE, dstByteOffset, count * sizeof(int));
            int* pBlock = (int*)p.ToPointer();
            for (long i = 0; i < count; i++)
                pBlock[i] = value;
            cq.EnqueueUnmapMemObject(this, p);
            cq.Finish();
        }

        public virtual void MemSet(CommandQueue cq, long dstByteOffset, float value, long count)
        {
            IntPtr p = cq.EnqueueMapBuffer(this, true, MapFlags.WRITE, dstByteOffset, count * sizeof(float));
            float* pBlock = (float*)p.ToPointer();
            for (long i = 0; i < count; i++)
                pBlock[i] = value;
            cq.EnqueueUnmapMemObject(this, p);
            cq.Finish();
        }

        public virtual void MemSet(CommandQueue cq, long dstByteOffset, double value, long count)
        {
            IntPtr p = cq.EnqueueMapBuffer(this, true, MapFlags.WRITE, dstByteOffset, count * sizeof(double));
            double* pBlock = (double*)p.ToPointer();
            for (long i = 0; i < count; i++)
                pBlock[i] = value;
            cq.EnqueueUnmapMemObject(this, p);
            cq.Finish();
        }

        public virtual void MemSet(CommandQueue cq, byte value)
        {
            long offset = 0;
            long count = MemSize.ToInt64();
            IntPtr p = cq.EnqueueMapBuffer(this, true, MapFlags.WRITE, offset, count);
            byte* pBlock = (byte*)p.ToPointer();
            for (long i = 0; i < count; i++)
                pBlock[i] = value;
            cq.EnqueueUnmapMemObject(this, p);
            cq.Finish();
        }

        #endregion

        #endregion

        /// <summary>
        /// OpenCL 1.1
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="flags"></param>
        /// <param name="buffer_create_info"></param>
        /// <param name="errcode_ret"></param>
        /// <returns></returns>
        public Mem CreateSubBuffer(Mem buffer, MemFlags flags, BufferRegion buffer_create_info, out ErrorCode errcode_ret)
        {
            IntPtr memID = OpenCL.CreateSubBuffer(buffer.MemID, flags, buffer_create_info, out errcode_ret);
            return new Mem(buffer.Context, memID);
        }

        public void GetGLObjectInfo(out CLGLObjectType glObjectType, out IntPtr glObjectName)
        {
            ErrorCode result;
            uint type;
            uint name;

            result = OpenCL.GetGLObjectInfo(MemID, out type, out name);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("GetGLObjectInfo failed: " + result, result);
            glObjectType = (CLGLObjectType)type;
            glObjectName = (IntPtr)name;
        }


        #region IPropertyContainer Members

        unsafe public virtual IntPtr GetPropertySize( uint key )
        {
            IntPtr size;
            ErrorCode result;

            result = (ErrorCode)OpenCL.GetMemObjectInfo( MemID, key, IntPtr.Zero, null, out size );
            if( result!=ErrorCode.SUCCESS )
                throw new OpenCLException( "GetMemObjectInfo failed: "+result, result );
            return size;
        }

        unsafe public virtual void ReadProperty( uint key, IntPtr keyLength, void* pBuffer )
        {
            IntPtr size;
            ErrorCode result;

            result = (ErrorCode)OpenCL.GetMemObjectInfo( MemID, key, keyLength, pBuffer, out size );
            if( result!=ErrorCode.SUCCESS )
                throw new OpenCLException( "GetMemObjectInfo failed: "+result, result );
        }

        #endregion

        public static implicit operator IntPtr(Mem m)
        {
            return m.MemID;
        }

        class TextureInfo : InteropTools.IPropertyContainer
        {
            Mem Mem;

            public TextureInfo(Mem mem)
            {
                Mem = mem;
            }

            #region IPropertyContainer Members

            public IntPtr GetPropertySize(uint key)
            {
                ErrorCode result;
                IntPtr size;

                result = (ErrorCode)OpenCL.GetGLTextureInfo(Mem.MemID, key, IntPtr.Zero, null, out size);
                if (result != ErrorCode.SUCCESS)
                    throw new OpenCLException("GetGLTextureInfo failed with error code " + result, result);

                return size;
            }

            public void ReadProperty(uint key, IntPtr keyLength, void* pBuffer)
            {
                ErrorCode result;
                IntPtr size;

                result = (ErrorCode)OpenCL.GetGLTextureInfo(Mem.MemID, key, keyLength, pBuffer, out size);
                if (result != ErrorCode.SUCCESS)
                    throw new OpenCLException("GetGLTextureInfo failed with error code " + result, result);
            }

            #endregion
        }

    }
}
