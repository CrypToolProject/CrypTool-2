using System;
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
            var str = String.Format("U1x : {0}\n", U1.XCoord.ToBigInteger());
            str += String.Format("U1y : {0}\n", U1.YCoord.ToBigInteger());
            str += String.Format("U2x : {0}\n", U2.XCoord.ToBigInteger());
            str += String.Format("U2y : {0}\n", U2.YCoord.ToBigInteger());
            str += String.Format("Vx : {0}\n", V.XCoord.ToBigInteger());
            str += String.Format("Vy : {0}\n", V.YCoord.ToBigInteger());
            return str;
        }
    }
}
