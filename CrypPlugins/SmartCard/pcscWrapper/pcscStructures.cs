using System;
using System.Runtime.InteropServices;

namespace SmartCard
{
    partial class pcscWrapper
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct READERSTATE
        {
            [MarshalAs(UnmanagedType.LPTStr)]
            internal string szReader;
            internal IntPtr pvUserData;
            internal UInt32 dwCurrentState;
            internal UInt32 dwEventState;
            internal UInt32 cbAtr;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 36)]
            internal byte[] rgbAtr;
        }

        public struct SCARD_IO_REQUEST {
            internal UInt32 dwProtocol;
            internal UInt32 cbPciLength;
        }

        public enum CardState
        {
            UNAWARE = 0x00000000,
            IGNORE = 0x00000001,
            CHANGED = 0x00000002,
            UNKNOWN = 0x00000004,
            UNAVAILABLE = 0x00000008,
            EMPTY = 0x00000010,
            PRESENT = 0x00000020,
            ATRMATCH = 0x00000040,
            EXCLUSIVE = 0x00000080,
            INUSE = 0x00000100,
            MUTE = 0x00000200,
            UNPOWERED = 0x00000400
        }
    }
}
