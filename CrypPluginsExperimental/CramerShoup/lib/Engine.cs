using System;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Crypto;

namespace CrypTool.Plugins.CramerShoup.lib
{
    public class ECCramerShoupEngine
    {

        public Tuple<ECCramerShoupCipherText, byte[]> Encaps(ECCramerShoupPublicParameter publ, SecureRandom random, IDigest digest)
        {
            var q = publ.Q;
            var min = BigInteger.Two;
            var max = q.Subtract(BigInteger.Two);
            var k = BigIntegers.CreateRandomInRange(min, max, random);
            var ciphertext = new ECCramerShoupCipherText();

            var u1 = publ.G1.Multiply(k);
            var u2 = publ.G2.Multiply(k);

            #region k

            var hk = publ.H.Multiply(k);
            var normhk = hk.Normalize();

            digest.BlockUpdate(normhk.XCoord.ToBigInteger().ToByteArray(), 0, normhk.XCoord.ToBigInteger().ToByteArray().Length);
            digest.BlockUpdate(normhk.YCoord.ToBigInteger().ToByteArray(), 0, normhk.YCoord.ToBigInteger().ToByteArray().Length);
            var outOff = 0;
            byte[] output = new byte[digest.GetDigestSize()];

            digest.DoFinal(output, outOff);

            digest.Reset();
            var e = output;

            #endregion

            #region Verify

            publ.Digest.BlockUpdate(u1.XCoord.ToBigInteger().ToByteArray(), 0, u1.XCoord.ToBigInteger().ToByteArray().Length);
            publ.Digest.BlockUpdate(u1.YCoord.ToBigInteger().ToByteArray(), 0, u1.YCoord.ToBigInteger().ToByteArray().Length);
            publ.Digest.BlockUpdate(u2.XCoord.ToBigInteger().ToByteArray(), 0, u2.XCoord.ToBigInteger().ToByteArray().Length);
            publ.Digest.BlockUpdate(u2.YCoord.ToBigInteger().ToByteArray(), 0, u2.YCoord.ToBigInteger().ToByteArray().Length);
            outOff = 0;
            output =new byte[64];

            publ.Digest.DoFinal(output, outOff);

            var a = new BigInteger(output).Mod(q);
            var v1 = publ.C.Multiply(k);
            var ka = a.Multiply(k);
            var v = publ.D.Multiply(ka).Add(v1);

            #endregion

            ciphertext.U1 = u1;
            ciphertext.U2 = u2;
            ciphertext.V = v;
            return new Tuple<ECCramerShoupCipherText,byte[]>(ciphertext,e);
        }

        public byte[] Decaps(ECCramerShoupPrivateParameter priv, ECCramerShoupCipherText ciphertext, IDigest digest)
        {
            var q = priv.Q;
            var u1 = ciphertext.U1;
            var u2 = ciphertext.U2;

            #region k

            var hk = u1.Multiply(priv.Z);
            var normhk = hk.Normalize();

            digest.BlockUpdate(normhk.XCoord.ToBigInteger().ToByteArray(), 0, normhk.XCoord.ToBigInteger().ToByteArray().Length);
            digest.BlockUpdate(normhk.YCoord.ToBigInteger().ToByteArray(), 0, normhk.YCoord.ToBigInteger().ToByteArray().Length);
            var outOff = 0;
            byte[] output = new byte[digest.GetDigestSize()];

            digest.DoFinal(output, outOff);

            digest.Reset();
            var e = output;

            #endregion

            #region Verify

            priv.Digest.BlockUpdate(u1.XCoord.ToBigInteger().ToByteArray(), 0, u1.XCoord.ToBigInteger().ToByteArray().Length);
            priv.Digest.BlockUpdate(u1.YCoord.ToBigInteger().ToByteArray(), 0, u1.YCoord.ToBigInteger().ToByteArray().Length);
            priv.Digest.BlockUpdate(u2.XCoord.ToBigInteger().ToByteArray(), 0, u2.XCoord.ToBigInteger().ToByteArray().Length);
            priv.Digest.BlockUpdate(u2.YCoord.ToBigInteger().ToByteArray(), 0, u2.YCoord.ToBigInteger().ToByteArray().Length);
            outOff = 0;
            output = new byte[64];

            priv.Digest.DoFinal(output, outOff);

            var a = new BigInteger(output).Mod(q);

            var u1x = u1.Multiply(priv.X1);
            var u2x = u2.Multiply(priv.X2);
            var u1y = u1.Multiply(priv.Y1);
            var u2y = u2.Multiply(priv.Y2);
            var test1 = u1x.Add(u2x);
            var test2 = u1y.Add(u2y);
            var test = test2.Multiply(a).Add(test1);

            priv.Digest.Reset();

            #endregion

            if (ciphertext.V.Equals(test))
            {
                return e;
            }
            else
            {
                throw new Exception();
            }

        }
    }
}
