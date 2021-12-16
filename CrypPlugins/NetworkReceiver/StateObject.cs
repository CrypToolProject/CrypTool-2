using NetworkSender;
using System.Collections.Generic;

namespace NetworkReceiver
{
    internal class StateObject
    {
        public const int BufferSize = 2048;
        private readonly byte[] buffer = new byte[BufferSize];
        private readonly List<byte> receivedData = new List<byte>();

        public TCPConnection Connection { get; set; }
        public byte[] Buffer => buffer;

        public void AddToReceivedData(byte[] newData)
        {
            receivedData.AddRange(newData);
        }

        public byte[] GetReceivedData()
        {
            return receivedData.ToArray();
        }
    }
}
