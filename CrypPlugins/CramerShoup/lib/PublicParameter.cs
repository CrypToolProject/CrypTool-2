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
