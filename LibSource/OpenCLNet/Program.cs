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
using System.Collections.Generic;

namespace OpenCLNet
{
    /// <summary>
    /// Wrapper for an OpenCL Program
    /// </summary>
    public unsafe class Program : IDisposable, InteropTools.IPropertyContainer
    {
        // Track whether Dispose has been called.
        private bool disposed = false;

        #region Properties

        public Context Context { get; protected set; }
        public IntPtr ProgramID { get; protected set; }
        public string Source => InteropTools.ReadString(this, (uint)ProgramInfo.SOURCE);
        public uint ReferenceCount => InteropTools.ReadUInt(this, (uint)ProgramInfo.REFERENCE_COUNT);
        public uint NumDevices => InteropTools.ReadUInt(this, (uint)ProgramInfo.NUM_DEVICES);

        public Device[] Devices
        {
            get
            {
                uint numDevices = NumDevices;
                if (numDevices == 0)
                {
                    return null;
                }

                byte[] data = InteropTools.ReadBytes(this, (uint)ProgramInfo.DEVICES);
                IntPtr[] deviceIDs = new IntPtr[numDevices];
                fixed (byte* pData = data)
                {
                    void** pBS = (void**)pData;
                    for (int i = 0; i < numDevices; i++)
                    {
                        deviceIDs[i] = new IntPtr(pBS[i]);
                    }
                }
                return InteropTools.ConvertDeviceIDsToDevices(Context.Platform, deviceIDs);
            }
        }


        public IntPtr[] BinarySizes
        {
            get
            {
                uint numDevices = NumDevices;
                if (numDevices == 0)
                {
                    return null;
                }

                byte[] data = InteropTools.ReadBytes(this, (uint)ProgramInfo.BINARY_SIZES);
                IntPtr[] binarySizes = new IntPtr[numDevices];
                fixed (byte* pData = data)
                {
                    void** pBS = (void**)pData;
                    for (int i = 0; i < numDevices; i++)
                    {
                        binarySizes[i] = new IntPtr(pBS[i]);
                    }
                }
                return binarySizes;
            }
        }

        public byte[][] Binaries
        {
            get
            {
                uint numDevices = NumDevices;
                if (numDevices == 0)
                {
                    return null;
                }

                IntPtr[] binarySizes = BinarySizes;
                byte[][] binaries = new byte[numDevices][];
                for (int i = 0; i < numDevices; i++)
                {
                    binaries[i] = new byte[binarySizes[i].ToInt64()];
                }

                InteropTools.ReadPreAllocatedBytePtrArray(this, (uint)ProgramInfo.BINARIES, binaries);
                return binaries;
            }
        }

        #endregion

        internal Program(Context context, IntPtr programID)
        {
            Context = context;
            ProgramID = programID;
        }

        ~Program()
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
                ErrorCode result = OpenCL.ReleaseProgram(ProgramID);
                if (result != ErrorCode.SUCCESS)
                {
                    throw new OpenCLException("ReleaseProgram failed: " + result, result);
                }

