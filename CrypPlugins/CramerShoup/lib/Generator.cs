using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.EC;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;

namespace CrypTool.Plugins.CramerShoup.lib
{
    public class ECCramerShoupGenerator
    {
        public ECCramerShoupKeyPair Generator(SecureRandom random, string customcurve)
        {
            Org.BouncyCastle.Asn1.X9.X9ECParameters parameter = CustomNamedCurves.GetByName(customcurve);
            Org.BouncyCastle.Math.EC.ECCurve curve = parameter.Curve;
            BigInteger p = parameter.N;
            BigInteger min = parameter.H;
            BigInteger max = p.Subtract(BigInteger.Two);
            BigInteger g1i = BigIntegers.CreateRandomInRange(min, max, random);
            Org.BouncyCastle.Math.EC.ECPoint g1 = parameter.G.Multiply(g1i);
            BigInteger g2i = BigIntegers.CreateRandomInRange(min, max, random);
            Org.BouncyCastle.Math.EC.ECPoint g2 = g1.Multiply(g2i);
            //    BigInteger p = BigInteger.ValueOf(23), g1 = BigInteger.ValueOf(7), g2 = BigInteger.ValueOf(13);


            BigInteger x1 = BigIntegers.CreateRandomInRange(min, max, random);
            BigInteger x2 = BigIntegers.CreateRandomInRange(min, max, random);
            BigInteger y1 = BigIntegers.CreateRandomInRange(min, max, random);
            BigInteger y2 = BigIntegers.CreateRandomInRange(min, max, random);
            BigInteger z = BigIntegers.CreateRandomInRange(min, max, random);

            Org.BouncyCastle.Math.EC.ECPoint c1 = g1.Multiply(x1);
            Org.BouncyCastle.Math.EC.ECPoint c = g2.Multiply(x2).Add(c1);
            Org.BouncyCastle.Math.EC.ECPoint d1 = g1.Multiply(y1);
            Org.BouncyCastle.Math.EC.ECPoint d = g2.Multiply(y2).Add(d1);
            Org.BouncyCastle.Math.EC.ECPoint h = g1.Multiply(z);

            Sha512Digest digest = new Sha512Digest();
            ECCramerShoupPrivateParameter priv = new ECCramerShoupPrivateParameter { Q = p, G1 = g1, G2 = g2, Digest = digest, X1 = x1, X2 = x2, Y1 = y1, Y2 = y2, Z = z };
            ECCramerShoupPublicParameter publ = new ECCramerShoupPublicParameter { Q = p, G1 = g1, G2 = g2, Digest = digest, H = h, C = c, D = d };

            return new ECCramerShoupKeyPair { Priv = priv, Public = publ };
        }
    }
}
