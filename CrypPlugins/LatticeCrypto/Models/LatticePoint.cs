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
using System.Numerics;
using System.Windows.Shapes;

namespace LatticeCrypto.Models
{
    public class LatticePoint
    {
        public BigInteger logicX;
        public BigInteger logicY;
        public Ellipse ellipse;

        public LatticePoint(BigInteger logicX, BigInteger logicY)
        {
            this.logicX = logicX;
            this.logicY = logicY;
        }
    }
}
