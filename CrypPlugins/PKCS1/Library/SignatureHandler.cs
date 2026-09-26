/*
   Copyright (C) CrypTool 2 Team

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
namespace PKCS1.Library
{
    internal class SignatureHandler
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
            m_Signature = sig;
        }

        public Signature getSignature()
        {
            return m_Signature;
        }

        public void setBleichenBSig(Signature sig)
        {
            m_BleichenbSig = sig;
        }

        public Signature getBleichenbSig()
        {
            return m_BleichenbSig;
        }

        public void setKuehnSig(Signature sig)
        {
            m_KuehnSig = sig;
        }

        public Signature getKuehnSig()
        {
            return m_KuehnSig;
        }

        public bool isRsaSigGenerated()
        {
            return m_Signature.isSigGenerated();
        }

        public bool isBleichenbSigGenerated()
        {
            return m_BleichenbSig.isSigGenerated();
        }

        public bool isKuehnSigGenerated()
        {
            return m_KuehnSig.isSigGenerated();
        }
    }
}
