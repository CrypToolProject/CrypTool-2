using System;
using System.Text;

namespace SmartCard
{
    partial class pcscWrapper
    {

        public static int EstablishContext()
        {
            return EstablishContext(pcscWrapper.SCARD_SCOPE_USER);
        }

        public static int EstablishContext(int Scope)
        {
            int hContext;
            if (SCardEstablishContext(Scope, IntPtr.Zero, IntPtr.Zero, out hContext) == SCARD_S_SUCCESS)
                return hContext;
            else return INVALID_HANDLE;            
        }

        public static int ReleaseContext(int hContext)
        {
            return SCardReleaseContext(hContext);
        }
        
        public static String[] ListReaders(int hContext)
        {
            UInt32 pcchReaders = 0;
            byte[] mszReaders;
            String[] sReturn;

            if (SCardListReadersA(hContext, null, null, ref pcchReaders) != SCARD_S_SUCCESS)
                return null;

            mszReaders = new byte[pcchReaders];

            if (SCardListReadersA(hContext, null, mszReaders, ref pcchReaders) != SCARD_S_SUCCESS)
                return null;

            ASCIIEncoding Encoding = new ASCIIEncoding();
            sReturn = Encoding.GetString(mszReaders).Split(new char[] {'\0'},StringSplitOptions.RemoveEmptyEntries);

            return sReturn;

        }

        public static READERSTATE getReaderState(int hContext, String sReader)
        {
            READERSTATE[] rState = new READERSTATE[1];
            rState[0].szReader = sReader;
            rState[0].dwCurrentState = (UInt32) CardState.UNAWARE;
            int i = SCardGetStatusChange(hContext, 0, rState, 1);

            if (i != SCARD_S_SUCCESS)
            {
                rState[0].dwEventState = (UInt32) CardState.UNKNOWN;
            }
            return rState[0];
        }

        public static int Connect(int hContext, String sReader)
        {
            uint itemp = pcscWrapper.SCARD_PROTOCOL_T1; 
            return Connect(hContext, sReader, pcscWrapper.SCARD_SHARE_SHARED, ref itemp);
        }

        public static int Connect(int hContext, String sReader, uint dwShareMode, ref uint dwProtocol)
        {
            int CardHandle;
            uint Protocol = SCARD_PROTOCOL_T1;
            int tmp = SCardConnect(hContext, sReader, dwShareMode, dwProtocol, out CardHandle, out Protocol);
            if ( tmp == SCARD_S_SUCCESS)
            {
                dwProtocol = Protocol;
                return CardHandle;
            }
            else
            {
                return INVALID_HANDLE;
            }
        }

        public static byte[] Transmit(int hCard, byte[] Command)
        {
            SCARD_IO_REQUEST ioRecvPci = new SCARD_IO_REQUEST();
            byte[] bTemp = new byte[2048];
            uint LenReturnBytes = 2048;
            ioRecvPci.cbPciLength = 2048;
            IntPtr SCARD_PCI_T1 = pcscWrapper.GetPciT1();

            if (SCardTransmit(hCard, SCARD_PCI_T1, Command, Command.Length, IntPtr.Zero, bTemp, ref LenReturnBytes) == SCARD_S_SUCCESS)
            {
                byte[] ReturnBytes = new byte[LenReturnBytes];
                for (int i = 0; i < LenReturnBytes; i++)
                {
                    ReturnBytes[i] = bTemp[i];
                }
                return ReturnBytes;
            }
            else
            {
                return null;
            }           
        }

        public static int Disconnect(int hCard, uint dwDisposition)
        {
            return SCardDisconnect(hCard, dwDisposition);
        }

        //Get the address of Pci from "Winscard.dll".
        private static IntPtr GetPciT1()
        {
            IntPtr handle = LoadLibrary("Winscard.dll");
            IntPtr pci = GetProcAddress(handle, "g_rgSCardT1Pci");
            FreeLibrary(handle);
            return pci;
        }
    }
}
