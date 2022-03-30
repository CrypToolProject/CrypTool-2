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