                // Note disposing has been done.
                disposed = true;
            }
        }


        #endregion

        public void Build()
        {
            Build(null, null, IntPtr.Zero);
        }

        public void Build(Device[] devices, ProgramNotify notify, IntPtr userData)
        {
            Build(devices, null, notify, userData);
        }

        public void Build(Device[] devices, string options, ProgramNotify notify, IntPtr userData)
        {
            ErrorCode result;
            IntPtr[] deviceIDs;
            int deviceLength = 0;

            if (devices != null)
            {
                deviceLength = devices.Length;
            }

            deviceIDs = InteropTools.ConvertDevicesToDeviceIDs(devices);
            result = OpenCL.BuildProgram(ProgramID,
                (uint)deviceLength,
                deviceIDs,
                options,
                notify,
                userData);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLBuildException(this, result);
            }
        }

        public Kernel CreateKernel(string kernelName)
        {
            IntPtr kernelID;

            kernelID = OpenCL.CreateKernel(ProgramID, kernelName, out ErrorCode result);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("CreateKernel failed with error code " + result, result);
            }

            return new Kernel(Context, this, kernelID);
        }

        /// <summary>
        /// Create all kernels in the program and return them as an array
        /// </summary>
        /// <returns></returns>
        public Kernel[] CreateKernels()
        {
            ErrorCode result;

            result = OpenCL.CreateKernelsInProgram(ProgramID, 0, null, out uint numKernels);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("CreateKernels failed with error code " + result, result);
            }

            IntPtr[] kernelIDs = new IntPtr[numKernels];
            result = OpenCL.CreateKernelsInProgram(ProgramID, numKernels, kernelIDs, out numKernels);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("CreateKernels failed with error code " + result, result);
            }

            Kernel[] kernels = new Kernel[numKernels];
            for (int i = 0; i < kernels.Length; i++)
            {
                kernels[i] = new Kernel(Context, this, kernelIDs[i]);
            }

            return kernels;
        }

        /// <summary>
        /// Create all kernels in the program and return them as a Dictionary.
        /// Its keys are the kernel names, its values are the kernels themselves.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, Kernel> CreateKernelDictionary()
        {
            Kernel[] kernels = CreateKernels();
            Dictionary<string, Kernel> kernelDictionary = new Dictionary<string, Kernel>();

            foreach (Kernel k in kernels)
            {
                kernelDictionary[k.FunctionName] = k;
            }

            return kernelDictionary;
        }

        public static implicit operator IntPtr(Program p)
        {
            return p.ProgramID;
        }

        public string GetBuildOptions(Device device)
        {
            BuildInfo buildInfo;

            buildInfo = new BuildInfo(this, device);
            return InteropTools.ReadString(buildInfo, (uint)ProgramBuildInfo.OPTIONS);
        }

        public string GetBuildLog(Device device)
        {
            BuildInfo buildInfo;

            buildInfo = new BuildInfo(this, device);
            return InteropTools.ReadString(buildInfo, (uint)ProgramBuildInfo.LOG);
        }

        public BuildStatus GetBuildStatus(Device device)
        {
            BuildInfo buildInfo;

            buildInfo = new BuildInfo(this, device);
            return (BuildStatus)InteropTools.ReadInt(buildInfo, (uint)ProgramBuildInfo.STATUS);
        }

        private class BuildInfo : InteropTools.IPropertyContainer
        {
            private readonly Program Program;
            private readonly Device Device;

            public BuildInfo(Program p, Device d)
            {
                Program = p;
                Device = d;
            }

            #region IPropertyContainer Members

            public IntPtr GetPropertySize(uint key)
            {
                ErrorCode result;

                result = OpenCL.GetProgramBuildInfo(Program.ProgramID, Device.DeviceID, key, IntPtr.Zero, null, out IntPtr size);
                if (result != ErrorCode.SUCCESS)
                {
                    throw new OpenCLException("clGetProgramBuildInfo failed with error code " + result, result);
                }

                return size;
            }

            public void ReadProperty(uint key, IntPtr keyLength, void* pBuffer)
            {
                ErrorCode result;

                result = OpenCL.GetProgramBuildInfo(Program.ProgramID, Device.DeviceID, key, keyLength, pBuffer, out IntPtr size);
                if (result != ErrorCode.SUCCESS)
                {
                    throw new OpenCLException("clGetProgramBuildInfo failed with error code " + result, result);
                }
            }

            #endregion
        }

        #region IPropertyContainer Members

        public IntPtr GetPropertySize(uint key)
        {
            ErrorCode result;

            result = OpenCL.GetProgramInfo(ProgramID, key, IntPtr.Zero, null, out IntPtr size);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("clGetProgramInfo failed with error code " + result, result);
            }

            return size;
        }

        public void ReadProperty(uint key, IntPtr keyLength, void* pBuffer)
        {
            ErrorCode result;

            result = OpenCL.GetProgramInfo(ProgramID, key, keyLength, pBuffer, out IntPtr size);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("clGetProgramInfo failed with error code " + result, result);
            }
        }

        #endregion
    }
}
