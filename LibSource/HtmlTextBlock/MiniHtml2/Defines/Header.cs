using System;
using System.Runtime.InteropServices;

namespace QuickZip.MiniHtml2
{
    public class ProcessInfo
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public int ProcessID;
        public int ThreadID;
    }

    public class Header
    {
#if CF
        const string user32 = "coredll.dll";
        const string kernel32 = "coredll.dll";
#else
        private const string user32 = "user32.dll";
        private const string kernel32 = "kernel32.dll";
#endif

        [DllImport(kernel32)]
        public static extern int CreateProcess(string appName,
            string cmdLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes,
            int boolInheritHandles, int dwCreationFlags, IntPtr lpEnvironment,
            IntPtr lpszCurrentDir, byte[] si, ProcessInfo pi);

        [DllImport(kernel32)]
        public static extern int WaitForSingleObject(IntPtr handle, int wait);

        [DllImport(kernel32)]
        public static extern int GetLastError();

        [DllImport(kernel32)]
        public static extern int CloseHandle(IntPtr handle);


    }
}
