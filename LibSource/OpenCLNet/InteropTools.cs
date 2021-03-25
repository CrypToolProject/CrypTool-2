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
using System.Text;
using System.IO;

namespace OpenCLNet
{
    public class InteropTools
    {
        public unsafe interface IPropertyContainer
        {
            IntPtr GetPropertySize(uint key);
            void ReadProperty(uint key, IntPtr keyLength, void* pBuffer);
        }

        public static unsafe void IntPtrToIntPtr(int num, IntPtr[] a, IntPtr* b)
        {
            if (a == null)
                return;

            if (num > a.Length)
                throw new ArgumentOutOfRangeException();
            for (int i = 0; i < num; i++)
                b[i] = a[i];
        }

        public static unsafe void A3ToIntPtr3(int[] a, IntPtr* b)
        {
            if (a == null || b == null)
                return;

            b[0] = (IntPtr)a[0];
            b[1] = (IntPtr)a[1];
            b[2] = (IntPtr)a[2];
        }

        public static unsafe void A3ToIntPtr3(long[] a, IntPtr* b)
        {
            if (a == null || b == null)
                return;

            b[0] = (IntPtr)a[0];
            b[1] = (IntPtr)a[1];
            b[2] = (IntPtr)a[2];
        }

        public static unsafe void AToIntPtr(int count, int[] a, IntPtr* b)
        {
            if (a == null || b==null )
                return;

            for (int i = 0; i < count; i++)
                b[i] = (IntPtr)a[i];
        }

        public static unsafe void AToIntPtr(int count, long[] a, IntPtr* b)
        {
            if (a == null || b == null)
                return;

            for (int i = 0; i < count; i++)
                b[i] = (IntPtr)a[i];
        }

        public static byte[] CreateNullTerminatedString(string s)
        {
            int len;
            byte[] data;

            len = Encoding.UTF8.GetByteCount(s);
            data = new byte[len + 1];
            Encoding.UTF8.GetBytes(s, 0, s.Length, data, 0);
            data[len] = 0;
            return data;
        }

        public static IntPtr[] ConvertDevicesToDeviceIDs(Device[] devices)
        {
            IntPtr[] deviceIDs;

            if (devices == null)
                return null;

            deviceIDs = new IntPtr[devices.Length];
            for (int i = 0; i < devices.Length; i++)
                deviceIDs[i] = devices[i];
            return deviceIDs;
        }

        public static Device[] ConvertDeviceIDsToDevices(Platform platform, IntPtr[] deviceIDs)
        {
            Device[] devices;

            if (deviceIDs == null)
                return null;

            devices = new Device[deviceIDs.Length];
            for (int i = 0; i < deviceIDs.Length; i++)
                devices[i] = platform.GetDevice(deviceIDs[i]);
            return devices;
        }

        public static IntPtr[] ConvertMemToMemIDs(Mem[] mems)
        {
            IntPtr[] memIDs;

            if (mems == null)
                return null;

            memIDs = new IntPtr[mems.Length];
            for (int i = 0; i < mems.Length; i++)
                memIDs[i] = mems[i].MemID;
            return memIDs;
        }

        public static IntPtr[] ConvertEventsToEventIDs(Event[] events)
        {
            IntPtr[] eventIDs;

            if (events == null)
                return null;

            eventIDs = new IntPtr[events.Length];
            for (int i = 0; i < events.Length; i++)
                eventIDs[i] = events[i];
            return eventIDs;
        }

        public unsafe static void ConvertEventsToEventIDs(int num, Event[] events, IntPtr* pHandles)
        {
            if (events == null)
                return;
            if (num > events.Length)
                throw new ArgumentOutOfRangeException();

            for (int i = 0; i < num; i++)
                pHandles[i] = events[i].EventID;
        }

        #region Helper functions to read properties

        unsafe public static bool ReadBool(IPropertyContainer propertyContainer, uint key)
        {
            return ReadUInt(propertyContainer, key) == (uint)Bool.TRUE ? true : false;
        }

        unsafe public static byte[] ReadBytes(IPropertyContainer propertyContainer, uint key)
        {
            IntPtr size;

            size = propertyContainer.GetPropertySize(key);
            byte[] data = new byte[size.ToInt64()];
            fixed (byte* pData = data)
            {
                propertyContainer.ReadProperty(key, size, pData);
            }
            return data;
        }

