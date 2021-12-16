/*
This file is part of SharpPcap.

SharpPcap is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

SharpPcap is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with SharpPcap.  If not, see <http://www.gnu.org/licenses/>.
*/
/* 
 * Copyright 2010-2011 Chris Morgan <chmorgan@gmail.com>
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpPcap.AirPcap
{
    /// <summary>
    /// Version
    /// </summary>
    public class AirPcapVersion
    {
        /// <summary>
        /// Returns the version in separate fields
        /// </summary>
        /// <param name="Major"></param>
        /// <param name="Minor"></param>
        /// <param name="Rev"></param>
        /// <param name="Build"></param>
        /// <returns></returns>
        public static void Version(out uint Major, out uint Minor, out uint Rev, out uint Build)
        {

            AirPcapSafeNativeMethods.AirpcapGetVersion(out uint major, out uint minor, out uint rev, out uint build);

            Major = major;
            Minor = minor;
            Rev = rev;
            Build = build;
        }

        /// <summary>
        /// Returns the version in a.b.c.d format
        /// </summary>
        /// <returns></returns>
        public static string VersionString()
        {
            Version(out uint Major, out uint Minor, out uint Rev, out uint Build);

            return string.Format("{0}.{1}.{2}.{3}",
                                 Major,
                                 Minor,
                                 Rev,
                                 Build);
        }
    }
}
