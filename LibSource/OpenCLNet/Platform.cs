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
using System.Text.RegularExpressions;

namespace OpenCLNet
{

    public unsafe class Platform : InteropTools.IPropertyContainer
    {
        public IntPtr PlatformID { get; protected set; }
        /// <summary>
        /// Equal to "FULL_PROFILE" if the implementation supports the OpenCL specification or
        /// "EMBEDDED_PROFILE" if the implementation supports the OpenCL embedded profile.
        /// </summary>
        public string Profile => InteropTools.ReadString(this, (uint)PlatformInfo.PROFILE);
        /// <summary>
        /// OpenCL version string. Returns the OpenCL version supported by the implementation. This version string
        /// has the following format: OpenCL&lt;space&gt;&lt;major_version.minor_version&gt;&lt;space&gt;&lt;platform specific information&gt;
        /// </summary>
        public string Version => InteropTools.ReadString(this, (uint)PlatformInfo.VERSION);
        /// <summary>
        /// Platform name string
        /// </summary>
        public string Name => InteropTools.ReadString(this, (uint)PlatformInfo.NAME);
        /// <summary>
        /// Platform Vendor string
        /// </summary>
        public string Vendor => InteropTools.ReadString(this, (uint)PlatformInfo.VENDOR);
        /// <summary>
        /// Space separated string of extension names.
        /// Note that this class has some support functions to help query extension capbilities.
        /// This property is only present for completeness.
        /// </summary>
        public string Extensions => InteropTools.ReadString(this, (uint)PlatformInfo.EXTENSIONS);
        /// <summary>
        /// Convenience method to get at the major_version field in the Version string
        /// </summary>
        public int OpenCLMajorVersion { get; protected set; }
        /// <summary>
        /// Convenience method to get at the minor_version field in the Version string
        /// </summary>
        public int OpenCLMinorVersion { get; protected set; }

        private readonly Regex VersionStringRegex = new Regex("OpenCL (?<Major>[0-9]+)\\.(?<Minor>[0-9]+)");
        protected Dictionary<IntPtr, Device> _Devices = new Dictionary<IntPtr, Device>();
        private readonly Device[] DeviceList;
        private readonly IntPtr[] DeviceIDs;

        protected HashSet<string> ExtensionHashSet = new HashSet<string>();

        public Platform(IntPtr platformID)
        {
            PlatformID = platformID;

            // Create a local representation of all devices
            DeviceIDs = QueryDeviceIntPtr(DeviceType.ALL);
            for (int i = 0; i < DeviceIDs.Length; i++)
            {
                _Devices[DeviceIDs[i]] = new Device(this, DeviceIDs[i]);
            }

            DeviceList = InteropTools.ConvertDeviceIDsToDevices(this, DeviceIDs);

            InitializeExtensionHashSet();

            Match m = VersionStringRegex.Match(Version);
            if (m.Success)
            {
                OpenCLMajorVersion = int.Parse(m.Groups["Major"].Value);
                OpenCLMinorVersion = int.Parse(m.Groups["Minor"].Value);
            }
            else
            {
                OpenCLMajorVersion = 1;
                OpenCLMinorVersion = 0;
            }
        }

        public Context CreateDefaultContext()
        {
            return CreateDefaultContext(null, IntPtr.Zero);
        }

        public Context CreateDefaultContext(ContextNotify notify, IntPtr userData)
        {
            IntPtr[] properties = new IntPtr[]
            {
                new IntPtr((long)ContextProperties.PLATFORM), PlatformID,
                IntPtr.Zero,
            };

            IntPtr contextID;

            contextID = OpenCL.CreateContext(properties,
                (uint)DeviceIDs.Length,
                DeviceIDs,
                notify,
                userData,
                out ErrorCode result);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("CreateContext failed with error code: " + result, result);
            }

            return new Context(this, contextID);
        }

