using System;
using System.Runtime.InteropServices;

namespace SmartCard
{
    partial class pcscWrapper
    {

        [DllImport("Winscard.dll", CharSet = CharSet.Auto)]
        private static extern int SCardEstablishContext(
            int dwScope,
            IntPtr pvReserved1,
            IntPtr pvReserved2,
            out int phContext
            );

        [DllImport("Winscard.dll", CharSet = CharSet.Auto)]
        private static extern int SCardReleaseContext(
            int hContext
            );

        [DllImport("Winscard.dll", CharSet = CharSet.Auto)]
        private static extern int SCardListReadersA(
            int hContext,
            byte[] mszGroups,
            byte[] mszReaders,
            ref UInt32 pcchReaders
            );

        [DllImport("Winscard.dll", CharSet = CharSet.Auto)]
        private static extern int SCardGetStatusChange(
            int hContext,
            int dwTimeout,
            [In, Out] READERSTATE[] rgReaderStates,
            int cReaders
            );

        [DllImport("Winscard.dll", CharSet = CharSet.Auto)]
        private static extern int SCardConnect(
            int hContext,
            [MarshalAs(UnmanagedType.LPTStr)] 
            string szReader,
            UInt32 dwShareMode,
            UInt32 dwPreferredProtocols,
            out int phCard,
            out UInt32 pdwActiveProtocol
            );

        [DllImport("Winscard.dll", CharSet = CharSet.Auto)]
        private static extern int SCardDisconnect(
            int hCard,
            UInt32 dwDisposition
            );

        [DllImport("Winscard.dll", CharSet = CharSet.Auto)]
        static extern int SCardTransmit(
            int hCard, 
            IntPtr pioSendPci, 
            byte[] pbSendBuffer, 
            int cbSendLength,
            IntPtr pioRecvPci,
            byte[] pbRecvBuffer,
            ref UInt32 pcbRecvLength
            );

        [DllImport("kernel32.dll")]
        private extern static void FreeLibrary(IntPtr handle);

        [DllImport("kernel32.dll")]
        private extern static IntPtr GetProcAddress(IntPtr handle, string
        procName);

        [DllImport("kernel32")]
        static extern IntPtr LoadLibrary(string lpFileName);
    }
}
