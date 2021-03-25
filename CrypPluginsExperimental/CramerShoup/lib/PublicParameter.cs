using System;
using Org.BouncyCastle.Math.EC;

namespace CrypTool.Plugins.CramerShoup.lib
{
    public class ECCramerShoupPublicParameter : ECCramerShoupParameter
    {
        public ECPoint C { get; set; }
        public ECPoint D { get; set; }
        public ECPoint H { get; set; }

        public override string ToString()
        {
            var str = String.Format("Cx : {0}\n", C.XCoord.ToBigInteger());
            str += String.Format("Cy : {0}\n", C.YCoord.ToBigInteger());
            str += String.Format("Dx : {0}\n", D.XCoord.ToBigInteger());
            str += String.Format("Dy : {0}\n", D.YCoord.ToBigInteger());
            str += String.Format("Hx : {0}\n", H.XCoord.ToBigInteger());
            str += String.Format("Hy : {0}\n", H.YCoord.ToBigInteger());
            return str;
        }
    }
}
