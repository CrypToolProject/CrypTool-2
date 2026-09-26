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
