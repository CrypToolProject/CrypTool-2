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
using System.Threading;

namespace OpenCLNet
{
    /// <summary>
    /// The CommandQueue class wraps an OpenCL command queue reference.
    /// 
    /// This class contains methods that correspond to all OpenCL functions that take
    /// a command queue as their first parameter. Most notably, all the Enqueue() functions.
    /// In effect, it makes this class into the workhorse of most OpenCL applications.
    /// </summary>
    unsafe public class CommandQueue : IDisposable
    {
        public IntPtr CommandQueueID { get; private set; }
        public Context Context { get; private set; }
        public Device Device { get; private set; }
        public uint ReferenceCount { get { return 0; } }
        public CommandQueueProperties Properties { get { return (CommandQueueProperties)0; } }

        // Track whether Dispose has been called.
        private bool disposed = false;

        #region Construction / Destruction

        internal CommandQueue( Context context, Device device, IntPtr commandQueueID )
        {
            Context = context;
            Device = device;
            CommandQueueID = commandQueueID;
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~CommandQueue()
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
                OpenCL.ReleaseCommandQueue( CommandQueueID );
                CommandQueueID = IntPtr.Zero;

                // Note disposing has been done.
                disposed = true;
            }
        }

        #endregion

        // Enqueue methods follow. Typically, each will have 3 versions.
        // One which takes an event wait list and and event output
        // One which takes an event wait list
        // and one which takes neither
        // There are also overloads which take int, long and IntPtr arguments

        #region EnqueueWriteBuffer

        /// <summary>
        /// Enqueues a command to write data to a buffer object identified by buffer from host memory identified by ptr.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="blockingWrite"></param>
        /// <param name="offset"></param>
        /// <param name="cb"></param>
        /// <param name="ptr"></param>
        /// <param name="event_wait_list"></param>
        /// <param name="InteropTools.ConvertEventsToEventIDs(event_wait_list)"></param>
        /// <param name="_event"></param>
        public void EnqueueWriteBuffer(Mem buffer, bool blockingWrite, IntPtr offset, IntPtr cb, IntPtr ptr, int numEventsInWaitList, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;

            result = OpenCL.EnqueueWriteBuffer(CommandQueueID,
                buffer.MemID,
                (uint)(blockingWrite ? Bool.TRUE : Bool.FALSE),
                offset,
                cb,
                ptr.ToPointer(),
                (uint)numEventsInWaitList,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                &tmpEvent);
            _event = new Event( Context, this, tmpEvent );
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueWriteBuffer failed with error code " + result, result);
        }
        public void EnqueueWriteBuffer(Mem buffer, bool blockingWrite, int offset, int cb, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);
            result = OpenCL.EnqueueWriteBuffer(CommandQueueID,
                buffer,
                (uint)(blockingWrite ? Bool.TRUE : Bool.FALSE),
                offset,
                cb,
                ptr,
                num_events_in_wait_list,
                repackedEvents,
                &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueWriteBuffer failed with error code " + result, result);
        }
        public void EnqueueWriteBuffer(Mem buffer, bool blockingWrite, long offset, long cb, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);
            result = OpenCL.EnqueueWriteBuffer(CommandQueueID,
                buffer,
                (uint)(blockingWrite ? Bool.TRUE : Bool.FALSE),
                offset,
                cb,
                ptr,
                num_events_in_wait_list,
                repackedEvents,
                &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueWriteBuffer failed with error code " + result, result);
        }

