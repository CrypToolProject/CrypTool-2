using System;
using System.Text;

namespace CrypTool.CertificateLibrary.Network
{
    public class Packet
    {

        #region Constructor

        /// <summary>
        /// Creates a new network packet.
        /// <para>Mostly used for receiving new messages</para>
        /// </summary>
        public Packet()
            : this(PacketType.Invalid, null)
        {
        }

        /// <summary>
        /// Creates a new network packet of PacketType type.
        /// <para>Mostly used for staccato messages</para>
        /// </summary>
        /// <param name="type">The packet type</param>
        public Packet(PacketType type)
            : this(type, null)
        {
        }

        /// <summary>
        /// Creates a new network packet of PacketType type with the given byte array as data.
        /// <para>Mostly used for real request/response packets</para>
        /// </summary>
        /// <param name="type">The packet type</param>
        /// <param name="data">The payload</param>
        public Packet(PacketType type, byte[] data)
        {
            Version = 0;
            Type = type;
            Data = data ?? new byte[] { 0 };
        }

        #endregion


        #region Properties

        /// <summary>
        /// The type of this packet.
        /// </summary>
        public PacketType Type { get; set; }

        /// <summary>
        /// The payload of this packet.
        /// </summary>
        public byte[] Data { get; set; }

        public byte Version { get; set; }

        #endregion



        #region Object methods

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Packet [ Version: ").Append(BitConverter.ToInt16(new byte[] { Version, 0 }, 0));
            sb.Append(" | Type: ").Append(Type);
            sb.Append(" | DataLength: ").Append((Data != null) ? Data.Length.ToString() : "null");
            sb.Append(" ]");
            return sb.ToString();
        }

        #endregion

    }
}
