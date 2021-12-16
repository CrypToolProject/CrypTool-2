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
 * Copyright 2005 Tamir Gal <tamir@tamirgal.com>
 * Copyright 2008-2009 Chris Morgan <chmorgan@gmail.com>
 */

using System;
using System.Runtime.InteropServices;

namespace SharpPcap.LibPcap
{
    /// <summary>
    ///  A wrapper for libpcap's pcap_pkthdr structure
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)] // Force it to match a 32-bit native header exactly
    public struct PcapHeader
    {
        private static readonly bool isWindows = Environment.OSVersion.Platform != PlatformID.Unix;
        private static readonly bool is32BitTs = IntPtr.Size == 4 || isWindows;

        /// <summary>
        ///  A wrapper class for libpcap's pcap_pkthdr structure
        /// </summary>
        private unsafe PcapHeader(IntPtr pcap_pkthdr)
        {
            if (!isWindows)
            {
                PcapUnmanagedStructures.pcap_pkthdr_unix pkthdr = *(PcapUnmanagedStructures.pcap_pkthdr_unix*)pcap_pkthdr;
                CaptureLength = pkthdr.caplen;
                PacketLength = pkthdr.len;
                Seconds = (uint)pkthdr.ts.tv_sec;
                MicroSeconds = (uint)pkthdr.ts.tv_usec;
            }
            else
            {
                PcapUnmanagedStructures.pcap_pkthdr_windows pkthdr = *(PcapUnmanagedStructures.pcap_pkthdr_windows*)pcap_pkthdr;
                CaptureLength = pkthdr.caplen;
                PacketLength = pkthdr.len;
                Seconds = (uint)pkthdr.ts.tv_sec;
                MicroSeconds = (uint)pkthdr.ts.tv_usec;
            }
        }

        /// <summary>
        /// Get a PcapHeader structure from a pcap_pkthdr pointer.
        /// </summary>
        public static unsafe PcapHeader FromPointer(IntPtr pcap_pkthdr)
        {
            if (is32BitTs)
            {
                return *(PcapHeader*)pcap_pkthdr;
            }
            else
            {
                return new PcapHeader(pcap_pkthdr);
            }
        }

        /// <summary>
        /// Constructs a new PcapHeader
        /// </summary>
        /// <param name="seconds">The seconds value of the packet's timestamp</param>
        /// <param name="microseconds">The microseconds value of the packet's timestamp</param>
        /// <param name="packetLength">The actual length of the packet</param>
        /// <param name="captureLength">The length of the capture</param>
        public PcapHeader(uint seconds, uint microseconds, uint packetLength, uint captureLength)
        {
            Seconds = seconds;
            MicroSeconds = microseconds;
            PacketLength = packetLength;
            CaptureLength = captureLength;
        }

        /// <summary>
        /// The seconds value of the packet's timestamp
        /// </summary>
        public uint Seconds;

        /// <summary>
        /// The microseconds value of the packet's timestamp
        /// </summary>
        public uint MicroSeconds;

        /// <summary>
        /// The length of the packet on the line
        /// </summary>
        public uint PacketLength;

        /// <summary>
        /// The the bytes actually captured. If the capture length
        /// is small CaptureLength might be less than PacketLength
        /// </summary>
        public uint CaptureLength;

        /// <summary>
        /// DateTime(1970, 1, 1).Ticks, saves cpu cycles in the Date property
        /// </summary>
        private const long epochTicks = 621355968000000000L;

        /// <summary>
        /// Return the DateTime value of this pcap header
        /// </summary>
        public System.DateTime Date => new DateTime(epochTicks + (Seconds * 10000000L) + (MicroSeconds * 10L));

        /// <summary>
        /// Marshal this structure into the platform dependent version and return
        /// and IntPtr to that memory
        ///
        /// NOTE: IntPtr MUST BE FREED via Marshal.FreeHGlobal()
        /// </summary>
        /// <returns>
        /// A <see cref="IntPtr"/>
        /// </returns>
        public IntPtr MarshalToIntPtr()
        {
            IntPtr hdrPtr;

            if (!isWindows)
            {
                // setup the structure to marshal
                PcapUnmanagedStructures.pcap_pkthdr_unix pkthdr = new PcapUnmanagedStructures.pcap_pkthdr_unix
                {
                    caplen = CaptureLength,
                    len = PacketLength
                };
                pkthdr.ts.tv_sec = (IntPtr)Seconds;
                pkthdr.ts.tv_usec = (IntPtr)MicroSeconds;

                hdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(PcapUnmanagedStructures.pcap_pkthdr_unix)));
                Marshal.StructureToPtr(pkthdr, hdrPtr, true);
            }
            else
            {
                PcapUnmanagedStructures.pcap_pkthdr_windows pkthdr = new PcapUnmanagedStructures.pcap_pkthdr_windows
                {
                    caplen = CaptureLength,
                    len = PacketLength
                };
                pkthdr.ts.tv_sec = (int)Seconds;
                pkthdr.ts.tv_usec = (int)MicroSeconds;

                hdrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(PcapUnmanagedStructures.pcap_pkthdr_windows)));
                Marshal.StructureToPtr(pkthdr, hdrPtr, true);
            }

            return hdrPtr;
        }
    }
}
