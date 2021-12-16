using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Utilities.Encoders;
using System;

namespace PKCS1.Library
{
    public class RsaSig : Signature
    {
        #region encrypted PKCS1 Signature

        private byte[] getCompleteHw()
        {
            byte[] bDerHashIdent = Hex.Decode(Datablock.getInstance().HashFunctionIdent.DERIdent);
            byte[] bMessage = Datablock.getInstance().Message;
            HashFunctionIdent hashIdent = Datablock.getInstance().HashFunctionIdent;
            byte[] hashDigest = Hashfunction.generateHashDigest(ref bMessage, ref hashIdent);
            byte[] returnArray = new byte[bDerHashIdent.Length + Hashfunction.getDigestSize()];
            Array.Copy(bDerHashIdent, 0, returnArray, 0, bDerHashIdent.Length);
            Array.Copy(hashDigest, 0, returnArray, returnArray.Length - hashDigest.Length, hashDigest.Length);

            return returnArray;
        }

        public override bool GenerateSignature()
        {
            if (RsaKey.Instance.isKeyGenerated())
            {
                // RSA Schlüssellänge setzen für Methode in Oberklasse
                m_KeyLength = RsaKey.Instance.RsaKeySize;

                IAsymmetricBlockCipher signerPkcs1Enc = new Pkcs1Encoding(new RsaEngine());
                signerPkcs1Enc.Init(true, RsaKey.Instance.getPrivKey());
                byte[] output = signerPkcs1Enc.ProcessBlock(getCompleteHw(), 0, getCompleteHw().Length);

                m_bSigGenerated = true;
                m_Signature = output;
                OnRaiseSigGenEvent(SignatureType.Pkcs1);
                return true;
            }
            return false;
        }

        #endregion //encrypted PKCS1 Signature
    }
}
