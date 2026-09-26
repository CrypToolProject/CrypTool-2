/*
   Copyright (C) CrypTool 2 Team

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
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
