/*
This file is part of PacketDotNet

PacketDotNet is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

PacketDotNet is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with PacketDotNet.  If not, see <http://www.gnu.org/licenses/>.
*/
/*
 *  Copyright 2010 Chris Morgan <chmorgan@gmail.com>
 */

namespace PacketDotNet
{
    /// <summary>
    /// Point to Point Protocol
    /// See http://tools.ietf.org/html/rfc2516
    /// </summary>
    public class PPPoEFields
    {
        /// <summary> Size in bytes of the version/type field </summary>
        public static readonly int VersionTypeLength = 1;

        /// <summary> Size in bytes of the code field </summary>
        public static readonly int CodeLength = 1;

        /// <summary> Size in bytes of the SessionId field </summary>
        public static readonly int SessionIdLength = 2;

        /// <summary> Size in bytes of the Length field </summary>
        public static readonly int LengthLength = 2;

        /// <summary> Offset from the start of the header to the version/type field </summary>
        public static readonly int VersionTypePosition = 0;

        /// <summary> Offset from the start of the header to the Code field </summary>
        public static readonly int CodePosition;

        /// <summary> Offset from the start of the header to the SessionId field </summary>
        public static readonly int SessionIdPosition;

        /// <summary> Offset from the start of the header to the Length field </summary>
        public static readonly int LengthPosition;

        /// <summary>
        /// Length of the overall PPPoe header
        /// </summary>
        public static readonly int HeaderLength;

        static PPPoEFields()
        {
            CodePosition = VersionTypePosition + VersionTypeLength;
            SessionIdPosition = CodePosition + CodeLength;
            LengthPosition = SessionIdPosition + SessionIdLength;

            HeaderLength = LengthPosition + LengthLength;
        }
    }
}