        public void EnqueueWriteBuffer(Mem buffer, bool blockingWrite, IntPtr offset, IntPtr cb, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;

            result = OpenCL.EnqueueWriteBuffer(CommandQueueID,
                buffer.MemID,
                (uint)(blockingWrite ? Bool.TRUE : Bool.FALSE),
                offset,
                cb,
                ptr.ToPointer(),
                (uint)num_events_in_wait_list,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueWriteBuffer failed with error code " + result, result);
        }
        public void EnqueueWriteBuffer(Mem buffer, bool blockingWrite, int offset, int cb, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);
            result = OpenCL.EnqueueWriteBuffer(CommandQueueID,
                buffer,
                (uint)(blockingWrite ? Bool.TRUE : Bool.FALSE),
                offset,
                cb,
                ptr,
                num_events_in_wait_list,
                repackedEvents,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueWriteBuffer failed with error code " + result, result);
        }
        public void EnqueueWriteBuffer(Mem buffer, bool blockingWrite, long offset, long cb, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);
            result = OpenCL.EnqueueWriteBuffer(CommandQueueID,
                buffer,
                (uint)(blockingWrite ? Bool.TRUE : Bool.FALSE),
                offset,
                cb,
                ptr,
                num_events_in_wait_list,
                repackedEvents,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueWriteBuffer failed with error code " + result, result);
        }

        public void EnqueueWriteBuffer(Mem buffer, bool blockingWrite, IntPtr offset, IntPtr cb, IntPtr ptr)
        {
            ErrorCode result;

            result = OpenCL.EnqueueWriteBuffer(CommandQueueID,
                buffer.MemID,
                (uint)(blockingWrite ? Bool.TRUE : Bool.FALSE),
                offset,
                cb,
                ptr.ToPointer(),
                (uint)0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReadBuffer failed with error code " + result, result);
        }
        public void EnqueueWriteBuffer(Mem buffer, bool blockingWrite, int offset, int cb, IntPtr ptr)
        {
            ErrorCode result;

            result = OpenCL.EnqueueWriteBuffer(CommandQueueID,
                buffer,
                (uint)(blockingWrite ? Bool.TRUE : Bool.FALSE),
                offset,
                cb,
                ptr,
                0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueWriteBuffer failed with error code " + result, result);
        }
        public void EnqueueWriteBuffer(Mem buffer, bool blockingWrite, long offset, long cb, IntPtr ptr)
        {
            ErrorCode result;

            result = OpenCL.EnqueueWriteBuffer(CommandQueueID,
                buffer,
                (uint)(blockingWrite ? Bool.TRUE : Bool.FALSE),
                offset,
                cb,
                ptr,
                0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueWriteBuffer failed with error code " + result, result);
        }

        #endregion

        #region EnqueueReadBuffer

        /// <summary>
        /// Enqueues a command to read data from a buffer object identified by buffer to host memory identified by ptr.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="blockingRead"></param>
        /// <param name="offset"></param>
        /// <param name="cb"></param>
        /// <param name="ptr"></param>
        /// <param name="numEventsInWaitList"></param>
        /// <param name="event_wait_list"></param>
        /// <param name="_event"></param>
        public void EnqueueReadBuffer(Mem buffer, bool blockingRead, IntPtr offset, IntPtr cb, IntPtr ptr, int numEventsInWaitList, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;

            result = OpenCL.EnqueueReadBuffer(CommandQueueID,
                buffer.MemID,
                (uint)(blockingRead ? Bool.TRUE : Bool.FALSE),
                offset,
                cb,
                ptr.ToPointer(),
                (uint)numEventsInWaitList,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                &tmpEvent);
            _event = new Event(Context, this, tmpEvent); 
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReadBuffer failed with error code " + result, result);
        }
        public void EnqueueReadBuffer(Mem buffer, bool blockingRead, int offset, int cb, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);
            result = OpenCL.EnqueueReadBuffer(CommandQueueID,
                buffer,
                (uint)(blockingRead ? Bool.TRUE : Bool.FALSE),
                offset,
                cb,
                ptr,
                num_events_in_wait_list,
                repackedEvents,
                &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReadBuffer failed with error code " + result, result);
        }
        public void EnqueueReadBuffer(Mem buffer, bool blockingRead, long offset, long cb, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);
            result = OpenCL.EnqueueReadBuffer(CommandQueueID,
                buffer,
                (uint)(blockingRead ? Bool.TRUE : Bool.FALSE),
                offset,
                cb,
                ptr,
                num_events_in_wait_list,
                repackedEvents,
                &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReadBuffer failed with error code " + result, result);
        }

        public void EnqueueReadBuffer(Mem buffer, bool blockingRead, IntPtr offset, IntPtr cb, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;

            result = OpenCL.EnqueueReadBuffer(CommandQueueID,
                buffer.MemID,
                (uint)(blockingRead ? Bool.TRUE : Bool.FALSE),
                offset,
                cb,
                ptr.ToPointer(),
                (uint)num_events_in_wait_list,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReadBuffer failed with error code " + result, result);
        }
        public void EnqueueReadBuffer(Mem buffer, bool blockingRead, int offset, int cb, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);
            result = OpenCL.EnqueueReadBuffer(CommandQueueID,
                buffer,
                (uint)(blockingRead ? Bool.TRUE : Bool.FALSE),
                offset,
                cb,
                ptr,
                num_events_in_wait_list,
                repackedEvents,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReadBuffer failed with error code " + result, result);
        }
        public void EnqueueReadBuffer(Mem buffer, bool blockingRead, long offset, long cb, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);
            result = OpenCL.EnqueueReadBuffer(CommandQueueID,
                buffer,
                (uint)(blockingRead ? Bool.TRUE : Bool.FALSE),
                offset,
                cb,
                ptr,
                num_events_in_wait_list,
                repackedEvents,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReadBuffer failed with error code " + result, result);
        }

        public void EnqueueReadBuffer(Mem buffer, bool blockingRead, IntPtr offset, IntPtr cb, IntPtr ptr)
        {
            ErrorCode result;

            result = OpenCL.EnqueueReadBuffer(CommandQueueID,
                buffer.MemID,
                (uint)(blockingRead ? Bool.TRUE : Bool.FALSE),
                offset,
                cb,
                ptr.ToPointer(),
                (uint)0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReadBuffer failed with error code " + result, result);
        }
        public void EnqueueReadBuffer(Mem buffer, bool blockingRead, int offset, int cb, IntPtr ptr)
        {
            ErrorCode result;

            result = OpenCL.EnqueueReadBuffer(CommandQueueID,
                buffer,
                (uint)(blockingRead ? Bool.TRUE : Bool.FALSE),
                offset,
                cb,
                ptr,
                0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReadBuffer failed with error code " + result, result);
        }
        public void EnqueueReadBuffer(Mem buffer, bool blockingRead, long offset, long cb, IntPtr ptr)
        {
            ErrorCode result;

            result = OpenCL.EnqueueReadBuffer(CommandQueueID,
                buffer,
                (uint)(blockingRead ? Bool.TRUE : Bool.FALSE),
                offset,
                cb,
                ptr,
                0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReadBuffer failed with error code " + result, result);
        }

        #endregion

        #region EnqueueCopyBuffer

        /// <summary>
        /// Enqueues a command to copy a buffer object identified by src_buffer to another buffer object identified by dst_buffer.
        /// </summary>
        /// <param name="src_buffer"></param>
        /// <param name="dst_buffer"></param>
        /// <param name="src_offset"></param>
        /// <param name="dst_offset"></param>
        /// <param name="cb"></param>
        /// <param name="num_events_in_wait_list"></param>
        /// <param name="event_wait_list"></param>
        /// <param name="_event"></param>
        public void EnqueueCopyBuffer(Mem src_buffer, Mem dst_buffer, IntPtr src_offset, IntPtr dst_offset, IntPtr cb, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;

            result = OpenCL.EnqueueCopyBuffer(CommandQueueID,
                src_buffer.MemID,
                dst_buffer.MemID,
                src_offset,
                dst_offset,
                cb,
                (uint)num_events_in_wait_list,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyBuffer failed with error code " + result, result);
        }
        public void EnqueueCopyBuffer(Mem src_buffer, Mem dst_buffer, int src_offset, int dst_offset, int cb, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            EnqueueCopyBuffer(src_buffer, dst_buffer, (IntPtr)src_offset, (IntPtr)dst_offset, (IntPtr)cb, num_events_in_wait_list, event_wait_list, out _event);
        }
        public void EnqueueCopyBuffer(Mem src_buffer, Mem dst_buffer, long src_offset, long dst_offset, long cb, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            EnqueueCopyBuffer(src_buffer, dst_buffer, (IntPtr)src_offset, (IntPtr)dst_offset, (IntPtr)cb, num_events_in_wait_list, event_wait_list, out _event);
        }

        public void EnqueueCopyBuffer(Mem src_buffer, Mem dst_buffer, IntPtr src_offset, IntPtr dst_offset, IntPtr cb, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;

            result = OpenCL.EnqueueCopyBuffer(CommandQueueID,
                src_buffer.MemID,
                dst_buffer.MemID,
                src_offset,
                dst_offset,
                cb,
                (uint)num_events_in_wait_list,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyBuffer failed with error code " + result, result);
        }
        public void EnqueueCopyBuffer(Mem src_buffer, Mem dst_buffer, int src_offset, int dst_offset, int cb, int num_events_in_wait_list, Event[] event_wait_list)
        {
            EnqueueCopyBuffer(src_buffer, dst_buffer, (IntPtr)src_offset, (IntPtr)dst_offset, (IntPtr)cb, num_events_in_wait_list, event_wait_list);
        }
        public void EnqueueCopyBuffer(Mem src_buffer, Mem dst_buffer, long src_offset, long dst_offset, long cb, int num_events_in_wait_list, Event[] event_wait_list)
        {
            EnqueueCopyBuffer(src_buffer, dst_buffer, (IntPtr)src_offset, (IntPtr)dst_offset, (IntPtr)cb, num_events_in_wait_list, event_wait_list);
        }

        public void EnqueueCopyBuffer(Mem src_buffer, Mem dst_buffer, IntPtr src_offset, IntPtr dst_offset, IntPtr cb)
        {
            ErrorCode result;

            result = OpenCL.EnqueueCopyBuffer(CommandQueueID,
                src_buffer.MemID,
                dst_buffer.MemID,
                src_offset,
                dst_offset,
                cb,
                (uint)0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyBuffer failed with error code " + result, result);
        }
        public void EnqueueCopyBuffer(Mem src_buffer, Mem dst_buffer, int src_offset, int dst_offset, int cb)
        {
            EnqueueCopyBuffer(src_buffer, dst_buffer, (IntPtr)src_offset, (IntPtr)dst_offset, (IntPtr)cb);
        }
        public void EnqueueCopyBuffer(Mem src_buffer, Mem dst_buffer, long src_offset, long dst_offset, long cb)
        {
            EnqueueCopyBuffer(src_buffer, dst_buffer, (IntPtr)src_offset, (IntPtr)dst_offset, (IntPtr)cb);
        }

        #endregion

        #region EnqueueReadImage

        public void EnqueueReadImage(Mem image, bool blockingRead, IntPtr[] origin, IntPtr[] region, IntPtr row_pitch, IntPtr slice_pitch, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;

            result = OpenCL.EnqueueReadImage(CommandQueueID,
                image.MemID,
                (uint)(blockingRead ? Bool.TRUE : Bool.FALSE),
                origin,
                region,
                row_pitch,
                slice_pitch,
                ptr.ToPointer(),
                (uint)num_events_in_wait_list,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReadImage failed with error code " + result, result);
        }
        public void EnqueueReadImage(Mem image, bool blockingRead, int[] origin, int[] region, int row_pitch, int slice_pitch, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;
            IntPtr* repackedOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.A3ToIntPtr3(origin, repackedOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);

            result = OpenCL.EnqueueReadImage(CommandQueueID,
                image.MemID,
                (uint)(blockingRead ? Bool.TRUE : Bool.FALSE),
                repackedOrigin,
                repackedRegion,
                row_pitch,
                slice_pitch,
                ptr,
                num_events_in_wait_list,
                repackedEvents,
                &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReadImage failed with error code " + result, result);
        }
        public void EnqueueReadImage(Mem image, bool blockingRead, long[] origin, long[] region, long row_pitch, long slice_pitch, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;
            IntPtr* repackedOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.A3ToIntPtr3(origin, repackedOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);

            result = OpenCL.EnqueueReadImage(CommandQueueID,
                image.MemID,
                (uint)(blockingRead ? Bool.TRUE : Bool.FALSE),
                repackedOrigin,
                repackedRegion,
                row_pitch,
                slice_pitch,
                ptr,
                num_events_in_wait_list,
                repackedEvents,
                &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReadImage failed with error code " + result, result);
        }

        public void EnqueueReadImage(Mem image, bool blockingRead, IntPtr[] origin, IntPtr[] region, IntPtr row_pitch, IntPtr slice_pitch, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;

            result = OpenCL.EnqueueReadImage(CommandQueueID,
                image.MemID,
                (uint)(blockingRead ? Bool.TRUE : Bool.FALSE),
                origin,
                region,
                row_pitch,
                slice_pitch,
                ptr.ToPointer(),
                (uint)num_events_in_wait_list,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReadImage failed with error code " + result, result);
        }
        public void EnqueueReadImage(Mem image, bool blockingRead, int[] origin, int[] region, int row_pitch, int slice_pitch, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;
            IntPtr* repackedOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.A3ToIntPtr3(origin, repackedOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);

            result = OpenCL.EnqueueReadImage(CommandQueueID,
                image.MemID,
                (uint)(blockingRead ? Bool.TRUE : Bool.FALSE),
                repackedOrigin,
                repackedRegion,
                row_pitch,
                slice_pitch,
                ptr,
                num_events_in_wait_list,
                repackedEvents,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReadImage failed with error code " + result, result);
        }
        public void EnqueueReadImage(Mem image, bool blockingRead, long[] origin, long[] region, long row_pitch, long slice_pitch, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;
            IntPtr* repackedOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.A3ToIntPtr3(origin, repackedOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);

            result = OpenCL.EnqueueReadImage(CommandQueueID,
                image.MemID,
                (uint)(blockingRead ? Bool.TRUE : Bool.FALSE),
                repackedOrigin,
                repackedRegion,
                row_pitch,
                slice_pitch,
                ptr,
                num_events_in_wait_list,
                repackedEvents,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReadImage failed with error code " + result, result);
        }

        public void EnqueueReadImage(Mem image, bool blockingRead, IntPtr[] origin, IntPtr[] region, IntPtr row_pitch, IntPtr slice_pitch, IntPtr ptr)
        {
            ErrorCode result;

            result = OpenCL.EnqueueReadImage(CommandQueueID,
                image.MemID,
                (uint)(blockingRead ? Bool.TRUE : Bool.FALSE),
                origin,
                region,
                row_pitch,
                slice_pitch,
                ptr.ToPointer(),
                (uint)0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReadImage failed with error code " + result, result);
        }
        public void EnqueueReadImage(Mem image, bool blockingRead, int[] origin, int[] region, int row_pitch, int slice_pitch, IntPtr ptr)
        {
            ErrorCode result;
            IntPtr* repackedOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];

            InteropTools.A3ToIntPtr3(origin, repackedOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);

            result = OpenCL.EnqueueReadImage(CommandQueueID,
                image.MemID,
                (uint)(blockingRead ? Bool.TRUE : Bool.FALSE),
                repackedOrigin,
                repackedRegion,
                row_pitch,
                slice_pitch,
                ptr,
                0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReadImage failed with error code " + result, result);
        }
        public void EnqueueReadImage(Mem image, bool blockingRead, long[] origin, long[] region, long row_pitch, long slice_pitch, IntPtr ptr)
        {
            ErrorCode result;
            IntPtr* repackedOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];

            InteropTools.A3ToIntPtr3(origin, repackedOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);

            result = OpenCL.EnqueueReadImage(CommandQueueID,
                image.MemID,
                (uint)(blockingRead ? Bool.TRUE : Bool.FALSE),
                repackedOrigin,
                repackedRegion,
                row_pitch,
                slice_pitch,
                ptr,
                0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReadImage failed with error code " + result, result);
        }

        #endregion

        #region EnqueueWriteImage

        public void EnqueueWriteImage(Mem image, bool blockingWrite, IntPtr[] origin, IntPtr[] region, IntPtr input_row_pitch, IntPtr input_slice_pitch, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;

            result = OpenCL.EnqueueWriteImage(CommandQueueID,
                image.MemID,
                (uint)(blockingWrite ? Bool.TRUE : Bool.FALSE),
                origin,
                region,
                input_row_pitch,
                input_slice_pitch,
                ptr.ToPointer(),
                (uint)num_events_in_wait_list,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueWriteImage failed with error code " + result, result);
        }
        public void EnqueueWriteImage(Mem image, bool blockingWrite, int[] origin, int[] region, int input_row_pitch, int input_slice_pitch, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;
            IntPtr* repackedOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.A3ToIntPtr3(origin, repackedOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);

            result = OpenCL.EnqueueWriteImage(CommandQueueID,
                image.MemID,
                (uint)(blockingWrite ? Bool.TRUE : Bool.FALSE),
                repackedOrigin,
                repackedRegion,
                input_row_pitch,
                input_slice_pitch,
                ptr,
                num_events_in_wait_list,
                repackedEvents,
                &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueWriteImage failed with error code " + result, result);
        }
        public void EnqueueWriteImage(Mem image, bool blockingWrite, long[] origin, long[] region, long input_row_pitch, long input_slice_pitch, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;
            IntPtr* repackedOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.A3ToIntPtr3(origin, repackedOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);

            result = OpenCL.EnqueueWriteImage(CommandQueueID,
                image.MemID,
                (uint)(blockingWrite ? Bool.TRUE : Bool.FALSE),
                repackedOrigin,
                repackedRegion,
                input_row_pitch,
                input_slice_pitch,
                ptr,
                num_events_in_wait_list,
                repackedEvents,
                &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueWriteImage failed with error code " + result, result);
        }

        public void EnqueueWriteImage(Mem image, bool blockingWrite, IntPtr[] origin, IntPtr[] region, IntPtr input_row_pitch, IntPtr input_slice_pitch, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;

            result = OpenCL.EnqueueWriteImage(CommandQueueID,
                image.MemID,
                (uint)(blockingWrite ? Bool.TRUE : Bool.FALSE),
                origin,
                region,
                input_row_pitch,
                input_slice_pitch,
                ptr.ToPointer(),
                (uint)num_events_in_wait_list,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueWriteImage failed with error code " + result, result);
        }
        public void EnqueueWriteImage(Mem image, bool blockingWrite, int[] origin, int[] region, int input_row_pitch, int input_slice_pitch, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;
            IntPtr* repackedOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.A3ToIntPtr3(origin, repackedOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);

            result = OpenCL.EnqueueWriteImage(CommandQueueID,
                image.MemID,
                (uint)(blockingWrite ? Bool.TRUE : Bool.FALSE),
                repackedOrigin,
                repackedRegion,
                input_row_pitch,
                input_slice_pitch,
                ptr,
                num_events_in_wait_list,
                repackedEvents,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueWriteImage failed with error code " + result, result);
        }
        public void EnqueueWriteImage(Mem image, bool blockingWrite, long[] origin, long[] region, long input_row_pitch, long input_slice_pitch, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;
            IntPtr* repackedOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.A3ToIntPtr3(origin, repackedOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);

            result = OpenCL.EnqueueWriteImage(CommandQueueID,
                image.MemID,
                (uint)(blockingWrite ? Bool.TRUE : Bool.FALSE),
                repackedOrigin,
                repackedRegion,
                input_row_pitch,
                input_slice_pitch,
                ptr,
                num_events_in_wait_list,
                repackedEvents,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueWriteImage failed with error code " + result, result);
        }

        public void EnqueueWriteImage(Mem image, bool blockingWrite, IntPtr[] origin, IntPtr[] region, IntPtr input_row_pitch, IntPtr input_slice_pitch, IntPtr ptr)
        {
            ErrorCode result;

            result = OpenCL.EnqueueWriteImage(CommandQueueID,
                image.MemID,
                (uint)(blockingWrite ? Bool.TRUE : Bool.FALSE),
                origin,
                region,
                input_row_pitch,
                input_slice_pitch,
                ptr.ToPointer(),
                (uint)0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueWriteImage failed with error code " + result, result);
        }
        public void EnqueueWriteImage(Mem image, bool blockingWrite, int[] origin, int[] region, int input_row_pitch, int input_slice_pitch, IntPtr ptr)
        {
            ErrorCode result;
            IntPtr* repackedOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];

            InteropTools.A3ToIntPtr3(origin, repackedOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);

            result = OpenCL.EnqueueWriteImage(CommandQueueID,
                image.MemID,
                (uint)(blockingWrite ? Bool.TRUE : Bool.FALSE),
                repackedOrigin,
                repackedRegion,
                input_row_pitch,
                input_slice_pitch,
                ptr,
                0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueWriteImage failed with error code " + result, result);
        }
        public void EnqueueWriteImage(Mem image, bool blockingWrite, long[] origin, long[] region, long input_row_pitch, long input_slice_pitch, IntPtr ptr)
        {
            ErrorCode result;
            IntPtr* repackedOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];

            InteropTools.A3ToIntPtr3(origin, repackedOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);

            result = OpenCL.EnqueueWriteImage(CommandQueueID,
                image.MemID,
                (uint)(blockingWrite ? Bool.TRUE : Bool.FALSE),
                repackedOrigin,
                repackedRegion,
                input_row_pitch,
                input_slice_pitch,
                ptr,
                0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueWriteImage failed with error code " + result, result);
        }

        #endregion

        #region EnqueueCopyImage

        public void EnqueueCopyImage(Mem src_image, Mem dst_image, IntPtr[] src_origin, IntPtr[] dst_origin, IntPtr[] region, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;

            result = OpenCL.EnqueueCopyImage(CommandQueueID,
                src_image.MemID,
                dst_image.MemID,
                src_origin,
                dst_origin,
                region,
                (uint)num_events_in_wait_list,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyImage failed with error code " + result, result);
        }
        public void EnqueueCopyImage(Mem src_image, Mem dst_image, int[] src_origin, int[] dst_origin, int[] region, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;
            IntPtr* repackedSrcOrigin = stackalloc IntPtr[3];
            IntPtr* repackedDstOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.A3ToIntPtr3(src_origin, repackedSrcOrigin);
            InteropTools.A3ToIntPtr3(dst_origin, repackedDstOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);

            result = OpenCL.EnqueueCopyImage(CommandQueueID, src_image, dst_image, repackedSrcOrigin, repackedDstOrigin, repackedRegion, num_events_in_wait_list, repackedEvents, &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyImage failed with error code " + result, result);
        }
        public void EnqueueCopyImage(Mem src_image, Mem dst_image, long[] src_origin, long[] dst_origin, long[] region, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;
            IntPtr* repackedSrcOrigin = stackalloc IntPtr[3];
            IntPtr* repackedDstOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.A3ToIntPtr3(src_origin, repackedSrcOrigin);
            InteropTools.A3ToIntPtr3(dst_origin, repackedDstOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);

            result = OpenCL.EnqueueCopyImage(CommandQueueID, src_image, dst_image, repackedSrcOrigin, repackedDstOrigin, repackedRegion, num_events_in_wait_list, repackedEvents, &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyImage failed with error code " + result, result);
        }

        public void EnqueueCopyImage(Mem src_image, Mem dst_image, IntPtr[] src_origin, IntPtr[] dst_origin, IntPtr[] region, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;

            result = OpenCL.EnqueueCopyImage(CommandQueueID,
                src_image.MemID,
                dst_image.MemID,
                src_origin,
                dst_origin,
                region,
                (uint)num_events_in_wait_list,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyImage failed with error code " + result, result);
        }
        public void EnqueueCopyImage(Mem src_image, Mem dst_image, int[] src_origin, int[] dst_origin, int[] region, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;
            IntPtr* repackedSrcOrigin = stackalloc IntPtr[3];
            IntPtr* repackedDstOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.A3ToIntPtr3(src_origin, repackedSrcOrigin);
            InteropTools.A3ToIntPtr3(dst_origin, repackedDstOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);

            result = OpenCL.EnqueueCopyImage(CommandQueueID, src_image, dst_image, repackedSrcOrigin, repackedDstOrigin, repackedRegion, num_events_in_wait_list, repackedEvents, null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyImage failed with error code " + result, result);
        }
        public void EnqueueCopyImage(Mem src_image, Mem dst_image, long[] src_origin, long[] dst_origin, long[] region, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;
            IntPtr* repackedSrcOrigin = stackalloc IntPtr[3];
            IntPtr* repackedDstOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.A3ToIntPtr3(src_origin, repackedSrcOrigin);
            InteropTools.A3ToIntPtr3(dst_origin, repackedDstOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);

            result = OpenCL.EnqueueCopyImage(CommandQueueID, src_image, dst_image, repackedSrcOrigin, repackedDstOrigin, repackedRegion, num_events_in_wait_list, repackedEvents, null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyImage failed with error code " + result, result);
        }

        public void EnqueueCopyImage(Mem src_image, Mem dst_image, IntPtr[] src_origin, IntPtr[] dst_origin, IntPtr[] region)
        {
            ErrorCode result;

            result = OpenCL.EnqueueCopyImage(CommandQueueID,
                src_image.MemID,
                dst_image.MemID,
                src_origin,
                dst_origin,
                region,
                (uint)0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyImage failed with error code " + result, result);
        }
        public void EnqueueCopyImage(Mem src_image, Mem dst_image, int[] src_origin, int[] dst_origin, int[] region)
        {
            ErrorCode result;
            IntPtr* repackedSrcOrigin = stackalloc IntPtr[3];
            IntPtr* repackedDstOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];

            InteropTools.A3ToIntPtr3(src_origin, repackedSrcOrigin);
            InteropTools.A3ToIntPtr3(dst_origin, repackedDstOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);

            result = OpenCL.EnqueueCopyImage(CommandQueueID, src_image, dst_image, repackedSrcOrigin, repackedDstOrigin, repackedRegion, 0, null, null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyImage failed with error code " + result, result);
        }
        public void EnqueueCopyImage(Mem src_image, Mem dst_image, long[] src_origin, long[] dst_origin, long[] region)
        {
            ErrorCode result;
            IntPtr* repackedSrcOrigin = stackalloc IntPtr[3];
            IntPtr* repackedDstOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];

            InteropTools.A3ToIntPtr3(src_origin, repackedSrcOrigin);
            InteropTools.A3ToIntPtr3(dst_origin, repackedDstOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);

            result = OpenCL.EnqueueCopyImage(CommandQueueID, src_image, dst_image, repackedSrcOrigin, repackedDstOrigin, repackedRegion, 0, null, null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyImage failed with error code " + result, result);
        }

        #endregion

        #region EnqueueCopyImageToBuffer

        public void EnqueueCopyImageToBuffer(Mem src_image, Mem dst_buffer, IntPtr[] src_origin, IntPtr[] region, IntPtr dst_offset, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;

            result = OpenCL.EnqueueCopyImageToBuffer(CommandQueueID,
                src_image.MemID,
                dst_buffer.MemID,
                src_origin,
                region,
                dst_offset,
                (uint)num_events_in_wait_list,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyImageToBuffer failed with error code " + result, result);
        }
        public void EnqueueCopyImageToBuffer(Mem src_image, Mem dst_buffer, int[] src_origin, int[] region, int dst_offset, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;
            IntPtr* repackedSrcOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.A3ToIntPtr3(src_origin, repackedSrcOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);

            result = OpenCL.EnqueueCopyImageToBuffer(CommandQueueID, src_image, dst_buffer, repackedSrcOrigin, repackedRegion, dst_offset, num_events_in_wait_list, repackedEvents, &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyImageToBuffer failed with error code " + result, result);
        }
        public void EnqueueCopyImageToBuffer(Mem src_image, Mem dst_buffer, long[] src_origin, long[] region, long dst_offset, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;
            IntPtr* repackedSrcOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.A3ToIntPtr3(src_origin, repackedSrcOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);

            result = OpenCL.EnqueueCopyImageToBuffer(CommandQueueID, src_image, dst_buffer, repackedSrcOrigin, repackedRegion, dst_offset, num_events_in_wait_list, repackedEvents, &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyImageToBuffer failed with error code " + result, result);
        }

        public void EnqueueCopyImageToBuffer(Mem src_image, Mem dst_buffer, IntPtr[] src_origin, IntPtr[] region, IntPtr dst_offset, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;

            result = OpenCL.EnqueueCopyImageToBuffer(CommandQueueID,
                src_image.MemID,
                dst_buffer.MemID,
                src_origin,
                region,
                dst_offset,
                (uint)num_events_in_wait_list,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyImageToBuffer failed with error code " + result, result);
        }
        public void EnqueueCopyImageToBuffer(Mem src_image, Mem dst_buffer, int[] src_origin, int[] region, int dst_offset, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;
            IntPtr* repackedSrcOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.A3ToIntPtr3(src_origin, repackedSrcOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);

            result = OpenCL.EnqueueCopyImageToBuffer(CommandQueueID, src_image, dst_buffer, repackedSrcOrigin, repackedRegion, dst_offset, num_events_in_wait_list, repackedEvents, null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyImageToBuffer failed with error code " + result, result);
        }
        public void EnqueueCopyImageToBuffer(Mem src_image, Mem dst_buffer, long[] src_origin, long[] region, long dst_offset, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;
            IntPtr* repackedSrcOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.A3ToIntPtr3(src_origin, repackedSrcOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);

            result = OpenCL.EnqueueCopyImageToBuffer(CommandQueueID, src_image, dst_buffer, repackedSrcOrigin, repackedRegion, dst_offset, num_events_in_wait_list, repackedEvents, null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyImageToBuffer failed with error code " + result, result);
        }

        public void EnqueueCopyImageToBuffer(Mem src_image, Mem dst_buffer, IntPtr[] src_origin, IntPtr[] region, IntPtr dst_offset)
        {
            ErrorCode result;

            result = OpenCL.EnqueueCopyImageToBuffer(CommandQueueID,
                src_image.MemID,
                dst_buffer.MemID,
                src_origin,
                region,
                dst_offset,
                (uint)0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyImageToBuffer failed with error code " + result, result);
        }
        public void EnqueueCopyImageToBuffer(Mem src_image, Mem dst_buffer, int[] src_origin, int[] region, int dst_offset)
        {
            ErrorCode result;
            IntPtr* repackedSrcOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];

            InteropTools.A3ToIntPtr3(src_origin, repackedSrcOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);

            result = OpenCL.EnqueueCopyImageToBuffer(CommandQueueID, src_image, dst_buffer, repackedSrcOrigin, repackedRegion, dst_offset, 0, null, null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyImageToBuffer failed with error code " + result, result);
        }
        public void EnqueueCopyImageToBuffer(Mem src_image, Mem dst_buffer, long[] src_origin, long[] region, long dst_offset)
        {
            ErrorCode result;
            IntPtr* repackedSrcOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];

            InteropTools.A3ToIntPtr3(src_origin, repackedSrcOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);

            result = OpenCL.EnqueueCopyImageToBuffer(CommandQueueID, src_image, dst_buffer, repackedSrcOrigin, repackedRegion, dst_offset, 0, null, null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyImageToBuffer failed with error code " + result, result);
        }

        #endregion

        #region EnqueueCopyBufferToImage

        public void EnqueueCopyBufferToImage(Mem src_buffer, Mem dst_image, IntPtr src_offset, IntPtr[] dst_origin, IntPtr[] region, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;

            result = OpenCL.EnqueueCopyBufferToImage(CommandQueueID,
                src_buffer.MemID,
                dst_image.MemID,
                src_offset,
                dst_origin,
                region,
                (uint)num_events_in_wait_list,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyBufferToImage failed with error code " + result, result);
        }
        public void EnqueueCopyBufferToImage(Mem src_buffer, Mem dst_image, int src_offset, int[] dst_origin, int[] region, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;
            IntPtr* repackedDstOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.A3ToIntPtr3(dst_origin, repackedDstOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);

            result = OpenCL.EnqueueCopyBufferToImage(CommandQueueID, src_buffer, dst_image, src_offset, repackedDstOrigin, repackedRegion, num_events_in_wait_list, repackedEvents, &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyBufferToImage failed with error code " + result, result);
        }
        public void EnqueueCopyBufferToImage(Mem src_buffer, Mem dst_image, long src_offset, long[] dst_origin, long[] region, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;
            IntPtr* repackedDstOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.A3ToIntPtr3(dst_origin, repackedDstOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);

            result = OpenCL.EnqueueCopyBufferToImage(CommandQueueID, src_buffer, dst_image, src_offset, repackedDstOrigin, repackedRegion, num_events_in_wait_list, repackedEvents, &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyBufferToImage failed with error code " + result, result);
        }

        public void EnqueueCopyBufferToImage(Mem src_buffer, Mem dst_image, IntPtr src_offset, IntPtr[] dst_origin, IntPtr[] region, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;

            result = OpenCL.EnqueueCopyBufferToImage(CommandQueueID,
                src_buffer.MemID,
                dst_image.MemID,
                src_offset,
                dst_origin,
                region,
                (uint)num_events_in_wait_list,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyBufferToImage failed with error code " + result, result);
        }
        public void EnqueueCopyBufferToImage(Mem src_buffer, Mem dst_image, int src_offset, int[] dst_origin, int[] region, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;
            IntPtr* repackedDstOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.A3ToIntPtr3(dst_origin, repackedDstOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);

            result = OpenCL.EnqueueCopyBufferToImage(CommandQueueID, src_buffer, dst_image, src_offset, repackedDstOrigin, repackedRegion, num_events_in_wait_list, repackedEvents, null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyBufferToImage failed with error code " + result, result);
        }
        public void EnqueueCopyBufferToImage(Mem src_buffer, Mem dst_image, long src_offset, long[] dst_origin, long[] region, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;
            IntPtr* repackedDstOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.A3ToIntPtr3(dst_origin, repackedDstOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);

            result = OpenCL.EnqueueCopyBufferToImage(CommandQueueID, src_buffer, dst_image, src_offset, repackedDstOrigin, repackedRegion, num_events_in_wait_list, repackedEvents, null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyBufferToImage failed with error code " + result, result);
        }

        public void EnqueueCopyBufferToImage(Mem src_buffer, Mem dst_image, IntPtr src_offset, IntPtr[] dst_origin, IntPtr[] region)
        {
            ErrorCode result;

            result = OpenCL.EnqueueCopyBufferToImage(CommandQueueID,
                src_buffer.MemID,
                dst_image.MemID,
                src_offset,
                dst_origin,
                region,
                (uint)0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyBufferToImage failed with error code " + result, result);
        }
        public void EnqueueCopyBufferToImage(Mem src_buffer, Mem dst_image, int src_offset, int[] dst_origin, int[] region)
        {
            ErrorCode result;
            IntPtr* repackedDstOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];

            InteropTools.A3ToIntPtr3(dst_origin, repackedDstOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);

            result = OpenCL.EnqueueCopyBufferToImage(CommandQueueID, src_buffer, dst_image, src_offset, repackedDstOrigin, repackedRegion, 0, null, null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyBufferToImage failed with error code " + result, result);
        }
        public void EnqueueCopyBufferToImage(Mem src_buffer, Mem dst_image, long src_offset, long[] dst_origin, long[] region)
        {
            ErrorCode result;
            IntPtr* repackedDstOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];

            InteropTools.A3ToIntPtr3(dst_origin, repackedDstOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);

            result = OpenCL.EnqueueCopyBufferToImage(CommandQueueID, src_buffer, dst_image, src_offset, repackedDstOrigin, repackedRegion, 0, null, null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyBufferToImage failed with error code " + result, result);
        }

        #endregion

        #region EnqueueMapBuffer

        public IntPtr EnqueueMapBuffer(Mem buffer, bool blockingMap, MapFlags map_flags, IntPtr offset, IntPtr cb, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr ptr;
            IntPtr tmpEvent;

            ptr = (IntPtr)OpenCL.EnqueueMapBuffer(CommandQueueID,
                buffer.MemID,
                (uint)(blockingMap ? Bool.TRUE : Bool.FALSE),
                (ulong)map_flags,
                offset,
                cb,
                (uint)num_events_in_wait_list,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                &tmpEvent,
                out result);
            _event = new Event(Context, this, tmpEvent); 

            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueMapBuffer failed with error code " + result, result);
            return ptr;
        }
        public IntPtr EnqueueMapBuffer(Mem buffer, bool blockingMap, MapFlags map_flags, int offset, int cb, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            void* pMappedPtr;
            ErrorCode result;
            IntPtr tmpEvent;
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);

            pMappedPtr = OpenCL.EnqueueMapBuffer(CommandQueueID, buffer, blockingMap ? (uint)Bool.TRUE : (uint)Bool.FALSE, (ulong)map_flags, offset, cb, num_events_in_wait_list, repackedEvents, &tmpEvent, out result);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueMapBuffer failed with error code " + result, result);

            return (IntPtr)pMappedPtr;
        }
        public IntPtr EnqueueMapBuffer(Mem buffer, bool blockingMap, MapFlags map_flags, long offset, long cb, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            void* pMappedPtr;
            ErrorCode result;
            IntPtr tmpEvent;
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);

            pMappedPtr = OpenCL.EnqueueMapBuffer(CommandQueueID, buffer, blockingMap ? (uint)Bool.TRUE : (uint)Bool.FALSE, (ulong)map_flags, offset, cb, num_events_in_wait_list, repackedEvents, &tmpEvent, out result);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueMapBuffer failed with error code " + result, result);
            return (IntPtr)pMappedPtr;
        }

        public IntPtr EnqueueMapBuffer(Mem buffer, bool blockingMap, MapFlags map_flags, IntPtr offset, IntPtr cb, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;
            IntPtr ptr;

            ptr = (IntPtr)OpenCL.EnqueueMapBuffer(CommandQueueID,
                buffer.MemID,
                (uint)(blockingMap ? Bool.TRUE : Bool.FALSE),
                (ulong)map_flags,
                offset,
                cb,
                (uint)num_events_in_wait_list,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                null,
                out result);

            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueMapBuffer failed with error code " + result, result);
            return ptr;
        }
        public IntPtr EnqueueMapBuffer(Mem buffer, bool blockingMap, MapFlags map_flags, int offset, int cb, int num_events_in_wait_list, Event[] event_wait_list)
        {
            void* pMappedPtr;
            ErrorCode result;
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);

            pMappedPtr = OpenCL.EnqueueMapBuffer(CommandQueueID, buffer, blockingMap ? (uint)Bool.TRUE : (uint)Bool.FALSE, (ulong)map_flags, offset, cb, num_events_in_wait_list, repackedEvents, null, out result);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueMapBuffer failed with error code " + result, result);
            return (IntPtr)pMappedPtr;
        }
        public IntPtr EnqueueMapBuffer(Mem buffer, bool blockingMap, MapFlags map_flags, long offset, long cb, int num_events_in_wait_list, Event[] event_wait_list)
        {
            void* pMappedPtr;
            ErrorCode result;
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);

            pMappedPtr = OpenCL.EnqueueMapBuffer(CommandQueueID, buffer, blockingMap ? (uint)Bool.TRUE : (uint)Bool.FALSE, (ulong)map_flags, offset, cb, num_events_in_wait_list, repackedEvents, null, out result);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueMapBuffer failed with error code " + result, result);
            return (IntPtr)pMappedPtr;
        }

        public IntPtr EnqueueMapBuffer(Mem buffer, bool blockingMap, MapFlags map_flags, IntPtr offset, IntPtr cb)
        {
            ErrorCode result;
            IntPtr ptr;

            ptr = (IntPtr)OpenCL.EnqueueMapBuffer(CommandQueueID,
                buffer.MemID,
                (uint)(blockingMap ? Bool.TRUE : Bool.FALSE),
                (ulong)map_flags,
                offset,
                cb,
                (uint)0,
                null,
                null,
                out result);

            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueMapBuffer failed with error code " + result, result);
            return ptr;
        }
        public IntPtr EnqueueMapBuffer(Mem buffer, bool blockingMap, MapFlags map_flags, int offset, int cb)
        {
            void* pMappedPtr;
            ErrorCode result;

            pMappedPtr = OpenCL.EnqueueMapBuffer(CommandQueueID, buffer, blockingMap ? (uint)Bool.TRUE : (uint)Bool.FALSE, (ulong)map_flags, offset, cb, 0, null, null, out result);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueMapBuffer failed with error code " + result, result);
            return (IntPtr)pMappedPtr;
        }
        public IntPtr EnqueueMapBuffer(Mem buffer, bool blockingMap, MapFlags map_flags, long offset, long cb)
        {
            void* pMappedPtr;
            ErrorCode result;

            pMappedPtr = OpenCL.EnqueueMapBuffer(CommandQueueID, buffer, blockingMap ? (uint)Bool.TRUE : (uint)Bool.FALSE, (ulong)map_flags, offset, cb, 0, null, null, out result);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueMapBuffer failed with error code " + result, result);
            return (IntPtr)pMappedPtr;
        }

        #endregion

        #region EnqueueMapImage

        /// <summary>
        /// Map the memory of a Mem object into host memory.
        /// This function must be used before native code accesses an area of memory that's under the control
        /// of OpenCL. This includes Mem objects allocated with MemFlags.USE_HOST_PTR, as results may be cached
        /// in another location. Mapping will ensure caches are synchronizatized.
        /// </summary>
        /// <param name="image">Mem object to map</param>
        /// <param name="blockingMap">Flag that indicates if the operation is synchronous or not</param>
        /// <param name="map_flags">Read/Write flags</param>
        /// <param name="origin">origin contains the x,y,z coordinates indicating the starting point to map</param>
        /// <param name="region">origin contains the width,height,depth coordinates indicating the size of the area to map</param>
        /// <param name="image_row_pitch"></param>
        /// <param name="image_slice_pitch"></param>
        /// <param name="num_events_in_wait_list"></param>
        /// <param name="event_wait_list"></param>
        /// <param name="_event"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public IntPtr EnqueueMapImage(Mem image, bool blockingMap, MapFlags map_flags, IntPtr[] origin, IntPtr[] region, out IntPtr image_row_pitch, out IntPtr image_slice_pitch, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            IntPtr ptr;
            IntPtr tmpEvent;
            ErrorCode result;
            
            ptr = (IntPtr)OpenCL.EnqueueMapImage(CommandQueueID,
                image.MemID,
                (uint)(blockingMap ? Bool.TRUE : Bool.FALSE),
                (ulong)map_flags,
                origin,
                region,
                out image_row_pitch,
                out image_slice_pitch,
                (uint)num_events_in_wait_list,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                &tmpEvent,
                out result);
            _event = new Event(Context, this, tmpEvent); 
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueMapImage failed with error code " + result, result);
            return ptr;
        }
        public IntPtr EnqueueMapImage(Mem image, bool blockingMap, MapFlags map_flags, int[] origin, int[] region, out int image_row_pitch, out int image_slice_pitch, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            void* pMappedPtr;
            ErrorCode result;
            IntPtr tmpEvent;
            IntPtr* repackedOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.A3ToIntPtr3(origin, repackedOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);

            pMappedPtr = OpenCL.EnqueueMapImage(CommandQueueID, image, (uint)(blockingMap ? Bool.TRUE : Bool.TRUE), (ulong)map_flags, repackedOrigin, repackedRegion, out image_row_pitch, out image_slice_pitch, num_events_in_wait_list, repackedEvents, &tmpEvent, out result);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueMapImage failed with error code " + result, result);
            return (IntPtr)pMappedPtr;
        }
        public IntPtr EnqueueMapImage(Mem image, bool blockingMap, MapFlags map_flags, long[] origin, long[] region, out long image_row_pitch, out long image_slice_pitch, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            void* pMappedPtr;
            ErrorCode result;
            IntPtr tmpEvent;
            IntPtr* repackedOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.A3ToIntPtr3(origin, repackedOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);

            pMappedPtr = OpenCL.EnqueueMapImage(CommandQueueID, image, (uint)(blockingMap ? Bool.TRUE : Bool.TRUE), (ulong)map_flags, repackedOrigin, repackedRegion, out image_row_pitch, out image_slice_pitch, num_events_in_wait_list, repackedEvents, &tmpEvent, out result);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueMapImage failed with error code " + result, result);
            return (IntPtr)pMappedPtr;
        }

        public IntPtr EnqueueMapImage(Mem image, bool blockingMap, MapFlags map_flags, IntPtr[] origin, IntPtr[] region, out IntPtr image_row_pitch, out IntPtr image_slice_pitch, int num_events_in_wait_list, Event[] event_wait_list)
        {
            IntPtr ptr;
            ErrorCode result;

            ptr = (IntPtr)OpenCL.EnqueueMapImage(CommandQueueID,
                image.MemID,
                (uint)(blockingMap ? Bool.TRUE : Bool.FALSE),
                (ulong)map_flags,
                origin,
                region,
                out image_row_pitch,
                out image_slice_pitch,
                (uint)num_events_in_wait_list,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                null,
                out result);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueMapImage failed with error code " + result, result);
            return ptr;
        }
        public IntPtr EnqueueMapImage(Mem image, bool blockingMap, MapFlags map_flags, int[] origin, int[] region, out int image_row_pitch, out int image_slice_pitch, int num_events_in_wait_list, Event[] event_wait_list)
        {
            void* pMappedPtr;
            ErrorCode result;

            IntPtr* repackedOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.A3ToIntPtr3(origin, repackedOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);

            pMappedPtr = OpenCL.EnqueueMapImage(CommandQueueID, image, (uint)(blockingMap ? Bool.TRUE : Bool.TRUE), (ulong)map_flags, repackedOrigin, repackedRegion, out image_row_pitch, out image_slice_pitch, num_events_in_wait_list, repackedEvents, null, out result);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueMapImage failed with error code " + result, result);
            return (IntPtr)pMappedPtr;
        }
        public IntPtr EnqueueMapImage(Mem image, bool blockingMap, MapFlags map_flags, long[] origin, long[] region, out long image_row_pitch, out long image_slice_pitch, int num_events_in_wait_list, Event[] event_wait_list)
        {
            void* pMappedPtr;
            ErrorCode result;
            IntPtr* repackedOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];
            IntPtr* repackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                repackedEvents = null;
            InteropTools.A3ToIntPtr3(origin, repackedOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, repackedEvents);

            pMappedPtr = OpenCL.EnqueueMapImage(CommandQueueID, image, (uint)(blockingMap ? Bool.TRUE : Bool.TRUE), (ulong)map_flags, repackedOrigin, repackedRegion, out image_row_pitch, out image_slice_pitch, num_events_in_wait_list, repackedEvents, null, out result);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueMapImage failed with error code " + result, result);
            return (IntPtr)pMappedPtr;
        }

        public IntPtr EnqueueMapImage(Mem image, bool blockingMap, MapFlags map_flags, IntPtr[] origin, IntPtr[] region, out IntPtr image_row_pitch, out IntPtr image_slice_pitch)
        {
            IntPtr ptr;
            ErrorCode result;

            ptr = (IntPtr)OpenCL.EnqueueMapImage(CommandQueueID,
                image.MemID,
                (uint)(blockingMap ? Bool.TRUE : Bool.FALSE),
                (ulong)map_flags,
                origin,
                region,
                out image_row_pitch,
                out image_slice_pitch,
                (uint)0,
                null,
                null,
                out result);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueMapImage failed with error code " + result, result);
            return ptr;
        }
        public IntPtr EnqueueMapImage(Mem image, bool blockingMap, MapFlags map_flags, int[] origin, int[] region, out int image_row_pitch, out int image_slice_pitch)
        {
            void* pMappedPtr;
            ErrorCode result;
            IntPtr* repackedOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];

            InteropTools.A3ToIntPtr3(origin, repackedOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);

            pMappedPtr = OpenCL.EnqueueMapImage(CommandQueueID, image, (uint)(blockingMap ? Bool.TRUE : Bool.TRUE), (ulong)map_flags, repackedOrigin, repackedRegion, out image_row_pitch, out image_slice_pitch, 0, null, null, out result);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueMapImage failed with error code " + result, result);
            return (IntPtr)pMappedPtr;
        }
        public IntPtr EnqueueMapImage(Mem image, bool blockingMap, MapFlags map_flags, long[] origin, long[] region, out long image_row_pitch, out long image_slice_pitch)
        {
            void* pMappedPtr;
            ErrorCode result;
            IntPtr* repackedOrigin = stackalloc IntPtr[3];
            IntPtr* repackedRegion = stackalloc IntPtr[3];

            InteropTools.A3ToIntPtr3(origin, repackedOrigin);
            InteropTools.A3ToIntPtr3(region, repackedRegion);

            pMappedPtr = OpenCL.EnqueueMapImage(CommandQueueID, image, (uint)(blockingMap ? Bool.TRUE : Bool.TRUE), (ulong)map_flags, repackedOrigin, repackedRegion, out image_row_pitch, out image_slice_pitch, 0, null, null, out result);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueMapImage failed with error code " + result, result);
            return (IntPtr)pMappedPtr;
        }

        #endregion

        #region EnqueueUnmapMemObject

        /// <summary>
        /// Unmap a previously mapped Mem object
        /// </summary>
        /// <param name="memobj"></param>
        /// <param name="mapped_ptr"></param>
        /// <param name="num_events_in_wait_list"></param>
        /// <param name="event_wait_list"></param>
        /// <param name="_event"></param>
        public void EnqueueUnmapMemObject(Mem memobj, IntPtr mapped_ptr, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;

            result = OpenCL.EnqueueUnmapMemObject(CommandQueueID,
                memobj.MemID,
                mapped_ptr.ToPointer(),
                (uint)num_events_in_wait_list,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueUnmapMemObject failed with error code " + result, result);
        }

        public void EnqueueUnmapMemObject(Mem memobj, IntPtr mapped_ptr, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;

            result = OpenCL.EnqueueUnmapMemObject(CommandQueueID,
                memobj.MemID,
                mapped_ptr.ToPointer(),
                (uint)num_events_in_wait_list,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueUnmapMemObject failed with error code " + result, result);
        }

        public void EnqueueUnmapMemObject(Mem memobj, IntPtr mapped_ptr)
        {
            ErrorCode result;

            result = OpenCL.EnqueueUnmapMemObject(CommandQueueID,
                memobj.MemID,
                mapped_ptr.ToPointer(),
                (uint)0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueUnmapMemObject failed with error code " + result, result);
        }

        #endregion

        #region EnqueueNDRangeKernel

        /// <summary>
        /// Execute a parallel kernel.
        /// </summary>
        /// <param name="kernel"></param>
        /// <param name="workDim">The number of dimensions in the workspace(0-2), must correspond to the number of dimensions in the following arrays</param>
        /// <param name="globalWorkOffset">null in OpenCL 1.0, but will allow indices to start at non-0 locations</param>
        /// <param name="globalWorkSize">Index n of this array=the length of the n'th dimension of global work space</param>
        /// <param name="localWorkSize">Index n of this array=the length of the n'th dimension of local work space</param>
        /// <param name="numEventsInWaitList"></param>
        /// <param name="event_wait_list"></param>
        /// <param name="_event"></param>
        public void EnqueueNDRangeKernel(Kernel kernel, uint workDim, IntPtr[] globalWorkOffset, IntPtr[] globalWorkSize, IntPtr[] localWorkSize, uint numEventsInWaitList, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;

            result = OpenCL.EnqueueNDRangeKernel(CommandQueueID,
                kernel.KernelID,
                workDim,
                globalWorkOffset,
                globalWorkSize,
                localWorkSize,
                numEventsInWaitList,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueNDRangeKernel failed with error code " + result, result);
        }
        public void EnqueueNDRangeKernel(Kernel kernel, int workDim, int[] globalWorkOffset, int[] globalWorkSize, int[] localWorkSize, int numEventsInWaitList, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;

            IntPtr* pGlobalWorkOffset = stackalloc IntPtr[workDim];
            IntPtr* pGlobalWorkSize = stackalloc IntPtr[workDim];
            IntPtr* pLocalWorkSize = stackalloc IntPtr[workDim];
            IntPtr* repackedEvents = stackalloc IntPtr[numEventsInWaitList];
            if (globalWorkOffset == null)
                pGlobalWorkOffset = null;
            if (globalWorkSize == null)
                pGlobalWorkSize = null;
            if (localWorkSize == null)
                pLocalWorkSize = null;
            if (event_wait_list == null)
                repackedEvents = null;

            InteropTools.AToIntPtr(workDim, globalWorkOffset, pGlobalWorkOffset);
            InteropTools.AToIntPtr(workDim, globalWorkSize, pGlobalWorkSize);
            InteropTools.AToIntPtr(workDim, localWorkSize, pLocalWorkSize);
            InteropTools.ConvertEventsToEventIDs(numEventsInWaitList, event_wait_list, repackedEvents);

            result = OpenCL.EnqueueNDRangeKernel(CommandQueueID,
                kernel.KernelID,
                workDim,
                pGlobalWorkOffset,
                pGlobalWorkSize,
                pLocalWorkSize,
                numEventsInWaitList,
                repackedEvents,
                &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueNDRangeKernel failed with error code " + result, result);
        }
        public void EnqueueNDRangeKernel(Kernel kernel, int workDim, long[] globalWorkOffset, long[] globalWorkSize, long[] localWorkSize, int numEventsInWaitList, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;

            IntPtr* pGlobalWorkOffset = stackalloc IntPtr[workDim];
            IntPtr* pGlobalWorkSize = stackalloc IntPtr[workDim];
            IntPtr* pLocalWorkSize = stackalloc IntPtr[workDim];
            IntPtr* repackedEvents = stackalloc IntPtr[numEventsInWaitList];
            if (globalWorkOffset == null)
                pGlobalWorkOffset = null;
            if (globalWorkSize == null)
                pGlobalWorkSize = null;
            if (localWorkSize == null)
                pLocalWorkSize = null;
            if (event_wait_list == null)
                repackedEvents = null;

            InteropTools.AToIntPtr(workDim, globalWorkOffset, pGlobalWorkOffset);
            InteropTools.AToIntPtr(workDim, globalWorkSize, pGlobalWorkSize);
            InteropTools.AToIntPtr(workDim, localWorkSize, pLocalWorkSize);
            InteropTools.ConvertEventsToEventIDs(numEventsInWaitList, event_wait_list, repackedEvents);

            result = OpenCL.EnqueueNDRangeKernel(CommandQueueID,
                kernel.KernelID,
                workDim,
                pGlobalWorkOffset,
                pGlobalWorkSize,
                pLocalWorkSize,
                numEventsInWaitList,
                repackedEvents,
                &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueNDRangeKernel failed with error code " + result, result);
        }

        public void EnqueueNDRangeKernel(Kernel kernel, uint workDim, IntPtr[] globalWorkOffset, IntPtr[] globalWorkSize, IntPtr[] localWorkSize, uint numEventsInWaitList, Event[] event_wait_list)
        {
            ErrorCode result;

            result = OpenCL.EnqueueNDRangeKernel(CommandQueueID,
                kernel.KernelID,
                workDim,
                globalWorkOffset,
                globalWorkSize,
                localWorkSize,
                numEventsInWaitList,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueNDRangeKernel failed with error code " + result, result);
        }
        public void EnqueueNDRangeKernel(Kernel kernel, int workDim, int[] globalWorkOffset, int[] globalWorkSize, int[] localWorkSize, int numEventsInWaitList, Event[] event_wait_list)
        {
            ErrorCode result;

            IntPtr* pGlobalWorkOffset = stackalloc IntPtr[workDim];
            IntPtr* pGlobalWorkSize = stackalloc IntPtr[workDim];
            IntPtr* pLocalWorkSize = stackalloc IntPtr[workDim];
            IntPtr* repackedEvents = stackalloc IntPtr[numEventsInWaitList];
            if (globalWorkOffset == null)
                pGlobalWorkOffset = null;
            if (globalWorkSize == null)
                pGlobalWorkSize = null;
            if (localWorkSize == null)
                pLocalWorkSize = null;
            if (event_wait_list == null)
                repackedEvents = null;

            InteropTools.AToIntPtr(workDim, globalWorkOffset, pGlobalWorkOffset);
            InteropTools.AToIntPtr(workDim, globalWorkSize, pGlobalWorkSize);
            InteropTools.AToIntPtr(workDim, localWorkSize, pLocalWorkSize);
            InteropTools.ConvertEventsToEventIDs(numEventsInWaitList, event_wait_list, repackedEvents);

            result = OpenCL.EnqueueNDRangeKernel(CommandQueueID,
                kernel.KernelID,
                workDim,
                pGlobalWorkOffset,
                pGlobalWorkSize,
                pLocalWorkSize,
                numEventsInWaitList,
                repackedEvents,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueNDRangeKernel failed with error code " + result, result);
        }
        public void EnqueueNDRangeKernel(Kernel kernel, int workDim, long[] globalWorkOffset, long[] globalWorkSize, long[] localWorkSize, int numEventsInWaitList, Event[] event_wait_list)
        {
            ErrorCode result;

            IntPtr* pGlobalWorkOffset = stackalloc IntPtr[workDim];
            IntPtr* pGlobalWorkSize = stackalloc IntPtr[workDim];
            IntPtr* pLocalWorkSize = stackalloc IntPtr[workDim];
            IntPtr* repackedEvents = stackalloc IntPtr[numEventsInWaitList];
            if (globalWorkOffset == null)
                pGlobalWorkOffset = null;
            if (globalWorkSize == null)
                pGlobalWorkSize = null;
            if (localWorkSize == null)
                pLocalWorkSize = null;
            if (event_wait_list == null)
                repackedEvents = null;

            InteropTools.AToIntPtr(workDim, globalWorkOffset, pGlobalWorkOffset);
            InteropTools.AToIntPtr(workDim, globalWorkSize, pGlobalWorkSize);
            InteropTools.AToIntPtr(workDim, localWorkSize, pLocalWorkSize);
            InteropTools.ConvertEventsToEventIDs(numEventsInWaitList, event_wait_list, repackedEvents);

            result = OpenCL.EnqueueNDRangeKernel(CommandQueueID,
                kernel.KernelID,
                workDim,
                pGlobalWorkOffset,
                pGlobalWorkSize,
                pLocalWorkSize,
                numEventsInWaitList,
                repackedEvents,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueNDRangeKernel failed with error code " + result, result);
        }

        public void EnqueueNDRangeKernel(Kernel kernel, uint workDim, IntPtr[] globalWorkOffset, IntPtr[] globalWorkSize, IntPtr[] localWorkSize)
        {
            ErrorCode result;

            result = OpenCL.EnqueueNDRangeKernel(CommandQueueID,
                kernel.KernelID,
                workDim,
                globalWorkOffset,
                globalWorkSize,
                localWorkSize,
                0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueNDRangeKernel failed with error code " + result, result);
        }
        public void EnqueueNDRangeKernel(Kernel kernel, int workDim, int[] globalWorkOffset, int[] globalWorkSize, int[] localWorkSize)
        {
            ErrorCode result;

            IntPtr* pGlobalWorkOffset = stackalloc IntPtr[workDim];
            IntPtr* pGlobalWorkSize = stackalloc IntPtr[workDim];
            IntPtr* pLocalWorkSize = stackalloc IntPtr[workDim];
            if (globalWorkOffset == null)
                pGlobalWorkOffset = null;
            if (globalWorkSize == null)
                pGlobalWorkSize = null;
            if (localWorkSize == null)
                pLocalWorkSize = null;

            InteropTools.AToIntPtr(workDim, globalWorkOffset, pGlobalWorkOffset);
            InteropTools.AToIntPtr(workDim, globalWorkSize, pGlobalWorkSize);
            InteropTools.AToIntPtr(workDim, localWorkSize, pLocalWorkSize);

            result = OpenCL.EnqueueNDRangeKernel(CommandQueueID,
                kernel.KernelID,
                workDim,
                pGlobalWorkOffset,
                pGlobalWorkSize,
                pLocalWorkSize,
                0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueNDRangeKernel failed with error code " + result, result);
        }
        public void EnqueueNDRangeKernel(Kernel kernel, int workDim, long[] globalWorkOffset, long[] globalWorkSize, long[] localWorkSize)
        {
            ErrorCode result;

            IntPtr* pGlobalWorkOffset = stackalloc IntPtr[workDim];
            IntPtr* pGlobalWorkSize = stackalloc IntPtr[workDim];
            IntPtr* pLocalWorkSize = stackalloc IntPtr[workDim];
            if (globalWorkOffset == null)
                pGlobalWorkOffset = null;
            if (globalWorkSize == null)
                pGlobalWorkSize = null;
            if (localWorkSize == null)
                pLocalWorkSize = null;

            InteropTools.AToIntPtr(workDim, globalWorkOffset, pGlobalWorkOffset);
            InteropTools.AToIntPtr(workDim, globalWorkSize, pGlobalWorkSize);
            InteropTools.AToIntPtr(workDim, localWorkSize, pLocalWorkSize);

            result = OpenCL.EnqueueNDRangeKernel(CommandQueueID,
                kernel.KernelID,
                workDim,
                pGlobalWorkOffset,
                pGlobalWorkSize,
                pLocalWorkSize,
                0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueNDRangeKernel failed with error code " + result, result);
        }

        #endregion

        #region EnqueueTask

        /// <summary>
        /// Execute a simple kernel
        /// </summary>
        /// <param name="kernel"></param>
        /// <param name="numEventsInWaitList"></param>
        /// <param name="event_wait_list"></param>
        /// <param name="_event"></param>
        public void EnqueueTask(Kernel kernel, int numEventsInWaitList, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;

            result = (ErrorCode)OpenCL.EnqueueTask(CommandQueueID,
                kernel.KernelID,
                (uint)numEventsInWaitList,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueTask failed with error code " + result, result);
        }

        /// <summary>
        /// Execute a simple kernel
        /// </summary>
        /// <param name="kernel"></param>
        /// <param name="numEventsInWaitList"></param>
        /// <param name="event_wait_list"></param>
        public void EnqueueTask(Kernel kernel, int numEventsInWaitList, Event[] event_wait_list)
        {
            ErrorCode result;

            result = (ErrorCode)OpenCL.EnqueueTask(CommandQueueID,
                kernel.KernelID,
                (uint)numEventsInWaitList,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueTask failed with error code " + result, result);
        }

        /// <summary>
        /// Execute a simple kernel
        /// </summary>
        /// <param name="kernel"></param>
        public void EnqueueTask(Kernel kernel)
        {
            ErrorCode result;

            result = (ErrorCode)OpenCL.EnqueueTask(CommandQueueID,
                kernel.KernelID,
                (uint)0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueTask failed with error code " + result, result);
        }

        #endregion

        #region EnqueueNativeKernel

        internal class NativeKernelCallbackData
        {
            internal NativeKernel NativeKernel;
            internal CommandQueue CQ;
            internal object O;
            internal Mem [] Buffers;

            internal NativeKernelCallbackData(NativeKernel nk,CommandQueue cq, object o, Mem[] buffers)
            {
                NativeKernel = nk;
                CQ = cq;
                O = o;
                Buffers = buffers;
            }
        }

        private static Mutex NativeKernelParamsMutex = new Mutex();
        private static int NativeKernelParamsId = 0;
        private static NativeKernelInternal NativeKernelDelegate = new NativeKernelInternal(NativeKernelCallback);
        private static Dictionary<int, NativeKernelCallbackData> NativeKernelDispatch = new Dictionary<int, NativeKernelCallbackData>();
        
        unsafe private static void NativeKernelCallback( void* args )
        {
            int callbackId = *(int*)args;
            NativeKernelCallbackData callbackData = GetCallback(callbackId);
            void*[] buffers;

            if( callbackData.Buffers!=null )
            {
                buffers = new void*[callbackData.Buffers.Length];
                for (int i = 0; i < buffers.Length; i++)
                    buffers[i] = callbackData.CQ.EnqueueMapBuffer(callbackData.Buffers[i], true, MapFlags.READ_WRITE, IntPtr.Zero, callbackData.Buffers[i].MemSize).ToPointer();
            }
            else
                buffers = null;

            callbackData.NativeKernel(callbackData.O, buffers);

            if (buffers != null)
            {
                for (int i = 0; i < buffers.Length; i++)
                    callbackData.CQ.EnqueueUnmapMemObject(callbackData.Buffers[i], (IntPtr)buffers[i]);
            }            
            RemoveCallback(callbackId);
        }

        private static int AddNativeKernelParams( NativeKernel nk, CommandQueue cq, object o, Mem[] buffers)
        {
            int callbackId;
            NativeKernelCallbackData callbackData = new NativeKernelCallbackData(nk, cq, o, buffers);
            bool gotMutex = false;
            try
            {
                gotMutex = NativeKernelParamsMutex.WaitOne();
                do
                {
                    callbackId = NativeKernelParamsId++;
                } while (NativeKernelDispatch.ContainsKey(callbackId));
                NativeKernelDispatch.Add(callbackId, callbackData);
            }
            finally
            {
                if (gotMutex)
                    NativeKernelParamsMutex.ReleaseMutex();
            }
            return callbackId;
        }

        private static NativeKernelCallbackData GetCallback(int callbackId)
        {
            NativeKernelCallbackData callbackData = null;
            bool gotMutex = false;
            try
            {
                gotMutex = NativeKernelParamsMutex.WaitOne();
                callbackData = NativeKernelDispatch[callbackId];
            }
            finally
            {
                if (gotMutex)
                    NativeKernelParamsMutex.ReleaseMutex();
            }
            return callbackData;
        }


        private static void RemoveCallback(int callbackId)
        {
            bool gotMutex = false;
            try
            {
                gotMutex = NativeKernelParamsMutex.WaitOne();
                NativeKernelDispatch.Remove(callbackId);
            }
            finally
            {
                if (gotMutex)
                    NativeKernelParamsMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Enqueue a user function. This function is only supported if
        /// DeviceExecCapabilities.NATIVE_KERNEL set in Device.ExecutionCapabilities
        /// </summary>
        /// <param name="nativeKernel"></param>
        /// <param name="numEventsInWaitList"></param>
        /// <param name="event_wait_list"></param>
        /// <param name="_event"></param>
        public void EnqueueNativeKernel(NativeKernel nativeKernel, object userObject, Mem[] buffers, int numEventsInWaitList, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;
            int callbackId;
            
            callbackId = AddNativeKernelParams(nativeKernel, this, userObject, buffers);
            result = OpenCL.EnqueueNativeKernel(CommandQueueID,
                NativeKernelDelegate,
                &callbackId,
                (IntPtr)4,
                0,
                null,
                null,
                (uint)numEventsInWaitList,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                &tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueNativeKernel failed with error code " + result, result);
            _event = new Event(Context, this, tmpEvent);
        }

        /// <summary>
        /// Enquque a user function. This function is only supported if
        /// DeviceExecCapabilities.NATIVE_KERNEL set in Device.ExecutionCapabilities
        /// </summary>
        /// <param name="nativeKernel"></param>
        /// <param name="numEventsInWaitList"></param>
        /// <param name="event_wait_list"></param>
        public void EnqueueNativeKernel(NativeKernel nativeKernel, object userObject, Mem[] buffers, int numEventsInWaitList, Event[] event_wait_list)
        {
            ErrorCode result;
            int callbackId;

            callbackId = AddNativeKernelParams(nativeKernel, this, userObject, buffers);
            result = OpenCL.EnqueueNativeKernel(CommandQueueID,
                NativeKernelDelegate,
                &callbackId,
                (IntPtr)4,
                0,
                null,
                null,
                (uint)numEventsInWaitList,
                InteropTools.ConvertEventsToEventIDs( event_wait_list ),
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueNativeKernel failed with error code " + result, result);
        }

        /// <summary>
        /// Enquque a user function. This function is only supported if
        /// DeviceExecCapabilities.NATIVE_KERNEL set in Device.ExecutionCapabilities
        /// </summary>
        /// <param name="nativeKernel"></param>
        public void EnqueueNativeKernel(NativeKernel nativeKernel, object userObject, Mem[] buffers)
        {
            ErrorCode result;
            int callbackId;

            callbackId = AddNativeKernelParams(nativeKernel, this, userObject, buffers);
            result = OpenCL.EnqueueNativeKernel(CommandQueueID,
                NativeKernelDelegate,
                &callbackId,
                (IntPtr)4,
                0,
                null,
                null,
                0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueNativeKernel failed with error code " + result, result);
        }

        #endregion

        #region EnqueueAcquireGLObjects

        public void EnqueueAcquireGLObjects(int numObjects, Mem[] memObjects, int numEventsInWaitList, Event[] event_wait_list, out Event _event)
        {
            IntPtr tmpEvent;
            ErrorCode result;

            result = OpenCL.EnqueueAcquireGLObjects(CommandQueueID,
                (uint)numObjects,
                InteropTools.ConvertMemToMemIDs(memObjects),
                (uint)numEventsInWaitList,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                &tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueAcquireGLObjects failed with error code " + result, result);
            _event = new Event(Context, this, tmpEvent);
        }

        public void EnqueueAcquireGLObjects(int numObjects, Mem[] memObjects, int numEventsInWaitList, Event[] event_wait_list)
        {
            ErrorCode result;

            result = OpenCL.EnqueueAcquireGLObjects(CommandQueueID,
                (uint)numObjects,
                InteropTools.ConvertMemToMemIDs(memObjects),
                (uint)numEventsInWaitList,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueAcquireGLObjects failed with error code " + result, result);
        }

        public void EnqueueAcquireGLObjects(int numObjects, Mem[] memObjects)
        {
            ErrorCode result;

            result = OpenCL.EnqueueAcquireGLObjects(CommandQueueID,
                (uint)numObjects,
                InteropTools.ConvertMemToMemIDs(memObjects),
                (uint)0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueAcquireGLObjects failed with error code " + result, result);
        }

        #endregion

        #region EnqueueReleaseGLObjects

        public void EnqueueReleaseGLObjects(int numObjects, Mem[] memObjects, int numEventsInWaitList, Event[] event_wait_list, out Event _event)
        {
            IntPtr tmpEvent;
            ErrorCode result;

            result = OpenCL.EnqueueReleaseGLObjects(CommandQueueID,
                (uint)numObjects,
                InteropTools.ConvertMemToMemIDs(memObjects),
                (uint)numEventsInWaitList,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                &tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReleaseGLObjects failed with error code " + result, result);
            _event = new Event(Context, this, tmpEvent);
        }

        public void EnqueueReleaseGLObjects(int numObjects, Mem[] memObjects, int numEventsInWaitList, Event[] event_wait_list)
        {
            ErrorCode result;

            result = OpenCL.EnqueueReleaseGLObjects(CommandQueueID,
                (uint)numObjects,
                InteropTools.ConvertMemToMemIDs(memObjects),
                (uint)numEventsInWaitList,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReleaseGLObjects failed with error code " + result, result);
        }

        public void EnqueueReleaseGLObjects(int numObjects, Mem[] memObjects)
        {
            ErrorCode result;

            result = OpenCL.EnqueueReleaseGLObjects(CommandQueueID,
                (uint)numObjects,
                InteropTools.ConvertMemToMemIDs(memObjects),
                (uint)0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReleaseGLObjects failed with error code " + result, result);
        }

        #endregion

        #region EnqueueMarker

        public void EnqueueMarker( out Event _event )
        {
            IntPtr tmpEvent;

            OpenCL.EnqueueMarker(CommandQueueID, &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
        }

        #endregion

        #region EnqueueWaitForEvents

        public void EnqueueWaitForEvents(int num_events, Event[] _event_list)
        {
            OpenCL.EnqueueWaitForEvents(CommandQueueID, (uint)num_events, InteropTools.ConvertEventsToEventIDs(_event_list));
        }

        public void EnqueueWaitForEvent( Event _event)
        {
            Event[] waitList = new Event[] { _event };
            EnqueueWaitForEvents(1, waitList);
        }

        #endregion

        #region EnqueueBarrier

        public void EnqueueBarrier()
        {
            OpenCL.EnqueueBarrier(CommandQueueID);
        }

        #endregion

        #region Flush

        public void Flush()
        {
            OpenCL.Flush(CommandQueueID);
        }

        #endregion

        #region Finish

        public void Finish()
        {
            OpenCL.Finish(CommandQueueID);
        }

        #endregion

        #region SetProperty

        [Obsolete("Function deprecated in OpenCL 1.1 due to being inherently unsafe", false)]
        public void SetProperty(CommandQueueProperties properties, bool enable, out CommandQueueProperties oldProperties)
        {
            ErrorCode result;
            ulong returnedProperties = 0;
#pragma warning disable 618
            result = (ErrorCode)OpenCL.SetCommandQueueProperty( CommandQueueID,
                (ulong)properties,
                enable,
                out returnedProperties );
            if( result!=ErrorCode.SUCCESS )
                throw new OpenCLException( "SetCommandQueueProperty failed with error code "+result , result);
            oldProperties = (CommandQueueProperties)returnedProperties;
#pragma warning restore 618
        }

        #endregion

        #region EnqueueReadBufferRect

        /// <summary>
        /// OpenCL 1.1
        /// </summary>
        /// <param name="command_queue"></param>
        /// <param name="buffer"></param>
        /// <param name="blocking_read"></param>
        /// <param name="buffer_offset"></param>
        /// <param name="host_offset"></param>
        /// <param name="region"></param>
        /// <param name="buffer_row_pitch"></param>
        /// <param name="buffer_slice_pitch"></param>
        /// <param name="host_row_pitch"></param>
        /// <param name="host_slice_pitch"></param>
        /// <param name="ptr"></param>
        /// <param name="num_events_in_wait_list"></param>
        /// <param name="event_wait_list"></param>
        /// <param name="_event"></param>
        public void EnqueueReadBufferRect(Mem buffer,
                                bool blocking_read,
                                IntPtr[] buffer_offset,
                                IntPtr[] host_offset,
                                IntPtr[] region,
                                IntPtr buffer_row_pitch,
                                IntPtr buffer_slice_pitch,
                                IntPtr host_row_pitch,
                                IntPtr host_slice_pitch,
                                IntPtr ptr,
                                uint num_events_in_wait_list,
                                Event[] event_wait_list,
                                out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;

            result = OpenCL.EnqueueReadBufferRect(CommandQueueID,
                buffer.MemID,
                (uint)(blocking_read ? Bool.TRUE : Bool.FALSE),
                buffer_offset,
                host_offset,
                region,
                buffer_row_pitch,
                buffer_slice_pitch,
                host_row_pitch,
                host_slice_pitch,
                ptr.ToPointer(),
                num_events_in_wait_list,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReadBufferRect failed with error code " + result, result);
        }
        public void EnqueueReadBufferRect(Mem buffer, bool blocking_read, int[] buffer_offset, int[] host_offset, int[] region, int buffer_row_pitch, int buffer_slice_pitch, int host_row_pitch, int host_slice_pitch, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;
            IntPtr* pBufferOffset = stackalloc IntPtr[3];
            IntPtr* pHostOffset = stackalloc IntPtr[3];
            IntPtr* pRegion = stackalloc IntPtr[3];
            IntPtr* pRepackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                pRepackedEvents = null;
            InteropTools.A3ToIntPtr3(buffer_offset, pBufferOffset);
            InteropTools.A3ToIntPtr3(host_offset, pHostOffset);
            InteropTools.A3ToIntPtr3(region, pRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, pRepackedEvents);

            result = OpenCL.EnqueueReadBufferRect(CommandQueueID,
                buffer.MemID,
                (uint)(blocking_read ? Bool.TRUE : Bool.FALSE),
                pBufferOffset,
                pHostOffset,
                pRegion,
                buffer_row_pitch,
                buffer_slice_pitch,
                host_row_pitch,
                host_slice_pitch,
                ptr.ToPointer(),
                num_events_in_wait_list,
                pRepackedEvents,
                &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReadBufferRect failed with error code " + result, result);
        }
        public void EnqueueReadBufferRect(Mem buffer, bool blocking_read, long[] buffer_offset, long[] host_offset, long[] region, long buffer_row_pitch, long buffer_slice_pitch, long host_row_pitch, long host_slice_pitch, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;
            IntPtr* pBufferOffset = stackalloc IntPtr[3];
            IntPtr* pHostOffset = stackalloc IntPtr[3];
            IntPtr* pRegion = stackalloc IntPtr[3];
            IntPtr* pRepackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                pRepackedEvents = null;
            InteropTools.A3ToIntPtr3(buffer_offset, pBufferOffset);
            InteropTools.A3ToIntPtr3(host_offset, pHostOffset);
            InteropTools.A3ToIntPtr3(region, pRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, pRepackedEvents);

            result = OpenCL.EnqueueReadBufferRect(CommandQueueID,
                buffer.MemID,
                (uint)(blocking_read ? Bool.TRUE : Bool.FALSE),
                pBufferOffset,
                pHostOffset,
                pRegion,
                buffer_row_pitch,
                buffer_slice_pitch,
                host_row_pitch,
                host_slice_pitch,
                ptr.ToPointer(),
                num_events_in_wait_list,
                pRepackedEvents,
                &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReadBufferRect failed with error code " + result, result);
        }

        public void EnqueueReadBufferRect( Mem buffer,
                                bool blocking_read,
                                IntPtr[] buffer_offset,
                                IntPtr[] host_offset,
                                IntPtr[] region,
                                IntPtr buffer_row_pitch,
                                IntPtr buffer_slice_pitch,
                                IntPtr host_row_pitch,
                                IntPtr host_slice_pitch,
                                IntPtr ptr,
                                uint num_events_in_wait_list,
                                Event[] event_wait_list)
        {
            ErrorCode result;

            result = OpenCL.EnqueueReadBufferRect(CommandQueueID,
                buffer.MemID,
                (uint)(blocking_read ? Bool.TRUE : Bool.FALSE),
                buffer_offset,
                host_offset,
                region,
                buffer_row_pitch,
                buffer_slice_pitch,
                host_row_pitch,
                host_slice_pitch,
                ptr.ToPointer(),
                num_events_in_wait_list,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReadBufferRect failed with error code " + result, result);
        }
        public void EnqueueReadBufferRect(Mem buffer, bool blocking_read, int[] buffer_offset, int[] host_offset, int[] region, int buffer_row_pitch, int buffer_slice_pitch, int host_row_pitch, int host_slice_pitch, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;
            IntPtr* pBufferOffset = stackalloc IntPtr[3];
            IntPtr* pHostOffset = stackalloc IntPtr[3];
            IntPtr* pRegion = stackalloc IntPtr[3];
            IntPtr* pRepackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                pRepackedEvents = null;
            InteropTools.A3ToIntPtr3(buffer_offset, pBufferOffset);
            InteropTools.A3ToIntPtr3(host_offset, pHostOffset);
            InteropTools.A3ToIntPtr3(region, pRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, pRepackedEvents);

            result = OpenCL.EnqueueReadBufferRect(CommandQueueID,
                buffer.MemID,
                (uint)(blocking_read ? Bool.TRUE : Bool.FALSE),
                pBufferOffset,
                pHostOffset,
                pRegion,
                buffer_row_pitch,
                buffer_slice_pitch,
                host_row_pitch,
                host_slice_pitch,
                ptr.ToPointer(),
                num_events_in_wait_list,
                pRepackedEvents,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReadBufferRect failed with error code " + result, result);
        }
        public void EnqueueReadBufferRect(Mem buffer, bool blocking_read, long[] buffer_offset, long[] host_offset, long[] region, long buffer_row_pitch, long buffer_slice_pitch, long host_row_pitch, long host_slice_pitch, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;
            IntPtr* pBufferOffset = stackalloc IntPtr[3];
            IntPtr* pHostOffset = stackalloc IntPtr[3];
            IntPtr* pRegion = stackalloc IntPtr[3];
            IntPtr* pRepackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                pRepackedEvents = null;
            InteropTools.A3ToIntPtr3(buffer_offset, pBufferOffset);
            InteropTools.A3ToIntPtr3(host_offset, pHostOffset);
            InteropTools.A3ToIntPtr3(region, pRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, pRepackedEvents);

            result = OpenCL.EnqueueReadBufferRect(CommandQueueID,
                buffer.MemID,
                (uint)(blocking_read ? Bool.TRUE : Bool.FALSE),
                pBufferOffset,
                pHostOffset,
                pRegion,
                buffer_row_pitch,
                buffer_slice_pitch,
                host_row_pitch,
                host_slice_pitch,
                ptr.ToPointer(),
                num_events_in_wait_list,
                pRepackedEvents,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReadBufferRect failed with error code " + result, result);
        }

        public void EnqueueReadBufferRect( Mem buffer,
                                bool blocking_read,
                                IntPtr[] buffer_offset,
                                IntPtr[] host_offset,
                                IntPtr[] region,
                                IntPtr buffer_row_pitch,
                                IntPtr buffer_slice_pitch,
                                IntPtr host_row_pitch,
                                IntPtr host_slice_pitch,
                                IntPtr ptr)
        {
            ErrorCode result;

            result = OpenCL.EnqueueReadBufferRect(CommandQueueID,
                buffer.MemID,
                (uint)(blocking_read ? Bool.TRUE : Bool.FALSE),
                buffer_offset,
                host_offset,
                region,
                buffer_row_pitch,
                buffer_slice_pitch,
                host_row_pitch,
                host_slice_pitch,
                ptr.ToPointer(),
                0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReadBufferRect failed with error code " + result, result);
        }
        public void EnqueueReadBufferRect(Mem buffer, bool blocking_read, int[] buffer_offset, int[] host_offset, int[] region, int buffer_row_pitch, int buffer_slice_pitch, int host_row_pitch, int host_slice_pitch, IntPtr ptr)
        {
            ErrorCode result;
            IntPtr* pBufferOffset = stackalloc IntPtr[3];
            IntPtr* pHostOffset = stackalloc IntPtr[3];
            IntPtr* pRegion = stackalloc IntPtr[3];

            InteropTools.A3ToIntPtr3(buffer_offset, pBufferOffset);
            InteropTools.A3ToIntPtr3(host_offset, pHostOffset);
            InteropTools.A3ToIntPtr3(region, pRegion);

            result = OpenCL.EnqueueReadBufferRect(CommandQueueID,
                buffer.MemID,
                (uint)(blocking_read ? Bool.TRUE : Bool.FALSE),
                pBufferOffset,
                pHostOffset,
                pRegion,
                buffer_row_pitch,
                buffer_slice_pitch,
                host_row_pitch,
                host_slice_pitch,
                ptr.ToPointer(),
                0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReadBufferRect failed with error code " + result, result);
        }
        public void EnqueueReadBufferRect(Mem buffer, bool blocking_read, long[] buffer_offset, long[] host_offset, long[] region, long buffer_row_pitch, long buffer_slice_pitch, long host_row_pitch, long host_slice_pitch, IntPtr ptr)
        {
            ErrorCode result;
            IntPtr* pBufferOffset = stackalloc IntPtr[3];
            IntPtr* pHostOffset = stackalloc IntPtr[3];
            IntPtr* pRegion = stackalloc IntPtr[3];

            InteropTools.A3ToIntPtr3(buffer_offset, pBufferOffset);
            InteropTools.A3ToIntPtr3(host_offset, pHostOffset);
            InteropTools.A3ToIntPtr3(region, pRegion);

            result = OpenCL.EnqueueReadBufferRect(CommandQueueID,
                buffer.MemID,
                (uint)(blocking_read ? Bool.TRUE : Bool.FALSE),
                pBufferOffset,
                pHostOffset,
                pRegion,
                buffer_row_pitch,
                buffer_slice_pitch,
                host_row_pitch,
                host_slice_pitch,
                ptr.ToPointer(),
                0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueReadBufferRect failed with error code " + result, result);
        }

        #endregion
        
        #region EnqueueWriteBufferRect

        /// <summary>
        /// OpenCL 1.1
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="blocking_write"></param>
        /// <param name="buffer_offset"></param>
        /// <param name="host_offset"></param>
        /// <param name="region"></param>
        /// <param name="buffer_row_pitch"></param>
        /// <param name="buffer_slice_pitch"></param>
        /// <param name="host_row_pitch"></param>
        /// <param name="host_slice_pitch"></param>
        /// <param name="ptr"></param>
        /// <param name="num_events_in_wait_list"></param>
        /// <param name="event_wait_list"></param>
        /// <param name="_event"></param>
        public void EnqueueWriteBufferRect(Mem buffer,
                                 bool blocking_write,
                                 IntPtr[] buffer_offset,
                                 IntPtr[] host_offset,
                                 IntPtr[] region,
                                 IntPtr buffer_row_pitch,
                                 IntPtr buffer_slice_pitch,
                                 IntPtr host_row_pitch,
                                 IntPtr host_slice_pitch,
                                 IntPtr ptr,
                                 uint num_events_in_wait_list,
                                 Event[] event_wait_list,
                                 out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;

            result = OpenCL.EnqueueWriteBufferRect(CommandQueueID,
                buffer.MemID,
                blocking_write ? 1u : 0u,
                buffer_offset,
                host_offset,
                region,
                buffer_row_pitch,
                buffer_slice_pitch,
                host_row_pitch,
                host_slice_pitch,
                ptr.ToPointer(),
                num_events_in_wait_list,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueWriteBufferRect failed with error code " + result, result);
        }
        public void EnqueueWriteBufferRect(Mem buffer, bool blocking_write, int[] buffer_offset, int[] host_offset, int[] region, int buffer_row_pitch, int buffer_slice_pitch, int host_row_pitch, int host_slice_pitch, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;
            IntPtr* pBufferOffset = stackalloc IntPtr[3];
            IntPtr* pHostOffset = stackalloc IntPtr[3];
            IntPtr* pRegion = stackalloc IntPtr[3];
            IntPtr* pRepackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                pRepackedEvents = null;
            InteropTools.A3ToIntPtr3(buffer_offset, pBufferOffset);
            InteropTools.A3ToIntPtr3(host_offset, pHostOffset);
            InteropTools.A3ToIntPtr3(region, pRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, pRepackedEvents);

            result = OpenCL.EnqueueWriteBufferRect(CommandQueueID,
                buffer.MemID,
                (uint)(blocking_write ? Bool.TRUE : Bool.FALSE),
                pBufferOffset,
                pHostOffset,
                pRegion,
                buffer_row_pitch,
                buffer_slice_pitch,
                host_row_pitch,
                host_slice_pitch,
                ptr.ToPointer(),
                num_events_in_wait_list,
                pRepackedEvents,
                &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueWriteBufferRect failed with error code " + result, result);
        }
        public void EnqueueWriteBufferRect(Mem buffer, bool blocking_write, long[] buffer_offset, long[] host_offset, long[] region, long buffer_row_pitch, long buffer_slice_pitch, long host_row_pitch, long host_slice_pitch, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;
            IntPtr* pBufferOffset = stackalloc IntPtr[3];
            IntPtr* pHostOffset = stackalloc IntPtr[3];
            IntPtr* pRegion = stackalloc IntPtr[3];
            IntPtr* pRepackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                pRepackedEvents = null;
            InteropTools.A3ToIntPtr3(buffer_offset, pBufferOffset);
            InteropTools.A3ToIntPtr3(host_offset, pHostOffset);
            InteropTools.A3ToIntPtr3(region, pRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, pRepackedEvents);

            result = OpenCL.EnqueueWriteBufferRect(CommandQueueID,
                buffer.MemID,
                (uint)(blocking_write ? Bool.TRUE : Bool.FALSE),
                pBufferOffset,
                pHostOffset,
                pRegion,
                buffer_row_pitch,
                buffer_slice_pitch,
                host_row_pitch,
                host_slice_pitch,
                ptr.ToPointer(),
                num_events_in_wait_list,
                pRepackedEvents,
                &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueWriteBufferRect failed with error code " + result, result);
        }

        public void EnqueueWriteBufferRect(Mem buffer,
                                 bool blocking_write,
                                 IntPtr[] buffer_offset,
                                 IntPtr[] host_offset,
                                 IntPtr[] region,
                                 IntPtr buffer_row_pitch,
                                 IntPtr buffer_slice_pitch,
                                 IntPtr host_row_pitch,
                                 IntPtr host_slice_pitch,
                                 IntPtr ptr,
                                 uint num_events_in_wait_list,
                                 Event[] event_wait_list)
        {
            ErrorCode result;

            result = OpenCL.EnqueueWriteBufferRect(CommandQueueID,
                buffer.MemID,
                blocking_write ? 1u : 0u,
                buffer_offset,
                host_offset,
                region,
                buffer_row_pitch,
                buffer_slice_pitch,
                host_row_pitch,
                host_slice_pitch,
                ptr.ToPointer(),
                num_events_in_wait_list,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                null);

            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueWriteBufferRect failed with error code " + result, result);
        }
        public void EnqueueWriteBufferRect(Mem buffer, bool blocking_write, int[] buffer_offset, int[] host_offset, int[] region, int buffer_row_pitch, int buffer_slice_pitch, int host_row_pitch, int host_slice_pitch, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;
            IntPtr* pBufferOffset = stackalloc IntPtr[3];
            IntPtr* pHostOffset = stackalloc IntPtr[3];
            IntPtr* pRegion = stackalloc IntPtr[3];
            IntPtr* pRepackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                pRepackedEvents = null;
            InteropTools.A3ToIntPtr3(buffer_offset, pBufferOffset);
            InteropTools.A3ToIntPtr3(host_offset, pHostOffset);
            InteropTools.A3ToIntPtr3(region, pRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, pRepackedEvents);

            result = OpenCL.EnqueueWriteBufferRect(CommandQueueID,
                buffer.MemID,
                (uint)(blocking_write ? Bool.TRUE : Bool.FALSE),
                pBufferOffset,
                pHostOffset,
                pRegion,
                buffer_row_pitch,
                buffer_slice_pitch,
                host_row_pitch,
                host_slice_pitch,
                ptr.ToPointer(),
                num_events_in_wait_list,
                pRepackedEvents,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueWriteBufferRect failed with error code " + result, result);
        }
        public void EnqueueWriteBufferRect(Mem buffer, bool blocking_write, long[] buffer_offset, long[] host_offset, long[] region, long buffer_row_pitch, long buffer_slice_pitch, long host_row_pitch, long host_slice_pitch, IntPtr ptr, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;
            IntPtr* pBufferOffset = stackalloc IntPtr[3];
            IntPtr* pHostOffset = stackalloc IntPtr[3];
            IntPtr* pRegion = stackalloc IntPtr[3];
            IntPtr* pRepackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                pRepackedEvents = null;
            InteropTools.A3ToIntPtr3(buffer_offset, pBufferOffset);
            InteropTools.A3ToIntPtr3(host_offset, pHostOffset);
            InteropTools.A3ToIntPtr3(region, pRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, pRepackedEvents);

            result = OpenCL.EnqueueWriteBufferRect(CommandQueueID,
                buffer.MemID,
                (uint)(blocking_write ? Bool.TRUE : Bool.FALSE),
                pBufferOffset,
                pHostOffset,
                pRegion,
                buffer_row_pitch,
                buffer_slice_pitch,
                host_row_pitch,
                host_slice_pitch,
                ptr.ToPointer(),
                num_events_in_wait_list,
                pRepackedEvents,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueWriteBufferRect failed with error code " + result, result);
        }

        public void EnqueueWriteBufferRect(Mem buffer,
                                 bool blocking_write,
                                 IntPtr[] buffer_offset,
                                 IntPtr[] host_offset,
                                 IntPtr[] region,
                                 IntPtr buffer_row_pitch,
                                 IntPtr buffer_slice_pitch,
                                 IntPtr host_row_pitch,
                                 IntPtr host_slice_pitch,
                                 IntPtr ptr)
        {
            ErrorCode result;

            result = OpenCL.EnqueueWriteBufferRect(CommandQueueID,
                buffer.MemID,
                blocking_write ? 1u : 0u,
                buffer_offset,
                host_offset,
                region,
                buffer_row_pitch,
                buffer_slice_pitch,
                host_row_pitch,
                host_slice_pitch,
                ptr.ToPointer(),
                0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueWriteBufferRect failed with error code " + result, result);
        }
        public void EnqueueWriteBufferRect(Mem buffer, bool blocking_write, int[] buffer_offset, int[] host_offset, int[] region, int buffer_row_pitch, int buffer_slice_pitch, int host_row_pitch, int host_slice_pitch, IntPtr ptr)
        {
            ErrorCode result;
            IntPtr* pBufferOffset = stackalloc IntPtr[3];
            IntPtr* pHostOffset = stackalloc IntPtr[3];
            IntPtr* pRegion = stackalloc IntPtr[3];

            InteropTools.A3ToIntPtr3(buffer_offset, pBufferOffset);
            InteropTools.A3ToIntPtr3(host_offset, pHostOffset);
            InteropTools.A3ToIntPtr3(region, pRegion);

            result = OpenCL.EnqueueWriteBufferRect(CommandQueueID,
                buffer.MemID,
                (uint)(blocking_write ? Bool.TRUE : Bool.FALSE),
                pBufferOffset,
                pHostOffset,
                pRegion,
                buffer_row_pitch,
                buffer_slice_pitch,
                host_row_pitch,
                host_slice_pitch,
                ptr.ToPointer(),
                0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueWriteBufferRect failed with error code " + result, result);
        }
        public void EnqueueWriteBufferRect(Mem buffer, bool blocking_write, long[] buffer_offset, long[] host_offset, long[] region, long buffer_row_pitch, long buffer_slice_pitch, long host_row_pitch, long host_slice_pitch, IntPtr ptr)
        {
            ErrorCode result;
            IntPtr* pBufferOffset = stackalloc IntPtr[3];
            IntPtr* pHostOffset = stackalloc IntPtr[3];
            IntPtr* pRegion = stackalloc IntPtr[3];

            InteropTools.A3ToIntPtr3(buffer_offset, pBufferOffset);
            InteropTools.A3ToIntPtr3(host_offset, pHostOffset);
            InteropTools.A3ToIntPtr3(region, pRegion);

            result = OpenCL.EnqueueWriteBufferRect(CommandQueueID,
                buffer.MemID,
                (uint)(blocking_write ? Bool.TRUE : Bool.FALSE),
                pBufferOffset,
                pHostOffset,
                pRegion,
                buffer_row_pitch,
                buffer_slice_pitch,
                host_row_pitch,
                host_slice_pitch,
                ptr.ToPointer(),
                0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueWriteBufferRect failed with error code " + result, result);
        }

        #endregion

        #region EnqueueCopyBufferRect

        /// <summary>
        /// OpenCL 1.1
        /// </summary>
        /// <param name="src_buffer"></param>
        /// <param name="dst_buffer"></param>
        /// <param name="src_origin"></param>
        /// <param name="dst_origin"></param>
        /// <param name="region"></param>
        /// <param name="src_row_pitch"></param>
        /// <param name="src_slice_pitch"></param>
        /// <param name="dst_row_pitch"></param>
        /// <param name="dst_slice_pitch"></param>
        /// <param name="num_events_in_wait_list"></param>
        /// <param name="event_wait_list"></param>
        /// <param name="_event"></param>
        public void EnqueueCopyBufferRect( Mem src_buffer,
                                Mem dst_buffer,
                                IntPtr[] src_origin,
                                IntPtr[] dst_origin,
                                IntPtr[] region,
                                IntPtr src_row_pitch,
                                IntPtr src_slice_pitch,
                                IntPtr dst_row_pitch,
                                IntPtr dst_slice_pitch,
                                uint num_events_in_wait_list,
                                Event[] event_wait_list,
                                out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;

            result = OpenCL.EnqueueCopyBufferRect(CommandQueueID,
                src_buffer.MemID,
                dst_buffer.MemID,
                src_origin,
                dst_origin,
                region,
                src_row_pitch,
                src_slice_pitch,
                dst_row_pitch,
                dst_slice_pitch,
                num_events_in_wait_list,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyBufferRect failed with error code " + result, result);
        }
        public void EnqueueCopyBufferRect(Mem src_buffer, Mem dst_buffer, int[] src_origin, int[] dst_origin, int[] region, int src_row_pitch, int src_slice_pitch, int dst_row_pitch, int dst_slice_pitch, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;
            IntPtr* pSrcOrigin = stackalloc IntPtr[3];
            IntPtr* pDstOrigin = stackalloc IntPtr[3];
            IntPtr* pRegion = stackalloc IntPtr[3];
            IntPtr* pRepackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                pRepackedEvents = null;
            InteropTools.A3ToIntPtr3(src_origin, pSrcOrigin);
            InteropTools.A3ToIntPtr3(dst_origin, pDstOrigin);
            InteropTools.A3ToIntPtr3(region, pRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, pRepackedEvents);

            result = OpenCL.EnqueueCopyBufferRect(CommandQueueID,
                src_buffer.MemID,
                dst_buffer.MemID,
                pSrcOrigin,
                pDstOrigin,
                pRegion,
                src_row_pitch,
                src_slice_pitch,
                dst_row_pitch,
                dst_slice_pitch,
                num_events_in_wait_list,
                pRepackedEvents,
                &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyBufferRect failed with error code " + result, result);
        }
        public void EnqueueCopyBufferRect(Mem src_buffer, Mem dst_buffer, long[] src_origin, long[] dst_origin, long[] region, long src_row_pitch, long src_slice_pitch, long dst_row_pitch, long dst_slice_pitch, int num_events_in_wait_list, Event[] event_wait_list, out Event _event)
        {
            ErrorCode result;
            IntPtr tmpEvent;
            IntPtr* pSrcOrigin = stackalloc IntPtr[3];
            IntPtr* pDstOrigin = stackalloc IntPtr[3];
            IntPtr* pRegion = stackalloc IntPtr[3];
            IntPtr* pRepackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                pRepackedEvents = null;
            InteropTools.A3ToIntPtr3(src_origin, pSrcOrigin);
            InteropTools.A3ToIntPtr3(dst_origin, pDstOrigin);
            InteropTools.A3ToIntPtr3(region, pRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, pRepackedEvents);

            result = OpenCL.EnqueueCopyBufferRect(CommandQueueID,
                src_buffer.MemID,
                dst_buffer.MemID,
                pSrcOrigin,
                pDstOrigin,
                pRegion,
                src_row_pitch,
                src_slice_pitch,
                dst_row_pitch,
                dst_slice_pitch,
                num_events_in_wait_list,
                pRepackedEvents,
                &tmpEvent);
            _event = new Event(Context, this, tmpEvent);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyBufferRect failed with error code " + result, result);
        }

        public void EnqueueCopyBufferRect(Mem src_buffer,
                                Mem dst_buffer,
                                IntPtr[] src_origin,
                                IntPtr[] dst_origin,
                                IntPtr[] region,
                                IntPtr src_row_pitch,
                                IntPtr src_slice_pitch,
                                IntPtr dst_row_pitch,
                                IntPtr dst_slice_pitch,
                                uint num_events_in_wait_list,
                                Event[] event_wait_list)
        {
            ErrorCode result;

            result = OpenCL.EnqueueCopyBufferRect(CommandQueueID,
                src_buffer.MemID,
                dst_buffer.MemID,
                src_origin,
                dst_origin,
                region,
                src_row_pitch,
                src_slice_pitch,
                dst_row_pitch,
                dst_slice_pitch,
                num_events_in_wait_list,
                InteropTools.ConvertEventsToEventIDs(event_wait_list),
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyBufferRect failed with error code " + result, result);
        }
        public void EnqueueCopyBufferRect(Mem src_buffer, Mem dst_buffer, int[] src_origin, int[] dst_origin, int[] region, int src_row_pitch, int src_slice_pitch, int dst_row_pitch, int dst_slice_pitch, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;
            IntPtr* pSrcOrigin = stackalloc IntPtr[3];
            IntPtr* pDstOrigin = stackalloc IntPtr[3];
            IntPtr* pRegion = stackalloc IntPtr[3];
            IntPtr* pRepackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                pRepackedEvents = null;
            InteropTools.A3ToIntPtr3(src_origin, pSrcOrigin);
            InteropTools.A3ToIntPtr3(dst_origin, pDstOrigin);
            InteropTools.A3ToIntPtr3(region, pRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, pRepackedEvents);

            result = OpenCL.EnqueueCopyBufferRect(CommandQueueID,
                src_buffer.MemID,
                dst_buffer.MemID,
                pSrcOrigin,
                pDstOrigin,
                pRegion,
                src_row_pitch,
                src_slice_pitch,
                dst_row_pitch,
                dst_slice_pitch,
                num_events_in_wait_list,
                pRepackedEvents,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyBufferRect failed with error code " + result, result);
        }
        public void EnqueueCopyBufferRect(Mem src_buffer, Mem dst_buffer, long[] src_origin, long[] dst_origin, long[] region, long src_row_pitch, long src_slice_pitch, long dst_row_pitch, long dst_slice_pitch, int num_events_in_wait_list, Event[] event_wait_list)
        {
            ErrorCode result;
            IntPtr* pSrcOrigin = stackalloc IntPtr[3];
            IntPtr* pDstOrigin = stackalloc IntPtr[3];
            IntPtr* pRegion = stackalloc IntPtr[3];
            IntPtr* pRepackedEvents = stackalloc IntPtr[num_events_in_wait_list];

            if (num_events_in_wait_list == 0)
                pRepackedEvents = null;
            InteropTools.A3ToIntPtr3(src_origin, pSrcOrigin);
            InteropTools.A3ToIntPtr3(dst_origin, pDstOrigin);
            InteropTools.A3ToIntPtr3(region, pRegion);
            InteropTools.ConvertEventsToEventIDs(num_events_in_wait_list, event_wait_list, pRepackedEvents);

            result = OpenCL.EnqueueCopyBufferRect(CommandQueueID,
                src_buffer.MemID,
                dst_buffer.MemID,
                pSrcOrigin,
                pDstOrigin,
                pRegion,
                src_row_pitch,
                src_slice_pitch,
                dst_row_pitch,
                dst_slice_pitch,
                num_events_in_wait_list,
                pRepackedEvents,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyBufferRect failed with error code " + result, result);
        }

        public void EnqueueCopyBufferRect(Mem src_buffer,
                                Mem dst_buffer,
                                IntPtr[] src_origin,
                                IntPtr[] dst_origin,
                                IntPtr[] region,
                                IntPtr src_row_pitch,
                                IntPtr src_slice_pitch,
                                IntPtr dst_row_pitch,
                                IntPtr dst_slice_pitch)
        {
            ErrorCode result;

            result = OpenCL.EnqueueCopyBufferRect(CommandQueueID,
                src_buffer.MemID,
                dst_buffer.MemID,
                src_origin,
                dst_origin,
                region,
                src_row_pitch,
                src_slice_pitch,
                dst_row_pitch,
                dst_slice_pitch,
                0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyBufferRect failed with error code " + result, result);
        }
        public void EnqueueCopyBufferRect(Mem src_buffer, Mem dst_buffer, int[] src_origin, int[] dst_origin, int[] region, int src_row_pitch, int src_slice_pitch, int dst_row_pitch, int dst_slice_pitch)
        {
            ErrorCode result;
            IntPtr* pSrcOrigin = stackalloc IntPtr[3];
            IntPtr* pDstOrigin = stackalloc IntPtr[3];
            IntPtr* pRegion = stackalloc IntPtr[3];

            InteropTools.A3ToIntPtr3(src_origin, pSrcOrigin);
            InteropTools.A3ToIntPtr3(dst_origin, pDstOrigin);
            InteropTools.A3ToIntPtr3(region, pRegion);

            result = OpenCL.EnqueueCopyBufferRect(CommandQueueID,
                src_buffer.MemID,
                dst_buffer.MemID,
                pSrcOrigin,
                pDstOrigin,
                pRegion,
                src_row_pitch,
                src_slice_pitch,
                dst_row_pitch,
                dst_slice_pitch,
                0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyBufferRect failed with error code " + result, result);
        }
        public void EnqueueCopyBufferRect(Mem src_buffer, Mem dst_buffer, long[] src_origin, long[] dst_origin, long[] region, long src_row_pitch, long src_slice_pitch, long dst_row_pitch, long dst_slice_pitch)
        {
            ErrorCode result;
            IntPtr* pSrcOrigin = stackalloc IntPtr[3];
            IntPtr* pDstOrigin = stackalloc IntPtr[3];
            IntPtr* pRegion = stackalloc IntPtr[3];

            InteropTools.A3ToIntPtr3(src_origin, pSrcOrigin);
            InteropTools.A3ToIntPtr3(dst_origin, pDstOrigin);
            InteropTools.A3ToIntPtr3(region, pRegion);

            result = OpenCL.EnqueueCopyBufferRect(CommandQueueID,
                src_buffer.MemID,
                dst_buffer.MemID,
                pSrcOrigin,
                pDstOrigin,
                pRegion,
                src_row_pitch,
                src_slice_pitch,
                dst_row_pitch,
                dst_slice_pitch,
                0,
                null,
                null);
            if (result != ErrorCode.SUCCESS)
                throw new OpenCLException("EnqueueCopyBufferRect failed with error code " + result, result);
        }

        #endregion

        public static implicit operator IntPtr( CommandQueue cq )
        {
            return cq.CommandQueueID;
        }
    }
}
