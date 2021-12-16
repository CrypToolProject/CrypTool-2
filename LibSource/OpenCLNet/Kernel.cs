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
    /// <summary>
    /// The Kernel class wraps an OpenCL kernel handle
    /// 
    /// The main purposes of this class is to serve as a handle to
    /// a compiled OpenCL function and to set arguments on the function
    /// before enqueueing calls.
    /// 
    /// Arguments are set using either the overloaded SetArg functions or
    /// explicit Set*Arg functions where * is a type. The most usual types
    /// are supported, but no vectors. If you need to set a parameter that's
    /// more advanced than what's supported here, use the version of SetArg
    /// that takes a pointer and size.
    /// 
    /// Note that pointer arguments are set by passing their OpenCL memory object,
    /// not native pointers.
    /// </summary>
    public unsafe class Kernel : InteropTools.IPropertyContainer
    {
        // Track whether Dispose has been called.
        private bool disposed = false;

        public string FunctionName => InteropTools.ReadString(this, (uint)KernelInfo.FUNCTION_NAME);
        public uint NumArgs => InteropTools.ReadUInt(this, (uint)KernelInfo.NUM_ARGS);
        public uint ReferenceCount => InteropTools.ReadUInt(this, (uint)KernelInfo.REFERENCE_COUNT);
        public Context Context { get; protected set; }
        public Program Program { get; protected set; }
        public IntPtr KernelID { get; set; }

        internal Kernel(Context context, Program program, IntPtr kernelID)
        {
            Context = context;
            Program = program;
            KernelID = kernelID;
        }

        ~Kernel()
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
                OpenCL.ReleaseKernel(KernelID);
                KernelID = IntPtr.Zero;

                // Note disposing has been done.
                disposed = true;
            }
        }


        #endregion

        public void SetArg(int argIndex, IntPtr argSize, IntPtr argValue)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, argSize, argValue.ToPointer());
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        #region SetArg functions

        /// <summary>
        /// Set argument argIndex to c
        /// </summary>
        /// <param name="argIndex"></param>
        /// <param name="c"></param>
        public void SetArg(int argIndex, sbyte c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, new IntPtr(sizeof(sbyte)), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        /// <summary>
        /// Set argument argIndex to c
        /// </summary>
        /// <param name="argIndex"></param>
        /// <param name="c"></param>
        public void SetArg(int argIndex, byte c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, new IntPtr(sizeof(byte)), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        /// <summary>
        /// Set argument argIndex to c
        /// </summary>
        /// <param name="argIndex"></param>
        /// <param name="c"></param>
        public void SetArg(int argIndex, short c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, new IntPtr(sizeof(short)), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        /// <summary>
        /// Set argument argIndex to c
        /// </summary>
        /// <param name="argIndex"></param>
        /// <param name="c"></param>
        public void SetArg(int argIndex, ushort c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, new IntPtr(sizeof(ushort)), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        /// <summary>
        /// Set argument argIndex to c
        /// </summary>
        /// <param name="argIndex"></param>
        /// <param name="c"></param>
        public void SetArg(int argIndex, int c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, new IntPtr(sizeof(int)), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        /// <summary>
        /// Set argument argIndex to c
        /// </summary>
        /// <param name="argIndex"></param>
        /// <param name="c"></param>
        public void SetArg(int argIndex, uint c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, new IntPtr(sizeof(uint)), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        /// <summary>
        /// Set argument argIndex to c
        /// </summary>
        /// <param name="argIndex"></param>
        /// <param name="c"></param>
        public void SetArg(int argIndex, long c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, new IntPtr(sizeof(long)), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        /// <summary>
        /// Set argument argIndex to c
        /// </summary>
        /// <param name="argIndex"></param>
        /// <param name="c"></param>
        public void SetArg(int argIndex, ulong c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, new IntPtr(sizeof(ulong)), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        /// <summary>
        /// Set argument argIndex to c
        /// </summary>
        /// <param name="argIndex"></param>
        /// <param name="c"></param>
        public void SetArg(int argIndex, float c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, new IntPtr(sizeof(float)), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        /// <summary>
        /// Set argument argIndex to c
        /// </summary>
        /// <param name="argIndex"></param>
        /// <param name="c"></param>
        public void SetArg(int argIndex, double c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, new IntPtr(sizeof(double)), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        /// <summary>
        /// Set argument argIndex to c
        /// </summary>
        /// <param name="argIndex"></param>
        /// <param name="c"></param>
        public void SetArg(int argIndex, IntPtr c)
        {
            ErrorCode result;
            IntPtr lc = c;
            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(IntPtr), &lc);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        /// <summary>
        /// Set argument argIndex to mem
        /// </summary>
        /// <param name="argIndex"></param>
        /// <param name="mem"></param>
        public void SetArg(int argIndex, Mem mem)
        {
            SetArg(argIndex, mem.MemID);
        }

        /// <summary>
        /// Set argument argIndex to sampler
        /// </summary>
        /// <param name="argIndex"></param>
        /// <param name="sampler"></param>
        public void SetArg(int argIndex, Sampler sampler)
        {
            SetArg(argIndex, sampler.SamplerID);
        }

        #region Vector Set functions

        #region Vector2

        public void SetArg(int argIndex, Char2 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Char2), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, UChar2 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(UChar2), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, Short2 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Short2), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, UShort2 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(UShort2), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, Int2 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Int2), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, UInt2 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(UInt2), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, Long2 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Long2), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, ULong2 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(ULong2), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, Float2 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Float2), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, Double2 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Double2), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        #endregion

        #region Vector3

        public void SetArg(int argIndex, Char3 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Char3), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, UChar3 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(UChar3), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, Short3 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Short3), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, UShort3 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(UShort3), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, Int3 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Int3), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, UInt3 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(UInt3), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, Long3 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Long3), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, ULong3 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(ULong3), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, Float3 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Float3), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, Double3 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Double3), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        #endregion

        #region Vector4

        public void SetArg(int argIndex, Char4 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Char4), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, UChar4 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(UChar4), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, Short4 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Short4), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, UShort4 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(UShort4), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, Int4 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Int4), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, UInt4 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(UInt4), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, Long4 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Long4), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, ULong4 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(ULong4), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, Float4 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Float4), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, Double4 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Double4), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        #endregion

        #region Vector8

        public void SetArg(int argIndex, Char8 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Char8), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, UChar8 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(UChar8), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, Short8 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Short8), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, UShort8 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(UShort8), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, Int8 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Int8), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, UInt8 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(UInt8), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, Long8 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Long8), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, ULong8 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(ULong8), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, Float8 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Float8), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, Double8 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Double8), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        #endregion

        #region Vector16

        public void SetArg(int argIndex, Char16 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Char16), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, UChar16 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(UChar16), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, Short16 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Short16), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, UShort16 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(UShort16), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, Int16 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Int16), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, UInt16 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(UInt16), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, Long16 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Long16), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, ULong16 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(ULong16), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, Float16 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Float16), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetArg(int argIndex, Double16 c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(Double16), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        #endregion

        #endregion

        #endregion

        #region SetSizeTArg

        /// <summary>
        /// This function will assign a value to a kernel argument of type size_t.
        /// size_t is 32 bit
        /// </summary>
        /// <param name="argIndex"></param>
        /// <param name="c"></param>
        public void SetSizeTArg(int argIndex, IntPtr c)
        {
            ErrorCode result;
            if (Context.Is64BitContext)
            {
                long l = c.ToInt64();
                result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, new IntPtr(8), &l);
            }
            else
            {
                int i = c.ToInt32();
                result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, new IntPtr(4), &i);
            }
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetSizeTArg(int argIndex, int c)
        {
            ErrorCode result;
            if (Context.Is64BitContext)
            {
                long l = c;
                result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, new IntPtr(8), &l);
            }
            else
            {
                int i = c;
                result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, new IntPtr(4), &i);
            }
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetSizeTArg(int argIndex, long c)
        {
            ErrorCode result;
            if (Context.Is64BitContext)
            {
                long l = c;
                result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, new IntPtr(8), &l);
            }
            else
            {
                int i = (int)c;
                result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, new IntPtr(4), &i);
            }
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        #endregion

        #region Setargs with explicit function names(For VB mostly)

        public void SetSByteArg(int argIndex, sbyte c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, new IntPtr(sizeof(sbyte)), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetByteArg(int argIndex, byte c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, new IntPtr(sizeof(byte)), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetShortArg(int argIndex, short c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, new IntPtr(sizeof(short)), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetUShortArg(int argIndex, ushort c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, new IntPtr(sizeof(ushort)), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetIntArg(int argIndex, int c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, new IntPtr(sizeof(int)), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetUIntArg(int argIndex, uint c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, new IntPtr(sizeof(uint)), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetLongArg(int argIndex, long c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, new IntPtr(sizeof(long)), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetULongArg(int argIndex, ulong c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, new IntPtr(sizeof(ulong)), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetSingleArg(int argIndex, float c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, new IntPtr(sizeof(float)), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetDoubleArg(int argIndex, double c)
        {
            ErrorCode result;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, new IntPtr(sizeof(double)), &c);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        /// <summary>
        /// Note that this function sets C# IntPtr args that are handles to OpenCL memory objects.
        /// The OpenCL-C datatype intptr_t is not available to use as a kernel argument.
        /// Use the type size_t and SetSizeTArg if you need platform specific integer sizes.
        /// </summary>
        /// <param name="argIndex"></param>
        /// <param name="c"></param>
        public void SetIntPtrArg(int argIndex, IntPtr c)
        {
            ErrorCode result;
            IntPtr lc = c;

            result = OpenCL.SetKernelArg(KernelID, (uint)argIndex, (IntPtr)sizeof(IntPtr), &lc);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetArg failed with error code " + result, result);
            }
        }

        public void SetMemArg(int argIndex, Mem mem)
        {
            SetIntPtrArg(argIndex, mem.MemID);
        }

        public void SetSamplerArg(int argIndex, Sampler sampler)
        {
            SetIntPtrArg(argIndex, sampler.SamplerID);
        }

        #endregion

#if false
        // Have to add some endian checking before compiling these into the library

        #region Set Char vectors
        
        public void SetChar2Arg(int argIndex, sbyte s0, sbyte s1)
        {
            sbyte* pBuffer = stackalloc sbyte[2];
            pBuffer[0] = s0;
            pBuffer[1] = s1;
            SetArg(argIndex, (IntPtr)(sizeof(sbyte) * 2), (IntPtr)pBuffer);
        }

        public void SetChar4Arg(int argIndex, sbyte s0, sbyte s1, sbyte s2, sbyte s3)
        {
            sbyte* pBuffer = stackalloc sbyte[4];
            pBuffer[0] = s0;
            pBuffer[1] = s1;
            pBuffer[2] = s2;
            pBuffer[3] = s3;
            SetArg(argIndex, (IntPtr)(sizeof(sbyte) * 4), (IntPtr)pBuffer);
        }

        #endregion
        
        #region Set UChar vectors

        public void SetUChar2Arg(int argIndex, byte s0, byte s1)
        {
            byte* pBuffer = stackalloc byte[2];
            pBuffer[0] = s0;
            pBuffer[1] = s1;
            SetArg(argIndex, (IntPtr)(sizeof(byte) * 2), (IntPtr)pBuffer);
        }

        public void SetUChar4Arg(int argIndex, byte s0, byte s1, byte s2, byte s3)
        {
            byte* pBuffer = stackalloc byte[4];
            pBuffer[0] = s0;
            pBuffer[1] = s1;
            pBuffer[2] = s2;
            pBuffer[3] = s3;
            SetArg(argIndex, (IntPtr)(sizeof(byte) * 4), (IntPtr)pBuffer);
        }

        #endregion

        #region Set Int vectors

        public void SetInt2Arg(int argIndex, int s0, int s1)
        {
            int* pBuffer = stackalloc int[2];
            pBuffer[0] = s0;
            pBuffer[1] = s1;
            SetArg(argIndex, (IntPtr)(sizeof(int) * 2), (IntPtr)pBuffer);
        }

        public void SetInt4Arg(int argIndex, int s0, int s1, int s2, int s3)
        {
            int* pBuffer = stackalloc int[4];
            pBuffer[0] = s0;
            pBuffer[1] = s1;
            pBuffer[2] = s2;
            pBuffer[3] = s3;
            SetArg(argIndex, (IntPtr)(sizeof(int) * 4), (IntPtr)pBuffer);
        }

        #endregion

        #region Set UInt vectors

        public void SetUInt2Arg(int argIndex, uint s0, uint s1)
        {
            uint* pBuffer = stackalloc uint[2];
            pBuffer[0] = s0;
            pBuffer[1] = s1;
            SetArg(argIndex, (IntPtr)(sizeof(uint) * 2), (IntPtr)pBuffer);
        }

        public void SetUInt4Arg(int argIndex, uint s0, uint s1, uint s2, uint s3)
        {
            uint* pBuffer = stackalloc uint[4];
            pBuffer[0] = s0;
            pBuffer[1] = s1;
            pBuffer[2] = s2;
            pBuffer[3] = s3;
            SetArg(argIndex, (IntPtr)(sizeof(uint) * 4), (IntPtr)pBuffer);
        }

        #endregion

        #region Set Long vectors

        public void SetLong2Arg(int argIndex, long s0, long s1)
        {
            long* pBuffer = stackalloc long[2];
            pBuffer[0] = s0;
            pBuffer[1] = s1;
            SetArg(argIndex, (IntPtr)(sizeof(long) * 2), (IntPtr)pBuffer);
        }

        public void SetLong4Arg(int argIndex, long s0, long s1, long s2, long s3)
        {
            long* pBuffer = stackalloc long[4];
            pBuffer[0] = s0;
            pBuffer[1] = s1;
            pBuffer[2] = s2;
            pBuffer[3] = s3;
            SetArg(argIndex, (IntPtr)(sizeof(long) * 4), (IntPtr)pBuffer);
        }

        #endregion

        #region Set ULong vectors

        public void SetULong2Arg(int argIndex, ulong s0, ulong s1)
        {
            ulong* pBuffer = stackalloc ulong[2];
            pBuffer[0] = s0;
            pBuffer[1] = s1;
            SetArg(argIndex, (IntPtr)(sizeof(ulong) * 2), (IntPtr)pBuffer);
        }

        public void SetULong4Arg(int argIndex, ulong s0, ulong s1, ulong s2, ulong s3)
        {
            ulong* pBuffer = stackalloc ulong[4];
            pBuffer[0] = s0;
            pBuffer[1] = s1;
            pBuffer[2] = s2;
            pBuffer[3] = s3;
            SetArg(argIndex, (IntPtr)(sizeof(ulong) * 4), (IntPtr)pBuffer);
        }

        #endregion

        #region Set Float vectors

        public void SetFloat2Arg(int argIndex, float s0, float s1)
        {
            float* pBuffer = stackalloc float[2];
            pBuffer[0] = s0;
            pBuffer[1] = s1;
            SetArg(argIndex, (IntPtr)(sizeof(float) * 2), (IntPtr)pBuffer);
        }

        public void SetFloat4Arg(int argIndex, float s0, float s1, float s2, float s3)
        {
            float* pBuffer = stackalloc float[4];
            pBuffer[0] = s0;
            pBuffer[1] = s1;
            pBuffer[2] = s2;
            pBuffer[3] = s3;
            SetArg(argIndex, (IntPtr)(sizeof(float) * 4), (IntPtr)pBuffer);
        }

        #endregion

#endif
        public static implicit operator IntPtr(Kernel k)
        {
            return k.KernelID;
        }

        #region IPropertyContainer Members

        public IntPtr GetPropertySize(uint key)
        {
            ErrorCode result;

            result = OpenCL.GetKernelInfo(KernelID, key, IntPtr.Zero, null, out IntPtr size);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("Unable to get kernel info for kernel " + KernelID, result);
            }

            return size;
        }

        public void ReadProperty(uint key, IntPtr keyLength, void* pBuffer)
        {
            ErrorCode result;

            result = OpenCL.GetKernelInfo(KernelID, key, keyLength, pBuffer, out IntPtr size);
            if (result != (int)ErrorCode.SUCCESS)
            {
                throw new OpenCLException("Unable to get kernel info for kernel " + KernelID, result);
            }
        }

        #endregion
    }
}
