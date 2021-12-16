using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Text;

namespace PKCS1.Library
{
    internal class SigValidator
    {
        private HashFunctionIdent funcIdent = null;

        #region verify Signatures

        private bool verifySig(ISigner verifier, byte[] message, byte[] signature)
        {
            verifier.Init(false, RsaKey.Instance.getPubKey());
            verifier.BlockUpdate(message, 0, message.Length); // update bekommt klartextnachricht als param
            return verifier.VerifySignature(signature); // input ist verschlüsselte Nachricht
        }

        public bool verifyRsaSignature(byte[] message, byte[] signature)
        {
            IAsymmetricBlockCipher eng = new Pkcs1Encoding(new RsaEngine());
            eng.Init(false, RsaKey.Instance.getPubKey());

            try
            {
                byte[] data = eng.ProcessBlock(signature, 0, signature.Length);
                funcIdent = extractHashFunction(Encoding.ASCII.GetString(Hex.Encode(data)));

                if (null != funcIdent)
                {
                    ISigner verifier = SignerUtilities.GetSigner(funcIdent.diplayName + "withRSA");
                    return verifySig(verifier, message, signature);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return false;
        }

        public string getHashFunctionName()
        {
            if (funcIdent != null)
            {
                return funcIdent.diplayName;
            }
            return string.Empty;
        }

        private HashFunctionIdent extractHashFunction(string input)
        {
            string inputString = input.ToLower();

            if (inputString.StartsWith(HashFuncIdentHandler.SHA1.DERIdent.ToLower()))
            {
                return HashFuncIdentHandler.SHA1;
            }
            else if (inputString.StartsWith(HashFuncIdentHandler.SHA256.DERIdent.ToLower()))
            {
                return HashFuncIdentHandler.SHA256;
            }
            else if (inputString.StartsWith(HashFuncIdentHandler.SHA384.DERIdent.ToLower()))
            {
                return HashFuncIdentHandler.SHA384;
            }
            else if (inputString.StartsWith(HashFuncIdentHandler.SHA512.DERIdent.ToLower()))
            {
                return HashFuncIdentHandler.SHA512;
            }
            else if (inputString.StartsWith(HashFuncIdentHandler.MD2.DERIdent.ToLower()))
            {
                return HashFuncIdentHandler.MD2;
            }
            else if (inputString.StartsWith(HashFuncIdentHandler.MD5.DERIdent.ToLower()))
            {
                return HashFuncIdentHandler.MD5;
            }

            return null;
        }

        public bool verifyRsaSignatureWithFlaw(byte[] message, byte[] signature)
        {
            BigInteger signatureBigInt = new BigInteger(1, signature);
            RsaKeyParameters pubkeyParam = (RsaKeyParameters)RsaKey.Instance.getPubKey();

            byte[] sigDecrypted = (signatureBigInt.ModPow(pubkeyParam.Exponent, pubkeyParam.Modulus)).ToByteArray();
            byte[] block = DerEncode(sigDecrypted); // hiernach steht DERIdent und hash am anfang des arrays, danach garbage

            funcIdent = extractHashFunction(Encoding.ASCII.GetString(Hex.Encode(block)));

            // SHA1, Digest length: 160 Bit
            if (funcIdent == HashFuncIdentHandler.SHA1)
            {
                return verifySigWithoutPad(block, message, HashFuncIdentHandler.SHA1, 20);
            }

            // SHA-256, Digest length: 256 Bit
            if (funcIdent == HashFuncIdentHandler.SHA256)
            {
                return verifySigWithoutPad(block, message, HashFuncIdentHandler.SHA256, 32);
            }

            // SHA-384, Digest length: 384 Bit
            if (funcIdent == HashFuncIdentHandler.SHA384)
            {
                return verifySigWithoutPad(block, message, HashFuncIdentHandler.SHA384, 48);
            }

            // SHA-512, Digest length: 512 Bit
            if (funcIdent == HashFuncIdentHandler.SHA512)
            {
                return verifySigWithoutPad(block, message, HashFuncIdentHandler.SHA512, 64);
            }

            // MD2, Digest length: 128 Bit
            if (funcIdent == HashFuncIdentHandler.MD2)
            {
                return verifySigWithoutPad(block, message, HashFuncIdentHandler.MD2, 16);
            }

            // MD5, Digest length: 128 Bit
            if (funcIdent == HashFuncIdentHandler.MD5)
            {
                return verifySigWithoutPad(block, message, HashFuncIdentHandler.MD5, 16);
            }

            return false;
        }

        private bool verifySigWithoutPad(byte[] sigWithoutPad, byte[] message, HashFunctionIdent hashFuncIdent, int digestLength)
        {
            //TODO Längen überprüfen!
            string blockString = Encoding.ASCII.GetString(Hex.Encode(sigWithoutPad)).ToLower();

            byte[] hashDigestFromSig = new byte[digestLength];
            int endOfIdent = hashFuncIdent.DERIdent.Length / 2;
            Array.Copy(sigWithoutPad, endOfIdent, hashDigestFromSig, 0, digestLength);

            IDigest hashFunctionDigest = DigestUtilities.GetDigest(hashFuncIdent.diplayName);
            byte[] hashDigestMessage = Hashfunction.generateHashDigest(ref message, ref hashFuncIdent);

            return compareByteArrays(hashDigestFromSig, hashDigestMessage);
        }

        private bool compareByteArrays(byte[] input1, byte[] input2)
        {
            bool bEqual = false;

            if (input1.Length == input2.Length)
            {
                int i = 0;
                while ((i < input1.Length) && (input1[i] == input2[i]))
                {
                    i += 1;
                }
                if (i == input1.Length)
                {
                    bEqual = true;
                }
            }
            return bEqual;
        }

        private byte[] DerEncode(byte[] block)
        {
            // hier 0001 checken, dann FF Bytes skippen, dann HW zurück geben, Länge auslesen?            
            byte type = block[0];

            if (type != 1 && type != 2)
            {
                // TODO Exception schmeissen
                //throw new InvalidCipherTextException("unknown block type");
            }

            int start;
            for (start = 1; start != block.Length; start++)
            {
                byte pad = block[start];

                if (pad == 0)
                {
                    break;
                }

                if (type == 1 && pad != 0xff)
                {
                    throw new InvalidCipherTextException("block padding incorrect");
                }
            }
            start++;           // data should start at the next byte

            /*
            if (start > block.Length || start < HeaderLength)
            {
                throw new InvalidCipherTextException("no data in block");
            }*/

            byte[] result = new byte[block.Length - start];
            Array.Copy(block, start, result, 0, result.Length);

            return result; // anschliessend muss punkt 2.3 kommen
        }

        #endregion // verify Signatures
    }
}
