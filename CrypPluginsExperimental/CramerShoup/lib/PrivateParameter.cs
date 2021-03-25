using System;
using Org.BouncyCastle.Math;

namespace CrypTool.Plugins.CramerShoup.lib
{
    public class ECCramerShoupPrivateParameter : ECCramerShoupParameter
    {
        public BigInteger X1 { get; set; }
        public BigInteger X2 { get; set; }
        public BigInteger Y1 { get; set; }
        public BigInteger Y2 { get; set; }
        public BigInteger Z { get; set; }

        public override string ToString()
        {
            var str = String.Format("X1 : {0}\n", X1);
            str += String.Format("X2 : {0}\n", X2);
            str += String.Format("Y1 : {0}\n", Y1);
            str += String.Format("Y2 : {0}\n", Y2);
            str += String.Format("Z : {0}\n", Z);
            return str;
        }
    }
}
