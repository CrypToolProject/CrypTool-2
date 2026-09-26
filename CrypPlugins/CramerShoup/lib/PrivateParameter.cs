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
            string str = string.Format("X1 : {0}\n", X1);
            str += string.Format("X2 : {0}\n", X2);
            str += string.Format("Y1 : {0}\n", Y1);
            str += string.Format("Y2 : {0}\n", Y2);
            str += string.Format("Z : {0}\n", Z);
            return str;
        }
    }
}
