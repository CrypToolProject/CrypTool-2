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
            string str = string.Format("Cx : {0}\n", C.XCoord.ToBigInteger());
            str += string.Format("Cy : {0}\n", C.YCoord.ToBigInteger());
            str += string.Format("Dx : {0}\n", D.XCoord.ToBigInteger());
            str += string.Format("Dy : {0}\n", D.YCoord.ToBigInteger());
            str += string.Format("Hx : {0}\n", H.XCoord.ToBigInteger());
            str += string.Format("Hy : {0}\n", H.YCoord.ToBigInteger());
            return str;
        }
    }
}
