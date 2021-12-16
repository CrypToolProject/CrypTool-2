/*
This file is part of SharpPcap.

SharpPcap is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

SharpPcap is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with SharpPcap.  If not, see <http://www.gnu.org/licenses/>.
*/
/* 
 * Copyright 2010-2011 Chris Morgan <chmorgan@gmail.com>
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace SharpPcap.AirPcap
{
    /// <summary>
    /// AirPcap device
    /// </summary>
    public class AirPcapDevice : SharpPcap.WinPcap.WinPcapDevice
    {
        /// <summary>
        /// See ThrowIfNotOpen(string ExceptionString)
        /// </summary>
        protected void ThrowIfNotOpen()
        {
            ThrowIfNotOpen("");
        }

        /// <summary>
        /// Handle to the device
        /// </summary>
        internal IntPtr AirPcapDeviceHandle { get; set; }

        internal AirPcapDevice(WinPcap.WinPcapDevice dev) : base(dev.Interface)
        {
        }

        /// <summary>
        /// Retrieve the last error string for a given pcap_t* device
        /// </summary>
        /// <param name="AirPcapDeviceHandle">
        /// A <see cref="IntPtr"/>
        /// </param>
        /// <returns>
        /// A <see cref="System.String"/>
        /// </returns>
        internal static new string GetLastError(IntPtr AirPcapDeviceHandle)
        {
            IntPtr err_ptr = AirPcapSafeNativeMethods.AirpcapGetLastError(AirPcapDeviceHandle);
            return Marshal.PtrToStringAnsi(err_ptr);
        }

        /// <summary>
        /// The last pcap error associated with this pcap device
        /// </summary>
        public override string LastError => GetLastError(AirPcapDeviceHandle);

        /// <summary>
        /// Open a device
        /// </summary>
        public override void Open()
        {
            // open the base adapter, the WinPcapDevice
            base.Open();

            // reteieve the airpcap device given the winpcap handle
            AirPcapDeviceHandle = WinPcap.SafeNativeMethods.pcap_get_airpcap_handle(PcapHandle);
        }

        /// <summary>
        /// Open the device. To start capturing call the 'StartCapture' function
        /// </summary>
        /// <param name="mode">
        /// A <see cref="DeviceMode"/>
        /// </param>
        public override void Open(DeviceMode mode)
        {
            base.Open(mode);

            // reteieve the airpcap device given the winpcap handle
            AirPcapDeviceHandle = WinPcap.SafeNativeMethods.pcap_get_airpcap_handle(PcapHandle);
        }

        /// <summary>
        /// Open the device. To start capturing call the 'StartCapture' function
        /// </summary>
        /// <param name="mode">
        /// A <see cref="DeviceMode"/>
        /// </param>
        /// <param name="read_timeout">
        /// A <see cref="System.Int32"/>
        /// </param>
        public override void Open(DeviceMode mode, int read_timeout)
        {
            base.Open(mode, read_timeout);

            // reteieve the airpcap device given the winpcap handle
            AirPcapDeviceHandle = WinPcap.SafeNativeMethods.pcap_get_airpcap_handle(PcapHandle);
        }

        /// <summary>
        /// Opens an Airpcap device with optional WinPcap.OpenFlags
        /// </summary>
        /// <param name="flags">
        /// A <see cref="WinPcap.OpenFlags"/>
        /// </param>
        /// <param name="read_timeout">
        /// A <see cref="System.Int32"/>
        /// </param>
        public override void Open(WinPcap.OpenFlags flags, int read_timeout)
        {
            base.Open(flags, read_timeout);

            // reteieve the airpcap device given the winpcap handle
            AirPcapDeviceHandle = WinPcap.SafeNativeMethods.pcap_get_airpcap_handle(PcapHandle);
        }

        /// <summary>
        /// Close a device
        /// </summary>
        public override void Close()
        {
            if (!Opened)
            {
                return;
            }

            base.Close();
            AirPcapDeviceHandle = IntPtr.Zero;
        }

        /// <summary>
        /// Device capabilities, whether the device can transmit, its id, model name etc
        /// </summary>
        public AirPcapDeviceCapabilities Capabilities
        {
            get
            {
                ThrowIfNotOpen();

                if (!AirPcapSafeNativeMethods.AirpcapGetDeviceCapabilities(AirPcapDeviceHandle, out IntPtr capablitiesPointer))
                {
                    throw new InvalidOperationException("error retrieving device capabilities");
                }

                return new AirPcapDeviceCapabilities(capablitiesPointer);
            }
        }

        /// <summary>
        /// Adapter channel
        /// </summary>
        public uint Channel
        {
            get
            {
                ThrowIfNotOpen();
                if (!AirPcapSafeNativeMethods.AirpcapGetDeviceChannel(AirPcapDeviceHandle, out uint channel))
                {
                    throw new System.InvalidOperationException("Failed to retrieve channel");
                }
                return channel;
            }

            set
            {
                ThrowIfNotOpen();
                if (!AirPcapSafeNativeMethods.AirpcapSetDeviceChannel(AirPcapDeviceHandle, value))
                {
                    throw new System.InvalidOperationException("Failed to set channel");
                }
            }
        }

        /// <summary>
        /// Channel information
        /// </summary>
        public AirPcapChannelInfo ChannelInfo
        {
            get
            {
                ThrowIfNotOpen();

                if (!AirPcapSafeNativeMethods.AirpcapGetDeviceChannelEx(AirPcapDeviceHandle, out AirPcapUnmanagedStructures.AirpcapChannelInfo channelInfo))
                {
                    throw new System.InvalidOperationException("Failed to get channel ex");
                }

                return new AirPcapChannelInfo(channelInfo);
            }

            set
            {
                ThrowIfNotOpen();

                AirPcapUnmanagedStructures.AirpcapChannelInfo channelInfo = value.UnmanagedInfo;
                if (!AirPcapSafeNativeMethods.AirpcapSetDeviceChannelEx(AirPcapDeviceHandle, channelInfo))
                {
                    throw new System.InvalidOperationException("Failed to set channel ex");
                }
            }
        }

        /// <summary>
        /// Size in bytes of a key collection with a given count of keys
        /// </summary>
        /// <param name="keyCount"></param>
        /// <returns></returns>
        private static int KeyCollectionSize(int keyCount)
        {
            int memorySize = Marshal.SizeOf(typeof(AirPcapUnmanagedStructures.AirpcapKeysCollection)) +
                                   (Marshal.SizeOf(typeof(AirPcapUnmanagedStructures.AirpcapKey)) * keyCount);
            return memorySize;
        }

        /// <summary>
        /// Convert a AirpcapKeysCollection unmanaged buffer to a list of managed keys
        /// </summary>
        /// <param name="pKeysCollection"></param>
        /// <returns></returns>
        private static List<AirPcapKey> IntPtrToKeys(IntPtr pKeysCollection)
        {
            List<AirPcapKey> retval = new List<AirPcapKey>();

            // marshal the memory into a keys collection
            AirPcapUnmanagedStructures.AirpcapKeysCollection keysCollection = (AirPcapUnmanagedStructures.AirpcapKeysCollection)Marshal.PtrToStructure(pKeysCollection,
                                                    typeof(AirPcapUnmanagedStructures.AirpcapKeysCollection));

            // go through the keys, offset from the start of the collection to the first key 
            IntPtr pKeys = new IntPtr(pKeysCollection.ToInt64() + Marshal.SizeOf(typeof(AirPcapUnmanagedStructures.AirpcapKeysCollection)));

            for (int x = 0; x < keysCollection.nKeys; x++)
            {
                // convert the key entry from unmanaged memory to managed memory
                AirPcapUnmanagedStructures.AirpcapKey airpcapKey = (AirPcapUnmanagedStructures.AirpcapKey)Marshal.PtrToStructure(pKeys, typeof(AirPcapUnmanagedStructures.AirpcapKey));

                // convert the now managed key into the key representation we want to see
                retval.Add(new AirPcapKey(airpcapKey));

                // advance the pointer to the next key in the collection
                pKeys = new IntPtr(pKeys.ToInt64() + Marshal.SizeOf(typeof(AirPcapUnmanagedStructures.AirpcapKey)));
            }

            return retval;
        }

        /// <summary>
        /// Convert an array of keys into unmanaged memory
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static IntPtr KeysToIntPtr(List<AirPcapKey> value)
        {
            // allocate memory for the entire collection
            IntPtr pKeyCollection = Marshal.AllocHGlobal(AirPcapDevice.KeyCollectionSize(value.Count));
            IntPtr pKeyCollectionPosition = pKeyCollection;

            // build the collection struct
            AirPcapUnmanagedStructures.AirpcapKeysCollection collection = new AirPcapUnmanagedStructures.AirpcapKeysCollection
            {
                nKeys = (uint)value.Count
            };

            // convert this collection to unmanaged memory
            Marshal.StructureToPtr(collection, pKeyCollectionPosition, false);

            // advance the pointer
            pKeyCollectionPosition = new IntPtr(pKeyCollectionPosition.ToInt64() +
                                        Marshal.SizeOf(typeof(AirPcapUnmanagedStructures.AirpcapKeysCollection)));

            // write the keys to memory
            for (int x = 0; x < value.Count; x++)
            {
                AirPcapUnmanagedStructures.AirpcapKey key = new AirPcapUnmanagedStructures.AirpcapKey
                {
                    KeyType = value[x].Type,
                    KeyLen = (uint)value[x].Data.Length,

                    // make sure we have the right size byte[], the fields in the structure passed to Marshal.StructureToPtr()
                    // have to match the specified sizes or an exception will be thrown
                    KeyData = new byte[AirPcapUnmanagedStructures.WepKeyMaxSize]
                };
                Array.Copy(value[x].Data, key.KeyData, value[x].Data.Length);

                // copy the managed memory into the unmanaged memory
                Marshal.StructureToPtr(key, pKeyCollectionPosition, false);

                // advance the pointer
                pKeyCollectionPosition = new IntPtr(pKeyCollectionPosition.ToInt64() + Marshal.SizeOf(typeof(AirPcapUnmanagedStructures.AirpcapKey)));
            }

            return pKeyCollection;
        }

        /// <summary>
        /// Decryption keys that are currently associated with the specified device
        /// </summary>
        public List<AirPcapKey> DeviceKeys
        {
            get
            {
                ThrowIfNotOpen();

                // Request the key collection size
                uint keysCollectionSize = 0;
                if (AirPcapSafeNativeMethods.AirpcapGetDeviceKeys(AirPcapDeviceHandle, IntPtr.Zero,
                                                              ref keysCollectionSize))
                {
                    // return value of true with an input size of zero indicates there are no
                    // device keys
                    return null;
                }

                // now that we have the desired collection size, allocate the appropriate memory
                //var memorySize = AirPcapDevice.KeyCollectionSize(keysCollectionSize);
                IntPtr pKeysCollection = Marshal.AllocHGlobal((int)keysCollectionSize);

                try
                {
                    if (!AirPcapSafeNativeMethods.AirpcapGetDeviceKeys(AirPcapDeviceHandle, pKeysCollection,
                                                                      ref keysCollectionSize))
                    {
                        throw new System.InvalidOperationException("Unexpected false from AirpcapGetDeviceKeys()");
                    }

                    // convert the unmanaged memory to an array of keys
                    return IntPtrToKeys(pKeysCollection);
                }
                finally
                {
                    Marshal.FreeHGlobal(pKeysCollection);
                }
            }

            set
            {
                ThrowIfNotOpen();

                IntPtr pKeyCollection = KeysToIntPtr(value);
                try
                {
                    if (!AirPcapSafeNativeMethods.AirpcapSetDeviceKeys(AirPcapDeviceHandle, pKeyCollection))
                    {
                        throw new System.InvalidOperationException("Unable to set device keys");
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(pKeyCollection);
                }
            }
        }

        /// <summary>
        /// Global list of decryption keys that AirPcap is using with all the devices.
        /// </summary>
        public List<AirPcapKey> DriverKeys
        {
            get
            {
                ThrowIfNotOpen();

                // Request the key collection size
                uint keysCollectionSize = 0;
                if (AirPcapSafeNativeMethods.AirpcapGetDriverKeys(AirPcapDeviceHandle, IntPtr.Zero,
                                                                  ref keysCollectionSize))
                {
                    // return value of true with an input size of zero indicates there are no
                    // device keys
                    return null;
                }

                // now that we have the desired collection size, allocate the appropriate memory
                //var memorySize = AirPcapDevice.KeyCollectionSize(keysCollectionSize);
                IntPtr pKeysCollection = Marshal.AllocHGlobal((int)keysCollectionSize);

                try
                {
                    if (!AirPcapSafeNativeMethods.AirpcapGetDriverKeys(AirPcapDeviceHandle, pKeysCollection,
                                                                       ref keysCollectionSize))
                    {
                        throw new System.InvalidOperationException("Unexpected false from AirpcapGetDriverKeys()");
                    }

                    // convert the unmanaged memory to an array of keys
                    return IntPtrToKeys(pKeysCollection);
                }
                finally
                {
                    Marshal.FreeHGlobal(pKeysCollection);
                }
            }

            set
            {
                ThrowIfNotOpen();

                IntPtr pKeyCollection = KeysToIntPtr(value);
                try
                {
                    if (!AirPcapSafeNativeMethods.AirpcapSetDriverKeys(AirPcapDeviceHandle, pKeyCollection))
                    {
                        throw new System.InvalidOperationException("Unable to set driver keys");
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(pKeyCollection);
                }
            }
        }

        /// <summary>
        /// Tells if decryption of the incoming frames with the <b>device-specific</b> keys.
        /// </summary>
        public AirPcapDecryptionState DecryptionState
        {
            get
            {
                ThrowIfNotOpen();

                if (!AirPcapSafeNativeMethods.AirpcapGetDecryptionState(AirPcapDeviceHandle, out AirPcapDecryptionState state))
                {
                    throw new System.InvalidOperationException("Failed to get decryption state");
                }
                return state;
            }

            set
            {
                ThrowIfNotOpen();

                if (!AirPcapSafeNativeMethods.AirpcapSetDecryptionState(AirPcapDeviceHandle, value))
                {
                    throw new System.InvalidOperationException("Failed to set decryption state");
                }
            }
        }

        /// <summary>
        /// Tells if this open instance is configured to perform the decryption of the incoming frames with the <b>global</b> set of keys.
        /// </summary>
        public AirPcapDecryptionState DriverDecryptionState
        {
            get
            {
                ThrowIfNotOpen();

                if (!AirPcapSafeNativeMethods.AirpcapGetDriverDecryptionState(AirPcapDeviceHandle, out AirPcapDecryptionState state))
                {
                    throw new System.InvalidOperationException("Failed to get driver decryption state");
                }
                return state;
            }

            set
            {
                ThrowIfNotOpen();

                if (AirPcapSafeNativeMethods.AirpcapSetDriverDecryptionState(AirPcapDeviceHandle, value))
                {
                    throw new System.InvalidOperationException("Failed to set decryption state");
                }
            }
        }

        /// <summary>
        /// Configures the adapter on whether to include the MAC Frame Check Sequence in the captured packets.
        /// </summary>
        public bool FcsPresence
        {
            get
            {
                ThrowIfNotOpen();

                if (!AirPcapSafeNativeMethods.AirpcapGetFcsPresence(AirPcapDeviceHandle, out bool isFcsPresent))
                {
                    throw new System.InvalidOperationException("Failed to get fcs presence");
                }
                return isFcsPresent;
            }

            set
            {
                ThrowIfNotOpen();

                if (!AirPcapSafeNativeMethods.AirpcapSetFcsPresence(AirPcapDeviceHandle, value))
                {
                    throw new System.InvalidOperationException("Failed to set fcs presence");
                }
            }
        }

        /// <summary>
        /// The kinds of frames that the device will capture
        /// By default all frames are captured
        /// </summary>
        public AirPcapValidationType FcsValidation
        {
            get
            {
                ThrowIfNotOpen();

                if (!AirPcapSafeNativeMethods.AirpcapGetFcsValidation(AirPcapDeviceHandle, out AirPcapValidationType validationType))
                {
                    throw new System.InvalidOperationException("Failed to get fcs validation");
                }
                return validationType;
            }

            set
            {
                ThrowIfNotOpen();

                if (!AirPcapSafeNativeMethods.AirpcapSetFcsValidation(AirPcapDeviceHandle, value))
                {
                    throw new System.InvalidOperationException("failed to set fcs validation");
                }
            }
        }

        /// <summary>
        /// Kernel packet buffer size for this adapter in bytes
        /// </summary>
        public override uint KernelBufferSize
        {
            get
            {
                ThrowIfNotOpen();

                if (!AirPcapSafeNativeMethods.AirpcapGetKernelBufferSize(AirPcapDeviceHandle, out uint kernelBufferSize))
                {
                    throw new System.InvalidOperationException("failed to get kernel buffer size");
                }
                return kernelBufferSize;
            }

            set
            {
                ThrowIfNotOpen();

                if (!AirPcapSafeNativeMethods.AirpcapSetKernelBuffer(AirPcapDeviceHandle, value))
                {
                    throw new System.InvalidOperationException("failed to set kernel buffer size");
                }
            }
        }

        /// <summary>
        /// Number of leds on this adapter
        /// </summary>
        public int LedCount
        {
            get
            {
                ThrowIfNotOpen();

                AirPcapSafeNativeMethods.AirpcapGetLedsNumber(AirPcapDeviceHandle, out uint numberOfLeds);
                return (int)numberOfLeds;
            }
        }

        /// <summary>
        /// Led states
        /// </summary>
        public enum LedState
        {
            /// <summary>
            /// Led on
            /// </summary>
            On,

            /// <summary>
            /// Led off
            /// </summary>
            Off
        };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ledIndex">
        /// A <see cref="System.Int32"/>
        /// </param>
        /// <param name="newLedState">
        /// A <see cref="LedState"/>
        /// </param>
        public void Led(int ledIndex, LedState newLedState)
        {
            ThrowIfNotOpen();

            if (newLedState == LedState.On)
            {
                AirPcapSafeNativeMethods.AirpcapTurnLedOn(AirPcapDeviceHandle, (uint)ledIndex);
            }
            else if (newLedState == LedState.Off)
            {
                AirPcapSafeNativeMethods.AirpcapTurnLedOff(AirPcapDeviceHandle, (uint)ledIndex);
            }
        }

        /// <summary>
        /// Link type
        /// </summary>
        public AirPcapLinkTypes AirPcapLinkType
        {
            get
            {
                ThrowIfNotOpen("Requires an open device");


                AirPcapSafeNativeMethods.AirpcapGetLinkType(AirPcapDeviceHandle,
                                                            out AirPcapLinkTypes linkType);


                return linkType;
            }

            set
            {
                ThrowIfNotOpen("Requires an open device");

                if (!AirPcapSafeNativeMethods.AirpcapSetLinkType(AirPcapDeviceHandle,
                                                                value))
                {
                    throw new InvalidOperationException("Setting link type failed");
                }
            }
        }

        /// <summary>
        /// Link type in terms of PacketDotNet.LinkLayers
        /// </summary>
        public override PacketDotNet.LinkLayers LinkType
        {
            get
            {
                PacketDotNet.LinkLayers packetDotNetLinkLayer = PacketDotNet.LinkLayers.Null;

                switch (AirPcapLinkType)
                {
                    case AirPcapLinkTypes._802_11_PLUS_RADIO:
                        packetDotNetLinkLayer = PacketDotNet.LinkLayers.Ieee80211_Radio;
                        break;
                    case AirPcapLinkTypes._802_11:
                        packetDotNetLinkLayer = PacketDotNet.LinkLayers.Ieee802;
                        break;
                    case AirPcapLinkTypes._802_11_PLUS_PPI:
                        packetDotNetLinkLayer = PacketDotNet.LinkLayers.PerPacketInformation;
                        break;
                    default:
                        throw new System.InvalidOperationException("Unexpected linkType " + AirPcapLinkType);
                }

                return packetDotNetLinkLayer;
            }
        }

        /// <summary>
        /// TODO: Get this from packet.net or another place in System.Net.xxx?
        /// </summary>
        private const int MacAddressSizeInBytes = 6;

        /// <summary>
        /// Mac address
        /// </summary>
        public override PhysicalAddress MacAddress
        {
            get
            {
                ThrowIfNotOpen();

                byte[] address = new byte[MacAddressSizeInBytes];
                IntPtr addressUnmanaged = Marshal.AllocHGlobal(MacAddressSizeInBytes);
                try
                {
                    if (!AirPcapSafeNativeMethods.AirpcapGetMacAddress(AirPcapDeviceHandle, addressUnmanaged))
                    {
                        throw new System.InvalidOperationException("Unable to get mac address");
                    }

                    Marshal.Copy(addressUnmanaged, address, 0, address.Length);

                    return new PhysicalAddress(address);
                }
                finally
                {
                    Marshal.FreeHGlobal(addressUnmanaged);
                }
            }

            set
            {
                ThrowIfNotOpen();

                byte[] address = value.GetAddressBytes();
                IntPtr addressUnmanaged = Marshal.AllocHGlobal(address.Length);
                try
                {
                    if (!AirPcapSafeNativeMethods.AirpcapSetMacAddress(AirPcapDeviceHandle, addressUnmanaged))
                    {
                        throw new System.InvalidOperationException("Unable to set mac address");
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(addressUnmanaged);
                }
            }
        }

        /// <summary>
        /// Mac flags
        /// </summary>
        public AirPcapMacFlags MacFlags
        {
            get
            {
                ThrowIfNotOpen();

                if (!AirPcapSafeNativeMethods.AirpcapGetDeviceMacFlags(AirPcapDeviceHandle, out AirPcapMacFlags macFlags))
                {
                    throw new System.InvalidOperationException("Failed to get device mac flags");
                }
                return macFlags;
            }

            set
            {
                ThrowIfNotOpen();

                if (!AirPcapSafeNativeMethods.AirpcapSetDeviceMacFlags(AirPcapDeviceHandle, value))
                {
                    throw new System.InvalidOperationException("Failed to set device mac flags");
                }
            }
        }

        /// <summary>
        /// Adapter statistics
        /// </summary>
        public override ICaptureStatistics Statistics => new AirPcapStatistics(AirPcapDeviceHandle);

        /// <summary>
        /// List of supported channels
        /// </summary>
        public List<AirPcapChannelInfo> SupportedChannels
        {
            get
            {
                ThrowIfNotOpen();

                List<AirPcapChannelInfo> retval = new List<AirPcapChannelInfo>();

                if (!AirPcapSafeNativeMethods.AirpcapGetDeviceSupportedChannels(AirPcapDeviceHandle, out IntPtr pChannelInfo, out uint numChannelInfo))
                {
                    throw new System.InvalidOperationException("Failed to get device supported channels");
                }

                for (int x = 0; x < numChannelInfo; x++)
                {
                    AirPcapUnmanagedStructures.AirpcapChannelInfo unmanagedChannelInfo = (AirPcapUnmanagedStructures.AirpcapChannelInfo)Marshal.PtrToStructure(pChannelInfo,
                                                                                                            typeof(AirPcapUnmanagedStructures.AirpcapChannelInfo));

                    AirPcapChannelInfo channelInfo = new AirPcapChannelInfo(unmanagedChannelInfo);

                    retval.Add(channelInfo);

                    // advance the pointer to the next address
                    pChannelInfo = new IntPtr(pChannelInfo.ToInt64() + Marshal.SizeOf(typeof(AirPcapUnmanagedStructures.AirpcapChannelInfo)));
                }

                return retval;
            }
        }

        /// <summary>
        /// Transmit power
        /// </summary>
        public uint TxPower
        {
            get
            {
                ThrowIfNotOpen();

                if (!AirPcapSafeNativeMethods.AirpcapGetTxPower(AirPcapDeviceHandle, out uint power))
                {
                    throw new System.NotSupportedException("Unable to retrieve the tx power for this adapter");
                }
                return power;
            }

            set
            {
                ThrowIfNotOpen();

                if (!AirPcapSafeNativeMethods.AirpcapSetTxPower(AirPcapDeviceHandle, value))
                {
                    throw new System.NotSupportedException("Unable to set the tx power for this adapter");
                }
            }
        }

        /// <summary>
        /// Device timestamp
        /// </summary>
        public AirPcapDeviceTimestamp Timestamp
        {
            get
            {
                ThrowIfNotOpen();

                IntPtr pTimestamp = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(AirPcapUnmanagedStructures.AirpcapDeviceTimestamp)));
                try
                {
                    if (!AirPcapSafeNativeMethods.AirpcapGetDeviceTimestamp(AirPcapDeviceHandle, pTimestamp))
                    {
                        throw new System.NotSupportedException("Failed to get device timestamp");
                    }

                    AirPcapUnmanagedStructures.AirpcapDeviceTimestamp timestamp = (AirPcapUnmanagedStructures.AirpcapDeviceTimestamp)Marshal.PtrToStructure(pTimestamp,
                                                        typeof(AirPcapUnmanagedStructures.AirpcapDeviceTimestamp));

                    return new AirPcapDeviceTimestamp(timestamp);
                }
                finally
                {
                    Marshal.FreeHGlobal(pTimestamp);
                }
            }
        }

        /// <summary>
        /// AirPcap specific capture thread
        /// </summary>
        protected override void CaptureThread()
        {
            IntPtr WaitIntervalMilliseconds = (IntPtr)500;

            //
            // Get the read event
            //
            if (!AirPcapSafeNativeMethods.AirpcapGetReadEvent(AirPcapDeviceHandle, out IntPtr ReadEvent))
            {
                SendCaptureStoppedEvent(CaptureStoppedEventStatus.ErrorWhileCapturing);
                Close();
                return;
            }

            // allocate a packet bufer in unmanaged memory
            int packetBufferSize = 256000;
            IntPtr packetBuffer = Marshal.AllocHGlobal(packetBufferSize);



            while (!shouldCaptureThreadStop)
            {
                // capture the packets
                if (!AirPcapSafeNativeMethods.AirpcapRead(AirPcapDeviceHandle,
                    packetBuffer,
                   (uint)packetBufferSize,
                    out uint BytesReceived))
                {
                    Marshal.FreeHGlobal(packetBuffer);
                    Close();
                    SendCaptureStoppedEvent(CaptureStoppedEventStatus.ErrorWhileCapturing);
                    return;
                }

                IntPtr bufferEnd = new IntPtr(packetBuffer.ToInt64() + BytesReceived);

                MarshalPackets(packetBuffer, bufferEnd, out List<RawCapture> packets);

                foreach (RawCapture p in packets)
                {
                    SendPacketArrivalEvent(p);
                }

                // wait until some packets are available. This prevents polling and keeps the CPU low. 
                Win32SafeNativeMethods.WaitForSingleObject(ReadEvent, WaitIntervalMilliseconds);
            }

            Marshal.FreeHGlobal(packetBuffer);
        }

        /// <summary>
        /// Marshal a chunk of captured packets into a packet list
        /// </summary>
        /// <param name="packetsBuffer"></param>
        /// <param name="bufferEnd"></param>
        /// <param name="packets"></param>
        protected virtual void MarshalPackets(IntPtr packetsBuffer, IntPtr bufferEnd,
                                              out List<RawCapture> packets)
        {
            RawCapture p;

            PacketDotNet.LinkLayers linkType = LinkType;

            packets = new List<RawCapture>();

            IntPtr bufferPointer = packetsBuffer;

            while (bufferPointer.ToInt64() < bufferEnd.ToInt64())
            {
                // marshal the header
                AirPcapPacketHeader header = new AirPcapPacketHeader(bufferPointer);

                // advance the pointer to the packet data and marshal that data
                // into a managed buffer
                bufferPointer = new IntPtr(bufferPointer.ToInt64() + header.Hdrlen);
                byte[] pkt_data = new byte[header.Caplen];
                Marshal.Copy(bufferPointer, pkt_data, 0, (int)header.Caplen);

                p = new RawCapture(linkType,
                                   new PosixTimeval(header.TsSec,
                                                    header.TsUsec),
                                   pkt_data);

                packets.Add(p);

                // advance the pointer by the size of the data
                // and round up to the next word offset since each frame header is on a word boundry
                int alignment = 4;
                long pointer = bufferPointer.ToInt64() + header.Caplen;
                pointer = AirPcapDevice.RoundUp(pointer, alignment);
                bufferPointer = new IntPtr(pointer);
            }
        }

        private static long RoundUp(long num, int multiple)
        {
            if (multiple == 0)
            {
                return 0;
            }

            int add = multiple / Math.Abs(multiple);
            return ((num + multiple - add) / multiple) * multiple;
        }
        internal static int AIRPCAP_ERRBUF_SIZE = 512;
    }
}
