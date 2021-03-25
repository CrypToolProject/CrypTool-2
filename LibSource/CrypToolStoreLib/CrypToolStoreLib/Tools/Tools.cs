/*
   Copyright 2018 Nils Kopal <Nils.Kopal<AT>Uni-Kassel.de>

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

namespace CrypToolStoreLib.Tools
{
    public class Tools
    {
        /// <summary>
        /// Converts a given byte array to a hex string
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ByteArrayToHexString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        /// <summary>
        /// Converts a string containing hex values to a byte array
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] HexStringToByteArray(string hex)
        {
            int length = hex.Length;
            byte[] bytes = new byte[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        /// <summary>
        /// Returns a formatted time left string
        /// Shows remaning hours, minutes, and seconds
        /// </summary>
        /// <param name="bytepersec"></param>
        /// <param name="totalbytes"></param>
        /// <param name="downloadedbytes"></param>
        /// <returns></returns>
        public static string RemainingTime(long bytepersec, long totalbytes, long downloadedbytes)
        {
            if (bytepersec <= 0)
            {
                return "-";
            }
            long remainingSeconds = (totalbytes - downloadedbytes) / bytepersec;
            string formatted = "";
            if (remainingSeconds > (60 * 60))//hours
            {
                long hours = remainingSeconds / (60 * 60);
                formatted += hours + " h ";
                remainingSeconds = remainingSeconds % (60 * 60);
            }
            if (remainingSeconds > 60) //minutes
            {
                long minutes = remainingSeconds / 60;
                formatted += minutes + " min ";
                remainingSeconds = remainingSeconds % 60;
            }
            formatted += remainingSeconds + " sec";
            return formatted;
        }

        /// <summary>
        /// Returns a formatted speed string based on byte/sec
        /// Shows speed in GB/sec, MB/sec, KB/sec, and byte/sec
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string FormatSpeedString(long bytes)
        {
            if (bytes > (1024 * 1024 * 1024)) //GiB / sec
            {
                return Math.Round(bytes / (1024 * 1024 * 1024.0), 2) + " GB/sec";
            }
            if (bytes > (1024 * 1024))
            {
                return Math.Round(bytes / (1024 * 1024.0), 2) + " MB/sec";
            }
            if (bytes > 1024)
            {
                return Math.Round(bytes / 1024.0, 2) + " KB/sec";
            }
            return bytes + " byte/sec";
        }

        /// <summary>
        /// Returns a formatted string for a file size based on bytes
        /// Shows GB, MB, KB, and byte
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string FormatFileSizeString(long bytes)
        {
            if (bytes > (1024 * 1024 * 1024)) //GiB 
            {
                return Math.Round(bytes / (1024 * 1024 * 1024.0), 2) + " GB";
            }
            if (bytes > (1024 * 1024))
            {
                return Math.Round(bytes / (1024 * 1024.0), 2) + " MB";
            }
            if (bytes > 1024)
            {
                return Math.Round(bytes / 1024.0, 2) + " KB";
            }
            return bytes + " byte";
        }
    }
}
