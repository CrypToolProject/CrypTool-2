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
using System.IO;

namespace OpenCLNet
{

    unsafe public class Context : IDisposable, InteropTools.IPropertyContainer
    {
        public IntPtr ContextID { get; protected set; }
        public Platform Platform { get; protected set; }

        /// <summary>
        /// True if there is at least one 64 bit device in the context.
        /// This guarantees that variables such as intptr_t, size_t etc are 64 bit
        /// </summary>
        public bool Is64BitContext { get; protected set; }

        // Track whether Dispose has been called.
        private bool disposed = false;

        #region Construction / Destruction

        internal Context( Platform platform, IntPtr contextID )
        {
            Platform = platform;
            ContextID = contextID;
            Is64BitContext = ContainsA64BitDevice();
        }

        internal Context( Platform platform, ContextProperties[] properties, Device[] devices )
        {
            IntPtr[] intPtrProperties;
            IntPtr[] deviceIDs;
            ErrorCode result;

            Platform = platform;
            deviceIDs = InteropTools.ConvertDevicesToDeviceIDs( devices );

            intPtrProperties = new IntPtr[properties.Length];
            for( int i=0; i<properties.Length; i++ )
                intPtrProperties[i] = new IntPtr( (long)properties[i] );

            ContextID = (IntPtr)OpenCL.CreateContext( intPtrProperties,
                (uint)devices.Length,
                deviceIDs,
                null,
                IntPtr.Zero,
                out result );
            if( result!=ErrorCode.SUCCESS )
                throw new OpenCLException( "CreateContext failed: "+result , result);
            Is64BitContext = ContainsA64BitDevice();
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~Context()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose( false );
        }

        protected bool ContainsA64BitDevice()
        {
            for (int i = 0; i < Devices.Length; i++)
                if (Devices[i].AddressBits == 64)
                    return true;
            return false;
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
                OpenCL.ReleaseContext( ContextID );
                ContextID = IntPtr.Zero;

                // Note disposing has been done.
                disposed = true;
            }
        }


        #endregion

        #region Properties

        public uint ReferenceCount { get { return InteropTools.ReadUInt( this, (uint)ContextInfo.REFERENCE_COUNT ); } }

        public Device[] Devices
        {
            get
            {
                IntPtr contextDevicesSize;
                ErrorCode result;
                IntPtr[] contextDevices;

                result = (ErrorCode)OpenCL.GetContextInfo( ContextID, (uint)ContextInfo.DEVICES, IntPtr.Zero, null, out contextDevicesSize );
                if( result!=ErrorCode.SUCCESS )
                    throw new OpenCLException( "Unable to get context info for context "+ContextID+" "+result, result);

                contextDevices = new IntPtr[contextDevicesSize.ToInt64()/sizeof( IntPtr )];
                fixed( IntPtr* pContextDevices = contextDevices )
                {
                    result = (ErrorCode)OpenCL.GetContextInfo( ContextID, (uint)ContextInfo.DEVICES, contextDevicesSize, (void*)pContextDevices, out contextDevicesSize );
                    if( result!=ErrorCode.SUCCESS )
                        throw new OpenCLException( "Unable to get context info for context "+ContextID+" "+result, result);
                }
                return InteropTools.ConvertDeviceIDsToDevices( Platform, contextDevices );
            }
        }

        public ContextProperties[] Properties
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region Create Command Queue

        public CommandQueue CreateCommandQueue(Device device)
        {
            return CreateCommandQueue(device, (CommandQueueProperties)0);
        }

        public CommandQueue CreateCommandQueue( Device device, CommandQueueProperties properties )
        {
            IntPtr commandQueueID;
            ErrorCode result;

            commandQueueID = (IntPtr)OpenCL.CreateCommandQueue( ContextID, device.DeviceID, (ulong)properties, out result );
            if( result!=ErrorCode.SUCCESS )
                throw new OpenCLException( "CreateCommandQueue failed with error code "+result, result);
            return new CommandQueue( this, device, commandQueueID );
        }

