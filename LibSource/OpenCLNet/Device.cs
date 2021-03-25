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
using System.Text;
using System.IO;

namespace OpenCLNet
{
    unsafe public class Device : IDisposable, InteropTools.IPropertyContainer
    {
        protected HashSet<string> ExtensionHashSet = new HashSet<string>();
        private bool IsSubDevice;
        // Track whether Dispose has been called.
        private bool disposed = false;
        
        internal Device(Platform platform, IntPtr deviceID)
        {
            Platform = platform;
            DeviceID = deviceID;

            InitializeExtensionHashSet();
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~Device()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
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
            if (!this.disposed)
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
                if( IsSubDevice )
                    OpenCL.ReleaseDeviceEXT(DeviceID);

                // Note disposing has been done.
                disposed = true;
            }
        }

        #endregion


        #region Device Fission API (Extension)

        public void ReleaseDeviceEXT()
        {
            ErrorCode result;
            
            result = OpenCL.ReleaseDeviceEXT(DeviceID);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("ReleaseDeviceEXT failed with error code: "+result, result);
        }

        public void RetainDeviceEXT()
        {
            ErrorCode result;

            result = OpenCL.RetainDeviceEXT(DeviceID);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("RetainDeviceEXT failed with error code: " + result, result);
        }

        /// <summary>
        /// CreateSubDevicesEXT uses a slightly modified API,
        /// due to the overall messiness of creating a
        /// cl_device_partition_property_ext in managed C#.
        /// 
        /// The object list properties is a linear list of partition properties and arguments
        /// add the DevicePartition property IDs  and ListTerminators as ulongs and the argument lists as ints
        /// CreateSubDevicesEXT will use that info to construct a binary block
        /// </summary>
        /// <param name="properties"></param>
        public unsafe Device[] CreateSubDevicesEXT(List<object> properties)
        {
            ErrorCode result;
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            for (int i = 0; i < properties.Count; i++)
            {
                if (properties[i] is ulong)
                    bw.Write((ulong)properties[i]);
                else if (properties[i] is int)
                    bw.Write((int)properties[i]);
                else
                    throw new ArgumentException("CreateSubDevicesEXT: property lists only accepts ulongs and ints");
            }
            bw.Flush();
            byte[] propertyArray = ms.ToArray();
            uint numDevices;
            result = OpenCL.CreateSubDevicesEXT(DeviceID, propertyArray, 0, null, &numDevices);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("CreateSubDevicesEXT failed with error code: "+result, result);

            IntPtr[] subDeviceIDs = new IntPtr[(int)numDevices];
            result = OpenCL.CreateSubDevicesEXT(DeviceID, propertyArray, numDevices, subDeviceIDs, null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("CreateSubDevicesEXT failed with error code: " + result, result);

            Device[] subDevices = new Device[(int)numDevices];
            for (int i = 0; i < (int)numDevices; i++)
            {
                Device d = new Device(Platform, subDeviceIDs[i]);
                d.IsSubDevice = true;
                subDevices[i] = d;
            }
            return subDevices;
        }

        #endregion

        public static implicit operator IntPtr(Device d)
        {
            return d.DeviceID;
        }

        #region Properties

        public IntPtr DeviceID { get; protected set; }

        public DeviceType DeviceType { get { return (DeviceType) InteropTools.ReadULong( this, (uint)DeviceInfo.TYPE ); } }
        /// <summary>
        /// A unique device vendor identifier. An example of a unique device identifier could be the PCIe ID.
        /// </summary>
        public uint VendorID { get { return InteropTools.ReadUInt( this, (uint)DeviceInfo.VENDOR_ID ); } }
        /// <summary>
        /// The number of parallel compute cores on the OpenCL device. The minimum value is 1.
        /// </summary>
        public uint MaxComputeUnits { get { return InteropTools.ReadUInt( this, (uint)DeviceInfo.MAX_COMPUTE_UNITS ); } }
        /// <summary>
        /// Maximum dimensions that specify the global and local work-item IDs used by the data parallel execution model.
        /// (Refer to clEnqueueNDRangeKernel). The minimum value is 3.
        /// </summary>
        public uint MaxWorkItemDimensions { get { return InteropTools.ReadUInt( this, (uint)DeviceInfo.MAX_WORK_ITEM_DIMENSIONS ); } }
        /// <summary>
        /// Maximum number of work-items that can be specified in each dimension of
        /// the work-group to clEnqueueNDRangeKernel.
        /// 
        /// Returns n size_t entries, where n is the value returned by the query for
        /// CL_DEVICE_MAX_WORK_ITEM_DIMENSIONS.
        /// 
        /// The minimum value is (1, 1, 1).
        /// </summary>
        public IntPtr[] MaxWorkItemSizes { get { return InteropTools.ReadIntPtrArray( this, (uint)DeviceInfo.MAX_WORK_ITEM_SIZES ); } }
        /// <summary>
        /// Maximum number of work-items in a work-group executing a kernel using the data parallel execution model.
        /// (Refer to clEnqueueNDRangeKernel). 
        /// 
        /// The minimum value is 1.
        /// </summary>
        public long MaxWorkGroupSize { get { return InteropTools.ReadIntPtr( this, (uint)DeviceInfo.MAX_WORK_GROUP_SIZE ).ToInt64(); } }
        public uint PreferredVectorWidthChar { get { return InteropTools.ReadUInt( this, (uint)DeviceInfo.PREFERRED_VECTOR_WIDTH_CHAR ); } }
        public uint PreferredVectorWidthShort { get { return InteropTools.ReadUInt( this, (uint)DeviceInfo.PREFERRED_VECTOR_WIDTH_SHORT ); } }
        public uint PreferredVectorWidthInt { get { return InteropTools.ReadUInt( this, (uint)DeviceInfo.PREFERRED_VECTOR_WIDTH_INT ); } }
        public uint PreferredVectorWidthLong { get { return InteropTools.ReadUInt( this, (uint)DeviceInfo.PREFERRED_VECTOR_WIDTH_LONG ); } }
        public uint PreferredVectorWidthFloat { get { return InteropTools.ReadUInt( this, (uint)DeviceInfo.PREFERRED_VECTOR_WIDTH_FLOAT ); } }
        public uint PreferredVectorWidthDouble { get { return InteropTools.ReadUInt(this, (uint)DeviceInfo.PREFERRED_VECTOR_WIDTH_DOUBLE); } }
        public uint PreferredVectorWidthHalf { get { return InteropTools.ReadUInt(this, (uint)DeviceInfo.PREFERRED_VECTOR_WIDTH_HALF); } }
        /// <summary>
        /// Maximum configured clock frequency of the device in MHz.
        /// </summary>
        public uint MaxClockFrequency { get { return InteropTools.ReadUInt( this, (uint)DeviceInfo.MAX_CLOCK_FREQUENCY ); } }
        /// <summary>
        /// The default compute device address space size specified as an unsigned
        /// integer value in bits. Currently supported values are 32 or 64 bits.
        /// </summary>
        public uint AddressBits { get { return InteropTools.ReadUInt( this, (uint)DeviceInfo.ADDRESS_BITS ); } }
        /// <summary>
        /// Max size of memory object allocation in bytes. The minimum value is max
        /// (1/4th of CL_DEVICE_GLOBAL_MEM_SIZE, 128*1024*1024)
        /// </summary>
        public ulong MaxMemAllocSize { get { return InteropTools.ReadULong( this, (uint)DeviceInfo.MAX_MEM_ALLOC_SIZE ); } }
        /// <summary>
        /// Is true if images are supported by the OpenCL device and CL_FALSE otherwise.
        /// </summary>
        public bool ImageSupport { get { return InteropTools.ReadBool( this, (uint)DeviceInfo.IMAGE_SUPPORT ); } }
        /// <summary>
        /// Max number of simultaneous image objects that can be read by a kernel.
        /// The minimum value is 128 if CL_DEVICE_IMAGE_SUPPORT is true.
        /// </summary>
        public uint MaxReadImageArgs { get { return InteropTools.ReadUInt( this, (uint)DeviceInfo.MAX_READ_IMAGE_ARGS ); } }
        /// <summary>
        /// Max number of simultaneous image objects that can be written to by a
        /// kernel. The minimum value is 8 if CL_DEVICE_IMAGE_SUPPORT is true.
        /// </summary>
        public uint MaxWriteImageArgs { get { return InteropTools.ReadUInt(this, (uint)DeviceInfo.MAX_WRITE_IMAGE_ARGS); } }
        /// <summary>
        /// Max width of 2D image in pixels. The minimum value is 8192 if CL_DEVICE_IMAGE_SUPPORT is true.
        /// </summary>
        public long Image2DMaxWidth { get { return InteropTools.ReadIntPtr( this, (uint)DeviceInfo.IMAGE2D_MAX_WIDTH ).ToInt64(); } }
        /// <summary>
        /// Max height of 2D image in pixels. The minimum value is 8192 if CL_DEVICE_IMAGE_SUPPORT is true.
        /// </summary>
        public long Image2DMaxHeight { get { return InteropTools.ReadIntPtr( this, (uint)DeviceInfo.IMAGE2D_MAX_HEIGHT ).ToInt64(); } }
        /// <summary>
        /// Max width of 3D image in pixels. The minimum value is 2048 if CL_DEVICE_IMAGE_SUPPORT is true.
        /// </summary>
        public long Image3DMaxWidth { get { return InteropTools.ReadIntPtr( this, (uint)DeviceInfo.IMAGE3D_MAX_WIDTH ).ToInt64(); } }
        /// <summary>
        /// Max height of 3D image in pixels. The minimum value is 2048 if CL_DEVICE_IMAGE_SUPPORT is true.
        /// </summary>
        public long Image3DMaxHeight { get { return InteropTools.ReadIntPtr( this, (uint)DeviceInfo.IMAGE3D_MAX_HEIGHT ).ToInt64(); } }
        /// <summary>
        /// Max depth of 3D image in pixels. The minimum value is 2048 if CL_DEVICE_IMAGE_SUPPORT is true.
        /// </summary>
        public long Image3DMaxDepth { get { return InteropTools.ReadIntPtr( this, (uint)DeviceInfo.IMAGE3D_MAX_DEPTH ).ToInt64(); } }
        /// <summary>
        /// Maximum number of samplers that can be used in a kernel. Refer to section 6.11.8 for a detailed
        /// description on samplers. The minimum value is 16 if CL_DEVICE_IMAGE_SUPPORT is true.
        /// </summary>
        public uint MaxSamplers { get { return InteropTools.ReadUInt( this, (uint)DeviceInfo.MAX_SAMPLERS ); } }
        /// <summary>
        /// Max size in bytes of the arguments that can be passed to a kernel. The minimum value is 256.
        /// </summary>
        public long MaxParameterSize { get { return InteropTools.ReadIntPtr( this, (uint)DeviceInfo.MAX_PARAMETER_SIZE ).ToInt64(); } }
        /// <summary>
        /// Describes the alignment in bits of the base address of any allocated memory object.
        /// </summary>
        public uint MemBaseAddrAlign { get { return InteropTools.ReadUInt( this, (uint)DeviceInfo.MEM_BASE_ADDR_ALIGN ); } }
        /// <summary>
        /// The smallest alignment in bytes which can be used for any data type.
        /// </summary>
        public uint MinDataTypeAlignSize { get { return InteropTools.ReadUInt( this, (uint)DeviceInfo.MIN_DATA_TYPE_ALIGN_SIZE ); } }
        public ulong SingleFPConfig { get { return InteropTools.ReadULong( this, (uint)DeviceInfo.SINGLE_FP_CONFIG ); } }
        /// <summary>
        /// Type of global memory cache supported. Valid values are: CL_NONE, CL_READ_ONLY_CACHE and CL_READ_WRITE_CACHE.
        /// </summary>
        public DeviceMemCacheType GlobalMemCacheType { get { return (DeviceMemCacheType)InteropTools.ReadUInt( this, (uint)DeviceInfo.GLOBAL_MEM_CACHE_TYPE ); } }
        /// <summary>
        /// Size of global memory cache line in bytes.
        /// </summary>
        public uint GlobalMemCacheLineSize { get { return InteropTools.ReadUInt( this, (uint)DeviceInfo.GLOBAL_MEM_CACHELINE_SIZE ); } }
        /// <summary>
        /// Size of global memory cache in bytes.
        /// </summary>
        public ulong GlobalMemCacheSize { get { return InteropTools.ReadULong( this, (uint)DeviceInfo.GLOBAL_MEM_CACHE_SIZE ); } }
        /// <summary>
        /// Size of global device memory in bytes.
        /// </summary>
        public ulong GlobalMemSize { get { return InteropTools.ReadULong( this, (uint)DeviceInfo.GLOBAL_MEM_SIZE ); } }
        /// <summary>
        /// Max size in bytes of a constant buffer allocation. The minimum value is 64 KB.
        /// </summary>
        public ulong MaxConstantBufferSize { get { return InteropTools.ReadULong( this, (uint)DeviceInfo.MAX_CONSTANT_BUFFER_SIZE ); } }
        /// <summary>
        /// Max number of arguments declared with the __constant qualifier in a kernel. The minimum value is 8.
        /// </summary>
        public uint MaxConstantArgs { get { return InteropTools.ReadUInt( this, (uint)DeviceInfo.MAX_CONSTANT_ARGS ); } }
        /// <summary>
        /// Type of local memory supported. This can be set to CL_LOCAL implying dedicated local memory storage such as SRAM, or CL_GLOBAL.
        /// </summary>
        public DeviceLocalMemType LocalMemType { get { return (DeviceLocalMemType)InteropTools.ReadUInt( this, (uint)DeviceInfo.LOCAL_MEM_TYPE ); } }
        /// <summary>
        /// Size of local memory arena in bytes. The minimum value is 16 KB.
        /// </summary>
        public ulong LocalMemSize { get { return InteropTools.ReadULong( this, (uint)DeviceInfo.LOCAL_MEM_SIZE ); } }
        /// <summary>
        /// Is CL_TRUE if the device implements error correction for the memories,
        /// caches, registers etc. in the device. Is CL_FALSE if the device does not
        /// implement error correction. This can be a requirement for certain clients of OpenCL.
        /// </summary>
        public bool ErrorCorrectionSupport { get { return InteropTools.ReadBool(this, (uint)DeviceInfo.ERROR_CORRECTION_SUPPORT); } }
        /// <summary>
        /// Is CL_TRUE if the device and the host have a unified memory subsystem
        /// and is CL_FALSE otherwise.
        /// </summary>
        public bool HostUnifiedMemory { get { return InteropTools.ReadBool(this, (uint)DeviceInfo.HOST_UNIFIED_MEMORY); } }
        /// <summary>
        /// Describes the resolution of device timer. This is measured in nanoseconds. Refer to section 5.9 for details.
        /// </summary>
        public ulong ProfilingTimerResolution { get { return (ulong)InteropTools.ReadIntPtr( this, (uint)DeviceInfo.PROFILING_TIMER_RESOLUTION ).ToInt64(); } }
        /// <summary>
        /// Is CL_TRUE if the OpenCL device is a little endian device and CL_FALSE otherwise.
        /// </summary>
        public bool EndianLittle { get { return InteropTools.ReadBool( this, (uint)DeviceInfo.ENDIAN_LITTLE ); } }
        /// <summary>
        /// Is CL_TRUE if the device is available and CL_FALSE if the device is not available.
        /// </summary>
        public bool Available { get { return InteropTools.ReadBool( this, (uint)DeviceInfo.AVAILABLE ); } }
        /// <summary>
        /// Is CL_FALSE if the implementation does not have a compiler available to compile the program source.
        /// Is CL_TRUE if the compiler is available.
        /// This can be CL_FALSE for the embededed platform profile only.
        /// </summary>
        public bool CompilerAvailable { get { return InteropTools.ReadBool( this, (uint)DeviceInfo.COMPILER_AVAILABLE ); } }
        /// <summary>
        /// Describes the execution capabilities of the device. This is a bit-field that describes one or more of the following values:
        /// CL_EXEC_KERNEL – The OpenCL device can execute OpenCL kernels.
        /// CL_EXEC_NATIVE_KERNEL – The OpenCL device can execute native kernels.
        /// The mandated minimum capability is: CL_EXEC_KERNEL.
        /// </summary>
        public ulong ExecutionCapabilities { get { return InteropTools.ReadULong( this, (uint)DeviceInfo.EXECUTION_CAPABILITIES ); } }
        /// <summary>
        /// Describes the command-queue properties supported by the device.
        /// This is a bit-field that describes one or more of the following values:
        /// CL_QUEUE_OUT_OF_ORDER_EXEC_MODE_ENABLE
        /// CL_QUEUE_PROFILING_ENABLE
        /// These properties are described in table 5.1.
        /// 
        /// The mandated minimum capability is:
        /// CL_QUEUE_PROFILING_ENABLE.
        /// </summary>
        public ulong QueueProperties { get { return InteropTools.ReadULong( this, (uint)DeviceInfo.QUEUE_PROPERTIES ); } }
        /// <summary>
        /// The platform associated with this device.
        /// </summary>
        public Platform Platform { get; protected set; }
        /// <summary>
        /// Device name string.
        /// </summary>
        public string Name { get { return InteropTools.ReadString( this, (uint)DeviceInfo.NAME ); } }
        /// <summary>
        /// Vendor name string.
        /// </summary>
        public string Vendor { get { return InteropTools.ReadString( this, (uint)DeviceInfo.VENDOR ); } }
        /// <summary>
        /// OpenCL software driver version string in the form major_number.minor_number.
        /// </summary>
        public string DriverVersion { get { return InteropTools.ReadString( this, (uint)DeviceInfo.DRIVER_VERSION ); } }
        /// <summary>
        /// OpenCL profile string. Returns the profile name supported by the device.
        /// The profile name returned can be one of the following strings:
        /// FULL_PROFILE – if the device supports the OpenCL specification (functionality defined as part of the
        /// core specification and does not require any extensions to be supported).
        /// EMBEDDED_PROFILE - if the device supports the OpenCL embedded profile.
        /// </summary>
        public string Profile { get { return InteropTools.ReadString( this, (uint)DeviceInfo.PROFILE ); } }
        /// <summary>
        /// OpenCL version string. Returns the OpenCL version supported by the device. This version string has the
        /// following format:
        /// OpenCL&lt;space&gt;&lt;major_version.minor_version&gt;&lt;space&gt;&lt;vendor-specificinformation&gt;
        /// </summary>
        public string Version { get { return InteropTools.ReadString(this, (uint)DeviceInfo.VERSION); } }
        /// <summary>
        /// OpenCL C version string. Returns the highest OpenCL C version supported
        /// by the compiler for this device. This version string has the following format:
        /// OpenCL&lt;space&gt;C&lt;space&gt;&lt;major_version.minor_version&gt;&lt;space&gt;&lt;vendor-specific information&gt;
        /// </summary>
        public string OpenCL_C_Version { get { return InteropTools.ReadString(this, (uint)DeviceInfo.OPENCL_C_VERSION); } }
        /// <summary>
        /// Returns a space separated list of extension names
        /// (the extension names themselves do not contain any spaces).
        /// The list of extension names returned currently can include one or more of
        /// the following approved extension names:
        /// cl_khr_fp64
        /// cl_khr_select_fprounding_mode
        /// cl_khr_global_int32_base_atomics
        /// cl_khr_global_int32_extended_atomics
        /// cl_khr_local_int32_base_atomics
        /// cl_khr_local_int32_extended_atomics
        /// cl_khr_int64_base_atomics
        /// cl_khr_int64_extended_atomics
        /// cl_khr_3d_image_writes
        /// cl_khr_byte_addressable_store
        /// cl_khr_fp16
        /// cl_khr_gl_sharing
        /// Please refer to the OpenCL specification for a detailed
        /// description of these extensions.
        /// </summary>
        public string Extensions { get { return InteropTools.ReadString( this, (uint)DeviceInfo.EXTENSIONS ); } }

        #endregion

        #region IPropertyContainer Members

        public IntPtr GetPropertySize( uint key )
        {
            IntPtr size;
            ErrorCode result;

            result = (ErrorCode)OpenCL.GetDeviceInfo( DeviceID, key, IntPtr.Zero, null, out size );
            if( result!=ErrorCode.SUCCESS )
                throw new OpenCLException( "Unable to get device info for device "+DeviceID, result);
            return size;
        }

        public void ReadProperty( uint key, IntPtr keyLength, void* pBuffer )
        {
            IntPtr size;
            ErrorCode result;

            result = (ErrorCode)OpenCL.GetDeviceInfo( DeviceID, (uint)key, keyLength, pBuffer, out size );
            if( result!=(int)ErrorCode.SUCCESS )
                throw new OpenCLException( "Unable to get device info for device "+DeviceID, result);
        }

        #endregion

        #region HasExtension

        protected void InitializeExtensionHashSet()
        {
            string[] ext = Extensions.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string s in ext)
                ExtensionHashSet.Add(s);
        }

