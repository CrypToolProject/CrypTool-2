using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;

namespace CrypTool.Plugins.CramerShoup.lib
{
    public abstract class ECCramerShoupParameter
    {
        public LongDigest Digest { get; set; }
        public ECCurve E { get; set; }
        public BigInteger Q { get; set; }
        public ECPoint G1 { get; set; }
        public ECPoint G2 { get; set; }
    }
}
