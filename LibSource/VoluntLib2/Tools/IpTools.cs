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
using System.Net;

namespace VoluntLib2.Tools
{
    public class IpTools
    {
        /// <summary>
        /// Checks if a given IP adress is inside one of the private ip address ranges
        /// </summary>
        /// <param name="ipaddress"></param>
        /// <returns></returns>
        public static bool IsPrivateIP(IPAddress ipaddress)
        {
            byte[] bytes = ipaddress.GetAddressBytes();
            bytes = new byte[] { bytes[3], bytes[2], bytes[1], bytes[0] }; //reverse the ip for calculation
            uint ipNumber = BitConverter.ToUInt32(bytes, 0);
            foreach (uint[] range in Constants.IPTOOLS_PRIVATE_IP_RANGES)
            {
                if (ipNumber >= range[0] && ipNumber <= range[1])
                {
                    return true;
                }
            }
            return false;
        }
    }
}
