﻿/*This file is part of SharpPcap.SharpPcap is free software: you can redistribute it and/or modifyit under the terms of the GNU Lesser General Public License as published bythe Free Software Foundation, either version 3 of the License, or(at your option) any later version.SharpPcap is distributed in the hope that it will be useful,but WITHOUT ANY WARRANTY; without even the implied warranty ofMERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See theGNU Lesser General Public License for more details.You should have received a copy of the GNU Lesser General Public Licensealong with SharpPcap.  If not, see <http://www.gnu.org/licenses/>.*/
/*  * Copyright 2010-2011 Chris Morgan <chmorgan@gmail.com> */



































using System;namespace SharpPcap.AirPcap{












    /// <summary>    /// Mac flags    /// </summary>    [Flags]    public enum AirPcapMacFlags : int    {






























        ///<summary>        /// If set, the device is configured to work in monitor mode.        /// When monitor mode is on, the device captures all the frames transmitted on the channel. This includes:        ///    - unicast packets        ///    - multicast packets        ///    - broadcast packets        ///    - control and management packets        ///        /// When monitor mode is off, the device has a filter on unicast packets to capture only the packets whose MAC        /// destination address equals the device's address. This means the following frames will be received:        ///   - unicast packets whose destination is the address of the device        ///   - multicast packets        ///   - broadcast packets        ///   - beacons and probe requests        /// </summary>        MonitorModeOn = 1,









        ///<summary>        /// If set, the device will acknowledge the data frames sent to its address. This is useful when the device needs to interact with other devices on the         /// 802.11 network, bacause handling the ACKs in software is normally too slow.        ///</summary>        AckFramesOn = 2,    };}