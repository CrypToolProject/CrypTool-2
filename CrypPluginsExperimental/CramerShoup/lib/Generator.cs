using System;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.EC;

namespace CrypTool.Plugins.CramerShoup.lib
{
    public class ECCramerShoupGenerator
    {
        public ECCramerShoupKeyPair Generator(SecureRandom random,String customcurve)
        {
            var parameter = CustomNamedCurves.GetByName(customcurve);
            var curve = parameter.Curve;
            var p = parameter.N;
            var min = parameter.H;
            var max = p.Subtract(BigInteger.Two);
            var g1i = BigIntegers.CreateRandomInRange(min, max, random);
            var g1 = parameter.G.Multiply(g1i);
            var g2i = BigIntegers.CreateRandomInRange(min, max, random);
            var g2 = g1.Multiply(g2i);
        //    BigInteger p = BigInteger.ValueOf(23), g1 = BigInteger.ValueOf(7), g2 = BigInteger.ValueOf(13);


            BigInteger x1 = BigIntegers.CreateRandomInRange(min, max, random);
            BigInteger x2 = BigIntegers.CreateRandomInRange(min, max, random);
            BigInteger y1 = BigIntegers.CreateRandomInRange(min, max, random);
            BigInteger y2 = BigIntegers.CreateRandomInRange(min, max, random);
            BigInteger z = BigIntegers.CreateRandomInRange(min, max, random);

            var c1 = g1.Multiply(x1);
            var c = g2.Multiply(x2).Add(c1);
            var d1 = g1.Multiply(y1);
            var d = g2.Multiply(y2).Add(d1);
            var h = g1.Multiply(z);

            var digest = new Sha512Digest();
            ECCramerShoupPrivateParameter priv = new ECCramerShoupPrivateParameter { Q = p, G1 = g1, G2 = g2, Digest = digest, X1 = x1, X2 = x2, Y1 = y1, Y2 = y2, Z = z };
            ECCramerShoupPublicParameter publ = new ECCramerShoupPublicParameter { Q = p, G1 = g1, G2 = g2, Digest = digest, H = h, C = c, D = d };

            return new ECCramerShoupKeyPair { Priv = priv, Public = publ };
        }
    }
}