        public Context CreateContext(IntPtr[] contextProperties, Device[] devices, ContextNotify notify, IntPtr userData)
        {
            IntPtr contextID;

            IntPtr[] deviceIDs = InteropTools.ConvertDevicesToDeviceIDs(devices);
            contextID = OpenCL.CreateContext(contextProperties,
                (uint)deviceIDs.Length,
                deviceIDs,
                notify,
                userData,
                out ErrorCode result);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("CreateContext failed with error code: " + result, result);
            }

            return new Context(this, contextID);
        }

        public Context CreateContextFromType(IntPtr[] contextProperties, DeviceType deviceType, ContextNotify notify, IntPtr userData)
        {
            IntPtr contextID;

            contextID = OpenCL.CreateContextFromType(contextProperties,
                deviceType,
                notify,
                userData,
                out ErrorCode result);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("CreateContextFromType failed with error code: " + result, result);
            }

            return new Context(this, contextID);
        }

        public Device GetDevice(IntPtr index)
        {
            return _Devices[index];
        }

        protected IntPtr[] QueryDeviceIntPtr(DeviceType deviceType)
        {
            ErrorCode result;
            IntPtr[] deviceIDs;

            result = OpenCL.GetDeviceIDs(PlatformID, deviceType, 0, null, out uint numberOfDevices);
            if (result == ErrorCode.DEVICE_NOT_FOUND)
            {
                return new IntPtr[0];
            }

            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("GetDeviceIDs failed: " + result.ToString(), result);
            }

            deviceIDs = new IntPtr[numberOfDevices];
            result = OpenCL.GetDeviceIDs(PlatformID, deviceType, numberOfDevices, deviceIDs, out numberOfDevices);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("GetDeviceIDs failed: " + result.ToString(), result);
            }

            return deviceIDs;
        }

        /// <summary>
        /// Find all devices of a specififc type
        /// </summary>
        /// <param name="deviceType"></param>
        /// <returns>Array containing the devices</returns>
        public Device[] QueryDevices(DeviceType deviceType)
        {
            IntPtr[] deviceIDs;

            deviceIDs = QueryDeviceIntPtr(deviceType);
            return InteropTools.ConvertDeviceIDsToDevices(this, deviceIDs);
        }

        protected void InitializeExtensionHashSet()
        {
            string[] ext = Extensions.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string s in ext)
            {
                ExtensionHashSet.Add(s);
            }
        }

        /// <summary>
        /// Test if this platform supports a specific extension
        /// </summary>
        /// <param name="extension"></param>
        /// <returns>Returns true if the extension is supported</returns>
        public bool HasExtension(string extension)
        {
            return ExtensionHashSet.Contains(extension);
        }

        /// <summary>
        /// Test if this platform supports a set of exentions
        /// </summary>
        /// <param name="extensions"></param>
        /// <returns>Returns true if all the extensions are supported</returns>
        public bool HasExtensions(string[] extensions)
        {
            foreach (string s in extensions)
            {
                if (!ExtensionHashSet.Contains(s))
                {
                    return false;
                }
            }

            return true;
        }

        public static implicit operator IntPtr(Platform p)
        {
            return p.PlatformID;
        }

        #region IPropertyContainer Members

        public IntPtr GetPropertySize(uint key)
        {
            ErrorCode result;

            result = OpenCL.GetPlatformInfo(PlatformID, key, IntPtr.Zero, null, out IntPtr propertySize);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("Unable to get platform info for platform " + PlatformID + ": " + result, result);
            }

            return propertySize;

        }

        public void ReadProperty(uint key, IntPtr keyLength, void* pBuffer)
        {
            ErrorCode result;

            result = OpenCL.GetPlatformInfo(PlatformID, key, keyLength, pBuffer, out IntPtr propertySize);
            if (result != ErrorCode.SUCCESS)
            {
                throw new OpenCLException("Unable to get platform info for platform " + PlatformID + ": " + result, result);
            }
        }

        #endregion
    }
}