        #endregion

        #region Create Buffer

        public Mem CreateBuffer(MemFlags flags, long size)
        {
            return CreateBuffer(flags, size, IntPtr.Zero);
        }

        public Mem CreateBuffer(MemFlags flags, long size, IntPtr pHost)
        {
            return CreateBuffer(flags, size, pHost.ToPointer());
        }

        public Mem CreateBuffer( MemFlags flags, long size, void* pHost )
        {
            IntPtr memID;
            ErrorCode result;

            memID = (IntPtr)OpenCL.CreateBuffer( ContextID, (ulong)flags, new IntPtr(size), pHost, out result );
            if( result!=ErrorCode.SUCCESS )
                throw new OpenCLException( "CreateBuffer failed with error code "+result, result);
            return new Mem( this, memID );
        }

        #endregion

        #region GL Interop

        public Mem CreateFromGLBuffer(MemFlags flags, IntPtr bufobj)
        {
            IntPtr memID;
            ErrorCode result;

            memID = OpenCL.CreateFromGLBuffer(ContextID, (ulong)flags, (uint)bufobj, out result);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("CreateFromGLBuffer failed with error code " + result, result);
            return new Mem(this, memID);
        }

        public Mem CreateFromGLTexture2D(MemFlags flags, int target, int mipLevel, int texture)
        {
            IntPtr memID;
            ErrorCode result;

            memID = OpenCL.CreateFromGLTexture2D(ContextID, (ulong)flags, target, mipLevel, (uint)texture, out result);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("CreateFromGLTexture2D failed with error code " + result, result);
            return new Mem(this, memID);
        }

        public Mem CreateFromGLTexture3D(MemFlags flags, int target, int mipLevel, int texture)
        {
            IntPtr memID;
            ErrorCode result;

            memID = OpenCL.CreateFromGLTexture3D(ContextID, (ulong)flags, target, mipLevel, (uint)texture, out result);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("CreateFromGLTexture3D failed with error code " + result, result);
            return new Mem(this, memID);
        }

        public Mem CreateFromGLRenderbuffer(MemFlags flags, IntPtr renderbuffer)
        {
            IntPtr memID;
            ErrorCode result;

            memID = OpenCL.CreateFromGLRenderbuffer(ContextID, (ulong)flags, (uint)renderbuffer, out result);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("CreateFromGLTexture3D failed with error code " + result, result);
            return new Mem(this, memID);
        }

        #endregion

        #region Create Program

        public Program CreateProgramFromFile(string path)
        {
            return CreateProgramWithSource(File.ReadAllText(path));
        }

        public Program CreateProgramWithSource( string source )
        {
            return CreateProgramWithSource( new string[] { source } );
        }

        public Program CreateProgramWithSource( string[] source )
        {
            IntPtr programID;
            ErrorCode result;

            programID = (IntPtr)OpenCL.CreateProgramWithSource( ContextID, (uint)source.Length, source, (IntPtr[])null, out result );
            if( result!=ErrorCode.SUCCESS )
                throw new OpenCLException( "CreateProgramWithSource failed with error code "+result, result);
            return new Program( this, programID );
        }

        public Program CreateProgramWithBinary( Device[] devices, byte[][] binaries, ErrorCode[] binaryStatus )
        {
            IntPtr programID; 
            ErrorCode result;
            IntPtr[] lengths;
            int[] binStatus = new int[binaryStatus.Length];

            lengths = new IntPtr[devices.Length];
            for (int i = 0; i < lengths.Length; i++)
                lengths[i] = (IntPtr)binaries[i].Length;
            programID = OpenCL.CreateProgramWithBinary(ContextID,
                (uint)devices.Length,
                InteropTools.ConvertDevicesToDeviceIDs(devices),
                lengths,
                binaries,
                binStatus,
                out result );
            for( int i=0; i<binaryStatus.Length; i++ )
                binaryStatus[i] = (ErrorCode)binStatus[i];
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("CreateProgramWithBinary failed with error code " + result, result);
            return new Program(this, programID);
        }