        unsafe public static string ReadString(IPropertyContainer propertyContainer, uint key)
        {
            IntPtr size;
            string s;

            size = propertyContainer.GetPropertySize((uint)key);
            byte[] stringData = new byte[size.ToInt64()];
            fixed (byte* pStringData = stringData)
            {
                propertyContainer.ReadProperty((uint)key, size, pStringData);
            }

            s = Encoding.UTF8.GetString(stringData);
            int nullIndex = s.IndexOf('\0');
            if (nullIndex >= 0)
                return s.Substring(0, nullIndex);
            else
                return s;
        }

        unsafe public static int ReadInt(IPropertyContainer propertyContainer, uint key)
        {
            int output;

            propertyContainer.ReadProperty(key, new IntPtr(sizeof(int)), &output);
            return output;
        }

        unsafe public static uint ReadUInt(IPropertyContainer propertyContainer, uint key)
        {
            uint output;

            propertyContainer.ReadProperty(key, new IntPtr(sizeof(uint)), &output);
            return output;
        }

        unsafe public static long ReadLong(IPropertyContainer propertyContainer, uint key)
        {
            long output;

            propertyContainer.ReadProperty(key, new IntPtr(sizeof(long)), &output);
            return output;
        }

        unsafe public static ulong ReadULong(IPropertyContainer propertyContainer, uint key)
        {
            ulong output;

            propertyContainer.ReadProperty(key, new IntPtr(sizeof(ulong)), &output);
            return output;
        }

        unsafe public static IntPtr ReadIntPtr(IPropertyContainer propertyContainer, uint key)
        {
            IntPtr output;

            propertyContainer.ReadProperty(key, new IntPtr(sizeof(IntPtr)), &output);
            return output;
        }

        unsafe public static IntPtr[] ReadIntPtrArray(IPropertyContainer propertyContainer, uint key)
        {
            IntPtr size = propertyContainer.GetPropertySize(key);
            long numElements = (long)size / sizeof(IntPtr);
            IntPtr[] ptrs = new IntPtr[numElements];
            byte[] data = InteropTools.ReadBytes(propertyContainer, key);

            fixed (byte* pData = data)
            {
                void** pBS = (void**)pData;
                for (int i = 0; i < numElements; i++)
                    ptrs[i] = new IntPtr(pBS[i]);
            }
            return ptrs;
        }

        unsafe public static void ReadPreAllocatedBytePtrArray(IPropertyContainer propertyContainer, uint key, byte[][] buffers)
        {
            GCHandle[] pinnedArrays = new GCHandle[buffers.Length];
            
            // Pin arrays
            for( int i=0; i<buffers.Length; i++ )
                pinnedArrays[i] = GCHandle.Alloc( buffers[i], GCHandleType.Pinned );

            byte** pointerArray = stackalloc byte*[buffers.Length];
            for( int i=0; i<buffers.Length; i++ )
                pointerArray[i] = (byte*)(pinnedArrays[i].AddrOfPinnedObject().ToPointer());

            propertyContainer.ReadProperty(key, new IntPtr(sizeof(IntPtr) * buffers.Length), pointerArray);

            for (int i = 0; i < buffers.Length; i++)
                pinnedArrays[i].Free();
        }
        #endregion

        public static void HexDump(string filename, byte[] array, int width)
        {
            StreamWriter sw = new StreamWriter(filename);

            HexDump(sw, array, width);
            sw.Close();
        }

        public static void HexDump(TextWriter tw, byte[] array, int width)
        {
            int hexWidth = width*3;
            int charWidth = width;
            int bytePtr = 0;
            int linePos;

            while (bytePtr < array.Length)
            {
                int dataLen = Math.Min(width, array.Length - bytePtr);

                tw.Write( string.Format("{0:x8} ", bytePtr) );
                // Output hex
                for (linePos = 0; linePos < dataLen; linePos++)
                {
                    byte b = array[bytePtr + linePos];

                    tw.Write(string.Format("{0:x2} ", (int)b));
                }
                for (; linePos < width; linePos++)
                    tw.Write("   ");

                // Output characters
                for (linePos = 0; linePos < dataLen; linePos++)
                {
                    byte b = array[bytePtr + linePos];
                    char c = (char)b;
                    if (Char.IsControl(c))
                        c = '.';
                    tw.Write(c);
                }
                for (; linePos < width; linePos++)
                    tw.Write(" ");

                tw.WriteLine();
                bytePtr += dataLen;
            }
        }
    }
}
