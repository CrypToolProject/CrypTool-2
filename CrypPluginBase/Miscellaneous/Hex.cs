/*
   Copyright 2008 - 2022 CrypTool Team

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
using System.Text;

namespace CrypTool.PluginBase.Miscellaneous
{
    public class Hex
    {
        public static string HexToString(byte[] buf)
        {
            return HexToString(buf, 0, buf.Length);
        }

        public static string HexToString(byte[] buf, int offset, int length)
        {
            if (offset + length > buf.Length)
            {
                throw new ArgumentException(string.Format("offset+length ({0}) must be < buf.Length ({1})", (offset + length), buf.Length));
            }

            if (length < 1)
            {
                throw new ArgumentOutOfRangeException("length", length, "must be >= 1");
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", offset, "must be >= 0");
            }

            StringBuilder sb = new StringBuilder(length * 2);
            for (int i = offset; i < (offset + length - 1); i++)
            {
                sb.AppendFormat("{0:X2} ", buf[i]);
            }

            sb.AppendFormat("{0:X2}", buf[offset + length - 1]);
            return sb.ToString();
        }
    }
}
