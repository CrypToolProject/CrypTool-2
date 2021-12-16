/*
This file is part of SharpPcap

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
 * Copyright 2009-2011 Chris Morgan <chmorgan@gmail.com>
 */

using System;
namespace SharpPcap
{
    /// <summary> POSIX.4 timeval</summary>
    public class PosixTimeval : IComparable<PosixTimeval>
    {
        private static readonly System.DateTime epochDateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        internal long microsecondsPerMillisecond = 1000;

        /// <value>
        /// Number of seconds in the timeval
        /// </value>
        public virtual ulong Seconds
        {
            get;
            set;
        }

        /// <value>
        /// Number of microseconds in the timeval
        /// </value>
        public virtual ulong MicroSeconds
        {
            get;
            set;
        }

        /// <summary> The timeval as a DateTime in Utc </summary>
        public virtual System.DateTime Date => UnixTimeValToDateTime(Seconds, MicroSeconds);

        /// <summary>
        /// Operator &lt; overload
        /// </summary>
        /// <param name="a">
        /// A <see cref="PosixTimeval"/>
        /// </param>
        /// <param name="b">
        /// A <see cref="PosixTimeval"/>
        /// </param>
        /// <returns>
        /// A <see cref="System.Boolean"/>
        /// </returns>
        public static bool operator <(PosixTimeval a, PosixTimeval b)
        {
            if (a.Seconds < b.Seconds)
            {
                return true;
            }

            if ((a.Seconds == b.Seconds) &&
               (a.MicroSeconds < b.MicroSeconds))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Operator &gt; overload
        /// </summary>
        /// <param name="a">
        /// A <see cref="PosixTimeval"/>
        /// </param>
        /// <param name="b">
        /// A <see cref="PosixTimeval"/>
        /// </param>
        /// <returns>
        /// A <see cref="System.Boolean"/>
        /// </returns>
        public static bool operator >(PosixTimeval a, PosixTimeval b)
        {
            return (b < a);
        }

        /// <summary>
        /// Operator &lt;=
        /// </summary>
        /// <param name="a">
        /// A <see cref="PosixTimeval"/>
        /// </param>
        /// <param name="b">
        /// A <see cref="PosixTimeval"/>
        /// </param>
        /// <returns>
        /// A <see cref="System.Boolean"/>
        /// </returns>
        public static bool operator <=(PosixTimeval a, PosixTimeval b)
        {
            if (a < b)
            {
                return true;
            }

            if ((a.Seconds == b.Seconds) &&
               (a.MicroSeconds <= b.MicroSeconds))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Operator &gt;=
        /// </summary>
        /// <param name="a">
        /// A <see cref="PosixTimeval"/>
        /// </param>
        /// <param name="b">
        /// A <see cref="PosixTimeval"/>
        /// </param>
        /// <returns>
        /// A <see cref="System.Boolean"/>
        /// </returns>
        public static bool operator >=(PosixTimeval a, PosixTimeval b)
        {
            return (b <= a);
        }

        /// <summary>
        /// Operator ==
        /// </summary>
        /// <param name="a">
        /// A <see cref="PosixTimeval"/>
        /// </param>
        /// <param name="b">
        /// A <see cref="PosixTimeval"/>
        /// </param>
        /// <returns>
        /// A <see cref="System.Boolean"/>
        /// </returns>
        public static bool operator ==(PosixTimeval a, PosixTimeval b)
        {
            // Object.Equals() checks for null and then calls a.Equals(b)
            return Object.Equals(a, b);
        }

        /// <summary>
        /// Operator !=
        /// </summary>
        /// <param name="a">
        /// A <see cref="PosixTimeval"/>
        /// </param>
        /// <param name="b">
        /// A <see cref="PosixTimeval"/>
        /// </param>
        /// <returns>
        /// A <see cref="System.Boolean"/>
        /// </returns>
        public static bool operator !=(PosixTimeval a, PosixTimeval b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Equals override
        /// </summary>
        /// <param name="obj">
        /// A <see cref="System.Object"/>
        /// </param>
        /// <returns>
        /// A <see cref="System.Boolean"/>
        /// </returns>
        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            PosixTimeval pt = (PosixTimeval)obj;

            return ((Seconds == pt.Seconds) && (MicroSeconds == pt.MicroSeconds));
        }

        /// <summary>
        /// GetHashCode override
        /// </summary>
        /// <returns>
        /// A <see cref="System.Int32"/>
        /// </returns>
        public override int GetHashCode()
        {
            return Seconds.GetHashCode() + MicroSeconds.GetHashCode();
        }

        private static void DateTimeToUnixTimeVal(DateTime dateTime,
                                                  out ulong tvSec,
                                                  out ulong tvUsec)
        {
            // diff this with the dateTime value
            // NOTE: make sure the time is in universal time when performing
            //       the subtraction so we get the difference between epoch in utc
            //       which is the definition of the unix timeval
            TimeSpan timeSpan = dateTime.ToUniversalTime().Subtract(epochDateTime);

            tvSec = (ulong)(timeSpan.TotalMilliseconds / 1000.0);
            // find the milliseconds remainder and convert to microseconds
            tvUsec = (ulong)((timeSpan.TotalMilliseconds - (tvSec * 1000)) * 1000);
        }

        private static DateTime UnixTimeValToDateTime(ulong tvSec, ulong tvUsec)
        {
            ulong ticks = (tvUsec * (TimeSpan.TicksPerMillisecond / 1000)) +
                         (tvSec * TimeSpan.TicksPerSecond);
            return epochDateTime.AddTicks((long)ticks);
        }

        /// <summary>
        /// Constructor with Seconds and MicroSeconds fields
        /// </summary>
        /// <param name="Seconds">
        /// A <see cref="System.UInt64"/>
        /// </param>
        /// <param name="MicroSeconds">
        /// A <see cref="System.UInt64"/>
        /// </param>
        public PosixTimeval(ulong Seconds, ulong MicroSeconds)
        {
            this.Seconds = Seconds;
            this.MicroSeconds = MicroSeconds;
        }

        /// <summary>
        /// Construct a PosixTimeval using the current UTC time
        /// </summary>
        public PosixTimeval()
        {

            DateTimeToUnixTimeVal(DateTime.UtcNow,
                                  out ulong seconds,
                                  out ulong microseconds);

            Seconds = seconds;
            MicroSeconds = microseconds;
        }

        /// <summary>
        /// Convert the timeval to a string like 'SECONDS.MICROSECONDSs'
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/>
        /// </returns>
        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(Seconds);
            sb.Append('.');
            sb.Append(MicroSeconds);
            sb.Append('s');

            return sb.ToString();
        }

        /// <summary>
        /// Compare this to another
        /// </summary>
        /// <param name="that">
        /// A <see cref="PosixTimeval"/>
        /// </param>
        /// <returns>
        /// A <see cref="System.Int32"/>
        /// </returns>
        public int CompareTo(PosixTimeval that)
        {
            if (this < that)
            {
                return -1;
            }
            else if (this > that)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}