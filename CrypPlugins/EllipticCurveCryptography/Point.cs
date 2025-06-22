/*                              
   Copyright 2025 Nils Kopal, CrypTool Project

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

namespace CrypTool.Plugins.EllipticCurveCryptography
{
    public class Point
    {
        public BigInteger X { get; set; }
        public BigInteger Y { get; set; }
        public bool IsInfinity { get; set; } = false;
        public EllipticCurve Curve { get; set; }

        /// <summary>
        /// Returns a string represantion of this point as (x,y) or if it is the infinity point
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (IsInfinity)
            {
                return Properties.Resources.InfinityPoint;
            }
            return string.Format("({0},{1})", X, Y);
        }
    }
}