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
using System;
using System.Globalization;

namespace RAPPOR.Helper
{
    /// <summary>
    /// This class contains mathematical helper functions of the component.
    /// </summary>
    public class MathFunctions
    {
        public MathFunctions()
        {

        }
        /// <summary>
        /// This method takes a hexstring and converts it to a long entity
        /// </summary>
        /// <param name="hex">The string to be converted</param>
        /// <returns>The value returned as a hex string</returns>
        public long hexToDec(string hex)
        {
            long l = 0;

            for (int i = 0; i < hex.Length; i++)
            {
                l += (long)(int.Parse(hex[hex.Length - (i + 1)].ToString(), NumberStyles.HexNumber) * Math.Pow(16, i));
            }
            return l;
        }
    }
}
