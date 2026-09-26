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
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Text;

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
            return m_bSigGenerated;
        }

        #region Getter

        public byte[] GetSignature()
        {
            return m_Signature;
        }

        public string GetSignatureToHexString()
        {
            return Encoding.ASCII.GetString(Hex.Encode(GetSignature()));
        }

        public byte[] GetSignatureDec()
        {
            return decryptedSig();
        }

        public string GetSignatureDecToHexString()
        {
            return Encoding.ASCII.GetString(Hex.Encode(decryptedSig()));
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
                m_bSigGenerated = false;
            }
            if (ParameterChangeType.HashfunctionType == type)
            {
                m_bSigGenerated = false;
            }
        }

        #endregion

        private byte[] decryptedSig()
        {
            BigInteger SigInBigInt = new BigInteger(1, GetSignature());
            BigInteger returnBigInt = SigInBigInt.ModPow(RsaKey.Instance.getPubKeyToBigInt(), RsaKey.Instance.getModulusToBigInt());
            byte[] returnByteArray = new byte[m_KeyLength / 8]; // KeyLength is in bit
            Array.Copy(returnBigInt.ToByteArray(), 0, returnByteArray, returnByteArray.Length - returnBigInt.ToByteArray().Length, returnBigInt.ToByteArray().Length);
            return returnByteArray;
        }

        public abstract bool GenerateSignature();

    }
}
