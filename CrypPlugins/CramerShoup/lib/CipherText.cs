using Org.BouncyCastle.Math.EC;

namespace CrypTool.Plugins.CramerShoup.lib
{
    public class ECCramerShoupCipherText
    {
        public ECPoint U1 { get; set; }// kG1
        public ECPoint U2 { get; set; }// kG2
        //public byte[] E { get; set; }// H(kH) xor m

        //Verify
        public ECPoint V { get; set; }// kC+kaD

        public override string ToString()
        {
            string str = string.Format("U1x : {0}\n", U1.XCoord.ToBigInteger());
            str += string.Format("U1y : {0}\n", U1.YCoord.ToBigInteger());
            str += string.Format("U2x : {0}\n", U2.XCoord.ToBigInteger());
            str += string.Format("U2y : {0}\n", U2.YCoord.ToBigInteger());
            str += string.Format("Vx : {0}\n", V.XCoord.ToBigInteger());
            str += string.Format("Vy : {0}\n", V.YCoord.ToBigInteger());
            return str;
        }
    }
}
