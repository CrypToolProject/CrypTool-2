using System;
using System.Text;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Math;

namespace PKCS1.Library
{
    public abstract class Signature
    {
        protected Signature()
        {
            Datablock.getInstance().RaiseParamChangedEvent += handleParamChanged;
        }

        protected byte[] m_Signature = null;
        protected int m_KeyLength = 0;

        protected bool m_bSigGenerated = false;
        public bool isSigGenerated()
        {
            return this.m_bSigGenerated;
        }        

        #region Getter

        public byte[] GetSignature()
        {
            return this.m_Signature;
        }

        public string GetSignatureToHexString()
        {
            return Encoding.ASCII.GetString(Hex.Encode(GetSignature() ) );
        }

        public byte[] GetSignatureDec()
        {
            return this.decryptedSig();
        }

        public string GetSignatureDecToHexString()
        {
            return Encoding.ASCII.GetString(Hex.Encode(this.decryptedSig()));
        }

        #endregion

        #region Eventhandling

        public event SigGenerated RaiseSigGenEvent;

        // trigger
        protected void OnRaiseSigGenEvent(SignatureType type)
        {
            if (null != RaiseSigGenEvent)
            {
                RaiseSigGenEvent(type);
            }
        }

        // listen
        private void handleParamChanged(ParameterChangeType type)
        {
            if (ParameterChangeType.Message == type)
            {
                this.m_bSigGenerated = false;
            }
            if (ParameterChangeType.HashfunctionType == type)
            {
                this.m_bSigGenerated = false;
            }
        }

        #endregion

        private byte[] decryptedSig()        
        {
            BigInteger SigInBigInt = new BigInteger(1,this.GetSignature());
            BigInteger returnBigInt = SigInBigInt.ModPow(RsaKey.Instance.getPubKeyToBigInt(), RsaKey.Instance.getModulusToBigInt());
            byte[] returnByteArray = new byte[ this.m_KeyLength/8 ]; // KeyLength is in bit
            Array.Copy(returnBigInt.ToByteArray(), 0, returnByteArray, returnByteArray.Length - returnBigInt.ToByteArray().Length, returnBigInt.ToByteArray().Length);
            return returnByteArray;
        }

        public abstract bool GenerateSignature();

    }
}
