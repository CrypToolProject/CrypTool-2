﻿/*
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
    /// See http://www.ucertify.com/article/ieee-802-11-frame-format.html
    ///
    /// NOTE: All positions are not defined here because the frame type changes
    /// whether some address fields are present or not, causing the sequence control
    /// field to move. In addition the payload size determines where the frame control
    /// sequence value is as it is after the payload bytes, if any payload is present
    /// </summary>
    internal class Ieee80211MacFields
    {
        public static readonly int FrameControlLength = 2;
        public static readonly int DurationIDLength = 2;
        public static readonly int AddressLength = EthernetFields.MacAddressLength;
        public static readonly int SequenceControlLength = 4;
        public static readonly int FrameCheckSequenceLength = 4;

        public static readonly int FrameControlPosition = 0;
        public static readonly int DurationIDPosition;
        public static readonly int Address1Position;

        static Ieee80211MacFields()
        {
            DurationIDPosition = FrameControlPosition + FrameControlLength;
            Address1Position = DurationIDPosition + DurationIDLength;
        }
    }
}