        #endregion

        #region Create Sampler

        public Sampler CreateSampler( bool normalizedCoords, AddressingMode addressingMode, FilterMode filterMode )
        {
            IntPtr samplerID;
            ErrorCode result;
            
            samplerID = OpenCL.CreateSampler( ContextID, normalizedCoords, (uint)addressingMode, (uint)filterMode, out result );
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("CreateSampler failed with error code " + result, result);
            return new Sampler(this, samplerID);
        }

        #endregion

        #region Image2D

        public Image CreateImage2D(MemFlags flags, ImageFormat imageFormat, int imageWidth, int imageHeight)
        {
            return CreateImage2D(flags, imageFormat, (IntPtr)imageWidth, (IntPtr)imageHeight, IntPtr.Zero, IntPtr.Zero);
        }
        public Image CreateImage2D(MemFlags flags, ImageFormat imageFormat, long imageWidth, long imageHeight)
        {
            return CreateImage2D(flags, imageFormat, (IntPtr)imageWidth, (IntPtr)imageHeight, IntPtr.Zero, IntPtr.Zero);
        }

        public Image CreateImage2D(MemFlags flags, ImageFormat imageFormat, int imageWidth, int imageHeight, int imageRowPitch, IntPtr pHost)
        {
            return CreateImage2D(flags, imageFormat, (IntPtr)imageWidth, (IntPtr)imageHeight, (IntPtr)imageRowPitch, pHost);
        }
        public Image CreateImage2D(MemFlags flags, ImageFormat imageFormat, long imageWidth, long imageHeight, long imageRowPitch, IntPtr pHost)
        {
            return CreateImage2D(flags, imageFormat, (IntPtr)imageWidth, (IntPtr)imageHeight, (IntPtr)imageRowPitch, pHost);
        }

        public Image CreateImage2D(MemFlags flags, ImageFormat imageFormat, IntPtr imageWidth, IntPtr imageHeight, IntPtr imageRowPitch, IntPtr pHost)
        {
            IntPtr memID;
            ErrorCode result;

            memID = (IntPtr)OpenCL.CreateImage2D(ContextID, (ulong)flags, imageFormat, imageWidth, imageHeight, imageRowPitch, pHost.ToPointer(), out result);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("CreateImage2D failed with error code " + result, result);
            return new Image(this, memID);
        }

        #endregion

        #region Image3D

        public Image CreateImage3D(MemFlags flags, ImageFormat imageFormat, int imageWidth, int imageHeight, int imageDepth, int imageRowPitch, int imageSlicePitch)
        {
            return CreateImage3D(flags, imageFormat, (IntPtr)imageWidth, (IntPtr)imageHeight, (IntPtr)imageDepth, (IntPtr)imageRowPitch, (IntPtr)imageSlicePitch, IntPtr.Zero);
        }

        public Image CreateImage3D(MemFlags flags, ImageFormat imageFormat, int imageWidth, int imageHeight, int imageDepth, int imageRowPitch, int imageSlicePitch, IntPtr pHost)
        {
            return CreateImage3D(flags, imageFormat, (IntPtr)imageWidth, (IntPtr)imageHeight, (IntPtr)imageDepth, (IntPtr)imageRowPitch, (IntPtr)imageSlicePitch, pHost);
        }

        public Image CreateImage3D(MemFlags flags, ImageFormat imageFormat, IntPtr imageWidth, IntPtr imageHeight, IntPtr imageDepth, IntPtr imageRowPitch, IntPtr imageSlicePitch, IntPtr pHost)
        {
            IntPtr memID;
            ErrorCode result;

            memID = (IntPtr)OpenCL.CreateImage3D(ContextID, (ulong)flags, imageFormat, imageWidth, imageHeight, imageDepth, imageRowPitch, imageSlicePitch, pHost.ToPointer(), out result);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("CreateImage3D failed with error code " + result, result);
            return new Image(this, memID);
        }

        #endregion

        #region Image format queries