        public bool HasExtension(string extension)
        {
            return ExtensionHashSet.Contains(extension);
        }

        public bool HasExtensions(string[] extensions)
        {
            foreach (string s in extensions)
                if (!ExtensionHashSet.Contains(s))
                    return false;
            return true;
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine( "Name: "+Name );
            sb.AppendLine( "Vendor: "+Vendor );
            sb.AppendLine( "VendorID: "+VendorID );
            sb.AppendLine( "DriverVersion: "+DriverVersion );
            sb.AppendLine( "Profile: "+Profile );
            sb.AppendLine( "Version: "+Version );
            sb.AppendLine( "Extensions: "+Extensions );
            sb.AppendLine( "DeviceType: "+DeviceType );
            sb.AppendLine( "MaxComputeUnits: "+MaxComputeUnits );
            sb.AppendLine( "MaxWorkItemDimensions: "+MaxWorkItemDimensions );
            sb.Append( "MaxWorkItemSizes:" );
            for( int i=0; i<MaxWorkItemSizes.Length; i++ )
                sb.Append( " "+i+"="+(int)MaxWorkItemSizes[i] );
            sb.AppendLine( "" );
            sb.AppendLine( "MaxWorkGroupSize: "+MaxWorkGroupSize );
            sb.AppendLine( "PreferredVectorWidthChar: "+PreferredVectorWidthChar );
            sb.AppendLine( "PreferredVectorWidthShort: "+PreferredVectorWidthShort );
            sb.AppendLine( "PreferredVectorWidthInt: "+PreferredVectorWidthInt );
            sb.AppendLine( "PreferredVectorWidthLong: "+PreferredVectorWidthLong );
            sb.AppendLine( "PreferredVectorWidthFloat: "+PreferredVectorWidthFloat );
            sb.AppendLine( "PreferredVectorWidthDouble: "+PreferredVectorWidthDouble );
            sb.AppendLine( "MaxClockFrequency: "+MaxClockFrequency );
            sb.AppendLine( "AddressBits: "+AddressBits );
            sb.AppendLine( "MaxMemAllocSize: "+MaxMemAllocSize );
            sb.AppendLine( "ImageSupport: "+ImageSupport );
            sb.AppendLine( "MaxReadImageArgs: "+MaxReadImageArgs );
            sb.AppendLine( "MaxWriteImageArgs: "+MaxWriteImageArgs );
            sb.AppendLine( "Image2DMaxWidth: "+Image2DMaxWidth );
            sb.AppendLine( "Image2DMaxHeight: "+Image2DMaxHeight );
            sb.AppendLine( "Image3DMaxWidth: "+Image3DMaxWidth );
            sb.AppendLine( "Image3DMaxHeight: "+Image3DMaxHeight );
            sb.AppendLine( "Image3DMaxDepth: "+Image3DMaxDepth );
            sb.AppendLine( "MaxSamplers: "+MaxSamplers );
            sb.AppendLine( "MaxParameterSize: "+MaxParameterSize );
            sb.AppendLine( "MemBaseAddrAlign: "+MemBaseAddrAlign );
            sb.AppendLine( "MinDataTypeAlignSize: "+MinDataTypeAlignSize );
            sb.AppendLine( "SingleFPConfig: "+SingleFPConfig );
            sb.AppendLine( "GlobalMemCacheType: "+GlobalMemCacheType );
            sb.AppendLine( "GlobalMemCacheLineSize: "+GlobalMemCacheLineSize );
            sb.AppendLine( "GlobalMemCacheSize: "+GlobalMemCacheSize );
            sb.AppendLine( "GlobalMemSize: "+GlobalMemSize );
            sb.AppendLine( "MaxConstantBufferSize: "+MaxConstantBufferSize );
            sb.AppendLine( "MaxConstantArgs: "+MaxConstantArgs );
            sb.AppendLine( "LocalMemType: "+LocalMemType );
            sb.AppendLine( "LocalMemSize: "+LocalMemSize );
            sb.AppendLine( "ErrorCorrectionSupport: "+ErrorCorrectionSupport );
            sb.AppendLine( "ProfilingTimerResolution: "+ProfilingTimerResolution );
            sb.AppendLine( "EndianLittle: "+EndianLittle );
            sb.AppendLine( "Available: "+Available );
            sb.AppendLine( "CompilerAvailable: "+CompilerAvailable );
            sb.AppendLine( "ExecutionCapabilities: "+ExecutionCapabilities );
            sb.AppendLine( "QueueProperties: "+QueueProperties );
            return sb.ToString();
        }

        #endregion

    }
}