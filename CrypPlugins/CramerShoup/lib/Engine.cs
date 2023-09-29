using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using System;

namespace CrypTool.Plugins.CramerShoup.lib
{
    public class ECCramerShoupEngine
    {

        public Tuple<ECCramerShoupCipherText, byte[]> Encaps(ECCramerShoupPublicParameter publ, SecureRandom random, IDigest digest)
        {
            BigInteger q = publ.Q;
            BigInteger min = BigInteger.Two;
            BigInteger max = q.Subtract(BigInteger.Two);
            BigInteger k = BigIntegers.CreateRandomInRange(min, max, random);
            ECCramerShoupCipherText ciphertext = new ECCramerShoupCipherText();

            Org.BouncyCastle.Math.EC.ECPoint u1 = publ.G1.Multiply(k);
            Org.BouncyCastle.Math.EC.ECPoint u2 = publ.G2.Multiply(k);

            #region k

            Org.BouncyCastle.Math.EC.ECPoint hk = publ.H.Multiply(k);
            Org.BouncyCastle.Math.EC.ECPoint normhk = hk.Normalize();

            digest.BlockUpdate(normhk.XCoord.ToBigInteger().ToByteArray(), 0, normhk.XCoord.ToBigInteger().ToByteArray().Length);
            digest.BlockUpdate(normhk.YCoord.ToBigInteger().ToByteArray(), 0, normhk.YCoord.ToBigInteger().ToByteArray().Length);
            int outOff = 0;
            byte[] output = new byte[digest.GetDigestSize()];

            digest.DoFinal(output, outOff);

            digest.Reset();
            byte[] e = output;

            #endregion

            #region Verify

            publ.Digest.BlockUpdate(u1.XCoord.ToBigInteger().ToByteArray(), 0, u1.XCoord.ToBigInteger().ToByteArray().Length);
            publ.Digest.BlockUpdate(u1.YCoord.ToBigInteger().ToByteArray(), 0, u1.YCoord.ToBigInteger().ToByteArray().Length);
            publ.Digest.BlockUpdate(u2.XCoord.ToBigInteger().ToByteArray(), 0, u2.XCoord.ToBigInteger().ToByteArray().Length);
            publ.Digest.BlockUpdate(u2.YCoord.ToBigInteger().ToByteArray(), 0, u2.YCoord.ToBigInteger().ToByteArray().Length);
            outOff = 0;
            output = new byte[64];

            publ.Digest.DoFinal(output, outOff);

            BigInteger a = new BigInteger(output).Mod(q);
            Org.BouncyCastle.Math.EC.ECPoint v1 = publ.C.Multiply(k);
            BigInteger ka = a.Multiply(k);
            Org.BouncyCastle.Math.EC.ECPoint v = publ.D.Multiply(ka).Add(v1);

            #endregion

            ciphertext.U1 = u1;
            ciphertext.U2 = u2;
            ciphertext.V = v;
            return new Tuple<ECCramerShoupCipherText, byte[]>(ciphertext, e);
        }

        public byte[] Decaps(ECCramerShoupPrivateParameter priv, ECCramerShoupCipherText ciphertext, IDigest digest)
        {
            BigInteger q = priv.Q;
            Org.BouncyCastle.Math.EC.ECPoint u1 = ciphertext.U1;
            Org.BouncyCastle.Math.EC.ECPoint u2 = ciphertext.U2;

            #region k

            Org.BouncyCastle.Math.EC.ECPoint hk = u1.Multiply(priv.Z);
            Org.BouncyCastle.Math.EC.ECPoint normhk = hk.Normalize();

            digest.BlockUpdate(normhk.XCoord.ToBigInteger().ToByteArray(), 0, normhk.XCoord.ToBigInteger().ToByteArray().Length);
            digest.BlockUpdate(normhk.YCoord.ToBigInteger().ToByteArray(), 0, normhk.YCoord.ToBigInteger().ToByteArray().Length);
            int outOff = 0;
            byte[] output = new byte[digest.GetDigestSize()];

            digest.DoFinal(output, outOff);

            digest.Reset();
            byte[] e = output;

            #endregion

            #region Verify

            priv.Digest.BlockUpdate(u1.XCoord.ToBigInteger().ToByteArray(), 0, u1.XCoord.ToBigInteger().ToByteArray().Length);
            priv.Digest.BlockUpdate(u1.YCoord.ToBigInteger().ToByteArray(), 0, u1.YCoord.ToBigInteger().ToByteArray().Length);
            priv.Digest.BlockUpdate(u2.XCoord.ToBigInteger().ToByteArray(), 0, u2.XCoord.ToBigInteger().ToByteArray().Length);
            priv.Digest.BlockUpdate(u2.YCoord.ToBigInteger().ToByteArray(), 0, u2.YCoord.ToBigInteger().ToByteArray().Length);
            outOff = 0;
            output = new byte[64];

            priv.Digest.DoFinal(output, outOff);

            BigInteger a = new BigInteger(output).Mod(q);

            Org.BouncyCastle.Math.EC.ECPoint u1x = u1.Multiply(priv.X1);
            Org.BouncyCastle.Math.EC.ECPoint u2x = u2.Multiply(priv.X2);
            Org.BouncyCastle.Math.EC.ECPoint u1y = u1.Multiply(priv.Y1);
            Org.BouncyCastle.Math.EC.ECPoint u2y = u2.Multiply(priv.Y2);
            Org.BouncyCastle.Math.EC.ECPoint test1 = u1x.Add(u2x);
            Org.BouncyCastle.Math.EC.ECPoint test2 = u1y.Add(u2y);
            Org.BouncyCastle.Math.EC.ECPoint test = test2.Multiply(a).Add(test1);

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
