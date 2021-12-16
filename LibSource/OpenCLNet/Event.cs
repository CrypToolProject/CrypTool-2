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
    public class Event : IDisposable, InteropTools.IPropertyContainer
    {
        #region Properties

        public IntPtr EventID { get; protected set; }
        public Context Context { get; protected set; }
        public CommandQueue CommandQueue { get; protected set; }
        public ExecutionStatus ExecutionStatus => (ExecutionStatus)InteropTools.ReadUInt(this, (uint)EventInfo.COMMAND_EXECUTION_STATUS);
        public CommandType CommandType => (CommandType)InteropTools.ReadUInt(this, (uint)EventInfo.COMMAND_TYPE);

        #endregion

        internal class CallbackData
        {
            public Event EventObject;
            public EventNotify UserMethod;
            public object UserData;

            internal CallbackData(Event _event, EventNotify userMethod, object userData)
            {
                EventObject = _event;
                UserMethod = userMethod;
                UserData = userData;
            }
        }

        private static int CallbackId;
        private static readonly Mutex CallbackMutex = new Mutex();
        private static readonly Dictionary<int, CallbackData> CallbackDispatch = new Dictionary<int, CallbackData>();
        private static readonly EventNotifyInternal CallbackDelegate = new EventNotifyInternal(EventCallback);
        private static int AddCallback(Event _event, EventNotify userMethod, object userData)
        {
            int callbackId;
            CallbackData callbackData = new CallbackData(_event, userMethod, userData);
            bool gotMutex = false;

            try
            {
                gotMutex = CallbackMutex.WaitOne();
                do
                {
                    callbackId = CallbackId++;
                } while (CallbackDispatch.ContainsKey(callbackId));
                CallbackDispatch.Add(callbackId, callbackData);
            }
            finally
            {
                if (gotMutex)
                {
                    CallbackMutex.ReleaseMutex();
                }
            }
            return callbackId;
        }

        private static CallbackData GetCallback(int callbackId)
        {
            CallbackData callbackData = null;
            bool gotMutex = false;
            try
            {
                gotMutex = CallbackMutex.WaitOne();
                callbackData = CallbackDispatch[callbackId];
            }
            finally
            {
                if (gotMutex)
                {
                    CallbackMutex.ReleaseMutex();
                }
            }
            return callbackData;
        }

        private static void RemoveCallback(int callbackId)
        {
            bool gotMutex = false;
            try
            {
                gotMutex = CallbackMutex.WaitOne();
                CallbackDispatch.Remove(callbackId);
            }
            finally
            {
                if (gotMutex)
                {
                    CallbackMutex.ReleaseMutex();
                }
            }
        }

        // Track whether Dispose has been called.
        private bool disposed = false;

        #region Construction / Destruction

        internal Event(Context context, CommandQueue cq, IntPtr eventID)
        {
            Context = context;
            CommandQueue = cq;
            EventID = eventID;
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~Event()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion

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
                OpenCL.ReleaseEvent(EventID);
                EventID = IntPtr.Zero;

                // Note disposing has been done.
                disposed = true;
            }
        }

        #endregion

        #region IPropertyContainer Members

        public unsafe IntPtr GetPropertySize(uint key)
        {
            ErrorCode result;

            result = OpenCL.GetEventInfo(EventID, key, IntPtr.Zero, null, out IntPtr size);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("GetEventInfo failed; " + result, result);
            }

            return size;
        }

        public unsafe void ReadProperty(uint key, IntPtr keyLength, void* pBuffer)
        {
            ErrorCode result;

            result = OpenCL.GetEventInfo(EventID, key, keyLength, pBuffer, out IntPtr size);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("GetEventInfo failed; " + result, result);
            }
        }

        #endregion

        /// <summary>
        /// Block the current thread until this event is completed
        /// </summary>
        public void Wait()
        {
            Context.WaitForEvent(this);
        }

        /// <summary>
        /// OpenCL 1.1
        /// </summary>
        /// <param name="_event"></param>
        /// <param name="execution_status"></param>
        public void SetUserEventStatus(ExecutionStatus execution_status)
        {
            ErrorCode result;

            result = OpenCL.SetUserEventStatus(EventID, execution_status);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetUserEventStatus failed with error code " + result, result);
            }
        }

        /// <summary>
        /// OpenCL 1.1
        /// </summary>
        /// <param name="command_exec_callback_type"></param>
        /// <param name="pfn_notify"></param>
        /// <param name="user_data"></param>
        public void SetCallback(ExecutionStatus command_exec_callback_type, EventNotify pfn_notify, object user_data)
        {
            ErrorCode result;
            int callbackId = AddCallback(this, pfn_notify, user_data);

            result = OpenCL.SetEventCallback(EventID, (int)command_exec_callback_type, CallbackDelegate, (IntPtr)callbackId);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("SetEventCallback failed with error code " + result, result);
            }
        }

        private static void EventCallback(IntPtr eventId, int executionStatus, IntPtr userData)
        {
            int callbackId = userData.ToInt32();
            CallbackData callbackData = GetCallback(callbackId);
            callbackData.UserMethod(callbackData.EventObject, (ExecutionStatus)executionStatus, callbackData.UserData);
            RemoveCallback(callbackId);
        }

        /// <summary>
        /// Returns the specified profiling counter
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="paramValue"></param>
        public unsafe void GetEventProfilingInfo(ProfilingInfo paramName, out ulong paramValue)
        {
            ulong v;
            ErrorCode errorCode;

            errorCode = OpenCL.GetEventProfilingInfo(EventID, paramName, (IntPtr)sizeof(ulong), &v, out IntPtr paramValueSizeRet);
            if (errorCode != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("GetEventProfilingInfo failed with error code " + errorCode, errorCode);
            }

            paramValue = v;
        }

        public static implicit operator IntPtr(Event _event)
        {
            return _event.EventID;
        }
    }
}
