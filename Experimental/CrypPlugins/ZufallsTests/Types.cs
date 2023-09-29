using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ZufallsTests
{
    class Types
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct Dtest
        {
            //[MarshalAs(UnmanagedType.U4)]
            public IntPtr name;
            //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
            public IntPtr sname;
            public IntPtr description;
            /* Standard test default */
            public uint psamples_std;
            /* Standard test default */
            public uint tsamples_std;
            /* Number of independent statistics generated per run */
            public uint nkps;

            public IntPtr test;
            //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            //public delegate int test();

            public IntPtr targs;

        }
    }
}
