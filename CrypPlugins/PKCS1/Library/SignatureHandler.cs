namespace PKCS1.Library
{
    class SignatureHandler
    {
        private static SignatureHandler instance = null;
        private Signature m_Signature = new RsaSig();
        private Signature m_BleichenbSig = new BleichenbacherSig();
        private Signature m_KuehnSig = new KuehnSig();

        private SignatureHandler()
        {
        }

        public static SignatureHandler getInstance()
        {
            if (null == instance)
            {
                instance = new SignatureHandler();
            }
            return instance;
        }

        public void setSignature(Signature sig)
        {
            this.m_Signature = sig;
        }

        public Signature getSignature()
        {
            return this.m_Signature;
        }

        public void setBleichenBSig(Signature sig)
        {
            this.m_BleichenbSig = sig;
        }

        public Signature getBleichenbSig()
        {
            return this.m_BleichenbSig;
        }

        public void setKuehnSig(Signature sig)
        {
            this.m_KuehnSig = sig;
        }

        public Signature getKuehnSig()
        {
            return this.m_KuehnSig;
        }

        public bool isRsaSigGenerated()
        {
            return this.m_Signature.isSigGenerated();
        }

        public bool isBleichenbSigGenerated()
        {
            return this.m_BleichenbSig.isSigGenerated();
        }

        public bool isKuehnSigGenerated()
        {
            return this.m_KuehnSig.isSigGenerated();
        }
    }
}
