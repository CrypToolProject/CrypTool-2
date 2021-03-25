namespace SmartCard
{
    partial class pcscWrapper
    {

        // Card Scopes
        static int SCARD_SCOPE_USER = 0;
        static int SCARD_SCOPE_TERMINAL = 1;
        static int SCARD_SCOPE_SYSTEM = 2;

        // SCard Return Codes
        public static int SCARD_S_SUCCESS = 0x00;
        public static int INVALID_HANDLE = -1;

        // Share Modes
        static uint SCARD_SHARE_EXCLUSIVE = 0x00000001;
        static uint SCARD_SHARE_SHARED = 0x00000002;
        static uint SCARD_SHARE_DIRECT = 0x00000003;

        // SCard Protocols
        static uint SCARD_PROTOCOL_T0 = 0x00000001;
        static uint SCARD_PROTOCOL_T1 = 0x00000002;
        static uint SCARD_PROTOCOL_Tx = (SCARD_PROTOCOL_T0 | SCARD_PROTOCOL_T0);
        static uint SCARD_PROTOCOL_RAW = 0x00000004;

        // Disconnection card states
        internal static uint SCARD_LEAVE_CARD = 0;
        internal static uint SCARD_RESET_CARD = 1;
        internal static uint SCARD_UNPOWER_CARD = 2;
        internal static uint SCARD_EJECT_CARD = 3;

        // Protocol header structure

    }
}