        /// <summary>
        /// Query which ImageFormats are supported by this context
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public ImageFormat[] GetSupportedImageFormats(MemFlags flags, MemObjectType type)
        {
            uint numImageFormats;
            ImageFormat[] imageFormats;
            ErrorCode result;

            result = OpenCL.GetSupportedImageFormats(ContextID, (ulong)flags, (uint)type, (uint)0, null, out numImageFormats);
            if( result!=ErrorCode.SUCCESS )
                throw new OpenCLException("GetSupportedImageFormats failed with error code " + result, result);

            imageFormats = new ImageFormat[numImageFormats];

            result = OpenCL.GetSupportedImageFormats(ContextID, (ulong)flags, (uint)type, numImageFormats, imageFormats, out numImageFormats);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("GetSupportedImageFormats failed with error code " + result, result);
            
            return imageFormats;
        }

        /// <summary>
        /// Convenience function. Checks if a context supports a specific image format
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="type"></param>
        /// <param name="channelOrder"></param>
        /// <param name="channelType"></param>
        /// <returns>true if the image format is supported, false otherwise</returns>
        public bool SupportsImageFormat(MemFlags flags, MemObjectType type, ChannelOrder channelOrder, ChannelType channelType)
        {
            ImageFormat[] imageFormats = GetSupportedImageFormats(flags, type);
            foreach (ImageFormat imageFormat in imageFormats)
            {
                if (imageFormat.ChannelOrder == channelOrder && imageFormat.ChannelType == channelType)
                    return true;
            }
            return false;
        }

        #endregion

        #region WaitForEvents

        /// <summary>
        /// Block until the event fires
        /// </summary>
        /// <param name="_event"></param>
        public void WaitForEvent(Event _event)
        {
            Event[] event_list = new Event[1];

            event_list[0] = _event;
            OpenCL.WaitForEvents(1, InteropTools.ConvertEventsToEventIDs(event_list));
        }

        /// <summary>
        /// Block until all events in the array have fired
        /// </summary>
        /// <param name="num_events"></param>
        /// <param name="event_list"></param>
        public void WaitForEvents(int num_events, Event[] event_list)
        {
            OpenCL.WaitForEvents((uint)num_events, InteropTools.ConvertEventsToEventIDs(event_list));
        }

        #endregion

        #region HasExtension

        public bool HasExtension(string extension)
        {
            foreach (Device d in Devices)
                if (!d.HasExtension(extension))
                    return false;
            return true;
        }

        public bool HasExtensions(string[] extensions)
        {
            foreach (Device d in Devices)
                if (!d.HasExtensions(extensions))
                    return false;
            return true;
        }

        #endregion

        #region User Events
        /// <summary>
        /// OpenCL 1.1
        /// </summary>
        /// <returns></returns>
        public Event CreateUserEvent()
        {
            ErrorCode result;
            IntPtr eventID;

            eventID = OpenCL.CreateUserEvent(ContextID, out result);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("CreateUserEvent failed with error code " + result, result);
            return new Event(this, null, eventID);
        }

        #endregion

        #region Casts

        public static implicit operator IntPtr( Context c )
        {
            return c.ContextID;
        }

        #endregion

        #region IPropertyContainer Members

        unsafe public IntPtr GetPropertySize( uint key )
        {
            IntPtr size;
            ErrorCode result;

            result = (ErrorCode)OpenCL.GetContextInfo( ContextID, key, IntPtr.Zero, null, out size );
            if( result!=ErrorCode.SUCCESS )
                throw new OpenCLException( "GetContextInfo failed: "+result, result);
            return size;
        }

        unsafe public void ReadProperty( uint key, IntPtr keyLength, void* pBuffer )
        {
            IntPtr size;
            ErrorCode result;

            result = (ErrorCode)OpenCL.GetContextInfo( ContextID, key, keyLength, pBuffer, out size );
            if( result!=ErrorCode.SUCCESS )
                throw new OpenCLException( "GetContextInfo failed: "+result, result);
        }

        #endregion
    }
}
